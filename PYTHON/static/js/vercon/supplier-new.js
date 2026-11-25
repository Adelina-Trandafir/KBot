// SLICE-VC20-01: the supplier of an e-invoice that is not in the supplier list yet. The NIR started from that invoice carries the supplier as
// a pending choice (shown as «nume (nou)»); it is really added, with the data ANAF gives, when the NIR is saved.

// Puts the pending supplier in the draft and in the list the «Furnizor» field offers. `lookups.partners` is the list of the editor.
export function markNewSupplier(draft, lookups, detail) {
  const cf = String(detail.cui || '').trim();
  draft.newSupplier = null;
  if (!cf) return null;
  const name = detail.supplier || cf;
  draft.newSupplier = { cf, name };
  if (lookups && Array.isArray(lookups.partners) && !lookups.partners.some((item) => String(item.cf) === cf)) {
    lookups.partners.push({ id: cf, name: `${name} (nou)`, cf, pending: true });
  }
  return cf;
}

// True when the pending supplier is the one the NIR is saved with (the Administrator may have chosen another supplier meanwhile).
export const needsAdding = (draft, header) => !!draft?.newSupplier && String(header?.CodFiscal ?? '').trim() === draft.newSupplier.cf;

// Adds the supplier (the server asks ANAF; `if_missing` makes a repeated call harmless) and replaces the pending entry of the list.
export async function addPendingSupplier({ api, sel, draft, lookups }) {
  const { cf } = draft.newSupplier;
  const selection = sel();
  const result = await api('/api/vercon/suppliers/create', { method: 'POST', headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ subunit: selection.subunit, gestiune: selection.gestiune, cod_fiscal: cf, if_missing: true }) });
  const entry = (lookups?.partners || []).find((item) => String(item.cf) === cf);
  if (entry) { entry.name = result.name || entry.name.replace(/ \(nou\)$/, ''); delete entry.pending; }
  draft.newSupplier = null;
  return result;
}
