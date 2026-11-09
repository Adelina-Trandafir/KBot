"""Shared warehouse validation for saved movements and document drafts.

Older movements contribute to opening stock. Daily validation begins in the work month.
"""
from datetime import date, timedelta
from decimal import Decimal, localcontext
from itertools import chain

from . import consumption_period, protocol
from .nomenclatoare import NomError


def work_month_start(cursor, ctx, on, previous_day=None):
    """Use the active consumption month of this ledger, or the document month if empty."""
    from .stock import day_of

    month = consumption_period.active_month(cursor, ctx, in_exercise=True)
    if month:
        return date.fromisoformat(month + '-01')
    dates = [day_of(day) for day in (on, previous_day) if day is not None]
    return min(dates, default=date.min).replace(day=1)


def require_nonnegative(cursor, ctx, variants, month_start, *, exclude=None, draft=()):
    """Check a saved ledger or replace one saved document with signed draft movements."""
    from .stock import day_of

    variants = {int(variant) for variant in variants if variant is not None}
    if not variants:
        return
    stored_variants = {variant for variant in variants if variant >= 0}
    movements = protocol.movements(cursor, ctx, stored_variants, exclude) if stored_variants else ()
    opening = {variant: Decimal(0) for variant in variants}
    current = {variant: [] for variant in variants}
    with localcontext() as precision:
        precision.prec = 60
        for variant, raw_day, quantity in chain(movements, draft):
            if variant not in variants:
                continue
            day = day_of(raw_day)
            if day < month_start:
                opening[variant] += quantity
            else:
                current[variant].append((day, quantity))
        for variant in sorted(variants):
            balance = opening[variant]
            if balance < 0:
                _reject(cursor, ctx, variant, month_start - timedelta(days=1), balance, month_start, opening=True)
            for day, quantity in by_day(current[variant]):
                balance += quantity
                if balance < 0:
                    _reject(cursor, ctx, variant, day, balance, month_start)


def by_day(items):
    """Yield each day's additions before its removals."""
    days = {}
    for day, quantity in items:
        entry = days.setdefault(day, [0, 0])
        entry[0 if quantity > 0 else 1] += quantity
    for day in sorted(days):
        adds, removes = days[day]
        yield day, adds
        yield day, removes


def _reject(cursor, ctx, variant, day, balance, month_start, *, opening=False):
    label = protocol._label(cursor, ctx, variant) if variant >= 0 else 'din ciornă'
    quantity = protocol._plain(balance, ctx['scale'])
    day_text = day.strftime('%d.%m.%Y')
    if opening:
        message = ('Stocul la sfârșitul lunii anterioare este negativ pentru produsul %s: %s la data %s. '
                   'Corectați soldul de pornire înainte de a continua.') % (label, quantity, day_text)
    else:
        message = ('Stoc negativ interzis: produsul %s ar ajunge la %s la data %s. '
                   'Calculul pornește de la stocul ultimei luni închise.') % (label, quantity, day_text)
    raise NomError('STOCK_NEGATIVE', message, 409,
                   {'product': variant, 'date': day.isoformat(), 'missing': protocol._plain(-balance, ctx['scale']),
                    'stock_period_start': month_start.isoformat()})
