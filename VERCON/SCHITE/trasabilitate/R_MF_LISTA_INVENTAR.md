# Trasabilitate schita raport MF_LISTA_INVENTAR

Sursa: [Reports/MF_LISTA_INVENTAR.txt](../../VERCON_ACCESS/Reports/MF_LISTA_INVENTAR.txt) · imagine: [R_MF_LISTA_INVENTAR.png](../R_MF_LISTA_INVENTAR.png)

- Titlu (Caption): —
- Sursa de date (RecordSource): `SELECT X.IdMasina, X.IDC, X.Masina, X.Denumire, X.Adresa, X.CodProdus, X.DENP, X.UMP, X.PT, Sum([CI]+[CCIn]-[CCOut]) AS CF, Sum([vi]+[vcin]-[vcout]) AS VF, ProduseP.COD, Produse.CC FROM (SELECT Clienti.Adresa,OI_Union.IDC, Clienti.Denumire, OI_Union.IdMasina, Masini.Masina, OI_Union.CodProdus, OI_Union.DenumireProdus AS DENP, Nz([UM],"Buc") AS UMP, OI_Union.PUTVA AS PT, Sum(OI_Union.C0) AS CI, Sum …`
- Latime: 15405 twips · sectiuni: 6 · controale: 84

## PageHeaderSection (inaltime 2256 twips, linia 191)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label0 | Label | LISTĂ DE INVENTARIERE MIJLOACE FIXE | 0,0 15251x735 | 198 |  |
| Text3 | TextBox | =UCase(DLookUp("Firma","CaleTemp")) | 0,1077 4536x345 | 217 |  |
| Label4 | Label | UNITATEA | 0,737 4530x690 | 239 |  |
| Text5 | TextBox | =[forms]![sfp]![dsf] | 4530,1077 5436x345 | 262 |  |
| Label6 | Label | DATA | 4530,737 5430x690 | 287 |  |
| Text9 | TextBox | =[Denumire] & " / " & [Adresa] | 9966,1077 4530x345 | 312 |  |
| Label10 | Label | Gestiunea | 9966,737 4530x690 | 337 |  |
| Text11 | TextBox |  | 14496,1077 765x345 | 362 |  |
| Label12 | Label | Pagina | 14496,737 765x690 | 386 |  |
| Label15 | Label |  DENUMIREA BUNURILOR IN INVENTAR  | 0,1512 3690x720 | 411 |  |
| Label16 | Label | Număr de inventar | 3690,1512 840x720 | 434 |  |
| Label17 | Label |  U/M  | 4530,1512 450x720 | 459 |  |
| Label18 | Label | CANTITĂŢI | 4980,1512 3405x225 | 484 |  |
| Label19 | Label | Stocuri | 4980,1737 1695x225 | 509 |  |
| Label20 | Label | Faptice | 4980,1962 855x270 | 534 |  |
| Label21 | Label | Diferenţe | 6675,1737 1710x225 | 560 |  |
| Label22 | Label | Scriptice | 5835,1962 840x270 | 585 |  |
| Label23 | Label | Plus | 6675,1962 855x270 | 611 |  |
| Label24 | Label | Minus | 7530,1962 855x270 | 637 |  |
| Label25 | Label |  Preţ unitar | 8385,1512 1035x720 | 663 |  |
| Label26 | Label | Valoare contabilă | 9420,1512 2790x225 | 688 |  |
| Label27 | Label | Diferenţe | 10545,1737 1665x225 | 713 |  |
| Label28 | Label | Plus | 10545,1962 855x270 | 738 |  |
| Label29 | Label | Minus | 11400,1962 810x270 | 764 |  |
| Label30 | Label | Valoare | 9420,1737 1125x495 | 790 |  |
| Label31 | Label | Valoare de inventar | 12210,1512 1125x720 | 815 |  |
| Label32 | Label | Deprecierea | 13335,1512 1920x450 | 840 |  |
| Label33 | Label | Valoare | 13335,1962 960x270 | 865 |  |
| Label34 | Label | Cantitatea | 14295,1962 960x270 | 891 |  |

## GroupHeader0 (inaltime 0 twips, linia 919)

_fara controale_

## Detail (inaltime 270 twips, linia 929)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| DP | TextBox | DENP | 0,0 3690x270 | 938 |  |
| Label36 | TextBox | COD | 3690,0 840x270 | 960 |  |
| Fapt | TextBox | =IIf([Forms]![sfp]![fapt],[CF],"") | 4980,0 855x270 | 984 |  |
| Label39 | TextBox | CF | 5835,0 840x270 | 1010 |  |
| Label40 | TextBox |  | 6675,0 855x270 | 1036 |  |
| Label41 | TextBox |  | 7530,0 855x270 | 1062 |  |
| Label42 | TextBox | PT | 8385,0 1035x270 | 1088 |  |
| Label45 | TextBox | VF | 9420,0 1125x270 | 1114 |  |
| Label43 | TextBox |  | 10545,0 855x270 | 1141 |  |
| Label44 | TextBox |  | 11400,0 810x270 | 1167 |  |
| Label46 | TextBox | =IIf([Forms]![sfp]![fapt],[VF],"") | 12210,0 1125x270 | 1193 |  |
| Label47 | TextBox |  | 13335,0 960x270 | 1219 |  |
| Label48 | TextBox |  | 14295,0 960x270 | 1245 |  |
| Label37 | TextBox | UMP | 4530,0 450x270 | 1271 |  |
| Line71 | Line |  | 0,0 0x270 | 1295 |  |
| Line72 | Line |  | 4980,0 0x270 | 1304 |  |
| Line73 | Line |  | 6675,0 0x270 | 1316 |  |
| Line74 | Line |  | 8385,0 0x270 | 1328 |  |
| Line77 | Line |  | 9420,0 0x270 | 1340 |  |
| Line75 | Line |  | 12210,0 0x270 | 1352 |  |
| Line78 | Line |  | 13335,0 0x270 | 1364 |  |
| Line76 | Line |  | 15255,0 0x270 | 1376 |  |
| Line79 | Line |  | 3690,0 0x270 | 1388 |  |
| Line80 | Line |  | 4530,0 0x270 | 1400 |  |

## GroupFooter2 (inaltime 285 twips, linia 1414)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text86 | TextBox | =Sum([vf]) | 8984,0 1555x285 | 1425 |  |
| Text87 | TextBox | ="TOTAL CLASIFICATIA " & [CC] | 0,0 9414x285 | 1449 |  |
| Text88 | TextBox | =IIf([Forms]![sfp]![fapt],Sum([vf]),"") | 11777,0 1555x285 | 1470 |  |
| Box89 | Rectangle |  | 10539,0 1236x285 | 1495 |  |
| Box90 | Rectangle |  | 13332,0 1932x285 | 1511 |  |

## GroupFooter1 (inaltime 1061 twips, linia 1529)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text51 | TextBox | =Sum([vf]) | 8984,0 1555x285 | 1540 |  |
| Label68 | Label | Toate bunurile au fost inventariate în prezenţa mea prin numărare şi/sau cântărire şi nu am obiecţiuni de făcut.  Bunuri | 0,498 15250x563 | 1566 |  |
| Label49 | TextBox | ="TOTAL " & [Denumire] | 0,0 9414x285 | 1587 |  |
| Text70 | TextBox | =IIf([Forms]![sfp]![fapt],Sum([vf]),"") | 11777,0 1555x285 | 1609 |  |
| Box81 | Rectangle |  | 10539,0 1236x285 | 1635 |  |
| Box82 | Rectangle |  | 13332,0 1932x285 | 1652 |  |

## PageFooterSection (inaltime 1644 twips, linia 1671)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label52 | Label | Numele şi prenumele  | 0,384 2202x285 | 1678 |  |
| Label53 | Label | Semnătura | 0,669 2202x285 | 1699 |  |
| Label54 | Label | Comisia de inventariere | 2202,96 8343x285 | 1720 |  |
| Text59 | TextBox |  | 2202,669 2081x285 | 1744 |  |
| Text60 | TextBox |  | 4283,669 2081x285 | 1768 |  |
| Text61 | TextBox |  | 6364,669 2081x285 | 1793 |  |
| Label62 | Label | Gestionar (responsabil I.C.) | 10545,96 2385x285 | 1818 |  |
| Text64 | TextBox |  | 10545,669 2385x285 | 1841 |  |
| Label65 | Label | Contabilitate | 12930,96 2325x285 | 1866 |  |
| Text66 | TextBox |  | 12930,384 2325x285 | 1890 |  |
| Text67 | TextBox |  | 12930,669 2325x285 | 1915 |  |
| Text46 | TextBox | ="Pagina " & [Page] & " din " & [Pages] | 10210,1008 5040x285 | 1940 |  |
| Box83 | Rectangle |  | 0,96 2202x285 | 1966 |  |
| Comisie1 | TextBox | =DLookUp("Receptie1","ComisieInventar") | 2202,384 2081x285 | 1983 |  |
| Comisie2 | TextBox | =DLookUp("Receptie2","ComisieInventar") | 4283,384 2081x285 | 2008 |  |
| Comisie3 | TextBox | =DLookUp("Receptie3","ComisieInventar") | 6364,384 2081x285 | 2033 |  |
| Gestionar | TextBox | =DLookUp("Gestionar","UNIT") | 10545,384 2385x285 | 2058 |  |
| Text84 | TextBox |  | 8445,669 2105x285 | 2083 |  |
| Text85 | TextBox | =DLookUp("Receptie4","ComisieInventar") | 8445,384 2105x285 | 2107 |  |
| Label107 | TextBox | =DLookUp("expl","ver_rap") | 0,1074 2736x477 | 2132 |  |

