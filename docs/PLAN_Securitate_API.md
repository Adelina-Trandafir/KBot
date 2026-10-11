# Plan de securizare API și retragere VBA legacy

Actualizat: 11.10.2026, în urma auditului rutelor Python și a deciziilor utilizatorului.

**Stare: plan pentru implementare.** Acest document nu dezactivează singur endpoint-urile și nu modifică deployment-ul. Fișierele VBA din `docs/vba/retirare_legacy` sunt pregătite pentru integrare, nu au fost importate într-o bază Access.

## Decizii acceptate

1. Parola inițială poate autentifica utilizatorul numai pentru schimbarea obligatorie a parolei. Kbot.App nu deschide aplicația principală până la schimbarea reușită. Politica MariaDB rămâne activă.
2. Toate rutele pentru vechiul VBA sunt retrase complet. Nu rămâne fallback pe X-Api-Key, nici opțiune de configurare care să reactiveze vechea cheie.
3. VBA nu mai apelează API-ul Python și nu mai conține acreditări pentru el. Funcționalitățile vechi de sincronizare/trimitere se retrag sau se mută explicit în Kbot.App; nu se înlocuiește cheia VBA cu un bearer salvat în VBA.
4. Operațiile administrative și de migrare se execută prin administrare separată, cu identitate și drepturi proprii, preferabil mecanismele SSH/CLI existente. Cheia distribuită clienților nu autorizează administrarea.

## S1. Schimbarea obligatorie a parolei inițiale

### Detectare și stare pe server

- Introducerea unei stări persistente per cont MariaDB/operator: `MustChangePassword`, `PasswordChangedAt` și `AuthVersion` într-un tabel dedicat. Parola contului este comună acceselor sale la unități; nu se creează stări contradictorii per unitate.
- Administratorul marchează contul obligatoriu pentru schimbare la creare și la fiecare resetare cu o parolă inițială. Parola inițială nu se salvează ca text în client, în răspunsuri sau în loguri.
- Pentru conturile existente: inventariere și marcare explicită a celor resetate/cu parole inițiale. Compatibilitatea cu vechile parole derivabile din numele bazei se verifică numai după autentificare reușită și numai pentru unitățile autorizate ale contului. Nu se presupune că orice parolă a tuturor operatorilor urmează acea formulă.
- Dacă inițialele sunt alese arbitrar de administrator, formula nu le poate detecta: este necesar flag-ul persistent, setat de fluxul de resetare. Flag-ul devine sursa de adevăr pentru viitor; nu se adaugă o listă de parole în Kbot.App.
- La autentificarea cu parola inițială se emite exclusiv o sesiune cu scop `password_change`, cu TTL scurt. API-ul verifică scopul, inclusiv în Redis și după restart. Nu se emite mai întâi un token normal și apoi se cere schimbarea doar în UI.
- O asemenea sesiune poate cere politica, codul de confirmare, schimbarea parolei și logout. Accesul la date, schimbarea unității, încărcări și operații financiare este refuzat cu `PASSWORD_CHANGE_REQUIRED`. Clientul nu intră în bucla de reautentificare pentru acest motiv.
- Verificarea stării contului trebuie să împiedice ocolirea printr-un client modificat sau altă intrare de autentificare. Sesiunile normale vechi se invalidează prin AuthVersion/revocare când se impune resetarea.

### Politica reală și erori explicite

- Citirea server-side a pluginurilor de validare active și a cerințelor efective. Pentru `simple_password_check`: `minimal_length`, `digits`, `letters_same_case` (cerință separată pentru majuscule și minuscule) și `other_characters`.
- Menținerea `strict_password_validation=ON`, plus verificarea existenței pluginului activ: flag-ul singur nu impune cerințe dacă nu există plugin de validare. Eliminarea tuturor instrucțiunilor care îl pun OFF din provizionare/setup.
- Backend-ul furnizează politica în răspunsul de schimbare obligatorie sau într-un endpoint modern, autorizat pentru sesiunea restrânsă. Nu se inventează o lungime fixă independentă de server și nu se încorporează în client o copie rigidă a politicii.
- UI: parolă nouă, confirmare și listă de cerințe actualizată pe măsură ce se scrie. Exemplu de rezultat, folosind valorile reale: «Lungime: 6 din minimum 12», «Lipsește o cifră», «Lipsește o majusculă», «Lipsește un caracter special». Afișarea tuturor lipsurilor cunoscute, nu doar a primei.
- Validare autoritară pe backend înainte de SET PASSWORD/ALTER USER, cu `reason=PASSWORD_POLICY_FAILED` și listă structurată `violations`. MariaDB validează din nou la schimbare. Dacă politica s-a schimbat între afișare și submit, backend-ul întoarce politica actuală.
- Refuzurile suplimentare ale pluginurilor de dicționar/reutilizare se traduc din diagnosticele disponibile, fără expunerea parolei. Pentru un plugin care nu oferă cauza exactă nu se inventează «lipsește o cifră»: se comunică refuzul suplimentar real sau faptul că serverul nu furnizează mai mult detaliu.
- Parola nouă trebuie să difere de cea inițială/actuală; confirmarea trebuie să corespundă. Limite rezonabile la payload și rate limiting separat pentru codurile email. Nu se loghează corpul acestor cereri.
- Parolele inițiale create de acum trebuie să respecte și ele politica. Dacă se permite alegerea manuală de către administrator, validarea îi arată aceleași lipsuri. Varianta recomandată: inițială aleatorie puternică, marcată obligatoriu pentru schimbare.

Documentație: [MariaDB simple_password_check](https://mariadb.com/docs/server/reference/plugins/password-validation-plugins/simple-password-check-plugin), [pluginuri și strict_password_validation](https://mariadb.com/docs/server/reference/plugins/password-validation-plugins/password-validation).

### Integrare Kbot.App și finalizare

- Extinderea contractului de autentificare și a DTO-urilor din KBot.Domain/KBot.Api. Rezultatul de login poate fi autentificare normală sau schimbare obligatorie, fără a cere contextul complet pentru al doilea caz.
- Adaptarea `LoginForm` și a pornirii aplicației înainte de încărcarea perioadelor, datelor și MainForm. Dialogul poate fi închis numai prin schimbare reușită sau ieșire/deconectare. Escape/X nu deschide aplicația.
- Refolosirea serviciului existent de schimbare parolă, extins pentru politica structurată și sesiunea restrânsă. Nu se folosește `ParolaAvansataForm`: acea parolă protejează doar afișarea unor setări locale.
- După schimbarea reușită: consum atomic al codului, ștergerea MustChangePassword, increment AuthVersion și revocarea tuturor sesiunilor vechi. Emitere de sesiune normală nouă sau relogin cu parola nouă.
- Actualizarea best-effort a parolei pe baza legacy, dacă rămâne necesară pentru accesul SQL/ODBC vechi, este o decizie separată de rutele HTTP. Retragerea API-ului nu înseamnă automat retragerea bazei vechi. Orice sincronizare de parolă trebuie să respecte politica serverului respectiv și să raporteze fidel eșecul.
- Tranzacțiile și recuperarea la erori trebuie proiectate ținând cont de implicit commit pentru DDL; nu se pretinde că ALTER USER și actualizarea tabelului aplicației sunt atomicizate de un simplu transaction wrapper.

Teste obligatorii: inițială vs parolă deja schimbată, cont cu mai multe unități, fiecare cerință lipsă și mai multe simultan, confirmare greșită, încercarea unei operații normale cu token restrâns, închiderea dialogului, resetare administrativă, schimbare de politică între afișare și submit și revocare în memory/Redis.

## S2. Retragerea completă a API-ului VBA

### Inventar și dependențe

Inventarul exact al handler-elor cu guard de cheie este în [security/legacy-route-inventory.json](security/legacy-route-inventory.json). Inventarul este o listă pentru retragere, nu o listă de URL-uri deja dezactivate.

| Zonă legacy | Acțiune |
|---|---|
| admin, nomenclatoare, salarii, parteneri, clasificatii, ddf, ord, wfls, ftp | Eliminarea înregistrării blueprint-urilor HTTP legacy și retragerea apelurilor VBA |
| forexe.seed și migrare | Retragerea HTTP cu cheie; adaptarea KBot.Migrator la administrare privată SSH/CLI înainte de utilizarea următoare |
| mfp/get_ef_token | Eliminare definitivă; secretul rămâne exclusiv pe server |
| mfp/template_ddf și template_ord | Eliminarea URL-urilor vechi; endpoint-uri moderne distincte cu bearer pentru Kbot.App |
| tools/say_hello, pdfium_dll, check_update, check_updates, upload | Eliminarea rutelor legacy; distribuția aplicației prin canalul update modern, publicarea prin administrare privată |
| tools/process_excel | Păstrarea funcției moderne, exclusiv require_session; eliminarea fallback-ului pe cheie |
| forexe/angajament/dump | Retragerea compatibilității legacy; dacă diagnosticul rămâne necesar, rută modernă cu unitate și operație autorizate sau CLI privat |

**Dependențe confirmate ale aplicațiilor noi:**

- `src/KBot.Xfa/TemplateDownloader.vb` folosește încă cheia pentru machetele PDF. Se adaptează la sesiunea Kbot.App și noile rute, inclusiv HEAD/cache; se șterge constanta secretă din Config.vb. Se verifică DDF și ORD cu cache gol și existent.
- `src/KBot.Migrator/Schema/SchemaSyncClient.vb` folosește cheia și rutele migrare. Se schimbă transportul și autentificarea administrativă; nu se păstrează excepție API legacy pentru migrator.
- `forexe/angajamente.py` și `angajament_dump.py` importă un validator din `routes.admin`. Validatorul se mută într-un utilitar neutru înainte de ștergerea modulului legacy; logica modernă nu trebuie să depindă de un blueprint retras.

### Implementare și verificare

1. Înlocuirea dependențelor moderne de mai sus și pregătirea release-ului Kbot.App fără API key.
2. Eliminarea importurilor/înregistrărilor legacy din main.py. Curățarea modulelor retrase după extragerea helper-elor încă folosite.
3. Eliminarea `require_session_or_api_key`: rutele comune devin exclusiv `require_session`. Eliminarea config.API_KEY și a `require_api_key` după retragerea ultimului consumator. Nicio rută sau altă instanță a serverului nu trebuie să mai autorizeze cheia.
4. Reguli nginx de 410 pentru URL-urile vechi, dacă se dorește mesaj explicit de retragere, fără forward către handlers. Altfel, URL-uri neînregistrate → 404. Toate aliasurile, vhost-urile și porturile intră în verificare; un 401 cu aceeași cheie încă activă nu este retragere completă.
5. Rotirea/revocarea cheii distribuite și a secretelor potențial expuse. Eliminarea copiilor de configurație și a literalelor; nu se promite ștergerea fizică din executabilele vechi deja distribuite. Acestea devin inutile prin refuzul pe server.
6. Teste pentru fiecare metodă și alias legacy: fără acreditări, cu cheia veche și cu bearer valid ⇒ 404/410 și zero efecte DB/FTP/fișier. Rutele moderne nu acceptă cheia ca autentificare. Teste pozitive Kbot.App pentru machete, Excel, documente și update.

## S3. Eliminarea apelurilor și a cheilor din VBA

Ghidul și codul pregătit sunt în [vba/RETIRARE_API_LEGACY.md](vba/RETIRARE_API_LEGACY.md).

- Eliminarea reală a clienților HTTP pentru API, helper-elor URL, constantelor și citirilor de chei din registru/tabele/fișiere, inclusiv setarea headerelor.
- Eliminarea apelurilor de sincronizare din formulare, rapoarte, AutoExec, macrocomenzi, Timer, Ribbon și query-uri care apelează funcții VBA.
- Pentru o tranziție care păstrează compilarea, stub-uri temporare cu aceleași semnături, care refuză explicit operația. Fără On Error Resume Next și fără succes fals. La final se șterg și stub-urile și apelanții lor.
- Exportul integral al obiectelor Access este necesar pentru inventarul complet: în repo există doar două module VBA exportate, nu întregul frontend VBA AVACONT/FOREXE. Bazele ADECHIT din repo nu reprezintă automat frontend-ul vizat.
- Modulul de export inclus nu elimină automat cod și nu dezactivează API-ul. Ajută la identificarea tuturor locurilor, inclusiv codul din formulare și macrocomenzile încorporate. Verificarea exportului se repetă după curățare.
- Exportul offline JSON din mdl_FX_ExportSeed nu face el însuși apeluri HTTP și poate rămâne ca export local. Conține însă o parolă Access hardcodată: aceasta se elimină și se primește la rulare printr-un mecanism adecvat, fără literal/parolă implicită distribuită.
- Inventarierea secretelor distincte (API key, client secret ANAF, token, parole). Nu se șterg arbitrar proprietăți Dictionary.Keys, primary keys sau toate șirurile care conțin cuvântul «key».
- Recompilarea și redistribuirea tuturor frontend-urilor ACCDE/MDE; eliminarea versiunilor de frontend de pe share-uri și din pachetele active. Copiile utilizatorilor vechi sunt neutralizate și de retragerea server-side.

Acceptare: compilare completă, execuție a fluxurilor rămase, inventar revizuit fără acces la API-ul Python sau acreditări pentru acesta și observație de trafic fără cereri către vechile intrări. O căutare textuală fără rezultate, singură, nu dovedește absența apelurilor construite dinamic sau făcute prin biblioteci externe.

## S4. Celelalte fixuri din audit

După/în paralel cu retragerea legacy: izolarea pe unitatea sesiunii pentru cele trei rute FOREXE, verificarea drepturilor KB pe server, verificarea criptografică PDF, igiena antetelor proxy, revocarea sesiunilor, regex cu buget real, cote de upload și erori fără detalii sensibile. Închiderea rutelor legacy nu rezolvă defectele din rutele bearer moderne.

## Ordinea de livrare

1. Inventar și înlocuirea consumatorilor moderni ai cheii; separarea administrării.
2. Retragerea tuturor rutelor legacy și revocarea cheii; verificare pe toate intrările deployment-ului.
3. Curățarea/recompilarea VBA și distribuirea frontend-urilor fără API.
4. Fluxul obligatoriu pentru parola inițială, cu politică reală, sesiune restrânsă și revocare.
5. Restul problemelor moderne din audit, cu teste negative pentru unitate, rol și documente semnate.

Pașii 1 și 4 se pot dezvolta independent. Retragerea trebuie să fie completă la livrare, fără o perioadă în care cheia veche rămâne validă «doar pentru anumite operații».
