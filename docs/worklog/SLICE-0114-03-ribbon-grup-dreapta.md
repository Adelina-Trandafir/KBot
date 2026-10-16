# SLICE-0114-03 - Ribbon: the right group (16.10.2026)

Sub-slice of 0114, asked by the operator: more than one button, and other kinds of elements, for what a view offers; a right
group, apart from the «Inchide» button. Replaces the one-button mechanism of 0114-02 on the App side.

## What changed and why
1. **`KBotRibbon.RightGroup`** (a `KBotRibbonGroup`, designer-authored: Buttons, Fields, Text, Visible; expandable in the property
   grid). It stands at the right end of the panel, before the large pinned buttons («Inchide»), the same on every tab. It is shown
   only while it has a visible button or field, and not while the ribbon is folded (its field controls are hidden then, like
   the tab groups'). Its right edge line (every group draws one) is the separator before «Inchide».
2. Everything that walked «the groups of the tabs» now walks `AllGroups()` (tab groups + the right group): `FindButton`,
   `FindField`, `ValidateKeys`, theme, field tooltips, field controls. Layout: the group placement became `PlaceGroup` (used by the
   tab groups and the right group; the right group is placed at 0 and shifted with `ShiftGroup`); paint: `PaintGroup`; hit test:
   `ButtonInGroup`. No change of look for the tab groups.
3. **App**: the button `sumar_parteneri` moved from `PinnedButtons` to `ribbon.RightGroup.Buttons` (designer);
   `SeparatorBefore` is no longer set on «Inchide» (the group's own line does it). `UpdateSumarPartnersButton` also sets the
   group's caption («Sumar») while it shows the button. Another button or field for a view = one more control in the right
   group with `Visible = False`, shown and hidden by the host.
4. `KBotRibbonButton.SeparatorBefore` (0114-02) stays in the control, unused for now (it still works between two pinned buttons).

## Files touched
`src/KBot.Controls/Ribbon/KBotRibbon.vb`; `src/KBot.App/AvacontMain.vb`, `AvacontMain.Designer.vb`; help `contabil/ribbon.md`,
`contabil/vederi/sumar.md`, `tours/tur-sumar.md` (help 0000-85, extended). Status rows in `KBOT_STATUS.md`,
`state/KBOT_STATUS_0110-0119.md`.

## Test results
`dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors. No tests run (house rule). Not run, not seen on screen.

## Unverified / deferred
- Not seen: the group at the right of the panel (width, caption «Sumar», the line before «Inchide»), the three themes, 100% / 150%.
- With a narrow window the right group can overlap the groups of a tab (no clipping or wrapping yet); a hidden-by-default
  field of the right group has never been shown (the tab groups' fields were seen only in `KBOT_TEST`).
- The right group is not shown while the ribbon is folded (decision, not asked): small versions of its buttons in the tab strip
  would be the alternative.
- With no tab shown at all the layout stops before the right group (as it does for the pinned large buttons).
- `KBotRibbon.md` / `CONTROLS.md` still unwritten (open thread of 0114).
- Not committed.
