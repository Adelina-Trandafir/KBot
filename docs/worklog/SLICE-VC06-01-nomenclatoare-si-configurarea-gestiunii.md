# SLICE-VC06-01 — nomenclatoare si configurarea gestiunii

Stare: **IMPLEMENTAT_LOCAL**. Doar sintaxa verificata (`py_compile`, `node --check`); **nimic rulat**, nicio proba, nimic vazut pe ecran, nimic pe MariaDB. Cererea operatorului, 12.10.2026 («toate odata»).

## Ce s-a facut si de ce
- **Provider real** (`routes/vercon/provider.py`, `SqlProvider`): pana acum productia folosea `EmptyProvider`, deci nicio gestiune nu aparea. Arborele subunitate -> gestiune -> ani vine acum din `VC_Subunitati` / `VC_Gestiuni` / `VC_Exercitii` (doar active; `Zec`/`Scara` incluse). Pe un DC fara tabelele `VC_*` raspunsul ramane «in pregatire» (eroarea 1146 -> arbore gol). `set_type` real, doar cat `Populated=0`.
- **Nomenclatoare** (`nomenclatoare.py`, tabel declarativ): categorii/conturi (`VC_Masini`), produse (`VC_Produse`) cu variante de pret (`VC_ProduseP`), clienti/gestionari (`VC_Clienti`; «Gestionari» din ribbon deschide Clienti, nu exista alt tabel), cote TVA (`VC_TVA`), comisie receptie / inventariere. Furnizorii raman in `Parteneri` (partajat cu K-BOT, fara editare aici). `FeluriMancare`/`Retetar` raman la VC07-01.
- **Reguli comune:** randul apartine unei singure gestiuni (`ScopedRepository`); nimic nu se sterge, doar inactiveaza; revizie `Version` la fiecare scriere (revizie veche = CONFLICT, nu suprascriere); `RequestKey` la creare (`VC_Operations`, raspuns reluat, aceeasi cheie cu alt corp refuzata); rand de audit (`VC_Audit`) in aceeasi tranzactie, CNP mascat; validari pe server (lungimi, obligatoriu, TVA 0-100 %, pret >= 0 pe INT/scara gestiunii, refuz — nu rotunjire — la prea multe zecimale). Primul pret scris seteaza `Populated=1` (inghiata tipul si scara).
- **Rute** (`nom_routes.py`): `GET/POST /api/vercon/nom/<kind>`, `PUT .../<id>`, `POST .../<id>/active`, `GET/POST /api/vercon/settings` (zecimale + scara, doar cat gestiunea nu are date; contoarele din `VC_Settings` doar afisate). Scrierile trec prin `scoped` (context re-verificat, tip, revizia contextului, jetonul curent de ocupare) si o singura tranzactie.
- **Meniu:** formulare noi `tva`, `comisie_nir`, `comisie_inventar` (drept `nomenclatoare`); raspunsul `forms/<f>/open` poarta `nom` (nomenclatoarele formularului).
- **Pagina** (`js/vercon/nomenclatoare.js`, `app.js`, `api.js`, `vercon.css`): lista/arbore + formular, butoane Adauga / Modifica / Renunta / Salveaza in footer (ca in schita Access `F_Produse` si in mockupurile NIR/BC: grila compacta, borduri subtiri); produsele au arbore categorie -> produs, variantele de pret apar sub produsul ales. Modificarile nesalvate cer confirmare la schimbarea randului/contextului/inchiderea paginii.

## Fisiere
`PYTHON/routes/vercon/`: `provider.py`, `nomenclatoare.py`, `nom_routes.py` (noi); `__init__.py`, `capabilities.py`, `repository.py` (`update(..., bump_version)`). `PYTHON/static/`: `js/vercon/nomenclatoare.js` (nou), `app.js`, `api.js`, `css/vercon.css`.

## Neverificat / amanat
- Nimic rulat: nici rutele, nici SQL-ul pe `000_DEMO`, nici pagina pe ecran. In previzualizarea locala (`VERCON/tools/preview.py`) nomenclatoarele raspund 503 (nu are baza de date).
- Presupuneri de verificat: `Version`/`Activ` exista pe toate tabelele folosite (DDL citit, nu si serverul); `VC_Audit.UserEmail` primeste e-mailul in clar (e in baza, nu in logger); nu exista verificare de unicitate pe denumire (istoricul migrat poate repeta nume).
- Neacoperit: ajutorul VERCON (nescris, ca la VC02-05), probele din `PROBE_BROWSER`, ecranul de furnizori, importul/exportul nomenclatoarelor, afisarea pretului la `Zec` (se afiseaza la scara de stocare).
- Mockupurile din `2026-10-12-corectate-v2` nu contin ecrane de nomenclatoare; layout-ul urmeaza regulile lor generale si schita `F_Produse`.
