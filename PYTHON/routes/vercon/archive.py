"""SLICE-VC14-04: an annulled document is ARCHIVED and DELETED, never left behind as «anulat».

Annulling a NIR, FCS (storno), BC, BR, BP, BT, BS, BU or food list used to set Anulat=1 and keep the rows: the document stayed in
the trees and its number stayed taken (the NIR series and the «next number» of every kind count the annulled rows too).
Now the cancel routes do their checks exactly as before and then call `purge`, which

  1. copies the document (header, lines and every row that hangs on it) as JSON into VC_ArhivaAnulate,
  2. deletes those rows from the VC_ tables (children first, so every foreign key holds),
  3. lowers the numbering counter (VC_Numbering) to the highest number that is still in use, so the freed number is the next
     automatic one again. A number in the middle of the series is free for a manual entry as soon as its row is gone.

Nothing here decides whether a document MAY be annulled: that stays in documents.py / oi.py / lista.py / bu.py (HAS_STORNO,
HAS_RETUR, LINKED_LIST, the stock timeline, the reverse-order rule ...). `purge` runs at the end of those checks, inside the
transaction of the route, so a refusal later in the same request rolls the archive back with everything else.
The same function serves the one-time cleanup of the documents annulled before this slice (admin_cli/vercon_arhiva_anulate.py).
"""
import json

from . import bon_number, consumption_numbering
from .documents import _all, _int, _one, _scope, _sub_text
from .nomenclatoare import NomError

ARCHIVE = 'VC_ArhivaAnulate'
_SC = 'SubunitId=%s AND GestiuneId=%s AND ExercitiuId=%s'
_UPPER_SC = 'SUBUNIT_ID=%s AND GESTIUNE_ID=%s AND EXERCITIU_ID=%s'
_OPTIONAL = ('EF_VERCON_ALOCARI', 'EF_PrimiteVerconNir')   # tables a database may not have
_HEADS = {'nir': ('VC_NIR', 'IdIntrare'), 'storno': ('VC_NIR', 'IdIntrare'), 'bc': ('VC_Iesiri_H', 'IdBonConsum'),
          'br': ('VC_Iesiri_H', 'IdBonConsum'), 'lista': ('VC_LABC', 'IdLista')}


def _has(cursor, table):
    return bool(_one(cursor, 'SELECT 1 AS x FROM information_schema.TABLES WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME=%s', (table,)))


def ready(cursor):
    """True when the archive table exists. Without it a cancel must fail loudly, not delete without keeping."""
    return _has(cursor, ARCHIVE)


def _marks(ids):
    return ','.join(['%s'] * len(ids))


def _by_key(table, column, ctx, doc_id):
    return (table, _SC + ' AND `%s`=%%s' % column, (*_scope(ctx), doc_id))


def _bon_steps(ctx, bon_id):
    """A consumption bon / return and everything on it (lines first, then the header)."""
    return [_by_key('VC_LegBCLA', 'IdBonConsum', ctx, bon_id), _by_key('VC_Iesiri', 'IdBonConsum', ctx, bon_id),
            _by_key('VC_IesiriRetur', 'IdBonConsum', ctx, bon_id), _by_key('VC_Iesiri_H', 'IdBonConsum', ctx, bon_id)]


def _plan(cursor, ctx, family, doc_id):
    """(steps, head_table, head_rows). A step is (table, where, params); the steps are in DELETE order."""
    sc = _scope(ctx)
    if family in ('nir', 'storno'):
        ids = [r['ID'] for r in _all(cursor, 'SELECT ID FROM VC_Intrari WHERE ' + _SC + ' AND IdIntrare=%s', (*sc, doc_id))]
        steps = []
        if ids:
            on_lines = _UPPER_SC + ' AND INTRARE_ID IN (%s)' % _marks(ids)
            steps += [('VC_FACTURI_REPARTIZARI', on_lines, (*sc, *ids)), ('VC_NIR_CALCULE', on_lines, (*sc, *ids))]
        steps += [('VC_NIR_FACTURI', _UPPER_SC + ' AND NIR_ID=%s', (*sc, doc_id)), ('EF_VERCON_ALOCARI', _UPPER_SC + ' AND NIR_ID=%s', (*sc, doc_id)),
                  ('EF_PrimiteVerconNir', _SC + ' AND IdIntrare=%s', (*sc, doc_id)),
                  _by_key('VC_Intrari', 'IdIntrare', ctx, doc_id), _by_key('VC_IntrariStorno', 'IdIntrare', ctx, doc_id), _by_key('VC_NIR', 'IdIntrare', ctx, doc_id)]
        return steps, 'VC_NIR', _all(cursor, 'SELECT * FROM VC_NIR WHERE ' + _SC + ' AND IdIntrare=%s', (*sc, doc_id))
    if family in ('bc', 'br'):
        return _bon_steps(ctx, doc_id), 'VC_Iesiri_H', _all(cursor, 'SELECT * FROM VC_Iesiri_H WHERE ' + _SC + ' AND IdBonConsum=%s', (*sc, doc_id))
    if family == 'lista':
        steps = [_by_key(table, 'IdLista', ctx, doc_id) for table in ('VC_LegBCLA', 'VC_Cina', 'VC_Dejun', 'VC_MicDejun', 'VC_ListaAlimente', 'VC_LABC')]
        return steps, 'VC_LABC', _all(cursor, 'SELECT * FROM VC_LABC WHERE ' + _SC + ' AND IdLista=%s', (*sc, doc_id))
    from .oi import SPEC
    from .oi_custody import document_where
    spec = SPEC[family]
    rows = _all(cursor, 'SELECT * FROM `%s` WHERE %s AND `%s`=%%s' % (spec['table'], _SC, spec['key']), (*sc, doc_id))
    steps = [('VC_OI_Produse', 'SubunitId=%s AND GestiuneId=%s AND ' + document_where(family), (ctx['subunit'], ctx['gestiune'], doc_id)),
             _by_key(spec['table'], spec['key'], ctx, doc_id)]
    if family == 'bp':   # a handover also made a consumption bon: it goes with it
        for bon in sorted({r['IdBonConsum'] for r in rows if r['IdBonConsum']}):
            steps += _bon_steps(ctx, bon)
    return steps, spec['table'], rows


def _text_of(family, row):
    if family in ('nir', 'storno'):
        return '%d%s' % (abs(row['Numar'] or 0), _sub_text(int(row.get('NumarSub') or 0)))
    if family in ('bc', 'br'):
        return str(bon_number.display(family, row['NumarBC']) or '')
    if family == 'lista':
        return str(row['NumarLA'] if row['NumarLA'] is not None else '')
    from .oi import SPEC
    return str(bon_number.display(family, row[SPEC[family]['num']]) or '')


def _date_of(family, row):
    if family in ('nir', 'storno'):
        return row['Data']
    if family in ('bc', 'br'):
        return row['DataBC']
    if family == 'lista':
        return row['DATALA']
    from .oi import SPEC
    return row[SPEC[family]['date']]


def _counter_series(cursor, ctx, family, head, *, active_only=False):
    params = _scope(ctx)
    where = _SC + (' AND Anulat=0' if active_only else '')
    if family in ('bc', 'br'):
        table, key, column = 'VC_Iesiri_H', 'IdBonConsum', 'NumarBC'
        account = head.get('IdMasina')
        scoped = account if family == 'bc' and account is not None and consumption_numbering.per_account(cursor, ctx) else None
        kind = 'BR' if family == 'br' else consumption_numbering.kind(scoped)
        where += ' AND COALESCE(Retur,0)=%s'
        params += (1 if family == 'br' else 0,)
        if scoped is not None:
            where += ' AND IdMasina=%s'
            params += (scoped,)
    elif family == 'lista':
        table, key, column, kind = 'VC_LABC', 'IdLista', 'NumarLA', 'LISTA'
    else:
        from .oi import SPEC
        spec = SPEC[family]
        table, key, column, kind = spec['table'], spec['key'], spec['num'], spec['kind']
    rows = _all(cursor, 'SELECT DISTINCT `%s` AS doc_id, `%s` AS n FROM `%s` WHERE %s' % (key, column, table, where), params)
    numbers = []
    for row in rows:
        number = _int(row['n']) if family == 'lista' else bon_number.parse(family, row['n'])
        if number is not None:
            numbers.append((row['doc_id'], number))
    return kind, head[key], numbers


def _release_number(cursor, ctx, family, head):
    """Lower the counter after cancellation. NIR/FCS use the saved documents as their series."""
    if family in ('nir', 'storno'):
        return
    kind, _, numbers = _counter_series(cursor, ctx, family, head)
    top = max((number for _, number in numbers), default=0)
    cursor.execute('UPDATE VC_Numbering SET LastNumber=%s, Version=Version+1 WHERE ' + _SC + ' AND Kind=%s AND LastNumber>%s',
                   (top, *_scope(ctx), kind, top))


def release_renumbered(cursor, ctx, family, before, replacement):
    """After saving the document, synchronize its old series if it held the highest number."""
    if family == 'lista':
        old, new = _int(before['NumarLA']), _int(replacement)
    else:
        old = bon_number.parse(family, _text_of(family, before))
        new = bon_number.parse(family, replacement)
    if old is None or new is None or new == old:
        return
    kind, doc_id, numbers = _counter_series(cursor, ctx, family, before, active_only=True)
    if any(number > old for other_id, number in numbers if other_id != doc_id):
        return
    top = max((number for _, number in numbers), default=0)
    cursor.execute('INSERT INTO VC_Numbering (SubunitId, GestiuneId, ExercitiuId, Kind, LastNumber) VALUES (%s,%s,%s,%s,%s) '
                   'ON DUPLICATE KEY UPDATE Version=IF(LastNumber=VALUES(LastNumber), Version, Version+1), LastNumber=VALUES(LastNumber)',
                   (*_scope(ctx), kind, max(0, top)))


def _dependents(cursor, ctx, family, doc_id):
    """Annulled documents that still point at this one (only seen in databases annulled before this slice): they go first."""
    sc = _scope(ctx)
    if family == 'nir':
        return [('storno', r['IdIntrare']) for r in _all(cursor, 'SELECT IdIntrare FROM VC_NIR WHERE ' + _SC + ' AND IdStorno=%s AND Anulat=1 AND COALESCE(Storno,0)=1', (*sc, doc_id))]
    if family == 'bc':
        return [('br', r['IdBonConsum']) for r in _all(cursor, 'SELECT IdBonConsum FROM VC_Iesiri_H WHERE ' + _SC + ' AND IdRetur=%s AND COALESCE(Retur,0)=1 AND Anulat=1', (*sc, doc_id))]
    return []


def _store(cursor, ctx, family, doc_id, head, content, reason, by):
    """The header rows of an annulled document hold the reason, the user and the time (the cancel routes stamp them first)."""
    first = head[0]
    cursor.execute('INSERT INTO ' + ARCHIVE + ' (SubunitId, GestiuneId, ExercitiuId, Familie, IdDocument, Numar, DataDoc, Motiv, AnulatDe, AnulatLa, Continut) '
                   'VALUES (%s,%s,%s,%s,%s,%s,%s,%s,%s,COALESCE(%s, NOW()),%s)',
                   (*_scope(ctx), family, doc_id, _text_of(family, first)[:60], _date_of(family, first),
                    reason or first.get('MotivAnulare'), by or first.get('AnulatDe'), first.get('AnulatLa'),
                    json.dumps(content, ensure_ascii=False, default=str)))
    return cursor.lastrowid


def purge(cursor, ctx, family, doc_id, reason=None, by=None):
    """Archive one document (annulled by the caller or already annulled) and delete it. Returns the archive id.
    `reason` / `by` default to what the annulled header already holds (the one-time cleanup of old annulments)."""
    if not ready(cursor):
        raise NomError('ARCHIVE_MISSING', 'Arhiva documentelor anulate nu există în această bază de date. Aplicați scriptul VC14_04_ARHIVA_ANULATE.sql.', 503)
    for child_family, child_id in _dependents(cursor, ctx, family, doc_id):
        purge(cursor, ctx, child_family, child_id, reason, by)
    steps, head_table, head = _plan(cursor, ctx, family, doc_id)
    if not head:
        raise NomError('NOT_FOUND', 'Documentul nu mai există.', 404)
    content, found = {}, []
    for table, where, params in steps:
        if table in _OPTIONAL and not _has(cursor, table):
            continue
        rows = _all(cursor, 'SELECT * FROM `%s` WHERE %s' % (table, where), params)
        if rows:
            content.setdefault(table, []).extend(rows)
            found.append((table, where, params))
    archive_id = _store(cursor, ctx, family, doc_id, head, content, reason, by)
    for table, where, params in found:   # same order as the plan: children before parents
        cursor.execute('DELETE FROM `%s` WHERE %s' % (table, where), params)
    _release_number(cursor, ctx, family, head[0])
    return archive_id


def purge_bu(cursor, source, target, link, reason, by):
    """A BU: the receipt, the handover and their bons in the destination, the bons and the transfer in the source, then the BU itself."""
    from .bu_links import LINES, TABLE
    if link['DEST_BP'] is not None:
        purge(cursor, target, 'bp', link['DEST_BP'], reason, by)
    purge(cursor, target, 'nir', link['DEST_NIR'], reason, by)
    lines = _all(cursor, 'SELECT * FROM ' + LINES + ' WHERE BU_ID=%s', (link['ID'],))
    for bon in sorted({r['SURSA_BC'] for r in lines if r['SURSA_BC']}):
        purge(cursor, source, 'bc', bon, reason, by)
    purge(cursor, source, 'bt', link['IDBT'], reason, by)
    content = {LINES: lines, TABLE: [link]}
    cursor.execute('INSERT INTO ' + ARCHIVE + ' (SubunitId, GestiuneId, ExercitiuId, Familie, IdDocument, Numar, DataDoc, Motiv, AnulatDe, AnulatLa, Continut) '
                   'VALUES (%s,%s,%s,%s,%s,%s,%s,%s,%s,NOW(),%s)',
                   (*_scope(source), 'bu', link['ID'], 'BU %d' % link['ID'], link['DATA_BU'], reason or link.get('MOTIV_ANULARE'), by,
                    json.dumps(content, ensure_ascii=False, default=str)))
    cursor.execute('DELETE FROM ' + LINES + ' WHERE BU_ID=%s', (link['ID'],))
    cursor.execute('DELETE FROM ' + TABLE + ' WHERE ID=%s', (link['ID'],))
