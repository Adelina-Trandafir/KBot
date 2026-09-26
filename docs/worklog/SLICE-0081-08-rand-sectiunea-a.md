# SLICE 0081-08 — Section A line window (and the two dead ends of «Adaugă rând»)

**Date:** 26.09.2026. **Request (operator), in the DDF editor (`DdfEditForm`):**
1. For «Adaugă rezervare» (a new reservation made in K-BOT, to be sent to FOREXE) «Adaugă rând» is
   not active.
2. For «Angajament nou» it is active, but double-clicking the Clasificație column does nothing; a
   combo to choose a classification should appear.
3. «Adaugă rând» should open a new window, laid out in the designer, with every value section A
   expects.

## What was wrong

- **(2) — verified in the files.** The column the operator sees is «Clsf» (`clsf`, read-only). The
  combo column «Clasificatie» (`clasificatie`) is `Visible = Hidden` in
  `DdfEditSectiuneaAPage.Designer.vb`, and `Grd_CellDoubleClick` answered ONLY to `clasificatie`. So
  a double click could never reach it.
- **(1) — the cause is NOT verified on data.** `btnAdauga.Enabled` was `disponibile > 0`: off when
  the server list came back empty, and off when the request failed. Neither «Adaugă rezervare»
  factory (`ForAddedReservation`, `ForFirstRevisionOfExisting`) sets `Sursa` or `GrpIdrz`, so the
  reservations lock (`DinRezervari`) is not it. For an angajament whose code no longer starts with
  «!» the list comes only from `FX_Indicatori` of that `CodAngajament` (+ same `Titlu`); an empty
  one there gives exactly this. The empty-list message also lied («toate clasificațiile sunt deja
  folosite» for a list that was simply empty).
- **Found on the way:** `DdfEditForm.AduClasificatiileAsync` took `Titlu` (manual angajament) as
  the THIRD dotted part of `Clsf`. `Capitol` and `Subcapitol` are `NN.NN` themselves (varchar(5),
  checked in `MariaDB_Schema/000_DEMO.sql`), so that is half of Subcapitol: `04` instead of `20`
  for `65.02.04.02.20.01.01`. Now `Substring(12, 2)` = Access's `Mid(Clsf, 13, 2)`.

## What changed

- **New window `DdfEditLinieAForm`** (`DDF_EDIT/`, all controls in the `.Designer.vb`, 144 dpi,
  `AutoScaleMode.Dpi`, `KBotThemedForm`): Clasificație (`KBotComboBox` with `Editable`,
  `FindAsYouType`, `FindAfterNChars = 2`, `InputMask = 00.00.00.00.00.00.00` — slice 0082), Denumire,
  Element fundamentare *, Parametrii, Partener (only when the document has a partner), Valoare
  curentă *; read-only: valoare precedentă, recepții, totală (live), cod indicator. Works on a COPY
  of the line; OK writes it back, «Renunță» leaves the draft untouched. The refusals are section A's
  own (separator never offered; empty element; value 0; negative value below receptions), all in one
  message. Picking a classification fills the element with its name unless the operator wrote
  their own.
- **`DdfEditSectiuneaAPage`:** «Adaugă rând» follows the reservations lock only. It opens the window
  on a new line — the line joins the draft only on OK (before, an empty line was added first and left
  behind on cancel). A double click on «Clsf» (or the hidden «Clasificatie») opens it on that line.
  The list is fetched again when the last fetch failed or was empty, and every time for a manual
  angajament (its `Titlu` restriction follows the first line). When there is nothing to choose, a
  message says which of the two cases it is. The status line now tells «server sent nothing» from
  «all used». The in-grid combo code (fill, validate, apply) is gone — its column is hidden.
- **`DdfEditForm.AduClasificatiileAsync`:** the `Titlu` fix above.

## Files touched

- `src/KBot.App/DDF_EDIT/DdfEditLinieAForm.vb` (new), `DdfEditLinieAForm.Designer.vb` (new)
- `src/KBot.App/DDF_EDIT/DdfEditSectiuneaAPage.vb`
- `src/KBot.App/DDF_EDIT/DdfEditForm.vb`
- (depends on slice 0082: `src/KBot.Controls/Combo/*`)

## Test results

- `dotnet build src\KBot.App` — **0 errors, 0 warnings**, built into a scratch folder: KBot.App was
  running (PID 15924) and Visual Studio held the normal `bin\Debug` copies.
- The window rendered off-screen with `DrawToBitmap` on sample classifications: layout checked,
  typing `650204` → `65.02.04` + list of 3 matches, Enter chose the first and filled Denumire,
  Element, previous value, receptions, total, indicator code. Height trimmed after the first render.
- No suite run.

## Left unverified / deferred

- **The real reason (1) was off on the operator's angajament** — the window now names it; read what
  it says the first time. If «Serverul nu a trimis nicio clasificație…», look at `FX_Indicatori` for
  that `CodAngajament`.
- Not opened in the VS designer; not run against the server; the grid double-click not tried by hand.
- The designer's hidden «Clasificatie» column is still declared as `Combo` (never fed). Left as it is
  to keep the designer file untouched.
