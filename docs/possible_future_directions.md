# Possible future directions

Things that were deliberately deferred out of a slice, with enough context to pick them up
later. Not a backlog of ideas — only decisions already taken, with the reason for the delay
and the question that still needs answering.

Each entry says: **what**, **why it was deferred**, **what has to be decided first**.

---

## Per-year reset of `FX_ORD.NrORD` and `FX_DDF.CUAL`

*Raised while writing slice 0049 (ORD edit form). Deferred by the operator on 01.09.2026.*

In the Access system there was **one database per year**, so `DMax("NrORD", "FX_ORD")` and
`DMax("CUAL", "FX_DDF", "DC='…'")` were *implicitly* per-year — the reset came from the
file, not from the code. In MariaDB one database spans years, so both counters now run
continuously across years.

The operator wants them reset per year. It is **deferred to its own slice** because:

* it touches DDF as well as ORD, so it is not an ORD-only change;
* it needs a decision on **which year governs** — `YEAR(DataORD)` / `YEAR(DataCreare)`, or
  the session `AN` from `Unitati_Ani`. The two diverge when a document is back-dated across
  the new year, and picking the wrong one produces duplicate numbers within a year, which is
  exactly the failure the reset is meant to prevent.

Existing rows keep the numbers they have; only the allocation rule changes. Slice 0049
allocates `NrORD` as `MAX(NrORD) + 1` over the whole table, inside the save transaction
(decision D8), and drops the Access `DC='…'` predicate because one database is now one unit
and the predicate selected everything anyway.

---

## The 25-partner cap on an ordonanțare

*Raised while writing slice 0049.*

`qFX_ORD_BASE` carries `TOP 25` **inside the saved query**, and `Populeaza_PartSel` repeats
the limit. Slice 0049 ports it faithfully (`LIMIT 25` with the same `ORDER BY`, because
*which* 25 partners get picked is a business rule, not an accident), and ports the warning
that fires when a day needs more than one document.

**The open question:** should the cap still exist at all, now that the base query runs on
MariaDB rather than on the JET engine? If the number came from a printed-form constraint
(how many beneficiaries fit on one ordonanțare), it stays. If it came from a JET query
limit, it is an artefact. Nobody has said which. **Do not change the limit until someone
does** — a silently larger document would reach the treasury looking wrong.

---

## `FX_ORD.CUAL` is `varchar`, `FX_DDF.CUAL` is `int`

*Raised while writing slice 0049; the same finding as `SLICE-0045-01` item 3.*

The ORD copies the DDF's CUAL, so slice 0049 converts int → text on the way in. That is
safe in one direction and assumes the stored text is always the DDF's integer.

**To check on live data:** does any unit's `FX_ORD.CUAL` hold something that is *not* an
integer? If so, the column is carrying a second meaning nobody has written down, and the
conversion has to learn about it before the two columns can be reconciled.

---

## `Explicatie` on `FX_ORD_TBL`: computed or edited?

*Raised while writing slice 0049.*

Access binds `Explicatie` in `frmFX_ORD_TBL` as an editable textbox, and slice 0049 keeps it
editable. But in the generation path it is always **computed** — from the payment's
description, or from `Incarca_Explicatii_Incasari` for negative values.

**The question:** does the operator ever actually edit it? If in practice it is always
computed, an editable column invites divergence between what the document says and what the
data means. If it is edited, the computation is only a default and should be documented as
such.

---

## `Incarca_Explicatii_Incasari` filters on a contract code that cannot exist

*Found while writing slice 0049, ported faithfully, needs an operator decision.*

The VBA reads:

```vba
WHERE FX_Extrase.NrDoc IN (SELECT NrDoc FROM FX_Extrase
                            WHERE CodContract='<cod>' AND Left(nrdoc,5)='00000')
  AND FX_Extrase.CodContract='ERRRRRRRRRR'
```

No contract is called `ERRRRRRRRRR`, so **the dictionary always comes back empty** and every
incasare gets `"LIPSA EXPLICATIE"` as its line explanation and `"INCASARE"` as its
justifying document. It reads like a debugging sentinel left behind.

Slice 0049 ports the defect faithfully (the same call as D18 in slice 0048-01: port it,
record it, do not repair it silently), but isolates it in a named constant —
`EXPLICATII_FILTRU_CODCONTRACT` in `routes/forexe/ord_edit.py` — so it is visible and fixable
in one line. **If the operator confirms it is a mistake, deleting that second predicate is
the whole fix.**

---

## `BIC` has no MariaDB successor

*Found while writing slice 0049.*

`Incarca_DicBanci` reads a table called `BIC` (bank code → bank name) to fill
`FX_ORD_PART.Banca`. That table exists nowhere in the migrated system: not in
`MariaDB_Schema/000_DEMO.sql`, not in `FX_System_Export/TABLES`, and no Python route mentions
it. It lived in the Access front-end.

Slice 0049 probes for it (`information_schema`) and, when it is absent, leaves `Banca` empty
and says so in a warning — the field is informative, not a foreign key, and inventing a
second source for a bank name would be worse than leaving it blank.

**To decide:** either migrate the `BIC` table into the unit databases (or into
`AVACONT_COMUN`, since bank codes are not per-unit), or accept that `Banca` is typed by hand.

---

## `FX_ORD_ATT.Imagine` — is it really dead?

*Found while writing slice 0049 (decision D9).*

`FX_ORD_ATT.Imagine` is `longtext` holding base64. In `000_DEMO` the whole table is **empty**
(`AUTO_INCREMENT = 1`) while `FX_ORD_DOC` sits at 719 and `FX_ORD_TBL` at 891 — i.e. the
screenshot feature was never really used. Slice 0049 therefore stores attachment bytes in a
new blob table (`FX_ORD_ATT_IMG`) and leaves `Imagine` in place, never written, never read.

**The finding is from the demo dump only.** Before treating the column as dead:

```sql
SELECT COUNT(*) AS randuri,
       SUM(CASE WHEN Imagine IS NOT NULL AND Imagine <> '' THEN 1 ELSE 0 END) AS cu_imagine
  FROM <BAZA>.FX_ORD_ATT;
```

If any live unit has rows with content, the old bytes have to be moved into the new table
before the column can be ignored.

Note that `FX_ORD_DOC` keeps its own base64-in-`DocJust` file attachments unchanged — that
one **is** live (719 rows), so slice 0049 ports it as it stands rather than migrating it.
Whether the two attachment mechanisms should be unified is a separate question.
