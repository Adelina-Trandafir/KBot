# SLICE-0114-02 - Ribbon: separator before a pinned button, page buttons, «Ajutor» removed (16.10.2026)

Sub-slice of 0114 (the slice that made `KBotRibbon`), asked by the operator: (1) remove the «Ajutor» button from the ribbon;
(2) while the «Sumar» view is on screen a new button «Asociaza parteneri» and a separator appear before «Inchide», and go away
when the view is no longer on screen; the system will be reused, so the control changes if it has to. The Sumar side is
`SLICE-0084-04-asociere-parteneri-pe-ribbon.md`.

## What changed and why
1. **Control** (`KBot.Controls/Ribbon`): `KBotRibbonButton.SeparatorBefore` (designer property, default False) + derived
   `SeparatorX`. `KBotRibbon.ComputeLayout` gives a pinned button that asked for it a 6 px gap and a thin line to its left,
   **only while another pinned button to its left is visible** (so «Inchide» alone shows no line); both layouts (the panel
   and the folded tab strip). `OnPaint` draws the line in the border colour. A pinned button already supported `Visible`, so
   no other change: a page button is a pinned button with `Visible = False` at design time that the host shows and hides.
2. **`AvacontMain`**: the pinned «Ajutor» (key `ajutor`) is gone, with `ShowRibbonHelp` and its click case. The new pinned
   button `sumar_parteneri` («Asociaza parteneri», icon `binvoice`, hidden at design time) comes first; «Inchide» has
   `SeparatorBefore = True`. `UpdateSumarPartnersButton` (called from `UpdateCloseButton`, i.e. on every page change, and from
   `KbotForm.ActiveViewChanged`) shows it only when no page is open, the shell is the base and the active view is «sumar»; it
   is enabled only when the angajament has a DDF. A click runs `KbotForm.OpenSumarPartners`.
3. Kept on purpose (house rule: old paths switched off, not deleted): `HelpService.ShowHelpPage`, the registry kind `ajutor`,
   `KBotRibbon.ButtonScreenBounds`, the resource `ajutor_128`. F1 and the caption bar «?» still open the help.

> Superseded on the App side by `SLICE-0114-03-ribbon-grup-dreapta.md` (same day): the button now lives in the ribbon's right group; `SeparatorBefore` stays in the control but «Inchide» no longer sets it.

## Files touched
`src/KBot.Controls/Ribbon/KBotRibbonItems.vb`, `KBotRibbon.vb`; `src/KBot.App/AvacontMain.vb`, `AvacontMain.Designer.vb`,
`KbotForm.Views.vb`; help: `contabil/ajutor.md`, `contabil/ribbon.md`, `contabil/ajutor-buton.md` (deleted), `docs/HELP_SYSTEM.md`
(+ `sumar.md`, `tur-sumar.md`, see 0084-04). Status rows in `KBOT_STATUS.md`, `state/KBOT_STATUS_0110-0119.md`.

## Test results
`dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors. No tests run (house rule). Not run, not seen on screen.
`Check-Help.ps1`: see the help status row 0000-85 (the errors that remain are old ones, in tutorials).

## Unverified / deferred
- The look: the 6 px gap and the line (panel: from 10 px below the tab strip to 12 px above the bottom; folded: 6 px in the tab
  strip) in the three themes, at 100% and 150%. The «Asociaza parteneri» caption width in the panel.
- The icon is `binvoice` (the one of «Parteneri»): there is no partners-specific picture in the resources. Say if another is wanted.
- Captures `ribbon-file` / `ribbon-strans` of the help still show «Ajutor»; to redo.
- `KBotRibbon.md` / `CONTROLS.md` still unwritten (open thread of 0114).
- Not committed (git writes are the operator's).
