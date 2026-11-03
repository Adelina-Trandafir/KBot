# SLICE-0093-03 - Parteneri: cautare dupa cod fiscal; salvarea actualizeaza tot furnizorul (03.11.2026)

Operator, 03.11.2026: (1) in copacul din «Parteneri» sa se poata cauta si dupa codul fiscal; (2) dupa salvarea unui partener, ce s-a salvat sa se vada actualizat si in K-BOT - "pare ca ramane cu datele vechi".

## What changed and why
1. **Cautare dupa cod fiscal.** `TreeItem` are campul nou `SearchExtra` (text in care cauta si cautarea, desi randul nu-l arata; gol = nimic in plus, deci nimic nu se schimba la ceilalti arbori); `MatchesSearch` il adauga dupa caption. Fereastra il umple cu `CodFiscal` asa cum e scris + cifrele goale (`RO 21015411` si `21015411`). Cautarea ramane de la 3 caractere. Textul-indiciu al casetei spune acum si «codul fiscal».
2. **Datele vechi.** Cauza gasita in cod (nu vazuta pe ecran): fereastra arata UN rand pe cod fiscal, dar acelasi furnizor poate avea randuri in mai multe unitati, iar restul K-BOT-ului grupeaza dupa `CodFiscal` peste toate randurile (ex. combo-ul de parteneri din editorul DDF: `GROUP BY CodFiscal`, `MIN(DenumirePartener)`). Salvarea scria doar randul afisat, deci celelalte pastrau numele vechi si acela revenea. Acum `propagate_to_same_cf` (`parteneri_edit.py`) scrie, la salvarea unui partener existent, in TOATE celelalte randuri cu acelasi cod fiscal normalizat (`RO 123` = `123`): denumirea si adresa mereu; IBAN si banca doar daca cele salvate nu sunt goale. Raman ale fiecarui rand: `CodPartener`, `Tip`, `Ascuns`, textul codului fiscal. Aceeasi propagare (denumire + adresa) se face si la «Sincronizeaza ANAF». Ajutorul spune asta intr-o propozitie.

## Decizii mele (de confirmat)
- Daca "datele vechi" vazute de operator sunt altele (ex. o fereastra deja deschisa care isi citeste lista doar la deschidere, ca editorul DDF), propagarea nu le schimba: ele se vad actualizate la redeschidere. Nu exista un mecanism de anuntare intre ferestre.
- IBAN/banca se propaga doar cand nu sunt goale (altfel o salvare cu IBAN gol ar sterge IBAN-ul celorlalte unitati).
- La partenerii dublati vechi din aceeasi unitate (acelasi cod fiscal, alt `CodPartener`) se aplica la fel.

## Files touched
- `PYTHON/routes/forexe/parteneri_edit.py` (`propagate_to_same_cf`, apelata la salvare), `PYTHON/routes/forexe/parteneri_anaf_sync.py` (apel + docstring).
- `src/KBot.Controls/Tree/AdvancedTreeControl.TreeItem.vb` (`SearchExtra`), `AdvancedTreeControl.Search.vb` (`MatchesSearch`).
- `src/KBot.App/Views/Nomenclatoare/ParteneriForm.vb` (`BuildTree`: `SearchExtra`), `ParteneriForm.Designer.vb` (textul-indiciu).
- Ajutor `0000-148`: `src/KBot.App/HelpContent/contabil/nomenclatoare/parteneri.md` (lupa cauta si dupa codul fiscal; propozitie despre salvare); notele de versiune 1.1.5.0 in `docs/release-notes/NOUTATI.md`.

## Test results
Fara teste (regula operatorului). `dotnet build src\KBot.App` (Debug): 0 avertismente, 0 erori. `py_compile` pe cele doua fisiere Python: OK.

## Unverified / deferred
- Nimic rulat: nici cautarea pe ecran, nici UPDATE-ul pe server (conditia `REGEXP` din `fiscal_sql` pe o baza reala); **server nedeployat** (`parteneri_edit.py`, `parteneri_anaf_sync.py` trebuie puse de mana).
- Cauza "datelor vechi" e deductie din cod, nu observata; vezi prima decizie de mai sus.
- Datele deja ramase vechi in celelalte randuri se corecteaza la urmatoarea salvare (sau sincronizare ANAF) a furnizorului respectiv; nu s-a scris un script care sa le uniformizeze acum.
