# SLICE-0022-02 — `IstoricView` + `IstoricFilter` (the fifth real hosted view)

Pass 2 of slice 0022, per `PLAN_IstoricView.md` §4–§7. `MainForm.CreateView("istoric")` no longer
returns a `PlaceholderView`. Read-only, no writes anywhere.

Access source was a **popup dialog** (`frmFX_ISTORIC`); here it is a **hosted view** in the shell,
like Sumar/Rezervări/Recepții/Plăți/DDF. It shows every `FX_Istoric` row for the selected
angajament, with the three column filters Access had, a three-column totals row, and a detail
pane for the two long text fields.

## Layout (§4.1)

`pnlFiltre` (Dock=Top, filter strip) over a horizontal `SplitContainer`: grid (`KBotDataView`,
read-only, totals row) on top, `pnlDetaliu` (two read-only multiline textboxes side by side) at
the bottom. `lblEmpty` is topmost (Fill) and covers everything in the empty/loading state.
All controls declared in `IstoricView.Designer.vb` (house rule). Dock-order trap respected: `split`
added before `pnlFiltre`.

## Grid (§4.2)

Twelve columns in Access `Left` order: `Clsf`, `TipRand`, `Data` (`dd.MM.yyyy`), the seven money
columns, `Descriere`, `Observații`. Money columns store a `Double`, `FormatString = "N2"`,
right-aligned — same idiom as `PlatiView`, read from it rather than invented. **Totals row on
EXACTLY the three columns Access totalled** (`tot_Val_Rezervare_Dif`, `tot_Val_Receptie`,
`tot_Val_Plata` → `vrdif`/`vrec`/`vpl`, `Aggregate = Sum`); totalling the other four would show a
figure the source never showed. `Descriere`/`Observatii` carry `AutoHide = True` (0016) — they
auto-hide first because the detail pane carries them in full.

## Detail pane (§4.3)

Grid selection change → fills `txtDescriere`/`txtObservatii` from the selected row's `Tag`
(`IstoricRand`); no selection → both empty. This **flattens** the Access two-band continuous-form
row (values on line 1, Descriere+Observatii on line 2) into one grid row plus a pane —
`KBotDataView` is one line per row and is not being changed for one consumer.

## The three filters

**`IstoricFilter`** (`KBot.App/Views/IstoricFilter.vb`) — a **pure class, no UI references**, a
faithful port of `mdl_FX_Popups.ApplyColumnFilter`: three independent segments (`Clsf`,
`TipRand`, `DataFx`); setting a segment **replaces** it; "TOATE" **clears only that segment**; the
effective filter is the AND of the non-empty segments; `Apply(randuri)` returns the filtered
list. `Clsf` = a set of `id_clsf`; `TipRand` = exact value or a `Rez_`/`Plata_` prefix group,
matched **case-insensitively**; `DataFx` = month+year or an exact day.

The three menus are themed `ContextMenuStrip`s built from the loaded data (this replaces Access's
hover-following `bFilter` button — deviation §6 below):

- **Clasificații** — hierarchical Subcapitol → Articol → Alineat from `clasificatii[]`. Root
  "TOATE"; a per-level "TOATE" only where that level has **more than one** child (the Access
  `dictArtPerSub`/`dictAlinPerArt` rule). Captions from the three `den_*` fields.
- **TipRand** — distinct `tip_rand` from the loaded rows. Submenu "REZERVĂRI" for `Rez_`, "PLĂȚI"
  for `Plata_` (both case-insensitive), rest flat. Root "TOATE"; a per-group "TOATE" only when the
  group has more than one entry (Access `rezCount > 1`); group "TOATE" filters by prefix. The `+`
  variants (`Rez_Initiala+`, …) are distinct values and get their own entries — correct, they
  mean something different from the unsuffixed ones.
- **DataFX** — `AnLuna` (`MM-YYYY`) → `Ziua`. Root "TOATE"; a per-month "TOATE" only when the
  month has more than one day (Access `dictZiPerAL`).

`lblFiltruActiv` shows a short summary of the active segments; `btnReset` clears all three.

## Loading (§6)

`SetContext(info)`: `Nothing`/blank cod → clear grid + pane + menus, **no network call**.
Otherwise set `_requestedCod`, call through `WithReauth(Of IstoricInfo)`, and **discard the
response if `_requestedCod` has moved on** (stale-guard, copied from `DdfView`). On success: fill
the grid, rebuild all three menus, and **reset all three filter segments unconditionally** — a
filter from the previous angajament must never survive (same rule as `DdfView`'s combo reset).

## `RandSchimbat` — wired, dormant, deliberate (§7)

Access set a hidden `CL` textbox to the row `ID` on `Form_Current` and declared
`Public Event RowChanged(key)` with the `RaiseEvent` commented out. Kept: `RandSchimbat` is raised
on grid selection change, carrying the row `Id`. **Nothing subscribes** — same shape as
`AdaugaDdfCerut` in Rezervări. Recorded here so a later pass does not "clean up" an apparently
unused member.

## Theming

`IstoricView` implements `IThemedControl`; `ApplyTheme` styles the strip/detail/empty state,
`ButtonStyles.ApplySecondary` on the four buttons, and themes the three `ContextMenuStrip`s (which
are **not** in the control tree, so `ThemeManager.Apply` cannot reach them) via a palette-derived
`KBotMenuColorTable : ProfessionalColorTable`. The grid self-themes (it implements
`IThemedControl`).

## The nine deviations from Access (§9 of the plan)

1. **TipRand and DataFX menus are scoped to the current angajament.** Access queried `FX_Istoric`
   unscoped, listing values from every angajament in the database — a bug, not a feature.
2. **TipRand matched case-insensitively** — Access got this free from `Option Compare Database`;
   the writer emits both `Rez_Initiala` and `PLATA_PLATA`.
3. **`cod` is an exact match.** Access used `Like`.
4. **The two-band Access row is flattened** to one grid row plus a detail pane.
5. **Alineat caption = `Clasificatii.Denumire`** (operator decision; `DefaClsfE.Denumire` was the
   alternative).
6. **The hover-following `bFilter`** is replaced by a filter strip with three menu buttons — an
   Office `CommandBars` idiom with no WinForms equivalent worth reproducing.
7. **Six `FX_Istoric` columns are deliberately not on the wire** (§2.1 — see 0022-01).
8. **`clasificatii` is deduped on `IdClsfAcc`** (see 0022-01).
9. **`data_fx` keeps its time component**, unlike DDF's `_iso` (see 0022-01).

### Two Access bugs NOT ported (§5.3)

- `bFilter_Click` builds `"SELECT * qFX_Clsf2026_Structura …"` — **no `FROM`**; the classification
  filter has never run in Access as exported. Nothing to be faithful to.
- `Popup_DataFx_Function` applies to `Forms!frmFX_MAIN.fxIstoric` while the other two apply to
  `Forms!frmfx_Istoric` — two different form objects. Here all three act on one grid.

## Tests

- **`IstoricFilterTests`** (pure, no STA) — replace-within-segment, accumulate-across-segments,
  "TOATE" clears one and leaves two, `Rez_`/`Plata_` case-insensitive grouping (`PLATA_PLATA`
  under `Plata_`), `Rez_Initiala` vs `Rez_Initiala+` distinct, month vs day segment, `ClearAll`.
- **`IstoricViewTests`** (STA thread + `Application.DoEvents()`, as Sumar/Rezervări/DDF) — blank
  context makes no call + clears grid/pane; response fills the grid with all rows; a stale
  response is discarded; the totals row runs on **exactly** the three Access columns; a new
  context rebuilds the menus and **resets all three filter segments**; the detail pane follows
  selection (no selection → empty); `RandSchimbat` carries the selected row's `Id`.
- 16 new `KBot.App.Tests` (App 120 → 136). **Test bug caught, not product:** the `Friend
  WithEvents` menu backing field is `_menuTipRand`, not `menuTipRand` — the reflection helper
  now reads `_<name>` first. `KBot.App` `FileVersion` 1.0.2.0 → 1.0.3.0.
- Full solution: **.NET 436 passed / 0 failed**, build 0 errors / 0 new warnings; **Python 75
  passed / 15 skipped / 0 failed** offline.

## ⚠️ Open

- **No visual verdict** — the view has never been seen on screen; the harness stays the only way
  to get one. Menu layout, the three `ContextMenuStrip`s' theming, the flattened two-band row and
  the auto-hide of Descriere/Observatii at real width are all unverified.
- Never run against a live database — the view has only seen hand-built `IstoricInfo` objects.
- The endpoint's `DefaClsfF`/`DefaArticol` caption risk (0022-01) means the classification **menu
  captions** are the least-certain part until the host run; filtering itself keys on `id_clsf`
  and is unaffected.
- `RandSchimbat` is raised but unsubscribed by design.
