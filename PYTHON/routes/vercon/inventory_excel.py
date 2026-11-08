"""VC12-04: read the supplied XLSX inventory layout; never persist the workbook."""
from io import BytesIO
import re
from zipfile import BadZipFile, ZipFile
from xml.etree import ElementTree as ET
from posixpath import normpath

from .nomenclatoare import NomError
from . import numeric
from routes.admin_cli.inventory_similarity import number
from utils.vercon_suppliers import name_key

NS = {'x': 'http://schemas.openxmlformats.org/spreadsheetml/2006/main'}
REL = '{http://schemas.openxmlformats.org/officeDocument/2006/relationships}id'


def reject(message):
    raise NomError('INVENTORY_EXCEL', message, 422)


def read(data, sheet=None):
    if not data or len(data) > 20000000:
        reject('Alegeți un fișier XLSX de cel mult 20 MB.')
    try:
        with ZipFile(BytesIO(data)) as book:
            if len(book.infolist()) > 10000:
                reject('Fișierul XLSX conține prea multe componente.')
            def xml(path):
                info = book.getinfo(path)
                if info.file_size > 25000000:
                    reject('Componentă Excel prea mare.')
                return ET.fromstring(book.read(info))
            rels = {r.get('Id'): r.get('Target') for r in xml('xl/_rels/workbook.xml.rels')
                    if r.get('TargetMode') != 'External'}
            paths = {}
            for row in xml('xl/workbook.xml').findall('x:sheets/x:sheet', NS):
                target = rels.get(row.get(REL), '')
                path = normpath(target.lstrip('/') if target.startswith('/') else 'xl/' + target)
                if path.startswith('xl/'):
                    paths[row.get('name')] = path
            sheets = sorted(paths, key=name_key)
            if sheet is None:
                return {'sheets': sheets}
            if sheet not in paths:
                reject('Foaia aleasă nu există în Excel.')
            shared = []
            if 'xl/sharedStrings.xml' in book.namelist():
                shared = [''.join(t.text or '' for t in row.findall('.//x:t', NS))
                          for row in xml('xl/sharedStrings.xml')]
            def text(cell):
                kind = cell.get('t')
                value = cell.findtext('x:v', '', NS)
                if kind == 's':
                    return shared[int(value)].strip()
                if kind == 'inlineStr':
                    return ''.join(t.text or '' for t in cell.findall('.//x:t', NS)).strip()
                return '' if kind == 'e' else value.strip()
            result, errors, seen, kind, omitted = [], [], set(), None, 0
            for row in xml(paths[sheet]).findall('x:sheetData/x:row', NS):
                cells = {re.sub(r'\d', '', c.get('r', '')): text(c) for c in row.findall('x:c', NS)}
                if cells.get('K', '').strip().rstrip(':').casefold() == 'gestiunea':
                    label = cells.get('L', '').casefold()
                    kind = 'MF' if 'mijloace fixe' in label else 'OI' if 'obiecte de inventar' in label else None
                serial = cells.get('A', '')
                if not serial.isdigit() or int(serial) <= 0:
                    continue
                if kind == 'MF':
                    omitted += 1
                    continue
                def skip(message):
                    errors.append({'row': row.get('r'), 'serial': int(serial), 'message': message,
                                   'name': cells.get('B'), 'um': cells.get('D'), 'price': cells.get('I')})
                if not kind or not cells.get('B'):
                    skip('Secțiune OI nerecunoscută sau denumire lipsă.')
                    continue
                qty, price = number(cells.get('F')), number(cells.get('I'))
                if qty is None or qty <= 0 or price is None or price < 0 or not cells.get('D'):
                    skip('Cantitate, UM sau preț lipsă/invalid.')
                    continue
                if len(cells['B']) > 255 or len(cells.get('C', '')) > 255 or len(cells['D']) > 50:
                    skip('Denumire/cod/UM prea lung.')
                    continue
                try:
                    qty, price = numeric.to_stored(qty), numeric.to_stored(price)
                except numeric.NumericError:
                    skip('Cantitate/preț în afara capacității sau cu prea multe zecimale.')
                    continue
                if qty <= 0:
                    skip('Cantitatea trebuie să fie pozitivă la precizia de stoc.')
                    continue
                try:
                    position = int(row.get('r', ''))
                except ValueError:
                    skip('Numărul fizic al rândului Excel este invalid.')
                    continue
                if position <= 0 or position in seen:
                    skip('Numărul fizic al rândului Excel este invalid sau duplicat.')
                    continue
                seen.add(position)
                result.append({'row': position, 'serial': int(serial), 'name': cells['B'],
                               'code': cells.get('C', ''), 'um': cells['D'], 'quantity': format(qty, 'f'),
                               'price': format(price, 'f'), 'kind': kind})
                if len(result) > 5000:
                    reject('Cel mult 5000 poziții per foaie.')
            if not result and not errors:
                reject('Foaia nu conține obiecte de inventar (OI) în macheta acceptată. Mijloacele fixe sunt omise.')
            return {'sheets': sheets, 'rows': result, 'sheet': sheet, 'omitted': omitted, 'errors': errors}
    except (BadZipFile, KeyError, IndexError, ValueError, ET.ParseError):
        reject('Fișierul nu este un inventar XLSX valid în macheta acceptată.')
