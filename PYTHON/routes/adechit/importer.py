"""Validated, repeatable ADE import and source/target reconciliation."""
import hashlib
import json
from collections import Counter
from datetime import date, datetime

from .domain import SCHEMA, DomainError, require
from .service import validate_values


IMPORT_ORDER = (
    'Grupe', 'ValoriTaxe', 'Platitori', 'Platitori_sub', 'LunaD',
    'Prezenta', 'Plati', 'Chitante', 'AlteDoc', 'Retur',
    'SS_Buget',
)
SOURCE_IGNORED = {'ZileLuna', 'ZileAbsenta', 'MZCPrezenta', 'MZCAbsenta', 'ValoareMancare', 'Detalii', 'Motivul',
                  'TaxaLunara', 'TaxaMicDejun', 'TaxaDejun', 'Murdar', 'DoarCalculZilnic', 'TaxaMancare', 'Valoare', 'Anticipat', 'Restanta', 'IDF', 'J', 'IDV', 'Avans', 'Adresa', 'Frate', 'ReducereFrate', 'ReducereBonus', 'ZileCon', 'DisCon', 'ZilePre', 'DisPre', 'DisBro'}
# Every foreign key of the AD_ tables (child table, column, parent table). Mirrors the FOREIGN KEY lines of sql/AD_01_schema.sql.
RELATIONS = (
    ('Platitori', 'IDG', 'Grupe'),
    ('Platitori_sub', 'IDP', 'Platitori'),
    ('LunaD', 'IDV', 'ValoriTaxe'), ('Prezenta', 'IDP', 'Platitori'),
    ('Prezenta', 'IDL', 'LunaD'), ('Prezenta', 'IDV', 'ValoriTaxe'),
    ('Prezenta', 'IDG', 'Grupe'),
    ('Plati', 'IDP', 'Platitori'), ('Plati', 'IDZ', 'Prezenta'),
    ('Plati', 'IDS', 'Platitori_sub'), ('Plati', 'IDL', 'LunaD'),
    ('Chitante', 'IDPL', 'Plati'), ('Chitante', 'IDL', 'LunaD'),
    ('AlteDoc', 'IDPL', 'Plati'), ('AlteDoc', 'IDL', 'LunaD'),
    ('Retur', 'IDP', 'Platitori'), ('Retur', 'IDZ', 'Prezenta'),
    ('Retur', 'IDS', 'Platitori_sub'), ('Retur', 'IDL', 'LunaD'),
    ('SS_Buget', 'IDG', 'Grupe'), ('SS_Buget', 'IDP', 'Platitori'),
    ('SS_Buget', 'IDL', 'LunaD'), ('SS_Buget', 'IDZ', 'Prezenta'),
)
# Links that old data may leave dangling (a month or attendance row that was deleted): the import stores NULL and counts a warning.
HISTORICAL_OPTIONAL = {
    ('Plati', 'IDZ', 'Prezenta'), ('Plati', 'IDL', 'LunaD'),
    ('Retur', 'IDZ', 'Prezenta'), ('Retur', 'IDL', 'LunaD'),
    ('SS_Buget', 'IDL', 'LunaD'), ('SS_Buget', 'IDZ', 'Prezenta'), ('SS_Buget', 'IDG', 'Grupe'),
    ('Chitante', 'IDL', 'LunaD'), ('AlteDoc', 'IDL', 'LunaD'),
}


def canonical_payload(dataset):
    return json.dumps(dataset, ensure_ascii=False, sort_keys=True, separators=(',', ':'), default=str)


def source_hash(dataset):
    return hashlib.sha256(canonical_payload(dataset).encode('utf-8')).hexdigest()


def _normal(value, field):
    if value is None:
        return None
    kind = field['type']
    if kind == 'Yes/No':
        return bool(value)
    if kind in ('Long', 'AutoNumber'):
        if isinstance(value, str) and not value.strip():
            return None  # Access Text columns turned numeric (Plati.TIP) may hold ''
        return int(value)
    if kind == 'Double':
        return float(value)
    if kind == 'Date/Time':
        if isinstance(value, (date, datetime)):
            return value.isoformat()
        return str(value).replace(' ', 'T', 1)
    return str(value)


def normalize_dataset(raw):
    require(isinstance(raw, dict) and isinstance(raw.get('tables'), dict), 'IMPORT_FORMAT',
            'Extractul ADE nu are formatul așteptat.', 400)
    require(raw.get('format') == 'adechit-access-v1', 'IMPORT_FORMAT',
            'Versiunea extractului ADE nu este acceptată.', 400)
    unknown = set(raw['tables']) - set(SCHEMA)
    require(not unknown, 'IMPORT_TABLE', 'Extractul conține tabele ADE necunoscute.', 400)
    tables = {}
    for table in IMPORT_ORDER:
        rows = raw['tables'].get(table, [])
        require(isinstance(rows, list), 'IMPORT_ROWS', f'Tabelul {table} nu conține o listă de rânduri.', 400)
        spec = SCHEMA[table]
        normalized = []
        keys = set()
        for source in rows:
            require(isinstance(source, dict), 'IMPORT_ROW', f'Rând invalid în {table}.', 400)
            unknown_fields = set(source) - set(spec['fields']) - SOURCE_IGNORED
            require(not unknown_fields, 'IMPORT_FIELDS', f'Câmpuri necunoscute în {table}.', 400)
            row = {key: _normal(value, spec['fields'][key]) for key, value in source.items() if key in spec['fields']}
            key = row.get(spec['key'])
            require(key is not None and key not in keys, 'IMPORT_KEY', f'Cheie lipsă sau duplicată în {table}.', 400)
            keys.add(key)
            normalized.append(validate_values(table, row))
        tables[table] = normalized
    # The address of the child (Access Platitori.Adresa) belongs to the payers: copy it onto every payer of that child whose own address is empty or just a town (shorter than 10 characters, e.g. 'Ploiesti').
    addresses = {}
    for source in raw['tables'].get('Platitori', []):
        text = str(source.get('Adresa') or '').strip()
        if text and source.get('IDP') is not None:
            addresses[_normal(source['IDP'], SCHEMA['Platitori']['fields']['IDP'])] = text
    for payer in tables['Platitori_sub']:
        if payer.get('IDP') in addresses and len((payer.get('Adresa') or '').strip()) < 10:
            payer['Adresa'] = addresses[payer['IDP']]
    # Working days: Access kept them on every Prezenta row; here they live on the month (the most common value of its rows).
    days = {}
    for source in raw['tables'].get('Prezenta', []):
        if source.get('IDL') is not None and source.get('ZileLuna') is not None:
            days.setdefault(_normal(source['IDL'], SCHEMA['LunaD']['fields']['IDL']), Counter())[int(source['ZileLuna'])] += 1
    for month in tables['LunaD']:
        if month.get('ZileLuna') is None and month['IDL'] in days:
            month['ZileLuna'] = days[month['IDL']].most_common(1)[0][0]
    result = {'format': 'adechit-access-v1', 'source': raw.get('source', ''), 'tables': tables}
    if raw.get('receipt_config') is not None:
        config = raw['receipt_config']
        require(isinstance(config, dict), 'IMPORT_CONFIG', 'Configurația chitanțelor este invalidă.', 400)
        result['receipt_config'] = {
            'Serie': str(config.get('Serie') or '').strip(),
            'Numar': int(config.get('Numar')),
            'Explicatie': str(config.get('Explicatie') or ''),
        }
    return result


def relationship_issues(dataset, include_historical=False):
    tables = dataset['tables']
    keys = {name: {row[SCHEMA[name]['key']] for row in rows} for name, rows in tables.items()}
    issues = []
    for table, field, parent in RELATIONS:
        for row in tables[table]:
            value = row.get(field)
            if value is not None and value not in keys[parent]:
                if not include_historical and (table, field, parent) in HISTORICAL_OPTIONAL:
                    continue
                issues.append({'table': table, 'key': row[SCHEMA[table]['key']],
                               'field': field, 'value': value, 'parent': parent})
    return issues


def without_dangling_links(dataset):
    """Copy of the tables where every dangling HISTORICAL_OPTIONAL link is NULL, so the foreign keys accept the rows."""
    keys = {name: {row[SCHEMA[name]['key']] for row in rows} for name, rows in dataset['tables'].items()}
    tables = {name: [dict(row) for row in rows] for name, rows in dataset['tables'].items()}
    for table, field, parent in RELATIONS:
        if (table, field, parent) not in HISTORICAL_OPTIONAL:
            continue
        for row in tables[table]:
            if row.get(field) is not None and row[field] not in keys[parent]:
                row[field] = None
    return tables


def validate_dataset(raw):
    dataset = normalize_dataset(raw)
    issues = relationship_issues(dataset)
    require(not issues, 'IMPORT_RELATION', f'Extractul conține {len(issues)} legături orfane.', 400)
    config = dataset.get('receipt_config')
    if config:
        require(config['Serie'] and config['Numar'] > 0 and config['Explicatie'], 'IMPORT_CONFIG',
                'Configurația chitanțelor este incompletă.', 400)
    return dataset


def _counts(dataset):
    return {table: len(dataset['tables'][table]) for table in IMPORT_ORDER}


def import_dataset(repo, raw, unit, source_file):
    dataset = validate_dataset(raw)
    historical_warnings = relationship_issues(dataset, include_historical=True)
    digest = source_hash(dataset)
    previous = repo.query('SELECT Result FROM AD_Imports WHERE SourceHash=%s', (digest,))
    if previous:
        return {**json.loads(previous[0]['Result']), 'repeated': True}
    other = repo.query('SELECT SourceHash FROM AD_Imports')
    require(not other, 'IMPORT_CHANGED', 'Există deja un alt lot importat; extractul modificat nu se aplică automat.')
    nonempty = [table for table in IMPORT_ORDER if repo.rows(table)]
    require(not nonempty, 'IMPORT_NOT_EMPTY', 'Importul inițial cere tabele AD goale; datele existente nu sunt suprascrise.')

    tables = without_dangling_links(dataset)
    for table in IMPORT_ORDER:
        for row in tables[table]:
            if table in ('Plati', 'Retur') and row.get('IDL') is not None:
                month = next((item for item in tables['LunaD'] if item['IDL'] == row['IDL']), None)
                if month is not None:
                    row = {**row, 'OriginMonth': month['Luna'], 'OriginYear': month['Anul']}
            repo.insert(table, row)

    config = dataset.get('receipt_config')
    if config:
        existing = repo.query('SELECT DC FROM AVACONT_COMUN.Unitati_Chitante WHERE DC=%s', (unit,))
        require(not existing, 'IMPORT_CONFIG_EXISTS', 'Configurația chitanțelor există deja și nu este suprascrisă.')
        repo.execute('INSERT INTO AVACONT_COMUN.Unitati_Chitante (DC,Serie,Numar,Explicatie) VALUES (%s,%s,%s,%s)',
                     (unit, config['Serie'], config['Numar'], config['Explicatie']))
    result = {'source_hash': digest, 'counts': _counts(dataset), 'unit': unit, 'repeated': False,
              'historical_relationship_warnings': len(historical_warnings)}
    repo.execute('INSERT INTO AD_Imports (SourceHash,SourceFile,Manifest,Result) VALUES (%s,%s,%s,%s)',
                 (digest, source_file, canonical_payload({'counts': result['counts'], 'unit': unit}),
                  canonical_payload(result)))
    return result


def reconcile_dataset(repo, raw, unit):
    dataset = validate_dataset(raw)
    differences = []
    tables = without_dangling_links(dataset)
    for table in IMPORT_ORDER:
        key = SCHEMA[table]['key']
        source = {row[key]: row for row in tables[table]}
        target = {row[key]: row for row in repo.rows(table)}
        for missing in sorted(set(source) - set(target)):
            differences.append({'table': table, 'key': missing, 'kind': 'missing'})
        for extra in sorted(set(target) - set(source)):
            differences.append({'table': table, 'key': extra, 'kind': 'extra'})
        for row_id in sorted(set(source) & set(target)):
            for field, info in SCHEMA[table]['fields'].items():
                if field in ('OriginMonth', 'OriginYear'):
                    continue
                left = _normal(source[row_id].get(field), info)
                right = _normal(target[row_id].get(field), info)
                if left != right:
                    differences.append({'table': table, 'key': row_id, 'kind': 'value',
                                        'field': field, 'source': left, 'target': right})
    config = dataset.get('receipt_config')
    if config:
        rows = repo.query('SELECT Serie,Numar,Explicatie FROM AVACONT_COMUN.Unitati_Chitante WHERE DC=%s', (unit,))
        if len(rows) != 1 or any(rows[0].get(key) != value for key, value in config.items()):
            differences.append({'table': 'Unitati_Chitante', 'key': unit, 'kind': 'config'})
    return {'ok': not differences, 'counts': _counts(dataset), 'differences': differences,
            'historical_relationship_warnings': len(relationship_issues(dataset, include_historical=True))}
