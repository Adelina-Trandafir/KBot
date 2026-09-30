# SLICE 0000-09 — ORD signing in the help, from the MF form

> **Superseded by 0000-10:** everything below describes the OLD template A1.0.08. The current A1.0.11
> flow is in `SLICE-0000-10-ord-a1-0-11.md` and `docs/FUNDAMENT_Ordonantare_CAB.md`.

**Date:** 30.09.2026. **Standing slice:** 0000 (help).
Operator: `Surse/ord_xdp.xml` is a fully signed ORD with the minimum signatures; once they are on,
the document can be turned into a RECEPȚIE and uploaded to the FOREXE CAB server. Also:
`Surse/ord_xdp_full.xml` shows what the document does behind the scenes.

## What changed and why
- NEW `docs/FUNDAMENT_Ordonantare_CAB.md` (findings only, English): the table columns and their
  checks, the two-pass «Validare formular / Generare XML» (hidden flag `StareSemnatura`; the second
  pass attaches `ORDNT.xml`, shape recorded), the five signature fields with captions, K-BOT roles,
  opening order and what each manifest locks; the minimum complete set; open questions for the
  ORD-sending slice.
- `HelpContent/contabil/vederi/ord.md`: «Documentul și semnarea» rewritten — validation (press
  twice, what each press says), the five signatures in order with who / required, «the ordonator's
  signature closes the CFP ones» (so CFP goes first), complete = 1 + 2 + 5, and the closing note now
  names the next step (recepție + upload to the CAB server) as not done by K-BOT yet. +1 capture
  `ord-semnaturi`. Keywords extended.
- `docs/HELP_SYSTEM.md` §5: the ORD exclusion narrowed to the step after signing.

## Findings worth knowing (details in the FUNDAMENT file)
- The form does NOT force the CFP signatures: after SignatureField2, fields 3, 4 and 5 all open.
- SignatureField5 (Ordonator) locks fields 3 and 4 → a CFP signature must precede the ordonator's.
- K-BOT's generated ORD leaves `StareSemnatura` at 0, so the operator validates twice (unlike the
  DDF, where K-BOT's placeholders make one «Validează» enough).

## Files touched
`docs/FUNDAMENT_Ordonantare_CAB.md` (new), `src/KBot.App/HelpContent/contabil/vederi/ord.md`,
`docs/HELP_SYSTEM.md`, status files.

## Test results
`Check-Help.ps1 -Coverage`: no errors, coverage «(none)». Build App 0 warnings, 0 errors.

## Confirmed by the operator (30.09.2026, second round)
- 1 + 2 + 5 is the signed minimum (screenshot of the signed PDF).
- Validation takes two presses. The operator's test (sign after col. 4 only, reopen, validate) hit
  the dead end the scripts predict; the help now warns «sign only after the second validation» and
  gives the two-person order (department: col. 4 + validation #1 → CAB person: cols. 1-3 +
  validation #2 → signature 1 → signature 2). Recorded in the FUNDAMENT file §2.

## Still to read (operator)
- Who signs field 1 vs field 2 (both «Compartiment de specialitate»): the help gives field 2's own
  sentence and leaves field 1 as «compartimentul care face plata».
