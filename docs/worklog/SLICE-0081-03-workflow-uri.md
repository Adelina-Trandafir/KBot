# SLICE 0081-03 — the workflow edits (G1, G2; G3 dropped)

**Date:** 25.09.2026. **Plan:** `docs/PLAN_DDF_Trimitere.md` §0081-03. Only `.wfl` files in
`src/KBot.Forexe/Workflows/Creare/` (never `Surse/`), plus the project entry that copies them.

## What changed and why

**G1 — `Definitivare_Derulare Angajament.wfl` split in two**, because «Definitivează» and «Derulează»
are two separate actions of the Rezervări footer menu. Selectors copied unchanged from the original,
which stays in the folder until both halves have run live.
- `adlop - Definitivare Angajament.wfl` — search, «Modificare», table before, «Definitivează» + reason,
  per row CB inițial → CB definitiv with a capture `Poza_Definitivare_[[idx]]` before «Salvează», then
  `Poza_Definitivare` (the page after the last row), the state label and `TabelIndicatoriFinali`.
  Variables: `COD_ANGAJAMENT`, `MOTIV_DEFINITIVARE`.
- `adlop - Derulare Angajament.wfl` — search, «Modificare», «În derulare» + reason, `Poza_Derulare`
  right after the success message, the state label and `TabelIndicatoriFinali`.
  Variables: `COD_ANGAJAMENT`, `MOTIV_DERULARE`.

**G2 — `Incarca Rezervare.wfl` V4.1:** at the end, when `{{CAPTURA_INFO_COMPLETE}}` is `true`:
«Afișează informații complete» → wait for «Informații complete contract» → `Poza_InfoComplete` →
«Înapoi». K-BOT sets it `true` for a Rev 0 on an angajament already «În derulare» (0081-04).

**G3** — dropped (plan, 25.09.2026): «Adaugă rezervare» is offered only on angajamente «În derulare».

**Deployment:** the `Creare\` workflows were not copied to the output at all (no project entry;
`publish-debug.ps1` copies only the top-level `*.wfl`). Four entries added to `KBot.Forexe.vbproj`;
the build now writes `bin\…\Workflows\Creare\` (checked on disk). They flow into KBot.App's output /
publish as the other workflows do. The code resolves them as `ResolvePath("Creare\…")` (0081-04).

## Two traps found while reading, recorded for whoever edits these files next

1. **`{{X|text}}` has NO default.** The part after `|` is a description
   (`WorkflowParser.ExtractVariablesDetailed`); `ApplyVariables` replaces only what the job passes, so
   an unpassed variable reaches the page as literal `{{X|…}}`. K-BOT always passes `CAPTURA_INFO_COMPLETE`.
2. **One capture = one variable name.** The executor appends to a per-name list and hands a multi-value
   name back as a JSON array. The original `Definitivare_Derulare` used `saveTo="Poza"` for every row
   (and put it in `collectFields`); the new file names each row's capture and drops `Poza` from
   `collectFields` (an image inside every table row is not wanted).

## Files touched

- `src/KBot.Forexe/Workflows/Creare/adlop - Definitivare Angajament.wfl` (new)
- `src/KBot.Forexe/Workflows/Creare/adlop - Derulare Angajament.wfl` (new)
- `src/KBot.Forexe/Workflows/Creare/adlop - Incarca Rezervare.wfl` — G2
- `src/KBot.Forexe/KBot.Forexe.vbproj` — copy the four sending workflows

## Test results

All six `Creare\*.wfl` parse as XML (Python `xml.etree`; an XML comment with `--` was caught and fixed).
`dotnet build src\KBot.Forexe` **0 errors, 0 warnings**. **No workflow was run** — selectors can only be
checked live.

## Left unverified / deferred — each branch needs ONE live run on 000_DEMO

- `Definitivare Angajament` on an angajament «Inițial» created by K-BOT.
- `Derulare Angajament` on the same angajament, now «În definitivare».
- `Incarca Rezervare` with `CAPTURA_INFO_COMPLETE = true`: the three new selectors are guesses from the
  guide's screenshots — `a|button:has-text('Afișează informații complete')`,
  `*:has-text('Informații complete contract')`, `button.btn-default|a:has-text('Înapoi')`.
- After both halves ran live: retire `adlop - Definitivare_Derulare Angajament.wfl`.
- After the first publish: check `C:\KBOT\Workflows\Creare\` exists.
