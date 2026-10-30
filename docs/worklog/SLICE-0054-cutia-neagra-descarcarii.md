# SLICE-0054/55 — Istoric al descărcării fără ingestie și al legării ei

Etapele diagnostic → instrumentare → apelant → prima rulare/corecție sunt păstrate integral. „Nu există apelant” este constatarea lui 0054, urmată de 0055; nu este o lipsă declarată pentru toate versiunile ulterioare. Prima rulare din 08.09 cade înainte de HTTP; a doua este consemnată în [0056](SLICE-0056-tabloul-intreg.md#slice-0056-baza). [0059](SLICE-0059-asociere-receptie-noua-din-cos.md#slice-0059) adaugă ulterior gestul UI de reconstituire care lipsea în 0055. Ancora inițială pe indice din 0055 este extinsă ulterior de [0068](SLICE-0062-receptii-neasezate-si-refacere-din-istoric.md#slice-0068). Confirmările operatorului sunt separate de verificările executate în vechea sesiune.

[0054-cutia](#slice-0054) · [0055-ingestia](#slice-0055)

## SLICE 0054 — cutia neagră a descărcătorului FOREXE <a id="slice-0054"></a>

### De ce

Operatorul a cerut «reîmprospătare» pe un angajament. Robotul FOREXE **a terminat cu bine**, dar în tabele nu a apărut nimic, iar jurnalul serverului din acea rulare arată **numai citiri**:

```
[forexe.tree]      000_DEMO: an=2026 ss=02A -> 31 randuri
[forexe.sumar]     000_DEMO: cod=AAB2MAACHXB -> 1 randuri
[forexe.rezervari] 000_DEMO: cod=AAB2MAACHXB -> 15 randuri
[forexe.receptii]  000_DEMO: cod=AAB2MAACHXB -> 17 randuri, 20 plati
[forexe.plati]     000_DEMO: cod=AAB2MAACHXB -> 20 randuri
```

Niciun `POST /api/forexe/prelucrare`. Nu e o eroare de rețea și nu e o tranzacție întoarsă din drum: **cererea nu a plecat niciodată.**

#### Cauza, verificată în cod

`MainForm.Tree_RightIconClicked` (`src/KBot.App/KBOT.vb:1339`) cheamă `ForexeController.DownloadNodeAsync`, iar aceasta se oprește exact după salvarea locală:

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

`grep` peste `src/` pe `TrimiteAsync` / `CerePropunereAsync` / `PrelucrareCoordinator` întoarce **numai teste** în afara claselor de mai sus. Chiar și comentariul clasei o spune: «ATENȚIE — în felia 0048-02 NIMIC nu cheamă încă această clasă… Legarea în `ForexeController.DownloadNodeAsync` (§9 din plan) vine odată cu restul pașilor.»

Deci: **descărcarea nu e stricată, e neterminată.** Pachetul ajunge pe disc și acolo rămâne. Legarea propriu-zisă (faza «propunere» → `AsociereForm` → faza «salvare») e restul feliei 0048-04 și **nu** face parte din felia asta.

### Ce s-a schimbat

Felia 0054 instrumentează descărcarea pentru diagnostic la distanță: rularea era pe calculatorul cu tokenul și FOREXE, inaccesibil de aici. Ingestia rămâne nelegată la această etapă.

1. **`ForexeRunDump` (clasă nouă)** — un folder per încercare de descărcare, sub `<WorkflowResults>\Runs\<stamp>_<operatie>_<cod>\`:

   | Fișier | Ce ține |
   |---|---|
   | `run.json` | ce s-a cerut, în ce context (`DbName`, an, SS), cu ce workflow și parametri, cum s-a terminat, câte rânduri pe fiecare tabel, ce chei scalare au venit |
   | `raw.json` | variabilele executorului **brute**, exact cum le-a lăsat robotul |
   | `tables.json` | aceleași variabile după ce runner-ul le-a rupt în tabele |
   | `mapped.json` | forma mapată, când apelantul a ajuns până acolo |
   | `forexe.log` | jurnalul întreg al lucrării, din `JobHistoryManager` |

   Se scrie **oricum s-ar termina**: reușit, workflow eșuat, tabel lipsă, ocupat, fără sesiune, excepție. Asta e toată ideea — `WorkflowResultStore` scrie doar calea fericită, deci exact rularea care n-a adus nimic era și cea care nu lăsa nimic în urmă.

2. **`ForexeController.DownloadNodeAsync` / `DownloadListaAsync`** — deschid jurnalul înainte de orice ieșire și îl scriu pe fiecare ramură. Cele două ieșiri mute de dinainte (`If _busy Then Return Nothing`, sesiune nedeschisă) **spun acum de ce** pe consolă, în loc să pară că nu s-a apăsat nimic.

3. **Calea folderului se anunță pe consolă** după fiecare descărcare (`ScrieJurnal`), tocmai ca operatorul să nu fie nevoit să-l caute. Dacă scrierea jurnalului eșuează, se spune și asta — un jurnal lipsă nu are voie să treacă neobservat.

4. **Descărcarea unui nod spune acum negru pe alb unde se oprește robotul:** «descărcarea s-a încheiat — pachetul e local; urmează ingestia». Un flux care raportează numai succes, când de fapt s-a oprit la jumătatea drumului, e mai rău decât unul care eșuează. *(În felia asta linia suna «scrierea pe server nu e încă legată de acest buton», fiindcă atunci chiar nu era. Felia 0055 a legat-o și a rescris linia.)*

Politica de erori a cutiei negre: `Save` loghează și întoarce `Nothing` în loc să arunce. Un diagnostic stricat nu are voie să omoare descărcarea pe care o descrie; eșecul nu e înghițit — merge în `GlobalErrorLog` **și** pe consola operatorului.

### Fișiere atinse

`src/KBot.App/Forexe/ForexeRunDump.vb` — **nou** (`ForexeRunDump` + POCO-ul `ForexeRunInfo`). `src/KBot.App/Forexe/ForexeController.vb` — jurnalul pe toate ramurile celor două descărcări, plus ajutorul `ScrieJurnal`. `tests/KBot.App.Tests/ForexeRunDumpTests.vb` — **nou**, 5 teste. `docs/worklog/KBOT_STATUS.md` — rândul 0054, numărul liber următor, firul deschis.

### Rezultate de test

- `dotnet build src\KBot.App\KBot.App.vbproj -c Debug` → **0 erori**, 7 avertismente, toate `MSB3825` **preexistente** pe `.resx`-uri (aceleași 7 pe care le numără și rândul 0053).
- `dotnet test tests\KBot.App.Tests` → **204 trecute / 13 picate / 217 total**. Cele 13 roșii stau toate în fișiere neatinse de felia asta (`DdfXfaParserTests`, `XfaXmlPreviewTests`, `MainFormNavItemsTests`, `DdfViewTests`, `IstoricViewTests`) și au același număr pe care rândul 0048-09 îl consemnează drept preexistent. **Linia de plecare NU a fost măsurată în această sesiune** — suita nu s-a rulat înainte de schimbare, deci «13 preexistente» e citit din STATUS, nu verificat aici.
- `dotnet test tests\KBot.App.Tests --filter ForexeRunDump` → **5 trecute / 0 picate**.
- Nu s-a rulat `dotnet test KBot.sln` (regula casei: bancul DevHarness deschide ferestre reale).

### Ce rămâne neverificat / amânat

- ⚠ **Cutia neagră nu a fost văzută pe un calculator cu FOREXE.** Testele o scriu și o citesc de pe disc, dar niciun folder n-a fost produs de un robot adevărat — rularea care a pornit felia s-a făcut pe alt calculator, la care nu există acces de aici. Prima rulare reală rămâne de făcut acolo, iar folderul `Runs\` de trimis înapoi.
- ✅ **Ingestia a fost legată în felia 0055** ([etapa 0055](#slice-0055)), imediat după. Felia asta doar a făcut lipsa vizibilă și a spus-o pe consolă. Linia care anunța «datele au rămas LOCAL» a rămas la locul ei: descărcarea chiar se oprește acolo, iar ingestia pornește din shell, pe rândul următor.
- ⚠ **Nota din STATUS despre «`sql/0049` neaplicat» (rândurile 0048-03 / 0048-04) trimite la un fișier care nu există.** În `sql/` sunt `0049_ord_att_img.sql` și `0049_receptii_stergere.sql`, niciunul al ingestiei; migrarea ingestiei e `sql/0048_alegeri_unitate.sql`. **Dacă e aplicată pe server nu se poate spune de aici** — nu s-a atins nicio bază. De verificat înainte de legarea ingestiei, altfel prima cerere adevărată va cădea pe o tabelă lipsă.
- Nimic nu curăță `Runs\`. Un folder per descărcare crește la nesfârșit; dacă devine o problemă, tăierea celor mai vechi e o felie separată (aceeași decizie ca `MaxHistory` din `JobHistoryManager`).
- Cutia neagră **nu** ține și răspunsurile HTTP ale vederilor (sumar / rezervări / recepții / plăți). Dacă va fi nevoie și de ele, e o adăugare, nu o rescriere.

---

## SLICE 0055 — ingestia legată de buton: `AsociereForm` devine și editorul propunerii <a id="slice-0055"></a>

### De ce

Felia 0055 leagă apelantul lipsă diagnosticat în [0054](#slice-0054): pachetul local ajunge la conducta de scriere existentă.

Pachetul descărcat pe 07.09.2026 (`AAB2MAACHXB`, trimis de operator) a fost verificat câmp cu câmp împotriva a ceea ce citește conducta din `PYTHON/routes/forexe/`: `TabelIndicatori_results` cu `BugetIndicator` imbricat, `TabelIstoric` cu `Timp` / `Utilizator` / `Descriere` / `Observatii`, `ListaReceptii_results` cu `Detaliu[]`, și scalarii `DescriereAngajament`, `StareAngajament`, `DataAngajament`, `DataInceputDerulare` în forma ZZ/LL/AAAA. **Tot ce cere serverul există în ce aduce robotul** — deci nu lipsea o mapare, lipsea legătura.

### Două constatări care au decis forma feliei

**(1) Fals succes retras:** `ApiClient.TrimitePrelucrareAsync`, prin `PrelucrareCoordinator.TrimiteAsync`, omitea mod. Serverul îl interpreta propunere, rula toți pașii și făcea `conn.rollback()`, dar clientul marca 200 ca `PrelucrareStare.Salvat`. Retrase metoda, declarația IApiClient și TrimiteAsync: implicitul sigur al serverului nu echivala cu salvare.

**(2) Decizie pentru fiecare instantaneu:** `verifica_acoperirea` refuză cu 400 orice lipsă din decizii; automatul așază numai ultimul din lanț (F9), deci restul cer operatorul prin construcție. Deciziile goale/parțiale nu sunt opțiune.

### Ce s-a schimbat

#### 1. Editorul de asociere lucrează acum peste amândouă tablourile

La alegerea explicită a operatorului, `AsociereForm` primește un al doilea constructor și un mod: același editor servește ambele tablouri, evitând două interfețe ale aceleiași decizii.

| | `ModAsociere.Oricand` | `ModAsociere.Propunere` |
|---|---|---|
| de unde vine tabloul | `GET /api/forexe/asociere` | răspunsul fazei întâi |
| ancora unui instantaneu | `FX_Receptii_H.IDRH` | INDICELE rândului în `TabelIstoric` (F24) |
| ce pleacă la salvare | doar ce s-a schimbat (`ComandaAsociere`) | o hotărâre pentru FIECARE rând (`DecizieAsociere`) |
| butonul se aprinde când | s-a schimbat ceva | **nu mai e niciun rând nehotărât** |
| o închidere fără salvare pierde | niște corecturi | **toată descărcarea** |

Funcția pură `AsociereStare.DinPropunere` (`KBot.Domain`, fără fereastră) adaptează propunerea la tabloul existent. Indicele rândului ocupă `Idrh` ca reper stabil pentru dicționarele editorului și revine la salvare ca `rand_istoric`, fără a deveni cheie. Tabloul provine exclusiv din propunere sau din bază; cele două numerotări nu se amestecă.

Sugestia serverului este poziția inițială vizibilă, confirmată prin buton (F18: automatul poate greși, pe lângă a fi incomplet). Faza doi ignoră automatul și aplică numai ce trimite formularul.

#### 2. Butonul de descărcare merge acum până la capăt

`MainForm.Tree_RightIconClicked` → `DuLaIngestieAsync`: descărcare → `PrelucrareCoordinator.CerePropunereAsync` (dialogul de unitate repetat la cererea serverului, până la MaxRunde) → `AsociereForm` propunere → salvare. La succes recitește arborele (Are* pentru istoric/recepții/plăți) și vederea activă.

Pachetul se păstrează/retrimite neschimbat între faze, cu alegerile unității: rand_istoric este indice în payload, iar „nu mă mai întreba” s-a derulat înapoi în tranzacția propunerii.

**Faza doi repetă alegerea unității** prin noul `PrelucrareCoordinator.SalveazaAsync`: `SalveazaAsociereaAsync` poate întoarce Stare = AlegereUnitate dacă o clasificație apărută între faze o cere la rerularea pașilor. Găsit prin recitire înaintea execuției: apelul direct din prima variantă a formularului ar fi tratat răspunsul drept succes, ca TrimiteAsync retras. Coordonatorul păstrează bucla existentă.

#### 3. Mesajele spun ce se pierde

Închiderea propunerii explică pierderea întregii descărcări: rollbackul fazei întâi nu lasă recepții/plăți/istoric în bază. Mesajul numără rândurile neașezate, explicând butonul inactiv înaintea erorii serverului.

### Fișiere atinse

- `src/KBot.Domain/AsociereStare.vb` — `DinPropunere` (nou).
- `src/KBot.App/Forexe/AsociereForm.vb` — al doilea constructor, `ModAsociere`, ramurile de încărcare / aprindere a butonului / salvare / închidere, `NehotarateleCount`, `DeciziiDin`, `SalveazaPropunereaAsync`, `TextDupaSalvare`.
- `src/KBot.App/KBOT.vb` — `DuLaIngestieAsync` + capătul lui `Tree_RightIconClicked`.
- `src/KBot.Api/IApiClient.vb`, `src/KBot.Api/ApiClient.vb` — retragerea lui `TrimitePrelucrareAsync`.
- `src/KBot.App/Forexe/PrelucrareCoordinator.vb` — retragerea lui `TrimiteAsync` (în locul ei, o notă care spune de ce nu se mai poate trimite într-o singură fază) și `SalveazaAsync` (nou).
- `tests/KBot.Domain.Tests/AsociereStareDinPropunereTests.vb` — **nou**, 8 teste.
- `tests/KBot.App.Tests/AsociereDeciziiTests.vb` — **nou**, 9 teste.
- `tests/KBot.App.Tests/PrelucrareCoordinatorTests.vb` — **+4 teste** pentru bucla fazei a doua.
- `tests/KBot.Api.Tests/PrelucrareApiClientTests.vb`, `tests/KBot.App.Tests/PrelucrareCoordinatorTests.vb` — mutate pe `CerePropunereAsync`; **niciun caz pierdut**, fiindcă bucla de întrebări e aceeași.
- Opt fișiere de teste cu clienți falși — stub-ul metodei retrase, șters.

### Rezultate de test

- `dotnet build KBot.sln` → **0 erori** (avertismentele `MSB3825` preexistente pe `.resx`).
- `KBot.App.Tests` → **217 trecute / 13 picate / 230**. Cele 13 roșii sunt aceleași dinaintea feliei (Ddf / Istoric / Xfa / MainFormNavItems), în fișiere neatinse aici; linia de plecare măsurată în această sesiune, după felia 0054, era 204/217 — deci cele **13 teste noi de aici trec toate** și nu s-a stricat nimic.
- `KBot.Domain.Tests` → **28 trecute / 3 picate / 31**. Cele 3 roșii sunt `DdfInfoTests.EtichetaRevizie_*`. **Preexistente, și se poate dovedi:** `DdfInfo.vb`, `DdfInfoTests.vb` sunt identice cu HEAD (`git status` gol pe ele), iar eticheta de revizie nu atinge nimic din felia asta.
- `KBot.Api.Tests` → **94 trecute / 1 picat / 95**. Roșul e `ApiClientTests.GetDdf_FormatsRevisionLabel_WithSpacePadding_Not_Zeroes` — același defect ca cele trei de mai sus (eticheta DDF spune `18.01.2026` în loc de `  0 - 18.01.2026`), în fișier identic cu HEAD.
- Nu s-a rulat `dotnet test KBot.sln` (regula casei: bancul DevHarness deschide ferestre reale).

### Ce rămâne neverificat / amânat

- ⚠⚠ **Nimic din felia asta n-a atins un server viu și nu s-a văzut pe ecran.** Formularul în modul propunere n-a fost desenat nici cu `DrawToBitmap`, iar drumul întreg — descărcare ▸ propunere ▸ așezare ▸ salvare — n-a rulat niciodată cap-coadă. Calculatorul cu FOREXE și cu tokenul e altul; aici nu se poate reproduce.
- ⚠ **Reconstituire fără comandă UI:** `ActiuneAsociere.Reconstituire`/`receptie_noua` există în ambele contracte, dar editorul nu le producea nici în modul oricând. F26 (recepție creată/ștearsă înaintea primei descărcări) nu are țintă; ignorarea ar scrie greșit Sters = 1, deci rândul blochează salvarea până la comanda recepție nouă. Amânată pentru a nu adăuga încă o formă de UI nevăzută în aceeași felie.
- ⚠ **Plățile lipsesc din tabloul de propunere** (propunerea nu le poartă), deci reperele de plată nu apar pe benzi și în grafic în timpul ingestiei. Apar după salvare, în editorul de oricând. Dacă se dovedește că operatorului îi trebuie acolo, e o adăugare pe rută, nu o rescriere aici.
- ⚠ **`sql/0048_alegeri_unitate.sql`**: operatorul confirmă că e aplicată pe server, și în sursă și în baza de lucru. Nu s-a verificat din repo — nu s-a atins nicio bază de aici.
- După salvare formularul NU se reîncarcă în modul de oricând: propunerea e consumată, iar o a doua citire ar cere celălalt mod. Corecturile se fac redeschizând editorul de oricând.

### Corectură după PRIMA rulare adevărată (08.09.2026)

Operatorul a apăsat butonul pe `AAB2MAACHXB`. Robotul a adus pachetul întreg (jurnalul cutiei negre îl arată complet: 5 tabele, 41 de rânduri de istoric, 10 recepții), iar apoi ingestia a căzut cu **«Object reference not set to an instance of an object»**.

**Cauza, dovedită în cod:** `MainForm._cts` este sursa de anulare **a sincronizării**, nu a formularului. În `KBOT.vb` are o singură atribuire — `SincronizeazaAsync`, linia dinaintea singurei ei folosiri — deci rămâne `Nothing` până când operatorul cere o sincronizare. `DuLaIngestieAsync` citea `_cts.Token` din alt flux: `NullReferenceException` **înainte ca vreo cerere să plece**. Se potrivește cu jurnalul serverului din acea rulare, care nu are niciun `POST /api/forexe/prelucrare` — numai `AUTH_LOGIN`, `forexe.tree`, `forexe.sumar`, `forexe.istoric`. Și cu mesajul: `NullReferenceException` nu e `ApiException`, deci a trecut prin `WithReauth` neatinsă, până la `Catch`-ul lui `DuLaIngestieAsync`.

Defectul e al feliei 0055: idiomul a fost copiat de la linia 1567 fără a observa că acolo câmpul tocmai fusese creat, două rânduri mai sus.

**Reparat:** `CancellationToken.None`, ca la toate celelalte paisprezece apeluri pornite de operator din shell. Ingestia nu are buton de anulare; când va avea, sursa se face lângă el. Declarația câmpului poartă acum o avertizare, ca să nu se mai copieze a treia oară.

- `src/KBot.App/KBOT.vb` — `DuLaIngestieAsync` (tokenul) + nota de la declarația lui `_cts`.
- `dotnet build src\KBot.App\KBot.App.vbproj -c Debug` → **0 erori, 0 avertismente**.
- `dotnet test tests\KBot.App.Tests` → **217 trecute / 13 picate / 230** — neschimbat față de măsurătoarea de dinainte de corectură, aceleași 13 roșii preexistente.

**Nu e acoperit de un test.** Câmpul se citește dintr-un `MainForm` construit, iar suita nu construiește shell-ul. Dovada rămâne cea de mai sus: o singură atribuire, într-un alt flux.

#### Ce se știe despre pachetul acela, din datele trimise de operator

Citit din istoricul descărcat, ca să se știe la ce se uită operatorul la prima propunere reușită: istoricul începe la 30.05.2026 (workflow REVERSE), deci cele patru recepții din ianuarie–martie nu au niciun instantaneu — normal, `verifica_acoperirea` cere acoperire pentru instantanee, nu pentru recepții. Rândurile de «Salvare receptie» formează lanțuri, nu recepții separate: `28.344,13` ▸ `28.152,13` ▸ `26.298,13` și `2.663,42` ▸ `1.429,42` sunt aceeași recepție micșorată de încasări (−192, −1.854, respectiv −414/−181/−639), iar numai ultima valoare apare în `ListaReceptii`. Exact cazul F9: trecerea automată poate așeza doar ULTIMUL instantaneu, deci verigile dinainte ajung la operator. **Niciun lanț nu pare să ceară o reconstituire** — nicio recepție creată ȘI dispărută complet —, deci golul de la «recepție nouă» nu ar trebui să blocheze salvarea acestui angajament. Dedus din date, **neverificat împotriva serverului.**

## Repere unice păstrate din registru

- Registrul datează confirmarea operatorului privind `sql/0048_alegeri_unitate.sql` la 07.09.2026, atât pentru sursă cât și pentru baza de lucru; nu era o verificare de bază făcută de asistent. Referințele vechi `sql/0049` erau necorespunzătoare, cum detaliază 0054.
- A doua rulare din 08.09 a ajuns la rută, a primit propunerea și a deschis formularul; 400 „Recepția 188 nu există” și lipsa contextului vechi sunt în 0056. Registrul încă cerea o salvare cap-coadă pe PC-ul cu FOREXE și vederea formularului cu context. Acest checkpoint nu este extins prin consolidare.
- Registrul propunea o țintă locală etichetată pentru comanda de recepție nouă; lipsa UI de reconstituire din 0055 rămâne descrisă în etapa ei, nu este o nouă decizie blocantă a sesiunii documentare.
