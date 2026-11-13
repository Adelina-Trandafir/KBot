"""Bank rows are shared by the DC and explicitly assigned to scoped ADE children.

The caller owns AD_Lock(0), the subunit lock and the transaction. No DDL.
AD_EXTRASE_ASOCIERI reserves each IDFXE across ALL subunits of the current DC.
"""
import hashlib
import json
from datetime import date
from decimal import Decimal, InvalidOperation

from .domain import DomainError, require
from . import service, bank_numbers
from .bank_targets import reviewed_target, ensure_payment_row


SOURCE_FIELDS = ('IDFXE IDFXH IDOP CodAI DataBanca DataDoc NrDoc Referinta ReferintaDest '
                 'platitor_nume platitor_cui platitor_iban suma_debit suma_credit Explicatii').split()
SOURCE_SELECT = ','.join('e.`' + field + '`' for field in SOURCE_FIELDS)
CNP_CLAUSE = "e.platitor_cui REGEXP '^[0-9]{13}$'"


def fingerprint(row):
    return hashlib.sha256(json.dumps({key: row.get(key) for key in SOURCE_FIELDS},
                                    sort_keys=True, ensure_ascii=False, default=str).encode()).hexdigest()


def schema_error(error):
    if getattr(error, 'errno', None) in (1146, 1054):
        raise DomainError('BANK_SCHEMA_REQUIRED',
                          'Asocierea extraselor necesită instalarea manuală a SQL-ului AD_11_extrase_asocieri.sql '
                          'în baza unității.', 409) from error
    raise error


def date_filter(value):
    if not value:
        return None
    try:
        return date.fromisoformat(value).isoformat()
    except (ValueError, TypeError) as error:
        raise DomainError('DATE', 'Intervalul extraselor conține o dată invalidă.', 400) from error


def available(repo, args):
    start, end = date_filter(args.get('start')), date_filter(args.get('end'))
    require(not start or not end or start <= end, 'DATE', 'Data de început depășește data de sfârșit.', 400)
    cursor = args.get('before', '')
    require(not cursor or str(cursor).isdigit(), 'FILTER', 'Pagina extraselor este invalidă.', 400)
    search = (args.get('search') or '').strip()
    require(len(search) <= 255, 'FILTER', 'Textul căutat este prea lung.', 400)
    # SLICE-AD21-01: a movement stays listed while its associated payments do not cover it (after a
    # single allocation was deleted the difference comes back as "unassociated").
    # SLICE-AD21-02: only payers identified by a personal number (CNP, exactly 13 digits).
    clauses = ['COALESCE(s.asociat,0)<e.suma_credit', 'e.suma_credit>0', 'COALESCE(e.suma_debit,0)=0',
               CNP_CLAUSE]
    params = []
    period = args.get('period', '')
    if period:
        try:
            first = date.fromisoformat(period + '-01')
        except (ValueError, TypeError) as error:
            raise DomainError('FILTER', 'Luna extraselor este invalidă.', 400) from error
        following = date(first.year + (first.month == 12), first.month % 12 + 1, 1)
        clauses.extend(['f.DataExtras>=%s', 'f.DataExtras<%s'])
        params.extend([first.isoformat(), following.isoformat()])
    for clause, value in [('f.DataExtras>=%s', start), ('DATE(f.DataExtras)<=%s', end), ('e.IDFXE<%s', int(cursor) if cursor else None)]:
        if value is not None:
            clauses.append(clause)
            params.append(value)
    if len(search) >= 2:
        # The bank importer stores the segment before the first '-' as Referinta.
        # Exclude that leading reference from searches, keeping the source itself intact.
        explanation = ("CASE WHEN LOCATE('-',e.Explicatii)>0 "
                       "AND TRIM(SUBSTRING_INDEX(e.Explicatii,'-',1))=TRIM(e.Referinta) "
                       "THEN SUBSTRING(e.Explicatii,LOCATE('-',e.Explicatii)+1) "
                       "ELSE e.Explicatii END")
        # Literal, case-insensitive substring: 'gr' finds both 'Gr40' and 'Grădinița 40'.
        clauses.append(f"LOWER({explanation}) LIKE %s ESCAPE '='")
        params.append('%' + search.lower().replace('=', '==').replace('%', '=%').replace('_', '=_') + '%')
    try:
        rows = repo.query('SELECT ' + SOURCE_SELECT + ', DATE(f.DataExtras) AS DataExtras, '
                          'COALESCE(s.asociat,0) AS suma_asociata, e.suma_credit-COALESCE(s.asociat,0) AS suma_ramasa '
                          'FROM FX_Extrase e '
                          'LEFT JOIN FX_Extrase_H h ON h.IDEXH=e.IDFXH '
                          'LEFT JOIN FX_Extrase_F f ON f.IDEXF=h.IDEXF '
                          'LEFT JOIN (SELECT a.IDFXE, SUM(COALESCE(p.Plata,0)) AS asociat FROM AD_EXTRASE_ASOCIERI a '
                          'LEFT JOIN AD_Plati p ON p.IDPL=a.IDPL AND p.SubunitId=a.SUBUNIT_ID GROUP BY a.IDFXE) s '
                          'ON s.IDFXE=e.IDFXE WHERE '
                          + ' AND '.join(clauses) + ' ORDER BY e.IDFXE DESC LIMIT 151', tuple(params))
        # AD_07 removed FelDoc; the requested correspondence restores it via manual SQL.
        repo.query('SELECT FelDoc FROM AD_AlteDoc LIMIT 0')
    except Exception as error:
        schema_error(error)
    return {'rows': [{**row, 'fingerprint': fingerprint(row)} for row in rows[:150]],
            'next': rows[149]['IDFXE'] if len(rows) > 150 else None}


def available_periods(repo):
    rows = repo.query('SELECT DISTINCT YEAR(f.DataExtras) AS year, MONTH(f.DataExtras) AS month '
                     'FROM FX_Extrase e JOIN FX_Extrase_H h ON h.IDEXH=e.IDFXH '
                     'JOIN FX_Extrase_F f ON f.IDEXF=h.IDEXF '
                     'WHERE f.DataExtras IS NOT NULL AND YEAR(f.DataExtras)>0 AND MONTH(f.DataExtras) BETWEEN 1 AND 12 '
                     'AND e.suma_credit>0 AND COALESCE(e.suma_debit,0)=0 AND ' + CNP_CLAUSE + ' '
                     'ORDER BY year DESC, month DESC')
    return {'periods': [{'year': int(row['year']), 'month': int(row['month']),
                         'value': f"{row['year']:04d}-{row['month']:02d}"} for row in rows]}


def payer_history(repo, child_id):
    """Scoped payer history with a display-only bank derived as in K-BOT Parteneri."""
    from utils.database import get_kbot_comun_connection

    repo.get('Platitori', child_id)
    rows = repo.rows('Platitori_sub', IDP=child_id)
    bic = {}
    if any(row.get('Cont') for row in rows):
        connection = get_kbot_comun_connection()
        try:
            cursor = connection.cursor(dictionary=True, buffered=True)
            try:
                cursor.execute('SELECT Cod, Banca FROM BIC')
                bic = {str(row['Cod'] or '').strip().upper(): str(row['Banca'] or '').strip()
                       for row in cursor.fetchall() if row['Cod']}
            finally:
                cursor.close()
        finally:
            connection.close()
    for row in rows:
        # Match Parteneri's _bank_from_iban: remove whitespace, uppercase, take characters 5–8.
        iban = ''.join(str(row.get('Cont') or '').split()).upper()
        row['BancaCont'] = bic.get(iban[4:8], '') if len(iban) >= 8 else ''
    return rows


def amount_of(source):
    try:
        credit = Decimal(str(source.get('suma_credit') or 0))
        debit = Decimal(str(source.get('suma_debit') or 0))
    except InvalidOperation as error:
        raise DomainError('AMOUNT', 'Suma extrasului este invalidă.', 400) from error
    require(credit.is_finite() and debit.is_finite() and credit > 0 and debit == 0,
            'AMOUNT', 'Selectați o încasare cu sumă credit pozitivă și debit zero.', 400)
    # AD_Plati.Plata and existing calculations use Access Long. Never silently round bank money.
    require(credit == credit.to_integral_value() and credit <= 2147483647,
            'AMOUNT_PRECISION', 'Suma extrasului nu poate fi păstrată exact în câmpul Plata '
            'actual (număr întreg). Asocierea nu a fost salvată.', 400)
    return int(credit)


def default_explanation(repo, month_id):
    month = repo.get('LunaD', month_id)
    config = repo.receipt_config()
    configured = service.receipt_explanation(config, month) if config and config.get('Explicatie') else ''
    return configured.strip() or f"Contravaloare hrană pentru {service.MONTHS[month['Luna'] - 1]} {month['Anul']}"


def editable_source(source, body):
    """Corrections affect the ADE document / payer only; the K-BOT bank source is immutable."""
    fields = {'name': 'platitor_nume', 'cui': 'platitor_cui', 'iban': 'platitor_iban',
              'date': 'DataDoc', 'number': 'NrDoc', 'amount': 'suma_credit'}
    changes = body.get('values', {})
    require(isinstance(changes, dict) and set(changes) <= set(fields), 'FIELDS', 'Date de asociere invalide.', 400)
    result = dict(source)
    for key, value in changes.items():
        if key == 'amount':
            require(type(value) in (int, float), 'AMOUNT', 'Suma este invalidă.', 400)
        else:
            require(isinstance(value, str) and len(value) <= 255, 'TEXT', 'Text prea lung sau invalid.', 400)
            value = value.strip()
        result[fields[key]] = value
    return result


def associate(repo, db_name, body, email):
    require(type(body.get('extract_id')) is int and body['extract_id'] > 0,
            'REQUEST', 'Selecție incompletă sau invalidă.', 400)
    try:
        # The movement row is locked first: it serializes concurrent associations of the same movement.
        sources = repo.query('SELECT ' + SOURCE_SELECT + ' FROM FX_Extrase e WHERE e.IDFXE=%s FOR UPDATE',
                             (body['extract_id'],))
        assigned = repo.query('SELECT a.IDP, COALESCE(p.Plata,0) AS Plata FROM AD_EXTRASE_ASOCIERI a '
                              'LEFT JOIN AD_Plati p ON p.IDPL=a.IDPL AND p.SubunitId=a.SUBUNIT_ID '
                              'WHERE a.IDFXE=%s', (body['extract_id'],))
        repo.query('SELECT FelDoc FROM AD_AlteDoc LIMIT 0')
    except DomainError:
        raise
    except Exception as error:
        schema_error(error)
    require(bool(sources), 'NOT_FOUND', 'Rândul de extras nu mai există.', 404)
    original = sources[0]
    require(Decimal(str(original.get('suma_credit') or 0)) > 0
            and Decimal(str(original.get('suma_debit') or 0)) == 0, 'AMOUNT',
            'Rândul de extras nu reprezintă o încasare.', 400)
    require(body.get('fingerprint') == fingerprint(original), 'BANK_CHANGED',
            'Datele extrasului s-au schimbat. Reîncărcați lista înainte de asociere.', 409)
    if 'allocations' in body:
        return associate_allocations(repo, db_name, original, body, email, assigned)
    require(not assigned, 'BANK_ALREADY_ASSIGNED',
            'Extrasul este deja asociat, în întregime sau parțial. Reîncărcați lista; restul se asociază manual, cu repartizare.', 409)
    require(type(body.get('payer_id')) is int and body['payer_id'] > 0,
            'REQUEST', 'Alegeți plătitorul activ al copilului.', 400)
    source = editable_source(original, body)
    return write_association(repo, db_name, original, source, body, reviewed_target(repo, body), email)


def associate_allocations(repo, db_name, original, body, email, assigned=()):
    """Save the complete distribution atomically under the caller's unit/subunit locks.

    SLICE-AD21-01: `assigned` are the allocations already saved for this movement (after one was deleted the
    movement is partly associated). The new distribution must cover exactly what is left."""
    allocations = body.get('allocations')
    require(isinstance(allocations, list) and 1 <= len(allocations) <= 50
            and all(isinstance(item, dict) for item in allocations),
            'ALLOCATION', 'Adăugați între 1 și 50 de copii la repartizare.', 400)
    require(not body.get('values'), 'ALLOCATION', 'Datele extrasului repartizat nu se modifică.', 400)
    left = amount_of(original) - sum(int(row['Plata'] or 0) for row in assigned)
    require(left > 0, 'BANK_ALREADY_ASSIGNED',
            'Extrasul a fost deja asociat în întregime în această unitate. Reîncărcați lista.', 409)
    taken = {row['IDP'] for row in assigned}
    # Keep single-child requests compatible with AD_11; several rows for one movement require manual AD_15.
    if len(allocations) > 1 or assigned:
        primary = repo.query("SHOW INDEX FROM AD_EXTRASE_ASOCIERI WHERE Key_name='PRIMARY'")
        require({row['Column_name'] for row in primary} == {'IDFXE', 'IDP'},
                'BANK_SPLIT_SCHEMA_REQUIRED',
                'Repartizarea necesită instalarea manuală a SQL-ului AD_15_extrase_repartizare.sql în baza unității.', 409)
    reviewed = []
    children = set()
    months = set()
    total = 0
    for item in allocations:
        require(type(item.get('amount')) is int and 0 < item['amount'] <= 2147483647,
                'ALLOCATION_AMOUNT', 'Suma fiecărui copil trebuie să fie pozitivă, în lei întregi.', 400)
        require(type(item.get('payer_id')) is int and item['payer_id'] > 0,
                'REQUEST', 'Alegeți plătitorul activ pentru fiecare copil.', 400)
        target = reviewed_target(repo, item)
        child, month, _, _ = target
        require(child['IDP'] not in children, 'DUPLICATE', 'Același copil apare de două ori în repartizare.', 400)
        require(child['IDP'] not in taken, 'DUPLICATE',
                f"Copilul {child['Nume']} are deja o repartizare din acest extras. Ștergeți-o din Asocieri curente înainte de a o reface.", 409)
        children.add(child['IDP'])
        months.add(month['IDL'])
        total += item['amount']
        reviewed.append((item, target))
    require(len(months) == 1, 'ALLOCATION_MONTH', 'Alegeți aceeași lună pentru întreaga repartizare.', 400)
    require(total == left, 'ALLOCATION_TOTAL',
            'Totalul sumelor repartizate trebuie să fie egal cu suma rămasă de asociat din extras.', 400)
    numbers = bank_numbers.allocation_numbers(repo, allocations, body.get('number_year'))
    saved = []
    # All target versions are checked before payer renewal can update related portal identities.
    for (item, (child, month, previous, attendance)), number in zip(reviewed, numbers):
        previous = repo.get('Platitori_sub', previous['IDS'])
        require(bool(previous.get('Activ')), 'CONFLICT', 'Plătitorul s-a modificat. Reîncărcați selecția.', 409)
        source = {**original, 'suma_credit': item['amount']}
        saved.append(write_association(repo, db_name, original, source, {**item, 'document_number': number},
                                       (child, month, previous, attendance), email))
    return {'extract_id': original['IDFXE'], 'rows': saved, 'count': len(saved)}


def write_association(repo, db_name, original, source, body, target, email):
    child, month, previous, attendance = target
    service.temporal(repo, month['IDL'])
    day = service.document_date(str(source.get('DataDoc') or '')[:10], month)
    amount = amount_of(source)
    explanation = body.get('explanation')
    require(explanation is None or isinstance(explanation, str), 'EXPLANATION', 'Explicație invalidă.', 400)
    explanation = (explanation or '').strip() or default_explanation(repo, month['IDL'])
    require(len(explanation) <= 255, 'EXPLANATION', 'Explicația poate avea maximum 255 de caractere.', 400)
    name = str(source.get('platitor_nume') or '').strip()
    number = str(body.get('document_number') if 'document_number' in body else source.get('NrDoc') or '').strip()
    require(bool(name) and bool(number), 'BANK_DOCUMENT', 'Extrasul trebuie să aibă nume plătitor și număr document.', 400)
    attendance = ensure_payment_row(repo, child, month, attendance)
    # Preserve the selected parent's contacts / portal identity, not a guessed identity from CUI.
    values = {key: previous.get(key) for key in
              ('Adresa', 'CNP_Platitor', 'Banca', 'Telefon', 'EMail', 'TrimiteMail')}
    values.update(IDP=child['IDP'], Nume=name, CUI=source.get('platitor_cui'), Cont=source.get('platitor_iban'), Activ=True)
    payer = service.save_catalog(repo, 'Platitori_sub', {'id': None, 'values': values})
    document = service.emit_document(repo, db_name, {
        'kind': 'other', 'attendance_id': attendance['IDZ'], 'payer_id': payer['IDS'],
        'date': day, 'amount': amount, 'number': number,
        'explanation': explanation,
    })
    document = repo.update('AlteDoc', document, {'FelDoc': source.get('Referinta')})
    repo.execute('INSERT INTO AD_EXTRASE_ASOCIERI '
                 '(IDFXE,SUBUNIT_ID,IDP,IDS_ANTERIOR,IDS,IDZ,IDL,IDPL,IDA,SURSA_HASH,ASOCIAT_DE) '
                 'VALUES (%s,%s,%s,%s,%s,%s,%s,%s,%s,%s,%s)',
                 (source['IDFXE'], repo.scope(), child['IDP'], previous['IDS'], payer['IDS'],
                  attendance['IDZ'], month['IDL'], document['IDPL'], document['IDA'], fingerprint(original), email))
    return {'extract_id': source['IDFXE'], 'payer': payer, 'document': document, 'subunit_id': repo.scope()}


# ---- SLICE-AD21-01: current associations (list, per-child tab, deletion) -----------------------------------

ASSOCIATION_SELECT = (
    'SELECT a.IDFXE, a.IDP, a.IDZ, a.IDL, a.IDPL, a.IDA, a.ASOCIAT_DE, a.ASOCIAT_LA, '
    'c.Nume AS copil, p.Plata AS suma_asociata, p.Anulata AS plata_anulata, '
    'd.NrDoc, d.DataDoc, d.Anulata AS document_anulat, d.Version AS document_version, '
    'e.NrDoc AS NrExtras, e.platitor_nume, e.suma_credit, DATE(f.DataExtras) AS DataExtras '
    'FROM AD_EXTRASE_ASOCIERI a '
    'JOIN AD_Platitori c ON c.IDP=a.IDP AND c.SubunitId=a.SUBUNIT_ID '
    'JOIN AD_Plati p ON p.IDPL=a.IDPL AND p.SubunitId=a.SUBUNIT_ID '
    'JOIN AD_AlteDoc d ON d.IDA=a.IDA AND d.SubunitId=a.SUBUNIT_ID '
    'JOIN FX_Extrase e ON e.IDFXE=a.IDFXE '
    'LEFT JOIN FX_Extrase_H h ON h.IDEXH=e.IDFXH '
    'LEFT JOIN FX_Extrase_F f ON f.IDEXF=h.IDEXF ')


def association_rows(repo, clause, params, deletable):
    """Associations of the CURRENT subunit only; `Cancelled` mirrors the ledger tabs."""
    try:
        rows = repo.query(ASSOCIATION_SELECT + 'WHERE a.SUBUNIT_ID=%s AND ' + clause
                          + ' ORDER BY f.DataExtras, e.IDFXE, c.Nume', (repo.scope(), *params))
    except Exception as error:
        schema_error(error)
    for row in rows:
        row['Cancelled'] = bool(row.get('plata_anulata') or row.get('document_anulat'))
        row['deletable'] = deletable
    return rows


def current(repo, args):
    """The associations recorded in one month, with the right to delete them only in an open month."""
    reference = args.get('IDL', '')
    require(str(reference).isdigit(), 'FILTER', 'Alegeți luna asocierilor.', 400)
    month = repo.get('LunaD', int(reference))
    closed = bool(month['Inchisa'])
    return {'rows': association_rows(repo, 'a.IDL=%s', (month['IDL'],), not closed),
            'month_id': month['IDL'], 'month_closed': closed}


def attendance_associations(repo, attendance_id, month_id):
    """The "Extrase" tab of the ledger: movements associated to one child's monthly record (read only)."""
    attendance = repo.get('Prezenta', attendance_id)
    require(attendance['IDL'] == month_id, 'CONTEXT_CHANGED', 'Prezența nu aparține lunii selectate.', 409)
    return association_rows(repo, 'a.IDZ=%s', (attendance_id,), False)


def delete_association(repo, body):
    """Delete ONE allocation completely: the link, the Alte plati document and its payment.

    Unlike cancelling, nothing is kept, so the movement (or the difference) can be associated again and the
    document number is free. Only a document of the open month can be deleted: no saved situation is touched.
    The payer created from the movement is kept (as in AD20-15)."""
    require(type(body.get('extract_id')) is int and body['extract_id'] > 0
            and type(body.get('child_id')) is int and body['child_id'] > 0,
            'REQUEST', 'Alegeți asocierea de șters.', 400)
    try:
        links = repo.query('SELECT * FROM AD_EXTRASE_ASOCIERI WHERE IDFXE=%s AND SUBUNIT_ID=%s AND IDP=%s FOR UPDATE',
                           (body['extract_id'], repo.scope(), body['child_id']))
    except Exception as error:
        schema_error(error)
    require(bool(links), 'NOT_FOUND', 'Asocierea nu mai există. Reîncărcați lista.', 404)
    link = links[0]
    month = repo.get('LunaD', link['IDL'])
    require(not month['Inchisa'], 'ASSOCIATION_MONTH_CLOSED',
            'Asocierea poate fi ștearsă numai din luna deschisă. Luna este închisă; asocierea se poate doar consulta.', 409)
    payment = repo.get('Plati', link['IDPL'])
    document = repo.get('AlteDoc', link['IDA'])
    require(document['IDPL'] == payment['IDPL'], 'CONFLICT', 'Documentul și plata asocierii nu se potrivesc. Reîncărcați lista.', 409)
    require(body.get('document_version') in (None, document['Version']), 'CONFLICT',
            'Documentul s-a modificat. Reîncărcați lista înainte de ștergere.', 409)
    require(not repo.query('SELECT IDC FROM AD_Chitante WHERE SubunitId=%s AND IDPL=%s', (repo.scope(), payment['IDPL'])),
            'CONFLICT', 'Plata are și o chitanță atașată; asocierea nu poate fi ștearsă automat.', 409)
    # The link goes first: it holds the foreign keys to the document and the payment.
    repo.execute('DELETE FROM AD_EXTRASE_ASOCIERI WHERE IDFXE=%s AND SUBUNIT_ID=%s AND IDP=%s',
                 (link['IDFXE'], repo.scope(), link['IDP']))
    require(repo.cursor.rowcount == 1, 'CONFLICT', 'Asocierea a fost modificată. Reîncărcați lista.', 409)
    repo.delete('AlteDoc', document)
    repo.delete('Plati', payment)
    return {'extract_id': link['IDFXE'], 'child_id': link['IDP'], 'deleted': True}
