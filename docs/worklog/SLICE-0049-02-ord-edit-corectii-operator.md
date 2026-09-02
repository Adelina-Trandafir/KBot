# SLICE-0049-02 — cele cinci corecții cerute pe editorul de ordonanțare (`OrdEditForm`)

**Pasul al doilea al feliei 0049** (editorul + cele opt rute), după 0049-01 (legarea lui de
«+»-ul din arborele de Plăți). Pasul ăsta nu adaugă nicio pagină și nicio rută nouă — repară ce a
găsit operatorul citind cele trei pagini.

**Trei runde**, toate pe aceeași felie: cele cinci corecții de mai jos, apoi runda a doua
(rândul sintetic doar-citire + unicitatea documentelor) și runda a treia (un singur beneficiar,
numărul următor).

**Cerință operator, 02.09.2026** (citată întocmai):

> - Beneficiari view:
>   1. nothing is filled in the text boxes when a row is selected in the detail dgv, however
>      they are filled when a row is selected in Beneficiari dgv when grupeaza pe clasificatii
>      is not checked.
>   2. on mariadb, in avacont_comun the table BIC exists. use it from there in py
> - Documente justificative view:
>   1. in the right dgv, when the "<TOTI BENEFICIARII>" row (first row) is selected in the
>      Document Justificativ and Fisiere Anexate dgvs MUST appear all common rows for each
>      beneficiar. if a row is added in this view, ALL beneficari must get that value
>   2. the buttons below both dgvs are NOT following the theme colors
> - Atasamente view:
>   1. same as Documente justificative view

**Stare:** cod verde. `dotnet build KBot.sln --no-incremental` **0 erori / 12 avertismente** —
toate 12 sunt `MSB3825` (`ImageStream` / `BinaryFormatter`) pe `.resx`-uri de vedere care existau
dinainte, cifră **identică** cu 0049 și 0049-01. `KBot.App.Tests` **188 trecute / 13 roșii**,
exact aceleași nume ca linia de plecare (DDF / Istoric / XFA), niciunul legat de ORD. Python
**465 trecute / 15 sărite**, identic cu 0049 / 0049-01.

**Nimic n-a atins MariaDB și niciun formular nu s-a deschis pe ecran.** Cele cinci corecții sunt
verificate prin citire și compilare, nu prin rulare.

---

## 1. Beneficiari — campurile de sus urmează și linia selectată

### Ce era

`PartenerulCurent()` răspundea `Nothing` în două situații întregi:

```vb
If _draft Is Nothing OrElse _cheieSelectata = 0 OrElse chkClsf.Checked Then Return Nothing
```

adică pe rândul sintetic «< TOȚI BENEFICIARII >» **și pe toată ramura «grupează pe
clasificații»** — exact cele două moduri în care operatorul umblă prin liniile mai multor
beneficiari deodată. Câmpurile DenBene / CodFiscal / ContIBAN / Banca rămâneau goale și stinse,
deși rândul selectat în grila din dreapta spune fără echivoc al cui e.

### Ce s-a făcut

`PartenerulCurent()` are acum **două surse, în ordine**:

1. lista din stânga, când ea numește un beneficiar (bifa stinsă, rând nesintetic) — calea de
   până acum, neschimbată;
2. altfel, beneficiarul **liniei selectate** în grilă (`OrdDraftLinie.CheiePart`).

Ca să se și miște, `grdLinii.SelectionChanged` cheamă `ActualizeazaAntetBeneficiar()`. Când lista
din stânga numește deja un beneficiar, recalculul dă același răspuns, deci ramura veche nu se
schimbă la nimic.

Două urmări au trebuit tratate, fiindcă beneficiarul editat nu mai e neapărat rândul pe care stă
cursorul din stânga:

- rescrierea numelui în lista din stânga **caută rândul după cheie**, nu-l mai ia pe cel curent;
- pe ramura «clasificații» coloana a doua a grilei arată IBAN-ul, deci editarea lui `ContIBAN`
  rescrie acum și celulele liniilor acelui beneficiar — două locuri care arată același lucru nu
  au voie să se poată contrazice pe ecran (aceeași regulă ca la «Ramas» în felia 0049).

**Fișier:** `src/KBot.App/Views/Ord/OrdBeneficiariPage.vb`.

---

## 2. `BIC` se citește din `AVACONT_COMUN`

### Ce era

Felia 0049 a căutat tabela `BIC` pe baza UNITĂȚII, n-a găsit-o (nu e în
`MariaDB_Schema/000_DEMO.sql`, nu e în `FX_System_Export/TABLES`, nicio rută Python n-o pomenea)
și a tras concluzia scrisă în comentariu: *«`BIC` NU există în MariaDB … trăia în front-end-ul
Access»*. Concluzia era greșită — tabela există, dar pe baza **comună**.

### Ce s-a făcut

`_are_bic` / `incarca_dic_banci` probează și citesc `AVACONT_COMUN`, pe **aceeași conexiune**, cu
numele bazei în față:

```sql
SELECT Cod, Banca FROM `AVACONT_COMUN`.`BIC`
```

`COMMON_DB` se importă din `utils.database` (nu se scrie a doua constantă cu același conținut), iar
tiparul citirii încrucișate e cel pe care îl folosea deja `schema_sync/schema_common.py` pentru
`` `AVACONT_COMUN`.`CAI` ``. Cele două nume de coloane (`Cod`, `Banca`) sunt cele din VBA —
`mdl_FX_ORD.Incarca_DicBanci`: `SELECT Cod, Banca FROM BIC` — nu ghicite.

Proba pe `information_schema` **rămâne**, mutată pe baza comună, și i s-a adăugat o a doua plasă:
tabela poate exista fără ca acest cont să aibă drept de citire pe ea, caz în care se avertizează
și se continuă. `Banca` e câmp informativ, nu obligatoriu — nicio generare nu se oprește din
cauza lui. Memoria probei e acum una singură per proces (tabela e una singură), nu una per bază.

**Fișier:** `PYTHON/routes/forexe/ord_edit.py`.

---

## 3. «< TOȚI BENEFICIARII >» arată ce au TOȚI, și ce se adaugă acolo îl primesc TOȚI

### De ce grila era goală

Portul 0049 a luat literal filtrul Access — `TIDORDPART IS NULL` — deci pe rândul sintetic se
vedeau doar documentele fără legătură de beneficiar. Dar **generarea nu produce niciodată așa
ceva**: `Adauga_Ord_Doc`, portată în `ord_edit.construieste_graf`, agață fiecare document de
beneficiarul liniei (`"part_temp_id": part["temp_id"]`). Pe date proaspăt generate rândul sintetic
arăta, prin construcție, o grilă goală.

Intenția cerută de operator există în Access, **comentată**, chiar în `frmFX_ORD_DOC.Form_Load`:

```vba
'SELECT tmpFX_ORD_DOC.DocJust, tmpFX_ORD_DOC.TipDoc, tmpFX_ORD_DOC.NumeDoc FROM tmpFX_ORD_DOC
' GROUP BY … HAVING Count(*)=(SELECT COUNT(*) FROM (SELECT DISTINCT IDORDPART FROM tmpFX_ORD_DOC))
```

adică «rândurile pe care le au TOȚI beneficiarii». Scrisă, dar niciodată pusă în folosință.

### De ce fan-out și nu o legătură NULL

Un singur rând cu `IDORDPARTP` NULL ar fi fost mai ieftin, dar **nu s-ar vedea**: calea de citire
a vederii 0033 (`routes/forexe/ord.py`) aduce documentele prin

```sql
(SELECT GROUP_CONCAT(DISTINCT d.DocJust …) FROM FX_ORD_DOC d WHERE d.IDORDPARTP = t.IDORDPARTP)
```

deci un rând fără legătură nu apare la niciun beneficiar. «ALL beneficiari must get that value»
înseamnă, în datele astea, **câte un rând per beneficiar**.

### Ce s-a făcut

Ambele pagini («Documente justificative» și «Atașamente») lucrează acum cu **grupuri**: un grup =
un rând pe ecran, iar rândul din grilă poartă în `Tag` **lista** rândurilor de date pe care le
reprezintă.

- **pe un beneficiar** — cele fără legătură ȘI ale lui, fiecare într-un grup de unul singur
  (comportamentul de până acum, neschimbat);
- **pe rândul sintetic** — două înțelesuri de «comun», amândouă arătate:
  1. rândurile fără legătură de beneficiar (cum le ținea Access);
  2. rândurile care există, cu aceeași valoare, la **fiecare** beneficiar, strânse într-un
     singur rând pe ecran.

Cheia de valoare e `DocJust + NumeDoc + TipDoc` la documente (exact cele trei coloane pe care le
grupa interogarea Access) și `NumeFisier + Sha256 + Dimensiune` la atașamente.

**Straturile.** Un grup de valoare nu dă un rând, ci atâtea câte poate susține cel mai puțin
bogat beneficiar (`min` peste numărul de copii ale fiecăruia). Fără asta, două rânduri identice
adăugate pe rândul sintetic s-ar fi contopit într-unul singur și al doilea «Adaugă» n-ar fi părut
să facă nimic.

**Ce urmează din asta, peste tot la fel:**

| acțiune pe rândul sintetic | ce se întâmplă |
|---|---|
| adaugă | câte un rând per beneficiar (fără niciun beneficiar: unul singur, cu legătură NULL) |
| editează | valoarea se scrie în TOATE cele N copii |
| șterge | se șterg TOATE cele N copii |

Refuzul din Access (`dtnDel_Click`: un document al întregii ordonanțări nu se șterge cât timp e
selectat un beneficiar anume) **rămâne**, mutat pe grup.

**Fișiere:** `src/KBot.App/Views/Ord/OrdDocumentePage.vb`,
`src/KBot.App/Views/Ord/OrdAtasamentePage.vb`.

### Costul asumat, la atașamente

N copii ale aceleiași imagini înseamnă **N urcări** după salvare (octeții urcă per `IDORDATTP`,
faza a doua din felia 0049). Alternativa — o singură imagine cu legătură NULL — n-ar fi văzută de
calea de citire, deci nu e o alternativă. Consemnat în nota de clasă a paginii.

---

## 4. Un defect găsit în trecere: rândul text nou apărea în grila FIȘIERELOR

`GrupuriVizibile` a moștenit de la `DocumenteVizibile` împărțirea pe `OrdDraftDoc.EsteText`, care
cere `NumeDoc` gol **și** `DocJust` necompletat. Un rând text proaspăt adăugat are `DocJust = ""`,
deci `EsteText = False`, deci ateriza în grila fișierelor — acolo unde operatorul tocmai nu se
uită. Butonul «Adaugă rând» părea că nu face nimic.

Împărțirea între cele două grile se face acum pe `NumeDoc` **singur**, ca în Access
(`Form_Load`: `If IsNull(!NumeDoc)` ▸ temp1 text, altfel temp2 fișiere). `EsteText` rămâne ce este —
proba de VALIDARE din `btnSav` (`IsNull(NumeDoc) And Not IsNull(DocJust)`), folosită de
`OrdEditForm.Valideaza`. Nu s-a atins.

---

## 5. Butoanele nu urmau tema — și de ce

Cauza e o regulă a motorului, nu o scăpare de culoare. `ThemeManager.Traverse` se oprește din
regulile GENERICE de îndată ce dă peste un `IThemedControl` și coboară mai departe **doar** în
copiii care sunt și ei `IThemedControl` (`ApplyToNestedThemed`). Cele trei pagini SUNT
`IThemedControl`; grilele dinăuntru sunt și ele, deci se colorau; `Button`-urile simple de sub ele
nu sunt, deci rămâneau gri de sistem sub orice schemă.

Reparația e cea a casei: `ButtonStyles`, chemat din `ApplyTheme`-ul fiecărei pagini — «Adaugă»
poartă accentul (`ApplyPrimary`), «Șterge» și «Lipește» rămân secundare (`ApplySecondary`), fiindcă
un buton distructiv nu se îmbracă în culoarea care cheamă degetul.

Motivul e scris în comentariul de deasupra fiecărui `ApplyTheme`, ca următorul control care ajunge
aici să nu mai reia diagnosticul.

---

## Fișiere atinse

| Fișier | Ce |
|---|---|
| `src/KBot.App/Views/Ord/OrdBeneficiariPage.vb` | câmpurile urmează linia selectată; rescrierea numelui pe cheie; IBAN-ul se rescrie și în grilă |
| `src/KBot.App/Views/Ord/OrdDocumentePage.vb` | grupurile; fan-out la adăugare; editare/ștergere pe grup; împărțirea pe `NumeDoc`; butoanele tematizate |
| `src/KBot.App/Views/Ord/OrdAtasamentePage.vb` | aceleași, plus previzualizarea pe primul din grup; butoanele tematizate |
| `PYTHON/routes/forexe/ord_edit.py` | `BIC` din `AVACONT_COMUN`, cu probă și a doua plasă pe drepturi |

Niciun `.Designer.vb` n-a fost atins: niciun control nou, nicio proprietate de așezare schimbată.
`src/KBot.Domain/OrdDraft.vb` neatins — modelul de date suportă deja și legătura NULL, și legătura
pe beneficiar.

---

## Rezultate

| Ce | Rezultat | Linie de plecare (0049 / 0049-01) |
|---|---|---|
| `dotnet build KBot.sln --no-incremental` | 0 erori / 12 avertismente (`MSB3825`, preexistente) | identic |
| `KBot.App.Tests` | 188 trecute / 13 roșii | identic, aceleași nume (DDF/Istoric/XFA) |
| Python (`PYTHON/.venv`, suita întreagă) | 465 trecute / 15 sărite | identic |

`KBot.Controls`, `KBot.Api`, `KBot.Domain` neatinse, deci nerulate.

---

## Rămâne neverificat / amânat

1. **⚠ Nimic nu s-a văzut pe ecran.** Cele trei pagini n-au fost deschise nici la rulare, nici în
   designerul Visual Studio — la fel ca în 0049 și 0049-01. Culorile butoanelor sunt cerute prin
   `ButtonStyles`, dar n-au fost PRIVITE sub cele trei scheme.
2. **⚠ `AVACONT_COMUN`.`BIC` n-a fost interogată.** Existența tabelei e spusă de operator, nu
   citită de aici; numele coloanelor (`Cod`, `Banca`) vin din VBA. Prima rulare pe o bază vie e
   proba. Dacă tabela stă acolo dar contul K-BOT n-are drept pe ea, generarea merge înainte cu un
   avertisment — pe drumul adăugat în felia asta, netestat.
3. **⚠ Regruparea «comun după valoare» n-a fost văzută pe date reale.** Pe date generate,
   documentele sunt explicații de linie, deci DIFERITE de la un beneficiar la altul: rândul
   sintetic va fi, cel mai probabil, tot gol până când operatorul adaugă ceva pe el. Asta E
   comportamentul cerut («arată ce au toți»), dar merită văzut o dată alături de operator, ca
   să nu fie luat drept aceeași grilă goală de dinainte.
4. **Fără teste noi**, ca în 0049 și 0049-01 (cerința operatorului). `GrupuriVizibile` e o funcție
   pură peste `OrdDraft` și s-ar putea proba fără interfață, dacă se cere mai târziu.
5. **Neatins, deliberat:** pagina «Fișiere» și bifele de selecție multiplă din editor (amânate
   încă din 0049), generarea PDF a ordonanțării (rămâne fratele DDF-ului) și `routes/ord/*`
   (clientul Access legacy).

---

# Runda a doua — rândul sintetic devine doar-citire, iar documentele devin unice pe beneficiar

Două cerințe noi ale operatorului, primite după ce pasul de mai sus era gata (02.09.2026):

> 1. când e selectat rândul `<TOȚI..>` să arate o grupare peste toate rândurile comune din
>    fiecare grilă — needitabilă — și să lase operatorul să adauge rânduri comune.
> 2. la construirea «Documentelor Justificative» ele trebuie să fie unice per beneficiar; dacă
>    parserul găsește duplicate, să scrie doar unul și să le arunce pe celelalte.

## 6. Rândul sintetic nu se mai editează în grilă

### De ce

Gruparea o făcuse deja pasul de mai sus: un rând de ecran de pe «< TOȚI BENEFICIARII >» stă pe N
rânduri de date, câte unul la fiecare beneficiar. Editarea lui însemna, prin construcție, o
scriere în toate N deodată — un gest mic, cu urmări pe care operatorul nu le vede în clipa în
care le face și pe care nu le poate întoarce. Cerința taie exact asta: acolo se **citește** și
se **adaugă**; corecturile se fac pe beneficiarul lor, unde rândul e al lui singur.

### Ce s-a făcut

`KBotDataView` avea deja `ReadOnlyGrid` (proprietate de designer, folosită de grila
beneficiarilor și de cea a fișierelor), deci nu s-a scris niciun mecanism nou:

- `OrdDocumentePage.ReumpleListele` pune `grdText.ReadOnlyGrid = (_cheieBene = 0)`.
  `grdFisiere` era oricum doar-citire — numele și extensia vin din fișierul ales, nu de la tastatură.
- `OrdAtasamentePage.ReumpleLista` pune la fel `grdAtasamente.ReadOnlyGrid`.
- Ca doar-citirea să nu pară o grilă stricată, titlul primește un sufix cât ține rândul sintetic:
  «Documente justificative (text)  ·  rânduri comune (doar citire)». Titlurile din designer se
  păstrează în constructor (`_titluText` / `_titluFisiere` / `_titluLista`) și se pun la loc
  neschimbate când se alege un beneficiar — nu se rescriu cu constante, ca să nu se desincronizeze
  de `.Designer.vb`.

### Adăugarea rămâne deschisă — deci textul se cere în dialog

Un rând text adăugat gol într-o grilă doar-citire n-ar mai putea fi completat niciodată. De aici
formularul nou **`OrdTextForm`** (`Views/Ord/`, `KBotThemedForm` + `.Designer.vb`, ca `OrdZiuaForm`):
bandă de titlu, un `TextBox` multilinie, «Renunță» / «Adaugă». Introducerea spune ce urmează —
«se adaugă la TOȚI beneficiarii, câte o copie fiecăruia» pe rândul sintetic, altfel «pentru
beneficiarul selectat». **N-are `AcceptButton`**: câmpul e multilinie, deci Enter trebuie să treacă
la rândul următor, nu să închidă dialogul. Textul gol se refuză în dialog, nu la salvare.

`btnAdaugaText` cheamă dialogul pe AMBELE drumuri, nu doar pe cel sintetic: pe un beneficiar
rândul se năștea gol și nu se deosebea de unul uitat, iar de la validarea lui `btnSav` încolo un
document fără text oprește salvarea. Adăugarea de fișiere și cele două adăugări de pe pagina
«Atașamente» n-au nevoie de dialog — ele își iau valorile din fișierul ales sau din memoria
temporară.

## 7. Un document justificativ e unic la un beneficiar

### Unde se năștea duplicatul

`Adauga_Ord_Doc` scria, în Access, **un document pentru fiecare rând-sursă**, iar textul lui e
`Descriere`-a plății făcută majuscule. Trei plăți ale aceluiași beneficiar cu aceeași descriere
dădeau deci trei rânduri identice: nedeosebite pe ecran, fără nimic în plus în PDF, și — de la
pasul de mai sus încoace — fiecare cu propriul rând în grupare. Portul din 0049 păstrase
comportamentul literal.

### Ce s-a făcut, pe trei niveluri

| Unde | Ce face |
|---|---|
| `construieste_graf` (generarea) | ține un set `(beneficiar, DocJust, NumeDoc, TipDoc)`; al doilea rând cu aceeași cheie nu se mai propune. Se numără și se scrie în jurnal, **nu** în avertismentele operatorului: se întâmplă la aproape orice zi cu plăți la fel și nu-l pune să facă nimic |
| `_fara_duplicate_doc` (salvarea) | aceeași cheie, pe ce trimite clientul. Ordonanțările scrise înainte de regulă au încă duplicate și intră înapoi pe aici la prima salvare. Dintre două rânduri identice rămâne cel care **există deja** în tabelă (`IDORDDOCP > 0`), ca să nu se șteargă o cheie vie doar ca să se scrie alta la loc cu același conținut |
| `OrdDocumentePage` (editorul) | `AdaugaRanduri` **sare peste** beneficiarii care au deja valoarea, iar `GrdText_CellValueChanged` refuză o editare care ar face două rânduri identice la același beneficiar și pune înapoi textul dinainte |

Sărirea la adăugare nu e o refuzare a întregii adăugări, și asta e intenționat: pe un rând
aproape-comun, căruia îi lipsește valoarea la doi beneficiari din zece, exact asta umple golurile
și îl face comun. Când n-a mai rămas nimic de adăugat, se spune («Toți beneficiarii au deja…»);
la fișiere se enumeră care s-au sărit.

Rândurile REALE aruncate la salvare se șterg prin `_sterge_absentii`, deci răspunsul le numără
deja în `sterse["documente"]` — nu s-a inventat niciun câmp nou în contract.

**Strângerea pe straturi din `GrupuriVizibile` rămâne** deși duplicatele nu se mai nasc: datele
vechi le au, iar un draft încărcat de pe server trece prin aceeași grilă.

## Ce NU s-a atins

- **Atașamentele n-au primit regula de unicitate.** Cerința vorbește despre «Documente
  Justificative»; două capturi identice la același beneficiar rămân posibile. Fan-out-ul lor nu e
  un duplicat — unicitatea e *per beneficiar*, iar acolo e câte o copie de fiecare.
- `EsteText` a rămas ce e (proba de validare a lui `btnSav`), ca în pasul de mai sus.

## Fișiere atinse (runda a doua)

| Fișier | Ce |
|---|---|
| `src/KBot.App/Views/Ord/OrdTextForm.vb` + `.Designer.vb` | **nou** — dialogul care cere textul documentului justificativ |
| `src/KBot.App/Views/Ord/OrdDocumentePage.vb` | doar-citire pe rândul sintetic + sufixul din titlu; adăugarea prin dialog; unicitatea la adăugare și la editare |
| `src/KBot.App/Views/Ord/OrdAtasamentePage.vb` | doar-citire pe rândul sintetic + sufixul din titlu |
| `PYTHON/routes/forexe/ord_edit.py` | fără duplicate la generare; `_fara_duplicate_doc` înainte de scriere |

`KBotDataView` neatins: `ReadOnlyGrid` exista deja.

## Rezultate (runda a doua)

| Ce | Rezultat | Linie de plecare |
|---|---|---|
| `dotnet build KBot.sln --no-incremental` | 0 erori / 12 avertismente (`MSB3825`, preexistente) | identic |
| `KBot.App.Tests` | 188 trecute / 13 roșii | identic, aceleași nume (DDF/Istoric/XFA) |
| Python (`PYTHON/.venv`, suita întreagă) | 465 trecute / 15 sărite | identic |

## Rămâne neverificat (runda a doua)

1. **⚠ Tot nimic nu s-a văzut pe ecran.** `OrdTextForm` e un formular NOU care n-a fost deschis
   niciodată — nici la rulare, nici în designerul Visual Studio. Așezarea lui e scrisă la 96 dpi
   după tiparul lui `OrdZiuaForm`, dar n-a fost privită.
2. **⚠ Doar-citirea n-a fost probată la mână.** `ReadOnlyGrid` e proprietatea grilei, iar
   `KBotDataView.Editing` o citește la începutul editării; comutarea ei în timpul vieții
   controlului n-a fost încercată pe ecran.
3. **⚠ Aruncarea duplicatelor la salvare ȘTERGE rânduri reale** din ordonanțările vechi, prin
   cascada obișnuită. E ce s-a cerut, dar prima salvare a unei ordonanțări vechi cu duplicate n-a
   fost făcută pe o bază vie.
4. **Fără teste noi**, ca în tot restul feliei.

---

# Runda a treia — un singur beneficiar nu mai are nevoie de rândul «toți», iar numărul se poate întreba dinainte

Cerut de operator, 03.09.2026, în trei puncte:

> deși `BIC` există 100% în `AVACONT_COMUN`, în formularul FxORD tot văd mesajul că nu există.
> am împins deja codul py pe server, deci nu asta e problema. până la urmă am scos condiția din
> PY care verifică tabela `BIC`
>
> 1. dacă e un singur beneficiar/clasificație → ascunde celula `tly` cu «Grupează pe
>    clasificații», și scoate «< TOȚI BENEFICIARII >» din toate grilele, în toate paginile.
> 2. să existe o cale de a afla numărul următor posibil al ordonanțării. înțeleg riscul ca doi
>    operatori deodată să primească același număr posibil, dar pune un sfat pe eticheta aia care
>    să-i explice omului ce-i cu numărul. până la urmă numărul REAL se alocă tot cum se alocă
>    acum — de server, la salvare.

## 8. De ce spunea «tabela `BIC` nu s-a găsit» deși tabela era acolo

Proba dinainte de citire nu întreba tabela, ci catalogul:

```sql
SELECT COUNT(*) FROM information_schema.TABLES
 WHERE TABLE_SCHEMA = 'AVACONT_COMUN' AND TABLE_NAME = 'BIC'
```

`information_schema.TABLES` **nu e o listă a bazei, ci o listă a ceea ce vede contul curent**:
MariaDB arată acolo doar obiectele pe care contul are măcar un drept. Contul cu care merge
serverul nu avea drept pe `AVACONT_COMUN`.`BIC`, deci proba răspundea cinstit «0» — iar noi
citeam din acel «0» concluzia greșită, că tabela lipsește. Tabela există; ea nu era a contului.

Așa că proba a plecat de tot (operatorul o scosese deja pe server; aici s-a curățat și restul:
`_SQL_ARE_BIC`, `_are_bic_cache`, `_are_bic` și funcția veche lăsată în comentariu). Se încearcă
de-a dreptul `SELECT`-ul, iar **eșecul lui** e singurul răspuns de încredere — el nu poate spune
«lipsește» despre o tabelă care e acolo și se poate citi.

Un defect adus de varianta scoasă în grabă, reparat aici: pe o tabelă existentă dar **goală**
funcția ieșea prin capătul de jos și întorcea `None`, nu `{}` — adică o excepție la primul
`dic_banci.get(...)`. Acum tabela goală dă dicționar gol și **niciun** avertisment: un
nomenclator nepopulat nu e o eroare.

**Ce rămâne de făcut pe server, și nu se poate face din cod:** dacă numele băncilor tot nu se
completează, contul are nevoie de dreptul de citire —
`GRANT SELECT ON AVACONT_COMUN.BIC TO '<contul serverului>'@'%'`.

## 9. Cu un singur beneficiar, rândul sintetic și bifa se retrag

Amândouă sunt comutatoare între două vederi. Când vederile ar arăta același lucru, comutatorul
nu ajută, ci întreabă: «pe care dai clic?».

| Suprafață | Ce se retrage | Când |
|---|---|---|
| «Beneficiari», lista din stânga | rândul «< TOȚI BENEFICIARII >» / «< TOATE CLASIFICAȚIILE >» | lista are **exact o** intrare |
| «Beneficiari», bifa | banda cu «Grupează pe clasificații», strânsă la înălțime 0 | un singur beneficiar **și** o singură clasificație |
| «Documente justificative», grila beneficiarilor | rândul «< TOȚI BENEFICIARII >» | ordonanțarea are **exact un** beneficiar |
| «Atașamente», grila beneficiarilor | la fel | la fel |

Trei hotărâri de care atârnă restul:

**Proba e pe UNU, nu pe «cel puțin doi».** Fără niciun beneficiar rândul sintetic e singurul
lucru de care se poate agăța cursorul, deci acolo rămâne. Se retrage numai la exact unul.

**Bifa rămâne pe un singur beneficiar cu mai multe clasificații.** Acolo ea chiar desface
liniile; ar fi fost o pierdere de funcție, nu o curățenie. Se retrage doar când ambele
dimensiuni au cel mult o valoare.

**Refuzul «documentul nu e al beneficiarului curent» se retrage odată cu rândul sintetic.**
Mesajul trimitea operatorul să selecteze «< TOȚI BENEFICIARII >» — un rând care acum nu mai
există. Și n-ar mai avea de ce să refuze: cu un singur beneficiar, «al întregii ordonanțări» și
«al lui» sunt același lucru. Fără această a treia hotărâre, documentele fără legătură ale
ordonanțărilor vechi ar fi rămas **de neșters**.

Consecința bună, pe lângă: cu un singur beneficiar `_cheieBene` nu mai e niciodată 0, deci
grilele nu mai trec pe doar-citire (runda a doua) și titlurile nu mai poartă sufixul
«· rânduri comune (doar citire)». Adăugarea scrie tot un singur rând, al lui.

Ascunderea benzii nu se uită la `chkClsf.Visible`: **getter-ul răspunde `False` cât timp
formularul însuși e încă nearătat** (dă vizibilitatea *efectivă*, nu steagul controlului), iar
prima așezare se face tocmai atunci, din `Form_Load`. Hotărârea se ține într-un câmp propriu.
Înălțimea benzii se reține la ascundere, nu se rescrie de la 96 dpi: `TableLayoutPanel` o
scalase deja pentru ecranul de față (regula DPI a casei).

## 10. Numărul următor: o presupunere care spune că e presupunere

`GET /api/forexe/ord/nr-urmator` → `{ "nr_ord": N }`, unde N e `COALESCE(MAX(NrORD), 0) + 1`.

**Fără `FOR UPDATE`** — și asta e toată deosebirea față de alocarea adevărată. Lacătul din
`_scrie_graf` ține cât ține tranzacția de salvare, adică o clipă; un lacăt luat aici ar ține cât
editează operatorul și ar opri restul casei din salvat. Deci ruta nu rezervă nimic, iar între
întrebare și salvare altcineva poate salva primul.

Numărul REAL se alocă exact unde se aloca și înainte: în tranzacția de salvare. Nimic din calea
aceea nu s-a atins.

Ca să nu se citească drept număr, cifra își poartă cuvântul în text — `probabil 137`, nu `137` —
și eticheta are sfatul cerut, care spune pe șleau ce e și ce nu e, plus că se poate da clic pe ea
ca să întrebe serverul din nou. Se întreabă o dată la deschidere (în tăcere: un eșec nu merită să
oprească editarea) și de câte ori vrea operatorul. Pe o ordonanțare deja salvată nu se întreabă
nimic — numărul ei e alocat, iar eticheta arată numărul, fără deget de mână pe cursor.

`IApiClient` a primit `GetOrdNrUrmatorAsync`, deci cele nouă clase de probă din
`KBot.App.Tests` au primit fiecare câte un ciot care aruncă `NotSupportedException` —
contabilitate de compilare, nu acoperire nouă.

## Ce NU s-a atins

- **`OrdVizualizarePage`** (vederea 0033, doar-citire) își păstrează «<TOȚI BENEFICIARII>».
  Acolo e o intrare de combo, nu un rând de grilă, iar cererea vorbea despre editor.
- **Alocarea numărului la salvare.** Neschimbată, dinadins.
- **Regula de unicitate a documentelor** (runda a doua). Neatinsă.

## Fișiere atinse (runda a treia)

| Fișier | Ce |
|---|---|
| `PYTHON/routes/forexe/ord_edit.py` | proba `information_schema` scoasă + `None` reparat; rută nouă `ord/nr-urmator` |
| `src/KBot.Api/OrdEditContract.vb` | `OrdNrUrmatorResponse` |
| `src/KBot.Api/IApiClient.vb` + `ApiClient.vb` | `GetOrdNrUrmatorAsync` |
| `src/KBot.App/MainForm.vb` | a cincea specializare `WithReauth(Of Integer)` |
| `src/KBot.App/Views/Ord/OrdEditForm.vb` + `.Designer.vb` | numărul presupus, clic pe etichetă, sfatul cerut |
| `src/KBot.App/Views/Ord/OrdBeneficiariPage.vb` | `IntrarileListei`, `AplicaVizibilitateaBifei`, rândul sintetic condiționat |
| `src/KBot.App/Views/Ord/OrdDocumentePage.vb` + `.Designer.vb` | `AreRandSintetic`, refuzul ștergerii condiționat, sfatul grilei |
| `src/KBot.App/Views/Ord/OrdAtasamentePage.vb` | `AreRandSintetic`, refuzul ștergerii condiționat |
| `tests/KBot.App.Tests/*.vb` (9 fișiere) | ciotul noii metode de interfață |

## Rezultate (runda a treia)

| Ce | Rezultat | Linie de plecare |
|---|---|---|
| `dotnet build KBot.sln --no-incremental` | 0 erori / 12 avertismente (`MSB3825`, preexistente) | identic |
| `KBot.App.Tests` | 188 trecute / 13 roșii | identic, aceleași nume (DDF/Istoric/XFA) |
| Python (`PYTHON/.venv`, suita întreagă) | 465 trecute / 15 sărite | identic |

## Rămâne neverificat (runda a treia)

1. **⚠ Tot nimic nu s-a văzut pe ecran.** Banda strânsă la 0, eticheta cu «probabil N» și
   grilele fără rândul sintetic n-au fost privite — nici la rulare, nici în designer.
2. **⚠ Ruta `ord/nr-urmator` n-a fost chemată pe o bază vie.** `MAX(NrORD) + 1` e aceeași
   formulă cu cea din salvare, dar răspunsul n-a fost văzut.
3. **⚠ Dreptul pe `AVACONT_COMUN`.`BIC` n-a fost verificat.** Dacă numele băncilor tot lipsesc,
   `GRANT SELECT` e pasul următor — pe server, nu în cod.
4. **Fără teste noi**, ca în tot restul feliei.
