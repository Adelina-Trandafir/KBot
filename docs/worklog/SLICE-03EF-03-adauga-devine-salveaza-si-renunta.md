# SLICE-03EF-03 - «Adaugă operații contabile» is replaced by «Salvează» and «Renunță» while booking (08.11.2026)

## What changed and why
Operator (08.11.2026): pressing «Adaugă operații contabile» makes it disappear from the view and puts «Salvează» and «Renunță» in its place.
- Booking is on while at least one invoice is ticked (`ContareEditing` = `_contare AndAlso _tickedIds.Count > 0`). Then «Adaugă» is hidden, «Salvează» and «Renunță» are shown (the ribbon button order is Adaugă | Salvează | Renunță | Salvează ca... | Ascunde...; the separator is `SeparatorAfter` on both «Adaugă» and «Renunță», only one of them is on screen at a time). «Salvează ca...» and «Ascunde...» stay.
- «Salvează» = the old page button (`SaveContare`, the old `PgContare_SalveazaCerut`): same confirmations, same request. Enabled by `pgContare.PoateSalva` (what `UpdateSave` decided; now also re-evaluated while the explanation is typed); the page raises `SalvareStareSchimbata` when it changes.
- «Renunță» = `ClearTicks`; «Adaugă» comes back.
- The tooltip of «Salvează» now carries what the foot of the page used to say (supplier, warnings, bookings): see 03EF-04.

## Files touched
Same set as 03EF-02, plus `FacturiCumparareContarePage.vb` (`PoateSalva`, `Detalii`, `SalvareStareSchimbata`).

## Test results
Nothing run, nothing seen on screen: the build is clean (KBot.App, Debug, 0 warnings, 0 errors), no test was run (rule), the designer was never opened in Visual Studio.

## Unverified / deferred
The group order and the separator behaviour with a hidden button (`SeparatorAfter` on a hidden button) were not seen.
