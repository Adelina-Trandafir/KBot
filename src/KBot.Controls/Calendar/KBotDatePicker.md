# KBotDatePicker

The date field: a text box you can type into, with a drawn calendar button on the right that
drops a [KBotCalendar](KBotCalendar.md). Same rounded outline, same input colours and same focus
ring as the calendar it opens, so the two read as one control.

`Calendar/KBotDatePicker.vb` · `Control` · sealed · Toolbox · `IThemedControl`
Conventions: [C1..C9](../CONTROLS.md). Status: slice 0050, **never seen on screen**.

## Why not `DateTimePicker` — it cannot be made taller
The stock control overrides its own bounds and snaps `Height` back to the system combo height, so
it can never line up with a taller row or a stretched form. This one is a plain `Control` that
never touches its own bounds: **set `Height` to anything, or dock it, and it fills what it was
given** — the outline stretches, the single line of text stays vertically centred, and the button
grows with the field. On top of that the stock control is a native window whose face keeps the
system colours on a dark scheme.

## API
- `Value: Date` — **time of day included**; clamped to `MinDate`/`MaxDate`; writing it always
  makes the field non-empty.
- `HasValue: Boolean`, `AllowEmpty: Boolean`, `ClearValue()` — the unfilled Access date column.
- `PlaceholderText`, `Format = "dd.MM.yyyy"`, `CultureName = "ro-RO"`, `MinDate`, `MaxDate`.
- `ReadOnlyText: Boolean` — typing off; the whole face then opens the calendar.
- `ShowDropDownButton`, `ButtonWidth`, `GlyphSize`, `BorderWidth`, `CornerRadius` (logical px).
- The air, all four sides each, all in the designer: `Padding` (inherited, inside the outline),
  `TextPadding` (around the text), `ButtonPadding` (around the glyph) — see **The air** below.
- Passed to the drop-down: `ShowToday`, `ShowWeekNumbers`, `FirstDayOfWeek`;
  `DropDownCalendar: KBotCalendar` reaches the live one while it is open.
- Colours, `Empty` = theme, each with an `Effective*` counterpart: `BorderColor`,
  `FocusBorderColor`, `HoverColor`, `GlyphColor` (+ `BackColor`/`ForeColor`/`Font` with the pin
  flags).
- `Text` (the formatted value; assigning parses), `CommitText()`, `InnerTextBox: TextBox`.
- `ShowDropDown()`, `CloseDropDown()`, `IsDropDownOpen`, `ApplyTheme(scheme)`.
- Events: `ValueChanged`, `DropDownOpened`, `DropDownClosed`.

## Date-time, not just date
`Value` is a full `Date` and nothing in the field strips it. Give `Format` a time part —
`dd.MM.yyyy HH:mm`, `dd.MM.yyyy HH:mm:ss` — and the hour is shown, typed and read back; the
shorthand list carries time-bearing patterns too, tried before the date-only ones.

One rule keeps that honest: **the field never destroys a part of the value it does not display.**
With a date-only format, picking a day in the calendar or retyping the date keeps the hour that is
already in the value instead of silently zeroing it. When the format DOES show the time, what was
typed is what is meant — midnight included. `FormatHasTime(format)` is the pure test behind it
(quoted runs and `\` escapes skipped, so `dd.MM.yyyy 'ora'` is not mistaken for an hour).

The calendar in the drop-down stays a DAY surface — it has no clock. It hands back a day and the
field puts the hour back on.

## The air
Four nested paddings, every side editable in the property grid, all logical px @96dpi (C2):

| Property | What it moves |
|---|---|
| `Padding` (inherited, under *Layout*) | everything inside the outline: text AND button |
| `TextPadding` (default `8,0,8,0`) | the text. Top/bottom squeeze the strip the one line is centred in — that is how you sit the text high or low in a tall field |
| `ButtonPadding` (default `6`) | the drawn calendar inside the button strip |
| `GlyphSize` (default `14`, `0` = fill) | how big that calendar is; `0` lets it grow with the field |

`Padding` is left inherited on purpose rather than shadowed into our category: shadowing it would
cut it off from the framework's own `ShouldSerializePadding` and a padding nobody set would start
being written into the host `.Designer.vb` (C4).

## Typing
The text is real and editable. It is read back on Enter and on leaving the field, through
`Format` first and then the shorthands the operator actually types — `2.9.26`, `02092026`,
`2/9/2026`, `2026-09-02`, `02.09.2026 14:30` — and finally the culture's own reading. **Text that is not a date at
all puts the last good value back**: a half-typed date is not a value anybody can save. Esc does
the same on purpose. Up/Down nudge the date by a day.

`AllowEmpty = False` (the default) means clearing the text is refused the same way: the previous
date returns.

## The drop-down window
`Calendar/KBotCalendarPopup.vb` — a borderless form whose whole client area is one
`KBotCalendar`. It is built in code, opened under the field, and closes itself on a chosen day,
on Esc, or when it loses activation. It ACTIVATES (the calendar needs the keyboard), so the title
bar underneath reads as inactive while it is open — the same trade `CustomPopup` makes. Placement
reuses `CustomPopup.FitToWorkArea`, so it flips above the field or aligns to its right edge in
exactly the same cases. `KBotCalendarPopup.ClosedJustNow` is the 250 ms guard that stops the
second click on the button from reopening what that click just closed.

## The glyph colour is derived, not stored
`GlyphColor` left `Empty` does NOT read a colour slot: it is the field's own `ForeColor` pulled a
third of the way towards its own `BackColor`. A stored dim grey is only dim against the background
it was picked for — on a dark scheme, or on a field whose background was set by hand, it can land
on top of its own background and the button goes blank. Derived this way the glyph moves with
whatever the field is actually painted with. A pinned `GlyphColor` still wins and keeps winning
(C1) — which also means a colour pinned by accident in the property grid is exactly what makes the
button disappear; reset it to get the theme back.

## Limits
- No `ShowUpDown` spinner and no per-field caret editing (the whole text is one string, read back
  as a whole). Up/Down nudge whole DAYS even when the format shows a time — there is no
  hour-under-the-caret stepping.
- No checkbox in the face: emptiness is `AllowEmpty` + `HasValue`, not a `Checked` box.
- The text is single line and left aligned; there is no `TextAlign` and no multi-line.
- The frame is not selectable — Tab lands on the inner `TextBox` (the KBotTextField pattern), so
  a host wanting `KeyDown` should hook `InnerTextBox`.
- Setting a `Format` the framework rejects is logged and falls back to `dd.MM.yyyy` rather than
  leaving the field blank.
- **Never opened in the Visual Studio designer and never rendered on screen.**
