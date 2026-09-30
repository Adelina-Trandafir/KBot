# K-BOT help system — maintainer guide (slice 0000)

Read this before touching anything under `src/KBot.App/HelpContent/` or `src/KBot.App/Help/`.
It is written for the next thread: after the operator changes or adds a feature, you should be able
to bring the help up to date from this file alone.

Status and history: [worklog/state/KBOT_STATUS_0000-0009.md](worklog/state/KBOT_STATUS_0000-0009.md)
(section «Slice 0000»). Authoring syntax in detail: [../src/KBot.App/HelpContent/README.md](../src/KBot.App/HelpContent/README.md).

---

## 1. What it is

- **F1 anywhere** (any K-BOT window) and the **«?» button** on every caption bar open the help
  window on the topic of the control under focus.
- The **help window**: contents tree, search (ignores diacritics), Back / Forward, guided tours,
  «Exportă manualul...» (one HTML file, printable to PDF).
- **Guided tours**: a coloured ring around a control plus a bubble with Înapoi / Înainte / Închide.
- **Three parts**, and who sees them:

  | Part | `part:` | Who sees it |
  |------|---------|-------------|
  | 1 «Contabil» | `contabil` | every non-director user |
  | 2 «Opțiuni avansate» | `avansat` | non-director users, only while the advanced options are on |
  | 3 «Director» | `director` | only a login with role `Director` (and it sees ONLY this part) |

- **Screenshots** are taken by the operator with the capture tool (below), never by Claude.

## 2. Where everything is

| What | Where |
|------|-------|
| Topics (Markdown, one file = one topic) | `src/KBot.App/HelpContent/<part>/**/*.md` |
| Guided tours | `src/KBot.App/HelpContent/tours/*.md` |
| Pictures | `src/KBot.App/HelpContent/img/<capture-id>.png` |
| Authoring syntax (headers, captures, source tags, tours) | `src/KBot.App/HelpContent/README.md` |
| Static checker | `tools/HelpCheck/Check-Help.ps1` |
| Engine | `src/KBot.App/Help/` (below) |
| Seam used by the controls | `src/KBot.Theming/KBotHelp.vb` (`IKBotHelpProvider`) |
| «?» on caption bars | `src/KBot.Controls/CaptionBar/KBotCaptionBar.HelpButton.vb` |
| Capture mode switch | `AppSettings.HelpCaptureMode`; Setări › Aplicație «Mod capturi pentru ajutor» (visible only with advanced options) |
| Capture menu + navigation for captures/tours | `src/KBot.App/KbotForm.HelpCapture.vb` |

The build copies `HelpContent\**` to `<AppDir>\Help\` (`KBot.App.vbproj`, `None Include ... Link=Help\...`).
The updater writes only the files in its package, so pictures shot on a client PC in
`C:\KBOT\Help\img\` survive updates.

### Engine files (`src/KBot.App/Help/`)

| File | Job |
|------|-----|
| `HelpService.vb` | installs the provider + the F1 key filter; who sees which part (`VisibleParts`); F1 → topic; `Navigate` for captures and tours; manual export |
| `HelpLibrary.vb` | loads and checks the topic files; tree, search, `FindByScreen` |
| `HelpTopic.vb` | `HelpPart` enum + `HelpTopic` |
| `HelpHtml.vb` | Markdown → HTML (Markdig), pages, the manual; capture tags → pictures / «Imagine lipsă» |
| `HelpForm.vb` | the help window |
| `HelpCapture*.vb`, `IHelpCaptureNavigator.vb` | capture tags, the capture list («Fă poza» / «Încarcă»), the screen freeze + rectangle, saving |
| `HelpTour*.vb` | tour parsing, runner, ring, bubble |

## 3. How F1 finds a topic

F1 walks from the focused control up through its parents. At each step it offers
`TypeName.controlName` and `TypeName`; the first key that a VISIBLE topic lists in `screens:` wins
(parts in order). Nothing found → the help start page. The Debug build shows the keys it tried
in the help window's bar — copy the right one from there.

Inner pages (a tab page inside a window) need no key of their own when their window has one.

## 4. PROCEDURE — bringing the help up to date after a change

Use this when the operator says «update the help» (or when your own task changed something the
operator sees and you are asked to cover it).

1. **Find what changed since the help was last brought up to date.** The watermark is in
   `state/KBOT_STATUS_0000-0009.md` («Ajutorul e la zi până la»). Read the `KBOT_STATUS.md` index
   rows after it, and the Open threads line «Ajutor de actualizat» (feature slices leave notes
   there). Open only those slices' worklogs.
2. **Map each change to topics.** Run
   `powershell -NoProfile -ExecutionPolicy Bypass -File tools\HelpCheck\Check-Help.ps1 -Map -Coverage`
   for the topic tree, then grep `HelpContent` for the window's type name (in `screens:`) and for
   the old captions / messages the change touched.
3. **Get the facts from the code, not from the plan.** Captions and tooltips: the window's
   `.Designer.vb` (`.Text`, `SetToolTipHeader/Text`, `HeaderText`). Messages: `KBotMessage.Show`
   strings. States: `DdfRevisionStates.Label`. Quote on-screen text exactly, in «».
4. **Edit the topics:**
   - **every section you write or change gets the slice that decided it** in its hidden
     `<!-- slice: ... -->` tag (under the `##` heading; the header tag for the text before the
     first `##`; tour steps too). Syntax: `HelpContent/README.md` «Source tags»;
   - changed behaviour → rewrite the paragraph; renamed button → rename it everywhere
     (`grep` the old caption);
   - a new window or view → a new topic (or a section in the nearest one) AND its type name in
     `screens:`; `-Coverage` must print «(none)»;
   - a renamed / removed control → fix `screens:` and tour `target:` (the checker catches it);
   - a new header-menu item, view or Setări page used by `goto:` → also add it to the lists in
     `HelpContent/README.md` (and to `KbotForm.HelpCapture.vb` if it is a new KIND of target).
5. **Pictures.**
   - A new screen worth showing → a new capture tag where the picture belongs (unique id).
   - A screen whose look changed → keep the tag; list its id in the worklog under «Capturi de
     refăcut» so the operator re-shoots it («Refă» in the capture list).
   - Never delete or overwrite files in `img/` — they are the operator's.
   - A window the capture list cannot reach (director's window, dialogs that appear only in a real
     situation) → no `goto:`, and a `prepare:` that says when / where to shoot it.
6. **Tours:** if a step's target moved or its text is now wrong, fix the step.
7. **Check + build:** `Check-Help.ps1 -Coverage` → «No errors.» and coverage «(none)», then
   `dotnet build src\KBot.App\KBot.App.vbproj` (0 warnings, 0 errors). Don't run the app to look
   at the help unless asked.
8. **Record it** as the next sub-slice `0000-NN` (never a new slice number):
   worklog `docs/worklog/SLICE-0000-NN-<slug>.md` (which topics, which capture ids are new / to
   re-shoot, what the operator must read), a row in `state/KBOT_STATUS_0000-0009.md`, move the
   watermark, clear the «Ajutor de actualizat» notes you handled, and update the index line in
   `KBOT_STATUS.md` («0000-01…NN GATA»).

### What a feature slice does (when it is NOT a help task)

**Rule (operator, 30.09.2026): ANY change to the visual experience must also be recorded in the
help and must reference the slice number.** So a feature slice that changes what the operator
sees updates the help itself, as a `0000-NN`, with its own slice number in the source tags.
Only when that cannot be done in the same task: in its worklog under «left unverified or deferred», and in the 0000 Open threads line «Ajutor de
actualizat», name the topic ids (and capture ids) the change makes stale. One line is enough, e.g.
`0098: contabil.vederi.plati (noua coloana «Cont»), captura plati de refacut`.

## 5. Writing rules

- **Romanian, «tu» form, plain words**, short sentences. Write for an accountant, not a developer.
- On-screen text exactly as the operator reads it, in «» or **bold**.
- Say what to do and what happens; one fact once — link to the topic that owns it
  (`[text](topic:id)`) instead of repeating it.
- Headers ASCII (rule 0); only `title` and the body carry diacritics.
- **Left out on purpose** (operator's decisions):
  - how the custom controls work inside, the browser's internal logic;
  - **how K-BOT works inside** (operator, 30.09.2026: «the help is meant for users only»): keys
    K-BOT presses for the user (Ctrl+H, Ctrl+2...), how it places or watches the Adobe window,
    log lines, timers, internal checks. Describe only what the user sees and does;
  - the capture mode itself (it is a tool for the operator, not for clients);
  - the clicks inside the FOREXE page — the help describes what K-BOT does, not FOREXE's own pages;
  - the ORD's next step after signing (turned into a recepție and uploaded to the FOREXE CAB
    server): K-BOT does not do it yet and the flow is not settled. The ORD signing (validation,
    the five signatures, the minimum set) IS described, from `docs/FUNDAMENT_Ordonantare_CAB.md`;
    write the sending section only when it exists;
  - anything Debug-only (the launcher, the dev harness).
- A fact you could not confirm in the code goes in the worklog under «de citit de operator», not
  into the help as if it were certain.

## 6. Engine changes (rare)

- New `goto:` kind → `KbotForm.HelpCapture.vb` `NavigateForCapture`, plus `HelpService.Navigate`
  and the `$GotoPrefix` pattern in `Check-Help.ps1`.
- New part → `HelpPart` enum, `HelpLibrary.ParsePart`, `HelpService.VisibleParts` / `ManualParts`,
  `$Parts` in `Check-Help.ps1`.
- New header key → `HelpLibrary` parser, README, `$HeaderKeys` in `Check-Help.ps1`.
- Every engine change follows the house rules in `CLAUDE.md` and is recorded as a `0000-NN` too.
