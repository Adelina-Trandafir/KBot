import '../components/tooltip/tooltip-titles.js';
import { portalToken } from '../vercon/api.js';
import { createLock } from '../vercon/lock.js';
import { installLayouts } from '../portal/layouts.js';
import { $, combo, input, say, discard, reportReadErrors } from './controls.js';
import { makeApi, login } from './api.js';
import { state, reset } from './state.js';
import { mountGrids } from './grids.js';
import { actions } from './actions.js';
import { startSessionTimer } from '../adechit/session.js';
import { mountSplitter } from './splitter.js';
import { mountHeader } from './header.js';
import { contextUrl, initialContext, rememberContext } from '../vercon/context-link.js';

const lifetime = new AbortController();
let lock, behavior, timer, sessionEnding = false, accounts = [];
const api = makeApi(lifetime.signal, () => lock);
lock = createLock({ api, signal: lifetime.signal, getUnit: () => state.unit,
  onLost: message => { say(message, 'error'); enable(); } });
installLayouts();
const inputs = { file: input('vi-file-name', { type: 'text', readOnly: true, placeholder: 'Niciun fișier ales', ariaLabel: 'Fișierul Excel' }),
  quantity: input('vi-quantity', { type: 'decimal', required: true, min: 0, ariaLabel: 'Cantitate asociată' }),
  excelPrice: input('vi-excel-price', { type: 'money', readOnly: true, ariaLabel: 'Preț Excel' }),
  verconPrice: input('vi-vercon-price', { type: 'money', readOnly: true, ariaLabel: 'Preț VERCON' }) };
const grids = mountGrids(row => behavior?.chooseExcel(row), row => behavior?.chooseCandidate(row), row => { state.assignment = row; enable(); });
mountSplitter({ host: $('vi-tables'), handle: $('vi-splitter'), bottom: $('vi-assigned'), signal: lifetime.signal });
const header = mountHeader({ api, signal: lifetime.signal,
  onSubunit: value => run(async () => {
    if (String(value) === String(state.selection.subunit)) return;
    if (!await discard()) { paintContext(); return; }
    clear(); await context({ subunit: Number(value) });
  }),
  onGestiune: value => run(async () => {
    if (String(value) === String(state.selection.gestiune)) return;
    if (!await discard()) { paintContext(); return; }
    clear(); await context({ subunit: state.selection.subunit, gestiune: Number(value) });
  }),
  onAccount: value => run(async () => {
    if (String(value) === String(state.account)) return;
    if (!await discard()) { picks.account.set(accounts, state.account); return; }
    clear(); state.account = Number(value); picks.account.set(accounts, state.account);
    say('Alegeți fișierul și foaia, apoi «Citește și compară».');
  }),
  onTheme: theme => [grids.excel, grids.vercon, grids.assigned].forEach(grid => grid.setTheme(theme)),
  navigate: path => run(async () => {
    if (!await discard()) return;
    await lock.release();
    sessionEnding = true;
    window.location.href = contextUrl(path, state.unit, state.selection);
  }),
});
const picks = { ...header.picks,
  sheet: combo('vi-sheet', value => run(async () => {
    if (!await discard()) { picks.sheet.set(state.sheets || [], state.sheet); return; }
    clear(); state.sheet = value; picks.sheet.set(state.sheets || [], value);
  })),
};
function clear() { reset(); behavior?.clearSend(); grids.paint(); behavior?.detail(); }
function enable() {
  const busy = state.busy, editable = !busy && !state.sent && !state.uncertain;
  const contextReady = !!(state.selection.gestiune && state.account);
  header.enable(!busy && !state.uncertain);
  picks.sheet.enable(!busy && !state.uncertain);
  inputs.quantity.setEnabled(editable && !!state.candidate);
  $('vi-browse').disabled = busy || state.uncertain;
  $('vi-load').disabled = busy || state.uncertain || !state.file || !state.sheet || !contextReady;
  $('vi-send').disabled = busy || state.sent || !state.workspace || !(state.ctx?.operations || []).includes('import');
  $('vi-send').textContent = state.uncertain ? 'Confirmă rezultatul trimiterii' : 'Trimite în VERCON';
  $('vi-associate').disabled = !editable || !state.excel || !state.candidate;
  $('vi-undo').disabled = !editable || !state.assignment;
  document.querySelector('.vi-work').inert = busy || state.sent || state.uncertain;
  document.querySelector('.vi-assigned').inert = busy || state.sent || state.uncertain;
  $('vi-app').setAttribute('aria-busy', String(busy));
}
async function run(work) {
  if (state.busy) return;
  state.busy = true; enable();
  try { await work(); }
  catch (error) { if (error.name !== 'AbortError') { say(''); say(error.message || 'Operația nu a fost finalizată.', 'error'); } }
  finally { state.busy = false; enable(); }
}
behavior = actions({ api, lock, inputs, grids, run, enable });
function paintContext() {
  header.paint(state.ctx, state.selection, accounts, state.account);
}
async function context(wanted) {
  state.ctx = null; state.account = null; accounts = []; state.selection = {};
  paintContext();
  const query = new URLSearchParams(Object.entries(wanted).filter(([, v]) => v != null));
  let ctx;
  try { ctx = await api('/api/vercon/context?' + query); }
  catch (error) {
    if (error.reason !== 'SELECTION_CHANGED' || !query.size) throw error;
    return context({});
  }
  state.ctx = ctx;
  state.selection = Object.fromEntries(['subunit', 'gestiune', 'year'].map(k => [k, ctx.selection[k]]).filter(([, v]) => v != null));
  rememberContext(state.unit, state.selection);
  paintContext();
  if (state.selection.gestiune && ctx.selection.type === 'OI') {
    const q = new URLSearchParams(state.selection);
    const result = await api('/api/vercon/nom/categorii?' + q);
    accounts = result.rows.filter(r => r.active && String(r.TIP || '').trim().toUpperCase() === 'OIM')
      .map(r => ({ id: r.id, name: r.Masina || '' }));
    if (accounts.length === 1) state.account = accounts[0].id;
  }
  paintContext();
  if (!(ctx.operations || []).includes('read')) throw new Error('Contul nu are drept de citire în VERCON.');
  if (state.selection.gestiune && ctx.selection.type !== 'OI') {
    say('Selectați o subgestiune de obiecte de inventar din antetul VERCON. Situația inventarului este la 31.12.2025.');
  } else if (state.selection.gestiune && !accounts.length) {
    throw new Error('Subgestiunea selectată nu are niciun cont OIM activ.');
  } else {
    say('Inventar OI la 31.12.2025 · magazie și folosință. Alegeți foaia Excel' +
      (accounts.length > 1 ? ' și contul OIM din antet' : '') + ', apoi «Citește și compară».');
  }
}

$('vi-browse').addEventListener('click', () => {
  if (state.busy || state.uncertain) return;
  $('vi-file').value = ''; $('vi-file').click();
});
$('vi-file').addEventListener('change', () => run(async () => {
  const file = $('vi-file').files[0];
  if (!file) return;
  if (!await discard()) return;
  clear(); state.file = null; state.sheet = null; state.sheets = []; picks.sheet.set([], null);
  if (file.size > 20000000 || !file.name.toLowerCase().endsWith('.xlsx')) throw new Error('Alegeți un XLSX de cel mult 20 MB.');
  const form = new FormData(); form.append('file', file);
  const result = await api('/api/vercon/inventory/workbook', { method: 'POST', body: form });
  state.file = file; inputs.file.setValue(file.name);
  state.sheets = result.sheets.map(name => ({ id: name, name }));
  state.sheet = result.sheets.find(name => name.toLocaleLowerCase('ro') === 'scoala 2025') || result.sheets[0] || null;
  picks.sheet.set(state.sheets, state.sheet);
  say('Fișier ales. Selectați foaia și apăsați «Citește și compară».');
}));
$('vi-load').addEventListener('click', () => run(async () => {
  if (!await discard()) return;
  clear();
  const form = new FormData(); form.append('file', state.file); form.append('sheet', state.sheet);
  form.append('account', state.account);
  for (const [key, value] of Object.entries(state.selection)) form.append(key, value);
  say('Se citește Excelul și situația VERCON la 31.12.2025…');
  const result = await api('/api/vercon/inventory/load', { method: 'POST', body: form });
  state.workspace = result.workspace; state.rows = result.rows;
  grids.paint();
  say(`${result.rows.length} poziții Excel; ${result.products} variante VERCON cu intrări înainte de 01.01.2026. ` +
    `${result.omitted} poziții de alt tip omise; ${result.errors?.length || 0} poziții cu erori sărite. ` +
    (result.rows.length ? 'Selectați un rând Excel pentru toți candidații.' : 'Nu există poziții valide de asociat.'));
  reportReadErrors(result.errors);
}));
window.addEventListener('beforeunload', event => {
  if (!sessionEnding && ((state.assignments.length && !state.sent) || state.uncertain)) { event.preventDefault(); event.returnValue = ''; }
});
window.addEventListener('app:session-expired', () => { sessionEnding = true; });
window.addEventListener('pagehide', () => {
  timer?.(); void lock.release(true); lifetime.abort(); grids.destroy();
  Object.values(inputs).forEach(box => box.destroy()); Object.values(picks).forEach(box => box.destroy());
}, { once: true });

void run(async () => {
  if (!portalToken()) { login(); return; }
  const me = await api('/api/portal/me');
  timer = startSessionTimer({ token: portalToken, host: $('vc-session-timer'), onExpired: login });
  header.identity(me);
  state.unit = me.db_name;
  if (!state.unit) { login(); return; }
  await context(initialContext(state.unit));
});
