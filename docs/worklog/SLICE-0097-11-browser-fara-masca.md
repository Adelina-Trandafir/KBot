# SLICE-0097-11 — Opțiune avansată: browserul fără mască în timpul lucrului robotului

Cererea operatorului (20.11.2026): o opțiune (avansată) care dezactivează masca pusă peste browser, ca să vadă exact ce face robotul (în special pagina de eroare a FOREXE de la citirea listei, vezi 0097-10).

## What changed and why

Cât lucrează robotul, scriptul din pagină (`ForexeWatch.js`, `setSuspended`) pune clasa `kbot-busy` pe `<html>`: fundal plin în culoarea temei + cardul «K-BOT lucrează în FOREXE: …». Acum, cu setarea nouă, clasa nu se mai pune.

- `AppSettings.ForexeNoBusyVeil` (implicit `False`; în `ToDto` / `FromDto` / `AppSettingsDto`).
- `ForexeWatchConfig.JsonFromSettings`: cheia `noVeil` = `ForexeNoBusyVeil AndAlso AdvancedOptions` — dacă opțiunile avansate se opresc, masca revine, deși bifa e ascunsă.
- `ForexeWatch.js`: `configFrom` citește `noVeil`; `syncBusy` pune clasa doar când `suspended` și nu `noVeil`; `configure` cheamă `syncBusy()` ca schimbarea să se vadă imediat, chiar în mijlocul unei lucrări.
- Setări › FOREXE › Browserul: bifa nouă `chkFaraMasca` («Nu acoperi browserul în timpul lucrului robotului»), în Designer, vizibilă doar cu opțiunile avansate pornite (`ArataSectiuneaIstoric`). Salvare imediată + `AplicaSetarilePaginiiAsync`, ca la bifa rezervărilor.
- **Neschimbat, intenționat:** blocarea mouse-ului și a tastaturii pe fereastra browserului (`LockDockedInput`), meniul K-BOT ascuns în timpul lucrării și amuțirea urmăririi: un clic al operatorului în mijlocul clicurilor robotului l-ar deruta.
- Ajutor: `avansat/index.md`, secțiunea «Browserul fără mască» (etichetă 0097-11).
- Versiuni: KBot.Forexe 1.0.22.0 → 1.0.23.0, KBot.Common 1.5.15.0 → 1.5.16.0 (KBot.App: la `push-update.ps1`).

## Files touched

`src/KBot.Common/AppSettings.vb`, `KBot.Common.vbproj`; `src/KBot.Forexe/Models/ForexeWatchConfig.vb`, `Services/JavaScripts/ForexeWatch.js`, `KBot.Forexe.vbproj`; `src/KBot.App/Setari/Views/SetariForexeView.vb`, `SetariForexeView.Designer.vb`; `src/KBot.App/HelpContent/avansat/index.md`.

## Test results

`dotnet build src/KBot.App/KBot.App.vbproj -c Debug`: 0 avertismente, 0 erori; `node --check` pe `ForexeWatch.js`: fără erori. Nimic rulat, nimic văzut pe ecran, niciun test (regula proiectului).

## Unverified / deferred

- Nevăzut pe ecran: poziția bifei în secțiunea «Browserul» și dimensiunile din Designer (puse de mână, tabelul e AutoSize).
- Nu s-a verificat în pagină că fără clasa `kbot-busy` cardul chiar dispare la o lucrare pornită cu bifa deja pusă (logica e în `syncBusy`, chemată și la fiecare resync).
- Fereastra browserului se vede doar andocată (butonul de arătare a browserului din consolă); nu s-a schimbat.
- Nu s-a făcut nicio operațiune git.
