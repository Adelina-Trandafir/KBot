// SLICE-VC16-03: «Modifica furnizor» of the saved-NIR tree. Only the supplier changes, in any period (also in a closed exercise);
// the server decides and its refusal stays in the box, so the operator can pick another supplier or give up.
import { ensureStyle } from '../utils/confirm-box.js';
import { Combobox } from './ui.js';
import { searchOptions } from './search-settings.js';

const norm = text => String(text ?? '').toLocaleLowerCase('ro');

// `suppliers` = [{ id (fiscal code), name, cf }]; `post(cf, version, request_key)` calls the server and resolves with { result, document }.
// Resolves with that answer, or null when the operator gave up.
export function changeDocumentSupplier({ doc, title, suppliers, post, key }) {
  ensureStyle();
  const items = suppliers.map(item => ({ value: String(item.cf ?? item.id), label: `${item.name} • ${item.cf ?? item.id}` }));
  return new Promise(resolve => {
    const dialog = document.createElement('dialog');
    dialog.className = 'confirm-box confirm-box--supplier';
    dialog.setAttribute('aria-label', 'Modificare furnizor');
    const header = document.createElement('header');
    const heading = document.createElement('h2'); heading.textContent = 'Modificare furnizor'; header.append(heading);
    const text = document.createElement('p'); text.className = 'confirm-box__text';
    text.textContent = `${title} ${doc.number}. Se schimbă numai furnizorul; funcționează și într-o perioadă închisă. Furnizorul actual: ${doc.header?.party || '—'}.`;
    const field = document.createElement('label'); field.className = 'confirm-box__field'; field.textContent = 'Furnizor nou';
    const host = document.createElement('div'); field.append(host);
    const error = document.createElement('p'); error.className = 'confirm-box__error'; error.setAttribute('role', 'alert'); error.hidden = true;
    const footer = document.createElement('footer');
    let busy = false, chosen = '';
    const current = String(doc.header?.CodFiscal ?? '');
    const finish = answer => { combo.destroy(); dialog.close(); dialog.remove(); resolve(answer); };
    const no = document.createElement('button'); no.type = 'button'; no.textContent = 'Renunță';
    no.addEventListener('click', () => { if (!busy) finish(null); });
    const yes = document.createElement('button'); yes.type = 'button'; yes.textContent = 'Salvează'; yes.disabled = true;
    const valid = () => !!chosen && chosen !== current;
    const combo = new Combobox(host, { placeholder: 'Alegeți furnizorul…', dropDownStyle: 'simple', maxResults: 200, dropDownMinWidth: 440, ...searchOptions(),
      onSearch: async query => items.filter(item => norm(item.label).includes(norm(query))),
      onSelect: value => { chosen = String(value); error.hidden = true; yes.disabled = busy || !valid(); } });
    combo.input.setAttribute('aria-label', 'Furnizor nou');
    combo.input.addEventListener('input', () => { chosen = ''; error.hidden = true; yes.disabled = true; });
    const submit = async () => {
      if (busy || !valid()) return;
      busy = true; yes.disabled = true; error.hidden = true;
      try { finish(await post(chosen, doc.version, key())); return; }
      catch (failure) { error.textContent = failure?.message || 'Furnizorul nu a putut fi salvat.'; error.hidden = false; }
      busy = false; yes.disabled = !valid(); combo.input.focus();
    };
    yes.addEventListener('click', submit);
    footer.append(no, yes);
    dialog.addEventListener('cancel', event => { event.preventDefault(); if (!busy) finish(null); });
    dialog.addEventListener('mousedown', event => { if (event.target === dialog && !busy) finish(null); });
    dialog.append(header, text, field, error, footer); document.body.append(dialog); dialog.showModal();
    combo.input.focus();
  });
}
