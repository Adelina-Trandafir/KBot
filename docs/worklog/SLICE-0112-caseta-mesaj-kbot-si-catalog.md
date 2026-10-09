# SLICE 0112 — K-BOT message box + debug message catalog (operator, 08.10.2026)

## What changed and why
The operator asked for (1) a control, in `KBot.Controls`, that produces a window imitating a message
box and will replace the native one everywhere, and (2) a debug-only bench listing EVERY message box
of the application in a grid (type, buttons, message, the function it is called from) with an editor
on the right.

1. **`KBot.Controls\MessageBox\`** — `KBotMessageBox` (entry point, `Install`, `Present`, `Show(owner, spec)`),
   `KBotMessageBoxForm` (+ `.Designer.vb`, a `KBotThemedForm`), `KBotMessageIcon` (drawn glyph,
   `IThemedControl`), `KBotMessageSpec` / `KBotMessageResult`, `KBotMsgKind` (None/Info/Warning/Error/Question),
   `KBotMsgButtons`. One optional EXTRA button (left of the standard ones). Doc: `KBotMessageBox.md`.
   Question was added to the operator's three kinds: 57 of the 474 calls use `MessageBoxIcon.Question`.
2. **Replacing everywhere without touching 470 call sites** — `KBotMessage` (Theming, already the single gate)
   gained a `Presenter` delegate; every `Show`/`ShowOnTop`/`Show(prompt, MsgBoxStyle, title)` goes through it.
   `Program.Main` calls `KBotMessageBox.Install()` after `ThemeManager.Initialize`. No presenter = native box
   (tests, early start-up). The `mesaje_operator.log` line is unchanged. The one stray `MessageBox.Show` in
   `SumarView` now uses `KBotMessage.Show(Me, …)`. Left alone on purpose: WPF `MessageBox` in
   `WorkflowExecutor.Browser.vb` (documented house exception) and `_reference/` (not compiled).
3. **Catalog** — `tools\MessageCatalog\scan.js` reads every `KBotMessage.Show/ShowOnTop`, `MessageBox.Show`,
   `MsgBox` call in `src` and writes `src\KBot.DevHarness\Config\mesaje_catalog.json` (id, file, line,
   function, type, buttons, default button, extra button, caption, text, flags). 474 calls, 0 unresolved;
   texts built from variables are followed back to their assignments in the same function (46); 302 keep
   `{expression}` parts. `--merge` re-scans and keeps the operator's edits (by id) and marks new calls.
4. **Bench** — `DevHarness\Internal\MessageCatalogForm` (+ Designer) + `MessageCatalog.vb` (JSON model) +
   harness test «Mesaje — catalogul tuturor casetelor de mesaj (editor)» (Controls/UI). Grid on the left
   (Tip, Butoane, Buton extra, Titlu, Mesaj, Funcția); on the right: combos for type and buttons, an editable
   combo for the extra button's text, caption field, `KBotRichTextEditor` for the message, a note for texts
   with `{parts}`; «Previzualizează» opens the real box with the edited spec, «Salvează în JSON» writes the file
   (asks on close when dirty). The file is found in the source tree by walking up from the exe.

## Files touched
New: `src/KBot.Controls/MessageBox/*` (8 files + .md), `src/KBot.DevHarness/Internal/MessageCatalog*.vb`,
`src/KBot.DevHarness/Tests/MessageCatalogTest.vb`, `src/KBot.DevHarness/Config/mesaje_catalog.json`,
`tools/MessageCatalog/scan.js`. Edited: `src/KBot.Theming/KBotMessage.vb`, `src/KBot.App/Program.vb`,
`src/KBot.App/Views/SumarView.vb`, `src/KBot.Controls/CONTROLS.md`, `README.md`.

## Test results
`dotnet build` of KBot.Controls, KBot.DevHarness and KBot.App: 0 warnings, 0 errors. No tests written or run
(operator rule). The scanner was run and its output inspected by hand on a sample.

## Left unverified / deferred
- **Nothing was seen on screen**: the box in Classic / Dark / Modern, at 125% / 150%, with a very long text, with
  the extra button, the catalog form itself. The Win32 behaviours (Esc, dead X, TopMost over the tutorial card,
  box opened from a background thread) are written from the design, not observed.
- The catalog does not feed the calls: the code still carries its own text. Making a call read its text from the
  catalog (by id) is a later step; ~73 messages are only `{ex.Message}`-like and would need templates.
- Scanner limits: a text built by `String.Format`/`StringBuilder`/a function stays a single `{expression}`;
  an `If(...)` text likewise; ids shift if a function gains a call before an existing one (`--merge` then adds
  the new id and the old one is dropped).
- Help: no topic describes the message box itself; screenshots that show a native message box are stale
  (named in «Ajutor de actualizat», `KBOT_STATUS_0000-0009.md`).
- Not committed (operator rule: no git writes).
- Slice number: operator said «ultimul disponibil» → 0112 (next free is now 0113).


---

# SLICE 0112-02 — the catalog feeds the calls; «Actualizează»; the ADMIN menu (operator, 08.10.2026)

## What changed and why
1. **The catalog now drives the wording.** `Config\mesaje_catalog.json` moved to `src/KBot.App/Config/` (shipped with
   the app in EVERY configuration, `KBot.App.vbproj`). Each entry keeps what the code has (`origType/origButtons/
   origCaption/origText`) next to what the operator edited. `KBotMessage` (every overload now also takes
   `CallerLineNumber`) goes through one `Run` → `MessageOverrides.Apply` (new, Theming): the entry of a call is found by
   source file + member and CONFIRMED by laying the code's template over the text the call really produced
   (`MessageTemplate`, new): `{ex.Message}`-style holes are read back from the real text and poured into the edited
   template, so the ~75 messages that are only a variable work with NO call site changed. Only entries that differ from the
   code do anything; no match / missing / broken file = the call keeps its own wording (broken file is logged).
   Type, caption, text, buttons and an extra button can all be overridden. The log line records what was really shown.
2. **Extra button at run time**: presenter delegate gained `extraButton`; pressing it returns `DialogResult.None` and sets
   `KBotMessage.LastExtraClicked` (per thread). No existing code reads it.
3. **Scanner**: new output path, `orig*` fields, literal braces of plain strings written `{{ }}`, `--merge` refreshes
   `orig*`/lines/function, keeps edits, marks new calls `isNew`, keeps the previous file as `.bak`, reports
   NEW / VANISHED / MIGRATED.
4. **Bench**: «Actualizează» (runs `node tools\MessageCatalog\scan.js --merge`, reloads, counts the NEW ones),
   «Revino la cod», a «Stare» column (NOU / MODIFICAT), a note that says whether the message differs from the code and warns
   when the buttons no longer match what the code tests; saving clears NOU, copies the file next to the executable and calls
   `KBotMessage.ReloadCatalog()`. Only the touched field is read back on an edit.
5. **Menu**: new folder **ADMIN** in MENIU holding «Mesaje (catalog)» (Debug only, `KbotForm.MessageCatalog.vb`),
   «Capturi pentru ajutor» and «Designer tutoriale» (both moved in; the old separator is gone). The folder shows only while one of
   its rows does. Keys unchanged, so captures `goto: menu:<key>` and the tutorial designer keep working.

## Files touched
New: `Theming/MessageTemplate.vb`, `Theming/MessageOverrides.vb`, `App/KbotForm.MessageCatalog.vb`, `App/Config/mesaje_catalog.json`
(moved from DevHarness). Edited: `Theming/KBotMessage.vb`, `Controls/MessageBox/KBotMessageBox.vb` + `.md`,
`App/KBot.App.vbproj`, `App/KbotForm.Designer.vb`, `KbotForm.HelpCapture.vb`, `KbotForm.Nomenclatoare.vb`,
`App/HelpContent/README.md`, `DevHarness/Internal/MessageCatalog*.vb`, `tools/MessageCatalog/scan.js`.

## Test results
`dotnet build` KBot.App Debug AND Release: 0 warnings, 0 errors. Nothing run, no tests; the template matcher and the
menu were NOT exercised.

## Left unverified / deferred
- Nothing seen on screen: the ADMIN folder (does a folder with `Visible = False` at design time open correctly once its
  children turn visible?), the bench's new buttons, the box with an extra button.
- The matcher has never run on real text: reading values back assumes the code's literal parts are unambiguous; a very long text
  falls back (250 ms regex limit). Messages whose text is an `If(...)` or `String.Format(...)` are one hole and work, but the editor
  shows them as a single `{expression}`.
- `Show(...)` answers a changed button set with the NEW set's answers; code that tests the old set will misread them.
- `.bak` files appear in `src/KBot.App/Config` after a merge (not shipped; not git-ignored yet).
- Help: only the developer note in `HelpContent/README.md` (menu path) changed; no user topic describes the capture/designer rows.

---

# SLICE 0112-03 — custom title bar, heading, custom buttons, HTML text (operator, 08.10.2026)

## What changed and why
The message window has nothing native left. **Title bar**: `KBotCaptionBar` with the X only; new `ShowClose` property (False hides it);
`CloseButton` Auto/Show/Hide per message (Auto = X only when there is a way out, so never on Yes/No; Alt+F4 follows the X). **Heading**: new
`Header` text (bold line above the message) — interpreted as «a new text for the header of the message»; the caption stays in the title bar.
**Buttons**: new control `KBotButton` (`Controls/Button/`, rounded, themed, `Primary`, `DialogResult`, Accept/Cancel capable). **Text**: `KBotHtmlLabel`,
simple HTML; `KBotHtmlLabel.MeasureHtml` added so the window hugs its message. Designer values (padding, gaps, bar/panel heights, size of `btn3`) are read
back as margins/minimums by `LayoutContent`. Catalog: new fields `header`, `closeButton` (scanner defaults, bench rows «Antet» and «Butonul X»,
`MessageExtras` replaces the bare extra-button string in the presenter delegate, `MessageOverrides` applies them).

## Files touched
New: `Controls/Button/KBotButton.vb` + `.md`, `Controls/MessageBox/KBotMsgClose.vb`, `Theming/MessageExtras.vb`. Rewritten: `KBotMessageBoxForm.vb` +
`.Designer.vb` (names `btnOK`, `btnNOK`, `btn3`, `btnExtra` kept from the operator's own designer edits). Edited: `KBotCaptionBar`, `KBotHtmlLabel`,
`KBotMessageSpec`, `KBotMessageBox`, `Theming/KBotMessage.vb`, `MessageOverrides.vb`, DevHarness `MessageCatalog*.vb`, `scan.js`, docs.

## Test results
`dotnet build` KBot.App Debug + Release: 0 errors, 0 warnings. Nothing run, nothing seen on screen.

## Left unverified / deferred
- Whole window unseen: tight width from `MeasureHtml`, the header gap, the X hidden, the buttons' look in the three themes, Esc/Enter/Space.
- `KBotButton` is new and has no tooltip/mnemonic; `VS` re-saved `KBotMessageBoxForm.resx` (a stale one may exist).
- Plain messages containing `<…>` text are read as HTML tags by `KBotHtmlLabel` when they look like one (unknown tags are dropped, their text kept).
- Help: no topic describes the message window.


---

# SLICE 0112-04 — «send the error» button in the message window (operator, 09.10.2026)

## What changed and why
For a message of kind **Error**, the title bar of the K-BOT message window now shows the bar's right-hand (options) button as «Trimite eroarea»: a drawn
arrow-out-of-a-tray glyph (follows the theme), a `KBotToolTip` (header + text, shown from the bar's new hover event), and a click that sends the error to the MariaDB
server. Not shown for other kinds, and not when the application has not installed the sender (tests, DevHarness preview of a spec shows it only if installed).
- **Window → application link**: `KBotMessage.ErrorReporter` (Theming delegate, like `Presenter`); `MessageErrorReport` (Theming) = the message as shown. `KBotMessage.Run` now also
  records the call's `FileName.Method` + line in `MessageExtras.Source/SourceLine` → `KBotMessageSpec` → the report. The window refuses a second click while sending, hides the
  button after a success and says so (Info); on failure it shows a **Warning** with the reason (never an Error: it would offer to send itself).
- **Application** (`KBot.App\MessageErrorReporter.vb`, installed in `Program.Main` right after the service provider is built): adds app FileVersion, OS, .NET, PC name, Windows user,
  culture, screen/dpi, theme, memory, uptime, session (unit, CF, year, SS, program, role), the active window, the list of open windows, and the tail of `harness_errors.log` (48 KB)
  and `mesaje_operator.log` (24 KB). Needs a logged-in session (otherwise a Romanian message says to connect first).
- **API**: `IErrorReportApi` / `ErrorReportRequest` / `ApiClient.SendErrorReportAsync` → `POST /api/errors/report` (bearer).
- **Server**: `PYTHON/routes/error_report.py` (registered in `main.py`) → new table `AVACONT_COMUN.FX_RaportErori` (`sql/0112_04_fx_raport_erori.sql`, utf8mb3). Who sent it (user, unit id,
  database, PC name) comes from the **session**, the IP from the request; the rest from the body. Idempotent by `Rid` (GUID per report). Text cut to column sizes; 4-byte characters → `?`.
- **Caption bar**: new event `OptionButtonHoverChanged` + read-only `OptionButtonHot` (the button is painted, so a host hangs its tooltip on this).
- NOUTATI: new section 1.1.2.1 (this + the whole custom message window, slices 0112 to 0112-04). FileVersion NOT bumped (the push script asks).

## Files touched
New: `sql/0112_04_fx_raport_erori.sql`, `PYTHON/routes/error_report.py`, `Api/IErrorReportApi.vb`, `Api/ApiClient.ErrorReport.vb`, `Theming/MessageErrorReport.vb`,
`App/MessageErrorReporter.vb`. Edited: `PYTHON/main.py`, `Theming/KBotMessage.vb`, `Theming/MessageExtras.vb`, `Controls/MessageBox/KBotMessageBoxForm.vb` + `.Designer.vb` (KBotToolTip `ttip`),
`KBotMessageSpec.vb`, `KBotMessageBox.vb`, `Controls/CaptionBar/KBotCaptionBar.vb`, `App/Program.vb`, `docs/release-notes/NOUTATI.md`, `state/KBOT_STATUS_0000-0009.md`.

## Test results
`dotnet build` KBot.App and KBot.DevHarness (Debug): 0 warnings, 0 errors. `py_compile` of `error_report.py` and `main.py` (venv): OK. Nothing run, no tests, **nothing seen on screen, no row written**.

## Left unverified / deferred
- **The table must be created on the server before the client is published** (`sql/0112_04_fx_raport_erori.sql`), and the server deployed (`error_report.py`, `main.py`). `DEFAULT UTC_TIMESTAMP()` on
  `MomentPrimit` relies on MariaDB expression defaults (10.2+): to be confirmed when the script is applied.
- Body limit 768 KB in the route; if nginx/Flask has a smaller `client_max_body_size` / `MAX_CONTENT_LENGTH` the send fails with a 413 (the window shows the reason).
- On screen: the glyph in the three themes, the tooltip position under the button, the title width next to the button, a success box opening over an error box.
- Help: no topic describes the message window; named in «Ajutor de actualizat» (0000-0009).
- Not committed (operator rule: no git writes).

**0112-04 revision (operator, 09.10.2026):** no success or failure message on sending (a failure goes to the error log only). The button is dimmed while the server answers
(new `KBotCaptionBar.OptionButtonEnabled`: dimmed, no hover, no click), hidden for good after a successful send, enabled again after a failed one. The first live try returned a 500 from
the server because the table had not been created yet; `error_report.py` now logs the driver message and the stack. Build: `KBot.Controls` 0 warnings / 0 errors; `KBot.App` compiled but could not copy
its DLLs while K-BOT was running (close it and build again). Help: `contabil.mesaje` (0000-59).
