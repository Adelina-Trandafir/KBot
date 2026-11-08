"""AlteDoc number proposals for the calendar year currently in progress in Romania."""
import re
from utils.clock import bucharest_today

from .domain import require


def numbers_in_year(repo, year):
    rows = repo.query('SELECT NrDoc FROM AD_AlteDoc WHERE SubunitId=%s '
                      'AND DataDoc>=%s AND DataDoc<%s FOR UPDATE',
                      (repo.scope(), f'{year:04d}-01-01', f'{year + 1:04d}-01-01'))
    return {int(text) for row in rows if re.fullmatch(r'[0-9]+', text := str(row['NrDoc'] or '').strip())}


def next_number(repo, year):
    number = max(numbers_in_year(repo, year), default=0) + 1
    require(len(str(number)) <= 255, 'DOCUMENT_NUMBER_LIMIT', 'Următorul număr de document depășește limita câmpului.')
    return number


def proposal(repo, month_id, extract_id=None):
    repo.get('LunaD', month_id)
    year = bucharest_today().year
    if extract_id is not None:
        sources = repo.query('SELECT IDFXE FROM FX_Extrase WHERE IDFXE=%s', (extract_id,))
        require(bool(sources), 'NOT_FOUND', 'Rândul de extras nu mai există.', 404)
    return {'year': year, 'number': str(next_number(repo, year))}


def allocation_numbers(repo, allocations, proposed_year=None):
    year = bucharest_today().year
    used = numbers_in_year(repo, year)
    start = max(used, default=0) + 1
    proposed = [item.get('document_number') for item in allocations]
    # Older manual pages have no proposal fields; give every child its own next number too.
    if proposed_year is None and all(number is None for number in proposed):
        numbers = [str(start + index) for index in range(len(allocations))]
        require(len(numbers[-1]) <= 255, 'DOCUMENT_NUMBER_LIMIT', 'Numerele documentelor depășesc limita câmpului.')
        return numbers
    require(proposed_year == year, 'BANK_NUMBER_CHANGED',
            'Anul numerotării s-a schimbat. Propunerea a fost actualizată; verificați numerele și asociați din nou.',
            409, {'year': year, 'number': str(start)})
    numbers, assigned = [], set()
    for value in proposed:
        require(isinstance(value, str) and 0 < len(value.strip()) <= 255
                and re.fullmatch(r'[0-9]+', value.strip()) and int(value.strip()) > 0,
                'BANK_DOCUMENT_NUMBER_INVALID', 'Completați un număr de document întreg, pozitiv pentru fiecare copil.', 400)
        number = int(value.strip())
        require(number not in assigned, 'BANK_DOCUMENT_NUMBER_DUPLICATE',
                f'Numărul {number} este atribuit mai multor copii din repartizare. Fiecare copil trebuie să aibă alt număr.', 400)
        require(number not in used, 'BANK_DOCUMENT_NUMBER_USED',
                f'Numărul {number} este deja folosit în AlteDoc în anul calendaristic {year}. Introduceți alt număr.')
        assigned.add(number)
        numbers.append(str(number))
    return numbers
