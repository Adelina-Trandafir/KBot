# routes/efactura/primite_plata.py
"""
04EF-01: the PAYMENT of received invoices from «Contari». SLICE-AV08-04: the payment itself is written by NoteC (NoteC_contari.plata_e_facturi); here only the date of the order and the delete route.

One payment = one accounting operation (the payment template, e.g. 401 = 770) = one order of payment (OP) = one document «OP» whose number is the number of the OP.
The allocation of the payment on each invoice is kept in TZ_PlatiFacturi (never in EF_PrimiteOperatii, which holds evidence only). Everything runs in the
CALLER'S transaction: the cursor comes from `salveaza`, nothing is committed here.
"""
import logging
from datetime import date, datetime
from decimal import Decimal

from . import primite
from .tokens import EfEroare

logger = logging.getLogger(__name__)



def data_ordin(body):
    """The date of the payment/OP: `dataOP` (ISO) or today."""
    raw = body.get("dataOP")
    if raw is None:
        return date.today()
    try:
        return datetime.strptime(str(raw)[:10], "%Y-%m-%d").date()
    except ValueError as err:
        raise EfEroare("Data ordinului de plată nu este validă.", "DATA_OP_INVALIDA", 400) from err


def sterge(dc, id_op, user):
    """Deletes an UNLOCKED order with its payment (04EF-01). Locked (in an OPME or confirmed by a statement): refused."""
    from routes.avacont.introducere_date.operatii import NoteC
    # SLICE-AV08-04: the same lock and transaction as every other write of the accounting operations
    try:
        with NoteC.salvare(dc) as conn:
            numar = NoteC.sterge_plata(conn.cursor(dictionary=True), id_op)
    except (NoteC.PlatiEroare, NoteC.NoteCEroare) as err:
        raise EfEroare(str(err), err.reason, err.status) from err
    return {"IdOP": id_op, "NrOP": numar}
