// VERCON v2: shared document controls. Cell edits update a draft; Save writes the entire document.
import { confirmBox, reasonBox } from '../utils/confirm-box.js';
import { format, parse } from './fields.js';
import { documentReport } from './document-report.js';
import { el, scope, field, column, grid, paintTree, groupedNodes, rowMenu, openMenu, showMessage, preview as openPreview } from './ui.js';

const families = {
  nir: { title: 'Recepție NIR', number: 'Număr NIR', lines: 'Produse recepționate', groups: ['party', 'date'] },
  bc: { title: 'Bon de consum', number: 'Număr BC', lines: 'Produse pe bon', groups: ['receiver', 'date'] },
  storno: { title: 'Factură storno', number: 'Număr FCS', lines: 'Produse stornate', groups: ['party', 'date'], source: 'Recepția stornată' },
  br: { title: 'Retur bon de consum', number: 'Număr retur', lines: 'Produse returnate', groups: ['receiver', 'date'], source: 'Bonul returnat' },
  bp: { title: 'Bon de predare', number: 'Număr BP', lines: 'Produse predate', groups: ['holder', 'date'] },
  bt: { title: 'Bon de transfer', number: 'Număr BT', lines: 'Produse transferate', groups: ['old', 'date'], holder: 'IDC_OLD' },
  bs: { title: 'Bon de casare', number: 'Număr BS', lines: 'Produse casate', groups: ['holder', 'date'], holder: 'IDC_BC' },
};
const key = () => crypto.randomUUID();
const number = (value) => value == null || value === '' ? null : Number(String(value).replace(',', '.'));
const fixed = (value, decimals) => value == null || !Number.isFinite(value) ? '' : value.toFixed(decimals);
const dateText = (value) => format(value, { type: 'date' });

export function mountDocuments(host, opts) {
  const family = opts.family, config = families[family], abort = new AbortController();
  const state = { docs: [], lookups: null, doc: null, draft: null, mode: 'view', dirty: false, requestKey: null, pendingBody: null, canWrite: false, closed: false, zec: 4, eligible: [], sources: [], available: {}, busy: false };
  const headerUi = scope(), linesUi = scope(), treeUi = scope(), dialogs = scope();
  const inputs = new Map(), errors = new Map();
  let linesGrid = null, availabilityTimer = null, numberTimer = null, numberSequence = 0, availabilitySequence = 0, openSequence = 0, eligibleSequence = 0;
  const status = el('span', { className: 'vc-save-status', role: 'status' });
  const say = (text, bad = false) => showMessage(status, text, bad ? 'error' : 'info');
  const report = (error) => { if (!abort.signal.aborted && error.name !== 'AbortError') say(error.message || 'Operația a eșuat.', true); };
  const query = () => new URLSearchParams(Object.entries(opts.sel()).filter(([, value]) => value != null && value !== ''));
  const url = (path, extra = '') => `/api/vercon/doc/${family}${path}?${query()}${extra}`;
  const json = (method, path, body) => opts.api(`/api/vercon/doc/${family}${path}`, { method, headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ ...opts.sel(), ...body }) });
  const mf = () => family === 'nir' && opts.sel().type === 'MF', editing = () => state.mode !== 'view';
  const accountKey = () => ({ nir: 'IdMasina', bc: 'IdMasina', bp: 'IdMasinaP' }[family]);
  const accountId = () => Number(inputs.get(accountKey())?.value ?? state.draft?.header[accountKey()] ?? state.doc?.header[accountKey()]) || null;
  const inAccount = (item) => !accountKey() || item?.category === accountId();
  const bon = () => ['bc', 'bp'].includes(family), variants = () => state.lookups?.variants || [];
  const variant = (id) => variants().find((item) => String(item.id) === String(id)), product = (id) => variants().find((item) => String(item.product) === String(id));
  const options = (items) => (items || []).map((item) => ({ value: item.id, label: item.label || item.name }));
  const newLine = (values = {}) => ({ key: key(), id: null, ...values });
  const title = el('h3'), pill = el('span', { className: 'vc-pill' });
  const headerHost = el('div', { className: 'vc-grid vc-doc__header' }), linesHost = el('div', { className: 'vc-doc__lines' }), bar = el('div', { className: 'vc-actions vc-doc__bar' });
  const main = el('div', { className: 'vc-doc__main' }, el('div', { className: 'vc-doc__titlebar' }, title, pill), headerHost, linesHost, bar);
  const savedHost = el('aside', { className: 'vc-doc__tree' });
  const root = el('section', { className: `vc-doc vc-doc--${family}` }, main, savedHost); host.replaceChildren(root);

  function headerSpecs() {
    const lookup = state.lookups || {}, specs = [], ref = (name, label, items, required = true) => ({ key: name, label, type: 'ref', required, showArrow: family === 'nir', options: options(items) });
    if (family === 'nir') specs.push(ref('IdPartener', 'Furnizor', lookup.partners), ref('IdMasina', 'Categorie / cont', lookup.categories), ref('IdComisie', 'Comisie', lookup.commissions, false), { key: 'NrFact', label: 'Număr factură', type: 'text', required: true }, { key: 'DataFc', label: 'Data facturii', type: 'date', required: true }, { key: 'Valoare', label: 'Valoare factură (lei)', type: 'decimal', min: 0, hint: 'Necompletat: suma produselor.' });
    if (family === 'storno') specs.push({ key: 'NrFact', label: 'Număr factură storno', type: 'text', required: true }, { key: 'DataFc', label: 'Data facturii', type: 'date', required: true }, ref('IdComisie', 'Comisie', lookup.commissions, false));
    if (family === 'bc') specs.push(ref('IDC', 'Primitor / gestionar', lookup.receivers), ref('IdMasina', 'Categorie / cont', lookup.categories));
    if (family === 'bp') specs.push(ref('IdMasinaP', 'Cont de intrare', lookup.categories), ref('IDC_BP', 'Primitor', lookup.holders), ref('IdMasina', 'Cont primitor', lookup.categories));
    if (family === 'bt') specs.push(ref('IDC_OLD', 'Predător', lookup.holders), ref('IDC_BT', 'Primitor', lookup.holders));
    if (family === 'bs') specs.push(ref('IDC_BC', 'Predător', lookup.holders));
    specs.push({ key: 'Numar', label: config.number, type: 'int', min: 1 }, { key: 'Data', label: family === 'nir' ? 'Data NIR' : 'Data', type: 'date', required: true });
    if (!['nir', 'storno'].includes(family)) specs.push({ key: 'Expl', label: 'Explicații', type: 'text', max: 255 }); return specs;
  }
  function lineSpecs() {
    if (config.source || config.holder) return [
      { key: config.holder && editing() ? '_choice' : 'name', label: 'Produs', type: 'ref', product: true, editable: !!config.holder }, { key: 'um', label: 'UM' },
      ...(editing() ? [{ key: 'qty', label: config.holder ? 'Disponibil la predător' : 'Cantitate sursă', type: 'decimal' }, ...(!config.holder ? [{ key: 'done', label: 'Deja', type: 'decimal' }, { key: 'left', label: 'Rămas', type: 'decimal' }] : [])] : []),
      { key: 'Cant', label: 'Cantitate', type: 'decimal', editable: true, min: 0 }, { key: 'PUTVA', label: 'Preț cu TVA (lei)', type: 'decimal' }, { key: 'ValoareCUTVA', label: 'Valoare (lei)', type: 'decimal' },
    ];
    const specs = [{ key: mf() ? 'product' : 'variant', label: 'Produs', type: 'ref', product: true, editable: true, required: true }, { key: 'um', label: 'UM' }, { key: 'Cant', label: 'Cantitate', type: 'decimal', editable: true, required: true, min: 0 }];
    if (family === 'nir') {
      specs.push({ key: 'Pret', label: 'Preț fără TVA (lei)', type: 'decimal', editable: true, required: true, min: 0 }, { key: 'IDTVA', label: 'TVA %', type: 'ref', editable: true, required: true }, { key: 'Valoare', label: 'Valoare fără TVA', type: 'decimal' }, { key: 'TVA', label: 'TVA (lei)', type: 'decimal' }, { key: 'ValoareCUTVA', label: 'Total cu TVA', type: 'decimal' });
      if (mf()) specs.push({ key: 'code', label: 'Nr. inventar', editable: true, required: true }, { key: 'data_intrarii', label: 'Data intrării', type: 'date', editable: true, required: true }, { key: 'DNF', label: 'Durată normală (luni)', type: 'int', editable: true, required: true, min: 1 }, { key: 'DF', label: 'Durată folosită (luni)', type: 'int', editable: true, min: 0 }, { key: 'ValoareLuna', label: 'Valoare lunară', type: 'decimal' });
    } else specs.push({ key: 'PUTVA', label: 'Preț cu TVA (lei)', type: 'decimal' }, { key: 'ValoareCUTVA', label: 'Valoare (lei)', type: 'decimal' }, { key: 'avail', label: 'Disponibil la data bonului', type: 'decimal' }); return specs;
  }
  const docLabel = (doc) => `${['nir', 'storno'].includes(family) ? doc.invoice || doc.number : `${family.toUpperCase()} ${doc.number}`} • ${format(doc.total, { type: 'decimal' })} lei${doc.annulled ? ' • anulat' : ''}`;
  const sourceLabel = (doc) => `${doc.invoice || doc.number} • ${dateText(doc.date)} • ${doc.party || doc.receiver || ''} • ${format(doc.total, { type: 'decimal' })} lei`;
  async function load() {
    const [list, lookups] = await Promise.all([opts.api(url('/list')), opts.api(url('/lookups'))]); if (abort.signal.aborted) return;
    Object.assign(state, { docs: list.documents, canWrite: list.can_write && !list.closed, closed: list.closed, zec: list.zec, lookups }); if (config.source) state.sources = (await opts.api(url('/sources'))).documents;
  }
  async function refresh() { const answer = await opts.api(url('/list')); if (!abort.signal.aborted) state.docs = answer.documents; }
  async function allowDiscard() { return !state.dirty && !headerUi.dirty() && !linesUi.dirty() || await confirmBox('Renunțați la modificările nesalvate?', { title: 'Modificări nesalvate', yes: 'Renunț', no: 'Rămân aici', warning: true }); }
  async function open(id) {
    if (state.busy || !await allowDiscard()) return; const sequence = ++openSequence, doc = await opts.api(url(`/${id}`)); if (sequence !== openSequence || abort.signal.aborted) return;
    Object.assign(state, { doc, draft: null, mode: 'view', dirty: false, requestKey: null, pendingBody: null }); say(''); paint();
  }
  function draftFromDoc(doc) { return { source: doc.header.source ?? null, tip: doc.doc_type || 'NIR', header: { ...doc.header, Numar: String(doc.number ?? '').replace(/\D/g, '') }, lines: doc.lines.map((line) => newLine({ ...line })) }; }
  async function add() { if (!await allowDiscard()) return; const today = state.lookups.today; Object.assign(state, { mode: 'add', dirty: false, requestKey: null, pendingBody: null, draft: { source: null, tip: 'NIR', vat: null, header: { Data: today, DataFc: today }, lines: config.source || config.holder ? [] : [newLine()] } }); say(''); paint(); if (family === 'nir') await checkNumber(true); }
  async function edit() { if (!state.doc || !state.canWrite || state.doc.annulled) return; state.mode = 'edit'; state.draft = draftFromDoc(state.doc); state.dirty = false; say(''); if (config.source || config.holder) await eligible(); paint(); }
  async function discard() { if (!await allowDiscard()) return; state.mode = 'view'; state.draft = null; state.dirty = false; state.requestKey = null; state.pendingBody = null; say(''); paint(); }
  async function eligible() {
    const draft = state.draft, sequence = ++eligibleSequence, source = config.holder ? Number(draft.header[config.holder]) : draft.source;
    if (!source) { state.eligible = []; draft.lines = []; return; }
    const answer = await opts.api(url('/eligible', `&${config.holder ? 'holder' : 'source'}=${source}${state.mode === 'edit' ? `&exclude=${state.doc.id}` : ''}`));
    if (abort.signal.aborted || draft !== state.draft || sequence !== eligibleSequence) return; state.eligible = answer.lines;
    if (!config.holder) draft.lines = answer.lines.map((item) => { const old = draft.lines.find((line) => line.variant === item.variant && (line.origin ?? line.batch) === item.origin); return newLine({ ...item, id: old?.id ?? null, Cant: old?.Cant ?? '' }); });
    else draft.lines.forEach((line) => { line._choice = answer.lines.findIndex((item) => item.variant === line.variant && item.origin === (line.origin ?? line.batch)); const item = answer.lines[line._choice]; if (item) { line.qty = item.qty; line.left = item.left; } });
  }
  function calculate(line) {
    if (!editing()) return line;
    const originalPrice = state.mode === 'edit' ? state.doc.lines.find((item) => String(item.variant) === String(line.variant))?.PUTVA : null;
    const qty = number(line.Cant), price = family === 'nir' ? number(line.Pret) : bon() ? number(originalPrice ?? variant(line.variant)?.putva) : number(line.PUTVA), vat = state.lookups.vat?.find((item) => String(item.id) === String(line.IDTVA));
    if (family === 'nir') {
      const net = qty != null && price != null ? qty * price : null, grossUnit = price != null && vat ? Number(fixed(price * (1 + Number(vat.percent) / 100), state.zec)) : null, total = qty != null && grossUnit != null ? Number(fixed(qty * grossUnit, state.zec)) : null;
      line.Valoare = fixed(net, state.zec); line.TVA = fixed(total != null && net != null ? total - net : null, state.zec); line.ValoareCUTVA = fixed(total, state.zec); if (mf()) line.ValoareLuna = fixed(number(line.DNF) > 0 && total != null ? total / number(line.DNF) : null, state.zec);
    } else { line.ValoareCUTVA = fixed(qty != null && price != null ? qty * price : null, state.zec); if (bon()) line.PUTVA = fixed(price, state.zec); } return line;
  }
  function paint() {
    if (abort.signal.aborted) return; inputs.clear(); errors.clear();
    title.textContent = `${config.title}${editing() ? state.mode === 'add' ? ' • document nou' : ' • modificare' : state.doc ? ` nr. ${state.doc.number}` : ''}`;
    pill.textContent = editing() ? 'Ciornă' : state.doc ? state.doc.annulled ? 'Anulat' : 'Salvat' : ''; pill.hidden = !pill.textContent; pill.dataset.state = editing() ? 'draft' : state.doc?.annulled ? 'annulled' : 'saved';
    paintHeader(); paintLines(); paintBar(); paintSaved(); freezePending();
  }
  function paintHeader() {
    headerUi.clear(); headerHost.replaceChildren(); if (!editing() && !state.doc) { headerHost.append(el('p', { textContent: 'Alegeți un document din arbore sau adăugați unul nou.' })); return; }
    const header = editing() ? state.draft.header : { ...state.doc.header, Numar: state.doc.number };
    headerHost.classList.toggle('vc-doc__header--nir', family === 'nir'); headerHost.classList.toggle('vc-doc__header--oi', ['bp', 'bt', 'bs'].includes(family));
    if (config.source) {
      const item = field({ key: 'source', label: config.source, type: 'ref', required: true, options: state.sources.map((doc) => ({ value: doc.id, label: sourceLabel(doc) })) }, editing() ? state.draft.source : header.source, editing() && state.mode === 'add', async (value) => { state.draft.source = Number(value) || null; state.dirty = true; try { await eligible(); paintLines(); } catch (error) { report(error); } }, headerUi); headerHost.append(item.node); errors.set('source', item.error);
    }
    const groups = family === 'nir' ? ['Furnizor și cont', 'Factură', 'NIR', 'Valoare și TVA'].map((label) => { const node = el('div', { className: 'vc-header-group', 'aria-label': label }); headerHost.append(node); return node; }) : null;
    for (const spec of headerSpecs()) {
      const item = field(spec, header[spec.key], editing(), async (value) => {
        const previous = state.draft.header[spec.key]; state.draft.header[spec.key] = value; state.dirty = true;
        if (spec.key === config.holder && String(previous) !== String(value)) {
          if (state.draft.lines.length && !await confirmBox('Schimbarea predătorului elimină produsele selectate din ciornă.', { yes: 'Schimbă predătorul', no: 'Păstrează' })) { state.draft.header[spec.key] = previous; paintHeader(); return; }
          state.draft.lines = []; try { await eligible(); paintLines(); } catch (error) { report(error); }
        }
        if (spec.key === accountKey()) { const incompatible = state.draft.lines.filter((line) => (line.variant || line.product) && !inAccount(mf() ? product(line.product) : variant(line.variant))); if (incompatible.length) { state.draft.lines = state.draft.lines.filter((line) => !incompatible.includes(line)); say('Produsele incompatibile cu noul cont au fost scoase din ciornă.', true); } paintLines(); }
        if (spec.key === 'Data') availability();
        if (family === 'nir' && spec.key === 'Numar') { clearTimeout(numberTimer); numberTimer = setTimeout(() => checkNumber().catch(report), 250); }
      }, headerUi);
      inputs.set(spec.key, item.input); errors.set(`h.${spec.key}`, item.error); if (spec.hint && editing()) item.node.append(el('span', { className: 'vc-hint', textContent: spec.hint }));
      const index = ['IdPartener', 'IdMasina', 'IdComisie'].includes(spec.key) ? 0 : ['NrFact', 'DataFc'].includes(spec.key) ? 1 : spec.key === 'Valoare' ? 3 : 2; (groups ? groups[index] : headerHost).append(item.node);
      if (family === 'nir' && editing() && ['NrFact', 'DataFc'].includes(spec.key)) {
        const copy = el('button', { type: 'button', className: 'vc-nir-copy', textContent: '→', title: spec.key === 'NrFact' ? 'Copiază numărul facturii în numărul NIR și verifică duplicatul' : 'Copiază data facturii în data NIR' });
        copy.addEventListener('click', async () => { if (!await headerUi.commit()) return; const target = spec.key === 'NrFact' ? 'Numar' : 'Data', raw = state.draft.header[spec.key]; if (target === 'Numar' && !/^\d+$/.test(String(raw || '')) || target === 'Data' && !raw) { say('Completați o valoare validă pentru copiere.', true); return; } state.draft.header[target] = raw; state.dirty = true; paintHeader(); if (target === 'Numar') checkNumber().catch(report); }, { signal: abort.signal }); item.node.append(copy);
      }
      if (family === 'nir' && spec.key === 'Numar' && editing()) item.node.append(el('span', { className: 'vc-hint vc-nir-number-status', textContent: 'Verificat din nou la salvare.' }));
      if (family === 'nir' && spec.key === 'IdPartener' && state.mode === 'add') {
        const stock = el('input', { type: 'checkbox', checked: state.draft.tip === 'STOC_INITIAL' }); stock.addEventListener('change', () => { state.draft.tip = stock.checked ? 'STOC_INITIAL' : 'NIR'; state.dirty = true; }, { signal: abort.signal }); item.node.append(el('label', { className: 'vc-nir-stock' }, 'Stoc inițial (fără furnizor)', stock));
      }
    }
    if (family === 'nir' && editing()) {
      const vat = field({ key: '_vat', label: 'Cotă TVA', type: 'ref', showArrow: true, options: (state.lookups.vat || []).map(item => ({ value: item.id, label: `${item.percent}%` })) }, state.draft.vat, true, async value => {
        if (!await linesUi.commit()) { paintHeader(); return; }
        state.draft.vat = Number(value) || null; state.draft.lines.forEach(line => { line.IDTVA = state.draft.vat; calculate(line); }); state.dirty = true; paintLines();
      }, headerUi); groups[3].append(vat.node);
    }
    if (state.doc?.annulled && !editing()) headerHost.append(el('p', { textContent: `Motiv anulare: ${state.doc.reason || ''}` }));
  }
  async function checkNumber(offer = false) {
    if (family !== 'nir' || !editing()) return;
    const draft = state.draft, raw = draft.header.Numar || '', seq = ++numberSequence;
    const answer = await opts.api(url('/number', `&number=${encodeURIComponent(raw)}${state.mode === 'edit' ? `&exclude=${state.doc.id}` : ''}`));
    if (abort.signal.aborted || draft !== state.draft || seq !== numberSequence || String(draft.header.Numar || '') !== String(raw)) return;
    if (offer && !raw) { draft.header.Numar = String(answer.suggested); inputs.get('Numar').value = draft.header.Numar; }
    const error = errors.get('h.Numar'); if (error) error.textContent = !answer.valid ? 'Introduceți un număr întreg pozitiv.' : answer.duplicate ? 'Numărul este deja folosit. Alegeți alt număr.' : '';
    const note = headerHost.querySelector('.vc-nir-number-status'); if (note) note.textContent = offer && !raw ? 'Propus automat. Verificat din nou la salvare.' : !raw ? `Următorul număr disponibil: ${answer.suggested}.` : answer.valid && !answer.duplicate ? 'Număr disponibil. Verificat din nou la salvare.' : `Număr disponibil propus: ${answer.suggested}.`;
  }
  function productOptions() { const items = variants().filter(inAccount), unique = mf() ? items.filter((item, index) => items.findIndex((other) => other.product === item.product) === index) : items; return unique.map((item) => ({ value: mf() ? item.product : item.id, label: `${item.name}${mf() ? '' : ` • ${format(item.putva, { type: 'decimal' })} lei`}` })); }
  function paintLines() {
    linesUi.clear(); linesGrid = null; linesHost.replaceChildren(); if (!editing() && !state.doc) return;
    const rows = (editing() ? state.draft.lines : state.doc.lines.map((line) => ({ ...line, key: `saved:${line.id}` }))).map(calculate); rows.forEach((row, index) => { row._index = index + 1; });
    const specs = lineSpecs(), columns = [{ key: '_index', title: '#', width: 42, filter: false, sortable: false }];
    for (const spec of specs) {
      const choices = () => spec.product ? config.holder ? state.eligible.map((item, index) => ({ value: index, label: `${item.name} • ${item.um} • disponibil ${format(item.left ?? item.qty, { type: 'decimal' })}` })) : productOptions() : (state.lookups.vat || []).map((item) => ({ value: item.id, label: `${item.percent}%` }));
      columns.push(column({ ...spec, type: spec.type || 'text' }, { width: spec.product || spec.key === 'name' ? 260 : spec.key === 'um' ? 64 : family === 'nir' && ['Pret', 'IDTVA', 'Valoare', 'TVA', 'ValoareCUTVA'].includes(spec.key) ? 100 : 128, showArrow: family === 'nir', editable: editing() && !!spec.editable, filter: !editing(), sortable: !editing(),
        ...(spec.type === 'ref' ? { editor: 'list', searchable: true, options: choices, formatter: (value) => spec.product && (!editing() || spec.key === 'name') ? rows.find((row) => row[spec.key] === value)?.name || String(value ?? '') : choices().find((item) => String(item.value) === String(value))?.label || '', validate: (value) => value == null || value === '' ? 'Alegeți o valoare din listă.' : null } : {}) }));
    }
    columns.push({ key: '_action', title: '⋮', width: 42, filter: false, sortable: false, display: 'button', actionText: '⋮', actionLabel: 'Opțiuni linie' });
    const body = el('div'), error = el('p', { className: 'vc-field-error', role: 'alert' }); errors.set('lines', error); linesHost.append(el('h4', { textContent: config.lines }), body, error);
    linesGrid = grid(body, { columns, rows, rowKey: 'key', proportionalWidths: true, editable: editing(), footer: true, footerCaption: '', layoutId: `vercon.${opts.sel().type}.${family}.lines.${family === 'nir' ? 'receipt-v2' : 'compact-v1'}`, footerAction: { label: '+ Adaugă rând', span: 2, disabled: !editing() || !!config.source }, onFooterAction: () => addLine().catch(report),
      onCellSave: ({ row, key: name, value }) => {
        const line = state.draft.lines.find((item) => item.key === row.key); if (!line) throw new Error('Linia nu mai este în document.');
        if (name === '_choice') { const item = state.eligible[Number(value)]; if (!item) throw new Error('Alegeți un produs al predătorului.'); if (state.draft.lines.some((other) => other !== line && other.variant === item.variant && (other.origin ?? other.batch) === item.origin)) throw new Error('Produsul din acest lot este deja în document.'); const id = line.variant === item.variant && (line.origin ?? line.batch) === item.origin ? line.id : null; Object.assign(line, { ...item, _choice: Number(value), id }); }
        else { line[name] = ['variant', 'product', 'IDTVA'].includes(name) ? Number(value) : value; if (['variant', 'product'].includes(name)) { const item = mf() ? product(value) : variant(value); line.name = item?.name || ''; line.um = item?.um || ''; } }
        state.dirty = true; return { ...calculate(line) };
      }, onCellSaved: () => { totals(); availability(); }, onEditError: ({ error }) => say(error.message, true),
      onCellAction: ({ row }) => rowMenu(linesGrid, row, [{ icon: '×', label: 'Scoate rândul', disabled: !editing() || !!config.source, run: async () => { if (!await linesUi.commit()) return; state.draft.lines = state.draft.lines.filter((item) => item.key !== row.key); state.dirty = true; paintLines(); } }]),
    }, linesUi); totals(); availability();
  }
  async function addLine(item) {
    if (!editing() || !await linesUi.commit()) return; if (accountKey() && !accountId()) { say('Alegeți mai întâi contul documentului.', true); return; } if (config.holder && !state.eligible.length) { say('Alegeți un predător cu produse disponibile.', true); return; }
    const line = newLine({ ...(family === 'nir' ? { IDTVA: state.draft.vat } : {}), ...(item ? { variant: item.id, name: item.name, um: item.um } : {}) }); state.draft.lines.push(line); state.dirty = true; paintLines(); linesGrid.beginNewRowEdit(line, config.holder ? '_choice' : mf() ? 'product' : 'variant', () => { state.draft.lines = state.draft.lines.filter((other) => other.key !== line.key); paintLines(); });
  }
  function totals() {
    if (!linesGrid) return;
    const rows = editing() ? state.draft.lines : state.doc.lines;
    const sum = (name) => fixed(rows.reduce((total, row) => total + (number(row[name]) || 0), 0), state.zec);
    const lineTotal = sum('ValoareCUTVA');
    const values = { PUTVA: 'TOTAL', ValoareCUTVA: format(lineTotal, { type: 'decimal' }) };
    if (family === 'nir') { values.Valoare = format(sum('Valoare'), { type: 'decimal' }); values.TVA = format(sum('TVA'), { type: 'decimal' }); }
    linesGrid.setFooter({ values });
    let note = linesHost.querySelector('.vc-doc__reconciliation');
    if (!note) { note = el('p', { className: 'vc-hint vc-doc__reconciliation' }); linesHost.append(note); }
    const declared = number(state.doc?.header.Valoare), difference = declared == null ? 0 : Number(fixed(declared - number(lineTotal), state.zec));
    note.hidden = editing() || !difference;
    note.textContent = note.hidden ? '' : `Valoare în antet: ${format(state.doc.header.Valoare, { type: 'decimal' })} lei. Diferență față de liniile documentului: ${format(fixed(difference, state.zec), { type: 'decimal' })} lei.`;
  }
  function availability() {
    clearTimeout(availabilityTimer); const sequence = ++availabilitySequence; if (!bon() || !editing()) return; const draft = state.draft; state.available = {};
    availabilityTimer = setTimeout(async () => {
      const date = parse(inputs.get('Data')?.value, { type: 'date', required: true }), ids = [...new Set([...draft.lines.map((line) => line.variant)].filter(Boolean))]; if (!date.ok || !ids.length) return;
      try { const exclude = state.mode === 'edit' && (family === 'bc' || state.doc.header.IdBonConsum) ? `&exclude_kind=bc&exclude_id=${family === 'bc' ? state.doc.id : state.doc.header.IdBonConsum}` : ''; const result = await opts.api(`/api/vercon/stock/available?${query()}&date=${date.value}&products=${ids.join(',')}${exclude}`); if (abort.signal.aborted || sequence !== availabilitySequence || state.draft !== draft) return; state.available = result.available; draft.lines.forEach((line) => { line.avail = result.available[String(line.variant)] ?? ''; }); if (linesGrid && !linesGrid.hasEdit) linesGrid.setRows(draft.lines, { keepWidths: true }); } catch (error) { report(error); }
    }, 300);
  }
  function paintSaved() {
    treeUi.clear(); savedHost.hidden = editing(); if (savedHost.hidden) return;
    paintTree(savedHost, groupedNodes(state.docs, config.groups.map((name) => (doc) => name === 'date' ? dateText(doc.date) : doc[name]), docLabel), { title: family === 'bc' ? 'Bonuri salvate' : 'Documente salvate', showSearchBox: false, collapsed: true, selected: state.doc?.id, onSelect: (id) => open(Number(id)).catch(report), onAction: (id, anchor) => {
      const doc = state.docs.find((item) => String(item.id) === id); if (!doc) return; const run = async (action) => { if (state.doc?.id !== doc.id) await open(doc.id); if (state.doc?.id === doc.id) await action(); };
      openMenu(anchor, [{ icon: '✎', label: 'Modifică', disabled: !state.canWrite || doc.annulled, run: () => run(edit).catch(report) }, { icon: '◉', label: 'Previzualizează', run: () => run(preview).catch(report) }, { icon: '◷', label: 'Istoric', run: () => run(history).catch(report) }, { icon: '×', label: 'Anulează', rule: true, disabled: !state.canWrite || doc.annulled, run: () => run(cancel).catch(report) }], { align: 'right' });
    } }, treeUi);
  }
  function paintBar() {
    bar.replaceChildren(); const button = (text, action, attrs = {}) => { const node = el('button', { type: 'button', textContent: text, ...attrs }); node.addEventListener('click', () => Promise.resolve(action()).catch(report), { signal: abort.signal }); return node; };
    if (editing()) bar.append(button('Renunță', discard), button('Previzualizează', preview), button(`Salvează ${family === 'nir' ? 'NIR' : 'bonul'}`, save, { className: 'is-primary' })); else { bar.append(button('+ Adaugă', add, { disabled: !state.canWrite })); if (state.doc) bar.append(button('Modifică', edit, { disabled: !state.canWrite || state.doc.annulled }), button('Previzualizează', preview)); } bar.append(status); if (state.closed) bar.append(el('span', { className: 'vc-hint', textContent: 'Perioadă închisă: numai consultare.' }));
  }
  function collect() {
    const out = { header: {}, lines: [], source: state.draft.source }, invalid = {};
    for (const spec of headerSpecs()) { const raw = inputs.get(spec.key)?.value ?? state.draft.header[spec.key]; if (spec.type === 'ref') { out.header[spec.key] = raw === '' || raw == null ? null : Number(raw); if (spec.required && out.header[spec.key] == null && !(family === 'nir' && state.draft.tip === 'STOC_INITIAL' && spec.key === 'IdPartener')) invalid[`h.${spec.key}`] = 'Alegeți o valoare.'; } else { const result = parse(raw, spec); if (!result.ok) invalid[`h.${spec.key}`] = result.message; else out.header[spec.key] = result.value; } }
    if (family === 'nir') out.header.TipDoc = state.draft.tip;
    for (const line of state.draft.lines) {
      if (config.source || config.holder) { if (line.Cant == null || line.Cant === '') continue; const result = parse(String(line.Cant), { type: 'decimal', required: true, min: 0 }); if (!result.ok || !line.variant) invalid[`l|${line.key}|Cant`] = result.message || 'Alegeți un produs.'; else out.lines.push({ key: line.key, id: line.id, variant: line.variant, batch: line.origin ?? line.batch ?? null, Cant: result.value }); continue; }
      const result = { key: line.key, id: line.id }; for (const spec of lineSpecs().filter((item) => item.editable)) { if (spec.type === 'ref') { result[spec.key] = line[spec.key] ?? null; if (result[spec.key] == null) invalid[`l|${line.key}|${spec.key}`] = 'Alegeți o valoare.'; } else { const parsed = parse(String(line[spec.key] ?? ''), { ...spec, type: spec.type || 'text' }); if (!parsed.ok) invalid[`l|${line.key}|${spec.key}`] = parsed.message; else result[spec.key] = parsed.value; } } out.lines.push(result);
    }
    if (!out.lines.length) invalid.lines = 'Adăugați cel puțin un produs și cantitatea.'; if (config.source && !out.source) invalid.source = 'Alegeți documentul sursă.'; return { body: out, errors: invalid };
  }
  function showErrors(invalid) { for (const node of errors.values()) node.textContent = ''; for (const [name, text] of Object.entries(invalid || {})) if (errors.has(name)) errors.get(name).textContent = text; const messages = Object.entries(invalid || {}).filter(([name]) => name.startsWith('l|')).map(([name, text]) => `Rând ${(state.draft?.lines.findIndex((item) => name.includes(item.key)) ?? -1) + 1}: ${text}`); if (messages.length) errors.get('lines').textContent = messages.join(' '); }
  function freezePending() { headerHost.inert = linesHost.inert = !!state.pendingBody; }
  async function save() {
    if (!state.canWrite || !editing() || state.busy || !await headerUi.commit() || !await linesUi.commit()) return; const data = collect(); if (Object.keys(data.errors).length) { showErrors(data.errors); say('Corectați câmpurile marcate.', true); return; } state.busy = true; root.inert = true; say('Se salvează…');
    try { let result; if (state.mode === 'add') { state.requestKey ||= key(); state.pendingBody ||= structuredClone(data.body); result = await json('POST', '', { ...state.pendingBody, request_key: state.requestKey }); } else result = await json('PUT', `/${state.doc.id}`, { ...data.body, version: state.doc.version }); if (abort.signal.aborted) return; Object.assign(state, { doc: result.document, mode: 'view', draft: null, dirty: false, requestKey: null, pendingBody: null }); paint(); say(result.replayed ? 'Salvat (cererea fusese deja aplicată).' : 'Salvat.'); try { await refresh(); if (!abort.signal.aborted) paintSaved(); } catch (error) { report(error); } }
    catch (error) { if (error.fields) showErrors(error.fields); if (error.status && error.status < 500) { state.requestKey = null; state.pendingBody = null; } report(error); if (!error.status || error.status >= 500) say('Răspunsul salvării nu este confirmat. Reîncercați salvarea aceleiași ciorne; cererea păstrează aceeași cheie.', true); }
    finally { state.busy = false; root.inert = false; freezePending(); }
  }
  async function cancel() { const doc = state.doc; if (!doc || !state.canWrite || doc.annulled) return; const reason = await reasonBox(`Anulați ${config.title.toLocaleLowerCase('ro')} nr. ${doc.number}?`, { title: 'Anulare document', yes: 'Anulează', no: 'Renunță' }); if (reason == null) return; try { const result = await json('POST', `/${doc.id}/cancel`, { reason, version: doc.version }); if (abort.signal.aborted) return; state.doc = result.document; await refresh(); paint(); say('Anulat.'); } catch (error) { report(error); } }
  async function history() { const doc = state.doc; if (!doc) return; const result = await opts.api(url(`/${doc.id}/history`)); if (!abort.signal.aborted) openPreview(`Istoric • ${config.title} nr. ${doc.number}`, [{ title: 'Operații', columns: [{ key: 'at', title: 'Data', width: 190 }, { key: 'user', title: 'Utilizator', width: 180 }, { key: 'operation', title: 'Operație', width: 160 }, { key: 'after', title: 'Valori', width: 420, formatter: (value) => value && typeof value === 'object' ? JSON.stringify(value) : value ?? '' }], rows: result.history }], dialogs); }
  async function preview() {
    if (editing() && (!await headerUi.commit() || !await linesUi.commit())) return; const data = editing() ? collect() : null; if (data && Object.keys(data.errors).length) { showErrors(data.errors); say('Completați câmpurile pentru previzualizare.', true); return; } const header = editing() ? data.body.header : state.doc ? { ...state.doc.header, Numar: state.doc.number } : null; if (!header) return;
    const token = openSequence, doc = editing() ? null : state.doc, draft = state.draft;
    const sections = [
      { title: 'Antet', columns: [{ key: 'label', title: 'Câmp', width: 240 }, { key: 'value', title: 'Valoare', width: 580 }], rows: headerSpecs().map((spec) => ({ label: spec.label, value: spec.type === 'ref' ? spec.options.find((item) => String(item.value) === String(header[spec.key]))?.label || '' : format(header[spec.key], spec) })) },
      { title: config.lines, columns: lineSpecs().map((spec) => ({ key: spec.product ? 'name' : spec.key, title: spec.label, width: spec.product ? 250 : 120, formatter: (value) => spec.key === 'IDTVA' ? `${state.lookups.vat.find(item => String(item.id) === String(value))?.percent ?? ''}%` : format(value, { type: spec.type === 'ref' ? 'text' : spec.type || 'text' }) })), rows: editing() ? state.draft.lines : state.doc.lines },
    ];
    await documentReport({ api: opts.api, selection: opts.sel(), family, document: doc,
      title: `${config.title}${editing() ? ' • Ciornă nesalvată' : ` nr. ${state.doc.number}`}`, sections, owner: dialogs,
      isCurrent: () => !abort.signal.aborted && token === openSequence && draft === state.draft && (!doc || doc === state.doc) });
  }
  const ready = load().then(() => { if (!abort.signal.aborted) paint(); }).catch(report); paint();
  return { ready, isDirty: () => state.dirty || headerUi.dirty() || linesUi.dirty(), search: (text) => { const input = savedHost.querySelector('.treeview-search-input'); if (input) { input.value = text; input.dispatchEvent(new Event('input')); } }, destroy: () => { abort.abort(); numberSequence++; openSequence++; availabilitySequence++; eligibleSequence++; clearTimeout(numberTimer); clearTimeout(availabilityTimer); [headerUi, linesUi, treeUi, dialogs].forEach((owner) => owner.clear()); host.replaceChildren(); } };
}
