# SLICE 0082 — `KBotComboBox`: find as you type + input mask

**Date:** 26.09.2026. **Request (operator):** (1) a `FindAsYouType` option that shows the values in
a drop-down list while typing, plus `FindAfterNChars` = start searching after at least N typed
characters, both in the designer; (2) an `InputMask` in the designer so that a classification like
`65.02.04.02.20.01.01` is typed as digits only, the dots written automatically — and with a mask,
`FindAfterNChars` must not count the dots (or any other mask literal); (3) **empty in the request.**

## What changed and why

**Three designer properties on `KBotComboBox`** (category «K-BOT Combo»), all defaulting so that a
freshly dropped combo serializes none of them:
- `FindAsYouType: Boolean = False`
- `FindAfterNChars: Integer = 1` (< 1 throws — no silent clamp)
- `InputMask: String = ""` (an invalid mask throws, and the old mask stays)
Plus `UnmaskedText` (read-only, hidden from the designer) and `FindMatches(captions, typed)`
(Shared, pure).

**Find as you type.** A separate window, `KBotComboFindList`, under the box — NOT the combo's own
list. Filtering the native list means changing `Items`, which a data-bound combo cannot do, and
every change of `Items` while it is open makes Windows rewrite the text and move the caret. The
window only shows rows; it never takes the focus (`WS_EX_NOACTIVATE`, `ShowWithoutActivation`,
`MA_NOACTIVATE` on click), so the caret stays in the box and the box drives it: Up/Down/PageUp/
PageDown, Enter or a click takes a row (as if picked from the drop-down), Escape closes it. While it
is open Enter/Escape are input keys (`IsInputKey`), otherwise a dialog's AcceptButton/CancelButton
would take them first. It closes on focus loss, when the native list opens, and when the form moves,
resizes or is deactivated. Matching: rows that START with the text first, then rows that CONTAIN it;
case and diacritics ignored. With `FindAsYouType` on, `CommitText` also accepts a text that is the
start of exactly ONE row (a full code picks its «code — name» row).

**Input mask** — `KBotInputMask` (pure). `0` digit, `L` letter, `A` letter/digit, `&` any non-space,
`\x` literal; anything else is a literal the mask writes. Every edit works on the raw value (typed
characters only) and the text is rebuilt from it; a literal is written only in front of a following
character, so `65` → `65`, next digit → `65.0` (no trailing dot to backspace over). Typed characters
and Backspace go through `OnKeyPress`, Delete and Ctrl+V / Shift+Insert through `OnKeyDown`, and any
other operator edit (Ctrl+X, the context-menu paste) is re-shaped in `OnTextUpdate` (which fires only
for operator edits, never for a selection or a host writing `Text`). `FindAfterNChars` counts
`UnmaskedText.Length` — the dots never count.

## Files touched

- `src/KBot.Controls/Combo/KBotComboBox.vb` — the three properties, find list driving, mask keys
- `src/KBot.Controls/Combo/KBotInputMask.vb` (new) — the mask engine + `KBotMaskEdit`
- `src/KBot.Controls/Combo/KBotComboFindList.vb` (new) — the non-activating list window
- `src/KBot.Controls/Combo/KBotComboBox.md` — documented
- `tests/KBot.Controls.Tests/KBotInputMaskTests.vb` (new)

## Test results

- `dotnet build src\KBot.Controls` — **0 errors, 0 warnings**.
- Verified by rendering (first user: the 0081-08 window, off-screen, `DrawToBitmap`, keys sent as
  `WM_CHAR` to the combo's own EDIT): `650204` → `65.02.04`; the list opened with the 3 matching rows
  of 4, first highlighted; Enter → `65.02.04.02.20.01.01 — Furnituri de birou`, `SelectedIndexChanged`
  fired (the window filled the name and the values from it).
- `KBotInputMaskTests` written, **not run** (house rule).

## Left unverified / deferred

- **Item 3 of the request was empty** — to ask the operator.
- Not tried by hand on a real screen: mouse wheel over the list, clicking a row, the list near the
  bottom edge of the screen (it then opens above the box), per-monitor DPI moves while open.
- With a digits-only mask the operator cannot search by NAME in that combo (the arrow still shows
  the whole list). A deliberate consequence of the mask, noted in STATUS.
- The DevHarness combo playground has no toggles for the new properties.
