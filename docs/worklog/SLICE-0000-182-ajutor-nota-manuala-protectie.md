# SLICE-0000-182 — ajutor: «Notă manuală / Modifică operație», fila «AVACONT», regulile de ștergere

Felia de funcționalitate AV08-01..06 (24.11.2026).

## What changed and why

- Subiect nou `contabil.nomenclatoare.nota-manuala` (`screens: NotaManualaForm`; editorul AV02-07 nu avea subiect): cum se deschide, datele și perioada închisă (AV08-02), avertismentele (AV08-05), casetele marcate și «Reîncarcă» (AV08-05), protecția operațiilor (AV08-01), ce se poate modifica (AV02-07, AV08-06).
- `contabil.nomenclatoare.modificare-operatii`: «Modifică operație» deschide editorul și lista rămâne pe operația salvată; la ștergere contează și plata din anul următor; aceleași reguli la «Șterge plata» din Facturi și plăți și Contări (AV08-04).
- `contabil.nomenclatoare.operatii`, secțiunea «Operații diverse» (AV08-04, a doua trecere): data cel mult cu un an în urmă și refuzul unui șablon din jurnalul **JI**, cu mesajul exact.
- `contabil.nomenclatoare.operatii`, secțiunea «Facturi și plăți», și `contabil.efactura`, secțiunea «Contări» (AV08-07): datele, numărul facturii și data O.P. se înregistrează cum sunt scrise; o dată greșită oprește salvarea cu mesaj, nu se mută.
- `contabil.setari`: fila «AVACONT» în tabelul paginilor și secțiune nouă «Pagina «Aplicație»: fila «AVACONT»» (AV08-01); captura `setari` marcată `redo` (fila nouă).
- `help-version.txt` rămâne 2026-11-24; reperul din `KBOT_STATUS_0000-0009.md` mutat la 0000-182.

## Files touched

`src/KBot.App/HelpContent/contabil/nomenclatoare/nota-manuala.md` (nou), `modificare-operatii.md`, `operatii.md`, `contabil/setari.md`, `contabil/efactura/index.md`; `docs/worklog/state/KBOT_STATUS_0000-0009.md`.

## Test results

`Check-Help.ps1 -Coverage`: `NotaManualaForm` nu mai apare în lista ferestrelor fără subiect; mesajele rămase sunt vechi (alte subiecte, alte felii). Build App 0/0.

## Unverified / deferred

- Capturi de refăcut: `setari` (fila nouă «AVACONT»). Editorul nu are captură (fereastră modală).
- Nevăzut pe ecran.
