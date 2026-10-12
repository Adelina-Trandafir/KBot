// VC00-04 step 1: parse Access form/report exports (text) into layout JSON + traceability markdown.
// Read-only on VERCON_ACCESS. Output: tools/out/layouts/*.json, SCHITE/trasabilitate/*.md
const fs = require('fs'), path = require('path');
const ROOT = path.join(__dirname, '..');
const SRC = path.join(ROOT, 'VERCON_ACCESS');
const inv = JSON.parse(fs.readFileSync(path.join(__dirname, 'out', 'inventar.json'), 'utf8'));
const LAY = path.join(__dirname, 'out', 'layouts');
const TRC = path.join(ROOT, 'SCHITE', 'trasabilitate');
fs.mkdirSync(LAY, { recursive: true }); fs.mkdirSync(TRC, { recursive: true });

const SKETCH_EXTRA = new Set(['VaerCon', 'Main']); // reference for context selector / navigation
function wanted(r) {
  if (r.type !== 'form' && r.type !== 'report') return false;
  if (SKETCH_EXTRA.has(r.name) && r.type === 'form') return true;
  return /^(INCLUS|DE-CLARIFICAT|NEREFERIT)/.test(r.disp);
}

function unq(v) { v = v.trim(); if (v.startsWith('"')) { return v.slice(1).replace(/"$/, '').replace(/""/g, '"').replace(/\\"/g, '"').replace(/\\015\\012/g, ' ').replace(/\\0(15|12)/g, ' '); } return v; }

function parse(file) {
  const raw = fs.readFileSync(file, 'utf8').replace(/\r/g, '').split('\n');
  const root = { type: 'ROOT', props: {}, kids: [], line: 0 };
  const stack = [root];
  let i = 0;
  while (i < raw.length) {
    const ln = raw[i]; const t = ln.trim(); const no = i + 1;
    i++;
    if (!t) continue;
    // binary blocks: "Key = Begin" ... "End"
    if (/^\w+\s*=\s*Begin$/.test(t)) { while (i < raw.length && raw[i].trim() !== 'End') i++; i++; continue; }
    let m = t.match(/^Begin(?:\s+(\w+))?$/);
    if (m) {
      const node = { type: m[1] || '_group', props: {}, kids: [], line: no };
      stack[stack.length - 1].kids.push(node); stack.push(node); continue;
    }
    if (t === 'End') { if (stack.length > 1) stack.pop(); continue; }
    m = t.match(/^(\w+)\s*=\s*(.*)$/);
    if (m) {
      let val = m[2];
      // multi-line string continuation: following lines that are just quoted strings
      if (val.startsWith('"') && !/"\s*$/.test(val.slice(1)) ) { /* unterminated; rare */ }
      while (i < raw.length && /^\s+"/.test(raw[i]) && !/^\s*\w+\s*=/.test(raw[i])) { val = val.replace(/"\s*$/, '') + raw[i].trim().replace(/^"/, ''); i++; }
      stack[stack.length - 1].props[m[1]] = unq(val);
    }
  }
  return root;
}
function flatten(n, acc = []) { for (const k of n.kids) { acc.push(k); flatten(k, acc); } return acc; }

function build(r) {
  const file = path.join(SRC, r.file);
  const tree = parse(file);
  const top = tree.kids[0]; // Form / Report
  const props = top.props;
  const defaults = {}; // per control type default props from the leading anonymous group
  const sections = [];
  for (const k of top.kids) {
    if (k.type === '_group' && !sections.length && k.kids.some(x => /^(Label|TextBox|CommandButton)$/.test(x.type)) && !k.kids.some(x => /Section|Header|Footer/.test(x.type))) {
      for (const d of k.kids) defaults[d.type] = d.props;
    } else if (/(Section|Header|Footer)$/.test(k.type) || k.type === 'Section') {
      sections.push(k);
    } else if (k.type === '_group') {
      // sections may live in the trailing group
      for (const s of k.kids) if (/(Section|Header|Footer)$/.test(s.type) || s.type === 'Section') sections.push(s);
    }
  }
  const out = { name: r.name, kind: r.type, family: r.family || '', file: r.file, caption: props.Caption || '', recordSource: (props.RecordSource || '').replace(/\s+/g, ' '), width: +props.Width || 10000, sections: [] };
  const num = (v, d) => (v === undefined || v === '' || isNaN(+v) ? d : +v);
  const mapSectionName = (s) => (s.props.Name || s.type);
  for (const s of sections) {
    const sec = { name: mapSectionName(s), type: s.type, height: num(s.props.Height, 0), back: s.props.BackColor, line: s.line, controls: [] };
    // controls = children of the anonymous group(s) inside the section; attached labels are nested inside controls
    const walk = (n, depth) => {
      for (const c of n.kids) {
        if (c.type === '_group') { walk(c, depth); continue; }
        const d = defaults[c.type] || {};
        const p = { ...d, ...c.props };
        const ctl = {
          t: c.type, n: p.Name || '', cap: p.Caption || '', cs: p.ControlSource || '', rs: p.RowSource || '', so: p.SourceObject || '',
          // geometry: own props first, then LayoutCached* (Width/Height cached = right/bottom edge), then type defaults
          x: num(c.props.Left, num(c.props.LayoutCachedLeft, num(d.Left, 0))),
          y: num(c.props.Top, num(c.props.LayoutCachedTop, num(d.Top, 0))),
          w: c.props.Width !== undefined ? num(c.props.Width, 0) : (c.props.LayoutCachedWidth !== undefined ? num(c.props.LayoutCachedWidth, 0) - num(c.props.Left, num(c.props.LayoutCachedLeft, 0)) : num(d.Width, 0)),
          h: c.props.Height !== undefined ? num(c.props.Height, 0) : (c.props.LayoutCachedHeight !== undefined ? num(c.props.LayoutCachedHeight, 0) - num(c.props.Top, num(c.props.LayoutCachedTop, 0)) : num(d.Height, 0)),
          fs: num(p.FontSize, 8), fw: num(p.FontWeight, 400),
          fc: p.ForeColor, bc: p.BackColor, bs: p.BackStyle, vis: p.Visible === '0' ? false : true, line: c.line, ev: Object.keys(p).filter(k => /^On[A-Z]/.test(k)).join(','),
        };
        if (!ctl.w && c.type === 'Line') ctl.w = num(p.Width, 100);
        sec.controls.push(ctl);
        walk(c, depth + 1);
      }
    };
    walk(s, 0);
    out.sections.push(sec);
  }
  return out;
}

const done = [];
for (const r of inv.filter(wanted)) {
  try {
    const l = build(r);
    const slug = `${r.type === 'form' ? 'F' : 'R'}_${r.name.replace(/[^A-Za-z0-9_-]+/g, '_')}`;
    l.slug = slug;
    fs.writeFileSync(path.join(LAY, slug + '.json'), JSON.stringify(l));
    const nctl = l.sections.reduce((a, s) => a + s.controls.length, 0);
    let md = `# Trasabilitate schita ${r.type === 'form' ? 'formular' : 'raport'} ${r.name}\n\nSursa: [${r.file}](../../VERCON_ACCESS/${encodeURI(r.file)}) · imagine: [${slug}.png](../${slug}.png)\n\n- Titlu (Caption): ${l.caption || '—'}\n- Sursa de date (RecordSource): ${l.recordSource ? '`' + l.recordSource.slice(0, 400) + (l.recordSource.length > 400 ? ' …' : '') + '`' : '—'}\n- Latime: ${l.width} twips · sectiuni: ${l.sections.length} · controale: ${nctl}\n\n`;
    for (const s of l.sections) {
      md += `## ${s.name} (inaltime ${s.height} twips, linia ${s.line})\n\n`;
      const rows = s.controls.filter(c => c.t !== 'Label' || !c.cap || true);
      if (!rows.length) { md += '_fara controale_\n\n'; continue; }
      md += '| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |\n|---|---|---|---|---:|---|\n';
      for (const c of rows) {
        const what = (c.cap || c.cs || c.so || c.rs || '').replace(/\|/g, '/').slice(0, 120);
        md += `| ${c.n || '—'} | ${c.t} | ${what}${c.cs && c.cap ? ' (camp: ' + c.cs.replace(/\|/g, '/') + ')' : ''} | ${c.x},${c.y} ${c.w}x${c.h} | ${c.line} | ${c.ev || ''} |\n`;
      }
      md += '\n';
    }
    fs.writeFileSync(path.join(TRC, slug + '.md'), md);
    done.push({ slug, name: r.name, kind: r.type, family: r.family, disp: r.disp, file: r.file, caption: l.caption, rs: l.recordSource, sections: l.sections.length, controls: nctl, width: l.width });
  } catch (e) { console.error('FAIL', r.name, e.message); done.push({ slug: null, name: r.name, kind: r.type, error: e.message }); }
}
fs.writeFileSync(path.join(__dirname, 'out', 'schite_index.json'), JSON.stringify(done, null, 1));
console.log(done.length, 'objects;', done.filter(d => d.error).length, 'failed;', 'zero-control:', done.filter(d => d.controls === 0).map(d => d.name).join(','));
