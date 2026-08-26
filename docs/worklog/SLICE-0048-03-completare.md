# SLICE-0048-03-completare — pasul 8, structura care călătorește, căile operatorului

Închiderea feliei 0048-03. Plan: `docs/PLAN_ForexeIngest_0048-03_Completion.md`, peste
`docs/PLAN_ForexeIngest.md` și `docs/FUNDAMENT_Asociere_Receptii.md`.

Transa **corectează trei decizii aplicate greșit**, **retrage o constatare care s-a dovedit
imposibilă**, și **face patru lucruri care fuseseră amânate și nu aveau voie să fie**. Niciun ciot,
niciun no-op documentat, nimic „lăsat pentru o felie viitoare".

---

## 0. Poarta de citire — ce a arătat fiecare verificare cerută

### 0.1 Nicio bază și niciun server nu au fost atinse

Cerința §0.1 a planului. **Nu s-a încercat nicio conexiune** — nici ca să se confirme o coloană,
nici ca să se ruleze un test. Fiecare fapt despre schemă vine din `MariaDB_Schema/000_DEMO.sql` și
din exportul Access de la `C:\AVACONT\FX_System_Export`, citite direct.

### 0.2 Numele de coloane ale pasului 8 — VERIFICATE, toate cinci

Cerința §2.1: orice nepotrivire ▸ stop. Nu a fost niciuna. Din `000_DEMO.sql`:

| Tabel | Coloană | Tip |
|---|---|---|
| `FX_Extrase` | `Referinta` | `varchar(255)` |
| `FX_Extrase` | `ReferintaDest` | `varchar(255)` |
| `FX_Extrase` | `CodAI` | `varchar(255)` |
| `FX_Plati` | `Referinta_TREZOR` | `varchar(255)` |
| `FX_Plati` | `CodAI` | `varchar(255)` |

### 0.3 Marcajul „nu s-a schimbat nimic" — GĂSIT în schemă, și cu mecanismul Access lângă el

Cerința §4.4: se stabilește din schemă **care coloană de pe `FX_Receptii_H`** îl poartă, sau se
raportează că lipsește. **Există**, și e chiar `FX_Receptii_H.Sters`, deja în `000_DEMO.sql`
(`tinyint(1) NULL`), deci nu se adaugă nimic.

Cum îl punea Access, citit în `FORMS/frmFX_DUBII_LISTA_HN.md`:

- e o **casetă de bifat** legată direct de coloană (`ControlSource = Sters`, eveniment `AfterUpdate`);
- la bifare, `Form_AfterUpdate` **golește** `DIFH`/`DIFHC` pe antet și `DIF`/`DIFC` pe linii
  (`UPDATE … SET DIFH=Null, DIFHC=Null WHERE ID=…`), fiindcă un instantaneu care nu consemnează
  nicio schimbare nu are o diferență de arătat;
- e chiar **poarta de salvare**: `frmFX_DUBII.btnSav_Click` refuză cât timp
  `DCount("ID", "tmpFX_Receptii_H", "TmpIDRecR IS NULL AND Sters=False") > 0` — adică un instantaneu
  poate rămâne neasociat **doar dacă** e marcat.

Asta confirmă §1.6 al fundamentului la literă: `Sters` pe latura de ISTORIC nu e un mecanism de
ascundere, e afirmația *acest instantaneu nu consemnează nicio schimbare*. Nu se confundă cu
`FX_Receptii_R.Sters` (D-L, «recepția a fost ștearsă pe site»): alt tabel, alt fapt. Codul portat îl
scria deja, prin acțiunea `ignorat`; ce lipsea era proba că e coloana potrivită, iar acum e făcută.

### 0.4 Două documente cerute de plan NU EXISTĂ, și n-au existat niciodată în depozit

Căutate în tot depozitul, în tot istoricul git (`git log --all --name-only`) și pe disc, în
`Desktop`/`Documents`/`Downloads`/`C:\AVACONT`:

- **`docs/PLAN_ForexeIngestSteps3to8.md`** — §6.5 și §8.10 cer corectarea §6 al lui. Fișierul nu e
  nicăieri și **nu apare în niciun commit**, deși e citat de F26, de O1, de worklog-ul 0048-03 și de
  antetul lui `routes/forexe/prelucrare.py`. A fost scris și s-a lucrat după el, dar nu a fost
  comis niciodată.
- **copia veche a lui `FUNDAMENT_Asociere_Receptii.md` din afara lui `docs/`** — §8.1 cere ștergerea
  ei. **Era deja ștearsă**: felia 0048-03 a mutat-o în `docs/` (worklog-ul ei §0.1). Nu există două
  revizii în circulație.

Nicio corecție nu s-a pierdut din cauza asta: amândouă s-au scris în
`docs/FUNDAMENT_Asociere_Receptii.md`, ca **corecția C5**, fiindcă el e documentul care
supraviețuiește. C5 consemnează și de ce §6 al planului absent era imposibil de urmat, și retragerea
cerinței lui §5.3.

---

## 1. Ce s-a schimbat și de ce

### 1.1 Pasul 8 — portat (§2)

`FX_Indicatori_Actualizare_Extrase` din `mdl_FX_Tasks_Receive_DWN`. Două instrucțiuni, în ordine, a
doua prinzând doar ce a lăsat prima:

```sql
UPDATE FX_Extrase E INNER JOIN FX_Plati P ON E.Referinta     = P.Referinta_TREZOR
   SET E.CodAI = P.CodAI WHERE E.CodAI IS NULL
UPDATE FX_Extrase E INNER JOIN FX_Plati P ON E.ReferintaDest = P.Referinta_TREZOR
   SET E.CodAI = P.CodAI WHERE E.CodAI IS NULL
```

- **Ordinea nu e o întâmplare și cele două nu se contopesc într-una cu `OR`.** Amândouă filtrează pe
  `CodAI IS NULL`, deci a doua trebuie să vadă rândurile pe care prima le-a completat deja. Contopite,
  `Referinta` și `ReferintaDest` ar concura pe același rând și ar câștiga oricare, tăcut.
- **Necondiționat**, la coada pașilor 1–7, în aceeași tranzacție. Nu se pazește cu niciun steag `are`
  și nu se sare când pasul 5 n-a scris nimic — exact ca originalul Access. `FX_Extrase` poate purta
  restanțe din rulări mai vechi, iar filtrul `CodAI IS NULL` face ca fiecare trecere să le recupereze.
- **Rulează în amândouă fazele**, fiindcă amândouă parcurg același drum; în propunere se derulează
  înapoi cu restul. **Rezultatul lui NU se raportează în propunere**: nu e ceva despre care operatorul
  are de decis, e o legătură mecanică între extrase și plăți. La salvare iese sub `scrise.FX_Extrase`.
- **Avertismentul care spunea că pasul 8 nu rulează a fost șters**, împreună cu `MESAJ_PAS8` și cu
  testul care îl pinuia.
- Comentariul la fața locului consemnează ce s-a cântărit: dacă două rânduri `FX_Plati` ar împărți o
  `Referinta_TREZOR`, MariaDB alege unul fără să spună care — aceeași purtare ca Access. Pasul 5
  deduplică plățile chiar pe `Referinta_TREZOR`, deci nu ar trebui să apară.

`PLAN_ForexeIngest.md` §12 e amendat: `FX_Extrase` rămâne în afara scopului **cu excepția** acestor
două instrucțiuni. §13 punctul 3 e închis.

### 1.2 F20 retrasă, pasul 4d mutat unde îl cheamă Access (§1, §4)

**F20 e RETRASĂ**, și nu prin măsurătoare, ci prin raționament: `DIFH` **nu e ceva ce trimite
FOREXE**. Se calculează local, de noi, în `FX_CalculeazaDIF_Receptii_Tmp`, **după** ce un instantaneu
a fost asociat unei recepții. În clipa în care ar fi nevoie de judecata «s-a schimbat ceva?», `DIFH`
nu există încă. **O3 se închide prin retragere, nu prin confirmare** — nicio interogare pe date vii nu
mai e datorată, iar interogarea scrisă în worklog-ul 0048-03 §5.3 nu mai trebuie rulată.

Ce s-a schimbat în cod: **nimic de șters, fiindcă nimic nu citea `DIFH` ca să decidă ceva.** Căutat
peste tot Python-ul și tot `src/`: singurii cititori sunt calea de AFIȘARE din Recepții (felia
0015-02, eticheta plutitoare), care e un consumator legitim al unei cifre calculate, nu o
clasificare. Se consemnează ca fapt verificat, nu ca presupunere.

`step4d_calculeaza_dif` **rulează acum în amândouă fazele**, prin `_pas4d_pe_receptiile_atinse`, exact
unde îl cheamă Access — după 4c. Înainte rula doar la salvare, ceea ce însemna o ramură care nu se
testa decât la salvare.

**F17 e amendată:** nu există clasificare automată de „salvare fără schimbare", în niciun punct. Două
blocuri cu aceleași cifre pot fi o salvare care n-a schimbat nimic, sau o modificare reală pe **altă**
recepție care se întâmplă să aibă aceeași sumă — mașina nu le poate deosebi, omul da. Formularul
(0048-04) arată «identic cu instantaneul dinainte în acest lanț» ca **informație**, recalculată la
fiecare drag, care nu scrie niciodată nimic. Marcajul e o **acțiune a operatorului**, pe
`FX_Receptii_H.Sters` — vezi §0.3.

### 1.3 Structura călătorește, de la site până în baza de date (§3, decizia D-N)

**Ce e imbricat NU s-a ghicit.** Enumerat din definițiile de workflow, din sarcina utilă reală și din
codul executorului. Tiparul e: un `ForEachVar` al cărui `collectFields` numește un câmp pe care un
`ScrapeTable` **interior** îl scrie cu `saveTo`. Peste toate cele șase `.wfl` din
`src/KBot.Forexe/Workflows/` sunt **exact două**:

| Tabel | Coloană | Ce e | Cine o citește |
|---|---|---|---|
| `ListaReceptii_results` | `Detaliu` | listă de obiecte — liniile recepției | pasul 4b |
| `TabelIndicatori_results` | `BugetIndicator` | listă de obiecte — bugetul indicatorului | **nimeni** (defectul D18, portat) |

Restul câmpurilor din `collectFields` (`Tip`, `Data`, `Suma`, `TipReceptie`, `DescriereReceptie`,
`CodIndicator`) sunt scrise de `<Read saveTo>`, deci scalare. `TabelIstoric` e un `ScrapeTable` simplu,
fără buclă, deci fără imbricare. `ListaAngajamente` la fel.

**Confirmat pe date reale, nu doar din definiții:** sarcina utilă de probă
`PYTHON/tests/fixtures/prelucrare_AAB37CNBK95.json` poartă exact aceste două coloane ca liste
imbricate, și nicio alta.

**Unde se pierdea și unde s-a oprit.** Executorul păstra deja structura —
`WorkflowExecutor.Actions.vb:BuildCollectedRow` face `JToken.Parse(fromVar)`, cu comentariul
«Preservă structura JArray/JObject». Pierderea era un pas mai încolo, în
`ForexeRunner.TryParseTable`: `row(prop.Name) = prop.Value.ToString()`. Pentru un scalar e fără
pierdere; pentru un `JArray` dă chiar textul JSON — o listă deghizată în text, pe care serverul
trebuia apoi să o parseze a doua oară.

**Clientul.** Trei tipuri noi în `KBot.Domain/CelulaTabel.vb`:

- `CelulaTabel` — o celulă e **exact una** din trei lucruri, recursiv: text, listă ordonată de celule,
  sau set de celule cu nume. Citirea ca text a unei celule imbricate **RIDICĂ**; turtirea tăcută e
  chiar defectul reparat.
- `RandTabel` — un rând, set ordonat de celule cu nume (ordinea coloanelor de pe site).
- `TabelRezultat` — un tabel, listă ordonată de rânduri, cu `ColoaneImbricate()` care răspunde «ce e
  imbricat aici» **din date**, nu dintr-o listă ținută de mână.

`JobResult.Tables` și `PrelucrareRezultat.Tabele` sunt acum `Dictionary(Of String, TabelRezultat)`.

**Serializarea: o punte, nu un `JsonConverter`.** `JsonConverter(Of T).Read` primește
`ByRef Utf8JsonReader`, iar `Utf8JsonReader` e `ref struct` — o formă pe care compilatorul VB.NET o
refuză din principiu (`BC30668`). **Niciun proiect VB nu poate scrie un convertor System.Text.Json.**
Deci conversia e explicită, în `TabeleJson`, la granițele de serializare care există: cererea HTTP,
depozitul local de rezultate, dosarul de asociere. `PrelucrareRezultat.Tabele` poartă `<JsonIgnore>`,
iar `TabeleSerializate As JsonObject` (`<JsonPropertyName("Tabele")>`) e puntea — deci numele pe disc
nu se schimbă și fișierele scrise înainte se citesc mai departe.

**Citit în depanator** (cerință, nu înfrumusețare):

- `DebuggerDisplay` pe toate trei tipurile: celula arată `listă[3]` sau `"Plata ces"`, rândul arată
  `7 coloane · Tip=«Plata ces» · Detaliu=listă[3]`, tabelul arată
  `12 rânduri · imbricate: Detaliu`;
- dumpul JSON local era deja `WriteIndented = True` (`WorkflowResultStore._jsonOptions`), deci
  structura se vede într-un editor de text;
- `PrelucrareRezultat.ToDebugString()` scrie tot arborele, indentat, cu coloanele imbricate numite pe
  fiecare tabel;
- **și istoricul lucrărilor** (`ForexeRunner.InregistreazaRezultat`) numește acum coloanele imbricate
  — e primul loc în care cineva se uită după o descărcare.

**Serverul.** Calea tolerantă a plecat odată cu aplatizarea. `_ca_lista` a devenit `cere_lista`:

- listă ▸ se acceptă;
- șir **negol** ▸ **RESPINS**, cu un mesaj care numește coloana și spune că *clientul trimite
  valoarea aplatizată* — nu «JSON nevalid», care ar trimite pe cine îl citește să caute o virgulă
  lipsă într-un șir perfect corect;
- șir gol ▸ depinde de apelant (`gol_permis`): pentru `BugetIndicator` înseamnă «tabelul interior
  n-a avut rânduri», care e chiar ce scrie `BuildCollectedRow`; pentru `Detaliu` rămâne o eroare.

De ce se poate închide ușa fără plasă: **D2 spune că Access e scos din uz și nu există un al doilea
scriitor, iar conducta nu a rulat încă niciodată pe o bază reală.** Nu există sarcini utile stocate în
forma veche de recuperat.

**`BugetIndicator` i se verifică forma deși nimeni nu-l citește.** Dacă EL sosește ca text, clientul
aplatizează — iar clientul acela aplatizează și `Detaliu`, pe care chiar îl citim. E cel mai ieftin
loc în care se prinde regresia, și prinde toată sarcina utilă deodată.

**Cealaltă jumătate a regulii (§3.6).** Fiecare loc din `routes/forexe/` care citește o celulă de
payload ca text o cere acum prin `text_celula(valoare, unde, coloana)`, care **ridică** pe o listă sau
un obiect. Un `.strip()` peste o listă ar crăpa cu `AttributeError` la prima rulare reală; un `str()`
peste ea ar fi mai rău — ar scrie liniștit `[{'Cod': 'AAB'}]` într-o coloană de bază de date și nimeni
n-ar afla vreodată. Locurile: cele patru celule ale rândului de istoric (3a), cele două de
identificare și cele patru de bani ale indicatorului (2), cele patru ale antetului de recepție (4b) și
cele cinci frunze din interiorul lui `Detaliu`.

### 1.4 F28 — reconstituirea neverificabilă (§5)

`FX_Receptii_R` capătă `ReconstituitNesigur tinyint(1) NOT NULL DEFAULT 0`, în **același**
`sql/0049_receptii_stergere.sql` (fișierul nu s-a aplicat nicăieri, deci se amendează, nu se dublează),
și pe `AVACONT_SURSA` la fel. Proba `information_schema` de dinainte de rulare și așteptările de după
sunt actualizate: patru coloane noi acum, nu trei.

Trei coloane, trei fapte, niciodată colapsate:

| Coloană | Înseamnă |
|---|---|
| `FX_Receptii_R.Sters` | recepția a fost ștearsă pe site |
| `FX_Receptii_R.Reconstituit` | a fost reconstruită din propriile ei instantanee; n-a avut niciodată rând în `ListaReceptii` |
| `FX_Receptii_R.ReconstituitNesigur` | în clipa reconstituirii, altă reconstituire pe același angajament făcea gruparea imposibil de verificat |

Regula e o **funcție pură**, `f28_de_marcat(reconstituite)`: una singură ▸ nimic; două sau mai multe ▸
**toate**. Ambiguitatea e ÎNTRE ele, deci nu aparține niciuneia singure — și nici măcar celei adăugate
ultima: fiecare instantaneu al oricăreia ar fi putut sta pe cealaltă. Exact condiția lui F27 și nimic
mai larg.

`marcheaza_reconstituirile_nesigure` o aplică la salvare, la coada pasului 4c, **recitind
reconstituirile din tabel** — nu numărând deciziile — ca să intre în socoteală și cele rămase din
rulări mai vechi: două reconstituiri făcute în două sesiuni diferite sunt exact la fel de imposibil de
deosebit ca două făcute în aceeași sesiune.

**Nu se șterge niciodată.** Funcția spune doar pe cine să marchezi, nu și pe cine să demarchezi, iar
un test pinuiază chiar absența acelui drum.

**Se vede:** răspunsul de salvare poartă un avertisment românesc care numește recepțiile;
`GET /api/forexe/receptii` întoarce `reconstituit` și `reconstituit_nesigur`; arborele Recepțiilor
pune un semn pe rând — `⟲ reconstituită`, sau `⚠ reconstituită (grupare neverificată)`. Text, nu
pictogramă: **nu s-a adăugat și nu s-a scos niciun control din vreun designer** (§11). Avertizarea
operatorului în clipa în care pornește a doua reconstituire e a formularului, deci 0048-04 — și
propunerea îi dă deja steagul pe fiecare recepție ca să o poată face.

F28 e scrisă în fundament, în Part 2.

### 1.5 Căile aparțin operatorului (§6, decizia D-O)

**Întâi s-au enumerat.** Fiecare loc care compunea o cale din directorul aplicației sau dintr-un
literal — cerința §6.1, făcută **înainte** de orice schimbare:

| Ce | Unde era compus | Setare | Implicit |
|---|---|---|---|
| Jurnale | `LogPaths.LogsDirectory` + `MainForm.vb:161` + `Program.vb:227` + `ForexeConnectTest.vb:69` | `Logs` | `<AppDir>\Logs` |
| Dosarele de asociere | `AsociereStore.Folder` | `Asociere` | `<AppDir>\Asociere` |
| Rezultatele brute FOREXE | `WorkflowResultStore.OutputFolder` | `WorkflowResults` | `<AppDir>\WorkflowResults` |
| PDF temporar | `TempPdfStore.Folder` | `TempPdf` | `<AppDir>\TempPdf` |
| Definițiile `.wfl` | `WorkflowCatalog.ResolvePath` + `ForexeController.vb:129` + `ForexeConnectTest.vb:55` | `Workflows` | `<AppDir>\Workflows` |
| Exporturile bancului | `TreeSettingsExporter.vb:123` | `Exports` | `<AppDir>\Exports` |
| PDF-uri DDF | `KBotPaths.DefaultDdfPdfRoot` | `DdfPdf` | `C:\AVACONT\FOREXE\PDF\DDF\` |
| PDF-uri ORD | `KBotPaths.DefaultOrdPdfRoot` | `OrdPdf` | `C:\AVACONT\FOREXE\PDF\ORD\` |

Rămase **deliberat** în afară, cu motivul: `AdobeWindowHosting` (`ProgramFiles` — caută unde e
INSTALAT Adobe, nu un folder în care scriem noi), `ThemeStore` (`%AppData%`, magazinul de temă, deja
per utilizator), `MigratorSettings` (setările uneltei de migrare, nu ale aplicației), `KBotPaths`
însuși (locul fișierului de setări), și `src/KBot.Forexe/_reference/` (instantaneu importat, nu se
compilează).

**Un singur rezolvator.** `KBot.Common.KBotPaths` e singurul loc în care se rezolvă o cale. Fiecare
loc de mai sus îl întreabă; nicăieri altundeva nu se mai compune una. Două căi compuse de mână în
`ForexeController` și `ForexeConnectTest` au căpătat pe drum o constantă,
`WorkflowCatalog.ConectareFile`, ca toate celelalte workflow-uri.

**Unde stau:** `%APPDATA%\AVACONT\KBot\settings.json`, o intrare per folder, JSON plat. *Alegerea mea,
nu o decizie deja luată* — ușor de privit, ușor de salvat, per utilizator prin construcție, și ține
căile în afara ramurii de registru care duce deja `CodFiscal`-ul per DC. Un cuvânt și devine registru;
nimic altceva nu se schimbă.

**Regulile, toate implementate:**

- fișier lipsă, cheie lipsă sau valoare goală ▸ implicitul. **Un operator care nu configurează nimic
  primește exact ce primea înainte de felia asta**, fișierul poate lipsi cu totul;
- o valoare relativă se rezolvă față de directorul aplicației;
- un folder configurat care nu există **se creează la pornire** — dar numai cele în care SCRIEM. Un
  folder doar-citit (`Workflows`) nu se inventează: un folder gol creat de noi ar ascunde o instalare
  incompletă în loc să o arate;
- un folder în care nu se poate scrie ▸ **se oprește lansarea**, cu un mesaj românesc care numește
  **setarea și calea** și spune ce să corecteze. Niciodată o cădere tăcută pe implicit: un operator
  care a pus o cale și a primit-o pe cea veche a fost mințit, iar minciuna se descoperă abia când
  caută un fișier care nu e unde l-a trimis;
- **totul se validează la PORNIRE**, în `Program.Main`, înaintea temei și a oricărei ferestre — o cale
  greșită trebuie să oprească lansarea, nu o ingestie ajunsă la jumătate.

**O buclă care trebuia evitată, și cum.** `GlobalErrorLog` scrie în folderul de jurnale, iar folderul
de jurnale se rezolvă tocmai de aici. Dacă `SetariFoldere.Incarca` ar loga, prima citire s-ar închide
în ea însăși. De-aia **nu loghează niciodată**: strânge problemele neblocante într-o listă
(`Probleme`), iar `Program.ValideazaSetarileDeFolder` le scrie în jurnal la pornire, unde oricum le e
locul.

**Cele două rădăcini de PDF trăiesc în amândouă fișierele.** `settings.json` are ultimul cuvânt,
`kbot_paths.json` rămâne citit ca al doilea. Un operator care le-a pus deja acolo nu se trezește tăcut
cu implicitul — ceea ce ar încălca chiar regula de mai sus. `kbot_paths.json` păstrează opțiunile de
COMPORTAMENT ale gazdei Adobe, care nu sunt căi.

**Nu s-a scris niciun formular** (§6.6). S-au livrat magazinul, validarea și API-ul de citit/scris;
tabelul de mai sus e contractul de care se leagă formularul, iar `SetariFoldere.Toate` îl poartă în
cod, cu descriere românească pe fiecare setare. Adăugarea controalelor în designer și proba
dus-întors în Visual Studio sunt ale Adelinei, ca întotdeauna.

**`AsociereStore` rămâne în `KBot.Common`** (§6.5), lângă `KBotPaths`. Motivul e scris acum în
corecția C5 a fundamentului, fiindcă planul care cerea altceva nu există ca fișier — vezi §0.4.

### 1.6 `FX_ORD_TBL_REC` (§7) — era deja făcut, mai puțin proba

Cele șase puncte, verificate unul câte unul, nu presupuse:

1. **Afară din lista de excluderi** — făcut în 0048-03 (`TableMaps.vb:49` nu-l mai conține).
2. **Harta** — există și e corectă: `IDORDREC` călătorește ca el însuși (cheia Access, păstrată),
   `IDORDRECP` exclus (oglinda serverului VECHI; pe MariaDB e `AUTO_INCREMENT`), `IDORDTBL` redenumit
   `IDORDTBLP`, `IDRP` exclus (mort, C2), `Valoare` și `IdPlataFX` directe.
3. **Ordinea** — cele două chei străine sunt reale și citite din `000_DEMO.sql`
   (`FX_ORD_TBL_REC__FX_ORD_TBL` pe `IDORDTBLP`, `FX_ORD_TBL_REC__FX_Plati` pe `IdPlataFX`, amândouă
   `ON DELETE CASCADE`). **Asta lipsea: proba.** Adăugat
   `test_fx_ord_tbl_rec_is_written_after_both_its_parents`, care verifică ordinea implicită din
   `routes/migrare/tables.ALL`. Latura VB (`WriteOrder.Derive`) deduce ordinea din cheile străine VII
   ale țintei, deci ajunge în același loc pe alt drum — **dar asta nu s-a putut rula**, fiindcă
   `TargetSchema` se construiește doar dintr-o conexiune vie și nu există proiect
   `KBot.Migrator.Tests`. Vezi §4.
4. **`FX_Plati` e în setul migrat** — da, `tables.py:78`; pinuit acum separat, fiindcă «e mai sus în
   listă» și «există» sunt două lucruri.
5. **`FX_Receptii_Plati` rămâne exclus** — pinuit: nu e în `ALL`, e în `OUT_OF_SCOPE`.
6. **Documentele** — `PLAN_ForexeIngest.md` D5 era deja împărțit în 0048-03; `KBOT_STATUS.md` nu mai
   listează tabelul ca exclus. C1 punctul 2 e marcat făcut, cu commit-ul.

---

## 2. Fișiere atinse

**Nou**
```
src/KBot.Domain/CelulaTabel.vb                     CelulaTabel / RandTabel / TabelRezultat / TabeleJson
src/KBot.Common/SetariFoldere.vb                   setările de folder + validarea + excepția
tests/KBot.Api.Tests/StructuraCelulelorTests.vb    D-N pe firul HTTP
tests/KBot.Common.Tests/SetariFoldereTests.vb      D-O
docs/worklog/SLICE-0048-03-completare.md
```

**Modificat — server**
```
PYTHON/routes/forexe/prelucrare.py            pasul 8, 4d în ambele faze, F28, text_celula
PYTHON/routes/forexe/prelucrare_pasi.py       step8_*, pas8_instructiuni, cere_lista, text_celula
PYTHON/routes/forexe/prelucrare_asociere.py   f28_de_marcat + marcheaza_reconstituirile_nesigure
PYTHON/routes/forexe/receptii.py              reconstituit / reconstituit_nesigur pe firul de citire
sql/0049_receptii_stergere.sql                + ReconstituitNesigur, proba actualizată
```

**Modificat — client**
```
src/KBot.Domain/PrelucrareRezultat.vb         TabelRezultat + puntea JSON + ToDebugString
src/KBot.Domain/AngajamentMapper.vb           supraîncărcare peste TabelRezultat
src/KBot.Domain/ReceptiiInfo.vb               Reconstituit / ReconstituitNesigur
src/KBot.Forexe/JobModels.vb                  Tables As Dictionary(Of String, TabelRezultat)
src/KBot.Forexe/ForexeRunner.vb               TryParseTable nu mai aplatizează; DinJToken
src/KBot.Forexe/WorkflowCatalog.vb            ConectareFile + ResolvePath prin KBotPaths
src/KBot.Forexe/Workflows/adlop - Prelucrare Completa*.wfl   nota despre TipReceptie / Detaliu
src/KBot.Api/ApiClient.vb                     TabeleJson.Catre + cele două steaguri
src/KBot.Api/UpsertAngajamenteRequest.vb      tabele As JsonObject; GetReceptieRow
src/KBot.App/Program.vb                       validarea setărilor la pornire
src/KBot.App/Forexe/ForexeController.vb       TabelRezultat + calea de conectare
src/KBot.App/Forexe/WorkflowResultStore.vb    TabelRezultat + KBotPaths
src/KBot.App/Views/ReceptiiView.vb            semnul de reconstituire în arbore
src/KBot.App/MainForm.vb                      LogPaths în loc de cale compusă
src/KBot.Common/KBotPaths.vb                  Foldere / ValideazaFoldere / cele șase proprietăți
src/KBot.Common/Logging/LogPaths.vb           prin KBotPaths
src/KBot.Common/AsociereStore.vb              prin KBotPaths
src/KBot.Common/TempPdfStore.vb               prin KBotPaths
src/KBot.DevHarness/…                         4 fișiere: tipurile noi + căile
src/KBot.{Api,App,Common,DevHarness,Domain,Forexe}.vbproj   FileVersion
```

**Modificat — documente**
```
docs/FUNDAMENT_Asociere_Receptii.md   F17 amendată, F20 retrasă, F28 nouă, C5 nouă, D-N/D-O, O3/O5 închise
docs/PLAN_ForexeIngest.md             §12 (FX_Extrase, cu excepția pasului 8), §13 punctul 3 închis
docs/worklog/KBOT_STATUS.md           rândul feliei + firele
```

**Modificat — teste** (6 fișiere Python, 2 .NET)

---

## 3. Rezultatele testelor

Numărate acum, nu copiate.

**Python** — `PYTHON/.venv`, din `PYTHON/`:

```
429 trecute, 15 sărite, 0 căzute        (înainte de transă: 410 / 15)
```

`+19`. Cele 15 sărite sunt neschimbate ca număr: `test_forexe_prelucrare_live.py` se sare **în
întregime** în afara gazdei, deci cele patru teste noi de acolo nu se văd în contor.

Cele care contează:

- `test_step8_sql_joins_on_referinta_then_referintadest_in_that_order` — forma SQL-ului, pinuită prin
  **același** ajutor pe care îl cheamă ruta (`pas8_instructiuni`); o constantă copiată în test ar
  rămâne verde și după ce ruta ar înceta să o mai folosească;
- `test_step8_runs_unconditionally_even_with_an_empty_payload` — niciun steag `are` ridicat, și tot
  rulează;
- `test_step8_result_is_not_reported_in_the_proposal` — a rulat, s-a derulat înapoi, nu se raportează;
- `test_a_flattened_detaliu_string_is_rejected_and_the_message_names_the_column` — mesajul spune
  *aplatizat*, nu «JSON nevalid»;
- `test_a_flattened_buget_indicator_is_rejected_by_name` + `…_nested_…_is_accepted` +
  `…_empty_…` + `…_missing_…` — a doua coloană imbricată, toate patru intrările;
- `test_a_nested_scalar_column_is_rejected_by_name` — cealaltă jumătate a regulii;
- `f28_de_marcat` pe cinci cazuri, inclusiv `…_never_cleared_by_a_later_run_seeing_only_one`;
- `test_fx_ord_tbl_rec_is_written_after_both_its_parents` (§7.3).

**Scrise și lăsate sărite** (au nevoie de MariaDB, tiparul de sărire din `test_forexe_ddf.py`):
`test_step8_links_an_extras_to_a_payment_by_referinta`,
`test_step8_does_not_overwrite_an_extras_that_already_has_a_codai`,
`test_the_proposal_rolls_step8_back_like_everything_else`,
`test_the_reconstituit_nesigur_column_exists`,
`test_two_reconstructions_on_one_angajament_flag_both` — plus cele șase din 0048-03, între care
**`test_the_proposal_leaves_every_table_byte_identical`**, care rămâne cel mai important test
nerulat al feliei.

**.NET** — `dotnet build KBot.sln`: **0 erori, 0 avertismente noi.** Cele 10 avertismente `MSB3825`
(BinaryFormatter în patru `.resx` de vederi) apar doar la o reconstrucție completă și sunt de pe
`master` — măsurate explicit, prin `git stash`, înainte și după.

| proiect | înainte | după |
|---|---|---|
| `KBot.Api.Tests` | 87 trecute / **1 căzut** | 95 trecute / **1 căzut** (`+8`) |
| `KBot.Common.Tests` | 66 trecute / 0 | 85 trecute / 0 (`+19`) |
| `KBot.Domain.Tests` | 14 trecute / **3 căzute** | 14 trecute / **3 căzute** |
| `KBot.App.Tests` | 169 trecute / **13 căzute** | 169 trecute / **13 căzute** |
| `KBot.Controls.Tests` | 917 / 0 | 917 / 0 |
| `KBot.Theming.Tests` | 110 / 0 | 110 / 0 |
| `KBot.LocalStore.Tests` | 1 / 0 | 1 / 0 |
| `KBot.Xfa.Tests` | 39 / 0 | 39 / 0 |

**Zero teste noi căzute.** Cele **17** roșii sunt exact cele de pe `master`, neatinse (§0.6): DDF /
Istoric / XFA, enumerate în worklog-ul 0048-03 §4.2. Nu s-a rulat `dotnet test KBot.sln` ca întreg —
suita DevHarness deschide ferestre Adobe reale pe ecranul operatorului; fiecare proiect s-a rulat
separat.

---

## 4. Rămas neverificat

- **Nimic din transă nu a rulat pe o bază MariaDB, și nici nu s-a încercat** (§0.1, §11).
- `sql/0049_receptii_stergere.sql` **nu s-a aplicat nicăieri**. Acum are patru `ALTER`-uri, nu trei;
  proba `information_schema` de rulat înainte e scrisă în fișier.
- **Nimic nu s-a văzut pe ecran.** Semnul de reconstituire din arborele Recepțiilor, mesajul de
  oprire la o cale nevalidă, și afișarea în depanator a celulelor imbricate sunt **cod verde,
  nerulat sub ochii cuiva**.
- **`WriteOrder.Derive` nu s-a probat** (§7.3, latura VB). `TargetSchema` are constructor privat și se
  încarcă doar dintr-o conexiune vie, iar `KBot.Migrator` nu are proiect de teste. Ordinea e
  confirmată **static**: cele două chei străine sunt în `000_DEMO.sql`, algoritmul construiește
  muchiile chiar din ele, și ambii părinți sunt în setul migrat. Latura Python a aceleiași ordini e
  probată. Ce ar închide golul: o fabrică de `TargetSchema` vizibilă testelor + un proiect
  `KBot.Migrator.Tests`.
- **Formularul de setări nu există** (§6.6, deliberat). Contractul de care se leagă e §1.5 și
  `SetariFoldere.Toate`.
- **Formularul de asociere nu există** — 0048-04, D-A. Nimic nu cheamă coordonatorul.
- Rata de potrivire a hash-ului față de datele migrate (§8 al planului părinte) — **nerulată**,
  același motiv.
- `%APPDATA%\AVACONT\KBot\settings.json` **nu s-a scris niciodată pe mașina asta**: toate testele
  lucrează pe directoare temporare, deliberat, ca să nu contamineze mașina operatorului.

---

## 5. Pentru cine vine după

### 5.1 Contractul setărilor de folder (pentru formularul din 0048-04)

Tabelul din §1.5 e complet: cheia, ce e folderul, implicitul. În cod, `SetariFoldere.Toate` poartă
aceleași lucruri, cu `Descriere` în română pentru etichetă și cu `SeScrie` care spune dacă folderul se
creează și i se verifică dreptul de scriere.

Formularul citește `Bruta(cheie)` — **nu** `Cale(cheie)`: câmpul trebuie să apară **gol** când
operatorul n-a configurat nimic, nu preumplut cu implicitul rezolvat, altfel prima salvare îngheață
implicitul de azi ca alegere deliberată pe veci. Aceeași capcană ca `ShouldSerialize` la controale.

Scrierea e `SetariFoldere.Salveaza(valori, [dir])`; cheile cu valoare goală se omit. După salvare,
setările se aplică **la repornire** — `KBotPaths.Foldere` e încărcat o singură dată, iar folderul de
jurnale nu se poate muta sub un fișier deja deschis.

### 5.2 Dacă vreo coloană devine imbricată în FOREXE

Nu se ghicește și nu se caută prin cod: se citesc `.wfl`-urile după tiparul din §1.3 (`ForEachVar` +
`collectFields` + `ScrapeTable saveTo` interior). La rulare, `TabelRezultat.ColoaneImbricate()`
răspunde din date, iar istoricul lucrărilor o scrie după fiecare descărcare.

O coloană nouă imbricată **se oprește singură**: dacă e citită ca text, `text_celula` ridică și o
numește. Asta e chiar de dorit — e o schimbare în FOREXE, și trebuie aflată, nu ocolită.

### 5.3 Ce NU mai trebuie făcut

- **Nu mai rulați interogarea F20** din worklog-ul 0048-03 §5.3. F20 e retrasă; O3 e închisă prin
  retragere. `DIFH` nu poate răspunde la întrebarea aceea, fiindcă se calculează după asociere.
- **Nu mai căutați `PLAN_ForexeIngestSteps3to8.md`.** Nu există, nu a fost comis niciodată, și tot ce
  a decis și mai contează e în fundament sau în comentariile codului — vezi corecția C5.
