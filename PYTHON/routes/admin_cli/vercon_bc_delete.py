"""SLICE-ME01-31: preview and permanent deletion of one standalone consumption bon."""
import re
from functools import partial

from routes.admin_cli.admin_common import COMMON_DB, AdminError, _exec, _journal, _rows

DELETE = 'vercon_bc_sterge'
PREVIEW = 'vercon_bc_analiza_stergerii'
TITLE = 'Șterge definitiv un bon de consum'
DELETED_TITLE = 'Bon de consum șters'
KEY = ('SubunitId', 'GestiuneId', 'ExercitiuId', 'IdBonConsum')
SCOPE = 'SubunitId=%s AND GestiuneId=%s AND ExercitiuId=%s'
BON = SCOPE + ' AND IdBonConsum=%s'
HEADERS = (
    'SELECT h.*, s.Name AS subunit_name, g.Name AS warehouse_name, g.Type AS warehouse_type, g.Scara AS warehouse_scale, '
    'e.An AS exercise_year, e.Closed AS exercise_closed, m.Masina AS account_name, c.Denumire AS receiver_name '
    'FROM VC_Iesiri_H h LEFT JOIN VC_Subunitati s ON s.SubunitId=h.SubunitId '
    'LEFT JOIN VC_Gestiuni g ON g.SubunitId=h.SubunitId AND g.GestiuneId=h.GestiuneId '
    'LEFT JOIN VC_Exercitii e ON e.SubunitId=h.SubunitId AND e.GestiuneId=h.GestiuneId AND e.ExercitiuId=h.ExercitiuId '
    'LEFT JOIN VC_Masini m ON m.SubunitId=h.SubunitId AND m.GestiuneId=h.GestiuneId AND m.IdMasina=h.IdMasina '
    'LEFT JOIN VC_Clienti c ON c.SubunitId=h.SubunitId AND c.GestiuneId=h.GestiuneId AND c.IDC=h.IDC '
    'WHERE COALESCE(h.Retur,0)=0 AND '
)
ID_PART = r'([0-9]{1,10})'
SELECTOR = '/'.join([ID_PART] * len(KEY))
CHILDREN = {
    'VC_IesiriRetur': ('IdBonConsum', 'Există poziții de retur cu același ID în acest exercițiu.'),
    'VC_LegBCLA': ('IdBonConsum', 'Bonul este asociat unei liste de alimente.'),
    'VC_OI_Predare': ('IdBonConsum', 'Bonul aparține unui bon de predare.'),
    'VC_Iesiri_H': ('IdRetur', 'Există un bon de retur care face trimitere la acest bon.'),
}


def commands(block):
    return {
        DELETE: (TITLE, 'Identifică bonul în baza aleasă și cere confirmare înainte de ștergerea antetului și a pozițiilor.',
                 True, partial(delete, block=block)),
        PREVIEW: ('Analiză înainte de ștergere BC', 'Identifică bonul și verifică dacă poate fi șters.',
                  False, partial(preview, block=block)),
    }


def _key(row):
    return tuple(row[column] for column in KEY)


def _selector(row):
    return '/'.join(map(str, _key(row)))


def _scoped_header(conn, key):
    return _headers(conn, ' AND '.join('h.' + col + '=%s' for col in KEY), key)


def _headers(conn, where, params):
    return _rows(conn, HEADERS + where + ' ORDER BY h.DataBC DESC, h.ValoareBC DESC', params)


def _find(conn, param):
    from routes.vercon import bon_number

    selector = re.fullmatch(r'ID\s+' + SELECTOR, param, re.IGNORECASE)
    if selector:
        return _scoped_header(conn, tuple(map(int, selector.groups())))
    identifier = re.fullmatch(r'ID\s+' + ID_PART, param, re.IGNORECASE)
    if identifier:
        return _headers(conn, 'h.IdBonConsum=%s', (int(identifier[1]),))
    number = bon_number.parse('bc', param)
    if number is None:
        # Preserve access to migrated numbers which VERCON displays as stored.
        return _headers(conn, 'TRIM(h.NumarBC)=%s', (param,))
    return _headers(conn, 'UPPER(h.NumarBC) REGEXP %s',
                    (rf'^[[:space:]]*(BC[[:space:]]*)?0*{number}[[:space:]]*$',))


def _label(row):
    from routes.vercon import bon_number

    day = row['DataBC'].strftime('%d.%m.%Y') if row['DataBC'] else 'fără dată'
    number = bon_number.display('bc', row['NumarBC']) or 'fără număr'
    subunit = row['subunit_name'] or 'subunitate necunoscută'
    warehouse = row['warehouse_name'] or 'gestiune necunoscută'
    year = row['exercise_year'] if row['exercise_year'] is not None else 'necunoscut'
    status = 'anulat' if row['Anulat'] else 'activ'
    return (f'{number} din {day} ({status}), {subunit} / {warehouse}, exercițiul {year}\n'
            f"Cont {row['account_name'] or '—'}, primitor {row['receiver_name'] or '—'}, "
            f"valoare {row['ValoareBC'] if row['ValoareBC'] is not None else '—'}")


def _blocker(conn, row):
    from routes.vercon.context import HISTORY_TYPES

    if row['subunit_name'] is None or row['warehouse_type'] is None or row['exercise_year'] is None:
        return 'Contextul bonului este incomplet.'
    if row['exercise_closed'] and row['warehouse_type'] not in HISTORY_TYPES:
        return 'Exercițiul bonului este închis.'
    if re.match(r'\s*BP(?:\s|[0-9]|$)', row['NumarBC'] or '', re.IGNORECASE):
        return 'Documentul are număr de bon de predare.'
    for table, (column, reason) in CHILDREN.items():
        if _rows(conn, f'SELECT 1 FROM {table} WHERE {SCOPE} AND {column}=%s LIMIT 1', _key(row)):
            return reason
    return ''


def _lines(conn, key, lock=False):
    return _rows(conn, 'SELECT * FROM VC_Iesiri WHERE ' + BON + ' ORDER BY ID' + (' FOR UPDATE' if lock else ''), key)


def _token(db, row, lines):
    from routes.vercon.protocol import fingerprint

    return _selector(row) + '/' + fingerprint(db, row, lines)


def _require_safe_schema(conn, db):
    tables = _rows(conn, "SELECT ENGINE AS engine FROM information_schema.TABLES "
                   "WHERE (TABLE_SCHEMA=%s AND TABLE_NAME IN ('VC_Iesiri_H','VC_Iesiri','VC_Gestiuni','VC_Exercitii')) "
                   "OR (TABLE_SCHEMA=%s AND TABLE_NAME='Jurnal')", (db, COMMON_DB))
    if len(tables) != 5 or any(r['engine'] != 'InnoDB' for r in tables):
        raise AdminError('Ștergerea cere tabele InnoDB pentru antet, poziții, gestiune, exercițiu și Jurnal.')
    if _rows(conn, "SELECT 1 FROM information_schema.TRIGGERS WHERE EVENT_OBJECT_SCHEMA=%s "
             "AND EVENT_OBJECT_TABLE IN ('VC_Iesiri_H','VC_Iesiri') AND EVENT_MANIPULATION='DELETE' LIMIT 1", (db,)):
        raise AdminError('Există un declanșator de ștergere pe tabelele bonului. Ștergerea a fost oprită.')

    # Foreign keys with write side effects must match the scoped relations covered by this command.
    relations = _rows(conn, "SELECT k.TABLE_SCHEMA AS db,k.TABLE_NAME AS child,k.REFERENCED_TABLE_NAME AS parent,"
                      "GROUP_CONCAT(CONCAT(k.COLUMN_NAME,'=',k.REFERENCED_COLUMN_NAME) "
                      "ORDER BY k.COLUMN_NAME SEPARATOR ',') AS columns_used "
                      "FROM information_schema.KEY_COLUMN_USAGE k JOIN information_schema.REFERENTIAL_CONSTRAINTS r "
                      "ON r.CONSTRAINT_SCHEMA=k.CONSTRAINT_SCHEMA AND r.TABLE_NAME=k.TABLE_NAME "
                      "AND r.CONSTRAINT_NAME=k.CONSTRAINT_NAME WHERE k.REFERENCED_TABLE_SCHEMA=%s "
                      "AND k.REFERENCED_TABLE_NAME IN ('VC_Iesiri_H','VC_Iesiri') "
                      "AND r.DELETE_RULE NOT IN ('RESTRICT','NO ACTION') "
                      "GROUP BY k.TABLE_SCHEMA,k.TABLE_NAME,k.REFERENCED_TABLE_NAME,k.CONSTRAINT_NAME", (db,))
    child_keys = {'VC_Iesiri': 'IdBonConsum', **{table: col for table, (col, _) in CHILDREN.items()}}
    for relation in relations:
        column = child_keys.get(relation['child'])
        allowed = {f'{col}={col}' for col in KEY[:-1]} | {f'{column}=IdBonConsum'}
        if (relation['db'] != db or relation['parent'] != 'VC_Iesiri_H' or column is None
                or set(relation['columns_used'].split(',')) != allowed):
            raise AdminError(f"Ștergerea ar putea modifica date printr-o cheie străină din {relation['child']}.")


def preview(conn, db, param, block):
    rows = _find(conn, param)
    if not rows:
        return block(db, TITLE, ['rezultat'], [], 'Nu s-a găsit un bon de consum cu această valoare.')
    if len(rows) > 1:
        matches = [[f'ID {_selector(row)}', _label(row).replace('\n', ' | ')] for row in rows]
        return block(db, 'Bonuri de consum găsite', ['identificator', 'bon'], matches,
                     'Alegeți bonul și introduceți identificatorul complet afișat în fața lui.', raw=True)
    row = rows[0]
    reason = _blocker(conn, row)
    lines = _lines(conn, _key(row))
    label = _label(row) + f'\nID {_selector(row)}'
    result = block(db, TITLE, ['bon', 'poziții'], [[label.replace('\n', ' | '), f'{len(lines)} poziții']],
                   reason or 'Antetul și pozițiile vor fi șterse definitiv după confirmare.', raw=True)
    if not reason:
        _require_safe_schema(conn, db)
        result['confirmation_token'] = _token(db, row, lines)
        result['confirmation_label'] = label + f'\nPoziții de șters: {len(lines)}'
    return result


def _check_stock(conn, row, lines):
    # Removing a normal consumption can only increase stock. Legacy negative lines can add stock.
    variants = {line['IdProdus'] for line in lines if line['Cant'] is not None and line['Cant'] < 0}
    if row['Anulat'] or not variants:
        return
    from routes.vercon import consumption_timeline
    from routes.vercon.nomenclatoare import NomError

    if row['DataBC'] is None:
        raise AdminError('Bonul cu cantități negative nu are dată. Stocul nu poate fi verificat.')
    subunit, warehouse, exercise, _ = _key(row)
    ctx = {'subunit': subunit, 'gestiune': warehouse, 'exercitiu': exercise, 'scale': int(row['warehouse_scale'])}
    cursor = conn.cursor(dictionary=True)
    try:
        month_start = row['DataBC'].date().replace(day=1)
        consumption_timeline.require_nonnegative(cursor, ctx, variants, month_start, exclude=('bc', row['IdBonConsum']))
    except NomError as error:
        raise AdminError(str(error)) from error
    finally:
        cursor.close()


def delete(conn, db, param, block):
    confirmed = re.fullmatch(SELECTOR + r'/[0-9a-f]{64}', param)
    if not confirmed:
        raise AdminError('Identificați și confirmați bonul în KBOTADMIN înainte de ștergere.')
    key = tuple(int(value) for value in confirmed.groups())
    changed = 'Bonul sau pozițiile sale s-au schimbat după analiză. Reluați comanda.'
    conn.start_transaction(isolation_level='READ COMMITTED')
    try:
        # VERCON document writers take the same warehouse lock before touching any document.
        if not _rows(conn, 'SELECT GestiuneId FROM VC_Gestiuni WHERE SubunitId=%s AND GestiuneId=%s FOR UPDATE', key[:2]):
            raise AdminError(changed)
        if not _rows(conn, 'SELECT ExercitiuId FROM VC_Exercitii WHERE ' + SCOPE + ' FOR UPDATE', key[:3]):
            raise AdminError(changed)
        if not _rows(conn, 'SELECT IdBonConsum FROM VC_Iesiri_H WHERE ' + BON + ' FOR UPDATE', key):
            raise AdminError(changed)
        rows = _scoped_header(conn, key)
        lines = _lines(conn, key, lock=True)
        if len(rows) != 1 or _token(db, rows[0], lines) != param:
            raise AdminError(changed)
        row = rows[0]
        reason = _blocker(conn, row)
        if reason:
            raise AdminError(reason)
        _require_safe_schema(conn, db)
        _check_stock(conn, row, lines)
        if _exec(conn, 'DELETE FROM VC_Iesiri WHERE ' + BON, key) != len(lines):
            raise AdminError(changed)
        if _exec(conn, 'DELETE FROM VC_Iesiri_H WHERE ' + BON + ' AND COALESCE(Retur,0)=0', key) != 1:
            raise AdminError(changed)
        label = _label(row).replace('\n', ' | ')
        _journal(conn, DELETE, db, 'VC_Iesiri_H ID ' + _selector(row),
                 f'{label} | Antete șterse: 1. Poziții șterse: {len(lines)}.')
        conn.commit()
    except Exception:
        conn.rollback()
        raise
    return block(db, DELETED_TITLE, ['bon', 'antete șterse', 'poziții șterse'],
                 [[label, '1 antet', f'{len(lines)} poziții']], 'Ștergerea este consemnată în Jurnal.', raw=True)
