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

Reguli decise (12.10.2026): numerotarea pornește de la ultimul număr real, nu din NR.*; ProduseP se transferă doar cu folosire; preț negativ în Intrari/Iesiri → preț pozitiv + cantitate negativă (avertisment), ambele negative blochează; NIR negativ doar pentru stoc inițial («stoc» în NrFact); total antet după regula N01/N02. Detalii: `DECIZII_DESCHISE.md`, secțiunea F. Schema țintă a scriitorului este `target_schema.json`, generat de `node VERCON/tools/ddl.js`.
