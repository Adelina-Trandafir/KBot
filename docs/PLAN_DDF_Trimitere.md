# PLAN — Sending a DDF revision from KBOT to forexecab (G0-G6)

**Status:** plan, no code. Written 25.09.2026.
**Parent document:** `docs/FUNDAMENT_DocumentFundamentare_CAB.md` (the flow, the form, the decisions
D1-D9). This plan builds what that document lists as missing.
**Slice:** **0081**, sub-slices **0081-01 … 0081-06** (confirmed by the operator, 25.09.2026).
**Scope reminder:** SENDING only. The import from forexecab is complete and correct and is
**not touched** by any step of this plan — it is only *called*, unchanged (step 4).

House rules that apply to every step: no tests run and no git (the operator does both); tests are
written, never executed; `.wfl` files are edited in `src/KBot.Forexe/Workflows/Creare/`, never in
`Surse/`; operator text in Romanian, code in English, no diacritics outside operator text.

---

## What already exists (read, 25.09.2026)

| Piece | Where | What it gives this plan |
|-------|-------|-------------------------|
| DDF editor | `src/KBot.App/DDF_EDIT/DdfEditForm.vb` + 4 pages (slice 0051) | Header, Section A page, **Section B page**, Descriere, Fisiere; one POST save |
| Section B derived from A | `DdfDraftRevizie.RecalculeazaSectiuneaB()` (`KBot.Domain/DdfDraft.vb`) | B is rebuilt from A on every A edit — CA and CB halves = A's numbers |
| Manual angajament | `DdfDraft.Manual` — code starting with `!` | A DDF can exist before forexecab knows the angajament |
| Print-screen attachments | `DdfDraftAtt.PrtScr` (`FX_DDF_REV_ATT` / `_IMG`) | «rows with `PrtScr = 1` arrive only from the future FOREXE workflow» — the slot for captures already exists |
| XFA builder | `src/KBot.App/Views/Ddf/DdfXmlBuilder.vb` | Writes header, A (`Table1`, point 5 ticks), B (`Table3`); **no `Table4`** |
| Signing | slice 0078: Adobe viewer, signature detection on save, `X-Semnatura` → `FX_DDF_REV.Semnatura` (`A,B,Ordonator`) in the same transaction as the PDF | The signature state of a revision is already stored |
| Signature journal | slice 0079: `FX_PDF_SEMNATURI` | Who signed, when, from where |
| `Incarcat` flag | `FX_DDF_REV.Incarcat` | «this revision is in forexecab» |
| Roles | `Unitati_Utilizatori.Rol`, returned at login as `Role` → `SessionContext.Role` | Base for the director's KBOT |
| Workflow runner | `KBot.Forexe/ForexeRunner.vb`, `App/Forexe/ForexeController.vb` | How the app runs a `.wfl` with variables and reads its results |
| Screenshots | `<Screenshot SaveTo=...>` → full-page PNG as **base64** in a variable | Fits `Table4` (`image/png`, base64) directly |
| Sending workflows | `Creare Angajament`, `Incarca Rezervare`, `Definitivare_Derulare Angajament` | Exist, **not wired in the app** (only the old Access app called them) |

---

## The revision states (the backbone — G0)

Derived, **no new column**: from `Incarcat` and `Semnatura` of `FX_DDF_REV`.

| # | State (operator text) | `Incarcat` | `Semnatura` | What the operator can do |
|---|-----------------------|-----------|-------------|--------------------------|
| S0 | «Ciornă» | 0 | empty | Edit A; generate the interim PDF |
| S1 | «Semnat A — gata de trimis» | 0 | `A` | Edit A (→ new interim PDF, signature dropped, back to S0); **send to forexecab** |
| S2a | «Trimis în FOREXE — în lucru» | 1 | `A` (still the interim PDF) | **Only a NEW angajament's Rev 0.** A is frozen (D4). «Definitivează», then «Derulează»; each capture is added. Then «Generează PDF final» |
| S2b | «PDF final — de semnat A și B» | 1 | empty (the final PDF, unsigned) | Sign A and B on the final PDF. No more forexecab actions on this revision |
| S3 | «Semnat A și B — la director» | 1 | `A,B` | Wait for the director |
| S4 | «Aprobat» | 1 | `A,B,Ordonator` | End of the flow |
| S1x | «Trimitere întreruptă» | see step 4 | `A` | Resume the send (step 4, partial failure) |

**S2a exists only for a new angajament (operator, 25.09.2026).** «Definitivează» / «Derulează»
belong to the creation of an angajament. Every later reservation change — «Adaugă rezervare»,
even one that adds new indicators — has **no** definitivare / derulare step: after its send it goes
straight to the final PDF (S1 → S2b), no S2a.

**Why S2a exists (operator, 25.09.2026).** The interim PDF is partial precisely because the
captures come later. The final PDF is generated only once every forexecab action of the revision
is done (send + any «Definitivează» / «Derulează»), and it is then signed again from scratch: A, B,
director. Nothing is ever added to a PDF after it is final.

Check before building: that uploading the **final** PDF (no signatures yet) resets `Semnatura` to
empty. The PDF route only touches the column when `X-Semnatura` is present (`pdf.py` docstring), so
the final upload must send it explicitly empty. **To verify in `pdf.py`** that an empty header
writes `''` rather than being treated as absent.

---

## Order of work

```
0081-01  G0a  revision state: derive + show              (foundation, everything reads it)
0081-02  G6a  editor: create / add-reservation modes, Section B hidden until sent,
              interim PDF without B; entry points («Angajament nou», Rezervări footer)
0081-03  G1 G2     workflow edits (G3 dropped)                        (independent, only .wfl files)
0081-04  G6b  THE SEND: run the workflows, write back, final PDF
0081-05  G5   captures into the PDF (Table4)
0081-06  G0b  the director's KBOT
```

G4 is not in the list: it was closed (D6 — "before" values from KBOT or forexecab, always in sync).

---

## 0081-01 — G0a: the revision state

**What.** One function that turns (`Incarcat`, `Semnatura`) into S0-S4, used everywhere.
**Where.**
- `KBot.Domain`: an enum + a pure function (easy to test, no I/O).
- `DdfView.vb`: show the state per revision. Not on the arrow icon — that now means the sign of the
  total (`StareOf`, comment at line ~897 says a separate visual sign is needed) — so a second
  marker: a right-side icon or a column «Stare».
- The GET that feeds `DdfView` must already carry `Incarcat` and `Semnatura`; **to verify**, add
  them to the route's SELECT if missing (a read-only addition to the DDF read route — that route
  belongs to the DDF view, not to the import).
**Done when.** Each revision in the DDF view shows its state; tests on the pure function (not run).

## 0081-02 — G6a: Section B not shown before the send

**The operator's two options:** (1) create Section B only after forexecab answers, or (2) create it
from the start but do not show it.

**Recommendation: option (2).** Reasons, from the code:
- Section B is **already** derived from A on every edit (`RecalculeazaSectiuneaB`), and by the rule
  A = B its numbers are right before the send. Building it later would mean removing working code
  and re-adding it.
- What forexecab returns that A does not know is only: the real `CodAngajament` (manual `!` code),
  the row codes (`Indicator ang`), and the captures. Those are filled in at the send (0081-04).
- The numbers returned by forexecab become a **check**, not a source: if the grid after the send
  differs from the derived B, the send stops with a message (a sync fault, D6).

**What changes.**
- Entry points (see 0081-04, «Where the operator starts»): the «Angajament nou» header button opens `DdfEditForm` in **create** mode; the Rezervări footer «Adaugă rezervare» opens it in **add-reservation** mode. The draft already carries `Nou`, `RevizieNoua` and `Manual`; read how `DdfEditForm.AplicaEnablement` and the draft endpoints (`/ddf/genereaza`, `/ddf/draft`) produce those today before adding anything.
- `DdfEditForm`: the «Secțiunea B» nav item is hidden while the revision is in S0/S1; visible,
  read-only, from S2a on. (`navSub` items are built in the designer — hide by key at run time, the
  same way pages are activated.)
- `DdfXmlBuilder`: an **interim** mode — no `Table3` rows, `CheckBox9 = 0`, no `Table4`. The final
  mode is today's output (+ `Table4`, step 0081-05).
- Where the interim PDF is generated (the DDF view / editor action that calls the builder): choose
  the mode from the state.
**Done when.** A new DDF / new revision never shows Section B, on screen or in the interim PDF.

## 0081-03 — G1, G2, G3: the workflow edits

Only `.wfl` files. Each needs **one live run on 000_DEMO** before it is trusted — selectors cannot
be verified offline.

**G1 — split `Definitivare_Derulare Angajament.wfl` into two actions, each with its capture.**
Operator answer 3: the two state changes are separate buttons, so one workflow doing both no
longer fits.
- `adlop - Definitivare Angajament.wfl` = today's SECȚIUNEA 1-6 (search, grid, «Definitivează»,
  per row CB inițial → CB definitiv), then `<Screenshot SaveTo="Poza_Definitivare"/>` after the
  last `WaitFor li.tab0.active`.
- `adlop - Derulare Angajament.wfl` = search + `Modificare` + today's SECȚIUNEA 7-8 («În
  derulare»), then `<Screenshot SaveTo="Poza_Derulare"/>` after the success toast.
- Keep the original file untouched until both new ones have run live (then retire it).

**G2 — `Incarca Rezervare.wfl`: «Informații complete contract» for carried-over amounts (S3 of
the parent document).**
- New optional variable `{{CAPTURA_INFO_COMPLETE|false}}`. When true, at the end: click
  `Afișează informații complete` → wait for `Informații complete contract` → `<Screenshot
  SaveTo="Poza_InfoComplete"/>` → `Înapoi`.
- KBOT sets it true when the DDF is a Rev 0 for an angajament that is already `În derulare`.

**G3 — `Incarca Rezervare.wfl`: both `inițial` and `definitiv` while `În definitivare`.**
**DROPPED (25.09.2026).** The approved menu table (0081-04) offers «Adaugă rezervare» only on
angajamente `În derulare`, and a new angajament cannot close its Rev 0 before «Derulează»
(variant (a)). So `Incarca Rezervare` always meets `În derulare`, where only `definitiv` exists —
which is what the workflow already does. Nothing to build; the text below is kept as the record of
why it was raised.
- Read the state label first (`.well.well-small span.label`, as `Definitivare_Derulare` does).
- `În definitivare`: fill `creditBugRezervatInitialAnCurent` **and** `creditBugRezervatDefinitivAnCurent`
  with `ValoareNoua` (the guide, SCREEN p.41).
- **Also to check live:** an angajament still `Inițial` (created, never definitivat) — the workflow
  waits for the `definitiv` input, which may not exist in that state. If so, `Inițial` →
  fill `inițial` only. This is a real hole today, not only a guide mismatch.

**Done when.** The three files parse (the XML check used in slice 0076), and each branch has been
run once on 000_DEMO by the operator.

## 0081-04 — G6b: THE SEND

The core step. Button «Trimite în FOREXE», enabled only in state S1.

**Sequence.**
1. Build the workflow inputs from the saved revision (never from the screen):

   | Case | Workflow | Input rows |
   |------|----------|------------|
   | Rev 0, new angajament (manual `!` code) | `Creare Angajament` | `{Cheie = "IdSecA-<id>", Clasificatia, CodProgram, Sursa, CB_INITIAL = A.col6}` |
   | Rev ≥ 1, or Rev 0 on an existing angajament | `Incarca Rezervare` | `{Cheie, Indicator_ang (or "!NOU" for a new SSI), ClsfSal, Clasificatia, CodProgram, Sursa, ValoareNoua = A.col7, Motiv}` |

   `Motiv` / `MOTIV_DEFINITIVARE` = A.2 + ` (REV:<n>)` (D9).
2. Run them through `ForexeController` / `ForexeRunner` (the same road as the existing
   downloads). The browser stays docked (slice 0070).
3. Collect: `CodAng_Final` / row codes, `TabelIndicatori[Finali]`, every `Poza_*`.
4. **Check** the grid against the derived Section B, per SSI. A difference → stop, message, state
   S1x.
5. One server POST, one transaction: real `CodAngajament` replaces the `!` code everywhere the DDF
   carries it (header, revision, A and B lines, attachments); row codes into B; captures stored
   as `PrtScr = 1` attachments; `Incarcat = 1`.
6. Run the **existing import** for that angajament, unchanged (parent document §5).
7. **New angajament (Rev 0 from «Angajament nou»):** state S2a. No final PDF yet: the operator
   goes on with «Definitivează» and «Derulează» from the Rezervări menu, and only then «Generează
   PDF final» (variant (a) — see the menu table). **Any other send** (Rev ≥ 1, or Rev 0 on
   an existing angajament): no S2a — the final PDF is generated right away. Either way: final PDF
   (0081-05 adds the captures), uploaded with an empty `X-Semnatura` → S2b → opened in the signing
   viewer.

**Partial failure — the dangerous case.** forexecab may be changed halfway. The revision is past
the point of no return.
- `Creare Angajament` must **never run twice** for one revision — a second run creates a second
  angajament. As soon as the angajament header exists, its code is saved on the revision (the
  workflow already reads it after every row: `CodAng_<Cheie>`). A resume continues with
  `Incarca Rezervare` for the rows not yet in the grid (`!NOU`).
- `Incarca Rezervare` on an existing row is safe to repeat (it sets a value, it does not add).
- State S1x = «Trimitere întreruptă», with the list of what is and is not in forexecab, and one
  action: «Reia trimiterea».

**Server side.** New route(s) under the DDF write path (`routes/forexe/ddf_edit.py` family, not the
import): the POST of point 5, and the resume data. The `!` → real code swap must be checked
against every table that stores the manual code — **to list from `MariaDB_Schema/` before writing
SQL**.

**Where the operator starts (operator, 25.09.2026).** Not the main KBOT tree — the
**Rezervări** view:
- **New angajament:** a new button **«Angajament nou»** on the main form's header, top left,
  **before «An»** (`KbotForm` `tlyHeader`, `lblAn` is today in column 1). It opens `DdfEditForm`
  in **create** mode (manual `!` angajament, Rev 0).
- **Existing angajament with at least one reservation (the initial one):** the **Rezervări tree
  footer LEFT** icon (`RezervariView`; free today — the right one is refresh). It offers
  **«Adaugă rezervare»**, which opens `DdfEditForm` in **manual add-reservation** mode (a new
  revision), and, by the angajament's state, **«Definitivează»** (`Inițial`) or **«Derulează»**
  (`În definitivare`).
- **One footer icon opening a `CustomPopup` menu** with the valid options only (operator,
  25.09.2026 — the pattern already used by the main form's options menu).
- «Definitivează» / «Derulează» exist **only for a new angajament**: offered only while the
  angajament's Rev 0 (created from «Angajament nou») is in **S2a**; their capture joins that
  revision. Never for later reservation changes, even with new indicators (operator,
  25.09.2026).
- The main KBOT tree footer (its left icon = «Extrase de cont») is **not** touched.

**The Rezervări footer menu — what it shows (operator, 25.09.2026).** Two rules decide it:
- **One open revision at a time.** While a revision of the angajament has no final PDF yet
  (S0, S1, S1x, S2a), no other revision can be started.
- **Variant (a):** a new angajament's Rev 0 can get its final PDF **only after «Derulează»** —
  so a new angajament can never be left `Inițial` / `În definitivare` with a closed Rev 0.

| Angajament (forexecab) | Last revision | Menu |
|------------------------|---------------|------|
| New, created in KBOT (`!` code), not sent | S0 / S1 | nothing (no reservations yet; the send is done from the DDF) |
| New, sent, `Inițial` | Rev 0 in S2a | «Definitivează» |
| New, sent, `În definitivare` | Rev 0 in S2a | «Derulează» |
| New, sent, `În derulare` | Rev 0 in S2a | «Generează PDF final» |
| `În derulare` | last one has its final PDF (S2b / S3 / S4) | «Adaugă rezervare» |
| any | a later revision still open (S0 / S1 / S1x) | nothing (finish it from the DDF) |
| `Anulat` / `Reziliat` / `Suspendat` | — | nothing |

So the menu never shows two options at once. «Generează PDF final» lives in this menu (it closes
the new angajament's Rev 0), and it only appears once the angajament is `În derulare`. The icon is
disabled when the menu would be empty.

Not covered, on purpose: an angajament that came from forexecab (not created in KBOT) and is still
`Inițial` / `În definitivare` shows nothing — KBOT does not finish angajamente it did not create.

**Done when.** A Rev 0 manual angajament and a Rev 1 change have gone S1 → S2a → S2b and S1 → S2b respectively on 000_DEMO,
a run interrupted on purpose resumed without a second angajament, and «Definitivează» /
«Derulează» each appear only in their state.

## 0081-05 — G5: captures into the PDF

**What.** `DdfXmlBuilder`, final mode: from the revision's `PrtScr = 1` attachments, write

```
SubformSectiuneaB/Subform51/Table4
  Row1  (empty — the hidden template row, as in Table1 / Table3)
  Row1 > Cell1 xfa:contentType="image/png"  = base64 of the PNG     (one per capture)
```

(template: `Surse/doc_fund_xdp.xml`). Order = the order the workflows took them.
**Decide:** captures in `Table4` only, or also as file attachments. Recommendation: `Table4` only —
the form label says «în rubrica de mai jos **sau** ca anexă», and twice doubles the size.
**To verify in `KBot.Xfa`** (`AdobeUtils.ProcessXmlNodes`): that it writes repeated `Row1` nodes
and keeps the `xfa:contentType` attribute; today it fills by node name. If not, the fix is there.
**Size.** A full-page PNG is a few hundred KB; base64 adds a third. Chunked storage (0078-05) takes
it; to watch on the first real document.
**Done when.** The final PDF opened in Adobe shows the captures under Section B; builder tests
written (the `Table4` shape), not run.

## 0081-06 — G0b: the director's KBOT

**What.** A simplified entry for the Ordonator: on login, «Aveți N documente de fundamentare de
semnat»; a list of revisions in state S3; open one → the existing signing viewer (0078) → sign →
the upload writes `A,B,Ordonator` → S4. Nothing else — no tree, no FOREXE, no editing.
**Where.**
- Login decides the shell from `SessionContext.Role` = `Director`.
- A server GET: revisions with `Incarcat = 1` and `Semnatura` = `A,B` (without `Ordonator`),
  **across every unit the director has access to** (as the accountant's multi-unit access works —
  read how `Contabil` spans units before writing it).
- A small form (`KBotShellForm`), list + open, reusing the signing session.
**Done when.** A director login sees only its waiting list and can sign one document end to end.

---

## Operator answers (25.09.2026)

1. **Slice 0081**, sub-slices -01 … -06.
2. **Role value `Director`** — plain, in Romanian, in `Unitati_Utilizatori.Rol`. Like the
   accountant (`Contabil`), a director works across **several units**, so the waiting list spans
   several databases.
3. **Definitivare / Derulare are NOT part of the send.** Separate actions on the **Rezervări tree
   footer LEFT** icon, by the angajament's state (`Inițial` → «Definitivează», `În definitivare`
   → «Derulează»), next to «Adaugă rezervare». A new angajament starts from a new **«Angajament
   nou»** button on the main header, before «An». Both open `DdfEditForm` (create / add
   reservation). See 0081-04.
4. **No action after the final PDF.** The interim PDF is partial because the captures come later;
   the final PDF is made after all forexecab actions of the revision and signed again from scratch
   (A, B, director). Hence state S2a.
5. **«Definitivează» / «Derulează» only for a NEW angajament.** Later reservation changes, even
   with new indicators, have no such step and go straight to the final PDF.
6. **The Rezervări footer LEFT is one icon with a menu** showing only the valid options.
7. **Variant (a):** a new angajament's Rev 0 gets its final PDF only after «Derulează».
   **One open revision at a time.** «Generează PDF final» is in the Rezervări menu. The menu
   table is in 0081-04.

## Risks

| Risk | Mitigation |
|------|-----------|
| Selectors untested offline | One live run per branch on 000_DEMO before trust (0081-03, -04) |
| A second angajament from a repeated `Creare` | Code saved as soon as it exists; resume uses `Incarca Rezervare` (0081-04) |
| `!` code left behind in a table | List the tables from `MariaDB_Schema/` first (0081-04) |
| XFA writer drops images | Check `ProcessXmlNodes` first (0081-05) |
| Empty `X-Semnatura` not resetting the roles | Check `pdf.py` (G0 note) |
