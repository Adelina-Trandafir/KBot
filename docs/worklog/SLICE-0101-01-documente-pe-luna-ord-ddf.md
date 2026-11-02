# SLICE-0101-01 — ORD and DDF: the nav bar stays on a month / «Toate…» node; its «Documente» tab holds the print list (operator request, 02.10.2026)

Branch: `SLICE-0100-Multithreading` (nothing committed; files only staged, as asked).

## What changed and why

0099 replaced pages with the print list on month / «Toate ordonanțările» / «Toate reviziile» nodes, hiding nav and both pages, including all the node's lines. Operator wants list and data available together.

- **The nav bar is always visible**, on leaves and on month / «Toate…» nodes alike.
- **«Vizualizare»** in ORD/DDF shows all underlying ordonanțare/revizie lines on month/root, already supplied through `_nodeLinii` / `_nodeRows` but previously hidden.
- **Document tab:** month/root **«Documente»** shows `PrintListPage` (files grid, «Generează și imprimă», «Salvează local»); leaf keeps «Document» (ORD) / «Document PDF» (DDF) and PDF page. Print list appears only on its selected tab; the other tab shows data.
- Code, both `OrdView.vb` and `DdfView.vb` the same way:
  - `IsListNode()` (a month / root with its list) and `IsListTab(key)` (the document tab on such a node); in DDF a file picked from the (parked) «Fișiere» page counts as a document, so it keeps the tab.
  - `PushToActivePage` renames the tab (`navSub.SetItemText`), keeps `ShowPageSurface` / `ShowListSurface` by `IsListTab`, and when it comes back from the list tab (no page was on screen) calls `ActivatePage` for the selected tab.
  - `ActivatePage`: on the list tab it puts the page away and shows the list (creates no page); otherwise it shows the pages.
  - `ShowListSurface` no longer hides `navSub`.
- `KBot.Controls/NavList/KBotNavList.vb`: new `SetItemText(key, text)` (the item's `Text` is a plain property and does not repaint by itself; this one sets it and invalidates the layout).

## Files touched

`src/KBot.App/Views/OrdView.vb`, `src/KBot.App/Views/DdfView.vb`, `src/KBot.Controls/NavList/KBotNavList.vb`; help: `SLICE-0000-38-ajutor-documente-pe-luna.md`; release notes `NOUTATI.md`; status files.

## Test results

No tests written or run (operator: no tests). `dotnet build src/KBot.App` → 0 warnings, 0 errors. The app was not run.

## Left unverified / deferred

- Nothing seen on screen: the tab rename, the switch between the list and the data page, and above all the move **leaf on the «Document» tab → month** (the document in Adobe must be cleared before the page is hidden: it is, by the same `SetContext`-then-hide order as in 0099, but unseen) and **month on «Documente» → leaf** (the document page is shown again).
- The caption «Documente» is wider than «Document»; the bar's items are `AutoSize`, so it should fit — not looked at.
- The tabs' guided-tour parts (`item:document`) use the key, so they still find the tab under either name.
