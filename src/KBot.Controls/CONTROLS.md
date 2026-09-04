# KBot.Controls — control docs index

One `.md` per control, next to its code. Each doc states what the control CAN do and
where it stops. Read the doc first; open the `.vb` only for the part you must change.

Conventions shared by every control are written ONCE here as `C1..C9`. A control doc
lists only its exceptions.

## Index

| Control | Doc | One line |
|---|---|---|
| `AdvancedTreeControl` | [Tree/AdvancedTreeControl.md](Tree/AdvancedTreeControl.md) | Owner-drawn tree / TreeListView, header + search + footer + collapse |
| `KBotDataView` | [DataView/KBotDataView.md](DataView/KBotDataView.md) | Owner-drawn virtualized grid, unbound, group/filter/sort/aggregate |
| `KBotChartView` | [Chart/KBotChartView.md](Chart/KBotChartView.md) | Time chart + single-select tab band |
| `KBotLaneView` | [Lane/KBotLaneView.md](Lane/KBotLaneView.md) | Placement surface: dated markers on draggable lanes, one time axis |
| `KBotNavList` | [NavList/KBotNavList.md](NavList/KBotNavList.md) | Sidebar / toolbar of keyed buttons, 3-state collapse, flyout |
| `CustomPopup` | [Popup/CustomPopup.md](Popup/CustomPopup.md) | Themed context menu window (icons, mnemonics, sliders) |
| `KBotToolTip` | [ToolTip/KBotToolTip.md](ToolTip/KBotToolTip.md) | Extender tooltip: header/body/footer, rich-text markup |
| `KBotCaptionBar` | [CaptionBar/KBotCaptionBar.md](CaptionBar/KBotCaptionBar.md) | Title bar for borderless forms + theme menu |
| `KBotChipBar` | [ChipBar/KBotChipBar.md](ChipBar/KBotChipBar.md) | Multi-select chip/filter bar with badges |
| `KBotComboBox` | [Combo/KBotComboBox.md](Combo/KBotComboBox.md) | Themed combo, list-only or typed (`Editable` + `LimitToList`) |
| `KBotCalendar` | [Calendar/KBotCalendar.md](Calendar/KBotCalendar.md) | Owner-drawn calendar: days / months / years, one zoom axis |
| `KBotDatePicker` | [Calendar/KBotDatePicker.md](Calendar/KBotDatePicker.md) | Date field, typed or picked, height NOT locked |
| `KBotRichTextEditor` | [RichText/KBotRichTextEditor.md](RichText/KBotRichTextEditor.md) | Rich-text surface: toolbar band + counter band, RTF and plain text |
| `KBotTextBox` | [TextField/KBotTextBox.md](TextField/KBotTextBox.md) | General text box, own border + own scrollbars |
| `KBotTextField` | [TextField/KBotTextField.md](TextField/KBotTextField.md) | Single-line form field with password eye |
| `KBotScrollBar` | [Scroll/KBotScrollBar.md](Scroll/KBotScrollBar.md) | Drawn scrollbar, `ScrollBar` value semantics |
| `KBotLabel` | [Label/KBotLabel.md](Label/KBotLabel.md) | Label with its own border colour/width/radius |
| `KBotProgressBar` | [Progress/KBotProgressBar.md](Progress/KBotProgressBar.md) | Determinate progress bar (0..Maximum) |
| `KBotBusyBar` | [BusyBar/KBotBusyBar.md](BusyBar/KBotBusyBar.md) | Indeterminate 3px activity bar |
| `KBotNotice` | [Notice/KBotNotice.md](Notice/KBotNotice.md) | Error / warning / success message box |
| `AdobeReaderHost` | [Adobe/AdobeReaderHost.md](Adobe/AdobeReaderHost.md) | PDF viewing: reparented Adobe window, or AcroPDF ActiveX |
| `ThemeEditorForm` | [ThemeEditor/ThemeEditorForm.md](ThemeEditor/ThemeEditorForm.md) | Per-control colour/font overrides (form, not a control) |
| `ThemeOptionsForm` | [ThemeOptions/ThemeOptionsForm.md](ThemeOptions/ThemeOptionsForm.md) | Edits the active scheme itself (form, not a control) |

Folder layout, reference direction and what does NOT belong here: [README.md](README.md).

## C1 — colour contract
`Color.Empty` (and `Nothing` for `Font`/`Image`/`Size`) = "take it from the active theme".
Anything set explicitly wins and keeps winning across scheme switches. Many controls also
expose `Effective*` read-only properties = the value actually painted.

## C2 — pixel metrics
Every public pixel number is a LOGICAL pixel at 96 dpi. Scaling happens at paint/layout
time from `DeviceDpi / 96` (`ThemeShapes.ScaleDpi`). Public getters stay logical — never
write a scaled value back into a public property or the next load scales it again.
Fonts are in points and scale themselves.

## C3 — no silent no-ops
Empty key, duplicate key, unknown key → `ArgumentException`. Impossible state (collapse a
non-collapsible bar, open an empty popup) → `InvalidOperationException`. Out-of-range
numeric values are CLAMPED, not thrown.

## C4 — designer serialization
Every settable `Color`/`Font`/`Size`/`Padding`/`Image` has `ShouldSerializeX` + `ResetX`,
including inherited `BackColor`/`ForeColor`/`Font`, answered from the control's own
"operator pinned it" flag. A freshly dropped control must emit ZERO property lines into
the host `.Designer.vb`. Verify with `TypeDescriptor.GetProperties(c)(name).ShouldSerializeValue(c)`.

## C5 — theming
Controls implement `IThemedControl`; `ApplyTheme(scheme)` repaints from
`ThemeManager.Current.Palette`. A control that owns child controls MUST implement it —
otherwise `ThemeManager.Traverse` recurses into the children and repaints them with the
generic per-type rules.

## C6 — design time
Under `KBotDesignTime.IsDesignTime`: timers do not run, nothing is logged, DPI scaling is
skipped (the VS surface is 96 dpi, so authoring at 150% is fine), and collection validation
is suspended between `BeginInit`/`EndInit`.

## C7 — errors
UI boundaries (`OnPaint`, mouse/key handlers, timer ticks) log to `GlobalErrorLog` and
swallow. Risky/boundary methods (I/O, interop, parsing) log and re-throw.

## C8 — hover labels
Always `KBotToolTip`, never `System.Windows.Forms.ToolTip`. Real child controls get
`SetToolTipText`/`SetToolTipHeader`; regions WE paint (header/footer icons, chart markers)
call `KBotToolTip.ShowAt` / `HideNow` from their own hover tracking.

## C9 — language
Identifiers, comments and doc text are ASCII English. Romanian, with diacritics, only in
strings the operator reads on screen.

## Status vocabulary used in the docs
- **screen-verified** — rendered and signed off on screen.
- **code-green** — unit tests pass; never rendered/exercised on screen.
- **not recorded** — no verification claim exists either way.
