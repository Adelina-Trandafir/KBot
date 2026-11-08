# routes/efactura/primite_contari_routes.py
"""
Route of the «Contari» of the received invoices (slice 01EF-02). `@require_session`; the unit and the role are the session's. The rules are in
primite_contari.py. It only CALCULATES; it writes nothing.

  POST /api/efactura/contari/calcul   body { ids: [IdPrimita...], idClsf?, idSablonE?, idSablonP?, platit?: { "<IdPrimita>": value }, an? }
        -> { poate_scrie, furnizor, clasificatii, clasificatie, clasificatieSugerata, sabloaneEvidentiere, sabloanePlata, idSablonE, idSablonP,
             subunitate: { IdSubunitate, Denumire } | null, subunitati: [{ IdSubunitate, Denumire }]  (those with the sector+source of the classification),
             note: { An, Urmatoare, Existente: [{ NumarNC, DataNc, Documente }] } | null  (the notes of the subunit and year, to choose from or to type a new number),
             coduri, contIban, evidenta: [...], plata: {...}, operatii: [...], avertismente: [{ Cod, Grav, Text }] }
        body fields (04EF-01 adds mod = evidentiere|plata|ambele, protectie = the tick «Protectie contari Plati/Receptii», dataOP, explicatieOP; the answer of the save then
        carries Plata { IdOper, IdOP, NrOP (the REAL number), IdDocument, Valoare, DataOP }; plata.NumarOP of the calculation is only PROVISIONAL): ids, idClsf, idSablonE, idSablonP, platit, an, idSubunitate (when several subunits share the sector+source), numarNC (save only),
        idPartener (the partner chosen when several have the supplier's tax code; the supplier is looked up by TAX CODE ONLY, hidden partners included; the partner kept for the code (EF_FurnizorPartener) or the only one is used without asking:
        `furnizor.Parteneri` lists them, `furnizor.Partener` is the one in force or null; with several and none chosen the warning PARTENER_DE_ALES blocks
        the booking). The save makes the partner visible (Ascuns = 0) and keeps it for the tax code in its transaction.

POST /api/efactura/contari/salveaza  body as above + { confirmari: [codes], idSubunitate? } -> 201 { IdNota, NumarNC, An, Subunitate, Operatii: [...] }   (01EF-04: books the EVIDENCE, role «Contabil»)
        refusal 409 CONFIRMARE_NECESARA carries { confirmari_necesare: [{ Cod, Text }] }: the operator confirms them and the call is repeated with their codes in `confirmari`.

Errors are `{ "error": "<text>", "reason": "<CODE>" }`. Audit journal: EF_CONTARE.
"""
from flask import g

from . import efactura_bp, primite_contari
from .factura_routes import _audit, _body, _guarded, _json
from .tokens import EfEroare
from routes.auth.guard import require_session

_CAMPURI = {"ids", "idClsf", "idSablonE", "idSablonP", "platit", "an", "idSubunitate", "numarNC", "idPartener", "mod", "protectie", "dataOP", "explicatieOP"}


@efactura_bp.route("/api/efactura/contari/calcul", methods=["POST"])
@require_session
@_guarded("contari/calcul")
def contari_calcul():
    body = _body()
    if set(body) - _CAMPURI:
        raise EfEroare("Corpul conține câmpuri nepermise.", "CAMP_NEPERMIS", 400)
    role = (getattr(g.session, "ctx", None) or {}).get("Role")
    return _json(primite_contari.calcul(g.session.db_name, body, role))


@efactura_bp.route("/api/efactura/contari/salveaza", methods=["POST"])
@require_session
@_guarded("contari/salveaza")
def contari_salveaza():
    body = _body()
    if set(body) - (_CAMPURI | {"confirmari"}):
        raise EfEroare("Corpul conține câmpuri nepermise.", "CAMP_NEPERMIS", 400)
    role = (getattr(g.session, "ctx", None) or {}).get("Role")
    rezultat = primite_contari.salveaza(g.session.db_name, body, role, g.session.username)
    plata = rezultat.get("Plata") or {}
    _audit("EF_CONTARE", ",".join(str(i) for i in body.get("ids", [])), "NC=%s operatii=%d%s" % (rezultat["NumarNC"], len(rezultat["Operatii"]),
                                                                                              (" OP=%s" % plata["NrOP"]) if plata else ""))
    return _json(rezultat, 201)


@efactura_bp.route("/api/efactura/contari/plata/<int:id_op>", methods=["DELETE"])
@require_session
@_guarded("contari/plata/sterge")
def contari_plata_sterge(id_op):
    """04EF-01: deletes an order of payment together with its payment operation and allocations. Refused (409 OP_BLOCAT) when the order sits in an OPME or a bank
    statement line confirms it. Role «Contabil». -> { IdOP, NrOP }"""
    from . import primite_ke, primite_plata
    role = (getattr(g.session, "ctx", None) or {}).get("Role")
    primite_ke.cere_contabil(role)
    rezultat = primite_plata.sterge(g.session.db_name, id_op, g.session.username)
    _audit("EF_CONTARE", str(id_op), "OP sters NrOP=%s" % rezultat["NrOP"])
    return _json(rezultat)


@efactura_bp.route("/api/efactura/contari/existente/<int:id_primita>", methods=["GET"])
@require_session
@_guarded("contari/existente")
def contari_existente(id_primita):
    """03EF-13: the operations an invoice already has, for the page «Contari» when a booked invoice is clicked.
    -> { factura: { IdPrimita, NrFact, NumarDocument, DataFact, FelDocument, Total }, clasificatie: { IdClsf, Clsf, Denumire, ... } | null,
         sabloaneEvidentiere, sabloanePlata (those fitting the classification in the subunit of the note),
         operatii: [{ IdOperatiePrimita, Faza: E|P, Suma, IdSablon|null, Explicatie, NumarDocument, DataDocument, FelDocument, NumarNC, DataNc, Subunitate }] }"""
    return _json(primite_contari.existente(g.session.db_name, id_primita))
