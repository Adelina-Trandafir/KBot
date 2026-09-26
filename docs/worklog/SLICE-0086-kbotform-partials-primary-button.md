# SLICE-0086 — KbotForm split into partial classes + «Angajament nou» as primary button

Operator request, 26.09.2026:

1. `KbotForm.vb` is monolithic (2766 lines). Split it into public partial classes by logic,
   none over ~500 lines.
2. `btnAngajamentNou` gets the primary button theme.

## What changed and why

**Split.** `KbotForm.vb` now holds only the fields, the constructor, the re-login net
(`WithReauth`, `IsContextMismatch`, `ContextMismatchError`), `MainForm_Load` and
`MainForm_Shown`. Everything else moved, unchanged in behaviour, into new partial files
(`Partial Public Class KbotForm`), one per concern:

| File | Lines | Contents |
|------|------:|----------|
| `KbotForm.vb` | 266 | fields, ctor, WithReauth, Load, Shown; a map of all partials in the class remarks |
| `KbotForm.Periods.vb` | 130 | year / SS / CodProgram combos, LastSS |
| `KbotForm.Views.vb` | 158 | left navigation, lazy `CreateView`, `ApplyViewGating`, `IsViewEnabled` |
| `KbotForm.Tree.vb` | 290 | `LoadTreeAsync`, `ReincarcaArborelePe`, `PopulateTree`, `TooltipFor`, node click, btnOpt, btnInfo, info window |
| `KbotForm.Receptii.vb` | 140 | receptie link editor, rebuild from FX_Istoric |
| `KbotForm.Ord.vb` | 333 | ORD editor entry points + refresh after an ORD write |
| `KbotForm.Ddf.vb` | 282 | DDF command dispatch, add / modify, «Angajament nou», Rezervari footer menu, `DeschideEditorulDdf` |
| `KbotForm.DdfDelete.vb` | 237 | the three DDF deletes, `DupaScriereaDdf`, `ActualizeazaPoartaDdf` |
| `KbotForm.Extrase.vb` | 142 | tree footer left icon, statements download + import |
| `KbotForm.Download.vb` | 231 | tree footer right icon (list), node right icon (whole angajament), the two "reuse from memory?" questions |
| `KbotForm.Ingest.vb` | 336 | two-phase ingest, save without the placement form, partial refreshes (receptii / rezervari), `AratEsecul` |
| `KbotForm.Console.vb` | 172 | FOREXE console, history, connect (both), show-browser, `SincronizeazaAsync` |
| `KbotForm.Chrome.vb` | 150 | `OnThemeChanged`, header/status paint, caption-bar options menu, `ShowLog` |

The existing partials (`Browser`, `DdfSend`, `ForexeWatch`, `TreeOptions`,
`UncorrectedOperations`) are untouched.

**Rule 0 / English-only sweep of the moved code.** Every Romanian comment and XML doc in the
moved code was rewritten in English without diacritics. Operator-visible strings
(`KBotMessage.Show`, `ArgumentException` messages, menu captions, `SpuneStare`) are unchanged,
diacritics included. No identifier was renamed except one local (`caleJurnal` → `logPath` in
`MainForm_Load`). The two stacked `<summary>` blocks above `IntreabaDacaRefolosescLista` were
separated, each now sits on its own function.

**Dead commented-out code dropped:** the old header «Conectare» button wiring
(`Forexe_StateChanged`, `ActualizeazaButonConectare`, the commented `OnFormClosed`, the
commented `AddHandler` in Load and the commented `ButtonStyles.ApplyPrimary(btnConectare, …)`).
The button no longer exists; connecting lives in the FOREXE footer band.

**Primary button.** `KbotForm.Chrome.vb` `OnThemeChanged` calls
`ButtonStyles.ApplyPrimary(btnAngajamentNou, scheme)`, so the button follows the accent colour of
the active scheme (rounded under Modern) on every theme switch. The designer is not touched.

## Pass 2 (same day) — DdfSend split + partials nested in Solution Explorer

**`KbotForm.DdfSend.vb` split** (558 lines, comments already English) by line ranges, no code
retyped:

| File | Lines | Contents |
|------|------:|----------|
| `KbotForm.DdfSend.vb` | 305 | the send (`TrimiteDdfAsync`), interrupted-create resume, stop notices, mode note, dry-run stop |
| `KbotForm.DdfSendMenu.vb` | 142 | Rezervari menu follow-ups: Definitiveaza / Deruleaza, final PDF, attachment read |
| `KbotForm.DdfSendHelpers.vb` | 152 | send API, codes, captures, grids, fresh reads, `ArataReviziaAsync` |

**Nesting.** Every partial of a form or control now has a
`<Compile Update="…"><DependentUpon>Main.vb</DependentUpon></Compile>` entry, so Solution
Explorer shows it as a leaf under the main file:

- `KBot.App.vbproj`: the 21 `KbotForm.*.vb` partials under `KbotForm.vb`.
- `KBot.Controls.vbproj`: 56 partials — `AdvancedTreeControl` (26), `KBotDataView` (17),
  `KBotLaneView` (3), `CustomPopup` (3), `KBotRichTextEditor` (2), `KBotTableLayoutPanel` (2),
  `KBotCalendar`, `KBotCaptionBar`, `KBotChartView` (1 each).

`*.Designer.vb` files are not listed (VS nests them already). Not touched, since they are not
forms or controls: `ApiClient.*.vb` (KBot.Api), `WorkflowModels.*.vb` (KBot.Forexe) and the
`_reference/KBOT_IPC.*.vb` snapshot (excluded from the build).

Build App (+ Controls): **0 warnings, 0 errors**. The nesting was not checked in Visual Studio.

## Files touched

- `src/KBot.App/KbotForm.vb` (rewritten, 2766 → 266 lines)
- new: `src/KBot.App/KbotForm.{Periods,Views,Tree,Receptii,Ord,Ddf,DdfDelete,Extrase,Download,Ingest,Console,Chrome}.vb`
- pass 2: `src/KBot.App/KbotForm.DdfSend.vb` (trimmed), new `KbotForm.DdfSendMenu.vb` /
  `KbotForm.DdfSendHelpers.vb`, `src/KBot.App/KBot.App.vbproj`, `src/KBot.Controls/KBot.Controls.vbproj`
- `docs/worklog/KBOT_STATUS.md`, this file

## Test results

- `dotnet build src\KBot.App\KBot.App.vbproj`: **0 warnings, 0 errors**.
- Method inventory: 79 method declarations across the 13 files, no duplicates (a method
  lost in the move would have been caught by the build if referenced; the `Handles` ones were
  counted by hand against the original).
- No test written or run (operator: no tests). Not seen on screen.

## Left unverified or deferred

- The primary look of «Angajament nou» was not rendered/seen on screen.
- Solution Explorer nesting not looked at in Visual Studio (only the build ran).
- Error-log tags still say `MainForm.*` (unchanged, as before the split).
- Nothing committed (operator does git).
