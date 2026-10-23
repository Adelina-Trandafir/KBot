# Fisa de citire: Conturi (plan de conturi) - AV01

> DECIZIE operator (23.10.2026): planul de conturi este **pe unitate**; datele de nomenclator (`DefaCont`, `DefaClsfE`,
> `DefaClsfF`, sector, sursa...) se trag din `AVACONT_COMUN`. Ramane de hotarat: SID/SIC in plan sau separat (S1).

Buton ribbon Access: `btnCfg_AdaugCont` (tab Configurari > Baza de date > «Conturi»).
Formular: `AdaugCont` (762 linii cod) + subformular `AdaugCont_sub`. Nomenclatorul de baza se
editeaza in fereastra `Setari implicite` (tab Conturi) prin subformularul `DefaCont subform`.
Stare in K-BOT: **LIPSESTE** (nici tabele in MariaDB, nici rute, nici fereastra).
Marcaj: VERIFICAT = citit in fisierul numit; PRESUPUS = dedus, de confirmat.

## 1. Ce vede / face operatorul in Access

VERIFICAT (`Forms/AdaugCont_code.txt`):
- Un **arbore** pe 4 niveluri: Clasa (prima cifra) > Cont (sintetic, din `DefaCont`) > ClsfF >
  ClsfE > frunza = contul complet (`SimbolCont`). ClsfF/ClsfE apar doar daca contul le foloseste.
- Cautare in arbore (`Caut`), buton «Plan» (comuta intre arbore si grila `AdaugCont_sub`),
  export Excel (`btnXLS`; in cod doar deschide un Excel gol - nu exporta nimic real).
- Butoane Adauga / Modifica / Salveaza / Anuleaza. Modificarea e permisa doar pe frunza.
- Campurile unui cont: Cont, Sector, Sursa, ClsfF (cu bifa `xClsfF`), ClsfE (cu bifa `xClsfE`),
  Analitic, Fel (A=Activ, B=Bifunctional, P=Pasiv), Denumire, SID, SIC, RPD, RPC.
- Comenzi rapide spre editarea nomenclatoarelor (Cont, Sector, Sursa, ClsfF, ClsfE) = deschid
  `Setari implicite` pe tabul potrivit.

## 2. Date

Structura reala, VERIFICAT in `AVACONT\DATABASES\baza2026.accdb` (copie, read-only; tabelele sunt LOCALE in
acel fisier, nu legate - corectie fata de prima versiune a fisei):

`PlanCont` (351 randuri; un rand = un cont complet): `SimbolCont` (Text 50, **cheia primara**), `Cont`, `Analitic`
(Text 10), `DenumireCont`, `FelCont` (A/B/P; in date apar `P` si gol), `CodSector`, `Sursa`, `ClsfF`, `ClsfE`,
`ContLung`, `TipCont`, `Extra` (Da/Nu), `SIC`, `SID` (Currency), `RPC`, `RPD` (Double), `DTQ`, `Expr1` (Long, fara
rost vizibil). Exemple de cheie: `117000001A` = cont `1170000` + sector `01` + sursa `A`.
- `SID`/`SIC` = sold initial debitor / creditor: VERIFICAT pe date (contul 213 are doar SID, contul 280/281 -
  amortizari - are doar SIC). `RPC`/`RPD` sunt 0 in toate exemplele (rulaje precedente: PRESUPUS).
- Soldurile initiale pe parteneri sunt in `ParteneriSI` (52 randuri: SimbolCont + CodPartener + SID/SIC/RPC/RPD),
  la conturi ca 421, 427, 431, 444, 461, 462, 550.
- Relatiile PlanCont -> Debit, Credit, DefaNote (debit/credit), ParteneriSI, Salarii*, ContChelt, ContVenit sunt cu
  Update+Delete in CASCADA (atribut 4352): in Access, stergerea unui cont sterge liniile de nota care il folosesc.

`DefaCont` (991 randuri, nomenclator general): `Cont` (PK), `Denumire`, `Tip` (P in exemple), `Sector`, `Sursa`,
`ClsfF`, `ClsfE`, `Vizibil` (Da/Nu: bifele spun din ce se compune simbolul). `DefaClsfF` 24, `DefaClsfE` 905,
`DefaSector` 5 (01..05), `DefaSursa` 11 (A..L). `DefaClase` nu e in `baza2026.accdb`, dar datele ei sunt in
`AVACONT/EXPORT_VBA/tabledata/DefaClase.csv` (VERIFICAT): coloane `Clasa`, `Denumire`; 8 randuri: 1 CAPITALURI,
2 IMOBILIZARI, 3 STOCURI, 4 TERTI, 5 TREZORERIE, 6 CHELTUIELI, 7 VENITURI, 8 CONTURI EXTRABILANTIERE. Nomenclator
fix de 8 randuri: se poate pune in `AVACONT_COMUN` ca tabel (sau in cod).
- `SimbolCont = Cont + Sector + Sursa + [ClsfF] + [ClsfE] [+ "." + Analitic]`.
- `ContLung = (Cont+Sector+Sursa+[ClsfF]+[ClsfE])` completat cu `X` pana la 41 de caractere.
- Sector si Sursa se iau din unitate (`UNIT.SectorSursa`: 2 caractere sector + 1 sursa).

Nomenclatoare: `DefaCont` (`Cont`, `Denumire`, `Tip`, `Sector`, `Sursa`, `ClsfF`, `ClsfE`, `Vizibil` -
bifele ClsfF/ClsfE spun daca contul se desparte pe clasificatie), `DefaClase` (`Clasa`, `denumire`),
`DefaClsfF` (`ClsfF`, `Explicatie`), `DefaClsfE` (`ClsfE`, `Denumire`), `DefaSector`, `DefaSursa`.

Importat din alta unitate: `CorelatiiPlan` (VERIFICAT, `tables/CorelatiiPlan.md`): tabel de
pasaj cu aceleasi coloane + `S` (selectat). Apartine mecanismului de copiere intre unitati.

Cine citeste `PlanCont` (VERIFICAT prin grep): 26 interogari, 9 formulare, 6 module - e baza
tuturor combo-urilor de cont, a fiselor de cont si a rapoartelor. `SID/SIC/RPD/RPC` =
solduri initiale debitoare/creditoare si rulaje precedente (PRESUPUS dupa nume; de confirmat).

## 3. Reguli de pastrat (logica)

- Un cont nu poate exista de doua ori (`SimbolCont` unic) - mesaj «Contul exista deja!».
- Campuri obligatorii: Cont, Sector, Sursa, Denumire, SID, SIC (+ ClsfF/ClsfE doar daca bifate).
- ClsfE nu se poate folosi fara ClsfF (cod din `DefaCont subform`).
- Analitic: doar cifre, litere majuscule si `_`; pentru conturile 401*, clasa 6 si clasa 7 se
  cere confirmare (contul de furnizori si cele de cheltuieli/venituri nu se despart usor analitic).
- Fel cont: A/B/P (Activ/Bifunctional/Pasiv).
- **STERGERE (cerinta operatorului, 23.10.2026): un cont care are operatii este BLOCAT la stergere, orice ar fi** -
  fara bifa, fara avertisment, fara exceptie. «Are operatii» = apare in vreo linie de nota (debit/credit). In
  MariaDB legaturile se fac `RESTRICT` (nu cascada ca in Access). Pentru un cont nefolosit in operatii dar
  referit de sabloane (DefaNote/DefaSalarii*) sau de solduri initiale (ParteneriSI): se blocheaza cu mesaj care
  spune unde e folosit (propunere; operatorul confirma).
- Fiecare adaugare/modificare se scrie in istoricul de actiuni (`SalveazaActiune`).
- La salvarea unui cont de nomenclator cu `Vizibil = False`, Access intreaba (mesajul 10480) daca
  sa-l faca vizibil; la «Da» seteaza `Vizibil = True` (VERIFICAT, `DefaCont subform_code.txt`).
  La contul nou, `Vizibil`, `Sector` si `Sursa` pornesc bifate.

## 4. Mecanisme Access de lasat deoparte

Arborele ActiveX/VBA (`clsTreeView`, `subFormVBNET`), campurile ascunse folosite ca stare
(`TV`, `Nou`, `SSCC`), culoarea rosie pe camp ca semnal de eroare, `SendKeysAPI`, fereastra
«Setari implicite» ca editor de nomenclator (in K-BOT: editor propriu), exportul Excel gol.

## 5. Suprapuneri cu K-BOT

Nimic pentru conturi. Foloseste ca model `ClasificatiiForm` (arbore + editare) si
`KBot.Controls` (`AdvancedTreeControl`, `KBotDataView`). Sector/Sursa vin deja din
`DefaSursaSector` / selectorul SS din bara. `DefaClsfE/F` exista in `AVACONT_COMUN.sql`.

## 6. Ce nu am putut verifica

- Structura reala: acum VERIFICATA in `baza2026.accdb` (vezi sectiunea 2). `MariaDB_Schema/` nu contine nimic de acest fel.
- `PlanCont` din `baza2026.accdb` e planul unei singure unitati/an (351 conturi, sector `01`, sursa `A`), iar
  `DefaCont` (991) e nomenclatorul general din care se alege: PRESUPUS "plan pe unitate, nomenclator comun".
- RPC/RPD: doar zerouri in exemple; semnificatia ramane PRESUPUSA.
- Denumirile claselor (`DefaClase`): gasite in `tabledata/DefaClase.csv` (rezolvat).

## 7. Propunere (eticheta de confirmat)

| Element | Eticheta | Nota |
|---|---|---|
| Plan de conturi pe unitate (tabel + API + fereastra arbore Clasa>Cont>ClsfF>ClsfE>cont) | PORT | model `ClasificatiiForm` |
| Nomenclator `DefaCont` + `DefaClase` (editabil) | PORT | in fereastra, nu in «Setari implicite» |
| Sector/Sursa la creare | EXISTA | din unitate / SS |
| Export Excel | DE DECIS | in Access nu face nimic real |
| Import plan din alta unitate (`CorelatiiPlan`) | DE DECIS | nu e in ribbon |
| Bifa «Stergere conturi» | ELIMINAT | decis de operator |

## 8. Intrebari pentru operator

1. Confirmi "plan pe unitate (cu analitice proprii), nomenclator `DefaCont` comun"? (schimba unde stau
   tabelele: COMUN sau baza unitatii)
2. SID/SIC sunt solduri initiale (verificat pe date). Raman in planul de conturi sau le mutam in «solduri
   initiale» (etapa 2), langa `ParteneriSI`? RPC/RPD mai trebuie?
3. Analiticul se mai foloseste (la ce conturi)? Pastram regula 401/clasa 6/clasa 7?
4. Pastram `ContLung` (cod pana la 41 de caractere cu X)? Il folosesc rapoarte sau doar sortare?
5. Se editeaza `DefaCont` doar de administrator sau si de contabil?
6. Exista conturi pe care nu se mai lucreaza (nevizibile) care sa nu se mai aduca?
