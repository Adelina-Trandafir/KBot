// VC00-01: who references each DE-CLARIFICAT object (static name search; menu XML included). Output: tools/out/referenti.json + stdout.
const fs = require('fs'), path = require('path');
const ROOT = path.join(__dirname, '..');
const SRC = path.join(ROOT, 'VERCON_ACCESS');
const inv = JSON.parse(fs.readFileSync(path.join(__dirname, 'out', 'inventar.json'), 'utf8'));
const texts = {};
for (const r of inv) {
  let t = ''; try { t = fs.readFileSync(path.join(SRC, r.file), 'utf8'); } catch (e) { /* missing */ }
  texts[r.type + ':' + r.name] = t.replace(/\r/g, '').split('\n').filter(l => !/^\s*0x/.test(l)).join('\n');
}
const menuXml = fs.readFileSync(path.join(ROOT, 'REFERINTE', 'tmpOptiuniSFP.xml'), 'utf8');
const esc = (x) => x.replace(/[.*+?^${}()|[\]\\]/g, (c) => '\\' + c);
const out = {};
for (const r of inv.filter(x => x.disp === 'DE-CLARIFICAT')) {
  const re = new RegExp('(?<![A-Za-z0-9_])' + esc(r.name) + '(?![A-Za-z0-9_])', 'i');
  const refs = [];
  for (const o of inv) {
    if (o === r) continue;
    if (re.test(texts[o.type + ':' + o.name])) refs.push({ k: o.type[0] + ':' + o.name, excl: /^EXCLUS/.test(o.disp) });
  }
  out[r.type + ':' + r.name] = { refs, menu: re.test(menuXml) };
  console.log((r.type + ':' + r.name).padEnd(34), refs.slice(0, 7).map(x => x.k + (x.excl ? '(x)' : '')).join(', ') + (refs.length > 7 ? ' +' + (refs.length - 7) : ''), re.test(menuXml) ? '[MENIU]' : '');
}
fs.writeFileSync(path.join(__dirname, 'out', 'referenti.json'), JSON.stringify(out, null, 1));
