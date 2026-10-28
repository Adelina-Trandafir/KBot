# Trasabilitate schite liste de inventariere MF, OI și magazie

Convenții și limite: [citirea fișelor consolidate](../INDEX.md#fise-consolidate). SLICE-D003-09, 27.10.2026.

Comune tuturor: Titlu (Caption): —; lățime 15405 twips. Coloana Evenimente era goală la fiecare control.

## Variante și surse

### Trasabilitate schita raport MF_LISTA_INVENTAR

Sursa: [Reports/MF_LISTA_INVENTAR.txt](../../VERCON_ACCESS/Reports/MF_LISTA_INVENTAR.txt) · imagine: [R_MF_LISTA_INVENTAR.png](../R_MF_LISTA_INVENTAR.png)

- Sursa de date (RecordSource): `SELECT X.IdMasina, X.IDC, X.Masina, X.Denumire, X.Adresa, X.CodProdus, X.DENP, X.UMP, X.PT, Sum([CI]+[CCIn]-[CCOut]) AS CF, Sum([vi]+[vcin]-[vcout]) AS VF, ProduseP.COD, Produse.CC FROM (SELECT Clienti.Adresa,OI_Union.IDC, Clienti.Denumire, OI_Union.IdMasina, Masini.Masina, OI_Union.CodProdus, OI_Union.DenumireProdus AS DENP, Nz([UM],"Buc") AS UMP, OI_Union.PUTVA AS PT, Sum(OI_Union.C0) AS CI, Sum …`
- sectiuni: 6 · controale: 84

### Trasabilitate schita raport OI_LISTA_INVENTAR

Sursa: [Reports/OI_LISTA_INVENTAR.txt](../../VERCON_ACCESS/Reports/OI_LISTA_INVENTAR.txt) · imagine: [R_OI_LISTA_INVENTAR.png](../R_OI_LISTA_INVENTAR.png)

- Sursa de date (RecordSource): `SELECT X.IdMasina, X.IDC, X.Masina, X.Denumire, X.Adresa, X.CodProdus, X.DENP, X.UMP, X.PT, Sum([CI]+[CCIn]-[CCOut]) AS CF, Sum([vi]+[vcin]-[vcout]) AS VF FROM (SELECT Clienti.Adresa,OI_Union.IDC, Clienti.Denumire, OI_Union.IdMasina, Masini.Masina, OI_Union.CodProdus, OI_Union.DenumireProdus AS DENP, Nz([UM],"Buc") AS UMP, OI_Union.PUTVA AS PT, Sum(OI_Union.C0) AS CI, Sum(OI_Union.V0) AS VI, Sum(O …`
- sectiuni: 5 · controale: 79

### Trasabilitate schita raport OI_LISTA_INVENTAR_TOTAL

Sursa: [Reports/OI_LISTA_INVENTAR_TOTAL.txt](../../VERCON_ACCESS/Reports/OI_LISTA_INVENTAR_TOTAL.txt) · imagine: [R_OI_LISTA_INVENTAR_TOTAL.png](../R_OI_LISTA_INVENTAR_TOTAL.png)

- Sursa de date (RecordSource): `SELECT X.IdMasina, X.Masina, X.CodProdus, X.DENP, X.UMP, X.PT, Sum([CI]+[CCIn]-[CCOut]) AS CF, Sum([vi]+[vcin]-[vcout]) AS VF FROM (SELECT OI_Union.IDC, Clienti.Denumire, OI_Union.IdMasina, Masini.Masina, OI_Union.CodProdus, OI_Union.DenumireProdus AS DENP, Nz([UM],"Buc") AS UMP, OI_Union.PUTVA AS PT, Sum(OI_Union.C0) AS CI, Sum(OI_Union.V0) AS VI, Sum(OI_Union.C1) AS CCIn, Sum(OI_Union.V1) AS VCI …`
- sectiuni: 5 · controale: 77

### Trasabilitate schita raport LISTA_INVENTAR

Sursa: [Reports/LISTA_INVENTAR.txt](../../VERCON_ACCESS/Reports/LISTA_INVENTAR.txt) · imagine: [R_LISTA_INVENTAR.png](../R_LISTA_INVENTAR.png)

- Sursa de date (RecordSource): `SELECT Balanta22.IDM, Balanta22.Masina, Balanta22.CODP, Balanta22.DENP, Balanta22.UMP, Balanta22.PT, Balanta22.CI, Balanta22.VI, Balanta22.CCIn, Balanta22.VCIn, Balanta22.CCout, Balanta22.VCOut, Balanta22.CF, Balanta22.VF FROM Balanta22 WHERE (((Balanta22.CF)<>0));`
- sectiuni: 7 · controale: 82

## Secțiuni

| Secțiune | MF_LISTA_INVENTAR | OI_LISTA_INVENTAR | OI_LISTA_INVENTAR_TOTAL | LISTA_INVENTAR |
|---|---|---|---|---|
| ReportHeader | — | — | 1 / 0 / 174 | 1 / 0 / 190 |
| PageHeaderSection | 1 / 2256 / 191 | 1 / 2256 / 182 | 2 / 2256 / 187 | 2 / 2256 / 203 |
| GroupHeader0 | 2 / 0 / 919 | 2 / 0 / 910 | — | 3 / 0 / 931 |
| Detail | 3 / 270 / 929 | 3 / 270 / 920 | 3 / 270 / 915 | 4 / 270 / 941 |
| GroupFooter2 | 4 / 285 / 1414 | — | — | — |
| GroupFooter1 | 5 / 1061 / 1529 | 4 / 1061 / 1407 | — | 5 / 1133 / 1429 |
| PageFooterSection | 6 / 1644 / 1671 | 5 / 1644 / 1549 | 4 / 1512 / 1403 | 6 / 1512 / 1572 |
| ReportFooter | — | — | 5 / 1134 / 1838 | 7 / 290 / 2012 |

## Controale

### ReportHeader <a id="reportheader-inaltime-0-twips-linia-174"></a><a id="reportheader-inaltime-0-twips-linia-190"></a>

_fara controale_

### PageHeaderSection <a id="pageheadersection-inaltime-2256-twips-linia-191"></a><a id="pageheadersection-inaltime-2256-twips-linia-182"></a><a id="pageheadersection-inaltime-2256-twips-linia-187"></a><a id="pageheadersection-inaltime-2256-twips-linia-203"></a>

| Control | Tip | Text / camp / sursa | Pozitie (twips) | MF_LISTA_INVENTAR | OI_LISTA_INVENTAR | OI_LISTA_INVENTAR_TOTAL | LISTA_INVENTAR |
|---|---|---|---|---|---|---|---|
| Label0 | Label | LISTĂ DE INVENTARIERE MIJLOACE FIXE | 0,0 15251x735 | 1:198 | — | — | — |
| Label0 | Label | LISTĂ DE INVENTARIERE | 0,0 15251x735 | — | 1:189 | — | — |
| Label0 | Label | LISTĂ DE INVENTARIERE OBIECTE INVENTAR | 0,0 15251x735 | — | — | 1:194 | — |
| Label0 | Label | LISTĂ DE INVENTARIERE MAGAZIE | 0,0 15251x735 | — | — | — | 1:210 |
| Text3 | TextBox | =UCase(DLookUp("Firma","CaleTemp")) | 0,1077 4536x345 | 2:217 | 2:208 | 2:213 | 2:229 |
| Label4 | Label | UNITATEA | 0,737 4530x690 | 3:239 | 3:230 | 3:235 | 3:251 |
| Text5 | TextBox | =[forms]![sfp]![dsf] | 4530,1077 5436x345 | 4:262 | 4:253 | — | — |
| Text5 | TextBox | ="__________________" | 4530,1077 5436x345 | — | — | 4:258 | 4:274 |
| Label6 | Label | DATA | 4530,737 5430x690 | 5:287 | 5:278 | 5:283 | 5:299 |
| Text9 | TextBox | =[Denumire] & " / " & [Adresa] | 9966,1077 4530x345 | 6:312 | 6:303 | — | — |
| Text9 | TextBox | =UCase(DLookUp("Firma","CaleTemp")) | 9966,1077 4530x345 | — | — | 6:308 | — |
| Text9 | TextBox | Masina | 9966,1077 4530x345 | — | — | — | 6:324 |
| Label10 | Label | Gestiunea | 9966,737 4530x690 | 7:337 | 7:328 | 7:333 | 7:349 |
| Text11 | TextBox |  | 14496,1077 765x345 | 8:362 | 8:353 | 8:358 | 8:374 |
| Label12 | Label | Pagina | 14496,737 765x690 | 9:386 | 9:377 | 9:382 | 9:398 |
| Label15 | Label |  DENUMIREA BUNURILOR IN INVENTAR  | 0,1512 3690x720 | 10:411 | 10:402 | 10:407 | 10:423 |
| Label16 | Label | Număr de inventar | 3690,1512 840x720 | 11:434 | 11:425 | 11:430 | 11:446 |
| Label17 | Label |  U/M  | 4530,1512 450x720 | 12:459 | 12:450 | 12:455 | 12:471 |
| Label18 | Label | CANTITĂŢI | 4980,1512 3405x225 | 13:484 | 13:475 | 13:480 | 13:496 |
| Label19 | Label | Stocuri | 4980,1737 1695x225 | 14:509 | 14:500 | 14:505 | 14:521 |
| Label20 | Label | Faptice | 4980,1962 855x270 | 15:534 | 15:525 | 15:530 | 15:546 |
| Label21 | Label | Diferenţe | 6675,1737 1710x225 | 16:560 | 16:551 | 16:556 | 16:572 |
| Label22 | Label | Scriptice | 5835,1962 840x270 | 17:585 | 17:576 | 17:581 | 17:597 |
| Label23 | Label | Plus | 6675,1962 855x270 | 18:611 | 18:602 | 18:607 | 18:623 |
| Label24 | Label | Minus | 7530,1962 855x270 | 19:637 | 19:628 | 19:633 | 19:649 |
| Label25 | Label |  Preţ unitar | 8385,1512 1035x720 | 20:663 | 20:654 | 20:659 | 20:675 |
| Label26 | Label | Valoare contabilă | 9420,1512 2790x225 | 21:688 | 21:679 | 21:684 | 21:700 |
| Label27 | Label | Diferenţe | 10545,1737 1665x225 | 22:713 | 22:704 | 22:709 | 22:725 |
| Label28 | Label | Plus | 10545,1962 855x270 | 23:738 | 23:729 | 23:734 | 23:750 |
| Label29 | Label | Minus | 11400,1962 810x270 | 24:764 | 24:755 | 24:760 | 24:776 |
| Label30 | Label | Valoare | 9420,1737 1125x495 | 25:790 | 25:781 | 25:786 | 25:802 |
| Label31 | Label | Valoare de inventar | 12210,1512 1125x720 | 26:815 | 26:806 | 26:811 | 26:827 |
| Label32 | Label | Deprecierea | 13335,1512 1920x450 | 27:840 | 27:831 | 27:836 | 27:852 |
| Label33 | Label | Valoare | 13335,1962 960x270 | 28:865 | 28:856 | 28:861 | 28:877 |
| Label34 | Label | Cantitatea | 14295,1962 960x270 | 29:891 | 29:882 | 29:887 | 29:903 |

### GroupHeader0 <a id="groupheader0-inaltime-0-twips-linia-919"></a><a id="groupheader0-inaltime-0-twips-linia-910"></a><a id="groupheader0-inaltime-0-twips-linia-931"></a>

_fara controale_

### Detail <a id="detail-inaltime-270-twips-linia-929"></a><a id="detail-inaltime-270-twips-linia-920"></a><a id="detail-inaltime-270-twips-linia-915"></a><a id="detail-inaltime-270-twips-linia-941"></a>

| Control | Tip | Text / camp / sursa | Pozitie (twips) | MF_LISTA_INVENTAR | OI_LISTA_INVENTAR | OI_LISTA_INVENTAR_TOTAL | LISTA_INVENTAR |
|---|---|---|---|---|---|---|---|
| DP | TextBox | DENP | 0,0 3690x270 | 1:938 | 1:929 | 1:924 | 1:949 |
| Label36 | TextBox | COD | 3690,0 840x270 | 2:960 | — | — | — |
| Label36 | TextBox | CodProdus | 3690,0 840x270 | — | 2:951 | — | — |
| Label36 | TextBox |  | 3690,0 840x270 | — | — | 2:946 | 2:971 |
| Fapt | TextBox | =IIf([Forms]![sfp]![fapt],[CF],"") | 4980,0 855x270 | 3:984 | 3:975 | 3:969 | — |
| Label39 | TextBox | CF | 5835,0 840x270 | 4:1010 | 4:1001 | 4:995 | 3:994 |
| Label40 | TextBox | ="-" | 6675,0 855x270 | — | — | 5:1021 | 4:1021 |
| Label40 | TextBox |  | 6675,0 855x270 | 5:1036 | 5:1027 | — | — |
| Label41 | TextBox | ="-" | 7530,0 855x270 | — | — | 6:1048 | 5:1048 |
| Label41 | TextBox |  | 7530,0 855x270 | 6:1062 | 6:1053 | — | — |
| Label42 | TextBox | PT | 8385,0 1035x270 | 7:1088 | 7:1079 | 7:1075 | 6:1075 |
| Label45 | TextBox | VF | 9420,0 1125x270 | 8:1114 | 8:1106 | 8:1102 | 7:1102 |
| Label43 | TextBox |  | 10545,0 855x270 | 9:1141 | 9:1133 | 9:1129 | 8:1129 |
| Label44 | TextBox |  | 11400,0 810x270 | 10:1167 | 10:1159 | 10:1155 | 9:1155 |
| Label46 | TextBox | =IIf([Forms]![sfp]![fapt],[VF],"") | 12210,0 1125x270 | 11:1193 | 11:1185 | 11:1181 | 10:1181 |
| Label47 | TextBox |  | 13335,0 960x270 | 12:1219 | 12:1212 | 12:1208 | 11:1208 |
| Text69 | TextBox | =IIf([Forms]![sfp]![fapt],[CF],"") | 4980,0 855x270 | — | — | — | 12:1234 |
| Label48 | TextBox |  | 14295,0 960x270 | 13:1245 | 13:1238 | 13:1234 | 13:1260 |
| Label37 | TextBox | UMP | 4530,0 450x270 | 14:1271 | 14:1264 | 14:1260 | 14:1286 |
| Line71 | Line |  | 0,0 0x270 | 15:1295 | 15:1288 | 15:1284 | 15:1310 |
| Line72 | Line |  | 4980,0 0x270 | 16:1304 | 16:1297 | 16:1293 | 16:1319 |
| Line73 | Line |  | 6675,0 0x270 | 17:1316 | 17:1309 | 17:1305 | 17:1331 |
| Line74 | Line |  | 8385,0 0x270 | 18:1328 | 18:1321 | 18:1317 | 18:1343 |
| Line77 | Line |  | 9420,0 0x270 | 19:1340 | 19:1333 | 19:1329 | 19:1355 |
| Line75 | Line |  | 12210,0 0x270 | 20:1352 | 20:1345 | 20:1341 | 20:1367 |
| Line78 | Line |  | 13335,0 0x270 | 21:1364 | 21:1357 | 21:1353 | 21:1379 |
| Line76 | Line |  | 15255,0 0x270 | 22:1376 | 22:1369 | 22:1365 | 22:1391 |
| Line79 | Line |  | 3690,0 0x270 | 23:1388 | 23:1381 | 23:1377 | 23:1403 |
| Line80 | Line |  | 4530,0 0x270 | 24:1400 | 24:1393 | 24:1389 | 24:1415 |

### GroupFooter2 <a id="groupfooter2-inaltime-285-twips-linia-1414"></a>

| Control | Tip | Text / camp / sursa | Pozitie (twips) | MF_LISTA_INVENTAR | OI_LISTA_INVENTAR | OI_LISTA_INVENTAR_TOTAL | LISTA_INVENTAR |
|---|---|---|---|---|---|---|---|
| Text86 | TextBox | =Sum([vf]) | 8984,0 1555x285 | 1:1425 | — | — | — |
| Text87 | TextBox | ="TOTAL CLASIFICATIA " & [CC] | 0,0 9414x285 | 2:1449 | — | — | — |
| Text88 | TextBox | =IIf([Forms]![sfp]![fapt],Sum([vf]),"") | 11777,0 1555x285 | 3:1470 | — | — | — |
| Box89 | Rectangle |  | 10539,0 1236x285 | 4:1495 | — | — | — |
| Box90 | Rectangle |  | 13332,0 1932x285 | 5:1511 | — | — | — |

### GroupFooter1 <a id="groupfooter1-inaltime-1061-twips-linia-1529"></a><a id="groupfooter1-inaltime-1061-twips-linia-1407"></a><a id="groupfooter1-inaltime-1133-twips-linia-1429"></a>

| Control | Tip | Text / camp / sursa | Pozitie comună (twips) | MF_LISTA_INVENTAR | OI_LISTA_INVENTAR | OI_LISTA_INVENTAR_TOTAL | LISTA_INVENTAR |
|---|---|---|---|---|---|---|---|
| Text51 | TextBox | =Sum([vf]) | 8984,0 1555x285 | 1:1540 | 1:1418 | — | 1:1440 / 9096,0 1449x285 |
| Label68 | Label | Toate bunurile au fost inventariate în prezenţa mea prin numărare şi/sau cântărire şi nu am obiecţiuni de făcut.  Bunuri | 0,498 15250x563 | 2:1566 | 2:1444 | — | — |
| Label49 | TextBox | ="TOTAL " & [Denumire] | 0,0 9414x285 | 3:1587 | 3:1465 | — | — |
| Text70 | TextBox | =IIf([Forms]![sfp]![fapt],Sum([vf]),"") | 11777,0 1555x285 | 4:1609 | 4:1487 | — | 4:1509 / 11886,0 1449x285 |
| Box81 | Rectangle |  | 10539,0 1236x285 | 5:1635 | 5:1513 / 10539,0 1668x285 | — | 5:1536 / 10545,0 1668x285 |
| Box82 | Rectangle |  | 13332,0 1932x285 | 6:1652 | 6:1530 | — | 6:1553 / 13335,0 1932x285 |
| Label68 | Label | Declar pe propria răspundere că inventarierea s-a făcut în prezenţa mea prin numărare . Nu am obiecţii, drept pentru car | 0,498 15250x635 | — | — | — | 2:1467 |
| Label49 | TextBox | ="TOTAL " & [Masina] | 0,0 9414x290 | — | — | — | 3:1487 |

### PageFooterSection <a id="pagefootersection-inaltime-1644-twips-linia-1671"></a><a id="pagefootersection-inaltime-1644-twips-linia-1549"></a><a id="pagefootersection-inaltime-1512-twips-linia-1403"></a><a id="pagefootersection-inaltime-1512-twips-linia-1572"></a>

| Control | Tip | Text / camp / sursa | Pozitie comună (twips) | MF_LISTA_INVENTAR | OI_LISTA_INVENTAR | OI_LISTA_INVENTAR_TOTAL | LISTA_INVENTAR |
|---|---|---|---|---|---|---|---|
| Label52 | Label | Numele şi prenumele  | 0,384 2202x285 | 1:1678 | 1:1556 | 1:1410 / 0,384 3690x285 | 1:1579 / 0,384 3690x285 |
| Label53 | Label | Semnătura | 0,669 2202x285 | 2:1699 | 2:1577 | 2:1431 / 0,669 3690x285 | 2:1600 / 0,669 3690x285 |
| Label54 | Label | Comisia de inventariere | 2202,96 8343x285 | 3:1720 | 3:1598 | 3:1452 / 3690,96 6855x285 | 3:1621 / 3690,96 6855x285 |
| Text59 | TextBox |  | 2202,669 2081x285 | 4:1744 | 4:1622 | 4:1476 / 3690,669 2285x285 | 7:1720 / 3690,669 2285x285 |
| Text60 | TextBox |  | 4283,669 2081x285 | 5:1768 | 5:1646 | 5:1500 / 5975,669 2285x285 | 8:1744 / 5975,669 2285x285 |
| Text61 | TextBox |  | 6364,669 2081x285 | 6:1793 | 6:1671 | 6:1525 / 8260,669 2285x285 | 9:1769 / 8260,669 2285x285 |
| Label62 | Label | Gestionar (responsabil I.C.) | 10545,96 2385x285 | 7:1818 | 7:1696 | 7:1550 | 10:1794 |
| Text64 | TextBox |  | 10545,669 2385x285 | 8:1841 | 8:1719 | 8:1573 | 12:1843 |
| Label65 | Label | Contabilitate | 12930,96 2325x285 | 9:1866 | 9:1744 | 9:1598 | 13:1868 |
| Text66 | TextBox |  | 12930,384 2325x285 | 10:1890 | 10:1768 | 10:1622 | 14:1892 |
| Text67 | TextBox |  | 12930,669 2325x285 | 11:1915 | 11:1793 | 11:1647 | 15:1917 |
| Text46 | TextBox | ="Pagina " & [Page] & " din " & [Pages] | 10210,1008 5040x285 | 12:1940 | 12:1818 | 13:1697 | 17:1967 |
| Box83 | Rectangle |  | 0,96 2202x285 | 13:1966 | 13:1844 | 14:1723 / 0,96 3690x285 | 18:1993 / 0,96 3690x285 |
| Comisie1 | TextBox | =DLookUp("Receptie1","ComisieInventar") | 2202,384 2081x285 | 14:1983 | 14:1861 | 15:1740 / 3690,384 2285x285 | 4:1645 / 3690,384 2285x285 |
| Comisie2 | TextBox | =DLookUp("Receptie2","ComisieInventar") | 4283,384 2081x285 | 15:2008 | 15:1886 | 16:1764 / 5975,384 2285x285 | 5:1670 / 5975,384 2285x285 |
| Comisie3 | TextBox | =DLookUp("Receptie3","ComisieInventar") | 6364,384 2081x285 | 16:2033 | 16:1911 | 17:1788 / 8260,384 2285x285 | 6:1695 / 8260,384 2285x285 |
| Gestionar | TextBox | =DLookUp("Gestionar","UNIT") | 10545,384 2385x285 | 17:2058 | 17:1936 | 18:1812 | 11:1817 |
| Text84 | TextBox |  | 8445,669 2105x285 | 18:2083 | 18:1961 | — | — |
| Text85 | TextBox | =DLookUp("Receptie4","ComisieInventar") | 8445,384 2105x285 | 19:2107 | 19:1985 | — | — |
| Label107 | TextBox | =DLookUp("expl","ver_rap") | 0,1008 2736x477 | 20:2132 / 0,1074 2736x477 | 20:2010 / 0,1077 2736x477 | 12:1672 | 16:1942 |

### ReportFooter <a id="reportfooter-inaltime-1134-twips-linia-1838"></a><a id="reportfooter-inaltime-290-twips-linia-2012"></a>

| Control | Tip | Text / camp / sursa | Pozitie (twips) | MF_LISTA_INVENTAR | OI_LISTA_INVENTAR | OI_LISTA_INVENTAR_TOTAL | LISTA_INVENTAR |
|---|---|---|---|---|---|---|---|
| Text51 | TextBox | =Sum([vf]) | 8644,0 1895x285 | — | — | 1:1849 | — |
| Text84 | TextBox | =Sum([vf]) | 9120,0 1425x285 | — | — | — | 1:2022 |
| Label68 | Label | Toate obiectele de inventar au fost inventariate în prezenţa mea prin numărare şi/sau cântărire şi nu am obiecţiuni de   | 0,498 15250x635 | — | — | 2:1875 | — |
| Text85 | TextBox | ="TOTAL GENERAL" | 0,0 9414x290 | — | — | — | 2:2047 |
| Label49 | TextBox | ="TOTAL " & UCase(DLookUp("Firma","CaleTemp")) | 0,0 8644x285 | — | — | 3:1896 | — |
| Text86 | TextBox | =IIf([Forms]![sfp]![fapt],Sum([vf]),"") | 11910,0 1425x285 | — | — | — | 3:2068 |
| Text70 | TextBox | =IIf([Forms]![sfp]![fapt],Sum([vf]),"") | 11437,0 1895x285 | — | — | 4:1918 | — |
| Box87 | Rectangle |  | 10545,0 1668x285 | — | — | — | 4:2094 |
| Box81 | Rectangle |  | 10539,0 898x285 | — | — | 5:1944 | — |
| Box88 | Rectangle |  | 13335,0 1932x285 | — | — | — | 5:2111 |
| Box82 | Rectangle |  | 13332,0 1932x285 | — | — | 6:1961 | — |
