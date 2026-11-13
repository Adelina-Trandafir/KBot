# SLICE-TRAY-02 — tray-ul ca ediție separată, pentru cei care doar semnează

Cererea operatorului, 10.11.2026. Planul: `docs/PLANS/PLAN_TRAY_SEPARATE_PACKAGE.md`.

Deciziile operatorului (10.11.2026):
- Tray-ul are **pachet propriu și versiune proprie**, nu mai e în zipul K-BOT. Contabilii (programul complet) nu au nevoie de el.
- **Mod «doar tray»** la publicare: se construiește, se semnează și se publică DOAR tray-ul; celelalte exe (KBot.App.exe, actualizatorul, migratoarele, driverul Playwright) nu sunt în pachet, ca semnatarii să nu le poată deschide.
- Cei care doar semnează primesc o variantă simplificată **doar pentru listă, vizualizare și semnare** de documente. Roluri: **Director, Administrator, Secretar**.
- Varianta **B**: tray-ul folosește `KBot.App.dll` (ferestrele de semnare rămân în `KBot.App`, nu se mută); pachetul e mare, dar fără `KBot.App.exe` nu e nimic de deschis cu dublu-clic. Mărimea pachetului nu contează.
- Ruta nouă din server: pe canalul propriu (`tray.json`), nu în `latest.json`. Publicarea scriptului în KBotAdmin: rând nou în «Scripturi».
- Tray-ul deja instalat din zip îl scoate operatorul de mână; nicio versiune-punte.

## Ce s-a schimbat și de ce

**Pachetul K-BOT** — `publish-release.ps1`: scos pasul 4c3 (construirea și copierea `KBot.Tray.exe`) și linia care îl semna; rămân 5 confirmări SimplySign pe build. `publish-debug.ps1`: scos și de acolo. `KBot.App` (`TrayRequestHub.StartTray`) nu pornește nimic dacă `KBot.Tray.exe` lipsește (era deja așa), deci aplicația completă nu mai are tray.

**Publicare nouă — `push-tray.ps1`** (la rădăcină): construiește doar `src\KBot.Tray` (folder framework-dependent, `Release`, `win-x64`), întreabă doar «semnez?» și «ridic versiunea?» (FileVersion din `KBot.Tray.vbproj`), lasă în pachet un singur `.exe` (`KBot.Tray.exe`; șterge `KBot.App.exe`, `.playwright` cu `node.exe` și orice alt `.exe`, și oprește scriptul dacă rămâne altceva decât unul), semnează DOAR `KBot.Tray.exe` (o confirmare; dacă s-a răspuns «da» și semnarea nu se poate face, scriptul se oprește, nu publică nesemnat pe tăcute), face zipul cu bare oblice, îl urcă prin sftp ca `KBot_Tray_<versiune>.zip` și scrie ULTIMUL `tray.json`; apoi verifică prin API versiunea și sha256. Nu atinge `latest.json`.

**Program de instalare pentru semnatarii noi (alegerea operatorului, 10.11.2026)** — `tools/KBotTrayInstaller/KBotTray.iss` (Inno Setup, salvat cu BOM): instalare **pe utilizator, fără administrator** în `%LOCALAPPDATA%\AVACONT\KBotSemnare` (folder în care tray-ul își poate rescrie singur fișierele la actualizare), AppId propriu (nu atinge K-BOT), scurtătură în meniul Start, pornește tray-ul la sfârșit; verifică «.NET Desktop Runtime 8» (dacă lipsește: mesaj, sau îl instalează din `tools/KBotInstaller/prereq/` dacă fișierul e acolo, singurul pas cu drepturi de administrator); închide un tray pornit înainte de scriere; dezinstalarea șterge folderul, cheia Run `KBotTray` și starea `HKCU\Software\AVACONT\KBot\Tray`. `push-tray.ps1 -Installer Yes|No` (implicit Yes) îl construiește în `artifacts\KBot_Tray_Setup_<versiune>.exe`, din același folder care devine zipul; cu semnare, Inno semnează Setup + dezinstalatorul (încă 2 confirmări). **Nu se urcă pe server** — se dă semnatarilor de mână. «Scripturi» are comutatorul «Fără program de instalare». Tray-urile deja instalate se actualizează singure din zip; instalatorul e doar pentru prima instalare.

**Server — `PYTHON/routes/update.py`:** `read_tray()` + `GET /api/update/tray/latest` (blocul din `tray.json`, 404 dacă n-a fost publicat) + `GET /api/update/tray` (pachetul, Range). Canal separat, ca `push-update.ps1` (care rescrie `latest.json` întreg) să nu poată șterge blocul tray. **Nepublicat pe server** — fișierul îl pune operatorul de mână.

**KBotAdmin — «Scripturi»:** rând nou `push-tray.ps1` în grupa Push (comutatoare: Force, cele patru părți de versiune, Semnează / Fără semnare; caseta de text → `-Notes`, păstrat în `tray.json`).

**Tray (`src/KBot.Tray`):**
- `KBot.Tray.vbproj` referă `KBot.App` și `KBot.Controls`.
- Nou `SignerWindows.vb`: deschide fereastra rolului (Secretar/Administrator → `DeSemnatRestransForm`; Director → `DirectorForm`, cu documentul ales în `IdrevInitial`), pe sesiunea proprie a tray-ului; fereastra e modală, după închidere lista se citește imediat. Autentificarea la expirarea sesiunii sau la un document de altă unitate: `LoginForm` fără serviciul de actualizări K-BOT.
- Nou `TrayUpdater.vb`: tray-ul își aduce singur versiunea nouă (cel mult din 6 în 6 ore, la verificarea documentelor; la eșec reîncearcă după 30 min): descarcă `GET /api/update/tray`, verifică sha256 și versiunea din interior, despachetează în `_update\`, **redenumește** fișierele vechi în `*.old` (Windows lasă redenumirea fișierelor încărcate), pune pe cele noi, anulează totul la prima eroare, pornește noul tray (care așteaptă să se termine cel vechi, `--wait-pid`) și se oprește. `*.old` se șterg la pornirea următoare. Un tray pornit din `\bin\Debug\` / `\bin\Release\` nu se actualizează niciodată.
- `Program.vb`: fără copia din `%LOCALAPPDATA%` (a dispărut `ShadowCopy.vb`) — rulează din folderul lui; `KBotMessageBox.Install()`. `KBotLauncher.vb` a dispărut (nu mai pornește `KBot.App.exe`). `Autostart` scrie în cheia Run exe-ul folderului în care rulează.
- `TrayChecker`: expune `Session`, `Api`, `Auth` ferestrelor; rolurile ediției (Director, Administrator, Secretar): mută sesiunea pe unitatea Director dacă există, altfel rămâne pe o unitate Administrator/Secretar, altfel utilizatorul nu are listă aici (6 ore). Contabil/Împuternicit nu mai primesc tray.
- `TrayContext`: meniul «Documente de semnat» (în loc de «Deschide K-BOT»), clic pe rând/listă → `SignerWindows.Open`; nicio verificare cât timp o fereastră e deschisă; textele de ajutor de pe iconiță nu mai trimit la K-BOT.
- `publish-tray.ps1` (doar local): folder în `src\KBot.Tray\bin\<Config>\net8.0-windows\local` (tray-ul îl socotește build de dezvoltator: fără cheie Run, fără auto-actualizare).

**Jurnalul tray-ului (10.11.2026, după prima pornire fără documente de semnat):** `TrayLog.vb` scrie în `<folderul tray-ului>\Logs\tray.log` (lângă `harness_errors.log`, cu rotația obișnuită) câte o linie pe pas: pornirea (versiune, folder, dacă e build de dezvoltare), de unde vin datele de autentificare (perechea din K-BOT / cea scrisă în tray / niciuna), unitățile contului, unitatea și rolul cu care s-a intrat, mutarea pe altă unitate, câte documente a citit (lista fluxului, lista veche de director, unități necitite), rezultatul fiecărei verificări, deschiderea și închiderea ferestrei de semnare, verificarea de actualizare. Fără parole, jetoane sau adrese de e-mail. În plus, **doar într-un build de dezvoltare** (folder `\bin\Debug|Release\`), meniul iconiței are «Probă: deschide fereastra de semnare», care deschide fereastra rolului chiar dacă nu e nimic de semnat (încearcă fereastra, Adobe-ul și autentificarea fără un document real).

**Eroare la parolă greșită (10.11.2026, cererea operatorului):** `TrayLoginForm` verifică acum perechea la server (`CheckAccessAsync`) ÎNAINTE să se închidă: la parolă greșită arată mesajul serverului în fereastră (`lblError`), golește parola și rămâne deschisă; la «cont fără nicio unitate» și «parola trebuie schimbată» arată mesaje proprii; la o problemă de rețea, «Serverul nu a putut fi contactat…». Cât timp întreabă serverul, câmpurile și butonul sunt înghețate («Se verifică...»), «Renunță» rămâne activ. Înainte, fereastra se închidea imediat și eroarea apărea abia după verificarea din fundal. Jurnalul `tray.log` are liniile «signin window». Compilat curat, nevăzut pe ecran. Ajutor: textul tray-ului din `director/lista.md` e oricum de rescris (vezi «Ajutor de actualizat»).

**`KBot.App` (mici):** `LoginForm` acceptă fără serviciul de actualizări (`Nothing` = doar autentificare); `DirectorForm.IdrevInitial` și `SelecteazaRevizia` au devenit `Public`.

## Fișiere atinse

`push-tray.ps1` (nou), `publish-release.ps1`, `publish-debug.ps1`, `publish-tray.ps1`, `PYTHON/routes/update.py`, `KBotAdmin/Scripts/ScriptCatalog.vb`, `tools/KBotTrayInstaller/KBotTray.iss` (nou), `src/KBot.Tray/{KBot.Tray.vbproj,Program,TrayContext,TrayChecker,Autostart}.vb`, `src/KBot.Tray/{SignerWindows,TrayUpdater}.vb` (noi), `src/KBot.Tray/{ShadowCopy,KBotLauncher}.vb` (șterse), `src/KBot.App/Auth/LoginForm.vb`, `src/KBot.App/Director/DirectorForm.vb`, `docs/PLANS/PLAN_TRAY_SEPARATE_PACKAGE.md`.

## Rezultate de verificare

- `dotnet build src\KBot.Tray` (Debug): 0 erori, 0 avertismente. `dotnet build KBotAdmin`: 0 erori, 0 avertismente. `update.py`: `py_compile` curat. `push-tray.ps1`, `publish-release.ps1`, `publish-tray.ps1`, `publish-debug.ps1`: se parsează fără erori.
- Publish de probă al tray-ului (Release, folder): 567 fișiere, 113 MB; `.exe` în el: `KBot.Tray.exe`, `KBot.App.exe`, `.playwright\...\node.exe`, `PrintDeps.exe`. După ce se scot cele pe care `push-tray.ps1` le scoate (`.playwright` și orice alt `.exe`): 227 fișiere, 25,4 MB, un singur `.exe` (`KBot.Tray.exe`). Probă ștearsă.
- **Instalatorul compilat** (ISCC 6, fără semnare, din acel folder): `KBot_Tray_Setup_probe.exe`, 11,9 MB, fără erori. **Neinstalat** pe acest PC (ar porni tray-ul și ar scrie cheia Run). Probă ștearsă.
- **Redenumirea unui fișier încărcat** (baza auto-actualizării), probă pe un `.exe` în execuție, în folderul de lucru: redenumirea în `*.old` merge, numele vechi se poate umple cu fișierul nou, iar ștergerea imaginii în execuție e refuzată — deci `*.old` se curăță la pornirea următoare, cum e scris. (Proba nu a fost făcută pe tray-ul însuși, doar pe același mecanism Windows.)
- **Ce citește fereastra de semnare din pornirea K-BOT (`Program.vb`), citit în cod:** `AppScreen.Reference` cade singur pe ecranul principal dacă nu e înregistrat nimic; `AppSettings.Current` și `TempPdfStore` se inițializează singure la prima folosire; `TempPdfStore.Wipe()` (curățenia la pornire) NU e apelat în tray, ca să nu șteargă fișierele unui K-BOT pornit în același timp (rămân PDF-uri temporare până la o curățenie făcută de K-BOT). Nimic din ele nu e obligatoriu înainte de fereastră.

## Nevăzut / nerulat / de hotărât

- **Tray-ul nu a fost pornit niciodată și nicio fereastră nu a fost văzută.** Ferestrele de semnare DESCHISE din tray (`DirectorForm` / `DdfView` / `DeSemnatRestransForm`, cu Adobe) cer o autentificare reală cu rol de semnatar: de încercat de operator cu `publish-tray.ps1` (rând în «Scripturi»). Scalarea DPI aleasă de utilizator în K-BOT nu se aplică în tray (PerMonitorV2).
- Auto-actualizarea nu s-a încercat pe tray (repornirea cu `--wait-pid`, despachetarea, anularea la eroare). `push-tray.ps1` nu s-a rulat (semnarea cere jetonul SimplySign, sftp-ul cere parola serverului și scrie pe server). Ruta din `update.py` nu e pe server.
- Instalatorul nu a fost rulat (vezi mai sus), nici semnat.
- Ajutorul: `HelpContent/director/lista.md` (secțiunea «Mesajul de lângă ceas», 0000-142) este ÎNVECHIT (vorbește de K-BOT pornit din tray și de rolul de Director); nu s-a rescris. Înscris în «Ajutor de actualizat» (0000-0009).
- Versiunile: `KBot.Tray` (1.0.0.0) se ridică la `push-tray.ps1`; `KBot.App` și `KBotAdmin` au fost atinse și își ridică FileVersion la publicarea lor.
- Nu s-au făcut teste automate (regula proiectului: doar la cerere).
