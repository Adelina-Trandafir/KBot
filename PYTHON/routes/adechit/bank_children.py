"""Read-only catalog children with the selected month's opening debit/credit balances."""
from .calculations import calculate
from .domain import require


def children(repo, group_id, month_id):
    repo.get('Grupe', group_id)
    month = repo.get('LunaD', month_id)
    rows = repo.rows('Platitori', IDG=group_id)
    if not rows:
        return {'rows': [], 'month_id': month_id}
    identities = {row['IDP'] for row in rows}
    balances = {}
    if month['Inchisa']:
        for snapshot in repo.rows('SS_Buget', IDL=month_id):
            identity = snapshot['IDP']
            if identity in identities:
                require(identity not in balances, 'BALANCE_AMBIGUOUS',
                        'Copilul are mai multe situații salvate în luna aleasă. Corectați evidența.')
                balances[identity] = {key: snapshot[key] for key in ('SID', 'SIC')}
    pending = identities - balances.keys()
    if pending:
        data = repo.data()
        data['Platitori'] = [row for row in rows if row['IDP'] in pending]
        current = [row for row in data['Prezenta'] if row['IDL'] == month_id]
        for index, child in enumerate(data['Platitori']):
            matches = [row for row in current if row['IDP'] == child['IDP']]
            require(len(matches) <= 1, 'ATTENDANCE_AMBIGUOUS',
                    'Copilul are mai multe înregistrări lunare. Corectați evidența.')
            if not matches:
                # A calculation-only row exposes SID/SIC without creating attendance or payments.
                data['Prezenta'].append({'IDZ': -index - 1, 'IDP': child['IDP'], 'IDG': child['IDG'],
                                        'IDL': month_id, 'IDV': month.get('IDV'), 'ZilePrezenta': 0,
                                        'ValoareContract': 0, 'ValoareTotala': 0})
        calculated, _ = calculate(data, month_id)
        balances.update({row['IDP']: {key: row[key] for key in ('SID', 'SIC')} for row in calculated})
    result = []
    for row in rows:
        balance = balances.get(row['IDP'], {})
        require(all(balance.get(key) is not None and balance[key] >= 0 for key in ('SID', 'SIC')), 'BALANCE_MISSING',
                'Soldurile SID/SIC ale copilului nu sunt disponibile pentru luna aleasă. Reîncărcați datele sau corectați evidența.')
        result.append({**row, **balance})
    return {'rows': result, 'month_id': month_id}
