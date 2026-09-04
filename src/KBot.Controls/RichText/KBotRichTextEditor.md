# KBotRichTextEditor

Themed rich-text editing surface: a `RichTextBox` between a toolbar band and a counter band.
The port of the `RTB` form from `VBA_DDF_INFO` (slice 0051), used by the DDF editor's
«Descriere» page for the long description of a revision.

`RichText/KBotRichTextEditor.vb` (+ `.Designer.vb`, `.Properties.vb`, `.Theming.vb`) ·
`UserControl` · Toolbox · `IThemedControl`, `IDpiScaledControl`
Helpers in the same folder: `KBotRichTextBand` (the two strips), `KBotNoFocusButton` (the
toolbar buttons, which also paint the scaled layouts), `RichTextImageKeyConverter` (the
`*ImageKey` drop-down), `RichTextImageLayout` (the `ButtonImageLayout` values).
Conventions: [C1..C9](../CONTROLS.md). Status: screen-verified (Classic / Dark / Modern,
folded, and with every metric moved off its default).

## Why it exists
`FX_DDF_REV` stores the long description TWICE — `Desc_Lunga` holds the RTF, `Desc_Lunga_ANSI`
the plain text — and both are still written, because the signed XFA document is fed from the
plain-text one and cannot take RTF control words. The control therefore has to produce both,
which is why it is a real rich-text surface and not a text box.

## Content
- `Rtf: String` — the RTF. The setter also accepts PLAIN text (anything not starting with
  `{\rtf`), because rows written before this editor existed hold plain text in that column.
  Malformed RTF falls back to plain text rather than throwing the operator's words away.
- `TextSimplu: String` (read-only) — the same content as plain text.
- `Editabil: Boolean` — read-only surface + disabled toolbar. The COLLAPSE button stays live:
  folding the editor is a way of looking at the form, not of changing the document.
- `TextBox: RichTextBox` (read-only) — for a host that needs Find / Select / Undo.
- `ContinutModificat` event — content changed by the OPERATOR. Loading through `Rtf` does not
  raise it.

## Header band
- `HeaderVisible: Boolean = True`, `HeaderHeight: Integer = 38`,
  `HeaderPadding: Padding = (4,4,4,4)`
- `HeaderSeparatorWidth: Integer = 1` + `HeaderSeparatorColor` — the baseline under the band,
  same names and same behaviour as `KBotDataView`'s header.
- `HeaderBackColor` — Empty = `SurfaceAltColor`.
- Buttons: `ButtonSize: Size = 30×30`, `ButtonSpacing: Integer = 2`,
  `ButtonPadding: Padding = Empty`.
- `GroupSpacing: Integer = 10` — the gap between the button group and the picker group.
- Pickers: `FontComboWidth = 186`, `SizeComboWidth = 76`, `ComboSpacing = 4`,
  `ComboHeight = 0` (0 = fill the band between its paddings), `ComboFont` (Nothing = the
  scheme's font).

Both pickers are `KBotComboBox`, so they theme themselves; assigning `ComboFont` pins the font
on them, which is what stops a later `ApplyTheme` from overwriting it.

Layout order, left to right: the five command buttons, `GroupSpacing`, the font picker, the
size picker. The collapse button is pinned to the RIGHT edge and the pickers are clipped
BEFORE it, so a narrow editor loses picker width rather than hiding the only control that can
bring the body back.

## Editing surface
- `EditorPadding: Padding = (4,4,4,4)` — the inset INSIDE the box. A `RichTextBox` ignores
  `Padding`, so this is applied to its formatting rectangle with `EM_SETRECT`.
- `EditorBackColor` / `EditorForeColor` — Empty = `InputBackColor` / `InputTextColor`. It is a
  field the operator types into, so it follows the INPUT colours, not the surface ones.
  Runs the operator coloured from the toolbar keep THEIR colour: that choice is stored in the
  RTF and belongs to the document.
- `EditorBorderWidth: Integer = 1` + `EditorBorderColor` (Empty = `InputBorderColor`) — the
  frame is painted by the control. `BorderStyle` on the inner box is `None` on purpose: the
  system border ignores the theme and stayed white on a dark scheme.
- `EditorFont` (Nothing = the scheme's font) — the BASE font of the document, not the font of a
  run.
- `ScrollBars: RichTextBoxScrollBars = Vertical`
- `EffectiveEditorBackColor` / `EffectiveEditorForeColor` (read-only) — what is actually painted.

## Footer band
- `FooterVisible = True`, `FooterHeight = 24`, `FooterPadding = (8,0,8,0)`,
  `FooterSeparatorWidth = 1` + `FooterSeparatorColor`, `FooterBackColor`,
  `FooterForeColor` (Empty = `TextDimColor`), `FooterItemSpacing = 16`, `FooterFont`.
- `FooterCharactersFormat = "{0:N0} caractere"`, `FooterWordsFormat = "{0:N0} cuvinte"`,
  `FooterSizeFormat = "{0:N1} KB"` — `{0}` is the number. A format `String.Format` cannot use
  falls back to the bare number rather than leaving the band empty.
- `CharacterCount` / `WordCount` / `SizeKilobytes` (read-only), `RefreshStatistics()`.

The size is the size of the **RTF**, not of the plain text: the RTF is what goes into
`Desc_Lunga`, so it is the number that says whether a description is getting out of hand.
Words are runs of non-whitespace — the plainest rule there is, so it agrees with a hand count.

The counters are recomputed on a 150 ms timer, never on the keystroke: building the RTF string
of a long description on every character typed would stall the form.

## Icons
- `Images: ImageList` — one shared source; the editor does not own it and never disposes it.
- Per button, an `*Image` (a picture from disk, landing in the host's `.resx`) and an
  `*ImageKey` (a key into `Images`, offered as a drop-down by `RichTextImageKeyConverter`):
  `Bold*`, `Italic*`, `Underline*`, `TextColor*`, `Highlight*`, plus `CollapseExpanded*` /
  `CollapseCollapsed*`.
- Resolution: the explicit picture wins, then the key, and with neither the button keeps its
  lettered glyph (B / I / U / A / ▨ / ▴ / ▾). A mistyped key shows a readable toolbar, not five
  blank squares. Text and picture never share a button.
- `ButtonImageLayout: RichTextImageLayout = Original` — how the pictures MEET the buttons, one
  property for the whole band (the operator binds one icon set, not six):
  - `Original` — the picture at its own size, placed by the button's `ImageAlign`. Drawn by the
    framework, so this is the toolbar's old look to the pixel.
  - `Stretch` — pulled to fill the button, aspect ratio ignored.
  - `Zoom` — the largest size that still fits, aspect ratio kept. What a 64×64 icon needs in a
    30×30 button, which `Original` would crop.
  - `Tile` — repeated from the top-left corner.

  The three scaled layouts are painted by `KBotNoFocusButton` itself — a `Button` can only put
  an image down at its own pixel size and there is no hook that skips just that step — inside
  what the flat border and `ButtonPadding` leave, and greyed while the button is disabled.
  The lettered glyphs are TEXT: with no picture bound there is nothing to lay out, so the
  property does not touch them.

### When the keys are turned into pictures
A key is resolved against `Images` **three times**, and the third one is the one that matters:

1. whenever a key, an `*Image` or `Images` itself is written;
2. when the bound list's contents are replaced wholesale (`ImageStream` loaded, `ColorDepth`
   or `ImageSize` changed) — the editor listens to its `RecreateHandle`;
3. **when the control gets its window handle** (`OnHandleCreated`).

The third is not belt and braces. A generated `.Designer.vb` creates the `ImageList` component
EMPTY, writes the editor's properties in alphabetical order — so `BoldImageKey` lands before
`Images`, and `Images` itself while the list is still empty — and only further down loads the
`ImageStream` and names the keys. With only (1), every button kept its lettered fallback at
runtime on a form whose designer showed the icons perfectly. Binding order is now irrelevant.

A host that ADDS pictures to an already-bound list while the form is on screen gets no signal
from `ImageList` at all; `RefreshButtonIcons()` is its way to ask for a re-read.

`ImageList` hands out a NEW bitmap on every read, so a picture that came from a key belongs to
the button and is freed when it is replaced or when the button dies; one assigned through an
`*Image` property belongs to the host and is never touched. Disposing the bound list drops
`Images` to `Nothing` and puts the letters back, rather than leaving pictures whose source is
gone.

## Collapse
Same contract as `KBotDataView`, `AdvancedTreeControl` and `KBotNavList`, deliberately:
- `CollapseButton: Boolean = False` — shows the button in the header. Turning it off while
  folded unfolds the editor first: without the button there is no way back.
- `Collapsed: Boolean` — RUNTIME state, never serialised. Setting it True without the button
  throws `InvalidOperationException`. `ToggleCollapse()` does not throw (it is a button press,
  not a request from code).
- `CollapsedChanged(collapsed)` event, `CollapsedHeight`, `ExpandedHeight`, `HostOwnsHeight`.
- Folded = the header band only; the body and the footer go away. If the height is NOT ours
  (docked, or anchored top+bottom) nothing is written to `Height` — the state changes, the
  event fires, and the host moves its splitter.

## Notes
- Every rectangle comes out of one `RebuildLayout` pass; nothing is positioned by a
  `TableLayoutPanel`. A metric changed at design time lands the same way as one changed at
  runtime.
- **Every published pixel number here is LOGICAL, at 96 dpi** (house rule C2) and is scaled once
  at layout time through `AppScaling`. `HeaderHeight = 40` is therefore 40 px on a 100 % screen
  and 60 px on a 150 % one — it is not the height in pixels, it is the height at 100 %. The
  designer surface always draws at factor 1, so a form authored on a 150 % machine shows the
  typed number and then grows by half at runtime. `AppScaling.Mode = Fixed100` puts the drawn
  geometry back on the typed numbers; the playground in `KBot.DevHarness` shows the logical
  value and the measured one side by side.
- Toolbar buttons never take the focus (`KBotNoFocusButton`). Without that, clicking "bold"
  would move the focus off the editor, the selection would collapse, and the command would
  apply to a caret instead of to the words the operator picked.
- The size picker shows the size the document ACTUALLY uses, adding it to the list in sorted
  position when it is not one of the twelve offered. A 11 pt base font at 110 % text size is
  12.1 pt, and an empty picker reads as broken. Sizes are written, read back and searched in
  the CURRENT culture, all in one place.
- Footer widths are measured with `TextRenderer`, not `Graphics.MeasureString`: a `Label` draws
  with GDI and the GDI+ measurement comes back narrower — which cost «151 caractere» its word
  on the first render.

## Limits
- No paragraph alignment, lists, indents, tables, images or hyperlinks — bold / italic /
  underline / text colour / highlight and the two pickers, which is what the Access original
  offered.
- No attachment button. The «Fisiere» page owns attachments; a second, half-wired way to add
  them would be a trap.
- No undo/redo buttons (Ctrl+Z still works), no find/replace UI, no print.
- The collapse axis is vertical only — there is no `CollapseDirection` here; a folded toolbar
  strip is the only shape that makes sense for an editor.
- The `Rtf` setter's plain-text fallback means a malformed RTF is shown as markup rather than
  refused.
