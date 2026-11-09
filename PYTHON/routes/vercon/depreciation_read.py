"""VC08-02: reproducible read contract, exact text amounts and per-asset diagnostics."""
from . import bu_mf, depreciation, depreciation_source
from .stock import quantity_text

AMOUNTS = ('value', 'monthly', 'accumulated', 'remaining', 'month_depreciation',
           'next_month_depreciation', 'written_off_remaining')


def read(cursor, ctx, on_date, variant=None):
    assets, events = depreciation_source.load(cursor, ctx, variant)
    transfers = bu_mf.by_variant(cursor, ctx)
    rows = []
    totals = {field: 0 for field in AMOUNTS}
    incomplete = []
    excluded = []
    for key, asset in assets.items():
        movements, zec = events.get(key, []), ctx['zec']
        if key in transfers:
            if not bu_mf.owns(ctx, key, transfers[key], on_date):
                if variant is not None:
                    raise NomError('MF_OUTSIDE', 'Activul nu aparține gestiunii la data aleasă.', 404)
                continue
            movements, zec = bu_mf.acquisition(cursor, ctx, key, transfers[key], movements)
        result = depreciation.calculate(asset, movements, on_date, ctx['scale'], zec)
        if result['status'] == 'invalid':
            incomplete.append(asset['IdProdus'])
        elif result['status'] in ('not_entered', 'not_in_use'):
            excluded.append(asset['IdProdus'])
        else:
            for field in AMOUNTS:
                totals[field] += result[field] or 0
        rows.append({'id': asset['IdProdus'], 'row_id': asset['RowId'], 'name': asset['name'], 'code': asset['code'],
                     'um': asset['um'], 'category': asset['category'], 'account': asset['account'],
                     'entry_date': _iso(asset['DataIntrare']), 'DNF': asset['DNF'], 'DF': asset['DF'],
                     'version': asset['Version'], 'price_version': asset['price_version'],
                     'source_monthly': {'ValoareLuna': _money(asset['ValoareLuna'], ctx['scale']),
                                        'ValoareLunara': _money(asset['ValoareLunara'], ctx['scale'])},
                     **result, **{field: _money(result[field], ctx['scale']) for field in AMOUNTS}})
    return {'date': on_date.isoformat(), 'scale': ctx['scale'], 'zec': ctx['zec'], 'assets': rows,
            'basis': 'current_ProduseP.PUTVA', 'parameter_mode': 'current_register',
            'rounding': 'HALF_EVEN_calculation_HALF_UP_display',
            'complete': not incomplete, 'invalid_assets': incomplete, 'excluded_assets': excluded,
            'totals_scope': 'calculated_assets',
            'calculated_count': len(rows) - len(incomplete) - len(excluded),
            'totals': {field: quantity_text(value, ctx['scale']) for field, value in totals.items()}}


def _money(value, scale):
    return None if value is None else quantity_text(value, scale)


def _iso(value):
    day = depreciation.as_date(value)
    return day.isoformat() if day else None
