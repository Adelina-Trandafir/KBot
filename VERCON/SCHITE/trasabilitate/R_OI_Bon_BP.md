# Trasabilitate schite bonuri OI/MF

Convenții și limite: [citirea fișelor consolidate](../INDEX.md#fise-consolidate). SLICE-D003-09, 27.10.2026.

Comune tuturor: Titlu (Caption): —; lățime 10752 twips. Coloana Evenimente era goală la fiecare control.

## Variante și surse

### Trasabilitate schita raport OI_Bon_BP

Sursa: [Reports/OI_Bon_BP.txt](../../VERCON_ACCESS/Reports/OI_Bon_BP.txt) · imagine: [R_OI_Bon_BP.png](../R_OI_Bon_BP.png)

- Sursa de date (RecordSource): `SELECT OI_Predare.IDBP, OI_Predare.IDC_BP, OI_Predare.IdMasina, Masini.Masina, Clienti.Denumire, OI_Predare.NRBP, OI_Predare.DATABP, OI_Predare.CodProdus, OI_Predare.DenumireProdus, OI_Predare.UM, OI_Predare.Cant, OI_Predare.PUTVA, OI_Predare.ValoareCUTVA, OI_Predare.Expl FROM Masini INNER JOIN (Clienti INNER JOIN OI_Predare ON Clienti.IDC = OI_Predare.IDC_BP) ON Masini.IdMasina = OI_Predare.IdMas …`
- sectiuni: 5 · controale: 41

### Trasabilitate schita raport OI_Bon_BS

Sursa: [Reports/OI_Bon_BS.txt](../../VERCON_ACCESS/Reports/OI_Bon_BS.txt) · imagine: [R_OI_Bon_BS.png](../R_OI_Bon_BS.png)

- Sursa de date (RecordSource): `SELECT OI_Casare.IDBC, OI_Casare.IDC_BC, OI_Casare.IdMasina, Masini.Masina, Clienti.Denumire, OI_Casare.NRBC, OI_Casare.DATABC, OI_Casare.CodProdus, OI_Casare.DenumireProdus, OI_Casare.UM, OI_Casare.Cant, OI_Casare.PUTVA, OI_Casare.ValoareCUTVA, OI_Casare.Expl FROM Masini INNER JOIN (Clienti INNER JOIN OI_Casare ON Clienti.IDC = OI_Casare.IDC_BC) ON Masini.IdMasina = OI_Casare.IdMasina WHERE (((OI …`
- sectiuni: 5 · controale: 41

### Trasabilitate schita raport OI_Bon_BT

Sursa: [Reports/OI_Bon_BT.txt](../../VERCON_ACCESS/Reports/OI_Bon_BT.txt) · imagine: [R_OI_Bon_BT.png](../R_OI_Bon_BT.png)

- Sursa de date (RecordSource): `SELECT OI_Transfer.IDBT, OI_Transfer.IDC_BT, OI_Transfer.IDC_OLD, OI_Transfer.IdMasina, Masini.Masina, C1.Denumire AS Primitor, C2.Denumire AS Predator, OI_Transfer.NRBT, OI_Transfer.DATABT, OI_Transfer.CodProdus, OI_Transfer.DenumireProdus, OI_Transfer.UM, OI_Transfer.Cant, OI_Transfer.PUTVA, OI_Transfer.ValoareCUTVA, OI_Transfer.Expl FROM Masini INNER JOIN (Clienti AS C2 INNER JOIN (Clienti AS C …`
- sectiuni: 5 · controale: 41

### Trasabilitate schita raport MF_Bon_BP

Sursa: [Reports/MF_Bon_BP.txt](../../VERCON_ACCESS/Reports/MF_Bon_BP.txt) · imagine: [R_MF_Bon_BP.png](../R_MF_Bon_BP.png)

- Sursa de date (RecordSource): `SELECT OI_Predare.IDBP, OI_Predare.IDC_BP, OI_Predare.IdMasina, Masini.Masina, Clienti.Denumire, OI_Predare.NRBP, OI_Predare.DATABP, ProduseP.COD, Produse.DenumireP, Produse.UM, OI_Predare.Cant, OI_Predare.PUTVA, OI_Predare.ValoareCUTVA, OI_Predare.Expl, Produse.CC FROM Produse INNER JOIN ((Masini INNER JOIN (Clienti INNER JOIN OI_Predare ON Clienti.IDC = OI_Predare.IDC_BP) ON Masini.IdMasina = OI …`
- sectiuni: 6 · controale: 45

### Trasabilitate schita raport MF_Bon_BS

Sursa: [Reports/MF_Bon_BS.txt](../../VERCON_ACCESS/Reports/MF_Bon_BS.txt) · imagine: [R_MF_Bon_BS.png](../R_MF_Bon_BS.png)

- Sursa de date (RecordSource): identică fragmentului din [OI_Bon_BS](#trasabilitate-schita-raport-oi_bon_bs).
- sectiuni: 5 · controale: 41

### Trasabilitate schita raport MF_Bon_BT

Sursa: [Reports/MF_Bon_BT.txt](../../VERCON_ACCESS/Reports/MF_Bon_BT.txt) · imagine: [R_MF_Bon_BT.png](../R_MF_Bon_BT.png)

- Sursa de date (RecordSource): identică fragmentului din [OI_Bon_BT](#trasabilitate-schita-raport-oi_bon_bt).
- sectiuni: 5 · controale: 41

### Trasabilitate schita raport OI_Predare

Sursa: [Reports/OI_Predare.txt](../../VERCON_ACCESS/Reports/OI_Predare.txt) · imagine: [R_OI_Predare.png](../R_OI_Predare.png)

- Sursa de date (RecordSource): `SELECT OI_Predare.IDBP, OI_Predare.IDC_BP, OI_Predare.IdMasina, Masini.Masina, Clienti.Denumire, OI_Predare.NRBP, OI_Predare.DATABP, OI_Predare.idprodus AS CodProdus, Produse.denumirep AS DenumireProdus, Produse.UM, OI_Predare.Cant, OI_Predare.PUTVA, OI_Predare.ValoareCUTVA, OI_Predare.Expl FROM Produse INNER JOIN ((Clienti INNER JOIN (Masini INNER JOIN OI_Predare ON Masini.IdMasina = OI_Predare.I …`
- sectiuni: 5 · controale: 41

### Trasabilitate schita raport OI_Casare

Sursa: [Reports/OI_Casare.txt](../../VERCON_ACCESS/Reports/OI_Casare.txt) · imagine: [R_OI_Casare.png](../R_OI_Casare.png)

- Sursa de date (RecordSource): `SELECT OI_Casare.IDBC, OI_Casare.IDC_BC, OI_Casare.IdMasina, Masini.Masina, Clienti.Denumire, OI_Casare.NRBC, OI_Casare.DATABC, OI_Casare.idprodus AS CodProdus, Produse.denumirep AS DenumireProdus, Produse.UM, OI_Casare.Cant, OI_Casare.PUTVA, OI_Casare.ValoareCUTVA, OI_Casare.Expl FROM (Clienti INNER JOIN (Masini INNER JOIN OI_Casare ON Masini.IdMasina = OI_Casare.IdMasina) ON Clienti.IDC = OI_Cas …`
- sectiuni: 5 · controale: 41

### Trasabilitate schita raport OI_Transfer

Sursa: [Reports/OI_Transfer.txt](../../VERCON_ACCESS/Reports/OI_Transfer.txt) · imagine: [R_OI_Transfer.png](../R_OI_Transfer.png)

- Sursa de date (RecordSource): `SELECT OI_Transfer.IDBT, OI_Transfer.IDC_BT, OI_Transfer.IDC_OLD, OI_Transfer.IdMasina, Masini.Masina, C1.Denumire AS Primitor, C2.Denumire AS Predator, OI_Transfer.NRBT, OI_Transfer.DATABT, OI_Transfer.idprodus AS CodProdus, Produse.denumirep AS DenumireProdus, Produse.UM, OI_Transfer.Cant, OI_Transfer.PUTVA, OI_Transfer.ValoareCUTVA, OI_Transfer.Expl FROM Produse INNER JOIN ((Masini INNER JOIN ( …`
- sectiuni: 5 · controale: 41

## Secțiuni

| Secțiune | OI_Bon_BP | OI_Bon_BS | OI_Bon_BT | MF_Bon_BP | MF_Bon_BS | MF_Bon_BT | OI_Predare | OI_Casare | OI_Transfer |
|---|---|---|---|---|---|---|---|---|---|
| PageHeaderSection | 1 / 0 / 154 | 1 / 0 / 153 | 1 / 0 / 155 | 1 / 0 / 159 | 1 / 0 / 153 | 1 / 0 / 155 | 1 / 0 / 156 | 1 / 0 / 155 | 1 / 0 / 157 |
| GroupHeader0 | 2 / 1974 / 161 | 2 / 1974 / 160 | 2 / 1974 / 162 | 2 / 1974 / 166 | 2 / 1974 / 160 | 2 / 1974 / 162 | 2 / 1974 / 163 | 2 / 1974 / 162 | 2 / 1974 / 164 |
| Detail | 3 / 285 / 768 | 3 / 285 / 767 | 3 / 285 / 771 | 3 / 285 / 771 | 3 / 285 / 767 | 3 / 285 / 771 | 3 / 285 / 770 | 3 / 285 / 769 | 3 / 285 / 773 |
| GroupFooter2 | — | — | — | 4 / 285 / 954 | — | — | — | — | — |
| GroupFooter1 | 4 / 1731 / 951 | 4 / 1134 / 951 | 4 / 1134 / 954 | 5 / 1725 / 1063 | 4 / 1725 / 951 | 4 / 1725 / 954 | 4 / 1725 / 952 | 4 / 1134 / 953 | 4 / 1725 / 955 |
| PageFooterSection | 5 / 0 / 1184 | 5 / 477 / 1161 | 5 / 477 / 1164 | 6 / 0 / 1297 | 5 / 0 / 1185 | 5 / 0 / 1188 | 5 / 0 / 1186 | 5 / 477 / 1163 | 5 / 0 / 1189 |

## Controale

### PageHeaderSection <a id="pageheadersection-inaltime-0-twips-linia-154"></a><a id="pageheadersection-inaltime-0-twips-linia-153"></a><a id="pageheadersection-inaltime-0-twips-linia-155"></a><a id="pageheadersection-inaltime-0-twips-linia-159"></a><a id="pageheadersection-inaltime-0-twips-linia-156"></a><a id="pageheadersection-inaltime-0-twips-linia-157"></a>

_fara controale_

### GroupHeader0 <a id="groupheader0-inaltime-1974-twips-linia-161"></a><a id="groupheader0-inaltime-1974-twips-linia-160"></a><a id="groupheader0-inaltime-1974-twips-linia-162"></a><a id="groupheader0-inaltime-1974-twips-linia-166"></a><a id="groupheader0-inaltime-1974-twips-linia-163"></a><a id="groupheader0-inaltime-1974-twips-linia-164"></a>

| Control | Tip | Text / camp / sursa | Pozitie (twips) | OI_Bon_BP | OI_Bon_BS | OI_Bon_BT | MF_Bon_BP | MF_Bon_BS | MF_Bon_BT | OI_Predare | OI_Casare | OI_Transfer |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Text0 | TextBox | =DLookUp("Firma","caleTemp") | 0,0 8133x510 | 1:169 | 1:168 | 1:170 | 1:174 | 1:168 | 1:170 | 1:171 | 1:170 | 1:172 |
| Label5 | Label | Data | 1251,510 2214x283 | 2:191 | 2:190 | 2:192 | 2:196 | 2:190 | 2:192 | 2:193 | 2:192 | 2:194 |
| Ziua | TextBox | =Day([DATABP]) | 1251,1077 737x285 | 3:210 | — | — | 3:215 | — | — | 3:212 | — | — |
| Ziua | TextBox | =Day([DATABC]) | 1251,1077 737x285 | — | 3:209 | — | — | 3:209 | — | — | 3:211 | — |
| Ziua | TextBox | =Day([DATABT]) | 1251,1077 737x285 | — | — | 3:211 | — | — | 3:211 | — | — | 3:213 |
| Label6 | Label | Ziua | 1251,793 737x284 | 4:235 | 4:234 | 4:236 | 4:240 | 4:234 | 4:236 | 4:237 | 4:236 | 4:238 |
| Text10 | TextBox | =Month([DATABP]) | 1988,1077 737x285 | 5:260 | — | — | 5:265 | — | — | 5:262 | — | — |
| Text10 | TextBox | =Month([DATABC]) | 1988,1077 737x285 | — | 5:259 | — | — | 5:259 | — | — | 5:261 | — |
| Text10 | TextBox | =Month([DATABT]) | 1988,1077 737x285 | — | — | 5:261 | — | — | 5:261 | — | — | 5:263 |
| Label7 | Label | Luna | 1988,793 737x284 | 6:285 | 6:284 | 6:286 | 6:290 | 6:284 | 6:286 | 6:287 | 6:286 | 6:288 |
| Text11 | TextBox | =Year([DATABP]) | 2725,1077 737x285 | 7:310 | — | — | 7:315 | — | — | 7:312 | — | — |
| Text11 | TextBox | =Year([DATABC]) | 2725,1077 737x285 | — | 7:309 | — | — | 7:309 | — | — | 7:311 | — |
| Text11 | TextBox | =Year([DATABT]) | 2725,1077 737x285 | — | — | 7:311 | — | — | 7:311 | — | — | 7:313 |
| Label8 | Label | Anul | 2725,793 737x284 | 8:335 | 8:334 | 8:336 | 8:340 | 8:334 | 8:336 | 8:337 | 8:336 | 8:338 |
| Label14 | Label | Nr. comandă | 6948,792 1185x285 | 9:360 | 9:359 | 9:361 | 9:365 | 9:359 | 9:361 | 9:362 | 9:361 | 9:363 |
| Text16 | TextBox | ="Magazie" | 3462,1077 1755x285 | 10:383 | — | — | 10:388 | — | — | 10:385 | — | — |
| Predator | TextBox | Denumire | 3462,1077 1755x285 | — | 10:382 | — | — | 10:382 | — | — | 10:384 | — |
| Text16 | TextBox | Predator | 3462,1077 1755x285 | — | — | 10:384 | — | — | 10:384 | — | — | 10:386 |
| Label12 | Label | Predător | 3462,793 1755x285 | 11:408 | 11:407 | 11:409 | 11:412 | 11:407 | 11:409 | 11:410 | 11:409 | 11:411 |
| Denumire | TextBox | Denumire | 5217,1077 1734x285 | 12:433 | — | — | 12:437 | — | — | 12:435 | — | — |
| Primitor | TextBox | ="Casare" | 5217,1077 1734x285 | — | 12:432 | — | — | 12:432 | — | — | 12:434 | — |
| Denumire | TextBox | Primitor | 5217,1077 1734x285 | — | — | 12:434 | — | — | 12:434 | — | — | 12:436 |
| Label13 | Label | Primitor | 5217,792 1734x285 | 13:458 | 13:457 | 13:459 | 13:461 | 13:457 | 13:459 | 13:460 | 13:459 | 13:461 |
| Label19 | Label | BON DE PREDARE | 8133,0 2616x1372 | 14:483 | — | — | 14:486 | — | — | 14:485 | — | — |
| Label19 | Label | BON DE CASARE | 8133,0 2616x1372 | — | 14:482 | — | — | 14:482 | — | — | 14:484 | — |
| Label19 | Label | BON DE TRANSFER | 8133,0 2616x1372 | — | — | 14:484 | — | — | 14:484 | — | — | 14:486 |
| Label20 | Label | Nr. crt. | 0,1464 630x510 | 15:505 | 15:504 | 15:508 | 15:508 | 15:504 | 15:508 | 15:507 | 15:506 | 15:510 |
| Label21 | Label | Denumirea materialelor (inclusiv sort, marca, profil, dimensiune) | 630,1464 4125x510 | 16:527 | 16:526 | 16:530 | 16:530 | 16:526 | 16:530 | 16:529 | 16:528 | 16:532 |
| Label23 | Label | Cod | 4755,1464 1485x510 | 17:551 | 17:550 | 17:554 | 17:554 | 17:550 | 17:554 | 17:553 | 17:552 | 17:556 |
| Label24 | Label | U/M | 6240,1464 711x510 | 18:575 | 18:574 | 18:578 | 18:578 | 18:574 | 18:578 | 18:577 | 18:576 | 18:580 |
| Label25 | Label | Cantitatea efectivă | 6951,1464 1185x510 | 19:599 | 19:598 | 19:602 | 19:602 | 19:598 | 19:602 | 19:601 | 19:600 | 19:604 |
| Label26 | Label | Preț unitar | 8136,1464 1185x510 | 20:623 | 20:622 | 20:626 | 20:626 | 20:622 | 20:626 | 20:625 | 20:624 | 20:628 |
| Label27 | Label | Valoarea | 9321,1464 1425x510 | 21:647 | 21:646 | 21:650 | 21:650 | 21:646 | 21:650 | 21:649 | 21:648 | 21:652 |
| Text47 | TextBox | Masina | 3465,510 4671x282 | 22:671 | 22:670 | 22:674 | 22:674 | 22:670 | 22:674 | 22:673 | 22:672 | 22:676 |
| NRBP | TextBox | NRBP | 0,1080 1247x285 | 23:697 | — | — | 23:700 | — | — | 23:699 | — | — |
| NRBP | TextBox | NRBC | 0,1080 1247x285 | — | 23:696 | — | — | 23:696 | — | — | 23:698 | — |
| NRBP | TextBox | NRBT | 0,1080 1247x285 | — | — | 23:700 | — | — | 23:700 | — | — | 23:702 |
| Label3 | Label | Număr document | 0,510 1247x565 | 24:720 | 24:719 | 24:723 | 24:723 | 24:719 | 24:723 | 24:722 | 24:721 | 24:725 |
| Label49 | Label |  | 6948,1077 1185x285 | 25:744 | 25:743 | 25:747 | 25:747 | 25:743 | 25:747 | 25:746 | 25:745 | 25:749 |

### Detail <a id="detail-inaltime-285-twips-linia-768"></a><a id="detail-inaltime-285-twips-linia-767"></a><a id="detail-inaltime-285-twips-linia-771"></a><a id="detail-inaltime-285-twips-linia-770"></a><a id="detail-inaltime-285-twips-linia-769"></a><a id="detail-inaltime-285-twips-linia-773"></a>

| Control | Tip | Text / camp / sursa | Pozitie (twips) | OI_Bon_BP | OI_Bon_BS | OI_Bon_BT | MF_Bon_BP | MF_Bon_BS | MF_Bon_BT | OI_Predare | OI_Casare | OI_Transfer |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| ValoareCUTVA | TextBox | ValoareCUTVA | 9312,0 1437x285 | 1:776 | 1:775 | 1:779 | 1:779 | 1:775 | 1:779 | 1:778 | 1:777 | 1:781 |
| PUTVA | TextBox | PUTVA | 8127,0 1185x285 | 2:801 | 2:800 | 2:804 | 2:804 | 2:800 | 2:804 | 2:803 | 2:802 | 2:806 |
| Text35 | TextBox | Cant | 6942,0 1185x285 | 3:827 | 3:826 | 3:830 | 3:830 | 3:826 | 3:830 | 3:829 | 3:828 | 3:832 |
| UM | TextBox | UM | 6231,0 711x285 | 4:853 | 4:852 | 4:856 | 4:856 | 4:852 | 4:856 | 4:855 | 4:854 | 4:858 |
| CodProdusx | TextBox | =Left([CodProdus],10) | 4755,0 1476x285 | 5:876 | 5:875 | 5:879 | — | 5:875 | 5:879 | 5:878 | 5:877 | 5:881 |
| CodProdusx | TextBox | COD | 4755,0 1476x285 | — | — | — | 5:879 | — | — | — | — | — |
| Produse.Denumire | TextBox | DenumireProdus | 630,0 4125x285 | 6:899 | 6:898 | 6:902 | — | 6:898 | 6:902 | 6:901 | 6:900 | 6:904 |
| Produse.Denumire | TextBox | DenumireP | 630,0 4125x285 | — | — | — | 6:902 | — | — | — | — | — |
| Text29 | TextBox | =1 | 0,0 630x285 | 7:926 | 7:925 | 7:929 | 7:929 | 7:925 | 7:929 | 7:928 | 7:927 | 7:931 |

### GroupFooter2 <a id="groupfooter2-inaltime-285-twips-linia-954"></a>

| Control | Tip | Text / camp / sursa | Pozitie (twips) | OI_Bon_BP | OI_Bon_BS | OI_Bon_BT | MF_Bon_BP | MF_Bon_BS | MF_Bon_BT | OI_Predare | OI_Casare | OI_Transfer |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Text54 | TextBox | ="TOTAL CLASIFICATIA " & [cc] | 630,0 4125x285 | — | — | — | 1:968 | — | — | — | — | — |
| Text55 | TextBox | =Sum([ValoareCUTVA]) | 9312,0 1437x285 | — | — | — | 2:991 | — | — | — | — | — |
| Text56 | TextBox |  | 0,0 630x285 | — | — | — | 3:1018 | — | — | — | — | — |
| Text57 | TextBox |  | 4755,0 4560x285 | — | — | — | 4:1039 | — | — | — | — | — |

### GroupFooter1 <a id="groupfooter1-inaltime-1731-twips-linia-951"></a><a id="groupfooter1-inaltime-1134-twips-linia-951"></a><a id="groupfooter1-inaltime-1134-twips-linia-954"></a><a id="groupfooter1-inaltime-1725-twips-linia-1063"></a><a id="groupfooter1-inaltime-1725-twips-linia-951"></a><a id="groupfooter1-inaltime-1725-twips-linia-954"></a><a id="groupfooter1-inaltime-1725-twips-linia-952"></a><a id="groupfooter1-inaltime-1134-twips-linia-953"></a><a id="groupfooter1-inaltime-1725-twips-linia-955"></a>

| Control | Tip | Text / camp / sursa | Pozitie (twips) | OI_Bon_BP | OI_Bon_BS | OI_Bon_BT | MF_Bon_BP | MF_Bon_BS | MF_Bon_BT | OI_Predare | OI_Casare | OI_Transfer |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Text43 | TextBox | ="TOTAL BON" | 630,0 4125x285 | 1:960 | 1:960 | 1:963 | 1:1072 | 1:960 | 1:963 | 1:961 | 1:962 | 1:964 |
| Text46 | TextBox | =Sum([ValoareCUTVA]) | 9312,0 1437x285 | 2:984 | 2:984 | 2:987 | 2:1096 | 2:984 | 2:987 | 2:985 | 2:986 | 2:988 |
| Text38 | TextBox | ="<b>Data și semnătura:</b><BR>" & [DATABP] | 0,624 2721x510 | 3:1012 | — | — | 3:1124 | — | — | 3:1013 | — | — |
| Text38 | TextBox | ="<b>Data și semnătura:</b><BR>" & [DATABC] | 0,624 2721x510 | — | 3:1012 | — | — | 3:1012 | — | — | 3:1014 | — |
| Text38 | TextBox | ="<b>Data și semnătura:</b><BR>" & [DATABT] | 0,624 2721x510 | — | — | 3:1015 | — | — | 3:1015 | — | — | 3:1016 |
| Text39 | TextBox | ="Viza C.T.C. (propus mișcarea)" | 2721,624 2826x510 | 4:1036 | — | — | 4:1148 | — | — | 4:1037 | — | — |
| Text39 | TextBox | ="Viza C.T.C." | 2721,624 2826x510 | — | 4:1036 | 4:1039 | — | 4:1036 | 4:1039 | — | 4:1038 | 4:1040 |
| Text40 | TextBox | ="Predător," | 5547,624 2601x510 | 5:1062 | — | — | 5:1174 | — | — | 5:1063 | — | — |
| Text40 | TextBox | ="Predător" | 5547,624 2601x510 | — | 5:1062 | 5:1065 | — | 5:1062 | 5:1065 | — | 5:1064 | 5:1066 |
| Text41 | TextBox | ="Primitor" | 8148,624 2601x510 | 6:1088 | 6:1088 | 6:1091 | 6:1200 | 6:1088 | 6:1091 | 6:1089 | 6:1090 | 6:1092 |
| Text51 | TextBox |  | 0,0 630x285 | 7:1114 | 7:1114 | 7:1117 | 8:1250 | 8:1138 | 8:1141 | 8:1139 | 7:1116 | 8:1142 |
| Label107 | TextBox | =DLookUp("expl","ver_rap") | 0,1248 2736x477 | — | — | — | 7:1226 | 7:1114 | 7:1117 | 7:1115 | — | 7:1118 |
| Text52 | TextBox |  | 4755,0 4560x285 | 8:1136 | 8:1136 | 8:1139 | 9:1272 | 9:1160 | 9:1163 | 9:1161 | 8:1138 | 9:1164 |
| Label107 | TextBox | =DLookUp("expl","ver_rap") | 0,1254 2736x477 | 9:1159 | — | — | — | — | — | — | — | — |

### PageFooterSection <a id="pagefootersection-inaltime-0-twips-linia-1184"></a><a id="pagefootersection-inaltime-477-twips-linia-1161"></a><a id="pagefootersection-inaltime-477-twips-linia-1164"></a><a id="pagefootersection-inaltime-0-twips-linia-1297"></a><a id="pagefootersection-inaltime-0-twips-linia-1185"></a><a id="pagefootersection-inaltime-0-twips-linia-1188"></a><a id="pagefootersection-inaltime-0-twips-linia-1186"></a><a id="pagefootersection-inaltime-477-twips-linia-1163"></a><a id="pagefootersection-inaltime-0-twips-linia-1189"></a>

| Control | Tip | Text / camp / sursa | Pozitie (twips) | OI_Bon_BP | OI_Bon_BS | OI_Bon_BT | MF_Bon_BP | MF_Bon_BS | MF_Bon_BT | OI_Predare | OI_Casare | OI_Transfer |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Label107 | TextBox | =DLookUp("expl","ver_rap") | 0,0 2736x477 | — | 1:1168 | 1:1171 | — | — | — | — | 1:1170 | — |
