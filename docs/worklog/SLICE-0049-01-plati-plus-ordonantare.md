# SLICE-0049-01 — «+»-ul din arborele de Plăți cheamă editorul de ordonanțare (`mdl_FX_ORD`)

**Pasul întâi al feliei 0049** (`OrdEditForm` + cele opt rute), care a construit tot mecanismul de
scriere dar l-a legat DOAR de `OrdView`. **Cerință operator (01.09.2026):** „it needs to be
linked to the tree in viewPlati on the right icon click and it must do what `mdlFX_Ord` does".

**Stare:** cod verde — `dotnet build KBot.sln --no-incremental` **0 erori / 12 avertismente**,
identic cu felia 0049; toate 12 sunt `MSB3825` (`ImageStream` / `BinaryFormatter`) pe `.resx`-uri
de vedere care existau dinainte, verificate prin numărare pe tip (`48 warning MSB3825`, zero de
alt fel). **Atenție la citire:** o compilare INCREMENTALĂ raportează 0 avertismente, fiindcă
`.resx`-urile nu se regenerează — cifra reală e cea de la `--no-incremental`.

Teste: `KBot.App` **188 trecute / 13 roșii**, `KBot.Api` **95 / 1**, `KBot.Domain` **14 / 3**,
`KBot.Controls` **953 / 0** — **exact** cifrele de plecare consemnate în worklog-ul 0049,
aceleași nume, toate străine de ORD (DDF / Istoric / XFA).

**Niciun apel n-a atins MariaDB și niciun formular nu s-a deschis pe ecran.** Python **neatins**
(diff gol): felia 0049 scrisese deja ambele rute de care e nevoie.

---

## 1. Ce lipsea, de fapt

Felia 0017 construise arborele de plăți cu «+»-ul lui cu tot, și chiar ridica două evenimente
la clic — dar comentariul lor spunea, negru pe alb, «fără abonat în această felie». Felia 0049
a construit cealaltă jumătate: generarea pe server, editorul, salvarea, ștergerea, lotul. Nimeni
nu le-a legat. Butonul exista, mecanismul exista, sârma dintre ele nu.

Felia asta e sârma. **Nu s-a scris nici rută nouă, nici formular nou** — ambele capete erau deja
acolo, exact în forma cerută.

### 1.1 Cele două ramuri ale lui `mdl_FX_ORD`, așa cum le cheamă Access

În `frmFX_MAIN` stau două handlere, unul per nivel al arborelui de plăți:

```vba
Private Sub fxPlati_AdaugareOrdonantare(vIdPlataFX As Long, vDataPlata As Date)
    FX_Adaugare_ORD_Din_Plati Me, Me.CodAngajament, vIdPlataFX, vDataPlata
    RefreshTreeQuery
End Sub

Private Sub fxPlati_AdaugareOrdonantari(vLunaAn As String)
    FX_Adaugare_ORD_Din_Plati_Batch Me.CodAngajament, vLunaAn
    RefreshTreeQuery
    fxPlati.RefreshPlati Me.CodAngajament, True
End Sub
```

Deci: **ziua deschide editorul, luna rulează lotul** — două lucruri diferite, nu două
înfățișări ale aceluiași. Traducerea, capăt la capăt:

| nivel | Access | K-BOT | ce se întâmplă |
|---|---|---|---|
| 0 — luna | `FX_Adaugare_ORD_Din_Plati_Batch(CodAng, vLunaAn)` | `OrdComanda.LotPeLuna(cod, luna, an)` ▸ `GenereazaInLotAsync` | se generează **și se salvează**, fără editor și fără confirmare per zi, câte o ordonanțare pentru fiecare zi a lunii cu plăți neordonanțate |
| 1 — ziua | `FX_Adaugare_ORD_Din_Plati(Frm, CodAng, -1, vDT)` | `OrdComanda.DinPlati(cod, ziua)` ▸ `AdaugaOrdonantareAsync` | se generează graful (**nimic scris**) și se deschide `OrdEditForm` |

### 1.2 Ce a trebuit schimbat ca ramurile să se potrivească

Cele patru comenzi ale feliei 0049 fuseseră croite pentru singurul apelant de atunci, `OrdView`,
care **nu știe nimic** despre zile: «Adaugă» cerea ziua operatorului prin `OrdZiuaForm`, iar
«Lot» lua tot angajamentul. Din arborele de plăți, amândouă informațiile sunt chiar nodul apăsat.

Deci `OrdComanda` a căpătat patru câmpuri — `Ziua`, `IdPlataFx`, `Luna`, `An` — și două funcții
de construcție numite după ce înseamnă (`DinPlati`, `LotPeLuna`). Constructorul vechi a rămas
neatins, deci **`OrdView` nu s-a modificat deloc**. În shell:

* `AdaugaOrdonantareAsync` primește `ziCeruta`; `Nothing` = **întreabă operatorul**, ca până
  acum. Nu s-a mutat dialogul din shell în vedere și nu s-a duplicat — ziua fie vine gata, fie
  se cere, în același loc.
* `GenereazaInLotAsync` primește `luna` / `an` și le trimite mai departe la
  `GET /api/forexe/ord/zile`, care le acceptă de la felia 0049.

### 1.3 `vIdPlataFX = -1` — de ce comanda de zi pleacă mereu fără plată anume

`FX_Adaugare_ORD_Din_Plati` are un al treilea parametru: o plată individuală, sau `-1` pentru
«toată ziua». Access putea trimite o plată anume fiindcă arborele lui cobora până la ea.

**Arborele nostru se oprește la ZI** — asta e chiar decizia feliei 0017, luată deliberat și
scrisă în comentariul lui `BuildTree`. Deci nu există nod care să poată numi o plată, iar
comanda de zi pleacă întotdeauna cu `IdPlataFx = Nothing`, adică `-1`, adică
`sIdPlataFX = "*"` pe fir.

Parametrul **există totuși** pe drum, până la `GenereazaOrdAsync`, din două motive: ruta îl
acceptă deja (felia 0049 l-a scris), iar nivelul de plată al Access-ului poate reveni oricând —
atunci se schimbă un singur apel, nu lanțul.

Consecință directă și dorită: **avertismentul de peste 25 de parteneri se dă mereu**, fiindcă
serverul îl calculează exact când `id_plata_fx` lipsește — aceeași condiție ca `If vIdPlataFX
= -1 Then` din VBA.

### 1.4 «+»-ul stă pe UN SINGUR nod, și nu s-a atins

`OldestUnordonantatDay` (felia 0017) pune «+» pe **cea mai veche zi cu plăți neordonanțate** și
pe luna care o conține — port al lui `cLeaf.IconRight` urmat de `cLeaf.ParentNode.IconRight` din
`Show_Plati`. Nimic altceva nu-l poartă.

Asta contează acum, fiindcă abia acum butonul FACE ceva: verificat în
`AdvancedTreeControl.Overrides.vb:356`, evenimentul `RightIconClicked` e ridicat sub
`If it.RightIcon IsNot Nothing`, deci un clic în banda din dreapta a unui rând fără pictogramă
nu poate porni nimic. Nu s-a adăugat nicio pază în plus în vedere — ar fi fost o a doua regulă
care spune același lucru, și care ar fi putut aluneca.

### 1.5 Ce se reîmprospătează după o scriere — și ce NU

Access rula, după fiecare din cele două ramuri, `RefreshTreeQuery` și (pe ramura de lot)
`fxPlati.RefreshPlati CodAngajament, True`. Cele trei puncte de reîmprospătare de dinainte,
împrăștiate prin `MainForm`, s-au strâns într-o metodă — `DupaScriereaOrdonantarii` — care face
trei lucruri:

1. **vederea ORD**, dacă e cea activă, pe documentul scris (comportamentul feliei 0049,
   nemodificat);
2. **vederea PLĂȚI**, dacă a fost vreodată deschisă, **chiar dacă nu e cea activă**. Motivul e
   ascuțit: plățile acoperite tocmai au încetat să fie neordonanțate, deci «+»-ul stă pe o zi
   greșită și iconițele de stare mint. Lăsată nereîmprospătată, ar minți TĂCUT până la
   următoarea schimbare de angajament — iar operatorul ar apăsa a doua oară «+» pe o zi deja
   ordonanțată;
3. **poarta vederilor**: prima ordonanțare a unui angajament îi aprinde `AreORD`, deci intrarea
   «ord» din navigație trebuie să apară.

**Punctul 3 se face LOCAL, nu prin re-citirea arborelui mare — și asta e o abatere conștientă
de la Access.** `RefreshTreeQuery` avea acolo un cost neglijabil; aici, `LoadTreeAsync` ▸
`PopulateTree` **golește selecția** (`_currentInfo = Nothing`, `ApplyViewGating(Nothing)`) și
aruncă operatorul înapoi pe «sumar» — exact în clipa de după ce și-a salvat documentul. Deci se
ridică flagul pe `AngajamentTreeInfo`-ul din mână și se re-aplică poarta; valoarea adevărată vine
oricum de la server la următoarea schimbare de an/SS sau descărcare.

Ștergerea cheamă aceeași metodă cu `maiExistaOrdonantari:=False`: o ștergere poate doar să
STINGĂ `AreORD`, iar câte au mai rămas nu se poate ști de aici. Lotul o cheamă cu
`reusite > 0` — poarta se aprinde doar dacă s-a scris ceva.

Metoda **nu rearuncă**: scrierea a reușit deja, iar o excepție la reîmprospătare arătată ca
eroare ar face să pară că salvarea a picat.

---

## 2. Fișiere atinse

Trei fișiere, toate în `KBot.App`. **Zero fișiere noi, zero rute, zero SQL, zero schimbări de
contract.**

* `src/KBot.App/Views/Ord/OrdComanda.vb` — patru câmpuri opționale (`Ziua`, `IdPlataFx`,
  `Luna`, `An`), un constructor privat și cele două funcții de construcție `DinPlati` /
  `LotPeLuna`. Constructorul public existent, neatins.
* `src/KBot.App/Views/PlatiView.vb` —
  * al treilea parametru **opțional** al constructorului: acțiunea `OrdComanda` a shell-ului
    (`Nothing` = gazdă fără shell, cum sunt testele; atunci rămân doar evenimentele);
  * `Reincarca()` public — portul lui `fxPlati.RefreshPlati`;
  * `Tree_RightIconClicked` traduce nivelul în comandă, **după** ce ridică evenimentul brut;
  * `PrimaDataDin(rows)` înlocuiește `DayOf(rows(0))`: scanează rândurile până găsește una cu
    dată, în loc să se bazeze pe primul rând și să cadă tăcut pe `Date.MinValue` când acela
    n-are. Un `Date.MinValue` trimis ca zi ar fi ajuns la server ca o cerere validă pentru anul 1.
  * `DayOf` a fost ȘTERS (rămăsese fără apelant), cu o notă la locul lui.
* `src/KBot.App/MainForm.vb` —
  * `CreateView("plati")` dă vederii `AddressOf ExecutaComandaOrd`, **aceeași** acțiune pe care
    o primește `OrdView` — o singură politică de re-login, un singur loc unde se deschide
    editorul;
  * `ExecutaComandaOrd` trece mai departe cele patru câmpuri noi;
  * `AdaugaOrdonantareAsync` / `GenereazaInLotAsync` au căpătat parametrii opționali;
  * `DupaScriereaOrdonantarii` + `NumeLuna` (nume de lună în română, doar pentru mesajele
    lotului), iar cele trei reîmprospătări de dinainte cheamă acum metoda.

### Regula 0 — fără diacritice în cod

Comentariile ADĂUGATE de felia asta sunt în română **fără** diacritice; șirurile pe care le vede
operatorul și le-au păstrat («Se încarcă plățile…», «Nu există plăți neordonanțate pentru
{cod} în Aprilie 2026.»). Pe cele trei fișiere preexistente s-au măturat **doar rândurile
adăugate aici**, identificate din `git diff` — comentariile de dinainte nu sunt treaba acestei
felii. Verificat după: `git diff` filtrat pe rândurile `+` care încep cu `'` sau `'''` întoarce
**zero** potriviri cu diacritice.

**Abatere consemnată, aceeași ca la 0049:** `CLAUDE.md` cere ca *limba* comentariilor să fie
engleza. Comentariile de aici au rămas în română fără diacritice, ca ale fișierelor în care se
inserează. Regula 0 e respectată integral; regula de limbă nu.

---

## 3. Ce a rămas neverificat

1. **Nimic nu a rulat pe MariaDB.** Cele două rute chemate (`POST /api/forexe/ord/genereaza`,
   `GET /api/forexe/ord/zile`) sunt scrise de felia 0049 și rămân, ca atunci, **nerulate pe date
   reale**. Felia asta doar le cheamă din al doilea loc.
2. **«+»-ul n-a fost apăsat.** Traseul clic ▸ comandă ▸ rută ▸ editor n-a fost parcurs de un
   mouse adevărat. `OrdEditForm` rămâne, ca după 0049, **niciodată deschis pe ecran**.
3. **Lotul pe lună n-a fost văzut rulând.** În special: dacă o lună are multe zile, bucla
   afișează busy-bar-ul dar **nu are contor per zi** — un lot lung arată identic cu unul blocat.
   `clsMeter`-ul Access-ului nu are succesor. Se vede abia la prima rulare reală.
4. **Avertismentul de 25 de parteneri** ajunge acum pe calea de zi (unde `id_plata_fx` lipsește
   întotdeauna), dar tot n-a fost văzut declanșându-se.
5. **Reîmprospătarea vederii de Plăți după lot n-a fost observată.** `Reincarca()` reface arborele
   și **golește grila** (`LoadAsync` ▸ `FillGrid` se face abia la următorul clic pe un nod) —
   comportament identic cu o schimbare de angajament, dar nevăzut.

## 4. Neportat deliberat

6. **Nivelul de plată individuală din arbore.** Vezi §1.3: e o decizie a feliei 0017, nu o
   scăpare a acesteia. Drumul e pregătit până la rută.
7. **`Contor_Zile_Luna` ca număr afișat înainte de confirmare.** Serverul întoarce deja
   `TotalEstimat` pe `GET /ord/zile`, iar confirmarea lotului îl arată — nu s-a mai portat a doua
   socoteală în client.
8. **`FX_Sterge_Ordonantari` (ștergerea în lot, pe o zi întreagă)** rămâne unde a pus-o felia
   0049: pe meniul contextual al arborelui de ORD, o ordonanțare o dată. `Popup_Commands`
   ramura `"ORD_DEL"` de nivel 0 aduna id-urile copiilor și le ștergea în bloc; asta ține de
   arborele de ordonanțări, nu de cel de plăți, deci nu intră în felia asta.
9. **`Case "ADD_ORD": Stop`** din `Popup_Commands` — în Access ramura aia e literalmente un
   `Stop` cu apelul comentat după el (`FX_Adaugare_ORD_Din_Receptie`). Funcția nu există în
   export. Nu s-a inventat.
