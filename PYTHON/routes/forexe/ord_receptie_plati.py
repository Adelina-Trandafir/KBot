# routes/forexe/ord_receptie_plati.py
"""
Slice 02EF-13 (operator, 07.11.2026): the payment that comes from the bank statement is attached
to the lines of an ordonantare made from a RECEPTION (slice 02EF-11).

WHAT IS PENDING. A line of such an ordonantare has a `FX_ORD_TBL_REC` row with `IDRP` (the
reception line it covers) and `IdPlataFX` EMPTY: nothing was paid when the ordonantare was made.
When the payment arrives (`FX_Plati`, with its bank statement row `FX_Extrase` joined on
`Referinta_TREZOR`) the row gets its `IdPlataFX`. A payment that is attached is `ordonantata`
(the rule everywhere is `EXISTS FX_ORD_TBL_REC.IdPlataFX`), so it is not offered for a second
ordonantare by `post_ord_genereaza`.

THE RULE (automatic): same angajament, same indicator, the payer's tax code (`platitor_cui`,
normalized) equals the beneficiary's of the line, and the payment's sum does not exceed what the
row still has. Oldest ordonantare first, oldest payment first. A payment SMALLER than the row
splits it: the row keeps the rest, a new row (same line, same `IDRP`) takes the payment, so the
line's total never changes. A payment that fits no row is left alone - the operator attaches it
by hand (`GET .../linii-candidate`, `POST .../leaga-plata`). Nothing is guessed.

WHERE IT RUNS. `leaga_automat` is called (a) at the end of the payments step of the FOREXE
ingest and (b) at the end of the bank statement import - the payment and its statement can come
in either order - and on request (`POST /api/forexe/ord/leaga-plati-automat`). The two ingest
calls are additive and protected: a failure here is logged and shown as a warning, it never
stops the import.
"""
import logging

from flask import request, g, current_app

from routes.auth.guard import require_session
from utils.database import get_kbot_connection

from . import forexe_bp, partner_links

logger = logging.getLogger(__name__)

TOLERANTA = 0.005


class Refuz(Exception):
    """A request the operator can fix; the message is Romanian."""

    def __init__(self, message, status=400):
        super().__init__(message)
        self.status = status


def _json_utf8(payload, status):
    import json
    return current_app.response_class(json.dumps(payload, ensure_ascii=False, default=str),
                                      status=status, mimetype="application/json")


def _num(v) -> float:
    return float(v or 0)


_SQL_RANDURI_IN_ASTEPTARE = (
    "SELECT R.IDORDRECP, R.IDORDTBLP, R.Valoare, R.IDRP, T.CodAngajament, T.CodIndicator, "
    "       PT.CodFiscal, O.NrORD, O.DataORD "
    "  FROM FX_ORD_TBL_REC R "
    "  JOIN FX_ORD_TBL T ON T.IDORDTBLP = R.IDORDTBLP "
    "  JOIN FX_ORD O ON O.IDORDP = T.IDORDP "
    "  LEFT JOIN FX_ORD_PART PT ON PT.IDORDPARTP = T.IDORDPARTP "
    " WHERE R.IdPlataFX IS NULL AND R.IDRP IS NOT NULL {filtru} "
    " ORDER BY O.DataORD, O.IDORDP, R.IDORDRECP"
)

_SQL_PLATI_LIBERE = (
    "SELECT P.IdPlataFX, P.CodAngajament, P.CodIndicator, P.Suma, P.NrOP, "
    "       DATE(COALESCE(E.DataBanca, P.Data_plata)) AS Data, E.platitor_cui, E.platitor_nume "
    "  FROM FX_Plati P "
    "  JOIN FX_Extrase E ON P.Referinta_TREZOR = E.Referinta "
    " WHERE NOT EXISTS (SELECT 1 FROM FX_ORD_TBL_REC X WHERE X.IdPlataFX = P.IdPlataFX) "
    "   AND P.Suma > 0 AND P.CodAngajament IN ({locuri}) {filtru} "
    " ORDER BY DATE(COALESCE(E.DataBanca, P.Data_plata)), P.IdPlataFX"
)


def _randuri_in_asteptare(cursor, cod=None) -> list:
    filtru = ""
    parametri = ()
    if cod:
        filtru = " AND T.CodAngajament = %s "
        parametri = (cod,)
    cursor.execute(_SQL_RANDURI_IN_ASTEPTARE.format(filtru=filtru), parametri)
    return list(cursor.fetchall() or [])


def _ataseaza(cursor, rand: dict, id_plata_fx: int, suma: float) -> None:
    """Attach the payment to the row; a smaller payment splits the row (the line's total stays)."""
    if abs(_num(rand["Valoare"]) - suma) <= TOLERANTA:
        cursor.execute(
            "UPDATE FX_ORD_TBL_REC SET IdPlataFX = %s WHERE IDORDRECP = %s AND IdPlataFX IS NULL",
            (id_plata_fx, rand["IDORDRECP"]))
        if cursor.rowcount != 1:
            raise RuntimeError(f"Randul {rand['IDORDRECP']} nu mai este liber.")
        rand["Valoare"] = 0.0
        return
    rest = round(_num(rand["Valoare"]) - suma, 2)
    cursor.execute(
        "UPDATE FX_ORD_TBL_REC SET Valoare = %s WHERE IDORDRECP = %s AND IdPlataFX IS NULL",
        (rest, rand["IDORDRECP"]))
    if cursor.rowcount != 1:
        raise RuntimeError(f"Randul {rand['IDORDRECP']} nu mai este liber.")
    cursor.execute(
        "INSERT INTO FX_ORD_TBL_REC (IDORDTBLP, IDORDREC, IdPlataFX, IDRP, Valoare) VALUES (%s, NULL, %s, %s, %s)",
        (rand["IDORDTBLP"], id_plata_fx, rand["IDRP"], round(suma, 2)))
    rand["Valoare"] = rest


def leaga_automat(cursor, cod=None) -> dict:
    """Attach the free payments that match a pending row. `cod` None = every angajament with pending rows.

    Returns {legate: [{id_plata_fx, idordrecp, nr_ord, suma}], ramase: n}. Runs in the caller's
    transaction (commit / rollback stay with the caller).
    """
    randuri = _randuri_in_asteptare(cursor, cod)
    if not randuri:
        return {"legate": [], "ramase": 0}
    coduri = sorted({r["CodAngajament"] for r in randuri if r["CodAngajament"]})
    if not coduri:
        return {"legate": [], "ramase": 0}
    locuri = ", ".join(["%s"] * len(coduri))
    cursor.execute(_SQL_PLATI_LIBERE.format(locuri=locuri, filtru=""), tuple(coduri))
    plati = list(cursor.fetchall() or [])

    legate = []
    for p in plati:
        cui = partner_links.normalize_cui(p["platitor_cui"])
        if not cui:
            continue
        suma = round(_num(p["Suma"]), 2)
        for rand in randuri:
            if (rand["CodAngajament"] != p["CodAngajament"] or rand["CodIndicator"] != p["CodIndicator"]
                    or partner_links.normalize_cui(rand["CodFiscal"]) != cui
                    or _num(rand["Valoare"]) < suma - TOLERANTA):
                continue
            _ataseaza(cursor, rand, int(p["IdPlataFX"]), suma)
            legate.append({"id_plata_fx": int(p["IdPlataFX"]), "idordrecp": int(rand["IDORDRECP"]),
                           "nr_ord": rand["NrORD"], "suma": suma})
            break
    ramase = sum(1 for p in plati if p["IdPlataFX"] not in {l["id_plata_fx"] for l in legate})
    if legate:
        logger.info("[forexe.ord_receptie_plati] %s plata(i) legata(e) de ordonantari din receptie: %s",
                    len(legate), legate)
    return {"legate": legate, "ramase": ramase}


def leaga_automat_protejat(cursor, cod, avertismente: list) -> None:
    """The call the two ingest paths make: a failure is logged and told as a warning, never raised."""
    try:
        rez = leaga_automat(cursor, cod)
        if rez["legate"]:
            avertismente.append(
                f"{len(rez['legate'])} plată(ți) au fost legate automat de ordonanțări făcute din recepție.")
    except Exception:
        logger.exception("[forexe.ord_receptie_plati] legarea automata a esuat (cod=%s); importul continua", cod)
        avertismente.append(
            "Legarea automată a plăților de ordonanțările făcute din recepție a eșuat; "
            "le puteți lega manual. Detalii în jurnalul serverului.")


def _in_tranzactie(treaba, eticheta: str):
    db_name = g.session.db_name
    conn = None
    try:
        conn = get_kbot_connection(db_name)
        conn.autocommit = False
        cursor = conn.cursor(dictionary=True)
        if not conn.in_transaction:
            conn.start_transaction()
        rezultat = treaba(cursor)
        conn.commit()
        return _json_utf8(rezultat, 200)
    except Refuz as e:
        if conn is not None:
            conn.rollback()
        return _json_utf8({"error": str(e)}, e.status)
    except Exception:
        if conn is not None:
            conn.rollback()
        logger.exception("[forexe.ord_receptie_plati] %s %s a esuat", db_name, eticheta)
        return _json_utf8({"error": "Plata nu a putut fi legată de ordonanțare. Detalii în jurnalul serverului."}, 500)
    finally:
        if conn is not None:
            conn.close()


@forexe_bp.route("/api/forexe/ord/leaga-plati-automat", methods=["POST"])
@require_session
def post_leaga_plati_automat():
    """Runs the automatic rule now. Body: { "cod": "..." } (cod missing = every angajament)."""
    date_in = request.get_json(silent=True) or {}
    cod = str(date_in.get("cod") or "").strip() or None
    return _in_tranzactie(lambda cursor: leaga_automat(cursor, cod), "leaga-plati-automat")


@forexe_bp.route("/api/forexe/ord/plata/<int:id_plata_fx>/linii-candidate", methods=["GET"])
@require_session
def get_linii_candidate(id_plata_fx):
    """The rows a free payment may be attached to by hand: same angajament and indicator, still pending, with room for the sum."""
    def treaba(cursor):
        plata = _plata_libera(cursor, id_plata_fx)
        suma = round(_num(plata["Suma"]), 2)
        candidate = []
        for r in _randuri_in_asteptare(cursor, plata["CodAngajament"]):
            if r["CodIndicator"] != plata["CodIndicator"] or _num(r["Valoare"]) < suma - TOLERANTA:
                continue
            cursor.execute("SELECT DenBene FROM FX_ORD_PART P JOIN FX_ORD_TBL T ON T.IDORDPARTP = P.IDORDPARTP "
                           " WHERE T.IDORDTBLP = %s LIMIT 1", (r["IDORDTBLP"],))
            beneficiar = (cursor.fetchone() or {}).get("DenBene") or ""
            candidate.append({"idordrecp": int(r["IDORDRECP"]), "idordtblp": int(r["IDORDTBLP"]),
                              "nr_ord": r["NrORD"], "data_ord": r["DataORD"], "beneficiar": beneficiar,
                              "cod_fiscal": r["CodFiscal"] or "", "liber": _num(r["Valoare"])})
        return {"plata": {"id_plata_fx": id_plata_fx, "suma": suma, "cod": plata["CodAngajament"],
                          "indicator": plata["CodIndicator"], "platitor": plata["platitor_nume"] or "",
                          "cui": plata["platitor_cui"] or "", "data": plata["Data"]},
                "candidate": candidate}
    return _in_tranzactie(treaba, f"linii-candidate {id_plata_fx}")


@forexe_bp.route("/api/forexe/ord/leaga-plata", methods=["POST"])
@require_session
def post_leaga_plata():
    """Attaches ONE free payment to ONE pending row by hand. Body: { "id_plata_fx": n, "idordrecp": n }."""
    date_in = request.get_json(silent=True)
    if not isinstance(date_in, dict):
        return _json_utf8({"error": "Corp JSON lipsă sau nevalid."}, 400)

    def treaba(cursor):
        try:
            id_plata = int(date_in.get("id_plata_fx") or 0)
            id_rand = int(date_in.get("idordrecp") or 0)
        except (TypeError, ValueError):
            raise Refuz("Plata și rândul ordonanțării trebuie să fie numere.")
        if id_plata <= 0 or id_rand <= 0:
            raise Refuz("Alegeți plata și rândul ordonanțării.")
        plata = _plata_libera(cursor, id_plata)
        suma = round(_num(plata["Suma"]), 2)
        rand = next((r for r in _randuri_in_asteptare(cursor, plata["CodAngajament"]) if int(r["IDORDRECP"]) == id_rand), None)
        if rand is None:
            raise Refuz("Rândul ordonanțării nu mai este în așteptare (a fost legat între timp sau nu e al aceluiași angajament).", 409)
        if rand["CodIndicator"] != plata["CodIndicator"]:
            raise Refuz(f"Plata este pe indicatorul {plata['CodIndicator']}, rândul pe {rand['CodIndicator']}.", 409)
        if _num(rand["Valoare"]) < suma - TOLERANTA:
            raise Refuz(f"Plata ({suma:.2f}) depășește ce mai are rândul ({_num(rand['Valoare']):.2f}).", 409)
        _ataseaza(cursor, rand, id_plata, suma)
        return {"id_plata_fx": id_plata, "idordrecp": id_rand, "nr_ord": rand["NrORD"], "suma": suma}
    return _in_tranzactie(treaba, "leaga-plata")


def _plata_libera(cursor, id_plata_fx: int) -> dict:
    cursor.execute(
        "SELECT P.IdPlataFX, P.CodAngajament, P.CodIndicator, P.Suma, "
        "       DATE(COALESCE(E.DataBanca, P.Data_plata)) AS Data, E.platitor_cui, E.platitor_nume "
        "  FROM FX_Plati P LEFT JOIN FX_Extrase E ON P.Referinta_TREZOR = E.Referinta "
        " WHERE P.IdPlataFX = %s LIMIT 1", (id_plata_fx,))
    plata = cursor.fetchone()
    if not plata:
        raise Refuz(f"Plata {id_plata_fx} nu există.", 404)
    cursor.execute("SELECT 1 AS x FROM FX_ORD_TBL_REC WHERE IdPlataFX = %s LIMIT 1", (id_plata_fx,))
    if cursor.fetchone():
        raise Refuz("Plata este deja legată de o ordonanțare.", 409)
    if _num(plata["Suma"]) <= 0:
        raise Refuz("Doar plățile (nu încasările) se leagă de o ordonanțare.", 409)
    return plata
