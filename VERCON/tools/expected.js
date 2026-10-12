// SLICE-VC05-01: writes VERCON/VRC.Migrator/expected_schema.json from TZ_SCHEMA (same reader/classification as the
// dictionary). The migrator embeds it to profile a source: which columns to measure, and which expected columns a
// source does not have (ABSENT is different from NULL). Only reads VERCON_ACCESS exports.
const fs = require('fs'), path = require('path');
const { TZ, EXCL, parseTz, cls } = require('./lib_tz');

const EXTRA = { ProduseM: [{ n: 'ValoareLunara', type: 'Double', size: 8, req: false, def: '' }] };   // only in the MF base
const SKIP = new Set(['Alimente', 'cpv', 'CPV1', 'Caletemp']);                                         // not business tables (see SCHEMA_VC.md)
const tables = fs.readdirSync(path.join(TZ, 'tables')).map((f) => f.replace(/\.md$/, '')).filter((n) => !EXCL.test(n) && !SKIP.has(n) && !/^(tmp|Tmp|FM_)/.test(n));

const out = { generated: 'VERCON/tools/expected.js', tables: {} };
for (const t of tables) {
  const p = parseTz(t);
  const pk = (p.idx.find((i) => /PRIMARY/.test(i.flags)) || {}).cols || null;
  out.tables[t] = {
    primaryKey: pk && !pk.includes(',') ? pk.trim() : null,
    fields: [...p.fields, ...(EXTRA[t] || [])].map((f) => {
      const c = cls(t, f);
      return { name: f.n, accessType: f.type, class: c[0] };
    }),
  };
}
const dest = path.join(__dirname, '..', 'VRC.Migrator', 'expected_schema.json');
fs.mkdirSync(path.dirname(dest), { recursive: true });
fs.writeFileSync(dest, JSON.stringify(out, null, 1) + '\n');
console.log(Object.keys(out.tables).length, 'tables ->', dest);
