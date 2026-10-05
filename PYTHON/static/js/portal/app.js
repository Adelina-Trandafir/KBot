// Slice 0110-06 -- the signed-in page of the web area: unit / year / source pickers (the existing
// Combobox), the list of angajamente and, for the one picked, its Sumar, Rezervari, Receptii and
// Plati in the read-only DataGrid. VIEW ONLY: nothing here sends data to the server except the
// choice of the unit; the data routes are GET (routes/portal/date.py).
//
// The shapes of the answers are those of the desktop app's routes (routes/forexe/*), because the
// portal calls the same functions.

import { Combobox } from '../components/combobox/combobox.js';
import { DataGrid } from '../dgv/datagrid.js';
import { TreeView } from '../components/treeview/treeview.js';
import { createPdfView } from './pdfview.js';
import { createClasificatiiPage } from './clasificatii.js';
import { createParteneriPage } from './parteneri.js';
import { createExtrasePage } from './extrase.js';

const ALL_SOURCES = '*';
const MONTH_KEY = /^\d+\.(\d+\.\d+)/; // dd.MM.yyyy -> MM.yyyy

const $ = (id) => document.getElementById(id);

// ---------------------------------------------------------------- column sets
const money = { valueType: 'number', format: 'standard', aggregate: 'sum' };

// The angajamente tree is flat, as in the desktop app: one node per angajament, a status mark
// in front (the desktop's coloured status icon, by the same words of Stare, first match wins),
// bold when it has indicators, red when a reception chain does not close.
const STATUS_MARKS = [
  ['derulare', '🟢'], ['anulat', '🔴'], ['reziliat', '🔴'], ['suspendat', '🟠'],
  ['initial', '⚪'], ['arhivat', '🔵'], ['manual', '⚪'], ['definitivare', '🔵'],
];

function statusMark(stare) {
  const norm = String(stare || '').normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLowerCase();
  const hit = STATUS_MARKS.find(([token]) => norm.includes(token));
  return hit ? hit[1] : '⚪';
}

/** Row height of the grids (group bands included): tight, tighter still on a phone. */
function gridRowHeight() {
  return window.matchMedia('(max-width: 900px)').matches ? 20 : 23;
}

const nameCmp = new Intl.Collator('ro', { sensitivity: 'base', numeric: true });

/**
 * The list order, as the desktop's tree sorts it: by name, or by the day the angajament was made
 * (the ones with no such day go last, by name); the code breaks every tie so two loads never swap rows.
 */
function sortRows(rows, by, desc) {
  const sign = desc ? -1 : 1;
  const name = (r) => String(r.Descriere || '').trim();
  const cod = (r) => String(r.CodAngajament || '');
  const tie = (a, b) => nameCmp.compare(name(a), name(b)) || cod(a).localeCompare(cod(b));
  if (by === 'name') return [...rows].sort((a, b) => sign * nameCmp.compare(name(a), name(b)) || cod(a).localeCompare(cod(b)));
  const day = (r) => (r.DataAngajamentNou ? Date.parse(r.DataAngajamentNou) : NaN);
  const dated = rows.filter((r) => !Number.isNaN(day(r)));
  const undated = rows.filter((r) => Number.isNaN(day(r)));
  dated.sort((a, b) => sign * (day(a) - day(b)) || tie(a, b));
  undated.sort(tie);
  return dated.concat(undated);
}

/** One tree row of GET /tree -> one TreeView node. */
function angajamentNode(row) {
  const cod = String(row.CodAngajament || '');
  const desc = String(row.Descriere || '').trim().toUpperCase();
  const lines = [row.Descriere, `Cod: ${cod}`, row.Stare ? `Stare: ${row.Stare}` : '',
    row.Surse ? `Surse: ${String(row.Surse).split(';').join(', ')}` : ''].filter(Boolean);
  return {
    id: cod,
    label: `${statusMark(row.Stare)} ${desc || cod}`,
    badge: cod,
    bold: !!row.AreIndicatori,
    error: Array.isArray(row.LantNeinchis) ? row.LantNeinchis.length > 0 : !!row.LantNeinchis,
    tooltip: lines.join('\n'),
    keywords: `${cod} ${row.Surse || ''}`,
  };
}

const TABS = {
  sumar: {
    path: 'sumar',
    rows: (d) => d.rows || [],
    columns: [
      { key: 'clsf', title: 'Clasificație' },
      { key: 'cod_indicator', title: 'Indicator' },
      { key: 'partener', title: 'Partener' },
      { key: 'credit_bug', title: 'Credit bugetar', ...money },
      { key: 'total_rezervari', title: 'Rezervări', ...money },
      { key: 'total_receptii', title: 'Recepții', ...money },
      { key: 'total_plati', title: 'Plăți', ...money },
      { key: 'total_revizii', title: 'Revizii', ...money },
      { key: 'total_ordonantari', title: 'Ordonanțări', ...money },
    ],
    options: { frozen: 1, footer: true, footerCaption: '{0} indicatori' },
    groups: [],
  },
  istoric: {
    path: 'istoric',
    rows: (d) => d.randuri || [],
    columns: [
      { key: 'data_fx', title: 'Data', valueType: 'datetime', format: 'generalDate' },
      { key: 'clsf', title: 'Clasificație' },
      { key: 'tip_rand', title: 'Tip' },
      { key: 'cod_indicator', title: 'Indicator' },
      { key: 'descriere', title: 'Descriere', width: 240 },
      { key: 'val_rezervare_i', title: 'Rez. inițială', ...money },
      { key: 'val_rezervare_d', title: 'Rez. definitivă', ...money },
      { key: 'val_rezervare_dif', title: 'Diferență', ...money },
      { key: 'val_ang_leg', title: 'Angajament legal', ...money },
      { key: 'val_receptie', title: 'Recepție', ...money },
      { key: 'val_plata', title: 'Plată', ...money },
      { key: 'doc', title: 'Document' },
      { key: 'observatii', title: 'Observații', width: 240 },
    ],
    options: { footer: true, footerCaption: '{0} rânduri' },
    groups: [],
  },
  rezervari: {
    path: 'rezervari',
    rows: (d) => d.rows || [],
    columns: [
      { key: 'data_rezervare', title: 'Data', valueType: 'datetime', format: 'shortDate' },
      { key: 'clsf', title: 'Clasificație' },
      { key: 'denumire', title: 'Denumire', width: 240 },
      { key: 'cod_indicator', title: 'Indicator' },
      { key: 'r_credit_bug', title: 'Credit bugetar', ...money },
      { key: 'r_initiala', title: 'Inițială', ...money },
      { key: 'r_valoare', title: 'Valoare', ...money },
      { key: 'r_definitiva', title: 'Definitivă', ...money },
      { key: 'are_ddf', title: 'DDF', valueType: 'boolean', format: 'yesNo' },
    ],
    options: { footer: true, footerCaption: '{0} rezervări' },
    groups: [{ key: 'data_rezervare', dir: 'asc', keyPattern: MONTH_KEY, headerCaption: 'Luna {1} ({2})', showFooter: false, headerAggregates: true }],
  },
  receptii: {
    path: 'receptii',
    rows: (d) => d.receptii || [],
    columns: [
      { key: 'data_r', title: 'Data recepției', valueType: 'datetime', format: 'shortDate' },
      { key: 'nrcrt_r', title: 'Nr.', valueType: 'number' },
      { key: 'descriere_r', title: 'Descriere', width: 220 },
      { key: 'data_h', title: 'Versiune antet', valueType: 'datetime', format: 'generalDate' },
      { key: 'clsf', title: 'Clasificație' },
      { key: 'denumire', title: 'Denumire', width: 220 },
      { key: 'cod_indicator', title: 'Indicator' },
      { key: 'valoare', title: 'Valoare', ...money },
      { key: 'dif', title: 'Diferență', ...money },
      { key: 'suma_antet', title: 'Suma antet', valueType: 'number', format: 'standard' },
      { key: 'este_stergere', title: 'Ștergere', valueType: 'boolean', format: 'yesNo' },
    ],
    options: { footer: true, footerCaption: '{0} rânduri' },
    groups: [{ key: 'data_r', dir: 'asc', keyPattern: MONTH_KEY, headerCaption: 'Luna {1} ({2})', showFooter: false }],
  },
  extrase: {
    path: 'extrase',
    // the operations of the angajament; the statement's date and classification come from its header
    rows: (d) => {
      const heads = new Map((d.antete || []).map((a) => [a.idexh, a]));
      return (d.operatiuni || []).map((o) => {
        const a = heads.get(o.idfxh) || {};
        return { ...o, clsf: a.clsf || '', data_extras: a.data_extras || null };
      });
    },
    columns: [
      { key: 'data_banca', title: 'Data banca', valueType: 'datetime', format: 'shortDate' },
      { key: 'data_extras', title: 'Data extras', valueType: 'datetime', format: 'shortDate' },
      { key: 'clsf', title: 'Clasificație' },
      { key: 'nr_doc', title: 'Nr. doc.' },
      { key: 'platitor_nume', title: 'Plătitor', width: 220 },
      { key: 'suma_debit', title: 'Debit', ...money },
      { key: 'suma_credit', title: 'Credit', ...money },
      { key: 'cod_contract', title: 'Cod angajament' },
      { key: 'rand_contract', title: 'Indicator' },
      { key: 'referinta', title: 'Referință', width: 160 },
      { key: 'explicatii', title: 'Explicații', width: 280 },
    ],
    options: { footer: true, footerCaption: '{0} operațiuni' },
    groups: [{ key: 'data_banca', dir: 'asc', keyPattern: MONTH_KEY, headerCaption: 'Luna {1} ({2})', showFooter: false, headerAggregates: true }],
  },
  plati: {
    path: 'plati',
    rows: (d) => d.plati || [],
    columns: [
      { key: 'data_plata', title: 'Data plății', valueType: 'datetime', format: 'shortDate' },
      { key: 'nr_op', title: 'Nr. OP' },
      { key: 'clsf', title: 'Clasificație' },
      { key: 'denumire', title: 'Denumire', width: 220 },
      { key: 'cod_indicator', title: 'Indicator' },
      { key: 'suma', title: 'Suma', ...money },
      { key: 'are_ord', title: 'Ordonanțat', valueType: 'boolean', format: 'yesNo' },
      { key: 'platitor_nume', title: 'Plătitor', width: 200 },
      { key: 'nr_doc_extras', title: 'Nr. document' },
      { key: 'explicatii', title: 'Explicații', width: 260 },
    ],
    options: { footer: true, footerCaption: '{0} plăți' },
    groups: [{ key: 'data_plata', dir: 'asc', keyPattern: MONTH_KEY, headerCaption: 'Luna {1} ({2})', showFooter: false, headerAggregates: true }],
  },
};

// The signed documents of one angajament (slice 0110-08): one list for the three kinds, a viewer under it.
const DOC_COLUMNS = [
  { key: 'tip', title: 'Document' },
  { key: 'nr', title: 'Număr' },
  { key: 'data', title: 'Data', valueType: 'datetime', format: 'shortDate' },
  { key: 'suma', title: 'Suma', valueType: 'number', format: 'standard' },
  { key: 'semn', title: 'Semnături' },
  { key: 'pdf', title: 'PDF semnat', valueType: 'boolean', format: 'yesNo' },
];
const DOC_ORDER = { DDF: 0, ORD: 1, 'Nota CAB': 2 };

/** The three list answers -> one array of rows for the documents grid. */
function documentRows(ddf, ord, note) {
  const rows = [];
  (ddf.revizii || []).forEach((r) => rows.push({
    tip: 'DDF', nr: `Rev. ${r.numar_rev}`, data: r.data_rev, suma: r.total_revizie, semn: r.semnatura || '',
    pdf: !!r.pdf_sha256, path: `ddf-pdf/${r.idrev}`,
  }));
  (ord.ordonantari || []).forEach((r) => rows.push({
    tip: 'ORD', nr: String(r.nr_ord), data: r.data_ord, suma: r.total_ord, semn: r.semnatura || '',
    pdf: !!r.pdf_sha256, path: `ord-pdf/${r.idordp}`,
  }));
  (note.note || []).forEach((r) => rows.push({
    tip: 'Nota CAB', nr: String(r.nr_nota), data: r.data_nota, suma: null, semn: r.semnatura || '',
    pdf: !!r.pdf_sha256, path: `nc-pdf/${r.idnc}`,
  }));
  return rows.sort((a, b) => DOC_ORDER[a.tip] - DOC_ORDER[b.tip] || String(a.data).localeCompare(String(b.data)));
}

// ---------------------------------------------------------------- the page
/**
 * @param {{call: Function, callBytes: Function, onUnauthorized: Function}} deps
 *   call(method, path, body) -> {ok, status, data}, as in portal.js;
 *   callBytes(path) -> the same, with the raw bytes (ArrayBuffer) as `data` when ok.
 */
export function createApp({ call, callBytes, onUnauthorized }) {
  let me = null;
  let unitCombo = null;
  let anCombo = null;
  let ssCombo = null;
  let treeAng = null;
  const rowsByCod = new Map(); // CodAngajament -> the tree row
  let treeRows = []; // the rows as the server sent them
  const order = { by: 'name', desc: false }; // the list order chosen in the options menu
  let gridTab = null;
  let tab = 'sumar';
  let picked = null; // the selected angajament row
  let treeSeq = 0; // the answer of an older request must not replace a newer one
  let tabSeq = 0;
  const cache = new Map(); // `${cod}|${tab}` -> rows of the tab
  let gridDocs = null;
  let pdfView = null;
  let docSeq = 0; // the same rule for documents
  let docBlob = null; // {bytes, name} of the document shown, for the download button

  const say = (text) => {
    $('msg-app').textContent = text || '';
    $('msg-app').hidden = !text;
  };
  const hint = (text) => {
    $('app-lead').textContent = text || '';
    $('app-lead').hidden = !text;
  };

  // A failed GET: 401 means the session is over; anything else shows the server's sentence.
  function fail(r, fallback) {
    if (r.status === 401) {
      onUnauthorized(r.data.error);
      return;
    }
    say(r.data.error || fallback);
  }

  function makeCombo(id, placeholder, onSelect) {
    const combo = new Combobox($(id), { readonly: true, placeholder, staticData: [], onSelect });
    combo.input.id = `${id}-input`;
    // no visible label (the chosen value says what it is); the name stays for screen readers
    const label = document.querySelector(`label[for="${id}"]`);
    if (label) {
      combo.input.setAttribute('aria-label', label.textContent.trim());
      label.remove();
    }
    return combo;
  }

  function setOptions(combo, items, value) {
    combo.options.staticData = items;
    const hit = items.find((i) => i.value === value);
    if (hit) combo.setValue(hit.value, hit.label);
    else combo.clear();
  }

  function ensureWidgets() {
    if (unitCombo) return;
    unitCombo = makeCombo('unit-combo', 'Alegeți unitatea', (value) => chooseUnit(value));
    anCombo = makeCombo('an-combo', 'An', () => {
      fillSources();
      if (activePage === 'clasificatii') pages.clasificatii.reload();
      else if (!activePage) loadTree();
    });
    ssCombo = makeCombo('ss-combo', 'Sursa', () => loadTree());
    treeAng = new TreeView($('grid-ang'), {
      inline: true,
      searchPlaceholder: 'Căutați (minimum 3 caractere)…',
      onSelect: (sel) => pick(rowsByCod.get(String(sel.id))),
    });
    pdfView = createPdfView($('pdf-host'));
    $('btn-docdl').addEventListener('click', downloadDoc);
    document.querySelectorAll('.pa__tabs button[data-tab]').forEach((btn) => {
      btn.addEventListener('click', () => switchTab(btn.dataset.tab));
    });
    // The unit field (label + combo) is always exactly as wide as the angajamente tree's search box.
    const unitField = document.querySelector('.pa__f--unit');
    const matchTreeWidth = () => {
      if (window.matchMedia('(max-width: 900px)').matches) {
        // phone: one line, the css decides the widths
        unitField.style.cssText = '';
        document.querySelector('.pa__f--an').style.marginLeft = '';
        return;
      }
      const w = $('grid-ang').getBoundingClientRect().width;
      if (w <= 0) return; // the list is hidden: keep the last width
      unitField.style.flex = '0 0 auto';
      unitField.style.minWidth = '0';
      unitField.style.maxWidth = 'none';
      unitField.style.width = `${Math.round(w)}px`;
      // pc: the year field starts exactly where the data card (pa__right) starts
      const an = document.querySelector('.pa__f--an');
      an.style.marginLeft = '';
      if (window.matchMedia('(min-width: 901px)').matches && !$('pa-body').hidden) {
        const bar = document.querySelector('.pa__bar');
        const gap = parseFloat(getComputedStyle(bar).columnGap) || 0;
        const shift = document.querySelector('.pa__right').getBoundingClientRect().left
          - unitField.getBoundingClientRect().right - gap;
        if (shift > 0) an.style.marginLeft = `${Math.round(shift)}px`;
      }
    };
    new ResizeObserver(matchTreeWidth).observe($('grid-ang'));
    setupTreeMenu();
    $('btn-back').addEventListener('click', () => $('card-app').classList.remove('is-detail'));
  }

  // ------------------------------------------------------------ unit -> year / source
  function fillPeriods() {
    const years = [...new Set(me.periods.map((p) => p.AN))].sort((a, b) => b - a);
    setOptions(anCombo, years.map((y) => ({ value: String(y), label: String(y) })), years.length ? String(years[0]) : '');
    fillSources();
  }

  function fillSources() {
    const year = Number(anCombo.getSelectedValue());
    const sources = [...new Set(me.periods.filter((p) => p.AN === year).map((p) => p.SS))].sort();
    const items = [{ value: ALL_SOURCES, label: 'Toate...' }, ...sources.map((s) => ({ value: s, label: s }))];
    setOptions(ssCombo, items, ALL_SOURCES);
  }

  async function chooseUnit(dbName) {
    say('');
    if (!dbName || dbName === me.db_name) return;
    const r = await call('POST', '/api/portal/unit', { db_name: dbName });
    if (!r.ok) {
      fail(r, 'Unitatea nu a putut fi deschisă.');
      setOptions(unitCombo, unitItems(), me.db_name);
      return;
    }
    me.db_name = r.data.db_name;
    me.role = r.data.role;
    me.periods = r.data.periods;
    cache.clear();
    resetSelection();
    fillPeriods();
    if (activePage) await pages[activePage].reload();
    else await loadTree();
  }

  const unitItems = () => me.units.map((u) => ({ value: u.DC, label: u.NumeUnitate }));

  // ------------------------------------------------------------ list of angajamente
  async function loadTree() {
    say('');
    const an = anCombo.getSelectedValue();
    const ss = ssCombo.getSelectedValue() || ALL_SOURCES;
    if (!me.db_name || !an) {
      $('pa-body').hidden = true;
      hint(me.db_name ? 'Unitatea nu are perioade configurate.' : 'Alegeți unitatea ca să vedeți angajamentele.');
      return;
    }
    hint('');
    $('pa-body').hidden = false;
    const mine = (treeSeq += 1);
    $('app-count').textContent = 'Se încarcă...';
    const r = await call('GET', `/api/portal/date/tree?an=${encodeURIComponent(an)}&ss=${encodeURIComponent(ss)}`);
    if (mine !== treeSeq) return; // a newer choice is already on its way
    if (!r.ok) {
      $('app-count').textContent = '';
      fail(r, 'Lista angajamentelor nu a putut fi citită.');
      return;
    }
    cache.clear();
    resetSelection();
    treeRows = r.data.rows;
    rowsByCod.clear();
    treeRows.forEach((row) => rowsByCod.set(String(row.CodAngajament), row));
    paintTree();
    if (!treeRows.length) {
      $('grid-ang').querySelector('.treeview-dropdown').innerHTML =
        '<div class="treeview-no-results">Niciun angajament pentru anul și sursa alese.</div>';
    }
    $('app-count').textContent = `${r.data.count} angajamente`;
  }

  /** Puts the rows in the chosen order into the tree; the picked row stays picked. */
  function paintTree() {
    treeAng.setData(sortRows(treeRows, order.by, order.desc).map(angajamentNode));
    document.querySelectorAll('#tree-menu button').forEach((b) => {
      const on = b.dataset.sort === order.by;
      b.setAttribute('aria-checked', String(on));
      const base = b.dataset.sort === 'name' ? 'Ordonare după nume' : 'Ordonare după dată';
      b.textContent = on ? `${base} ${order.desc ? '↓' : '↑'}` : base;
    });
  }

  function setupTreeMenu() {
    const menu = $('tree-menu');
    const opener = $('btn-tree-opt');
    const close = () => {
      menu.hidden = true;
      opener.setAttribute('aria-expanded', 'false');
    };
    opener.addEventListener('click', (e) => {
      e.stopPropagation();
      const open = menu.hidden;
      menu.hidden = !open;
      opener.setAttribute('aria-expanded', String(open));
    });
    menu.addEventListener('click', (e) => {
      const b = e.target.closest('button[data-sort]');
      if (!b) return;
      // the same choice again flips the direction
      if (order.by === b.dataset.sort) order.desc = !order.desc;
      else {
        order.by = b.dataset.sort;
        order.desc = false;
      }
      paintTree();
      close();
    });
    document.addEventListener('click', (e) => {
      if (!menu.hidden && !menu.contains(e.target)) close();
    });
    document.addEventListener('keydown', (e) => {
      if (e.key === 'Escape') close();
    });
    paintTree();
  }

  function resetSelection() {
    picked = null;
    $('card-app').classList.remove('is-detail');
    if (treeAng) treeAng.clearSelection();
    $('pa-sel').textContent = 'Alegeți un angajament din listă.';
    if (gridTab) gridTab.setRows([]);
  }

  // ------------------------------------------------------------ one angajament
  async function pick(row) {
    if (!row) return;
    picked = row;
    $('card-app').classList.add('is-detail'); // phone: the data replaces the list (CSS only reacts there)
    $('pa-sel').textContent = `${row.CodAngajament} — ${row.Descriere || ''}${row.Stare ? ` (${row.Stare})` : ''}`;
    await loadTab();
  }

  function switchTab(name) {
    if (!TABS[name] && name !== 'documente') throw new Error(`Unknown tab: ${name}`);
    tab = name;
    const docs = name === 'documente';
    $('grid-tab').hidden = docs;
    $('docs-tab').hidden = !docs;
    document.querySelectorAll('.pa__tabs button[data-tab]').forEach((b) => {
      const on = b.dataset.tab === name;
      b.classList.toggle('is-active', on);
      b.setAttribute('aria-selected', String(on));
    });
    loadTab();
  }

  function paintTab(def, rows) {
    if (gridTab) gridTab.destroy();
    gridTab = new DataGrid($('grid-tab'), {
      columns: def.columns,
      rows,
      groups: def.groups,
      rowHeight: gridRowHeight(),
      emptyText: 'Nu există date pentru acest angajament.',
      ...def.options,
    });
  }

  async function loadTab() {
    if (!picked) return;
    say('');
    if (tab === 'documente') {
      await loadDocs();
      return;
    }
    const def = TABS[tab];
    const key = `${picked.CodAngajament}|${tab}`;
    const mine = (tabSeq += 1);
    if (cache.has(key)) {
      paintTab(def, cache.get(key));
      return;
    }
    const r = await call('GET', `/api/portal/date/${def.path}?cod=${encodeURIComponent(picked.CodAngajament)}`);
    if (mine !== tabSeq) return;
    if (!r.ok) {
      fail(r, 'Datele nu au putut fi citite.');
      return;
    }
    const rows = def.rows(r.data);
    cache.set(key, rows);
    paintTab(def, rows);
  }

  // ------------------------------------------------------------ documents (slice 0110-08)
  const docMsg = (text) => {
    $('msg-doc').textContent = text || '';
    $('msg-doc').hidden = !text;
  };

  async function loadDocs() {
    const mine = (docSeq += 1);
    docMsg('');
    $('btn-docdl').hidden = true;
    docBlob = null;
    await pdfView.clear();
    const cod = encodeURIComponent(picked.CodAngajament);
    const key = `${picked.CodAngajament}|documente`;
    let rows = cache.get(key);
    if (!rows) {
      const [a, b, c] = await Promise.all(['ddf', 'ord', 'note'].map((n) => call('GET', `/api/portal/date/${n}?cod=${cod}`)));
      if (mine !== docSeq) return;
      const bad = [a, b, c].find((r) => !r.ok);
      if (bad) {
        fail(bad, 'Documentele nu au putut fi citite.');
        return;
      }
      rows = documentRows(a.data, b.data, c.data);
      cache.set(key, rows);
    }
    if (gridDocs) gridDocs.destroy();
    gridDocs = new DataGrid($('grid-docs'), {
      columns: DOC_COLUMNS,
      rows,
      rowHeight: gridRowHeight(),
      emptyText: 'Angajamentul nu are documente DDF, ORD sau note.',
      onSelect: (row) => openDoc(row),
    });
    // One document: show it at once; several: the person chooses.
    const openable = rows.filter((r) => r.pdf);
    if (openable.length === 1) openDoc(openable[0]);
    else if (rows.length) docMsg('Alegeți un document din listă.');
  }

  async function openDoc(row) {
    if (!row) return;
    const mine = (docSeq += 1);
    docMsg('');
    $('btn-docdl').hidden = true;
    docBlob = null;
    await pdfView.clear();
    if (!row.pdf) {
      docMsg('Documentul nu are un PDF semnat pe server. Îl găsiți în K-BOT.');
      return;
    }
    docMsg('Se încarcă documentul...');
    const r = await callBytes(`/api/portal/date/${row.path}`);
    if (mine !== docSeq) return;
    if (!r.ok) {
      docMsg('');
      fail(r, 'PDF-ul nu a putut fi citit.');
      return;
    }
    docBlob = { bytes: r.data, name: fileName(row) };
    $('btn-docdl').hidden = false;
    try {
      await pdfView.open(r.data);
    } catch (err) {
      if (mine === docSeq) docMsg(err.message);
      return;
    }
    if (mine === docSeq) docMsg('');
  }

  function fileName(row) {
    const clean = (t) => String(t).normalize('NFD').replace(/[^A-Za-z0-9._-]+/g, '_').replace(/^_+|_+$/g, '');
    return `${clean(row.tip)}_${clean(row.nr)}_${clean(picked.CodAngajament)}.pdf`;
  }

  function downloadDoc() {
    if (!docBlob) return;
    const url = URL.createObjectURL(new Blob([docBlob.bytes], { type: 'application/pdf' }));
    const a = document.createElement('a');
    a.href = url;
    a.download = docBlob.name;
    document.body.appendChild(a);
    a.click();
    a.remove();
    setTimeout(() => URL.revokeObjectURL(url), 10000);
  }

  // ------------------------------------------------------------ the pages of the header menu
  // Each page module offers show / hide / reload and is made on first use; only one is open at a time.
  const pages = {};
  let activePage = '';
  const pageMakers = {
    clasificatii: () => createClasificatiiPage({
      call, fail, say, year: () => anCombo.getSelectedValue(), hasUnit: () => !!(me && me.db_name),
    }),
    parteneri: () => createParteneriPage({ call, fail, say, hasUnit: () => !!(me && me.db_name) }),
    extrase: () => createExtrasePage({ call, fail, say, hasUnit: () => !!(me && me.db_name) }),
  };

  function closePage() {
    if (activePage) pages[activePage].hide();
    activePage = '';
  }

  async function openPage(name) {
    if (!pageMakers[name]) throw new Error(`Unknown page: ${name}`);
    if (!me || !me.db_name) {
      say('Alegeți mai întâi unitatea.');
      return;
    }
    closePage();
    if (!pages[name]) pages[name] = pageMakers[name]();
    activePage = name;
    await pages[name].show();
  }

  // ------------------------------------------------------------ public
  return {
    openPage,
    closePage,
    /** Shows the page for the account `meData` ({email, units, db_name, role, periods}). */
    async open(meData) {
      me = meData;
      ensureWidgets();
      say('');
      cache.clear();
      resetSelection();
      setOptions(unitCombo, unitItems(), me.db_name);
      if (me.db_name) {
        fillPeriods();
        await loadTree();
      } else {
        setOptions(anCombo, [], '');
        setOptions(ssCombo, [], '');
        $('pa-body').hidden = true;
        $('app-count').textContent = '';
        hint('Contul dumneavoastră are acces la mai multe unități. Alegeți una din bara de sus.');
      }
    },
  };
}
