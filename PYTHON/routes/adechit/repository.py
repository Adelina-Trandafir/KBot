"""SLICE-ADE3/8: shared DB connections; one unit lock serializes all ADE mutations.

Lock order: AD_Lock -> permission -> business rows -> common receipt configuration.
No automatic retry around business writes; operation keys handle lost responses.
"""
from contextlib import contextmanager
from datetime import date, datetime
from .domain import SCHEMA, require


class Repository:
    def __init__(self, connection, sqlite=False):
        self.connection = connection
        self.sqlite = sqlite
        self.cursor = connection.cursor() if sqlite else connection.cursor(dictionary=True)

    def execute(self, sql, params=()):
        if self.sqlite:
            sql = sql.replace('%s', '?').replace(' FOR UPDATE', '').replace('AVACONT_COMUN.Unitati_Chitante', 'Unitati_Chitante')
        self.cursor.execute(sql, params)
        return self.cursor

    def query(self, sql, params=()):
        return [{key: value.isoformat() if isinstance(value, (date, datetime)) else value
                 for key, value in dict(row).items()} for row in self.execute(sql, params).fetchall()]

    def rows(self, table, **filters):
        require(table in SCHEMA, 'TABLE', 'Tabel necunoscut.', 404)
        require(set(filters) <= set(SCHEMA[table]['fields']), 'FILTER', 'Filtru invalid.', 400)
        where = ' WHERE ' + ' AND '.join(f'`{key}`=%s' for key in filters) if filters else ''
        return self.query(f'SELECT * FROM `AD_{table}`{where} ORDER BY `{SCHEMA[table]["key"]}`', tuple(filters.values()))

    def years(self):
        return [row['Anul'] for row in self.query('SELECT DISTINCT Anul FROM AD_LunaD ORDER BY Anul DESC')]

    def situation_data(self, month_id, group_id):
        # Keep all earlier movements of the selected children for opening balances,
        # including movements in groups they previously belonged to.
        people = 'SELECT IDP FROM AD_Prezenta WHERE IDL=%s AND IDG=%s'
        attendance = f'SELECT IDZ FROM AD_Prezenta WHERE IDP IN ({people})'
        payments = f'SELECT IDPL FROM AD_Plati WHERE IDZ IN ({attendance})'
        data = {table: [] for table in SCHEMA}
        data['LunaD'] = [self.get('LunaD', month_id)]
        data['Grupe'] = [self.get('Grupe', group_id)]
        data['Grupe_Educator'] = self.rows('Grupe_Educator', IDG=group_id)
        for table, clause in [('Platitori', f'IDP IN ({people})'),
                              ('Prezenta', f'IDP IN ({people})'),
                              ('Plati', f'IDZ IN ({attendance})'),
                              ('Retur', f'IDP IN ({people})'),
                              ('Chitante', f'IDPL IN ({payments})'),
                              ('AlteDoc', f'IDPL IN ({payments})')]:
            data[table] = self.query(f'SELECT * FROM `AD_{table}` WHERE {clause}', (month_id, group_id))
        return data

    def get(self, table, row_id):
        key = SCHEMA[table]['key']
        result = self.query(f'SELECT * FROM `AD_{table}` WHERE `{key}`=%s', (row_id,))
        require(bool(result), 'NOT_FOUND', 'Înregistrarea nu există în unitatea curentă.', 404)
        return result[0]

    def insert(self, table, values):
        require(table in SCHEMA and set(values) <= set(SCHEMA[table]['fields']), 'FIELDS', 'Câmpuri necunoscute.', 400)
        keys = list(values)
        self.execute(f'INSERT INTO `AD_{table}` (' + ','.join(f'`{k}`' for k in keys) + ') VALUES (' + ','.join(['%s'] * len(keys)) + ')', tuple(values[k] for k in keys))
        return self.get(table, values.get(SCHEMA[table]['key'], self.cursor.lastrowid))

    def update(self, table, row, changes):
        require(bool(changes), 'VALUES', 'Nu există modificări.', 400)
        require(set(changes) <= set(SCHEMA[table]['fields']), 'FIELDS', 'Câmpuri necunoscute.', 400)
        key = SCHEMA[table]['key']
        self.execute(f'UPDATE `AD_{table}` SET ' + ','.join(f'`{k}`=%s' for k in changes) + ', Version=Version+1 '
                     f'WHERE `{key}`=%s AND Version=%s', (*changes.values(), row[key], row['Version']))
        require(self.cursor.rowcount == 1, 'CONFLICT', 'Rândul a fost modificat. Reîncărcați înainte de a reîncerca.')
        return self.get(table, row[key])

    def delete(self, table, row):
        require(table in SCHEMA, 'TABLE', 'Tabel necunoscut.', 404)
        key = SCHEMA[table]['key']
        self.execute(f'DELETE FROM `AD_{table}` WHERE `{key}`=%s AND Version=%s',
                     (row[key], row['Version']))
        require(self.cursor.rowcount == 1, 'CONFLICT', 'Rândul a fost modificat. Reîncărcați înainte de a reîncerca.')

    def setting(self, key, default=None):
        rows = self.query('SELECT SettingValue FROM AD_Settings WHERE SettingKey=%s', (key,))
        return rows[0]['SettingValue'] if rows else default

    def data(self):
        return {table: self.rows(table) for table in SCHEMA}



@contextmanager
def transaction(db_name, factory=None):
    if factory is None:
        from utils.database import get_kbot_connection
        connection = get_kbot_connection(db_name)
        repo = Repository(connection)
    else:
        repo = factory(db_name)
        connection = repo.connection
    try:
        if repo.sqlite:
            connection.execute('BEGIN IMMEDIATE')
        else:
            connection.start_transaction()
        repo.execute('SELECT ID FROM AD_Lock WHERE ID=1 FOR UPDATE').fetchall()
        yield repo
        connection.commit()
    except Exception:
        connection.rollback()
        raise
    finally:
        repo.cursor.close()
        connection.close()
