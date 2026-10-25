# SLICE-0081-01…13 — Istoricul trimiterii și editorului DDF

Reunire documentară D001-21. Se păstrează cele 12 etape existente (01…10, 12, 13), cu probele și limitele lor. Decizia 01 înlocuiește deducerea stării numai din Incarcat/Semnatura cu StareTrimitere; 07 adaugă excepția modului probă fără scrierea etapei 1. Blocarea antetului din 09 este restrânsă în 10; runda a treia din 12 extinde programul din indicatori și la revizia goală, deși finalul vechi al aceleiași etape încă repeta amânarea. În 13, dialogul și butoanele din 08…12 sunt înlocuite de completarea directă în grilă; clasa dialogului rămâne nefolosită. Aceste repere explică reviziile, fără a transforma probele statice sau presupunerile în confirmări live.

[0081-01](#slice-0081-01) · [0081-02](#slice-0081-02) · [0081-03](#slice-0081-03) · [0081-04](#slice-0081-04) · [0081-05](#slice-0081-05) · [0081-06](#slice-0081-06) · [0081-07](#slice-0081-07) · [0081-08](#slice-0081-08) · [0081-09](#slice-0081-09) · [0081-10](#slice-0081-10) · [0081-12](#slice-0081-12) · [0081-13](#slice-0081-13)

<a id="slice-0081-01"></a>

## SLICE 0081-01 — the revision state (G0a)

**Date:** 25.09.2026. **Plan:** `docs/PLAN_DDF_Trimitere.md` §0081-01. **Handoff:** absorbed into [the plan](../PLAN_DDF_Trimitere.md#handoff-notes-absorbed-in-d001-04) in D001-04; its original assumptions are corrected below.

### What changed and why

The plan said the state S0–S4 (and S1x) would be derived from `FX_DDF_REV.Incarcat` + `Semnatura`, no new column. Reading the code showed that does not hold, and the operator chose a new column (question asked in this thread, 25.09.2026):

1. The import sets `FX_DDF_REV.Incarcat = 1` by itself — `prelucrare_pasi.py`, step 3e, case 1:
   any reservation carrying an `IDREV` (from the `(REV:n)` tag the send writes, D9) marks its
   revision `Incarcat = 1`. So `Incarcat` cannot hold the send stage: the import, which is off limits,
   overwrites it right after the send.
2. «A signed on the final PDF» and «A signed on the interim PDF» both read `Semnatura = 'A'`, so S2a
   and S2b could not be told apart; the Rezervări menu would have offered «Generează PDF final» again
   and wiped the signature.
3. S1x had no trace in either column.

**New column `FX_DDF_REV.StareTrimitere` (tinyint, default 0)** — `sql/0081_ddf_rev_stare_trimitere.sql`: 0 = not sent by K-BOT (and every older revision), 1 = interrupted, 2 = sent / in progress (S2a), 3 = final PDF generated. `Incarcat` keeps its old meaning and is not written by this slice.

**The derivation** (`KBot.Domain/DdfRevisionState.vb`, pure, one place):

| Stage | Extra | State |
|------:|-------|-------|
| 0 | forexecab already has it (`Incarcat` or `Preluat` or a linked `FX_Rezervari` row) | by signature: `Ordonator` → S4, `A`+`B` → S3, else S2b |
| 0 | otherwise | `A` → S1, else S0 |
| 1 | — | S1x |
| 2 | — | S2a |
| 3 | — | by signature, as the first row |

Plus `IsOpen` (S0, S1, S1x, S2a — «one open revision at a time»), `CanEdit` (S0, S1), `CanSend` (S1, S1x), `IsSent` (S2a and after), `Label` (the Romanian text), `HasRole`.

**The check the plan left for this step — settled:** an empty `X-Semnatura` is REFUSED (400, «Antetul X-Semnatura este gol»), not treated as a reset. Added: `X-Semnatura: -` (the same «nothing» marker as `X-Sha-Precedent`) writes `''`. The final PDF (0081-04) is uploaded with it. An empty header stays refused — empty is a client mistake, `-` is a statement.

**Read route** (`routes/forexe/ddf.py`, read-only additions): `stare_trimitere` (the literal 0 when the column is not there yet — probed once per database, `routes/forexe/ddf_stare.py`) and `are_rezervari` (scalar `EXISTS` on `FX_Rezervari.IDREV`, no fan-out).

**DDF view:** each revision leaf gets a RIGHT icon for its state (the left arrow keeps meaning the sign of the total) and a second tooltip line «Stare: …». Icons: `image_list` key `stare_<state>` first (e.g. `stare_signeda`), else a GDI shape per state from `DdfIcons.StateIcon`, coloured from the palette — a different SHAPE per state, so it reads without colour.

### Files touched

- `sql/0081_ddf_rev_stare_trimitere.sql` (new)
- `src/KBot.Domain/DdfRevisionState.vb` (new) — `DdfSendStage`, `DdfRevisionState`, `DdfRevisionStates`
- `src/KBot.Domain/DdfInfo.vb` — `RevizieRow.StareTrimitere`, `.AreRezervari`, `.DejaInForexe`, `.Stare`
- `src/KBot.Api/UpsertAngajamenteRequest.vb` — `GetDdfRevizieRow.stare_trimitere`, `.are_rezervari`
- `src/KBot.Api/ApiClient.vb` — mapping; `ApiClient.SemnaturaNiciuna = "-"`
- `src/KBot.App/DdfIcons.vb` — `StateIcon`
- `src/KBot.App/Views/DdfView.vb` — right icon + tooltip per revision, `StateIconFor`
- `PYTHON/routes/forexe/ddf_stare.py` (new) — stage constants + column probe
- `PYTHON/routes/forexe/ddf.py` — the two read-only fields
- `PYTHON/routes/forexe/pdf.py` — `X-Semnatura: -`
- `tests/KBot.Domain.Tests/DdfRevisionStateTests.vb` (new)
- `PYTHON/tests/test_pdf_semnaturi.py` — `TestParseRolesNoSignature`
- `PYTHON/tests/test_forexe_ddf.py` — `REVIZIE_KEYS` (+ the three `pdf_*` keys of slice 0041, which
  the set-equality assertion was already missing)

### Test results

`dotnet build src\KBot.App` — **0 errors, 0 warnings**. `py_compile` green on the three Python files. Tests written, **not run** (house rule).

### Left unverified / deferred

- `sql/0081_ddf_rev_stare_trimitere.sql` not run anywhere. Until it is, every revision reads stage 0
  and the send routes of 0081-04 refuse with a message naming the script.
- The tree was not drawn on screen (no `DrawToBitmap` pass) — the icons are unseen.
- Assumption: a stage-0 revision with `Incarcat`, `Preluat` or a linked reservation is «already in
  forexecab» and is read as a final document (its PDF, generated the old way, has Section B).

---

<a id="slice-0081-02"></a>

## SLICE 0081-02 — editor modes, Section B hidden before the send, interim PDF, entry points (G6a)

**Date:** 25.09.2026. **Plan:** `docs/PLAN_DDF_Trimitere.md` §0081-02 (option 2: B is created but not shown).

### What changed and why

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

**The Rezervări footer LEFT icon** — one icon, one option at most (`RezervariMenu.Decide`, the menu table of plan 0081-04, pure and tested). Computed after each Rezervări load from the angajament's forexecab state (`FX_Angajamente.Stare`, read without diacritics/case) and its DDF revisions (one more GET of the existing DDF read route, only when the angajament has a document). No option → the icon is HIDDEN (the tree has no disabled look for a footer icon). Click → `CustomPopup` with that one option → the shell. In this sub-slice the shell handles «Adaugă rezervare»; the other three are wired in 0081-04.

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

**Interim PDF** — `DdfXmlBuilder.BuildComplete(…, mode)`, `DdfPdfMode.Interim`: `Table3` only the hidden template row, `CheckBox9 = 0`, NOTAFD `sectiuneaB` with `ckbx_secta_inreg_ctrl_ang = 0` and no rows, no `PrtScr` attachment. `DdfView` picks Interim for a revision not sent, Final otherwise. The «Vizualizare» page never rendered Section B (checked), so nothing changes there.

**Sending API kept out of `IApiClient`** — `KBot.Api/IDdfSendApi.vb` + `ApiClient.DdfSend.vb`, the `IMarcajApi` pattern: nine test fakes implement `IApiClient`, and every one would have had to learn the new calls. The shell reaches it with `TryCast(_apiClient, IDdfSendApi)`. The whole 0081 set is declared here (start / codes / capture / stage / director list); the server routes arrive with 0081-04 and -06.

**Read route** — `FX_DDF.Manual` added to the header (`DdfAntet.Manual`): only a K-BOT-created document's Rev 0 gets «Definitivează» / «Derulează».

### Files touched

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

### Test results

`dotnet build src\KBot.App` — **0 errors, 0 warnings**. `py_compile` green. Tests written, **not run**.

### Assumptions (decided here, per the operator's instructions)

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

### Left unverified / deferred

- Nothing drawn on screen (header button, footer icon, hidden nav item); nothing run.
- The DDF editor's `/genereaza` still adds its warning «…printr-un flux separat, care nu există încă» for
  a manual angajament — that route belongs to the generation (off limits); the two new entry points do not
  go through it, so the warning is not shown there.
- Whether the tree of the current year/SS shows a new manual angajament right after its save depends on
  its classifications' SS matching the combo — not verified.

---

<a id="slice-0081-03"></a>

## SLICE 0081-03 — the workflow edits (G1, G2; G3 dropped)

**Date:** 25.09.2026. **Plan:** `docs/PLAN_DDF_Trimitere.md` §0081-03. Only `.wfl` files in `src/KBot.Forexe/Workflows/Creare/` (never `Surse/`), plus the project entry that copies them.

### What changed and why

**G1 — `Definitivare_Derulare Angajament.wfl` split in two**, because «Definitivează» and «Derulează»
are two separate actions of the Rezervări footer menu. Selectors copied unchanged from the original,
which stays in the folder until both halves have run live.
- `adlop - Definitivare Angajament.wfl` — search, «Modificare», table before, «Definitivează» + reason,
  per row CB inițial → CB definitiv with a capture `Poza_Definitivare_[[idx]]` before «Salvează», then
  `Poza_Definitivare` (the page after the last row), the state label and `TabelIndicatoriFinali`.
  Variables: `COD_ANGAJAMENT`, `MOTIV_DEFINITIVARE`.
- `adlop - Derulare Angajament.wfl` — search, «Modificare», «În derulare» + reason, `Poza_Derulare`
  right after the success message, the state label and `TabelIndicatoriFinali`.
  Variables: `COD_ANGAJAMENT`, `MOTIV_DERULARE`.

**G2 — `Incarca Rezervare.wfl` V4.1:** at the end, when `{{CAPTURA_INFO_COMPLETE}}` is `true`: «Afișează informații complete» → wait for «Informații complete contract» → `Poza_InfoComplete` → «Înapoi». K-BOT sets it `true` for a Rev 0 on an angajament already «În derulare» (0081-04).

**G3** — dropped (plan, 25.09.2026): «Adaugă rezervare» is offered only on angajamente «În derulare».

**Deployment:** the `Creare\` workflows were not copied to the output at all (no project entry; `publish-debug.ps1` copies only the top-level `*.wfl`). Four entries added to `KBot.Forexe.vbproj`; the build now writes `bin\…\Workflows\Creare\` (checked on disk). They flow into KBot.App's output / publish as the other workflows do. The code resolves them as `ResolvePath("Creare\…")` (0081-04).

### Two traps found while reading, recorded for whoever edits these files next

1. **`{{X|text}}` has NO default.** The part after `|` is a description
   (`WorkflowParser.ExtractVariablesDetailed`); `ApplyVariables` replaces only what the job passes, so
   an unpassed variable reaches the page as literal `{{X|…}}`. K-BOT always passes `CAPTURA_INFO_COMPLETE`.
2. **One capture = one variable name.** The executor appends to a per-name list and hands a multi-value
   name back as a JSON array. The original `Definitivare_Derulare` used `saveTo="Poza"` for every row
   (and put it in `collectFields`); the new file names each row's capture and drops `Poza` from
   `collectFields` (an image inside every table row is not wanted).

### Files touched

- `src/KBot.Forexe/Workflows/Creare/adlop - Definitivare Angajament.wfl` (new)
- `src/KBot.Forexe/Workflows/Creare/adlop - Derulare Angajament.wfl` (new)
- `src/KBot.Forexe/Workflows/Creare/adlop - Incarca Rezervare.wfl` — G2
- `src/KBot.Forexe/KBot.Forexe.vbproj` — copy the four sending workflows

### Test results

All six `Creare\*.wfl` parse as XML (Python `xml.etree`; an XML comment with `--` was caught and fixed). `dotnet build src\KBot.Forexe` **0 errors, 0 warnings**. **No workflow was run** — selectors can only be checked live.

### Left unverified / deferred — each branch needs ONE live run on 000_DEMO

- `Definitivare Angajament` on an angajament «Inițial» created by K-BOT.
- `Derulare Angajament` on the same angajament, now «În definitivare».
- `Incarca Rezervare` with `CAPTURA_INFO_COMPLETE = true`: the three new selectors are guesses from the
  guide's screenshots — `a|button:has-text('Afișează informații complete')`,
  `*:has-text('Informații complete contract')`, `button.btn-default|a:has-text('Înapoi')`.
- After both halves ran live: retire `adlop - Definitivare_Derulare Angajament.wfl`.
- After the first publish: check `C:\KBOT\Workflows\Creare\` exists.

---

<a id="slice-0081-04"></a>

## SLICE 0081-04 — the send (G6b)

**Date:** 25.09.2026. **Plan:** `docs/PLAN_DDF_Trimitere.md` §0081-04. Only the SEND (KBOT → forexecab); the import (`prelucrare_pasi.py`, the read workflows) is untouched and is called as it is.

### The flow, as coded (`src/KBot.App/KbotForm.DdfSend.vb`)

«Trimite în FOREXE» (S1) / «Reia trimiterea în FOREXE» (S1x) in the DDF view menu → `DdfActiune.Trimite` → `ExecutaComandaDdf` → `TrimiteDdfAsync`:

1. Fresh read of the revision (the tree may be old), `CanSend` checked again, the SAVED revision read
   with `GetDdfDraftAsync` (never the screen), Yes/No confirmation.
2. FOREXE session opened first (`ConnectAsync`), THEN `POST …/start` (stage 0 → 1). A crash from here on
   leaves S1x, never a revision that looks unsent.
3. Workflow:
   - code starts with `!` → `Creare Angajament` (rows `CreareRows`, CB inițial = A.col6);
   - otherwise → `Incarca Rezervare` (`IncarcaRows`, value = A.col7, reason = A.2 + ` (REV:n)`),
     `CAPTURA_INFO_COMPLETE = true` only for Rev 0 of a NON-manual angajament already «În derulare».
   - Resume (S1x): forexecab is read first (`DownloadRezervariAsync`), row codes it already has are saved,
     and only the rest is sent (`LinesToResume`; for a new angajament: the rows not in the grid at all).
     Nothing left → no workflow, straight to the check.
4. Whatever the run left is written AT ONCE, success or not: the real code (`CodAng_Final`, else the
   last `CodAng_<Cheie>`) + the row codes of the run's `TabelIndicatori` (`POST …/coduri`), and every
   `Poza_*` (`PUT …/captura`). A failed run → message, stays S1x.
5. `DownloadRezervariAsync(code)` → row codes from its `TabelIndicatori` saved → `Differences` against
   section A (new angajament: «CB rezervat inițial» = A.col6; otherwise «CB rezervat definitiv an curent»
   = A.col7). Any difference → message listing them, stays S1x.
6. The existing import (`DuLaIngestieAsync`, unchanged apart from now returning True/False). Not saved
   → stays S1x («Reia trimiterea» re-reads and re-imports; nothing is sent twice).
7. Stage 2. New angajament → message «Definitivează → Derulează → Generează PDF final» (variant a).
   Any other send → final PDF right away (`DdfPdfGenerator`, mode Final, captures in Table4), uploaded
   with `X-Semnatura: -` over the signed interim PDF (sha precedent = the server's), then stage 3 (S2b).
   A failed PDF → message, stays stage 2; the Rezervări menu offers «Generează PDF final».
8. Tree reloaded onto the (possibly new) code, DDF view opened on the revision — its signing session
   is the normal one (slice 0078), so A/B sign the final PDF from there.

Rezervări footer menu (`ExecutaMeniulRezervari`): «Definitivează» / «Derulează» run their workflow on the code with `Motiv(A.2 of Rev 0, 0)`, upload the captures to Rev 0, then download + import (which brings the new `FX_Angajamente.Stare` and redraws the menu). «Generează PDF final» = step 7 on the open S2a revision.

### Assumptions (details, decided and written down)

1. **Code unknown after an interrupted Creare.** If the page shows an angajament code, the operator is asked
   whether it is the one created (Yes = continue on it). If not, the operator is asked whether the
   angajament exists in forexecab: Yes («it does NOT exist») = run Creare; No = stop and resume after
   opening it in «Browser FOREXE». There is no text-input dialog in K-BOT, so reading the code off the
   page is the only way to give it.
2. **`angajamentNou` = `FX_DDF.Manual` AND Rev 0.** `Manual` stays 1 after the `!` swap, so it holds on
   a resume.
3. **Row codes** come from the run's own `TabelIndicatori` (Creare scrapes it) and, for every send, from
   the download's `TabelIndicatori` — Incarca Rezervare scrapes no indicator table.
4. **A resume uploads the captures of the resumed run as new attachments**; those of the first run stay.
   All of them go into Table4 in `IdRevAtt` order.
5. **A capture upload that fails** is logged, counted and reported; the rest continue.
6. **The check happens before the import** (plan step 4 before 6). The codes are written before the check
   (the dangerous-case rule: saved as soon as known).
7. **Definitivare/Derulare** do not change `StareTrimitere` (Rev 0 stays S2a until its final PDF).
8. **The final-PDF upload** uses the unsigned-PDF path of `UploadDdfPdfAsync` with no `semnaturi` rows
   (nobody signed anything).

### Files touched
- NEW `src/KBot.App/KbotForm.DdfSend.vb`.
- `src/KBot.App/KbotForm.vb` — `Case DdfActiune.Trimite`; `Definitiveaza` / `Deruleaza` /
  `GenereazaPdfFinal` in `ExecutaMeniulRezervari`; `DuLaIngestieAsync` → `Task(Of Boolean)`.
- Already written in the earlier part of this sub-slice: `PYTHON/routes/forexe/ddf_trimitere.py`
  (`start`, `coduri`, `captura`, `stare`), `src/KBot.Domain/DdfSendInputs.vb`,
  `src/KBot.Api/IDdfSendApi.vb` + `ApiClient.DdfSend.vb`, `ForexeController.RuleazaTrimitereAsync`,
  `ForexeRunner.FailedWithVariables` (a failed run keeps its variables), `WorkflowCatalog` / `JobBuilder`
  (four builders), `DdfPdfGenerator`, `DdfView` («Trimite» / «Reia trimiterea» entries), `DdfComanda`.
- NEW test `tests/KBot.Domain.Tests/DdfSendInputsTests.vb` (written, NOT run).

### State
Build `KBot.App` **0 errors / 0 warnings**. Nothing run live, nothing seen on screen, no suite run.
**Done when** (operator, 000_DEMO): Rev 0 manual S1 → S2a → (Definitivează, Derulează, PDF final) S2b;
a Rev 1 S1 → S2b; one run interrupted on purpose and resumed without a second angajament.

---

<a id="slice-0081-05"></a>

## SLICE 0081-05 — the FOREXE captures in the final PDF (G5)

**Date:** 25.09.2026. **Plan:** `docs/PLAN_DDF_Trimitere.md` §0081-05.

### What changed

- **`DdfXmlBuilder.Table4Of(capturi)`** — `SubformSectiuneaB/Subform51/Table4`: the hidden template `Row1`
  (empty `Cell1`) first, then one `Row1/Cell1 xfa:contentType="image/png" href=""` per capture, the PNG as
  base64 text. Shape taken from the template data in `Surse/doc_fund_xdp.xml`. No capture → no
  `Subform51` at all (the template keeps its defaults).
- **`BuildComplete(…, mode, capturi)`** — Final mode only. The `PrtScr = 1` attachments never go into the
  PDF's attachment list in either mode (decision in the plan: Table4 only, the form label says «or as an
  annex», and both would double the size).
- **`DdfPdfGenerator`** (written in 0081-04) gathers the captures in `IdRevAtt` order = the order the
  workflows took them: from `DateFisier` when the old column carries them, else from the attachment store
  (`GET /api/forexe/ddf/att/{id}/imagine`). A capture that cannot be read STOPS the generation — a final
  PDF missing a capture would be signed as complete.
- **`KBot.Xfa/AdobeUtils` — a real fix.** `ModifyXfaFromXml` rebuilt table cells by name and value only:
  `xfa:contentType` was dropped, so an image cell would have come out as a wall of base64 text. Cells now
  copy their attributes (`CopyAttributes`, namespace declarations skipped). Repeated `Row1` nodes were
  already written one by one (checked in `ProcessXmlNodes` before changing anything). `CopyAttributes`
  is `Public` so the tests can reach it.

### Assumptions
1. The XFA template's `Table4/Row1/Cell1` is an image field that takes `image/png` base64 data in the
   data DOM (as `doc_fund_xdp.xml` shows). Not proven until a final PDF is opened in Adobe.
2. Size: a full-page PNG is a few hundred KB; several captures make the PDF several MB. Left as the plan
   says — to watch on the first real document.

### Files touched
- `src/KBot.App/Views/Ddf/DdfXmlBuilder.vb` (`Table4Of`, `DdfPdfMode`, mode parameter).
- `src/KBot.App/Views/Ddf/DdfPdfGenerator.vb` (captures gathering).
- `src/KBot.Xfa/AdobeUtils.vb` (`CopyAttributes`, used for every table cell).
- Tests (written, NOT run): `tests/KBot.App.Tests/DdfXmlBuilderModeTests.vb` (4 new: Table4 shape,
  order, placement, none), `tests/KBot.Xfa.Tests/CopyAttributesTests.vb` (new).

### State
Builds 0 / 0 (App, Xfa). **Done when:** a final PDF opened in Adobe shows the captures under Section B.

---

<a id="slice-0081-06"></a>

## SLICE 0081-06 — the director's K-BOT (G0b)

**Date:** 25.09.2026. **Plan:** `docs/PLAN_DDF_Trimitere.md` §0081-06.

### Read first: how a user spans several units

`Contabil` does NOT span units inside one session. `AVACONT_COMUN.Unitati_Utilizatori` has one row per (user, unit, role); `/api/auth/units` lists them, the login picks ONE, and the session (token) is bound to that unit's database (`g.session.db_name`). Every data route reads the session's database. The service account (`get_kbot_connection(dc)`) can open any unit database.

### What was built

- **Server** `PYTHON/routes/forexe/ddf_director.py`, `GET /api/forexe/ddf/director/de-semnat`:
  only for a session whose role is `Director` (else 403). Reads the director's units
  (`Unitati_Utilizatori.Rol = 'Director'`) and, in each database, the revisions that have a PDF on the
  server, are past the send (`StareTrimitere = 3`, or `Incarcat` / `Preluat` for those sent before 0081)
  and carry signatures A and B but not Ordonator. A unit that cannot be read is named in
  `unitati_necitite` and does not stop the list. A database without the 0081 column is still read
  (only `Incarcat` / `Preluat`).
- **`Program`**: after login, `Role = "Director"` → `DirectorForm` instead of `KbotForm`.
- **`DirectorForm`** (`KBotShellForm`, all controls in the Designer except the DDF view, which takes
  the API client in its constructor — same as the shell's views): «Aveți N documente de fundamentare de
  semnat», the list (unit, angajament, object, revision, date, total), and on the right the existing
  `DdfView` on the selected revision. Its signing session (0078) signs and uploads; the upload writes
  `A,B,Ordonator` → S4. The write commands of the DDF menu answer «here the document is only signed».
- **`LoginForm.PreferredDc`**: the unit pre-selected at phase 2 (wins over the remembered one).
- **`IDdfSendApi.GetDdfDeSemnatDirectorAsync`** now returns `DdfDeSemnatLista` (revisions + units not read).

### Assumptions (details, decided and written down)
1. **A document of another unit is opened after a login on that unit** (login window, unit pre-selected,
   password again); the old session is then logged out. No new authentication route — a «switch unit
   without password» route would be a new security surface the plan did not ask for.
2. **Waiting = A and B and no Ordonator**, compared as a comma list with spaces removed; a PDF must exist
   on the server (without one there is nothing to sign).
3. **The list is not refreshed by itself after a signature**: «Reîncarcă lista» does it (the signing
   session lives inside `DdfView`, which does not tell its host).
4. **The DDF view's own menu stays** (the director sees «Modifică», «Șterge»…); each answers with a
   refusal message instead of acting. Hiding them would need a read-only mode in `DdfView`.
5. The list is a plain `ListView` (a standard control, themed by the generic rules), not a new custom
   control.

### Files touched
- NEW `PYTHON/routes/forexe/ddf_director.py`; `PYTHON/routes/forexe/__init__.py` (import).
- NEW `src/KBot.App/Director/DirectorForm.vb` + `DirectorForm.Designer.vb`.
- `src/KBot.App/Program.vb` (role → window, DI), `src/KBot.App/LoginForm.vb` (`PreferredDc`).
- `src/KBot.Api/IDdfSendApi.vb`, `src/KBot.Api/ApiClient.DdfSend.vb` (`DdfDeSemnatLista`).
- NEW test `PYTHON/tests/test_forexe_ddf_director.py` (written, NOT run).

### State
Build App **0 / 0**; `py_compile` green. Nothing run, nothing seen on screen.
**Done when:** a login with a `Director` row sees only its waiting list and signs one document end to end.
The operator must add the `Director` rows to `AVACONT_COMUN.Unitati_Utilizatori` (one per unit).

---

<a id="slice-0081-07"></a>

## SLICE 0081-07 — dry run, replay, and every FOREXE answer kept

**Date:** 26.09.2026. **Asked by the operator (26.09.2026):**
1. There is no FOREXE test server: every run on any database writes to the real forexecab.
2. Once an angajament is created or a reservation is changed, what FOREXE answers MUST be kept in
   `KBOT\Rezultate_Forexe`, in a form that can be loaded back. If K-BOT reads the answer wrong,
   the fix is made in K-BOT and the answer is loaded again, as if the workflow had run, with no
   second run in FOREXE.

A fake FOREXE server was considered and dropped: the robot drives the CABWeb pages by selector, so a fake site would have to rebuild every page from the robot's own selectors, and would then confirm exactly the selectors it should be testing.

### What was built

#### 1. Every answer kept — `Rezultate_Forexe`
- New folder setting `RezultateForexe`, default `Rezultate_Forexe` next to the executable
  (`C:\KBOT\Rezultate_Forexe` on an installed machine). It shows on the «Aplicație» page with the
  other folders and in Setări → FOREXE.
- `ForexeController.RunOrReplayAsync` is now the ONE way a data job reaches the robot (the list,
  the complete download, the partial downloads, the send workflows). After every run, success or
  not, `ForexeAnswerStore.Save` writes
  `<yyyyMMdd_HHmmss_fff>_<Workflow>_<code>.json`: the workflow, the .wfl file, the code, the
  database/year/sector, the parameters K-BOT sent, the verdict (success, message, stopped before
  save) and **all the executor's variables** exactly as the runner returned them, including the
  captures (base64).
- That is enough to rebuild the result: the tables are only those variables parsed, and
  `ForexeAnswerStore.ToJobResult` parses them with the runner's own `ForexeRunner.TryParseTable`.
- Writing the file never fails the run (logged, the console says so).
- `DeschideAngajament` (opens a page, brings no data) and «Conectare» are not recorded.

#### 2. Replay mode — «Mod reîncărcare»
- Setări → FOREXE → «Probă și reîncărcare» → second box. Session only: off at every start.
- On: FOREXE is not opened (the connection answers yes without a browser), and every job asks for
  a file from `Rezultate_Forexe` (the dialog shows only that workflow's answers). K-BOT then goes
  on exactly as if FOREXE had just answered that.
- Refused: an answer of ANOTHER workflow. Asked first (default «Nu»): another angajament code,
  another database, or other parameters (the names of the ones that differ are shown).
- Cancelling the file dialog = the job did not run (same as «nothing started»).

#### 3. Dry run — «Mod probă»
- Setări → FOREXE → first box. Session only. The two modes exclude each other.
- New workflow attribute `commits="true"` on a `<Click>` (parser, model, recorder writer). With the
  dry run on, the executor does NOT click it: it captures the page (variable `Proba_Captura`, not
  `Poza_*`, so it never reaches the revision) and stops like an `<Exit>`, with the message
  «Mod probă: robotul s-a oprit înainte de pasul care salvează («…»). FOREXE nu a salvat nimic.»
  `JobResult.StoppedBeforeSave` says so to K-BOT.
- The send in dry run: stage 1 is NOT written, and on the stop nothing is written on the revision
  (no codes, no captures, no stage). The operator gets «Proba s-a încheiat» and the revision stays
  as it was. Same for «Definitivează» / «Derulează».
- Every send question («Trimit…», «Reiau…», «Definitivez…», «Trec … în derulare») gets a line in
  capitals when one of the two modes is on.

Marked `commits="true"` (only in `Workflows\Creare\`):

| Workflow | Line | Step |
|---|---|---|
| Creare Angajament | 164, 170 | Save final / save intermediate |
| Definitivare Angajament | 105 | «Continuă» in the Definitivare modal |
| Definitivare Angajament | 143, 154 | «Salvează» per indicator, «Continuă» in its modal |
| Derulare Angajament | 74 | «Continuă» in the «În derulare» modal |
| Incarca Rezervare | 138 | Save of a NEW indicator |
| Incarca Rezervare | 196, 209 | «Salvează» of an existing indicator, «Continuă» in the reason modal |

#### 4. Captures are not stored twice (server)
Replaying an answer brings its captures again. `ddf_trimitere.py::_captura` now answers with the
existing attachment when the revision already has a capture with the same bytes (same `Sha256` in
`FX_DDF_REV_ATT_IMG`, or the same base64 on a database without that table), field
`exista_deja: true`.

### Assumptions (details, decided and written down)
1. **«Adaugă angajament» is NOT a save** (operator, 26.09.2026): it only opens the «Adaugă rând»
   form; forexecab saves at «Salvează» (final) or «Salvează și continuă» (intermediate, green
   toast «Rândul angajamentului a fost salvat», but NOT the administration page with the code --
   only the final save leads there). First marked by mistake, unmarked the same day: the dry run
   of Creare now fills the first row (program, source, indicator, amount, limits, capture) and
   stops before its save.
2. **A save opens a reason modal → both the «Salvează» and the «Continuă» are marked.** Stopping
   at the first is the safe side; the modal's selectors are then not tried.
3. **Both modes are session only** and are not saved in AppSettings: a forgotten replay mode would
   make sends look done while FOREXE got nothing.
4. **The dry run does not write stage 1.** A dry run on a real unit therefore leaves the revision
   exactly as it was (S1 stays S1).
5. **Two captures with the same bytes on one revision are the same capture.** Full-page captures of
   different steps do not come out byte-identical in practice.
6. **Replaying needs no state reset in K-BOT.** A send stopped by a wrong reading leaves S1x; with
   the replay mode on, «Reia trimiterea» walks the normal resume path and asks, in order, for the
   answers it needs (the recorded read-back after the send, and the send's own answer if the code
   was not saved). If a wrong reading already WROTE wrong data into K-BOT (a wrong code, for
   example), that is corrected by hand in the database first.
7. **`Definitivare_Derulare Angajament.wfl` and `Rezervare si Receptie.wfl` are not marked**: K-BOT
   does not run them (WorkflowCatalog has no entry for them).

### Files touched
- `src/KBot.Forexe/Models/WorkflowModels.ClickAction.vb` (`Commits`), `Services/WorkflowParser.vb`,
  `Services/WflWriter.vb`, `Executor/WorkflowExecutor.Core.vb` (`StopBeforeCommit`,
  `StoppedBeforeCommit`), `Executor/WorkflowExecutor.Flow.vb`,
  `Executor/Actions/WorkflowExecutor.Actions.Click.vb` (`StopBeforeCommitAsync`),
  `JobModels.vb` (`StopBeforeSave`, `StoppedBeforeSave`), `ForexeRunner.vb`.
- `src/KBot.Forexe/Workflows/Creare/` — `adlop - Creare Angajament.wfl`,
  `adlop - Definitivare Angajament.wfl`, `adlop - Derulare Angajament.wfl`,
  `adlop - Incarca Rezervare.wfl` (only `commits="true"` added).
- `src/KBot.Common/SetariFoldere.vb`, `KBotPaths.vb` (`FolderRezultateForexe`).
- NEW `src/KBot.App/Forexe/ForexeAnswerStore.vb` (`ForexeAnswerStore`, `ForexeAnswer`).
- `src/KBot.App/Forexe/ForexeController.vb` (`DryRunMode`, `ReplayMode`, `RunOrReplayAsync`,
  `AnswerFromFile`; the four downloads and the send go through it).
- `src/KBot.App/KbotForm.DdfSend.vb` (`ModeNote`, `ReportDryRunStop`; no stage 1 in dry run).
- `src/KBot.App/Setari/SetariForexeView.vb` + `.Designer.vb` (section «Probă și reîncărcare», the
  folder row).
- `PYTHON/routes/forexe/ddf_trimitere.py` (`_captura_existenta`).
- NEW tests, written, NOT run: `tests/KBot.App.Tests/ForexeAnswerStoreTests.vb`,
  `PYTHON/tests/test_forexe_ddf_captura_dublura.py`.
- Rule 0 swept in the small touched files (`JobModels.vb`, `WorkflowModels.ClickAction.vb`,
  `SetariForexeView.vb` header). Left: the old Romanian comments of the big touched files
  (`ForexeController.vb`, `ForexeRunner.vb`, `SetariFoldere.vb`, `KBotPaths.vb`, the executor) —
  a sweep of its own.
- FileVersion: Forexe 1.0.15.0, Common 1.5.4.0, App 1.0.42.0.

### The order checked against the ministry guide (operator's question, 26.09.2026)
`Surse/GHID UTILIZARE_ALOP_V2.pdf`, Example 3 (p.44-49, Rev 0 of a salary angajament): the
captures go «Crearea angajamentului» (after the final save, B col. 9 = «Credit bugetar rezervat
inițial») → «Starea În definitivare» (p.48) → «Starea În derulare» (p.49). The same order K-BOT
offers: the Rezervari menu gives «Definitivează» only on `Inițial`, «Derulează» only on
`În definitivare`, «Generează PDF final» only on `În derulare` (`RezervariMenu.Decide`). Nothing
changed. (The guide's pages are mostly images; the text was read from the PDF streams.)

### State
Build App (with Common, Forexe) **0 / 0**; `py_compile` green. Nothing run, nothing seen on screen.

**Done when:** (a) a dry run of each send workflow on the real FOREXE stops at its first marked
step with nothing saved; (b) one real send leaves its answers in `Rezultate_Forexe`; (c) the same
send, replayed from those files on 000_DEMO, reaches the same state in K-BOT without FOREXE.

---

<a id="slice-0081-08"></a>

## SLICE 0081-08 — Section A line window (and the two dead ends of «Adaugă rând»)

**Date:** 26.09.2026. **Request (operator), in the DDF editor (`DdfEditForm`):**
1. For «Adaugă rezervare» (a new reservation made in K-BOT, to be sent to FOREXE) «Adaugă rând» is
   not active.
2. For «Angajament nou» it is active, but double-clicking the Clasificație column does nothing; a
   combo to choose a classification should appear.
3. «Adaugă rând» should open a new window, laid out in the designer, with every value section A
   expects.

### What was wrong

- **(2) — verified in the files.** The column the operator sees is «Clsf» (`clsf`, read-only). The
  combo column «Clasificatie» (`clasificatie`) is `Visible = Hidden` in
  `DdfEditSectiuneaAPage.Designer.vb`, and `Grd_CellDoubleClick` answered ONLY to `clasificatie`. So
  a double click could never reach it.
- **(1) — the cause is NOT verified on data.** `btnAdauga.Enabled` was `disponibile > 0`: off when
  the server list came back empty, and off when the request failed. Neither «Adaugă rezervare»
  factory (`ForAddedReservation`, `ForFirstRevisionOfExisting`) sets `Sursa` or `GrpIdrz`, so the
  reservations lock (`DinRezervari`) is not it. For an angajament whose code no longer starts with
  «!» the list comes only from `FX_Indicatori` of that `CodAngajament` (+ same `Titlu`); an empty
  one there gives exactly this. The empty-list message also lied («toate clasificațiile sunt deja
  folosite» for a list that was simply empty).
- **Found on the way:** `DdfEditForm.AduClasificatiileAsync` took `Titlu` (manual angajament) as
  the THIRD dotted part of `Clsf`. `Capitol` and `Subcapitol` are `NN.NN` themselves (varchar(5),
  checked in `MariaDB_Schema/000_DEMO.sql`), so that is half of Subcapitol: `04` instead of `20`
  for `65.02.04.02.20.01.01`. Now `Substring(12, 2)` = Access's `Mid(Clsf, 13, 2)`.

### What changed

- **New window `DdfEditLinieAForm`** (`DDF_EDIT/`, all controls in the `.Designer.vb`, 144 dpi,
  `AutoScaleMode.Dpi`, `KBotThemedForm`): Clasificație (`KBotComboBox` with `Editable`,
  `FindAsYouType`, `FindAfterNChars = 2`, `InputMask = 00.00.00.00.00.00.00` — slice 0082), Denumire,
  Element fundamentare *, Parametrii, Partener (only when the document has a partner), Valoare
  curentă *; read-only: valoare precedentă, recepții, totală (live), cod indicator. Works on a COPY
  of the line; OK writes it back, «Renunță» leaves the draft untouched. The refusals are section A's
  own (separator never offered; empty element; value 0; negative value below receptions), all in one
  message. Picking a classification fills the element with its name unless the operator wrote
  their own.
- **`DdfEditSectiuneaAPage`:** «Adaugă rând» follows the reservations lock only. It opens the window
  on a new line — the line joins the draft only on OK (before, an empty line was added first and left
  behind on cancel). A double click on «Clsf» (or the hidden «Clasificatie») opens it on that line.
  The list is fetched again when the last fetch failed or was empty, and every time for a manual
  angajament (its `Titlu` restriction follows the first line). When there is nothing to choose, a
  message says which of the two cases it is. The status line now tells «server sent nothing» from
  «all used». The in-grid combo code (fill, validate, apply) is gone — its column is hidden.
- **`DdfEditForm.AduClasificatiileAsync`:** the `Titlu` fix above.

### Files touched

- `src/KBot.App/DDF_EDIT/DdfEditLinieAForm.vb` (new), `DdfEditLinieAForm.Designer.vb` (new)
- `src/KBot.App/DDF_EDIT/DdfEditSectiuneaAPage.vb`
- `src/KBot.App/DDF_EDIT/DdfEditForm.vb`
- (depends on slice 0082: `src/KBot.Controls/Combo/*`)

### Test results

- `dotnet build src\KBot.App` — **0 errors, 0 warnings**, built into a scratch folder: KBot.App was
  running (PID 15924) and Visual Studio held the normal `bin\Debug` copies.
- The window rendered off-screen with `DrawToBitmap` on sample classifications: layout checked,
  typing `650204` → `65.02.04` + list of 3 matches, Enter chose the first and filled Denumire,
  Element, previous value, receptions, total, indicator code. Height trimmed after the first render.
- No suite run.

### Left unverified / deferred

- **The real reason (1) was off on the operator's angajament** — the window now names it; read what
  it says the first time. If «Serverul nu a trimis nicio clasificație…», look at `FX_Indicatori` for
  that `CodAngajament`.
- Not opened in the VS designer; not run against the server; the grid double-click not tried by hand.
- The designer's hidden «Clasificatie» column is still declared as `Combo` (never fed). Left as it is
  to keep the designer file untouched.

---

<a id="slice-0081-09"></a>

## SLICE 0081-09 — Locked header, the source / sector in section A, the partner only in the header

**Date:** 26.09.2026. **Request (operator), in the DDF editor (`DdfEditForm`):**
1. A new revision on an existing angajament: the header (Compartiment, Program, ...) MUST be
   locked -- it comes from the initial angajament.
2. Adding a row: the classifications offered must follow the source (SS).
3. A new angajament: a new option to choose the source / sector first, then the classification;
   the SS list follows the header's program. «Adaugă rând» stays off until the header is complete.
4. The Partener field goes out of the row window: it is per document, the same on every row, so it
   lives in the header.

**Correction (operator, same day), which replaced the first version of 2 and 3:**
- The Sursa / Sector field in the row window is NOT locked. It is tied to the header's CodProgram
  through `AVACONT_COMUN.DefaProgram`: 0000000000 -> 02A / 02E, 0000002510 -> 01A.
- The same applies to a new revision: one angajament may have lines on several SSs, as long as they
  belong to its program.
- The first version locked an existing angajament to K-BOT's selected SS and read the SSs from
  `Unitati_Ani`. Both are gone.

### What was wrong

- **(1)** `AplicaEnablement` opened the header whenever the DOCUMENT was new (Access's `DDF_NOU`).
  «Adaugă rezervare» on an angajament with no K-BOT document (`ForFirstRevisionOfExisting`) and the
  generated first revision are new documents on an angajament that already exists, so their header
  was typeable.

### What changed

- **Server** (`PYTHON/routes/forexe/ddf_edit.py`): new `GET /api/forexe/ddf/surse-program`.
  - It reads `DefaProgram` LEFT JOIN `DefaSursaSector` from AVACONT_COMUN
    (`get_kbot_comun_connection`) and returns `{surse: [{program, ss, denumire}]}`.
  - SS = `CONCAT(Sursa, Sectorul)`, the same code as `DefaSursaSector.SursaSector` and `Clasificatii.SS`.
- **Api:** `IDdfProgramApi` (new; kept out of `IApiClient` like `IDdfSendApi`, so the nine test fakes
  are untouched) and `ApiClient.DdfProgram.vb`.
- **Domain:**
  - `DdfDraft.AngajamentNou`, set only by `ForNewAngajament`.
  - `DdfSursaProgram`.
  - `DdfSectiuneaAReguli` (pure): `AcelasiProgram` (numeric compare), `AcelasiSs`,
    `SurseAleProgramului`, `ClasificatiileSursei`, `ClasificatiileProgramului`, `LipsuriAntet`,
    `LiniiCuAltaSursa`.
- **`DdfEditForm`**
  - The header is locked when the angajament exists, i.e. a new revision on it, or a change to a
    revision of an angajament forexecab has (code without «!»). Locked: CUAL, creation date, object,
    program, compartment, the partner flag and the partner.
  - **Exception:** the compartment and the object stay open while EMPTY. `FX_Angajamente` has no
    compartment.
  - It fetches the program -> SS map when it opens (`AduSurseleProgramelorAsync`, with an optional
    re-login net in `DdfEditReauth`) and hands it to section A on demand.
  - Every header edit pushes «what is missing» to section A.
  - The save refuses a new angajament's line whose SS is not one of the program's SSs.
- **`DdfEditSectiuneaAPage`**
  - It offers only the classifications on one of the document program's SSs. A line's own
    classification stays offered.
  - The messages tell three cases apart: server sent nothing / nothing on the program's SSs /
    everything already used.
  - A new angajament: «Adaugă rând» is off until the header has a program, compartiment, object
    (+ partner when ticked).
  - The line window gets the program's SSs; K-BOT's selected SS is only proposed.
- **`DdfEditLinieAForm`** (+ designer)
  - New first row «Sursă / sector» (`cmbSursa`, captions «02A — denumire»). It is always enabled
    when there is at least one SS. It preselects the line's own SS, then K-BOT's SS if it belongs to
    the program, then the only SS.
  - The classification combo holds only the chosen SS's classifications and is off until an SS is
    chosen. OK refuses a missing SS.
  - The Partener row is gone. The line keeps the partner the page puts on it from the header.

### Files touched

- `PYTHON/routes/forexe/ddf_edit.py`
- `src/KBot.Api/IDdfProgramApi.vb` (new), `ApiClient.DdfProgram.vb` (new)
- `src/KBot.Domain/DdfSectiuneaAReguli.vb` (new), `DdfDraft.vb`, `DdfSending.vb`
- `src/KBot.App/DDF_EDIT/DdfEditForm.vb`, `DdfEditSectiuneaAPage.vb`, `DdfEditLinieAForm.vb`,
  `DdfEditLinieAForm.Designer.vb`, `DdfEditReauth.vb`
- `src/KBot.App/KbotForm.vb`
- `tests/KBot.Domain.Tests/DdfSectiuneaAReguliTests.vb` (new)

### Test results

- `dotnet build src\KBot.App` (into a scratch folder): **0 errors, 0 warnings**.
- The row window was rendered off-screen with DrawToBitmap.
  - Program 0000000000: SS enabled with 02A / 02E, 02A proposed, 2 classifications; 02E gives 1.
  - Program 0000002510: SS enabled with only 01A (K-BOT's 02A not proposed: not in the program),
    1 classification.
- The VB Domain tests are written and **not run**. The Flask route has no test and was **not run**.

### Left unverified / deferred

- The route must be deployed to the VPS. Until then, «Adaugă rând» says the sources could not be
  fetched.
- The editor's header lock was not rendered: it needs the API to open.
- The compartment/object exception (open while empty) needs the operator's word.

---

<a id="slice-0081-10"></a>

## SLICE 0081-10 — Classification without «.02», the partner list, the header of revision 0

**Date:** 26.09.2026. **Request (operator), in the DDF editor:**
1. In the classification combo «65.02» must be just «65». The «02» after the chapter is an Access
   relic: not shown, and not filled into the sending workflows. Change only the row window and the
   workflow inputs.
2. The partners in `DdfEditForm`:
   1. «Angajament nou» + tick «Partener asociat»: the list is empty.
   2. A new document on an existing angajament: the «Partener asociat» box is disabled.
   3. A NEW document (revision 0) on an angajament DOWNLOADED from forexecab: everything but the
      compartment is disabled. It worked before 0081-09.

### What was wrong

- **(1)** `Clasificatii.Clsf` is `concat_ws('.', Capitol, Subcapitol, Articol, Alineat)`, and
  `Capitol` is Access's «65.02». The send passed it through: `Clasificatia` = «65.02.04.02.20.01.01»
  and `ClsfSal` = «65020402200101», while the workflows expect «65.04.02.20.01.01» /
  «650402200101» (their own headers say so; the table's `ClsfSal` column already drops it).
- **(2.2, 2.3)** 0081-09 locked the header whenever `RevizieNoua` was set on an angajament not
  created now. `ForFirstRevisionOfExisting` (revision 0 of a downloaded angajament) is a NEW
  document with `RevizieNoua = True`, so it was locked too; only the compartment escaped, through
  the "open while empty" exception.
- **(2.1)** `/api/forexe/ddf/parteneri` scoped partners to the units of the angajament's
  `FX_Indicatori` rows. A new angajament (code «!…») has none yet, so nothing matched.

### What changed

- **Domain:** `DdfSendInputs.ForexeClsf` drops the second dotted part only when the text has
  exactly seven parts (the Access shape); anything else comes back trimmed, unchanged.
  `DdfSendLine.FromDraft` uses it, so `Clasificatia`, `ClsfSal` and the difference messages all get
  forexecab's form. Stored data (`FX_DDF_SA.Clsf`) is untouched.
- **`DdfEditLinieAForm`:** the combo items use `ForexeClsf`; the mask is `00.00.00.00.00.00`.
- **`DdfEditForm.AplicaEnablement`:** the lock is back to "a new revision on a document that
  already exists" (`RevizieNoua AndAlso Not Nou`) -- which Access's rule already locked. Revision 0
  of a downloaded angajament and the change of an existing revision follow Access's rule again.
  The "open while empty" exception for compartment / object stays.
- **Server** (`ddf_edit.py`, `_SQL_PARTENERI`): when the angajament has no indicator with a unit,
  every unit of the database is offered (the database is one DC).

### Files touched

- `src/KBot.Domain/DdfSendInputs.vb`
- `src/KBot.App/DDF_EDIT/DdfEditLinieAForm.vb`, `DdfEditLinieAForm.Designer.vb`, `DdfEditForm.vb`
- `PYTHON/routes/forexe/ddf_edit.py`
- `tests/KBot.Domain.Tests/DdfSendInputsTests.vb`

### Test results

- `dotnet build src\KBot.App` (scratch folder): **0 errors, 0 warnings**. `ddf_edit.py` compiles.
- The row window rendered off-screen: items read «65.04.02.20.01.30 — …».
- VB tests written, **not run**. The partner SQL change was **not run** against a database.

### Left unverified / deferred

- The section-A grid (column «Clsf») still shows the stored «65.02…» -- the request limited the
  change to the row window and the workflows.
- `ddf_edit.py` (this change and 0081-09's route) must be deployed to the VPS.
- The header enablement was not seen on screen: the editor needs the API to open.

---

<a id="slice-0081-12"></a>

## SLICE 0081-12 — New revision from the existing indicators, used classifications first, zero lines dropped at save

**Date:** 26.09.2026. **Request (operator), DDF editor:**
1. «Adauga rezervare» from the + in the Rezervari tree footer opens a popup with
   «1. Adaugă revizie goală» and «2. Folosește indicatorii existenți». With 2, the section-A grid
   of the editor starts filled with every indicator code the selected angajament already has.
2. In `DdfEditLinieAForm`, the classification list -- the real dropdown AND find-as-you-type --
   shows the classifications already in the angajament first, then the unused ones.
3. At save, section-A lines with value 0 are dropped, after a confirmation message.
4. If no line with a non-zero value is left, the document is NOT saved.

### What changed

- **Domain (`DdfSending.vb`):** `RezervariMenuOption.AdaugaRezervareCuIndicatori` (= 5, never
  returned by `Decide`) and `RezervariMenu.Intrari(opt)` -- the popup entries for an option:
  «Adauga rezervare» gives the two numbered entries, every other option its own single entry.
- **Domain (`DdfSectiuneaAReguli.vb`):**
  - `EsteInAngajament(c, manual)`: a classification is "in the angajament" when it has an
    indicator code (from `FX_DDF_REV_SA`, i.e. an earlier revision) or -- for a non-manual
    angajament -- when the server put it in group `SortOrd = 1` (on `FX_Indicatori`). The manual
    list marks every row `SortOrd = 1`, so there only the indicator code counts.
  - `InAngajamentIntai`: that group first, then the rest, each in the order it came.
  - `LiniiDinIndicatori`: one section-A line per such classification, value 0, element of
    fundamentation = the classification's name, only on the program's SSs (nothing filtered while
    the `DefaProgram` map is unknown), each classification once, none already in section A;
    indicator code = the angajament's, or a freshly minted one when it has none (the line window's
    rule); header partner copied onto the line.
  - `LiniiCuValoareZero`: the lines whose current value rounds to 0.
- **`KBotComboBox`:** runtime-only `FindFirstGroupCount` (Browsable False, not serialized) and a
  `FindMatches(captions, typed, firstGroupCount)` overload: matches in the top group (start, then
  contain) come before the matches in the rest. The old two-argument `FindMatches` is unchanged
  in behaviour (group of 0).
- **`RezervariView`:** the footer popup is built from `RezervariMenu.Intrari`; the clicked item's
  key is parsed back to the option.
- **`KbotForm`:** `ExecutaMeniulRezervari` handles the new option; `AdaugaRezervareDdfAsync` and
  `DeschideEditorulDdf` carry `cuIndicatori` to `DdfEditForm.PornesteCuIndicatorii`.
- **`DdfEditForm`:**
  - `PornesteCuIndicatorii`: on Shown, after the lists are fetched, an EMPTY section A is filled
    through `LiniiDinIndicatori` (classifications fetched by the form's own `AduClasificatiileAsync`).
    The notice says how many lines were added, or that there were none / the fetch failed.
  - Save: if every section-A line is 0 -> message, nothing saved. Otherwise the zero lines are
    listed in the save question («... se șterg la salvare»); only on Yes are they removed (section B
    rebuilt, section-A page refreshed). If the save fails (API or other exception), they are put
    back at their places. `MotiveDeRefuz` skips the lines about to be dropped (their section-B twins
    too) but keeps counting them, so row numbers match the grid.
- **`DdfEditLinieAForm`:** takes `manual`; orders its classifications with `InAngajamentIntai` and
  tells the combo where the group ends (`FindFirstGroupCount`), recomputed every time the SS filter
  refills the list.
- **`DdfEditSectiuneaAPage`:** passes `_draft.Manual` to the line window.

### Correction the same day: the program of the prefilled revision

The operator got «Angajamentul nu are indicatori de preluat» on an angajament that has them. `ForAddedReservation` copies the program of the LAST REVISION (`FX_DDF.Program`), which can be wrong; the SS filter of the prefill then removed every classification. Operator's rule: the program is `FX_Indicatori.SS` JOIN `AVACONT_COMUN.DefaProgram` (`CONCAT(Sursa, Sectorul)`) -- the source of truth, nothing else. («1. Adaugă revizie goală» and the new DDF keep the current logic.)

- `DdfSectiuneaAReguli.ProgrameleIndicatorilor(surse, defaProgram)`: the distinct programs of the
  angajament's SSs.
- `KbotForm` hands the tree node's `Surse` (the server's
  `GROUP_CONCAT(DISTINCT FX_Indicatori.SS)`) to `DdfEditForm.SurseIndicatori`.
- `PrecompleteazaIndicatoriiAsync` maps them through `DefaProgram` (the editor's
  `/surse-program` fetch) BEFORE the lines are built: exactly one program -> it replaces the
  header's program (header redrawn); none or several -> nothing is prefilled and the notice names
  the SSs / programs.
- The «no indicators» notice now says how many classifications the server sent, how many are the
  angajament's, and how many are on the program's SSs -- so the next empty case names its filter.
- No server change: both pieces (`Surse`, `DefaProgram`) already reach the client.

### Second request the same day: program on the «+» path, budget, the value grids

**Request (operator):** (a) the same program rule when a revision is added from the Rezervari tree's row «+» (`/genereaza`); (b) the budget of a new revision NOT based on a reservation comes from `FX_Indicatori.Credit_Bugetar`; (c) in `DdfEditLinieAForm` every value in a designer-built grid, 90 wide, Standard format, no footer, only the current value editable, columns Buget, Val. receptii, Disponibil (= Buget - Receptii), Val. precedenta, Val. curenta, Val. ramasa; (d) the same columns in the section-A grid of `DdfEditForm`. Operator's answers to two questions: **Val. ramasa = Disponibil - ValCur**; **ValPrec stays** (before ValCur).

- **(a)** `DdfEditForm.ProgramDinIndicatori` (split out of the prefill as
  `AplicaProgramulIndicatorilorAsync`): on Shown, the header's program becomes the one
  `FX_Indicatori.SS` -> `DefaProgram` gives. `KbotForm.AdaugaDdfAsync` (the row «+», both the
  initial and the later revision) sets it with `_currentInfo.Surse`. The prefill (option 2) runs
  only when that step found exactly one program. Option 1 («revizie goala») unchanged.
- **(b)** Server `/api/forexe/ddf/clasificatii`: each row carries
  `buget = SUM(FX_Indicatori.Credit_Bugetar)` for the angajament + classification (a fourth `cod`
  parameter in the three queries). `DdfClasificatie.Buget`; the line window
  (`ScrieInLinie`) and the prefill (`LiniiDinIndicatori`) put it on the line. Lines from
  `/genereaza` keep the server's value (`R_CreditBug` / `Credit_Bugetar`).
- **(c)** `DdfEditLinieAForm.Designer.vb`: `txtValCur` and the ValPrec / ValRec / ValTot labels
  removed; `grdValori` (one row, six columns above) declared in the designer. The value is typed in
  the cell; `CellValidating` refuses a non-number; OK first commits a pending edit
  (`KBotDataView.CommitPendingEdit` -- new, because Enter on AcceptButton never reaches the grid's
  editor). An existing line opens with the editor on the value (`KBotDataView.EditCell`, new).
- **(d)** `DdfEditSectiuneaAPage.Designer.vb`: value columns replaced by the same six (90 wide,
  Standard); `val_tot` column and the footer (`FooterVisible`, Sum aggregates) removed. `ValTot`
  stays on the line and is saved.
- The old message «Valoarea rămasă nu poate fi mai mică decât valoarea recepțiilor» (Access's
  ValPrec + ValCur) now reads «Valoarea totală (precedentă + curentă) ...», so it no longer names
  the new, different «Val. rămasă».
- Domain: `DdfSectiuneaAReguli.Disponibil` / `ValoareRamasa`.

### Third round the same day (operator: "still wrong")

1. **Program of the empty revision** («1. Adaugă revizie goală», not based on a reservation, no
   indicators prefilled): now also `FX_Indicatori.SS` -> `DefaProgram` --
   `KbotForm.AdaugaRezervareDdfAsync` passes `programDinIndicatori:=True` for both entries. This
   replaces the earlier "keep the current logic" for option 1.
2. **Values after a classification is picked in the line window:** checked off-screen (scratch
   program, fake list): picking from the list, typing a code + leaving the field
   (`CommitText`) and choosing a find-list row all rewrite the six cells from the picked
   classification (Buget 1000 / Rec 100 -> Disp 900; then Buget 2000 / Rec 300 -> Disp 1700).
   No client change. The numbers are the server's: without the deployed `ddf_edit.py`, Buget is
   0 for every classification.
3. **Classifications already added are not offered:** `DdfSectiuneaAReguli.EsteFolosita` --
   same `IDClsf`, OR same code (forexecab spelling, digits only) on the same SS (a line with no SS
   matches on the code). Used by the page's `ClasificatiileLibere` (what the line window's
   dropdown and find list hold) and by `LiniiDinIndicatori`. Checked off-screen: line on id 1,
   list {1, twin 5 with the same code, 2} -> a new line is offered {2}; the line itself {1, 5, 2}.

### Files touched

- `src/KBot.Domain/DdfSending.vb`, `src/KBot.Domain/DdfSectiuneaAReguli.vb`, `src/KBot.Domain/DdfDraft.vb`
- `src/KBot.Api/DdfEditContract.vb`, `src/KBot.Api/ApiClient.vb`
- `src/KBot.Controls/Combo/KBotComboBox.vb`, `src/KBot.Controls/DataView/KBotDataView.Editing.vb`
- `src/KBot.App/DDF_EDIT/DdfEditLinieAForm.Designer.vb`, `DdfEditSectiuneaAPage.Designer.vb`
- `PYTHON/routes/forexe/ddf_edit.py`
- `src/KBot.App/Views/RezervariView.vb`, `src/KBot.App/KbotForm.vb`
- `src/KBot.App/DDF_EDIT/DdfEditForm.vb`, `DdfEditLinieAForm.vb`, `DdfEditSectiuneaAPage.vb`

### Test results

- `dotnet build src\KBot.App\KBot.App.vbproj -c Debug`: **0 errors, 0 warnings**.
- No tests written, run or built (operator's rule, 26.09.2026: no test code unless asked).
- `ddf_edit.py` compiles (`py_compile`, the venv).
- Rendered off-screen (DrawToBitmap, scratch program): the line window with Buget 10.000,
  Receptii 200, ValPrec 300, ValCur 1.500 -> Disponibil 9.800,00, Val. ramasa 8.300,00; the
  section-A page with two lines -- six value columns, no footer.
- Nothing run live.

### Left unverified / deferred

- **Assumption:** "indicator codes existing in the angajament" = the classifications of the
  existing `/api/forexe/ddf/clasificatii` list that pass `EsteInAngajament` (see above). The code
  comes from `FX_DDF_REV_SA`, exactly as Access's `cmbClsf_AfterUpdate`; a classification that is on
  `FX_Indicatori` but never had a K-BOT line gets a freshly minted «!xxx» code (same as choosing it
  by hand today). The server was not changed.
- **Assumption:** the prefilled line's element of fundamentation is the classification's name, not
  the text of an earlier revision's line.
- **`ddf_edit.py` must be deployed to the VPS**: until then the classification list has no
  `buget` and manual lines show Buget 0 (Disponibil = -receptions).
- A revision reopened for a change (`/draft/{iddf}/{idrev}`) still reads Buget 0 (the server
  sends 0; no column stores it) -- only NEW revisions were asked for.
- Nothing refuses a Val. ramasa below 0 (not asked).
- At 90 px the header «Val. precedentă» is cut to «Val. precede...».
- The SSs come from the tree node, so they are as fresh as the last tree load (minutes old at
  most); a fresh server read would need a new route and a VPS deploy.
- Only the prefilled revision takes the program from the indicators; the empty revision still
  copies the last revision's `FX_DDF.Program` (operator: keep the current logic there).
- The popup, the prefilled grid, the ordering in the combo and the save question were not seen on
  screen.

---

<a id="slice-0081-13"></a>

## SLICE 0081-13 — Section A entered straight in the grid; no new angajament while reservations lack a DDF

**Date:** 23.10.2026. **Request (operator), «+» / «Angajament nou» and the DDF editor:**
1. If there are downloaded angajamente WITHOUT a DDF but WITH reservations, a new angajament cannot be created.
2. In the «Adauga DDF» editor the grid always has an empty row, not via «Adauga rand».
3. With «Adauga rand», after picking a classification the budget and the other values did not update in the grid — they must update straight in the editor's grid.
4. The «Clasificatie» cell is a combo tree with ONLY the classifications of the selected «Program».
5. «Sterge rand» is a button in the grid, like everywhere else.

### What changed

- **`KbotForm.Ddf.vb`:** `DeschideAngajamentNou` first calls the new `AngajamenteCuRezervariFaraDdf()`: the
  rows of the loaded tree (`_treeRows`) with `AreRezervari` and not `AreDDF`, hidden ones left out. Any
  found → a message naming up to 8 codes («Adaugati intai DDF-ul...») and the editor does not open.
- **`DdfEditSectiuneaAPage` (+ Designer):**
  - The buttons `btnAdauga` / `btnSterge` are gone; `tlyButoane` keeps only the status label. The
    line window `DdfEditLinieAForm` is no longer opened from here (the class stays in the project, unused).
  - **Permanent empty row** (`UmpleGrila` appends a row with `Tag = Nothing` unless the grid is locked).
    `Grd_CellFormatting` makes every cell of it read-only except «Clasificatie» (read-only too while a
    new angajament's header is incomplete), blanks and disables its delete button, and shows a hint.
  - **«Clasificatie» column** is visible again: a combo with `CommitOnSelect`, fed by
    `ActualizeazaListaClasificatii` with `KBotComboGroupedItem`s — only `ClasificatiileLibere` (the
    header program's SSs, minus the classifications other lines use), the ones already in the angajament
    first, the session's SS first; one folding header per «Sursa XX — din angajament / alte clasificatii».
    The list is rebuilt whenever the status line is (`ActualizeazaStarea`) and on every header edit
    (`SeteazaLipsurileAntetului`), so a program change reaches it.
  - **Picking** (`SchimbaClasificatia` / `AplicaClasificatia`, ported from `DdfEditLinieAForm.ScrieInLinie`):
    on the empty row it creates the line (joins `_draft.LiniiA`, a new empty row appears, the cursor goes
    to «Val. curenta»); on a line it changes the classification. Budget, receptions, available, previous
    value and remaining are written into the row at once. Typed text must name an item of the list
    (`ValideazaClasificatia`), else the cell is refused.
  - **«sterge» button column** (`✕`, like ClasificatiiForm / ParteneriForm): `Grd_ButtonClick` asks
    «Stergi randul?», removes the line and the grid row, raises `DraftModificat` (section B is rebuilt by
    the form). Hidden on a grid locked because the document comes from reservations.
  - A MANUAL angajament's list depends on its first line, so every add / change / delete refetches it
    (`ReincarcaListaManualaAsync`).
  - «Clsf» column is hidden (the code is inside «Clasificatie»); the cell label is now
    `code — denumire` with the code written as forexecab writes it.
- **Help (0000-NN, slice tag 0081-13):** `contabil/ddf/editor.md` (section «Sectiunea A» rewritten, capture
  `ddf-rand-nou` removed, `ddf-editor` capture marked for redo), `contabil/ddf/nou.md` (the block + redo
  mark), `tours/tur-ddf-editor.md`.

### Files touched

`src/KBot.App/KbotForm.Ddf.vb`, `src/KBot.App/DDF_EDIT/DdfEditSectiuneaAPage.vb`, `src/KBot.App/DDF_EDIT/DdfEditSectiuneaAPage.Designer.vb`, `src/KBot.App/DDF_EDIT/DdfEditForm.vb` (two comments), `src/KBot.App/HelpContent/contabil/ddf/editor.md`, `.../nou.md`, `.../tours/tur-ddf-editor.md`.

### Test results

- `dotnet build src\KBot.App\KBot.App.vbproj`: **0 warnings, 0 errors**.
- `Check-Help.ps1`: no message about the three help files touched (the run exits 1 on messages about
  `tutorials/ordonantare-din-plata.md`, which this work did not touch).
- No test written or run (house rule); **not seen on screen**.

### Unverified / deferred

- Not run in the app: the grouped combo inside a grid cell (first use of `KBotComboGroupedItem` in
  `KBotDataView`), `CommitOnSelect` with those items, typed find in the cell, the cursor jump to
  «Val. curenta» after a pick, the placeholder text of the empty row, the dead button on it.
- «Din angajament» / «alte clasificatii» are one level of folding (the combo has no deeper tree); a real
  tree by chapter would need a new control — say so if wanted.
- Item 1 reads the tree that is loaded (current year / SS selection, hidden angajamente off). An
  angajament outside that selection is not seen. A server-side check would need a new route.
- `DdfEditLinieAForm` is now dead code, kept until the operator says to delete it.
- `NOUTATI.md` not written (release notes are written at release time).

## Repere și întrebări unice din registrul 0081

Planul de trimitere este [PLAN_DDF_Trimitere](../PLAN_DDF_Trimitere.md), cu fundamentul [DocumentFundamentare_CAB](../FUNDAMENT_DocumentFundamentare_CAB.md). Rândul principal al registrului rezuma încă 01…10, deși 12/13 erau adăugate: stările detaliate de etapă rămân autoritatea acestui istoric. Scriptul pentru StareTrimitere era consemnat de rulat pe fiecare bază; consolidarea nu execută și nu autorizează SQL.

Întrebările vechi neînchise sunt păstrate cu reperul lor: 12 — trebuie luat codul din `FX_Indicatori.CodIndicator`, în locul regulii istorice `FX_DDF_REV_SA`/cod nou «!xxx»? Elementul precompletat = denumirea clasificației, nu textul reviziei vechi, rămânea de confirmat. 09 — compartiment/obiect deschise cât sunt goale la revizia nouă și sursa principală doar preselectată dacă aparține programului rămâneau de confirmat (cu excepția reviziei 0 din 10). 08/0082 — punctul 3 al cererii pentru 0082 a venit gol; masca numai cifre nu permite căutarea după denumire în combo-ul ferestrei, lista integrală rămânând la săgeată. Această limită privește fereastra istorică, înlocuită în 13; nu se deduce de aici comportamentul noii celule grupate. Cauza reală a butonului inițial inactiv rămânea neconfirmată pe date (listă goală sau cerere eșuată).
