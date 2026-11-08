import { DataGrid, ValueType } from '../dgv/datagrid.js';
import { decimalText } from '../vercon/decimal-text.js';
import { state, excelLeft, verconLeft, text } from './state.js';

const money = v => decimalText(v, 2), qty = v => decimalText(v, 4).replace(/,?0+$/, '').replace(/,$/, '');
const number = (key, title, width, formatter = qty) => ({ key, title, width, minWidth: Math.min(width, 50),
  fixedWidth: true, valueType: ValueType.Number, formatter });
const columns = first => [number(first, first === 'serial' ? 'NrCrt' : 'Ordinea', 56, v => String(v)),
  { key: 'name', title: 'Denumire', width: 80, minWidth: 80, valueType: ValueType.Text },
  { key: 'um', title: 'UM', width: 65, valueType: ValueType.Text }, number('quantity', 'Cant', 45), number('price', 'PU', 70, money)];
const countFooter = grid => grid.setFooter({ caption: '', values: { name: `${grid.shownRowCount} rânduri` } });

export function mountGrids(onExcel, onCandidate, onAssignment) {
  const create = (id, opts) => new DataGrid(document.getElementById(id), { enableGrouping: false, editable: false,
    filterSearch: { minLength: 3, debounceMs: 300, resetSelection: true },
    autoSize: false, rowHeight: 30, headerHeight: 32, footer: true, footerCaption: '', footerValues: { name: '0 rânduri' },
    rowKey: 'row', layout: { fill: 'name' },
    theme: document.documentElement.dataset.theme === 'dark' ? 'dark' : 'modern', ...opts,
    onFilter: (filter, grid) => { countFooter(grid); opts.onFilter?.(filter, grid); } });
  const excel = create('vi-excel-grid', { columns: columns('serial'), defaultSort: { key: 'name', dir: 'asc' }, onSelect: onExcel });
  const vercon = create('vi-candidate-grid', { rowKey: 'variant_id', columns: columns('rank'), defaultSort: { key: 'rank', dir: 'asc' }, onSelect: onCandidate });
  const assigned = create('vi-assigned-grid', { rowKey: 'id', proportionalWidths: true, columns: [number('serial', 'NrCrt Excel', 76, v => String(v)),
    { key: 'name', title: 'Denumire Excel', width: 80, minWidth: 80 }, { key: 'vercon_name', title: 'Denumire VERCON', width: 80, minWidth: 80 },
    number('quantity', 'Cant asociată', 60), number('excel_price', 'PU Excel', 70, money), number('vercon_price', 'PU VERCON', 70, money)],
    defaultSort: { key: 'name', dir: 'asc' }, onSelect: onAssignment });
  return {
    excel, vercon, assigned,
    paint() {
      const sources = state.sent ? [] : state.rows.filter(r => excelLeft(r) > 0n).map(r => ({ ...r, quantity: text(excelLeft(r)) }));
      excel.setRows(sources, { keepWidths: true });
      if (state.excel && sources.some(r => r.row === state.excel.row)) excel.selectRow(state.excel.row);
      const targets = state.candidates.filter(r => verconLeft(r) > 0n).map((r, i) => ({ ...r, rank: i + 1, quantity: text(verconLeft(r)) }));
      vercon.setRows(targets, { keepWidths: true });
      if (state.candidate) vercon.selectRow(state.candidate.variant_id);
      assigned.setRows(state.assignments, { keepWidths: true });
      if (state.assignment) assigned.selectRow(state.assignment.id);
      [excel, vercon, assigned].forEach(countFooter);
      document.getElementById('vi-excel-count').textContent = `(${sources.length})`;
      document.getElementById('vi-candidate-count').textContent = `(${targets.length})`;
      document.getElementById('vi-assigned-count').textContent = `(${state.assignments.length})`;
    },
    destroy() { excel.destroy(); vercon.destroy(); assigned.destroy(); },
  };
}
