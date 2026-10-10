// Run with: node --experimental-vm-modules --test PYTHON/tests/adechit_lazy.test.mjs
import { test } from 'node:test';
import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import vm from 'node:vm';

function environment(mobile = false, storage = new Map()) {
  const elements = new Map(); const grids = new Map(); const calls = [];
  const element = (id) => {
    if (!elements.has(id)) elements.set(id, {
      id, value: '', clientWidth: 1000, hidden: false, dataset: {}, parentElement: {},
      classList: { toggle() {}, contains() { return false; } }, listeners: new Map(),
      addEventListener(name, fn) { this.listeners.set(name, fn); },
      setAttribute() {}, replaceChildren() {}, showModal() { this.open = true; }, close() { this.open = false; },
    });
    return elements.get(id);
  };
  class DataGrid {
    constructor(host, options) { this.options = options; this.rows = options.rows; grids.set(host.id, this); }
    destroy() {} setRows(rows) { this.rows = rows; } selectRow() {}
    sortBy(key) { this.rows = [...this.rows].sort((a, b) => String(a[key] || '').localeCompare(String(b[key] || ''), 'ro', { sensitivity: 'base' })); }
  }
  class TreeView {
    constructor(host, options) { this.options = options; this.expandedNodes = new Set(); }
    setData(nodes) { this.nodes = nodes; } renderTree() {}
    toggleNode(id) { if (this.expandedNodes.has(id)) this.expandedNodes.delete(id); else this.expandedNodes.add(id); }
  }
  class Combobox {
    constructor(host, options) { this.options = options; this.input = element(host.id + '-input'); }
    setValue(value) { this.value = value; } clear() { this.value = null; }
  }
  const context = vm.createContext({
    console, URLSearchParams, Intl, Date, Map, Set, Number, String, Boolean, Math,
    setTimeout, clearTimeout, crypto: { randomUUID: () => 'test-key' },
    document: { getElementById: element, querySelector: element, querySelectorAll: () => [],
      documentElement: { dataset: {} }, body: { classList: { toggle() {} } }, addEventListener() {} },
    window: { sessionStorage: { getItem: (key) => (key.startsWith('ade.subunit.') ? storage.get(key) ?? null : 'token'),
      setItem(key, value) { storage.set(key, value); }, removeItem(key) { storage.delete(key); } }, location: { reload() {} } }, location: { search: '' },
    localStorage: { getItem() {}, setItem() {} }, addEventListener() {},
    matchMedia: () => ({ matches: mobile, addEventListener() {} }), ResizeObserver: class { observe() {} },
  });
  const groups = [{ IDG: 1, Grupa: 'One' }, { IDG: 2, Grupa: 'Two' }];
  let responder = async (path) => {
    if (path.endsWith('/context')) return { unit: 'preview', email: 'test', limitations: [], permissions: ['catalog'], subunits: [{ id: 1, name: 'Evidenta' }], subunit: { id: 1, name: 'Evidenta' } };
    if (path.endsWith('/years')) return { years: [2026, 2025] };
    if (path.includes('LunaD?Anul=')) return { rows: [{ IDL: 1, Anul: 2026, Luna: 10, LunaT: 'October', Inchisa: false }] };
    if (path.split('?')[0].endsWith('/catalog-data')) return { groups };
    if (path.includes('/situation/')) return { rows: [{ IDZ: 1, IDG: Number(path.split('IDG=')[1]), Nume: 'Child' }] };
    if (path.includes('/Platitori?IDG=')) return { rows: [{ IDP: Number(path.split('IDG=')[1]), IDG: Number(path.split('IDG=')[1]), Nume: 'Child' }] };
    if (path.includes('/Platitori_sub?IDP=')) return { rows: [{ IDS: 1, IDP: Number(path.split('IDP=')[1]), Nume: 'Payer', Activ: true }] };
    throw new Error('Unexpected request: ' + path);
  };
  const api = async (path, options) => { calls.push(path); return responder(path, options); };
  context.fetch = async (path, options) => {
    const result = await api(path, options);
    return { ok: true, status: 200, json: async () => result };
  };
  const bindings = { DataGrid, ValueType: {}, TreeView, Combobox, DatePicker: class {},
    showMessage() {}, bindCatalogs() {}, bindAnnual() {}, bindReports() { return { syncDates() {} }; }, outputReport() {}, cnpMessage() {}, openMenu() {}, startSessionTimer() {} };
  async function load(file, appended = '') {
    let source = await readFile(new URL('../static/js/adechit/' + file, import.meta.url), 'utf8');
    if (file === 'app.js') source = source.replace('start().catch(report);', 'export const ready = start();');
    const module = new vm.SourceTextModule(source + appended, { context });
    await module.link(async () => new vm.SyntheticModule(Object.keys(bindings), function () {
      for (const [key, value] of Object.entries(bindings)) this.setExport(key, value);
    }, { context }));
    await module.evaluate(); return module.namespace;
  }
  return { load, calls, element, grids, api, groups, context, respond: (fn) => { responder = fn; } };
}

test('a live subunit selection survives reloading /adechit without a unit query', async () => {
  const storage = new Map();
  const subunits = [{ id: 11, name: 'First' }, { id: 12, name: 'Second' }];
  const headers = [];
  const respond = (path, options) => {
    headers.push({ path, ...options.headers });
    if (path.endsWith('/context')) return { unit: 'DC_LIVE', email: 'test', limitations: [], permissions: [],
      subunits, subunit: subunits.find((row) => String(row.id) === options.headers['X-Ade-Subunit']) || null };
    if (path.endsWith('/years')) return { years: [] };
    throw new Error('Unexpected request: ' + path);
  };
  const first = environment(false, storage);
  first.respond(respond);
  first.context.document.querySelector = (selector) => selector === 'dialog[open]' ? null : first.element(selector);
  let reloads = 0;
  first.context.window.location.reload = () => { reloads += 1; };
  const app = await first.load('app.js', '\nexport { state };');
  await app.ready;
  assert.equal(app.state.subunit, null);
  assert.deepEqual(first.calls, ['/api/adechit/context']);
  await app.state.subunitCombo.options.onSelect('12');
  assert.equal(storage.get('ade.subunit.DC_LIVE'), '12');
  assert.equal(reloads, 1);

  const second = environment(false, storage);
  second.respond(respond);
  const reloaded = await second.load('app.js', '\nexport { state };');
  await reloaded.ready;
  assert.equal(reloaded.state.subunit, 12);
  assert.deepEqual(second.calls, ['/api/adechit/context', '/api/adechit/context', '/api/adechit/years']);
  assert.equal(headers.at(-1)['X-Ade-Unit'], 'DC_LIVE');
  assert.equal(headers.at(-1)['X-Ade-Subunit'], '12');
});

test('a stale URL unit never sends its saved subunit to the session unit', async () => {
  const env = environment(false, new Map([['ade.subunit.DC_OLD', '99'], ['ade.subunit.DC_LIVE', '12']]));
  env.context.location.search = '?unit=DC_OLD';
  const contextHeaders = [];
  env.respond((path, options) => {
    if (path.endsWith('/context')) {
      contextHeaders.push(options.headers);
      return { unit: 'DC_LIVE', email: 'test', limitations: [], permissions: [],
        subunits: [{ id: 12, name: 'Live' }, { id: 13, name: 'Other' }],
        subunit: options.headers['X-Ade-Subunit'] === '12' ? { id: 12, name: 'Live' } : null };
    }
    if (path.endsWith('/years')) return { years: [] };
    throw new Error('Unexpected request: ' + path);
  });
  const app = await env.load('app.js', '\nexport { state };');
  await app.ready;
  assert.equal(contextHeaders[0]['X-Ade-Subunit'], undefined);
  assert.equal(contextHeaders[1]['X-Ade-Unit'], 'DC_LIVE');
  assert.equal(contextHeaders[1]['X-Ade-Subunit'], '12');
  assert.equal(app.state.subunit, 12);
});

test('attendance starts empty and requests months, groups, then scoped children', async () => {
  const env = environment();
  const app = await env.load('app.js', '\nexport { state, chooseMonth, chooseGroup };');
  await app.ready;
  assert.deepEqual(env.calls, ['/api/adechit/context', '/api/adechit/years', '/api/adechit/rows/LunaD?Anul=2026']);
  assert.equal(app.state.monthId, null); assert.equal(app.state.groupId, null);
  assert.equal(app.state.monthTree.expandedNodes.size, 1);
  assert.ok(app.state.monthTree.expandedNodes.has('year:2026'));
  assert.ok(!app.state.monthTree.expandedNodes.has('year:2025'));
  assert.equal(env.grids.get('ade-grid').rows.length, 0);
  await app.state.monthTree.toggleNode('year:2026');
  assert.equal(env.calls.at(-1), '/api/adechit/rows/LunaD?Anul=2026');
  await app.chooseMonth(1);
  assert.equal(env.calls.at(-1), '/api/adechit/catalog-data?IDL=1');
  assert.equal(env.grids.get('ade-grid').rows.length, 0);
  assert.equal(app.state.groupTree.nodes.length, 2);
  assert.ok(!app.state.groupTree.nodes.some((row) => row.id === 'group:all'));
  await app.chooseGroup(1);
  assert.equal(env.calls.at(-1), '/api/adechit/situation/1?IDG=1');
  await app.chooseGroup(2);
  assert.equal(env.calls.at(-1), '/api/adechit/situation/1?IDG=2');
  assert.equal(env.grids.get('ade-grid').rows[0].IDG, 2);
});

test('attendance displays all payment types, sums numeric columns, and loads documents on selection', async () => {
  const env = environment();
  const app = await env.load('app.js', '\nexport { state, decorateRows, situationColumns };');
  await app.ready;
  const [row] = app.decorateRows([{ IDZ: 1, IDP: 1, Nume: 'Child', Plata: 80, Plati: 120, SFD: 350 }]);
  assert.equal(row.TotalPlati, 200);
  const columns = app.situationColumns(false);
  assert.equal(columns.find((column) => column.title === 'Plăți').key, 'TotalPlati');
  for (const column of columns.filter((column) => column.key !== 'Nume')) assert.equal(column.aggregate, 'sum');
  env.context.document.createElement = () => ({ textContent: '', get innerHTML() { return this.textContent; } });
  env.element('ade-ledger-body').querySelectorAll = () => [];
  app.state.monthId = 1;
  env.respond((path) => {
    assert.equal(path, '/api/adechit/ledger/1?IDL=1&kind=receipt');
    return { rows: [{ IDC: 1, IDPL: 1, Numar: 10, Valoare: 80, Data: '2026-10-01', Cancelled: false }] };
  });
  env.grids.get('ade-grid').options.onSelect(row);
  await new Promise((resolve) => setImmediate(resolve));
  assert.equal(env.calls.at(-1), '/api/adechit/ledger/1?IDL=1&kind=receipt');
  assert.match(env.element('ade-ledger-body').innerHTML, />80<\/td>/);
});

test('main group visibility starts off, includes hidden groups on demand, and clears hidden selection', async () => {
  const env = environment();
  const app = await env.load('app.js', '\nexport { state, chooseMonth, chooseGroup, toggleHiddenGroups };');
  await app.ready;
  env.respond((path) => {
    if (path.startsWith('/api/adechit/catalog-data')) return { groups: path.includes('include_hidden=1')
      ? [{ IDG: 1, Grupa: 'Visible' }, { IDG: 2, Grupa: 'Hidden', Ascunsa: true }] : [{ IDG: 1, Grupa: 'Visible' }] };
    if (path.includes('/situation/')) return { rows: [] };
    throw new Error('Unexpected request: ' + path);
  });
  assert.equal(env.element('ade-show-hidden-groups').checked, false);
  await app.chooseMonth(1);
  assert.equal(app.state.groupTree.nodes.length, 1);
  await app.toggleHiddenGroups(true);
  assert.equal(app.state.groupTree.nodes.length, 2);
  assert.equal(app.state.groupCombo.options.staticData.length, 2);
  await app.chooseGroup(2);
  await app.toggleHiddenGroups(false);
  assert.equal(app.state.groupId, null);
  assert.equal(app.state.groupTree.nodes.length, 1);
  assert.equal(app.state.groupCombo.options.staticData.length, 1);
});

test('changing months reloads groups and ignores a late response from the previous month', async () => {
  const env = environment();
  const app = await env.load('app.js', '\nexport { state, chooseMonth };');
  await app.ready;
  app.state.context.months.push({ IDL: 2, Anul: 2026, Luna: 11, LunaT: 'November', Inchisa: false });
  let firstReply;
  env.respond((path) => {
    if (path === '/api/adechit/catalog-data?IDL=1') return new Promise((resolve) => { firstReply = resolve; });
    if (path === '/api/adechit/catalog-data?IDL=2') return { groups: [{ IDG: 2, Grupa: 'November group' }] };
    throw new Error('Unexpected request: ' + path);
  });
  const october = app.chooseMonth(1);
  await app.chooseMonth(2);
  assert.equal(app.state.groupTree.nodes[0].id, 'group:2');
  firstReply({ groups: [{ IDG: 1, Grupa: 'October group' }] });
  await october;
  assert.equal(app.state.groupTree.nodes[0].id, 'group:2');
  assert.equal(app.state.groupCombo.options.staticData[0].value, '2');
});

for (const mobile of [false, true]) test(`hidden group checkbox confirms active children and respects cancellation (mobile=${mobile})`, async () => {
  const env = environment(mobile);
  const { bindPayers } = await env.load('payers.js');
  let refreshes = 0;
  bindPayers({ api: env.api, context: () => ({ permissions: ['catalog'] }), refresh: async () => { refreshes += 1; } });
  await env.element('ade-payers').listeners.get('click')();
  const options = env.grids.get('ade-groups-list').options;
  assert.equal(options.columns.find((column) => column.key === 'Ascunsa').title, 'A');
  assert.equal(options.columns.find((column) => column.key === 'Ascunsa').editor, 'checkbox');
  const writes = [];
  env.respond((path, request) => {
    assert.equal(path, '/api/adechit/group-hidden');
    const body = JSON.parse(request.body); writes.push(body);
    if (!body.confirm_active && body.hidden) throw Object.assign(new Error('Grupa are copii fără bifa Plecat.'), { reason: 'GROUP_ACTIVE_CHILDREN' });
    return { IDG: 1, Grupa: 'One', Ascunsa: body.hidden, Version: 2 };
  });
  const row = { IDG: 1, Grupa: 'One', Ascunsa: false, Version: 1 };
  env.context.window.confirm = () => false;
  assert.equal(await options.onCellSave({ row, value: true }), row);
  assert.equal(writes.length, 1);
  env.context.window.confirm = () => true;
  const saved = await options.onCellSave({ row, value: true });
  assert.equal(saved.Ascunsa, true);
  assert.equal(writes.at(-1).confirm_active, true);
  options.onCellSaved();
  assert.equal(refreshes, 1);
  env.context.window.confirm = () => { throw new Error('Unhiding must not ask for confirmation'); };
  assert.equal((await options.onCellSave({ row: saved, value: false })).Ascunsa, false);
});

for (const mobile of [false, true]) test(`payers lazy navigation and stale group replies (mobile=${mobile})`, async () => {
  const env = environment(mobile); const { bindPayers } = await env.load('payers.js');
  bindPayers({ api: env.api, context: () => ({ permissions: ['catalog'] }), refresh() {} });
  const flush = () => new Promise((resolve) => setImmediate(resolve));
  await env.element('ade-payers').listeners.get('click')();
  assert.deepEqual(env.calls, ['/api/adechit/catalog-data?include_hidden=1']);
  assert.equal(env.grids.get('ade-children-list').rows.length, 0);
  const pick = (host, row) => {
    const options = env.grids.get(host).options;
    if (mobile) options.onCellAction({ row }); else options.onSelect(row);
  };
  pick('ade-groups-list', env.groups[0]); await flush();
  assert.equal(env.calls.at(-1), '/api/adechit/rows/Platitori?IDG=1');
  assert.equal(env.grids.get('ade-payers-list').rows.length, 0);
  pick('ade-children-list', env.grids.get('ade-children-list').rows[0]); await flush();
  assert.equal(env.calls.at(-1), '/api/adechit/rows/Platitori_sub?IDP=1');
  assert.equal(env.grids.get('ade-payers-list').rows.length, 1);
  let firstReply; let secondReply;
  env.respond((path) => new Promise((resolve) => { if (path.endsWith('IDG=1')) firstReply = resolve; else secondReply = resolve; }));
  pick('ade-groups-list', env.groups[0]); pick('ade-groups-list', env.groups[1]);
  secondReply({ rows: [{ IDP: 2, IDG: 2 }] }); await flush();
  firstReply({ rows: [{ IDP: 1, IDG: 1 }] }); await flush();
  assert.equal(env.grids.get('ade-children-list').rows[0].IDG, 2);
  assert.equal(env.grids.get('ade-payers-list').rows.length, 0);
});
