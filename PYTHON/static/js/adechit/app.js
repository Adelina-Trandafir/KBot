import { DataGrid, ValueType } from '../dgv/datagrid.js';
import { TreeView } from '../components/treeview/treeview.js';
import { Combobox } from '../components/combobox/combobox.js';
import { DatePicker } from '../components/datepicker/datepicker.js';
import { showMessage } from '../portal/messages.js';
import { bindCatalogs } from './catalogs.js';
import { bindAnnual } from './annual.js';

const $ = (id) => document.getElementById(id);
const state = { context: null, unit: null, monthId: null, groupId: null, rows: [], shownRows: [],
  selected: null, grid: null, monthTree: null, groupTree: null, yearCombo: null, monthCombo: null, groupCombo: null, year: null, loadedYears: new Map(), groupsRequest: null, plan: { hidden: [], nameWidth: null }, ledger: 'receipt', request: 0 };

function idempotencyKey() {
  return globalThis.crypto?.randomUUID?.() || `${Date.now()}-${Math.random().toString(16).slice(2)}`;
}

const PORTAL_TOKEN_KEY = 'kbot-portal-token';

function portalToken() {
  try { return window.sessionStorage.getItem(PORTAL_TOKEN_KEY) || ''; } catch (err) { console.error('[ade] sessionStorage is blocked', err); return ''; }
}

// Back to the portal, which returns here afterwards: to sign in (dropToken) or, with a live
// session and no unit opened yet, to pick the unit.
function toPortal(dropToken) {
  if (dropToken) {
    try { window.sessionStorage.removeItem(PORTAL_TOKEN_KEY); } catch (err) { console.error('[ade] sessionStorage is blocked', err); }
  }
  window.location.replace('/portal?next=' + encodeURIComponent('/adechit'));
}

async function api(path, options = {}) {
  const headers = { Accept: 'application/json', 'X-Ade-Unit': state.unit, 'X-Portal-Token': portalToken(), ...(options.headers || {}) };
  if (options.body) headers['Content-Type'] = 'application/json';
  if (options.method && options.method !== 'GET') headers['Idempotency-Key'] = options.key || idempotencyKey();
  const response = await fetch(path, { ...options, headers });
  const result = await response.json().catch(() => ({}));
  if (response.status === 401 || result.reason === 'UNITATE_NEDESCHISA') { toPortal(response.status === 401); throw new Error(result.error || 'Autentificați-vă.'); }
  if (!response.ok) throw new Error(result.error || `Eroare HTTP ${response.status}`);
  return result;
}

function report(error) { showMessage($('ade-message'), error.message, 'error'); }
function money(value) { return Number(value || 0).toLocaleString('ro-RO', { maximumFractionDigits: 2 }); }
function dateText(value) { return value ? new Intl.DateTimeFormat('ro-RO').format(new Date(`${String(value).slice(0, 10)}T12:00:00`)) : ''; }
function selectedMonth() { return state.context.months.find((row) => row.IDL === state.monthId); }
function selectedGroup() { return state.context.groups.find((row) => row.IDG === state.groupId); }

function syncCloseButton(month = selectedMonth()) {
  const desktopOnly = month?.Luna === 8 && mobileQuery.matches;
  $('ade-close').disabled = !month || Boolean(month.Inchisa) || desktopOnly;
  $('ade-close').title = desktopOnly ? 'Închiderea anuală pentru august se face pe desktop.' : 'Închide luna';
}

function monthNodes() {
  return state.context.years.map((year) => ({
    id: `year:${year}`, label: `Anul ${year}`, bold: true,
    children: state.loadedYears.has(year) ? state.context.months.filter((row) => row.Anul === year)
      .sort((a, b) => b.Luna - a.Luna).map((row) => ({ id: `month:${row.IDL}`,
        label: `${row.LunaT}/${row.Anul}${row.Inchisa ? ' · închisă' : ''}`, error: Boolean(row.Inchisa) }))
      : [{ id: `pending:${year}`, label: 'Se încarcă…' }],
  }));
}

async function loadYear(year) {
  if (!state.loadedYears.has(year)) {
    const pending = api(`/api/adechit/rows/LunaD?Anul=${year}`).then(({ rows }) => {
      state.context.months = [...state.context.months.filter((row) => row.Anul !== year), ...rows];
    });
    state.loadedYears.set(year, pending);
    pending.catch(() => state.loadedYears.delete(year));
  }
  await state.loadedYears.get(year);
  state.monthTree.setData(monthNodes());
  state.monthTree.isTreeRendered = false; state.monthTree.renderTree('');
  syncPickers();
}

async function loadGroups() {
  if (!state.groupsRequest) state.groupsRequest = api('/api/adechit/catalog-data').then(({ groups }) => {
    state.context.groups = [...groups].sort((a, b) => String(a.Grupa || '').localeCompare(String(b.Grupa || ''), 'ro', { sensitivity: 'base' }));
    state.groupTree.setData(state.context.groups.map((row) => ({ id: `group:${row.IDG}`, label: row.Grupa, tooltip: row.Educators || '' })));
    state.groupTree.isTreeRendered = false; state.groupTree.renderTree('');
    syncPickers();
  }).catch((error) => { state.groupsRequest = null; throw error; });
  await state.groupsRequest;
}

function buildTrees() {
  state.monthTree = new TreeView($('ade-month-tree'), { inline: true, autoCollapse: false, selectableLevel: 2,
    showSearchBox: false, onSelect: ({ id }) => { if (String(id).startsWith('month:')) chooseMonth(Number(String(id).split(':')[1])).catch(report); } });
  const months = monthNodes();
  state.monthTree.setData(months);
  const toggle = state.monthTree.toggleNode.bind(state.monthTree);
  state.monthTree.toggleNode = async (id, skip) => {
    try {
      if (!state.monthTree.expandedNodes.has(String(id))) await loadYear(Number(String(id).split(':')[1]));
      toggle(id, skip);
    } catch (error) { report(error); }
  };
  state.monthTree.isTreeRendered = false;
  state.monthTree.renderTree('');

  state.groupTree = new TreeView($('ade-group-tree'), { inline: true, autoCollapse: false, showSearchBox: false,
    onSelect: ({ id }) => chooseGroup(Number(String(id).split(':')[1])).catch(report) });
  state.groupTree.setData([]);
  state.groupTree.isTreeRendered = false;
  state.groupTree.renderTree('');
}

// Phone layout: year, month and group use comboboxes instead of the two trees.
function buildPickers() {
  const k_make = (k_id, k_label, k_onPick) => {
    const k_combo = new Combobox($(k_id), { readonly: true, placeholder: k_label, staticData: [], onSelect: k_onPick });
    k_combo.input.id = `${k_id}-input`;
    k_combo.input.setAttribute('aria-label', k_label);
    return k_combo;
  };
  state.yearCombo = k_make('ade-year-combo', 'Anul', async (value) => {
    state.year = Number(value); state.monthId = null; state.groupId = null;
    loadSituation(); syncPickers();
    try { await loadYear(state.year); } catch (error) { report(error); }
  });
  state.monthCombo = k_make('ade-month-combo', 'Luna', (k_value) => chooseMonth(Number(k_value)).catch(report));
  state.groupCombo = k_make('ade-group-combo', 'Grupa', (k_value) => chooseGroup(Number(k_value)).catch(report));
}

function syncPickers() {
  if (!state.monthCombo || !state.context) return;
  state.yearCombo.options.staticData = state.context.years.map((year) => ({ value: String(year), label: String(year) }));
  if (state.year) state.yearCombo.setValue(String(state.year), String(state.year)); else state.yearCombo.clear();
  const k_months = state.context.months.filter((row) => row.Anul === state.year)
    .sort((a, b) => String(a.LunaT || '').localeCompare(String(b.LunaT || ''), 'ro', { sensitivity: 'base' }))
    .map((row) => ({ value: String(row.IDL), label: `${row.LunaT} ${row.Anul}${row.Inchisa ? ' · închisă' : ''}` }));
  state.monthCombo.options.staticData = k_months;
  const k_month = k_months.find((item) => item.value === String(state.monthId));
  if (k_month) state.monthCombo.setValue(k_month.value, k_month.label); else state.monthCombo.clear();
  const k_groups = state.context.groups.map((row) => ({ value: String(row.IDG), label: row.Grupa }));
  state.groupCombo.options.staticData = k_groups;
  const k_group = k_groups.find((item) => item.value === (state.groupId == null ? '' : String(state.groupId)));
  if (k_group) state.groupCombo.setValue(k_group.value, k_group.label); else state.groupCombo.clear();
}

function situationColumns(canEdit) {
  const number = (key, title, width = 90) => ({ key, title, valueType: ValueType.Number, width, align: 'right', filter: false });
  return [
    { key: 'Nume', title: 'Copil', valueType: ValueType.Text, width: 140, filter: true },
    number('SID', 'S.I. Debit', 96), number('SIC', 'S.I. Credit', 96),
    { ...number('ZilePrezenta', 'Prezență', 76), editable: canEdit, editor: 'number', nullable: false,
      validate: (value) => Number.isInteger(value) && value >= 0 && value <= 31 ? '' : 'Introduceți un număr întreg între 0 și 31.' },
    number('ValoareContract', 'Valoare', 100), number('Plata', 'Plăți', 100),
    number('Retur', 'Restituiri', 100), number('TotalLuna', 'TOTAL', 100),
  ];
}

function decorateRows(rows) {
  return rows.map((row) => ({ ...row, TotalLuna: row.SFD > 0 ? row.SFD : row.SFC || 0 }));
}

async function saveAttendance({ row, value }) {
  $('ade-save-state').textContent = 'Se salvează și se recalculează…';
  await api('/api/adechit/attendance', { method: 'POST', body: JSON.stringify({ id: row.IDZ,
    version: row.Version, values: { ZilePrezenta: value } }) });
  const result = await api(`/api/adechit/situation/${state.monthId}?IDG=${state.groupId}`);
  const saved = decorateRows(result.rows).find((item) => item.IDZ === row.IDZ);
  $('ade-save-state').textContent = 'Prezența a fost salvată; situația a fost recalculată.';
  showMessage($('ade-message'), 'Prezența și valorile lunare au fost recalculate.', 'info');
  return saved;
}

function filterGrid() {
  const query = $('ade-search').value.trim().toLocaleLowerCase('ro-RO');
  state.shownRows = state.rows.filter((row) => (state.groupId == null || row.IDG === state.groupId)
    && (!query || `${row.Nume || ''} ${row.CNP || ''}`.toLocaleLowerCase('ro-RO').includes(query)));
  state.selected = null;
  $('ade-selected-child').textContent = 'Selectați un copil din tabel.';
  state.grid?.setRows(state.shownRows, { keepWidths: true });
  renderLedger();
}

// Phone layout (keep in step with the 980px breakpoint in adechit.css): the grid keeps Copil, Prezență and TOTAL;
// a long press on a row shows the other figures, the next long press hides them.
const mobileQuery = matchMedia('(max-width: 980px)');
const MOBILE_HIDDEN = ['SID', 'SIC', 'ValoareContract', 'Plata', 'Retur'];
const LONG_PRESS_MS = 500;
// When the grid is too narrow its columns drop out in this order (the phone layout is the last step); the grid never needs a sideways scroll.
const HIDE_STEPS = [[], ['Retur'], ['Retur', 'Plata'], ['Retur', 'Plata', 'ValoareContract'], MOBILE_HIDDEN];
const GRID_SLACK = 24; // vertical scrollbar + borders

function planColumns() {
  const k_avail = $('ade-grid').clientWidth;
  const k_columns = situationColumns(false);
  const k_steps = mobileQuery.matches ? [MOBILE_HIDDEN] : HIDE_STEPS;
  const k_need = (k_hidden, k_skip) => k_columns.filter((c) => !k_hidden.includes(c.key) && c.key !== k_skip)
    .reduce((sum, c) => sum + c.width, 0) + GRID_SLACK;
  if (!k_avail) return { hidden: k_steps[0], nameWidth: null };
  const k_fit = k_steps.find((k_hidden) => k_need(k_hidden) <= k_avail);
  if (k_fit) return { hidden: k_fit, nameWidth: null };
  const k_last = k_steps.at(-1);
  return { hidden: k_last, nameWidth: Math.max(80, k_avail - k_need(k_last, 'Nume')) };
}

function hideRowDetail() { $('ade-row-detail').hidden = true; }

function showRowDetail(k_row, k_node) {
  const k_card = $('ade-row-detail');
  // every figure of the row, in grid order, plus Alte plăți which the grid never shows
  const k_figures = situationColumns(false).filter((c) => c.key !== 'Nume').map((c) => [c.title, k_row[c.key]]);
  k_figures.splice(k_figures.findIndex(([k_title]) => k_title === 'Restituiri'), 0, ['Alte plăți', k_row.Plati]);
  k_card.innerHTML = `<strong>${escapeHtml(k_row.Nume ?? '')}</strong>${k_figures
    .map(([k_label, k_value]) => `<span>${k_label}</span><b>${money(k_value)}</b>`).join('')}`;
  k_card.hidden = false;
  const k_host = k_card.offsetParent.getBoundingClientRect();
  const k_rowBox = k_node.getBoundingClientRect();
  const k_below = k_rowBox.bottom - k_host.top + 2;
  k_card.style.top = `${k_below + k_card.offsetHeight > k_host.height ? Math.max(0, k_rowBox.top - k_host.top - k_card.offsetHeight - 2) : k_below}px`;
}

function wireLongPress() {
  const k_grid = $('ade-grid');
  let k_timer = 0;
  let k_start = null;
  let k_closed = false;
  const k_cancel = () => { clearTimeout(k_timer); k_timer = 0; };
  // Any touch while the card is open closes it (the press that closes it never starts a new long press).
  document.addEventListener('pointerdown', (k_event) => {
    if ($('ade-row-detail').hidden) return;
    hideRowDetail();
    k_closed = true;
    setTimeout(() => { k_closed = false; }, 0);
  }, true);
  k_grid.addEventListener('pointerdown', (k_event) => {
    if (k_closed) { k_closed = false; return; }
    if (!state.plan.hidden.length || !k_event.target.closest('.dgv__row.is-data')) return;
    k_start = { x: k_event.clientX, y: k_event.clientY };
    const k_node = k_event.target.closest('.dgv__row');
    k_timer = setTimeout(() => {
      k_timer = 0;
      const k_row = state.grid?.rowFromNode(k_node);
      if (k_row) showRowDetail(k_row, k_node);
    }, LONG_PRESS_MS);
  });
  k_grid.addEventListener('pointermove', (k_event) => {
    if (k_timer && k_start && Math.hypot(k_event.clientX - k_start.x, k_event.clientY - k_start.y) > 10) k_cancel();
  });
  ['pointerup', 'pointercancel', 'pointerleave'].forEach((k_name) => k_grid.addEventListener(k_name, k_cancel));
  k_grid.addEventListener('contextmenu', (k_event) => { if (mobileQuery.matches) k_event.preventDefault(); });
  mobileQuery.addEventListener('change', () => { hideRowDetail(); if (state.grid) createGrid(); syncCloseButton(); });
  let k_frame = 0;
  const k_refit = () => {
    clearTimeout(k_frame);
    k_frame = setTimeout(() => {
      const k_plan = planColumns();
      if (state.grid && `${k_plan.hidden}|${k_plan.nameWidth}` !== `${state.plan.hidden}|${state.plan.nameWidth}`) createGrid();
    }, 100);
  };
  new ResizeObserver(k_refit).observe(k_grid);
  addEventListener('resize', k_refit);
}

function createGrid() {
  hideRowDetail();
  state.grid?.destroy();
  state.plan = planColumns();
  const canEdit = Boolean(state.monthId && state.groupId) && !selectedMonth()?.Inchisa;
  state.grid = new DataGrid($('ade-grid'), { columns: situationColumns(canEdit), rows: state.shownRows,
    rowKey: 'IDZ', mobileRowScale: 1.2, editable: canEdit, layoutId: 'ade.attendance', layout: { fill: 'Nume', hidden: state.plan.hidden, widths: state.plan.nameWidth ? { Nume: state.plan.nameWidth } : {} }, footer: true,
    footerCaption: '{0} copii', frozen: 1, onSelect: selectChild, onCellSave: saveAttendance,
    onCellSaved: ({ row }) => { state.selected = row; loadLedger().catch(report); },
    onEditError: ({ error }) => { $('ade-save-state').textContent = 'Salvarea nu a reușit; valoarea introdusă a fost păstrată.'; report(error); } });
  state.grid.sortBy('Nume', 'asc');
}

async function loadSituation() {
  const own = ++state.request;
  state.rows = []; state.shownRows = []; state.selected = null; createGrid(); renderLedger();
  $('ade-selected-child').textContent = 'Selectați un copil din tabel.';
  const month = selectedMonth();
  $('ade-period').textContent = month ? `${month.LunaT} ${month.Anul}${month.Inchisa ? ' · lună închisă' : ''}` : 'Alegeți luna';
  $('ade-group-caption').textContent = selectedGroup()?.Grupa || 'Alegeți grupa';
  syncCloseButton(month);
  $('ade-reopen').disabled = !month || !month.Inchisa;
  $('ade-refresh').disabled = !month || state.groupId == null;
  $('ade-add-child').disabled = !month || state.groupId == null;
  $('ade-take-children').disabled = !month || state.groupId == null;
  if (!month || state.groupId == null) return;
  const result = await api(`/api/adechit/situation/${state.monthId}?IDG=${state.groupId}`);
  if (own !== state.request) return;
  state.rows = decorateRows(result.rows || []);
  state.shownRows = state.rows.filter((row) => state.groupId == null || row.IDG === state.groupId);
  state.selected = null;
  createGrid();
  $('ade-period').textContent = `${month.LunaT} ${month.Anul}${month.Inchisa ? ' · lună închisă' : ''}`;
  $('ade-group-caption').textContent = selectedGroup()?.Grupa || 'Alegeți grupa';
  syncCloseButton(month);
  $('ade-reopen').disabled = !month.Inchisa;
  $('ade-selected-child').textContent = 'Selectați un copil din tabel.';
  renderLedger();
  showMessage($('ade-message'), `${state.shownRows.length} copii încărcați.`, 'info');
}

async function chooseMonth(id) {
  if (!Number.isFinite(id) || id === state.monthId) return;
  state.monthId = id;
  state.year = selectedMonth()?.Anul;
  syncPickers();
  await loadGroups();
  await loadSituation();
}

async function chooseGroup(id) {
  if (!Number.isFinite(id) || id === state.groupId) return;
  state.groupId = id;
  syncPickers();
  await loadSituation();
}

function selectChild(row) {
  state.selected = row;
  $('ade-selected-child').textContent = `${row.Nume} · SID ${money(row.SID)} · SIC ${money(row.SIC)}`;
  loadLedger().catch(report);
}

// One entry per visible column; the first column is always the document number. The width classes are
// in adechit.css (ade-col-*), the explanation column takes the remaining width so the table never scrolls sideways.
function ledgerSpec(kind) {
  const dateInput = '<input id="ade-ledger-date" type="date" aria-label="Data documentului">';
  const valueInput = '<input id="ade-ledger-value" inputmode="decimal" aria-label="Valoarea documentului">';
  const explanationInput = '<input id="ade-ledger-explanation" aria-label="Explicația documentului">';
  const date = (row) => dateText(kind === 'other' ? row.DataDoc : row.Data);
  const value = (row) => money(kind === 'refund' ? row.Suma : row.Valoare);
  const common = [
    { title: 'Data', cls: 'ade-col-date', cell: date, draft: dateInput },
    { title: 'Valoare', cls: 'ade-col-value is-number', cell: value, draft: valueInput },
    { title: 'Explicație', cls: 'ade-col-explanation', cell: (row) => row.Explicatie, draft: explanationInput },
  ];
  if (kind === 'receipt') return [{ title: 'Număr', cls: 'ade-col-number', cell: (row) => row.Numar }, ...common];
  if (kind === 'other') return [{ title: 'Număr', cls: 'ade-col-number', cell: (row) => row.NrDoc },
    { title: 'Document', cls: 'ade-col-document', cell: (row) => row.FelDoc }, ...common];
  return [{ title: 'Număr', cls: 'ade-col-number', cell: (row) => row.NrDoc }, ...common];
}

const REFUND_EXPLANATION = 'Restituire sumă';
let receiptMenuCleanup = null;

function receiptActionButton(id, latest = false) {
  return `<button class="ade-cell-button ade-receipt-actions" type="button" data-receipt-id="${id || ''}" ${id ? '' : 'disabled'}
    aria-haspopup="menu" aria-expanded="false" title="${latest ? 'Opțiuni pentru ultima chitanță salvată' : 'Opțiuni chitanță'}" aria-label="Opțiuni chitanță">📥</button>`;
}

async function fetchReceiptOutput(id, kind) {
  const response = await fetch(`/api/adechit/receipts/${id}/${kind}`, {
    headers: { 'X-Ade-Unit': state.unit, 'X-Portal-Token': portalToken(), Accept: kind === 'pdf' ? 'application/pdf' : 'text/html' },
  });
  if (!response.ok) {
    const result = await response.json().catch(() => ({}));
    if (response.status === 401 || result.reason === 'UNITATE_NEDESCHISA') toPortal(response.status === 401);
    throw new Error(result.error || `Eroare HTTP ${response.status}`);
  }
  return response;
}

async function outputReceipt(id, kind) {
  // Open synchronously from the menu click so the browser accepts the print window.
  const printWindow = kind === 'print' ? window.open('', '_blank') : null;
  try {
    if (kind === 'print' && !printWindow) throw new Error('Browserul a blocat fereastra de listare. Permiteți ferestrele popup pentru această pagină.');
    if (printWindow) { printWindow.document.title = 'Chitanță'; printWindow.document.body.textContent = 'Se pregătește chitanța…'; }
    const response = await fetchReceiptOutput(id, kind);
    if (kind === 'print') {
      const html = await response.text();
      if (printWindow.closed) return;
      printWindow.document.open(); printWindow.document.write(html); printWindow.document.close();
      printWindow.opener = null;
    } else {
      const url = URL.createObjectURL(await response.blob());
      const link = document.createElement('a'); link.href = url; link.download = `Chitanta_${id}.pdf`;
      document.body.append(link); link.click(); link.remove();
      setTimeout(() => URL.revokeObjectURL(url), 60000);
    }
  } catch (error) { printWindow?.close(); throw error; }
}

async function openReceiptMenu(button, receipt) {
  receiptMenuCleanup?.();
  if (mobileQuery.matches || !receipt || !state.selected) return;
  const child = state.selected.IDP;
  const payers = await api(`/api/adechit/rows/Platitori_sub?IDP=${child}`);
  if (mobileQuery.matches || !button.isConnected || state.selected?.IDP !== child) return;
  receiptMenuCleanup?.();
  const payer = payers.rows.find((row) => row.IDS === receipt.PayerId);
  const menu = document.createElement('div'); menu.className = 'ade-receipt-menu';
  menu.setAttribute('role', 'menu'); menu.setAttribute('aria-label', 'Opțiuni chitanță');
  const listeners = new AbortController();
  const close = (focus = false) => {
    listeners.abort(); menu.remove(); button.setAttribute('aria-expanded', 'false');
    if (focus && button.isConnected) button.focus();
    if (receiptMenuCleanup === close) receiptMenuCleanup = null;
  };
  receiptMenuCleanup = close;
  const entries = [{ icon: '🖨️', label: 'Listare', kind: 'print' }];
  if (payer?.EMail?.trim()) entries.push({ icon: '✉️', label: 'Trimitere pe mail', disabled: true });
  entries.push({ icon: '📄', label: 'Descarcă PDF', kind: 'pdf' });
  for (const entry of entries) {
    const item = document.createElement('button'); item.type = 'button'; item.setAttribute('role', 'menuitem');
    item.disabled = Boolean(entry.disabled);
    const icon = document.createElement('span'); icon.className = 'ade-receipt-menu__icon'; icon.setAttribute('aria-hidden', 'true'); icon.textContent = entry.icon;
    const label = document.createElement('span'); label.textContent = entry.label; item.append(icon, label);
    if (entry.kind) item.addEventListener('click', () => { close(true); outputReceipt(receipt.IDC, entry.kind).catch(report); });
    menu.append(item);
  }
  document.body.append(menu); button.setAttribute('aria-expanded', 'true');
  menu.style.zIndex = String(window.ZIndexManager?.getNext() || 1001);
  const rect = button.getBoundingClientRect();
  menu.style.left = `${Math.max(8, Math.min(rect.right - menu.offsetWidth, innerWidth - menu.offsetWidth - 8))}px`;
  menu.style.top = `${Math.max(8, Math.min(rect.bottom + menu.offsetHeight > innerHeight - 8 ? rect.top - menu.offsetHeight : rect.bottom, innerHeight - menu.offsetHeight - 8))}px`;
  const items = [...menu.querySelectorAll('button:not(:disabled)')]; items[0]?.focus();
  menu.addEventListener('keydown', (event) => {
    const index = items.indexOf(document.activeElement);
    if (event.key === 'ArrowDown' || event.key === 'ArrowUp') {
      event.preventDefault(); items[(index + (event.key === 'ArrowDown' ? 1 : items.length - 1)) % items.length]?.focus();
    } else if (event.key === 'Home' || event.key === 'End') {
      event.preventDefault(); items[event.key === 'Home' ? 0 : items.length - 1]?.focus();
    }
  });
  document.addEventListener('pointerdown', (event) => { if (!menu.contains(event.target) && !button.contains(event.target)) close(); }, { capture: true, signal: listeners.signal });
  document.addEventListener('keydown', (event) => { if (event.key === 'Escape') { event.preventDefault(); close(true); } else if (event.key === 'Tab') close(); }, { signal: listeners.signal });
  addEventListener('resize', () => close(), { signal: listeners.signal });
  addEventListener('scroll', () => close(), { capture: true, signal: listeners.signal });
}

function renderLedger(rows = []) {
  receiptMenuCleanup?.();
  const kind = state.ledger;
  const spec = ledgerSpec(kind);
  const body = $('ade-ledger-body');
  if (!state.selected) { body.innerHTML = '<div class="ade-ledger-empty">Selectați copilul pentru a vedea documentele și a adăuga un rând.</div>'; return; }
  const existing = [...rows].sort((a, b) => String(a.Explicatie || '').localeCompare(String(b.Explicatie || ''), 'ro', { sensitivity: 'base' }))
    .map((row) => `<tr class="${row.Cancelled ? 'is-cancelled' : ''}">${spec
    .map((column) => `<td class="${column.cls}">${escapeHtml(column.cell(row) ?? '')}</td>`).join('')}<td class="ade-row-actions">${kind === 'receipt' ? receiptActionButton(row.IDC) : row.Cancelled ? 'Anulat' : ''}</td></tr>`).join('');
  const draftCells = spec.map((column) => `<td class="${column.cls}">${column.draft || ''}</td>`).join('');
  const latestReceipt = kind === 'receipt' ? [...rows].sort((a, b) => b.IDC - a.IDC)[0] : null;
  body.innerHTML = `<table class="ade-ledger-table${kind === 'receipt' ? ' ade-ledger-table--receipts' : ''}"><colgroup>${spec.map((column) => `<col class="${column.cls.split(' ')[0]}">`).join('')}<col class="ade-col-actions"></colgroup>
    <thead><tr>${spec.map((column) => `<th>${column.title}</th>`).join('')}<th class="ade-row-actions"></th></tr></thead>
    <tbody>${existing}<tr class="is-draft">${draftCells}<td class="ade-row-actions"><div class="ade-ledger-buttons"><button class="ade-cell-button" id="ade-ledger-save" type="button" title="Salvează documentul nou și recalculează situația" aria-label="Salvează documentul nou">💾</button>${kind === 'receipt' ? receiptActionButton(latestReceipt?.IDC, true) : ''}</div></td></tr></tbody></table>`;
  body.querySelectorAll('[data-receipt-id]').forEach((button) => button.addEventListener('click', () => {
    openReceiptMenu(button, rows.find((row) => row.IDC === Number(button.dataset.receiptId))).catch(report);
  }));
  // the calendar opens on today when today is in the selected month, otherwise on the first day of that month
  new DatePicker($('ade-ledger-date'), { startDate: () => {
    const k_month = selectedMonth();
    const k_now = new Date();
    return !k_month || (k_now.getMonth() + 1 === k_month.Luna && k_now.getFullYear() === k_month.Anul) ? k_now : new Date(k_month.Anul, k_month.Luna - 1, 1);
  } });
  const explanation = $('ade-ledger-explanation');
  let touched = false;
  explanation.addEventListener('input', () => { touched = true; });
  $('ade-ledger-date').addEventListener('change', () => {
    fillSuggestedAmount();
    fillSuggestedExplanation(explanation, () => touched).catch(report);
  });
  $('ade-ledger-save').addEventListener('click', () => saveLedger().catch(report));
}

function escapeHtml(value) {
  const node = document.createElement('span');
  node.textContent = String(value);
  return node.innerHTML;
}

function fillSuggestedAmount() {
  const amount = state.ledger === 'refund' ? state.selected.SIC : state.selected.SID;
  if (amount > 0) $('ade-ledger-value').value = amount;
}

// Fills the explanation when the date is chosen; never overwrites what the operator typed.
async function fillSuggestedExplanation(k_input, k_touched) {
  if (state.ledger === 'other') return;
  const k_month = state.monthId;
  const k_text = state.ledger === 'refund' ? REFUND_EXPLANATION
    : (await api(`/api/adechit/receipt-defaults?IDL=${k_month}`)).explanation;
  if (!k_touched() && k_month === state.monthId && k_input.isConnected) k_input.value = k_text;
}

async function ledgerData() {
  const idz = state.selected.IDZ;
  const month = state.monthId;
  if (state.ledger === 'refund') {
    const result = await api(`/api/adechit/rows/Retur?IDL=${month}&IDZ=${idz}`);
    return result.rows.map((row) => ({ ...row, Cancelled: Boolean(row.Anulat) }));
  }
  const [paymentResult, documentResult] = await Promise.all([
    api(`/api/adechit/rows/Plati?IDL=${month}&IDZ=${idz}`),
    api(`/api/adechit/rows/${state.ledger === 'receipt' ? 'Chitante' : 'AlteDoc'}?IDL=${month}`),
  ]);
  const payments = new Map(paymentResult.rows.map((row) => [row.IDPL, row]));
  return documentResult.rows.filter((row) => payments.has(row.IDPL)).map((row) => ({ ...row,
    Valoare: payments.get(row.IDPL).Plata, PayerId: payments.get(row.IDPL).IDS,
    Cancelled: Boolean(row.Anulata || payments.get(row.IDPL).Anulata) }));
}

async function loadLedger() {
  if (!state.selected) { renderLedger(); return; }
  const childId = state.selected.IDZ;
  const rows = await ledgerData();
  if (state.selected?.IDZ === childId) renderLedger(rows);
}

async function saveLedger() {
  const date = $('ade-ledger-date').value;
  const raw = $('ade-ledger-value').value.trim().replace(',', '.');
  const amount = Number(raw);
  if (!date) throw new Error('Completați data documentului.');
  if (!raw || !Number.isFinite(amount) || amount === 0) throw new Error('Completați o valoare numerică diferită de zero.');
  const payers = await api(`/api/adechit/rows/Platitori_sub?IDP=${state.selected.IDP}`);
  const payer = payers.rows.find((row) => row.Activ);
  if (!payer) throw new Error('Copilul nu are un plătitor activ.');
  const kind = state.ledger;
  const body = { kind, attendance_id: state.selected.IDZ, payer_id: payer.IDS, date, amount, explanation: $('ade-ledger-explanation').value.trim() };
  if (kind === 'other') Object.assign(body, { document_type: 'Altă plată', number: 'Fără număr' });
  await api('/api/adechit/documents', { method: 'POST', body: JSON.stringify(body) });
  const selectedId = state.selected.IDZ;
  await loadSituation();
  state.selected = state.rows.find((row) => row.IDZ === selectedId) || null;
  if (state.selected) $('ade-selected-child').textContent = `${state.selected.Nume} · SID ${money(state.selected.SID)} · SIC ${money(state.selected.SIC)}`;
  await loadLedger();
  showMessage($('ade-message'), 'Documentul a fost salvat și situația a fost recalculată.', 'info');
}

async function command(path, success) {
  try {
    await api(path, { method: 'POST', body: JSON.stringify({ id: state.monthId }) });
    await loadContext(true);
    showMessage($('ade-message'), success, 'info');
  } catch (error) { report(error); }
}

async function takeChildren() {
  if (state.groupId == null) throw new Error('Selectați grupa pentru preluarea copiilor noi.');
  const result = await api('/api/adechit/attendance/prepare', { method: 'POST', body: JSON.stringify({
    month_id: state.monthId, group_id: state.groupId }) });
  await loadSituation();
  showMessage($('ade-message'), `${result.count} copii au fost preluați în luna selectată.`, 'info');
}

function bind() {
  const catalogs = bindCatalogs({ api, context: () => state.context, refresh: () => loadContext(true).catch(report) });
  const annual = bindAnnual({ api, refresh: () => loadContext(true), message: (text) => showMessage($('ade-message'), text, 'info') });
  wireLongPress();
  document.querySelectorAll('.ade-report-date input').forEach((k_input) => new DatePicker(k_input));
  $('ade-search').addEventListener('input', filterGrid);
  $('ade-refresh').addEventListener('click', () => loadSituation().catch(report));
  $('ade-close').addEventListener('click', () => {
    if (selectedMonth()?.Luna === 8) annual(state.monthId).catch(report);
    else command('/api/adechit/close', 'Luna a fost închisă.');
  });
  $('ade-reopen').addEventListener('click', () => command('/api/adechit/reopen', 'Luna a fost redeschisă.'));
  $('ade-add-child').addEventListener('click', () => catalogs.addChild(state.groupId, (child) =>
    api('/api/adechit/attendance/prepare', { method: 'POST', body: JSON.stringify({
      month_id: state.monthId, group_id: child.IDG, person_id: child.IDP }) })).catch(report));
  $('ade-take-children').addEventListener('click', () => takeChildren().catch(report));
  document.querySelectorAll('[data-ledger]').forEach((button) => button.addEventListener('click', () => {
    state.ledger = button.dataset.ledger;
    document.querySelectorAll('[data-ledger]').forEach((item) => item.setAttribute('aria-selected', String(item === button)));
    loadLedger().catch(report);
  }));
  const showThemeIcon = () => { $('ade-theme').textContent = document.documentElement.dataset.theme === 'dark' ? '🌙' : '☀️'; };
  showThemeIcon();
  $('ade-theme').addEventListener('click', () => { const dark = document.documentElement.dataset.theme === 'dark';
    document.documentElement.dataset.theme = dark ? 'light' : 'dark'; state.grid?.setTheme(dark ? 'modern' : 'dark'); showThemeIcon(); });
  const shell = document.querySelector('.ade-shell');
  const reportsToggle = $('ade-reports-toggle');
  const setReportsCollapsed = (collapsed) => {
    shell.classList.toggle('is-reports-collapsed', collapsed);
    reportsToggle.setAttribute('aria-expanded', String(!collapsed));
    reportsToggle.textContent = collapsed ? '‹' : '›';
    reportsToggle.title = collapsed ? 'Extinde panoul Rapoarte' : 'Restrânge panoul Rapoarte';
    localStorage.setItem('ade.reports.collapsed', collapsed ? '1' : '0');
  };
  setReportsCollapsed(localStorage.getItem('ade.reports.collapsed') === '1');
  reportsToggle.addEventListener('click', () => setReportsCollapsed(!shell.classList.contains('is-reports-collapsed')));
  addEventListener('pagehide', () => { receiptMenuCleanup?.(); state.grid?.destroy(); state.monthTree?.destroy?.(); state.groupTree?.destroy?.(); }, { once: true });
}

async function loadContext(rebuildTrees = false) {
  const context = await api('/api/adechit/context');
  state.context = { ...context, months: [], groups: [], years: [] }; state.unit = context.unit;
  state.loadedYears.clear(); state.groupsRequest = null;
  state.context.years = (await api('/api/adechit/years')).years;
  $('ade-context').textContent = `${context.unit} · ${context.email}`;
  $('ade-limitations').textContent = context.limitations.join(' ');
  if (!rebuildTrees) {
    buildTrees(); buildPickers();
    const latestYear = state.context.years[0];
    state.monthTree.expandedNodes.clear();
    if (latestYear != null) {
      await loadYear(latestYear);
      state.monthTree.expandedNodes.add(`year:${latestYear}`);
      state.monthTree.isTreeRendered = false; state.monthTree.renderTree('');
    }
  }
  else {
    if (state.year) await loadYear(state.year);
    state.monthTree.setData(monthNodes()); state.monthTree.isTreeRendered = false; state.monthTree.renderTree('');
    if (state.monthId) await loadGroups();
  }
  if (!state.context.months.some((row) => row.IDL === state.monthId)) state.monthId = null;
  if (!state.context.groups.some((row) => row.IDG === state.groupId)) state.groupId = null;
  syncPickers();
  await loadSituation();
}

async function start() {
  state.unit = new URLSearchParams(location.search).get('unit') || 'preview';
  bind();
  await loadContext();
}

start().catch(report);
