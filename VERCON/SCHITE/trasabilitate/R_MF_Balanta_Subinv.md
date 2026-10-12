# Trasabilitate schita raport MF_Balanta_Subinv

Sursa: [Reports/MF_Balanta_Subinv.txt](../../VERCON_ACCESS/Reports/MF_Balanta_Subinv.txt) · imagine: [R_MF_Balanta_Subinv.png](../R_MF_Balanta_Subinv.png)

- Titlu (Caption): Balanta produse
- Sursa de date (RecordSource): `MF_Balanta`
- Latime: 14380 twips · sectiuni: 10 · controale: 53

## ReportHeader (inaltime 459 twips, linia 230)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label44 | Label | Balanţă Mijloace FIXE | 96,0 6189x459 | 238 |  |
| Text71 | TextBox | =[Forms]![sfp]![di] | 7986,0 2811x459 | 257 |  |
| Label72 | Label | De la | 6453,0 1533x459 | 279 |  |
| Text73 | TextBox | =[Forms]![sfp]![dsf] | 11562,0 2811x459 | 301 |  |
| Label74 | Label | la | 10797,0 765x459 | 324 |  |

## PageHeaderSection (inaltime 627 twips, linia 348)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Denumire_Label | Label | Denumire | 72,120 3108x495 | 355 |  |
| UM_Label | Label | Nr. Inv. | 3180,120 798x495 | 380 |  |
| Cantitate Initiala_Label | Label | Cant. Iniţiala | 5133,120 1155x495 | 405 |  |
| Valoare Initiala_Label | Label | Valoare Iniţiala | 6288,120 1155x495 | 430 |  |
| Cantitate Intrata_Label | Label | Cant. Intrată | 7443,120 1155x495 | 455 |  |
| Valoare Intrata_Label | Label | Valoare Intrată | 8598,120 1155x495 | 480 |  |
| Cantiate Iesita_Label | Label | Cant. Ieşita | 9753,120 1155x495 | 505 |  |
| Valoare Iesita_Label | Label | Valoare Ieşita | 10908,120 1155x495 | 530 |  |
| Cantitate Finala_Label | Label | Cant. Finală | 12063,120 1155x495 | 555 |  |
| Valoare Finala_Label | Label | Valoare Finală | 13218,120 1155x495 | 580 |  |
| Label89 | Label | PREŢ | 3978,120 1155x495 | 605 |  |

## GroupHeader0 (inaltime 297 twips, linia 632)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text108 | TextBox | ="PRIMITOR: " & [Denumire] & " / " & [Adresa] | 75,0 14298x285 | 643 |  |

## GroupHeader1 (inaltime 285 twips, linia 667)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text120 | TextBox | ="CLASIFICARE: " & [CC] | 75,0 14298x285 | 682 |  |

## Detail (inaltime 285 twips, linia 706)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| DENP | TextBox | DENP | 72,0 3108x285 | 714 |  |
| COD | TextBox | COD | 3180,0 798x285 | 737 |  |
| Cantitate Initiala | TextBox | CI | 5133,0 1155x285 | 761 |  |
| Valoare Initiala | TextBox | VI | 6288,0 1155x285 | 788 |  |
| Cantitate Intrata | TextBox | CCIn | 7443,0 1155x285 | 815 |  |
| Valoare Intrata | TextBox | VCIn | 8598,0 1155x285 | 842 |  |
| Cantiate Iesita | TextBox | CCOut | 9753,0 1155x285 | 869 |  |
| Valoare Iesita | TextBox | VCOut | 10908,0 1155x285 | 896 |  |
| Cantitate Finala | TextBox | CF | 12063,0 1155x285 | 923 |  |
| Valoare Finala | TextBox | VF | 13218,0 1155x285 | 960 |  |
| Pret | TextBox | PT | 3978,0 1155x285 | 997 |  |
| Line115 | Line |  | 72,0 0x285 | 1023 |  |
| Line116 | Line |  | 14373,0 0x285 | 1035 |  |
| Line118 | Line |  | 3978,0 0x285 | 1047 |  |
| Line119 | Line |  | 5133,0 0x285 | 1059 |  |

## GroupFooter2 (inaltime 297 twips, linia 1073)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text121 | TextBox | =Sum([VI]) | 5133,0 2307x285 | 1084 |  |
| Text122 | TextBox | =Sum([VCIn]) | 7440,0 2307x285 | 1111 |  |
| Text123 | TextBox | =Sum([VCOut]) | 9747,0 2307x285 | 1139 |  |
| Text124 | TextBox | =Sum([VF]) | 12061,0 2319x285 | 1167 |  |
| Text125 | TextBox | ="TOTAL CLASIFICAREA: " & [CC] | 69,0 5064x285 | 1195 |  |
| Line126 | Line |  | 69,285 14311x0 | 1221 |  |
| Line127 | Line |  | 69,0 14311x0 | 1234 |  |

## GroupFooter1 (inaltime 491 twips, linia 1246)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text109 | TextBox | =Sum([VI]) | 5136,0 2307x285 | 1258 |  |
| Text110 | TextBox | =Sum([VCIn]) | 7443,0 2307x285 | 1285 |  |
| Text111 | TextBox | =Sum([VCOut]) | 9750,0 2307x285 | 1313 |  |
| Text112 | TextBox | =Sum([VF]) | 12057,0 2319x285 | 1341 |  |
| Text114 | TextBox | ="TOTAL CONTUL: " & [Masina] | 72,0 5064x285 | 1369 |  |

## GroupFooter3 (inaltime 0 twips, linia 1397)

_fara controale_

## PageFooterSection (inaltime 501 twips, linia 1411)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text46 | TextBox | ="Pagina " & [Page] & " din " & [Pages] | 9336,0 5040x285 | 1418 |  |
| Line117 | Line |  | 72,0 14298x0 | 1439 |  |
| Label107 | TextBox | =DLookUp("expl","ver_rap") | 72,0 2736x477 | 1449 |  |

## ReportFooter (inaltime 528 twips, linia 1475)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label70 | Label | TOTAL General: | 360,113 5328x285 | 1483 |  |
| Text100 | TextBox | =Sum([VI]) | 5691,113 1947x285 | 1499 |  |
| Text102 | TextBox | =Sum([VCIn]) | 7638,113 1947x285 | 1521 |  |
| Text104 | TextBox | =Sum([VCOut]) | 9585,113 1947x285 | 1544 |  |
| Text106 | TextBox | =Sum([VF]) | 11532,113 1947x285 | 1567 |  |

