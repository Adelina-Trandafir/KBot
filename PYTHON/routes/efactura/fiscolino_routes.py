"""Certificate login and OAuth endpoints for FISCOLINO on AVACONT (SLICE-FS02-01)."""

import logging
import re
from functools import wraps
from urllib.parse import urlsplit

from flask import Blueprint, Response, request
from mysql.connector import Error as DatabaseError

from routes.auth.ratelimit import LIMITER
from routes.portal.certificat import _CertError, _presented
from routes.portal.portal import TOKEN_HEADER, _fail, _ip, _json

from . import fiscolino_store as store, fiscolino_tokens as tokens
from .tokens import EfEroare

fiscolino_efactura_bp = Blueprint("fiscolino_efactura", __name__)
logger = logging.getLogger(__name__)

_TOKEN_PATH = "/api/fiscolino/efactura/token/"
_UNIT_PATH = "/api/fiscolino/efactura/unitate/"
_CERT_HOST = "cert.k-bot.ro"
_SESSION = re.compile(r"[A-Za-z0-9_-]{43}")
_CIF = re.compile(r"[0-9]{2,13}")
_STATE = re.compile(r"[0-9a-f]{32}")
_EMAIL = re.compile(r"[^@\s]+@[^@\s]+\.[^@\s]+")


@fiscolino_efactura_bp.after_request
def no_cache(response):
    response.headers["Cache-Control"] = "no-store"
    response.headers["Referrer-Policy"] = "no-referrer"
    return response


@fiscolino_efactura_bp.errorhandler(EfEroare)
def refuse(err):
    return _fail(err.reason, str(err), err.status)


@fiscolino_efactura_bp.errorhandler(500)
def failed(_err):
    return _fail("EROARE_SERVER", "Operația e-Factura nu a putut fi finalizată pe AVACONT.", 500)


@fiscolino_efactura_bp.errorhandler(DatabaseError)
def database_error(err):
    logger.error("FISCOLINO e-Factura: MariaDB a returnat eroarea %s.", err.errno)
    return failed(err)


@fiscolino_efactura_bp.route("/api/portal/certificat/fiscolino/login", methods=["POST"])
def login():
    if request.host != _CERT_HOST:
        return _fail("HOST_INVALID", "Folosiți hostul AVACONT de autentificare cu certificat.", 404)
    try:
        cert = _presented()
    except _CertError as err:
        return _fail(err.reason, str(err), err.status)

    fingerprint = cert["amprenta"]
    ip, who = _ip(), "fiscolino-cert:" + fingerprint[:16]
    if LIMITER.is_blocked(ip, who):
        return _fail("RATE_LIMITED", "Prea multe încercări nereușite. Reîncercați mai târziu.", 429)
    with store.connection() as conn:
        cur = conn.cursor()
        enrolled = store.enrolled_user(cur, fingerprint)
        if enrolled is None:
            LIMITER.record_failure(ip, who)
            if store.enrollment_state(cur, fingerprint) == "IN_ASTEPTARE":
                return _fail("CERT_IN_ASTEPTARE", "Cererea de înregistrare a certificatului așteaptă validarea administratorului.", 401)
            return _fail("CERT_NEINROLAT", "Certificatul nu este înrolat în FISCOLINO.", 401)
        token, ttl = store.new_session(cur, enrolled[0], fingerprint, cert["valid_pana"])
        conn.commit()
    LIMITER.record_success(ip, who)
    return _json({"token": token, "expires_in": ttl})


@fiscolino_efactura_bp.route("/api/portal/certificat/fiscolino/cerere", methods=["POST"])
def enrollment_request():
    """Enrollment request for the presented certificate. The server reads the certificate data from the TLS handshake;
    the client only adds an informational token description."""
    if request.host != _CERT_HOST:
        return _fail("HOST_INVALID", "Folosiți hostul AVACONT de autentificare cu certificat.", 404)
    try:
        cert = _presented()
    except _CertError as err:
        return _fail(err.reason, str(err), err.status)

    ip, who = _ip(), "fiscolino-cerere:" + cert["amprenta"][:16]
    if LIMITER.is_blocked(ip, who):
        return _fail("RATE_LIMITED", "Prea multe încercări. Reîncercați mai târziu.", 429)
    LIMITER.record_failure(ip, who)     # every request spends budget, so the endpoint cannot be used to fill the table
    data = request.get_json(silent=True)
    data = data if isinstance(data, dict) else {}
    email = data.get("email")
    email = email.strip().lower() if isinstance(email, str) else ""
    if len(email) > 255 or not _EMAIL.fullmatch(email):
        return _fail("EMAIL_INVALID", "Introduceți o adresă de email validă. Fără ea cererea nu se poate trimite.", 400)
    token = data.get("token")
    token = token.strip()[:255] if isinstance(token, str) and token.strip() else None
    with store.connection() as conn:
        state = store.request_enrollment(conn.cursor(), cert, email, token)
        conn.commit()
    logger.info("FISCOLINO: cerere de înregistrare certificat, stare %s.", state)
    return _json({"stare": state})


def _authenticated(require_unit):
    """Session check on every call; require_unit also demands that the account is linked to the CIF."""
    def decorate(fn):
        @wraps(fn)
        def dispatch():
            session = request.headers.get(TOKEN_HEADER, "")
            if not _SESSION.fullmatch(session):
                return _fail("SESIUNE_EXPIRATA", "Conectați-vă din nou cu certificatul FISCOLINO.", 401)
            cif = request.args.get("cif", "")
            if not _CIF.fullmatch(cif):
                return _fail("CIF_INVALID", "Introduceți un CIF valid.", 400)
            with store.connection() as conn:
                cur = conn.cursor()
                identity = store.session_identity(cur, session)
                if identity is None:
                    return _fail("SESIUNE_EXPIRATA", "Conectați-vă din nou cu certificatul FISCOLINO.", 401)
                username, fingerprint = identity
                if require_unit and not store.user_has_unit(cur, username, cif):
                    return _fail("CIF_INTERZIS", "Contul FISCOLINO nu are acces la CIF-ul din editor.", 403)
                return _json(fn(conn, cur, username, cif, fingerprint))
        return dispatch
    return decorate


_for_company = _authenticated(True)
_for_account = _authenticated(False)


@fiscolino_efactura_bp.route(_UNIT_PATH + "inregistreaza", methods=["POST"])
@_for_account
def unit_register(conn, cur, username, cif, fingerprint):
    store.register_unit(cur, username, cif)
    conn.commit()
    logger.info("FISCOLINO: cod fiscal înregistrat pentru un cont.")
    return {"inregistrat": True}


@fiscolino_efactura_bp.route(_TOKEN_PATH + "stare", methods=["GET"])
@_for_company
def token_status(conn, cur, username, cif, fingerprint):
    return tokens.status(conn, cur, cif)


@fiscolino_efactura_bp.route(_TOKEN_PATH + "start", methods=["POST"])
@_for_company
def token_start(conn, cur, username, cif, fingerprint):
    return tokens.start(conn, cur, username, cif, fingerprint)


@fiscolino_efactura_bp.route(_TOKEN_PATH + "cod", methods=["POST"])
@_for_company
def token_code(conn, cur, username, cif, fingerprint):
    data = request.get_json(silent=True)
    if not isinstance(data, dict):
        raise EfEroare("Corpul cererii trebuie să fie JSON.", "COD_INVALID", 400)
    state = data.get("state")
    if not isinstance(state, str) or not _STATE.fullmatch(state):
        raise EfEroare("Starea de autorizare este invalidă.", "STATE_INVALID", 400)
    label = data.get("certificat")
    answer = tokens.finish(conn, cur, username, cif, fingerprint, state, data.get("code"),
                           label if isinstance(label, str) else None)
    logger.info("FISCOLINO e-Factura: autorizare salvată.")
    return answer


@fiscolino_efactura_bp.route(_TOKEN_PATH + "refresh", methods=["POST"])
@_for_company
def token_refresh(conn, cur, username, cif, fingerprint):
    return tokens.refresh(conn, cur, cif, tokens.config(), force=True)


@fiscolino_efactura_bp.route(urlsplit(tokens.CALLBACK_URL).path, methods=["GET"])
def callback():
    # Desktop intercepts the redirect; only the authenticated /cod endpoint exchanges the code.
    return Response(status=204)
