"""Local-only ADE launcher. Never imported or shipped by the production application."""
import argparse
import functools
import sqlite3
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'PYTHON'))
# Preview owns its configuration before auth modules initialize their stores; no local Redis is required.
import config
config.SESSION_BACKEND = 'memory'
config.ADE_PARENT_PUBLIC_URL = ''
from flask import Flask, g, jsonify, request, redirect
from routes.adechit import create_blueprint
from routes.adechit.domain import SCHEMA
from routes.adechit.repository import Repository
from routes.adechit.parent_portal import create_parent_blueprint
from routes.adechit.parent_identity import provision_existing
from utils.logger import setup_logger


def initialize(path, second_subunit=False, closed_month=False):
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
            fields.append('SubunitId INTEGER NOT NULL DEFAULT 1')
            connection.execute(f'CREATE TABLE IF NOT EXISTS AD_{table} (' + ','.join(fields) + ')')
            existing = {row[1] for row in connection.execute(f'PRAGMA table_info(AD_{table})')}
            if 'SubunitId' not in existing:
                connection.execute(f'ALTER TABLE AD_{table} ADD COLUMN SubunitId INTEGER NOT NULL DEFAULT 1')
            for field, info in spec['fields'].items():
                if field in existing:
                    continue
                kind = 'INTEGER' if info['type'] in ('Long', 'Yes/No') else 'REAL' if info['type'] == 'Double' else 'TEXT'
                default = ' DEFAULT 0' if info['default'] in ('No', '0') else ''
                connection.execute(f'ALTER TABLE AD_{table} ADD COLUMN `{field}` {kind}{default}')
        # A preview database made before the subunits existed: its helper tables (no business data) are recreated.
        for helper in ('AD_Operations', 'AD_Settings', 'AD_Imports'):
            columns = {row[1] for row in connection.execute(f'PRAGMA table_info({helper})')}
            if columns and 'SubunitId' not in columns:
                connection.execute(f'DROP TABLE {helper}')
        connection.executescript('''
        CREATE TABLE IF NOT EXISTS AD_Lock (ID INTEGER PRIMARY KEY);
        INSERT OR IGNORE INTO AD_Lock VALUES (1);
        CREATE TABLE IF NOT EXISTS AD_Operations (SubunitId INTEGER NOT NULL DEFAULT 1, RequestKey TEXT, Fingerprint TEXT, Result TEXT, PRIMARY KEY (SubunitId, RequestKey));
        CREATE TABLE IF NOT EXISTS AD_Settings (SubunitId INTEGER NOT NULL DEFAULT 1, SettingKey TEXT, SettingValue TEXT NOT NULL, PRIMARY KEY (SubunitId, SettingKey));
        INSERT OR IGNORE INTO AD_Settings (SubunitId, SettingKey, SettingValue) VALUES (1,'BlockReopenWithMovements','1');
        INSERT OR IGNORE INTO AD_Settings (SubunitId, SettingKey, SettingValue) VALUES (1,'AllowCancelDocuments','true');
        CREATE TABLE IF NOT EXISTS AD_Imports (SubunitId INTEGER NOT NULL DEFAULT 1, SourceHash TEXT PRIMARY KEY, SourceFile TEXT NOT NULL, Manifest TEXT NOT NULL, Result TEXT NOT NULL, ImportedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP);
        CREATE TABLE IF NOT EXISTS AD_Subunits (SubunitId INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT NOT NULL UNIQUE, Active INTEGER NOT NULL DEFAULT 1, Version INTEGER NOT NULL DEFAULT 1);
        CREATE TABLE IF NOT EXISTS AD_ReceiptConfig (SubunitId INTEGER PRIMARY KEY, Serie TEXT, Numar INTEGER, Explicatie TEXT, Version INTEGER DEFAULT 1);
        CREATE TABLE IF NOT EXISTS AD_SubunitAccess (SubunitId INTEGER NOT NULL, Email TEXT NOT NULL, PRIMARY KEY (SubunitId, Email));
        CREATE TABLE IF NOT EXISTS AD_IdMap (SubunitId INTEGER NOT NULL, ImportHash TEXT NOT NULL, TableName TEXT NOT NULL, SourceId INTEGER NOT NULL, TargetId INTEGER NOT NULL, PRIMARY KEY (SubunitId, TableName, SourceId));
        INSERT OR IGNORE INTO AD_Subunits (SubunitId, Name) VALUES (1, 'Evidenta de proba');
        ''')
        if not connection.execute('SELECT 1 FROM AD_Grupe').fetchone():
            connection.executescript('''
            INSERT INTO AD_Grupe (IDG,Grupa,Educator,Tip) VALUES (1,'Grupa de probă','Educator de probă','NORMALA');
            INSERT INTO AD_ValoriTaxe (IDV,TaxaZilnica,Activ,Expl) VALUES (1,25,1,'Taxă de probă');
            INSERT INTO AD_Platitori (IDP,IDG,Nume,SI,Plecat) VALUES (1,1,'Copil fictiv',0,0);
            INSERT INTO AD_Platitori_sub (IDS,IDP,Nume,Activ) VALUES (1,1,'Părinte fictiv',1);
            INSERT INTO AD_LunaD (IDL,Ordine,IDV,Luna,Anul,LunaT,LA,Inchisa,ZileLuna) VALUES (1,1,1,10,2026,'octombrie','102026',0,22);
            INSERT INTO AD_Prezenta (IDZ,IDP,IDG,IDV,IDL,ZilePrezenta,ValoareContract,ValoareTotala) VALUES (1,1,1,1,1,10,250,250);
            ''')
        # Local legacy previews had only a text educator; preserve it without inventing dates.
        for group in connection.execute('SELECT IDG,Educator FROM AD_Grupe').fetchall():
            if group[1] and not connection.execute('SELECT 1 FROM AD_Grupe_Educator WHERE IDG=?', (group[0],)).fetchone():
                connection.execute('INSERT INTO AD_Grupe_Educator (IDG,Educator) VALUES (?,?)', group)
        connection.execute('INSERT OR IGNORE INTO AD_ReceiptConfig (SubunitId,Serie,Numar,Explicatie) VALUES (?,?,?,?)',
                           (1, 'DEMO', 1, 'C/Val. luna [LA] conf. contract'))
        if second_subunit:
            # A second evidence in the same unit, with its own receipt series and the same month, to try switching by hand.
            connection.executescript('''
            INSERT OR IGNORE INTO AD_Subunits (SubunitId, Name) VALUES (2, 'Evidenta B');
            INSERT OR IGNORE INTO AD_Lock VALUES (2);
            INSERT OR IGNORE INTO AD_Settings (SubunitId, SettingKey, SettingValue) VALUES (2,'BlockReopenWithMovements','1');
            INSERT OR IGNORE INTO AD_Settings (SubunitId, SettingKey, SettingValue) VALUES (2,'AllowCancelDocuments','true');
            ''')
            connection.execute('INSERT OR IGNORE INTO AD_ReceiptConfig (SubunitId,Serie,Numar,Explicatie) VALUES (?,?,?,?)',
                               (2, 'DEMOB', 500, 'C/Val. luna [LA] conf. contract B'))
            if not connection.execute('SELECT 1 FROM AD_Grupe WHERE SubunitId=2').fetchone():
                connection.executescript('''
                INSERT INTO AD_Grupe (IDG,SubunitId,Grupa,Tip) VALUES (101,2,'Grupa B','NORMALA');
                INSERT INTO AD_ValoriTaxe (IDV,SubunitId,TaxaZilnica,Activ,Expl) VALUES (101,2,40,1,'Taxa B');
                INSERT INTO AD_Platitori (IDP,SubunitId,IDG,Nume,SI,Plecat) VALUES (101,2,101,'Copil B',0,0);
                INSERT INTO AD_Platitori_sub (IDS,SubunitId,IDP,Nume,Activ) VALUES (101,2,101,'Parinte B',1);
                INSERT INTO AD_LunaD (IDL,SubunitId,Ordine,IDV,Luna,Anul,LunaT,LA,Inchisa,ZileLuna) VALUES (101,2,1,101,10,2026,'octombrie','102026',0,22);
                INSERT INTO AD_Prezenta (IDZ,SubunitId,IDP,IDG,IDV,IDL,ZilePrezenta,ValoareContract,ValoareTotala) VALUES (101,2,101,101,101,101,5,200,200);
                ''')
        # Every evidence gets one CLOSED month (September 2026, before the open October) to try close/reopen and the lock icons.
        for sub, month_id, attendance_id, person_id, group_id, tax_id in (((1, 2, 2, 1, 1, 1), (2, 102, 102, 101, 101, 101)) if closed_month else ()):
            if connection.execute('SELECT 1 FROM AD_LunaD WHERE SubunitId=? AND Inchisa=1', (sub,)).fetchone():
                continue
            if not connection.execute('SELECT 1 FROM AD_Platitori WHERE SubunitId=? AND IDP=?', (sub, person_id)).fetchone():
                continue
            connection.execute("INSERT OR IGNORE INTO AD_LunaD (IDL,SubunitId,Ordine,IDV,Luna,Anul,LunaT,LA,Inchisa,ZileLuna) VALUES (?,?,0,?,9,2026,'septembrie','092026',1,22)", (month_id, sub, tax_id))
            connection.execute('INSERT OR IGNORE INTO AD_Prezenta (IDZ,SubunitId,IDP,IDG,IDV,IDL,ZilePrezenta,ValoareContract,ValoareTotala) VALUES (?,?,?,?,?,?,20,500,500)', (attendance_id, sub, person_id, group_id, tax_id, month_id))
            connection.execute('INSERT OR IGNORE INTO AD_SS_Buget (SubunitId,IDG,IDP,IDL,IDZ,Luna,Anul,Nume,ZilePrezenta,SID,SIC,ValoareContract,ValoareTotala,Plata,Plati,Retur,SFD,SFC,Plecat) '
                               'SELECT ?,?,?,?,?,9,2026,Nume,20,0,0,500,500,0,0,0,500,0,0 FROM AD_Platitori WHERE SubunitId=? AND IDP=?', (sub, group_id, person_id, month_id, attendance_id, sub, person_id))
        connection.commit()
    finally:
        connection.close()


def create_app(directory, second_subunit=False, closed_month=False):
    directory = Path(directory)
    directory.mkdir(parents=True, exist_ok=True)
    for unit in ('preview', 'preview2'):
        initialize(directory / (unit + '.sqlite'), second_subunit, closed_month)
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

    # Local-only delivery replaces SMTP; the real authentication routes and checks are identical.
    inbox = []
    def local_mail(address, subject, body):
        inbox.append({'to': address, 'subject': subject, 'body': body})
        del inbox[:-100]

    for unit in ('preview', 'preview2'):
        repo = factory(unit)
        try:
            repo.connection.executescript('''
            CREATE TABLE IF NOT EXISTS AD_PortalParents (CNP TEXT PRIMARY KEY,Email TEXT UNIQUE,CodAccesPortal TEXT);
            CREATE TABLE IF NOT EXISTS AD_PortalChallenges (ChallengeId TEXT PRIMARY KEY,CNP TEXT,Email TEXT,AccessHash TEXT,CodeHash TEXT,ExpiresAt INTEGER,Attempts INTEGER DEFAULT 0,CreatedAt INTEGER);
            INSERT OR IGNORE INTO AD_Lock VALUES (0);
            ''')
            repo.subunit_id = 1
            if not repo.execute('SELECT 1 FROM AD_PortalParents').fetchone():
                # Valid synthetic CNPs, with a computed checksum. No real family data.
                def demo_cnp(prefix):
                    check = sum(int(a)*int(b) for a,b in zip(prefix,'279146358279')) % 11
                    return prefix + str(1 if check == 10 else check)
                first = demo_cnp('180010140001'); second = demo_cnp('280010140002'); third = demo_cnp('180010140003')
                repo.execute('UPDATE AD_Platitori_sub SET CNP_Platitor=%s,EMail=%s WHERE IDS=1', (first,'parinte.demo@example.test'))
                for identity,name,departed in ((201,'Copil fictiv II',0),(202,'Copil fictiv plecat',1),(203,'Copil părinte unic',0),(204,'Copil fără email',0)):
                    repo.execute('INSERT OR IGNORE INTO AD_Platitori (IDP,SubunitId,IDG,Nume,SI,Plecat,DataIntrare) VALUES (%s,1,1,%s,0,%s,%s)',(identity,name,departed,'2025-01-01'))
                    repo.execute('INSERT OR IGNORE INTO AD_Platitori_sub (IDS,SubunitId,IDP,Nume,CNP_Platitor,EMail,Activ) VALUES (%s,1,%s,%s,%s,%s,1)',
                        (identity,identity,'Părinte demo' if identity<203 else 'Părinte unic',first if identity<203 else second if identity==203 else third,
                         'parinte.demo@example.test' if identity<203 else 'parinte.unic@example.test' if identity==203 else None))
                repo.execute('UPDATE AD_Platitori SET DataIntrare=%s WHERE IDP=1',('2025-01-01',))
                # Use chronological month ids, matching the existing Access calculation contract.
                repo.execute('UPDATE AD_LunaD SET IDL=3,Ordine=3 WHERE IDL=1')
                for table in ('Prezenta','Plati','Retur','Chitante','AlteDoc','SS_Buget'):
                    repo.execute(f'UPDATE AD_{table} SET IDL=3 WHERE IDL=1')
                repo.execute("INSERT OR IGNORE INTO AD_LunaD (IDL,Ordine,IDV,Luna,Anul,LunaT,LA,Inchisa,ZileLuna) VALUES (1,1,1,1,2025,'ianuarie','012025',1,22)")
                for child_id,month_id,attendance_id,days in ((1,1,3,8),(201,3,201,6),(201,2,202,12),(201,1,203,5),(203,3,204,4)):
                    repo.execute('INSERT OR IGNORE INTO AD_Prezenta (IDZ,IDP,IDG,IDV,IDL,ZilePrezenta,ValoareContract,ValoareTotala) VALUES (%s,%s,1,1,%s,%s,%s,%s)',
                        (attendance_id,child_id,month_id,days,days*25,days*25))
                for month_id,attendance_id,day,amount in ((1,3,'2025-01-16',300),(2,2,'2026-09-16',400)):
                    repo.execute('INSERT INTO AD_Plati (IDP,IDZ,IDS,IDL,Data,Plata,TIP,Anulata) VALUES (1,%s,1,%s,%s,%s,2,0)', (attendance_id,month_id,day,amount))
                    payment_id=repo.cursor.lastrowid
                    repo.execute('INSERT INTO AD_Chitante (IDPL,Data,Serie,Numar,Explicatie,Anulata,IDL) VALUES (%s,%s,%s,%s,%s,0,%s)',(payment_id,day,'DEMO',month_id,'Plată de probă',month_id))
                repo.execute("INSERT INTO AD_Retur (IDP,IDZ,IDS,IDL,Data,Suma,Anulat,NrDoc,Explicatie) VALUES (1,2,1,2,'2026-09-20',50,0,'R-DEMO','Restituire de probă')")
                repo.execute('UPDATE AD_ReceiptConfig SET Numar=100 WHERE SubunitId=1')
                # Calculate closed snapshots once, in chronological id order as the shared engine requires.
                # Existing month ids remain untouched for the administrative preview.
                from routes.adechit.calculations import calculate
                for month_id in (1,2):
                    repo.execute('DELETE FROM AD_SS_Buget WHERE SubunitId=1 AND IDL=%s',(month_id,))
                    calculated,_=calculate(repo.data(),month_id)
                    for snapshot in calculated:
                        allowed={k:v for k,v in snapshot.items() if k in SCHEMA['SS_Buget']['fields'] and k!='ID'}
                        repo.insert('SS_Buget',allowed)
                provision_existing(repo)
            repo.connection.commit()
        finally:
            repo.cursor.close(); repo.connection.close()

    @app.get('/adechit/preview/inbox')
    def preview_inbox():
        if request.remote_addr not in ('127.0.0.1','::1'):
            return jsonify(error='Disponibil numai local.'),403
        from html import escape
        messages=''.join('<article><h2>'+escape(m['subject'])+'</h2><p>'+escape(m['to'])+'</p><pre>'+escape(m['body'])+'</pre></article>' for m in reversed(inbox))
        response=app.response_class('<!doctype html><html lang="ro"><meta charset="utf-8"><title>Emailuri de probă</title><h1>Emailuri de probă localhost</h1><p>Reîncarcă pagina după trimiterea unui cod.</p>'+messages+'</html>',mimetype='text/html')
        response.headers['Cache-Control']='no-store'
        return response

    @app.get('/adechit/preview/parents')
    def preview_parents():
        if request.remote_addr not in ('127.0.0.1','::1'):
            return jsonify(error='Disponibil numai local.'),403
        from html import escape
        repo=factory('preview')
        try:
            rows=repo.query('SELECT CNP,Email,CodAccesPortal FROM AD_PortalParents')
        finally:
            repo.cursor.close(); repo.connection.close()
        cards=''.join('<p>Email: '+escape(r['Email'] or '')+'<br>CNP: '+escape(r['CNP'])+'<br>Cod acces: '+escape(r['CodAccesPortal'] or '')+'</p>' for r in rows)
        response=app.response_class('<!doctype html><html lang="ro"><meta charset="utf-8"><title>Părinți demo</title><h1>Părinți demo</h1><a href="/adechit/parinti/preview">Conectare părinte</a> · <a href="/adechit/preview/inbox">Emailuri de probă</a>'+cards+'</html>',mimetype='text/html')
        response.headers['Cache-Control']='no-store'
        return response

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
    app.register_blueprint(create_parent_blueprint(factory,local_mail,('preview','preview2')))
    app.config['ADE_PARENT_DELIVER']=local_mail
    app.config['ADE_TEST_RIGHTS'] = granted
    app.config['ADE_TEST_FACTORY'] = factory
    return app


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--directory', type=Path, default=ROOT / 'artifacts/ade-preview')
    parser.add_argument('--port', type=int, default=5050)
    parser.add_argument('--single-subunit', action='store_true', help='do NOT seed the second subunit (Evidenta B); by default every preview unit has two')
    args = parser.parse_args()
    setup_logger()
    create_app(args.directory, not args.single_subunit, True).run(host='127.0.0.1', port=args.port, debug=False, use_reloader=False)
