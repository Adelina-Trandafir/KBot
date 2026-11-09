"""Product routes and current locations, derived from the scoped document ledgers."""
from collections import defaultdict
from datetime import date, datetime, timezone
import re
from zoneinfo import ZoneInfo, ZoneInfoNotFoundError

from . import bu_links, stock, stock_source
from .documents import _one
from .nomenclatoare import NomError


def _labels(cursor, ctx):
    scope = (ctx['subunit'], ctx['gestiune'])
    cursor.execute('SELECT IDC,Denumire FROM VC_Clienti WHERE SubunitId=%s AND GestiuneId=%s', scope)
    holders = {row['IDC']: row['Denumire'] or str(row['IDC']) for row in cursor.fetchall()}
    cursor.execute('SELECT IdMasina,Masina FROM VC_Masini WHERE SubunitId=%s AND GestiuneId=%s', scope)
    accounts = {row['IdMasina']: row['Masina'] or str(row['IdMasina']) for row in cursor.fetchall()}
    return holders, accounts


def _routes(cursor, ctx, warehouse, custody, gestiune_name, holders, accounts):
    scope = (ctx['subunit'], ctx['gestiune'])
    cursor.execute('SELECT DISTINCT IdBonConsum FROM VC_OI_Predare '
                   'WHERE SubunitId=%s AND GestiuneId=%s AND ExercitiuId=%s AND Anulat=0',
                   (*scope, ctx['exercitiu']))
    linked = {row['IdBonConsum'] for row in cursor.fetchall()}
    cursor.execute('SELECT IdIntrare,Numar,NumarSub FROM VC_NIR '
                   'WHERE SubunitId=%s AND GestiuneId=%s AND ExercitiuId=%s AND Anulat=0',
                   (*scope, ctx['exercitiu']))
    receipts = {row['IdIntrare']: str(row['Numar'] or '') + (
        ',' + ('%02d' % int(row['NumarSub'])).rstrip('0') if row['NumarSub'] else '') for row in cursor.fetchall()}
    routes = defaultdict(list)
    for row in warehouse:
        if row['kind'] == 'bc' and row['document_id'] in linked:
            continue  # The BP below is the same warehouse withdrawal, displayed once.
        incoming = row['q'] >= 0
        routes[row['p']].append({'date': stock.day_of(row['d']).isoformat(), 'kind': row['kind'],
                               'number': receipts.get(row['document_id'], row['number']) if row['kind'] in ('nir', 'storno') else row['number'],
                               'document_id': row['document_id'], 'line_id': row['line_id'],
                               'from': 'Exterior' if incoming else 'Magazie',
                               'to': ('Magazie' if incoming else 'Transferat în afara subunității'
                                      if bu_links.is_bu(row['number']) else 'Exterior'), 'account': '',
                               'quantity': format(abs(row['q']), 'f'), 'gestiune': gestiune_name})
    grouped = defaultdict(dict)
    for row in custody:
        family = 'bt' if row['kind'].startswith('bt_') else row['kind']
        grouped[(row['p'], family, row['document_id'], row['line_id'])][row['kind']] = row
    for (variant, family, document, line), events in grouped.items():
        row = next(iter(events.values()))
        source = events.get('bt_out', events.get('bs'))
        destination = events.get('bt_in', events.get('bp'))
        from_name = holders.get(source['holder'], str(source['holder'])) if source else 'Magazie'
        to_name = (holders.get(destination['holder'], str(destination['holder'])) if destination else
                   'Transferat în afara subunității' if bu_links.is_bu(row['number']) else 'Casare')
        quantity = destination['q'] if destination else -source['q']
        if quantity < 0:
            from_name, to_name = to_name, from_name
        routes[variant].append({'date': stock.day_of(row['d']).isoformat(), 'kind': family,
                                'number': row['number'], 'document_id': document, 'line_id': line,
                                'from': from_name, 'to': to_name,
                                'account': accounts.get(row['category'], str(row['category'] or '')),
                                'quantity': format(abs(row['q']), 'f'), 'gestiune': gestiune_name})
    return routes


def attach(cursor, ctx, variants, warehouse, gestiune_name):
    ids = {variant['id'] for variant in variants}
    if not ids:
        return
    custody = [row for row in stock_source.custody(cursor, ctx) if row['p'] in ids]
    holders, accounts = _labels(cursor, ctx)
    routes = _routes(cursor, ctx, warehouse, custody, gestiune_name, holders, accounts)
    try:
        today = datetime.now(ZoneInfo('Europe/Bucharest')).date()
    except ZoneInfoNotFoundError:
        today = datetime.now(timezone.utc).date()
    warehouse_balances, _ = stock.balance(warehouse, date.min, today)
    held, _ = stock.balance(custody, date.min, today, ('p', 'holder', 'category'))
    for variant in variants:
        key = variant['id']
        locations = []
        remaining = warehouse_balances.get((key,), {}).get('closing', 0)
        if remaining:
            locations.append({'holder': 'Magazie', 'account': '', 'quantity': format(remaining, 'f'), 'gestiune': gestiune_name})
        for (product, holder, account), balance in held.items():
            if product == key and balance['closing']:
                locations.append({'holder': holders.get(holder, str(holder)), 'account': accounts.get(account, str(account or '')),
                                  'quantity': format(balance['closing'], 'f'), 'gestiune': gestiune_name})
        variant.update(movements=sorted(routes[key], key=lambda row: (row['date'], row['document_id'], row['line_id']), reverse=True),
                       locations=locations, location_date=today.isoformat())


def report_history(cursor, ctx, variant, start, end, gestiune_name):
    product = _one(cursor, 'SELECT v.COD AS code,v.PUTVA AS price,p.DenumireP AS name,p.UM AS um '
                   'FROM VC_ProduseP v JOIN VC_Produse p '
                   'ON p.SubunitId=v.SubunitId AND p.GestiuneId=v.GestiuneId AND p.ID=v.ID '
                   'WHERE v.SubunitId=%s AND v.GestiuneId=%s AND v.IdProdus=%s',
                   (ctx['subunit'], ctx['gestiune'], variant))
    if not product:
        raise NomError('NOT_FOUND', 'Produsul nu mai există în gestiunea raportului.', 404)
    warehouse = [
        row for row in stock_source.warehouse(cursor, ctx, {variant}) if start <= stock.day_of(row['d']) <= end]
    custody = [row for row in stock_source.custody(cursor, ctx)
               if row['p'] == variant and start <= stock.day_of(row['d']) <= end]
    holders, accounts = _labels(cursor, ctx)
    routes = _routes(cursor, ctx, warehouse, custody, gestiune_name, holders, accounts)

    def order(row):
        number = tuple((1, int(part)) if part.isdecimal() else (0, part.casefold())
                       for part in re.split(r'(\d+)', str(row['number'] or '')))
        return row['date'], number

    return {'product': {**product, 'price': None if product['price'] is None else format(product['price'], 'f')},
            'zec': ctx['zec'], 'movements': sorted(routes[variant], key=order, reverse=True)}
