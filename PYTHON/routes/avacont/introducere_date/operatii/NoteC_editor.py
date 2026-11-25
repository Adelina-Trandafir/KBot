"""SLICE-AV02-07, SLICE-AV08-01..06 (NoteC family since SLICE-AV08-04). Exact reads and updates for the accounting editor («Nota manuala» / «Modifica operatie»), in postare's transaction.

The rules are the ones of the Access form Note (Surse/RawExport/Forms/Note.txt), read 24.11.2026:
- a date is checked when it is typed or changed (Access BeforeUpdate): work year, not older than one year, after the last closing unless the journal is JI;
- an operation whose document is up to the last closing is not saved; a closing document (CCV) only when it is the last one (its date = the last closing);
- an earlier year once January is over only WARNS (Access Modifica_Operatie, message 138);
- «Protectie operatii» (per user, sent by the client): the accounts fit the classification of the operation; not for closing operations;
- two warnings the operator confirms: a new month (Access 217) and a document number already in the note (Access 347).
Every refusal names the box or the line to correct (NoteCEroare.camp / parte / linie).
"""
from datetime import date
from decimal import Decimal, InvalidOperation
from hashlib import sha256
import json

from . import NoteC
from .NoteC_tranzactie import FEL_INCHIDERE


PARTI = (("Debit", "TZ_Debit"), ("Credit", "TZ_Credit"))
TIPURI_EDITEZ = {"", "A", "V", "E", "P", FEL_INCHIDERE}
# Access TipOperatie, with the stored codes. V remains the legacy code for an uncategorized KBOT manual entry.
TIPURI_OPERATIE_MANUALA = {
    "": "< GENERAL >",
    "E": "Evidențiere furnizor",
    "P": "Plată furnizor",
    "A": "Operații diverse",
    FEL_INCHIDERE: "Închidere automată",
    "ANG": "Angajament salarii",
    "VA": "Rețineri salarii",
    "PC": "Plată card salarii",
    "PN": "Plată numerar salarii",
    "T": "Extras Trezorerie",
    "EB": "Evidențiere Burse",
    "PB": "Plată Burse",
}

# What the operator reads when a box is empty or not chosen from its list (SLICE-AV08-05).
MESAJE_ALEGERE = {
    "IdSubunitate": "Subunitatea de lucru nu este aleasă.",
    "An": "Anul de lucru nu este ales.",
    "IdOper": "Operația nu mai poate fi identificată. Reîncărcați lista.",
    "IdDocument": "Alegeți documentul existent din listă.",
    "IdNota": "Alegeți nota contabilă existentă din listă.",
    "NumarNC": "Numărul notei noi trebuie să fie un număr întreg pozitiv.",
    "IdJurnal": "Alegeți jurnalul din listă.",
    "IdClsf": "Alegeți clasificația din listă (sau «Fără clasificație»).",
}
ETICHETE = {
    "NumarDocument": "Număr document", "FelDocument": "Fel document", "Explicatie": "Explicație", "Compartiment": "Compartiment",
    "DataDocument": "Dată document", "DataOperatie": "Dată operație", "TipOperatie": "Tip operație",
}
CONFIRMARE_DOCUMENT = "DOCUMENT_DUBLAT"


def refuz(mesaj, status=409, reason="NOTA_MANUALA", camp=None, parte=None, linie=None):
    raise NoteC.NoteCEroare(mesaj, reason, status, camp=camp, parte=parte, linie=linie)


def id_exact(corp, camp, optional=False):
    valoare = corp.get(camp)
    if valoare is None and optional:
        return None
    if type(valoare) is not int or not 0 < valoare <= 2147483647:
        refuz(MESAJE_ALEGERE.get(camp, "O alegere nu este validă. Reîncărcați datele."), 400, camp=camp)
    return valoare


def text_exact(corp, camp, lungime, obligatoriu=False):
    valoare = corp.get(camp) or ""
    if not isinstance(valoare, str):
        refuz("Câmpul «%s» nu este valid." % ETICHETE.get(camp, camp), 400, camp=camp)
    valoare = valoare.strip()
    if obligatoriu and not valoare:
        refuz("Completați «%s»." % ETICHETE.get(camp, camp), 400, camp=camp)
    if len(valoare) > lungime:
        refuz("«%s» are cel mult %d de caractere." % (ETICHETE.get(camp, camp), lungime), 400, camp=camp)
    return valoare


def data_exacta(corp, camp):
    try:
        return date.fromisoformat(corp.get(camp))
    except (TypeError, ValueError):
        refuz("Completați o dată validă la «%s»." % ETICHETE.get(camp, camp), 400, camp=camp)


def confirma_cerere(corp, cod, mesaj, camp=None):
    """A warning the operator answers (the single copy in NoteC.confirma); the editor's confirmations travel in Confirmari."""
    confirmari = corp.get("Confirmari") or []
    NoteC.confirma(confirmari if isinstance(confirmari, list) else [], cod, mesaj, camp)


def document(cursor, sid, an, id_document, blocheaza=False):
    cursor.execute("SELECT d.IdDocument, d.IdNota, d.NumarDocument, d.DataDocument, d.FelDocument, d.Fel, d.IdSalariiAccess, "
                   "d.EditManual AS DocumentEditManual, d.Auto AS DocumentAuto, d.IdDocumentAccess, "
                   "d.Platit AS DocumentPlatit, d.UpdatedAt, n.NumarNC FROM TZ_Documente d JOIN TZ_NoteC n ON n.IdNota = d.IdNota "
                   "WHERE d.IdDocument = %s AND n.IdSubunitate = %s AND n.An = %s" + (" FOR UPDATE" if blocheaza else ""),
                   (id_document, sid, an))
    rand = cursor.fetchone()
    if rand is None:
        refuz("Documentul nu mai există în subunitatea și anul de lucru. Reîncărcați datele.", 404, camp="IdDocument")
    return rand


def detalii(cursor, sid, an, id_oper, blocheaza=False):
    cursor.execute("SELECT IdDocument FROM TZ_Oper WHERE IdOper = %s", (id_oper,))
    cheie = cursor.fetchone()
    if cheie is None:
        refuz("Operația nu mai există. Reîncărcați lista.", 404)
    doc = document(cursor, sid, an, cheie["IdDocument"], blocheaza)
    blocare = " FOR UPDATE" if blocheaza else ""
    cursor.execute("SELECT IdOper, IdDocument, IdJurnal, IdClsf, Explicatie, Compartiment, DataOperatie, Tip, Fel, "
                   "Cfpp, Chelt, IdOrdTbl, UpdatedAt FROM TZ_Oper WHERE IdDocument = %s ORDER BY IdOper" + blocare, (doc["IdDocument"],))
    operatii = cursor.fetchall()
    operatie = next((r for r in operatii if r["IdOper"] == id_oper), None)
    if operatie is None:
        refuz("Operația a fost mutată. Reîncărcați datele.", reason="VERSIUNE")
    linii = {}
    for parte, tabel in PARTI:
        cursor.execute("SELECT IdLinie, IdCont, IdPartener, Suma FROM " + tabel + " WHERE IdOper = %s ORDER BY IdLinie" + blocare, (id_oper,))
        linii[parte] = cursor.fetchall()
    cursor.execute("SELECT IdOperatiePrimita, IdPrimita, Suma FROM EF_PrimiteOperatii WHERE IdOperatie = %s ORDER BY IdOperatiePrimita" + blocare, (id_oper,))
    facturi = cursor.fetchall()
    cursor.execute("SELECT IdAlocare, IdPlata, IdEvidentiere, IdPrimita, Platit FROM TZ_PlatiFacturi "
                   "WHERE IdPlata = %s OR IdEvidentiere = %s ORDER BY IdAlocare" + blocare, (id_oper, id_oper))
    alocari = cursor.fetchall()
    cursor.execute("SELECT IdOP, IdOPME, NrOP, Valoare, UpdatedAt FROM TZ_OPuri WHERE IdOper = %s" + blocare, (id_oper,))
    op = cursor.fetchone()
    # SLICE-AV08-04: paid also through a payment of the next year (IdEvidentiereAnterior); the same SQL as the delete checks
    cursor.execute("SELECT " + NoteC.SQL_PLATI_PE_EVIDENTA + " AS N FROM TZ_Oper o JOIN TZ_Documente d ON d.IdDocument = o.IdDocument "
                   "JOIN TZ_NoteC n ON n.IdNota = d.IdNota WHERE o.IdOper = %s", (id_oper,))
    platita = int(cursor.fetchone()["N"] or 0) > 0
    tip = (doc["Fel"] or "").upper()
    tip_oper = (operatie["Tip"] or "").upper()
    inchidere = NoteC.data_ultimei_inchideri(cursor, sid, an)
    document_inchidere = NoteC.este_document_inchidere(doc["Fel"], doc["NumarDocument"])
    tip_manual = (doc["IdDocumentAccess"] is None and doc["DocumentEditManual"] and not doc["DocumentAuto"] and tip in TIPURI_OPERATIE_MANUALA
                  and tip_oper == tip and tip not in ("E", FEL_INCHIDERE) and not (doc["DocumentPlatit"] or platita))
    structura = ((tip in ("", "A", "V") and tip_oper in ("", "A", "V")) or tip_manual) and not (
        facturi or alocari or op or document_inchidere)
    evidentiere = tip == "E" and tip_oper == "E"
    plata_pe_factura = False
    if evidentiere and len(facturi) == 1:
        cursor.execute("SELECT IdAlocare FROM TZ_PlatiFacturi WHERE IdPrimita = %s LIMIT 1" + blocare, (facturi[0]["IdPrimita"],))
        plata_pe_factura = cursor.fetchone() is not None
    editare_evidentiere = evidentiere and len(facturi) <= 1 and not (doc["DocumentPlatit"] or alocari or op or platita or plata_pe_factura)
    # SLICE-AV08-02: the last closing document (CCV at the date of the last closing) is edited like in Access: its lines and its header, not its document
    editare_inchidere = document_inchidere and inchidere is not None and doc["DataDocument"] == inchidere and not (facturi or alocari or op)
    stare = json.dumps([doc, operatii, linii, facturi, alocari, op, plata_pe_factura, platita], sort_keys=True, default=str)
    motiv = ""
    if doc["IdSalariiAccess"] or operatie["Fel"] or (not tip_manual and (tip not in TIPURI_EDITEZ or tip_oper not in TIPURI_EDITEZ)):
        motiv = "Operația aparține unui flux contabil cu reguli proprii."
    elif operatie["IdOrdTbl"]:
        motiv = "Operația are o ordonanțare generată."
    elif op:
        motiv = NoteC.motiv_blocare(cursor, op["IdOP"]) or ""
    if not motiv:
        if document_inchidere and not editare_inchidere:
            motiv = "Un document de închidere se poate modifica doar când este ultimul (la data ultimei închideri)."
        elif not document_inchidere and inchidere is not None and doc["DataDocument"] <= inchidere:
            motiv = "Documentul face parte dintr-o lună închisă (ultima închidere: %s)." % inchidere.strftime("%d.%m.%Y")
    avertisment = ""
    if not motiv and not document_inchidere and NoteC.perioada_inchisa(doc["DataDocument"], inchidere):
        avertisment = "Atenție: operația este dintr-un an anterior, care este închis."
    return {"Operatie": operatie, "Document": doc, **linii, "Facturi": facturi, "Alocari": alocari, "OP": op,
            "NrOperatiiDocument": len(operatii), "OperatiiDocument": operatii,
            "EditareCompleta": structura or editare_evidentiere or editare_inchidere,
            "EditareDocument": structura or editare_evidentiere, "Evidentiere": evidentiere, "Inchidere": document_inchidere,
            "SchimbareTip": bool(structura and len(operatii) == 1 and not (doc["DocumentPlatit"] or platita)),
            "Refuz": motiv, "Avertisment": avertisment, "Versiune": sha256(stare.encode("utf-8")).hexdigest()}


def verifica_rol(detalii, rol):
    if not detalii or str(rol or "").strip().casefold() == "contabil":
        return
    if detalii["Facturi"] or any(a["IdPrimita"] is not None for a in detalii["Alocari"]):
        refuz("Modificarea operațiilor legate de e-facturi necesită rolul «Contabil».", 403)


def citeste_linii(cursor, corp, sid, ss, an, parte, vechi):
    sursa = corp.get(parte)
    if not isinstance(sursa, list) or not sursa:
        refuz("Introduceți cel puțin o linie pe %s." % parte, 400, camp=parte)
    rezultat = {}
    for index, rand in enumerate(sursa):
        numar = index + 1
        if not isinstance(rand, dict):
            refuz("Linia %d de la %s nu este validă." % (numar, parte), 400, parte=parte, linie=index)
        id_linie = rand.get("IdLinie")
        if id_linie is not None and (type(id_linie) is not int or id_linie not in vechi or id_linie in rezultat):
            refuz("Linia %d de la %s nu mai aparține operației. Reîncărcați operația." % (numar, parte), reason="VERSIUNE", parte=parte, linie=index)
        cont = rand.get("IdCont")
        if type(cont) is not int or cont <= 0:
            refuz("Alegeți contul pe linia %d de la %s." % (numar, parte), 400, parte=parte, linie=index)
        partener = rand.get("IdPartener")
        if partener is not None and (type(partener) is not int or partener <= 0):
            refuz("Alegeți partenerul din listă pe linia %d de la %s." % (numar, parte), 400, parte=parte, linie=index)
        try:
            suma = Decimal(str(rand.get("Suma")))
            if not suma.is_finite() or not suma or abs(suma) >= Decimal("10000000000000000") or suma != suma.quantize(NoteC.CENT):
                raise InvalidOperation
        except (InvalidOperation, ValueError):
            refuz("Pe linia %d de la %s este necesară o sumă nenulă, cu cel mult două zecimale." % (numar, parte), 400, parte=parte, linie=index)
        veche = vechi.get(id_linie) or {}
        if cont != veche.get("IdCont"):
            try:
                NoteC.cont_din_subunitate(cursor, cont, sid)
            except NoteC.NoteCEroare:
                refuz("Contul de pe linia %d de la %s nu este activ în planul de conturi al subunității." % (numar, parte), parte=parte, linie=index)
        if partener is not None and partener != veche.get("IdPartener"):
            if NoteC.partener_al_sursei(cursor, partener, ss, an) is None:
                refuz("Partenerul de pe linia %d de la %s nu aparține sursei în lucru." % (numar, parte), parte=parte, linie=index)
        rezultat[id_linie if id_linie is not None else -len(rezultat) - 1] = (cont, partener, suma, index)
    return rezultat


def valori(linii):
    """The (IdCont, IdPartener, Suma) of the lines read by citeste_linii, without the index of the line in the request."""
    return {k: v[:3] for k, v in linii.items()}


def scrie_linii(cursor, tabel, id_oper, vechi, noi):
    for id_linie in vechi.keys() - noi.keys():
        cursor.execute("DELETE FROM " + tabel + " WHERE IdLinie = %s AND IdOper = %s", (id_linie, id_oper))
    for id_linie, valoare in noi.items():
        if id_linie < 0:
            cursor.execute("INSERT INTO " + tabel + " (IdOper, IdCont, IdPartener, Suma) VALUES (%s, %s, %s, %s)", (id_oper, *valoare))
        elif valoare != tuple(vechi[id_linie][k] for k in ("IdCont", "IdPartener", "Suma")):
            cursor.execute("UPDATE " + tabel + " SET IdCont = %s, IdPartener = %s, Suma = %s WHERE IdLinie = %s AND IdOper = %s", (*valoare, id_linie, id_oper))


def verifica_protectia(cursor, corp, clsf, noi, vechi, clsf_schimbata):
    """SLICE-AV08-01: with «Protectie operatii» on, the accounts of the lines fit the classification. Checked for the lines whose account changed, or for every
    line when the classification changed (an old line left as it was keeps its account, as in Access)."""
    if corp.get("Protectie") is not True or clsf is None:
        return
    for parte, _ in PARTI:
        de_verificat = {k: v for k, v in noi[parte].items() if clsf_schimbata or v[0] != (vechi[parte].get(k) or {}).get("IdCont")}
        rele = NoteC.conturi_nepotrivite(cursor, [v[0] for v in de_verificat.values()], clsf["ClsfF"], clsf["ClsfE"])
        for _, (cont, _, _, index) in sorted(de_verificat.items(), key=lambda x: x[1][3]):
            if cont in rele:
                refuz("Contul de pe linia %d de la %s nu se potrivește clasificației alese («Protecție operații»)." % (index + 1, parte),
                      reason="PROTECTIE", parte=parte, linie=index)


def luna_noua_cerere(cursor, corp, sid, an, date_noi):
    """Access 217, the single copy in NoteC.luna_noua; the box name labels the date."""
    NoteC.luna_noua(cursor, corp.get("Confirmari") or [], sid, an, [(camp, data, ETICHETE.get(camp, camp)) for camp, data in date_noi])


def document_dublat(cursor, corp, id_nota, numar, id_document):
    """Access 347: the document number already exists in the same note."""
    cursor.execute("SELECT COUNT(*) AS N FROM TZ_Documente WHERE IdNota = %s AND TRIM(NumarDocument) = %s AND IdDocument <> %s",
                   (id_nota, numar.strip(), id_document or 0))
    if cursor.fetchone()["N"]:
        confirma_cerere(corp, CONFIRMARE_DOCUMENT, "Documentul %s există deja în această notă contabilă. Continuați?" % numar, "NumarDocument")


def antet_document(cursor, corp, sid, an, user):
    """The document fields of the request: (IdNota, NumarDocument, DataDocument, FelDocument). The note is resolved here (a new one is made when asked)."""
    data = data_exacta(corp, "DataDocument")
    numar = text_exact(corp, "NumarDocument", 100, True)
    if numar.upper().startswith("CCV"):
        refuz("Documentele de închidere au un flux propriu.", camp="NumarDocument")
    fel = text_exact(corp, "FelDocument", 100, True)
    intentie = corp.get("NotaNoua")
    numar_nota = id_exact(corp, "NumarNC", True)
    id_nota = id_exact(corp, "IdNota", True)
    if intentie is None and id_nota is None and numar_nota is None:
        refuz("Completați «Notă contabilă».", 400, camp="NumarNC")
    try:
        id_nota, _ = NoteC.rezolva_nota(cursor, sid, an, user, numar_nota, id_nota, intentie)
    except NoteC.NoteCEroare as err:
        if err.camp is None:
            err.camp = "NumarNC"
        raise
    return id_nota, numar, data, fel


def tip_document(corp, document_vechi=None, schimbare_permisa=True):
    vechi = (document_vechi or {}).get("Fel")
    if corp.get("TipOperatie") is None:
        return vechi
    tip = text_exact(corp, "TipOperatie", 3).upper()
    if tip not in TIPURI_OPERATIE_MANUALA:
        refuz("Alegeți tipul operației din listă.", camp="TipOperatie")
    if tip == (vechi or "").upper():
        return vechi
    if tip == FEL_INCHIDERE:
        refuz("Închiderea automată se generează din fluxul de închidere.", camp="TipOperatie")
    if not schimbare_permisa:
        refuz("Tipul operației este comun documentului sau ține de un flux contabil dedicat.", camp="TipOperatie")
    return tip or None


def jurnalul(cursor, id_jurnal, ss, vechi):
    cursor.execute("SELECT IdJurnal, Cod, SursaSector FROM TZ_Jurnale WHERE IdJurnal = %s", (id_jurnal,))
    rand = cursor.fetchone()
    if rand is None or (id_jurnal != vechi and rand["SursaSector"] != ss):
        refuz("Jurnalul nu aparține sursei în lucru.", camp="IdJurnal")
    return rand["Cod"]


def clasificatia(cursor, id_clsf, ss, vechi):
    if id_clsf is None:
        return None
    cursor.execute("SELECT IDClsf, ClsfF, ClsfE, SS FROM Clasificatii WHERE IDClsf = %s", (id_clsf,))
    rand = cursor.fetchone()
    if rand is None or (id_clsf != vechi and rand["SS"] != ss):
        refuz("Clasificația nu aparține sursei în lucru.", camp="IdClsf")
    return rand


def raspuns(cursor, id_oper):
    """SLICE-AV08-05: what was really saved, for the window to show and select it."""
    cursor.execute("SELECT o.IdOper, o.IdDocument, o.DataOperatie, d.IdNota, d.NumarDocument, d.DataDocument, n.NumarNC FROM TZ_Oper o "
                   "JOIN TZ_Documente d ON d.IdDocument = o.IdDocument JOIN TZ_NoteC n ON n.IdNota = d.IdNota WHERE o.IdOper = %s", (id_oper,))
    rand = cursor.fetchone()
    cursor.execute("SELECT IdOper FROM TZ_Oper WHERE IdDocument = %s ORDER BY IdOper", (rand["IdDocument"],))
    return {"IdOper": rand["IdOper"], "IdDocument": rand["IdDocument"], "IdNota": rand["IdNota"], "NumarNC": rand["NumarNC"],
            "NumarDocument": rand["NumarDocument"], "DataDocument": rand["DataDocument"].isoformat(), "DataOperatie": rand["DataOperatie"].isoformat(),
            "OperatiiDocument": [r["IdOper"] for r in cursor.fetchall()]}


def aplica_nota(cursor, user, rol, corp):
    if not isinstance(corp, dict):
        refuz("Cerere nevalidă.", 400)
    sid, an = id_exact(corp, "IdSubunitate"), id_exact(corp, "An")
    sub = NoteC.subunitate_pentru(cursor, None, sid)
    ss = sub["SursaSector"]
    id_oper, id_document = id_exact(corp, "IdOper", True), id_exact(corp, "IdDocument", True)
    editare = detalii(cursor, sid, an, id_oper, True) if id_oper else None
    operatie = editare["Operatie"] if editare else {}
    if editare:
        verifica_rol(editare, rol)
        if editare["Versiune"] != corp.get("Versiune") or id_document != operatie["IdDocument"]:
            refuz("Operația sau legăturile ei s-au modificat între timp. Reîncărcați operația.", reason="VERSIUNE")
        if editare["Refuz"]:
            refuz(editare["Refuz"])
    explicatie = text_exact(corp, "Explicatie", 255, True)
    compartiment = text_exact(corp, "Compartiment", 50) or None
    schimba_document = corp.get("ModificaDocument") is True
    if editare and not editare["EditareCompleta"]:
        if schimba_document:
            refuz("Documentul evidențierii sau al plății se gestionează prin fluxul aferent.", camp="NumarDocument")
        cursor.execute("UPDATE TZ_Oper SET Explicatie = %s, Compartiment = %s WHERE IdOper = %s", (explicatie, compartiment, id_oper))
        return raspuns(cursor, id_oper)
    if editare and schimba_document and not editare["EditareDocument"]:
        refuz("Documentul acestei operații nu se poate modifica de aici.", camp="NumarDocument")

    tip = None
    if id_document is None:
        tip = tip_document(corp)
    elif editare:
        doc_vechi = editare["Document"]
        tip = tip_document(corp, doc_vechi, editare["SchimbareTip"]) if schimba_document else doc_vechi["Fel"]
    if tip == "P" and (id_document is None or schimba_document):
        if not text_exact(corp, "NumarDocument", 100, True).isdecimal():
            refuz("Pentru «Plată furnizor», numărul documentului trebuie să fie numeric.", camp="NumarDocument")

    inchidere = NoteC.data_ultimei_inchideri(cursor, sid, an)
    jurnal = id_exact(corp, "IdJurnal")
    cod_jurnal = jurnalul(cursor, jurnal, ss, operatie.get("IdJurnal"))
    id_clsf = id_exact(corp, "IdClsf", True)
    clsf = clasificatia(cursor, id_clsf, ss, operatie.get("IdClsf"))
    data_oper = data_exacta(corp, "DataOperatie")
    date_noi = []
    if not editare or data_oper != operatie["DataOperatie"]:
        NoteC.verifica_data_operatie(data_oper, an, inchidere, cod_jurnal, "DataOperatie")
        date_noi.append(("DataOperatie", data_oper))
    vechi = {parte: {r["IdLinie"]: r for r in editare[parte]} if editare else {} for parte, _ in PARTI}
    noi = {parte: citeste_linii(cursor, corp, sid, ss, an, parte, vechi[parte]) for parte, _ in PARTI}
    if min(len(noi["Debit"]), len(noi["Credit"])) > 1:
        refuz("Debit sau Credit trebuie să aibă o singură linie.", camp="Debit")
    if sum(l[2] for l in noi["Debit"].values()) != sum(l[2] for l in noi["Credit"].values()):
        refuz("Totalul Debit trebuie să fie egal cu totalul Credit.", camp="Debit")
    if not (editare and editare["Inchidere"]):
        verifica_protectia(cursor, corp, clsf, noi, vechi, bool(editare) and id_clsf != operatie["IdClsf"])
    if tip == "E" or (editare and editare["Evidentiere"]):
        if id_clsf is None:
            refuz("Evidențierea trebuie să aibă o clasificație.", camp="IdClsf")
        if len(noi["Credit"]) != 1:
            refuz("Evidențierea are o singură linie de credit.", camp="Credit")
        _, furnizor, suma, index = next(iter(noi["Credit"].values()))
        if furnizor is None:
            refuz("Alegeți partenerul de pe creditul evidențierii.", parte="Credit", linie=index)
        if editare and editare["Facturi"]:
            vechi_parteneri = {r["IdPartener"] for r in vechi["Credit"].values()}
            if vechi_parteneri != {furnizor}:
                refuz("Păstrați partenerul legăturii cu e-factura.", parte="Credit", linie=index)
            if id_clsf != operatie["IdClsf"]:
                refuz("Păstrați clasificația legăturii cu e-factura.", camp="IdClsf")
            legatura = editare["Facturi"][0]
            if suma != legatura["Suma"]:
                NoteC.actualizeaza_suma_evidentiata(cursor, id_oper, legatura, suma)

    nota_veche = None
    if id_document is None:
        id_nota, numar, data_doc, fel = antet_document(cursor, corp, sid, an, user)
        NoteC.verifica_data_operatie(data_doc, an, inchidere, cod_jurnal, "DataDocument")
        date_noi.append(("DataDocument", data_doc))
        luna_noua_cerere(cursor, corp, sid, an, date_noi)
        document_dublat(cursor, corp, id_nota, numar, None)
        id_document = NoteC.creeaza_document(cursor, id_nota, numar, data_doc, fel, tip, False, user)
    else:
        doc = editare["Document"] if editare else document(cursor, sid, an, id_document, True)
        id_nota = doc["IdNota"]
        if not editare:
            if id_exact(corp, "IdNota") != id_nota:
                refuz("Documentul nu mai aparține notei alese. Reîncărcați documentele.", camp="IdDocument")
            if doc["IdSalariiAccess"] or (doc["Fel"] or "").upper() not in ("", "A", "V") or NoteC.este_document_inchidere(doc["Fel"], doc["NumarDocument"]):
                refuz("Alegeți un document de notă manuală sau de operații diverse.", camp="IdDocument")
            tip = (doc["Fel"] or "").upper()
            if inchidere is not None and doc["DataDocument"] <= inchidere and (cod_jurnal or "").upper() != NoteC.JURNAL_INCHIDERE:
                refuz("Documentul ales face parte dintr-o lună închisă.", camp="IdDocument")
        if schimba_document and editare:
            altele = [r for r in editare["OperatiiDocument"] if r["IdOper"] != id_oper]
            if any(r["IdOrdTbl"] or (r["Tip"] or "").upper() not in ("", "A", "V") or r["Fel"] for r in altele):
                refuz("Documentul are alte operații cu legături contabile sau cu un flux propriu.", camp="NumarDocument")
            if altele:
                cursor.execute("SELECT COUNT(*) AS N FROM TZ_Oper o WHERE o.IdDocument = %s AND o.IdOper <> %s AND ("
                               "EXISTS (SELECT 1 FROM EF_PrimiteOperatii e WHERE e.IdOperatie = o.IdOper) OR "
                               "EXISTS (SELECT 1 FROM TZ_PlatiFacturi a WHERE a.IdEvidentiere = o.IdOper OR a.IdPlata = o.IdOper) OR "
                               "EXISTS (SELECT 1 FROM TZ_OPuri p WHERE p.IdOper = o.IdOper))", (id_document, id_oper))
                if cursor.fetchone()["N"]:
                    refuz("Documentul are alte operații asociate unor facturi sau plăți.", camp="NumarDocument")
            nou_nota, numar, data_doc, fel = antet_document(cursor, corp, sid, an, user)
            if data_doc != doc["DataDocument"]:
                NoteC.verifica_data_operatie(data_doc, an, inchidere, cod_jurnal, "DataDocument")
                for alta in altele:
                    if alta["DataOperatie"] != data_doc:
                        NoteC.verifica_data_operatie(alta["DataOperatie"], an, inchidere, cod_jurnal, "DataDocument")
                date_noi.append(("DataDocument", data_doc))
            luna_noua_cerere(cursor, corp, sid, an, date_noi)
            if nou_nota != id_nota or numar.strip() != (doc["NumarDocument"] or "").strip():
                document_dublat(cursor, corp, nou_nota, numar, id_document)
            cursor.execute("UPDATE TZ_Documente SET IdNota = %s, NumarDocument = %s, DataDocument = %s, FelDocument = %s, Fel = %s, EditManual = 1 "
                           "WHERE IdDocument = %s", (nou_nota, numar, data_doc, fel, tip, id_document))
            if nou_nota != id_nota:
                nota_veche = id_nota   # SLICE-AV08-04: the note left empty goes
            id_nota = nou_nota
        else:
            luna_noua_cerere(cursor, corp, sid, an, date_noi)

    cfpp, chelt = corp.get("Cfpp") is True, corp.get("Chelt") is True
    if editare:
        tip_oper = (tip or "V") if schimba_document and tip != editare["Document"]["Fel"] else operatie["Tip"]
        cursor.execute("UPDATE TZ_Oper SET IdJurnal = %s, IdClsf = %s, Explicatie = %s, Compartiment = %s, DataOperatie = %s, "
                       "Tip = %s, Cfpp = %s, Chelt = %s WHERE IdOper = %s",
                       (jurnal, id_clsf, explicatie, compartiment, data_oper, tip_oper, cfpp, chelt, id_oper))
        for parte, tabel in PARTI:
            scrie_linii(cursor, tabel, id_oper, vechi[parte], valori(noi[parte]))
        NoteC.verifica_echilibru(cursor, id_oper)
        if nota_veche is not None:
            NoteC.curata_goale(cursor, None, nota_veche)
    else:
        antet = {"IdJurnal": jurnal, "IdClsf": id_clsf, "Explicatie": explicatie, "Compartiment": compartiment,
                 "DataOperatie": data_oper, "Tip": tip or "V", "Cfpp": cfpp, "Chelt": chelt}
        id_oper = NoteC.creeaza_operatie(cursor, sid, an, id_document, antet, valori(noi["Debit"]).values(), valori(noi["Credit"]).values(), user)
    return raspuns(cursor, id_oper)


def salveaza_nota_manuala(db_name, user, rol, cerere):
    """The save of «Nota manuala» / «Modifica operatie»: the lock and the transaction of NoteC.salvare around aplica_nota."""
    with NoteC.salvare(db_name) as conn:
        return aplica_nota(conn.cursor(dictionary=True), user, rol, cerere)
