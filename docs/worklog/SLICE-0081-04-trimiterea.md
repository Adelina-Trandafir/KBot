# SLICE 0081-04 — the send (G6b)

**Date:** 25.09.2026. **Plan:** `docs/PLAN_DDF_Trimitere.md` §0081-04. Only the SEND
(KBOT → forexecab); the import (`prelucrare_pasi.py`, the read workflows) is untouched and is
called as it is.

## The flow, as coded (`src/KBot.App/KbotForm.DdfSend.vb`)

«Trimite în FOREXE» (S1) / «Reia trimiterea în FOREXE» (S1x) in the DDF view menu → `DdfActiune.Trimite`
→ `ExecutaComandaDdf` → `TrimiteDdfAsync`:

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

Rezervări footer menu (`ExecutaMeniulRezervari`): «Definitivează» / «Derulează» run their workflow on the
code with `Motiv(A.2 of Rev 0, 0)`, upload the captures to Rev 0, then download + import (which brings the
new `FX_Angajamente.Stare` and redraws the menu). «Generează PDF final» = step 7 on the open S2a revision.

## Assumptions (details, decided and written down)

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

## Files touched
- NEW `src/KBot.App/KbotForm.DdfSend.vb`.
- `src/KBot.App/KbotForm.vb` — `Case DdfActiune.Trimite`; `Definitiveaza` / `Deruleaza` /
  `GenereazaPdfFinal` in `ExecutaMeniulRezervari`; `DuLaIngestieAsync` → `Task(Of Boolean)`.
- Already written in the earlier part of this sub-slice: `PYTHON/routes/forexe/ddf_trimitere.py`
  (`start`, `coduri`, `captura`, `stare`), `src/KBot.Domain/DdfSendInputs.vb`,
  `src/KBot.Api/IDdfSendApi.vb` + `ApiClient.DdfSend.vb`, `ForexeController.RuleazaTrimitereAsync`,
  `ForexeRunner.FailedWithVariables` (a failed run keeps its variables), `WorkflowCatalog` / `JobBuilder`
  (four builders), `DdfPdfGenerator`, `DdfView` («Trimite» / «Reia trimiterea» entries), `DdfComanda`.
- NEW test `tests/KBot.Domain.Tests/DdfSendInputsTests.vb` (written, NOT run).

## State
Build `KBot.App` **0 errors / 0 warnings**. Nothing run live, nothing seen on screen, no suite run.
**Done when** (operator, 000_DEMO): Rev 0 manual S1 → S2a → (Definitivează, Derulează, PDF final) S2b;
a Rev 1 S1 → S2b; one run interrupted on purpose and resumed without a second angajament.
