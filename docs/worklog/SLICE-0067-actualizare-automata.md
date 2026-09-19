# SLICE-0067 — Actualizarea automată a aplicației (server Flask + `push-update.ps1` + `KBot.Updater`)

**Data:** 18.09.2026
**Cerere (operator):**

> i need a system to update the app whenever i make changes. this will be a new slice. we can use
> the python server for the updates. i also need a ps which i will run when i consider a viable
> update exists and which will create and push the update file. any questions you have, ask me.
> don't assume.
>
> extra: don't edit either KbotForm or the LoginForm until everything is done and i accept!
>
> (apoi) NO FUCKING TESTS

Numărul feliei: 0067 = «Next free» din STATUS.

## 0. Hotărârile operatorului (întrebate, nu presupuse)

Toate cele de mai jos sunt răspunsuri date la întrebări explicite în 18.09.2026:

| Întrebare | Hotărâre |
|---|---|
| Cum se aplică pe PC-ul clientului | **`KBot.Updater.exe` separat + zip** (nu instalatorul Inno rulat silențios, nu rename-swap în proces) |
| Unitatea actualizării | **Toată aplicația, un singur număr de versiune** (nu per-componentă prin `manifest.xml`) |
| Ce număr hotărăște «e mai nou» | **`FileVersion` din `KBot.App.vbproj`, incrementat de operator, de mână** |
| Cum ajunge pachetul pe server | **SFTP, ca AvacontPush** (nu POST HTTP — plafonul e 17 MB, pachetul ~40 MB) |
| Când se verifică | **La pornire, înainte de login, opțional** + **buton de re-verificare** în aplicație; **unele actualizări obligatorii, altele nu** |
| Build-urile Debug | **Nu verifică niciodată** (doar Release) |
| Scriptul | **Construiește + împinge într-o singură rulare** (cheamă `publish-release.ps1`) |
| Ce nu se suprascrie | **Tot în afară de `Logs\`** |
| «Obligatoriu» | **`latest` + `minimum`** pe server: sub `minimum` = obligatoriu chiar dacă s-au sărit versiuni opționale |
| Credențialele SFTP | **`Host/Port/User/RemoteRoot` din `PYTHON\_push\push_settings.json`, parola tastată la fiecare rulare** |
| Cine servește descărcarea | **Flask, `send_file`** (nu nginx static) |
| Butonul «Caută actualizări» | **Și în LoginForm, și în KbotForm** (același drum) |
| Clientul SFTP | **OpenSSH `sftp.exe` din Windows, batch mode** (WinSCP nu e instalat; SSH.NET nu se poate împrumuta din exe-ul single-file) |
| Fereastra updater-ului | **WinForms simplu, fără referințe KBot** (rulează cât timp Theming/Controls sunt suprascrise) |
| Folderul pe server | **`/root/AVACONT/updates`** = implicitul din cod (`config.UPDATE_DIR` îl poate muta) |
| Protecția rutelor | **GET public, fără autentificare** — clientul n-are niciun secret înainte de login (`ApiOptions`: doar bearer), iar pachetul e chiar instalatorul |

Un fapt care a schimbat o hotărâre pe drum: `ApiOptions.vb` spune că **nu mai există nicio cheie
API client-side** (memoria despre `KBOT_API_KEY` era din designul vechi). De aici întrebarea
despre protecția rutelor.

## 1. Ce s-a făcut și de ce

### 1.1 Server — `PYTHON/routes/update.py` (nou), înregistrat în `main.py`

Două rute **publice**:

- `GET /api/update/latest` → `{version, minimum, file, size, sha256, published_utc, notes}`
  citit din `<UPDATE_DIR>/latest.json`; **404** `NO_UPDATE_PUBLISHED` când nu s-a publicat
  nimic; **500** `LATEST_INVALID` când fișierul e stricat (validare: chei obligatorii, versiuni
  `x.x.x.x`, `file` doar nume simplu — niciodată o cale, `size` întreg, `sha256` 64 hex).
- `GET /api/update/download` → pachetul numit în `latest.json`, `send_file(conditional=True)`
  (Range merge), antete `X-KBot-Version` / `X-KBot-Sha256`; **500** `PACKAGE_MISSING` dacă
  `latest.json` arată spre un fișier absent.

`UPDATE_DIR` = `getattr(config, "UPDATE_DIR", <PYTHON>/updates)`. `config.py` e doar pe server
(nu se împinge), de aceea implicitul stă în cod. Fără cache: fișierul e mic și se recitește la
fiecare cerere, deci un push se vede **fără restart**.

### 1.2 `push-update.ps1` (nou, la rădăcina soluției)

`.\push-update.ps1 [-Mandatory] [-Notes "..."] [-SkipBuild] [-Force] [-SignThumbprint ...]`

1. Citește `Host/Port/User/RemoteRoot` din `PYTHON\_push\push_settings.json`; găsește
   `sftp.exe`. Totul **înaintea** build-ului, ca lipsa lor să nu se descopere după 3 minute.
2. `publish-release.ps1` (build, semnare, zip, instalator) — sau, cu `-SkipBuild`, cel mai nou
   `artifacts\KBot_Release_*.zip`.
3. Citește `FileVersion` al lui `KBot.App.exe` **din interiorul zip-ului** (pachetul e adevărul,
   nu `.vbproj`-ul de pe disc); refuză un zip fără `KBot.Updater.exe`.
4. `GET /api/update/latest` de pe producție: **refuză** dacă serverul are deja versiunea sau una
   mai nouă (`-Force` trece peste). `minimum` = ce avea serverul, sau versiunea de acum cu
   `-Mandatory`; niciodată peste versiunea publicată.
5. Scrie `latest.json` (UTF-8 fără BOM) și un fișier batch sftp: `-mkdir`, `put zip → .part`,
   `-rm vechi`, `rename`, apoi **la urmă** `latest.json` (tot prin `.part` + `rename`). O singură
   sesiune, **o singură parolă**, tastată la promptul OpenSSH. Un cititor nu vede niciodată o
   versiune al cărei fișier nu e întreg.
6. Verifică prin API că serverul răspunde cu versiunea și suma împinse.

### 1.3 `KBot.Updater` (proiect nou în `src\`, în `KBot.sln`)

WinExe `net8.0-windows`, **zero referințe KBot** (rulează din `%TEMP%` cât timp `C:\KBOT` se
suprascrie). Publicat **single-file** de `publish-release.ps1` (pas nou 4c2) și pus lângă
`KBot.App.exe`; semnat (a cincea confirmare SimplySign). Instalatorul Inno îl ia cu `*`.

- `UpdaterArgs` — linia de comandă (`--zip --target --wait --restart --sha256 --version
  --elevated`), validată; `ToArgumentList` pentru relansarea cu drepturi.
- `UpdateApplier` — inima: scoate folderul de sus al zip-ului (`KBot_Release_<stamp>/`), **sare
  peste `Logs\`**, scrie fiecare fișier ca `*.kbot-new` și îl mută peste cel vechi (un fișier e
  ori cel vechi, ori cel nou), reîncearcă un fișier ocupat (10 × 1 s), gardă zip-slip, **nu
  șterge nimic** din ce nu e în pachet — deci `Asociere\`, `WorkflowResults\`, `Extrase\`,
  `kbot_paths.json` supraviețuiesc. Un DLL scos din produs rămâne orfan, inofensiv.
- `UpdaterForm` — titlu, o linie de stare, `ProgressBar`; firul de lucru: așteaptă PID-ul
  aplicației (max 60 s), verifică SHA-256, probează scrierea în țintă (`IsWritable`) și dacă
  n-are drept **se relansează singur prin UAC** (`--elevated`, o singură dată), aplică, pornește
  aplicația (când e elevat, prin `explorer.exe`, ca aplicația să NU ruleze ca administrator),
  șterge zip-ul. Orice eșec = o casetă cu calea jurnalului + cod 1.
- `UpdaterLog` — `<țintă>\Logs\updater.log` (folderul păstrat), cu cădere pe
  `%TEMP%\KBot\updater.log`.
- `app.manifest` — `asInvoker` (UAC doar unde chiar trebuie).

### 1.4 `KBot.Domain\Update\` — `UpdateInfo`, `UpdateDecision`, `UpdatePolicy`

`UpdatePolicy.Decide(current, latest, minimum)` e **singura** comparație, pură: sub `minimum` →
`Required`; sub `latest` → `Available`; altfel `UpToDate`. `Normalize` umple la patru părți —
`System.Version` pune -1 pe partea lipsă, deci fără ea `"1.0.30" < "1.0.30.0"` și clientul ar fi
fost trimis să instaleze exact versiunea pe care o are. Versiune care nu se parsează = excepție,
nu «la zi».

### 1.5 `KBot.Api` — `IUpdateApi` / `UpdateApi`

`GetLatestAsync` (404 → `Nothing`, nu excepție; 200 cu chei lipsă → `LATEST_INVALID`) și
`DownloadAsync(cale, shaAșteptat, progres, ct)`: **streaming** pe disc (nu se bufferizează 40 MB),
SHA-256 calculat pe aceeași trecere, la nepotrivire fișierul se **șterge** și iese
`ApiException(SHA_MISMATCH)` — un pachet corupt nu ajunge niciodată la updater. Niciun antet
`Authorization` pe niciunul din apeluri.

### 1.6 `KBot.App\Update\` — `AppUpdateService`, `UpdateProgressForm`

- `AppUpdateService.CurrentVersion` = `AssemblyFileVersion` al lui `KBot.App` (normalizat).
- `CheckAsync` → decizie + info. `RunStartupCheck()` (sincron, înainte de bucla de mesaje):
  server de nepătruns = **jurnal + merge la login**, nu blochează; `RunManualCheckAsync(owner)`
  (butoanele): fiecare rezultat se spune, inclusiv «Aveți ultima versiune». Amândouă întorc
  `True` = **procesul trebuie să iasă** (updater pornit, sau actualizare obligatorie refuzată).
- Oferta: opțional = Da/Nu («Nu» = mai târziu); obligatoriu = OK/Anulare (Anulare = aplicația
  se închide; descărcare anulată/eșuată pe obligatoriu = tot se închide, cu o propoziție).
  Textul include notele din `latest.json` și mărimea.
- `UpdateProgressForm` (`KBotThemedForm`, tot în Designer, `KBotProgressBar`, `KBotCaptionBar`,
  `KBotToolTip`): descarcă modal; X-ul din bara de titlu / Esc / «Renunță» = anulare (închiderea
  e refuzată cât timp descarcă, tokenul se anulează, `DownloadAsync` închide dialogul).
- Predarea: `KBot.Updater.exe` se **copiază** din folderul aplicației într-un folder proaspăt
  `%TEMP%\KBot\update\<guid>\` (e el însuși în pachetul care se înlocuiește), se pornește cu
  `--zip --target --wait <pid> --restart <exe> --sha256 --version`, se scrie în
  `mesaje_operator.log` prin `OperatorLog`, și apelantul iese. Lipsa updater-ului lângă aplicație
  (instalare mai veche) se spune pe nume.
- `Program.vb`: DI (`IUpdateApi`, `AppUpdateService`) + în ramura `#Else` (Release):
  `If ...RunStartupCheck() Then Return` **înaintea** lui `RunShellWithLogin`. Debug nu verifică.

### 1.7 Ce s-a lăsat deliberat pentru DUPĂ acceptare (cererea operatorului)

**`KbotForm` și `LoginForm` nu s-au atins.** Butoanele «Caută actualizări» (în amândouă) se
adaugă după ce operatorul acceptă restul; drumul e gata:
`Await provider.GetRequiredService(Of AppUpdateService)().RunManualCheckAsync(Me)` → dacă
întoarce `True`, formularul se închide (`Close()` pe shell; `DialogResult.Cancel` + `Close()` pe
login).

## 2. Fișiere atinse

**Noi:** `PYTHON/routes/update.py`, `PYTHON/tests/test_update_routes.py`, `push-update.ps1`,
`src/KBot.Updater/{KBot.Updater.vbproj, app.manifest, Program.vb, UpdaterArgs.vb,
UpdateApplier.vb, UpdaterLog.vb, UpdaterForm.vb, UpdaterForm.Designer.vb}`,
`src/KBot.Domain/Update/{UpdateInfo.vb, UpdateDecision.vb, UpdatePolicy.vb}`,
`src/KBot.Api/{IUpdateApi.vb, UpdateApi.vb}`,
`src/KBot.App/Update/{AppUpdateService.vb, UpdateProgressForm.vb, UpdateProgressForm.Designer.vb}`,
`tests/KBot.Updater.Tests/{KBot.Updater.Tests.vbproj, UpdaterArgsTests.vb, UpdateApplierTests.vb}`,
`tests/KBot.Domain.Tests/UpdatePolicyTests.vb`, `tests/KBot.Api.Tests/UpdateApiTests.vb`.

**Modificate:** `PYTHON/main.py` (import + `register_blueprint(update_bp)`),
`publish-release.ps1` (pas 4c2 updater single-file, semnare, raport), `src/KBot.App/Program.vb`
(DI + poarta de pornire pe Release), `KBot.sln` (două proiecte), `.gitignore`
(`/PYTHON/updates/`), `CLAUDE.md` (comanda + rândul din tabel), `docs/worklog/KBOT_STATUS.md`.

## 3. Rezultatele testelor

**NICIUN TEST N-A FOST RULAT ȘI NICIUN PROIECT DE TESTE N-A FOST CONSTRUIT** (cerere explicită,
repetată: «NO FUCKING TESTS»). Testele sunt scrise, atât: 16 funcții în Python, una parametrizată cu 10 cazuri (contractul JSON, fiecare
fel în care `latest.json` poate fi stricat, descărcarea cu antete, Range, zip-slip, seam-ul de
config), 8 `UpdatePolicyTests`, 9 `UpdateApiTests` (rute, fără `Authorization`, 404 → Nothing,
SHA greșit = fără fișier), 6 `UpdaterArgsTests`, 14 `UpdateApplierTests` (zip-uri reale în temp:
folderul de sus, `Logs\`, fișierele în plus păstrate, zip-slip, fișier blocat → reîncercare →
eșec numit / reușită când se eliberează).

**Build:** `src\KBot.Updater` **0 erori, 0 avertismente**; `src\KBot.Api` **0/0**;
`src\KBot.App` **0 erori, 0 avertismente** (prima încercare a picat DOAR la copierea în `bin\`,
aplicația rula sub Visual Studio). `dotnet build KBot.sln` NU s-a rulat (soluția conține
proiectele de teste). Python: verificare de sintaxă (`ast.parse`) pe ambele fișiere; suita nu.

## 4. Neverificat / amânat

- **Nimic n-a rulat cap la cap**: niciun push real, nicio descărcare, niciun updater pornit,
  nimic pe ecran (nici `UpdateProgressForm`, nici `UpdaterForm` — nici măcar `DrawToBitmap`).
- **`publish-release.ps1` nu s-a rulat** cu pasul nou 4c2 (single-file al updater-ului): de
  văzut la primul build că `KBot.Updater.exe` iese ~1 fișier și pornește.
- **Serverul**: `/root/AVACONT/updates` nu există încă; primul `push-update.ps1` îl face
  (`-mkdir`). `main.py` cu blueprint-ul nou trebuie **împins și serviciul repornit** (AvacontPush)
  ÎNAINTE de primul push, altfel verificarea din pasul 4/7 al scriptului primește 404 de la nginx
  (nu de la rută) și scriptul o citește ca «nimic publicat».
- **`sftp -b` cu parolă**: OpenSSH cere parola pe consolă; scriptul trebuie rulat dintr-un
  terminal interactiv. Prima conectare cere confirmarea cheii gazdei.
- **`rename` peste un nume existent**: scriptul face `-rm` înainte, deci nu depinde de extensia
  posix-rename a serverului. Pachetele vechi (`KBot_<v-1>.zip`) rămân pe server — curățenia e
  de mână.
- **Drepturile pe `C:\KBOT`**: presupus scriibil de operator (folder creat sub `C:\` moștenește
  Modify pentru Authenticated Users); dacă nu, drumul UAC al updater-ului — nevăzut.
- **Butoanele «Caută actualizări»** în `LoginForm` / `KbotForm`: după acceptare (§1.7).
- Pachetul nu e **delta**: ~40 MB per client per actualizare (hotărârea «toată aplicația»).
- `ApiOptions.TimeoutSeconds = 100` se aplică și descărcării (HttpClient comun); pe o legătură
  foarte lentă 40 MB pot depăși 100 s — de mărit dacă se vede pe teren.
- Memoriile `kbot-versioning-manifest` / `kbot-setup-sfx` vorbesc de `KBOT_API_KEY` care nu mai
  există în client — de corectat la o trecere de consolidare.
