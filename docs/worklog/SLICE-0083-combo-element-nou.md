# SLICE 0083 — `KBotComboBox.OfferNewItem`

**Date:** 26.09.2026. **Request (operator):** with `LimitToList` on, a new option `OfferNewItem`.
When it is on and the list is empty, one row appears (its text editable in the designer as
`OfferNewItemText`); clicking it raises an event.

## What changed

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
- **`KBotComboFindList`**: the row is a `KBotComboFindRow` with `IsNewItem`, drawn in italic, and
  reported through its own `NewItemChosen` event.
- **New file:** `KBotComboNewItemEventArgs.vb`.

## Files touched

- `src/KBot.Controls/Combo/KBotComboBox.vb`, `KBotComboFindList.vb`, `KBotComboNewItemEventArgs.vb`
  (new), `KBotComboBox.md`

## Test results

- `dotnet build src\KBot.Controls` (scratch folder): **0 errors, 0 warnings**.
- The operator asked for **no checks and no tests**: nothing rendered or written as a test.
