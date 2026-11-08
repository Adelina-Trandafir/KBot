"""VC12-04: standalone web inventory workbench, using the existing portal and VR rights."""
from contextlib import contextmanager
import uuid

from flask import current_app, g, request, send_from_directory
from routes.auth.session_store import STORE
from routes.admin_cli.inventory_similarity import compare, number
from . import inventory_excel, inventory_source, inventory_write, protocol
from .nomenclatoare import NomError
from .repository import ScopedRepository
from utils.vercon_suppliers import name_key

NOTE = 'vercon_inventory'
TTL = 4 * 60 * 60


def register(bp, guard, scoped, open_db, build):
    @contextmanager
    def session(write=False):
        if open_db is None:
            raise NomError('NO_DATABASE', 'Inventarul nu este disponibil în previzualizare.', 503)
        with open_db(g.portal['db_name']) as conn:
            cursor = conn.cursor(dictionary=True)
            try:
                cursor.execute('SET TRANSACTION ISOLATION LEVEL ' + ('READ COMMITTED' if write else 'REPEATABLE READ'))
                cursor.execute('START TRANSACTION' if write else 'START TRANSACTION WITH CONSISTENT SNAPSHOT, READ ONLY')
                yield cursor
                if write:
                    conn.commit()
                else:
                    conn.rollback()
            except Exception:
                conn.rollback()
                raise
            finally:
                cursor.close()

    def body():
        data = request.get_json(silent=True)
        if not isinstance(data, dict):
            raise NomError('INVALID', 'Cerere invalidă.', 400)
        return data

    def uploaded():
        file = request.files.get('file')
        if file is None or not (file.filename or '').casefold().endswith('.xlsx'):
            raise NomError('INVALID', 'Alegeți un fișier XLSX.', 422)
        return file, file.stream.read(20000001)

    def selection(vctx):
        return {key: vctx['selection'][key] for key in ('subunit', 'gestiune')}

    def workspace(data, vctx):
        identity = data.get('workspace')
        saved = STORE.get_note(identity, NOTE) if isinstance(identity, str) and len(identity) == 32 else None
        if (not saved or saved['owner'] != g.portal['email'] or saved['unit'] != g.portal['db_name'] or
                saved['selection'] != selection(vctx)):
            raise NomError('INVENTORY_EXPIRED', 'Comparația nu mai este disponibilă în acest context. Citiți din nou fișierul.', 409)
        STORE.put_note(identity, NOTE, saved, TTL)
        return saved

    @bp.get('/vercon-inventar')
    def inventory_page():
        response = send_from_directory(current_app.static_folder, 'vercon-inventar.html')
        response.headers.update({'Cache-Control': 'no-store', 'X-Content-Type-Options': 'nosniff',
                                 'Content-Security-Policy': "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; "
                                 "img-src 'self' data:; connect-src 'self'; frame-ancestors 'none'; base-uri 'none'"})
        return response

    @bp.post('/api/vercon/inventory/workbook')
    @guard('read')
    def inventory_workbook():
        _, data = uploaded()
        return inventory_excel.read(data)

    @bp.post('/api/vercon/inventory/load')
    @guard('read')
    def inventory_load():
        vctx = scoped('read', request.form)
        if vctx['gestiune']['type'] != 'OI':
            raise NomError('INVALID', 'Alegeți o subgestiune de obiecte de inventar (OI).', 422)
        try:
            account = int(request.form.get('account', ''))
        except ValueError:
            raise NomError('INVALID', 'Alegeți contul.', 422) from None
        file, content = uploaded()
        parsed = inventory_excel.read(content, request.form.get('sheet', ''))
        rows = [r for r in parsed['rows'] if r['kind'] == vctx['gestiune']['type']]
        if not rows:
            return {'workspace': None, 'rows': [], 'products': 0, 'date': inventory_source.ON.isoformat(),
                    'omitted': parsed['omitted'], 'type': 'OI', 'errors': parsed['errors']}
        with session() as cursor:
            ctx, _ = build(cursor, vctx, None, False)
            products, fingerprint, _ = inventory_source.read(cursor, ctx, account)
        identity = uuid.uuid4().hex
        source_hash = protocol.fingerprint([{k: v for k, v in row.items() if k != 'row'} for row in rows],
                                           parsed['sheet'], account, 'OI')
        saved = {'owner': g.portal['email'], 'unit': g.portal['db_name'], 'selection': selection(vctx),
                 'account': account, 'rows': rows, 'products': inventory_source.serializable(products),
                 'fingerprint': fingerprint, 'operation_key': 'inventory-' + source_hash[:50],
                 'source_hash': source_hash, 'source_name': file.filename.rsplit('/', 1)[-1].rsplit('\\', 1)[-1],
                 'sheet': parsed['sheet']}
        STORE.put_note(identity, NOTE, saved, TTL)
        return {'workspace': identity, 'rows': rows, 'products': len(products), 'date': inventory_source.ON.isoformat(),
                'omitted': parsed['omitted'], 'type': 'OI', 'errors': parsed['errors']}

    @bp.post('/api/vercon/inventory/candidates')
    @guard('read')
    def inventory_candidates():
        data = body()
        vctx = scoped('read', data)
        if vctx['gestiune']['type'] != 'OI':
            raise NomError('INVALID', 'Miniaplicația lucrează exclusiv cu OI.', 422)
        saved = workspace(data, vctx)
        row = next((r for r in saved['rows'] if r['row'] == data.get('row')), None)
        if row is None:
            raise NomError('INVALID', 'Alegeți o poziție Excel.', 422)
        result = compare([row], saved['products'], limit=len(saved['products']), min_name_score=0)[0]
        result['candidates'].sort(key=lambda p: (-p['score'], -p['name_score'], name_key(p['name']),
                                                number(p['price']) or 0, name_key(p['code'])))
        return {'row': row['row'], 'candidates': result['candidates']}

    @bp.post('/api/vercon/inventory/send')
    @guard('import')
    def inventory_send():
        data = body()
        vctx = scoped('import', data)
        saved = workspace(data, vctx)
        if vctx['gestiune']['type'] != 'OI':
            raise NomError('INVALID', 'Operația este disponibilă exclusiv pentru OI.', 422)
        fp = protocol.fingerprint('inventory', saved['source_hash'], data.get('assignments', []))
        with session(True) as cursor:
            ctx, exercise = build(cursor, vctx, None, True)
            previous = protocol.replayed(cursor, ctx, saved['operation_key'], fp)
            if previous is not None:
                return previous
            products, fingerprint, _ = inventory_source.read(cursor, ctx, saved['account'])
            if fingerprint != saved['fingerprint']:
                raise NomError('INVENTORY_CHANGED', 'Stocul, prețurile sau produsele s-au schimbat. Reîncărcați comparația înainte de trimitere.', 409)
            repo = ScopedRepository(cursor, ctx['subunit'], ctx['gestiune'], ctx['exercitiu'])
            result = inventory_write.apply(cursor, repo, ctx, exercise,
                                           {**saved, 'current_products': products}, data,
                                           request.headers.get('X-Request-Id'))
            protocol.remember(cursor, ctx, saved['operation_key'], fp, result)
        return result
