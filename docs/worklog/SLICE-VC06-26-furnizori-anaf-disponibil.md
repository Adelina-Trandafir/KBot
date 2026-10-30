# SLICE-VC06-26 — Sincronizarea ANAF disponibilă tuturor utilizatorilor listei

Data: 30.10.2026.

Stare: **IMPLEMENTAT_LOCAL, NERULAT, NEPUBLICAT**.

Utilizatorul a cerut ca „Sincronizează cu ANAF” să fie activată oricând, după ce opțiunea apărea dezactivată. Cauza din cod: VC06-22 acorda `parteneri_anaf` exclusiv rolului Gestionar, iar VC06-23 afișa opțiunea dezactivată pentru celelalte roluri.

Permisiunea limitată `parteneri_anaf` este acum disponibilă oricui are dreptul VERCON `read` în unitatea curentă. Interfața nu mai dezactivează opțiunea după rol; rămâne protecția temporară în timpul unei operații/selecții în curs. Acțiunea din meniul rândului selectează furnizorul respectiv înainte de sincronizare, deci nu depinde de o selecție anterioară.

Se păstrează autentificarea, dreptul VR al unității, verificarea contextului, lock-ul de scriere, versiunea rândului și toate restricțiile de câmp: denumirea/adresa vin numai din ANAF; manual se poate salva numai CodFiscal, după răspuns explicit „negăsit”. Mesajul serverului pentru un cod găsit între timp indică noua etichetă „Sincronizează cu ANAF”. Nu se acordă drepturi generale de nomenclatoare sau recepție.

Combinarea partenerilor folosea prezența `parteneri_anaf` drept indicator al rolului Gestionar. Pentru a păstra exact restricția existentă numai asupra Gestionarului, verifică acum rolul real din unitate prin helperul deja existent. Altfel, noua disponibilitate ANAF ar fi blocat neintenționat combinarea pentru toate rolurile. Drepturile de combinare ale celorlalte roluri nu sunt extinse sau retrase.

Fișiere de aplicație:

- `PYTHON/routes/vercon/__init__.py`, `operations.py`, `partners.py`, `supplier_merge.py`;
- `PYTHON/static/js/vercon/partners.js`.

Fără teste, build, verificări automate/de sintaxă, browser, HTTP sau SQL. Analiză de proiectare prin citirea traseului drepturilor și a consumatorilor permisiunii. Publicarea necesită cele cinci fișiere și repornirea serviciului web; nu necesită DDL sau seed SQL.
