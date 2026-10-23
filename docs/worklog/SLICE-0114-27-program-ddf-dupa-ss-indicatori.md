# SLICE-0114-27 - The DDF program follows the SS of the indicators; repair for documents saved wrong; third red in the main tree (23.10.2026)

## What changed and why
Operator, 23.10.2026: revision 0 of an angajament that comes from FOREXE got its program from the session (`_session.CodProgram`, or the fixed `0000000000` on the `/genereaza` path). The rule is: **SS = 01A -> `0000002510`, otherwise `0000000000`**. Every operator works with «Ordoneaza dupa data crearii», which hides the sector / source selector in the caption bar, so the session's SS says nothing about the angajament: the program must come from the SS of the angajament's own indicators (`FX_Indicatori.SS`). Documents already saved with a wrong program need a repair, put in the DDF ribbon, with the main-tree row painted in a red not used before and the error written out in the tooltip.

Note on the rule: it is the same one `AVACONT_COMUN.DefaProgram` holds (01A -> 0000002510, 02A / 02E -> 0000000000). An angajament with indicators on 01A AND on another SS has no single program: nothing is guessed (not reported, not repaired; the editor already warns).

### 1. Server
- NEW `PYTHON/routes/forexe/ddf_program.py`: the rule (`program_pentru_surse`, `program_pentru_angajament`), the finder `gaseste_programe_gresite` (one query: `FX_DDF` JOIN `FX_Indicatori`, `GROUP_CONCAT(DISTINCT SS)`), `POST /api/forexe/ddf/repara-program {cod, aplica}` (dry run or write; writes `FX_DDF.Program` only, one transaction).
- `tree.py`: tree rows get a `ProgramGresit` column («actual~asteptat~surse», NULL = right).
- `ddf_edit.py`: `/genereaza` with `rev0` takes the program from the indicators (it was `PROGRAME[0]`). `PROGRAME` stays: it is the combo's row source.
- `__init__.py`: registers the module.

### 2. Client
- Domain: `DdfSectiuneaAReguli.ProgramPentruSurse` (same rule, from the node's `Surse`) + constants; `DdfProgramGresit`, `DdfProgramRepairResult` (new `DdfProgramGresit.vb`); `AngajamentTreeInfo.ProgramGresit`.
- Api: `GetTreeRow.ProgramGresit`, `ParseProgramGresit`, `IDdfProgramApi.RepairDdfProgramAsync` + `ApiClient.DdfProgram.vb`.
- `KbotForm.Ddf.vb`: `AdaugaRezervareDdfAsync` (revision 0 of an existing angajament) starts from `ProgramPentruSurse(info.Surse)`; `ReparaProgramDdf` = dry run, Yes/No (default No), write, tree reloaded with the selection kept. The editor's existing correction on opening (`AplicaProgramulIndicatorilorAsync`) stays as the second safety net.
- `DdfView` implements `IViewRibbon`: button `ddf_repara_program` («Repara programul»), shown only while `ProgramGresit` exists. `AvacontMain.vb` / `.Designer.vb`: key, dispatch, button in the right group. `KbotForm.Views.vb` passes `ReparaProgramDdf`.
- `KbotForm.Tree.vb`: third red = `RosuAlternativ(ErrorColor, +20 degrees)` (orange-red; the second red is -22, crimson); a row with an older error keeps the older red. The hover table gets a block «Program gresit in DDF»: program in the document, correct program, source of the indicators, the rule, where to repair.

### 3. Same day, second pass (operator: «DE CE?» about the XML, and a new angajament must not get a default)
- **The DDF XML / PDF takes the program from the document's header, not the session.** `DdfXmlBuilder.Context.ForDocument(session, antet)` (unit name and tax code from the session, program from `antet.Program`; a header without a program THROWS). Used by `DdfPdfGenerator` (the generated PDF) and `DdfView` (section B insert; its fallback is now the header too). `KbotForm.DdfSend` sends `draft.Program` only, no session fallback; a document without a program is not sent (message). `FromSession` stays for the DevHarness bench.
- **A new manual angajament starts with NO program** (`ForNewAngajament` is given an empty one). The editor no longer selects the first item of the program combo when the document has none (`SelectedIndex = -1`; this default is what put wrong programs in saved documents). The header already counts an empty program as missing (`LipsuriAntet`), and the server save (`_valideaza_graf` in `ddf_edit.py`) now refuses it: «Programul lipsește: alegeți programul documentului.»
- **The ordonantare (ORD) too** (operator, same day): `OrdXmlBuilder.Context.ForAngajament(session, surse)` takes the program from the angajament's indicators' SS (`DdfSectiuneaAReguli.ProgramPentruSurse`; indicators spanning programs throw). `OrdView` keeps the node's `Surse` (`_surse`, set in `SetContext`, cleared in `ClearAll`) and passes it to `OrdPdfGenerator.GenereazaAsync` (new parameter) and to the generation in the view. `FromSession` stays for the bench.
- **The interim document no longer carries a placeholder program** (operator: «programul e critic»). `DdfXmlBuilder.InterimContext` used to force `0000000000` in section A (Cell2), section B (Cell3) and the NOTAFD `program` attribute, so section A was SIGNED with a program that could be wrong and section B copied it. It now keeps the real program of the document (the caller's context from `Context.ForDocument`); `InterimProgram` is gone. The placeholders that remain are section B's code (`___________`) and indicator (`___`), which «Valideaza» needs. `DdfSectionBInsert.InsertAsync` writes the HEADER's program in section B (the program of the signed A is only read and reported back, `Rezultat.ProgramSectiuneaA/B`); `DdfView` warns when the signed A and the header disagree (a document signed on an older interim: A cannot change once signed, the revision must be redone).

## Files touched
Server: `PYTHON/routes/forexe/ddf_program.py` (new), `__init__.py`, `tree.py`, `ddf_edit.py` (rev 0 program, save refuses an empty program).
Client: `src/KBot.Domain/DdfProgramGresit.vb` (new), `DdfSectiuneaAReguli.vb`, `AngajamentTreeInfo.vb`; `src/KBot.Api/IDdfProgramApi.vb`, `ApiClient.DdfProgram.vb`, `ApiClient.vb`, `UpsertAngajamenteRequest.vb`; `src/KBot.App/KbotForm.Ddf.vb`, `KbotForm.Tree.vb`, `KbotForm.Views.vb`, `KbotForm.DdfSend.vb`, `Views/DdfView.vb`, `Views/Ddf/DdfXmlBuilder.vb`, `Views/Ddf/DdfPdfGenerator.vb`, `Views/Ord/OrdXmlBuilder.vb`, `Views/Ord/OrdPdfGenerator.vb`, `Views/OrdView.vb`, `DDF_EDIT/DdfEditForm.vb`, `AvacontMain.vb`, `AvacontMain.Designer.vb`.
Help (0000-121): `contabil/fereastra.md`, `contabil/ddf/index.md`, `contabil/ddf/rezervare.md`.

## Test results
- `dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors.
- `py_compile` of the four Python files: ok.
- No tests written or run (operator rule). Nothing run on MariaDB, nothing seen on screen, server not touched (the operator puts the files on it by hand: `ddf_program.py` is NEW and goes together with `tree.py`, `ddf_edit.py`, `__init__.py`).

## Unverified / deferred
- The interim form was proven (28.09.2026) with the placeholder program `0000000000`; it now gets the real one (`0000002510` for 01A). The form's «Valideaza» was not run with it: if it refuses `0000002510` (it should not, it is the Access value), the signature field will not enable.
- Documents whose section A was ALREADY signed with the placeholder keep it (a signed A cannot change); the section-B insert warns on the mismatch.
- The repair writes the header only: a PDF already generated or signed keeps what it holds; a revision already sent to FOREXE is not resent.
- The editor combo with `SelectedIndex = -1` and the save refusal were not run or seen.
- The orange-red (+20 degrees from the theme's error colour) and the ribbon button were not seen on screen; the +20 may need a nudge on a theme where the error colour is not near hue 0.
- The detection assumes one `FX_DDF` row per angajament (it groups by `IDDF` anyway).

### 4. Detection rewritten on the saved TABLE data (operator, 23.10.2026: «strict datele din tabele», shown at opening like the reservation / reception errors)
The first detector compared only `FX_DDF.Program` with the SS of `FX_Indicatori`, so it showed nothing for the operator's case. Now `gaseste_programe_gresite` (`ddf_program.py`, four queries for the whole database, run by `tree.py`) compares the DDF header program with the program of the SS saved in: the indicators (`FX_Indicatori.SS`), every saved DDF revision (`FX_DDF_REV_SA.SS`) and every saved ordonantare (`FX_ORD_TBL.CodSSI`, first 3 characters = SS; `FX_ORD` has no program column). Any place that calls for another program than the header is reported, one entry per place (`IND` / `REV n` / `ORD n`, with the SS and the program it calls for). The correct program is the indicators' when they give a single one, else the single program all the data agree on, else none (the repair then refuses). PDFs are not read.
- Wire: tree `ProgramGresit` = `actual~asteptat~tip^numar^ss^program|...`; repair answer carries `detalii`.
- Client: `DdfProgramGresit.Dovezi` (`DdfProgramDovada`, `DdfProgramSursa`), tooltip lists each disagreeing place («Revizia 3 (DDF)», «Ordonanțarea 12», «Indicatorii angajamentului»), the repair window lists them, ribbon tooltip updated.
- Unverified: queries not run on MariaDB; `FX_ORD_TBL.CodSSI` taken as `SS + ClsfSal` (`ord_edit.py`), so its first 3 characters are the SS; a table with another shape would report nothing / fail loudly in the log. The tree response now needs `ddf_program.py` on the server and a restart.
