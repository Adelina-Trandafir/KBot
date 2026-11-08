"""VC12-04: existing ProduseP with real entries before 2026; balances at 2025-12-31."""
from collections import defaultdict
from datetime import date
from decimal import Decimal

from . import oi_custody, stock_source, stock, protocol
from .nomenclatoare import NomError
from utils.vercon_suppliers import name_key

ON = date(2025, 12, 31)
CUTOFF = date(2026, 1, 1)


def read(cursor, ctx, account):
    if ctx['type'] != 'OI':
        raise NomError('INVALID', 'Miniaplicația lucrează exclusiv cu obiecte de inventar (OI).', 422)
    cursor.execute('SELECT IdMasina, Masina, TIP FROM VC_Masini WHERE SubunitId=%s AND GestiuneId=%s '
                   'AND IdMasina=%s AND Activ=1', (ctx['subunit'], ctx['gestiune'], account))
    category = cursor.fetchone()
    if not category:
        raise NomError('INVALID', 'Contul nu aparține subgestiunii alese.', 422)
    cursor.execute('SELECT v.IdProdus AS variant_id, v.ID AS product_id, v.COD AS code, v.PUTVA AS price, '
                   'v.Version AS variant_version, p.Version AS product_version, p.DenumireP AS name, p.UM AS um, '
                   '(p.Activ=1 AND v.Activ=1) AS active, '
                   'MIN(n.Data) AS first_entry '
                   'FROM VC_ProduseP v JOIN VC_Produse p ON p.SubunitId=v.SubunitId '
                   'AND p.GestiuneId=v.GestiuneId AND p.ID=v.ID '
                   'JOIN VC_Intrari i ON i.SubunitId=v.SubunitId AND i.GestiuneId=v.GestiuneId '
                   'AND i.ExercitiuId=%s AND i.IdProdus=v.IdProdus '
                   'JOIN VC_NIR n ON n.SubunitId=i.SubunitId AND n.GestiuneId=i.GestiuneId '
                   'AND n.ExercitiuId=i.ExercitiuId AND n.IdIntrare=i.IdIntrare '
                   'WHERE v.SubunitId=%s AND v.GestiuneId=%s AND p.IdMasina=%s '
                   'AND n.Anulat=0 AND COALESCE(n.Storno,0)=0 AND i.Cant>0 AND n.Data<%s '
                   'GROUP BY v.IdProdus,v.ID,v.COD,v.PUTVA,v.Version,p.Version,p.DenumireP,p.UM,p.Activ,v.Activ',
                   (ctx['exercitiu'], ctx['subunit'], ctx['gestiune'], account, CUTOFF))
    products = cursor.fetchall()
    ids = {p['variant_id'] for p in products}
    warehouse = stock_source.warehouse(cursor, ctx, ids)
    held = oi_custody.events(cursor, ctx)
    balances = stock.available(warehouse, ON)
    # A backdated reclassification may not consume quantities used by later documents.
    capacities = {}
    for identity in ids:
        running = balances.get(identity, Decimal(0))
        capacity = running
        days = defaultdict(Decimal)
        for movement in warehouse:
            if movement['p'] == identity and stock.day_of(movement['d']) > ON:
                days[stock.day_of(movement['d'])] += movement['q']
        for day in sorted(days):
            running += days[day]
            capacity = min(capacity, running)
        capacities[identity] = max(Decimal(0), capacity)
    holders = {e['IDC_PTC'] for e in held if e['IdProdus'] in ids}
    lots = defaultdict(list)
    for holder in sorted(holders, key=lambda value: (value is None, value or 0)):
        if holder is None:
            raise NomError('INVENTORY_HOLDER', 'Registrul de folosință are o poziție fără deținător.', 409)
        for batch in oi_custody.batches(cursor, ctx, holder, on_date=ON).values():
            if batch['product'] in ids:
                lots[batch['product']].append({**batch, 'holder': holder})
    result = []
    for product in products:
        identity = product['variant_id']
        quantity = balances.get(identity, Decimal(0)) + sum((b['qty'] for b in lots[identity]), Decimal(0))
        if quantity <= 0:
            continue
        if balances.get(identity, Decimal(0)) < 0:
            raise NomError('INVENTORY_STOCK', 'Un produs are stoc de magazie negativ la 31.12.2025.', 409)
        result.append({**product, 'active': bool(product['active']), 'quantity': quantity,
                       'warehouse': balances.get(identity, Decimal(0)), 'warehouse_capacity': capacities[identity],
                       'lots': lots[identity], 'capacity': capacities[identity] + sum((b['capacity'] for b in lots[identity]), Decimal(0))})
    result.sort(key=lambda p: (name_key(p['name']), p['price'] or Decimal(0), name_key(p['code'])))
    # Detect stock/price/metadata changes between loading and sending, including later movements.
    fingerprint = protocol.fingerprint(result,
                                       sorted(warehouse, key=lambda m: (m['kind'], m['document_id'], m['line_id'])),
                                       sorted((e for e in held if e['IdProdus'] in ids), key=lambda e: e['IDOI']))
    return result, fingerprint, category


def serializable(value):
    if isinstance(value, dict):
        return {str(key): serializable(item) for key, item in value.items()}
    if isinstance(value, (list, tuple)):
        return [serializable(item) for item in value]
    if isinstance(value, Decimal):
        return format(value, 'f')
    if hasattr(value, 'isoformat'):
        return value.isoformat()
    return value
