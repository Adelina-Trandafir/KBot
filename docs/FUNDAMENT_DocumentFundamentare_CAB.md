# Document de fundamentare (DDF) — SENDING from KBOT to forexecab

**Status:** findings only, no code. Written 25.09.2026, revised the same day after operator review.
**Sources:**
- `Surse/GHID UTILIZARE_ALOP_V2.pdf` — the Ministry of Finance guide for the DDF under
  OMF 1140/2025. `p.NN` = the guide's page.
- The FOREXEBUG workflows in `src/KBot.Forexe/Workflows/Creare/`:
  `adlop - Creare Angajament.wfl` (identical copy in `Surse/`),
  `adlop - Definitivare_Derulare Angajament.wfl`, `adlop - Incarca Rezervare.wfl`.
- The DDF template `Surse/doc_fund_xdp.xml` (`A1.0.08`) and `src/KBot.App/Views/Ddf/DdfXmlBuilder.vb`.

---

## THE SCOPE — read this first

> **We work ONLY on the SENDING flow: from KBOT into forexecab.**
>
> **The IMPORT flow (forexecab → KBOT) is complete and correct. It is NOT part of this work and
> is NOT changed.** That covers everything already in place: reading the angajamente, the
> reservations, `Istoric`, the receptions, and generating DDFs from what was read
> (`PYTHON/routes/forexe/prelucrare_pasi.py`, `ddf_edit.py`, the read workflows).
> (Operator, 25.09.2026.)

Within the sending flow, KBOT uses exactly two options of the form:

| Where | Option used |
|-------|-------------|
| Section A, point 4 | **`Se stabilește ținând cont de:`** — the value table |
| Section B | **`Propunerile de la secțiunea A au fost înregistrate în sistemul de control al angajamentelor după cum urmează:`** — the table filled from forexecab |

Everything else on the form is out of scope and left at its default: the `rămâne în sumă de` option
of point 4, point 5 beyond its default choice (2.3), Section B's `Nu s-au rezervat ...` option, and
the header checkbox for legal obligations / third parties. Also out of scope: accounting and the
Ordonantare de plata.

Quoted labels are copied exactly as the operator sees them in forexecab or in the PDF form.
Source marks: `GUIDE` = guide text, `SCREEN` = guide screenshot, `WFL` = workflow.

---

## 1. The flow (operator, 25.09.2026)

At KBOT's clients the department and the person working in forexecab are **the same person**, so
there are two roles: the **operator** and the **director** (Ordonator de credite).

Two PDFs per revision: an **interim** one (header + A, no B, no captures) and a **final** one
(header + A + B + captures).

```
 OPERATOR in KBOT                          KBOT → forexecab (existing workflows)
 ────────────────                          ─────────────────────────────────────
 1. New angajament or new revision:
    header + Section A (A.4 table)
 2. KBOT generates the INTERIM PDF
    (no Section B, no captures)
 3. Operator signs A on the interim PDF
    - while only A is signed, values can
      still be edited → new interim PDF,
      signed again
 4. Operator sends it to forexecab ──────► 5. Rev 0 → Creare Angajament
                                              Rev ≥ 1 → Incarca Rezervare
                                              + Definitivare_Derulare when the contract
                                                is signed in this revision
                                           6. New angajament → the real CodAngajament
                                           7. Captures
                                           8. Grid after → Section B values
                                           9. The existing import runs for this
                                              angajament (unchanged) — §5
10. KBOT fills the missing fields (code,
    Section B, captures) and generates
    the FINAL PDF
11. Operator signs A and B on the final PDF
12. The director gets a message in a
    simplified KBOT that DDFs are waiting,
    and signs them. END OF THE FLOW.
```

Rules that follow from it:
- **When is it a new revision.** While only Semnătura A is placed, the same revision is edited (new
  interim PDF, signed again). Once the revision is sent to forexecab and fully signed, any further
  change of values is a **new revision**. Step 4 is the point of no return.
- The interim PDF is a working copy; its A signature does not carry over — A is signed again on the
  final PDF.
- The revision's values (header, A.4, B rows, CodAngajament, captures) are stored in KBOT per
  revision from step 1; the final PDF is built from them.
- Director side: a simplified KBOT that lists the DDFs waiting for his signature (new piece, not
  designed yet).
- **Golden rule: nobody changes an angajament in forexecab by hand — only KBOT does.**

---

## 2. The DDF form (the parts in scope)

### 2.1 Header (GUIDE p.3)

| Field | Rule |
|-------|------|
| `Instituția publică`, `Cod de identificare fiscală` | Institution |
| Title box under "DOCUMENT DE FUNDAMENTARE" | The object |
| `Număr unic de înregistrare` | **Never changes** over the life of the spending |
| `revizuirea` | 0, then 1, 2, ... |
| `data` | Date of this revision |

### 2.2 Section A (GUIDE p.4-10)

| # | Field | Rev 0 | Rev ≥ 1 |
|---|-------|-------|---------|
| 1 | `Compartiment de specialitate` | Initiating department | Unchanged |
| 2 | `Descrierea pe scurt ... /motivul revizuirii` | Short object | **Reason for the revision** |
| 3 | `Descrierea pe larg a stării de fapt și de drept` | Context and arguments | What changed versus the previous revision |
| 4 | `Valoarea angajamentelor legale` → **`Se stabilește ținând cont de:`** | table below | table below |

Point 4 table, one row per SSI, plus TOTAL (GUIDE p.6-7, SCREEN p.7):

| Col | Header | Rule |
|-----|--------|------|
| 1 | Element de fundamentare | Short description |
| 2 | Program | Program code; `0000000000` for sector 02 (Buget local) and sources without programs |
| 3 | Cod SSI | Sector + Sursă + functional + economic classification, no separators (`01A510103564801`) |
| 4 | Parametrii de fundamentare | Optional |
| 5 | Valoare totală revizie precedentă | Rev 0 → 0; Rev n → col 7 of Rev n-1 |
| 6 | Influențe +/- | Rev 0 → the value; Rev n → new − previous |
| 7 | Valoarea totală actualizată | 5 + 6 |

Then `Validez antet și secțiunea A` and the Section A signature.

### 2.3 Point 5 — left at the default

The validation button checks point 4 **and** point 5 (GUIDE p.28), so point 5 cannot be left
empty. Every example in the guide ticks `în anul curent se anticipează emiterea a cel puțin unui
angajament legal / ...` and under it `se sting în anul curent toate obligațiile de plată`.
`DdfXmlBuilder.vb` already writes `Subform5/CheckBox5 = 1`, `CheckBox6 = 1`, next to
`Subform41/CheckBox2 = 1` ("Se stabilește ținând cont de").

### 2.4 Section B — option 1 (GUIDE p.11-13)

Tick **`Propunerile de la secțiunea A au fost înregistrate în sistemul de control al angajamentelor
după cum urmează:`**. One row per forexecab budget row, plus TOTAL:

| Col | Header | Source in forexecab |
|-----|--------|---------------------|
| 1 | Cod angajament | Angajament header (`Cod:` / `Număr angajament:`) |
| 2 | Indicator angajament | `Indicator ang` column (`AAB`, `AA2` ...) — the ROW's code |
| 3 | Program | `Program` column |
| 4 | Cod SSI | `Sector - Sursa - Indicator`, concatenated |
| 5 | CA rezervat pentru anul curent, revizia precedentă | The value **before** this send. Rev 0 → 0 |
| 6 | Influențe +/- (CA) | The change |
| 7 | = 5 + 6 | |
| 8 | CB rezervat pentru anul curent, revizia precedentă | The value **before** this send. Rev 0 → 0 |
| 9 | Influențe +/- (CB) | The change |
| 10 | = 8 + 9 | |

The "before" values can come from KBOT or from forexecab — they are always in sync (operator).

`DdfXmlBuilder.vb` already writes this table: `SubformSectiuneaB/Table3` with `CheckBox9 = 1`
(XFA), and `sectiuneaB` rows with `ckbx_secta_inreg_ctrl_ang = 1` (XML) — col 5/6/8/9 given,
col 7/10 computed.

**Captures** go under the table (form label `Captura de imagine/imagini din sistemul de control al
angajamentelor bugetare este redată în rubrica de mai jos sau ca anexă la documentul de
fundamentare`, SCREEN p.30, p.36). In the template:

```
SubformSectiuneaB
  Subform51
    Table4
      Row1            ← hidden template row, written empty (like Table1 / Table3)
      Row1            ← one per image
        Cell1  xfa:contentType="image/png"   → the PNG as base64 text
```

`DdfXmlBuilder.vb` does **not** write `Table4` yet — G5. Other Section B nodes
(`Subform52/Subform522`: `CheckBox10`, `creditAngajament`, `creditBugetar`, `CheckBox11`,
`CheckBox12`; `Subform521/Intrucit`) stay at 0.

Then `Validez secțiunea B`, the signatures.

### 2.5 A and B must agree (GUIDE p.11-12)

Per SSI:

```
A.col5  =  B.col5  =  B.col8      (previous)
A.col6  =  B.col6  =  B.col9      (change)
A.col7  =  B.col7  =  B.col10     (updated)
```

Before sending a Rev ≥ 1, A.col5 must equal the current reserved value; if not, the revision is
wrong and is not sent (GUIDE p.11-12: "va restitui documentul de fundamentare").

---

## 3. forexecab — the screens the send goes through

### 3.1 States (SCREEN p.35, 41, 48-49)

```
 "Angajament nou" ──► "Inițial"            [Anulează] [Definitivează]
                         │ Definitivează
                         ▼
                      "În definitivare"    [În derulare] [Anulează]
                         │ În derulare  (= legal commitment signed)
                         ▼
                      "În derulare"        [Reziliază] [Suspendă] [Transferă]
```

One forexecab row = one (Program, Sursă, Indicator) = one row in A.4 and in B. forexecab assigns
the row code. After a save: green `Confirmare — Succes Rândul angajamentului a fost salvat.`

### 3.2 The sending workflows

**`Creare Angajament`** — new angajament (Rev 0).
Inputs: `DESCRIERE_ANGAJAMENT`; rows `{Cheie, Clasificatia, CodProgram, Sursa, CB_INITIAL}`.
- `Angajament nou` → description → `Adaugă angajament` (the whole header form).
- Per row: Program, Sursa, Indicator, types `creditBugRezervatInitialAnCurent` = `CB_INITIAL`
  (forexecab fills `Credit de angajament rezervat` itself); reads CA/CB Total/Limită/Disponibil;
  **screenshot of the filled row**; `Salvează și continuă` (or `Salvează` on the last row).
- **Final screenshot**, reads `CodAng_Final`, scrapes the grid (`TabelIndicatori`).

**`Definitivare_Derulare Angajament`** — `Inițial` → `În derulare`.
Inputs: `COD_ANGAJAMENT`, `MOTIV_DEFINITIVARE`.
- Scrapes the grid first; `Definitivează`; per row copies CB inițial into
  `creditBugRezervatDefinitivAnCurent` → **screenshot** → `Salvează`; `În derulare`; reads the
  state and scrapes the final grid.

**`Incarca Rezervare`** — change or add rows on an existing angajament (Rev ≥ 1).
Inputs: `COD_ANGAJAMENT`; rows `{Cheie, Indicator_ang, ClsfSal, Clasificatia, CodProgram, Sursa,
ValoareNoua, Motiv}` (`Indicator_ang` starting with `!` = new row).
- Existing row: eye → `creditBugRezervatDefinitivAnCurent` = `ValoareNoua` → **screenshot** →
  `Salvează` → motiv → `Continuă` → re-reads the row.
- New row: `Adaugă` → Program / Sursa / Indicator → `creditBugRezervatDefinitivAnCurent` →
  **screenshot** → `Salvează` → reads the new row code.
- Then the last `Istoric` row; at the end a **final screenshot**.

The reason text sent to forexecab keeps the `(REV:n)` tag (e.g. `Incarcare rezervare definitiva
(REV:7)`) — operator decision: whoever reads forexecab sees which revision made the change.

### 3.3 From the DDF to the workflow inputs

| Workflow input | From the DDF |
|----------------|--------------|
| `DESCRIERE_ANGAJAMENT` | Title / A.2 of Rev 0 |
| `CB_INITIAL` (Rev 0) / `ValoareNoua` (Rev ≥ 1) | A.4 col6 / col7, per SSI |
| `CodProgram`, `Sursa`, `Clasificatia` | A.4 col2 and col3 |
| `Motiv`, `MOTIV_DEFINITIVARE` | A.2 of the revision + `(REV:n)` |

| Workflow output | Into the DDF |
|-----------------|--------------|
| `CodAng_Final` | Header / B.col1 |
| Row codes (`Indicator ang`) | B.col2 |
| Grid after the action | B.col6 / col9 (and 7 / 10) |
| `Poza_*` screenshots | B captures (`Table4`) |

---

## 4. The scenarios

### S1 — New angajament (Rev 0) — GUIDE Examples 2 Rev 0, 3

| Step | What |
|------|------|
| 1 | Header rev 0; A.4 per SSI: col5 = 0, col6 = col7 = value; interim PDF; sign A |
| 2 | `Creare Angajament`, `CB_INITIAL` = A.col6 per SSI |
| 3 | If the legal commitment is already signed in this revision (salaries): `Definitivare_Derulare` |
| 4 | B: col1 = `CodAng_Final`, col2-4 from the grid, col5 = col8 = 0, col6 = CA rezervat, col9 = CB rezervat; captures into `Table4`; final PDF; sign A + B |
| 5 | Director signs |

### S2 — Revision of the amount (Rev ≥ 1) — GUIDE Example 2 Rev 1

| Step | What |
|------|------|
| 1 | Same number, rev +1, new date; A.2 = reason; A.4: col5 = previous col7, col6 = change, col7 = new; check col5 against the current value (2.5); interim PDF; sign A |
| 2 | `Incarca Rezervare`, existing row, `ValoareNoua` = A.col7. A new SSI in this revision = a new row (`!` prefix) with col5 = col8 = 0 |
| 3 | If the contract is signed now and the angajament is not yet `În derulare`: `Definitivare_Derulare` |
| 4 | B: col5/col8 = before, col6/col9 = the change, col7/col10 = new; captures; final PDF; sign A + B |
| 5 | Director signs |

### S3 — Carried over from previous years — GUIDE Example 1 (art. 47)

The angajament is already `În derulare`; the unpaid remainder is reserved on this year's budget.
- Rev 0 of a new DDF; A.4: col5 = 0, col6 = col7 = remaining unpaid.
- `Incarca Rezervare`, existing row, `ValoareNoua` = remaining unpaid.
- B: col5 = col6 = col7 = 0 (no CA reserved), col8 = 0, col9 = col10 = remaining unpaid.
  **Only the CB side moves** (SCREEN p.30).

---

## 5. After the send

Right after the workflows finish, the **existing import** runs for that angajament, **unchanged**.
Because of the golden rule (only KBOT edits forexecab), everything new it brings for the
angajament's reservations belongs to the revision just sent — nothing else could have made it.
The `(REV:n)` tag in the reason text is there as well. No change to the import is needed or made.

---

## 6. Gaps in the sending flow

| # | Gap | Where |
|---|-----|-------|
| G1 | No screenshot after `Definitivează` and after `În derulare`; the guide captures both pages (SCREEN p.48-49) | `Definitivare_Derulare` |
| G2 | No `Informații complete contract` screenshot for S3 (SCREEN p.30) | `Incarca Rezervare` |
| G3 | On an angajament still `În definitivare`, the guide changes both `inițial` and `definitiv` (SCREEN p.41); the workflow changes only `definitiv` | `Incarca Rezervare` |
| G5 | The captures are not written into the PDF: no `Subform51/Table4` rows yet | `DdfXmlBuilder.vb` |
| — | The interim → send → final sequence, the per-revision storage of the send, and the director's simplified KBOT are not built | new |

What the guide captures, against what the workflows take today:

| Situation | Guide | Workflow today |
|-----------|-------|----------------|
| New row(s) | Filled row form + `Administrare angajament` with the toast (p.35) | Per row + final ✔ |
| Existing row changed | Filled `Modifică rând` + page after save (p.41) | Per row + final ✔ |
| State reached (`În definitivare`, `În derulare`) | One page per state (p.48-49) | None — G1 |
| Carried over (S3) | `Modifică rând` + `Informații complete contract` (p.30) | First only — G2 |

---

## 7. Decisions taken (operator, 25.09.2026)

| # | Decision |
|---|----------|
| D1 | Scope = SENDING only. The import from forexecab is complete, correct, untouched. |
| D2 | Only A.4 "Se stabilește ținând cont de" and B option 1 are used. |
| D3 | Interim PDF (A only) → send → final PDF (A + B + captures); the operator signs A and B, the director signs last in a simplified KBOT. |
| D4 | Only-A-signed = still editable (same revision); sent and fully signed = frozen, changes need a new revision. |
| D5 | The revision's values are stored in KBOT per revision. |
| D6 | KBOT and forexecab are always in sync; "before" values from either. |
| D7 | The DDF carries screen captures (`Subform51/Table4`), on the final PDF only. |
| D8 | Golden rule: only KBOT edits forexecab. |
| D9 | The `(REV:n)` tag stays in the reason text sent to forexecab. |

No open questions.

---

## 8. Summary

1. **This work is the SENDING flow, KBOT → forexecab.** The import is done and stays as it is.
2. **Per revision:** Section A in KBOT → interim PDF signed on A → the existing workflows act in
   forexecab (`Creare Angajament` / `Incarca Rezervare` / `Definitivare_Derulare`) → KBOT fills
   Section B and the captures → final PDF signed on A and B → the director signs.
3. **A.4 and B match number for number**; B's "before" values are the current reserved values.
4. **One forexecab row = one Program + SSI = one row in A.4 and in B.**
5. **Already built:** the three workflows, and Section B's table in `DdfXmlBuilder.vb`.
   **Missing:** G1-G3, G5, and the interim → send → final sequence with the director's step.
