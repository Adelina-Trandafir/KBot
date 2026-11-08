# routes/forexe/receptii_observate.py
"""
Facts about a reception that K-BOT SAW happen in the FOREXE browser (slices 0076-01, 0076-02).

POST /api/forexe/receptii/data-schimbata -- the operator changed a reception's DATE in the
FOREXE page inside K-BOT and the page confirmed the save.
POST /api/forexe/receptii/stearsa -- the operator DELETED a reception there (slice 0076-02):
the stored reception gets Sters = 1 plus StersObservatLa / StersObservatDe (sql/0076_02_sursa.sql).

WHY THIS EXISTS. Step 4b matches the reception list of a download against `FX_Receptii_R` on
the DAY (`DATE(DataR)`, F25/F30). A reception whose date changed on the site therefore finds
no stored reception on its new day: 4b inserts a NEW `FX_Receptii_R` (with its `RHR`) and the
old one keeps the chain of snapshots -- the chain of receptions is torn in two (case of
06.11.2026, a date changed outside K-BOT). When the change is made INSIDE K-BOT the page
tells us both dates (`ForexeWatch.js`, `rowDate` / `formDate`), so the stored reception is
moved to the new date BEFORE the download that follows; 4b then finds it.

WHY NOT THROUGH THE INGEST PACKAGE. The date change is a fact that already happened on the
site, not a decision of the operator. The two-phase ingest rolls back and can be abandoned;
a fact must not be lost with it. So it is written at once, in a transaction of its own, and
the client keeps it on disk until this route has answered (ObservatiiReceptiiStore).

THE REUSE. Candidates are read with the very SQL step 4b uses (`_R_CANDIDATI_SQL`), so "which
reception is on that day" has one definition. The CHOICE among several is stricter than
`_potriveste_ziua` on purpose: that function always hands back SOME legacy candidate (a
download can afford a wrong guess that the next one repairs); here a wrong pick moves a
date on a reception that is not the one the operator edited, silently. Several candidates
that cannot be told apart refuse (409) and write nothing.
"""
import json
import logging
from datetime import date, datetime

from flask import request, g, current_app

from routes.auth.guard import require_session
from utils.database import get_kbot_connection

from . import forexe_bp
from .prelucrare_pasi import _R_CANDIDATI_SQL, _intreg_optional

logger = logging.getLogger(__name__)

# The page writes dates as dd/MM/yyyy (WorkflowCatalog.DataReceptieFormat).
_FORMAT_DATA = "%d/%m/%Y"

# A reception on the NEW day with this sum: the "already done" check below.
_R_PE_ZI_SQL = (
    "SELECT IDRR, DataR, SumaAntet, RangZi FROM FX_Receptii_R "
    "WHERE CodAngajament = %s AND Sters = 0 AND DATE(DataR) = %s"
)
# RangZi = NULL: the rank belonged to the OLD day. NULL is the "row written before the
# column" case, which `_potriveste_ziua` matches on exact value and re-stamps at the next
# download -- no second rule for it.
_R_MUTA_DATA_SQL = "UPDATE FX_Receptii_R SET DataR = %s, RangZi = NULL WHERE IDRR = %s"


def _json_utf8(payload, status):
    """A JSON response with LITERAL diacritics (ensure_ascii=False)."""
    body = json.dumps(payload, ensure_ascii=False, default=str)
    return current_app.response_class(body, status=status, mimetype="application/json")


def _data(text, nume: str) -> date:
    """dd/MM/yyyy -> date; a clear ValueError naming the field otherwise."""
    try:
        return datetime.strptime(str(text or "").strip(), _FORMAT_DATA).date()
    except ValueError:
        raise ValueError(f"Câmpul «{nume}» nu e o dată zz/ll/aaaa: «{text}».")


def _numar(valoare) -> float:
    """The sum the page read from the row (Romanian or plain number), as a float."""
    from .prelucrare_helpers import parse_loose_number
    return parse_loose_number(str(valoare if valoare is not None else "0"))


def alege_receptia(candidati: list, suma: float, rang):
    """
    The ONE stored reception a FOREXE row of that day is, or None when it cannot be said.

    Returns (reception or None, reason). In order:
      1. a single candidate on the day -> it (nothing else it could be);
      2. exactly one candidate with the row's exact sum -> it;
      3. several with that sum: the one whose stored `RangZi` equals the row's rank;
      4. otherwise ambiguous -> (None, reason).
    """
    def val(x) -> float:
        return round(float(x or 0), 2)

    if len(candidati) == 1:
        return candidati[0], "singura recepție a zilei"
    la_suma = [c for c in candidati if val(c.get("SumaAntet")) == val(suma)]
    if len(la_suma) == 1:
        return la_suma[0], "singura recepție a zilei cu această valoare"
    if len(la_suma) > 1 and rang is not None:
        dupa_rang = [c for c in la_suma if _intreg_optional(c.get("RangZi")) == rang]
        if len(dupa_rang) == 1:
            return dupa_rang[0], "după rangul din zi"
    return None, ("mai multe recepții în ziua veche nu se pot deosebi "
                  f"({len(candidati)} în zi, {len(la_suma)} cu valoarea {val(suma):.2f})")


# Slice 0076-02: a reception already marked deleted by an observation, on that day (idempotence).
_R_STERSE_OBSERVATE_SQL = (
    "SELECT IDRR, DataR, SumaAntet, RangZi FROM FX_Receptii_R "
    "WHERE CodAngajament = %s AND Sters = 1 AND StersObservatLa IS NOT NULL AND DATE(DataR) = %s"
)
# `Sters = 1` AND the sign that FOREXE proved it. The association windows leave a Sters that carries
# `StersObservatLa` alone (asociere.py / prelucrare_asociere.py: the two «Sters = 0» statements).
_R_MARCHEAZA_STEARSA_OBSERVATA_SQL = (
    "UPDATE FX_Receptii_R SET Sters = 1, StersObservatLa = NOW(), StersObservatDe = %s WHERE IDRR = %s"
)


@forexe_bp.route("/api/forexe/receptii/stearsa", methods=["POST"])
@require_session
def post_receptie_stearsa():
    """Mark a stored reception deleted because the operator deleted it in the FOREXE page.

    Body:   {"cod", "data_veche" (zz/ll/aaaa: the row's date), "suma_veche", "rang"}
            (`data_noua` is ignored)
    Answer: 200 {"cod", "idrr", "data_veche", "deja": bool}
            409 {"error": ...} -- nothing found / cannot be told apart; NOTHING written
            400 {"error": ...} -- bad body
    The history row «Stergere receptie» that follows is placed on this reception by the ingest
    (prelucrare_asociere.pas4c_automat).
    """
    sarcina = request.get_json(silent=True)
    if not isinstance(sarcina, dict):
        return _json_utf8({"error": "Corp JSON lipsă sau nevalid."}, 400)
    cod = str(sarcina.get("cod") or "").strip()
    if not cod:
        return _json_utf8({"error": "Câmpul «cod» lipsește."}, 400)
    try:
        data_veche = _data(sarcina.get("data_veche"), "data_veche")
        suma = _numar(sarcina.get("suma_veche"))
    except ValueError as e:
        return _json_utf8({"error": str(e)}, 400)
    rang = _intreg_optional(sarcina.get("rang"))

    db_name = g.session.db_name
    utilizator = (getattr(g.session, "username", "") or "")[:100]
    conn = None
    try:
        conn = get_kbot_connection(db_name)
        conn.autocommit = False
        cursor = conn.cursor(dictionary=True)
        if not conn.in_transaction:
            conn.start_transaction()

        cursor.execute(_R_CANDIDATI_SQL, (cod, data_veche))
        candidati = cursor.fetchall()
        gasita, motiv = alege_receptia(candidati, suma, rang) if candidati else (None, "")

        if gasita is None:
            # Idempotence: a repeat finds nothing alive on that day and exactly one reception
            # already marked deleted by an observation, with this sum.
            cursor.execute(_R_STERSE_OBSERVATE_SQL, (cod, data_veche))
            deja = [c for c in cursor.fetchall()
                    if round(float(c.get("SumaAntet") or 0), 2) == round(suma, 2)]
            conn.rollback()
            if not candidati and len(deja) == 1:
                logger.info("[forexe.receptii] %s %s: recepția %s din %s era deja marcată ștearsă",
                            db_name, cod, deja[0]["IDRR"], data_veche)
                return _json_utf8({"cod": cod, "idrr": int(deja[0]["IDRR"]),
                                   "data_veche": str(data_veche), "deja": True}, 200)
            if not candidati:
                text = (f"Nu găsesc în K-BOT nicio recepție a angajamentului «{cod}» din "
                        f"{data_veche.strftime('%d.%m.%Y')}.")
            else:
                text = (f"Recepția ștearsă din {data_veche.strftime('%d.%m.%Y')} nu poate fi "
                        f"deosebită de celelalte din aceeași zi: {motiv}.")
            logger.warning("[forexe.receptii] %s %s: ștergerea din %s REFUZATĂ: %s",
                           db_name, cod, data_veche, text)
            return _json_utf8({"error": text}, 409)

        idrr = int(gasita["IDRR"])
        cursor.execute(_R_MARCHEAZA_STEARSA_OBSERVATA_SQL, (utilizator, idrr))
        conn.commit()
        logger.info("[forexe.receptii] %s %s: recepția %s din %s marcată ștearsă (%s)",
                    db_name, cod, idrr, data_veche, motiv)
        return _json_utf8({"cod": cod, "idrr": idrr, "data_veche": str(data_veche),
                           "deja": False}, 200)
    except Exception as e:
        if conn is not None:
            try:
                conn.rollback()
            except Exception:
                logger.warning("[forexe.receptii] rollback esuat", exc_info=True)
        logger.error(f"[forexe.receptii] stearsa {cod}: {e}", exc_info=True)
        return _json_utf8({"error": f"Eroare la marcarea recepției ca ștearsă: {e}"}, 500)
    finally:
        if conn is not None:
            conn.close()


@forexe_bp.route("/api/forexe/receptii/data-schimbata", methods=["POST"])
@require_session
def post_receptie_data_schimbata():
    """Move a stored reception to the date the operator gave it in the FOREXE page.

    Body:   {"cod", "data_veche" (zz/ll/aaaa), "data_noua" (zz/ll/aaaa),
             "suma_veche" (text or number), "rang" (int or null: rank of the row among the
             rows with the old date, in list order)}
    Answer: 200 {"cod", "idrr", "data_veche", "data_noua", "deja": bool}
            409 {"error": ...} -- nothing found / cannot be told apart; NOTHING written
            400 {"error": ...} -- bad body
    """
    sarcina = request.get_json(silent=True)
    if not isinstance(sarcina, dict):
        return _json_utf8({"error": "Corp JSON lipsă sau nevalid."}, 400)
    cod = str(sarcina.get("cod") or "").strip()
    if not cod:
        return _json_utf8({"error": "Câmpul «cod» lipsește."}, 400)
    try:
        data_veche = _data(sarcina.get("data_veche"), "data_veche")
        data_noua = _data(sarcina.get("data_noua"), "data_noua")
        suma = _numar(sarcina.get("suma_veche"))
    except ValueError as e:
        return _json_utf8({"error": str(e)}, 400)
    rang = _intreg_optional(sarcina.get("rang"))
    if data_veche == data_noua:
        return _json_utf8({"error": "Data veche și data nouă sunt aceleași."}, 400)

    db_name = g.session.db_name
    conn = None
    try:
        conn = get_kbot_connection(db_name)
        conn.autocommit = False
        cursor = conn.cursor(dictionary=True)
        if not conn.in_transaction:
            conn.start_transaction()

        cursor.execute(_R_CANDIDATI_SQL, (cod, data_veche))
        candidati = cursor.fetchall()
        gasita, motiv = alege_receptia(candidati, suma, rang) if candidati else (None, "")

        if gasita is None:
            # Idempotence: a repeat of a call that already worked (the client keeps the
            # fact until it is answered) finds nothing on the old day and exactly one
            # reception with this sum on the new one.
            cursor.execute(_R_PE_ZI_SQL, (cod, data_noua))
            pe_noua = [c for c in cursor.fetchall()
                       if round(float(c.get("SumaAntet") or 0), 2) == round(suma, 2)]
            if not candidati and len(pe_noua) == 1:
                conn.rollback()
                logger.info("[forexe.receptii] %s %s: data %s -> %s era deja mutată (IDRR %s)",
                            db_name, cod, data_veche, data_noua, pe_noua[0]["IDRR"])
                return _json_utf8({"cod": cod, "idrr": int(pe_noua[0]["IDRR"]),
                                   "data_veche": str(data_veche), "data_noua": str(data_noua),
                                   "deja": True}, 200)
            conn.rollback()
            if not candidati:
                text = (f"Nu găsesc în K-BOT nicio recepție a angajamentului «{cod}» din "
                        f"{data_veche.strftime('%d.%m.%Y')}.")
            else:
                text = (f"Recepția din {data_veche.strftime('%d.%m.%Y')} nu poate fi "
                        f"deosebită de celelalte din aceeași zi: {motiv}.")
            logger.warning("[forexe.receptii] %s %s: data %s -> %s REFUZATĂ: %s",
                           db_name, cod, data_veche, data_noua, text)
            return _json_utf8({"error": text}, 409)

        idrr = int(gasita["IDRR"])
        cursor.execute(_R_MUTA_DATA_SQL, (data_noua, idrr))
        conn.commit()
        logger.info("[forexe.receptii] %s %s: recepția %s mutată de pe %s pe %s (%s)",
                    db_name, cod, idrr, data_veche, data_noua, motiv)
        return _json_utf8({"cod": cod, "idrr": idrr, "data_veche": str(data_veche),
                           "data_noua": str(data_noua), "deja": False}, 200)
    except Exception as e:
        if conn is not None:
            try:
                conn.rollback()
            except Exception:
                logger.warning("[forexe.receptii] rollback esuat", exc_info=True)
        logger.error(f"[forexe.receptii] data-schimbata {cod}: {e}", exc_info=True)
        return _json_utf8({"error": f"Eroare la mutarea datei recepției: {e}"}, 500)
    finally:
        if conn is not None:
            conn.close()
