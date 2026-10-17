# SLICE-0000-90 - Help for 0114-06 .. 0114-14 (17.10.2026)

## What changed and why
`contabil/fereastra.md` (rotita no longer has «Actualizeaza angajamente...»; «Grupe» only with a group; the footer popup; «Editeaza grupe» in the ribbon; E-Factura row in the views table), `contabil/ribbon.md` (the form group and the view group; «Descarcare» group removed; MENIU table), `contabil/forexe/lista.md`, `contabil/forexe/descarcare-multipla.md` (the «VRAC» row), `contabil/vederi/extrase.md`, `contabil/efactura/index.md`, tours `tur-fereastra` / `tur-extrase`, tutorials `descarcare-angajamente-noi`, `actualizare-multipla`, `grupe-deschide-fereastra`, `fereastra-extrase`, `descarca-extrase`. Captures marked for redo: 41 (whole-window ones, the ribbon ones, the footer / groups menus).

## Files touched
The help files above under `src/KBot.App/HelpContent/`.

## Test results
`tools\HelpCheck\Check-Help.ps1`: see the status row; the old errors (efactura tutorial controls, ordonantare-din-plata tags, tur-setari) are not from this slice.

## Unverified / deferred
Nothing seen on screen. The tutorial steps without a target (`grupe-deschide-fereastra`, `descarca-extrase`, `fereastra-extrase`) and the new anchors `popup.noi` / `popup.vrac` + signal `footer-menu:*` are not tried in the runner. Captures to be redone by the operator.
