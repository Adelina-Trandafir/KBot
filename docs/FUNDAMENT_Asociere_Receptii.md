# Reception association — why `FX_DUBII` exists

**Status:** findings and decisions. The pipeline half is implemented (slices 0048-01…03 and
0048-03-completare); the association FORM is not (slice 0048-04).
**Written:** 26.08.2026. Supersedes the first version of the same day. Revised the same day:
F17 amended, F20 withdrawn, F28 added, D-N and D-O added, O3 and O5 closed.
**Revised 31.08.2026 (slice 0048-07): F13 WITHDRAWN** — `DataR` is a hand-typed, editable field,
not a creation timestamp, so the date veto rested on nothing; it survives as a sign only, on both
paths. **F25 amended** — step 4b matches on `CLng(DataR)`, not on the header hash. **F29 added**
(the premise itself). **F30 added** — a recorded, deliberately unfixed hazard that follows from
F25's real key. §1.5 rewritten accordingly; O2 closed, O7 added.
**Location:** `docs/FUNDAMENT_Asociere_Receptii.md` (moved here from the repo root in slice 0048-03).

This document exists because the association form could not be explained to its users. Working
through the real flow showed why: the form has been presented as an error list, and it is not one.
It is the main data-entry step of the whole ingest.

Part 1 is the reasoning, meant to be read start to finish. Part 2 is the same content as numbered
rules with sources, meant to be quoted at implementation time. Part 3 corrects existing documents.
Part 4 records what was decided on 26.08. Part 5 lists what is still open.

Sources are marked. `VERIFIED` = read in the code or the schema, file named. `OPERATOR` = stated by
the operator, date given. `DERIVED` = follows from something above, derivation shown. `UNVERIFIED` =
a hypothesis needing a query against live data before anything relies on it.

---

# Part 1 — The reasoning

## 1.1 The data has two halves that do not meet

`FX_Receptii_R`, with `FX_Receptii_RHR` under it, is **each reception as it stands today**: one row
per reception, one `RHR` row per indicator, holding the current value. This half knows *which*
reception every row belongs to. It has no time axis.

`FX_Receptii_H`, with `FX_Receptii` under it, is **one dated snapshot of some reception** — the
moment somebody edited it. `DataH` is when, `Total` is the reception's whole value at that moment,
and the `FX_Receptii` rows are the per-indicator breakdown. This half has a time axis and does not
know which reception it describes.

The snapshots of one reception, ordered by `DataH`, are the value of that reception over time. That
sequence is what the ordonanțare arithmetic needs, and it does not exist until somebody builds it.

Two details about the Istoric blocks that matter downstream:

- The values are **absolute, not deltas**. A reception at 1000 with 100 added arrives as 1100. The
  same reception with 100 removed arrives as 900.
- Each block ends with a **summing row over every indicator in that reception**, including the ones
  that did not move. `Total` is the whole reception at that instant, not the size of the change.

Worked through: a reception holding AAB = 1000, when the operator adds 100 to AAB and adds a new
AA2 at 200, produces `AAB 1100`, `AA2 200`, `Total 1300`. When the operator later takes 500 off AAB
and 200 off AA2, it produces `AAB 600`, `AA2 0`, `Total 600`.

Nowhere in either block does the reception get named.

## 1.2 Four reasons no automatic key exists

**One — the page never sends it.** Istoric does not carry the reception identity. Not a parsing gap
on our side; the information is not displayed.

**Two — value cannot stand in for it.** Several receptions on the same angajament can hold the same
amount, and often do.

**Three — date cannot stand in for it.** `DataH` is the system clock at the moment of the edit.
`DataR` is the reception's creation date. Two different kinds of date; testing them for equality is
meaningless.

**Four — a save that changes nothing still produces a snapshot.** The operator opens a reception on
the site, alters nothing, saves, and Istoric receives a complete block carrying exactly the previous
values. The *number* of snapshots does not tell you how many real changes occurred.

The fourth has more consequences than it first appears to. Two snapshots can both equal a
reception's current total, so "the snapshot whose total matches" no longer identifies the end of a
chain unambiguously. And if two receptions happen to hold the same total, a no-op snapshot matches
both equally well — the automatic pass hands it to whichever it reaches first, and says nothing.

## 1.3 Why the sequence has to be right

Every ordonanțare needs the reception total **as it stood on the payment date**, together with the
payments already made by then. Reconstructed on 26/08 for a payment made on 02/01, that means
walking the snapshot chain of the correct reception back to that date.

| Date | Event | `TotalReceptii` | `PlatiAnt` | `Valoare` | `Ramas` |
|---|---|---|---|---|---|
| 01/01 | reception created at 100 | | | | |
| 02/01 | payment 100 | 100 | 0 | 100 | 0 |
| 03/01 | reception increased by 100 | | | | |
| 04/01 | payment 50 | 200 | 100 | 50 | 50 |

Put a January snapshot on the wrong reception and the figures for every payment after that date are
wrong. Nothing raises. No later step compares them against anything. The error surfaces months
later, if at all, as a reconciliation that will not close.

## 1.4 What the form actually is

The existing automatic pass (`TMP_Asociaza_Receptii_Istoric`, `mdl_FX_Receptii`) compares `H.Total`
against `R.SumaAntet` — the reception's total **now** — walks both sides newest-first, and consumes
each reception at most once.

Follow that through. `SumaAntet` is the current value, so the only snapshot that can match by value
is the last one in the chain. A reception edited four times produces four snapshots, exactly one of
which is placed automatically, and three of which fall out of both passes and land on the operator.

The operator's workload is therefore roughly *(snapshots) − (receptions)*, and it grows with how
active the angajament is. This is not an exception path. It is the normal outcome of every download.

That is almost certainly why it cannot be explained. Users have been handed what looks like a list
of problems, when what they are being asked is: *this is the life story of reception 2, in order —
confirm it.*

## 1.5 What the data can check for us

These cannot identify the right reception. They can only rule out wrong ones. Since most angajamente
carry a single indicator, and since the date rule turned out not to be a rule at all (see below),
what is really left is the chain-end mark and the operator's memory — so the form's job is not to be
clever. **It cannot usefully refuse a placement, so it must instead make every placement cheap to
undo and instantly visible in its consequences.** That is the whole design brief.

- **Date rule — WITHDRAWN 31.08.2026, see F13.** ~~A reception created in March cannot own a
  January snapshot.~~ `DataR` is not a creation date: it is typed by hand on the site and can be
  changed afterwards, and `FX_Receptii_R` has no creation timestamp at all (F29). What is left is a
  **sign** — "this snapshot is older than the date written on the reception" — which says that
  either the date is wrong or the snapshot belongs elsewhere, and leaves the operator to say which.
- **Indicator subset.** Indicators get added to a reception over time and fall to zero, but never
  disappear from the block, so a snapshot naming an indicator the reception does not have cannot
  belong to it. Correct but weak: usually one indicator per angajament.
- **Chain end.** The latest snapshot by `DataH` in a chain must equal the reception's `SumaAntet`,
  and its lines must equal the `RHR` rows. Shown per reception as a mark, this turns "did I do this
  right" into something visible instead of something guessed.
- **Sets only grow.** Along one chain ordered by `DataH`, each snapshot's indicator set must contain
  the previous one's.
- **No-op detection.** A snapshot equal to the latest snapshot already in a reception's chain is a
  save that changed nothing *for that reception*. The detection is relative, not absolute.

## 1.6 Why ignoring beats forcing, for no-op saves

A save that changed nothing carries zero information. Discarding it loses nothing at all. Placing it
on the wrong reception injects a false value into that reception's timeline at that date, and the
chain-end check will not catch it if it lands mid-chain.

So "everything must be placed" is the wrong rule for this class of row. Ignoring is lossless,
placing is lossy. `Sters` survives from the Access form, but reframed: not a hiding mechanism, but
the statement *this records no change*.

## 1.7 Deletions

A reception deleted on the site produces a history row with `Descriere = "Stergere receptie"` (that
spelling exactly, no diacritics) and Observații of the form
`Receptie: Plata ces, valoare: 7150, (activ:true)`. It carries no per-indicator rows.

Because it carries `(activ:true)` it becomes a snapshot like any other — and that is the right
outcome, not something to divert. It becomes the **last snapshot in that reception's chain**, its
`DataH` is the deletion date, and its `Total` is what the reception was worth when it went. Read
that way, no separate deletion-date column is needed: the chain already says when.

A deleted reception still counts toward the receptions total for payments made **before** its
deletion, and stops counting after. It disappears from the Recepții tree and from ORD computation
from that date onward.

---

# Part 2 — Rules

Quote these by number.

### The shape of the data

**F1.** `FX_Receptii_R` + `FX_Receptii_RHR` hold each reception's **current** state, one `RHR` row
per indicator. — `VERIFIED`: schemas in `FX_System_Export/TABLES/`; write path in
`Receptii_Prelucrare`.

**F2.** `FX_Receptii_H` + `FX_Receptii` hold **one dated snapshot** of some reception. — `VERIFIED`:
same sources plus `FX_Istoric_Populeaza_Receptii`.

**F3.** Istoric values are **absolute, not deltas**, and each block ends with a summing row over
**every** indicator in the reception. — `OPERATOR`, 26.08.2026.

**F4.** Istoric **never names the reception**. — `OPERATOR`, 26.08.2026.

**F5.** Value is not a key: several receptions can carry the same amount. — `OPERATOR`.

**F6.** Date is not a key: `DataH` is the system clock at edit time, `DataR` the creation date. —
`OPERATOR`.

**F7.** A save that changes nothing still produces a complete snapshot with identical values. —
`OPERATOR`, 26.08.2026.

**F8.** Reception identity **between downloads** (incoming ▸ stored) is keyed on the day, through
`FX_Receptii_H_GetHashIdent`. This is a different problem from F4 and it does have a key. —
`VERIFIED`: `Receptii_Prelucrare`.

### What follows

**F9.** The automatic pass can only place the **last** snapshot of a chain. — `DERIVED` from
`TMP_Asociaza_Receptii_Istoric`, both runs plus `dicRFolosite`.

**F10.** Operator workload is roughly *(snapshots) − (receptions)* per angajament. The form is a
primary step, not an exception dialog. — `DERIVED` from F9.

**F11.** With F7 in play, two snapshots can both equal a reception's current total, and a no-op
snapshot matching two receptions goes to whichever the pass reaches first, silently. — `DERIVED`
from F5 + F7 + F9.

**F12.** A wrong association is **silent and permanent**. It corrupts `TotalReceptii` / `PlatiAnt` /
`Ramas` for every payment after that date and nothing compares the figures against anything. —
`DERIVED` from §1.3.

### Constraints the form enforces

**F13.** ~~**Date veto.** A reception whose `DataR` is later than a snapshot's `DataH` cannot own
it. **Full timestamp**, not truncated to the day.~~ **WITHDRAWN 31.08.2026.** `DataR` is an
ordinary editable field typed by the operator on the site, not a creation timestamp, and
`FX_Receptii_R` has no creation timestamp at all (F29). A veto built on a typed field can refuse a
correct placement, and on the ingest path that is a deadlock in exactly the situation F10 says must
never deadlock — the operator is stuck on a reception the form gives them no way to repair.

**The comparison survives as a SIGN only**, never as a refusal, on both the ingest path and the
editor. The "full timestamp, not day granularity" wording goes with it: comparing a system-clock
`DataH` against a hand-typed midnight was never a real comparison, and as a sign it would light up
on every snapshot saved on the reception's own date. The sign is measured **on the day**. —
`OPERATOR`, 31.08.2026.

**F14.** **Indicator subset.** A snapshot naming an indicator absent from the reception's current
`RHR` cannot belong to it. Implement as a veto; do not rely on it — most angajamente carry one
indicator. — `OPERATOR`, 26.08.2026.

**F15.** **Chain end.** The latest snapshot by `DataH` must equal the reception's `SumaAntet` and its
lines must equal the `RHR` rows. Shown per reception as a visible mark. Deliberately worded "the
latest by date", not "exactly one that matches" — see F11. Does not apply to a chain terminated by a
deletion (F21). — `OPERATOR`, 26.08.2026.

**F16.** **Sets only grow.** Along one chain ordered by `DataH`, each snapshot's indicator set must
contain the previous one's. — `OPERATOR`, 26.08.2026.

### What the form shows and offers

**F17.** **AMENDED 26.08.2026.** A snapshot equal to the latest snapshot already in a reception's
chain is **shown as such, and nothing more**. Without the note the operator sees duplicate numbers
and assumes an error; with it, they can decide.

There is **no automatic no-op classification, at any point.** Two blocks carrying the same numbers
may be a save that changed nothing, or may be a real edit on a *different* reception that happens to
hold the same amount — the machine cannot tell the two apart, and the operator can. "Identical to
the previous snapshot in this chain" is therefore information on the form, computed live from the
chain the operator has currently built and recomputed on every drag. It never writes anything and it
never sets a marker.

The marker itself is an **operator action**: `FX_Receptii_H.Sters`, which already exists in the base
schema and which the Access form (`frmFX_DUBII_LISTA_HN`) set through a checkbox — clearing
`DIFH`/`DIFHC` on the header and `DIF`/`DIFC` on the lines when ticked, and gating the save
(`frmFX_DUBII.btnSav_Click` refuses while any snapshot has `TmpIDRecR IS NULL AND Sters=False`).
Not to be confused with `FX_Receptii_R.Sters`, which D-L defines as "this reception was deleted on
the site" — a different fact on a different table. — `DERIVED` from F7; amendment `OPERATOR`,
26.08.2026.

**F18.** Automatically placed snapshots are **marked as automatic**, because under F11 the automatic
pass can be wrong rather than merely incomplete. — `DERIVED` from F11.

**F19.** ~~A reception deleted on the site leaves its snapshots with no possible home.~~
**WITHDRAWN 26.08.2026, then PARTIALLY REINSTATED as F26.** A deletion cannot exist without a prior
creation, so the reception's earlier snapshots are already in `FX_Receptii_H` — *provided K-BOT
downloaded before the deletion*. Where it did not, the chain genuinely has no home and is
reconstructed. See F21 and F26.

**F20.** ~~`DIFH = 0` may mark a no-op save. Suggested by `tmpFX_Receptii_H` rows 414/415 (same
reception, both 1723,58, one minute apart) and 417/418 (both 441,74, one minute apart), each pair
showing the full amount then 0.~~ **WITHDRAWN 26.08.2026.**

`DIFH` is not something FOREXE sends. It is computed locally, by us, in
`FX_CalculeazaDIF_Receptii_Tmp`, **after** a snapshot has been associated with a reception — step 4d,
which Access calls only from 4c onward. At the moment a no-change judgement would be needed, `DIFH`
does not exist yet, so it cannot inform that judgement.

Withdrawn on the reasoning, not on a measurement: **no query against live data is owed**, and O3 is
closed by the withdrawal. Nothing in the pipeline reads `DIFH` to decide anything; the display use in
Recepții (slice 0015-02) is a legitimate consumer of a computed figure, not a classification. See the
amended F17 for what replaces it.

**F21.** A history row with `Descriere = "Stergere receptie"` (exact spelling, no diacritics) is a
**deletion**. It carries `(activ:true)` and no per-indicator rows, so it becomes an ordinary
snapshot — specifically, the **last** snapshot in its reception's chain. Its `DataH` is the deletion
date, its `Total` the reception's value at deletion. — `OPERATOR`, 26.08.2026.

**F22.** A deleted reception counts toward the receptions total for payments made **before** its
deletion date and stops counting after. It is hidden from the Recepții tree. — `OPERATOR`,
26.08.2026.

**F23.** ~~`FX_Receptii_H.IDH` appears to be the FOREXE history row id.~~ **WITHDRAWN 26.08.2026.**
`IDH` in every table is a foreign key into `FX_Istoric`, and `FX_Istoric.ID` is a **local** key — one
of the seven the parent plan §3 turns into `AUTO_INCREMENT`. The non-contiguous sample values are
gaps in a local sequence, not site-side ids. — `OPERATOR`, 26.08.2026.

**F24.** A snapshot's durable identity across the two phases is **the position of its history row in
the `TabelIstoric` payload**, not any database key. Both phases carry the same payload, so the index
is stable by construction and survives the proposal's rollback. `DataFX` travels alongside and is
checked on arrival, so a stale decisions file fails loudly rather than associating the wrong row. —
`DERIVED` from F23's withdrawal.

**F25.** **AMENDED 31.08.2026 — the match key is named correctly now.** Step 4b **never matches an
incoming payload reception against a stored reception with `Sters = 1`.** A deleted reception cannot
reappear in `ListaReceptii`, so an apparent match is a collision. The rule itself stands unchanged.

What was wrong was the key. This document (and the reading it came from) said 4b matches on the
header hash. **It does not.** Verified in the real `Receptii_Prelucrare`, supplied by the operator
on 31.08.2026:

```vba
rsTmpR_Snap.FindFirst "CLng(DataR)=" & CLng(dtDataR)
```

`sHashIdent` is computed by `ObtineDateHeader` and **stored** in `tmpFX_Receptii_R!HASH` on insert,
but it is **never read for matching**. The match key is `CLng(DataR)` — day granularity, `FindFirst`,
first hit wins. That is why F25 matters at all: without it, a reception created on the same calendar
day as a deleted one would be re-matched onto it and silently overwritten with another reception's
values. — `VERIFIED` in `Receptii_Prelucrare`; `OPERATOR`, 26.08.2026 and 31.08.2026.

**F26.** A reception created **and** deleted before K-BOT first downloaded the angajament has no
`FX_Receptii_R` row: its whole history arrives at once and `ListaReceptii` does not contain it. Its
chain is **reconstructed** from its own snapshots — the operator groups them in the form, the commit
path materialises the reception. Field derivations in `PLAN_ForexeIngestSteps3to8.md` §4c-bis. Marked
`Reconstituit = 1` alongside `Sters = 1`; the two are different facts and are not collapsed. —
`OPERATOR`, 26.08.2026. Partially reinstates F19, which was withdrawn on the grounds that a deletion
implies a prior creation — true on the site, true locally only if K-BOT saw the reception first.

**F27.** Where **two** receptions were both created and deleted before the first download, their
snapshots are indistinguishable except by amount and indicator, and the operator's grouping cannot be
verified by the machine. F14, F16 and the one-deletion-per-chain rule are guards, not a proof. —
`DERIVED` from F26.

**F28.** Where two or more receptions on one angajament are reconstructed, **all of them carry
`ReconstituitNesigur = 1`**; the flag records that the grouping was constrained by F14/F16 (and, until 31.08.2026, by F13) but
not verified. It is set at commit, counting the receptions with `Reconstituit = 1` after the run —
existing ones included, because two reconstructions made in two different sessions are exactly as
indistinguishable as two made in one. It is **never cleared**: a later run that happens to see only
one reconstruction does not make the earlier grouping any more provable than it was when it was made.
The commit response names the affected receptions, and the Recepții tree marks them. — `OPERATOR`,
26.08.2026. Records in the data the limit F27 states, so a total that does not add up months later
can be traced back to a grouping that was a judgement rather than a check.

**F29.** `FX_Receptii_R.DataR` is **operator-editable, including after creation**, and
`FX_Receptii_R` carries no creation timestamp. The table is
`IDRR, NRCRT, CodAngajament, Tip, DataR, SumaAntet, Descriere, HASH, TipReceptie, Incarcat, Preluat`
— there is no such column anywhere in it, and the sample rows carry `dd.MM.yyyy` with no time
component, which is what a typed date looks like. **No rule may treat `DataR` as a fact about when
the reception came into existence.** — `OPERATOR`, 31.08.2026; `VERIFIED` against
`MariaDB_Schema/000_DEMO.sql` and `FX_System_Export/TABLES/FX_Receptii_R.md`.

**F30.** **A known hazard, recorded and deliberately NOT fixed.** Because step 4b matches on
`CLng(DataR)` with `FindFirst` (F25), **two receptions carrying the same `DataR` collide on every
download after the first**: both incoming receptions find the same stored row, the second overwrites
the first's `SumaAntet` and `RHR` values, and no second row is inserted. On the *first* download both
are inserted, which is how such pairs come to exist — `AAB2HFBEEAF` rows 268 and 269 in the sample
data are both dated 16.01.2026, and they even share an identical `HASH`, so the hash would not
discriminate either.

Editing `DataR` on the site (F29) produces the mirror failure: the key moves, 4b no longer recognises
the reception, and a duplicate is inserted while the original keeps its chain.

The operator states he has never seen the `DataR` edit happen in practice, and that the same-day
behaviour is "not right all the time, but mostly works". **Recorded, deliberately not fixed here.**
If a duplicate or a silently overwritten reception ever appears after a download, this is the first
place to look. — `VERIFIED` in `Receptii_Prelucrare`; `OPERATOR`, 31.08.2026.

---

# Part 3 — Corrections to existing documents

## C1 — `FX_ORD_TBL_REC` is not a relic

> «PLAN_ForexeIngest.md IS WRONG. The FX_ORD_TBL_REC IS NOT A RELIC. I WAS WRONG! it needs to travel
> through migration and it needs to be used.» — operator, 26.08.2026

It links a payment to the ordonanțare line that consumed it. Three places carry the wrong version:

1. `docs/PLAN_ForexeIngest.md` §2 D5 — the `FX_ORD_TBL_REC` half is withdrawn.
2. The migrator exclusion list (locked decisions, slice 0046) — remove it; it migrates normally.
   **DONE** — slice 0048-03 (`db0e71d`), completed in slice 0048-03-completare: off the exclusion
   list in `KBot.Migrator/Transfer/TableMaps.vb`, mapped there and in `routes/migrare/tables.py`
   (selection kind `BY_ORD_TBL`), and written after both its parents — pinned offline by
   `test_fx_ord_tbl_rec_is_written_after_both_its_parents`.
3. Any text describing `FX_ORD_TBL.IDRP` as untouched on the strength of D5.

Schema, read from `000_DEMO.sql`:

| Column | Type | Meaning |
|---|---|---|
| `IDORDRECP` | `int(11) NOT NULL AUTO_INCREMENT` | MariaDB PK |
| `IDORDTBLP` | `int(11)` | FK ▸ `FX_ORD_TBL.IDORDTBLP`, `ON DELETE CASCADE` |
| `IDORDREC` | `int(11)` | Access PK, preserved |
| `IDRP` | `int(20)` | **DEAD** — see C2 |
| `Valoare` | `double` | commented `FX_Plati -> Suma` |
| `IdPlataFX` | `int(11)` | FK ▸ `FX_Plati.IdPlataFX`, `ON DELETE CASCADE` |

Two real foreign keys, so `FX_ORD_TBL` and `FX_Plati` must both be written before it.

## C2 — `IDRP` and `FX_Receptii_Plati` are dead

> «IDRP IS DEAD» · «NOT used anymore. it contains no data anymore. Excluded completely from
> migration» — operator, 26.08.2026

`FX_Receptii_Plati` was an early attempt to join a reception to a payment directly, made before the
flow was fully understood. It is empty and excluded from migration entirely. `FX_ORD_TBL_REC.IDRP`
pointed at it and is therefore dead as well — consistent with what the data already showed: 0 on all
sample rows, absent from the payload built by `mdl_FX_ORD_SYNC_TO_MARIADB`, and carrying no FK
constraint on MariaDB. The column stays in the schema, unmapped and unwritten.

## C3 — D1 and D3 are amended

`PLAN_ForexeIngest.md` D1 ("everything runs in Flask, one round trip") and D3 ("no temp tables")
were written before the association problem was understood. The ingest is now **two-phase**: a
proposal that writes nothing, and a commit that carries the operator's decisions. See Part 4, D-B.

Note that `PLAN MIGRARE` §3.2 already described this shape — build the whole graph, one payload, one
transactional POST. D1/D3 were the deviation, not the correction.

## C4 — D4 is amended

D4 said an unmatched header is written with `IDRR` NULL and the run continues. Under the two-phase
contract nothing is written until the operator has resolved everything, so no row is ever committed
with `IDRR` NULL by this pipeline.

## C5 — `PLAN_ForexeIngestSteps3to8.md` is not in the repository, and its §6 was wrong

Two document corrections were asked for on 26.08.2026 and **could not be applied where they were
meant to go**, because the target files are not there. Recorded here instead, because this document
is the one that survives.

**The plan file itself is absent.** `PLAN_ForexeIngestSteps3to8.md` is referenced by F26 (§4c-bis),
by O1, by the slice 0048-03 worklog and by the headers of `routes/forexe/prelucrare.py` — but it is
in neither `docs/` nor anywhere else on the machine, and it appears in no commit. It was written and
worked from, never committed. Everything it decided that still matters has been carried into this
document or into the code comments; anyone looking for the file should stop looking.

**Its §6 asked for two things that cannot both be true.** It wanted the local decisions store in
`KBot.App` *and* exercised from `KBot.DevHarness`. `KBot.App` **references** `KBot.DevHarness` (on
Debug, `KBot.App.vbproj:82`), so the arrow runs App ▸ DevHarness and a type in App is invisible to
the harness. Corrected: `AsociereStore` lives in **`KBot.Common`**, beside `KBotPaths` — the other
file-backed store next to the executable, and the shape §6 itself named as the one to follow — and
the POCO `AsociereDosar` lives in `KBot.Domain`. Both are visible to App and to the harness. This is
settled; it is not to be re-opened.

**Its §5.3 asked for a 400 on a missing `TipReceptie`. Withdrawn.** In Access the column existed so
the code could decide, after filling the temporary tables, whether a real row needed inserting or
updating. That decision does not exist here, and nothing reads the field —
`FX_Receptii_H_GetHashIdent` is built from `Tip`, not `TipReceptie`, and `FX_Receptii_R.TipReceptie`
is computed (`NOU`/`EDIT`). The rejection is not written. The key stays in `collectFields` in the
`.wfl` — D11's purpose was to make the two workflow files match, and carrying one unread column
costs nothing — with a comment at the reading site recording that nothing consumes it.

**And the stale copy of this document is already gone.** It was moved from the repository root into
`docs/` during slice 0048-03 (worklog §0.1). There is no second revision in circulation.

---

# Part 4 — Decisions, 26.08.2026

**D-A. The form ships inside slice 0048.** There must never be a build where the ingest can produce
unplaced snapshots with nowhere to resolve them. Slice shape: **0048-03** = steps 3–8 plus the
two-phase contract, coordinator deliberately not wired; **0048-04** = drag support in
`AdvancedTreeControl`, the form, and the wiring, together.

**D-B. Two-phase ingest.** Phase one runs the whole pipeline in a transaction, **rolls it back**, and
returns the proposed picture with everything unresolved. The client saves that plus the operator's
decisions to a local file. Phase two re-sends payload and decisions; the server runs it once more and
commits. Nothing reaches the database until acceptance. Same shape as the 409 `ALEGERE_UNITATE`
round trip already working in 0048-02.

**D-C. Local decisions file survives restarts.** `<AppDir>\Asociere\<cod>.json`, holding the proposal
and the decisions. Deleted once the server confirms, or when the operator abandons the run.

**D-D. Refusal discards the run.** Closing the form with anything unresolved asks for confirmation.
On yes, nothing is sent and the local file is deleted. On no, the form stays open. Under D-B this is
free: there is nothing to undo.

**D-E. `FX_Rezervari.IDREV`** absent from `FX_DDF_REV` ▸ write NULL and warn. Should never happen.

**D-F. Every snapshot from every run is in play** — first download or later ones. None are left
untouched.

**D-G. Step 8** (`FX_Indicatori_Actualizare_Extrase`) is ported.

**D-H. One save at the end**, not per action.

**D-I. Modal**, opened from a button in `ReceptiiView`, and automatically when a proposal comes back
with anything unresolved.

**D-J. Unassociate is per snapshot.** Reception roots are not draggable.

**D-K. Drag is built into `AdvancedTreeControl`**, not bespoke in the view. The date veto arrives as
a drop-validation veto rather than being buried in drop handling.

**D-L. Schema.** `FX_Receptii_R` gains `Sters tinyint(1) NOT NULL DEFAULT 0` and
`Reconstituit tinyint(1) NOT NULL DEFAULT 0`. `FX_Receptii_H` gains `EsteStergere tinyint(1)`. No
deletion-date column — F21 makes `DataH` the date. Applied to `AVACONT_SURSA` as well: new defaulted
columns the migration never writes.

**D-N. Structure travels.** Values arriving from the site keep their shape end to end. A list stays
a list, including lists inside lists. Nothing is flattened to text at any point in the chain. The
nested columns are read off the workflow definitions, not guessed: a `ForEachVar` whose
`collectFields` names a field that an inner `ScrapeTable` also writes with `saveTo` produces a nested
cell. Across all six `.wfl` files that is exactly two — `ListaReceptii.Detaliu` and
`TabelIndicatori.BugetIndicator`. `ForexeRunner.TryParseTable` no longer flattens, the request body
carries real JSON arrays, and the server **rejects** a flattened string instead of tolerating it. —
`OPERATOR`, 26.08.2026.

**D-O. Paths belong to the operator.** Every folder the application writes to is configurable at
runtime, from `%APPDATA%\\AVACONT\\KBot\\settings.json`, one entry per folder.
`KBot.Common.KBotPaths` is the only place a path is resolved. Defaults preserve today's behaviour
exactly. A missing file, missing key or blank value gives the default; a relative value resolves
against the application directory; a configured folder that does not exist is created at startup; a
configured folder that cannot be written **stops the application with a Romanian message naming the
setting and the path** — never a silent fall back to the default, because an operator who set a path
and got the old one anyway has been lied to. Every value is validated at startup, so a bad path
fails on launch rather than halfway through an ingest. — `OPERATOR`, 26.08.2026.

**D-M. Reconstructed receptions are built** (F26). The form offers "starts a reception that no longer
exists" as a fourth action on a snapshot; the operator drags the rest of the chain onto it, ending
with its deletion row; the commit path materialises it. Same drag, same vetoes, same chain rules — no
new concept for the operator to learn.

---

# Part 5 — Still open

**O1 — settled.** Reconstruction is built; see F26, D-M, and `PLAN_ForexeIngestSteps3to8.md` §4c-bis.
What remains is not a decision but a limit: F27. Two receptions both created and deleted before the
first download cannot be told apart by the machine, and the operator's grouping of their snapshots
cannot be verified — only constrained.

**O2 — closed 31.08.2026.** F23 was withdrawn on 26.08 (`IDH` is a foreign key into `FX_Istoric`,
whose `ID` is a LOCAL key), and F24 replaced it: a snapshot's durable identity across the two phases
is the position of its history row in the `TabelIstoric` payload, with `DataFX` travelling alongside
so a stale decisions file fails loudly. Neither F23 nor F24 depends on anything still open here, and
the two-phase contract has been built and shipped on F24. Nothing is owed.

**O7 — not a decision, an accepted hazard: F30.** Two receptions sharing a `DataR` collide on every
download after the first, and an edited `DataR` inserts a duplicate. Recorded in F30 with the
evidence; no code was written for it in slice 0048-07, on the operator's judgement that it has not
been seen in practice. It is listed here so that a duplicate or a silently overwritten reception is
diagnosed in one step rather than investigated from scratch.

**O3 — closed 26.08.2026, by F20's withdrawal, not by confirmation.** `DIFH` is computed locally
after association and does not exist in the incoming payload, so it cannot mark anything at the
moment the judgement would be needed. No live-data query is owed. What replaces it is in the amended
F17: the form shows "identical to the previous snapshot in this chain", and the operator marks it
through `FX_Receptii_H.Sters`.

**O4. Form layout** is not specified: pane arrangement, what the grid shows, what the right-icon
menus offer. Belongs in the 0048-04 plan.

**O5 — closed 26.08.2026.** `FX_Indicatori_Actualizare_Extrase` was read in slice 0048-01 (D20) and
is **ported** in slice 0048-03-completare: two statements, in that order, unconditional, inside the
same transaction as steps 1–7. `PLAN_ForexeIngest` §12 is amended — `FX_Extrase` stays out of scope
**except** those two statements, which are step 8 and were never optional. D-G stands, and is now
implemented rather than intended.

**O6.** `Descriere` on a history block matches the reception description, but operators rarely write
anything distinctive there, so it is not usable as a discriminator today. Parked: it becomes the
carrier for the real ForexeBug reception id once K-BOT posts back to the site — at which point DUBII
stops being necessary for anything created from then on.
