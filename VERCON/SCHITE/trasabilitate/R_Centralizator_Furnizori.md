# Trasabilitate schita raport Centralizator_Furnizori

Sursa: [Reports/Centralizator_Furnizori.txt](../../VERCON_ACCESS/Reports/Centralizator_Furnizori.txt) · imagine: [R_Centralizator_Furnizori.png](../R_Centralizator_Furnizori.png)

- Titlu (Caption): —
- Sursa de date (RecordSource): `Listare toti furnizorii`
- Latime: 8946 twips · sectiuni: 7 · controale: 30

## ReportHeader (inaltime 0 twips, linia 170)

_fara controale_

## PageHeaderSection (inaltime 1560 twips, linia 178)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text20 | TextBox | =DLookUp("firma","caletemp") | 0,0 8946x315 | 185 |  |
| Text22 | TextBox | =[Forms]![sfp]![di] & " - " & [Forms]![sfp]![dsf] | 0,1032 8946x315 | 205 |  |
| Label1 | Label | Situații furnizori | 0,576 8946x450 | 227 |  |

## GroupHeader0 (inaltime 702 twips, linia 250)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Denumire | TextBox | Denumire | 0,72 4122x258 | 258 |  |
| Adresa | TextBox | Adresa | 4128,72 2826x258 | 280 |  |
| CF | TextBox | CF | 6960,72 1986x258 | 304 |  |
| Label9 | Label | Serie/Număr fact. | 0,432 2040x270 | 329 |  |
| Label10 | Label | Dată | 2040,432 1125x270 | 351 |  |
| Label26 | Label | NIR | 3165,432 813x270 | 375 |  |
| Label27 | Label | Cont | 3978,432 3465x270 | 399 |  |
| Label11 | Label | Valoare | 7443,432 1503x270 | 423 |  |
| Line34 | Line |  | 0,0 8946x0 | 447 |  |
| Line35 | Line |  | 0,0 0x426 | 455 |  |
| Line36 | Line |  | 8946,0 0x426 | 464 |  |

## Detail (inaltime 270 twips, linia 478)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| NrFact | TextBox | NrFact | 0,0 2040x270 | 486 |  |
| Data | TextBox | DataFc | 2040,0 1125x270 | 506 |  |
| Total | TextBox | Total | 7443,0 1503x270 | 529 |  |
| Numar | TextBox | Numar | 3165,0 813x270 | 555 |  |
| Masina | TextBox | Masina | 3978,0 3465x270 | 580 |  |
| Line30 | Line |  | 0,0 0x270 | 606 |  |
| Line31 | Line |  | 3978,0 0x270 | 615 |  |
| Line32 | Line |  | 8946,0 0x270 | 627 |  |
| Line33 | Line |  | 7443,0 0x270 | 639 |  |

## GroupFooter1 (inaltime 624 twips, linia 653)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text12 | TextBox | =Sum([Total]) | 7443,0 1503x270 | 661 |  |
| Label13 | TextBox | ="TOTAL " & [Denumire] | 0,4 7437x270 | 685 |  |

## PageFooterSection (inaltime 636 twips, linia 710)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Line212 | Line |  | 0,0 8946x0 | 717 |  |
| Text211 | TextBox | =DLookUp("expl","ver_rap") | 48,0 2736x477 | 725 |  |
| Text46 | TextBox | ="Pagina " & [Page] & " din " & [Pages] | 3894,0 5040x285 | 748 |  |

## ReportFooter (inaltime 406 twips, linia 773)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text18 | TextBox | =Sum([Total]) | 7443,72 1503x330 | 781 |  |
| Label19 | TextBox | ="TOTAL PERIOADA " & fluna(Month([Forms]![sfp]![di])) & "/" & Year([Forms]![sfp]![di]) & " - " & fluna(Month([Forms]![sf | 0,76 7440x330 | 805 |  |

