// SLICE-VC03-02: generates sql/VC_02_business.sql from TZ_SCHEMA (same reader/classification as the dictionary).
// Rules (PLAN, runda 3 + CONTRACT_CALCULE): every business table carries SubunitId + GestiuneId (+ ExercitiuId for
// annual documents); every FK is composite on that context so a row can never reference another gestiune; values are
// INT scaled 10^Gestiuni.Scara; price >= 0 by CHECK; RESTRICT everywhere (no CASCADE over history); suppliers are the
// KBOT partners (no VC_Furnizori); no per-year / per-file / per-type tables. Only reads VERCON_ACCESS exports.
const fs = require('fs'), path = require('path');
const { TZ, parseTz, cls } = require('./lib_tz');

const GEST = ['Masini', 'Produse', 'ProduseP', 'ProduseM', 'Clienti', 'ComisieNIR', 'ComisieInventar', 'TVA', 'FeluriMancare', 'Retetar', 'OI_Produse'];
const EXER = ['NIR', 'Intrari', 'IntrariStorno', 'Iesiri_H', 'Iesiri', 'IesiriRetur', 'OI_Predare', 'OI_Transfer', 'OI_Casare', 'LABC', 'ListaAlimente', 'LegBCLA', 'MicDejun', 'Dejun', 'Cina'];
const NOMENCLATOR = new Set(['Masini', 'Produse', 'ProduseP', 'Clienti', 'ComisieNIR', 'ComisieInventar', 'TVA', 'FeluriMancare']);
const CANCELABLE = new Set(['NIR', 'IntrariStorno', 'Iesiri_H', 'LABC', 'OI_Predare', 'OI_Transfer', 'OI_Casare']);
const PRICE = /^(Pret|PUTVA|PU)$/;
const EXTRA = { ProduseM: [{ n: 'ValoareLunara', type: 'Double', size: 8, req: false, def: '' }] };   // present only in the MF base (dictionary §6)
const SKIP_FIELDS = { NIR: new Set(['IDF', 'IDFV']) };   // suppliers are KBOT partners: IdPartener replaces them

const scope = (t) => (EXER.includes(t) ? 'E' : 'G');
const ALL = [...GEST, ...EXER];

// relations
const rel = fs.readFileSync(path.join(TZ, 'relationships.md'), 'utf8').replace(/\r/g, '').split('\n').filter((l) => l.startsWith('- **')).map((l) => {
  const m = l.match(/: (\S+?)\.(\S+) -> (\S+?)\.(\S+?)(?: \[(.*)\])?(?: \((.*)\))?$/);
  return m ? { p: m[1], pf: m[2], c: m[3], cf: m[4], attr: m[5] || '' } : null;
}).filter(Boolean).filter((r) => ALL.includes(r.p) && ALL.includes(r.c));

const info = {};
for (const t of ALL) {
  const p = parseTz(t);
  const pkIdx = p.idx.find((i) => /PRIMARY/.test(i.flags));
  let pk = pkIdx && !pkIdx.cols.includes(',') ? pkIdx.cols.trim() : null;
  // a primary key that is itself a foreign key (one-to-one detail tables) cannot be generated: the table gets a RowId
  if (pk && rel.some((r) => r.c === t && r.cf === pk)) pk = null;
  info[t] = { fields: [...p.fields, ...(EXTRA[t] || [])], pk };
}

function sqlType(c, f) {
  const k = c[0];
  if (k === 'VALOARE-INT' || k === 'COTA' || k === 'NUMAR' || k === 'INT') return 'INT';
  if (k === 'CHEIE') return 'INT UNSIGNED';
  if (k === 'TEXT') return f.type === 'Memo' ? 'TEXT' : `VARCHAR(${Math.min(f.size, 255)})`;
  if (k === 'DATA') return 'DATETIME';
  if (k === 'BOOL') return 'TINYINT(1)';
  throw new Error(`clasa necunoscuta ${k} la ${f.n}`);
}
function sqlDefault(c, f) {
  const d = (f.def || '').trim();
  if (d === '' || /[=()]/.test(d)) return '';
  if (c[0] === 'BOOL') return /^(no|false|0)$/i.test(d) ? ' DEFAULT 0' : /^(yes|true|-1|1)$/i.test(d) ? ' DEFAULT 1' : '';
  if (['VALOARE-INT', 'COTA', 'NUMAR', 'INT'].includes(c[0])) return /^-?\d+(\.\d+)?$/.test(d) ? ` DEFAULT ${Math.trunc(Number(d))}` : '';
  if (c[0] === 'TEXT' && /^[A-Za-z0-9 _.\-]+$/.test(d)) return ` DEFAULT '${d}'`;
  return '';
}

const ctx = (t) => (scope(t) === 'E' ? ['SubunitId', 'GestiuneId', 'ExercitiuId'] : ['SubunitId', 'GestiuneId']);
const vname = (t) => `VC_${t}`;
const pkOf = (t) => info[t].pk || 'RowId';

// order: parents first
const deps = {}; for (const t of ALL) deps[t] = new Set(rel.filter((r) => r.c === t && r.p !== t).map((r) => r.p));
const order = []; const seen = new Set();
for (const t of ALL) (function v(x) { if (order.includes(x)) return; for (const d of deps[x]) v(d); if (!order.includes(x)) order.push(x); })(t);

const ruleChecks = [];
let out = '';
for (const t of order) {
  const I = info[t]; const cols = []; const keys = []; const fks = []; const checks = [];
  const pk = pkOf(t);
  cols.push(`  \`${pk}\` INT UNSIGNED NOT NULL AUTO_INCREMENT`);
  for (const c of ctx(t)) cols.push(`  \`${c}\` INT UNSIGNED NOT NULL`);
  const relsOut = rel.filter((r) => r.c === t);
  for (const f of I.fields) {
    if (f.n === I.pk) continue;
    if (SKIP_FIELDS[t]?.has(f.n)) continue;
    const c = cls(t, f);
    if (c[0] === 'CONFIG' || c[0] === 'DE-CLARIFICAT') throw new Error(`${t}.${f.n}: clasa ${c[0]}`);
    const isFk = relsOut.some((r) => r.cf === f.n);
    const notNull = f.req && (c[0] !== 'CHEIE' || isFk) ? ' NOT NULL' : ' NULL';
    cols.push(`  \`${f.n}\` ${sqlType(c, f)}${notNull}${sqlDefault(c, f)}`);
    if (PRICE.test(f.n)) checks.push(`  CONSTRAINT \`ck_${t}_${f.n}_nonneg\` CHECK (\`${f.n}\` IS NULL OR \`${f.n}\` >= 0)`);
  }
  if (t === 'NIR') {
    cols.push('  `IdPartener` INT(11) NULL COMMENT \'KBOT Parteneri.IdPartener; NULL for a Stoc initial document\'');
    cols.push("  `CodFiscalSursa` VARCHAR(255) NULL COMMENT 'Furnizori.CF of the Access supplier at migration time: history only, the link is IdPartener'");
    cols.push("  `NumarSub` SMALLINT NOT NULL DEFAULT 0 COMMENT 'fraction of the document number in hundredths: 707.1 = Numar 707 + NumarSub 10; a NIR inserted between two numbers keeps its chronological place'");
    cols.push("  `TipDoc` VARCHAR(20) NOT NULL DEFAULT 'NIR' COMMENT 'NIR or STOC_INITIAL (document type without partner)'");
    checks.push("  CONSTRAINT `ck_NIR_TipDoc` CHECK (`TipDoc` IN ('NIR','STOC_INITIAL'))");
    checks.push("  CONSTRAINT `ck_NIR_StocInitial_fara_partener` CHECK (`TipDoc` <> 'STOC_INITIAL' OR `IdPartener` IS NULL)");
    keys.push('  KEY `ix_NIR_Partener` (`IdPartener`)');
    fks.push('  CONSTRAINT `fk_NIR_Partener` FOREIGN KEY (`IdPartener`) REFERENCES `Parteneri` (`IdPartener`) ON DELETE RESTRICT');
  }
  if (NOMENCLATOR.has(t)) cols.push('  `Activ` TINYINT(1) NOT NULL DEFAULT 1');
  if (CANCELABLE.has(t)) {
    cols.push('  `Anulat` TINYINT(1) NOT NULL DEFAULT 0', '  `MotivAnulare` VARCHAR(255) NULL', '  `AnulatDe` VARCHAR(255) NULL', '  `AnulatLa` DATETIME NULL');
    checks.push(`  CONSTRAINT \`ck_${t}_motiv\` CHECK (\`Anulat\` = 0 OR (\`MotivAnulare\` IS NOT NULL AND \`MotivAnulare\` <> ''))`);
  }
  cols.push('  `CreatedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP', '  `UpdatedAt` DATETIME NULL ON UPDATE CURRENT_TIMESTAMP', '  `Version` BIGINT NOT NULL DEFAULT 1');
  // keys
  keys.unshift(`  PRIMARY KEY (\`${pk}\`)`);
  keys.push(`  UNIQUE KEY \`uq_${t}_ctx\` (${[...ctx(t), pk].map((c) => `\`${c}\``).join(', ')})`);
  fks.unshift(`  CONSTRAINT \`fk_${t}_gestiune\` FOREIGN KEY (\`SubunitId\`, \`GestiuneId\`) REFERENCES \`VC_Gestiuni\` (\`SubunitId\`, \`GestiuneId\`) ON DELETE RESTRICT`);
  if (scope(t) === 'E') fks.splice(1, 0, `  CONSTRAINT \`fk_${t}_exercitiu\` FOREIGN KEY (\`SubunitId\`, \`GestiuneId\`, \`ExercitiuId\`) REFERENCES \`VC_Exercitii\` (\`SubunitId\`, \`GestiuneId\`, \`ExercitiuId\`) ON DELETE RESTRICT`);
  for (const r of relsOut) {
    if (r.p === 'Furnizori') continue;
    const parentPk = pkOf(r.p);
    if (info[r.p].pk !== r.pf) throw new Error(`${r.p}.${r.pf} nu este cheia primara`);
    // an annual child may reference an annual parent at exercise level, or a gestiune-level parent at gestiune level
    const levelE = scope(t) === 'E' && scope(r.p) === 'E';
    const cc = levelE ? ['SubunitId', 'GestiuneId', 'ExercitiuId'] : ['SubunitId', 'GestiuneId'];
    const k_cols = `(${[...cc, r.cf].map((c) => `\`${c}\``).join(', ')})`;
    // one-to-one: the unique key is also the index of the foreign key
    // LABC -> LegBCLA is one-to-one in Access, but the web target is 1:N (a list may have several BCs): plain index, no unique key
    const oneToOne = /one-to-one/.test(r.attr) && !(r.p === 'LABC' && r.c === 'LegBCLA');
    keys.push(oneToOne ? `  UNIQUE KEY \`uq_${t}_${r.cf}\` ${k_cols}` : `  KEY \`ix_${t}_${r.cf}\` ${k_cols}`);
    fks.push(`  CONSTRAINT \`fk_${t}_${r.cf}\` FOREIGN KEY (${[...cc, r.cf].map((c) => `\`${c}\``).join(', ')}) REFERENCES \`${vname(r.p)}\` (${[...cc, parentPk].map((c) => `\`${c}\``).join(', ')}) ON DELETE RESTRICT`);
  }
  if (t === 'LegBCLA') {
    // list <-> BC is 1:N (new in the web system; Access was 1:1): a BC belongs to ONE list (so it also appears once in it)
    keys.push('  UNIQUE KEY `uq_LegBCLA_IdBonConsum` (`SubunitId`, `GestiuneId`, `ExercitiuId`, `IdBonConsum`)');
  }
  // CHECK constraints are not carried by schema sync: they go to the optional per-DC rules script
  for (const c of checks) ruleChecks.push(`ALTER TABLE \`${vname(t)}\` ADD ${c.trim()};`);
  out += `\n-- ${t} (${scope(t) === 'E' ? 'gestiune + exercitiu' : 'gestiune'})\nCREATE TABLE IF NOT EXISTS \`${vname(t)}\` (\n${[...cols, ...keys, ...fks].join(',\n')}\n) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;\n`;
}

// ---- assemble the two scripts ---------------------------------------------------------------------------------------
const part = (f) => fs.readFileSync(path.join(__dirname, 'ddl_parts', f), 'utf8').replace(/\r/g, '');
const SQL = path.join(__dirname, '..', '..', 'sql');
const head1 = `-- VERCON schema for AVACONT_SURSA (SLICE-VC03). GENERATED by VERCON/tools/ddl.js -- do not edit by hand.
-- Run this file ONCE on AVACONT_SURSA (plain statements, CREATE TABLE IF NOT EXISTS, no procedures, no DELIMITER).
-- KBotAdmin schema sync then carries the tables, columns, keys and foreign keys to every DC database.
-- It does NOT carry triggers or CHECK constraints: those are in VC_Reguli_DC.sql (optional, per DC) and are also
-- enforced by the service. Not run from this chat.
-- Model: one set of tables for every gestiune type (no per-type / per-year / per-file tables); every row carries its
-- context (SubunitId, GestiuneId[, ExercitiuId]) and every FK is composite on it; values are INT scaled
-- 10^VC_Gestiuni.Scara (VERCON/CONTRACT_CALCULE.md); RESTRICT, no CASCADE over history; suppliers = KBOT Parteneri;
-- source primary keys are not stored: MariaDB generates them and VC_IdMap keeps (source id -> target id).

-- ===== hierarchy and exclusive access =====
`;
fs.writeFileSync(path.join(SQL, 'VC_AVACONT_SURSA.sql'), head1 + part('tables_01.sql') + '\n-- ===== business tables (generated) =====\n' + out + '\n-- ===== import, audit, settings, numbering =====\n' + part('tables_03.sql'));
const head2 = `-- VERCON rules that schema sync does NOT carry (CHECK constraints and triggers). OPTIONAL, run on a DC database AFTER
-- the tables reached it through schema sync; also valid on a test DC for VC_99_probe.sql. Re-running fails on the first
-- ADD CONSTRAINT / CREATE TRIGGER that already exists -- that means the rules are already there. Not run from this chat.
-- The same rules are enforced by the service; this file is a second line of defence, not the only one.

`;
fs.writeFileSync(path.join(SQL, 'VC_Reguli_DC.sql'), head2 + '-- ===== CHECK constraints =====\n' + part('rules_checks_01_03.sql') + ruleChecks.join('\n') + '\n\n-- ===== triggers =====\n' + part('rules_triggers.sql'));

// relations diagram (Mermaid) next to the DDL, so the schema can be read without MariaDB
const er = ['erDiagram', '  VC_Subunitati ||--o{ VC_Gestiuni : "are"', '  VC_Gestiuni ||--o{ VC_Exercitii : "are"', '  VC_Gestiuni ||--o| VC_ContextLock : "ocupata de"',
  ...ALL.filter((t) => scope(t) === 'E').map((t) => `  VC_Exercitii ||--o{ ${vname(t)} : "contine"`),
  ...ALL.filter((t) => scope(t) === 'G').map((t) => `  VC_Gestiuni ||--o{ ${vname(t)} : "contine"`),
  ...rel.map((r) => `  ${vname(r.p)} ||--o{ ${vname(r.c)} : "${r.cf}"`),
  '  Parteneri ||--o{ VC_NIR : "IdPartener"'];
fs.writeFileSync(path.join(__dirname, '..', 'SCHEMA_VC_RELATII.mmd'), er.join('\n') + '\n');
console.log('tables', order.length, 'order', order.join(','));

// table scopes for the server-side repository (routes/vercon/tables.py): the repository only touches these tables
const py = '"""GENERATED by VERCON/tools/ddl.js -- do not edit. Scope of every VC_ business table (SLICE-VC04-02)."""\n'
  + `GESTIUNE_TABLES = frozenset(${JSON.stringify(ALL.filter((t) => scope(t) === 'G').map(vname))})\n`
  + `EXERCITIU_TABLES = frozenset(${JSON.stringify(ALL.filter((t) => scope(t) === 'E').map(vname))})\n`;
fs.writeFileSync(path.join(__dirname, '..', '..', 'PYTHON', 'routes', 'vercon', 'tables.py'), py.replace(/\[/g, '{').replace(/\]/g, '}'));

// ---- target description for VRC.Migrator (SLICE-VC05-02/03): the writer is driven by this file, not by 26 hand-written mappers.
// Link kinds: fk = declared relation (also a FOREIGN KEY in the DDL); logical = documented reference WITHOUT a declared relation
// (remapped, no FK); group = OI group label (renumbered per source, kept consistent across tables); supplier = NIR.IDF -> KBOT
// Parteneri (by tax code); open = a key column nobody has classified yet: allowed only when every source value is NULL/0.
const LOGICAL = {
  'IntrariStorno.IdIntrare': 'NIR', 'IntrariStorno.IdProdus': 'ProduseP', 'ProduseM.IdProdus': 'ProduseP', 'OI_Transfer.IDC_OLD': 'Clienti',
  'NIR.IdMasina': 'Masini', 'Iesiri_H.IdMasina': 'Masini', 'OI_Casare.IdMasina': 'Masini', 'OI_Predare.IdMasina': 'Masini', 'OI_Transfer.IdMasina': 'Masini',
  'OI_Predare.IdBonConsum': 'Iesiri_H',
};
const GROUPS = new Set(['OI_Predare.IDBP', 'OI_Transfer.IDBT', 'OI_Casare.IDBC', 'OI_Produse.IDBP', 'OI_Produse.IDBT', 'OI_Produse.IDBC']);
const target = { generated: 'VERCON/tools/ddl.js', tables: [] };
for (const t of order) {
  const I = info[t]; const relsOut = rel.filter((r) => r.c === t);
  const columns = [];
  for (const f of I.fields) {
    if (f.n === I.pk) continue;
    const c = cls(t, f);
    const fk = relsOut.find((r) => r.cf === f.n && r.p !== 'Furnizori');
    const nullable = !(f.req && (c[0] !== 'CHEIE' || !!fk));
    let link = null;
    if (c[0] === 'CHEIE') {
      const key = `${t}.${f.n}`;
      if (t === 'NIR' && f.n === 'IDF') link = { kind: 'supplier' };
      else if (SKIP_FIELDS[t]?.has(f.n)) link = { kind: 'skip' };
      else if (f.type === 'AutoNumber') link = { kind: 'tech' };   // a technical row number that is not the primary key (LABC.ID, ProduseM.IDPMF): carries no reference
      else if (fk) link = { kind: 'fk', parent: fk.p, parentField: fk.pf };
      else if (LOGICAL[key]) link = { kind: 'logical', parent: LOGICAL[key], parentField: info[LOGICAL[key]].pk };
      else if (GROUPS.has(key)) link = { kind: 'group', group: f.n };
      else link = { kind: 'open' };
    }
    columns.push({ name: f.n, cls: c[0], nullable, link });
  }
  target.tables.push({ name: t, vname: vname(t), scope: scope(t), pk: I.pk, rowIdIsPk: !I.pk, columns });
}
fs.writeFileSync(path.join(__dirname, '..', 'VRC.Migrator', 'target_schema.json'), JSON.stringify(target, null, 1) + '\n');
