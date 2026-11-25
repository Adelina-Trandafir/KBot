"""KBotAdmin: bank statements that FX_Extrase_F holds but FX_Extrase_H / FX_Extrase do not (slice ME04-03).

`check`  - READ ONLY. Reads the XML kept in every FX_Extrase_F row again, exactly the way the import reads it, and lists what the
           import should have written and did not: account headers (FX_Extrase_H) and operations (FX_Extrase).
`repair` - WRITES. Writes only what `check` finds, in one transaction, under the FX_Extrase_F row the XML belongs to. No
           FX_Extrase_F row is created, changed or deleted; nothing already there is touched.

What counts as missing is decided with the SAME rules as the import (routes/forexe/extrase.py), not with new ones:
  * a header is missing when the file has no FX_Extrase_H row for the same account (`Cont`);
  * an operation is missing when its natural key (DataDoc, NrDoc, platitor_cui, suma_debit, suma_credit, Referinta) is not in
    FX_Extrase at all. The key is global on purpose (the same operation in two files is one operation), and an operation that
    appears twice in the XML with the whole key equal is one operation too - the import drops the second one in the same way.
The rows are written by the import's own functions (`_scrie_antet`, `_scrie_operatiune`), so a repaired row is the row the import
would have written: same unit, classification, partner, HASH.

Born from TZ521102968922 (16.11.2026): two payments of one payer, same day, same document number, same amount, differing only by
the reference; the import of that time dropped the second as a duplicate (fixed by AD21-02, which put the reference in the key),
so files imported before the fix are short of operations that the XML still holds.
"""
import xml.etree.ElementTree as ElementTree
from collections import defaultdict

from routes.admin_cli.admin_common import AdminError, _journal, _rows

TITLE = 'Extrase lipsa'
SUMMARY_TITLE = TITLE + ' - sumar'
DETAIL_TITLE = TITLE + ' - detalii'
REPAIR_COLUMN = 'reparabile'
DETAIL_LIMIT = 1000
JOURNAL_LIMIT = 60000

REQUIRED = {
    'FX_Extrase_F': 'IDEXF NumeFisier XML',
    'FX_Extrase_H': 'IDEXH IDEXF Cont IdUnitate IdClsf',
    'FX_Extrase': 'IDFXH DataDoc NrDoc platitor_cui suma_debit suma_credit Referinta',
}


def _extrase():
    """The import module, loaded lazily (it loads the whole FOREXE package, like receptii_lanturi.py does)."""
    from routes.forexe import extrase
    return extrase


def _check_schema(conn, db):
    columns = defaultdict(set)
    for row in _rows(conn, 'SELECT TABLE_NAME AS t,COLUMN_NAME AS c FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=%s', (db,)):
        columns[row['t']].add(row['c'])
    missing = [table + '.' + col for table, fields in REQUIRED.items() for col in fields.split() if col not in columns.get(table, set())]
    if missing:
        raise AdminError('Lipsesc din baza %s: %s. Structura nu se modifica automat.' % (db, ', '.join(missing)))


def _xml_text(value):
    if value is None:
        return ''
    if isinstance(value, (bytes, bytearray)):
        return bytes(value).decode('utf-8', 'replace')
    return str(value)


def _day(value):
    return value.strftime('%d.%m.%Y') if hasattr(value, 'strftime') else ('' if value is None else str(value))


def _scan(conn, db, writer=None):
    """Walks every FX_Extrase_F row. `writer` is None for a read, or (cursor, nomenclatoare, warnings) to write what is missing.

    Returns a dict: files, no_xml, bad_xml (lists of (IDEXF, name[, error])), headers, ops (counts), lines (one text per finding),
    written_headers / written_ops (what was really inserted), touched (IDEXF of the files that got rows)."""
    ex = _extrase()
    headers = {}
    for row in _rows(conn, 'SELECT IDEXH,IDEXF,Cont,IdUnitate,IdClsf FROM FX_Extrase_H ORDER BY IDEXH'):
        headers.setdefault((row['IDEXF'], str(row['Cont'] or '').strip()), (row['IDEXH'], row['IdUnitate'], row['IdClsf']))
    known = {(ex._zi(row['DataDoc']), row['NrDoc'], row['platitor_cui'], ex._f(row['suma_debit']), ex._f(row['suma_credit']),
              ex._ref_cheie(row['Referinta']))
             for row in _rows(conn, 'SELECT DataDoc,NrDoc,platitor_cui,suma_debit,suma_credit,Referinta FROM FX_Extrase')}

    out = {'files': 0, 'no_xml': [], 'bad_xml': [], 'headers': 0, 'ops': 0, 'lines': [], 'written_headers': 0, 'written_ops': 0,
           'touched': set(), 'journal': []}
    for file_row in _rows(conn, 'SELECT IDEXF,NumeFisier FROM FX_Extrase_F ORDER BY IDEXF'):
        idexf, name = file_row['IDEXF'], file_row['NumeFisier'] or ''
        xml_text = _xml_text(_rows(conn, 'SELECT XML FROM FX_Extrase_F WHERE IDEXF=%s', (idexf,))[0]['XML'])
        if not xml_text.strip():
            out['no_xml'].append((idexf, name))
            continue
        try:
            root = ElementTree.fromstring(xml_text)
        except ElementTree.ParseError as err:
            out['bad_xml'].append((idexf, name, str(err)))
            continue
        out['files'] += 1
        for cont_ext in root.findall('cont_ext'):
            node = cont_ext.find('cont')
            if node is None:
                continue          # the import skips such a node too: its rows have no header to hang on
            cont = ex._text(node)
            head = headers.get((idexf, cont))
            if head is None:
                out['headers'] += 1
                out['lines'].append(['antet', idexf, name, cont, '', '', '', '', ''])
                if writer is not None:
                    cursor, nom, warnings = writer
                    idexh, id_unitate, id_clsf = ex._scrie_antet(cursor, nom, cont_ext, idexf, warnings)
                    head = (idexh, id_unitate, id_clsf)
                    headers[(idexf, cont)] = head
                    out['written_headers'] += 1
                    out['touched'].add(idexf)
                    out['journal'].append('antet IDEXF %s cont %s -> IDEXH %s' % (idexf, cont, idexh))
                else:
                    head = (None, None, None)
            for cont_misc in cont_ext.findall('cont_misc'):
                key = ex._cheie_operatiune(cont_misc)
                if key in known:
                    continue
                known.add(key)
                out['ops'] += 1
                data_doc, nr_doc, cui, debit, credit, ref = key
                out['lines'].append(['operatiune', idexf, name, cont, _day(data_doc), nr_doc or '', cui or '',
                                     ('+%s' % credit) if credit else ('-%s' % debit), ref])
                if writer is not None:
                    cursor, nom, _warnings = writer
                    # the key was just checked and added above, so this call always writes
                    if not ex._scrie_operatiune(cursor, nom, cont_misc, head[0], head[1], head[2], set()):
                        raise AdminError('Operatiunea %s din fisierul %s nu s-a putut scrie. Nu s-a aplicat nimic.' % (ref, name))
                    out['written_ops'] += 1
                    out['touched'].add(idexf)
                    out['journal'].append('operatiune IDEXF %s cont %s: %s' % (idexf, cont, ' | '.join(str(x) for x in key)))
    return out


def check(conn, db, block):
    _check_schema(conn, db)
    found = _scan(conn, db)
    repairable = found['headers'] + found['ops']
    rows = [
        ['fisiere citite din XML', found['files'], 0, 'fisiere din FX_Extrase_F al caror XML a putut fi citit'],
        ['fisiere fara XML', len(found['no_xml']), 0, 'FX_Extrase_F nu are XML pentru ele: nu se poate reface nimic din ele'],
        ['fisiere cu XML invalid', len(found['bad_xml']), 0, 'XML-ul nu se poate citi: se listeaza, nu se ating'],
        ['antete lipsa', found['headers'], found['headers'], 'un cont din XML fara rand in FX_Extrase_H sub fisierul lui'],
        ['operatiuni lipsa', found['ops'], found['ops'],
         'o operatiune din XML a carei cheie (data, numar, platitor, sume, referinta) nu este in FX_Extrase'],
    ]
    blocks = [block(db, SUMMARY_TITLE, ['problema', 'cazuri', REPAIR_COLUMN, 'ce inseamna'], rows,
                    'Doar citire. Zero la antete si operatiuni lipsa = tot ce are XML-ul este in baza. Cu cazuri apare butonul «Repara», care '
                    'scrie DOAR ce lipseste, sub fisierul din care vine, fara sa atinga ce exista.')]
    detail = [['ce lipseste', 'IDEXF', 'nume fisier', 'cont', 'data doc', 'nr doc', 'platitor', 'suma', 'referinta']]
    detail.extend(found['lines'][:DETAIL_LIMIT])
    skipped = [['fisier fara XML', i, n, '', '', '', '', '', ''] for i, n in found['no_xml']]
    skipped += [['XML invalid', i, n, '', '', '', '', '', e] for i, n, e in found['bad_xml']]
    if found['lines'] or skipped:
        note = 'Toate cazurile sunt listate. Nimic nu a fost schimbat.'
        if len(found['lines']) > DETAIL_LIMIT:
            note = 'Primele %d din %d cazuri. Repararea le scrie pe toate, nu doar pe cele afisate.' % (DETAIL_LIMIT, len(found['lines']))
        # the first column of the list says what each line is: «antet» / «operatiune» / why a file could not be read
        blocks.append(block(db, DETAIL_TITLE, detail[0], detail + skipped, note, raw=True))
    if repairable == 0 and not skipped:
        blocks[0]['note'] += ' Acum: nimic de reparat.'
    return blocks


def repair(conn, db, block):
    """WRITES. Adds the missing headers and operations under their own FX_Extrase_F row, all or nothing, then shows what check finds now."""
    _check_schema(conn, db)
    engines = {r['t']: str(r['e'] or '').upper() for r in _rows(
        conn, 'SELECT TABLE_NAME AS t,ENGINE AS e FROM information_schema.TABLES WHERE TABLE_SCHEMA=%s '
              "AND TABLE_NAME IN ('FX_Extrase_F','FX_Extrase_H','FX_Extrase')", (db,))}
    if any(engines.get(t) != 'INNODB' for t in REQUIRED):
        raise AdminError('Repararea cere tabele InnoDB (FX_Extrase_F, FX_Extrase_H, FX_Extrase), ca totul sa se aplice intr-o singura '
                         'tranzactie. Structura nu se modifica automat.')
    ex = _extrase()
    conn.start_transaction(isolation_level='READ COMMITTED')
    cursor = None
    try:
        cursor = conn.cursor()
        nom = ex._Nomenclatoare(cursor)
        warnings = list(nom.avertismente)
        done = _scan(conn, db, (cursor, nom, warnings))
        if done['written_ops']:
            # like the import: payments that waited for their statement row may now attach to ordonantari made from a reception
            from routes.forexe.ord_receptie_plati import leaga_automat_protejat
            leaga_automat_protejat(cursor, None, warnings)
        warnings.extend(w for w in nom.avertismente if w not in warnings)
        if done['written_headers'] or done['written_ops']:
            _journal(conn, 'extrase_lipsa_repara', db, 'FX_Extrase',
                     ('Fisiere atinse (IDEXF): %s. Antete scrise: %d. Operatiuni scrise: %d. Randuri: %s'
                      % (', '.join(str(i) for i in sorted(done['touched'])), done['written_headers'], done['written_ops'],
                         ' | '.join(done['journal'])))[:JOURNAL_LIMIT])
        summary = [['%d antete si %d operatiuni scrise, in %d fisiere.' % (done['written_headers'], done['written_ops'], len(done['touched']))]]
        summary.extend([w] for w in warnings)
        result = block(db, TITLE + ' - reparare', ['rezultat'], summary,
                       'Tot ce s-a scris este in Jurnal, cu cheia fiecarei operatiuni. Mai jos, ce mai gaseste verificarea acum.', raw=True)
        after = check(conn, db, block)
        conn.commit()
        return [result] + after
    except Exception:
        conn.rollback()
        raise
    finally:
        if cursor is not None:
            cursor.close()
