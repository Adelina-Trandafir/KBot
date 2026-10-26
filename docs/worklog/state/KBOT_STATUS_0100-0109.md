# K-BOT — STATUS, slices 0100–0109

Everything recorded about each slice: its registry row, its «Current focus» notes and its
«Open threads» notes. The index in `../KBOT_STATUS.md` says what each slice is.

---

## Slice 0100

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0100 | Descărcări multiple — arhitectura inițială | GATA pe cod (build `src/` 0 avertismente, 0 erori); nimic rulat, nevăzut; **DDL + server nedeployate** | [0100](../SLICE-0100-multithreading-descarcari.md#slice-0100-baza) | Contractele, probele și reviziile rămân în etapa legată. |
| 0100-02 | Permisiune și limită multi-thread din server | GATA pe cod (build `src/` 0 avertismente, 0 erori); nimic rulat, nevăzut; **DDL + server nedeployate; ajutorul: 0000-36 (fără capturi)** | [0100-02](../SLICE-0100-multithreading-descarcari.md#slice-0100-02) | Contractele, probele și reviziile rămân în etapa legată. |
| 0100-03 | Progres și coada robotului, cu opt revizii | GATA pe cod (build `src/` 0 avertismente, 0 erori); nimic rulat, nevăzut; **ajutorul: 0000-48 (captura `coada-descarcari-multiple` de făcut, nu există încă)** | [0100-03](../SLICE-0100-multithreading-descarcari.md#slice-0100-03) | Contractele, probele și reviziile rămân în etapa legată. |
| 0100-04 | Clicul acoperit de bara fixă și timpii Reverse | GATA pe cod (build `KBot.App` 0 avertismente, 0 erori; nimic rulat, nevăzut); `KBot.Forexe` 1.0.19 ▸ 1.0.20; fără ajutor / NOUTATI (ordinul operatorului) | [0100-04](../SLICE-0100-multithreading-descarcari.md#slice-0100-04) | Contractele, probele și reviziile rămân în etapa legată. |
| 0100-05 | Capturi după actualizare multiplă, fără zgomot | GATA pe cod (build `KBot.App` 0 avertismente, 0 erori; nimic rulat); fără ajutor / NOUTATI (ordinul operatorului) | [0100-05](../SLICE-0100-multithreading-descarcari.md#slice-0100-05) | Contractele, probele și reviziile rămân în etapa legată. |
| 0101 | **Lanțuri de recepții care nu se închid: roșu în fereastra de asociere și în arborele principal; «+» pe lună fără mesaj de succes (cererea operatorului, 02.10.2026)** — `AsociereForm` (R și ultimul H în roșu, peste orice altă regulă), `GET /api/forexe/tree` (coloana `LantNeinchis`), arborele din `KbotForm` (rând roșu + motivul în tooltip), generarea în lot din Plăți fără caseta de după | GATA pe cod (build `src/` 0 avertismente, 0 erori); nimic rulat, nevăzut; **`tree.py` nedeployat; ajutorul: 0000-37** | `SLICE-0101-lanturi-neinchise-si-ord-lot.md` | Fără DDL. SQL-ul testat de operator pe `014_SCSV`: 5 din 39 angajamente semnalate. `tests/test_forexe_tree.py` neatins și nerulat (coloană nouă pe rând). |
| 0101-01 | **ORD și DDF: bara cu pagini rămâne pe o lună / «Toate…»; pagina «Documente» are lista de tipărire (cererea operatorului, 02.10.2026)** — pe o lună sau pe rădăcină «Vizualizare» arată din nou datele (rândurile tuturor ordonanțărilor / reviziilor de sub ea), iar pagina «Document» / «Document PDF» se numește «Documente» și arată lista de tipărire din 0099; pe o frunză rămâne documentul; `OrdView` + `DdfView` (`IsListNode` / `IsListTab`), `KBotNavList.SetItemText` | GATA pe cod (build `src/` 0 avertismente, 0 erori); nimic rulat, nevăzut; **ajutorul: 0000-38** | `SLICE-0101-01-documente-pe-luna-ord-ddf.md` | Fără server, fără DDL. Neverificat: trecerea frunză (pagina Document, cu Adobe) → lună și înapoi. |
| 0101-02 | **Fără fereastra «Istoric angajament» după o salvare în pagina FOREXE (cererea operatorului, 02.10.2026)** — scos apelul `DeschideIstoricInterval` și metoda din `KbotForm.ForexeWatch.vb`; **`IstoricIntervalForm` rămâne în proiect, neatinsă**; ajutorul: paragraful și captura `istoric-interval` din `browser.md` șterse | GATA pe cod (build `src/` 0 avertismente, 0 erori); nimic rulat, nevăzut | `SLICE-0101-02-fara-fereastra-istoric.md` | Fără server, fără DDL. |
| 0102 | Buget pe versiuni datate — regula inițială | GATA pe cod (build `src/` 0 avertismente, 0 erori; Python compilat); nimic rulat, nevăzut; **DDL + server nedeployate; ajutorul: 0000-40 (captura `clasificatii` de refăcut)** | [0102](../SLICE-0102-buget-pe-versiuni.md#slice-0102) | Reguli, ajutor, ordine de instalare și limite în etapa legată. |
| 0103 | Interogări unice și registru pe baze | GATA pe cod (AvacontPush 0 avertismente, 0 erori; Python compilat); nimic rulat pe o bază sau peste SSH, fila nevăzută; **DDL pe șablon + server nedeployate** | [0103](../SLICE-0103-interogari-unice.md#slice-0103-baza) | Contractele, probele și reviziile rămân în etapa legată. |
| 0103-02 | AvacontPush — cheie SSH și ținte bifate | GATA pe cod (AvacontPush 0 avertismente, 0 erori); nicio conectare făcută, nevăzut; fără ajutor | [0103-02](../SLICE-0103-interogari-unice.md#slice-0103-02) | Contractele, probele și reviziile rămân în etapa legată. |
| 0103-03 | Buget de deschidere 1/12 în Migrator | GATA pe cod (build curat, nimic rulat; cere DDL-ul 0102; fișierul anului anterior dedus din nume) | [0103-03](../SLICE-0103-03-buget-deschidere-migrator.md#slice-0103-03) | Contractele, probele și reviziile rămân în etapa legată. |
| 0103-04 | Comparația bugetului K-BOT cu FOREXE | GATA pe cod (build curat, nimic rulat, nevăzut; server nedeployat; ajutorul: 0000-41) | [0103-04](../SLICE-0103-03-buget-deschidere-migrator.md#slice-0103-04) | Contractele, probele și reviziile rămân în etapa legată. |
| 0103-05 | Buget Access → K-BOT prin VBA | GATA pe cod (Python compilat; VBA necompilat, neimportat în Access; server nedeployat; fără ajutor, e cod VBA) | [0103-05](../SLICE-0103-05-vba-access-spre-kbot.md#slice-0103-05) | Contractele, probele și reviziile rămân în etapa legată. |
| 0103-06 | Buget K-BOT → Access | GATA pe cod (build curat, nimic rulat pe un fișier Access real; «registru» citit ca `cale.accdb`; parola unică ofuscată în cod; ajutorul: 0000-41) | [0103-06](../SLICE-0103-05-vba-access-spre-kbot.md#slice-0103-06) | Contractele, probele și reviziile rămân în etapa legată. |
| 0104 | Componente Access, două pachete și pagina de setări | GATA pe cod (build `src/` curat; sondă fără ferestre pe încărcarea componentei; nimic rulat cap-coadă; **DDL + server nedeployate**) | [0104](../SLICE-0104-componente-access-separate.md#slice-0104), [0104-02](../SLICE-0104-componente-access-separate.md#slice-0104-02) | Starea inițială este păstrată; confirmarea operatorului din 03.10 privește închiderea aplicației, cu limitele din jurnal. |
| 0105 | Clasificații — sumar pe noduri și filtrul de mișcare | GATA pe cod (build curat, nimic rulat, nevăzut; **server nedeployat**: ruta nouă `GET …/clasificatii/sumar-buget`; ajutorul: 0000-42 în `clasificatii.md`) | [0105](../SLICE-0105-clasificatii-sumar-si-miscare.md#slice-0105) | Etapele ulterioare ale grilelor sunt în același istoric. |
| 0106 | Capturi, retry PDF și clic dreapta fără reîncărcare | GATA pe cod (build `KBot.App` 0 avertismente, 0 erori; nimic rulat, nevăzut; fără ajutor) | [0106](../SLICE-0106-captura-folder-clic-dreapta-pdf.md#slice-0106) | Cele trei remedieri și problemele excluse sunt delimitate în jurnal. |
| 0107 | Arbore — clic dreapta fără selecție; grile de buget | GATA pe cod (build `KBot.App` 0 avertismente, 0 erori; nimic rulat, nevăzut) | [Arbore](../SLICE-0106-captura-folder-clic-dreapta-pdf.md#slice-0107), [buget](../SLICE-0105-clasificatii-sumar-si-miscare.md#slice-0107-buget) | Ajutor 0000-43 pentru arbore, 0000-46 pentru grile; probe și limite pe etapă. |
| 0107-02 | Buget — total pe rând și subsoluri condiționate | GATA pe cod (build `KBot.App` 0 avertismente, 0 erori; nimic rulat, nevăzut) | [0107-02](../SLICE-0105-clasificatii-sumar-si-miscare.md#slice-0107-02) | Ajutor 0000-44; fără total agregat în subsolul bugetului. |
| 0108 | Credit pe clasificație și total anual — amendament | GATA pe cod (build `KBot.App` + `KBot.Migrator` 0 avertismente, 0 erori; nimic rulat, nevăzut) | [0108](../SLICE-0102-buget-pe-versiuni.md#slice-0108-01) | Reguli, ajutor, ordine de instalare și limite în etapa legată. |
| 0109 | Actualizare multiplă și prin coada cu un singur robot | GATA pe cod (build `KBot.App` 0 avertismente, 0 erori; nimic rulat, nevăzut; ordinea cozii și întrebarea de recepții doar din citirea codului; NOUTATI: linia din worklog de adăugat la următoarea versiune) | [0109](../SLICE-0100-multithreading-descarcari.md#slice-0109) | Reguli, ajutor, ordine de instalare și limite în etapa legată. |

### Current focus

- Branch `SLICE-0100-Multithreading`. Nimic din partea browserului (taburi în același context, adoptarea tabului păstrat)
  nu a fost văzut rulând.

### Open threads

- **0100-04/05:** [clicul acoperit și capturile](../SLICE-0100-multithreading-descarcari.md#slice-0100-04), cu proba propusă pe 5+ indicatori; nerulate.
- **0108:** [instalare, chei de coloane presupuse, limite și restanța stg](../SLICE-0102-buget-pe-versiuni.md#slice-0108-01); nimic rulat/văzut la checkpoint. Eliminarea stg rămâne cererea pentru o felie separată, fără operații aici.
- **0104:** [instalarea și probele cap-coadă încă restante](../SLICE-0104-componente-access-separate.md#slice-0104), inclusiv PC curat și cele două categorii de actualizare. [Confirmarea închiderii](../SLICE-0104-componente-access-separate.md#inchidere-remediere) din 03.10 nu le închide.
- **0103:** [drepturi, ordine și interogări repetabile](../SLICE-0103-interogari-unice.md); nimic rulat/văzut.
- **0103-03…06:** [ordinea și confirmările](../SLICE-0103-03-buget-deschidere-migrator.md), [puntea Access/parola](../SLICE-0103-05-vba-access-spre-kbot.md); nimic rulat/văzut.
- **0102:** [condițiile de instalare, versiunile de ianuarie, combo-ul și proba UI](../SLICE-0102-buget-pe-versiuni.md#slice-0102); calculul pe trimestre este amendat în 0108.
- **0101 — nimic văzut pe ecran** (rândurile roșii din arbore și din fereastra de asociere, tooltipul nou; lizibilitatea roșului pe un rând SELECTAT). `tree.py` trebuie pus pe server; până atunci clientul nou nu arată roșu. Ajutorul e făcut în 0000-37, fără capturi noi.
- **0101-01 — nimic văzut pe ecran** (numele filei «Documente», trecerea între listă și pagina de date, trecerea frunză → lună cu un document deschis în Adobe). Ajutorul: 0000-38, fără capturi de refăcut (cele patru poze ale listei nu fuseseră făcute).
- **0100-03:** [coada, oprirea individuală și follow-up-urile](../SLICE-0100-multithreading-descarcari.md#slice-0100-03); limitele/probele rămân pe revizie.
- **0100:** [probe taburi și reperele registrului](../SLICE-0100-multithreading-descarcari.md); neexecutate.
- **0100:** [DDL și server](../SLICE-0100-multithreading-descarcari.md#slice-0100-baza) înaintea datelor reale; limita de timp și proiectul de teste necompilabil sunt consemnate acolo.
- **0100-02:** [capturile 0000-36](../SLICE-0000-36-ajutor-setari-multithread.md#captures) cer o unitate cu Multithread = 1.
- **0100-02:** [ordinea publicării](../SLICE-0100-multithreading-descarcari.md#slice-0100-02); Setari lipsă înseamnă multi-thread oprit.
