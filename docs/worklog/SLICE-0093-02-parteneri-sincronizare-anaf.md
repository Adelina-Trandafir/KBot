# SLICE-0093-02 - Parteneri: «Selecteaza Parteneri Sincronizare Anaf» / «Sincronizeaza ANAF» (03.11.2026)

Operator, 03.11.2026, fereastra «Parteneri». Numar de felie: primul sub-numar liber al feliei 0093 (Parteneri), dupa 0093-01.

## What changed and why
Doua butoane noi in grupul de ribbon al formularului (cel din dreapta, ca «Preia date din Access»), vizibile doar cat «Parteneri» e pe ecran:

1. **«Selectează Parteneri Sincronizare Anaf»** - lista arata TOTI partenerii (si ascunsii, si cei fara activitate) si fiecare rand primeste o casuta de bifat. Butonul dispare, apare **«Sincronizează ANAF»**. Daca fereastra are modificari nesalvate se intreaba ca la schimbarea partenerului.
2. In acest mod meniul de la iconita din capul listei are alte trei randuri (cele obisnuite revin la sfarsit): **«Selectează DOAR partenerii folosiți»** (`Activ` = apar pe DDF/ORD), **«Selectează TOȚI partenerii»**, **«Selectează TOȚI partenerii NESINCRONIZAȚI»**. Fiecare ÎNLOCUIESTE bifele.
3. **«Sincronizează ANAF»** - fara nicio bifa nu face nimic (doar un rand in linia de stare). Altfel cere confirmare, apoi pentru fiecare partener bifat (in ordinea listei) apeleaza `POST /api/forexe/nomenclatoare/parteneri/<id>/anaf-sync`: serverul cere de la ANAF CUI-ul, scrie `DenumirePartener` si `Adresa` (adresa ANAF goala nu o sterge pe cea veche) si marcheaza partenerul sincronizat. La sfarsit lista se reincarca si totul revine la normal; un mesaj da cifrele (sincronizati / cu date schimbate / negasiti / fara denumire). Daca ANAF nu raspunde sau apelul esueaza, sincronizarea se opreste, cei prelucrati se debifeaza, ceilalti raman bifati, modul ramane (se reia apasand din nou).

Regula codului (operator): doar cod fiscal NUMERIC cu lungime > 5 si < 13 (6-12 cifre; «RO» la inceput si spatiile se ignora). Restul (CNP-uri, text) au casuta gri (`CheckLocked`) si nu sunt luate de nicio alegere rapida; aceeasi regula e si pe server (`sync_cf`), care raspunde `cod_invalid` fara sa intrebe ANAF.

«Nesincronizat»: tabela NOUA `Parteneri_Anaf` (`IdPartener` PK + FK ON DELETE CASCADE, `DataSincronizare`, `CreatedAt`, `UpdatedAt`) - `sql/0093_02_Parteneri_Anaf.sql`. Un partener fara rand acolo = nesincronizat. Se scrie un rand doar cand ANAF a recunoscut codul; negasit / fara denumire raman nesincronizate. `GET /parteneri` trimite `anaf_sincronizat` (data sau null); cat timp tabela nu exista, citirea ei e sarita (toti «nesincronizati») ca lista sa mearga.

## Decizii mele (de confirmat)
- Se actualizeaza DOAR randul `Parteneri` cerut (fereastra arata un rand pe cod fiscal), nu si ceilalti cu acelasi cod din alte unitati.
- Marcajul «sincronizat» e in tabela noua (nu o coloana noua in `Parteneri`: tabelele vechi nu se ating).
- «Folositi» = `Activ` (apare pe un DDF/ORD), cum e lista implicita.
- O cerere pe partener, secvential, cu minim 1,1 s intre cereri pe server (limita ANAF de 1 cerere/s e cifra uzuala, NEVERIFICATA aici); 100 de parteneri ~ 2 minute. Nu s-a folosit cererea in lot a ANAF (pana la 100 CUI) - ar fi mai rapida, e o decizie noua.
- Textul butoanelor are diacritici (sunt vazute de operator): «Selectează Parteneri Sincronizare Anaf», «Sincronizează ANAF».
- Nu exista o iesire din modul de alegere fara sincronizare (cererea nu o cere); inchiderea ferestrei il reseteaza.

## Files touched
- Server: `PYTHON/routes/forexe/parteneri_anaf_sync.py` (nou), `PYTHON/routes/forexe/parteneri_edit.py` (`anaf_sincronizat`), `PYTHON/routes/forexe/__init__.py` (import), `PYTHON/routes/actiuni.py` (eticheta din jurnal), `sql/0093_02_Parteneri_Anaf.sql` (nou).
- `src/KBot.Domain/Nomenclatoare.vb` (`Partener.AnafSyncedAt`, `PartenerAnafSync`, `PartenerAnafSyncResult`), `src/KBot.Api/INomenclatoareApi.vb` + `ApiClient.Nomenclatoare.vb` (`SyncPartenerWithAnafAsync`, `anaf_sincronizat`).
- `src/KBot.App/Views/Nomenclatoare/ParteneriForm.Anaf.vb` (nou, clasa partial), `ParteneriForm.vb` (`IsListed`, `BuildTree`, meniul capului, `Tree_NodeMouseUp`).
- `src/KBot.App/AvacontMain.Designer.vb` (butoanele `parteneri_anaf_selecteaza`, `parteneri_anaf_sincronizeaza`; imaginea refolosita de la «Sincronizeaza facturi primite»), `AvacontMain.vb` (clic, vizibilitate in `UpdateFormGroup`, evenimentul `AnafModeChanged`), `KbotForm.Nomenclatoare.vb` (`RunParteneriAnaf`).
- Ajutor: vezi `SLICE-0000-147-ajutor-sincronizare-anaf-parteneri.md`.

## Test results
La cererea operatorului (regula «fara teste»): niciun test scris sau rulat. `dotnet build src\KBot.App` (Debug): 0 avertismente, 0 erori. `py_compile` pe cele 4 fisiere Python atinse: OK.

## Unverified / deferred
- **Nimic rulat**: nici butoanele pe ecran (imaginea e refolosita; nu s-a vazut cum arata doua butoane cu text lung in grup), nici casutele din lista (starea `CheckLocked` desenata gri, tooltipul cu data), nici o cerere reala la ANAF.
- **Server nedeployat** si **tabela `Parteneri_Anaf` necreata**: de rulat `sql/0093_02_Parteneri_Anaf.sql` pe fiecare baza (prin aplicatia admin) si de pus manual ruta noua; pana atunci «Sincronizeaza ANAF» primeste 404 / 500 cu mesaj, iar lista merge normal.
- Limita de cereri a ANAF (v9) neverificata; `PYTHON/routes/inregistrare/anaf.py` o declara si el neverificata.
- Un clic pe casuta nu mai incarca partenerul (steag `_anafTickJustNow`); daca eliberarea mouse-ului nu ajunge pe arbore, primul clic pe text dupa aceea e inghitit o data.
- Ajutor scris; captura `parteneri-anaf` o face operatorul.
