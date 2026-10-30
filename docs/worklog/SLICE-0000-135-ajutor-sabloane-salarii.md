# SLICE-0000-135 - Help for the salary templates (slice AV01-15, 27.10.2026)

## What changed and why
`contabil/nomenclatoare/sabloane.md` (`contabil.nomenclatoare.sabloane`): the bar now has «Salarii»; new section «Salarii» (chapter, the list of articles, the commitment row, retentions, viraments,
cash payments, the minimum of one virament, delete); `screens:` gains `SablonSalariiView`; title and keywords widened; `nomenclatoare/index.md` says the topic covers the salary templates (label `AV01-15`).
Nothing about the former system or the migration is in the help (operator rule).

## Files touched
`HelpContent/contabil/nomenclatoare/sabloane.md`, `HelpContent/contabil/nomenclatoare/index.md`, `HelpContent/help-version.txt` (unchanged: 2026-10-27).

## Test results
`Check-Help.ps1 -Coverage`: no message about the topic (the messages it prints are the ones that already existed). No picture, no run.

## Unverified / deferred
- Written from the Designer text and the code, nothing seen on screen. Capture to take later: the «Salarii» page.
