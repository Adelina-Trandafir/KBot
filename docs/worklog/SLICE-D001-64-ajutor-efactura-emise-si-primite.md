# SLICE-D001-64 — Ajutor E-Factura: emitere și primite

Data: 26.10.2026. Numai documentație, aplicat local.

## What changed and why

20 de jurnale de ajutor sunt reunite în două istorice existente: șase etape emitere/token/unitate și 14 etape facturi primite. Textele, etichetele, capturile, rezultatele verificatorului și limitele sunt păstrate; numai limita identică a etapelor 61/62/63 are o definiție comună. Registrele trimit la ancore, iar jurnalele emiterii/unității au legături actualizate. Coliziunea veche 0000-61 cu ajutorul grupelor este consemnată; rândul grupelor rămâne intact. Nu se modifică fișierele de ajutor runtime sau capturile.

## Files touched

Rânduri eliminate (minus = adăugate).

| Cale | Rânduri |
|---|---|
| `docs/worklog/SLICE-0000-54-ajutor-efactura.md` | -105 |
| `docs/worklog/SLICE-0000-61-ajutor-facturi-primite.md` | -148 |
| `docs/worklog/SLICE-0000-55-ajutor-facturi-emise.md` | 28 |
| `docs/worklog/SLICE-0000-56-ajutor-conturi.md` | 20 |
| `docs/worklog/SLICE-0000-57-ajutor-arbore-facturi.md` | 27 |
| `docs/worklog/SLICE-0000-58-ajutor-date-unitate.md` | 21 |
| `docs/worklog/SLICE-0000-60-ajutor-corectie-luni-atasament.md` | 7 |
| `docs/worklog/SLICE-0000-62-ajutor-fereastra-primite.md` | 7 |
| `docs/worklog/SLICE-0000-63-ajutor-leaga-ddf.md` | 7 |
| `docs/worklog/SLICE-0000-64-ajutor-fila-doar-cu-facturi.md` | 4 |
| `docs/worklog/SLICE-0000-65-ajutor-fila-din-arbore.md` | 3 |
| `docs/worklog/SLICE-0000-66-ajutor-arhiva-zip.md` | 3 |
| `docs/worklog/SLICE-0000-75-ajutor-primite-arbore.md` | 25 |
| `docs/worklog/SLICE-0000-76-ajutor-ribbon-sincronizare.md` | 18 |
| `docs/worklog/SLICE-0000-77-ajutor-primite-fara-file.md` | 12 |
| `docs/worklog/SLICE-0000-78-ajutor-rezultat-sincronizare.md` | 11 |
| `docs/worklog/SLICE-0000-79-ajutor-actualizare-in-subsol.md` | 13 |
| `docs/worklog/SLICE-0000-80-ajutor-optiuni-arbore.md` | 11 |
| `docs/worklog/SLICE-0000-81-ajutor-xml-factura.md` | 11 |
| `docs/worklog/SLICE-0000-82-ajutor-meniu-actualizare.md` | 11 |
| `docs/worklog/SLICE-00EF-08-ecran-facturi-emise.md` | 0 |
| `docs/worklog/SLICE-00EF-13-date-unitate.md` | 0 |
| `docs/worklog/state/KBOT_STATUS_0000-0009.md` | 0 |
| `docs/worklog/state/KBOT_STATUS_D000-D009.md` | -1 |
| `docs/worklog/KBOT_STATUS.md` | 0 |
| `docs/worklog/SLICE-D001-64-ajutor-efactura-emise-si-primite.md` | -50 |

Bilanț: **26 căi; net 17 fișiere mai puțin; 65 rânduri adăugate**. Cumul D001-01…64: **198 fișiere și 7600 rânduri mai puțin**.

## Test results

Numai lectură/comparație documentară; fără teste, builduri, verificatoare, aplicații, ajutor runtime, SQL/MariaDB, commit sau push. Stage doar pentru Markdown propriu.

## Unverified / deferred

Confirmările tehnice/vizuale nu sunt extinse prin consolidare. AVACONT, CLAUDE, ajutorul runtime, codul și exporturile brute sunt excluse. Continuare D001-65.
