# SLICE-02EF-06 - «Angajamente»: standard delete button; the tree opens with the invoices not associated in full (06.11.2026)

## Request
Operator: (1) in «Angajamente» the button that opened a popup with «Elimină asociere» becomes the standard delete button of the grid; (2) the tree opens with the invoices NOT associated IN FULL; the header icon offers all of them.

## Final state
- Column `opt` of the table of angajamente (`KeAngajamenteView`) = Button, `ButtonKind.Delete`; it asks the question and removes the association; hidden on the empty row, on an angajament a reception is linked to and while an invoice already covered is only looked at.
- Header-icon menu, three choices, the one on ticked: **«Arată facturile fără recepții»** (default: not associated in full, none or partly linked), **«Arată facturile asociate»** (at least one link, so a partly linked invoice is in both lists), **«Arată toate facturile»**. The tree caption follows the choice. In «Facturi primite» the menu is the header popup (only while the tab «Recepții» is on); the shell's view of an angajament has no menu and lists all the invoices of the angajament.

Help 0000-161. Never seen on screen.
