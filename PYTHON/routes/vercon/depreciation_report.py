"""VC19-01: classification label and per-classification summary for the depreciation reports.

The classification is ProduseP/Produse.CC (the MF classification code), named from
AVACONT_COMUN.VC_ClasificariMF. Read-only; amounts stay exact decimal text.
"""
from decimal import Decimal, localcontext

NO_CLASSIFICATION = 'Fără clasificare'
AMOUNTS = ('value', 'monthly', 'month_depreciation', 'accumulated', 'remaining')


def names(cursor):
    """{code: name} for the whole MF classification catalogue."""
    cursor.execute('SELECT Cod, Denumire FROM AVACONT_COMUN.VC_ClasificariMF WHERE Cod IS NOT NULL')
    return {str(row['Cod']): row['Denumire'] or '' for row in cursor.fetchall()}


def label(code, catalogue):
    """'code / name' as in the report selection tree; never an invented classification."""
    code = '' if code is None else str(code).strip()
    if not code:
        return NO_CLASSIFICATION
    name = catalogue.get(code, '')
    return code + ' / ' + name if name else code


def summarize(rows):
    """One row per classification. A missing amount (invalid asset) stays missing in the sum."""
    groups = {}
    with localcontext() as context:
        context.prec = 60
        for row in rows:
            target = groups.setdefault(row['classification'], {
                'classification': row['classification'], 'count': 0, **{key: Decimal(0) for key in AMOUNTS}})
            target['count'] += 1
            for key in AMOUNTS:
                value = row.get(key)
                target[key] = None if value is None or target[key] is None else target[key] + Decimal(str(value))
        return [{**item, **{key: None if item[key] is None else format(item[key], 'f') for key in AMOUNTS}}
                for _, item in sorted(groups.items(), key=lambda pair: pair[0].casefold())]
