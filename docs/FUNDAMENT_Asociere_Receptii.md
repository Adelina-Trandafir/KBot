# Reception association — why `FX_DUBII` exists

**Status:** findings and decisions. Nothing is implemented yet.
**Written:** 26.08.2026. Supersedes the first version of the same day.
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
carry a single indicator, what is really left is the date rule, the chain-end mark, and the
operator's memory — so the form's job is not to be clever. It is to make each placement cheap,
reversible, and immediately visible in its consequences.

- **Date rule.** A reception created in March cannot own a January snapshot. Full timestamp
  comparison, not day granularity: operators save the same reception several times within one
  minute.
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

**F13.** **Date veto.** A reception whose `DataR` is later than a snapshot's `DataH` cannot own it.
**Full timestamp**, not truncated to the day. — `OPERATOR`, 26.08.2026. New rule, no Access
equivalent.

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

**F17.** A snapshot equal to the latest snapshot already in a reception's chain is a **no-op save**
and must be labelled as such. Without the label the operator sees duplicate numbers and assumes an
error. — `DERIVED` from F7.

**F18.** Automatically placed snapshots are **marked as automatic**, because under F11 the automatic
pass can be wrong rather than merely incomplete. — `DERIVED` from F11.

**F19.** ~~A reception deleted on the site leaves its snapshots with no possible home.~~
**WITHDRAWN 26.08.2026, then PARTIALLY REINSTATED as F26.** A deletion cannot exist without a prior
creation, so the reception's earlier snapshots are already in `FX_Receptii_H` — *provided K-BOT
downloaded before the deletion*. Where it did not, the chain genuinely has no home and is
reconstructed. See F21 and F26.

**F20.** `DIFH = 0` may mark a no-op save. Suggested by `tmpFX_Receptii_H` rows 414/415 (same
reception, both 1723,58, one minute apart) and 417/418 (both 441,74, one minute apart), each pair
showing the full amount then 0. — `UNVERIFIED`. Confirm on live data; the fallback is comparing the
lines directly.

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

**F25.** Step 4b **never matches an incoming payload reception against a stored reception with
`Sters = 1`.** A deleted reception cannot reappear in `ListaReceptii`, so an apparent match is a
collision. The match is on `CLng(DataR)` — day granularity — so without this rule a reception created
on the same calendar day as a deleted one would be re-matched onto it and silently overwritten with
another reception's values. — `OPERATOR`, 26.08.2026.

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

---

# Part 3 — Corrections to existing documents

## C1 — `FX_ORD_TBL_REC` is not a relic

> «PLAN_ForexeIngest.md IS WRONG. The FX_ORD_TBL_REC IS NOT A RELIC. I WAS WRONG! it needs to travel
> through migration and it needs to be used.» — operator, 26.08.2026

It links a payment to the ordonanțare line that consumed it. Three places carry the wrong version:

1. `docs/PLAN_ForexeIngest.md` §2 D5 — the `FX_ORD_TBL_REC` half is withdrawn.
2. The migrator exclusion list (locked decisions, slice 0046) — remove it; it migrates normally.
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

**O2. F23 must be confirmed** before the two-phase contract is built. If `IDH` is not a stable
FOREXE-side id, the decisions need a different natural key and the contract changes shape.

**O3. F20 must be confirmed or discarded** — whether `DIFH = 0` marks a no-op save, or whether the
lines have to be compared directly.

**O4. Form layout** is not specified: pane arrangement, what the grid shows, what the right-icon
menus offer. Belongs in the 0048-04 plan.

**O5.** `FX_Indicatori_Actualizare_Extrase` has not been read. If porting it requires `FX_Extrase`,
which `PLAN_ForexeIngest` §12 puts out of scope, that is to be reported, not worked around.

**O6.** `Descriere` on a history block matches the reception description, but operators rarely write
anything distinctive there, so it is not usable as a discriminator today. Parked: it becomes the
carrier for the real ForexeBug reception id once K-BOT posts back to the site — at which point DUBII
stops being necessary for anything created from then on.
