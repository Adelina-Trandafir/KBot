# SLICE-0089-01 — server journals only with «Opțiuni avansate» (operator request, 01.10.2026)

## What changed and why
Operator: the server journals are tied to the advanced options switch; with it off the user must not see the
combo that picks the journal type.

`SetariJurnalView`:
- `PotrivesteTipuriCuClientul`: «Server FOREXE» / «Timpi FOREXE» are listed only with an `IApiClient` AND
  `AppSettings.AdvancedOptions`; otherwise only «Jurnale locale» exists.
- `ArataCombinatiileDeTip` (called from `Activated`): `CmbTipJurnal` and `cmbSesiuni` are hidden when the server
  journals are off, and the first two columns of `tlyFilterActual` collapse to 0 so no gap is left. The designer
  widths are remembered the first time they are collapsed and restored when the combos come back. It is only
  called from `Activated` (not the constructor) so the widths are read after the DPI scaling.
- The switch lives on another settings page, so it can only change while the journal page is away; activation
  re-reads it. Switching it off while a server journal was selected falls back to «Jurnale locale» and reloads.

## Assumption
Read as a requirement («the server journals are connected to the advanced options checkbox, if false the combo is
not visible»): the code did not have this link before. «Jurnale locale» stays available without the switch.

## Files
`src/KBot.App/Setari/SetariJurnalView.vb`; help `avansat/jurnale.md` (see 0000-35).

## Test results
`dotnet build src\KBot.App`: no compile errors. Nothing run on screen; the collapsed columns were not seen.
