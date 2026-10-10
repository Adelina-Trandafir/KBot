"""Exercise initial import and reconciliation against a disposable local SQLite database."""
import argparse
import json
import sqlite3
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'PYTHON'))

from routes.adechit.domain import SCHEMA
from routes.adechit.importer import import_dataset, reconcile_dataset
from routes.adechit.repository import Repository
from preview import initialize


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('extract', type=Path)
    parser.add_argument('database', type=Path)
    args = parser.parse_args()
    args.database.parent.mkdir(parents=True, exist_ok=True)
    initialize(args.database)
    connection = sqlite3.connect(args.database)
    connection.row_factory = sqlite3.Row
    try:
        for table in SCHEMA:
            connection.execute(f'DELETE FROM `AD_{table}`')
        connection.execute('DELETE FROM AD_Imports')
        connection.execute('DELETE FROM AD_Operations')
        connection.execute('DELETE FROM AD_ReceiptConfig')
        connection.execute('DELETE FROM AD_IdMap')
        connection.commit()
        repo = Repository(connection, sqlite=True)
        repo.subunit_id, repo.subunit_name = 1, 'Evidenta de proba'
        dataset = json.loads(args.extract.read_text(encoding='utf-8'))
        result = import_dataset(repo, dataset, 'fixture', args.extract.name)
        reconciliation = reconcile_dataset(repo, dataset, 'fixture')
        connection.commit()
        print(json.dumps({'import': result, 'reconciliation': reconciliation}, ensure_ascii=False))
        if not reconciliation['ok']:
            raise SystemExit(1)
    finally:
        connection.close()


if __name__ == '__main__':
    main()
