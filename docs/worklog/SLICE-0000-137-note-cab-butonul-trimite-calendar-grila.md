# SLICE-0000-137 - Note corectie: refresh + «Trimite in CAB» in ribbon; calendar in date cells of the grid (29.10.2026, no slice number given)

Operator request of 29.10.2026 (four points). The slice number for the CODE was not given; it is recorded here and in `state/KBOT_STATUS_SLICELESS.md` until the operator names one.

## What changed and why
1. **Refresh after saving a correction note.** `KbotForm.CabNotes.vb` / `ShowUncorrelatedAsync`: the nav entry «Note corectie» was raised only locally and only when a saved correction touched the angajament on screen, so a note saved on another angajament showed nothing until the app was reopened. Now, after the window closes with saved notes, the tree is read again with the selection kept (`LoadTreeAsync(pastreazaSelectia:=True)`); that refreshes every `Are*` flag and hands the open view its context again (`NoteCabView` reloads its notes). The local flag is still raised first, so the entry shows even if the reload fails.
2. **«Trimite in CAB» in the ribbon, right click menu removed.** `NoteCabView` implements `IViewRibbon` (key `notecab_trimite`, group caption «Note corectie»): enabled when a note leaf is selected, dimmed otherwise, text «Trimite din nou in CAB» for a note already sent; it runs the same `UploadAsync` as the old menu item. The right click popup (`ShowMenu` / `OnMenu`) is gone; the «Genereaza din nou documentul» item is gone with it (the Document page keeps its own «Genereaza» surface). Button declared in `AvacontMain.Designer.vb` (`KBotRibbonButton41`, existing PDF-export icon), routed in `AvacontMain.vb` (`Ribbon_ItemClicked`, `ViewGroupKeys`).
3. **Calendar in the date cells of `KBotDataView`.** New `KBotDataView.DateEditor.vb`: an editable Text column with `ValueType = DateTime` paints a calendar button on the right of the cell; a click on it, Alt+Down or F4 in the editor opens a `KBotCalendarPopup`; the chosen day becomes the editor text (`FormatValue`) and is committed through `CommitEdit`, so the host's `CellValidating` parses it like typed text. Text and editor stop before the button strip (`Painting`, `Editing.PlaceEditor`), `AutoSize` reserves the strip, `Input` handles click/cursor. It applies to every editable DateTime text column of every grid (Clasificatii cheltuieli / venituri are the ones asked for).
4. **«Val. receptii» when adding a revision from a reservation: NOT CHANGED.** `routes/forexe/ddf_edit.py` computes it as the receptions strictly BEFORE the revision date, keyed by `CodIndicator` (a faithful port of `FX_Adaugare_DDF`, `mdl_FX_DDF.md` line 274), while the saved revision reopens with ALL receptions of the indicator, and the classification list uses a per-classification total. Which of these the operator expects is not known; waiting for an example (angajament, shown vs expected value).

## Files touched
`src/KBot.App/Views/NoteCabView.vb`, `src/KBot.App/KbotForm.CabNotes.vb`, `src/KBot.App/AvacontMain.vb`, `src/KBot.App/AvacontMain.Designer.vb`, `src/KBot.Controls/DataView/KBotDataView.DateEditor.vb` (new), `KBotDataView.Editing.vb`, `.Painting.vb`, `.Input.vb`, `.AutoSize.vb`; help: `contabil/notecab.md`, `contabil/nomenclatoare/clasificatii.md`, `help-version.txt` (tag `0000-137`).

## Test results
`dotnet build src\KBot.App\KBot.App.vbproj -c Debug`: 0 errors, 0 warnings. No tests run (standing rule). Nothing seen on screen.

## Unverified / deferred
- Never run: the calendar popup from a grid cell (focus hand-over between the cell editor and the popup, Esc, a click outside), the button glyph at 125%/150% DPI, the ribbon button's states, the tree reload after saving a note (selection kept, view context).
- The new button's image is an existing resource (PDF export); the operator may want another.
- The date button also shows on every other editable DateTime text column in the app (none other known to be editable besides the two Clasificatii pages); to be looked at.
- Point 4 above.
- Help: no capture retaken; the `note-corectie` and `clasificatii` captures stay marked `redo`.
