# SLICE-AV01-03 - DefaClsfV extins (cod despartit + Tip VN/CH)

Data: 24.10.2026. Doar un script SQL, nerulat. Niciun cod aplicatie atins.

## Ce s-a facut si de ce
Operatorul vrea nomenclatorul de venituri din Access (`tabledata/DefaClsfV.csv`: ID, C, S, P, D, CV...) in `AVACONT_COMUN.DefaClsfV`.
Comparatie facuta (scripturi in scratchpad, nu in repo): CSV 1404 coduri, dump COMUN 1429; 1403 comune, 1 doar in CSV (`420700`),
26 doar in COMUN; la 789 de coduri denumirile difera (in COMUN au diacritice, in CSV nu). `CV` = `VN` (1078, venituri) / `CH` (326, cheltuieli).
Decizie operator: se EXTINDE tabela (nu se inlocuieste): `ClsfV` + `DENUMIREA` raman; se adauga `Capitol`/`Subcapitol`/`Paragraf`
(coloane generate din cod) si `Tip` (VN/CH). `Field5` si `Field7` nu se aduc (chei interne ale ministerului, nefolosite).
Operatorul incarca CSV-ul in Navicat ca `DefaClsfV_Temp` si ruleaza scriptul de acolo.

## Fisiere atinse
Creat: `sql/AVACONT/Configurari/ClasificatiiVenituri/AV01_03_DefaClsfV_extindere.sql` (DDL, verificari, UPDATE prin JOIN, INSERT pentru codul lipsa, verificari finale).

## Verificari
Codul nu foloseste `DefaClsfV` nicaieri (grep in `.vb`, `.py`, `.sql`, `.js`, `.ps1`, `.json`, `.md`, fara `AVACONT`, `Surse`, `MariaDB_Schema`, `docs`).
Codul din `DefaClsfV_Temp` se reconstruieste cu `LPAD`, fiindca importul din Navicat poate pierde zerourile de la inceput.

## Teste
Niciunul; scriptul NU a fost rulat (nici macar sintactic). Primul `ALTER` foloseste `IF NOT EXISTS`, deci se poate relua.

## Nerezolvat / neverificat
- Tipul coloanelor `C`, `S`, `P` dupa import (text sau numar) nu e cunoscut; `LPAD` acopera ambele cazuri.
- Cele 26 de coduri doar din COMUN raman fara `Tip` (nu sunt in CSV); lista iese din interogarea 2c / 5.
- Denumirile nu au fost comparate ca SENS, doar ca text (diferentele vazute sunt diacritice si punctuatie).
