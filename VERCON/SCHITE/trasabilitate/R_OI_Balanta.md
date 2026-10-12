# Trasabilitate schita raport OI_Balanta

Sursa: [Reports/OI_Balanta.txt](../../VERCON_ACCESS/Reports/OI_Balanta.txt) · imagine: [R_OI_Balanta.png](../R_OI_Balanta.png)

- Titlu (Caption): Balanta produse
- Sursa de date (RecordSource): `OI_Balanta`
- Latime: 14376 twips · sectiuni: 7 · controale: 45

## ReportHeader (inaltime 459 twips, linia 216)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label44 | Label | Balanţă Obiecte Inventar | 96,0 6189x459 | 224 |  |
| Text71 | TextBox | =[Forms]![sfp]![di] | 7986,0 2811x459 | 243 |  |
| Label72 | Label | De la | 6453,0 1533x459 | 265 |  |
| Text73 | TextBox | =[Forms]![sfp]![dsf] | 11562,0 2811x459 | 287 |  |
| Label74 | Label | la | 10797,0 765x459 | 310 |  |

## PageHeaderSection (inaltime 627 twips, linia 334)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Denumire_Label | Label | Denumire | 72,120 3108x495 | 341 |  |
| UM_Label | Label | UM | 3180,120 798x495 | 366 |  |
| Cantitate Initiala_Label | Label | Cant. Iniţiala | 5133,120 1155x495 | 391 |  |
| Valoare Initiala_Label | Label | Valoare Iniţiala | 6288,120 1155x495 | 416 |  |
| Cantitate Intrata_Label | Label | Cant. Intrată | 7443,120 1155x495 | 441 |  |
| Valoare Intrata_Label | Label | Valoare Intrată | 8598,120 1155x495 | 466 |  |
| Cantiate Iesita_Label | Label | Cant. Ieşita | 9753,120 1155x495 | 491 |  |
| Valoare Iesita_Label | Label | Valoare Ieşita | 10908,120 1155x495 | 516 |  |
| Cantitate Finala_Label | Label | Cant. Finală | 12063,120 1155x495 | 541 |  |
| Valoare Finala_Label | Label | Valoare Finală | 13218,120 1155x495 | 566 |  |
| Label89 | Label | PREŢ | 3978,120 1155x495 | 591 |  |

## GroupHeader0 (inaltime 297 twips, linia 618)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text108 | TextBox | ="Categorie: " & [masina] | 75,0 14298x285 | 628 |  |

## Detail (inaltime 285 twips, linia 652)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Denumire | TextBox | DENP | 72,0 3108x285 | 660 |  |
| UM | TextBox | UMP | 3180,0 798x285 | 683 |  |
| Cantitate Initiala | TextBox | CI | 5133,0 1155x285 | 706 |  |
| Valoare Initiala | TextBox | VI | 6288,0 1155x285 | 733 |  |
| Cantitate Intrata | TextBox | CCIn | 7443,0 1155x285 | 760 |  |
| Valoare Intrata | TextBox | VCIn | 8598,0 1155x285 | 787 |  |
| Cantiate Iesita | TextBox | CCOut | 9753,0 1155x285 | 814 |  |
| Valoare Iesita | TextBox | VCOut | 10908,0 1155x285 | 841 |  |
| Cantitate Finala | TextBox | CF | 12063,0 1155x285 | 868 |  |
| Valoare Finala | TextBox | VF | 13218,0 1155x285 | 905 |  |
| Pret | TextBox | PT | 3978,0 1155x285 | 942 |  |
| Line115 | Line |  | 72,0 0x285 | 968 |  |
| Line116 | Line |  | 14373,0 0x285 | 980 |  |
| Line118 | Line |  | 3978,0 0x285 | 992 |  |
| Line119 | Line |  | 5133,0 0x285 | 1004 |  |

## GroupFooter1 (inaltime 288 twips, linia 1018)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text109 | TextBox | =Sum([VI]) | 5136,0 2307x285 | 1030 |  |
| Text110 | TextBox | =Sum([VCIn]) | 7443,0 2307x285 | 1057 |  |
| Text111 | TextBox | =Sum([VCOut]) | 9750,0 2307x285 | 1085 |  |
| Text112 | TextBox | =Sum([VF]) | 12057,0 2319x285 | 1113 |  |
| Text114 | TextBox | ="TOTAL CONTUL: " & [Masina] | 72,0 5064x285 | 1141 |  |

## PageFooterSection (inaltime 501 twips, linia 1169)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text46 | TextBox | ="Pagina " & [Page] & " din " & [Pages] | 9336,0 5040x285 | 1176 |  |
| Line117 | Line |  | 72,0 14298x0 | 1197 |  |
| Label107 | TextBox | =DLookUp("expl","ver_rap") | 0,0 2736x477 | 1207 |  |

## ReportFooter (inaltime 528 twips, linia 1230)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label70 | Label | TOTAL General: | 360,113 5328x285 | 1238 |  |
| Text100 | TextBox | =Sum([VI]) | 5691,113 1947x285 | 1254 |  |
| Text102 | TextBox | =Sum([VCIn]) | 7638,113 1947x285 | 1276 |  |
| Text104 | TextBox | =Sum([VCOut]) | 9585,113 1947x285 | 1299 |  |
| Text106 | TextBox | =Sum([VF]) | 11532,113 1947x285 | 1322 |  |

