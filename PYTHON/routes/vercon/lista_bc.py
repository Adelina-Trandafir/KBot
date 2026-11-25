"""SLICE-VC07-01 (rules only): the set of consumption vouchers (BC) of one food list, 1:N.

In the Access databases a list had exactly one BC; the web system lets a list have several (new feature). The rules:
a BC belongs to ONE list (unique key uq_LegBCLA_IdBonConsum) and appears once in it; only BCs of the current gestiune
can be chosen (the ScopedRepository carries the context); a cancelled BC cannot be added.

set_bonuri() replaces the whole set in one go and does NOT commit: the caller owns the transaction, so the header, the
meals and the whole LegBCLA set are saved together or not at all (a failure rolls everything back). It never creates or
re-saves a BC and never repeats the consumption.
"""
from datetime import datetime

from . import bon_number


class ListaBcError(Exception):
    """A rule of the list/BC association was broken; the message is the sentence the operator reads."""


def _ids(bon_ids):
    seen, out = set(), []
    for b in bon_ids:
        if isinstance(b, bool) or not isinstance(b, int) or b <= 0:
            raise ListaBcError('Bon de consum invalid.')
        if b in seen:
            raise ListaBcError('Bonul de consum %s este ales de doua ori.' % b)
        seen.add(b)
        out.append(b)
    return out


def current(repo, id_lista):
    """BC ids now linked to the list."""
    return [r['IdBonConsum'] for r in repo.select('VC_LegBCLA', {'IdLista': id_lista}, ('IdBonConsum',)) if r['IdBonConsum'] is not None]


def calendar_day(value):
    return value.date() if isinstance(value, datetime) else value


def _bon(repo, bon_id):
    rows = repo.select('VC_Iesiri_H', {'IdBonConsum': bon_id},
                       ('NumarBC', 'DataBC', 'Anulat', 'Retur'), for_update=True)
    if not rows:
        raise ListaBcError('Bonul de consum %s nu există în această gestiune.' % bon_id)
    return rows[0]


def _require_date(bon, list_day):
    day = calendar_day(bon['DataBC'])
    label = bon_number.display('bc', bon['NumarBC']) or 'Bonul de consum'
    if day is None:
        raise ListaBcError('%s nu are o dată validă.' % label)
    if day > list_day:
        raise ListaBcError('%s din %s este mai nou decât lista din %s.'
                           % (label, day.strftime('%d.%m.%Y'), list_day.strftime('%d.%m.%Y')))


def require_dates(repo, bon_ids, list_day):
    for bon_id in bon_ids:
        bon = _bon(repo, bon_id)
        if not bon['Anulat']:
            _require_date(bon, list_day)


def set_bonuri(repo, id_lista, bon_ids, list_day, *, validate_retained=False):
    """Make the BC set of `id_lista` exactly `bon_ids`. Returns {'added': [...], 'removed': [...]}. Caller commits/rolls back."""
    wanted = _ids(bon_ids)
    if not repo.select('VC_LABC', {'IdLista': id_lista}, ('IdLista',), for_update=True):
        raise ListaBcError('Lista nu exista.')
    have = set(current(repo, id_lista))
    added = [b for b in wanted if b not in have]
    removed = sorted(have - set(wanted))
    if validate_retained:
        require_dates(repo, (b for b in wanted if b in have), list_day)
    for b in added:
        bon = _bon(repo, b)
        if bon['Anulat']:
            raise ListaBcError('Bonul de consum %s este anulat.' % b)
        if bon['Retur']:
            raise ListaBcError('Un bon de retur nu se poate atașa listei de alimente.')
        _require_date(bon, list_day)
        other = repo.select('VC_LegBCLA', {'IdBonConsum': b}, ('IdLista',))
        if other:
            raise ListaBcError('Bonul de consum %s este deja asociat altei liste.' % b)
    for b in removed:
        repo.delete('VC_LegBCLA', {'IdLista': id_lista, 'IdBonConsum': b})
    for b in added:
        try:
            repo.insert('VC_LegBCLA', {'IdLista': id_lista, 'IdBonConsum': b})
        except Exception as ex:   # a concurrent save won the BC between the check and the insert: the unique key decides
            if getattr(ex, 'errno', None) == 1062:
                raise ListaBcError('Bonul de consum %s este deja asociat altei liste.' % b) from ex
            raise
    return {'added': added, 'removed': removed}
