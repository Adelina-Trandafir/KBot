# VRC.Migrator — utilizare (SLICE-VC05-01)

Utilitar Windows separat pentru transferul **unic** Access → MariaDB al unei subunități VERCON. În această felie știe
doar să **citească și să profileze**; planul, testarea și migrarea vin cu VC05-02/03.

## Ce face

- Deschide fiecare bază **doar în mod citire** (`Mode=Read`); nu scrie nimic în Access, nu creează tabelul `Ver`, nu atinge MariaDB.
- Calculează SHA-256 înainte și după citire: dacă diferă, raportul spune că fișierul s-a schimbat (se recitește pe o copie).
- Pentru fiecare tabel așteptat: rânduri, coloane **lipsă** (absentă ≠ NULL) și coloane în plus; pentru coloanele numerice: valori, NULL, zero, negative, minim, maxim și zecimalele necesare.
- Numără antetele NIR / bon de consum față de suma liniilor (N01/N02): antet 0, NULL, diferență > 1 leu. Nu modifică nimic.
- Citește configurarea (`Setari`, `NR`, doar `DC` și `AnDate` din `UNIT`; **numele persoanelor nu se citesc**) și propune tipul, ZEC și scara de stocare (propuneri, nu decizii).
- Raportează tabelele legate (datele lor nu sunt în fișier) și tabelele neprevăzute.

## Folosire

1. Pornește `VRC.Migrator.exe` → **Alege folder…** (folderul cu bazele subunității). Apare un tabel cu bifă pe fișier; **bifează anii dorești**, apoi **Citește bifatele** și **Salvează raportul…**.
   - Se listează fișierele `tz_aaaa` (exercițiul unei gestiuni Materiale/Alimente) și `tz_2100` (bază specială OI sau MF: **2100 nu este un an**), atât `.accdb` cât și `.mdb`; un an prezent în ambele formate apare de două ori.
   - `tz.mdb` / `tz.accdb` (fără an) nu se iau niciodată. Alte nume `tz_...` apar nebifate.
2. Fără fereastră: `VRC.Migrator.exe --profile raport.md "C:\cale\folder"` (sau fișiere). Un folder aduce toate fișierele tz_aaaa/tz_2100; cod de ieșire 0 = ok, 2 = nimic de citit sau o bază nu s-a putut citi.
3. Nu închide procese MSACCESS: dacă o bază este deschisă de cineva, hash-ul se citește oricum, iar raportul arată dacă s-a schimbat.

Schema așteptată este încorporată în executabil din `expected_schema.json`, generat cu `node VERCON/tools/expected.js` (din TZ_SCHEMA).
Parola comună a fișierelor se încearcă prin `KBot.Common.AccessFilePassword` și nu apare nicăieri.

x64, ca restul migratoarelor (furnizorul ACE este doar pe 64 de biți pe aceste stații; presupunerea că toate stațiile au ACE nu este verificată).

## Transferul (VC05-02…05)

Nerulat pe baze reale. Pașii:

1. După **Citește bifatele** apare planul: pe fiecare fișier alegeți **gestiunea** (denumire), **tipul** (MATERIALE, ALIMENTE, OI, MF) și **anul** exercițiului; completați **subunitatea** (propusă din UNIT.NumeUnitate al fișierelor; dacă diferă, se ia ultima găsită; o puteți modifica) și serverul. Propunerile vin din nume și steaguri, nu sunt aprobări. **Verifică planul** arată motivele concrete de blocare; **Exportă planul** salvează JSON (fără parole).
2. **Testează** verifică baza DC (din UNIT.DC), tabelele VC_* cu coloanele scrise, InnoDB și utf8mb3_general_ci, subunitatea nouă și furnizorii fără partener KBOT. Orice modificare a planului cere retestare.
   **Parteneri ANAF…** (opțional, după avertismentul cu furnizori fără partener): pentru fiecare cod fiscal lipsă se interoghează ANAF (v9, câte 100 de coduri pe cerere), operatorul bifează per furnizor sursa-sector (una sau mai multe; click pe titlul coloanei bifează tot) și «Pregătește pentru transfer» reține partenerii în plan. Nu se scrie nimic în DC din fereastră: **«Migrează» îi inserează în `Parteneri` pe aceeași tranzacție cu transferul**, deci dacă transferul eșuează sau este oprit nu rămâne niciunul. Un cod fiscal = un singur partener în DC; `IdUnitate` = cea mai mică unitate dintre SS bifate, `CodPartener` = următorul număr liber (minim 3 cifre) din acel SS, `Tip` = 1, `Provenienta` = `MIGRARE_VERCON` (coloană nouă în `Parteneri`: rulați întâi `sql/Parteneri_Provenienta.sql` pe AVACONT_SURSA și sincronizați DC-urile). Lista pregătită se pierde la alegerea altui folder / citirea altor fișiere. Raportul de migrare are secțiunea «Parteneri din ANAF».
3. **Migrează** face tot transferul într-o tranzacție: nimic nu e vizibil înainte de COMMIT. **Oprește** anulează înainte de COMMIT. După COMMIT subunitatea nu mai poate fi importată (VC_Imports unic pe subunitate); nu există reimport.
4. Dacă răspunsul la COMMIT se pierde, starea este REZULTAT_NECUNOSCUT: folosiți **Diagnostic** (doar citire); nu repetați transferul.
5. Jurnalul și raportul sunt în `Logs\vrc_migrare_<data>.log` și `.md`.

Reguli decise (12.10.2026): numerotarea pornește de la ultimul număr real, nu din NR.*; ProduseP se transferă doar cu folosire; valorile documentelor sunt copiate ca în Access (revizie 14.10.2026): semnele și totalurile, inclusiv 0/NULL, nu se corectează; diferențele sunt doar raportate. Detalii: `DECIZII_DESCHISE.md`, secțiunea F. Schema țintă a scriitorului este `target_schema.json`, generat de `node VERCON/tools/ddl.js`.

## Stocare DECIMAL — 14.10.2026

Cantitățile, prețurile, sumele, cotele TVA fracționare și soldul SI_LA se scriu
ca valori directe `DECIMAL(20,4)`, fără înmulțire cu `10^Scara`. `Scara=4`
este precizia fixă de stocare; `Zec` rămâne precizia afișării și calculelor.
Denumirea internă `VALOARE-INT` a clasei din exporturi este păstrată pentru
compatibilitatea descrierilor de coloane; conversia ei produce acum `Decimal`.

Pentru demo-ul care conține vechea migrare scalată:

1. Executați manual `sql/VC_DEMO_golire_pentru_decimal.sql` — șterge numai
   datele VC din `000_DEMO`, inclusiv markerii de import; păstrează tabelele,
   interogările salvate, partenerii și datele celorlalte module.
2. Executați manual `sql/VC_precizie_valori_4_zecimale.sql` și
   `sql/00EF_24_primite_linii_precizie.sql` pe `AVACONT_SURSA`, apoi sincronizați
   schema către DC-uri. ALTER-ul VC presupune lipsa datelor scalate în sursă/țintă.
3. Folosiți migratoarele recompilate și aplicația web actualizată, apoi reluați
   migrarea VERCON. Migratoarele blochează scrierea dacă schema de valori nu
   este `DECIMAL(20,4)`; nu creează și nu modifică automat scheme.

Nu s-au executat SQL-uri, compilări sau teste în cadrul acestei modificări.
Datele EF deja rotunjite nu sunt recuperate prin ALTER și nu sunt șterse de resetul VC.

## Copierea valorilor din Access — revizie 14.10.2026

VRC.Migrator nu calculează totaluri, TVA, prețuri, cantități sau amortizare și nu
corectează semne. Antetul NIR/BC rămâne ca în sursă, chiar dacă este 0, NULL sau
nu corespunde sumei liniilor. Cantitatea și prețul negative se păstrează simultan.
Constatările despre aceste situații sunt informative, fără modificarea datelor.
Numerotarea actuală după ultimul număr real rămâne, conform confirmării utilizatorului;
reprezentarea fără pierdere Numar/NumarSub și remaparea cheilor rămân necesare.

Dacă vechile constrângeri de preț nenegativ au fost instalate, executați manual
`sql/VC_IMPORT_valori_sursa.sql` înainte de migrare. Elimină numai aceste CHECK-uri
pe AVACONT_SURSA și 000_DEMO; nu se execută automat din aplicație. Regula de validare
a documentelor noi din web rămâne separată și neschimbată.

Nu s-au rulat compilări, teste sau importuri pentru această revizie.


## Raport HTML în fereastră separată

Butonul **Raport HTML…**, în partea de sus a zonei Raport / jurnal, deschide
rezultatul curent în HTML. După Verifică planul sau Testează sunt incluse și detaliile
profilării fișierelor deja citite, cu tabele, titluri și evidențierea blocajelor și
avertizărilor. Nu citește din nou Access și nu pornește o nouă verificare pe server.
Fereastra poate fi redimensionată; **Salvează HTML…** exportă un fișier complet,
cu stilurile incluse. **Închide** / Escape revine la formularul principal.

Folosește controlul WebBrowser existent și Markdig 0.45.0 (aceeași versiune ca
ajutorul KBot.App). HTML-ul din textul sursă este dezactivat; imaginile și linkurile
active sunt eliminate. Stilurile urmăresc tema la deschiderea ferestrei.
Modificare de sursă: fără teste, compilare, lansare aplicație sau comenzi Git.

## Feluri de mâncare din TIP — revizie 15.10.2026

`VC_FeluriMancare` se construiește din valorile distincte `TIP` din MicDejun,
Dejun și Cina, pe toți anii unei gestiuni. Denumirile sunt „Felul 1”, „Felul 2”,
etc.; diferențele de majuscule, spații și zerouri inițiale se normalizează.
Explicațiile meselor rămân în tabelele meselor. Vechea tabelă Access
FeluriMancare nu mai este sursa acestui nomenclator.

`IdFel` țintă este cheia noului nomenclator, derivată din `TIP`; valoarea
`IdFel` din Access nu este folosită pentru această asociere. `TIP` și
`Explicatie` de pe rândul sursă se păstrează. Un TIP gol produce IdFel NULL;
un TIP nenul care nu respectă forma „Felul n” oprește importul cu o constatare
X13. Asocierea TIP → IDFel este inclusă în VC_IdMap și în constatările N11.
Versiunea regulilor planului este `VC05-MEAL-TYPES-2026-10-15`.

Pentru date deja migrate este pregătit `sql/VC_Feluri_din_TIP.sql`, de executat
manual în baza DC după completarea ID-urilor subunității și gestiunii.
Completează nomenclatorul din TIP și corectează referințele celor trei mese,
fără ștergerea înregistrărilor existente sau modificarea schemei.

Nu s-au executat SQL-uri, importuri, compilări sau teste pentru această revizie.
