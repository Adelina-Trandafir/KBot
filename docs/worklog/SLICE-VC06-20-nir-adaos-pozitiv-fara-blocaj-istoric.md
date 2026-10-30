# SLICE-VC06-20 — NIR: adaos pozitiv fără blocaj pentru deficit istoric

30.10.2026 — **IMPLEMENTAT_LOCAL, NERULAT, NEPUBLICAT**.

## Cerere și cauză

La adăugarea produsului APA 19L într-un NIR, validarea ciornei răspundea cu HTTP 409, STOCK_NEGATIVE, deficit 4.0000 la 19.05.2026. Datele furnizate de utilizator arată un deficit cronologic deja existent: intrare 10 la 08.05, ieșiri 5 la 11.05, 4 la 14.05 și 5 la 19.05, înaintea intrării următoare din 22.05. Acest istoric nu trebuie să împiedice adăugarea unei intrări pozitive.

`receipt_rules.simulate` verifica întregul istoric al fiecărei variante din ciornă, fără să distingă adaosul pozitiv de reducerea stocului. La editare exista și un control după scriere pe toate variantele atinse, care putea produce același blocaj la salvare.

## Schimbări

- `PYTHON/routes/vercon/receipt_rules.py`: compară contribuția cumulată a ciornei cu contribuția NIR-ului salvat, pe variantă de preț și dată efectivă de stoc (`DataFc`, cu fallback la `Data`). Simulează istoricul numai pentru variantele a căror contribuție scade la cel puțin o dată. Un NIR nou cu cantități pozitive nu reduce contribuția; nici adăugarea de linii sau creșterea cantității la aceeași variantă și dată. Agregarea pe zi evită tratarea împărțirii aceleiași cantități în mai multe linii ca reducere.
- `PYTHON/routes/vercon/receipts.py`: controlul de după scriere, în tranzacția editării, folosește numai variantele identificate cu reducere de contribuție de simularea anterioară. Un adaos pozitiv nu mai este respins la acest al doilea control.
- Reducerea/eliminarea cantităților, mutarea intrării la o dată ulterioară și schimbarea variantei de produs/preț păstrează controlul cronologic pentru variantele afectate prin reducere. Regulile celorlalte documente și motorul comun de stoc nu au fost schimbate.

## Stare și limite

Analiză de proiectare pe surse și pe textul furnizat. Fără teste, compilare, verificări automate, browser, cereri HTTP, aplicație pornită sau operații MariaDB. Comportamentul nu este confirmat prin execuție. Nu s-au modificat date istorice sau scheme și nu s-a publicat codul.
