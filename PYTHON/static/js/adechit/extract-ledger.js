// SLICE-AD21-01: the "Extrase" tab of the ledger - bank statement lines associated to the selected child,
// with the value of the whole line and the part associated here. Read only: deleting is done in the bank window.
const COLUMNS = [
  { title: 'Număr', cls: 'ade-col-extras-number', cell: (row) => row.NrExtras },
  { title: 'Data extras', cls: 'ade-col-date', cell: (row, tools) => tools.dateText(row.DataExtras) },
  { title: 'Valoare extras', cls: 'ade-col-extras-value is-number', cell: (row, tools) => tools.money(row.suma_credit) },
  { title: 'Valoare asociată', cls: 'ade-col-extras-value is-number', cell: (row, tools) => tools.money(row.suma_asociata) },
  { title: 'Plătitor bancă', cls: 'ade-col-explanation', cell: (row) => row.platitor_nume },
  { title: 'Doc. Alte plăți', cls: 'ade-col-number', cell: (row) => row.NrDoc },
];

export function extractLedgerHtml(rows, tools) {
  const body = rows.length
    ? rows.map((row) => `<tr class="${row.Cancelled ? 'is-cancelled' : ''}">${COLUMNS
      .map((column) => `<td class="${column.cls}">${tools.escapeHtml(column.cell(row, tools) ?? '')}</td>`).join('')}<td class="ade-row-actions">${row.Cancelled ? 'Anulat' : ''}</td></tr>`).join('')
    : `<tr><td colspan="${COLUMNS.length + 1}" class="ade-ledger-empty">Niciun extras asociat acestui copil în luna aleasă.</td></tr>`;
  return `<table class="ade-ledger-table"><colgroup>${COLUMNS.map((column) => `<col class="${column.cls.split(' ')[0]}">`).join('')}<col class="ade-col-actions"></colgroup>
    <thead><tr>${COLUMNS.map((column) => `<th>${column.title}</th>`).join('')}<th class="ade-row-actions"></th></tr></thead><tbody>${body}</tbody></table>`;
}
