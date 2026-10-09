# KBotHtmlLabel

A `KBotLabel` (border, theme colours, `ForeColor`) whose text is a small HTML subset (slice 000T-06). Used
by the tutorial / tour bubble (`HelpTourBubble.lblText`).

`RichText/KBotHtmlLabel.vb` (the control) · `RichText/KBotHtmlText.vb` (the reader, `Public Module KBotHtmlText`
+ `Friend HtmlRunBuilder`) · reuses `ToolTip/KBotRichText.vb` for wrapping and drawing.
Conventions: [C1..C9](../CONTROLS.md). Status: builds; **never run, never seen on screen**.

## Use
- `Html: String` — the text. `Text` stays empty on purpose (the base label then draws nothing but its
  background and border). Serialised only when set.
- `MeasureHtml(maxWidth)` (slice 0112) — the size the text needs wrapped at `maxWidth`, as wide as its widest line (a dialog that hugs its message).
- `GetPreferredSize(proposed)` — the proposed width (else `MaximumSize.Width`, else `Width`) and the height the
  text needs there. A host that sizes itself to its text (the bubble's `FitToText`) calls it.
- `KBotHtmlText.LooksLikeHtml(text)` / `ToRuns(...)` are public and pure (no screen): text in, runs out.

## What is read
- **Tags:** `b strong i em u s strike del small big mark code tt kbd span font div p br h1..h6 ul ol li
  blockquote`. `style script head title` are skipped with their content; comments vanish; any other tag is
  dropped and its text kept.
- **Style (on any tag, `style="..."`):** `color`, `background-color`, `font-weight`, `font-style`,
  `text-decoration`, `font-size` (px, pt, em, rem, %, keywords), `font-family` (only monospace or not);
  `<font color size>`.
- **Colours:** `#rgb`, `#rrggbb`, `rgb()` / `rgba()`, a .NET colour name, or a theme word
  (`accent dim warning error success text`) taken from the active scheme at paint time. `mark` is the scheme's
  warning colour thinned into the surface.
- **Whitespace** collapses like a browser's; a break is a `br` or a block tag (`div` one line, `p` / `h*` /
  lists a small gap). `&nbsp;` and the other entities are decoded.
- **Plain text:** a text with none of those tags is shown as written (its line breaks are breaks, `&` stays
  `&`); a blank line keeps a full line's height.

## Limits
- No tables, images, links, alignment or font names; list items have a marker («•» / «1.») but a wrapped line is
  not indented (`KBotRichText` has no hanging indent).
- Underline / strike-through are font styles, so the space between two underlined words is not underlined.
- Text is drawn with GDI+ (`KBotRichText`), so it can differ by a pixel from a plain `Label` next to it.
- The runs are rebuilt on text / font / `ForeColor` / scheme change; a scheme switch that leaves the palette
  object and `ForeColor` the same would keep the old theme-word colours until the text changes.
