// The «Extrase de cont» page of the portal (menu Extrase). VIEW ONLY: the web counterpart of the
// desktop ExtraseForm / ExtrasePanel (mode «antet + operatii») without the download from FOREXE.
//
//   left   the tree «Toate extrasele» > month > day. A day is a DataBanca of the operations; a header
//          (FX_Extrase_H) sits under every day on which one of its operations has a DataBanca, and a
//          header with no operation sits on its statement's date
//   right  root / month: the headers on top, the operations below (all of the node's until a header is
//          chosen, then the chosen header's); a day: that day's operations and, under them, the chosen
//          operation in full

import { DataGrid } from '../dgv/datagrid.js';
import { TreeView } from '../components/treeview/treeview.js';

const $ = (id) => document.getElementById(id);
const money = { valueType: 'number', format: 'standard', aggregate: 'sum' };
const plain = { valueType: 'number', format: 'standard' };
const rowHeight = () => (window.matchMedia('(max-width: 900px)').matches ? 20 : 23);

const MONTHS = ['Ianuarie', 'Februarie', 'Martie', 'Aprilie', 'Mai', 'Iunie', 'Iulie',
  'August', 'Septembrie', 'Octombrie', 'Noiembrie', 'Decembrie'];

const HEADER_COLUMNS = [
  { key: 'data_extras', title: 'Data', valueType: 'datetime', format: 'shortDate' },
  { key: 'numar_extras', title: 'Nr.' },
  { key: 'clsf', title: 'Clasificație' },
  { key: 'denumire', title: 'Denumire', width: 220 },
  { key: 'cont', title: 'Cont' },
  { key: 'cod_iban', title: 'IBAN', width: 200 },
  { key: 'sid', title: 'Sold inițial D', ...plain },
  { key: 'sic', title: 'Sold inițial C', ...plain },
  { key: 'rpd', title: 'Rulaj D', ...plain },
  { key: 'rpc', title: 'Rulaj C', ...plain },
  { key: 'tsd', title: 'Total sume D', ...plain },
  { key: 'tsc', title: 'Total sume C', ...plain },
  { key: 'sfd', title: 'Sold final D', ...plain },
  { key: 'sfc', title: 'Sold final C', ...plain },
];

const OPERATION_COLUMNS = [
  { key: 'data_banca', title: 'Data banca', valueType: 'datetime', format: 'shortDate' },
  { key: 'data_doc', title: 'Data doc.', valueType: 'datetime', format: 'shortDate' },
  { key: 'clsf', title: 'Clasificație' },
  { key: 'nr_doc', title: 'Nr. doc.' },
  { key: 'referinta', title: 'Referință', width: 160 },
  { key: 'platitor_nume', title: 'Plătitor', width: 220 },
  { key: 'platitor_cui', title: 'CUI' },
  { key: 'platitor_iban', title: 'IBAN plătitor', width: 200 },
  { key: 'suma_debit', title: 'Debit', ...money },
  { key: 'suma_credit', title: 'Credit', ...money },
  { key: 'cod_contract', title: 'Cod angajament' },
  { key: 'rand_contract', title: 'Indicator' },
  { key: 'cod_program', title: 'Cod program' },
  { key: 'cod_ai', title: 'Cod AI' },
  { key: 'explicatii', title: 'Explicații', width: 300 },
];

const dayKey = (iso) => String(iso || '').slice(0, 10);
const roDate = (iso) => (iso ? dayKey(iso).split('-').reverse().join('.') : '');
const roMoney = (n) => new Intl.NumberFormat('ro-RO', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(n || 0);
const byDateThen = (dateKey, idKey) => (a, b) => (
  (a[dateKey] ? String(a[dateKey]) : '9999').localeCompare(b[dateKey] ? String(b[dateKey]) : '9999') || a[idKey] - b[idKey]
);

/**
 * @param {{call: Function, fail: Function, say: Function, hasUnit: () => boolean}} deps
 *   call(method, path) -> {ok, status, data}; fail(r, fallback) shows the server's sentence.
 */
export function createExtrasePage({ call, fail, say, hasUnit }) {
  let tree = null;
  let gTop = null;
  let gOps = null;
  let loaded = false;
  let seq = 0;
  let bound = false;
  let antetById = new Map();
  let opsByAntet = new Map();
  const nodes = new Map(); // tree id -> {title, isDay, antete, operatii}

  const set = (id, text) => { $(id).textContent = text == null ? '' : String(text); };

  // ------------------------------------------------------------ building the tree
  function build(antete, operatiuni) {
    antetById = new Map(antete.map((a) => [a.idexh, a]));
    opsByAntet = new Map();
    operatiuni.forEach((o) => {
      if (o.idfxh == null) return;
      if (!opsByAntet.has(o.idfxh)) opsByAntet.set(o.idfxh, []);
      opsByAntet.get(o.idfxh).push(o);
    });
    nodes.clear();

    const orderedAntete = (list) => [...list].sort((a, b) => (
      (a.data_extras ? String(a.data_extras) : '9999').localeCompare(b.data_extras ? String(b.data_extras) : '9999')
      || String(a.clsf).localeCompare(String(b.clsf)) || a.idexh - b.idexh));
    const orderedOps = (list) => [...list].sort(byDateThen('data_banca', 'idfxe'));

    // the days a header sits on: its operations' DataBanca; none -> its statement date
    const headersByDay = new Map();
    orderedAntete(antete).forEach((a) => {
      const days = [...new Set((opsByAntet.get(a.idexh) || []).filter((o) => o.data_banca).map((o) => dayKey(o.data_banca)))];
      if (!days.length && a.data_extras) days.push(dayKey(a.data_extras));
      days.forEach((d) => {
        if (!headersByDay.has(d)) headersByDay.set(d, []);
        headersByDay.get(d).push(a);
      });
    });
    const opsByDay = new Map();
    operatiuni.filter((o) => o.data_banca).forEach((o) => {
      const d = dayKey(o.data_banca);
      if (!opsByDay.has(d)) opsByDay.set(d, []);
      opsByDay.get(d).push(o);
    });
    opsByDay.forEach((list, d) => { if (!headersByDay.has(d)) headersByDay.set(d, []); }); // orphans still get a day

    const days = [...headersByDay.keys()].sort();
    const years = new Set(days.map((d) => d.slice(0, 4)));
    const months = new Map();
    days.forEach((d) => {
      const m = d.slice(0, 7);
      if (!months.has(m)) months.set(m, []);
      months.get(m).push(d);
    });

    nodes.set('all', { title: 'Toate extrasele', isDay: false, antete: orderedAntete(antete), operatii: orderedOps(operatiuni) });
    const children = [...months.entries()].map(([m, list]) => {
      const [y, mm] = m.split('-');
      const title = MONTHS[Number(mm) - 1] + (years.size > 1 ? ` ${y}` : '');
      const monthAntete = orderedAntete([...new Set(list.flatMap((d) => headersByDay.get(d)))]);
      const monthOps = orderedOps(list.flatMap((d) => opsByDay.get(d) || []));
      nodes.set(`L_${m}`, { title, isDay: false, antete: monthAntete, operatii: monthOps });
      return {
        id: `L_${m}`,
        label: title,
        bold: true,
        children: list.map((d) => {
          const label = roDate(d);
          nodes.set(`Z_${d}`, { title: label, isDay: true, antete: [], operatii: orderedOps(opsByDay.get(d) || []) });
          return { id: `Z_${d}`, label };
        }),
      };
    });
    return [{ id: 'all', label: 'Toate extrasele', bold: true, children }];
  }

  // ------------------------------------------------------------ the right side
  function withClsf(list) {
    return list.map((o) => ({ ...o, clsf: (antetById.get(o.idfxh) || {}).clsf || '' }));
  }

  function destroyGrids() {
    [gTop, gOps].forEach((g) => g && g.destroy());
    gTop = null;
    gOps = null;
  }

  function showDetail(o) {
    set('ed-nr', o && o.nr_doc);
    set('ed-banca', o && roDate(o.data_banca));
    set('ed-doc', o && roDate(o.data_doc));
    set('ed-ref', o && o.referinta);
    set('ed-refd', o && o.referinta_dest);
    set('ed-plat', o && o.platitor_nume);
    set('ed-cui', o && o.platitor_cui);
    set('ed-iban', o && o.platitor_iban);
    set('ed-deb', o && roMoney(o.suma_debit));
    set('ed-cred', o && roMoney(o.suma_credit));
    set('ed-cod', o && o.cod_contract);
    set('ed-ind', o && o.rand_contract);
    set('ed-prog', o && o.cod_program);
    set('ed-expl', o && o.explicatii);
  }

  function showNode(id) {
    const node = nodes.get(id);
    if (!node) return;
    destroyGrids();
    set('ext-title', node.title);
    const ops = withClsf(node.operatii);
    const rh = rowHeight();
    if (node.isDay) {
      $('grid-ext-ops').hidden = true;
      $('ext-detail').hidden = false;
      showDetail(null);
      gTop = new DataGrid($('grid-ext-top'), {
        columns: OPERATION_COLUMNS, rows: ops, rowHeight: rh, footer: true, footerCaption: '{0} operațiuni',
        emptyText: 'Nu există operațiuni în această zi.', onSelect: (row) => showDetail(row),
      });
      return;
    }
    $('grid-ext-ops').hidden = false;
    $('ext-detail').hidden = true;
    gOps = new DataGrid($('grid-ext-ops'), {
      columns: OPERATION_COLUMNS, rows: ops, rowHeight: rh, footer: true, footerCaption: '{0} operațiuni',
      emptyText: 'Nu există operațiuni.',
    });
    gTop = new DataGrid($('grid-ext-top'), {
      columns: HEADER_COLUMNS, rows: node.antete, rowHeight: rh, footer: true, footerCaption: '{0} extrase',
      emptyText: 'Nu există extrase de cont.',
      onSelect: (row) => gOps.setRows(withClsf(row ? (opsByAntet.get(row.idexh) || []) : node.operatii)),
    });
  }

  // ------------------------------------------------------------ loading
  async function load() {
    if (!hasUnit()) {
      say('Alegeți unitatea ca să vedeți extrasele.');
      return false;
    }
    const mine = (seq += 1);
    set('ext-count', 'Se încarcă...');
    const r = await call('GET', '/api/portal/date/extrase');
    if (mine !== seq) return false;
    if (!r.ok) {
      set('ext-count', '');
      fail(r, 'Extrasele nu au putut fi citite.');
      return false;
    }
    const antete = r.data.antete || [];
    const operatiuni = r.data.operatiuni || [];
    tree.setData(build(antete, operatiuni));
    set('ext-count', `${antete.length} extrase, ${operatiuni.length} operațiuni`);
    loaded = true;
    tree.selectNodeById('all');
    return true;
  }

  function bind() {
    if (bound) return;
    bound = true;
    tree = new TreeView($('tree-ext'), {
      inline: true,
      selectParents: true,
      searchPlaceholder: 'Căutați (minimum 3 caractere)…',
      onSelect: (sel) => showNode(String(sel.id)),
    });
  }

  async function show() {
    bind();
    say('');
    $('card-app').classList.add('is-extrase');
    $('ext-view').hidden = false;
    if (!loaded) await load();
  }

  async function reload() {
    loaded = false;
    if ($('ext-view').hidden) return;
    destroyGrids();
    await show();
  }

  function hide() {
    seq += 1;
    $('card-app').classList.remove('is-extrase');
    $('ext-view').hidden = true;
  }

  return { show, reload, hide };
}
