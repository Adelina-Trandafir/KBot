# SLICE-0096 - Extrase: display menu in the tree header

Operator request, 29.09.2026: in Extrase (the view AND the «Extrase de cont» window) the left
tree's header gets a right icon (the usual settings icon) with two menu rows:

1. «Arată antet + operații» - as it was.
2. «Arată operații + detalii» - the top grid (which held FX_Extrase_H) holds FX_Extrase for the
   period selected in the tree; the bottom holds the detail of the selected operation. «Data»
   gets grouping (only in this mode); «Plătitor» and «CUI» get filtering and grouping.

## Change

Both hosts use `ExtrasePanel`, so the change is there only.

- `Views/Extrase/ExtrasePanel.Designer.vb`: `tree.HeaderRightIcon = settings__1_` +
  `HeaderRightIconTooltip`. Nothing else in the design touched.
- `Views/Extrase/ExtrasePanel.vb`:
  - `ExtraseDisplayMode` enum (`AntetOperatii` / `OperatiiDetalii`), `DisplayMode` property
    (not serialized), default `AntetOperatii`.
  - `Tree_HeaderRightIconClicked` -> `CustomPopup` with the two rows (current one checked),
    same pattern as the main tree options menu; unknown key -> `ArgumentException`.
  - Root and month nodes now also carry their operations (root = all, month = its days'),
    ordered by DataBanca then IdFxe. In `OperatiiDetalii` every node goes the «day» path:
    `gridZi` on top + `detailPane` below (the existing grid and detail, reused).
  - `ApplyDisplayColumns`: on `gridZi`, in `OperatiiDetalii` the columns `o_data_banca`,
    `o_platitor`, `o_cui` get `ShowColumnFilter = True` + `AllowGrouping = True` (CUI takes
    the same filter icon as Plătitor). In `AntetOperatii` the designer values come back,
    except that «Data bancă» loses grouping (the request says grouping by date only in the new
    mode); the grouping is lifted and a filter left on a column that loses its filter button is
    cleared.

## Assumptions

- «Data» = «Data bancă» (`o_data_banca`), the date column of the operations grid.
- «Grupare per Data» = the «Grupare» tab is OFFERED in the column menu (like Plătitor / CUI),
  not an automatic grouping applied on entering the mode.
- The choice is kept per window for the session, not saved in `AppSettings`.

## Status

Build of KBot.App clean (0 warnings, 0 errors). Not run, not seen on screen, no tests
(operator's rule). No git. KBot.App FileVersion not bumped here - `push-update.ps1` asks.
