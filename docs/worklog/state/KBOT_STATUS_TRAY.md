# K-BOT — STATUS, slice family TRAY (the notification-area application)

Operator request 30.10.2026. TRAY is a family inside K-BOT, like KE and 00EF. Numbering TRAY-01, TRAY-02, ... **Next free: TRAY-03.**

## Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| TRAY-01 | Aplicatia din zona de notificari: documente de semnat deasupra ceasului, clic pe rand = K-BOT pe document; porneste cu Windows, se inchide cu clic dreapta | GATA pe cod (build curat, publish single-file verificat); **nimic vazut pe ecran, nimic rulat**. Ajutor 0000-142 (text) | `SLICE-TRAY-01-documente-de-semnat.md` | Doar Director are lista. |
| TRAY-02 | **Ediția pentru semnatari** (10.11.2026): tray-ul = program separat pentru Director / Administrator / Secretar (doar listă, vizualizare, semnare); pachet + versiune proprii (`push-tray.ps1`: construiește, semnează DOAR `KBot.Tray.exe`, nimic altceva ca .exe), canal propriu pe server (`tray.json`, `/api/update/tray[/latest]`), rând în KBotAdmin «Scripturi»; scos din pachetul K-BOT (5 confirmări SimplySign); deschide singur `DirectorForm` / `DeSemnatRestransForm` din `KBot.App.dll` (varianta B); se actualizează singur (redenumire în `*.old`); **program de instalare pe utilizator** (`tools/KBotTrayInstaller/KBotTray.iss`, construit de `push-tray.ps1`, dat de mână semnatarilor) | GATA pe cod (build curat KBot.Tray + KBotAdmin, `py_compile` curat, publish de probă verificat, instalatorul compilat 11,9 MB, redenumirea fișierelor încărcate probată); **NIMIC rulat, nimic văzut pe ecran; ruta din `update.py` NEpublicată pe server** | `SLICE-TRAY-02-editia-pentru-semnatari.md` | Plan: `docs/PLANS/PLAN_TRAY_SEPARATE_PACKAGE.md`. |

## Current focus

- **TRAY-01 (30.10.2026).** `src/KBot.Tray` (nou) + `KBot.Common/TrayRequest` + `KBot.App/TrayRequestHub`. Decizii ale operatorului: acum doar Director; autentificare prin «Tine minte parola»; verificare la 10 minute.

## Open threads

- **TRAY-02 nerulat:** ferestrele de semnare deschise DIN tray (Adobe, `AppScreen`, `TempPdfStore.Wipe`, scalarea DPI nu sunt pornite acolo), auto-actualizarea (redenumirea fișierelor încărcate, `--wait-pid`), `push-tray.ps1` (semnare, sftp) și ruta din `update.py` (de publicat de operator pe server) — nimic încercat. Instalatorul nu a fost rulat și nici semnat; el nu se urcă pe server, se dă de mână.

- Gestionar / Contabil: ce documente de semnat au si ce ruta de server le da lista (azi nimic; clic pe iconita le deschide fereastra principala).
- Mesajul de deasupra ceasului, iconita, meniul si cheia Run nu au fost vazute pe ecran (pozitie pe monitoare cu bara nu jos, DPI, culori).
- Mesajul apare la fiecare verificare cu documente (cum s-a cerut); de hotarat daca se vrea reamintire doar la schimbare.
- La actualizare K-BOT opreste tray-ul prin eveniment (apoi il inchide dupa 5 s) si noul tray porneste cu K-BOT repornit; `KBot.Tray.exe` e semnat in `publish-release.ps1`. Ambele NEPROBATE (nicio actualizare reala, nicio semnare rulata).
- Parola proprie a tray-ului (doar in memorie): fereastra de autentificare, amprenta perechii refuzate si dezactivarea pornirii cu Windows din foldere `bin\Debug|Release` sunt scrise si construite curat, NEVAZUTE si neincercate. La fiecare pornire a Windows fara «Tine minte parola» tray-ul cere din nou parola.
