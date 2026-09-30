# SLICE 0000-06 — Part 3 «Director»

**Date:** 30.09.2026. **Standing slice:** 0000 (help).

## What was written

Content only; no code changed.

- `HelpContent/director/index.md` (rewritten from the stub): what the director's window is, where the
  director's signature sits in the DDF flow (states «Semnat A și B — la director» → «Aprobat»), the
  window at a glance.
- `director/lista.md`: the list, its six columns, the hover text, «Reîncarcă lista» (the list does not
  refresh itself after a signature), the three failure cases («Nu s-au putut citi: ...», an error in
  place of the title, a document that does not appear).
- `director/unitati.md`: one session = one unit; a document of another unit → question → login window
  with that unit pre-selected → old session logged out; the two follow-up messages; the expired-session
  re-login on the same unit.
- `director/semnare.md`: read-only DDF view (the refusal sentence quoted), the signing steps on
  «Document PDF», what happens after, the same «if something goes wrong» list as the accountant's
  signing topic.
- `tours/tur-director.md`: 5 steps on `DirectorForm` (lblTitlu, lstDocumente, pnlDocument, lblStare,
  btnReincarca).

F1 keys: `DirectorForm`, `DirectorForm.capBar`, `.lstDocumente`, `.lblTitlu`, `.btnReincarca`,
`.pnlDocument`, and the DDF view's page types. A director sees only Part 3, so the DDF page types
here do not clash with the accountant's DDF topics.

Captures: `director-fereastra`, `director-lista`, `director-semnare`, with no `goto` (the capture list
opens only from the accountant's main window, which cannot open the director's). Each `prepare` says
to shoot on the director's PC and load the picture with «Încarcă».

## Source
`src/KBot.App/Director/DirectorForm.vb` + `.Designer.vb` (operator strings, control names),
`SLICE-0081-06-kbot-director.md`, `KBot.Domain/DdfRevisionState.vb` (state labels),
`contabil/ddf/semnare.md` (shared signing behaviour).

## To check (operator)
1. The signature field's name in the final PDF: the text says «câmpul semnăturii ordonatorului de
   credite». Is there a «Validez» button for the director too, as for A and B?
2. «Anunță administratorul K-BOT» — right wording for who the director calls?

## State
Build App **0 warnings, 0 errors**; the files reach `bin\...\Help\director\` and `Help\tours\`.
Nothing seen on screen (no director login available here).
