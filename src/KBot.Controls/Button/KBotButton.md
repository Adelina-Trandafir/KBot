# KBotButton

K-BOT's own push button (slice 0112): a rounded face drawn from the active theme, no system colours.

`Button/KBotButton.vb` · `Control` · Toolbox · `IButtonControl`, `IThemedControl`
Conventions: [C1..C9](../CONTROLS.md). Status: builds; not seen on screen.

## API
- `Text`, `Font`, `Enabled`, `TabStop` — as on a `Button`.
- `Primary: Boolean = False` — accent-filled face (the action the dialog is for).
- `DialogResult` — pressing the button sets it on the form, which closes (as `Button`).
- `AcceptButton` / `CancelButton` work (it is an `IButtonControl`): Enter / Esc, the default button gets an accent border.
- `PerformClick()`, `Click` event. Space presses it from the keyboard; mouse press shifts the caption by a pixel.
- `GetPreferredSize` = text + padding (use it, `AutoSize` is not wired).

## Look
Colours from the palette only: face `ButtonBack` (hover `ButtonHover`, pressed `ButtonPressed`), border `ButtonBorder`,
text `ButtonText`; `Primary` uses `Accent` / `AccentHover` / `AccentText`; disabled uses `SurfaceAlt` / `DisabledText`.
Corner radius = the theme's `Style.CornerRadius`, scaled with the dpi. Keyboard focus draws a dotted inner ring.

## Limits
No image, no tooltip hook of its own (use `KBotToolTip`), no mnemonic (`&` is drawn as written), no per-button colour override.
