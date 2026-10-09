"""Build reproducible web phase output from a read-only Access extract.

The Access result file is intentionally only a pending template: it must be populated
by executing the named queries in Access. It is never inferred from Python output.
"""
import argparse
import json
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'PYTHON'))

from routes.adechit.calculations import calculate
from routes.adechit.importer import validate_dataset
from compare_phases import PHASES


def write(path, value):
    path.write_text(json.dumps(value, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('extract', type=Path)
    parser.add_argument('month_id', type=int)
    parser.add_argument('output', type=Path)
    args = parser.parse_args()
    dataset = validate_dataset(json.loads(args.extract.read_text(encoding='utf-8')))
    month = next((row for row in dataset['tables']['LunaD'] if row['IDL'] == args.month_id), None)
    if month is None:
        raise SystemExit(f'IDL {args.month_id} is not present in the extract.')
    rows, phases = calculate(dataset['tables'], args.month_id)
    args.output.mkdir(parents=True, exist_ok=True)
    write(args.output / 'input.json', {'month': month, 'dataset': dataset})
    write(args.output / 'web-phases.json', phases)
    write(args.output / 'access-phases.pending.json', {
        '_status': 'PENDING_ACCESS_RUN',
        '_month_id': args.month_id,
        '_required_phases': list(PHASES),
        '_instruction': 'Replace metadata with phase arrays exported after each Access query; never copy web results.',
    })
    print(f'Built fixture for IDL={args.month_id}: {len(rows)} rows; Access results remain pending.')


if __name__ == '__main__':
    main()
