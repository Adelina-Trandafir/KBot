# SLICE-0000-13 — Where each explanation comes from + help for slice 0097 (30.09.2026)

Operator request (30.09.2026):
1. every explanation in the help carries its slice, invisible to the user, so it is known where
   the decision behind it came from;
2. «Descărcarea unui angajament» must point to the window that picks the receptions to read again;
3. the new options in Setări › Autentificare and the unit selector in the KbotForm caption bar.

Mid-task, a new standing rule: **ANY change to what the operator sees must also be recorded in the
help, and must reference its slice number.** Written into `CLAUDE.md` (section «Help (slice 0000)»),
`CODE_WORKFLOW.md` (Definition of Done) and `docs/HELP_SYSTEM.md` §4 / §5.

## 1. Source tags

- Syntax: one line `<!-- slice: 0072, 0097 -->`, right after a topic's header block (covers the
  text before the first `##`) and right under **every** `## ` heading of topics and tour steps
  (`###` optional). Ids: a slice from the `KBOT_STATUS.md` index (`0048` or `0048-04`), a help
  sub-slice `0000-NN`, or `fara-felie` (work recorded only in `KBOT_STATUS_SLICELESS.md`).
- Engine: `HelpLibrary.SourceTagPattern` / `StripSourceTags`; the tags are removed when a topic
  (`HelpLibrary.Parse`) or a tour (`HelpTour.Parse`) is read, so no page, bubble, search hit or
  exported manual ever carries them.
- Checker: `Check-Help.ps1` fails on a topic without the header tag, a `##` section / tour step
  without a tag under it, an empty tag, or an id that is neither in the index nor a 0000-NN worklog.
- All 47 topics and 12 tours tagged (222 tags). **Best effort** (operator: «where the source isn't
  clear, don't bother too much»): taken from the code comments of each window («Slice 00NN») and
  from the worklogs that name the window / message. Least certain: the «Butonul ⓘ» section and
  «Mesajele K-BOT» (`fara-felie`), `contabil.ddf.mf` (0000-03 = the MF guide summary), the
  Asocieri concept sections (0048-03…09 + 0000-12). A later edit of a section replaces its tag
  with the slices that decided the new text.

## 2. Help for slice 0097

| Topic / tour | Change |
|--------------|--------|
| `contabil.forexe.descarcare` | new section «Înainte de descărcare: ce recepții se citesc din nou» (the «Ce recepții reîmprospătez?» window, columns, same-day rule, Descarcă / Renunță, the Setări switch); new «A doua apăsare, cât K-BOT e deschis» (reuse of the package, 0058); «Reîmprospătări parțiale» points up |
| `contabil` (index), `contabil.vederi.receptii`, `tur-forexe` | mention of the window |
| `contabil.autentificare` | «Când expiră sesiunea» (silent re-login vs window), «Ține minte parola până la repornirea calculatorului»; «Parola nu se salvează niciodată» removed (no longer true) |
| `contabil.fereastra` | new «Unitatea de lucru» (the caption selector, the two refusals, secondary windows); «Note corecție» gated; «cinci zone»; `KbotForm.capBar` in `screens:` |
| `contabil.setari` | new «Pagina «Autentificare»» (three groups, what needs the advanced options); table row |
| `contabil.vederi.ord`, `contabil.ord.generare`, `tur-ord` | root «Toate ordonanțările», month / root delete, signed = no menu, no box after a delete |
| `contabil.ddf`, `contabil.ddf.semnare`, `tur-ddf` | root «Toate reviziile», «Șterge documentul (TOATE reviziile)», signed = only the send; «Semnat A» no longer editable |
| `contabil.notecab` | the view appears only with a note |
| `tur-setari`, `tur-fereastra` | Autentificare step, unit selector in the title step |

Capture ids: **new** `unitate-selector`, `setari-autentificare`. **Moved** `selectie-receptii`
(now in the new section; same window). **To re-shoot**: `ord`, `ddf-vedere` (the new roots),
`conectare-fereastra` (only if the password box is shown on the shooting PC).

## Check + build

- `Check-Help.ps1 -Coverage`: 47 topics, 12 tours, 52 capture tags, **No errors**, coverage «(none)».
- `dotnet build src/KBot.App/KBot.App.vbproj`: **0 warnings, 0 errors**. Nothing run, nothing on screen.

## Files touched

- engine: `src/KBot.App/Help/HelpLibrary.vb`, `Help/HelpTour.vb`
- checker: `tools/HelpCheck/Check-Help.ps1`
- all 47 topics and 12 tours under `src/KBot.App/HelpContent/` (tags); content changes as in §2
- rule + syntax: `CLAUDE.md`, `docs/worklog/CODE_WORKFLOW.md`, `docs/HELP_SYSTEM.md`, `src/KBot.App/HelpContent/README.md`
- status: `state/KBOT_STATUS_0000-0009.md` (row, watermark, Open threads), `KBOT_STATUS.md` index

## De citit de operator

- The source tags are a best effort — correct any you know better.
- ORD: with the menu gone on a signed ordonanțare, the help still says a broken signed document is
  «generated again from K-BOT» (the «Generează» button of the Document page). Is regenerating a
  signed ORD still intended?
- «Stare» in the reception window shows «încărcată» / «preluată» without explaining them — say
  what they mean to the accountant if it matters.
- The unit switch depends on the server routes of 0097 (`auth.py`, deploy by hand).
