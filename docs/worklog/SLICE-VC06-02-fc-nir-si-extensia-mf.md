# SLICE-VC06-02 — FC/NIR, inclusiv extensia MF

Stare: **IMPLEMENTAT_LOCAL**. Doar sintaxa verificata (`py_compile`, `node --check`, o verificare automata de nume nedefinite); **nimic rulat**, nicio proba, nimic vazut pe ecran, nimic pe MariaDB. Cererea operatorului, 12.10.2026 («fa tot VC06»).

## Ce s-a facut
- **Recepție NIR** (`documents.create_nir/edit_nir`, `GET/POST/PUT /api/vercon/doc/nir…`): furnizor (Parteneri), categorie/cont, comisie, factura (numar, data, valoare), numar si data NIR, **o singura grila** de produse (combobox cu cautare), cota TVA pe linie. Tip «Stoc inițial» fara furnizor.
- **Calculul il face serverul** (CONTRACT_CALCULE 2.2): `PUTVA = rnd(Pret·(1+cota))`, `Valoare = rnd(Cant·Pret)`, `ValoareCUTVA = rnd(Cant·PUTVA)`, `TVA = ValoareCUTVA − Valoare`, rotunjit o data pe linie la `Zec`, stocat la `Scara`; refuz (nu rotunjire) la prea multe zecimale; pret < 0 si cantitate ≤ 0 refuzate.
- **Totalul antetului N01/N02:** valoarea facturii declarata de operator 0/goala/la mai mult de 1 leu de suma liniilor → suma liniilor (`numeric.header_total`).
- **MF:** aceeasi grila are nr. inventar, data intrarii, durata normala/folosita, valoare lunara (= total cu TVA / durata normala). Fiecare linie creeaza un activ nou: o varianta (`VC_ProduseP`, `COD` = nr. inventar) + un rand `VC_ProduseM`, in aceeasi tranzactie cu NIR. Cantitate intreaga.
- **Numerotare** (`protocol.next_number`): contor `VC_Numbering` sub citire cu blocare + primul numar cu adevarat liber; numar manual doar daca e liber (si ridica contorul).
- **Pagina** (`docs.js`): antet pe doua-trei coloane, grila NIR compacta, `+ Adaugă rând` in footer, totaluri, arbore **furnizor → data → factura + total** in dreapta (ascuns la adaugare/modificare), Previzualizeaza (fereastra tiparibila).
- Cheia cererii (`VC_Operations`): un raspuns pierdut e reluat, nu produce al doilea NIR.

## Fisiere
`PYTHON/routes/vercon/`: `protocol.py`, `documents.py`, `doc_routes.py` (noi); `__init__.py` (inregistrare). `PYTHON/static/js/vercon/docs.js` (nou), `app.js`; `PYTHON/static/css/vercon.css`.

## Presupuneri si neverificat
- Linia alege o **varianta existenta**; pretul de pe linie nu modifica `ProduseP.PUTVA` (valorizarea stocului ramane pretul variantei, modificabil din nomenclator, decizia Q9). Un pret nou = o varianta noua in nomenclator.
- `NumarSub` se scrie (0) doar daca coloana exista pe server (verificat la rulare prin `information_schema`); dump-ul din `MariaDB_Schema` nu o are.
- Perioada: la Materiale/Alimente data NIR trebuie sa fie in anul exercitiului.
- Nimic rulat; ValoareLuna foloseste rotunjirea HALF_EVEN a contractului (de verificat la VC08-05).
