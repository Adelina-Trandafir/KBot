import { DataGrid, ValueType } from '../dgv/datagrid.js';
import { showMessage } from '../portal/messages.js';
import { bindPayers } from './payers.js';

const text = (key, title, width = 150) => ({ key, title, width, valueType: ValueType.Text, editable: true, filter: key === 'Nume' || key === 'Grupa' });
const flag = (key, title) => ({ ...text(key, title, 100),
  valueType: ValueType.Boolean, format: 'yesNo',
  parse: (raw) => {
    const value = raw.trim().toLowerCase();
    if (!['da', 'nu', '1', '0'].includes(value)) throw new Error('Introduceți Da sau Nu.');
    return value === 'da' || value === '1';
  } });

export function bindCatalogs({ api, context, refresh }) {
  const dialog = document.getElementById('ade-catalog');
  const content = document.getElementById('ade-catalog-content');
  const message = document.getElementById('ade-catalog-message');
  const save = document.getElementById('ade-catalog-save');
  let grids = [];
  let drafts = new Map();
  let busy = false;
  let loaded = false;
  let sequence = 0;
  let request = null;
  const error = (failure) => { console.error('[ADE catalog]', failure); showMessage(message, failure.message, 'error'); };
  const hint = () => showMessage(message, 'Pe PC editați cu un clic; pe mobil cu dublu clic. Enter confirmă și trece la următoarea celulă editabilă. Pentru bife scrieți Da sau Nu. Salvați pentru a închide fereastra.', 'info');
  const grid = (host, table, rows, columns, onSelect) => {
    const key = context().schema[table].key;
    const instance = new DataGrid(host, { columns, rows, rowKey: key, mobileRowScale: 1.2, editable: context().permissions.includes('catalog'),
      theme: document.documentElement.dataset.theme === 'dark' ? 'dark' : 'modern',
      footer: true, footerCaption: '{0} înregistrări', layoutId: `ade.catalog.${table}`, onSelect,
      onCellSave: ({ row, key: field, value }) => {
        if (busy) throw new Error('Așteptați finalizarea salvării.');
        const token = `${table}:${row[key]}`;
        const draft = drafts.get(token) || { table, id: row[key] < 0 ? null : row[key], version: row.Version, values: {} };
        draft.values[field] = value;
        drafts.set(token, draft);
        return { ...row, [field]: value };
      }, onEditError: ({ error: failure }) => error(failure) });
    grids.push(instance);
    return instance;
  };
  const finishEdits = async () => {
    for (const instance of grids) if (instance.hasEdit && !await instance.commitEdit()) return false;
    return true;
  };
  const addButton = (label, callback) => {
    const button = document.createElement('button');
    button.type = 'button'; button.className = 'btn'; button.textContent = label;
    button.disabled = !context().permissions.includes('catalog');
    button.addEventListener('click', () => callback().catch(error));
    return button;
  };
  async function open(kind) {
    if (dialog.open || !context()) return;
    drafts = new Map(); grids = []; request = null; loaded = false;
    save.disabled = true; content.replaceChildren();
    document.getElementById('ade-catalog-title').textContent = kind === 'taxes' ? 'Taxe' : 'Plătitori';
    dialog.showModal(); hint();
    try {
      if (kind === 'taxes') {
        const rows = (await api('/api/adechit/rows/ValoriTaxe')).rows;
        const host = document.createElement('div'); host.className = 'ade-catalog-grid';
        const instance = grid(host, 'ValoriTaxe', rows, [
          { ...text('TaxaZilnica', 'Taxă zilnică', 140), editor: 'number', valueType: ValueType.Number,
            validate: (value) => value >= 0 ? '' : 'Taxa nu poate fi negativă.' }, text('Expl', 'Explicație', 350), flag('Activ', 'Activă')]);
        content.append(addButton('Adaugă taxă', async () => {
          if (busy || !await finishEdits()) return;
          const row = { IDV: --sequence, TaxaZilnica: 0, Expl: '', Activ: false };
          drafts.set(`ValoriTaxe:${row.IDV}`, { table: 'ValoriTaxe', id: null, values: { TaxaZilnica: 0, Expl: '', Activ: false } });
          rows.push(row); instance.setRows(rows); instance.beginEdit(row, 'TaxaZilnica');
        }), host);
      }
      loaded = true; save.disabled = false;
    } catch (failure) {
      error(failure); save.disabled = false; save.textContent = 'Închide';
    }
  }
  dialog.addEventListener('cancel', (event) => event.preventDefault());
  save.addEventListener('click', async () => {
    if (busy) return;
    try {
      if (!await finishEdits()) return;
      busy = true; save.disabled = true; content.inert = true;
      if (drafts.size) {
        const body = JSON.stringify({ items: [...drafts.values()] });
        if (!request || request.body !== body) request = { body, key: crypto.randomUUID() };
        await api('/api/adechit/catalog-save', { method: 'POST', ...request });
        drafts.clear();
      }
      dialog.close(); grids.forEach((instance) => instance.destroy()); grids = [];
      if (loaded) await refresh();
    } catch (failure) { error(failure); }
    finally { busy = false; content.inert = false; save.disabled = false; save.textContent = 'Salvează și închide'; }
  });
  addEventListener('beforeunload', (event) => {
    if (dialog.open && (drafts.size || grids.some((instance) => instance.hasEdit))) { event.preventDefault(); event.returnValue = ''; }
  });
  const payers = bindPayers({ api, context, refresh });
  document.getElementById('ade-taxes').addEventListener('click', () => open('taxes'));
  return payers;
}
