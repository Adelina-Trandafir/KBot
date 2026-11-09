"""SLICE-VC14-02: warehouse and custody postings of a Bon transfer Unitate."""
from collections import defaultdict
from decimal import Decimal

from utils.clock import utc_now

from . import (bu_catalog, bu_links, bu_mf, consumption_timeline, invoices, numeric, oi, oi_custody,
               protocol, receipt_products, receipts, stock, stock_source)
from .documents import (_all, _check_cancel_input, _dec, _document_variants, _fail, _int, _mark_cancelled, _one, _scope,
                        _text, priced_line)
from .nomenclatoare import NomError


def target_context(ctx, destination):
    return {**ctx, 'subunit': destination['subunit'], 'gestiune': destination['gestiune'],
            'zec': int(destination['zec']) if destination['zec'] is not None else 4,
            'scale': int(destination['scale']) if destination['scale'] is not None else 4}


def account(options, raw, label):
    if raw in (None, '') and len(options) == 1:
        return options[0]['id']
    identity = _int(raw)
    if any(item['id'] == identity for item in options):
        return identity
    raise NomError('BU_ACCOUNT', 'Alegeți contul de ' + label + ' din gestiunea de destinație.', 422)


def eligible(cursor, ctx, day, holder):
    products = stock_source.variants(cursor, ctx)
    if holder:
        return [{**line, 'code': products.get(line['variant'], {}).get('code')}
                for line in oi.eligible(cursor, ctx, holder, on_date=day)]
    accounts = {row['IdMasina'] for row in _all(cursor, 'SELECT IdMasina,TIP FROM VC_Masini '
                'WHERE SubunitId=%s AND GestiuneId=%s AND Activ=1', (ctx['subunit'], ctx['gestiune']))
                if receipt_products.warehouse(ctx['type'], row['TIP'])}
    available = stock.available(stock_source.warehouse(cursor, ctx), day)
    return [{**item, 'left': format(available[variant], 'f'), 'PUTVA': format(item['price'], 'f')}
            for variant, item in products.items()
            if item['category'] in accounts and available.get(variant, 0) > 0
            and item['price'] is not None and item['price'] >= 0]


def source_lines(cursor, repo, ctx, day, holder, raw_lines):
    if not isinstance(raw_lines, list) or not 1 <= len(raw_lines) <= oi._MAX_LINES or any(
            not isinstance(row, dict) for row in raw_lines):
        raise NomError('INVALID', 'Adăugați produse cu cantități valide pe BU.', 422)
    if holder:
        if not repo.select('VC_Clienti', {'IDC': holder, 'Activ': 1}):
            raise NomError('BU_SOURCE', 'Alegeți un predător activ în gestiunea sursă.', 422)
        errors = {}
        lines = oi._taken_lines(cursor, ctx, 'bt', holder, None, raw_lines, day, errors)
        _fail(errors)
        return lines
    products = {item['variant']: item for item in eligible(cursor, ctx, day, None)}
    requested, lines = {}, []
    for row in raw_lines:
        variant, quantity = _int(row.get('variant')), _dec(row.get('Cant'))
        product = products.get(variant)
        if not product or quantity is None or quantity <= 0:
            raise NomError('BU_SOURCE', 'Alegeți produse din magazie și cantități pozitive.', 422)
        requested[variant] = requested.get(variant, 0) + quantity
        if requested[variant] > Decimal(product['left']):
            raise NomError('BU_SOURCE', f"Stoc insuficient la data BU pentru «{product['name']}».", 409)
        try:
            lines.append({'variant': variant, 'idm': product['category'],
                          **priced_line(quantity, product['price'], ctx['zec'], ctx['scale'])})
        except numeric.NumericError as error:
            raise NomError('BU_SOURCE', str(error), 422) from error
    return lines


def post_source(cursor, repo, ctx, document, number, day, holder, lines, now):
    for line in lines:
        repo.insert('VC_OI_Transfer', {'IDBT': document, 'NRBT': number, 'DATABT': day,
                    'IDC_OLD': holder, 'IDC_BT': None, 'IdMasina': line['idm'], 'IdProdus': line['variant'],
                    'Cant': line['Cant'], 'PUTVA': line.get('book_price', line['PUTVA']),
                    'ValoareCUTVA': line['ValoareCUTVA'],
                    'Expl': bu_links.reference(number), 'DataSalvare': now})
    variants = {line['variant'] for line in lines}
    if holder:
        oi._insert_register(repo, 'bt', document, [
            {'IDBO': line['origin'], 'IDM': line['idm'], 'IDC_PTC': holder, 'IdProdus': line['variant'],
             'PUTVA': line['PUTVA'], 'Cant': -line['Cant'], 'ValoareCUTVA': -line['ValoareCUTVA'], 'DataSalvare': now}
            for line in lines])
        oi_custody.check_timeline(cursor, ctx, variants)
        return {}
    by_account = defaultdict(list)
    for line in lines:
        by_account[line['idm']].append(line)
    bc_ids = {}
    for category, batch in by_account.items():
        bc_id = repo.insert('VC_Iesiri_H', {'IDC': None, 'IdMasina': category, 'DataBC': day,
                            'NumarBC': number, 'Expl': bu_links.reference(number),
                            'ValoareBC': sum(line['ValoareCUTVA'] for line in batch), 'Retur': 0, 'DataSalvare': now})
        for line in batch:
            repo.insert('VC_Iesiri', {'IdBonConsum': bc_id, 'IdProdus': line['variant'], 'Cant': line['Cant'],
                        'PUTVA': line['PUTVA'], 'ValoareCUTVA': line['ValoareCUTVA'], 'DataSalvare': now})
            bc_ids[line['variant']] = bc_id
    month_start = consumption_timeline.work_month_start(cursor, ctx, day)
    consumption_timeline.require_nonnegative(cursor, ctx, variants, month_start)
    return bc_ids


def prepare_receipt(cursor, repo, target_repo, ctx, target, day, warehouse, lines, number, request_id):
    fiscal, name = bu_catalog.supplier(cursor, ctx, request_id)
    vat = bu_catalog.zero_vat(target_repo)
    parsed = {'cols': {'TipDoc': 'NIR', 'Storno': 0, 'CodPartener': fiscal, 'FurnizorV': name, 'NrFact': number,
                       'DataFc': day, 'Data': day, 'IdMasina': warehouse, 'IdComisie': None},
              'declared': None, 'lines': []}
    mf = ctx['type'] == 'MF'
    assets = set()
    receiving = []
    for index, line in enumerate(lines):
        source = bu_catalog.source_product(cursor, ctx, line['variant'])
        product = bu_catalog.product(cursor, target_repo, target, source, warehouse, request_id)
        identity = (None, None, None)
        if mf:
            if line['variant'] in assets:
                raise NomError('BU_MF', 'Același număr de inventar apare de mai multe ori pe BU.', 422)
            assets.add(line['variant'])
            variant, identity = bu_mf.receive(cursor, repo, target_repo, ctx, target, line, source, product, day, request_id)
            line['book_price'] = line['ValoareCUTVA'] = source['PUTVA']
            amount = {'Cant': line['Cant'], 'PUTVA': source['PUTVA'], 'ValoareCUTVA': source['PUTVA']}
        else:
            amount = {key: line[key] for key in ('Cant', 'PUTVA', 'ValoareCUTVA')}
            variant = bu_catalog.variant(cursor, target_repo, target, product, source['COD'], amount['PUTVA'], request_id)
        parsed['lines'].append({
            'key': str(index), 'variant': variant, 'IDTVA': vat, 'rate': Decimal(0),
            'name': source['DenumireP'], **({'prepared_mf_variant': variant} if mf else {}),
            'v': {**amount, 'Pret': amount['PUTVA'], 'Valoare': amount['ValoareCUTVA'], 'TVA': Decimal(0)},
        })
        receiving.append((line['variant'], variant, identity, {**amount, 'variant': variant}))
    parsed['declared'] = sum(line['v']['ValoareCUTVA'] for line in parsed['lines'])
    return parsed, receiving


def create(cursor, repo, target_repo, ctx, target, exercise, destination, data, request_id):
    day = protocol.to_date(data.get('date'), 'Data transferului')
    holder = _int(data.get('giver'))
    if data.get('giver') not in (None, '') and (holder is None or holder <= 0):
        raise NomError('BU_SOURCE', 'Alegeți predătorul sau magazia.', 422)
    raw_receiver = data.get('receiver')
    receiver = _int(raw_receiver)
    if raw_receiver not in (None, '') and not any(item['id'] == receiver for item in destination['holders']):
        raise NomError('BU_RECEIVER', 'Alegeți un primitor activ din gestiunea de destinație.', 422)
    if receiver is None:
        bu_links.require_optional_receiver(cursor)
    warehouse = account(destination['warehouse_accounts'], data.get('warehouse'), 'magazie')
    use = account(destination['use_accounts'], data.get('use'), 'folosință') if receiver is not None else None
    errors = {}
    explanation = _text(data.get('explanation'), 'explanation', errors)
    _fail(errors)
    lines = source_lines(cursor, repo, ctx, day, holder, data.get('lines'))
    document = oi._next_id(cursor, ctx)
    cursor.execute(f'INSERT INTO {bu_links.TABLE} (SURSA_SUBUNITATE,SURSA_GESTIUNE,SURSA_EXERCITIU,IDBT,PREDATOR,'
                   'DEST_SUBUNITATE,DEST_GESTIUNE,DEST_EXERCITIU,PRIMITOR,DATA_BU,EXPLICATIE) '
                   'VALUES (%s,%s,%s,%s,%s,%s,%s,%s,%s,%s,%s)',
                   (*_scope(ctx), document, holder, *_scope(target), receiver, day, explanation))
    identity = cursor.lastrowid
    number = 'BU %d' % identity
    parsed, receiving = prepare_receipt(cursor, repo, target_repo, ctx, target, day, warehouse, lines, number, request_id)
    now = utc_now()
    source_bcs = post_source(cursor, repo, ctx, document, number, day, holder, lines, now)
    cols = parsed['cols']
    nir = receipts.create_nir(cursor, target_repo, target, exercise, {'header': {
        'CodFiscal': cols['CodPartener'], 'NrFact': number, 'DataFc': day.isoformat(),
    }}, None, request_id, parsed=parsed)
    bp_id = bp_number = None
    if receiver is not None:
        bp_id = oi._next_id(cursor, target)
        bp_number = oi._number(cursor, target, 'bp', None, {})
        bp_header = {'IDC_BP': receiver, 'IdMasinaP': warehouse, 'IdMasina': use, '_date': day,
                     'Expl': bu_links.reference(number)}
        bc_id = oi._write_bp(cursor, target_repo, target, bp_id, bp_number, bp_header,
                             [entry[3] for entry in receiving], None, now)
        oi_custody.check_timeline(cursor, target, {entry[1] for entry in receiving})
        protocol.audit(cursor, target, 'bp.create', oi.SPEC['bp']['table'], bp_id, None,
                       {'number': bp_number, 'bc': bc_id, 'bu': number}, None, request_id)
    cursor.execute(f'UPDATE {bu_links.TABLE} SET DEST_NIR=%s,DEST_BP=%s WHERE ID=%s', (nir['id'], bp_id, identity))
    cursor.executemany(f'INSERT INTO {bu_links.LINES} (BU_ID,SURSA_VARIANTA,SURSA_BC,DEST_VARIANTA,'
                       'ORIGINE_SUBUNITATE,ORIGINE_GESTIUNE,ORIGINE_VARIANTA) VALUES (%s,%s,%s,%s,%s,%s,%s)',
                       [(identity, source, source_bcs.get(source), dest, *origin)
                        for source, dest, origin, _ in receiving])
    protocol.freeze(cursor, ctx)
    result = {'id': identity, 'number': number, 'nir': nir['number'], 'bp': bp_number}
    protocol.audit(cursor, ctx, 'bu.create', bu_links.TABLE, identity, None, result, data['request_key'], request_id)
    return result


def cancel(cursor, repo, target_repo, ctx, target, row, data, request_id):
    reason = _check_cancel_input(data)
    identity, doc_id = row['ID'], row['IDBT']
    if row['ANULAT']:
        raise NomError('CANCELLED', 'Bonul BU este deja anulat.', 409)
    bp_id, nir_id = row['DEST_BP'], row['DEST_NIR']
    bp_rows = oi._rows(cursor, target, 'bp', bp_id) if bp_id is not None else []
    nir_rows = target_repo.select('VC_NIR', {'IdIntrare': nir_id}, for_update=True)
    if not nir_rows or (row['PRIMITOR'] is not None and not bp_rows) or any(
            item['Anulat'] for item in (*bp_rows, *nir_rows)):
        raise NomError('BU_LINKED', 'Documentele din destinație nu mai corespund bonului BU.', 409)
    variants = _document_variants(cursor, target, 'nir', nir_id)
    month_start = consumption_timeline.work_month_start(cursor, target, row['DATA_BU'])
    if bp_rows:
        oi._guard(cursor, target, 'bp', bp_id, bp_rows)
    else:
        # Include the whole interval in which the removed receipt supplied stock.
        month_start = min(month_start, row['DATA_BU'])
        if target['type'] == 'MF' and any(
            movement['variant'] in variants and movement['delta'] < 0
            and (movement['day'], movement['bu']) > (row['DATA_BU'], identity)
            for movement in bu_mf.movements(cursor, target)
        ):
            raise NomError('BU_LINKED', 'Un activ MF a fost transferat ulterior. Anulați întâi transferul ulterior.', 409)
    from . import efactura_receipts
    efactura_receipts.require_editable(cursor, target, nir_id)
    if _one(cursor, 'SELECT 1 FROM VC_NIR WHERE SubunitId=%s AND GestiuneId=%s AND ExercitiuId=%s '
            'AND IdStorno=%s AND Anulat=0', (*_scope(target), nir_id)):
        raise NomError('HAS_STORNO', 'Anulați mai întâi stornarea NIR-ului din destinație.', 409)
    link = invoices.link_of(cursor, target, nir_id)
    if link:
        invoices.load(cursor, link['FACTURA_ID'], lock=True)
    source_bcs = _all(cursor, f'SELECT DISTINCT SURSA_BC FROM {bu_links.LINES} WHERE BU_ID=%s AND SURSA_BC IS NOT NULL',
                      (identity,))
    bcs = [(target, target_repo, bp_rows[0]['IdBonConsum'])] if bp_rows else []
    bcs.extend((ctx, repo, item['SURSA_BC']) for item in source_bcs)
    for local_ctx, local, bc_id in bcs:
        bc = local.select('VC_Iesiri_H', {'IdBonConsum': bc_id}, for_update=True)
        if len(bc) != 1 or bc[0]['Anulat']:
            raise NomError('BU_LINKED', 'Un bon de descărcare din magazie lipsește sau a fost anulat.', 409)
        if _one(cursor, 'SELECT 1 FROM VC_Iesiri_H WHERE SubunitId=%s AND GestiuneId=%s AND ExercitiuId=%s '
                'AND IdRetur=%s AND Retur=1 AND Anulat=0', (*_scope(local_ctx), bc_id)):
            raise NomError('HAS_RETUR', 'Anulați mai întâi returul legat de ieșirea BU.', 409)
    stamp = {'Anulat': 1, 'MotivAnulare': reason, 'AnulatDe': ctx['email'], 'AnulatLa': utc_now()}
    for _, local, bc_id in bcs:
        local.update('VC_Iesiri_H', stamp, {'IdBonConsum': bc_id}, bump_version=True)
    if bp_rows:
        target_repo.update('VC_OI_Predare', stamp, {'IDBP': bp_id}, bump_version=True)
        oi._forget(target_repo, 'bp', bp_id)
    _mark_cancelled(cursor, target, 'VC_NIR', 'IdIntrare', nir_id, reason, nir_rows[0]['Version'])
    repo.update('VC_OI_Transfer', stamp, {'IDBT': doc_id}, bump_version=True)
    oi._forget(repo, 'bt', doc_id)
    cursor.execute(f'UPDATE {bu_links.TABLE} SET ANULAT=1,MOTIV_ANULARE=%s WHERE ID=%s', (reason, identity))
    consumption_timeline.require_nonnegative(cursor, target, variants, month_start)
    if bp_rows:
        oi_custody.check_timeline(cursor, target, variants)
    if link:
        invoices.refresh_state(cursor, link['FACTURA_ID'])
    protocol.audit(cursor, ctx, 'bu.cancel', bu_links.TABLE, identity, {'Anulat': 0},
                   {'Anulat': 1, 'Motiv': reason}, data['request_key'], request_id)
    return {'id': identity, 'number': 'BU %d' % identity}
