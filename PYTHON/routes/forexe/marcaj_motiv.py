# routes/forexe/marcaj_motiv.py
"""
The reason typed in FOREXE when a reservation is saved (slice 0127) -- POST /api/forexe/marcaj/motiv, plus
the helper the DDF generation uses to offer it as the short description of the revision that follows.

WHERE IT COMES FROM. In the "Browser FOREXE" work mode the K-BOT menu inside the FOREXE page holds the
click on «Continua» of the motive window (to append the marker «(IDREV: n)», routes/forexe/marcaj.py). The
text the operator typed is in hand at that moment, so the page keeps it and hands it over with the save it
reports, together with the code of the indicator the save was about. This route stores it.

WHERE IT IS KEPT. `FX_RezervariMotiv` (sql/0127): one row per (IDREV, CodIndicator). IDREV is the number the
marker reserved for the edit session, i.e. the number of the revision to come. No history table is read:
what FX_Istoric holds for a reservation is not trusted for this (operator, 04.11.2026).

THE JOIN RULE (operator, 04.11.2026). The reasons of several saves are joined with «; » ONLY when they are
for the same angajament AND the same indicator (the row key). Reasons of different indicators are never
merged into one sentence; when a revision covers several indicators with different reasons the proposal
lists them separately, each with its indicator code (`text_motiv_revizie`).

Only the work mode of the in-app browser sends reasons. A revision whose reservations were made any other
way (the robot, FOREXE in an outside browser) has no row and keeps the old short description.
"""
import logging

from flask import request, g, current_app
import json

from routes.auth.guard import require_session
from utils.database import get_kbot_connection

from . import forexe_bp
from .marcaj import idrev_tinut
from .prelucrare_helpers import fara_marcaj

logger = logging.getLogger(__name__)

_ER_NO_SUCH_TABLE = 1146

# Desc_Scurta is VARCHAR(255) in FX_DDF_REV.
LUNGIME_DESC_SCURTA = 255
# Between the reasons of DIFFERENT indicators in a proposal; between saves of the SAME indicator it is «; ».
SEP_SALVARI = "; "
SEP_INDICATORI = " | "


def _json_utf8(payload, status):
    """A JSON response with LITERAL diacritics (ensure_ascii=False)."""
    body = json.dumps(payload, ensure_ascii=False, default=str)
    return current_app.response_class(body, status=status, mimetype="application/json")


def _curata(text) -> str:
    """The reason as the operator wrote it: no K-BOT marker, runs of blanks collapsed, trimmed."""
    brut = "" if text is None else str(text)
    return " ".join((fara_marcaj(brut) or "").split())


def _cheie_indicator(cod) -> str:
    """The way two indicator codes are compared (the page and the history may differ in blanks / case)."""
    return "".join(("" if cod is None else str(cod)).split()).upper()


def uneste_motive(vechi: str, nou: str) -> str:
    """The saved reason with a new one of the same indicator: appended, unless it is already there."""
    vechi = (vechi or "").strip()
    nou = (nou or "").strip()
    if not vechi:
        return nou
    if not nou or nou == vechi or nou in vechi.split(SEP_SALVARI):
        return vechi
    return vechi + SEP_SALVARI + nou


def text_motiv_revizie(potriviri: list) -> str:
    """
    The proposal for a revision from (CodIndicator, Motiv) pairs, one per indicator of the revision that has a
    reason. One reason, or the same reason for every indicator -> just that text. Different reasons -> each one
    with its indicator code, separated by « | » (never merged into a single sentence).
    """
    if not potriviri:
        return ""
    distincte = []
    for _, motiv in potriviri:
        if motiv not in distincte:
            distincte.append(motiv)
    if len(distincte) == 1:
        return distincte[0]
    return SEP_INDICATORI.join(f"{cod}: {motiv}" for cod, motiv in potriviri)


@forexe_bp.route("/api/forexe/marcaj/motiv", methods=["POST"])
@require_session
def post_marcaj_motiv():
    """
    Keep the reason of one reservation save.

    Body:   {"cod": "<CodAngajament>", "indicator": "<CodIndicator>", "motiv": "<text>"}
    Answer: 200 {"idrev", "cod", "indicator", "motiv"}   -- `motiv` is what is stored now (joined if repeated)
            409 {"error"}  -- the angajament has no revision number held (no marker was handed to the page)
    """
    sarcina = request.get_json(silent=True)
    if not isinstance(sarcina, dict):
        return _json_utf8({"error": "Corp JSON lipsă sau nevalid."}, 400)
    cod = str(sarcina.get("cod") or "").strip()
    indicator = str(sarcina.get("indicator") or "").strip()
    motiv = _curata(sarcina.get("motiv"))
    if not cod:
        return _json_utf8({"error": "Câmpul «cod» lipsește."}, 400)
    if not indicator:
        return _json_utf8({"error": "Câmpul «indicator» lipsește."}, 400)
    if not motiv:
        return _json_utf8({"error": "Câmpul «motiv» lipsește."}, 400)

    db_name = g.session.db_name
    conn = None
    try:
        conn = get_kbot_connection(db_name)
        conn.autocommit = False
        cursor = conn.cursor(dictionary=True)
        if not conn.in_transaction:
            conn.start_transaction()

        # The server's own held number, never one the client remembers: a stale one would file the reason
        # under a revision that is not the one to come.
        idrev = idrev_tinut(cursor, db_name, cod)
        if idrev is None:
            conn.rollback()
            return _json_utf8({"error": f"Pentru «{cod}» nu este ținut niciun număr de revizie; "
                                        "motivul nu a fost păstrat."}, 409)

        cursor.execute("SELECT IdMotiv, Motiv FROM FX_RezervariMotiv "
                       " WHERE IDREV = %s AND CodIndicator = %s FOR UPDATE", (idrev, indicator))
        existent = cursor.fetchone()
        if existent is None:
            salvat = motiv
            cursor.execute("INSERT INTO FX_RezervariMotiv (IDREV, CodAngajament, CodIndicator, Motiv) "
                           "VALUES (%s, %s, %s, %s)", (idrev, cod, indicator, salvat))
        else:
            salvat = uneste_motive(str(existent["Motiv"] or ""), motiv)
            cursor.execute("UPDATE FX_RezervariMotiv SET Motiv = %s WHERE IdMotiv = %s",
                           (salvat, int(existent["IdMotiv"])))
        conn.commit()
        logger.info("[forexe.marcaj_motiv] %s: IDREV %s, %s / %s: motiv pastrat (%d caractere)",
                    db_name, idrev, cod, indicator, len(salvat))
        return _json_utf8({"idrev": idrev, "cod": cod, "indicator": indicator, "motiv": salvat}, 200)
    except Exception as e:
        if conn is not None:
            try:
                conn.rollback()
            except Exception:
                logger.warning("[forexe.marcaj_motiv] rollback esuat", exc_info=True)
        logger.error(f"[forexe.marcaj_motiv] {cod} / {indicator}: {e}", exc_info=True)
        if getattr(e, "errno", None) == _ER_NO_SUCH_TABLE:
            return _json_utf8({"error": "Tabelul FX_RezervariMotiv nu există în această unitate "
                                        "(sql/0127 nerulat)."}, 500)
        return _json_utf8({"error": f"Eroare la păstrarea motivului: {e}"}, 500)
    finally:
        if conn is not None:
            conn.close()


def propune_motiv(cursor, dc: str, cod: str, indicatori: list) -> dict:
    """
    The reason to offer for the revision being generated for `cod`, whose lines are about the indicators
    `indicatori` (their CodIndicator values, in line order). The cursor is a DICTIONARY one.

    Returns {"text": str, "fara_potrivire": bool, "lipsa_tabel": bool}:
      * text            -- the proposal ("" = nothing to offer);
      * fara_potrivire  -- a reason is kept for the held revision number, but for none of these indicators;
      * lipsa_tabel     -- the unit does not have FX_RezervariMotiv yet (sql/0127 not carried there).
    Only the reasons of the revision number the server holds NOW are read (the session being turned into this
    revision), and only those of the indicators the revision covers.
    """
    gol = {"text": "", "fara_potrivire": False, "lipsa_tabel": False}
    idrev = idrev_tinut(cursor, dc, cod)
    if idrev is None:
        return gol
    try:
        cursor.execute("SELECT CodIndicator, Motiv FROM FX_RezervariMotiv "
                       " WHERE IDREV = %s AND CodAngajament = %s ORDER BY IdMotiv", (idrev, cod))
        pastrate = cursor.fetchall() or []
    except Exception as e:
        if getattr(e, "errno", None) == _ER_NO_SUCH_TABLE:
            logger.warning("[forexe.marcaj_motiv] FX_RezervariMotiv lipseste pe %s (sql/0127 nerulat)", dc)
            return {**gol, "lipsa_tabel": True}
        raise
    if not pastrate:
        return gol

    pe_cheie = {_cheie_indicator(r["CodIndicator"]): str(r["Motiv"] or "").strip() for r in pastrate}
    potriviri = []
    vazute = set()
    for ind in indicatori:
        cheie = _cheie_indicator(ind)
        if not cheie or cheie in vazute:
            continue
        vazute.add(cheie)
        motiv = pe_cheie.get(cheie)
        if motiv:
            potriviri.append((str(ind).strip(), motiv))
    if not potriviri:
        return {**gol, "fara_potrivire": True}
    return {**gol, "text": text_motiv_revizie(potriviri)}
