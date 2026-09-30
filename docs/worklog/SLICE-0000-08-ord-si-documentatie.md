# SLICE 0000-08 — ORD in the help + documentation of the help system

**Date:** 30.09.2026. **Standing slice:** 0000 (help).
Operator: «let's do ORD as well — what is done is done; I'm not sure about the flow required by the
MF» + «this help system should be properly documented, so another thread picks it right off».

## What changed and why

### ORD topics (Part 1)
- `contabil/vederi/ord.md` rewritten: the tree, the pages «Vizualizare» / «Document», the right-click
  menu (Adaugă / Modifică / Șterge / Generare în lot) and the footer «Adaugă», «Generează» and signing
  on «Document», the question before regenerating over a signed PDF. Closing note: what MF requires
  when an ORD is SENT to FOREXE is not described (K-BOT does not send ORDs yet).
- NEW `contabil/vederi/ord-generare.md` (`contabil.ord.generare`): one ORD per day (the «+» on a day in
  Plăți, or «Adaugă» + the day dialog `OrdZiuaForm`), the >25 partners warning, «Generare în lot» (the
  «+» on a month = that month), stop at the first error, deletion and what goes with it.
- NEW `contabil/vederi/ord-editor.md` (`contabil.ord.editor`): header, Beneficiari, Documente
  justificative (incl. «< TOȚI BENEFICIARII >», `OrdTextForm`), Atașamente (Adaugă / Lipește /
  Șterge), save validation (all problems at once), all-or-nothing save, images after the save.
- `contabil/vederi/plati.md`: the «+» on days / months.
- Captures: `ord` (prepare updated), new `ord-ziua`, `ord-editor`.
- Source: `KbotForm.Ord.vb`, `Views/OrdView.vb`, `Views/PlatiView.vb`, the Designer files of
  `OrdEditForm`, `OrdBeneficiariPage`, `OrdDocumentePage`, `OrdAtasamentePage`, `OrdZiuaForm`,
  `OrdTextForm`; validation strings in `ORD_EDIT/`.

### Fix seen in passing
- `ORD_EDIT/OrdAtasamentePage.Designer.vb`: the «Lipește» button carried the tooltip of «Șterge»
  («Șterge imaginea» / «Scoate imaginea...»). Now «Lipește imaginea» / «Adaugă imaginea din memoria
  temporară...». KBot.App FileVersion not bumped (`push-update.ps1` asks).

### Documentation for later threads
- NEW `docs/HELP_SYSTEM.md`: what the system is, the parts and who sees them, where every piece lives,
  how F1 picks a topic, **the step-by-step procedure for updating the help after a change**, what a
  feature slice must leave behind, writing rules and what is deliberately left out, engine extension
  points.
- NEW `tools/HelpCheck/Check-Help.ps1` (ASCII, PowerShell 5.1): the static check done by hand until
  now — headers, ids, parents, links, capture tags, tours, screen keys / tour targets against the
  code; `-Coverage` lists windows without a topic, `-Map` prints the topic tree (so no hand-kept map
  goes stale).
- Pointers: `CLAUDE.md` (new «Help (slice 0000)» section), `CODE_WORKFLOW.md` (definition of done
  item 4: update the help or name the stale topics), `KBOT_STATUS.md` (the locked decision points to
  the guide), `HelpContent/README.md` (points to the guide).
- `state/KBOT_STATUS_0000-0009.md`: a watermark «Ajutorul e la zi până la» (30.09.2026, slice 0096)
  and an Open threads line «Ajutor de actualizat» where feature slices leave their notes.

## Files touched
`src/KBot.App/HelpContent/contabil/vederi/{ord,ord-generare,ord-editor,plati}.md`,
`src/KBot.App/HelpContent/README.md`, `src/KBot.App/ORD_EDIT/OrdAtasamentePage.Designer.vb`,
`docs/HELP_SYSTEM.md`, `tools/HelpCheck/Check-Help.ps1`, `CLAUDE.md`, `docs/worklog/CODE_WORKFLOW.md`,
`docs/worklog/KBOT_STATUS.md`, `docs/worklog/state/KBOT_STATUS_0000-0009.md`.

## Test results
`Check-Help.ps1 -Coverage -Map`: 43 topics, 6 tours, 46 capture tags, no errors, coverage «(none)».
Build App 0 warnings, 0 errors. Nothing on screen.

## To read (operator)
- ORD signing: the text says «ca documentul de fundamentare» and does not name the ORD's signature
  fields.
- The generation warning text (>25 partners): described from the code comment and the day dialog's
  tooltip.
