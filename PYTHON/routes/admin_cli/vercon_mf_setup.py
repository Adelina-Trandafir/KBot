"""SLICE-VC11-02: create an operational MF import destination in existing VC tables. No schema changes."""
import json
import re

from .admin_common import AdminError, _actor
from .vercon_reports import unit
from routes.vercon.connection import connection
from routes.vercon import nomenclatoare as nom, numeric, protocol
from routes.vercon.repository import ScopedRepository


def create(args):
    db = unit(args)
    name = str(args.name or '').strip()
    if not name or len(name) > 100 or re.search(r'[\x00-\x1f\x7f]', name):
        raise AdminError('Introduceți un nume de gestiune de maximum 100 de caractere.')
    if not 1990 <= args.year <= 2100:
        raise AdminError('Anul de început al exercițiului trebuie să fie între 1990 și 2100.')
    if not re.fullmatch(r'[a-f0-9]{32}', args.request_key or ''):
        raise AdminError('Lipsește identificarea cererii de creare. Reluați operația.')
    key = 'mfsetup-' + args.request_key
    fingerprint = protocol.fingerprint('mf.setup', args.subunit, name, args.year)
    with connection(db) as conn:
        conn.start_transaction(isolation_level='READ COMMITTED')
        cursor = conn.cursor(dictionary=True)
        try:
            # Serialise setup requests in this subunit, including retries after a lost response.
            cursor.execute('SELECT Name FROM VC_Subunitati WHERE SubunitId=%s AND Active=1 FOR UPDATE', (args.subunit,))
            if not cursor.fetchone():
                raise AdminError('Subunitatea aleasă nu există sau nu mai este activă.')
            cursor.execute('SELECT Fingerprint, Result FROM VC_Operations WHERE SubunitId=%s AND RequestKey=%s FOR UPDATE',
                           (args.subunit, key))
            seen = cursor.fetchone()
            if seen:
                if seen['Fingerprint'] != fingerprint:
                    raise AdminError('Cererea a fost folosită pentru alte opțiuni de creare.')
                result = json.loads(seen['Result'])
                conn.commit()
                return result
            cursor.execute('SELECT GestiuneId FROM VC_Gestiuni WHERE SubunitId=%s AND Name=%s LIMIT 1', (args.subunit, name))
            if cursor.fetchone():
                raise AdminError('Există deja o gestiune cu numele «%s» în această subunitate. Citiți subunitățile și selectați-o sau alegeți alt nume.' % name)
            cursor.execute("INSERT INTO VC_Gestiuni (SubunitId, Name, Type, State, Zec, Scara, Populated, Active) "
                           "VALUES (%s,%s,'MF','ACTIVA',%s,%s,0,1)",
                           (args.subunit, name, numeric.DEFAULT_ZEC, numeric.MAX_SCALE))
            gestiune = cursor.lastrowid
            cursor.execute('INSERT INTO VC_Exercitii (SubunitId, GestiuneId, An, Closed) VALUES (%s,%s,%s,0)',
                           (args.subunit, gestiune, args.year))
            exercise = cursor.lastrowid
            ctx = {'unit': db, 'subunit': args.subunit, 'gestiune': gestiune, 'exercitiu': exercise, 'email': _actor()}
            repo = ScopedRepository(cursor, args.subunit, gestiune, exercise)
            warehouse, _ = nom.create(repo, cursor, ctx, 'categorii', 'MF', numeric.MAX_SCALE,
                                      {'Masina': 'Mijloace Fixe in Magazie', 'TIP': 'MFM'})
            use, _ = nom.create(repo, cursor, ctx, 'categorii', 'MF', numeric.MAX_SCALE,
                                {'Masina': 'Mijloace Fixe in Folosinta', 'TIP': 'MFF'})
            for percent in (0, 21):
                nom.create(repo, cursor, ctx, 'tva', 'MF', numeric.MAX_SCALE,
                           {'TVA': percent, 'Explicatie': 'TVA %s%%' % percent})
            for kind in ('NIR', 'STORNO', 'BC', 'BR', 'BP', 'BT', 'BS'):
                protocol.raise_counter(cursor, ctx, kind, 0)
            result = {'unit': db, 'subunit': args.subunit, 'gestiune': gestiune, 'name': name,
                      'account': warehouse['id'], 'use_account': use['id'], 'exercise': exercise, 'year': args.year}
            protocol.audit(cursor, ctx, 'mf.setup', 'VC_Gestiuni', gestiune, None,
                           {**result, 'vat_percent': [0, 21], 'type': 'MF', 'state': 'ACTIVA'}, key, key)
            protocol.remember(cursor, ctx, key, fingerprint, result)
            conn.commit()
            return result
        except nom.NomError as error:
            conn.rollback()
            raise AdminError(str(error)) from None
        except Exception:
            conn.rollback()
            raise


def register(sub):
    parser = sub.add_parser('vercon-mf-create')
    parser.add_argument('--db', required=True)
    parser.add_argument('--subunit', type=int, required=True)
    parser.add_argument('--name', default='Mijloace Fixe')
    parser.add_argument('--year', type=int, required=True)
    parser.add_argument('--request-key', required=True)
    parser.set_defaults(fn=create)
