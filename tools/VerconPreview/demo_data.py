"""Invented food lists kept in memory; no database or production configuration."""
from copy import deepcopy
from datetime import date
from decimal import Decimal, InvalidOperation, ROUND_HALF_UP

from routes.vercon.nomenclatoare import NomError

YEAR = 2026
CTX = {'subunit': 1, 'gestiune': 1, 'exercitiu': 1, 'year': YEAR,
       'an': YEAR, 'type': 'ALIMENTE', 'zec': 4, 'scale': 4}
SELECTION = {key: CTX[key] for key in ('subunit', 'gestiune', 'year', 'type')}
MEALS = (('md', 'MicDejun', 'VMD'), ('d', 'Dejun', 'VD'), ('c', 'Cina', 'VC'))
DISHES = [{'id': 1, 'label': 'Felul I'}, {'id': 2, 'label': 'Felul II'},
          {'id': 3, 'label': 'Desert'}, {'id': 4, 'label': 'Gustare'}]
PRODUCTS = [('Pâine integrală', 'kg', '28.8', '6.5'), ('Lapte', 'l', '72', '7.8'),
            ('Unt', 'kg', '2.88', '45'), ('Carne de pui', 'kg', '28.8', '28'),
            ('Orez', 'kg', '14.4', '10.2'), ('Legume proaspete', 'kg', '57.6', '8.42')]


def amount(value):
    return format(Decimal(str(value)).quantize(Decimal('.0001'), rounding=ROUND_HALF_UP), 'f')


def fail(message, status=422, reason='DEMO_INPUT'):
    raise NomError(reason, message, status)


class DemoData:
    def __init__(self):
        self.documents = {}
        self.bons = {}
        self.requests = {}
        for index, day in enumerate((7, 8, 9)):
            doc_id = 129 + index
            dishes = [('Lapte cu pâine și unt',), ('Supă de legume', 'Pui cu orez'), ('Iaurt cu biscuiți',)]
            meals = {key: {'P': 144, 'VMD': '18.0000' if key == 'md' else '0.0000',
                           'V': '2592.0000' if key == 'md' else '0.0000', 'D': 0,
                           'items': [{'id': at + 1, 'IdFel': at + 1, 'fel': DISHES[at]['label'],
                                      'Tip': DISHES[at]['label'], 'Explicatie': text}
                                     for at, text in enumerate(dishes[position])]}
                     for position, (key, _, _) in enumerate(MEALS)}
            self.bons[doc_id] = {'id': doc_id, 'number': 510 + index,
                                 'date': f'{YEAR}-10-{day:02}', 'receiver': 'Bucătărie DEMO',
                                 'total': self.food_total(), 'annulled': False}
            self.documents[doc_id] = {'family': 'lista', 'id': doc_id, 'version': 1, 'number': doc_id,
                'annulled': False, 'reason': None, 'annulled_by': None,
                'header': {'Data': f'{YEAR}-10-{day:02}', 'Comentarii': 'Date demonstrative locale',
                           'Valoare': '2592.0000', 'bons_total': self.food_total(),
                           'diff': amount(Decimal(2592) - Decimal(self.food_total()))},
                'meals': meals, 'bons': [deepcopy(self.bons[doc_id])]}
        self.bons[999] = {'id': 999, 'number': 600, 'date': f'{YEAR}-10-17',
                          'receiver': 'Bucătărie DEMO', 'total': self.food_total(), 'annulled': False}

    def food_total(self):
        return amount(sum((Decimal(qty) * Decimal(price) for _, _, qty, price in PRODUCTS), Decimal(0)))

    def get(self, doc_id):
        if doc_id not in self.documents:
            fail('Lista demonstrativă nu există.', 404, 'NOT_FOUND')
        return deepcopy(self.documents[doc_id])

    def listing(self):
        return [{'id': doc['id'], 'number': doc['number'], 'date': doc['header']['Data'],
                 'total': doc['header']['Valoare'], 'bons_total': doc['header']['bons_total'],
                 'diff': doc['header']['diff'], 'bons': len(doc['bons']),
                 'annulled': doc['annulled'], 'version': doc['version']}
                for doc in sorted(self.documents.values(), key=lambda d: (d['header']['Data'], d['id']), reverse=True)]

    def eligible(self, exclude=None):
        used = {bon['id'] for doc in self.documents.values()
                if not doc['annulled'] and doc['id'] != exclude for bon in doc['bons']}
        return [deepcopy(bon) for key, bon in self.bons.items() if key not in used]

    def save(self, body, doc_id=None):
        if not isinstance(body, dict):
            fail('Date demonstrative invalide.')
        request_key = body.get('request_key')
        if doc_id is None and request_key and request_key in self.requests:
            return self.get(self.requests[request_key])
        old = self.get(doc_id) if doc_id is not None else None
        if old and (old['annulled'] or body.get('version') != old['version']):
            fail('Lista a fost schimbată. Redeschideți lista.', 409, 'CONFLICT')
        header, meals = body.get('header', {}), body.get('meals', {})
        try:
            day = date.fromisoformat(header['Data'])
            if day.year != YEAR:
                fail(f'Alegeți o dată în anul {YEAR}.')
            number = int(header.get('Numar') or (old['number'] if old else max(self.documents) + 1))
            if number <= 0 or any(d['number'] == number and d['id'] != doc_id for d in self.documents.values()):
                fail('Numărul listei trebuie să fie pozitiv și unic.')
            cleaned = {}
            for key, _, _ in MEALS:
                raw = meals[key]
                portions, distributed = int(str(raw['P'])), int(str(raw['D']))
                price = Decimal(str(raw['VMD']))
                if portions < 0 or distributed < 0 or not price.is_finite() or price < 0:
                    fail('Porțiile și valorile trebuie să fie pozitive sau zero.')
                items = [{**item, 'id': index + 1, 'fel': item.get('Tip') or ''}
                         for index, item in enumerate(raw.get('items', [])) if item.get('Explicatie')]
                cleaned[key] = {'P': portions, 'D': distributed, 'VMD': amount(price),
                                'V': amount(portions * price), 'items': items}
            bon_ids = [int(value) for value in body.get('bons', [])]
            eligible = {bon['id'] for bon in self.eligible(doc_id)}
            if len(set(bon_ids)) != len(bon_ids) or not set(bon_ids) <= eligible:
                fail('Un bon demonstrativ nu mai este disponibil.', 409, 'BON_INVALID')
        except (ValueError, TypeError, KeyError, InvalidOperation):
            fail('Completați data, porțiile și valorile cu date valide.')
        doc_id = doc_id if old else max(self.documents) + 1
        total = sum((Decimal(meal['V']) for meal in cleaned.values()), Decimal(0))
        bons = [deepcopy(self.bons[key]) for key in bon_ids]
        bon_total = sum((Decimal(bon['total']) for bon in bons), Decimal(0))
        self.documents[doc_id] = {'family': 'lista', 'id': doc_id, 'number': number,
            'version': old['version'] + 1 if old else 1, 'annulled': False, 'reason': None,
            'annulled_by': None, 'meals': cleaned, 'bons': bons,
            'header': {'Data': day.isoformat(), 'Comentarii': header.get('Comentarii') or '',
                       'Valoare': amount(total), 'bons_total': amount(bon_total), 'diff': amount(total - bon_total)}}
        if request_key:
            self.requests[request_key] = doc_id
        return self.get(doc_id)


class DemoSource:
    """The production food-report Source interface, supplied entirely by fixtures."""
    def __init__(self, data, params):
        self.data, self.params, self.ctx = data, params, dict(CTX)
        self.versions = {'demo': 'in-memory'}
        self.doc = data.get(params['document'])

    def metadata(self):
        return {index: {'variant': index, 'product': index, 'name': name, 'um': um,
                        'code': f'DEMO{index:03}', 'category': 1, 'account': '302.7'}
                for index, (name, um, _, _) in enumerate(PRODUCTS, 1)}

    def rows(self, name):
        doc = self.doc
        if name == 'food_header':
            head = {'IdLista': doc['id'], 'NumarLA': doc['number'], 'DATALA': date.fromisoformat(doc['header']['Data']),
                    'Anulat': doc['annulled'], 'DEP': doc['header']['diff'],
                    'Comentarii': doc['header']['Comentarii']}
            for key, prefix, price in MEALS:
                meal = doc['meals'][key]
                head.update({prefix + '_P': meal['P'], price: meal['VMD'],
                             prefix + '_V': meal['V'], prefix + '_D': meal['D']})
            return [head]
        if name == 'food_items':
            return [{**item, 'meal': key} for key, meal in doc['meals'].items() for item in meal['items']]
        if name == 'food_consumption':
            count = len(doc['bons'])
            return [{'line_id': index, 'variant': index, 'quantity': amount(Decimal(qty) * count),
                     'price': amount(price), 'value': amount(Decimal(qty) * Decimal(price) * count)}
                    for index, (_, _, qty, price) in enumerate(PRODUCTS, 1)] if count else []
        if name == 'food_bons':
            return [{**bon, 'document': bon['id'], 'value': bon['total']} for bon in doc['bons']]
        if name == 'food_history':
            return [{'IdLista': row['id'], 'date': date.fromisoformat(row['date']), 'difference': row['diff']}
                    for row in self.data.listing() if not row['annulled']]
        if name == 'settings':
            return [{'SettingKey': 'SI_LA', 'IntValue': '3199.6973'}]
        raise KeyError('Sursă demonstrativă necunoscută: ' + name)
