// Builds graph.json from the RawExport folder: every form/report/module/class/macro/query/table with its references.
const fs = require('fs'), path = require('path');
const ROOT = process.argv[2];
const OUT = process.argv[3];

const read = p => { const b = fs.readFileSync(p); return (b[0] === 0xEF && b[1] === 0xBB) ? b.toString('utf8', 3) : b.toString('utf8'); };
const list = d => fs.existsSync(path.join(ROOT, d)) ? fs.readdirSync(path.join(ROOT, d)) : [];
const strip = (f, ext) => f.replace(ext, '');

const objs = { table: {}, query: {}, form: {}, report: {}, module: {}, class: {}, macro: {} };
for (const f of list('tables')) objs.table[strip(f, /\.md$/)] = { file: 'tables/' + f, text: read(path.join(ROOT, 'tables', f)) };
for (const f of list('queries')) objs.query[strip(f, /\.sql$/)] = { file: 'queries/' + f, text: read(path.join(ROOT, 'queries', f)) };
for (const f of list('Forms')) objs.form[strip(f, /\.txt$/)] = { file: 'Forms/' + f, text: read(path.join(ROOT, 'Forms', f)) };
for (const f of list('Reports')) objs.report[strip(f, /\.txt$/)] = { file: 'Reports/' + f, text: read(path.join(ROOT, 'Reports', f)) };
for (const f of list('Modules')) objs.module[strip(f, /\.bas\.txt$/)] = { file: 'Modules/' + f, text: read(path.join(ROOT, 'Modules', f)) };
for (const f of list('Classes')) objs.class[strip(f, /\.cls\.txt$/)] = { file: 'Classes/' + f, text: read(path.join(ROOT, 'Classes', f)) };
for (const f of list('Macros')) objs.macro[strip(f, /\.txt$/)] = { file: 'Macros/' + f, text: read(path.join(ROOT, 'Macros', f)) };

// ---- procedures in modules / classes ---------------------------------------
const procRe = /^[ \t]*(Public |Private |Friend )?(Static )?(Sub|Function|Property Get|Property Let|Property Set)[ \t]+(\w+)/gim;
const procOwner = Object.create(null); // name(lower) -> [{owner, kind, scope}]
function procsOf(text) {
  const out = []; let m; procRe.lastIndex = 0;
  while ((m = procRe.exec(text))) out.push({ name: m[4], kind: m[3], scope: (m[1] || 'Public ').trim(), line: text.slice(0, m.index).split('\n').length });
  return out;
}
for (const t of ['module', 'class']) for (const [n, o] of Object.entries(objs[t])) {
  o.procs = procsOf(o.text);
  for (const p of o.procs) if (p.scope !== 'Private') (procOwner[p.name.toLowerCase()] ||= []).push({ owner: n, type: t });
}

// ---- name matchers ----------------------------------------------------------
const allRel = new Set(); // table + query names
const nameType = Object.create(null);
for (const t of ['table', 'query']) for (const n of Object.keys(objs[t])) { nameType[n.toLowerCase()] = t; }
const formNames = new Set(Object.keys(objs.form).map(s => s.toLowerCase()));
const reportNames = new Set(Object.keys(objs.report).map(s => s.toLowerCase()));

const esc = s => s.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
// quoted/bracketed/bare references to tables and queries inside SQL-ish text
function relRefs(sql) {
  const found = new Set();
  const txt = sql.replace(/\\"/g, '"');
  // bracketed [Name]
  for (const m of txt.matchAll(/\[([^\]\[]+)\]/g)) { const k = m[1].toLowerCase(); if (nameType[k]) found.add(m[1]); }
  // bare identifiers after FROM / JOIN / INTO / UPDATE
  for (const m of txt.matchAll(/\b(?:FROM|JOIN|INTO|UPDATE|TABLE)\s+\(*\s*([A-Za-z_@#][\w@#$]*)/gi)) { const k = m[1].toLowerCase(); if (nameType[k]) found.add(Object.keys(objs.table).concat(Object.keys(objs.query)).find(x => x.toLowerCase() === k)); }
  // table.field style
  for (const m of txt.matchAll(/\b([A-Za-z_@#][\w@#$]*)\.(?:\*|\[|[A-Za-z_])/g)) { const k = m[1].toLowerCase(); if (nameType[k] && k.length > 2) found.add(Object.keys(objs.table).concat(Object.keys(objs.query)).find(x => x.toLowerCase() === k)); }
  return [...found].filter(Boolean);
}
const classNames = new Set(Object.keys(objs.class));
function classRefs(text, selfOwner){ const f=new Set(); for(const m of text.matchAll(/(?:New|As)s+(w+)/gi)) if(classNames.has(m[1])&&m[1]!==selfOwner) f.add(m[1]); return [...f]; }
function fnRefs(text, selfOwner) {
  // identifiers followed by ( or used as calls that match a known public procedure
  const found = new Map();
  for (const m of text.matchAll(/\b([A-Za-z_]\w{3,})\b/g)) {
    const k = m[1].toLowerCase();
    const owners = procOwner[k];
    if (owners) for (const o of owners) if (o.type === 'module' && o.owner !== selfOwner) found.set(o.owner + '.' + m[1], { owner: o.owner, type: o.type, proc: m[1] });
  }
  return [...found.values()];
}

// ---- form / report parsing ---------------------------------------------------
function parseQuotedValue(lines, i, afterEq) {
  // value like  ="a"  followed by continuation lines  "b" ; returns [value, nextIndex]
  let val = ''; let cur = afterEq.trim(); let j = i;
  const take = s => { const m = s.match(/^"((?:[^"\\]|\\.)*)"/); return m ? m[1] : null; };
  let seg = take(cur);
  if (seg === null) return [cur, i];
  val += seg;
  while (j + 1 < lines.length) {
    const nxt = lines[j + 1].trim();
    if (nxt.startsWith('"')) { const s2 = take(nxt); if (s2 === null) break; val += s2; j++; } else break;
  }
  return [val.replace(/\\"/g, '"').replace(/\\\\/g, '\\'), j];
}
const EVENT_PROPS = /^(On\w+|AfterUpdate|BeforeUpdate)$/;
function parseDesign(text) {
  const idx = text.search(/^CodeBehindForm\s*$/m);
  const design = idx >= 0 ? text.slice(0, idx) : text;
  const code = idx >= 0 ? text.slice(idx) : '';
  const lines = design.split(/\r?\n/);
  const out = { props: {}, controls: [], sections: [], events: {} };
  const stack = []; // {kind, name, depth}
  let cur = null;
  for (let i = 0; i < lines.length; i++) {
    const raw = lines[i]; const ln = raw.trim();
    if (ln === 'Begin') { cur = { bare: true, props: {}, events: {}, kind: 'bare' }; stack.push(cur); continue; }
    let m = ln.match(/^Begin (\w+)$/);
    if (m) {
      const kind = m[1];
      if (kind === 'Form' || kind === 'Report') { cur = { kind, name: '', top: true, props: out.props, events: out.events }; stack.push(cur); continue; }
      if (/^(Section|FormHeader|FormFooter|PageHeader|PageFooter|GroupHeader|GroupFooter|Detail)$/.test(kind) || /Section$/.test(kind)) { cur = { kind, name: kind, isSection: true, props: {}, events: {} }; out.sections.push(cur); stack.push(cur); continue; }
      cur = { kind, name: '', props: {}, events: {}, section: [...stack].reverse().find(s => s.isSection)?.kind || null, parentRef: [...stack].reverse().find(s => !s.isSection && !s.top && !s.bare) || null };
      out.controls.push(cur); stack.push(cur); continue;
    }
    if (ln === 'End') { stack.pop(); cur = stack[stack.length - 1] || null; continue; }
    m = ln.match(/^(\w+)\s*=\s*(.*)$/);
    if (m && cur) {
      let key = m[1], rest = m[2];
      if (/^Begin\s*$/.test(rest.trim())) { // binary block, skip to its End
        while (i + 1 < lines.length && lines[i + 1].trim() !== 'End') i++;
        i++; continue;
      }
      let [val, ni] = parseQuotedValue(lines, i, rest); i = ni;
      if (EVENT_PROPS.test(key)) cur.events[key] = val; else cur.props[key] = val;
    }
  }
  return { design: out, code };
}

for (const t of ['form', 'report']) for (const [n, o] of Object.entries(objs[t])) {
  const { design, code } = parseDesign(o.text);
  o.design = design; o.codeText = code; o.procs = procsOf(code);
  o.formProps = design.props; o.sections = design.sections.map(x => ({ kind: x.kind, props: x.props, events: x.events })); o.formEvents = design.events;
  const evProcs = new Set();
  o.controls = design.controls.map(c => ({
    kind: c.kind, props: c.props, name: c.props.Name || '', parent: c.parentRef ? (c.parentRef.props.Name || '') : '', fontName: c.props.FontName || '', fontSize: c.props.FontSize || '', fontWeight: c.props.FontWeight || '', backColor: c.props.BackColor || '', foreColor: c.props.ForeColor || '', format: c.props.Format || '', inputMask: c.props.InputMask || '', textAlign: c.props.TextAlign || '', picture: c.props.Picture || '', locked2: c.props.Locked || '', tabStop: c.props.TabStop || '', defaultB: c.props.Default || '', cancel: c.props.Cancel || '', validationRule: c.props.ValidationRule || '', borderStyle: c.props.BorderStyle || '', special: c.props.SpecialEffect || '', optionValue: c.props.OptionValue || '', allowAutoCorrect: c.props.AllowAutoCorrect || '', anchorH: c.props.HorizontalAnchor || '', anchorV: c.props.VerticalAnchor || '', section: c.section, controlSource: c.props.ControlSource || '', rowSource: c.props.RowSource || '', rowSourceType: c.props.RowSourceType || '',
    sourceObject: c.props.SourceObject || '', linkChild: c.props.LinkChildFields || '', linkMaster: c.props.LinkMasterFields || '', caption: c.props.Caption || '', defaultValue: c.props.DefaultValue || '',
    visible: c.props.Visible || '', enabled: c.props.Enabled || '', locked: c.props.Locked || '', tag: c.props.Tag || '',
    left: c.props.Left, top: c.props.Top, width: c.props.Width, height: c.props.Height, events: c.events, tabIndex: c.props.TabIndex, columnWidths: c.props.ColumnWidths || '', boundColumn: c.props.BoundColumn || '', controlTipText: c.props.ControlTipText || ''
  }));
}

// ---- references per object ----------------------------------------------------
const refs = {}; // key type:name -> {rel:[], forms:[], reports:[], fns:[], macros:[], subforms:[]}
function addRef(r, k, v) { (r[k] ||= new Set()).add(typeof v === 'string' ? v : JSON.stringify(v)); }
const openRe = {
  forms: /DoCmd\.OpenForm\s*\(?\s*"([^"]+)"|Forms\s*!\s*\[?([\w ]+?)\]?\s*!|Forms\s*\(\s*"([^"]+)"\s*\)|\bForm_(\w+)\b/gi,
  reports: /DoCmd\.OpenReport\s*\(?\s*"([^"]+)"|Reports\s*!\s*\[?([\w ]+?)\]?\s*!|Reports\s*\(\s*"([^"]+)"\s*\)|\bReport_(\w+)\b/gi,
  queries: /DoCmd\.OpenQuery\s*\(?\s*"([^"]+)"|\.OpenRecordset\s*\(\s*"([^"]+)"|\.Execute\s+"([^"]+)"|DoCmd\.RunSQL\s+"([^"]+)"/gi,
  macros: /DoCmd\.RunMacro\s*\(?\s*"([^"]+)"/gi,
};
function codeRefs(owner, text, r) {
  for (const m of text.matchAll(openRe.forms)) { const v = m[1] || m[2] || m[3] || m[4]; if (v && formNames.has(v.toLowerCase())) addRef(r, 'forms', Object.keys(objs.form).find(x => x.toLowerCase() === v.toLowerCase())); }
  for (const m of text.matchAll(openRe.reports)) { const v = m[1] || m[2] || m[3] || m[4]; if (v && reportNames.has(v.toLowerCase())) addRef(r, 'reports', Object.keys(objs.report).find(x => x.toLowerCase() === v.toLowerCase())); }
  for (const m of text.matchAll(openRe.macros)) if (objs.macro[m[1]]) addRef(r, 'macros', m[1]);
  // every quoted literal and every SQL-ish snippet may name tables / queries
  for (const m of text.matchAll(/"((?:[^"]|"")*)"/g)) { for (const x of relRefs(m[1])) addRef(r, 'rel', x); if (nameType[m[1].toLowerCase()]) addRef(r, 'rel', Object.keys(objs.table).concat(Object.keys(objs.query)).find(x => x.toLowerCase() === m[1].toLowerCase())); }
  for (const f of fnRefs(text, owner)) addRef(r, 'fns', f);
  for (const c of classRefs(text, owner)) addRef(r, 'classes', c);
}
for (const t of Object.keys(objs)) for (const [n, o] of Object.entries(objs[t])) {
  const r = {}; o.refs = r;
  if (t === 'query') {
    for (const x of relRefs(o.text)) if (x !== n) addRef(r, 'rel', x);
    for (const m of o.text.matchAll(/\[?Forms\]?\s*!\s*\[?([\w ]+?)\]?\s*!/gi)) { const v = Object.keys(objs.form).find(x => x.toLowerCase() === m[1].toLowerCase()); if (v) addRef(r, 'forms', v); }
    for (const f of fnRefs(o.text, n)) addRef(r, 'fns', f);
  } else if (t === 'form' || t === 'report') {
    if (o.formProps.RecordSource) { const rs = o.formProps.RecordSource; const direct = nameType[rs.toLowerCase()] ? [Object.keys(objs.table).concat(Object.keys(objs.query)).find(x => x.toLowerCase() === rs.toLowerCase())] : relRefs(rs); for (const x of direct) addRef(r, 'recordsource', x); for (const m of rs.matchAll(/Forms\s*!\s*\[?([\w ]+?)\]?\s*!/gi)) { const v = Object.keys(objs.form).find(x => x.toLowerCase() === m[1].toLowerCase()); if (v) addRef(r, 'forms', v); } for (const f of fnRefs(rs, n)) addRef(r, 'fns', f); }
    for (const c of o.controls) {
      for (const s of [c.rowSource]) if (s) { const direct = nameType[s.toLowerCase()] ? [Object.keys(objs.table).concat(Object.keys(objs.query)).find(x => x.toLowerCase() === s.toLowerCase())] : relRefs(s); for (const x of direct) addRef(r, 'rowsource', x); for (const m of s.matchAll(/Forms\s*!\s*\[?([\w ]+?)\]?\s*!/gi)) { const v = Object.keys(objs.form).find(x => x.toLowerCase() === m[1].toLowerCase()); if (v) addRef(r, 'forms', v); } }
      if (c.sourceObject) { const so = c.sourceObject.replace(/^(Form|Report|Query|Table)\./i, ''); if (objs.form[so]) addRef(r, 'subforms', { name: c.name, form: so, child: c.linkChild, master: c.linkMaster }); else if (objs.report[so]) addRef(r, 'subforms', { name: c.name, form: so, child: c.linkChild, master: c.linkMaster, report: true }); }
      for (const [ev, v] of Object.entries(c.events)) if (v.startsWith('=')) for (const f of fnRefs(v, n)) addRef(r, 'fns', f);
      if (c.controlSource.startsWith('=')) for (const f of fnRefs(c.controlSource, n)) addRef(r, 'fns', f);
      for (const [ev, v] of Object.entries(c.events)) if (v && !v.startsWith('=') && v !== '[Event Procedure]' && objs.macro[v]) addRef(r, 'macros', v);
    }
    codeRefs(n, o.codeText, r);
  } else if (t === 'module' || t === 'class') {
    codeRefs(n, o.text, r);
  }
}

// ---- per-procedure refs and write targets ------------------------------------
function writeTargets(txt) {
  const w = new Set(); const t = txt.replace(/\\"/g, '"');
  for (const m of t.matchAll(/\b(INSERT\s+INTO|UPDATE|DELETE\s+(?:\*\s+)?FROM|SELECT\b[^;]*?\bINTO)\s+\[?([A-Za-z_@#][\w@#$]*)\]?/gi)) {
    const k = m[2].toLowerCase();
    if (nameType[k]) w.add(Object.keys(objs.table).concat(Object.keys(objs.query)).find(x => x.toLowerCase() === k));
  }
  if (/\.(AddNew|Edit)\b/i.test(txt)) {
    for (const m of t.matchAll(/OpenRecordset\s*\(\s*"([^"]+)"/gi)) {
      const sql = m[1]; const direct = nameType[sql.toLowerCase()] ? [sql] : relRefs(sql).slice(0, 1);
      for (const x of direct) { const k = x.toLowerCase(); if (nameType[k]) w.add(Object.keys(objs.table).concat(Object.keys(objs.query)).find(y => y.toLowerCase() === k)); }
    }
    for (const m of t.matchAll(/With\s+CurrentDb\.OpenRecordset\s*\(\s*"([^"]+)"/gi)) { const k = m[1].toLowerCase(); if (nameType[k]) w.add(Object.keys(objs.table).concat(Object.keys(objs.query)).find(y => y.toLowerCase() === k)); }
  }
  return [...w];
}
for (const t of ['form', 'report', 'module', 'class']) for (const [n, o] of Object.entries(objs[t])) {
  const body = (t === 'form' || t === 'report') ? o.codeText : o.text;
  const lines = body.split(/\r?\n/);
  o.procs.forEach((p, i) => {
    const start = p.line - 1, end = (i + 1 < o.procs.length) ? o.procs[i + 1].line - 1 : lines.length;
    const txt = lines.slice(start, end).join('\n'); p.lines = end - start;
    const r = {}; codeRefs(n, txt, r);
    p.refs = {}; for (const [k, v] of Object.entries(r)) p.refs[k] = [...v].map(x => { try { return JSON.parse(x); } catch { return x; } });
    p.writes = writeTargets(txt);
    p.callsLocal = [...new Set([...txt.matchAll(/\b([A-Za-z_]\w{3,})\b/g)].map(m => m[1]).filter(w => o.procs.some(q => q.name.toLowerCase() === w.toLowerCase() && q.name !== p.name)))];
    const prev = (lines[start - 1] || '').trim();
    p.header = prev.startsWith("'") ? prev : '';
    p.sig = lines[start].trim();
  });
}
for (const [n, o] of Object.entries(objs.query)) {
  const m = o.text.match(/^-- Type: (.+)$/m); o.qtype = m ? m[1].trim() : '';
  o.writes = /Append|Update|Delete|MakeTable/i.test(o.qtype) ? writeTargets(o.text) : [];
}
for (const [n, o] of Object.entries(objs.table)) {
  const L = o.text.split(/\r?\n/); const lk = L.find(l => l.startsWith('Linked table'));
  o.linked = lk ? lk.replace(/PWD=[^;`]*/, 'PWD=***') : '';
  o.fields = L.filter(l => /^\| \d+ \|/.test(l)).map(l => l.split('|').slice(1, -1).map(x => x.trim()));
  o.indexes = L.filter(l => l.startsWith('- ')).map(l => l.slice(2));
}
// serialise
const g = {};
for (const t of Object.keys(objs)) {
  g[t] = {};
  for (const [n, o] of Object.entries(objs[t])) {
    const refsOut = {}; for (const [k, s] of Object.entries(o.refs)) refsOut[k] = [...s].map(x => { try { return JSON.parse(x); } catch { return x; } });
    g[t][n] = { qtype:o.qtype, writes:o.writes, linked:o.linked, fields:o.fields, indexes:o.indexes, file: o.file, size: o.text.length, procs: o.procs, refs: refsOut, sections: o.sections, formEvents: o.formEvents, allFormProps: o.formProps, formProps: o.formProps ? { RecordSource: o.formProps.RecordSource, Caption: o.formProps.Caption, DefaultView: o.formProps.DefaultView, PopUp: o.formProps.PopUp, Modal: o.formProps.Modal, OrderBy: o.formProps.OrderBy, Filter: o.formProps.Filter, AllowAdditions: o.formProps.AllowAdditions, AllowEdits: o.formProps.AllowEdits, AllowDeletions: o.formProps.AllowDeletions, DataEntry: o.formProps.DataEntry, Width: o.formProps.Width } : undefined, controls: o.controls };
  }
}
fs.writeFileSync(OUT, JSON.stringify(g));
const cnt = Object.fromEntries(Object.entries(g).map(([k, v]) => [k, Object.keys(v).length]));
console.log(cnt, Object.keys(procOwner).length + ' public procs');
