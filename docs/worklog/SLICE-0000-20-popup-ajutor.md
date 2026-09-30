# SLICE-0000-20 — The «?» popup

Plan: `docs/PLAN_help_assistant.md` § 0000-20.

## What changed and why

The «?» of every caption bar used to open the help window, exactly like F1. It now opens a
popup under the button with a search box, the topic of the current screen and the guided tours
of the windows on screen. **F1 is unchanged**: it still opens the help window on the topic of
the focused control. The help window's search box is the same control as the popup's.

1. **Seam.** `IKBotHelpProvider.ShowHelpMenu(origin, anchorScreenRect)` + `KBotHelp.RequestMenu`
   (KBot.Theming). `KBotCaptionBar.HelpButtonClicked` calls `RequestMenu` with the button's screen
   rectangle. `ShowHelp` / `KBotHelp.Request` (F1) stay as they were.
2. **Controls** (`KBot.Controls/Popup/`, doc `KBotHelpPopup.md`, row in `CONTROLS.md`). The existing
   popups were looked at first: `CustomPopup` is a flat painted menu with no text box and no
   submenus, `KBotDropDownMenu` never activates (so no keyboard for a search box). Neither fits,
   so a small family was added next to them, reusing `KBotTextField` (search box),
   `KBotScrollBar`, `KBotToolTip.ShowAt`, `ThemeShapes`, `KBotThemedForm` /
   `KBotThemedUserControl`:
   - `KBotHelpRow` + `IKBotHelpSearchSource` — the rows and the interface the application
     implements (the controls cannot reference KBot.App);
   - `KBotHelpList` — painted list: headers, hits (title › section, two lines of text,
     «Deschide «...»» / «Tur ghidat» buttons), the screen's topic, tours, folders of tours; own
     scroll bar; hover tooltips;
   - `KBotHelpSearchPanel` (UserControl, designer) — box + list; types → local search after
     150 ms; Up / Down / Enter drive the list; Esc raised to the host;
   - `KBotHelpPopup` (Form, designer) — the panel + last line «Deschide ajutorul complet (F1)»;
     placed under the button (above it when there is no room); closes on Esc, click elsewhere
     and when a row hands over (deferred with `BeginInvoke`).
   All `IThemedControl`, colours only from the palette (kept in fields, so a host designer has
   nothing to serialize), logical px scaled at layout, `AutoScaleMode.Dpi` 96/96.
3. **Tours in the popup** (`HelpPopupTours`): the application windows in z-order (`EnumWindows`),
   skipping minimized, disabled (behind a modal dialog) and the help's own windows. A tour
   belongs to a window when one of its screens is VISIBLE in it (`HelpTourRunner.FindInWindow`,
   split out of `FindTarget`): the form, a control, or the selected view of the main window (the
   other views are hidden). A tour is offered once, on the top window it belongs to; only parts in
   `VisibleParts()`. One window with tours → listed directly; several → one folder per window,
   titled with its caption, the top one open.
   New optional tour header key `screens:` (`HelpTour.Screens`, `$TourKeys` + key check in
   `Check-Help.ps1`, README) — used by `tur-avansat` (`screens: SetariForm`), whose topic lists only
   a checkbox.
4. **Rows and actions** (`HelpSearchSession`, one per panel, `popup` / `fereastra`): a hit →
   the help window at its section (`HelpService.ShowHit`); from the popup the window's search list
   takes over the question and its results (`HelpForm.TakeOverSearch`). «Deschide «...»» →
   `HelpService.OpenScreen` (0000-19); «Tur ghidat» / a tour row → `StartTour`; the topic row → the
   topic; «Deschide ajutorul complet» → what F1 would open for that window. Nothing found → a
   short note row.
5. **Help window**: `txtCauta` replaced by `pnlCautare` (`KBotHelpSearchPanel`) in
   `HelpForm.Designer.vb`. Folded to its box over the contents while empty; with results it takes
   the whole left side (the contents come back when the box is emptied or on Esc). The HTML
   results page (`HelpHtml.SearchPage`, the `s:` pages) is gone: the list is the results page now.
   The start page sentence about F1 / «?» / the search box updated.
6. **Tours, content**: five new tours for screens that had none — `tur-sumar`, `tur-istoric`,
   `tur-notecab`, `tur-clasificatii`, `tur-parteneri` (text only from what their topics already
   say; targets checked against the designers). Left without a tour on purpose: the DDF and ORD
   editors (operator's decision in 0000-07 / 0000-11), `BrowserView` (its buttons are in
   `tur-forexe`), `RobotQueueForm` (0098, has no topic yet).

## Files touched

- `src/KBot.Theming/KBotHelp.vb`
- `src/KBot.Controls/CaptionBar/KBotCaptionBar.HelpButton.vb`
- `src/KBot.Controls/Popup/KBotHelpRow.vb`, `KBotHelpList.vb`, `KBotHelpSearchPanel.vb`,
  `KBotHelpSearchPanel.Designer.vb`, `KBotHelpPopup.vb`, `KBotHelpPopup.Designer.vb`,
  `KBotHelpPopup.md` (new); `src/KBot.Controls/CONTROLS.md`
- `src/KBot.App/Help/HelpService.vb` (`ShowHelpMenu`, `PopupHomeRows`, `ShowHit`, `MainWindow`,
  `ShowHelpForKeys`, `TopicForKeys`)
- `src/KBot.App/Help/HelpSearchSession.vb`, `HelpPopupTours.vb` (new)
- `src/KBot.App/Help/HelpTour.vb` (`Screens`), `HelpTourRunner.vb` (`FindInWindow`)
- `src/KBot.App/Help/HelpForm.vb`, `HelpForm.Designer.vb`, `HelpHtml.vb`
- `src/KBot.App/HelpContent/README.md`; `tours/tur-avansat.md`; new `tours/tur-sumar.md`,
  `tur-istoric.md`, `tur-notecab.md`, `tur-clasificatii.md`, `tur-parteneri.md`
- `tools/HelpCheck/Check-Help.ps1`

## Test results

- `dotnet build src\KBot.App\KBot.App.vbproj` (builds Theming and Controls too): **0 warnings,
  0 errors**.
- `tools\HelpCheck\Check-Help.ps1 -Coverage -Map`: **No errors.** 48 topics, 17 tours, 54 capture
  tags; coverage `RobotQueueForm` (0098) as before.
- No tests, no app run: the popup was never seen on screen.

## Left unverified or deferred

- Nothing here was rendered: placement under the button, the list's row heights at 125 / 150 %,
  the folder toggle, the tooltips, the deferred close when a tour bubble or a message takes the
  focus. The operator should open «?» on the main window (one window, tours listed) and with
  Setări open over it (two folders).
- The popup's stars and the question log: 0000-21. The help text for the popup: 0000-22.
- Folders open inline in the list rather than as a cascading submenu (simpler keyboard, no
  second activating window); say if a real submenu is wanted.
