# KBotToolTip

The K-BOT floating label: an `IExtenderProvider` component with a header band (left icon +
title), a rich-text body and a footer band. Themed, rounded, per-control styles.
Never use `System.Windows.Forms.ToolTip` — it cannot be themed or given bands (C8).

`ToolTip/` — `KBotToolTip.vb` (+ `KBotToolTipContent`), `KBotToolTipStyle.vb`
(+ `KBotToolTipBand`, `KBotToolTipSeparator`), `KBotToolTipWindow.vb`, `KBotRichText.vb`
`Component` · `IExtenderProvider`
Conventions: [C1..C9](../CONTROLS.md). Status: code-green (slice 0035), never seen on screen.

## Two ways to use it
1. **Real child controls** — drop the component in the form's `components` and set
   `SetToolTipText` / `SetToolTipHeader` / `SetToolTipFooter` / `SetIconFor` in
   `InitializeComponent` (Romanian, like every operator-facing string).
2. **Regions WE paint** (tree/grid header icons, chart markers — painted areas, not
   `Control`s, so there is nothing to extend): call
   `ShowAt(owner, KBotToolTipContent, screenPos)` and `HideNow()` from the hover tracking
   the control already has.

## Component API
- `Style: KBotToolTipStyle` (read-only) — the default look.
- `Active = True`, `InitialDelay = 500` ms, `AutoPopDelay = 8000` ms (0 = only on mouse-out)
- Extender: `Get/SetToolTipText`, `Get/SetToolTipHeader`, `Get/SetToolTipFooter`,
  `SetIconFor(ctrl, icon)`, `Get/SetStyleFor(ctrl, style)`, `CanExtend(o)`
- `ShowAt(owner, content, screenPos)`, `HideNow()`

**A content object may be REWRITTEN between calls, and it is.** Every drawn-region caller
owns one `KBotToolTipContent` and rewrites its fields before each `ShowAt`, so what decides
"this label is already up" is the TEXT, not the object identity. A guard on the reference
alone stuck the label on the first thing a control ever showed: on the lane surface, moving
from one marker to another inside the same lane left the previous marker's name standing, and
only leaving the whole control put it right. Two consequences, both deliberate:
- a request whose text differs from what is on screen always goes through, same object or not;
- when the label is already open, the swap happens **immediately**, without the initial delay
  — a second delay would leave one thing's name standing over another for half a second.

**Different looks on one form**: `SetStyleFor(ctrl, Style.Clone())` — the style is a VALUE,
not a component. Do not widen a control's internal tooltip object.

## KBotToolTipContent
`Text` (body), `HeaderText`, `FooterText`, `HeaderIcon`, `FooterIcon`, `Style`.
`Nothing` on any field = "take it from the style", so one caller can change only the title
and another only the icon.

## KBotToolTipStyle
`BackColor`, `ForeColor`, `BorderColor` (Empty = theme, C1), `BorderWidth = 1`,
`CornerRadius = 6`, `Font` (unset = the requesting control's font), `Padding`,
`MaxWidth = 420` (wraps beyond it), `Header` / `Footer` (`KBotToolTipBand`), `Separator`
(`KBotToolTipSeparator`), `Clone()`. All px are logical (C2).

- `KBotToolTipBand`: `Kind`, `Visible = True`, `Text`, `Font` (unset = body font; the header
  bolds itself), `ForeColor`, `BackColor` (Transparent = the label background),
  `TextAlign = MiddleLeft`, `Icon`, `IconSize`, `IconGap = 6`, `Padding`, `HasContent`.
- `KBotToolTipSeparator`: `Visible = True`, `ForeColor`, `Width = 1`, `Inset = 0`,
  `Margin = 4`, `IsDrawn`. Drawn ONLY between two sections that are both visible.

## KBotRichText — the markup engine (`Public Module`, pure)
Markup: `<b>`, `<i>`, `<u>`, `<color=#RRGGBB>`, `<back=#RRGGBB>`, each closed by its pair.
An UNRECOGNISED tag stays on screen as literal text, so a typo is visible instead of
vanishing. API: `Parse(rawText, baseFont, baseColor)` → `List(Of RichRun)`;
`Layout(runs, g, maxWidth)` → `RichLayout` (lines + size); `Draw(g, layout, bounds, align)`;
`DisposeDerivedFonts(runs, baseFont)`.

## Limits
- Sizes come out in the DPI of the `Graphics` you pass — the engine scales NOTHING; the
  caller must already have picked the right font (C2).
- Derived fonts are created by `Parse`: the caller MUST call `DisposeDerivedFonts` (the base
  font is borrowed and is never disposed).
- The tree has its OWN parser (`AdvancedTreeControl.ParseRichText`, `Friend Shared`, tied to
  its internals). Same markup, separate code — do not route one through the other.
- No links, no images inside the body, no per-run alignment, no tail/arrow pointing at the
  target.
- `KBotToolTipWindow` is `WS_EX_NOACTIVATE`: the label never takes focus and cannot be
  interacted with.
