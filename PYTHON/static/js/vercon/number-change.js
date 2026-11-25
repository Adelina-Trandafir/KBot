// Shared number dialog for saved documents. Server validation stays visible in the dialog.
import { numberBox } from '../utils/confirm-box.js';
import { changeReceiptNumber } from './receipt-number-change.js';

const digits = text => String(text ?? '').replace(/\D+/g, '');

// `post(number, version, request_key)` calls the server; resolves with the saved document, or null when the operator gave up.
export function changeDocumentNumber({ doc, title, post, propose, key, isCurrent }) {
  if (doc.family === 'nir') return changeReceiptNumber({ doc, post, propose, key, isCurrent });
  const foodList = doc.family === 'lista';
  const value = foodList && !(doc.number > 0) ? '' : digits(doc.number);
  return numberBox(`${title} ${doc.number ?? '—'}. Se schimbă numai numărul. Numerele trebuie să urmeze ordinea datelor, iar numărul trebuie să fie liber.`,
    { title: 'Modificare număr', label: 'Număr nou', value, maxLength: foodList ? 10 : 9,
      save: async number => (await post(number, doc.version, key())).document });
}
