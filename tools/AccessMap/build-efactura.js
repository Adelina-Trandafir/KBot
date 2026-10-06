// Generates docs/access-map/efactura/*.md from graph.json (made by extract.js).
// usage: node build-efactura.js <graph.json> <RawExportDir> <outDir>
const fs = require('fs'), path = require('path');
const [GRAPH, RAW, OUT] = process.argv.slice(2);
const g = JSON.parse(fs.readFileSync(GRAPH, 'utf8'));
fs.mkdirSync(path.join(OUT, 'forms'), { recursive: true });

// ---------- canonical names ------------------------------------------------------
const canon = {};
for (const t of Object.keys(g)) { canon[t] = {}; for (const n of Object.keys(g[t])) canon[t][n.toLowerCase()] = n; }
const relName = n => canon.table[n.toLowerCase()] || canon.query[n.toLowerCase()] || n;
const relType = n => canon.table[n.toLowerCase()] ? 'table' : (canon.query[n.toLowerCase()] ? 'query' : '?');
const NOISE_REL = new Set(['a', 'z', 'tc', 'test', 'b', 'c']);
const goodRel = n => n.length > 2 && !NOISE_REL.has(n.toLowerCase());
const uniq = a => [...new Set(a)];
const fnKey = f => f.owner + '.' + f.proc;
const NOISE_FN = /^(Compare|ADD_TRACE|REM_TRACE|DebugPrint|Terminate|Count|Item|Enabled|Nodes|parent|Initialize|Name|Test|Width|Height|Left|Refresh|Body|Data|EndClass|Checked|Level)$/i;

// ---------- scope ----------------------------------------------------------------
const seedForm = /^(EFACTURA_|EF_|Note_EFactura|MF_PDF_EF|Note_PDF_EF|_?MF2019_Asoc_EF|Mesaj_ANAF)/i;
const S = { form: new Set(Object.keys(g.form).filter(n => seedForm.test(n))), module: new Set(['mdl_EFactura', 'mdl_EFactura_Add']), class: new Set(['clsEF_element', 'clsAnaf_Async']) };
const efTables = /^(EF|EFS|EFT|EFT_C|EFT_M|EFT_O|EF_\w+|TEF_\w+|tmpEF\w*|TrecereEFactura|tmpMF_EFactura|tmpNote_EFactura|tmpNoteFacturi|ClientiEF|Factura|FacturaC|tmpFacturaC|tblMSG)$/i;
S.table = new Set(Object.keys(g.table).filter(n => efTables.test(n)));
S.query = new Set(Object.keys(g.query).filter(n => /EF|Factura/i.test(n)));
const coreTables = new Set(S.table);
// objects referenced from scope code
function refsOf(t, n) { return g[t][n].refs; }
let changed = true;
while (changed) {
  changed = false;
  for (const t of ['form', 'module', 'class', 'query']) for (const n of [...S[t]]) {
    const r = refsOf(t, n);
    for (const k of ['rel', 'recordsource', 'rowsource']) for (const x0 of r[k] || []) {
      const x = relName(x0); if (!goodRel(x)) continue; const tt = relType(x);
      if (tt === 'query' && !S.query.has(x)) { if (t === 'query' || t === 'form' || t === 'module' || t === 'class') { S.query.add(x); changed = true; } }
      if (tt === 'table' && !S.table.has(x)) { S.table.add(x); changed = true; }
    }
  }
}
const sharedTables = [...S.table].filter(n => !coreTables.has(n)).sort();
const sharedForms = new Set();
for (const t of ['form', 'module', 'class', 'query']) for (const n of S[t]) {
  for (const f of refsOf(t, n).forms || []) { const c = canon.form[f.toLowerCase()]; if (c && !S.form.has(c)) sharedForms.add(c); }
  for (const s of refsOf(t, n).subforms || []) if (!S.form.has(s.form)) sharedForms.add(s.form);
}

// ---------- reverse indexes (whole app) --------------------------------------------
const usedBy = {}; // key type:name -> Set of 'type:name'
function addUse(k, by) { (usedBy[k] ||= new Set()).add(by); }
for (const t of Object.keys(g)) for (const [n, o] of Object.entries(g[t])) {
  const by = t + ':' + n; const r = o.refs;
  for (const k of ['rel', 'recordsource', 'rowsource']) for (const x of r[k] || []) { const c = relName(x); if (goodRel(c)) addUse(relType(c) + ':' + c, by); }
  for (const f of r.forms || []) { const c = canon.form[f.toLowerCase()]; if (c) addUse('form:' + c, by); }
  for (const s of r.subforms || []) addUse('form:' + s.form, by);
  for (const f of r.fns || []) addUse('module:' + f.owner + '.' + f.proc, by);
}
const outside = k => [...(usedBy[k] || [])].filter(x => { const [t, n] = [x.split(':')[0], x.slice(x.indexOf(':') + 1)]; return !S[t] || !S[t].has(n); });

// ---------- helpers -----------------------------------------------------------------
const md = [];
const fileLink = (o) => '`' + o.file + '`';
function tbl(head, rows) { return ['| ' + head.join(' | ') + ' |', '|' + head.map(() => '---').join('|') + '|', ...rows.map(r => '| ' + r.map(c => String(c ?? '').replace(/\|/g, '\\|').replace(/\r?\n/g, ' ')).join(' | ') + ' |')].join('\n'); }
const code = (s, lang = '') => '```' + lang + '\n' + s.trim() + '\n```';
const w = (f, text) => fs.writeFileSync(path.join(OUT, f), text.replace(/\n{3,}/g, '\n\n'), 'utf8');
const nameList = (a, max = 999) => { a = uniq(a); return a.slice(0, max).map(x => '`' + x + '`').join(', ') + (a.length > max ? ` (+${a.length - max})` : ''); };
const EVENT_NAME = { OnClick: 'Click', OnDblClick: 'DblClick', OnLoad: 'Load', OnClose: 'Close', OnOpen: 'Open', OnCurrent: 'Current', OnChange: 'Change', OnEnter: 'Enter', OnExit: 'Exit', OnGotFocus: 'GotFocus', OnLostFocus: 'LostFocus', OnKeyDown: 'KeyDown', OnKeyUp: 'KeyUp', OnKeyPress: 'KeyPress', OnMouseDown: 'MouseDown', OnMouseUp: 'MouseUp', OnMouseMove: 'MouseMove', OnResize: 'Resize', OnTimer: 'Timer', OnUnload: 'Unload', OnActivate: 'Activate', OnDeactivate: 'Deactivate', OnNotInList: 'NotInList', OnDirty: 'Dirty', OnNoData: 'NoData', OnFormat: 'Format', OnPrint: 'Print', OnRetreat: 'Retreat', AfterUpdate: 'AfterUpdate', BeforeUpdate: 'BeforeUpdate', AfterInsert: 'AfterInsert', BeforeInsert: 'BeforeInsert', OnDelete: 'Delete', OnApplyFilter: 'ApplyFilter', OnFilter: 'Filter', OnError: 'Error', OnUndo: 'Undo', OnMouseWheel: 'MouseWheel', OnPage: 'Page' };
const TYPE_LABEL = { TextBox: 'TextBox', Label: 'Label', CommandButton: 'Button', ComboBox: 'ComboBox', ListBox: 'ListBox', CheckBox: 'CheckBox', OptionButton: 'OptionButton', OptionGroup: 'OptionGroup', ToggleButton: 'ToggleButton', Subform: 'Subform', Rectangle: 'Rectangle', Line: 'Line', Image: 'Image', TabCtl: 'TabControl', Page: 'TabPage', CustomControl: 'ActiveX', ObjectFrame: 'OleFrame', BoundObjectFrame: 'OleFrame(bound)', PageBreak: 'PageBreak', EmptyCell: 'EmptyCell', WebBrowser: 'WebBrowser', NavigationControl: 'NavControl', Attachment: 'Attachment', Chart: 'Chart' };

// ---------- per form pages --------------------------------------------------------------
const twips = v => (v === undefined || v === '' ? '' : v);
function geometry(c, defaults) {
  const p = c.props || {}, d = defaults[c.kind] || {};
  const L = p.LayoutCachedLeft ?? p.Left ?? '0', T = p.LayoutCachedTop ?? p.Top ?? '0';
  const W = p.LayoutCachedWidth !== undefined ? String(+p.LayoutCachedWidth - +L) : (p.Width ?? d.Width ?? '');
  const H = p.LayoutCachedHeight !== undefined ? String(+p.LayoutCachedHeight - +T) : (p.Height ?? d.Height ?? '');
  return { L, T, W, H };
}
function declsOf(fo) {
  const raw = fs.readFileSync(path.join(RAW, fo.file), 'utf8'); const i = raw.search(/^CodeBehindForm\s*$/m);
  const lines = (i >= 0 ? raw.slice(i) : '').split(/\r?\n/).slice(1);
  const out = []; const we = {};
  for (const l of lines) {
    if (/^\s*(Public |Private |Friend )?(Static )?(Sub|Function|Property)\s/i.test(l)) break;
    if (l.trim() === '' || /^Option /i.test(l)) continue;
    out.push(l);
    const m = l.match(/WithEvents\s+(\w+)\s+As\s+(?:New\s+)?(\w+)/i); if (m) we[m[1].toLowerCase()] = m[2];
  }
  return { text: out.join('\n'), we };
}
function handlerRows(fo, fname) {
  const procs = fo.procs || []; const we = declsOf(fo).we;
  const bind = {}; // proc name lower -> 'Control.Event'
  for (const c of fo.controls) for (const [ev, v] of Object.entries(c.events || {})) if (v === '[Event Procedure]') bind[(c.name + '_' + (EVENT_NAME[ev] || ev.replace(/^On/, ''))).toLowerCase()] = c.name + '.' + ev;
  for (const [ev, v] of Object.entries(fo.formEvents || {})) if (v === '[Event Procedure]') bind[('Form_' + (EVENT_NAME[ev] || ev.replace(/^On/, ''))).toLowerCase()] = 'Form.' + ev;
  for (const sec of fo.sections || []) for (const [ev, v] of Object.entries(sec.events || {})) if (v === '[Event Procedure]') bind[((sec.props.Name || sec.kind) + '_' + (EVENT_NAME[ev] || ev.replace(/^On/, ''))).toLowerCase()] = (sec.props.Name || sec.kind) + '.' + ev;
  return procs.map(p => {
    const r = p.refs || {};
    const touches = [];
    const rel = uniq((r.rel || []).map(relName).filter(goodRel));
    const reads = rel.filter(x => !p.writes.map(relName).includes(x));
    return {
      name: p.name, kind: p.kind, scope: p.scope, line: p.line, lines: p.lines,
      bound: bind[p.name.toLowerCase()] || (() => { const k = p.name.split('_')[0].toLowerCase(); return we[k] && p.name.includes('_') ? 'WithEvents ' + p.name.split('_')[0] + ' (' + we[k] + ').' + p.name.split('_').slice(1).join('_') : ''; })(),
      opens: uniq([...(r.forms || []).map(f => canon.form[f.toLowerCase()] || f), ...(r.reports || [])]).filter(x => x !== fname),
      reads, writes: uniq(p.writes.map(relName)),
      fns: uniq((r.fns || []).filter(f => !NOISE_FN.test(f.proc)).map(fnKey)),
      classes: uniq(r.classes || []), local: p.callsLocal || [], macros: r.macros || [],
    };
  });
}
const formInfo = {}; // for navigation / matrix
for (const fname of [...S.form].sort()) {
  const fo = g.form[fname];
  const defaults = {};
  for (const c of fo.controls) if (!c.name && !c.section) defaults[c.kind] = c.props || {};
  const real = fo.controls.filter(c => c.name || c.section);
  const H = handlerRows(fo, fname);
  const props = fo.allFormProps || {};
  const subs = (fo.refs.subforms || []);
  formInfo[fname] = { fo, H, subs, real };
  const L = [];
  L.push(`# Form \`${fname}\``);
  L.push('');
  L.push(`Source: ${fileLink(fo)} (${fo.size.toLocaleString('en')} chars). Units are twips (1 inch = 1440). \`Parent\` is the tab page or group a control sits in, or - for a label - the control it is attached to. Positions are relative to the section that holds the control; \`LayoutCached*\` values are used when present.`);
  L.push('');
  L.push('## Form properties');
  L.push('');
  const keyProps = ['Caption', 'RecordSource', 'Filter', 'OrderBy', 'DefaultView', 'ViewsAllowed', 'PopUp', 'Modal', 'AutoCenter', 'BorderStyle', 'MinMaxButtons', 'ControlBox', 'NavigationButtons', 'RecordSelectors', 'DividingLines', 'ScrollBars', 'AllowAdditions', 'AllowEdits', 'AllowDeletions', 'DataEntry', 'Width', 'Left', 'Top', 'Right', 'Bottom', 'FitToScreen', 'AllowDatasheetView', 'AllowFormView', 'Cycle', 'Moveable', 'GridX', 'GridY', 'TabularCharSet', 'DatasheetFontName', 'DatasheetFontHeight'];
  L.push(tbl(['Property', 'Value'], keyProps.filter(k => props[k] !== undefined).map(k => [k, k === 'RecordSource' ? '`' + props[k] + '`' : props[k]])));
  L.push('');
  L.push(`Form events wired: ${Object.entries(fo.formEvents || {}).map(([k, v]) => `\`${k}\` = ${v === '[Event Procedure]' ? 'VBA' : '`' + v + '`'}`).join(', ') || 'none'}`);
  L.push('');
  L.push('## Sections');
  L.push('');
  L.push(tbl(['Section', 'Height', 'Visible', 'BackColor', 'Events'], (fo.sections || []).map(s => [s.props.Name || s.kind, s.props.Height ?? '', s.props.Visible ?? '', s.props.BackColor ?? s.props.BackThemeColorIndex ?? '', Object.entries(s.events || {}).map(([k, v]) => k).join(', ')])));
  L.push('');
  L.push('## Data');
  L.push('');
  const rs = props.RecordSource;
  if (rs) L.push('- RecordSource: ' + (/^\s*SELECT/i.test(rs) ? '\n' + code(rs, 'sql') : '`' + rs + '`'));
  const rsRel = uniq((fo.refs.recordsource || []).map(relName));
  if (rsRel.length) L.push('- Tables/queries behind the RecordSource: ' + nameList(rsRel));
  const rowRel = uniq((fo.refs.rowsource || []).map(relName));
  if (rowRel.length) L.push('- Combo/list sources: ' + nameList(rowRel));
  if (!rs && !rsRel.length && !rowRel.length) L.push('- Unbound form (data loaded/saved by code).');
  L.push('');
  if (subs.length) {
    L.push('## Subforms'); L.push('');
    L.push(tbl(['Control', 'Subform', 'Link child', 'Link master'], subs.map(s => [s.name, '`' + s.form + '`' + (S.form.has(s.form) ? '' : ' (outside scope)'), s.child, s.master])));
    L.push('');
  }
  L.push('## Controls');
  L.push('');
  const rows = real.map((c, i) => {
    const p = c.props || {}; const d = defaults[c.kind] || {}; const g2 = geometry(c, defaults);
    const evs = Object.entries(c.events || {}).map(([k, v]) => k + (v === '[Event Procedure]' ? '' : '=' + v)).join(', ');
    const bits = [];
    if (p.Visible === '0') bits.push('hidden'); if (p.Enabled === '0') bits.push('disabled'); if (p.Locked === '-1') bits.push('locked'); if (p.TabStop === '0') bits.push('no-tab');
    if (p.Format) bits.push('fmt=' + p.Format); if (p.InputMask) bits.push('mask=' + p.InputMask); if (p.DefaultValue) bits.push('def=' + p.DefaultValue); if (p.ValidationRule) bits.push('valid=' + p.ValidationRule);
    if (c.rowSourceType) bits.push('rowType=' + c.rowSourceType); if (p.ColumnCount) bits.push('cols=' + p.ColumnCount); if (p.ColumnWidths) bits.push('colW=' + p.ColumnWidths); if (p.BoundColumn) bits.push('bound=' + p.BoundColumn); if (p.LimitToList) bits.push('limit'); if (p.Tag) bits.push('tag=' + p.Tag);
    if (p.ClassName || p.OleClass || p.Class) bits.push('class=' + (p.Class || p.OleClass || p.ClassName));
    if (p.Picture) bits.push('picture=' + p.Picture); if (p.ControlTipText) bits.push('tip=' + p.ControlTipText);
    const font = (p.FontName || d.FontName || '') + (p.FontSize || d.FontSize ? ' ' + (p.FontSize || d.FontSize) : '') + ((p.FontWeight || d.FontWeight) >= 700 ? ' bold' : '');
    return [i + 1, TYPE_LABEL[c.kind] || c.kind, c.name, c.parent, c.section || '', `${g2.L},${g2.T} ${g2.W}x${g2.H}`, c.caption, c.controlSource, c.rowSource ? '`' + c.rowSource.slice(0, 160) + (c.rowSource.length > 160 ? '...' : '') + '`' : '', c.tabIndex ?? '', font, evs, bits.join('; ')];
  });
  L.push(tbl(['#', 'Type', 'Name', 'Parent', 'Section', 'Pos L,T WxH', 'Caption', 'ControlSource', 'RowSource', 'Tab', 'Font', 'Events', 'Other'], rows));
  L.push('');
  const dd = declsOf(fo);
  if (dd.text.trim()) { L.push('## Module-level declarations'); L.push(''); L.push('Form state kept in variables, constants, types and WithEvents helpers (the Access form class body before the first procedure).'); L.push(''); L.push(code(dd.text, 'vb')); L.push(''); }
  L.push('## Event handlers and what they touch');
  L.push('');
  L.push('Reads/writes come from SQL text and recordset calls found in the procedure body; they are a lead for reading the code, not a proof (dynamic SQL built from variables is missed).');
  L.push('');
  L.push(tbl(['Procedure', 'Bound to', 'Lines', 'Opens forms/reports', 'Reads', 'Writes', 'Calls (modules)', 'Classes', 'Local calls'],
    H.map(h => [h.name + (h.scope === 'Private' ? '' : ' (' + h.scope + ')'), h.bound, h.lines, nameList(h.opens), nameList(h.reads), nameList(h.writes), nameList(h.fns, 12), nameList(h.classes), nameList(h.local)])));
  L.push('');
  L.push('## Opened from');
  L.push('');
  const by = outside('form:' + fname).concat([...(usedBy['form:' + fname] || [])].filter(x => S[x.split(':')[0]] && S[x.split(':')[0]].has(x.slice(x.indexOf(':') + 1))));
  L.push(uniq(by).length ? nameList(uniq(by).filter(x => x !== 'form:' + fname)) : 'Nothing found by static scan (probably opened by name from a ribbon, a menu string or a variable).');
  L.push('');
  L.push(`Raw source: \`Surse/RawExport/${fo.file}\``);
  w('forms/' + fname.replace(/[\\/:*?"<>|]/g, '_') + '.md', L.join('\n'));
}

// ---------- forms.md index -----------------------------------------------------------------
{
  const L = ['# Forms in scope', '', 'One page per form in `forms/`. Role column is derived from the caption and the data it binds to; read the per-form page for the details.', ''];
  L.push(tbl(['Form', 'Caption', 'Bound to', 'Subforms', 'Opens', 'Controls', 'Procs'],
    [...S.form].sort().map(n => { const fi = formInfo[n]; const p = fi.fo.allFormProps || {}; return ['[`' + n + '`](forms/' + n + '.md)', p.Caption || '', p.RecordSource ? (/^\s*SELECT/i.test(p.RecordSource) ? nameList(fi.fo.refs.recordsource.map(relName)) + ' (SQL)' : '`' + p.RecordSource + '`') : 'unbound', nameList(fi.subs.map(s => s.form)), nameList(uniq(fi.H.flatMap(h => h.opens)).filter(x => x !== n)), fi.real.length, fi.H.length]; })));
  L.push('');
  L.push('## Forms outside the scope that these forms open'); L.push('');
  L.push(tbl(['Form', 'Opened by', 'Bound to'], [...sharedForms].sort().map(n => [`\`${n}\``, nameList(outside('form:' + n).length ? [...(usedBy['form:' + n] || [])].filter(x => S[x.split(':')[0]] && S[x.split(':')[0]].has(x.slice(x.indexOf(':') + 1))) : []), (g.form[n].formProps && g.form[n].formProps.RecordSource ? '`' + g.form[n].formProps.RecordSource.slice(0, 80) + '`' : '')])));
  w('forms.md', L.join('\n'));
}

// ---------- navigation.md -----------------------------------------------------------------------
{
  const L = ['# Navigation: who opens whom', ''];
  L.push('## Form graph'); L.push('');
  L.push('Solid arrow = opens (DoCmd.OpenForm / Forms! reference in code or query); thick = hosts as subform.'); L.push('');
  L.push('```mermaid\nflowchart LR');
  const id = n => n.replace(/[^A-Za-z0-9_]/g, '_');
  const edges = new Set();
  for (const n of S.form) {
    const fi = formInfo[n];
    for (const o of uniq(fi.H.flatMap(h => h.opens))) if (o !== n) edges.add(`${id(n)}["${n}"] --> ${id(o)}["${o}"]`);
    for (const s of fi.subs) edges.add(`${id(n)}["${n}"] ==> ${id(s.form)}["${s.form}"]`);
  }
  for (const e of edges) L.push('  ' + e);
  L.push('```'); L.push('');
  L.push('## Entry points: code outside the scope that opens these forms'); L.push('');
  const rowsE = [];
  for (const t of ['module', 'class', 'form']) for (const [n, o] of Object.entries(g[t])) {
    if (S[t] && S[t].has(n)) continue;
    for (const p of o.procs || []) { const r = p.refs || {}; const hits = uniq((r.forms || []).map(f => canon.form[f.toLowerCase()] || f).filter(f => S.form.has(f))); if (hits.length) rowsE.push([t + ':' + n, p.name, nameList(hits)]); }
  }
  L.push(tbl(['Object', 'Procedure', 'Opens'], rowsE));
  L.push('');
  L.push('## Entry points: queries that read these forms (parameters)'); L.push('');
  const rowsQ = [];
  for (const [n, o] of Object.entries(g.query)) { const hits = (o.refs.forms || []).map(f => canon.form[f.toLowerCase()]).filter(f => f && S.form.has(f)); if (hits.length) rowsQ.push([n, nameList(hits)]); }
  L.push(tbl(['Query', 'Reads form controls of'], rowsQ));
  w('navigation.md', L.join('\n'));
}

// ---------- data-model.md ---------------------------------------------------------------------------
function tableSection(n, full) {
  const o = g.table[n]; const L = [];
  L.push(`### \`${n}\``); L.push('');
  L.push(o.linked ? `Linked table. ${o.linked.replace(/^Linked table\.\s*/, '')}` : 'Local table in Avacont.accdb.');
  L.push('');
  if (full) { L.push(tbl(['#', 'Field', 'Type', 'Size', 'Required', 'Default', 'Validation'], o.fields.map(f => f.slice(0, 7)))); L.push(''); }
  else L.push('Fields: ' + o.fields.map(f => '`' + f[1] + '`').join(', ')); L.push('');
  if (o.indexes.length) { L.push('Indexes: ' + o.indexes.map(x => '`' + x + '`').join('; ')); L.push(''); }
  const rd = [], wr = [];
  for (const x of [...(usedBy['table:' + n] || [])]) { const [t, nm] = [x.split(':')[0], x.slice(x.indexOf(':') + 1)]; if (!S[t] || !S[t].has(nm)) continue; const writes = (t === 'query' ? g.query[nm].writes : (g[t][nm].procs || []).flatMap(p => p.writes)).map(relName); (writes.includes(n) ? wr : rd).push(x); }
  L.push('In scope - written by: ' + (wr.length ? nameList(wr) : 'nothing found'));
  L.push(''); L.push('In scope - read by: ' + (rd.length ? nameList(rd) : 'nothing found'));
  const out = outside('table:' + n); L.push(''); L.push(`Also used by ${out.length} object(s) outside the scope${out.length && out.length <= 8 ? ': ' + nameList(out) : ''}.`);
  L.push('');
  return L.join('\n');
}
{
  const L = ['# Data model', ''];
  L.push('Tables in scope. "Linked" means the table lives in another .accdb and Avacont.accdb only links to it - the passwords in the connect strings are masked. Relationships come from the Access relationship window (`relationships.md` in the export), plus the join keys used by the queries below.'); L.push('');
  const groups = {};
  for (const n of [...coreTables].sort()) { const k = g.table[n].linked ? g.table[n].linked.match(/DATABASE=([^;`]+)/i)?.[1] || 'linked' : 'Avacont.accdb (local)'; (groups[k] ||= []).push(n); }
  L.push('## Storage'); L.push('');
  L.push(tbl(['Database', 'Tables'], Object.entries(groups).map(([k, v]) => ['`' + k + '`', nameList(v)])));
  L.push('');
  L.push('Note: `Surse/RawExport/EF_ACCDB/` is the export of `EF_2025.accdb` (the e-invoice store, owned tables EF, EFS, EFT, EFT_C, EFT_M, EFT_O, Ver and 15 helper queries). Avacont.accdb sees the same tables through links to `ef_2026.accdb`.'); L.push('');
  L.push('## Relationships inside the e-invoice store'); L.push('');
  const rel = fs.readFileSync(path.join(RAW, 'EF_ACCDB', 'relationships.md'), 'utf8').split('\n').filter(l => l.startsWith('- '));
  L.push(rel.join('\n')); L.push('');
  L.push('Logical keys the code and queries rely on (no Access relationship defined):'); L.push('');
  L.push(tbl(['From', 'To', 'Meaning'], [
    ['EFT.CUI', 'Parteneri.CodFiscal', 'supplier/customer match for received invoices (QEF_NOI)'],
    ['EFT_O.IdOperatie', 'Oper.IdOperatie', 'accounting operation the invoice was booked on'],
    ['EFT_O.IdClsf', 'Clasificatii.IDClsf', 'budget classification of the booked amount'],
    ['EFT.IDEFT_REF', 'EFT.IDEFT', 'credit note / correction points to the original invoice'],
    ['EF.cui_unit', 'UNIT (CUI)', 'which unit (entity) the downloaded message belongs to'],
    ['Factura.IdClient', 'ClientiEF.IdClient', 'customer on an issued invoice'],
    ['FacturaC.IdFactura', 'Factura.IdFactura', 'lines of an issued invoice'],
  ]));
  L.push('');
  L.push('## Tables'); L.push('');
  for (const n of [...coreTables].sort()) L.push(tableSection(n, true));
  w('data-model.md', L.join('\n'));
  const L2 = ['# Shared tables used by the scope', '', 'These belong to the wider Avacont system (accounting, partners, classifications). The e-invoice screens read them; most are NOT part of what has to be rebuilt for e-invoice, but the columns the queries use must exist in K-BOT.', ''];
  for (const n of sharedTables) L2.push(tableSection(n, true));
  w('shared-tables.md', L2.join('\n'));
}

// ---------- queries.md ---------------------------------------------------------------------------------
{
  const L = ['# Queries in scope', '', 'Every query that is named like an e-invoice object, or is read by an e-invoice form / module, plus the queries those call. SQL is exported as-is from Access (Jet/ACE dialect). Functions such as `Concat_WS`, `ConcatRelated`, `REGEXP`, `Nz`, `IIf` are VBA functions called from SQL.', ''];
  const rows = [...S.query].sort().map(n => { const o = g.query[n]; return ['`' + n + '`', o.qtype, nameList(uniq((o.refs.rel || []).map(relName).filter(goodRel).filter(x => x !== n)), 8), nameList(o.writes.map(relName)), nameList(uniq((o.refs.fns || []).map(f => f.proc)), 6), nameList(outside('query:' + n).concat([...(usedBy['query:' + n] || [])].filter(x => S[x.split(':')[0]] && S[x.split(':')[0]].has(x.slice(x.indexOf(':') + 1)))), 8)]; });
  L.push(tbl(['Query', 'Type', 'Reads', 'Writes', 'VBA functions', 'Used by'], rows)); L.push('');
  L.push('## SQL'); L.push('');
  for (const n of [...S.query].sort()) {
    const o = g.query[n]; const sql = fs.readFileSync(path.join(RAW, o.file), 'utf8').replace(/^-- .*\r?\n/gm, '').trim();
    L.push(`### \`${n}\` (${o.qtype})`); L.push(''); L.push(code(sql, 'sql')); L.push('');
  }
  w('queries.md', L.join('\n'));
}

// ---------- code.md ----------------------------------------------------------------------------------------
{
  const L = ['# Code in scope', '', 'Procedures of the standard modules/classes that are specific to e-invoice, with what each touches. Line numbers are inside the exported `.txt` file.', ''];
  for (const [t, set] of [['module', S.module], ['class', S.class]]) for (const n of [...set].sort()) {
    const o = g[t][n];
    L.push(`## ${t} \`${n}\``); L.push(''); L.push(`Source: ${fileLink(o)} (${o.size.toLocaleString('en')} chars, ${o.procs.length} procedures)`); L.push('');
    L.push(tbl(['Line', 'Procedure', 'Scope', 'Length', 'Comment above', 'Tables/queries', 'Writes', 'Opens forms', 'Calls outside', 'Local calls'],
      o.procs.map(p => { const r = p.refs || {}; return [p.line, '`' + p.name + '`' + (p.kind.startsWith('Property') ? ' (' + p.kind + ')' : ''), p.scope, p.lines, p.header.replace(/^'+\s*/, ''), nameList(uniq((r.rel || []).map(relName).filter(goodRel)), 8), nameList(p.writes.map(relName)), nameList(r.forms || []), nameList(uniq((r.fns || []).filter(f => !NOISE_FN.test(f.proc)).map(fnKey)), 8), nameList(p.callsLocal)]; })));
    L.push('');
  }
  L.push('## Call tree starting from the UI entry points'); L.push('');
  L.push('Which module procedure is reached from which form handler (one level), taken from the per-form pages:'); L.push('');
  const rowsC = [];
  for (const fname of [...S.form].sort()) for (const h of formInfo[fname].H) { const fns = h.fns.filter(f => /^mdl_EFactura|^clsAnaf|^mdl_2025\.(Token|CALE)|Salvare_NoteContabile|TRECERE/i.test(f)); if (fns.length) rowsC.push(['`' + fname + '`', h.name, h.bound, nameList(fns)]); }
  L.push(tbl(['Form', 'Handler', 'Bound to', 'Calls'], rowsC));
  w('code.md', L.join('\n'));
}

// ---------- external-dependencies.md -----------------------------------------------------------------------------
{
  const L = ['# What the e-invoice scope needs from the rest of Avacont', '', 'Code and objects outside the scope that scope objects call. Everything in the table must either be rebuilt in K-BOT, replaced by a K-BOT service/control, or deliberately dropped. Noise (tracing, `Compare`, `Terminate`, ...) is filtered out.', ''];
  const usage = {};
  for (const t of ['form', 'module', 'class', 'query']) for (const n of S[t]) for (const f of g[t][n].refs.fns || []) { if (S.module.has(f.owner) || NOISE_FN.test(f.proc)) continue; const k = f.owner + '.' + f.proc; (usage[k] ||= new Set()).add(t + ':' + n); }
  const rows = Object.entries(usage).sort((a, b) => b[1].size - a[1].size).map(([k, s]) => { const [mod, proc] = k.split('.'); const m = g.module[mod]; const pr = m && m.procs.find(p => p.name.toLowerCase() === proc.toLowerCase()); return ['`' + k + '`', s.size, pr ? pr.sig.slice(0, 110) : '', nameList([...s], 4)]; });
  L.push('## Procedures from other modules'); L.push('');
  L.push(tbl(['Procedure', 'Used by (count)', 'Signature', 'Examples'], rows)); L.push('');
  const cls = {};
  for (const t of ['form', 'module', 'class']) for (const n of S[t]) for (const c of g[t][n].refs.classes || []) (cls[c] ||= new Set()).add(t + ':' + n);
  L.push('## Classes instantiated or declared'); L.push('');
  L.push(tbl(['Class', 'Used by (count)', 'Examples'], Object.entries(cls).filter(([c]) => !S.class.has(c)).sort((a, b) => b[1].size - a[1].size).map(([c, s]) => ['`' + c + '`', s.size, nameList([...s], 5)]))); L.push('');
  L.push('## Shared forms'); L.push(''); L.push(nameList([...sharedForms])); L.push('');
  L.push('## Shared tables'); L.push(''); L.push(nameList(sharedTables)); L.push(''); L.push('Field lists in `shared-tables.md`.');
  w('external-dependencies.md', L.join('\n'));
}

// ---------- matrix.md ----------------------------------------------------------------------------------------------------
{
  const L = ['# Form x table matrix', '', '`R` = read, `W` = write, lowercase `r`/`w` = through a query that the form (or its code) runs. Combo/list sources count as `R`.', ''];
  const tabs = [...S.table].sort();
  function qTables(q, seen = new Set()) { if (seen.has(q)) return { r: new Set(), w: new Set() }; seen.add(q); const o = g.query[q]; const r = new Set(), w2 = new Set(); if (!o) return { r, w: w2 }; for (const x of o.refs.rel || []) { const c = relName(x); if (!goodRel(c)) continue; if (relType(c) === 'table') r.add(c); else { const s = qTables(c, seen); s.r.forEach(v => r.add(v)); s.w.forEach(v => w2.add(v)); } } for (const x of o.writes || []) { const c = relName(x); if (relType(c) === 'table') w2.add(c); } return { r, w: w2 }; }
  const rows = [];
  const used = new Set();
  for (const f of [...S.form].sort()) {
    const cell = {}; const fi = formInfo[f];
    const rel = new Set([...(fi.fo.refs.recordsource || []), ...(fi.fo.refs.rowsource || [])].map(relName)); const wr = new Set(), relc = new Set();
    for (const h of fi.H) { h.reads.forEach(x => relc.add(x)); h.writes.forEach(x => wr.add(x)); }
    for (const x of rel) { if (relType(x) === 'table') cell[x] = 'R'; else { const s = qTables(x); s.r.forEach(t => cell[t] = cell[t] || 'r'); s.w.forEach(t => cell[t] = 'w'); } }
    for (const x of relc) { if (relType(x) === 'table') cell[x] = cell[x] || 'R'; else { const s = qTables(x); s.r.forEach(t => cell[t] = cell[t] || 'r'); s.w.forEach(t => cell[t] = (cell[t] || '') + 'w'); } }
    for (const x of wr) { if (relType(x) === 'table') cell[x] = (cell[x] && cell[x] !== 'R' ? cell[x] : '') + 'W'; else { const s = qTables(x); s.w.forEach(t => cell[t] = (cell[t] || '') + 'w'); } }
    rows.push([f, cell]); Object.keys(cell).forEach(k => used.add(k));
  }
  for (const m of [...S.module, ...S.class]) {
    const t = S.module.has(m) ? 'module' : 'class'; const cell = {};
    for (const p of g[t][m].procs) { for (const x of (p.refs.rel || []).map(relName).filter(goodRel)) { if (relType(x) === 'table') cell[x] = cell[x] || 'R'; } for (const x of p.writes.map(relName)) if (relType(x) === 'table') cell[x] = 'W'; }
    rows.push([m + ' (code)', cell]); Object.keys(cell).forEach(k => used.add(k));
  }
  const cols = tabs.filter(t => used.has(t));
  L.push(tbl(['Object', ...cols], rows.map(([n, c]) => [n, ...cols.map(t => c[t] || '')])));
  w('matrix.md', L.join('\n'));
}
console.log('forms', S.form.size, 'tables', S.table.size, '(core', coreTables.size + ')', 'queries', S.query.size, 'shared forms', sharedForms.size);
