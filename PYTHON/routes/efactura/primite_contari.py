# routes/efactura/primite_contari.py
"""
01EF-02: the CALCULATION of the «Contari» of received e-invoices (the accounting half of the Access form EFACTURA_2025 with its subforms EF_F and EF_P).
NOTHING IS WRITTEN HERE. The operator ticks invoices of ONE supplier, the server answers what Access kept in its work tables tmpFacturi / tmpDoc2017P:
the evidence rows, the payment row, the templates that fit the classification, the codes of the partner and the warnings; and, from the chosen
templates, the debit / credit lines each booking would produce. The save (documents, operations, debit, credit, notes) is 01EF-04; the payment and its order (OP) are 04EF-01 (primite_plata.py).

Rules taken from Access (verified in EFACTURA_2025 / EF_F / EF_P / QEF_EV / QEF_Plata / SAV_*), with the FOREXE part of Access left out on purpose
(operator rule 31.10.2026) and without any link to receptions (the «Contari» has nothing to do with them):
- Suma of an invoice = its total (negative for a credit note, `Semn`) minus what EF_PrimiteOperatii already holds; Platit <= Suma, by default = Suma.
- NumarDocument = the digits of the LAST word of the invoice number (the whole number when it has no digit).   [reading of QEF_EV: inferred]
- Only templates of `Tip` M; evidence = `Fel` E, payment = `Fel` P. A template fits a classification when its `Cheie` equals Clasificatii.ClsfX and the
  sector+source of its journal equals Clasificatii.SS; its lines are those of the form (Subcapitol) of the classification, or without a form.
- Explanation of the order of payment: «F <numbers of the invoices paid> <FelOP of the template>». (Access added «C <client code>» from ParteneriAng.CodClient,
  a column K-BOT does not have.)
The role only decides `poate_scrie`; reading is open to every session.
"""
import logging
import re
from contextlib import contextmanager
from datetime import date
from decimal import Decimal

from . import facturi as F
from . import primite_ke
from .primite_parteneri import activeaza as activeaza_partener, leaga as leaga_partener, rezolva as rezolva_partener
from .tokens import EfEroare

logger = logging.getLogger(__name__)

CONTARI_SQL = "sql/00EF_27_primite_operatii.sql, sql/AV01_14_sablon_operatii.sql, sql/00EF_55_furnizor_partener.sql"
MAX_FACTURI = 200
MODURI = ("evidentiere", "plata", "ambele")
MIN_EXPLICATIE_OP = 1
MAX_EXPLICATIE_OP = 75   # the official limit of the explanation of an order of payment

_dec = primite_ke._dec
_num = primite_ke._num

_GEMENI = {"–": "-", "—": "-", "‘": "'", "’": "'", "‚": "'", "“": '"', "”": '"', "„": '"',
           "«": '"', "»": '"', " ": " "}


def text_op(text, limita=None):
    """Text for an order of payment: ASCII ONLY (ANAF refuses diacritics) and NO comma (the bar code of the order reads it as «next field»).
    Diacritics become the plain letter, a comma a space, other non-ASCII characters are dropped; single spaces; at most `limita` characters."""
    import unicodedata
    out = []
    for ch in unicodedata.normalize("NFD", text or ""):
        if unicodedata.category(ch) == "Mn":
            continue
        ch = _GEMENI.get(ch, ch)
        if ch in ",\t\r\n":
            ch = " "
        if " " <= ch < "\x7f":
            out.append(ch)
    curat = " ".join("".join(out).split())
    return curat[:limita].rstrip() if limita else curat


def numar_document(nr_fact):
    """The digits of the last word of the invoice number; the whole number when there is no digit (reading of QEF_EV)."""
    words = re.findall(r"\w+", nr_fact or "")
    digits = re.sub(r"\D+", "", words[-1]) if words else ""
    return digits or (nr_fact or "")


def _iso(value):
    return value.isoformat() if value is not None else None


def _ids(raw):
    if not isinstance(raw, list) or not raw or len(raw) > MAX_FACTURI:
        raise EfEroare("Alegeți între 1 și %d facturi." % MAX_FACTURI, "FACTURI_INVALIDE", 400)
    out = []
    for item in raw:
        if isinstance(item, bool) or not isinstance(item, int) or item <= 0:
            raise EfEroare("Lista facturilor conține o valoare nepermisă.", "FACTURI_INVALIDE", 400)
        if item not in out:
            out.append(item)
    return out


def _opt_int(body, key):
    value = body.get(key)
    if value is None:
        return None
    if isinstance(value, bool) or not isinstance(value, int) or value <= 0:
        raise EfEroare("Câmpul «%s» nu este valid." % key, "CAMP_INVALID", 400)
    return value


def _marks(n):
    return ", ".join(["%s"] * n)


# ---------------------------------------------------------------------------------------------
# reading
# ---------------------------------------------------------------------------------------------
def _facturi(cursor, ids, blocheaza=False):
    cursor.execute("SELECT IdPrimita, NrFact, DataFact, Total, Semn, Tip, CuiNormalizat, CUI, DenumireP FROM EF_Primite "
                   "WHERE IdPrimita IN (%s) ORDER BY DataFact, IdPrimita%s" % (_marks(len(ids)), " FOR UPDATE" if blocheaza else ""), tuple(ids))
    rows = cursor.fetchall()
    if len(rows) != len(ids):
        raise EfEroare("Una dintre facturi nu mai există.", "FACTURA_INEXISTENTA", 404)
    suppliers = {(r["CuiNormalizat"] or "").strip() or ("N" + (r["DenumireP"] or "").strip().upper()) for r in rows}
    if len(suppliers) > 1:
        raise EfEroare("Facturile alese trebuie să fie de la același furnizor.", "FURNIZORI_DIFERITI", 409)
    return rows


def _salvat(cursor, ids):
    cursor.execute("SELECT IdPrimita, SUM(Suma) AS Salvat FROM EF_PrimiteOperatii WHERE IdPrimita IN (%s) GROUP BY IdPrimita" % _marks(len(ids)), tuple(ids))
    return {r["IdPrimita"]: _dec(r["Salvat"]) for r in cursor.fetchall()}


def _sabloane(cursor, id_subunitate=None):
    """{(Cheie, SS): {'E': [template], 'P': [template]}} for the templates of type M; with `id_subunitate`, only the templates of that subunit (as in Access, nothing is common)."""
    cursor.execute("SELECT s.IdSablon, s.Fel, s.Explicatie, s.FelOP, s.Cheie, s.Compartiment, j.Cod AS Jurnal, j.SursaSector "
                   "FROM TZ_SablonOperatii s JOIN TZ_Jurnale j ON j.IdJurnal = s.IdJurnal "
                   "WHERE s.Tip = 'M' AND s.Fel IN ('E', 'P') AND s.Cheie IS NOT NULL AND (%s IS NULL OR s.IdSubunitate = %s) ORDER BY s.Explicatie, s.IdSablon",
                   (id_subunitate, id_subunitate))
    index = {}
    for r in cursor.fetchall():
        slot = index.setdefault((r["Cheie"], r["SursaSector"]), {"E": [], "P": []})
        slot[r["Fel"]].append({"IdSablon": r["IdSablon"], "Explicatie": r["Explicatie"] or "", "FelOP": r["FelOP"] or "",
                               "Compartiment": r["Compartiment"] or "", "Jurnal": r["Jurnal"]})
    return index


def _clasificatii(cursor, index):
    """The classifications a template fits: [{IdClsf, Clsf, Denumire, Forma, Cheie, SS}] in code order."""
    cursor.execute("SELECT IDClsf, Clsf, ClsfX, ClsfF, ClsfE, Subcapitol, Denumire, SS, Titlu, IdUnitate FROM Clasificatii ORDER BY Clsf")
    return [{"IdClsf": r["IDClsf"], "Clsf": r["Clsf"], "Denumire": r["Denumire"] or "", "Forma": r["Subcapitol"], "Cheie": r["ClsfX"], "SS": r["SS"],
             "Titlu": r["Titlu"], "ClsfF": r["ClsfF"], "ClsfE": r["ClsfE"], "IdUnitate": r["IdUnitate"]} for r in cursor.fetchall() if (r["ClsfX"], r["SS"]) in index]


def _coduri(cursor, id_partener, id_clsf):
    if id_partener is None or id_clsf is None:
        return None
    cursor.execute("SELECT CodAng, CodInd, ContBancar FROM Parteneri_Coduri WHERE IdPartener = %s AND IdClsf = %s", (id_partener, id_clsf))
    row = cursor.fetchone()
    if row is None:
        return None
    return {"CodAng": row["CodAng"] or "", "CodInd": row["CodInd"] or "", "ContBancar": row["ContBancar"] or ""}


def _sugestie(cursor, partener, oferite):
    """(suggested IdClsf or None, [IdClsf of the partner offered]). The partner's classifications first; otherwise the only one of article 20 that has a template."""
    din_partener = []
    if partener is not None:
        cursor.execute("SELECT IdClsf FROM Parteneri_Coduri WHERE IdPartener = %s", (partener["IdPartener"],))
        ids = {r["IdClsf"] for r in cursor.fetchall()}
        din_partener = [c["IdClsf"] for c in oferite if c["IdClsf"] in ids]
    if len(din_partener) == 1:
        return din_partener[0], din_partener
    if din_partener:
        return None, din_partener
    art20 = [c["IdClsf"] for c in oferite if c["Titlu"] == "20"]
    return (art20[0] if len(art20) == 1 else None), din_partener


# ---------------------------------------------------------------------------------------------
# the lines each booking would produce
# ---------------------------------------------------------------------------------------------
def _parti(cursor, linii):
    ids = {l["IdPartDebit"] for l in linii if l["IdPartDebit"]} | {l["IdPartCredit"] for l in linii if l["IdPartCredit"]}
    if not ids:
        return {}
    cursor.execute("SELECT IdPartener, DenumirePartener FROM Parteneri WHERE IdPartener IN (%s)" % _marks(len(ids)), tuple(ids))
    return {r["IdPartener"]: r["DenumirePartener"] or "" for r in cursor.fetchall()}


def _linii_sablon(cursor, id_sablon, forma):
    cursor.execute("SELECT IdLinie, Forma, ContDebit, AnaliticDebit, ContCredit, AnaliticCredit, PartDebitMod, IdPartDebit, PartCreditMod, IdPartCredit, "
                   "PA, Ordonantare, Chelt FROM TZ_SablonOperatiiLinie WHERE IdSablon = %s AND (Forma = %s OR Forma IS NULL OR Forma = '') ORDER BY IdLinie",
                   (id_sablon, forma))
    rows = cursor.fetchall()
    exacte = [r for r in rows if r["Forma"] == forma]   # a template has one line per form: the line of the form wins over the line valid for every form
    return exacte or rows[:1]


def _simbol(cont, analitic):
    return "%s.%s" % (cont, analitic) if analitic else str(cont)


def _partener_text(mod, id_fix, partener, nume):
    if mod == "OPERATIE":
        return partener["Denumire"] if partener else "(furnizorul)"
    if mod == "FIX":
        return nume.get(id_fix, "")
    return ""


def _operatie(titlu, linii, suma, partener, nume):
    """One booking: the template lines with the amount (Access wrote the same amount on the debit and on the credit)."""
    return {"Titlu": titlu, "Suma": _num(suma), "Linii": [{
        "Debit": _simbol(l["ContDebit"], l["AnaliticDebit"]), "PartenerDebit": _partener_text(l["PartDebitMod"], l["IdPartDebit"], partener, nume),
        "Credit": _simbol(l["ContCredit"], l["AnaliticCredit"]), "PartenerCredit": _partener_text(l["PartCreditMod"], l["IdPartCredit"], partener, nume),
        "PA": bool(l["PA"]), "Ordonantare": bool(l["Ordonantare"]), "Chelt": bool(l["Chelt"])} for l in linii]}


# ---------------------------------------------------------------------------------------------
# the calculation
# ---------------------------------------------------------------------------------------------
@contextmanager
def _conexiune(dc, conn=None):
    """The caller's own connection when one is given (the save works in ONE transaction), else a new one of the unit."""
    if conn is not None:
        yield conn
    else:
        with F._unit(dc, CONTARI_SQL) as nou:
            yield nou


def calcul(dc, body, role, conn=None, interne=None, blocheaza=False, fara_plata=False):
    """Everything the page «Contari» shows for the ticked invoices. body: {ids, idClsf?, idSablonE?, idSablonP?, platit?: {id: value}, an?, idSubunitate?, idPartener?}.
    The save (01EF-04) calls it with its own connection (`conn`), `blocheaza` (the invoices are locked), `fara_plata` (no payment template, no payment warnings) and an
    `interne` dict that receives the objects it needs (invoices, exact rests, partner, classification, subunit, dates)."""
    ids = _ids(body.get("ids"))
    mod = body.get("mod") or "evidentiere"   # 04EF-01: evidentiere | plata | ambele
    if mod not in MODURI:
        raise EfEroare("Câmpul «mod» nu este valid.", "CAMP_INVALID", 400)
    protectie = bool(body.get("protectie"))   # the tick «Protectie contari Plati/Receptii» of the operator (Setari > Aplicatie)
    id_clsf = _opt_int(body, "idClsf")
    id_e = _opt_int(body, "idSablonE")
    id_p = None if (fara_plata or mod == "evidentiere") else _opt_int(body, "idSablonP")
    an = _opt_int(body, "an")
    platit_cerut = body.get("platit") or {}
    if not isinstance(platit_cerut, dict):
        raise EfEroare("Câmpul «platit» nu este valid.", "CAMP_INVALID", 400)
    with _conexiune(dc, conn) as conn:
        cursor = conn.cursor(dictionary=True)
        facturi = _facturi(cursor, ids, blocheaza)
        salvat = _salvat(cursor, ids)
        from routes.avacont.introducere_date.operatii import plati
        rest_plata = plati.rest_de_plata(cursor, ids) if mod != "evidentiere" else {}
        cui = (facturi[0]["CuiNormalizat"] or "").strip()
        # The partner is found by TAX CODE ONLY (hidden ones too): the one named in the body (`idPartener`), else the one kept for the code, else the only one;
        # several and none kept: None and the operator chooses.
        candidati, partener = rezolva_partener(cursor, cui, _opt_int(body, "idPartener"))
        # The subunit in force (chosen in the title bar; the booking belongs to it): only ITS templates and the classifications of ITS sector+source are offered.
        subunitate = None
        id_subunitate = _opt_int(body, "idSubunitate")
        index = _sabloane(cursor, id_subunitate)
        oferite = _clasificatii(cursor, index)
        if id_subunitate is not None:
            from routes.avacont.introducere_date.operatii import postare
            try:
                subunitate = postare.subunitate_pentru(cursor, None, id_subunitate)
            except postare.PostareEroare as err:
                raise EfEroare(str(err), err.reason, err.status) from err
            oferite = [c for c in oferite if c["SS"] == subunitate["SursaSector"]]
        sugerata, ale_partenerului = _sugestie(cursor, partener, oferite)
        avertismente = []
        if not candidati:
            avertismente.append({"Cod": "PARTENER_LIPSA", "Grav": "eroare",
                                 "Text": "Nu există niciun partener cu codul fiscal al furnizorului. Adăugați-l întâi în lista de parteneri."})
        elif partener is None:
            avertismente.append({"Cod": "PARTENER_DE_ALES", "Grav": "eroare",
                                 "Text": "Mai mulți parteneri au codul fiscal al furnizorului. Alegeți unul."})
        clsf = next((c for c in oferite if c["IdClsf"] == id_clsf), None) if id_clsf else None
        if id_clsf and clsf is None:
            raise EfEroare("Clasificația aleasă nu are șabloane de operații în sursa subunității în vigoare.", "CLASIFICATIE_FARA_SABLON", 409)

        # the invoices (EF_F)
        evidenta, total_platit, numere = [], Decimal(0), []
        ramas_exacte = {}
        for r in facturi:
            semn = int(r["Semn"] or 1)
            total = _dec(r["Total"]) * semn
            ramas = total - salvat.get(r["IdPrimita"], Decimal(0))
            ramas_exacte[r["IdPrimita"]] = ramas
            cerut = platit_cerut.get(str(r["IdPrimita"]))
            if mod == "evidentiere":
                limita = ramas   # old behaviour: the amount is bounded by what is left to evidence
            else:
                # a payment is bounded by what is evidenced and not paid yet; with the combined save also by what this save evidences
                limita = rest_plata[r["IdPrimita"]] + (ramas if mod == "ambele" else Decimal(0))
            platit = _dec(cerut) if cerut is not None else limita
            if (limita >= 0 and (platit < 0 or platit > limita)) or (limita < 0 and (platit > 0 or platit < limita)):
                raise EfEroare("Suma plătită a facturii %s nu poate depăși %s (%s)." % (r["NrFact"], "valoarea rămasă de înregistrat" if mod == "evidentiere" else "restul de plată", _num(limita)),
                               "PLATA_PESTE_RAMAS" if mod == "evidentiere" else "PLATA_PESTE_REST", 400)
            if mod == "evidentiere" and ramas == 0:
                avertismente.append({"Cod": "FACTURA_COMPLETA", "Grav": "eroare", "Text": "Factura %s este deja înregistrată integral." % r["NrFact"]})
            if mod != "evidentiere" and limita == 0:
                avertismente.append({"Cod": "FACTURA_ACHITATA", "Grav": "eroare", "Text": "Factura %s nu mai are rest de plată." % r["NrFact"]})
            if mod != "evidentiere" and platit != 0 and not plati.are_receptie(cursor, r["IdPrimita"]):
                # 04EF-01: the factura has no reception. Tick OFF (2026 default): a warning only; tick ON: the payment is refused (the evidence stays possible)
                avertismente.append({"Cod": "RECEPTIE_LIPSA", "Grav": "eroare" if protectie else "atentie",
                                     "Text": "Factura %s nu are o recepție activă asociată%s." % (r["NrFact"], ". Plata nu se poate salva" if protectie else "")})
            if r["Tip"] == "NC":
                avertismente.append({"Cod": "NOTA_CREDIT", "Grav": "confirmare",
                                     "Text": "Atenție! Rândul %s reprezintă o notă de credit, nu o factură!" % r["NrFact"]})
            if an and r["DataFact"] is not None and r["DataFact"].year != an:
                avertismente.append({"Cod": "ALT_AN", "Grav": "confirmare", "Text": "Atenție! Factura %s este din alt an decât cel curent!" % r["NrFact"]})
            evidenta.append({"IdPrimita": r["IdPrimita"], "NrFact": r["NrFact"], "NumarDocument": numar_document(r["NrFact"]),
                             "DataDocument": _iso(r["DataFact"]), "FelDocument": "Notă credit" if r["Tip"] == "NC" else "Factură",
                             "Total": _num(total), "Salvat": _num(salvat.get(r["IdPrimita"], Decimal(0))), "Suma": _num(ramas), "Platit": _num(platit),
                             "RestPlata": _num(rest_plata[r["IdPrimita"]]) if mod != "evidentiere" else None})
            total_platit += platit
            if platit != 0:
                numere.append(numar_document(r["NrFact"]))
        cursor.execute("SELECT a.IdPrimita, COUNT(*) AS N FROM EF_Primite a JOIN EF_Primite b ON b.CuiNormalizat = a.CuiNormalizat AND b.NrFact = a.NrFact "
                       "AND b.DataFact <=> a.DataFact AND b.Total = a.Total AND b.IdPrimita <> a.IdPrimita WHERE a.IdPrimita IN (%s) GROUP BY a.IdPrimita"
                       % _marks(len(ids)), tuple(ids))
        for r in cursor.fetchall():
            nr = next(f["NrFact"] for f in facturi if f["IdPrimita"] == r["IdPrimita"])
            avertismente.append({"Cod": "DUBLA", "Grav": "confirmare",
                                 "Text": "ATENȚIE! Factura %s apare de mai multe ori în lista primită de la ANAF (același număr, dată și valoare)." % nr})

        # classification, templates, codes
        sabloane_e, sabloane_p, coduri, operatii = [], [], None, []
        cont_iban = (partener or {}).get("ContIBAN", "")
        if clsf is not None:
            slot = index[(clsf["Cheie"], clsf["SS"])]
            sabloane_e, sabloane_p = slot["E"], slot["P"]
            valid_e = {s["IdSablon"] for s in sabloane_e}
            valid_p = {s["IdSablon"] for s in sabloane_p}
            if id_e and id_e not in valid_e:
                raise EfEroare("Șablonul de evidențiere ales nu se potrivește clasificației.", "SABLON_INVALID", 409)
            if id_p and id_p not in valid_p:
                raise EfEroare("Șablonul de plată ales nu se potrivește clasificației.", "SABLON_INVALID", 409)
            if id_e is None and len(sabloane_e) == 1 and mod != "plata":   # a payment alone has no evidence template (the operator took it off)
                id_e = sabloane_e[0]["IdSablon"]
            # The payment template is NEVER taken for the operator (01EF-07): the order of payment exists only when a payment operation is chosen.
            coduri = _coduri(cursor, (partener or {}).get("IdPartener"), clsf["IdClsf"])
            if coduri and coduri["ContBancar"]:
                cont_iban = coduri["ContBancar"]
            if id_p and not coduri:
                avertismente.append({"Cod": "CODURI_LIPSA", "Grav": "atentie",
                                     "Text": "Codurile Angajament și Indicator nu sunt configurate pentru furnizorul și clasificația selectate!"})
            if id_p and not cont_iban:
                avertismente.append({"Cod": "CONT_BANCAR_LIPSA", "Grav": "eroare",
                                     "Text": "Nu se poate adăuga operația de plată: partenerul selectat nu are niciun cont bancar configurat!"})
            nume = {}
            for id_sablon, suma_de, eticheta in ((id_e, None, "Evidențiere"), (id_p, total_platit, "Plată")):
                if not id_sablon:
                    continue
                linii = _linii_sablon(cursor, id_sablon, clsf["Forma"])
                if not linii:
                    avertismente.append({"Cod": "SABLON_FARA_LINIE", "Grav": "eroare",
                                         "Text": "Șablonul de %s nu are nicio linie pentru forma %s a clasificației." % (eticheta.lower(), clsf["Forma"])})
                    continue
                nume.update(_parti(cursor, linii))
                if suma_de is None:
                    if mod == "plata":
                        continue
                    for e in evidenta:
                        if e["Suma"] != 0:
                            operatii.append(dict(_operatie("%s · %s" % (eticheta, e["NrFact"]), linii, _dec(e["Suma"]), partener, nume), Faza="E",
                                                 IdPrimita=e["IdPrimita"]))
                else:
                    operatii.append(dict(_operatie(eticheta, linii, suma_de, partener, nume), Faza="P", IdPrimita=None))
        elif evidenta:
            avertismente.append({"Cod": "CLASIFICATIE_NEALEASA", "Grav": "info", "Text": "Alegeți clasificația pentru a vedea șabloanele."})

        # the subunit of the booking and the dates of the documents (they need the classification)
        date_doc, subunitati, note = {}, [], None
        if clsf is not None and evidenta:
            from routes.avacont.introducere_date.operatii import postare
            cursor.execute("SELECT IdSubunitate, Denumire, Implicita FROM TZ_Subunitati WHERE SursaSector = %s AND Activ = 1 ORDER BY Implicita DESC, Denumire", (clsf["SS"],))
            subunitati = cursor.fetchall()
            try:
                if subunitate is None:
                    subunitate = postare.subunitate_pentru(cursor, clsf["SS"], None)
                for f, e in zip(facturi, evidenta):
                    if f["DataFact"] is None:
                        avertismente.append({"Cod": "FARA_DATA", "Grav": "eroare", "Text": "Factura %s nu are dată." % f["NrFact"]})
                        continue
                    data, motiv_an = postare.ajusteaza_data(f["DataFact"], None, an)
                    data, motiv_luna = postare.ajusteaza_data(data, postare.data_ultimei_inchideri(cursor, subunitate["IdSubunitate"], data.year), None)
                    date_doc[f["IdPrimita"]] = data
                    if data != f["DataFact"]:
                        e["DataDocument"] = _iso(data)
                        avertismente.append({"Cod": "DATA_MUTATA", "Grav": "info", "Text": "Data documentului pentru factura %s este %s (%s)."
                                             % (f["NrFact"], data.strftime("%d.%m.%Y"), motiv_luna or motiv_an)})
            except postare.PostareEroare as err:
                avertismente.append({"Cod": err.reason, "Grav": "eroare", "Text": str(err)})
            if subunitate is not None:
                an_nota = an or (max(d.year for d in date_doc.values()) if date_doc else None)
                if an_nota:
                    randuri_note, urmatoarea = postare.note_ale_subunitatii(cursor, subunitate["IdSubunitate"], an_nota)
                    note = {"An": an_nota, "Urmatoare": urmatoarea,
                            "Existente": [{"NumarNC": n["NumarNC"], "DataNc": _iso(n["DataNc"]), "Documente": n["Documente"]} for n in randuri_note]}

        # the payment row (EF_P)
        fel_op = text_op(next((s["FelOP"] for s in sabloane_p if s["IdSablon"] == id_p), "")) if id_p else ""
        explicatie = " ".join(x for x in ["F " + " ".join(numere) if numere else "", fel_op] if x)
        explicatie = text_op(explicatie, MAX_EXPLICATIE_OP)
        data_minima = max((f["DataFact"] for f in facturi if f["DataFact"] is not None), default=None)
        numar_op = None
        if id_p and total_platit != 0 and subunitate is not None:
            numar_op = plati.numar_op_urmator(cursor, subunitate["IdSubunitate"])   # PROVISIONAL: the real number comes back from the save
        rezultat = {
            "poate_scrie": (role or "").strip().lower() == primite_ke.ROL_CONTABIL.lower(),
            "furnizor": {"Cui": cui, "Denumire": facturi[0]["DenumireP"] or "", "Partener": partener, "Parteneri": candidati},
            "clasificatii": [{"IdClsf": c["IdClsf"], "Clsf": c["Clsf"], "Denumire": c["Denumire"], "Forma": c["Forma"],
                              "AlePartenerului": c["IdClsf"] in ale_partenerului} for c in oferite],
            "clasificatie": ({"IdClsf": clsf["IdClsf"], "Clsf": clsf["Clsf"], "Denumire": clsf["Denumire"], "Forma": clsf["Forma"]} if clsf else None),
            "clasificatieSugerata": sugerata,
            "subunitate": ({"IdSubunitate": subunitate["IdSubunitate"], "Denumire": subunitate["Denumire"]} if subunitate else None),
            "subunitati": [{"IdSubunitate": s["IdSubunitate"], "Denumire": s["Denumire"]} for s in subunitati],
            "note": note,
            "sabloaneEvidentiere": sabloane_e, "sabloanePlata": sabloane_p, "idSablonE": id_e, "idSablonP": id_p, "mod": mod,
            "coduri": coduri, "contIban": cont_iban,
            "evidenta": evidenta,
            "plata": {"Valoare": _num(total_platit), "ExplicatieOP": explicatie, "FelOP": fel_op, "NumarOP": numar_op, "NumarOPProvizoriu": numar_op is not None,
                      "DataMinima": _iso(data_minima), "Activa": bool(id_p) and total_platit != 0},
            "operatii": operatii, "avertismente": avertismente}
        if interne is not None:
            interne.update(facturi=facturi, ramas=ramas_exacte, partener=partener, clsf=clsf, id_e=id_e, id_p=id_p, subunitate=subunitate, date_doc=date_doc,
                           platit={f["IdPrimita"]: _dec(e["Platit"]) for f, e in zip(facturi, evidenta)}, coduri=coduri, cont_iban=cont_iban,
                           explicatie_op=explicatie, cui=cui, mod=mod)
        return rezultat


# ---------------------------------------------------------------------------------------------
# the save of the EVIDENCE (01EF-04)
# ---------------------------------------------------------------------------------------------
def _id_partener(mod, id_fix, partener):
    if mod == "OPERATIE":
        return partener["IdPartener"] if partener else None
    return id_fix if mod == "FIX" else None


def salveaza(dc, body, role, user):
    """Books, through the posting service and in ONE transaction, the EVIDENCE and/or the PAYMENT of the ticked invoices (`mod`: evidentiere | plata | ambele).
    Evidence: one accounting note, a document per invoice, an operation per template line with its debit and credit, and a row of EF_PrimiteOperatii that ties the
    operation to the invoice. Payment (04EF-01, primite_plata.py): one document «OP», one payment operation, one order of payment, the allocations on the invoices.
    body: the same fields as the calculation plus `confirmari` (the codes of the warnings the operator confirmed) and, for a payment, `dataOP`, `explicatieOP`.
    Role «Contabil» only (checked here)."""
    primite_ke.cere_contabil(role)
    from routes.avacont.introducere_date.operatii import postare
    from . import primite_plata
    confirmari = body.get("confirmari") or []
    if not isinstance(confirmari, list) or any(not isinstance(c, str) for c in confirmari):
        raise EfEroare("Câmpul «confirmari» nu este valid.", "CAMP_INVALID", 400)
    mod = body.get("mod") or "evidentiere"
    if mod not in MODURI:
        raise EfEroare("Câmpul «mod» nu este valid.", "CAMP_INVALID", 400)
    cu_evidenta, cu_plata = mod != "plata", mod != "evidentiere"
    an = _opt_int(body, "an")
    numar_cerut = _opt_int(body, "numarNC")
    data_op = primite_plata.data_ordin(body) if cu_plata else None
    with F._unit(dc, CONTARI_SQL) as conn:
        interne = {}
        r = calcul(dc, body, role, conn=conn, interne=interne, blocheaza=True, fara_plata=not cu_plata)
        clsf, id_e, id_p, sub = interne.get("clsf"), interne.get("id_e"), interne.get("id_p"), interne.get("subunitate")
        if clsf is None or (cu_evidenta and not id_e):
            raise EfEroare("Alegeți clasificația și șablonul de evidențiere.", "ALEGERE_INCOMPLETA", 400)
        if cu_plata and not id_p:
            raise EfEroare("Alegeți șablonul de plată.", "ALEGERE_INCOMPLETA", 400)
        for a in r["avertismente"]:
            if a["Grav"] == "eroare":
                raise EfEroare(a["Text"], "AVERTISMENT_BLOCANT", 409)
        necesare = [a for a in r["avertismente"] if a["Grav"] == "confirmare" and a["Cod"] not in confirmari]
        if necesare:
            err = EfEroare("Confirmați avertismentele înainte de salvare.", "CONFIRMARE_NECESARA", 409)
            err.extra = {"confirmari_necesare": [{"Cod": a["Cod"], "Text": a["Text"]} for a in necesare]}
            raise err
        randuri = [(f, interne["ramas"][f["IdPrimita"]]) for f in interne["facturi"] if primite_ke._cent(interne["ramas"][f["IdPrimita"]]) != 0] if cu_evidenta else []
        platite = [(f, interne["platit"][f["IdPrimita"]]) for f in interne["facturi"] if interne["platit"][f["IdPrimita"]] != 0] if cu_plata else []
        if sub is None or (mod == "evidentiere" and not randuri) or (mod == "plata" and not platite) or (mod == "ambele" and not randuri and not platite):
            raise EfEroare("Nimic de înregistrat: facturile alese sunt deja înregistrate integral." if not cu_plata
                           else "Nimic de plătit: nicio factură aleasă nu are sumă de plată.", "NIMIC_DE_INREGISTRAT", 409)
        cursor = conn.cursor(dictionary=True)
        ani = {interne["date_doc"][f["IdPrimita"]].year for f, _ in randuri}
        if cu_plata:
            inchidere = postare.data_ultimei_inchideri(cursor, sub["IdSubunitate"], data_op.year)
            data_op, _motiv = postare.ajusteaza_data(data_op, inchidere, None)   # a closed month moves the payment to the 1st of the next one
            ani.add(data_op.year)
        an_nota = an or max(ani)
        if ani - {an_nota}:
            raise EfEroare("Data unui document iese din anul de lucru (%d). Alegeți alt an sau corectați data facturii/plății." % an_nota, "AN_DIFERIT", 409)
        operatii, plata = [], None
        try:
            sid = sub["IdSubunitate"]
            # the note the operator chose: an existing one takes the new documents, a number not used yet makes a new note with that number, none = the next number
            id_nota = postare.nota_existenta(cursor, sid, an_nota, numar_cerut) if numar_cerut else None
            if id_nota is not None:
                numar_nota = numar_cerut
            else:
                id_nota, numar_nota = postare.creeaza_nota(cursor, sid, an_nota, user, numar_cerut)
            partener = interne["partener"]
            activeaza_partener(cursor, partener["IdPartener"])   # the partner the booking is made for enters the flow (not hidden) and is kept for the tax code, in this transaction
            leaga_partener(cursor, interne["facturi"][0]["CuiNormalizat"], partener["IdPartener"], user)
            if randuri:
                cursor.execute("SELECT IdJurnal, Explicatie, Compartiment FROM TZ_SablonOperatii WHERE IdSablon = %s", (id_e,))
                sablon = cursor.fetchone()
                linii = _linii_sablon(cursor, id_e, clsf["Forma"])
                if sablon is None or not linii:
                    raise postare.PostareEroare("Șablonul de evidențiere nu are linie pentru forma %s a clasificației." % clsf["Forma"], "SABLON_FARA_LINIE")
                linie = linii[0]   # a template has at most one line per form (the window allows no more), or one line valid for every form
                cursor.execute("SELECT COALESCE(MAX(IdOperatiePrimita), 0) AS M FROM EF_PrimiteOperatii FOR UPDATE")
                urmator = int(cursor.fetchone()["M"]) + 1
                for f, ramas in randuri:
                    suma = primite_ke._cent(ramas)
                    data = interne["date_doc"][f["IdPrimita"]]
                    id_doc = postare.creeaza_document(cursor, id_nota, numar_document(f["NrFact"]), data, "Notă credit" if f["Tip"] == "NC" else "Factură", "E", True, user)
                    cont_d = postare.cont_pentru(cursor, sid, linie["ContDebit"], linie["AnaliticDebit"], clsf["ClsfF"], clsf["ClsfE"])
                    cont_c = postare.cont_pentru(cursor, sid, linie["ContCredit"], linie["AnaliticCredit"], clsf["ClsfF"], clsf["ClsfE"])
                    antet = {"IdJurnal": sablon["IdJurnal"], "IdClsf": clsf["IdClsf"], "IdSablon": id_e, "DataOperatie": data, "Explicatie": sablon["Explicatie"], "Tip": "E",
                             "Compartiment": sablon["Compartiment"], "Angajament": linie["PA"], "Propunere": linie["PA"], "Ordonantare": linie["Ordonantare"],
                             "Chelt": linie["Chelt"], "Auto": True}
                    id_oper, nr_ang = postare.creeaza_operatie(
                        cursor, sid, an_nota, id_doc, antet,
                        [(cont_d, _id_partener(linie["PartDebitMod"], linie["IdPartDebit"], partener), suma)],
                        [(cont_c, _id_partener(linie["PartCreditMod"], linie["IdPartCredit"], partener), suma)], user)
                    cursor.execute("INSERT INTO EF_PrimiteOperatii (IdOperatiePrimita, IdPrimita, IdOperatie, IdClsf, IdUnitate, Sursa, Suma) VALUES (%s, %s, %s, %s, %s, %s, %s)",
                                   (urmator, f["IdPrimita"], id_oper, clsf["IdClsf"], clsf["IdUnitate"], clsf["SS"], suma))
                    urmator += 1
                    operatii.append({"IdPrimita": f["IdPrimita"], "IdDocument": id_doc, "IdOper": id_oper, "NrAng": nr_ang})
            if platite:
                cursor.execute("SELECT IdJurnal, Explicatie, Compartiment FROM TZ_SablonOperatii WHERE IdSablon = %s", (id_p,))
                sablon_p = cursor.fetchone()
                linii_p = _linii_sablon(cursor, id_p, clsf["Forma"])
                if sablon_p is None or not linii_p:
                    raise postare.PostareEroare("Șablonul de plată nu are linie pentru forma %s a clasificației." % clsf["Forma"], "SABLON_FARA_LINIE")
                cerut = body.get("explicatieOP")
                explicatie = cerut if isinstance(cerut, str) and cerut.strip() else interne["explicatie_op"]
                plata = primite_plata.posteaza(cursor, dc, user, {
                    "clsf": clsf, "partener": partener, "sid": sid, "an_nota": an_nota, "id_nota": id_nota, "data_op": data_op, "id_p": id_p, "linie_p": linii_p[0],
                    "sablon_p": sablon_p, "explicatie_op": explicatie, "cont_iban": interne["cont_iban"], "coduri": interne["coduri"], "cui": interne["cui"],
                    "facturi_platite": platite})
        except postare.PostareEroare as err:
            raise EfEroare(str(err), err.reason, err.status) from err
        conn.commit()
    return {"IdNota": id_nota, "NumarNC": numar_nota, "An": an_nota, "Subunitate": sub["Denumire"], "Operatii": operatii, "Plata": plata}


# ---------------------------------------------------------------------------------------------
# what is ALREADY booked for one invoice (03EF-13)
# ---------------------------------------------------------------------------------------------
def existente(dc, id_primita, conn=None):
    """The accounting operations an invoice already has (EF_PrimiteOperatii -> TZ_Oper -> document -> note), to show them on the page «Contari» when the
    invoice is clicked: the classification, the templates that fit it (the one used is flagged) and one row per operation, evidence (`Faza` E) or payment (P).
    The operation of a link is TZ_Oper.IdOper = EF_PrimiteOperatii.IdOperatie, for K-BOT and migrated links alike. Read-only: reading is open to every session."""
    with _conexiune(dc, conn) as conn:
        cursor = conn.cursor(dictionary=True)
        cursor.execute("SELECT IdPrimita, NrFact, DataFact, Total, Semn, Tip FROM EF_Primite WHERE IdPrimita = %s", (id_primita,))
        factura = cursor.fetchone()
        if factura is None:
            raise EfEroare("Factura nu mai există.", "FACTURA_INEXISTENTA", 404)
        # what the page shows is what is saved: the link EF_PrimiteOperatii.IdOperatie = TZ_Oper.IdOper and the document / note of that operation. A link with no
        # operation under its key is sent as it is (empty fields, an explanation that says so).
        from routes.avacont.introducere_date.operatii import plati as plati_modul
        legate = plati_modul.operatii_legate(cursor, [id_primita])[id_primita]
        operatii, prima = [], None
        for rand in sorted(legate, key=lambda x: ((x["Oper"] or {}).get("DataOperatie") or date.max, x["Legatura"]["IdOperatiePrimita"])):
            e, o = rand["Legatura"], rand["Oper"]
            if o is None:
                operatii.append({"IdOperatiePrimita": e["IdOperatiePrimita"], "Faza": "E", "Suma": _num(_dec(e["Suma"])), "IdSablon": None,
                                 "Explicatie": "Legătura facturii nu are operație contabilă în TZ_Oper (IdOperatie %s)." % e["IdOperatie"], "NumarDocument": "", "DataDocument": None,
                                 "FelDocument": "", "NumarNC": None, "DataNc": None, "Subunitate": "", "Gasita": False})
                continue
            plata = (o["FelDoc"] == "P") or (o["TipOp"] == "P")
            operatii.append({"IdOperatiePrimita": e["IdOperatiePrimita"], "Faza": "P" if plata else "E", "Suma": _num(_dec(e["Suma"])),
                             "IdSablon": o["IdSablon"], "Explicatie": o["Explicatie"] or "", "NumarDocument": o["NumarDocument"] or "",
                             "DataDocument": _iso(o["DataDocument"] or o["DataOperatie"]), "FelDocument": o["FelDocument"] or "",
                             "NumarNC": o["NumarNC"], "DataNc": _iso(o["DataNc"]), "Subunitate": o["Subunitate"] or ""})
            if prima is None and (o["IdClsfOp"] or e["IdClsf"]) is not None:
                prima = {"IdClsf": o["IdClsfOp"] or e["IdClsf"], "Clsf": o["Clsf"], "DenClsf": o["DenClsf"], "ClsfX": o["ClsfX"], "SS": o["SS"], "IdSubunitate": o["IdSubunitate"]}
        clasificatie, sabloane_e, sabloane_p = None, [], []
        if prima is not None:
            clasificatie = {"IdClsf": prima["IdClsf"], "Clsf": prima["Clsf"] or "", "Denumire": prima["DenClsf"] or "", "Forma": "", "AlePartenerului": False}
            slot = _sabloane(cursor, prima["IdSubunitate"]).get((prima["ClsfX"], prima["SS"]))
            if slot is not None:
                sabloane_e, sabloane_p = slot["E"], slot["P"]
        semn = int(factura["Semn"] or 1)
        # 04EF-01: the payments already made on the invoice, each with its order and whether the order is still open to change
        from routes.avacont.introducere_date.operatii import plati
        cursor.execute("SELECT a.IdOP, a.IdPlata, a.Platit, a.DataPlata, p.NrOP, p.Explicatie, n.NumarNC FROM TZ_PlatiFacturi a "
                       "LEFT JOIN TZ_OPuri p ON p.IdOP = a.IdOP LEFT JOIN TZ_Oper po ON po.IdOper = a.IdPlata LEFT JOIN TZ_Documente pd ON pd.IdDocument = po.IdDocument "
                       "LEFT JOIN TZ_NoteC n ON n.IdNota = pd.IdNota WHERE a.IdPrimita = %s ORDER BY a.DataPlata, a.IdAlocare", (id_primita,))
        plati_facturii = []
        for a in cursor.fetchall():
            motiv = plati.motiv_blocare(cursor, a["IdOP"]) if a["IdOP"] is not None else None
            plati_facturii.append({"IdOP": a["IdOP"], "IdPlata": a["IdPlata"], "NrOP": a["NrOP"], "Platit": _num(_dec(a["Platit"])), "DataPlata": _iso(a["DataPlata"]), "Explicatie": a["Explicatie"] or "", "NumarNC": a["NumarNC"],
                                   "Blocat": motiv is not None, "MotivBlocare": motiv or ""})
        rest = plati.rest_de_plata(cursor, [id_primita])[id_primita]
        return {"plati": plati_facturii, "restPlata": _num(rest), "factura": {"IdPrimita": id_primita, "NrFact": factura["NrFact"], "NumarDocument": numar_document(factura["NrFact"]),
                            "DataFact": _iso(factura["DataFact"]), "FelDocument": "Notă credit" if factura["Tip"] == "NC" else "Factură",
                            "Total": _num(_dec(factura["Total"]) * semn)},
                "clasificatie": clasificatie, "sabloaneEvidentiere": sabloane_e, "sabloanePlata": sabloane_p, "operatii": operatii}
