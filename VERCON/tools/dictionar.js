// VC00-02: generates VERCON/DICTIONAR_DATE.md from TZ_SCHEMA (md) + live read-only schema dumps (tools/out/schema_*.json).
const fs = require('fs'), path = require('path');
const ROOT = path.join(__dirname, '..');
const TZ = path.join(ROOT, 'VERCON_ACCESS', 'Databases', 'TZ_SCHEMA');
const OUT = path.join(__dirname, 'out');
const TN = { 1: 'Yes/No', 2: 'Byte', 3: 'Integer', 4: 'Long', 5: 'Currency', 6: 'Single', 7: 'Double', 8: 'Date/Time', 10: 'Text', 11: 'OLE', 12: 'Memo', 15: 'GUID' };
const DBS = ['tz_Alimente', 'tz_Mijloace_Fixe', 'tz_Obiecte_Inventar'];
const dumps = {}; for (const d of DBS) dumps[d] = JSON.parse(fs.readFileSync(path.join(OUT, `schema_${d}.json`), 'utf8').replace(/^\uFEFF/, ''));
const cale = JSON.parse(fs.readFileSync(path.join(OUT, 'schema_cale.json'), 'utf8').replace(/^\uFEFF/, ''));

const { EXCL, parseTz, cls, VALUE } = require('./lib_tz');
const tables = fs.readdirSync(path.join(TZ, 'tables')).map(f => f.replace(/\.md$/, '')).filter(n => !EXCL.test(n));

let md = `# VERCON — dictionar de date si graf FK (SLICE-VC00-02)

Generat de \`VERCON/tools/dictionar.js\` din \`TZ_SCHEMA\` (export tz_Alimente, referinta comuna decisa de utilizator) si din dump-uri **doar-schema** ale celor trei baze reale (\`tools/dump_schema.ps1\`, DAO read-only, numarul de randuri, fara continut). Tipurile tinta sunt **propuneri de dictionar** pentru VC03-02, nu DDL aplicat.

Starea: DOCUMENTAT. Nimic implementat, nimic validat pe server.

## 1. Reguli de tip (se aplica uniform pe toate gestiunile)

| Clasa sursa | Tinta MariaDB | Nota |
|---|---|---|
| CHEIE (AutoNumber/Long de legatura) | \`INT UNSIGNED\` generat | Cheia sursa se citeste **semnata** (ID-uri random pot fi negative) si se pastreaza in \`VC_IdMap\`. |
| VALOARE-INT (cantitati, preturi, valori) | \`INT\` (cu semn) | Scalat 10^SCARA, SCARA = scara de stocare a gestiunii (Alimente 4, MF 2, OI 3); afisarea foloseste NR.ZEC al gestiunii, rotunjit in sus la jumatate. Vezi CONTRACT_CALCULE §1. Pretul are CHECK >= 0. |
| COTA (TVA.TVA) | \`INT\` | cota x 10000 (0,19 -> 1900). |
| NUMAR / INT | \`INT\` | nu se scaleaza. |
| TEXT | \`VARCHAR(n)\` utf8mb3_general_ci | fara eliminarea diacriticelor; trunchierea se raporteaza. |
| DATA | \`DATETIME\` | \`Data\`, \`DataFc\`, \`DataBC\`, \`DataSalvare\`, \`DataIntrare\` nu se substituie reciproc. |
| BOOL | \`TINYINT(1)\` NULL/0/1 | -1 -> 1. |

Fiecare tabel business tinta primeste in plus: \`SubunitId\`, \`GestiuneId\` (si \`ExercitiuId\` unde relatia o cere), cheie sursa in \`VC_IdMap\`, audit. Fara tabele per an / per fisier / per tip.

## 2. Tabele business din TZ_SCHEMA si tratamentul coloanelor
`;

const clsCount = {};
for (const t of tables) {
  const p = parseTz(t);
  const rows = DBS.map(d => { const x = dumps[d].tables.find(y => y.name === t); return x ? `${d.replace('tz_', '')}: ${x.rows}` : `${d.replace('tz_', '')}: —`; }).join('; ');
  md += `\n### ${t}\n\nRanduri (alimente/MF/OI): ${rows}\n\n| Camp | Tip Access | Marime | Obligatoriu | Implicit | Clasa | Tinta / conversie |\n|---|---|---:|---|---|---|---|\n`;
  for (const f of p.fields) {
    const c = cls(t, f); clsCount[c[0]] = (clsCount[c[0]] || 0) + 1;
    md += `| ${f.n} | ${f.type} | ${f.size} | ${f.req ? 'da' : 'nu'} | ${f.def || ''} | ${c[0]} | ${c[1]} |\n`;
  }
  md += `\nIndexuri/chei (export): ${p.idx.map(i => `\`${i.n}\` (${i.cols})${i.flags ? ' ' + i.flags : ''}`).join('; ') || '—'}\n`;
}

// FK matrix
const rel = fs.readFileSync(path.join(TZ, 'relationships.md'), 'utf8').replace(/\r/g, "").split('\n').filter(l => l.startsWith('- **')).map(l => {
  const m = l.match(/: (\S+?)\.(\S+) -> (\S+?)\.(\S+?)(?: \[(.*)\])?(?: \((.*)\))?$/); return m ? { p: m[1], pf: m[2], c: m[3], cf: m[4], attr: m[5] || '', extra: m[6] || '' } : null;
}).filter(Boolean);
const business = rel.filter(r => !/^_|^SQL/.test(r.p));
const ctrl = (r) => {
  if (r.p === 'Iesiri_H' && r.c === 'LegBCLA') return 'FK composit de context spre Iesiri_H; atribut DAO 8192 neclasificat (vezi §4); cardinalitate verificata in date; RESTRICT';
  if (/^OI_/.test(r.c)) return `FK de context (SubunitId, GestiuneId, ${r.pf}); RESTRICT/inactivare; ${/IDC/.test(r.cf) ? 'detinatorul este nomenclator, nu se sterge daca e folosit' : 'linie OI pe cheie de linie (ID), nu pe IDBP/IDBT/IDBC'}`;
  if (r.p === 'LABC') return 'FK de context; antet lista -> detalii mese; stergere numai prin serviciul tranzactional (D05)';
  if (r.p === 'Produse' || r.p === 'ProduseP' || r.p === 'Masini') return 'FK de context; nomenclator folosit = RESTRICT/inactivare (nu CASCADE DELETE)';
  return 'FK de context; RESTRICT la nomenclator folosit; antet+linii se anuleaza numai prin serviciul tranzactional (D05)';
};
md += `\n## 3. Cele ${business.length} relatii business din TZ_SCHEMA si tratamentul tinta (SLICE-VC00-02 acceptare)\n\nAccess declara cascade update + delete pe aproape toate; tinta **nu copiaza** CASCADE DELETE peste istoric (PLAN §Politica FK; decizia finala pe relatie = D05). Totalul din export: ${rel.length} relatii (${rel.length - business.length} de metadate Access, excluse).\n\n| # | Parinte.camp | Copil.camp | Atribute exportate | Tratament tinta |\n|---:|---|---|---|---|\n`;
business.forEach((r, i) => { md += `| ${i + 1} | ${r.p}.${r.pf} | ${r.c}.${r.cf} | ${r.attr || '—'}${r.extra ? '; ' + r.extra : ''} | ${ctrl(r)} |\n`; });

// live relation attributes
md += `\n## 4. Atribute DAO brute ale relatiilor din bazele reale\n\nBitii: 1 unica, 2 fara integritate, 256 cascade update, 4096 cascade delete, 16777216 LEFT JOIN (referinta DAO RelationAttributeEnum). Valoarea 8192 nu are semnificatie documentata in lista publica consultata: **nu se interpreteaza**; maparea acestei legaturi se confirma pe o copie (D05).\n\n| Baza | Relatii | Distributie atribut |\n|---|---:|---|\n`;
for (const d of DBS) { const c = {}; for (const r of dumps[d].relations) { if (/^_|^SQL/.test(r.parent)) continue; c[r.attr] = (c[r.attr] || 0) + 1; } md += `| ${d} | ${dumps[d].relations.length} | ${Object.entries(c).map(([k, v]) => `${k}: ${v}`).join(', ')} |\n`; }
const odd = []; for (const d of DBS) for (const r of dumps[d].relations) if (![4352].includes(r.attr) && !/^_|^SQL/.test(r.parent)) odd.push(`${d}: ${r.parent} -> ${r.child} attr=${r.attr} (${r.fields.join(',')})`);
md += `\nRelatii business cu atribut diferit de 4352 (cascade update+delete):\n\n${odd.map(o => '- ' + o).join('\n') || '—'}\n`;

// logical links
md += `\n## 5. Legaturi logice fara relatie declarata (de profilat inainte de FK tinta)

| Legatura | Observatie |
|---|---|
| IntrariStorno.IdIntrare / IdProdus | referinta in cod; fara relatie declarata |
| ProduseM.IdProdus | referinta la ProduseP; fara relatie declarata |
| OI_Transfer.IDC_OLD | vechiul detinator; fara relatie declarata |
| NIR.IdMasina, IdStorno, IDFV; Iesiri_H.IdMasina, IdRetur; OI_Predare.IdBonConsum, IdMasinaP; OI_Produse.IDBP/IDBT/IDBC/IDBO/IDM | nu se deduc parinti dupa nume |
| OI_Predare.IDBP, OI_Transfer.IDBT, OI_Casare.IDBC | **grup, nu cheie unica** (PK este ID). FK simplu spre ele este interzis; normalizare in antete cu \`GroupSourceKey\` si pastrarea ID-ului de linie |
| LegBCLA.IdBonConsum | fara unicitate declarata; cardinalitatea reala se pastreaza |
| Retetar.IdFel | PK; nu se inventeaza ingrediente multiple |

## 6. Abateri de schema intre bazele reale si TZ_SCHEMA (N05)

Abaterile sunt versiuni anterioare/ulterioare, nu trei scheme tinta. Convertorul de transfer citeste punctual campurile prezente; camp **absent** (nu exista in sursa) este diferit de **NULL** (exista, fara valoare).
`;
for (const d of DBS) {
  md += `\n### ${d}\n\n`;
  const lines = [];
  for (const t of tables) {
    const p = parseTz(t); const x = dumps[d].tables.find(y => y.name === t);
    if (!x) { lines.push(`- tabel **lipsa** in baza reala: ${t}`); continue; }
    const live = new Map(x.fields.map(f => [f.n.toLowerCase(), f]));
    const tz = new Map(p.fields.map(f => [f.n.toLowerCase(), f]));
    for (const [k, f] of tz) if (!live.has(k)) lines.push(`- ${t}.${f.n}: lipsa in baza reala`);
    for (const [k, f] of live) if (!tz.has(k)) lines.push(`- ${t}.${f.n}: in plus fata de TZ_SCHEMA (tip ${TN[f.t]})`);
    for (const [k, f] of tz) { const l = live.get(k); if (l) { const lt = (TN[l.t] === 'Long' && l.ai) ? 'AutoNumber' : TN[l.t]; if (lt !== f.type) lines.push(`- ${t}.${f.n}: tip ${lt} in baza reala vs ${f.type} in TZ_SCHEMA`); } }
  }
  const extra = dumps[d].tables.filter(y => !EXCL.test(y.name) && !tables.includes(y.name)).map(y => y.name);
  md += (lines.join('\n') || 'fara abateri') + (extra.length ? `\n- tabele in plus fata de TZ_SCHEMA: ${extra.join(', ')}` : '') + '\n';
}
md += `\n### cale.accdb\n\nTabele: ${cale.tables.map(t => `${t.name} (${t.rows} randuri)`).join(', ')}. Mapare in VC03-04 (cale.PASS si orice credential **nu se importa**). EF/EFS/EFT sunt legate catre cale in frontend dar absente aici: excluse (e-Factura).\n`;
md += `\n## 7. Sinteza claselor de coloane (tabele business)\n\n${Object.entries(clsCount).map(([k, v]) => `- ${k}: ${v}`).join('\n')}\n\nCamp \`DE-CLARIFICAT\` = Double/alt tip fara regula: se decide la VC03-02, nu se ghiceste.\n`;
fs.writeFileSync(path.join(ROOT, 'DICTIONAR_DATE.md'), md);
console.log(clsCount, 'relations', rel.length, 'business', business.length);
