// Run with: node --experimental-vm-modules --test PYTHON/tests/adechit_lazy.test.mjs
import { test } from 'node:test';
import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import vm from 'node:vm';

function environment(mobile = false) {
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
      documentElement: { dataset: {} }, addEventListener() {} },
    window: { sessionStorage: { getItem: () => 'token' } }, location: { search: '' },
    localStorage: { getItem() {}, setItem() {} }, addEventListener() {},
    matchMedia: () => ({ matches: mobile, addEventListener() {} }), ResizeObserver: class { observe() {} },
  });
  const groups = [{ IDG: 1, Grupa: 'One' }, { IDG: 2, Grupa: 'Two' }];
  let responder = async (path) => {
    if (path.endsWith('/context')) return { unit: 'preview', email: 'test', limitations: [], permissions: ['catalog'] };
    if (path.endsWith('/years')) return { years: [2026, 2025] };
    if (path.includes('LunaD?Anul=')) return { rows: [{ IDL: 1, Anul: 2026, Luna: 10, LunaT: 'October', Inchisa: false }] };
    if (path.endsWith('/catalog-data')) return { groups };
    if (path.includes('/situation/')) return { rows: [{ IDZ: 1, IDG: Number(path.split('IDG=')[1]), Nume: 'Child' }] };
    if (path.includes('/Platitori?IDG=')) return { rows: [{ IDP: Number(path.split('IDG=')[1]), IDG: Number(path.split('IDG=')[1]), Nume: 'Child' }] };
    if (path.includes('/Platitori_sub?IDP=')) return { rows: [{ IDS: 1, IDP: Number(path.split('IDP=')[1]), Nume: 'Payer', Activ: true }] };
    throw new Error('Unexpected request: ' + path);
  };
  const api = async (path) => { calls.push(path); return responder(path); };
  context.fetch = async (path) => ({ ok: true, status: 200, json: () => api(path) });
  const bindings = { DataGrid, ValueType: {}, TreeView, Combobox, DatePicker: class {},
    showMessage() {}, bindCatalogs() {}, bindAnnual() {}, cnpMessage() {} };
  async function load(file, appended = '') {
    let source = await readFile(new URL('../static/js/adechit/' + file, import.meta.url), 'utf8');
    if (file === 'app.js') source = source.replace('start().catch(report);', 'export const ready = start();');
    const module = new vm.SourceTextModule(source + appended, { context });
    await module.link(async () => new vm.SyntheticModule(Object.keys(bindings), function () {
      for (const [key, value] of Object.entries(bindings)) this.setExport(key, value);
    }, { context }));
    await module.evaluate(); return module.namespace;
  }
  return { load, calls, element, grids, api, groups, respond: (fn) => { responder = fn; } };
}

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
  assert.equal(env.calls.at(-1), '/api/adechit/catalog-data');
  assert.equal(env.grids.get('ade-grid').rows.length, 0);
  assert.equal(app.state.groupTree.nodes.length, 2);
  assert.ok(!app.state.groupTree.nodes.some((row) => row.id === 'group:all'));
  await app.chooseGroup(1);
  assert.equal(env.calls.at(-1), '/api/adechit/situation/1?IDG=1');
  await app.chooseGroup(2);
  assert.equal(env.calls.at(-1), '/api/adechit/situation/1?IDG=2');
  assert.equal(env.grids.get('ade-grid').rows[0].IDG, 2);
});

for (const mobile of [false, true]) test(`payers lazy navigation and stale group replies (mobile=${mobile})`, async () => {
  const env = environment(mobile); const { bindPayers } = await env.load('payers.js');
  bindPayers({ api: env.api, context: () => ({ permissions: ['catalog'] }), refresh() {} });
  const flush = () => new Promise((resolve) => setImmediate(resolve));
  await env.element('ade-payers').listeners.get('click')();
  assert.deepEqual(env.calls, ['/api/adechit/catalog-data']);
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
