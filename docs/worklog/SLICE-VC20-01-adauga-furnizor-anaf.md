# SLICE-VC20-01 — Adăugarea furnizorilor (ANAF) în macheta Furnizori și furnizorul nou din e-Factura la salvarea NIR-ului

18.11.2026 — Cerință (Adelina): în VERCON online, adăugarea furnizorilor de către Administrator; se verifică după codul fiscal că nu există înainte de salvare; se introduce întâi codul fiscal, datele vin din ANAF; NU se umblă la coduri de angajament sau solduri. Dacă din e-Factura contabilului ajunge un furnizor nou, să poată fi adăugat de acolo, la salvarea NIR-ului.

**Corectura din 18.11.2026 (aceeași zi).** Prima variantă era o pagină separată «Adaugă furnizor» cu intrare în meniu: respinsă de Adelina («NU trebuie să fie o pagină separată»). Pagina, intrarea din meniu și CSS-ul ei au fost șterse; adăugarea s-a mutat în macheta existentă «Furnizori (Parteneri)».

## Ce s-a făcut

**«+» în macheta Furnizori (`partners.js`, `vercon-partners.css`).** Lângă titlul formularului din dreapta, butonul «+» (`ActionAddButton`, vizibil numai cu dreptul `parteneri_adauga`, adică cine are «receptie»). La click: formularul se golește, lista (DGV) și căsuța de căutare se dezactivează, Cod partener se completează cu următorul cod liber (doar de citit), Codul fiscal devine editabil. La pierderea focusului din Cod fiscal se interoghează serverul (`/api/vercon/suppliers/check`): furnizor existent → mesaj și nu se salvează; ascuns → se poate salva (devine vizibil); negăsit în ANAF → mesaj; găsit → Denumire și Adresă completate. Sub Adresă, jos, aliniate cu partea de jos a listei: «Renunță» (stânga) și «Salvează» (dreapta), vizibile doar după «+». După salvare, furnizorul apare în listă și e selectat.

**Server — `PYTHON/routes/vercon/supplier_create.py`.**
- `GET /api/vercon/suppliers/next-code`: următorul Cod partener = maximul codurilor numerice scurte (1–6 cifre) din toată baza + 1, cu cel puțin 3 cifre (022, 074, 1049…); codurile care sunt de fapt coduri fiscale se ignoră; recalculat sub blocare la salvare.
- `POST …/check` și `POST …/create` `{cod_fiscal}`: clientul trimite NUMAI codul; denumirea și adresa vin dintr-un răspuns ANAF proaspăt. Verificarea de duplicat se face pe TOATE rândurile `Parteneri` ale bazei (toate unitățile clientului, ascunse incluse), normalizat după codul fiscal, sub blocarea rândului unității. Cod existent doar ca rând ascuns → rândul devine din nou vizibil (audit `supplier.reactivate`), nu se adaugă încă unul. Se scrie numai `Parteneri` (`Provenienta=VERCON_FURNIZOR`); `Parteneri_Angajamente` și soldurile nu se ating. Răspunsul conține și rândul în forma listei.
- `partners.py`: lista întoarce și `can_add`. `operations.py` / `__init__.py`: operația `parteneri_adauga`, acordată oricui are `receptie`. `capabilities.py`: intrarea din meniu a fost ștearsă.

**Lista de furnizori din NIR.** Neschimbată: `receipt_choices.partners` folosește `suppliers.relation(True)`, care grupează după codul fiscal toate rândurile active ale bazei, fără să țină cont de unitate. Singurele rânduri care lipsesc din listă sunt cele ascunse (`Ascuns=1`); pentru ele, «+» oferă reactivarea.

**Furnizor nou din e-Factura.**
- `PYTHON/routes/efactura/primite_vercon.py`: lipsa furnizorului din `Parteneri` nu mai blochează trimiterea la Vercon de către contabil; `trimite` sare peste activarea/legarea partenerului când nu există niciunul. Aplicația K-BOT ocolește deja dialogul de alegere la 0 parteneri.
- `efactura-intake.js` + `supplier-new.js` + `docs.js`: la «NIR din e-Factura…», dacă factura nu are partener, furnizorul (CUI-ul facturii) intră în câmpul Furnizor ca «nume (nou)». La **Salvează**, înainte de NIR, se apelează `create` cu `if_missing`; dacă eșuează (ANAF indisponibil, cod necunoscut), NIR-ul nu se salvează și se arată motivul. Dacă Administratorul alege între timp alt furnizor, nu se adaugă nimic.

**Ajutor.** `vercon.json` — secțiunea «Adăugarea unui furnizor nou» (VC20-01); `src/KBot.App/HelpContent/contabil/efactura/index.md` — furnizorul nou nu mai blochează trimiterea.

## Fișiere

- `PYTHON/routes/vercon/supplier_create.py`, `partners.py`, `operations.py`, `capabilities.py`, `__init__.py`
- `PYTHON/routes/efactura/primite_vercon.py`
- `PYTHON/static/js/vercon/partners.js`, `supplier-new.js`, `efactura-intake.js`, `docs.js`, `app.js`
- `PYTHON/static/css/vercon-partners.css`, `vercon.css`
- `PYTHON/static/help/vercon.json`, `src/KBot.App/HelpContent/contabil/efactura/index.md`
- ȘTERSE: `PYTHON/static/js/vercon/supplier-add.js`, `PYTHON/static/css/vercon-supplier-add.css` (existau în registrul de deployment înainte de ștergere)
- `VERCON/VERCON_STATUS.md`, `implementare_vercon_v2.md`, acest worklog

## Presupuneri

- «Furnizorul nou din e-Factura la salvarea NIR-ului» = contabilul poate trimite factura chiar dacă furnizorul nu există, iar Administratorul îl adaugă la salvarea NIR-ului.
- Cine poate adăuga: oricine are dreptul `receptie`, nu numai rolul Administrator.
- Cod partener: următorul număr liber global (nu codul fiscal), cu cel puțin 3 cifre.
- IBAN și Banca nu se completează la adăugare (ANAF nu le dă); se pot completa ulterior numai dacă formularul permite.

## Neverificat

Doar sintaxa (`py_compile`, `node --check`, JSON valid). Nerulat în aplicație: «+», golirea formularului, dezactivarea listei, preluarea ANAF la pierderea focusului, alinierea butoanelor Renunță / Salvează cu partea de jos a listei, inserția în `Parteneri` (inclusiv `next_code` pe date reale), reactivarea unui rând ascuns, NIR din e-Factura cu furnizor nou, trimiterea contabilului fără partener. Nicio rulare de teste. Nepublicat. Fișierele VC20-01 deja înregistrate în registrul de deployment au acum alt conținut (iar două au fost șterse): înregistrarea se refă abia după proba Adelinei.
