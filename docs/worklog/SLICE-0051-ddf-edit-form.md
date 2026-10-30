# SLICE-0051 / 03 — Istoricul editorului DDF și al alinierii la ORD

Reunire documentară D001-27. Implementarea inițială și corecțiile 03 sunt păstrate pe etapă. Contractul general al combo-ului editabil rămâne separat în [0051-02](SLICE-0051-02-combo-editabil.md): el înlocuiește premisa inițială că nu se poate tema EDIT-ul; 03 leagă apoi textul compartimentului de salvare și corectează aspectul grilelor/butoanelor. Mențiunile vechi «RichTextEditor niciodată randat» preced probele statice din §4.7/4.8 ale etapei inițiale; formularul DDF și sistemul live nu sunt declarate văzute. Cheile și căile inițiale sunt istorice, anterioare [0080](SLICE-0080-01-idclsf-pereche-datadoc.md) și [0081](SLICE-0081-01-starea-reviziei.md).

[0051-ddf](#slice-0051-baza) · [0051-03](#slice-0051-03)

<a id="slice-0051-baza"></a>

## SLICE-0051 — `DdfEditForm`, the DDF write path

**State: code complete, builds clean, unverified against a live system.**

Language note: this worklog is in English, per plan §0.3 (this slice is English-only in comments and documentation, unlike slice 0049 which shipped Romanian-without-diacritics). Operator-facing strings are Romanian with real UTF-8 diacritics.

**No MariaDB/Python-server execution:** neither was reachable. SQL comes from `MariaDB_Schema/000_DEMO.sql`, a structure-only dump with no `FX_DDF*` INSERT rows to sample. Deployment limits are in §9.

---

### 1. What was ported

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

#### Files added

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

#### Files changed

`PYTHON/routes/forexe/__init__.py` (registration only), `src/KBot.Api/ApiClient.vb`, `src/KBot.Api/IApiClient.vb`, `src/KBot.App/MainForm.vb`, `src/KBot.App/Views/DdfView.vb`, `src/KBot.App/Views/Ddf/DdfFileBrowser.vb + .Designer.vb` (§4.5), and nine files under `tests/KBot.App.Tests/` — the last of these are **test doubles that implement `IApiClient`**. They gained sixteen `Throw New NotSupportedException()` stubs so the interface still compiles. That is compilation bookkeeping, **not** coverage: no test exercises any of the new members.

**D19 held.** `git diff -- PYTHON/routes/ddf/ PYTHON/routes/forexe/ddf.py` is empty, and so is the diff on `sql/0049_ord_att_img.sql`.

---

### 2. The four decisions the operator made

Three §13 stop-and-report conditions were genuine stops, and two further defects were found in the plan that were not on the §13 list. All were put to the operator; all four answers are recorded here because the code cannot be read without them.

#### 2.1 `Desc_Lunga_ANSI` is not dead — D9 is reversed

D9 incorrectly declared `Desc_Lunga_ANSI` dead: `PYTHON/routes/forexe/ddf.py` emits it as `desc_lunga`, `ApiClient` maps `DescLunga`, and `DdfXmlBuilder` writes `DescrieObFundRevizuireLung` plus `Left(…, 500)` into section-A `obiect_fd_reviz_lung`. Writing only `Desc_Lunga` would leave the generated long description empty; D19 forbids changing that read route.

**Operator's answer: write both columns.** Both `Desc_Lunga` and `Desc_Lunga_ANSI` receive the same text; this deliberate duplication preserves the signed-document read path outside this slice.

#### 2.2 `Comp` — read the existing values, but let the operator type a new one

The plan gave no source for the `Comp` field. **Operator's answer:** list the values already used on earlier DDFs; if there are none the list is empty; the operator may always type a value that is not in the list.

Server side that is `SELECT DISTINCT Comp FROM FX_DDF` (`GET /api/forexe/ddf/comp`).

The initial workaround uses authoritative `txtComp` (`KBotTextField`) plus picker `cmbComp`, documented in `DdfEditForm.Designer.vb`. At this checkpoint `KBotComboBox` throws unless `DropDownStyle = DropDownList`: owner drawing cannot theme native `EDIT`. Editable-combo work is listed separately in `possible_future_directions.md`.

#### 2.3 `GenerateUniqueSequence` — the VBA source, ported

The plan referred to a function whose source was not in the export. The operator supplied the original VBA, with its attribution:

```
' Source - https://stackoverflow.com/a/44681287
' Posted by Séb Cô, modified by community. See post 'Timeline' for change history
' Retrieved 2026-02-11, License - CC BY-SA 4.0
```

It is ported in `DdfDraft.vb` as `CaracterAleator` / `GenereazaUnic`, attribution kept.

The original comment incorrectly gives each character class 1/3 probability: banker rounding in `Round(Rnd * 2 + 1)` yields 1/4 digits, 1/2 uppercase, 1/4 lowercase. Behaviour is ported unchanged, comment corrected. `GenereazaUnic` retries up to twenty times against draft codes, then throws rather than returning a duplicate that would collide on save.

#### 2.4 `FX_DDF_REV_ATT_IMG` is missing from the schema — DDL delivered

The attachment-bytes table the plan assumes does not exist in `000_DEMO.sql`. The operator confirmed it is missing from the real schema and asked for DDL, keeping the unique index.

Delivered as `sql/0051_ddf_rev_att_img.sql`, shaped exactly like the ORD equivalent (`FX_ORD_ATT_IMG`) including the `TipMime` column and `UNIQUE INDEX UQ_FX_DDF_REV_ATT_IMG_ATT(IdRevAtt)`, with `FOREIGN KEY (IdRevAtt) REFERENCES FX_DDF_REV_ATT(IdRevAtt) ON DELETE CASCADE ON UPDATE RESTRICT`. The file also carries probe statements and a commented `AVACONT_SURSA` variant (that database does not use `AUTO_INCREMENT`). `sql/0051_fx_numberlock.sql` follows the same pattern for `FX_NumberLock`.

---

### 3. The server module

`PYTHON/routes/forexe/ddf_edit.py`, sixteen routes, every one `@require_session` (bearer — this is a K-BOT client, not the VBA/FOREXE legacy path). The database name comes from `g.session.db_name`; no route accepts one from the caller. Every response is dumped with `ensure_ascii=False`; every refusal message is Romanian with diacritics.

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

The route map was dumped from a bare `Flask()` to prove three things: `GET /api/forexe/ddf` (slice 0020) still resolves, `DELETE /api/forexe/ddf/<int:iddf>` does not shadow it, and nothing collides with `/api/forexe/ddf/pdf/<int:idrev>`.

#### 3.1 The classification key inversion

This trips every DDF query and is written down again because getting it wrong silently returns the wrong rows:

* `FX_DDF_REV_SA.IdClsf` / `FX_DDF_REV_SB.IdClsf` = the **MariaDB** primary key `Clasificatii.IDClsf`.
* `FX_Indicatori.IdClsf`, `FX_Rezervari.IdClsf`, `FX_Receptii.IdClsf` = the **Access** id, which matches `Clasificatii.IdClsfAcc`.

So the generation queries join `Clasificatii C ON C.IdClsfAcc = I.IdClsf AND C.IdUnitate = I.IdUnitate`, while the draft reads join on `C.IDClsf`.

`Clasificatii` has **no** `CodSSI` column. It is composed: `CONCAT(C.SS, C.ClsfSal) AS CodSSI`.

#### 3.2 Deliberate divergences from Access

Each of these is a place where the port does **not** do what the Access original did. They are choices, not accidents.

1. **Validation checks every row.** `msgEroare` in Access examined only the first row of each section before returning. The port checks all of them and lists every reason at once.
2. **Deleting a document releases every revision's reservations.** The Access `FX_Stergere_DDF` released only the reservations of one revision, leaving the rest of the document's reservations marked as used against a document that no longer existed. The port releases all of them. This is a bug fix, and if the old behaviour was load-bearing somewhere it will show up here first.
3. **The source of the generated lines is chosen from the data** (decision D5) rather than from a form's state as Access did. `_alege_sursa()` looks at what actually exists — reservations or indicators — and **refuses loudly, naming both conditions**, when it cannot tell. It never guesses.
4. **The `IdUnitate` predicate is kept** in the generation queries. Access omitted it in places; `Clasificatii` and the other nomenclature tables are unit-scoped, so omitting it returns another unit's rows on a multi-unit database.
5. **The `PrtScr` filter was not ported.** It filtered the Access screen; it has no meaning in the K-BOT editor.
6. **Section B copies the partner from section A.** Access wrote `NULL` there. The B rows are derived from the A rows, and a derived row with no partner cannot be reconciled afterwards.
7. **The earliest-date, lowest-operation-type selection rule** in `_SQL_GEN_REZERVARI` is in the Access query and was omitted from the plan; it is ported, because dropping it changes which reservation row wins.

#### 3.3 Writing

`_scrie_graf` runs nine writes in one transaction. Negative `TempId` values map to real keys in the response, avoiding a reread. `_cheie_noua` rejects `lastrowid = 0`; `_sterge_absentii` deletes removed rows. After graph writes, `FX_Rezervari` / `FX_Angajamente` text updates truncate explicitly to 255, avoiding strict-mode rollback for long descriptions.

Attachments are a **second phase**, after the transaction: the bytes need an `IdRevAtt` to hang off. The PUT carries the file name in the `X-Nume-Fisier` header (same as ORD) and uses `ON DUPLICATE KEY UPDATE`, which is what the unique index on `IdRevAtt` is for.

Phase-two failure rolls back nothing; the operator receives the failed-file list and must reattach those files.

#### 3.4 The number lock

`FX_NumberLock`, TTL 60 minutes, heartbeat every 5. Allocation is `GREATEST(<max in use>, <max locked>) + 1`, with the floor at 0 for CUAL and **-1** for `NumarRev` so that the first revision of a document is numbered 0. Expired locks are swept on every reserve. The lock is consumed inside the save transaction (`_consuma_lacatul`), so a form that saved no longer holds it and does not try to release it on the way out.

This is deliberately unlike slice 0049, which guessed the next number ("probabil N") and hoped.

---

### 4. The client

#### 4.1 Shape

`DdfDraft` (in `KBot.Domain`) is the whole editable graph: header, `LiniiA`, `LiniiB`, `Atasamente`. `RecalculeazaSectiuneaB()` derives B from A and is called immediately before validation on save, not merely when a cell changes — a stale B row would otherwise be written.

The four `IDdfEditPage` pages make no network requests. Access `cmbClsf_AfterUpdate` lookups (previous value, reception value, indicator code) are precomputed sub-selects in `GET /api/forexe/ddf/clasificatii`, read from the loaded row.

The classification list is the three-part list Access built (on this angajament / same title / manual), with a separator row carrying `IDClsf = -1` inserted in Python.

#### 4.2 Re-authentication

`DdfEditReauth` receives seven named delegates wrapping the private generic `MainForm.WithReauth` 401 policy, keeping re-login policy in one place as for other views.

#### 4.3 Where the commands come from

`DdfView` receives optional `executaComanda As Action(Of DdfComanda)`. Its context menu runs after selection in existing `Tree_NodeMouseUp`, so menu failure preserves the read path. Without an action (test hosts), the full read-only view remains and the menu explains the limitation.

`MainForm.ExecutaComandaDdf` executes them.

**`Adauga` / `AdaugaRevizieInitiala` originate at the RESERVATIONS leaf `+`.** `RezervariView.Tree_RightIconClicked` passes the command to `MainForm.ExecutaComandaDdf`, matching Access `frmFX_MAIN_REZ.mcTree_RightIconClick` → `AdaugaRevizie(CBool(cNode.Value2))` → `FX_Adaugare_DDF`. Reservation `EInitiala` chooses the action, as `cNode.Value2` did; the server rejects either action when document state contradicts it, with Romanian feedback.

`DdfView`'s context menu still has no «add» entry, deliberately: one trigger, in the one place the operator already knows. The commands had no caller only for as long as slice 0051 itself lasted (decision D20).

This replaces dormant `RezervariView.AdaugaDdfCerut` (0014) with the same optional `Action(Of DdfComanda)` seam as `DdfView` / `PlatiView`. Without a delegate, read-only use remains and `+` explains the limitation.

#### 4.4 The rich-text editor

`KBotRichTextEditor` (bold / italic / underline / text colour / background colour / font family / font size over a `RichTextBox`) is new in `KBot.Controls/RichText/`. Its attachment buttons were **not** ported: the «Fișiere» page owns attachments now, and a second half-wired attachment path would be worse than none.

`KBotTextField` re-raises `TextChanged` but **not** `Leave`. Anywhere the editor needs a `Leave`, it is hooked on `InnerTextBox`. This has bitten before.

#### 4.5 The grid columns moved into the designers

`DdfEditSectiuneaAPage`, `DdfEditSectiuneaBPage` and `DdfFileBrowser` move `KBotDataColumn` construction from constructor `ConstruiesteColoanele` / `BuildColumns` into `.Designer.vb`, added to `grd.Columns` as in `DdfValoriPage` and `docs/kbot-forms-ui-convention.md`. Code-behind `COL_*` keys must match these sole definitions. `FooterVisible` moves too; the missing `DdfFileBrowser.BeginInit` / `EndInit` pair reduces layout to once, rather than per column.

#### 4.6 Section A is READ-ONLY for a document generated from reservations

Operator rule narrowing D8: only section A can be editable, but a DDF generated from `FX_Rezervari` locks it. Retyping would disagree with the reservations updated after save. Only a MANUALLY BUILT document can unlock it; that path does not yet exist.

How it is decided (`DdfDraft.DinRezervari`, in the domain so the client and any later caller read one copy of the rule):

* `sursa` from the server — `_alege_sursa`'s answer, decision D5 — is now carried into the domain instead of being dropped at the wire. `"rezervari"` locks the grid.
* That alone is not enough: `GET /draft/{iddf}/{idrev}` answers `"existent"` for EVERY revision read back, whatever it was generated from, so a REOPENED document would have looked editable. What survives the round trip is `GrpIdrz`, the `IDRZ` list a line was generated from, empty on a line that came from `FX_Istoric` or that the operator added. Any line carrying one locks the grid too.

`DdfEditSectiuneaAPage.AplicaModulDeEditare` sets `grd.ReadOnlyGrid`, disables both buttons, closes Partener regardless of `PartAng`, skips the unusable classification fetch, and changes that column from `Combo` to `Text`. It runs BEFORE filling, since column type changes require an empty grid.

#### 4.7 `ButtonImageLayout` — one property for the toolbar's pictures

Toolbar buttons are 30x30; native `Button.Image` uses natural size, leaving space around 16x16 icons and cropping 64x64. One toolbar-wide `KBotRichTextEditor.ButtonImageLayout` («K-BOT Icons», default `Original`) takes `RichTextImageLayout`: `Original`, `Stretch`, `Zoom`, `Tile`, fitting the single operator-bound icon set consistently.

`Original` passes the picture to `MyBase.Image`, preserving framework rendering. For `Stretch` / `Zoom` / `Tile`, `KBotNoFocusButton.OnPaint` draws after the base flat background, border and text; no image-only skip hook exists, so the base receives `Nothing`. Shadowed `Image` (`Browsable(False)`, serialization hidden) stores one picture for both paths; `ApplyIcon` still assigns `btn.Image`.

Three details that are not obvious:

* The picture is fitted inside what the flat border and `ButtonPadding` leave, not inside the whole button — otherwise a `Stretch` would slide under its own border.
* A disabled toolbar (`Editabil = False`, a read-only description) greys the picture through an `ImageAttributes` colour matrix. `ControlPaint.DrawImageDisabled` draws at natural size only, which is no use once the point is that the size changed.
* `Tile` shifts the brush to the content box's origin; without it the pattern starts at the CONTROL's origin and the first tile is cut by the border instead of sitting against it.

The lettered glyphs (B / I / U / A / ▨ / ▴ / ▾) are TEXT and are untouched: with no picture bound there is nothing to lay out.

Files: `RichTextImageLayout.vb` (new), `KBotNoFocusButton.vb`, `KBotRichTextEditor.Properties.vb`, `KBotRichTextEditor.md`, `KBot.Controls.vbproj` (the folder comment), and `tests/KBot.Controls.Tests/RichTextButtonImageLayoutTests.vb` (new).

Verified: `dotnet build KBot.sln` — 0 errors, same 7 resx warnings as before. Controls tests **960 passed, 0 failed** (953 before + the 7 new). Screen check with `DrawToBitmap` on a 12x12 and a 64x64 icon, all four layouts plus the disabled band: `Original` crops the big icon, `Zoom` fits it, `Stretch` fills the button, `Tile` repeats, disabled greys. `ButtonImageLayout` writes no designer line while it sits on `Original`.

---

#### 4.8 The icons that never arrived, and the header that grew

Two things reported from a running `DdfEditDescrierePage`: the toolbar drew the letters B / I / U although icons were bound in the designer, and the header band was taller than the number typed into `HeaderHeight`. They have nothing to do with each other. One was a bug; the other is the house rule doing what it says.

**The letters — a bug, now fixed.** A generated `.Designer.vb` writes in this order:

```
il_rtb = New ImageList(components)          ' empty
edtLunga.BoldImageKey = "bold"              ' properties, ALPHABETICALLY
edtLunga.Images = il_rtb                    ' the list is STILL empty here
...
il_rtb.ImageStream = resources.GetObject(...)   ' the pictures, further down
il_rtb.Images.SetKeyName(2, "bold")             ' and the key names, last
```

Keys were resolved only when assigned; the designer order above therefore left letter glyphs at runtime. The designer surface worked because its property grid reapplies values after loading. A scratch WinForms reproduction confirmed six `image=NOTHING`; filling the list BEFORE binding gave six `image=ok`.

The fix resolves keys again in `OnHandleCreated`, after `InitializeComponent`, and on bound-list `RecreateHandle` (wholesale replacement). Public `RefreshButtonIcons()` handles pictures added to a bound list on screen, for which `ImageList` raises no event. Binding order no longer matters.

`OnHandleCreated` and not `OnCreateControl`: `CreateControl` is a no-op while the form is not visible, so a control rendered with `DrawToBitmap` — every visual test and the bench itself — would never have re-resolved.

`ImageList.Images(i)` returns a NEW bitmap per read. `KBotNoFocusButton.SetPicture(picture, owned)` now frees a previous picture only if owned, preventing repeated resolution leaks. Host-assigned `BoldImage` and sibling pictures remain host-owned; disposing the list clears `Images` and restores letters.

**The header — not a bug.** C2 defines `HeaderHeight` at 96 dpi, scaled once by `AppScaling`: requested 40 becomes 60 px at 144 dpi, while designer scale 1 shows 40. All control metrics follow this rule. Operator choices: author the 100% value (`40 / 1,5 ≈ 27`), choose `AppScaling.Mode = Fixed100`, or accept screen scaling. No control change.

**The bench.** `KBot.DevHarness\Internal\RichTextPlaygroundForm` (Controls/UI, safe), reached from the harness as «KBotRichTextEditor — proprietăți runtime (playground)». The editor on the right; on the left a `PropertyGrid` bound to it, which IS the complete list of editable properties — categories, descriptions, the `RichTextImageKeyConverter` drop-downs and the `ShouldSerialize` behaviour all come along, and nothing can fall behind a property added later. Above it, only what a property grid cannot give: which `ImageList` is bound (none / 16 / 24 / 64 px / mixed, drawn in code), in WHICH ORDER it is bound (the `.Designer.vb` order is one of the two), `ButtonImageLayout`, `Editabil`, `Collapsed`, and the application scale.

Under the editor, the line that answers both reports at once: the scale factor and `DeviceDpi`, `HeaderHeight` logical next to the band's real height in pixels, `ButtonSize`, and — button by button — whether a picture landed on it (`[imagine 64×64 în 30×29]`) or the fallback letter did (`[literă B]`). Theme and scale are persisted operator settings, so the bench puts both back on close.

Files: `KBotRichTextEditor.vb`, `KBotRichTextEditor.Properties.vb`, `KBotNoFocusButton.vb`, `KBotRichTextEditor.md`, `KBot.Controls.vbproj` (FileVersion 1.42 → 1.43), `src/KBot.DevHarness/Internal/RichTextPlaygroundForm.{vb,Designer.vb}` (new), `src/KBot.DevHarness/Tests/KBotRichTextEditorPlaygroundTest.vb` (new), `KBot.DevHarness.vbproj` (FileVersion 1.0.22 → 1.0.23), and `tests/KBot.Controls.Tests/RichTextIconResolutionTests.vb` (new).

Verified: `dotnet build KBot.sln` — 0 errors, same 7 resx warnings. Controls tests **964 passed, 0 failed** (960 + 4 new). The scratch repro goes from six `image=NOTHING` to six `image=ok` with the same designer-order code. The bench itself rendered with `DrawToBitmap` in four states — 24 px / Original, 64 px / Zoom, the `.Designer.vb` binding order, and no icons at all — and the measurement line matched what the toolbar showed in each.

---

### 5. Verification actually performed

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

The Python baseline could not be taken from a clean worktree: `config.py` and `routes/auth/auth.py` are gitignored, so a worktree checkout cannot start the app. It was taken instead by temporarily removing this slice's two Python changes from the working tree, running the suite, and restoring them. Both routes gave the same numbers.

The four affected .NET test projects have a **pre-existing** failure set, measured from a clean worktree at `be688cd` before any slice-0051 work:

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

Every failure is one of the pre-existing set above, and none of them touches `DdfDraft`, `ApiClient`'s DDF mapping, or the three pages: they are about nav captions, XFA parsing, and the `DdfView` / `IstoricView` trees.

#### 5.1 A RULE 0 sweep that was reverted

While auditing diacritics, an over-broad pass de-accented ~400 **pre-existing** comment lines in `MainForm.vb` and `DdfView.vb` that belong to earlier slices. Those were restored: this slice's diff on those two files contains only this slice's lines. RULE 0 does say old code that disagrees gets swept, but a 400-line sweep of unrelated comments does not belong in this commit. It is listed in `possible_future_directions.md` as its own job.

---

### 6. What is NOT verified

Everything that needs a running system:

* **No SQL statement in this slice has ever executed.** They were written from a structure-only dump with no sample rows.
* The two `sql/0051_*.sql` files have not been applied to any database. Until `FX_DDF_REV_ATT_IMG` exists, every attachment route fails.
* No route has served a request. No response shape has been observed on the wire.
* `DdfEditForm` has never been shown on screen. Layout, DPI behaviour, theming and tab order are unverified by eye. **That now includes the `+` trigger on the reservations tree: the click path was never exercised against a server, so neither `POST /genereaza` nor the editor it opens has been seen to work from there.** The three grids of §4.5 / §4.6 ARE the exception: they were rendered off-screen with `DrawToBitmap` on a themed host form and looked at — headers, alignment, the two-decimal money format, the footer sums, the greyed buttons and the status line of a locked section A, and the chevron present on an editable classification column and absent on a locked one. That is the columns and the lock; it is not the form around them.
* `KBotRichTextEditor` has never been rendered.
* The number lock has never been contended by two clients.
* No document produced by this slice has been rendered as a signed XFA.

---

### 7. Known gaps

* ~~`RezervariView` does not refresh after a save.~~ **Closed.** It has a `Reincarca()` that goes through the same `LoadAsync` a normal selection uses, and `DupaScriereaDdf` calls it. Only the ACTIVE view is refreshed — `ActivateView` calls `SetContext` on every activation, so an inactive one reloads by itself when the operator switches to it.
* **`IDdfForexeUpload`** (plan §10.4) exists as an interface with one method and a `NotImplementedException` implementation. The FOREXE portal run belongs to a later slice; the seam exists so it is not retro-fitted into the save handler.
* **`KBotComboBox` cannot be editable** — see §2.2.
* The nine `IApiClient` test doubles have stubbed the new members, not covered them.

---

### 8. Deployment order

1. Apply `sql/0051_fx_numberlock.sql`.
2. Apply `sql/0051_ddf_rev_att_img.sql`.
3. Deploy the Python module (`routes/forexe/ddf_edit.py` plus the one-line registration).
4. Deploy the client.

Steps 1 and 2 must come first: without `FX_NumberLock` no document can be numbered, and without `FX_DDF_REV_ATT_IMG` no attachment can be stored.

### 9. Read this before the first live run

The first live run is the first time any of this SQL executes. Run it on a copy, with one document, and check: the CUAL and revision numbers allocated; that `Desc_Lunga` and `Desc_Lunga_ANSI` both carry the text; that section B was written from section A; that a deleted revision released exactly its own reservations; and that a deleted document released all of them.

---

<a id="slice-0051-03"></a>

## SLICE-0051-03 — `DdfEditForm` aliniat la `OrdEditForm`

Rundă corectivă la felia 0051, cerută de operator în trei puncte: (1) **toate butoanele trebuie să urmeze tema**; (2) **grilele secțiunilor A și B nu au voie să arate dezactivate** — un rând care nu se editează rămâne un rând care se citește normal, nu unul stins; (3) restul neconcordanțelor față de editorul de ordonanțare.

**Stare:** cod verde. `dotnet build src/KBot.App/KBot.App.vbproj` → **0 erori / 7 avertismente**, toate `MSB3825` PREEXISTENTE pe `.resx`-uri (`*.ImageStream` prin `BinaryFormatter`); niciunul nou. `dotnet test tests/KBot.App.Tests` → **194 trecute / 13 picate**, aceleași 13 înainte și după (§4).

**NIMIC N-A FOST VĂZUT PE ECRAN.** `DdfEditForm` nu s-a deschis, nici în aplicație, nici în designerul Visual Studio, nici prin `DrawToBitmap`. Ce e mai jos e citit din cod și din teste.

---

### 0. Ce s-a citit înainte de orice

`docs/worklog/CODE_WORKFLOW.md` · `docs/worklog/KBOT_STATUS.md` (rândurile 0051 și 0051-02) · `docs/worklog/SLICE-0051-02-combo-editabil.md` · `CLAUDE.md` · integral: `src/KBot.App/Views/Ord/OrdEditForm.{vb,Designer.vb}`, `OrdAtasamentePage.vb`, `OrdBeneficiariPage.Designer.vb`, `IOrdEditPage.vb` (referința) · `src/KBot.App/Views/Ddf/DdfEdit*.{vb,Designer.vb}`, `IDdfEditPage.vb` · `src/KBot.Theming/ButtonStyles.vb`, `ModernRenderer.vb`, `ThemeManager.vb` (regulile generice și `Traverse`) · `src/KBot.Controls/DataView/KBotDataColumn.vb`, `KBotDataView.vb`, `KBotDataView.Painting.vb` · `src/KBot.Controls/Combo/KBotComboBox.vb` · `src/KBot.Controls/RichText/KBotRichTextEditor.Theming.vb` · `tests/KBot.App.Tests/DdfEditGrileTests.vb`.

---

### 1. Butoanele

#### Ce era

Cele două pagini cu butoane (`DdfEditSectiuneaAPage`, `DdfEditFisierePage`) și formularul însuși își pictau butoanele cu mâna, dintr-o buclă care scria direct culorile din paletă:

```vb
btn.FlatStyle = FlatStyle.Flat
btn.BackColor = p.ButtonBackColor
...
```

Bucla ocolea ModernRenderer: pe Modern, ApplyPrimary/ApplySecondary folosesc ModernRenderer.ApplyButton pentru Region rotunjit/umplutură. DDF rămânea plat, fără accent, cu Adaugă/Șterge identice, spre deosebire de ORD și conform reclamației.

#### Ce e acum

| loc | buton | stil |
|---|---|---|
| `DdfEditSectiuneaAPage` | `btnAdauga` | `ButtonStyles.ApplyPrimary` |
| | `btnSterge` | `ApplySecondary` |
| `DdfEditFisierePage` | `btnAdauga` | `ApplyPrimary` |
| | `btnSterge`, `btnSalveazaPeDisc` | `ApplySecondary` |
| `DdfEditForm` (subsol) | `btnRenunta`, `btnSalveaza` | **nimic scris de mână** |

Aceeași repartiție ca în `OrdAtasamentePage` / `OrdDocumentePage`: acțiunea poartă accentul, iar butonul distructiv rămâne secundar.

Subsolul btnRenunta/btnSalveaza primea deja regula generică, inclusiv ModernRenderer, prin MyBase.OnThemeChanged → ThemeManager; repictarea elimina rotunjirea. DdfEditForm.OnThemeChanged nu mai repictează butoanele, ca OrdEditForm.

ThemeManager.Traverse nu aplică regulile generice copiilor paginilor IThemedControl (ThemeManager.vb:304–321); aplicarea explicită pe pagini rămâne necesară, explicată în ambele locuri.

### 2. Grilele nu mai arată dezactivate

`DdfEditSectiuneaAPage.Designer.vb` avea `Enabled = False` pe toate cele zece coloane. Proprietatea desenează coloana gri și o face inertă; nu exprimă doar interdicția editării.

Două urmări, nu una:

1. **Aspectul.** Exact ce a semnalat operatorul. Nicio grilă din `OrdEditForm` nu folosește `Enabled = False`; toate spun `ReadOnly = True`.
2. **Funcțional:** clasificația, elementul de fundamentare, parametrii, partenerul și valoarea curentă, declarate editabile, erau tot inerte în DDF manual. AplicaModulDeEditare nu le putea debloca: Enabled prevalează asupra ReadOnly (KBotDataColumn.vb:1005–1018), iar KBotDataView.EsteCelulaActiva iese întâi pe Not col.Enabled.

Toate cele zece linii au fost scoase. Refuzul editării rămâne unde îi e locul și unde era deja:

- pe coloană, `ReadOnly = True` pentru cele derivate (`clsf`, `buget`, `val_rec`, `val_prec`, `val_tot`) — neatins;
- pe grilă, `grd.ReadOnlyGrid` pentru un document generat din rezervări — neatins;
- pe celula `cod_partener`, poarta din `AplicaGateulPartenerului` — neatins, tot prin `ReadOnly`.

Niciunul dintre cele trei nu atinge pictura. Regula e scrisă acum în capul designerului, ca să nu se întoarcă.

`DdfEditSectiuneaBPage` **nu avea** `Enabled = False` pe nicio coloană — verificat, nu presupus. Ce avea în schimb e la §3.

### 3. Restul neconcordanțelor

#### 3.1 `cmbComp` — compartimentul tastat nu ajungea nicăieri (funcțional, blocant)

0051-02 adăugase Editable = True/LimitToList = False, amânând unificarea txtComp/cmbComp. După eliminarea txtComp din designer, legarea rămăsese în cinci linii comentate:

- Numai CmbComp_SelectedIndexChanged scria `_draft.Comp`; textul tastat lipsea din schiță și MotiveDeRefuz spunea «Compartimentul lipsește» cu câmpul aparent complet.
- Lista goală (unitate fără DDF anterior/nomenclator compartimente) ori Catch din IncarcaListeleAsync dezactiva cmbComp, unica intrare rămasă, blocând salvarea pe baza nouă.
- `cmbComp.SelectedItem = _draft.Comp` la încărcare ignora tăcut valoarea absentă din listă, frecventă, golind compartimentul din antet.
- Sfatul de pe ecran spunea «Alegerea unuia îl scrie în caseta din stânga» și «Scrie compartimentul în caseta din stânga». Nu mai există nicio casetă în stânga.

`PreiaCompartimentul()` face idempotent cmbComp.CommitText() (0051-02) și copiază cmbComp.Text.Trim() în schiță la Leave și BtnSalveaza_Click, inclusiv fără blur. SelectedIndexChanged actualizează imediat. Lista goală/eroarea schimbă numai sfatul; după reumplerea care golește textul, acesta revine sub `_seIncarca`.

Cele cinci linii comentate `txtComp` și nota din capul designerului care descria perechea au fost șterse; nota descrie acum un singur control.

#### 3.2 `DdfEditFisierePage` construia coloanele în cod

`ConstruiesteColoanele()` era unica excepție a celor două editoare de la Designer (convenția/§4.5), lăsând grila goală la proiectare. Cele patru coloane trec în Designer cu font/aliniere/ReadOnly și umplere pe cale_fisier, ca OrdAtasamentePage.

#### 3.3 `DdfEditSectiuneaBPage` era îmbrăcată altfel decât sora ei

Aceeași formă, un clic de navigație distanță, același document — și patru diferențe:

| | secțiunea A (și `OrdEditForm`) | secțiunea B, înainte |
|---|---|---|
| `AutoScaleDimensions` | `10F, 25F` | `7F, 15F` ⇒ pagina ieșea scalată cu ≈1,43 față de surorile ei |
| font coloană / antet | Calibri 9 / Calibri 9 bold | nesetat |
| format bani | `Standard` (separatori de mii) | `Fixed` (fără) |
| bandă subsol, chenare, umplere | `FooterFont`, `Footer/HeaderBackColor`, separatori, `FillColumnKey` | nimic |

Toate aliniate pe secțiunea A. `KBotFormat.Standard` e ce folosesc **toate** cele opt coloane de bani din `OrdEditForm` — verificat, nu presupus.

Componenta `tips` era declarată și nefolosită pe pagina B; are acum sfatul de pe grilă (ce e acolo se recalculează din secțiunea A la fiecare modificare).

#### 3.4 `edtLunga.BackColor = Color.White`

Linia `edtLunga.BackColor = Color.White` activa `_backColorPinned`; `ShouldSerializeBackColor = True` fixa albul inclusiv în schema întunecată. Ștergerea ei restabilește fundalul din temă și respectă regula culorilor din designer.

#### 3.5 Un test care pica

`DdfEditGrileTests.SectiuneaA_AreColoaneleDinDesigner` cerea `KBotFormat.Fixed` pe coloanele de bani, iar designerul scria `Standard`. **Testul pica pe ramură înainte de rundă asta.** Cum `Standard` e ce folosesc `OrdEditForm` și secțiunea A, testul a fost corectat la `Standard`, nu invers.

---

### 4. Ce s-a rulat și cu ce rezultat

```
dotnet build src\KBot.App\KBot.App.vbproj      → 0 erori / 7 avertismente (MSB3825, preexistente)
dotnet test  tests\KBot.App.Tests              → 194 trecute / 13 picate / 207 total
```

`DdfEditGrileTests` — **6 din 6 trecute**, inclusiv cele două care verifică blocarea secțiunii A pentru un document din rezervări: `ReadOnlyGrid` și butoanele stinse rămân exact cum erau, doar grila nu mai e desenată ștearsă.

**Cele 13 picate sunt PREEXISTENTE pe ramură și nu ating niciun fișier atins aici:** `DdfXfaParserTests` (2), `XfaXmlPreviewTests` (1), `MainFormNavItemsTests` (1), `DdfViewTests` (3), `IstoricViewTests` (6). Aceleași 13 s-au numărat înainte de prima modificare din runda asta. Exemplu, `DdfViewTests.StaleResponse_IsDiscarded`: `Assert.Empty()` pe arborele lui `DdfView` — garda de răspuns învechit din vederea DOAR-CITIRE, altă felie. **Nu s-au atins.**

### 5. Fișiere atinse

| fișier | ce |
|---|---|
| `Views/Ddf/DdfEditSectiuneaAPage.Designer.vb` | cele zece `Enabled = False` scoase; nota despre `ReadOnly` vs `Enabled` |
| `Views/Ddf/DdfEditSectiuneaAPage.vb` | `ButtonStyles.ApplyPrimary/ApplySecondary` în locul buclei |
| `Views/Ddf/DdfEditSectiuneaBPage.Designer.vb` | `AutoScaleDimensions`, fonturi, `Standard`, bandă/chenare/umplere, sfat pe grilă |
| `Views/Ddf/DdfEditFisierePage.Designer.vb` | cele patru coloane, mutate din cod |
| `Views/Ddf/DdfEditFisierePage.vb` | `ConstruiesteColoanele` scoasă; `ButtonStyles` |
| `Views/Ddf/DdfEditDescrierePage.Designer.vb` | `edtLunga.BackColor = Color.White` scoasă |
| `Views/Ddf/DdfEditForm.Designer.vb` | nota despre compartiment rescrisă; sfatul lui `cmbComp` |
| `Views/Ddf/DdfEditForm.vb` | `PreiaCompartimentul`, `CmbComp_Leave`, `Text` în loc de `SelectedItem`, combo-ul rămâne aprins pe listă goală, liniile `txtComp` comentate șterse, bucla de butoane din `OnThemeChanged` scoasă |
| `tests/KBot.App.Tests/DdfEditGrileTests.vb` | `Fixed` ▸ `Standard` |
| `docs/worklog/SLICE-0051-03-ddf-edit-congruenta.md` | fișierul ăsta |
| `docs/worklog/KBOT_STATUS.md` | rândul feliei |

Nicio versiune de proiect n-a fost urcată: `KBot.App` n-are `FileVersion` propriu de urcat în runda asta (doar `Views/`), iar `KBot.Controls` și `KBot.Theming` **nu s-au atins**.

### 6. Rămas neverificat / amânat

- **`DdfEditForm` tot n-a fost văzut pe ecran** — nici în aplicație, nici în designerul VS, nici prin `DrawToBitmap`. Toate afirmațiile despre aspect (butoane rotunjite pe `Modern`, grilă nestinsă, secțiunea B la aceeași scară cu A) sunt citite din cod, **nu văzute**. Asta e verificarea care lipsește și e cea care contează cel mai mult pentru o rundă despre aspect.
- **Nimic n-a rulat pe MariaDB sau pe serverul Python** — nici de data asta. Calea corectată a compartimentului (tastare ⇒ `_draft.Comp` ⇒ POST) **n-a fost probată cu un server adevărat**; e citită din cod și din contractul lui `KBotComboBox.CommitText` descris în 0051-02.
- **Zero teste noi.** Ce s-a schimbat aici e în cea mai mare parte designer și pictură; singurul lucru testabil ieftin — compartimentul tastat — cere un `DdfEditForm` construit cu un `IApiClient` fals și un `DdfEditReauth`, adică o schelă pe care felia n-o are. **Merită scrisă** și e cel mai bun candidat pentru runda următoare.
- **Cele 13 teste picate de pe ramură rămân picate.** Sunt din alte felii (`DdfView`, `IstoricView`, `XfaXmlPreview`, parserul XFA, iconițele din `MainForm`) și n-au fost atinse aici, ca să nu se amestece o rundă de aspect cu remedieri fără legătură.
- **`AplicaModulDeEditare` lasă `lblStare` cu mesajul de blocare** dacă aceeași instanță de pagină primește pe rând un document blocat și apoi unul liber: mesajul se rescrie abia la `ReimprospateazaCombo`. Astăzi formularul e modal și pornește o pagină nouă de fiecare dată, deci nu se vede. Consemnat, nu remediat.
- **`Dim _salariiArFiFostEditabil`** din `AplicaEnablement` rămâne o variabilă scrisă și necitită, ținută intenționat de felia 0051 ca urmă a regulii din Access. Lăsată cum era.
