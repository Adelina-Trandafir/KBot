"""Prepared VC08-02 synthetic comparator. Never opens the app/Access/MariaDB.

Execute only when verification is explicitly requested. Cases compare per asset,
with a literal legacy DateDiff/formula reference and explicit corrected differences.
"""
import importlib
import sys
import types
import unittest
from datetime import date
from pathlib import Path
from unittest.mock import patch

ROOT = Path(__file__).resolve().parents[2]
PACKAGE = '_vc08_amortization'
module = types.ModuleType(PACKAGE)
module.__path__ = [str(ROOT / 'PYTHON' / 'routes' / 'vercon')]
sys.modules[PACKAGE] = module
engine = importlib.import_module(PACKAGE + '.depreciation')
reader = importlib.import_module(PACKAGE + '.depreciation_read')
source = importlib.import_module(PACKAGE + '.depreciation_source')

CTX = {'subunit': 4, 'gestiune': 8, 'exercitiu': 12, 'scale': 2, 'zec': 2, 'type': 'MF'}


def asset(**changes):
    return {'IdProdus': 1, 'RowId': 11, 'name': 'Activ', 'code': 'MF001', 'um': 'buc', 'category': 2,
            'account': '213', 'Version': 3, 'price_version': 7, 'price': 10000, 'DataIntrare': date(2024, 1, 1),
            'DNF': 3, 'DF': 0, 'ValoareLuna': 3333, 'ValoareLunara': None, **changes}


def event(kind, day, quantity=100):
    return {'IdProdus': 1, 'kind': kind, 'date': day, 'quantity': quantity, 'document_id': 1, 'line_id': 1}


EVENTS = [event('receipt', date(2024, 1, 1)), event('handover', date(2024, 1, 31))]


def legacy_used_and_remaining(dnf, df, handover, on):
    """Literal Amortizare.sql DateDiff('m', dsf, databp), including its Abs defect."""
    months = (handover.year - on.year) * 12 + handover.month - on.month
    return min(dnf, df + abs(months)), max(0, dnf - df + months)


class Cursor:
    def __init__(self, rows):
        self.responses = iter(rows)
        self.calls = []

    def execute(self, sql, params):
        self.calls.append((sql, params))

    def fetchall(self):
        return next(self.responses)


class DepreciationCases(unittest.TestCase):
    def test_calendar_boundaries_and_leap_year(self):
        for start, end, expected in [(date(2024, 1, 31), date(2024, 2, 1), 1),
                                     (date(2024, 2, 1), date(2024, 2, 29), 0),
                                     (date(2024, 2, 29), date(2024, 3, 1), 1),
                                     (date(2023, 12, 31), date(2024, 1, 1), 1)]:
            with self.subTest(start=start, end=end):
                self.assertEqual(engine.calendar_months(start, end), expected)

    def test_durations_match_legacy_after_handover(self):
        for on in [date(2024, 1, 31), date(2024, 2, 1), date(2024, 3, 31), date(2024, 4, 1)]:
            result = engine.calculate(asset(), EVENTS, on, 2, 2)
            expected_used, expected_remaining = legacy_used_and_remaining(3, 0, date(2024, 1, 31), on)
            self.assertEqual((result['used_months'], result['remaining_months']), (expected_used, expected_remaining))

    def test_final_month_absorbs_cent_instead_of_legacy_99_99(self):
        previous = engine.calculate(asset(), EVENTS, date(2024, 3, 31), 2, 2)
        final = engine.calculate(asset(), EVENTS, date(2024, 4, 1), 2, 2)
        self.assertEqual(previous['accumulated'], 6666)
        self.assertEqual(previous['remaining'], 3334)
        self.assertEqual(final['month_depreciation'], 3334)
        self.assertEqual(final['accumulated'], 10000)
        self.assertEqual(final['remaining'], 0)
        self.assertEqual(3 * 3333, 9999)  # documented intentional legacy difference

    def test_df_at_start_and_complete_duration(self):
        first = engine.calculate(asset(DF=2), EVENTS, date(2024, 1, 31), 2, 2)
        final = engine.calculate(asset(DF=2), EVENTS, date(2024, 2, 1), 2, 2)
        self.assertEqual(first['accumulated'], 6666)
        self.assertEqual(first['month_depreciation'], 0)
        self.assertEqual(final['month_depreciation'], 3334)
        self.assertEqual(final['remaining_months'], 0)
        complete = engine.calculate(asset(DF=3), EVENTS, date(2024, 1, 31), 2, 2)
        self.assertEqual(complete['accumulated'], 10000)
        self.assertEqual(complete['next_month_depreciation'], 0)

    def test_no_abs_depreciation_before_future_handover_or_entry(self):
        later = [event('receipt', date(2024, 1, 1)), event('handover', date(2024, 5, 1))]
        before = engine.calculate(asset(), later, date(2024, 2, 1), 2, 2)
        self.assertEqual(before['status'], 'not_in_use')
        self.assertIsNone(before['accumulated'])
        self.assertEqual(legacy_used_and_remaining(3, 0, date(2024, 5, 1), date(2024, 2, 1))[0], 3)
        self.assertEqual(engine.calculate(asset(), EVENTS, date(2023, 12, 31), 2, 2)['status'], 'not_entered')

    def test_null_or_zero_duration_and_bad_df_are_diagnostics(self):
        for changes in ({'DNF': None}, {'DNF': 0}, {'DF': None}, {'DF': -1}, {'DF': 4}):
            with self.subTest(changes=changes):
                result = engine.calculate(asset(**changes), EVENTS, date(2024, 2, 1), 2, 2)
                self.assertEqual(result['status'], 'invalid')
                self.assertIsNone(result['accumulated'])
                self.assertTrue(result['issues'])

    def test_source_mapping_zec_and_last_exact_fraction(self):
        mapped = engine.calculate(asset(ValoareLuna=None, ValoareLunara=3333), EVENTS, date(2024, 2, 1), 2, 2)
        self.assertEqual(mapped['monthly_source'], 'ValoareLunara')
        exact = engine.calculate(asset(price=100005, ValoareLuna=33335),
                                 [event('receipt', date(2024, 1, 1), 1000), event('handover', date(2024, 1, 31), 1000)],
                                 date(2024, 4, 1), 3, 2)
        self.assertEqual(exact['monthly'], 33340)  # 33.335 -> 33.34 HALF_EVEN
        self.assertEqual(exact['month_depreciation'], 33325)
        self.assertEqual(exact['accumulated'], 100005)

    def test_transfer_does_not_reset_and_scrapping_stops(self):
        moved = EVENTS + [event('transfer', date(2024, 2, 20))]
        result = engine.calculate(asset(), moved, date(2024, 3, 1), 2, 2)
        self.assertEqual(result['used_months'], 2)
        self.assertEqual(result['start_date'], '2024-01-31')
        disposed = engine.calculate(asset(), moved + [event('scrapping', date(2024, 2, 20))], date(2025, 1, 1), 2, 2)
        self.assertEqual(disposed['status'], 'written_off')
        self.assertEqual(disposed['accumulated'], 3333)
        self.assertEqual(disposed['month_depreciation'], 0)
        self.assertEqual(disposed['written_off_remaining'], 6667)

    def test_multiple_document_lines_do_not_multiply_an_asset(self):
        documents = [event('receipt', date(2024, 1, 1), 40), event('receipt', date(2024, 1, 2), 60),
                     event('handover', date(2024, 1, 31), 40), event('handover', date(2024, 2, 1), 60)]
        with patch.object(source, 'load', return_value=({1: asset()}, {1: documents})):
            result = reader.read(None, CTX, date(2024, 2, 1))
        self.assertEqual(len(result['assets']), 1)
        self.assertEqual(result['totals']['accumulated'], '33.33')
        self.assertEqual(result['assets'][0]['receipt_lines'], 2)

    def test_ambiguous_units_partial_scrapping_and_duplicate_register(self):
        variants = [EVENTS + [event('receipt', date(2024, 1, 2))], EVENTS + [event('scrapping', date(2024, 2, 1), 50)]]
        for documents in variants:
            self.assertEqual(engine.calculate(asset(), documents, date(2024, 2, 1), 2, 2)['status'], 'invalid')
        self.assertEqual(engine.calculate(asset(duplicate_rows=[12]), EVENTS, date(2024, 2, 1), 2, 2)['status'], 'invalid')

    def test_loader_separates_register_from_scoped_document_union(self):
        cursor = Cursor([[asset(), asset(RowId=12)], EVENTS])
        assets, _ = source.load(cursor, CTX)
        self.assertEqual(len(assets), 1)
        self.assertEqual(assets[1]['duplicate_rows'], [12])
        self.assertEqual(len(cursor.calls), 2)
        sql, params = cursor.calls[1]
        self.assertEqual(sql.count('ExercitiuId=%(e)s'), 5)
        self.assertEqual(sql.count('GestiuneId=%(g)s'), 5)
        self.assertEqual(params, {'s': 4, 'g': 8, 'e': 12})


if __name__ == '__main__':
    unittest.main(verbosity=2)
