// VC00-01: generates VERCON/INVENTAR_OBIECTE.md + tools/out/inventar.json from VERCON_ACCESS (read-only).
// Static classification by name rules + content markers. Not proof of production use.
const fs = require('fs'), path = require('path');
const ROOT = path.join(__dirname, '..');
const SRC = path.join(ROOT, 'VERCON_ACCESS');
const rd = (p) => { try { return fs.readFileSync(p, 'utf8'); } catch (e) { return ''; } };

const D = {
  BIZ: 'INCLUS-DATE', CFG: 'INCLUS-CONFIG', NOM: 'INCLUS-NOMENCLATOR', UI: 'INCLUS-UI', RPT: 'INCLUS-RAPORT',
  CALC: 'INCLUS-CALCUL', DER: 'DERIVAT', TMP: 'TEMP-LUCRU', MIX: 'INCLUS-PARTIAL',
  XEF: 'EXCLUS-EFACTURA', XCOD: 'EXCLUS-CODURI', XSCH: 'EXCLUS-SCHEMA-ACCESS', XINF: 'EXCLUS-INFRA-ACCESS', CLR: 'DE-CLARIFICAT',
};
const rules = {
  table: [
    [/^(@EF|@EFS|@EFT|EF|EFS|EFT|EF_TMP|tmpXMLPath)$/i, D.XEF, 'e-Factura (N13)'],
    [/^(tmpCoduri|tmp_Coduri)$/i, D.XCOD, 'sistem Coduri (N06)'],
    [/^(_TBLS_|_Struct_|_IDX_|_REL_|_ADODAODDL|_QRY|_tblQry|_tblRel|_TBLS_Transfer|SQL|SQL_|SQL_FIX_BD|SQL_FIX_BD_SUB|tSQL|RunOnce|@RunOnce|VEROFFLINE|VER_RAP|EXT_IDX)$/i, D.XSCH, 'mecanism schema Access (DBS/clsDatabase)'],
    [/^(@ComisieInventar|@ProduseM|@Setari|@UNIT|@Trimis)$/i, D.XSCH, 'sablon structura "@" folosit de mecanismul de schema Access'],
    [/^(Ver)$/i, D.XSCH, 'versiune schema Access; citit doar ca diagnostic daca exista'],
    [/^(tblFTP|tblFiles|tblPopupMsg|Trimis|_Mail|Rbb|tmpRbb|TipFirmeL)$/i, D.XINF, 'FTP/backup/ribbon/popup/mail/ANAF-firme: infrastructura desktop'],
    [/^(NIR|Intrari|IntrariStorno|Iesiri_H|Iesiri|IesiriRetur|OI_Predare|OI_Transfer|OI_Casare|OI_Produse|ProduseM|LABC|LegBCLA|ListaAlimente|MicDejun|Dejun|Cina)$/i, D.BIZ, 'date business istorice'],
    [/^(Produse|ProduseP|Masini|Furnizori|Clienti|ComisieNIR|ComisieInventar|TVA|FeluriMancare|Retetar|Feluri|Alimente)$/i, D.NOM, 'nomenclator/registru business'],
    [/^(cale|Caletemp|UNIT|Setari|NR)$/i, D.CFG, 'identitate/configurare; mapare pe comune/VC_Settings (nu copiere)'],
    [/^(cpv|CPV1|Clasificari|gest_subCPV|Jud)$/i, D.CLR, 'nomenclator local frontend; verificare date efective si disponibilitate in comune'],
    [/^(TmpFact|TmpFactV|OI_TmpFact|tmpProduseM|tmpQFC|tmp_BC_DenProdus|tmp_LABC|tmp_LEGBCLA|tmp_II|Tmp_Cina|Tmp_Dejun|Tmp_Lista|Tmp_MicDejun|tmpFMANTET|tmpFM_2|FM_2|tmpOI)$/i, D.TMP, 'scratch Access; drafturile se reconstruiesc izolat pe utilizator'],
    [/^(tmpSFP|tmp_Balanta)$/i, D.DER, 'rezultat de calcul/raport'],
    [/^(tmpOptiuniSFP)$/i, D.CLR, 'metadate meniu SFP (MODEL/TIP): datele trebuie recuperate, nu doar schema'],
    [/^(SFP_CautCBX|TblExcel|XCX|x|Erori Intrare|Erori Intrare Tbl|Erori_Intrare)$/i, D.CLR, 'nume/rol neclar; profilare date si utilizare'],
  ],
  form: [
    [/^(EFACTURA|XMLPath|XMLPath_sub|Mesaje_ANAF)$/i, D.XEF, 'e-Factura / mesaje ANAF'],
    [/^(AdaugaTabelInTBLS)$/i, D.XSCH, 'mecanism schema Access'],
  ],
};

// explicit per-object notes for forms/reports/others (family + treatment)
const formMap = {
  FC: ['FC', 'Recepție FC/NIR (+extensie MF)', D.UI, 'Contine cod legacy Coduri in ramuri exclusive: ignorat.'],
  TmpFact: ['FC', 'subform linii FC (sursa TmpFact)', D.UI, ''], TmpFactMF: ['FC', 'subform linii FC MF', D.UI, ''],
  TmpFactV: ['FC', 'variante produs in FC', D.UI, ''], OI_TmpFact: ['OI', 'linii OI temporare', D.UI, ''],
  'ComisieNIR subform': ['Nomenclatoare', 'comisie NIR', D.UI, ''], 'ComisieINV subform': ['Nomenclatoare', 'comisie inventar', D.UI, ''],
  BC: ['BC', 'Bon de consum', D.UI, 'Setarea FIFO legacy nu reactiveaza Coduri.'], BCP: ['BC', 'BC pe produs (de clarificat)', D.CLR, ''],
  ALEGE_BC: ['BC', 'alegere BC (selector)', D.UI, ''], ALEGE_BC_RETUR: ['BR', 'alegere BC pentru retur', D.UI, ''],
  BR: ['BR', 'Bon retur', D.UI, ''], FCS: ['FCS', 'Storno FC', D.UI, ''],
  OI_Predare: ['OI', 'Predare OI/MF', D.UI, ''], OI_Transfer: ['OI', 'Transfer OI/MF', D.UI, ''], OI_Casare: ['OI', 'Casare OI/MF', D.UI, ''],
  ListaAlimente: ['Alimente', 'Lista alimente', D.UI, ''], LABC: ['Alimente', 'Lista alimente-BC', D.UI, ''], LEGBCLA: ['Alimente', 'Legatura BC-lista alimente', D.UI, ''],
  ListaAlimente_copy: ['Alimente', 'copie ListaAlimente (de clarificat)', D.CLR, ''], Feluri: ['Alimente', 'feluri de mancare', D.UI, ''],
  Produse: ['Nomenclatoare', 'Produse', D.UI, ''], ProduseM: ['Nomenclatoare', 'Produse MF', D.UI, ''], Furnizori: ['Nomenclatoare', 'Furnizori', D.UI, 'Verificare CF ANAF din formular: neportata (nu e e-Factura; de clarificat).'],
  Furnizori2022: ['Nomenclatoare', 'Furnizori 2022 (de clarificat)', D.CLR, ''], Clienti: ['Nomenclatoare', 'Clienti/detinatori', D.UI, ''],
  Masini: ['Nomenclatoare', 'Categorii/conturi (Masini)', D.UI, ''], Masini_sub: ['Nomenclatoare', 'subform Masini', D.UI, ''],
  Gestionari: ['Configurare', 'Gestionari (RecordSource Gestionari; caption Clienti)', D.CLR, 'tabelul Gestionari nu apare in export; de clarificat'], SETARI: ['Configurare', 'Setari (precizie etc.)', D.UI, ''], UNIT: ['Configurare', 'Date unitate (partial e-Factura: cheile EFAUTH excluse)', D.MIX, ''],
  Config: ['Configurare', 'GESTIUNI: modificare date unitate', D.UI, 'mapare pe VC_Gestiuni/VC_Settings'],
  SFP: ['Situatii', 'Situatii financiare/rapoarte (meniu dinamic)', D.UI, 'Meniu din tmpOptiuniSFP + MODEL.'], tmpSFP: ['Situatii', 'subform SFP', D.UI, ''], 'tmp_Balanta subform': ['Situatii', 'subform balanta', D.UI, ''],
  IA: ['Inchidere an', 'Inchidere/initializare an', D.UI, 'NU se executa pe originale.'],
  Main: ['Shell', 'meniu principal Access', D.XINF, 'navigare Access; inlocuita de portal'], VaerCon: ['Shell', 'formular de start/selectie unitate-an', D.XINF, 'inlocuit de selectorul de context; regulile de context se mapeaza'],
  subTreeView: ['Shell', 'TreeView ActiveX', D.XINF, 'componenta UI Access'], MonthPicker: ['Shell', 'selector luna', D.XINF, 'inlocuit de calendarul comun'],
  frmMail: ['Shell', 'trimite backup FTP', D.XINF, 'backup/FTP'], frmPB: ['Shell', 'progress bar', D.XINF, ''], frmProgressBar: ['Shell', 'progress bar', D.XINF, ''],
  D: ['Alimente', 'subform mese: Dejun (Tmp_Dejun)', D.UI, ''], MD: ['Alimente', 'subform mese: Mic dejun (Tmp_MicDejun)', D.UI, ''], CN: ['Alimente', 'subform mese: Cina (Tmp_Cina)', D.UI, ''],
  CLS2024: ['Nomenclatoare', 'CPV Vercon 2017 (selector)', D.UI, 'depinde de decizia D08 (CPV comun)'], CPV2017: ['Nomenclatoare', 'CPV Vercon 2017 (selector)', D.UI, 'depinde de D08'],
};
const rptMap = (n) => {
  if (/^(NIR|NIR_SFP|NIR_Storno)$/.test(n)) return ['NIR', D.RPT];
  if (/^(Bon_Consum|BonRetur|BUConsum.*|BC subreport 2013)$/.test(n)) return ['BC/BR', D.RPT];
  if (/^OI_(Predare|Transfer|Casare)$/.test(n)) return ['OI documente (formular-raport)', D.RPT];
  if (/^OI_Bon_/.test(n) || /^Centralizator_OI/.test(n)) return ['OI bonuri', D.RPT];
  if (/^OI_/.test(n)) return ['OI evidenta/inventar', D.RPT];
  if (/^MF_/.test(n)) return ['MF', D.RPT];
  if (/^(ListaAlimente2015|Cina subreport|Dejun subreport|MicDejun subreport)$/.test(n)) return ['Alimente', D.RPT];
  if (/^(Fisa_|FM1-u|FM2|Balanta|Stoc_Final|LISTA_INVENTAR)/.test(n)) return ['Stoc/fise/balante/inventar', D.RPT];
  if (/^Centralizator|^Consumuri/.test(n)) return ['Centralizatoare', D.RPT];
  if (/^(Nota_Comanda|Plan_Achizitii|Referat|Situatie_Intrari|nou_Furnizori|Amortizare)/.test(n)) return ['Comenzi/referate/situatii', D.RPT];
  return ['de clarificat', D.CLR];
};

const refMarkers = [[/\bCoduri\b/i, 'refera Coduri'], [/\bEF(S|T)?\b|EFactura|e-?Factura|XMLPath/i, 'refera e-Factura'], [/clsDatabase|_TBLS_|_Struct_|Fix_Database/i, 'refera mecanism schema Access'], [/WinSCP|clsWinSCP|FTP/i, 'refera FTP'], [/ANAF|clsFirma/i, 'refera ANAF']];

const rows = [];
function rule(list, name) { for (const [re, d, why] of list) if (re.test(name)) return [d, why]; return null; }

// tables
for (const f of fs.readdirSync(path.join(SRC, 'tables'))) {
  const n = f.replace(/\.md$/, ''); const txt = rd(path.join(SRC, 'tables', f));
  const linked = /Linked|Connect|Source:/i.test(txt);
  const r = rule(rules.table, n) || [D.CLR, 'neclasificat prin regula; profilare'];
  rows.push({ type: 'table', name: n, file: 'tables/' + f, disp: r[0], note: r[1], fields: (txt.match(/^\| \d+ \|/gm) || []).length, linked });
}
// queries
const qExc = (n, t) => {
  if (/\bCoduri\b/i.test(t)) return [D.XCOD, 'refera Coduri (N06)'];
  if (/^(Query1|temp1|tempQ|_TEMP_Actualizeaza_BC_IDFV)$/i.test(n)) return [D.CLR, 'interogare de lucru/temporara'];
  if (/^(SFP_|Balanta22_IA|Balanta_inchidere_an)/i.test(n)) return [D.CALC, 'calcul actual'];
  return [D.CALC, 'calcul/sursa de raport actual; parametri Forms!/TempVars de extras (VC00-03)'];
};
for (const f of fs.readdirSync(path.join(SRC, 'queries'))) {
  const n = f.replace(/\.sql$/, ''); const t = rd(path.join(SRC, 'queries', f)); const r = qExc(n, t);
  rows.push({ type: 'query', name: n, file: 'queries/' + f, disp: r[0], note: r[1], usesForms: /\[?Forms\]?!|TempVars/i.test(t) });
}
// forms
for (const f of fs.readdirSync(path.join(SRC, 'Forms'))) {
  const n = f.replace(/\.txt$/, ''); const t = rd(path.join(SRC, 'Forms', f));
  let e = formMap[n]; let r = rule(rules.form, n);
  if (/^(EFACTURA|XMLPath|XMLPath_sub|Mesaje_ANAF|AdaugaTabelInTBLS)$/.test(n)) e = [r[1], r[1], r[0], ''], e = ['Exclus', r[1], r[0], ''];
  if (!e) e = ['de clarificat', '', D.CLR, ''];
  rows.push({ type: 'form', name: n, file: 'Forms/' + f, family: e[0], desc: e[1], disp: e[2], note: e[3], marks: refMarkers.filter(([re]) => re.test(t)).map(x => x[1]) });
}
// reports
for (const f of fs.readdirSync(path.join(SRC, 'Reports'))) {
  const n = f.replace(/\.txt$/, ''); const t = rd(path.join(SRC, 'Reports', f)); const r = rptMap(n);
  let disp = r[1], note = '';
  if (/\bCoduri\b/i.test(t)) { disp = D.XCOD; note = 'refera Coduri'; }
  rows.push({ type: 'report', name: n, file: 'Reports/' + f, family: r[0], disp, note, marks: refMarkers.filter(([re]) => re.test(t)).map(x => x[1]) });
}
// modules
const modRule = (n) => {
  if (/EFactura/.test(n)) return [D.XEF, 'e-Factura'];
  if (/^(mdl_2024|mdl_LegTabele|mdl_CascadeToNull)/.test(n)) return [D.XSCH, 'migrare/legare schema Access'];
  if (/^(mdl_Autoexec|mdl_UpdateFE|mdl_siguranta|mdl_FTP|mdl_Zip_Unzip|mdl_Detect7Zip|mdl_ExecWait|mdl_compact|mdl_Retea|mdl_ChangeRemovePWD|mdl_CryptDecrypt|BCrypt|BStorage|basGDIPlus|basRibbon|mdl_64Bits|mdl_MouseCursor|mdl_MouseScroll|mdl_PositionForm|mdl_PozitieForm|mdl_Timer|mdl_WindowsVersion|mdl_DISKRW|mdl_debugPrint|mdl_ExistaFunctia|mdl_Popup2022|newProgressBar|modStartup|mdl_AppendText|mdl_HTML|mdl_JSON|mdl_ListSeparator)/.test(n)) return [D.XINF, 'infrastructura Access/desktop (update, FTP, arhivare, GDI, ribbon, API Windows); nu e functie web'];
  if (/^mdl_VerificaCF/.test(n)) return [D.CLR, 'verificare CF ANAF; neportat, de clarificat'];
  if (/^(mdl_AdaugStocFM|mdl_OI_Produse|mdl_FormatNumber|mdl_Export_Excel)/.test(n)) return [D.CALC, 'logica de business/export de analizat (N09 pentru AdaugStocFM)'];
  if (/^(mdl_EditData|mdl_Functii|Utile)/.test(n)) return [D.MIX, 'amestec: functii business si infrastructura; cod Coduri/legacy ignorat; extragere pe apel'];
  return [D.CLR, ''];
};
for (const f of fs.readdirSync(path.join(SRC, 'Modules'))) {
  const n = f.replace(/\.bas\.txt$/, ''); const t = rd(path.join(SRC, 'Modules', f)); const r = modRule(n);
  rows.push({ type: 'module', name: n, file: 'Modules/' + f, disp: r[0], note: r[1] + (/\bCoduri\b/i.test(t) ? '; contine referinte Coduri' : '') });
}
// classes
const clsRule = (n) => {
  if (/^(clsDatabase|clsDatabaseError|clsDatabaseErrors|clsField|clsIndex|clsRelation|clsTable|clsQuery)$/.test(n)) return [D.XSCH, 'DBS/clsDatabase si clasele copil'];
  if (/^(clsWinSCP|clsWinScpFile)$/.test(n)) return [D.XINF, 'FTP/WinSCP'];
  if (/^clsFirma/.test(n) || n === 'clsFirme') return [D.CLR, 'client ANAF (informatii firma); neportat, nu e e-Factura'];
  return [D.XINF, 'utilitar UI/Windows (regex, treeview, popup, timer, ping, form window)'];
};
for (const f of fs.readdirSync(path.join(SRC, 'Classes'))) {
  const n = f.replace(/\.cls\.txt$/, ''); const r = clsRule(n);
  rows.push({ type: 'class', name: n, file: 'Classes/' + f, disp: r[0], note: r[1] });
}
for (const f of fs.readdirSync(path.join(SRC, 'Macros'))) rows.push({ type: 'macro', name: f.replace(/\.txt$/, ''), file: 'Macros/' + f, disp: D.XINF, note: 'pornire Access (Autoexec_ apeleaza Productie/AExec: nu se executa)' });


// ---- overrides from evidence gathered after the first pass (referrers: tools/referenti.js; ribbon + tmpOptiuniSFP + VaerCon code from the operator)
D.NEREF = 'NEREFERIT-NEPORTAT';
D.XPAR = 'INLOCUIT-PARTENERI-KBOT';
const ov = {
  'table:Clasificari': [D.NOM, 'folosit de FC, Produse, CLS2024 (formulare active); date de profilat'],
  'table:cpv': [D.NOM, 'nomenclator CPV in backend (9.454 randuri in baza Alimente), folosit de Produse; D08'],
  'table:CPV1': [D.NOM, 'folosit de formularul CPV2017 (deschis din Produse)'],
  'table:Jud': [D.NOM, 'lista judete, folosita de formularul UNIT'],
  'table:Erori Intrare Tbl': [D.XINF, 'jurnal de erori al mecanismului de schema Access (referit doar de VaerCon in cod comentat, clsDatabaseErrors, newProgressBar)'],
  'table:Erori Intrare': [D.XINF, 'idem'], 'table:Erori_Intrare': [D.XINF, 'jurnal de erori clsDatabaseErrors (DBS)'],
  'table:XCX': [D.XINF, 'tabel de lucru al modulului de criptare'],
  'table:SFP_CautCBX': [D.TMP, 'sursa casetei de cautare din SFP'],
  'table:TblExcel': [D.TMP, 'tabel de lucru al exportului Excel (mdl_Export_Excel)'],
  'table:tmpOptiuniSFP': [D.CFG, 'meniul SFP (MODEL/TIP/Raport); DATE PRIMITE de la utilizator: REFERINTE/tmpOptiuniSFP.xml; se transforma in catalog de rapoarte (VC_ReportCatalog)'],
  'table:x': [D.NEREF, 'tabel de lucru fara nicio referinta in export'],
  'table:gest_subCPV': [D.NEREF, 'nicio referinta in export'],
  'query:Query1': [D.NEREF, 'INSERT in TmpFact; nicio referinta in export'],
  'query:temp1': [D.NEREF, 'coloane inexistente in schema curenta (Intrari.CodProdus); nicio referinta'],
  'query:tempQ': [D.NEREF, 'sursa de lista cu REȚETAR pentru ListaAlimente (Forms!ListaAlimente!mdv); nicio referinta'],
  'query:_TEMP_Actualizeaza_BC_IDFV': [D.NEREF, 'UPDATE unic pe coloane inexistente (Iesiri.IdIntrare/IDF); nicio referinta'],
  'form:BCP': [D.UI, 'fereastra de alegere produs pentru BC (pozitionata langa Denprodus, BC.txt:2590); BC'],
  'form:Furnizori2022': [D.XPAR, 'Furnizorii VERCON = Parteneri KBOT (decizie 11.10.2026): legare dupa CodFiscal; nu se migreaza furnizorii fara inregistrari; «Stoc initial» nu este furnizor real; completarea dupa CUI din ANAF nu se portează'],
  'form:Furnizori': [D.XPAR, 'forma veche de furnizori; inlocuita de Parteneri KBOT'],
  'table:Furnizori': [D.XPAR, 'Furnizorii VERCON = Parteneri KBOT (decizie 11.10.2026): legare dupa CodFiscal; nu se migreaza furnizorii fara inregistrari; «Stoc initial» nu este furnizor real'],
  'form:Gestionari': [D.NEREF, 'nedeschis din ribbon/cod; sursa „Gestionari" nu este tabel in export; ribbonul „Gestionari" deschide Clienti'],
  'form:ListaAlimente_copy': [D.NEREF, 'copie a ListaAlimente, nereferita'],
  'module:mdl_VerificaCF': [D.XPAR, 'verificare CF/ANAF a furnizorului; Partenerii sunt ai KBOT'],
};
for (const r of rows) { const o = ov[r.type + ':' + r.name]; if (o) { r.disp = o[0]; r.note = o[1]; } }
const clsF = rows.filter(r => r.type === 'class' && /^clsFirma|^clsFirme$/.test(r.name));
for (const r of clsF) { r.disp = D.XPAR; r.note = 'client ANAF pentru furnizor (CUI); Partenerii sunt ai KBOT, nu se portează'; }
require("./inventar_trans.js")(rows, D, SRC, rd);
fs.mkdirSync(path.join(__dirname, 'out'), { recursive: true });
fs.writeFileSync(path.join(__dirname, 'out', 'inventar.json'), JSON.stringify(rows, null, 1));

const count = {}; for (const r of rows) { count[r.type] = (count[r.type] || 0) + 1; }
const byDisp = {}; for (const r of rows) { byDisp[r.disp] = (byDisp[r.disp] || 0) + 1; }
let md = `# VERCON — inventarul obiectelor Access (SLICE-VC00-01)\n\nGenerat de \`VERCON/tools/inventar.js\` din \`VERCON_ACCESS\` (citire doar-citire, 11.10.2026). Clasificare **statica** dupa nume si marcaje din continut; nu dovedeste utilizare in productie si nu autorizeaza stergeri. Fiecare obiect apare o singura data. Ce nu e demonstrat: utilizarea reala a obiectelor DE-CLARIFICAT (vezi [DECIZII_DESCHISE](DECIZII_DESCHISE.md)).\n\nTotal ${rows.length} obiecte: ${Object.entries(count).map(([k, v]) => k + ' ' + v).join(', ')}.\n\n## Sinteza pe dispozitie\n\n| Dispozitie | Nr. |\n|---|---:|\n` + Object.entries(byDisp).sort().map(([k, v]) => `| ${k} | ${v} |`).join('\n') + '\n\nLegenda: INCLUS-* = portat/migrat/reconstruit; EXCLUS-* = decizie utilizator (N06 Coduri, N13 e-Factura, DBS/clsDatabase, infrastructura desktop); DE-CLARIFICAT = fara destinatie aprobata (cu dovada adunata); NEREFERIT-NEPORTAT = nicio referinta in export, neportat pana la cerere; INLOCUIT-PARTENERI-KBOT = furnizorii se leaga de Partenerii KBOT dupa CodFiscal; TEMP-LUCRU/DERIVAT = nu se migreaza ca date.\n';
const titles = { table: 'tables', query: 'queries', form: 'Forms', report: 'Reports', module: 'Modules', class: 'Classes', macro: 'Macros' };
for (const t of Object.keys(titles)) {
  md += `\n## ${titles[t]} — ${count[t]}\n\n| Obiect | Familie | Dispozitie | Nota |\n|---|---|---|---|\n`;
  for (const r of rows.filter(x => x.type === t)) {
    const link = `[${r.name.replace(/\|/g, '/')}](VERCON_ACCESS/${encodeURI(r.file)})`;
    const extra = [r.note, r.marks && r.marks.length ? 'marcaje: ' + r.marks.join(', ') : '', r.usesForms ? 'Forms!/TempVars' : '', r.fields ? r.fields + ' campuri' : ''].filter(Boolean).join('; ');
    md += `| ${link} | ${r.family || ''} | ${r.disp} | ${extra} |\n`;
  }
}
fs.writeFileSync(path.join(ROOT, 'INVENTAR_OBIECTE.md'), md);
console.log(count, byDisp);
