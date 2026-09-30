# Reanalizarea rezervarilor unui angajament (fara slice, cererea operatorului 30.09.2026)

## Ce s-a schimbat si de ce
Unele randuri au ajuns gresit din Access in noul sistem: o rezervare nu si-a recunoscut la
vremea ei rezervarile anterioare, deci valoarea ei curenta nu tine cont de cele vechi.
Metoda noua reia din `FX_Istoric` (fara redescarcare din FOREXE) lantul unui angajament:
- `Rez_Ord` (ca la pasul 3a) si `TipRand` / `Val_Rezervare_Ant` / `Val_Rezervare_Dif`
  (`_calculeaza_val_rezervare_dif`, pasul 3b) sunt recalculate de la zero, in ordinea
  (zi, Rez_Ord, ID);
- `FX_Rezervari.R_Anterioara` / `R_Valoare` (+ `EMarire` / `EMicsorare` dupa semn) iau valorile
  noi, dupa `IDH`, doar pe randurile care nu sunt initiale.
- Nu insereaza/sterge rezervari, nu atinge `IDREV`/`AreDDF`; o schimbare de tip intre
  initiala/definitiva si influenta se SEMNALEAZA, nu se rescrie.
- Proba (`aplica=false`) apoi confirmare, apoi scriere; aceeasi plimbare in ambele moduri.
- In K-BOT: iconita din stanga subsolului arborelui Rezervari are mereu intrarea
  «Reanalizeaza rezervarile» (pe langa optiunea DDF, cand exista una).

## Fisiere
- NOU `PYTHON/routes/forexe/rezervari_reanaliza.py` (+ import in `routes/forexe/__init__.py`)
- NOU `src/KBot.Domain/RezervariReanalizaResult.vb`
- `src/KBot.Domain/DdfSending.vb` (`RezervariMenuOption.Reanalizeaza`, eticheta)
- `src/KBot.Api/IApiClient.vb`, `ApiClient.vb`, `UpsertAngajamenteRequest.vb` (DTO-uri)
- `src/KBot.App/Views/RezervariView.vb` (icoana mereu vizibila, intrarea in meniu)
- `src/KBot.App/KbotForm.Ddf.vb` (`ReanalizeazaRezervariAsync`)
- 9 fake-uri `IApiClient` din `tests/KBot.App.Tests` (doar stub-ul, ca sa compileze)

## Teste / verificare
- `rezervari_reanaliza.py` parsat cu `.venv`. Nimic rulat pe MariaDB, nimic pe ecran, teste nerulate.
- `KBot.Api` / `KBot.Domain` compileaza. `KBot.App` nu se termina: erori in `Forexe/RobotQueue.vb`
  si `RobotQueueForm` lipsa (lucru necomis, felia 0098 a altcuiva); fisierele atinse aici nu dau erori.

## Neverificat / amanat
- Ca ordinea crescatoare a `ID` reproduce ordinea FOREXE la randurile migrate din Access.
- Reluarea intr-o singura bucata (ingestia reseteaza steagurile initial/definitivare la fiecare descarcare).
- Fara numar de felie: de atribuit. Ajutorul (`contabil/vederi/rezervari.md`, tabelul «Iconitele arborelui»,
  randul «subsol, stanga») trebuie completat cu intrarea noua.
