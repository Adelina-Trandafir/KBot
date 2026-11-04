"""SLICE-AD18-01: read-only reconciliation of consecutive monthly situations."""
import unicodedata
from datetime import date, timedelta
from decimal import Decimal
from .domain import require
from .receipt_output import issuer_data


def _romanian_key(value):
    alphabet = 'a\u0103\u00e2bcdefghi\u00eejklmnopqrs\u0219t\u021buvwxyz'
    text = unicodedata.normalize('NFC', str(value or '').casefold())
    text = text.replace('\u015f', '\u0219').replace('\u0163', '\u021b')
    return tuple((1, alphabet.index(char)) if char in alphabet else (0, ord(char)) for char in text)


def _by_child(rows):
    indexed = {}
    for row in rows:
        child = row.get('IDP')
        require(child is not None and child not in indexed, 'COMPARISON_ROWS',
                'Situația conține rânduri fără copil sau repetate pentru același copil. Comparația nu poate fi generată.', 409)
        indexed[child] = row
    return indexed


def _differences(current, previous):
    if current is None:
        return 'Lipsește din luna curentă'
    if previous is None:
        return 'Lipsește din luna anterioară'
    return '; '.join(f'{initial} ≠ {final}' for initial, final in (('SID', 'SFD'), ('SIC', 'SFC'))
                     if Decimal(str(current.get(initial) or 0)) != Decimal(str(previous.get(final) or 0)))


def _pair_key(pair):
    row = pair[0] if pair[0] is not None else pair[1]
    return _romanian_key(row.get('Nume')), str(row.get('CNP') or '')


def _comparison_table(pairs, current_month, previous_month):
    from .reports import _situation_table, col
    base_columns, _ = _situation_table([])
    columns = [base_columns[0], col('Luna', 1100, 'c', key='LUNA'), col('Grupa', 1500, key='GRUPA'),
               *base_columns[1:], col('Diferență / lipsă', 1900, key='DIFERENTA')]
    body = []
    for number, (current, previous, difference) in enumerate(sorted(pairs, key=_pair_key), 1):
        reference = current if current is not None else previous
        for row, month, label in ((current, current_month, 'Curentă'), (previous, previous_month, 'Anterioară')):
            if row is None:
                cells = [str(reference.get('Nume') or ''), str(reference.get('CNP') or ''),
                         *[''] * (len(base_columns) - 3)]
            else:
                _, formatted = _situation_table([row])
                cells = formatted[0]['cells'][1:]
            period = f"{month['Luna']:02d}/{month['Anul']}\n{label}" + ('\nLIPSĂ' if row is None else '')
            body.append({'cells': [str(number), period, str(row.get('Grupa') or '') if row is not None else '',
                                   *cells, difference]})
    return {'t': 'table', 'cols': columns, 'rows': body}


def month_comparison_report(repo, unit, args):
    from .reports import need_int, opt_int, situation_rows, month_label, letterhead, model
    month_id, group_id = need_int(args, 'month'), opt_int(args, 'group')
    everything, split = args.get('all') == '1', args.get('split') == '1'
    require(everything or group_id is not None, 'FILTER', 'Alegeți grupa sau bifați «Toate grupele».', 400)
    # Read both entire months before applying the group filter, so a transfer keeps its counterpart.
    current_month, current_rows = situation_rows(repo, month_id, None)
    previous_date = date(current_month['Anul'], current_month['Luna'], 1) - timedelta(days=1)
    previous_months = repo.rows('LunaD', Anul=previous_date.year, Luna=previous_date.month)
    require(bool(previous_months), 'PREVIOUS_MONTH',
            f"Luna calendaristică anterioară ({previous_date:%m/%Y}) nu există în evidența subunității.", 404)
    previous_month, previous_rows = situation_rows(repo, previous_months[0]['IDL'], None)
    current, previous = _by_child(current_rows), _by_child(previous_rows)
    pairs = []
    for child in current.keys() | previous.keys():
        now, before = current.get(child), previous.get(child)
        if not everything and not any(row is not None and row.get('IDG') == group_id for row in (now, before)):
            continue
        difference = _differences(now, before)
        if difference:
            pairs.append((now, before, difference))
    require(bool(pairs), 'NO_DATA',
            'Nu există diferențe SID/SIC față de SFD/SFC și nici rânduri lipsă între cele două luni pentru selecția aleasă.', 404)
    issuer = issuer_data(repo, unit)
    changed = sum(now is not None and before is not None for now, before, _ in pairs)
    missing_current = sum(now is None for now, _, _ in pairs)
    missing_previous = sum(before is None for _, before, _ in pairs)
    heading = [letterhead(issuer, f'{month_label(current_month)} / {month_label(previous_month)}'),
               {'t': 'title', 'text': 'Diferențe între luni consecutive'},
               {'t': 'text', 'text': 'Comparație exclusivă: SID curent cu SFD anterior; SIC curent cu SFC anterior. '
                'Fiecare pereche: luna curentă sus, luna anterioară jos.', 'align': 'center'},
               {'t': 'text', 'text': f'Perechi cu solduri diferite: {changed}; lipsă în luna curentă: {missing_current}; '
                f'lipsă în luna anterioară: {missing_previous}.', 'align': 'center'}]
    if not everything:
        group = repo.get('Grupe', group_id)
        scope = f"Grupa: {group.get('Grupa') or ''} (în oricare dintre luni)"
    else:
        scope = 'TOATE GRUPELE'
    if not everything or not split:
        blocks = [*heading, {'t': 'text', 'text': scope, 'align': 'center', 'bold': True},
                  _comparison_table(pairs, current_month, previous_month)]
    else:
        groups = {}
        for pair in pairs:
            row = pair[0] if pair[0] is not None else pair[1]
            groups.setdefault(row.get('IDG'), []).append(pair)
        ordered = sorted(groups.values(), key=lambda members: _romanian_key(
            (members[0][0] if members[0][0] is not None else members[0][1]).get('Grupa')))
        blocks = []
        for index, members in enumerate(ordered):
            row = members[0][0] if members[0][0] is not None else members[0][1]
            if index:
                blocks.append({'t': 'break'})
            blocks.extend([*heading, {'t': 'text', 'text': str(row.get('Grupa') or ''), 'align': 'center', 'bold': True},
                           _comparison_table(members, current_month, previous_month)])
    return model('Diferențe între luni consecutive',
                 f"Diferente_luni_{current_month['Anul']}_{current_month['Luna']:02d}", blocks, landscape=True, font=7)
