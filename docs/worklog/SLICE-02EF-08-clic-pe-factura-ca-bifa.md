# SLICE-02EF-08 - A press on an invoice in the tree works like its tick (06.11.2026)

## Request
Operator: «la click pe o factura din copac, sa faca ce face check-ul. doar ca la click pe alta factura, sa se curete ce nu e checked in tree». The pressed invoice must NOT show a tick.

## Final state
The invoice pressed counts as ticked for everything that reads the ticked set; the invoice pressed before goes out, ticked ones stay; a press that comes with a tick is ignored for 500 ms on that node; a press on a covered invoice opens the read-only view. In the shared view (KE21) the rule is: the ticked invoices, else the shown (pressed) one (`ApplyReceptiiSetAsync`). Help 0000-164.

## Open problem (reported 06.11.2026, NOT fixed)
After a plain press the classification and the sum were not put on the invoice's row in the old window; after ticking the same invoice they were. No cause was established. The old window is gone, so it must be checked again in the shared view.
