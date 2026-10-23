# SLICE-0097-05 — Opțiunile avansate doar permit schimbarea, nu blochează aplicarea

Cererea operatorului, 23.10.2026. Numărul 0097-05 a fost confirmat de operator.

## What changed and why

Operatorul a observat că bifa «Reafișează fereastra de autentificare când expiră sesiunea» se
întoarce pe «bifat» (credea că la fiecare update). Cauza din cod: `AppSettings.ReloginPromptInEffect`
întorcea `ReloginPrompt OrElse Not AdvancedOptions`, deci cu opțiunile avansate oprite bifa era
forțată pe «bifat», oricum fusese salvată. Fișierul de setări (`%APPDATA%\AVACONT\KBot`) nu e atins de
update; nu există cod care să reseteze valoarea.

Regula (operator, generalizată): **`AdvancedOptions` doar permite SCHIMBAREA unei setări; nu oprește
niciodată aplicarea uneia salvate.** Aplicată peste tot unde o setare era blocată de avansate:

- `AppSettings.ReloginPromptInEffect` = `ReloginPrompt`.
- `AppSettings.ReloginMinutesInEffect` = `Max(1, ReloginMinutes)` (fără limitarea 10..60 fără avansate).
  În Setări › Autentificare, lista de intervale conține mereu valoarea în vigoare (înainte afișa
  cea mai apropiată din listă, deci un 120 salvat se vedea ca 60).
- `KbotForm.HelpCaptureModeOn` = `AppSettings.Current.HelpCaptureMode` (înainte cerea și avansate).
- Bifa din Setări rămâne dezactivată fără avansate (schimbarea cere avansate), dar afișează valoarea salvată.
- Ajutor: `contabil/setari.md` (tag `0097-05`).

Lăsate neschimbate, pentru că nu sunt setări aplicate, ci unelte/conținut vizibile doar cu avansate:
jurnalele de pe server (`SetariJurnalView`), secțiunea «Istoric (avansat)» (`SetariForexeView`),
partea «Opțiuni avansate» din ajutor (`HelpService`), paginile avansate din `SetariForm`.

## Files touched

- `src/KBot.Common/AppSettings.vb`
- `src/KBot.App/KbotForm.HelpCapture.vb`
- `src/KBot.App/Setari/SetariAutentificareView.vb`
- `src/KBot.App/HelpContent/contabil/setari.md`

## Test results

`dotnet build src/KBot.App/KBot.App.vbproj`: 0 warnings, 0 errors. Netestat, nevăzut pe ecran.

## Unverified / deferred

- Nerulat pe ecran; nu s-a verificat cu un `app_settings.json` real cu `ReloginPrompt: false` și avansate oprite.
- Nu s-a găsit niciun cod care să reseteze bifa la update; ipoteza „reset la update" a operatorului
  rămâne neconfirmată (efectul observat se explică prin forțarea de mai sus). Cine oprește `AdvancedOptions` pe un PC nu se știe din cod.
- `ReloginMinutesMin` / `ReloginMinutesMax` au rămas în `AppSettings` neîntrebuințate.
- `Check-Help.ps1` nerulat.
- Ce rămâne vizibil doar cu avansate (jurnale server, istoric, partea avansată din ajutor) a fost lăsat
  așa; de confirmat cu operatorul dacă vrea și acestea mereu vizibile.
