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
    //  EVERY selector in OPS below is copied from the workflows in
    //  Workflows/Creare (Creare Angajament, Incarca Rezervare, Rezervare si
    //  Receptie) - they are the selectors the robot itself clicks, so they are as
    //  verified as anything in this repo. "receptie-modificare" has no rules yet:
    //  the flow is unknown (operator, 21.09.2026) and stays a TODO.
    // =========================================================================

    if (window._kbotWatchInstalled) { return; }
    window._kbotWatchInstalled = true;

    var KEY_STATE = 'kbotWatchState';      // sessionStorage: JSON of the state below
    var KEY_SUSPEND = 'kbotWatchSuspend';  // sessionStorage: '1' while a robot job runs
    var KEY_ZOOM = 'kbotWatchZoom';        // localStorage: zoom factor as text
    var KEY_POS = 'kbotWatchPos';          // localStorage: JSON {right, bottom} of the menu

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
                return !!readCod() && !exists('form.form-horizontal select[name=\'receptie\']');
            }
        }
        // 'receptie-modificare': TODO - the FOREXE flow for changing a reception is not
        // known yet, so nothing arms it automatically. The manual Start / Gata buttons
        // cover it meanwhile.
    };

    // ── State ────────────────────────────────────────────────────────────────
    var state = loadState();
    var suspended = false;
    var pendingTimer = null;
    var menu = null;
    var lblStatus = null;
    var btnStart = null;
    var btnFinish = null;
    var btnCancel = null;
    var lblZoom = null;

    function loadState() {
        try {
            var raw = sessionStorage.getItem(KEY_STATE);
            if (raw) { return JSON.parse(raw); }
        } catch (ignored) { }
        return { op: null, label: '', startedAt: null, codAtStart: '', pendingSince: null };
    }

    function saveState() {
        try { sessionStorage.setItem(KEY_STATE, JSON.stringify(state)); } catch (ignored) { }
    }

    function resetState() {
        state = { op: null, label: '', startedAt: null, codAtStart: '', pendingSince: null };
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
            message: (extra && extra.message) || ''
        };
        try { window._kbotWatchCallback(JSON.stringify(payload)); } catch (ignored) { }
    }

    // ── Operation life cycle ─────────────────────────────────────────────────
    function startOperation(opName, label) {
        state.op = opName;
        state.label = label;
        state.startedAt = new Date().toISOString();
        state.codAtStart = readCod();
        state.pendingSince = null;
        saveState();
        emit('started');
        renderStatus();
    }

    function armFinish() {
        if (!state.op) { return; }
        state.pendingSince = new Date().toISOString();
        saveState();
        emit('info', { message: 'salvare apăsată' });
        renderStatus();
        schedulePendingCheck();
    }

    function finishOperation(message) {
        if (!state.op) { return; }
        stopPendingCheck();
        emit('finished', { finishedAt: new Date().toISOString(), message: message || '' });
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

    // ── Click watching (capture phase, so Wicket's own handlers cannot hide it) ──
    function onClick(e) {
        if (suspended) { return; }
        var el = e.target;
        if (!el || !el.tagName) { return; }
        if (menu && menu.contains(el)) { return; }

        if (state.op) {
            var current = OPS[state.op];
            if (current && anyRuleMatches(el, current.finish)) { armFinish(); }
            return;
        }

        for (var name in OPS) {
            if (!Object.prototype.hasOwnProperty.call(OPS, name)) { continue; }
            var rules = OPS[name];
            if (rules.when && !exists(rules.when)) { continue; }
            if (anyRuleMatches(el, rules.start)) {
                startOperation(name, rules.label);
                return;
            }
        }
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
        lblZoom.style.cssText = 'color:#9a9a9a;';
        head.appendChild(title);
        head.appendChild(lblZoom);
        menu.appendChild(head);

        var rowZoom = document.createElement('div');
        rowZoom.style.cssText = 'display:flex;align-items:center;margin-bottom:4px;';
        rowZoom.appendChild(mkButton('−', 'Micșorează pagina', function () { applyZoom(readZoom() - ZOOM_STEP); }));
        rowZoom.appendChild(mkButton('+', 'Mărește pagina', function () { applyZoom(readZoom() + ZOOM_STEP); }));
        rowZoom.appendChild(mkButton('100%', 'Mărimea normală', function () { applyZoom(1); }));
        menu.appendChild(rowZoom);

        lblStatus = document.createElement('div');
        lblStatus.style.cssText = 'padding:2px 0 4px 0;color:#d2d2d2;white-space:nowrap;';
        menu.appendChild(lblStatus);

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
        menu.appendChild(rowOps);

        makeDraggable(head);
        restorePosition();
        document.body.appendChild(menu);
        renderStatus();
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
    function setSuspended(flag) {
        suspended = !!flag;
        try {
            if (suspended) { sessionStorage.setItem(KEY_SUSPEND, '1'); }
            else { sessionStorage.removeItem(KEY_SUSPEND); }
        } catch (ignored) { }
        if (menu) { menu.style.display = suspended ? 'none' : ''; }
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
        getState: function () { return JSON.parse(JSON.stringify(state)); }
    };

    // ── Boot ─────────────────────────────────────────────────────────────────
    function boot() {
        suspended = readSuspended();
        buildMenu();
        applyZoom(readZoom(), true);
        if (suspended && menu) { menu.style.display = 'none'; }
        document.addEventListener('click', onClick, true);
        // A save that navigated: the pending flag survived in sessionStorage, so the new
        // page decides whether the operation is done.
        if (state.op && state.pendingSince) { schedulePendingCheck(); }
        // Wicket re-renders pieces of the page, never the body - but should the menu ever
        // be dropped, it comes back.
        setInterval(function () {
            if (!document.body) { return; }
            if (!menu || !document.body.contains(menu)) { menu = null; buildMenu(); applyZoom(readZoom(), true); }
            if (menu) { menu.style.display = suspended ? 'none' : ''; }
        }, MENU_KEEPALIVE_MS);
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', boot);
    } else {
        boot();
    }

})();
