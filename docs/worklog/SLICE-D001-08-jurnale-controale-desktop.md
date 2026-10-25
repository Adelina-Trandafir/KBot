# SLICE-D001-08 — Jurnale istorice ale controalelor desktop

Data: 25.10.2026. Lot documentar aplicat local.

## What changed and why

- Familia 0010-01…07 este reunită în worklogul 0010-01, cu ancore distincte. Conținutul tehnic, fișierele, probele și limitele fiecărui pas sunt păstrate. Buildul identic al primelor șase etape și actualizarea indexului sunt consemnate o dată; rezultatele de test rămân specifice etapei.
- Contractul inițial IsDirty nu devine regulă curentă: 06 îl înlocuiește explicit. Distincția thumb-track/release din 07 și cele două defecte de proprietăți VB rămân. Reguli și excepții citate în istoric nu înlocuiesc AGENTS actual.
- Registrul 0010 păstrează verdictul istoric fără acceptare vizuală și trimite la detalii; acceptările din alte felii nu sunt extinse automat.

- Cele șapte fișiere 0025 sunt reunite în jurnalul Designer existent. Migrarea MainForm (pasul 7 din plan) și flyoutul 0025-07 rămân identificabile separat; scurtarea ciclului, mărimea 0, DPI/serialization, proprietatea imaginilor și limitele de test/VS sunt păstrate. Nota unică a registrului despre pașii 03/04 fără worklog este transferată, nu eliminată.

## Files touched

Inventar complet; rânduri eliminate (minus = adăugate). Corpul tehnic este păstrat aproape integral în această regrupare, astfel că reducerea de fișiere adaugă separatoare/ancore și evidență.

| Cale | Rânduri |
|---|---|
| `docs/worklog/SLICE-0010-02-kbotdataview-virtualizare.md` | 98 |
| `docs/worklog/SLICE-0010-03-kbotdataview-tipuri-coloana.md` | 64 |
| `docs/worklog/SLICE-0010-04-kbotdataview-formatare-disable.md` | 71 |
| `docs/worklog/SLICE-0010-05-kbotdataview-input-selectie.md` | 83 |
| `docs/worklog/SLICE-0010-06-kbotdataview-editare.md` | 99 |
| `docs/worklog/SLICE-0010-07-scroll-by-column.md` | 67 |
| `docs/worklog/SLICE-0010-01-kbotdataview-skeleton.md` | -505 |
| `docs/worklog/state/KBOT_STATUS_0010-0019.md` | 6 |
| `docs/worklog/SLICE-D001-08-jurnale-controale-desktop.md` | -47 |
| `docs/worklog/state/KBOT_STATUS_D000-D009.md` | -2 |
| `docs/worklog/KBOT_STATUS.md` | 0 |
| `docs/worklog/SLICE-0025-02-navitem-icon.md` | 127 |
| `docs/worklog/SLICE-0025-05-navlist-collapse-padding.md` | 181 |
| `docs/worklog/SLICE-0025-06-navlist-collapse-button-icons.md` | 141 |
| `docs/worklog/SLICE-0025-07-navlist-collapsed-flyout.md` | 150 |
| `docs/worklog/SLICE-0025-08-navlist-corner-radius-gradient.md` | 129 |
| `docs/worklog/SLICE-0025-09-navlist-collapse-button-size.md` | 107 |
| `docs/worklog/SLICE-0025-designer-surfaces.md` | -875 |
| `docs/worklog/state/KBOT_STATUS_0020-0029.md` | 0 |

Bilanț: **19 căi; 12 fișiere retrase, un jurnal nou; 11 fișiere mai puțin și 106 rânduri în plus**. Cumul D001-01…08: **41 fișiere și 4021 rânduri mai puțin**.

## Test results

Numai lectură și analiză documentară. Fără teste, builduri, verificatoare, aplicații, SQL sau acces MariaDB. Probe și comenzi din worklogurile istorice nu sunt executate acum. Fără commit/push; stage numai pentru Markdown propriu.

## Unverified / deferred

Gruparea reduce fragmentarea; proba vizuală și rotunda designerului nu sunt certificate de această lucrare. AVACONT, documentația activă AV, CLAUDE, ajutorul runtime, codul și exporturile brute rămân excluse. Familia 0025 este absorbită; probele descrise rămân istorice. Continuarea documentară este D001-09.
