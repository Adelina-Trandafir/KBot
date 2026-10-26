# SLICE-D001-73 — Migrare: istoricul HTTP și trecerea la citirea directă

Data: 26.10.2026. Numai documentație, aplicat local.

## What changed and why

Ambele istorice au fost citite integral. S-au compactat paragrafele și continuările listelor și s-au eliminat separatoarele de secțiune redundante; cuprinsul folosește ancorele deja existente. Rămân integral diferențele dintre rutare/selecție, insert-if-absent/upsert, validare/conversie, jurnal reconstructiv/tranzacție și coloana absentă/NULL. Primele probe Access nu devin probe de scriere MariaDB; descoperirea, revizia pe DDL și corecția unității rândului rămân cronologice. Exemplele de schema_sync/DDL/SSH sunt istoric citit, fără autorizare sau execuție în această lucrare. Nu s-au citit AVACONT, dumpuri ori documente Tests.

## Files touched

Rânduri eliminate (minus = adăugate).

| Cale | Rânduri |
|---|---|
| `docs/worklog/SLICE-0044-migrare-accdb-impins.md` | 477 |
| `docs/worklog/SLICE-0045-01-descoperire-nomenclatoare.md` | 484 |
| `docs/worklog/state/KBOT_STATUS_D000-D009.md` | -1 |
| `docs/worklog/KBOT_STATUS.md` | 0 |
| `docs/worklog/SLICE-D001-73-migrare-http-si-directa-compactare.md` | -29 |

Bilanț: **5 căi; net -1 fișiere mai puțin; 931 rânduri mai puțin**. Cumul D001-01…73: **233 fișiere și 9829 rânduri mai puțin**.

## Test results

Numai lectură/comparație documentară; fără teste, builduri, verificatoare, aplicații, ajutor runtime, SQL/MariaDB, commit sau push. Stage doar pentru Markdown propriu.

## Unverified / deferred

Confirmările tehnice/vizuale nu sunt extinse prin consolidare. AVACONT, CLAUDE, ajutorul runtime, codul și exporturile brute sunt excluse. Continuare D001-74.
