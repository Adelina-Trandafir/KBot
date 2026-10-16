// Shared by dictionar.js and import_map.js: TZ_SCHEMA reader and Access column classification.
const fs = require('fs'), path = require('path');
const ROOT = path.join(__dirname, '..');
const TZ = path.join(ROOT, 'VERCON_ACCESS', 'Databases', 'TZ_SCHEMA');
const EXCL = /^(_|SQL|RunOnce|Ver$|Trimis)/; // schema-mechanism / infra tables: excluded (see INVENTAR_OBIECTE)
function parseTz(name) {
  const t = fs.readFileSync(path.join(TZ, 'tables', name + '.md'), 'utf8').replace(/\r/g, "");
  const fields = []; const idx = [];
  for (const l of t.split('\n')) {
    let m = l.match(/^\| (\d+) \| (.+?) \| (.+?) \| (\d+) \| (True|False) \| (.*?) \| (.*?) \| (.*?) \|$/);
    if (m) fields.push({ n: m[2], type: m[3], size: +m[4], req: m[5] === 'True', def: m[6] });
    m = l.match(/^- (.+?): \((.+?)\)(.*)$/); if (m) idx.push({ n: m[1], cols: m[2], flags: m[3].trim() });
  }
  return { fields, idx };
}
const VALUE = new Set(['Cant', 'Pret', 'TVA@Intrari', 'Valoare', 'PUTVA', 'ValoareCUTVA', 'ValoareBC', 'ValoareLuna', 'ValoareLunara', 'SI_LA', 'ValoareBon', 'DEP', 'ValoareLista', 'VMD', 'VD', 'VC', 'MicDejun_V', 'Dejun_V', 'Cina_V', 'PU', 'Cantitate']);
const KEYRE = /^(ID|Id)([A-Z_]|$)|^IDC|^IDF|^IDB|^IDM|^IDOI|^IDPMF|^IDFel|^IdFel/;
function cls(table, f) {
  const k = f.n, key = k + '@' + table;
  if (table === 'TVA' && k === 'TVA') return ['COTA', 'DECIMAL(20,4) = cota directa, independenta de zecimalele gestiunii'];
  if (table === 'Setari' && k === 'V') return ['CONFIG', 'nu se copiaza; cheie tipizata in VC_Settings (INT); valorile reale: -1/0 sau ZEC'];
  if (table === 'NIR' && k === 'Numar') return ['NUMAR', 'INT (numar document; negativ la storno/seria istorica); NU se scaleaza; fractia (document inserat intre doua numere) in NumarSub, in sutimi'];
  if (VALUE.has(k) || VALUE.has(key)) {
    if (f.type === 'Long') return ['VALOARE-INT', 'DECIMAL(20,4), valoare directa (sursa Long: fara fractiune de inventat)'];
    return ['VALOARE-INT', 'DECIMAL(20,4), valoare directa; stocat exact, refuz la overflow (nu se rotunjeste la migrare)'];
  }
  if (f.type === 'AutoNumber' || (f.type === 'Long' && KEYRE.test(k))) return ['CHEIE', 'cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap'];
  if (f.type === 'Text') return ['TEXT', `VARCHAR(${Math.min(f.size, 255)})  utf8mb3_general_ci; zerouri initiale pastrate`];
  if (f.type === 'Date/Time') return ['DATA', 'DATETIME, data locala fara conversie UTC'];
  if (f.type === 'Yes/No') return ['BOOL', 'TINYINT(1) NULL/0/1 (Access -1 -> 1); NULL separat'];
  if (f.type === 'Long' || f.type === 'Integer') return ['INT', 'INT (contor/numar/durata); NU se scaleaza'];
  if (f.type === 'Memo') return ['TEXT', 'TEXT utf8mb3'];
  if (f.type === 'Double') return ['DE-CLARIFICAT', 'Double neclasificat: se decide la VC03-02'];
  return ['DE-CLARIFICAT', ''];
}
module.exports = { ROOT, TZ, EXCL, parseTz, cls, VALUE };
