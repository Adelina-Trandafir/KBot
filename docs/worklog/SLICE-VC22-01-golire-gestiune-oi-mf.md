# SLICE-VC22-01 — Golire gestiune OI/MF: bonuri PTC + consumuri + NIR (KBotAdmin › Comenzi › VERCON)

18.11.2026 — Cerere: o comandă doar pentru gestiunile OI/MF, care pentru o gestiune anume șterge complet bonurile PTC cu consumurile aferente (toate / pe ani) și NIR-urile (intrări).

## Decizii (confirmate de Adelina în conversație)

- «Bonuri PTC» = predare BP, transfer BT, casare BS (`VC_OI_Predare`, `VC_OI_Transfer`, `VC_OI_Casare`) **cu registrul OI** (`VC_OI_Produse`).
- Consumuri = **toate** bonurile de consum ale gestiunii din anii aleși (BC și BR, cu `VC_Iesiri`, `VC_IesiriRetur`), nu doar cele născute din BP.
- Anii: «TOATE» sau o listă / un interval (`2022,2023`, `2020-2023`); NIR-urile din aceiași ani. Gestiunile OI/MF au **un singur exercițiu** (`HISTORY_TYPES`), deci anul = anul din data documentului (BC `DataBC`, BP/BT/BS `DATABP`/`DATABT`/`DATABC`, NIR `Data`), nu `ExercitiuId`.
- Legăturile E-Factura ale NIR-urilor șterse (`EF_PrimiteVerconNir`, `EF_VERCON_ALOCARI`, fără cheie străină) se șterg odată cu NIR-urile. Trimiterea (`EF_PrimiteVercon`) și facturile (`VC_FACTURI*`) rămân.
- Contoarele de numerotare (`VC_Numbering`: `BC`, `BC:<cont>`, `BR`, `BP`, `BT`, `BS`, `OIID`) se pun pe 0 **doar dacă în gestiune nu mai rămâne niciun document din acel fel** (la «TOATE» mereu; la ani parțiali, contoarele rămân dacă mai sunt documente). NIR nu are contor (seria se calculează din rânduri).

## Ce s-a făcut

- **Flux în fereastră (varianta finală, la cererea Adelinei: fără listă, fără lipit, fără nume de tastat):** în listă apare O SINGURĂ comandă, «Golire gestiune OI/MF». În casetă se scriu anii (`TOATE`, `2023`, `2022,2023`, `2020-2023`); dacă baza are o singură gestiune OI/MF nu trebuie nimic altceva, altfel `IdGestiune;ani` sau `numele gestiunii;ani` (`*` listează gestiunile). «Rulează» face ANALIZA; dacă nu e niciun blocaj, același buton devine «Rulează ștergerea»; apăsat, cere confirmare (cu ce a numărat analiza) și șterge. Schimbarea comenzii, a bazei sau a anilor readuce butonul la «Rulează». (Variantele anterioare — valoare de lipit cu numele gestiunii, apoi listă de gestiuni — au fost retrase.)
- `PYTHON/routes/admin_cli/vercon_oi_golire.py` (nou): două comenzi, pe o singură bază și o singură gestiune.
  - `vercon_oi_analiza_golirii` (doar citire; singura vizibilă): numără rândurile de șters pe fiecare tabelă și listează blocajele; când nu e niciun blocaj, blocul «Rezultat analiză» poartă `confirmation_token` (`IdGestiune;ani;nume`) și `confirmation_label`, pe care fereastra le păstrează.
  - `vercon_oi_golire` (scrie, ireversibil; ascunsă în fereastră, pornită doar de «Rulează ștergerea» cu tokenul analizei): numele din token e verificat pe server față de gestiune; o singură tranzacție (`READ COMMITTED`), blocare pe `VC_Gestiuni` ca la scriitorii de documente; refuză gestiunile care nu sunt OI/MF; refuză dacă există declanșator de ștergere pe tabelele atinse; reface numărătoarea în tranzacție și anulează tot dacă o ștergere nu dă exact numărul așteptat; Jurnal; contoare.
  - Ordinea (copii întâi): registru OI → BT → BS → BP → poziții consum → bonuri consum → repartizări facturi → calcule NIR → legături NIR-factură → alocări / trimiteri E-Factura → poziții NIR → storno pe poziții → NIR.
  - Blocaje: documente BU (transfer între subunități) în interval; consumuri pe liste de alimente; **la ani parțiali**: retururi / storno păstrate care arată spre un document șters, BP și BC-ul lui în ani diferiți, transferuri / casări păstrate care au luat bunuri dintr-un lot (`IDBO`) șters.
  - La ani parțiali stocul nu se mai verifică după ștergere (notă în rezultat).
- `PYTHON/routes/admin_cli/admin_cli.py`: comenzile înregistrate (`COMMANDS`, `COMMAND_PARAMS`, `SINGLE_DB_COMMANDS`).
- `KBotAdmin/Views/CommandsView.vb`: comanda de analiză sub VERCON, explicația «Ce te aștepți», butonul «Rulează» → «Rulează ștergerea» (`ConfigureOiWipe`, `HideOiWipe`, `RunOiWipeAsync`); comanda de ștergere în `HiddenCommands`. Nicio modificare în Designer (același buton).

## Verificat

- `py_compile` pe ambele fișiere Python: curat. `dotnet build KBotAdmin/KBotAdmin.vbproj`: 0 avertismente, 0 erori.
- Interogările sunt scrise după `MariaDB_Schema/000_DEMO.sql` (chei și FK-uri citite) și după lanțul de ștergere din `routes/vercon/archive.py`.

## Neverificat / amânat

- **Nimic nu a rulat pe o bază** (regula «fără teste / fără rulări»): sintaxa SQL (DELETE cu JOIN, subinterogări în registru), numărătorile și blocajele sunt verificate doar prin citire. Prima rulare: `*`, apoi analiza pe `000_DEMO`.
- `VC_TransferUnitati*` (BU) nu apare în `MariaDB_Schema/`; blocajul BU se bazează pe formatul numerelor (`BU n`) din `bu_links.py`.
- Fișierele nu sunt pe server; înregistrarea pentru publicare se face abia după proba Adelinei. KBotAdmin nu are pagină în ajutor (unealtă de administrare, nepublicată).
- Rămân neatinse: `VC_ArhivaAnulate`, `VC_Audit`, `VC_Operations`, `VC_AnnualClosings`, `VC_TransferUnitati*`, `VC_Gestiuni.Populated`.
