# VERCON — dictionar de date si graf FK (SLICE-VC00-02)

Generat de `VERCON/tools/dictionar.js` din `TZ_SCHEMA` (export tz_Alimente, referinta comuna decisa de utilizator) si din dump-uri **doar-schema** ale celor trei baze reale (`tools/dump_schema.ps1`, DAO read-only, numarul de randuri, fara continut). Tipurile tinta sunt **propuneri de dictionar** pentru VC03-02, nu DDL aplicat.

Starea: DOCUMENTAT. Nimic implementat, nimic validat pe server.

## 1. Reguli de tip (se aplica uniform pe toate gestiunile)

| Clasa sursa | Tinta MariaDB | Nota |
|---|---|---|
| CHEIE (AutoNumber/Long de legatura) | `INT UNSIGNED` generat | Cheia sursa se citeste **semnata** (ID-uri random pot fi negative) si se pastreaza in `VC_IdMap`. |
| VALOARE-INT (cantitati, preturi, valori) | `INT` (cu semn) | Scalat 10^SCARA, SCARA = scara de stocare a gestiunii (Alimente 4, MF 2, OI 3); afisarea foloseste NR.ZEC al gestiunii, rotunjit in sus la jumatate. Vezi CONTRACT_CALCULE §1. Pretul are CHECK >= 0. |
| COTA (TVA.TVA) | `INT` | cota x 10000 (0,19 -> 1900). |
| NUMAR / INT | `INT` | nu se scaleaza. |
| TEXT | `VARCHAR(n)` utf8mb3_general_ci | fara eliminarea diacriticelor; trunchierea se raporteaza. |
| DATA | `DATETIME` | `Data`, `DataFc`, `DataBC`, `DataSalvare`, `DataIntrare` nu se substituie reciproc. |
| BOOL | `TINYINT(1)` NULL/0/1 | -1 -> 1. |

Fiecare tabel business tinta primeste in plus: `SubunitId`, `GestiuneId` (si `ExercitiuId` unde relatia o cere), cheie sursa in `VC_IdMap`, audit. Fara tabele per an / per fisier / per tip.

## 2. Tabele business din TZ_SCHEMA si tratamentul coloanelor

### Alimente

Randuri (alimente/MF/OI): Alimente: 0; Mijloace_Fixe: 0; Obiecte_Inventar: —

| Camp | Tip Access | Marime | Obligatoriu | Implicit | Clasa | Tinta / conversie |
|---|---|---:|---|---|---|---|
| ID | Long | 4 | nu |  | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| Denumire | Text | 255 | nu |  | TEXT | VARCHAR(255)  utf8mb3_general_ci; zerouri initiale pastrate |
| UM | Text | 255 | nu |  | TEXT | VARCHAR(255)  utf8mb3_general_ci; zerouri initiale pastrate |
| PU | Double | 8 | nu |  | VALOARE-INT | INT scalat 10^SCARA, SCARA pe gestiune; stocat exact, refuz la overflow (nu se rotunjeste la migrare) |
| Cant | Double | 8 | nu |  | VALOARE-INT | INT scalat 10^SCARA, SCARA pe gestiune; stocat exact, refuz la overflow (nu se rotunjeste la migrare) |
| Valoare | Double | 8 | nu |  | VALOARE-INT | INT scalat 10^SCARA, SCARA pe gestiune; stocat exact, refuz la overflow (nu se rotunjeste la migrare) |
| P | Long | 4 | nu | 0 | INT | INT (contor/numar/durata); NU se scaleaza |

Indexuri/chei (export): `ID` (ID) PRIMARY UNIQUE

### Cina

Randuri (alimente/MF/OI): Alimente: 127; Mijloace_Fixe: 0; Obiecte_Inventar: 0

| Camp | Tip Access | Marime | Obligatoriu | Implicit | Clasa | Tinta / conversie |
|---|---|---:|---|---|---|---|
| Id | AutoNumber | 4 | nu |  | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| IdLista | Long | 4 | nu | 0 | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| IdFel | Long | 4 | nu |  | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| Tip | Text | 50 | nu |  | TEXT | VARCHAR(50)  utf8mb3_general_ci; zerouri initiale pastrate |
| Explicatie | Text | 255 | nu |  | TEXT | VARCHAR(255)  utf8mb3_general_ci; zerouri initiale pastrate |

Indexuri/chei (export): `Id` (Id) PRIMARY UNIQUE; `LABC_IdLista__Cina_IdLista` (IdLista) FOREIGN

### Clienti

Randuri (alimente/MF/OI): Alimente: 1; Mijloace_Fixe: 7; Obiecte_Inventar: 105

| Camp | Tip Access | Marime | Obligatoriu | Implicit | Clasa | Tinta / conversie |
|---|---|---:|---|---|---|---|
| IDC | Long | 4 | da |  | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| Denumire | Text | 50 | nu |  | TEXT | VARCHAR(50)  utf8mb3_general_ci; zerouri initiale pastrate |
| Adresa | Text | 50 | nu |  | TEXT | VARCHAR(50)  utf8mb3_general_ci; zerouri initiale pastrate |
| CF | Text | 10 | nu |  | TEXT | VARCHAR(10)  utf8mb3_general_ci; zerouri initiale pastrate |
| J | Text | 20 | nu |  | TEXT | VARCHAR(20)  utf8mb3_general_ci; zerouri initiale pastrate |
| CNP | Text | 13 | nu |  | TEXT | VARCHAR(13)  utf8mb3_general_ci; zerouri initiale pastrate |
| Cont | Text | 50 | nu |  | TEXT | VARCHAR(50)  utf8mb3_general_ci; zerouri initiale pastrate |

Indexuri/chei (export): `IDC` (IDC) PRIMARY UNIQUE

### ComisieInventar

Randuri (alimente/MF/OI): Alimente: 0; Mijloace_Fixe: 0; Obiecte_Inventar: 0

| Camp | Tip Access | Marime | Obligatoriu | Implicit | Clasa | Tinta / conversie |
|---|---|---:|---|---|---|---|
| IdComisieInv | Long | 4 | da | 0 | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| Receptie1 | Text | 50 | nu |  | TEXT | VARCHAR(50)  utf8mb3_general_ci; zerouri initiale pastrate |
| Receptie2 | Text | 50 | nu |  | TEXT | VARCHAR(50)  utf8mb3_general_ci; zerouri initiale pastrate |
| Receptie3 | Text | 50 | nu |  | TEXT | VARCHAR(50)  utf8mb3_general_ci; zerouri initiale pastrate |
| Receptie4 | Text | 50 | nu |  | TEXT | VARCHAR(50)  utf8mb3_general_ci; zerouri initiale pastrate |

Indexuri/chei (export): `IdComisie` (IdComisieInv) PRIMARY UNIQUE

### ComisieNIR

Randuri (alimente/MF/OI): Alimente: 1; Mijloace_Fixe: 1; Obiecte_Inventar: 1

| Camp | Tip Access | Marime | Obligatoriu | Implicit | Clasa | Tinta / conversie |
|---|---|---:|---|---|---|---|
| IdComisie | Long | 4 | da | 0 | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| Receptie1 | Text | 50 | nu |  | TEXT | VARCHAR(50)  utf8mb3_general_ci; zerouri initiale pastrate |
| Receptie2 | Text | 50 | nu |  | TEXT | VARCHAR(50)  utf8mb3_general_ci; zerouri initiale pastrate |
| Receptie3 | Text | 50 | nu |  | TEXT | VARCHAR(50)  utf8mb3_general_ci; zerouri initiale pastrate |
| Receptie4 | Text | 50 | nu |  | TEXT | VARCHAR(50)  utf8mb3_general_ci; zerouri initiale pastrate |

Indexuri/chei (export): `IdComisie` (IdComisie) PRIMARY UNIQUE

### cpv

Randuri (alimente/MF/OI): Alimente: 9454; Mijloace_Fixe: 9454; Obiecte_Inventar: 9454

| Camp | Tip Access | Marime | Obligatoriu | Implicit | Clasa | Tinta / conversie |
|---|---|---:|---|---|---|---|
| CODE | Text | 255 | nu |  | TEXT | VARCHAR(255)  utf8mb3_general_ci; zerouri initiale pastrate |
| RO | Text | 255 | nu |  | TEXT | VARCHAR(255)  utf8mb3_general_ci; zerouri initiale pastrate |

Indexuri/chei (export): `CODE` (CODE) PRIMARY UNIQUE

### Dejun

Randuri (alimente/MF/OI): Alimente: 401; Mijloace_Fixe: 0; Obiecte_Inventar: 0

| Camp | Tip Access | Marime | Obligatoriu | Implicit | Clasa | Tinta / conversie |
|---|---|---:|---|---|---|---|
| Id | AutoNumber | 4 | nu |  | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| IdLista | Long | 4 | nu | 0 | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| IdFel | Long | 4 | nu |  | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| Tip | Text | 50 | nu |  | TEXT | VARCHAR(50)  utf8mb3_general_ci; zerouri initiale pastrate |
| Explicatie | Text | 255 | nu |  | TEXT | VARCHAR(255)  utf8mb3_general_ci; zerouri initiale pastrate |

Indexuri/chei (export): `Id` (Id) PRIMARY UNIQUE; `LABC_IdLista__Dejun_IdLista` (IdLista) FOREIGN

### FeluriMancare

Randuri (alimente/MF/OI): Alimente: 0; Mijloace_Fixe: 0; Obiecte_Inventar: 0

| Camp | Tip Access | Marime | Obligatoriu | Implicit | Clasa | Tinta / conversie |
|---|---|---:|---|---|---|---|
| IDFel | Long | 4 | da |  | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| DenFel | Text | 255 | nu |  | TEXT | VARCHAR(255)  utf8mb3_general_ci; zerouri initiale pastrate |
| Explicatie | Text | 255 | nu |  | TEXT | VARCHAR(255)  utf8mb3_general_ci; zerouri initiale pastrate |

Indexuri/chei (export): `IDFel` (IDFel) PRIMARY UNIQUE

### Furnizori

Randuri (alimente/MF/OI): Alimente: 28; Mijloace_Fixe: 2; Obiecte_Inventar: 5

| Camp | Tip Access | Marime | Obligatoriu | Implicit | Clasa | Tinta / conversie |
|---|---|---:|---|---|---|---|
| IDF | Long | 4 | da |  | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| Denumire | Text | 255 | nu |  | TEXT | VARCHAR(255)  utf8mb3_general_ci; zerouri initiale pastrate |
| Adresa | Text | 255 | nu |  | TEXT | VARCHAR(255)  utf8mb3_general_ci; zerouri initiale pastrate |
| CF | Text | 255 | nu |  | TEXT | VARCHAR(255)  utf8mb3_general_ci; zerouri initiale pastrate |
| J | Text | 255 | nu |  | TEXT | VARCHAR(255)  utf8mb3_general_ci; zerouri initiale pastrate |
| Cont | Text | 255 | nu |  | TEXT | VARCHAR(255)  utf8mb3_general_ci; zerouri initiale pastrate |
| TIP | Text | 255 | nu |  | TEXT | VARCHAR(255)  utf8mb3_general_ci; zerouri initiale pastrate |
| ONLINE | Yes/No | 1 | nu |  | BOOL | TINYINT(1) NULL/0/1 (Access -1 -> 1); NULL separat |
| Orasul | Text | 255 | nu |  | TEXT | VARCHAR(255)  utf8mb3_general_ci; zerouri initiale pastrate |

Indexuri/chei (export): `IDF` (IDF) PRIMARY UNIQUE

### Iesiri

Randuri (alimente/MF/OI): Alimente: 3196; Mijloace_Fixe: 154; Obiecte_Inventar: 1529

| Camp | Tip Access | Marime | Obligatoriu | Implicit | Clasa | Tinta / conversie |
|---|---|---:|---|---|---|---|
| ID | AutoNumber | 4 | nu |  | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| IdBonConsum | Long | 4 | nu | 0 | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| IdProdus | Long | 4 | nu |  | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| Cant | Double | 8 | nu | 0 | VALOARE-INT | INT scalat 10^SCARA, SCARA pe gestiune; stocat exact, refuz la overflow (nu se rotunjeste la migrare) |
| PUTVA | Double | 8 | nu | 0 | VALOARE-INT | INT scalat 10^SCARA, SCARA pe gestiune; stocat exact, refuz la overflow (nu se rotunjeste la migrare) |
| ValoareCUTVA | Double | 8 | nu |  | VALOARE-INT | INT scalat 10^SCARA, SCARA pe gestiune; stocat exact, refuz la overflow (nu se rotunjeste la migrare) |
| DataSalvare | Date/Time | 8 | nu |  | DATA | DATETIME, data locala fara conversie UTC |

Indexuri/chei (export): `ID` (ID) PRIMARY UNIQUE; `IdBonConsum` (IdBonConsum); `IdProdus` (IdProdus); `Iesiri_H_IdBonConsum__Iesiri_IdBonConsum` (IdBonConsum) FOREIGN; `ProdusePIesiri` (IdProdus) FOREIGN

### IesiriRetur

Randuri (alimente/MF/OI): Alimente: 0; Mijloace_Fixe: 0; Obiecte_Inventar: 0

| Camp | Tip Access | Marime | Obligatoriu | Implicit | Clasa | Tinta / conversie |
|---|---|---:|---|---|---|---|
| ID | AutoNumber | 4 | nu |  | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| IdBonConsum | Long | 4 | nu |  | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| IdProdus | Long | 4 | nu |  | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| Cant | Double | 8 | nu | 0 | VALOARE-INT | INT scalat 10^SCARA, SCARA pe gestiune; stocat exact, refuz la overflow (nu se rotunjeste la migrare) |
| PUTVA | Long | 4 | nu |  | VALOARE-INT | INT scalat 10^SCARA, SCARA pe gestiune (sursa Long: fara fractiune de inventat) |
| DataSalvare | Date/Time | 8 | nu | Now() | DATA | DATETIME, data locala fara conversie UTC |

Indexuri/chei (export): `ID` (ID) PRIMARY UNIQUE; `idbc` (IdBonConsum); `IdProdus` (IdProdus); `Iesiri_HIesiriRetur` (IdBonConsum) FOREIGN; `ProdusePIesiriRetur` (IdProdus) FOREIGN

### Iesiri_H

Randuri (alimente/MF/OI): Alimente: 127; Mijloace_Fixe: 7; Obiecte_Inventar: 96

| Camp | Tip Access | Marime | Obligatoriu | Implicit | Clasa | Tinta / conversie |
|---|---|---:|---|---|---|---|
| IdBonConsum | Long | 4 | da | 0 | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| IdMasina | Long | 4 | nu | 0 | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| IdRetur | Long | 4 | nu | 0 | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| IDC | Long | 4 | nu | 0 | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| NumarBC | Text | 255 | nu |  | TEXT | VARCHAR(255)  utf8mb3_general_ci; zerouri initiale pastrate |
| DataBC | Date/Time | 8 | nu |  | DATA | DATETIME, data locala fara conversie UTC |
| ValoareBC | Double | 8 | nu | 0 | VALOARE-INT | INT scalat 10^SCARA, SCARA pe gestiune; stocat exact, refuz la overflow (nu se rotunjeste la migrare) |
| Expl | Text | 255 | nu |  | TEXT | VARCHAR(255)  utf8mb3_general_ci; zerouri initiale pastrate |
| Retur | Yes/No | 1 | nu | No | BOOL | TINYINT(1) NULL/0/1 (Access -1 -> 1); NULL separat |
| DataSalvare | Date/Time | 8 | nu | Now() | DATA | DATETIME, data locala fara conversie UTC |

Indexuri/chei (export): `Clienti_IDC__Iesiri_H_IDC` (IDC) FOREIGN; `IdBonConsum` (IdBonConsum) PRIMARY UNIQUE; `IDC` (IDC); `IdMasina` (IdMasina); `IdRetur` (IdRetur)

### Intrari

Randuri (alimente/MF/OI): Alimente: 1671; Mijloace_Fixe: 156; Obiecte_Inventar: 1137

| Camp | Tip Access | Marime | Obligatoriu | Implicit | Clasa | Tinta / conversie |
|---|---|---:|---|---|---|---|
| ID | AutoNumber | 4 | nu |  | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| IdIntrare | Long | 4 | nu | 0 | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| IdProdus | Long | 4 | nu |  | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| IDTVA | Long | 4 | nu | 0 | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| Cant | Double | 8 | nu | 0 | VALOARE-INT | INT scalat 10^SCARA, SCARA pe gestiune; stocat exact, refuz la overflow (nu se rotunjeste la migrare) |
| Pret | Double | 8 | nu | 0 | VALOARE-INT | INT scalat 10^SCARA, SCARA pe gestiune; stocat exact, refuz la overflow (nu se rotunjeste la migrare) |
| TVA | Double | 8 | nu | 0 | VALOARE-INT | INT scalat 10^SCARA, SCARA pe gestiune; stocat exact, refuz la overflow (nu se rotunjeste la migrare) |
| Valoare | Double | 8 | nu | 0 | VALOARE-INT | INT scalat 10^SCARA, SCARA pe gestiune; stocat exact, refuz la overflow (nu se rotunjeste la migrare) |
| PUTVA | Double | 8 | nu | 0 | VALOARE-INT | INT scalat 10^SCARA, SCARA pe gestiune; stocat exact, refuz la overflow (nu se rotunjeste la migrare) |
| ValoareCUTVA | Double | 8 | nu |  | VALOARE-INT | INT scalat 10^SCARA, SCARA pe gestiune; stocat exact, refuz la overflow (nu se rotunjeste la migrare) |
| DataSalvare | Date/Time | 8 | nu |  | DATA | DATETIME, data locala fara conversie UTC |

Indexuri/chei (export): `ID` (ID) PRIMARY UNIQUE; `IdProdus` (IdProdus); `NIR_IdIntrare__Intrari_IdIntrare` (IdIntrare) FOREIGN; `ProdusePIntrari` (IdProdus) FOREIGN; `PUTVA` (PUTVA); `TVAIntrari` (IDTVA) FOREIGN

### IntrariStorno

Randuri (alimente/MF/OI): Alimente: 8; Mijloace_Fixe: 2; Obiecte_Inventar: 2

| Camp | Tip Access | Marime | Obligatoriu | Implicit | Clasa | Tinta / conversie |
|---|---|---:|---|---|---|---|
| ID | AutoNumber | 4 | nu |  | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| IdIntrare | Long | 4 | nu |  | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| IdProdus | Long | 4 | nu |  | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| Cant | Double | 8 | nu | 0 | VALOARE-INT | INT scalat 10^SCARA, SCARA pe gestiune; stocat exact, refuz la overflow (nu se rotunjeste la migrare) |
| PUTVA | Long | 4 | nu |  | VALOARE-INT | INT scalat 10^SCARA, SCARA pe gestiune (sursa Long: fara fractiune de inventat) |
| DataSalvare | Date/Time | 8 | nu | Now() | DATA | DATETIME, data locala fara conversie UTC |

Indexuri/chei (export): `ID` (ID) PRIMARY UNIQUE; `idintrare` (IdIntrare); `IdProdus` (IdProdus)

### LABC

Randuri (alimente/MF/OI): Alimente: 127; Mijloace_Fixe: 0; Obiecte_Inventar: 0

| Camp | Tip Access | Marime | Obligatoriu | Implicit | Clasa | Tinta / conversie |
|---|---|---:|---|---|---|---|
| ID | AutoNumber | 4 | nu |  | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| IdLista | Long | 4 | da | 0 | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| NumarLA | Long | 4 | nu | 0 | INT | INT (contor/numar/durata); NU se scaleaza |
| DATALA | Date/Time | 8 | nu |  | DATA | DATETIME, data locala fara conversie UTC |
| ValoareBon | Double | 8 | nu | 0 | VALOARE-INT | INT scalat 10^SCARA, SCARA pe gestiune; stocat exact, refuz la overflow (nu se rotunjeste la migrare) |
| DEP | Double | 8 | nu | 0 | VALOARE-INT | INT scalat 10^SCARA, SCARA pe gestiune; stocat exact, refuz la overflow (nu se rotunjeste la migrare) |
| ValoareLista | Double | 8 | nu |  | VALOARE-INT | INT scalat 10^SCARA, SCARA pe gestiune; stocat exact, refuz la overflow (nu se rotunjeste la migrare) |

Indexuri/chei (export): `IdLista` (IdLista) PRIMARY UNIQUE

### LegBCLA

Randuri (alimente/MF/OI): Alimente: 127; Mijloace_Fixe: 0; Obiecte_Inventar: 0

| Camp | Tip Access | Marime | Obligatoriu | Implicit | Clasa | Tinta / conversie |
|---|---|---:|---|---|---|---|
| IdBonConsum | Long | 4 | nu | 0 | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| IdLista | Long | 4 | da | 0 | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |

Indexuri/chei (export): `IdLista` (IdLista) PRIMARY UNIQUE; `Iesiri_HLegBCLA` (IdBonConsum) FOREIGN; `LABCLegBCLA` (IdLista) UNIQUE FOREIGN

### ListaAlimente

Randuri (alimente/MF/OI): Alimente: 127; Mijloace_Fixe: 0; Obiecte_Inventar: 0

| Camp | Tip Access | Marime | Obligatoriu | Implicit | Clasa | Tinta / conversie |
|---|---|---:|---|---|---|---|
| ID | AutoNumber | 4 | nu |  | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| IDLISTA | Long | 4 | da |  | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| MicDejun_P | Long | 4 | nu | 0 | INT | INT (contor/numar/durata); NU se scaleaza |
| VMD | Double | 8 | nu | 0 | VALOARE-INT | INT scalat 10^SCARA, SCARA pe gestiune; stocat exact, refuz la overflow (nu se rotunjeste la migrare) |
| MicDejun_V | Double | 8 | nu | 0 | VALOARE-INT | INT scalat 10^SCARA, SCARA pe gestiune; stocat exact, refuz la overflow (nu se rotunjeste la migrare) |
| MicDejun_D | Long | 4 | nu | 0 | INT | INT (contor/numar/durata); NU se scaleaza |
| Dejun_P | Long | 4 | nu | 0 | INT | INT (contor/numar/durata); NU se scaleaza |
| VD | Double | 8 | nu | 0 | VALOARE-INT | INT scalat 10^SCARA, SCARA pe gestiune; stocat exact, refuz la overflow (nu se rotunjeste la migrare) |
| Dejun_V | Double | 8 | nu | 0 | VALOARE-INT | INT scalat 10^SCARA, SCARA pe gestiune; stocat exact, refuz la overflow (nu se rotunjeste la migrare) |
| Dejun_D | Long | 4 | nu | 0 | INT | INT (contor/numar/durata); NU se scaleaza |
| Cina_P | Long | 4 | nu | 0 | INT | INT (contor/numar/durata); NU se scaleaza |
| VC | Double | 8 | nu | 0 | VALOARE-INT | INT scalat 10^SCARA, SCARA pe gestiune; stocat exact, refuz la overflow (nu se rotunjeste la migrare) |
| Cina_V | Double | 8 | nu | 0 | VALOARE-INT | INT scalat 10^SCARA, SCARA pe gestiune; stocat exact, refuz la overflow (nu se rotunjeste la migrare) |
| Cina_D | Long | 4 | nu | 0 | INT | INT (contor/numar/durata); NU se scaleaza |
| Comentarii | Text | 255 | nu |  | TEXT | VARCHAR(255)  utf8mb3_general_ci; zerouri initiale pastrate |

Indexuri/chei (export): `ID` (ID) PRIMARY UNIQUE; `LABC_IdLista__ListaAlimente_IDLISTA` (IDLISTA) FOREIGN

### Masini

Randuri (alimente/MF/OI): Alimente: 4; Mijloace_Fixe: 2; Obiecte_Inventar: 2

| Camp | Tip Access | Marime | Obligatoriu | Implicit | Clasa | Tinta / conversie |
|---|---|---:|---|---|---|---|
| IdMasina | Long | 4 | da | 0 | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| Masina | Text | 50 | nu |  | TEXT | VARCHAR(50)  utf8mb3_general_ci; zerouri initiale pastrate |
| TIP | Text | 255 | nu |  | TEXT | VARCHAR(255)  utf8mb3_general_ci; zerouri initiale pastrate |

Indexuri/chei (export): `IdMasina` (IdMasina) PRIMARY UNIQUE

### MicDejun

Randuri (alimente/MF/OI): Alimente: 127; Mijloace_Fixe: 0; Obiecte_Inventar: 0

| Camp | Tip Access | Marime | Obligatoriu | Implicit | Clasa | Tinta / conversie |
|---|---|---:|---|---|---|---|
| Id | AutoNumber | 4 | nu |  | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| IdLista | Long | 4 | nu | 0 | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| IdFel | Long | 4 | nu |  | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| Tip | Text | 50 | nu |  | TEXT | VARCHAR(50)  utf8mb3_general_ci; zerouri initiale pastrate |
| Explicatie | Text | 255 | nu |  | TEXT | VARCHAR(255)  utf8mb3_general_ci; zerouri initiale pastrate |

Indexuri/chei (export): `ID` (Id) PRIMARY UNIQUE; `LABC_IdLista__MicDejun_IdLista` (IdLista) FOREIGN

### NIR

Randuri (alimente/MF/OI): Alimente: 192; Mijloace_Fixe: 2; Obiecte_Inventar: 4

| Camp | Tip Access | Marime | Obligatoriu | Implicit | Clasa | Tinta / conversie |
|---|---|---:|---|---|---|---|
| IdIntrare | Long | 4 | da | 0 | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| IdComisie | Long | 4 | nu | 0 | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| IdMasina | Long | 4 | nu | 0 | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| IDF | Long | 4 | nu |  | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| IDFV | Long | 4 | nu |  | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| IdStorno | Long | 4 | nu | 0 | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| NrFact | Text | 255 | nu |  | TEXT | VARCHAR(255)  utf8mb3_general_ci; zerouri initiale pastrate |
| Data | Date/Time | 8 | nu |  | DATA | DATETIME, data locala fara conversie UTC |
| Numar | Double | 8 | nu | 0 | NUMAR | INT (numar document; negativ la storno/seria istorica); NU se scaleaza |
| Valoare | Double | 8 | nu |  | VALOARE-INT | INT scalat 10^SCARA, SCARA pe gestiune; stocat exact, refuz la overflow (nu se rotunjeste la migrare) |
| DataFc | Date/Time | 8 | nu |  | DATA | DATETIME, data locala fara conversie UTC |
| Storno | Yes/No | 1 | nu | No | BOOL | TINYINT(1) NULL/0/1 (Access -1 -> 1); NULL separat |
| FurnizorV | Text | 255 | nu |  | TEXT | VARCHAR(255)  utf8mb3_general_ci; zerouri initiale pastrate |

Indexuri/chei (export): `ComisieNIRNIR` (IdComisie) FOREIGN; `Furnizori_IDF__NIR_IDF` (IDF) FOREIGN; `IdComisie` (IdComisie); `IDF` (IDF); `IdIntrare` (IdIntrare) PRIMARY UNIQUE; `IdMasina` (IdMasina); `IdStorno` (IdStorno)

### NR

Randuri (alimente/MF/OI): Alimente: 1; Mijloace_Fixe: 1; Obiecte_Inventar: 1

| Camp | Tip Access | Marime | Obligatoriu | Implicit | Clasa | Tinta / conversie |
|---|---|---:|---|---|---|---|
| ID | AutoNumber | 4 | nu |  | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| NR | Long | 4 | nu | 0 | INT | INT (contor/numar/durata); NU se scaleaza |
| NUM | Text | 50 | nu |  | TEXT | VARCHAR(50)  utf8mb3_general_ci; zerouri initiale pastrate |
| NIR | Long | 4 | nu | 0 | INT | INT (contor/numar/durata); NU se scaleaza |
| NRFCT | Long | 4 | nu | 0 | INT | INT (contor/numar/durata); NU se scaleaza |
| NRBC | Long | 4 | nu | 0 | INT | INT (contor/numar/durata); NU se scaleaza |
| SI_LA | Double | 8 | nu | 0 | VALOARE-INT | INT scalat 10^SCARA, SCARA pe gestiune; stocat exact, refuz la overflow (nu se rotunjeste la migrare) |
| ZEC | Long | 4 | nu |  | INT | INT (contor/numar/durata); NU se scaleaza |

Indexuri/chei (export): `ID` (ID) PRIMARY UNIQUE

### OI_Casare

Randuri (alimente/MF/OI): Alimente: 0; Mijloace_Fixe: 0; Obiecte_Inventar: 0

| Camp | Tip Access | Marime | Obligatoriu | Implicit | Clasa | Tinta / conversie |
|---|---|---:|---|---|---|---|
| ID | AutoNumber | 4 | nu |  | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| IDBC | Long | 4 | da | 0 | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| IdMasina | Long | 4 | nu | 0 | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| IDC_BC | Long | 4 | nu | 0 | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| IdProdus | Long | 4 | nu |  | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| NRBC | Text | 255 | nu |  | TEXT | VARCHAR(255)  utf8mb3_general_ci; zerouri initiale pastrate |
| DATABC | Date/Time | 8 | nu |  | DATA | DATETIME, data locala fara conversie UTC |
| Cant | Double | 8 | nu | 0 | VALOARE-INT | INT scalat 10^SCARA, SCARA pe gestiune; stocat exact, refuz la overflow (nu se rotunjeste la migrare) |
| PUTVA | Double | 8 | nu | 0 | VALOARE-INT | INT scalat 10^SCARA, SCARA pe gestiune; stocat exact, refuz la overflow (nu se rotunjeste la migrare) |
| ValoareCUTVA | Double | 8 | nu | 0 | VALOARE-INT | INT scalat 10^SCARA, SCARA pe gestiune; stocat exact, refuz la overflow (nu se rotunjeste la migrare) |
| Expl | Text | 255 | nu |  | TEXT | VARCHAR(255)  utf8mb3_general_ci; zerouri initiale pastrate |
| DataSalvare | Date/Time | 8 | nu |  | DATA | DATETIME, data locala fara conversie UTC |

Indexuri/chei (export): `Clienti_IDC__OI_Casare_IDC_BC` (IDC_BC) FOREIGN; `ID` (ID) PRIMARY UNIQUE; `IDC_BC` (IDC_BC); `IdMasina` (IdMasina); `IdProdus` (IdProdus); `NRBC` (NRBC); `OI_CasareIDBC` (IDBC); `ProdusePOI_Casare` (IdProdus) FOREIGN; `PUTVA` (PUTVA)

### OI_Predare

Randuri (alimente/MF/OI): Alimente: 0; Mijloace_Fixe: 154; Obiecte_Inventar: 1529

| Camp | Tip Access | Marime | Obligatoriu | Implicit | Clasa | Tinta / conversie |
|---|---|---:|---|---|---|---|
| ID | AutoNumber | 4 | nu |  | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| IDBP | Long | 4 | da | 0 | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| IdBonConsum | Long | 4 | nu | 0 | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| IdMasina | Long | 4 | nu | 0 | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| IdMasinaP | Long | 4 | nu | 0 | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| IDC_BP | Long | 4 | nu | 0 | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| IdProdus | Long | 4 | nu |  | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| NRBP | Text | 255 | nu |  | TEXT | VARCHAR(255)  utf8mb3_general_ci; zerouri initiale pastrate |
| DATABP | Date/Time | 8 | nu |  | DATA | DATETIME, data locala fara conversie UTC |
| Cant | Double | 8 | nu | 0 | VALOARE-INT | INT scalat 10^SCARA, SCARA pe gestiune; stocat exact, refuz la overflow (nu se rotunjeste la migrare) |
| PUTVA | Double | 8 | nu | 0 | VALOARE-INT | INT scalat 10^SCARA, SCARA pe gestiune; stocat exact, refuz la overflow (nu se rotunjeste la migrare) |
| ValoareCUTVA | Double | 8 | nu | 0 | VALOARE-INT | INT scalat 10^SCARA, SCARA pe gestiune; stocat exact, refuz la overflow (nu se rotunjeste la migrare) |
| Expl | Text | 255 | nu |  | TEXT | VARCHAR(255)  utf8mb3_general_ci; zerouri initiale pastrate |
| DataSalvare | Date/Time | 8 | nu |  | DATA | DATETIME, data locala fara conversie UTC |

Indexuri/chei (export): `Clienti_IDC__OI_Predare_IDC_BP` (IDC_BP) FOREIGN; `ID` (ID) PRIMARY UNIQUE; `IdBonConsum` (IdBonConsum); `IDC_BP` (IDC_BP); `IdMasina` (IdMasina); `IdMasinaP` (IdMasinaP); `IdProdus` (IdProdus); `NRBP` (NRBP); `OI_PredareIDBP` (IDBP); `ProdusePOI_Predare` (IdProdus) FOREIGN; `PUTVA` (PUTVA)

### OI_Produse

Randuri (alimente/MF/OI): Alimente: 0; Mijloace_Fixe: 154; Obiecte_Inventar: 1529

| Camp | Tip Access | Marime | Obligatoriu | Implicit | Clasa | Tinta / conversie |
|---|---|---:|---|---|---|---|
| IDOI | AutoNumber | 4 | nu |  | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| IDBP | Long | 4 | nu | 0 | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| IDBT | Long | 4 | nu | 0 | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| IDBC | Long | 4 | nu | 0 | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| IDBO | Long | 4 | nu | 0 | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| IDM | Long | 4 | nu | 0 | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| IDC_PTC | Long | 4 | nu | 0 | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| IdProdus | Long | 4 | nu |  | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| Cant | Double | 8 | nu | 0 | VALOARE-INT | INT scalat 10^SCARA, SCARA pe gestiune; stocat exact, refuz la overflow (nu se rotunjeste la migrare) |
| PUTVA | Double | 8 | nu | 0 | VALOARE-INT | INT scalat 10^SCARA, SCARA pe gestiune; stocat exact, refuz la overflow (nu se rotunjeste la migrare) |
| ValoareCUTVA | Double | 8 | nu | 0 | VALOARE-INT | INT scalat 10^SCARA, SCARA pe gestiune; stocat exact, refuz la overflow (nu se rotunjeste la migrare) |
| DataSalvare | Date/Time | 8 | nu |  | DATA | DATETIME, data locala fara conversie UTC |

Indexuri/chei (export): `Clienti_IDC__OI_Produse_IDC_PTC` (IDC_PTC) FOREIGN; `IDBC` (IDBC); `IDBO` (IDBO); `IDBP` (IDBP); `IDBT` (IDBT); `IDC_PTC` (IDC_PTC); `IDM` (IDM); `IDOI` (IDOI) PRIMARY UNIQUE; `IdProdus` (IdProdus); `ProdusePOI_Produse` (IdProdus) FOREIGN; `PUTVA` (PUTVA)

### OI_Transfer

Randuri (alimente/MF/OI): Alimente: 0; Mijloace_Fixe: 0; Obiecte_Inventar: 0

| Camp | Tip Access | Marime | Obligatoriu | Implicit | Clasa | Tinta / conversie |
|---|---|---:|---|---|---|---|
| ID | AutoNumber | 4 | nu |  | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| IDBT | Long | 4 | da | 0 | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| IdMasina | Long | 4 | nu | 0 | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| IDC_BT | Long | 4 | nu | 0 | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| IDC_OLD | Long | 4 | nu | 0 | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| IdProdus | Long | 4 | nu |  | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| NRBT | Text | 255 | nu |  | TEXT | VARCHAR(255)  utf8mb3_general_ci; zerouri initiale pastrate |
| DATABT | Date/Time | 8 | nu |  | DATA | DATETIME, data locala fara conversie UTC |
| Cant | Double | 8 | nu | 0 | VALOARE-INT | INT scalat 10^SCARA, SCARA pe gestiune; stocat exact, refuz la overflow (nu se rotunjeste la migrare) |
| PUTVA | Double | 8 | nu | 0 | VALOARE-INT | INT scalat 10^SCARA, SCARA pe gestiune; stocat exact, refuz la overflow (nu se rotunjeste la migrare) |
| ValoareCUTVA | Double | 8 | nu | 0 | VALOARE-INT | INT scalat 10^SCARA, SCARA pe gestiune; stocat exact, refuz la overflow (nu se rotunjeste la migrare) |
| Expl | Text | 255 | nu |  | TEXT | VARCHAR(255)  utf8mb3_general_ci; zerouri initiale pastrate |
| DataSalvare | Date/Time | 8 | nu |  | DATA | DATETIME, data locala fara conversie UTC |

Indexuri/chei (export): `Clienti_IDC__OI_Transfer_IDC_BT` (IDC_BT) FOREIGN; `ID` (ID) PRIMARY UNIQUE; `IDBT` (IDBT); `IDC_BT` (IDC_BT); `IDC_OLD` (IDC_OLD); `IdMasina` (IdMasina); `IdProdus` (IdProdus); `NRBT` (NRBT); `ProdusePOI_Transfer` (IdProdus) FOREIGN; `PUTVA` (PUTVA)

### Produse

Randuri (alimente/MF/OI): Alimente: 274; Mijloace_Fixe: 41; Obiecte_Inventar: 951

| Camp | Tip Access | Marime | Obligatoriu | Implicit | Clasa | Tinta / conversie |
|---|---|---:|---|---|---|---|
| ID | AutoNumber | 4 | nu | GenUniqueID() | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| IdMasina | Long | 4 | nu | 0 | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| DenumireP | Text | 255 | nu |  | TEXT | VARCHAR(255)  utf8mb3_general_ci; zerouri initiale pastrate |
| UM | Text | 50 | nu |  | TEXT | VARCHAR(50)  utf8mb3_general_ci; zerouri initiale pastrate |
| CPV | Text | 255 | nu |  | TEXT | VARCHAR(255)  utf8mb3_general_ci; zerouri initiale pastrate |
| CC | Text | 255 | nu |  | TEXT | VARCHAR(255)  utf8mb3_general_ci; zerouri initiale pastrate |
| DataSalvare | Date/Time | 8 | nu | Now() | DATA | DATETIME, data locala fara conversie UTC |
| DNF | Long | 4 | nu | 0 | INT | INT (contor/numar/durata); NU se scaleaza |

Indexuri/chei (export): `ID` (ID) PRIMARY UNIQUE; `IdMasina` (IdMasina); `Masini_IdMasina__Produse_IdMasina` (IdMasina) FOREIGN

### ProduseM

Randuri (alimente/MF/OI): Alimente: 0; Mijloace_Fixe: 12; Obiecte_Inventar: —

| Camp | Tip Access | Marime | Obligatoriu | Implicit | Clasa | Tinta / conversie |
|---|---|---:|---|---|---|---|
| IDPMF | AutoNumber | 4 | nu |  | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| IdProdus | Long | 4 | nu |  | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| DataIntrare | Date/Time | 8 | nu |  | DATA | DATETIME, data locala fara conversie UTC |
| DNF | Long | 4 | nu |  | INT | INT (contor/numar/durata); NU se scaleaza |
| DF | Long | 4 | nu |  | INT | INT (contor/numar/durata); NU se scaleaza |
| ValoareLuna | Double | 8 | nu | 0 | VALOARE-INT | INT scalat 10^SCARA, SCARA pe gestiune; stocat exact, refuz la overflow (nu se rotunjeste la migrare) |

Indexuri/chei (export): —

### ProduseP

Randuri (alimente/MF/OI): Alimente: 2304; Mijloace_Fixe: 156; Obiecte_Inventar: 1144

| Camp | Tip Access | Marime | Obligatoriu | Implicit | Clasa | Tinta / conversie |
|---|---|---:|---|---|---|---|
| IdProdus | AutoNumber | 4 | nu |  | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| ID | Long | 4 | nu | 0 | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| PUTVA | Double | 8 | nu | 0 | VALOARE-INT | INT scalat 10^SCARA, SCARA pe gestiune; stocat exact, refuz la overflow (nu se rotunjeste la migrare) |
| DataSalvare | Date/Time | 8 | nu | Now() | DATA | DATETIME, data locala fara conversie UTC |
| COD | Text | 255 | nu |  | TEXT | VARCHAR(255)  utf8mb3_general_ci; zerouri initiale pastrate |

Indexuri/chei (export): `ID` (ID); `PrimaryKey` (IdProdus) PRIMARY UNIQUE; `ProduseProduseP` (ID) FOREIGN

### Retetar

Randuri (alimente/MF/OI): Alimente: 0; Mijloace_Fixe: 0; Obiecte_Inventar: 0

| Camp | Tip Access | Marime | Obligatoriu | Implicit | Clasa | Tinta / conversie |
|---|---|---:|---|---|---|---|
| IdFel | Long | 4 | da |  | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| Continut | Text | 255 | nu |  | TEXT | VARCHAR(255)  utf8mb3_general_ci; zerouri initiale pastrate |
| Cantitate | Double | 8 | nu |  | VALOARE-INT | INT scalat 10^SCARA, SCARA pe gestiune; stocat exact, refuz la overflow (nu se rotunjeste la migrare) |
| AlteDetalii | Text | 255 | nu |  | TEXT | VARCHAR(255)  utf8mb3_general_ci; zerouri initiale pastrate |
| UM | Text | 255 | nu |  | TEXT | VARCHAR(255)  utf8mb3_general_ci; zerouri initiale pastrate |
| CPV | Text | 255 | nu |  | TEXT | VARCHAR(255)  utf8mb3_general_ci; zerouri initiale pastrate |

Indexuri/chei (export): `IdFel` (IdFel) PRIMARY UNIQUE

### Setari

Randuri (alimente/MF/OI): Alimente: 6; Mijloace_Fixe: 6; Obiecte_Inventar: 5

| Camp | Tip Access | Marime | Obligatoriu | Implicit | Clasa | Tinta / conversie |
|---|---|---:|---|---|---|---|
| ID | AutoNumber | 4 | nu |  | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| S | Text | 255 | nu |  | TEXT | VARCHAR(255)  utf8mb3_general_ci; zerouri initiale pastrate |
| V | Double | 8 | nu | 0 | CONFIG | nu se copiaza; cheie tipizata in VC_Settings (INT); valorile reale: -1/0 sau ZEC |

Indexuri/chei (export): `ID` (ID); `PrimaryKey` (ID) PRIMARY UNIQUE

### TVA

Randuri (alimente/MF/OI): Alimente: 6; Mijloace_Fixe: 4; Obiecte_Inventar: 4

| Camp | Tip Access | Marime | Obligatoriu | Implicit | Clasa | Tinta / conversie |
|---|---|---:|---|---|---|---|
| IdTVA | Long | 4 | da | 0 | CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| TVA | Double | 8 | nu | 0 | COTA | INT = cota x 10000 (scara fixa a cotei, independenta de zecimalele subunitatii) |
| Explicatie | Text | 50 | nu |  | TEXT | VARCHAR(50)  utf8mb3_general_ci; zerouri initiale pastrate |

Indexuri/chei (export): `IdTVA` (IdTVA) PRIMARY UNIQUE

### UNIT

Randuri (alimente/MF/OI): Alimente: 1; Mijloace_Fixe: 1; Obiecte_Inventar: —

| Camp | Tip Access | Marime | Obligatoriu | Implicit | Clasa | Tinta / conversie |
|---|---|---:|---|---|---|---|
| NumeUnitate | Text | 255 | nu |  | TEXT | VARCHAR(255)  utf8mb3_general_ci; zerouri initiale pastrate |
| CUI | Text | 50 | nu |  | TEXT | VARCHAR(50)  utf8mb3_general_ci; zerouri initiale pastrate |
| Adresa | Text | 255 | nu |  | TEXT | VARCHAR(255)  utf8mb3_general_ci; zerouri initiale pastrate |
| Director | Text | 255 | nu |  | TEXT | VARCHAR(255)  utf8mb3_general_ci; zerouri initiale pastrate |
| Contabil | Text | 255 | nu |  | TEXT | VARCHAR(255)  utf8mb3_general_ci; zerouri initiale pastrate |
| Culoare | Text | 255 | nu |  | TEXT | VARCHAR(255)  utf8mb3_general_ci; zerouri initiale pastrate |
| Expirat | Yes/No | 1 | nu |  | BOOL | TINYINT(1) NULL/0/1 (Access -1 -> 1); NULL separat |
| Secretar | Text | 255 | nu |  | TEXT | VARCHAR(255)  utf8mb3_general_ci; zerouri initiale pastrate |
| SursaCont | Text | 255 | nu |  | TEXT | VARCHAR(255)  utf8mb3_general_ci; zerouri initiale pastrate |
| AdresaMail | Text | 255 | nu |  | TEXT | VARCHAR(255)  utf8mb3_general_ci; zerouri initiale pastrate |
| Orasul | Text | 255 | nu |  | TEXT | VARCHAR(255)  utf8mb3_general_ci; zerouri initiale pastrate |
| TelefonContact | Text | 255 | nu |  | TEXT | VARCHAR(255)  utf8mb3_general_ci; zerouri initiale pastrate |
| SectorSursa | Text | 255 | nu |  | TEXT | VARCHAR(255)  utf8mb3_general_ci; zerouri initiale pastrate |
| AlteDetalii | Text | 255 | nu |  | TEXT | VARCHAR(255)  utf8mb3_general_ci; zerouri initiale pastrate |
| AnDate | Long | 4 | nu | 0 | INT | INT (contor/numar/durata); NU se scaleaza |
| DC | Text | 255 | nu |  | TEXT | VARCHAR(255)  utf8mb3_general_ci; zerouri initiale pastrate |
| Alte | Text | 255 | nu |  | TEXT | VARCHAR(255)  utf8mb3_general_ci; zerouri initiale pastrate |
| Gestionar | Text | 255 | nu |  | TEXT | VARCHAR(255)  utf8mb3_general_ci; zerouri initiale pastrate |
| CodProgram | Text | 255 | nu | 0000000000 | TEXT | VARCHAR(255)  utf8mb3_general_ci; zerouri initiale pastrate |
| Judetul | Text | 255 | nu |  | TEXT | VARCHAR(255)  utf8mb3_general_ci; zerouri initiale pastrate |

Indexuri/chei (export): —

## 3. Cele 26 relatii business din TZ_SCHEMA si tratamentul tinta (SLICE-VC00-02 acceptare)

Access declara cascade update + delete pe aproape toate; tinta **nu copiaza** CASCADE DELETE peste istoric (PLAN §Politica FK; decizia finala pe relatie = D05). Totalul din export: 29 relatii (3 de metadate Access, excluse).

| # | Parinte.camp | Copil.camp | Atribute exportate | Tratament tinta |
|---:|---|---|---|---|
| 1 | Clienti.IDC | Iesiri_H.IDC | cascade update, cascade delete | FK de context; RESTRICT la nomenclator folosit; antet+linii se anuleaza numai prin serviciul tranzactional (D05) |
| 2 | Clienti.IDC | OI_Casare.IDC_BC | cascade update, cascade delete | FK de context (SubunitId, GestiuneId, IDC); RESTRICT/inactivare; detinatorul este nomenclator, nu se sterge daca e folosit |
| 3 | Clienti.IDC | OI_Predare.IDC_BP | cascade update, cascade delete | FK de context (SubunitId, GestiuneId, IDC); RESTRICT/inactivare; detinatorul este nomenclator, nu se sterge daca e folosit |
| 4 | Clienti.IDC | OI_Produse.IDC_PTC | cascade update, cascade delete | FK de context (SubunitId, GestiuneId, IDC); RESTRICT/inactivare; detinatorul este nomenclator, nu se sterge daca e folosit |
| 5 | Clienti.IDC | OI_Transfer.IDC_BT | cascade update, cascade delete | FK de context (SubunitId, GestiuneId, IDC); RESTRICT/inactivare; detinatorul este nomenclator, nu se sterge daca e folosit |
| 6 | ComisieNIR.IdComisie | NIR.IdComisie | cascade update, cascade delete | FK de context; RESTRICT la nomenclator folosit; antet+linii se anuleaza numai prin serviciul tranzactional (D05) |
| 7 | Furnizori.IDF | NIR.IDF | cascade update, cascade delete | FK de context; RESTRICT la nomenclator folosit; antet+linii se anuleaza numai prin serviciul tranzactional (D05) |
| 8 | Iesiri_H.IdBonConsum | IesiriRetur.IdBonConsum | cascade update, cascade delete | FK de context; RESTRICT la nomenclator folosit; antet+linii se anuleaza numai prin serviciul tranzactional (D05) |
| 9 | Iesiri_H.IdBonConsum | LegBCLA.IdBonConsum | — | FK composit de context spre Iesiri_H; atribut DAO 8192 neclasificat (vezi §4); cardinalitate verificata in date; RESTRICT |
| 10 | Iesiri_H.IdBonConsum | Iesiri.IdBonConsum | cascade update, cascade delete | FK de context; RESTRICT la nomenclator folosit; antet+linii se anuleaza numai prin serviciul tranzactional (D05) |
| 11 | LABC.IdLista | LegBCLA.IdLista | one-to-one, cascade update, cascade delete | FK de context; antet lista -> detalii mese; stergere numai prin serviciul tranzactional (D05) |
| 12 | LABC.IdLista | Cina.IdLista | cascade update, cascade delete | FK de context; antet lista -> detalii mese; stergere numai prin serviciul tranzactional (D05) |
| 13 | LABC.IdLista | Dejun.IdLista | cascade update, cascade delete | FK de context; antet lista -> detalii mese; stergere numai prin serviciul tranzactional (D05) |
| 14 | LABC.IdLista | ListaAlimente.IDLISTA | cascade update, cascade delete | FK de context; antet lista -> detalii mese; stergere numai prin serviciul tranzactional (D05) |
| 15 | LABC.IdLista | MicDejun.IdLista | cascade update, cascade delete | FK de context; antet lista -> detalii mese; stergere numai prin serviciul tranzactional (D05) |
| 16 | Masini.IdMasina | Produse.IdMasina | cascade update, cascade delete | FK de context; nomenclator folosit = RESTRICT/inactivare (nu CASCADE DELETE) |
| 17 | NIR.IdIntrare | Intrari.IdIntrare | cascade update, cascade delete | FK de context; RESTRICT la nomenclator folosit; antet+linii se anuleaza numai prin serviciul tranzactional (D05) |
| 18 | ProduseP.IdProdus | Iesiri.IdProdus | cascade update, cascade delete | FK de context; nomenclator folosit = RESTRICT/inactivare (nu CASCADE DELETE) |
| 19 | ProduseP.IdProdus | IesiriRetur.IdProdus | cascade update, cascade delete | FK de context; nomenclator folosit = RESTRICT/inactivare (nu CASCADE DELETE) |
| 20 | ProduseP.IdProdus | Intrari.IdProdus | cascade update, cascade delete | FK de context; nomenclator folosit = RESTRICT/inactivare (nu CASCADE DELETE) |
| 21 | ProduseP.IdProdus | OI_Casare.IdProdus | cascade update, cascade delete | FK de context (SubunitId, GestiuneId, IdProdus); RESTRICT/inactivare; linie OI pe cheie de linie (ID), nu pe IDBP/IDBT/IDBC |
| 22 | ProduseP.IdProdus | OI_Predare.IdProdus | cascade update, cascade delete | FK de context (SubunitId, GestiuneId, IdProdus); RESTRICT/inactivare; linie OI pe cheie de linie (ID), nu pe IDBP/IDBT/IDBC |
| 23 | ProduseP.IdProdus | OI_Produse.IdProdus | cascade update, cascade delete | FK de context (SubunitId, GestiuneId, IdProdus); RESTRICT/inactivare; linie OI pe cheie de linie (ID), nu pe IDBP/IDBT/IDBC |
| 24 | ProduseP.IdProdus | OI_Transfer.IdProdus | cascade update, cascade delete | FK de context (SubunitId, GestiuneId, IdProdus); RESTRICT/inactivare; linie OI pe cheie de linie (ID), nu pe IDBP/IDBT/IDBC |
| 25 | Produse.ID | ProduseP.ID | cascade update, cascade delete | FK de context; nomenclator folosit = RESTRICT/inactivare (nu CASCADE DELETE) |
| 26 | TVA.IdTVA | Intrari.IDTVA | cascade update, cascade delete | FK de context; RESTRICT la nomenclator folosit; antet+linii se anuleaza numai prin serviciul tranzactional (D05) |

## 4. Atribute DAO brute ale relatiilor din bazele reale

Bitii: 1 unica, 2 fara integritate, 256 cascade update, 4096 cascade delete, 16777216 LEFT JOIN (referinta DAO RelationAttributeEnum). Valoarea 8192 nu are semnificatie documentata in lista publica consultata: **nu se interpreteaza**; maparea acestei legaturi se confirma pe o copie (D05).

| Baza | Relatii | Distributie atribut |
|---|---:|---|
| tz_Alimente | 29 | 4352: 24, 4353: 1, 8192: 1 |
| tz_Mijloace_Fixe | 30 | 4352: 24, 4353: 2, 8192: 1 |
| tz_Obiecte_Inventar | 29 | 4352: 24, 4353: 1, 8192: 1 |

Relatii business cu atribut diferit de 4352 (cascade update+delete):

- tz_Alimente: Iesiri_H -> LegBCLA attr=8192 (IdBonConsum->IdBonConsum)
- tz_Alimente: LABC -> LegBCLA attr=4353 (IdLista->IdLista)
- tz_Mijloace_Fixe: Iesiri_H -> LegBCLA attr=8192 (IdBonConsum->IdBonConsum)
- tz_Mijloace_Fixe: LABC -> LegBCLA attr=4353 (IdLista->IdLista)
- tz_Mijloace_Fixe: ProduseP -> ProduseM attr=4353 (IdProdus->IdProdus)
- tz_Obiecte_Inventar: Iesiri_H -> LegBCLA attr=8192 (IdBonConsum->IdBonConsum)
- tz_Obiecte_Inventar: LABC -> LegBCLA attr=4353 (IdLista->IdLista)

## 5. Legaturi logice fara relatie declarata (de profilat inainte de FK tinta)

| Legatura | Observatie |
|---|---|
| IntrariStorno.IdIntrare / IdProdus | referinta in cod; fara relatie declarata |
| ProduseM.IdProdus | referinta la ProduseP; fara relatie declarata |
| OI_Transfer.IDC_OLD | vechiul detinator; fara relatie declarata |
| NIR.IdMasina, IdStorno, IDFV; Iesiri_H.IdMasina, IdRetur; OI_Predare.IdBonConsum, IdMasinaP; OI_Produse.IDBP/IDBT/IDBC/IDBO/IDM | nu se deduc parinti dupa nume |
| OI_Predare.IDBP, OI_Transfer.IDBT, OI_Casare.IDBC | **grup, nu cheie unica** (PK este ID). FK simplu spre ele este interzis; normalizare in antete cu `GroupSourceKey` si pastrarea ID-ului de linie |
| LegBCLA.IdBonConsum | fara unicitate declarata; cardinalitatea reala se pastreaza |
| Retetar.IdFel | PK; nu se inventeaza ingrediente multiple |

## 6. Abateri de schema intre bazele reale si TZ_SCHEMA (N05)

Abaterile sunt versiuni anterioare/ulterioare, nu trei scheme tinta. Convertorul de transfer citeste punctual campurile prezente; camp **absent** (nu exista in sursa) este diferit de **NULL** (exista, fara valoare).

### tz_Alimente

fara abateri

### tz_Mijloace_Fixe

- ProduseM.ValoareLunara: in plus fata de TZ_SCHEMA (tip Double)

### tz_Obiecte_Inventar

- tabel **lipsa** in baza reala: Alimente
- ListaAlimente.Comentarii: lipsa in baza reala
- Produse.DNF: lipsa in baza reala
- tabel **lipsa** in baza reala: ProduseM
- ProduseP.COD: lipsa in baza reala
- tabel **lipsa** in baza reala: UNIT

### cale.accdb

Tabele: cale (7 randuri), Ver (1 randuri). Mapare in VC03-04 (cale.PASS si orice credential **nu se importa**). EF/EFS/EFT sunt legate catre cale in frontend dar absente aici: excluse (e-Factura).

## 7. Sinteza claselor de coloane (tabele business)

- CHEIE: 81
- TEXT: 76
- VALOARE-INT: 43
- INT: 17
- BOOL: 4
- DATA: 19
- NUMAR: 1
- CONFIG: 1
- COTA: 1

Camp `DE-CLARIFICAT` = Double/alt tip fara regula: se decide la VC03-02, nu se ghiceste.
