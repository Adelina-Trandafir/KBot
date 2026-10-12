// slice VC06-01: the nomenclature screens (categories, products + price variants, holders, VAT rates, committees) and
// the gestiune settings. One generic list + form (`mountCrud`); the server describes the columns, so a column is never
// invented here. Layout follows the NIR/BC mockups: compact grid with thin borders, the form beside the list, and the
// buttons Adaugă / Modifică / Renunță / Salvează in the footer (the Access Produse form had the same four).
//
// Rules the page keeps (the server enforces them again): nothing is deleted -- a row is switched inactive; every
// change carries the revision the page saw (an old revision is a conflict and the list is reloaded); a create carries
// one RequestKey that is kept until the server answers, so a lost answer cannot create the row twice; unsaved input
// is never thrown away silently (`isDirty` is asked before the context or the form changes).
import { confirmBox } from '../utils/confirm-box.js';
import { format, parse } from './fields.js';

const el = (k_tag, k_attrs = {}, ...k_children) => {
  const k_node = Object.assign(document.createElement(k_tag), k_attrs);
  k_node.append(...k_children);
  return k_node;
};
const newKey = () => globalThis.crypto?.randomUUID?.() || `${Date.now()}-${Math.random().toString(16).slice(2)}`;
const GROUP_FIELD = { produse: 'IdMasina' };   // the list of products is a tree: category -> product

// opts: { api, sel: () => ({subunit, gestiune, year}), kind, parent?: () => id|null, title, onSelect?(row|null), signal }
function mountCrud(k_host, k_opts) {
  const k_state = { rows: [], columns: [], lookups: {}, canWrite: false, selected: null, mode: 'view', filter: '', inactive: false, key: null, dirty: false, loaded: false };
  const k_abort = new AbortController();
  const k_signal = { signal: k_abort.signal };
  const k_inputs = new Map();   // field -> input / select
  const k_errors = new Map();   // field -> message element
  const k_say = el('span', { className: 'vc-save-status', role: 'status' });
  const say = (k_text, k_bad) => { k_say.textContent = k_text || ''; k_say.style.color = k_bad ? '#c0392b' : ''; };
  const query = () => new URLSearchParams(Object.entries(k_opts.sel()).filter(([, k_v]) => k_v != null && k_v !== '').map(([k_k, k_v]) => [k_k, String(k_v)]));
  const parentId = () => (k_opts.parent ? k_opts.parent() : null);

  const k_search = el('input', { type: 'search', placeholder: 'Caută…', 'aria-label': 'Caută' });
  const k_inactive = el('input', { type: 'checkbox' });
  const k_list = el('div', { className: 'vc-nom__list' });
  const k_form = el('form', { className: 'vc-grid vc-nom__form', noValidate: true });
  const k_btnAdd = el('button', { type: 'button', textContent: '+ Adaugă' });
  const k_btnEdit = el('button', { type: 'button', textContent: 'Modifică' });
  const k_btnToggle = el('button', { type: 'button', textContent: 'Dezactivează' });
  const k_btnCancel = el('button', { type: 'button', textContent: 'Renunță' });
  const k_btnSave = el('button', { type: 'submit', textContent: 'Salvează' });
  const k_empty = el('p', { className: 'vc-nom__empty' });
  const k_footer = el('div', { className: 'vc-actions' }, k_btnCancel, k_btnToggle, k_btnEdit, k_btnAdd, k_btnSave, k_say);
  k_form.append(el('div', { className: 'vc-nom__fields' }));
  const k_body = el('div', { className: 'vc-nom__body' }, el('div', { className: 'vc-nom__left' }, k_list, k_empty), el('div', { className: 'vc-nom__right' }, k_form));
  const k_section = el('section', { className: 'vc-nom' },
    el('div', { className: 'vc-nom__bar' }, el('h3', { textContent: k_opts.title }), k_search,
      el('label', { className: 'vc-nom__check' }, k_inactive, ' Arată inactivele')),
    k_body, k_footer);
  k_host.replaceChildren(k_section);

  const rowTitle = (k_row) => {
    const k_col = k_state.columns[0];
    return k_row && k_col ? (k_row[k_col.key] ?? '') : '';
  };
  const selectedRow = () => k_state.rows.find((k_r) => k_r.id === k_state.selected) || null;

  function visibleRows() {
    const k_needle = k_state.filter.trim().toLowerCase();
    return k_state.rows.filter((k_r) => (k_state.inactive || k_r.active)
      && (!k_needle || k_state.columns.some((k_c) => String(k_r[k_c.key] ?? '').toLowerCase().includes(k_needle))));
  }

  function paintList() {
    const k_rows = visibleRows();
    k_empty.textContent = !k_state.loaded ? 'Se încarcă…' : (k_rows.length ? '' : 'Nu există înregistrări.');
    const k_groupKey = GROUP_FIELD[k_opts.kind];
    if (k_groupKey) { paintTree(k_rows, k_groupKey); return; }
    const k_table = el('table', { className: 'vc-lines' });
    k_table.append(el('thead', {}, el('tr', {}, ...k_state.columns.map((k_c) => el('th', { textContent: k_c.label })))));
    const k_tbody = el('tbody');
    for (const k_row of k_rows) {
      const k_tr = el('tr', { className: `${k_row.id === k_state.selected ? 'is-selected' : ''}${k_row.active ? '' : ' is-inactive'}`, tabIndex: 0 });
      for (const k_c of k_state.columns) k_tr.append(el('td', { textContent: cellText(k_row, k_c) }));
      k_tr.addEventListener('click', () => choose(k_row.id), k_signal);
      k_tr.addEventListener('keydown', (k_e) => { if (k_e.key === 'Enter') choose(k_row.id); }, k_signal);
      k_tbody.append(k_tr);
    }
    k_table.append(k_tbody);
    k_list.replaceChildren(k_table);
  }

  function paintTree(k_rows, k_groupKey) {
    const k_groups = new Map();
    for (const k_option of k_state.lookups[k_groupKey] || []) k_groups.set(k_option.id, { label: k_option.label, rows: [] });
    const k_loose = { label: 'Fără categorie', rows: [] };
    for (const k_row of k_rows) (k_groups.get(k_row[k_groupKey]) || k_loose).rows.push(k_row);
    const k_tree = el('div', { className: 'vc-nom__tree' });
    for (const k_group of [...k_groups.values(), k_loose]) {
      if (!k_group.rows.length) continue;
      const k_open = !!k_state.filter || k_group.rows.some((k_r) => k_r.id === k_state.selected);
      const k_details = el('details', { open: k_open }, el('summary', { textContent: `${k_group.label} (${k_group.rows.length})` }));
      for (const k_row of k_group.rows) {
        const k_btn = el('button', { type: 'button', textContent: rowTitle(k_row), className: `vc-nom__leaf${k_row.id === k_state.selected ? ' is-selected' : ''}${k_row.active ? '' : ' is-inactive'}` });
        k_btn.addEventListener('click', () => choose(k_row.id), k_signal);
        k_details.append(k_btn);
      }
      k_tree.append(k_details);
    }
    k_list.replaceChildren(k_tree);
  }

  function cellText(k_row, k_col) {
    const k_value = k_row[k_col.key];
    if (k_col.type === 'ref') return (k_state.lookups[k_col.key] || []).find((k_o) => k_o.id === k_value)?.label ?? '';
    return format(k_value, k_col);
  }

  function paintForm() {
    const k_row = k_state.mode === 'add' ? null : selectedRow();
    const k_editing = k_state.mode !== 'view';
    const k_host2 = k_form.querySelector('.vc-nom__fields');
    k_inputs.clear(); k_errors.clear();
    k_host2.replaceChildren();
    const k_blocked = k_opts.parent && parentId() == null;
    for (const k_col of k_state.columns) {
      let k_input;
      if (k_col.type === 'ref') {
        k_input = el('select', { disabled: !k_editing });
        k_input.append(el('option', { value: '', textContent: '— fără —' }));
        for (const k_o of k_state.lookups[k_col.key] || []) k_input.append(el('option', { value: String(k_o.id), textContent: k_o.label }));
        k_input.value = k_row?.[k_col.key] != null ? String(k_row[k_col.key]) : '';
      } else {
        k_input = el('input', { type: 'text', value: k_row ? format(k_row[k_col.key], k_col) : '', readOnly: !k_editing, autocomplete: 'off', name: k_col.key });
        if (k_col.type === 'decimal' || k_col.type === 'int') k_input.inputMode = 'decimal';
      }
      k_input.setAttribute('aria-label', k_col.label);
      k_input.addEventListener('input', () => { k_state.dirty = true; }, k_signal);
      const k_msg = el('div', { className: 'vc-field-error' });
      k_inputs.set(k_col.key, k_input); k_errors.set(k_col.key, k_msg);
      k_host2.append(el('label', {}, `${k_col.label}${k_col.required ? ' *' : ''}`, k_input, k_msg));
    }
    if (k_blocked) k_host2.replaceChildren(el('p', { className: 'vc-nom__empty', textContent: 'Alegeți mai întâi un produs.' }));
    const k_w = k_state.canWrite && !k_blocked;
    k_btnAdd.hidden = !k_w || k_editing; k_btnEdit.hidden = !k_w || k_editing || !k_row;
    k_btnToggle.hidden = !k_w || k_editing || !k_row; k_btnToggle.textContent = k_row?.active === false ? 'Activează' : 'Dezactivează';
    k_btnSave.hidden = !k_editing; k_btnCancel.hidden = !k_editing;
  }

  function paintAll() { paintList(); paintForm(); }

  async function choose(k_id) {
    if (k_state.dirty && !(await confirmBox('Aveți modificări nesalvate în formular. Dacă alegeți alt rând, ele se pierd.', { title: 'Modificări nesalvate', yes: 'Renunț la modificări', no: 'Rămân aici', warning: true }))) return;
    k_state.selected = k_id; k_state.mode = 'view'; k_state.dirty = false; say('');
    paintAll();
    k_opts.onSelect?.(selectedRow());
  }

  async function load(k_keepSelection = true) {
    const k_keep = k_keepSelection ? k_state.selected : null;
    const k_p = parentId();
    if (k_opts.parent && k_p == null) { k_state.rows = []; k_state.loaded = true; paintAll(); return; }
    const k_url = `/api/vercon/nom/${encodeURIComponent(k_opts.kind)}?${query()}${k_p != null ? `&parent=${k_p}` : ''}`;
    const k_data = await k_opts.api(k_url);
    k_state.rows = k_data.rows; k_state.columns = k_data.columns; k_state.lookups = k_data.lookups; k_state.canWrite = k_data.can_write;
    k_state.loaded = true;
    k_state.selected = k_state.rows.some((k_r) => k_r.id === k_keep) ? k_keep : null;
    paintAll();
  }

  function collect() {
    const k_values = {}; let k_ok = true;
    for (const k_col of k_state.columns) {
      const k_input = k_inputs.get(k_col.key);
      k_errors.get(k_col.key).textContent = '';
      if (k_col.type === 'ref') { k_values[k_col.key] = k_input.value === '' ? null : Number(k_input.value); continue; }
      const k_r = parse(k_input.value, k_col);
      if (!k_r.ok) { k_errors.get(k_col.key).textContent = k_r.message; k_ok = false; } else k_values[k_col.key] = k_r.value;
    }
    return k_ok ? k_values : null;
  }

  async function save(k_event) {
    k_event.preventDefault();
    const k_values = collect();
    if (!k_values) { say('Corectați câmpurile marcate.', true); return; }
    k_btnSave.disabled = true; say('Se salvează…');
    const k_sel = k_opts.sel();
    try {
      let k_result;
      if (k_state.mode === 'add') {
        k_state.key = k_state.key || newKey();   // kept until the server answers: a retry replays, never duplicates
        k_result = await k_opts.api(`/api/vercon/nom/${encodeURIComponent(k_opts.kind)}`, { method: 'POST', headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ subunit: k_sel.subunit, gestiune: k_sel.gestiune, values: k_values, request_key: k_state.key, parent: parentId() }) });
      } else {
        const k_row = selectedRow();
        k_result = await k_opts.api(`/api/vercon/nom/${encodeURIComponent(k_opts.kind)}/${k_row.id}`, { method: 'PUT', headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ subunit: k_sel.subunit, gestiune: k_sel.gestiune, values: k_values, version: k_row.version }) });
      }
      k_state.key = null; k_state.dirty = false; k_state.mode = 'view'; k_state.selected = k_result.row.id;
      await load(true); say(k_result.replayed ? 'Salvat (cererea fusese deja aplicată).' : 'Salvat.');
      k_opts.onSelect?.(selectedRow());
    } catch (k_error) { await failed(k_error); } finally { k_btnSave.disabled = false; }
  }

  async function failed(k_error) {
    if (k_error.name === 'AbortError') return;
    if (k_error.reason === 'INVALID' && k_error.fields) {
      for (const [k_f, k_m] of Object.entries(k_error.fields)) { const k_msg = k_errors.get(k_f); if (k_msg) k_msg.textContent = k_m; }
      k_state.key = null;                          // nothing was applied: the next try is a new request
      say(k_error.message, true); return;
    }
    if (k_error.reason === 'CONFLICT') {
      k_state.dirty = false; k_state.mode = 'view'; k_state.key = null;
      await load(true); say(`${k_error.message} Datele introduse nu au fost salvate.`, true); return;
    }
    if (k_error.reason === 'NOT_FOUND') { k_state.dirty = false; k_state.mode = 'view'; await load(false); say(k_error.message, true); return; }
    console.error('[vercon] nomenclature call failed', k_error);
    say(k_error.status && k_error.status < 500 ? k_error.message : `${k_error.message || 'Operația nu a putut fi trimisă.'} Dacă ați apăsat Salvează, apăsați din nou: aceeași cerere nu se aplică de două ori.`, true);
  }

  async function toggle() {
    const k_row = selectedRow(); if (!k_row) return;
    const k_next = !k_row.active;
    if (!k_next || await confirmBox(`«${rowTitle(k_row)}» nu va mai putea fi ales în documente noi. Documentele salvate rămân neschimbate.`, { title: 'Dezactivare', yes: 'Dezactivează', no: 'Renunț' })) {
      const k_sel = k_opts.sel();
      try {
        await k_opts.api(`/api/vercon/nom/${encodeURIComponent(k_opts.kind)}/${k_row.id}/active`, { method: 'POST', headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ subunit: k_sel.subunit, gestiune: k_sel.gestiune, active: k_next, version: k_row.version }) });
        await load(true); say(k_next ? 'Activat.' : 'Dezactivat.');
      } catch (k_error) { await failed(k_error); }
    }
  }

  k_search.addEventListener('input', () => { k_state.filter = k_search.value; paintList(); }, k_signal);
  k_inactive.addEventListener('change', () => { k_state.inactive = k_inactive.checked; paintList(); }, k_signal);
  k_btnAdd.addEventListener('click', async () => {
    if (k_state.dirty && !(await confirmBox('Aveți modificări nesalvate. Dacă adăugați un rând nou, ele se pierd.', { title: 'Modificări nesalvate', yes: 'Renunț la modificări', no: 'Rămân aici', warning: true }))) return;
    k_state.mode = 'add'; k_state.selected = null; k_state.dirty = false; k_state.key = null; say(''); paintAll();
    k_inputs.values().next().value?.focus();
  }, k_signal);
  k_btnEdit.addEventListener('click', () => { k_state.mode = 'edit'; k_state.dirty = false; say(''); paintForm(); k_inputs.values().next().value?.focus(); }, k_signal);
  k_btnCancel.addEventListener('click', () => { k_state.mode = 'view'; k_state.dirty = false; k_state.key = null; say(''); paintAll(); }, k_signal);
  k_btnToggle.addEventListener('click', () => toggle().catch((k_e) => failed(k_e)), k_signal);
  k_form.addEventListener('submit', (k_e) => { save(k_e).catch((k_error) => failed(k_error)); }, k_signal);

  return {
    load, isDirty: () => k_state.dirty, selected: selectedRow,
    destroy: () => { k_abort.abort(); k_host.replaceChildren(); },
  };
}

// Products carry price variants (the price is per variant and may be changed at any time; saved document lines keep
// their own values). The variant list follows the product chosen in the tree.
export function mountNomenclator(k_host, k_opts) {
  const { api, sel, kinds, signal } = k_opts;
  const k_titles = { categorii: 'Categorii / Conturi', produse: 'Produse', variante: 'Variante de preț', clienti: 'Clienți / Gestionari', tva: 'Cote TVA', comisie_nir: 'Comisie de recepție', comisie_inventar: 'Comisie de inventariere', feluri: 'Feluri de mâncare' };
  const k_main = el('div'); const k_sub = el('div', { className: 'vc-nom__sub' });
  k_sub.hidden = kinds.length > 1;   // the variants appear once a product is chosen
  k_host.replaceChildren(k_main, k_sub);
  let k_variants = null;
  const k_crud = mountCrud(k_main, {
    api, sel, signal, kind: kinds[0], title: k_titles[kinds[0]] || kinds[0],
    onSelect: (k_row) => { k_variants?.load(false).catch((k_e) => console.error('[vercon]', k_e)); k_sub.hidden = !k_row && kinds.length > 1; },
  });
  if (kinds[1]) {
    k_variants = mountCrud(k_sub, { api, sel, signal, kind: kinds[1], title: k_titles[kinds[1]], parent: () => k_crud.selected()?.id ?? null });
  }
  const k_all = [k_crud, k_variants].filter(Boolean);
  return {
    ready: Promise.all(k_all.map((k_c) => k_c.load(false))),
    isDirty: () => k_all.some((k_c) => k_c.isDirty()),
    destroy: () => { k_all.forEach((k_c) => k_c.destroy()); k_host.replaceChildren(); },
  };
}

// Gestiune settings: displayed decimals and storage scale (editable only while the gestiune holds no data), the type
// is on the page header; the stored counters are shown read-only.
export function mountSettings(k_host, k_opts) {
  const { api, sel } = k_opts;
  const k_abort = new AbortController();
  const k_say = el('span', { className: 'vc-save-status', role: 'status' });
  const k_zec = el('input', { type: 'text', inputMode: 'numeric', 'aria-label': 'Zecimale afișate' });
  const k_scara = el('input', { type: 'text', inputMode: 'numeric', 'aria-label': 'Scara de stocare' });
  const k_msgZec = el('div', { className: 'vc-field-error' }); const k_msgScara = el('div', { className: 'vc-field-error' });
  const k_note = el('p', { className: 'vc-nom__empty' });
  const k_save = el('button', { type: 'submit', textContent: 'Salvează' });
  const k_counters = el('table', { className: 'vc-lines' });
  const k_form = el('form', { noValidate: true },
    el('div', { className: 'vc-grid' }, el('label', {}, 'Zecimale afișate (0-4) *', k_zec, k_msgZec), el('label', {}, 'Scara de stocare (zecimale ≤ 4) *', k_scara, k_msgScara)),
    k_note, el('div', { className: 'vc-actions' }, k_save, k_say), k_counters);
  k_host.replaceChildren(el('section', { className: 'vc-nom' }, el('div', { className: 'vc-nom__bar' }, el('h3', { textContent: 'Setări gestiune' })), k_form));
  let k_dirty = false;
  const q = () => new URLSearchParams(Object.entries(sel()).filter(([, k_v]) => k_v != null && k_v !== '').map(([k_k, k_v]) => [k_k, String(k_v)]));
  const say = (k_t, k_bad) => { k_say.textContent = k_t || ''; k_say.style.color = k_bad ? '#c0392b' : ''; };

  async function load() {
    const k_d = await api(`/api/vercon/settings?${q()}`);
    k_zec.value = String(k_d.zec); k_scara.value = String(k_d.scara);
    const k_edit = k_d.can_write && !k_d.populated;
    k_zec.readOnly = k_scara.readOnly = !k_edit; k_save.hidden = !k_edit;
    k_note.textContent = k_d.populated ? 'Gestiunea conține date: zecimalele și scara nu se mai pot schimba.' : 'Valorile se pot schimba doar cât timp gestiunea nu are date.';
    k_counters.replaceChildren(el('thead', {}, el('tr', {}, el('th', { textContent: 'Setare' }), el('th', { textContent: 'Valoare' }))),
      el('tbody', {}, ...k_d.settings.map((k_s) => el('tr', {}, el('td', { textContent: k_s.key }), el('td', { textContent: String(k_s.value) })))));
    k_counters.hidden = !k_d.settings.length; k_dirty = false;
  }
  k_form.addEventListener('input', () => { k_dirty = true; }, { signal: k_abort.signal });
  k_form.addEventListener('submit', async (k_e) => {
    k_e.preventDefault(); k_msgZec.textContent = k_msgScara.textContent = '';
    const k_z = parse(k_zec.value, { type: 'int', required: true, min: 0 }); const k_s = parse(k_scara.value, { type: 'int', required: true, min: 0 });
    if (!k_z.ok || !k_s.ok) { k_msgZec.textContent = k_z.ok ? '' : k_z.message; k_msgScara.textContent = k_s.ok ? '' : k_s.message; say('Corectați câmpurile marcate.', true); return; }
    k_save.disabled = true; say('Se salvează…');
    const k_sel = sel();
    try {
      await api('/api/vercon/settings', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ subunit: k_sel.subunit, gestiune: k_sel.gestiune, zec: k_z.value, scara: k_s.value }) });
      await load(); say('Salvat. Reîncărcați pagina pentru ca noua scară să se aplice peste tot.');
    } catch (k_error) {
      if (k_error.name === 'AbortError') return;
      if (k_error.fields) { k_msgZec.textContent = k_error.fields.zec || ''; k_msgScara.textContent = k_error.fields.scara || ''; }
      say(k_error.message || 'Salvarea a eșuat.', true);
    } finally { k_save.disabled = false; }
  }, { signal: k_abort.signal });
  return { ready: load(), isDirty: () => k_dirty, destroy: () => { k_abort.abort(); k_host.replaceChildren(); } };
}
