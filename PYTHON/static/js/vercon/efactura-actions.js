import { confirmBox } from '../utils/confirm-box.js';
import { associateEfactura } from './efactura-association.js';
import { pickReceiptEfactura } from './efactura-view.js';

export function efacturaMenu(doc, { busy, blocked, view, change }) {
  const initial = doc.doc_type === 'STOC_INITIAL';
  const actions = initial ? [] : [{ icon: '↔', label: 'Asociază eFactura…', disabled: blocked, run: () => change('associate') }];
  if (doc.has_efactura) actions.push(
    { icon: '↔', label: 'Asociază cu altă eFactură', disabled: blocked || initial, run: () => change('replace') },
    { icon: '×', label: 'Elimină asocierea cu eFactura', disabled: blocked, run: () => change('unlink') },
    { icon: '📄', label: 'Afișează eFactura', disabled: busy, run: view },
  );
  return actions;
}

export function receiptEfacturaChanges(api) {
  const unconfirmed = new WeakMap();
  return async function change({ doc, mode, selection, signal }) {
    const association = { api, sel: () => selection, doc, signal };
    if (mode === 'associate') return associateEfactura(association);

    const invoice = await pickReceiptEfactura(doc, mode === 'replace' ? 'Alegeți eFactura de înlocuit' : 'Alegeți asocierea de eliminat');
    if (!invoice || signal.aborted) return null;
    if (mode === 'replace') return associateEfactura({ ...association, replace: invoice });

    const yes = await confirmBox(`Eliminați asocierea NIR-ului cu eFactura ${invoice.number} (${invoice.supplier})?`,
      { title: 'Elimină asocierea cu eFactura', yes: 'Elimină asocierea', no: 'Renunță' });
    if (!yes || signal.aborted) return null;
    const keys = unconfirmed.get(doc) || new Map();
    unconfirmed.set(doc, keys);
    const request_key = keys.get(invoice.sending) || crypto.randomUUID();
    keys.set(invoice.sending, request_key);
    const body = { ...selection, nir: doc.id, version: doc.version, request_key };
    try {
      const result = await api(`/api/vercon/doc/nir/efactura/${invoice.sending}/unlink`, {
        method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body),
      });
      keys.delete(invoice.sending);
      return result;
    } catch (error) {
      if (error.status >= 400 && error.status < 500) keys.delete(invoice.sending);
      throw error;
    }
  };
}
