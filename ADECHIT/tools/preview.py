"""Local-only ADE launcher. Never imported or shipped by the production application."""
import argparse
import functools
import sqlite3
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'PYTHON'))
from flask import Flask, g, jsonify, request, redirect
from routes.adechit import create_blueprint
from routes.adechit.domain import SCHEMA
from routes.adechit.repository import Repository
from utils.logger import setup_logger


def initialize(path):
    connection = sqlite3.connect(path)
    try:
        for table, spec in SCHEMA.items():
            fields = []
            for field, info in spec['fields'].items():
                if field == spec['key']:
                    fields.append(f'`{field}` INTEGER PRIMARY KEY AUTOINCREMENT')
                else:
                    kind = 'INTEGER' if info['type'] in ('Long', 'Yes/No') else 'REAL' if info['type'] == 'Double' else 'TEXT'
                    default = ' DEFAULT 0' if info['default'] in ('No', '0') else ''
                    fields.append(f'`{field}` {kind}{default}')
            fields.append('Version INTEGER NOT NULL DEFAULT 1')
            connection.execute(f'CREATE TABLE IF NOT EXISTS AD_{table} (' + ','.join(fields) + ')')
            existing = {row[1] for row in connection.execute(f'PRAGMA table_info(AD_{table})')}
            for field, info in spec['fields'].items():
                if field in existing:
                    continue
                kind = 'INTEGER' if info['type'] in ('Long', 'Yes/No') else 'REAL' if info['type'] == 'Double' else 'TEXT'
                connection.execute(f'ALTER TABLE AD_{table} ADD COLUMN `{field}` {kind}')
        connection.executescript('''
        CREATE TABLE IF NOT EXISTS AD_Lock (ID INTEGER PRIMARY KEY);
        INSERT OR IGNORE INTO AD_Lock VALUES (1);
        CREATE TABLE IF NOT EXISTS AD_Operations (RequestKey TEXT PRIMARY KEY, Fingerprint TEXT, Result TEXT);
        CREATE TABLE IF NOT EXISTS AD_Settings (SettingKey TEXT PRIMARY KEY, SettingValue TEXT NOT NULL);
        INSERT OR IGNORE INTO AD_Settings VALUES ('BlockReopenWithMovements','1');
        CREATE TABLE IF NOT EXISTS AD_Imports (SourceHash TEXT PRIMARY KEY, SourceFile TEXT NOT NULL, Manifest TEXT NOT NULL, Result TEXT NOT NULL, ImportedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP);
        CREATE TABLE IF NOT EXISTS Unitati_Chitante (DC TEXT PRIMARY KEY, Serie TEXT, Numar INTEGER, Explicatie TEXT, Version INTEGER DEFAULT 1);
        ''')
        if not connection.execute('SELECT 1 FROM AD_Grupe').fetchone():
            connection.executescript('''
            INSERT INTO AD_Grupe (IDG,Grupa,Educator,Tip) VALUES (1,'Grupa de probă','Educator de probă','NORMALA');
            INSERT INTO AD_ValoriTaxe (IDV,TaxaZilnica,Activ,Expl) VALUES (1,25,1,'Taxă de probă');
            INSERT INTO AD_Platitori (IDP,IDG,Nume,SI,Plecat) VALUES (1,1,'Copil fictiv',0,0);
            INSERT INTO AD_Platitori_sub (IDS,IDP,Nume,Activ) VALUES (1,1,'Părinte fictiv',1);
            INSERT INTO AD_LunaD (IDL,IDV,Luna,Anul,LunaT,LA,Inchisa,ZileLuna) VALUES (1,1,10,2026,'octombrie','102026',0,22);
            INSERT INTO AD_Prezenta (IDZ,IDP,IDG,IDV,IDL,ZilePrezenta,ValoareContract,ValoareTotala) VALUES (1,1,1,1,1,10,250,250);
            ''')
        # Local legacy previews had only a text educator; preserve it without inventing dates.
        for group in connection.execute('SELECT IDG,Educator FROM AD_Grupe').fetchall():
            if group[1] and not connection.execute('SELECT 1 FROM AD_Grupe_Educator WHERE IDG=?', (group[0],)).fetchone():
                connection.execute('INSERT INTO AD_Grupe_Educator (IDG,Educator) VALUES (?,?)', group)
        connection.execute('INSERT OR IGNORE INTO Unitati_Chitante (DC,Serie,Numar,Explicatie) VALUES (?,?,?,?)',
                           (path.stem, 'DEMO', 1, 'C/Val. luna [LA] conf. contract'))
        connection.commit()
    finally:
        connection.close()


def create_app(directory):
    directory = Path(directory)
    directory.mkdir(parents=True, exist_ok=True)
    for unit in ('preview', 'preview2'):
        initialize(directory / (unit + '.sqlite'))
    app = Flask('ade_preview', static_folder=str(ROOT / 'PYTHON/static'))
    app.json.ensure_ascii = False

    @app.get('/')
    @app.get('/portal')
    def entry():
        # The local preview has no portal login; recover old sign-in redirects here.
        return redirect('/adechit')

    def factory(unit):
        if unit not in ('preview', 'preview2'):
            raise ValueError('Unknown preview unit')
        connection = sqlite3.connect(directory / (unit + '.sqlite'), timeout=10)
        connection.row_factory = sqlite3.Row
        return Repository(connection, sqlite=True)

    def authenticate(function):
        @functools.wraps(function)
        def wrapped():
            if request.remote_addr not in ('127.0.0.1', '::1'):
                return jsonify(error='Preview disponibil numai local.'), 403
            g.portal = {'email': 'preview@local', 'db_name': request.headers.get('X-Ade-Unit', 'preview'), 'role': 'preview'}
            return function()
        return wrapped

    granted = {'read', 'catalog', 'attendance', 'collect', 'cancel', 'close', 'reopen', 'transfer', 'import'}
    app.register_blueprint(create_blueprint(authenticate, factory, lambda context: granted))
    app.config['ADE_TEST_RIGHTS'] = granted
    app.config['ADE_TEST_FACTORY'] = factory
    return app


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--directory', type=Path, default=ROOT / 'artifacts/ade-preview')
    parser.add_argument('--port', type=int, default=5050)
    args = parser.parse_args()
    setup_logger()
    create_app(args.directory).run(host='127.0.0.1', port=args.port, debug=False, use_reloader=False)
