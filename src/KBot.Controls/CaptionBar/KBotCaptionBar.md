# KBotCaptionBar

Title bar for borderless forms (`FormBorderStyle.None`): icon + title on the left, control
box on the right, window drag from the free area. All colours from the active scheme.

`CaptionBar/KBotCaptionBar.vb` (+ `.ThemeButton.vb`, `.Selectors.vb`, `ThemeSchemeChangedEventArgs.vb`)
`Control` · sealed · partial · Toolbox · `IThemedControl`, `IPopupAnchor`
Conventions: [C1..C9](../CONTROLS.md).
Status: covered by `KBotCaptionBarOptionButtonTests`, `KBotCaptionBarThemeButtonTests`.

## API — bar
- `IconImage: Image` — Empty leaves the title flush left.
- `ShowClose = True` (slice 0112) — False hides the X completely (the message box does it for questions that must be answered); the other buttons keep their slots.
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
- `ShowTextScaleSlider = True` — the text-size slider row at the top of the menu, shown only
  while `ThemeManager.WritesFormFont` is on. It snaps at `TextScaleSnapPoints` (100 / 110 / 125 %).
- The menu is slider + separator + selectable schemes, nothing else (operator request,
  20.09.2026). "Font din temă", "Opțiuni temă…" and "Stiluri…" left the menu; the first two
  live in the settings window, page "Temă".  `ShowThemeOptions` / `ShowThemeEditor` no longer exist.
- `ThemeButtonImage`, `ThemeButtonPadding = 2`, `TintThemeButtonImage = True`
- `ThemeButtonActive`, `ThemeButtonBounds` (read-only)
- `ShowThemeMenu()`, `ThemeSchemeChanged As EventHandler(Of ThemeSchemeChangedEventArgs)`

The menu builds itself here, not in the host: `MainForm` used to own ~100 lines of it that
a second bordered form would have had to copy.

## API — title selectors (`KBotCaptionBar.Selectors.vb`, slices 0097 / 0097-03)
Painted drop-downs after the title, addressed by name: `SelectorUnit` (drawn with 2+ choices),
`SelectorYear`, `SelectorSector`, `SelectorKind` (1+ choices; each has a dim label before the box).
`SelectorKind` («Tip factură») is the invoice-kind selector of the E-Factura window (sales /
purchases); it is not drawn until the host gives it choices.
- `SetSelectorItems(selector, items, key)`, `ClearSelector(selector)` (overloads without a name = unit)
- `GetSelectorKey`, `SetSelectorKey` (does not raise the event), `SetSelectorShown` (host hides one)
- `SelectorChanged` — `Selector` + `Key`; the selector moves only when the host sets the key.

## Behaviour
- The host does NOT re-apply the theme after a choice — `ThemeManager.SetScheme` broadcasts
  to every open form. `ThemeSchemeChanged` is for EXTRA work only (a scheme-dependent icon).
- The slider row uses an `@`-prefixed key (`@TextScale`) so a user scheme cannot collide
  with it.
- Implements `IPopupAnchor`, so the button stays lit while its menu is open.

## Limits
- Needs a borderless form; on a form with a system frame you get two title bars.
- No tooltips on its buttons yet (known gap, together with `KBotNavList` items).
- The control box is min/max/close only — no custom extra buttons beyond the two above.
