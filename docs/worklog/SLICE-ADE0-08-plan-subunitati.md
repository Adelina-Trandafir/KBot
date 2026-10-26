# SLICE-ADE0-08…09 — Planul subunităților și analiza selectivă a surselor

Plan și analiză din 09.10.2026. [Planul detaliat](../../ADECHIT/plan_subunitati.md), inclusiv tabelele și scenariile din §12, rămâne separat; [implementarea ADE10](SLICE-AD10-01-schema-subunitati.md) consemnează etapele ulterioare. Diagnosticele citirilor eșuate și reușite rămân distincte.

- [ADE0-08](#slice-ADE0-08)
- [ADE0-09](#slice-ADE0-09)

## SLICE-ADE0-08 — plan pentru subunități în același DC <a id="slice-ADE0-08"></a>

Data: 09.10.2026. Stare: **TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026)**.

Utilizatorul a cerut un plan, fără cod, pentru coexistența mai multor evidențe MDB în același DC și comutarea între ele. A confirmat denumirea **Subunitate** și serii și contoare de chitanțe separate pentru fiecare subunitate.

### Livrabile

- `ADECHIT/plan_subunitati.md`: model, izolarea datelor și relațiilor, maparea ID-urilor, configurații de chitanțe, API, selector, migrator, conversia datelor existente, etape și scenarii de acceptare.
- `ADECHIT_STATUS.md` și `docs/worklog/state/ADECHIT_STATUS_ADE0-ADE9.md`: înregistrarea planului și actualizarea următoarei subfelii de analiză.

### Analiză și limite

Citire statică a regulilor/statusului, exporturilor locale MariaDB și componentelor ADE Python/JS/VB relevante. Exportul `000_DEMO.sql` este datat 09.10.2026; nu confirmă schema fiecărui DC țintă.

A fost identificată dependența temporală din `service.py` care compară numeric `IDL + 1`. Planul cere păstrarea comportamentului sursei printr-o ordine locală explicită înaintea remapării globale; nu autorizează schimbarea regulii de business.

Nu s-au modificat codul aplicației, scripturile SQL sau sursele MDB. Nu s-au rulat teste, verificări vizuale, migrări ori operații pe server. Implementarea și scenariile de acceptare rămân planificate. Denumirile concrete și exporturile țintelor se clarifică înaintea etapelor dependente.

## SLICE-ADE0-09 — verificare selectivă a MDB-urilor pentru subunități <a id="slice-ADE0-09"></a>

Data: 09.10.2026. Stare: **TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026)**.

Utilizatorul a indicat `ADECHIT/ACCESS_SOURCES` pentru verificarea bazelor distincte, cu instrucțiunea explicită de a nu le citi integral.

### Operații și rezultate

Inventariate `baza_40.mdb` și `baza_47.mdb`. Citire ACE OLE DB cu `Mode=Read`, numărători, metadate și interogări TOP limitate, fără exportul tuturor rândurilor. Nu s-a pornit MSACCESS. Au fost comparate 151 de coloane din 13 tabele relevante; nu au fost găsite diferențe de nume/tip/lungime/nullable între aceste două surse.

Serii proprii: GR40 și G.R.; valori NUMAR: 6715 și 10199. DC-uri sursă diferite: 051_GR40 și 047_GR47. Destinația părinte rămâne de confirmat. Aceleași luni calendaristice, ID-uri de grupe/taxe comune și intervale de plăți suprapuse.

Constatări relevante: baza_40 nu are chitanțe, dar are contor configurat; baza_47 păstrează SS_Buget pentru 2020–2026, dar numai două luni în LunaD. Referințe la luni lipsă: 295 în Retur din baza_40 și 35.834 în SS_Buget din baza_47. Chitante și AlteDoc nu au IDL în aceste surse. Detaliile și scenariile suplimentare sunt în secțiunea 12 din `ADECHIT/plan_subunitati.md`.

### Diagnostice și limite

Prima citire de metadate a returnat rezultate pentru ambele surse, dar procesul shell a terminat cu cod de eroare nativ. Interogările ulterioare au fost executate cu Windows PowerShell și au terminat cu succes. Prima verificare a referințelor a întâlnit lipsa coloanei IDL; interogările au fost apoi limitate la tabelele unde coloana există. Nu se declară reușite comenzile eșuate.

Nu s-au citit în masă date personale, nu s-au modificat MDB-urile, codul sau SQL-ul. Nu s-au rulat teste ale aplicației, migrări ori operații pe server. Agregările nu validează relațiile în întregime sau paritatea soldurilor. Fișierul vechi baza2020_PP.mdb nu este prezent în inventarul curent; referințele istorice nu sunt folosite ca dovadă a schemei actualelor MDB-uri.

Actualizate planul, indexul și registrul ADE0. Următoarea subfelie de analiză: ADE0-10.
