"""SLICE-VC12-01: read-only Excel inventory comparison through KBotAdmin's SSH CLI.

Only the selected database / SubunitId / GestiuneId / IdMasina is read. Prices are
VC_ProduseP.PUTVA decimals (no scaling, VAT multiplication or current-stock claim).
Candidates are existing VC_ProduseP rows; VC_Produse supplies their name, UM and account.
The local XLSX never leaves the PC; a bounded temporary JSON contains goods only.
"""
import json
import os
import re
import stat

from routes.admin_cli.admin_common import AdminError
from routes.admin_cli.vercon_reports import unit
from routes.admin_cli.inventory_similarity import compare
from routes.vercon.connection import connection
from utils.vercon_suppliers import name_key


def _all(cursor, sql, params=()):
    cursor.execute(sql, params)
    return cursor.fetchall()


def targets(args):
    name = unit(args)
    with connection(name) as conn:
        cursor = conn.cursor(dictionary=True)
        result = {
            'unit': name,
            'subunits': _all(cursor, 'SELECT SubunitId AS id, Name AS name FROM VC_Subunitati WHERE Active=1'),
            'gestiuni': _all(cursor, "SELECT SubunitId AS subunit, GestiuneId AS id, Name AS name, Type AS type "
                                     "FROM VC_Gestiuni WHERE Active=1 AND Type IN ('OI','MF')"),
            'accounts': _all(cursor, 'SELECT SubunitId AS subunit, GestiuneId AS gestiune, '
                                     'IdMasina AS id, Masina AS name FROM VC_Masini WHERE Activ=1'),
        }
        for key in ('subunits', 'gestiuni', 'accounts'):
            result[key].sort(key=lambda r: name_key(r['name']))
        return result


def _payload(path):
    if not re.fullmatch(r'/tmp/kbot-inventory-[0-9a-f]{32}\.json', path):
        raise AdminError('Calea fișierului temporar de comparație nu este validă.')
    fd = os.open(path, os.O_RDONLY | os.O_NOFOLLOW)
    try:
        info = os.fstat(fd)
        if not stat.S_ISREG(info.st_mode) or info.st_uid != os.getuid() or info.st_size > 2000000:
            raise AdminError('Fișier temporar invalid sau prea mare (maximum 2 MB).')
        with os.fdopen(fd, 'r', encoding='utf-8') as stream:
            fd = None
            try:
                rows = json.loads(stream.read(2000001))
            except (ValueError, UnicodeError):
                raise AdminError('Datele Excel trimise nu sunt valide.') from None
    finally:
        if fd is not None:
            os.close(fd)
        os.unlink(path)
    if not isinstance(rows, list) or not 1 <= len(rows) <= 5000:
        raise AdminError('Comparația acceptă între 1 și 5000 de poziții.')
    seen = set()
    for row in rows:
        if (not isinstance(row, dict) or type(row.get('row')) is not int or row['row'] < 1 or
                row['row'] in seen or not isinstance(row.get('name'), str) or not row['name'].strip()):
            raise AdminError('Rândurile Excel trebuie să aibă denumire și număr unic de rând.')
        seen.add(row['row'])
        for key in ('name', 'code', 'um', 'price', 'quantity', 'kind'):
            if row.get(key) is not None and (not isinstance(row[key], (str, int, float)) or len(str(row[key])) > 1000):
                raise AdminError('Valoare Excel prea lungă sau invalidă: %s.' % key)
    return rows


def check(args):
    rows = _payload(args.payload_file)
    name = unit(args)
    with connection(name) as conn:
        cursor = conn.cursor(dictionary=True)
        # SLICE-VC12-02: Connector/Python can parse MariaDB's compatibility prefix
        # as MySQL 5.5.5 and reject readonly=True before sending SQL. Let MariaDB
        # handle its native syntax; preserve both read-only mode and one snapshot.
        cursor.execute('SET TRANSACTION ISOLATION LEVEL REPEATABLE READ')
        cursor.execute('START TRANSACTION WITH CONSISTENT SNAPSHOT, READ ONLY')
        context = _all(cursor, 'SELECT g.Name AS gestiune, g.Type AS type, s.Name AS subunit, m.Masina AS account '
                       'FROM VC_Gestiuni g JOIN VC_Subunitati s ON s.SubunitId=g.SubunitId '
                       'JOIN VC_Masini m ON m.SubunitId=g.SubunitId AND m.GestiuneId=g.GestiuneId '
                       'WHERE s.Active=1 AND g.Active=1 AND m.Activ=1 '
                       'AND g.SubunitId=%s AND g.GestiuneId=%s AND m.IdMasina=%s',
                       (args.subunit, args.gestiune, args.account))
        if not context or context[0]['type'] not in ('OI', 'MF'):
            raise AdminError('Alegeți Gestiune / Subgestiune OI sau MF / Cont din aceeași bază.')
        kind = context[0]['type']
        if any(r.get('kind') != kind for r in rows):
            raise AdminError('Secțiunea Excel nu corespunde tipului subgestiunii alese.')
        # SLICE-VC12-03: only actual ProduseP rows can be comparison candidates.
        products = _all(cursor, 'SELECT p.ID AS product_id, v.IdProdus AS variant_id, p.DenumireP AS name, '
                        'p.UM AS um, v.COD AS code, v.PUTVA AS price, (p.Activ=1 AND v.Activ=1) AS active '
                        'FROM VC_ProduseP v JOIN VC_Produse p ON p.SubunitId=v.SubunitId '
                        'AND p.GestiuneId=v.GestiuneId AND p.ID=v.ID '
                        'WHERE v.SubunitId=%s AND v.GestiuneId=%s AND p.IdMasina=%s',
                        (args.subunit, args.gestiune, args.account))
        conn.rollback()
    for product in products:
        product['price'] = str(product['price']) if product['price'] is not None else None
        product['active'] = bool(product['active'])
    return {'unit': name, **context[0], 'products': len(products), 'rows': compare(rows, products)}


def register(sub):
    parser = sub.add_parser('vercon-inventory-targets')
    parser.add_argument('--db', required=True)
    parser.set_defaults(fn=targets)
    parser = sub.add_parser('vercon-inventory-check')
    parser.add_argument('--db', required=True)
    for key in ('subunit', 'gestiune', 'account'):
        parser.add_argument('--' + key, type=int, required=True)
    parser.add_argument('--payload-file', required=True)
    parser.set_defaults(fn=check)
