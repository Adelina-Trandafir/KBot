# routes/avacont/introducere_date/operatii/NoteC_contari_ajutatoare.py
"""SLICE-AV08-04 (moved from efactura/primite_contari.py, slice 01EF-02): the helpers of the «Contari» of received e-invoices: invoice reading, the templates that
fit a classification, the codes of a partner, the texts of an order of payment, the description of the lines a template would produce."""
import logging
import re
from decimal import Decimal

from routes.efactura import facturi as F
from routes.efactura import primite_ke
from routes.efactura.tokens import EfEroare

from . import NoteC

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

    return NoteC.sume_evidentiate(cursor, ids)


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

    try:
        return NoteC.linii_sablon(cursor, id_sablon, forma)
    except NoteC.NoteCEroare as err:
        raise EfEroare(str(err), err.reason, err.status) from err


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


