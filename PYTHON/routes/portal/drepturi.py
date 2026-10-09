"""
Rights per SECTION of the site (slice ADE9-01).

A user holds ROLES per unit (AVACONT_COMUN.Utilizatori_Roluri); a role belongs to one section
(AVACONT_COMUN.Roluri.Sectiune: AD = ADECHIT, later VR, AV) and stands for one operation
(Roluri.Operatie), e.g. AD_CITIRE = read. Everything is read from the server on each call -- never from the client.

    sections_of(email, db_name)             -> ['AD', ...] the sections the user may enter in that unit
    operations_of(email, db_name, section)  -> {'read', 'catalog', ...}
    allowed(email, db_name, section, op)    -> bool
"""
import logging

from utils.database import get_kbot_comun_connection

logger = logging.getLogger(__name__)


def _rows(sql, params):
    conn = None
    try:
        conn = get_kbot_comun_connection()
        cur = conn.cursor(dictionary=True)
        cur.execute(sql, params)
        return cur.fetchall()
    except Exception:
        logger.exception("drepturi: read failed")
        raise
    finally:
        if conn is not None and conn.is_connected():
            conn.close()


def sections_of(email, db_name):
    if not email or not db_name:
        return []
    rows = _rows(
        "SELECT DISTINCT r.Sectiune AS Sectiune FROM Utilizatori_Roluri ur "
        "JOIN Roluri r ON r.IdRol = ur.IdRol WHERE ur.UN = %s AND ur.DC = %s ORDER BY r.Sectiune",
        (email, db_name),
    )
    return [r["Sectiune"] for r in rows]


def operations_of(email, db_name, section):
    if not email or not db_name or not section:
        return set()
    rows = _rows(
        "SELECT DISTINCT r.Operatie AS Operatie FROM Utilizatori_Roluri ur "
        "JOIN Roluri r ON r.IdRol = ur.IdRol "
        "WHERE ur.UN = %s AND ur.DC = %s AND r.Sectiune = %s",
        (email, db_name, section),
    )
    return {r["Operatie"] for r in rows}


def allowed(email, db_name, section, operation):
    return operation in operations_of(email, db_name, section)
