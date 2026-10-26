# SLICE-0000-61…82 — Istoricul ajutorului facturilor primite

Actualizările din 17.10 (93, 96…98) descriu vederea fără furnizor, etichetele arborelui și lista cu recepția asociată. Capturile vechi rămân indicate ca necesitând refacere; nu sunt declarate actualizate.

14 etape 61…66 și 75…82 din 09–16.10.2026. Pentru 61/62/63 se aplică aceeași limită: numai ajutor pentru utilizatori, fără server/tabele/jurnale; nevăzut pe ecran, fără captură. Restul probelor și capturilor rămân pe etapă. Regula de vizibilitate, sincronizarea, vederile și popup-ul evoluează conform [istoricului ecranelor](SLICE-00EF-18-primite-client.md). Numărul 0000-61 exista și la ajutorul grupelor 0008-02; acel rând rămâne distinct, neatins, fără renumerotare.

- [0000-61](#slice-0000-61)
- [0000-62](#slice-0000-62)
- [0000-63](#slice-0000-63)
- [0000-64](#slice-0000-64)
- [0000-65](#slice-0000-65)
- [0000-66](#slice-0000-66)
- [0000-75](#slice-0000-75)
- [0000-76](#slice-0000-76)
- [0000-77](#slice-0000-77)
- [0000-78](#slice-0000-78)
- [0000-79](#slice-0000-79)
- [0000-80](#slice-0000-80)
- [0000-81](#slice-0000-81)
- [0000-82](#slice-0000-82)
- [0000-93](#slice-0000-93)
- [0000-96](#slice-0000-96)
- [0000-97](#slice-0000-97)
- [0000-98](#slice-0000-98)

## SLICE 0000-61 — help for 00EF-18 (09.10.2026) <a id="slice-0000-61"></a>

### What changed and why
Sections «Facturile primite» (the view «E-Factura» of the DDF: tree by months, views Linii / Factura PDF / Atasamente / Mesaje, right-click ZIP / XML, the manual link with the «show all» tick) and «Sincronizarea facturilor primite» (the period, the progress line, no figures box).

Topic: `contabil.efactura` (`src/KBot.App/HelpContent/contabil/efactura/index.md`). Tags: `00EF-18, 0000-61`.

## SLICE 0000-62 — help for 00EF-19 (09.10.2026) <a id="slice-0000-62"></a>

### What changed and why
Section «Fereastra Facturi primite»: all the received invoices of the year, the search box, «Sincronizeaza cu ANAF», «Reimprospateaza», «Iesire»; opened from the title-bar menu of the issued-invoice window.

Topic: `contabil.efactura` (`src/KBot.App/HelpContent/contabil/efactura/index.md`). Tags: `00EF-19, 0000-62`.

## SLICE 0000-63 — help for 00EF-20 (09.10.2026) <a id="slice-0000-63"></a>

### What changed and why
In the section «Fereastra Facturi primite»: «Leaga de un DDF…» (the list of fundamentari with a search box) and «Scoate legatura cu …» in the invoice menu.

Topic: `contabil.efactura` (`src/KBot.App/HelpContent/contabil/efactura/index.md`). Tags: `00EF-20, 0000-63`.

## SLICE 0000-64 — help for 00EF-21 (09.10.2026) <a id="slice-0000-64"></a>

### What changed and why
Section «Facturile primite» of `contabil.efactura`: the view «E-Factura» of the shell shows only for an angajament whose DDF has received invoices; for the others it is not there. Tags: `00EF-21, 0000-64`.

## SLICE 0000-65 — help for 00EF-22 (09.10.2026) <a id="slice-0000-65"></a>

Section «Facturile primite» of `contabil.efactura`: the tab «E-Factura» follows the flag read with the tree; after a sync or a link it shows when the tree is refreshed. Tags: `00EF-22, 0000-65`.

## SLICE 0000-66 -- help for 00EF-23 (09.10.2026) <a id="slice-0000-66"></a>

Section «Facturile primite» of `contabil.efactura`: «Salveaza ca ZIP» gives the signed ANAF archive kept with the invoice (fetched again from ANAF only when it is missing); «Salveaza ca XML» is the invoice from that archive. Tags: `00EF-18, 00EF-21, 00EF-22, 00EF-23, 0000-61, 0000-64, 0000-65, 0000-66`. No picture changed. The Migrator tab is a developer tool: not in the help.

## SLICE-0000-75 - Help for 00EF-29: received invoices, tree by supplier (15.10.2026) <a id="slice-0000-75"></a>

Help sub-slice of 0000 for [00EF-29](SLICE-00EF-18-primite-client.md#slice-00EF-29) (what changed on the screen is described there).

### What changed and why
`src/KBot.App/HelpContent/contabil/efactura/index.md`:
- «Facturile primite»: the tree is now root > month > supplier (by tax code) > invoice (date, number, total, value on the right); the dot of an invoice is neutral for now (no longer orange / green; it does not show whether the invoice was opened); the magnifier in the tree header opens the search (3 characters, Esc clears); the button on the right of the header is for filter options (none yet); «Continut» has the supplier and the notes on top and the lines below, and the VAT per rate is gone.
- «E-Factura Cumparare: «Facturi primite»»: the search box above the tree is gone, the search is the magnifier of the header; the tree has the suppliers level.
- Tags `00EF-29, 0000-75` on both sections; `help-version.txt` = `2026-10-15`.

### Files touched
`src/KBot.App/HelpContent/contabil/efactura/index.md`, `src/KBot.App/HelpContent/help-version.txt`, `docs/worklog/state/KBOT_STATUS_0000-0009.md`, `docs/worklog/KBOT_STATUS.md`.

### Test results
- `Check-Help.ps1 -Coverage`: no error for `efactura/index.md`; the other errors are old (tours / tutorials).
- `dotnet build src/KBot.App`: 0 warnings, 0 errors. The help was not opened on screen.

### Unverified / deferred
- Captures to redo: `efactura-cumparare` (the tag carries `redo:`).
- The text describes the code, not what was seen on screen.

## SLICE-0000-76 - Help for 0114-01 / 00EF-30: sync from the ribbon (15.10.2026) <a id="slice-0000-76"></a>

### What changed and why
- `contabil/efactura/index.md`: the intro (order «Cumparare» / «Vanzare»; the sync button only with the «Cumparare» page), the section «Sincronizarea facturilor primite» (no window any more; the list of days, the box for days 1-60, the bottom line) and the section of the «Facturi primite» page (no button «Sincronizeaza cu ANAF»; the bar of views; no tooltips on the tables).
- `contabil/ribbon.md`: the E-Factura row (order, the fields, the conditional button).
- Tags `0114-01, 00EF-30, 0000-76`; `help-version.txt` stays `2026-10-15`.

### Files touched
`src/KBot.App/HelpContent/contabil/efactura/index.md`, `src/KBot.App/HelpContent/contabil/ribbon.md`, `docs/worklog/state/KBOT_STATUS_0000-0009.md`, `docs/worklog/KBOT_STATUS.md`.

### Test results
`Check-Help.ps1 -Coverage`: see the worklog of the task; build of KBot.App: 0 warnings, 0 errors. The help was not opened on screen.

### Unverified / deferred
- Captures to redo: `ribbon-efactura` (the tab changed: order, button hidden by default) and `efactura-cumparare` (already marked).

## SLICE-0000-77 - Help for 00EF-31: received invoices without «Atasamente» / «Observatii», save asks where (15.10.2026) <a id="slice-0000-77"></a>

### What changed and why
`contabil/efactura/index.md` («Facturile primite» and «E-Factura Cumparare»): the lists of views no longer name «Atasamente» / «Observatii»; one line about Adobe's save button in «Factura PDF» (asks whether and where). Tags `00EF-31, 0000-77`. `help-version.txt` stays `2026-10-15`.

### Files touched
`src/KBot.App/HelpContent/contabil/efactura/index.md`, `docs/worklog/state/KBOT_STATUS_0000-0009.md`, `docs/worklog/KBOT_STATUS.md`.

### Test results / unverified
`Check-Help.ps1 -Coverage`: no new error for `efactura/index.md`. Text describes code that was not seen working.

## SLICE-0000-78 - Help for 00EF-32 (15.10.2026) <a id="slice-0000-78"></a>

### What changed and why
`contabil/efactura/index.md`, section «Sincronizarea facturilor primite»: what the operator sees at the end (the table of new invoices, or «Nu există facturi noi pe serverul ANAF.»). Tags `00EF-32, 0000-78`; `help-version.txt` stays `2026-10-15`.

### Files touched
`src/KBot.App/HelpContent/contabil/efactura/index.md`, the status files.

### Test results / unverified
`Check-Help.ps1 -Coverage`: no new error. The text describes code not seen working.

## SLICE-0000-79 - Help for 00EF-33 (15.10.2026) <a id="slice-0000-79"></a>

### What changed and why
`contabil/efactura/index.md`: «Sincronizarea facturilor primite» (the footer «Actualizare…» and its popup: list of periods, box for 1-60 days with Enter) and the page «Cumparare» (no «Reimprospateaza»; the sync starts from the footer or the ribbon). Tags `00EF-33, 0000-79`; `help-version.txt` stays `2026-10-15`.

### Files touched
`src/KBot.App/HelpContent/contabil/efactura/index.md`, the status files.

### Test results / unverified
`Check-Help.ps1 -Coverage`: no new error. Capture `efactura-cumparare` is already marked to redo (the footer is new in it). The text describes code not seen working.

## SLICE-0000-80 - Help for 00EF-35 (15.10.2026) <a id="slice-0000-80"></a>

### What changed and why
`contabil/efactura/index.md`, section «Facturile primite»: the options of the header button (the two filters, the grouping, the orders). Tags `00EF-35, 0000-80`; `help-version.txt` stays `2026-10-15`. Capture `efactura-cumparare` is already marked to redo.

### Files touched
`src/KBot.App/HelpContent/contabil/efactura/index.md`, the status files.

### Test results / unverified
`Check-Help.ps1 -Coverage`: no new error. The text describes code not seen working.

## SLICE-0000-81 - Help for 00EF-36 (15.10.2026) <a id="slice-0000-81"></a>

### What changed and why
`contabil/efactura/index.md`, section «Facturile primite»: the view «XML Factura» (right end of the bar of views): map without the cac / cbc prefixes, coloured text, read only, long values cut. Tags `00EF-36, 0000-81`; `help-version.txt` stays `2026-10-15`.

### Files touched
`src/KBot.App/HelpContent/contabil/efactura/index.md`, the status files.

### Test results / unverified
`Check-Help.ps1 -Coverage`: no new error. The text describes code not seen working.

## SLICE-0000-82 - Help for 00EF-37 (16.10.2026) <a id="slice-0000-82"></a>

### What changed and why
`contabil/efactura/index.md`, section «Facturile primite», paragraph «Din subsolul arborelui»: the icons of the rows and the row «Sincronizeaza cu serverul» under a separator. Tags `00EF-37, 0000-82`; `help-version.txt` unchanged.

### Files touched
`src/KBot.App/HelpContent/contabil/efactura/index.md`, the status files.

### Test results / unverified
`Check-Help.ps1 -Coverage`: see the run of this task. The text describes code not seen working.

## SLICE-0000-93 - Help for 0114-22 (17.10.2026) <a id="slice-0000-93"></a>

### What changed and why
`contabil/efactura/index.md` («Facturile primite»): in the view of the angajament the tree is «Toate facturile» / month / invoice (no supplier level) and the header has only the magnifier (no options button); the options menu and the supplier level are described as belonging to the «Facturi primite» window.

### Files touched
`src/KBot.App/HelpContent/contabil/efactura/index.md`.

### Test results
`tools\HelpCheck\Check-Help.ps1`: no new error from this file.

### Unverified / deferred
Nothing seen on screen.

## SLICE-0000-96 - Help for 00EF-40 (17.10.2026) <a id="slice-0000-96"></a>

### What changed and why
`contabil/efactura/index.md`, the bullet about the tree on the left: the root «Toate facturile» has no count of invoices (only the total), the months show only the month's name, and the year appears only when the list has invoices of more than one year. Section tag: `00EF-40, 0000-96`.

### Files touched
`src/KBot.App/HelpContent/contabil/efactura/index.md`.

### Test results
`tools\HelpCheck\Check-Help.ps1 -Coverage`: see the status row.

### Unverified / deferred
Nothing seen on screen. The captures of the received-invoices tree (`efactura-cumparare` and any other showing «Toate facturile (n)» or «Luna AAAA») may need a redo.

## SLICE-0000-97 - Help for 00EF-41 (17.10.2026) <a id="slice-0000-97"></a>

### What changed and why
`contabil/efactura/index.md`, the list of what the received-invoices view does: a new bullet after the one about the tree describes the list shown when «Toate facturile» is chosen (the ticks and the icon that ticks all, «Listeaza», «Salveaza XML», the «...» button of a row, what happens on a failure, how the bar of views comes back). The section tag carries `00EF-41, 0000-97`.

### Files touched
`src/KBot.App/HelpContent/contabil/efactura/index.md`.

### Test results
`tools\HelpCheck\Check-Help.ps1 -Coverage`: see the status row.

### Unverified / deferred
Nothing seen on screen. A capture of the list is not made (no `<!-- capture -->` line added); the existing captures of the received-invoices tree are unchanged.

## SLICE-0000-98 - Help for 00EF-42 (17.10.2026) <a id="slice-0000-98"></a>

### What changed and why
`contabil/efactura/index.md`, the bullet about the list shown when «Toate facturile» is chosen: the grid has the column «Recepție asociată» (date, description and value of the linked reception in one cell, several separated by «|», empty when not linked) instead of the supplier. Section tag: `00EF-42, 0000-98`.

### Files touched
`src/KBot.App/HelpContent/contabil/efactura/index.md`.

### Test results
`tools\HelpCheck\Check-Help.ps1 -Coverage`: see the status row.

### Unverified / deferred
Nothing seen on screen.
