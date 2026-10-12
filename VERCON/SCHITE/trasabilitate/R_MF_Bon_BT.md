# Trasabilitate schita raport MF_Bon_BT

Sursa: [Reports/MF_Bon_BT.txt](../../VERCON_ACCESS/Reports/MF_Bon_BT.txt) · imagine: [R_MF_Bon_BT.png](../R_MF_Bon_BT.png)

- Titlu (Caption): —
- Sursa de date (RecordSource): `SELECT OI_Transfer.IDBT, OI_Transfer.IDC_BT, OI_Transfer.IDC_OLD, OI_Transfer.IdMasina, Masini.Masina, C1.Denumire AS Primitor, C2.Denumire AS Predator, OI_Transfer.NRBT, OI_Transfer.DATABT, OI_Transfer.CodProdus, OI_Transfer.DenumireProdus, OI_Transfer.UM, OI_Transfer.Cant, OI_Transfer.PUTVA, OI_Transfer.ValoareCUTVA, OI_Transfer.Expl FROM Masini INNER JOIN (Clienti AS C2 INNER JOIN (Clienti AS C …`
- Latime: 10752 twips · sectiuni: 5 · controale: 41

## PageHeaderSection (inaltime 0 twips, linia 155)

_fara controale_

## GroupHeader0 (inaltime 1974 twips, linia 162)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text0 | TextBox | =DLookUp("Firma","caleTemp") | 0,0 8133x510 | 170 |  |
| Label5 | Label | Data | 1251,510 2214x283 | 192 |  |
| Ziua | TextBox | =Day([DATABT]) | 1251,1077 737x285 | 211 |  |
| Label6 | Label | Ziua | 1251,793 737x284 | 236 |  |
| Text10 | TextBox | =Month([DATABT]) | 1988,1077 737x285 | 261 |  |
| Label7 | Label | Luna | 1988,793 737x284 | 286 |  |
| Text11 | TextBox | =Year([DATABT]) | 2725,1077 737x285 | 311 |  |
| Label8 | Label | Anul | 2725,793 737x284 | 336 |  |
| Label14 | Label | Nr. comandă | 6948,792 1185x285 | 361 |  |
| Text16 | TextBox | Predator | 3462,1077 1755x285 | 384 |  |
| Label12 | Label | Predător | 3462,793 1755x285 | 409 |  |
| Denumire | TextBox | Primitor | 5217,1077 1734x285 | 434 |  |
| Label13 | Label | Primitor | 5217,792 1734x285 | 459 |  |
| Label19 | Label | BON DE TRANSFER | 8133,0 2616x1372 | 484 |  |
| Label20 | Label | Nr. crt. | 0,1464 630x510 | 508 |  |
| Label21 | Label | Denumirea materialelor (inclusiv sort, marca, profil, dimensiune) | 630,1464 4125x510 | 530 |  |
| Label23 | Label | Cod | 4755,1464 1485x510 | 554 |  |
| Label24 | Label | U/M | 6240,1464 711x510 | 578 |  |
| Label25 | Label | Cantitatea efectivă | 6951,1464 1185x510 | 602 |  |
| Label26 | Label | Preț unitar | 8136,1464 1185x510 | 626 |  |
| Label27 | Label | Valoarea | 9321,1464 1425x510 | 650 |  |
| Text47 | TextBox | Masina | 3465,510 4671x282 | 674 |  |
| NRBP | TextBox | NRBT | 0,1080 1247x285 | 700 |  |
| Label3 | Label | Număr document | 0,510 1247x565 | 723 |  |
| Label49 | Label |  | 6948,1077 1185x285 | 747 |  |

## Detail (inaltime 285 twips, linia 771)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| ValoareCUTVA | TextBox | ValoareCUTVA | 9312,0 1437x285 | 779 |  |
| PUTVA | TextBox | PUTVA | 8127,0 1185x285 | 804 |  |
| Text35 | TextBox | Cant | 6942,0 1185x285 | 830 |  |
| UM | TextBox | UM | 6231,0 711x285 | 856 |  |
| CodProdusx | TextBox | =Left([CodProdus],10) | 4755,0 1476x285 | 879 |  |
| Produse.Denumire | TextBox | DenumireProdus | 630,0 4125x285 | 902 |  |
| Text29 | TextBox | =1 | 0,0 630x285 | 929 |  |

## GroupFooter1 (inaltime 1725 twips, linia 954)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text43 | TextBox | ="TOTAL BON" | 630,0 4125x285 | 963 |  |
| Text46 | TextBox | =Sum([ValoareCUTVA]) | 9312,0 1437x285 | 987 |  |
| Text38 | TextBox | ="<b>Data și semnătura:</b><BR>" & [DATABT] | 0,624 2721x510 | 1015 |  |
| Text39 | TextBox | ="Viza C.T.C." | 2721,624 2826x510 | 1039 |  |
| Text40 | TextBox | ="Predător" | 5547,624 2601x510 | 1065 |  |
| Text41 | TextBox | ="Primitor" | 8148,624 2601x510 | 1091 |  |
| Label107 | TextBox | =DLookUp("expl","ver_rap") | 0,1248 2736x477 | 1117 |  |
| Text51 | TextBox |  | 0,0 630x285 | 1141 |  |
| Text52 | TextBox |  | 4755,0 4560x285 | 1163 |  |

## PageFooterSection (inaltime 0 twips, linia 1188)

_fara controale_

