"""Calendar-year receipt series. Cancelled numbers retain their positions."""
from . import documents as d, protocol
from .nomenclatoare import NomError


def series(cursor, ctx, day):
    return d._all(cursor, 'SELECT IdIntrare, Numar, COALESCE(NumarSub,0) AS sub FROM VC_NIR '
                  "WHERE SubunitId=%s AND GestiuneId=%s AND YEAR(Data)=%s AND COALESCE(Storno,0)=0 AND Numar>0 AND TipDoc<>'STOC_INITIAL'",
                  (ctx['subunit'], ctx['gestiune'], day.year))


def check(cursor, ctx, raw=None, exclude=None, sub=0, on=None):
    day = protocol.to_date(on, 'Data NIR') if on else protocol.to_date('%s-12-31' % ctx['year'], 'Data NIR')
    rows = series(cursor, ctx, day)
    proposed = max((r['Numar'] for r in rows), default=0) + 1
    manual = d._int(raw)
    empty = raw in (None, '')
    valid = (empty and not sub) or manual is not None and manual > 0 and 0 <= sub <= 99
    duplicate = bool(valid and not empty and any(r['IdIntrare'] != exclude and (r['Numar'], r['sub']) == (manual, sub) for r in rows))
    bounds = {}
    if valid and not duplicate:
        from .receipt_rules import chronology
        bounds = chronology(cursor, ctx, proposed if empty else manual, sub, day, exclude)
    return {'suggested': proposed, 'valid': valid, 'duplicate': duplicate, 'reserved': False, **bounds}


def assign(cursor, ctx, raw, sub, day, exclude=None):
    d._one(cursor, 'SELECT GestiuneId FROM VC_Gestiuni WHERE SubunitId=%s AND GestiuneId=%s FOR UPDATE',
           (ctx['subunit'], ctx['gestiune']))
    result = check(cursor, ctx, raw, exclude, sub, day.isoformat())
    if not result['valid'] or result['duplicate']:
        message = 'Numărul NIR este invalid sau deja folosit în acest an.'
        raise NomError('INVALID', message, 422, {'h.Numar': message})
    return result['suggested'] if raw in (None, '') else d._int(raw)
