# SLICE-0094-03 — DDF editor: the «Parteneri» tab follows «Partener asociat»

Corrective on 0094-02 (operator, 04.10.2026). Only `DdfEditForm`.

## What changed and why

The «Parteneri» tab of the DDF editor was always on the page bar. It is now tied to the
«Partener asociat» checkbox (`chkPartAng`, `_draft.PartAng`):

- **On load** the tab is visible only if the document has `PartAng` (set next to the
  section-B visibility, before the first page is selected).
- **On toggling the box** (`ChkPartAng_CheckedChanged`) the tab is shown/hidden. A hidden nav
  item stays selected in `KBotNavList`, so when «Parteneri» is the open page and the box is
  cleared, the editor switches to «Secțiunea A».

Nothing else moved: `KBotNavList` unchanged, no other form touched, the draft and the save are
unchanged (see deferred).

## Files touched

- `src/KBot.App/DDF_EDIT/DdfEditForm.vb` — `DdfEditForm_Load`, `ChkPartAng_CheckedChanged`.
- Help (0094-03 added to the slice tags): `HelpContent/contabil/ddf/editor.md`,
  `HelpContent/tours/tur-ddf-editor.md`. The tours skip a part that is not on screen
  (`KBotNavList.HelpParts` returns an empty rectangle for a hidden item), and the 000T tutorial
  step «Fila «Parteneri»» already has `when: checked:DdfEditForm.chkPartAng`, so neither needed
  a structural change.

## Test results

`dotnet build src\KBot.App\KBot.App.vbproj -c Debug`: 0 warnings, 0 errors. No tests run, nothing
seen on screen (house rule).

## Unverified / deferred

- Not seen on screen: the tab disappearing/appearing, and the fall-back to section A.
- **Hidden partners are still saved.** Clearing the box removes only the main partner from the
  list (0094-02 behaviour); the other associated partners stay in `_draft.Parteneri` and are
  sent on save, although the tab that shows them is hidden. Whether clearing the box should drop
  them too is the operator's call.
- `help-version.txt` not bumped here (already modified in the working tree by other help work).
