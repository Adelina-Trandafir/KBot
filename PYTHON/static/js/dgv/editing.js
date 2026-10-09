// SLICE-ADE2-01/02: opt-in cell editing, independent of recycled row elements.
import { Combobox } from '../components/combobox/combobox.js';
import eventBus from '../event-bus/event-bus.js';

export function parseCell(raw, column) {
  if (column.parse) return column.parse(raw);
  if (raw === '' && column.nullable) return null;
  if (column.editor === 'number') {
    if (!/^[+-]?(?:\d+(?:[.,]\d+)?|[.,]\d+)$/.test(raw.trim())) {
      throw new Error('Introduceți un număr fără separatori de mii.');
    }
    const value = Number(raw.replace(',', '.'));
    if (!Number.isFinite(value)) throw new Error('Număr în afara limitelor.');
    return value;
  }
  if (column.editor === 'date' && !/^\d{4}-\d{2}-\d{2}$/.test(raw)) {
    throw new Error('Introduceți o dată validă.');
  }
  return raw;
}

export const editing = {
  get hasEdit() { return !!this._edit; },
  canEdit(row, column) {
    return !!this._opts.editable && !!column?.editable
      && (typeof column.editable !== 'function' || column.editable(row));
  },
  beginEdit(row, key) {
    if (this._edit || this._destroyed) return false;
    const column = this._col(key);
    if (!this.canEdit(row, column)) return false;
    if (!this._opts.rowKey || row[this._opts.rowKey] == null || !this._opts.onCellSave) {
      throw new Error('Editing requires rowKey and onCellSave');
    }
    const host = document.createElement('div');
    host.className = 'dgv__editor';
    const state = { row, key, column, host, raw: row[key] ?? '', pending: false,
      abort: new AbortController(), value: row[key] };
    this._edit = state;
    if (column.editor === 'list') {
      state.combo = new Combobox(host, { staticData: column.options || [],
        onSelect: (value) => { state.raw = value; state.value = value; } });
      const selected = (column.options || []).find((item) => item.value === row[key]);
      state.combo.setValue(row[key], selected?.text ?? String(row[key] ?? ''));
      state.input = state.combo.input;
    } else {
      state.input = document.createElement('input');
      state.input.type = column.editor === 'date' ? 'date' : 'text';
      if (column.editor === 'number') state.input.inputMode = 'decimal';
      state.input.value = state.raw;
      host.append(state.input);
      state.input.addEventListener('input', () => { state.raw = state.input.value; }, { signal: state.abort.signal });
    }
    state.input.setAttribute('aria-label', column.title);
    host.addEventListener('click', (event) => event.stopPropagation(), { signal: state.abort.signal });
    host.addEventListener('dblclick', (event) => event.stopPropagation(), { signal: state.abort.signal });
    host.addEventListener('keydown', (event) => {
      event.stopPropagation();
      if (event.key === 'Escape') { event.preventDefault(); this.cancelEdit(); }
      if (event.key === 'Enter' || event.key === 'Tab') {
        event.preventDefault(); this.commitEdit(event.key === 'Tab' ? (event.shiftKey ? -1 : 1) : 0);
      }
    }, { signal: state.abort.signal });
    this._selected = row;
    this._ensureVisible(this._items.findIndex((item) => item.row === row));
    this._renderWindow(true);
    state.input.focus();
    state.input.select?.();
    this._emit('onEditState', { state: 'editing', key });
    return true;
  },
  cancelEdit(force = false) {
    const state = this._edit;
    if (!state || (state.pending && !force)) return false;
    this._edit = null;
    state.abort.abort();
    state.combo?.destroy();
    state.host.remove();
    if (!this._destroyed) { this._renderWindow(true); this._root.focus(); }
    this._emit('onEditState', { state: 'idle', key: state.key });
    return true;
  },
  async commitEdit(direction = 0) {
    const state = this._edit;
    if (!state || state.pending) return false;
    try {
      const value = state.column.editor === 'list' ? state.value : parseCell(String(state.raw), state.column);
      const message = state.column.validate?.(value, state.row);
      if (message) throw new Error(message);
      state.pending = true;
      state.input.disabled = true;
      state.host.setAttribute('aria-busy', 'true');
      this._emit('onEditState', { state: 'saving', key: state.key });
      const saved = await this._opts.onCellSave({ row: { ...state.row }, key: state.key, value,
        previous: state.row[state.key], rowId: state.row[this._opts.rowKey] });
      if (this._destroyed || this._edit !== state) return false;
      Object.assign(state.row, saved || { [state.key]: value });
      this.cancelEdit(true);
      this._refresh({ rebuildHeader: true });
      eventBus.emit('dgv:cell-saved', { grid: this._opts.layoutId, key: state.key });
      this._emit('onCellSaved', { row: state.row, key: state.key });
      if (direction) {
        const columns = this._visibleColumns().filter((column) => this.canEdit(state.row, column));
        const next = columns[columns.findIndex((column) => column.key === state.key) + direction];
        if (next) this.beginEdit(state.row, next.key);
      }
      return true;
    } catch (error) {
      if (this._destroyed || this._edit !== state) return false;
      state.pending = false;
      state.input.disabled = false;
      state.input.setAttribute('aria-invalid', 'true');
      state.host.setAttribute('aria-busy', 'false');
      state.host.title = error.message;
      this._emit('onEditError', { error, key: state.key });
      console.error('[DataGrid] Cell save failed', error);
      state.input.focus();
      return false;
    }
  },
};
