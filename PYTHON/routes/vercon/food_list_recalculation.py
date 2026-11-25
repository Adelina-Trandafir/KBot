"""Recalculate the linked food list inside the consumption or return transaction."""
from decimal import Decimal

from . import food_list_portions, protocol, recalculation_audit
from .nomenclatoare import NomError


def refresh_for_bon(cursor, repo, ctx, bon_id, request_id):
    bon = repo.select('VC_Iesiri_H', {'IdBonConsum': bon_id})[0]
    origin = recalculation_audit.cause('bc', bon_id, bon['NumarBC'], bon['DataBC'], bon['Version'])
    return _refresh(cursor, repo, ctx, bon_id, origin, request_id)


def snapshot_for_return(cursor, repo, ctx, source_id):
    if ctx['type'] != 'ALIMENTE' or source_id is None:
        return {}
    if not repo.select('VC_LegBCLA', {'IdBonConsum': source_id}, ('IdLista',)):
        return {}
    return recalculation_audit.food_snapshot(cursor, ctx)


def refresh_after_return(cursor, repo, ctx, source_id, before, origin, request_id):
    if not before:
        return []
    _refresh(cursor, repo, ctx, source_id, origin, request_id)
    after = recalculation_audit.food_snapshot(cursor, ctx)
    return recalculation_audit.record(cursor, ctx, before, after, origin,
                                      'lista.balance_recalculated', 'VC_LABC', request_id)


def _refresh(cursor, repo, ctx, bon_id, origin, request_id):
    from .lista import _bons_total

    changed = []
    for link in repo.select('VC_LegBCLA', {'IdBonConsum': bon_id}, ('IdLista',)):
        list_id = link['IdLista']
        heads = repo.select('VC_LABC', {'IdLista': list_id}, for_update=True)
        if not heads or heads[0]['Anulat']:
            raise NomError('LINKED_LIST', 'Lista asociată nu mai este disponibilă.', 409)
        before = heads[0]
        if origin['family'] == 'bc':
            food_list_portions.require_month(cursor, ctx, before['DATALA'])
        if before['ValoareLista'] is None:
            raise NomError('LIST_VALUE_MISSING', 'Lista asociată nu are valoarea păstrată. Corectați mai întâi lista.', 409)
        total = _bons_total(cursor, ctx, list_id)
        values = {'ValoareBon': total, 'DEP': Decimal(before['ValoareLista']) - total}
        if origin['family'] == 'br' and all(before[key] == value for key, value in values.items()):
            continue
        if repo.update('VC_LABC', values, {'IdLista': list_id, 'Version': before['Version']}, bump_version=True) != 1:
            raise NomError('CONFLICT', 'Lista asociată a fost modificată. Reîncărcați documentele.', 409)
        if origin['family'] == 'bc':
            protocol.audit(cursor, ctx, 'lista.bc_update', 'VC_LABC', list_id,
                           {key: before[key] for key in values},
                           {**values, 'bon_id': bon_id, 'cause': origin, 'reason': origin['reason']}, None, request_id)
        changed.append(list_id)
    return changed
