# SLICE-0114-05 - «Descarca extrase»: only the ribbon button, tied to the Extrase window / the shell (17.10.2026)

## What changed and why
Operator: the download of the statements belongs to the ribbon button «Descarca extrase» (0114-04), which must be associated with
the Extrase window or with the main shell (KbotMain); the icons in the trees are to be hidden.
1. **Ribbon button** `extrase_descarca` (`AvacontMain`): `Visible = False` in the designer; new `UpdateExtraseDownloadButton`
   (called from `UpdateCloseButton`, so on every page / shell change) shows it only while `CurrentPage Is ExtraseForm` or the shell
   is on screen (`CurrentPage Is Nothing AndAlso _baseIsShell`). The ribbon skips a group with nothing visible, so the «Descarcare»
   group goes with it.
2. **Tree icons gone**: the main tree lost its left icon (credit card + tooltip, `KbotForm.Designer.vb`) and its handler
   `Tree_FooterLeftIconClicked` (`KbotForm.Extrase.vb`); `ExtraseView` no longer takes the `descarca` delegate, sets
   `panel.ShowDownloadIcon = False` and drops `Panel_DescarcaCerut`; `KbotForm.Views.vb` builds it without the delegate.
   With the icon out, the footer text of the Extrase tree («Actualizeaza») goes too (see the 00EF-33 follow-up: the caption needs a
   visible icon).
3. Kept: the window's own button `ExtraseForm.btnDescarca`, `ExtrasePanel.ShowDownloadIcon` / `DescarcaCerut` (unused now, harmless),
   the resource `credit_card`.
4. Help: 0000-89. Captures marked for redo (36, see its worklog).

## Files touched
`src/KBot.App/AvacontMain.vb`, `AvacontMain.Designer.vb`, `KbotForm.Designer.vb`, `KbotForm.Extrase.vb`, `KbotForm.Views.vb`,
`Views/ExtraseView.vb`.

## Test results
`dotnet build src/KBot.App/KBot.App.vbproj`: 0 warnings, 0 errors. Nothing run, nothing seen on screen.

## Unverified / deferred
- Read of «associated with the Extrase form or KbotMain»: visible on the Extrase window and on the shell (any view). If only the
  shell's Extrase view was meant, narrow `UpdateExtraseDownloadButton` with `_kbot.ActiveViewKey = "extrase"`.
- Not seen: the button appearing / disappearing when pages change, nor the empty-group rule of the ribbon.
- `ExtraseForm.btnDescarca` left in place (not mentioned).
