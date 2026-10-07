# SLICE-0000-58 - Ajutor pentru 00EF-13: fereastra «Date Unitate»

Help sub-slice (CLAUDE.md: any change to what the operator sees goes into the help, with its slice number). 07.10.2026. Text only.

## What changed and why
- `contabil/efactura/index.md`: the view «Vânzător» is gone; new section **«Date unitate»** (the K button of the title bar and its menu, the window «Date Unitate», its fields, the phone format, the series and first number that lock after the first invoice, «Salvează» / «Ieșire», «Preia de la ANAF» that works once; the accounts window «Conturi Unitate» from the same menu); «Vederea Generale» gains **Cont emitent (IBAN)**; the list of views in «Arborele facturilor» and step 4 of «O factură nouă» updated; keywords (`vanzator` → `date unitate`); `screens:` = `FacturiForm, DateUnitateForm`; tags `00EF-13, 0000-58` on every touched section; capture tag `date-unitate` (new).
- `contabil/efactura/conturi.md`: opened from «Date Unitate» (title-bar button), not from a tab; `screens:` = `ConturiForm` (the button `FacturiForm.btnVConturi` no longer exists); tags `00EF-13, 0000-58`; capture preparation updated.
- **New interactive tutorial** `tutorials/efactura-factura-noua.md` («Cum adaug o factură nouă (E-Factura)», `starts: KbotForm`, `host-key: efactura`): MENIU › E-Factura (optional opening steps), «Adăugare», client, vederea Conținut, «Linie nouă», completarea liniei, vederea Generale, cont emitent, «Salvare» (`guard: yes`), pointer to the invoice menu for sending. All the rest stays in the help topics. No other E-Factura tutorial existed. `index.md` mentions it in «O factură nouă».
- `help-version.txt`: `2026-10-07` (unchanged value).
- Nothing about where the data is kept (no table names, no server) went into the help: users only.

## Files touched
`src/KBot.App/HelpContent/contabil/efactura/{index,conturi}.md`, `src/KBot.App/HelpContent/tutorials/efactura-factura-noua.md`, `help-version.txt`, this file, `SLICE-00EF-13-date-unitate.md`, the status files.

## Test results
`Check-Help.ps1` NOT run (no scripts or builds unless asked). Links, tags and `screens:` written by hand.

## Unverified / deferred
- Run `tools\HelpCheck\Check-Help.ps1 -Coverage -Map` once (the `screens:` value `DateUnitateForm` is the likely complaint if the engine wants a registered window name).
- Captures for the operator: `date-unitate` (new, missing), `conturi-unitate` (re-take: the way to open it changed), `efactura-facturi` (the bottom bar has the new button).
- Windows never seen on screen. The tutorial was never run: the targets (`FacturiForm.cmbClient`, `navDetaliu` with `tab:continut` / `tab:generale`, `btnLinieNoua`, `gridLinii`, `cmbContPlata`, `btnSalveaza`) and the anchor `menu.efactura` are taken from the code by reading; a topic cannot link to a tutorial, so it is only mentioned in text.
