# Trasabilitate schita raport Stoc_Final

Sursa: [Reports/Stoc_Final.txt](../../VERCON_ACCESS/Reports/Stoc_Final.txt) · imagine: [R_Stoc_Final.png](../R_Stoc_Final.png)

- Titlu (Caption): Balanta produse
- Sursa de date (RecordSource): `SELECT Balanta22.Masina, Balanta22.CODP, Balanta22.DENP, Balanta22.UMP, Balanta22.PT, Balanta22.CF, Balanta22.VF, IDM FROM Balanta22 WHERE (((Balanta22.CF)<>0)); `
- Latime: 9231 twips · sectiuni: 7 · controale: 31

## ReportHeader (inaltime 0 twips, linia 215)

_fara controale_

## PageHeaderSection (inaltime 1530 twips, linia 223)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label89 | Label | PREŢ | 5406,1035 1275x495 | 230 |  |
| Valoare Finala_Label | Label | Valoare Finală | 7956,1035 1275x495 | 256 |  |
| Cantitate Finala_Label | Label | Cant. Finală | 6681,1035 1275x495 | 282 |  |
| UM_Label | Label | UM | 4278,1035 1128x495 | 308 |  |
| Denumire_Label | Label | Denumire | 75,1035 4203x495 | 334 |  |
| Text73 | TextBox | =fluna(Month([Forms]![sfp]![dsf])) & "/" & Year([Forms]![sfp]![dsf]) | 3768,0 5448x330 | 360 |  |
| Label44 | Label | Stoc final luna | 0,0 3768x330 | 384 |  |
| Text108 | TextBox | =UCase(DLookUp("Firma","CaleTemp")) | 0,330 9216x315 | 402 |  |
| Text124 | TextBox | =UCase(DLookUp("detalii","CaleTemp")) | 0,645 9216x315 | 426 |  |

## GroupHeader0 (inaltime 297 twips, linia 451)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text110 | TextBox | ="Categorie: " & [masina] | 60,0 9156x285 | 462 |  |

## Detail (inaltime 285 twips, linia 487)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Pret | TextBox | PT | 5391,0 1275x285 | 495 |  |
| Valoare Finala | TextBox | VF | 7941,0 1275x285 | 522 |  |
| Cantitate Finala | TextBox | CF | 6666,0 1275x285 | 561 |  |
| UM | TextBox | UMP | 4263,0 1128x285 | 600 |  |
| Denumire | TextBox | DENP | 60,0 4203x285 | 624 |  |
| Line116 | Line |  | 60,0 0x285 | 649 |  |
| Line117 | Line |  | 5391,0 0x285 | 661 |  |
| Line118 | Line |  | 9216,0 0x285 | 674 |  |

## GroupFooter1 (inaltime 300 twips, linia 688)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text111 | TextBox |  | 5391,0 1275x285 | 700 |  |
| Text112 | TextBox | =Sum([VF]) | 7941,0 1275x285 | 726 |  |
| Text113 | TextBox |  | 6666,0 1275x285 | 765 |  |
| Text115 | TextBox | ="Total " & [Masina] | 60,0 5331x285 | 802 |  |
| Line119 | Line |  | 60,285 9156x0 | 827 |  |
| Line120 | Line |  | 60,0 0x285 | 840 |  |
| Line121 | Line |  | 5391,0 0x285 | 852 |  |
| Line122 | Line |  | 9216,0 0x285 | 865 |  |
| Line123 | Line |  | 60,0 9156x0 | 877 |  |

## PageFooterSection (inaltime 525 twips, linia 889)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label107 | TextBox | =DLookUp("expl","ver_rap") | 72,48 2736x477 | 896 |  |
| Text46 | TextBox | ="Pagina " & [Page] & " din " & [Pages] | 4176,48 5040x285 | 921 |  |

## ReportFooter (inaltime 456 twips, linia 948)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text106 | TextBox | =Sum([VF]) | 5391,72 3819x285 | 956 |  |
| Label70 | Label | TOTAL General: | 60,72 5331x285 | 984 |  |

