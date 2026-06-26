(function(e) {
    if (!e) return 'EROARE: null';
    try {
        const style = window.getComputedStyle(e);
        const rect = e.getBoundingClientRect();
        const cx = rect.left + rect.width / 2;
        const cy = rect.top + rect.height / 2;
        const top = document.elementFromPoint(cx, cy);
        let isObscured = top && top !== e && !e.contains(top);
        let obscuredBy = 'Nimeni (Clean)';
        if (isObscured) {
            let cls = top.className ? '.' + top.className.toString().replace(/ /g, '.') : '';
            let id = top.id ? '#' + top.id : '';
            obscuredBy = (top.tagName || 'UNKNOWN') + id + cls;
        }
        return JSON.stringify({
            tagName: e.tagName,
            id: e.id || '(fara id)',
            classes: e.className ? e.className.toString() : '(fara clase)',
            text: e.innerText ? e.innerText.substring(0, 50).replace(/\n/g, ' ') : '(fara text)',
            href: e.href || 'n/a',
            disabled: e.disabled ? 'DA' : 'Nu',
            readonly: e.readOnly ? 'DA' : 'Nu',
            display: style.display,
            visibility: style.visibility,
            opacity: style.opacity,
            position: style.position,
            zIndex: style.zIndex,
            pointerEvents: style.pointerEvents,
            width: Math.round(rect.width) + 'px',
            height: Math.round(rect.height) + 'px',
            isCovered: isObscured ? 'DA ⚠️' : 'Nu',
            coveredBy: obscuredBy,
            htmlPreview: e.outerHTML ? e.outerHTML.substring(0, 100) : 'N/A'
        });
    } catch (err) { return 'EROARE JS: ' + err.toString(); }
})