import { CheckBox, docNode } from './ui.js';
import { format, formatMoney } from './fields.js';
import { receiptTotal } from './receipt-reconciliation.js';
import { oiDocumentNodes } from './oi-ui.js';

const MONTHS = ['Ianuarie', 'Februarie', 'Martie', 'Aprilie', 'Mai', 'Iunie',
  'Iulie', 'August', 'Septembrie', 'Octombrie', 'Noiembrie', 'Decembrie'];

function dateNodes(documents, family) {
  const months = new Map();
  for (const doc of documents) {
    const date = String(doc.date || '');
    const month = /^\d{4}-(0[1-9]|1[0-2])/.test(date) ? date.slice(0, 7) : '';
    if (!months.has(month)) months.set(month, []);
    months.get(month).push(doc);
  }
  return [...months].sort(([a], [b]) => b.localeCompare(a)).map(([month, docs]) => ({
    id: `oi-month:${month || 'undated'}`,
    label: month ? `${MONTHS[Number(month.slice(5)) - 1]}/${month.slice(0, 4)}` : 'Fără dată',
    rightText: `TOTAL ${formatMoney(receiptTotal(docs.filter(doc => !doc.annulled), 'total', 4))}`,
    children: [...docs].sort((a, b) => String(b.date || '').localeCompare(String(a.date || ''))
      || Number(b.number) - Number(a.number) || Number(b.id) - Number(a.id))
      .map(doc => docNode(doc, `${family.toUpperCase()} ${doc.number} • ${format(doc.date, { type: 'date' })}${doc.annulled ? ' • anulat' : ''}`, formatMoney(doc.total))),
  }));
}

// Each document screen starts in date order; toggling leaves its data and selection intact.
export function oiTreeOrder() {
  let byDate = true, checkbox = null;
  return {
    get byDate() { return byDate; },
    nodes: (documents, family) => byDate ? dateNodes(documents, family) : oiDocumentNodes(documents, family),
    mount(host, owner, changed, enabled = true) {
      if (checkbox?.element.isConnected) { checkbox.setEnabled(enabled); return; }
      checkbox = owner.own(new CheckBox(host, { label: 'Ordoneaza dupa Dată', checked: byDate,
        className: 'vc-oi-tree-order', disabled: !enabled, onChange: checked => { byDate = checked; changed(); } }));
    },
  };
}
