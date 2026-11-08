# SLICE-0093-04 - Arbore: lupa din antet pune cursorul direct in caseta de cautare (06.11.2026)

Operator, 06.11.2026: in `AdvancedTreeControl`, cand se apasa pe iconita de cautare, focusul sa intre automat in caseta text, ca utilizatorul sa poata tasta direct. Slice-ul 0093 = ultimul care a modificat codul controlului arbore (0093-03, `SearchExtra`); 0093-04 = primul sub-numar liber.

## What changed and why
- `OnMouseDown` (clic pe iconita de cautare din antet) apela `OpenSearchMode()`, care dadea focusul ARBORELUI (`Me.Focus()`), deci utilizatorul trebuia sa mai dea un clic in caseta. Acum: `OpenSearchMode(focusTree:=False)` apoi `FocusSearchBox()`.
- `FocusSearchBox` (nou, in `AdvancedTreeControl.Search.vb`): `_searchTextBox.Focus()`; daca nu reuseste (fereastra casetei inca nu exista imediat dupa afisarea benzii), se reia o data prin `BeginInvoke`, cu verificarea ca banda e tot deschisa. Cu Try/Catch + `GlobalErrorLog.Write`.
- Banda permanenta (`SearchShow` fara iconita) ramane ca inainte: nu fura focusul la deschidere (`ApplySearchShow` trimite `focusTree:=False` si nu cheama `FocusSearchBox`).
- Inchiderea prin lupa, `Esc`, butonul ✕: neschimbate.

## Files touched
- `src/KBot.Controls/Tree/AdvancedTreeControl.Overrides.vb` (`OnMouseDown`), `src/KBot.Controls/Tree/AdvancedTreeControl.Search.vb` (`FocusSearchBox`).
- `src/KBot.Controls/KBot.Controls.vbproj`: FileVersion 1.85.0.0 -> 1.85.0.1.

## Test results
Fara teste (regula operatorului). `dotnet build src\KBot.Controls`: 0 avertismente, 0 erori.

## Unverified / deferred
- **Nimic vazut pe ecran**: ca cursorul apare in caseta imediat dupa clic pe lupa, in fiecare fereastra cu arbore cu lupa (DDF, E-Factura, Parteneri etc.); in special daca prima incercare de `Focus()` reuseste sau intra pe `BeginInvoke`.
- Ajutor: textele descriu lupa («deschide cautarea»), nu locul cursorului; nimic stricat. Nu am adaugat o propozitie noua in ajutor; de decis daca vrei una (ar fi `0000-NN`).
