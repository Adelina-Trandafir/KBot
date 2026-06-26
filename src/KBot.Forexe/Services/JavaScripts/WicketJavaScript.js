(function () {
    'use strict';

    if (window._clickMonitorInstalled) return;
    window._clickMonitorInstalled = true;

    // ── Helper: proprietati per tip element ──────────────────────────────────
    function getElementInfo(el) {
        if (!el || !el.tagName) return { selector: '?', props: {} };

        var tag = el.tagName.toLowerCase();

        var selector = tag;
        for (var i = 0; i < el.classList.length; i++) {
            selector += '.' + el.classList[i];
        }

        var props = {};

        if (tag === 'input' || tag === 'textarea' || tag === 'select') {
            if (el.name) props['name'] = el.name;
            if (tag === 'input' && el.type) props['type'] = el.type;
            if (tag === 'select') {
                props['value'] = el.selectedIndex >= 0
                    ? el.options[el.selectedIndex].text
                    : '(none)';
            } else {
                var val = (el.value || '').trim();
                props['value'] = val.length > 30
                    ? val.substring(0, 30) + '\u2026'
                    : (val || '(empty)');
            }
            if (el.placeholder) props['placeholder'] = el.placeholder;

        } else if (tag === 'a') {
            var href = el.getAttribute('href') || '';
            props['href'] = href.length > 50
                ? '\u2026' + href.substring(href.length - 47)
                : (href || '(no href)');
            var aText = (el.innerText || '').replace(/\s+/g, ' ').trim();
            if (aText) props['text'] = aText.length > 40
                ? aText.substring(0, 40) + '\u2026' : aText;

        } else if (tag === 'button') {
            var btnText = (el.innerText || '').replace(/\s+/g, ' ').trim();
            if (btnText) props['text'] = btnText.length > 40
                ? btnText.substring(0, 40) + '\u2026' : btnText;
            if (el.name) props['name'] = el.name;
            if (el.disabled) props['disabled'] = 'true';

        } else if (tag === 'td' || tag === 'th') {
            // getCellText — strip tooltip-uri (pattern FindInTable)
            var cellText = Array.prototype.filter.call(el.childNodes, function (n) {
                return !n.classList || !n.classList.contains('tooltip');
            }).map(function (n) {
                return n.textContent || '';
            }).join('').replace(/[\n\t\r\s]+/g, ' ').trim();
            if (cellText) props['text'] = cellText.length > 40
                ? cellText.substring(0, 40) + '\u2026' : cellText;
            if (typeof el.cellIndex !== 'undefined') props['col'] = el.cellIndex;

        } else {
            var elText = (el.innerText || '').replace(/\s+/g, ' ').trim();
            if (elText) props['text'] = elText.length > 40
                ? elText.substring(0, 40) + '\u2026' : elText;
        }

        return { selector: selector, props: props };
    }

    // ── Click listener (mousedown, capture) ──────────────────────────────────
    document.addEventListener('mousedown', function (e) {
        if (typeof window._clickMonitorCallback !== 'function') return;
        try {
            var el = e.target;
            var parent = el.parentElement || null;
            var grandparent = parent && parent.parentElement ? parent.parentElement : null;

            window._clickMonitorCallback(JSON.stringify({
                selector: getElementInfo(el).selector,
                props: getElementInfo(el).props,
                parent: parent ? getElementInfo(parent) : null,
                grandparent: grandparent ? getElementInfo(grandparent) : null,
                url: window.location.href,
                ts: new Date().toISOString()
            }));
        } catch (ignored) { }
    }, true);

    // ── Key listener (input event, o singura data per element) ───────────────
    var _lastKeyEl = null;
    document.addEventListener('input', function (e) {
        if (typeof window._keyMonitorCallback !== 'function') return;
        if (e.target === _lastKeyEl) return;
        _lastKeyEl = e.target;
        try {
            var elInfo = getElementInfo(e.target);
            window._keyMonitorCallback(JSON.stringify({
                selector: elInfo.selector,
                props: elInfo.props,
                url: window.location.href,
                ts: new Date().toISOString()
            }));
        } catch (ignored) { }
    }, true);

})();