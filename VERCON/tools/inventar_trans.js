// Transitive Coduri exclusion (N06): a query/form/report that reads an excluded Coduri query/table is excluded too.
// Strict FROM/JOIN matching so column aliases (e.g. "AS Storno") do not create false dependencies.
const fs = require('fs'), path = require('path');
module.exports = function (rows, D, SRC, rd) {
  const esc = (x) => x.replace(/[.*+?^${}()|[\]\\]/g, (c) => '\\' + c);
  const refRe = (n) => new RegExp('(?:FROM|JOIN|,)\\s*[(\\s]*\\[?' + esc(n) + '\\]?(?=[\\s),;]|$)', 'i');
  const qText = {};
  for (const r of rows.filter(x => x.type === 'query')) qText[r.name] = rd(path.join(SRC, r.file));
  const qNames = rows.filter(x => x.type === 'query').map(x => x.name);
  const tNames = rows.filter(x => x.type === 'table').map(x => x.name);
  const xT = new Set(rows.filter(x => x.type === 'table' && x.disp === D.XCOD).map(x => x.name));
  const xQ = new Set(rows.filter(x => x.type === 'query' && x.disp === D.XCOD).map(x => x.name));
  const directQ = (text) => qNames.filter(n => refRe(n).test(text));
  const directT = (text) => tNames.filter(n => refRe(n).test(text));
  let changed = true;
  while (changed) {
    changed = false;
    for (const r of rows.filter(x => x.type === 'query' && x.disp !== D.XCOD)) {
      const t = qText[r.name];
      const hit = directQ(t).find(n => xQ.has(n)) || directT(t).find(n => xT.has(n));
      if (hit) { r.disp = D.XCOD; r.note = 'dependenta tranzitiva de ' + hit + ' (Coduri)'; xQ.add(r.name); changed = true; }
    }
  }
  for (const r of rows.filter(x => x.type === 'form' || x.type === 'report')) {
    const t = rd(path.join(SRC, r.file));
    const rsAll = [...t.matchAll(/(?:RecordSource|RowSource)\s*=\s*"([^"]*)"/g)].map(m => m[1]).join(' ');
    const sql = ' FROM ' + rsAll.replace(/;/g, ' ,') + ' ';
    const hit = directQ(sql).find(n => xQ.has(n)) || directT(sql).find(n => xT.has(n))
      || qNames.find(n => xQ.has(n) && new RegExp('^\\s*' + esc(n) + '\\s*$', 'i').test(rsAll));
    if (hit && r.disp !== D.XCOD) {
      r.note = ((r.note || '') + ' dependenta tranzitiva de ' + hit + ' (Coduri); obiect legacy exclus N06').trim();
      r.disp = D.XCOD;
    }
  }
};
