"""Dedicated HTTP surface for transfers between subunits."""
from flask import request
from utils.clock import bucharest_today

from . import bu, bu_catalog, bu_links, oi, protocol, receipt_destinations
from .documents import _all, _int, _iso, _numarsub_column, _one, _scope
from .nomenclatoare import NomError

PATH = '/api/vercon/transfer-unitati'


def register(bp, guard, scoped, session, build, repo_of):
    def read(cursor):
        ctx, _ = build(cursor, scoped('read'), 'bu', False)
        bu_links.require(cursor)
        return ctx

    @bp.get(PATH)
    @guard('oi_transfer')
    def bu_context():
        with session(False) as cursor:
            ctx = read(cursor)
            fraction = 'n.NumarSub' if _numarsub_column(cursor, ctx) else '0'
            documents = _all(cursor, f'SELECT t.ID AS id,t.DATA_BU AS day,t.ANULAT AS cancelled,'
                             't.MOTIV_ANULARE AS reason,t.EXPLICATIE AS explanation,'
                             's.Name AS subunit,g.Name AS gestiune,c.Denumire AS receiver,p.Denumire AS giver,'
                             f't.DEST_NIR AS nir,t.DEST_BP AS bp,n.Numar AS nir_number,{fraction} AS nir_sub,'
                             '(SELECT MIN(b.NRBP) FROM VC_OI_Predare b WHERE b.SubunitId=t.DEST_SUBUNITATE '
                             'AND b.GestiuneId=t.DEST_GESTIUNE AND b.ExercitiuId=t.DEST_EXERCITIU AND b.IDBP=t.DEST_BP) AS bp_number '
                             f'FROM {bu_links.TABLE} t JOIN VC_Subunitati s ON s.SubunitId=t.DEST_SUBUNITATE '
                             'JOIN VC_Gestiuni g ON g.SubunitId=t.DEST_SUBUNITATE AND g.GestiuneId=t.DEST_GESTIUNE '
                             'LEFT JOIN VC_Clienti c ON c.SubunitId=t.DEST_SUBUNITATE AND c.GestiuneId=t.DEST_GESTIUNE AND c.IDC=t.PRIMITOR '
                             'LEFT JOIN VC_Clienti p ON p.SubunitId=t.SURSA_SUBUNITATE AND p.GestiuneId=t.SURSA_GESTIUNE AND p.IDC=t.PREDATOR '
                             'LEFT JOIN VC_NIR n ON n.SubunitId=t.DEST_SUBUNITATE AND n.GestiuneId=t.DEST_GESTIUNE '
                             'AND n.ExercitiuId=t.DEST_EXERCITIU AND n.IdIntrare=t.DEST_NIR '
                             'WHERE t.SURSA_SUBUNITATE=%s AND t.SURSA_GESTIUNE=%s AND t.SURSA_EXERCITIU=%s '
                             'ORDER BY t.DATA_BU DESC,t.ID DESC', _scope(ctx))
            holders = _all(cursor, 'SELECT IDC AS id,Denumire AS label FROM VC_Clienti '
                           'WHERE SubunitId=%s AND GestiuneId=%s AND Activ=1', (ctx['subunit'], ctx['gestiune']))
            return {'today': bucharest_today().isoformat(), 'destinations': bu_catalog.destinations(cursor, ctx),
                    'holders': holders, 'documents': [{**doc, 'day': _iso(doc['day'])} for doc in documents]}

    @bp.get(PATH + '/eligible')
    @guard('oi_transfer')
    def bu_eligible():
        day = protocol.to_date(request.args.get('date'), 'Data transferului')
        holder = _int(request.args.get('giver'))
        with session(False) as cursor:
            ctx = read(cursor)
            return {'lines': bu.eligible(cursor, ctx, day, holder)}

    @bp.get(PATH + '/<int:identity>')
    @guard('oi_transfer')
    def bu_detail(identity):
        with session(False) as cursor:
            ctx = read(cursor)
            row = saved(cursor, ctx, identity)
            return {'lines': [{
                'name': product['DenumireP'], 'code': product['COD'], 'quantity': format(line['Cant'], 'f'),
                'price': format(line['PUTVA'], 'f'), 'value': format(line['ValoareCUTVA'], 'f'), 'um': product['UM'],
            } for line in oi._rows(cursor, ctx, 'bt', row['IDBT'])
                for product in (bu_catalog.source_product(cursor, ctx, line['IdProdus']),)]}

    def saved(cursor, ctx, identity):
        row = _one(cursor, f'SELECT * FROM {bu_links.TABLE} WHERE SURSA_SUBUNITATE=%s AND SURSA_GESTIUNE=%s '
                   'AND SURSA_EXERCITIU=%s AND ID=%s', (*_scope(ctx), identity))
        if not row:
            raise NomError('NOT_FOUND', 'Bonul BU nu există în gestiunea curentă.', 404)
        return row

    def write(identity=None):
        data = request.get_json(silent=True)
        if not isinstance(data, dict) or not isinstance(data.get('request_key'), str) or not 1 <= len(data['request_key']) <= 64:
            raise NomError('INVALID', 'Cerere BU invalidă.', 400)
        verified = scoped('oi_transfer', data)
        request_id = (request.headers.get('X-Request-Id') or '')[:64] or None
        fingerprint = protocol.fingerprint('bu.cancel' if identity else 'bu.create', identity, data)
        with session(True) as cursor:
            ctx, exercise = build(cursor, verified, 'bu', False)
            protocol.ensure_open(exercise)
            bu_links.require(cursor)
            replay = protocol.replayed(cursor, ctx, data['request_key'], fingerprint)
            if replay is not None:
                return replay
            row = saved(cursor, ctx, identity) if identity else None
            pair = ((row['DEST_SUBUNITATE'], row['DEST_GESTIUNE']) if row else
                    (_int(data.get('destination_subunit')), _int(data.get('destination_gestiune'))))
            destination = None if row else next((item for item in bu_catalog.destinations(cursor, ctx)
                                                   if (item['subunit'], item['gestiune']) == pair), None)
            if not row and destination is None:
                raise NomError('BU_DESTINATION', 'Alegeți o gestiune eligibilă din altă subunitate.', 422)
            target_verified = scoped('read', {'subunit': pair[0], 'gestiune': pair[1]}, version='')
            target, target_exercise = build(cursor, target_verified, 'bu', False)
            if (target['subunit'], target['gestiune']) != pair or target['type'] != ctx['type']:
                raise NomError('BU_DESTINATION', 'Gestiunea de destinație nu mai este disponibilă.', 409)
            protocol.ensure_open(target_exercise)
            with receipt_destinations.reserve(cursor, ctx, [{'ctx': target}]):
                replay = protocol.replayed(cursor, ctx, data['request_key'], fingerprint)
                if replay is not None:
                    return replay
                repo, target_repo = repo_of(cursor, ctx), repo_of(cursor, target)
                if row:
                    row = saved(cursor, ctx, identity)
                    if target['exercitiu'] != row['DEST_EXERCITIU']:
                        raise NomError('BU_DESTINATION', 'Exercițiul de destinație nu mai corespunde bonului BU.', 409)
                    result = bu.cancel(cursor, repo, target_repo, ctx, target, row, data, request_id)
                else:
                    current = next((item for item in bu_catalog.destinations(cursor, ctx)
                                    if (item['subunit'], item['gestiune']) == pair), None)
                    if current is None:
                        raise NomError('BU_DESTINATION', 'Gestiunea de destinație nu mai este disponibilă.', 409)
                    target.update(bu.target_context(target, current))
                    result = bu.create(cursor, repo, target_repo, ctx, target, target_exercise, current, data, request_id)
                protocol.remember(cursor, ctx, data['request_key'], fingerprint, result)
                return result

    @bp.post(PATH)
    @guard('oi_transfer')
    def bu_create():
        return write()

    @bp.post(PATH + '/<int:identity>/cancel')
    @guard('oi_transfer')
    def bu_cancel(identity):
        return write(identity)
