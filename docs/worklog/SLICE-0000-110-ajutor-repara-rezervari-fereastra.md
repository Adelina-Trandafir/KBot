# SLICE-0000-110/120/121 — Ajutorul diagnosticării și reparării rezervărilor și programului

68 documentează reparația inițială și ștergerea ultimei luni din istoric; 110 descrie previzualizarea; 120 adaugă legăturile greșite, poarta și tooltipul, iar 121 păstrează separat în același istoric regula programului și a treia nuanță de roșu. Cerințele diferite ale capturilor și limitele fiecărei etape sunt păstrate.

[0000-68](#slice-0000-68) · [0000-110](#slice-0000-110) · [0000-120](#slice-0000-120) · [0000-121](#slice-0000-121)

## SLICE-0000-68 - help for the repair button, the red reservation rows and the history-month delete (12.10.2026) <a id="slice-0000-68"></a>

Work: see `SLICE-0092-01-rezervari-gresite-sterge-luna.md`.

- Changed (tag `0092-01, 0000-68`): `contabil/vederi/rezervari.md` (the footer-right row, new section «Rezervări greșite și repararea lor»), `tours/tur-rezervari.md` (step «Arbore › Repară», was «Reîmprospătează»), `contabil/fereastra.md` (the red row also means broken reservations), `contabil/forexe/descarcare.md` («Reîmprospătări parțiale»: only Recepții still refreshes from FOREXE), `avansat/index.md` (the «Istoric (avansat)» section of Setări › FOREXE and the new section «Ștergerea ultimei luni din istoric»).
- Tests: `tools\HelpCheck\Check-Help.ps1` run; the messages for these pages were only the missing worklog (this file) and the status row for `0000-68`.
- Unverified / stale: the pictures `rezervari.png` and `rezervari-meniu-ddf.png` still show the old footer (refresh icon, caption «Actualizează»); no capture could be redone (nothing is run on screen). Listed in the «Ajutor de actualizat» line of `state/KBOT_STATUS_0000-0009.md`. No capture was added for the new Setări section.

## SLICE-0000-110 - Help for 0114-25 (19.10.2026) <a id="slice-0000-110"></a>

### What changed and why
The «Repara...» button of the Rezervari view now opens a window with what was detected and what will be written. Help updated to say so.

### Files touched
`src/KBot.App/HelpContent/contabil/vederi/rezervari.md`, section «Rezervari gresite si repararea lor»: the bullet on «Repara...» describes the window (two tables, «Repara» / «Renunta»). Tag `0114-25, 0000-110`. `src/KBot.App/HelpContent/tours/tur-rezervari.md`, step «Ribbon > Repara...»: same sentence. Same tag.

### Test results
`tools\HelpCheck\Check-Help.ps1`: the only message for these files was «0000-110 not in KBOT_STATUS» before the status row was added.

### Unverified / deferred
Not seen on screen. No new capture needed (the existing «grafic-rezervari» capture does not show the window).

## SLICE-0000-120 - Help: reservations linked wrongly, no new reservation in the browser while one waits, bigger tree tooltip (23.10.2026) <a id="slice-0000-120"></a>

Help recorded for slice 0114-26.

### Topics changed (tag `0114-26, 0000-120`)
- `contabil/vederi/rezervari.md`: ribbon button «Repara...» row (also shown for wrongly linked reservations); tag added on the wrong-reservations section; NEW section «Rezervari legate gresit».
- `contabil/forexe/browser.md`: NEW sub-section «Nu incepi o rezervare cat alta asteapta revizia».
- `contabil/fereastra.md`: «Lista angajamentelor» - another red for wrongly linked reservations; the hover label is now a table with bigger letters.

### Captures to redo
- `rezervari` (ribbon button text changed), and the main list capture where the tooltip is shown (new table look, bigger font), if one exists.

### Notes
- Check-Help error for `0000-120` missing from the status file is solved by the status rows of this task.
- Not seen on screen.

## SLICE-0000-121 - Help: the DDF program follows the indicators' source, «Repara programul», third red (23.10.2026) <a id="slice-0000-121"></a>

Help recorded for slice 0114-27.

### Topics changed (tag `0114-27, 0000-121`)
- `contabil/fereastra.md`: «Lista angajamentelor» - a third red (more orange) when the DDF header program does not match the SS saved in indicators, DDF revisions or ordonantari; what the hover label shows; where it is repaired.
- `contabil/ddf/index.md`: «Vederea «Fundamentare»» - the rule for the program (01A -> 0000002510, otherwise 0000000000), the ribbon button «Repara programul», what it changes and what it leaves.
- `contabil/ddf/rezervare.md`: «Adauga rezervare» - revision 0 of an angajament from FOREXE starts with the program of the indicators' source.
- `contabil/vederi/ord.md`: «Documentul si semnarea» - the program of the ordonantare comes from the indicators' source.

### Captures to redo
- None required. The `ddf-document` capture shows the ribbon without the new button, which appears only while a document has the wrong program.

### Notes
- Written for the operator only: no table or key names.
- Not seen on screen.
