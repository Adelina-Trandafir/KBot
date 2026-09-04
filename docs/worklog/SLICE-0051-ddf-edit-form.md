# SLICE-0051 — `DdfEditForm`, the DDF write path

**State: code complete, builds clean, unverified against a live system.**

Language note: this worklog is in English, per plan §0.3 (this slice is English-only in
comments and documentation, unlike slice 0049 which shipped Romanian-without-diacritics).
Operator-facing strings are Romanian with real UTF-8 diacritics.

**Nothing in this slice has run against MariaDB or against the Python server.** Neither is
reachable from the development machine. Every SQL statement below was written by reading
`MariaDB_Schema/000_DEMO.sql` — a structure-only dump, carrying no `INSERT` rows for any
`FX_DDF*` table, so not one value could be sampled. Read §9 before deploying anything.

---

## 1. What was ported

| Access source | K-BOT |
|---|---|
| `frmFX_DDF` (the editor shell) | `src/KBot.App/Views/Ddf/DdfEditForm.vb` |
| `frmFX_DDF_REV` (header) | the header band of `DdfEditForm` |
| `frmFX_DDF_REV_SECT_A` | `DdfEditSectiuneaAPage.vb` |
| `frmFX_DDF_REV_SECT_B` | `DdfEditSectiuneaBPage.vb` |
| `frmFX_DDF_ATT` | `DdfEditFisierePage.vb` |
| the long-description box | `DdfEditDescrierePage.vb` + `KBot.Controls/RichText/` |
| `mdl_FX_DDF` (generation) | `POST /api/forexe/ddf/genereaza` |
| `mdl_FX_DDF_Salvare` | `POST /api/forexe/ddf/save` |
| `mdl_FX_DDF_Salvare_Local` | folded into the same save transaction |
| `FX_Stergere_Revizie / _DDF / _Revizii` | the three `DELETE` routes |

### Files added

```
sql/0051_ddf_rev_att_img.sql                                132 lines
sql/0051_fx_numberlock.sql                                  116
PYTHON/routes/forexe/ddf_edit.py                           2400
src/KBot.Domain/DdfDraft.vb                                 699
src/KBot.Api/DdfEditContract.vb                             293
src/KBot.App/Views/Ddf/DdfEditForm.vb + .Designer.vb       1733
src/KBot.App/Views/Ddf/DdfEditSectiuneaAPage.vb + .Des.     755
src/KBot.App/Views/Ddf/DdfEditSectiuneaBPage.vb + .Des.     265
src/KBot.App/Views/Ddf/DdfEditDescrierePage.vb + .Des.      261
src/KBot.App/Views/Ddf/DdfEditFisierePage.vb + .Des.        498
src/KBot.App/Views/Ddf/IDdfEditPage.vb                       48
src/KBot.App/Views/Ddf/DdfEditReauth.vb                      56
src/KBot.App/Views/Ddf/DdfComanda.vb                         80
src/KBot.App/Views/Ddf/IDdfForexeUpload.vb                   45
src/KBot.Controls/RichText/KBotRichTextEditor.* (4 partials)  2696
src/KBot.Controls/RichText/KBotNoFocusButton.vb             245
src/KBot.Controls/RichText/RichTextImageLayout.vb            26
tests/KBot.App.Tests/DdfEditGrileTests.vb                   163
tests/KBot.Controls.Tests/RichTextButtonImageLayoutTests.vb  209
tests/KBot.Controls.Tests/RichTextIconResolutionTests.vb      157
src/KBot.DevHarness/Internal/RichTextPlaygroundForm.vb + .Des. 906
src/KBot.DevHarness/Tests/KBotRichTextEditorPlaygroundTest.vb  49
```

### Files changed

`PYTHON/routes/forexe/__init__.py` (registration only), `src/KBot.Api/ApiClient.vb`,
`src/KBot.Api/IApiClient.vb`, `src/KBot.App/MainForm.vb`, `src/KBot.App/Views/DdfView.vb`,
`src/KBot.App/Views/Ddf/DdfFileBrowser.vb + .Designer.vb` (§4.5), and nine files under
`tests/KBot.App.Tests/` — the last of these are **test doubles that implement `IApiClient`**.
They gained sixteen `Throw New NotSupportedException()` stubs so the interface still compiles.
That is compilation bookkeeping, **not** coverage: no test exercises any of the new members.

**D19 held.** `git diff -- PYTHON/routes/ddf/ PYTHON/routes/forexe/ddf.py` is empty, and so
is the diff on `sql/0049_ord_att_img.sql`.

---

## 2. The four decisions the operator made

Three §13 stop-and-report conditions were genuine stops, and two further defects were found
in the plan that were not on the §13 list. All were put to the operator; all four answers are
recorded here because the code cannot be read without them.

### 2.1 `Desc_Lunga_ANSI` is not dead — D9 is reversed

D9 declared `Desc_Lunga_ANSI` dead. It is not. `PYTHON/routes/forexe/ddf.py` selects it,
emits it as `desc_lunga`, `ApiClient` maps it onto `DescLunga`, and `DdfXmlBuilder` writes it
into the signed XFA document (`DescrieObFundRevizuireLung`, and `Left(…, 500)` into the
section-A attribute `obiect_fd_reviz_lung`). Writing only `Desc_Lunga` would have made every
document this slice creates render with an empty long description — through a route D19
forbids touching.

**Operator's answer: write both columns.** The save writes the same text to `Desc_Lunga` and
`Desc_Lunga_ANSI`. It is a duplicated column and it is deliberate: the read path that feeds
the signed document is out of this slice's reach.

### 2.2 `Comp` — read the existing values, but let the operator type a new one

The plan gave no source for the `Comp` field. **Operator's answer:** list the values already
used on earlier DDFs; if there are none the list is empty; the operator may always type a
value that is not in the list.

Server side that is `SELECT DISTINCT Comp FROM FX_DDF` (`GET /api/forexe/ddf/comp`).

Client side it forced a **two-control workaround**, and the reason is worth recording:
`KBotComboBox` **throws** on any `DropDownStyle` other than `DropDownList` — its owner-drawn
painting cannot theme a native `EDIT` child, so an editable combo does not exist in this
code base. The header therefore carries `txtComp` (a `KBotTextField`, which holds the
authoritative value) next to `cmbComp` (a picker that writes into it). Both are documented as
such in `DdfEditForm.Designer.vb`. Making `KBotComboBox` editable is a control-level job and
is listed in `possible_future_directions.md`.

### 2.3 `GenerateUniqueSequence` — the VBA source, ported

The plan referred to a function whose source was not in the export. The operator supplied the
original VBA, with its attribution:

```
' Source - https://stackoverflow.com/a/44681287
' Posted by Séb Cô, modified by community. See post 'Timeline' for change history
' Retrieved 2026-02-11, License - CC BY-SA 4.0
```

It is ported in `DdfDraft.vb` as `CaracterAleator` / `GenereazaUnic`, attribution kept.

Two notes on the port. First, the original's comment claims each character class has "one
chance out of 3"; it does not — `Round(Rnd * 2 + 1)` with banker's rounding gives 1/4 digits,
1/2 uppercase, 1/4 lowercase. The skew is harmless for a uniqueness token, so the behaviour
was ported unchanged and the comment corrected in place. Second, `GenereazaUnic` re-draws up
to twenty times against the codes already in the draft and **throws** rather than returning a
duplicate: a silent duplicate would collide on save.

### 2.4 `FX_DDF_REV_ATT_IMG` is missing from the schema — DDL delivered

The attachment-bytes table the plan assumes does not exist in `000_DEMO.sql`. The operator
confirmed it is missing from the real schema and asked for DDL, keeping the unique index.

Delivered as `sql/0051_ddf_rev_att_img.sql`, shaped exactly like the ORD equivalent
(`FX_ORD_ATT_IMG`) including the `TipMime` column and
`UNIQUE INDEX UQ_FX_DDF_REV_ATT_IMG_ATT(IdRevAtt)`, with
`FOREIGN KEY (IdRevAtt) REFERENCES FX_DDF_REV_ATT(IdRevAtt) ON DELETE CASCADE ON UPDATE RESTRICT`.
The file also carries probe statements and a commented `AVACONT_SURSA` variant (that database
does not use `AUTO_INCREMENT`). `sql/0051_fx_numberlock.sql` follows the same pattern for
`FX_NumberLock`.

---

## 3. The server module

`PYTHON/routes/forexe/ddf_edit.py`, sixteen routes, every one `@require_session` (bearer —
this is a K-BOT client, not the VBA/FOREXE legacy path). The database name comes from
`g.session.db_name`; no route accepts one from the caller. Every response is dumped with
`ensure_ascii=False`; every refusal message is Romanian with diacritics.

```
POST   /api/forexe/ddf/genereaza
GET    /api/forexe/ddf/draft/<iddf>/<idrev>
GET    /api/forexe/ddf/clasificatii
GET    /api/forexe/ddf/parteneri
GET    /api/forexe/ddf/comp
POST   /api/forexe/ddf/numar/rezerva
POST   /api/forexe/ddf/numar/<idlock>/schimba
POST   /api/forexe/ddf/numar/<idlock>/prelungeste
DELETE /api/forexe/ddf/numar/<idlock>
POST   /api/forexe/ddf/save
DELETE /api/forexe/ddf/rev/<idrev>
DELETE /api/forexe/ddf/<iddf>
DELETE /api/forexe/ddf/<iddf>/luna/<an>/<luna>
GET    /api/forexe/ddf/att/<idrevatt>/imagine
PUT    /api/forexe/ddf/att/<idrevatt>/imagine
DELETE /api/forexe/ddf/att/<idrevatt>/imagine
```

The route map was dumped from a bare `Flask()` to prove three things:
`GET /api/forexe/ddf` (slice 0020) still resolves, `DELETE /api/forexe/ddf/<int:iddf>` does
not shadow it, and nothing collides with `/api/forexe/ddf/pdf/<int:idrev>`.

### 3.1 The classification key inversion

This trips every DDF query and is written down again because getting it wrong silently
returns the wrong rows:

* `FX_DDF_REV_SA.IdClsf` / `FX_DDF_REV_SB.IdClsf` = the **MariaDB** primary key
  `Clasificatii.IDClsf`.
* `FX_Indicatori.IdClsf`, `FX_Rezervari.IdClsf`, `FX_Receptii.IdClsf` = the **Access** id,
  which matches `Clasificatii.IdClsfAcc`.

So the generation queries join
`Clasificatii C ON C.IdClsfAcc = I.IdClsf AND C.IdUnitate = I.IdUnitate`, while the draft
reads join on `C.IDClsf`.

`Clasificatii` has **no** `CodSSI` column. It is composed: `CONCAT(C.SS, C.ClsfSal) AS CodSSI`.

### 3.2 Deliberate divergences from Access

Each of these is a place where the port does **not** do what the Access original did. They
are choices, not accidents.

1. **Validation checks every row.** `msgEroare` in Access examined only the first row of each
   section before returning. The port checks all of them and lists every reason at once.
2. **Deleting a document releases every revision's reservations.** The Access
   `FX_Stergere_DDF` released only the reservations of one revision, leaving the rest of the
   document's reservations marked as used against a document that no longer existed. The port
   releases all of them. This is a bug fix, and if the old behaviour was load-bearing
   somewhere it will show up here first.
3. **The source of the generated lines is chosen from the data** (decision D5) rather than
   from a form's state as Access did. `_alege_sursa()` looks at what actually exists —
   reservations or indicators — and **refuses loudly, naming both conditions**, when it
   cannot tell. It never guesses.
4. **The `IdUnitate` predicate is kept** in the generation queries. Access omitted it in
   places; `Clasificatii` and the other nomenclature tables are unit-scoped, so omitting it
   returns another unit's rows on a multi-unit database.
5. **The `PrtScr` filter was not ported.** It filtered the Access screen; it has no meaning
   in the K-BOT editor.
6. **Section B copies the partner from section A.** Access wrote `NULL` there. The B rows are
   derived from the A rows, and a derived row with no partner cannot be reconciled afterwards.
7. **The earliest-date, lowest-operation-type selection rule** in `_SQL_GEN_REZERVARI` is in
   the Access query and was omitted from the plan; it is ported, because dropping it changes
   which reservation row wins.

### 3.3 Writing

`_scrie_graf` performs the nine write steps in one transaction. New rows arrive with negative
`TempId` values and the response carries `TempId → real key` maps, so the client can adopt the
keys without re-reading. `_cheie_noua` raises when `lastrowid` comes back `0` rather than
writing a child against key zero. `_sterge_absentii` removes rows the operator deleted.
After the graph is written, `FX_Rezervari` and `FX_Angajamente` are updated, with the text
columns explicitly truncated to 255 — MariaDB in strict mode would otherwise refuse the whole
transaction on a long description.

Attachments are a **second phase**, after the transaction: the bytes need an `IdRevAtt` to
hang off. The PUT carries the file name in the `X-Nume-Fisier` header (same as ORD) and uses
`ON DUPLICATE KEY UPDATE`, which is what the unique index on `IdRevAtt` is for.

If phase two fails, **nothing is rolled back**. The operator is told which files did not
upload and to attach them again. A half-rolled-back document is worse than a document missing
a file.

### 3.4 The number lock

`FX_NumberLock`, TTL 60 minutes, heartbeat every 5. Allocation is
`GREATEST(<max in use>, <max locked>) + 1`, with the floor at 0 for CUAL and **-1** for
`NumarRev` so that the first revision of a document is numbered 0. Expired locks are swept on
every reserve. The lock is consumed inside the save transaction (`_consuma_lacatul`), so a
form that saved no longer holds it and does not try to release it on the way out.

This is deliberately unlike slice 0049, which guessed the next number ("probabil N") and hoped.

---

## 4. The client

### 4.1 Shape

`DdfDraft` (in `KBot.Domain`) is the whole editable graph: header, `LiniiA`, `LiniiB`,
`Atasamente`. `RecalculeazaSectiuneaB()` derives B from A and is called immediately before
validation on save, not merely when a cell changes — a stale B row would otherwise be written.

The four pages implement `IDdfEditPage` and make **no network requests of their own**. That
is why the three lookups `cmbClsf_AfterUpdate` performed in Access (previous value, receptions
value, indicator code) were moved server-side into `GET /api/forexe/ddf/clasificatii` as
pre-computed sub-selects: the page reads them off the row it already has.

The classification list is the three-part list Access built (on this angajament / same title /
manual), with a separator row carrying `IDClsf = -1` inserted in Python.

### 4.2 Re-authentication

`DdfEditReauth` carries seven named delegates. The 401 re-login net (`WithReauth`) is private
and generic in `MainForm`, so the editor is handed the wrapped calls rather than reaching for
the policy itself. That keeps one re-login policy in one place, as every other view does.

### 4.3 Where the commands come from

`DdfView` gained an optional `executaComanda As Action(Of DdfComanda)` constructor parameter
and a right-click menu on the tree, hung off the **existing** `Tree_NodeMouseUp` *after* the
read path — a failure while building the menu cannot break selection, and the view stays
usable read-only. A host that supplies no action (the tests) gets the full read-only view and
the menu says so rather than doing nothing.

`MainForm.ExecutaComandaDdf` executes them.

**`Adauga` and `AdaugaRevizieInitiala` are called from the RESERVATIONS tree.** The `+`
icon on a reservation leaf is the trigger — `RezervariView.Tree_RightIconClicked` builds the
command and hands it to `MainForm.ExecutaComandaDdf` — which is exactly where Access put it:
`mcTree_RightIconClick` in `frmFX_MAIN_REZ` raised `AdaugaRevizie(CBool(cNode.Value2))`,
which landed in `FX_Adaugare_DDF`. Which of the two actions is chosen comes from the
reservation's `EInitiala`, as `cNode.Value2` did; the client does not second-guess it,
because the server refuses each of the two when the document's state contradicts it and that
refusal reaches the operator in Romanian.

`DdfView`'s context menu still has no «add» entry, deliberately: one trigger, in the one
place the operator already knows. The commands had no caller only for as long as slice 0051
itself lasted (decision D20).

The seam this replaced was `RezervariView`'s dormant `AdaugaDdfCerut` event from slice 0014.
It is gone; the view now takes the same optional `Action(Of DdfComanda)` that `DdfView` and
`PlatiView` take, so a host that supplies none (the tests) still gets the full read-only view
and the `+` icon says so rather than doing nothing.

### 4.4 The rich-text editor

`KBotRichTextEditor` (bold / italic / underline / text colour / background colour / font
family / font size over a `RichTextBox`) is new in `KBot.Controls/RichText/`. Its attachment
buttons were **not** ported: the «Fișiere» page owns attachments now, and a second half-wired
attachment path would be worse than none.

`KBotTextField` re-raises `TextChanged` but **not** `Leave`. Anywhere the editor needs a
`Leave`, it is hooked on `InnerTextBox`. This has bitten before.

### 4.5 The grid columns moved into the designers

`DdfEditSectiuneaAPage`, `DdfEditSectiuneaBPage` and `DdfFileBrowser` built their
`KBotDataView` columns in code, from a `ConstruiesteColoanele` / `BuildColumns` called in the
constructor. All three now declare them in their `.Designer.vb`, as
`KBotDataColumn` instances added to `grd.Columns` — the same shape the Visual Studio designer
emits (`docs/kbot-forms-ui-convention.md`, and the pattern already in `DdfValoriPage`). The
`COL_*` constants stay in the code-behind: they are the keys the cells are written through and
must match the designer's, which is now the only place a column is defined. `FooterVisible`
moved into the designer with them; `DdfFileBrowser` gained the `BeginInit` / `EndInit` pair it
was missing, so the layout pass runs once rather than once per column.

### 4.6 Section A is READ-ONLY for a document generated from reservations

The operator's rule, and it narrows decision D8: section A is still the only grid that can be
edited at all, but **a DDF built from `FX_Rezervari` is not edited by hand**. Its lines are
what the reservations made them, and the post-save `FX_Rezervari` update writes back against
those same reservations, so a retyped value would put the document and its reservations in a
position to disagree. The grid unlocks only for a MANUALLY BUILT document — a path that does
not exist yet.

How it is decided (`DdfDraft.DinRezervari`, in the domain so the client and any later caller
read one copy of the rule):

* `sursa` from the server — `_alege_sursa`'s answer, decision D5 — is now carried into the
  domain instead of being dropped at the wire. `"rezervari"` locks the grid.
* That alone is not enough: `GET /draft/{iddf}/{idrev}` answers `"existent"` for EVERY
  revision read back, whatever it was generated from, so a REOPENED document would have looked
  editable. What survives the round trip is `GrpIdrz`, the `IDRZ` list a line was generated
  from, empty on a line that came from `FX_Istoric` or that the operator added. Any line
  carrying one locks the grid too.

What the lock does, in `DdfEditSectiuneaAPage.AplicaModulDeEditare`: `grd.ReadOnlyGrid`, both
buttons off, the Partener cell shut regardless of `PartAng`, the classification list not even
fetched (no round trip for a list nothing can use), and the classification column switched from
`Combo` to `Text` — a chevron that opens nothing is the same silent no-op as a button that does
nothing. That last one is why `AplicaModulDeEditare` runs BEFORE the fill: a column's type only
moves while the grid has no rows.

### 4.7 `ButtonImageLayout` — one property for the toolbar's pictures

The toolbar buttons are 30x30, and the icon set the operator binds is whatever size it is: a
`Button` draws its `Image` at the picture's own pixel size and nothing else, so a 16x16 icon
left a ring of background inside the button and a 64x64 one was cropped by its edges. The
editor now publishes **one** property for the whole band — `KBotRichTextEditor.ButtonImageLayout`,
category «K-BOT Icons», default `Original` — taking `RichTextImageLayout`: `Original`,
`Stretch`, `Zoom`, `Tile`. One and not six, because the operator binds one icon set; a toolbar
whose icons were fitted six different ways would read as broken.

`Original` changes nothing at all: the picture goes to `MyBase.Image` and the framework draws
it, so the old look survives to the pixel. The other three are painted by `KBotNoFocusButton`
itself, in its `OnPaint`, after the base class has laid down the flat background, the border
and the text — there is no hook that skips only the image step, so the button keeps the
picture in a field of its own and hands the base class `Nothing` while it is doing the drawing.
That is why `Image` is shadowed there (`Browsable(False)` + serialization hidden, the house
rule for a shadowed member): one storage, two drawing paths, and `btn.Image = picture` in
`ApplyIcon` reads exactly as it did.

Three details that are not obvious:

* The picture is fitted inside what the flat border and `ButtonPadding` leave, not inside the
  whole button — otherwise a `Stretch` would slide under its own border.
* A disabled toolbar (`Editabil = False`, a read-only description) greys the picture through an
  `ImageAttributes` colour matrix. `ControlPaint.DrawImageDisabled` draws at natural size only,
  which is no use once the point is that the size changed.
* `Tile` shifts the brush to the content box's origin; without it the pattern starts at the
  CONTROL's origin and the first tile is cut by the border instead of sitting against it.

The lettered glyphs (B / I / U / A / ▨ / ▴ / ▾) are TEXT and are untouched: with no picture
bound there is nothing to lay out.

Files: `RichTextImageLayout.vb` (new), `KBotNoFocusButton.vb`, `KBotRichTextEditor.Properties.vb`,
`KBotRichTextEditor.md`, `KBot.Controls.vbproj` (the folder comment), and
`tests/KBot.Controls.Tests/RichTextButtonImageLayoutTests.vb` (new).

Verified: `dotnet build KBot.sln` — 0 errors, same 7 resx warnings as before. Controls tests
**960 passed, 0 failed** (953 before + the 7 new). Screen check with `DrawToBitmap` on a 12x12
and a 64x64 icon, all four layouts plus the disabled band: `Original` crops the big icon,
`Zoom` fits it, `Stretch` fills the button, `Tile` repeats, disabled greys. `ButtonImageLayout`
writes no designer line while it sits on `Original`.

---

### 4.8 The icons that never arrived, and the header that grew

Two things reported from a running `DdfEditDescrierePage`: the toolbar drew the letters
B / I / U although icons were bound in the designer, and the header band was taller than the
number typed into `HeaderHeight`. They have nothing to do with each other. One was a bug; the
other is the house rule doing what it says.

**The letters — a bug, now fixed.** A generated `.Designer.vb` writes in this order:

```
il_rtb = New ImageList(components)          ' empty
edtLunga.BoldImageKey = "bold"              ' properties, ALPHABETICALLY
edtLunga.Images = il_rtb                    ' the list is STILL empty here
...
il_rtb.ImageStream = resources.GetObject(...)   ' the pictures, further down
il_rtb.Images.SetKeyName(2, "bold")             ' and the key names, last
```

The editor resolved a key the moment it was written and never again, so every button fell back
to its lettered glyph — on a page whose designer surface showed the icons perfectly, because
there the property grid re-applies each value after the file has been read. Reproduced exactly
in a scratch WinForms exe before touching anything: all six buttons `image=NOTHING`, and the
same code with the list filled BEFORE it is bound gave `image=ok` on all of them.

The fix resolves the keys again in `OnHandleCreated`, by which time `InitializeComponent` has
run to its end. The editor also listens to the bound list's `RecreateHandle` (raised when the
contents are replaced wholesale), and `RefreshButtonIcons()` is now public for a host that adds
pictures to an already-bound list while the form is on screen — `ImageList` raises nothing for
that. Binding order no longer matters.

`OnHandleCreated` and not `OnCreateControl`: `CreateControl` is a no-op while the form is not
visible, so a control rendered with `DrawToBitmap` — every visual test and the bench itself —
would never have re-resolved.

Resolving more than once made an existing leak matter: `ImageList.Images(i)` returns a NEW
bitmap on every read, so each pass left one orphaned bitmap per button. `KBotNoFocusButton`
now takes the picture through `SetPicture(picture, owned)` and frees the previous one only when
it owned it. A picture assigned through `BoldImage` and its siblings belongs to the host and is
never touched; disposing the bound list drops `Images` to `Nothing` and puts the letters back.

**The header — not a bug.** `HeaderHeight` is a LOGICAL number at 96 dpi (rule C2), scaled once
at layout time through `AppScaling`. The page asks for 40; this machine runs at 144 dpi, so the
band is 60 px, and the designer surface — where the scale is always 1 — showed 40. Every other
metric of the control behaves the same way. Three ways out, all the operator's call: type the
number the 100 % form would want (40 / 1,5 ≈ 27), switch `AppScaling.Mode` to `Fixed100`, or
leave it and accept that the band follows the screen. Nothing was changed in the control.

**The bench.** `KBot.DevHarness\Internal\RichTextPlaygroundForm` (Controls/UI, safe), reached
from the harness as «KBotRichTextEditor — proprietăți runtime (playground)». The editor on the
right; on the left a `PropertyGrid` bound to it, which IS the complete list of editable
properties — categories, descriptions, the `RichTextImageKeyConverter` drop-downs and the
`ShouldSerialize` behaviour all come along, and nothing can fall behind a property added later.
Above it, only what a property grid cannot give: which `ImageList` is bound (none / 16 / 24 /
64 px / mixed, drawn in code), in WHICH ORDER it is bound (the `.Designer.vb` order is one of
the two), `ButtonImageLayout`, `Editabil`, `Collapsed`, and the application scale.

Under the editor, the line that answers both reports at once: the scale factor and `DeviceDpi`,
`HeaderHeight` logical next to the band's real height in pixels, `ButtonSize`, and — button by
button — whether a picture landed on it (`[imagine 64×64 în 30×29]`) or the fallback letter did
(`[literă B]`). Theme and scale are persisted operator settings, so the bench puts both back on
close.

Files: `KBotRichTextEditor.vb`, `KBotRichTextEditor.Properties.vb`, `KBotNoFocusButton.vb`,
`KBotRichTextEditor.md`, `KBot.Controls.vbproj` (FileVersion 1.42 → 1.43),
`src/KBot.DevHarness/Internal/RichTextPlaygroundForm.{vb,Designer.vb}` (new),
`src/KBot.DevHarness/Tests/KBotRichTextEditorPlaygroundTest.vb` (new),
`KBot.DevHarness.vbproj` (FileVersion 1.0.22 → 1.0.23), and
`tests/KBot.Controls.Tests/RichTextIconResolutionTests.vb` (new).

Verified: `dotnet build KBot.sln` — 0 errors, same 7 resx warnings. Controls tests **964 passed,
0 failed** (960 + 4 new). The scratch repro goes from six `image=NOTHING` to six `image=ok`
with the same designer-order code. The bench itself rendered with `DrawToBitmap` in four
states — 24 px / Original, 64 px / Zoom, the `.Designer.vb` binding order, and no icons at all
— and the measurement line matched what the toolbar showed in each.

---

## 5. Verification actually performed

| Check | Result |
|---|---|
| `dotnet build KBot.sln --no-incremental` | **0 errors, 12 warnings** — identical to the baseline taken from a clean `git worktree` |
| Python suite (`PYTHON/.venv`, `pytest -q`) | **465 passed, 15 skipped** — identical before and after |
| D19 diff (`routes/ddf/`, `routes/forexe/ddf.py`) | empty |
| `sql/0049_ord_att_img.sql` | untouched |
| Flask route map from a bare `Flask()` | no shadowing, no collision |
| Diacritics, new files | 259 lines carry diacritics, **0** outside a string literal |
| Diacritics, added lines in changed files | 54 carry diacritics, **0** outside a string literal |
| New tests | **none for the original pass**, per plan §0.4. §4.5 / §4.6 added `DdfEditGrileTests` — 6 tests, all passing. §4.7 added `RichTextButtonImageLayoutTests` — 7 tests, all passing; §4.8 added `RichTextIconResolutionTests` — 4 tests, all passing |

The Python baseline could not be taken from a clean worktree: `config.py` and
`routes/auth/auth.py` are gitignored, so a worktree checkout cannot start the app. It was
taken instead by temporarily removing this slice's two Python changes from the working tree,
running the suite, and restoring them. Both routes gave the same numbers.

The four affected .NET test projects have a **pre-existing** failure set, measured from a
clean worktree at `be688cd` before any slice-0051 work:

```
Api        Failed:  1, Passed:  95, Total:  96
App        Failed: 13, Passed: 188, Total: 201
Domain     Failed:  3, Passed:  14, Total:  17
Controls   Failed:  0, Passed: 953, Total: 953
```

The matching run on the working tree, after §4.5 / §4.6:

```
Api        Failed:  1, Passed:  95, Total:  96      same failure, same test
App        Failed: 13, Passed: 194, Total: 207      same 13, +6 new passing
Domain     Failed:  3, Passed:  14, Total:  17      same 3
Controls   Failed:  0, Passed: 964, Total: 964      same 0, +7 (§4.7) +4 (§4.8) new passing
```

Every failure is one of the pre-existing set above, and none of them touches `DdfDraft`,
`ApiClient`'s DDF mapping, or the three pages: they are about nav captions, XFA parsing, and
the `DdfView` / `IstoricView` trees.

### 5.1 A RULE 0 sweep that was reverted

While auditing diacritics, an over-broad pass de-accented ~400 **pre-existing** comment lines
in `MainForm.vb` and `DdfView.vb` that belong to earlier slices. Those were restored: this
slice's diff on those two files contains only this slice's lines. RULE 0 does say old code
that disagrees gets swept, but a 400-line sweep of unrelated comments does not belong in this
commit. It is listed in `possible_future_directions.md` as its own job.

---

## 6. What is NOT verified

Everything that needs a running system:

* **No SQL statement in this slice has ever executed.** They were written from a
  structure-only dump with no sample rows.
* The two `sql/0051_*.sql` files have not been applied to any database. Until
  `FX_DDF_REV_ATT_IMG` exists, every attachment route fails.
* No route has served a request. No response shape has been observed on the wire.
* `DdfEditForm` has never been shown on screen. Layout, DPI behaviour, theming and tab order
  are unverified by eye. **That now includes the `+` trigger on the reservations tree: the
  click path was never exercised against a server, so neither `POST /genereaza` nor the
  editor it opens has been seen to work from there.** The three grids of §4.5 / §4.6 ARE the
  exception: they were rendered off-screen with `DrawToBitmap` on a themed host form and
  looked at — headers, alignment, the two-decimal money format, the footer sums, the greyed
  buttons and the status line of a locked section A, and the chevron present on an editable
  classification column and absent on a locked one. That is the columns and the lock; it is
  not the form around them.
* `KBotRichTextEditor` has never been rendered.
* The number lock has never been contended by two clients.
* No document produced by this slice has been rendered as a signed XFA.

---

## 7. Known gaps

* ~~`RezervariView` does not refresh after a save.~~ **Closed.** It has a `Reincarca()` that
  goes through the same `LoadAsync` a normal selection uses, and `DupaScriereaDdf` calls it.
  Only the ACTIVE view is refreshed — `ActivateView` calls `SetContext` on every activation,
  so an inactive one reloads by itself when the operator switches to it.
* **`IDdfForexeUpload`** (plan §10.4) exists as an interface with one method and a
  `NotImplementedException` implementation. The FOREXE portal run belongs to a later slice;
  the seam exists so it is not retro-fitted into the save handler.
* **`KBotComboBox` cannot be editable** — see §2.2.
* The nine `IApiClient` test doubles have stubbed the new members, not covered them.

---

## 8. Deployment order

1. Apply `sql/0051_fx_numberlock.sql`.
2. Apply `sql/0051_ddf_rev_att_img.sql`.
3. Deploy the Python module (`routes/forexe/ddf_edit.py` plus the one-line registration).
4. Deploy the client.

Steps 1 and 2 must come first: without `FX_NumberLock` no document can be numbered, and
without `FX_DDF_REV_ATT_IMG` no attachment can be stored.

## 9. Read this before the first live run

The first live run is the first time any of this SQL executes. Run it on a copy, with one
document, and check: the CUAL and revision numbers allocated; that `Desc_Lunga` and
`Desc_Lunga_ANSI` both carry the text; that section B was written from section A; that a
deleted revision released exactly its own reservations; and that a deleted document released
all of them.
