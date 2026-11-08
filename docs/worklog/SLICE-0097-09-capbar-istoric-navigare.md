# SLICE-0097-09 — Navigation history in the ribbon and caption bar

Operator-approved plan (06.11.2026): keep a caption-bar history shortcut, move it leftmost in the caption-button group, add a permanent ribbon shortcut before "Inchide", and give each an independent application setting. Only the ribbon shortcut is on by default. The approved popup revision halves its width, adds two font points, separates open/closed entries and uses each command's ribbon icon on the existing CustomPopup icon band. Help and release notes are included; the operator requested the notes under 1.1.5.3.

Numbering continues slice 0097 (caption-bar unit selector, then year/source selectors in 0097-03); sub-slices 02 through 08 are occupied. The operator corrected the initial incidental E-Factura association; no 01EF number is consumed.

## Behaviour

- `ShowHistoryButton` is opt-in, leftmost in the caption-button group before options/theme/help. Existing close/maximize/minimize positions are restored. Its main-window visibility follows `ShowNavigationHistoryCaption` (False by default).
- The ribbon's pinned `navigare_istoric` button precedes `pagina_inchide`. It follows `ShowNavigationHistoryRibbon` (True by default), is outside the form/view groups, stays visible on Home and becomes a small tab-strip button when the ribbon is folded. It is also retained by the existing director ribbon filter.
- Settings > Application > General has two independent checkboxes, saved immediately through the existing AppSettings clone/DTO/store and applied to the running main window. Both may be on or off. Hiding the shortcut that owns an open history popup closes that popup.
- Both shortcuts open the same `CustomPopup`, fixed at 180 logical pixels (previously 360), with `FontSizeOffset = 2`, ribbon icons and the existing coloured icon band. The old task-created KBotSidePanelPopup files were removed. Optional `CustomPopupItem.Italic` and row-font offset default to existing behaviour for other menus; measurement and painting share the derived font, disposed by the popup.
- Open entries come first, closed entries in italics below a separator (only when both groups exist). Each group is sorted by Romanian title text. Sidebar and popup share this presentation order while retaining the existing history retention rules and scope. Live history changes update both surfaces.
- Selection uses the existing `OpenMachetaAsync`: bring an open page forward or reopen a closed one. Open entries reuse the existing in-memory snapshot previews; full names remain available in tooltips when clipped. Mouse wheel, keyboard navigation, Enter, Escape and outside-click dismissal are the existing CustomPopup behaviours. No second history store, schema, endpoint or screenshot file is created.
- Help explains the shared history and its settings in the main-window, ribbon, settings and sidebar topics. Release notes are in 1.1.5.3, without a fabricated publication marker.
- Approved continuation: Ctrl+Tab switches only already open machete (not internal Sumar/Rezervari/Istoric views). Ctrl+Shift+Tab reverses; releasing the last Ctrl commits, Escape/outside dismissal cancels. A rapid chord switches to the previously used page; committing synchronously on Ctrl release makes the next chord toggle back. Activation is recorded separately from the alphabetic history presentation, including the shell's relative use order. The candidate sequence is frozen for the gesture and revalidated against actual open page instances before activation; no closed entry is reopened.
- The selector is the same `CustomPopup`, with opt-in `PreviewColumns = 3`, centred on the main window. Tiles reuse the existing borrowed snapshots, capturing the current page first; missing snapshots fall back to the ribbon icon/title. No new window class or persistent screenshot is added. Other popup instances keep list mode by default.
- A main-window message filter handles the chord before child controls consume Tab, consumes Tab key-up, and is removed on teardown. It handles main-window descendants and the history popup, leaves modal/other windows alone, and cancels when a candidate closes or the selector loses activation. The shortcut remains available with both history buttons hidden. Source/design reviewed; native PDF/browser focus and quick-key timing remain for the operator's trial.
- The ribbon tooltip header is exactly `Istoricul navigarii (CTRL+TAB)`, as requested; its body and Help explain the chord. Notes remain in 1.1.5.3.

## Files owned by this task

Operator follow-up: the Ctrl+Tab snapshot area is doubled on both axes, keeping the existing caption/icon size and padding. The selector reduces its column count to fit the screen and uses its existing scrolling/selection visibility for longer lists.

- `src/KBot.Controls/CaptionBar/KBotCaptionBar.vb`
- `src/KBot.Controls/CaptionBar/KBotCaptionBar.HelpButton.vb`
- `src/KBot.Controls/CaptionBar/KBotCaptionBar.HistoryButton.vb` (new)
- `src/KBot.Controls/CaptionBar/KBotCaptionBar.md`
- `src/KBot.Controls/Popup/CustomPopup.vb`
- `src/KBot.Controls/Popup/CustomPopup.Painting.vb`
- `src/KBot.Controls/Popup/CustomPopup.Input.vb`
- `src/KBot.Controls/Popup/CustomPopupItem.vb`
- `src/KBot.Controls/Popup/CustomPopup.RowFont.vb` (new)
- `src/KBot.Controls/Popup/CustomPopup.Previews.vb` (new)
- `src/KBot.Controls/Popup/CustomPopup.md`
- `src/KBot.Controls/KBot.Controls.vbproj` (partial nesting; FileVersion 1.85.0.3)
- `src/KBot.Common/AppSettings.vb`
- `src/KBot.Common/KBot.Common.vbproj` (FileVersion 1.5.14.1)
- `src/KBot.App/Avacont/AvacontMain.vb`
- `src/KBot.App/Avacont/AvacontMain.Designer.vb`
- `src/KBot.App/Avacont/AvacontMain.Sidebar.vb`
- `src/KBot.App/Avacont/AvacontMain.Navigation.vb` (new)
- `src/KBot.App/Avacont/AvacontMain.Switcher.vb` (new)
- `src/KBot.App/KBot.App.vbproj` (partial nesting; FileVersion 1.1.5.3)
- `src/KBot.App/Director/DirectorMode.vb`
- `src/KBot.App/Setari/Views/SetariAplicatieView.vb`
- `src/KBot.App/Setari/Views/SetariAplicatieView.Designer.vb`
- `src/KBot.App/HelpContent/contabil/fereastra.md`
- `src/KBot.App/HelpContent/contabil/ribbon.md`
- `src/KBot.App/HelpContent/contabil/setari.md`
- `src/KBot.App/HelpContent/contabil/panou-lateral.md`
- `docs/release-notes/NOUTATI.md`
- `docs/worklog/KBOT_STATUS.md`
- `docs/worklog/state/KBOT_STATUS_0090-0099.md`
- This worklog.

## Status

Implemented; source/design review only. No build, tests, runtime checks, HelpCheck or publication were run. Awaiting the operator's trial and explicit confirmation before deployment-task registration. The plan approval is not the confirmation of the trial; no registration or local Git mutation has been performed.
