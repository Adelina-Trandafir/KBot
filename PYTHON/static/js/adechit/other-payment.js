// Manual other-payment entry uses the common message journal and blocking dialog.
import { showMessage } from '../portal/messages.js';
import { messageBox } from '../utils/confirm-box.js';

export function otherPaymentNumber() {
  const input = document.getElementById('ade-ledger-number');
  const value = input?.value.trim() || '';
  const label = value.normalize('NFD').replace(/\p{M}/gu, '').toLowerCase().replace(/\s+/g, ' ');
  if (!value || label === 'fara numar') {
    throw Object.assign(new Error('Completați numărul documentului. Salvarea fără număr nu este permisă.'),
      { reason: 'DOCUMENT_NUMBER_REQUIRED' });
  }
  return value;
}

export function reportOtherPaymentError(error) {
  const element = document.getElementById('ade-message');
  showMessage(element, error.message, 'error', { tooltip: false });
  const notice = messageBox(error.message, { title: 'Documentul nu poate fi salvat', warning: true });
  notice.closed.then(() => {
    if (['DOCUMENT_NUMBER_REQUIRED', 'DOCUMENT_NUMBER_USED'].includes(error.reason)) {
      document.getElementById('ade-ledger-number')?.focus();
    }
  });
}
