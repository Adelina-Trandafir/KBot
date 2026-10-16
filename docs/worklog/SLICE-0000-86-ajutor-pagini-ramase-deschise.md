# SLICE-0000-86 - Help for 0113-01: pages stay open, editors are windows again (16.10.2026)

## What changed and why
The operator's rule of 16.10.2026 (see `SLICE-0113-01-pagini-deschise-si-ferestre-modale.md`) changed what the operator sees:
pages (Extrase, Clasificatii, Parteneri, Setari, Jurnal, Istoric, Ajutor, E-Factura) stay open behind each other; the DDF / ORD
editors, the reception links («Asocieri»), «Operatiuni necorelate», the reservations chart and «Grupe de angajamente» are
windows of their own again, with their own title bar and buttons; the PDF of a page that is covered is closed.

Topics (tag `0113-01`, the slice that decided the text, where it changed):
- `contabil/fereastra.md`: section «Machetele se deschid ca pagini» -> «Pagini si ferestre» (two kinds; pages stay open and come
  back from the ribbon or «Machete deschise»; the PDF closes and the bar goes to a tab without a PDF; closing asks each page);
  «Fereastra «Grupe de angajamente»».
- `contabil/ddf/editor.md`, `contabil/ddf/nou.md`, `contabil/vederi/ord-editor.md`: a window, «Renunta» is back (no more
  «Inchide» from the ribbon for the editors).
- `contabil/forexe/descarcare.md`, `contabil/asocieri/fereastra.md`, `contabil/asocieri/cazuri.md`: the «Asocieri» window,
  «Renunta».
- `contabil/notecab.md`: the correction window, «Iesire» or Esc (`CabNoteForm.CancelButton`).
- `contabil/vederi/rezervari.md`: the chart in its own window, «Inchide» (`GraficRezervariForm.btnInchide`).
- `contabil/panou-lateral.md` (section «Machete deschise»): the list is a stack -- open ones first in opening order and fixed, closed
  ones in italics at the end, closed ones leave first when the 10 lines are full; modal windows and the views of the shell
  (Sumar, Rezervari...) are not listed.
Button words checked in the Designers: `btnRenunta` = «Renunta» (DdfEditForm, OrdEditForm, AsociereForm), `btnInchide` =
«Inchide» (GraficRezervariForm), `btnIesire` = «Iesire» (GrupeForm), `btnInchide` = «Iesire» + CancelButton (CabNoteForm).

## Files touched
`src/KBot.App/HelpContent/contabil/`: `fereastra.md`, `ddf/editor.md`, `ddf/nou.md`, `vederi/ord-editor.md`,
`vederi/rezervari.md`, `forexe/descarcare.md`, `asocieri/fereastra.md`, `asocieri/cazuri.md`, `notecab.md`, `panou-lateral.md`.
`help-version.txt` stays `2026-10-16` (same day as 0000-83 / 0000-85).

## Test results
`tools\HelpCheck\Check-Help.ps1 -Coverage` run 16.10.2026: no error from these files. The errors left are old and not from here
(`tutorials\*` sections without a slice tag, `tur-setari.md` nav item `jurnal`); coverage lists screens without a topic that
were already listed (EFacturaView, ForcedPasswordChangeForm, ...).

## Capturi de refacut
`ddf-editor`, `ord-editor`, `asocieri-fereastra`, `asocieri-propunere`, `operatiuni-necorelate`, `grafic-rezervari`,
`panou-lateral` (italic closed lines), `grupe-fereastra` (whole-window pictures: the title bar is back / the size changed). Their tags carry `redo: 2026-10-16 12:00`.

## Unverified / deferred
- Not seen on screen; the wording follows the code of 0113-01 (build only).
- Not re-walked: the tours and tutorials that mention these windows (`tur-ddf-editor`, `tur-notecab`, `ordonantare-din-plata`, `fereastra-extrase`...).
- Not touched on purpose: the topics that say a page «se deschide ca pagina» (Jurnal, Setari, Nomenclatoare, Istoric, E-Factura): still true.
