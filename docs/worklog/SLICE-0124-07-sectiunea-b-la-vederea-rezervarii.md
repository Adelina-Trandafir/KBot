# SLICE-0124-07 — Section B and the captures: the old logic, unchanged

**Date:** 30.10.2026.

Initial design: «B goes in when the import has seen the reservation», `DdfSectionBInsert.IsCandidate` also accepting flow revisions with `Incarcat`, B built from reservations. **Operator rejected it:** after A, K-BOT sends to FOREXE as before (nobody works there directly); robot stores real codes in B rows, and existing `SectiuneaBInseratAsync` (stage 2/3) inserts B/captures into A-signed PDF. `IsCandidate` reverted, adding only step 6: flow revisions (`DeSemnat` set) require a linked reservation in K-BOT's table (`AreRezervari`) before B insertion/signing option. Steps 4–5 remain existing sending: store real codes (B data), read back/import reservations, then set stage.

## Test results

Built with the rest (0 errors). Not run.

## Left unverified / deferred

- Not run. Whether `AreRezervari` is true right after the sending's ingest (the link is the `(IDREV: n)` marker) is to be seen on the first run; if it is not, the option to sign B would not appear.
