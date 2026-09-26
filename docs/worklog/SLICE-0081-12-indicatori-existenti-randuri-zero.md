# SLICE 0081-12 — New revision from the existing indicators, used classifications first, zero lines dropped at save

**Date:** 26.09.2026. **Request (operator), DDF editor:**
1. «Adauga rezervare» from the + in the Rezervari tree footer opens a popup with
   «1. Adaugă revizie goală» and «2. Folosește indicatorii existenți». With 2, the section-A grid
   of the editor starts filled with every indicator code the selected angajament already has.
2. In `DdfEditLinieAForm`, the classification list -- the real dropdown AND find-as-you-type --
   shows the classifications already in the angajament first, then the unused ones.
3. At save, section-A lines with value 0 are dropped, after a confirmation message.
4. If no line with a non-zero value is left, the document is NOT saved.

## What changed

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

## Correction the same day: the program of the prefilled revision

The operator got «Angajamentul nu are indicatori de preluat» on an angajament that has them.
`ForAddedReservation` copies the program of the LAST REVISION (`FX_DDF.Program`), which can be
wrong; the SS filter of the prefill then removed every classification. Operator's rule: the
program is `FX_Indicatori.SS` JOIN `AVACONT_COMUN.DefaProgram` (`CONCAT(Sursa, Sectorul)`) -- the
source of truth, nothing else. («1. Adaugă revizie goală» and the new DDF keep the current logic.)

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

## Second request the same day: program on the «+» path, budget, the value grids

**Request (operator):** (a) the same program rule when a revision is added from the Rezervari
tree's row «+» (`/genereaza`); (b) the budget of a new revision NOT based on a reservation comes
from `FX_Indicatori.Credit_Bugetar`; (c) in `DdfEditLinieAForm` every value in a designer-built
grid, 90 wide, Standard format, no footer, only the current value editable, columns Buget,
Val. receptii, Disponibil (= Buget - Receptii), Val. precedenta, Val. curenta, Val. ramasa;
(d) the same columns in the section-A grid of `DdfEditForm`. Operator's answers to two questions:
**Val. ramasa = Disponibil - ValCur**; **ValPrec stays** (before ValCur).

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

## Third round the same day (operator: "still wrong")

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

## Files touched

- `src/KBot.Domain/DdfSending.vb`, `src/KBot.Domain/DdfSectiuneaAReguli.vb`, `src/KBot.Domain/DdfDraft.vb`
- `src/KBot.Api/DdfEditContract.vb`, `src/KBot.Api/ApiClient.vb`
- `src/KBot.Controls/Combo/KBotComboBox.vb`, `src/KBot.Controls/DataView/KBotDataView.Editing.vb`
- `src/KBot.App/DDF_EDIT/DdfEditLinieAForm.Designer.vb`, `DdfEditSectiuneaAPage.Designer.vb`
- `PYTHON/routes/forexe/ddf_edit.py`
- `src/KBot.App/Views/RezervariView.vb`, `src/KBot.App/KbotForm.vb`
- `src/KBot.App/DDF_EDIT/DdfEditForm.vb`, `DdfEditLinieAForm.vb`, `DdfEditSectiuneaAPage.vb`

## Test results

- `dotnet build src\KBot.App\KBot.App.vbproj -c Debug`: **0 errors, 0 warnings**.
- No tests written, run or built (operator's rule, 26.09.2026: no test code unless asked).
- `ddf_edit.py` compiles (`py_compile`, the venv).
- Rendered off-screen (DrawToBitmap, scratch program): the line window with Buget 10.000,
  Receptii 200, ValPrec 300, ValCur 1.500 -> Disponibil 9.800,00, Val. ramasa 8.300,00; the
  section-A page with two lines -- six value columns, no footer.
- Nothing run live.

## Left unverified / deferred

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
