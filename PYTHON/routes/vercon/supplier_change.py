"""SLICE-VC16-03: «Modifica furnizor» -- the operator changes ONLY the supplier of a saved NIR, in any period.

The route that serves it does not call `protocol.ensure_open`: a NIR of a closed exercise can have its supplier corrected
(the supplier is a label of the receipt, it moves no quantity and no value). The lock of the gestiune and the right of the
operation are still required, and so is the revision of the document.

What moves together, in the same transaction:
  * VC_NIR.CodPartener + FurnizorV of the receipt (the supplier is identified only by its normalized fiscal code);
  * the same two columns of the storno invoices (FCS) of this receipt -- an FCS has no supplier of its own, it copies the receipt's;
  * VC_FACTURI.CodPartener of the common invoice each of them is tied to, but only when that invoice belongs to no other live receipt.

What is refused (the message says why): a cancelled document, a document tied to an e-invoice (its supplier comes from the e-invoice),
an invoice shared with other receipts (they would keep the old supplier), a supplier that is not registered and active, the same supplier.
A second invoice of the new supplier with the same number is only a WARNING returned to the page (as VC14-05).
"""
from utils import vercon_suppliers as suppliers

from . import invoice_twins, invoices, protocol
from .documents import _all, _load_for_write, _one, _scope
from .efactura_receipts import require_editable
from .nomenclatoare import NomError


def _live_receipts_of(cursor, invoice_id, ctx, nir_id):
    """How many OTHER live receipts are tied to the common invoice."""
    row = _one(cursor, 'SELECT COUNT(*) AS n FROM VC_NIR_FACTURI f JOIN VC_NIR n ON n.SubunitId=f.SUBUNIT_ID AND n.GestiuneId=f.GESTIUNE_ID '
               'AND n.ExercitiuId=f.EXERCITIU_ID AND n.IdIntrare=f.NIR_ID WHERE f.FACTURA_ID=%s AND n.Anulat=0 '
               'AND NOT (f.SUBUNIT_ID=%s AND f.GESTIUNE_ID=%s AND f.EXERCITIU_ID=%s AND f.NIR_ID=%s)', (invoice_id, *_scope(ctx), nir_id))
    return row['n'] if row else 0


def _plan_invoice(cursor, ctx, nir_id, code):
    """The common invoice to move to the new supplier, or None. Raises when it cannot move alone."""
    if not invoices.ready(cursor):
        return None
    link = invoices.link_of(cursor, ctx, nir_id)
    if not link:
        return None   # no common invoice yet: it is built later from the receipt, which already holds the new supplier
    invoice = invoices.load(cursor, link['FACTURA_ID'], lock=True)
    if suppliers.code(invoice['CodPartener']) == code:
        return None
    if _live_receipts_of(cursor, invoice['ID'], ctx, nir_id):
        raise NomError('INVOICE_SHARED', 'Factura este folosită și de alte recepții, care ar rămâne la furnizorul vechi. '
                       'Schimbați furnizorul pe toate recepțiile acestei facturi, sau separați mai întâi recepțiile.', 409)
    return invoice['ID']


def change(cursor, repo, ctx, doc_id, body, request_id):
    _, _, before = _load_for_write(repo, ctx, 'nir', doc_id, body.get('version'))
    require_editable(cursor, ctx, doc_id)
    code = suppliers.code(body.get('cf'))
    partner = suppliers.find(cursor, code, active_only=True) if code else None
    if not partner:
        message = 'Alegeți furnizorul după codul fiscal.'
        raise NomError('INVALID', message, 422, {'cf': message})
    old_code = suppliers.code(before['CodPartener'])
    if code == old_code:
        message = 'Documentul are deja acest furnizor. Alegeți alt furnizor.'
        raise NomError('NO_CHANGE', message, 422, {'cf': message})
    name = partner['DenumirePartener'] or code
    storno = [row['IdIntrare'] for row in _all(cursor, 'SELECT IdIntrare FROM VC_NIR WHERE SubunitId=%s AND GestiuneId=%s AND ExercitiuId=%s '
                                                         'AND IdStorno=%s AND Anulat=0 FOR UPDATE', (*_scope(ctx), doc_id))]
    moves = [invoice for invoice in (_plan_invoice(cursor, ctx, identity, code) for identity in [doc_id, *storno]) if invoice]
    if repo.update('VC_NIR', {'CodPartener': code, 'FurnizorV': name}, {'IdIntrare': doc_id, 'Version': before['Version']}, bump_version=True) != 1:
        raise NomError('CONFLICT', 'Documentul a fost modificat de altcineva. Reîncărcați documentul.', 409)
    for identity in storno:
        repo.update('VC_NIR', {'CodPartener': code, 'FurnizorV': name}, {'IdIntrare': identity}, bump_version=True)
    for invoice_id in moves:
        cursor.execute('UPDATE VC_FACTURI SET CodPartener=%s, VERSION=VERSION+1 WHERE ID=%s', (code, invoice_id))
    protocol.audit(cursor, ctx, 'nir.supplier', 'VC_NIR', doc_id,
                   {'supplier': old_code, 'name': before['FurnizorV']}, {'supplier': code, 'name': name, 'storno': storno, 'invoices': moves}, None, request_id)
    return {'id': doc_id, 'supplier': {'cf': code, 'name': name}, 'twins': invoice_twins.find(cursor, ctx, code, before['NrFact'], exclude=doc_id)}
