# routes/auth/auth.py
"""
Login-ul aplicatiei K-BOT (token bearer opac; fara X-Api-Key).

Identitatea e dovedita prin connect-to-validate pe MariaDB cu userul+parola
operatorului; unitatile accesibile = `SHOW DATABASES` intersectat cu registrul
central CAI. La login se emite un token opac (secrets.token_urlsafe) tinut in
registrul in-memory (session_store.STORE); logout-ul il revoca instant.
Parola operatorului traieste DOAR in memoria procesului, in sesiune, pe durata
sesiunii (decizia #3/#4 din planul de auth): nu se persista, nu se logheaza.
"""
import logging

import mysql.connector
from mysql.connector import errorcode
from flask import Blueprint, request, jsonify, g

from utils.database import get_db_connection    # R5 verificat: database.py:9
from config import DB_CONFIG

from .session_store import STORE
from .guard import require_session

auth_bp = Blueprint("auth", __name__)
logger = logging.getLogger(__name__)

COMMON_DB = "AVACONT_COMUN"

# Roluri de aplicatie cunoscute (store-only; enforcement e o felie ulterioara).
KNOWN_ROLES = ("Contabil", "Administrator")

# Coloanele citite din CAI (subsetul pentru picker + contextul complet).
_CAI_COLUMNS = ("IdUnitate, DbName, NumeUnitate, AlteDetalii, "
                "Sursa, CF, CodProgram, AnDate, DC")


def _role_from_username(username):
    """
    Deriva rolul din sufixul contului (conturile sunt '<db_name>_<role>').
    db_name poate contine el insusi underscore-uri, deci ne bazam DOAR pe ultimul
    segment si numai daca este un rol cunoscut. Orice altceva -> None (login-ul
    continua, rolul e store-only). Nu ridica niciodata exceptie.
    """
    if not username:
        return None
    suffix = username.rsplit("_", 1)[-1]
    return suffix if suffix in KNOWN_ROLES else None


def connect_as_operator(username, password, db_name=None):
    """
    Deschide o conexiune MariaDB CA OPERATORUL (optional pe o baza anume).
    Succes == credentiale valide si (cu db_name) acces la baza. Apelantul
    TREBUIE sa inchida conexiunea. Nu exista fallback pe cont de serviciu —
    esecul de autentificare se propaga (model a).
    SECURITY: parola in clar e folosita doar pentru a deschide conexiunea;
    nu este niciodata logata.
    """
    kwargs = dict(
        host=DB_CONFIG["host"],
        port=DB_CONFIG.get("port", 3306),
        user=username,
        password=password,
        connection_timeout=10,
    )
    if db_name:
        kwargs["database"] = db_name
    return mysql.connector.connect(**kwargs)


def _operator_databases(op_conn):
    """Intoarce multimea bazelor de date vizibile contului operatorului."""
    cur = op_conn.cursor()
    try:
        cur.execute("SHOW DATABASES")
        return {row[0] for row in cur.fetchall()}
    finally:
        cur.close()


@auth_bp.route("/api/auth/units", methods=["POST"])
def auth_units():
    """
    Faza 1 (proba pre-login, fara token): valideaza credentialele, intoarce
    unitatile accesibile operatorului. NU emite token aici.
    Body: { "username": str, "password": str, "an": int|null }
    """
    data = request.json or {}
    username = (data.get("username") or "").strip()
    password = data.get("password") or ""
    an = data.get("an")  # filtru optional de an

    if not username or not password:
        return jsonify({"error": "Utilizator sau parola lipsa."}), 400

    op_conn = None
    admin_conn = None
    try:
        # --- valideaza credentialele conectandu-te ca operator ---
        try:
            op_conn = connect_as_operator(username, password)
        except mysql.connector.Error as db_err:
            if db_err.errno == errorcode.ER_ACCESS_DENIED_ERROR:
                return jsonify({"error": "Utilizator sau parola incorecte."}), 401
            logger.error(f"Eroare conectare operator '{username}': {db_err}")
            return jsonify({"error": "Conectare esuata la server."}), 502

        op_dbs = _operator_databases(op_conn)

        # --- intersecteaza cu CAI (conexiune admin) ---
        admin_conn = get_db_connection(COMMON_DB)
        acur = admin_conn.cursor(dictionary=True)
        try:
            if an is not None:
                acur.execute(f"SELECT {_CAI_COLUMNS} FROM CAI WHERE AnDate = %s",
                             (int(an),))
            else:
                acur.execute(f"SELECT {_CAI_COLUMNS} FROM CAI")
            all_units = acur.fetchall()
        finally:
            acur.close()

        units = [
            {
                "IdUnitate": u["IdUnitate"],
                "DbName": u["DbName"],
                "NumeUnitate": u["NumeUnitate"],
                "AlteDetalii": u["AlteDetalii"],
                "Sursa": u["Sursa"],
                "AnDate": u["AnDate"],
                "DC": u["DC"],
            }
            for u in all_units
            if u["DbName"] in op_dbs
        ]

        return jsonify({"units": units}), 200

    except Exception as e:
        logger.error(f"Eroare auth_units: {e}", exc_info=True)
        return jsonify({"error": "Eroare interna la listarea unitatilor."}), 500
    finally:
        if op_conn is not None and op_conn.is_connected():
            op_conn.close()
        if admin_conn is not None and admin_conn.is_connected():
            admin_conn.close()


@auth_bp.route("/api/auth/login", methods=["POST"])
def auth_login():
    """
    Faza 2: re-valideaza credentialele, confirma accesul la unitate, incarca
    randul CAI complet si emite token-ul opac de sesiune.
    Body: { "username", "password", "IdUnitate", "pcname" }
    Raspuns: { "Token": str, "SessionContext": {...} } — fara session_id.
    """
    data = request.json or {}
    username = (data.get("username") or "").strip()
    password = data.get("password") or ""
    pcname = (data.get("pcname") or "").strip()
    id_unitate = data.get("IdUnitate")

    if not username or not password:
        return jsonify({"error": "Utilizator sau parola lipsa."}), 400
    try:
        id_unitate = int(id_unitate)
    except (TypeError, ValueError):
        return jsonify({"error": "Unitate invalida."}), 400

    role = _role_from_username(username)  # store-only; poate fi None

    op_conn = None
    admin_conn = None
    try:
        # --- re-valideaza credentialele ---
        try:
            op_conn = connect_as_operator(username, password)
        except mysql.connector.Error as db_err:
            if db_err.errno == errorcode.ER_ACCESS_DENIED_ERROR:
                return jsonify({"error": "Utilizator sau parola incorecte."}), 401
            logger.error(f"Eroare conectare operator '{username}': {db_err}")
            return jsonify({"error": "Conectare esuata la server."}), 502

        op_dbs = _operator_databases(op_conn)

        # --- incarca randul CAI complet pentru unitatea aleasa ---
        admin_conn = get_db_connection(COMMON_DB)
        acur = admin_conn.cursor(dictionary=True)
        try:
            acur.execute(f"SELECT {_CAI_COLUMNS} FROM CAI WHERE IdUnitate = %s",
                         (id_unitate,))
            unit = acur.fetchone()
        finally:
            acur.close()

        if unit is None:
            return jsonify({"error": "Unitatea selectata nu exista."}), 404

        # --- autorizare: operatorul trebuie sa poata ajunge la baza unitatii ---
        if unit["DbName"] not in op_dbs:
            return jsonify({"error": "Nu aveti acces la unitatea selectata."}), 403

        session_context = {
            "DbName": unit["DbName"],
            "IdUnitate": unit["IdUnitate"],
            "ANL": unit["AnDate"],
            "CodProgram": unit["CodProgram"],
            "SectorSursa": unit["Sursa"],
            "CF": unit["CF"],
            "NumeUnitate": unit["NumeUnitate"],
            "Role": role,
        }

        # --- emite token-ul opac; parola ramane in sesiune (model a) ---
        token, _ = STORE.create(username, password, unit["IdUnitate"],
                                unit["DbName"], session_context, pcname)

        return jsonify({"Token": token,
                        "SessionContext": session_context}), 200

    except Exception as e:
        logger.error(f"Eroare auth_login: {e}", exc_info=True)
        return jsonify({"error": "Eroare interna la autentificare."}), 500
    finally:
        if op_conn is not None and op_conn.is_connected():
            op_conn.close()
        if admin_conn is not None and admin_conn.is_connected():
            admin_conn.close()


@auth_bp.route("/api/auth/logout", methods=["POST"])
@require_session
def auth_logout():
    """
    Revoca token-ul sesiunii curente (evacueaza si parola din memorie).
    Corp gol — token-ul din Authorization identifica sesiunea. Un token deja
    mort nu ajunge aici (guard-ul intoarce 401); clientul trateaza logout-ul
    ca best-effort si ignora rezultatul.
    """
    STORE.revoke(g.session_token)
    return jsonify({"ok": True}), 200
