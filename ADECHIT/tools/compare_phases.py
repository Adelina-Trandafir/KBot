"""Compare Access and web mdl_Situatie results phase by phase, without tolerances."""
import argparse
import json
from pathlib import Path

PHASES = (
    'qPrezenta', 'qSolduri', 'Update_Solduri', 'Update_Situatie',
    'qExplicatie', 'Update_detalii', 'Update_Compensare', 'Salvare_Lunara',
)


def _rows(value):
    if isinstance(value, dict):
        return value
    if not isinstance(value, list):
        raise ValueError('Each phase must be a list or an object keyed by row identity.')
    result = {}
    for index, row in enumerate(value):
        if not isinstance(row, dict):
            raise ValueError('Phase rows must be objects.')
        key = row.get('IDZ', row.get('IDP', index))
        if key in result:
            key = f'{key}#{index}'
        result[str(key)] = row
    return result


def _details(value):
    if value is None:
        return None
    return sorted(str(value).split(';'))


def compare(access, web):
    differences = []
    first_phase = None
    for phase in PHASES:
        if phase not in access and phase not in web:
            continue
        if phase not in access or phase not in web:
            differences.append({'phase': phase, 'kind': 'missing_phase',
                                'side': 'access' if phase not in access else 'web'})
            first_phase = first_phase or phase
            continue
        left, right = _rows(access[phase]), _rows(web[phase])
        for key in sorted(set(left) | set(right)):
            if key not in left or key not in right:
                differences.append({'phase': phase, 'key': key, 'kind': 'missing_row',
                                    'side': 'access' if key not in left else 'web'})
                first_phase = first_phase or phase
                continue
            for field in sorted(set(left[key]) | set(right[key])):
                a, b = left[key].get(field), right[key].get(field)
                equal = _details(a) == _details(b) if field == 'Detalii' else a == b
                if not equal:
                    differences.append({'phase': phase, 'key': key, 'field': field,
                                        'kind': 'value', 'access': a, 'web': b})
                    first_phase = first_phase or phase
    return {'ok': not differences, 'first_divergent_phase': first_phase, 'differences': differences}


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('access', type=Path)
    parser.add_argument('web', type=Path)
    parser.add_argument('--output', type=Path)
    args = parser.parse_args()
    result = compare(json.loads(args.access.read_text(encoding='utf-8')),
                     json.loads(args.web.read_text(encoding='utf-8')))
    text = json.dumps(result, ensure_ascii=False, indent=2) + '\n'
    if args.output:
        args.output.write_text(text, encoding='utf-8')
    else:
        print(text, end='')
    raise SystemExit(0 if result['ok'] else 1)


if __name__ == '__main__':
    main()
