# SLICE-VC05-07 — partenerii din ANAF în aceeași tranzacție cu transferul

Stare: **VALIDAT_SERVER** (000_DEMO). Cererea operatorului, 12.10.2026: «dacă dă fail, pică și ei».

## Facut
- Fereastra «Parteneri ANAF» nu mai scrie nimic în bază. «Pregătește pentru transfer» reține partenerii în `VrcPlan.NewPartners` (nu intră în hash-ul planului); «Anulează pregătirea» îi șterge.
- `VrcPartnerStore.AddInTransaction(cn, tx, dc, items)` rulează pe conexiunea și tranzacția transferului și nu face niciodată COMMIT. `VrcRunner.AddPreparedPartners` îl apelează după `OpenSources` și înainte de `LoadPartners`, deci NIR-urile se leagă direct de partenerii noi.
- Orice eșec, oprire sau diferență de reconciliere => rollback pentru tot, inclusiv partenerii. Dacă între timp apare un partener cu același cod fiscal, cel pregătit nu se adaugă, iar linia din raport o spune.
- «Testează» nu mai numără furnizorii pregătiți ca lipsă și spune câți vor fi adăugați în aceeași tranzacție.
- Raportul are secțiunea «Parteneri din ANAF», cu o linie pe partener; apare și la rollback, alături de verdictul de sus.
- Lista pregătită se pierde la alegerea altui folder sau la citirea altor fișiere; rămâne după un transfer anulat.

## Dovada
Raportul din rularea operatorului pe `000_DEMO` (COMMIT confirmat): 13 parteneri din ANAF adăugați (coduri 1073–1085, unitatea 75 / 02A), numărătorile sursă/țintă egale pe toate tabelele, 4 NIR fără cod fiscal rămase fără partener (X04).

## Neacoperit
- Nu s-a provocat un eșec după inserarea partenerilor ca să se vadă dispariția lor (rollback): urmează din construcția tranzacției, dar n-a fost probat.
- Un singur DC (000_DEMO) și o singură unitate de SS.
