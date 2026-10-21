# ME02-05 — Pagina «Copii locale» și copia periodică

20.10.2026. **IMPLEMENTAT ȘI COMPILAT (KBotAdmin, 0 avertismente). NERULAT, NEVĂZUT PE ECRAN.**

## Ce s-a făcut

**Pagina «Copii locale»** (nouă; fosta «Copii» s-a redenumit «Copii server»): `Views/LocalBackupView.vb|.Designer.vb`, înregistrată în `MainForm`.

- Butoane: **Dosar...** (alege dosarul, salvat în `admin_settings.json`), **Deschide** (citește lista, cere parola dacă cheia nu e păstrată), **Copie acum**,
  **Verifică**, **Export SQL**, **Istoric rând**, **Curăță**, **Programează**.
- **Copie acum**: baza aleasă în arbore, sau toate bazele de unitate fără `000_DEMO` pentru `<TOATE BAZELE>`; cere un motiv comun (`ReasonForm`, obligatoriu); pentru fiecare bază
  trimite serverului lista copiei precedente și scrie doar paginile noi. O bază eșuată nu le oprește pe celelalte; la eșec o fereastră spune care și de ce.
- Lista copiilor (stânga, cele mai noi sus, filtrată după baza aleasă în arbore) și, pentru copia aleasă, tabelele ei cu **câte pagini s-au schimbat față de copia precedentă** a aceleiași baze (dreapta).
- Linia de stare: dosarul, câte copii, cât ocupă pe disc, avertisment dacă vreo copie nu se poate deschide.

**Copia periodică** (`Backup/BackupHeadless.vb`): `KBotAdmin.exe --backup` rulează fără fereastră: deschide depozitul cu **cheia păstrată pe calculator** (nu cere parola), citește lista bazelor, copiază
fiecare bază de unitate (fără `000_DEMO`) cu motivul „periodic”, apoi aplică retenția. Modul e `full` pentru o bază fără nicio copie `full` sau cu ultima mai veche de `BackupFullEveryDays` (7), altfel `fast`.
Rezultatul se scrie în `Logs\copii_locale.log`; codul de ieșire: 0 totul bine, 1 vreo bază eșuată, 2 nu a putut porni.
Butonul **Programează** creează (sau șterge, dacă există) sarcina Windows «KBotAdmin copii locale»: zilnic la 02:00, `-StartWhenAvailable` (dacă PC-ul era oprit, pornește la prima ocazie),
doar cât timp utilizatorul e conectat la Windows. Cere întâi păstrarea cheii pe calculator. Apasă dumneavoastră butonul; nimic nu se programează singur.

## Corecții după hotărârile operatorului (20.10.2026, după prima predare)

- **Fără copie automată de noapte.** Butonul «Programează» a fost scos din pagină (în locul lui e «Șterge»). Codul `--backup` și `BackupScheduler` au rămas în `BackupHeadless.vb`,
  neatinse de nicio interfață (nu se șterg, dar nu se pornesc); sarcina Windows nu se creează singură.
- **Mesaj la deschiderea KBotAdmin**: dacă azi nu s-a făcut nicio copie locală, programul întreabă «O faceți acum?»; la «Da» deschide pagina «Copii locale» și pornește «Copie acum»
  (cere motivul). Dacă nu e configurat nimic, întreabă la fel. Dacă depozitul există, dar cheia nu e păstrată pe calculator, nu poate ști și propune doar deschiderea paginii.
  Se verifică prin ziua LOCALĂ a PC-ului. (`MainForm.RemindLocalBackupAsync`)
- **Cheia rămâne păstrată pe PC** (DPAPI), aplicația nu pleacă de pe acest calculator. Bifa e pe «da» implicit în fereastra parolei.
- **Ora**: serverul scrie acum orele în UTC (`PYTHON/docs/TIMEZONES.md`); lista «Copii server» le arată convertite la ora PC-ului. Manifestele copiilor locale poartă ora cu offset.

## Nevalidat / limite

- **Nimic nu a rulat**: nici pagina pe ecran (aranjarea celor 8 butoane și a 16 intrări din bara de pagini, la lățimea ferestrei, nu am văzut-o), nici sarcina Windows (`New-ScheduledTask…` din PowerShell 5.1), nici o copie.
- Copia periodică merge doar cu **o cheie SSH fără parolă** (ca la publicare) — altfel iese cu codul 2.
- Cât timp o copie rulează, apelurile obișnuite către server din KBotAdmin așteaptă (o singură sesiune SSH, serializată); bara „ocupat” se vede.
- Dacă PC-ul e oprit la 02:00, copia pornește la prima ocazie după pornire; dacă nu pornește deloc câteva zile, nu avertizează încă nimeni (nu există alertă de „ultima copie e veche”).
- Anularea unei copii în curs nu există (se așteaptă sfârșitul sau se închide programul; instantaneul rămâne nescris, obiectele scrise rămân și se curăță la «Curăță»).

## Fișiere

`KBotAdmin/Views/LocalBackupView.vb|.Designer.vb` (nou), `KBotAdmin/Backup/BackupHeadless.vb` (nou; include `BackupScheduler`), `KBotAdmin/Program.vb` (comutatorul `--backup`),
`KBotAdmin/MainForm.vb|.Designer.vb` (intrarea «Copii locale»; «Copii» → «Copii server»).
