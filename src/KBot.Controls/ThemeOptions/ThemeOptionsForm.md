# ThemeOptionsForm — "Opțiuni temă…"

A tool WINDOW (not a control) that edits the SCHEME itself: its ~23 colour slots, the style
options (button rendering, corner radius, base font, spacing) and, in a separate panel, the
application scaling.

`ThemeOptions/` — `ThemeOptionsForm.vb` (+ `.Designer.vb`), `SchemeOptionsProxy.vb`.
Opened from the caption bar's theme menu
([KBotCaptionBar](../CaptionBar/KBotCaptionBar.md), `ShowThemeOptions`).
Conventions: [C1..C9](../CONTROLS.md). Status: not recorded.

Not [ThemeEditorForm](../ThemeEditor/ThemeEditorForm.md): that one puts exceptions on named
controls of one window; this one changes what "Modern" means for every window at once.

## API
- `New(host As Form)`, `ShowFor(host) As ThemeOptionsForm` (Shared) — modeless, owned by the
  host; an editor already open for the same host is brought to the front (two windows on one
  scheme would edit the same object from two places).
- `SchemeOptionsProxy(scheme, onChanged)` — the property-grid facade over a `ThemeScheme`:
  `Scheme`, `Nume` (read-only — it is also the file key), `Intunecata`, the colour slots
  (`Suprafata`, `SuprafataAlt`, `TextCuloare`, `TextEstompat`, `Contur`, `FundalCamp`,
  `TextCamp`, `ConturCamp`, `FundalButon`, `ConturButon`, `ButonHover`, `ButonApasat`,
  `TextButon`, `Accent`, `TextPeAccent`, `AccentHover`, `AccentFila`, `FilaInactiva`,
  `Eroare`, `Succes`, `Avertisment`, `InelFocus`, `TextDezactivat`), and the style options
  (`CuloriDeSistem`, `PastreazaCuloriDesigner`, `ControalePlate`, `RandareButoane`
  (`ButtonRenderStyle`: `System` | `Flat` | `ModernOwnerDrawn`), `RazaColt`,
  `AccentPeFocus`, `BaraTitluIntunecata`, `DeseneazaFilele`, `FontDeBaza`,
  `DimensiuneFont`, `Spatiere`).
- `InstalledFontNameConverter` — the installed-font dropdown for `FontDeBaza`.

## Rules
- **Picking a scheme from the list also ACTIVATES it.** The editor shows its effect on the
  windows behind, so what you edit must be what you see.
- **Immediate effect, explicit save.** Every touched value shows at once, but nothing
  reaches disk until "Salvează" — then the scheme is written to
  `…\AVACONT\Themes\<Nume>.json` and from the next start THAT is "Modern".
  "Restaurează implicit" deletes the file and restores the compiled scheme; source code is
  never touched, so there is no one-way door.
- **Scaling is not part of the scheme** — it writes separately to `theme.json`. It is a
  property of the operator's screen; inside a scheme, switching Modern → Dark would silently
  resize the application.
- `Intunecata` only tells the engine the scheme is dark (DWM title bar, the "DarkMode"
  variant of system lists/combos). It changes no palette colour by itself.

## Limits
- `Nume` cannot be changed here: it is the persistence key.
- `CuloriDeSistem = True` (how "Classic" is built) makes the engine paint nothing from the
  palette — the colour slots above then have no effect on ordinary controls.
  `PastreazaCuloriDesigner = True` (how "Colorat" is built) puts the designer-authored
  colours back instead of writing the palette; K-BOT controls still take their internal
  colours from the palette.
- `AccentFila` / `FilaInactiva` are visible only with `DeseneazaFilele` on; `InelFocus` only
  with `AccentPeFocus` on; `BaraTitluIntunecata` only on windows WITH a system frame.
- A missing font falls back gracefully to the default; `DimensiuneFont = 0` means "do not
  touch form fonts".
- A stored `…\Themes\<Nume>.json` silently REPLACES the compiled scheme — check it before
  concluding a colour change in code did nothing.
