# Trasabilitate schita raport OI_Predare

Sursa: [Reports/OI_Predare.txt](../../VERCON_ACCESS/Reports/OI_Predare.txt) · imagine: [R_OI_Predare.png](../R_OI_Predare.png)

- Titlu (Caption): —
- Sursa de date (RecordSource): `SELECT OI_Predare.IDBP, OI_Predare.IDC_BP, OI_Predare.IdMasina, Masini.Masina, Clienti.Denumire, OI_Predare.NRBP, OI_Predare.DATABP, OI_Predare.idprodus AS CodProdus, Produse.denumirep AS DenumireProdus, Produse.UM, OI_Predare.Cant, OI_Predare.PUTVA, OI_Predare.ValoareCUTVA, OI_Predare.Expl FROM Produse INNER JOIN ((Clienti INNER JOIN (Masini INNER JOIN OI_Predare ON Masini.IdMasina = OI_Predare.I …`
- Latime: 10752 twips · sectiuni: 5 · controale: 41

## PageHeaderSection (inaltime 0 twips, linia 156)

_fara controale_

## GroupHeader0 (inaltime 1974 twips, linia 163)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text0 | TextBox | =DLookUp("Firma","caleTemp") | 0,0 8133x510 | 171 |  |
| Label5 | Label | Data | 1251,510 2214x283 | 193 |  |
| Ziua | TextBox | =Day([DATABP]) | 1251,1077 737x285 | 212 |  |
| Label6 | Label | Ziua | 1251,793 737x284 | 237 |  |
| Text10 | TextBox | =Month([DATABP]) | 1988,1077 737x285 | 262 |  |
| Label7 | Label | Luna | 1988,793 737x284 | 287 |  |
| Text11 | TextBox | =Year([DATABP]) | 2725,1077 737x285 | 312 |  |
| Label8 | Label | Anul | 2725,793 737x284 | 337 |  |
| Label14 | Label | Nr. comandă | 6948,792 1185x285 | 362 |  |
| Text16 | TextBox | ="Magazie" | 3462,1077 1755x285 | 385 |  |
| Label12 | Label | Predător | 3462,793 1755x285 | 410 |  |
| Denumire | TextBox | Denumire | 5217,1077 1734x285 | 435 |  |
| Label13 | Label | Primitor | 5217,792 1734x285 | 460 |  |
| Label19 | Label | BON DE PREDARE | 8133,0 2616x1372 | 485 |  |
| Label20 | Label | Nr. crt. | 0,1464 630x510 | 507 |  |
| Label21 | Label | Denumirea materialelor (inclusiv sort, marca, profil, dimensiune) | 630,1464 4125x510 | 529 |  |
| Label23 | Label | Cod | 4755,1464 1485x510 | 553 |  |
| Label24 | Label | U/M | 6240,1464 711x510 | 577 |  |
| Label25 | Label | Cantitatea efectivă | 6951,1464 1185x510 | 601 |  |
| Label26 | Label | Preț unitar | 8136,1464 1185x510 | 625 |  |
| Label27 | Label | Valoarea | 9321,1464 1425x510 | 649 |  |
| Text47 | TextBox | Masina | 3465,510 4671x282 | 673 |  |
| NRBP | TextBox | NRBP | 0,1080 1247x285 | 699 |  |
| Label3 | Label | Număr document | 0,510 1247x565 | 722 |  |
| Label49 | Label |  | 6948,1077 1185x285 | 746 |  |

## Detail (inaltime 285 twips, linia 770)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| ValoareCUTVA | TextBox | ValoareCUTVA | 9312,0 1437x285 | 778 |  |
| PUTVA | TextBox | PUTVA | 8127,0 1185x285 | 803 |  |
| Text35 | TextBox | Cant | 6942,0 1185x285 | 829 |  |
| UM | TextBox | UM | 6231,0 711x285 | 855 |  |
| CodProdusx | TextBox | =Left([CodProdus],10) | 4755,0 1476x285 | 878 |  |
| Produse.Denumire | TextBox | DenumireProdus | 630,0 4125x285 | 901 |  |
| Text29 | TextBox | =1 | 0,0 630x285 | 928 |  |

## GroupFooter1 (inaltime 1725 twips, linia 952)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text43 | TextBox | ="TOTAL BON" | 630,0 4125x285 | 961 |  |
| Text46 | TextBox | =Sum([ValoareCUTVA]) | 9312,0 1437x285 | 985 |  |
| Text38 | TextBox | ="<b>Data și semnătura:</b><BR>" & [DATABP] | 0,624 2721x510 | 1013 |  |
| Text39 | TextBox | ="Viza C.T.C. (propus mișcarea)" | 2721,624 2826x510 | 1037 |  |
| Text40 | TextBox | ="Predător," | 5547,624 2601x510 | 1063 |  |
| Text41 | TextBox | ="Primitor" | 8148,624 2601x510 | 1089 |  |
| Label107 | TextBox | =DLookUp("expl","ver_rap") | 0,1248 2736x477 | 1115 |  |
| Text51 | TextBox |  | 0,0 630x285 | 1139 |  |
| Text52 | TextBox |  | 4755,0 4560x285 | 1161 |  |

## PageFooterSection (inaltime 0 twips, linia 1186)

_fara controale_

