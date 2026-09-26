# SLICE 0081-07 — dry run, replay, and every FOREXE answer kept

**Date:** 26.09.2026. **Asked by the operator (26.09.2026):**
1. There is no FOREXE test server: every run on any database writes to the real forexecab.
2. Once an angajament is created or a reservation is changed, what FOREXE answers MUST be kept in
   `KBOT\Rezultate_Forexe`, in a form that can be loaded back. If K-BOT reads the answer wrong,
   the fix is made in K-BOT and the answer is loaded again, as if the workflow had run, with no
   second run in FOREXE.

A fake FOREXE server was considered and dropped: the robot drives the CABWeb pages by selector,
so a fake site would have to rebuild every page from the robot's own selectors, and would then
confirm exactly the selectors it should be testing.

## What was built

### 1. Every answer kept — `Rezultate_Forexe`
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

### 2. Replay mode — «Mod reîncărcare»
- Setări → FOREXE → «Probă și reîncărcare» → second box. Session only: off at every start.
- On: FOREXE is not opened (the connection answers yes without a browser), and every job asks for
  a file from `Rezultate_Forexe` (the dialog shows only that workflow's answers). K-BOT then goes
  on exactly as if FOREXE had just answered that.
- Refused: an answer of ANOTHER workflow. Asked first (default «Nu»): another angajament code,
  another database, or other parameters (the names of the ones that differ are shown).
- Cancelling the file dialog = the job did not run (same as «nothing started»).

### 3. Dry run — «Mod probă»
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

### 4. Captures are not stored twice (server)
Replaying an answer brings its captures again. `ddf_trimitere.py::_captura` now answers with the
existing attachment when the revision already has a capture with the same bytes (same `Sha256` in
`FX_DDF_REV_ATT_IMG`, or the same base64 on a database without that table), field
`exista_deja: true`.

## Assumptions (details, decided and written down)
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

## Files touched
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

## The order checked against the ministry guide (operator's question, 26.09.2026)
`Surse/GHID UTILIZARE_ALOP_V2.pdf`, Example 3 (p.44-49, Rev 0 of a salary angajament): the
captures go «Crearea angajamentului» (after the final save, B col. 9 = «Credit bugetar rezervat
inițial») → «Starea În definitivare» (p.48) → «Starea În derulare» (p.49). The same order K-BOT
offers: the Rezervari menu gives «Definitivează» only on `Inițial`, «Derulează» only on
`În definitivare`, «Generează PDF final» only on `În derulare` (`RezervariMenu.Decide`). Nothing
changed. (The guide's pages are mostly images; the text was read from the PDF streams.)

## State
Build App (with Common, Forexe) **0 / 0**; `py_compile` green. Nothing run, nothing seen on screen.

**Done when:** (a) a dry run of each send workflow on the real FOREXE stops at its first marked
step with nothing saved; (b) one real send leaves its answers in `Rezultate_Forexe`; (c) the same
send, replayed from those files on 000_DEMO, reaches the same state in K-BOT without FOREXE.
