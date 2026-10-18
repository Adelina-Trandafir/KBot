// Period / report type / data selection, with the existing VERCON controls and theme.
import { el, scope, field, paintTree, pdfPreview, showMessage, CheckBox } from './ui.js';
import { mountSelection, selectionFilters, selectionError } from './report-selection.js';
import { paintReportBlock, reportNodes } from './report-result.js';
import { downloadFile } from './pdf-preview.js';
import { format as formatField } from './fields.js';
import { reportPeriod } from './report-period.js';

export function mountReports(host, { api, sel, reportName = null }) {
  const owner = scope(), selectionOwner = scope(), tablesOwner = scope();
  owner.own(() => selectionOwner.clear()); owner.own(() => tablesOwner.clear());
  const year = Number(sel().year);
  const dateOnly = reportName === 'Amortizare';
  const state = { start: `${year}-01-01`, end: `${year}-12-31`, name: null, label: '', spec: null,
    checked: [], documents: [], model: null, seq: 0, lookupSeq: 0, destroyed: false, loading: false, selectionLoading: false, busy: false };
  let selectionTree;
  const root = el('section', { className: 'vc-reports' });
  const left = el('aside', { className: 'vc-reports__period' }, el('h4', { textContent: dateOnly ? 'Selectează data' : 'Selectează perioada' }));
  const periods = reportPeriod(owner, (start, finish) => {
    state.start = dateOnly ? finish : start; state.end = finish;
    begin.input._picker.setValue(new Date(state.start + 'T12:00:00'), false);
    end.input._picker.setValue(new Date(state.end + 'T12:00:00'), false);
    invalidate(); void lookups();
  });
  const begin = field({ key: 'start', label: 'Dată început', type: 'date', required: true }, state.start, true,
    value => { state.start = value; periods.sync(state.start, state.end); invalidate(); void lookups(); }, owner);
  const end = field({ key: 'end', label: dateOnly ? 'La data' : 'Dată sfârșit', type: 'date', required: true }, state.end, true,
    value => {
      state.end = value;
      if (dateOnly) { state.start = value; begin.input._picker.setValue(value ? new Date(value + 'T12:00:00') : null, false); }
      periods.sync(state.start, state.end); invalidate(); void lookups();
    }, owner);
  periods.node.hidden = begin.node.hidden = dateOnly;
  const options = el('section', { className: 'vc-reports__options' }, el('h4', { textContent: 'Opțiuni afișare' }));
  const showOutput = owner.own(new CheckBox(options, { label: 'Afișează rezultatul în pagină', className: 'vc-reports__check' })).input;
  left.append(periods.node, begin.node, end.node, options);
  const center = el('section', { className: 'vc-reports__types' });
  const right = el('section', { className: 'vc-reports__selection' }, el('h4', { textContent: 'Selectare date' }),
    el('p', { className: 'vc-hint', textContent: 'Alegeți mai întâi tipul raportului.' }));
  const status = el('p', { role: 'status', className: 'vc-reports__status' });
  const excel = el('button', { type: 'button', textContent: 'Export Excel', disabled: true });
  const list = el('button', { type: 'button', textContent: 'Listare', disabled: true, className: 'is-primary' });
  const actions = el('div', { className: 'vc-actions vc-reports__actions' }, status, excel, list);
  const output = el('section', { className: 'vc-reports__output', hidden: true }), notes = el('div');
  root.append(left, center, right, actions, notes, output); host.replaceChildren(root);

  const query = (withFilters = true) => new URLSearchParams({ ...sel(), start: state.start, end: state.end,
    ...(withFilters ? selectionFilters(state.checked) : {}) });
  function available() {
    const error = periodError() || selectionError(state.spec, state.checked);
    const disabled = !state.name || state.loading || state.selectionLoading || state.busy || !!error;
    excel.disabled = list.disabled = disabled;
    if ((state.spec || periodError()) && !state.loading && !state.busy && error) status.textContent = error;
  }
  function periodError() {
    if (!state.start || !state.end) return 'Completați data de început și data de sfârșit.';
    return state.start > state.end ? 'Data de început trebuie să fie înaintea datei de sfârșit.' : '';
  }
  function invalidate() {
    ++state.seq; state.model = null;
    tablesOwner.clear(); output.replaceChildren(); notes.replaceChildren();
    status.textContent = 'Alegeți datele, apoi Listare sau Export Excel.'; available();
  }
  function paintOutput() {
    tablesOwner.clear(); output.replaceChildren(); output.hidden = !showOutput.checked;
    if (showOutput.checked && state.model) state.model.blocks.forEach(block => paintReportBlock(block, output, tablesOwner));
  }
  showOutput.addEventListener('change', paintOutput);
  async function lookups() {
    if (!state.name || state.destroyed) return;
    if (periodError()) { ++state.lookupSeq; state.loading = false; available(); return; }
    const name = state.name, seq = ++state.lookupSeq;
    state.loading = true; state.selectionLoading = false; state.spec = null; state.documents = []; available(); status.textContent = 'Se încarcă datele de selecție…';
    // A failed lookup must not leave the previous report's selectable data on screen.
    selectionOwner.clear(); right.replaceChildren(el('h4', { textContent: 'Selectare date' })); selectionTree = null;
    try {
      const endpoint = `/api/vercon/reports/${encodeURIComponent(name)}/selection`;
      const base = query(false);
      const initial = new URLSearchParams(base); initial.set('branch', 'root');
      const response = await api(`${endpoint}?${initial}`);
      if (state.destroyed || seq !== state.lookupSeq || name !== state.name) return;
      state.spec = response.spec;
      state.documents = response.documents || [];
      selectionTree = mountSelection(right, response, state.checked, ids => {
        state.checked = ids; invalidate();
      }, selectionOwner, {
        loadBranch: (branch, recursive, signal) => {
          const params = new URLSearchParams(base); params.set('branch', branch);
          if (recursive) params.set('recursive', '1');
          return api(`${endpoint}?${params}`, { signal });
        },
        onDocuments: documents => {
          if (!state.destroyed && seq === state.lookupSeq) state.documents = documents;
        },
        onBusy: value => {
          if (!state.destroyed && seq === state.lookupSeq) { state.selectionLoading = value; available(); }
        },
      });
      await selectionTree.ready;
      if (state.destroyed || seq !== state.lookupSeq || name !== state.name) return;
      state.checked = selectionTree.getCheckedLeaves();
      status.textContent = 'Alegeți datele, apoi Listare sau Export Excel.';
    } catch (error) {
      if (!state.destroyed && seq === state.lookupSeq) { state.spec = null; showMessage(status, error.message, 'error'); }
    } finally {
      if (!state.destroyed && seq === state.lookupSeq) { state.loading = false; available(); }
    }
  }
  async function generate(seq) {
    if (state.model) return state.model;
    const model = await api(`/api/vercon/reports/${encodeURIComponent(state.name)}/generate?${query()}`);
    if (state.destroyed || seq !== state.seq) return null;
    state.model = model; paintOutput();
    notes.replaceChildren(...model.notes.map(note => el('p', { className: 'vc-hint', textContent: note })));
    return model;
  }
  function previewDetails() {
    const period = { start: state.spec?.kind === 'depreciation' ? state.end : state.start, end: state.end };
    if (!state.spec.filters.includes('document')) return { period };
    const selected = state.checked.filter(id => id.startsWith('document:')).map(id => id.slice('document:'.length));
    const documents = state.documents.filter(item => selected.includes(String(item.id)));
    if (documents.length === 1) return { documentLabel: documents[0].label.replace(/\b\d{4}-\d{2}-\d{2}\b/g, date => formatField(date, { type: 'date' })) };
    return { documentLabel: selected.length ? `${selected.length} documente` : '', period };
  }
  async function exportReport(format) {
    if (state.busy || state.loading || state.selectionLoading || !state.name || periodError() || selectionError(state.spec, state.checked) || !await owner.commit()) return;
    if (state.destroyed || state.busy || state.loading || state.selectionLoading || periodError() || selectionError(state.spec, state.checked)) return;
    const seq = state.seq;
    state.busy = true; available(); status.textContent = 'Se pregătește raportul…';
    try {
      let model = await generate(seq);
      if (!model) return;
      let data;
      try {
        data = await api(`/api/vercon/reports/export/${model.snapshot}/${format}?${new URLSearchParams(sel())}`, { responseType: 'arrayBuffer' });
      } catch (error) {
        if (error.reason !== 'REPORT_EXPIRED' || state.destroyed || seq !== state.seq) throw error;
        state.model = null; model = await generate(seq); if (!model) return;
        data = await api(`/api/vercon/reports/export/${model.snapshot}/${format}?${new URLSearchParams(sel())}`, { responseType: 'arrayBuffer' });
      }
      if (state.destroyed || seq !== state.seq) return;
      if (format === 'pdf') await pdfPreview(model.title, data, owner, () => !state.destroyed && seq === state.seq, previewDetails());
      else downloadFile(data, 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet', model.filename + '.xlsx');
      if (!state.destroyed && seq === state.seq) status.textContent = `${state.label}: raport pregătit.`;
    } catch (error) {
      if (!state.destroyed && seq === state.seq) showMessage(status, error.message, 'error');
    } finally { if (!state.destroyed) { state.busy = false; available(); } }
  }
  list.addEventListener('click', () => void exportReport('pdf'));
  excel.addEventListener('click', () => void exportReport('xlsx'));
  const ready = (async () => {
    try {
      const data = await api(`/api/vercon/reports/catalog?${new URLSearchParams(sel())}`);
      if (state.destroyed) return;
      periods.configure(data, begin, end);
      const reports = reportName ? data.reports.filter(item => item.id === reportName) : data.reports;
      const selectReport = name => {
        ++state.lookupSeq; state.name = name; state.label = reports.find(item => item.id === name)?.label || name;
        state.checked = []; state.spec = null; invalidate(); return lookups();
      };
      const nodes = reportNodes(reports);
      center._vcExpanded = new Set(nodes.map(node => node.id));
      paintTree(center, nodes, { title: 'Tipul raportului', collapsed: true, onSelect: name => { void selectReport(name); } }, owner);
      if (reportName && reports.length) await selectReport(reportName);
      else status.textContent = 'Alegeți raportul și perioada.';
    } catch (error) { if (!state.destroyed) showMessage(status, error.message, 'error'); }
  })();
  return { ready, isDirty: () => false,
    destroy() { state.destroyed = true; ++state.seq; ++state.lookupSeq; owner.clear(); host.replaceChildren(); } };
}
