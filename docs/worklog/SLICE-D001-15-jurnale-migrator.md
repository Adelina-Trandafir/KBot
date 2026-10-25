# SLICE-D001-15 — Jurnalele istorice ale migratorului

Data: 25.10.2026. Aplicat local, exclusiv documentație.

## What changed and why

Cele patru jurnale 0044 sunt reunite în jurnalul HTTP inițial; cele șapte 0045, în jurnalul descoperirii migratorului direct. Corpurile tehnice sunt păstrate integral, cu titluri subordonate/ancore. Regruparea elimină nouă fișiere vechi, însă adaugă rânduri de trasabilitate. Registrele/focusul repetitiv trimit la aceleași surse.

Se păstrează trecerea JSON/HTTP → direct, cele două situații ale fișierelor sursă, corelațiile/NULL/cheile/ORD, ordinea, arhitectura jurnalului SQL, porțile și limitele citirii Access față de scrierea MariaDB. Istoricul creării/sincronizării schemei nu este autorizare de execuție acum. Notele unice de focus sunt transferate; existența AVACONT_COMUN, metadatele dumpului, LastInsertedId și acceptarea formularului rămân explicit datate.

## Files touched

Rânduri eliminate (minus = adăugate).

| Cale | Rânduri |
|---|---|
| `docs/worklog/SLICE-0044-02-selectie-unitate-upsert-bife.md` | 132 |
| `docs/worklog/SLICE-0044-03-corelatii-coloane-grile-kbot.md` | 165 |
| `docs/worklog/SLICE-0044-04-sql-dump-coloane-obligatorii.md` | 713 |
| `docs/worklog/SLICE-0044-migrare-accdb-impins.md` | -1023 |
| `docs/worklog/SLICE-0045-02-motor-access-mariadb.md` | 152 |
| `docs/worklog/SLICE-0045-03-corelatii-si-porti.md` | 153 |
| `docs/worklog/SLICE-0045-04-scriere-si-jurnal-sql.md` | 130 |
| `docs/worklog/SLICE-0045-05-formular-si-stergerea-lantului-http.md` | 185 |
| `docs/worklog/SLICE-0045-06-schema-sync-progres-si-constatari.md` | 176 |
| `docs/worklog/SLICE-0045-07-unitatea-randului-nu-a-buclei.md` | 136 |
| `docs/worklog/SLICE-0045-01-descoperire-nomenclatoare.md` | -952 |
| `docs/worklog/state/KBOT_STATUS_0040-0049.md` | 8 |
| `docs/worklog/state/KBOT_STATUS_D000-D009.md` | -2 |
| `docs/worklog/KBOT_STATUS.md` | 0 |
| `docs/worklog/SLICE-D001-15-jurnale-migrator.md` | -41 |

Bilanț: **15 căi; nouă fișiere retrase, un jurnal nou; 68 rânduri adăugate**. Cumul D001-01…15: **59 fișiere și 4480 rânduri mai puțin**.

## Test results

Numai lectură/comparație documentară, fără teste, builduri, verificatoare, procese, registry, SQL/MariaDB sau SSH. Fără commit/push; stage doar pentru Markdown propriu. Fișierele Access/exporturile citate în istoric nu au fost deschise acum.

## Unverified / deferred

Stările istorice ale uneltelor nu sunt recertificate. AVACONT, CLAUDE, ajutorul runtime, codul și exporturile brute rămân excluse. Continuare D001-16.
