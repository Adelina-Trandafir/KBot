# HANDOFF — slice 0081: sending a DDF revision from KBOT to forexecab

**For:** the next Claude Code thread. **Written:** 25.09.2026, at the end of the thread that
produced the analysis and the plan. Read this first, then do what §4 says.

---

## 1. Read, in this order (all of it, before any code)

1. `docs/worklog/CODE_WORKFLOW.md` — how work is done here.
2. `docs/worklog/KBOT_STATUS.md` — the row for **0081** (next free is now 0082).
3. `docs/FUNDAMENT_DocumentFundamentare_CAB.md` — the flow, the form, decisions **D1-D9**.
4. `docs/PLAN_DDF_Trimitere.md` — the plan: states S0-S4 (with S2a / S2b), sub-slices 0081-01 … 0081-06, the
   operator's answers, the risks.
5. Only when a sub-slice needs it: `Surse/GHID UTILIZARE_ALOP_V2.pdf` (the Ministry guide) and
   `Surse/doc_fund_xdp.xml` (the DDF XFA template, `A1.0.08`).

## 2. The fifteen things the previous thread established — do not re-open them

1. **Scope = SENDING only, KBOT → forexecab.** The import (forexecab → KBOT: `prelucrare_pasi.py`,
   the DDF generation in `ddf_edit.py`, the read workflows) is **complete and correct — never
   change it, never ask about it.** It is only *called*, unchanged, after a send.
2. Only two form options: A.4 «Se stabilește ținând cont de» and Section B option 1.
   Point 5 is always «în anul curent se anticipează…» + «se sting în anul curent…» (already
   written by `DdfXmlBuilder`).
3. Flow per revision: Section A → **interim PDF** (no B, no captures) → operator signs A → send
   to forexecab → KBOT fills B + captures → **final PDF** → operator signs A and B → the
   **director** signs in a simplified KBOT. The operator and the forexecab person are the same
   person.
4. Only A signed = still editable (same revision). Sent = frozen; any change = new revision.
5. KBOT and forexecab are always in sync — "before" values from either.
6. **Golden rule: only KBOT edits forexecab.** What happens to an angajament belongs to its last
   DDF revision.
7. The DDF **does** carry captures: `SubformSectiuneaB/Subform51/Table4/Row1/Cell1`, PNG base64,
   final PDF only. Stored as `PrtScr = 1` attachments (the slot already exists).
8. The reason text sent to forexecab keeps the `(REV:n)` tag.
9. «Definitivează» / «Derulează» exist **only for a NEW angajament** (its Rev 0, state S2a).
   Later reservation changes — «Adaugă rezervare», even with new indicators — have **no**
   definitivare / derulare and go straight to the final PDF. They are **not** part of the send. They live on the **Rezervări** tree
   footer LEFT icon (not the main KBOT tree — its footer and «Extrase de cont» are untouched), by
   angajament state (`Inițial` → «Definitivează», `În definitivare` → «Derulează»).
10. **Entry points:** a new **«Angajament nou»** button on the main header, top left, before «An»
    → `DdfEditForm` in create mode; the Rezervări footer LEFT icon also offers **«Adaugă
    rezervare»** (angajament with at least the initial reservation) → `DdfEditForm` in manual
    add-reservation mode.
11. **Nothing is added to a PDF once it is final.** The interim PDF is partial because captures
    come later; the final PDF is generated after ALL forexecab actions of the revision (send +
    «Definitivează» / «Derulează»), then signed from scratch (A, B, director). Hence state **S2a**
    («Trimis — în lucru») before **S2b** («PDF final — de semnat»).
12. The Rezervări footer LEFT is **one icon with a menu** showing only the valid options.
13. **One open revision at a time** per angajament: no new revision while one has no final PDF.
14. **Variant (a):** a new angajament's Rev 0 gets its final PDF only after «Derulează», through
    «Generează PDF final» in the Rezervări menu. The menu therefore shows **at most one option**:
    `Inițial` → «Definitivează», `În definitivare` → «Derulează», `În derulare` with Rev 0 open →
    «Generează PDF final», `În derulare` with no open revision → «Adaugă rezervare». Full table in
    the plan, 0081-04.
15. Role **`Director`** (plain Romanian, `Unitati_Utilizatori.Rol`); like `Contabil`, it spans
    several units.

## 3. House rules that bite on this slice

- **No tests run, no git.** Write tests; never run them; never build test projects; the operator
  commits. (Memory: `never-run-tests`, `never-git`.)
- Rule 0: no diacritics anywhere in code except operator-visible strings.
- `.wfl` edits in `src/KBot.Forexe/Workflows/Creare/`, never in `Surse/` (snapshots).
- Every message box through `KBotMessage.Show`; every control in `.Designer.vb`; colours from the
  theme; forms `AutoScaleMode.Dpi`.
- `MariaDB_Schema/` is the truth for columns (gitignored, on disk) — read it before any SQL.
- Plain words in explanations to the operator; she writes in Romanian, answer in Romanian.
- Each sub-slice ends with its worklog `docs/worklog/SLICE-0081-0N-<slug>.md` and the STATUS row
  updated.

## 4. Where to start

**0081-01 — G0a, the revision state** (plan §0081-01). It is the foundation: every other
sub-slice reads it.
1. Open and read `src/KBot.Domain/DdfDraft.vb`, `src/KBot.App/Views/DdfView.vb`, the DDF read route
   that feeds `DdfView`, and `PYTHON/routes/forexe/pdf.py` (the `X-Semnatura` handling).
2. Confirm the plan against the code (CODE_WORKFLOW §1.3). Report any mismatch before coding.
3. Settle the one check the plan leaves for this step: does an **empty** `X-Semnatura` reset
   `Semnatura` to `''`, or is it treated as absent?
4. Implement: the pure state function in `KBot.Domain` (S0-S4 + S1x), the state shown per revision
   in `DdfView` (NOT on the arrow icon — that is the sign of the total), `Incarcat` / `Semnatura`
   carried by the read route if missing.
5. Worklog + STATUS, then stop and report to the operator before 0081-02.

Then 0081-02 … 0081-06 in the plan's order. **G3 is dropped** — the menu table offers «Adaugă
rezervare» only on angajamente `În derulare`, so the question once planned for 0081-03 is already
answered (plan, G3).
