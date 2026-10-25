# SLICE-0123-06 - Arbore: textul din dreapta se taie cu «...», nu dispare (25.10.2026)

Operator: in `AdvancedTreeControl`, cand datele sunt afisate pe doua parti (`stanga~~~dreapta`, ca la arborele din Plan de conturi) si textul din dreapta nu incape, trebuie sa nu dispara complet, ci sa apara «...» in dreapta. Slice-ul: ultimul folosit pe arbore (0123-02), primul sub-slice liber (0123-06).

## What changed and why
- **`KBot.Controls/Tree/AdvancedTreeControl.Painting.vb`, `DrawRichText`, cazul A** (nici `LeftTextWidth`, nici `RightTextWidth` setate - cazul arborelui din Plan de conturi): inainte, daca partea din dreapta nu incapea dupa cea din stanga, ramura facea `Return` si textul disparea. Acum: spatiul ramas intre capatul stangii (plus `PaddingSeparatorGapPx`) si marginea dreapta merge la `DrawRichPartsInZone`, care scrie cat incape si pune «...» fix la capatul drept. Daca nici «...» nu incape in spatiul ramas, nu se deseneaza nimic (ca inainte). Cand textul incape, comportamentul ramane cel vechi (aliniat la dreapta).
- Cazurile B/C/D si `RightTextColumn` aveau deja taiere cu «...»; neschimbate.
- `KBot.Controls` FileVersion 1.68.0.0 -> 1.69.0.0.
- Ajutor: `contabil/nomenclatoare/plan-conturi.md` (sectiunea Conturile, eticheta `0123-06`).

## Files touched
`src/KBot.Controls/Tree/AdvancedTreeControl.Painting.vb`, `src/KBot.Controls/KBot.Controls.vbproj`, `src/KBot.App/HelpContent/contabil/nomenclatoare/plan-conturi.md`, `docs/worklog/state/KBOT_STATUS_0120-0129.md`, `docs/worklog/KBOT_STATUS.md`.

## Test results
`dotnet build src\KBot.App\KBot.App.vbproj -c Debug`: 0 avertismente, 0 erori. Nimic rulat.

## Unverified / deferred
- **Nevazut pe ecran.** Comportamentul ales: se scrie cat incape din textul din dreapta, apoi «...» la margine (nu doar «...»). De confirmat de operator.
- Efectul este general: orice arbore cu `~~~` in cazul A (fara latimi setate) il are, nu doar Plan de conturi. Alte arbori cu acelasi caz nu au fost cautati.
- Nimic scris in `NOUTATI.md`; `help-version.txt` neatins.
