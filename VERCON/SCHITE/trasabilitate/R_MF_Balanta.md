# Trasabilitate schite balanțe MF, OI și produse

Convenții și limite: [citirea fișelor consolidate](../INDEX.md#fise-consolidate). SLICE-D003-09, 27.10.2026.

Coloana Evenimente era goală la fiecare control.

## Variante și surse

### Trasabilitate schita raport MF_Balanta

Sursa: [Reports/MF_Balanta.txt](../../VERCON_ACCESS/Reports/MF_Balanta.txt) · imagine: [R_MF_Balanta.png](../R_MF_Balanta.png)

- Titlu (Caption): Balanta produse
- Sursa de date (RecordSource): `MF_Balanta`
- Latime: 14380 twips · sectiuni: 9 · controale: 53

### Trasabilitate schita raport MF_Balanta_Subinv

Sursa: [Reports/MF_Balanta_Subinv.txt](../../VERCON_ACCESS/Reports/MF_Balanta_Subinv.txt) · imagine: [R_MF_Balanta_Subinv.png](../R_MF_Balanta_Subinv.png)

- Titlu (Caption): Balanta produse
- Sursa de date (RecordSource): identică fragmentului din [MF_Balanta](#trasabilitate-schita-raport-mf_balanta).
- Latime: 14380 twips · sectiuni: 10 · controale: 53

### Trasabilitate schita raport OI_Balanta

Sursa: [Reports/OI_Balanta.txt](../../VERCON_ACCESS/Reports/OI_Balanta.txt) · imagine: [R_OI_Balanta.png](../R_OI_Balanta.png)

- Titlu (Caption): Balanta produse
- Sursa de date (RecordSource): `OI_Balanta`
- Latime: 14376 twips · sectiuni: 7 · controale: 45

### Trasabilitate schita raport Balanta_produse

Sursa: [Reports/Balanta_produse.txt](../../VERCON_ACCESS/Reports/Balanta_produse.txt) · imagine: [R_Balanta_produse.png](../R_Balanta_produse.png)

- Titlu (Caption): Balanta produse
- Sursa de date (RecordSource): `Balanta22`
- Latime: 14376 twips · sectiuni: 7 · controale: 52

## Secțiuni

| Secțiune | MF_Balanta | MF_Balanta_Subinv | OI_Balanta | Balanta_produse |
|---|---|---|---|---|
| ReportHeader | 1 / 459 / 224 | 1 / 459 / 230 | 1 / 459 / 216 | 1 / 459 / 214 |
| PageHeaderSection | 2 / 627 / 342 | 2 / 627 / 348 | 2 / 627 / 334 | 2 / 1005 / 332 |
| GroupHeader0 | 3 / 297 / 626 | 3 / 297 / 632 | 3 / 297 / 618 | 3 / 297 / 660 |
| GroupHeader1 | 4 / 285 / 660 | 4 / 285 / 667 | — | — |
| Detail | 5 / 285 / 699 | 5 / 285 / 706 | 4 / 285 / 652 | 4 / 300 / 694 |
| GroupFooter2 | 6 / 297 / 1066 | 6 / 297 / 1073 | — | — |
| GroupFooter1 | 7 / 288 / 1239 | 7 / 491 / 1246 | 5 / 288 / 1018 | 5 / 288 / 1123 |
| GroupFooter3 | — | 8 / 0 / 1397 | — | — |
| PageFooterSection | 8 / 501 / 1390 | 9 / 501 / 1411 | 6 / 501 / 1169 | 6 / 501 / 1277 |
| ReportFooter | 9 / 528 / 1454 | 10 / 528 / 1475 | 7 / 528 / 1230 | 7 / 443 / 1341 |

## Controale

### ReportHeader <a id="reportheader-inaltime-459-twips-linia-224"></a><a id="reportheader-inaltime-459-twips-linia-230"></a><a id="reportheader-inaltime-459-twips-linia-216"></a><a id="reportheader-inaltime-459-twips-linia-214"></a>

| Control | Tip | Text / camp / sursa | Pozitie (twips) | MF_Balanta | MF_Balanta_Subinv | OI_Balanta | Balanta_produse |
|---|---|---|---|---|---|---|---|
| Label44 | Label | Balanţă Mijloace FIXE | 96,0 6189x459 | 1:232 | 1:238 | — | — |
| Label44 | Label | Balanţă Obiecte Inventar | 96,0 6189x459 | — | — | 1:224 | — |
| Label44 | Label | Balanţă stocuri produse | 96,0 6189x459 | — | — | — | 1:222 |
| Text71 | TextBox | =[Forms]![sfp]![di] | 7986,0 2811x459 | 2:251 | 2:257 | 2:243 | 2:241 |
| Label72 | Label | De la | 6453,0 1533x459 | 3:273 | 3:279 | 3:265 | 3:263 |
| Text73 | TextBox | =[Forms]![sfp]![dsf] | 11562,0 2811x459 | 4:295 | 4:301 | 4:287 | 4:285 |
| Label74 | Label | la | 10797,0 765x459 | 5:318 | 5:324 | 5:310 | 5:308 |

### PageHeaderSection <a id="pageheadersection-inaltime-627-twips-linia-342"></a><a id="pageheadersection-inaltime-627-twips-linia-348"></a><a id="pageheadersection-inaltime-627-twips-linia-334"></a><a id="pageheadersection-inaltime-1005-twips-linia-332"></a>

| Control | Tip | Text / camp / sursa | Pozitie comună (twips) | MF_Balanta | MF_Balanta_Subinv | OI_Balanta | Balanta_produse |
|---|---|---|---|---|---|---|---|
| Denumire_Label | Label | Denumire | 72,120 3108x495 | 1:349 | 1:355 | 1:341 | 1:339 / 75,510 3108x495 |
| UM_Label | Label | Nr. Inv. | 3180,120 798x495 | 2:374 | 2:380 | — | — |
| Cantitate Initiala_Label | Label | Cant. Iniţiala | 5133,120 1155x495 | 3:399 | 3:405 | 3:391 | — |
| Valoare Initiala_Label | Label | Valoare Iniţiala | 6288,120 1155x495 | 4:424 | 4:430 | 4:416 | — |
| Cantitate Intrata_Label | Label | Cant. Intrată | 7443,120 1155x495 | 5:449 | 5:455 | 5:441 | — |
| Valoare Intrata_Label | Label | Valoare Intrată | 8598,120 1155x495 | 6:474 | 6:480 | 6:466 | — |
| Cantiate Iesita_Label | Label | Cant. Ieşita | 9753,120 1155x495 | 7:499 | 7:505 | 7:491 | — |
| Valoare Iesita_Label | Label | Valoare Ieşita | 10908,120 1155x495 | 8:524 | 8:530 | 8:516 | — |
| Cantitate Finala_Label | Label | Cant. Finală | 12063,120 1155x495 | 9:549 | 9:555 | 9:541 | — |
| Valoare Finala_Label | Label | Valoare Finală | 13218,120 1155x495 | 10:574 | 10:580 | 10:566 | — |
| Label89 | Label | PREŢ | 3978,120 1155x495 | 11:599 | 11:605 | 11:591 | 11:581 / 3981,510 1155x495 |
| UM_Label | Label | UM | 3180,120 798x495 | — | — | 2:366 | 2:364 / 3183,510 798x495 |
| Cantitate_Initiala_Label | Label | Cant. Iniţiala | 5136,510 1155x495 | — | — | — | 3:389 |
| Valoare_Initiala_Label | Label | Valoare Iniţiala | 6291,510 1155x495 | — | — | — | 4:413 |
| Cantitate_Intrata_Label | Label | Cant. Intrată | 7446,510 1155x495 | — | — | — | 5:437 |
| Valoare_Intrata_Label | Label | Valoare Intrată | 8601,510 1155x495 | — | — | — | 6:461 |
| Cantiate_Iesita_Label | Label | Cant. Ieşita | 9756,510 1155x495 | — | — | — | 7:485 |
| Valoare_Iesita_Label | Label | Valoare Ieşita | 10911,510 1155x495 | — | — | — | 8:509 |
| Cantitate_Finala_Label | Label | Cant. Finală | 12066,510 1155x495 | — | — | — | 9:533 |
| Valoare_Finala_Label | Label | Valoare Finală | 13221,510 1155x495 | — | — | — | 10:557 |
| Text0 | TextBox | =DLookUp("FIrma","CaleTemp") | 90,30 5229x343 | — | — | — | 12:606 |
| Text53 | TextBox | =DLookUp("Detalii","CaleTemp") | 9120,30 5229x343 | — | — | — | 13:631 |

### GroupHeader0 <a id="groupheader0-inaltime-297-twips-linia-626"></a><a id="groupheader0-inaltime-297-twips-linia-632"></a><a id="groupheader0-inaltime-297-twips-linia-618"></a><a id="groupheader0-inaltime-297-twips-linia-660"></a>

| Control | Tip | Text / camp / sursa | Pozitie (twips) | MF_Balanta | MF_Balanta_Subinv | OI_Balanta | Balanta_produse |
|---|---|---|---|---|---|---|---|
| Text108 | TextBox | ="Categorie: " & [masina] | 75,0 14298x285 | 1:636 | — | 1:628 | 1:670 |
| Text108 | TextBox | ="PRIMITOR: " & [Denumire] & " / " & [Adresa] | 75,0 14298x285 | — | 1:643 | — | — |

### GroupHeader1 <a id="groupheader1-inaltime-285-twips-linia-660"></a><a id="groupheader1-inaltime-285-twips-linia-667"></a>

| Control | Tip | Text / camp / sursa | Pozitie (twips) | MF_Balanta | MF_Balanta_Subinv | OI_Balanta | Balanta_produse |
|---|---|---|---|---|---|---|---|
| Text120 | TextBox | ="CLASIFICARE: " & [CC] | 75,0 14298x285 | 1:675 | 1:682 | — | — |

### Detail <a id="detail-inaltime-285-twips-linia-699"></a><a id="detail-inaltime-285-twips-linia-706"></a><a id="detail-inaltime-285-twips-linia-652"></a><a id="detail-inaltime-300-twips-linia-694"></a>

| Control | Tip | Text / camp / sursa | Pozitie (twips) | MF_Balanta | MF_Balanta_Subinv | OI_Balanta | Balanta_produse |
|---|---|---|---|---|---|---|---|
| Denumire | TextBox | DENP | 72,0 3108x285 | 1:707 | — | 1:660 | 1:702 |
| DENP | TextBox | DENP | 72,0 3108x285 | — | 1:714 | — | — |
| COD | TextBox | COD | 3180,0 798x285 | 2:730 | 2:737 | — | — |
| UM | TextBox | UMP | 3180,0 798x285 | — | — | 2:683 | 2:725 |
| Cantitate Initiala | TextBox | CI | 5133,0 1155x285 | 3:754 | 3:761 | 3:706 | — |
| Cantitate_Initiala | TextBox | CI | 5133,0 1155x285 | — | — | — | 3:748 |
| Valoare Initiala | TextBox | VI | 6288,0 1155x285 | 4:781 | 4:788 | 4:733 | — |
| Valoare_Initiala | TextBox | VI | 6288,0 1155x285 | — | — | — | 4:775 |
| Cantitate Intrata | TextBox | CCIn | 7443,0 1155x285 | 5:808 | 5:815 | 5:760 | — |
| Cantitate_Intrata | TextBox | CCIn | 7443,0 1155x285 | — | — | — | 5:802 |
| Valoare Intrata | TextBox | VCIn | 8598,0 1155x285 | 6:835 | 6:842 | 6:787 | — |
| Valoare_Intrata | TextBox | VCIn | 8598,0 1155x285 | — | — | — | 6:829 |
| Cantiate Iesita | TextBox | CCOut | 9753,0 1155x285 | 7:862 | 7:869 | 7:814 | — |
| Cantiate_Iesita | TextBox | CCOut | 9753,0 1155x285 | — | — | — | 7:856 |
| Valoare Iesita | TextBox | VCOut | 10908,0 1155x285 | 8:889 | 8:896 | 8:841 | — |
| Valoare_Iesita | TextBox | VCOut | 10908,0 1155x285 | — | — | — | 8:883 |
| Cantitate Finala | TextBox | CF | 12063,0 1155x285 | 9:916 | 9:923 | 9:868 | — |
| Cantitate_Finala | TextBox | CF | 12063,0 1155x285 | — | — | — | 9:910 |
| Valoare Finala | TextBox | VF | 13218,0 1155x285 | 10:953 | 10:960 | 10:905 | — |
| Valoare_Finala | TextBox | VF | 13218,0 1155x285 | — | — | — | 10:947 |
| Pret | TextBox | PT | 3978,0 1155x285 | 11:990 | 11:997 | 11:942 | 11:984 |
| Line115 | Line |  | 72,0 0x285 | 12:1016 | 12:1023 | 12:968 | 12:1011 |
| Line116 | Line |  | 14373,0 0x285 | 13:1028 | 13:1035 | 13:980 | 13:1023 |
| Line118 | Line |  | 3978,0 0x285 | 14:1040 | 14:1047 | 14:992 | 14:1035 |
| Line119 | Line |  | 5133,0 0x285 | 15:1052 | 15:1059 | 15:1004 | 15:1047 |
| Line120 | Line |  | 72,285 14301x0 | — | — | — | 16:1059 |
| Line121 | Line |  | 7443,0 0x285 | — | — | — | 17:1073 |
| Line122 | Line |  | 9753,0 0x285 | — | — | — | 18:1085 |
| Line123 | Line |  | 12063,0 0x285 | — | — | — | 19:1097 |
| Line124 | Line |  | 14373,0 0x285 | — | — | — | 20:1109 |

### GroupFooter2 <a id="groupfooter2-inaltime-297-twips-linia-1066"></a><a id="groupfooter2-inaltime-297-twips-linia-1073"></a>

| Control | Tip | Text / camp / sursa | Pozitie (twips) | MF_Balanta | MF_Balanta_Subinv | OI_Balanta | Balanta_produse |
|---|---|---|---|---|---|---|---|
| Text121 | TextBox | =Sum([VI]) | 5133,0 2307x285 | 1:1077 | 1:1084 | — | — |
| Text122 | TextBox | =Sum([VCIn]) | 7440,0 2307x285 | 2:1104 | 2:1111 | — | — |
| Text123 | TextBox | =Sum([VCOut]) | 9747,0 2307x285 | 3:1132 | 3:1139 | — | — |
| Text124 | TextBox | =Sum([VF]) | 12061,0 2319x285 | 4:1160 | 4:1167 | — | — |
| Text125 | TextBox | ="TOTAL CLASIFICAREA: " & [CC] | 69,0 5064x285 | 5:1188 | 5:1195 | — | — |
| Line126 | Line |  | 69,285 14311x0 | 6:1214 | 6:1221 | — | — |
| Line127 | Line |  | 69,0 14311x0 | 7:1227 | 7:1234 | — | — |

### GroupFooter1 <a id="groupfooter1-inaltime-288-twips-linia-1239"></a><a id="groupfooter1-inaltime-491-twips-linia-1246"></a><a id="groupfooter1-inaltime-288-twips-linia-1018"></a><a id="groupfooter1-inaltime-288-twips-linia-1123"></a>

| Control | Tip | Text / camp / sursa | Pozitie (twips) | MF_Balanta | MF_Balanta_Subinv | OI_Balanta | Balanta_produse |
|---|---|---|---|---|---|---|---|
| Text109 | TextBox | =Sum([VI]) | 5136,0 2307x285 | 1:1251 | 1:1258 | 1:1030 | 1:1134 |
| Text110 | TextBox | =Sum([VCIn]) | 7443,0 2307x285 | 2:1278 | 2:1285 | 2:1057 | 2:1162 |
| Text111 | TextBox | =Sum([VCOut]) | 9750,0 2307x285 | 3:1306 | 3:1313 | 3:1085 | 3:1191 |
| Text112 | TextBox | =Sum([VF]) | 12057,0 2319x285 | 4:1334 | 4:1341 | 4:1113 | 4:1220 |
| Text114 | TextBox | ="TOTAL CONTUL: " & [Masina] | 72,0 5064x285 | 5:1362 | 5:1369 | 5:1141 | — |
| Text114 | TextBox | [Masina] | 72,0 5064x285 | — | — | — | 5:1249 |

### GroupFooter3 <a id="groupfooter3-inaltime-0-twips-linia-1397"></a>

_fara controale_

### PageFooterSection <a id="pagefootersection-inaltime-501-twips-linia-1390"></a><a id="pagefootersection-inaltime-501-twips-linia-1411"></a><a id="pagefootersection-inaltime-501-twips-linia-1169"></a><a id="pagefootersection-inaltime-501-twips-linia-1277"></a>

| Control | Tip | Text / camp / sursa | Pozitie comună (twips) | MF_Balanta | MF_Balanta_Subinv | OI_Balanta | Balanta_produse |
|---|---|---|---|---|---|---|---|
| Text46 | TextBox | ="Pagina " & [Page] & " din " & [Pages] | 9336,0 5040x285 | 1:1397 | 1:1418 | 1:1176 | — |
| Line117 | Line |  | 72,0 14298x0 | 2:1418 | 2:1439 | 2:1197 | 2:1305 |
| Label107 | TextBox | =DLookUp("expl","ver_rap") | 72,0 2736x477 | 3:1428 | 3:1449 | 3:1207 / 0,0 2736x477 | 3:1315 |
| Text46 | TextBox | ="Pagina " & [Page] & " din " & [Pages]-1 | 9336,0 5040x285 | — | — | — | 1:1284 |

### ReportFooter <a id="reportfooter-inaltime-528-twips-linia-1454"></a><a id="reportfooter-inaltime-528-twips-linia-1475"></a><a id="reportfooter-inaltime-528-twips-linia-1230"></a><a id="reportfooter-inaltime-443-twips-linia-1341"></a>

| Control | Tip | Text / camp / sursa | Pozitie comună (twips) | MF_Balanta | MF_Balanta_Subinv | OI_Balanta | Balanta_produse |
|---|---|---|---|---|---|---|---|
| Label70 | Label | TOTAL General: | 360,113 5328x285 | 1:1462 | 1:1483 | 1:1238 | — |
| Text100 | TextBox | =Sum([VI]) | 5691,113 1947x285 | 2:1478 | 2:1499 | 2:1254 | 2:1373 / 5136,113 2307x330 |
| Text102 | TextBox | =Sum([VCIn]) | 7638,113 1947x285 | 3:1500 | 3:1521 | 3:1276 | 3:1402 / 7443,113 2307x330 |
| Text104 | TextBox | =Sum([VCOut]) | 9585,113 1947x285 | 4:1523 | 4:1544 | 4:1299 | 4:1432 / 9750,113 2307x330 |
| Text106 | TextBox | =Sum([VF]) | 11532,113 1947x285 | 5:1546 | 5:1567 | 5:1322 | 5:1462 / 12057,113 2319x330 |
| Label70 | Label | TOTAL General | 72,113 5064x330 | — | — | — | 1:1349 |
