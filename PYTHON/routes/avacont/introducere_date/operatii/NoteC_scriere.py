# routes/avacont/introducere_date/operatii/NoteC_scriere.py
"""SLICE-AV08-04. What writes the accounting notes (the only copy): notes and their numbers, documents, operations and their lines, the balance, a template line
made operation, the link with a received e-invoice, the payment, and the deletes (an operation, the documents and notes left empty)."""
import logging
from decimal import Decimal

from . import NoteC
from .NoteC_tranzactie import CENT, NoteCEroare

logger = logging.getLogger(__name__)


# ---------------------------------------------------------------------------------------------
# numbers
# ---------------------------------------------------------------------------------------------
def numar_nota_urmator(cursor, id_subunitate, an):
    cursor.execute("SELECT COALESCE(MAX(NumarNC), 0) + 1 AS N FROM TZ_NoteC WHERE IdSubunitate = %s AND An = %s FOR UPDATE", (id_subunitate, an))
    return int(cursor.fetchone()["N"])


def nota_existenta(cursor, id_subunitate, an, numar):
    """IdNota of the note with this number in the subunit and year (locked), or None."""
    cursor.execute("SELECT IdNota FROM TZ_NoteC WHERE IdSubunitate = %s AND An = %s AND NumarNC = %s FOR UPDATE", (id_subunitate, an, numar))
    row = cursor.fetchone()
    return row["IdNota"] if row else None


def rezolva_nota(cursor, sid, an, user, numar, id_nota=None, nota_noua=None):
    if nota_noua is not None and not isinstance(nota_noua, bool):
        raise NoteCEroare("Alegerea notei contabile nu este validă.", "NOTA_INVALIDA", 400)
    if id_nota is not None:
        cursor.execute("SELECT NumarNC FROM TZ_NoteC WHERE IdNota = %s AND IdSubunitate = %s AND An = %s FOR UPDATE", (id_nota, sid, an))
        rand = cursor.fetchone()
        if nota_noua or rand is None or (numar is not None and rand["NumarNC"] != numar):
            raise NoteCEroare("Nota aleasă s-a schimbat sau nu aparține subunității și anului de lucru.", "NOTA_INVALIDA")
        return id_nota, rand["NumarNC"]
    if nota_noua is False:
        raise NoteCEroare("Alegeți nota contabilă existentă.", "NOTA_LIPSA", 400)
    if nota_noua is None and numar is not None:  # exact unique key used by existing clients
        id_nota = nota_existenta(cursor, sid, an, numar)
        if id_nota is not None:
            return id_nota, numar
    return creeaza_nota(cursor, sid, an, user, numar)


def note_ale_subunitatii(cursor, id_subunitate, an):
    """The notes of the subunit and year, newest number first, with the number of documents: [{NumarNC, DataNc, Documente}], and the next free number."""
    cursor.execute("SELECT n.IdNota, n.NumarNC, n.DataNc, COUNT(d.IdDocument) AS Documente FROM TZ_NoteC n LEFT JOIN TZ_Documente d ON d.IdNota = n.IdNota "
                   "WHERE n.IdSubunitate = %s AND n.An = %s GROUP BY n.IdNota, n.NumarNC, n.DataNc ORDER BY n.NumarNC DESC", (id_subunitate, an))
    rows = cursor.fetchall()
    cursor.execute("SELECT COALESCE(MAX(NumarNC), 0) + 1 AS N FROM TZ_NoteC WHERE IdSubunitate = %s AND An = %s", (id_subunitate, an))
    return rows, int(cursor.fetchone()["N"])


# ---------------------------------------------------------------------------------------------
# writing
# ---------------------------------------------------------------------------------------------
def creeaza_nota(cursor, id_subunitate, an, user, numar=None):
    """A new note. `numar` = the number the operator typed (it must not exist yet); None = the next number."""
    if numar is None:
        numar = numar_nota_urmator(cursor, id_subunitate, an)
    else:
        cursor.execute("SELECT IdNota FROM TZ_NoteC WHERE IdSubunitate = %s AND An = %s AND NumarNC = %s FOR UPDATE", (id_subunitate, an, numar))
        if cursor.fetchone() is not None:
            raise NoteCEroare("Nota contabilă %d există deja." % numar, "NOTA_EXISTENTA")
    cursor.execute("INSERT INTO TZ_NoteC (IdSubunitate, An, NumarNC, DataNc, UN) VALUES (%s, %s, %s, NOW(), %s)", (id_subunitate, an, numar, user))
    return cursor.lastrowid, numar


def creeaza_document(cursor, id_nota, numar_document, data_document, fel_document, fel, auto, user):
    cursor.execute("INSERT INTO TZ_Documente (IdNota, NumarDocument, DataDocument, FelDocument, Fel, Auto, EditManual, UN) VALUES (%s, %s, %s, %s, %s, %s, %s, %s)",
                   (id_nota, numar_document, data_document, fel_document, fel, int(bool(auto)), int(not auto), user))
    return cursor.lastrowid


def creeaza_operatie(cursor, id_subunitate, an, id_document, antet, linii_debit, linii_credit, user):
    """One operation with its lines. antet: IdJurnal, IdClsf, IdSablon, DataOperatie, Explicatie, Tip, Compartiment, Angajament, Propunere, Ordonantare, Chelt, Auto.
    linii_debit / linii_credit: [(IdCont, IdPartener or None, Suma)]. Returns IdOper (SLICE-AV08-03: NrAng stays NULL, the commitment number of old ALOP)."""
    for id_cont, _, _ in list(linii_debit) + list(linii_credit):
        NoteC.cont_din_subunitate(cursor, id_cont, id_subunitate)
    cursor.execute(
        "INSERT INTO TZ_Oper (IdDocument, IdJurnal, IdClsf, IdSablon, DataOperatie, Explicatie, Tip, Compartiment, Cfpp, Angajament, Propunere, Ordonantare, Chelt, Auto, UN) "
        "VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s)",
        (id_document, antet["IdJurnal"], antet.get("IdClsf"), antet.get("IdSablon"), antet["DataOperatie"], antet.get("Explicatie") or "", antet.get("Tip"),
         antet.get("Compartiment"), int(bool(antet.get("Cfpp", True))), 1 if antet.get("Angajament") else 0, 1 if antet.get("Propunere") else 0, 1 if antet.get("Ordonantare") else 0,
         1 if antet.get("Chelt") else 0, 1 if antet.get("Auto") else 0, user))
    id_oper = cursor.lastrowid
    for tabel, linii in (("TZ_Debit", linii_debit), ("TZ_Credit", linii_credit)):
        for id_cont, id_partener, suma in linii:
            cursor.execute("INSERT INTO " + tabel + " (IdOper, IdCont, IdPartener, Suma) VALUES (%s, %s, %s, %s)", (id_oper, id_cont, id_partener, suma))
    verifica_echilibru(cursor, id_oper)
    return id_oper


def verifica_echilibru(cursor, id_oper):
    cursor.execute("SELECT (SELECT COALESCE(SUM(Suma), 0) FROM TZ_Debit WHERE IdOper = %s) AS D, (SELECT COALESCE(SUM(Suma), 0) FROM TZ_Credit WHERE IdOper = %s) AS C",
                   (id_oper, id_oper))
    row = cursor.fetchone()
    if row["D"].quantize(CENT) != row["C"].quantize(CENT):
        raise NoteCEroare("Operația nu este echilibrată: debit %s, credit %s." % (row["D"], row["C"]), "DEBIT_DIFERIT_CREDIT")


def document_si_operatie(cursor, sid, an, id_nota, numar, data, fel_document, fel, antet, debit, credit, user):
    id_document = creeaza_document(cursor, id_nota, numar, data, fel_document, fel, antet.get("Auto"), user)
    id_oper = creeaza_operatie(cursor, sid, an, id_document, antet, debit, credit, user)
    return id_document, id_oper


# ---------------------------------------------------------------------------------------------
# deleting (SLICE-AV08-04: the only copy; the callers have checked the rules under the lock of `salvare`)
# ---------------------------------------------------------------------------------------------
def sterge_operatie(cursor, id_oper):
    """Deletes one operation WITHOUT an order of payment (an operation with an order goes through NoteC.sterge_plata): its allocations as a payment,
    its link to the received invoice (the invoice can be booked again), its lines and the operation. The document and the note: curata_goale."""
    cursor.execute("DELETE FROM TZ_PlatiFacturi WHERE IdPlata = %s", (id_oper,))
    cursor.execute("DELETE FROM EF_PrimiteOperatii WHERE IdOperatie = %s", (id_oper,))
    cursor.execute("DELETE FROM TZ_Debit WHERE IdOper = %s", (id_oper,))
    cursor.execute("DELETE FROM TZ_Credit WHERE IdOper = %s", (id_oper,))
    cursor.execute("DELETE FROM TZ_Oper WHERE IdOper = %s", (id_oper,))


def curata_goale(cursor, id_document, id_nota):
    """The document left without operations goes, then the note left without documents (after a delete, or after a document moved to another note)."""
    if id_document is not None:
        cursor.execute("DELETE FROM TZ_Documente WHERE IdDocument = %s AND NOT EXISTS (SELECT 1 FROM TZ_Oper WHERE IdDocument = %s)", (id_document, id_document))
    if id_nota is not None:
        cursor.execute("DELETE FROM TZ_NoteC WHERE IdNota = %s AND NOT EXISTS (SELECT 1 FROM TZ_Documente WHERE IdNota = %s)", (id_nota, id_nota))


def leaga_evidentiere(cursor, id_primita, id_oper, clsf, suma):
    cursor.execute("SELECT COALESCE(MAX(IdOperatiePrimita), 0) + 1 AS N FROM EF_PrimiteOperatii FOR UPDATE")
    id_legatura = cursor.fetchone()["N"]
    cursor.execute(
        "INSERT INTO EF_PrimiteOperatii (IdOperatiePrimita, IdPrimita, IdOperatie, IdClsf, IdUnitate, Sursa, Suma) VALUES (%s, %s, %s, %s, %s, %s, %s)",
        (id_legatura, id_primita, id_oper, clsf["IdClsf"], clsf["IdUnitate"], clsf["SS"], suma))


def sume_evidentiate(cursor, ids_primita, fara_legatura=None):

    return {i: sum((e["Suma"] for e in randuri), Decimal(0))
            for i, randuri in NoteC.evidentieri(cursor, ids_primita, fara_legatura).items()}


def actualizeaza_suma_evidentiata(cursor, id_oper, legatura, suma):

    id_primita = legatura["IdPrimita"]
    cursor.execute("SELECT Total, Semn FROM EF_Primite WHERE IdPrimita = %s FOR UPDATE", (id_primita,))
    factura = cursor.fetchone()
    if factura is None:
        raise NoteCEroare("E-factura asociată nu mai există.", "FACTURA_LIPSA")
    id_legatura = legatura["IdOperatiePrimita"]
    salvat = sume_evidentiate(cursor, [id_primita], id_legatura)[id_primita]
    rest = NoteC.cent(factura["Total"]) * int(factura["Semn"] or 1) - salvat
    if not NoteC.in_limita(suma, rest):
        raise NoteCEroare("Noua sumă nu se încadrează în restul de evidențiat al e-facturii (%s)." % rest, "EVIDENTA_PESTE_REST")
    cursor.execute("UPDATE EF_PrimiteOperatii SET Suma = %s WHERE IdOperatiePrimita = %s AND IdOperatie = %s AND IdPrimita = %s",
                   (suma, id_legatura, id_oper, id_primita))


def salveaza_plata(cursor, date_plata, *, operatii=None, e_facturi=None):

    if operatii is not None:
        ids = sorted(a["IdOper"] for a in operatii)
        legaturi = NoteC.primite_pe_evidentieri(cursor, ids)
        valori = NoteC.valori_pe_operatii(cursor, ids, date_plata["id_partener"], legaturi)
        pe_factura = {}
        for alocare in operatii:
            id_evidenta = alocare["IdOper"]
            suma = alocare["Platit"]
            if not NoteC.in_limita(suma, valori[id_evidenta][2]):
                raise NoteC.PlatiEroare("Restul de plată pentru %s s-a modificat. Reîncărcați facturile." % alocare["NumarDocument"], "PLATA_PESTE_REST")
            if alocare["DataDocument"] > date_plata["data_op"]:
                raise NoteC.PlatiEroare("Data plății precedă factura %s." % alocare["NumarDocument"], "DATA_PLATA")
            if id_evidenta in legaturi:
                id_primita = legaturi[id_evidenta][0]
                alocare["IdPrimita"] = id_primita
                pe_factura[id_primita] = pe_factura.get(id_primita, Decimal(0)) + suma
        disponibil = NoteC.rest_de_plata(cursor, sorted(pe_factura))
        if pe_factura and str(date_plata.get("rol") or "").strip().casefold() != "contabil":
            raise NoteCEroare("Plata unei e-facturi necesită rolul «Contabil».", "ROL_INTERZIS", 403)
        if any(not NoteC.in_limita(total, disponibil[id_primita]) for id_primita, total in pe_factura.items()):
            raise NoteC.PlatiEroare("Sumele cumulate depășesc restul unei e-facturi selectate.", "PLATA_PESTE_REST")
        fara_receptie = pe_factura.keys() - NoteC.primite_cu_receptie(cursor, pe_factura)
        if fara_receptie and (date_plata.get("protectie") or not fara_receptie <= date_plata.get("confirmari_receptie", set())):
            raise NoteC.PlatiEroare("Plata include o e-factură fără recepție activă. Reîncărcați facturile și verificați recepția.", "RECEPTIE_LIPSA")
    if e_facturi is not None:
        for factura in e_facturi:
            if factura["DataFact"] and factura["DataFact"] > date_plata["data_op"]:
                raise NoteC.PlatiEroare("Data plății precedă factura %s." % factura["NrFact"], "DATA_PLATA")
    id_oper, id_op, numar, id_document = NoteC.posteaza_ordin(cursor, date_plata)
    if e_facturi is not None:
        NoteC.aloca_pe_evidentieri(cursor, id_oper, id_op, e_facturi, date_plata["data_op"], date_plata["total"], date_plata["user"])
    if operatii is not None:
        NoteC.aloca_pe_operatii(cursor, id_oper, id_op, operatii, date_plata["data_op"], date_plata["total"], date_plata["user"])
    return id_oper, id_op, numar, id_document


def sablonul(cursor, id_sablon, forma, faza):
    """(template header, its line for the form of the classification) or a refusal. faza = «evidențiere» / «plată» (only for the message)."""
    cursor.execute("SELECT IdSablon, IdJurnal, Explicatie, Compartiment, FelOP FROM TZ_SablonOperatii WHERE IdSablon = %s", (id_sablon,))
    sablon = cursor.fetchone()
    linii = NoteC.linii_sablon(cursor, id_sablon, forma) if sablon is not None else []
    if sablon is None or not linii:
        raise NoteCEroare("Șablonul de %s nu are linie pentru forma %s a clasificației." % (faza, forma), "SABLON_FARA_LINIE")
    return sablon, linii[0]


def operatie_din_sablon(cursor, sid, sablon, linie, clsf, tip, data, id_partener, suma):
    """(antet, debit, credit) of ONE template line: the accounts of the chart for the classification, the partner of each side, the flags of the line.
    The only copy, used by «Operatii diverse», «Facturi si plati», «Contari» and the payment (NoteC_plati.posteaza_ordin). sablon: IdSablon, IdJurnal, Explicatie,
    Compartiment; clsf: {IdClsf, ClsfF, ClsfE} or None."""
    clsf = clsf or {}
    cont_d = NoteC.cont_pentru(cursor, sid, linie["ContDebit"], linie["AnaliticDebit"], clsf.get("ClsfF"), clsf.get("ClsfE"))
    cont_c = NoteC.cont_pentru(cursor, sid, linie["ContCredit"], linie["AnaliticCredit"], clsf.get("ClsfF"), clsf.get("ClsfE"))
    antet = {"IdJurnal": sablon["IdJurnal"], "IdClsf": clsf.get("IdClsf"), "IdSablon": sablon["IdSablon"], "DataOperatie": data, "Explicatie": sablon["Explicatie"],
             "Tip": tip, "Compartiment": sablon["Compartiment"], "Angajament": linie["PA"], "Propunere": linie["PA"], "Ordonantare": linie["Ordonantare"],
             "Chelt": linie["Chelt"], "Auto": True}
    debit = [(cont_d, NoteC.id_partener_linie(linie["PartDebitMod"], linie["IdPartDebit"], id_partener), suma)]
    credit = [(cont_c, NoteC.id_partener_linie(linie["PartCreditMod"], linie["IdPartCredit"], id_partener), suma)]
    return antet, debit, credit


def sterge_randuri(cursor, rows):
    """Deletes the operations of «Modificare operatii» that passed motiv_stergere: an operation with an order through sterge_plata, any other through
    sterge_operatie; then the documents and the notes left empty. rows = rows of citire.SQL_OPERATII."""
    for r in rows:
        if r["IdOP"] is not None:
            NoteC.sterge_plata(cursor, r["IdOP"])
        else:
            sterge_operatie(cursor, r["IdOper"])
    for id_document, id_nota in {(r["IdDocument"], r["IdNota"]) for r in rows}:
        curata_goale(cursor, id_document, id_nota)
