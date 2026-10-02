# SLICE-0101-01 — ORD and DDF: the nav bar stays on a month / «Toate…» node; its «Documente» tab holds the print list (operator request, 02.10.2026)

Branch: `SLICE-0100-Multithreading` (nothing committed; files only staged, as asked).

## What changed and why

Slice 0099 made a month / «Toate ordonanțările» / «Toate reviziile» node show the print list INSTEAD of the pages: the nav
bar and both pages were hidden, so the operator lost the node's data (the lines of all its ordonanțări / revizii). The
operator wants both: the list stays, and the data comes back.

- **The nav bar is always visible**, on leaves and on month / «Toate…» nodes alike.
- **«Vizualizare»** (ORD) / «Vizualizare» (DDF) on a month or the root shows the lines of every ordonanțare / revizie under it.
  That was already what the page received for a root (`_nodeLinii` / `_nodeRows`); it was only hidden.
- **The document tab** is named **«Documente»** on a month / root and shows the print list (`PrintListPage`, the grid with the
  files, «Generează și imprimă», «Salvează local»). On a leaf it keeps its name («Document» in ORD, «Document PDF» in DDF) and
  shows the PDF page. The list shows only while that tab is the selected one; on the other tab the data page shows.
- Code, both `OrdView.vb` and `DdfView.vb` the same way:
  - `IsListNode()` (a month / root with its list) and `IsListTab(key)` (the document tab on such a node); in DDF a file picked
    from the (parked) «Fișiere» page counts as a document, so it keeps the tab.
  - `PushToActivePage` renames the tab (`navSub.SetItemText`), keeps `ShowPageSurface` / `ShowListSurface` by `IsListTab`, and
    when it comes back from the list tab (no page was on screen) calls `ActivatePage` for the selected tab.
  - `ActivatePage`: on the list tab it puts the page away and shows the list (creates no page); otherwise it shows the pages.
  - `ShowListSurface` no longer hides `navSub`.
- `KBot.Controls/NavList/KBotNavList.vb`: new `SetItemText(key, text)` (the item's `Text` is a plain property and does not
  repaint by itself; this one sets it and invalidates the layout).

## Files touched

`src/KBot.App/Views/OrdView.vb`, `src/KBot.App/Views/DdfView.vb`, `src/KBot.Controls/NavList/KBotNavList.vb`;
help: `SLICE-0000-38-ajutor-documente-pe-luna.md`; release notes `NOUTATI.md`; status files.

## Test results

No tests written or run (operator: no tests). `dotnet build src/KBot.App` → 0 warnings, 0 errors. The app was not run.

## Left unverified / deferred

- Nothing seen on screen: the tab rename, the switch between the list and the data page, and above all the move **leaf on the
  «Document» tab → month** (the document in Adobe must be cleared before the page is hidden: it is, by the same
  `SetContext`-then-hide order as in 0099, but unseen) and **month on «Documente» → leaf** (the document page is shown again).
- The caption «Documente» is wider than «Document»; the bar's items are `AutoSize`, so it should fit — not looked at.
- The tabs' guided-tour parts (`item:document`) use the key, so they still find the tab under either name.
