# SLICE-0000-68 - help for the repair button, the red reservation rows and the history-month delete (12.10.2026)

Work: see `SLICE-0092-01-rezervari-gresite-sterge-luna.md`.

- Changed (tag `0092-01, 0000-68`): `contabil/vederi/rezervari.md` (the footer-right row, new section «Rezervări
  greșite și repararea lor»), `tours/tur-rezervari.md` (step «Arbore › Repară», was «Reîmprospătează»),
  `contabil/fereastra.md` (the red row also means broken reservations), `contabil/forexe/descarcare.md`
  («Reîmprospătări parțiale»: only Recepții still refreshes from FOREXE), `avansat/index.md` (the «Istoric (avansat)»
  section of Setări › FOREXE and the new section «Ștergerea ultimei luni din istoric»).
- Tests: `tools\HelpCheck\Check-Help.ps1` run; the messages for these pages were only the missing worklog (this file)
  and the status row for `0000-68`.
- Unverified / stale: the pictures `rezervari.png` and `rezervari-meniu-ddf.png` still show the old footer
  (refresh icon, caption «Actualizează»); no capture could be redone (nothing is run on screen). Listed in the
  «Ajutor de actualizat» line of `state/KBOT_STATUS_0000-0009.md`. No capture was added for the new Setări section.
