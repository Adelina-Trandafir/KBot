# SLICE 0081-02 — editor modes, Section B hidden before the send, interim PDF, entry points (G6a)

**Date:** 25.09.2026. **Plan:** `docs/PLAN_DDF_Trimitere.md` §0081-02 (option 2: B is created but not shown).

## What changed and why

**Two new ways into the editor** (both build the draft on the client — there is nothing on the
server to propose from yet; `KBot.Domain/DdfSending.vb`, `DdfDraftFactory`):
- **«Angajament nou»** — a new button on the main header, top left, in the empty column 0 of
  `tlyHeader`, before «An». Port of Access `FX_Adaugare_ANG`: code `"!" + 10 chars`
  (`DdfCodIndicator.Genereaza(10)`), `Stare = "MANUAL"`, `Manual = True`, `Incarcat/Preluat/Buget = False`,
  revision 0, `Tip = "Manual"`, DC = the session's database, Program = the session's program, section A
  empty. The existing save (its Manual branch) writes `FX_Angajamente` / `FX_Indicatori`; after the save
  the tree reloads ONTO the new code (`LoadTreeAsync(codDeSelectat:=…)`).
- **«Adaugă rezervare»** — the Rezervări tree footer LEFT icon. On an angajament with a document: a new
  revision on the same header (read through the editor's `/draft` of the LAST revision; only the header
  is kept), section A empty, `NumarRev = last + 1` (the number lock then holds the real one). Without a
  document: its revision 0 on the existing forexecab angajament (header from the tree node: description,
  creation date, state). Refused when a revision is still open (checked again on fresh data).

**The Rezervări footer LEFT icon** — one icon, one option at most (`RezervariMenu.Decide`, the menu table
of plan 0081-04, pure and tested). Computed after each Rezervări load from the angajament's forexecab
state (`FX_Angajamente.Stare`, read without diacritics/case) and its DDF revisions (one more GET of the
existing DDF read route, only when the angajament has a document). No option → the icon is HIDDEN (the
tree has no disabled look for a footer icon). Click → `CustomPopup` with that one option → the shell.
In this sub-slice the shell handles «Adaugă rezervare»; the other three are wired in 0081-04.

**The editor knows the revision state** (`DdfEditForm(…, stare, sendApi)`):
- «Secțiunea B» nav item hidden by key (`navSub.SetItemVisible`) unless the revision is sent. Section B
  stays derived from A on every edit (unchanged), it is only not shown.
- «Modifică revizia» from the DDF view is refused unless the state is S0 / S1 (D4) — message names the
  state and points to «Adaugă rezervare».
- Saving an **S1** revision asks first (the A signature will go), and after the save calls the new
  route `POST /api/forexe/ddf/trimitere/<idrev>/anuleaza-semnatura`: deletes the stored signed interim
  PDF (`FX_DDF_PDF`) and sets `Semnatura = NULL` → S0. Refused (409) once sent. If it fails the document
  stays saved and the operator is told the revision still reads «semnată A».
- «Șterge revizia» refused for a revision K-BOT sent or began to send (`StareTrimitere <> 0`).

**Interim PDF** — `DdfXmlBuilder.BuildComplete(…, mode)`, `DdfPdfMode.Interim`: `Table3` only the hidden
template row, `CheckBox9 = 0`, NOTAFD `sectiuneaB` with `ckbx_secta_inreg_ctrl_ang = 0` and no rows, no
`PrtScr` attachment. `DdfView` picks Interim for a revision not sent, Final otherwise. The «Vizualizare»
page never rendered Section B (checked), so nothing changes there.

**Sending API kept out of `IApiClient`** — `KBot.Api/IDdfSendApi.vb` + `ApiClient.DdfSend.vb`, the
`IMarcajApi` pattern: nine test fakes implement `IApiClient`, and every one would have had to learn the
new calls. The shell reaches it with `TryCast(_apiClient, IDdfSendApi)`. The whole 0081 set is declared
here (start / codes / capture / stage / director list); the server routes arrive with 0081-04 and -06.

**Read route** — `FX_DDF.Manual` added to the header (`DdfAntet.Manual`): only a K-BOT-created
document's Rev 0 gets «Definitivează» / «Derulează».

## Files touched

- `src/KBot.Domain/DdfSending.vb` (new) — `DdfDraftFactory`, `ForexeAngajamentState`, `RezervariMenuOption`, `RezervariMenu`
- `src/KBot.Domain/DdfInfo.vb` — `DdfAntet.Manual`
- `src/KBot.Api/IDdfSendApi.vb` (new), `src/KBot.Api/ApiClient.DdfSend.vb` (new)
- `src/KBot.Api/UpsertAngajamenteRequest.vb`, `src/KBot.Api/ApiClient.vb` — `manual` on the header
- `src/KBot.App/KbotForm.Designer.vb` — `btnAngajamentNou` (+ tooltip)
- `src/KBot.App/KbotForm.vb` — `BtnAngajamentNou_Click`, `AdaugaRezervareDdfAsync`, `ExecutaMeniulRezervari`,
  `ReincarcaArborelePe`, edit gate in `ModificaDdfAsync`, delete gate in `StergeRevizieDdfAsync`,
  `DeschideEditorulDdf(draft, stare)`, `LoadTreeAsync(…, codDeSelectat)`, Rezervări view wiring
- `src/KBot.App/DDF_EDIT/DdfEditForm.vb` — state, Section B hidden, S1 → S0 on save
- `src/KBot.App/Views/RezervariView.vb` + `.Designer.vb` — footer LEFT icon + menu
- `src/KBot.App/Views/Ddf/DdfXmlBuilder.vb` — `DdfPdfMode`, interim mode
- `src/KBot.App/Views/DdfView.vb` — mode choice at generation
- `PYTHON/routes/forexe/ddf_trimitere.py` (new) — `anuleaza-semnatura`
- `PYTHON/routes/forexe/__init__.py` — registers it
- `PYTHON/routes/forexe/ddf.py` — `manual` on the header
- Tests: `tests/KBot.Domain.Tests/DdfSendingTests.vb` (new), `tests/KBot.App.Tests/DdfXmlBuilderModeTests.vb`
  (new), `PYTHON/tests/test_forexe_ddf.py` (`ANTET_KEYS` + `manual`)

## Test results

`dotnet build src\KBot.App` — **0 errors, 0 warnings**. `py_compile` green. Tests written, **not run**.

## Assumptions (decided here, per the operator's instructions)

1. «Adaugă rezervare» starts with section A **empty** (the classification list brings each line's previous
   value), not a copy of the last revision's lines — a copied line with an unchanged value would be
   refused by the existing rule «Valoarea curentă este 0».
2. `Tip` of every revision K-BOT writes by hand = `"Manual"` (Access's literal in `FX_Adaugare_ANG`).
3. «Adaugă rezervare» on an angajament with NO document opens its revision 0 (the carried-over case, S3
   of the parent document) instead of refusing.
4. «Nothing to offer» hides the footer icon (the tree control has no disabled state for it).
5. The signed interim PDF is DELETED (not kept) when an S1 revision is edited: keeping it would show a
   signed document that no longer matches the data. The shared chunks it used stay in `FX_PDF_BUCATI`.
6. Deleting a revision is refused once K-BOT sent (or began sending) it.

## Left unverified / deferred

- Nothing drawn on screen (header button, footer icon, hidden nav item); nothing run.
- The DDF editor's `/genereaza` still adds its warning «…printr-un flux separat, care nu există încă» for
  a manual angajament — that route belongs to the generation (off limits); the two new entry points do not
  go through it, so the warning is not shown there.
- Whether the tree of the current year/SS shows a new manual angajament right after its save depends on
  its classifications' SS matching the combo — not verified.
