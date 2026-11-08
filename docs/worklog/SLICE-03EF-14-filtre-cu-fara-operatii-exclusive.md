# SLICE-03EF-14 - Tree options: «Afisare facturi cu operatii» next to «fara operatii», exclusive (08.11.2026)

## What changed and why
Operator (08.11.2026): the options popup of the tree gets «Afisare facturi cu operatii in AVACONT». Choosing one of the two shows ONLY that state; they are never combined.
- `FacturiCumparareView`: new `_onlyWithOperations` and `OptCuOperatii`; `ShownItems` keeps `NrOperatii > 0`; `OptionChosen` turns the other filter off when one is turned on. A click on the ticked one unticks it (every invoice is shown again) - my reading, to confirm.
- Same day: «Afisare facturi din arhiva» (`_showArchived`) joins the group: the three are one choice, never combined (each one turned on turns the other two off). «Afisare facturi noi» stays combinable.
- The first-opening state of 03EF-13 is unchanged (only invoices without operations).

## Files touched
`KBot.EFactura/Views/FacturiCumparare/FacturiCumparareView.vb`; help `contabil/efactura/index.md` (tag 03EF-14, text of the options paragraph; recorded with 0000-174).

## Test results
KBot.EFactura builds clean (0 warnings, 0 errors). Nothing run or seen on screen.

## Unverified / deferred
Whether a click on the ticked option should untick it (as built) or do nothing.

## Correction (08.11.2026)
Operator: only ONE of the three can be selected at a time. Now a strict radio choice: exactly one is on, a click on the one in force changes nothing (the earlier «click unticks it» is gone). The consequence: there is no longer a way to see working invoices with and without operations together.
