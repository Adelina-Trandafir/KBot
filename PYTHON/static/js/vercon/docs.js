// slice VC06-02..05: the document screens -- NIR (receipt, with the fixed-asset columns on MF), storno (FCS), consumption
// bon (BC) and return (BR). One generic screen driven by a description per family.
//
// States (FLUXURI_UI section 1): Consultare (the saved document, read only, the tree of saved documents on the right) /
// Adăugare and Modificare (the draft; the tree is hidden; Renunță abandons the draft) / Salvare. The page never decides
// a value: the server computes the lines (VAT, rounding), numbers the document, checks the stock and the period, and
// answers with the saved document. What the page shows while typing (line values, availability) is a convenience.
//
// Save protocol: one RequestKey per logical save, kept until the server answers; a lost answer is retried with the SAME
// key (the server replays it, it never creates the document twice). A conflict, a closed period or a refusal keeps the
// draft on screen. Cancelling asks for a reason; a cancelled document stays visible, marked.
import { confirmBox } from '../utils/confirm-box.js';
import { format, parse } from './fields.js';

const el = (k_tag, k_attrs = {}, ...k_children) => {
  const k_node = Object.assign(document.createElement(k_tag), k_attrs);
  k_node.append(...k_children);
  return k_node;
};
const newKey = () => globalThis.crypto?.randomUUID?.() || `${Date.now()}-${Math.random().toString(16).slice(2)}`;
let k_rowCounter = 0;
const rowKey = () => `r${++k_rowCounter}`;
const num = (k_text) => { const k_n = Number(String(k_text ?? '').replace(/\s/g, '').replace(',', '.')); return Number.isFinite(k_n) ? k_n : null; };
const fixed = (k_n, k_zec) => (k_n == null ? '' : k_n.toFixed(k_zec).replace('.', ','));

const FAMILIES = {
  nir: { title: 'Recepție NIR', noun: 'NIR', right: 'receptie', group: ['party', 'date'], leaf: (k_d) => `${k_d.invoice || ''} • ${k_d.total ?? ''} lei`, numberLabel: 'Număr NIR' },
  storno: { title: 'Factură stornare (FCS)', noun: 'factură de stornare', right: 'storno', group: ['party', 'date'], leaf: (k_d) => `${k_d.invoice || ''} • ${k_d.total ?? ''} lei`, numberLabel: 'Număr FCS', source: 'Recepția stornată' },
  bc: { title: 'Bon de consum', noun: 'bon', right: 'consum', group: ['receiver', 'date'], leaf: (k_d) => `BC ${k_d.number ?? ''} • ${k_d.total ?? ''} lei`, numberLabel: 'Număr BC' },
  bp: { title: 'Bon de predare', noun: 'bon de predare', right: 'oi_predare', group: ['holder', 'date'], leaf: (k_d) => `BP ${k_d.number ?? ''} • ${k_d.total ?? ''} lei`, numberLabel: 'Număr BP' },
  bt: { title: 'Bon de transfer', noun: 'bon de transfer', right: 'oi_transfer', group: ['holder', 'date'], holderSource: 'IDC_OLD', leaf: (k_d) => `BT ${k_d.number ?? ''} • ${k_d.old || ''} → ${k_d.holder || ''} • ${k_d.total ?? ''} lei`, numberLabel: 'Număr BT' },
  bs: { title: 'Bon de casare', noun: 'bon de casare', right: 'oi_casare', group: ['holder', 'date'], holderSource: 'IDC_BC', leaf: (k_d) => `BS ${k_d.number ?? ''} • ${k_d.total ?? ''} lei`, numberLabel: 'Număr BS' },
  br: { title: 'Retur bon de consum', noun: 'retur', right: 'retur', group: ['receiver', 'date'], leaf: (k_d) => `${k_d.number ?? ''} • ${k_d.total ?? ''} lei`, numberLabel: 'Număr retur', source: 'Bonul returnat' },
};
const ERRORS_BANNER = { PERIOD_CLOSED: 'Perioada este închisă.', STOCK_NEGATIVE: 'Stoc negativ interzis.' };

const dateText = (k_iso) => (k_iso ? format(k_iso, { type: 'date' }) : '');

// opts: { api, sel, family, signal }
export function mountDocuments(k_host, k_opts) {
  const k_cfg = FAMILIES[k_opts.family];
  const k_abort = new AbortController();
  const k_signal = { signal: k_abort.signal };
  const k_state = { docs: [], lookups: null, doc: null, mode: 'view', draft: null, dirty: false, key: null, canWrite: false, closed: false, zec: 4, scale: 4, sources: [], eligible: [], loaded: false };
  const k_inputs = new Map();   // 'h.Field' | 'l|rowKey|Field' -> input
  const k_msgs = new Map();     // same keys -> error element
  const k_say = el('span', { className: 'vc-save-status', role: 'status' });
  const say = (k_text, k_bad) => { k_say.textContent = k_text || ''; k_say.style.color = k_bad ? '#c0392b' : ''; };
  const query = () => new URLSearchParams(Object.entries(k_opts.sel()).filter(([, k_v]) => k_v != null && k_v !== '').map(([k_k, k_v]) => [k_k, String(k_v)]));
  const url = (k_path, k_extra = '') => `/api/vercon/doc/${k_opts.family}${k_path}?${query()}${k_extra}`;
  const json = (k_method, k_path, k_body) => k_opts.api(`/api/vercon/doc/${k_opts.family}${k_path}`, { method: k_method, headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ ...k_opts.sel(), ...k_body }) });
  const mf = () => k_opts.sel().type === 'MF' && k_opts.family === 'nir';
  const picks = () => !!(k_cfg.source || k_cfg.holderSource);   // storno / return / transfer / write-off pick from what the source still holds
  const isBon = () => k_opts.family === 'bc' || k_opts.family === 'bp';

  const k_title = el('h3');
  const k_pill = el('span', { className: 'vc-pill' });
  const k_headerHost = el('div', { className: 'vc-grid vc-doc__header' });
  const k_linesHost = el('div', { className: 'vc-doc__lines' });
  const k_bar = el('div', { className: 'vc-actions vc-doc__bar' });
  const k_main = el('div', { className: 'vc-doc__main' }, el('div', { className: 'vc-doc__titlebar' }, k_title, k_pill), k_headerHost, k_linesHost, k_bar);
  const k_tree = el('aside', { className: 'vc-doc__tree', 'aria-label': 'Documente salvate' });
  const k_root = el('section', { className: 'vc-doc' }, k_main, k_tree);
  k_host.replaceChildren(k_root);

  // ---- data ------------------------------------------------------------------------------------------------------
  async function loadAll() {
    const [k_list, k_look] = await Promise.all([k_opts.api(url('/list')), k_opts.api(url('/lookups'))]);
    k_state.docs = k_list.documents; k_state.canWrite = k_list.can_write && !k_list.closed; k_state.closed = k_list.closed;
    k_state.zec = k_list.zec; k_state.scale = k_list.scale; k_state.lookups = k_look; k_state.loaded = true;
    if (k_cfg.source) k_state.sources = (await k_opts.api(url('/sources'))).documents;
  }
  async function refreshList() { k_state.docs = (await k_opts.api(url('/list'))).documents; }

  async function openDoc(k_id) {
    if (k_state.dirty && !(await confirmBox('Aveți modificări nesalvate. Dacă deschideți alt document, ele se pierd.', { title: 'Modificări nesalvate', yes: 'Renunț la modificări', no: 'Rămân aici', warning: true }))) return;
    k_state.doc = await k_opts.api(url(`/${k_id}`));
    k_state.mode = 'view'; k_state.draft = null; k_state.dirty = false; k_state.key = null; say('');
    paint();
  }

  // ---- tree (consultation only) ----------------------------------------------------------------------------------
  function paintTree() {
    k_tree.hidden = k_state.mode !== 'view';
    if (k_tree.hidden) return;
    const k_groups = new Map();
    for (const k_d of k_state.docs) {
      const [k_a, k_b] = k_cfg.group;
      const k_name = (k_a === 'date' ? dateText(k_d.date) : k_d[k_a]) || '—';
      const k_sub = k_b === 'date' ? dateText(k_d.date) : (k_d[k_b] || '—');
      if (!k_groups.has(k_name)) k_groups.set(k_name, new Map());
      const k_inner = k_groups.get(k_name);
      if (!k_inner.has(k_sub)) k_inner.set(k_sub, []);
      k_inner.get(k_sub).push(k_d);
    }
    const k_nodes = [el('h4', { textContent: 'Documente salvate' })];
    if (!k_state.docs.length) k_nodes.push(el('p', { className: 'vc-nom__empty', textContent: 'Nu există documente.' }));
    for (const [k_name, k_inner] of [...k_groups].sort((k_x, k_y) => k_x[0].localeCompare(k_y[0], 'ro'))) {
      const k_open = [...k_inner.values()].flat().some((k_d) => k_d.id === k_state.doc?.id);
      const k_g = el('details', { open: k_open || k_groups.size === 1 }, el('summary', { textContent: k_name }));
      for (const [k_sub, k_docs] of k_inner) {
        const k_d2 = el('details', { open: k_open || k_docs.some((k_d) => k_d.id === k_state.doc?.id) }, el('summary', { textContent: k_sub }));
        for (const k_d of k_docs) {
          const k_leaf = el('button', { type: 'button', className: `vc-nom__leaf${k_d.id === k_state.doc?.id ? ' is-selected' : ''}${k_d.annulled ? ' is-inactive' : ''}`, textContent: `${k_cfg.leaf(k_d)}${k_d.annulled ? ' • anulat' : ''}` });
          k_leaf.addEventListener('click', () => openDoc(k_d.id).catch(report), k_signal);
          k_d2.append(k_leaf);
        }
        k_g.append(k_d2);
      }
      k_nodes.push(k_g);
    }
    k_tree.replaceChildren(...k_nodes);
  }

  function report(k_error) {
    if (k_error?.name === 'AbortError') return;
    console.error('[vercon] document call failed', k_error);
    say(k_error.message || 'Operația nu a putut fi trimisă.', true);
  }

  // ---- field definitions -----------------------------------------------------------------------------------------
  const options = (k_list, k_label = (k_x) => k_x.label) => k_list.map((k_x) => ({ value: String(k_x.id), label: k_label(k_x) }));
  const L = () => k_state.lookups;
  function headerFields() {
    const k_f = [];
    if (k_opts.family === 'nir') {
      k_f.push({ key: 'IdPartener', label: 'Furnizor', type: 'select', req: true, opts: () => options(L().partners, (k_p) => `${k_p.name}${k_p.cf ? ` (${k_p.cf})` : ''}`) });
      k_f.push({ key: 'IdMasina', label: 'Categorie / cont', type: 'select', opts: () => options(L().categories) });
      k_f.push({ key: 'IdComisie', label: 'Comisie', type: 'select', opts: () => options(L().commissions) });
      k_f.push({ key: 'NrFact', label: 'Factură', type: 'text', req: true });
      k_f.push({ key: 'DataFc', label: 'Data facturii', type: 'date', req: true });
      k_f.push({ key: 'Valoare', label: 'Valoare factură (lei)', type: 'decimal', hint: 'Dacă este necompletată, se folosește suma produselor.' });
      k_f.push({ key: 'Numar', label: k_cfg.numberLabel, type: 'int', hint: 'Gol = număr automat.' });
      k_f.push({ key: 'Data', label: 'Data NIR', type: 'date', req: true });
    } else if (k_opts.family === 'storno') {
      k_f.push({ key: 'NrFact', label: 'Factură storno', type: 'text', req: true });
      k_f.push({ key: 'DataFc', label: 'Data facturii storno', type: 'date', req: true });
      k_f.push({ key: 'IdComisie', label: 'Comisie', type: 'select', opts: () => options(L().commissions) });
      k_f.push({ key: 'Numar', label: k_cfg.numberLabel, type: 'int', hint: 'Gol = număr automat.' });
      k_f.push({ key: 'Data', label: 'Data NIR storno', type: 'date', req: true });
    } else if (k_opts.family === 'bc') {
      k_f.push({ key: 'IDC', label: 'Primitor / gestionar', type: 'select', req: true, opts: () => options(L().receivers) });
      k_f.push({ key: 'IdMasina', label: 'Categorie / cont', type: 'select', req: true, opts: () => options(L().categories) });
      k_f.push({ key: 'Numar', label: k_cfg.numberLabel, type: 'int', hint: 'Gol = număr automat.' });
      k_f.push({ key: 'Data', label: 'Data', type: 'date', req: true });
      k_f.push({ key: 'Expl', label: 'Explicații', type: 'text' });
    } else if (k_opts.family === 'bp') {
      k_f.push({ key: 'IDC_BP', label: 'Deținător (primitor)', type: 'select', req: true, opts: () => options(L().holders) });
      k_f.push({ key: 'IdMasina', label: 'Cont de intrare', type: 'select', req: true, opts: () => options(L().categories) });
      k_f.push({ key: 'IdMasinaP', label: 'Cont primitor', type: 'select', req: true, opts: () => options(L().categories) });
      k_f.push({ key: 'Numar', label: k_cfg.numberLabel, type: 'int', hint: 'Gol = număr automat.' });
      k_f.push({ key: 'Data', label: 'Data', type: 'date', req: true });
      k_f.push({ key: 'Expl', label: 'Explicații', type: 'text' });
    } else if (k_opts.family === 'bt') {
      k_f.push({ key: 'IDC_OLD', label: 'Predător', type: 'select', req: true, opts: () => options(L().holders) });
      k_f.push({ key: 'IDC_BT', label: 'Primitor', type: 'select', req: true, opts: () => options(L().holders) });
      k_f.push({ key: 'Numar', label: k_cfg.numberLabel, type: 'int', hint: 'Gol = număr automat.' });
      k_f.push({ key: 'Data', label: 'Data', type: 'date', req: true });
      k_f.push({ key: 'Expl', label: 'Explicații', type: 'text' });
    } else if (k_opts.family === 'bs') {
      k_f.push({ key: 'IDC_BC', label: 'Deținător', type: 'select', req: true, opts: () => options(L().holders) });
      k_f.push({ key: 'Numar', label: k_cfg.numberLabel, type: 'int', hint: 'Gol = număr automat.' });
      k_f.push({ key: 'Data', label: 'Data', type: 'date', req: true });
      k_f.push({ key: 'Expl', label: 'Explicații', type: 'text' });
    } else {
      k_f.push({ key: 'Numar', label: k_cfg.numberLabel, type: 'int', hint: 'Gol = număr automat.' });
      k_f.push({ key: 'Data', label: 'Data returului', type: 'date', req: true });
      k_f.push({ key: 'Expl', label: 'Explicații', type: 'text' });
    }
    return k_f;
  }

  // ---- the draft -------------------------------------------------------------------------------------------------
  function blankDraft() {
    const k_today = L().today;
    const k_header = { Data: k_today };
    if (k_opts.family === 'nir') k_header.DataFc = k_today;
    if (k_opts.family === 'storno') k_header.DataFc = k_today;
    return { source: null, header: k_header, lines: picks() ? [] : [newLine()], tip: 'NIR' };
  }
  function newLine(k_from = {}) { return { key: rowKey(), id: null, ...k_from }; }
  function draftFromDoc(k_doc) {
    const k_h = k_doc.header;
    const k_header = { ...k_h, Data: k_h.Data, Numar: k_doc.number != null && k_doc.family !== 'bc' && k_doc.family !== 'br' ? String(k_doc.number) : '' };
    if (k_doc.family === 'bc') k_header.Numar = String(k_doc.number ?? '').replace(/\D/g, '');
    if (k_doc.family === 'br') k_header.Numar = String(k_doc.number ?? '').replace(/\D/g, '');
    return { source: k_h.source ?? null, header: k_header, tip: k_doc.doc_type || 'NIR', lines: k_doc.lines.map((k_l) => newLine({ ...k_l, id: k_l.id })) };
  }

  async function startAdd() {
    if (k_state.dirty && !(await confirmBox('Aveți modificări nesalvate. Dacă începeți un document nou, ele se pierd.', { title: 'Modificări nesalvate', yes: 'Renunț la modificări', no: 'Rămân aici', warning: true }))) return;
    k_state.mode = 'add'; k_state.draft = blankDraft(); k_state.doc = null; k_state.dirty = false; k_state.key = null; say('');
    paint();
  }
  async function startEdit() {
    k_state.mode = 'edit'; k_state.draft = draftFromDoc(k_state.doc); k_state.dirty = false; k_state.key = null; say('');
    if (picks()) await loadEligible();
    paint();
  }
  async function cancelEdit() {
    if (k_state.dirty && !(await confirmBox('Renunțați la modificările nesalvate?', { title: 'Renunțare', yes: 'Renunț', no: 'Rămân aici', warning: true }))) return;
    k_state.mode = 'view'; k_state.draft = null; k_state.dirty = false; k_state.key = null; say('');
    if (!k_state.doc && k_state.docs.length) k_state.doc = null;
    paint();
  }

  async function loadEligible() {
    const k_src = k_state.draft.source;
    const k_exclude = k_state.mode === 'edit' ? `&exclude=${k_state.doc.id}` : '';
    if (k_cfg.holderSource) {
      const k_holder = k_inputs.get(`h.${k_cfg.holderSource}`)?.value || k_state.draft.header[k_cfg.holderSource] || '';
      k_state.eligible = k_holder ? (await k_opts.api(url('/eligible', `&holder=${k_holder}${k_exclude}`))).lines : [];
    } else {
      k_state.eligible = k_src ? (await k_opts.api(url('/eligible', `&source=${k_src}${k_exclude}`))).lines : [];
    }
    // one draft line per eligible product; the quantity the operator types is what is stornoed / returned
    const k_id = (k_l) => `${k_l.origin ?? k_l.batch ?? ''}:${k_l.variant}`;
    const k_old = new Map(k_state.draft.lines.map((k_l) => [k_id(k_l), k_l]));
    k_state.draft.lines = k_state.eligible.map((k_e) => {
      const k_prev = k_old.get(k_id({ origin: k_e.origin, variant: k_e.variant }));
      return newLine({ variant: k_e.variant, origin: k_e.origin, name: k_e.name, um: k_e.um, Cant: k_prev?.Cant ?? '', PUTVA: k_e.PUTVA, left: k_e.left, qty: k_e.qty, done: k_e.done, id: k_prev?.id ?? null });
    });
  }

  // ---- painting --------------------------------------------------------------------------------------------------
  function paint() {
    k_inputs.clear(); k_msgs.clear();
    const k_editing = k_state.mode !== 'view';
    const k_doc = k_state.doc;
    k_title.textContent = k_doc && !k_editing ? `${k_cfg.title} • nr. ${k_doc.family === 'storno' ? `-${k_doc.number}` : k_doc.number}` : (k_state.mode === 'add' ? `${k_cfg.title} • document nou` : (k_state.mode === 'edit' ? `${k_cfg.title} • modificare` : k_cfg.title));
    k_pill.textContent = k_editing ? 'Ciornă' : (k_doc ? (k_doc.annulled ? 'Anulat' : 'Salvat') : '');
    k_pill.dataset.state = k_editing ? 'draft' : (k_doc?.annulled ? 'annulled' : 'saved');
    k_pill.hidden = !k_pill.textContent;
    paintHeader(); paintLines(); paintBar(); paintTree();
    k_main.classList.toggle('is-wide', k_editing);
  }

  function valueOfHeader(k_key) {
    if (k_state.mode === 'view') {
      const k_v = k_state.doc?.header?.[k_key];
      if (k_key === 'Numar') return k_state.doc ? String(k_state.doc.number ?? '') : '';
      return k_v == null ? '' : k_v;
    }
    return k_state.draft.header[k_key] ?? '';
  }

  function paintHeader() {
    k_headerHost.replaceChildren();
    if (k_state.mode === 'view' && !k_state.doc) {
      k_headerHost.append(el('p', { className: 'vc-nom__empty', textContent: k_state.docs.length ? 'Alegeți un document din arbore sau adăugați unul nou.' : 'Nu există documente. Adăugați primul.' }));
      return;
    }
    const k_editing = k_state.mode !== 'view';
    if (k_cfg.source && k_editing && k_state.mode === 'add') {
      const k_sel = el('select', { 'aria-label': k_cfg.source });
      k_sel.append(el('option', { value: '', textContent: '— alegeți —' }));
      for (const k_d of k_state.sources) k_sel.append(el('option', { value: String(k_d.id), textContent: sourceLabel(k_d) }));
      k_sel.value = k_state.draft.source != null ? String(k_state.draft.source) : '';
      k_sel.addEventListener('change', async () => {
        k_state.draft.source = k_sel.value ? Number(k_sel.value) : null; k_state.dirty = true;
        try { await loadEligible(); paintLines(); paintBar(); } catch (k_e) { report(k_e); }
      }, k_signal);
      const k_msg = el('div', { className: 'vc-field-error' }); k_msgs.set('source', k_msg); k_inputs.set('source', k_sel);
      k_headerHost.append(el('label', {}, `${k_cfg.source} *`, k_sel, k_msg));
    } else if (k_cfg.source && k_state.doc) {
      const k_src = k_state.sources.find((k_d) => k_d.id === (k_state.doc.header.source));
      k_headerHost.append(el('label', {}, k_cfg.source, el('input', { type: 'text', readOnly: true, value: k_src ? sourceLabel(k_src) : `#${k_state.doc.header.source ?? ''}` })));
    }
    if (k_opts.family === 'nir' && k_editing && k_state.mode === 'add') {
      const k_stock = el('input', { type: 'checkbox', checked: k_state.draft.tip === 'STOC_INITIAL' });
      k_stock.addEventListener('change', () => { k_state.draft.tip = k_stock.checked ? 'STOC_INITIAL' : 'NIR'; k_state.dirty = true; }, k_signal);
      k_headerHost.append(el('label', { className: 'vc-nom__check' }, k_stock, ' Stoc inițial (fără furnizor)'));
    }
    for (const k_f of headerFields()) {
      const k_value = valueOfHeader(k_f.key);
      let k_input;
      if (k_f.type === 'select') {
        k_input = el('select', { disabled: !k_editing });
        k_input.append(el('option', { value: '', textContent: '— alegeți —' }));
        for (const k_o of k_f.opts()) k_input.append(el('option', { value: k_o.value, textContent: k_o.label }));
        k_input.value = k_value === '' || k_value == null ? '' : String(k_value);
        if (k_state.mode === 'view' && k_f.key === 'IdPartener' && !k_input.value) k_input.append(el('option', { value: '', textContent: k_state.doc?.header?.party || '—', selected: true }));
      } else {
        const k_shown = k_f.type === 'date' ? dateText(k_value) : (k_f.type === 'decimal' && k_value !== '' ? format(k_value, { type: 'decimal' }) : k_value);
        k_input = el('input', { type: 'text', value: k_shown ?? '', readOnly: !k_editing, autocomplete: 'off', name: k_f.key });
        if (k_f.type === 'date') k_input.placeholder = 'zz.ll.aaaa';
      }
      k_input.setAttribute('aria-label', k_f.label);
      k_input.addEventListener('input', () => { k_state.dirty = true; }, k_signal);
      k_input.addEventListener('change', async () => {
        if (k_f.key === 'Data' || k_f.key === 'DataFc') refreshAvailability();
        if (k_f.key === k_cfg.holderSource) { try { await loadEligible(); paintLines(); } catch (k_e) { report(k_e); } }
        recalcTotals();
      }, k_signal);
      const k_msg = el('div', { className: 'vc-field-error' }); k_msgs.set(`h.${k_f.key}`, k_msg); k_inputs.set(`h.${k_f.key}`, k_input);
      k_headerHost.append(el('label', {}, `${k_f.label}${k_f.req && k_editing ? ' *' : ''}`, k_input, k_editing && k_f.hint ? el('span', { className: 'vc-hint', textContent: k_f.hint }) : '', k_msg));
    }
    if (k_state.doc?.annulled && k_state.mode === 'view') {
      k_headerHost.append(el('label', {}, 'Motiv anulare', el('input', { type: 'text', readOnly: true, value: `${k_state.doc.reason || ''} (${k_state.doc.annulled_by || ''})` })));
    }
  }

  const sourceLabel = (k_d) => (k_opts.family === 'storno' ? `${k_d.party} • ${k_d.invoice || ''} • ${dateText(k_d.date)} • ${k_d.total} lei` : `${k_d.number} • ${dateText(k_d.date)} • ${k_d.receiver} • ${k_d.total} lei`);

  // line columns --------------------------------------------------------------------------------------------------------
  function lineColumns() {
    const k_c = [];
    if (picks() && k_state.mode === 'view') {   // a saved storno / return / transfer / write-off
      k_c.push({ key: 'name', label: 'Produs', kind: 'text' }, { key: 'um', label: 'UM', kind: 'text' }, { key: 'Cant', label: 'Cantitate', kind: 'input', num: true },
        { key: 'PUTVA', label: 'Preț cu TVA', kind: 'text', num: true }, { key: 'ValoareCUTVA', label: 'Valoare', kind: 'text', num: true });
      return k_c;
    }
    if (k_cfg.holderSource) {   // transfer / write-off being edited: what the holder holds, per batch
      k_c.push({ key: 'name', label: 'Produs', kind: 'text' }, { key: 'um', label: 'UM', kind: 'text' }, { key: 'qty', label: 'Deținut', kind: 'text', num: true },
        { key: 'Cant', label: k_opts.family === 'bt' ? 'De transferat' : 'De casat', kind: 'input', num: true }, { key: 'PUTVA', label: 'Preț cu TVA', kind: 'text', num: true },
        { key: 'ValoareCUTVA', label: 'Valoare', kind: 'calc', num: true });
      return k_c;
    }
    if (k_cfg.source) {   // storno / return being edited: the products of the source, with what is left
      k_c.push({ key: 'name', label: 'Produs', kind: 'text' }, { key: 'um', label: 'UM', kind: 'text' }, { key: 'qty', label: 'Cantitate sursă', kind: 'text', num: true },
        { key: 'done', label: 'Deja', kind: 'text', num: true }, { key: 'left', label: 'Rămas', kind: 'text', num: true },
        { key: 'Cant', label: k_opts.family === 'storno' ? 'De stornat' : 'De returnat', kind: 'input', num: true }, { key: 'PUTVA', label: 'Preț cu TVA', kind: 'text', num: true },
        { key: 'ValoareCUTVA', label: 'Valoare', kind: 'calc', num: true });
      return k_c;
    }
    k_c.push({ key: mf() ? 'product' : 'variant', label: 'Produs', kind: 'combo' }, { key: 'um', label: 'UM', kind: 'calc' });
    if (k_opts.family === 'nir') {
      k_c.push({ key: 'Cant', label: 'Cant.', kind: 'input', num: true }, { key: 'Pret', label: 'Preț fără TVA', kind: 'input', num: true },
        { key: 'IDTVA', label: 'TVA %', kind: 'vat' }, { key: 'Valoare', label: 'Valoare fără TVA', kind: 'calc', num: true }, { key: 'TVA', label: 'TVA', kind: 'calc', num: true },
        { key: 'ValoareCUTVA', label: 'Total cu TVA', kind: 'calc', num: true });
      if (mf()) k_c.push({ key: 'code', label: 'Nr. inventar', kind: 'input' }, { key: 'data_intrarii', label: 'Data intrării', kind: 'input', date: true },
        { key: 'DNF', label: 'Durată normală (luni)', kind: 'input', num: true }, { key: 'DF', label: 'Durată folosită (luni)', kind: 'input', num: true }, { key: 'ValoareLuna', label: 'Valoare lunară', kind: 'calc', num: true });
    } else {   // bc
      k_c.push({ key: 'Cant', label: 'Cantitate', kind: 'input', num: true }, { key: 'PUTVA', label: 'Preț cu TVA (lei)', kind: 'calc', num: true },
        { key: 'ValoareCUTVA', label: 'Valoare (lei)', kind: 'calc', num: true }, { key: 'avail', label: 'Disponibil', kind: 'calc', num: true });
    }
    return k_c;
  }

  const variantById = (k_id) => L().variants.find((k_v) => k_v.id === Number(k_id));
  const productById = (k_id) => L().variants.find((k_v) => k_v.product === Number(k_id));
  const variantLabel = (k_v) => `${k_v.name}${k_v.um ? ` (${k_v.um})` : ''}${k_v.code ? ` [${k_v.code}]` : ''} — ${format(k_v.putva, { type: 'decimal' })}`;
  const productLabel = (k_v) => k_v.name;

  function paintLines() {
    k_linesHost.replaceChildren();
    const k_editing = k_state.mode !== 'view';
    const k_rows = k_editing ? k_state.draft.lines : (k_state.doc?.lines || []).map((k_l) => ({ key: `v${k_l.id}`, ...k_l }));
    if (!k_editing && !k_state.doc) return;
    if (picks() && k_editing && (k_cfg.holderSource ? !(k_inputs.get(`h.${k_cfg.holderSource}`)?.value || k_state.draft.header[k_cfg.holderSource]) : !k_state.draft.source)) {
      const k_what = { storno: 'recepția', br: 'bonul', bt: 'predătorul', bs: 'deținătorul' }[k_opts.family];
      k_linesHost.append(el('p', { className: 'vc-nom__empty', textContent: `Alegeți ${k_what} pentru a vedea produsele.` })); return;
    }
    const k_cols = lineColumns();
    const k_table = el('table', { className: 'vc-lines vc-lines--doc' });
    k_table.append(el('thead', {}, el('tr', {}, el('th', { textContent: '#' }), ...k_cols.map((k_c) => el('th', { textContent: k_c.label })), ...(k_editing && !picks() ? [el('th')] : []))));
    const k_body = el('tbody');
    k_rows.forEach((k_row, k_index) => k_body.append(lineRow(k_row, k_index, k_cols, k_editing)));
    const k_foot = el('tfoot');
    k_table.append(k_body, k_foot);
    k_linesHost.append(k_table);
    k_state.footer = k_foot; k_state.cols = k_cols;
    const k_add = el('button', { type: 'button', textContent: '+ Adaugă rând', disabled: !k_editing || picks() });
    k_add.addEventListener('click', () => { k_state.draft.lines.push(newLine()); k_state.dirty = true; paintLines(); recalcTotals(); k_body.lastElementChild?.querySelector('input,select')?.focus(); }, k_signal);
    k_linesHost.append(el('div', { className: 'vc-doc__linesfoot' }, picks() ? '' : k_add, el('span', { className: 'vc-doc__total', 'data-total': '' })));
    recalcTotals();
    if (isBon()) refreshAvailability();
  }

  function lineRow(k_row, k_index, k_cols, k_editing) {
    const k_tr = el('tr', {}); k_tr.dataset.key = k_row.key;
    k_tr.append(el('td', { textContent: String(k_index + 1) }));
    for (const k_col of k_cols) {
      const k_td = el('td', { className: k_col.num ? 'is-num' : '' });
      const k_errKey = `l|${k_row.key}|${k_col.key}`;
      const k_msg = el('div', { className: 'vc-field-error' }); k_msgs.set(k_errKey, k_msg);
      if (k_col.kind === 'text') {
        k_td.textContent = k_col.num && k_row[k_col.key] != null ? format(k_row[k_col.key], { type: 'decimal' }) : (k_row[k_col.key] ?? '');
      } else if (k_col.kind === 'calc') {
        k_td.dataset.calc = k_col.key; k_td.textContent = k_state.mode === 'view' && k_row[k_col.key] != null ? format(k_row[k_col.key], { type: 'decimal' }) : '';
      } else if (k_col.kind === 'combo') {
        k_td.append(comboCell(k_row, k_col, k_editing, k_errKey), k_msg);
      } else if (k_col.kind === 'vat') {
        const k_s = el('select', { disabled: !k_editing });
        k_s.append(el('option', { value: '', textContent: '—' }));
        for (const k_v of L().vat) k_s.append(el('option', { value: String(k_v.id), textContent: `${k_v.percent}%` }));
        k_s.value = k_row.IDTVA != null ? String(k_row.IDTVA) : '';
        k_s.addEventListener('change', () => { k_row.IDTVA = k_s.value ? Number(k_s.value) : null; k_state.dirty = true; recalcRow(k_tr, k_row); }, k_signal);
        k_inputs.set(k_errKey, k_s); k_td.append(k_s, k_msg);
      } else {
        const k_shown = k_col.date ? dateText(k_row[k_col.key]) : (k_col.num && k_row[k_col.key] != null && k_row[k_col.key] !== '' ? format(k_row[k_col.key], { type: 'decimal' }) : (k_row[k_col.key] ?? ''));
        const k_i = el('input', { type: 'text', value: k_shown, readOnly: !k_editing, autocomplete: 'off' });
        if (k_col.num) k_i.inputMode = 'decimal';
        k_i.setAttribute('aria-label', k_col.label);
        k_i.addEventListener('input', () => { k_row[k_col.key] = k_i.value; k_state.dirty = true; recalcRow(k_tr, k_row); }, k_signal);
        k_i.addEventListener('change', () => { if (k_col.key === 'Cant' && isBon()) refreshAvailability(); }, k_signal);
        k_inputs.set(k_errKey, k_i); k_td.append(k_i, k_msg);
      }
      k_tr.append(k_td);
    }
    if (k_editing && !picks()) {
      const k_del = el('button', { type: 'button', textContent: '✕', title: 'Șterge linia' });
      k_del.setAttribute('aria-label', 'Șterge linia');
      k_del.addEventListener('click', () => { k_state.draft.lines = k_state.draft.lines.filter((k_l) => k_l.key !== k_row.key); if (!k_state.draft.lines.length) k_state.draft.lines.push(newLine()); k_state.dirty = true; paintLines(); }, k_signal);
      k_tr.append(el('td', {}, k_del));
    }
    recalcRow(k_tr, k_row, true);
    return k_tr;
  }

  function comboCell(k_row, k_col, k_editing, k_errKey) {
    const k_pickFrom = mf() ? L().variants.filter((k_v, k_i, k_all) => k_all.findIndex((k_x) => k_x.product === k_v.product) === k_i) : L().variants;
    const k_labelOf = mf() ? productLabel : variantLabel;
    const k_listId = `vc-list-${k_opts.family}-${k_row.key}`;
    const k_labels = new Map();
    const k_list = el('datalist', { id: k_listId });
    for (const k_v of k_pickFrom) {
      let k_label = k_labelOf(k_v);
      if (k_labels.has(k_label)) k_label = `${k_label} #${mf() ? k_v.product : k_v.id}`;
      k_labels.set(k_label, k_v); k_list.append(el('option', { value: k_label }));
    }
    const k_current = k_row[k_col.key] != null ? (mf() ? productById(k_row[k_col.key]) : variantById(k_row[k_col.key])) : null;
    const k_text = k_state.mode === 'view' ? (k_row.name ? `${k_row.name}${k_row.um ? '' : ''}` : '') : (k_current ? [...k_labels].find(([, k_v]) => k_v === k_current)?.[0] || '' : '');
    const k_i = el('input', { type: 'text', value: k_text, readOnly: !k_editing, autocomplete: 'off' });
    k_i.setAttribute('list', k_listId); k_i.setAttribute('aria-label', 'Produs'); k_i.placeholder = 'Caută produs…';
    k_i.addEventListener('change', () => {
      const k_picked = k_labels.get(k_i.value);
      k_row[k_col.key] = k_picked ? (mf() ? k_picked.product : k_picked.id) : null;
      k_row.um = k_picked?.um || '';
      k_row._putva = k_picked?.putva ?? null;
      k_state.dirty = true;
      const k_tr = k_i.closest('tr'); if (k_tr) recalcRow(k_tr, k_row);
      if (isBon()) refreshAvailability();
    }, k_signal);
    k_inputs.set(k_errKey, k_i);
    return el('div', {}, k_i, k_list);
  }

  // indicative arithmetic only (the server recomputes with the exact rules)
  function recalcRow(k_tr, k_row, k_initial = false) {
    if (k_state.mode === 'view') return;
    const k_zec = k_state.zec;
    const k_set = (k_key, k_text) => { const k_c = k_tr.querySelector(`[data-calc="${k_key}"]`); if (k_c) k_c.textContent = k_text; };
    const k_qty = num(k_row.Cant);
    if (k_opts.family === 'nir') {
      const k_pret = num(k_row.Pret);
      const k_rate = L().vat.find((k_v) => k_v.id === Number(k_row.IDTVA));
      const k_cota = k_rate ? Number(k_rate.percent) / 100 : null;
      const k_val = k_qty != null && k_pret != null ? k_qty * k_pret : null;
      const k_putva = k_pret != null && k_cota != null ? Number((k_pret * (1 + k_cota)).toFixed(k_zec)) : null;
      const k_tot = k_qty != null && k_putva != null ? Number((k_qty * k_putva).toFixed(k_zec)) : null;
      k_row._calc = { Valoare: k_val, TVA: k_tot != null && k_val != null ? k_tot - k_val : null, ValoareCUTVA: k_tot };
      k_set('um', k_row.um || ''); k_set('Valoare', fixed(k_val, k_zec)); k_set('TVA', fixed(k_row._calc.TVA, k_zec)); k_set('ValoareCUTVA', fixed(k_tot, k_zec));
      if (mf()) k_set('ValoareLuna', num(k_row.DNF) > 0 && k_tot != null ? fixed(k_tot / num(k_row.DNF), k_zec) : '');
    } else if (isBon()) {
      const k_v = variantById(k_row.variant);
      const k_putva = k_row.PUTVA != null && k_row.id ? num(k_row.PUTVA) : (k_v ? num(k_v.putva) : null);
      const k_tot = k_qty != null && k_putva != null ? Number((k_qty * k_putva).toFixed(k_zec)) : null;
      k_row._calc = { ValoareCUTVA: k_tot };
      k_set('um', k_v?.um || k_row.um || ''); k_set('PUTVA', fixed(k_putva, k_zec)); k_set('ValoareCUTVA', fixed(k_tot, k_zec));
    } else {
      const k_putva = num(k_row.PUTVA);
      const k_tot = k_qty != null && k_putva != null ? Number((k_qty * k_putva).toFixed(k_zec)) : null;
      k_row._calc = { ValoareCUTVA: k_tot };
      k_set('ValoareCUTVA', fixed(k_tot, k_zec));
    }
    if (!k_initial) recalcTotals();
  }

  function recalcTotals() {
    const k_target = k_linesHost.querySelector('[data-total]');
    if (!k_target) return;
    if (k_state.mode === 'view') { k_target.textContent = k_state.doc ? `TOTAL ${format(k_state.doc.header.Valoare, { type: 'decimal' })} lei` : ''; return; }
    const k_sum = k_state.draft.lines.reduce((k_a, k_l) => k_a + (k_l._calc?.ValoareCUTVA || 0), 0);
    k_target.textContent = `Total linii ${fixed(k_sum, k_state.zec)} lei`;
  }

  // availability of the bon's products on its date (informative; the server decides on save)
  let k_availTimer = null;
  function refreshAvailability() {
    if (!isBon() || k_state.mode === 'view') return;
    clearTimeout(k_availTimer);
    k_availTimer = setTimeout(async () => {
      const k_date = parse(k_inputs.get('h.Data')?.value, { type: 'date', required: true });
      const k_ids = [...new Set(k_state.draft.lines.map((k_l) => k_l.variant).filter(Boolean))];
      if (!k_date.ok || !k_ids.length) return;
      try {
        const k_r = await k_opts.api(`/api/vercon/stock/available?${query()}&date=${k_date.value}&products=${k_ids.join(',')}${k_state.mode === 'edit' && (k_opts.family === 'bc' || k_state.doc.header.IdBonConsum) ? `&exclude_kind=bc&exclude_id=${k_opts.family === 'bc' ? k_state.doc.id : k_state.doc.header.IdBonConsum}` : ''}`);
        const k_used = new Map();
        for (const k_l of k_state.draft.lines) if (k_l.variant) k_used.set(k_l.variant, (k_used.get(k_l.variant) || 0) + (num(k_l.Cant) || 0));
        for (const k_l of k_state.draft.lines) {
          const k_cell = k_linesHost.querySelector(`tr[data-key="${k_l.key}"] [data-calc="avail"]`);
          if (!k_cell || !k_l.variant) continue;
          const k_av = num(k_r.available[String(k_l.variant)] ?? '0');
          const k_after = k_av - (k_used.get(k_l.variant) || 0);
          k_cell.textContent = `${fixed(k_av, k_state.zec)}`;
          k_cell.style.color = k_after < 0 ? '#c0392b' : '';
          k_cell.title = k_after < 0 ? 'Cantitatea cerută depășește disponibilul la data bonului.' : `După bon: ${fixed(k_after, k_state.zec)}`;
        }
      } catch (k_e) { if (k_e.name !== 'AbortError') console.error('[vercon] availability', k_e); }
    }, 300);
  }

  // ---- commands --------------------------------------------------------------------------------------------------
  function paintBar() {
    k_bar.replaceChildren();
    const k_editing = k_state.mode !== 'view';
    const btn = (k_text, k_fn, k_attrs = {}) => { const k_b = el('button', { type: 'button', textContent: k_text, ...k_attrs }); k_b.addEventListener('click', () => Promise.resolve(k_fn()).catch(report), k_signal); return k_b; };
    if (k_editing) {
      k_bar.append(btn('Renunță', cancelEdit), btn('Previzualizează', preview), (() => { const k_s = el('button', { type: 'button', textContent: 'Salvează', className: 'is-primary' }); k_s.addEventListener('click', () => save().catch(report), k_signal); return k_s; })(), k_say);
      return;
    }
    const k_doc = k_state.doc;
    const k_can = k_state.canWrite;
    k_bar.append(btn('+ Adaugă', startAdd, { hidden: !k_can }));
    if (k_doc) {
      k_bar.append(btn('Modifică', startEdit, { hidden: !k_can || k_doc.annulled }), btn('Anulează', cancelDocument, { hidden: !k_can || k_doc.annulled, className: 'is-danger' }), btn('Previzualizează', preview), btn('Istoric', showHistory));
    }
    k_bar.append(k_say);
    if (k_state.closed) k_bar.append(el('span', { className: 'vc-hint', textContent: 'Perioada este închisă: documentele se pot doar consulta.' }));
  }

  function collect() {
    const k_errors = {};
    const k_d = k_state.draft;
    const k_header = {};
    for (const k_f of headerFields()) {
      const k_input = k_inputs.get(`h.${k_f.key}`);
      const k_raw = k_input ? k_input.value : '';
      if (k_f.type === 'select') { k_header[k_f.key] = k_raw === '' ? null : Number(k_raw); continue; }
      if (k_f.type === 'text') { k_header[k_f.key] = k_raw.trim() === '' ? null : k_raw.trim(); continue; }
      const k_r = parse(k_raw, { type: k_f.type, required: !!k_f.req, min: k_f.type === 'decimal' ? 0 : (k_f.key === 'Numar' ? 1 : undefined), label: k_f.label });
      if (!k_r.ok) k_errors[`h.${k_f.key}`] = k_r.message; else k_header[k_f.key] = k_r.value;
    }
    if (k_opts.family === 'nir' && k_d.tip) k_header.TipDoc = k_d.tip;
    const k_lines = [];
    for (const k_row of k_d.lines) {
      if (picks()) {
        if (String(k_row.Cant ?? '').trim() === '') continue;      // untouched products are not part of the storno / return
        const k_r = parse(k_row.Cant, { type: 'decimal', required: true, min: 0 });
        if (!k_r.ok) { k_errors[`l|${k_row.key}|Cant`] = k_r.message; continue; }
        k_lines.push({ key: k_row.key, id: k_row.id, variant: k_row.variant, batch: k_row.origin ?? k_row.batch ?? null, Cant: k_r.value });
        continue;
      }
      const k_line = { key: k_row.key, id: k_row.id };
      for (const k_col of lineColumns()) {
        if (k_col.kind === 'calc' || k_col.kind === 'text') continue;
        const k_v = k_row[k_col.key];
        if (k_col.kind === 'combo') { k_line[k_col.key] = k_v ?? null; if (k_v == null) k_errors[`l|${k_row.key}|${k_col.key}`] = 'Alegeți produsul.'; continue; }
        if (k_col.kind === 'vat') { k_line.IDTVA = k_v ?? null; if (k_v == null) k_errors[`l|${k_row.key}|IDTVA`] = 'Alegeți cota TVA.'; continue; }
        const k_type = k_col.date ? 'date' : (k_col.num ? (k_col.key === 'DNF' || k_col.key === 'DF' ? 'int' : 'decimal') : 'text');
        const k_r = parse(k_v ?? '', { type: k_type, required: !(k_col.key === 'DF'), min: k_type === 'text' ? undefined : 0 });
        if (!k_r.ok) k_errors[`l|${k_row.key}|${k_col.key}`] = k_r.message; else k_line[k_col.key === 'code' ? 'code' : k_col.key] = k_r.value;
      }
      if (mf() && k_row.product != null) k_line.product = k_row.product;
      k_lines.push(k_line);
    }
    if (!k_lines.length) k_errors.lines = picks() ? 'Introduceți cantitatea de la cel puțin un produs.' : 'Adăugați cel puțin o linie.';
    if (k_cfg.source && k_state.mode === 'add' && !k_d.source) k_errors.source = 'Alegeți documentul sursă.';
    return { errors: k_errors, body: { header: k_header, lines: k_lines, source: k_d.source } };
  }

  function showErrors(k_errors) {
    for (const k_m of k_msgs.values()) k_m.textContent = '';
    for (const [k_key, k_text] of Object.entries(k_errors || {})) {
      const k_m = k_msgs.get(k_key);
      if (k_m) k_m.textContent = k_text;
      else if (k_key === 'lines' || k_key === 'source') say(k_text, true);
    }
  }

  async function save() {
    const k_c = collect();
    if (Object.keys(k_c.errors).length) { showErrors(k_c.errors); say(k_c.errors.lines || 'Corectați câmpurile marcate.', true); return; }
    say('Se salvează…');
    const k_buttons = k_bar.querySelectorAll('button'); k_buttons.forEach((k_b) => { k_b.disabled = true; });
    try {
      let k_r;
      if (k_state.mode === 'add') {
        k_state.key = k_state.key || newKey();   // kept until the server answers: a retry replays, it never duplicates
        k_r = await json('POST', '', { ...k_c.body, request_key: k_state.key });
      } else {
        k_r = await json('PUT', `/${k_state.doc.id}`, { ...k_c.body, version: k_state.doc.version });
      }
      k_state.key = null; k_state.dirty = false; k_state.mode = 'view'; k_state.draft = null; k_state.doc = k_r.document;
      await refreshList(); say(k_r.replayed ? 'Salvat (cererea fusese deja aplicată).' : 'Salvat.');
      paint();
    } catch (k_error) {
      k_buttons.forEach((k_b) => { k_b.disabled = false; });
      await failed(k_error);
    }
  }

  async function failed(k_error) {
    if (k_error.name === 'AbortError') return;
    if (k_error.fields && (k_error.reason === 'INVALID' || k_error.reason === 'STOCK_NEGATIVE')) {
      k_state.key = null;                                  // refused: nothing was applied, the next try is a new request
      showErrors(k_error.fields); say(k_error.message, true); return;
    }
    if (k_error.reason === 'CONFLICT' || k_error.reason === 'NOT_FOUND' || k_error.reason === 'CANCELLED') {
      k_state.key = null; say(`${k_error.message} Datele introduse au rămas în formular.`, true); return;
    }
    if (k_error.status && k_error.status < 500) { k_state.key = null; say(k_error.message, true); return; }
    console.error('[vercon] document save failed', k_error);
    say(`${k_error.message || 'Salvarea nu a putut fi trimisă.'} Dacă ați apăsat Salvează, apăsați din nou: aceeași cerere nu se aplică de două ori.`, true);
  }

  async function cancelDocument() {
    const k_doc = k_state.doc; if (!k_doc) return;
    const k_reason = await askReason(`Anulați ${k_cfg.noun} nr. ${k_doc.number}? Documentul rămâne în listă, marcat ca anulat.`);
    if (k_reason == null) return;
    try {
      const k_r = await json('POST', `/${k_doc.id}/cancel`, { reason: k_reason, version: k_doc.version });
      k_state.doc = k_r.document; await refreshList(); say('Anulat.'); paint();
    } catch (k_error) { await failed(k_error); }
  }

  function askReason(k_text) {
    return new Promise((k_resolve) => {
      const k_dialog = el('dialog', { className: 'confirm-box confirm-box--warning' });
      const k_area = el('textarea', { rows: 3, maxLength: 255, 'aria-label': 'Motivul anulării', placeholder: 'Motivul anulării (obligatoriu)' });
      const k_err = el('div', { className: 'vc-field-error' });
      const k_close = (k_value) => { k_dialog.close(); k_dialog.remove(); k_resolve(k_value); };
      const k_no = el('button', { type: 'button', textContent: 'Renunț' }); k_no.addEventListener('click', () => k_close(null));
      const k_yes = el('button', { type: 'button', textContent: 'Anulează documentul' });
      k_yes.addEventListener('click', () => { if (!k_area.value.trim()) { k_err.textContent = 'Motivul este obligatoriu.'; return; } k_close(k_area.value.trim()); });
      k_dialog.addEventListener('cancel', (k_e) => { k_e.preventDefault(); k_close(null); });
      k_dialog.append(el('header', {}, el('h2', { textContent: 'Anulare document' })), el('p', { className: 'confirm-box__text', textContent: k_text }), k_area, k_err, el('footer', {}, k_no, k_yes));
      document.body.append(k_dialog); k_dialog.showModal(); k_area.focus();
    });
  }

  async function showHistory() {
    const k_doc = k_state.doc; if (!k_doc) return;
    const k_r = await k_opts.api(url(`/${k_doc.id}/history`));
    const k_dialog = el('dialog', { className: 'confirm-box vc-history' });
    const k_label = { create: 'creat', update: 'modificat', cancel: 'anulat' };
    const k_rows = k_r.history.map((k_h) => el('tr', {}, el('td', { textContent: k_h.at || '' }), el('td', { textContent: k_h.user }),
      el('td', { textContent: k_label[String(k_h.operation).split('.').pop()] || k_h.operation }), el('td', { textContent: k_h.after || '' })));
    const k_close = el('button', { type: 'button', textContent: 'Închide' });
    k_close.addEventListener('click', () => { k_dialog.close(); k_dialog.remove(); });
    k_dialog.addEventListener('cancel', () => k_dialog.remove());
    k_dialog.append(el('header', {}, el('h2', { textContent: `Istoric • ${k_cfg.noun} nr. ${k_doc.number}` })),
      k_rows.length ? el('table', { className: 'vc-lines' }, el('thead', {}, el('tr', {}, ...['Data', 'Utilizator', 'Operație', 'Valori'].map((k_t) => el('th', { textContent: k_t })))), el('tbody', {}, ...k_rows))
        : el('p', { className: 'confirm-box__text', textContent: 'Nu există înregistrări în istoric (documentul a fost importat).' }),
      el('footer', {}, k_close));
    document.body.append(k_dialog); k_dialog.showModal();
  }

  function preview() {
    const k_doc = k_state.mode === 'view' ? k_state.doc : null;
    if (!k_doc) { say('Salvați documentul pentru a-l previzualiza.', true); return; }
    const k_w = window.open('', '_blank');
    if (!k_w) { say('Fereastra de previzualizare a fost blocată de browser.', true); return; }
    const k_esc = (k_s) => String(k_s ?? '').replace(/[&<>"]/g, (k_c) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[k_c]));
    const k_head = Object.entries(k_doc.header).filter(([k_k, k_v]) => k_v != null && k_v !== '' && !['source'].includes(k_k) && !/^Id/.test(k_k)).map(([k_k, k_v]) => `<tr><th>${k_esc(k_k)}</th><td>${k_esc(k_v)}</td></tr>`).join('');
    const k_cols = Object.keys(k_doc.lines[0] || {}).filter((k_k) => !['id', 'variant', 'product', 'IDTVA'].includes(k_k));
    const k_rows = k_doc.lines.map((k_l) => `<tr>${k_cols.map((k_c) => `<td>${k_esc(k_l[k_c])}</td>`).join('')}</tr>`).join('');
    k_w.document.write(`<!doctype html><html lang="ro"><head><meta charset="utf-8"><title>${k_esc(k_cfg.title)} ${k_esc(k_doc.number)}</title><style>body{font:13px Arial,sans-serif;margin:24px}table{border-collapse:collapse;margin:8px 0}th,td{border:1px solid #999;padding:3px 6px;text-align:left}h1{font-size:18px}.anulat{color:#c0392b;font-weight:700}</style></head><body><h1>${k_esc(k_cfg.title)} nr. ${k_esc(k_doc.number)}${k_doc.annulled ? ' <span class="anulat">ANULAT</span>' : ''}</h1><table>${k_head}</table><table><thead><tr>${k_cols.map((k_c) => `<th>${k_esc(k_c)}</th>`).join('')}</tr></thead><tbody>${k_rows}</tbody></table><p><button onclick="print()">Tipărește</button></p></body></html>`);
    k_w.document.close();
  }

  loadAll().then(() => { k_state.mode = 'view'; paint(); }).catch(report);
  paint();
  return {
    ready: Promise.resolve(),
    isDirty: () => k_state.dirty,
    destroy: () => { clearTimeout(k_availTimer); k_abort.abort(); k_host.replaceChildren(); },
  };
}
