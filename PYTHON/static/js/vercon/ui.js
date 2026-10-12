// VERCON v2: small adapters around the application's shared controls. Business state stays in the screen.
import { DataGrid, ValueType } from '../dgv/datagrid.js';
import { TreeView } from '../components/treeview/treeview.js';
import { Combobox } from '../components/combobox/combobox.js';
import { DatePicker } from '../components/datepicker/datepicker.js';
import { openMenu, closeMenu } from '../adechit/menu.js';
import { showMessage } from '../portal/messages.js';
import { createPdfView } from '../portal/pdfview.js';
import { snapshotPdf } from './snapshot-pdf.js';
import { parse, format } from './fields.js';

export { DataGrid, ValueType, TreeView, Combobox, DatePicker, openMenu, closeMenu, showMessage };
export function el(tag, attrs = {}, ...children) {
  const node = document.createElement(tag);
  for (const [key, value] of Object.entries(attrs)) {
    if (key.startsWith('aria-') || key.startsWith('data-') || key === 'role') node.setAttribute(key, value);
    else node[key] = value;
  }
  node.append(...children.filter((child) => child != null));
  return node;
}
export function scope() {
  const resources = new Set();
  return {
    own(resource) { resources.add(resource); return resource; },
    clear() { for (const item of resources) { if (typeof item === 'function') item(); else item.destroy?.(); } resources.clear(); },
    async commit() {
      for (const item of [...resources]) {
        if (item.hasEdit && !await item.commitEdit()) return false;
        if (item instanceof DatePicker && !item.field.disabled) { item.commitText(); if (item.field.classList.contains('is-invalid')) { item.field.focus(); return false; } }
      }
      return true;
    },
    dirty() { return [...resources].some((item) => item.hasEdit || item._pendingSelection || item instanceof DatePicker && !item.field.disabled && item.field.value !== (item.value ? format(item.iso, { type: 'date' }) : '')); },
    busy(on) { for (const item of resources) item.setBusy?.(on); },
  };
}
export function field(column, value, editable, onChange, owner) {
  const label = el('label', { className: 'vc-field', 'data-field': column.key }, el('span', {}, column.label + (column.required && editable ? ' *' : '')));
  const error = el('span', { className: 'vc-field-error', role: 'status' });
  let source;
  if (column.type === 'ref' || column.type === 'select') {
    source = el('input', { type: 'hidden', value: value == null ? '' : String(value) });
    const host = el('div', { className: 'vc-field__combo' }); label.append(host, source);
    const items = (column.options || []).map((item) => ({ value: String(item.value ?? item.id), label: item.label }));
    const combo = owner.own(new Combobox(host, { placeholder: column.placeholder || 'Alegeți…', readonly: false,
      onSearch: async (query) => items.filter((item) => item.label.toLocaleLowerCase('ro').includes(query.toLocaleLowerCase('ro'))),
      onSelect: (id) => { combo._pendingSelection = false; source.value = id; source.dispatchEvent(new Event('change', { bubbles: true })); } }));
    combo.setValue(source.value, items.find((item) => item.value === source.value)?.label || column.fallback || ''); combo.setEnabled(editable);
    combo.input.setAttribute('aria-label', column.label);
    combo.input.addEventListener('input', () => { source.value = ''; combo._pendingSelection = true; });
  } else {
    source = el('input', { type: column.type === 'date' ? 'date' : 'text', value: column.type === 'date' ? (value || '') : format(value, column), readOnly: !editable, autocomplete: 'off', 'aria-label': column.label });
    if (column.type === 'decimal' || column.type === 'int') source.inputMode = 'decimal';
    label.append(source);
    if (column.type === 'date') {
      const picker = owner.own(new DatePicker(source)); picker.field.disabled = picker.button.disabled = !editable;
      source._picker = picker;
    }
  }
  source.addEventListener('change', () => { if (editable) onChange?.(source.value, source); });
  if (column.type !== 'date' && column.type !== 'ref' && column.type !== 'select') source.addEventListener('input', () => { if (editable) onChange?.(source.value, source); });
  label.append(error);
  return { node: label, input: source, error };
}
export function column(spec, extras = {}) {
  const type = spec.type || 'text';
  return { key: spec.key, title: spec.label, valueType: type === 'date' ? ValueType.DateTime : ['decimal', 'int'].includes(type) ? ValueType.Number : ValueType.Text,
    width: type === 'text' ? 180 : 120, editable: true, editor: type === 'date' ? 'date' : ['decimal', 'int'].includes(type) ? 'number' : type === 'ref' ? 'list' : 'text',
    formatter: (value) => format(value, { type }),
    parse: (raw) => { if (type === 'ref') return raw == null || raw === '' ? null : raw; const result = parse(raw, { ...spec, type }); if (!result.ok) throw new Error(result.message); return result.value; },
    align: ['decimal', 'int'].includes(type) ? 'right' : 'left', ...extras };
}
export function grid(host, options, owner) {
  host.classList.add('vc-dgv');
  const instance = new DataGrid(host, { rowHeight: 30, headerHeight: 30, autoSize: false, enableGrouping: false,
    theme: document.documentElement.dataset.theme === 'dark' ? 'dark' : 'modern', ...options,
    columns: options.columns.map((item) => ({ width: 120, ...item })) });
  owner?.own(instance); return instance;
}
export function paintTree(host, nodes, { selected, onSelect, onAction, title = 'Documente salvate', checkable = false, onCheckChange } = {}, owner) {
  host.replaceChildren();
  const body = el('div', { className: 'vc-tree-body' }); host.append(el('h4', { textContent: title }), body);
  const tree = new TreeView(body, { inline: true, autoCollapse: false, autoResize: false, indent: 14, checkable, onCheckChange,
    onSelect: (selection) => { if (String(selection.id).startsWith('doc:')) onSelect?.(String(selection.id).slice(4)); } });
  owner?.own(tree);
  tree.setData(nodes); tree.expandAllNodes(nodes); tree.treeContainer.replaceChildren(); tree.isTreeRendered = false;
  if (selected != null) tree.selectedValue = `doc:${selected}`;
  if (nodes.length) tree.renderTree(''); else tree.renderNoResults();
  const action = (event) => {
    const target = event.target.closest('[data-action]'); if (!target || !body.contains(target)) return;
    if (event.type === 'keydown' && !['Enter', ' '].includes(event.key)) return;
    event.preventDefault(); event.stopPropagation(); onAction?.(target.dataset.nodeId.slice(4), target);
  };
  body.addEventListener('click', action, true); body.addEventListener('keydown', action, true);
  return tree;
}
export function groupedNodes(rows, groups, leaf) {
  const walk = (items, level, prefix) => {
    if (level === groups.length) return items.map((row) => ({ id: `doc:${row.id}`, label: leaf(row), error: !!row.annulled,
      rightIcon: { icon: '⋮', action: 'options', title: 'Opțiuni document' } }));
    const grouped = new Map();
    for (const row of items) { const label = String(groups[level](row) || '—'); if (!grouped.has(label)) grouped.set(label, []); grouped.get(label).push(row); }
    return [...grouped].map(([label, children], index) => ({ id: `group:${prefix}:${index}`, label, children: walk(children, level + 1, `${prefix}:${index}`) }));
  }; return walk(rows, 0, 'root');
}
export function rowMenu(gridInstance, row, entries) {
  const index = gridInstance._items.findIndex((item) => item.kind === 'row' && item.row[gridInstance._opts.rowKey] === row[gridInstance._opts.rowKey]);
  const anchor = gridInstance._root.querySelector('.dgv__row[data-i="' + index + '"] .dgv__cell-action') || gridInstance._root;
  openMenu(anchor, entries, { label: 'Opțiuni rând', align: 'right' });
}
export function modal(title, owner) {
  const dialog = el('dialog', { className: 'confirm-box vc-modal', 'aria-label': title });
  const body = el('div', { className: 'vc-modal__body' }), footer = el('footer');
  const local = scope(); let closed = false;
  const close = () => { if (closed) return; closed = true; local.clear(); closeMenu(); dialog.close(); dialog.remove(); };
  dialog.append(el('header', {}, el('h2', { textContent: title })), body, footer);
  dialog.addEventListener('cancel', (event) => { event.preventDefault(); close(); });
  owner?.own(close); document.body.append(dialog); dialog.showModal();
  return { dialog, body, footer, close, scope: local, get closed() { return closed; } };
}
export function preview(title, sections, owner) {
  const view = modal(title, owner);
  for (const section of sections) {
    view.body.append(el('h4', { textContent: section.title }));
    const host = el('div'); view.body.append(host); host.style.height = Math.min(280, Math.max(92, 32 + section.rows.length * 30 + 16)) + 'px'; grid(host, { columns: section.columns, rows: section.rows, footer: false }, view.scope);
  }
  const close = el('button', { type: 'button', textContent: 'Închide' }); close.addEventListener('click', view.close);
  const pdf = el('button', { type: 'button', textContent: 'Previzualizare PDF' });
  pdf.addEventListener('click', async () => {
    pdf.disabled = true;
    try { const data = await snapshotPdf(title, sections); if (!view.closed) await pdfPreview(title, data, view.scope); }
    catch (error) { if (!view.closed) { const message = el('p'); view.body.append(message); showMessage(message, error.message || 'PDF-ul nu a putut fi pregătit.', 'error'); } }
    finally { pdf.disabled = false; }
  });
  view.footer.append(close, pdf);
  return view;
}
// Reports that return PDF use the same PDF.js viewer as Portal; no endpoint is invented by the UI.
export async function pdfPreview(title, data, owner) {
  const view = modal(title, owner); const host = el('div', { className: 'vc-pdf' }); view.body.append(host);
  const pdf = createPdfView(host); view.scope.own(() => { void pdf.clear(); });
  const blobUrl = URL.createObjectURL(new Blob([data], { type: 'application/pdf' })); view.scope.own(() => URL.revokeObjectURL(blobUrl));
  const close = el('button', { type: 'button', textContent: 'Închide' }); close.addEventListener('click', view.close);
  const download = el('a', { href: blobUrl, download: title.replace(/[<>:"/\\|?*]/g, '_') + '.pdf', textContent: 'Descarcă PDF' });
  const print = el('button', { type: 'button', textContent: 'Tipărește' }); print.addEventListener('click', () => { const frame = window.open(blobUrl, '_blank'); if (frame) frame.addEventListener('load', () => frame.print(), { once: true }); });
  view.footer.append(close, download, print);
  try { await pdf.open(data); } catch (error) { showMessage(host, error.message, 'error'); }
  return view;
}
