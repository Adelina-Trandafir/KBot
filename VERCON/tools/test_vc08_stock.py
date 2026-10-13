"""Local synthetic regression cases for VC08-01; no app or DB is opened."""
import importlib
import sys
import types
import unittest
from datetime import date
from pathlib import Path
from unittest.mock import patch

ROOT = Path(__file__).resolve().parents[2]
PACKAGE = '_vc08_local'
module = types.ModuleType(PACKAGE)
module.__path__ = [str(ROOT / 'PYTHON' / 'routes' / 'vercon')]
sys.modules[PACKAGE] = module
stock = importlib.import_module(PACKAGE + '.stock')
source = importlib.import_module(PACKAGE + '.stock_source')
protocol = importlib.import_module(PACKAGE + '.protocol')

START, END = date(2026, 10, 1), date(2026, 10, 31)
CTX = {'subunit': 4, 'gestiune': 8, 'exercitiu': 12, 'scale': 2, 'zec': 2, 'type': 'MATERIALE'}


def movement(q, day=START, *, p=1, supplier=None, holder=None, kind=None, doc=1):
    return {'p': p, 'd': day, 'q': q, 'supplier': supplier, 'holder': holder,
            'kind': kind or ('nir' if q >= 0 else 'bc'), 'document_id': doc, 'line_id': doc, 'number': str(doc)}


class Cursor:
    def __init__(self, rows=()):
        self.rows = rows
        self.calls = []

    def execute(self, sql, params):
        self.calls.append((sql, params))

    def fetchall(self):
        return self.rows


class StockCases(unittest.TestCase):
    def test_opening_and_closed_interval(self):
        rows = [movement(1000, date(2026, 9, 1)), movement(-200, START),
                movement(300, END), movement(-999, date(2026, 11, 1))]
        groups, sheet = stock.balance(rows, START, END)
        self.assertEqual(groups[(1,)]['opening'], 1000)
        self.assertEqual(groups[(1,)]['closing'], 1100)
        self.assertEqual([r['balance'] for r in sheet], [800, 1100])

    def test_n07_scrapping_at_both_boundaries_quantity_and_value(self):
        rows = [movement(600, date(2026, 9, 1), kind='bp', holder=7),
                movement(-200, START, kind='bs', holder=7), movement(-200, END, kind='bs', holder=7)]
        groups, _ = stock.balance(rows, START, END, ('holder', 'p'))
        quantities = groups[(7, 1)]
        self.assertEqual(quantities['scrapping'], 400)
        self.assertEqual(quantities['closing'], 200)
        self.assertEqual(stock.value_text(200, 10000, 2), '200.0000')

    def test_transfer_zero_net_but_independent_holders(self):
        rows = [movement(500, date(2026, 9, 1), kind='bp', holder=7),
                movement(-200, kind='bt_out', holder=7), movement(200, kind='bt_in', holder=8)]
        groups, _ = stock.balance(rows, START, END, ('holder', 'p'))
        self.assertEqual(groups[(7, 1)]['closing'], 300)
        self.assertEqual(groups[(8, 1)]['closing'], 200)
        self.assertEqual(sum(r['closing'] for r in groups.values()), 500)

    def test_n08_later_storno_and_exact_draft(self):
        rows = [movement(10000), movement(-10000, date(2026, 11, 1), kind='storno')]
        self.assertEqual(stock.available(rows, END, {1: 10040})[1], -40)
        self.assertEqual(stock.available(rows, END)[1], 10000)

    def test_n09_each_supplier_or_product_changes_the_complete_key(self):
        rows = [movement(100, supplier=10), movement(200, supplier=20),
                movement(300, supplier=10, p=2), movement(-40, supplier=10, doc=2)]
        groups, sheet = stock.balance(rows, START, END, ('supplier', 'p'))
        self.assertEqual(groups[(10, 1)]['closing'], 60)
        self.assertEqual(groups[(20, 1)]['closing'], 200)
        self.assertEqual(groups[(10, 2)]['closing'], 300)
        self.assertEqual(sheet[-1]['balance'], 60)

    def test_supplier_filter_keeps_earlier_consumption_of_selected_variant(self):
        metadata = {1: {'variant': 1, 'product': 3, 'category': 2, 'category_name': 'Cont',
                        'price': 10000, 'name': 'A', 'um': 'buc', 'code': 'A'}}
        rows = [movement(500, date(2026, 9, 1), supplier=10), movement(-100, date(2026, 9, 2)),
                movement(-100, START)]
        with patch.object(source, 'variants', return_value=metadata), patch.object(source, 'warehouse', return_value=rows):
            result = stock.read(Cursor(), CTX, START, END, supplier=10)
        self.assertEqual(result['rows'][0]['quantities']['opening'], '4.00')
        self.assertEqual(result['rows'][0]['quantities']['closing'], '3.00')

    def test_current_price_revalues_stock_without_changing_movements(self):
        metadata = {1: {'variant': 1, 'product': 3, 'category': 2, 'category_name': 'Cont',
                        'price': 10000, 'name': 'A', 'um': 'buc', 'code': 'A'}}
        rows = [movement(200)]
        with patch.object(source, 'variants', return_value=metadata), patch.object(source, 'warehouse', return_value=rows):
            first = stock.read(Cursor(), CTX, START, END)
            metadata[1]['price'] = 12500
            second = stock.read(Cursor(), CTX, START, END)
        self.assertEqual(first['value_totals']['closing'], '200.0000')
        self.assertEqual(second['value_totals']['closing'], '250.0000')
        self.assertEqual(rows[0]['q'], 200)

    def test_each_sql_branch_has_context_and_exclusion_only_in_edited_family(self):
        cursor = Cursor()
        source.warehouse(cursor, CTX, [1, 2], ('bc', 88))
        sql, params = cursor.calls[0]
        self.assertEqual(sql.count('ExercitiuId=%(e)s'), 4)
        self.assertEqual(sql.count('GestiuneId=%(g)s'), 4)
        self.assertEqual(sql.count('SubunitId=%(s)s'), 4)
        self.assertEqual(sql.count('IdProdus IN (1,2)'), 4)
        self.assertEqual(sql.count('<>%(exclude)s'), 1)
        self.assertIn('h.IdBonConsum<>%(exclude)s', sql)
        self.assertEqual(params['exclude'], 88)
        self.assertNotIn('BETWEEN', sql)

    def test_empty_variant_filter_does_not_load_all_stock(self):
        cursor = Cursor()
        self.assertEqual(source.warehouse(cursor, CTX, []), [])
        self.assertEqual(cursor.calls, [])

    def test_protocol_keeps_existing_quantity_api_and_negative_timeline(self):
        cursor = Cursor([movement(200), movement(-300, doc=2)])
        self.assertEqual(protocol.available(cursor, CTX, END), {1: -100})
        with patch.object(protocol, '_label', return_value='A'):
            with self.assertRaises(stock.NomError) as caught:
                protocol.check_timeline(cursor, CTX, [1])
        self.assertEqual(caught.exception.reason, 'STOCK_NEGATIVE')


if __name__ == '__main__':
    unittest.main(verbosity=2)
