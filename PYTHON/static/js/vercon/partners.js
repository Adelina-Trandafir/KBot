// SLICE-VC06-22: the existing nomenclature workspace and shared CUSTOM controls.
import { confirmBox } from '../utils/confirm-box.js';
import { ActionAddButton } from '../components/action-add-button/action-add-button.js';
import { DialogButton, dialogButtonRow } from '../components/dialog-button/dialog-button.js';
import { el, scope, column, grid, TextBox, statusBar, showMessage, rowMenu, closeMenu } from './ui.js';
import { debounced, effectiveSearchText } from './search-settings.js';

const endpoint = '/api/vercon/partners';
const compare = new Intl.Collator('ro', { sensitivity: 'base', numeric: true });

export function mountPartners(host, opts) {
  const ui = scope();
  let disposed = false, busy = false, choosing = false, canUpdate = false, canAdd = false, rows = [], selected = null, dataGrid, missingCode = null;
  // SLICE-VC20-01: the «+» next to the title starts a new supplier in this same form (adding); the fiscal code is looked up in ANAF when it loses focus
  let adding = false, lookingUp = false, checked = null, lastLooked = '', lookupSeq = 0;
  const status = el('span', { className: 'vc-save-status', role: 'status' });
  const refresh = el('button', { type: 'button', textContent: 'Reîmprospătează' });
  const manual = el('button', { type: 'button', textContent: 'Salvează numai CodulFiscal', hidden: true });
  const searchHost = el('div', { className: 'vc-partners__search' });
  const list = el('div', { className: 'vc-nom__list vc-partners__list' });
  const details = el('div', { className: 'vc-partners__details vc-nom__fields' });
  const title = el('h4', { textContent: 'Alegeți un furnizor din listă.' });
  const addButton = ui.own(new ActionAddButton(null, { title: 'Adaugă furnizor', className: 'vc-partners__add', onClick: () => { void startAdd().catch(report); } }));
  addButton.element.hidden = true;
  const head = el('div', { className: 'vc-partners__head' }, title, addButton.element);
  // the shared dialog buttons (Renunță on the left, Salvează on the right), visible only while a new supplier is being added
  const addBar = dialogButtonRow(null, 'vc-partners__addbar'); addBar.hidden = true;
  const cancelNew = ui.own(new DialogButton(addBar, { kind: 'cancel', onClick: () => { if (!busy) leaveAdd(); } }));
  const saveNew = ui.own(new DialogButton(addBar, { kind: 'ok1', disabled: true, onClick: () => { void saveAdd().catch(report); } }));
  const root = el('section', { className: 'vc-nom-page vc-partners', 'aria-label': 'Furnizori (Parteneri)' },
    el('div', { className: 'vc-nom__bar' }, el('h3', { textContent: 'Furnizori (Parteneri)' }), searchHost),
    el('div', { className: 'vc-partners__body' }, list, el('section', { className: 'vc-partners__card' }, head, details, addBar)),
    statusBar(el('div', { className: 'vc-actions' }), { start: [refresh], status, end: [manual] }));
  host.replaceChildren(root);
  const say = (text = '', bad = false) => { if (!disposed) showMessage(status, text, bad ? 'error' : 'info'); };
  const report = (error) => { if (!disposed && error.name !== 'AbortError') say(error.message || 'Operația a eșuat.', true); };
  const controls = new Map();
  for (const [key, label] of [['CodPartener', 'Cod partener'], ['CodFiscal', 'Cod fiscal'], ['DenumirePartener', 'Denumire'],
    ['ContIBAN', 'Cont IBAN'], ['Banca', 'Bancă'], ['Adresa', 'Adresă']]) {
    const node = el('label', { className: 'vc-field', 'data-field': key }, el('span', { textContent: label }));
    controls.set(key, ui.own(new TextBox(node, { type: 'text', maxLength: key === 'CodFiscal' ? 50 : 255,
      readOnly: true, ariaLabel: label, multiline: key === 'Adresa', rows: 3,
      onInput: () => { if (key === 'CodFiscal') { missingCode = null; if (adding) { checked = null; lastLooked = ''; lookupSeq++; lookingUp = false; fill('', ''); controls.get('CodFiscal').clearError(); } sync(); } } })));
    details.append(node);
  }
  const fiscal = controls.get('CodFiscal');
  const search = ui.own(new TextBox(searchHost, { type: 'search', placeholder: 'Caută furnizor / cod fiscal…',
    ariaLabel: 'Caută furnizor', onInput: () => applySearch() }));
  // The list is filtered after a pause in typing and once the text has the minimum characters (spaces not counted).
  const applySearch = debounced(() => { if (!disposed) paintRows(); });
  ui.own(applySearch.cancel);
  const dirty = () => !!selected && fiscal.text !== String(selected.CodFiscal || '');
  const query = () => new URLSearchParams(Object.entries(opts.sel()).filter(([, value]) => value != null && value !== ''));
  const visible = () => {
    const text = effectiveSearchText(search.text).toLocaleLowerCase('ro');
    return rows.filter((row) => !text || ['DenumirePartener', 'CodFiscal', 'CodPartener'].some((key) =>
      String(row[key] || '').toLocaleLowerCase('ro').includes(text)))
      .sort((a, b) => compare.compare(a.DenumirePartener, b.DenumirePartener)
        || compare.compare(a.CodPartener, b.CodPartener) || compare.compare(a.CodFiscal, b.CodFiscal));
  };
  function sync() {
    manual.hidden = !canUpdate || missingCode == null || adding; manual.disabled = busy || !selected;
    refresh.disabled = busy || adding;
    fiscal.setReadOnly(adding ? false : !canUpdate || !selected);
    saveNew.setEnabled(!busy && !lookingUp && !!checked); cancelNew.setEnabled(!busy);
    addButton.setEnabled(!busy && !adding && !choosing);
  }
  const fill = (name, address) => { controls.get('DenumirePartener').setValue(name || ''); controls.get('Adresa').setValue(address || ''); };
  const body = (extra) => JSON.stringify({ subunit: opts.sel().subunit, gestiune: opts.sel().gestiune, ...extra });
  function show(row) {
    selected = row; missingCode = null;
    title.textContent = row?.DenumirePartener || (row ? 'Furnizor fără denumire' : 'Alegeți un furnizor din listă.');
    for (const [key, box] of controls) { box.setValue(row?.[key] || ''); box.clearError(); }
    sync();
  }
  function paintRows() {
    if (!dataGrid) return;
    const listRows = visible();
    dataGrid.setRows(listRows, { keepWidths: true });
    if (selected && listRows.some((row) => row.id === selected.id)) dataGrid.selectRow(selected.id);
  }
  async function discard() {
    return !dirty() || await confirmBox('Renunțați la codul fiscal nesalvat?',
      { title: 'Modificări nesalvate', yes: 'Renunț', no: 'Rămân aici', warning: true });
  }
  async function choose(row) {
    if (selected?.id === row.id) return true;
    if (busy || choosing) { if (selected) dataGrid.selectRow(selected.id); return false; }
    choosing = true;
    try {
      if (!await discard() || disposed) { if (!disposed && selected) dataGrid.selectRow(selected.id); return false; }
      show(row); say('');
      dataGrid.selectRow(row.id);
      return true;
    } finally { choosing = false; }
  }
  function actions(row) {
    rowMenu(dataGrid, row, [{ label: 'Sincronizează cu ANAF', disabled: busy || choosing,
      run: async () => {
        try { if (await choose(row) && !disposed) await run('anaf'); } catch (error) { report(error); }
      } }]);
  }
  async function load() {
    const answer = await opts.api(endpoint + '?' + query());
    if (disposed) return;
    const previous = selected && { id: selected.id, fiscal: selected.CodFiscal };
    rows = answer.rows;
    canUpdate = answer.can_update;
    canAdd = !!answer.can_add; addButton.element.hidden = !canAdd;
    if (!dataGrid) dataGrid = grid(list, { layoutId: 'vercon.partners', rowKey: 'id', editable: false,
      footerCaption: '{0} furnizori', emptyText: 'Nu există furnizori în filtrul ales.', layout: {}, proportionalWidths: true,
      defaultSort: { key: 'DenumirePartener', dir: 'asc' }, columns: [
        column({ key: 'DenumirePartener', label: 'Denumire' }, { editable: false, width: 280, minWidth: 0 }),
        column({ key: 'CodFiscal', label: 'Cod fiscal' }, { editable: false, width: 135, minWidth: 0 }),
        column({ key: 'CodPartener', label: 'Cod partener' }, { editable: false, width: 110, minWidth: 0 }),
        { key: 'actions', title: '⋮', width: 36, minWidth: 36, fixedWidth: true, editable: false,
          filter: false, sortable: false, display: 'button', actionText: '⋮' },
      ], onSelect: (row) => { void choose(row).catch(report); },
      onCellAction: ({ row, key }) => { if (key === 'actions') actions(row); } }, ui);
    show(rows.find((row) => previous && (previous.fiscal ? row.CodFiscal === previous.fiscal : row.id === previous.id)) || null); paintRows();
  }
  async function startAdd() {
    if (adding || busy || choosing || !canAdd) return;
    if (!await discard() || disposed) return;
    busy = true; root.inert = true; sync();
    let started = false;
    try {
      await opts.ensureWrite();
      if (disposed) return;
      const answer = await opts.api('/api/vercon/suppliers/next-code?' + query());
      if (disposed) return;
      adding = true; started = true; checked = null; lastLooked = ''; selected = null; missingCode = null;
      title.textContent = 'Furnizor nou';
      for (const box of controls.values()) { box.setValue(''); box.clearError(); }
      controls.get('CodPartener').setValue(answer.cod_partener);
      list.inert = true; searchHost.inert = true; list.classList.add('is-disabled');
      addBar.hidden = false;
    } catch (error) { report(error); }
    finally { busy = false; if (!disposed) { root.inert = false; sync(); if (started) fiscal.input.focus(); } }
  }
  function leaveAdd() {
    adding = false; checked = null; lastLooked = ''; lookupSeq++; lookingUp = false;
    list.inert = false; searchHost.inert = false; list.classList.remove('is-disabled'); addBar.hidden = true;
    lock(false);
    show(null);
  }
  // The boxes are disabled while the server works (ANAF or the save); no message is written for adding, looking up or saving: a refusal is the error state of the fiscal code
  const lock = (on) => { for (const box of controls.values()) box.setEnabled(!on); };
  async function lookupNew() {
    const code = fiscal.text.trim();
    if (!adding || !code || code === lastLooked) return;
    const sequence = ++lookupSeq;
    lastLooked = code; checked = null; lookingUp = true; fill('', ''); fiscal.clearError(); lock(true); sync();
    try {
      const result = await opts.api('/api/vercon/suppliers/check', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: body({ cod_fiscal: code }) });
      if (disposed || !adding || sequence !== lookupSeq) return;
      if (result.exists) {
        fill(result.name, result.address);
        if (result.hidden) checked = result.cod_fiscal;   // hidden supplier: the save makes it visible again
        else fiscal.setError(`Furnizorul există deja: ${result.name}.`);
      } else if (!result.found) {
        fiscal.setError('ANAF nu cunoaște acest cod fiscal.');
      } else {
        fill(result.name, result.address); checked = result.cod_fiscal;
      }
    } catch (error) { if (sequence === lookupSeq) { lastLooked = ''; if (!disposed && adding) fiscal.setError(error.message || 'Interogarea a eșuat.'); } }
    finally { if (sequence === lookupSeq) { lookingUp = false; if (!disposed) { lock(false); sync(); } } }
  }
  async function saveAdd() {
    if (!adding || !checked || busy || lookingUp) return;
    busy = true; root.inert = true; lock(true); sync();
    try {
      await opts.ensureWrite();
      if (disposed) return;
      const result = await opts.api('/api/vercon/suppliers/create', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: body({ cod_fiscal: checked }) });
      if (disposed) return;
      leaveAdd();
      if (result.row) { rows = rows.filter((row) => row.id !== result.row.id).concat(result.row); show(result.row); paintRows(); } else await load();
    } catch (error) { if (!disposed && adding) fiscal.setError(error.message || 'Salvarea a eșuat.'); }
    finally { busy = false; if (!disposed) { root.inert = false; if (adding) lock(false); sync(); } }
  }
  // Enter goes to the next box in the order of the form (a disabled one is skipped); after the last box it reaches «Salvează» while adding.
  // In the fiscal code, Enter while adding first runs the ANAF lookup, then goes to «Salvează» (or stays in the code when the lookup refused it).
  function focusNext(key) {
    const keys = [...controls.keys()];
    for (let index = keys.indexOf(key) + 1; index < keys.length; index++) {
      const box = controls.get(keys[index]);
      if (!box.input.disabled) { box.input.focus(); return; }
    }
    if (adding && !saveNew.element.disabled) saveNew.element.focus();
  }
  for (const [key, box] of controls) {
    box.input.addEventListener('keydown', async (event) => {
      if (event.key !== 'Enter' || event.shiftKey || event.ctrlKey || event.altKey) return;
      event.preventDefault();
      if (key === 'CodFiscal' && adding) {
        try { await lookupNew(); } catch (error) { report(error); }
        if (disposed || !adding) return;
        if (!saveNew.element.disabled) saveNew.element.focus(); else fiscal.input.focus();
        return;
      }
      focusNext(key);
    });
  }
  fiscal.input.addEventListener('blur', () => { void lookupNew().catch(report); });
  async function run(mode) {
    if (busy || choosing || !canUpdate || !selected) return;
    const code = fiscal.text.trim();
    if (!code) { fiscal.setError('Completați codul fiscal.'); fiscal.input.focus(); return; }
    if (mode === 'fiscal' && !await confirmBox('ANAF nu a găsit codul fiscal ' + missingCode
      + '. Salvați numai CodulFiscal pentru «' + selected.DenumirePartener + '»?',
      { title: 'Cod fiscal negăsit în ANAF', yes: 'Salvează CodulFiscal', no: 'Renunță' })) return;
    if (disposed) return;
    busy = true; root.inert = true; sync(); say('Se interoghează ANAF…');
    try {
      await opts.ensureWrite();
      if (disposed) return;
      const selection = opts.sel();
      const result = await opts.api(endpoint + '/' + selected.id + '/anaf', { method: 'POST',
        headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ subunit: selection.subunit,
          gestiune: selection.gestiune, version: selected.version, cod_fiscal: code, mode }) });
      if (disposed) return;
      if (!result.row) {
        missingCode = result.cod_fiscal;
        say('Codul nu a fost găsit în ANAF. Puteți salva numai CodulFiscal.');
        return;
      }
      rows = rows.map((row) => row.id === result.row.id ? result.row : row);
      show(result.row); paintRows();
      say(mode === 'fiscal' ? 'CodulFiscal a fost salvat.' : 'Furnizorul a fost actualizat din ANAF.');
    } catch (error) { missingCode = null; report(error); }
    finally { busy = false; if (!disposed) { root.inert = false; sync(); } }
  }
  manual.addEventListener('click', () => { void run('fiscal').catch(report); });
  refresh.addEventListener('click', async () => {
    if (busy || choosing || !await discard() || disposed) return;
    busy = true; root.inert = true; sync();
    try { await load(); say('Lista a fost reîmprospătată.'); } catch (error) { report(error); }
    finally { busy = false; if (!disposed) { root.inert = false; sync(); } }
  });
  return { ready: load(), isDirty: () => busy || dirty() || (adding && !!fiscal.text.trim()), canChangeContext: () => !busy && !choosing,
    destroy: () => { disposed = true; closeMenu(); ui.clear(); host.replaceChildren(); } };
}
