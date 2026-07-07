# routes/auth/auth.py
"""
Login-ul aplicatiei K-BOT.

Valideaza operatorul conectandu-se la MariaDB cu userul+parola lui, deriva
unitatile accesibile din `SHOW DATABASES` intersectat cu registrul central CAI,
scrie randul de audit in FX_LoginLog si intoarce un session_id + SessionContext.
Parola trece prin server DOAR pentru a deschide conexiunea de validare; nu se
stocheaza, nu se hash-uieste, nu se logheaza nicaieri.
"""
import logging

import mysql.connector
from mysql.connector import errorcode
from flask import Blueprint, request, jsonify

from utils.security import require_api_key      # R5 verificat: security.py:8
from utils.database import get_db_connection    # R5 verificat: database.py:9
from config import DB_CONFIG

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


def _connect_as_operator(username, password):
    """
    Valideaza credentialele operatorului deschizand o conexiune MariaDB la nivel
    de server CA ACEL OPERATOR, FARA baza de date implicita selectata. Succes ==
    credentiale valide. Apelantul TREBUIE sa inchida conexiunea.
    SECURITY: parola in clar e folosita doar pentru a deschide aceasta conexiune;
    nu este niciodata stocata sau logata.
    """
    return mysql.connector.connect(
        host=DB_CONFIG["host"],
        port=DB_CONFIG.get("port", 3306),
        user=username,
        password=password,
        connection_timeout=10,
    )


def _operator_databases(op_conn):
    """Intoarce multimea bazelor de date vizibile contului operatorului."""
    cur = op_conn.cursor()
    try:
        cur.execute("SHOW DATABASES")
        return {row[0] for row in cur.fetchall()}
    finally:
        cur.close()


@auth_bp.route("/api/auth/units", methods=["POST"])
@require_api_key
def auth_units():
    """
    Faza 1: valideaza credentialele, intoarce unitatile accesibile operatorului.
    Body: { "username": str, "password": str, "an": int|null }
    Nu se scrie rand de audit aici (nu s-a ales inca o unitate).
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
            op_conn = _connect_as_operator(username, password)
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
@require_api_key
def auth_login():
    """
    Faza 2: re-valideaza, confirma accesul la unitate, incarca randul CAI complet,
    scrie randul de audit (cu rol), intoarce session_id + SessionContext.
    Body: { "username", "password", "IdUnitate", "pcname" }
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
            op_conn = _connect_as_operator(username, password)
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

        # --- scrie randul de audit (conexiune admin) ---
        ip_address = request.remote_addr
        wcur = admin_conn.cursor()
        try:
            wcur.execute(
                "INSERT INTO FX_LoginLog "
                "(Username, Role, IdUnitate, DbName, IpAddress, PcName, LoginTime, LogoutTime) "
                "VALUES (%s, %s, %s, %s, %s, %s, NOW(), NULL)",
                (username, role, unit["IdUnitate"], unit["DbName"], ip_address, pcname),
            )
            session_id = wcur.lastrowid
            admin_conn.commit()
        finally:
            wcur.close()

        # lastrowid == 0 inseamna ca IdLog nu e AUTO_INCREMENT -> eroare dura, fara pass silentios.
        if not session_id:
            return jsonify({"error": "Nu s-a putut inregistra sesiunea."}), 500

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
        return jsonify({"session_id": session_id,
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
@require_api_key
def auth_logout():
    """
    Stampileaza LogoutTime pe randul de sesiune deschis.
    Body: { "session_id": int }
    """
    data = request.json or {}
    session_id = data.get("session_id")
    try:
        session_id = int(session_id)
    except (TypeError, ValueError):
        return jsonify({"error": "Sesiune invalida."}), 400

    admin_conn = None
    try:
        admin_conn = get_db_connection(COMMON_DB)
        cur = admin_conn.cursor()
        try:
            cur.execute(
                "UPDATE FX_LoginLog SET LogoutTime = NOW() "
                "WHERE IdLog = %s AND LogoutTime IS NULL",
                (session_id,),
            )
            stamped = cur.rowcount
            admin_conn.commit()
        finally:
            cur.close()
        return jsonify({"status": "ok", "stamped": stamped}), 200
    except Exception as e:
        logger.error(f"Eroare auth_logout: {e}", exc_info=True)
        return jsonify({"error": "Eroare interna la deconectare."}), 500
    finally:
        if admin_conn is not None and admin_conn.is_connected():
            admin_conn.close()
