# SLICE-0082/83 — Istoric ComboBox: căutare, mască și element nou

Cele două cereri din 26.09.2026 folosesc aceeași listă separată fără activare: 0082 introduce căutarea și masca, 0083 adaugă oferta de element nou cu LimitToList. Corpurile, API-urile, sursele și limitele sunt păstrate integral. Punctul 3 gol al cererii 0082 rămâne consemnat, fără a-l transforma într-o cerere nouă în această curățare. Proba randată a lui 0082 nu certifică gesturile sau oferta din 0083, unde operatorul ceruse lipsa verificărilor. Contractul public rămâne în [KBotComboBox](../../src/KBot.Controls/Combo/KBotComboBox.md).

[0082-combo](#slice-0082) · [0083-combo](#slice-0083)

## SLICE 0082 — `KBotComboBox`: find as you type + input mask <a id="slice-0082"></a>

**Date:** 26.09.2026. **Request (operator):** (1) a `FindAsYouType` option that shows the values in a drop-down list while typing, plus `FindAfterNChars` = start searching after at least N typed characters, both in the designer; (2) an `InputMask` in the designer so that a classification like `65.02.04.02.20.01.01` is typed as digits only, the dots written automatically — and with a mask, `FindAfterNChars` must not count the dots (or any other mask literal); (3) **empty in the request.**

### What changed and why

**Three designer properties on `KBotComboBox`** (category «K-BOT Combo»), all defaulting so that a freshly dropped combo serializes none of them:
- `FindAsYouType: Boolean = False`
- `FindAfterNChars: Integer = 1` (< 1 throws — no silent clamp)
- `InputMask: String = ""` (an invalid mask throws, and the old mask stays) Plus `UnmaskedText` (read-only, hidden from the designer) and `FindMatches(captions, typed)` (Shared, pure).

**Find as you type.** A separate window, `KBotComboFindList`, under the box — NOT the combo's own list. Filtering the native list means changing `Items`, which a data-bound combo cannot do, and every change of `Items` while it is open makes Windows rewrite the text and move the caret. The window only shows rows; it never takes the focus (`WS_EX_NOACTIVATE`, `ShowWithoutActivation`, `MA_NOACTIVATE` on click), so the caret stays in the box and the box drives it: Up/Down/PageUp/ PageDown, Enter or a click takes a row (as if picked from the drop-down), Escape closes it. While it is open Enter/Escape are input keys (`IsInputKey`), otherwise a dialog's AcceptButton/CancelButton would take them first. It closes on focus loss, when the native list opens, and when the form moves, resizes or is deactivated. Matching: rows that START with the text first, then rows that CONTAIN it; case and diacritics ignored. With `FindAsYouType` on, `CommitText` also accepts a text that is the start of exactly ONE row (a full code picks its «code — name» row).

**Input mask** — `KBotInputMask` (pure). `0` digit, `L` letter, `A` letter/digit, `&` any non-space, `\x` literal; anything else is a literal the mask writes. Every edit works on the raw value (typed characters only) and the text is rebuilt from it; a literal is written only in front of a following character, so `65` → `65`, next digit → `65.0` (no trailing dot to backspace over). Typed characters and Backspace go through `OnKeyPress`, Delete and Ctrl+V / Shift+Insert through `OnKeyDown`, and any other operator edit (Ctrl+X, the context-menu paste) is re-shaped in `OnTextUpdate` (which fires only for operator edits, never for a selection or a host writing `Text`). `FindAfterNChars` counts `UnmaskedText.Length` — the dots never count.

### Files touched

- `src/KBot.Controls/Combo/KBotComboBox.vb` — the three properties, find list driving, mask keys
- `src/KBot.Controls/Combo/KBotInputMask.vb` (new) — the mask engine + `KBotMaskEdit`
- `src/KBot.Controls/Combo/KBotComboFindList.vb` (new) — the non-activating list window
- `src/KBot.Controls/Combo/KBotComboBox.md` — documented
- `tests/KBot.Controls.Tests/KBotInputMaskTests.vb` (new)

### Test results

- `dotnet build src\KBot.Controls` — **0 errors, 0 warnings**.
- Verified by rendering (first user: the 0081-08 window, off-screen, `DrawToBitmap`, keys sent as `WM_CHAR` to the combo's own EDIT): `650204` → `65.02.04`; the list opened with the 3 matching rows of 4, first highlighted; Enter → `65.02.04.02.20.01.01 — Furnituri de birou`, `SelectedIndexChanged` fired (the window filled the name and the values from it).
- `KBotInputMaskTests` written, **not run** (house rule).

### Left unverified / deferred

- **Item 3 of the request was empty** — to ask the operator.
- Not tried by hand on a real screen: mouse wheel over the list, clicking a row, the list near the bottom edge of the screen (it then opens above the box), per-monitor DPI moves while open.
- With a digits-only mask the operator cannot search by NAME in that combo (the arrow still shows the whole list). A deliberate consequence of the mask, noted in STATUS.
- The DevHarness combo playground has no toggles for the new properties.

---

## SLICE 0083 — `KBotComboBox.OfferNewItem` <a id="slice-0083"></a>

**Date:** 26.09.2026. **Request (operator):** with `LimitToList` on, a new option `OfferNewItem`. When it is on and the list is empty, one row appears (its text editable in the designer as `OfferNewItemText`); clicking it raises an event.

### What changed

- **`KBotComboBox`**
  - `OfferNewItem` (default off) and `OfferNewItemText` (default «Adaugă un element nou…»).
  - `NewItemRequested` event with `KBotComboNewItemEventArgs.Text` (the typed text).
  - "The list is empty" means one of two cases:
    - the typed text matches no row. This works with or without `FindAsYouType`.
    - the combo has no items and its drop-down is opened. The empty native list is closed and
      replaced by the row.
  - Enter on the row works like a click. If the host adds an item named exactly like the typed text,
    it becomes the selection.
  - With `LimitToList` off it does nothing.
- **`KBotComboFindList`**: the row is a `KBotComboFindRow` with `IsNewItem`, drawn in italic, and reported through its own `NewItemChosen` event.
- **New file:** `KBotComboNewItemEventArgs.vb`.

### Files touched

- `src/KBot.Controls/Combo/KBotComboBox.vb`, `KBotComboFindList.vb`, `KBotComboNewItemEventArgs.vb` (new), `KBotComboBox.md`

### Test results

- `dotnet build src\KBot.Controls` (scratch folder): **0 errors, 0 warnings**.
- The operator asked for **no checks and no tests**: nothing rendered or written as a test.
