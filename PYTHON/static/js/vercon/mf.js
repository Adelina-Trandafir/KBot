// slice VC07-04: the fixed-asset register (MF gestiune). One row per asset: name, inventory number, entry date, value, normal
// and used duration (months), monthly value, who holds it, how many were written off. The register data (entry date,
// durations) is edited in the row and saved with the revision the page saw; the monthly value is recomputed by the server
// from what the receipts say the asset cost. The identity of an asset never changes here.
import { format, parse } from './fields.js';

const el = (k_tag, k_attrs = {}, ...k_children) => {
  const k_node = Object.assign(document.createElement(k_tag), k_attrs);
  k_node.append(...k_children);
  return k_node;
};
const dateText = (k_iso) => (k_iso ? format(k_iso, { type: 'date' }) : '');

export function mountMf(k_host, k_opts) {
  const k_abort = new AbortController();
  const k_signal = { signal: k_abort.signal };
  const k_state = { assets: [], canWrite: false, filter: '', editing: null, dirty: false };
  const k_say = el('span', { className: 'vc-save-status', role: 'status' });
  const say = (k_t, k_bad) => { k_say.textContent = k_t || ''; k_say.style.color = k_bad ? '#c0392b' : ''; };
  const query = () => new URLSearchParams(Object.entries(k_opts.sel()).filter(([, k_v]) => k_v != null && k_v !== '').map(([k_k, k_v]) => [k_k, String(k_v)]));
  const search = el('input', { type: 'search', placeholder: 'Caută mijloc fix…', 'aria-label': 'Caută' });
  const k_table = el('div', { className: 'vc-doc__lines' });
  k_host.replaceChildren(el('section', { className: 'vc-nom' }, el('div', { className: 'vc-nom__bar' }, el('h3', { textContent: 'Evidență mijloace fixe' }), search), k_table, el('div', { className: 'vc-actions' }, k_say)));

  async function load() {
    const k_d = await k_opts.api(`/api/vercon/mf/assets?${query()}`);
    k_state.assets = k_d.assets; k_state.canWrite = k_d.can_write; paint();
  }

  function paint() {
    const k_needle = k_state.filter.trim().toLowerCase();
    const k_rows = k_state.assets.filter((k_a) => !k_needle || `${k_a.name} ${k_a.code}`.toLowerCase().includes(k_needle));
    const k_t = el('table', { className: 'vc-lines vc-lines--doc' });
    k_t.append(el('thead', {}, el('tr', {}, ...['Mijloc fix', 'Nr. inventar', 'Data intrării', 'Valoare (lei)', 'Durată normală (luni)', 'Durată folosită (luni)', 'Valoare lunară', 'Deținător', 'Casat', ...(k_state.canWrite ? [''] : [])].map((k_h) => el('th', { textContent: k_h })))));
    const k_b = el('tbody');
    if (!k_rows.length) k_b.append(el('tr', {}, el('td', { colSpan: 10, textContent: 'Nu există mijloace fixe.' })));
    for (const k_a of k_rows) k_b.append(k_state.editing === k_a.id ? editRow(k_a) : viewRow(k_a));
    k_t.append(k_b); k_table.replaceChildren(k_t);
  }

  function viewRow(k_a) {
    const k_tr = el('tr', {}, el('td', { textContent: k_a.name || '' }), el('td', { textContent: k_a.code || '' }), el('td', { textContent: dateText(k_a.entry_date) }),
      el('td', { className: 'is-num', textContent: format(k_a.value, { type: 'decimal' }) }), el('td', { className: 'is-num', textContent: k_a.DNF ?? '' }), el('td', { className: 'is-num', textContent: k_a.DF ?? '' }),
      el('td', { className: 'is-num', textContent: format(k_a.monthly, { type: 'decimal' }) }), el('td', { textContent: k_a.holders.map((k_h) => `${k_h.holder} (${format(k_h.qty, { type: 'decimal' })})`).join(', ') }),
      el('td', { className: 'is-num', textContent: Number(k_a.written_off) ? format(k_a.written_off, { type: 'decimal' }) : '' }));
    if (k_state.canWrite) { const k_e = el('button', { type: 'button', textContent: 'Modifică' }); k_e.addEventListener('click', () => { if (k_state.editing != null && k_state.dirty) { say('Salvați sau renunțați mai întâi la modificarea începută.', true); return; } k_state.editing = k_a.id; k_state.dirty = false; say(''); paint(); }, k_signal); k_tr.append(el('td', {}, k_e)); }
    return k_tr;
  }

  function editRow(k_a) {
    const k_d = el('input', { type: 'text', value: dateText(k_a.entry_date), 'aria-label': 'Data intrării' });
    const k_dnf = el('input', { type: 'text', value: k_a.DNF ?? '', inputMode: 'numeric', 'aria-label': 'Durată normală' });
    const k_df = el('input', { type: 'text', value: k_a.DF ?? '', inputMode: 'numeric', 'aria-label': 'Durată folosită' });
    for (const k_i of [k_d, k_dnf, k_df]) k_i.addEventListener('input', () => { k_state.dirty = true; }, k_signal);
    const k_err = el('div', { className: 'vc-field-error' });
    const k_save = el('button', { type: 'button', textContent: 'Salvează', className: 'is-primary' });
    const k_no = el('button', { type: 'button', textContent: 'Renunță' });
    k_no.addEventListener('click', () => { k_state.editing = null; k_state.dirty = false; say(''); paint(); }, k_signal);
    k_save.addEventListener('click', async () => {
      const k_pd = parse(k_d.value, { type: 'date', required: true }), k_pn = parse(k_dnf.value, { type: 'int', required: true, min: 1 }), k_pf = parse(k_df.value, { type: 'int', required: false, min: 0 });
      const k_bad = [k_pd, k_pn, k_pf].find((k_p) => !k_p.ok);
      if (k_bad) { k_err.textContent = k_bad.message; return; }
      k_save.disabled = true; say('Se salvează…');
      const k_sel = k_opts.sel();
      try {
        await k_opts.api(`/api/vercon/mf/assets/${k_a.id}`, { method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ subunit: k_sel.subunit, gestiune: k_sel.gestiune, year: k_sel.year, version: k_a.version, DataIntrare: k_pd.value, DNF: k_pn.value, DF: k_pf.value ?? 0 }) });
        k_state.editing = null; k_state.dirty = false; await load(); say('Salvat.');
      } catch (k_error) {
        if (k_error.name === 'AbortError') return;
        k_save.disabled = false;
        if (k_error.fields) k_err.textContent = Object.values(k_error.fields).join(' ');
        if (k_error.reason === 'CONFLICT') { k_state.editing = null; k_state.dirty = false; await load(); }
        say(k_error.message || 'Salvarea a eșuat.', true);
      }
    }, k_signal);
    return el('tr', {}, el('td', { textContent: k_a.name || '' }), el('td', { textContent: k_a.code || '' }), el('td', {}, k_d), el('td', { className: 'is-num', textContent: format(k_a.value, { type: 'decimal' }) }),
      el('td', {}, k_dnf), el('td', {}, k_df), el('td', { className: 'is-num', textContent: format(k_a.monthly, { type: 'decimal' }) }), el('td', { textContent: '' }), el('td', {}), el('td', {}, k_save, k_no, k_err));
  }

  search.addEventListener('input', () => { k_state.filter = search.value; paint(); }, k_signal);
  load().catch((k_e) => { if (k_e.name !== 'AbortError') say(k_e.message || 'Evidența nu a putut fi încărcată.', true); });
  return { ready: Promise.resolve(), isDirty: () => k_state.dirty, destroy: () => { k_abort.abort(); k_host.replaceChildren(); } };
}
