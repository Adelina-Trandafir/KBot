# SLICE-0097-04 — «Ține minte parola»: bifa nu mai depinde de opțiunile avansate

Cererea operatorului, 19.10.2026. (Numărul 0097-04 e ales de mine, urmând după 0097-03; de confirmat.)

## What changed and why

Pe unele calculatoare fereastra de autentificare nu arăta bifa «Ține minte parola până la repornirea
calculatorului», deși în Setări era bifată. Cauza (din `app_settings.json` de pe un astfel de PC):
`"AdvancedOptions": false` — `AppSettings.RememberPasswordInEffect` cerea opțiunile avansate
pornite, iar bifa din Setări era ascunsă fără ele.

Regula nouă (operator): starea bifei rămâne cea stabilită, indiferent de opțiunile avansate;
**doar schimbarea ei** cere opțiunile avansate.

- `AppSettings.RememberPasswordInEffect` = `RememberPasswordOption` (implicit `True`).
- Setări › Autentificare: bifa «Arată în fereastra de autentificare bifa …» e mereu vizibilă,
  dezactivată (`Enabled = False`) fără opțiunile avansate. «Uită parola memorată» rămâne doar cu avansate.
- Ajutor: `contabil/autentificare.md`, `contabil/setari.md` (tag `0097-04`).

## Files touched

- `src/KBot.Common/AppSettings.vb`
- `src/KBot.App/LoginForm.vb` (doar comentariul)
- `src/KBot.App/Setari/SetariAutentificareView.vb`
- `src/KBot.App/HelpContent/contabil/autentificare.md`, `setari.md`

## Test results

`dotnet build src/KBot.App/KBot.App.vbproj`: 0 warnings, 0 errors. Netestat pe ecran.

## Unverified / deferred

- Neprobat pe un PC cu problema. Operatorul raportase că bifa din Setări „nu rămâne bifată”; cu
  avansatele oprite bifa din Setări era ascunsă, deci nu e clar cum se vedea — de reverificat după deploy.
- Cine a oprit `AdvancedOptions` pe acel PC nu se știe din cod (nimic nu o resetează singur).
- `Check-Help.ps1` nerulat.
- Numărul 0097-04 și rândurile de STATUS: de confirmat.
