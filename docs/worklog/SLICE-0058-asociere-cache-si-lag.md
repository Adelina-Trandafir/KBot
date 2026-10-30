# SLICE-0058 — Istoric cache, verificări, benzi și jurnal de asociere

Cele două etape din 09.09.2026 rămân distincte. Prima repară cazuri demonstrabile în cod, fără a dovedi că erau cazul operatorului; a doua adaugă jurnalul, iar nota despre mesajul „Recepția 247” arată că rularea live încă folosea codul anterior. Cazul nu este declarat rezolvat prin simpla existență a jurnalului. Probe cu jurnal pornit/oprit și blocuri de date sintetice rămân probe offline. Eliminarea semnului de dată din baza 0058 este amendamentul ulterior lui F13 din [fundament](../FUNDAMENT_Asociere_Receptii.md#f13).

[0058-asociere](#slice-0058) · [0058-02](#slice-0058-02)

## SLICE 0058 — Așezarea recepțiilor: cache la descărcare, buton de golire, tăcere pe dată, lag pe benzi <a id="slice-0058"></a>

Șapte sesizări ale operatorului, toate din 09.09.2026, toate pe drumul «descărcare FOREXE ▸ propunere ▸ `AsociereForm` ▸ salvare».

---

### 1. Datele descărcate rămân în memorie și se pot refolosi

**Ce se întâmpla.** Fiecare apăsare relua robotul integral, inclusiv browserul/tokenul, deși a doua cerere pe același angajament era de obicei o reluare după renunțare sau o așezare greșită, nu o actualizare a datelor.

**Ce s-a făcut.** `WorkflowResultStore` ținea deja pachetele în memorie (`_ultimulNod`) — nu s-a inventat niciun cache nou, s-a folosit cel care exista. S-au adăugat:

- `MomentLista` — când s-a descărcat lista (pachetele de nod își poartă singure `Moment`);
- `NumaraRanduri(pachet)` — rânduri, nu tabele (fluxul întoarce cele cinci tabele ale lui și când toate sunt goale, deci `Tabele.Count` ar răspunde 5 pe un pachet gol);
- PachetBunDeRefolosit(cod): oferă numai pachet existent cu măcar un rând. Depozitul acceptă numai succesul, iar ingestia ar refuza oricum pachetul gol: acesta este criteriul «corect descărcat».

`MainForm.IntreabaDacaRefolosescPachetul` (iconița nodului) / `IntreabaDacaRefolosescLista` (sincronizare) afișează ORA descărcării: Da refolosește memoria, Nu descarcă din nou. Eșecul întrebării descarcă din nou.

Refolosirea păstrează contractul 0055: același payload traversează ambele faze, iar pachetul memorat este obiectul descărcat.

`ForexeController.SpuneStare` e nou și public: fără el consola ar fi arătat o ingestie fără descărcarea ei.

### 2. Buton de golire a așezărilor pasului curent

btnReseteaza «Golește așezările» apare numai în propunere; editorul de oricând nu are pas curent, deci nu oferă desprinderea tuturor legăturilor.

Limita este `InstantaneuLegat.Blocat`, comună cu `NehotarateleCount` și `DeciziiDin`: serverul blochează contextul deja scris și deblochează exact mulțimea de decis. Butonul folosește aceeași regulă ca formularul.

Resetarea trece la NEAȘEZAT, fără revenire la sugestiile automate pe care operatorul nu le-a ales. Cere confirmare: pierde munca fără Undo.

### 3. Niciun avertisment despre data recepției — nicăieri

F13 fusese retras ca veto pe 31.08.2026 și lăsat ca SEMN. Semnul se aprindea pe date perfect corecte (`DataR` e tastat pe site și sosește la miezul nopții, deci orice instantaneu din chiar ziua recepției ieșea «înainte de ea»), deci apărea pe rând după rând fără să spună nimic. Șters din:

- `AsociereForm.CaptionInstantaneu` (semnul de pe rând), `TooltipReceptie` (numărătoarea), `TooltipInstantaneu` (paragraful), plus funcția `EsteInainteDeDataReceptiei` cu totul;
- `prelucrare_asociere.valideaza_plasarile` — blocul care scria în `avertismente`.

Parametrul `rec` a ieșit din `CaptionInstantaneu` și `TooltipInstantaneu`: nu-l mai folosea nimeni.

### 4. «Recepția 235» nu exista nicăieri în listă

**Ce s-a văzut.** `ApiException: Recepția 235: liniile ultimului instantaneu nu se potrivesc cu cele ale recepției.` — cu recepțiile puse corect, și cu un număr pe care operatorul nu-l găsea nicăieri.

**Două defecte, nu unul.**

**(a) Numărul:** ca în 0056 («Recepția 188 nu există…»), `IDRR` creat în tranzacția ingestiei nu este nume stabil. `valideaza_plasarile.id_stabil` primește `False` din `aplica_decizii`, iar mesajul folosește DATA și VALOAREA din `CaptionReceptie`. Editorul de oricând păstrează implicit `True`, având IDRR real.

**(b) Refuzul:** dicționarul de capăt de lanț pe `cod_indicator` păstra numai ultima linie când același indicator avea alt `CodAI` / `CodSSI`. Ordini diferite pe cele două tabele puteau refuza o plasare corectă. `_valori_pe_indicator` însumează independent de ordine (totalul antetului este verificat anterior) și elimină zerourile, permise de F16 chiar când lipsesc pe cealaltă latură. Mesajul numește indicatorul diferit.

### 5. Arborii nu mai sar la primul rând după fiecare tragere

AdvancedTreeControl.Clear resetează legitim derularea; AsociereForm.Reconstruieste golea/reumplea ambii arbori la fiecare drag, întorcând operatorul la primul rând, departe de recepția curentă.

Noul `AdvancedTreeControl.ScrollOffsetY` se salvează înainte și se restaurează după reconstrucție. Setterul cheamă `RefreshScrollVisibility()` înainte: după `Clear`, cursa zero până la pictare ar limita `AutoScrollPosition` la zero. Selecția nu se păstrează; hrănește grila/graficul și se schimbă prin tragere.

### 6. Recepțiile noi se văd altfel

Rădăcinile noi primesc aldin+cursiv, cele de server rămân aldine. ReceptiePropusa.RandReceptie marchează exact recepțiile create în descărcarea curentă (0056), Nothing pentru celelalte; aldinul singur nu le distingea.

Eticheta plutitoare a unei recepții noi nu mai scrie numărul, ci «Recepție NOUĂ, din descărcarea curentă»: numărul de acum nu e cel de după salvare (vezi §4a).

### 7. Lag pe machetele de distribuție

**Dubla tamponare exista** (`OptimizedDoubleBuffer` în constructor). Costul era desenul: 20 de benzi × 20 de marcaje alocau și eliberau peste o mie de `Pen` / `SolidBrush` la fiecare repictare, inclusiv la trecerea cursorului peste bandă, marcaj sau reper.

Ce s-a schimbat în `KBotLaneView`:

- **GDI+ reutilizat:** _fillBrush/_backBrush/_edgePen/_detailPen, patru obiecte pentru forme pregătite simultan. ScratchPen/ScratchBrush rescriu toate proprietățile, fără capete rotunde/stil punctat moștenite din desenul precedent.
- **`KBotLane.PlottedInOrder`**: marcajele desenate, deja sortate pe X, umplute de pasul de așezare. Înainte, `DrawLaneSegments` construia și sorta o listă PE BANDĂ, PE REPICTARE.
- **`_px1` / `_px2` / `_px3`**, cele trei grosimi scalate la DPI, calculate cu așezarea: `ScaleDpi` era chemat de patru-cinci ori pe marcaj pentru trei numere care nu se pot schimba între două marcaje.
- **`EffectivePlotBackColor()` citit o dată** pe pictare, nu de două-trei ori pe marcaj.
- **`ControlStyles.Opaque`:** elimină ștergerea WinForms redundantă cu `g.Clear(BackColor)` de la începutul `OnPaint`. Ieșirea timpurie pe `_updateDepth > 0` curăță acum explicit suprafața.

Amândouă suprafețele beneficiază: sunt același control (`benzi` din `AsociereForm` și `benziMari` din `AsociereBenziForm`).

---

### Fișiere atinse

**VB.NET**
- `src/KBot.App/Forexe/AsociereForm.vb` — §2, §3, §5, §6
- `src/KBot.App/Forexe/AsociereForm.Designer.vb` — `btnReseteaza`, `tlyAsociere` la 3 coloane
- `src/KBot.App/Forexe/WorkflowResultStore.vb` — §1
- `src/KBot.App/Forexe/ForexeController.vb` — `SpuneStare`
- `src/KBot.App/KBOT.vb` — §1 (cele două întrebări)
- `src/KBot.Controls/Tree/AdvancedTreeControl.vb` — `ScrollOffsetY`
- `src/KBot.Controls/Lane/KBotLane.vb` — `PlottedInOrder`
- `src/KBot.Controls/Lane/KBotLaneView.vb` — câmpurile de lucru, `Opaque`, `Dispose`
- `src/KBot.Controls/Lane/KBotLaneView.Painting.vb` — §7

**Python**
`PYTHON/routes/forexe/prelucrare_asociere.py` — §3, §4; `PYTHON/routes/forexe/asociere.py` — nota din antet; `PYTHON/tests/test_forexe_prelucrare_asociere.py`, `PYTHON/tests/test_forexe_asociere.py`

### Rezultatele rulărilor

- `dotnet build KBot.sln` — **0 erori, 0 avertismente**.
- `dotnet build src\KBot.App\KBot.App.vbproj` (curat) — 0 erori, cele **7 `MSB3825` preexistente** pe `.resx`.
- `PYTHON/.venv` suita întreagă — **514 trecute / 19 sărite / 0 picate** (de la 509). Cinci teste noi în `test_forexe_prelucrare_asociere.py`; două rescrise acolo și unul în `test_forexe_asociere.py`, fiindcă pineau semnul F13 care tocmai a fost șters.

### Ce NU s-a verificat

- **Nimic nu s-a văzut pe ecran.** Nici formularul cu butonul nou, nici arborii care își țin derularea, nici cursivul recepțiilor noi, nici benzile după curățarea desenului. Câștigul de viteză e ARGUMENTAT din cod (alocări per repictare), **nu măsurat** — nu există niciun cronometru pus pe `OnPaint` și nicio rulare pe calculatorul operatorului.
- **`KBotLaneView` nu are NICIUN test.** `tests/KBot.Controls.Tests` nu conține niciun fișier de bandă, deci refactorizarea desenului n-are plasă de siguranță în afară de compilare. Suitele VB.NET nu s-au rulat în sesiunea asta (cerere explicită a operatorului: «no tests»).
- **Nimic n-a plecat spre serverul viu.** Repararea de la §4 (suma pe indicator, numele recepției) e verificată doar pe funcții pure; nicio salvare adevărată n-a trecut prin ea, deci nu se știe încă dacă exact ăsta era cazul care refuza salvarea operatorului — se știe doar că e un caz care refuza, și că mesajul numea o recepție de negăsit.
- **Refolosirea din memorie n-a fost încercată.** Nicio sesiune FOREXE n-a fost deschisă în sesiunea asta.
- Nimic nu s-a comis și nimic nu s-a împins (cerere explicită: «no git»).

---

## SLICE 0058-02 — Jurnalul asocierii (`asociere.log`), cu întrerupător în `main.py` <a id="slice-0058-02"></a>

A doua trecere peste felia 0058. Prima a reparat două defecte ale verificării F15 (suma pe indicator, numele recepției) și a lăsat scris negru pe alb că **nu se știe dacă exact acela era cazul care refuza salvarea operatorului**. Rularea adevărată din 09.09.2026, ora 14:18, a răspuns: nu era.

```
2026-09-09 14:18:36,608 - WARNING - PRELUCRARE dc=000_DEMO cod=AAB2EF2MCP4
decizii respinse: Recepția 247: liniile ultimului instantaneu nu se potrivesc cu
cele ale recepției. Lanțul nu se închide.
```

Linia asta e tot ce lasă în urmă o salvare refuzată — și nu spune nici ce lanț s-a format, nici ce cifre s-a uitat serverul la ele, nici ce a vrut operatorul să facă. Felia asta scrie exact ce lipsea.

> **Observație despre linia de mai sus, nu o reparație a acestei felii.** Mesajul zice «Recepția 247», adică `id_stabil=True`. Pe drumul de ingestie 0058 §4 a pus `id_stabil=False` chiar în aceeași zi, deci serverul care a scris linia **rula codul de dinaintea feliei 0058**. Reparația de la §4 (suma pe indicator — două linii RHR pe același indicator păstrau doar ultima și comparația cădea pe date corecte) **nu era pe server când a apărut respingerea**. Se verifică după ce se împinge codul; până atunci, cazul rămâne deschis.

---

### 1. Ce s-a făcut

#### `PYTHON/utils/asociere_log.py` (nou)

Jurnal separat după utils/timing.py: logger kbot.asociere, asociere.log și handler proprii, propagate=False; fără linii în api_server.log.

Un **bloc pe cerere**, ținut în memorie și scris întreg la sfârșit — două cereri simultane nu-și pot amesteca liniile. Blocul poartă, în ordinea în care le face codul:

| Parte | Ce conține |
|---|---|
| antetul | rută, `dc`, `user`, `cod`, `mod`, statusul HTTP, durata |
| starea | amprenta ȘI cele opt numere din care s-a calculat |
| tabloul | fiecare recepție cu sumele ei pe indicator; fiecare instantaneu cu liniile lui |
| intenția | sugestiile trecerii automate, apoi fiecare hotărâre/comandă cu recepția în care se rezolvă |
| lanțurile | fiecare lanț rezultat, în ordinea datei, exact cum îl văd regulile |
| verdictele | F14 / F15 / F16, fiecare cu cifrele comparate — și cele care TREC, nu doar cea care refuză |
| scrierea | ce s-a scris, sau că nu s-a scris nimic (rollback) |

O respingere se scrie ca `RESPINS` acolo unde s-a întâmplat **și** se repetă la piciorul blocului, iar ultima linie a blocului e un `SUMAR` de o singură linie. Deci o zi întreagă se citește cu `grep RESPINS asociere.log` sau `grep SUMAR asociere.log`, fără blocuri în cale.

Funcțiile publice: `set_enabled`, `enabled`, `traced` (decoratorul de rută), `note`, `section`, `line`, `table`, `refusal`, plus doi ajutori de formatare (`moment`, `sume_pe_indicator`). Toate se întorc pe prima linie când jurnalul e închis; `_write` e prins în `try/except`, ca un jurnal să nu poată strica niciodată o rută.

#### Întrerupătorul

`PYTHON/main.py`, sus de tot, deasupra tuturor celorlalte:

```python
ASOCIERE_LOG_ENABLED = True
asociere_log.set_enabled(ASOCIERE_LOG_ENABLED)
```

`False` + repornirea serverului = nu se deschide niciun fișier și nu se construiește nicio linie. Ordinea în care se decide: **mediul întâi** (`KBOT_ASOCIERE=0`), apoi comutatorul din `main.py`, apoi `ASOCIERE_LOG_ENABLED` din `config.py`, apoi pornit.

Mediul bate `main.py` **dinadins**: jumătate din suita de teste face `from main import app`, deci un comutator care ar fi bătut mediul ar fi aprins jurnalul în fiecare rulare de pytest. `tests/conftest.py` pune `KBOT_ASOCIERE=0` exact ca la cronometru.

Fișierul se mută cu `KBOT_ASOCIERE_LOG` în mediu sau `ASOCIERE_LOG_PATH` în `config.py`. Se rotește la 10 MB, cu 5 copii — ca celelalte două jurnale.

#### Unde s-au pus apelurile

**`routes/forexe/prelucrare_asociere.py`** — inima asocierii, deci acolo e cea mai mare parte:

- `amprenta` — cele opt numere, nu doar hash-ul: când două faze nu se potrivesc, singura întrebare utilă e CARE dintre ele s-a mișcat.
- `citeste_receptii` / `citeste_instantanee` / `citeste_instantanee_context` — tablourile, ca tabele, cu sumele pe indicator pe fiecare rând.
- `pas4c_automat` — candidații după sumă, apoi fiecare potrivire și fiecare ratare, pe ture (tura 1 = de la nou la vechi, tura 2 = restul).
- `verifica_acoperirea` — câte hotărâri pentru câte instantanee, câte lipsesc.
- `materializeaza_reconstituite` — ce recepție se naște din ce lanț și din ce rânduri.
- valideaza_plasarile: per lanț, recepția/sumele, întregul tabel, F14/F16 per instantaneu și F15 cu ambele tablouri de valori alăturate vertical. Jurnalizează atât trecerea, cât și refuzul fiecărei reguli.
- aplica_decizii: tabelul hotărârilor, ancorele 4b și intenția rulării per rând, inclusiv instantaneele deja legate atrase în lanț. F15/F16 pot refuza dintr-un rând neatins de operator, deoarece verifică lanțul întreg; acum acel rând devine vizibil.
- `recalculeaza_final`, `marcheaza_reconstituirile_nesigure` — trecerile `Final`/`Partial` și marcajul F28.

**`routes/forexe/prelucrare.py`** — `@journal.traced("prelucrare")` sub `@timing.timed`, antetul (`dc`, `user`, `cod`, `mod`, rândurile sosite pe tabel), sfârșitul fiecărei căi: amprentă nepotrivită, alegere de unitate, decizii respinse, sarcină de nefolosit, eroare MariaDB, rollback-ul propunerii, commit-ul salvării.

**`routes/forexe/asociere.py`** — `@journal.traced("asociere GET")` și `("asociere POST")`, tabloul citit cu blocajele lui, comenzile ca tabel (cu `IDRR` de acum lângă `IDRR` cerut), demarcarea `Sters`, numărătorile, și fiecare cale de refuz (instantaneu blocat, comandă invalidă, cădere).

---

### 2. Cum arată

Bloc adevărat, scos dintr-o rulare de probă pe funcțiile pure (scriptul e temporar, nu e în depozit):

```
====================================================================================
2026-09-09 17:32:30.932  rulare 1  [proba F15 pe linii]  status 200  0 ms
  dc=000_DEMO  user=proba  cod=AAB2EF2MCP4  mod=salvare
--- verificarea lanțurilor (1 recepții atinse; F15 = veto) -------------------------
  recepția 247 (Recepția din 18.05.2026 · 1200.00) SumaAntet=1200.00  linii=AAB=1150.00
  IDRH | rand | DataH      | Total   | stergere | linii
  -----+------+------------+---------+----------+------------
  9000 | 2    | 12.05.2026 | 900.00  | nu       | AAB=900.00
  9001 | 3    | 20.05.2026 | 1200.00 | nu       | AAB=1200.00
  F14 și F16 trec pe toate cele 2 instantanee ale lanțului
  F15 pe linii: instantaneu AAB=1200.00
                recepție    AAB=1150.00
  F15 CADE pe linii: tablourile de mai sus nu sunt egale
  RESPINS: Recepția din 18.05.2026 · 1200.00: liniile ultimului instantaneu nu se
  potrivesc cu cele ale recepției (AAB: instantaneu 1200.00 / recepție 1150.00). ...
--- ce a refuzat rularea -----------------------------------------------------------
  RESPINS: ...
SUMAR rulare=1 ruta=... status=200 ms=0 respingeri=1 dc=000_DEMO cod=AAB2EF2MCP4
```

(«status 200» fiindcă proba nu e o rută Flask, ci un apel direct: statusul se citește din răspunsul rutei, iar aici nu există unul. Într-o rulare adevărată ar scrie 400.)

Pentru cazul din 14:18 asta ar fi arătat, dintr-o privire: ce recepție, ce lanț s-a format, care instantaneu a fost socotit ultimul, ce indicator nu se potrivea și cu cât.

---

### 3. Fișiere atinse

`PYTHON/utils/asociere_log.py` — **nou**, 316 rânduri. `PYTHON/main.py` — importul, comutatorul `ASOCIERE_LOG_ENABLED` și predarea lui. `PYTHON/routes/forexe/prelucrare_asociere.py` — importul + apelurile de jurnal (niciun rând de logică schimbat; singurul cod nou care nu e jurnal e `_ca_text`, un ajutor de formatare). `PYTHON/routes/forexe/prelucrare.py` — decoratorul, antetul, capetele de drum. `PYTHON/routes/forexe/asociere.py` — decoratoarele pe cele două rute, tabloul, comenzile, capetele de drum. `PYTHON/tests/conftest.py` — `KBOT_ASOCIERE=0`.

---

### 4. Rezultatele rulărilor

- `PYTHON/.venv` suita întreagă, jurnalul ÎNCHIS (cum îl ține `conftest.py`) — **514 trecute / 19 sărite / 0 picate**, neschimbat față de 0058.
- Aceeași suită cu `KBOT_ASOCIERE=1` și `KBOT_ASOCIERE_LOG` într-un fișier temporar — **514 trecute / 19 sărite / 0 picate**, iar fișierul s-a scris (57 KB). Asta acoperă și drumul în care jurnalul e chemat FĂRĂ nicio rulare deschisă (testele cheamă funcțiile pure direct): liniile ies neindentate, dar ies.
- Rulare de probă pe `valideaza_plasarile` și `pas4c_automat` cu jurnalul pornit, cu trei blocuri: F14 cade, F15 cade pe linii, lanț care se închide. Blocul de mai sus e de acolo.

---

### 5. Ce NU s-a verificat

- **Nicio cerere adevărată n-a trecut prin jurnal.** Nu s-a pornit serverul, nu s-a atins baza vie, deci blocul complet — cu tabloul citit din MariaDB și cu antetul de sesiune — n-a fost văzut niciodată. Ce s-a văzut e partea de verificare a lanțurilor, pe date făcute de mână.
- **Cazul operatorului din 14:18 rămâne nediagnosticat.** Jurnalul e unealta, nu răspunsul. Ca să dea răspunsul trebuie: (1) împins codul pe server, (2) repornit serverul cu `ASOCIERE_LOG_ENABLED = True`, (3) refăcută salvarea pe `AAB2EF2MCP4`, (4) citit blocul.
- **Nu s-a măsurat cât încetinește.** Un bloc de propunere pe un angajament mare are câteva sute de rânduri; costul e o listă de șiruri în memorie plus o scriere la sfârșit, dar nu e cronometrat. `forexe_timing.log` va arăta diferența dacă vreodată contează.
- **Nimic nu s-a comis și nimic nu s-a împins.**
