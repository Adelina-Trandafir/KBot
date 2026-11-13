// slice VC14-03: Bon transfer intre subunitati (BU). The page is the Bon Transfer (BT) page: title bar, header,
// lines grid with the always-open new row, action bar, saved-documents tree on the right. What BU adds to BT is the
// choice of the destination unit (and gestiune when the unit has several) and the OPTIONAL receiver.
import { confirmBox, reasonBox } from '../utils/confirm-box.js';
import { ActionAddButton } from '../components/action-add-button/action-add-button.js';
import { documentTreeOrder } from './document-tree-order.js';
import { oiLineValue } from './oi-values.js';
import { format } from './fields.js';
import { el, scope, field, column, grid, paintTree, refreshTree, markSelected, rowMenu, openMenu, showMessage, statusBar, preview } from './ui.js';

const path = '/api/vercon/transfer-unitati';
const WAREHOUSE = 'warehouse';
const romanian = new Intl.Collator('ro', { sensitivity: 'base' });
const byLabel = items => [...items].sort((a, b) => romanian.compare(a.label || '', b.label || ''));
const dayLabel = day => (day || '').split('-').reverse().join('.');
const newKey = () => globalThis.crypto?.randomUUID?.() || String(Date.now()) + Math.random();
const number = value => value == null || String(value).trim() === '' ? null : Number(String(value).replace(',', '.'));
const options = items => items.map(item => ({ value: String(item.id), label: item.label }));

export function mountBu(host, opts) {
  const abort = new AbortController();
  const headerUi = scope(), linesUi = scope(), treeUi = scope(), dialogs = scope();
  const selection = opts.sel(), isMf = selection.type === 'MF';
  const state = { context: null, docs: [], doc: null, details: [], mode: 'view', draft: null, eligible: [], zec: 4,
    dirty: false, busy: false, pending: null };
  const inputs = new Map();
  let linesGrid = null, savedTree = null, treeAddButton = null, treeDirty = true, eligibleSequence = 0, openSequence = 0;

  const title = el('h3'), pill = el('span', { className: 'vc-pill' });
  const headerHost = el('div', { className: 'vc-grid vc-doc__header vc-doc__header--oi' });
  const linesHost = el('div', { className: 'vc-doc__lines' }), bar = el('div', { className: 'vc-actions vc-doc__bar' });
  const status = el('span', { className: 'vc-save-status', role: 'status' });
  const main = el('div', { className: 'vc-doc__main' }, el('div', { className: 'vc-doc__titlebar' }, title, pill), headerHost, linesHost, bar);
  const savedHost = el('aside', { className: 'vc-doc__tree' }), treeHost = el('div', { className: 'vc-receipt-tree' });
  savedHost.append(treeHost);
  const root = el('section', { className: 'vc-doc vc-doc--bt vc-doc--bu' }, main, savedHost);
  host.replaceChildren(root);

  const editing = () => state.mode === 'add';
  const say = (text, bad = false) => { if (!abort.signal.aborted) showMessage(status, text, bad ? 'error' : 'info'); };
  const report = error => { if (error.name !== 'AbortError') say(error.message || 'Operația a eșuat.', true); };
  const clearRowError = () => { if (status.getAttribute('role') === 'alert') say(''); };
  const query = (extra = {}) => new URLSearchParams(Object.entries({ ...selection, ...extra })
    .filter(([, value]) => value != null && value !== ''));
  const read = (suffix = '', params = {}) => opts.api(path + suffix + '?' + query(params), { signal: abort.signal });
  const button = (label, onClick, attrs = {}) => {
    const node = el('button', { type: 'button', textContent: label, ...attrs });
    node.addEventListener('click', () => Promise.resolve(onClick()).catch(report), { signal: abort.signal });
    return node;
  };

  // ---- destination ------------------------------------------------------------------------------------------------
  const destinations = () => state.context?.destinations || [];
  const units = () => [...new Map(destinations().map(item => [String(item.subunit), { id: item.subunit, label: item.subunit_name }])).values()];
  const gestiuni = () => destinations().filter(item => String(item.subunit) === String(state.draft.unit))
    .map(item => ({ id: item.gestiune, label: item.name }));
  const destination = () => destinations().find(item => String(item.subunit) === String(state.draft.unit)
    && String(item.gestiune) === String(state.draft.gestiune));
  const pickAccount = (list, current) => list.some(item => String(item.id) === String(current)) ? String(current)
    : list.length === 1 ? String(list[0].id) : '';
  function syncDestination() {
    const draft = state.draft, choices = gestiuni();
    if (!choices.some(item => String(item.id) === String(draft.gestiune))) draft.gestiune = choices.length === 1 ? String(choices[0].id) : '';
    const target = destination();
    if (!target || !target.holders.some(item => String(item.id) === String(draft.receiver))) draft.receiver = '';
    draft.warehouse = target ? pickAccount(target.warehouse_accounts, draft.warehouse) : '';
    draft.use = target && draft.receiver ? pickAccount(target.use_accounts, draft.use) : '';
  }

  // ---- header -----------------------------------------------------------------------------------------------------
  function addField(spec, value, editable, onChange) {
    const item = field(spec, value, editable, onChange, headerUi);
    headerHost.append(item.node);
    inputs.set(spec.key, item);
    return item;
  }
  const ref = (key, label, items, extra = {}) => ({ key, label, type: 'ref', required: true, showArrow: true, options: options(byLabel(items)), ...extra });
  const changed = () => { state.dirty = true; clearRowError(); };
  // a chosen value of a list field, or '' when the operator typed over it without choosing
  const chosen = key => inputs.get(key)?.input.value ?? String(state.draft[key] ?? '');

  function paintHeader() {
    headerUi.clear(); headerHost.replaceChildren(); inputs.clear();
    if (!editing() && !state.doc) {
      headerHost.append(el('p', { textContent: 'Alegeți un document din arbore sau adăugați unul nou.' }));
      return;
    }
    if (!editing()) { paintSavedHeader(); return; }
    const draft = state.draft, target = destination();
    addField(ref('giver', 'Predător', [], { options: options([{ id: WAREHOUSE, label: 'Magazie' }, ...byLabel(state.context.holders)]) }), draft.giver, true,
      value => changeGiver(value).catch(report));
    addField(ref('unit', 'Unitate destinație', units()), draft.unit, true, value => {
      draft.unit = value; syncDestination(); changed(); paintHeader();
    });
    addField({ key: 'Numar', label: 'Număr BU', type: 'text' }, 'automat', false);
    if (gestiuni().length > 1) addField(ref('gestiune', 'Gestiune destinație', gestiuni()), draft.gestiune, true, value => {
      draft.gestiune = value; syncDestination(); changed(); paintHeader();
    });
    addField(ref('receiver', 'Primitor (opțional)', target?.holders || [], { required: false, placeholder: 'Doar NIR în magazie' }),
      draft.receiver, true, value => { draft.receiver = value; syncDestination(); changed(); paintHeader(); });
    addField({ key: 'Data', label: 'Data', type: 'date', required: true }, draft.date, true, value => {
      draft.date = value; changed();
      if (value) reloadEligible().catch(report);
    });
    if (target?.warehouse_accounts.length > 1) addField(ref('warehouse', 'Cont de magazie la destinație', target.warehouse_accounts),
      draft.warehouse, true, value => { draft.warehouse = value; changed(); });
    if (draft.receiver && target?.use_accounts.length > 1) addField(ref('use', 'Cont de folosință la destinație', target.use_accounts),
      draft.use, true, value => { draft.use = value; changed(); });
    addField({ key: 'Expl', label: 'Explicații', type: 'text', max: 255 }, draft.explanation, true, value => { draft.explanation = value; changed(); });
  }

  function paintSavedHeader() {
    const doc = state.doc, plain = (key, label, value) => addField({ key, label, type: 'text' }, value || '', false);
    plain('giver', 'Predător', doc.giver || 'Magazie');
    plain('unit', 'Unitate destinație', doc.subunit);
    plain('Numar', 'Număr BU', String(doc.id));
    plain('gestiune', 'Gestiune destinație', doc.gestiune);
    plain('receiver', 'Primitor', doc.bp == null ? 'Doar NIR în magazie' : doc.receiver);
    plain('Data', 'Data', dayLabel(doc.day));
    plain('created', 'Documente create în destinație', ['NIR ' + nirNumber(doc), doc.bp_number ? 'BP ' + doc.bp_number : ''].filter(Boolean).join(' · '));
    plain('Expl', 'Explicații', doc.explanation);
    if (doc.cancelled) headerHost.append(el('p', { textContent: `Motiv anulare: ${doc.reason || ''}` }));
  }

  function nirNumber(doc) {
    return doc.nir_number + (doc.nir_sub ? ',' + String(doc.nir_sub).padStart(2, '0').replace(/0$/, '') : '');
  }

  async function changeGiver(value) {
    const draft = state.draft, previous = draft.giver;
    if (value === previous) return;
    if (!await linesUi.commit()) { paintHeader(); return; }
    if (draft.lines.some(line => !isBlank(line))
      && !await confirmBox('Schimbarea predătorului elimină produsele selectate din ciornă.', { yes: 'Schimbă predătorul', no: 'Păstrează' })) {
      paintHeader(); return;
    }
    draft.giver = value; draft.lines = []; changed();
    await reloadEligible();
    paintLines();
  }

  // The lines grid stays locked until the required header fields are filled.
  function headerLock() {
    if (!editing()) return '';
    const missing = [['giver', 'Predător'], ['unit', 'Unitate destinație'], ...(gestiuni().length > 1 ? [['gestiune', 'Gestiune destinație']] : []), ['Data', 'Data']]
      .filter(([key]) => !(key === 'Data' ? inputs.get(key)?.input.value : chosen(key))).map(([, label]) => label);
    return missing.length ? `Completați mai întâi antetul documentului: ${missing.join(', ')}.` : '';
  }

  // ---- lines ------------------------------------------------------------------------------------------------------
  const isBlank = line => !line.choice && (line.Cant == null || String(line.Cant).trim() === '');
  const newLine = (values = {}) => ({ key: newKey(), ...values });
  const choiceOf = key => state.eligible.find(item => item.key === key);
  const draftRows = () => state.draft.lines.map((line, index) => ({ ...line, _index: index + 1 }));
  const savedRows = () => state.details.map((line, index) => ({ key: 'saved:' + index, _index: index + 1, name: line.name, code: line.code, um: line.um,
    Cant: line.quantity, PUTVA: line.price, ValoareCUTVA: line.value }));

  function toChoice(item) {
    const account = item.category_name || item.account || '';
    const key = [item.variant, item.origin || '', item.IdMasina || '', item.PUTVA].join(':');
    return { key, variant: item.variant, batch: item.origin, name: item.name || '', code: item.code || '', um: item.um || '',
      account, available: item.left, price: item.PUTVA,
      label: [item.name, item.code, dayLabel(item.date), item.PUTVA + ' lei', 'disponibil ' + format(item.left, { type: 'decimal' })].filter(Boolean).join(' / ') };
  }

  async function reloadEligible() {
    const draft = state.draft, sequence = ++eligibleSequence, day = draft.date;
    state.eligible = [];
    if (!day) return;
    const result = await read('/eligible', { date: day, giver: draft.giver === WAREHOUSE ? '' : draft.giver });
    if (abort.signal.aborted || sequence !== eligibleSequence || draft !== state.draft) return;
    state.eligible = result.lines.map(toChoice);
    // the quantities already chosen are judged against the stock of the new date / predecessor
    for (const line of draft.lines) line.left = choiceOf(line.choice)?.available ?? '0';
    if (!state.eligible.length) say('Sursa aleasă nu are produse disponibile la această dată.');
    if (linesGrid && !linesGrid.hasEdit) linesGrid.setRows(draftRows(), { keepWidths: true });
  }

  function productNodes(row) {
    const used = new Set(state.draft.lines.filter(line => line.key !== row?.key && line.choice).map(line => line.choice));
    const groups = new Map();
    for (const item of state.eligible) {
      if (used.has(item.key)) continue;
      const name = item.account || 'Fără cont';
      if (!groups.has(name)) groups.set(name, { id: 'bu-account:' + name, label: name, children: [] });
      groups.get(name).children.push({ id: item.key, label: item.label });
    }
    return [...groups.values()];
  }

  function problems(line) {
    const missing = [];
    if (!line.choice) missing.push('Produs');
    const quantity = number(line.Cant);
    if (!(quantity > 0) || quantity > number(line.left ?? 0)) missing.push('Cantitate');
    return missing;
  }
  const rowIssue = row => {
    if (isBlank(row)) return '';
    const missing = problems(row);
    return missing.length ? `Rândul ${row._index} nu este complet. Lipsesc sau sunt greșite: ${missing.join(', ')}.` : '';
  };
  const confirmRowIssue = async ({ message }) => await confirmBox(`${message} Continuați editarea rândului?`,
    { title: 'Rând incomplet', yes: 'Continuă editarea', no: 'Anulează rândul' }) ? 'continue' : 'cancel';

  function lineColumns() {
    const edit = editing(), common = { filter: !edit, sortable: !edit, editable: false };
    const columns = [{ key: '_index', title: '#', width: 46, fixedWidth: true, filter: false, sortable: false, align: 'center' }];
    if (edit) columns.push(column({ key: 'choice', label: 'Produs', type: 'ref', required: true }, { ...common, width: 234, showArrow: true,
      editable: true, editor: 'tree', searchable: true, nodes: productNodes, selectParents: false, dropDownMinWidth: 430,
      ...(isMf ? {} : { focusAfterSelect: 'Cant' }),
      formatter: value => choiceOf(value)?.name || state.draft.lines.find(line => line.choice === value)?.name || '',
      validate: value => value == null || value === '' ? 'Alegeți un produs din listă.' : null }));
    else columns.push(column({ key: 'name', label: 'Produs' }, { ...common, width: 234 }));
    columns.push(column({ key: 'um', label: 'UM' }, { ...common, width: 64, align: 'center' }));
    columns.push(edit ? column({ key: 'left', label: 'Disponibil', type: 'decimal' }, { ...common, width: 128 })
      : column({ key: 'code', label: 'Cod / nr. inventar' }, { ...common, width: 128 }));
    columns.push(column({ key: 'Cant', label: 'Cantitate', type: 'decimal', required: true }, { ...common, width: 90, editable: edit && !isMf }));
    columns.push(column({ key: 'PUTVA', label: 'Preț cu TVA (lei)', type: 'decimal' }, { ...common, width: 128 }));
    columns.push(column({ key: 'ValoareCUTVA', label: 'Valoare (lei)', type: 'decimal' }, { ...common, width: 128 }));
    if (edit) columns.push({ key: '_action', title: '⋮', width: 46, fixedWidth: true, filter: false, sortable: false, display: 'button', actionText: '⋮', actionLabel: 'Opțiuni linie' });
    return columns;
  }

  function dropLine(line) {
    state.draft.lines = state.draft.lines.filter(other => other.key !== line.key);
    linesGrid.setRows(draftRows(), { keepWidths: true });
    clearRowError();
  }

  async function saveCell({ row, key: name, value }) {
    const line = state.draft.lines.find(item => item.key === row.key);
    if (!line) throw new Error('Linia nu mai este în document.');
    const before = structuredClone(line);
    try {
      if (name === 'choice') {
        const item = choiceOf(value);
        if (!item) throw new Error('Alegeți un produs disponibil.');
        if (state.draft.lines.some(other => other !== line && other.choice === item.key)) throw new Error('Produsul din acest lot și preț este deja pe bon.');
        Object.assign(line, { choice: item.key, variant: item.variant, batch: item.batch, name: item.name, code: item.code, um: item.um,
          left: item.available, PUTVA: item.price, Cant: isMf ? '1' : '' });
      } else line[name] = value;
      if (name === 'Cant') {
        if (!(number(line.Cant) > 0)) throw new Error('Cantitatea trebuie să fie pozitivă.');
        if (number(line.Cant) > number(line.left ?? 0)) throw new Error('Cantitatea depășește disponibilul la data bonului.');
      }
      line.ValoareCUTVA = oiLineValue(line.Cant, line.PUTVA, state.zec);
    } catch (error) {
      Object.keys(line).forEach(item => delete line[item]);
      Object.assign(line, before);
      throw error;
    }
    state.dirty = true;
    return { ...line };
  }

  function paintLines() {
    linesUi.clear(); linesGrid = null; linesHost.replaceChildren();
    if (!editing() && !state.doc) return;
    const edit = editing(), body = el('div', { className: 'vc-doc__grid' });
    linesHost.append(el('h4', { textContent: 'Produse transferate' }), body);
    linesGrid = grid(body, { columns: lineColumns(), rows: edit ? draftRows() : savedRows(), rowKey: 'key', proportionalWidths: true,
      footer: true, footerCaption: '', layoutId: `vercon.${selection.type}.bu.lines.${edit ? 'edit' : 'view'}-v1`,
      ...(edit ? {
        editable: true, allowIncompleteEdits: true, singleClickEdit: true, newRowAlwaysVisible: true,
        suspendRowLeave: () => state.busy, isBlankRow: isBlank, onCancelNewRow: dropLine, onRowLeave: clearRowError,
        rowIssue, confirmRowIssue, editGuard: headerLock,
        onEditBlocked: ({ message }) => { say(message, true); headerUi.validate(); },
        onNewRow: () => { const line = newLine(); state.draft.lines.push(line); line._index = state.draft.lines.length; return line; },
        onCellSave: saveCell,
        onCellSaved: ({ row }) => { if (!rowIssue(row)) clearRowError(); },
        onEditError: ({ error }) => say(error.message, true),
        onCellAction: ({ row }) => rowMenu(linesGrid, row, [{ icon: '×', label: 'Scoate rândul', run: async () => {
          if (!await linesUi.commit()) return;
          dropLine(row); state.dirty = true;
        } }]),
      } : { editable: false }) }, linesUi);
  }

  // ---- saved documents tree --------------------------------------------------------------------------------------
  const bonOrder = documentTreeOrder({ party: 'subunit', initialByDate: true });
  const treeDoc = doc => ({ id: doc.id, date: doc.day, number: 'BU ' + doc.id, sub_number: 0, annulled: !!doc.cancelled, total: null, subunit: doc.subunit });
  const openNodes = () => savedTree ? savedTree.expandedNodes : (treeHost._vcExpanded ||= new Set());
  function treeNodes() {
    const byId = new Map(state.docs.map(doc => [String(doc.id), doc]));
    const tip = node => {
      const doc = byId.get(String(node.id).slice(4));
      return doc ? { ...node, tooltip: `${node.label} • ${doc.subunit} / ${doc.gestiune} / ${doc.bp == null ? 'Doar NIR în magazie' : doc.receiver || ''}` } : node;
    };
    return bonOrder.nodes(state.docs.map(treeDoc)).map(node => ({ ...node, children: (node.children || []).map(tip) }));
  }
  async function changeTreeOrder() {
    const selected = state.docs.find(doc => doc.id === state.doc?.id);
    openNodes().clear();
    if (selected) openNodes().add(bonOrder.branch(treeDoc(selected)));
    treeDirty = true; paintSaved();
  }
  function paintSaved() {
    savedHost.inert = editing();
    savedHost.setAttribute('aria-disabled', String(editing()));
    if (savedTree?.treeContainer?.isConnected) {
      if (treeDirty) refreshTree(savedTree, treeNodes(), state.doc?.id); else markSelected(savedTree, state.doc?.id);
      paintTreeAdd(); treeDirty = false; return;
    }
    treeUi.clear(); savedTree = null; treeDirty = false;
    savedTree = paintTree(treeHost, treeNodes(), { title: 'Bonuri BU salvate', showSearchBox: false, collapsed: true, selected: state.doc?.id,
      onSelect: id => open(Number(id)).catch(report),
      onAction: (id, anchor) => {
        const doc = state.docs.find(item => String(item.id) === id);
        if (!doc) return;
        const run = async action => { if (state.doc?.id !== doc.id) await open(doc.id); if (state.doc?.id === doc.id) await action(); };
        openMenu(anchor, [{ icon: '◉', label: 'Previzualizează', run: () => run(previewDoc).catch(report) },
          { icon: '×', label: 'Anulează BU', rule: true, disabled: !!doc.cancelled || state.busy, run: () => run(cancelDoc).catch(report) }], { align: 'right' });
      } }, treeUi);
    paintTreeAdd();
  }
  function paintTreeAdd() {
    bonOrder.mount(savedHost, treeUi, changeTreeOrder, !editing(), report);
    const heading = treeHost.querySelector(':scope > h4');
    if (!heading) return;
    if (!treeAddButton || treeAddButton.element.parentElement !== heading) {
      treeAddButton?.destroy();
      treeAddButton = new ActionAddButton(heading, { className: 'vc-receipt-tree__add', title: 'Adaugă bon de transfer între subunități',
        onClick: () => add().catch(report) });
      treeUi.own(treeAddButton);
    }
    treeAddButton.setEnabled(!editing() && !state.pending);
  }

  // ---- bar and painting ------------------------------------------------------------------------------------------
  function paintBar() {
    bar.classList.toggle('vc-doc__bar--nir-editing', editing());
    if (state.pending) {
      statusBar(bar, { status, end: [button('Reia aceeași operație', retryPending, { 'data-retry-save': 'true' })] });
      return;
    }
    if (editing()) {
      statusBar(bar, { start: [button('Renunță', discard, { className: 'vc-doc__cancel' })], status,
        end: [button('Salvează bonul', save, { className: 'is-primary vc-doc__save', 'data-retry-save': 'true' })] });
      return;
    }
    statusBar(bar, { start: state.doc ? [button('Previzualizează', previewDoc)] : [], status,
      end: state.doc ? [button('Anulează BU', cancelDoc, { className: 'is-danger', disabled: !!state.doc.cancelled || state.busy })] : [] });
  }

  function paint() {
    if (abort.signal.aborted) return;
    root.classList.toggle('is-adding', editing());
    title.textContent = 'Bon transfer între subunități' + (editing() ? ' • document nou' : state.doc ? ` nr. BU ${state.doc.id}` : '');
    pill.textContent = editing() ? 'Ciornă' : state.doc ? state.doc.cancelled ? 'Anulat' : 'Salvat' : '';
    pill.hidden = !pill.textContent;
    pill.dataset.state = editing() ? 'draft' : state.doc?.cancelled ? 'annulled' : 'saved';
    paintHeader(); paintLines(); paintBar(); paintSaved();
    headerHost.inert = linesHost.inert = !!state.pending;
  }

  // ---- actions ----------------------------------------------------------------------------------------------------
  const unsaved = () => state.dirty || headerUi.dirty() || linesUi.dirty();
  async function allowDiscard() {
    if (state.pending) { say('Rezultatul operației nu este confirmat. Reluați aceeași operație înainte de a schimba documentul.', true); return false; }
    return !unsaved() || await confirmBox('Renunțați la modificările nesalvate?',
      { title: 'Modificări nesalvate', yes: 'Renunț', no: 'Rămân aici', warning: true });
  }

  async function refresh() {
    state.context = await read();
    if (abort.signal.aborted) return;
    state.docs = state.context.documents;
    state.zec = Number.isInteger(state.context.zec) ? state.context.zec : 4;
    treeDirty = true;
  }

  async function open(id) {
    if (state.busy || !await allowDiscard()) return;
    const sequence = ++openSequence, doc = state.docs.find(item => item.id === id);
    if (!doc) return;
    const details = await read('/' + id);
    if (abort.signal.aborted || sequence !== openSequence) return;
    Object.assign(state, { doc, details: details.lines, mode: 'view', draft: null, dirty: false });
    openNodes().add(bonOrder.branch(treeDoc(doc)));
    say(''); paint();
  }

  async function add() {
    if (state.busy || !await allowDiscard()) return;
    if (!destinations().length) { say('Nu există gestiuni eligibile în alte subunități.', true); return; }
    state.draft = { date: state.context.today, giver: WAREHOUSE, unit: '', gestiune: '', receiver: '', warehouse: '', use: '', explanation: '', lines: [] };
    if (units().length === 1) state.draft.unit = String(units()[0].id);
    syncDestination();
    Object.assign(state, { mode: 'add', dirty: false, eligible: [] });
    say(''); paint();
    await reloadEligible();
  }

  async function discard() {
    if (state.busy || !await allowDiscard()) return;
    Object.assign(state, { mode: 'view', draft: null, dirty: false, eligible: [] });
    say(''); paint();
  }

  // The body is built once and kept with its request key until the server answers, so a retry repeats the same request.
  async function submit(suffix, values) {
    if (state.busy) return null;
    state.pending ||= { suffix, body: { ...selection, ...values, request_key: newKey() } };
    state.busy = true; root.inert = true; say('Se salvează…');
    let result;
    try {
      result = await opts.api(path + state.pending.suffix, { method: 'POST', headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(state.pending.body), signal: abort.signal });
      state.pending = null;
    } catch (error) {
      if (error.status && error.status < 500) state.pending = null;
      if (state.pending) say('Răspunsul nu a fost primit. Reîncercați aceeași operație.', true);
      else report(error);
      return null;
    } finally {
      state.busy = false;
      if (!abort.signal.aborted) { root.inert = false; if (state.pending) paint(); else paintBar(); }
    }
    return result;
  }

  async function finished(result, cancelled) {
    if (!result || abort.signal.aborted) return;
    const id = cancelled ?? result.id;
    state.dirty = false; state.draft = null; state.mode = 'view';
    await refresh();
    const doc = state.docs.find(item => item.id === id);
    if (doc) {
      state.doc = doc; state.details = (await read('/' + id)).lines;
      openNodes().add(bonOrder.branch(treeDoc(doc)));
    }
    paint();
    if (cancelled) { say(`${result.number} a fost anulat.`); return; }
    const nir = `NIR ${doc ? nirNumber(doc) : ''}`.trim();
    say(`Salvat ${result.number}. ` + (result.bp ? `În destinație au fost create ${nir} și ${result.bp}.` : `În destinație a fost creat ${nir} în magazie.`));
  }

  function collect() {
    const draft = state.draft, missing = [], target = destination();
    const required = (key, label) => { const value = chosen(key); if (!value) missing.push(label); return value; };
    const giver = required('giver', 'Predător'), unit = required('unit', 'Unitate destinație');
    const gestiune = gestiuni().length > 1 ? required('gestiune', 'Gestiune destinație') : String(draft.gestiune || '');
    const date = inputs.get('Data')?.input.value;
    if (!date) missing.push('Data');
    const receiverField = inputs.get('receiver')?.input, receiver = receiverField?.value || '';
    if (receiverField?._combo?._pendingSelection && receiverField._combo.input.value.trim() !== '') missing.push('Primitor (alegeți din listă)');
    const warehouse = target?.warehouse_accounts.length > 1 ? required('warehouse', 'Cont de magazie la destinație') : draft.warehouse;
    const use = receiver && target?.use_accounts.length > 1 ? required('use', 'Cont de folosință la destinație') : receiver ? draft.use : '';
    if (!target) missing.push('Destinația');
    const lines = draft.lines.filter(line => !isBlank(line));
    if (!lines.length) missing.push('Cel puțin un produs');
    const bad = lines.filter(line => problems(line).length).map(line => draft.lines.indexOf(line) + 1);
    if (bad.length) missing.push(`Rândurile ${bad.join(', ')}`);
    return { missing, body: { date, giver: giver === WAREHOUSE || !giver ? null : Number(giver), explanation: draft.explanation || '',
      destination_subunit: Number(unit), destination_gestiune: Number(gestiune), receiver: receiver ? Number(receiver) : null,
      warehouse: warehouse ? Number(warehouse) : null, use: use ? Number(use) : null,
      lines: lines.map(line => ({ variant: line.variant, batch: line.batch, PUTVA: line.PUTVA, Cant: line.Cant })) } };
  }

  async function save() {
    if (state.busy || !editing()) return;
    if (!await headerUi.commit() || !await linesUi.commit()) { say('Salvarea este oprită. Finalizați sau corectați câmpul marcat.', true); return; }
    headerUi.validate(); linesUi.validate();
    const { missing, body } = collect();
    if (missing.length) { say(`Completați sau corectați: ${missing.join(', ')}.`, true); return; }
    await finished(await submit('', body));
  }

  async function retryPending() {
    const cancelled = state.pending?.suffix ? Number(state.pending.suffix.split('/')[1]) : null;
    await finished(await submit(state.pending.suffix), cancelled);
  }

  async function cancelDoc() {
    const doc = state.doc;
    if (!doc || doc.cancelled || state.busy || state.pending) return;
    const reason = await reasonBox(`Anulați BU ${doc.id}?`, { title: 'Anulare BU', yes: 'Anulează', no: 'Renunță' });
    if (reason == null) return;
    await finished(await submit(`/${doc.id}/cancel`, { reason }), doc.id);
  }

  async function previewDoc() {
    const doc = state.doc;
    if (!doc) return;
    const text = { key: 'value', title: 'Valoare', width: 580 };
    const header = [['Predător', doc.giver || 'Magazie'], ['Unitate destinație', doc.subunit], ['Gestiune destinație', doc.gestiune],
      ['Primitor', doc.bp == null ? 'Doar NIR în magazie' : doc.receiver], ['Data', dayLabel(doc.day)],
      ['NIR în destinație', nirNumber(doc)], ['BP în destinație', doc.bp_number], ['Explicații', doc.explanation]];
    await preview(`Bon de transfer între subunități • BU ${doc.id} • ${dayLabel(doc.day)}${doc.cancelled ? ' • Anulat' : ''}`, [
      { title: 'Antet', columns: [{ key: 'label', title: 'Câmp', width: 240 }, text],
        rows: header.filter(([, value]) => value).map(([label, value]) => ({ label, value: String(value) })) },
      { title: 'Produse transferate', columns: lineColumns().filter(item => !item.key.startsWith('_')), rows: savedRows() },
    ], dialogs, { date: doc.day });
  }

  const ready = (async () => {
    await refresh();
    if (abort.signal.aborted) return;
    paint();
  })().catch(report);
  paint();
  return {
    ready,
    isDirty: () => unsaved() || !!state.pending || state.busy,
    canChangeContext() {
      if (!state.busy && !state.pending) return true;
      say('Finalizați operația în curs înainte de a schimba gestiunea.', true);
      return false;
    },
    destroy() {
      abort.abort();
      for (const owner of [headerUi, linesUi, treeUi, dialogs]) owner.clear();
      host.replaceChildren();
    },
  };
}
