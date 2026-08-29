# SLICE-0048-04 — editorul de legături R ▸ H, disponibil ORICÂND

**Cerută de operator, 29.08.2026:**

> «nu văd nicio schimbare la vederea recepții unde să pot deschide formularul popup ca să editez
> legăturile dintre R și H. trebuie să pot face asta oricând, nu doar când sunt date noi.»
>
> «citește din `FX_System_Export/FORMS` formularele dubii din Access. acolo e logica. dacă există
> ordonanțări bazate pe plăți în R sau H, sau plăți în acele date, legăturile nu vor fi editabile,
> dar rămân vizibile»

**Stare:** cod verde, build 0 erori / 0 avertismente. **Nimic nu a rulat pe MariaDB, formularul
nu a fost văzut niciodată pe ecran și nici deschis în designerul Visual Studio.**

---

## 0. Ce era înainte, și de ce nu era o regresie

Formularul de asociere **nu exista**. Nu s-a pierdut și nu s-a stricat nimic: `D-A` din
`docs/FUNDAMENT_Asociere_Receptii.md` împarte felia 0048 anume așa — **0048-03** = conducta plus
contractul în două faze, coordonatorul deliberat nelegat; **0048-04** = tragerea în
`AdvancedTreeControl`, formularul și legarea, împreună. Până azi, `ReceptiiView.Designer.vb`
declara cinci controale și niciun buton, iar `src/KBot.Controls/Tree/` nu avea niciun rând de cod
de tragere.

Felia asta e 0048-04, plus cerința nouă (deschiderea oricând + regula de blocare).

---

## 1. Ce s-a citit în Access, și ce NU e acolo

### 1.1 Gazda «oricând» există în Access și **lipsește din export**

`frmFX_DUBII_LISTA_HA.Form_Open` și `frmFX_DUBII_LISTA_RH.Form_Open` se ramifică identic:

```vba
If isLoaded("frmFX_ASOC") Then
    ... WHERE tmpFX_Receptii_H.TmpIDRecR=[forms]![frmFX_ASOC]![frmFX_ASOC_SUB]![ID_R]
Else
    ... WHERE tmpFX_Receptii_H.TmpIDRecR=[forms]![frmFX_DUBII]![ID_R]
End If
```

Deci Access avea **două gazde peste aceleași patru subformulare**: `frmFX_DUBII` în timpul
ingestiei și `frmFX_ASOC` oricând — exact ce a cerut operatorul. `mdl_FX_Popups.md:888` adaugă o a
treia referință, `Forms!frmFX_ASOC!frmFX_ASOC_SUB!frmFX_DUBII_P_LISTA_R` (o listă soră, de plăți).

**`frmFX_ASOC`, `frmFX_ASOC_SUB` și `frmFX_DUBII_P_LISTA_R` NU sunt în
`FX_System_Export/FORMS`** — acolo sunt doar cele cinci fișiere `frmFX_DUBII*`. Codul gazdei
«oricând» nu poate fi citit. Cele patru subformulare SUNT exportate și ele poartă regulile, deci
pierderea e **aspectul gazdei, nu logica**. Aspectul formularului de aici e prin urmare
**PROIECTAT, nu portat** — la fel ca `AlegereUnitateForm` în 0048-02, și din același motiv.

### 1.2 Ce fac cele patru panouri

| Panou | Sursă | Ce face |
|---|---|---|
| `_LISTA` (stânga sus) | `tmpFX_Receptii_R` | recepțiile. Clic pe rând ridică `RowChanged(ID)`; gazda scrie `ID_R` și recheamă celelalte trei. Fără ștergere, fără tragere |
| `_LISTA_RH` (stânga jos) | `tmpFX_Receptii_RHR WHERE TmpID = ID_R` | liniile curente ale recepției selectate, `ORDER BY CodSSI`, cu total `Sum(Valoare)` |
| `_LISTA_HA` (dreapta sus) | instantaneele deja atașate lui `ID_R` | `btnDel` desprinde unul: `SET TmpIDRecR=Null, TipIntern=Null`; dacă rândul scos era `Final`, re-promovează ultimul rămas la `Final`/`EDIT`; apoi `FX_CalculeazaDIF_Receptii_Tmp` |
| `_LISTA_HN` (dreapta jos) | `tmpFX_Receptii_H WHERE TmpIDRecR Is Null` | combo-ul cu toate recepțiile (`NrCrt / DataR / SumaAntet`) **este** acțiunea de așezare; plus caseta `Sters` (F17), care golește `DIFH`/`DIFHC` pe antet și `DIF`/`DIFC` pe linii |

`frmFX_DUBII.btnSav_Click` refuză salvarea cât timp există un instantaneu cu
`TmpIDRecR IS NULL AND Sters=False`.

### 1.3 O divergență portată deliberat, consemnată ca să nu fie «reparată»

`_LISTA_HN.Form_BeforeUpdate` calculează `TipReceptie` **per plasare**: fiecare instantaneu mai
vechi al acelei recepții devine `PARTIAL`, iar cel plasat devine `FINAL` doar dacă TOATE sunt mai
vechi. Felia 0048-03 a înlocuit asta cu «`Final` = cel mai TÂRZIU după `DataH`, recalculat o dată
per recepție», fiindcă regula Access promovează orice tocmai a fost atașat — iar sub plasare
manuală asta ar urca un instantaneu din ianuarie peste unul din mai. **Regula 0048-03 rămâne**;
nu s-a re-portat cea din Access.

---

## 2. Regula de blocare

### 2.1 Access o avea, și e COMENTATĂ, pe un tabel acum mort

`frmFX_DUBII_LISTA_HA.btnDel_Click`:

```vba
'If Nz(Me!origidrh, 0) > 0 Then
'    If DCount("IDRP", "FX_Receptii_Plati", "IDRH=" & Me!origidrh) <> 0 Then
'        MsgBox "Acest rand are Plati / Incasari asociate! Nu mai poate fi dez-asociat!", vbCritical
```

Cheiată pe **`IDRH` — instantaneul, nu recepția** — prin `FX_Receptii_Plati`, pe care corecția C2
din fundament îl declară GOL și scos complet din migrare. Regula supraviețuiește, sursa ei de date
nu, deci s-a re-cheiat pe tabelele vii.

### 2.2 Ce oferă schema vie (citit în `MariaDB_Schema/000_DEMO.sql`)

* **`FX_ORD.IDRR` și `FX_ORD.IDRH`** — amândouă există, amândouă nullable, comentate
  «PK ACCESS FX_Receptii_R / _H». `FX_Receptii_R.IDRR` și `FX_Receptii_H.IDRH` sunt
  `int(11) NOT NULL PRIMARY KEY` (cheile Access păstrate), deci se leagă direct. Scrise de
  `mdl_FX_ORD_Salvare` la liniile 284 și 362, marcate «v5» / «v6».
* **Plățile consumate de o ordonanțare**:
  `FX_ORD → FX_ORD_TBL (IDORDP) → FX_ORD_TBL_REC (IDORDTBLP) → FX_Plati (IdPlataFX)`, ambele
  salturi fiind constrângeri FK reale.
* ⚠ **`FX_ORD_TBL.IDRR` NU EXISTĂ pe MariaDB.** Access îl are; nu s-a migrat. Legătura
  ordonanțare ▸ recepție trăiește doar la nivel de CAP de ordonanțare, niciodată pe linie.
* ⚠ **În exportul Access, TOATE rândurile `FX_ORD` poartă `IDRR = 0, IDRH = 0`.** Pe date de
  vechimea aceea jumătatea «ordonanțare» a regulii nu găsește nimic, și tot blocajul se sprijină
  pe jumătatea «plăți». De-aia jumătatea aia trebuia să fie bine definită — și de-aia s-a întrebat
  operatorul în loc să se ghicească.

### 2.3 Ce a hotărât operatorul (29.08.2026)

| Întrebare | Răspuns |
|---|---|
| Care plăți îngheață o legătură? | **Orice plată a angajamentului cu `Data_plata >= DataH`** a instantaneului. Motivul e §1.3 din fundament: fiecare plată de după acea dată citește totalul recepției așa cum stătea atunci |
| Cât se îngheață? | **Doar instantaneul atins.** Restul lanțului aceleiași recepții rămâne editabil |

Regula, ca funcție PURĂ (`routes/forexe/asociere.py:motive_blocare`), peste un rând de
`_BLOCAJE_SQL`. Un instantaneu ASOCIAT e blocat dacă oricare dintre:

1. există `FX_ORD` cu `IDRH = H.IDRH` — o ordonanțare s-a construit chiar pe el;
2. există `FX_ORD` cu `IDRR = H.IDRR` și `DataORD >= H.DataH` (sau `DataORD IS NULL`);
3. există `FX_Plati` pe angajament cu `Data_plata >= H.DataH`.

Motivele se întorc ca listă de propoziții românești, **de la cel mai specific la cel mai general**:
operatorul citește primul mesaj, deci el trebuie să spună cel mai mult.

### 2.4 Asimetria deliberată: blocajul nu păzește AȘEZAREA

Blocajul păzește **editarea unei legături existente** — desprinderea sau re-țintirea unui
instantaneu care are deja `IDRR`. **NU** păzește atașarea unuia încă neașezat.

Nu e o scăpare, e necesar, și e ce făcea și Access: verificarea trăia în `btnDel_Click` — butonul
de DESPRINDERE — și nicăieri altundeva. Sub F10, rezultatul normal al fiecărei descărcări e un
teanc de instantanee istorice neașezate, toate cu plăți după ele; dacă blocajul le-ar opri
așezarea, formularul de ingestie s-ar bloca în prima zi și nimic nu s-ar mai putea ingera.
Consemnat în antetul rutei și pinuit de `test_un_instantaneu_neasezat_nu_poate_fi_blocat`.

### 2.5 F15 coboară de la veto la SEMN — dar numai în editor

`valideaza_plasarile` a primit `f15_ca_avertisment` (implicit **False** = purtarea de până acum,
calea de ingestie neatinsă). Motivul: în editor se DESPRIND legături, iar desprinderea ultimului
instantaneu lasă, prin definiție, un lanț care nu se mai închide. Un veto acolo ar face imposibil
tocmai lucrul pentru care există editorul. Fundamentul însuși descrie F15 ca pe un **semn** arătat
per recepție (§1.5), iar Access nu îl verifica deloc la desprindere.

F13, F14 și F16 rămân vetouri în AMÂNDOUĂ căile: sunt absolute.

O grijă mică, dar reală: în modul avertisment, nepotrivirea de total și nepotrivirea de linii ar fi
raportat aceeași veste de două ori (în modul veto, primul `raise` scurtcircuita). Se iese după
prima — dacă totalul nu se potrivește, nici liniile nu au cum.

---

## 3. Ce s-a construit

### 3.1 Server — `PYTHON/routes/forexe/asociere.py` (NOU)

* `GET /api/forexe/asociere?cod=…` — tabloul citit DIRECT din bază: amprentă, TOATE recepțiile,
  TOATE instantaneele (asociate, neașezate și marcate `Sters`), fiecare cu `blocat` + `motive`,
  plus plățile ca context.
* `POST /api/forexe/asociere` — aplică un set **PARȚIAL** de comenzi, o singură fază, o singură
  tranzacție. Amprenta rămâne (două sesiuni pot edita același angajament).

**Ancora e `IDRH`, nu `rand_istoric`.** Contractul în două faze ancorează pe indicele rândului în
`TabelIstoric` (F24) fiindcă id-urile atribuite în propunere dispar la rollback. Aici nu există
sarcină utilă și nu se derulează nimic înapoi, deci ancora e cheia reală.

**Acoperirea NU e obligatorie**, spre deosebire de ingestie. Aici tăcerea înseamnă «lasă legătura
cum e», care e un răspuns adevărat; acolo ar fi fost o alegere ascunsă.

A cincea acțiune, **`desprins`**, există doar aici: în ingestie nimic nu e încă atașat, deci nu e
nimic de desprins. `ApiClient.CatreFir` o refuză explicit pe calea de ingestie, cu un mesaj care
spune DE CE.

Codul-motiv nou: **409 `INSTANTANEU_BLOCAT`**. Nu 400 — cererea nu e greșită ca formă, doar
clientul are un tablou învechit (sau o ordonanțare a apărut între citire și salvare). Verificarea
rulează pe SERVER chiar dacă formularul știe deja blocajele.

Refolosite neschimbate din `prelucrare_asociere.py`: `amprenta`, `citeste_receptii`,
`verifica_etichetele`, `materializeaza_reconstituite`, `valideaza_plasarile`, `recalculeaza_final`,
`marcheaza_reconstituirile_nesigure`. Aliasul `rand_istoric = idrh` pe fiecare comandă e ce le face
refolosibile fără să fie atinse.

`_lanturi_rezultate` construiește lanțurile REZULTATE în memorie și validează ÎNAINTE de a scrie —
și întoarce **doar lanțurile schimbate**: o încălcare veche, deja în bază, nu are voie să blocheze
o corecție fără legătură cu ea.

`_R_DEMARCHEAZA_SQL`: desprinderea rândului de ștergere lasă recepția «neștearsă» din nou. Steagul
de pe `R` nu e o părere, e umbra unui instantaneu anume; dacă acela pleacă, umbra pleacă cu el.
(Nu și pentru o recepție `Reconstituit = 1` — aceea există tocmai fiindcă a fost ștearsă.)

### 3.2 Domeniu — `src/KBot.Domain/AsociereStare.vb` (NOU)

`AsociereStare`, `InstantaneuLegat` (cu `Blocat` + `Motive`), `PlataAsociere`, `ComandaAsociere`,
`AsociereRezultat`. `ActiuneAsociere` a primit `Desprins = 4`. `ReceptiePropusa` a primit
`ReconstituitNesigur` (F28) — serverul îl trimitea deja, clientul îl arunca.

### 3.3 API — `GetAsociereAsync` / `SalveazaLegaturiAsync`

Plus DTO-urile de fir. `PostPropunereReceptie` se refolosește la citire, fiindcă serverul
folosește ACELAȘI `citeste_receptii` pentru amândouă rutele — două copii ar aluneca una față de
alta fără să se vadă.

### 3.4 Control — `src/KBot.Controls/Tree/AdvancedTreeControl.Drag.vb` (NOU)

D-K: tragerea aparține arborelui. Consecința care contează e că **vetoul ajunge la validarea
aruncării**, nu îngropat în tratarea ei — cine ascultă `NodeDragOver` spune «da» sau «nu, uite de
ce» ÎNAINTE ca operatorul să dea drumul mouse-ului.

* `DragEnabled` (implicit **False**, deci cele nouă vederi existente nu capătă comportament nou),
  `DragHighlightColor` / `DragForbiddenColor` (`Color.Empty` = din temă, cu perechea
  `ShouldSerialize`/`Reset` — altfel VS ar îngheța culoarea rezolvată în fiecare formular gazdă).
* Trei evenimente: `NodeDragStarting` (gazda oprește rândurile care nu se mută),
  `NodeDragOver` (**`Allow` implicit False — refuzul e implicitul**), `NodeDropped`.
* Se folosește `DoDragDrop` din WinForms: bucla modală dă gratis cursorul «nu se poate» și oprește
  corect la ESC. Efect secundar binevenit — `MouseUp` nu mai ajunge la control după o tragere, deci
  `ClickDelayTimer` nu pornește și aruncarea nu se mai citește ȘI ca un clic.
* **Nodul tras se ia din OBIECTUL DE DATE, nu din câmpul propriu.** La tragerea între doi arbori,
  `_dragSource` e completat doar pe controlul care a pornit tragerea; ținta e altul și l-ar vedea
  gol. A fost prima greșeală a implementării, prinsă înainte de a fi scrisă în teste.
* Controlul **NU mută nodul** — ridică evenimentul și atât. Arborele e o proiecție a datelor; o
  frunză mutată local ar arăta o legătură pe care nimeni nu a scris-o.
* Chenarul se desenează și la REFUZ, deliberat: un rând care nu răspunde deloc s-ar citi ca
  «arborele nu m-a văzut».

### 3.5 Formularul — `src/KBot.App/Forexe/AsociereForm.{vb,Designer.vb,resx}` (NOU)

Cele patru panouri Access devin trei zone, fiindcă aici mutarea se face trăgând:

```
stânga        recepțiile cu lanțurile lor      (Access: _LISTA + _LISTA_HA)
dreapta sus   instantaneele neașezate          (Access: _LISTA_HN)
dreapta jos   liniile rândului selectat        (Access: _LISTA_RH)
```

Combo-ul de așezare din `_LISTA_HN` și butonul de desprindere din `_LISTA_HA` se contopesc într-o
singură mișcare: tragi la stânga ca să așezi, tragi la dreapta ca să desprinzi. Meniul contextual
poartă cele două marcaje pe care tragerea nu le poate exprima (F17 «nu consemnează nicio schimbare»
și F21 «este rândul de ștergere»).

**O singură salvare, la sfârșit** (D-H): tragerile schimbă doar tabloul local, iar `Comenzi()`
trimite DOAR ce diferă de ce s-a citit. Închiderea cu modificări nesalvate cere confirmare (D-D).

Se reconstruiește tot arborele la fiecare mișcare, nu se mută frunza: un arbore peticit și un
tablou de date ajung, după câteva trageri, să spună lucruri diferite — și acolo cel care minte e
ecranul.

### 3.6 Legarea — `ReceptiiView` + `MainForm`

Iconița din **antetul** arborelui de recepții (`HeaderRightIcon`, liberă până acum — subsolul avea
deja reîncărcarea la dreapta și strângerea la stânga), cu etichetă românească autorată în
`.Designer.vb`. `MainForm.DeschideLegaturileReceptiilor` o deschide **modal** (D-I) și reîncarcă
recepțiile după o salvare.

`ReceptiiView` primește acțiunea ca parametru **opțional**; când lipsește, iconița se STINGE, nu
rămâne un buton care nu face nimic.

---

## 4. Fișiere atinse

**Noi**
```
PYTHON/routes/forexe/asociere.py
PYTHON/tests/test_forexe_asociere.py
src/KBot.Domain/AsociereStare.vb
src/KBot.Controls/Tree/AdvancedTreeControl.Drag.vb
src/KBot.App/Forexe/AsociereForm.vb
src/KBot.App/Forexe/AsociereForm.Designer.vb
src/KBot.App/Forexe/AsociereForm.resx
tests/KBot.Controls.Tests/AdvancedTreeDragTests.vb
tests/KBot.App.Tests/AsociereFormTests.vb
tests/KBot.App.Tests/AsociereFakeApi.vb
docs/worklog/SLICE-0048-04-asociere-oricand.md
```

**Modificate**
```
PYTHON/routes/forexe/__init__.py                  înregistrarea rutei
PYTHON/routes/forexe/prelucrare_asociere.py       f15_ca_avertisment (implicit False)
src/KBot.Domain/AsociereInfo.vb                   Desprins; ReceptiePropusa.ReconstituitNesigur
src/KBot.Api/IApiClient.vb                        cele două metode noi
src/KBot.Api/ApiClient.vb                         implementarea + CitesteAsociere + refuzul lui Desprins
src/KBot.Api/UpsertAngajamenteRequest.vb          DTO-urile de fir
src/KBot.Controls/Tree/AdvancedTreeControl.Overrides.vb   armare / prag / desen
src/KBot.Controls/Tree/AdvancedTreeControl.Theming.vb     cele două culori auto
src/KBot.App/Views/ReceptiiView.vb                iconița + Reincarca
src/KBot.App/Views/ReceptiiView.Designer.vb       HeaderRightIcon + eticheta
src/KBot.App/MainForm.vb                          DeschideLegaturileReceptiilor
tests/KBot.App.Tests/*.vb (8 fișiere)             cei doi membri noi în clienții de probă
```

---

## 5. Rezultatele testelor

| Suită | Înainte | După |
|---|---|---|
| Python | 429 trecute / 15 sărite | **460 trecute / 15 sărite** (`+31`) |
| `KBot.Controls.Tests` | 917 | **931** (`+14`) |
| `KBot.App.Tests` | 169 trecute / 13 roșii | **185 trecute** / 13 roșii (`+16`) |
| `KBot.Common.Tests` | 85 | 85 |
| Build `KBot.sln` | — | **0 erori / 0 avertismente** |

**⚠ Cele 17 teste roșii sunt PREEXISTENTE și nu au legătură cu felia asta** — verificat rulând
suitele pe arborele curățat (`git stash`) înainte și după: aceleași numere, 1 în `KBot.Api.Tests`,
13 în `KBot.App.Tests`, 3 în `KBot.Domain.Tests`. Sunt cele din firul `DdfInfo.vb:130` (prefixul cu
numărul reviziei comentat) plus grupul Ddf/Istoric/Xfa. **`KBOT_STATUS.md` spunea «șapte»; sunt
șaptesprezece.**

---

## 6. Rămas neverificat / amânat

1. ⚠⚠ **Formularul nu a fost văzut pe ecran și nici deschis în designerul Visual Studio.** Aspectul
   e proiectat, nu portat (gazda Access lipsește din export, §1.1), deci nu există nimic cu care să
   fie comparat. Prima privire pe ecran e obligatorie.
2. ⚠⚠ **Nicio interogare nu a rulat pe MariaDB.** În particular `_BLOCAJE_SQL` nu a fost executată
   niciodată: `GROUP_CONCAT` într-o subinterogare corelată, trei `EXISTS`-uri corelate și un
   `MIN(DataORD)` — sintaxa e verificată doar prin citire.
3. ⚠⚠ **`sql/0049_receptii_stergere.sql` tot nu a fost aplicat.** Ruta citește `EsteStergere`, deci
   fără el ambele capete cad cu «unknown column». Aceeași precondiție ca 0048-03.
4. ⚠ **Jumătatea «ordonanțare» a blocajului nu a găsit niciodată nimic**, fiindcă în exportul
   Access `FX_ORD.IDRR` / `IDRH` sunt 0 peste tot (§2.2). Pe date scrise de `mdl_FX_ORD_Salvare`
   v5/v6 ar trebui să fie completate; nu s-a putut proba.
5. ⚠ **Tragerea nu a fost făcută cu un mouse adevărat.** `DoDragDrop` intră într-o buclă modală care
   nu se poate porni fără dispozitiv, deci testele acoperă pragul care o precede, contractul
   evenimentelor și proiecția locală — **nu și bucla însăși**.
6. **Reconstituirea (F26 / D-M) e susținută de server, dar nu are încă intrare în meniul
   formularului.** `ACTIUNE_RECONSTITUIRE` trece prin `normalizeaza_comenzi` și prin
   `materializeaza_reconstituite`; ce lipsește e gestul din interfață.
7. **`treeLibere` nu are grupare pe lună** ca arborele din `ReceptiiView`. Pe un angajament cu multe
   instantanee neașezate lista va fi lungă. Amânat deliberat: `O4` (aspectul formularului) nu era
   specificat, iar gruparea e o alegere de aspect, nu una de corectitudine.
8. **Formularul nu folosește `AsociereStore`.** Dosarul local (`D-C`) există pentru contractul în
   două faze, unde un client căzut ar pierde deciziile luate pe o sarcină utilă care nu se mai poate
   reface. Aici totul se poate reciti de la server oricând, deci nu s-a legat. De rediscutat dacă
   operatorul lucrează sesiuni lungi.
