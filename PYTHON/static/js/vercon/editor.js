// slice VC02-03/04: header + lines editor for a document form, bound to a draft that lives outside the DOM.
// Every input writes its raw text into the draft on `input`; on `change` the text is parsed (decimal comma, local
// date, NULL vs 0) and shown back normalised. Repainting the lines from the draft by stable row key keeps what was
// typed, so a repaint (or later a virtualised list) never loses an edit.
import { format, parse } from './fields.js';

const el = (k_tag, k_attrs = {}, ...k_children) => {
  const k_node = Object.assign(document.createElement(k_tag), k_attrs);
  k_node.append(...k_children);
  return k_node;
};

// opts: { draft, columns, saver, saveUrl, getRevision, onSaved, report }  -> { destroy }
export function mountEditor(k_host, k_opts) {
  const { draft, columns, saver } = k_opts;
  const k_abort = new AbortController();
  const k_signal = { signal: k_abort.signal };
  const k_status = el('span', { className: 'vc-save-status', role: 'status' });
  const k_errors = new Map();    // "where|field" -> message element

  const k_input = (k_col, k_value, k_write, k_errorKey) => {
    const k_box = el('input', { type: 'text', value: k_value ?? '', name: k_col.key, autocomplete: 'off' });
    if (k_col.type === 'decimal' || k_col.type === 'int') k_box.inputMode = 'decimal';
    k_box.setAttribute('aria-label', k_col.label);
    const k_msg = el('div', { className: 'vc-field-error' });
    k_errors.set(k_errorKey, k_msg);
    k_box.addEventListener('input', () => k_write(k_box.value), k_signal);
    k_box.addEventListener('change', () => {
      const k_r = parse(k_box.value, k_col);
      k_msg.textContent = k_r.ok ? '' : k_r.message;
      if (k_r.ok) { k_box.value = format(k_r.value, k_col); k_write(k_box.value); }
    }, k_signal);
    return { k_box, k_msg };
  };

  const k_headerGrid = el('div', { className: 'vc-grid' });
  for (const k_col of columns.header) {
    const { k_box, k_msg } = k_input(k_col, draft.header()[k_col.key] ?? null, (k_t) => draft.setHeader(k_col.key, k_t), `header|${k_col.key}`);
    k_headerGrid.append(el('label', {}, `${k_col.label}${k_col.required ? ' *' : ''}`, k_box, k_msg));
  }
  const k_table = el('table', { className: 'vc-lines' });
  const k_body = el('tbody');
  k_table.append(el('thead', {}, el('tr', {}, ...columns.lines.map((k_c) => el('th', { textContent: k_c.label })), el('th'))), k_body);

  function paintLines() {
    k_body.replaceChildren();
    for (const k_row of draft.rows()) {
      const k_tr = el('tr', {}); k_tr.dataset.rowKey = k_row.key;
      for (const k_col of columns.lines) {
        const { k_box, k_msg } = k_input(k_col, k_row.cells[k_col.key] ?? null, (k_t) => draft.setCell(k_row.key, k_col.key, k_t), `${k_row.key}|${k_col.key}`);
        k_tr.append(el('td', {}, k_box, k_msg));
      }
      const k_del = el('button', { type: 'button', textContent: '✕', title: 'Șterge linia' });
      k_del.setAttribute('aria-label', 'Șterge linia');
      k_del.addEventListener('click', () => { draft.removeRow(k_row.key); paintLines(); }, k_signal);
      k_tr.append(el('td', {}, k_del));
      k_body.append(k_tr);
    }
  }
  const k_add = el('button', { type: 'button', textContent: '+ Linie' });
  k_add.addEventListener('click', () => { draft.addRow(); paintLines(); k_body.lastElementChild?.querySelector('input')?.focus(); }, k_signal);
  const k_save = el('button', { type: 'submit', textContent: 'Salvează' });
  const k_reconcile = el('button', { type: 'button', textContent: 'Verifică salvarea', hidden: true });
  const k_form = el('form', { noValidate: true }, k_headerGrid, k_table, el('div', { className: 'vc-actions' }, k_add, k_save, k_reconcile, k_status));

  const k_say = (k_text, k_bad) => { k_status.textContent = k_text; k_status.style.color = k_bad ? '#c0392b' : ''; };

  k_form.addEventListener('submit', async (k_event) => {
    k_event.preventDefault();
    for (const k_m of k_errors.values()) k_m.textContent = '';
    const k_data = draft.collect(columns, parse);
    if (k_data.errors) {
      for (const k_e of k_data.errors) { const k_m = k_errors.get(`${k_e.where}|${k_e.field}`); if (k_m) k_m.textContent = k_e.message; }
      k_say('Corectați câmpurile marcate.', true); return;
    }
    if (!k_opts.saveUrl) { k_say('Salvarea documentelor vine cu SLICE-VC06.', true); return; }
    k_save.disabled = true; k_say('Se salvează…');
    try {
      const k_r = await saver.save({ form: k_opts.form, subunit: draft.context.subunit, gestiune: draft.context.gestiune, base_revision: k_opts.getRevision(), header: k_data.header, lines: k_data.lines },
        k_opts.simulate ? { 'X-Preview-Simulate': k_opts.simulate() } : null);
      if (k_r.outcome === 'saved') { draft.markSaved(); k_reconcile.hidden = true; k_opts.onSaved(k_r.revision); k_say(k_r.replayed ? 'Salvat (cererea fusese deja aplicată).' : 'Salvat.'); }
      else if (k_r.outcome === 'conflict') { k_opts.onSaved(k_r.revision, true); k_say(`${k_r.message} Datele dvs. au rămas în formular; reîncărcați documentul înainte de a salva din nou.`, true); }
      else if (k_r.outcome === 'unknown') { k_reconcile.hidden = false; k_say(k_r.message, true); }
      else if (k_r.outcome === 'expired') { k_say(k_r.message, true); }
      else if (k_r.outcome === 'stale') { k_say('Contextul s-a schimbat în timpul salvării; răspunsul a fost ignorat.', true); }
      else k_say(k_r.message, true);
    } catch (k_error) {
      if (k_error.name !== 'AbortError') { console.error('[vercon] save failed', k_error); k_say('Salvarea nu a putut fi trimisă.', true); }
    } finally { k_save.disabled = false; }
  }, k_signal);

  k_reconcile.addEventListener('click', async () => {
    const k_r = await saver.reconcile();
    if (k_r.outcome === 'saved') { draft.markSaved(); k_reconcile.hidden = true; k_opts.onSaved(k_r.revision); k_say('Salvarea fusese aplicată.'); }
    else if (k_r.outcome === 'not-applied') { k_reconcile.hidden = true; k_say('Salvarea nu fusese aplicată; puteți salva din nou.', true); }
    else k_say(k_r.message || 'Nu s-a putut verifica.', true);
  }, k_signal);

  paintLines();
  k_host.replaceChildren(k_form);
  return { destroy: () => { k_abort.abort(); k_host.replaceChildren(); } };
}
