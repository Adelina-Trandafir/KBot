// SLICE-VC16-04: NIR summary and number entry; receipt_number validates on the server.
import { ensureStyle } from '../utils/confirm-box.js';
import { numberText } from './receipt-number.js';
import { format } from './fields.js';

export function changeReceiptNumber({ doc, post, propose, key, isCurrent }) {
  ensureStyle();
  return new Promise(resolve => {
    const dialog = document.createElement('dialog'); dialog.className = 'confirm-box vc-nir-number-change';
    dialog.setAttribute('aria-label', 'Schimbă număr NIR');
    const header = document.createElement('header');
    const heading = document.createElement('h2'); heading.textContent = 'Schimbă număr NIR'; header.append(heading);
    const info = document.createElement('p'); info.className = 'confirm-box__text';
    const date = value => format(value, { type: 'date' }) || '—';
    info.textContent = `NIR ${numberText(doc)} din ${date(doc.header.Data)}\n${doc.header.party || '—'}\nFactura ${doc.header.NrFact || '—'} din ${date(doc.header.DataFc)}`;
    const field = document.createElement('div'); field.className = 'confirm-box__field';
    const label = document.createElement('label'); label.textContent = 'Număr NIR nou';
    const row = document.createElement('div'); row.className = 'vc-nir-number-change__row';
    const input = document.createElement('input'); input.type = 'text'; input.inputMode = 'decimal';
    input.id = `vc-nir-number-${key()}`; label.htmlFor = input.id;
    const proposeButton = document.createElement('button'); proposeButton.type = 'button'; proposeButton.textContent = 'Propune Număr';
    input.value = numberText(doc); row.append(input, proposeButton); field.append(label, row);
    const error = document.createElement('p'); error.className = 'confirm-box__error'; error.setAttribute('role', 'alert'); error.hidden = true;
    const footer = document.createElement('footer');
    const no = document.createElement('button'); no.type = 'button'; no.textContent = 'Renunță';
    const yes = document.createElement('button'); yes.type = 'button'; yes.textContent = 'Salvează'; yes.disabled = true;
    let busy = false, finished = false;
    let attempt = null;
    const old = String(numberText(doc));
    const changed = () => input.value.trim() !== '' && input.value.trim().replace('.', ',') !== old;
    const setBusy = value => {
      busy = value; input.disabled = value; proposeButton.disabled = value;
      no.disabled = value; yes.disabled = value || !changed();
    };
    const finish = answer => { if (finished) return; finished = true; dialog.close(); dialog.remove(); resolve(answer); };
    const suggest = async () => {
      if (busy) return;
      if (!isCurrent()) { finish(null); return; }
      setBusy(true); error.hidden = true;
      try {
        const answer = await propose();
        if (!isCurrent()) { finish(null); return; }
        input.value = numberText({ number: answer.suggested, sub_number: answer.suggested_sub });
      } catch (failure) {
        if (!isCurrent()) { finish(null); return; }
        error.textContent = failure?.message || 'Propunerea nu a putut fi obținută.'; error.hidden = false;
      }
      setBusy(false); input.focus(); input.select();
    };
    const submit = async () => {
      if (busy || !changed()) return;
      if (!isCurrent()) { finish(null); return; }
      setBusy(true); error.hidden = true;
      // A retry of the same value keeps its idempotency key after a lost response.
      const value = input.value.trim();
      if (attempt?.value !== value) attempt = { value, key: key() };
      try { finish((await post(input.value.trim(), doc.version, attempt.key)).document); return; }
      catch (failure) {
        if (!isCurrent()) { finish(null); return; }
        error.textContent = failure?.message || 'Numărul nu a putut fi salvat.'; error.hidden = false;
      }
      setBusy(false); input.focus(); input.select();
    };
    no.addEventListener('click', () => { if (!busy) finish(null); });
    yes.addEventListener('click', submit);
    proposeButton.addEventListener('click', suggest);
    input.addEventListener('input', () => { error.hidden = true; yes.disabled = !changed(); });
    input.addEventListener('keydown', event => { if (event.key === 'Enter') { event.preventDefault(); submit(); } });
    dialog.addEventListener('cancel', event => { event.preventDefault(); if (!busy) finish(null); });
    dialog.addEventListener('mousedown', event => { if (event.target === dialog && !busy) finish(null); });
    footer.append(no, yes); dialog.append(header, info, field, error, footer);
    document.body.append(dialog); dialog.showModal();
    input.focus(); input.select();
  });
}
