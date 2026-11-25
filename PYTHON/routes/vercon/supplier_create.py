"""SLICE-VC20-01: a NEW supplier (a Parteneri row) added from the «Furnizori (Parteneri)» form of VERCON online (the «+» next to the title).

    GET  /api/vercon/suppliers/next-code  the next free Cod partener (shown in the form before anything is saved)
    POST /api/vercon/suppliers/check      {cod_fiscal}: does the fiscal code exist already, and what does ANAF say about it
    POST /api/vercon/suppliers/create     {cod_fiscal, if_missing?}: adds the supplier with the name and address ANAF gives

The client sends ONLY the fiscal code. The name and the address always come from a fresh ANAF answer on the server (same rule as
partners.py) and the Cod partener is the next free one, computed again under the lock. The fiscal code is checked against EVERY
Parteneri row of the database (every unit of the client, hidden ones included) before the insert, so two sessions cannot add the same
supplier. A code that exists only as a HIDDEN row is not added twice: the row is made visible again (audited). Only the Parteneri
row is written: the commitment codes (Parteneri_Angajamente) and the balances belong to the accountant and are never touched here.
The same `create` (with `if_missing`) is used by the save of a NIR started from an e-invoice whose supplier is new.
"""
from contextlib import contextmanager

from flask import g, request
from utils import vercon_suppliers as suppliers

from .capabilities import FormUnavailable
from .documents import _one
from .nomenclatoare import NomError, audit
from .partners import _SELECT, _VISIBLE, _display_rows, _lookup, _present, _refuse_own_code

OPERATION = 'parteneri_adauga'
PROVENANCE = 'VERCON_FURNIZOR'
_ALLOWED = {'subunit', 'gestiune', 'year', 'cod_fiscal', 'if_missing'}
_CODE_WIDTH = 3   # the existing codes are 022, 074, 1049 ...


def clean_code(value):
    """The normalized fiscal code of a Romanian supplier (2-10 digits), or an INVALID error."""
    from routes.inregistrare import anaf
    code = suppliers.code(value)
    if not code.isascii() or not code.isdigit() or not anaf.is_valid_cf(code):
        raise NomError('INVALID', 'Introduceți un cod fiscal românesc de 2–10 cifre.', 422)
    return code


def existing(cursor, code, locked=False):
    """EVERY Parteneri row with this fiscal code, whatever the unit and whether it is hidden."""
    cursor.execute('SELECT IdPartener,CodPartener,DenumirePartener,Adresa,COALESCE(Ascuns,0) AS Ascuns FROM Parteneri WHERE '
                   + suppliers.sql('CodFiscal') + '=%s ORDER BY (COALESCE(Ascuns,0)<>0),IdPartener'
                   + (' FOR UPDATE' if locked else ''), (code,))
    return cursor.fetchall()


def next_code(cursor, unit_id):
    """The next free Cod partener: one more than the biggest short numeric code of the whole database (codes that are really a fiscal
    code are ignored), and free in the unit the row goes to."""
    row = _one(cursor, "SELECT MAX(CAST(CodPartener AS UNSIGNED)) AS n FROM Parteneri WHERE CodPartener REGEXP '^[0-9]{1,6}$'")
    number = int(row['n'] or 0) + 1
    while _one(cursor, 'SELECT 1 FROM Parteneri WHERE IdUnitate=%s AND CAST(CodPartener AS UNSIGNED)=%s AND CodPartener REGEXP %s',
               (unit_id, number, '^[0-9]{1,6}$')):
        number += 1
    return str(number).zfill(_CODE_WIDTH)


def _unit(cursor, lock=False):
    """The unit the new rows go to: the principal sector (the same choice as bu_catalog.supplier)."""
    unit = _one(cursor, 'SELECT IdUnitate FROM Unitati WHERE Ascuns=0 ORDER BY (SursaSector=%s) DESC,An DESC,IdUnitate LIMIT 1'
                + (' FOR UPDATE' if lock else ''), ('01A',))
    if not unit:
        raise NomError('NO_UNIT', 'Nomenclatorul de furnizori nu are o unitate locală activă.', 409)
    return unit


def _row_of(cursor, code):
    cursor.execute(_SELECT + ' WHERE ' + _VISIBLE + ' AND ' + suppliers.sql('CodFiscal') + '=%s', (code,))
    rows = _display_rows(cursor.fetchall())
    return _present(rows[0]) if rows else None


def _answer(cursor, code, name, address, created, reactivated=False):
    return {'cod_fiscal': code, 'name': name or '', 'address': address or '', 'created': created, 'reactivated': reactivated,
            'row': _row_of(cursor, code)}


def register(bp, guard, scoped, open_db):
    @contextmanager
    def session(write=False):
        if open_db is None:
            raise FormUnavailable('NO_DATABASE', 'Furnizorii nu sunt disponibili în previzualizarea locală.', 503)
        with open_db(g.portal['db_name']) as conn:
            cursor = conn.cursor(dictionary=True)
            try:
                yield cursor
                if write:
                    conn.commit()
            except Exception:
                conn.rollback()
                raise

    def body():
        data = request.get_json(silent=True)
        if not isinstance(data, dict) or set(data) - _ALLOWED:
            raise NomError('INVALID', 'Cerere invalidă.', 400)
        return data

    @bp.get('/api/vercon/suppliers/next-code')
    @guard(OPERATION)
    def supplier_next_code():
        scoped('read')
        with session() as cursor:
            return {'cod_partener': next_code(cursor, _unit(cursor)['IdUnitate'])}

    @bp.post('/api/vercon/suppliers/check')
    @guard(OPERATION)
    def supplier_check():
        data = body()
        scoped(OPERATION, data)
        code = clean_code(data.get('cod_fiscal'))
        _refuse_own_code(code)
        with session() as cursor:
            rows = existing(cursor, code)
        if rows:
            first = rows[0]
            return {'cod_fiscal': code, 'exists': True, 'hidden': bool(first['Ascuns']),
                    'name': first['DenumirePartener'] or first['CodPartener'], 'address': first['Adresa'] or ''}
        found = _lookup(code)   # no database lock is held while the external service answers
        if found is None:
            return {'cod_fiscal': code, 'exists': False, 'found': False}
        return {'cod_fiscal': code, 'exists': False, 'found': True, 'name': found['denumire'], 'address': found['adresa'],
                'nr_reg_com': found['nr_reg_com']}

    @bp.post('/api/vercon/suppliers/create')
    @guard(OPERATION)
    def supplier_create():
        data = body()
        ctx = scoped(OPERATION, data)
        code = clean_code(data.get('cod_fiscal'))
        _refuse_own_code(code)
        with session() as cursor:
            known = bool(existing(cursor, code))
        found = None
        if not known:
            found = _lookup(code)
            if found is None:
                raise NomError('ANAF_NOT_FOUND', 'ANAF nu cunoaște acest cod fiscal. Furnizorul nu poate fi adăugat.', 422)
        who = {**ctx['selection'], 'email': g.portal['email']}
        with session(True) as cursor:
            # The unit row is locked first: every insert of a supplier waits here, so the checks below cannot be raced.
            unit = _unit(cursor, lock=True)
            rows = existing(cursor, code, locked=True)
            visible = next((row for row in rows if not row['Ascuns']), None)
            if visible:
                if data.get('if_missing') is True:
                    return _answer(cursor, code, visible['DenumirePartener'], visible['Adresa'], False)
                raise NomError('FISCAL_EXISTS', 'Furnizorul există deja: ' + (visible['DenumirePartener'] or visible['CodPartener']) + '.', 409)
            if rows:   # only hidden rows: the supplier is not added twice, it becomes visible again
                cursor.execute('UPDATE Parteneri SET Ascuns=0 WHERE ' + suppliers.sql('CodFiscal') + '=%s AND COALESCE(Ascuns,0)<>0', (code,))
                audit(cursor, who, 'supplier.reactivate', 'Parteneri', rows[0]['IdPartener'], {'Ascuns': 1}, {'Ascuns': 0, 'CodFiscal': code},
                      None, request.headers.get('X-Request-Id'))
                return _answer(cursor, code, rows[0]['DenumirePartener'], rows[0]['Adresa'], False, True)
            if found is None:
                raise NomError('SUPPLIER_CHANGED', 'Lista furnizorilor s-a schimbat între timp. Reîncercați.', 409)
            number = next_code(cursor, unit['IdUnitate'])
            cursor.execute('INSERT INTO Parteneri (IdUnitate,CodPartener,DenumirePartener,CodFiscal,Adresa,Tip,Ascuns,Provenienta) '
                           "VALUES (%s,%s,%s,%s,%s,'1',0,%s)",
                           (unit['IdUnitate'], number, found['denumire'], code, found['adresa'] or None, PROVENANCE))
            audit(cursor, who, 'supplier.create', 'Parteneri', cursor.lastrowid, None,
                  {'CodPartener': number, 'CodFiscal': code, 'DenumirePartener': found['denumire'], 'Adresa': found['adresa']},
                  None, request.headers.get('X-Request-Id'))
            return _answer(cursor, code, found['denumire'], found['adresa'], True)
