# SLICE-0000-40 — Help for 0102: the budget on versions, and how the DDF uses it (02.10.2026)

## What changed and why

Slice 0102 changed what the operator sees in «Clasificații bugetare» (the budget is a list of versions with a start date, no
total) and in the DDF (the budget is the one the classification had on the revision day). Written from the code
(`ClasificatiiForm.Designer.vb`, `ddf_edit.py`, `budget_on_day.py`).

- `contabil.nomenclatoare.clasificatii`: bullets rewritten (versions, «Început», no total, «+» / «✕»; corrections change quarters);
  new section «Cum se folosește bugetul în documentul de fundamentare» (the rule in plain words, the 01.01 version for
  January–March revisions, the fallback notice). Tags `0102`.
- `tur-clasificatii`: step «Bugetul» rewritten, NEW step «Buget › Adaugă o versiune» (`footer.right` of `gridBuget`), «Salvează»
  text. Tags `0102, 0000-40`.
- `contabil.ddf.editor` (Secțiunea A): the «Buget» of a generated line is the budget on the revision day; a line added by hand
  keeps today's credit. Tag `0102`.

Help sub-slice number: **0000-40** (0000-39 is taken by the sliceless work in `SLICELESS-lista-sursa-asociere-denumire-wicket.md`).

## Files touched

`src/KBot.App/HelpContent/contabil/nomenclatoare/clasificatii.md`, `src/KBot.App/HelpContent/tours/tur-clasificatii.md`,
`src/KBot.App/HelpContent/contabil/ddf/editor.md`. `help-version.txt` already holds `2026-10-02`.

## Capturi de refăcut

- `clasificatii` (the window changed: multi-row budget grid with «Început» and a footer, no «Total») — the capture tag carries
  `redo: 2026-10-02 16:46 | why: 0102: …`.

## Test results

`Check-Help.ps1 -Coverage`: the only errors left are not from this task: `contabil\fereastra.md: 0077-3` (old) and the
`0000-39` tags of `asocieri/index.md` and `forexe/lista.md` (the other work in progress); coverage «(none)». No new
errors from the three files above once 0102 / 0000-40 are in the status files. Build of `KBot.App`: 0 warnings, 0 errors.

## Left unverified / deferred

Nothing looked at on screen. The sentence about a line added by hand («Adaugă rând») keeping today's credit is true only until the
combo gets the revision date (see the 0102 worklog); update it then.
