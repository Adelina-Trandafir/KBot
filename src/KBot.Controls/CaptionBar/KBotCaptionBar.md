# KBotCaptionBar

Title bar for borderless forms (`FormBorderStyle.None`): icon + title on the left, control
box on the right, window drag from the free area. All colours from the active scheme.

`CaptionBar/KBotCaptionBar.vb` (+ `.ThemeButton.vb`, `ThemeSchemeChangedEventArgs.vb`)
`Control` · sealed · partial · Toolbox · `IThemedControl`, `IPopupAnchor`
Conventions: [C1..C9](../CONTROLS.md).
Status: covered by `KBotCaptionBarOptionButtonTests`, `KBotCaptionBarThemeButtonTests`.

## API — bar
- `IconImage: Image` — Empty leaves the title flush left.
- `ShowMinimize = False`, `ShowMaximize = False` (maximize also enables double-click on the
  drag area). A dialog gets close only.
- `ApplyTheme(scheme)`

## API — options button (left of the control box)
- `ShowOptionsButton = False`, `OptionButtonImage`, `OptionButtonPadding`,
  `TintOptionButtonImage = True` (recolours the glyph so it follows the theme; turn off for
  a coloured icon)
- `OptionButtonActive`, `OptionButtonBounds` (read-only), `OptionButtonClick` event.

## API — theme button (`KBotCaptionBar.ThemeButton.vb`)
Second icon button, left of the control box, that drops the scheme menu.
- `ShowThemeButton = False` — one flag is all a host needs.
- `ShowTextScaleSlider = True` (text-size slider row at the top of the menu),
  `ShowThemeOptions = True` (row "Opțiuni temă…" → `ThemeOptionsForm`),
  `ShowThemeEditor = True` (last row "Stiluri…" → `ThemeEditorForm`)
- `ThemeButtonImage`, `ThemeButtonPadding = 2`, `TintThemeButtonImage = True`
- `ThemeButtonActive`, `ThemeButtonBounds` (read-only)
- `ShowThemeMenu()`, `ThemeSchemeChanged As EventHandler(Of ThemeSchemeChangedEventArgs)`

The menu builds itself here, not in the host: `MainForm` used to own ~100 lines of it that
a second bordered form would have had to copy.

## Behaviour
- The host does NOT re-apply the theme after a choice — `ThemeManager.SetScheme` broadcasts
  to every open form. `ThemeSchemeChanged` is for EXTRA work only (a scheme-dependent icon).
- The special menu rows use `@`-prefixed keys so a user scheme literally named "Stiluri"
  cannot be confused with them.
- Implements `IPopupAnchor`, so the button stays lit while its menu is open.

## Limits
- Needs a borderless form; on a form with a system frame you get two title bars.
- Theme-menu actions throw `InvalidOperationException` when the bar has no parent form.
- No tooltips on its buttons yet (known gap, together with `KBotNavList` items).
- The control box is min/max/close only — no custom extra buttons beyond the two above.
