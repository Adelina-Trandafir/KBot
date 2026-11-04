# SLICE-AD19-04 — Report fără validare prin situația istorică

## Cerere și limită

Adelina a cerut eliminarea verificării care blochează transferul separat când soldul calculat din MDB nu coincide cu prima situație salvată din 2021: istoricul se portează cu valorile sale, inclusiv greșelile. A precizat explicit că restul trebuie să rămână la fel.

## Modificarea

În AdeOpeningBalanceSource.Build a fost eliminat numai blocul care, pentru copilul existent în Platitori, verifica unicitatea/completitudinea primei situații salvate din 2021 și compara SID−SIC ajustat cu soldul calculat. Acest snapshot nu este necesar calculului din SI și mișcările sursei și nu mai poate bloca rezultatul.

Formula reportului rămâne aceeași: soldul complet înainte de 2021 minus SI de bază, cu plăți/restituiri neanulate și conversia existentă la Access Long. Situațiile salvate nu sunt rescrise sau corectate; diferențele lor istorice nu sunt introduse artificial în reportul folosit la calculul lunilor următoare.

Ramura pentru copilul absent din Platitori, care are nevoie de prima situație salvată ca sursă a soldului, rămâne integral neschimbată, inclusiv validarea acelei surse. Sunt păstrate verificările de identitate, hartă, SI de bază, perioade, mișcări fără lună, limite numerice, stabilitatea sursei/destinației, blocarea și tranzacția. Nu au fost modificate formularul, filtrul AD19-03, writerul, migrarea completă, Python, formulele sau schema.

## Fișiere proprii

1. `ADECHIT/ADE.Migrator/AdeOpeningBalanceSource.vb` — eliminarea blocului de validare descris.
2. `ADECHIT/ADE.Migrator/README.md` — regula actuală și limita recuperării din snapshot.
3. `ADECHIT_STATUS.md` — indexul intervenției.
4. `docs/worklog/state/ADECHIT_STATUS_ADE10-ADE19.md` — stare și următoarea subfelie AD19-05.
5. `docs/worklog/SLICE-AD19-04-report-fara-validare-situatie-istorica.md` — acest worklog.

## Probe și restanțe

SCRIS LOCAL, NECOMPILAT, NETESTAT, NEPUBLICAT. Numai citire și inspecție de proiectare a surselor necesare, urmate de editarea blocului și a documentației aferente. Fără teste, compilări, pornirea aplicației, citiri noi ale MDB-ului sau operații pe baze de date. Operatorul compilează/distribuie și probează previzualizarea unui copil cu situație istorică neconcordantă și recitirea după aplicare; eligibilitatea efectivă depinde în continuare de celelalte validări.

## Înregistrare

În așteptarea confirmării explicite pentru cele cinci fișiere ale acestei intervenții; lotul AD19-03 rămâne de asemenea neînregistrat. Fișierele comune se înregistrează cu versiunile curente și motivele aplicabile ambelor subfelii. Nu s-a executat scriptul de înregistrare sau Git local. Motiv AD19-04: «Eliminarea exclusivă a validării reportului calculat față de prima situație istorică din 2021, cu păstrarea valorilor istorice și a celorlalte validări».
