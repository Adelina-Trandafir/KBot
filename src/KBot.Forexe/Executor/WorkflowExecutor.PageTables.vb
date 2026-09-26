Imports KBot.Common

' =============================================================================
'  PageTables - read-only reads of tables the FOREXE page shows (slice 0084).
'
'  The first one is the «Operatiuni necorectate» table of the landing page the
'  Conectare workflow leaves the browser on. It is read with one EvaluateAsync,
'  with no dependency on the floating K-BOT menu (ForexeWatch.js): the read
'  happens right after the login, before the menu may be installed.
' =============================================================================
Partial Public Class WorkflowExecutor

    ' Finds the <h4> whose text, without diacritics, is "Operatiuni necorectate", takes the
    ' first <table> after it inside the same container and returns every body row as an
    ' object keyed by its (ASCII, lower-case) column header. Also counts the pages the
    ' pagination under the table offers, so a second page is not lost silently.
    ' Answer: {"found": bool, "pages": n, "rows": [{"<header>": "<text>", ...}, ...]}.
    Private Const UncorrectedOperationsJs As String =
        "() => {" &
        "  const plain = s => (s || '').normalize('NFD').replace(/\p{M}/gu, '')" &
        "                    .replace(/\s+/g, ' ').trim();" &
        "  const key = s => plain(s).toLowerCase();" &
        "  const h4 = Array.from(document.querySelectorAll('h4'))" &
        "               .find(h => key(h.textContent) === 'operatiuni necorectate');" &
        "  if (!h4) return JSON.stringify({found: false, pages: 0, rows: []});" &
        "  const box = h4.parentElement;" &
        "  const table = box ? box.querySelector('table') : null;" &
        "  if (!table) return JSON.stringify({found: true, pages: 0, rows: []});" &
        "  const heads = Array.from(table.querySelectorAll('thead th')).map(th => key(th.textContent));" &
        "  const rows = [];" &
        "  table.querySelectorAll('tbody > tr').forEach(tr => {" &
        "    const cells = Array.from(tr.children).filter(c => c.tagName === 'TD');" &
        "    if (cells.length === 0) return;" &
        "    const row = {};" &
        "    cells.forEach((td, i) => {" &
        "      const h = heads[i] || ('col' + i);" &
        "      if (h === 'sector - sursa - indicator') {" &
        "        row[h] = (td.textContent || '').replace(/\s+/g, '');" &
        "        const tip = td.querySelector('[data-original-title], [title]');" &
        "        row['ssi_title'] = tip ? (tip.getAttribute('data-original-title') || tip.getAttribute('title') || '').trim() : '';" &
        "      } else {" &
        "        row[h] = (td.innerText || td.textContent || '').replace(/\s+/g, ' ').trim();" &
        "      }" &
        "    });" &
        "    rows.push(row);" &
        "  });" &
        "  const pager = box.querySelector('.pagination .goto');" &
        "  const pages = pager ? Math.max(1, pager.querySelectorAll('span[title]').length) : 1;" &
        "  return JSON.stringify({found: true, pages: pages, rows: rows});" &
        "}"

    ''' <summary>
    ''' The «Operațiuni necorectate» table of the page on screen, as JSON
    ''' (<c>{"found", "pages", "rows"}</c>, see <see cref="UncorrectedOperationsJs"/>).
    ''' An empty string when there is no page. Throws when the page cannot answer: the caller
    ''' decides what an unreadable page means.
    ''' </summary>
    Public Async Function ReadUncorrectedOperationsAsync() As Task(Of String)
        If _page Is Nothing OrElse _page.IsClosed Then Return String.Empty
        Try
            Dim json As String = Await _page.EvaluateAsync(Of String)(UncorrectedOperationsJs)
            Return If(json, String.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("WorkflowExecutor.ReadUncorrectedOperationsAsync", ex)
            Throw
        End Try
    End Function

End Class
