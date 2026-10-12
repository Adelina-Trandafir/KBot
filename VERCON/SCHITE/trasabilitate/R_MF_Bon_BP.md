# Trasabilitate schita raport MF_Bon_BP

Sursa: [Reports/MF_Bon_BP.txt](../../VERCON_ACCESS/Reports/MF_Bon_BP.txt) · imagine: [R_MF_Bon_BP.png](../R_MF_Bon_BP.png)

- Titlu (Caption): —
- Sursa de date (RecordSource): `SELECT OI_Predare.IDBP, OI_Predare.IDC_BP, OI_Predare.IdMasina, Masini.Masina, Clienti.Denumire, OI_Predare.NRBP, OI_Predare.DATABP, ProduseP.COD, Produse.DenumireP, Produse.UM, OI_Predare.Cant, OI_Predare.PUTVA, OI_Predare.ValoareCUTVA, OI_Predare.Expl, Produse.CC FROM Produse INNER JOIN ((Masini INNER JOIN (Clienti INNER JOIN OI_Predare ON Clienti.IDC = OI_Predare.IDC_BP) ON Masini.IdMasina = OI …`
- Latime: 10752 twips · sectiuni: 6 · controale: 45

## PageHeaderSection (inaltime 0 twips, linia 159)

_fara controale_

## GroupHeader0 (inaltime 1974 twips, linia 166)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text0 | TextBox | =DLookUp("Firma","caleTemp") | 0,0 8133x510 | 174 |  |
| Label5 | Label | Data | 1251,510 2214x283 | 196 |  |
| Ziua | TextBox | =Day([DATABP]) | 1251,1077 737x285 | 215 |  |
| Label6 | Label | Ziua | 1251,793 737x284 | 240 |  |
| Text10 | TextBox | =Month([DATABP]) | 1988,1077 737x285 | 265 |  |
| Label7 | Label | Luna | 1988,793 737x284 | 290 |  |
| Text11 | TextBox | =Year([DATABP]) | 2725,1077 737x285 | 315 |  |
| Label8 | Label | Anul | 2725,793 737x284 | 340 |  |
| Label14 | Label | Nr. comandă | 6948,792 1185x285 | 365 |  |
| Text16 | TextBox | ="Magazie" | 3462,1077 1755x285 | 388 |  |
| Label12 | Label | Predător | 3462,793 1755x285 | 412 |  |
| Denumire | TextBox | Denumire | 5217,1077 1734x285 | 437 |  |
| Label13 | Label | Primitor | 5217,792 1734x285 | 461 |  |
| Label19 | Label | BON DE PREDARE | 8133,0 2616x1372 | 486 |  |
| Label20 | Label | Nr. crt. | 0,1464 630x510 | 508 |  |
| Label21 | Label | Denumirea materialelor (inclusiv sort, marca, profil, dimensiune) | 630,1464 4125x510 | 530 |  |
| Label23 | Label | Cod | 4755,1464 1485x510 | 554 |  |
| Label24 | Label | U/M | 6240,1464 711x510 | 578 |  |
| Label25 | Label | Cantitatea efectivă | 6951,1464 1185x510 | 602 |  |
| Label26 | Label | Preț unitar | 8136,1464 1185x510 | 626 |  |
| Label27 | Label | Valoarea | 9321,1464 1425x510 | 650 |  |
| Text47 | TextBox | Masina | 3465,510 4671x282 | 674 |  |
| NRBP | TextBox | NRBP | 0,1080 1247x285 | 700 |  |
| Label3 | Label | Număr document | 0,510 1247x565 | 723 |  |
| Label49 | Label |  | 6948,1077 1185x285 | 747 |  |

## Detail (inaltime 285 twips, linia 771)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| ValoareCUTVA | TextBox | ValoareCUTVA | 9312,0 1437x285 | 779 |  |
| PUTVA | TextBox | PUTVA | 8127,0 1185x285 | 804 |  |
| Text35 | TextBox | Cant | 6942,0 1185x285 | 830 |  |
| UM | TextBox | UM | 6231,0 711x285 | 856 |  |
| CodProdusx | TextBox | COD | 4755,0 1476x285 | 879 |  |
| Produse.Denumire | TextBox | DenumireP | 630,0 4125x285 | 902 |  |
| Text29 | TextBox | =1 | 0,0 630x285 | 929 |  |

## GroupFooter2 (inaltime 285 twips, linia 954)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text54 | TextBox | ="TOTAL CLASIFICATIA " & [cc] | 630,0 4125x285 | 968 |  |
| Text55 | TextBox | =Sum([ValoareCUTVA]) | 9312,0 1437x285 | 991 |  |
| Text56 | TextBox |  | 0,0 630x285 | 1018 |  |
| Text57 | TextBox |  | 4755,0 4560x285 | 1039 |  |

## GroupFooter1 (inaltime 1725 twips, linia 1063)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text43 | TextBox | ="TOTAL BON" | 630,0 4125x285 | 1072 |  |
| Text46 | TextBox | =Sum([ValoareCUTVA]) | 9312,0 1437x285 | 1096 |  |
| Text38 | TextBox | ="<b>Data și semnătura:</b><BR>" & [DATABP] | 0,624 2721x510 | 1124 |  |
| Text39 | TextBox | ="Viza C.T.C. (propus mișcarea)" | 2721,624 2826x510 | 1148 |  |
| Text40 | TextBox | ="Predător," | 5547,624 2601x510 | 1174 |  |
| Text41 | TextBox | ="Primitor" | 8148,624 2601x510 | 1200 |  |
| Label107 | TextBox | =DLookUp("expl","ver_rap") | 0,1248 2736x477 | 1226 |  |
| Text51 | TextBox |  | 0,0 630x285 | 1250 |  |
| Text52 | TextBox |  | 4755,0 4560x285 | 1272 |  |

## PageFooterSection (inaltime 0 twips, linia 1297)

_fara controale_

