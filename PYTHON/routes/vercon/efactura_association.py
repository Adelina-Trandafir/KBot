"""Explicit, partial links from a saved NIR to an e-invoice. Never changes stock or NIR contents."""
from decimal import Decimal, InvalidOperation

from flask import request

from utils import vercon_efactura as shared
from . import efactura_queue as queue, efactura_product_cpv, efactura_receipts, protocol
from .documents import _one, _all, _int, _scope
from .nomenclatoare import NomError


def require(cursor):
    if not shared.ready(cursor):
        raise NomError('EF_ASSOC_SCHEMA_REQUIRED', 'Asocierea necesită aplicarea manuală a sql/EV_02_vercon_asocieri_partiale.sql și sincronizarea structurii în DC.', 503)


def receipt(cursor, ctx, identity, lock=False, *, allow_initial=False):
    supplier = shared.receipt_supplier_sql(cursor)
    row = _one(cursor, 'SELECT n.*,' + supplier + ' AS EF_CUI FROM VC_NIR n '
               'WHERE n.SubunitId=%s AND n.GestiuneId=%s AND n.ExercitiuId=%s AND n.IdIntrare=%s'
               + (' FOR UPDATE' if lock else ''), (*_scope(ctx), identity))
    if not row or row['Anulat'] or row.get('Storno') or (not allow_initial and row.get('TipDoc') == 'STOC_INITIAL'):
        raise NomError('INVALID', 'Alegeți un NIR salvat, neanulat, de cumpărare.', 422)
    return row


def invoice(cursor, sending_id, lock=False):
    row = _one(cursor, 'SELECT * FROM EF_Primite WHERE IdPrimita=(SELECT IdPrimita FROM EF_PrimiteVercon WHERE IdTrimitere=%s)'
               + (' FOR UPDATE' if lock else ''), (sending_id,))
    sending = _one(cursor, 'SELECT * FROM EF_PrimiteVercon WHERE IdTrimitere=%s' + (' FOR UPDATE' if lock else ''), (sending_id,))
    if not row or not sending or sending['Stare'] not in ('TRIMISA', 'PRELUATA'):
        raise NomError('CONFLICT', 'Factura a fost retrasă sau returnată contabilului.', 409)
    return row


def lock_replacement(cursor, sending_id, old_id):
    # Every replacement locks the invoice/dispatch pairs in the same order, including opposite swaps.
    pairs = _all(cursor, 'SELECT IdTrimitere FROM EF_PrimiteVercon WHERE IdTrimitere IN (%s,%s) ORDER BY IdPrimita,IdTrimitere',
                 (sending_id, old_id))
    if len(pairs) != 2:
        raise NomError('CONFLICT', 'Una dintre facturile alese nu mai este disponibilă.', 409)
    locked = {row['IdTrimitere']: invoice(cursor, row['IdTrimitere'], True) for row in pairs}
    return locked[sending_id]


def check_link(cursor, ctx, nir_id, old_id, new_id=None):
    linked = {item['sending'] for item in efactura_receipts.linked(cursor, ctx, [nir_id]).get(nir_id, [])}
    if old_id not in linked:
        raise NomError('CONFLICT', 'Asocierea aleasă nu mai există. Redeschideți NIR-ul.', 409)
    if new_id in linked:
        raise NomError('CONFLICT', 'Alegeți o altă eFactură, care nu este deja asociată acestui NIR.', 409)


def replacement_id(raw):
    if raw is None:
        return None
    identity = _int(raw)
    if identity is None or identity <= 0:
        raise NomError('INVALID', 'Alegeți asocierea care trebuie înlocuită.', 422)
    return identity


def compatible(cursor, nir, ef):
    # The same resolved fiscal identity filters the picker and validates saved links.
    nir_cui = nir['EF_CUI']
    ef_cui = shared.fiscal_code(ef['CuiNormalizat'])
    if not nir_cui or not ef_cui or nir_cui != ef_cui:
        raise NomError('INVOICE_SUPPLIER',
                       'Codurile fiscale ale furnizorilor nu coincid: NIR — %s; eFactura — %s. '
                       'Comparația folosește codul fiscal normalizat, fără prefixul RO, spații sau semne; '
                       'denumirea și ID-ul furnizorului nu sunt criterii.' % (nir_cui or 'lipsă', ef_cui or 'lipsă'), 422)
    nir_date = (queue._iso(nir['DataFc']) or '')[:10]
    ef_date = (queue._iso(ef['DataFact']) or '')[:10]
    if not nir_date or not ef_date or nir_date != ef_date:
        raise NomError('INVOICE_DATE',
                       'Datele facturii nu coincid: în NIR — %s; în eFactura — %s. '
                       'Se compară Data facturii din NIR, nu Data NIR. Codurile fiscale coincid.'
                       % (nir_date or 'lipsă', ef_date or 'lipsă'), 422)


def nir_lines(cursor, ctx, nir_id):
    return _all(cursor, 'SELECT i.ID,i.IdProdus,i.Cant,i.PUTVA,i.ValoareCUTVA,p.ID AS PRODUCT_ID,p.DenumireP,p.UM,p.CPV '
                'FROM VC_Intrari i JOIN VC_ProduseP pp ON pp.SubunitId=i.SubunitId AND pp.GestiuneId=i.GestiuneId AND pp.IdProdus=i.IdProdus '
                'JOIN VC_Produse p ON p.SubunitId=pp.SubunitId AND p.GestiuneId=pp.GestiuneId AND p.ID=pp.ID '
                'WHERE i.SubunitId=%s AND i.GestiuneId=%s AND i.ExercitiuId=%s AND i.IdIntrare=%s', (*_scope(ctx), nir_id))


def same_nir(link, ctx, nir_id):
    return (link['SUBUNIT_ID'], link['GESTIUNE_ID'], link['EXERCITIU_ID'], link['NIR_ID']) == (*_scope(ctx), nir_id)


def budgets(cursor, ctx, sending_id, nir_id, old_id=None):
    used_ef, used_nir = {}, {}
    links = shared.allocations(cursor)
    for link in links:
        if not link['VALID'] or (link['ID_TRIMITERE'] in (sending_id, old_id) and same_nir(link, ctx, nir_id)):
            continue
        if link['ID_TRIMITERE'] == sending_id:
            used_ef[link['ID_LINIE']] = used_ef.get(link['ID_LINIE'], Decimal(0)) + link['CANT_FACTURA']
        if same_nir(link, ctx, nir_id):
            used_nir[link['INTRARE_ID']] = used_nir.get(link['INTRARE_ID'], Decimal(0)) + link['CANT_NIR']
    return links, used_ef, used_nir


def legacy_guard(cursor, ctx, sending_id, nir_id, old_id=None):
    # Integral links predate the line budgets and cannot safely be guessed or silently converted.
    row = _one(cursor, 'SELECT 1 FROM EF_PrimiteVerconNir a JOIN VC_NIR n ON n.SubunitId=a.SubunitId '
               'AND n.GestiuneId=a.GestiuneId AND n.ExercitiuId=a.ExercitiuId AND n.IdIntrare=a.IdIntrare '
               'WHERE COALESCE(n.Anulat,0)=0 AND NOT EXISTS (SELECT 1 FROM EF_VERCON_ASOCIERI m WHERE m.ID_TRIMITERE=a.IdTrimitere) '
               'AND (a.IdTrimitere=%s OR (a.SubunitId=%s AND a.GestiuneId=%s AND a.ExercitiuId=%s AND a.IdIntrare=%s '
               'AND a.IdTrimitere<>%s)) LIMIT 1', (sending_id, *_scope(ctx), nir_id, old_id or 0))
    if row:
        raise NomError('EF_LEGACY_LINK', 'Factura aleasă sau NIR-ul are o asociere integrală care ocupă cantitățile disponibile.', 409)


def detail(cursor, ctx, sending_id, nir_id, old_id=None):
    require(cursor)
    nir, ef = receipt(cursor, ctx, nir_id), invoice(cursor, sending_id)
    compatible(cursor, nir, ef)
    if old_id is not None:
        check_link(cursor, ctx, nir_id, old_id, sending_id)
    legacy_guard(cursor, ctx, sending_id, nir_id, old_id)
    links, used_ef, used_nir = budgets(cursor, ctx, sending_id, nir_id, old_id)
    source = _all(cursor, 'SELECT l.IdLinie,l.Denumire,l.Unit,l.Cpv,l.Cant FROM EF_PrimiteLinii l WHERE l.IdPrimita=%s AND ' + shared.sent_line_sql('%s', 'l.IdLinie'),
                  (ef['IdPrimita'], sending_id))
    target = nir_lines(cursor, ctx, nir_id)
    memory = _all(cursor, 'SELECT NUME_HASH,PRODUS_ID FROM EF_VERCON_CORELARI WHERE CUI=%s AND SUBUNIT_ID=%s AND GESTIUNE_ID=%s',
                  (ef['CuiNormalizat'], ctx['subunit'], ctx['gestiune']))
    learned = {}
    for row in memory:
        learned.setdefault(row['NUME_HASH'], set()).add(row['PRODUS_ID'])
    ef_lines = [{'id': l['IdLinie'], 'name': l['Denumire'] or '', 'um': l['Unit'] or '', 'cpv': shared.cpv(l['Cpv']),
                 'qty': queue._text(l['Cant']), 'left': queue._text((l['Cant'] or Decimal(0)) - used_ef.get(l['IdLinie'], 0)),
                 'learned': sorted(learned.get(shared.name_hash(l['Denumire']), set()))} for l in source]
    receipt_lines = [{'id': l['ID'], 'product': l['PRODUCT_ID'], 'name': l['DenumireP'] or '', 'um': l['UM'] or '', 'cpv': shared.cpv(l['CPV']),
                      'qty': queue._text(l['Cant']), 'left': queue._text((l['Cant'] or Decimal(0)) - used_nir.get(l['ID'], 0))} for l in target]
    own = [l for l in links if l['ID_TRIMITERE'] == sending_id and same_nir(l, ctx, nir_id)]
    others = _all(cursor, 'SELECT n.SubunitId,n.GestiuneId,n.ExercitiuId,n.IdIntrare,n.Numar,n.NumarSub,n.Data,'
                  's.Name AS SUBUNITATE,g.Name AS GESTIUNE FROM EF_PrimiteVerconNir r '
                  'JOIN VC_NIR n ON n.SubunitId=r.SubunitId AND n.GestiuneId=r.GestiuneId '
                  'AND n.ExercitiuId=r.ExercitiuId AND n.IdIntrare=r.IdIntrare '
                  'JOIN VC_Subunitati s ON s.SubunitId=n.SubunitId '
                  'JOIN VC_Gestiuni g ON g.SubunitId=n.SubunitId AND g.GestiuneId=n.GestiuneId WHERE r.IdTrimitere=%s', (sending_id,))
    valid_others = {(l['SUBUNIT_ID'], l['GESTIUNE_ID'], l['EXERCITIU_ID'], l['NIR_ID']) for l in links
                    if l['ID_TRIMITERE'] == sending_id and l['VALID'] and not same_nir(l, ctx, nir_id)}
    return {'id': sending_id, 'nir': nir_id, 'version': nir['Version'],
            'number': ef['NrFact'], 'supplier': ef['DenumireP'], 'lines': ef_lines, 'nir_lines': receipt_lines,
            'allocations': [{'line': l['ID_LINIE'], 'nir_line': l['INTRARE_ID'], 'qty': queue._text(l['CANT_FACTURA']),
                             'nir_qty': queue._text(l['CANT_NIR']), 'valid': bool(l['VALID'])} for l in own],
            'other_receipts': [{'subunit': r['SUBUNITATE'], 'warehouse': r['GESTIUNE'], 'number': r['Numar'],
                                'sub_number': r['NumarSub'], 'date': queue._iso(r['Data'])}
                               for r in others if (r['SubunitId'], r['GestiuneId'], r['ExercitiuId'], r['IdIntrare']) in valid_others]}


def quantity(value):
    try:
        result = Decimal(str(value))
        if result.is_finite() and 0 < result < Decimal('10000000000000000') and result == result.quantize(Decimal('.0001')):
            return result
    except (InvalidOperation, ValueError, TypeError):
        pass
    raise NomError('INVALID', 'Cantitățile asociate trebuie să fie pozitive, cu maximum 4 zecimale.', 422)


def locked_receipt(cursor, ctx, nir_id, data, *, allow_initial=False):
    nir = receipt(cursor, ctx, nir_id, True, allow_initial=allow_initial)
    if _int(data.get('version')) != nir['Version']:
        raise NomError('CONFLICT', 'NIR-ul s-a modificat. Redeschideți asocierea.', 409)
    return nir


def drop_links(cursor, ctx, sending_id, nir_id, *, manual):
    identity = (sending_id, *_scope(ctx), nir_id)
    if manual:
        cursor.execute('DELETE FROM EF_VERCON_ALOCARI WHERE ID_TRIMITERE=%s AND SUBUNIT_ID=%s AND GESTIUNE_ID=%s '
                       'AND EXERCITIU_ID=%s AND NIR_ID=%s', identity)
    cursor.execute('DELETE FROM EF_PrimiteVerconNir WHERE IdTrimitere=%s AND SubunitId=%s AND GestiuneId=%s '
                   'AND ExercitiuId=%s AND IdIntrare=%s', identity)


def refresh_sending(cursor, sending_id, received_by=None):
    progress = shared.progress(cursor, sending_id)
    state = progress['state'] if progress is not None else (
        'PRELUATA' if _one(cursor, queue.ACTIVE_LEGACY_NIRS + ' LIMIT 1', (sending_id,)) else 'TRIMISA')
    if state == 'TRIMISA':
        cursor.execute("UPDATE EF_PrimiteVercon SET Stare='TRIMISA',UnPreluat=NULL,DataPreluat=NULL WHERE IdTrimitere=%s", (sending_id,))
    elif received_by:
        cursor.execute("UPDATE EF_PrimiteVercon SET Stare='PRELUATA',UnPreluat=%s,DataPreluat=NOW() WHERE IdTrimitere=%s",
                       (received_by, sending_id))
    else:
        cursor.execute("UPDATE EF_PrimiteVercon SET Stare='PRELUATA' WHERE IdTrimitere=%s", (sending_id,))
    return state


def unlink(cursor, ctx, sending_id, nir_id, data):
    locked_receipt(cursor, ctx, nir_id, data, allow_initial=True)
    check_link(cursor, ctx, nir_id, sending_id)
    invoice(cursor, sending_id, True)
    drop_links(cursor, ctx, sending_id, nir_id, manual=shared.ready(cursor))
    state = refresh_sending(cursor, sending_id)
    protocol.audit(cursor, ctx, 'efactura.unlink', 'EF_PrimiteVercon', sending_id, {'nir': nir_id}, {'state': state},
                   data['request_key'], request.headers.get('X-Request-Id'))
    return {'id': sending_id, 'state': state, 'linked': 0}


def save(cursor, ctx, sending_id, nir_id, data):
    require(cursor)
    nir = locked_receipt(cursor, ctx, nir_id, data)
    old_id = replacement_id(data.get('replace'))
    if old_id == sending_id:
        raise NomError('INVALID', 'Alegeți o altă eFactură.', 422)
    ef = invoice(cursor, sending_id, True) if old_id is None else lock_replacement(cursor, sending_id, old_id)
    if old_id is not None:
        check_link(cursor, ctx, nir_id, old_id, sending_id)
    compatible(cursor, nir, ef)
    legacy_guard(cursor, ctx, sending_id, nir_id, old_id)
    _, used_ef, used_nir = budgets(cursor, ctx, sending_id, nir_id, old_id)
    source = {l['IdLinie']: l for l in _all(cursor, 'SELECT l.IdLinie,l.Denumire,l.Cpv,l.Cant FROM EF_PrimiteLinii l WHERE l.IdPrimita=%s AND ' + shared.sent_line_sql('%s', 'l.IdLinie'),
                                            (ef['IdPrimita'], sending_id))}
    target = {l['ID']: l for l in nir_lines(cursor, ctx, nir_id)}
    raw = data.get('allocations')
    if not isinstance(raw, list) or len(raw) > 1000:
        raise NomError('INVALID', 'Lista asocierilor este invalidă (maximum 1000).', 422)
    clean, seen, learned = [], set(), set()
    for item in raw:
        if not isinstance(item, dict):
            raise NomError('INVALID', 'Asociere invalidă.', 422)
        lid, nid = _int(item.get('line')), _int(item.get('nir_line'))
        if lid not in source or nid not in target or (lid, nid) in seen:
            raise NomError('INVALID', 'Un rând este necunoscut sau o asociere apare de două ori.', 422)
        seen.add((lid, nid))
        qty, nqty = quantity(item.get('qty')), quantity(item.get('nir_qty'))
        used_ef[lid] = used_ef.get(lid, Decimal(0)) + qty
        used_nir[nid] = used_nir.get(nid, Decimal(0)) + nqty
        if used_ef[lid] > (source[lid]['Cant'] or 0) or used_nir[nid] > (target[nid]['Cant'] or 0):
            raise NomError('EF_QUANTITY', 'Cantitatea depășește restul disponibil în factură sau NIR. Reîncărcați asocierea.', 409)
        clean.append((lid, nid, qty, nqty))
        if item.get('learn') is True:
            learned.add((lid, nid))
    if old_id is not None and not clean:
        raise NomError('INVALID', 'Asociați cel puțin o poziție din factura nouă înainte de salvare.', 422)
    request_id = request.headers.get('X-Request-Id')
    cpv_updated = efactura_product_cpv.apply(cursor, ctx, sending_id, nir_id, source, target, clean, data['request_key'], request_id)
    replaced = {}
    if old_id is not None:
        drop_links(cursor, ctx, old_id, nir_id, manual=True)
        replaced = {'replaced_sending': old_id, 'replaced_state': refresh_sending(cursor, old_id)}
    drop_links(cursor, ctx, sending_id, nir_id, manual=True)
    if clean:
        cursor.execute('INSERT IGNORE INTO EF_VERCON_ASOCIERI (ID_TRIMITERE) VALUES (%s)', (sending_id,))
        cursor.execute('INSERT IGNORE INTO EF_PrimiteVerconNir (IdTrimitere,SubunitId,GestiuneId,ExercitiuId,IdIntrare) VALUES (%s,%s,%s,%s,%s)',
                       (sending_id, *_scope(ctx), nir_id))
    for lid, nid, qty, nqty in clean:
        line = target[nid]
        cursor.execute('INSERT INTO EF_VERCON_ALOCARI (ID_TRIMITERE,ID_LINIE,SUBUNIT_ID,GESTIUNE_ID,EXERCITIU_ID,NIR_ID,NIR_VERSION,INTRARE_ID,'
                       'PRODUS_PRET_ID,CANT_FACTURA,CANT_NIR,CANT_SURSA,PRET_SURSA,CREAT_DE) VALUES (%s,%s,%s,%s,%s,%s,%s,%s,%s,%s,%s,%s,%s,%s)',
                       (sending_id, lid, *_scope(ctx), nir_id, nir['Version'], nid, line['IdProdus'], qty, nqty, line['Cant'], line['PUTVA'], ctx['email']))
        if (lid, nid) in learned and ef['CuiNormalizat'] and source[lid]['Denumire']:
            cursor.execute('INSERT INTO EF_VERCON_CORELARI (CUI,NUME_HASH,NUME_EFACTURA,SUBUNIT_ID,GESTIUNE_ID,PRODUS_ID,NUME_VERCON,CONFIRMAT_DE) '
                           'VALUES (%s,%s,%s,%s,%s,%s,%s,%s) ON DUPLICATE KEY UPDATE NUME_VERCON=VALUES(NUME_VERCON),'
                           'CONFIRMAT_DE=VALUES(CONFIRMAT_DE),CONFIRMAT_LA=NOW()',
                           (ef['CuiNormalizat'], shared.name_hash(source[lid]['Denumire']), source[lid]['Denumire'], ctx['subunit'], ctx['gestiune'],
                            line['PRODUCT_ID'], line['DenumireP'], ctx['email']))
    state = refresh_sending(cursor, sending_id, ctx['email'] if clean else None)
    protocol.audit(cursor, ctx, 'efactura.replace' if replaced else 'efactura.associate', 'EF_PrimiteVercon', sending_id, None,
                   {'nir': nir_id, 'allocations': clean, 'state': state, 'learned': sorted(learned), 'cpv_updated': cpv_updated, **replaced},
                   data['request_key'], request_id)
    return {'id': sending_id, 'state': state, 'linked': len(clean), 'cpv_updated': cpv_updated}


def register(bp, guard, scoped, session, build, need):
    @bp.get('/api/vercon/doc/nir/efactura/<int:sending_id>/association')
    @guard('read')
    def get_association(sending_id):
        need('nir')
        verified = scoped('read')
        with session(False) as cursor:
            ctx, _ = build(cursor, verified, 'nir', False)
            return detail(cursor, ctx, sending_id, _int(request.args.get('nir')), replacement_id(request.args.get('replace')))

    def write(sending_id, operation, apply):
        need('nir')
        data = request.get_json(silent=True)
        if not isinstance(data, dict) or not isinstance(data.get('request_key'), str) or not 1 <= len(data['request_key']) <= 64:
            raise NomError('INVALID', 'Cerere invalidă sau cheie de cerere lipsă.', 400)
        verified = scoped('receptie', data)
        with session(True) as cursor:
            ctx, _ = build(cursor, verified, 'nir', True)
            fp = protocol.fingerprint(operation, sending_id, data)
            replay = protocol.replayed(cursor, ctx, data['request_key'], fp)
            if replay is not None:
                return replay
            result = apply(cursor, ctx, sending_id, _int(data.get('nir')), data)
            protocol.remember(cursor, ctx, data['request_key'], fp, result)
            return result

    @bp.post('/api/vercon/doc/nir/efactura/<int:sending_id>/association')
    @guard('receptie')
    def post_association(sending_id):
        return write(sending_id, 'efactura.association', save)

    @bp.post('/api/vercon/doc/nir/efactura/<int:sending_id>/unlink')
    @guard('receptie')
    def post_unlink(sending_id):
        return write(sending_id, 'efactura.unlink', unlink)
