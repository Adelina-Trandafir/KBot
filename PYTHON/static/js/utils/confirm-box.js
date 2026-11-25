// The application's one confirmation box (replaces window.confirm everywhere): the page behind it is blurred and
// the question sits in the middle. «No» / «Renunta» is on the left, «Yes» / «OK» on the right.
// `warning: true` paints the box as a serious warning (red header). Escape and the backdrop answer No. Usage: if (!await confirmBox('Intrebare?', { title, yes, no })) return;
import { DialogButton } from '../components/dialog-button/dialog-button.js';

// VC21-01: the buttons are the shared dialog buttons: «No» = Cancel (left), «Yes» = OK1 (right), a message's extra action = OK2.
const dialogButton = (k_kind, k_label, k_className = '') => new DialogButton(null, { kind: k_kind, label: k_label, className: k_className }).element;

const styledDocuments = new WeakSet();
export function ensureStyle(doc = document) {
  if (styledDocuments.has(doc)) return; styledDocuments.add(doc);
  for (const k_href of ['/static/js/components/dialog-button/dialog-button.css', '/static/css/confirm-box.css']) {
    const k_link = doc.createElement('link'); k_link.rel = 'stylesheet'; k_link.href = k_href;
    doc.head.append(k_link);
  }
}

// Notice with an optional left-hand action, using the same custom dialog as confirmations.
export function messageBox(text, { title = 'Mesaj', button = 'Am înțeles', action = null, warning = false, className = '', ownerDocument = document } = {}) {
  ensureStyle(ownerDocument);
  const dialog = ownerDocument.createElement('dialog');
  dialog.className = `confirm-box confirm-box--message${warning ? ' confirm-box--warning' : ''}${className ? ` ${className}` : ''}`;
  dialog.setAttribute('aria-label', title);
  const header = ownerDocument.createElement('header'), heading = ownerDocument.createElement('h2');
  heading.textContent = title; header.append(heading);
  const body = ownerDocument.createElement('p'); body.className = 'confirm-box__text'; body.textContent = text;
  const footer = ownerDocument.createElement('footer'), acknowledge = dialogButton('ok1', button);
  footer.append(acknowledge);
  let finish, done = false;
  const closed = new Promise(resolve => { finish = resolve; });
  const close = () => { if (done) return; done = true; if (dialog.open) dialog.close(); dialog.remove(); finish(); };
  if (action) {
    const actionButton = dialogButton('ok2', action.label, 'confirm-box__action');
    actionButton.addEventListener('click', () => { if (done) return; close(); action.run(); });
    footer.prepend(actionButton);
  }
  acknowledge.addEventListener('click', close);
  dialog.addEventListener('cancel', event => { event.preventDefault(); close(); });
  dialog.addEventListener('close', close);
  dialog.append(header, body, footer); ownerDocument.body.append(dialog); dialog.showModal(); acknowledge.focus();
  return { closed, close, setText: value => { body.textContent = value; } };
}

export function confirmBox(k_text, { title = 'Confirmare', yes = 'Da', no = 'Nu', warning = false } = {}) {
  ensureStyle();
  return new Promise((k_resolve) => {
    const k_dialog = document.createElement('dialog');
    k_dialog.className = warning ? 'confirm-box confirm-box--warning' : 'confirm-box';
    k_dialog.setAttribute('aria-label', title);
    const k_header = document.createElement('header');
    const k_title = document.createElement('h2'); k_title.textContent = title; k_header.append(k_title);
    const k_body = document.createElement('p'); k_body.className = 'confirm-box__text'; k_body.textContent = k_text;
    const k_footer = document.createElement('footer');
    const k_finish = (k_answer) => { k_dialog.close(); k_dialog.remove(); k_resolve(k_answer); };
    for (const [k_kind, k_label, k_answer] of [['cancel', no, false], ['ok1', yes, true]]) {
      const k_button = dialogButton(k_kind, k_label);
      k_button.addEventListener('click', () => k_finish(k_answer)); k_footer.append(k_button);
    }
    k_dialog.addEventListener('cancel', (k_event) => { k_event.preventDefault(); k_finish(false); });
    k_dialog.addEventListener('mousedown', (k_event) => { if (k_event.target === k_dialog) k_finish(false); });
    k_dialog.append(k_header, k_body, k_footer); document.body.append(k_dialog); k_dialog.showModal();
    k_footer.lastElementChild.focus();
  });
}

// A warning box that also asks for a reason: resolves with the trimmed reason, or null on No / Escape / outside click.
// The «Yes» button stays disabled while the reason is empty. Usage: const k_reason = await reasonBox('Intrebare?', { title, label, yes, no });
export function reasonBox(k_text, { title = 'Confirmare', label = 'Motiv', yes = 'Da', no = 'Nu', maxLength = 255 } = {}) {
  ensureStyle();
  return new Promise((k_resolve) => {
    const k_dialog = document.createElement('dialog');
    k_dialog.className = 'confirm-box confirm-box--warning';
    k_dialog.setAttribute('aria-label', title);
    const k_header = document.createElement('header');
    const k_title = document.createElement('h2'); k_title.textContent = title; k_header.append(k_title);
    const k_body = document.createElement('p'); k_body.className = 'confirm-box__text'; k_body.textContent = k_text;
    const k_field = document.createElement('label'); k_field.className = 'confirm-box__field'; k_field.textContent = label;
    const k_input = document.createElement('textarea'); k_input.rows = 3; k_input.maxLength = maxLength; k_input.required = true;
    k_field.append(k_input);
    const k_footer = document.createElement('footer');
    const k_finish = (k_answer) => { k_dialog.close(); k_dialog.remove(); k_resolve(k_answer); };
    const k_no = dialogButton('cancel', no);
    k_no.addEventListener('click', () => k_finish(null));
    const k_yes = dialogButton('ok1', yes); k_yes.disabled = true;
    k_yes.addEventListener('click', () => { const k_reason = k_input.value.trim(); if (k_reason) k_finish(k_reason); });
    k_input.addEventListener('input', () => { k_yes.disabled = !k_input.value.trim(); });
    k_footer.append(k_no, k_yes);
    k_dialog.addEventListener('cancel', (k_event) => { k_event.preventDefault(); k_finish(null); });
    k_dialog.addEventListener('mousedown', (k_event) => { if (k_event.target === k_dialog) k_finish(null); });
    k_dialog.append(k_header, k_body, k_field, k_footer); document.body.append(k_dialog); k_dialog.showModal();
    k_input.focus();
  });
}

// A box that asks for ONE whole number and saves it itself (SLICE-VC16-02, «Modifica numar»). `save(number)` is async and resolves with the
// result; when it throws, the message stays in the box under the field (the operator corrects the number or gives up). Resolves with the
// result of `save`, or null on No / Escape / outside click. «Yes» stays off until the field holds a different positive number than `value`.
export function numberBox(k_text, { title = 'Număr', label = 'Număr nou', value = '', maxLength = 9, yes = 'Salvează', no = 'Renunță', save } = {}) {
  ensureStyle();
  return new Promise((k_resolve) => {
    const k_dialog = document.createElement('dialog');
    k_dialog.className = 'confirm-box';
    k_dialog.setAttribute('aria-label', title);
    const k_header = document.createElement('header');
    const k_title = document.createElement('h2'); k_title.textContent = title; k_header.append(k_title);
    const k_body = document.createElement('p'); k_body.className = 'confirm-box__text'; k_body.textContent = k_text;
    const k_field = document.createElement('label'); k_field.className = 'confirm-box__field'; k_field.textContent = label;
    const k_input = document.createElement('input'); k_input.type = 'text'; k_input.inputMode = 'numeric'; k_input.maxLength = maxLength; k_input.value = String(value);
    k_field.append(k_input);
    const k_error = document.createElement('p'); k_error.className = 'confirm-box__error'; k_error.setAttribute('role', 'alert'); k_error.hidden = true;
    const k_footer = document.createElement('footer');
    let k_busy = false;
    const k_finish = (k_answer) => { k_dialog.close(); k_dialog.remove(); k_resolve(k_answer); };
    const k_no = dialogButton('cancel', no);
    k_no.addEventListener('click', () => { if (!k_busy) k_finish(null); });
    const k_yes = dialogButton('ok1', yes); k_yes.disabled = true;
    const k_valid = () => /^[1-9][0-9]*$/.test(k_input.value.trim()) && k_input.value.trim() !== String(value);
    const k_submit = async () => {
      if (k_busy || !k_valid()) return;
      k_busy = true; k_yes.disabled = true; k_error.hidden = true;
      try { k_finish(await save(Number(k_input.value.trim()))); return; }
      catch (k_failure) { k_error.textContent = k_failure?.message || 'Numărul nu a putut fi salvat.'; k_error.hidden = false; }
      k_busy = false; k_yes.disabled = !k_valid(); k_input.focus(); k_input.select();
    };
    k_yes.addEventListener('click', k_submit);
    k_input.addEventListener('input', () => { k_error.hidden = true; k_yes.disabled = !k_valid(); });
    k_input.addEventListener('keydown', (k_event) => { if (k_event.key === 'Enter') { k_event.preventDefault(); k_submit(); } });
    k_footer.append(k_no, k_yes);
    k_dialog.addEventListener('cancel', (k_event) => { k_event.preventDefault(); if (!k_busy) k_finish(null); });
    k_dialog.addEventListener('mousedown', (k_event) => { if (k_event.target === k_dialog && !k_busy) k_finish(null); });
    k_dialog.append(k_header, k_body, k_field, k_error, k_footer); document.body.append(k_dialog); k_dialog.showModal();
    k_input.focus(); k_input.select();
  });
}

// A box with a list of choices (the phone's group picker): resolves with the chosen value, or null on «Renunta» / Escape / outside click.
export function pickBox(title, items, { no = 'Renunță' } = {}) {
  ensureStyle();
  return new Promise((k_resolve) => {
    const k_dialog = document.createElement('dialog');
    k_dialog.className = 'confirm-box confirm-box--pick';
    k_dialog.setAttribute('aria-label', title);
    const k_header = document.createElement('header');
    const k_title = document.createElement('h2'); k_title.textContent = title; k_header.append(k_title);
    const k_list = document.createElement('div'); k_list.className = 'confirm-box__list';
    const k_footer = document.createElement('footer');
    const k_finish = (k_answer) => { k_dialog.close(); k_dialog.remove(); k_resolve(k_answer); };
    for (const k_item of items) {
      const k_button = document.createElement('button'); k_button.type = 'button'; k_button.textContent = k_item.label;
      k_button.addEventListener('click', () => k_finish(k_item.value)); k_list.append(k_button);
    }
    if (!items.length) { const k_empty = document.createElement('p'); k_empty.className = 'confirm-box__text'; k_empty.textContent = 'Nu există alte grupe.'; k_list.append(k_empty); }
    const k_cancel = dialogButton('cancel', no);
    k_cancel.addEventListener('click', () => k_finish(null)); k_footer.append(k_cancel);
    k_dialog.addEventListener('cancel', (k_event) => { k_event.preventDefault(); k_finish(null); });
    k_dialog.addEventListener('mousedown', (k_event) => { if (k_event.target === k_dialog) k_finish(null); });
    k_dialog.append(k_header, k_list, k_footer); document.body.append(k_dialog); k_dialog.showModal();
  });
}
