# SLICE-0129-01 - Culorile ferestrelor, grilelor si arborilor vin din tema, nu din designer (07.11.2026)

## Request
Operator, 07.11.2026: BackColor pentru orice view/form = «Control»; BackColor pentru controale = «Window»;
BorderColor pentru controale si view-uri = «Active Border». Clarificare: acestea sunt valorile din Classic,
dar trebuie sa se schimbe odata cu tema.

## What changed and why
Designerele fixau pe fiecare grila / arbore / lista / rich-textbox valoarea de Classic
(`BackColor = SystemColors.Window`, `BorderColor = SystemColors.ActiveBorder`). Un control cu culoarea fixata
nu mai urmeaza tema (arborele are `_backColorPinned`), deci ramanea alb in Modern si Dark.

Scoase din `.Designer.vb` (137 de linii, 70 de fisiere), fiecare control revenind la «din tema» (`Color.Empty`):
1. 93 x `BackColor = SystemColors.Window` (grile, arbori, liste, rich-textbox, date picker, previewuri).
2. 45 x `BorderColor = SystemColors.ActiveBorder` (grile, arbori, liste, `FlatAppearance` pe butoanele din
   `ForexeFooterView`).
3. `SystemColors.Control` fixat si fundaluri fixe alb / WhiteSmoke din `HistoryForm`, `CertificateSelectionForm`
   (`pnlBottom`, `lblTitle`, `btnClose`, `btnRefresh`) si `DevHarnessForm.rtbResults`.

Rezultatul in tema: fereastra / view = `Surface` (Control), controale = `SurfaceAlt` / `InputBack` (Window),
chenare = `Border` (ActiveBorder). Classic are deja exact aceste valori (#F0F0F0 / #FFFFFF / #B4B4B4);
`BuiltInSchemes` nu s-a schimbat. Scriptul de curatare a fost un script node de unica folosinta (nepastrat).

## Files touched
70 `*.Designer.vb` sub `src/KBot.App`, `src/KBot.EFactura`, `src/KBot.Forexe`, `src/KBot.Controls`
(`Xml/XmlViz`), `src/KBot.DevHarness`. Neatinse: `_reference/`, `KBot.Migrator`, `AVC.Migrator`.

## Test results
Nici un test scris sau rulat. `dotnet build KBot.sln`: 0 avertismente, 0 erori.

## Unverified / deferred
- Nimic vazut pe ecran: aspectul in Classic / Modern / Dark / Colorful nu a fost comparat.
- `pnlHeader` din `SumarView` si `KbotForm` ramane `Window` fixat (header alb ales din design).
- Raman culori proprii intentionate: butoanele portocaliu / albastru din `CertificateSelectionForm`,
  bannerul rosu din `SimulatedForexePage`, fereastra de debug `FrmNodeDebug`.
- Culorile fixate direct in cod `.vb` (nu in designere) nu au fost cautate.
- Ajutor: 0000-166.
