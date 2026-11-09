import { ComboTree } from '../components/combotree/combotree.js';
import { confirmBox, reasonBox } from '../utils/confirm-box.js';
import { compareTyped } from '../utils/typed-order.js';
import { Combobox, el, field, scope, grid, column, showMessage, statusBar, preview } from './ui.js';
import { searchOptions } from './search-settings.js';

const path = '/api/vercon/transfer-unitati';
const romanian = new Intl.Collator('ro', { sensitivity: 'base' });
const byLabel = items => [...items].sort((a, b) => romanian.compare(a.label || '', b.label || ''));
const dayLabel = day => (day || '').split('-').reverse().join('.');
const requestKey = () => globalThis.crypto?.randomUUID?.() || String(Date.now()) + Math.random();

export function mountBu(host, opts) {
  const mainUi = scope(), accountsUi = scope(), pickerUi = scope(), linesUi = scope(), historyUi = scope(), dialogs = scope();
  const abort = new AbortController();
  const header = el('div', { className: 'vc-doc__header' });
  const accounts = el('div', { className: 'vc-doc__header' });
  const picker = el('div', { className: 'vc-doc__header' });
  const lines = el('div', { className: 'vc-doc__lines vc-bu__lines' });
  const edit = el('div', { className: 'vc-bu__editor' }, header, accounts, picker, lines);
  const history = el('div', { className: 'vc-doc__header vc-bu__history' });
  const actions = el('div', { className: 'vc-actions' });
  const status = el('span', { className: 'vc-save-status', role: 'status' });
  const root = el('section', { className: 'vc-nom vc-bu' },
    el('h3', { textContent: 'Transferă între subunități' }), history, edit, actions);
  host.replaceChildren(root);
  let context, destination, draft, chosen, productChoices = new Map(), sequence = 0, busy = false, dirty = false, pending;
  const selection = opts.sel();
  const say = (message, bad = false) => {
    if (!abort.signal.aborted) showMessage(status, message, bad ? 'error' : 'info');
  };
  const report = error => { if (error.name !== 'AbortError') say(error.message, true); };
  const query = (extra = {}) => new URLSearchParams(Object.entries({ ...selection, ...extra })
    .filter(([, value]) => value != null && value !== ''));
  const read = (suffix = '', params = {}) => opts.api(path + suffix + '?' + query(params), { signal: abort.signal });
  const button = (label, onClick) => {
    const node = el('button', { type: 'button', textContent: label });
    node.addEventListener('click', () => Promise.resolve(onClick()).catch(report));
    return node;
  };
  const spec = (key, label, type = 'ref', extra = {}) => ({ key, label, type, ...extra });
  const control = (owner, parent, column, value, update) => {
    const input = field(column, value, true, update, owner);
    parent.append(input.node);
    return input.input;
  };
  function combo(owner, parent, label, items, selected, update, options = {}) {
    const container = el('div', { className: 'vc-field__combo' });
    parent.append(el('div', { className: 'vc-field' }, el('span', { textContent: label }), container));
    const choices = items.map(item => ({ ...item, value: String(item.id) }));
    const input = owner.own(new Combobox(container, {
      ...searchOptions(), ...options, allowEmpty: false,
      onSearch: async text => choices.filter(item => item.label.toLocaleLowerCase('ro').includes(text.toLocaleLowerCase('ro'))),
      onSelect: value => update(String(value)),
    }));
    input.setValue(String(selected ?? ''), choices.find(item => item.value === String(selected))?.label || '');
    input.input.setAttribute('aria-label', label);
    return input;
  }
  function draftChanged() { dirty = true; }

  function destinationTree() {
    const groups = new Map(), choices = new Map(), counts = new Map();
    for (const item of context.destinations) counts.set(item.subunit, 1 + (counts.get(item.subunit) || 0));
    for (const item of context.destinations) {
      if (!groups.has(item.subunit)) groups.set(item.subunit, {
        id: 'subunit:' + item.subunit, label: item.subunit_name, selectable: false, children: [],
      });
      const entries = [{ id: null, label: 'Doar NIR în magazie' }, ...item.holders];
      for (const entry of entries) {
        const id = [item.subunit, item.gestiune, entry.id ?? 'warehouse'].join(':');
        const label = (entry.label || '') + (counts.get(item.subunit) > 1 ? ' / ' + item.name : '');
        groups.get(item.subunit).children.push({ id, label, sortValue: entry.label || '', sortType: 'text',
          sortThen: [{ value: item.name, type: 'text' }] });
        choices.set(id, { item, receiver: entry.id });
      }
    }
    const nodes = byLabel(groups.values());
    const container = el('div', { className: 'vc-field__combo' });
    header.append(el('div', { className: 'vc-field' }, el('span', { textContent: 'Destinație *' }), container));
    const tree = mainUi.own(new ComboTree(container, {
      ...searchOptions(), nodes, selectableLevel: 2, selectParents: false, checkable: false,
      placeholder: 'Alegeți magazia sau primitorul…',
      onSelect: ({ id }) => {
        const selected = choices.get(id);
        if (!selected) return;
        const { item, receiver } = selected;
        const modeChanged = (draft.receiver == null) !== (receiver == null);
        if (destination !== item) {
          draft.warehouse = item.warehouse_accounts.length === 1 ? item.warehouse_accounts[0].id : null;
        }
        if (destination !== item || modeChanged) {
          draft.use = receiver != null && item.use_accounts.length === 1 ? item.use_accounts[0].id : null;
        }
        const updateAccounts = destination !== item || modeChanged;
        destination = item;
        Object.assign(draft, { destination_subunit: item.subunit, destination_gestiune: item.gestiune, receiver });
        if (updateAccounts) paintAccounts();
        draftChanged();
      },
    }));
    tree.input.setAttribute('aria-label', 'Destinație');
    tree.input.addEventListener('input', () => {
      destination = null;
      Object.assign(draft, { destination_subunit: null, destination_gestiune: null, receiver: null });
      paintAccounts();
      draftChanged();
    });
  }

  function paintAccounts() {
    accountsUi.clear(); accounts.replaceChildren();
    for (const [key, label, choices] of [
      ['warehouse', 'Cont de magazie la destinație', destination?.warehouse_accounts],
      ['use', 'Cont de folosință la destinație', draft.receiver == null ? null : destination?.use_accounts],
    ]) {
      if (choices?.length > 1) control(accountsUi, accounts, spec(key, label, 'ref', {
        required: true, options: byLabel(choices),
      }), draft[key], value => { draft[key] = value; draftChanged(); });
    }
  }

  const columns = [
    spec('name', 'Produs', 'text'), spec('code', 'Cod / nr. inventar', 'text'), spec('um', 'UM', 'text'),
    spec('available', 'Disponibil', 'decimal'), spec('quantity', 'Cantitate', 'decimal'), spec('price', 'Preț', 'decimal'),
  ];
  function paintLines() {
    linesUi.clear(); lines.replaceChildren();
    const built = columns.map(item => column(item, {
      editable: item.key === 'quantity' && selection.type !== 'MF', width: item.key === 'name' ? 240 : 120,
    }));
    built.push({ key: 'remove', title: '', width: 46, fixedWidth: true, sortable: false,
      filter: false, display: 'button', actionText: '×', actionLabel: 'Elimină linia' });
    grid(lines, {
      columns: built, rows: draft.lines, rowKey: 'key', editable: true, proportionalWidths: true,
      layoutId: 'vercon.bu.lines', footer: true, footerCaption: '{0} produse',
      onCellSave: ({ row, value }) => {
        const line = draft.lines.find(item => item.key === row.key);
        if (!line) throw new Error('Linia a fost scoasă de pe bon.');
        line.quantity = value;
        draftChanged();
        return { quantity: value };
      },
      onCellAction: async ({ row }) => {
        if (!await linesUi.commit()) return;
        draft.lines = draft.lines.filter(item => item.key !== row.key);
        draftChanged(); paintLines(); paintPicker();
      },
    }, linesUi);
  }

  function paintPicker() {
    pickerUi.clear(); picker.replaceChildren();
    const available = [...productChoices.values()].filter(item => !draft.lines.some(row => row.key === item.key));
    if (!available.length) return;
    let picked;
    const input = combo(pickerUi, picker, 'Produs disponibil', available, '', value => { picked = productChoices.get(value); });
    input.input.addEventListener('input', () => { picked = null; });
    const add = button('Adaugă produs', async () => {
      if (!picked || !await linesUi.commit()) return;
      draft.lines.push({ ...picked, quantity: selection.type === 'MF' ? '1' : picked.available });
      draftChanged(); paintLines(); paintPicker();
    });
    picker.append(el('div', { className: 'vc-bu__add' }, add));
  }

  async function loadEligible(reset = false) {
    const revision = ++sequence;
    productChoices.clear(); paintPicker();
    if (reset) { draft.lines = []; paintLines(); }
    if (!draft.date) return;
    const result = await read('/eligible', { date: draft.date, giver: draft.giver });
    if (abort.signal.aborted || revision !== sequence) return;
    const rows = result.lines.map(item => {
      const key = [item.variant, item.origin || '', item.IdMasina || '', item.PUTVA].join(':');
      const detail = [item.code, item.category_name || item.account, dayLabel(item.date), item.PUTVA + ' lei'].filter(Boolean);
      return { id: key, key, variant: item.variant, batch: item.origin, name: item.name || '', code: item.code || '',
        label: [item.name, ...detail].filter(Boolean).join(' / '), price: item.PUTVA, available: item.left, um: item.um,
        sortType: 'text', sortValue: item.name || '', sortThen: [
          { value: item.code, type: 'text' }, { value: item.category_name || item.account, type: 'text' },
          { value: item.date, type: 'date' }, { value: item.PUTVA, type: 'number' },
        ] };
    });
    productChoices = new Map(rows.map(row => [row.key, row]));
    paintPicker();
    if (!rows.length) say('Sursa aleasă nu are produse disponibile la această dată.');
  }

  function paintDraft() {
    mainUi.clear(); header.replaceChildren();
    control(mainUi, header, spec('date', 'Data transferului', 'date', { required: true }), draft.date, value => {
      if (value === draft.date) return;
      draft.date = value; draftChanged(); loadEligible().catch(report);
    });
    control(mainUi, header, spec('giver', 'Predător', 'ref', {
      options: [{ id: 'warehouse', label: 'Magazie' }, ...byLabel(context.holders)],
    }), draft.giver || 'warehouse', value => {
      draft.giver = value === 'warehouse' ? null : value;
      draftChanged(); loadEligible(true).catch(report);
    });
    destinationTree();
    control(mainUi, header, spec('explanation', 'Explicație', 'text', { max: 255 }), draft.explanation,
      value => { draft.explanation = value; draftChanged(); });
    paintAccounts(); paintLines(); paintPicker();
  }

  function resetDraft() {
    destination = null;
    draft = { date: context.today, giver: null, explanation: '', lines: [] };
    dirty = false; paintDraft();
    return loadEligible();
  }

  function paintHistory() {
    historyUi.clear(); history.replaceChildren();
    const label = doc => `BU ${doc.id} · ${dayLabel(doc.day)} · ${doc.subunit} / ${doc.gestiune} / ${doc.bp == null ? 'Doar NIR în magazie' : doc.receiver || ''}${doc.cancelled ? ' · Anulat' : ''}`;
    const saved = [...context.documents].sort((a, b) => compareTyped(b.day, a.day, 'date') || compareTyped(b.id, a.id, 'number'))
      .map(doc => ({ id: doc.id, label: label(doc), sortValue: doc.day, sortType: 'date',
        sortThen: [{ value: doc.id, type: 'number' }] }));
    combo(historyUi, history, 'Bonuri BU salvate', saved, chosen?.id,
      value => { chosen = context.documents.find(doc => String(doc.id) === value); paintHistory(); }, { sortDirection: 'desc' });
    if (!chosen) return;
    const open = button('Previzualizează', async () => {
      const doc = chosen, details = await read('/' + doc.id);
      if (abort.signal.aborted) return;
      const title = [doc.giver || 'Magazie', 'NIR ' + nirNumber(doc), doc.bp_number, doc.explanation, doc.reason]
        .filter(Boolean).join(' · ');
      await preview('Bon de transfer între subunități · ' + label(doc), [{ title,
        columns: [...columns.filter(item => item.key !== 'available'), spec('value', 'Valoare', 'decimal')].map(item => column(item)),
        rows: details.lines }], dialogs, { date: doc.day });
    });
    const cancel = button('Anulează BU', async () => {
      const doc = chosen;
      if (doc.cancelled || busy) return;
      const reason = await reasonBox('Anulați BU ' + doc.id + '?', { title: 'Anulare BU', yes: 'Anulează', no: 'Renunță' });
      if (reason == null) return;
      await submit('/' + doc.id + '/cancel', { reason });
    });
    cancel.disabled = !!chosen.cancelled || !!pending;
    history.append(el('div', { className: 'vc-bu__add' }, open, cancel));
  }

  function nirNumber(doc) {
    return doc.nir_number + (doc.nir_sub ? ',' + String(doc.nir_sub).padStart(2, '0').replace(/0$/, '') : '');
  }

  async function refresh() {
    context = await read();
    if (abort.signal.aborted) return;
    chosen = context.documents.find(doc => doc.id === chosen?.id);
    paintHistory();
  }

  async function submit(suffix, values) {
    if (busy) return;
    pending ||= { suffix, body: { ...selection, ...values, request_key: requestKey() } };
    busy = true; edit.inert = history.inert = true; save.disabled = true; say('Se salvează…');
    let result;
    try {
      result = await opts.api(path + pending.suffix, {
        method: 'POST', headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(pending.body), signal: abort.signal,
      });
      pending = null;
    } catch (error) {
      if (error.status && error.status < 500) pending = null;
      if (pending) {
        say('Răspunsul nu a fost primit. Reîncercați aceeași operație.', true);
        return;
      }
      throw error;
    } finally {
      busy = false;
      if (!abort.signal.aborted) {
        edit.inert = history.inert = !!pending;
        save.disabled = false;
        save.textContent = pending ? 'Reîncearcă operația' : 'Salvează BU';
      }
    }
    if (abort.signal.aborted) return;
    chosen = { id: result.id };
    await refresh();
    if (!suffix) await resetDraft();
    if (suffix) say(`${result.number} a fost anulat.`);
    else {
      const nir = `NIR ${nirNumber(chosen)}`;
      const receipt = result.bp ? `În destinație au fost create ${nir} și ${result.bp}.` : `În destinație a fost creat ${nir} în magazie.`;
      say(`Salvat ${result.number}. ${receipt}`);
    }
  }

  const save = button('Salvează BU', async () => {
    if (pending) return submit(pending.suffix);
    if (!await mainUi.commit() || !await linesUi.commit() || !mainUi.validate() || !accountsUi.validate() ||
      !destination || !draft.lines.length) {
      say('Completați data, destinația, conturile și produsele.', true);
      return;
    }
    const source = header.querySelector('[data-field="giver"] input[type="hidden"]');
    if (source?._combo?._pendingSelection) { say('Alegeți magazia sau un predător din listă.', true); return; }
    const { lines: rows, ...values } = draft;
    await submit('', { ...values, lines: rows.map(row => ({
      variant: row.variant, batch: row.batch, PUTVA: row.price, Cant: row.quantity,
    })) });
  });
  const discard = button('Renunță', async () => {
    if (busy || pending || !context) return;
    if (dirty && !await confirmBox('Renunțați la datele nesalvate?', { title: 'Renunțare', yes: 'Renunț', no: 'Rămân aici' })) return;
    await resetDraft(); say('');
  });
  statusBar(actions, { start: [discard], status, end: [save] });
  async function load() {
    save.disabled = true;
    await refresh();
    if (abort.signal.aborted) return;
    await resetDraft();
    save.disabled = false;
  }
  const ready = load().catch(report);
  return {
    ready,
    isDirty: () => dirty || !!pending || busy || mainUi.dirty() || linesUi.dirty(),
    destroy() {
      abort.abort();
      for (const owner of [mainUi, accountsUi, pickerUi, linesUi, historyUi, dialogs]) owner.clear();
      root.remove();
    },
  };
}
