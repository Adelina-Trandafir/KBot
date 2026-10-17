"""Local VERCON food-list preview. Explicit fixtures; no main.py or SQL routes."""
import argparse
from datetime import date
from pathlib import Path
import secrets
import sys
from threading import RLock

HERE = Path(__file__).resolve().parent
REPO = HERE.parents[1]
PYTHON_DIR = REPO / 'PYTHON'
sys.path.insert(0, str(PYTHON_DIR))

from flask import Flask, jsonify, request, send_from_directory
from routes.vercon import food_balance, report_catalog, report_output, report_service
from routes.vercon.nomenclatoare import NomError
from demo_data import CTX, DISHES, SELECTION, YEAR, DemoData, DemoSource, fail

UNIT = 'LOCAL_VERCON_DEMO'
VERSION = 'vercon-local-demo-v1'


def build_app():
    app = Flask(__name__, static_folder=str(PYTHON_DIR / 'static'))
    app.config.update(SEND_FILE_MAX_AGE_DEFAULT=0, MAX_CONTENT_LENGTH=1024 * 1024)
    app.json.ensure_ascii = False
    data, snapshots, mutex = DemoData(), {}, RLock()

    @app.before_request
    def local_only():
        # No LAN mode, proxy, real login, database factory, or upstream API.
        if request.remote_addr not in ('127.0.0.1', '::1') or request.host.split(':')[0] not in ('127.0.0.1', 'localhost'):
            return jsonify(error='Preview disponibil numai pe localhost.'), 403
        if request.method not in ('GET', 'HEAD', 'OPTIONS'):
            origin = request.headers.get('Origin')
            if origin and origin != request.host_url.rstrip('/'):
                return jsonify(error='Origine invalidă pentru preview.'), 403

    @app.after_request
    def no_cache(response):
        response.headers['Cache-Control'] = 'no-store'
        response.headers['X-Content-Type-Options'] = 'nosniff'
        response.headers['X-Frame-Options'] = 'DENY'
        return response

    @app.errorhandler(NomError)
    def input_error(error):
        return jsonify(error=str(error), reason=error.reason, fields=error.fields), error.status

    @app.errorhandler(404)
    def missing(_error):
        return jsonify(error='Funcție neinclusă în preview-ul local Lista de alimente.',
                       reason='DEMO_UNAVAILABLE'), 404

    @app.get('/')
    @app.get('/demo')
    @app.get('/portal')
    def home():
        return send_from_directory(HERE, 'index.html')

    @app.get('/preview.css')
    def stylesheet():
        return send_from_directory(HERE, 'preview.css')

    @app.get('/vercon')
    def vercon():
        # The production HTML and JS run unchanged. Only this local response skips
        # the early token redirect; production still uses all its normal guards.
        html = (PYTHON_DIR / 'static' / 'vercon.html').read_text(encoding='utf-8')
        html = html.replace('<script src="/static/js/vercon/gate.js"></script>', '', 1)
        html = html.replace('<title>VERCON</title>', '<title>VERCON · DEMO LOCAL</title>', 1)
        return html

    @app.get('/api/portal/me')
    def me():
        return jsonify(db_name=UNIT, email='DEMO LOCAL · date în memorie', authenticated=True)

    @app.route('/api/portal/session/<action>', methods=['GET', 'POST'])
    def session(action):
        if action not in ('check', 'renew'):
            fail('Acțiune demonstrativă necunoscută.', 404)
        return jsonify(authenticated=True, expires_in=86400)

    @app.post('/api/portal/logout')
    def logout():
        return jsonify(ok=True)

    @app.get('/api/vercon/context')
    def context():
        return jsonify(ready=True, version=VERSION, selection=SELECTION, unit=UNIT,
            type_locked=True, operations=['read', 'alimente', 'rapoarte'],
            options={'subunits': [{'id': 1, 'name': 'Subunitate DEMO LOCAL'}],
                     'gestiuni': [{'id': 1, 'name': 'Alimente DEMO', 'type': 'ALIMENTE'}], 'years': [YEAR]},
            menu=[{'key': 'lista_alimente', 'label': 'Lista Alimente', 'group': 'Iesiri', 'enabled': True},
                  {'key': 'rapoarte', 'label': 'Rapoarte', 'group': 'Rapoarte', 'enabled': True}])

    @app.get('/api/vercon/forms/<form>/open')
    def open_form(form):
        if form not in ('lista_alimente', 'rapoarte'):
            fail('Preview-ul include Lista de alimente și raportul acesteia.', 404)
        return jsonify(form=form, selection=SELECTION, version=VERSION, type='ALIMENTE',
                       write=form == 'lista_alimente', nom=[], columns={'lines': [], 'header': []})

    @app.post('/api/vercon/lock/<action>')
    def local_lock(action):
        if action == 'acquire':
            return jsonify(token='local-memory-only', ttl=3600)
        if action in ('release', 'heartbeat'):
            return jsonify(ok=True, released=action == 'release')
        fail('Operație demonstrativă necunoscută.', 404)

    @app.get('/api/vercon/doc/lista/list')
    def documents():
        with mutex:
            return jsonify(documents=data.listing(), can_write=True, closed=False, zec=4)

    @app.get('/api/vercon/doc/lista/lookups')
    def lookups():
        with mutex:
            explanations = [item for doc in data.documents.values()
                            for meal in doc['meals'].values() for item in meal['items']]
            return jsonify(dishes=DISHES, explanations=explanations, today=f'{YEAR}-10-17')

    @app.get('/api/vercon/doc/lista/bons')
    def bons():
        exclude = request.args.get('exclude', type=int)
        with mutex:
            return jsonify(bons=data.eligible(exclude))

    @app.get('/api/vercon/doc/lista/balance')
    def balance():
        try:
            day = date.fromisoformat(request.args.get('date', ''))
        except ValueError:
            fail('Alegeți o dată validă.')
        with mutex:
            source = DemoSource(data, {'document': 130})
            amount = food_balance.before(source, day, request.args.get('exclude', type=int))
            return jsonify(food_balance.columns(amount, CTX['scale']))

    @app.get('/api/vercon/doc/lista/<int:doc_id>')
    def document(doc_id):
        with mutex:
            return jsonify(data.get(doc_id))

    @app.post('/api/vercon/doc/lista')
    @app.put('/api/vercon/doc/lista/<int:doc_id>')
    def save(doc_id=None):
        with mutex:
            return jsonify(document=data.save(request.get_json(silent=True) or {}, doc_id))

    @app.post('/api/vercon/doc/lista/<int:doc_id>/cancel')
    def cancel(doc_id):
        body = request.get_json(silent=True) or {}
        with mutex:
            doc = data.get(doc_id)
            if body.get('version') != doc['version'] or doc['annulled']:
                fail('Lista a fost schimbată. Redeschideți lista.', 409, 'CONFLICT')
            reason = str(body.get('reason') or '').strip()
            if not reason:
                fail('Completați motivul anulării.')
            doc.update(annulled=True, reason=reason, annulled_by='DEMO LOCAL', version=doc['version'] + 1)
            doc['bons'] = []
            data.documents[doc_id] = doc
            return jsonify(document=doc)

    def spec_for(name):
        if name != 'ListaAlimente2015':
            fail('În preview este disponibil raportul Lista de alimente.', 404)
        return report_catalog.spec_of(name, 'ALIMENTE')

    @app.get('/api/vercon/reports/catalog')
    def catalog():
        return jsonify(reports=[spec_for('ListaAlimente2015')])

    @app.get('/api/vercon/reports/<name>/selection')
    def selection(name):
        with mutex:
            docs = [{'id': row['id'], 'label': f"Lista {row['number']} / {row['date']}"}
                    for row in data.listing() if not row['annulled']
                    and request.args.get('start', f'{YEAR}-01-01') <= row['date'] <= request.args.get('end', f'{YEAR}-12-31')]
            return jsonify(spec=spec_for(name), documents=docs)

    def model_for(name, doc_id):
        spec_for(name)
        try:
            params = {'document': doc_id,
                      'start': date.fromisoformat(request.args.get('start', f'{YEAR}-01-01')),
                      'end': date.fromisoformat(request.args.get('end', f'{YEAR}-12-31'))}
        except ValueError:
            fail('Intervalul raportului este invalid.')
        source = DemoSource(data, params)
        # Use the actual builder and renderers. Only the data source is synthetic.
        return report_service.build(None, UNIT, dict(CTX), name, params, 'Alimente DEMO', source=source)

    @app.get('/api/vercon/reports/<name>/generate')
    def generate(name):
        doc_id = request.args.get('document', type=int)
        if doc_id is None:
            fail('Selectați o singură listă demonstrativă.')
        with mutex:
            model = model_for(name, doc_id)
            if len(snapshots) >= 40:
                snapshots.pop(next(iter(snapshots)))
            token = secrets.token_urlsafe(24)
            snapshots[token] = model
            return jsonify(**model, snapshot=token)

    def export_model(model, output):
        if output == 'pdf':
            return app.response_class(report_output.pdf(model), mimetype='application/pdf')
        if output == 'xlsx':
            response = app.response_class(report_output.xlsx(model),
                mimetype='application/vnd.openxmlformats-officedocument.spreadsheetml.sheet')
            response.headers['Content-Disposition'] = f'attachment; filename="{model["filename"]}.xlsx"'
            return response
        fail('Format necunoscut.', 404)

    @app.get('/api/vercon/reports/export/<token>/<output>')
    def export(token, output):
        with mutex:
            if token not in snapshots:
                fail('Regenerați raportul demonstrativ.', 409, 'REPORT_EXPIRED')
            return export_model(snapshots[token], output)

    @app.get('/preview/report/<int:doc_id>.<output>')
    def direct_report(doc_id, output):
        with mutex:
            return export_model(model_for('ListaAlimente2015', doc_id), output)

    @app.get('/health')
    def health():
        return jsonify(app='vercon-local-preview', database=False, year=YEAR)

    @app.get('/.well-known/appspecific/com.chrome.devtools.json')
    def devtools_workspace():
        return jsonify(workspace={'root': str(PYTHON_DIR), 'uuid': '567f0466-cdbb-4c68-9185-9f656d02bd63'})

    return app


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--port', type=int, default=5053)
    args = parser.parse_args()
    if not 1024 <= args.port <= 65535:
        parser.error('Portul trebuie să fie între 1024 și 65535.')
    build_app().run(host='127.0.0.1', port=args.port, debug=False, use_reloader=False)
