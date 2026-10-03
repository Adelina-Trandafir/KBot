# SLICE-0106 — capture folder null, quieter Adobe lock retries, right click keeps the PDF

Operator request, 03.10.2026. Three things seen while refreshing the info of an angajament.

## What changed and why

1. **«Capturile nu au putut fi trimise: Value cannot be null. (Parameter 'path1')»** on every
   refresh of the info of an angajament.
   Cause (read in code, matches the stack in the log): `CapturaStore.FolderSau(folder As String)`
   returned `If(IsNullOrWhiteSpace(folder), Folder, folder)`. VB is case-insensitive, so inside
   that method `Folder` is the PARAMETER `folder`, not the shared property `Folder` - it was
   `Nothing` exactly in the branch that wanted the default. `Path.Combine(Nothing, cod)` then threw
   in `FolderAngajament`, from `AleSale` / `CapturileDe` / `TrimiteCapturileAsync`. Fixed with
   `CapturaStore.Folder`. `AsociereStore.FolderSau` had the identical line (same defect, not yet
   reported); fixed the same way. This is the known trap in the memory note on VB.NET shadowing.
2. **`SignedPdfFiles.ReadShared` / `ReadWhenSettledAsync.Retry` IOExceptions** in the error log.
   These are the settle loop waiting for Adobe to release a file it is mid-write on; the loop
   retries by design. The defect was only that every attempt wrote two error entries
   (`ReadShared` + `.Retry`). Now the retry path reads through an unlogged private
   `ReadSharedCore`; the last lock exception is logged ONCE (`...ReadWhenSettledAsync.Timeout`)
   and only if the file never became readable within the timeout. `ReadShared` (public, single
   shot) still logs and rethrows. Behaviour of the loop itself is unchanged.
3. **Right click on the tree in the DDF view (and the ORD view) reloaded the PDF.**
   `Tree_NodeMouseUp` moved the view context, pushed it to the active page and started
   `EnsureSignedPdfAsync` for any button. Now the right button only opens the context menu: both
   menus take their node from the clicked item (`pNode` / its payload), not from the view's
   selection state, so nothing else was needed. The PDF reloads on the LEFT click only. Keyboard
   Enter is unaffected (it raises the event with the left button).

## Files touched

- `src/KBot.Common/CapturaStore.vb` - `FolderSau`
- `src/KBot.Common/AsociereStore.vb` - `FolderSau`
- `src/KBot.App/Views/SignedPdfFiles.vb` - `ReadShared`, new `ReadSharedCore`, `ReadWhenSettledAsync`
- `src/KBot.App/Views/DdfView.vb` - `Tree_NodeMouseUp`
- `src/KBot.App/Views/OrdView.vb` - `Tree_NodeMouseUp`

## Test results

`dotnet build src\KBot.App\KBot.App.vbproj`: 0 warnings, 0 errors. Nothing run, nothing seen on
screen (operator rule: no test runs unless asked).

## Left unverified / deferred

- Not run: the refresh of an angajament's info (the capture message should be gone), a right click
  in DDF and ORD (menu opens, PDF stays), a left click (PDF still loads).
- Visual side effect, by design: on a right click the tree's highlight moves to the clicked node
  (tree control behaviour) while the page keeps showing the previous document until a left click.
  If the operator wants the highlight to stay put on a right click, that is a change in
  `AdvancedTreeControl.Overrides.vb` - not made.
- The 10:33:29 warning «Salvare după semnătură ... Adobe nu a primit tastele» in the same log is a
  separate matter and was not touched.
- Other reported lines were not addressed: `BudgetCheckForm.FillGrid` NullReferenceException at
  `BudgetCheckForm.vb:88` (10:20:49), and the logout-on-close «Sesiune necunoscută» (10:15:00).
  Both are outside the three items asked for.
- Help: no change (no new operator-visible text or flow; a right click now does less).
