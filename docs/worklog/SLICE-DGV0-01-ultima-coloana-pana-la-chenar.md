# SLICE-DGV0-01 - the last column of a grid reaches its border

Operator, 19.11.2026: «in DGV-uri exista un spatiu de cativa pixeli la dreapta care se vad urat tare. ar trebui ca ultima coloana sa aiba border comun cu controlul». Slice and number given by the operator.

## What changed and why
- Widths of columns are LOGICAL integers; every column is rounded to the screen on its own (`ScalePx`), and the fill column is written with the rounding that never goes over
  (`SetLayoutWidthAtMost`, on purpose: a pixel too many = a horizontal bar). At 125 % / 150 % the columns together therefore could fall a few device pixels short of the
  viewport, leaving a strip to the right of the last column. **This is my reading of the code, NOT confirmed on screen** (the app was not run, by the operator's rule).
- Fix: after the leftover is handed out, `AbsorbRoundingGap` (KBotDataView.AutoSize.vb) gives the remaining device pixels to the fill column
  (`KBotDataColumn.LayoutExtraPx`, added to `WidthPx`, reset at the start of every pass). Only a remainder of at most (number of columns + the DPI scale, rounded up) pixels
  is taken: a real leftover (a column capped by `MaxWidth`, no fill column) stays empty. `Proportional` gives it to the last column. At 100 % the remainder is 0 (no effect).
  The existing rule `RightEdgeTouchesBorder` already draws no separator on a column that reaches the border, so the last column now shares the border of the control.
- `KBot.Controls` FileVersion 1.85.2.0 -> 1.85.3.0.

## Second pass (19.11.2026, the gap was still there at 150 % on the grid of new invoices)
- The first fix covered only the branch where the columns were NARROWER than the viewport. On `gridNoi` of «Facturi si plati» the authored widths (1004 logical) are WIDER than the grid (about 855 logical), so the pass takes the SHRINK branch (`ShrinkToFit`), which lowers every column with the rounding that never goes over (up to one logical pixel each); the losses add up and the viewport is left short by several pixels. `AbsorbRoundingGap` is now also called after `ShrinkToFit` and after `ShrinkFillTarget` (`afterShrink:=True`: there the whole remainder is rounding, so it is taken without the size bound).
- Still NOT confirmed on screen.
- The nav bar of «Facturi si plati» drew its own 2 px footer line (`KBotNavList.FooterBorderVisible`, default True); switched off in the designer of the page (AV02-06). The frame thickness of the bar follows `BorderWidth` of the grid.

## Files touched
`src/KBot.Controls/DataView/KBotDataColumn.vb`, `src/KBot.Controls/DataView/KBotDataView.AutoSize.vb`, `src/KBot.Controls/KBot.Controls.vbproj`.

## Test results
`dotnet build src/KBot.App/KBot.App.vbproj` (output redirected): 0 warnings, 0 errors. No tests, nothing run or seen on screen.

## Unverified / deferred
- The cause (rounding) is a hypothesis from the code. If a gap remains on screen it has another cause: look at the right border strip (`ReserveVScrollSpace`, the border width,
  a column with `Resizable = False` / `MinWidth`) and tell me which grid and which Windows scale.
- The control is shared by ALL grids, E-Factura included (not edited, but it will look different by a few pixels); not seen on screen anywhere.
- Uncommitted work of another slice (the filter popup, `SLICE-DV01-01`) is in the same folder: not touched.
