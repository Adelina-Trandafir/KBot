# VERCON — dictionar de date si graf FK (SLICE-VC00-02)

Generat de `VERCON/tools/dictionar.js` din `TZ_SCHEMA` (export tz_Alimente, referinta comuna decisa de utilizator) si din dump-uri **doar-schema** ale celor trei baze reale (`tools/dump_schema.ps1`, DAO read-only, numarul de randuri, fara continut). Tipurile tinta sunt **propuneri de dictionar** pentru VC03-02, nu DDL aplicat. Numărătorile sunt centralizate în [volumele surselor](MANIFEST_SURSE.md#volume-randuri), cu proveniența VC00-01/02 păstrată separat.

Starea declarată la checkpointul VC00-02: DOCUMENTAT. Nimic implementat, nimic validat pe server.

## 1. Reguli de tip (se aplica uniform pe toate gestiunile)

| Clasa sursa | Tinta MariaDB | Nota |
|---|---|---|
| CHEIE (AutoNumber/Long de legatura) | `INT UNSIGNED` generat | Cheia sursa se citeste **semnata** (ID-uri random pot fi negative) si se pastreaza in `VC_IdMap`. |
| VALOARE-INT (cantitati, preturi, valori) | `DECIMAL(20,4)` | Valori directe, maximum 4 zecimale. ZEC controleaza doar afisarea si calculele. Denumirea clasei ramane pentru compatibilitate. |
| COTA (TVA.TVA) | `DECIMAL(20,4)` | Cota fractionara directa: 0,21 pentru 21%, fara scalare. |
| NUMAR / INT | `INT` | nu se scaleaza. |
| TEXT | `VARCHAR(n)` utf8mb3_general_ci | fara eliminarea diacriticelor; trunchierea se raporteaza. |
| DATA | `DATETIME` | `Data`, `DataFc`, `DataBC`, `DataSalvare`, `DataIntrare` nu se substituie reciproc. |
| BOOL | `TINYINT(1)` NULL/0/1 | -1 -> 1. |

Fiecare tabel business tinta primeste in plus: `SubunitId`, `GestiuneId` (si `ExercitiuId` unde relatia o cere), cheie sursa in `VC_IdMap`, audit. Fara tabele per an / per fisier / per tip.

### Conversii comune ale câmpurilor <a id="conversii-comune"></a>

Compactare documentară SLICE-D003-11: un rând grupează numai câmpuri consecutive cu toate atributele identice; ordinea numelor și a rândurilor păstrează ordinea sursă. Profilul are forma `Tip Access · Marime · Clasa — câmpuri`. Atributele omise codifică Obligatoriu=nu și Implicit gol; în lipsa atributului Conversie se aplică exact conversia clasei de mai jos. Excepțiile sunt scrise integral. `{Marime}` este valoarea coloanei Marime din același rând, fără altă transformare. Celulele goale din Implicit rămân distincte de valoarea `0`. Regenerarea dicționarului poate suprascrie această prezentare.

| Clasa | Conversia comună |
|---|---|
| CHEIE | cheie tehnica generata MariaDB; valoarea sursa (semnata) in VC_IdMap |
| TEXT | VARCHAR({Marime})  utf8mb3_general_ci; zerouri initiale pastrate |
| VALOARE-INT | DECIMAL(20,4), valoare directa; stocat exact, refuz la overflow (nu se rotunjeste la migrare) |
| DATA | DATETIME, data locala fara conversie UTC |
| INT | INT (contor/numar/durata); NU se scaleaza |
| BOOL | TINYINT(1) NULL/0/1 (Access -1 -> 1); NULL separat |

## 2. Tabele business din TZ_SCHEMA si tratamentul coloanelor

### Alimente

- Long · 4 · CHEIE — ID
- Text · 255 · TEXT — Denumire<br>UM
- Double · 8 · VALOARE-INT — PU<br>Cant<br>Valoare
- Long · 4 · INT — P · Implicit=0

Indexuri/chei (export): `ID` (ID) PRIMARY UNIQUE

### Cina

- AutoNumber · 4 · CHEIE — Id
- Long · 4 · CHEIE — IdLista · Implicit=0
- Long · 4 · CHEIE — IdFel
- Text · 50 · TEXT — Tip
- Text · 255 · TEXT — Explicatie

Indexuri/chei (export): `Id` (Id) PRIMARY UNIQUE; `LABC_IdLista__Cina_IdLista` (IdLista) FOREIGN

### Clienti

- Long · 4 · CHEIE — IDC · Obligatoriu=da
- Text · 50 · TEXT — Denumire<br>Adresa
- Text · 10 · TEXT — CF
- Text · 20 · TEXT — J
- Text · 13 · TEXT — CNP
- Text · 50 · TEXT — Cont

Indexuri/chei (export): `IDC` (IDC) PRIMARY UNIQUE

### ComisieInventar

- Long · 4 · CHEIE — IdComisieInv · Obligatoriu=da · Implicit=0
- Text · 50 · TEXT — Receptie1<br>Receptie2<br>Receptie3<br>Receptie4

Indexuri/chei (export): `IdComisie` (IdComisieInv) PRIMARY UNIQUE

### ComisieNIR

- Long · 4 · CHEIE — IdComisie · Obligatoriu=da · Implicit=0
- Text · 50 · TEXT — Receptie1<br>Receptie2<br>Receptie3<br>Receptie4

Indexuri/chei (export): `IdComisie` (IdComisie) PRIMARY UNIQUE

### cpv

- Text · 255 · TEXT — CODE<br>RO

Indexuri/chei (export): `CODE` (CODE) PRIMARY UNIQUE

### Dejun

- AutoNumber · 4 · CHEIE — Id
- Long · 4 · CHEIE — IdLista · Implicit=0
- Long · 4 · CHEIE — IdFel
- Text · 50 · TEXT — Tip
- Text · 255 · TEXT — Explicatie

Indexuri/chei (export): `Id` (Id) PRIMARY UNIQUE; `LABC_IdLista__Dejun_IdLista` (IdLista) FOREIGN

### FeluriMancare

- Long · 4 · CHEIE — IDFel · Obligatoriu=da
- Text · 255 · TEXT — DenFel<br>Explicatie

Indexuri/chei (export): `IDFel` (IDFel) PRIMARY UNIQUE

### Furnizori

- Long · 4 · CHEIE — IDF · Obligatoriu=da
- Text · 255 · TEXT — Denumire<br>Adresa<br>CF<br>J<br>Cont<br>TIP
- Yes/No · 1 · BOOL — ONLINE
- Text · 255 · TEXT — Orasul

Indexuri/chei (export): `IDF` (IDF) PRIMARY UNIQUE

### Iesiri

- AutoNumber · 4 · CHEIE — ID
- Long · 4 · CHEIE — IdBonConsum · Implicit=0
- Long · 4 · CHEIE — IdProdus
- Double · 8 · VALOARE-INT — Cant<br>PUTVA · Implicit=0
- Double · 8 · VALOARE-INT — ValoareCUTVA
- Date/Time · 8 · DATA — DataSalvare

Indexuri/chei (export): `ID` (ID) PRIMARY UNIQUE; `IdBonConsum` (IdBonConsum); `IdProdus` (IdProdus); `Iesiri_H_IdBonConsum__Iesiri_IdBonConsum` (IdBonConsum) FOREIGN; `ProdusePIesiri` (IdProdus) FOREIGN

### IesiriRetur

- AutoNumber · 4 · CHEIE — ID
- Long · 4 · CHEIE — IdBonConsum<br>IdProdus
- Double · 8 · VALOARE-INT — Cant · Implicit=0
- Long · 4 · VALOARE-INT — PUTVA · Conversie=DECIMAL(20,4), valoare directa (sursa Long: fara fractiune de inventat)
- Date/Time · 8 · DATA — DataSalvare · Implicit=Now()

Indexuri/chei (export): `ID` (ID) PRIMARY UNIQUE; `idbc` (IdBonConsum); `IdProdus` (IdProdus); `Iesiri_HIesiriRetur` (IdBonConsum) FOREIGN; `ProdusePIesiriRetur` (IdProdus) FOREIGN

### Iesiri_H

- Long · 4 · CHEIE — IdBonConsum · Obligatoriu=da · Implicit=0
- Long · 4 · CHEIE — IdMasina<br>IdRetur<br>IDC · Implicit=0
- Text · 255 · TEXT — NumarBC
- Date/Time · 8 · DATA — DataBC
- Double · 8 · VALOARE-INT — ValoareBC · Implicit=0
- Text · 255 · TEXT — Expl
- Yes/No · 1 · BOOL — Retur · Implicit=No
- Date/Time · 8 · DATA — DataSalvare · Implicit=Now()

Indexuri/chei (export): `Clienti_IDC__Iesiri_H_IDC` (IDC) FOREIGN; `IdBonConsum` (IdBonConsum) PRIMARY UNIQUE; `IDC` (IDC); `IdMasina` (IdMasina); `IdRetur` (IdRetur)

### Intrari

- AutoNumber · 4 · CHEIE — ID
- Long · 4 · CHEIE — IdIntrare · Implicit=0
- Long · 4 · CHEIE — IdProdus
- Long · 4 · CHEIE — IDTVA · Implicit=0
- Double · 8 · VALOARE-INT — Cant<br>Pret<br>TVA<br>Valoare<br>PUTVA · Implicit=0
- Double · 8 · VALOARE-INT — ValoareCUTVA
- Date/Time · 8 · DATA — DataSalvare

Indexuri/chei (export): `ID` (ID) PRIMARY UNIQUE; `IdProdus` (IdProdus); `NIR_IdIntrare__Intrari_IdIntrare` (IdIntrare) FOREIGN; `ProdusePIntrari` (IdProdus) FOREIGN; `PUTVA` (PUTVA); `TVAIntrari` (IDTVA) FOREIGN

### IntrariStorno

- AutoNumber · 4 · CHEIE — ID
- Long · 4 · CHEIE — IdIntrare<br>IdProdus
- Double · 8 · VALOARE-INT — Cant · Implicit=0
- Long · 4 · VALOARE-INT — PUTVA · Conversie=DECIMAL(20,4), valoare directa (sursa Long: fara fractiune de inventat)
- Date/Time · 8 · DATA — DataSalvare · Implicit=Now()

Indexuri/chei (export): `ID` (ID) PRIMARY UNIQUE; `idintrare` (IdIntrare); `IdProdus` (IdProdus)

### LABC

- AutoNumber · 4 · CHEIE — ID
- Long · 4 · CHEIE — IdLista · Obligatoriu=da · Implicit=0
- Long · 4 · INT — NumarLA · Implicit=0
- Date/Time · 8 · DATA — DATALA
- Double · 8 · VALOARE-INT — ValoareBon<br>DEP · Implicit=0
- Double · 8 · VALOARE-INT — ValoareLista

Indexuri/chei (export): `IdLista` (IdLista) PRIMARY UNIQUE

### LegBCLA

- Long · 4 · CHEIE — IdBonConsum · Implicit=0
- Long · 4 · CHEIE — IdLista · Obligatoriu=da · Implicit=0

Indexuri/chei (export): `IdLista` (IdLista) PRIMARY UNIQUE; `Iesiri_HLegBCLA` (IdBonConsum) FOREIGN; `LABCLegBCLA` (IdLista) UNIQUE FOREIGN

**Ținta web — decizie utilizator 12.10.2026:** în Access o listă are exact un BC (1=1); în sistemul web o listă are mai multe BC-uri (funcționalitate nouă). IdLista țintă este neunic; RowId tehnic, FK contextuale, un BC aparține unei singure liste. DDL/generator aliniate: [SCHEMA_VC](SCHEMA_VC.md), [FLUXURI_UI](FLUXURI_UI.md). Datele istorice nu se repară tacit.

### ListaAlimente

- AutoNumber · 4 · CHEIE — ID
- Long · 4 · CHEIE — IDLISTA · Obligatoriu=da
- Long · 4 · INT — MicDejun_P · Implicit=0
- Double · 8 · VALOARE-INT — VMD<br>MicDejun_V · Implicit=0
- Long · 4 · INT — MicDejun_D<br>Dejun_P · Implicit=0
- Double · 8 · VALOARE-INT — VD<br>Dejun_V · Implicit=0
- Long · 4 · INT — Dejun_D<br>Cina_P · Implicit=0
- Double · 8 · VALOARE-INT — VC<br>Cina_V · Implicit=0
- Long · 4 · INT — Cina_D · Implicit=0
- Text · 255 · TEXT — Comentarii

Indexuri/chei (export): `ID` (ID) PRIMARY UNIQUE; `LABC_IdLista__ListaAlimente_IDLISTA` (IDLISTA) FOREIGN

### Masini

- Long · 4 · CHEIE — IdMasina · Obligatoriu=da · Implicit=0
- Text · 50 · TEXT — Masina
- Text · 255 · TEXT — TIP

Indexuri/chei (export): `IdMasina` (IdMasina) PRIMARY UNIQUE

### MicDejun

- AutoNumber · 4 · CHEIE — Id
- Long · 4 · CHEIE — IdLista · Implicit=0
- Long · 4 · CHEIE — IdFel
- Text · 50 · TEXT — Tip
- Text · 255 · TEXT — Explicatie

Indexuri/chei (export): `ID` (Id) PRIMARY UNIQUE; `LABC_IdLista__MicDejun_IdLista` (IdLista) FOREIGN

### NIR

- Long · 4 · CHEIE — IdIntrare · Obligatoriu=da · Implicit=0
- Long · 4 · CHEIE — IdComisie<br>IdMasina · Implicit=0
- Long · 4 · CHEIE — IDF<br>IDFV
- Long · 4 · CHEIE — IdStorno · Implicit=0
- Text · 255 · TEXT — NrFact
- Date/Time · 8 · DATA — Data
- Double · 8 · NUMAR — Numar · Implicit=0 · Conversie=INT (numar document; negativ la storno/seria istorica); NU se scaleaza; fractia (document inserat intre doua numere) in NumarSub, in sutimi
- Double · 8 · VALOARE-INT — Valoare
- Date/Time · 8 · DATA — DataFc
- Yes/No · 1 · BOOL — Storno · Implicit=No
- Text · 255 · TEXT — FurnizorV

Indexuri/chei (export): `ComisieNIRNIR` (IdComisie) FOREIGN; `Furnizori_IDF__NIR_IDF` (IDF) FOREIGN; `IdComisie` (IdComisie); `IDF` (IDF); `IdIntrare` (IdIntrare) PRIMARY UNIQUE; `IdMasina` (IdMasina); `IdStorno` (IdStorno)

### NR

- AutoNumber · 4 · CHEIE — ID
- Long · 4 · INT — NR · Implicit=0
- Text · 50 · TEXT — NUM
- Long · 4 · INT — NIR<br>NRFCT<br>NRBC · Implicit=0
- Double · 8 · VALOARE-INT — SI_LA · Implicit=0
- Long · 4 · INT — ZEC

Indexuri/chei (export): `ID` (ID) PRIMARY UNIQUE

### OI_Casare

- AutoNumber · 4 · CHEIE — ID
- Long · 4 · CHEIE — IDBC · Obligatoriu=da · Implicit=0
- Long · 4 · CHEIE — IdMasina<br>IDC_BC · Implicit=0
- Long · 4 · CHEIE — IdProdus
- Text · 255 · TEXT — NRBC
- Date/Time · 8 · DATA — DATABC
- Double · 8 · VALOARE-INT — Cant<br>PUTVA<br>ValoareCUTVA · Implicit=0
- Text · 255 · TEXT — Expl
- Date/Time · 8 · DATA — DataSalvare

Indexuri/chei (export): `Clienti_IDC__OI_Casare_IDC_BC` (IDC_BC) FOREIGN; `ID` (ID) PRIMARY UNIQUE; `IDC_BC` (IDC_BC); `IdMasina` (IdMasina); `IdProdus` (IdProdus); `NRBC` (NRBC); `OI_CasareIDBC` (IDBC); `ProdusePOI_Casare` (IdProdus) FOREIGN; `PUTVA` (PUTVA)

### OI_Predare

- AutoNumber · 4 · CHEIE — ID
- Long · 4 · CHEIE — IDBP · Obligatoriu=da · Implicit=0
- Long · 4 · CHEIE — IdBonConsum<br>IdMasina<br>IdMasinaP<br>IDC_BP · Implicit=0
- Long · 4 · CHEIE — IdProdus
- Text · 255 · TEXT — NRBP
- Date/Time · 8 · DATA — DATABP
- Double · 8 · VALOARE-INT — Cant<br>PUTVA<br>ValoareCUTVA · Implicit=0
- Text · 255 · TEXT — Expl
- Date/Time · 8 · DATA — DataSalvare

Indexuri/chei (export): `Clienti_IDC__OI_Predare_IDC_BP` (IDC_BP) FOREIGN; `ID` (ID) PRIMARY UNIQUE; `IdBonConsum` (IdBonConsum); `IDC_BP` (IDC_BP); `IdMasina` (IdMasina); `IdMasinaP` (IdMasinaP); `IdProdus` (IdProdus); `NRBP` (NRBP); `OI_PredareIDBP` (IDBP); `ProdusePOI_Predare` (IdProdus) FOREIGN; `PUTVA` (PUTVA)

### OI_Produse

**Clarificare confirmată la 18.10.2026:** `IDBC` identifică bonul curent pentru predare/transfer/casare; `IDBP` și `IDBT` marchează numai predarea, respectiv transferul. `IDBO` este proveniența BP/BT numai la transfer/casare. ID-urile bonurilor se remapează dintr-o secvență comună pe gestiune, păstrând familia sursă. [Contractul complet, exemple și compatibilitatea VBA](REGISTRU_OI_MF.md). Aceste coloane sunt referințe/grupuri, nu chei AUTO_INCREMENT independente.

- AutoNumber · 4 · CHEIE — IDOI
- Long · 4 · CHEIE — IDBP · Implicit=0 · Conversie=ID de grup al predarii, numai la BP; remapat identic cu VC_OI_Predare.IDBP
- Long · 4 · CHEIE — IDBT · Implicit=0 · Conversie=ID de grup al transferului, numai la BT; remapat identic cu VC_OI_Transfer.IDBT
- Long · 4 · CHEIE — IDBC · Implicit=0 · Conversie=bonul curent BP/BT/BS, ales dupa marcajele IDBP/IDBT; aceeasi mapare ca documentul, nu grup separat rezervat casarii
- Long · 4 · CHEIE — IDBO · Implicit=0 · Conversie=provenienta BP/BT numai la transfer/casare; aceeasi mapare ca documentul sursa, inclusiv pe ambele laturi ale transferului
- Long · 4 · CHEIE — IDM<br>IDC_PTC · Implicit=0
- Long · 4 · CHEIE — IdProdus
- Double · 8 · VALOARE-INT — Cant<br>PUTVA<br>ValoareCUTVA · Implicit=0
- Date/Time · 8 · DATA — DataSalvare

Indexuri/chei (export): `Clienti_IDC__OI_Produse_IDC_PTC` (IDC_PTC) FOREIGN; `IDBC` (IDBC); `IDBO` (IDBO); `IDBP` (IDBP); `IDBT` (IDBT); `IDC_PTC` (IDC_PTC); `IDM` (IDM); `IDOI` (IDOI) PRIMARY UNIQUE; `IdProdus` (IdProdus); `ProdusePOI_Produse` (IdProdus) FOREIGN; `PUTVA` (PUTVA)

### OI_Transfer

- AutoNumber · 4 · CHEIE — ID
- Long · 4 · CHEIE — IDBT · Obligatoriu=da · Implicit=0
- Long · 4 · CHEIE — IdMasina<br>IDC_BT<br>IDC_OLD · Implicit=0
- Long · 4 · CHEIE — IdProdus
- Text · 255 · TEXT — NRBT
- Date/Time · 8 · DATA — DATABT
- Double · 8 · VALOARE-INT — Cant<br>PUTVA<br>ValoareCUTVA · Implicit=0
- Text · 255 · TEXT — Expl
- Date/Time · 8 · DATA — DataSalvare

Indexuri/chei (export): `Clienti_IDC__OI_Transfer_IDC_BT` (IDC_BT) FOREIGN; `ID` (ID) PRIMARY UNIQUE; `IDBT` (IDBT); `IDC_BT` (IDC_BT); `IDC_OLD` (IDC_OLD); `IdMasina` (IdMasina); `IdProdus` (IdProdus); `NRBT` (NRBT); `ProdusePOI_Transfer` (IdProdus) FOREIGN; `PUTVA` (PUTVA)

### Produse

- AutoNumber · 4 · CHEIE — ID · Implicit=GenUniqueID()
- Long · 4 · CHEIE — IdMasina · Implicit=0
- Text · 255 · TEXT — DenumireP
- Text · 50 · TEXT — UM
- Text · 255 · TEXT — CPV<br>CC
- Date/Time · 8 · DATA — DataSalvare · Implicit=Now()
- Long · 4 · INT — DNF · Implicit=0

Indexuri/chei (export): `ID` (ID) PRIMARY UNIQUE; `IdMasina` (IdMasina); `Masini_IdMasina__Produse_IdMasina` (IdMasina) FOREIGN

### ProduseM

- AutoNumber · 4 · CHEIE — IDPMF
- Long · 4 · CHEIE — IdProdus
- Date/Time · 8 · DATA — DataIntrare
- Long · 4 · INT — DNF<br>DF
- Double · 8 · VALOARE-INT — ValoareLuna · Implicit=0

Indexuri/chei (export): —

### ProduseP

- AutoNumber · 4 · CHEIE — IdProdus
- Long · 4 · CHEIE — ID · Implicit=0
- Double · 8 · VALOARE-INT — PUTVA · Implicit=0
- Date/Time · 8 · DATA — DataSalvare · Implicit=Now()
- Text · 255 · TEXT — COD

Indexuri/chei (export): `ID` (ID); `PrimaryKey` (IdProdus) PRIMARY UNIQUE; `ProduseProduseP` (ID) FOREIGN

### Retetar

- Long · 4 · CHEIE — IdFel · Obligatoriu=da
- Text · 255 · TEXT — Continut
- Double · 8 · VALOARE-INT — Cantitate
- Text · 255 · TEXT — AlteDetalii<br>UM<br>CPV

Indexuri/chei (export): `IdFel` (IdFel) PRIMARY UNIQUE

### Setari

- AutoNumber · 4 · CHEIE — ID
- Text · 255 · TEXT — S
- Double · 8 · CONFIG — V · Implicit=0 · Conversie=nu se copiaza; cheie tipizata in VC_Settings (INT); valorile reale: -1/0 sau ZEC

Indexuri/chei (export): `ID` (ID); `PrimaryKey` (ID) PRIMARY UNIQUE

### TVA

- Long · 4 · CHEIE — IdTVA · Obligatoriu=da · Implicit=0
- Double · 8 · COTA — TVA · Implicit=0 · Conversie=DECIMAL(20,4), cota fractionara directa
- Text · 50 · TEXT — Explicatie

Indexuri/chei (export): `IdTVA` (IdTVA) PRIMARY UNIQUE

### UNIT

- Text · 255 · TEXT — NumeUnitate
- Text · 50 · TEXT — CUI
- Text · 255 · TEXT — Adresa<br>Director<br>Contabil<br>Culoare
- Yes/No · 1 · BOOL — Expirat
- Text · 255 · TEXT — Secretar<br>SursaCont<br>AdresaMail<br>Orasul<br>TelefonContact<br>SectorSursa<br>AlteDetalii
- Long · 4 · INT — AnDate · Implicit=0
- Text · 255 · TEXT — DC<br>Alte<br>Gestionar
- Text · 255 · TEXT — CodProgram · Implicit=0000000000
- Text · 255 · TEXT — Judetul

Indexuri/chei (export): —

## 3. Cele 26 relatii business din TZ_SCHEMA si tratamentul tinta (SLICE-VC00-02 acceptare)

Access declara cascade update + delete pe aproape toate; tinta **nu copiaza** CASCADE DELETE peste istoric (PLAN §Politica FK; decizia finala pe relatie = D05). Totalul din export: 29 relatii (3 de metadate Access, excluse).

| Parinte.camp | Copii (# inițial: camp) | Atribute exportate | Tratament tinta |
|---|---|---|---|
| Clienti.IDC | 1: Iesiri_H.IDC | cascade update, cascade delete | FK de context; RESTRICT la nomenclator folosit; antet+linii se anuleaza numai prin serviciul tranzactional (D05) |
| Clienti.IDC | 2: OI_Casare.IDC_BC<br>3: OI_Predare.IDC_BP<br>4: OI_Produse.IDC_PTC<br>5: OI_Transfer.IDC_BT | cascade update, cascade delete | FK de context (SubunitId, GestiuneId, IDC); RESTRICT/inactivare; detinatorul este nomenclator, nu se sterge daca e folosit |
| ComisieNIR.IdComisie | 6: NIR.IdComisie | cascade update, cascade delete | FK de context; RESTRICT la nomenclator folosit; antet+linii se anuleaza numai prin serviciul tranzactional (D05) |
| Furnizori.IDF | 7: NIR.IDF | cascade update, cascade delete | FK de context; RESTRICT la nomenclator folosit; antet+linii se anuleaza numai prin serviciul tranzactional (D05) |
| Iesiri_H.IdBonConsum | 8: IesiriRetur.IdBonConsum<br>10: Iesiri.IdBonConsum | cascade update, cascade delete | FK de context; RESTRICT la nomenclator folosit; antet+linii se anuleaza numai prin serviciul tranzactional (D05) |
| Iesiri_H.IdBonConsum | 9: LegBCLA.IdBonConsum | — | FK composit de context spre Iesiri_H; atribut DAO 8192 neclasificat (vezi §4); cardinalitate verificata in date; RESTRICT |
| LABC.IdLista | 11: LegBCLA.IdLista | one-to-one, cascade update, cascade delete | FK de context; one-to-one doar in Access, tinta web este 1:N (SCHEMA_VC); stergere numai prin serviciul tranzactional (D05) |
| LABC.IdLista | 12: Cina.IdLista<br>13: Dejun.IdLista<br>14: ListaAlimente.IDLISTA<br>15: MicDejun.IdLista | cascade update, cascade delete | FK de context; antet lista -> detalii mese; stergere numai prin serviciul tranzactional (D05) |
| Masini.IdMasina | 16: Produse.IdMasina | cascade update, cascade delete | FK de context; nomenclator folosit = RESTRICT/inactivare (nu CASCADE DELETE) |
| NIR.IdIntrare | 17: Intrari.IdIntrare | cascade update, cascade delete | FK de context; RESTRICT la nomenclator folosit; antet+linii se anuleaza numai prin serviciul tranzactional (D05) |
| ProduseP.IdProdus | 18: Iesiri.IdProdus<br>19: IesiriRetur.IdProdus<br>20: Intrari.IdProdus | cascade update, cascade delete | FK de context; nomenclator folosit = RESTRICT/inactivare (nu CASCADE DELETE) |
| ProduseP.IdProdus | 21: OI_Casare.IdProdus<br>22: OI_Predare.IdProdus<br>23: OI_Produse.IdProdus<br>24: OI_Transfer.IdProdus | cascade update, cascade delete | FK de context (SubunitId, GestiuneId, IdProdus); RESTRICT/inactivare; linie OI pe cheie de linie (ID), nu pe IDBP/IDBT/IDBC |
| Produse.ID | 25: ProduseP.ID | cascade update, cascade delete | FK de context; nomenclator folosit = RESTRICT/inactivare (nu CASCADE DELETE) |
| TVA.IdTVA | 26: Intrari.IDTVA | cascade update, cascade delete | FK de context; RESTRICT la nomenclator folosit; antet+linii se anuleaza numai prin serviciul tranzactional (D05) |

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
| NIR.IdMasina, IDFV; Iesiri_H.IdMasina, IdRetur; OI_Predare.IdBonConsum, IdMasinaP; OI_Produse.IDBP/IDBT/IDBC/IDBO/IDM | nu se deduc parinti dupa nume |
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
