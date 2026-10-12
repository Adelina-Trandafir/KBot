# Trasabilitate schita raport MF_Bon_BS

Sursa: [Reports/MF_Bon_BS.txt](../../VERCON_ACCESS/Reports/MF_Bon_BS.txt) · imagine: [R_MF_Bon_BS.png](../R_MF_Bon_BS.png)

- Titlu (Caption): —
- Sursa de date (RecordSource): `SELECT OI_Casare.IDBC, OI_Casare.IDC_BC, OI_Casare.IdMasina, Masini.Masina, Clienti.Denumire, OI_Casare.NRBC, OI_Casare.DATABC, OI_Casare.CodProdus, OI_Casare.DenumireProdus, OI_Casare.UM, OI_Casare.Cant, OI_Casare.PUTVA, OI_Casare.ValoareCUTVA, OI_Casare.Expl FROM Masini INNER JOIN (Clienti INNER JOIN OI_Casare ON Clienti.IDC = OI_Casare.IDC_BC) ON Masini.IdMasina = OI_Casare.IdMasina WHERE (((OI …`
- Latime: 10752 twips · sectiuni: 5 · controale: 41

## PageHeaderSection (inaltime 0 twips, linia 153)

_fara controale_

## GroupHeader0 (inaltime 1974 twips, linia 160)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text0 | TextBox | =DLookUp("Firma","caleTemp") | 0,0 8133x510 | 168 |  |
| Label5 | Label | Data | 1251,510 2214x283 | 190 |  |
| Ziua | TextBox | =Day([DATABC]) | 1251,1077 737x285 | 209 |  |
| Label6 | Label | Ziua | 1251,793 737x284 | 234 |  |
| Text10 | TextBox | =Month([DATABC]) | 1988,1077 737x285 | 259 |  |
| Label7 | Label | Luna | 1988,793 737x284 | 284 |  |
| Text11 | TextBox | =Year([DATABC]) | 2725,1077 737x285 | 309 |  |
| Label8 | Label | Anul | 2725,793 737x284 | 334 |  |
| Label14 | Label | Nr. comandă | 6948,792 1185x285 | 359 |  |
| Predator | TextBox | Denumire | 3462,1077 1755x285 | 382 |  |
| Label12 | Label | Predător | 3462,793 1755x285 | 407 |  |
| Primitor | TextBox | ="Casare" | 5217,1077 1734x285 | 432 |  |
| Label13 | Label | Primitor | 5217,792 1734x285 | 457 |  |
| Label19 | Label | BON DE CASARE | 8133,0 2616x1372 | 482 |  |
| Label20 | Label | Nr. crt. | 0,1464 630x510 | 504 |  |
| Label21 | Label | Denumirea materialelor (inclusiv sort, marca, profil, dimensiune) | 630,1464 4125x510 | 526 |  |
| Label23 | Label | Cod | 4755,1464 1485x510 | 550 |  |
| Label24 | Label | U/M | 6240,1464 711x510 | 574 |  |
| Label25 | Label | Cantitatea efectivă | 6951,1464 1185x510 | 598 |  |
| Label26 | Label | Preț unitar | 8136,1464 1185x510 | 622 |  |
| Label27 | Label | Valoarea | 9321,1464 1425x510 | 646 |  |
| Text47 | TextBox | Masina | 3465,510 4671x282 | 670 |  |
| NRBP | TextBox | NRBC | 0,1080 1247x285 | 696 |  |
| Label3 | Label | Număr document | 0,510 1247x565 | 719 |  |
| Label49 | Label |  | 6948,1077 1185x285 | 743 |  |

## Detail (inaltime 285 twips, linia 767)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| ValoareCUTVA | TextBox | ValoareCUTVA | 9312,0 1437x285 | 775 |  |
| PUTVA | TextBox | PUTVA | 8127,0 1185x285 | 800 |  |
| Text35 | TextBox | Cant | 6942,0 1185x285 | 826 |  |
| UM | TextBox | UM | 6231,0 711x285 | 852 |  |
| CodProdusx | TextBox | =Left([CodProdus],10) | 4755,0 1476x285 | 875 |  |
| Produse.Denumire | TextBox | DenumireProdus | 630,0 4125x285 | 898 |  |
| Text29 | TextBox | =1 | 0,0 630x285 | 925 |  |

## GroupFooter1 (inaltime 1725 twips, linia 951)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text43 | TextBox | ="TOTAL BON" | 630,0 4125x285 | 960 |  |
| Text46 | TextBox | =Sum([ValoareCUTVA]) | 9312,0 1437x285 | 984 |  |
| Text38 | TextBox | ="<b>Data și semnătura:</b><BR>" & [DATABC] | 0,624 2721x510 | 1012 |  |
| Text39 | TextBox | ="Viza C.T.C." | 2721,624 2826x510 | 1036 |  |
| Text40 | TextBox | ="Predător" | 5547,624 2601x510 | 1062 |  |
| Text41 | TextBox | ="Primitor" | 8148,624 2601x510 | 1088 |  |
| Label107 | TextBox | =DLookUp("expl","ver_rap") | 0,1248 2736x477 | 1114 |  |
| Text51 | TextBox |  | 0,0 630x285 | 1138 |  |
| Text52 | TextBox |  | 4755,0 4560x285 | 1160 |  |

## PageFooterSection (inaltime 0 twips, linia 1185)

_fara controale_

