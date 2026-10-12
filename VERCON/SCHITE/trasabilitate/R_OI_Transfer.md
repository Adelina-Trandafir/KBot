# Trasabilitate schita raport OI_Transfer

Sursa: [Reports/OI_Transfer.txt](../../VERCON_ACCESS/Reports/OI_Transfer.txt) · imagine: [R_OI_Transfer.png](../R_OI_Transfer.png)

- Titlu (Caption): —
- Sursa de date (RecordSource): `SELECT OI_Transfer.IDBT, OI_Transfer.IDC_BT, OI_Transfer.IDC_OLD, OI_Transfer.IdMasina, Masini.Masina, C1.Denumire AS Primitor, C2.Denumire AS Predator, OI_Transfer.NRBT, OI_Transfer.DATABT, OI_Transfer.idprodus AS CodProdus, Produse.denumirep AS DenumireProdus, Produse.UM, OI_Transfer.Cant, OI_Transfer.PUTVA, OI_Transfer.ValoareCUTVA, OI_Transfer.Expl FROM Produse INNER JOIN ((Masini INNER JOIN ( …`
- Latime: 10752 twips · sectiuni: 5 · controale: 41

## PageHeaderSection (inaltime 0 twips, linia 157)

_fara controale_

## GroupHeader0 (inaltime 1974 twips, linia 164)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text0 | TextBox | =DLookUp("Firma","caleTemp") | 0,0 8133x510 | 172 |  |
| Label5 | Label | Data | 1251,510 2214x283 | 194 |  |
| Ziua | TextBox | =Day([DATABT]) | 1251,1077 737x285 | 213 |  |
| Label6 | Label | Ziua | 1251,793 737x284 | 238 |  |
| Text10 | TextBox | =Month([DATABT]) | 1988,1077 737x285 | 263 |  |
| Label7 | Label | Luna | 1988,793 737x284 | 288 |  |
| Text11 | TextBox | =Year([DATABT]) | 2725,1077 737x285 | 313 |  |
| Label8 | Label | Anul | 2725,793 737x284 | 338 |  |
| Label14 | Label | Nr. comandă | 6948,792 1185x285 | 363 |  |
| Text16 | TextBox | Predator | 3462,1077 1755x285 | 386 |  |
| Label12 | Label | Predător | 3462,793 1755x285 | 411 |  |
| Denumire | TextBox | Primitor | 5217,1077 1734x285 | 436 |  |
| Label13 | Label | Primitor | 5217,792 1734x285 | 461 |  |
| Label19 | Label | BON DE TRANSFER | 8133,0 2616x1372 | 486 |  |
| Label20 | Label | Nr. crt. | 0,1464 630x510 | 510 |  |
| Label21 | Label | Denumirea materialelor (inclusiv sort, marca, profil, dimensiune) | 630,1464 4125x510 | 532 |  |
| Label23 | Label | Cod | 4755,1464 1485x510 | 556 |  |
| Label24 | Label | U/M | 6240,1464 711x510 | 580 |  |
| Label25 | Label | Cantitatea efectivă | 6951,1464 1185x510 | 604 |  |
| Label26 | Label | Preț unitar | 8136,1464 1185x510 | 628 |  |
| Label27 | Label | Valoarea | 9321,1464 1425x510 | 652 |  |
| Text47 | TextBox | Masina | 3465,510 4671x282 | 676 |  |
| NRBP | TextBox | NRBT | 0,1080 1247x285 | 702 |  |
| Label3 | Label | Număr document | 0,510 1247x565 | 725 |  |
| Label49 | Label |  | 6948,1077 1185x285 | 749 |  |

## Detail (inaltime 285 twips, linia 773)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| ValoareCUTVA | TextBox | ValoareCUTVA | 9312,0 1437x285 | 781 |  |
| PUTVA | TextBox | PUTVA | 8127,0 1185x285 | 806 |  |
| Text35 | TextBox | Cant | 6942,0 1185x285 | 832 |  |
| UM | TextBox | UM | 6231,0 711x285 | 858 |  |
| CodProdusx | TextBox | =Left([CodProdus],10) | 4755,0 1476x285 | 881 |  |
| Produse.Denumire | TextBox | DenumireProdus | 630,0 4125x285 | 904 |  |
| Text29 | TextBox | =1 | 0,0 630x285 | 931 |  |

## GroupFooter1 (inaltime 1725 twips, linia 955)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text43 | TextBox | ="TOTAL BON" | 630,0 4125x285 | 964 |  |
| Text46 | TextBox | =Sum([ValoareCUTVA]) | 9312,0 1437x285 | 988 |  |
| Text38 | TextBox | ="<b>Data și semnătura:</b><BR>" & [DATABT] | 0,624 2721x510 | 1016 |  |
| Text39 | TextBox | ="Viza C.T.C." | 2721,624 2826x510 | 1040 |  |
| Text40 | TextBox | ="Predător" | 5547,624 2601x510 | 1066 |  |
| Text41 | TextBox | ="Primitor" | 8148,624 2601x510 | 1092 |  |
| Label107 | TextBox | =DLookUp("expl","ver_rap") | 0,1248 2736x477 | 1118 |  |
| Text51 | TextBox |  | 0,0 630x285 | 1142 |  |
| Text52 | TextBox |  | 4755,0 4560x285 | 1164 |  |

## PageFooterSection (inaltime 0 twips, linia 1189)

_fara controale_

