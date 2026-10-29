# SLICE-0117 - «Actiuni recente»: scrierile pe server ale operatorului, in panoul lateral (13.10.2026)

Reper ulterior: [0115-01](SLICE-0115-acasa-panou-lateral.md#slice-0115-01) limitează afișarea la azi și adaugă golirea după miezul nopții. Starea nelivrată a serverului de mai jos rămâne checkpointul inițial.

Construit si probat in `KBOT_TEST`, adus in `KBOT\src` la 13.10.2026 (doar clientul). Istoria: `KBOT_TEST\MERGE_NOTES.md` (sectiunile 3f, 3h). Felie inregistrata dupa unire, ca lucru deja facut.

## Ce s-a schimbat si de ce
1. **Decizie: actiunile se scriu in tabela `Jurnal` de pe server, cu lista alba de rute.** Un hook `after_request` pe server scrie, pentru fiecare POST / PUT / DELETE reusit, cu sesiune, al carui (metoda, regula) e in lista alba `_LABELS`, un rand `Actiune = 'SCRIERE'`, `Tinta` = ruta, `Detalii` = textul romanesc din lista. Fara schimbari de schema. Textele din `_LABELS` se completeaza pe masura ce apar rute noi care scriu date.
2. **Ruta** `GET /api/actiuni?n=10`: ultimele actiuni ale operatorului pe unitatea curenta.
3. **Clientul**: `RecentAction` (KBot.Common), `IActionsApi` + `ApiClient.Actions.vb` (KBot.Api), `KbotForm.GetRecentActionsAsync` (lista goala cand nu e unitate, cand ruta lipseste sau apelul cade: o lista de comoditate nu ajunge niciodata la operator ca eroare). Un server vechi nu are ruta -> 404 -> lista goala.
4. **Lista in panoul lateral** (sectiunea «Actiuni recente», 0115): textul actiunii si ora (azi «HH:mm», altfel «dd.MM HH:mm»); tooltip cu data si ora intregi. Se reciteste la fiecare reimprospatare a panoului si **live**: `ServerGateHandler.WriteSucceeded` (eveniment Shared, dupa orice scriere reusita) porneste o citire la 2 s dupa ultima scriere dintr-o rafala (un singur GET). Randurile nu se pot apasa.

## Fisiere
NOU: `src/KBot.Common/RecentAction.vb`, `src/KBot.Api/IActionsApi.vb`, `src/KBot.Api/ApiClient.Actions.vb`. Modificate: `src/KBot.Api/ServerGateHandler.vb` (`WriteSucceeded`), `src/KBot.App/KbotForm.Host.vb` (`GetRecentActionsAsync`), `src/KBot.App/AvacontMain.Sidebar.vb` (`LoadActionsAsync`, temporizatorul de 2 s). Serverul: **doar in `KBOT_TEST\PYTHON`** (`routes/actiuni.py` nou + 2 linii in `main.py`). NU e in `KBOT\PYTHON` si nu e pe VPS (operatorul: nu se atinge PYTHON in aceasta unire).

## Teste / verificare
- Build-ul unirii: 0 erori, 0 avertismente (operator). Nimic rulat in aceasta sesiune.
- Probat in `KBOT_TEST` cu serverul local din `KBOT_TEST\PYTHON`.

## Neverificat / amanat
- **Partea de server nelivrata**: pana se aduce `routes/actiuni.py` in `KBOT\PYTHON` si se pune pe VPS, sectiunea «Actiuni recente» ramane goala («Nicio actiune inca») la toti utilizatorii.
- Costul lui `after_request` (o scriere in `Jurnal` la fiecare scriere de date) pe serverul real: nemasurat.
- FileVersion `KBot.Api`, `KBot.Common`, `KBot.App`: nebumpate.
- Ajutorul: 0000-71 (sectiunea «Actiuni recente» a panoului lateral).
