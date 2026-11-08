// Bank matching searches visible catalog groups independently of attendance and statement months.
import { Combobox } from '../components/combobox/combobox.js';
import { orderItems } from '../utils/typed-order.js';

export function bindBankChildSearch({ api, context, ready, run, select }) {
  const lifetime = new AbortController();
  let request = null;
  const contextKey = () => JSON.stringify([context()?.unit, context()?.subunit?.id]);
  const cancel = () => { request?.abort(); request = null; };
  const combo = new Combobox(document.getElementById('ade-bank-child-search'), {
    placeholder: 'Nume sau CNP · minimum 2 caractere', minSearchLength: 2,
    searchDelay: 300, maxResults: 101, dropDownMinWidth: 420,
    autoHighlightMatch: false, highlightMatches: false, sortType: 'text',
    onSearch: async text => {
      cancel();
      const query = text.trim(), key = contextKey();
      if (!ready() || query.length < 2) return [];
      const own = new AbortController(); request = own;
      try {
        const result = await api(`/api/adechit/bank-extracts/child-search?q=${encodeURIComponent(query)}`, { signal: own.signal });
        if (own.signal.aborted || !ready() || contextKey() !== key) return [];
        const matches = orderItems(result.rows.map(row => ({
          value: String(row.IDP), label: `${row.Nume} · ${row.Grupa}${row.CNP_ULTIME ? ` · CNP …${row.CNP_ULTIME}` : ''}`,
          sortValue: row.Nume, sortThen: [{ value: row.Grupa, type: 'text' }, { value: row.CNP_ULTIME, type: 'text' }],
          data: { row, key },
        })), { sortType: 'text' });
        const choices = matches.slice(0, 100);
        if (!choices.length) choices.push({ value: 'empty', label: 'Niciun copil găsit în grupele vizibile.', disabled: true });
        if (matches.length > 100) choices.push({ value: 'more', sortValue: '\uffff', label: 'Mai sunt rezultate. Completați numele pentru a restrânge căutarea.', disabled: true });
        return choices;
      } catch (error) {
        if (own.signal.aborted || !ready() || contextKey() !== key) return [];
        throw error;
      } finally { if (request === own) request = null; }
    },
    onSelect: (_value, _label, item) => {
      if (!item?.data || !ready() || item.data.key !== contextKey()) return;
      const { row, key } = item.data;
      void run(async () => {
        if (key !== contextKey()) return false;
        await select(row);
        return true;
      }).then(selected => {
        if (key !== contextKey()) return;
        if (selected) combo.setValue(String(row.IDP), `${row.Nume} · ${row.Grupa}`);
        else combo.clear();
      });
    },
  });
  combo.input.id = 'ade-bank-child-query';
  combo.input.maxLength = 100;
  combo.input.setAttribute('aria-label', 'Caută copil în toate grupele vizibile ale subunității');
  let pointerEntry = false;
  combo.input.addEventListener('pointerdown', () => {
    pointerEntry = document.activeElement !== combo.input;
  }, { signal: lifetime.signal });
  combo.input.addEventListener('focus', () => combo.input.select(), { signal: lifetime.signal });
  // The first pointer click must not collapse the selection made on focus.
  combo.input.addEventListener('click', () => {
    if (pointerEntry) combo.input.select();
    pointerEntry = false;
  }, { signal: lifetime.signal });
  combo.input.addEventListener('blur', () => { pointerEntry = false; }, { signal: lifetime.signal });
  combo.input.addEventListener('input', () => {
    cancel(); combo.hide(); combo.results = [];
  }, { capture: true, signal: lifetime.signal });
  function reset() { cancel(); combo.cancelSearch(); combo.clear(); combo.results = []; }
  function sync() { combo.setEnabled(ready()); }
  sync();
  return { reset, sync, destroy() { cancel(); lifetime.abort(); combo.destroy(); } };
}
