# Trasabilitate schita raport OI_Bon_BP

Sursa: [Reports/OI_Bon_BP.txt](../../VERCON_ACCESS/Reports/OI_Bon_BP.txt) · imagine: [R_OI_Bon_BP.png](../R_OI_Bon_BP.png)

- Titlu (Caption): —
- Sursa de date (RecordSource): `SELECT OI_Predare.IDBP, OI_Predare.IDC_BP, OI_Predare.IdMasina, Masini.Masina, Clienti.Denumire, OI_Predare.NRBP, OI_Predare.DATABP, OI_Predare.CodProdus, OI_Predare.DenumireProdus, OI_Predare.UM, OI_Predare.Cant, OI_Predare.PUTVA, OI_Predare.ValoareCUTVA, OI_Predare.Expl FROM Masini INNER JOIN (Clienti INNER JOIN OI_Predare ON Clienti.IDC = OI_Predare.IDC_BP) ON Masini.IdMasina = OI_Predare.IdMas …`
- Latime: 10752 twips · sectiuni: 5 · controale: 41

## PageHeaderSection (inaltime 0 twips, linia 154)

_fara controale_

## GroupHeader0 (inaltime 1974 twips, linia 161)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text0 | TextBox | =DLookUp("Firma","caleTemp") | 0,0 8133x510 | 169 |  |
| Label5 | Label | Data | 1251,510 2214x283 | 191 |  |
| Ziua | TextBox | =Day([DATABP]) | 1251,1077 737x285 | 210 |  |
| Label6 | Label | Ziua | 1251,793 737x284 | 235 |  |
| Text10 | TextBox | =Month([DATABP]) | 1988,1077 737x285 | 260 |  |
| Label7 | Label | Luna | 1988,793 737x284 | 285 |  |
| Text11 | TextBox | =Year([DATABP]) | 2725,1077 737x285 | 310 |  |
| Label8 | Label | Anul | 2725,793 737x284 | 335 |  |
| Label14 | Label | Nr. comandă | 6948,792 1185x285 | 360 |  |
| Text16 | TextBox | ="Magazie" | 3462,1077 1755x285 | 383 |  |
| Label12 | Label | Predător | 3462,793 1755x285 | 408 |  |
| Denumire | TextBox | Denumire | 5217,1077 1734x285 | 433 |  |
| Label13 | Label | Primitor | 5217,792 1734x285 | 458 |  |
| Label19 | Label | BON DE PREDARE | 8133,0 2616x1372 | 483 |  |
| Label20 | Label | Nr. crt. | 0,1464 630x510 | 505 |  |
| Label21 | Label | Denumirea materialelor (inclusiv sort, marca, profil, dimensiune) | 630,1464 4125x510 | 527 |  |
| Label23 | Label | Cod | 4755,1464 1485x510 | 551 |  |
| Label24 | Label | U/M | 6240,1464 711x510 | 575 |  |
| Label25 | Label | Cantitatea efectivă | 6951,1464 1185x510 | 599 |  |
| Label26 | Label | Preț unitar | 8136,1464 1185x510 | 623 |  |
| Label27 | Label | Valoarea | 9321,1464 1425x510 | 647 |  |
| Text47 | TextBox | Masina | 3465,510 4671x282 | 671 |  |
| NRBP | TextBox | NRBP | 0,1080 1247x285 | 697 |  |
| Label3 | Label | Număr document | 0,510 1247x565 | 720 |  |
| Label49 | Label |  | 6948,1077 1185x285 | 744 |  |

## Detail (inaltime 285 twips, linia 768)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| ValoareCUTVA | TextBox | ValoareCUTVA | 9312,0 1437x285 | 776 |  |
| PUTVA | TextBox | PUTVA | 8127,0 1185x285 | 801 |  |
| Text35 | TextBox | Cant | 6942,0 1185x285 | 827 |  |
| UM | TextBox | UM | 6231,0 711x285 | 853 |  |
| CodProdusx | TextBox | =Left([CodProdus],10) | 4755,0 1476x285 | 876 |  |
| Produse.Denumire | TextBox | DenumireProdus | 630,0 4125x285 | 899 |  |
| Text29 | TextBox | =1 | 0,0 630x285 | 926 |  |

## GroupFooter1 (inaltime 1731 twips, linia 951)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text43 | TextBox | ="TOTAL BON" | 630,0 4125x285 | 960 |  |
| Text46 | TextBox | =Sum([ValoareCUTVA]) | 9312,0 1437x285 | 984 |  |
| Text38 | TextBox | ="<b>Data și semnătura:</b><BR>" & [DATABP] | 0,624 2721x510 | 1012 |  |
| Text39 | TextBox | ="Viza C.T.C. (propus mișcarea)" | 2721,624 2826x510 | 1036 |  |
| Text40 | TextBox | ="Predător," | 5547,624 2601x510 | 1062 |  |
| Text41 | TextBox | ="Primitor" | 8148,624 2601x510 | 1088 |  |
| Text51 | TextBox |  | 0,0 630x285 | 1114 |  |
| Text52 | TextBox |  | 4755,0 4560x285 | 1136 |  |
| Label107 | TextBox | =DLookUp("expl","ver_rap") | 0,1254 2736x477 | 1159 |  |

## PageFooterSection (inaltime 0 twips, linia 1184)

_fara controale_

