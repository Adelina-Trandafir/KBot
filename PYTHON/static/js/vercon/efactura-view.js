import { pdfPreview } from './ui.js';
import { pickBox } from '../utils/confirm-box.js';
import { compareTyped } from '../utils/typed-order.js';

export async function pickReceiptEfactura(doc, title) {
  const invoices = [...(doc.efactura || [])].sort((a, b) => compareTyped(a.supplier, b.supplier, 'text')
    || compareTyped(a.date, b.date, 'date') || compareTyped(a.number, b.number, 'text'));
  if (invoices.length <= 1) return invoices[0];
  const sending = await pickBox(title, invoices.map(item => ({
    value: item.sending, label: `${item.supplier} / ${item.number} / ${item.date.slice(0, 10)}`,
  })));
  return invoices.find(item => item.sending === sending);
}

export async function showReceiptEfactura({ api, selection, doc, owner }) {
  const chosen = await pickReceiptEfactura(doc, 'Afișează eFactura'); if (!chosen) return;
  const query = new URLSearchParams(Object.entries(selection).filter(([, value]) => value != null && value !== ''));
  await pdfPreview(`eFactura ${chosen.number} · ${chosen.supplier}`, async () => ({
    data: await api(`/api/vercon/doc/nir/${doc.id}/efactura/${chosen.id}/pdf?${query}`, { responseType: 'arrayBuffer' }),
    filename: `eFactura_${chosen.number}.pdf`.replace(/[<>:"/\\|?*]/g, '_'),
  }), owner);
}
