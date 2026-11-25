// SLICE-VC20-02: the return bon (BR) edited in place, like the FCS: a typed combo with ONLY the consumption bons that still have something
// to return, and a product list made of the lines of the chosen bon.
import { ComboTree } from '../components/combotree/combotree.js';
import { el } from './ui.js';
import { searchOptions } from './search-settings.js';
import { format } from './fields.js';
import { numberValue } from './receipt-number.js';

export function returnSourceLabel(doc) {
  return `${doc.number} • ${format(doc.date, { type: 'date' })} • ${doc.category || ''} • ${format(doc.total, { type: 'decimal' })} lei`;
}

// Same contract as stornoSourceField: {node, input (hidden, holds the id), error}; the combo is kept on `input._combo`.
export function returnSourceField(owner, sources, value, fallback, editable, onChange, onTab, placeholder = 'Alegeți bonul…') {
  const node = el('label', { className: 'vc-field', 'data-field': 'source' },
    el('span', { textContent: `Bonul returnat${editable ? ' *' : ''}` }));
  const host = el('div'), input = el('input', { type: 'hidden', value: value == null ? '' : String(value) });
  node.append(host, input);
  const receivers = new Map();
  for (const doc of sources) {
    const id = String(doc.receiver_id ?? '');
    if (!receivers.has(id)) receivers.set(id, { id: `receiver:${id}`, label: doc.receiver || 'Fără primitor', children: [] });
    receivers.get(id).children.push({ id: String(doc.id), label: returnSourceLabel(doc),
      sortType: 'date', sortValue: doc.date, sortDirection: 'desc', sortThen: [{ type: 'number', value: numberValue(doc.number) }] });
  }
  const combo = owner.own(new ComboTree(host, { ...searchOptions(), nodes: [...receivers.values()], placeholder,
    minWidth: 480, selectParents: false, onTab,
    onSelect: selection => {
      input.value = String(selection.id);
      error.textContent = '';
      const pending = Promise.resolve(onChange(Number(selection.id)));
      combo._sourceChange = pending;
      pending.catch(() => {}).finally(() => { if (combo._sourceChange === pending) combo._sourceChange = null; });
    } }));
  const selected = sources.find(doc => String(doc.id) === input.value);
  combo.setValue(input.value, selected ? returnSourceLabel(selected) : fallback || '');
  combo.setEnabled(editable);
  combo.arrow.setAttribute('aria-disabled', String(!editable));
  combo.input.setAttribute('aria-label', 'Bonul returnat');
  input._combo = combo;
  const error = { set textContent(message) { node.title = message || ''; combo.input.classList.toggle('is-invalid', !!message); } };
  combo.validate = () => { const ok = !editable || !!input.value; error.textContent = ok ? '' : 'Alegeți bonul returnat.'; return { ok }; };
  Object.defineProperty(combo, 'hasEdit', { get: () => !!combo._sourceChange });
  combo.commitEdit = async () => { try { await combo._sourceChange; return true; } catch { return false; } };
  return { node, input, error };
}

// The products of the chosen bon that still have quantity left; a product already on another line of the return is not offered again.
export function returnProductNodes(eligible, selected = []) {
  const used = new Set(selected.filter(line => line.variant != null).map(line => String(line.variant)));
  return eligible.filter(line => !used.has(String(line.variant)) && Number(String(line.left ?? 0).replace(',', '.')) > 0)
    .map(line => ({ id: String(line.variant), sortType: 'text', sortValue: line.name,
      sortThen: [{ type: 'number', value: line.PUTVA }],
      label: `${line.name} • ${format(line.PUTVA, { type: 'decimal' })} lei • rămas ${format(line.left, { type: 'decimal' })} ${line.um || ''}` }));
}
