# Trasabilitate schita raport MF_Balanta

Sursa: [Reports/MF_Balanta.txt](../../VERCON_ACCESS/Reports/MF_Balanta.txt) · imagine: [R_MF_Balanta.png](../R_MF_Balanta.png)

- Titlu (Caption): Balanta produse
- Sursa de date (RecordSource): `MF_Balanta`
- Latime: 14380 twips · sectiuni: 9 · controale: 53

## ReportHeader (inaltime 459 twips, linia 224)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label44 | Label | Balanţă Mijloace FIXE | 96,0 6189x459 | 232 |  |
| Text71 | TextBox | =[Forms]![sfp]![di] | 7986,0 2811x459 | 251 |  |
| Label72 | Label | De la | 6453,0 1533x459 | 273 |  |
| Text73 | TextBox | =[Forms]![sfp]![dsf] | 11562,0 2811x459 | 295 |  |
| Label74 | Label | la | 10797,0 765x459 | 318 |  |

## PageHeaderSection (inaltime 627 twips, linia 342)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Denumire_Label | Label | Denumire | 72,120 3108x495 | 349 |  |
| UM_Label | Label | Nr. Inv. | 3180,120 798x495 | 374 |  |
| Cantitate Initiala_Label | Label | Cant. Iniţiala | 5133,120 1155x495 | 399 |  |
| Valoare Initiala_Label | Label | Valoare Iniţiala | 6288,120 1155x495 | 424 |  |
| Cantitate Intrata_Label | Label | Cant. Intrată | 7443,120 1155x495 | 449 |  |
| Valoare Intrata_Label | Label | Valoare Intrată | 8598,120 1155x495 | 474 |  |
| Cantiate Iesita_Label | Label | Cant. Ieşita | 9753,120 1155x495 | 499 |  |
| Valoare Iesita_Label | Label | Valoare Ieşita | 10908,120 1155x495 | 524 |  |
| Cantitate Finala_Label | Label | Cant. Finală | 12063,120 1155x495 | 549 |  |
| Valoare Finala_Label | Label | Valoare Finală | 13218,120 1155x495 | 574 |  |
| Label89 | Label | PREŢ | 3978,120 1155x495 | 599 |  |

## GroupHeader0 (inaltime 297 twips, linia 626)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text108 | TextBox | ="Categorie: " & [masina] | 75,0 14298x285 | 636 |  |

## GroupHeader1 (inaltime 285 twips, linia 660)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text120 | TextBox | ="CLASIFICARE: " & [CC] | 75,0 14298x285 | 675 |  |

## Detail (inaltime 285 twips, linia 699)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Denumire | TextBox | DENP | 72,0 3108x285 | 707 |  |
| COD | TextBox | COD | 3180,0 798x285 | 730 |  |
| Cantitate Initiala | TextBox | CI | 5133,0 1155x285 | 754 |  |
| Valoare Initiala | TextBox | VI | 6288,0 1155x285 | 781 |  |
| Cantitate Intrata | TextBox | CCIn | 7443,0 1155x285 | 808 |  |
| Valoare Intrata | TextBox | VCIn | 8598,0 1155x285 | 835 |  |
| Cantiate Iesita | TextBox | CCOut | 9753,0 1155x285 | 862 |  |
| Valoare Iesita | TextBox | VCOut | 10908,0 1155x285 | 889 |  |
| Cantitate Finala | TextBox | CF | 12063,0 1155x285 | 916 |  |
| Valoare Finala | TextBox | VF | 13218,0 1155x285 | 953 |  |
| Pret | TextBox | PT | 3978,0 1155x285 | 990 |  |
| Line115 | Line |  | 72,0 0x285 | 1016 |  |
| Line116 | Line |  | 14373,0 0x285 | 1028 |  |
| Line118 | Line |  | 3978,0 0x285 | 1040 |  |
| Line119 | Line |  | 5133,0 0x285 | 1052 |  |

## GroupFooter2 (inaltime 297 twips, linia 1066)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text121 | TextBox | =Sum([VI]) | 5133,0 2307x285 | 1077 |  |
| Text122 | TextBox | =Sum([VCIn]) | 7440,0 2307x285 | 1104 |  |
| Text123 | TextBox | =Sum([VCOut]) | 9747,0 2307x285 | 1132 |  |
| Text124 | TextBox | =Sum([VF]) | 12061,0 2319x285 | 1160 |  |
| Text125 | TextBox | ="TOTAL CLASIFICAREA: " & [CC] | 69,0 5064x285 | 1188 |  |
| Line126 | Line |  | 69,285 14311x0 | 1214 |  |
| Line127 | Line |  | 69,0 14311x0 | 1227 |  |

## GroupFooter1 (inaltime 288 twips, linia 1239)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text109 | TextBox | =Sum([VI]) | 5136,0 2307x285 | 1251 |  |
| Text110 | TextBox | =Sum([VCIn]) | 7443,0 2307x285 | 1278 |  |
| Text111 | TextBox | =Sum([VCOut]) | 9750,0 2307x285 | 1306 |  |
| Text112 | TextBox | =Sum([VF]) | 12057,0 2319x285 | 1334 |  |
| Text114 | TextBox | ="TOTAL CONTUL: " & [Masina] | 72,0 5064x285 | 1362 |  |

## PageFooterSection (inaltime 501 twips, linia 1390)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text46 | TextBox | ="Pagina " & [Page] & " din " & [Pages] | 9336,0 5040x285 | 1397 |  |
| Line117 | Line |  | 72,0 14298x0 | 1418 |  |
| Label107 | TextBox | =DLookUp("expl","ver_rap") | 72,0 2736x477 | 1428 |  |

## ReportFooter (inaltime 528 twips, linia 1454)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label70 | Label | TOTAL General: | 360,113 5328x285 | 1462 |  |
| Text100 | TextBox | =Sum([VI]) | 5691,113 1947x285 | 1478 |  |
| Text102 | TextBox | =Sum([VCIn]) | 7638,113 1947x285 | 1500 |  |
| Text104 | TextBox | =Sum([VCOut]) | 9585,113 1947x285 | 1523 |  |
| Text106 | TextBox | =Sum([VF]) | 11532,113 1947x285 | 1546 |  |

