"""SLICE-VC14-05: «the same invoice number at the same supplier» -- a WARNING, never a block.

Two receipts (NIR) or storno invoices (FCS) of one gestiune, not cancelled, that carry the same invoice number for the same
supplier (the normalized fiscal code, not the name) are probably the same invoice entered twice. The number is compared with
upper/lower case and spaces ignored; the invoice date is not compared. A saved document is never refused for this: the page
shows the answer through its message system and the operator decides.

Used by the NIR / FCS screen (while the number is typed and before saving) and by the e-invoice association window.
"""
import re

from utils import vercon_efactura as shared
from utils import vercon_suppliers as suppliers

from .documents import _all, _iso, _scope, _sub_text
from .nomenclatoare import NomError

_LIMIT = 6


def key(number):
    """The number as it is compared: no spaces, no case."""
    return re.sub(r'\s+', '', str(number or '')).upper()


def find(cursor, ctx, fiscal_code, number, exclude=None, ignore=()):
    """Other saved NIR / FCS of this gestiune with the same invoice number at the same supplier. `exclude` = the document being edited,
    `ignore` = documents that must not count (those already tied to the same e-invoice). [] when the supplier or the number is empty."""
    code, wanted = suppliers.code(fiscal_code), key(number)
    if not code or not wanted:
        return []
    supplier = shared.receipt_supplier_sql(cursor, 'n')
    sql = ('SELECT n.IdIntrare, n.Numar, COALESCE(n.NumarSub,0) AS sub_number, COALESCE(n.Storno,0) AS storno, n.Data, n.DataFc, n.NrFact '
           'FROM VC_NIR n WHERE n.SubunitId=%s AND n.GestiuneId=%s AND n.Anulat=0 AND n.TipDoc<>\'STOC_INITIAL\' '
           "AND NULLIF(" + supplier + ",'')=%s AND REPLACE(UPPER(TRIM(COALESCE(n.NrFact,''))),' ','')=%s")
    params = [ctx['subunit'], ctx['gestiune'], code, wanted]
    skip = [int(item) for item in (exclude, *ignore) if item]
    if skip:
        sql += ' AND n.IdIntrare NOT IN (' + ','.join(['%s'] * len(skip)) + ')'
        params.extend(skip)
    rows = _all(cursor, sql + ' ORDER BY n.Data DESC, n.IdIntrare DESC LIMIT %d' % _LIMIT, params)
    return [{'id': r['IdIntrare'], 'family': 'storno' if r['storno'] else 'nir', 'number': abs(r['Numar'] or 0),
             'sub_number': r['sub_number'], 'sub_text': _sub_text(int(r['sub_number'] or 0)), 'date': _iso(r['Data']), 'invoice_date': _iso(r['DataFc']),
             'invoice': r['NrFact'] or ''} for r in rows]


def source_supplier(cursor, ctx, source_id):
    """The fiscal code of the receipt a storno invoice reverses (an FCS has no supplier of its own)."""
    rows = _all(cursor, 'SELECT CodPartener FROM VC_NIR WHERE SubunitId=%s AND GestiuneId=%s AND ExercitiuId=%s AND IdIntrare=%s', (*_scope(ctx), source_id))
    return rows[0]['CodPartener'] if rows else None


def check(cursor, ctx, family, args):
    """The answer of GET /api/vercon/doc/<family>/invoice-twins: {'twins': [...]}. `args` = the query string."""
    if family not in ('nir', 'storno'):
        raise NomError('FAMILY_UNKNOWN', 'Acest document nu are număr de factură.', 404)
    try:
        exclude = int(args.get('exclude')) if args.get('exclude') else None
        source = int(args.get('source')) if args.get('source') else None
    except ValueError:
        raise NomError('INVALID', 'Cerere invalidă.', 400) from None
    fiscal_code = source_supplier(cursor, ctx, source) if family == 'storno' else args.get('cf')
    return {'twins': find(cursor, ctx, fiscal_code, args.get('number'), exclude)}
