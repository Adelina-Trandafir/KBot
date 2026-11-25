---
name: form
description: Construieste sau modifica o macheta VERCON online (formular, lista cu "+", adaugare/editare pe loc) dupa conventiile fixe ale Adelinei - butoane custom, "+" in antet, lista dezactivata la adaugare, fara mesaje, Enter la urmatoarea caseta - fara sa le mai ceara de fiecare data. Argumentul descrie doar ce este specific machetei.
---

# `$form` — macheta VERCON dupa conventiile Adelinei

`$form <ce se cere>` = cererea e o macheta noua sau o schimbare de macheta in VERCON online (`PYTHON/static/js/vercon`, `PYTHON/static/css`, `PYTHON/routes/vercon`). Conventiile de mai jos se aplica IMPLICIT; Adelina descrie doar ce difera (campuri, sursa datelor, drepturi). Nu le mai intreba, nu le reinterpreta. Daca ceva din cerere le contrazice explicit, cererea ei castiga pentru acel punct.

**Inainte de orice cod:** slice-ul il decide Adelina (vezi memoria «ANY change needs a slice FROM ADELINA»). Daca nu l-a dat, intreaba si nu edita nimic. Nu rula teste, nu te conecta la server, nu face deploy, nu scrie in Git. `$ssh` doar la cererea ei.

## 1. Structura
- **Nu pagina separata si nu fereastra popup.** O functie noua traieste in macheta existenta a subiectului (ex. adaugarea de furnizor e in «Furnizori (Parteneri)»). Fara rand nou de meniu, fara fereastra de dialog, daca nu cere ea explicit.
- Nu adauga butoane, coloane sau casete pe care schita ei nu le are. Nu adauga butoane de stergere pe rand (la nevoie: coloana de stergere in DGV).
- Daca exista deja o macheta asemanatoare (BC, FCS, Furnizori), citeste-o si copiaz-o ca model inainte sa inventezi ceva.

## 2. Controale (doar cele comune, nu HTML/stil propriu)
- Buton «+» : `ActionAddButton` din `components/action-add-button/action-add-button.js`, pus in antetul formularului, langa titlu (la arbori: in antetul arborelui, ca in celelalte machete). Vizibil numai cu dreptul respectiv.
- Butoane de formular: `DialogButton` + `dialogButtonRow` din `components/dialog-button/dialog-button.js`. **Renunta = `kind: 'cancel'`, STANGA. Salveaza = `kind: 'ok1'`, DREAPTA.** Nu `<button>` simplu, nu `vc-actions`, nu CSS propriu pentru butoane.
- Casete: `TextBox` etc. din `ui.js`; liste cu tastare: `ComboTree` / combobox cu tastare din `components/`; liste de date: DataGrid obisnuit (clasic, cu rand gol la adaugare acolo unde se adauga rand, ca in FCS). Nu inventa controale noi cat timp exista unul comun.
- CSS in fisier propriu sub `static/css`, importat din `vercon.css`; culori/stil din temele existente, nimic hardcodat.

## 3. Comportament la «+» (adaugare in aceeasi macheta)
1. Formularul se goleste.
2. Lista (DGV) si cautarea se dezactiveaza (`inert` + aspect disabled).
3. Codul/cheia generata automat se dezactiveaza si i se ofera automat urmatoarea valoare libera din baza de date, recalculata sub blocare la salvare.
4. **Renunta / Salveaza apar DOAR dupa «+»**, sub ultimul camp, aliniate cu marginea de jos a listei. La Renunta sau dupa Salveaza dispar, lista se reactiveaza, noul rand apare selectat.
5. Cheia naturala (ex. Cod fiscal) se introduce prima. La pierderea focusului din ea se face automat interogarea (ANAF sau alta sursa) care completeaza restul campurilor; clientul trimite serverului NUMAI cheia, restul il aduce serverul.
6. Verificarea de duplicat se face inainte de salvare, dupa cheia naturala normalizata, pe **toate unitatile aceluiasi DC**, ascunsi (`Ascuns=1`) incluse. Un rand existent doar in alta subunitate nu e motiv de «exista deja» pentru lista de alegere: listele de alegere (ex. NIR) arata TOATE randurile DC-ului grupate dupa cheia naturala. Un rand ascuns se reactiveaza la Salveaza, nu se adauga altul. Nu se ating coduri de angajament, solduri sau alte date contabile.

## 4. Fara mesaje
- **Niciun mesaj** la adaugare, interogare sau salvare reusita: nu bara de stare, nu casete, nu text.
- Un refuz (exista deja, necunoscut, eroare de retea) apare ca **stare de eroare pe campul vinovat** (`setError`), nu ca mesaj separat. Mesaje separate doar daca Adelina le cere.
- Cat ruleaza o interogare sau un salvat, **toate casetele sunt dezactivate** (`setEnabled(false)`), apoi reactivate in `finally`; un numar de secventa ignora raspunsurile vechi.

## 5. Tastatura
- **Enter in casete trece la urmatoarea caseta logica**, in ordinea formularului, sarind peste cele dezactivate; dupa ultima ajunge pe Salveaza (cat timp se adauga). In caseta cheie, Enter in modul de adaugare ruleaza intai interogarea, apoi trece mai departe sau ramane in caseta daca a fost refuzata.
- Shift/Ctrl/Alt+Enter nu se intercepteaza.

## 6. Server (daca machete scrie date)
- Rute sub `routes/vercon`, `@guard('<operatie>')`, scrise cu `scoped(...)` (verifica contextul si blocarea), operatia in `operations.py`, dreptul acordat in `__init__.py`. Audit pentru fiecare scriere.
- Tabele noi: `CreatedAt` + `UpdatedAt` (cu `ON UPDATE`); MariaDB `utf8mb3_general_ci`.
- Text pentru operator cu diacritice; **restul codului (nume, chei JSON, comentarii) FARA diacritice** si in engleza (REGULA 0).
- `Provenienta` / coloanele de sursa se completeaza ca la modelul existent.

## 7. Dupa implementare
- Verificare doar de sintaxa (`py_compile`, `node --check`, `JSON.parse`). Spune clar ce NU a fost rulat in aplicatie.
- Ajutorul: sectiune noua in `PYTHON/static/help/vercon.json` cu `slice`, doar ce vede si face utilizatorul. Worklog `docs/worklog/SLICE-<...>.md`, `VERCON/VERCON_STATUS.md`, `implementare_vercon_v2.md`. Cand elementul e gata, taie din ele pasii intermediari, pastreaza erorile si ce e neverificat.
- Registrul de deployment: numai la `$ssh`, dupa proba ei.
- Raportul catre ea: cuvinte simple, fara jargon.
