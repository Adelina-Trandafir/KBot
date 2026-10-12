# Trasabilitate schita raport nou_Furnizori

Sursa: [Reports/nou_Furnizori.txt](../../VERCON_ACCESS/Reports/nou_Furnizori.txt) · imagine: [R_nou_Furnizori.png](../R_nou_Furnizori.png)

- Titlu (Caption): —
- Sursa de date (RecordSource): `Listare Furnizori`
- Latime: 8685 twips · sectiuni: 8 · controale: 29

## ReportHeader (inaltime 0 twips, linia 158)

_fara controale_

## PageHeaderSection (inaltime 525 twips, linia 166)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Box16 | Rectangle |  | 0,0 8632x510 | 173 |  |
| Label1 | Label | Situatii furnizori... | 0,0 3450x510 | 182 |  |
| Line2 | Line |  | 56,510 8573x0 | 194 |  |
| Text18 | TextBox | =DLookUp("Firma","Caletemp") | 3514,56 5046x390 | 203 |  |

## GroupHeader0 (inaltime 1635 twips, linia 224)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Denumire | TextBox | Denumire | 1359,0 3516x330 | 232 |  |
| Label0 | Label | Denumire: | 56,0 1185x315 | 250 |  |
| Text3 | TextBox | adresa | 1359,450 3516x330 | 265 |  |
| Label4 | Label | Adresa: | 56,435 1185x315 | 286 |  |
| Text6 | TextBox | CF | 1359,900 3516x330 | 303 |  |
| Label7 | Label | Cod fiscal: | 56,870 1185x315 | 324 |  |
| Line8 | Line |  | 56,1303 8629x0 | 341 |  |
| Label9 | Label | Numar factura | 3011,1365 1695x270 | 354 |  |
| Label10 | Label | Data: | 4995,1365 1695x270 | 373 |  |
| Label11 | Label | Total: | 6980,1365 1695x270 | 392 |  |

## Detail (inaltime 283 twips, linia 413)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| NrFact | TextBox | NrFact | 2955,0 0x270 | 421 |  |
| Data | TextBox | Data | 4939,0 0x270 | 435 |  |
| Total | TextBox | Total | 6924,0 0x270 | 449 |  |

## GroupFooter0 (inaltime 396 twips, linia 468)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text21 | TextBox | =Sum([Total]) | 6923,0 0x270 | 477 |  |
| Label22 | Label | Total plati furnizor | 2891,4 3225x270 | 492 |  |
| Text23 | TextBox | =Month([Data]) & "." & Year([Data]) | 4755,0 0x285 | 506 |  |
| Line25 | Line |  | 2891,0 5726x0 | 521 |  |

## GroupFooter1 (inaltime 274 twips, linia 531)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text12 | TextBox | =Sum([Total]) | 6923,0 0x270 | 539 |  |
| Label13 | Label | Total plati furnizor... | 120,4 2775x270 | 554 |  |
| Line15 | Line |  | 0,0 8629x0 | 568 |  |

## PageFooterSection (inaltime 636 twips, linia 577)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Box17 | Rectangle |  | 0,0 8632x630 | 584 |  |
| Text14 | TextBox | ="Page " & [Page] & " of " & [Pages] | 7591,396 1026x0 | 593 |  |
| Label107 | TextBox | =DLookUp("expl","ver_rap") | 0,0 2736x477 | 607 |  |

## ReportFooter (inaltime 360 twips, linia 630)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text26 | TextBox | =Sum([Total]) | 6947,0 1701x270 | 639 |  |
| Label27 | Label | Total plati perioada. | 144,4 2775x270 | 657 |  |

