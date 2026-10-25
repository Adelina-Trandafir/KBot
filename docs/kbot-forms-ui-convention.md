# K-BOT — Form UI convention

Reference implementation: [`LoginForm`](../src/KBot.App/LoginForm.vb) +
[`LoginForm.Designer.vb`](../src/KBot.App/LoginForm.Designer.vb).
**Every WinForms form in the solution must follow the rules below.** New forms
should be built by copying LoginForm's structure, not hand-placing controls by
pixel coordinate.

## 1. Layout is built from docked `TableLayoutPanel`s

The form's body is a tree of the house [`KBotTableLayoutPanel`](../src/KBot.Controls/Table/KBotTableLayoutPanel.md) (TLPs; 0066), each **docked**, never
positioned by absolute `Location`/`Size`. A form resizes and DPI-scales correctly
only when every region is a docked TLP.

- The outermost container docks `Fill`.
- Header/caption regions dock `Top`.
- Status/progress regions (e.g. `pbBusy`) dock `Bottom`.
- The main content TLP docks `Fill` so it absorbs the remaining space.

In LoginForm:

```
LoginForm (Form)
├─ pbBusy            Dock = Bottom      (marquee progress)
└─ pnlCard (Panel)   Dock = Fill        (padded card)
   ├─ pnlCaption (TLP)  Dock = Top      title + subtitle, 2 rows
   └─ pnlCreds  (TLP)   Dock = Fill     labels/inputs grid
      └─ pnlUnit (TLP)  Dock = Fill     phase-2 unit picker, nested
```

Rows/columns are sized with `RowStyles`/`ColumnStyles` (`Percent` for elastic
regions, `Absolute` for fixed-height rows like a 48px input row). Spacers are
just short `Absolute` rows, not empty labels.

## 2. Every child control is docked inside its logical TLP

Controls do **not** carry meaningful `Location`/`Size` — they are placed in a
TLP cell via `Controls.Add(ctrl, col, row)` and set `Dock = Fill` (use
`SetColumnSpan`/`SetRowSpan` for controls that span cells, e.g. a full-width
button). The cell — not the control — owns the geometry. This keeps alignment
stable across themes, fonts and DPI.

## 3. Everything is flat so the theme can paint it

The shared colour/theming rules for custom K-BOT controls are maintained in [CONTROLS C1/C5](../src/KBot.Controls/CONTROLS.md#c1--colour-contract): theme defaults unless explicitly pinned; IThemedControl.ApplyTheme(scheme) paints from ThemeManager.Current.Palette. A control owning children implements IThemedControl so generic traversal does not repaint its internals.

Historical LoginForm pattern: KBotTheme.ApplyTheme(Me) walked the control tree; stock buttons and ComboBox used FlatStyle.Flat (ComboBox also DropDownList), TextBox used FixedSingle. Accents without KBotTheme.CLR_* came from theme-aware ApplyAccentColors/ClrError helpers, never per-control Designer literals. In *_Load the structural KBotTheme.ApplyTheme call preceded those accents. These legacy API names describe that example; they do not replace the shared contract for custom controls.

## 4. All procedures start with an UpperCase letter

Every `Sub`/`Function` — including event handlers — uses PascalCase:
`ShowPhaseCreds`, `SetBusy`, `ApplyAccentColors`, `BtnContinue_Click`,
`BtnLogin_Click`, `LoginForm_Load`. (Designer-wired handlers still use
`Handles ctrl.Event`; only the method name is PascalCased.)

## 5. Controls are declared in `*.Designer.vb`

Per the standing rule, all controls are declared and built in
`InitializeComponent` inside `*.Designer.vb` so the form renders at design time.
No control is `New`-ed up in the code-behind.

## 6. Layout changes go in the Designer, not a runtime helper

If a change belongs to the Designer — moving/re-parenting/re-docking a control,
resizing, adding a control — make it **in `*.Designer.vb` directly**. Do not write
a code-behind method that mutates the layout at `Load` time to work around the
Designer. Code-behind is for behavior (event handlers, phase logic, data binding),
not for relocating controls the Designer owns.

## 7. No comments in `*.Designer.vb`

Do not add explanatory comments to any form's `.Designer.vb`. Keep only the default
comments Visual Studio generates (the `' controlName` separators). Anything that
needs explaining goes in the code-behind or here.

## 8. Custom K-BOT controls are declared and configured in the Designer too

`AdvancedTreeControl` and `KBotDataView` are Toolbox controls (`<ToolboxItem(True)>`),
so the standing rule (§5/§6) applies to them exactly like stock controls: **declare
the instance in `*.Designer.vb` and set its initial/static properties there**, via the
property grid — not by `New`-ing it and configuring it in code-behind.

- **Initial/static props go in the Designer**: appearance and fixed behavior —
  `ItemHeight`, `Indent`, `RootExpander`, `CheckBoxes`, `HeaderVisible`,
  `HeaderCaption`, `ScrollBarThickness`, the color properties, `TreeListView`/
  `DynamicColumns`/`ColumnsLevel`, etc. (These now carry `<Category>`/`<Description>`/
  `<DefaultValue>` so the grid is self-documenting.)
- **Code-behind is for runtime data only**: building the node tree / rows, wiring event
  handlers, resolving image keys against the runtime icon cache, and anything driven by
  the session or API response. Runtime-only members (`SelectedNode`, `Items`, resolved
  header `Image`s, …) are `<Browsable(False)>` and never appear in the grid by design.

## 9. Every form and UserControl autoscales by DPI, never by font (slice 0066-02)

`AutoScaleMode = Dpi` and `AutoScaleDimensions = (d, d)`, where `d` is the dpi of the screen the file was last saved on (`96`, `120`, `144`). Visual Studio writes that stamp and every coordinate together; they change as a pair. `AutoScaleMode.Font` is forbidden: its integer-font-metric ratios differ by axis and DPI. The [0066-02 evidence](worklog/SLICE-0066-tabele-la-dpi-ca-arborele.md#slice-0066-02) keeps the exact measurements and the correction. Dpi uses `DeviceDpi / d`; `AppScaling.ApplyZoom` adds one uniform operator/text zoom to geometry and fonts.
