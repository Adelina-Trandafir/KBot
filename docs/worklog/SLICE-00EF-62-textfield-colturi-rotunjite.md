# SLICE-00EF-62 — KBotTextField: arc de colț ca laturile pe tema Modern (08.11.2026)

Ultimul slice care a atins direct controlul: 00EF-37 (o singură sursă pentru raza colțului). Aceeași cerere ca la 00EF-61.

## What changed and why
`OnPaint` desena cadrul pe coordonate întregi, cu arcul mai subțire și mai palid decât laturile. Acum cadrul e pe jumătăți de pixel (`PixelOffsetMode.Half` + `ThemeShapes.FrameBounds` / `RoundedRectF`, vezi 00EF-61); modul revine la implicit înainte de ochiul de la parolă, ca acela să nu se mute.
Nu are nevoie de culoarea din spate: `BackColor` e `Transparent`, colțul vine de la părinte.

## Files touched
`src/KBot.Controls/TextField/KBotTextField.vb` (versiunea proiectului: vezi 00EF-61).

## Test results
Build KBot.Controls: 0 erori, 0 avertismente. Niciun test scris sau rulat. Probă pe ecran real la 150% DPI: linie de 1 px (2 px la focus) întreagă pe toate laturile, arc de aceeași grosime. **Văzut pe ecran de operator: corect.**

## Unverified / deferred
Ochiul de parolă (modul implicit restabilit după cadru) nu a fost privit separat pe ecran.
