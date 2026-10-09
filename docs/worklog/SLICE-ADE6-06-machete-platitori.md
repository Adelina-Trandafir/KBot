# SLICE-ADE6-06 — machete Plătitori, grupe și copii

Data: 09.10.2026. TESTAT LOCAL; fără commit, push sau validare pe server.

## Ce s-a schimbat și de ce

Plătitori urmează cele patru machete furnizate: trei liste dependente Grupe → Copii →
Plătitori, fiecare cu Adaugă/Modifică, și formulare separate, uniforme, pe fundal alb.
Listele permit selecție; nu permit editare în celule. PL este exclusiv checkbox pentru
afișare, modificabil numai în editorul elementului: Grupă închisă la grupe, Plecat la
copii, conform precizării utilizatorului. ACT este tot checkbox pentru afișare.
Bifa grupei păstrează anul de închidere existent sau folosește anul curent la prima
închidere; debifarea elimină închiderea. Nu există câmp suplimentar de an în formular.

Editorul grupei include numele, bifa, lista ANI și perioadele educatorilor. Editările
grupei și perioadelor se salvează atomic; perioadele aceluiași educator nu se suprapun.
Editorul copilului include numele, CNP, combobox comun pentru grupă, calendar pentru
intrare/ieșire și bifa Plecat. Editorul plătitorului include toate câmpurile machetei,
inclusiv telefon, email și Activ. CNP se validează în JS și API după algoritmul VBA:
13 cifre, limitele superioare pentru lună/zi/județ, checksum 279146358279, rest 10 → 1.
CNP necompletat rămâne permis; nu reinterpretăm regulile VBA drept validare calendaristică.

Ferestrele blochează fundalul; Escape și clicul în afară nu închid editorul. Salvează
închide numai după reușită. Renunță abandonează explicit editările, conform machetelor
ulterioare cererii inițiale. Erorile păstrează formularul și datele; reîncercarea aceluiași
corp refolosește cheia idempotentă. Pe telefon conținutul derulează, butoanele rămân vizibile.
Taxe păstrează editarea în celule și salvarea atomică din ADE6-05.

Mutarea individuală a copilului scrie istoricul și modifică numai prezențele lunilor
deschise. Lunile închise păstrează grupa și snapshotul. Starea Plecat necesită dreptul
transfer și data ieșirii. Copiii plecați cu sold sunt preluați în grupa specială PLECATI
la luna nouă; nu implementăm compensarea. Educatorii se citesc din istoricul perioadelor,
inclusiv pentru situația lunară; textul legacy este fallback doar dacă nu există istoric.

## Fișiere atinse

- PYTHON/static/adechit.html, css/adechit.css, js/adechit/app.js și catalogs.js;
  modulele noi payers.js și cnp.js.
- Componente comune: js/dgv/datagrid.js și css/dgv.css (selecție publică și afișare
  checkbox opt-in), combobox-ui.js/combobox-events.js, datepicker.js. Popupurile apar
  în dialogul nativ; alegerea din combobox nu redeschide lista.
- PYTHON/routes/adechit/__init__.py, catalog_forms.py (nou), domain.py, repository.py,
  service.py, calculations.py și schema.json; PYTHON/tests/test_adechit.py.
- ADECHIT/tools/preview.py și build_schema.py. Preview-ul are tabelele locale necesare;
  generatorul AD_01 refuză să suprascrie metadatele AD_03.
- ADECHIT_STATUS.md, state/ADECHIT_STATUS_ADE0-ADE9.md și acest worklog.

## Verificări efective

- 16/16 teste pytest în mediul virtual al proiectului: CNP, validare, rollback atomic,
  conflicte, drepturi, idempotency, istoric, transfer fără modificarea lunilor închise,
  soldul copilului plecat și perioadele educatorilor; regresiile existente.
- Edge izolat, localhost:5050, unitate și date fictive: creare grupă/educator/copil/plătitor,
  telefon persistent, combobox, calendare în dialog, CNP invalid, Escape/fundal blocat,
  conflict 409 simulat și reîncercare, Renunță fără scriere, telefon și tema întunecată.
- Proba dedicată PL: checkboxurile ambelor liste sunt disabled; clicul selectează rândul
  fără schimbarea bifei; închiderea/redeschiderea grupei se salvează numai din editor;
  Plecat și data ieșirii copilului persistă. Formularul grupei conține numai bifa.
- Adaugă copil din Prezență folosește formularul și preia copilul salvat în luna aleasă.
- Regresie browser Taxe: deschidere și salvare/închidere. Sintaxa JS și whitespace verificate.
- Probe locale reproductibile în artifacts/ade6-06-browser.cjs, ade6-06-add-child.cjs,
  ade6-06-checkbox.cjs; capturi PNG în același director, inspectate vizual.

## Neverificat și amânat

Exportul local MariaDB_Schema nu conține tabele AD_. Întrebarea privind schema serverului
rămâne fără răspuns. Implementarea locală folosește AD_03 aprobat în ADE0-07; nu afirmăm
că baza live corespunde. Nu am accesat MariaDB/Linux și nu am schimbat sursele Access.
Transferul lot, compensarea, înlocuirea endpointului legacy /transfer și paritatea integrală
Access rămân în feliile lor. Importerul Python legacy rămâne pentru preview-ul vechi;
migrarea AD_03 folosește ADE.Migrator. Nu există SQL nou în această intervenție.

## Predare și publicare

Lucrarea rămâne locală, împreună cu modificările preexistente; nu am creat commit și nu
am publicat. Prin AvacontPush trebuie predate modulele ADECHIT Python/JS/CSS/HTML și
componentele comune enumerate mai sus. Instrumentele preview/testele nu sunt runtime server.
Înaintea probei pe server trebuie confirmată schema finală AD_03 (Grupe_Educator,
Platitori_Istoric, Telefon, Tip și InchisaDinAn); nu se rulează AD_01 peste AD_03.
Nu există script suplimentar de ordonat pentru această felie.
Pe server se probează încărcarea listelor, salvarea celor trei formulare, validarea CNP,
perioadele educatorilor, PL numai din editor, drepturile și refuzul conflictelor, apoi Taxe.
Următoarea subfelie liberă: ADE6-07.
