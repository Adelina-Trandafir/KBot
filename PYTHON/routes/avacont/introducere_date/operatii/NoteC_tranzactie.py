# routes/avacont/introducere_date/operatii/NoteC_tranzactie.py
"""SLICE-AV08-04. The family NoteC_* is the ONE place where the accounting notes are written (operations, documents, notes, payments, links) and where their
rules live; the windows' routes only read the request and answer. This file: the lock and the transaction shared by every write, the refusal, the constants."""
import logging
from contextlib import contextmanager
from decimal import Decimal
from hashlib import sha256

from utils.database import get_kbot_connection

logger = logging.getLogger(__name__)

TREZORERIE_BIC = "TREZROBU"   # the payer bank of every order of payment (Access SAV_OP_2017: «TREZROBU»)

FEL_INCHIDERE = "I"
JURNAL_INCHIDERE = "JI"   # Access: the journal that may still write into a closed month (DataDoc_BeforeUpdate of Note)
CENT = Decimal("0.01")

# SLICE-AV08-04: how many payments cover an evidence operation `o` of the note `n`. Two keys: TZ_PlatiFacturi.IdEvidentiere (a payment of the same
# database), and TZ_PlatiFacturi.IdEvidentiereAnterior, which holds the Access IdOperatie of the evidence in the PREVIOUS year's Access file (Access
# IdFacturaAA, filled by AVC.Migrator). That number is an Access one, so it is matched on the evidence's IdOperatieAccess, for payments of the same subunit
# and of the year after the evidence. Used by the delete checks and by the editor: one copy of the rule.
SQL_PLATI_PE_EVIDENTA = (
    "((SELECT COUNT(*) FROM TZ_PlatiFacturi pfe WHERE pfe.IdEvidentiere = o.IdOper) + "
    "(SELECT COUNT(*) FROM TZ_PlatiFacturi pfa JOIN TZ_Oper opa ON opa.IdOper = pfa.IdPlata JOIN TZ_Documente doa ON doa.IdDocument = opa.IdDocument "
    "JOIN TZ_NoteC noa ON noa.IdNota = doa.IdNota WHERE o.IdOperatieAccess IS NOT NULL AND pfa.IdEvidentiereAnterior = o.IdOperatieAccess "
    "AND noa.IdSubunitate = n.IdSubunitate AND noa.An = n.An + 1))")

# The unit of Parteneri.IdUnitate: one row of Unitati per sector+source and year, and every partner is kept once per unit (the same partner shows up once for each
# sector+source and year). A window that works on ONE sector+source and year offers only the partners of that unit, else each partner would be offered several times.
SQL_UNITATI_SURSEI = "SELECT IdUnitate FROM Unitati WHERE SursaSector = %s AND An = %s AND COALESCE(Ascuns, 0) = 0"


class NoteCEroare(Exception):
    """A refusal the operator can act on: `reason` is a code, the text is Romanian.
    SLICE-AV08-05: `camp` names the box to correct, `parte` + `linie` a line of Debit / Credit (index from 0), `confirmare` the warning code the operator
    may accept (reason CONFIRMARE)."""

    def __init__(self, message, reason, status=409, camp=None, parte=None, linie=None, confirmare=None):
        super().__init__(message)
        self.reason = reason
        self.status = status
        self.camp = camp
        self.parte = parte
        self.linie = linie
        self.confirmare = confirmare

    def corp(self):
        """The JSON body of the refusal (only the keys that are set)."""
        out = {"error": str(self), "reason": self.reason, "code": self.reason}
        for cheie, valoare in (("camp", self.camp), ("parte", self.parte), ("linie", self.linie), ("confirmare", self.confirmare)):
            if valoare is not None:
                out[cheie] = valoare
        return out


@contextmanager
def salvare(db_name):
    """Own the transaction shared by all interactive accounting saves in this unit database."""
    conn = get_kbot_connection(db_name)
    # Note and OP series are allocated with MAX + 1 and may be empty. Serialize their writers even when there is no row to lock.
    cheie = "kbot:notec:" + sha256(db_name.encode("utf-8")).hexdigest()[:40]
    try:
        with conn.cursor() as cursor:
            cursor.execute("SELECT GET_LOCK(%s, 10)", (cheie,))
            if cursor.fetchone()[0] != 1:
                raise NoteCEroare("Se salvează alte operații ale unității. Încercați din nou.", "NOTEC_OCUPAT")
        conn.start_transaction(isolation_level="READ COMMITTED")
        yield conn
        conn.commit()
    except BaseException:
        try:
            if conn.is_connected():
                conn.rollback()
        except Exception:
            logger.warning("Anularea tranzactiei NoteC a esuat.", exc_info=True)
        raise
    finally:
        conn.close()  # the connection also releases its advisory lock

