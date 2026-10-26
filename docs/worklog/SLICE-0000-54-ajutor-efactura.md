# SLICE-0000-54…60 — Ajutorul autorizării, emiterii și datelor unității

În 17.10, 94 mută atașamentul în vederea Conținut și retrage vederea separată Atașamente; capturile anterioare pot rămâne în urmă.

Șase etape 54/55/56/57/58/60 din 06–09.10.2026. Tokenul devine subiect propriu, lista devine arbore, iar trimiterea/corecția și datele unității apar în etape ulterioare. „Nu se trimite încă”, „preluare ANAF o singură dată” și amplasarea comenzilor sunt checkpointuri amendate în istoricele [emiterii](SLICE-00EF-08-ecran-facturi-emise.md) și [unității](SLICE-00EF-13-date-unitate.md). Capturile, etichetele și rezultatele rămân pe etapă; ajutorul runtime nu se modifică.

- [0000-54](#slice-0000-54)
- [0000-55](#slice-0000-55)
- [0000-56](#slice-0000-56)
- [0000-57](#slice-0000-57)
- [0000-58](#slice-0000-58)
- [0000-60](#slice-0000-60)
- [0000-94](#slice-0000-94)

## SLICE-0000-54 - Help for 00EF-05: the «E-Factura» menu row and the token window (text; not seen on screen) <a id="slice-0000-54"></a>

Help sub-slice (operator rule 30.09.2026: any visual change goes into the help with its slice number).

### What changed and why
- New topic `contabil.efactura` (`HelpContent/contabil/efactura/index.md`, order 75 under «Despre K-BOT»): what the window shows, how to get or renew the token (connect the token, «Autorizează» / «Reînnoiește tokenul», pick the certificate, PIN, result), and the messages the operator can meet. `screens:` `TokenForm`, `CertificatePickerForm`; `open: menu:efactura`. Quotes are the exact captions / messages of the code (`TokenForm.vb`, `TokenForm.Designer.vb`, `CertificatePickerForm.Designer.vb`, `TokenAuthorizer.vb`, `AnafAuthorizeClient.vb`).
- `contabil.fereastra`, section MENIU: new row «E-Factura» with a link; `tur-fereastra`, step «Butonul MENIU»: the sentence names the new window.
- `HelpContent/README.md`: `efactura` added to the `menu:` keys. No change to `KbotForm.HelpCapture.vb`: `menu:` already finds any row by key.
- Left out on purpose (users only): how the server keeps and renews the token, the code exchange, the certificate rules' internals.

### Files touched
New: `HelpContent/contabil/efactura/index.md`, this file. Edited: `HelpContent/contabil/fereastra.md`, `HelpContent/tours/tur-fereastra.md`, `HelpContent/README.md`, `docs/worklog/state/KBOT_STATUS_0000-0009.md`, `docs/worklog/KBOT_STATUS.md`.

### Test results
`tools\HelpCheck\Check-Help.ps1 -Coverage`: see the last run in the 00EF-05 report; the only remaining errors are the pre-existing ones in `tutorials\ordonantare-din-plata.md` (missing slice tags, not touched here). The `dotnet build` of `KBot.App` is clean.

### Unverified / deferred
- **Capturi noi, lipsă:** `efactura-token` (window with a unit that already has a token; prepare text in the tag). The certificate picker has no capture (it needs a real token).
- Nothing was run: the text is written from the code, not from the window on screen.
- The help watermark (`help-version.txt`) stays `2026-10-06` (same day as 0000-53).

## SLICE-0000-55 - Ajutor pentru 00EF-08: fereastra «E-Factura — facturi emise» <a id="slice-0000-55"></a>

Help sub-slice (CLAUDE.md: any change to what the operator sees goes into the help, with its slice number). 07.10.2026.

### What changed and why
- `contabil/efactura/index.md` rewritten around the new window: «Lista facturilor» (columns, states, colours), «O factură nouă» (steps, what K-BOT refuses to save, Renunță, Modificare, Ștergere), one section per tab (Generale, Cumpărător, Vânzător, Atașamente, Conținut) and «Tokenul ANAF» (the old token sections, now reached from the «Token ANAF» button). Title `E-Factura: facturile emise și tokenul ANAF`; `screens:` now `FacturiForm, TokenForm, CertificatePickerForm`; keywords widened (factură, client, vânzător, cont emitent, IBAN, unitate de măsură, ciornă ...). Tags `00EF-08, 0000-55`.
- `contabil/fereastra.md` (row «E-Factura» of MENIU) and `tours/tur-fereastra.md` (step «Butonul MENIU»): the entry no longer opens the token window.
- `help-version.txt` = `2026-10-07`. The help says plainly that sending to ANAF, checking the state and correcting / cancelling an accepted invoice are not done from this window yet (a user-visible fact).

### Files touched
`src/KBot.App/HelpContent/contabil/efactura/index.md`, `contabil/fereastra.md`, `tours/tur-fereastra.md`, `help-version.txt`; status files.

### Test results
`Check-Help.ps1 -Coverage`: no error about these files except the slice id `0000-55` before the status rows existed (written with this file); the coverage list no longer names `FacturiForm` (it is in `screens:`). The old errors of `tutorials\ordonantare-din-plata.md` remain (not touched). The help was not opened in the app.

### Capturi
- **New, missing:** `efactura-facturi` (the new window; `goto: menu:efactura`, prepare: a unit with a few invoices, one chosen).
- **Changed:** `efactura-token` no longer has `goto:` (the menu now opens the invoice window): its `prepare:` says to press «Token ANAF». The picture never existed.

### Unverified / deferred (de citit de operator)
- All wording comes from the code of `FacturiForm` (captions, tooltips, messages), not from the window on screen: the window was never run. Read the sections against it. In particular «rândul are culoarea de avertisment / de eroare» (the colours come from the theme) and the sentence about what K-BOT asks before saving.

## SLICE-0000-56 - Ajutor pentru 00EF-12 si pentru token: conturile unitatii emitente, obtinerea / reinnoirea tokenului <a id="slice-0000-56"></a>

Help sub-slice (CLAUDE.md: any change to what the operator sees goes into the help, with its slice number). 07.10.2026. Help only: no tutorial (operator's request).

### What changed and why
- **New topic `contabil.efactura.conturi`** (`contabil/efactura/conturi.md`): the window «E-Factura — conturile unității» (opened by «Conturile unității…» in the «Vânzător» tab): what the two columns show (the bank is filled by K-BOT from the bank code inside the IBAN, «— banca nu este în listă —» when unknown), «Cont nou», «✕», «Salvează», «Închide» with the unsaved-rows question, what is refused (empty row, wrong IBAN, repeated account, more than 50), the «tables missing» message. Tags `00EF-12, 0000-56`; `screens: ConturiForm, FacturiForm.btnVConturi`; capture tag `conturi-unitate`.
- **New topic `contabil.efactura.token`** (`contabil/efactura/token.md`): getting and renewing the ANAF token, moved out of `contabil.efactura` (text of 00EF-05 / 0000-54 kept, one sentence added: renew in good time, after full expiry the authorisation starts again): what the window shows, the steps with the certificate and the PIN, what can happen. `screens: TokenForm, CertificatePickerForm, FacturiForm.btnToken`; the capture tag `efactura-token` moved with the text.
- `contabil/efactura/index.md`: the token sections replaced by a short «Tokenul ANAF» pointer with a link; the accounts paragraph of «Fila Vânzător» shortened to a link; `screens:` reduced to `FacturiForm`; tags updated (`00EF-12`, `0000-56`).
- `help-version.txt` not changed (already `2026-10-07`).

### Files touched
`src/KBot.App/HelpContent/contabil/efactura/{index,token,conturi}.md`, this file, [00EF-12](SLICE-00EF-13-date-unitate.md#slice-00EF-12), the status files.

### Test results
Nothing was run for this step (operator: no build, no tests, no git). `Check-Help.ps1` was NOT run after the split: the new topics, their `parent`, `screens`, tags and the two `topic:` links were written by hand and are unchecked. Earlier, before the split, it had reported only the missing worklog of `0000-56` for `contabil.efactura` (now written).

### Unverified / deferred
- Run `tools\HelpCheck\Check-Help.ps1 -Coverage -Map` once: the `order` values 76 / 77, the `FacturiForm.btnToken` / `FacturiForm.btnVConturi` screen keys and the moved capture tag are the likely places for a complaint.
- Captures to make by the operator: `conturi-unitate` (new); `efactura-token` (unchanged, now in the token topic).
- Text only; the windows were never seen on screen.

## SLICE-0000-57 - Ajutor pentru 00EF-09: arborele facturilor, meniul facturii, vederile cu document <a id="slice-0000-57"></a>

Help sub-slice (CLAUDE.md: any change to what the operator sees goes into the help, with its slice number). 07.10.2026.

### What changed and why
- `contabil/efactura/index.md`:
  - «Lista facturilor» → **«Arborele facturilor»** (clients and invoices, the dot colours, the three-dots sign, the search magnifier);
  - new sections **«Meniul unei facturi»** (table: state → items), **«Trimiterea la ANAF»**, **«Stornarea unei facturi»**, **«Factură PDF, Factură ANAF și Eroare ANAF»**;
  - the five «Fila X» sections became **«Vederea X»** (the window has a bar of views, not tabs); «O factură nouă» and the Generale section state the date rule (new invoice or the last of the series, never before the previous invoice);
  - the sentence «trimiterea … nu se fac încă» is gone (it is done now);
  - slice tags `00EF-09, 0000-57` on every section touched; keywords widened (arbore, trimitere, validare, stornare, storno, factura clasica, pdf, eroare anaf).
- `help-version.txt` unchanged (`2026-10-07`, already that day).

### Files touched
`src/KBot.App/HelpContent/contabil/efactura/index.md`; status files.

### Test results
`tools\HelpCheck\Check-Help.ps1 -Coverage`: no error about `contabil/efactura/index.md` or about 0000-57; the only errors left are the old ones of `tutorials\ordonantare-din-plata.md` (not touched). The help was not opened in the app.

### Capturi
- **New, missing:** `efactura-meniu-factura` (the menu of an accepted invoice; `goto: menu:efactura`, prepare: hover an accepted invoice, press the three dots).
- **Changed:** `efactura-facturi` — the window is now the tree + the views bar; the picture never existed, only its prepare text changed.
- No capture for the PDF views (they need Adobe and an accepted invoice at ANAF).

### Unverified / deferred
- All wording comes from the code of `FacturiForm` (captions, menu items, messages), not from the window on screen: it was never run. Read the new sections against it. In particular the colour words (grey / orange / green / red come from the theme palette) and the sentence about «Factură ANAF» needing a token and a connection.
- The sections of 0000-56 (accounts) were not touched.

## SLICE-0000-58 - Ajutor pentru 00EF-13: fereastra «Date Unitate» <a id="slice-0000-58"></a>

Help sub-slice (CLAUDE.md: any change to what the operator sees goes into the help, with its slice number). 07.10.2026. Text only.

### What changed and why
- `contabil/efactura/index.md`: the view «Vânzător» is gone; new section **«Date unitate»** (the K button of the title bar and its menu, the window «Date Unitate», its fields, the phone format, the series and first number that lock after the first invoice, «Salvează» / «Ieșire», «Preia de la ANAF» that works once; the accounts window «Conturi Unitate» from the same menu); «Vederea Generale» gains **Cont emitent (IBAN)**; the list of views in «Arborele facturilor» and step 4 of «O factură nouă» updated; keywords (`vanzator` → `date unitate`); `screens:` = `FacturiForm, DateUnitateForm`; tags `00EF-13, 0000-58` on every touched section; capture tag `date-unitate` (new).
- `contabil/efactura/conturi.md`: opened from «Date Unitate» (title-bar button), not from a tab; `screens:` = `ConturiForm` (the button `FacturiForm.btnVConturi` no longer exists); tags `00EF-13, 0000-58`; capture preparation updated.
- **New interactive tutorial** `tutorials/efactura-factura-noua.md` («Cum adaug o factură nouă (E-Factura)», `starts: KbotForm`, `host-key: efactura`): MENIU › E-Factura (optional opening steps), «Adăugare», client, vederea Conținut, «Linie nouă», completarea liniei, vederea Generale, cont emitent, «Salvare» (`guard: yes`), pointer to the invoice menu for sending. All the rest stays in the help topics. No other E-Factura tutorial existed. `index.md` mentions it in «O factură nouă».
- `help-version.txt`: `2026-10-07` (unchanged value).
- Nothing about where the data is kept (no table names, no server) went into the help: users only.

### Files touched
`src/KBot.App/HelpContent/contabil/efactura/{index,conturi}.md`, `src/KBot.App/HelpContent/tutorials/efactura-factura-noua.md`, `help-version.txt`, this file, `SLICE-00EF-13-date-unitate.md`, the status files.

### Test results
`Check-Help.ps1` NOT run (no scripts or builds unless asked). Links, tags and `screens:` written by hand.

### Unverified / deferred
- Run `tools\HelpCheck\Check-Help.ps1 -Coverage -Map` once (the `screens:` value `DateUnitateForm` is the likely complaint if the engine wants a registered window name).
- Captures for the operator: `date-unitate` (new, missing), `conturi-unitate` (re-take: the way to open it changed), `efactura-facturi` (the bottom bar has the new button).
- Windows never seen on screen. The tutorial was never run: the targets (`FacturiForm.cmbClient`, `navDetaliu` with `tab:continut` / `tab:generale`, `btnLinieNoua`, `gridLinii`, `cmbContPlata`, `btnSalveaza`) and the anchor `menu.efactura` are taken from the code by reading; a topic cannot link to a tutorial, so it is only mentioned in text.

## SLICE 0000-60 — help for 00EF-16 (09.10.2026) <a id="slice-0000-60"></a>

### What changed and why
Section «Corectarea unei facturi acceptate» (the 384 correction); the months and the year in «Arborele facturilor»; the unit's own accounts at «Cont emitent»; the attached original PDF at «Trimiterea la ANAF» and «Vederea Atasamente». Written in the 00EF-16 thread; this worklog was missing, which the help checker reported.

Topic: `contabil.efactura` (`src/KBot.App/HelpContent/contabil/efactura/index.md`). Tags: `00EF-16, 0000-60`. Help is for users only: nothing about the server, the tables or the logs. Not seen on screen; no capture.

## SLICE-0000-94 - Help for 0114-23 (17.10.2026) <a id="slice-0000-94"></a>

### What changed and why
`contabil/efactura/index.md`: the list of views of an issued invoice no longer has «Atasamente»; the section «Vederea Atasamente» is gone and its text (the tick «Ataseaza factura originala») is a bullet of «Vederea Continut», in the footer of the view.

### Files touched
`src/KBot.App/HelpContent/contabil/efactura/index.md`.

### Test results
`tools\HelpCheck\Check-Help.ps1`: see below in the status row.

### Unverified / deferred
Nothing seen on screen. Captures that show the old tab bar of the issued invoices may need a redo.
