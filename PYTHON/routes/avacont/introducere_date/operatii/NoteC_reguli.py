# routes/avacont/introducere_date/operatii/NoteC_reguli.py
"""SLICE-AV08-04. The rules of the accounting notes (the only copy): the subunit, the closed period (Access EInchis, the journal JI), the accounts of the chart
(template accounts, «Protectie operatii»), the partner of the source, the IBAN of a partner, whether an operation may be deleted."""
import logging
from datetime import date, timedelta

from . import NoteC
from .NoteC_tranzactie import CENT, FEL_INCHIDERE, JURNAL_INCHIDERE, SQL_UNITATI_SURSEI, NoteCEroare

logger = logging.getLogger(__name__)


# ---------------------------------------------------------------------------------------------
# subunit and dates
# ---------------------------------------------------------------------------------------------
def subunitate_pentru(cursor, sursa_sector, cerut=None):
    """The subunit a booking belongs to: the one asked for, else the only active one with this sector+source, else the default among them."""
    if cerut is not None:
        cursor.execute("SELECT IdSubunitate, Denumire, SursaSector, Capitol, Implicita FROM TZ_Subunitati WHERE IdSubunitate = %s AND Activ = 1", (cerut,))
        row = cursor.fetchone()
        if row is None:
            raise NoteCEroare("Subunitatea aleasă nu există sau nu este activă.", "SUBUNITATE_INVALIDA")
        return row
    cursor.execute("SELECT IdSubunitate, Denumire, SursaSector, Capitol, Implicita FROM TZ_Subunitati WHERE SursaSector = %s AND Activ = 1 ORDER BY Implicita DESC, IdSubunitate",
                   (sursa_sector,))
    rows = cursor.fetchall()
    if not rows:
        raise NoteCEroare("Nu există nicio subunitate cu sectorul și sursa %s. Creați-o în «Plan de conturi»." % sursa_sector, "SUBUNITATE_LIPSA")
    if len(rows) > 1 and sum(bool(r["Implicita"]) for r in rows) != 1:
        raise NoteCEroare("Mai multe subunități au sectorul și sursa %s: alegeți una." % sursa_sector, "SUBUNITATE_AMBIGUA")
    return rows[0]


def data_ultimei_inchideri(cursor, id_subunitate, an):
    """The date of the newest closing document («CCV», Fel = I) of the subunit in that year, or None."""
    cursor.execute("SELECT MAX(d.DataDocument) AS D FROM TZ_Documente d JOIN TZ_NoteC n ON n.IdNota = d.IdNota "
                   "WHERE n.IdSubunitate = %s AND n.An = %s AND d.Fel = %s", (id_subunitate, an, FEL_INCHIDERE))
    row = cursor.fetchone()
    return row["D"] if row else None


def perioada_inchisa(data, inchidere, azi=None):
    """SLICE-AV08-02, Access EInchis (mdl_Functii2022): a date up to the last closing document, or of an earlier year once January of the current year is over."""
    if data is None:
        return False
    azi = azi or date.today()
    if inchidere is not None and data <= inchidere:
        return True
    return data.year < azi.year and azi.month > 1


CONFIRMARE_LUNA = "LUNA_NOUA"


def confirma(confirmari, cod, mesaj, camp=None):
    """A warning the operator answers: refused (reason CONFIRMARE, the code in `confirmare`) until the request carries the code in its confirmations;
    the client asks Da / Nu and sends again."""
    if not isinstance(confirmari, (list, set, tuple)) or cod not in confirmari:
        raise NoteCEroare(mesaj, "CONFIRMARE", 409, camp=camp, confirmare=cod)


def luna_noua(cursor, confirmari, sid, an, date_noi):
    """Access 217 (Note, OPuri2017_Fact): a date in a month after the month of the newest operation of the subunit and year asks for a confirmation.
    date_noi: [(camp, data, eticheta)], eticheta names the date for the operator. Checked BEFORE anything is written."""
    if not date_noi:
        return
    cursor.execute("SELECT MAX(o.DataOperatie) AS D FROM TZ_Oper o JOIN TZ_Documente d ON d.IdDocument = o.IdDocument JOIN TZ_NoteC n ON n.IdNota = d.IdNota "
                   "WHERE n.IdSubunitate = %s AND n.An = %s", (sid, an))
    rand = cursor.fetchone()
    ultima = rand["D"] if rand else None
    for camp, data, eticheta in date_noi:
        if ultima is not None and (data.year, data.month) > (ultima.year, ultima.month):
            confirma(confirmari, CONFIRMARE_LUNA, "%s (%s) este într-o lună nouă (cea mai nouă operație este din %s). Continuați?"
                     % (eticheta, data.strftime("%d.%m.%Y"), ultima.strftime("%m.%Y")), camp)


def verifica_data_document(data, an, inchidere, eticheta, camp=None, azi=None):
    """SLICE-AV08-07, Access OPuri2017_Fact.DataDocument_BeforeUpdate and EF_P (messages 218, 362): a date out of the work year or in a closed period
    is REFUSED, never moved. `eticheta` names the date for the operator (the invoice date with its number, the date of the order of payment)."""
    text = "%s (%s)" % (eticheta, data.strftime("%d.%m.%Y"))
    if data.year != an:
        raise NoteCEroare("%s nu este în anul de lucru (%d). Corectați data." % (text, an), "DATA_AN", camp=camp)
    if inchidere is not None and data <= inchidere:
        raise NoteCEroare("%s este într-o lună închisă (ultima închidere: %s). Corectați data." % (text, inchidere.strftime("%d.%m.%Y")),
                          "DATA_INCHISA", camp=camp)
    if perioada_inchisa(data, None, azi):
        raise NoteCEroare("%s este dintr-un an închis. Corectați data." % text, "DATA_INCHISA", camp=camp)


def este_document_inchidere(fel, numar_document):
    """A closing document: Fel I (K-BOT) or a number starting with CCV (Access)."""
    return (fel or "").upper() == FEL_INCHIDERE or (numar_document or "").upper().startswith("CCV")


def verifica_data_operatie(data, an, inchidere, cod_jurnal, camp, azi=None):
    """SLICE-AV08-02, the date rules of the Access form Note (DataDoc/DataOp_BeforeUpdate, VerificaDateIntroduse): the date is in the work year, not more
    than one year old, and after the last closing unless the journal is JI. Refusals name the box (`camp`)."""
    azi = azi or date.today()
    if data.year != an:
        raise NoteCEroare("Data trebuie să fie în anul de lucru %d." % an, "DATA_AN", camp=camp)
    if azi.year - data.year > 1:
        raise NoteCEroare("Anul operației este prea vechi (%d)." % data.year, "DATA_VECHE", camp=camp)
    if inchidere is not None and data <= inchidere and (cod_jurnal or "").upper() != JURNAL_INCHIDERE:
        raise NoteCEroare("Data se află într-o lună închisă (ultima închidere: %s)." % inchidere.strftime("%d.%m.%Y"), "DATA_INCHISA", camp=camp)


def ajusteaza_data(data, inchidere, an_lucru):
    """(date, reason): a date in a closed month goes to the 1st of the next month; a date of an earlier year to 1 January of the work year. reason is '' when unchanged."""
    if data is None:
        return data, ""
    nou, motiv = data, ""
    if inchidere is not None and data.year == inchidere.year and data.month <= inchidere.month:
        nou = (date(inchidere.year, inchidere.month, 28) + timedelta(days=4)).replace(day=1)
        motiv = "luna era închisă"
    if an_lucru and nou.year < an_lucru:
        nou, motiv = date(an_lucru, 1, 1), "factura este dintr-un an anterior"
    return nou, motiv


# ---------------------------------------------------------------------------------------------
# accounts
# ---------------------------------------------------------------------------------------------
def cont_pentru(cursor, id_subunitate, cont, analitic, clsf_f, clsf_e):
    """Resolve a template account only when the chart yields one compatible IdCont."""
    cursor.execute("SELECT p.IdCont FROM TZ_PlanCont p JOIN TZ_Subunitati s ON s.IdSubunitate = p.IdSubunitate AND s.SursaSector = p.SursaSector "
                   "WHERE p.IdSubunitate = %s AND p.Cont = %s AND COALESCE(p.Analitic, '') = %s AND p.Activ = 1 "
                   "AND (p.ClsfF IS NULL OR p.ClsfF = %s) AND (p.ClsfE IS NULL OR p.ClsfE = %s)", (id_subunitate, cont, analitic or "", clsf_f, clsf_e))
    rows = cursor.fetchall()
    if not rows:
        etichet = "%s%s" % (cont, ("." + analitic) if analitic else "")
        raise NoteCEroare("Contul %s nu există (activ) în planul de conturi al subunității, pentru această clasificație." % etichet, "CONT_LIPSA")
    if len(rows) > 1:
        raise NoteCEroare("Șablonul indică mai multe conturi pentru %s. Corectați șablonul sau planul de conturi." % cont, "CONT_AMBIGUU")
    return rows[0]["IdCont"]


def linii_sablon(cursor, id_sablon, forma):
    cursor.execute("SELECT IdLinie, Forma, ContDebit, AnaliticDebit, ContCredit, AnaliticCredit, PartDebitMod, IdPartDebit, PartCreditMod, IdPartCredit, "
                   "PA, Ordonantare, Chelt FROM TZ_SablonOperatiiLinie WHERE IdSablon = %s AND (Forma = %s OR Forma IS NULL OR Forma = '')",
                   (id_sablon, forma))
    randuri = cursor.fetchall()
    # An explicit form takes precedence over the template's row configured for every form.
    aplicabile = [r for r in randuri if forma and r["Forma"] == forma] or randuri
    if len(aplicabile) > 1:
        raise NoteCEroare("Șablonul are mai multe linii aplicabile aceleiași forme. Corectați șablonul.", "SABLON_AMBIGUU")
    return aplicabile


def conturi_nepotrivite(cursor, ids_cont, clsf_f, clsf_e):
    """SLICE-AV08-01, «Protectie operatii» (Access CTD.Tag of Note): the accounts among `ids_cont` that do not fit the classification of the operation.
    An account fits when its ClsfF / ClsfE are empty or equal to the classification's (the same test as cont_pentru)."""
    ids = sorted({int(i) for i in ids_cont})
    if not ids:
        return set()
    cursor.execute("SELECT IdCont FROM TZ_PlanCont WHERE IdCont IN (" + ", ".join(["%s"] * len(ids)) + ") "
                   "AND NOT ((ClsfF IS NULL OR ClsfF = %s) AND (ClsfE IS NULL OR ClsfE = %s))", (*ids, clsf_f, clsf_e))
    return {r["IdCont"] for r in cursor.fetchall()}


def cont_din_subunitate(cursor, id_cont, id_subunitate):
    cursor.execute("SELECT IdSubunitate, Activ FROM TZ_PlanCont WHERE IdCont = %s", (id_cont,))
    row = cursor.fetchone()
    if row is None or row["IdSubunitate"] != id_subunitate or not row["Activ"]:
        raise NoteCEroare("Un cont al operației nu aparține planului de conturi al subunității.", "CONT_STRAIN")


def partener_al_sursei(cursor, id_partener, ss, an):
    """The partner row (IdPartener, DenumirePartener, CodFiscal) when it is a visible partner of the unit of this sector+source and year, else None."""
    cursor.execute("SELECT IdPartener, DenumirePartener, CodFiscal FROM Parteneri WHERE IdPartener = %s AND COALESCE(Ascuns, 0) = 0 "
                   "AND IdUnitate IN (" + SQL_UNITATI_SURSEI + ")", (id_partener, ss, an))
    return cursor.fetchone()


def cont_iban_partener(cursor, id_partener, id_clsf):
    """(IBAN, codes of the partner for the classification): the bank account of the codes, else Parteneri.ContIBAN."""
    from .NoteC_contari_ajutatoare import _coduri
    coduri = _coduri(cursor, id_partener, id_clsf) if id_clsf else None
    iban = (coduri or {}).get("ContBancar") or ""
    if not iban:
        cursor.execute("SELECT ContIBAN FROM Parteneri WHERE IdPartener = %s", (id_partener,))
        row = cursor.fetchone()
        iban = (row["ContIBAN"] if row else "") or ""
    return iban, coduri


def motiv_stergere(row, inchidere, azi=None):
    """None when the operation can be deleted, else why not (Romanian, the operator reads it). The rules of the Access buttons «Sterge operatie» / «Sterge Document»
    (ModificareNC1280: STO_Click, STD_Click; mdl_Salvare_NoteContabile: StergeOperatie2020), with the gaps closed: the same checks for a document, plus the
    ordonantare and the locked order of payment. row = a row of modificare_date/operatii/citire.SQL_OPERATII."""
    azi = azi or date.today()
    if (row["FelDoc"] or "").upper() == FEL_INCHIDERE:
        return "Operațiile de închidere nu se șterg de aici."
    if (row["NumarDocument"] or "").upper().startswith("CCV"):
        return "Nu se poate șterge o operație din nota de închidere."
    data = row["DataDocument"]
    if perioada_inchisa(data, inchidere, azi):
        if inchidere is not None and data <= inchidere:
            return "Operația face parte dintr-o lună închisă (ultima închidere: %s)." % inchidere.strftime("%d.%m.%Y")
        return "Operația este dintr-un an anterior, care este închis."
    if row["IdOrdTbl"] is not None and row["OrdRand"] is not None:
        return "Operația are o ordonanțare generată. Ștergeți întâi ordonanțarea."
    if row["PlatiPeEvidenta"]:
        return "Operația selectată este plătită! NU SE POATE ȘTERGE!"
    if row["IdOP"] is not None:
        if row["IdOPME"] is not None:
            return "Ordinul de plată %s este adăugat într-un OPME." % row["NrOP"]
        if row["OPExtras"]:
            return "Ordinul de plată %s este confirmat printr-un extras de cont." % row["NrOP"]
    return None
