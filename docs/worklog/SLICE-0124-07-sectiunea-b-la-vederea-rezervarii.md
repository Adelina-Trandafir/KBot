# SLICE-0124-07 — Section B and the captures: the old logic, unchanged

**Date:** 30.10.2026.

First written as «B goes in when the import has seen the reservation», with `DdfSectionBInsert.IsCandidate` also accepting a flow revision with `Incarcat`, and a plan to build B from the reservations. **The operator rejected it:** after the signature on A the
sending to FOREXE is K-BOT's own, as before (nobody works directly in FOREXE), the robot saves the real codes into the B rows, and section B + captures go into the document signed on A by the existing path (`SectiuneaBInseratAsync`, stage 2/3). `IsCandidate` went back to what it was, plus ONE line for the operator's step 6: for a revision of the flow (`DeSemnat` set) section B is written into the document, and the option to sign B appears, only when a reservation linked to the revision exists in K-BOT's reservations table (`AreRezervari`). Steps 4-5 (the robot saves the real codes = section B's data, the reservations are read back and brought in, then the stage is set) are the existing sending, unchanged.

## Test results

Built with the rest (0 errors). Not run.

## Left unverified / deferred

- Not run. Whether `AreRezervari` is true right after the sending's ingest (the link is the `(IDREV: n)` marker) is to be seen on the first run; if it is not, the option to sign B would not appear.
