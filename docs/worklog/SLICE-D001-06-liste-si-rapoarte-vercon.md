# SLICE-D001-06 — Liste, fișe, rapoarte și import VERCON

Data: 25.10.2026. Lot documentar aplicat local, continuarea D001-05.

## What changed and why

- Opt etape ale formularului Lista de alimente (17.10) sunt în VC07-01: antet/meniuri, păstrarea D ascuns, allowCustom, sold anterior și recapitulare BigInt, editori FELUL/Explicații și corecțiile de focus/scroll. Stările locale nerulate, publicarea necesară și fișierele sunt păstrate.
- Patru etape ale machetei și proveniența serverului demonstrativ sunt în VC08-03. Matricea separă examinarea PDF-ului fictiv, răspunsul Excel HTTP 200 și versiunile ulterioare ale exportului/multipaginării nerulate. Nu se extinde confirmarea la MariaDB sau producție.
- Titlurile/ancorele jurnalului general sunt păstrate și trimit la sursele de detaliu.

- Cele două revizii numerice/de copiere și popupul HTML VRC (14.10) sunt în VC05-03/09: DECIMAL și precizia EF/VC, sursa păstrată fără recalculare/normalizare de semn, CHECK-urile/SQL manual și caracterul istoric al generatorului, raportul HTML fără recitire și restricțiile lui. Nu se transformă vechea confirmare de compilare a writerului în compilare a noii revizii.

- Șase etape FCS/BC și cache (17.10) sunt în VC06-03/04: disponibil la dată/excludere, numerotare anuală comună NIR/FCS, evitarea duplicatelor, cache în memorie cu setările/invalidările lui, mini fișe și tipografia lor. Regula inițială a disponibilului în consultare este delimitată de corecția ulterioară; contorul FCS separat rămâne numai istoric.

- Șapte etape NIR/TreeView sunt centralizate în VC06-06 și contractul TreeView: fișele informative, geometria și formulele/ordinea lor, factura cumulată readonly versus ținta locală de salvare, liniile externe fără payload și schimbarea contextului cu cereri anulate/lock. Marcajele și datele locale nu sunt reinterpretate.

## Files touched

Inventar complet (rânduri eliminate, minus = informație absorbită/adăugată):

| Cale | Rânduri |
|---|---|
| `docs/worklog/SLICE-VC07-01-lista-alimente.md` | -46 |
| `implementare_vercon_v2.md` | 648 |
| `docs/worklog/SLICE-VC08-03-rapoarte-magazie-alimente.md` | -38 |
| `docs/worklog/SLICE-D001-06-liste-si-rapoarte-vercon.md` | -44 |
| `docs/worklog/state/KBOT_STATUS_D000-D009.md` | -2 |
| `docs/worklog/KBOT_STATUS.md` | 0 |
| `docs/worklog/SLICE-VC05-03-writer-atomic-si-maparea-cheilor.md` | -12 |
| `docs/worklog/SLICE-VC05-09-rezultat-colorat-si-ordinea-butoanelor.md` | -6 |
| `docs/worklog/SLICE-VC06-04-br-si-fcs.md` | -27 |
| `docs/worklog/SLICE-VC06-03-bc-si-controlul-disponibilului.md` | -15 |
| `docs/worklog/SLICE-VC06-06-facturi-comune-corectii-nir-si-schema-unica.md` | -26 |
| `VERCON/CONTROALE_CUSTOM_THREAD.md` | -2 |

Bilanț: **12 căi, 430 rânduri eliminate net**, un jurnal nou și niciun fișier retras în acest lot. Cumul D001-01…06: **27 fișiere și 3976 rânduri mai puțin**.

## Test results

Numai citire și analiză documentară; fără teste, builduri, verificatoare, aplicații, SQL sau acces MariaDB. Probe și operații de publicare descrise în istoric nu sunt executate acum. Stage selectiv numai pentru Markdown propriu, conform cererii utilizatorului; fără commit/push.

## Unverified / deferred

Lot închis documentar; consolidarea continuă în D001-07. AVACONT, documentația activă AV, fișierele CLAUDE, ajutorul runtime și exporturile sursă sunt excluse. Nu se certifică starea funcțională prin reorganizare.
