# Trasabilitate schite NIR: normal, storno și SFP

Convenții și limite: [citirea fișelor consolidate](../INDEX.md#fise-consolidate). SLICE-D003-09, 27.10.2026.

Coloana Evenimente era goală la fiecare control.

## Variante și surse

### Trasabilitate schita raport NIR

Sursa: [Reports/NIR.txt](../../VERCON_ACCESS/Reports/NIR.txt) · imagine: [R_NIR.png](../R_NIR.png)

- Titlu (Caption): Nota de receptie si constatare diferente
- Sursa de date (RecordSource): `SELECT Furnizori.Denumire, Furnizori.Adresa, Furnizori.CF, Furnizori.J, Masini.Masina, NIR.NrFact, NIR.DataFc, NIR.Numar, NIR.Data, ProduseP.IdProdus, Produse.DenumireP, Produse.UM, ProduseP.PUTVA, Intrari.ID AS Ordine, Intrari.Cant, Intrari.Pret, Intrari.TVA, Intrari.Valoare, Intrari.ValoareCUTVA, ComisieNIR.Receptie1, ComisieNIR.Receptie2, ComisieNIR.Receptie3, ComisieNIR.Receptie4, ProduseP.COD …`
- Latime: 11091 twips · sectiuni: 9 · controale: 86

### Trasabilitate schita raport NIR_Storno

Sursa: [Reports/NIR_Storno.txt](../../VERCON_ACCESS/Reports/NIR_Storno.txt) · imagine: [R_NIR_Storno.png](../R_NIR_Storno.png)

- Titlu (Caption): Nota de receptie si constatare diferente
- Sursa de date (RecordSource): `SELECT Masini.Masina, Furnizori.Denumire, Furnizori.Adresa, Furnizori.CF, NIR.NrFact, NIR.DataFc, NIR.Numar, NIR.Data, ProduseP.IdProdus, Produse.DenumireP, Produse.UM, ProduseP.PUTVA, -1*[cant] AS CantR, -1*Round([produsep]![putva]*[intraristorno]![cant],[tempvars]![zec]) AS ValoareCT, ComisieNIR.Receptie1, ComisieNIR.Receptie2, ComisieNIR.Receptie3, ComisieNIR.Receptie4 FROM (((Masini INNER JOIN …`
- Latime: 11091 twips · sectiuni: 9 · controale: 82

### Trasabilitate schita raport NIR_SFP

Sursa: [Reports/NIR_SFP.txt](../../VERCON_ACCESS/Reports/NIR_SFP.txt) · imagine: [R_NIR_SFP.png](../R_NIR_SFP.png)

- Titlu (Caption): Nota de receptie si constatare diferente
- Sursa de date (RecordSource): `SELECT Furnizori.Denumire, Furnizori.Adresa, Furnizori.CF, Furnizori.J, Masini.Masina, NIR.NrFact, NIR.DataFc, NIR.Numar, NIR.Data, ProduseP.IdProdus, Produse.DenumireP, Produse.UM, ProduseP.PUTVA, Intrari.ID AS Ordine, Intrari.Cant, Intrari.Pret, Intrari.TVA, Intrari.Valoare, Intrari.ValoareCUTVA, ComisieNIR.Receptie1, ComisieNIR.Receptie2, ComisieNIR.Receptie3, ComisieNIR.Receptie4 FROM Masini I …`
- Latime: 11105 twips · sectiuni: 7 · controale: 78

## Secțiuni

| Secțiune | NIR | NIR_Storno | NIR_SFP |
|---|---|---|---|
| ReportHeader | 1 / 0 / 176 | 1 / 0 / 175 | 1 / 0 / 171 |
| PageHeaderSection | 2 / 1303 / 184 | 2 / 925 / 183 | 2 / 855 / 179 |
| GroupHeader0 | 3 / 2928 / 251 | 3 / 2928 / 243 | 3 / 2892 / 225 |
| GroupHeader1 | 4 / 315 / 1006 | 4 / 315 / 986 | — |
| Detail | 5 / 240 / 1060 | 5 / 240 / 1038 | 4 / 240 / 921 |
| GroupFooter2 | 6 / 0 / 1238 | 6 / 96 / 1197 | — |
| GroupFooter1 | 7 / 2867 / 1252 | 7 / 2867 / 1211 | 5 / 0 / 1185 |
| PageFooterSection | 8 / 510 / 1720 | 8 / 510 / 1631 | 6 / 1725 / 1292 |
| ReportFooter | 9 / 0 / 1750 | 9 / 0 / 1661 | 7 / 0 / 1799 |

## Controale

### ReportHeader <a id="reportheader-inaltime-0-twips-linia-176"></a><a id="reportheader-inaltime-0-twips-linia-175"></a><a id="reportheader-inaltime-0-twips-linia-171"></a>

_fara controale_

### PageHeaderSection <a id="pageheadersection-inaltime-1303-twips-linia-184"></a><a id="pageheadersection-inaltime-925-twips-linia-183"></a><a id="pageheadersection-inaltime-855-twips-linia-179"></a>

| Control | Tip | Text / camp / sursa | Pozitie comună (twips) | NIR | NIR_Storno | NIR_SFP |
|---|---|---|---|---|---|---|
| Label7 | Label | NOTĂ DE RECEPȚIE ȘI CONSTATARE DE DIFERENȚE | 0,795 11085x375 | 1:191 | — | 1:186 / 0,0 11085x435 |
| Text199 | TextBox | =DLookUp("Firma","CaleTemp") | 2,0 7926x330 | 2:208 | 2:207 / 1307,0 6621x375 | — |
| Text205 | TextBox | =DLookUp("detalii","CaleTemp") | 2,330 7926x330 | 3:227 | — | — |
| Label7 | Label | NOTA DE RECEPTIE SI CONSTATARE DE DIFERENTE | 0,450 11085x375 | — | 1:190 | — |
| Label200 | Label | Unitatea: | 0,4 1275x375 | — | 3:225 | — |
| Text199 | TextBox | =DLookUp("Firma","caletemp") | 0,480 11085x375 | — | — | 2:202 |

### GroupHeader0 <a id="groupheader0-inaltime-2928-twips-linia-251"></a><a id="groupheader0-inaltime-2928-twips-linia-243"></a><a id="groupheader0-inaltime-2892-twips-linia-225"></a>

| Control | Tip | Text / camp / sursa | Pozitie comună (twips) | NIR | NIR_Storno | NIR_SFP |
|---|---|---|---|---|---|---|
| Label15 | Label | Nr. comandă | 7905,79 1860x525 | 1:260 | 1:252 | — |
| Box16 | Rectangle |  | 1380,64 8385x851 | 2:280 | 2:273 | 30:903 / 192,96 10698x779 |
| Line17 | Line |  | 1380,64 0x851 | 3:296 | 3:289 | — |
| Line18 | Line |  | 2627,64 0x851 | 4:310 | 4:303 | — |
| Line19 | Line |  | 4215,64 0x851 | 5:324 | 5:317 | — |
| Line20 | Line |  | 5462,64 0x851 | 6:338 | 6:331 | — |
| Line21 | Line |  | 7900,64 0x851 | 7:352 | 7:345 | — |
| Line22 | Line |  | 5462,348 2438x0 | 8:366 | 8:359 | — |
| Line23 | Line |  | 6600,345 0x559 | 9:379 | 9:372 | — |
| Label24 | Label | Numar | 5460,345 1140x255 | 10:393 | — | — |
| Label25 | Label | Data | 6600,345 1305x255 | 11:413 | 11:406 | 3:281 / 5966,348 1305x255 |
| Line27 | Line |  | 1365,600 8400x0 | 12:433 | 12:426 | — |
| Label28 | Label | Subsemnații, membrii ai comisiei de recepție, am procedat la recepționarea valorilor materiale furnizate de | 0,1020 4755x735 | 13:446 | — | — |
| DenumirePartener | TextBox | Denumire | 4755,1020 2286x735 | 14:463 | 14:455 | 5:325 / 5139,1008 2586x603 |
| Adresa | TextBox | Adresa | 7026,1020 1701x735 | 15:479 | 15:471 | 6:347 / 7725,1008 1701x603 |
| Label31 | Label | cu auto nr.: | 8727,1260 1020x285 | 16:498 | 16:490 | — |
| Label33 | Label | Documente insoțitoare | 0,1755 1980x255 | 17:513 | — | — |
| Label36 | Label |  Constatându-se următoarele:  Marfa corespunde d.p.d.v. cantitativ | 3681,1755 7410x255 | 18:529 | — | — |
| Label146 | Label | Cod furnizor | 1380,64 1247x543 | 19:547 | 19:534 | 10:430 / 192,96 1511x507 |
| Label147 | Label | Nr. Factură/ Aviz Expediere | 2618,60 1590x540 | 20:567 | 20:555 | — |
| Label148 | Label | Data Expedierii | 4215,79 1245x525 | 21:587 | 21:576 | 12:476 / 3581,96 1245x507 |
| Label149 | Label | Nota de recepție | 5460,64 2445x285 | 22:607 | 22:597 | 13:499 / 4826,96 2445x249 |
| Box151 | Rectangle |  | 4755,1020 6330x735 | 23:627 | 23:618 | — |
| Box152 | Rectangle |  | 7020,1020 1716x735 | 24:642 | 24:633 | — |
| Box153 | Rectangle |  | 9750,1020 1341x735 | 25:653 | 25:644 | — |
| IDF | TextBox | CF | 1380,600 1251x300 | 26:668 | 26:659 | 14:522 / 192,603 1511x264 |
| NrFact | TextBox | NrFact | 2625,600 1596x300 | 27:691 | 27:684 | 15:546 / 1703,603 1878x264 |
| Text171 | TextBox | Data | 4215,600 1251x315 | 28:714 | 28:709 | — |
| Text172 | TextBox | Numar | 5460,600 1146x315 | 29:737 | 29:734 | 17:594 / 4826,603 1140x264 |
| Text174 | TextBox | Data | 6600,600 1311x315 | 30:760 | 30:759 | 18:618 / 5966,603 1305x264 |
| Text175 | TextBox | NrFact | 1980,1755 0x255 | 31:783 | 31:784 | 19:642 / 2340,1704 2817x255 |
| Label96 | Label | Nr. crt. | 0,2325 505x600 | 32:803 | 32:805 | 20:668 / 24,2328 505x552 |
| Label97 | Label | DENUMIRE ARTICOL | 1546,2325 3345x600 | 33:819 | 33:821 / 1726,2325 4095x600 | 21:692 / 1750,2328 3165x552 |
| Label99 | Label | UM | 4891,2325 520x600 | 34:840 | 34:842 / 5823,2325 520x600 | 22:716 / 4915,2328 520x552 |
| Label100 | Label | Cant. cnf. Docum. | 5411,2325 885x600 | 35:861 | — | 23:740 / 5435,2328 885x552 |
| Label136 | Label | COD ARTICOL | 505,2325 1041x600 | 36:882 | 36:883 / 505,2325 1221x600 | 24:763 / 529,2328 1221x552 |
| Label137 | Label | RECEPŢIONAT | 6296,2325 3855x285 | 37:903 | 37:904 / 7228,2325 3855x285 | 25:787 / 6320,2328 4785x285 |
| Label139 | Label | Cantitate | 6296,2610 1125x315 | 38:923 | 38:924 / 7228,2610 1125x315 | 26:810 / 6320,2613 1125x267 |
| Label140 | Label | Preţ unitar | 7421,2610 1365x315 | 39:943 | 39:944 / 8353,2610 1365x315 | 27:833 / 7445,2613 1365x267 |
| Label141 | Label | Valoare | 8786,2610 1365x315 | 40:963 | 40:964 / 9718,2610 1365x315 | 28:856 / 8810,2613 2295x267 |
| Label191 | Label | din care T.V.A. | 10151,2325 930x600 | 41:983 | — | — |
| Label24 | Label | Număr | 5460,345 1140x255 | — | 10:386 | 2:258 / 4826,348 1140x255 |
| Label28 | Label | Subsemnatii membrii ai comisiei de receptie, am procedat la receptionarea valorilor materiale furnizate de :  | 0,1020 4755x735 | — | 13:439 | — |
| Label33 | Label | Documente insotitoare: | 0,1755 1980x255 | — | 17:505 | — |
| Label36 | Label |  Constatandu-se urmatoarele:  Marfa corespunde d.p.d.v. cantitativ | 3681,1755 6075x255 | — | 18:520 | — |
| Label100 | Label | Cant. cnf. doc. | 6343,2325 885x600 | — | 35:863 | — |
| Label15 | Label | Număr comandă | 7271,96 3621x507 | — | — | 1:234 |
| Label28 | Label |  Subsemnații, membrii ai comisiei de recepție ,am procedat la recepționarea valorilor materiale furnizate de | 192,1008 4947x603 | — | — | 4:304 |
| Label31 | Label | cu auto nr. | 9426,1008 1452x603 | — | — | 7:369 |
| Label33 | Label | Documente însoțitoare: | 192,1704 2148x255 | — | — | 8:390 |
| Label36 | Label | , constatăndu-se următoarele:  Marfa corespunde d.p.d.v. cantitativ | 5157,1704 5727x255 | — | — | 9:410 |
| Label147 | Label | Nr. Factură / Aviz Expediere | 1703,96 1878x507 | — | — | 11:453 |
| Text171 | TextBox | DataFc | 3581,603 1245x264 | — | — | 16:570 |
| Text202 | TextBox | Masina | 7271,603 3627x264 | — | — | 29:879 |

### GroupHeader1 <a id="groupheader1-inaltime-315-twips-linia-1006"></a><a id="groupheader1-inaltime-315-twips-linia-986"></a>

| Control | Tip | Text / camp / sursa | Pozitie comună (twips) | NIR | NIR_Storno | NIR_SFP |
|---|---|---|---|---|---|---|
| Text201 | TextBox | Masina | 2436,0 3345x315 | 1:1020 | 1:1000 / 1728,0 4095x315 | — |
| Label204 | Label | Categoria (contul) | 888,0 1728x312 | 2:1041 | 2:1021 / 0,0 1728x312 | — |

### Detail <a id="detail-inaltime-240-twips-linia-1060"></a><a id="detail-inaltime-240-twips-linia-1038"></a><a id="detail-inaltime-240-twips-linia-921"></a>

| Control | Tip | Text / camp / sursa | Pozitie comună (twips) | NIR | NIR_Storno | NIR_SFP |
|---|---|---|---|---|---|---|
| Text49 | TextBox | =1 | 0,0 505x0 | 1:1068 | 1:1046 | 1:929 / 24,0 505x240 |
| IdProdus | TextBox | IdProdus | 505,0 1041x240 | 2:1081 | 2:1059 / 505,0 1221x240 | — |
| DenumireP | TextBox | DenumireP | 1546,0 3345x240 | 3:1101 | 3:1077 / 1726,0 4095x240 | — |
| UM | TextBox | UM | 4891,0 520x240 | 4:1120 | 4:1095 / 5813,0 520x240 | 4:991 / 4915,0 520x240 |
| Cant | TextBox | Cant | 5411,0 895x240 | 5:1138 | — | — |
| Cant2 | TextBox | Cant | 6306,0 1125x240 | 6:1156 | — | — |
| Pret | TextBox | PUTVA | 7431,0 1365x240 | 7:1176 | 7:1153 / 8353,0 1365x240 | — |
| Valoare | TextBox | ValoareCuTVA | 8796,0 1365x240 | 8:1196 | — | — |
| TVA | TextBox | TVA | 10161,0 930x240 | 9:1216 | — | — |
| Cant | TextBox | CantR | 6333,0 895x240 | — | 5:1113 | — |
| Cant2 | TextBox | CantR | 7228,0 1125x240 | — | 6:1132 | — |
| Valoare | TextBox | ValoareCT | 9718,0 1365x240 | — | 8:1174 | — |
| CodProdu | TextBox | IdProdus | 529,0 1221x240 | — | — | 2:949 |
| Text177 | TextBox | DenumireP | 1750,0 3165x240 | — | — | 3:970 |
| Text179 | TextBox | Cant | 5435,0 885x240 | — | — | 5:1012 |
| Text180 | TextBox | Cant | 6320,0 1125x240 | — | — | 6:1033 |
| Text181 | TextBox | PUTVA | 7445,0 1365x240 | — | — | 7:1057 |
| Text182 | TextBox | ValoareCUTVA | 8810,0 2295x240 | — | — | 8:1081 |
| Line204 | Line |  | 24,0 0x240 | — | — | 9:1105 |
| Line205 | Line |  | 529,0 0x240 | — | — | 10:1118 |
| Line206 | Line |  | 1750,0 0x240 | — | — | 11:1131 |
| Line207 | Line |  | 5435,0 0x240 | — | — | 12:1144 |
| Line208 | Line |  | 6320,0 0x240 | — | — | 13:1157 |
| Line209 | Line |  | 11105,0 0x240 | — | — | 14:1170 |

### GroupFooter2 <a id="groupfooter2-inaltime-0-twips-linia-1238"></a><a id="groupfooter2-inaltime-96-twips-linia-1197"></a>

_fara controale_

### GroupFooter1 <a id="groupfooter1-inaltime-2867-twips-linia-1252"></a><a id="groupfooter1-inaltime-2867-twips-linia-1211"></a><a id="groupfooter1-inaltime-0-twips-linia-1185"></a>

| Control | Tip | Text / camp / sursa | Pozitie comună (twips) | NIR | NIR_Storno | NIR_SFP |
|---|---|---|---|---|---|---|
| Label65 | Label | CERTIFICARE COMISIA DE RECEPȚIE | 0,1815 7680x285 | 1:1262 | 1:1221 | — |
| Label71 | Label | NUME ȘI PRENUME | 0,2102 2040x255 | 2:1277 | 2:1236 | — |
| Label72 | Label | SEMNĂTURA | 2040,2102 1800x255 | 3:1292 | 3:1251 | — |
| Label73 | Label | NUME ȘI PRENUME | 3840,2102 1920x255 | 4:1308 | 4:1267 | — |
| Label74 | Label | SEMNĂTURA | 5760,2102 1920x255 | 5:1324 | 5:1283 | — |
| Label75 | Label | Gestionar | 7680,1817 3375x285 | 6:1340 | 6:1299 / 7680,1817 3405x285 | — |
| Box155 | Rectangle |  | 0,2357 2040x255 | 7:1361 | 7:1320 | — |
| Box156 | Rectangle |  | 0,2612 2040x255 | 8:1370 | 8:1329 | — |
| Box157 | Rectangle |  | 2040,2357 1800x255 | 9:1379 | 9:1338 | — |
| Box158 | Rectangle |  | 2040,2612 1815x255 | 10:1389 | 10:1348 | — |
| Box159 | Rectangle |  | 3840,2357 1920x255 | 11:1399 | 11:1358 | — |
| Box160 | Rectangle |  | 3840,2612 1920x255 | 12:1409 | 12:1368 | — |
| Box161 | Rectangle |  | 5760,2357 1920x255 | 13:1419 | 13:1378 | — |
| Box162 | Rectangle |  | 5760,2612 1920x255 | 14:1429 | 14:1388 | — |
| Box163 | Rectangle |  | 7680,2357 1920x510 | 15:1439 | 15:1398 | — |
| Box165 | Rectangle |  | 9605,2357 1455x510 | 16:1449 | 16:1408 / 9605,2357 1485x510 | — |
| Label166 | Label | DATA PRIMIRII | 7680,2102 1920x255 | 17:1463 | 17:1422 | — |
| Label167 | Label | SEMNĂTURA | 9600,2102 1455x255 | 18:1478 | 18:1437 / 9600,2102 1485x255 | — |
| Receptie1 | TextBox | Receptie1 | 0,2357 2046x255 | 19:1497 | 19:1456 | — |
| Receptie2 | TextBox | Receptie2 | 0,2607 2046x255 | 20:1513 | 20:1472 | — |
| Line188 | Line |  | 120,690 10920x0 | 21:1530 | 21:1489 | 1:1194 / 79,360 10920x0 |
| Line189 | Line |  | 120,1695 10920x0 | 22:1543 | 22:1502 | 2:1207 / 79,1365 10920x0 |
| Line190 | Line |  | 113,693 10965x1007 | 23:1556 | 23:1515 | 3:1220 / 72,363 10965x1007 |
| tVAL | TextBox | =Sum([valoarecutva]) | 8796,0 1365x285 | 24:1571 | — | — |
| tTVA | TextBox | =Sum([tva]) | 10161,0 930x285 | 25:1593 | — | — |
| Label192 | Label | TOTAL | 7431,0 1365x570 | 26:1615 | — | — |
| TOT | TextBox | =Sum([valoarecutva]) | 8796,285 2295x285 | 27:1635 | — | — |
| Text196 | TextBox | Receptie3 | 3840,2355 1926x255 | 28:1659 | 26:1570 | — |
| Text197 | TextBox | Receptie4 | 3840,2605 1926x255 | 29:1677 | 27:1588 | — |
| Text202 | TextBox | =DLookUp("Reprezentant","Cale","ID=" & DLookUp("ID","CaleTemp")) | 8844,1814 2211x270 | 30:1695 | 28:1606 / 8844,1814 2241x270 | — |
| Label192 | Label | TOTAL : | 8352,0 1365x285 | — | 24:1530 | — |
| TOT | TextBox | =Sum([ValoareCT]) | 9717,0 1365x285 | — | 25:1547 | — |
| Label66 | Label | TOTALURI | 7445,0 1365x285 | — | — | 4:1235 |
| tVAL | TextBox | =Sum([ValoareCUTVA]) | 8810,0 2295x285 | — | — | 5:1256 |
| Line210 | Line |  | 24,0 7392x0 | — | — | 6:1280 |

### PageFooterSection <a id="pagefootersection-inaltime-510-twips-linia-1720"></a><a id="pagefootersection-inaltime-510-twips-linia-1631"></a><a id="pagefootersection-inaltime-1725-twips-linia-1292"></a>

| Control | Tip | Text / camp / sursa | Pozitie comună (twips) | NIR | NIR_Storno | NIR_SFP |
|---|---|---|---|---|---|---|
| Text207 | TextBox | =DLookUp("expl","ver_rap") | 0,0 2736x477 | 1:1727 | — | — |
| Label107 | TextBox | =DLookUp("expl","ver_rap") | 0,0 2736x477 | — | 1:1638 | 26:1771 / 48,1197 2736x477 |
| Label65 | Label | CERTIFICARE COMISIA DE RECEPȚIE | 48,25 7200x285 | — | — | 1:1299 |
| Label71 | Label | NUME ȘI PRENUME | 48,312 1800x255 | — | — | 2:1320 |
| Label72 | Label | SEMNATURA | 1848,312 1800x255 | — | — | 3:1343 |
| Label73 | Label | NUME ȘI PRENUME | 3648,312 1800x255 | — | — | 4:1366 |
| Label74 | Label | SEMNATURA | 5448,312 1800x255 | — | — | 5:1389 |
| Box155 | Rectangle |  | 48,567 1800x255 | — | — | 6:1412 |
| Box156 | Rectangle |  | 48,822 1800x255 | — | — | 7:1426 |
| Box157 | Rectangle |  | 1848,567 1800x255 | — | — | 8:1440 |
| Box158 | Rectangle |  | 1848,822 1800x255 | — | — | 9:1454 |
| Box159 | Rectangle |  | 3648,567 1800x255 | — | — | 10:1468 |
| Box160 | Rectangle |  | 3648,822 1800x255 | — | — | 11:1482 |
| Box161 | Rectangle |  | 5448,567 1800x255 | — | — | 12:1496 |
| Box162 | Rectangle |  | 5448,822 1800x255 | — | — | 13:1510 |
| Box163 | Rectangle |  | 7248,567 1920x510 | — | — | 14:1524 |
| Box165 | Rectangle |  | 9168,567 1875x510 | — | — | 15:1538 |
| Label166 | Label | DATA PRIMIRII | 7248,312 1920x255 | — | — | 16:1552 |
| Label167 | Label | SEMNATURA | 9168,312 1875x255 | — | — | 17:1575 |
| Receptie1 | TextBox | Receptie1 | 48,567 1800x255 | — | — | 18:1598 |
| Receptie2 | TextBox | Receptie2 | 48,822 1800x255 | — | — | 19:1620 |
| Text196 | TextBox | Receptie3 | 3648,567 1800x255 | — | — | 20:1643 |
| Text197 | TextBox | Receptie4 | 3648,822 1800x255 | — | — | 21:1666 |
| Label201 | TextBox | =DLookUp("Reprezentant","Cale","ID=" & DLookUp("ID","CaleTemp")) | 8298,24 2745x285 | — | — | 22:1689 |
| Label75 | Label | Gestionar | 7248,27 3795x285 | — | — | 23:1712 |
| Line212 | Line |  | 0,1248 11085x0 | — | — | 24:1736 |
| Text46 | TextBox | ="Pagina " & [Page] & " din " & [Pages] | 5976,1248 5040x285 | — | — | 25:1747 |

### ReportFooter <a id="reportfooter-inaltime-0-twips-linia-1750"></a><a id="reportfooter-inaltime-0-twips-linia-1661"></a><a id="reportfooter-inaltime-0-twips-linia-1799"></a>

_fara controale_
