# SLICE-VC02-03 — editare

Stare: **IMPLEMENTAT_LOCAL**. Verificat doar sintaxa (Python `ast`, `node --check`); nimic rulat, previzualizat sau testat; nimic validat pe server.

## Facut
- `static/js/vercon/fields.js`: contract per coloana (text/int/decimal/date): parse, validare, format. Virgula zecimala, `1.234,56`, data `zz.ll.aaaa`, gol = NULL diferit de 0, text pastrat exact (apostrof, diacritice).
- `draft.js`: draft in afara DOM, randuri dupa cheie stabila (`n1`, `n2`…), `collect` valideaza tot si da erori pe camp.
- `editor.js`: antet + linii pe fixture; repictarea liniilor din draft nu pierde valorile.

## Limite
Nu s-a folosit `DataGrid` comun (nu exista inca date mari / virtualizare); decizia de a-l adopta se ia la G5 din PROBE_BROWSER. Combobox/date picker custom nu sunt legate in editor. Totalurile read-only nu exista inca (VC06).
