(function(root) {
    const children = Array.from(root.querySelectorAll('*'));
    if (children.length === 0) return '[]';

    function getDepth(node) {
        let d = 0, p = node.parentElement;
        while (p && p !== root) { d++; p = p.parentElement; }
        return d;
    }

    return JSON.stringify(children.map(function (c) {
        try {
            const style = window.getComputedStyle(c);
            const rect = c.getBoundingClientRect();
            const cx = rect.left + rect.width / 2;
            const cy = rect.top + rect.height / 2;
            const top = document.elementFromPoint(cx, cy);
            let isObscured = top && top !== c && !c.contains(top);
            let obscuredBy = 'Nimeni (Clean)';
            if (isObscured) {
                let cls = top.className ? '.' + top.className.toString().replace(/ /g, '.') : '';
                let id = top.id ? '#' + top.id : '';
                obscuredBy = (top.tagName || 'UNKNOWN') + id + cls;
            }
            return {
                depth: getDepth(c),
                tagName: c.tagName,
                id: c.id || '(fara id)',
                classes: c.className ? c.className.toString() : '(fara clase)',
                text: c.innerText ? c.innerText.substring(0, 50).replace(/\n/g, ' ') : '(fara text)',
                href: c.href || 'n/a',
                disabled: c.disabled ? 'DA' : 'Nu',
                readonly: c.readOnly ? 'DA' : 'Nu',
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
                htmlPreview: c.outerHTML ? c.outerHTML.substring(0, 100) : 'N/A'
            };
        } catch (err) {
            return { depth: 0, error: err.toString() };
        }
    }));
})