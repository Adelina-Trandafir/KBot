# SLICE-0129-02 - Panel: fundalul implicit dupa tema, ca la KBotTableLayoutPanel (08.11.2026)

Ultimul slice care a atins direct panourile: 0129-01 (fundaluri fixate scoase din designere, inclusiv `pnlHeader`). Cerere: operatorul vrea ca orice `Panel` sa urmeze culorile ca tabelul editat in [0066-03](SLICE-0066-03-tabel-culoare-fundal-din-tema.md).

## What changed and why
Regula generica de `Panel` din `ThemeManager` (fundalul unui panel care NU e `Tag = "Card"` si nu are culoare pusa de mana) este acum aceeasi cu cea a `KBotTableLayoutPanel`:
- Classic (`StyleSystem`): `SystemColors.Window` (inainte `Control`);
- Dark: `SurfaceAlt` (locul lui «Window» in Dark; inainte `Surface`);
- Modern si orice alta schema: `Surface` (neschimbat);
- (`Card`, `SplitContainer`, `TableLayoutPanel`, `GroupBox`: vezi 0129-03, aceeasi regula.)

Tot aici, din 0129-01: `BackColor = SystemColors.Window` scos din `pnlHeader` (`KbotForm.Designer.vb`, `SumarView.Designer.vb`), ca sa urmeze tema.

## Files touched
`src/KBot.Theming/ThemeManager.vb` (`StylePalette`, `StyleSystem`: ramura `Panel`); `src/KBot.App/KBotForm/KbotForm.Designer.vb`, `src/KBot.App/KBotForm/Views/SumarView.Designer.vb`, `SumarView.vb` (`ApplyTheme`), `ThemeScheme.vb` (`PanelBack`) si cele 17 fisiere de mai jos.

## Test results
`KBot.Theming` se compileaza: 0 avertismente, 0 erori. **Verificat pe ecran si testat de operator: corect.** Nicio suita de teste rulata.

## Unverified / deferred
Nothing open. The Panel rule, the `SumarView` / `pnlHeader` follow-up and the sweep of code-set panel colours onto `ThemeScheme.PanelBack()` (Classic Window, Dark SurfaceAlt, rest Surface) were seen on screen and approved by the operator, 08.11.2026. Later superseded in part by [0129-03](SLICE-0129-03-toate-containerele-acelasi-fundal.md): a `Tag = "Card"` panel, `SplitContainer`, `TableLayoutPanel`, `GroupBox`, views / pages and their labels now take `PanelBack()` too. Help: nothing to write (operator).
