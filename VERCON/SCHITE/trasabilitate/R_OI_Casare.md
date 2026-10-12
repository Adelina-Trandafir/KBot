# Trasabilitate schita raport OI_Casare

Sursa: [Reports/OI_Casare.txt](../../VERCON_ACCESS/Reports/OI_Casare.txt) · imagine: [R_OI_Casare.png](../R_OI_Casare.png)

- Titlu (Caption): —
- Sursa de date (RecordSource): `SELECT OI_Casare.IDBC, OI_Casare.IDC_BC, OI_Casare.IdMasina, Masini.Masina, Clienti.Denumire, OI_Casare.NRBC, OI_Casare.DATABC, OI_Casare.idprodus AS CodProdus, Produse.denumirep AS DenumireProdus, Produse.UM, OI_Casare.Cant, OI_Casare.PUTVA, OI_Casare.ValoareCUTVA, OI_Casare.Expl FROM (Clienti INNER JOIN (Masini INNER JOIN OI_Casare ON Masini.IdMasina = OI_Casare.IdMasina) ON Clienti.IDC = OI_Cas …`
- Latime: 10752 twips · sectiuni: 5 · controale: 41

## PageHeaderSection (inaltime 0 twips, linia 155)

_fara controale_

## GroupHeader0 (inaltime 1974 twips, linia 162)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text0 | TextBox | =DLookUp("Firma","caleTemp") | 0,0 8133x510 | 170 |  |
| Label5 | Label | Data | 1251,510 2214x283 | 192 |  |
| Ziua | TextBox | =Day([DATABC]) | 1251,1077 737x285 | 211 |  |
| Label6 | Label | Ziua | 1251,793 737x284 | 236 |  |
| Text10 | TextBox | =Month([DATABC]) | 1988,1077 737x285 | 261 |  |
| Label7 | Label | Luna | 1988,793 737x284 | 286 |  |
| Text11 | TextBox | =Year([DATABC]) | 2725,1077 737x285 | 311 |  |
| Label8 | Label | Anul | 2725,793 737x284 | 336 |  |
| Label14 | Label | Nr. comandă | 6948,792 1185x285 | 361 |  |
| Predator | TextBox | Denumire | 3462,1077 1755x285 | 384 |  |
| Label12 | Label | Predător | 3462,793 1755x285 | 409 |  |
| Primitor | TextBox | ="Casare" | 5217,1077 1734x285 | 434 |  |
| Label13 | Label | Primitor | 5217,792 1734x285 | 459 |  |
| Label19 | Label | BON DE CASARE | 8133,0 2616x1372 | 484 |  |
| Label20 | Label | Nr. crt. | 0,1464 630x510 | 506 |  |
| Label21 | Label | Denumirea materialelor (inclusiv sort, marca, profil, dimensiune) | 630,1464 4125x510 | 528 |  |
| Label23 | Label | Cod | 4755,1464 1485x510 | 552 |  |
| Label24 | Label | U/M | 6240,1464 711x510 | 576 |  |
| Label25 | Label | Cantitatea efectivă | 6951,1464 1185x510 | 600 |  |
| Label26 | Label | Preț unitar | 8136,1464 1185x510 | 624 |  |
| Label27 | Label | Valoarea | 9321,1464 1425x510 | 648 |  |
| Text47 | TextBox | Masina | 3465,510 4671x282 | 672 |  |
| NRBP | TextBox | NRBC | 0,1080 1247x285 | 698 |  |
| Label3 | Label | Număr document | 0,510 1247x565 | 721 |  |
| Label49 | Label |  | 6948,1077 1185x285 | 745 |  |

## Detail (inaltime 285 twips, linia 769)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| ValoareCUTVA | TextBox | ValoareCUTVA | 9312,0 1437x285 | 777 |  |
| PUTVA | TextBox | PUTVA | 8127,0 1185x285 | 802 |  |
| Text35 | TextBox | Cant | 6942,0 1185x285 | 828 |  |
| UM | TextBox | UM | 6231,0 711x285 | 854 |  |
| CodProdusx | TextBox | =Left([CodProdus],10) | 4755,0 1476x285 | 877 |  |
| Produse.Denumire | TextBox | DenumireProdus | 630,0 4125x285 | 900 |  |
| Text29 | TextBox | =1 | 0,0 630x285 | 927 |  |

## GroupFooter1 (inaltime 1134 twips, linia 953)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text43 | TextBox | ="TOTAL BON" | 630,0 4125x285 | 962 |  |
| Text46 | TextBox | =Sum([ValoareCUTVA]) | 9312,0 1437x285 | 986 |  |
| Text38 | TextBox | ="<b>Data și semnătura:</b><BR>" & [DATABC] | 0,624 2721x510 | 1014 |  |
| Text39 | TextBox | ="Viza C.T.C." | 2721,624 2826x510 | 1038 |  |
| Text40 | TextBox | ="Predător" | 5547,624 2601x510 | 1064 |  |
| Text41 | TextBox | ="Primitor" | 8148,624 2601x510 | 1090 |  |
| Text51 | TextBox |  | 0,0 630x285 | 1116 |  |
| Text52 | TextBox |  | 4755,0 4560x285 | 1138 |  |

## PageFooterSection (inaltime 477 twips, linia 1163)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label107 | TextBox | =DLookUp("expl","ver_rap") | 0,0 2736x477 | 1170 |  |

