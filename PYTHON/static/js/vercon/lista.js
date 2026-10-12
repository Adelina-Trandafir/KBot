// slice VC07-01: the food list (ListaAlimente) of an Alimente gestiune.
//
// One list = header (number, date, comments) + the three meals (portions, value of a portion, value of the meal,
// distributed) + the dishes of each meal + the SET of consumption bons (BC) it covers. A list can have SEVERAL bons: a DGV
// in the NIR style with one row per bon, TOTAL BC in the footer, the value of the list and the difference. "+ Adaugă rând"
// opens the popup with the bons that can still be taken (this gestiune, not cancelled, in no other list); choosing ADDS to
// the set, never replaces it; the ⋮ of a row only removes the link from the draft, the bon itself is not touched. Save
// writes the header, the meals and the whole set in one transaction; it never creates or re-saves a bon and never
// repeats a consumption. States as in the other documents: Consultare (tree of saved lists on the right) / Adăugare /
// Modificare (tree hidden) / Salvare. The numbers shown while typing are a convenience; the server computes the real ones.
import { confirmBox } from '../utils/confirm-box.js';
import { format, parse } from './fields.js';

const el = (k_tag, k_attrs = {}, ...k_children) => {
  const k_node = Object.assign(document.createElement(k_tag), k_attrs);
  k_node.append(...k_children);
  return k_node;
};
const newKey = () => globalThis.crypto?.randomUUID?.() || `${Date.now()}-${Math.random().toString(16).slice(2)}`;
const num = (k_t) => { const k_n = Number(String(k_t ?? '').replace(/\s/g, '').replace(',', '.')); return Number.isFinite(k_n) ? k_n : 0; };
const MEALS = [['md', 'Mic dejun'], ['d', 'Dejun'], ['c', 'Cină / Gustare']];
const monthName = (k_iso) => (k_iso ? ['ianuarie', 'februarie', 'martie', 'aprilie', 'mai', 'iunie', 'iulie', 'august', 'septembrie', 'octombrie', 'noiembrie', 'decembrie'][Number(k_iso.slice(5, 7)) - 1] : '—');
const dateText = (k_iso) => (k_iso ? format(k_iso, { type: 'date' }) : '');
let k_rows = 0;
const rowKey = () => `r${++k_rows}`;

export function mountLista(k_host, k_opts) {
  const k_abort = new AbortController();
  const k_signal = { signal: k_abort.signal };
  const k_state = { docs: [], lookups: null, doc: null, mode: 'view', draft: null, dirty: false, key: null, canWrite: false, closed: false, zec: 4, scale: 4 };
  const k_inputs = new Map();
  const k_msgs = new Map();
  const k_say = el('span', { className: 'vc-save-status', role: 'status' });
  const say = (k_t, k_bad) => { k_say.textContent = k_t || ''; k_say.style.color = k_bad ? '#c0392b' : ''; };
  const query = () => new URLSearchParams(Object.entries(k_opts.sel()).filter(([, k_v]) => k_v != null && k_v !== '').map(([k_k, k_v]) => [k_k, String(k_v)]));
  const url = (k_path, k_extra = '') => `/api/vercon/doc/lista${k_path}?${query()}${k_extra}`;
  const json = (k_method, k_path, k_body) => k_opts.api(`/api/vercon/doc/lista${k_path}`, { method: k_method, headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ ...k_opts.sel(), ...k_body }) });

  const k_title = el('h3'); const k_pill = el('span', { className: 'vc-pill' });
  const k_head = el('div', { className: 'vc-grid vc-doc__header' });
  const k_meals = el('div', { className: 'vc-lista__meals' });
  const k_bons = el('div', { className: 'vc-lista__bons' });
  const k_bar = el('div', { className: 'vc-actions vc-doc__bar' });
  const k_main = el('div', { className: 'vc-doc__main' }, el('div', { className: 'vc-doc__titlebar' }, k_title, k_pill), k_head, k_meals, k_bons, k_bar);
  const k_tree = el('aside', { className: 'vc-doc__tree', 'aria-label': 'Liste salvate' });
  k_host.replaceChildren(el('section', { className: 'vc-doc' }, k_main, k_tree));

  function report(k_error) {
    if (k_error?.name === 'AbortError') return;
    console.error('[vercon] list call failed', k_error);
    say(k_error.message || 'Operația nu a putut fi trimisă.', true);
  }

  // ---- data ------------------------------------------------------------------------------------------------------
  async function loadAll() {
    const [k_list, k_look] = await Promise.all([k_opts.api(url('/list')), k_opts.api(url('/lookups'))]);
    k_state.docs = k_list.documents; k_state.canWrite = k_list.can_write && !k_list.closed; k_state.closed = k_list.closed; k_state.zec = k_list.zec; k_state.scale = k_list.scale; k_state.lookups = k_look;
  }
  async function refresh() { k_state.docs = (await k_opts.api(url('/list'))).documents; }
  async function openDoc(k_id) {
    if (k_state.dirty && !(await confirmBox('Aveți modificări nesalvate. Dacă deschideți altă listă, ele se pierd.', { title: 'Modificări nesalvate', yes: 'Renunț la modificări', no: 'Rămân aici', warning: true }))) return;
    k_state.doc = await k_opts.api(url(`/${k_id}`)); k_state.mode = 'view'; k_state.draft = null; k_state.dirty = false; k_state.key = null; say(''); paint();
  }

  // ---- the draft -------------------------------------------------------------------------------------------------
  const blankMeal = () => ({ P: '', VMD: '', D: '', items: [] });
  function blank() { return { header: { Data: k_state.lookups.today, Numar: '', Comentarii: '' }, meals: { md: blankMeal(), d: blankMeal(), c: blankMeal() }, bons: [] }; }
  function fromDoc(k_doc) {
    const k_m = {};
    for (const [k_key] of MEALS) { const k_s = k_doc.meals[k_key]; k_m[k_key] = { P: k_s.P ?? '', VMD: k_s.VMD ?? '', D: k_s.D ?? '', items: k_s.items.map((k_i) => ({ key: rowKey(), IdFel: k_i.IdFel, Tip: k_i.Tip || '', Explicatie: k_i.Explicatie || '' })) }; }
    return { header: { Data: k_doc.header.Data, Numar: String(k_doc.number ?? ''), Comentarii: k_doc.header.Comentarii || '' }, meals: k_m, bons: k_doc.bons.map((k_b) => ({ ...k_b })) };
  }
  async function startAdd() {
    if (k_state.dirty && !(await confirmBox('Aveți modificări nesalvate. Dacă începeți o listă nouă, ele se pierd.', { title: 'Modificări nesalvate', yes: 'Renunț la modificări', no: 'Rămân aici', warning: true }))) return;
    k_state.mode = 'add'; k_state.doc = null; k_state.draft = blank(); k_state.dirty = false; k_state.key = null; say(''); paint();
  }
  function startEdit() { k_state.mode = 'edit'; k_state.draft = fromDoc(k_state.doc); k_state.dirty = false; k_state.key = null; say(''); paint(); }
  async function cancelEdit() {
    if (k_state.dirty && !(await confirmBox('Renunțați la modificările nesalvate?', { title: 'Renunțare', yes: 'Renunț', no: 'Rămân aici', warning: true }))) return;
    k_state.mode = 'view'; k_state.draft = null; k_state.dirty = false; k_state.key = null; say(''); paint();
  }

  // ---- painting --------------------------------------------------------------------------------------------------
  const editing = () => k_state.mode !== 'view';
  function paint() {
    k_inputs.clear(); k_msgs.clear();
    const k_doc = k_state.doc;
    k_title.textContent = !editing() && k_doc ? `Listă de alimente • nr. ${k_doc.number}` : (k_state.mode === 'add' ? 'Listă de alimente • listă nouă' : (k_state.mode === 'edit' ? 'Listă de alimente • modificare' : 'Liste de alimente'));
    k_pill.textContent = editing() ? 'Ciornă' : (k_doc ? (k_doc.annulled ? 'Anulată' : 'Salvată') : ''); k_pill.dataset.state = editing() ? 'draft' : (k_doc?.annulled ? 'annulled' : 'saved'); k_pill.hidden = !k_pill.textContent;
    paintHead(); paintMeals(); paintBons(); paintBar(); paintTree();
  }
  const view = () => (editing() ? k_state.draft : (k_state.doc ? fromDoc(k_state.doc) : null));
  const field = (k_key, k_label, k_value, k_readonly, k_attrs = {}) => {
    const k_i = el('input', { type: 'text', value: k_value ?? '', readOnly: k_readonly, autocomplete: 'off', ...k_attrs });
    k_i.setAttribute('aria-label', k_label); k_inputs.set(k_key, k_i);
    const k_m = el('div', { className: 'vc-field-error' }); k_msgs.set(k_key, k_m);
    k_i.addEventListener('input', () => { k_state.dirty = true; }, k_signal);
    return { k_i, k_m };
  };

  function paintHead() {
    k_head.replaceChildren();
    const k_v = view();
    if (!k_v) { k_head.append(el('p', { className: 'vc-nom__empty', textContent: k_state.docs.length ? 'Alegeți o listă din arbore sau adăugați una nouă.' : 'Nu există liste. Adăugați prima.' })); return; }
    const k_e = editing();
    const k_n = field('h.Numar', 'Număr', k_v.header.Numar, !k_e);
    const k_d = field('h.Data', 'Data', k_e ? dateText(k_v.header.Data) : dateText(k_v.header.Data), !k_e);
    const k_c = field('h.Comentarii', 'Comentarii', k_v.header.Comentarii, !k_e);
    k_head.append(el('label', {}, 'Număr listă', k_n.k_i, el('span', { className: 'vc-hint', textContent: k_e ? 'Gol = număr automat.' : '' }), k_n.k_m), el('label', {}, `Data${k_e ? ' *' : ''}`, k_d.k_i, k_d.k_m), el('label', {}, 'Comentarii', k_c.k_i, k_c.k_m));
  }

  function paintMeals() {
    if (editing() && k_state.draft) {   // a repaint (a dish added / removed) must not lose what was typed in the meal rows
      for (const [k_key] of MEALS) for (const [k_f, k_name] of [['P', 'P'], ['VMD', 'VMD'], ['D', 'D']]) { const k_i = k_inputs.get(`m.${k_key}.${k_f}`); if (k_i) k_state.draft.meals[k_key][k_name] = k_i.value; }
    }
    k_meals.replaceChildren();
    const k_v = view(); if (!k_v) return;
    const k_e = editing();
    const k_table = el('table', { className: 'vc-lines vc-lines--doc' });
    k_table.append(el('thead', {}, el('tr', {}, ...['Masa', 'Porții', 'Valoare porție (lei)', 'Valoare masă (lei)', 'Distribuit'].map((k_t) => el('th', { textContent: k_t })))));
    const k_body = el('tbody');
    for (const [k_key, k_label] of MEALS) {
      const k_m = k_v.meals[k_key];
      const k_p = field(`m.${k_key}.P`, `${k_label}: porții`, k_m.P, !k_e), k_vmd = field(`m.${k_key}.VMD`, `${k_label}: valoare porție`, k_m.VMD === '' ? '' : format(k_m.VMD, { type: 'decimal' }), !k_e), k_dd = field(`m.${k_key}.D`, `${k_label}: distribuit`, k_m.D, !k_e);
      const k_val = el('td', { className: 'is-num', 'data-meal-value': k_key });
      const refresh = () => { k_val.textContent = (num(k_p.k_i.value) * num(k_vmd.k_i.value)).toFixed(k_state.zec).replace('.', ','); recalc(); };
      if (k_e) { k_p.k_i.addEventListener('input', refresh, k_signal); k_vmd.k_i.addEventListener('input', refresh, k_signal); }
      k_val.textContent = (num(k_m.P) * num(k_m.VMD)).toFixed(k_state.zec).replace('.', ',');
      k_body.append(el('tr', {}, el('td', { textContent: k_label }), el('td', {}, k_p.k_i, k_p.k_m), el('td', {}, k_vmd.k_i, k_vmd.k_m), k_val, el('td', {}, k_dd.k_i, k_dd.k_m)));
    }
    k_table.append(k_body);
    k_meals.append(k_table);
    // the dishes of each meal
    for (const [k_key, k_label] of MEALS) {
      const k_m = k_v.meals[k_key];
      const k_items = el('table', { className: 'vc-lines vc-lines--doc' });
      k_items.append(el('thead', {}, el('tr', {}, el('th', { textContent: `${k_label} — feluri` }), el('th', { textContent: 'Tip' }), el('th', { textContent: 'Explicații' }), ...(k_e ? [el('th')] : []))));
      const k_ib = el('tbody');
      for (const k_it of k_m.items) {
        const k_sel = el('select', { disabled: !k_e });
        k_sel.append(el('option', { value: '', textContent: '— fel —' }));
        for (const k_dsh of k_state.lookups.dishes) k_sel.append(el('option', { value: String(k_dsh.id), textContent: k_dsh.label }));
        k_sel.value = k_it.IdFel != null ? String(k_it.IdFel) : '';
        k_sel.addEventListener('change', () => { k_it.IdFel = k_sel.value ? Number(k_sel.value) : null; if (k_sel.value && !k_it.Explicatie) { k_it.Explicatie = k_sel.selectedOptions[0].textContent; paintMeals(); } k_state.dirty = true; }, k_signal);
        const k_tip = el('input', { type: 'text', value: k_it.Tip || '', readOnly: !k_e, maxLength: 50 }); k_tip.addEventListener('input', () => { k_it.Tip = k_tip.value; k_state.dirty = true; }, k_signal);
        const k_ex = el('input', { type: 'text', value: k_it.Explicatie || '', readOnly: !k_e, maxLength: 255 }); k_ex.addEventListener('input', () => { k_it.Explicatie = k_ex.value; k_state.dirty = true; }, k_signal);
        const k_tr = el('tr', {}, el('td', {}, k_sel), el('td', {}, k_tip), el('td', {}, k_ex));
        if (k_e) { const k_x = el('button', { type: 'button', textContent: '✕', title: 'Șterge rândul' }); k_x.addEventListener('click', () => { k_m.items = k_m.items.filter((k_o) => k_o !== k_it); k_state.dirty = true; paintMeals(); }, k_signal); k_tr.append(el('td', {}, k_x)); }
        k_ib.append(k_tr);
      }
      k_items.append(k_ib);
      const k_add = el('button', { type: 'button', textContent: '+ Adaugă rând', disabled: !k_e }); k_add.addEventListener('click', () => { k_m.items.push({ key: rowKey(), IdFel: null, Tip: '', Explicatie: '' }); k_state.dirty = true; paintMeals(); }, k_signal);
      k_meals.append(k_items, el('div', { className: 'vc-doc__linesfoot' }, k_add));
    }
    recalc();
  }

  function paintBons() {
    k_bons.replaceChildren();
    const k_v = view(); if (!k_v) return;
    const k_e = editing();
    const k_table = el('table', { className: 'vc-lines vc-lines--doc' });
    k_table.append(el('thead', {}, el('tr', {}, ...['#', 'Număr BC', 'Data', 'Primitor', 'Valoare bon (lei)'].map((k_t) => el('th', { textContent: k_t })), ...(k_e ? [el('th')] : []))));
    const k_body = el('tbody');
    k_v.bons.forEach((k_b, k_i) => {
      const k_tr = el('tr', { className: k_b.annulled ? 'is-inactive' : '' }, el('td', { textContent: String(k_i + 1) }), el('td', { textContent: k_b.number ?? '' }), el('td', { textContent: dateText(k_b.date) }), el('td', { textContent: k_b.receiver || '' }),
        el('td', { className: 'is-num', textContent: format(k_b.total, { type: 'decimal' }) }));
      if (k_e) { const k_x = el('button', { type: 'button', textContent: '⋮', title: 'Scoate bonul din listă (bonul nu se șterge)' }); k_x.addEventListener('click', () => { k_state.draft.bons = k_state.draft.bons.filter((k_o) => k_o.id !== k_b.id); k_state.dirty = true; paintBons(); recalc(); }, k_signal); k_tr.append(el('td', {}, k_x)); }
      k_body.append(k_tr);
    });
    k_table.append(k_body);
    const k_add = el('button', { type: 'button', textContent: '+ Adaugă rând', disabled: !k_e });
    k_add.addEventListener('click', () => chooseBon().catch(report), k_signal);
    k_bons.append(el('h4', { textContent: 'Bonuri de consum asociate' }), k_table, el('div', { className: 'vc-doc__linesfoot' }, k_add, el('span', { className: 'vc-doc__total', 'data-sums': '' })));
    recalc();
  }

  function recalc() {
    const k_t = k_bons.querySelector('[data-sums]'); if (!k_t) return;
    const k_v = view(); if (!k_v) return;
    const k_zec = k_state.zec; const f = (k_n) => k_n.toFixed(k_zec).replace('.', ',');
    const k_bonTotal = k_v.bons.filter((k_b) => !k_b.annulled).reduce((k_a, k_b) => k_a + num(k_b.total), 0);
    let k_list = 0;
    if (editing()) {
      for (const [k_key] of MEALS) { const k_m = k_v.meals[k_key]; const k_p = k_inputs.get(`m.${k_key}.P`), k_vm = k_inputs.get(`m.${k_key}.VMD`); k_list += num(k_p ? k_p.value : k_m.P) * num(k_vm ? k_vm.value : k_m.VMD); }
    } else k_list = num(k_state.doc?.header.Valoare);
    k_t.textContent = `TOTAL BC ${f(k_bonTotal)} • Valoare listă ${f(k_list)} • Diferență ${f(k_list - k_bonTotal)} lei`;
  }

  // the popup: only bons that can still be taken, grouped receiver -> date; a choice ADDS to the draft set
  async function chooseBon() {
    const k_r = await k_opts.api(url('/bons', k_state.mode === 'edit' ? `&exclude=${k_state.doc.id}` : ''));
    const k_have = new Set(k_state.draft.bons.map((k_b) => k_b.id));
    const k_list = k_r.bons.filter((k_b) => !k_have.has(k_b.id));
    const k_dialog = el('dialog', { className: 'confirm-box vc-history' });
    const k_close = () => { k_dialog.close(); k_dialog.remove(); };
    const k_groups = new Map();
    for (const k_b of k_list) { if (!k_groups.has(k_b.receiver || '—')) k_groups.set(k_b.receiver || '—', new Map()); const k_g = k_groups.get(k_b.receiver || '—'); const k_d = dateText(k_b.date); if (!k_g.has(k_d)) k_g.set(k_d, []); k_g.get(k_d).push(k_b); }
    const k_tree = el('div', { className: 'vc-nom__tree' });
    if (!k_list.length) k_tree.append(el('p', { className: 'confirm-box__text', textContent: 'Nu există bonuri de consum disponibile (neasociate altei liste).' }));
    for (const [k_name, k_g] of k_groups) {
      const k_d1 = el('details', { open: true }, el('summary', { textContent: k_name }));
      for (const [k_day, k_bs] of k_g) {
        const k_d2 = el('details', { open: true }, el('summary', { textContent: k_day }));
        for (const k_b of k_bs) {
          const k_leaf = el('button', { type: 'button', className: 'vc-nom__leaf', textContent: `BC ${k_b.number} • ${format(k_b.total, { type: 'decimal' })} lei` });
          k_leaf.addEventListener('click', () => { k_state.draft.bons.push({ id: k_b.id, number: k_b.number, date: k_b.date, receiver: k_b.receiver, total: k_b.total, annulled: false }); k_state.dirty = true; k_close(); paintBons(); }, { signal: k_abort.signal });
          k_d2.append(k_leaf);
        }
        k_d1.append(k_d2);
      }
      k_tree.append(k_d1);
    }
    const k_no = el('button', { type: 'button', textContent: 'Renunț' }); k_no.addEventListener('click', k_close);
    k_dialog.addEventListener('cancel', (k_e) => { k_e.preventDefault(); k_close(); });
    k_dialog.append(el('header', {}, el('h2', { textContent: 'Alege B.C.' })), k_tree, el('footer', {}, k_no));
    document.body.append(k_dialog); k_dialog.showModal();
  }

  function paintTree() {
    k_tree.hidden = editing();
    if (k_tree.hidden) return;
    const k_nodes = [el('h4', { textContent: 'Liste salvate' })];
    if (!k_state.docs.length) k_nodes.push(el('p', { className: 'vc-nom__empty', textContent: 'Nu există liste.' }));
    const k_months = new Map();
    for (const k_d of k_state.docs) { const k_m = monthName(k_d.date); if (!k_months.has(k_m)) k_months.set(k_m, new Map()); const k_g = k_months.get(k_m); const k_day = dateText(k_d.date); if (!k_g.has(k_day)) k_g.set(k_day, []); k_g.get(k_day).push(k_d); }
    for (const [k_m, k_days] of k_months) {
      const k_det = el('details', { open: [...k_days.values()].flat().some((k_d) => k_d.id === k_state.doc?.id) || k_months.size === 1 }, el('summary', { textContent: k_m }));
      for (const [k_day, k_ds] of k_days) {
        const k_d2 = el('details', { open: true }, el('summary', { textContent: k_day }));
        for (const k_d of k_ds) { const k_leaf = el('button', { type: 'button', className: `vc-nom__leaf${k_d.id === k_state.doc?.id ? ' is-selected' : ''}${k_d.annulled ? ' is-inactive' : ''}`, textContent: `Lista ${k_d.number} • ${format(k_d.total, { type: 'decimal' })} lei${k_d.annulled ? ' • anulată' : ''}` }); k_leaf.addEventListener('click', () => openDoc(k_d.id).catch(report), k_signal); k_d2.append(k_leaf); }
        k_det.append(k_d2);
      }
      k_nodes.push(k_det);
    }
    k_tree.replaceChildren(...k_nodes);
  }

  // ---- commands --------------------------------------------------------------------------------------------------
  function paintBar() {
    k_bar.replaceChildren();
    const btn = (k_t, k_fn, k_attrs = {}) => { const k_b = el('button', { type: 'button', textContent: k_t, ...k_attrs }); k_b.addEventListener('click', () => Promise.resolve(k_fn()).catch(report), k_signal); return k_b; };
    if (editing()) { k_bar.append(btn('Renunță', cancelEdit), btn('Salvează lista', save, { className: 'is-primary' }), k_say); return; }
    const k_doc = k_state.doc; const k_can = k_state.canWrite;
    k_bar.append(btn('+ Adaugă', startAdd, { hidden: !k_can }));
    if (k_doc) k_bar.append(btn('Modifică', startEdit, { hidden: !k_can || k_doc.annulled }), btn('Anulează', cancelList, { hidden: !k_can || k_doc.annulled, className: 'is-danger' }));
    k_bar.append(k_say);
    if (k_state.closed) k_bar.append(el('span', { className: 'vc-hint', textContent: 'Perioada este închisă: listele se pot doar consulta.' }));
  }

  function collect() {
    const k_errors = {}; const k_v = k_state.draft;
    const k_header = {};
    const k_n = parse(k_inputs.get('h.Numar')?.value, { type: 'int', required: false, min: 1 }); if (!k_n.ok) k_errors['h.Numar'] = k_n.message; else k_header.Numar = k_n.value;
    const k_dt = parse(k_inputs.get('h.Data')?.value, { type: 'date', required: true }); if (!k_dt.ok) k_errors['h.Data'] = k_dt.message; else k_header.Data = k_dt.value;
    k_header.Comentarii = (k_inputs.get('h.Comentarii')?.value || '').trim() || null;
    const k_meals = {};
    for (const [k_key] of MEALS) {
      const k_m = k_v.meals[k_key];
      const k_p = parse(k_inputs.get(`m.${k_key}.P`)?.value, { type: 'int', required: false, min: 0 }); if (!k_p.ok) k_errors[`m.${k_key}.P`] = k_p.message;
      const k_vm = parse(k_inputs.get(`m.${k_key}.VMD`)?.value, { type: 'decimal', required: false, min: 0 }); if (!k_vm.ok) k_errors[`m.${k_key}.VMD`] = k_vm.message;
      const k_dd = parse(k_inputs.get(`m.${k_key}.D`)?.value, { type: 'int', required: false, min: 0 }); if (!k_dd.ok) k_errors[`m.${k_key}.D`] = k_dd.message;
      k_meals[k_key] = { P: k_p.value ?? 0, VMD: k_vm.value ?? 0, D: k_dd.value ?? 0, items: k_m.items.map((k_i) => ({ IdFel: k_i.IdFel, Tip: (k_i.Tip || '').trim() || null, Explicatie: (k_i.Explicatie || '').trim() })) };
    }
    return { errors: k_errors, body: { header: k_header, meals: k_meals, bons: k_v.bons.map((k_b) => k_b.id) } };
  }

  async function save() {
    const k_c = collect();
    if (Object.keys(k_c.errors).length) { for (const [k_k, k_t] of Object.entries(k_c.errors)) { const k_m = k_msgs.get(k_k); if (k_m) k_m.textContent = k_t; } say('Corectați câmpurile marcate.', true); return; }
    if (!k_c.body.bons.length && !(await confirmBox('Nu ați ales niciun bon de consum pentru lista curentă. Este corect?', { title: 'Listă fără bonuri', yes: 'Da, salvează', no: 'Revin' }))) return;
    say('Se salvează…');
    const k_buttons = k_bar.querySelectorAll('button'); k_buttons.forEach((k_b) => { k_b.disabled = true; });
    try {
      let k_r;
      if (k_state.mode === 'add') { k_state.key = k_state.key || newKey(); k_r = await json('POST', '', { ...k_c.body, request_key: k_state.key }); }
      else k_r = await json('PUT', `/${k_state.doc.id}`, { ...k_c.body, version: k_state.doc.version });
      k_state.key = null; k_state.dirty = false; k_state.mode = 'view'; k_state.draft = null; k_state.doc = k_r.document;
      await refresh(); say(k_r.replayed ? 'Salvat (cererea fusese deja aplicată).' : 'Salvat.'); paint();
    } catch (k_error) { k_buttons.forEach((k_b) => { k_b.disabled = false; }); await failed(k_error); }
  }

  async function failed(k_error) {
    if (k_error.name === 'AbortError') return;
    if (k_error.fields && k_error.reason === 'INVALID') { k_state.key = null; for (const [k_k, k_t] of Object.entries(k_error.fields)) { const k_m = k_msgs.get(k_k); if (k_m) k_m.textContent = k_t; } say(k_error.message, true); return; }
    if (k_error.status && k_error.status < 500) { k_state.key = null; say(`${k_error.message} Datele introduse au rămas în formular.`, true); return; }
    console.error('[vercon] list save failed', k_error);
    say(`${k_error.message || 'Salvarea nu a putut fi trimisă.'} Dacă ați apăsat Salvează, apăsați din nou: aceeași cerere nu se aplică de două ori.`, true);
  }

  async function cancelList() {
    const k_doc = k_state.doc; if (!k_doc) return;
    const k_reason = await new Promise((k_resolve) => {
      const k_dialog = el('dialog', { className: 'confirm-box confirm-box--warning' });
      const k_area = el('textarea', { rows: 3, maxLength: 255, 'aria-label': 'Motivul anulării', placeholder: 'Motivul anulării (obligatoriu)' });
      const k_err = el('div', { className: 'vc-field-error' });
      const k_close = (k_v) => { k_dialog.close(); k_dialog.remove(); k_resolve(k_v); };
      const k_no = el('button', { type: 'button', textContent: 'Renunț' }); k_no.addEventListener('click', () => k_close(null));
      const k_yes = el('button', { type: 'button', textContent: 'Anulează lista' }); k_yes.addEventListener('click', () => { if (!k_area.value.trim()) { k_err.textContent = 'Motivul este obligatoriu.'; return; } k_close(k_area.value.trim()); });
      k_dialog.addEventListener('cancel', (k_e) => { k_e.preventDefault(); k_close(null); });
      k_dialog.append(el('header', {}, el('h2', { textContent: 'Anulare listă' })), el('p', { className: 'confirm-box__text', textContent: `Anulați lista nr. ${k_doc.number}? Bonurile de consum rămân neschimbate și pot fi asociate altei liste.` }), k_area, k_err, el('footer', {}, k_no, k_yes));
      document.body.append(k_dialog); k_dialog.showModal(); k_area.focus();
    });
    if (k_reason == null) return;
    try { const k_r = await json('POST', `/${k_doc.id}/cancel`, { reason: k_reason, version: k_doc.version }); k_state.doc = k_r.document; await refresh(); say('Anulată.'); paint(); } catch (k_error) { await failed(k_error); }
  }

  loadAll().then(() => paint()).catch(report);
  paint();
  return { ready: Promise.resolve(), isDirty: () => k_state.dirty, destroy: () => { k_abort.abort(); k_host.replaceChildren(); } };
}
