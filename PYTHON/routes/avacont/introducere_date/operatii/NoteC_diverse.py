# routes/avacont/introducere_date/operatii/NoteC_diverse.py
"""SLICE-AV08-04 (moved from op9.py, slice AV02-03): the SAVE of «Operatii diverse» (the Access form OP9, routine Sav_Mat2015).

Every template line with an amount becomes ONE document (Fel = A) and ONE operation (Tip = A), through NoteC.operatie_din_sablon, in the transaction of
NoteC.salvare. The date follows the rule of every note (NoteC.verifica_data_operatie). «Operatii diverse» never writes in the closing journal JI (decision
24.11.2026): a template of that journal is refused.
"""
from . import NoteC
from .NoteC_tranzactie import JURNAL_INCHIDERE, NoteCEroare


def salveaza_operatii_diverse(cursor, user, cerere):
    """cerere: idsubunitate, an, idsablon, numarnc, idnota, nota_noua, idpartener, numardocument, feldocument, datadocument, sume {IdLinie: suma}.
    Returns (IdNota, [{idlinie, iddocument, idoper}])."""
    sid, an = cerere["idsubunitate"], cerere["an"]
    sub = NoteC.subunitate_pentru(cursor, None, sid)
    cursor.execute("SELECT s.IdSablon, s.IdJurnal, s.IdSubunitate, s.Tip, s.Explicatie, s.Compartiment, j.SursaSector, j.Cod FROM TZ_SablonOperatii s "
                   "JOIN TZ_Jurnale j ON j.IdJurnal = s.IdJurnal WHERE s.IdSablon = %s", (cerere["idsablon"],))
    sablon = cursor.fetchone()
    if sablon is None or sablon["Tip"] != "A":
        raise NoteCEroare("Șablonul ales nu există sau nu este de tip «operații diverse».", "SABLON_INVALID", 404)
    if sablon["IdSubunitate"] not in (None, 0, sid) or sablon["SursaSector"] != sub["SursaSector"]:
        raise NoteCEroare("Șablonul ales nu aparține subunității în lucru.", "SABLON_STRAIN")
    if (sablon["Cod"] or "").upper() == JURNAL_INCHIDERE:
        raise NoteCEroare("Operațiile diverse nu pot fi înregistrate în jurnalul de închidere (JI).", "JURNAL_INCHIDERE")
    data = cerere["datadocument"]
    NoteC.verifica_data_operatie(data, an, NoteC.data_ultimei_inchideri(cursor, sid, an), sablon["Cod"], "datadocument")
    cursor.execute("SELECT IdLinie, Forma, ContDebit, AnaliticDebit, ContCredit, AnaliticCredit, PartDebitMod, IdPartDebit, PartCreditMod, IdPartCredit, "
                   "PA, Ordonantare, Chelt, IdClsf FROM TZ_SablonOperatiiLinie WHERE IdSablon = %s ORDER BY IdLinie", (cerere["idsablon"],))
    linii = [l for l in cursor.fetchall() if l["IdLinie"] in cerere["sume"]]
    if len(linii) != len(cerere["sume"]):
        raise NoteCEroare("O linie nu aparține șablonului ales (șablonul s-a schimbat între timp).", "SABLON_SCHIMBAT")
    if any("OPERATIE" in (l["PartDebitMod"], l["PartCreditMod"]) for l in linii) and cerere["idpartener"] is None:
        raise NoteCEroare("Alegeți partenerul: șablonul are linii cu «partenerul operației».", "PARTENER_LIPSA")
    if cerere["idpartener"] is not None and NoteC.partener_al_sursei(cursor, cerere["idpartener"], sub["SursaSector"], an) is None:
        raise NoteCEroare("Partenerul ales nu mai există.", "PARTENER_LIPSA", 404)
    id_nota, _ = NoteC.rezolva_nota(cursor, sid, an, user, cerere["numarnc"], cerere["idnota"], cerere["nota_noua"])
    rezultat = []
    for l in linii:
        clsf = None
        if l["IdClsf"]:
            cursor.execute("SELECT IDClsf AS IdClsf, ClsfF, ClsfE FROM Clasificatii WHERE IDClsf = %s", (l["IdClsf"],))
            clsf = cursor.fetchone() or {"IdClsf": l["IdClsf"], "ClsfF": None, "ClsfE": None}
        antet, debit, credit = NoteC.operatie_din_sablon(cursor, sid, sablon, l, clsf, "A", data, cerere["idpartener"], cerere["sume"][l["IdLinie"]])
        id_doc, id_oper = NoteC.document_si_operatie(cursor, sid, an, id_nota, cerere["numardocument"], data, cerere["feldocument"], "A", antet, debit, credit, user)
        rezultat.append({"idlinie": l["IdLinie"], "iddocument": id_doc, "idoper": id_oper})
    return id_nota, rezultat
