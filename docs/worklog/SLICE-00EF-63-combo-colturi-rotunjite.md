# SLICE-00EF-63 — KBotComboBox: colțuri rotunjite curate pe tema Modern (08.11.2026)

Ultimul slice care a atins direct controlul: 00EF-46 (grupe pliabile în listă). Aceeași cerere ca la 00EF-61.

## What changed and why
Aceeași cauză ca la `KBotTextBox` (00EF-61): `BackColor` opac, pasul de fundal umple tot dreptunghiul, colțurile din afara arcului rămâneau albe. În `OnPaint` (ramura fără `_cellEditorMode`):
- `g.Clear(ThemeShapes.BackdropColor(Me))` înainte de arc;
- cadrul pe jumătăți de pixel (`PixelOffsetMode.Half`, `FrameBounds(Size, 1)`, `RoundedRectF`), cu modul readus la implicit înainte de săgeată;
- `OnParentBackColorChanged` → `Invalidate()`.
Caseta interioară a combo-ului nu a avut nevoie de decupare: stă în zona de text, departe de colțuri.

## Files touched
`src/KBot.Controls/Combo/KBotComboBox.vb`, `KBotComboBox.md` neschimbat (versiunea proiectului: vezi 00EF-61).

## Test results
Build KBot.Controls: 0 erori, 0 avertismente. Niciun test scris sau rulat. Captură reală de ecran la 150% DPI: niciun pixel alb pur în afara arcului, colțul are culoarea paginii. **Văzut pe ecran de operator: corect.**

## Unverified / deferred
Modul `_cellEditorMode` (combo în celula grilei) nu a fost atins și nu a fost privit.
