# SLICE-00EF-08 - Screen «E-Factura — facturi emise» (EFACTURA_ADD in K-BOT) (code written, builds clean, NEVER RUN, never seen on screen)

Slice 00EF (operator, 06.10.2026; «Pornește cu 00EF-08 acum», 07.10.2026). The window the operator did not find: the menu entry
«E-Factura» opened only the ANAF token window (00EF-05); the invoice screen of Access (`EFACTURA_ADD` + `EFACTURA_ADD_FACTURI`,
`EFACTURA_CLIENTI`, `EFACTURA_VANZATOR`, `EFACTURA_ADD_SUB`, `EFACTURA_UM`) did not exist. Sending to ANAF is 00EF-09, not here.

## What changed and why
- **New window `FacturiForm`** (`src/KBot.EFactura/Views/`, `KBotShellForm`, all controls in `FacturiForm.Designer.vb`, code split in four
  partials): the list of the unit's issued invoices on the left (`KBotDataView`: Număr, Data, Client, Total, Stare; newest first; column
  filters; refused rows in the error colour, sent-but-unconfirmed ones in the warning colour) and the chosen invoice on the right in five
  tabs, as in Access:
  - **Generale** - number, date (`KBotDatePicker`), type (380 / 384, server-owned, read only), state, comments (255), order reference BT-13 (30), total;
    a line of words about the invoice's situation (draft, accepted, why ANAF refused it, which invoice a storno cancels).
  - **Cumpărător** - combo of the unit's customers + the fields of `EF_Clienti` (CNP tick, «RO» prefix, fiscal code, name, county, city, sector,
    address, IBAN, bank). A customer is saved on its OWN («Salvează clientul», straight to the database; «Client nou», «Șterge clientul») and the
    invoice keeps only its id, as in Access. An invoice is not saved while the customer's fields hold unsaved changes.
  - **Vânzător** - the invoice's payment account («Cont emitent», a combo of the accounts already used, typed ones allowed) and the unit's issuer
    data (`GET/PUT /furnizor`) with their own «Modifică / Salvează / Renunță» buttons and Access's warning sentence. A unit with no issuer data
    opens straight on this tab and cannot add invoices until they are saved. `AfiseazaPrimiteNoi` (received invoices, a later slice) is carried
    through unchanged so a save does not reset it.
  - **Atașamente** - the «Atașează factura originală» tick (stored on the invoice).
  - **Conținut** - the lines in a `KBotDataView` typed into: Nr (read only), Conținut, Um (combo of the UN/ECE list, «COD — explanation», default XPP as in
    Access), Cant (3 decimals), PU (4), Valoare (computed as the operator types, half-up, the server's figure is the one stored), «✕» per line, footer
    total, «Linie nouă» button.
- **Footer**: «Ieșire», status line, «Token ANAF» (opens the 00EF-05 `TokenForm` as a dialog), «Ștergere», «Renunță», «Modificare», «Adăugare», «Salvare».
- **What may be done with an invoice comes from the server** (`poate_modifica`, `poate_sterge`, `stare`), never from a rule in the window:
  only a draft is modified; delete = a draft with the last number of its series; the others are shown read-only. New invoice: series and number are
  the server's, shown as «provizoriu» until the save.
- **`KBot.Api`**: `IEFacturaApi` extended + new `ApiClient.EFactura.Facturi.vb` (issuer, units of measure, customers, invoices: list, detail, next number,
  save = POST/PUT, delete). Wire names exactly the server's (`factura_routes.py`, case sensitive). FileVersion 1.0.20.0 → 1.0.21.0.
- **`KBot.Domain`**: `EFacturaFacturi.vb` (`EFacturaFurnizor`, `EFacturaClient`, `EFacturaUm`, `EFacturaLinie`, `EFacturaFactura`, `EFacturaStare`,
  `EFacturaNumarUrmator`). FileVersion 1.2.12.0 → 1.2.13.0. **`KBot.EFactura`** FileVersion 1.0.0.0 → 1.1.0.0.
- **`KBot.App`**: `KbotForm.EFactura.vb` now opens `FacturiForm` (same one-at-a-time, modeless rule); App FileVersion NOT bumped here (`push-update.ps1` asks).
- **Help** `0000-55` (see `SLICE-0000-55-ajutor-facturi-emise.md`): topic `contabil.efactura` rewritten around the new window, MENIU row and main-window tour text.

## Decisions taken here (state them, change if wrong)
- **Customers are saved separately from the invoice**, as in Access (its customer subform had its own Salvează). The alternative (one save for both) was not chosen.
- **«Modificare» is a separate step** (Access had bMod) and the fields are disabled otherwise; an unchanged edit does not ask on leaving.
- **The list is the unit's last 500 invoices** (the server default), filtered by the grid's column filters; there is no year selector or search box (the
  routes have `an=` and `q=`; the API methods take them, the window passes nothing).
- **Per-customer county and issuer county are a fixed list of the 41 counties + București** (the same codes the server accepts, `validare.COUNTIES`),
  carried in the window; Access read them from its `Jud` table. A code that is not in the list (migrated data) is kept, not lost.
- **Unit of measure**: from the server list; a typed code that is not in the list is refused; when the list cannot be read any typed code is accepted
  (a warning notice says so). Default XPP (Access `def="XPP"`).
- **Number input**: comma or point as the decimal mark, never a thousands separator («1.234» is 1.234).
- **The «Cont emitent» of a new invoice** starts as the account of the newest invoice (Access had no such default that I read; unverified what it did).
- **Line numbers** (`NrCrt`): kept as they are; a new line gets the highest numeric one + 1.

## Files touched
New: `src/KBot.EFactura/Views/FacturiForm.{vb,Designer.vb}`, `FacturiForm.{Client,Furnizor,Linii}.vb`, `src/KBot.Api/ApiClient.EFactura.Facturi.vb`,
`src/KBot.Domain/EFacturaFacturi.vb`, this file. Edited: `src/KBot.Api/IEFacturaApi.vb`, the three `.vbproj` FileVersions,
`src/KBot.App/KbotForm.EFactura.vb`, help files (see 0000-55), `PLAN_00EF_EFactura.md`, `state/KBOT_STATUS_0000-0009.md`, `KBOT_STATUS.md`.

## Test results
`dotnet build` of `KBot.Api`, `KBot.EFactura` and `KBot.App` (Debug): **0 errors, 0 warnings**. `Check-Help.ps1 -Coverage`: the only errors left
are the old ones in `tutorials\ordonantare-din-plata.md` and the «slice 0000-55 is not in KBOT_STATUS.md» ones, cleared by the status rows written
with this file. **No test code written and no test run** (standing rule), the app was not started: **never run, never seen on screen**.

## Unverified / deferred
- **Everything on screen**: the layout of the five tabs (row heights, wrapping of the long labels, the 38/62 split, the list columns), the theme and DPI
  behaviour, the designer was never opened in Visual Studio. Designer coordinates are written at 96 dpi (`AutoScaleDimensions = 96,96`).
- **Everything against a server**: the routes of 00EF-06 are not deployed (the operator reports only the token refresh works), the DDL of 00EF-02 +
  00EF-06 has not been run, `EF_UM` has no rows. Until then the window shows the server's error in the yellow/red notice at the top of the right side
  (route missing, or «Tabelele E-Factura ale unității lipsesc sau sunt vechi. Rulați …»). Nothing here was exercised against real data.
- **KBotDataView details assumed from its doc**: Number cells hold `Decimal` and format with `DecimalPlaces`; `CellValidating` may rewrite
  `ProposedValue` to a `Decimal` / list object; the footer sum of `valoare` shows; `RowFormatting` colours a row; setting `CurrentRowIndex` raises
  `SelectionChanged` (the window ignores it while it fills the list). The Um column holds 2113 combo entries (find as you type): speed not seen.
- `KBotComboBox` (Editable + LimitToList) for customer / county: typing, `Text` setter selecting the matching item, and `TextChanged` on the account combo
  firing on typing are assumed from `KBotComboBox.md`; if the last one does not, the account change would not mark the invoice as changed.
- **IBAN picker of Access (`EFACTURA_PickIBAN`, accounts from `Clasificatii`)**: not rebuilt; the account is a combo of the accounts used before, or typed.
- **«Atașează factura originală»**: stored, but `ubl.py` / `trimitere.py` do not read `AtasamentOriginal`; whether and how the original is attached is
  an open question for 00EF-09.
- **What the window does NOT do (00EF-09)**: generate / validate / send, check the state at ANAF, correct (384) or cancel (storno) an accepted invoice,
  download the answer. The server has the routes; the buttons and the confirmations are the next slice.
- A start-up notice about the token ending soon is still not built (open since 00EF-05).
