# SLICE-0000-40…47 — Istoricul ajutorului pentru bugete

Șase jurnale existente din 02–03.10.2026 sunt reunite pe tema bugetelor. „Fără total” din 40 precede coloana pe rând din 44 și rândul separat din 46; 45 schimbă creditul la total anual, iar 47 mută verificarea într-un subiect propriu. Secțiunea Access din 41 rămâne o descriere istorică a ajutorului 0103; restricția ulterioară din 0104 și 45 este păstrată, fără editarea ajutorului runtime. 0000-42 este menționat în [0105](SLICE-0105-clasificatii-sumar-si-miscare.md#slice-0105), dar nu are jurnal propriu sau rând de registru; nu inventăm unul prin consolidare.

- [0000-40](#slice-0000-40)
- [0000-41](#slice-0000-41)
- [0000-44](#slice-0000-44)
- [0000-45](#slice-0000-45)
- [0000-46](#slice-0000-46)
- [0000-47](#slice-0000-47)

<a id="slice-0000-40"></a>

## SLICE-0000-40 — Help for 0102: the budget on versions, and how the DDF uses it (02.10.2026)

### What changed and why

Slice 0102 changed what the operator sees in «Clasificații bugetare» (the budget is a list of versions with a start date, no total) and in the DDF (the budget is the one the classification had on the revision day). Written from the code (`ClasificatiiForm.Designer.vb`, `ddf_edit.py`, `budget_on_day.py`).

- `contabil.nomenclatoare.clasificatii`: bullets rewritten (versions, «Început», no total, «+» / «✕»; corrections change quarters); new section «Cum se folosește bugetul în documentul de fundamentare» (the rule in plain words, the 01.01 version for January–March revisions, the fallback notice). Tags `0102`.
- `tur-clasificatii`: step «Bugetul» rewritten, NEW step «Buget › Adaugă o versiune» (`footer.right` of `gridBuget`), «Salvează» text. Tags `0102, 0000-40`.
- `contabil.ddf.editor` (Secțiunea A): the «Buget» of a generated line is the budget on the revision day; a line added by hand keeps today's credit. Tag `0102`.

Help sub-slice number: **0000-40** (0000-39 is taken by the sliceless work in `SLICELESS-lista-sursa-asociere-denumire-wicket.md`).

### Files touched

`src/KBot.App/HelpContent/contabil/nomenclatoare/clasificatii.md`, `src/KBot.App/HelpContent/tours/tur-clasificatii.md`, `src/KBot.App/HelpContent/contabil/ddf/editor.md`. `help-version.txt` already holds `2026-10-02`.

### Capturi de refăcut

- `clasificatii` (the window changed: multi-row budget grid with «Început» and a footer, no «Total») — the capture tag carries `redo: 2026-10-02 16:46 | why: 0102: …`.

### Test results

`Check-Help.ps1 -Coverage`: the only errors left are not from this task: `contabil\fereastra.md: 0077-3` (old) and the `0000-39` tags of `asocieri/index.md` and `forexe/lista.md` (the other work in progress); coverage «(none)». No new errors from the three files above once 0102 / 0000-40 are in the status files. Build of `KBot.App`: 0 warnings, 0 errors.

### Left unverified / deferred

Nothing looked at on screen. The sentence about a line added by hand («Adaugă rând») keeping today's credit is true only until the combo gets the revision date (see the 0102 worklog); update it then.

---

<a id="slice-0000-41"></a>

## SLICE-0000-41 — Help for 0103-04 / 0103-06: «Verifică bugetul» and «Trimite în Access» (operator request, 02.10.2026)

### What changed and why

`contabil.nomenclatoare.clasificatii` (`src/KBot.App/HelpContent/contabil/nomenclatoare/clasificatii.md`): two bullets for the new buttons in the window's list; new sections «Verificarea bugetului față de FOREXE» (`<!-- slice: 0103-04 -->`, the three values, «Doar diferențele», the automatic check after a download) and «Trimite în Access» (`<!-- slice: 0103-06 -->`); header tag now `0087, 0075-00, 0102, 0103-04, 0103-06`; `screens:` gets `BudgetCheckForm`; new keywords. Only what the operator sees and does.

### Files touched

`src/KBot.App/HelpContent/contabil/nomenclatoare/clasificatii.md`.

### Test results

`Check-Help.ps1 -Coverage`: `BudgetCheckForm` is covered; 3 errors remain that are NOT from this change (`contabil\fereastra.md` slice `0077-3`; `contabil\asocieri\index.md` and `contabil\forexe\lista.md` slice `0000-39`, which has no registry row). `dotnet build src\KBot.App` → 0 warnings, 0 errors.

### Capturi de refăcut

- `clasificatii` (tag already marked `redo: 2026-10-02 18:00`, `why: 0103-04 / 0103-06`): two new buttons in the footer.
- No capture exists yet for the new window «Verificare buget FOREXE» (`BudgetCheckForm`; it opens only through the button or after a download with differences — no `goto:`).

### Left unverified / deferred

Nothing run, nothing seen on screen. `help-version.txt` already reads 2026-10-02.

---

<a id="slice-0000-44"></a>

## SLICE-0000-44 - help for 0107-02: «Clasificatii bugetare» totals

Operator rule (30.09.2026): any change the operator sees is recorded in the help with its slice.

### What changed and why

Text only, tag `0107-02` added to every section touched:

- `contabil.nomenclatoare.clasificatii`: the budget grid has a row Total and no footer total; the corrections footer sums the chosen classification's corrections; summaries and the empty state have no footer total.
- Tour `tur-clasificatii`: steps «Bugetul» and «Rectificari > Total».

### Files touched

`src/KBot.App/HelpContent/contabil/nomenclatoare/clasificatii.md`, `src/KBot.App/HelpContent/tours/tur-clasificatii.md`.

### Test results

`Check-Help.ps1 -Coverage`: no errors. No capture needs redoing: the captured `clasificatii` image shows a leaf and now lacks the budget «Total» column - flagged below.

### Left unverified / deferred

- Capture `clasificatii` (leaf with budget and corrections) is stale: it shows no budget «Total» column.

---

<a id="slice-0000-45"></a>

## SLICE-0000-45 - help for 0108: credit per classification, budget without quarters

Operator rule (30.09.2026): any change the operator sees is recorded in the help with its slice.

### What changed and why

Text only, tag `0108` added to every section touched:

- `contabil.nomenclatoare.clasificatii`: «Cum se folosește bugetul în documentul de fundamentare» (the total of the version + corrections, no quarter), «Verificarea bugetului față de FOREXE» (the two columns, one credit per classification).
- `contabil.ddf.editor` (the «Buget» of the lines): total, no quarter.
- `contabil.vederi.sumar`: the credit bugetar is the classification's.
- Nothing about the Migrator or the Access system (rule: no Access in help).

### Files touched

`src/KBot.App/HelpContent/contabil/nomenclatoare/clasificatii.md`, `.../ddf/editor.md`, `.../vederi/sumar.md`.

### Test results

`Check-Help.ps1 -Coverage` run after the status rows were added (see the 0108 worklog for the code).

### Left unverified / deferred

No capture needs redoing for this change except possibly `sumar` if the credit column was framed (not checked).

---

<a id="slice-0000-46"></a>

## SLICE-0000-46 - help for 0107: «Clasificatii bugetare» widths and final row

Operator rule (30.09.2026): any change the operator sees is recorded in the help with its slice.

### What changed and why

Text only, tag `0107` added to every section touched:

- `contabil.nomenclatoare.clasificatii`: «Clsf» first on the budget grid, equal-width columns, the new row under both grids (last budget + all corrections, also for a node).
- Tour `tur-clasificatii`: step «Bugetul» mentions «Clsf» and the widths; new step «Bugetul in vigoare» (target `ClasificatiiForm.gridTotal`).

### Files touched

`src/KBot.App/HelpContent/contabil/nomenclatoare/clasificatii.md`, `src/KBot.App/HelpContent/tours/tur-clasificatii.md`.

### Test results

`Check-Help.ps1 -Coverage`: no errors.

### Left unverified / deferred

- Capture `clasificatii` is stale (no «Clsf» column, no last row).
- The tour step `gridTotal` not seen running.

---

<a id="slice-0000-47"></a>

## SLICE-0000-47 - help topic for the «Verificare buget FOREXE» window

Operator request, 03.10.2026: note the double click (slice 0107) in the help of the check window; create that help if it does not exist; at least one picture of the window.

### What changed and why

- The check window had no topic of its own (its text was a section of `contabil.nomenclatoare.clasificatii`, `BudgetCheckForm` in that topic's `screens:`). New topic **`contabil.nomenclatoare.verificare-buget`** (child of `clasificatii`, `screens: BudgetCheckForm`, so F1 / «?» on the window opens it): how it opens (the button; the automatic check after a download, only on differences), what it shows (columns, the three values, only classifications used in FOREXE, only differences), the double click (0107) and that the automatic window has no double-click pick.
- `clasificatii.md`: the old section is now two sentences and a link; `BudgetCheckForm` removed from its `screens:`. `nomenclatoare/index.md` links the new topic.
- Capture tag `verificare-buget` in the new topic (no `goto:`: the window is a dialog opened by a button, so `prepare:` says how to reach it).

### Files touched

`src/KBot.App/HelpContent/contabil/nomenclatoare/verificare-buget.md` (new), `clasificatii.md`, `index.md`.

### Test results

`Check-Help.ps1 -Coverage -Map`: see below in the status row; no build needed (text only).

### Capturi de refăcut / de făcut

- **`verificare-buget` — the PICTURE DOES NOT EXIST YET.** I did not run K-BOT. Take it from «Meniu › Capturi pentru ajutor» (the capture list cannot open this dialog by itself): open «Clasificații bugetare», press «Verifică bugetul», shoot the window with at least one row. Needs `FX_Indicatori_Buget` filled (a download with slice 0108) and the server with the new `check_budget`; until then the window shows no rows.
- `clasificatii` is still stale (0107: «Clsf» first on the budget grid, last row, the new check box).

### Left unverified / deferred

- Topic not seen in the help window; the F1 key `BudgetCheckForm` not tried.

Reper din registrul 0000-47: la checkpoint, `Check-Help.ps1 -Coverage` era fără erori. Acest rezultat istoric nu confirmă imaginea lipsă sau proba F1.
