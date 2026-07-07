# K-BOT — Form UI convention

Reference implementation: [`LoginForm`](../src/KBot.App/LoginForm.vb) +
[`LoginForm.Designer.vb`](../src/KBot.App/LoginForm.Designer.vb).
**Every WinForms form in the solution must follow the rules below.** New forms
should be built by copying LoginForm's structure, not hand-placing controls by
pixel coordinate.

## 1. Layout is built from docked `TableLayoutPanel`s

The form's body is a tree of `TableLayoutPanel`s (TLPs), each **docked**, never
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

`KBotTheme.ApplyTheme(Me)` walks the control tree and recolors it. Controls must
be flat for that to look right:

- Buttons: `FlatStyle = Flat`.
- ComboBox: `FlatStyle = Flat`, `DropDownStyle = DropDownList`.
- TextBox: `BorderStyle = FixedSingle`.
- Accent colors that have no `KBotTheme.CLR_*` constant are applied in code from
  theme-aware helpers (see `ApplyAccentColors` / `ClrError` in LoginForm), never
  hard-coded per control in the Designer.

Call order in `*_Load`: `KBotTheme.ApplyTheme(Me)` **first** (structural theming),
then apply the code-side accent colors.

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
