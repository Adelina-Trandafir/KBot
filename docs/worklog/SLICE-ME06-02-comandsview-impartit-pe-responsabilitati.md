# SLICE-ME06-02 - KBotAdmin: CommandsView.vb split by responsibility (19.11.2026)

Asked by Adelina right after ME06-01 («fa-o», about the 1100-line `CommandsView.vb`); sub-slice chosen by her. Code only moved: no line was
changed, added or removed apart from the partial wrappers. `KBotAdmin.vbproj` builds clean (0 errors, 0 warnings). Nothing run, nothing seen on screen.

## What changed

`KBotAdmin/Views/CommandsView.vb` (1100 lines) became eight files of the same class, none over 310 lines:

| File | Holds |
|---|---|
| `CommandsView.vb` | the class, state fields, constructor, `Attach` / `ActivatedAsync` / `SelectionChanged`, `ShowTarget`, `ShowSecondParam`, `ExplainCommand`, `CurrentCommand`, the command combo handler, `BtnRun_Click`, `BtnCopy_Click` |
| `CommandsView.Names.vb` | command names, block titles, `HiddenCommands`, `AppRoots`, `CommandApps`, `Expectations` |
| `CommandsView.Choices.vb` | `PayChoice`, `PartnerChoice`, `BonChoice` |
| `CommandsView.Partner.vb` | `ShowPartner`, `HidePartner` (ME06-01) |
| `CommandsView.Vercon.vb` | OI/MF emptying with its token, bon deletion (`FindBonsAsync`, `ShowBons`, `BtnDeleteBon_Click`) |
| `CommandsView.Move.vb` | ADECHIT payment move (list, analysis, move) |
| `CommandsView.Repairs.vb` | lanturi / DDF explain / fix / angajament deletion / generic repair buttons and their `Hide*` / `Configure*` |
| `CommandsView.Render.vb` | `RenderResult`, `AppendRow` |

The seven new files are registered in `KBotAdmin.vbproj` with `<Compile Update>` + `SubType Code` + `DependentUpon` (project rule for partials).
Checked with a script: every non-blank line of the old file is present in the new files, none lost.

## Unverified

- No behaviour could change (the compiler would refuse a missing member), but the form was not opened.
- Other files of the repo above the line limit were not looked at.
