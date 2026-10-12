# Trasabilitate schita raport Balanta_produse

Sursa: [Reports/Balanta_produse.txt](../../VERCON_ACCESS/Reports/Balanta_produse.txt) · imagine: [R_Balanta_produse.png](../R_Balanta_produse.png)

- Titlu (Caption): Balanta produse
- Sursa de date (RecordSource): `Balanta22`
- Latime: 14376 twips · sectiuni: 7 · controale: 52

## ReportHeader (inaltime 459 twips, linia 214)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label44 | Label | Balanţă stocuri produse | 96,0 6189x459 | 222 |  |
| Text71 | TextBox | =[Forms]![sfp]![di] | 7986,0 2811x459 | 241 |  |
| Label72 | Label | De la | 6453,0 1533x459 | 263 |  |
| Text73 | TextBox | =[Forms]![sfp]![dsf] | 11562,0 2811x459 | 285 |  |
| Label74 | Label | la | 10797,0 765x459 | 308 |  |

## PageHeaderSection (inaltime 1005 twips, linia 332)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Denumire_Label | Label | Denumire | 75,510 3108x495 | 339 |  |
| UM_Label | Label | UM | 3183,510 798x495 | 364 |  |
| Cantitate_Initiala_Label | Label | Cant. Iniţiala | 5136,510 1155x495 | 389 |  |
| Valoare_Initiala_Label | Label | Valoare Iniţiala | 6291,510 1155x495 | 413 |  |
| Cantitate_Intrata_Label | Label | Cant. Intrată | 7446,510 1155x495 | 437 |  |
| Valoare_Intrata_Label | Label | Valoare Intrată | 8601,510 1155x495 | 461 |  |
| Cantiate_Iesita_Label | Label | Cant. Ieşita | 9756,510 1155x495 | 485 |  |
| Valoare_Iesita_Label | Label | Valoare Ieşita | 10911,510 1155x495 | 509 |  |
| Cantitate_Finala_Label | Label | Cant. Finală | 12066,510 1155x495 | 533 |  |
| Valoare_Finala_Label | Label | Valoare Finală | 13221,510 1155x495 | 557 |  |
| Label89 | Label | PREŢ | 3981,510 1155x495 | 581 |  |
| Text0 | TextBox | =DLookUp("FIrma","CaleTemp") | 90,30 5229x343 | 606 |  |
| Text53 | TextBox | =DLookUp("Detalii","CaleTemp") | 9120,30 5229x343 | 631 |  |

## GroupHeader0 (inaltime 297 twips, linia 660)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text108 | TextBox | ="Categorie: " & [masina] | 75,0 14298x285 | 670 |  |

## Detail (inaltime 300 twips, linia 694)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Denumire | TextBox | DENP | 72,0 3108x285 | 702 |  |
| UM | TextBox | UMP | 3180,0 798x285 | 725 |  |
| Cantitate_Initiala | TextBox | CI | 5133,0 1155x285 | 748 |  |
| Valoare_Initiala | TextBox | VI | 6288,0 1155x285 | 775 |  |
| Cantitate_Intrata | TextBox | CCIn | 7443,0 1155x285 | 802 |  |
| Valoare_Intrata | TextBox | VCIn | 8598,0 1155x285 | 829 |  |
| Cantiate_Iesita | TextBox | CCOut | 9753,0 1155x285 | 856 |  |
| Valoare_Iesita | TextBox | VCOut | 10908,0 1155x285 | 883 |  |
| Cantitate_Finala | TextBox | CF | 12063,0 1155x285 | 910 |  |
| Valoare_Finala | TextBox | VF | 13218,0 1155x285 | 947 |  |
| Pret | TextBox | PT | 3978,0 1155x285 | 984 |  |
| Line115 | Line |  | 72,0 0x285 | 1011 |  |
| Line116 | Line |  | 14373,0 0x285 | 1023 |  |
| Line118 | Line |  | 3978,0 0x285 | 1035 |  |
| Line119 | Line |  | 5133,0 0x285 | 1047 |  |
| Line120 | Line |  | 72,285 14301x0 | 1059 |  |
| Line121 | Line |  | 7443,0 0x285 | 1073 |  |
| Line122 | Line |  | 9753,0 0x285 | 1085 |  |
| Line123 | Line |  | 12063,0 0x285 | 1097 |  |
| Line124 | Line |  | 14373,0 0x285 | 1109 |  |

## GroupFooter1 (inaltime 288 twips, linia 1123)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text109 | TextBox | =Sum([VI]) | 5136,0 2307x285 | 1134 |  |
| Text110 | TextBox | =Sum([VCIn]) | 7443,0 2307x285 | 1162 |  |
| Text111 | TextBox | =Sum([VCOut]) | 9750,0 2307x285 | 1191 |  |
| Text112 | TextBox | =Sum([VF]) | 12057,0 2319x285 | 1220 |  |
| Text114 | TextBox | [Masina] | 72,0 5064x285 | 1249 |  |

## PageFooterSection (inaltime 501 twips, linia 1277)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text46 | TextBox | ="Pagina " & [Page] & " din " & [Pages]-1 | 9336,0 5040x285 | 1284 |  |
| Line117 | Line |  | 72,0 14298x0 | 1305 |  |
| Label107 | TextBox | =DLookUp("expl","ver_rap") | 72,0 2736x477 | 1315 |  |

## ReportFooter (inaltime 443 twips, linia 1341)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label70 | Label | TOTAL General | 72,113 5064x330 | 1349 |  |
| Text100 | TextBox | =Sum([VI]) | 5136,113 2307x330 | 1373 |  |
| Text102 | TextBox | =Sum([VCIn]) | 7443,113 2307x330 | 1402 |  |
| Text104 | TextBox | =Sum([VCOut]) | 9750,113 2307x330 | 1432 |  |
| Text106 | TextBox | =Sum([VF]) | 12057,113 2319x330 | 1462 |  |

