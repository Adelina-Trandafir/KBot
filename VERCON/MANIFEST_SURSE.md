# VERCON — manifestul surselor si mediul de referinta (SLICE-VC00-01)

Starea: **DOCUMENTAT + VERIFICAT LOCAL** (hash-uri si schema citite doar-citire, 11.10.2026). Nimic importat, nimic scris in surse.

## 1. Sursele de date

| Fisier | Rol (propunere, nu aprobare: D01) | Dimensiune | SHA-256 | Tabele (fara sistem) | Configuratie (Setari/NR) |
|---|---|---:|---|---:|---|
| cale.accdb | catalog unitati/cai (cale, Ver) | 454656 | `CC337A0D583C36383BA91969C9AE472F9A211857918C98C859744C3949B13212` | 2 | — |
| tz_Alimente.accdb | gestiune ALIMENTE (nume fisier fara an; exercitiul NU se deduce din nume) | 4231168 | `E3FDEDF8D8FB21B80B76CD66FA49AFEAED05D36B4DD99DEB3CC339BEA388E4D7` | 41 | MF=0, OBINV=0, FIFO=-1, ZEC=4, Ver=4 |
| tz_Mijloace_Fixe.accdb | gestiune MF (o baza, intreg istoricul) | 2805760 | `B824C563375FC8C23B85EB17384661F13354720FA3D2691295A5DE5CAD71E618` | 41 | MF=-1, OBINV=0, FIFO=-1, ZEC=2, Ver=124 |
| tz_Obiecte_Inventar.accdb | gestiune OI (o baza, intreg istoricul) | 4296704 | `DC66D2D1E6FB291FFA2E692E409C457335DBD486DA3C04C6C74183DBBB7275C3` | 37 | MF=0, OBINV=-1, FIFO=-1, ZEC=2, tabel Ver absent |

Hash-urile au fost recalculate dupa prima runda de lucru si sunt **identice** cu cele de la inceput. La sfarsitul rundei a doua hash-urile `cale`, `tz_Mijloace_Fixe` si `tz_Obiecte_Inventar` sunt iar identice; `tz_Alimente.accdb` **nu a putut fi recalculat**: este deschis de doua procese MSACCESS (PID 25404 si 39860, pornite la 17:34 si 17:36, cu fisierul `tz_Alimente.laccdb` langa baza), deci probabil de utilizator pentru exportul `tmpOptiuniSFP`; motorul DAO folosit de scripturile mele nu porneste Access. Procesele nu au fost oprite (regula proiectului: le opreste utilizatorul). Verificarea hash-ului `tz_Alimente` se refera deci doar la prima runda. Acces: `DAO.DBEngine.120.OpenDatabase(path, False, True)` (ReadOnly), fara parola (bazele s-au deschis fara ea), recordset-uri `snapshot`/doar agregate.

### Volume (randuri)

Proveniență: **M** = volumele consemnate în manifestul VC00-01; **D** = numărătorile consemnate în dicționarul VC00-02, din dumpurile DAO doar-schema ale celor trei baze reale. `M, D` păstrează ambele observații când valorile coincid. Marcajul `—` rămâne distinct de `0`; MF/OI denumesc Mijloace_Fixe/Obiecte_Inventar. Observațiile istorice M de mai jos rămân la checkpointul lor.

| Tabel | Alimente | MF | OI | Proveniență |
|---|---:|---:|---:|---|
| NIR | 192 | 2 | 4 | M, D |
| Intrari | 1671 | 156 | 1137 | M, D |
| IntrariStorno | 8 | 2 | 2 | M, D |
| Iesiri_H | 127 | 7 | 96 | M, D |
| Iesiri | 3196 | 154 | 1529 | M, D |
| IesiriRetur | 0 | 0 | 0 | M, D |
| Produse | 274 | 41 | 951 | M, D |
| ProduseP | 2304 | 156 | 1144 | M, D |
| ProduseM | 0 | 12 | — | M, D |
| OI_Predare | 0 | 154 | 1529 | M, D |
| OI_Transfer | 0 | 0 | 0 | M, D |
| OI_Casare | 0 | 0 | 0 | M, D |
| OI_Produse | 0 | 154 | 1529 | M, D |
| LABC | 127 | 0 | 0 | M, D |
| ListaAlimente | 127 | 0 | 0 | M, D |
| LegBCLA | 127 | 0 | 0 | M, D |
| MicDejun | 127 | 0 | 0 | M, D |
| Dejun | 401 | 0 | 0 | M, D |
| Cina | 127 | 0 | 0 | M, D |
| Furnizori | 28 | 2 | 5 | M, D |
| Clienti | 1 | 7 | 105 | M, D |
| Masini | 4 | 2 | 2 | M, D |
| TVA | 6 | 4 | 4 | M, D |
| cpv | 9454 | 9454 | 9454 | M, D |
| Alimente | 0 | 0 | — | D |
| ComisieInventar | 0 | 0 | 0 | D |
| ComisieNIR | 1 | 1 | 1 | D |
| FeluriMancare | 0 | 0 | 0 | D |
| NR | 1 | 1 | 1 | D |
| Retetar | 0 | 0 | 0 | D |
| Setari | 6 | 6 | 5 | D |
| UNIT | 1 | 1 | — | D |

Observatii (dovada): `OI_Predare`, `OI_Transfer`, `OI_Casare`, `OI_Produse`, `IesiriRetur` au 0 randuri in toate cele trei baze (nu exista documente reale pentru predare/transfer/casare/retur -> scenariile sunt sintetice). Predarile OI apar in `Iesiri`/`Iesiri_H` (OI: 96 bonuri, 1.529 linii; MF: 7 bonuri, 154 linii). `cpv` are 9454 randuri in backend (nomenclator CPV comun in baza: D08).

## 2. Obiectele frontend si ce lipseste

Frontend: `Vercon.accdb` (exportat in `VERCON_ACCESS`, 377 obiecte) — **fisierul nu este in director**; exista doar exportul text. Clasificarea completa: [INVENTAR_OBIECTE.md](INVENTAR_OBIECTE.md).

| Lipsa | Impact | Tratament |
|---|---|---|
| Frontendul `Vercon.accdb` si backendul anual legat `14\tz_2024.accdb` | 35 de tabele legate nu pot fi citite cu datele frontendului; legaturile sunt documentate doar in export | Se foloseste TZ_SCHEMA + cele trei baze reale; nu se construieste mediu Access (decizie) |
| `ufTreeView` (MSForms) | Componenta ActiveX de arbore nu se reconstituie din export | Inlocuita de selector + panou web (F8) |
| ~~Datele `tmpOptiuniSFP`~~ | **PRIMITE** de la utilizator la 11.10.2026: `REFERINTE/tmpOptiuniSFP.xml` + `.xsd` (47 randuri) | Folosite in RAPOARTE.md |
| Continutul tabelului `Rbb` (vizibilitate/activare butoane ribbon) | Ribbonul (primit: `REFERINTE/ribbon_Vercon.xml`) isi ia vizibilitatea din `Rbb`; datele nu sunt in export | Reguli de tip din `Form_Open` (MAPARE_ACCESS_WEB); decis: ribbonul nu diferentiaza dupa tip |
| Sabloane `.tmpl` (6: fm, Balanta, ListaInventar, referat_per, plan, nota_comanda) si rapoartele `Centralizator_MF_BON_BS/BP/BT` | Sabloanele **nu se portează** (decizie 11.10.2026: export `.xlsx` simplu pe fiecare raport); cele 3 rapoarte MF lipsa se trateaza la VC08-03 | |
| Date `Setari`, `NR` pentru Materiale (nu exista baza Materiale) | Gestiunea Materiale nu are probe reale | Sintetic; fisierele reale le atribuie operatorul in migrator, la import |
| RES/IMG/TMPLS, `RegExpWrapper*.dll`, `WinScpNet.tlb` | Sabloane/imagini de raport lipsesc; DLL/FTP sunt infrastructura exclusa | Sabloanele vizuale vin din schite; fisierele de raport se refac in VC08-03 |
| Cod VBA al `ufTreeView` si al raportelor (doar proprietati exportate) | Nu toata logica de raport este in export | Se noteaza explicit in RAPOARTE.md |

## 3. Mediu de referinta

- Doar-citire: motor DAO (`DAO.DBEngine.120`) disponibil pe aceasta statie; scripturile din `VERCON/tools/` sunt reproductibile (`dump_schema.ps1`, `oracle_probe*.ps1`).
- Fara Access: nicio rulare VBA/macrocomanda; `Autoexec_`, `Aexec`, `Repara2024`, `IA`, `clsDatabase.Version` nu se apeleaza.
- Fara reparare/compatibilitate Access (decizie). Sursele raman referinta nemodificata.
- Comanda de regenerare: `node VERCON/tools/inventar.js; powershell ... dump_schema.ps1; node VERCON/tools/dictionar.js; node VERCON/tools/schite.js; powershell ... schite_render.ps1; node VERCON/tools/mapare.js`.
