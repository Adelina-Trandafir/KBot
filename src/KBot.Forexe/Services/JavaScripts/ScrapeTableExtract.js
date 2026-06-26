table => {
    const data = [];

    // ── Construieste matricea header (rowspan + colspan) ──────────────────────
    const headerRows = table.querySelectorAll('thead tr');
    const matrix = [];
    headerRows.forEach(() => matrix.push([]));

    headerRows.forEach((row, rowIndex) => {
        const cells = row.querySelectorAll('th, td');
        let colIndex = 0;
        cells.forEach(cell => {
            while (typeof matrix[rowIndex][colIndex] !== 'undefined') colIndex++;
            let text = cell.innerText.replace(/[\r\n]+/g, ' ').trim();
            const rowspan = parseInt(cell.getAttribute('rowspan') || 1);
            const colspan = parseInt(cell.getAttribute('colspan') || 1);
            for (let r = 0; r < rowspan; r++)
                for (let c = 0; c < colspan; c++) {
                    if (!matrix[rowIndex + r]) matrix[rowIndex + r] = [];
                    matrix[rowIndex + r][colIndex + c] = text;
                }
            colIndex += colspan;
        });
    });

    // ── Aplatizeaza header-ele multi-rand in stringuri cu separator "!" ────────
    const headers = [];
    let numCols = 0;
    matrix.forEach(row => { if (row.length > numCols) numCols = row.length; });

    for (let c = 0; c < numCols; c++) {
        let parts = [];
        for (let r = 0; r < matrix.length; r++) {
            if (matrix[r]) {
                const val = matrix[r][c];
                if (val && val.length > 0 && !parts.includes(val)) parts.push(val);
            }
        }
        headers.push(parts.join('!') || ('Col_' + (c + 1)));
    }

    // ── Extrage randurile tbody ────────────────────────────────────────────────
    table.querySelectorAll('tbody tr').forEach(row => {
        const rowData = {};
        row.querySelectorAll('td').forEach((cell, index) => {
            let key = headers[index] || ('Col_' + (index + 1));
            // Normalizeaza cheia: sterge diacritice, pastreaza doar alfanumeric + _ + !
            key = key.normalize('NFD').replace(/[\u0300-\u036f]/g, '');
            key = key.replace(/[^a-zA-Z0-9_!]+/g, '_').replace(/^_+|_+$/g, '');

            // Prefer valoarea input/select/textarea din celula fata de innerText
            const inputEl = cell.querySelector(
                'input:not([type=checkbox]):not([type=radio]), select, textarea'
            );
            rowData[key] = ((inputEl ? inputEl.value : cell.innerText) || '')
                .replace(/[\r\n]+/g, ' ').trim();
        });
        data.push(rowData);
    });

    return data;
}