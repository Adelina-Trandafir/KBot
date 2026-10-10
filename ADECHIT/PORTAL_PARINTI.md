# ADECHIT — portalul părinților

<!-- slice: AD11-01 -->
Implementat local la 10.10.2026. Validarea funcțională/vizuală aparține utilizatorului;
implementarea locală și verificarea pornirii nu reprezintă validare pe MariaDB.

## Acces

Linkul este `/adechit/parinti/<DC>`. Autentificarea cere CNP-ul părintelui,
codul permanent `AD_Platitori_sub.CodAccesPortal` și apoi un cod temporar prin email.
Browserul poate salva CNP-ul/codul permanent prin autocomplete username/current-password;
codul temporar nu se salvează. Codul permanent este aleatoriu (128 biți), comun
tuturor asocierilor aceluiași CNP în DC. Codul temporar expiră după 5 minute,
are cel mult 5 încercări și se consumă la conectare. Retrimiterea are pauză de
60 secunde; solicitările repetate sunt limitate prin infrastructura comună.

Sesiunea folosește infrastructura comună memory/Redis într-un registru separat de
operatori; cookie HttpOnly, Secure în producție, SameSite Strict. Expirarea urmează
registrul comun: 20 minute de inactivitate, maximum 30 minute de la conectare.
Fiecare solicitare verifică din nou CNP/email/cod și copiii fără Plecat, inclusiv PDF.
Subunitățile sunt folosite numai intern pentru autorizarea datelor; nu sunt afișate.
Părinții nu primesc tokenuri sau drepturi de operator.

## Macheta părinților

Butonul „Trimite Codul de Acces” apare numai cu email completat. Salvează mai întâi
datele părintelui și apoi trimite codul permanent și linkul. Dacă emailul nu este
livrat, datele salvate rămân și se poate reîncerca. CNP/email sunt obligatorii
pentru trimitere și copilul trebuie să fie fără Plecat.

Emailurile sunt normalizate (spații exterioare eliminate, litere mici). Același email
nu poate fi asociat altui CNP: mesajul din macheta administrativă identifică părintele,
CNP-ul și copilul. Modificarea emailului unei identități existente îl sincronizează
pe toate asocierile aceluiași CNP. Asocierea nouă reutilizează identitatea existentă.

AD11-03: înregistrările vechi cu email comun unor CNP-uri diferite sau emailuri
diferite pentru același CNP sunt acceptate la import. În **Plătitori**, avertizarea
de portal listează înregistrările afectate din subunitatea curentă; verificarea
conflictului acoperă întregul DC. Click pe o intrare selectează grupa/copilul/părintele
și deschide editorul, inclusiv dacă grupa este ascunsă. Corectarea reface accesul
când nu mai există ambiguități. Introducerea unui conflict nou rămâne refuzată.

## Dashboard și fișă

<!-- slice: AD11-02 -->
Portalul părinților se derulează vertical și pe mobil până la fișa de cont.
Opțiunile selectorului copilului folosesc font 16px și rânduri de minimum 44px.
Subtotalurile fișei au fundal gri deschis, iar totalul final gri puțin mai închis,
atât în imprimare cât și în PDF; imprimarea solicită păstrarea fundalurilor.

Un copil eligibil se deschide direct. Cu mai mulți apare Comboboxul comun custom.
Dashboardul folosește DataGrid-ul comun readonly, solduri debit/credit, zile,
plăți/restituiri, două grafice SVG cu culorile temei. Lunile închise folosesc SS_Buget;
lunile deschise folosesc motorul ADE existent și sunt marcate „În curs”. Documentele
anulate sunt excluse conform regulii Access (flag=False; NULL nu este False).

Fișa imprimabilă și PDF-ul au același model de date: antet unitate, identificare copil,
Data/Explicație/DEBIT/CREDIT/Sold/Tip sold, sold inițial și totaluri lunare.
CNP-ul copilului este mascat în raport. Prezența este înregistrată la ultima zi a
lunii, plățile/restituirile la data lor. Debit=obligații+restituiri, credit=plăți.
Soldul pornește din SI, care poate reprezenta un sold importat fără documentele vechi.

„Interval” este implicit debifat: întreaga evidență disponibilă pe IDP, indiferent de an
sau mutarea grupei. Bifat: două DatePicker custom active; limitele sunt incluse și
soldul inițial include toate mișcările anterioare datei de început. PDF-ul și imprimarea
folosesc aceleași filtre. O situație salvată lipsă într-o lună închisă sau o mișcare
fără dată blochează raportul cu mesaj, fără succes aparent.

## Testarea manuală localhost

AD11-04: pe mobil (lățime maximum 800px), selectorul ocupă întreaga lățime a
dashboardului; cardurile și calendarele nu lățesc pagina. Graficele cu multe luni
se derulează în interiorul cardului. „Imprimă” este ascuns, iar PDF-ul rămâne disponibil.

Serverul curent: `http://localhost:5050`, date SQLite separate în
`artifacts/ade-parents-preview`. Nu se folosesc emailuri sau date reale.

1. Deschide `/adechit/preview/parents`: CNP-urile și codurile permanente fictive.
2. Deschide `/adechit/parinti/preview`; folosește părintele `parinte.demo@example.test`.
3. Cere codul; deschide/reîncarcă `/adechit/preview/inbox`, copiază codul temporar și conectează-te.
4. Verifică cei doi copii eligibili și absența copilului Plecat. Schimbă copilul și verifică grila/graficele.
5. Descarcă PDF și deschide imprimarea, fără Interval și apoi cu date alese.
6. Cu `parinte.unic@example.test`, verifică absența selectorului. Cazul fără email nu primește cod.
7. În `/adechit`, Plătitori, verifică butonul de trimitere și eroarea emailului folosit de alt CNP.
8. Verifică cod greșit/expirat/reutilizat, limitele, deconectarea, retragerea accesului după Plecat/email modificat.

Inboxul și lista cu acreditări sunt definite exclusiv în launcherul local, cu verificare
loopback; nu sunt înregistrate în `PYTHON/main.py` și nu se publică. SMTP este înlocuit
numai local. Session backend este memory numai în launcherul local.

Repornire locală (din rădăcina KBOT):

```powershell
PYTHON/.venv/Scripts/python.exe ADECHIT/tools/preview.py --directory artifacts/ade-parents-preview --port 5050
```

## Predare pentru server (după testarea localhost)

- DDL: `sql/AD_08_portal_parinti.sql` pe **AVACONT_SURSA**, după AD_05–07.
  Sincronizarea schemei din AvacontPush adaugă coloana CodAccesPortal și tabelele portalului în baze.
- Date: conținutul `sql/AD_08_02_interogare_unica.sql` în **Interogări unice**, nume
  `AD_08_portal_parinti_date`, întâi „Vezi (nu execută)”, apoi „Execută”. Instrucțiuni simple,
  fără proceduri sau DELIMITER; păstrează codurile existente, completează codurile lipsă și lock-ul.
  Scrierile ADE se suspendă pentru această operație. SQL-ul nu a fost executat în chat.
  AD11-03: conflictele vechi CNP/email nu blochează migrarea. Emailul identității de portal
  este NULL pentru CNP-urile afectate, contactele sursă rămân intacte. Portalul verifică
  aceste conflicte la fiecare cerere, inclusiv pentru o sesiune deja conectată.
- Publică împreună fișierele Python/JS/CSS/template din AD11-01 și noul ADE.Migrator;
  după AD_08, aplicația veche/migratorul vechi nu trebuie folosite pentru scrieri.
- Instalează `PYTHON/requirements-adechit.txt`; biblioteca a fost instalată în venv local.
- SMTP folosește configurația comună. Opțional `ADE_PARENT_PUBLIC_URL` în config reprezintă
  originea publică HTTPS pentru linkurile din email; altfel se folosește originea cererii.
- MariaDB din export este 10.11.14. Generarea SQL folosește [RANDOM_BYTES](https://mariadb.com/docs/server/reference/sql-functions/secondary-functions/encryption-hashing-and-compression-functions/random_bytes), disponibilă din 10.10.
- Ordinea lock-urilor: AD_Lock(0) pentru identitatea în DC, apoi lock-ul subunității.
  Registrul email unic și această ordine serializează salvările concurente și migrarea.
- Datele Access, launcherul local, SQLite, inboxul și jurnalele locale sunt excluse din publicare.
- Publicarea, SQL și probele SMTP/MariaDB aparțin utilizatorului, prin AvacontPush.
