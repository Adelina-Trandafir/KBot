"""Generate the explicit ADE schema from the immutable Access field inventory."""
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
TABLES = 'Grupe ValoriTaxe LunaD Platitori Platitori_sub Prezenta Plati Chitante AlteDoc Retur SS_Buget'.split()
# Access columns that are not carried over, for one table only (the child has no tax of its own: Prezenta/LunaD hold IDV; Avans is not used).
EXCLUDED_BY_TABLE = {'Platitori': {'IDV', 'Avans', 'Adresa'}, 'Platitori_sub': {'J'},
                     'Plati': {'Valoare', 'Anticipat', 'Restanta'},
                     'Chitante': {'Valoare'},
                     'Prezenta': {'ZileLuna', 'ZileAbsenta', 'MZCPrezenta', 'MZCAbsenta', 'ValoareMancare', 'Detalii', 'Restanta', 'Avans', 'SI'},
                     'Retur': {'Motivul'},
                     'ValoriTaxe': {'TaxaLunara', 'TaxaMicDejun', 'TaxaDejun', 'Murdar', 'DoarCalculZilnic', 'TaxaMancare'}}  # Adresa moves to Platitori_sub at import
EXCLUDED = {'IDF', 'Frate', 'ReducereFrate', 'ReducereBonus', 'ZileCon', 'DisCon', 'ZilePre', 'DisPre', 'DisBro'}
EXTRA_FIELDS = {
    # M05: provenance survives deleting the following open month. It is never
    # reconstructed from the document date because M04 intentionally allows a
    # date outside the selected month/year in one dimension.
    'Plati': {
        'OriginMonth': {'type': 'Long', 'size': 4, 'default': ''},
        'OriginYear': {'type': 'Long', 'size': 4, 'default': ''},
    },
    # Web-only explanation for the other-payments tab (operator request 08.10.2026); Access has none.
    'AlteDoc': {
        'Explicatie': {'type': 'Text', 'size': 255, 'default': ''},
    },
    # Web-only: the payer's own CNP (the child's CNP stays on Platitori); Access has none.
    'Platitori_sub': {
        'CNP_Platitor': {'type': 'Text', 'size': 255, 'default': ''},
    },
    # Working days of the month, kept once here instead of on every Prezenta row; Access has none.
    'LunaD': {
        'ZileLuna': {'type': 'Long', 'size': 4, 'default': ''},
    },
    'Retur': {
        'OriginMonth': {'type': 'Long', 'size': 4, 'default': ''},
        'OriginYear': {'type': 'Long', 'size': 4, 'default': ''},
    },
}
# Foreign keys: child table -> column -> (parent table, ON DELETE). Every FK column is nullable (NULL = no link);
# money history is RESTRICT, rows that only exist inside a month/attendance row follow it, and a movement outliving its
# deleted month keeps NULL (the web reopen flow already clears IDL/IDZ before deleting; origin month/year stay).
FOREIGN_KEYS = {
    'Platitori': {'IDG': ('Grupe', 'RESTRICT')},
    'Platitori_sub': {'IDP': ('Platitori', 'RESTRICT')},
    'LunaD': {'IDV': ('ValoriTaxe', 'RESTRICT')},
    'Prezenta': {'IDP': ('Platitori', 'RESTRICT'), 'IDL': ('LunaD', 'RESTRICT'), 'IDV': ('ValoriTaxe', 'RESTRICT'),
                 'IDG': ('Grupe', 'RESTRICT')},
    'Plati': {'IDP': ('Platitori', 'RESTRICT'), 'IDZ': ('Prezenta', 'SET NULL'), 'IDS': ('Platitori_sub', 'RESTRICT'),
              'IDL': ('LunaD', 'SET NULL')},
    'Chitante': {'IDPL': ('Plati', 'RESTRICT'), 'IDL': ('LunaD', 'SET NULL')},
    'AlteDoc': {'IDPL': ('Plati', 'RESTRICT'), 'IDL': ('LunaD', 'SET NULL')},
    'Retur': {'IDP': ('Platitori', 'RESTRICT'), 'IDZ': ('Prezenta', 'SET NULL'), 'IDS': ('Platitori_sub', 'RESTRICT'),
              'IDL': ('LunaD', 'SET NULL')},
    'SS_Buget': {'IDG': ('Grupe', 'RESTRICT'), 'IDP': ('Platitori', 'RESTRICT'), 'IDL': ('LunaD', 'CASCADE'),
                 'IDZ': ('Prezenta', 'CASCADE')},
}
# Access Text columns that only ever hold digits and are stored as numbers here.
NUMERIC_TEXT = {'Plati': {'TIP'}}
UNIQUE_KEYS = {
    'LunaD': (('Luna', 'Anul'),),
    'Prezenta': (('IDP', 'IDL'),),
    'Chitante': (('Serie', 'Numar'),),
}


def generate():
    # This generator targets the old AD_01 inventory; AD_03 has explicit web-only histories.
    schema_path = ROOT / 'PYTHON/routes/adechit/schema.json'
    if schema_path.exists() and 'Grupe_Educator' in json.loads(schema_path.read_text(encoding='utf-8')):
        raise RuntimeError('Legacy AD_01 generator cannot overwrite AD_03 runtime metadata.')
    schema = {}
    ddl = ['-- SLICE-ADE3-01. Tables of the kindergarten module (prefix AD_), for AVACONT_SURSA. Charset matches the server (utf8mb3).',
           '-- No USE, DROP, data seeds, or changes to existing business tables.',
           '-- Legacy monetary types are preserved. Server execution is not locally validated.']
    types = {'Long': 'INT', 'AutoNumber': 'INT', 'Double': 'DOUBLE',
             'Yes/No': 'BOOLEAN', 'Date/Time': 'DATETIME', 'Memo/Long Text': 'LONGTEXT'}
    TABLE_KEYS = {}
    for name in TABLES:
        fields = {}
        for line in (ROOT / 'ADECHIT/ACCESS_SOURCES/tables' / (name + '.md')).read_text(encoding='utf-8-sig').splitlines():
            cells = [cell.strip() for cell in line.split('|')]
            if len(cells) < 10 or not cells[1].isdigit() or cells[2] in EXCLUDED or cells[2] in EXCLUDED_BY_TABLE.get(name, ()):
                continue
            if cells[2] in NUMERIC_TEXT.get(name, ()):
                fields[cells[2]] = {'type': 'Long', 'size': 4, 'default': ''}
                continue
            fields[cells[2]] = {'type': cells[3], 'size': int(cells[4]), 'default': cells[6]}
        fields.update(EXTRA_FIELDS.get(name, {}))
        key = next(iter(fields))
        TABLE_KEYS[name] = key
        schema[name] = {'key': key, 'fields': fields}
        columns = []
        for field, spec in fields.items():
            sqltype = f"VARCHAR({spec['size']})" if spec['type'] == 'Text' else types[spec['type']]
            suffix = ' NOT NULL AUTO_INCREMENT PRIMARY KEY' if field == key else ' NULL'
            if field in FOREIGN_KEYS.get(name, {}):
                suffix = ' NULL DEFAULT NULL'
            elif field != key and spec['default'] in ('0', 'No'):
                suffix += ' DEFAULT 0'
            columns.append(f'  `{field}` {sqltype}{suffix}')
        columns.append('  `Version` BIGINT NOT NULL DEFAULT 1')
        for field in ('IDP', 'IDL', 'IDZ', 'IDPL', 'IDS', 'IDG', 'IDV'):
            if field in fields and field != key:
                columns.append(f'  INDEX `ix_{field}` (`{field}`)')
        for field, (parent, rule) in FOREIGN_KEYS.get(name, {}).items():
            columns.append(f'  CONSTRAINT `fk_{name}_{field}` FOREIGN KEY (`{field}`) REFERENCES `AD_{parent}` '
                           f'(`{TABLE_KEYS[parent]}`) ON DELETE {rule} ON UPDATE CASCADE')
        for unique in UNIQUE_KEYS.get(name, ()):
            label = '_'.join(unique)
            columns.append(f'  UNIQUE KEY `uq_{label}` (' + ','.join(f'`{field}`' for field in unique) + ')')
        ddl.append(f'CREATE TABLE IF NOT EXISTS `AD_{name}` (\n' + ',\n'.join(columns) +
                   '\n) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;')
    ddl.extend([
        'CREATE TABLE IF NOT EXISTS AD_Lock (ID INT PRIMARY KEY) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;',
        'INSERT IGNORE INTO AD_Lock (ID) VALUES (1);',
        'CREATE TABLE IF NOT EXISTS AD_Operations (RequestKey VARCHAR(100) PRIMARY KEY, Fingerprint CHAR(64) NOT NULL, Result LONGTEXT NOT NULL) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;',
        'CREATE TABLE IF NOT EXISTS AD_Settings (SettingKey VARCHAR(64) PRIMARY KEY, SettingValue VARCHAR(255) NOT NULL) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;',
        "INSERT IGNORE INTO AD_Settings (SettingKey, SettingValue) VALUES ('BlockReopenWithMovements', '1');",
        'CREATE TABLE IF NOT EXISTS AD_Imports (SourceHash CHAR(64) PRIMARY KEY, SourceFile VARCHAR(255) NOT NULL, Manifest LONGTEXT NOT NULL, Result LONGTEXT NOT NULL, ImportedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;',
    ])
    (ROOT / 'PYTHON/routes/adechit/schema.json').write_text(json.dumps(schema, indent=2) + '\n', encoding='utf-8')
    (ROOT / 'sql/AD_01_schema.sql').write_text('\n\n'.join(ddl) + '\n', encoding='utf-8')


if __name__ == '__main__':
    generate()
