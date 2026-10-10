import { confirmBox } from '../utils/confirm-box.js';
import { DataGrid, ValueType } from '../dgv/datagrid.js';
import { Combobox } from '../components/combobox/combobox.js';
import { DatePicker } from '../components/datepicker/datepicker.js';
import { showMessage } from '../portal/messages.js';
import { cnpMessage } from './cnp.js';

const $ = (id) => document.getElementById(id);
const column = (key, title, width = 160) => ({ key, title, width, valueType: ValueType.Text, filter: key === 'Nume' || key === 'Grupa' });
const closedFlag = (key) => ({ ...column(key, 'I', 72), valueType: ValueType.Boolean, display: 'checkbox', filter: false });
const day = (value) => value?.slice(0, 10) || '';

export function bindPayers({ api, context, refresh }) {
  const page = $('ade-payers-dialog');
  const editor = $('ade-record-dialog');
  const body = $('ade-record-body');
  const form = $('ade-record-form');
  const message = $('ade-record-message');
  const mobile = matchMedia('(max-width: 980px)');
  const listHosts = [$('ade-groups-list'), $('ade-children-list'), $('ade-payers-list')];
  let level = 0;
  let data = null;
  let selectedGroup = null;
  let selectedChild = null;
  let selectedPayer = null;
  let grids = [];
  let controls = [];
  let editGrid = null;
  let collect = null;
  let endpoint = null;
  let onSaved = null;
  let request = null;
  let busy = false;
  let pendingNext = null;   // what opens once the saving form is fully done (the payer form after a new child)
  let dirty = false;
  let sequence = 0;
  let listRequest = 0;
  const canEdit = () => context().permissions.includes('catalog');
  const report = (error, host = message) => { console.error('[ADE forms]', error); showMessage(host, error.message, 'error'); };
  const grid = (host, rows, columns, key, onSelect, options = {}) => {
    const instance = new DataGrid(host, {
      columns, rows, rowKey: key, onSelect, headerHeight: 32, mobileRowScale: 1.2, footer: true, footerCaption: '{0} înregistrări',
      theme: document.documentElement.dataset.theme === 'dark' ? 'dark' : 'modern', ...options });
    instance.sortBy(columns.find((item) => ['Nume', 'Grupa', 'Educator'].includes(item.key)).key, 'asc');
    return instance;
  };
  function syncMobilePage() {
    listHosts.forEach((host, index) => { host.parentElement.hidden = mobile.matches && index !== level; });
    $('ade-payers-title').textContent = mobile.matches ? ['Grupe', 'Copii', 'Plătitori'][level] : 'Plătitori';
    const label = level ? 'Înapoi' : 'Închide';
    $('ade-payers-back').setAttribute('aria-label', label);
    $('ade-payers-back').title = label;
  }
  function openLevel(row, index) {
    if (!mobile.matches) return;
    level = index + 1;
    (index === 0 ? chooseGroup(row) : chooseChild(row)).catch((error) => report(error, $('ade-payers-message')));
  }
  async function chooseGroup(row) {
    const own = ++listRequest;
    selectedGroup = row; selectedChild = null; selectedPayer = null;
    data.children = []; data.payers = []; renderLists();
    const result = await api(`/api/adechit/rows/Platitori?IDG=${row.IDG}`);
    if (own !== listRequest || !page.open) return;
    data.children = result.rows; renderLists();
  }
  async function chooseChild(row) {
    const own = ++listRequest;
    selectedChild = row; selectedPayer = null;
    data.payers = []; renderLists();
    const result = await api(`/api/adechit/rows/Platitori_sub?IDP=${row.IDP}`);
    if (own !== listRequest || !page.open) return;
    data.payers = result.rows; renderLists();
  }
  listHosts.forEach((host) => {
    host.addEventListener('contextmenu', (event) => { if (mobile.matches) event.preventDefault(); });
  });
  mobile.addEventListener('change', () => { if (page.open && data) renderLists(); });
  function cleanEditor() {
    editGrid?.destroy(); editGrid = null;
    controls.forEach((control) => control.destroy()); controls = [];
    body.replaceChildren(); collect = null; dirty = false;
  }
  function begin(title, path, callback, kind = 'compact') {
    if (busy) return false;
    cleanEditor(); request = null; endpoint = path; onSaved = callback;
    $('ade-record-title').textContent = title;
    editor.dataset.kind = kind;
    showMessage(message, '', 'info');
    editor.showModal();
    return true;
  }
  function field(key, label, value, type = 'text', parent = body) {
    const row = document.createElement('label'); row.className = 'ade-form-field';
    const caption = document.createElement('span'); caption.textContent = label;
    const input = document.createElement('input'); input.name = key; input.id = `ade-field-${key}`;
    input.type = type; input.setAttribute('aria-label', label);
    if (type === 'checkbox') input.checked = Boolean(value); else input.value = value ?? '';
    input.maxLength = context().schema[type === 'checkbox' ? 'Platitori' : 'Platitori_sub']?.fields[key]?.size || 255;
    row.append(caption, input); parent.append(row);
    if (type === 'date') {
      const picker = new DatePicker(input); controls.push(picker);
    }
    return input;
  }
  function read(key) {
    const input = form.elements.namedItem(key);
    return input.type === 'checkbox' ? input.checked : input.value.trim();
  }
  function validateDates() {
    for (const control of controls) if (control instanceof DatePicker) {
      control.commitText();
      if (control.field.classList.contains('is-invalid')) {
        control.field.focus(); throw new Error('Introduceți o dată validă, în format zz.ll.aaaa.');
      }
    }
  }
  function editChild(row = null, afterCreate = null) {
    if (!canEdit()) return;
    if (!row && !selectedGroup) { report(new Error('Selectați grupa copilului.'), $('ade-payers-message')); return; }
    if (!begin(row ? 'Modifică copilul' : 'Adaugă copil', '/api/adechit/child-save', (saved) => {
      selectedChild = saved; selectedGroup = data.groups.find((group) => group.IDG === saved.IDG) || selectedGroup;
      return afterCreate?.(saved);
    })) return;
    field('Nume', 'Nume copil', row?.Nume).maxLength = 255;
    field('CNP', 'CNP copil', row?.CNP).inputMode = 'numeric';
    const groupRow = document.createElement('div'); groupRow.className = 'ade-form-field';
    const label = document.createElement('span'); label.textContent = 'Grupa';
    const host = document.createElement('div'); groupRow.append(label, host); body.append(groupRow);
    const available = data.groups.filter((group) => !group.InchisaDinAn || group.IDG === row?.IDG);
    const combo = new Combobox(host, { readonly: true, staticData: available.map((group) => ({ value: String(group.IDG), label: group.Grupa })),
      onSelect: () => { dirty = true; } });
    combo.input.setAttribute('aria-label', 'Grupa'); controls.push(combo);
    const group = data.groups.find((item) => item.IDG === (row?.IDG ?? selectedGroup.IDG));
    combo.setValue(String(group.IDG), group.Grupa);
    field('DataIntrare', 'Data intrare', day(row?.DataIntrare), 'date');
    const exit = document.createElement('div'); exit.className = 'ade-form-pair'; body.append(exit);
    field('Plecat', 'Plecat', row?.Plecat, 'checkbox', exit);
    field('DataIesire', 'Data ieșire', day(row?.DataIesire), 'date', exit);
    collect = () => {
      validateDates();
      const values = { Nume: read('Nume'), CNP: read('CNP'), IDG: Number(combo.getSelectedValue()),
        DataIntrare: read('DataIntrare') || null, Plecat: read('Plecat'), DataIesire: read('DataIesire') || null };
      const cnp = cnpMessage(values.CNP); if (cnp) throw new Error(cnp);
      if (!values.Nume) throw new Error('Numele copilului este obligatoriu.');
      return { id: row?.IDP ?? null, version: row?.Version, values };
    };
    $('ade-field-CNP').addEventListener('blur', () => showMessage(message, cnpMessage(read('CNP')), cnpMessage(read('CNP')) ? 'error' : 'info'));
  }
  function editPayer(row = null) {
    if (!canEdit() || !selectedChild) return;
    if (!begin(row ? 'Modifică plătitorul' : 'Adaugă plătitor', '/api/adechit/catalog/Platitori_sub', (saved) => { selectedPayer = saved; })) return;
    for (const [key, label] of [['Nume', 'Nume plătitor'], ['Adresa', 'Adresă plătitor'], ['CNP_Platitor', 'CNP plătitor'],
      ['CUI', 'Cod fiscal'], ['Cont', 'Cont bancar'], ['Banca', 'Banca']]) field(key, label, row?.[key]);
    const contacts = document.createElement('div'); contacts.className = 'ade-form-pair'; body.append(contacts);
    field('Telefon', 'Telefon', row?.Telefon, 'tel', contacts); field('EMail', 'Email', row?.EMail, 'email', contacts);
    const sendCode = document.createElement('button'); sendCode.type = 'button'; sendCode.className = 'btn';
    sendCode.textContent = 'Trimite Codul de Acces'; body.append(sendCode);
    const updateSendCode = () => { sendCode.hidden = !read('EMail'); };
    $('ade-field-EMail').addEventListener('input', updateSendCode); updateSendCode();
    sendCode.addEventListener('click', async () => {
      if (busy) return;
      try {
        const payload = collect(); busy = true; body.inert = true;
        // Save newly entered contact data before delivery; a failed mail does not discard the saved parent.
        row = await api('/api/adechit/catalog/Platitori_sub', { method: 'POST', body: JSON.stringify(payload), key: crypto.randomUUID() });
        selectedPayer = row; dirty = false; request = null;
        const result = await api(`/api/adechit/parents/${row.IDS}/send-access`, { method: 'POST', body: '{}' });
        showMessage(message, result.message, 'info'); await reload(); await refresh();
      } catch (error) { report(error); }
      finally { busy = false; body.inert = false; }
    });
    field('Activ', 'Activ', row ? row.Activ : true, 'checkbox').disabled = !row;
    collect = () => {
      const values = Object.fromEntries(['Nume', 'Adresa', 'CNP_Platitor', 'CUI', 'Cont', 'Banca', 'Telefon', 'EMail', 'Activ'].map((key) => [key, read(key)]));
      values.IDP = selectedChild.IDP;
      if (!row) values.Activ = true;
      const cnp = cnpMessage(values.CNP_Platitor); if (cnp) throw new Error(cnp);
      if (!values.Nume) throw new Error('Numele plătitorului este obligatoriu.');
      return { id: row?.IDS ?? null, version: row?.Version, values };
    };
    $('ade-field-CNP_Platitor').addEventListener('blur', () => {
      const text = cnpMessage(read('CNP_Platitor')); showMessage(message, text, text ? 'error' : 'info');
    });
  }
  async function editGroup(row = null) {
    if (!canEdit()) return;
    const [yearData, educators] = await Promise.all([
      api('/api/adechit/years'), row ? api(`/api/adechit/rows/Grupe_Educator?IDG=${row.IDG}`) : Promise.resolve({ rows: [] }),
    ]);
    data.years = [...new Set([new Date().getFullYear(), ...yearData.years])].sort((a, b) => b - a);
    data.educators = educators.rows;
    if (!begin(row ? 'Modifică grupa' : 'Adaugă grupă', '/api/adechit/group-save', (saved) => { selectedGroup = saved; }, 'group')) return;
    field('Grupa', 'Nume grupă', row?.Grupa).maxLength = 50;
    const closed = document.createElement('div'); closed.className = 'ade-form-pair'; body.append(closed);
    field('Closed', 'Grupă închisă', Boolean(row?.InchisaDinAn), 'checkbox', closed);
    const workspace = document.createElement('div'); workspace.className = 'ade-group-editor';
    const yearsHost = document.createElement('div'); yearsHost.className = 'ade-years dgv';
    yearsHost.dataset.theme = document.documentElement.dataset.theme === 'dark' ? 'dark' : 'modern';
    const right = document.createElement('section'); const host = document.createElement('div'); host.className = 'ade-educator-grid';
    right.append(host); workspace.append(yearsHost, right); body.append(workspace);
    const periods = data.educators.filter((item) => item.IDG === row?.IDG).map((item) => ({ ...item, DeLa: day(item.DeLa), PanaLa: day(item.PanaLa) }));
    const drafts = new Map(); let year = null;
    const columns = [{ ...column('Educator', 'Educator', 230), editable: true },
      { ...column('DeLa', 'Începând cu', 160), editable: true, editor: 'date', valueType: ValueType.DateTime },
      { ...column('PanaLa', 'Până la', 160), editable: true, editor: 'date', nullable: true, valueType: ValueType.DateTime }];
    const addEducator = async () => {
      if (editGrid.hasEdit && !await editGrid.commitEdit()) return;
      const period = { IDGE: --sequence, Educator: '', DeLa: '', PanaLa: '' };
      const previousDirty = dirty;
      periods.push(period); drafts.set(period.IDGE, period); dirty = true; filter();
      editGrid.beginNewRowEdit(period, 'Educator', () => {
        periods.splice(periods.indexOf(period), 1); drafts.delete(period.IDGE);
        dirty = previousDirty; filter();
      });
    };
    editGrid = grid(host, periods, columns, 'IDGE', null, { editable: true, layout: { fill: 'Educator' },
      footerCaption: '', footerAction: { label: '+ Adaugă' }, onFooterAction: () => addEducator().catch(report),
      onCellSave: ({ row: period, key, value }) => {
        const saved = { ...period, [key]: value };
        Object.assign(periods.find((item) => item.IDGE === saved.IDGE), saved);
        drafts.set(saved.IDGE, saved); dirty = true; return saved;
      }, onEditError: ({ error }) => report(error) });
    const filter = () => editGrid.setRows(periods.filter((item) => year == null || item.IDGE < 0
      || (!item.DeLa || item.DeLa <= `${year}-12-31`) && (!item.PanaLa || item.PanaLa >= `${year}-01-01`)));
    const years = [...new Set([...data.years, ...periods.flatMap((item) => [item.DeLa, item.PanaLa].filter(Boolean).map((date) => Number(date.slice(0, 4))))])].sort((a, b) => b - a);
    const title = document.createElement('strong'); title.textContent = 'ANI'; yearsHost.append(title);
    const yearButton = (label, value) => {
      const button = document.createElement('button'); button.type = 'button'; button.textContent = label;
      button.setAttribute('aria-pressed', String(value === year)); yearsHost.append(button);
      button.addEventListener('click', async () => {
        if (editGrid.hasEdit && !await editGrid.commitEdit()) return;
        year = value; filter(); yearsHost.querySelectorAll('button').forEach((item) => item.setAttribute('aria-pressed', String(item === button)));
      });
    };
    yearButton('Toți anii', null); years.forEach((value) => yearButton(String(value), value));
    collect = () => {
      const values = { Grupa: read('Grupa'), InchisaDinAn: read('Closed') ? (row?.InchisaDinAn || new Date().getFullYear()) : null };
      if (!values.Grupa) throw new Error('Numele grupei este obligatoriu.');
      return { id: row?.IDG ?? null, version: row?.Version, values, educators: [...drafts.values()].map((item) => ({
        id: item.IDGE < 0 ? null : item.IDGE, version: item.Version,
        values: { Educator: item.Educator, DeLa: item.DeLa || null, PanaLa: item.PanaLa || null } })) };
    };
  }
  async function saveHiddenGroup({ row, value }) {
    const payload = { id: row.IDG, version: row.Version, hidden: value };
    let saved;
    try {
      saved = await api('/api/adechit/group-hidden', { method: 'POST', body: JSON.stringify(payload) });
    } catch (error) {
      if (error.reason !== 'GROUP_ACTIVE_CHILDREN') throw error;
      if (!await confirmBox(`${error.message}\nAscundeți grupa „${row.Grupa}”?`, { title: 'Ascundere grupă', yes: 'Ascunde', no: 'Renunță' })) return row;
      saved = await api('/api/adechit/group-hidden', { method: 'POST', body: JSON.stringify({ ...payload, confirm_active: true }) });
    }
    Object.assign(data.groups.find((group) => group.IDG === saved.IDG), saved);
    return saved;
  }
  function renderLists() {
    syncMobilePage();
    grids.forEach((instance) => instance.destroy()); grids = [];
    const groupRows = data.groups.map((row) => ({ ...row, Closed: Boolean(row.InchisaDinAn) }));
    if (selectedGroup) selectedGroup = groupRows.find((row) => row.IDG === selectedGroup.IDG) || null;
    const children = selectedGroup ? data.children.filter((row) => row.IDG === selectedGroup.IDG) : [];
    if (selectedChild) selectedChild = children.find((row) => row.IDP === selectedChild.IDP) || null;
    const payers = selectedChild ? data.payers.filter((row) => row.IDP === selectedChild.IDP)
      .map((row) => ({ ...row, Closed: !Boolean(row.Activ) })) : [];
    if (selectedPayer) selectedPayer = payers.find((row) => row.IDS === selectedPayer.IDS) || null;
    const mobileColumns = (columns, weights, opens = false) => {
      const result = columns.map((item, index) => ({ ...item, width: weights[index], minWidth: 0,
        widthDeduction: opens && index === 0 ? 40 : 0 }));
      if (opens) result.push({ key: 'Open', title: '', width: 40, fixedWidth: true, minWidth: 0,
        display: 'button', actionText: '➡️', actionLabel: 'Deschide', filter: false, sortable: false });
      return result;
    };
    const hiddenFlag = { ...closedFlag('Ascunsa'), title: 'A', editable: canEdit(), editor: 'checkbox' };
    const groupColumns = mobile.matches
      ? mobileColumns([column('Grupa', 'Grupa'), closedFlag('Closed'), hiddenFlag], [70, 15, 15], true)
      : [column('Grupa', 'Grupa'), column('Educators', 'Educatori', 210), closedFlag('Closed'), hiddenFlag];
    const childColumns = [column('Nume', 'Copil', 210), column('CNP', 'CNP copil', 140), closedFlag('Plecat')];
    const payerColumns = [column('Nume', 'Plătitor', 210), column('CNP_Platitor', 'CNP plătitor', 140), closedFlag('Closed')];
    const listOptions = (key) => mobile.matches ? { proportionalWidths: true, autoSize: false } : { layout: { fill: key } };
    grids.push(grid($('ade-groups-list'), groupRows, groupColumns, 'IDG', (row) => {
      chooseGroup(row).catch((error) => report(error, $('ade-payers-message')));
    }, { ...listOptions('Grupa'), onCellAction: ({ row }) => openLevel(row, 0), editable: canEdit(),
      onCellSave: saveHiddenGroup, onCellSaved: () => refresh().catch((error) => report(error, $('ade-payers-message'))) }));
    grids.push(grid($('ade-children-list'), children, mobile.matches ? mobileColumns(childColumns, [50, 35, 15], true) : childColumns, 'IDP', (row) => {
      chooseChild(row).catch((error) => report(error, $('ade-payers-message')));
    }, { ...listOptions('Nume'), onCellAction: ({ row }) => openLevel(row, 1) }));
    grids.push(grid($('ade-payers-list'), payers, mobile.matches ? mobileColumns(payerColumns, [50, 35, 15]) : payerColumns, 'IDS', (row) => {
      selectedPayer = row; updateButtons();
    }, listOptions('Nume')));
    // Restore visible selection using the common grid selection behavior.
    [selectedGroup, selectedChild, selectedPayer].forEach((row, index) => {
      if (row) grids[index].selectRow(row[['IDG', 'IDP', 'IDS'][index]]);
    });
    updateButtons();
  }
  function updateButtons() {
    $('ade-group-add').disabled = !canEdit(); $('ade-group-edit').disabled = !canEdit() || !selectedGroup;
    $('ade-child-add').disabled = !canEdit() || !selectedGroup; $('ade-child-edit').disabled = !canEdit() || !selectedChild;
    $('ade-payer-add').disabled = !canEdit() || !selectedChild; $('ade-payer-edit').disabled = !canEdit() || !selectedPayer;
    const kind = ['group', 'child', 'payer'][level];
    for (const action of ['add', 'edit']) {
      const button = $(`ade-payers-mobile-${action}`);
      button.disabled = $(`ade-${kind}-${action}`).disabled;
      const label = `${action === 'add' ? 'Adaugă' : 'Modifică'} ${['grupă', 'copil', 'plătitor'][level]}`;
      button.setAttribute('aria-label', label); button.title = label;
    }
  }
  async function reload() {
    const own = ++listRequest;
    const result = await api('/api/adechit/catalog-data?include_hidden=1');
    if (own !== listRequest || !page.open) return;
    const next = { ...result, children: [], payers: [], educators: [], years: [] };
    next.groups.sort((a, b) => String(a.Grupa || '').localeCompare(String(b.Grupa || ''), 'ro', { sensitivity: 'base' }));
    const groupId = selectedGroup?.IDG; const childId = selectedChild?.IDP;
    if (groupId != null && next.groups.some((row) => row.IDG === groupId)) {
      next.children = (await api(`/api/adechit/rows/Platitori?IDG=${groupId}`)).rows;
      if (childId != null && next.children.some((row) => row.IDP === childId))
        next.payers = (await api(`/api/adechit/rows/Platitori_sub?IDP=${childId}`)).rows;
    }
    if (own === listRequest && page.open) { data = next; renderLists(); }
  }
  async function open() {
    if (page.open || !context()) return;
    selectedGroup = null; selectedChild = null; selectedPayer = null; data = null;
    listHosts.forEach((host) => host.replaceChildren());
    level = 0; syncMobilePage();
    page.showModal(); showMessage($('ade-payers-message'), '', 'info');
    $('ade-payers-mobile-add').disabled = true; $('ade-payers-mobile-edit').disabled = true;
    document.querySelectorAll('#ade-payers-dialog .ade-list-actions button').forEach((button) => { button.disabled = true; });
    try { await reload(); }
    catch (error) { report(error, $('ade-payers-message')); }
  }
  form.addEventListener('input', () => { dirty = true; });
  form.addEventListener('change', () => { dirty = true; });
  form.addEventListener('submit', async (event) => {
    event.preventDefault(); if (busy || !collect) return;
    try {
      if (editGrid?.hasEdit && !await editGrid.commitEdit()) return;
      const values = collect(); const json = JSON.stringify(values);
      if (!request || request.body !== json) request = { body: json, key: crypto.randomUUID() };
      busy = true; $('ade-record-save').disabled = true; $('ade-record-cancel').disabled = true; body.inert = true;
      const saved = await api(endpoint, { method: 'POST', ...request });
      const continuation = onSaved(saved); editor.close(); cleanEditor();
      await continuation;
      await reload(); await refresh();
      showMessage($('ade-payers-message'), '', 'info');
    } catch (error) { pendingNext = null; report(error, editor.open ? message : $('ade-payers-message')); }
    finally {
      busy = false; body.inert = false; $('ade-record-save').disabled = false; $('ade-record-cancel').disabled = false;
      if (pendingNext) { const next = pendingNext; pendingNext = null; next(); }
    }
  });
  $('ade-record-cancel').addEventListener('click', () => { if (!busy) { editor.close(); cleanEditor(); } });
  [page, editor].forEach((dialog) => dialog.addEventListener('cancel', (event) => event.preventDefault()));
  $('ade-payers-close').addEventListener('click', () => {
    if (editor.open || busy) return; ++listRequest; page.close(); grids.forEach((instance) => instance.destroy()); grids = [];
  });
  $('ade-payers-back').addEventListener('click', () => {
    if (editor.open || busy) return;
    if (level) { level -= 1; renderLists(); }
    else $('ade-payers-close').click();
  });
  for (const action of ['add', 'edit']) $('ade-payers-mobile-' + action).addEventListener('click', () => {
    $(`ade-${['group', 'child', 'payer'][level]}-${action}`).click();
  });
  $('ade-payers').addEventListener('click', open);
  $('ade-group-add').addEventListener('click', () => editGroup().catch((error) => report(error, $('ade-payers-message'))));
  $('ade-group-edit').addEventListener('click', () => { if (selectedGroup) editGroup(selectedGroup).catch((error) => report(error, $('ade-payers-message'))); });
  $('ade-child-add').addEventListener('click', () => editChild());
  $('ade-child-edit').addEventListener('click', () => { if (selectedChild) editChild(selectedChild); });
  $('ade-payer-add').addEventListener('click', () => editPayer());
  $('ade-payer-edit').addEventListener('click', () => { if (selectedPayer) editPayer(selectedPayer); });
  addEventListener('beforeunload', (event) => { if (editor.open && dirty) { event.preventDefault(); event.returnValue = ''; } });
  addEventListener('pagehide', () => { cleanEditor(); grids.forEach((instance) => instance.destroy()); }, { once: true });
  // The grid's "+": only the child form and then the payer form of the new child (no groups, no lists).
  return { addChildWithPayer: async (groupId) => {
    if (groupId == null) throw new Error('Selectați grupa copilului.');
    if (page.open || editor.open || busy) return;
    const result = await api('/api/adechit/catalog-data?include_hidden=1');
    data = { ...result, children: [], payers: [], educators: [], years: [] };
    selectedGroup = data.groups.find((group) => group.IDG === groupId) || null;
    selectedChild = null; selectedPayer = null;
    // the payer form opens once the child form is saved and closed (the same dialog element serves both)
    editChild(null, () => { pendingNext = () => editPayer(); });
  } };
}
