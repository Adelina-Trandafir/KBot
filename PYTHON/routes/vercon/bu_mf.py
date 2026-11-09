"""MF identity and dated ownership across BU transfers."""
from collections import defaultdict

from . import bu_links, depreciation_source, protocol, stock
from .documents import _all, _one
from .nomenclatoare import NomError

FIELDS = ('DataIntrare', 'DNF', 'DF', 'ValoareLuna', 'ValoareLunara')
ORIGIN = ('ORIGINE_SUBUNITATE', 'ORIGINE_GESTIUNE', 'ORIGINE_VARIANTA')


def movements(cursor, ctx, *, cancelled=False):
    sql = []
    for side, delta in (('SURSA', -1), ('DEST', 1)):
        sql.append(f'SELECT l.{side}_VARIANTA AS variant,t.DATA_BU AS day,t.ID AS bu,'
                   f'CASE WHEN t.ANULAT=0 THEN {delta} ELSE 0 END AS delta,'
                   't.DEST_NIR AS nir,t.DEST_BP AS bp,l.ORIGINE_SUBUNITATE,l.ORIGINE_GESTIUNE,l.ORIGINE_VARIANTA '
                   f'FROM {bu_links.LINES} l JOIN {bu_links.TABLE} t ON t.ID=l.BU_ID '
                   f'WHERE t.{side}_SUBUNITATE=%(s)s AND t.{side}_GESTIUNE=%(g)s AND l.ORIGINE_VARIANTA IS NOT NULL '
                   + ('' if cancelled else 'AND t.ANULAT=0'))
    return _all(cursor, ' UNION ALL '.join(sql) + ' ORDER BY day,bu',
                {'s': ctx['subunit'], 'g': ctx['gestiune']})


def by_variant(cursor, ctx):
    result = defaultdict(list)
    if bu_links.ready(cursor):
        for row in movements(cursor, ctx, cancelled=True):
            result[row['variant']].append(row)
    return result


def origin_of(ctx, variant, rows):
    return tuple(rows[0][key] for key in ORIGIN) if rows else (ctx['subunit'], ctx['gestiune'], variant)


def owns(ctx, variant, rows, day):
    initial = int(origin_of(ctx, variant, rows) == (ctx['subunit'], ctx['gestiune'], variant))
    return initial + sum(row['delta'] for row in rows if row['day'] <= day) == 1


def receive(cursor, source_repo, target_repo, ctx, target, line, source, product_id, day, request_id):
    variant = line['variant']
    if line['Cant'] != 1 or not (source['COD'] or '').strip():
        raise NomError('BU_MF', 'Un număr de inventar MF se transferă cu cantitatea exactă de 1.', 422)
    if source['PUTVA'] is None or source['PUTVA'] < 0:
        raise NomError('BU_MF', 'Completați valoarea de intrare a activului înainte de transfer.', 422)
    assets = source_repo.select('VC_ProduseM', {'IdProdus': variant})
    if len(assets) != 1 or assets[0]['DataIntrare'] is None or stock.day_of(assets[0]['DataIntrare']) > day:
        raise NomError('BU_MF', 'Activul trebuie să aibă o fișă MF cu data de intrare cel târziu la data BU.', 409)
    outgoing = [row for row in movements(cursor, ctx) if row['variant'] == variant]
    if any(row['day'] > day for row in outgoing) or not owns(ctx, variant, outgoing, day):
        raise NomError('BU_MF', 'Activul are un BU ulterior sau nu aparține sursei la data aleasă.', 409)
    identity = origin_of(ctx, variant, outgoing)
    matches = _all(cursor, 'SELECT IdProdus,ID,Activ,PUTVA FROM VC_ProduseP WHERE SubunitId=%s AND GestiuneId=%s '
                   'AND TRIM(COD)=TRIM(%s)', (target['subunit'], target['gestiune'], source['COD']))
    values = {field: assets[0][field] for field in FIELDS}
    if matches:
        found = matches[0]
        received = [row for row in movements(cursor, target, cancelled=True) if row['variant'] == found['IdProdus']]
        active = [row for row in movements(cursor, target) if row['variant'] == found['IdProdus']]
        if (len(matches) != 1 or not found['Activ'] or found['ID'] != product_id
                or found['PUTVA'] != source['PUTVA'] or origin_of(target, found['IdProdus'], received) != identity):
            raise NomError('BU_MF', 'Numărul de inventar este deja folosit în destinație pentru altă fișă sau valoare.', 409)
        if any(row['day'] > day for row in active) or owns(target, found['IdProdus'], received, day):
            raise NomError('BU_MF', 'Activul figurează deja în destinație sau are un BU ulterior.', 409)
        target_variant = found['IdProdus']
        if len(target_repo.select('VC_ProduseM', {'IdProdus': target_variant})) != 1:
            raise NomError('BU_MF', 'Varianta existentă din destinație trebuie să aibă o singură fișă MF.', 409)
        target_repo.update('VC_ProduseM', values, {'IdProdus': target_variant}, bump_version=True)
    else:
        target_variant = target_repo.insert('VC_ProduseP', {
            'ID': product_id, 'COD': source['COD'], 'PUTVA': source['PUTVA'], 'Activ': 1})
        row_id = target_repo.insert('VC_ProduseM', {'IdProdus': target_variant, **values})
        protocol.audit(cursor, target, 'bu.mf.receive', 'VC_ProduseM', row_id, None,
                       {'origin': identity, 'variant': target_variant, **values}, None, request_id)
    return target_variant, identity


def acquisition(cursor, ctx, variant, rows, local_events):
    """Use original acquisition and the first actual BP even after a return BU."""
    subunit, gestiune, original = origin_of(ctx, variant, rows)
    source = {**ctx, 'subunit': subunit, 'gestiune': gestiune}
    exercise = protocol.exercise_of(cursor, subunit, gestiune, 'MF', None)
    source['exercitiu'] = exercise['id']
    _, original_events = depreciation_source.load(cursor, source, original)
    source_transfers = [row for row in movements(cursor, source) if row['variant'] == original and row['delta'] == 1]
    generated_nir = {row['nir'] for row in source_transfers}
    generated_bp = {row['bp'] for row in source_transfers}
    facts = [event for event in original_events.get(original, []) if
             event['kind'] == 'receipt' and event['document_id'] not in generated_nir
             or event['kind'] == 'storno'
             or event['kind'] == 'handover' and event['document_id'] not in generated_bp]
    first_handover = _one(cursor, f'SELECT MIN(DATE(p.DATABP)) AS day FROM {bu_links.LINES} l '
                          f'JOIN {bu_links.TABLE} t ON t.ID=l.BU_ID '
                          'JOIN VC_OI_Predare p ON p.SubunitId=t.DEST_SUBUNITATE AND p.GestiuneId=t.DEST_GESTIUNE '
                          'AND p.ExercitiuId=t.DEST_EXERCITIU AND p.IdProdus=l.DEST_VARIANTA '
                          'WHERE t.ANULAT=0 AND p.Anulat=0 AND p.Cant>0 AND p.DATABP>=t.DATA_BU '
                          'AND l.ORIGINE_SUBUNITATE=%s AND l.ORIGINE_GESTIUNE=%s AND l.ORIGINE_VARIANTA=%s',
                          (subunit, gestiune, original))
    if first_handover['day']:
        facts.append({'kind': 'handover', 'date': first_handover['day'], 'quantity': 1})
    facts.extend(event for event in local_events if event['kind'] in ('scrapping', 'transfer'))
    zec = _one(cursor, 'SELECT Zec FROM VC_Gestiuni WHERE SubunitId=%s AND GestiuneId=%s', (subunit, gestiune))
    return facts, int(zec['Zec']) if zec and zec['Zec'] is not None else ctx['zec']
