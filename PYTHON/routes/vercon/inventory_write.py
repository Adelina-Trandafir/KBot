"""VC12-04: atomic, audited inventory reclassification dated 2025-12-31.

Existing documents remain intact. Warehouse reclassification uses signed initial
stock lines; custody uses paired correction BS/BP, with the normal BP warehouse
exit. Unmatched Excel quantities remain in the initial-stock warehouse receipt.
"""
from collections import defaultdict
from decimal import Decimal, ROUND_HALF_UP

from utils.clock import utc_now
from routes.admin_cli.inventory_similarity import number
from . import numeric, nomenclatoare as nom, protocol, oi, receipt_number, receipt_products
from .inventory_source import ON


def reject(text):
    raise nom.NomError('INVENTORY_INVALID', text, 422)


def amount(raw, label):
    value = number(raw)
    if value is None or value < 0:
        reject('%s nu este validă.' % label)
    try:
        return numeric.to_stored(value)
    except numeric.NumericError as error:
        reject(str(error))


def unchanged(row, product):
    return (row['name'] == product['name'] and row['um'] == product['um'] and
            (row['code'] or '') == (product['code'] or '') and number(row['price']) == product['price'])


def apply(cursor, repo, ctx, exercise, workspace, body, request_id):
    if ctx['type'] != 'OI' or any(r.get('kind') != 'OI' for r in workspace['rows']):
        reject('Miniaplicația lucrează exclusiv cu obiecte de inventar (OI).')
    rows = {r['row']: r for r in workspace['rows']}
    products = {p['variant_id']: p for p in workspace['current_products']}
    raw = body.get('assignments', [])
    if not isinstance(raw, list) or len(raw) > 10000:
        reject('Lista asocierilor nu este validă.')
    remaining = {key: amount(r['quantity'], 'Cantitatea Excel') for key, r in rows.items()}
    available = {key: amount(p['quantity'], 'Cantitatea VERCON') for key, p in products.items()}
    capacity = {key: amount(p['capacity'], 'Disponibilul pentru corecție') for key, p in products.items()}
    allocations = []
    for item in raw:
        if not isinstance(item, dict) or type(item.get('row')) is not int or type(item.get('variant')) is not int:
            reject('O asociere nu identifică rândul Excel și varianta VERCON.')
        row, source = rows.get(item['row']), products.get(item['variant'])
        if row is None or source is None:
            reject('O poziție nu mai aparține comparației curente.')
        quantity = amount(item.get('quantity'), 'Cantitatea asociată')
        if quantity <= 0 or quantity > remaining[row['row']] or quantity > available[source['variant_id']]:
            reject('Cantitatea asociată depășește restul Excel sau cantitatea VERCON la 31.12.2025.')
        if not unchanged(row, source):
            if quantity > capacity[source['variant_id']]:
                reject('Documentele ulterioare nu permit corectarea acestei cantități. Reduceți cantitatea sau revizuiți documentele sursă.')
            capacity[source['variant_id']] -= quantity
        price, old_price = amount(row['price'], 'Prețul Excel'), number(source['price'])
        if old_price is None or old_price < 0:
            reject('Varianta VERCON nu are un preț valid.')
        if price.quantize(Decimal('.01'), rounding=ROUND_HALF_UP) != old_price.quantize(Decimal('.01'), rounding=ROUND_HALF_UP):
            if item.get('price_confirmed') is not True:
                reject('Confirmați diferența de preț la două zecimale pentru poziția Excel %s.' % row['serial'])
        remaining[row['row']] -= quantity
        available[source['variant_id']] -= quantity
        allocations.append((row, source, quantity))
    # Every write below belongs to the route's single transaction.
    if not receipt_products.account(repo, ctx, workspace['account']):
        reject('Contul stocului inițial trebuie să fie de tip MAGAZIE sau OIM.')
    vat_rows = repo.select('VC_TVA', {'TVA': 0, 'Activ': 1})
    if not vat_rows:
        reject('Adăugați cota TVA 0 în nomenclatorul gestiunii. Prețul din Excel se păstrează fără TVA suplimentar.')
    vat = vat_rows[0]['IdTVA']
    now = utc_now()
    key = workspace['operation_key']
    explanation = 'CORECTIE INVENTAR EXCEL 31.12.2025 — RECLASIFICARE, nu casare fizica. ' + key[-12:]
    incoming, initial, removals, custody_out, custody_in, targets, audit_links = [], [], [], [], [], {}, []
    new_ids = set()
    product_ids = {}
    kept_quantity = Decimal(0)
    changed_quantities = defaultdict(Decimal)

    def target(row):
        identity = row['row']
        if identity not in product_ids:
            values = {'DenumireP': row['name'], 'UM': row['um'], 'IdMasina': workspace['account']}
            values = nom.clean(nom.SPECS['produse'], ctx['type'], values, ctx['scale'], repo)
            product_ids[identity] = repo.insert('VC_Produse', {**values, 'Activ': 1, 'DataSalvare': now})
        if identity in targets:
            return targets[identity]
        price = amount(row['price'], 'Prețul Excel')
        variant = repo.insert('VC_ProduseP', {'ID': product_ids[identity], 'COD': row['code'] or None,
                                            'PUTVA': price, 'Activ': 1, 'DataSalvare': now})
        targets[identity] = variant
        new_ids.add(variant)
        return variant

    def value(quantity, price):
        return numeric.to_stored(numeric.calc_round(quantity * price, 4))

    used_wh, used_lots = defaultdict(Decimal), defaultdict(Decimal)
    for row, source, quantity in allocations:
        if unchanged(row, source):
            kept_quantity += quantity
            audit_links.append({'excel_row': row['row'], 'serial': row['serial'], 'old_variant': source['variant_id'],
                                'new_variant': source['variant_id'], 'quantity': quantity, 'unchanged': True, 'excel': row})
            continue
        left = quantity
        portions = []
        wh = min(left, max(Decimal(0), source['warehouse_capacity'] - used_wh[source['variant_id']]))
        if wh:
            portions.append((wh, None))
            used_wh[source['variant_id']] += wh
            left -= wh
        for batch in sorted(source['lots'], key=lambda b: (b['date'], b['origin'])):
            batch_key = (source['variant_id'], batch['holder'], batch['origin'], batch['idm'], batch['putva'])
            take = min(left, max(Decimal(0), batch['capacity'] - used_lots[batch_key]))
            if take:
                portions.append((take, batch))
                used_lots[batch_key] += take
                left -= take
        if left:
            reject('Loturile disponibile s-au schimbat. Reîncărcați comparația.')
        for part, batch in portions:
            variant = target(row)
            price = number(row['price'])
            changed_quantities[source['variant_id']] += part
            incoming.append((variant, part, price, row['row']))
            if batch is None:
                removals.append((source['variant_id'], -part, source['price'], row['row']))
            else:
                custody_out.append((source, batch, part))
                custody_in.append((variant, batch, part, price))
            audit_links.append({'excel_row': row['row'], 'serial': row['serial'], 'old_variant': source['variant_id'],
                                'new_variant': variant, 'quantity': part, 'holder': batch['holder'] if batch else None,
                                'old_price': source['price'], 'unchanged': False, 'excel': row})
    for identity, quantity in remaining.items():
        row = rows[identity]
        if quantity:
            variant = target(row)
            initial.append((variant, quantity, number(row['price']), row['row']))
    documents = []
    for all_lines, title in ((removals + incoming, 'CORECTIE INVENTAR EXCEL '), (initial, 'STOC INITIAL EXCEL ')):
        for start in range(0, len(all_lines), 100):
            lines = all_lines[start:start + 100]
            doc_number = receipt_number.assign(cursor, ctx, None, 0, ON)
            total = sum((value(q, p) for _, q, p, _ in lines), Decimal(0))
            document = repo.insert('VC_NIR', {'TipDoc': 'STOC_INITIAL', 'Data': ON, 'Numar': doc_number, 'NumarSub': 0,
                                             'NrFact': title + key[-12:], 'IdMasina': workspace['account'],
                                             'Storno': 0, 'Anulat': 0, 'Valoare': total})
            for variant, quantity, price, excel_row in lines:
                repo.insert('VC_Intrari', {'IdIntrare': document, 'IdProdus': variant, 'IDTVA': vat, 'Cant': quantity,
                                          'Pret': price, 'PUTVA': price, 'TVA': 0, 'Valoare': value(quantity, price),
                                          'ValoareCUTVA': value(quantity, price), 'DataSalvare': now})
            protocol.audit(cursor, ctx, 'inventory.initial', 'VC_NIR', document, None,
                           {'explanation': explanation, 'lines': lines}, key, request_id)
            documents.append({'family': 'nir', 'id': document, 'number': doc_number})
    for source, batch, quantity in custody_out:
        document = oi._next_id(cursor, ctx)
        doc_number = oi._number(cursor, ctx, 'bs', None, {})
        line = {'origin': batch['origin'], 'idm': batch['idm'], 'variant': source['variant_id'], 'Cant': quantity,
                'PUTVA': batch['putva'], 'ValoareCUTVA': value(quantity, batch['putva'])}
        oi._write_bs(repo, ctx, document, doc_number,
                     {'IDC_BC': batch['holder'], '_date': ON, 'Expl': explanation}, [line], now)
        protocol.audit(cursor, ctx, 'inventory.custody.out', 'VC_OI_Casare', document, None, line, key, request_id)
        documents.append({'family': 'bs', 'id': document, 'number': doc_number})
    for variant, batch, quantity, price in custody_in:
        document = oi._next_id(cursor, ctx)
        doc_number = oi._number(cursor, ctx, 'bp', None, {})
        line = {'variant': variant, 'Cant': quantity, 'PUTVA': price, 'ValoareCUTVA': value(quantity, price)}
        oi._write_bp(cursor, repo, ctx, document, doc_number,
                     {'IDC_BP': batch['holder'], 'IdMasina': batch['idm'], 'IdMasinaP': workspace['account'],
                      '_date': ON, 'Expl': explanation}, [line], None, now)
        protocol.audit(cursor, ctx, 'inventory.custody.in', 'VC_OI_Predare', document, None, line, key, request_id)
        documents.append({'family': 'bp', 'id': document, 'number': doc_number})
    touched = set(changed_quantities) | new_ids
    protocol.check_timeline(cursor, ctx, touched)
    from . import oi_custody
    oi_custody.check_timeline(cursor, ctx, touched)
    from . import inventory_source
    after, _, _ = inventory_source.read(cursor, ctx, workspace['account'])
    after_qty = {p['variant_id']: p['quantity'] for p in after}
    expected = {identity: p['quantity'] for identity, p in products.items()}
    for identity, quantity in changed_quantities.items():
        expected[identity] -= quantity
    for identity, quantity in expected.items():
        if after_qty.get(identity, Decimal(0)) != quantity:
            reject('Corecțiile nu păstrează restul variantei VERCON. Lotul nu a fost salvat.')
    if sum((after_qty.get(identity, Decimal(0)) for identity in new_ids), Decimal(0)) != sum(
            (number(row['quantity']) for row in rows.values()), Decimal(0)) - kept_quantity:
        reject('Cantitățile noi nu corespund Excelului. Lotul nu a fost salvat.')
    if documents:
        protocol.freeze(cursor, ctx)
    result = {'assignments': len(allocations), 'new_quantity': format(sum(remaining.values(), Decimal(0)), 'f'),
              'documents': documents, 'date': ON.isoformat(), 'variants_created': len(new_ids),
              'unchanged_quantity': format(kept_quantity, 'f')}
    protocol.audit(cursor, ctx, 'inventory.reconcile', 'VC_ProduseP', key, None,
                   {'result': result, 'assignments': audit_links, 'unmatched': remaining,
                    'source': workspace['source_name'], 'sheet': workspace['sheet']}, key, request_id)
    return result
