# routes/inregistrare/inregistrare.py
"""
The first three calls of the self-service registration page (slice 0075-01,
plan 5.2 and 5.3).

    POST /api/inregistrare/anaf      {cf}     -> opens a registration, pre-fills it
    POST /api/inregistrare/cod       {email}  -> mails a six-digit code
    POST /api/inregistrare/verifica  {cod}    -> proves the applicant reads that inbox

Everything here runs BEFORE anyone is authenticated, on a page anybody on the
internet can open. So, per the plan: the rate limiter on every route, parameterized
SQL only, a reason code beside every Romanian message, and no exception swallowed --
each one is logged where it happens and reported honestly to the caller.

THE TOKEN. `/anaf` mints it; the two routes after it want it back in the
`X-Registration-Token` header. A header rather than the body because 5.4 (`/nume`)
and the later nomenclator calls are GETs, and one way of carrying it beats two.
Where the registration lives and why it starts at the ANAF lookup: routes/
inregistrare/store.py.

THE CODE. Same machinery as the password change of slice 0072 (auth.py): six digits
from `secrets`, only the SHA-256 hash stored, `hmac.compare_digest` to compare, ten
minutes, five wrong tries and the code is thrown away. The one difference is where
it is kept -- a note on the registration token instead of a note on a session.

WHAT THE TWO DATABASE CHECKS ARE FOR. `/anaf` refuses a fiscal code that already has
a database (decision D11): the applicant is an existing customer and needs access,
not a second database. `/cod` refuses an e-mail that is already a MariaDB account or
already appears in `Unitati_Utilizatori`, and says so in words that do not tell a
stranger which of the two it was. That the service account can read `mysql.user` on
the K-BOT server was checked on the machine, 22.09.2026: it can.
"""
import hashlib
import hmac
import json
import logging
import secrets
import time

import mysql.connector
from flask import Blueprint, current_app, request

from routes.auth import mailer
from routes.auth.ratelimit import LIMITER
from utils.database import get_kbot_comun_connection

from . import anaf as anaf_client
from . import store

logger = logging.getLogger(__name__)

inregistrare_bp = Blueprint("inregistrare", __name__)

# The header the page sends back on every call after /anaf.
TOKEN_HEADER = "X-Registration-Token"

# Life of one six-digit code, and how many wrong tries it survives (slice 0072).
CODE_TTL = 10 * 60
CODE_MAX_ATTEMPTS = 5

# `Unitati_Utilizatori.UN` is varchar(80) and `sql_mode` has STRICT_TRANS_TABLES, so
# a longer address would be error 1406 at the far end of the wizard. Refused here,
# on the screen where the applicant can still do something about it.
EMAIL_MAX_LENGTH = 80

# Sentences used by more than one route. Everything the applicant reads is Romanian
# with literal diacritics; the reason codes beside them are ASCII and stable.
_MSG_RATE_LIMITED = "Prea multe încercări. Așteptați câteva minute și reluați."
_MSG_TOKEN = "Sesiunea de înregistrare a expirat. Reluați de la codul fiscal."
_MSG_MAIL_NOT_CONFIGURED = (
    "Trimiterea e-mailului nu este configurată pe server. Contactați-ne."
)


def _json(payload, status):
    """JSON with LITERAL diacritics, whatever the app-wide setting is."""
    body = json.dumps(payload, ensure_ascii=False)
    return current_app.response_class(body, status=status, mimetype="application/json")


def _fail(reason, message, status):
    return _json({"error": message, "reason": reason}, status)


def _ip():
    # ProxyFix is active in main.py, so this is the real client address and the
    # limiter's per-IP bucket is per client rather than per nginx.
    return request.remote_addr


# ---------------------------------------------------------------------------
# POST /api/inregistrare/anaf   {cf}
# Step 1. Refuses a fiscal code that already has a database, asks ANAF about the
# rest, and opens the registration around the answer.
# ---------------------------------------------------------------------------
@inregistrare_bp.route("/api/inregistrare/anaf", methods=["POST"])
def inregistrare_anaf():
    data = request.get_json(silent=True) or {}
    cf = anaf_client.normalize_cf(data.get("cf"))
    if not anaf_client.is_valid_cf(cf):
        return _fail(
            "CF_INVALID",
            "Codul fiscal trebuie să conțină între "
            f"{anaf_client.MIN_CF_DIGITS} și {anaf_client.MAX_CF_DIGITS} cifre.",
            400,
        )

    ip = _ip()
    if LIMITER.is_blocked(ip, cf):
        return _fail("RATE_LIMITED", _MSG_RATE_LIMITED, 429)

    try:
        taken = _cf_has_database(cf)
    except mysql.connector.Error as err:
        logger.error("inregistrare_anaf CF check failed for %s: %s", cf, err)
        return _fail(
            "DB_ERROR",
            "Verificarea codului fiscal nu a putut fi făcută. Reîncercați mai târziu.",
            500,
        )
    if taken:
        # D11. Said plainly, because it is good news for the applicant: the database
        # exists, what they need is access to it.
        return _fail(
            "CF_EXISTS",
            "Pentru acest cod fiscal există deja o bază de date. "
            "Contactați-ne pentru acces.",
            409,
        )

    try:
        fields = anaf_client.lookup(cf)
    except anaf_client.AnafNotFound:
        LIMITER.record_failure(ip, cf)
        return _fail(
            "ANAF_NOT_FOUND",
            "Codul fiscal nu a fost găsit în baza de date ANAF.",
            404,
        )
    except anaf_client.AnafIncomplete:
        return _fail(
            "ANAF_INCOMPLETE",
            "Răspunsul ANAF nu conține denumirea unității. Contactați-ne.",
            502,
        )
    except anaf_client.AnafUnavailable:
        return _fail(
            "ANAF_UNAVAILABLE",
            "Serverul ANAF a generat o eroare. Reîncercați mai târziu.",
            502,
        )
    LIMITER.record_success(ip, cf)

    # The applicant may have come back to this screen to look up a different code.
    # The registration they had is finished with, so it goes rather than lingering
    # for the rest of its half hour with a proven e-mail attached to it.
    store.discard((request.headers.get(TOKEN_HEADER) or "").strip())

    token, expires_in = store.create(cf, fields)
    logger.info("registration opened for CF %s from %s", cf, ip)
    return _json(
        {"token": token, "expires_in": expires_in, "cf": cf, "unitate": fields}, 200
    )


# ---------------------------------------------------------------------------
# POST /api/inregistrare/cod   {email}
# Step 2a. Attaches an e-mail to the registration and mails it a code.
# ---------------------------------------------------------------------------
@inregistrare_bp.route("/api/inregistrare/cod", methods=["POST"])
def inregistrare_cod():
    token, registration, refusal = _registration()
    if refusal is not None:
        return refusal

    body = request.get_json(silent=True) or {}
    email = (body.get("email") or "").strip().lower()
    if not _is_email(email):
        return _fail("EMAIL_INVALID", "Introduceți o adresă de e-mail validă.", 400)
    if len(email) > EMAIL_MAX_LENGTH:
        return _fail(
            "EMAIL_TOO_LONG",
            f"Adresa de e-mail nu poate depăși {EMAIL_MAX_LENGTH} de caractere.",
            400,
        )

    ip = _ip()
    if LIMITER.is_blocked(ip, email):
        return _fail("RATE_LIMITED", _MSG_RATE_LIMITED, 429)

    # Said before anything else is spent: a code nobody can receive is not a code.
    if not mailer.is_configured():
        return _fail("MAIL_NOT_CONFIGURED", _MSG_MAIL_NOT_CONFIGURED, 503)

    try:
        taken = _email_taken(email)
    except mysql.connector.Error as err:
        logger.error(
            "inregistrare_cod e-mail check failed for %s: %s",
            mailer.mask_address(email), err,
        )
        return _fail(
            "DB_ERROR",
            "Verificarea adresei nu a putut fi făcută. Reîncercați mai târziu.",
            500,
        )
    if taken:
        # Deliberately vague: a stranger must not learn from this page whether an
        # address is a MariaDB account, a K-BOT user, or neither.
        return _fail(
            "EMAIL_TAKEN",
            "Această adresă de e-mail nu poate fi folosită. "
            "Folosiți altă adresă sau contactați-ne.",
            409,
        )

    code = f"{secrets.randbelow(1_000_000):06d}"
    try:
        mailer.send_registration_code(email, code, CODE_TTL // 60)
    except mailer.MailNotConfigured:
        return _fail("MAIL_NOT_CONFIGURED", _MSG_MAIL_NOT_CONFIGURED, 503)
    except Exception as err:            # smtplib, socket: nothing arrived, say so
        logger.error(
            "registration code mail failed for %s: %s",
            mailer.mask_address(email), err,
        )
        return _fail(
            "MAIL_FAILED",
            "E-mailul cu codul nu a putut fi trimis. Reîncercați mai târziu.",
            502,
        )

    # A fresh code replaces the previous one, and a change of address un-verifies the
    # registration: whoever proved the old inbox proved nothing about this one.
    now = time.time()
    registration["email"] = email
    registration["code_hash"] = _hash_code(code)
    registration["code_attempts"] = 0
    registration["code_expires_at"] = now + CODE_TTL
    registration["verified"] = False
    left = store.save(token, registration)
    if left <= 0:
        return _fail("TOKEN_EXPIRED", _MSG_TOKEN, 401)

    logger.info("registration code sent to %s", mailer.mask_address(email))
    # The honest number: a code cannot outlive the registration that carries it.
    return _json(
        {"email_masked": mailer.mask_address(email),
         "expires_in": min(CODE_TTL, left)},
        200,
    )


# ---------------------------------------------------------------------------
# POST /api/inregistrare/verifica   {cod}
# Step 2b. The code back from the inbox.
# ---------------------------------------------------------------------------
@inregistrare_bp.route("/api/inregistrare/verifica", methods=["POST"])
def inregistrare_verifica():
    token, registration, refusal = _registration()
    if refusal is not None:
        return refusal

    body = request.get_json(silent=True) or {}
    code = (body.get("cod") or "").strip()
    if not code:
        return _fail("CODE_ABSENT", "Introduceți codul primit pe e-mail.", 400)

    stored = registration.get("code_hash") or ""
    if not stored:
        return _fail("CODE_NOT_REQUESTED", "Cereți întâi un cod de confirmare.", 400)

    if time.time() >= float(registration.get("code_expires_at", 0)):
        _forget_code(token, registration)
        return _fail("CODE_EXPIRED", "Codul a expirat. Cereți un cod nou.", 400)

    email = registration.get("email") or ""
    ip = _ip()

    if not hmac.compare_digest(stored, _hash_code(code)):
        attempts = int(registration.get("code_attempts", 0)) + 1
        LIMITER.record_failure(ip, email)
        if attempts >= CODE_MAX_ATTEMPTS:
            _forget_code(token, registration)
            return _fail(
                "CODE_ATTEMPTS_EXHAUSTED",
                "Prea multe coduri greșite. Cereți un cod nou.",
                400,
            )
        registration["code_attempts"] = attempts
        store.save(token, registration)
        return _fail("CODE_WRONG", "Codul de confirmare este greșit.", 400)

    # Right. The code is spent, the address is proven, the registration goes on.
    registration["verified"] = True
    registration["code_hash"] = None
    registration["code_attempts"] = 0
    registration["code_expires_at"] = 0.0
    left = store.save(token, registration)
    LIMITER.record_success(ip, email)
    logger.info("registration verified for %s", mailer.mask_address(email))
    return _json(
        {"ok": True,
         "email_masked": mailer.mask_address(email),
         "expires_in": left},
        200,
    )


# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------
def _registration():
    """
    `(token, registration, None)` for a live registration, or
    `(None, None, <401 response>)`. Both routes after /anaf start with this line.
    """
    token = (request.headers.get(TOKEN_HEADER) or "").strip()
    if not token:
        return None, None, _fail("TOKEN_ABSENT", _MSG_TOKEN, 401)
    registration = store.read(token)
    if registration is None:
        return None, None, _fail("TOKEN_UNKNOWN", _MSG_TOKEN, 401)
    return token, registration, None


def _forget_code(token, registration):
    """Throws the code away and keeps the registration -- a new one can be asked for."""
    registration["code_hash"] = None
    registration["code_attempts"] = 0
    registration["code_expires_at"] = 0.0
    store.save(token, registration)


def _hash_code(code) -> str:
    # Six digits with a ten-minute life: a plain SHA-256 keeps it out of the store in
    # clear, and compare_digest keeps the comparison constant-time. Same as auth.py.
    return hashlib.sha256(code.encode("utf-8")).hexdigest()


def _is_email(text) -> bool:
    """
    Shape only: one `@`, something on the left, a dotted domain on the right, no
    whitespace. The real proof is the code that lands in the inbox.
    """
    if not text or any(ch.isspace() for ch in text):
        return False
    local, sep, domain = text.partition("@")
    if not sep or not local or not domain:
        return False
    if "@" in domain:
        return False
    return "." in domain and not domain.startswith(".") and not domain.endswith(".")


def _cf_has_database(cf):
    """
    True when this fiscal code already owns a unit database (D11).

    `CAI` and `AVACONT_COMUN.Unitati` are both consulted: the plan's finding F0
    showed the two lists are not the same, and either one is reason enough to refuse.
    Codes are stored both bare and with the `RO` prefix, so both spellings are asked
    for -- that keeps the comparison on a plain indexed column instead of wrapping it
    in REPLACE().
    """
    spellings = (cf, "RO" + cf)
    conn = get_kbot_comun_connection()
    try:
        # buffered: two queries on one cursor, and an unread LIMIT 1 result would
        # make the second execute() raise "Unread result found".
        cur = conn.cursor(buffered=True)
        cur.execute("SELECT 1 FROM CAI WHERE CF IN (%s, %s) LIMIT 1", spellings)
        if cur.fetchone() is not None:
            return True
        cur.execute("SELECT 1 FROM Unitati WHERE CF IN (%s, %s) LIMIT 1", spellings)
        return cur.fetchone() is not None
    finally:
        if conn.is_connected():
            conn.close()


def _email_taken(email):
    """
    True when the address is already a K-BOT user or already a MariaDB account.

    Both matter, for different reasons: a row in `Unitati_Utilizatori` means the
    person already has access to some unit, and an account in `mysql.user` means
    step 7 of the provisioning job would fail at `CREATE USER` after the database was
    already built. Better refused on the first screen.

    That the service account can read `mysql.user` here was checked on the K-BOT
    server on 22.09.2026: it can.
    """
    conn = get_kbot_comun_connection()
    try:
        cur = conn.cursor(buffered=True)
        cur.execute("SELECT 1 FROM Unitati_Utilizatori WHERE UN = %s LIMIT 1", (email,))
        if cur.fetchone() is not None:
            return True
        cur.execute("SELECT 1 FROM mysql.user WHERE User = %s LIMIT 1", (email,))
        return cur.fetchone() is not None
    finally:
        if conn.is_connected():
            conn.close()
