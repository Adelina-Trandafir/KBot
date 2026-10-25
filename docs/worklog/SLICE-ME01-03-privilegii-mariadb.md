# ME01-03 — privilegiile MariaDB ale conturilor de lucru

19.10.2026. **IMPLEMENTAT ÎN SURSE, NERULAT.** Nu s-au executat teste,
build, verificări automate, conexiuni MariaDB sau SQL. Numai analiză de design.

Comenzile KBotAdmin `grant`, `revoke`, `acct-add`, `acct-role`, `acct-remove`
sincronizează și privilegiile reale MariaDB. `privileges-sync --un EMAIL` și
butonul **Aplică drepturile MariaDB** aplică politica unui cont existent fără
schimbarea bifelor. Se procesează toate legăturile contului, indiferent de baza
selectată în interfață, și toate variantele exacte `User@Host` existente.

## Politică

- Se retrag TOATE privilegiile directe vechi, inclusiv cele globale, pe alte
  baze, pe coloane, rutine și opțiunea de acordare. Se reconstruiește accesul
  din `Unitati_Utilizatori`, `Utilizatori_Roluri`, `Roluri_Operatii`.
- Contabil: SELECT, INSERT, UPDATE, DELETE pe tabelele bazelor sale. Bifele
  ADECHIT/VERCON continuă să controleze aplicațiile web; rolul Contabil are
  acces SQL la toate datele din unitățile lui, inclusiv AD/VC.
- Pentru celelalte roluri, operațiile KB `CITIRE` / `EDITARE` dau citire /
  scriere pe datele K-BOT, separate de tabelele `AD_` / `VC_`. Operațiile
  restrânse (semnături, CITIRE_DDF_ORD, FOREXE) rămân prin API; nu se transformă
  într-un drept SQL larg de scriere. Rolurile KB nu sunt roluri de DBA.
- ADECHIT / VERCON: dreptul `read` permite citirea tabelelor modulului;
  drepturile de lucru dau DML pe familiile de tabele ale operației, conform
  `privilege_policy.py`. Importul are o familie mai largă în modulul lui.
  Fără `read`, un drept suplimentar singur nu deschide acces SQL la modul.
  Listele de acces, autentificarea părinților și metadatele migratorului nu
  se acordă prin drepturile unui modul. Parteneri este numai citit de VERCON.
- Fără privilegii personale pe `AVACONT_COMUN`, `AVACONT_SURSA`, `mysql` sau
  alte baze neatribuite. API-ul citește nomenclatoarele și metadatele comune
  prin contul de serviciu existent.
- Numai tabele concrete existente: fără wildcard de baze (`_` în MariaDB este
  wildcard la grantul pe bază), view-uri cu definer sau EXECUTE pe rutine.
  Nu se acordă CREATE, ALTER, DROP, CREATE USER, SUPER, FILE sau GRANT OPTION.
  Tabelele noi sunt incluse la următoarea sincronizare a contului.

Privilegiile SQL sunt pe tabele, nu pe gestiuni, subunități, rânduri, perioade
sau câmpuri. Bifele și validările detaliate din aplicație rămân obligatorii în
API, care execută în continuare operațiile prin contul de serviciu. Limitarea
contului personal nu înlocuiește autorizarea API și nu îi reduce privilegiile
contului de serviciu.

## Aplicare și erori

Este necesar un cont administrativ separat, `DB_CONFIG_DREPTURI`, obligatoriu
pe același host/port ca `DB_CONFIG_NEW`. Nu există fallback la contul de
serviciu sau la serverul vechi. Fișierul
`sql/SEC03_kbotadmin_privilegii.sql` se adaptează și se execută MANUAL de DBA;
include contul dedicat și exemplul configurării. Nu introduce tabele, coloane,
proceduri sau creare automată de scheme în aplicație.

Conturile tehnice, root/Admin/AVACONT, proprietarul, `PORTAL_ADMIN_EMAILS` și
`DREPTURI_CONTURI_PROTEJATE` sunt excluse. Roluri MariaDB moștenite, PROXY sau
privilegii acordate prin PUBLIC opresc operația înaintea oricărei modificări;
DBA trebuie să le trateze separat. Nu sunt ignorate pentru a raporta un succes
fals. Contul MariaDB trebuie să existe deja; nu se creează implicit prin GRANT.

Un lock MariaDB serializează schimbările din KBotAdmin. Datele despre roluri
și jurnalul sunt într-o tranzacție separată de conexiunea GRANT/REVOKE.
Planul se calculează înaintea primei retrageri. Succesul se întoarce după
aplicarea SQL și salvarea tranzacției. La eșec se anulează modificările rolurilor
și se încearcă retragerea tuturor privilegiilor SQL parțiale, pe fiecare host.
Dacă salvarea sau rollback-ul metadatelor nu pot fi confirmate, eroarea cere
recitirea conturilor; nu pretinde că modificările rolurilor au fost anulate.
Nu se restaurează vechiul ALL PRIVILEGES. Eroarea cere reaplicare sau intervenție
DBA dacă retragerea nu a reușit. Închiderea conexiunilor MariaDB ale contului
este obligatorie, pentru eliminarea privilegiilor globale memorate în sesiuni.

GRANT/REVOKE fac commit implicit: nu există atomicitate între metadatele
aplicației și ACL MariaDB. Oprirea procesului sau pierderea conexiunilor în
timpul schimbării pot necesita reaplicarea politicii. Lock-ul nu coordonează
modificări manuale făcute în paralel de DBA. Nu se rulează administrare manuală
simultană pentru același cont. Sesiunile API folosesc contul de serviciu;
acest slice închide conexiunile MariaDB personale, nu sesiunile API.

Documentație primară consultată pentru design:
[REVOKE](https://mariadb.com/docs/server/reference/sql-statements/account-management-sql-statements/revoke),
[GRANT](https://mariadb.com/docs/server/reference/sql-statements/account-management-sql-statements/grant),
[commit implicit](https://mariadb.com/docs/server/reference/sql-statements/transactions/sql-statements-that-cause-an-implicit-commit).

## Fișiere

`PYTHON/routes/admin_cli/admin_common.py`, `admin_rights.py`,
`mariadb_privileges.py`, `privilege_policy.py`, `admin_cli.py`;
`KBotAdmin/Views/RightsView.vb`, `RightsView.Designer.vb`, `RightsForm.vb`, `RightsForm.Designer.vb`,
`AccountForm.vb`, `AccountForm.Designer.vb`, `README.md`;
`sql/SEC03_kbotadmin_privilegii.sql`, acest jurnal, `implementare_vercon_v2.md`.

Separarea helperelor și comenzilor de conturi din `admin_cli.py` păstrează
modulele sub limita de aproximativ 1000 de linii. DTO-urile existente acceptă
răspunsurile cu metadate suplimentare `sql_privileges`.

Completare de trasabilitate D001-11, preluată din jurnalul general: `RightsForm.Designer.vb` rezervă spațiu pentru explicația rolului Contabil. Explicația accesului SQL și raportarea salvării/rollback-ului incert sunt contractele de mai sus.
