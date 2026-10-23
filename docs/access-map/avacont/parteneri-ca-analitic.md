# Fisa de citire: Partener = furnizor SAU analitic - AV01

Aparut din observatia operatorului (23.10.2026): in Avacont `CodPartener` NU inseamna doar furnizor; poate fi
un fel de **analitic cu informatii speciale** fata de un analitic simplu. Deci ecranul de furnizori din K-BOT nu
poate fi singura baza. Eticheta in index: DE DECIS (nu "EXISTA").
Marcaj: VERIFICAT = citit (fisier sau date din `baza2026.accdb`, copie read-only); PRESUPUS = dedus.

## 1. Ce arata datele (VERIFICAT, `baza2026.accdb`)

`Parteneri` (75 randuri): `CodPartener` (Text 50, **cheie primara**, 3 sau 4 caractere), `DenumirePartener`,
`CodFiscal`, `ContIBAN`, `Banca`, `Adresa`, `Tip`, `CodClient`, `Ascuns`, `Esinc`, `IDPART`, `IdPartener`, `NumePartener`,
+ campuri fara nume (`ContPl`, `F8`, `F9`).
- Doar **43 din 75** au cod fiscal; **32 nu au** (inclusiv marcajul `XXX`). Niciunul nu are IBAN in cele fara CF.
- `Tip`: `1` = 57 randuri (43 cu CF + 14 fara), `2` = 17 randuri (toate fara CF, coduri `016`, `017`, `021`, `1000`...`1031`),
  gol = 1 (`XXX`). Sensul lui `Tip` 1/2 NU e documentat in cod (vezi intrebarea 1).
- Coduri fara CF cu `Tip = 1`: `001`...`013`, `025`: in sabloanele de salarii `001` apare pe contul 4210000
  (personal), `002` pe 4310100 (asigurari sociale), `005` pe 4310500 etc. Adica ei sunt **analitice ale unor conturi
  de personal / asigurari / retineri**, nu furnizori.
- Folosire reala: din cele **20 de coduri** care apar in liniile de nota (`Debit`/`Credit`), **13 nu au cod fiscal**
  si doar 7 il au. Deci majoritatea partenerilor folositi sunt analitice.
- `ParteneriSI` (52 randuri): solduri initiale pe (cont, partener): `SID`, `SIC`, `RPC`, `RPD`; la conturi 421, 423, 426, 427,
  431, 444, 446, 461, 462, 550, 552. `ParteneriAng` (2): partener -> clasificatie (`CodAng`, `CodInd`, `ContBanca`).
- Liniile de nota au partener PER LINIE: `Debit(IdOperatie, SimbolCont, CodPartener, SumaDebit)` si
  `Credit(...)`; legatura Parteneri -> Debit/Credit/ParteneriSI/ParteneriAng este cu Update+Delete in cascada in Access.

## 2. Ce are K-BOT azi (VERIFICAT: `MariaDB_Schema/AVACONT_SURSA.sql`)

`Parteneri`: `IdPartener` (AUTO_INCREMENT), `IdUnitate`, `CodPartener` varchar(50) **unic per unitate**
(`UK_Parteneri_IdUnitate_CodPartener`), `CodFiscal` varchar(255) NULL, `ContIBAN`, `Banca`, `Adresa`, `Tip`, `Ascuns`,
`Provenienta`. `Parteneri_Coduri` = coduri de angajament per (partener, clasificatie).
- **Schema suporta analitice**: cheia este `CodPartener`, nu `CodFiscal`, iar `CodFiscal` accepta NULL.
- **Ce nu am verificat:** `ParteneriForm` si slicele 0121-0123 (coduri de angajament pe cod fiscal, "un partener per cod
  fiscal") pot impune cod fiscal obligatoriu / unic sau pot ascunde randurile fara CF. De citit inainte de etapa 2.
- Lipseste: solduri initiale pe parteneri (`ParteneriSI`), orice legatura nota -> partener.

## 3. Reguli de pastrat / de lasat

PASTRAT: partener per linie de nota; `CodPartener` stabil (3-4 caractere) ca analitic; soldurile initiale pe (cont, partener);
`Ascuns` ca sa retragi un cod fara sa-l stergi.
DE LASAT: marcajul `XXX` ("cere partenerul la utilizare") din sabloane; campurile fara nume (`F8`, `F9`, `ContPl`);
`Esinc`, `IDPART` (urme de sincronizare).
STERGERE: acelasi principiu ca la conturi si jurnale - un partener care apare in operatii sau in solduri initiale este
BLOCAT la stergere (propunere in acelasi spirit cu cerinta operatorului; de confirmat).

## 4. Propunere

| Element | Eticheta | Nota |
|---|---|---|
| Un singur nomenclator de parteneri pentru furnizori SI analitice | DE DECIS | fara cod fiscal obligatoriu pentru analitice; `Tip` din Access NU se portheaza |
| `Parteneri.Tip` (valori 1/2 din Access) | ELIMINAT | decis de operator, 23.10.2026 |
| `ContPl` (cont de plata) | ELIMINAT | decis de operator: nu se adauga (gol la toti cei 75 de parteneri) |
| `ParteneriSI` (solduri initiale cont + partener) | PORT, etapa 2 | leaga de planul de conturi |
| Ecranul de furnizori existent | pastrat | analiticele apar intr-un mod/filtru separat, nu in lista ANAF |
| Informatiile speciale ale analiticului | DE DECIS | ce campuri are in plus (vezi intrebarea 2) |

## 4b. Decizii operator (23.10.2026)
Analiticele sunt **proprii fiecarei unitati** (ca tabela `Parteneri` de azi, pe `IdUnitate`). `Tip` si `ContPl` nu se portheaza.
Campuri optionale ale unui analitic: denumire, adresa, IBAN, banca. Ramane de verificat in cod: `ParteneriForm` si
slicele 0121-0123 (impun cod fiscal?).

## 5. Intrebari pentru operator (ramase doar cele de verificat in cod; restul sunt rezolvate mai sus)

1. (REZOLVAT, 23.10.2026) `Tip` 1/2: operatorul nu mai stie la ce folosea si il considera irelevant -> **NU se portheaza**.
   Coloana `Tip` din MariaDB `Parteneri` ramane neatinsa si nefolosita. Consecinta: un analitic si un furnizor se
   deosebesc doar prin lipsa/prezenta codului fiscal (sau printr-un semn nou, daca operatorul il vrea - vezi 2).
2. (RASPUNS operator, 23.10.2026: «IBAN, cont de plata; denumire, adresa... cateva chestii posibile».) Informatiile
   speciale ale unui analitic sunt campuri OPTIONALE: denumire, adresa, IBAN, cont de plata. VERIFICAT pe date
   (`baza2026.accdb`, doar numaratori): la cele 32 de parteneri fara cod fiscal doar `DenumirePartener` este completat
   (32/32); `Adresa`, `ContIBAN`, `Banca`, `ContPl` sunt goale (0/32). La cei 43 cu cod fiscal: `ContIBAN` 39/43,
   `Banca` 26/43, `Adresa` 0/43. `ContPl` este gol la toti cei 75. Concluzie: campurile exista si sunt posibile, dar in
   practica nu se folosesc la analitice. K-BOT are deja `DenumirePartener`, `Adresa`, `ContIBAN`, `Banca`; **`ContPl`
   (cont de plata) NU exista in MariaDB**. Propunere: nu adaugam campuri noi acum; `ContPl` se adauga doar daca
   operatorul spune ca il vrea (vezi 2b).
2b. `ContPl` (cont de plata) e diferit de `ContIBAN` (altceva decat IBAN-ul de incasare)? Daca da, ce reprezinta?
   Daca nu stii, il lasam deoparte.
2c. (vechea 2) Ce "informatii speciale" primeste un analitic fata de unul simplu (IBAN? cont de plata? legatura la salarii/angajat?
   cod de angajament)? Enumera campurile.
3. Analiticele sunt comune tuturor unitatilor sau proprii fiecareia? (azi K-BOT are `Parteneri` pe `IdUnitate`.)
4. Cum se numeroteaza codul (3 cifre pentru institutii/personal, 4 pentru restul?) - il da utilizatorul sau se genereaza?
5. Un analitic poate fi si furnizor (acelasi cod)? Cum se leaga de cod fiscal cand il are?
