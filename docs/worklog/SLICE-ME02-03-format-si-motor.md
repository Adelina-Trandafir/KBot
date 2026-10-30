# SLICE-ME02-03…08 — Copiile locale: motor, server și operații

Reunire documentară D001-30. 03 definește motorul, 04 fluxul snap-run, 05 pagina/programarea și corecțiile operatorului, 06…08 operațiile offline. Corecțiile din 20.10.2026 retrag programarea automată din interfață, păstrează cheia pe PC și păstrează copiile manuale fără limită; afirmația veche «un an pentru manuale» din limitele 06…08 este anterioară acestei hotărâri. Exportul SQL înlocuiește încărcarea automată propusă în plan. [Registrul ME02](state/KBOT_STATUS_ME02.md) consemnează la 21.10 confirmarea operatorului pentru două copii locale după corecția SFTP ChangePermissions(…, 600); această confirmare nu este extinsă la Verifică, export, istoric, curățare, ștergere sau mesajul de la deschidere. Metadatele SQL [01](SLICE-ME02-01-date-creare-modificare.md) și dumpul server [02](SLICE-ME02-02-copie-manuala-toate-bazele.md) rămân distincte.

[ME02-03](#slice-me02-03) · [ME02-04](#slice-me02-04) · [ME02-05](#slice-me02-05) · [ME02-06](#slice-me02-06-07-08)

<a id="slice-me02-03"></a>

## ME02-03 — Formatul copiilor locale și motorul din KBotAdmin

20.10.2026. **IMPLEMENTAT ȘI COMPILAT (KBotAdmin, 0 avertismente). NERULAT: nici o copie făcută, nici o conexiune la server, nici un test.**

### Ce s-a cerut

Operatorul a ales formatul propriu: copii criptate pe PC, economie de spațiu și istoric pe momente. Specificația: [ME02_FORMAT.md](../ME02_FORMAT.md).

### Ce s-a făcut (`KBotAdmin/Backup/`)

- `BackupCrypto.vb` — criptografie standard .NET: PBKDF2-SHA256 (600.000 iterații) → HKDF (chei de criptare/denumire), AES-256-GCM (nonce aleator, tag de 16; numele fișierului autentificat), HMAC-SHA256 pentru nume, Brotli înaintea criptării.
- `BackupStore.vb` — `store.kbk` (singurul necriptat: sare, iterații, verificarea parolei), `o/xx/…` obiecte, `s/….snp` instantanee; scriere atomică (temporar, `Flush(True)`, mutare), instantaneul **ultimul**, lacăt `store.lock`; `Verify` este descris în [ME02-06](#slice-me02-06).
- `BackupModels.vb` — manifestul (JSON): tabele → pagini (interval de chei, rânduri, valori de control, obiectul paginii), DDL și „extra”-uri (views, triggere, rutine, evenimente) ca obiecte.
- `BackupRunner.vb` — trimite paginile precedente prin SFTP (temporar în `backup/`, 0600), citește fluxul, scrie obiectele noi, apoi instantaneul. `RunManyAsync` continuă după eșecul unei baze. Curăță temporarul după `Try`: VB nu permite `Await` în `Finally`.
- `BackupKeyring.vb` — cheia păstrată pe acest calculator, protejată cu DPAPI (cont Windows curent), fișier `backup_key_<id>.dat` lângă program (în `.gitignore`).
- `BackupAccess.vb` — alege dosarul, creează sau deschide depozitul (parola de două ori la creare), folosește cheia păstrată dacă există.
- `AdminClient.StreamLinesAsync`, `UploadBytesAsync`, `DeleteRemoteAsync`; `SshSession.RunStreamingLean` (nu păstrează ieșirea, linii de ordinul MB); `SftpSession.UploadBytes` / `DeleteRemote`.
- `PassphraseForm` (+ Designer), setările noi din `admin_settings.json`: `BackupRoot`, `BackupKeepDaily/Weekly/Monthly`, `BackupFullEveryDays`.
- `KBotAdmin.vbproj`: pachetul `System.Security.Cryptography.ProtectedData` 8.0.0 (era deja în cache-ul NuGet).

### Hotărâri de proiectare (și de ce)

Paginile evită milioane de fișiere per rând și refolosesc obiectele nemodificate. HMAC-ul permite deduplicarea fără confirmarea unui conținut cunoscut din nume; primitivele .NET sunt cele enumerate mai sus. Instantaneele complete permit ștergerea copiilor vechi fără afectarea celor noi.

### Nevalidat / limite

- **Niciun cod al etapei nu a rulat:** inclusiv criptarea, scrierea atomică, citirea și DPAPI. «Verifică» (ME02-06) este prevăzut după prima copie reală.
- DPAPI după repornire sau schimbarea parolei Windows pe contul operatorului este nevalidat; rezerva rămâne parola din managerul de parole.
- Pe un depozit cu zeci de mii de obiecte, `Verify` citește totul; poate dura minute.
- Lanțul „PC oprit la ora sarcinii”: ratează ziua, nu strică nimic (vezi ME02-05).
- Parola fără recuperare: dacă se pierde, copiile nu se mai pot citi (spus în fereastra de creare).

### Fișiere

`KBotAdmin/Backup/*.vb` (nou, 9 fișiere), `KBotAdmin/PassphraseForm.vb|.Designer.vb` (nou), `KBotAdmin/AdminSettings.vb`, `AdminContext.vb` (proprietatea `LocalStore`), `Server/AdminClient.vb`, `Ssh/SshSession.vb`, `Ssh/SftpSession.vb`, `KBotAdmin.vbproj`, `KBotAdmin/.gitignore`, `docs/ME02_FORMAT.md`.

---

<a id="slice-me02-04"></a>

## ME02-04 — Verbul de server `snap-run` (doar citire)

20.10.2026. **SCRIS; `py_compile` curat. NERULAT pe server, fără nicio conexiune la o bază.**

### Ce s-a făcut

`PYTHON/routes/admin_cli/admin_snapshot.py` (nou, ~380 de linii), înregistrat în `admin_cli.py`:

    snap-run --db D [--state-file P] [--mode fast|full] [--page-rows N] [--page-bytes N]

- O singură tranzacție `START TRANSACTION WITH CONSISTENT SNAPSHOT` (REPEATABLE READ), ca `--single-transaction`: copia nu prinde jumătate dintr-o operație. Nimic nu se scrie în baze.
- Paginarea, `same`, recitirea intervalelor schimbate (500 rânduri/4 MB) și `sum`/`full` urmează [specificația KBK1](../ME02_FORMAT.md#cum-se-alege-ce-se-citește). Implementarea folosește tupluri pentru cheile compuse; cheile absente, binare **sau generate** dau o singură pagină. Intervalul devenit gol rămâne pagină goală, păstrând acoperirea continuă (`lo = null` la început, `hi = null` la sfârșit).
- Baza calculează `COUNT(*)` + două `BIT_XOR` de MD5 pe rând. În `sum`, binarele mari (PDF/imagini) contează prin lungime numai cu `ON UPDATE CURRENT_TIMESTAMP` sau `DataModif` de încredere; `full` include MD5-ul conținutului și detectează înlocuirea la aceeași lungime fără schimbarea datei.
- Coloanele generate (ex. `Clasificatii.Clsf*`, PERSISTENT) nu se salvează; DDL-ul se salvează fără `AUTO_INCREMENT=n` (se schimba la fiecare inserare).
- Valorile ies ca text (binarele în base64), pagina e comprimată cu zlib și pusă pe o linie JSON; views, triggere, rutine, evenimente ies ca înregistrări `extra`.
- `os.nice(10)`: copia nu trebuie să încetinească aplicația. Fișierul de stare trebuie să fie în `backup/` și e șters după citire.

Protocolul și forma înregistrărilor: antetul modulului și [ME02_FORMAT.md](../ME02_FORMAT.md).

### Verificat

- `py_compile` pe `admin_cli.py`, `admin_backup.py`, `admin_snapshot.py`. Revăzut de două ori pe hârtie (am găsit și corectat: golul de interval la ultima pagină, tabelele fără cheie care nu se mai recunoșteau, compararea blob-urilor între moduri, un `AUTO_INCREMENT` care schimba DDL-ul la fiecare rulare).

### Nevalidat / de știut înainte de prima rulare

- **Nerulat pe bază reală:** sintaxa/indexul `(a,b) > (%s,%s)`, performanța pe tabele mari (o interogare de control/pagină), cheia numerică transmisă ca text, `start_transaction(consistent_snapshot=True)` cu `autocommit` pornit, `cursor(raw=True)` în extensia C a conectorului.
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

- Butoane: **Dosar...** (alege dosarul, salvat în `admin_settings.json`), **Deschide** (citește lista, cere parola dacă cheia nu e păstrată), **Copie acum**, **Verifică**, **Export SQL**, **Istoric rând**, **Curăță**, **Programează**.
- **Copie acum**: baza din arbore sau, pentru `<TOATE BAZELE>`, toate bazele de unitate fără `000_DEMO`; motiv comun obligatoriu (`ReasonForm`). Folosește `BackupRunner` descris în ME02-03; fereastra de eșec enumeră bazele și cauzele.
- Lista copiilor (stânga, cele mai noi sus, filtrată după baza aleasă în arbore) și, pentru copia aleasă, tabelele ei cu **câte pagini s-au schimbat față de copia precedentă** a aceleiași baze (dreapta).
- Linia de stare: dosarul, câte copii, cât ocupă pe disc, avertisment dacă vreo copie nu se poate deschide.

**Copia periodică** (`Backup/BackupHeadless.vb`): `KBotAdmin.exe --backup` rulează fără fereastră: deschide depozitul cu **cheia păstrată pe calculator** (nu cere parola), citește lista bazelor, copiază fiecare bază de unitate (fără `000_DEMO`) cu motivul „periodic”, apoi aplică retenția. Modul e `full` pentru o bază fără nicio copie `full` sau cu ultima mai veche de `BackupFullEveryDays` (7), altfel `fast`. Rezultatul se scrie în `Logs\copii_locale.log`; codul de ieșire: 0 totul bine, 1 vreo bază eșuată, 2 nu a putut porni. Butonul **Programează** creează (sau șterge, dacă există) sarcina Windows «KBotAdmin copii locale»: zilnic la 02:00, `-StartWhenAvailable` (dacă PC-ul era oprit, pornește la prima ocazie), doar cât timp utilizatorul e conectat la Windows. Cere întâi păstrarea cheii pe calculator. Apasă dumneavoastră butonul; nimic nu se programează singur.

### Corecții după hotărârile operatorului (20.10.2026, după prima predare)

- **Fără copie automată de noapte.** Butonul «Programează» a fost scos din pagină (în locul lui e «Șterge»). Codul `--backup` și `BackupScheduler` au rămas în `BackupHeadless.vb`, neatinse de nicio interfață (nu se șterg, dar nu se pornesc); sarcina Windows nu se creează singură.
- **Mesaj la deschiderea KBotAdmin**: dacă azi nu s-a făcut nicio copie locală, programul întreabă «O faceți acum?»; la «Da» deschide pagina «Copii locale» și pornește «Copie acum» (cere motivul). Dacă nu e configurat nimic, întreabă la fel. Dacă depozitul există, dar cheia nu e păstrată pe calculator, nu poate ști și propune doar deschiderea paginii. Se verifică prin ziua LOCALĂ a PC-ului. (`MainForm.RemindLocalBackupAsync`)
- **Cheia rămâne păstrată pe PC** (DPAPI), aplicația nu pleacă de pe acest calculator. Bifa e pe «da» implicit în fereastra parolei.
- **Ora**: serverul scrie acum orele în UTC (`PYTHON/docs/TIMEZONES.md`); lista «Copii server» le arată convertite la ora PC-ului. Manifestele copiilor locale poartă ora cu offset.

### Nevalidat / limite

- **Nimic nu a rulat**: nici pagina pe ecran (aranjarea celor 8 butoane și a 16 intrări din bara de pagini, la lățimea ferestrei, nu am văzut-o), nici sarcina Windows (`New-ScheduledTask…` din PowerShell 5.1), nici o copie.
- Copia periodică merge doar cu **o cheie SSH fără parolă** (ca la publicare) — altfel iese cu codul 2.
- Cât timp o copie rulează, apelurile obișnuite către server din KBotAdmin așteaptă (o singură sesiune SSH, serializată); bara „ocupat” se vede.
- Dacă PC-ul e oprit la 02:00, copia pornește la prima ocazie după pornire; dacă nu pornește deloc câteva zile, nu avertizează încă nimeni (nu există alertă de „ultima copie e veche”).
- Anularea unei copii în curs nu există (se așteaptă sfârșitul sau se închide programul; instantaneul rămâne nescris, obiectele scrise rămân și se curăță la «Curăță»).

### Fișiere

`KBotAdmin/Views/LocalBackupView.vb|.Designer.vb` (nou), `KBotAdmin/Backup/BackupHeadless.vb` (nou; include `BackupScheduler`), `KBotAdmin/Program.vb` (comutatorul `--backup`), `KBotAdmin/MainForm.vb|.Designer.vb` (intrarea «Copii locale»; «Copii» → «Copii server»).

---

<a id="slice-me02-06-07-08"></a>

## ME02-06 / ME02-07 / ME02-08 — verificare + export SQL, istoricul unui rând, retenție

20.10.2026. **IMPLEMENTAT ȘI COMPILAT (KBotAdmin, 0 avertismente). NERULAT.**

<a id="slice-me02-06"></a>

### ME02-06 — Verificare și restaurare

- **Verifică** (`BackupStore.Verify`): deschide toate obiectele/instantaneele, recalculează numele HMAC, verifică referințele; raportează fișiere stricate, lipsă și „fără stăpân”. Numai local.
- **Export SQL** (`Backup/BackupSql.vb`): scrie o copie (toate tabelele, apoi views/triggere/rutine/evenimente) ca fișier SQL (`.sql` sau `.sql.gz`) pentru o **bază de probă** `zz_restore_<baza>_<data>`. Numele țintă trebuie să înceapă cu `zz_`: fișierul nu poate suprascrie o bază vie. Valorile sunt scrise ca text între ghilimele (MariaDB le convertește), binarele ca `0x…`; `FOREIGN_KEY_CHECKS=0` cât se încarcă; `DEFINER` scos din views/triggere. Nu atinge nicio bază: fișierul îl rulează operatorul.
- **Abatere de la plan:** export SQL decis pentru a lăsa scrierea pe server operatorului; încărcarea automată într-o bază de probă rămâne **neconstruită**.

<a id="slice-me02-07"></a>

### ME02-07 — «Istoricul unui rând» (time-machine)

- Fereastra `RecordHistoryForm`: tabela aleasă (cu o copie selectată în pagina «Copii locale»), cheia primară scrisă de operator (părțile separate prin `;`), apoi lista tuturor copiilor bazei: unde rândul **apare / e neschimbat / modificat / a dispărut / nu există**, cu coloanele schimbate; jos, valorile rândului în copia aleasă, cu valoarea de dinainte la coloanele schimbate.
- **Copiază SQL pentru această versiune**: pune în clipboard `INSERT … ON DUPLICATE KEY UPDATE` care readuce rândul la acea versiune. Nu îl rulează.
- Citește doar din depozitul local; o pagină identică în mai multe copii se deschide o singură dată.
- Istoricul are granularitatea copiilor (zilnic ⇒ fără schimbările dintre rulări); numărul paginilor schimbate este cel descris la pagina ME02-05.
- **Neconstruit**: răsfoirea tuturor rândurilor unei tabele la un moment dat (există doar exportul SQL și istoricul pe cheie); comparația completă între două date.

<a id="slice-me02-08"></a>

### ME02-08 — Retenție

- `BackupRetention`: pentru fiecare bază păstrează ultima copie a fiecăreia dintre ultimele **N zile**, **N săptămâni ISO**, **N luni** (14 / 8 / 12, în `admin_settings.json`), ultima copie oricât de veche, **toate copiile făcute de mână, fără limită de timp** (hotărâre operator: se șterg doar de mână) și orice copie cu data ilizibilă. Șterge restul și fișierele la care nu mai trimite nimic (inclusiv resturi ale unei rulări eșuate). Cum toate copiile sunt acum manuale (fără copie automată), «Curăță» nu mai șterge practic decât resturi de fișiere.
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
