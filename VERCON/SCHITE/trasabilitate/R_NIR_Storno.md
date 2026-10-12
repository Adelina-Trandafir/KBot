# Trasabilitate schita raport NIR_Storno

Sursa: [Reports/NIR_Storno.txt](../../VERCON_ACCESS/Reports/NIR_Storno.txt) · imagine: [R_NIR_Storno.png](../R_NIR_Storno.png)

- Titlu (Caption): Nota de receptie si constatare diferente
- Sursa de date (RecordSource): `SELECT Masini.Masina, Furnizori.Denumire, Furnizori.Adresa, Furnizori.CF, NIR.NrFact, NIR.DataFc, NIR.Numar, NIR.Data, ProduseP.IdProdus, Produse.DenumireP, Produse.UM, ProduseP.PUTVA, -1*[cant] AS CantR, -1*Round([produsep]![putva]*[intraristorno]![cant],[tempvars]![zec]) AS ValoareCT, ComisieNIR.Receptie1, ComisieNIR.Receptie2, ComisieNIR.Receptie3, ComisieNIR.Receptie4 FROM (((Masini INNER JOIN …`
- Latime: 11091 twips · sectiuni: 9 · controale: 82

## ReportHeader (inaltime 0 twips, linia 175)

_fara controale_

## PageHeaderSection (inaltime 925 twips, linia 183)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label7 | Label | NOTA DE RECEPTIE SI CONSTATARE DE DIFERENTE | 0,450 11085x375 | 190 |  |
| Text199 | TextBox | =DLookUp("Firma","CaleTemp") | 1307,0 6621x375 | 207 |  |
| Label200 | Label | Unitatea: | 0,4 1275x375 | 225 |  |

## GroupHeader0 (inaltime 2928 twips, linia 243)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label15 | Label | Nr. comandă | 7905,79 1860x525 | 252 |  |
| Box16 | Rectangle |  | 1380,64 8385x851 | 273 |  |
| Line17 | Line |  | 1380,64 0x851 | 289 |  |
| Line18 | Line |  | 2627,64 0x851 | 303 |  |
| Line19 | Line |  | 4215,64 0x851 | 317 |  |
| Line20 | Line |  | 5462,64 0x851 | 331 |  |
| Line21 | Line |  | 7900,64 0x851 | 345 |  |
| Line22 | Line |  | 5462,348 2438x0 | 359 |  |
| Line23 | Line |  | 6600,345 0x559 | 372 |  |
| Label24 | Label | Număr | 5460,345 1140x255 | 386 |  |
| Label25 | Label | Data | 6600,345 1305x255 | 406 |  |
| Line27 | Line |  | 1365,600 8400x0 | 426 |  |
| Label28 | Label | Subsemnatii membrii ai comisiei de receptie, am procedat la receptionarea valorilor materiale furnizate de :  | 0,1020 4755x735 | 439 |  |
| DenumirePartener | TextBox | Denumire | 4755,1020 2286x735 | 455 |  |
| Adresa | TextBox | Adresa | 7026,1020 1701x735 | 471 |  |
| Label31 | Label | cu auto nr.: | 8727,1260 1020x285 | 490 |  |
| Label33 | Label | Documente insotitoare: | 0,1755 1980x255 | 505 |  |
| Label36 | Label |  Constatandu-se urmatoarele:  Marfa corespunde d.p.d.v. cantitativ | 3681,1755 6075x255 | 520 |  |
| Label146 | Label | Cod furnizor | 1380,64 1247x543 | 534 |  |
| Label147 | Label | Nr. Factură/ Aviz Expediere | 2618,60 1590x540 | 555 |  |
| Label148 | Label | Data Expedierii | 4215,79 1245x525 | 576 |  |
| Label149 | Label | Nota de recepție | 5460,64 2445x285 | 597 |  |
| Box151 | Rectangle |  | 4755,1020 6330x735 | 618 |  |
| Box152 | Rectangle |  | 7020,1020 1716x735 | 633 |  |
| Box153 | Rectangle |  | 9750,1020 1341x735 | 644 |  |
| IDF | TextBox | CF | 1380,600 1251x300 | 659 |  |
| NrFact | TextBox | NrFact | 2625,600 1596x300 | 684 |  |
| Text171 | TextBox | Data | 4215,600 1251x315 | 709 |  |
| Text172 | TextBox | Numar | 5460,600 1146x315 | 734 |  |
| Text174 | TextBox | Data | 6600,600 1311x315 | 759 |  |
| Text175 | TextBox | NrFact | 1980,1755 0x255 | 784 |  |
| Label96 | Label | Nr. crt. | 0,2325 505x600 | 805 |  |
| Label97 | Label | DENUMIRE ARTICOL | 1726,2325 4095x600 | 821 |  |
| Label99 | Label | UM | 5823,2325 520x600 | 842 |  |
| Label100 | Label | Cant. cnf. doc. | 6343,2325 885x600 | 863 |  |
| Label136 | Label | COD ARTICOL | 505,2325 1221x600 | 883 |  |
| Label137 | Label | RECEPŢIONAT | 7228,2325 3855x285 | 904 |  |
| Label139 | Label | Cantitate | 7228,2610 1125x315 | 924 |  |
| Label140 | Label | Preţ unitar | 8353,2610 1365x315 | 944 |  |
| Label141 | Label | Valoare | 9718,2610 1365x315 | 964 |  |

## GroupHeader1 (inaltime 315 twips, linia 986)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text201 | TextBox | Masina | 1728,0 4095x315 | 1000 |  |
| Label204 | Label | Categoria (contul) | 0,0 1728x312 | 1021 |  |

## Detail (inaltime 240 twips, linia 1038)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text49 | TextBox | =1 | 0,0 505x0 | 1046 |  |
| IdProdus | TextBox | IdProdus | 505,0 1221x240 | 1059 |  |
| DenumireP | TextBox | DenumireP | 1726,0 4095x240 | 1077 |  |
| UM | TextBox | UM | 5813,0 520x240 | 1095 |  |
| Cant | TextBox | CantR | 6333,0 895x240 | 1113 |  |
| Cant2 | TextBox | CantR | 7228,0 1125x240 | 1132 |  |
| Pret | TextBox | PUTVA | 8353,0 1365x240 | 1153 |  |
| Valoare | TextBox | ValoareCT | 9718,0 1365x240 | 1174 |  |

## GroupFooter2 (inaltime 96 twips, linia 1197)

_fara controale_

## GroupFooter1 (inaltime 2867 twips, linia 1211)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label65 | Label | CERTIFICARE COMISIA DE RECEPȚIE | 0,1815 7680x285 | 1221 |  |
| Label71 | Label | NUME ȘI PRENUME | 0,2102 2040x255 | 1236 |  |
| Label72 | Label | SEMNĂTURA | 2040,2102 1800x255 | 1251 |  |
| Label73 | Label | NUME ȘI PRENUME | 3840,2102 1920x255 | 1267 |  |
| Label74 | Label | SEMNĂTURA | 5760,2102 1920x255 | 1283 |  |
| Label75 | Label | Gestionar | 7680,1817 3405x285 | 1299 |  |
| Box155 | Rectangle |  | 0,2357 2040x255 | 1320 |  |
| Box156 | Rectangle |  | 0,2612 2040x255 | 1329 |  |
| Box157 | Rectangle |  | 2040,2357 1800x255 | 1338 |  |
| Box158 | Rectangle |  | 2040,2612 1815x255 | 1348 |  |
| Box159 | Rectangle |  | 3840,2357 1920x255 | 1358 |  |
| Box160 | Rectangle |  | 3840,2612 1920x255 | 1368 |  |
| Box161 | Rectangle |  | 5760,2357 1920x255 | 1378 |  |
| Box162 | Rectangle |  | 5760,2612 1920x255 | 1388 |  |
| Box163 | Rectangle |  | 7680,2357 1920x510 | 1398 |  |
| Box165 | Rectangle |  | 9605,2357 1485x510 | 1408 |  |
| Label166 | Label | DATA PRIMIRII | 7680,2102 1920x255 | 1422 |  |
| Label167 | Label | SEMNĂTURA | 9600,2102 1485x255 | 1437 |  |
| Receptie1 | TextBox | Receptie1 | 0,2357 2046x255 | 1456 |  |
| Receptie2 | TextBox | Receptie2 | 0,2607 2046x255 | 1472 |  |
| Line188 | Line |  | 120,690 10920x0 | 1489 |  |
| Line189 | Line |  | 120,1695 10920x0 | 1502 |  |
| Line190 | Line |  | 113,693 10965x1007 | 1515 |  |
| Label192 | Label | TOTAL : | 8352,0 1365x285 | 1530 |  |
| TOT | TextBox | =Sum([ValoareCT]) | 9717,0 1365x285 | 1547 |  |
| Text196 | TextBox | Receptie3 | 3840,2355 1926x255 | 1570 |  |
| Text197 | TextBox | Receptie4 | 3840,2605 1926x255 | 1588 |  |
| Text202 | TextBox | =DLookUp("Reprezentant","Cale","ID=" & DLookUp("ID","CaleTemp")) | 8844,1814 2241x270 | 1606 |  |

## PageFooterSection (inaltime 510 twips, linia 1631)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label107 | TextBox | =DLookUp("expl","ver_rap") | 0,0 2736x477 | 1638 |  |

## ReportFooter (inaltime 0 twips, linia 1661)

_fara controale_

