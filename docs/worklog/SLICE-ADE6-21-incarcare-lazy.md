# SLICE-ADE6-21 — Pornire fără selecții și încărcare lazy

09.10.2026. Implementat local; 18 teste API și 3 teste JS trecute.
Fără probă vizuală, server sau MariaDB.

## Ce s-a schimbat și de ce

Pagina nu mai selectează ultima lună sau toate grupele la pornire și nu cere
situația copiilor până la alegerea lunii și grupei. Contextul conține numai
metadate/drepturi. Endpointul years citește anii DISTINCT, fără lunile lor;
arborele pornește restrâns, iar deschiderea anului cere numai lunile acelui an.
Pe mobil selecția folosește Anul, Luna și Grupa. Grupele se încarcă după
alegerea lunii. Opțiunea Toate este eliminată din arbore și combobox.

Situația primește IDG și descarcă numai copiii grupei. Citirea SQL păstrează
istoricul copiilor selectați din toate grupele/lunile lor pentru soldurile
inițiale. Formulele calculate sunt nemodificate. Lunile închise citesc
snapshotul filtrat după IDL și IDG. Endpointul situației păstrează citirea
nefiltrată pentru compatibilitate, dar pagina trimite întotdeauna grupa.

Plătitori încarcă inițial numai grupele. Se cer copiii după IDG și persoanele
după IDP, cu resetarea listelor dependente și ignorarea răspunsurilor vechi.
Editorul grupei citește separat anii și numai istoricul educatorilor săi.
Filtrele rows sunt validate și aplicate SQL, nu după descărcarea întregii tabele.

## Fișiere

- API: PYTHON/routes/adechit/__init__.py, repository.py, catalog_forms.py.
- UI: PYTHON/static/js/adechit/app.js, payers.js, static/adechit.html și css/adechit.css.
- Probe: PYTHON/tests/test_adechit.py, adechit_lazy.test.mjs.
- Documentație: ADECHIT/UTILIZARE_WEB.md, ADECHIT_STATUS.md și registrul ADE0–ADE9.

## Verificări efective

18 teste pytest API trecute, inclusiv lipsa citirilor copiilor la context/catalog,
filtre SQL, situație pe grupă egală cu situația completă, sold din grupa anterioară
și snapshot închis. O fixture veche de taxe a primit Expl și DeLa cerute deja
de ADE6-20; implementarea taxelor nu s-a schimbat.

3 teste JS prin node:test/VM, cu DOM și componente simulate, trecute: pornire
goală, ordinea cererilor lazy, navigare Plătitori PC/mobil și răspunsuri întârziate.
Aceste probe nu reprezintă verificarea vizuală în browser. Sintaxa modulelor JS
a fost verificată. Prima încercare pytest a fost blocată de directorul temporar
al sandboxului; rerularea a folosit un director dedicat în artifacts.

Testele au fost rulate înainte de citirea preferinței proiectului de a lăsa
testarea utilizatorului. Nu s-au mai pornit probe după identificarea regulii.

## Neverificat și predare

Publicarea prin AvacontPush și restartul backendului aparțin utilizatorului.
Se publică cele 7 fișiere runtime enumerate mai sus. Nu există DDL nou,
commit, push sau restart din chat. Verificarea vizuală PC/mobil și MariaDB
rămâne utilizatorului. Următoarea subfelie liberă: ADE6-22.
