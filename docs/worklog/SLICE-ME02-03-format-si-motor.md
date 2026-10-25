# SLICE-ME02-03…08 — Copiile locale: motor, server și operații

Reunire documentară D001-30. 03 definește motorul, 04 fluxul snap-run, 05 pagina/programarea și corecțiile operatorului, 06…08 operațiile offline. Corecțiile din 20.10.2026 retrag programarea automată din interfață, păstrează cheia pe PC și păstrează copiile manuale fără limită; afirmația veche «un an pentru manuale» din limitele 06…08 este anterioară acestei hotărâri. Exportul SQL înlocuiește încărcarea automată propusă în plan. [Registrul ME02](state/KBOT_STATUS_ME02.md) consemnează la 21.10 confirmarea operatorului pentru două copii locale după corecția SFTP ChangePermissions(…, 600); această confirmare nu este extinsă la Verifică, export, istoric, curățare, ștergere sau mesajul de la deschidere. Metadatele SQL [01](SLICE-ME02-01-date-creare-modificare.md) și dumpul server [02](SLICE-ME02-02-copie-manuala-toate-bazele.md) rămân distincte.

[ME02-03](#slice-me02-03) · [ME02-04](#slice-me02-04) · [ME02-05](#slice-me02-05) · [ME02-06](#slice-me02-06-07-08)

<a id="slice-me02-03"></a>

## ME02-03 — Formatul copiilor locale și motorul din KBotAdmin

20.10.2026. **IMPLEMENTAT ȘI COMPILAT (KBotAdmin, 0 avertismente). NERULAT: nici o copie făcută, nici o conexiune la server, nici un test.**

### Ce s-a cerut

Copii ale bazelor pe PC-ul operatorului, într-un format propriu, criptat (conținutul nu se vede), cu spațiu economisit și cu posibilitatea unui istoric pe momente. Format propriu hotărât de operator. Specificația: [ME02_FORMAT.md](../ME02_FORMAT.md).

### Ce s-a făcut (`KBotAdmin/Backup/`)

- `BackupCrypto.vb` — doar criptografie standard din .NET: PBKDF2-SHA256 (600.000 de iterații) → HKDF (cheie de criptare, cheie de denumire),
  AES-256-GCM (nonce aleator, tag de 16; numele fișierului intră în datele autentificate), HMAC-SHA256 pentru numele obiectelor, Brotli înainte de criptare.
- `BackupStore.vb` — dosarul: `store.kbk` (singurul fișier necriptat: sare, iterații, verificarea parolei), `o/xx/…` obiecte, `s/….snp` instantanee;
  scriere atomică (fișier temporar, `Flush(True)`, mutare), instantaneul se scrie **ultimul**, lacăt de scriere (`store.lock`), `Verify`
  (deschide fiecare obiect, recalculează numele, verifică obiectele fiecărui instantaneu).
- `BackupModels.vb` — manifestul (JSON): tabele → pagini (interval de chei, rânduri, valori de control, obiectul paginii), DDL și „extra”-uri
  (views, triggere, rutine, evenimente) ca obiecte.
- `BackupRunner.vb` — o rulare: trimite serverului paginile copiei precedente (fișier temporar în `backup/` pe server, prin SFTP, 0600), citește fluxul de
  înregistrări, scrie obiectele noi, scrie instantaneul. O bază eșuată nu oprește celelalte (`RunManyAsync`). Curățarea fișierului de pe server se face după `Try`
  (VB nu permite `Await` în `Finally`).
- `BackupKeyring.vb` — cheia păstrată pe acest calculator, protejată cu DPAPI (cont Windows curent), fișier `backup_key_<id>.dat` lângă program (în `.gitignore`).
- `BackupAccess.vb` — alege dosarul, creează sau deschide depozitul (parola de două ori la creare), folosește cheia păstrată dacă există.
- `AdminClient.StreamLinesAsync`, `UploadBytesAsync`, `DeleteRemoteAsync`; `SshSession.RunStreamingLean` (nu păstrează ieșirea, linii de ordinul MB);
  `SftpSession.UploadBytes` / `DeleteRemote`.
- `PassphraseForm` (+ Designer), setările noi din `admin_settings.json`: `BackupRoot`, `BackupKeepDaily/Weekly/Monthly`, `BackupFullEveryDays`.
- `KBotAdmin.vbproj`: pachetul `System.Security.Cryptography.ProtectedData` 8.0.0 (era deja în cache-ul NuGet).

### Hotărâri de proiectare (și de ce)

- **Pagini pe intervale de chei**, nu fișier pe rând: un obiect pe rând ar însemna milioane de fișiere; o pagină nemodificată e același obiect, deci nu ocupă loc nou.
- **Numele = HMAC cu cheie** a conținutului: dedup fără să se poată confirma un conținut cunoscut din numele fișierului.
- **Instantaneu complet**, nu lanț de diferențe: ștergerea unei copii vechi nu strică pe cele noi (retenția e simplă și sigură).
- Cifrul și derivarea cheii sunt cele din .NET; **nimic criptografic nu e inventat**.

### Nevalidat / limite

- **Nimic din acest cod nu a rulat.** Nici criptarea, nici scrierea atomică, nici citirea, nici DPAPI. Funcția «Verifică» (ME02-06) e menită să dovedească depozitul după prima copie reală.
- Nu am verificat dacă pe contul operatorului DPAPI deschide fișierul după o repornire sau după o schimbare de parolă Windows (dacă nu, rămâne parola din managerul de parole).
- Pe un depozit cu zeci de mii de obiecte, `Verify` citește totul; poate dura minute.
- Lanțul „PC oprit la ora sarcinii”: ratează ziua, nu strică nimic (vezi ME02-05).
- Parola fără recuperare: dacă se pierde, copiile nu se mai pot citi (spus în fereastra de creare).

### Fișiere

`KBotAdmin/Backup/*.vb` (nou, 9 fișiere), `KBotAdmin/PassphraseForm.vb|.Designer.vb` (nou), `KBotAdmin/AdminSettings.vb`, `AdminContext.vb` (proprietatea `LocalStore`),
`Server/AdminClient.vb`, `Ssh/SshSession.vb`, `Ssh/SftpSession.vb`, `KBotAdmin.vbproj`, `KBotAdmin/.gitignore`, `docs/ME02_FORMAT.md`.

---

<a id="slice-me02-04"></a>

## ME02-04 — Verbul de server `snap-run` (doar citire)

20.10.2026. **SCRIS; `py_compile` curat. NERULAT pe server, fără nicio conexiune la o bază.**

### Ce s-a făcut

`PYTHON/routes/admin_cli/admin_snapshot.py` (nou, ~380 de linii), înregistrat în `admin_cli.py`:

    snap-run --db D [--state-file P] [--mode fast|full] [--page-rows N] [--page-bytes N]

- O singură tranzacție `START TRANSACTION WITH CONSISTENT SNAPSHOT` (REPEATABLE READ), ca `--single-transaction`: copia nu prinde jumătate dintr-o operație. Nimic nu se scrie în baze.
- Fiecare tabelă e tăiată în **pagini pe intervale de cheie primară** care se ating și acoperă tot spațiul de chei (prima `lo = null`, ultima `hi = null`); chei compuse = tupluri.
  Tabelele fără cheie primară (sau cu cheie binară/generată) sunt o singură pagină.
- Valoarea de control a paginii o calculează **baza de date** (`COUNT(*)` + două `BIT_XOR` de MD5 pe rând). Clientul trimite paginile copiei precedente; cele cu aceeași valoare
  sunt răspunse doar prin descriere (`same`), fără să li se citească rândurile; cele schimbate se citesc din nou și se taie în pagini noi (500 de rânduri sau 4 MB).
  Un interval devenit gol rămâne ca pagină goală, ca intervalele să se atingă și data viitoare.
- Două valori de control: `sum` (rapidă: coloanele mari binare — PDF, imagini — contează prin LUNGIME când tabela are o coloană „modificat” de încredere: `ON UPDATE CURRENT_TIMESTAMP` sau `DataModif`)
  și `full` (MD5 pe conținut). Modul `full` compară `full`, ca să prindă un fișier înlocuit cu unul de aceeași lungime fără ca data să se schimbe.
- Coloanele generate (ex. `Clasificatii.Clsf*`, PERSISTENT) nu se salvează; DDL-ul se salvează fără `AUTO_INCREMENT=n` (se schimba la fiecare inserare).
- Valorile ies ca text (binarele în base64), pagina e comprimată cu zlib și pusă pe o linie JSON; views, triggere, rutine, evenimente ies ca înregistrări `extra`.
- `os.nice(10)`: copia nu trebuie să încetinească aplicația. Fișierul de stare trebuie să fie în `backup/` și e șters după citire.

Protocolul și forma înregistrărilor: antetul modulului și [ME02_FORMAT.md](../ME02_FORMAT.md).

### Verificat

- `py_compile` pe `admin_cli.py`, `admin_backup.py`, `admin_snapshot.py`. Revăzut de două ori pe hârtie (am găsit și corectat: golul de interval la ultima pagină, tabelele fără cheie
  care nu se mai recunoșteau, compararea blob-urilor între moduri, un `AUTO_INCREMENT` care schimba DDL-ul la fiecare rulare).

### Nevalidat / de știut înainte de prima rulare

- **Nu a rulat pe o bază reală.** Chestiuni care se văd doar acolo: sintaxa `(a,b) > (%s,%s)` pe chei compuse cu indexul folosit; performanța pe tabele mari (o interogare de control pe pagină);
  comparația unei chei numerice trimise ca text; `start_transaction(consistent_snapshot=True)` pe conexiunea cu `autocommit` pornit; `cursor(raw=True)` pe extensia C a conectorului.
- Un rând cu coloanele însumate mai mari decât `max_allowed_packet` face `CONCAT_WS` să dea NULL: rândul nu intră în valoarea de control (limită scrisă în antet).
- Chei `float`/`double` nu sunt potrivite (compararea ca text); nu există în șablon.
- Primul test recomandat: **o singură bază mică**, nu toate.
- Fișierele noi (`admin_backup.py`, `admin_snapshot.py`, `admin_cli.py` modificat) le trimiteți dumneavoastră pe server; `admin_cli` pornește la fiecare apel SSH, deci **nu e nevoie de repornirea serviciului**.

### Fișiere

`PYTHON/routes/admin_cli/admin_snapshot.py` (nou), `PYTHON/routes/admin_cli/admin_cli.py` (înregistrare + antet).

---

<a id="slice-me02-05"></a>

## ME02-05 — Pagina «Copii locale» și copia periodică

20.10.2026. **IMPLEMENTAT ȘI COMPILAT (KBotAdmin, 0 avertismente). NERULAT, NEVĂZUT PE ECRAN.**

### Ce s-a făcut

**Pagina «Copii locale»** (nouă; fosta «Copii» s-a redenumit «Copii server»): `Views/LocalBackupView.vb|.Designer.vb`, înregistrată în `MainForm`.

- Butoane: **Dosar...** (alege dosarul, salvat în `admin_settings.json`), **Deschide** (citește lista, cere parola dacă cheia nu e păstrată), **Copie acum**,
  **Verifică**, **Export SQL**, **Istoric rând**, **Curăță**, **Programează**.
- **Copie acum**: baza aleasă în arbore, sau toate bazele de unitate fără `000_DEMO` pentru `<TOATE BAZELE>`; cere un motiv comun (`ReasonForm`, obligatoriu); pentru fiecare bază
  trimite serverului lista copiei precedente și scrie doar paginile noi. O bază eșuată nu le oprește pe celelalte; la eșec o fereastră spune care și de ce.
- Lista copiilor (stânga, cele mai noi sus, filtrată după baza aleasă în arbore) și, pentru copia aleasă, tabelele ei cu **câte pagini s-au schimbat față de copia precedentă** a aceleiași baze (dreapta).
- Linia de stare: dosarul, câte copii, cât ocupă pe disc, avertisment dacă vreo copie nu se poate deschide.

**Copia periodică** (`Backup/BackupHeadless.vb`): `KBotAdmin.exe --backup` rulează fără fereastră: deschide depozitul cu **cheia păstrată pe calculator** (nu cere parola), citește lista bazelor, copiază fiecare bază de unitate (fără `000_DEMO`) cu motivul „periodic”, apoi aplică retenția. Modul e `full` pentru o bază fără nicio copie `full` sau cu ultima mai veche de `BackupFullEveryDays` (7), altfel `fast`. Rezultatul se scrie în `Logs\copii_locale.log`; codul de ieșire: 0 totul bine, 1 vreo bază eșuată, 2 nu a putut porni. Butonul **Programează** creează (sau șterge, dacă există) sarcina Windows «KBotAdmin copii locale»: zilnic la 02:00, `-StartWhenAvailable` (dacă PC-ul era oprit, pornește la prima ocazie), doar cât timp utilizatorul e conectat la Windows. Cere întâi păstrarea cheii pe calculator. Apasă dumneavoastră butonul; nimic nu se programează singur.

### Corecții după hotărârile operatorului (20.10.2026, după prima predare)

- **Fără copie automată de noapte.** Butonul «Programează» a fost scos din pagină (în locul lui e «Șterge»). Codul `--backup` și `BackupScheduler` au rămas în `BackupHeadless.vb`,
  neatinse de nicio interfață (nu se șterg, dar nu se pornesc); sarcina Windows nu se creează singură.
- **Mesaj la deschiderea KBotAdmin**: dacă azi nu s-a făcut nicio copie locală, programul întreabă «O faceți acum?»; la «Da» deschide pagina «Copii locale» și pornește «Copie acum»
  (cere motivul). Dacă nu e configurat nimic, întreabă la fel. Dacă depozitul există, dar cheia nu e păstrată pe calculator, nu poate ști și propune doar deschiderea paginii.
  Se verifică prin ziua LOCALĂ a PC-ului. (`MainForm.RemindLocalBackupAsync`)
- **Cheia rămâne păstrată pe PC** (DPAPI), aplicația nu pleacă de pe acest calculator. Bifa e pe «da» implicit în fereastra parolei.
- **Ora**: serverul scrie acum orele în UTC (`PYTHON/docs/TIMEZONES.md`); lista «Copii server» le arată convertite la ora PC-ului. Manifestele copiilor locale poartă ora cu offset.

### Nevalidat / limite

- **Nimic nu a rulat**: nici pagina pe ecran (aranjarea celor 8 butoane și a 16 intrări din bara de pagini, la lățimea ferestrei, nu am văzut-o), nici sarcina Windows (`New-ScheduledTask…` din PowerShell 5.1), nici o copie.
- Copia periodică merge doar cu **o cheie SSH fără parolă** (ca la publicare) — altfel iese cu codul 2.
- Cât timp o copie rulează, apelurile obișnuite către server din KBotAdmin așteaptă (o singură sesiune SSH, serializată); bara „ocupat” se vede.
- Dacă PC-ul e oprit la 02:00, copia pornește la prima ocazie după pornire; dacă nu pornește deloc câteva zile, nu avertizează încă nimeni (nu există alertă de „ultima copie e veche”).
- Anularea unei copii în curs nu există (se așteaptă sfârșitul sau se închide programul; instantaneul rămâne nescris, obiectele scrise rămân și se curăță la «Curăță»).

### Fișiere

`KBotAdmin/Views/LocalBackupView.vb|.Designer.vb` (nou), `KBotAdmin/Backup/BackupHeadless.vb` (nou; include `BackupScheduler`), `KBotAdmin/Program.vb` (comutatorul `--backup`),
`KBotAdmin/MainForm.vb|.Designer.vb` (intrarea «Copii locale»; «Copii» → «Copii server»).

---

<a id="slice-me02-06-07-08"></a>

## ME02-06 / ME02-07 / ME02-08 — verificare + export SQL, istoricul unui rând, retenție

20.10.2026. **IMPLEMENTAT ȘI COMPILAT (KBotAdmin, 0 avertismente). NERULAT.**

<a id="slice-me02-06"></a>

### ME02-06 — Verificare și restaurare

- **Verifică** (`BackupStore.Verify`): deschide fiecare obiect (nume = HMAC-ul conținutului), fiecare instantaneu, controlează că fiecare copie își găsește toate obiectele;
  raportează fișiere stricate, lipsă și „fără stăpân”. Nu atinge serverul.
- **Export SQL** (`Backup/BackupSql.vb`): scrie o copie (toate tabelele, apoi views/triggere/rutine/evenimente) ca fișier SQL (`.sql` sau `.sql.gz`) pentru o **bază de probă** `zz_restore_<baza>_<data>`.
  Numele țintă trebuie să înceapă cu `zz_`: fișierul nu poate suprascrie o bază vie. Valorile sunt scrise ca text între ghilimele (MariaDB le convertește), binarele ca `0x…`;
  `FOREIGN_KEY_CHECKS=0` cât se încarcă; `DEFINER` scos din views/triggere. Nu atinge nicio bază: fișierul îl rulează operatorul.
- **Abatere de la plan**: planul vorbea despre încărcarea într-o bază de probă prin server. Am ales exportul ca fișier SQL: nu cere nicio scriere nouă pe server, iar rularea o decide operatorul.
  Încărcarea automată pe server rămâne **neconstruită**.

<a id="slice-me02-07"></a>

### ME02-07 — «Istoricul unui rând» (time-machine)

- Fereastra `RecordHistoryForm`: tabela aleasă (cu o copie selectată în pagina «Copii locale»), cheia primară scrisă de operator (părțile separate prin `;`), apoi lista tuturor copiilor
  bazei: unde rândul **apare / e neschimbat / modificat / a dispărut / nu există**, cu coloanele schimbate; jos, valorile rândului în copia aleasă, cu valoarea de dinainte la coloanele schimbate.
- **Copiază SQL pentru această versiune**: pune în clipboard `INSERT … ON DUPLICATE KEY UPDATE` care readuce rândul la acea versiune. Nu îl rulează.
- Citește doar din depozitul local; o pagină identică în mai multe copii se deschide o singură dată.
- Limita onestă: istoricul e la fel de fin ca frecvența copiilor (zilnic ⇒ nu se vede ce s-a întâmplat între două rulări). Pagina «Copii locale» arată și, pe fiecare tabelă a unei copii, câte pagini s-au schimbat față de copia precedentă.
- **Neconstruit**: răsfoirea tuturor rândurilor unei tabele la un moment dat (există doar exportul SQL și istoricul pe cheie); comparația completă între două date.

<a id="slice-me02-08"></a>

### ME02-08 — Retenție

- `BackupRetention`: pentru fiecare bază păstrează ultima copie a fiecăreia dintre ultimele **N zile**, **N săptămâni ISO**, **N luni** (14 / 8 / 12, în `admin_settings.json`), ultima copie oricât de veche,
  **toate copiile făcute de mână, fără limită de timp** (hotărâre operator: se șterg doar de mână) și orice copie cu data ilizibilă. Șterge restul și fișierele la care nu mai trimite nimic (inclusiv resturi ale unei rulări eșuate).
  Cum toate copiile sunt acum manuale (fără copie automată), «Curăță» nu mai șterge practic decât resturi de fișiere.
- Butonul **Șterge**: șterge copia aleasă (cu confirmare) și fișierele pe care nu le mai folosește nicio altă copie. Dacă vreo copie nu se poate deschide, nu șterge fișiere (ca să nu piardă ceva de care are nevoie), ci cere întâi «Verifică».
- **Curăță** arată întâi ce se șterge și cât se eliberează, cere confirmare; ia lacătul depozitului (nu rulează cât se scrie o copie) și recalculează ce e „fără stăpân” după lacăt.
- Se aplică și automat la sfârșitul copiei periodice.

### Nevalidat / limite

- **Nimic nu a rulat.** Mai ales exportul SQL trebuie încercat pe o bază mică într-un MariaDB de probă (potrivirea tipurilor, coloanele generate, ordinea creării) înainte să te bazezi pe el.
- Ghilimelele din jurul numerelor sunt valide în MariaDB cu `sql_mode` fără strict; fișierul pune `sql_mode='NO_AUTO_VALUE_ON_ZERO'`.
- Istoricul unui rând caută cheia prin toate paginile tabelei (nu folosește intervalele), deci pe tabele foarte mari prima căutare e lentă.
- Regulile de păstrare (14/8/12 și un an pentru manuale) sunt alegerea mea, nu a operatorului: se schimbă în `admin_settings.json`.

### Fișiere

`KBotAdmin/Backup/BackupSql.vb`, `BackupReader.vb`, `BackupRetention.vb` (noi), `KBotAdmin/RecordHistoryForm.vb|.Designer.vb` (nou).
