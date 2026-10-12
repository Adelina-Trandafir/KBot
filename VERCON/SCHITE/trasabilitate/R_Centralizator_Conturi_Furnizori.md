# Trasabilitate schita raport Centralizator_Conturi_Furnizori

Sursa: [Reports/Centralizator_Conturi_Furnizori.txt](../../VERCON_ACCESS/Reports/Centralizator_Conturi_Furnizori.txt) · imagine: [R_Centralizator_Conturi_Furnizori.png](../R_Centralizator_Conturi_Furnizori.png)

- Titlu (Caption): —
- Sursa de date (RecordSource): `SELECT Furnizori.IDF, Produse.IdMasina, Masini.Masina, Furnizori.Denumire, Furnizori.Adresa, Furnizori.CF, NIR.NrFact, NIR.Numar, NIR.Data, NIR.DataFc, Sum(Intrari.Valoarecutva) AS Total FROM (tmpSFP INNER JOIN ((Masini INNER JOIN Produse ON Masini.IdMasina = Produse.IdMasina) INNER JOIN ProduseP ON Produse.ID = ProduseP.ID) ON tmpSFP.CodProdus = ProduseP.IdProdus) INNER JOIN ((Furnizori INNER JOI …`
- Latime: 8970 twips · sectiuni: 9 · controale: 37

## ReportHeader (inaltime 0 twips, linia 182)

_fara controale_

## PageHeaderSection (inaltime 1560 twips, linia 190)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text20 | TextBox | =DLookUp("firma","caletemp") | 0,0 8946x315 | 197 |  |
| Line46 | Line |  | 0,1536 8970x0 | 217 |  |
| Text22 | TextBox | =[Forms]![sfp]![di] & " - " & [Forms]![sfp]![dsf] | 0,1032 8946x315 | 228 |  |
| Label1 | Label | Situații furnizori | 0,576 8946x450 | 250 |  |

## GroupHeader0 (inaltime 426 twips, linia 273)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Masina | TextBox | Masina | 0,72 8949x270 | 281 |  |
| Line34 | Line |  | 0,0 8970x0 | 307 |  |
| Line35 | Line |  | 0,0 0x426 | 315 |  |
| Line36 | Line |  | 8949,0 0x426 | 324 |  |

## GroupHeader1 (inaltime 654 twips, linia 338)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Denumire | TextBox | Denumire | 1752,48 7197x270 | 349 |  |
| Label9 | Label | Serie/Număr fact. | 3468,372 2040x270 | 373 |  |
| Label10 | Label | Dată | 5511,372 1125x270 | 397 |  |
| Label26 | Label | NIR | 6636,372 813x270 | 421 |  |
| Line38 | Line |  | 8949,0 0x654 | 445 |  |
| Line37 | Line |  | 0,0 0x654 | 457 |  |
| Label11 | Label | Valoare | 7449,372 1503x270 | 466 |  |
| Line39 | Line |  | 1752,0 0x654 | 490 |  |
| Line41 | Line |  | 1752,0 7197x0 | 502 |  |

## Detail (inaltime 270 twips, linia 514)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| NrFact | TextBox | NrFact | 3468,0 2040x270 | 522 |  |
| Data | TextBox | DataFc | 5508,0 1125x270 | 544 |  |
| Total | TextBox | Total | 7446,0 1503x270 | 567 |  |
| Numar | TextBox | Numar | 6633,0 813x270 | 593 |  |
| Line30 | Line |  | 3468,0 0x270 | 618 |  |
| Line31 | Line |  | 6633,0 0x270 | 630 |  |
| Line32 | Line |  | 7446,0 0x270 | 642 |  |
| Line33 | Line |  | 8949,0 0x270 | 654 |  |
| Line40 | Line |  | 1752,0 0x270 | 666 |  |
| Line42 | Line |  | 0,0 0x270 | 678 |  |

## GroupFooter2 (inaltime 330 twips, linia 689)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text12 | TextBox | =Sum([Total]) | 7446,0 1503x270 | 700 |  |
| Label13 | TextBox | ="TOTAL " & [Denumire] | 1752,0 5697x270 | 724 |  |
| Line43 | Line |  | 0,0 0x330 | 748 |  |

## GroupFooter1 (inaltime 624 twips, linia 759)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text44 | TextBox | =Sum([Total]) | 7446,0 1503x270 | 767 |  |
| Text45 | TextBox | ="TOTAL " & [Masina] | 0,0 7449x270 | 791 |  |

## PageFooterSection (inaltime 477 twips, linia 815)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Line212 | Line |  | 0,0 8946x0 | 822 |  |
| Text211 | TextBox | =DLookUp("expl","ver_rap") | 48,0 2736x477 | 830 |  |
| Text46 | TextBox | ="Pagina " & [Page] & " din " & [Pages] | 3894,0 5040x285 | 853 |  |

## ReportFooter (inaltime 406 twips, linia 878)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text18 | TextBox | =Sum([Total]) | 7443,72 1503x330 | 886 |  |
| Label19 | TextBox | ="TOTAL PERIOADA " & fluna(Month([Forms]![sfp]![di])) & "/" & Year([Forms]![sfp]![di]) & " - " & fluna(Month([Forms]![sf | 0,76 7440x330 | 910 |  |

