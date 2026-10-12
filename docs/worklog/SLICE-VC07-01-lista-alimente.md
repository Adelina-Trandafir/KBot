# SLICE-VC07-01 — ListaAlimente

Stare: **IMPLEMENTAT_LOCAL**. Doar sintaxa verificata (`py_compile`, `node --check`, verificare automata de nume nedefinite); **nimic rulat**, nimic vazut pe ecran, nimic pe MariaDB. Cererea operatorului, 12.10.2026 («continuă cu VC07»).

## Ce s-a facut
- **Lista de alimente** (`lista.py`, `js/vercon/lista.js`, rutele `/api/vercon/doc/lista…`): antet (numar automat/manual, data, comentarii), cele trei mese (mic dejun, dejun, cina/gustare) cu **porții, valoare porție, valoare masă (= porții × valoare porție, rotunjit o data la ZEC) si distribuit**, feluri pe masa (fel din nomenclator + tip + explicatie; randurile fara explicatie nu se salveaza, ca in Access) si **setul de BC-uri**.
- **O lista are mai multe BC-uri** (`lista_bc.set_bonuri`, 1:N): DGV in stilul NIR, un rand per bon, `+ Adaugă rând` deschide popupul **Alege B.C.** (bonurile gestiunii si exercitiului, neanulate, fara retur, fara a fi in alta lista), alegerea ADAUGA la set, ⋮ scoate doar legatura din draft. Footer: TOTAL BC, valoare lista, diferenta.
- **Salvarea e atomica** (o tranzactie): antet + mese + feluri + intreg setul LegBCLA; nu creeaza si nu resalveaza bonuri, nu repeta consumul; un BC devenit neeligibil opreste salvarea intregului set (`BON_INVALID`), draftul ramane. Lista fara bonuri cere confirmare. `ValoareLista` = suma meselor; `ValoareBon` = suma bonurilor neanulate; `DEP` = lista − bonuri.
- **Anulare** cu motiv: lista ramane marcata anulata, **bonurile sunt eliberate** (legaturile se sterg, bonurile nu se ating). **Editare** cu revizie (`CONFLICT`).
- **Nomenclator «Feluri de mâncare»** (`VC_FeluriMancare`) adaugat in meniul Configurari pentru gestiunile Alimente.
- Arbore **luna → data → lista + valoare** in dreapta, ascuns la adaugare/modificare.
- O modificare de reteta nu rescrie o lista salvata (lista isi pastreaza textul si valorile).

## Fisiere
`PYTHON/routes/vercon/`: `lista.py` (nou), `doc_routes.py`, `nomenclatoare.py`, `capabilities.py`; `PYTHON/static/js/vercon/lista.js` (nou), `app.js`, `nomenclatoare.js`.

## Presupuneri si neverificat
- **Retetarul (`VC_Retetar`) nu este implementat**: tabelul nu are `Activ`, are cheia proprie `IdFel` fara legatura clara cu `FeluriMancare` si **0 randuri** in bazele reale; nu s-a inventat un model. Ramane deschis.
- Sensul coloanelor `D` («distribuit») si `VMD`/`VD`/`VC` (valoare/portie) este luat din formularul Access (`MicDejun_V = porții × VMD`); distribuitul nu este limitat la porții.
- «Alimente necesare»/costuri din reteta nu exista (eliminate prin decizie, FLUXURI_UI).
- Cele 127 de liste istorice nu au fost reconciliate pe snapshot (necesita rulare); legaturile lor LegBCLA existente se citesc ca atare.
- Nimic rulat.
