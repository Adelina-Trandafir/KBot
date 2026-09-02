# KBotCalendar

The calendar surface: a month grid, a year of months and a decade of years, all three drawn by
us so every pixel comes from the active scheme. Works on a form (dock it, stretch it) and is
also what [KBotDatePicker](KBotDatePicker.md) drops.

`Calendar/KBotCalendar.vb` + `Calendar/KBotCalendar.Painting.vb` · `Control` · sealed · Toolbox ·
`IThemedControl`
Conventions: [C1..C9](../CONTROLS.md). Status: slice 0050, **never seen on screen**.

## Why not `MonthCalendar`
The stock control is a native window painted by Windows: `BackColor`/`ForeColor` reach only part
of it, the header keeps the system colours, and on a dark scheme it stays a white card — the same
reason `ComboBox` became `KBotComboBox`. It also refuses most sizes (it snaps to whole month
tiles), so it cannot be docked or stretched into the space it was given.

## The zoom axis
The header title is a button: it zooms OUT (days → months → years). Picking a cell zooms back IN.
Only a pick in the day view produces a value. The arrows page by month, year or decade depending
on the view — so does the mouse wheel, and PageUp/PageDown.

## API
- `Value: Date` — the selected day, clamped to `MinDate`/`MaxDate`; writing it pages to its month.
- `DisplayMonth: Date`, `View: KBotCalendarView` (`Days`/`Months`/`Years`) — the page, runtime only.
- `MinDate`, `MaxDate` — cells outside the range are drawn dim and do not answer clicks.
- `CultureName: String = "ro-RO"`, `Culture: CultureInfo` (read-only), `FirstDayOfWeek = Monday`,
  `TodayFormat = "dd.MM.yyyy"`.
- `ShowToday`, `ShowWeekNumbers` (ISO weeks), `ShowTrailingDays`, `HighlightWeekend`.
- `HeaderHeight`, `DayNamesHeight`, `FooterHeight`, `BorderWidth`, `CornerRadius`,
  `CellCornerRadius`, `CellGradient` (0..100, 0 = flat) — logical px @96dpi (C2).
- The air, every side editable in the designer: `Padding` (inherited), `HeaderPadding`,
  `GridPadding`, `CellPadding`, `FooterPadding` — see **The air** below.
- Colours, all `Empty` = theme, each with an `Effective*` counterpart: `BorderColor`,
  `HeaderBackColor`, `HeaderForeColor`, `ArrowColor`, `DayNameColor`, `WeekNumberColor`,
  `TrailingForeColor`, `WeekendForeColor`, `SelectionBackColor`, `SelectionForeColor`,
  `HoverColor`, `TodayColor`, `GridColor`, `FooterForeColor` (+ `BackColor`/`ForeColor`/`Font`
  with the pin flags).
- `NaturalSize: Size` (read-only), `GetPreferredSize(...)` — what the drop-down is sized from.
- `StepPage(delta)`, `ZoomOut()`, `GoToToday()`, `ApplyTheme(scheme)`.
- Events: `ValueChanged`, `DateSelected`, `ViewChanged`, `DisplayMonthChanged`.

## Two events, on purpose
`ValueChanged` fires on every move, arrow keys included. `DateSelected` fires only when the
operator CHOSE — a click on a day, Enter, or the today row. A drop-down closes on the second and
never on the first, otherwise the first arrow key would shut it.

## Keyboard
Arrows move by one unit (day / month / year), Up-Down by a whole row, PageUp/PageDown page,
Home/End jump to the first and last day of the month, Enter chooses (or zooms in, above the day
view), Backspace zooms out.

## The air
Five paddings, nested outwards in, every side its own number, all logical px @96dpi (C2):

| Property | Default | What it moves |
|---|---|---|
| `Padding` (inherited, under *Layout*) | `0` | everything inside the border: header, day names, grid, today row |
| `HeaderPadding` | `0` | the arrows and the title INSIDE the header band — the painted band still spans the full width |
| `GridPadding` | `0` | the day-name strip and the cells together, so the headings stay over their columns |
| `CellPadding` | `2` | the gap between a cell and its coloured tile (selection, hover, the ring around today). The number stays centred on the whole cell |
| `FooterPadding` | `0` | the text in the today row, not the band or its hover fill |

`GridPadding` is taken ONCE, before the day-name strip is split off, which is what stops the
headings and the columns underneath them from drifting apart. `NaturalSize` counts the outer and
grid air, so a drop-down sized from it is not squeezed by exactly the amount you asked to be left
empty. `Padding` is left inherited rather than shadowed into our category: shadowing it would cut
it off from the framework's own `ShouldSerializePadding` and a padding nobody set would start
being written into the host `.Designer.vb` (C4).

## Wording
Month and day names come from `CultureName`, which defaults to **`ro-RO`, not to the machine
culture**: the operator must read Romanian months on any workstation. The today row says
`Astăzi: <date>`. These are the only Romanian strings in the family — everything else is ASCII
English (C9).

## Limits
- **One date, no range.** No `SelectionStart`/`SelectionEnd`, no multi-select, no
  "bold these days" list.
- **One month per page.** `MonthCalendar`'s `CalendarDimensions` (2x2 months at once) has no
  equivalent — the grid always draws the one month in `DisplayMonth`.
- **No time-of-day, and that is deliberate**: `Value` is a day, always with the time stripped —
  there is no clock on the surface. `KBotDatePicker` is the one that carries a date-time, and it
  puts the hour back on the day the calendar hands it.
- The week numbers are ISO (`ISOWeek.GetWeekOfYear`), which is not settable.
- Layout is recomputed on size/font/view changes and cached; nothing else re-measures per paint.
- **Never opened in the Visual Studio designer and never rendered on screen** — the colours,
  the geometry and the DPI behaviour are argued from the code, not from a screenshot.
