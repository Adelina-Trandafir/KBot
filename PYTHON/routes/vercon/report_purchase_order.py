"""The landscape purchase order form used by VERCON Listare and Excel."""
from decimal import Decimal
from itertools import groupby

from . import report_model
from .report_balance import alphabetical

REPORT_ID = 'Nota_Comanda'
FORM = 'purchase-order'
# Set to False to reactivate the former generic purchase order.
ENABLED = True
LINES_PER_PAGE = 12
COLUMNS = (
    ('code', 'COD\nPRODUS', 1.12, 'text', 'l'),
    ('name', 'PRODUS', 3.33, 'text', 'l'),
    ('um', 'UM', .58, 'text', 'c'),
    ('um_code', 'COD\nUM', .73, 'text', 'c'),
    ('quantity', 'CANTITATE', 2.10, 'decimal', 'r'),
    ('price', 'PREȚ', 1.25, 'decimal', 'r'),
    ('value', 'VALOARE', 1.28, 'decimal', 'r'),
    ('delivery_term', 'TERMEN LIVRARE', 1.74, 'text', 'l'),
)
FIELDS = tuple(column[0] for column in COLUMNS)


def active(spec):
    return ENABLED and spec['id'] == REPORT_ID


def layout(previous):
    columns = [{'key': key, 'label': label, 'width': width, 'type': kind, 'align': align}
               for key, label, width, kind, align in COLUMNS]
    return {**previous, 'form': FORM, 'landscape': True, 'font_size': 9, 'columns': columns,
            'fields': list(FIELDS), 'groups': ['party'],
            'signatures': ['DIRECTOR,', 'ADMINISTRATOR,', 'Vizat CFP,'],
            'reference': 'Modelul Notă de comandă din 20.11.2026'}


def _supplier(row):
    party = row.get('party') or ''
    return alphabetical(party), party, str(row.get('supplier') or '')


def _line_order(row):
    price = row.get('price')
    return (alphabetical(row.get('name')), alphabetical(row.get('code')),
            price is None, Decimal(price or 0))


def blocks(definition, rows, issuer, zec):
    if not rows:
        return [{'t': 'title', 'text': definition['title']},
                {'t': 'text', 'text': 'Nu există înregistrări pentru selecția curentă.'}]
    # A fiscal identity can be absent on older receipts. In that case the
    # name still keeps different recipients apart.
    sorted_rows = sorted(rows, key=_supplier)
    pages = []
    value_decimals = definition['columns'][FIELDS.index('value')].get('decimals', zec)
    for _, items in groupby(sorted_rows, _supplier):
        lines = sorted(items, key=_line_order)
        for first in range(0, len(lines), LINES_PER_PAGE):
            if pages:
                pages.append({'t': 'break', 'space_after': 0})
            end = first + LINES_PER_PAGE
            total_text = None
            if end >= len(lines):
                total_text = report_model.number(report_model.total(lines, 'value'), value_decimals)
            pages.append(_page(definition, lines[first:end], issuer, zec, total_text))
    return pages


def _page(definition, lines, issuer, zec, total_text):
    cells = []
    size = definition['font_size']
    columns = definition['columns']

    def cell(column, row, text='', *, colspan=1, rowspan=1, edges='', align='l', **options):
        cells.append({'column': column, 'row': row, 'text': text, 'colspan': colspan,
                      'rowspan': rowspan, 'edges': edges, 'align': align, 'size': size, **options})

    supplier = lines[0]
    cell(0, 0, '\n'.join((
        'Denumire angajator  ' + issuer['name'],
        'CF  ' + issuer['fiscal_code'],
        'Adresă  ' + issuer['address'],
        'Nr. Reg. Com.  ......................................',
    )), colspan=4, rowspan=2)
    cell(4, 0, definition['title'], colspan=4, edges='lrt', align='c', bold=True, size=definition['title_size'])
    cell(4, 1, '\n'.join((
        'Către  ' + (supplier.get('party') or '..........................................................'),
        'Localitatea  ' + (supplier.get('supplier_city') or '...........................................'),
        'Adresă  ' + (supplier.get('address') or '........................................................'),
    )), colspan=4, rowspan=3, edges='lrb')
    cell(0, 3, 'Număr comandă  ................................\n\nData  .................................................',
         colspan=2, edges='lrtb')
    cell(0, 5, '\n'.join((
        'Rugăm a expedia la adresa  .....................................................................................................................................................',
        'Prin  .....................................................   Tel.  ...............................................................................................................................',
        'Plata se va face     Nr. cont  ......................................................................   Banca / Trezoreria  ....................................................',
        'Recepția se va face la primirea bunurilor sau prestarea serviciilor.  Condiție de livrare  ........................................................',
    )), colspan=8)
    for index, column in enumerate(columns):
        cell(index, 6, column['label'], edges='lrtb', align='c', size=column.get('header_size', size))
    for index in range(LINES_PER_PAGE):
        line = lines[index] if index < len(lines) else {}
        for column_index, column in enumerate(columns):
            key = column['key']
            value = line.get(key)
            numeric = column['type'] == 'decimal'
            text = '' if value is None else str(value)
            if numeric:
                text = report_model.number(text, column.get('decimals', zec))
            cell(column_index, 7 + index, text, edges='lrtb', align=column.get('align', 'l'),
                 numeric=numeric, size=column.get('size', size))
            if key == 'name' and line:
                cells[-1]['product'] = {field: line.get(field) for field in ('variant', 'name', 'code', 'um')}
    total_row = 7 + LINES_PER_PAGE
    if total_text is None:
        for column_index in range(len(columns)):
            cell(column_index, total_row, edges='lrtb')
    else:
        value_column = FIELDS.index('value')
        cell(0, total_row, 'TOTAL', colspan=value_column, edges='lrtb', align='r', bold=True)
        cell(value_column, total_row, total_text, edges='lrtb', align='r', bold=True, numeric=True)
        cell(value_column + 1, total_row, edges='lrtb')
    signature_row = total_row + 2
    for (column, span), caption in zip(((0, 2), (2, 3), (5, 3)), definition['signatures']):
        cell(column, signature_row, caption, colspan=span, align='c', bold=True)
    return {'t': 'form_grid', 'widths': [float(column['width']) * 100 for column in columns],
            'keys': [column['key'].upper() for column in columns], 'key_prefix': '',
            'heights': [26, 43, 7, 48, 8, 61, 32] + [17] * (LINES_PER_PAGE + 1) + [16, 32],
            'cells': cells, 'cell_edges': True, 'fit_page': True, 'space_after': 0}
