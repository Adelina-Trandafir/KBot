# KBotBusyBar

Indeterminate activity bar: one accent segment slides back and forth while `Running`.
Replaces the marquee `ProgressBar`, which Windows paints and the theme cannot reach.

`BusyBar/KBotBusyBar.vb` · `Control` · sealed · Toolbox · `IThemedControl`
Conventions: [C1..C9](../CONTROLS.md). Status: not recorded.

## API
- `Running: Boolean = False` — starts/stops the 15 ms timer.
- `ApplyTheme(scheme)`

## Behaviour
- Height is set to 3 (logical) in the constructor; the host may override it.
- Stopped = paints nothing over the track, so it is invisible on a card.
- Colours: accent segment + track, both from the palette.

## Limits
- Indeterminate only. For 0..100 use [KBotProgressBar](../Progress/KBotProgressBar.md).
- No percentage, no text, no orientation switch — it is horizontal.
- At design time the value is remembered and the segment is painted where it stands, but
  the timer never runs (a 15 ms tick inside `devenv` repaints forever).
- No `Value`/`Maximum`, no completion event: the host decides when to stop it.
