// SLICE-VC14-05: «the same invoice number at the same supplier» -- a WARNING through the message system, never a block.
// The NIR / FCS screen asks the server while the invoice number or the supplier is typed and once more before saving; a saved
// document is never refused for this. A warning that cannot be fetched is only logged: it must not stop the work.
import { showMessage } from './ui.js';

const dateText = value => value ? `${value.slice(8, 10)}.${value.slice(5, 7)}.${value.slice(0, 4)}` : '';
const label = twin => `${twin.family === 'storno' ? 'FCS' : 'NIR'} ${twin.number}${twin.sub_text || ''} din ${dateText(twin.date)}`
  + (twin.invoice_date ? ` (data facturii ${dateText(twin.invoice_date)})` : '');

// The text of the warning; `number` is the invoice number as the operator wrote it.
export function twinText(number, twins) {
  if (!twins.length) return '';
  return twins.length === 1
    ? `Mai există o factură cu numărul «${number}» la acest furnizor: ${label(twins[0])}. Verificați să nu fie introdusă de două ori. Puteți continua.`
    : `Mai există ${twins.length} facturi cu numărul «${number}» la acest furnizor: ${twins.map(label).join('; ')}. Verificați să nu fie introduse de două ori. Puteți continua.`;
}

export function invoiceTwins({ family, api, url, state, status, aborted }) {
  const seen = new WeakMap();   // draft -> the supplier/number already warned about, so one value warns once
  // The callers (docs.js) use `twins.warn()`, so the result is an object, like the `{ warn: async () => {} }` of the other families.
  async function warn() {
    const draft = state.draft;
    if (!draft || state.mode === 'view' || draft.tip === 'STOC_INITIAL' || draft.header.FacturaId) return;   // a shared invoice (several NIR on it) is the normal case
    const number = String(draft.header.NrFact ?? '').trim();
    const supplier = family === 'nir' ? String(draft.header.CodFiscal ?? '').trim() : '';
    const source = family === 'storno' ? draft.source : null;
    if (!number || (family === 'nir' && !supplier) || (family === 'storno' && !source)) return;
    const key = `${supplier}|${source ?? ''}|${number.toUpperCase().replace(/\s+/g, '')}`;
    if (seen.get(draft) === key) return;
    seen.set(draft, key);
    const extra = new URLSearchParams({ number, ...(supplier ? { cf: supplier } : {}), ...(source ? { source } : {}),
      ...(state.mode === 'edit' && state.doc?.id ? { exclude: state.doc.id } : {}) });
    try {
      const answer = await api(url('/invoice-twins', `&${extra}`));
      if (aborted() || draft !== state.draft) return;
      const text = twinText(number, answer.twins || []);
      if (text) showMessage(status, text, 'warning');
    } catch (error) {
      console.error('[vercon] the invoice-number check could not run', error);
    }
  }
  return { warn };
}
