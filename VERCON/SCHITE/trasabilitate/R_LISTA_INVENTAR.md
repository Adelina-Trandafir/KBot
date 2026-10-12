# Trasabilitate schita raport LISTA_INVENTAR

Sursa: [Reports/LISTA_INVENTAR.txt](../../VERCON_ACCESS/Reports/LISTA_INVENTAR.txt) · imagine: [R_LISTA_INVENTAR.png](../R_LISTA_INVENTAR.png)

- Titlu (Caption): —
- Sursa de date (RecordSource): `SELECT Balanta22.IDM, Balanta22.Masina, Balanta22.CODP, Balanta22.DENP, Balanta22.UMP, Balanta22.PT, Balanta22.CI, Balanta22.VI, Balanta22.CCIn, Balanta22.VCIn, Balanta22.CCout, Balanta22.VCOut, Balanta22.CF, Balanta22.VF FROM Balanta22 WHERE (((Balanta22.CF)<>0));`
- Latime: 15405 twips · sectiuni: 7 · controale: 82

## ReportHeader (inaltime 0 twips, linia 190)

_fara controale_

## PageHeaderSection (inaltime 2256 twips, linia 203)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label0 | Label | LISTĂ DE INVENTARIERE MAGAZIE | 0,0 15251x735 | 210 |  |
| Text3 | TextBox | =UCase(DLookUp("Firma","CaleTemp")) | 0,1077 4536x345 | 229 |  |
| Label4 | Label | UNITATEA | 0,737 4530x690 | 251 |  |
| Text5 | TextBox | ="__________________" | 4530,1077 5436x345 | 274 |  |
| Label6 | Label | DATA | 4530,737 5430x690 | 299 |  |
| Text9 | TextBox | Masina | 9966,1077 4530x345 | 324 |  |
| Label10 | Label | Gestiunea | 9966,737 4530x690 | 349 |  |
| Text11 | TextBox |  | 14496,1077 765x345 | 374 |  |
| Label12 | Label | Pagina | 14496,737 765x690 | 398 |  |
| Label15 | Label |  DENUMIREA BUNURILOR IN INVENTAR  | 0,1512 3690x720 | 423 |  |
| Label16 | Label | Număr de inventar | 3690,1512 840x720 | 446 |  |
| Label17 | Label |  U/M  | 4530,1512 450x720 | 471 |  |
| Label18 | Label | CANTITĂŢI | 4980,1512 3405x225 | 496 |  |
| Label19 | Label | Stocuri | 4980,1737 1695x225 | 521 |  |
| Label20 | Label | Faptice | 4980,1962 855x270 | 546 |  |
| Label21 | Label | Diferenţe | 6675,1737 1710x225 | 572 |  |
| Label22 | Label | Scriptice | 5835,1962 840x270 | 597 |  |
| Label23 | Label | Plus | 6675,1962 855x270 | 623 |  |
| Label24 | Label | Minus | 7530,1962 855x270 | 649 |  |
| Label25 | Label |  Preţ unitar | 8385,1512 1035x720 | 675 |  |
| Label26 | Label | Valoare contabilă | 9420,1512 2790x225 | 700 |  |
| Label27 | Label | Diferenţe | 10545,1737 1665x225 | 725 |  |
| Label28 | Label | Plus | 10545,1962 855x270 | 750 |  |
| Label29 | Label | Minus | 11400,1962 810x270 | 776 |  |
| Label30 | Label | Valoare | 9420,1737 1125x495 | 802 |  |
| Label31 | Label | Valoare de inventar | 12210,1512 1125x720 | 827 |  |
| Label32 | Label | Deprecierea | 13335,1512 1920x450 | 852 |  |
| Label33 | Label | Valoare | 13335,1962 960x270 | 877 |  |
| Label34 | Label | Cantitatea | 14295,1962 960x270 | 903 |  |

## GroupHeader0 (inaltime 0 twips, linia 931)

_fara controale_

## Detail (inaltime 270 twips, linia 941)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| DP | TextBox | DENP | 0,0 3690x270 | 949 |  |
| Label36 | TextBox |  | 3690,0 840x270 | 971 |  |
| Label39 | TextBox | CF | 5835,0 840x270 | 994 |  |
| Label40 | TextBox | ="-" | 6675,0 855x270 | 1021 |  |
| Label41 | TextBox | ="-" | 7530,0 855x270 | 1048 |  |
| Label42 | TextBox | PT | 8385,0 1035x270 | 1075 |  |
| Label45 | TextBox | VF | 9420,0 1125x270 | 1102 |  |
| Label43 | TextBox |  | 10545,0 855x270 | 1129 |  |
| Label44 | TextBox |  | 11400,0 810x270 | 1155 |  |
| Label46 | TextBox | =IIf([Forms]![sfp]![fapt],[VF],"") | 12210,0 1125x270 | 1181 |  |
| Label47 | TextBox |  | 13335,0 960x270 | 1208 |  |
| Text69 | TextBox | =IIf([Forms]![sfp]![fapt],[CF],"") | 4980,0 855x270 | 1234 |  |
| Label48 | TextBox |  | 14295,0 960x270 | 1260 |  |
| Label37 | TextBox | UMP | 4530,0 450x270 | 1286 |  |
| Line71 | Line |  | 0,0 0x270 | 1310 |  |
| Line72 | Line |  | 4980,0 0x270 | 1319 |  |
| Line73 | Line |  | 6675,0 0x270 | 1331 |  |
| Line74 | Line |  | 8385,0 0x270 | 1343 |  |
| Line77 | Line |  | 9420,0 0x270 | 1355 |  |
| Line75 | Line |  | 12210,0 0x270 | 1367 |  |
| Line78 | Line |  | 13335,0 0x270 | 1379 |  |
| Line76 | Line |  | 15255,0 0x270 | 1391 |  |
| Line79 | Line |  | 3690,0 0x270 | 1403 |  |
| Line80 | Line |  | 4530,0 0x270 | 1415 |  |

## GroupFooter1 (inaltime 1133 twips, linia 1429)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text51 | TextBox | =Sum([vf]) | 9096,0 1449x285 | 1440 |  |
| Label68 | Label | Declar pe propria răspundere că inventarierea s-a făcut în prezenţa mea prin numărare . Nu am obiecţii, drept pentru car | 0,498 15250x635 | 1467 |  |
| Label49 | TextBox | ="TOTAL " & [Masina] | 0,0 9414x290 | 1487 |  |
| Text70 | TextBox | =IIf([Forms]![sfp]![fapt],Sum([vf]),"") | 11886,0 1449x285 | 1509 |  |
| Box81 | Rectangle |  | 10545,0 1668x285 | 1536 |  |
| Box82 | Rectangle |  | 13335,0 1932x285 | 1553 |  |

## PageFooterSection (inaltime 1512 twips, linia 1572)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label52 | Label | Numele şi prenumele  | 0,384 3690x285 | 1579 |  |
| Label53 | Label | Semnătura | 0,669 3690x285 | 1600 |  |
| Label54 | Label | Comisia de inventariere | 3690,96 6855x285 | 1621 |  |
| Comisie1 | TextBox | =DLookUp("Receptie1","ComisieInventar") | 3690,384 2285x285 | 1645 |  |
| Comisie2 | TextBox | =DLookUp("Receptie2","ComisieInventar") | 5975,384 2285x285 | 1670 |  |
| Comisie3 | TextBox | =DLookUp("Receptie3","ComisieInventar") | 8260,384 2285x285 | 1695 |  |
| Text59 | TextBox |  | 3690,669 2285x285 | 1720 |  |
| Text60 | TextBox |  | 5975,669 2285x285 | 1744 |  |
| Text61 | TextBox |  | 8260,669 2285x285 | 1769 |  |
| Label62 | Label | Gestionar (responsabil I.C.) | 10545,96 2385x285 | 1794 |  |
| Gestionar | TextBox | =DLookUp("Gestionar","UNIT") | 10545,384 2385x285 | 1817 |  |
| Text64 | TextBox |  | 10545,669 2385x285 | 1843 |  |
| Label65 | Label | Contabilitate | 12930,96 2325x285 | 1868 |  |
| Text66 | TextBox |  | 12930,384 2325x285 | 1892 |  |
| Text67 | TextBox |  | 12930,669 2325x285 | 1917 |  |
| Label107 | TextBox | =DLookUp("expl","ver_rap") | 0,1008 2736x477 | 1942 |  |
| Text46 | TextBox | ="Pagina " & [Page] & " din " & [Pages] | 10210,1008 5040x285 | 1967 |  |
| Box83 | Rectangle |  | 0,96 3690x285 | 1993 |  |

## ReportFooter (inaltime 290 twips, linia 2012)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text84 | TextBox | =Sum([vf]) | 9120,0 1425x285 | 2022 |  |
| Text85 | TextBox | ="TOTAL GENERAL" | 0,0 9414x290 | 2047 |  |
| Text86 | TextBox | =IIf([Forms]![sfp]![fapt],Sum([vf]),"") | 11910,0 1425x285 | 2068 |  |
| Box87 | Rectangle |  | 10545,0 1668x285 | 2094 |  |
| Box88 | Rectangle |  | 13335,0 1932x285 | 2111 |  |

