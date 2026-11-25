"""SLICE-VC16-04: the single validator and allocator of NIR/FCS numbers."""
import re

from . import documents as d, protocol
from .nomenclatoare import NomError


def _error(code, message, field='h.Numar'):
    raise NomError(code, message, 422, {field: message})


def sub_number(raw, field='h.Numar'):
    value = 0 if raw in (None, '') else d._int(raw)
    if value is None or not 0 <= value <= 99:
        _error('INVALID', 'Subnumărul trebuie să fie între 0 și 99.', field)
    return value


def text(pair):
    return '%s%s' % (pair[0], d._sub_text(pair[1]))


def series(cursor, ctx, day):
    """Live NIR/FCS of the calendar year; opening stock does not define the series."""
    return d._all(cursor, 'SELECT IdIntrare, ABS(Numar) AS Numar, COALESCE(NumarSub,0) AS sub, DATE(Data) AS day, COALESCE(Storno,0) AS storno FROM VC_NIR '
                  "WHERE SubunitId=%s AND GestiuneId=%s AND YEAR(Data)=%s AND Numar<>0 AND TipDoc<>'STOC_INITIAL' AND Anulat=0",
                  (ctx['subunit'], ctx['gestiune'], day.year))


def last_number(cursor, ctx, day):
    return max((row['Numar'] for row in series(cursor, ctx, day)), default=0)


def _neighbors(rows, day):
    earlier = [row for row in rows if row['day'] and row['day'] < day]
    later = [row for row in rows if row['day'] and row['day'] > day]
    previous_day = max((row['day'] for row in earlier), default=None)
    next_day = min((row['day'] for row in later), default=None)
    lower = max(((row['Numar'], row['sub']) for row in earlier if row['day'] == previous_day), default=None)
    upper = min(((row['Numar'], row['sub']) for row in later if row['day'] == next_day), default=None)
    return lower, upper, previous_day, next_day


def _in_range(pair, lower, upper):
    return (lower is None or lower < pair) and (upper is None or pair < upper)


def _range_error(lower, upper, field):
    limits = []
    if lower is not None:
        limits.append('mai mare decât %s' % text(lower))
    if upper is not None:
        limits.append('mai mic decât %s' % text(upper))
    _error('NIR_CHRONOLOGY', 'Numărul trebuie să fie %s.' % ' și '.join(limits), field)


def suggestion(rows, day, whole=False, field='h.Numar'):
    """Use a free whole number first, then a fraction between the nearest dates."""
    lower, upper, _, _ = _neighbors(rows, day)
    occupied = {(row['Numar'], row['sub']) for row in rows}
    same_day = [(row['Numar'], row['sub']) for row in rows if row['day'] == day]
    anchor = max([*same_day, *([lower] if lower is not None else [])], default=None)
    if anchor is not None and upper is not None and anchor >= upper:
        anchor = lower
    start = anchor[0] + 1 if anchor is not None else max(1, upper[0] - 1) if upper is not None else 1
    number = start
    for used in sorted(value for value, fraction in occupied if fraction == 0 and value >= start):
        if used > number:
            break
        if used == number:
            number += 1
    if _in_range((number, 0), lower, upper):
        return number, 0
    if not whole:
        base = anchor[0] if anchor is not None else max(1, (upper or (1, 0))[0] - 1)
        bases = sorted({base, *(value for value, _ in occupied if value >= base and (upper is None or value <= upper[0]))})
        fractions = sorted(range(1, 100), key=lambda fraction: (fraction % 10 != 0, fraction))
        for number in bases:
            for fraction in fractions:
                pair = (number, fraction)
                if _in_range(pair, anchor, upper) and _in_range(pair, lower, upper) and pair not in occupied:
                    return pair
    _error('NIR_CHRONOLOGY', 'Nu există număr liber între documentele vecine.', field)


def check(cursor, ctx, raw=None, exclude=None, sub=0, on=None, *, whole=False, field='h.Numar'):
    day = protocol.to_date(on or '%s-12-31' % ctx['year'], 'Data NIR')
    if exclude:
        previous = d._one(cursor, 'SELECT DATE(Data) AS day FROM VC_NIR WHERE SubunitId=%s AND GestiuneId=%s AND ExercitiuId=%s AND IdIntrare=%s',
                          (*d._scope(ctx), exclude))
        if previous and previous['day'] and previous['day'].year != day.year:
            _error('NIR_YEAR', 'Data trebuie să rămână în anul documentului.', 'h.Data')
    rows = [row for row in series(cursor, ctx, day) if row['IdIntrare'] != exclude]
    lower, upper, previous_day, next_day = _neighbors(rows, day)
    fraction = sub_number(sub, field)
    if raw in (None, ''):
        if fraction:
            _error('INVALID', 'Lipsește numărul NIR.', field)
        number, fraction = suggestion(rows, day, whole, field)
    else:
        number = d._int(raw)
        if number is None or number < 1:
            _error('INVALID', 'Numărul trebuie să fie un întreg pozitiv.', field)
    if whole and fraction:
        _error('INVALID', 'Numărul trebuie să fie întreg.', field)
    pair = (number, fraction)
    if any((row['Numar'], row['sub']) == pair for row in rows):
        _error('DUPLICATE', 'Numărul %s este deja folosit.' % text(pair), field)
    if not _in_range(pair, lower, upper):
        _range_error(lower, upper, field)
    if fraction and not d._numarsub_column(cursor, ctx):
        _error('INVALID', 'Zecimalele nu sunt disponibile.', field)
    return {'suggested': number, 'suggested_sub': fraction, 'number_locked': False,
            'valid': True, 'duplicate': False, 'reserved': False,
            'minimum': previous_day.isoformat() if previous_day else None,
            'maximum': next_day.isoformat() if next_day else None}


def assign_pair(cursor, ctx, raw, sub, day, exclude=None, *, whole=False, field='h.Numar'):
    d._one(cursor, 'SELECT GestiuneId FROM VC_Gestiuni WHERE SubunitId=%s AND GestiuneId=%s FOR UPDATE',
           (ctx['subunit'], ctx['gestiune']))
    result = check(cursor, ctx, raw, exclude, sub, day.isoformat(), whole=whole, field=field)
    return result['suggested'], result['suggested_sub']


def assign(cursor, ctx, raw, sub, day, exclude=None):
    """Whole-number allocation for callers whose document has no fraction field."""
    number, _ = assign_pair(cursor, ctx, raw, sub, day, exclude, whole=True)
    return number


def assign_text(cursor, ctx, raw, day, exclude=None, current=None):
    """The popup submits one display value; all number checks remain here."""
    match = re.fullmatch(r'([1-9][0-9]*)(?:[,.]([0-9]{1,2}))?', str(raw or '').strip())
    if not match:
        _error('INVALID', 'Număr invalid. Exemplu: 80 sau 80,1.', 'number')
    pair = (int(match[1]), int((match[2] or '').ljust(2, '0')))
    if pair == current:
        _error('NO_CHANGE', 'Numărul nu a fost schimbat.', 'number')
    return assign_pair(cursor, ctx, pair[0], pair[1], day, exclude, field='number')
