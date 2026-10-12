// VC00-04 step 3: SCHITE/INDEX.md, MAPARE_ACCESS_WEB.md, RAPOARTE.md from inventory + layouts + query dependency scan.
const fs = require('fs'), path = require('path');
const ROOT = path.join(__dirname, '..');
const SRC = path.join(ROOT, 'VERCON_ACCESS');
const inv = JSON.parse(fs.readFileSync(path.join(__dirname, 'out', 'inventar.json'), 'utf8'));
const idx = JSON.parse(fs.readFileSync(path.join(__dirname, 'out', 'schite_index.json'), 'utf8'));
const rd = (p) => { try { return fs.readFileSync(p, 'utf8'); } catch (e) { return ''; } };

const tables = inv.filter(r => r.type === 'table').map(r => r.name);
const queries = inv.filter(r => r.type === 'query').map(r => r.name);
const queryText = {}; for (const q of queries) queryText[q] = rd(path.join(SRC, 'queries', q + '.sql')).replace(/\r/g, '');
const esc = (s) => s.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
function deps(text, seen, depth = 0) {
  const t = new Set(), q = new Set();
  const ref = (n) => new RegExp('(?:FROM|JOIN|,)\\s*[(\\s]*\\[?' + esc(n) + '\\]?(?=[\\s),;]|$)', 'i');
  const body = ' FROM ' + text.replace(/;/g, ' ,') + ' ';
  for (const n of tables) if (ref(n).test(body) && !/^(x|A|B|SQL|Rbb)$/.test(n)) t.add(n);
  for (const n of queries) if (!seen.has(n) && ref(n).test(body)) q.add(n);
  if (depth < 5) for (const n of [...q]) { seen.add(n); const d = deps(queryText[n] || '', seen, depth + 1); d.t.forEach(x => t.add(x)); d.q.forEach(x => q.add(x)); }
  return { t, q };
}
function srcOf(r, layout) {
  let text = layout ? layout.recordSource + ' ' : '';
  const raw = rd(path.join(SRC, r.file)).replace(/\r/g, '');
  // row sources, subforms
  const sub = [...raw.matchAll(/SourceObject\s*=\s*"(?:Form|Report)\.([^"]+)"/g)].map(m => m[1]);
  const rs = [...raw.matchAll(/RowSource\s*=\s*"([^"]*)"/g)].map(m => m[1]).join(' ');
  return { text: text + rs, sub: [...new Set(sub)] };
}
const metaBy = Object.fromEntries(idx.filter(x => x.slug).map(x => [x.name + '|' + x.kind, x]));

// gestiune capability from data dependencies / family
function caps(r, d) {
  if (r.type === 'form' && formRule[r.name]) { const f = formRule[r.name]; return { MAT: f.MAT, OI: f.OI, MF: f.MF, AL: f.AL, src: f.src }; }
  if (r.type === 'report' && menuByReport[lc(r.name)]) {
    const tips = menuByReport[lc(r.name)].map(x => x.e.TIP);
    const c = { MAT: false, OI: false, MF: false, AL: false, src: 'meniu SFP, TIP ' + [...new Set(tips)].join(' + ') };
    for (const t of tips) { const k = tipCaps(t); for (const x of ['MAT', 'OI', 'MF', 'AL']) c[x] = c[x] || k[x]; }
    return c;
  }
  const inf = caps0(r, d); inf.src = 'dedus din dependente (nu din meniu/cod)'; return inf;
}
function caps0(r, d) {
  const T = d.t;
  const has = (re) => [...T].some(x => re.test(x));
  const c = { MAT: false, OI: false, MF: false, AL: false };
  const fam = r.family || '';
  if (r.type === 'form') {
    if (/^FC$/.test(fam)) { c.MAT = c.OI = c.MF = c.AL = true; }
    else if (/^BC$/.test(fam)) { c.MAT = true; c.AL = true; }
    else if (/^(BR|FCS)$/.test(fam)) { c.MAT = c.OI = c.MF = c.AL = null; }
    else if (/^OI$/.test(fam)) { c.OI = true; c.MF = true; }
    else if (/^Alimente$/.test(fam)) { c.AL = true; }
    else { c.MAT = c.OI = c.MF = c.AL = true; }
  } else {
    if (/^MF$/.test(fam)) c.MF = true;
    else if (/^OI /.test(fam)) { c.OI = true; c.MF = true; }
    else if (/^Alimente$/.test(fam)) c.AL = true;
    else if (has(/^OI_|^ProduseM$/)) { c.OI = true; c.MF = true; }
    else if (has(/^(ListaAlimente|LABC|MicDejun|Dejun|Cina)$/)) c.AL = true;
    else if (has(/^Iesiri_H$|^Iesiri$/)) { c.MAT = true; c.AL = true; c.OI = null; c.MF = null; }
    else { c.MAT = c.OI = c.MF = c.AL = true; }
  }
  return c;
}
const mark = (v) => v === true ? 'da' : v === null ? 'de verificat' : 'nu';

// ---- tmpOptiuniSFP (REFERINTE/tmpOptiuniSFP.xml, furnizat de utilizator): meniul SFP, TIP G/O/M
const menuXml = rd(path.join(ROOT, 'REFERINTE', 'tmpOptiuniSFP.xml')).replace(/\r/g, '');
const menu = [...menuXml.matchAll(/<tmpOptiuniSFP>([\s\S]*?)<\/tmpOptiuniSFP>/g)].map(m => { const o = {}; for (const f of m[1].matchAll(/<(\w+)>([^<]*)<\/\1>/g)) o[f[1]] = f[2]; return o; });
const lc = (x) => (x || '').toLowerCase();
const menuByReport = {};
for (const e of menu) for (const k of ['Raport', 'RaportA']) if (e[k]) (menuByReport[lc(e[k])] = menuByReport[lc(e[k])] || []).push({ e, k });
function tipCaps(tip) { const t = (tip || '').split('-'); return { MAT: t.includes('G'), AL: t.includes('G'), OI: t.includes('O'), MF: t.includes('M') }; }
// confirmed Form_Open rules in the export (section "Reguli din cod" of MAPARE_ACCESS_WEB)
const formRule = {
  BC: { MAT: true, AL: true, OI: false, MF: false, src: 'Form_Open: refuza daca OBINV sau MF' },
  BR: { MAT: true, AL: true, OI: false, MF: false, src: 'Form_Open: refuza daca OBINV sau MF' },
  ListaAlimente: { MAT: true, AL: true, OI: false, MF: false, src: 'Form_Open: refuza daca OBINV sau MF (apare si la gestiuni fara Alimente)' },
  OI_Predare: { MAT: false, AL: false, OI: true, MF: true, src: 'Form_Open: refuza daca nici OBINV nici MF' },
  OI_Transfer: { MAT: false, AL: false, OI: true, MF: true, src: 'Form_Open: refuza daca nici OBINV nici MF' },
  OI_Casare: { MAT: false, AL: false, OI: true, MF: true, src: 'Form_Open: refuza daca nici OBINV nici MF' },
  IA: { MAT: true, AL: true, OI: false, MF: false, src: 'decizie utilizator 11.10.2026: OI si MF NU se inchid anual (Access refuza doar la OBINV)' },
};
const rows = [];
for (const r of inv.filter(x => (x.type === 'form' || x.type === 'report'))) {
  const m = metaBy[r.name + '|' + r.type];
  let layout = null; if (m) layout = JSON.parse(rd(path.join(__dirname, 'out', 'layouts', m.slug + '.json')));
  const s = srcOf(r, layout);
  const d = deps(s.text, new Set());
  s.sub.forEach(sf => { const o = inv.find(x => x.name === sf && x.type === 'form'); if (o) { const l2 = metaBy[sf + '|form'] ? JSON.parse(rd(path.join(__dirname, 'out', 'layouts', metaBy[sf + '|form'].slug + '.json'))) : null; const dd = deps((l2 ? l2.recordSource : '') + ' ' + srcOf(o, l2).text, new Set()); dd.t.forEach(x => d.t.add(x)); dd.q.forEach(x => d.q.add(x)); } });
  rows.push({ r, m, s, d, c: caps(r, d), layout });
}

// ---------- SCHITE/INDEX.md
const fam = {};
for (const x of rows.filter(x => x.m)) { const k = x.r.family || 'altele'; (fam[k] = fam[k] || []).push(x); }
let md = `# Index schite VERCON (SLICE-VC00-04)\n\nImagini PNG generate din exporturile \`FORMS\`/\`REPORTS\` de \`tools/schite.js\` + \`tools/schite_render.ps1\` (parsarea proprietatilor \`Left/Top/Width/Height/Caption/ControlSource/SourceObject\`). **Nu sunt functionalitate implementata** si nu sunt designul web final: dimensiunile si ordinea din export sunt repere; adaptarea web/mobil este in VC02-02 si VC08-03. Elementele nerecuperabile din export (ribbon, userform \`ufTreeView\`, imagini, ActiveX, valorile calculate la rulare) sunt marcate in coloana Limite.\n\nFiecare schita are un fisier de trasabilitate cu **fiecare control** (nume, tip, text/camp/sursa, pozitie, linia din export, evenimente). Totaluri: ${rows.filter(x => x.m).length} schite (${rows.filter(x => x.m && x.r.type === 'form').length} formulare + ${rows.filter(x => x.m && x.r.type === 'report').length} rapoarte).\n\n**Verificare (11.10.2026):** toate cele 101 imagini au fost privite individual; un defect de randare (controale fara \`Height\` explicit, desenate cu dimensiune 0) a fost gasit la \`Fisa_Produsului\`, corectat prin folosirea \`LayoutCachedLeft/Top/Width/Height\` din export, apoi imaginile au fost regenerate. Dupa corectie am reprivit individual doar Fisa_Produsului si 25 de rapoarte privite abia dupa corectie; restul (aprox. 70, privite inainte de corectie) nu au fost reprivite una cate una si sunt acoperite de o verificare automata: nicio schita nu mai are controale de dimensiune 0 in afara celor 23 de mai jos. Verificarea automata arata 23 de controale vizibile cu dimensiune 0 in export (nu se deseneaza; apar in fisierul de trasabilitate): \`LABC\` (3), \`Consumuri_Produs\`/\`Consumuri_Unitati\` (2+2 etichete), \`Fisa_Evidenta\`/\`OI_Fisa_Evidenta\` (2+2 dreptunghiuri), \`FM1-u\` (1), \`NIR\`/\`NIR_Storno\` (2+2), \`nou_Furnizori\` (7). Defecte ale **sursei** vazute pe schite: \`OI_Fisa_Evidenta_Primitori\` si \`_STOC\` afiseaza perioada ca \`di - di\` (a doua data ar trebui sa fie \`dsf\`); \`Centralizator_OI_Bon_BS\` are eticheta „Numar BP" la casare; \`Plan_Achizitii\` are acelasi antet ca \`Situatie_Intrari\`; \`BR\` are titlul „Bonuri de Consum" (copie din BC). Web-ul corecteaza, nu reproduce.

Cititi imaginile cu atentie la suprapuneri: in Access mai multe controale se suprapun intentionat (pagini de tab, elemente cu vizibilitate conditionata, \`Visible\` setat din cod); schita le deseneaza pe toate.\n`;
for (const k of Object.keys(fam).sort()) {
  md += `\n## ${k}\n\n| Obiect | Schita | Sursa | Date | Sectiuni / controale | Limite |\n|---|---|---|---|---|---|\n`;
  for (const x of fam[k]) {
    const lim = [];
    if (x.r.type === 'form' && x.s.sub.length) lim.push('subformulare: ' + x.s.sub.join(', '));
    if (/^DE-CLARIFICAT/.test(x.r.disp)) lim.push('DE CLARIFICAT');
    if (x.m.controls === 0) lim.push('fara controale in export');
    md += `| ${x.r.type === 'form' ? 'formular' : 'raport'} **${x.r.name}** | [${x.m.slug}.png](${x.m.slug}.png) | [${x.r.file}](../VERCON_ACCESS/${encodeURI(x.r.file)}) · [controale](trasabilitate/${x.m.slug}.md) | ${[...x.d.t].slice(0, 8).join(', ')}${x.d.t.size > 8 ? ' …' : ''} | ${x.m.sections} / ${x.m.controls} | ${lim.join('; ') || '—'} |\n`;
  }
}
fs.writeFileSync(path.join(ROOT, 'SCHITE', 'INDEX.md'), md);

// ---------- MAPARE_ACCESS_WEB.md
let mp = `# VERCON — maparea machetelor Access la web (SLICE-VC00-04)\n\nGenerat de \`VERCON/tools/mapare.js\`. Starea: DOCUMENTAT. Capabilitatile sunt cele din PLAN (matricea functionala a gestiunilor) aplicate pe dependentele de date detectate static in exporturi (tabele/interogari citite transitiv, 4 nivele). **da** = disponibil; **nu** = nu se expune (si apelul direct este refuzat de server); **de verificat** = depinde de verificarea de la VC06/VC08.\n\nTipuri: MAT = Materiale/alt tip fara MF si OI; OI; MF; AL = Alimente.\n\n## Formulare\n\n| Formular | Familie | Dispozitie | MAT | OI | MF | AL | Date (tabele) | Interogari | Schita |\n|---|---|---|---|---|---|---|---|---|---|\n`;
for (const x of rows.filter(x => x.r.type === 'form')) {
  mp += `| ${x.r.name} | ${x.r.family || ''} | ${x.r.disp} | ${mark(x.c.MAT)} | ${mark(x.c.OI)} | ${mark(x.c.MF)} | ${mark(x.c.AL)} | ${[...x.d.t].join(', ')} | ${[...x.d.q].slice(0, 8).join(', ')}${x.d.q.size > 8 ? ' …' : ''} | ${x.m ? `[png](SCHITE/${x.m.slug}.png)` : '—'} |\n`;
}
mp += `\n## Reguli de capabilitate verificate la schita\n
- **FC**: formular comun; extensia MF (nr. inventar, data intrarii, durate, ProduseM, panoul „TABELA INTERACTIVA") apare numai la gestiunea MF. Sursa \`FC.Form_Load\` citeste \`MF\`/\`OBINV\`; schita arata ambele stari suprapuse.
- **BC**: nu apare pentru OI si MF; apel direct refuzat. Alimente acceseaza BC numai prin circuitul listei (traseul manual se stabileste la VC08-03).
- **OI_Predare / OI_Transfer / OI_Casare**: OI si MF; nu pentru MAT si AL (implicit).
- **ListaAlimente, LABC, LEGBCLA, Feluri, subformularele de mese**: numai AL.
- **BR / FCS**: disponibilitate si efect pe registru OI / activ MF de verificat la VC06/VC08.
- **Meniul SFP**: se construieste din \`tmpOptiuniSFP\` (MODEL, TIP) — metadatele **nu sunt in export** si au fost primite de la utilizator (vezi mai jos)
- **Forme de shell Access** (Main, VaerCon, subTreeView, MonthPicker, frmMail, frmPB, frmProgressBar) nu se portează; \`VaerCon\` si \`Main\` sunt schitate doar ca referinta pentru selectorul de context (VC02-01).

## Rapoarte

Vezi [RAPOARTE.md](RAPOARTE.md).

## Limite

- Detectia de dependente este textuala (nume de tabele/interogari in RecordSource/RowSource si in SQL), nu demonstreaza ca ramura este rulata.
- Formularele cu **subformulare** includ dependentele subformularului, dar controalele legate prin \`LinkChildFields\` nu sunt analizate.
- Starea de vizibilitate setata din VBA nu este in export.
`;
fs.writeFileSync(path.join(ROOT, 'MAPARE_ACCESS_WEB.md'), mp);

// ---------- RAPOARTE.md
let rp = `# VERCON — catalogul rapoartelor (SLICE-VC00-04)\n\nGenerat de \`VERCON/tools/mapare.js\`. Starea: DOCUMENTAT; nicio generare implementata (VC08-03). ${rows.filter(x => x.r.type === 'report').length} rapoarte in export; cele care refera Coduri sunt EXCLUSE (N06) si nu se investigheaza.\n\nColoane: **Familie** (din denumire), **Surse** (tabele citite transitiv prin interogarile din RecordSource), **Capabilitate** (MAT/OI/MF/AL: da/nu/de verificat), **Parametri din formular** (\`[Forms]![...]\`/\`TempVars\` in interogari — de inlocuit cu parametri expliciti), **Sectiuni** (inclusiv grupari), schita.\n\n| Raport | Familie | Dispozitie | MAT | OI | MF | AL | Surse | Interogari | Parametri Forms!/TempVars | Schita |\n|---|---|---|---|---|---|---|---|---|---|---|\n`;
for (const x of rows.filter(x => x.r.type === 'report')) {
  const params = new Set();
  const all = [x.s.text, ...[...x.d.q].map(q => queryText[q] || '')].join(' ');
  for (const m of all.matchAll(/\[?Forms\]?!\[?(\w+)\]?!\[?(\w+)\]?|\[?TempVars\]?!\[?(\w+)\]?/gi)) params.add(m[3] ? 'TempVars!' + m[3] : `${m[1]}.${m[2]}`);
  rp += `| ${x.r.name} | ${x.r.family || ''} | ${x.r.disp}${x.r.note ? ' (' + x.r.note + ')' : ''} | ${mark(x.c.MAT)} | ${mark(x.c.OI)} | ${mark(x.c.MF)} | ${mark(x.c.AL)} | ${[...x.d.t].join(', ')} | ${[...x.d.q].slice(0, 6).join(', ')}${x.d.q.size > 6 ? ' …' : ''} | ${[...params].join(', ')} | ${x.m ? `[png](SCHITE/${x.m.slug}.png)` : '—'} |\n`;
}
rp += `\n## Observatii\n
- Rapoartele **comune** (NIR*, Bon_Consum, BonRetur, fise, balante, centralizatoare, inventare, comenzi/referate) sunt oferite numai in variantele compatibile cu tipul; variantele de balanta (BF/AFB/furnizor) **nu sunt interschimbabile** si nu se presupune ca meniul curent le foloseste pe toate (VC08-03).
- Parametrii \`[Forms]!SFP.di/dsf/st\`, \`[Forms]!FC.IdIntrare\`, \`[Forms]!BC.*\`, \`TempVars!Zec\` din interogari devin parametri expliciti ai serviciilor (CONTRACT_CALCULE §3).
- Rapoartele de gestiune (OI/MF/Alimente) nu apar la alt tip. PDF/listare/export folosesc aceleasi drepturi si date ca ecranul.
`;
fs.writeFileSync(path.join(ROOT, 'RAPOARTE.md'), rp);
// ---------- sectiuni din dovezi noi (ribbon, meniu SFP, cod de intrare)
const reportNames = new Set(inv.filter(x => x.type === 'report').map(x => lc(x.name)));
const dispOf = (n) => { const o = inv.find(x => x.type === 'report' && lc(x.name) === lc(n)); return o ? o.disp : 'LIPSA din export'; };
const parentName = {}; for (const e of menu) if (e.Tot === '1') parentName[e.Parinte + '|' + e.Parinte1] = e.Explicatie;
let menuMd = `\n## Meniul SFP — \`tmpOptiuniSFP\` (furnizat de utilizator, ${menu.length} randuri; [XML](REFERINTE/tmpOptiuniSFP.xml), [XSD](REFERINTE/tmpOptiuniSFP.xsd))\n\n\`TIP\`: **G** = gestiune de materiale/alimente, **O** = obiecte de inventar, **M** = mijloace fixe; \`G-O-M\` = vizibil la toate. \`Raport\` = raportul deschis; \`RaportA\` = varianta alternativa; \`XLS\` = export Excel disponibil; \`TMPL\` = sablon Excel din \`TMPLS\` (**sabloanele .tmpl nu se portează**: decizie 11.10.2026, export .xlsx simplu pe fiecare raport). Randurile cu \`Tot=1\` sunt titluri de grup. Compararea numelor se face fara diferenta de litere (Access).\n\n| ID | Grup | Optiune | TIP | Raport | Disp. | RaportA | Disp. | XLS | Sablon |\n|---:|---|---|---|---|---|---|---|---|---|\n`;
for (const e of menu) {
  if (e.Tot === '1') { menuMd += `| ${e.ID} | ${e.Parinte1} | **${e.Explicatie.trim()}** (grup) | ${e.TIP} | | | | | | |\n`; continue; }
  menuMd += `| ${e.ID} | ${e.Parinte1} | ${e.Explicatie.trim()} | ${e.TIP} | ${e.Raport || ''} | ${e.Raport ? dispOf(e.Raport) : ''} | ${e.RaportA || ''} | ${e.RaportA ? dispOf(e.RaportA) : ''} | ${e.XLS === '1' ? 'da' : ''} | ${e.TMPL || ''} |\n`;
}
const inMenu = new Set(); for (const e of menu) for (const k of ['Raport', 'RaportA']) if (e[k]) inMenu.add(lc(e[k]));
const notInMenu = inv.filter(x => x.type === 'report' && !inMenu.has(lc(x.name)));
const missing = [...inMenu].filter(n => !reportNames.has(n));
const dup = {}; for (const e of menu) if (e.Raport) (dup[lc(e.Raport)] = dup[lc(e.Raport)] || []).push(e.ID);
menuMd += `\n**Constatari din meniu (dovada, comparatie automata):**\n\n- Rapoarte din meniu care **nu exista** in export: ${missing.length ? missing.join(', ') : 'niciunul'}.\n- Rapoarte din export **fara intrare in meniul SFP** (${notInMenu.length}): ${notInMenu.map(x => x.name + ' [' + x.disp + ']').join(', ')}. Ele sunt deschise din alte locuri (butoane ale formularelor, subrapoarte, \`IA\`) sau sunt orfane; se verifica la VC08-03.\n- Aceeasi optiune pe doua intrari: ${Object.entries(dup).filter(([, v]) => v.length > 1).map(([k, v]) => k + ' (ID ' + v.join(', ') + ')').join('; ') || 'niciuna'}. Intrarea 107 „Balanta produse / furnizor" deschide **Centralizator_Furnizori**, la fel ca 106: probabil eroare de configurare (de confirmat; **Balanta_produse_furnizor nu este referit in meniu**).\n- Variantele alternative \`Balanta_Produse_BF\` (ID 2) si \`Fisa_Magazie_BF\` (ID 1) sunt \`RaportA\`; \`Balanta_Produse_BF\` depinde de Coduri (exclus), \`Fisa_Magazie_BF\` nu.\n- Optiunile cu TIP \`G-O\` (Situatie intrari anuale, Plan anual de achizitii) **nu** apar la MF.\n`;
fs.appendFileSync(path.join(ROOT, 'RAPOARTE.md'), menuMd);

const ribbon = [
  ['Configurari', 'Adauga produse', 'btnAddProd', 'Produse', 'toate'], ['Configurari', 'Categorii / Conturi', 'btnCategorii', 'Masini', 'toate'], ['Configurari', 'Furnizori', 'btnFurnizori', 'Furnizori2022', 'toate'], ['Configurari', 'Gestionari', 'btnClienti', 'Clienti', 'toate'], ['Configurari', 'Unitate', 'btnUnitate', 'SETARI', 'toate'],
  ['Configurari', 'Editare coduri', 'btnCoduri', 'frmPB + UpDateP + IntrariIesiriTotal (Coduri)', 'EXCLUS N06'], ['Configurari', 'Inchidere an', 'btnInchidere', 'IA', 'nu la OI si MF (decizie utilizator)'], ['Configurari', 'Retetar', 'btnRetetar', '(fara actiune in OnActionButton: „Lipseste actiunea")', 'neimplementat in Access'],
  ['Configurari / Online', 'Incarca unitate', 'btnUPDN', 'frmUpl (formular absent din export)', 'infrastructura, exclus'], ['Configurari / Online', 'Creeaza backup', 'btnBck', 'frmMail', 'infrastructura, exclus'],
  ['Gestiuni / Intrari', 'Adaugare / Editare N.I.R.', 'btnAddNIR', 'FC', 'toate'], ['Gestiuni / Intrari', 'Adaugare / Editare Storno', 'btnAddStorno', 'FCS', 'toate (cod nu blocheaza)'],
  ['Gestiuni / Iesiri', 'Bon de consum', 'btnAddBC', 'BC', 'nu la OI/MF (cod)'], ['Gestiuni / Iesiri', 'Adaugare / Editare Retur', 'btnAddRE', 'BR', 'nu la OI/MF (cod)'], ['Gestiuni / Iesiri', 'Lista de alimente', 'btnAddLA', 'ListaAlimente', 'nu la OI/MF (cod)'],
  ['OI / MF', 'Bon de predare', 'btnAddBP', 'OI_Predare', 'numai OI/MF (cod)'], ['OI / MF', 'Bon de transfer', 'btnAddBT', 'OI_Transfer', 'numai OI/MF (cod)'], ['OI / MF', 'Bon de casare', 'btnAddBS', 'OI_Casare', 'numai OI/MF (cod)'], ['OI / MF', 'Lista Mijloace Fixe', 'btnListMF', '(fara actiune in OnActionButton: „Lipseste actiunea")', 'neimplementat in Access'],
  ['Rapoarte', 'TOATE RAPOARTELE', 'btnLstSFP', 'SFP', 'meniu dupa TIP'],
];
let rb = `\n## Ribbonul „Vercon" (furnizat de utilizator; [XML](REFERINTE/ribbon_Vercon.xml))\n\nLa pornire ribbonul este \`startFromScratch\`; **vizibilitatea si activarea fiecarui buton vin din tabelul \`Rbb\` (coloanele \`N\`, \`V\`, \`E\`)** prin \`GetVisible\`/\`GetEnabled\` (\`basRibbonCallbacks\`). Codul exportat nu scrie in \`Rbb\` nicaieri; **continutul tabelului a fost primit de la utilizator** ([Rbb.xml](REFERINTE/Rbb.xml), [Rbb.xsd](REFERINTE/Rbb.xsd), 49 randuri, 11.10.2026): toate butoanele si grupurile au \`V=1, E=1\`; singura exceptie este tabul \`Tab_MF\` (\`V=0, E=0\`, fila MF este ascunsa, MF foloseste fila „Obiecte de Inventar / Mijloace Fixe") — **ribbonul nu diferentiaza dupa tip prin date**; diferentierea o face doar \`Form_Open\`. \`Rbb\` mai contine butoane absente din ribbonul primit (\`btnDelNir\`, \`btnDelBC\`), elementele ribbonului de previzualizare la tiparire (\`ExportExcel\`, \`FilePrintQuick\`, \`FileSaveAsPdfOrXps\`, \`PageSetupDialog\`, \`PrintPreviewClose\`, \`tabPPVW\`) si, in coloana \`C\`, \`gest_CPV\` (la \`btnAddProd\`), \`Masini\` (\`btnCategorii\`), \`Config\` (\`btnUnitate\`); codul deschide insa \`produse\`, \`Masini\` si \`SETARI\`, deci coloana \`C\` nu este folosita de \`OnActionButton\`. Reguli de gestiune sunt insa impuse si in \`Form_Open\` al formularelor (mai jos), deci nu depind de ribbon.\n\n| Tab / grup | Buton | Id | Deschide | Disponibilitate (dovada) |\n|---|---|---|---|---|\n`;
for (const r of ribbon) rb += `| ${r[0]} | ${r[1]} | ${r[2]} | ${r[3]} | ${r[4]} |\n`;
rb += `\nNote: \`OnActionButton\` mai contine ramuri pentru butoane care **nu sunt in ribbonul furnizat** (\`btnAmmy\`, \`btn_AddMF\`, \`btn_Amortizare\`, \`btn_Casari\`, \`btn_Restaurare\`), spre formulare absente din export (\`Mijloace Fixe\`, \`Amortizare\`, \`Casari\`, \`MFIesite\`): cod mort sau ribbon mai vechi. Butonul „Gestionari" deschide formularul \`Clienti\`; formularul \`Gestionari\` din export nu este deschis de ribbon. \`Furnizori\` din ribbon deschide \`Furnizori2022\`.\n\n## Reguli din cod (confirmate in \`Form_Open\` / \`Form_Load\`)\n\n| Formular | Regula | Linia in export |\n|---|---|---|\n| BC | Refuza daca \`Setari.OBINV\` sau \`Setari.MF\` (mesaj „baza de date pentru Obiecte de Inventar / Mijloace Fixe… nu se pot adauga Bonuri de CONSUM") | Forms/BC.txt:2639 |\n| BR | idem BC | Forms/BR.txt:2436 |\n| ListaAlimente | idem BC (deci este permisa si la o baza fara Alimente) | Forms/ListaAlimente.txt:2473 |\n| OI_Predare / OI_Transfer / OI_Casare | Refuza daca **nici** OBINV **nici** MF | OI_Predare.txt:2756, OI_Transfer.txt:2449, OI_Casare.txt:2355 |\n| IA | Access refuza **doar** daca OBINV („nu se face inchidere anuala"); **decizie utilizator 11.10.2026: OI si MF NU se inchid anual** (la MF Access nu o impiedica, web o interzice) | Forms/IA.txt:896 |\n| FC | Citeste \`MF\` si \`OBINV\` ca \`isMF\`/\`isOI\` si adapteaza ecranul | Forms/FC.txt:4461 |\n| Main | La incarcare, daca \`UNIT\` este goala, o completeaza din \`CaleTemp\` (Firma, CUI, DC, Detalii) | Forms/Main.txt:180 |\n\nConcluzie: in Access exista trei situatii efective determinate de \`Setari\`: **OI** (\`OBINV\`), **MF** (\`MF\`) si **restul** (fara steaguri). **Alimente vs Materiale nu este stabilit de niciun steag**: formularul \`ListaAlimente\` este permis oricarei baze fara OBINV/MF. Diferenta apare doar in date (ex.: ${'`ListaAlimente`'} are 127 randuri in baza Alimente si 0 in MF si OI). Tipul web \`ALIMENTE\` vs \`MATERIALE\` este deci o alegere a operatorului in migrator.\n\n## Intrarea in aplicatie — \`VaerCon.Intrare_Click\` (Forms/VaerCon.txt:1697)\n\nOrdinea executata la „Intrare" (confirmat in cod):\n\n1. Citeste randul ales din \`cale\` (\`Combo1\` = ID; coloana 2 = folderul \`Cale\`); \`AnDate\` obligatoriu (altfel „Vercon 3 deschide doar bazele incepand cu 2022").\n2. Fisierul este \`<folderul aplicatiei><Cale>\\tz_<AnDate>.accdb\`; daca lipseste dar exista \`tz.accdb\`, il **redenumeste** in \`tz_<AnDate>.accdb\` (si \`tz_vechi.accdb\` in \`tz.accdb\`); altfel „Lipseste baza de date pentru anul selectat!".\n3. Completeaza \`Caletemp\` (ID, CaleTemp = folder, Firma, ANL = an, DC, CaleReala = fisier, Detalii, Gestionar = \`cale.Reprezentant\`, CUI).\n4. \`Repara2024\` (repara/actualizeaza schema backendului: **exclus**), apoi \`CnnTbl_2022\` (leaga tabelele din fisier).\n5. \`TempVars\`: separatorul zecimal si **\`Zec = Nz(DLookup("ZEC","NR"),4)\`** (fallback 4 si la eroare).\n6. \`Setari\` (UseTreescroll), \`SalvezSiguranta\` (ZIP local in C:\\SIG), \`Functii\` (reparatii unice din \`RunOnce\`), verificare backup online (>10 zile, doar anul curent), deschide \`Main\` ascuns.\n\n**Ce rezulta pentru atribuirea in migrator (dovada):** un rand din \`cale\` este o pereche (folder, an): \`IA\` creeaza pentru anul urmator un rand nou cu acelasi \`Cale\`, \`DC\`, \`Detalii\`. Deci **gestiunea in Access = folderul \`cale.Cale\`** (+ \`Detalii\`), iar fisierele \`tz_<an>.accdb\` din folder sunt exercitiile ei. **Subunitatea nu exista in Access**; se atribuie in \`VRC.Migrator\`. \`cale.DC\` este codul DC; in \`cale.accdb\` furnizat toate cele 7 randuri au DC \`00000000\` (valoare-semnal, nu un DC real).\n\n| Rand \`cale\` | Folder | An | Observatie |\n|---:|---|---:|---|\n| 1 si 12 | \\ISJ2020 | 2024 | doua randuri pentru acelasi folder si an |\n| 11 | \\14 | 2024 | folderul pentru care frontendul leaga \`14\\tz_2024.accdb\` |\n| 13 | \\OI_MONNET | 2024 | |\n| 14 | \\AUTO | 2024 | |\n| 15 | \\14 | 2100 | an 2100: codul nu il trateaza special (ar cauta \`tz_2100.accdb\`); **ipoteza**: marcaj pentru baza fara an |\n| 16 | \\MF | 2100 | idem |\n\nToate au \`Detalii = Manuale\` (\`UNIT\` il suprascrie pe toate randurile din \`AlteDetalii\`). Fisierele de pe disc nu au denumirile din \`cale\`: \`tz_Alimente.accdb\`, \`tz_Mijloace_Fixe.accdb\`, \`tz_Obiecte_Inventar.accdb\` sunt nume puse la export; legatura cu folderele \\14, \\MF, \\OI_MONNET, \\AUTO, \\ISJ2020 o stabileste operatorul in migrator, la import.\n`;
fs.appendFileSync(path.join(ROOT, 'MAPARE_ACCESS_WEB.md'), rb);
console.log('forms', rows.filter(x => x.r.type === 'form').length, 'reports', rows.filter(x => x.r.type === 'report').length, 'menu', menu.length, 'missing', missing.join(','), 'notInMenu', notInMenu.length);
