# SLICE-TRAY-01 — aplicatia din zona de notificari (documente de semnat)

Cererea operatorului, 30.10.2026 (familia de felii TRAY, numerotare TRAY-01, TRAY-02...): o mini aplicatie in systray care, la un termen, verifica daca utilizatorul curent
(Director, Gestionar, Contabil...) are documente de semnat; daca da, un mesaj deasupra ceasului cu lista; clic pe un rand = K-BOT direct pe
«Documente de semnat» cu documentul incarcat (pentru contabil: fereastra principala); clic pe mesaj in afara randurilor = lista fara selectie;
porneste cu Windows si ramane pana se inchide; se inchide cu clic dreapta.

Decizii luate de operator (AskUserQuestion, 30.10.2026):
- **Documente:** acum doar Director (DDF cu A+B semnate, fara Ordonator, ruta existenta `GET /api/forexe/ddf/director/de-semnat`). Gestionar / Contabil: structura e pregatita, fara lista pana spune operatorul ce semneaza ei.
- **Autentificare:** perechea din «Tine minte parola» (cheia volatila `HKCU\Software\AVACONT\KBot\Session`, DPAPI) — nimic nou pe server.
- **Termen:** la fiecare 10 minute.

## Ce s-a schimbat si de ce

**Proiect nou `src/KBot.Tray` (WinExe, un singur fisier la publicare, referinte Api/Common/Domain/Theming):**
- `Program.vb` — o singura instanta (mutex); exe-ul din `C:\KBOT` este doar lansatorul: se copiaza in `%LOCALAPPDATA%\AVACONT\KBot\Tray` si porneste copia (`ShadowCopy.vb`), ca actualizatorul sa nu gaseasca `KBot.Tray.exe` blocat (actualizatorul suprascrie tot `C:\KBOT`).
- `TrayChecker.vb` — citeste perechea, `CheckAccess` → `Login` pe ultima unitate → `my-units`; rolul = rolul din unitatile utilizatorului; pentru Director muta sesiunea pe o unitate unde rolul e «Director» (`switch-unit`) si citeste lista. Sesiune proprie intre verificari, relogin o data la 401; un utilizator fara lista nu mai e intrebat 6 ore.
- `TrayContext.vb` — iconita, meniul (clic dreapta: Deschide K-BOT / Verifica acum / Porneste cu Windows / Inchide), cronometrul (10 min; 1 min cat timp nu are cu ce verifica sau ultimul raspuns a esuat; prima verificare dupa 20 s), iesirea face logout.
- `DocumentsToast.vb` + `.Designer.vb` — mesajul de deasupra ceasului (`KBotThemedForm`, culori din paleta, nu ia focusul, dispare singur la 30 s / Esc / clic in alta parte dupa ce a fost folosit).
- `Autostart.vb` — cheia `Run` (la prima pornire; daca utilizatorul o opreste din meniu, ramane oprita). `KBotLauncher.vb` — scrie cererea si porneste `KBot.App.exe`.

**`KBot.Common/TrayRequest.vb`** — mesajul tray → K-BOT: `%APPDATA%\AVACONT\KBot\tray_request.json` (`sign` + unitate + `Idrev`, sau `main`), scris atomic.

**`KBot.App`:**
- `TrayRequestHub.vb` — citeste cererea la aparitia ferestrei principale si urmareste fisierul cu un `FileSystemWatcher` (K-BOT deja pornit: al doilea proces il aduce in fata prin `SingleInstance` si se inchide, cererea ajunge prin fisier); porneste tray-ul dupa login (`StartTray`, daca `KBot.Tray.exe` exista langa aplicatie).
- `Program.vb` — `LoginForm.PreferredDc` = unitatea documentului cerut; conectarea hub-ului.
- `KbotForm.Director.vb` — `DeschideDirectorDeSemnat(idrev)` + `RaspundeCereriiDinTray` (schimba unitatea daca documentul e pe alta unitate de Director); `KbotForm.Units.vb` — corpul schimbarii de unitate scos in `SchimbaUnitateaAsync` (acelasi cod, folosit si de selector).
- `Director/DirectorForm.vb` — `IdrevInitial` / `SelecteazaRevizia`.

**Actualizare (cerere 30.10.2026):** `AppUpdateService` cheama `TrayRequestHub.StopTray()` inainte de a porni actualizatorul: seteaza evenimentul `Local\KBOT_Tray_Exit_5C1E7A90` (`TrayRequest.ExitEventName`), pe care tray-ul il asteapta (`RegisterWaitForSingleObject`, nu cronometru), asteapta pana la 5 s iesirea procesului si, daca nu iese, il inchide. Noul tray porneste odata cu K-BOT-ul repornit de actualizator. Fara repornirea PC-ului.

**Parola proprie a tray-ului (cerere 30.10.2026, varianta «doar in memorie»):** `TrayLoginForm` (fereastra de autentificare, Designer + cod) se deschide o data la prima verificare fara parola, la clic pe iconita si din meniu («Autentificare...»); perechea se tine in proces cu `SessionCredentials.Remember` (DPAPI, nimic pe disc). `TrayChecker` ia intai perechea din K-BOT, apoi pe cea proprie; o pereche refuzata de server (400/401/403) se tine minte doar ca amprenta SHA-256 si nu mai e incercata pana apare alta (`TrayStatus.Rejected`, fereastra revine cu mesajul serverului).

**Debug:** `Autostart.IsDevelopmentFolder` — un tray pornit dintr-un folder `...\bin\Debug\` sau `...\bin\Release\` nu scrie niciodata cheia `Run` (elementul de meniu «Porneste cu Windows» e dezactivat); un Debug publicat pe alt PC (`publish-debug.ps1`) nu sta sub `bin`, deci isi pune pornirea.

**Scripturi:** `publish-release.ps1` (pasul 4c3; `KBot.Tray.exe` este si SEMNAT, a sasea confirmare SimplySign) si `publish-debug.ps1` publica `KBot.Tray.exe` single-file langa aplicatie. Ajutor: `HelpContent/director/lista.md`, sectiunea «Mesajul de langa ceas» (0000-142).

## Fisiere atinse

`src/KBot.Tray/*` (nou), `src/KBot.Common/TrayRequest.vb` (nou), `src/KBot.App/{TrayRequestHub,Program,KbotForm.Director,KbotForm.Units}.vb`, `src/KBot.App/Director/DirectorForm.vb`, `src/KBot.App/HelpContent/director/lista.md`, `KBot.sln`, `publish-release.ps1`, `publish-debug.ps1`.

## Rezultate

`dotnet build` pe `KBot.Tray` si `KBot.App`: curat, 0 avertismente. `dotnet publish` single-file al tray-ului: un `KBot.Tray.exe` de ~1,7 MB. Oprirea tray-ului la actualizare si semnarea: build curat, NEPROBATE (actualizare reala nerulata, semnarea nerulata). Niciun test rulat (regula casei), nimic pornit pe ecran.

## Nerulat / nevazut / de stiut

- **Nimic vazut pe ecran:** mesajul de deasupra ceasului (pozitia pe monitoare cu bara in alta parte decat jos, scalarea DPI, culorile), iconita, meniul, primul pas al `Run`.
- **Doar Director are lista.** Gestionar / Contabil: tray-ul nu arata nimic pentru ei; clic pe iconita le deschide fereastra principala. Ce documente semneaza ei si ce ruta le da lista — de stabilit.
- **Parola:** fara «Tine minte parola» bifat in K-BOT, tray-ul nu are cu ce verifica (cheia e volatila: dupa repornirea Windows trebuie o conectare in K-BOT). Verificarile deschid sesiuni de server proprii (login pe ultima unitate, `switch-unit` daca rolul de Director e pe alta); `revoke` nu se face la login, deci sesiunea K-BOT nu e atinsa — verificat in `routes/auth/auth.py`, nu pe server.
- **Interval:** 10 minute, constanta in `TrayContext.vb` (nu exista fisier de setari).
- **Mesajul apare la FIECARE verificare cu documente** (cum s-a cerut); nu exista «amintire doar la schimbare».
- **Cerere pierduta:** daca K-BOT ruleaza dar e inca la fereastra de login, cererea ramane in fisier si e luata la aparitia ferestrei principale (maxim 10 minute vechime). Daca lista e deja deschisa pe alta unitate, cererea pentru alta unitate se ignora.
- K-BOT porneste tray-ul si cand operatorul l-a inchis din meniu (la urmatoarea pornire a K-BOT); autostart-ul oprit din meniu ramane oprit.
