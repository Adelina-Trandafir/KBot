# SLICE 0054 — cutia neagră a descărcătorului FOREXE

## De ce

Operatorul a cerut «reîmprospătare» pe un angajament. Robotul FOREXE **a terminat cu bine**,
dar în tabele nu a apărut nimic, iar jurnalul serverului din acea rulare arată **numai citiri**:

```
[forexe.tree]      000_DEMO: an=2026 ss=02A -> 31 randuri
[forexe.sumar]     000_DEMO: cod=AAB2MAACHXB -> 1 randuri
[forexe.rezervari] 000_DEMO: cod=AAB2MAACHXB -> 15 randuri
[forexe.receptii]  000_DEMO: cod=AAB2MAACHXB -> 17 randuri, 20 plati
[forexe.plati]     000_DEMO: cod=AAB2MAACHXB -> 20 randuri
```

Niciun `POST /api/forexe/prelucrare`. Nu e o eroare de rețea și nu e o tranzacție întoarsă din
drum: **cererea nu a plecat niciodată.**

### Cauza, verificată în cod

`MainForm.Tree_RightIconClicked` (`src/KBot.App/KBOT.vb:1339`) cheamă
`ForexeController.DownloadNodeAsync`, iar aceasta se oprește exact după salvarea locală:

```
Dim pachet = WorkflowResultStore.DinJobResult(cod, rezultat)
Dim cale   = _store.SalveazaNod(cod, pachet)     ' <WorkflowResults>\PrelucrareCompleta_<cod>_<stamp>.json
Return pachet                                     ' ... și atât
```

Treapta care scrie pe server există întreagă, dar **nu o cheamă nimeni**:

| Piesă | Unde | Stare |
|---|---|---|
| Ruta | `PYTHON/routes/forexe/prelucrare.py:477` (`POST /api/forexe/prelucrare`) | scrisă |
| Clientul | `ApiClient.TrimitePrelucrareAsync` / `CerePropunereAsync` | scris |
| Coordonatorul | `PrelucrareCoordinator.TrimiteAsync` / `CerePropunereAsync` | scris |
| **Apelantul** | — | **nu există** |

`grep` peste `src/` pe `TrimiteAsync` / `CerePropunereAsync` / `PrelucrareCoordinator` întoarce
**numai teste** în afara claselor de mai sus. Chiar și comentariul clasei o spune:
«ATENȚIE — în felia 0048-02 NIMIC nu cheamă încă această clasă… Legarea în
`ForexeController.DownloadNodeAsync` (§9 din plan) vine odată cu restul pașilor.»

Deci: **descărcarea nu e stricată, e neterminată.** Pachetul ajunge pe disc și acolo rămâne.
Legarea propriu-zisă (faza «propunere» → `AsociereForm` → faza «salvare») e restul feliei
0048-04 și **nu** face parte din felia asta.

## Ce s-a schimbat

Felia 0054 nu leagă ingestia. Face lucrul care lipsea ca să se poată **vedea de la distanță ce
a adus robotul**: o cutie neagră a descărcătorului, fiindcă rularea s-a făcut pe alt calculator
(cel cu tokenul și cu FOREXE), iar aici nu se poate reproduce nimic.

1. **`ForexeRunDump` (clasă nouă)** — un folder per încercare de descărcare, sub
   `<WorkflowResults>\Runs\<stamp>_<operatie>_<cod>\`:

   | Fișier | Ce ține |
   |---|---|
   | `run.json` | ce s-a cerut, în ce context (`DbName`, an, SS), cu ce workflow și parametri, cum s-a terminat, câte rânduri pe fiecare tabel, ce chei scalare au venit |
   | `raw.json` | variabilele executorului **brute**, exact cum le-a lăsat robotul |
   | `tables.json` | aceleași variabile după ce runner-ul le-a rupt în tabele |
   | `mapped.json` | forma mapată, când apelantul a ajuns până acolo |
   | `forexe.log` | jurnalul întreg al lucrării, din `JobHistoryManager` |

   Se scrie **oricum s-ar termina**: reușit, workflow eșuat, tabel lipsă, ocupat, fără sesiune,
   excepție. Asta e toată ideea — `WorkflowResultStore` scrie doar calea fericită, deci exact
   rularea care n-a adus nimic era și cea care nu lăsa nimic în urmă.

2. **`ForexeController.DownloadNodeAsync` / `DownloadListaAsync`** — deschid jurnalul înainte de
   orice ieșire și îl scriu pe fiecare ramură. Cele două ieșiri mute de dinainte
   (`If _busy Then Return Nothing`, sesiune nedeschisă) **spun acum de ce** pe consolă, în loc
   să pară că nu s-a apăsat nimic.

3. **Calea folderului se anunță pe consolă** după fiecare descărcare (`ScrieJurnal`), tocmai ca
   operatorul să nu fie nevoit să-l caute. Dacă scrierea jurnalului eșuează, se spune și asta —
   un jurnal lipsă nu are voie să treacă neobservat.

4. **Descărcarea unui nod spune acum negru pe alb unde se oprește robotul:** «descărcarea s-a
   încheiat — pachetul e local; urmează ingestia». Un flux care raportează numai succes, când de
   fapt s-a oprit la jumătatea drumului, e mai rău decât unul care eșuează. *(În felia asta
   linia suna «scrierea pe server nu e încă legată de acest buton», fiindcă atunci chiar nu era.
   Felia 0055 a legat-o și a rescris linia.)*

Politica de erori a cutiei negre: `Save` loghează și întoarce `Nothing` în loc să arunce. Un
diagnostic stricat nu are voie să omoare descărcarea pe care o descrie; eșecul nu e înghițit —
merge în `GlobalErrorLog` **și** pe consola operatorului.

## Fișiere atinse

- `src/KBot.App/Forexe/ForexeRunDump.vb` — **nou** (`ForexeRunDump` + POCO-ul `ForexeRunInfo`).
- `src/KBot.App/Forexe/ForexeController.vb` — jurnalul pe toate ramurile celor două descărcări,
  plus ajutorul `ScrieJurnal`.
- `tests/KBot.App.Tests/ForexeRunDumpTests.vb` — **nou**, 5 teste.
- `docs/worklog/KBOT_STATUS.md` — rândul 0054, numărul liber următor, firul deschis.

## Rezultate de test

- `dotnet build src\KBot.App\KBot.App.vbproj -c Debug` → **0 erori**, 7 avertismente, toate
  `MSB3825` **preexistente** pe `.resx`-uri (aceleași 7 pe care le numără și rândul 0053).
- `dotnet test tests\KBot.App.Tests` → **204 trecute / 13 picate / 217 total**. Cele 13 roșii
  stau toate în fișiere neatinse de felia asta (`DdfXfaParserTests`, `XfaXmlPreviewTests`,
  `MainFormNavItemsTests`, `DdfViewTests`, `IstoricViewTests`) și au același număr pe care
  rândul 0048-09 îl consemnează drept preexistent. **Linia de plecare NU a fost măsurată în
  această sesiune** — suita nu s-a rulat înainte de schimbare, deci «13 preexistente» e citit
  din STATUS, nu verificat aici.
- `dotnet test tests\KBot.App.Tests --filter ForexeRunDump` → **5 trecute / 0 picate**.
- Nu s-a rulat `dotnet test KBot.sln` (regula casei: bancul DevHarness deschide ferestre reale).

## Ce rămâne neverificat / amânat

- ⚠ **Cutia neagră nu a fost văzută pe un calculator cu FOREXE.** Testele o scriu și o citesc de
  pe disc, dar niciun folder n-a fost produs de un robot adevărat — rularea care a pornit felia
  s-a făcut pe alt calculator, la care nu există acces de aici. Prima rulare reală rămâne de
  făcut acolo, iar folderul `Runs\` de trimis înapoi.
- ✅ **Ingestia a fost legată în felia 0055** (`SLICE-0055-ingestia-legata.md`), imediat după.
  Felia asta doar a făcut lipsa vizibilă și a spus-o pe consolă. Linia care anunța «datele au
  rămas LOCAL» a rămas la locul ei: descărcarea chiar se oprește acolo, iar ingestia pornește
  din shell, pe rândul următor.
- ⚠ **Nota din STATUS despre «`sql/0049` neaplicat» (rândurile 0048-03 / 0048-04) trimite la un
  fișier care nu există.** În `sql/` sunt `0049_ord_att_img.sql` și `0049_receptii_stergere.sql`,
  niciunul al ingestiei; migrarea ingestiei e `sql/0048_alegeri_unitate.sql`. **Dacă e aplicată
  pe server nu se poate spune de aici** — nu s-a atins nicio bază. De verificat înainte de
  legarea ingestiei, altfel prima cerere adevărată va cădea pe o tabelă lipsă.
- Nimic nu curăță `Runs\`. Un folder per descărcare crește la nesfârșit; dacă devine o problemă,
  tăierea celor mai vechi e o felie separată (aceeași decizie ca `MaxHistory` din
  `JobHistoryManager`).
- Cutia neagră **nu** ține și răspunsurile HTTP ale vederilor (sumar / rezervări / recepții /
  plăți). Dacă va fi nevoie și de ele, e o adăugare, nu o rescriere.
