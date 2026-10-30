# K-BOT — STATUS, slice family TRAY (the notification-area application)

Operator request 30.10.2026. TRAY is a family inside K-BOT, like KE and 00EF. Numbering TRAY-01, TRAY-02, ... **Next free: TRAY-02.**

## Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| TRAY-01 | Aplicatia din zona de notificari: documente de semnat deasupra ceasului, clic pe rand = K-BOT pe document; porneste cu Windows, se inchide cu clic dreapta | GATA pe cod (build curat, publish single-file verificat); **nimic vazut pe ecran, nimic rulat**. Ajutor 0000-142 (text) | `SLICE-TRAY-01-documente-de-semnat.md` | Doar Director are lista. |

## Current focus

- **TRAY-01 (30.10.2026).** `src/KBot.Tray` (nou) + `KBot.Common/TrayRequest` + `KBot.App/TrayRequestHub`. Decizii ale operatorului: acum doar Director; autentificare prin «Tine minte parola»; verificare la 10 minute.

## Open threads

- Gestionar / Contabil: ce documente de semnat au si ce ruta de server le da lista (azi nimic; clic pe iconita le deschide fereastra principala).
- Mesajul de deasupra ceasului, iconita, meniul si cheia Run nu au fost vazute pe ecran (pozitie pe monitoare cu bara nu jos, DPI, culori).
- Mesajul apare la fiecare verificare cu documente (cum s-a cerut); de hotarat daca se vrea reamintire doar la schimbare.
- La actualizare K-BOT opreste tray-ul prin eveniment (apoi il inchide dupa 5 s) si noul tray porneste cu K-BOT repornit; `KBot.Tray.exe` e semnat in `publish-release.ps1`. Ambele NEPROBATE (nicio actualizare reala, nicio semnare rulata).
- Parola proprie a tray-ului (doar in memorie): fereastra de autentificare, amprenta perechii refuzate si dezactivarea pornirii cu Windows din foldere `bin\Debug|Release` sunt scrise si construite curat, NEVAZUTE si neincercate. La fiecare pornire a Windows fara «Tine minte parola» tray-ul cere din nou parola.
