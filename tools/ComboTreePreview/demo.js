import { DataGrid } from '/static/js/dgv/datagrid.js';

const nodes = [
  { id: 'account:alimente', label: 'ALIMENTE · Produse alimentare', children: [
    { id: 'p:1', label: 'ALMETTE CU SMÂNTÂNĂ 250G', um: 'BUC' },
    { id: 'p:2', label: 'ALMETTE SMÂNTÂNĂ 150G', um: 'BUC' },
    { id: 'p:3', label: 'ALPRO PRODUS FERMENTAT DIN SOIA 150G', um: 'BUC' },
    { id: 'p:4', label: 'AMESTEC 10 LEGUME 2.5KG', um: 'KG' },
    { id: 'p:5', label: 'AMESTEC MEXICAN 2.5KG', um: 'KG' },
    { id: 'p:6', label: 'AMESTEC PENTRU CIORBĂ 2.5KG', um: 'KG' },
    { id: 'p:7', label: 'LAPTE 1L', um: 'L' },
    { id: 'p:8', label: 'FĂINĂ ALBĂ 1KG', um: 'KG' },
  ] },
  { id: 'account:materiale', label: 'MATERIALE · Consumabile', children: [
    { id: 'p:9', label: 'HÂRTIE A4 80G · TOP 500 COLI', um: 'TOP' },
    { id: 'p:10', label: 'DETERGENT PENTRU SUPRAFEȚE 5L', um: 'BUC' },
    { id: 'p:11', label: 'SACI MENAJERI 120L', um: 'SET' },
    { id: 'p:12', label: 'MĂNUȘI DE PROTECȚIE', um: 'PER' },
  ] },
  { id: 'account:inventar', label: 'INVENTAR · Mobilier și echipamente', children: [
    { id: 'branch:mobilier', label: 'Mobilier · subramură de probă', children: [
      { id: 'p:13', label: 'SCAUN BIROU REGLABIL', um: 'BUC' },
      { id: 'p:14', label: 'DULAP CU DOUĂ UȘI', um: 'BUC' },
    ] },
    { id: 'p:15', label: 'MONITOR 27 INCH', um: 'BUC' },
  ] },
];
const catalog = new Map();
function index(items) {
  for (const item of items) { catalog.set(item.id, item); if (item.children) index(item.children); }
}
index(nodes);
const $ = id => document.getElementById(id);
let grid, rows;
const status = (text, error = false) => {
  $('status').textContent = text;
  $('status').classList.toggle('is-error', error);
};
function log(text) {
  const item = document.createElement('li');
  item.textContent = `${new Date().toLocaleTimeString('ro-RO')} · ${text}`;
  $('events').prepend(item);
  while ($('events').children.length > 30) $('events').lastElementChild.remove();
}
function reset() {
  rows = Array.from({ length: 18 }, (_, i) => ({ id: i + 1, product: null,
    um: i < 2 ? 'BUC' : '', quantity: i < 2 ? 1.25 : '',
    price: i < 2 ? '0.5550' : null, amount: i < 2 ? 1234.5 : null,
    note: i < 2 ? 'Text de probă' : '' }));
  $('selection').textContent = 'Nicio selecție încă.';
  $('events').replaceChildren();
  build();
}
function build() {
  grid?.destroy();
  $('grid').replaceChildren();
  $('grid').classList.toggle('is-large-type', $('font-size').value === '20');
  grid = new DataGrid($('grid'), {
    rows, rowKey: 'id', editable: true, singleClickEdit: true,
    autoSize: false, enableGrouping: false, rowHeight: 34, headerHeight: 36,
    columns: [
      { key: 'id', title: '#', width: 48, fixedWidth: true, align: 'center', filter: false, sortable: false },
      { key: 'product', title: 'Produs', width: Number($('width').value), editable: true, editor: 'tree',
        nodes, selectParents: $('parents').checked, dropDownMinWidth: Number($('minimum').value), focusAfterSelect: 'quantity',
        filter: false, sortable: false, formatter: value => catalog.get(value)?.label || '' },
      { key: 'um', title: 'UM', width: 72, align: 'center', editable: true, editor: 'text', filter: false, sortable: false },
      { key: 'quantity', title: 'Cantitate', width: 120, valueType: 'number', format: 'fixed', decimals: 2,
        editable: true, editor: 'number', nullable: true, filter: false, sortable: false },
      { key: 'price', title: 'Preț', width: 150, valueType: 'number', format: 'fixed', decimals: 4,
        editable: true, editor: 'number', nullable: true, filter: false, sortable: false },
      { key: 'amount', title: 'Valoare', width: 150, valueType: 'number', format: 'standard', decimals: 2,
        editable: true, editor: 'number', nullable: true, filter: false, sortable: false },
      { key: 'note', title: 'Observații', width: 220, editable: true, editor: 'text', filter: false, sortable: false },
    ],
    onCellSave: ({ row, key, value }) => {
      const saved = { ...row, [key]: value };
      if (key === 'product') saved.um = catalog.get(value)?.um || '—';
      Object.assign(rows.find(item => item.id === row.id), saved);
      $('selection').textContent = JSON.stringify(saved, null, 2);
      log(`Rând ${row.id}: ${key} = ${value ?? 'gol'}`);
      status(`Rândul ${row.id} actualizat: ${catalog.get(saved.product)?.label || 'fără produs'}.`);
      return saved;
    },
    onEditError: ({ error }) => { status(error.message, true); log(`Eroare: ${error.message}`); },
    onEditState: ({ state, key }) => {
      if (state === 'editing') status(`Editare ${key}. Părinți selectabili: ${$('parents').checked ? 'DA' : 'NU'}.`);
    },
  });
  status('Grila este pregătită. Click într-o celulă Produs.');
}
for (const id of ['parents', 'width', 'minimum', 'font-size']) $(id).addEventListener('change', () => {
  build(); log('Opțiunile grilei au fost schimbate.');
});
$('reset').addEventListener('click', reset);
window.addEventListener('pagehide', () => grid?.destroy(), { once: true });
reset();
