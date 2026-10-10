// SLICE-ADE9-01: the report page prints itself once its font is loaded; the button is the manual retry.
document.getElementById('report-print').addEventListener('click', () => window.print());
window.addEventListener('load', async () => { await document.fonts.ready; window.print(); }, { once: true });
