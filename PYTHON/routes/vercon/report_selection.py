"""Lazy report filter branches, read through the existing scoped SELECT sources."""
from decimal import Decimal
from . import stock
from .nomenclatoare import NomError


def account_filter(spec):
    return spec.get('ledger') == 'custody' or spec.get('family') in ('bp', 'bt', 'bs')


def roots(spec):
    nodes = []
    if account_filter(spec) and 'category' in spec['filters']:
        nodes.append(branch('selection:use-accounts', 'Conturi în folosință'))
    if 'variant' in spec['filters']:
        nodes.append(branch('selection:products', 'Produse'))
    for key, label in [('supplier', 'Furnizori'), ('receiver', 'Primitori'), ('document', 'Documente')]:
        if key not in spec['filters']:
            continue
        if key == 'receiver':
            label = 'Predători' if spec.get('family') == 'bs' else 'Deținători' if spec.get('ledger') == 'custody' else label
        nodes.append(branch('selection:' + key, label))
    return nodes


def branch(node_id, label):
    return {'id': node_id, 'label': label, 'lazy': True}


def options(items, prefix):
    return [{'id': f'{prefix}:{key}', 'label': label} for key, label in items.items() if key is not None]


def product_nodes(source, node_id, recursive):
    parts = node_id.split(':')
    filters = {}
    if parts[0] in ('category', 'product'):
        if len(parts) != (2 if parts[0] == 'category' else 3):
            raise NomError('REPORT_BRANCH', 'Ramură de produse invalidă.', 422)
        try:
            filters['category'] = None if parts[1] == 'none' else int(parts[1])
            if parts[0] == 'product':
                filters['product'] = int(parts[2])
        except ValueError:
            raise NomError('REPORT_BRANCH', 'Ramură de produse invalidă.', 422) from None
    rows = list(source.metadata(result_filters=filters).values())

    def variants(items):
        return [{'id': f"variant:{row['variant']}", 'label': str(row['code'] or row['variant']) + ' / '
                 + (stock.quantity_text(Decimal(row['price']), source.ctx['scale']) if row['price'] is not None else '—') + ' lei'}
                for row in items]

    def products(items, category):
        grouped = {}
        for row in items:
            grouped.setdefault(row['product'], []).append(row)
        nodes = []
        for product, group in grouped.items():
            row = group[0]
            node = branch(f'product:{category}:{product}', (row['name'] or '') + (' / ' + row['um'] if row['um'] else ''))
            if recursive:
                node.pop('lazy')
                node['children'] = variants(group)
            nodes.append(node)
        return nodes

    if parts[0] == 'product':
        return variants(rows)
    if parts[0] == 'category':
        return products(rows, parts[1])
    grouped = {}
    for row in rows:
        grouped.setdefault(row['category'], []).append(row)
    nodes = []
    for category, group in grouped.items():
        key = str(category) if category is not None else 'none'
        node = branch(f'category:{key}', group[0]['account'] or 'Fără încadrare')
        if recursive:
            node.pop('lazy')
            node['children'] = products(group, key)
        nodes.append(node)
    return nodes


def selection_branch(source, spec, node_id, *, recursive=False):
    allowed = {node['id'] for node in roots(spec)}
    if node_id == 'root':
        # No metadata, movements, partners or documents are read for the initial tree.
        return {'spec': spec, 'nodes': roots(spec), 'documents': []}
    if node_id.startswith(('category:', 'product:')):
        if 'selection:products' not in allowed:
            raise NomError('REPORT_BRANCH', 'Filtrul de produse nu este disponibil.', 422)
        return {'nodes': product_nodes(source, node_id, recursive)}
    if node_id not in allowed:
        raise NomError('REPORT_BRANCH', 'Ramura nu este disponibilă pentru acest raport.', 422)
    if node_id == 'selection:products':
        return {'nodes': product_nodes(source, node_id, recursive)}
    if node_id == 'selection:use-accounts':
        return {'nodes': options({row['category']: row['account'] for row in source.rows('oi_accounts')}, 'category-filter')}
    if node_id == 'selection:receiver' and account_filter(spec):
        return {'nodes': options({row['receiver']: row['party'] for row in source.rows('oi_holders')}, 'receiver')}
    items = {}
    key = node_id.split(':', 1)[1]
    if spec['kind'] == 'food':
        for row in source.rows('food_history'):
            day = stock.day_of(row['date'])
            if day is not None and source.params['start'] <= day <= source.params['end']:
                items[row['IdLista']] = f"{row['NumarLA']} / {row['date']}"
    else:
        query = 'oi_documents' if account_filter(spec) else 'consumption' if spec.get('family') in ('bc', 'br') or spec['kind'].startswith('consumption') else 'receipts'
        for row in source.rows(query):
            if row['annulled'] or spec.get('family') and row['family'] != spec['family']:
                continue
            if key in ('supplier', 'receiver'):
                if row.get(key) is not None:
                    items[row[key]] = row['party'] or str(row[key])
            else:
                day = row.get('movement_date') or row['date']
                if day is not None and source.params['start'] <= stock.day_of(day) <= source.params['end']:
                    items[row['document']] = f"{row['family'].upper()} {row['number']} / {day} / {row.get('party') or ''}"
    result = {'nodes': options(items, key)}
    if key == 'document':
        result['documents'] = [{'id': item, 'label': label} for item, label in items.items() if item is not None]
    return result
