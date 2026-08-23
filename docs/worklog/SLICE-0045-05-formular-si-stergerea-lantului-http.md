# SLICE-0045-05 — formularul, ștergerea lanțului HTTP, și PRIMA rulare pe date reale

Data: 23.08.2026 · Slice **0045**, pasul 05 (ultimul pas de implementare).

## Ce s-a facut si de ce

Ultima transa: formularul, scoaterea lantului HTTP al feliei 0044, si — pentru prima data
in toata felia — **cod de productie rulat pe fisierele reale**.

### Formularul

`MigratorForm.vb` + `.Designer.vb`, rescrise de la zero. **Toate controalele sunt declarate
in `.Designer.vb`**, niciunul construit in cod; formularul mosteneste `KBotThemedForm`, deci
nu-si aplica singur nicio culoare. Toate grilele sunt `KBotDataView`, nu `DataGridView`.
Etichetele plutitoare sunt `KBotToolTip`, scrise in designer, in romana cu diacritice.

Patru grupuri, cum cere §2 al planului — cu **o abatere deliberata**, pe care o consemnez
fiindca planul spune altceva:

> §2 cerea o casuta «Cale FX» si una «Cale CAI». **Pasul de descoperire a schimbat asta.**
> Caile fiecarei unitati catre `baza<an>.accdb` si `FX_<an>.accdb` sunt CHIAR IN registru
> (`cai.FullPath`, `cai.CaleForexe`), deci operatorul alege doar `cale.accdb`, iar unealta
> citeste restul. A cere doua cai tastate de mana ar insemna sa i se ceara operatorului
> informatie pe care fisierul o are deja — si ar face posibila o nepotrivire intre ce alege
> el si ce spune registrul.

- **«Fișiere»** — registrul, parola fisierelor de unitate, parola fisierelor FOREXE,
  dosarul jurnalului. Ambele parole cu `UseSystemPasswordChar`.
- **«Server MariaDB»** — gazda, port, utilizator administrator, parola, «Testează
  conexiunea» + o eticheta de stare care arata versiunea serverului.
- **«Unitate»** — DC-ul intr-un combo, «Citește registrul», baza-tinta rezolvata, si o grila
  cu o bifa pe unitate. **Decizia D2: operatorul alege unitatile**, niciodata «toate din DC».
  Grila arata pe fiecare rand daca fisierul de nomenclatoare si cel FOREXE chiar exista.
- **«Transfer»** — «Verifică» / «Transferă» / «Oprește», o bara de progres, grila de tabele
  cu bife, grila de constatari si jurnalul rularii.

Reguli respectate, fiecare cu motivul ei:

- **«Transferă» porneste dezactivat** si se activeaza doar dupa o verificare fara constatari
  blocante; se dezactiveaza din nou dupa un COMMIT reusit, fiindca tabelele doar-inserare
  (D8) nu suporta o a doua rulare.
- **Cele doua operatii lungi ruleaza in afara firului de interfata** si se pot opri; oprirea
  deruleaza tranzactia inapoi. Inchiderea formularului in timpul unei operatii intreaba
  intai, apoi opreste.
- **Parolele se citesc din casute exact in clipa in care se construieste cererea** si nu se
  pastreaza nicaieri.
- Fiecare tratator de eveniment e invelit in `Try/Catch` care logheaza si inghite — o
  exceptie dintr-un tratator WinForms nu poate fi rearuncata.

### Setarile

`MigratorSettings.vb` — un JSON mic langa executabil. **Nicio parola**: tipul nu are camp
de parola deloc, ca sa nu existe unul pe care cineva sa decida mai tarziu sa-l umple.

**Nepotrivire fata de plan, consemnata:** §2 cere persistenta «prin mecanismul existent
`KBot.LocalStore`». **Mecanismul acela nu exista.** `KBot.LocalStore` are `ITempStore` si
`SqliteTempStore` — o stocare temporara cu `Open`/`Reset`/`Dispose` si nicio interfata de
setari. Decat sa inventez una intr-o felie de migrare, am scris un JSON; daca apare o
stocare de setari reala, e o singura clasa de mutat.

### Ce a fost sters

Lantul HTTP al feliei 0044, care nu mai era referit de nimic dupa ce formularul a fost
rescris:

- `ConnectForm.vb` / `.Designer.vb` / `.resx` — dialogul de pornire care cerea adresa
  serverului si cheia API. Nu mai are ce dovedi: nu mai exista nici adresa, nici cheie, iar
  credentialele MariaDB sunt campuri pe singurul formular, probate de «Testează conexiunea».
- `Api/MigrareApiClient.vb` — 762 de randuri de client HTTP.
- `Registry/AvacontRegistry.vb` — citea unitatile din
  `HKCU\Software\VB and VBA Program Settings\AVACONT`. Inlocuit de citirea directa a lui
  `cai`, care e sursa autoritara.
- Referinta de proiect catre `KBot.Api`.
- Comentariul din `.vbproj` care spunea «NICIO referință la Access … urcă fișierul .accdb pe
  server și vorbește HTTP» — devenise exact pe dos fata de cod, iar un comentariu care
  contrazice codul e mai rau decat niciunul.

`Program.vb` porneste acum direct `MigratorForm`.

## PRIMA rulare pe date reale

Pana acum fiecare worklog al feliei spunea «nimic rulat». Nu mai e adevarat pentru jumatatea
Access.

`tools/MigratorSmoke/` — un ham aruncabil (**nu in `KBot.sln`**) care referă chiar
`src/KBot.Migrator` si conduce **clasele de productie** peste fisierele reale. Spike-ul
feliei 0045-01 dovedise ca ACE deschide fisierele, dar era alt cod, in alt proces; asta
dovedeste ca `AccessProvider`, `AccessSchema`, `AccessTableReader`, `CaiRegistry`,
`ClasificatieDerived` si `TableMaps` fac acelasi lucru.

Rulat pe `Avacont\cale.accdb`, iesire completa, **fara parola**:

```
CaiRegistry.Read            13 unități, un singur DC (000_DEMO), toate 13 pe el
AccessSchema (ENERGETIC)    69 tabele; Clasificatii 54/30col, Parteneri 75/17col,
                            Rectificari 0/14col, ParteneriAng 2/8col, UNIT 1/21col
AccessTableReader           HasColumn("idunitate") = False
                            ValueOrMissing("capitol") → valoarea lui «Capitol»
                            ValueOrMissing(coloană inexistentă) → Nothing
TableMaps                   28 tabele în catalog, 14 pe potrivire după nume, 10 excluse
```

Patru lucruri dovedite, nu presupuse:

1. **Potrivirea neinsensibila la caz chiar functioneaza pe date reale** — `"capitol"` a citit
   `Capitol`, iar Regula 4 nu mai e doar o intentie.
2. **`ValueOrMissing` deosebeste lipsa de NULL** — coloana inexistenta a intors `Nothing`,
   nu `DBNull`.
3. **Constatarea F3 e confirmata de codul de productie:** `HasColumn("idunitate")` e `False`
   pe `Clasificatii`. Unitatea chiar NU e pe rand.
4. **`ClasificatieDerived` se potriveste cu ce calculeaza Access insusi.** Pentru
   `65.01 / 04.02 / 10.01 / 01` a dat `Clsf=65.01.04.02.10.01.01`, `Titlu=10`,
   `ClsfF=650402`, `ClsfE=100101`, `Sector=01`, `Sursa=A`, `SS=01A` — **exact** valorile pe
   care fisierul Access le poarta in propriile coloane pe acel rand (vezi
   `artifacts/accdb-schema/baza2026.md`). Replicarea DDL-ului MariaDB produce aceleasi
   valori ca formulele Access, ceea ce e o dovada serioasa ca poarta `AVACONT_COMUN` va
   verifica valorile corecte.

**Si o constatare noua, utila:** din 13 unitati, **una singura are fisierul de nomenclatoare
prezent** (unitatea 76, ENERGETIC ISJ). Restul de 12 arata `nom=NO`. Depozitul poarta doar
acel `baza2026.accdb`. Codul le detecteaza corect, iar verificatorul va da `FISIER_LIPSA`
**BLOCANT** pe oricare dintre ele daca e bifata. De asemenea, toate cele 11 unitati cu
`CaleForexe` arata catre **acelasi** `C:\AVACONT\Forexe\FX_2026.accdb` — exact forma
«un fisier, mai multe unitati» pentru care exista filtrul `BelongsToUnit`.

## Rezultate

- `dotnet build src/KBot.Migrator/KBot.Migrator.vbproj`: **0 erori, 0 avertismente.**
- `dotnet build KBot.sln`: **0 erori** (5 avertismente, toate din alte proiecte, dinainte).
- `tools/MigratorSmoke`: **0 erori, 0 avertismente**, rulat, iesire `OK`.

Fara teste automate (§9 al planului).

## Fisiere atinse

| Fisier | Ce |
|---|---|
| `src/KBot.Migrator/MigratorForm.Designer.vb` | rescris — patru grupuri, toate controalele declarate aici |
| `src/KBot.Migrator/MigratorForm.vb` | rescris — comportamentul |
| `src/KBot.Migrator/MigratorSettings.vb` | nou — JSON, fara parole |
| `src/KBot.Migrator/Program.vb` | pornire directa, fara `ConnectForm` |
| `src/KBot.Migrator/KBot.Migrator.vbproj` | fara `KBot.Api`; comentariul refacut |
| `src/KBot.Migrator/ConnectForm.*` | **STERSE** |
| `src/KBot.Migrator/Api/MigrareApiClient.vb` | **STERS** |
| `src/KBot.Migrator/Registry/AvacontRegistry.vb` | **STERS** |
| `tools/MigratorSmoke/` | nou — ham aruncabil, nu in solutie |

## Neverificat / amanat

1. **Formularul nu a fost deschis niciodata** — nici pe ecran, nici in designerul VS. Toata
   aranjarea (`TableLayoutPanel`-uri, latimi de coloana, ancorele barei de progres) e scrisa,
   nu vazuta. Asta ramane riscul cel mai mare al feliei.
2. **Jumatatea MariaDB nu a fost atinsa deloc.** `TargetSchema`, `TargetServer`,
   `WriteOrder`, `Verifier`, `TransferRunner` si `SqlDumpWriter` compileaza si sunt scrise pe
   `000_DEMO.sql`, dar **niciun server nu a fost contactat, niciun rand nu a fost scris,
   niciun dosar de jurnal nu a fost creat**.
3. **`LastInsertedId`** pe `MySqlConnector` cu o comanda pregatita refolosita ramane neprobat;
   harta de rezolutie a clasificatiilor depinde de el.
4. **`AVACONT_COMUN` tot nu a fost vazuta.** Numele coloanelor din dictionare vin din tintele
   cheilor straine din `000_DEMO.sql`, nu dintr-o schema citita.
5. **Doar o unitate din 13 are fisier de nomenclatoare** in depozit, deci o rulare peste mai
   multe unitati nu poate fi probata aici.
6. Bifele grilei de unitati se citesc **dupa indice de rand**, presupunand ca ordinea grilei
   e cea a lui `CaiRegistry.UnitsOf`. Adevarat azi (grila nu-si muta randurile singura), dar
   e o legatura implicita: daca grila capata sortare, trebuie mutata pe cheie.

## Starea feliei 0045

Cei cinci pasi sunt gata ca **cod**:

| Pas | Ce | Stare |
|---|---|---|
| 01 | descoperirea + `MAPARE_NOMENCLATOARE.md` | gata, **rulat** (spike) |
| 02 | motorul Access + descrierea tintei | cod verde; Access **rulat** (pasul 05) |
| 03 | catalogul, hartile, portile | cod verde, nerulat |
| 04 | scrierea + jurnalul SQL | cod verde, nerulat |
| 05 | formularul + stergerea lantului HTTP | cod verde, formular nedeschis |

Ce lipseste inainte de o rulare reala, in ordinea in care blocheaza:

1. **`AVACONT_COMUN`** trebuie sa existe si sa fie populata pe server — altfel niciun rand de
   clasificatie nu intra (cinci chei straine).
2. **Formularul deschis in designerul VS si apoi pe ecran**, macar o data.
3. **O baza-tinta noua**, creata dupa `AVACONT_SURSA` (D7), si o rulare de proba pe
   unitatea 76.
