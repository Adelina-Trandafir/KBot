(function () {
    'use strict';

    // =========================================================================
    //  Recorder.js - DOM capture for the K-BOT recorder.
    //
    //  Installed twice on purpose: once through AddInitScriptAsync (future
    //  navigations) and once through EvaluateAsync (the page that is already
    //  loaded when recording starts). The guard below makes the second install
    //  harmless.
    //
    //  Never reuses _clickMonitorCallback / _keyMonitorCallback: those names are
    //  taken by WorkflowExecutor.ClickMonitor.vb and ExposeFunctionAsync throws
    //  on a second registration of the same name.
    //
    //  Wicket ids are regenerated on every rerender (id6, id2a, ...), so #id is
    //  never used as a selector source.
    // =========================================================================

    if (window._kbotRecorderInstalled) { return; }
    window._kbotRecorderInstalled = true;

    var MAX_TEXT = 60;
    var MAX_PATH_TEXT = 30;
    var MAX_PATH_DEPTH = 8;
    var MAX_CANDIDATES = 5;
    var MIN_HAS_TEXT = 6;

    var CLASS_BLACKLIST = [
        'active', 'focus', 'focused', 'hover', 'open', 'selected', 'disabled',
        'error', 'has-error', 'select2-container-active', 'select2-dropdown-open'
    ];
    var DATA_WHITELIST = ['data-recordid', 'data-id', 'data-code'];
    var HAS_TEXT_TAGS = ['a', 'button', 'label', 'h3', 'li'];

    // ── Text helpers ─────────────────────────────────────────────────────────
    function norm(s) {
        return (s || '').replace(/\s+/g, ' ').trim();
    }

    function cut(s, n) {
        var t = norm(s);
        return t.length > n ? t.substring(0, n) : t;
    }

    // A quoted :has-text() fragment cannot contain an apostrophe. Keep the longest
    // apostrophe free run; give up when nothing usable is left.
    function apostropheSafe(text) {
        if (text.indexOf("'") < 0) { return text; }
        var parts = text.split("'");
        var best = '';
        for (var i = 0; i < parts.length; i++) {
            var p = norm(parts[i]);
            if (p.length > best.length) { best = p; }
        }
        return best.length >= MIN_HAS_TEXT ? best : null;
    }

    // ── Class helpers ────────────────────────────────────────────────────────
    function isNoisyClass(c) {
        if (!c) { return true; }
        var lower = c.toLowerCase();
        if (CLASS_BLACKLIST.indexOf(lower) >= 0) { return true; }
        if (lower.indexOf('ng-') === 0) { return true; }
        if (lower.indexOf('ui-') === 0) { return true; }
        if (/^id[0-9a-f]{1,8}$/.test(lower)) { return true; }
        if (/\d{3,}/.test(lower)) { return true; }
        return false;
    }

    function cleanClasses(el) {
        var out = [];
        if (!el || !el.classList) { return out; }
        for (var i = 0; i < el.classList.length; i++) {
            var c = el.classList[i];
            if (!isNoisyClass(c)) { out.push(c); }
        }
        return out;
    }

    function cssEscapeClass(c) {
        return c.replace(/([^A-Za-z0-9_-])/g, '\\$1');
    }

    function tagOf(el) {
        return el && el.tagName ? el.tagName.toLowerCase() : '';
    }

    function indexInParent(el) {
        if (!el || !el.parentElement) { return 0; }
        var kids = el.parentElement.children;
        for (var i = 0; i < kids.length; i++) {
            if (kids[i] === el) { return i + 1; }
        }
        return 0;
    }

    function dataAttrs(el) {
        var out = {};
        if (!el || !el.getAttribute) { return out; }
        for (var i = 0; i < DATA_WHITELIST.length; i++) {
            var name = DATA_WHITELIST[i];
            var val = el.getAttribute(name);
            if (val) { out[name] = val; }
        }
        return out;
    }

    // ── Widget classification, first match wins ──────────────────────────────
    function closestSel(el, sel) {
        if (!el || !el.closest) { return null; }
        try { return el.closest(sel); } catch (e) { return null; }
    }

    function classifyWidget(el) {
        if (!el || !el.tagName) { return 'none'; }
        if (closestSel(el, '#select2-drop-mask')) { return 'select2-mask'; }
        if (closestSel(el, 'a.select2-choice')) { return 'select2-open'; }
        if (closestSel(el, '#select2-drop input.select2-input')) { return 'select2-search'; }
        if (closestSel(el, '#select2-drop li.select2-result-selectable')) { return 'select2-pick'; }
        if (tagOf(el) === 'select' && el.getAttribute('name')) { return 'wicket-select'; }
        if (closestSel(el, "a[rel='next'], a[rel='prev'], .btn-next, .btn-prev")) { return 'paginator'; }
        if (closestSel(el, 'a.haction, .glyphicon-eye-open, .glyphicon-list, .divaction')) { return 'row-action'; }
        return 'none';
    }

    // ── Match counting ───────────────────────────────────────────────────────
    // Returns -1 when the selector cannot be counted from JS (invalid or too
    // exotic); -1 never discards a candidate, it only leaves the score alone.
    function cssCount(sel) {
        try {
            return document.querySelectorAll(sel).length;
        } catch (e) {
            return -1;
        }
    }

    // :has-text() is Playwright syntax, not CSS. querySelectorAll does not
    // understand it, so the matches are counted by hand.
    function hasTextCount(tag, text) {
        var needle = norm(text).toLowerCase();
        if (!needle) { return 0; }
        var els = document.getElementsByTagName(tag);
        var n = 0;
        for (var i = 0; i < els.length; i++) {
            if (norm(els[i].innerText).toLowerCase().indexOf(needle) >= 0) { n++; }
        }
        return n;
    }

    function pushCandidate(list, selector, strategy, score, matchCount) {
        if (!selector) { return; }
        if (matchCount === 0) { return; }              // built wrong, drop it
        var fragile = false;
        if (matchCount > 1) {
            score -= 25;
            fragile = true;
        }
        list.push({
            selector: selector,
            strategy: strategy,
            score: score,
            matchCount: matchCount,
            fragile: fragile
        });
    }

    // ── Candidate generation ─────────────────────────────────────────────────
    function buildCandidates(el) {
        var list = [];
        if (!el || !el.tagName) { return list; }

        var tag = tagOf(el);
        var classes = cleanClasses(el);
        var nameAttr = el.getAttribute ? el.getAttribute('name') : null;

        // 1. name-strict
        if (nameAttr) {
            var byName = tag + "[name='" + nameAttr + "']";
            pushCandidate(list, byName, 'name-strict', 100, cssCount(byName));
            if (tag === 'select') {
                var enabled = "select[name='" + nameAttr + "']:not([disabled])";
                pushCandidate(list, enabled, 'name-strict', 98, cssCount(enabled));
            }
        }

        // 2. data-attr from the white list
        var dattrs = dataAttrs(el);
        for (var key in dattrs) {
            if (!Object.prototype.hasOwnProperty.call(dattrs, key)) { continue; }
            var byData = tag + "[" + key + "='" + dattrs[key] + "']";
            pushCandidate(list, byData, 'data-attr', 90, cssCount(byData));
            break;
        }

        // 3. has-text
        if (HAS_TEXT_TAGS.indexOf(tag) >= 0) {
            var raw = norm(el.innerText);
            if (raw.length >= 3) {
                var safe = apostropheSafe(cut(raw, 40));
                if (safe && safe.length >= 3) {
                    pushCandidate(list, tag + ":has-text('" + safe + "')",
                                  'has-text', 80, hasTextCount(tag, safe));
                }
            }
        }

        // 4. ancestor-anchor - the pattern used by the hand written workflows:
        //    an ancestor that owns a named input/select anchors the element.
        var anchor = findAncestorAnchor(el);
        if (anchor) {
            var self = tag;
            if (classes.length > 0) { self += '.' + cssEscapeClass(classes[0]); }
            var byAnchor = "*:has(> " + anchor + ") " + self;
            pushCandidate(list, byAnchor, 'ancestor-anchor', 70, cssCount(byAnchor));
        }

        // 5. class-path
        if (classes.length > 0) {
            var byClass = tag;
            for (var c = 0; c < Math.min(classes.length, 3); c++) {
                byClass += '.' + cssEscapeClass(classes[c]);
            }
            pushCandidate(list, byClass, 'class-path', 50, cssCount(byClass));
        }

        // 6. nth-child, last resort
        var parent = el.parentElement;
        if (parent && tagOf(parent) !== 'html') {
            var parentSel = tagOf(parent);
            var pClasses = cleanClasses(parent);
            if (pClasses.length > 0) { parentSel += '.' + cssEscapeClass(pClasses[0]); }
            var idx = indexInParent(el);
            if (idx > 0) {
                var byNth = parentSel + ' > ' + tag + ':nth-child(' + idx + ')';
                var nthCount = cssCount(byNth);
                if (nthCount !== 0) {
                    list.push({
                        selector: byNth,
                        strategy: 'nth-child',
                        score: nthCount > 1 ? 20 - 25 : 20,
                        matchCount: nthCount,
                        fragile: true
                    });
                }
            }
        }

        list.sort(function (a, b) { return b.score - a.score; });
        return list.slice(0, MAX_CANDIDATES);
    }

    function findAncestorAnchor(el) {
        var node = el ? el.parentElement : null;
        var depth = 0;
        while (node && depth < MAX_PATH_DEPTH && tagOf(node) !== 'body') {
            var kids = node.children;
            for (var i = 0; i < kids.length; i++) {
                var kid = kids[i];
                var kidTag = tagOf(kid);
                if (kidTag !== 'input' && kidTag !== 'select') { continue; }
                var kidName = kid.getAttribute('name');
                if (kidName) { return kidTag + "[name='" + kidName + "']"; }
            }
            node = node.parentElement;
            depth++;
        }
        return null;
    }

    // ── Ancestor path ────────────────────────────────────────────────────────
    function buildPath(el) {
        var path = [];
        var node = el;
        var depth = 0;
        while (node && node.tagName && depth < MAX_PATH_DEPTH) {
            path.push({
                tag: tagOf(node),
                classes: cleanClasses(node),
                name: node.getAttribute ? node.getAttribute('name') : null,
                dataAttrs: dataAttrs(node),
                text: cut(node.innerText, MAX_PATH_TEXT),
                indexInParent: indexInParent(node)
            });
            if (tagOf(node) === 'body') { break; }
            node = node.parentElement;
            depth++;
        }
        return path;
    }

    // ── Wicket busy flags ────────────────────────────────────────────────────
    function busyState() {
        function disp(id) {
            var e = document.querySelector(id);
            if (!e) { return 'none'; }
            try { return getComputedStyle(e).display; } catch (err) { return 'none'; }
        }
        return { statlogo: disp('#statlogo'), animlogo: disp('#animlogo') };
    }

    // ── Reported value ───────────────────────────────────────────────────────
    function valueOf(el) {
        var tag = tagOf(el);
        if (tag === 'select') {
            return el.selectedIndex >= 0 ? norm(el.options[el.selectedIndex].text) : '';
        }
        if (tag === 'input' || tag === 'textarea') {
            return el.value || '';
        }
        return '';
    }

    // ── Emit ─────────────────────────────────────────────────────────────────
    // keyName carries Enter / Tab for kind === 'key'; empty for every other kind.
    // Without it the two keydown events would be indistinguishable downstream.
    function emit(kind, el, keyName) {
        if (typeof window._kbotRecorderCallback !== 'function') { return; }
        if (!el || !el.tagName) { return; }
        try {
            window._kbotRecorderCallback(JSON.stringify({
                kind: kind,
                keyName: keyName || '',
                ts: new Date().toISOString(),
                url: window.location.href,
                tag: tagOf(el),
                type: el.type || null,
                nameAttr: el.getAttribute ? el.getAttribute('name') : null,
                value: valueOf(el),
                text: cut(el.innerText, MAX_TEXT),
                widget: classifyWidget(el),
                candidates: buildCandidates(el),
                path: buildPath(el),
                wicketBusy: busyState()
            }));
        } catch (ignored) { }
    }

    // ── Listeners, all in the capture phase ──────────────────────────────────
    document.addEventListener('mousedown', function (e) { emit('click', e.target, ''); }, true);
    document.addEventListener('input', function (e) { emit('input', e.target, ''); }, true);
    document.addEventListener('change', function (e) { emit('change', e.target, ''); }, true);
    document.addEventListener('focusout', function (e) { emit('blur', e.target, ''); }, true);

    document.addEventListener('keydown', function (e) {
        if (e.key !== 'Enter' && e.key !== 'Tab') { return; }
        emit('key', e.target, e.key);
    }, true);

})();
