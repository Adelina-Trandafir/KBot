"""Local VERCON with the existing 000_DEMO MariaDB reached through the owner's SSH key.

Only this launcher imports this module. Credentials stay in process memory; no
production files are uploaded, no schema commands are run, no SMTP is involved.
"""
import argparse
import atexit
from contextlib import contextmanager
from functools import wraps
import json
import logging
import os
from pathlib import Path
import secrets
import shlex
import socket
import subprocess
import sys
import time

ROOT = Path(__file__).resolve().parents[2]
TARGET = '000_DEMO'
LOCAL = ('127.0.0.1', '::1')


def settings(path=None):
    candidates = [Path(path)] if path else [
        ROOT / '_Admin/admin_settings.json',
        ROOT / 'KBotAdmin/bin/Debug/net8.0-windows/admin_settings.json',
    ]
    saved = next((p for p in candidates if p.is_file()), None)
    values = json.loads(saved.read_text(encoding='utf-8-sig')) if saved else {}
    defaults = {'Host': '89.33.25.34', 'Port': 27015, 'User': 'root',
                'RemoteRoot': '/root/AVACONT', 'RemotePython': '/root/AVACONT/.venv/bin/python',
                'PrivateKeyPath': ''}
    return {key: values.get(key) or value for key, value in defaults.items()}


def ssh_args(options):
    args = ['ssh', '-T', '-p', str(options['Port']), '-o', 'BatchMode=yes',
            '-o', 'ConnectTimeout=12', '-o', 'StrictHostKeyChecking=yes',
            '-o', 'ServerAliveInterval=30', '-o', 'ServerAliveCountMax=3']
    if options['PrivateKeyPath']:
        args += ['-i', str(Path(options['PrivateKeyPath']).expanduser())]
    return args


def remote_database_config(options):
    # The JSON is captured, never logged, saved, or included in an exception.
    program = ('import json,config; '
               'print(json.dumps({k:v for k,v in config.DB_CONFIG_NEW.items() '
               'if k in ("host","port","user","password","charset","auth_plugin")}))')
    command = ('cd ' + shlex.quote(options['RemoteRoot']) + ' && ' +
               shlex.quote(options['RemotePython']) + ' -c ' + shlex.quote(program))
    result = subprocess.run(ssh_args(options) + [options['User'] + '@' + options['Host'], command],
                            capture_output=True, encoding='utf-8', timeout=25)
    if result.returncode:
        raise RuntimeError('Conexiunea SSH a eșuat. Folosiți cheia și hostul cunoscut din KBotAdmin / ~/.ssh.')
    try:
        cfg = json.loads(result.stdout)
        if not all(cfg.get(key) for key in ('host', 'user', 'password')):
            raise ValueError()
        return cfg
    except (ValueError, TypeError):
        raise RuntimeError('Configurația MariaDB de pe server nu a putut fi citită.') from None


def start_tunnel(options, cfg):
    with socket.socket() as probe:
        probe.bind(('127.0.0.1', 0))
        port = probe.getsockname()[1]
    forward = '127.0.0.1:%d:%s:%d' % (port, cfg['host'], int(cfg.get('port', 3306)))
    process = subprocess.Popen(ssh_args(options) + ['-N', '-o', 'ExitOnForwardFailure=yes',
                              '-L', forward, options['User'] + '@' + options['Host']],
                              stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL,
                              creationflags=getattr(subprocess, 'CREATE_NO_WINDOW', 0))
    def stop():
        if process.poll() is None:
            process.terminate()
            try:
                process.wait(timeout=5)
            except subprocess.TimeoutExpired:
                process.kill()
    atexit.register(stop)
    for _ in range(50):
        if process.poll() is not None:
            raise RuntimeError('Tunelul SSH nu a putut fi pornit.')
        with socket.socket() as probe:
            probe.settimeout(.2)
            if probe.connect_ex(('127.0.0.1', port)) == 0:
                return port
        time.sleep(.1)
    stop()
    raise RuntimeError('Tunelul SSH nu a devenit disponibil.')


def create_app(cfg, tunnel_port, http_port, actor, report_directory):
    sys.path.insert(0, str(ROOT / 'PYTHON'))
    # Applied before auth/VERCON imports; nothing requires local Redis.
    import config
    config.SESSION_BACKEND = 'memory'
    os.environ['VERCON_REPORT_CONFIG_DIR'] = str(report_directory)
    from flask import Flask, g, jsonify, redirect, request
    import mysql.connector
    from routes.vercon import create_blueprint
    from routes.vercon.lock import LockService, SqlLockStore
    from routes.vercon.operations import OPERATIONS
    from routes.vercon.provider import SqlProvider
    from routes.vercon import report_store
    # preview.py may have imported VERCON already; make this launcher's directory explicit.
    report_store.ROOT = Path(report_directory)

    cfg = {**cfg, 'host': '127.0.0.1', 'port': tunnel_port, 'database': TARGET,
           'autocommit': False, 'connection_timeout': 12}

    def factory(unit):
        if unit != TARGET:
            raise ValueError('Launcherul permite exclusiv baza 000_DEMO.')
        return mysql.connector.connect(**cfg)

    @contextmanager
    def open_db(unit):
        conn = factory(unit)
        try:
            yield conn
        finally:
            conn.close()

    # SELECT only: catch a wrong database / missing installation before opening the page.
    with open_db(TARGET) as conn:
        cur = conn.cursor()
        cur.execute('SELECT SubunitId FROM VC_Subunitati LIMIT 1')
        cur.fetchall()
        cur.close()

    app = Flask('vercon_local_ssh', static_folder=str(ROOT / 'PYTHON/static'))
    app.json.ensure_ascii = False
    app.config['SEND_FILE_MAX_AGE_DEFAULT'] = 0
    app.config['MAX_CONTENT_LENGTH'] = 17 * 1024 * 1024
    secret = secrets.token_urlsafe(32)
    hosts = {'localhost:%d' % http_port, '127.0.0.1:%d' % http_port}
    origins = {'http://' + host for host in hosts}

    @app.before_request
    def local_only():
        if request.remote_addr not in LOCAL or request.host not in hosts:
            return jsonify(error='Disponibil numai pe localhost.', reason='LOCAL_ONLY'), 403
        if request.headers.get('Sec-Fetch-Site') == 'cross-site':
            return jsonify(error='Acces din alt site refuzat.', reason='LOCAL_ONLY'), 403
        if request.method not in ('GET', 'HEAD', 'OPTIONS') and request.headers.get('Origin') not in origins:
            return jsonify(error='Origine locală necesară.', reason='LOCAL_ONLY'), 403
        if request.path.startswith('/api/') and not secrets.compare_digest(request.cookies.get('vc_local_access', ''), secret):
            return jsonify(error='Deschideți mai întâi /vercon.', reason='LOCAL_SESSION'), 403

    @app.after_request
    def local_headers(response):
        response.headers['Cache-Control'] = 'no-store'
        if request.path == '/vercon' and response.status_code == 200:
            response.direct_passthrough = False
            response.set_data(response.get_data(as_text=True).replace(
                '<html lang="ro">', '<html lang="ro" data-vc-local-ssh="true">', 1))
            response.set_cookie('vc_local_access', secret, httponly=True, samesite='Strict')
        return response

    @app.get('/__vercon_local_status')
    def launcher_status():
        return jsonify(application='vercon_local_ssh', database=TARGET, ready=True)

    @app.get('/')
    @app.get('/portal')
    def entry():
        return redirect('/vercon')

    @app.get('/api/portal/me')
    def me():
        return jsonify(email=actor, db_name=TARGET, units=[TARGET])

    @app.get('/api/portal/session/check')
    def session_check():
        return jsonify(expires_in=3600)

    @app.post('/api/portal/logout')
    def logout():
        response = jsonify(ok=True)
        response.delete_cookie('vc_local_access')
        return response

    def authenticate(function):
        @wraps(function)
        def wrapped(*args, **kwargs):
            g.portal = {'email': actor, 'db_name': TARGET, 'role': 'local-ssh-admin'}
            return function(*args, **kwargs)
        return wrapped

    locks = LockService(SqlLockStore(lambda: factory(TARGET)))
    app.register_blueprint(create_blueprint(authenticate=authenticate,
        rights=lambda context: set(OPERATIONS), provider=SqlProvider(connect=open_db),
        lock_factory=lambda unit: locks, open_db=open_db))
    return app


def main():
    parser = argparse.ArgumentParser(description='VERCON local, SSH, exclusiv 000_DEMO.')
    parser.add_argument('--port', type=int, default=5053)
    parser.add_argument('--settings')
    parser.add_argument('--actor', default='localhost-ssh@local.test')
    args = parser.parse_args()
    logging.basicConfig(level=logging.INFO, format='%(levelname)s %(message)s')
    with socket.socket() as probe:
        if probe.connect_ex(('127.0.0.1', args.port)) == 0:
            raise RuntimeError('Portul localhost este deja ocupat.')
    options = settings(args.settings)
    cfg = remote_database_config(options)
    port = start_tunnel(options, cfg)
    directory = ROOT / 'VERCON/tools/.local/report-config'
    directory.mkdir(parents=True, exist_ok=True)
    app = create_app(cfg, port, args.port, args.actor, directory)
    print('VERCON LOCAL: http://localhost:%d/vercon — baza 000_DEMO prin SSH' % args.port, flush=True)
    print('Salvările modifică datele din 000_DEMO. Configurările rapoartelor rămân locale.', flush=True)
    app.run(host='127.0.0.1', port=args.port, threaded=True, debug=False, use_reloader=False)


if __name__ == '__main__':
    try:
        main()
    except KeyboardInterrupt:
        pass
    except Exception as error:
        # Never print connector arguments, captured remote output, or passwords.
        print('Pornirea locală a eșuat: ' + str(error), file=sys.stderr)
        sys.exit(1)
