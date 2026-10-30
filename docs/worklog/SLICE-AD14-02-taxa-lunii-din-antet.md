# SLICE-AD14-02 — Taxa lunii deschise din antet

30.10.2026. Cerință: schimbarea taxei și în timpul lunii, prin combobox în locul textului simplu «Taxa zilnică» din antet.

## Ce s-a schimbat și de ce

- Eticheta de citire a fost înlocuită de Combobox-ul comun, în același loc din antet. Câmpul arată suma curentă; lista arată valoarea și explicația taxelor pozitive ale subunității. Ordonarea are cheie numerică explicită pe valoare, apoi chei pentru explicație în română și data începutului; nu se sortează după ID sau eticheta formatată.
- După alegerea altei taxe, confirmarea explică aplicarea pentru întreaga lună selectată și toate grupele subunității. Nu reprezintă taxe diferite pe zile ale aceleiași luni. Sunt păstrate zilele introduse; pentru fiecare rând lunar se actualizează IDV și se recalculează ValoareContract = zile × taxă și ValoareTotala cu conversia Access Long existentă.
- Se actualizează și IDV al lunii, astfel încât noii copii preluați în aceeași lună și raportul Situație de plată să folosească taxa aleasă. Situația deschisă se citește din nou, cu păstrarea sortării și a copilului selectat.
- Se folosește dreptul existent Catalog. Luna închisă este refuzată pe server și selectorul ei este dezactivat; dreptul de editare a prezenței închise nu permite această comandă. Înainte de schimbare se finalizează celula editată; documentele începute și operațiile în curs blochează schimbarea. Interfața se blochează pe durata salvării/reîncărcării pentru a nu abandona editări începute între timp.
- Ruta nouă `/api/adechit/month-tax` refolosește izolarea subunității, blocările, tranzacția, verificările de versiune și idempotenta existente. Întreaga schimbare este atomică. O eroare revine controlat în UI; reîncercarea aceluiași payload păstrează cheia operației.
- Alegerea lunii nu modifică valoarea, perioadele sau bifa Activ a taxelor din catalog. Lunile închise și celelalte luni/subunități nu sunt rescrise. Taxa activă din catalog rămâne implicită pentru o lună nouă. Pentru valori noi se folosește catalogul Taxe existent.
- Vizibilitatea pe telefon rămâne cea existentă: controlul din antet nu se afișează la lățimi de maximum 980px.

## Fișiere de aplicație de publicat

- `PYTHON/static/adechit.html` — înlocuirea etichetei cu gazda comboboxului.
- `PYTHON/static/css/adechit-layout.css` — dimensiuni și stil din tema comună.
- `PYTHON/static/js/adechit/month-tax.js` — control, încărcare, confirmare, salvare și reîmprospătare.
- `PYTHON/static/js/adechit/app.js` — dependențele controlului și starea aplicației; logica taxei rămâne în modulul ei.
- `PYTHON/routes/adechit/month_tax.py` — comanda lunară și recalcularea prezenței, fără schimbări de schemă.
- `PYTHON/routes/adechit/__init__.py` — ruta protejată Catalog.
- `PYTHON/routes/adechit/service.py` — comentariu și mesaj de taxă utilizată actualizate pentru alegerea într-o lună deschisă; validările catalogului rămân cele existente.
- `PYTHON/static/help/adechit.json` — descrierea noului control și a condițiilor lui.

Documentație: `ADECHIT/UTILIZARE_WEB.md`, `ADECHIT_STATUS.md`, `docs/worklog/state/ADECHIT_STATUS_ADE10-ADE19.md` și acest worklog.

## Verificări și limite

**SCRIS LOCAL.** Conform AGENTS.md nu s-au rulat teste, compilări, verificări executabile, aplicația sau SQL. S-a făcut numai analiză de cod și proiectare pe schema locală `MariaDB_Schema/AVACONT_SURSA.sql` (export 29.10.2026: LunaD.IDV, Prezenta.IDV/valori, ValoriTaxe și versiuni). Nu sunt necesare DDL sau seed SQL. Nu s-au făcut commit, publicare, restart sau probe MariaDB.

De probat de operator după publicare: alegerea unei taxe într-o lună cu copii în două grupe; valorile, soldurile și Situația de plată; zilele păstrate; preluarea ulterioară a unui copil; lună fără prezență; Renunță; lună închisă și cont fără Catalog; modificări concurente de lună/taxă; refacerea cererii după răspuns pierdut; păstrarea sortării/seleției și aspectul dropdownului în temele aplicației.

Ajutorul textual este actualizat cu AD14-02. Capturile `01-panouri-luna-grupa.jpg` și `02-tabel-complet.jpg` rămân de actualizat pentru combobox; nu s-au realizat capturi noi. Nu se afirmă validarea vizuală.

## Predare

Operatorul publică prin AvacontPush cele opt fișiere de aplicație de mai sus și probează scenariile indicate. Backendul și fișierele web se publică împreună. **Fără SQL de rulat pentru AD14-02.**
