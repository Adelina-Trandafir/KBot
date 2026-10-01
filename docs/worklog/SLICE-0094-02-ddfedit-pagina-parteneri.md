# SLICE 0094/02 — DDF editor: the «Parteneri» page (operator, 01.10.2026)

Operator request: `DdfEditForm` must allow associating several partners with a DDF. (a) The current
combo in the header stays: it selects ONE partner — most documents have just that. (b) A new view,
aligned to the right — «Parteneri» — presents the form that associates partners with a DDF.

Filed under 0094-02 because 0094 is the last slice that did something to `DdfEditForm` (its designer
changed with the combo rewrite). The table, the server routes and the shared list control are
**0084/02** (the Sumar button); the help and the guided tour are **0000-28**.

## What changed and why

- **`DdfEditForm`** gets a fifth nav entry, **«Parteneri»**, `KBotNavAlign.Far` (right-aligned in the
  `navSub` bar), key `parteneri`. The page is created on first activation like the other four.
- **`DdfEditPartnersPage`** (new `IDdfEditPage`): an information line + the shared
  `DdfPartnersView`. It edits `DdfDraft.Parteneri` — no request of its own; its picker is the header
  combo's list, handed down by the form.
- **The header combo stays and stays the MAIN partner.** `DdfDraft.SyncHeaderPartner` keeps the two in
  step: the combo's partner is the first row of the list (role «Principal»), cannot be removed from
  the page, and picking another one REPLACES the previous main partner (the combo is a single choice;
  extras stay). Unticking «Partener asociat» removes the main partner's row; the others stay.
- **Save**: `CatreFir` always sends the COMPLETE list; the server (`sincronizeaza_parteneri` inside
  the save transaction, step 3b) makes `FX_DDF_Parteneri` match it, the main partner always among
  the rows. A body without the key (an older client) leaves the table untouched.
- **Read**: `GET /ddf/draft/<iddf>/<idrev>` and `POST /ddf/genereaza` (existing document) now carry
  `parteneri`; `DdfDraftFactory.ForAddedReservation` copies the list, so a new revision on an existing
  document does not drop the partners on its save.
- Only the MAIN partner is written on the section-A / B lines (unchanged). The page says so.

## Files touched
`src/KBot.App/DDF_EDIT/DdfEditForm.vb`, `DdfEditForm.Designer.vb`, `DdfEditPartnersPage.vb` +
`.Designer.vb` (new), `src/KBot.Domain/DdfDraft.vb` (`Parteneri`, `SyncHeaderPartner`,
`AddAssociatedPartner`, `RemoveAssociatedPartner`), `src/KBot.Domain/DdfSending.vb`,
`src/KBot.Api/DdfEditContract.vb` (`DdfDraftDto.parteneri`, `DdfDraftPartenerDto`),
`src/KBot.Api/ApiClient.vb` (`CitesteDdfDraft`, `CatreFir`), `PYTHON/routes/forexe/ddf_edit.py`
(import, `genereaza`, `draft`, save step 3b).

## Test results
- `py_compile`: green. `dotnet build src\KBot.App\KBot.App.vbproj`: **0 warnings, 0 errors**.
- No tests written or run. The editor was **not run and not seen on screen**: the right alignment of
  the nav entry, the page layout at other DPIs and the theme colours are unchecked.

## Left unverified / deferred
- ⚠ Same deploy order as 0084/02: the DDL first, then `ddf_edit.py` + `ddf_parteneri.py`. Before the
  DDL, a save that adds a SECOND partner is refused with the name of the .sql file; a save with only
  the main partner works.
- Replacing the previous main partner when the combo changes is a choice, not something the operator
  said. If an extra the operator added by hand should survive being the main partner and then not,
  that needs a per-row «added by hand» flag.
- The page's picker is filled from `GET /ddf/parteneri`, scoped to the units of the angajament's
  indicators (a new angajament: every unit). A partner outside that scope is not offered.
- `OrdEditForm` and the ORD flow still read only the header partner.
