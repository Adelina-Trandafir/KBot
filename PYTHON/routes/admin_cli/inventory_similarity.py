"""SLICE-VC12-01: explainable inventory candidates; never a decision to import."""
import re
import unicodedata
from collections import Counter
from decimal import Decimal, InvalidOperation
from difflib import SequenceMatcher

from utils.vercon_suppliers import name_key


def normalized(value):
    text = unicodedata.normalize('NFKD', str(value or '').casefold())
    return ' '.join(re.sub(r'[^a-z0-9]+', ' ', ''.join(
        ch for ch in text if not unicodedata.combining(ch))).split())


def code_key(value):
    # Punctuation and leading zeroes in inventory numbers remain significant.
    return str(value or '').strip().casefold()


def number(value):
    if value is None or str(value).strip() == '':
        return None
    text = str(value).strip().replace('\u00a0', '').replace(' ', '')
    if ',' in text and '.' in text:
        text = text.replace('.', '').replace(',', '.') if text.rfind(',') > text.rfind('.') else text.replace(',', '')
    else:
        text = text.replace(',', '.')
    try:
        result = Decimal(text)
        return result if result.is_finite() else None
    except InvalidOperation:
        return None


def _um(value):
    key = normalized(value)
    return 'buc' if key in ('buc', 'bucata', 'bucati') else key


def _name_score(left, right):
    if not left or not right:
        return 0
    direct = SequenceMatcher(None, left, right, autojunk=False).ratio()
    reordered = SequenceMatcher(None, ' '.join(sorted(left.split())),
                                ' '.join(sorted(right.split())), autojunk=False).ratio()
    return round(100 * max(direct, reordered), 1)


def compare(rows, products, limit=5, min_name_score=40):
    """Compare within one already-resolved context, including inactive nomenclature entries."""
    prepared = [(p, normalized(p['name']), code_key(p['code']), number(p['price'])) for p in products]
    codes = Counter(code_key(r.get('code')) for r in rows if code_key(r.get('code')))
    results = []
    for row in rows:
        source_name = normalized(row.get('name'))
        source_code = code_key(row.get('code'))
        price = number(row.get('price'))
        candidates = []
        notes = []
        if codes[source_code] > 1 and source_code:
            notes.append('Cod repetat în Excel.')
        if price is None or price < 0:
            notes.append('Preț Excel lipsă sau invalid.')
        for product, target_name, target_code, target_price in prepared:
            same_code = bool(source_code and target_code and source_code == target_code)
            similarity = _name_score(source_name, target_name)
            if not same_code and similarity < min_name_score:
                continue
            delta = target_price - price if target_price is not None and price is not None else None
            same_price = delta is not None and abs(delta) <= Decimal('0.01')
            price_score = (max(0, 1 - float(abs(delta) / max(abs(price), abs(target_price), Decimal('0.01'))))
                           if delta is not None else None)
            score = round(similarity * .8 + 20 * price_score, 1) if price_score is not None else similarity
            reasons = ['Cod identic.' if same_code else 'Cod diferit.' if source_code and target_code else 'Cod incomplet.']
            reasons.append('Preț egal la 0,01 lei.' if same_price else 'Preț diferit.' if delta is not None else 'Preț incomplet.')
            same_um = bool(_um(row.get('um')) and _um(row.get('um')) == _um(product['um']))
            if not same_um:
                reasons.append('UM diferită sau incompletă.')
            # Models/dimensions must not disappear into a high textual score.
            digits_left = re.findall(r'\d+', source_name)
            digits_right = re.findall(r'\d+', target_name)
            model_conflict = bool(digits_left and digits_right and sorted(digits_left) != sorted(digits_right))
            if model_conflict:
                reasons.append('Numere/model/dimensiuni diferite în denumire.')
            if not product['active']:
                reasons.append('Produs sau variantă inactivă în VERCON.')
            candidates.append({**product, 'name_score': similarity, 'score': score, 'same_code': same_code,
                               'same_price': same_price, 'same_um': same_um, 'model_conflict': model_conflict,
                               'price_delta': str(delta) if delta is not None else None, 'reason': ' '.join(reasons)})
        candidates.sort(key=lambda p: (not p['same_code'], -p['score'], -p['name_score'],
                                       name_key(p['name']), number(p['price']) or Decimal(0), name_key(p['code'])))
        status = 'Fără candidat convingător'
        if candidates:
            best = candidates[0]
            ambiguous = len(candidates) > 1 and (
                (best['same_code'] and candidates[1]['same_code']) or
                (best['same_code'] == candidates[1]['same_code'] and best['score'] - candidates[1]['score'] < 5))
            conflicting_code = bool(source_code and code_key(best['code']) and not best['same_code'])
            if ambiguous or (source_code and codes[source_code] > 1):
                status = 'Ambiguu — de revizuit'
            elif (price is not None and price >= 0 and best['name_score'] >= 90 and best['same_price'] and best['same_um'] and
                  best['active'] and not best['model_conflict'] and not conflicting_code):
                status = 'Concordanță puternică — de revizuit'
            elif best['same_code']:
                status = 'Cod identic — de revizuit'
            elif best['name_score'] >= 60:
                status = 'Denumire similară — de revizuit'
            else:
                status = 'Candidat slab — de revizuit'
        results.append({'row': row['row'], 'status': status, 'note': ' '.join(notes),
                        'candidate_count': len(candidates), 'candidates': candidates[:limit]})
    # Two source rows must not silently consume the same existing variant.
    reused = Counter(r['candidates'][0]['variant_id'] for r in results if r['candidates'])
    for result in results:
        if result['candidates'] and reused[result['candidates'][0]['variant_id']] > 1:
            result['status'] = 'Ambiguu — de revizuit'
            result['note'] = (result['note'] + ' Același candidat principal pentru mai multe rânduri Excel.').strip()
    return results
