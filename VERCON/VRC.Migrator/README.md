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

1. Pornește `VRC.Migrator.exe`, **Adaugă baze…** (toate bazele subunității: MF, OI și toate tz_aaaa), **Citește**, apoi **Salvează raportul…**.
2. Fără fereastră: `VRC.Migrator.exe --profile raport.md baza1.accdb baza2.accdb` (cod de ieșire 0 = ok, 2 = o bază n-a putut fi citită).
3. Nu închide procese MSACCESS: dacă o bază este deschisă de cineva, hash-ul se citește oricum, iar raportul arată dacă s-a schimbat.

Schema așteptată este încorporată în executabil din `expected_schema.json`, generat cu `node VERCON/tools/expected.js` (din TZ_SCHEMA).
Parola comună a fișierelor se încearcă prin `KBot.Common.AccessFilePassword` și nu apare nicăieri.

x64, ca restul migratoarelor (furnizorul ACE este doar pe 64 de biți pe aceste stații; presupunerea că toate stațiile au ACE nu este verificată).
