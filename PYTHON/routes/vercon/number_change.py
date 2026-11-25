"""Change the number of a saved voucher or food list in its own series.

Two rules decide it, both checked again here on save (the menu only hints):

  * month   vouchers must be dated in the current calendar month; food lists need an open exercise;
  * order   the numbers keep following the dates of their series: every document with a smaller number has a date on or
            before this one, every document with a bigger number a date on or after it (the same day is free). The number
            must also be free in the series.

The series of a BC is the whole gestiune/exercise, or ONE account when the gestiune numbers its BC per account
(consumption_numbering.per_account). BP / BT / BS have one series per type, gestiune and exercise (as oi_number.check).
A BP also owns a BC row in VC_Iesiri_H that carries the same text; both change together. Nothing but the number moves.
"""
from utils.clock import bucharest_today

from . import bon_number, consumption_numbering, protocol
from .nomenclatoare import NomError

TABLES = {   # family -> (table, document key, date column, number column)
    'bc': ('VC_Iesiri_H', 'IdBonConsum', 'DataBC', 'NumarBC'),
    'br': ('VC_Iesiri_H', 'IdBonConsum', 'DataBC', 'NumarBC'),   # VC20-02: the return bon (BX) is a row of VC_Iesiri_H with Retur=1
    'bp': ('VC_OI_Predare', 'IDBP', 'DATABP', 'NRBP'),
    'bt': ('VC_OI_Transfer', 'IDBT', 'DATABT', 'NRBT'),
    'bs': ('VC_OI_Casare', 'IDBC', 'DATABC', 'NRBC'),
    'lista': ('VC_LABC', 'IdLista', 'DATALA', 'NumarLA'),
}
FAMILIES = tuple(TABLES)


def _current_month():
    return bucharest_today().isoformat()[:7]


def _month_text(month):
    return '%s.%s' % (month[5:7], month[:4])


def month_ok(day):
    return bool(day) and str(day)[:7] == _current_month()


def month_message():
    return 'Numărul se poate modifica numai la documentele din luna în curs (%s).' % _month_text(_current_month())


def _series(cursor, ctx, family, account):
    """[(id, number, day)] of the live documents of the series; the unreadable numbers (another kind) are left out."""
    from .documents import _all, _scope
    table, key, day, number = TABLES[family]
    if family == 'lista':
        return [(row['id'], row['number'] if (row['number'] or 0) > 0 else None, row['day']) for row in _all(cursor,
                'SELECT IdLista AS id, DATE(DATALA) AS day, NumarLA AS number FROM VC_LABC '
                'WHERE SubunitId=%s AND GestiuneId=%s AND ExercitiuId=%s AND Anulat=0', _scope(ctx))]
    if family in ('bc', 'br'):
        sql = ('SELECT {k} AS id, DATE({d}) AS day, {n} AS number FROM {t} WHERE SubunitId=%s AND GestiuneId=%s AND ExercitiuId=%s '
               'AND Anulat=0 AND COALESCE(Retur,0)={r}').format(k=key, d=day, n=number, t=table, r=1 if family == 'br' else 0)
        params = list(_scope(ctx))
        if account is not None:
            sql += ' AND IdMasina=%s'
            params.append(account)
    else:
        sql = ('SELECT {k} AS id, MIN(DATE({d})) AS day, MIN({n}) AS number FROM {t} WHERE SubunitId=%s AND GestiuneId=%s AND ExercitiuId=%s '
               'AND Anulat=0 AND {k} IS NOT NULL AND {k}<>0 GROUP BY {k}').format(k=key, d=day, n=number, t=table)
        params = list(_scope(ctx))
    out = []
    for row in _all(cursor, sql, params):
        parsed = bon_number.parse(family, row['number'])
        if parsed is not None and (row['day'] or family == 'br'):
            out.append((row['id'], parsed, row['day']))
    return out


def allowed(rows, doc_id, day):
    """(lowest, highest) number that keeps the numbers in the order of the dates; highest is None when nothing is above."""
    others = [(number, when) for ident, number, when in rows if ident != doc_id and number is not None and when]
    low = max([number for number, when in others if when < day], default=0) + 1
    later = [number for number, when in others if when > day]
    return low, (min(later) - 1 if later else None)


def _range_text(low, high):
    if high is None:
        return 'cel puțin %d' % low
    if high < low:
        return 'într-un interval liber (ordinea numerelor existente nu mai lasă niciun număr pentru această dată)'
    return 'exact %d' % low if low == high else 'între %d și %d' % (low, high)


def return_number(cursor, ctx, day, raw=None):
    """Choose or validate a whole number for a new BX in the existing return series."""
    from .documents import _int

    rows = _series(cursor, ctx, 'br', None)
    low, high = allowed(rows, None, day)
    occupied = {number for _, number, _ in rows}
    if raw in (None, ''):
        number = low
        while number in occupied and (high is None or number <= high):
            number += 1
        if high is not None and number > high:
            message = 'Nu există un număr BX liber pentru data returului.'
            raise NomError('NUMBER_CHRONOLOGY', message, 422, {'h.Numar': message})
        return number

    number = _int(raw)
    if number is None or number < 1:
        message = 'Numărul trebuie să fie un întreg pozitiv.'
        raise NomError('INVALID', message, 422, {'h.Numar': message})
    if number in occupied:
        message = 'Numărul %s este deja folosit.' % bon_number.text('br', number)
        raise NomError('DUPLICATE', message, 422, {'h.Numar': message})
    if number < low or (high is not None and number > high):
        message = 'Pentru un retur din %s, numărul BX trebuie să fie %s.' % (day.strftime('%d.%m.%Y'), _range_text(low, high))
        raise NomError('NUMBER_CHRONOLOGY', message, 422, {'h.Numar': message})
    return number


def annotate(cursor, ctx, family, rows):
    """Mark documents that can offer the number action; the series is checked on save."""
    if family not in TABLES:
        return rows
    for row in rows:
        reason = ''
        if row.get('annulled'):
            reason = 'Documentul este anulat.'
        elif family == 'lista':
            if not (row.get('date') or (row.get('header') or {}).get('Data')):
                reason = 'Lista nu are o dată înregistrată.'
        elif bon_number.parse(family, row.get('number')) is None:
            reason = 'Numărul acestui document nu poate fi modificat de aici.'
        elif not month_ok(row.get('date')):
            reason = month_message()
        row['number_change'] = not reason
        row['number_change_message'] = reason
    return rows


def change(cursor, repo, ctx, family, doc_id, body, request_id):
    from .archive import release_renumbered
    from .documents import _int
    table, key, _, column = TABLES[family]
    if family in ('bc', 'br', 'lista'):
        from .documents import _load_for_write
        _, _, before = _load_for_write(repo, ctx, family, doc_id, body.get('version'))
        old_text, account, bon_id = before[column], before.get('IdMasina'), None
    else:
        from . import oi
        rows = oi._load(cursor, repo, ctx, family, doc_id, body.get('version'))
        before = rows[0]
        old_text, account, bon_id = before[column], None, before.get('IdBonConsum') if family == 'bp' else None
    old = old_text if family == 'lista' else bon_number.parse(family, old_text)
    if old is None and family != 'lista':
        raise NomError('NUMBER_KIND', 'Numărul acestui document nu poate fi modificat de aici.', 409)
    number = _int(body.get('number'))
    if number is None or number < 1:
        message = 'Numărul trebuie să fie un întreg pozitiv.'
        raise NomError('INVALID', message, 422, {'number': message})
    if family == 'lista' and number > 2_147_483_647:
        message = 'Numărul listei poate avea cel mult valoarea 2147483647.'
        raise NomError('INVALID', message, 422, {'number': message})
    scoped = account if family == 'bc' and account is not None and consumption_numbering.per_account(cursor, ctx) else None
    series = _series(cursor, ctx, family, scoped)
    own = next((row for row in series if row[0] == doc_id), None)
    if own is None:
        raise NomError('NOT_FOUND', 'Documentul nu mai există.', 404)
    day = own[2]
    if family == 'lista' and day is None:
        raise NomError('NUMBER_DATE', 'Lista nu are o dată înregistrată.', 422)
    if family != 'lista' and not month_ok(day):
        raise NomError('NUMBER_MONTH', month_message(), 422, {'number': month_message()})
    if number == old:
        message = 'Numărul este același. Introduceți alt număr.'
        raise NomError('NO_CHANGE', message, 422, {'number': message})
    if any(ident != doc_id and value == number for ident, value, _ in series):
        shown = number if family == 'lista' else bon_number.text(family, number)
        message = 'Numărul %s este deja folosit.' % shown
        raise NomError('DUPLICATE', message, 422, {'number': message})
    low, high = allowed(series, doc_id, day)
    if number < low or (high is not None and number > high):
        message = 'Numerele trebuie să urmeze ordinea datelor. Pentru un document din %s numărul trebuie să fie %s.' % (
            day.strftime('%d.%m.%Y'), _range_text(low, high))
        raise NomError('NUMBER_CHRONOLOGY', message, 422, {'number': message})
    text = number if family == 'lista' else bon_number.text(family, number)
    if family in ('bc', 'br', 'lista'):
        if repo.update(table, {column: text}, {key: doc_id, 'Version': before['Version']}, bump_version=True) != 1:
            raise NomError('CONFLICT', 'Documentul a fost modificat de altcineva. Reîncărcați documentul.', 409)
    else:
        repo.update(table, {column: text}, {key: doc_id}, bump_version=True)   # every line of the document carries the number
        if bon_id:   # the BC a BP owns shows the same text
            repo.update('VC_Iesiri_H', {'NumarBC': text}, {'IdBonConsum': bon_id}, bump_version=True)
    if family == 'bc':
        kind = consumption_numbering.kind(scoped)
    elif family == 'br':
        from .documents import _kind
        kind = _kind('br')
    elif family == 'lista':
        kind = 'LISTA'
    else:
        from .oi import SPEC
        kind = SPEC[family]['kind']
    protocol.raise_counter(cursor, ctx, kind, number)
    release_renumbered(cursor, ctx, family, before, text)
    protocol.audit(cursor, ctx, family + '.number', table, doc_id, {'number': old_text}, {'number': text}, None, request_id)
    return {'id': doc_id, 'number': text}
