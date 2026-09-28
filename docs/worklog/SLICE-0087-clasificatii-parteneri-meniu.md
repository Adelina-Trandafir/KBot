# SLICE-0087 — «Clasificații bugetare», «Parteneri», KBotDropDownMenu, header menu

Operator request, 26.09.2026:

1. A standalone window for adding / editing classifications (like the Access budget screen): tree
   Capitol > Subcapitol > Articol > Alineat with names as a second column; on the right a one-row
   budget grid (Trim 1-4 editable, Total = sum, read-only) and a corrections grid (same columns +
   Nr. doc + Data, footer totals, footer right «+» adds a row edited in place); «Salvează» writes
   to MariaDB; «+» in the tree footer opens a popup that adds classifications the way the
   registration page does.
2. A window for adding / editing partners like the Access form, tree on the left, no «Burse» box.
3. A new drop-down menu control, Windows 10 style (coloured icon bar down the left), submenus,
   per-item height / icon / formatted text, items edited as a collection in the designer.
4. The new windows and «Angajament nou» reached from that menu, opened by a button that replaces
   `btnAngajamentNou`.

## What changed and why

### 0087-01 Clasificații bugetare
- **Server** `PYTHON/routes/forexe/clasificatii_edit.py` (bearer, `require_session`):
  `GET /api/forexe/nomenclatoare/clasificatii` (all rows of the session DB + level names from
  `DefaClsfF` / `DefaArticol` / `DefaSursaSector`), `GET|POST .../clasificatii/<id>/buget?an=`
  (budget upsert on `uq_clasificatii_buget_idclsf_an` + corrections update / insert / delete in
  ONE transaction; a correction's date must be in the year — `Clasificatii_Rectificari` has no
  year column, the year is `YEAR(Data)`; duplicate (IdClsf, Data, Document) → 409 in Romanian),
  `GET .../clasificatii/nomenclator?an=` and `POST .../clasificatii/adauga`.
- **Adding reuses the registration code as is**: the lists are
  `routes/inregistrare/nomenclatoare.read_clasificatii / read_group_captions`, the rows and every
  foreign-key check are `routes/inregistrare/randuri.build`. Only sector-sources that already have
  a unit in the DB can be chosen (a new SS is a new unit = the registration's job). Rows already
  present (same unit + capitol + subcapitol + articol + alineat) are skipped and counted.
- **Window** `Views/Nomenclatoare/ClasificatiiForm` (KBotShellForm, modeless, maximizable). Tree
  grouped by (Capitol, SS) because «65.02» is shared by 02A / 02C / 02D / 02G. Unsaved changes are
  asked about (Yes / No / Cancel) on another node and on close.
- **Popup** `ClasificatiiAddForm` (KBotThemedForm): steps 3 and 4 of `inregistrare.html` — SS
  check boxes, functional and economic check-trees built by `ClassificationCodeTree` (a VB port of
  `static/js/inregistrare/tree-builder.js`, same leaves the server accepts).

### 0087-02 Parteneri
- **Server** `PYTHON/routes/forexe/parteneri_edit.py`: `GET` (partners + codes + Tip values +
  classifications for the combo), `POST` (partner + codes in one transaction; new partner goes into
  the unit of the working SS), `DELETE` — refused (409) for a partner used on documents, because
  `FX_DDF_REV_SA/SB` and `FX_ORD_TBL` reference `Parteneri` with ON DELETE CASCADE.
- «Activity» = IdPartener on `FX_DDF_REV_SA`, `FX_DDF_REV_SB`, `FX_ORD_TBL`, or CodFiscal on
  `FX_DDF` (every link the schema has; the Access rule was not in the export).
- **Window** `ParteneriForm`: tree (code | name columns, search), filters «Arată partenerii
  ascunși» / «Ascunde partenerii fără activitate», details, «Coduri angajament» grid (combo of the
  unit's classifications, «+» / «✕»), Ieșire / Ștergere / Renunță / Adăugare / Salvare.
  «Adăugare» proposes the next free numeric code.

### 0087-03 KBotDropDownMenu (KBot.Controls/Menu/)
New component + `KBotMenuItem` collection (nested for submenus), `KBotMenuWindow`. Windows never
activate; a message filter reads the keys and closes on outside clicks. Doc:
`src/KBot.Controls/Menu/KBotDropDownMenu.md`. Not an extension of `CustomPopup`: that one
activates (it needs focus for its keyboard), which rules out several open levels.

Small generic additions to existing controls, needed by the windows:
- `KBotDataView.FooterRightIcon` (+ tooltip, event, rect) — its own corner square; `RemoveRowAt`.
- `AdvancedTreeControl.RightTextColumn` — right part of a `~~~` caption at a fixed X on every
  row, left-aligned (a real name column across levels).

### 0087-04 Header menu
`btnAngajamentNou` replaced by `btnMeniu` («Meniu ▾», primary style) + `menuNou`
(KBotDropDownMenu, items in `KbotForm.Designer.vb`): Angajament nou | — | Nomenclatoare ▸
(Clasificații bugetare, Parteneri). `KbotForm.Nomenclatoare.vb` dispatches; each window opens
once (a second request brings it to the front). `ReauthGate` hands the shell's 401 net to the
windows for any response type.

## Files touched
- new: `PYTHON/routes/forexe/clasificatii_edit.py`, `PYTHON/routes/forexe/parteneri_edit.py`
- `PYTHON/routes/forexe/__init__.py` (imports)
- new: `src/KBot.Domain/Nomenclatoare.vb`, `src/KBot.Domain/ClassificationCodeTree.vb`
- new: `src/KBot.Api/INomenclatoareApi.vb`, `src/KBot.Api/ApiClient.Nomenclatoare.vb`
- new: `src/KBot.Controls/Menu/{KBotDropDownMenu.vb, KBotDropDownMenu.Input.vb, KBotMenuItem.vb, KBotMenuWindow.vb, KBotDropDownMenu.md}`
- new: `src/KBot.Controls/DataView/KBotDataView.FooterRightIcon.vb`
- `src/KBot.Controls/DataView/{KBotDataView.vb, .Collapse.vb, .Painting.vb, .Input.vb, KBotDataView.md}`
- `src/KBot.Controls/Tree/{AdvancedTreeControl.Painting.vb, .Properties.vb, AdvancedTreeControl.md}`
- `src/KBot.Controls/{CONTROLS.md, README.md, KBot.Controls.vbproj}`
- new: `src/KBot.App/Views/Nomenclatoare/{ClasificatiiForm, ClasificatiiAddForm, ParteneriForm}(.Designer).vb`, `ReauthGate.vb`
- new: `src/KBot.App/KbotForm.Nomenclatoare.vb`
- `src/KBot.App/{KbotForm.Designer.vb, KbotForm.Ddf.vb, KbotForm.Chrome.vb, KBot.App.vbproj}`
- FileVersion: Domain 1.2.6.0, Api 1.0.12.0, Controls 1.53.0.0 (App left to push-update.ps1)

## Test results
- Build App (pulls Domain, Api, Controls, Theming): **0 warnings, 0 errors**.
- `py_compile` of the two route files and `__init__.py`: OK.
- Rendered off-screen with `DrawToBitmap` (scratch program, fake data, Classic + Dark): the
  Clasificații window with a leaf selected, the add popup, the Parteneri window, the menu with an
  open submenu. All four looked right; the budget grid height was trimmed after the first render.
- No tests written, run or built (operator's rule).

## Left unverified or deferred
- **Nothing ran against the server or MariaDB**: routes, SQL, the transaction and the 409 paths
  are untested live. The Flask app was not restarted.
- Nothing clicked through on screen: menu keyboard, outside-click closing, submenu timing, grid
  editing, the dirty / save-on-close flow.
- The Parteneri Access form was not in `C:\AVACONT\FX_System_Export`: «Alte detalii» is mapped to
  `Parteneri.Adresa`; «Cod Client (OP)» and «Solduri inițiale» have no column / table and were left
  out; «Exportă în Excel» not done. Meaning of `Tip` values unknown (combo = values in use).
- Tree levels follow the request (Capitol, Subcapitol, Articol, Alineat) — the Access screenshot
  also had a Titlu level; not added.
- «Names as a new column» read as a second column in the tree, not new DB columns.
