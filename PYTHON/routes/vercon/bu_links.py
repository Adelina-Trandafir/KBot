"""Persisted links of a Bon transfer Unitate and its ordinary destination documents."""
import re

from .documents import _one, _scope
from .nomenclatoare import NomError

TABLE = 'VC_TransferUnitati'
LINES = 'VC_TransferUnitati_Linii'
LOCKED = 'Documentul aparține unui BU. Anulați transferul din «Transferă între subunități», în gestiunea sursă.'


def ready(cursor):
    row = _one(cursor, "SELECT COUNT(*) AS n FROM information_schema.TABLES WHERE TABLE_SCHEMA=DATABASE() "
               "AND TABLE_NAME IN (%s,%s)", (TABLE, LINES))
    return row['n'] == 2


def require(cursor):
    if not ready(cursor):
        raise NomError('BU_SCHEMA', 'Aplicați manual sql/VC14_02_TRANSFER_INTRE_SUBUNITATI.sql și sincronizați schema VERCON.', 503)


def require_optional_receiver(cursor):
    column = _one(cursor, 'SELECT IS_NULLABLE FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=DATABASE() '
                  "AND TABLE_NAME=%s AND COLUMN_NAME='PRIMITOR'", (TABLE,))
    if not column or column['IS_NULLABLE'] != 'YES':
        raise NomError('BU_SCHEMA', 'Aplicați versiunea actualizată a scriptului VC14_02_TRANSFER_INTRE_SUBUNITATI.sql '
                       'și sincronizați schema VERCON pentru transferul fără primitor.', 503)


def is_bu(number):
    return bool(re.fullmatch(r'BU [1-9][0-9]*', str(number or '')))


def reference(number):
    return 'Transferat în afara subunității · ' + number


def require_independent(cursor, ctx, family, doc_id, row):
    if family == 'bt' and is_bu(row['NRBT']):
        raise NomError('BU_LINKED', LOCKED, 409)
    marker = {'nir': ('NrFact', 'DEST_NIR'), 'bp': ('Expl', 'DEST_BP')}.get(family)
    if not marker:
        return
    field, key = marker
    raw = row.get(field) or ''
    number = raw.rsplit(' · ', 1)[-1] if family == 'bp' else raw
    if is_bu(number) and ready(cursor) and _one(
        cursor, f'SELECT 1 FROM {TABLE} WHERE ID=%s AND DEST_SUBUNITATE=%s '
        f'AND DEST_GESTIUNE=%s AND DEST_EXERCITIU=%s AND {key}=%s',
        (int(number[3:]), *_scope(ctx), doc_id),
    ):
        raise NomError('BU_LINKED', LOCKED, 409)
