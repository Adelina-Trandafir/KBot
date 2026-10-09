# KBotMessageBox

K-BOT's own message box: a themed window that stands in for the native `MessageBox` and has nothing native left in it —
its own title bar (`KBotCaptionBar`: an X and nothing else, hidden when the message has no way out), an optional heading,
the message as simple HTML (`KBotHtmlLabel`), a drawn glyph, one to three standard `KBotButton`s and, optionally, one EXTRA one.

`MessageBox/` · `KBotMessageBox` (entry point) · `KBotMessageBoxForm` (the window,
`KBotThemedForm`, authored in the designer) · `KBotMessageIcon` (drawn glyph, `IThemedControl`) ·
`KBotMessageSpec` / `KBotMessageResult` (data in / answer out) · `KBotMsgKind` · `KBotMsgButtons`.
Conventions: [C1..C9](../CONTROLS.md). Status: slice-less (08.10.2026), built, not seen on screen.

## How it replaces every `MessageBox` in the solution
`KBotMessage` (KBot.Theming) is the one gate all dialogs already go through (~470 calls). It now
has a `Presenter` delegate; `Program.Main` calls `KBotMessageBox.Install()` after
`ThemeManager.Initialize`, and from then on every `KBotMessage.Show` / `ShowOnTop` — and the
`Show(prompt, MsgBoxStyle, title)` form — opens this window. No call site changed. With no
presenter (unit tests, before start-up) the native box is used, so nothing depends on the order.
The log line to `mesaje_operator.log` is unchanged (still written by `KBotMessage`).

## API
- `KBotMessageBox.Install()` — set `KBotMessage.Presenter`. Idempotent.
- `KBotMessageBox.Present(owner, text, caption, buttons, icon, defaultButton, topMost)` —
  `MessageBox.Show` shape, returns `DialogResult`.
- `KBotMessageBox.Show(owner, spec) As KBotMessageResult` — the full form. `spec.ExtraButton`
  adds a button left of the standard ones; `Result.ExtraClicked` says it was pressed
  (`Result.Result` is then `None`). Safe from any thread (marshals to the owner's thread).
- `KBotMessageSpec` — `Kind` (`None Info Warning Error Question`), `Buttons` (`OK OKCancel
  AbortRetryIgnore YesNoCancel YesNo RetryCancel`), `Caption`, `Text`, `ExtraButton`,
  `DefaultButton` (1-based among the standard ones), `TopMost`; `FromWinForms(...)`, `Clone()`.

## Title bar, heading, HTML (slice 0112-03)
- Borderless; `KBotCaptionBar` shows the caption, `ShowClose` is the X. `CloseButton`: `Auto` (X when there is a Cancel button, or OK alone —
  so never on Yes/No), `Show` (an X on a set with no Cancel answers Cancel), `Hide` (Alt+F4 does nothing either).
- `Header` = a bold line above the text (the code has none; the catalog can add it). It was read as «the new text for the header of the message»; if a
  second title-bar text was meant, say so.
- `Text` and `Header` take simple HTML (`b i u br div font color…`, see KBotHtmlLabel); text without tags is shown as written.
- Designer: `pnlBody.Padding`, `picIcon.Margin.Right`, `lblHeader.Margin.Bottom`, `capBar.Height`, `pnlButtons.Height` and the size of `btn3` are
  read at run time as margins / minimums; positions are computed (the window hugs its message).

## Behaviour kept from the native box
System sound per kind · Esc = Cancel (or OK when it is the only button) · the X does nothing on
sets with no way out (Yes/No, Retry… without Cancel) · default button focused and on Enter ·
`TopMost` for the tutorials (`ShowOnTop`).

## Sizing
Everything is measured in units of the font height, so the box follows text size, dpi and zoom
with no hard-coded pixel. The layout runs in `OnLoad` AFTER the base class applied theme and
zoom (otherwise zoom would scale it twice), then the box is centred again over its owner.
Text width is capped at 36 line-heights; a taller message scrolls inside the body (box capped at
80% of the working area).

## Limits
- Plain text only (no markup). Button captions are Romanian (`Da Nu Anulare Reîncearcă Renunță
  Ignoră`), not localised by Windows.
- One extra button, never the default. No "help" button, no checkbox ("don't ask again"), no
  timeout.
- Not seen on screen yet — the first look at Classic / Dark / Modern and at 125% / 150% is open.

## The message catalog feeds the calls (slice 0112-02)
`src\KBot.App\Config\mesaje_catalog.json` (shipped as `<AppDir>\Config\`, every configuration) holds one
entry per message box call: what the CODE has (`origType/origButtons/origCaption/origText`) and what the
operator edited (`type/buttons/caption/text/extraButton`). `KBotMessage.Run` → `MessageOverrides.Apply` finds
the entry of a call by source file + member (compiler caller-info), CONFIRMS it by laying the code's template
over the text the call really produced (`MessageTemplate.Match`), and changes something only when the entry
differs from the code:
- `{expression}` holes (`{ex.Message}`) are read back from the real text and put into the edited template
  (`MessageTemplate.Fill`); a literal brace is `{{` / `}}`. The hole is named by its expression, so it may
  move, repeat or disappear.
- No match (message reworded, catalog stale, file missing or broken) = the call keeps its own wording. A broken
  file is logged, never fatal.
- A changed *buttons* value is applied as written: the code still tests the answers of the ORIGINAL set — the
  editor warns about it.
- Extra button: pressing it returns `DialogResult.None` and sets `KBotMessage.LastExtraClicked` (per thread);
  no code reacts to it yet.
`KBotMessage.ReloadCatalog()` re-reads the file (the editor calls it after a save).

## Debug catalog (DevHarness only)
Opened from the main menu: **MENIU › ADMIN › Mesaje (catalog)** (Debug build; ADMIN also holds «Capturi pentru
ajutor» and «Designer tutoriale»). `tools\MessageCatalog\scan.js` lists every call in `src` into the catalog; the
bench edits it. **Actualizează** runs `node tools\MessageCatalog\scan.js --merge`: new calls appear marked NOU,
lines and the code's wording are refreshed, edits are kept, the previous file stays as `.bak`. **Revino la cod**
drops the edits of one message. Saving also copies the file next to the executable and reloads it. Needs Node.js
in PATH.
