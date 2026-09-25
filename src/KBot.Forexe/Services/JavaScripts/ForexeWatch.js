(function () {
    'use strict';

    // =========================================================================
    //  ForexeWatch.js - the floating K-BOT menu inside the FOREXE page (slice 0073).
    //
    //  Two jobs:
    //    1. A small always-visible panel with zoom in / zoom out / 100% and the
    //       state of the watcher, plus manual Start / Gata / Renunta buttons.
    //    2. Watching what the OPERATOR does: when a click matches the start of a
    //       known operation the watcher arms itself; when a click matches that
    //       operation's save button it waits for the page to settle and then
    //       reports "finished" to .NET together with the angajament code read
    //       from the page header. .NET (RecorderForm -> ForexeRunner -> shell)
    //       downloads that angajament and opens its history for the interval.
    //
    //  Installed twice on purpose, like Recorder.js: once through AddInitScript
    //  (future navigations) and once through EvaluateAsync (the page already on
    //  screen). The guard below makes the second install harmless. State that must
    //  outlive a navigation (an operation in progress, a pending save) lives in
    //  sessionStorage; the zoom factor lives in localStorage.
    //
    //  Callback name _kbotWatchCallback is its own: _kbotRecorderCallback,
    //  _clickMonitorCallback, _keyMonitorCallback and _wicketMonitorCallback are
    //  taken, and ExposeFunctionAsync throws on a second registration.
    //
    //  3. (slice 0074) Saying WHICH angajament the page shows: whenever the code in
    //     the header changes - a navigation, a search the operator made by hand, a
    //     Wicket re-render - a "page" event carries it to .NET, and the shell selects
    //     that node in its tree without running the robot. Sent again on resume, so
    //     the page the robot just opened is reported the moment it is handed back.
    //
    //  4. The operator's PAGE STYLES (operator, 21.09.2026): the CSS rules kept in the
    //     K-BOT settings («Setari» -> «Pagina FOREXE») are written into a <style> element
    //     the moment this script runs - before the first paint, on EVERY document, whatever
    //     address Wicket moved to (contract?6, contract_edit_rand?7, receptie_edit?7,
    //     wicket/page?9 ...). A stylesheet is not touched by the Ajax re-renders that
    //     replace pieces of the page, so nothing has to be re-applied. Each declaration is
    //     marked !important to beat FOREXE's inline styles. .NET hands the rules over with
    //     configure(); they are kept in localStorage so the next load has them at once.
    //     The sheet stays ON while the robot drives (operator, 21.09.2026: it used to go
    //     dark for the whole job, and the menu the rules hide was in view until the job
    //     ended). A robot click whose target the rules hide lifts the sheet for that one
    //     click and no longer (liftStyles / hiddenByStyles, called by the Click action).
    //     A rule may name ONE page (its "page", compared without the query string): it is
    //     in the sheet only while the document's address is that page (pageMatches).
    //
    //  5. The developer tools stay shut unless the settings allow them: F12,
    //     Ctrl+Shift+I / J / C and Ctrl+U are swallowed before Chromium sees them, and the
    //     context menu (the «Inspect» entry lives there) is not shown at all.
    //
    //  6. «Renunta» (the cancel button of any FOREXE form or modal) ENDS the watched
    //     operation: nothing is reported as finished, so the shell downloads nothing.
    //
    //  7. A save that changes NOTHING is stopped (operator, 21.09.2026): when the edit
    //     form of a reservation or of a reception is on screen its value fields (for a
    //     reception also Tip, Data and Descriere - 25.09.2026) are snapshotted the moment
    //     the form appears (MutationObserver + the beat); a click
    //     on that form's save button - or Enter in one of its fields - with the very same
    //     values is swallowed and a blocking message tells the operator to press
    //     «Renunta» instead. Never while the robot drives, and never for a button inside
    //     a modal dialog (the «Renuntare» question has a btn-success «Da» of its own). No
    //     element id is used anywhere: FOREXE's ids change from render to render.
    //
    //  8. The floating menu folds: a small button in its title row hides everything but
    //     the title row (remembered in localStorage).
    //
    //  9. THE VEIL (operator, 21.09.2026; 24.09.2026 - solid, no blur): while a robot job
    //     drives the page an opaque backdrop in K-BOT's theme colour (config.veil, the
    //     colour of the view that hosts the browser) covers it, and a card in the middle
    //     asks the operator to wait, with the name of the job. The robot's work is not
    //     seen through it at all. The veil is a class on <html> plus a stylesheet written
    //     at script start, so a navigation in the middle of the job comes up covered from
    //     its first paint. The veil takes no pointer events, so the robot's clicks pass
    //     through it as if it were not there (the operator's clicks are stopped by the
    //     window lock on the .NET side).
    //
    // 10. DARK MODE (operator, 21.09.2026): when K-BOT itself runs a dark scheme the page is
    //     inverted (invert + hue-rotate on the root element - the one element whose filter
    //     does not break position:fixed), with images and the K-BOT elements inverted back.
    //     It is a flag in the same config as the rules (darkMode), so it is in place before
    //     the first paint and follows a theme change at once.
    //
    // 11. WHEN THE SHEETS GO IN (operator, 21.09.2026 - «the blur / the dark colours / the
    //     hidden menu bar come back only after a moment»): Playwright's init script runs
    //     before the parser has produced <html>, so at that moment there is NOTHING to hang
    //     a <style> on (document.head and document.documentElement are both null) and the
    //     first install used to fail quietly; the 2 s beat was the first thing that put the
    //     sheets back. Now the install waits for the root element (a MutationObserver on the
    //     document fires the moment <html> exists, long before the first paint), boot()
    //     installs again, and three watchers keep the sheets and the busy class in place
    //     at once rather than at the next beat: the Wicket Ajax events (/ajax/call/complete,
    //     /dom/node/added - a re-render that swaps the page's pieces), a MutationObserver on
    //     <html> (its class) and one on <head> (its children). The beat stays as the last
    //     safety net.
    //
    // 12. (slice 0076) WHAT A SAVE WAS ABOUT travels with the "finished" event (its "data"):
    //     - a reception EDIT is armed by the eye of its row on the receptions tab (the same
    //       eye «Prelucrare Completa» presses to open «Modifica receptie»); the row's date is
    //       kept at that click and the form's date at the save, and the save is known by
    //       that date. A NEW reception carries the form's date too, but .NET reads the last
    //       row for it (operator, 23.09.2026).
    //     - a reservation carries what «Rezervari Angajament.wfl» would have read for it: the
    //       indicator's row of the tab0 table (read once the save is confirmed, when the page
    //       is back on tab0) and its budget table (read at the save click, on the edit page -
    //       the very table the complete flow scrapes behind the eye). .NET keeps one per
    //       indicator until the operator says the reservations are done.
    //     A form closed WITHOUT a save (the page's «Inapoi», another tab) ends the operation
    //     at the next beat, like «Renunta» does.
    //
    // 13. (slice 0076) THE MARKERS. Before a reservation's motive («Continua» of the motive
    //     modal) or a reception's form («Salveaza») is submitted, the click is held, K-BOT is
    //     asked for the ids the save will become (the server reserves them), the marker is
    //     appended to the text - «(IDREV: n)» to the motive, «(IDRH: n; IDR: m)» to the
    //     reception's description, replacing an older one - and the click is replayed. FOREXE
    //     copies that text into its history, so the ingest knows which history row became
    //     which record. When K-BOT cannot answer in time the save goes through WITHOUT a
    //     marker (said on the console): the operator's work is never held hostage.
    //
    //  EVERY selector in OPS below is copied from the workflows in
    //  Workflows/Creare (Creare Angajament, Incarca Rezervare, Rezervare si
    //  Receptie) and from «Prelucrare Completa» - they are the selectors the robot
    //  itself clicks, so they are as verified as anything in this repo.
    // =========================================================================

    if (window._kbotWatchInstalled) { return; }
    window._kbotWatchInstalled = true;

    var KEY_STATE = 'kbotWatchState';      // sessionStorage: JSON of the state below
    var KEY_SUSPEND = 'kbotWatchSuspend';  // sessionStorage: '1' while a robot job runs
    var KEY_SUSPEND_MSG = 'kbotWatchSuspendMsg'; // sessionStorage: what the veil says meanwhile
    var KEY_ZOOM = 'kbotWatchZoom';        // localStorage: zoom factor as text
    var KEY_POS = 'kbotWatchPos';          // localStorage: JSON {right, bottom} of the menu
    var KEY_CONFIG = 'kbotWatchConfig';    // localStorage: JSON {devTools, rules} from .NET
    var KEY_COLLAPSED = 'kbotWatchFold';   // localStorage: '1' while the menu is folded

    var ZOOM_STEP = 0.1;
    var ZOOM_MIN = 0.5;
    var ZOOM_MAX = 2.0;

    var PENDING_POLL_MS = 400;
    var PENDING_TIMEOUT_MS = 30000;
    var MENU_KEEPALIVE_MS = 2000;

    // The header of an open angajament, in both modify and freshly created state.
    var SEL_COD = '.well.well-small h4 span:nth-child(2)';
    var SEL_ERROR = '.feedbackPanelERROR, .alert.alert-danger';
    var SEL_MODAL = 'div.modal-content textarea[name=\'modal:form:motiv\'], .modal.in, .modal[style*=\'display: block\']';
    var SEL_BUSY = '#animlogo';
    // Slice 0076 - all of them from «Prelucrare Completa» / Workflows/Creare.
    var SEL_FORM_RECEPTIE = 'form.form-horizontal select[name=\'receptie\']';
    var SEL_DATA_RECEPTIE = 'form.form-horizontal input[name=\'data\']';
    var SEL_DESCRIERE_RECEPTIE = 'form.form-horizontal textarea[name=\'descriere\']';
    var SEL_MOTIV = 'textarea[name=\'modal:form:motiv\']';
    var SEL_LISTA = 'table.table-striped.table-bordered.table-condensed';
    var SEL_BUGET = 'table.table-bordered.table-hover.table-condensed';
    // How long a save waits for K-BOT's marker before it goes through without one.
    var MARCAJ_TIMEOUT_MS = 8000;
    // Any K-BOT marker, as prelucrare_helpers.py reads it (section 13).
    var RE_MARCAJ = /\s*\(\s*ID(?:REV|RH|R)\s*:\s*\d+(?:\s*;\s*ID(?:REV|RH|R)\s*:\s*\d+)*\s*\)/gi;
    // Any «Renunta» button of a FOREXE form or modal (btn-danger on the angajament forms,
    // btn-default on the reception form): pressing it ends the operation.
    var RULE_CANCEL = { closest: 'button', text: 'Renunta' };

    // ── Operation rules ──────────────────────────────────────────────────────
    // start / finish: a click COUNTS when el.closest(rule.closest) exists and, when
    // given, the matched element's text satisfies text / textStart (compared without
    // diacritics and case). "when" must exist in the page for the START rules to be
    // considered at all. "done" is what the page must show for the save to count.
    var OPS = {
        'angajament': {
            label: 'Angajament nou',
            start: [{ closest: 'a', text: 'Angajament nou' }],
            // The FINAL save is the first btn-success in the form group; the second one is
            // the intermediate "save this row and add another" (Creare Angajament.wfl 3.8).
            finish: [{ closest: '.form-group button.btn-success:nth-child(1)' }],
            done: function () { return !!readCod() && exists('li.tab0'); }
        },
        'rezervare': {
            label: 'Rezervare',
            when: 'li.tab0.active',
            start: [
                { closest: 'button.btn-default.btn-small', textStart: 'Adaug' },
                { closest: 'table.table-striped a:has(.glyphicon-eye-open)' }
            ],
            finish: [
                { closest: 'button.btn-success.btn-small:nth-child(1)' },
                { closest: 'div.modal-footer button.btn-success' }
            ],
            done: function () { return !!readCod() && exists('li.tab0.active'); }
        },
        'receptie': {
            label: 'Recepție nouă',
            when: 'li.tab1.active',
            start: [{ closest: 'button', text: 'Adauga' }],
            finish: [{ closest: 'form.form-horizontal button', text: 'Salveaza' }],
            done: function () {
                return !!readCod() && !exists(SEL_FORM_RECEPTIE);
            }
        },
        // Slice 0076: the eye of a row on the receptions tab opens «Modifica receptie» (the
        // same eye «Prelucrare Completa» presses, tbody tr:nth-child(idx) .glyphicon-eye-open).
        // Saved with the same button as a new reception.
        'receptie-modificare': {
            label: 'Modificare recepție',
            when: 'li.tab1.active',
            start: [
                { closest: 'table.table-striped tbody tr a:has(.glyphicon-eye-open)' },
                { closest: 'table.table-striped tbody tr .glyphicon-eye-open' }
            ],
            finish: [{ closest: 'form.form-horizontal button', text: 'Salveaza' }],
            done: function () {
                return !!readCod() && !exists(SEL_FORM_RECEPTIE);
            }
        }
    };

    // The form each operation edits (slice 0076): once it has been on screen, its going away
    // with no save pending means the operator left without saving (section 12).
    var OP_FORMS = {
        'rezervare': 'input[name^=\'tableContainer:\']',
        'receptie': SEL_FORM_RECEPTIE,
        'receptie-modificare': SEL_FORM_RECEPTIE
    };

    // ── State ────────────────────────────────────────────────────────────────
    var state = loadState();
    var suspended = false;
    var pendingTimer = null;
    // The header code last reported through "page"; null = nothing reported yet.
    var lastCodReported = null;
    // The operator's page choices from .NET (see section 4) and the sheet they live in.
    var config = loadConfig();
    var styleEl = null;
    // The veil of section 9: its stylesheet, the element, and the line the card shows.
    var busyStyleEl = null;
    var veil = null;
    var veilText = null;
    var DEFAULT_BUSY_TEXT = 'K-BOT lucrează în pagină';
    var menu = null;
    var menuBody = null;
    var btnFold = null;
    var lblStatus = null;
    var btnStart = null;
    var btnFinish = null;
    var btnCancel = null;
    var lblZoom = null;

    // The slice 0076 fields: formSeen (the op's form has been on screen), rowDate / formDate
    // (the reception's date at the eye / at the save), rezBefore (the indicator codes of the
    // tab0 table when a reservation started), rezIndicator (the code of the row whose eye was
    // pressed), rezBuget (the budget table read at the save click).
    function emptyState() {
        return {
            op: null, label: '', startedAt: null, codAtStart: '', pendingSince: null,
            formSeen: false, rowDate: '', formDate: '',
            rezBefore: null, rezIndicator: '', rezBuget: null
        };
    }

    function loadState() {
        try {
            var raw = sessionStorage.getItem(KEY_STATE);
            if (raw) { return JSON.parse(raw); }
        } catch (ignored) { }
        return emptyState();
    }

    function saveState() {
        try { sessionStorage.setItem(KEY_STATE, JSON.stringify(state)); } catch (ignored) { }
    }

    function resetState() {
        state = emptyState();
        saveState();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────
    function norm(s) {
        return (s || '').replace(/\s+/g, ' ').trim();
    }

    // Diacritics and case are dropped on BOTH sides so the rules can be plain ASCII.
    function fold(s) {
        var t = norm(s).toLowerCase();
        try { t = t.normalize('NFD').replace(/[\u0300-\u036f]/g, ''); } catch (ignored) { }
        return t;
    }

    function exists(sel) {
        try { return !!document.querySelector(sel); } catch (e) { return false; }
    }

    function isVisible(el) {
        if (!el) { return false; }
        try {
            var cs = getComputedStyle(el);
            if (cs.display === 'none' || cs.visibility === 'hidden') { return false; }
            return el.offsetWidth > 0 || el.offsetHeight > 0 || el.getClientRects().length > 0;
        } catch (e) { return false; }
    }

    function anyVisible(sel) {
        var list;
        try { list = document.querySelectorAll(sel); } catch (e) { return false; }
        for (var i = 0; i < list.length; i++) {
            if (isVisible(list[i])) { return true; }
        }
        return false;
    }

    function readCod() {
        var el = null;
        try { el = document.querySelector(SEL_COD); } catch (ignored) { }
        return el ? norm(el.innerText) : '';
    }

    function wicketBusy() {
        var el = null;
        try { el = document.querySelector(SEL_BUSY); } catch (ignored) { }
        if (!el) { return false; }
        try { return getComputedStyle(el).display !== 'none'; } catch (e) { return false; }
    }

    function closestOf(el, sel) {
        if (!el || !el.closest) { return null; }
        try { return el.closest(sel); } catch (e) { return null; }
    }

    function ruleMatches(el, rule) {
        var hit = closestOf(el, rule.closest);
        if (!hit) { return false; }
        if (rule.text && fold(hit.innerText).indexOf(fold(rule.text)) < 0) { return false; }
        if (rule.textStart && fold(hit.innerText).indexOf(fold(rule.textStart)) !== 0) { return false; }
        return true;
    }

    function anyRuleMatches(el, rules) {
        if (!rules) { return false; }
        for (var i = 0; i < rules.length; i++) {
            if (ruleMatches(el, rules[i])) { return true; }
        }
        return false;
    }

    function timeText(iso) {
        if (!iso) { return ''; }
        var d = new Date(iso);
        if (isNaN(d.getTime())) { return ''; }
        function two(n) { return n < 10 ? '0' + n : '' + n; }
        return two(d.getHours()) + ':' + two(d.getMinutes()) + ':' + two(d.getSeconds());
    }

    // ── Reading a table the way the robot does (slice 0076) ──────────────────
    // A COPY of Services/JavaScripts/ScrapeTableExtract.js (the function ScrapeTable runs),
    // so a row read here has exactly the keys the same row has in a downloaded package:
    // multi-row headers joined with "!", diacritics dropped, anything else made "_", a
    // cell's input value preferred to its text. Change one, change the other.
    function scrapeTable(table) {
        var data = [];
        if (!table) { return data; }
        var headerRows = table.querySelectorAll('thead tr');
        var matrix = [];
        var r, c;
        for (r = 0; r < headerRows.length; r++) { matrix.push([]); }
        for (r = 0; r < headerRows.length; r++) {
            var cells = headerRows[r].querySelectorAll('th, td');
            var colIndex = 0;
            for (var k = 0; k < cells.length; k++) {
                var cell = cells[k];
                while (typeof matrix[r][colIndex] !== 'undefined') { colIndex++; }
                var text = (cell.innerText || '').replace(/[\r\n]+/g, ' ').trim();
                var rowspan = parseInt(cell.getAttribute('rowspan') || 1, 10);
                var colspan = parseInt(cell.getAttribute('colspan') || 1, 10);
                for (var rr = 0; rr < rowspan; rr++) {
                    for (var cc = 0; cc < colspan; cc++) {
                        if (!matrix[r + rr]) { matrix[r + rr] = []; }
                        matrix[r + rr][colIndex + cc] = text;
                    }
                }
                colIndex += colspan;
            }
        }
        var headers = [];
        var numCols = 0;
        for (r = 0; r < matrix.length; r++) { if (matrix[r].length > numCols) { numCols = matrix[r].length; } }
        for (c = 0; c < numCols; c++) {
            var parts = [];
            for (r = 0; r < matrix.length; r++) {
                var val = matrix[r] ? matrix[r][c] : null;
                if (val && val.length > 0 && parts.indexOf(val) < 0) { parts.push(val); }
            }
            headers.push(parts.join('!') || ('Col_' + (c + 1)));
        }
        var rows = table.querySelectorAll('tbody tr');
        for (r = 0; r < rows.length; r++) {
            var rowData = {};
            var tds = rows[r].querySelectorAll('td');
            for (c = 0; c < tds.length; c++) {
                var key = headers[c] || ('Col_' + (c + 1));
                try { key = key.normalize('NFD').replace(/[̀-ͯ]/g, ''); } catch (ignored) { }
                key = key.replace(/[^a-zA-Z0-9_!]+/g, '_').replace(/^_+|_+$/g, '');
                var inputEl = tds[c].querySelector('input:not([type=checkbox]):not([type=radio]), select, textarea');
                rowData[key] = ((inputEl ? inputEl.value : tds[c].innerText) || '').replace(/[\r\n]+/g, ' ').trim();
            }
            data.push(rowData);
        }
        return data;
    }

    // The indicator code of a tab0 row («Indicator ang.» - the server's Indicator_ang).
    function indicatorOf(row) {
        if (!row) { return ''; }
        if (row.Indicator_ang) { return norm(row.Indicator_ang); }
        for (var k in row) {
            if (Object.prototype.hasOwnProperty.call(row, k) && k.indexOf('Indicator_ang') === 0) {
                return norm(row[k]);
            }
        }
        return '';
    }

    function indicatorCodes(rows) {
        var out = [];
        for (var i = 0; i < rows.length; i++) {
            var cod = indicatorOf(rows[i]);
            if (cod) { out.push(cod); }
        }
        return out;
    }

    // The table ROW an element sits in, with that row read like the robot reads it.
    function rowReadOf(el) {
        var tr = closestOf(el, 'tr');
        var table = closestOf(el, 'table');
        if (!tr || !table) { return null; }
        var trs = table.querySelectorAll('tbody tr');
        var idx = Array.prototype.indexOf.call(trs, tr);
        if (idx < 0) { return null; }
        return scrapeTable(table)[idx] || null;
    }

    // Any d/m/yyyy spelling (/ . -) -> dd/MM/yyyy, the way the receptions list writes «Data»
    // (WorkflowCatalog.DataReceptieFormat). '' when it is not a date.
    function normDate(s) {
        var m = /(\d{1,2})\s*[\/.\-]\s*(\d{1,2})\s*[\/.\-]\s*(\d{4})/.exec(s || '');
        if (!m) { return ''; }
        function two(x) { return x.length < 2 ? '0' + x : x; }
        return two(m[1]) + '/' + two(m[2]) + '/' + m[3];
    }

    // ── Reporting to .NET ────────────────────────────────────────────────────
    function emit(event, extra) {
        if (typeof window._kbotWatchCallback !== 'function') { return; }
        var payload = {
            event: event,
            op: state.op || (extra && extra.op) || '',
            label: state.label || (extra && extra.label) || '',
            cod: readCod(),
            codAtStart: state.codAtStart || '',
            startedAt: state.startedAt || null,
            finishedAt: (extra && extra.finishedAt) || null,
            url: window.location.href,
            message: (extra && extra.message) || '',
            // Slice 0076: what the save was about (section 12) and the marker requests
            // (section 13). Plain objects; .NET reads them as JSON.
            data: (extra && extra.data) || null,
            tip: (extra && extra.tip) || '',
            requestId: (extra && extra.requestId) || ''
        };
        try { window._kbotWatchCallback(JSON.stringify(payload)); } catch (ignored) { }
    }

    // ── Which angajament is on the page (slice 0074) ─────────────────────────
    // Reported when the header code CHANGES (including to nothing: the operator left the
    // angajament), or on demand. Not gated by "suspended": the .NET side drops what it
    // does not want while a job runs, and asks again when the job is over.
    function reportPage(force) {
        var cod = readCod();
        if (!force && cod === lastCodReported) { return; }
        lastCodReported = cod;
        emit('page', { message: cod ? 'angajament în pagină' : 'niciun angajament în pagină' });
    }

    // ── Operation life cycle ─────────────────────────────────────────────────
    // el = the element whose click started it (Nothing for the manual Start).
    function startOperation(opName, label, el) {
        var fresh = emptyState();
        fresh.op = opName;
        fresh.label = label;
        fresh.startedAt = new Date().toISOString();
        fresh.codAtStart = readCod();
        state = fresh;
        // Slice 0076 (section 12): what the start click says about the save to come.
        try {
            if (opName === 'receptie-modificare' && el) {
                var rowR = rowReadOf(el);
                state.rowDate = normDate(rowR ? rowR.Data : '');
            } else if (opName === 'rezervare') {
                state.rezBefore = indicatorCodes(scrapeTable(document.querySelector(SEL_LISTA)));
                // The eye of an existing row names the indicator; «Adauga» does not - the new
                // code is the one that was not in the table before (read at the finish).
                var rowZ = el && closestOf(el, '.glyphicon-eye-open, a:has(.glyphicon-eye-open)') ? rowReadOf(el) : null;
                state.rezIndicator = indicatorOf(rowZ);
            }
        } catch (ignored) { }
        saveState();
        emit('started');
        renderStatus();
    }

    // el = the save button that was pressed (Nothing when .NET or the menu ends it).
    function armFinish(el) {
        if (!state.op) { return; }
        state.pendingSince = new Date().toISOString();
        // Slice 0076 (section 12): what only the form still on screen can tell.
        try {
            if (state.op === 'receptie' || state.op === 'receptie-modificare') {
                var d = document.querySelector(SEL_DATA_RECEPTIE);
                if (d) { state.formDate = normDate(d.value); }
            } else if (state.op === 'rezervare') {
                var buget = document.querySelector(SEL_BUGET);
                if (buget) { state.rezBuget = scrapeTable(buget); }
            }
        } catch (ignored) { }
        saveState();
        emit('info', { message: 'salvare apăsată' });
        renderStatus();
        schedulePendingCheck();
    }

    // Section 12: the "data" of a finished operation, read now that the page is settled.
    function finishedData() {
        if (state.op === 'receptie' || state.op === 'receptie-modificare') {
            return { dataReceptie: state.formDate || state.rowDate || '', rowDate: state.rowDate || '',
                     formDate: state.formDate || '' };
        }
        if (state.op === 'rezervare') {
            var rows = scrapeTable(document.querySelector(SEL_LISTA));
            var cod = state.rezIndicator || '';
            var found = null;
            var i;
            if (cod) {
                for (i = 0; i < rows.length; i++) { if (indicatorOf(rows[i]) === cod) { found = rows[i]; break; } }
            } else if (state.rezBefore) {
                // The new indicator: the row whose code was not in the table at the start.
                for (i = 0; i < rows.length; i++) {
                    var c = indicatorOf(rows[i]);
                    if (c && state.rezBefore.indexOf(c) < 0) { found = rows[i]; break; }
                }
            }
            return { indicator: found, indicatorCod: found ? indicatorOf(found) : cod,
                     buget: state.rezBuget || [] };
        }
        return null;
    }

    function finishOperation(message) {
        if (!state.op) { return; }
        stopPendingCheck();
        var data = null;
        try { data = finishedData(); } catch (ignored) { }
        emit('finished', { finishedAt: new Date().toISOString(), message: message || '', data: data });
        resetState();
        renderStatus();
    }

    function cancelOperation(message) {
        if (!state.op) { return; }
        stopPendingCheck();
        emit('cancelled', { message: message || '' });
        resetState();
        renderStatus();
    }

    // The save was pressed; wait for the page to settle. Errors put the operation back
    // to "in progress" (the operator will correct and save again); a modal keeps waiting
    // (the motive dialog of a changed reservation); the op's own "done" test ends it.
    function schedulePendingCheck() {
        stopPendingCheck();
        pendingTimer = setInterval(checkPending, PENDING_POLL_MS);
    }

    function stopPendingCheck() {
        if (pendingTimer) { clearInterval(pendingTimer); pendingTimer = null; }
    }

    function checkPending() {
        if (!state.op || !state.pendingSince) { stopPendingCheck(); return; }

        var started = new Date(state.pendingSince).getTime();
        if (!isNaN(started) && Date.now() - started > PENDING_TIMEOUT_MS) {
            state.pendingSince = null;
            saveState();
            emit('info', { message: 'salvarea nu s-a confirmat în 30 s; operațiunea rămâne în curs' });
            renderStatus();
            stopPendingCheck();
            return;
        }

        if (anyVisible(SEL_ERROR)) {
            state.pendingSince = null;
            saveState();
            emit('info', { message: 'FOREXE a respins salvarea; operațiunea rămâne în curs' });
            renderStatus();
            stopPendingCheck();
            return;
        }

        if (wicketBusy()) { return; }
        if (anyVisible(SEL_MODAL)) { return; }

        var rules = OPS[state.op];
        var done = rules && rules.done ? rules.done() : !!readCod();
        if (done) { finishOperation('salvare confirmată'); }
    }

    // ── Form guard: a save that changes nothing is stopped ───────────────────
    // "fields" are the value inputs the snapshot follows; "save" is the form's save button
    // (the same rules the watcher uses for the finish). Names, never ids.
    var GUARDS = {
        'rezervare': {
            label: 'rezervare',
            fields: 'input[name^=\'tableContainer:\']',
            save: { closest: 'button.btn-success.btn-small' }
        },
        // Tip / Data / Descriere count as much as the values (operator, 25.09.2026): a
        // reception whose only change is its type, date or description is a real save.
        'receptie': {
            label: 'recepție',
            fields: 'form.form-horizontal input[name$=\':valoare\'], ' + SEL_FORM_RECEPTIE + ', ' +
                SEL_DATA_RECEPTIE + ', ' + SEL_DESCRIERE_RECEPTIE,
            save: { closest: 'form.form-horizontal button', text: 'Salveaza' }
        }
    };
    var guardSnapshots = {};      // kind -> {name: value} read when the form appeared
    var guardRefreshTimer = null;
    var guardBox = null;

    function readFields(sel) {
        var list;
        try { list = document.querySelectorAll(sel); } catch (e) { return null; }
        if (!list.length) { return null; }
        var out = {};
        for (var i = 0; i < list.length; i++) {
            out[list[i].name || ('#' + i)] = norm(list[i].value);
        }
        return out;
    }

    function sameFields(a, b) {
        if (!a || !b) { return false; }
        var k;
        for (k in a) { if (Object.prototype.hasOwnProperty.call(a, k) && a[k] !== b[k]) { return false; } }
        for (k in b) { if (Object.prototype.hasOwnProperty.call(b, k) && !Object.prototype.hasOwnProperty.call(a, k)) { return false; } }
        return true;
    }

    // A form that has just appeared is snapshotted; one that went away forgets its
    // snapshot. A form still on screen keeps the first snapshot, whatever Wicket redraws;
    // a field that shows up after the snapshot (rendered a beat later) joins it with the
    // first value it is seen with, so a late field can never make every save look changed.
    function refreshGuards() {
        for (var kind in GUARDS) {
            if (!Object.prototype.hasOwnProperty.call(GUARDS, kind)) { continue; }
            var now = readFields(GUARDS[kind].fields);
            if (!now) { delete guardSnapshots[kind]; continue; }
            var snap = guardSnapshots[kind];
            if (!snap) { guardSnapshots[kind] = now; continue; }
            for (var k in now) {
                if (Object.prototype.hasOwnProperty.call(now, k) &&
                    !Object.prototype.hasOwnProperty.call(snap, k)) { snap[k] = now[k]; }
            }
        }
    }

    function scheduleGuardRefresh() {
        if (guardRefreshTimer) { return; }
        guardRefreshTimer = setTimeout(function () { guardRefreshTimer = null; refreshGuards(); }, 50);
    }

    // The guarded form the element belongs to, if its save would change nothing.
    function unchangedGuard(kind) {
        var g = GUARDS[kind];
        if (!g || !guardSnapshots[kind]) { return false; }
        var now = readFields(g.fields);
        return !!now && sameFields(guardSnapshots[kind], now);
    }

    function blockSave(e, kind) {
        e.preventDefault();
        e.stopImmediatePropagation();
        showBlockingMessage('Nu ați schimbat nicio valoare a acestei ' + GUARDS[kind].label +
            ' față de momentul deschiderii.', 'NU salvați! Apăsați «Renunță».');
        emit('info', { message: 'salvare fără modificări oprită (' + GUARDS[kind].label + ')' });
    }

    function onGuardClick(e) {
        if (suspended) { return; }
        var el = e.target;
        if (!el || !el.tagName) { return; }
        if (menu && menu.contains(el)) { return; }
        // A modal's own buttons (the «Renuntare» question: Nu / Da) are never a save.
        if (closestOf(el, '.modal-dialog')) { return; }
        for (var kind in GUARDS) {
            if (!Object.prototype.hasOwnProperty.call(GUARDS, kind)) { continue; }
            if (!guardSnapshots[kind]) { continue; }
            if (!ruleMatches(el, GUARDS[kind].save)) { continue; }
            if (unchangedGuard(kind)) { blockSave(e, kind); }
            return;
        }
    }

    // Enter in a field submits the form without any click - same check.
    function onGuardKey(e) {
        if (suspended || e.key !== 'Enter') { return; }
        var el = e.target;
        if (!el || el.tagName !== 'INPUT') { return; }
        var form = closestOf(el, 'form');
        if (!form) { return; }
        for (var kind in GUARDS) {
            if (!Object.prototype.hasOwnProperty.call(GUARDS, kind)) { continue; }
            if (!guardSnapshots[kind]) { continue; }
            var inside = false;
            try { inside = !!form.querySelector(GUARDS[kind].fields); } catch (ignored) { }
            if (!inside) { continue; }
            if (unchangedGuard(kind)) { blockSave(e, kind); }
            return;
        }
    }

    // A message the operator must close before touching the page again.
    function showBlockingMessage(line1, line2) {
        hideBlockingMessage();
        if (!document.body) { return; }
        guardBox = document.createElement('div');
        guardBox.id = 'kbot-watch-block';
        guardBox.style.cssText = 'position:fixed;left:0;top:0;right:0;bottom:0;z-index:2147483100;' +
            'background:rgba(0,0,0,.55);display:flex;align-items:center;justify-content:center;';
        var box = document.createElement('div');
        box.style.cssText = 'background:#2d2d30;color:#f0f0f0;border:2px solid #d9534f;border-radius:8px;' +
            'box-shadow:0 8px 32px rgba(0,0,0,.6);padding:18px 24px;max-width:520px;' +
            'font:14px Segoe UI,Arial,sans-serif;text-align:center;';
        var t = document.createElement('div');
        t.textContent = 'K-BOT';
        t.style.cssText = 'font-weight:bold;color:#d9534f;font-size:16px;margin-bottom:10px;';
        var l1 = document.createElement('div');
        l1.textContent = line1;
        l1.style.cssText = 'margin-bottom:8px;';
        var l2 = document.createElement('div');
        l2.textContent = line2;
        l2.style.cssText = 'font-weight:bold;font-size:16px;margin-bottom:14px;';
        var ok = mkButton('Am înțeles', 'Închide mesajul', hideBlockingMessage);
        ok.style.padding = '6px 18px';
        box.appendChild(t); box.appendChild(l1); box.appendChild(l2); box.appendChild(ok);
        guardBox.appendChild(box);
        // Captured at the box, so nothing under it hears the click - which also means the
        // button's own listener never runs (the event stops here, before its target): the
        // button is answered HERE. Found on screen (operator, 21.09.2026).
        guardBox.addEventListener('click', function (ev) {
            ev.stopPropagation();
            if (ok === ev.target || ok.contains(ev.target)) { hideBlockingMessage(); }
        }, true);
        document.body.appendChild(guardBox);
        try { ok.focus(); } catch (ignored) { }
    }

    function hideBlockingMessage() {
        if (guardBox && guardBox.parentNode) { guardBox.parentNode.removeChild(guardBox); }
        guardBox = null;
    }

    function onBlockKey(e) {
        if (!guardBox) { return; }
        if (e.key === 'Escape' || e.key === 'Enter') { hideBlockingMessage(); }
        e.preventDefault();
        e.stopImmediatePropagation();
    }

    // ── The operator's page choices: styles + developer tools ────────────────
    function loadConfig() {
        try {
            var raw = localStorage.getItem(KEY_CONFIG);
            if (raw) {
                var c = JSON.parse(raw);
                if (c && typeof c === 'object') {
                    return { devTools: !!c.devTools, darkMode: !!c.darkMode, veil: veilColors(c.veil), rules: Array.isArray(c.rules) ? c.rules : [] };
                }
            }
        } catch (ignored) { }
        return { devTools: false, darkMode: false, veil: veilColors(null), rules: [] };
    }

    // From .NET: the JSON text of ForexeWatchConfig. Applied now, kept for the next load.
    function configure(json) {
        var c = null;
        try { c = typeof json === 'string' ? JSON.parse(json) : json; } catch (ignored) { }
        if (!c || typeof c !== 'object') { return; }
        config = { devTools: !!c.devTools, darkMode: !!c.darkMode, veil: veilColors(c.veil), rules: Array.isArray(c.rules) ? c.rules : [] };
        try { localStorage.setItem(KEY_CONFIG, JSON.stringify(config)); } catch (ignored) { }
        installStyles();
        installBusyStyle();
    }

    // A rule's "page" (operator, 21.09.2026): empty = every page; otherwise the rule is in
    // the sheet only while THIS document's address is that page, query string and hash
    // left out on both sides. A full address («https://host/CABWeb/contract») is matched
    // against origin + path, a value starting with «/» against the path, anything else
    // («contract») against the last segment of the path. Case does not matter.
    function pageMatches(page) {
        var want = norm(page).replace(/[?#].*$/, '').replace(/\/+$/, '').toLowerCase();
        if (!want) { return true; }
        var path = '', origin = '';
        try { path = (window.location.pathname || '').replace(/\/+$/, '').toLowerCase(); } catch (ignored) { }
        try { origin = (window.location.origin || '').toLowerCase(); } catch (ignored) { }
        if (want.indexOf('://') >= 0) { return want === origin + path; }
        if (want.charAt(0) === '/') { return want === path; }
        return want === path.slice(path.lastIndexOf('/') + 1);
    }

    // One stylesheet from the enabled rules of THIS page; every declaration made !important.
    // Recomputed at every resync (section 11), so a rule bound to a page comes and goes
    // with the address.
    function cssFromRules(rules) {
        var out = '';
        for (var i = 0; i < rules.length; i++) {
            var r = rules[i];
            if (!r || r.enabled === false || !r.selector) { continue; }
            if (!pageMatches(r.page)) { continue; }
            var decls = String(r.css || '').split(';');
            var body = '';
            for (var j = 0; j < decls.length; j++) {
                var k = decls[j].indexOf(':');
                if (k < 0) { continue; }
                var p = decls[j].slice(0, k).trim();
                var v = decls[j].slice(k + 1).trim();
                if (!p || !v) { continue; }
                if (!/!\s*important\s*$/i.test(v)) { v += ' !important'; }
                body += p + ':' + v + ';';
            }
            if (body) { out += String(r.selector).trim() + '{' + body + '}\n'; }
        }
        return out;
    }

    // Section 10. The root element is the only place a filter leaves fixed elements alone;
    // body gets a light grey so the inverted page is a dark grey, not pitch black.
    var DARK_CSS =
        'html{filter:invert(1) hue-rotate(180deg) !important;background:#fff !important;}' +
        'body{background-color:#e8e8e8 !important;}' +
        'img,video,canvas,iframe,svg,[style*="background-image"]{filter:invert(1) hue-rotate(180deg) !important;}' +
        '#kbot-watch-menu,#kbot-watch-veil,#kbot-watch-block{filter:invert(1) hue-rotate(180deg) !important;}\n';

    // Where a <style> of ours can hang: <head> when it exists, else <html> (a sheet works
    // just the same there). Null before the parser has produced <html> (section 11).
    function sheetParent() {
        return document.head || document.documentElement || null;
    }

    // Runs at script start (document_start for a navigation) and again from every
    // watcher of section 11. True when the sheet is in the document afterwards.
    function installStyles() {
        try {
            if (!styleEl || !styleEl.parentNode) {
                var parent = sheetParent();
                if (!parent) { return false; }
                styleEl = document.getElementById('kbot-watch-style');
                if (!styleEl) {
                    styleEl = document.createElement('style');
                    styleEl.id = 'kbot-watch-style';
                    parent.appendChild(styleEl);
                }
            }
            var css = (config.darkMode ? DARK_CSS : '') + cssFromRules(config.rules);
            if (styleEl.textContent !== css) { styleEl.textContent = css; }
            return true;
        } catch (ignored) { return false; }
    }

    // For the robot (WorkflowExecutor's Click action): true when EL is hidden only because
    // of the operator's rules - it becomes visible the moment the sheet is switched off.
    function hiddenByStyles(el) {
        if (!el || !styleEl || styleEl.disabled) { return false; }
        var before = isVisible(el);
        if (before) { return false; }
        var after = false;
        try {
            styleEl.disabled = true;
            after = isVisible(el);
        } finally {
            styleEl.disabled = false;
        }
        return after;
    }

    // Switches the operator's rules off (true) or on (false) - for the one robot click
    // whose target they hide. Never left off: a navigation replaces the document anyway.
    function liftStyles(flag) {
        if (styleEl) { styleEl.disabled = !!flag; }
    }

    // ── The veil (section 9) ─────────────────────────────────────────────────
    // The colours come from .NET (ForexeWatchConfig.VeilColorsNow, the theme palette). Each
    // one must be a plain #rrggbb - anything else falls back to the default, so nothing but
    // a colour ever reaches the sheet. The defaults live inside the function because
    // loadConfig() calls it at the top of the script, before any var down here is set.
    function veilColors(v) {
        var defaults = { back: '#f0f0f0', card: '#2d2d30', text: '#ffffff', dim: '#d0d0d0', accent: '#ffffff', border: '#2d2d30' };
        var out = {};
        for (var k in defaults) {
            if (!defaults.hasOwnProperty(k)) { continue; }
            var c = (v && typeof v === 'object') ? String(v[k] || '') : '';
            out[k] = /^#[0-9a-fA-F]{6}$/.test(c) ? c : defaults[k];
        }
        return out;
    }

    // An opaque backdrop: nothing of the page is painted through it. The page's own
    // elements are NOT hidden (visibility/display would make the robot's targets "not
    // visible" to Playwright); the veil only sits on top of them.
    function busyCss(v) {
        return '#kbot-watch-veil{position:fixed;left:0;top:0;right:0;bottom:0;z-index:2147483000;' +
            'display:none;align-items:center;justify-content:center;pointer-events:none;' +
            'background:' + v.back + ' !important;font-family:Segoe UI,Arial,sans-serif;}' +
            'html.kbot-busy #kbot-watch-veil{display:flex;}' +
            '#kbot-watch-veil .kbot-veil-card{display:flex;align-items:center;gap:18px;' +
            'background:' + v.card + ';color:' + v.text + ';border:1px solid ' + v.border + ';' +
            'padding:22px 34px;border-radius:12px;' +
            'box-shadow:0 8px 28px rgba(0,0,0,0.18);max-width:70vw;}' +
            '#kbot-watch-veil .kbot-veil-spin{width:30px;height:30px;flex:0 0 30px;border-radius:50%;' +
            'border:4px solid ' + v.border + ';border-top-color:' + v.accent + ';' +
            'animation:kbot-veil-spin 0.9s linear infinite;}' +
            '#kbot-watch-veil .kbot-veil-title{font-size:18px;font-weight:600;line-height:1.3;}' +
            '#kbot-watch-veil .kbot-veil-sub{font-size:14px;color:' + v.dim + ';margin-top:4px;}' +
            '@keyframes kbot-veil-spin{to{transform:rotate(360deg);}}';
    }

    // At script start, like the rules: a document loaded mid-job is covered before it paints.
    // Rewritten when the colours change (configure). True when the sheet is in the document.
    function installBusyStyle() {
        try {
            var css = busyCss(config.veil || veilColors(null));
            if (!busyStyleEl || !busyStyleEl.parentNode) {
                var parent = sheetParent();
                if (!parent) { return false; }
                busyStyleEl = document.getElementById('kbot-watch-busy-style');
                if (!busyStyleEl) {
                    busyStyleEl = document.createElement('style');
                    busyStyleEl.id = 'kbot-watch-busy-style';
                    parent.appendChild(busyStyleEl);
                }
            }
            if (busyStyleEl.textContent !== css) { busyStyleEl.textContent = css; }
            return true;
        } catch (ignored) { return false; }
    }

    function buildVeil() {
        if (!document.body) { return; }
        veil = document.getElementById('kbot-watch-veil');
        if (veil) { veilText = veil.querySelector('.kbot-veil-title'); return; }
        veil = document.createElement('div');
        veil.id = 'kbot-watch-veil';
        var card = document.createElement('div');
        card.className = 'kbot-veil-card';
        var spin = document.createElement('div');
        spin.className = 'kbot-veil-spin';
        var lines = document.createElement('div');
        veilText = document.createElement('div');
        veilText.className = 'kbot-veil-title';
        var sub = document.createElement('div');
        sub.className = 'kbot-veil-sub';
        sub.textContent = 'Vă rugăm așteptați: pagina se deblochează singură la final.';
        lines.appendChild(veilText);
        lines.appendChild(sub);
        card.appendChild(spin);
        card.appendChild(lines);
        veil.appendChild(card);
        document.body.appendChild(veil);
        syncBusy();
    }

    function readBusyText() {
        try { return sessionStorage.getItem(KEY_SUSPEND_MSG) || DEFAULT_BUSY_TEXT; } catch (e) { return DEFAULT_BUSY_TEXT; }
    }

    // The <html> class drives the veil's display (see busyCss). Touches
    // the class only when it is wrong: the root observer of section 11 calls this too, and
    // a rewrite of an already-right class would wake it again for nothing.
    function syncBusy() {
        try {
            var root = document.documentElement;
            if (!root) { return; }
            var has = root.classList.contains('kbot-busy');
            if (suspended && !has) { root.classList.add('kbot-busy'); }
            else if (!suspended && has) { root.classList.remove('kbot-busy'); }
            if (veilText) { veilText.textContent = readBusyText(); }
        } catch (ignored) { }
    }

    // ── Section 11: the sheets and the busy class, kept in place at once ─────
    // Everything a re-render or a late-built document could have dropped, put back in one
    // go: the two sheets, the busy class, the veil, the menu's hidden state. Cheap enough
    // to run on every Wicket event and every mutation of <html> / <head>.
    var resyncTimer = null;
    var wicketHooked = false;
    var sheetsLateOnce = false;

    function resync() {
        resyncTimer = null;
        installStyles();
        installBusyStyle();
        syncBusy();
        if (document.body) {
            if (!veil || !document.body.contains(veil)) { veil = null; buildVeil(); }
            if (menu && document.body.contains(menu)) { menu.style.display = suspended ? 'none' : ''; }
        }
        hookWicket();
    }

    // Many mutations arrive in one burst (a Wicket re-render replaces several pieces);
    // one resync per burst is enough.
    function scheduleResync() {
        if (resyncTimer) { return; }
        resyncTimer = setTimeout(resync, 0);
    }

    // Wicket 6+ publishes its Ajax life cycle on Wicket.Event; older versions take a post
    // call handler. Either way the page hears about a re-render the moment it is done.
    // Retried from the beat until Wicket's script is on the page.
    function hookWicket() {
        if (wicketHooked) { return; }
        try {
            var W = window.Wicket;
            if (!W) { return; }
            if (W.Event && typeof W.Event.subscribe === 'function') {
                W.Event.subscribe('/ajax/call/complete', scheduleResync);
                W.Event.subscribe('/dom/node/added', scheduleResync);
                wicketHooked = true;
            } else if (W.Ajax && typeof W.Ajax.registerPostCallHandler === 'function') {
                W.Ajax.registerPostCallHandler(scheduleResync);
                wicketHooked = true;
            }
        } catch (ignored) { }
    }

    // The root element's class and <head>'s children are watched directly: a sheet taken
    // out or the busy class dropped comes back in the same tick, not at the next beat.
    function watchRootAndHead() {
        try {
            if (document.documentElement) {
                new MutationObserver(scheduleResync).observe(document.documentElement,
                    { childList: true, attributes: true, attributeFilter: ['class'] });
            }
            if (document.head) {
                new MutationObserver(scheduleResync).observe(document.head, { childList: true });
            }
        } catch (ignored) { }
    }

    // Runs FN once <html> exists: now when it already does, else the moment the parser
    // inserts it (well before the first paint). DOMContentLoaded is the fallback when the
    // observer cannot be made.
    function whenRootExists(fn) {
        if (document.documentElement) { fn(); return; }
        try {
            var mo = new MutationObserver(function () {
                if (!document.documentElement) { return; }
                mo.disconnect();
                fn();
            });
            mo.observe(document, { childList: true });
        } catch (ignored) {
            document.addEventListener('DOMContentLoaded', fn);
        }
    }

    // The first install: sheets + busy class before anything is painted.
    function earlyInstall() {
        installStyles();
        installBusyStyle();
        syncBusy();
    }

    // The page's element outline for the settings window's rule editor (its tree of the
    // page). Static ids only (never Wicket's idXX), else the name, else up to three
    // classes. The elements themselves are kept, in the same order, so highlightElement(i)
    // can point at the i-th one afterwards.
    var listed = [];
    var MAX_LISTED = 2500;
    function listElements() {
        var out = [];
        listed = [];
        function ownText(el) {
            var t = '';
            for (var i = 0; i < el.childNodes.length; i++) {
                if (el.childNodes[i].nodeType === 3) { t += el.childNodes[i].nodeValue; }
            }
            t = norm(t);
            if (!t) { try { t = norm(el.innerText); } catch (ignored) { } }
            return t.slice(0, 60);
        }
        function walk(el, depth) {
            if (out.length >= MAX_LISTED || !el || el.nodeType !== 1) { return; }
            var tag = el.tagName.toLowerCase();
            if (tag === 'script' || tag === 'style' || tag === 'link' || tag === 'noscript') { return; }
            if (el.id === 'kbot-watch-menu' || el.id === 'kbot-watch-block' || el.id === 'kbot-watch-style' ||
                el.id === 'kbot-watch-veil' || el.id === 'kbot-watch-hl') { return; }
            var id = el.id && !/^id[0-9a-f]+$/i.test(el.id) ? el.id : '';
            var classes = [];
            try { classes = Array.prototype.slice.call(el.classList, 0); } catch (ignored) { }
            var name = el.getAttribute('name') || '';
            var sel = tag;
            if (id) { sel += '#' + id; }
            else if (name) { sel += '[name=\'' + name + '\']'; }
            else if (classes.length) { sel += '.' + classes.slice(0, 3).join('.'); }
            out.push({
                index: out.length, depth: depth, tag: tag, id: id, name: name, classes: classes.join(' '),
                selector: sel, text: ownText(el), style: el.getAttribute('style') || ''
            });
            listed.push(el);
            for (var i = 0; i < el.children.length; i++) { walk(el.children[i], depth + 1); }
        }
        if (document.body) { walk(document.body, 0); }
        return JSON.stringify(out);
    }

    // A frame drawn over the i-th listed element (the settings window's tree selection),
    // scrolled into view; a negative index, or an element no longer in the page, clears it.
    // Says what it did: 'shown', 'gone' (the page changed since the listing), 'cleared'.
    var hl = null;
    function highlightElement(i) {
        try {
            if (hl && hl.parentNode) { hl.parentNode.removeChild(hl); }
            hl = null;
            var el = (i >= 0 && i < listed.length) ? listed[i] : null;
            if (!el) { return i < 0 ? 'cleared' : 'gone'; }
            if (!document.body || !document.body.contains(el)) { return 'gone'; }
            try { el.scrollIntoView({ block: 'center', inline: 'nearest' }); } catch (ignored) { }
            var r = el.getBoundingClientRect();
            hl = document.createElement('div');
            hl.id = 'kbot-watch-hl';
            hl.style.cssText = 'position:fixed;z-index:2147482900;pointer-events:none;box-sizing:border-box;' +
                'border:3px solid #ff7f0e;background:rgba(255,127,14,0.18);border-radius:3px;' +
                'box-shadow:0 0 0 3px rgba(255,255,255,0.7), 0 0 14px rgba(255,127,14,0.9);' +
                'left:' + (r.left - 3) + 'px;top:' + (r.top - 3) + 'px;' +
                'width:' + Math.max(8, r.width + 6) + 'px;height:' + Math.max(8, r.height + 6) + 'px;';
            document.body.appendChild(hl);
            return 'shown';
        } catch (e) {
            return 'gone';
        }
    }

    // ── Developer tools stay shut (unless the settings allow them) ───────────
    function isDevToolsKey(e) {
        var k = (e.key || '').toUpperCase();
        if (k === 'F12') { return true; }
        if (e.ctrlKey && e.shiftKey && (k === 'I' || k === 'J' || k === 'C')) { return true; }
        if (e.ctrlKey && !e.shiftKey && k === 'U') { return true; }
        return false;
    }

    function onKeyDown(e) {
        if (config.devTools || !isDevToolsKey(e)) { return; }
        e.preventDefault();
        e.stopPropagation();
    }

    function onContextMenu(e) {
        if (config.devTools) { return; }
        e.preventDefault();
        e.stopPropagation();
    }

    // ── Click watching (capture phase, so Wicket's own handlers cannot hide it) ──
    function onClick(e) {
        if (suspended) { return; }
        var el = e.target;
        if (!el || !el.tagName) { return; }
        if (menu && menu.contains(el)) { return; }

        if (state.op) {
            // «Renunta» anywhere - the form, a modal - drops the operation: no finish,
            // nothing downloaded. Checked before the finish rules on purpose.
            if (ruleMatches(el, RULE_CANCEL)) { cancelOperation('Renunță apăsat în FOREXE'); return; }
            var current = OPS[state.op];
            if (current && anyRuleMatches(el, current.finish)) { armFinish(el); }
            return;
        }

        for (var name in OPS) {
            if (!Object.prototype.hasOwnProperty.call(OPS, name)) { continue; }
            var rules = OPS[name];
            if (rules.when && !exists(rules.when)) { continue; }
            if (anyRuleMatches(el, rules.start)) {
                startOperation(name, rules.label, el);
                return;
            }
        }
    }

    // ── A form left without a save (slice 0076, section 12) ──────────────────
    // Called on the beat. Once the operation's form has been on screen, its absence with no
    // save pending means the operator went away (the page's «Inapoi», another tab): the
    // operation ends, as with «Renunta», instead of blocking every later start.
    function checkAbandoned() {
        if (suspended || !state.op || state.pendingSince) { return; }
        var sel = OP_FORMS[state.op];
        if (!sel) { return; }
        if (exists(sel)) {
            if (!state.formSeen) { state.formSeen = true; saveState(); }
            return;
        }
        if (state.formSeen && !wicketBusy()) { cancelOperation('formularul s-a închis fără salvare'); }
    }

    // ── The markers (slice 0076, section 13) ─────────────────────────────────
    var marcajPending = {};    // requestId -> {resolve, timer}
    var marcajSeq = 0;
    var marcajBypass = false;  // true while a held click is replayed
    var marcajBusy = false;    // a click is being held right now

    // Asks .NET for the marker; resolves with its text, or '' (no answer, an error, a timeout).
    function requestMarcaj(tip) {
        return new Promise(function (resolve) {
            if (typeof window._kbotWatchCallback !== 'function') { resolve(''); return; }
            marcajSeq++;
            var id = 'm' + Date.now() + '_' + marcajSeq;
            var timer = setTimeout(function () {
                if (!marcajPending[id]) { return; }
                delete marcajPending[id];
                emit('info', { message: 'marcajul K-BOT nu a venit în ' + (MARCAJ_TIMEOUT_MS / 1000) + ' s; salvez fără el' });
                resolve('');
            }, MARCAJ_TIMEOUT_MS);
            marcajPending[id] = { resolve: resolve, timer: timer };
            emit('marcaj', { tip: tip, requestId: id });
        });
    }

    // From .NET: the answer to requestMarcaj. text '' = no marker (message says why).
    function setMarcaj(id, text, message) {
        var p = marcajPending[id];
        if (!p) { return false; }
        delete marcajPending[id];
        clearTimeout(p.timer);
        if (!text && message) { emit('info', { message: 'marcajul K-BOT lipsește: ' + message }); }
        p.resolve(text || '');
        return true;
    }

    // The text with every K-BOT marker taken out and the new one appended.
    function withMarcaj(text, marcaj) {
        var clean = (text || '').replace(RE_MARCAJ, '').replace(/\s+$/, '');
        return clean ? clean + ' ' + marcaj : marcaj;
    }

    function setFieldValue(field, value) {
        field.value = value;
        try { field.dispatchEvent(new Event('input', { bubbles: true })); } catch (ignored) { }
        try { field.dispatchEvent(new Event('change', { bubbles: true })); } catch (ignored) { }
    }

    // Which marker, if any, the clicked button's submit must carry: {tip, field} or null.
    function marcajTarget(el) {
        // The reservation: «Continua» of the motive modal.
        if (closestOf(el, 'div.modal-footer button.btn-success')) {
            var motiv = document.querySelector(SEL_MOTIV);
            if (motiv && isVisible(motiv)) { return { tip: 'rezervare', field: motiv }; }
            return null;
        }
        // The reception: «Salveaza» of the reception form, new or edited.
        if (ruleMatches(el, { closest: 'form.form-horizontal button', text: 'Salveaza' })) {
            var descr = document.querySelector(SEL_DESCRIERE_RECEPTIE);
            if (descr && exists(SEL_FORM_RECEPTIE)) { return { tip: 'receptie', field: descr }; }
        }
        return null;
    }

    // Capture phase, after the form guard (a save it stops needs no marker) and before the
    // watcher (so the finish is armed by the REPLAYED click, the one that really submits).
    function onMarcajClick(e) {
        if (suspended || marcajBypass) { return; }
        if (typeof window._kbotWatchCallback !== 'function') { return; }
        var el = e.target;
        if (!el || !el.tagName) { return; }
        if (menu && menu.contains(el)) { return; }
        var target = marcajTarget(el);
        if (!target) { return; }
        var button = closestOf(el, 'button') || el;

        e.preventDefault();
        e.stopImmediatePropagation();
        if (marcajBusy) { return; }   // a second click while the first is held
        marcajBusy = true;
        emit('info', { message: 'cer marcajul K-BOT pentru ' + target.tip });

        requestMarcaj(target.tip).then(function (marcaj) {
            try {
                if (marcaj) { setFieldValue(target.field, withMarcaj(target.field.value, marcaj)); }
            } catch (ignored) { }
            marcajBusy = false;
            marcajBypass = true;
            try { button.click(); } finally { marcajBypass = false; }
        });
    }

    // Enter in a text box of the reception form or of the motive modal would save through the
    // browser's implicit submit, which may never pass through onMarcajClick. Swallowed there:
    // the operator saves with the button, and the button carries the marker. Textareas keep
    // Enter (a new line, never a submit); the buttons themselves keep it too (a key on a focused
    // button is a real click event, onMarcajClick sees it).
    function onMarcajKey(e) {
        if (suspended || e.key !== 'Enter') { return; }
        if (typeof window._kbotWatchCallback !== 'function') { return; }
        var el = e.target;
        if (!el || el.tagName !== 'INPUT') { return; }
        var type = (el.type || '').toLowerCase();
        if (type === 'button' || type === 'submit') { return; }
        var inside = false;
        try {
            var form = closestOf(el, 'form');
            if (form && form.querySelector(SEL_FORM_RECEPTIE)) { inside = true; }
            var modal = closestOf(el, '.modal');
            if (modal && modal.querySelector(SEL_MOTIV)) { inside = true; }
        } catch (ignored) { }
        if (!inside) { return; }
        e.preventDefault();
        e.stopImmediatePropagation();
        emit('info', { message: 'Enter oprit în formular; salvați cu butonul (acolo se pune marcajul K-BOT)' });
    }

    // ── Zoom (CSS zoom on the root; the menu is counter-zoomed so it keeps its size) ──
    function readZoom() {
        try {
            var z = parseFloat(localStorage.getItem(KEY_ZOOM));
            if (!isNaN(z) && z >= ZOOM_MIN && z <= ZOOM_MAX) { return z; }
        } catch (ignored) { }
        return 1;
    }

    function applyZoom(z, silent) {
        z = Math.round(Math.min(ZOOM_MAX, Math.max(ZOOM_MIN, z)) * 100) / 100;
        try { localStorage.setItem(KEY_ZOOM, String(z)); } catch (ignored) { }
        try { document.documentElement.style.zoom = z === 1 ? '' : String(z); } catch (ignored) { }
        if (menu) {
            try { menu.style.zoom = z === 1 ? '' : String(1 / z); } catch (ignored) { }
        }
        if (lblZoom) { lblZoom.textContent = Math.round(z * 100) + '%'; }
        if (!silent) { emit('info', { message: 'zoom ' + Math.round(z * 100) + '%' }); }
    }

    // ── Menu ─────────────────────────────────────────────────────────────────
    function mkButton(text, title, onclick) {
        var b = document.createElement('button');
        b.type = 'button';
        b.textContent = text;
        b.title = title;
        b.style.cssText = 'border:1px solid #5a5a5e;background:#3e3e42;color:#e8e8e8;' +
            'border-radius:4px;padding:2px 8px;margin:0 2px;font:12px Segoe UI,Arial,sans-serif;' +
            'cursor:pointer;line-height:18px;min-width:28px;';
        b.onmouseenter = function () { b.style.background = '#4b4b50'; };
        b.onmouseleave = function () { b.style.background = '#3e3e42'; };
        b.addEventListener('click', function (e) {
            e.preventDefault();
            e.stopPropagation();
            try { onclick(); } catch (ignored) { }
        }, true);
        return b;
    }

    function buildMenu() {
        if (menu && document.body && document.body.contains(menu)) { return; }
        if (!document.body) { return; }

        menu = document.createElement('div');
        menu.id = 'kbot-watch-menu';
        menu.style.cssText = 'position:fixed;right:16px;bottom:16px;z-index:2147483000;' +
            'background:#2d2d30;color:#d2d2d2;border:1px solid #555558;border-radius:8px;' +
            'box-shadow:0 4px 16px rgba(0,0,0,.45);padding:6px 8px;' +
            'font:12px Segoe UI,Arial,sans-serif;user-select:none;min-width:230px;';

        var head = document.createElement('div');
        head.style.cssText = 'display:flex;align-items:center;justify-content:space-between;' +
            'cursor:move;padding-bottom:4px;margin-bottom:4px;border-bottom:1px solid #444;';
        var title = document.createElement('span');
        title.textContent = 'K-BOT';
        title.style.cssText = 'font-weight:bold;color:#7ab8ff;letter-spacing:.5px;';
        lblZoom = document.createElement('span');
        lblZoom.style.cssText = 'color:#9a9a9a;margin-left:8px;';
        btnFold = mkButton('', '', function () { setFolded(!readFolded()); });
        btnFold.style.cssText += 'min-width:22px;padding:0 5px;margin-left:8px;';
        var headLeft = document.createElement('span');
        headLeft.style.cssText = 'display:flex;align-items:center;';
        headLeft.appendChild(title);
        headLeft.appendChild(lblZoom);
        head.appendChild(headLeft);
        head.appendChild(btnFold);
        menu.appendChild(head);

        // Everything under the title row folds away with the fold button.
        menuBody = document.createElement('div');
        menu.appendChild(menuBody);

        var rowZoom = document.createElement('div');
        rowZoom.style.cssText = 'display:flex;align-items:center;margin-bottom:4px;';
        rowZoom.appendChild(mkButton('−', 'Micșorează pagina', function () { applyZoom(readZoom() - ZOOM_STEP); }));
        rowZoom.appendChild(mkButton('+', 'Mărește pagina', function () { applyZoom(readZoom() + ZOOM_STEP); }));
        rowZoom.appendChild(mkButton('100%', 'Mărimea normală', function () { applyZoom(1); }));
        menuBody.appendChild(rowZoom);

        lblStatus = document.createElement('div');
        lblStatus.style.cssText = 'padding:2px 0 4px 0;color:#d2d2d2;white-space:nowrap;';
        menuBody.appendChild(lblStatus);

        var rowOps = document.createElement('div');
        rowOps.style.cssText = 'display:flex;align-items:center;';
        btnStart = mkButton('▶ Începe', 'Pornește urmărirea manual (când butonul din FOREXE nu a fost recunoscut)',
            function () { startOperation('manual', 'Operațiune manuală'); });
        btnFinish = mkButton('■ Gata', 'Operațiunea s-a salvat: trimite angajamentul spre K-BOT acum',
            function () { finishOperation('încheiat manual'); });
        btnCancel = mkButton('✕', 'Renunță la urmărirea operațiunii curente',
            function () { cancelOperation('renunțat manual'); });
        rowOps.appendChild(btnStart);
        rowOps.appendChild(btnFinish);
        rowOps.appendChild(btnCancel);
        menuBody.appendChild(rowOps);

        makeDraggable(head);
        restorePosition();
        document.body.appendChild(menu);
        renderStatus();
        setFolded(readFolded());
    }

    function readFolded() {
        try { return localStorage.getItem(KEY_COLLAPSED) === '1'; } catch (e) { return false; }
    }

    function setFolded(folded) {
        try {
            if (folded) { localStorage.setItem(KEY_COLLAPSED, '1'); }
            else { localStorage.removeItem(KEY_COLLAPSED); }
        } catch (ignored) { }
        if (menuBody) { menuBody.style.display = folded ? 'none' : ''; }
        if (menu) { menu.style.minWidth = folded ? '0' : '230px'; }
        if (btnFold) {
            btnFold.textContent = folded ? '▸' : '▾';
            btnFold.title = folded ? 'Desfășoară meniul K-BOT' : 'Strânge meniul K-BOT';
        }
    }

    function renderStatus() {
        if (!lblStatus) { return; }
        var text;
        if (!state.op) {
            text = '○ urmărire inactivă';
            lblStatus.style.color = '#9a9a9a';
        } else if (state.pendingSince) {
            text = '⏳ ' + state.label + ' · aștept salvarea';
            lblStatus.style.color = '#e6aa46';
        } else {
            text = '● ' + state.label + ' · din ' + timeText(state.startedAt);
            lblStatus.style.color = '#7ee787';
        }
        lblStatus.textContent = text;
        if (btnStart) { btnStart.disabled = !!state.op; btnStart.style.opacity = state.op ? '.4' : '1'; }
        if (btnFinish) { btnFinish.disabled = !state.op; btnFinish.style.opacity = state.op ? '1' : '.4'; }
        if (btnCancel) { btnCancel.disabled = !state.op; btnCancel.style.opacity = state.op ? '1' : '.4'; }
    }

    function restorePosition() {
        try {
            var raw = localStorage.getItem(KEY_POS);
            if (!raw) { return; }
            var pos = JSON.parse(raw);
            if (typeof pos.right === 'number' && typeof pos.bottom === 'number') {
                menu.style.right = Math.max(0, pos.right) + 'px';
                menu.style.bottom = Math.max(0, pos.bottom) + 'px';
            }
        } catch (ignored) { }
    }

    function makeDraggable(handle) {
        var dragging = false;
        var startX = 0, startY = 0, startRight = 0, startBottom = 0;
        handle.addEventListener('mousedown', function (e) {
            if (e.button !== 0) { return; }
            if (e.target && e.target.tagName === 'BUTTON') { return; }
            dragging = true;
            startX = e.clientX;
            startY = e.clientY;
            startRight = parseFloat(menu.style.right) || 16;
            startBottom = parseFloat(menu.style.bottom) || 16;
            e.preventDefault();
            e.stopPropagation();
        }, true);
        document.addEventListener('mousemove', function (e) {
            if (!dragging) { return; }
            var z = readZoom();
            menu.style.right = Math.max(0, startRight - (e.clientX - startX) / z) + 'px';
            menu.style.bottom = Math.max(0, startBottom - (e.clientY - startY) / z) + 'px';
        }, true);
        document.addEventListener('mouseup', function () {
            if (!dragging) { return; }
            dragging = false;
            try {
                localStorage.setItem(KEY_POS, JSON.stringify({
                    right: parseFloat(menu.style.right) || 16,
                    bottom: parseFloat(menu.style.bottom) || 16
                }));
            } catch (ignored) { }
        }, true);
    }

    // ── Suspend (a robot job is driving the page) ────────────────────────────
    // The robot's clicks would arm operations of their own, and its Playwright clicks
    // would fail on a menu sitting over the target, so both go away while a job runs.
    function setSuspended(flag, message) {
        suspended = !!flag;
        try {
            if (suspended) {
                sessionStorage.setItem(KEY_SUSPEND, '1');
                sessionStorage.setItem(KEY_SUSPEND_MSG, norm(message) || DEFAULT_BUSY_TEXT);
            } else {
                sessionStorage.removeItem(KEY_SUSPEND);
                sessionStorage.removeItem(KEY_SUSPEND_MSG);
            }
        } catch (ignored) { }
        if (menu) { menu.style.display = suspended ? 'none' : ''; }
        // The rules stay on (section 4); the veil follows the flag (section 9).
        if (styleEl) { styleEl.disabled = false; }
        if (!veil) { buildVeil(); }
        syncBusy();
        // Handed back after a robot job: say at once which angajament it left on screen,
        // even when it is the same code as before the job (the shell may have missed it).
        if (!suspended) { reportPage(true); }
    }

    function readSuspended() {
        try { return sessionStorage.getItem(KEY_SUSPEND) === '1'; } catch (e) { return false; }
    }

    // ── Public surface for .NET (EvaluateAsync) ──────────────────────────────
    window._kbotWatch = {
        setSuspended: setSuspended,
        setZoom: applyZoom,
        getZoom: readZoom,
        start: function (label) { startOperation('manual', label || 'Operațiune manuală'); },
        finish: function () { finishOperation('încheiat din K-BOT'); },
        cancel: function () { cancelOperation('renunțat din K-BOT'); },
        getState: function () { return JSON.parse(JSON.stringify(state)); },
        reportPage: function () { reportPage(true); },
        getCod: readCod,
        setMarcaj: setMarcaj,
        configure: configure,
        getConfig: function () { return JSON.parse(JSON.stringify(config)); },
        listElements: listElements,
        highlightElement: highlightElement,
        hiddenByStyles: hiddenByStyles,
        liftStyles: liftStyles
    };

    // The styles go in as early as the document allows - the moment <html> exists, which
    // is before anything is painted (section 11): no unstyled first paint, and a document
    // that comes up in the middle of a robot job comes up covered.
    suspended = readSuspended();
    whenRootExists(earlyInstall);

    // ── Boot ─────────────────────────────────────────────────────────────────
    function boot() {
        suspended = readSuspended();
        // Should the early install have missed (no root element at the time and no
        // observer), the sheets go in here and the console says so once - that is the
        // «comes back after a moment» of section 11, and it must not happen quietly.
        var sheetsLate = !styleEl || !styleEl.parentNode || !busyStyleEl || !busyStyleEl.parentNode;
        resync();
        watchRootAndHead();
        if (sheetsLate && !sheetsLateOnce) {
            sheetsLateOnce = true;
            emit('info', { message: 'stilurile paginii au intrat abia la încărcarea completă (nu înainte de prima afișare)' });
        }
        buildMenu();
        buildVeil();
        applyZoom(readZoom(), true);
        if (suspended && menu) { menu.style.display = 'none'; }
        // The form guard goes BEFORE the watcher: a swallowed save must not arm a finish.
        document.addEventListener('click', onGuardClick, true);
        document.addEventListener('keydown', onGuardKey, true);
        // Slice 0076: the marker holds a save click between the guard and the watcher.
        document.addEventListener('click', onMarcajClick, true);
        document.addEventListener('keydown', onMarcajKey, true);
        document.addEventListener('click', onClick, true);
        // While the blocking message is up no key reaches the page; registered first so
        // it runs before the guard and the devtools filter.
        window.addEventListener('keydown', onBlockKey, true);
        refreshGuards();
        try {
            new MutationObserver(scheduleGuardRefresh).observe(document.body || document.documentElement,
                { childList: true, subtree: true });
        } catch (ignored) { }
        // Always on, suspended or not: the robot never presses these, the operator must not.
        window.addEventListener('keydown', onKeyDown, true);
        window.addEventListener('contextmenu', onContextMenu, true);
        // A save that navigated: the pending flag survived in sessionStorage, so the new
        // page decides whether the operation is done.
        if (state.op && state.pendingSince) { schedulePendingCheck(); }
        // Which angajament this page shows - the new page says so as soon as it is ready.
        reportPage(true);
        // Wicket re-renders pieces of the page, never the body - but should the menu ever
        // be dropped, it comes back. The same beat re-reads the header code: a Wicket
        // re-render that swaps the angajament is not a navigation, so nothing else sees it.
        setInterval(function () {
            if (!document.body) { return; }
            if (!menu || !document.body.contains(menu)) { menu = null; buildMenu(); applyZoom(readZoom(), true); }
            reportPage(false);
            refreshGuards();
            checkAbandoned();
            // The last safety net under the watchers of section 11: should anything drop
            // the sheets, the busy class or the veil past all of them, they come back here.
            resync();
        }, MENU_KEEPALIVE_MS);
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', boot);
    } else {
        boot();
    }

})();
