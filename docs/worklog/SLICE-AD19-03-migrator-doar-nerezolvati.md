# SLICE-AD19-03 — ADE.Migrator: doar copiii nerezolvați

## Cerere și limită

Adelina a cerut afișarea numai a cazurilor nerezolvate după completarea soldurilor reportate și investigarea punctuală a unui copil în MDB-ul complet indicat. Intervenția adaugă filtrarea previzualizării și documentează mecanismul blocajului. Nu schimbă formule, validări financiare, tabele sau date și nu reia migrarea.

## Schimbări

- Bifa comună KBotCheckBox «Doar copiii nerezolvați», creată în Designer și bifată implicit, păstrează rândurile De aplicat, Omise și Blocate.
- Starea explicită Resolved este atribuită numai reportărilor deja egale cu valoarea propusă, cazurilor fără report suplimentar necesar și rândurilor aplicate după COMMIT confirmat. Un blocaj rămâne vizibil chiar dacă nu este aplicabil.
- Filtrul schimbă numai grila, păstrând previzualizarea integrală și selecția existentă pentru aplicare. Debifarea permite consultarea întregii liste inclusiv după salvare. Salvarea reușită dezactivează aplicarea până la o nouă previzualizare, deoarece toate rândurile aplicabile au fost consumate.
- Rezumatul arată copii afișați/total, nerezolvați, de aplicat și omişi/blocați. Ordinea alfabetică românească a sursei și sortarea selectată în controlul comun sunt păstrate; valorile numerice rămân numerice.
- Documentația de utilizare explică faptul că starea rezolvat se referă la transferul reportului, nu la toate diferențele dintre situațiile lunare.

## Diagnostic cerut pe date reale

Citire punctuală numai în mod Read prin ACE/OleDb în Windows PowerShell Desktop, fără pornirea Microsoft Access și fără afișarea parolei. Nu s-au executat comenzi de modificare în MDB sau MariaDB. Datele personale și detaliile documentelor citite nu au fost copiate în proiect.

Pentru cazul investigat, calculul din SI și mișcările valabile înainte de 2021 nu coincide cu SID−SIC din prima situație salvată din 2021. Diferența este egală cu o plată anulată din istoricul anterior; situațiile ulterioare consultate coincid cu soldul recalculat. Acest lucru sugerează un efect păstrat în situații istorice după anulare, fără a demonstra data sau operația exactă care a produs neconcordanța. Codul existent blochează automat transferul pe baza primei situații salvate: aceasta explică blocajul sursă. Destinația nu a fost citită pentru acest copil, iar celălalt caz raportat nu a fost identificat nominal.

Ocolirea validării sau alegerea altei situații de referință nu sunt incluse. Nu a fost aplicată componenta propusă și nu au fost corectate situațiile istorice.

## Fișiere proprii

1. `ADECHIT/ADE.Migrator/AdeOpeningBalanceTransfer.vb` — stare explicită rezolvat/nerezolvat.
2. `ADECHIT/ADE.Migrator/AdeOpeningBalanceForm.Designer.vb` — bifa implicită în Designer.
3. `ADECHIT/ADE.Migrator/AdeOpeningBalanceForm.vb` — filtrare, rezumat și starea listei după aplicare.
4. `ADECHIT/ADE.Migrator/README.md` — utilizarea filtrului și limita validării.
5. `ADECHIT_STATUS.md` — indexul intervenției.
6. `docs/worklog/state/ADECHIT_STATUS_ADE10-ADE19.md` — stare, restanțe și următoarea subfelie AD19-04.
7. `docs/worklog/SLICE-AD19-03-migrator-doar-nerezolvati.md` — acest worklog.

## Probe și restanțe

SCRIS LOCAL, NECOMPILAT, NETESTAT, NEPUBLICAT. Nu s-au rulat teste, compilări sau aplicația; citirea și analiza punctuală MDB au fost cerute explicit de Adelina. Inspecție de proiectare a controlului comun: ClearRows nu resetează cheia/sensul sortării, iar EndUpdate reconstruiește afișarea.

Operatorul compilează/distribuie și probează bifa implicită/debifarea, rândurile deja corecte/fără report, rândurile De aplicat, blocate/omise și afișarea după COMMIT. Nu există DDL sau cod Python de publicat pentru această subfelie.

## Înregistrare

În așteptarea confirmării explicite a Adelinei pentru cele șapte fișiere și versiunile acestei intervenții. Confirmarea anterioară pentru AD19-02 nu este refolosită pentru modificările ulterioare. Nu s-a executat scriptul de înregistrare și nu s-a executat Git local. Motiv propus: «Filtru implicit pentru copiii cu sold reportat nerezolvat, cu păstrarea blocajelor și a explicațiilor în previzualizare».
