"""SLICE-VC22-01: complete deletion of the documents of ONE OI/MF gestiune, for all years or for chosen years.

What goes (the document year is the year of its date: an OI/MF gestiune has one exercise holding the whole history):
  * the PTC documents -- handovers (BP), transfers (BT), write-offs (BS) -- with their rows in the OI register (VC_OI_Produse);
  * ALL the consumption bons (BC, BR returns included) of the gestiune with their lines and return positions;
  * the NIR (receipts, opening-stock NIR and storno included) with their lines, storno rows, invoice links and distributions,
    and the received-invoice links the E-Factura side keeps for them (EF_PrimiteVerconNir, EF_VERCON_ALOCARI; no foreign key).
What stays: the invoices themselves (VC_FACTURI*), nomenclatures, products, holders, accounts, commissions, the audit trail, the archive.

Two commands, both for ONE database and ONE gestiune:
  * the analysis (read only) counts every table and says what blocks the deletion;
  * the deletion (WRITES, IRREVERSIBLE) repeats the analysis inside one transaction, deletes, and refuses when the counts moved.
The window shows ONE command, the analysis. The typed value is `<years>` when the database has one OI/MF gestiune, else
`<GestiuneId or name>;<years>` (years = TOATE, 2023, 2022,2023 or 2020-2023); `*` lists the OI/MF gestiuni. When the analysis finds
nothing against the deletion it returns a confirmation token (`<GestiuneId>;<years>;<gestiune name>`); the window then turns its
«Rulează» button into «Rulează ștergerea», which sends that token to the (hidden) deletion command. The token's name is checked here too.
"""
import re
from functools import partial

from routes.admin_cli.admin_common import AdminError, _exec, _journal, _rows

ANALYSIS = 'vercon_oi_analiza_golirii'
DELETE = 'vercon_oi_golire'
TITLE = 'Golire gestiune OI/MF'
HISTORY_TYPES = ('OI', 'MF')
ALL_YEARS = 'TOATE'
SC = 'SubunitId=%s AND GestiuneId=%s'
OPTIONAL_TABLES = ('EF_VERCON_ALOCARI', 'EF_PrimiteVerconNir')   # a database may not have them
PTC_FAMILIES = (   # family, table, document key, date column, register condition for one family (see oi_custody.document_where)
    ('bp', 'VC_OI_Predare', 'IDBP', 'DATABP', 'IDBP IN ({ids}) AND COALESCE(IDBT,0)=0'),
    ('bt', 'VC_OI_Transfer', 'IDBT', 'DATABT', 'IDBT IN ({ids}) AND COALESCE(IDBP,0)=0'),
    ('bs', 'VC_OI_Casare', 'IDBC', 'DATABC', 'IDBC IN ({ids}) AND COALESCE(IDBP,0)=0 AND COALESCE(IDBT,0)=0'),
)
BC_KEYS = 'h.SubunitId=x.SubunitId AND h.GestiuneId=x.GestiuneId AND h.ExercitiuId=x.ExercitiuId AND h.IdBonConsum=x.IdBonConsum'


def commands(block):
    return {
        ANALYSIS: ('Golire gestiune OI/MF', 'Pas 1: numără ce s-ar șterge dintr-o gestiune OI/MF (bonuri PTC cu registrul, consumuri, NIR) pentru toți anii '
                   'sau pentru anii aleși și spune ce oprește ștergerea. Doar citire. Dacă totul e în regulă, butonul «Rulează» devine «Rulează ștergerea» (pas 2).',
                   False, partial(analyze, block=block)),
        DELETE: ('Golire gestiune OI/MF (ștergerea)', 'Pas 2, pornit din butonul «Rulează ștergerea» de după analiză: șterge definitiv bonurile PTC (BP, BT, BS) '
                 'cu registrul OI, toate consumurile și NIR-urile gestiunii, pentru anii analizați. SCRIE, într-o singură tranzacție, cu Jurnal; NU se poate anula.',
                 True, partial(delete, block=block)),
    }


# ---- the typed value --------------------------------------------------------------------------------------------------
def _years(text):
    """None for every year, else the sorted list of the chosen years (`2023`, `2022,2023`, `2020-2023`)."""
    value = (text or '').strip().upper()
    if value in (ALL_YEARS, 'ALL', 'TOTI', '*'):
        return None
    chosen = set()
    for part in re.split(r'[,\s]+', value):
        if not part:
            continue
        span = re.fullmatch(r'(\d{4})-(\d{4})', part)
        if span and int(span[1]) <= int(span[2]) <= int(span[1]) + 60:
            chosen.update(range(int(span[1]), int(span[2]) + 1))
        elif re.fullmatch(r'\d{4}', part):
            chosen.add(int(part))
        else:
            raise AdminError('Anii se scriu TOATE, un an (2023), mai mulți ani separați prin virgulă (2022,2023) sau un interval (2020-2023).')
    if not chosen:
        raise AdminError('Scrieți anii: TOATE, 2023, 2022,2023 sau 2020-2023.')
    return sorted(chosen)


def _years_text(years):
    return ALL_YEARS if years is None else ','.join(str(y) for y in years)


def _parse_delete(param):
    """The value the analysis hands to the deletion: `<GestiuneId>;<years>;<exact gestiune name>`."""
    parts = (param or '').split(';', 2)
    if len(parts) < 3 or not parts[0].strip().isdigit():
        raise AdminError('Ștergerea se pornește din fereastră, după analiză (butonul «Rulează ștergerea»). Nu s-a șters nimic.')
    return int(parts[0]), _years(parts[1]), parts[2].strip()


def _oi_mf_gestiuni(conn):
    return _rows(conn, "SELECT g.GestiuneId, g.Name FROM VC_Gestiuni g WHERE g.Type IN ('OI','MF') ORDER BY g.GestiuneId")


def _resolve(conn, param):
    """(gestiune, years) from `<id or name>;<years>`, or from `<years>` alone when the database has exactly one OI/MF gestiune
    (then the gestiune is None when there are several, and the caller lists them)."""
    text = (param or '').strip()
    first, sep, rest = text.partition(';')
    if not sep and first.isdigit() and len(first) != 4:
        first, sep, rest = first, ';', ALL_YEARS   # a bare id means every year
    if sep:
        years = _years(rest)
        key = first.strip()
        if key.isdigit():
            return _gestiune(conn, int(key)), years
        found = [r for r in _oi_mf_gestiuni(conn) if str(r['Name']).casefold() == key.casefold()]
        if len(found) != 1:
            raise AdminError('Gestiunea «%s» nu a fost găsită sau există în mai multe subunități: scrieți IdGestiune (valoarea * le listează).' % key)
        return _gestiune(conn, found[0]['GestiuneId']), years
    years = _years(text)
    found = _oi_mf_gestiuni(conn)
    if len(found) == 1:
        return _gestiune(conn, found[0]['GestiuneId']), years
    return None, years


def _gestiune(conn, gid):
    rows = _rows(conn, 'SELECT g.SubunitId, g.GestiuneId, g.Name, g.Type, s.Name AS subunit_name FROM VC_Gestiuni g '
                 'LEFT JOIN VC_Subunitati s ON s.SubunitId=g.SubunitId WHERE g.GestiuneId=%s', (gid,))
    if not rows:
        raise AdminError('Gestiunea cu IdGestiune %d nu există în această bază.' % gid)
    if rows[0]['Type'] not in HISTORY_TYPES:
        raise AdminError('Comanda este doar pentru gestiuni OI sau MF; gestiunea «%s» este de tip %s. Nu s-a șters nimic.' % (rows[0]['Name'], rows[0]['Type']))
    return rows[0]


# ---- the steps: one FROM/WHERE per table, shared by the count and the delete -------------------------------------------
def _year_in(column, years):
    """(sql, params): true when the year of `column` is one of `years`; always true for every year."""
    if years is None:
        return '1=1', ()
    return 'COALESCE(YEAR(%s),0) IN (%s)' % (column, ','.join(['%s'] * len(years))), tuple(years)


def _existing_optional(conn):
    found = {r['t'] for r in _rows(conn, 'SELECT TABLE_NAME AS t FROM information_schema.TABLES WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME IN (%s,%s)' % ('%s', '%s'),
                                   OPTIONAL_TABLES)}
    return found


def _register_condition(years, sc):
    """(sql, params) selecting the OI register rows that belong to the PTC documents of the chosen years; every row for every year."""
    if years is None:
        return '1=1', ()
    parts, params = [], []
    for _, table, key, date, family_where in PTC_FAMILIES:
        year_sql, year_params = _year_in(date, years)
        ids = 'SELECT t.%s FROM %s t WHERE t.%s IS NOT NULL AND t.SubunitId=%%s AND t.GestiuneId=%%s AND %s' % (key, table, key, year_sql.replace(date, 't.' + date))
        parts.append('(' + family_where.format(ids=ids) + ')')
        params += [*sc, *year_params]
    return '(' + ' OR '.join(parts) + ')', tuple(params)


def _steps(conn, sc, years):
    """[(label, alias, from_sql, where_sql, params)] in DELETE order (children first)."""
    optional = _existing_optional(conn)
    steps = []
    reg_sql, reg_params = _register_condition(years, sc)
    steps.append(('Registru OI (VC_OI_Produse)', 'o', 'VC_OI_Produse o', 'o.SubunitId=%s AND o.GestiuneId=%s AND ' + reg_sql, (*sc, *reg_params)))
    for _, table, _key, date, _where in PTC_FAMILIES:
        year_sql, year_params = _year_in(date, years)
        label = {'VC_OI_Predare': 'Bonuri de predare BP', 'VC_OI_Transfer': 'Bonuri de transfer BT', 'VC_OI_Casare': 'Bonuri de casare BS'}[table]
        steps.append(('%s (%s)' % (label, table), 'p', table + ' p', 'p.SubunitId=%s AND p.GestiuneId=%s AND ' + year_sql.replace(date, 'p.' + date), (*sc, *year_params)))
    bc_year, bc_params = _year_in('h.DataBC', years)
    bc_where = 'h.SubunitId=%s AND h.GestiuneId=%s AND ' + bc_year
    for table in ('VC_Iesiri', 'VC_IesiriRetur'):
        steps.append(('Pozitii consum (%s)' % table, 'x', '%s x JOIN VC_Iesiri_H h ON %s' % (table, BC_KEYS), bc_where, (*sc, *bc_params)))
    steps.append(('Bonuri de consum si retur (VC_Iesiri_H)', 'h', 'VC_Iesiri_H h', bc_where, (*sc, *bc_params)))
    n_year, n_params = _year_in('n.Data', years)
    n_where = 'n.SubunitId=%s AND n.GestiuneId=%s AND ' + n_year
    on_line = 'JOIN VC_Intrari i ON i.SubunitId={a}.SUBUNIT_ID AND i.GestiuneId={a}.GESTIUNE_ID AND i.ExercitiuId={a}.EXERCITIU_ID AND i.ID={a}.INTRARE_ID ' \
              'JOIN VC_NIR n ON n.SubunitId=i.SubunitId AND n.GestiuneId=i.GestiuneId AND n.ExercitiuId=i.ExercitiuId AND n.IdIntrare=i.IdIntrare'
    on_nir = 'JOIN VC_NIR n ON n.SubunitId={a}.SUBUNIT_ID AND n.GestiuneId={a}.GESTIUNE_ID AND n.ExercitiuId={a}.EXERCITIU_ID AND n.IdIntrare={a}.NIR_ID'
    steps.append(('Repartizari facturi pe linii (VC_FACTURI_REPARTIZARI)', 'a', 'VC_FACTURI_REPARTIZARI a ' + on_line.format(a='a'), n_where, (*sc, *n_params)))
    steps.append(('Calcule NIR (VC_NIR_CALCULE)', 'a', 'VC_NIR_CALCULE a ' + on_line.format(a='a'), n_where, (*sc, *n_params)))
    steps.append(('Legaturi NIR-factura (VC_NIR_FACTURI)', 'a', 'VC_NIR_FACTURI a ' + on_nir.format(a='a'), n_where, (*sc, *n_params)))
    if 'EF_VERCON_ALOCARI' in optional:
        steps.append(('Alocari E-Factura (EF_VERCON_ALOCARI)', 'a', 'EF_VERCON_ALOCARI a ' + on_nir.format(a='a'), n_where, (*sc, *n_params)))
    if 'EF_PrimiteVerconNir' in optional:
        steps.append(('Legaturi trimiteri E-Factura (EF_PrimiteVerconNir)', 'a', 'EF_PrimiteVerconNir a JOIN VC_NIR n ON n.SubunitId=a.SubunitId AND n.GestiuneId=a.GestiuneId '
                      'AND n.ExercitiuId=a.ExercitiuId AND n.IdIntrare=a.IdIntrare', n_where, (*sc, *n_params)))
    on_head = 'JOIN VC_NIR n ON n.SubunitId=a.SubunitId AND n.GestiuneId=a.GestiuneId AND n.ExercitiuId=a.ExercitiuId AND n.IdIntrare=a.IdIntrare'
    steps.append(('Pozitii NIR (VC_Intrari)', 'a', 'VC_Intrari a ' + on_head, n_where, (*sc, *n_params)))
    steps.append(('Storno pe pozitii (VC_IntrariStorno)', 'a', 'VC_IntrariStorno a ' + on_head, n_where, (*sc, *n_params)))
    steps.append(('NIR (VC_NIR)', 'n', 'VC_NIR n', n_where, (*sc, *n_params)))
    return steps


def _count(conn, step):
    _label, _alias, source, where, params = step
    return int(_rows(conn, 'SELECT COUNT(*) AS n FROM %s WHERE %s' % (source, where), params)[0]['n'])


# ---- what blocks the deletion --------------------------------------------------------------------------------------------
def _blockers(conn, sc, years):
    found = []

    def check(reason, sql, params):
        n = int(_rows(conn, sql, params)[0]['n'])
        if n:
            found.append('%s (%d)' % (reason, n))

    pn, pn_p = _year_in('n.Data', years)
    pb, pb_p = _year_in('h.DataBC', years)
    # a bon of a transfer between subunits (BU) is cancelled from its own page in the source gestiune; its link table lives outside this command
    bu = r'^BU [1-9][0-9]*$'
    check('Bonuri de transfer între subunități (BU) în intervalul ales; anulați-le din «Transferă între subunități»',
          'SELECT COUNT(*) AS n FROM VC_OI_Transfer p WHERE p.SubunitId=%s AND p.GestiuneId=%s AND p.NRBT REGEXP %s AND ' + _year_in('p.DATABT', years)[0],
          (*sc, bu, *_year_in('p.DATABT', years)[1]))
    check('NIR legate de un BU', 'SELECT COUNT(*) AS n FROM VC_NIR n WHERE n.SubunitId=%s AND n.GestiuneId=%s AND n.NrFact REGEXP %s AND ' + pn, (*sc, bu, *pn_p))
    check('Bonuri de predare legate de un BU', 'SELECT COUNT(*) AS n FROM VC_OI_Predare p WHERE p.SubunitId=%s AND p.GestiuneId=%s AND p.Expl REGEXP %s AND '
          + _year_in('p.DATABP', years)[0], (*sc, r' · BU [1-9][0-9]*$', *_year_in('p.DATABP', years)[1]))
    check('Consumuri asociate unor liste de alimente',
          'SELECT COUNT(*) AS n FROM VC_LegBCLA x JOIN VC_Iesiri_H h ON ' + BC_KEYS + ' WHERE h.SubunitId=%s AND h.GestiuneId=%s AND ' + pb, (*sc, *pb_p))
    if years is None:
        return found   # nothing is kept, so nothing kept can point at what goes
    keep_b, keep_b_p = 'NOT (' + _year_in('k.DataBC', years)[0] + ')', _year_in('k.DataBC', years)[1]
    keep_n, keep_n_p = 'NOT (' + _year_in('k.Data', years)[0] + ')', _year_in('k.Data', years)[1]
    gone_b, gone_b_p = _year_in('d.DataBC', years)
    gone_n, gone_n_p = _year_in('d.Data', years)
    check('Retururi (BR) din anii păstrați care arată spre un consum șters',
          'SELECT COUNT(*) AS n FROM VC_Iesiri_H k JOIN VC_Iesiri_H d ON d.SubunitId=k.SubunitId AND d.GestiuneId=k.GestiuneId AND d.ExercitiuId=k.ExercitiuId '
          'AND d.IdBonConsum=k.IdRetur WHERE k.SubunitId=%s AND k.GestiuneId=%s AND ' + keep_b + ' AND ' + gone_b, (*sc, *keep_b_p, *gone_b_p))
    check('Storno din anii păstrați care arată spre un NIR șters',
          'SELECT COUNT(*) AS n FROM VC_NIR k JOIN VC_NIR d ON d.SubunitId=k.SubunitId AND d.GestiuneId=k.GestiuneId AND d.ExercitiuId=k.ExercitiuId '
          'AND d.IdIntrare=k.IdStorno WHERE k.SubunitId=%s AND k.GestiuneId=%s AND ' + keep_n + ' AND ' + gone_n, (*sc, *keep_n_p, *gone_n_p))
    # a handover and the consumption bon it made are written together: both must fall in the chosen years or neither
    pp, pp_p = _year_in('p.DATABP', years)
    check('Predări (BP) și consumul lor sunt în ani diferiți',
          'SELECT COUNT(*) AS n FROM VC_OI_Predare p JOIN VC_Iesiri_H h ON h.SubunitId=p.SubunitId AND h.GestiuneId=p.GestiuneId AND h.ExercitiuId=p.ExercitiuId '
          'AND h.IdBonConsum=p.IdBonConsum WHERE p.SubunitId=%s AND p.GestiuneId=%s AND ((' + pp + ' AND NOT (' + pb + ')) OR (NOT (' + pp + ') AND ' + pb + '))',
          (*sc, *pp_p, *pb_p, *pp_p, *pb_p))
    # a transfer or write-off that stays took goods from a batch (IDBO) of a handover or transfer that goes
    reg_sql, reg_params = _register_condition(years, sc)
    batches = []
    batch_params = []
    for _, table, key, date, _w in PTC_FAMILIES[:2]:
        year_sql, year_params = _year_in(date, years)
        batches.append('SELECT t.%s FROM %s t WHERE t.%s IS NOT NULL AND t.SubunitId=%%s AND t.GestiuneId=%%s AND %s' % (key, table, key, year_sql.replace(date, 't.' + date)))
        batch_params += [*sc, *year_params]
    check('Transferuri sau casări din anii păstrați care au luat bunuri dintr-un lot șters',
          'SELECT COUNT(*) AS n FROM VC_OI_Produse o WHERE o.SubunitId=%s AND o.GestiuneId=%s AND o.IDBO IN (' + ' UNION '.join(batches) + ') AND NOT ' + reg_sql,
          (*sc, *batch_params, *reg_params))
    return found


def _deletion_triggers(conn, db, steps):
    tables = sorted({s[2].split(' ')[0] for s in steps})
    marks = ','.join(['%s'] * len(tables))
    rows = _rows(conn, 'SELECT EVENT_OBJECT_TABLE AS t FROM information_schema.TRIGGERS WHERE EVENT_OBJECT_SCHEMA=%s AND EVENT_MANIPULATION=\'DELETE\' '
                 'AND EVENT_OBJECT_TABLE IN (' + marks + ')', (db, *tables))
    if rows:
        raise AdminError('Există un declanșator de ștergere pe %s. Ștergerea a fost oprită.' % ', '.join(sorted({r['t'] for r in rows})))


# ---- the analysis -------------------------------------------------------------------------------------------------------
def _list_gestiuni(conn, db, block):
    rows = []
    for g in _rows(conn, "SELECT g.SubunitId, g.GestiuneId, g.Name, g.Type, s.Name AS sub FROM VC_Gestiuni g LEFT JOIN VC_Subunitati s ON s.SubunitId=g.SubunitId "
                   "WHERE g.Type IN ('OI','MF') ORDER BY g.SubunitId, g.Name"):
        sc = (g['SubunitId'], g['GestiuneId'])
        years = _rows(conn, 'SELECT DISTINCT YEAR(d) AS y FROM (SELECT DataBC AS d FROM VC_Iesiri_H WHERE ' + SC + ' UNION SELECT Data FROM VC_NIR WHERE ' + SC
                      + ' UNION SELECT DATABP FROM VC_OI_Predare WHERE ' + SC + ' UNION SELECT DATABT FROM VC_OI_Transfer WHERE ' + SC
                      + ' UNION SELECT DATABC FROM VC_OI_Casare WHERE ' + SC + ') u WHERE d IS NOT NULL ORDER BY y', (*sc,) * 5)
        rows.append([str(g['GestiuneId']), g['sub'] or '-', g['Name'], g['Type'], ', '.join(str(r['y']) for r in years) or 'fără documente'])
    return block(db, 'Gestiuni OI/MF', ['IdGestiune', 'subunitate', 'gestiune', 'tip', 'ani cu documente'], rows,
                 'Sunt mai multe gestiuni OI/MF. Scrieți IdGestiune;ani (sau numele gestiunii;ani), de exemplu 12;TOATE sau 12;2022,2023, și rulați din nou.')


def analyze(conn, db, param, block):
    if (param or '').strip() == '*':
        return _list_gestiuni(conn, db, block)
    g, years = _resolve(conn, param)
    if g is None:
        return _list_gestiuni(conn, db, block)
    sc = (g['SubunitId'], g['GestiuneId'])
    steps = _steps(conn, sc, years)
    counts = [_count(conn, s) for s in steps]
    blockers = _blockers(conn, sc, years)
    who = '%s / %s (%s), ani: %s' % (g['subunit_name'] or '-', g['Name'], g['Type'], _years_text(years))
    rows = [[s[0], str(n)] for s, n in zip(steps, counts)]
    total = sum(counts)
    note = ('SE POATE ȘTERGE. Verificați numerele; butonul «Rulează» a devenit «Rulează ștergerea».' if not blockers and total else
            'NU SE POATE ȘTERGE: ' + '; '.join(blockers) if blockers else 'Nu există nimic de șters pentru această alegere.')
    out = [block(db, TITLE + ' - analiză: ' + who, ['tabelă', 'rânduri de șters'], rows,
                 'Se șterg doar rândurile gestiunii alese. Facturile (VC_FACTURI*), nomenclatoarele și Auditul rămân. ' + (
                     'Registrul OI se golește complet (toți anii).' if years is None else
                     'Stocul nu se mai verifică după ștergere: anii păstrați pot rămâne cu stoc negativ.'))]
    if not blockers and total:
        verdict = block(db, 'Rezultat analiză', ['rezultat'], [[note]], raw=True)
        # the window keeps this value and sends it to the deletion when the operator presses «Rulează ștergerea»
        verdict['confirmation_token'] = '%d;%s;%s' % (g['GestiuneId'], _years_text(years), g['Name'])
        verdict['confirmation_label'] = '%s: %s / %s (%s), ani: %s. Se șterg %d rânduri.' % (
            db, g['subunit_name'] or '-', g['Name'], g['Type'], _years_text(years), total)
        out.append(verdict)
    else:
        out.append(block(db, 'Rezultat analiză', ['rezultat'], [[note]], raw=True))
    return out


# ---- the deletion -------------------------------------------------------------------------------------------------------
def _reset_counters(conn, sc):
    """A counter goes back to 0 only when no document of its kind is left in the gestiune."""
    def empty(table):
        return int(_rows(conn, 'SELECT COUNT(*) AS n FROM ' + table + ' WHERE ' + SC, sc)[0]['n']) == 0
    rules = [("(Kind='BC' OR LEFT(Kind,3)='BC:')", int(_rows(conn, 'SELECT COUNT(*) AS n FROM VC_Iesiri_H WHERE ' + SC + ' AND COALESCE(Retur,0)=0', sc)[0]['n']) == 0),
             ("Kind='BR'", int(_rows(conn, 'SELECT COUNT(*) AS n FROM VC_Iesiri_H WHERE ' + SC + ' AND COALESCE(Retur,0)=1', sc)[0]['n']) == 0),
             ("Kind='BP'", empty('VC_OI_Predare')), ("Kind='BT'", empty('VC_OI_Transfer')), ("Kind='BS'", empty('VC_OI_Casare')),
             ("Kind='OIID'", empty('VC_OI_Predare') and empty('VC_OI_Transfer') and empty('VC_OI_Casare') and empty('VC_OI_Produse'))]
    reset = 0
    for where, no_documents in rules:
        if no_documents:
            reset += _exec(conn, 'UPDATE VC_Numbering SET LastNumber=0, Version=Version+1 WHERE ' + SC + ' AND LastNumber<>0 AND ' + where, sc)
    return reset


def delete(conn, db, param, block):
    gid, years, typed = _parse_delete(param)
    g = _gestiune(conn, gid)
    if typed.casefold() != str(g['Name']).casefold():
        raise AdminError('Confirmarea nu se potrivește: numele gestiunii este «%s», ați scris «%s». Nu s-a șters nimic.' % (g['Name'], typed))
    sc = (g['SubunitId'], g['GestiuneId'])
    conn.start_transaction(isolation_level='READ COMMITTED')
    try:
        # the document writers of VERCON take the same lock before they touch a document of the gestiune
        if not _rows(conn, 'SELECT GestiuneId FROM VC_Gestiuni WHERE ' + SC + ' FOR UPDATE', sc):
            raise AdminError('Gestiunea nu mai există. Nu s-a șters nimic.')
        steps = _steps(conn, sc, years)
        _deletion_triggers(conn, db, steps)
        blockers = _blockers(conn, sc, years)
        if blockers:
            raise AdminError('Ștergerea este oprită: ' + '; '.join(blockers) + '. Nu s-a șters nimic.')
        expected = [_count(conn, s) for s in steps]
        deleted = []
        for step, wanted in zip(steps, expected):
            _label, alias, source, where, params = step
            done = _exec(conn, 'DELETE %s FROM %s WHERE %s' % (alias, source, where), params)
            if done != wanted:
                raise AdminError('Tabela «%s»: s-au șters %d rânduri în loc de %d. Nu s-a șters nimic; reluați analiza.' % (step[0], done, wanted))
            deleted.append(done)
        counters = _reset_counters(conn, sc)
        who = '%s / %s (%s), ani: %s' % (g['subunit_name'] or '-', g['Name'], g['Type'], _years_text(years))
        _journal(conn, DELETE, db, 'VC_Gestiuni ID %d' % g['GestiuneId'],
                 '%s | %s | contoare resetate: %d' % (who, '; '.join('%s: %d' % (s[0], n) for s, n in zip(steps, deleted) if n), counters))
        conn.commit()
    except Exception:
        conn.rollback()
        raise
    rows = [[s[0], str(n)] for s, n in zip(steps, deleted)]
    return [block(db, TITLE + ' - șters: ' + who, ['tabelă', 'rânduri șterse'], rows, 'Ștergerea este consemnată în Jurnal.'),
            block(db, 'Rezultat', ['rezultat'], [['%d rânduri șterse; %d contoare de numerotare puse pe 0.' % (sum(deleted), counters)]],
                  'NU se poate anula. Rulați din nou analiza ca să vedeți ce a rămas.', raw=True)]
