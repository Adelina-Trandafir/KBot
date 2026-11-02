# SLICE-0094-02…03 — Partenerii asociați în editorul DDF

Istoric distinct de rescrierea controlului ComboBox din 0094. Etapa 03 înlocuiește vizibilitatea permanentă a filei din 02; ascunderea ei păstrează partenerii suplimentari la salvare, cu decizia operatorului încă deschisă. Probe, presupuneri și limite pe etapă.

[0094-02 — listă, partener principal și salvare](#slice-0094-02) · [0094-03 — vizibilitate după bifă](#slice-0094-03)

## SLICE 0094/02 — DDF editor: the «Parteneri» page (operator, 01.10.2026) <a id="slice-0094-02"></a>

Operator request: `DdfEditForm` must allow associating several partners with a DDF. (a) The current combo in the header stays: it selects ONE partner — most documents have just that. (b) A new view, aligned to the right — «Parteneri» — presents the form that associates partners with a DDF.

Filed under 0094-02 because 0094 is the last slice that did something to `DdfEditForm` (its designer changed with the combo rewrite). The table, the server routes and the shared list control are **0084/02** (the Sumar button); the help and the guided tour are **0000-28**.

### What changed and why

- **`DdfEditForm`** gets a fifth nav entry, **«Parteneri»**, `KBotNavAlign.Far` (right-aligned in the `navSub` bar), key `parteneri`. The page is created on first activation like the other four.
- **`DdfEditPartnersPage`** (new `IDdfEditPage`): an information line + the shared `DdfPartnersView`. It edits `DdfDraft.Parteneri` — no request of its own; its picker is the header combo's list, handed down by the form.
- **Header combo = MAIN partner.** `DdfDraft.SyncHeaderPartner` keeps it first in the list (role «Principal»), nonremovable from the page. Another combo choice REPLACES the main partner, retaining extras; unticking «Partener asociat» removes only the main row.
- **Save**: `CatreFir` always sends the COMPLETE list; the server (`sincronizeaza_parteneri` inside the save transaction, step 3b) makes `FX_DDF_Parteneri` match it, the main partner always among the rows. A body without the key (an older client) leaves the table untouched.
- **Read**: `GET /ddf/draft/<iddf>/<idrev>` and `POST /ddf/genereaza` (existing document) now carry `parteneri`; `DdfDraftFactory.ForAddedReservation` copies the list, so a new revision on an existing document does not drop the partners on its save.
- Only the MAIN partner is written on the section-A / B lines (unchanged). The page says so.

### Files touched
`src/KBot.App/DDF_EDIT/DdfEditForm.vb`, `DdfEditForm.Designer.vb`, `DdfEditPartnersPage.vb` + `.Designer.vb` (new), `src/KBot.Domain/DdfDraft.vb` (`Parteneri`, `SyncHeaderPartner`, `AddAssociatedPartner`, `RemoveAssociatedPartner`), `src/KBot.Domain/DdfSending.vb`, `src/KBot.Api/DdfEditContract.vb` (`DdfDraftDto.parteneri`, `DdfDraftPartenerDto`), `src/KBot.Api/ApiClient.vb` (`CitesteDdfDraft`, `CatreFir`), `PYTHON/routes/forexe/ddf_edit.py` (import, `genereaza`, `draft`, save step 3b).

### Test results
- `py_compile`: green. `dotnet build src\KBot.App\KBot.App.vbproj`: **0 warnings, 0 errors**.
- No tests written or run. The editor was **not run and not seen on screen**: the right alignment of the nav entry, the page layout at other DPIs and the theme colours are unchecked.

### Left unverified / deferred
- ⚠ Same deploy order as 0084/02: the DDL first, then `ddf_edit.py` + `ddf_parteneri.py`. Before the DDL, a save that adds a SECOND partner is refused with the name of the .sql file; a save with only the main partner works.
- Replacing the previous main partner when the combo changes is a choice, not something the operator said. If an extra the operator added by hand should survive being the main partner and then not, that needs a per-row «added by hand» flag.
- The page's picker is filled from `GET /ddf/parteneri`, scoped to the units of the angajament's indicators (a new angajament: every unit). A partner outside that scope is not offered.
- `OrdEditForm` and the ORD flow still read only the header partner.

---

## SLICE-0094-03 — DDF editor: the «Parteneri» tab follows «Partener asociat» <a id="slice-0094-03"></a>

Corrective on 0094-02 (operator, 04.10.2026). Only `DdfEditForm`.

### What changed and why

The «Parteneri» tab of the DDF editor was always on the page bar. It is now tied to the «Partener asociat» checkbox (`chkPartAng`, `_draft.PartAng`):

- **On load** the tab is visible only if the document has `PartAng` (set next to the section-B visibility, before the first page is selected).
- **On toggling the box** (`ChkPartAng_CheckedChanged`) the tab is shown/hidden. A hidden nav item stays selected in `KBotNavList`, so when «Parteneri» is the open page and the box is cleared, the editor switches to «Secțiunea A».

Nothing else moved: `KBotNavList` unchanged, no other form touched, the draft and the save are unchanged (see deferred).

### Files touched

`src/KBot.App/DDF_EDIT/DdfEditForm.vb`: `DdfEditForm_Load`, `ChkPartAng_CheckedChanged`. Help tags +0094-03 in `HelpContent/contabil/ddf/editor.md`, `HelpContent/tours/tur-ddf-editor.md`. No tour/tutorial structural change: `KBotNavList.HelpParts` gives hidden items an empty rectangle, skipping them; 000T step «Fila «Parteneri»» already uses `when: checked:DdfEditForm.chkPartAng`.

### Test results

`dotnet build src\KBot.App\KBot.App.vbproj -c Debug`: 0 warnings, 0 errors. No tests run, nothing seen on screen (house rule).

### Unverified / deferred

- Not seen on screen: the tab disappearing/appearing, and the fall-back to section A.
- **Hidden partners are still saved.** Clearing the box removes only the main partner from the list (0094-02 behaviour); the other associated partners stay in `_draft.Parteneri` and are sent on save, although the tab that shows them is hidden. Whether clearing the box should drop them too is the operator's call.
- `help-version.txt` not bumped here (already modified in the working tree by other help work).

Reper de instalare din registrul 0094-02: `sql/0084_02_fx_ddf_parteneri.sql` pe bazele de unitate și `AVACONT_SURSA`, apoi `ddf_parteneri.py`, `ddf_edit.py`, `__init__.py` pe VPS. Nimic deployat la checkpoint.
