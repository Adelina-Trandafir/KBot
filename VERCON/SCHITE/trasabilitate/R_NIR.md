# Trasabilitate schita raport NIR

Sursa: [Reports/NIR.txt](../../VERCON_ACCESS/Reports/NIR.txt) · imagine: [R_NIR.png](../R_NIR.png)

- Titlu (Caption): Nota de receptie si constatare diferente
- Sursa de date (RecordSource): `SELECT Furnizori.Denumire, Furnizori.Adresa, Furnizori.CF, Furnizori.J, Masini.Masina, NIR.NrFact, NIR.DataFc, NIR.Numar, NIR.Data, ProduseP.IdProdus, Produse.DenumireP, Produse.UM, ProduseP.PUTVA, Intrari.ID AS Ordine, Intrari.Cant, Intrari.Pret, Intrari.TVA, Intrari.Valoare, Intrari.ValoareCUTVA, ComisieNIR.Receptie1, ComisieNIR.Receptie2, ComisieNIR.Receptie3, ComisieNIR.Receptie4, ProduseP.COD …`
- Latime: 11091 twips · sectiuni: 9 · controale: 86

## ReportHeader (inaltime 0 twips, linia 176)

_fara controale_

## PageHeaderSection (inaltime 1303 twips, linia 184)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label7 | Label | NOTĂ DE RECEPȚIE ȘI CONSTATARE DE DIFERENȚE | 0,795 11085x375 | 191 |  |
| Text199 | TextBox | =DLookUp("Firma","CaleTemp") | 2,0 7926x330 | 208 |  |
| Text205 | TextBox | =DLookUp("detalii","CaleTemp") | 2,330 7926x330 | 227 |  |

## GroupHeader0 (inaltime 2928 twips, linia 251)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label15 | Label | Nr. comandă | 7905,79 1860x525 | 260 |  |
| Box16 | Rectangle |  | 1380,64 8385x851 | 280 |  |
| Line17 | Line |  | 1380,64 0x851 | 296 |  |
| Line18 | Line |  | 2627,64 0x851 | 310 |  |
| Line19 | Line |  | 4215,64 0x851 | 324 |  |
| Line20 | Line |  | 5462,64 0x851 | 338 |  |
| Line21 | Line |  | 7900,64 0x851 | 352 |  |
| Line22 | Line |  | 5462,348 2438x0 | 366 |  |
| Line23 | Line |  | 6600,345 0x559 | 379 |  |
| Label24 | Label | Numar | 5460,345 1140x255 | 393 |  |
| Label25 | Label | Data | 6600,345 1305x255 | 413 |  |
| Line27 | Line |  | 1365,600 8400x0 | 433 |  |
| Label28 | Label | Subsemnații, membrii ai comisiei de recepție, am procedat la recepționarea valorilor materiale furnizate de | 0,1020 4755x735 | 446 |  |
| DenumirePartener | TextBox | Denumire | 4755,1020 2286x735 | 463 |  |
| Adresa | TextBox | Adresa | 7026,1020 1701x735 | 479 |  |
| Label31 | Label | cu auto nr.: | 8727,1260 1020x285 | 498 |  |
| Label33 | Label | Documente insoțitoare | 0,1755 1980x255 | 513 |  |
| Label36 | Label |  Constatându-se următoarele:  Marfa corespunde d.p.d.v. cantitativ | 3681,1755 7410x255 | 529 |  |
| Label146 | Label | Cod furnizor | 1380,64 1247x543 | 547 |  |
| Label147 | Label | Nr. Factură/ Aviz Expediere | 2618,60 1590x540 | 567 |  |
| Label148 | Label | Data Expedierii | 4215,79 1245x525 | 587 |  |
| Label149 | Label | Nota de recepție | 5460,64 2445x285 | 607 |  |
| Box151 | Rectangle |  | 4755,1020 6330x735 | 627 |  |
| Box152 | Rectangle |  | 7020,1020 1716x735 | 642 |  |
| Box153 | Rectangle |  | 9750,1020 1341x735 | 653 |  |
| IDF | TextBox | CF | 1380,600 1251x300 | 668 |  |
| NrFact | TextBox | NrFact | 2625,600 1596x300 | 691 |  |
| Text171 | TextBox | Data | 4215,600 1251x315 | 714 |  |
| Text172 | TextBox | Numar | 5460,600 1146x315 | 737 |  |
| Text174 | TextBox | Data | 6600,600 1311x315 | 760 |  |
| Text175 | TextBox | NrFact | 1980,1755 0x255 | 783 |  |
| Label96 | Label | Nr. crt. | 0,2325 505x600 | 803 |  |
| Label97 | Label | DENUMIRE ARTICOL | 1546,2325 3345x600 | 819 |  |
| Label99 | Label | UM | 4891,2325 520x600 | 840 |  |
| Label100 | Label | Cant. cnf. Docum. | 5411,2325 885x600 | 861 |  |
| Label136 | Label | COD ARTICOL | 505,2325 1041x600 | 882 |  |
| Label137 | Label | RECEPŢIONAT | 6296,2325 3855x285 | 903 |  |
| Label139 | Label | Cantitate | 6296,2610 1125x315 | 923 |  |
| Label140 | Label | Preţ unitar | 7421,2610 1365x315 | 943 |  |
| Label141 | Label | Valoare | 8786,2610 1365x315 | 963 |  |
| Label191 | Label | din care T.V.A. | 10151,2325 930x600 | 983 |  |

## GroupHeader1 (inaltime 315 twips, linia 1006)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text201 | TextBox | Masina | 2436,0 3345x315 | 1020 |  |
| Label204 | Label | Categoria (contul) | 888,0 1728x312 | 1041 |  |

## Detail (inaltime 240 twips, linia 1060)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text49 | TextBox | =1 | 0,0 505x0 | 1068 |  |
| IdProdus | TextBox | IdProdus | 505,0 1041x240 | 1081 |  |
| DenumireP | TextBox | DenumireP | 1546,0 3345x240 | 1101 |  |
| UM | TextBox | UM | 4891,0 520x240 | 1120 |  |
| Cant | TextBox | Cant | 5411,0 895x240 | 1138 |  |
| Cant2 | TextBox | Cant | 6306,0 1125x240 | 1156 |  |
| Pret | TextBox | PUTVA | 7431,0 1365x240 | 1176 |  |
| Valoare | TextBox | ValoareCuTVA | 8796,0 1365x240 | 1196 |  |
| TVA | TextBox | TVA | 10161,0 930x240 | 1216 |  |

## GroupFooter2 (inaltime 0 twips, linia 1238)

_fara controale_

## GroupFooter1 (inaltime 2867 twips, linia 1252)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label65 | Label | CERTIFICARE COMISIA DE RECEPȚIE | 0,1815 7680x285 | 1262 |  |
| Label71 | Label | NUME ȘI PRENUME | 0,2102 2040x255 | 1277 |  |
| Label72 | Label | SEMNĂTURA | 2040,2102 1800x255 | 1292 |  |
| Label73 | Label | NUME ȘI PRENUME | 3840,2102 1920x255 | 1308 |  |
| Label74 | Label | SEMNĂTURA | 5760,2102 1920x255 | 1324 |  |
| Label75 | Label | Gestionar | 7680,1817 3375x285 | 1340 |  |
| Box155 | Rectangle |  | 0,2357 2040x255 | 1361 |  |
| Box156 | Rectangle |  | 0,2612 2040x255 | 1370 |  |
| Box157 | Rectangle |  | 2040,2357 1800x255 | 1379 |  |
| Box158 | Rectangle |  | 2040,2612 1815x255 | 1389 |  |
| Box159 | Rectangle |  | 3840,2357 1920x255 | 1399 |  |
| Box160 | Rectangle |  | 3840,2612 1920x255 | 1409 |  |
| Box161 | Rectangle |  | 5760,2357 1920x255 | 1419 |  |
| Box162 | Rectangle |  | 5760,2612 1920x255 | 1429 |  |
| Box163 | Rectangle |  | 7680,2357 1920x510 | 1439 |  |
| Box165 | Rectangle |  | 9605,2357 1455x510 | 1449 |  |
| Label166 | Label | DATA PRIMIRII | 7680,2102 1920x255 | 1463 |  |
| Label167 | Label | SEMNĂTURA | 9600,2102 1455x255 | 1478 |  |
| Receptie1 | TextBox | Receptie1 | 0,2357 2046x255 | 1497 |  |
| Receptie2 | TextBox | Receptie2 | 0,2607 2046x255 | 1513 |  |
| Line188 | Line |  | 120,690 10920x0 | 1530 |  |
| Line189 | Line |  | 120,1695 10920x0 | 1543 |  |
| Line190 | Line |  | 113,693 10965x1007 | 1556 |  |
| tVAL | TextBox | =Sum([valoarecutva]) | 8796,0 1365x285 | 1571 |  |
| tTVA | TextBox | =Sum([tva]) | 10161,0 930x285 | 1593 |  |
| Label192 | Label | TOTAL | 7431,0 1365x570 | 1615 |  |
| TOT | TextBox | =Sum([valoarecutva]) | 8796,285 2295x285 | 1635 |  |
| Text196 | TextBox | Receptie3 | 3840,2355 1926x255 | 1659 |  |
| Text197 | TextBox | Receptie4 | 3840,2605 1926x255 | 1677 |  |
| Text202 | TextBox | =DLookUp("Reprezentant","Cale","ID=" & DLookUp("ID","CaleTemp")) | 8844,1814 2211x270 | 1695 |  |

## PageFooterSection (inaltime 510 twips, linia 1720)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text207 | TextBox | =DLookUp("expl","ver_rap") | 0,0 2736x477 | 1727 |  |

## ReportFooter (inaltime 0 twips, linia 1750)

_fara controale_

