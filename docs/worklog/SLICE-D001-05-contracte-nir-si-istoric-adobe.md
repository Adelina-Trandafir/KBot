# SLICE-D001-05 — Contracte NIR/UI, nomenclatoare, rapoarte și istoricul Adobe

Data: 24–25.10.2026. Continuarea loturilor documentare D001, aplicată local. Nu schimbă comportamentul aplicațiilor sau starea probării lor.

## What changed and why

1. Comportamentul editorului NIR este întreținut în `VERCON/FACTURI_NIR_IMPLEMENTARE.md`; FLUXURI_UI, SCHEMA_VC, CONTRACT_CALCULE și rezumatul VC06-06 trimit la acesta și la deciziile D01–D11. Lanțul tehnic de cont, antetul NULL/multicont, metadatele facturii, focusul popupului, dependențele/stocul și limitele migrării rămân explicite. N01/N02 din 12.10 este delimitat de egalitatea exactă pentru NIR adoptată la 16.10. Istoricul corecțiilor UI și probele izolate nu devin probe ale întregului editor.
2. Nouă workloguri Adobe: șapte (0078-07/08/09/11/12/13/14) sunt absorbite în `SLICE-0078-05-fereastra-gazduita-ctrl-h.md`, cu ancore `slice-0078-NN`. Corpurile, datele, listele de fișiere, probele și limitele sunt păstrate, cu ajustarea nivelurilor titlurilor. În plus, 0078-06 intră în 0078-04 (semnarea A/B), iar 0078-16 în 0078-03 (diagnostic ActiveX). Registrul 0070–0079 devine navigabil spre secțiuni și nu mai copiază mecanismele.
3. Istoricul distinge «Da» inițial / «Nu» final la închidere, defectul confirmării constatat în 0078-12 / modificarea ulterioară nerulată din 13, monitorizarea <2024 / recomandarea de versiune >=2025. Un titlu duplicat `Slice 0078` este eliminat. Jurnalele semnării și rescrierii ActiveX rămân separate; comentariile sursă care depind de 0078-15 nu sunt afectate.

4. Opt intrări despre Menu devin trimiteri la contractul comun: dimensiuni, animații și cleanup, tranziția de la 180/320 la 420 ms, excepția explicită reduced-motion, minime pe buton și deplasarea între dropdownuri. Banda, chenarul și umbra aveau deja contract în CONTROALE_CUSTOM_THREAD.
5. Douăsprezece etape din 15.10 despre nomenclatoare/Produse/CPV și ActionAddButton sunt consolidate în VC06-01. Rămân succesiunile adoptat/retras (istoricul de prețuri, pagina CPV → modal, salvare pe celulă → rând, padding stânga → dreapta), validările/importul TIP, schema și cache-ul CPV, contextul/stocurile, filtrele și fișierele pe etapă. Contractele DGV/ModalFrame/ActionAddButton sunt referite, nu recopiate.

6. Opt etape MF/perioade din 20.10 sunt în VC08-11: selecții și identități compuse, deduplicarea variantei la amortizare, mișcări/date efective, ani/luni și fallbackul absent versus null, regenerare peste PDF cu păstrarea parametrilor/raportului anterior, minWidth=0. Confirmarea din 23.10 rămâne numai pentru aceste etape.

## Files touched

Inventar complet (rânduri eliminate; valoarea negativă indică adăugare/absorbție):

| Cale | Rânduri |
|---|---|
| `VERCON/FLUXURI_UI.md` | 27 |
| `VERCON/SCHEMA_VC.md` | 17 |
| `VERCON/CONTRACT_CALCULE.md` | 6 |
| `docs/worklog/SLICE-VC06-06-facturi-comune-corectii-nir-si-schema-unica.md` | 44 |
| `VERCON/FACTURI_NIR_IMPLEMENTARE.md` | -1 |
| `docs/worklog/SLICE-0078-07-adobe-redimensionat-singur.md` | 64 |
| `docs/worklog/SLICE-0078-08-arbore-blocat-cat-se-deschide-pdf.md` | 86 |
| `docs/worklog/SLICE-0078-09-intrebarea-adobe-la-inchidere.md` | 80 |
| `docs/worklog/SLICE-0078-11-procese-adobe-fantoma-la-inchidere.md` | 45 |
| `docs/worklog/SLICE-0078-12-fereastra-gazduita-adobe-vechi.md` | 98 |
| `docs/worklog/SLICE-0078-13-confirmare-suprascriere-si-f8.md` | 30 |
| `docs/worklog/SLICE-0078-14-avertisment-adobe-vechi.md` | 46 |
| `docs/worklog/SLICE-0078-05-fereastra-gazduita-ctrl-h.md` | -490 |
| `docs/worklog/state/KBOT_STATUS_0070-0079.md` | 0 |
| `docs/worklog/SLICE-D001-05-contracte-nir-si-istoric-adobe.md` | -56 |
| `docs/worklog/state/KBOT_STATUS_D000-D009.md` | -2 |
| `docs/worklog/KBOT_STATUS.md` | 0 |
| `docs/worklog/SLICE-0078-04-sectiunea-b-peste-semnat.md` | -152 |
| `docs/worklog/SLICE-0078-06-sectiunea-b-angajament-nou.md` | 145 |
| `docs/worklog/SLICE-0078-03-jurnal-activex.md` | -42 |
| `docs/worklog/SLICE-0078-16-lane-jurnal-activex.md` | 35 |
| `VERCON/CONTROALE_CUSTOM_THREAD.md` | -14 |
| `implementare_vercon_v2.md` | 587 |
| `docs/worklog/SLICE-VC06-01-nomenclatoare-si-configurarea-gestiunii.md` | -96 |
| `docs/worklog/SLICE-VC08-11-rapoarte-oi-structura-si-selectie.md` | -39 |

Bilanț: 25 de căi atinse, 9 fișiere retrase și un jurnal nou; **8 fișiere și 418 rânduri eliminate net** în lot. Cumul D001-01…05: **28 de fișiere și 3546 rânduri eliminate net**.

## Test results

Numai citire și analiză documentară sursă/destinație. Nu s-au rulat teste, builduri, verificatoare, aplicații sau SQL. Rezultatele din jurnalele absorbite sunt istorice. Stage-ul cerut de utilizator pentru D001-01…04 conține numai Markdown-ul propriu; fragmentele AVACONT din registrele comune sunt excluse. Fără commit/push.

## Unverified / deferred

Lot închis documentar; consolidarea continuă în D001-06. AVACONT, documentația activă AV și fișierele CLAUDE sunt excluse. Ajutorul runtime, codul și exporturile sursă nu sunt modificate. Nu se certifică funcționarea linkurilor în aplicație sau starea mediului utilizatorului prin reorganizare editorială.
