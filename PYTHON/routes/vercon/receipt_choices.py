"""Receipt choices use product identities and one supplier per fiscal code."""
from utils import vercon_suppliers as suppliers


def products(cursor, subunit, gestiune):
    cursor.execute(
        'SELECT ID, DenumireP, UM, IdMasina FROM VC_Produse '
        'WHERE SubunitId=%s AND GestiuneId=%s AND Activ=1 ORDER BY DenumireP, ID',
        (subunit, gestiune))
    return [{'id': row['ID'], 'name': row['DenumireP'] or '', 'um': row['UM'] or '',
             'category': row['IdMasina']} for row in cursor.fetchall()]


def partners(cursor):
    cursor.execute('SELECT p.* FROM ' + suppliers.relation(True) + ' p ORDER BY p.DenumirePartener,p.CodFiscal')
    items = [{'id': row['CodFiscal'], 'name': row['DenumirePartener'] or row['CodFiscal'],
              'cf': row['CodFiscal']} for row in cursor.fetchall()]
    return sorted(items, key=lambda item: (suppliers.name_key(item['name']), item['cf']))


def find_price_variant(repo, product, price, preferred=None, *, for_update=False):
    """Use the same active price variant for the draft and the saved receipt."""
    rows = repo.select('VC_ProduseP', {'ID': product, 'PUTVA': price, 'Activ': 1},
                       columns=('IdProdus',), for_update=for_update)
    ids = {row['IdProdus'] for row in rows}
    if preferred in ids:
        return preferred
    return min(ids, default=None)


def price_variant(repo, product, price, now, existing=None):
    """Resolve the received price without changing prices used by other documents."""
    variant = find_price_variant(repo, product, price, existing['IdProdus'] if existing else None, for_update=True)
    if variant is not None:
        return variant
    return repo.insert('VC_ProduseP', {'ID': product, 'PUTVA': price, 'Activ': 1,
                                      'COD': None, 'DataSalvare': now})
