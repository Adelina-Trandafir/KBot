# SLICE-0110-05 - JS data grid (read-only) for the web area (operator request, 05.10.2026)

Part of the site plan (`SLICE-0110`). The web counterpart of `KBotDataView` WITHOUT editing, with filtering and grouping.
No page uses it yet: the views come in 0110-06.

## What changed and why
New folder `PYTHON/static/js/dgv/` (the copy Flask serves, like the other components; not duplicated in `JS_COMPONENTS/`)
and `PYTHON/static/css/dgv.css`.

- `engine.js` - the pure half, no DOM: value types (text / number / datetime / boolean), display formats (Access vocabulary
  of the desktop grid: general, fixed, standard, currency, euro, percent, shortDate, generalDate[Sec|Ms], shortTime, longTime[Ms],
  yesNo, trueFalse, onOff; or a `formatString` date pattern), the 13 filter operators offered per value type exactly as the desktop
  `KBotFilterEngine` does (an unreadable operand is inert), blanks sort first, aggregates per value type (sum count average min max
  countDistinct countEmpty countTrue countFalse first last; a not-offered aggregate throws), multi-level grouping with `keyPattern`
  (regex over the DISPLAYED text, key read in the column's type so a date key sorts as a date), collapse state, header / footer bands.
- `datagrid.js` - `DataGrid(container, options)`: virtualized rows (fixed height, only the window is in the page), sticky header and
  footer, frozen leading columns (`frozen`), resizable columns, auto width from the first 200 rows, click-to-sort (asc, desc, none),
  per-column filter popup (searchable checklist of the display values + one condition with 1 or 2 operands; `date` / `datetime-local`
  inputs on date columns), header context menu (sort, filter, group by this column, collapse / expand all), group header / footer bands
  with aggregates and `{0}` title, `{1}` value, `{2}` count captions, grid footer with totals, tooltip for clipped cells, keyboard
  (up / down / home / end), single-row selection, three looks: `modern`, `classic`, `dark`.
  API: `setColumns`, `setRows`, `sortBy`, `setFilter`, `clearFilters`, `setGroups`, `groupBy`, `ungroup`, `clearGrouping`,
  `collapseAll`, `expandAll`, `getSelectedRow`, `autoSizeColumns`, `setTheme`, `destroy`; properties `shownRowCount`, `rowCount`,
  `isFiltered`, `isGrouped`, `sort`; callbacks `onSelect`, `onRowDblClick`, `onSort`, `onFilter`, `onGroupToggle`.
  Unknown keys throw (no silent no-ops). Cell text is always `textContent` (no HTML from data).
- Not done on purpose, as in the desktop grid: no editing, no multi-row selection, no column reordering, no frozen rows.
- Decisions: the Calendar component of `JS_COMPONENTS` is NOT used for date operands - the native date inputs do the job inside a
  popup and the Calendar needs its own manager / z-index plumbing; revisit if the operator wants the K-BOT look there. No `Combobox`
  inside the grid either (the operator list is a plain select). No ListenerTracker: listeners hang on one `AbortController`.

## Files touched
`PYTHON/static/js/dgv/engine.js`, `PYTHON/static/js/dgv/datagrid.js`, `PYTHON/static/css/dgv.css` (all new).

## Checked
- Engine, with a throw-away script (not kept in the repository, no test project touched): formats, date parsing, sort, every
  filter kind, grouping by day with a key pattern, two levels, collapse and collapsed-by-default, aggregates - all passed.
- In the browser pane with a stub page of 5000 rows: virtualized (19-25 rows in the page at any scroll position), sort by header
  click, filter popup (checklist and condition, together), context menu, grouping one and two levels, collapse by clicking a band,
  collapse / expand all, footer totals recomputed on the filtered rows, theme switch, header / body / footer columns aligned to the pixel,
  frozen column stays under horizontal scroll, no console errors.
- Found and fixed on the way: autosize ran before the rows existed (widths too small); header cells were wider than body cells
  (`box-sizing`); scrolling rendered through a frame callback that does not run in a hidden tab (now synchronous).

## Not done / to do
- NOT seen on screen: the pane refused screenshots (page never finished drawing), so colors, borders, the three themes and the popup
  look are unchecked visually. First thing to do in 0110-06: open it and look.
- No keyboard path inside the popups beyond Tab / Esc; no touch-specific behavior; no column hide/show menu.
- Server not touched; nothing deployed for this slice.
