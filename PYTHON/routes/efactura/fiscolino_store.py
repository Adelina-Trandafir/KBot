"""SQL for the FISCOLINO database on AVACONT (SLICE-FS02-01)."""

import hashlib
import os
import secrets
from contextlib import contextmanager
from datetime import timedelta

import mysql.connector

from .token_store import START_LIFETIME_MINUTES, utc_now
from .tokens import EfEroare

SESSION_SECONDS = 30 * 60
SQL_FILE = "sql/SLICE-FS02-01_fiscolino_efactura.sql"


@contextmanager
def connection():
    settings = {key: os.environ.get("FISCOLINO_DB_" + key.upper(), "") for key in ("host", "user", "password")}
    missing = ["FISCOLINO_DB_" + key.upper() for key, value in settings.items() if not value]
    try:
        port = int(os.environ.get("FISCOLINO_DB_PORT", "").strip() or "3306")
    except ValueError:
        port = 0
    if not 1 <= port <= 65535:
        missing.append("FISCOLINO_DB_PORT")
    if missing:
        raise EfEroare("Lipsește configurația bazei FISCOLINO: " + ", ".join(missing) + ".", "DB_NECONFIGURAT", 503)

    try:
        conn = mysql.connector.connect(
            **settings, port=port, database="FISCOLINO", autocommit=False, time_zone="+00:00",
            connection_timeout=8, read_timeout=30, write_timeout=30,
        )
    except mysql.connector.Error as err:
        if err.errno != 1049:
            raise
        raise EfEroare(f"Baza FISCOLINO nu există. Rulați {SQL_FILE}.", "TABELE_LIPSA", 503) from err

    try:
        conn.cursor().execute("SET SESSION TRANSACTION ISOLATION LEVEL READ COMMITTED")
        yield conn
    except mysql.connector.Error as err:
        if err.errno != 1146:
            raise
        raise EfEroare(f"Lipsesc tabelele FISCOLINO. Rulați {SQL_FILE}.", "TABELE_LIPSA", 503) from err
    finally:
        # Closing an uncommitted InnoDB transaction rolls it back.
        conn.close()


def enrolled_user(cur, fingerprint):
    # An enrolled certificate may log in before its account has any unit: the units are registered from FISCOLINO.
    # A certificate counts only after the administrator validated it (VALIDAT = 1) and gave it an account (UN).
    cur.execute(
        "SELECT C.UN FROM UTILIZATORI_CERTIFICATE C "
        "WHERE C.AMPRENTA = %s AND C.ACTIV = 1 AND C.VALIDAT = 1 AND C.UN IS NOT NULL",
        (fingerprint,),
    )
    return cur.fetchone()


def enrollment_state(cur, fingerprint):
    """None = no row, DEZACTIVAT, VALIDAT or IN_ASTEPTARE (request sent, waiting for the administrator)."""
    cur.execute("SELECT ACTIV, VALIDAT, UN FROM UTILIZATORI_CERTIFICATE WHERE AMPRENTA = %s", (fingerprint,))
    row = cur.fetchone()
    if row is None:
        return None
    active, validated, username = row
    if not active:
        return "DEZACTIVAT"
    return "VALIDAT" if validated and username is not None else "IN_ASTEPTARE"


def request_enrollment(cur, cert, email, token_info):
    """Record an enrollment request for the certificate the connection carries. The account name (UN) is the requester's
    email; the administrator validates by setting VALIDAT = 1. An existing row is never overwritten."""
    state = enrollment_state(cur, cert["amprenta"])
    if state is not None:
        return state
    cur.execute(
        "INSERT IGNORE INTO UTILIZATORI_CERTIFICATE (AMPRENTA, UN, SUBIECT, EMITENT, VALID_PANA, TOKEN_INFO, CERERE_LA) "
        "VALUES (%s, %s, %s, %s, %s, %s, UTC_TIMESTAMP())",
        (cert["amprenta"], email, cert["subiect"], cert["emitent"], cert["valid_pana"], token_info),
    )
    return "IN_ASTEPTARE"


def _session_hash(token):
    return hashlib.sha256(token.encode("ascii")).digest()


def new_session(cur, username, fingerprint, valid_until):
    now = utc_now()
    deadline = min(now + timedelta(seconds=SESSION_SECONDS), valid_until) if valid_until else now + timedelta(seconds=SESSION_SECONDS)
    ttl = int((deadline - now).total_seconds())
    if ttl <= 0:
        raise EfEroare("Certificatul a expirat.", "CERT_EXPIRAT", 401)
    token = secrets.token_urlsafe(32)
    cur.execute("DELETE FROM PORTAL_SESIUNI WHERE EXPIRA_LA <= %s", (now,))
    cur.execute(
        "INSERT INTO PORTAL_SESIUNI (TOKEN_HASH, UN, AMPRENTA, EXPIRA_LA) VALUES (%s, %s, %s, %s)",
        (_session_hash(token), username, fingerprint, deadline),
    )
    return token, ttl


def session_identity(cur, token):
    cur.execute(
        "SELECT C.UN, C.AMPRENTA FROM PORTAL_SESIUNI S "
        "JOIN UTILIZATORI_CERTIFICATE C ON C.AMPRENTA = S.AMPRENTA AND C.UN = S.UN "
        "WHERE S.TOKEN_HASH = %s AND S.EXPIRA_LA > UTC_TIMESTAMP() AND C.ACTIV = 1 AND C.VALIDAT = 1",
        (_session_hash(token),),
    )
    return cur.fetchone()


def user_has_unit(cur, username, cif):
    """The unit is its CF (no other key): the account has access to it when UNITATI_UTILIZATORI holds (UN, CF)."""
    cur.execute("SELECT 1 FROM UNITATI_UTILIZATORI WHERE UN = %s AND CF = %s", (username, cif))
    return cur.fetchone() is not None


def register_unit(cur, username, cif):
    """Link a CF to the account. The first account to register a CF owns it; another account needs the administrator."""
    cur.execute("INSERT IGNORE INTO UNITATI (CF) VALUES (%s)", (cif,))
    lock_unit(cur, cif)
    cur.execute("SELECT 1 FROM UNITATI_UTILIZATORI WHERE CF = %s AND UN <> %s LIMIT 1", (cif, username))
    if cur.fetchone() is not None:
        raise EfEroare(
            "Codul fiscal este deja înregistrat pentru alt cont FISCOLINO. Cereți administratorului să-l lege și de contul dumneavoastră.",
            "CIF_ALT_CONT", 409,
        )
    cur.execute("INSERT IGNORE INTO UNITATI_UTILIZATORI (UN, CF) VALUES (%s, %s)", (username, cif))


def start(cur, username, cif, fingerprint):
    state = secrets.token_hex(16)
    cur.execute("DELETE FROM EF_TOKEN_START WHERE EXPIRA_LA <= UTC_TIMESTAMP()")
    cur.execute(
        "INSERT INTO EF_TOKEN_START (STATE, UN, CIF, AMPRENTA, EXPIRA_LA) "
        "VALUES (%s, %s, %s, %s, DATE_ADD(UTC_TIMESTAMP(), INTERVAL %s MINUTE))",
        (state, username, cif, fingerprint, START_LIFETIME_MINUTES),
    )
    return state


def spend_start(cur, state, username, cif, fingerprint):
    cur.execute(
        "DELETE FROM EF_TOKEN_START WHERE STATE = %s AND UN = %s AND CIF = %s "
        "AND AMPRENTA = %s AND EXPIRA_LA > UTC_TIMESTAMP()",
        (state, username, cif, fingerprint),
    )
    return cur.rowcount == 1


def lock_unit(cur, cif):
    # An existing unit row also serializes the first authorization, before any token row exists.
    cur.execute("SELECT CF FROM UNITATI WHERE CF = %s FOR UPDATE", (cif,))
    row = cur.fetchone()
    if row is None:
        raise EfEroare("CIF-ul unității a fost schimbat. Redeschideți editorul firmei.", "CIF_SCHIMBAT", 409)


_TOKEN_FIELDS = (
    "CUI", "AccessTokenCrypt", "RefreshTokenCrypt", "AccessExpiraLa", "RefreshExpiraLa",
    "DataAutorizare", "DataReinnoire", "CertEticheta", "UltimaEroare",
)


def token_row(cur, cif, for_update=False):
    cur.execute(
        "SELECT CIF, ACCES_CRYPT, REFRESH_CRYPT, ACCES_EXPIRA_LA, REFRESH_EXPIRA_LA, AUTORIZAT_LA, "
        "REFRESH_LA, CERT_ETICHETA, ULTIMA_EROARE FROM EF_TOKEN WHERE CIF = %s"
        + (" FOR UPDATE" if for_update else ""), (cif,),
    )
    row = cur.fetchone()
    return dict(zip(_TOKEN_FIELDS, row)) if row else None


def save_authorised(cur, cif, username, fingerprint, label, access, refresh, access_until, refresh_until):
    cur.execute(
        "INSERT INTO EF_TOKEN (CIF, ACCES_CRYPT, REFRESH_CRYPT, ACCES_EXPIRA_LA, REFRESH_EXPIRA_LA, "
        "AUTORIZAT_LA, AUTORIZAT_DE, CERT_ETICHETA, CERT_AMPRENTA) "
        "VALUES (%s, %s, %s, %s, %s, UTC_TIMESTAMP(), %s, %s, %s) "
        "ON DUPLICATE KEY UPDATE ACCES_CRYPT=VALUES(ACCES_CRYPT), REFRESH_CRYPT=VALUES(REFRESH_CRYPT), "
        "ACCES_EXPIRA_LA=VALUES(ACCES_EXPIRA_LA), REFRESH_EXPIRA_LA=VALUES(REFRESH_EXPIRA_LA), AUTORIZAT_LA=VALUES(AUTORIZAT_LA), "
        "AUTORIZAT_DE=VALUES(AUTORIZAT_DE), CERT_ETICHETA=VALUES(CERT_ETICHETA), CERT_AMPRENTA=VALUES(CERT_AMPRENTA), "
        "REFRESH_LA=NULL, ULTIMA_EROARE=NULL, ULTIMA_EROARE_LA=NULL",
        (cif, access, refresh, access_until, refresh_until, username, label, fingerprint),
    )


def save_refreshed(cur, cif, access, refresh, access_until):
    cur.execute(
        "UPDATE EF_TOKEN SET ACCES_CRYPT=%s, REFRESH_CRYPT=%s, ACCES_EXPIRA_LA=%s, "
        "REFRESH_LA=UTC_TIMESTAMP(), ULTIMA_EROARE=NULL, ULTIMA_EROARE_LA=NULL WHERE CIF=%s",
        (access, refresh, access_until, cif),
    )


def save_error(cur, cif, message, needs_authorization=False):
    cur.execute(
        "UPDATE EF_TOKEN SET ULTIMA_EROARE=%s, ULTIMA_EROARE_LA=UTC_TIMESTAMP()"
        + (", REFRESH_EXPIRA_LA=UTC_TIMESTAMP()" if needs_authorization else "") + " WHERE CIF=%s", (message[:255], cif),
    )
