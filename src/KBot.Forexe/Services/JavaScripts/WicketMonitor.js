(function () {
    'use strict';

    function setupWicketMonitor() {
        var statEl = document.querySelector('#statlogo');
        var animEl = document.querySelector('#animlogo');
        if (!statEl && !animEl) return;

        function report(id, displayVal) {
            if (typeof window._wicketMonitorCallback !== 'function') return;
            try {
                window._wicketMonitorCallback(JSON.stringify({
                    element: id,
                    state: displayVal,
                    url: window.location.href,
                    ts: new Date().toISOString()
                }));
            } catch (e) { }
        }

        if (statEl) report('#statlogo', getComputedStyle(statEl).display);
        if (animEl) report('#animlogo', getComputedStyle(animEl).display);

        var observer = new MutationObserver(function (mutations) {
            for (var i = 0; i < mutations.length; i++) {
                var el = mutations[i].target;
                if (el.id === 'statlogo' || el.id === 'animlogo') {
                    report('#' + el.id, getComputedStyle(el).display);
                }
            }
        });

        observer.observe(document.body, {
            subtree: true,
            attributes: true,
            attributeFilter: ['style', 'class']
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', setupWicketMonitor);
    } else {
        setupWicketMonitor();
    }
})();