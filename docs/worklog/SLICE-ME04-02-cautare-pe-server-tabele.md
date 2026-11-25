# SLICE-ME04-02 - KBotAdmin, Baze Date > Tabele: condițiile filtrului se caută pe server (16.11.2026)

Slice follows ME04-01 (same request, second part). `KBotAdmin.vbproj` builds clean (0 errors, 0 warnings); `py_compile -W error` clean on
`admin_dbview.py`. Never run against a database, never seen on screen.

## What changed and why

Request: in «Tabele», searching in the grid searched only the rows already downloaded; on tables with thousands of rows she had to press
«Încarcă încă» many times before the row showed. Adelina chose: the existing column filters become server-side.

- **Server** (`PYTHON/routes/admin_cli/admin_dbview.py`): `db-table` and `db-rows` take `--filter-json`, a list of
  `{"c": column, "o": op, "a": operand, "b": operand2}` with ops `eq ne contains ncontains begins nbegins ends nends lt gt between empty nempty`.
  All conditions are ANDed, together with `--where-json`. Columns are checked against the real columns, operands travel as SQL parameters,
  `%`, `_` and `\` typed by the user are escaped in LIKE patterns. Paging (`offset`/`limit`/`has_more`) works on the filtered result.
- **Client** (`DbBrowserView.vb`): when a CONDITION (Contains, Equals, Between, ...) is set or lifted on a column header, the page asks the server
  again for the same table with the conditions, shows the first 1000 matches («Încarcă încă» continues in the matches). Numbers are sent with a
  point, dates as year-month-day (read in the grid's own culture). The result is cached apart per search (`NavNode.SearchJson` is part of the view key).
  The header filters stay lit after the reload (stored on the view, put back). The info line says «căutat pe server în tot tabelul».
  A condition whose operand cannot be read is left out, as the grid leaves it inert. Blob, tick-box and time columns are not searched on the server.
- The ticked-values list of the filter menu stays LOCAL (over the rows loaded); it is not sent to the server.
- Queries («_Interogari») keep their local filtering.

## Files touched
- `PYTHON/routes/admin_cli/admin_dbview.py`
- `KBotAdmin/Views/DbBrowserView.vb`
- this file, `state/KBOT_STATUS_ME04.md`, index line in `KBOT_STATUS.md`

## Unverified / deferred
- NEVER RUN. The server file must be copied by hand to the server (rule: no deploy by me); until then the client's new `--filter-json` argument is refused by the old server.
- The grid's own filter engine is applied again locally to the rows the server returned; if a local rule differs from the SQL one (text collation, date
  with time), the grid could hide a row the server matched. The info line shows the loaded count to notice this.
- Sorting is still local (over the loaded rows).
- The value list in the filter menu shows only values of loaded rows.
- `/ssh` registration only after Adelina confirms a test.
- Help: none (KBotAdmin is the owner's private tool).
