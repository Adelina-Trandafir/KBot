# SLICE-00EF-64 — KBotButton: arc de colț ca laturile pe tema Modern (08.11.2026)

Ultimul slice care a atins direct controlul: 00EF-37 (raza colțului din temă). Aceeași cerere ca la 00EF-61.

## What changed and why
Culoarea din spate exista deja (`Clear(Parent.BackColor)`). Rămânea arcul subțire și palid, de la desenul pe coordonate întregi. Acum cadrul e pe jumătăți de pixel (`PixelOffsetMode.Half`, `FrameBounds`, `RoundedRectF`, vezi 00EF-61), cu grosimea păstrată: 2 px la butonul implicit neprimar, 1 px altfel.

## Files touched
`src/KBot.Controls/Button/KBotButton.vb` (versiunea proiectului: vezi 00EF-61).

## Test results
Build KBot.Controls: 0 erori, 0 avertismente. Niciun test scris sau rulat. Captură reală de ecran la 150% DPI: colțuri curate. **Văzut pe ecran de operator: corect.**

## Unverified / deferred
Inelul punctat de focus (`DrawRectangle` pe dreptunghiul întreg) nu a fost schimbat și nu a fost privit separat.
