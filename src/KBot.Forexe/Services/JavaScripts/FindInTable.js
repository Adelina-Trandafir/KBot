((table, args) => {
    // ── Transform helper ─────────────────────────────────────────────────────
    function applyTransform(text, transforms) {
        let result = text;
        for (const t of transforms) {
            if (t === 'trim') result = result.trim();
            else if (t === 'lower') result = result.toLowerCase();
            else if (t === 'upper') result = result.toUpperCase();
            else if (t === 'stripNonAlphaNum') result = result.replace(/[^a-zA-Z0-9]/g, '');
            else if (t.startsWith('left:')) result = result.substring(0, parseInt(t.split(':')[1]));
            else if (t.startsWith('right:')) result = result.slice(-parseInt(t.split(':')[1]));
            else if (t.startsWith('regex:')) {
                const parts = t.substring(6).split(':');
                const rParts = parts[0].match(/^\/(.+)\/([gimy]*)$/);
                if (rParts) result = result.replace(new RegExp(rParts[1], rParts[2]), parts[1] || '');
            }
        }
        return result;
    }

    // ── getCellText — strip tooltip-uri (pattern FindInTable) ────────────────
    function getCellText(cell) {
        return Array.from(cell.childNodes)
            .filter(n => !n.classList || !n.classList.contains('tooltip'))
            .map(n => n.textContent || '')
            .join('')
            .replace(/[\n\t\r\s]+/g, ' ')
            .trim();
    }

    const transforms = args.fieldTransform ? args.fieldTransform.split('|') : [];
    const useIncludes = transforms.includes('stripNonAlphaNum') ||
        transforms.some(t => t.startsWith('regex:'));

    // ── A. Găsire index coloană ───────────────────────────────────────────────
    const fieldName = args.field.trim().toLowerCase();
    let headers = Array.from(table.querySelectorAll('thead th'));
    if (headers.length === 0)
        headers = Array.from(table.querySelectorAll('tr:first-child td, tr:first-child th'));

    const colIndex = headers.findIndex(h => h.innerText.trim().toLowerCase() === fieldName);
    if (colIndex === -1) {
        const foundCols = headers.map(h => h.innerText.trim()).join(', ');
        return { error: `Coloana '${args.field}' nu a fost găsită. Coloane disponibile: ${foundCols}` };
    }

    // ── B. Valoare căutată cu transform ───────────────────────────────────────
    const valueToFind = applyTransform(args.val.trim(), transforms).toLowerCase();

    // ── C. Iterare rânduri ────────────────────────────────────────────────────
    const rows = table.querySelectorAll('tbody tr');
    for (let i = 0; i < rows.length; i++) {
        const cells = rows[i].querySelectorAll('td');
        if (cells.length <= colIndex) continue;

        const cellText = applyTransform(getCellText(cells[colIndex]), transforms).toLowerCase();
        const isMatch = useIncludes ? cellText.includes(valueToFind) : cellText === valueToFind;

        if (isMatch) {
            const rowData = {};
            headers.forEach((h, idx) => {
                if (cells[idx]) rowData[h.innerText.trim()] = getCellText(cells[idx]);
            });
            return { found: true, rowIndex: i, data: rowData };
        }
    }

    // ── D. Nu s-a găsit — returnează debug info ───────────────────────────────
    const firstCell = rows.length > 0
        ? (rows[0].querySelectorAll('td')[colIndex] || { childNodes: [] })
        : { childNodes: [] };

    return {
        found: false,
        debug_valueToFind: valueToFind,
        debug_firstCellText: rows.length > 0 ? getCellText(firstCell) : 'NO_ROWS',
        debug_firstCellText_transformed: rows.length > 0
            ? applyTransform(getCellText(firstCell), transforms).toLowerCase()
            : 'NO_ROWS'
    };
})