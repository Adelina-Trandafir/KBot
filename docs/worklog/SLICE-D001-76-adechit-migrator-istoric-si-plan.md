# SLICE-D001-76 — ADECHIT: migratorul, reconcilierea și amendamentele

Data: 26.10.2026. Numai documentație, aplicat local.

## What changed and why

Cincisprezece jurnale ADE5-01…14/ADE10-07 sunt reunite în două istorice: pipeline-ul inițial și utilitarul direct. Corpurile, probele, cifrele, ordinea conversiilor, regulile NULL, rezultatul necunoscut al COMMIT, oprirea după rollback și corecția chitanțelor rămân pe etapă. Nota identică Rezolvat din cinci etape este definită o singură dată cu domeniul și data confirmării operatorului. Planul păstrează titlurile/ancorele 05…14, trimite detaliile duplicate la jurnal și păstrează utilizarea/numărul următor; nu schimbă celelalte familii ADE. Registrele păstrează stările și destinațiile noi. Nu s-au modificat surse, schema, runtime, documentație Tests sau fișiere AVACONT/CLAUDE.

## Files touched

Rânduri eliminate (minus = adăugate).

| Cale | Rânduri |
|---|---|
| `docs/worklog/SLICE-ADE5-01-extract-read-only.md` | -15 |
| `docs/worklog/SLICE-ADE5-04-ade-migrator.md` | -225 |
| `docs/worklog/SLICE-ADE5-02-import-repetabil.md` | 5 |
| `docs/worklog/SLICE-ADE5-03-reconciliere.md` | 5 |
| `docs/worklog/SLICE-ADE5-05-migrator-oprire-jurnal.md` | 59 |
| `docs/worklog/SLICE-ADE5-06-migrator-log-formular.md` | 47 |
| `docs/worklog/SLICE-ADE5-07-migrator-dc-editabil.md` | 34 |
| `docs/worklog/SLICE-ADE5-08-grupa-plecati.md` | 19 |
| `docs/worklog/SLICE-ADE5-09-cnp-parinte.md` | 27 |
| `docs/worklog/SLICE-ADE5-10-log-nou-pornire.md` | 26 |
| `docs/worklog/SLICE-ADE5-11-cnp-lipsa-null.md` | 28 |
| `docs/worklog/SLICE-ADE5-12-taxe-lipsa-null.md` | 24 |
| `docs/worklog/SLICE-ADE5-13-chei-randuri-generate.md` | 21 |
| `docs/worklog/SLICE-ADE5-14-oprire-fara-exceptie.md` | 26 |
| `docs/worklog/SLICE-ADE10-07-migrator-chitante-dublate-si-platitori-reconstituiti.md` | 47 |
| `ADECHIT/PLAN_IMPLEMENTARE.md` | 35 |
| `docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md` | 0 |
| `docs/worklog/state/ADECHIT_STATUS_ADE10-ADE19.md` | 0 |
| `docs/worklog/state/KBOT_STATUS_D000-D009.md` | -1 |
| `docs/worklog/KBOT_STATUS.md` | 0 |
| `docs/worklog/SLICE-D001-76-adechit-migrator-istoric-si-plan.md` | -45 |

Bilanț: **21 căi; net 12 fișiere mai puțin; 117 rânduri mai puțin**. Cumul D001-01…76: **244 fișiere și 11149 rânduri mai puțin**.

## Test results

Numai lectură/comparație documentară; fără teste, builduri, verificatoare, aplicații, ajutor runtime, SQL/MariaDB, commit sau push. Stage doar pentru Markdown propriu.

## Unverified / deferred

Confirmările tehnice/vizuale nu sunt extinse prin consolidare. AVACONT, CLAUDE, ajutorul runtime, codul și exporturile brute sunt excluse. Continuare D001-77.
