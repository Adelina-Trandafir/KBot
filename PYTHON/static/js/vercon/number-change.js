// SLICE-VC16-02: «Modifica numar» of the saved-documents tree (BC, BP, BT, BS). Only the number changes; the server decides (current month,
// numbers in the order of the dates, number free in its series, per account at BC when the gestiune numbers that way) and its refusal
// stays in the box, so the operator can try another number.
import { numberBox } from '../utils/confirm-box.js';

const digits = text => String(text ?? '').replace(/\D+/g, '');

// `post(number, version, request_key)` calls the server; resolves with the saved document, or null when the operator gave up.
export function changeDocumentNumber({ doc, title, post, key }) {
  return numberBox(`${title} ${doc.number}. Se schimbă numai numărul. Numerele trebuie să urmeze ordinea datelor, iar numărul trebuie să fie liber.`,
    { title: 'Modificare număr', label: 'Număr nou', value: digits(doc.number), yes: 'Salvează', no: 'Renunță',
      save: async number => (await post(number, doc.version, key())).document });
}
