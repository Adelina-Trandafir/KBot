# SLICE 0068 — Ancora pe id-ul de istoric (F34) și rândul de istoric consumat o singură dată (F33)

**Data:** 18.09.2026
**Cererea operatorului:** «something is happening and not all receptiiH are getting associated
with receptiiR on a new download … it did the 25.08.2026 receptie but it didn't do the
27.07.2026 … if i manually open the asociere form, it settles the problem automatically. what's
going on?» — apoi «go ahead with both changes».

## Ce s-a găsit

Descărcarea `AAB2KPRT2EB` (workflow `PrelucrareCompletaReverse`) a salvat cu avertismentul
«1 instantanee neasociate nu au rândul lor de istoric în această descărcare și nu pot fi rezolvate
acum». Două cauze, una peste alta:

1. **Ancora F24 nu acoperă rândurile vechi sub fluxul REVERSE.** `citeste_instantanee` punea un
   instantaneu neașezat în tabloul de decis DOAR dacă `IDH`-ul lui era în `TabelIstoric` din
   sarcina utilă (indicele rândului e ancora deciziei). Dar `adlop - Prelucrare Completa
   Reverse.wfl` merge înapoi prin paginile de istoric și se oprește la `DATA_IESIRE`
   (`exitIfCellEquals`, `startFromLast` în `WorkflowExecutor.Actions.ScrapeTable.vb`), deci
   `TabelIstoric` e o DIFERENȚĂ, nu istoricul întreg — presupunerea din docstring-ul pasului 3a
   («FOREXE trimite istoricul intreg la fiecare descarcare») era adevărată doar la prima
   descărcare. Orice instantaneu rămas neașezat dintr-o rulare mai veche era, prin construcție,
   de nerezolvat din descărcare; editorul de oricând îl vedea fiindcă ancorează pe `IDRH`.
2. **Un rând de istoric cu `Prelucrat = 0` care nu a fost inserat de rularea curentă nu era
   marcat niciodată.** Interogarea operatorului a arătat rândul `IDH 5786` (recepția 27.07.2026)
   cu `Prelucrat = 0`, restul 1 — venit pe altă cale (migrare). Pasul 4a îl citea la fiecare
   descărcare și îi năștea un `FX_Receptii_H` nou, neașezat (`FX_Receptii_H: 2` în mesaj, cu un
   singur antet în payload), iar cauza 1 îl făcea de neașezat din descărcare.

## Ce s-a schimbat și de ce

### Server (`PYTHON/routes/forexe/`)

- **`prelucrare_pasi.py` — F33.** `step4a_populeaza_receptii` întoarce acum
  `(antete, ids_consumate)`: nu mai scrie un antet al cărui `IDH` are deja `FX_Receptii_H`
  (`_H_EXISTA_IDH_SQL`; liniile din tampon se aruncă, nu se toarnă a doua oară) și raportează
  fiecare rând consumat (antete scrise, antete sărite F32/F33, liniile turnate sub un antet).
  Liniile rămase fără antet la sfârșit NU sunt consumate. `step7_actualizeaza_rezolvat` primește
  `ids_noi ∪ ids_consumate` (`prelucrare.py`). Docstring-ul pasului 3a corectat.
- **`prelucrare_asociere.py` — F34.** `ancora(x)` / `ancora_text(a)`: numele unui instantaneu e
  `("rand", indice)` când rândul de istoric e în payload, altfel `("idh", FX_Istoric.ID)`.
  `citeste_instantanee` nu mai lasă afară rândurile fără indice — le pune cu
  `rand_istoric = None` și `idh`; rămân afară (numărate, avertizate) doar cele fără `IDH` și
  dublurile pe același `IDH` (resturi F33). `normalizeaza_decizii` cere exact una dintre
  `rand_istoric` și `idh` (`null` = absent). `verifica_acoperirea`, `materializeaza_reconstituite`,
  `aplica_decizii` cheiază pe ancoră; jurnalul arată «rândul 3» / «istoric 5786». Membrii de lanț
  deja legați nu mai poartă `rand_istoric = -1`, ci `None`. `MOTIV_FARA_ISTORIC` rescris pentru
  cele două cazuri rămase.
- **`prelucrare.py`.** Propunerea trimite pe fiecare instantaneu `rand_istoric` (sau `null`),
  `idh` și `idrh` (id-ul fazei întâi — DOAR cheia dicționarelor formularului, nu se trimite
  înapoi).
- Editorul de oricând (`asociere.py`) NU s-a schimbat: aliasul `rand_istoric = idrh` dă aceeași
  ancoră pe ambele laturi.

### Client

- **`KBot.Domain/AsociereInfo.vb`.** `AncoraAsociere.Cheie/Text`; `InstantaneuPropus.RandIstoric`
  devine `Integer?`, plus `Idh`, `Idrh`, `Ancora()`; `DecizieAsociere.RandIstoric` devine
  `Integer?`, plus `Idh As Integer?`, `Ancora()`; `AsociereDosar.EsteComplet` compară ancore.
- **`KBot.Domain/AsociereStare.vb`.** `InstantaneuLegat.RandIstoric As Integer?`.
  `DinPropunere`: cheia rândurilor de decis e `IDRH`-ul propunerii (pozitiv, unic), contextul
  rămâne `-IDRH`; ancora călătorește separat pe `RandIstoric` / `Idh`. Până acum cheia ERA
  indicele, ceea ce nu lăsa loc pentru un al doilea fel de ancoră.
- **`KBot.Api/UpsertAngajamenteRequest.vb`, `ApiClient.vb`.** DTO-urile poartă
  `rand_istoric?` / `idh?` (decizie) și `rand_istoric?` / `idh?` / `idrh?` (propunere);
  `CatreFir` trimite exact una dintre ele (`_jsonFaraNull` omite pe cea nepusă).
- **`KBot.App/Forexe/AsociereForm.vb`.** `DeciziiDin` citește ancora din
  `inst.RandIstoric` / `inst.Idh`, nu din cheie; mesajul «nu are nicio hotărâre» numește ancora.

### Documente

- `docs/FUNDAMENT_Asociere_Receptii.md`: F33 și F34 adăugate, antetul revizuit.

## Fișiere atinse

`PYTHON/routes/forexe/prelucrare_pasi.py`, `prelucrare_asociere.py`, `prelucrare.py`;
`PYTHON/tests/test_forexe_prelucrare_pasi.py`, `test_forexe_prelucrare_asociere.py`,
`test_forexe_prelucrare_route.py`; `src/KBot.Domain/AsociereInfo.vb`, `AsociereStare.vb`;
`src/KBot.Api/UpsertAngajamenteRequest.vb`, `ApiClient.vb`; `src/KBot.App/Forexe/AsociereForm.vb`;
`tests/KBot.App.Tests/AsociereDeciziiTests.vb`, `tests/KBot.Domain.Tests/AsociereStareDinPropunereTests.vb`,
`tests/KBot.Api.Tests/AsociereApiClientTests.vb`; `docs/FUNDAMENT_Asociere_Receptii.md`;
`docs/worklog/KBOT_STATUS.md`; acest fișier.

## Teste

- **Scrise, nu rulate** (regula casei: nicio suită nu se rulează și nu se construiește).
  Python: 3 teste noi pe 4a (F33: antet cu instantaneu existent, tamponul nu se scurge în antetul
  următor, liniile fără antet nu se consumă) + cele existente adaptate la tuplul întors; 8 teste
  noi pe ancoră (`ancora`, forma deciziei, acoperire prin `idh`, `citeste_instantanee` pe cazul
  real 5786/6253, dubluri, fără IDH). VB: `AsociereDeciziiTests` (+2), `AsociereStareDinPropunereTests`
  (cheie ≠ ancoră, +1), `AsociereApiClientTests` (+1, decizia a 5-a prin `idh`).
- `dotnet build src\KBot.App\KBot.App.vbproj -c Debug`: **0 erori, 0 avertismente BC** (doar
  cele 7 `MSB3825` preexistente). Modulele Python s-au importat și `ancora` /
  `normalizeaza_decizii` / `verifica_acoperirea` s-au exercitat de mână pe cazul 5786.

## Neverificat / amânat

- Proiectele de test VB nu s-au construit (regulă); modificările din ele sunt mecanice
  (`.Value`, câmpuri noi în obiecte), dar necompilate aici.
- Nimic rulat cap la cap și nimic pe ecran. Prima descărcare reală după felia asta ar trebui
  să arate, pe `AAB2KPRT2EB`: `FX_Receptii_H: 0` (antetul 5786 sărit F33), rândul 5786 marcat
  `Prelucrat = 1`, și niciun avertisment despre istoric.
- **Dublurile deja născute** înainte de felie (două `FX_Receptii_H` pe același `IDH`, dacă
  există în alte angajamente) rămân în bază; ingestia le lasă afară cu avertisment, editorul le
  vede. O curățare ar trebui să atingă `FX_ORD.IDRH` — nu s-a scris.
- Un dosar de asociere (`AsociereStore`) salvat cu forma veche a propunerii (fără `idrh`/`idh`)
  ar da chei 0 la `DinPropunere`; amprenta îl respinge oricum la salvare.
- Liniile de recepție inserate în rularea curentă fără antet sunt în continuare marcate
  `Prelucrat = 1` prin `ids_noi` (purtare veche, contrazisă de comentariul din 4a); nu s-a atins,
  fiindcă REVERSE nu desparte niciodată o secundă.
