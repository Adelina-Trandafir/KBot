# SLICE-0076-02 - receptie stearsa in browserul FOREXE din K-BOT

**Data:** 06.11.2026. **Cerere (operator):** cand un user STERGE o receptie in browserul din aplicatie, K-BOT o marcheaza direct ca stearsa, ca la venirea istoricului sa stie la ce sa se astepte. **Reguli (operator, 06.11.2026):** (1) fereastra de confirmare a stergerii **nu trebuie prinsa de sistemul JS**; (2) **K-BOT NU apasa niciodata singur «Da» in ea — CRITIC**; (3) o stergere confirmata de FOREXE **nu mai poate fi afectata din ferestrele de asociere**. Planul: `.claude/plans/mellow-popping-sifakis.md`; continua [0076-01](SLICE-0076-01-data-receptiei-schimbata.md).

> **STARE: SFARSITUL DETECTIEI DIN BROWSER ESTE PROVIZORIU — DE FINALIZAT.** Stergerea se recunoaste acum dupa **numarul de cosuri** din pagina (mai putine decat la clic, fara fereastra deschisa, pagina nu e ocupata). Operatorul a hotarat (06.11.2026) ca asta **ramane pana suntem siguri ce mesaj arata pagina** cand stergerea s-a terminat (probabil de acelasi fel ca mesajul de salvare cu succes; operatorul nu il stie si nu poate testa acum). **Dupa ce mesajul e cunoscut, detectia dupa cosuri se inlocuieste cu el.** Metoda e declarata nefiabila.

## 1. Ce s-a facut

- **Browser** (`ForexeWatch.js`): operatia `receptie-stergere`. `start` = clicul pe cos (`a > i.glyphicon-trash`, in randurile tabelului de pe tab1); se retine randul (`rowReadOf` / `rowPlaceOf`: data, suma, pozitie, rang) si numarul de cosuri. **Fara regula `finish` si fara nicio atingere a ferestrei «Sterge receptie» / «Confirmati stergere receptie?»**: raspunde operatorul. `checkDeletion` (temporizator de 500 ms) doar **citeste** pagina: rand disparut -> `finished`; fereastra a fost deschisa, s-a inchis si randul e inca acolo dupa 3 s -> `cancelled` («Nu»); 60 s fara rezultat -> `cancelled`. In `checkConfirm` (auto-«Da» din 0097-02, care raspunde ferestrelor cu «sigur») exista o **excludere explicita** a ferestrei de stergere: K-BOT nu apasa NICIODATA «Da» in ea.
- **Shell:** `ForexeOperationKind.ReceptieStergere`, `ParseOperation("receptie-stergere")`; `PastreazaSchimbareaDeData` pastreaza pe disc si faptul «stearsa» (`ObservatieReceptie.Fel = Stearsa`); la o stergere se descarca nodul cu istoric (`Case Else`), fara poze.
- **Livrare:** `IngestFlow.TrimiteObservatiileAsync` apeleaza `SpuneStearsaAsync` pentru fapte `Stearsa`; 409 -> intrebare Da/Nu cu efectul spus.
- **Server:** `POST /api/forexe/receptii/stearsa` (`receptii_observate.py`): aceeasi alegere stricta a receptiei ca la 0076-01, apoi `Sters = 1, StersObservatLa = NOW(), StersObservatDe = <utilizator>`. Idempotent.
- **Legarea randului «Stergere receptie»:** `prelucrare_asociere.pas4c_automat` sugereaza instantaneul de stergere pe receptia marcata cu aceeasi suma, cand perechea e 1-la-1; scutit de pragul F37 de 31 de zile.
- **Blocajul fata de asociere:** cele doua `UPDATE ... Sters = 0` (`asociere.py`, `prelucrare_asociere.py`, `_R_DEMARCHEAZA_SQL`) au `AND StersObservatLa IS NULL`. Grep pe `routes`: nu exista alta cale care sa puna `FX_Receptii_R.Sters = 0`.
- **DDL:** `sql/0076_02_sursa.sql` (`StersObservatLa`, `StersObservatDe` pe `FX_Receptii_R`, pe `AVACONT_SURSA`, apoi sincronizare pe toate bazele).

**Greseala de pe parcurs (06.11.2026):** o versiune anterioara a acestei felii facea ca scriptul sa apese singur «Da» in fereastra de stergere. **Retras** — operatorul nu a cerut asa si e CRITIC sa nu se faca; nu mai exista in cod, iar `checkConfirm` o exclude explicit. Detectia dupa numarul de cosuri a fost retrasa un moment si apoi **pastrata ca provizorie**, la cererea operatorului.

## 2. Fisiere atinse

`ForexeWatch.js` (comentariu + excludere in `checkConfirm`); `ForexeWatchEvent.vb`; `ObservatieReceptie.vb` (enum `FelObservatieReceptie`), `ObservatiiReceptiiStore.vb`; `IReceptiiObservateApi.vb`, `ApiClient.ReceptiiObservate.vb`; `KbotForm.ForexeWatch.vb`, `IngestFlow.vb`; `receptii_observate.py`, `prelucrare_asociere.py`, `asociere.py`; `sql/0076_02_sursa.sql` (nou).

## 3. Verificare

`dotnet build src\KBot.App\KBot.App.vbproj -c Debug`: fara erori de compilator la ultimul build (inainte de retragerea auto-«Da»; dupa ea s-a schimbat doar JS). `node --check` pe `ForexeWatch.js`: OK. `py_compile`: OK. **Nimic rulat**: nici K-BOT, nici browser, nici MariaDB, nicio suita.

## 4. Nerezolvat

- **FINALIZAREA DETECTIEI DIN BROWSER**: dupa ce operatorul are mesajul de succes al stergerii (markup + text), `checkDeletion` (numararea cosurilor) se inlocuieste cu mesajul ca semn al sfarsitului; `finished` ramane cu `{rowDate, rowSuma, rowPozitie, rowRang}`. Pana atunci stergerea e recunoscuta provizoriu dupa cosuri; nefiabil (de ex. un rand care dispare din alt motiv, sau o lista redesenata cu alt numar de randuri, ar putea marca gresit o recepție ștearsă — de urmarit la prima proba).
- **Ordinea pe server cand se pune:** DDL pe `AVACONT_SURSA` -> sincronizare pe toate bazele -> fisierele Python. `pas4c_automat` si cele doua `_R_DEMARCHEAZA_SQL` citesc `StersObservatLa`; pe o baza fara coloana, **orice ingest cade**. (Acest lucru se aplica deja pentru fisierele Python ale acestei felii, chiar daca partea de browser e inerta.)
- Semn in tooltip-ul din vederea Receptii («stearsa pe site, astept istoricul»): nefacut.
- Markup-ul cosului si al ferestrei vine din pagina reala (copiat de operator); neprobata este doar comportarea scriptului pe ea.
- Ajutor: nimic adaugat pentru stergere (K-BOT nu schimba nimic din ce vede operatorul in fereastra de confirmare; efectul — marcarea — e fara fereastra noua). De reevaluat la finalizare. `Check-Help.ps1` nerulat. Fundament: F21 completat.
- Nimic pus pe server; `Register-DeploymentTask.ps1` nerulat.
