# SLICE-AV08-01 — «Protecție operații» (Setări › Aplicație › AVACONT)

Plan: [`PLAN_AV08_OPERATII_CONTABILE.md`](../PLANS/PLAN_AV08_OPERATII_CONTABILE.md). Decizia Adelinei (24.11.2026): opțiune în Setări, tab nou «AVACONT», per utilizator, implementată ca în Access.

## What changed and why

- Access (`Note.txt`): `Scheme._MNC.C5` («Protectie operatii») este forțat pe activ la fiecare deschidere a formularului `Note`; cu el, lista de conturi (`CTD.Tag`) are doar conturile potrivite clasificației, iar pentru `Fel = I` este oprit. `mPick_ClsfClick` refăcea analiticul conturilor la schimbarea clasificației și avertiza (10310) când contul nu exista.
- `AppSettings.AvacontProtectieOperatii` (implicit **True**, ca în Access), în `app_settings.json` (per utilizator Windows).
- Setări › Aplicație: fila nouă **«AVACONT»** (`navPagini` Key `avacont`), titlul «Operații contabile» și bifa «Protecție operații» cu tooltip; codul în partialul `SetariAplicatieView.Avacont.vb` (intrare `Compile Update` în `.vbproj`).
- Editorul (`NotaManualaForm.Protectie.vb`): cu bifa pusă și o clasificație aleasă, combo-ul contului oferă doar conturile cu `ClsfF`/`ClsfE` goale sau egale cu ale clasificației; la schimbarea clasificației, liniile cu un cont de clasificație trec pe contul unic activ cu același cont de bază, analitic și aceleași părți de clasificație (mereu, ca `mPick_ClsfClick`); lipsă / ambiguu → linia marcată și mesaj «Nu există cont configurat pentru clasificația aleasă: …». Pe ultima notă de închidere protecția e oprită.
- Server: cererea poartă `Protectie`; `NoteC_editor.verifica_protectia` refuză (reason `PROTECTIE`, cu `parte`/`linie`) un cont nepotrivit, pentru liniile cu cont schimbat sau pentru toate când s-a schimbat clasificația; regula unică `NoteC.conturi_nepotrivite` (același test ca `cont_pentru`). Contextul editorului trimite `Cont`, `Analitic`, `ClsfF`, `ClsfE`, `Activ` la conturi și `ClsfF`, `ClsfE` la clasificații.
- Fișa `docs/access-map/avacont/setari-implicite-si-bife.md` spunea (23.10.2026) că «Protectie operatii» nu devine setare: decizia s-a schimbat la 24.11.2026.

## Files touched

`src/KBot.Common/AppSettings.vb`; `src/KBot.App/Setari/Views/SetariAplicatieView.Designer.vb`, `SetariAplicatieView.vb`, `SetariAplicatieView.Avacont.vb` (nou); `src/KBot.App/KBot.App.vbproj`; `src/KBot.App/Avacont/IntroducereDate/Operatii/NotaManualaForm.Protectie.vb` (nou); `src/KBot.Domain/Avacont/IntroducereDate/Operatii/NotaManualaInfo.vb`; `PYTHON/routes/avacont/introducere_date/operatii/NoteC`, `NoteC_editor.py`, `note_manuale.py`.

## Test results

`dotnet build src/KBot.App/KBot.App.vbproj`: 0 erori, 0 avertismente. Python: `py_compile` pe fișierele schimbate, fără erori. Nimic rulat, netestat, nevăzut pe ecran (regula: teste doar la cerere).

## Unverified / deferred

- Neverificat pe date: că `TZ_PlanCont.ClsfF/ClsfE` sunt scrise la fel ca `Clasificatii.ClsfF/ClsfE` (același test îl folosește deja `cont_pentru`, deci PRESUPUS corect).
- Fila «AVACONT» nu are încă celelalte bife din etapa 1 (TextPart, NDes, ordonarea listei): neconfirmate.
- Serverul nou nu este pe server (îl pune Adelina).
