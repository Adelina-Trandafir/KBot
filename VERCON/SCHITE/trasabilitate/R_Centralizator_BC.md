# Trasabilitate schita raport Centralizator_BC

Sursa: [Reports/Centralizator_BC.txt](../../VERCON_ACCESS/Reports/Centralizator_BC.txt) · imagine: [R_Centralizator_BC.png](../R_Centralizator_BC.png)

- Titlu (Caption): —
- Sursa de date (RecordSource): `Centralizator_BC`
- Latime: 9525 twips · sectiuni: 7 · controale: 23

## ReportHeader (inaltime 0 twips, linia 154)

_fara controale_

## PageHeaderSection (inaltime 1434 twips, linia 162)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text18 | TextBox | =DLookUp("Firma","caletemp") | 0,0 9522x306 | 169 |  |
| Label19 | Label | Centralizator Bonuri de Consum | 0,504 9522x480 | 189 |  |
| Text49 | TextBox | =[Forms]![sfp]![di] & " - " & [Forms]![sfp]![dsf] | 0,984 9522x450 | 209 |  |

## GroupHeader1 (inaltime 588 twips, linia 235)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Masina | TextBox | Masina | 0,0 9525x288 | 248 |  |
| Label53 | Label | Număr bon consum | 504,288 1824x288 | 269 |  |
| Label54 | Label | Data | 2328,288 1701x288 | 292 |  |
| Label55 | Label | Valoarea | 4029,288 1425x288 | 316 |  |
| Label56 | Label | Primitor  | 5454,288 3453x288 | 340 |  |

## Detail (inaltime 258 twips, linia 366)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Valoare | TextBox | Valoare | 4029,0 1425x258 | 374 |  |
| NumarBC | TextBox | NumarBC | 504,0 1824x258 | 399 |  |
| DataBC | TextBox | DataBC | 2328,0 1701x258 | 422 |  |
| Client | TextBox | Client | 5454,0 3453x258 | 444 |  |
| Line62 | Line |  | 504,0 0x258 | 469 |  |
| Line63 | Line |  | 8907,0 0x258 | 483 |  |
| Line64 | Line |  | 5454,0 0x258 | 497 |  |

## GroupFooter2 (inaltime 384 twips, linia 513)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label58 | TextBox | ="TOTAL " & [Masina] | 504,0 3528x288 | 527 |  |
| Text60 | TextBox | =Sum([Valoare]) | 4029,0 1425x288 | 549 |  |
| Text61 | TextBox |  | 5454,0 3453x288 | 574 |  |

## PageFooterSection (inaltime 501 twips, linia 598)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text50 | TextBox | ="Pagina " & [Page] & " din " & [Pages] | 7125,24 2400x285 | 605 |  |
| Label107 | TextBox | =DLookUp("expl","ver_rap") | 0,24 2736x477 | 628 |  |
| Line51 | Line |  | 0,0 9522x0 | 652 |  |

## ReportFooter (inaltime 670 twips, linia 664)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text47 | TextBox | =Sum([Valoare]) | 4830,165 2610x330 | 672 |  |
| Label48 | Label | Total bonuri conusm: | 1920,165 2910x330 | 698 |  |

