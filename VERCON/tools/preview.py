"""Local-only VERCON launcher (SLICE-VC01-04). Never imported or shipped by the production application.

Two fictional DCs, two subunits in the first one, gestiuni of every type (MATERIALE / ALIMENTE / OI / MF)
and open/closed years, all from preview_fixture.json. No MariaDB, no network: the portal session is
replaced by a local stand-in, the VERCON blueprint and its guard are the real ones.

    PYTHON\\.venv\\Scripts\\python.exe VERCON\\tools\\preview.py --port 5053
    reset: nothing to reset -- the fixture is read-only and the launcher keeps no state on disk.
    switch DC: /vercon/preview/unit/preview  or  /vercon/preview/unit/preview2
"""
import argparse
import functools
import json
import copy
import socket
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'PYTHON'))
# The preview owns its configuration before the auth modules initialise their stores; no Redis needed.
import config
config.SESSION_BACKEND = 'memory'
from flask import Flask, g, jsonify, redirect, request
from routes.vercon import create_blueprint
from routes.vercon.lock import LockLost, LockService, MemoryLockStore
from routes.vercon.operations import OPERATIONS
from utils.logger import setup_logger

FIXTURE = Path(__file__).with_name('preview_fixture.json')
LOCAL = ('127.0.0.1', '::1')
GRANTED = set(OPERATIONS)   # the preview user holds every VR operation (real roles: sql/VC04_01_roluri_vr.sql)


class FixtureProvider:
    """In-memory context provider over preview_fixture.json (changes last until the launcher stops)."""

    def __init__(self, units):
        self.units = copy.deepcopy(units)

    def tree(self, unit, email):
        return copy.deepcopy(self.units[unit]['subunits'])

    def set_type(self, unit, subunit_id, gestiune_id, new_type):
        for sub in self.units[unit]['subunits']:
            if sub['id'] == subunit_id:
                for gest in sub['gestiuni']:
                    if gest['id'] == gestiune_id:
                        gest['type'] = new_type


def create_app():
    fixture = json.loads(FIXTURE.read_text(encoding='utf-8'))
    units = {unit['db_name']: unit for unit in fixture['units']}
    app = Flask('vercon_preview', static_folder=str(ROOT / 'PYTHON/static'))
    app.json.ensure_ascii = False

    def current_unit():
        name = request.cookies.get('vc_unit', 'preview')
        return name if name in units else 'preview'

    def current_user():
        name = request.cookies.get('vc_user', 'preview')
        return (name if name in ('preview', 'altul') else 'preview') + '@local.test'

    @app.get('/vercon/preview/user/<name>')
    def switch_user(name):
        # a second preview user ("altul") to try the exclusive access from two browsers / profiles
        if request.remote_addr not in LOCAL or name not in ('preview', 'altul'):
            return jsonify(error='Utilizator necunoscut.'), 404
        response = redirect('/vercon')
        response.set_cookie('vc_user', name, samesite='Strict')
        return response

    @app.get('/')
    @app.get('/portal')
    def entry():
        return redirect('/vercon')

    @app.get('/vercon/preview/unit/<name>')
    def switch_unit(name):
        if request.remote_addr not in LOCAL or name not in units:
            return jsonify(error='Unitate necunoscuta.'), 404
        response = redirect('/vercon')
        response.set_cookie('vc_unit', name, samesite='Strict')
        return response

    @app.get('/vercon/preview/fixture')
    def show_fixture():
        if request.remote_addr not in LOCAL:
            return jsonify(error='Disponibil numai local.'), 403
        return jsonify(units[current_unit()])

    # Stand-ins for the two portal calls the page makes; the real portal is never involved.
    @app.get('/api/portal/me')
    def me():
        return jsonify(email=current_user(), db_name=current_unit(), units=[u['db_name'] for u in fixture['units']])

    @app.get('/api/portal/session/check')
    def session_check():
        return jsonify(expires_in=3600)

    def authenticate(function):
        @functools.wraps(function)
        def wrapped(*args, **kwargs):
            if request.remote_addr not in LOCAL:
                return jsonify(error='Preview disponibil numai local.'), 403
            g.portal = {'email': current_user(), 'db_name': current_unit(), 'role': 'preview'}
            return function(*args, **kwargs)
        return wrapped

    provider = FixtureProvider(units)
    services = {}   # one in-memory lock service per preview DC; a lock lives until the launcher stops or expires

    def lock_factory(db_name):
        return services.setdefault(db_name, LockService(MemoryLockStore(), ttl=20))   # short ttl: expiry is easy to try

    app.register_blueprint(create_blueprint(authenticate, lambda context: GRANTED, preview=True, provider=provider,
                                            lock_factory=lock_factory))

    # ---- stand-in for the save protocol of VC02-04 (the real endpoints come with VC06) -------------------------
    # One revision per form; a RequestKey is applied once and its answer is replayed. The header
    # X-Preview-Simulate: lose-response applies the save but answers 504, to try the "unknown" state.
    revisions, answers = {}, {}

    @app.post('/api/vercon/preview/save')
    @authenticate
    def preview_save():
        body = request.get_json(silent=True) or {}
        key = str(body.get('request_key') or '')
        try:   # the same door as production writes: only the current holder of the lock may save
            lock_factory(current_unit()).validate(body.get('subunit'), body.get('gestiune'), request.headers.get('X-Vercon-Lock'), current_user())
        except LockLost as error:
            return jsonify(error=str(error), reason='LOCK_LOST'), 409
        if not key:
            return jsonify(error='Lipseste cheia cererii.', reason='REQUEST'), 400
        if key in answers:
            return jsonify({**answers[key], 'replayed': True}), 200
        form = str(body.get('form') or 'nir')
        slot = (current_unit(), body.get('gestiune'), form)
        current = revisions.get(slot, 1)
        if body.get('base_revision') != current:
            return jsonify(error='Documentul a fost modificat de altcineva.', reason='CONFLICT', revision=current), 409
        revisions[slot] = current + 1
        answers[key] = {'saved': True, 'revision': current + 1}
        if request.headers.get('X-Preview-Simulate') == 'lose-response':
            return jsonify(error='Raspuns pierdut (simulare).', reason='SIMULATED'), 504
        return jsonify(answers[key]), 200

    @app.get('/api/vercon/preview/save/<key>')
    @authenticate
    def preview_save_status(key):
        return jsonify(known=key in answers, result=answers.get(key))
    return app


def port_is_busy(port):
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as probe:
        return probe.connect_ex(('127.0.0.1', port)) == 0


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--port', type=int, default=5053)
    args = parser.parse_args()
    if port_is_busy(args.port):
        sys.exit('Portul %d este ocupat. Opriti procesul care il foloseste sau alegeti alt port cu --port.' % args.port)
    setup_logger()
    create_app().run(host='127.0.0.1', port=args.port, debug=False, use_reloader=False)
