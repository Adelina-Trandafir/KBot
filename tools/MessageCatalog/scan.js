// Scans src/ for every message box call and writes the catalog the debug editor reads:
//   src/KBot.DevHarness/Config/mesaje_catalog.json
//
//   node tools/MessageCatalog/scan.js            -> first run: writes the catalog
//   node tools/MessageCatalog/scan.js --merge    -> keeps what the operator edited (by id),
//                                                   adds new calls (isNew), refreshes locations
//
// Calls found: KBotMessage.Show / ShowOnTop, MessageBox.Show, MsgBox. The text of a call that is
// not a plain literal keeps its variable parts as {expression}; an identifier-only text
// ("Show(intrebare, ...)") is followed back to its assignments in the same member.
'use strict';
const fs = require('fs');
const path = require('path');

const root = path.resolve(__dirname, '..', '..');
const srcRoot = path.join(root, 'src');
const outFile = path.join(srcRoot, 'KBot.App', 'Config', 'mesaje_catalog.json');
const merge = process.argv.includes('--merge');

const SKIP_DIRS = new Set(['bin', 'obj', '_reference', 'Surse', 'node_modules', '.vs']);
const SKIP_FILES = new Set(['KBotMessage.vb']);
const Q = String.fromCharCode(34);   // the double quote, spelled out so no replace() pattern can eat it

function walk(dir, out) {
  for (const e of fs.readdirSync(dir, { withFileTypes: true })) {
    if (e.isDirectory()) { if (!SKIP_DIRS.has(e.name)) walk(path.join(dir, e.name), out); }
    else if (e.name.endsWith('.vb') && !SKIP_FILES.has(e.name)) out.push(path.join(dir, e.name));
  }
  return out;
}

// Copies a VB string literal (plain or interpolated) that starts at src[i]; returns [text, nextIndex].
function copyString(src, i) {
  let out = '';
  if (src[i] === '$') { out += '$'; i++; }
  out += Q; i++;
  let d = 0;
  while (i < src.length) {
    if (src[i] === Q && src[i + 1] === Q) { out += Q + Q; i += 2; continue; }
    if (src[i] === Q && d === 0) break;
    if (src[i] === '{' && src[i + 1] === '{') { out += '{{'; i += 2; continue; }
    if (src[i] === '{') d++;
    if (src[i] === '}' && d > 0) d--;
    out += src[i++];
  }
  return [out + Q, i + 1];
}
const startsString = (src, i) => src[i] === Q || (src[i] === '$' && src[i + 1] === Q);

// --- low level: split the arguments of a call, aware of strings, comments, nesting ---------
function readArgs(text, open) {
  const args = []; let cur = ''; let depth = 0; let i = open + 1;
  while (i < text.length) {
    const c = text[i];
    if (startsString(text, i)) { const [s, n] = copyString(text, i); cur += s; i = n; continue; }
    if (c === "'") { while (i < text.length && text[i] !== '\n') i++; continue; }
    if (c === '(' || c === '{' || c === '[') depth++;
    if (c === ')' || c === '}' || c === ']') {
      if (depth === 0 && c === ')') { args.push(cur.trim()); return { args, end: i }; }
      depth--;
    }
    if (c === ',' && depth === 0) { args.push(cur.trim()); cur = ''; i++; continue; }
    if (c === '_' && /\s/.test(text[i - 1] || '') && /^[ \t]*\r?\n/.test(text.slice(i + 1, i + 8))) { i++; continue; }
    cur += c; i++;
  }
  return null;
}

// One statement starting at 'i' (continuation lines, open parens, trailing operators).
function readStatement(src, i) {
  let out = ''; let depth = 0;
  while (i < src.length) {
    const c = src[i];
    if (startsString(src, i)) { const [s, n] = copyString(src, i); out += s; i = n; continue; }
    if (c === "'") { while (i < src.length && src[i] !== '\n') i++; continue; }
    if (c === '(' || c === '{') depth++;
    if (c === ')' || c === '}') depth--;
    if (c === '\n') {
      const t = out.replace(/\s+$/, '');
      if (depth > 0 || /(\s_|[&+,(])$/.test(t)) { out = t.replace(/\s_$/, ' ') + ' '; i++; continue; }
      break;
    }
    out += c; i++;
  }
  return out.trim();
}

// --- text expression -> display text --------------------------------------------------------
function splitConcat(expr) {
  const parts = []; let cur = ''; let depth = 0; let i = 0;
  while (i < expr.length) {
    const c = expr[i];
    if (startsString(expr, i)) { const [s, n] = copyString(expr, i); cur += s; i = n; continue; }
    if (c === '(' || c === '{' || c === '[') depth++;
    if (c === ')' || c === '}' || c === ']') depth--;
    if ((c === '&' || c === '+') && depth === 0) { parts.push(cur.trim()); cur = ''; i++; continue; }
    cur += c; i++;
  }
  parts.push(cur.trim());
  return parts.filter(p => p.length);
}

function oneTerm(t) {
  if (/^(Environment\.NewLine|vbCrLf|vbLf|vbNewLine|ControlChars\.(NewLine|CrLf|Lf))$/i.test(t)) return { s: '\n', dyn: false };
  const plain = new RegExp('^' + Q + '((?:[^' + Q + ']|' + Q + Q + ')*)' + Q + '$').exec(t);
  if (plain) return { s: plain[1].split(Q + Q).join(Q).replace(/[{]/g, '{{').replace(/[}]/g, '}}'), dyn: false };
  const interp = new RegExp('^\\$' + Q + '(.*)' + Q + '$', 's').exec(t);
  if (interp) {
    const s = interp[1].split(Q + Q).join(Q);
    return { s, dyn: /(^|[^{])\{(?!\{)/.test(s) };
  }
  if (/^\(.*\)$/.test(t)) return oneTerm(t.slice(1, -1).trim());
  return { s: '{' + t.replace(/\s+/g, ' ') + '}', dyn: true };
}

// An identifier-only term: the nearest earlier assignment in the same member, plus the &= after it.
function resolveVar(name, before) {
  if (!before) return null;
  const re = new RegExp('^[ \\t]*(?:Dim\\s+)?' + name + '(?:\\s+As\\s+String)?\\s*(&=|=)(?!=)\\s*', 'gim');
  const hits = []; let m;
  while ((m = re.exec(before))) hits.push({ op: m[1], expr: readStatement(before, m.index + m[0].length) });
  if (!hits.length) return null;
  let start = hits.length - 1; while (start > 0 && hits[start].op === '&=') start--;
  if (hits[start].op === '&=') return null;
  return hits.slice(start).map(h => h.expr).join(' & ');
}

function textOf(expr, before, depth) {
  if (expr == null) return { text: '', dynamic: false, fromVariable: false };
  depth = depth || 0;
  let dynamic = false; let out = ''; let fromVariable = false;
  for (const p of splitConcat(expr)) {
    if (/^[A-Za-z_]\w*$/.test(p) && before && depth < 3 && !/^(vb\w+|Nothing|Me)$/i.test(p)) {
      const ex = resolveVar(p, before);
      if (ex) { const r = textOf(ex, before, depth + 1); out += r.text; dynamic = dynamic || r.dynamic; fromVariable = true; continue; }
    }
    const r = oneTerm(p); out += r.s; dynamic = dynamic || r.dyn;
  }
  return { text: out, dynamic, fromVariable };
}

// --- classification -------------------------------------------------------------------------
const ICON = { Error: 'Error', Hand: 'Error', Stop: 'Error', Warning: 'Warning', Exclamation: 'Warning', Information: 'Info', Asterisk: 'Info', Question: 'Question', None: 'None' };
const MSGSTYLE_ICON = { Critical: 'Error', Exclamation: 'Warning', Information: 'Info', Question: 'Question' };
const MSGSTYLE_BTN = { OkOnly: 'OK', OkCancel: 'OKCancel', YesNo: 'YesNo', YesNoCancel: 'YesNoCancel', RetryCancel: 'RetryCancel', AbortRetryIgnore: 'AbortRetryIgnore' };

function classify(styleForm, args, before) {
  const r = { type: 'None', buttons: 'OK', defaultButton: 1, caption: '', text: '', dynamic: false, fromVariable: false, resolved: true };
  if (styleForm) {                                   // (prompt, MsgBoxStyle, title)
    const t = textOf(args[0], before); r.text = t.text; r.dynamic = t.dynamic; r.fromVariable = t.fromVariable;
    r.caption = textOf(args[2], before).text;
    const st = args[1] || '';
    for (const k of Object.keys(MSGSTYLE_ICON)) if (new RegExp('MsgBoxStyle\\.' + k + '\\b', 'i').test(st)) r.type = MSGSTYLE_ICON[k];
    for (const k of Object.keys(MSGSTYLE_BTN)) if (new RegExp('MsgBoxStyle\\.' + k + '\\b', 'i').test(st)) r.buttons = MSGSTYLE_BTN[k];
    return r;
  }
  let a = args.slice();
  const db = a.findIndex(x => /MessageBoxDefaultButton\./.test(x));
  if (db >= 0) { r.defaultButton = parseInt((/Button(\d)/.exec(a[db]) || [0, 1])[1], 10); a = a.slice(0, db); }
  const bi = a.findIndex(x => /MessageBoxButtons\./.test(x));
  const ii = a.findIndex(x => /MessageBoxIcon\./.test(x));
  if (bi >= 0) r.buttons = (/MessageBoxButtons\.(\w+)/.exec(a[bi]) || [0, 'OK'])[1];
  if (ii >= 0) r.type = ICON[(/MessageBoxIcon\.(\w+)/.exec(a[ii]) || [0, 'None'])[1]] || 'None';
  let capIdx;
  if (bi >= 0) capIdx = bi - 1;
  else if (ii >= 0) { r.resolved = false; capIdx = ii - 2; }      // buttons passed as a variable
  else capIdx = a.length - 1;                                      // Show([owner,] text, caption)
  const textIdx = capIdx - 1;
  if (capIdx < 0 || textIdx < 0) { r.resolved = false; return r; }
  r.caption = textOf(a[capIdx], before).text;
  const t = textOf(a[textIdx], before); r.text = t.text; r.dynamic = t.dynamic; r.fromVariable = t.fromVariable;
  return r;
}

// --- enclosing member -----------------------------------------------------------------------
const MEMBER = /^\s*(?:(?:Public|Private|Protected|Friend|Shared|Overrides|Overridable|Overloads|Async|Iterator|NotOverridable|MustOverride|ReadOnly|WriteOnly|Default|Partial|Shadows)\s+)*(Sub|Function|Property|Operator)\s+(\w+)/i;
function memberInfo(lines, lineIdx) {
  for (let i = lineIdx; i >= 0; i--) {
    const l = lines[i];
    if (i !== lineIdx && /^\s*End\s+(Sub|Function|Property)\b/i.test(l)) return { name: '(module level)', start: i + 1 };
    const m = MEMBER.exec(l);
    if (m && !/^\s*'/.test(l)) return { name: m[2], start: i };
  }
  return { name: '(module level)', start: 0 };
}

function codeBefore(lineText, col) {          // False when a comment quote precedes the column
  let inStr = false;
  for (let k = 0; k < col; k++) {
    const c = lineText[k];
    if (c === Q) inStr = !inStr;
    else if (c === "'" && !inStr) return false;
  }
  return true;
}

// --- main -----------------------------------------------------------------------------------
const CALL = /\b(KBotMessage\.ShowOnTop|KBotMessage\.Show|MessageBox\.Show|MsgBox)\s*\(/g;
const entries = []; const unresolved = [];
for (const file of walk(srcRoot, []).sort()) {
  const text = fs.readFileSync(file, 'utf8').replace(/^﻿/, '');
  const lines = text.split(/\r?\n/);
  const lineStarts = []; { let p = 0; for (const l of text.split('\n')) { lineStarts.push(p); p += l.length + 1; } }
  const rel = path.relative(root, file).replace(/\\/g, '/');
  const base = path.basename(file, '.vb');
  const seen = new Map();
  let m; CALL.lastIndex = 0;
  while ((m = CALL.exec(text))) {
    let ln = 0; while (ln + 1 < lineStarts.length && lineStarts[ln + 1] <= m.index) ln++;
    if (!codeBefore(lines[ln], m.index - lineStarts[ln])) continue;
    const parsed = readArgs(text, m.index + m[0].length - 1);
    if (!parsed) { unresolved.push(rel + ':' + (ln + 1) + ' (unbalanced)'); continue; }
    const mem = memberInfo(lines, ln);
    const key = base + '.' + mem.name;
    const n = (seen.get(key) || 0) + 1; seen.set(key, n);
    const styleForm = m[1] === 'MsgBox' || (parsed.args.length === 3 && /MsgBoxStyle\./.test(parsed.args[1]));
    const c = classify(styleForm, parsed.args, text.slice(lineStarts[mem.start], m.index));
    if (!c.resolved) unresolved.push(rel + ':' + (ln + 1) + '  ' + parsed.args.join(' | ').slice(0, 160));
    entries.push({
      id: key + '#' + n, file: rel, line: ln + 1, function: key, api: m[1],
      type: c.type, buttons: c.buttons, defaultButton: c.defaultButton,
      extraButton: '', header: '', closeButton: 'Auto', caption: c.caption, text: c.text, dynamic: c.dynamic, fromVariable: c.fromVariable,
      origType: c.type, origButtons: c.buttons, origCaption: c.caption, origText: c.text
    });
  }
}

let result = entries; let vanished = 0; let added = 0; let migrated = 0;
if (merge && fs.existsSync(outFile)) {
  const old = JSON.parse(fs.readFileSync(outFile, 'utf8'));
  fs.copyFileSync(outFile, outFile + '.bak');                       // the previous catalog is never lost
  const byId = new Map((old.messages || []).map(x => [x.id, x]));
  result = entries.map(e => {
    const o = byId.get(e.id);
    if (!o) { added++; return Object.assign(e, { isNew: true }); }
    byId.delete(e.id);
    // 'orig*' always follows the code; a catalog from before 'orig*' existed held no edits worth keeping.
    if (o.origText === undefined) { migrated++; return Object.assign(e, { isNew: !!o.isNew }); }
    return Object.assign({}, o, { file: e.file, line: e.line, function: e.function, api: e.api, dynamic: e.dynamic,
      fromVariable: e.fromVariable, origType: e.origType, origButtons: e.origButtons, origCaption: e.origCaption, origText: e.origText });
  });
  vanished = byId.size;
}
fs.mkdirSync(path.dirname(outFile), { recursive: true });
fs.writeFileSync(outFile, JSON.stringify({ version: 2, generated: new Date().toISOString().slice(0, 10), messages: result }, null, 2) + String.fromCharCode(10), 'utf8');
console.log('calls: ' + entries.length + '  dynamic: ' + entries.filter(e => e.dynamic).length + '  fromVariable: ' + entries.filter(e => e.fromVariable).length + '  unresolved: ' + unresolved.length);
console.log('NEW: ' + added + '  VANISHED: ' + vanished + '  MIGRATED: ' + migrated);
for (const u of unresolved) console.log('UNRESOLVED ' + u);
