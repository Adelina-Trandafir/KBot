# SLICE 0081-09 — Locked header, the source / sector in section A, the partner only in the header

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

## What was wrong

- **(1)** `AplicaEnablement` opened the header whenever the DOCUMENT was new (Access's `DDF_NOU`).
  «Adaugă rezervare» on an angajament with no K-BOT document (`ForFirstRevisionOfExisting`) and the
  generated first revision are new documents on an angajament that already exists, so their header
  was typeable.

## What changed

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

## Files touched

- `PYTHON/routes/forexe/ddf_edit.py`
- `src/KBot.Api/IDdfProgramApi.vb` (new), `ApiClient.DdfProgram.vb` (new)
- `src/KBot.Domain/DdfSectiuneaAReguli.vb` (new), `DdfDraft.vb`, `DdfSending.vb`
- `src/KBot.App/DDF_EDIT/DdfEditForm.vb`, `DdfEditSectiuneaAPage.vb`, `DdfEditLinieAForm.vb`,
  `DdfEditLinieAForm.Designer.vb`, `DdfEditReauth.vb`
- `src/KBot.App/KbotForm.vb`
- `tests/KBot.Domain.Tests/DdfSectiuneaAReguliTests.vb` (new)

## Test results

- `dotnet build src\KBot.App` (into a scratch folder): **0 errors, 0 warnings**.
- The row window was rendered off-screen with DrawToBitmap.
  - Program 0000000000: SS enabled with 02A / 02E, 02A proposed, 2 classifications; 02E gives 1.
  - Program 0000002510: SS enabled with only 01A (K-BOT's 02A not proposed: not in the program),
    1 classification.
- The VB Domain tests are written and **not run**. The Flask route has no test and was **not run**.

## Left unverified / deferred

- The route must be deployed to the VPS. Until then, «Adaugă rând» says the sources could not be
  fetched.
- The editor's header lock was not rendered: it needs the API to open.
- The compartment/object exception (open while empty) needs the operator's word.
