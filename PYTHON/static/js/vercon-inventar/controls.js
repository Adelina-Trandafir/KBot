import { Combobox } from '../components/combobox/combobox.js';
import { TextBox } from '../components/textbox/textbox.js';
import { confirmBox } from '../utils/confirm-box.js';
import { state } from './state.js';
import { notify } from '../vercon/messages.js';

export const $ = id => document.getElementById(id);
const names = (a, b) => a.name.localeCompare(b.name, 'ro', { sensitivity: 'base' });
export function combo(id, onSelect, options = {}) {
  let items = [];
  const box = new Combobox($(id), { dropDownStyle: 'simple', placeholder: 'Alegeți…', maxResults: 100000, ...options,
    onSearch: async query => items.filter(r => r.name.toLocaleLowerCase('ro').includes(query.toLocaleLowerCase('ro')))
      .map(r => ({ value: String(r.id), label: r.label || r.name })),
    onSelect: value => onSelect(String(value)) });
  return { box, set(list, value) { items = [...list].sort(names); const row = items.find(r => String(r.id) === String(value));
    box.setValue(row ? String(row.id) : '', row?.label || row?.name || ''); },
  enable: on => box.setEnabled(on), destroy: () => box.destroy() };
}
export const input = (id, opts) => new TextBox($(id), { allowCopyPaste: true, ...opts });
export function say(text, kind = '') {
  if (kind === 'error') { notify(text, 'error'); return; }
  $('vi-status').textContent = text;
  $('vi-status').className = 'vi-status' + (kind ? ` is-${kind}` : '');
}
export function reportReadErrors(errors) {
  if (!errors?.length) return;
  const value = raw => raw == null || String(raw).trim() === '' ? 'lipsă' : String(raw);
  const lines = errors.map(error => `Rând Excel ${error.row ?? 'necunoscut'} (NrCrt ${error.serial ?? 'necunoscut'}): ${error.message}\n` +
    `Denumire: ${value(error.name)}\nUM: ${value(error.um)}\nPreț: ${value(error.price)}`);
  notify(`${errors.length} poziții cu erori au fost sărite. Citirea celorlalte poziții a continuat.\n\n${lines.join('\n\n')}`,
    'error', { title: 'Erori la citirea Excelului' });
}
export async function discard() {
  if (state.uncertain) throw new Error('Mai întâi reluați «Trimite în VERCON» pentru a afla rezultatul lotului deja trimis.');
  return !state.assignments.length || state.sent || await confirmBox('Asocierile curente nu au fost trimise. Doriți să le eliminați?', { yes: 'Elimină asocierile', no: 'Păstrează' });
}

