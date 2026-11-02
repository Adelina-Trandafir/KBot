# SLICE-0010-10 - KBotDataView: barele de derulare sunt KBotScrollBar, urmeaza tema (02.11.2026)

Cererea operatorului: barele grilei sa urmeze tema dark ca la arbore. Sub-felie 0010-10 (0010-09 era ultimul numar). **Inchisa: operatorul a confirmat ca arata bine, rulat si testat (02.11.2026).**

## What it is now
- `vScroll` / `hScroll` din `KBotDataView.Designer.vb` sunt `KBotScrollBar` (vertical / orizontal), ca la `AdvancedTreeControl` si `KBotTextBox`; nu mai exista bare native in grila. Semantica `Minimum` / `Maximum` / `LargeChange` / `Value` e cea a barei native, deci logica de derulare (virtualizare, scroll pe coloana, snap) e neatinsa.
- `ApplyTheme(scheme)` da schema direct barelor (`vScroll.ApplyTheme`, `hScroll.ApplyTheme`).
- Grosimea vine din `ScrollBarThicknessPx()` = `ScaleDpi(KBotScrollBar.GrosimeImplicita)` (12 px logici), folosita in `KBotDataView.Layout.vb` (`UpdateScrollBars`, `ViewportWidth`) si `KBotDataView.AutoSize.vb`. Rotita grilei (`OnMouseWheel`) lucreaza pe `KBotScrollBar`.
- KBot.Controls 1.78.0.0. Ajutor: nicio sectiune nu descrie barele grilei, nimic de etichetat. `KBotDataView.md` actualizat.

## Trap
Barele native nu pot lua culorile paletei (le deseneaza Windows; `SetWindowTheme` da doar griul sistemului). Orice bara noua intr-un control propriu = `KBotScrollBar`, nu `VScrollBar` / `HScrollBar`. Un test care cauta bara dupa tip (`TypeOf c Is VScrollBar`) nu o mai gaseste.

## Files touched
`KBot.Controls/DataView/KBotDataView.Designer.vb`, `.Layout.vb`, `.AutoSize.vb`, `.Theming.vb`, `KBotDataView.md`; `KBot.Controls.vbproj`; `tests/KBot.Controls.Tests/KBotDataViewAutoSizeTests.vb` (`VScrollVisible` cauta `KBotScrollBar` vertical); `state/KBOT_STATUS_0010-0019.md`.

## Test results
Build `KBot.Controls` si `KBot.Controls.Tests`: 0 avertismente, 0 erori. Rulat si verificat pe ecran de operator.

## Unverified / deferred
Nimic cunoscut.
