# Trasabilitate schite centralizatoare de furnizori

Convenții și limite: [citirea fișelor consolidate](../INDEX.md#fise-consolidate). SLICE-D003-17, 28.10.2026. Secțiunea este coloana întâi.

## Variante și surse

### Trasabilitate schita raport Centralizator_Conturi_Furnizori

Sursa: [Reports/Centralizator_Conturi_Furnizori.txt](../../VERCON_ACCESS/Reports/Centralizator_Conturi_Furnizori.txt) · imagine: [R_Centralizator_Conturi_Furnizori.png](../R_Centralizator_Conturi_Furnizori.png)

- Titlu (Caption): — · Sursa de date (RecordSource): `SELECT Furnizori.IDF, Produse.IdMasina, Masini.Masina, Furnizori.Denumire, Furnizori.Adresa, Furnizori.CF, NIR.NrFact, NIR.Numar, NIR.Data, NIR.DataFc, Sum(Intrari.Valoarecutva) AS Total FROM (tmpSFP INNER JOIN ((Masini INNER JOIN Produse ON Masini.IdMasina = Produse.IdMasina) INNER JOIN ProduseP ON Produse.ID = ProduseP.ID) ON tmpSFP.CodProdus = ProduseP.IdProdus) INNER JOIN ((Furnizori INNER JOI …` · Latime: 8970 twips · sectiuni: 9 · controale: 37

### Trasabilitate schita raport Centralizator_Furnizori

Sursa: [Reports/Centralizator_Furnizori.txt](../../VERCON_ACCESS/Reports/Centralizator_Furnizori.txt) · imagine: [R_Centralizator_Furnizori.png](../R_Centralizator_Furnizori.png)

- Titlu (Caption): — · Sursa de date (RecordSource): `Listare toti furnizorii` · Latime: 8946 twips · sectiuni: 7 · controale: 30

## Secțiuni

| Secțiune | Centralizator_Conturi_Furnizori | Centralizator_Furnizori |
|---|---|---|
| ReportHeader <a id="reportheader-inaltime-0-twips-linia-182"></a><a id="reportheader-inaltime-0-twips-linia-170"></a> | 1 / 0 / 182 | 1 / 0 / 170 |
| PageHeaderSection <a id="pageheadersection-inaltime-1560-twips-linia-190"></a><a id="pageheadersection-inaltime-1560-twips-linia-178"></a> | 2 / 1560 / 190 | 2 / 1560 / 178 |
| GroupHeader0 <a id="groupheader0-inaltime-426-twips-linia-273"></a><a id="groupheader0-inaltime-702-twips-linia-250"></a> | 3 / 426 / 273 | 3 / 702 / 250 |
| GroupHeader1 <a id="groupheader1-inaltime-654-twips-linia-338"></a> | 4 / 654 / 338 | — |
| Detail <a id="detail-inaltime-270-twips-linia-514"></a><a id="detail-inaltime-270-twips-linia-478"></a> | 5 / 270 / 514 | 4 / 270 / 478 |
| GroupFooter2 <a id="groupfooter2-inaltime-330-twips-linia-689"></a> | 6 / 330 / 689 | — |
| GroupFooter1 <a id="groupfooter1-inaltime-624-twips-linia-759"></a><a id="groupfooter1-inaltime-624-twips-linia-653"></a> | 7 / 624 / 759 | 5 / 624 / 653 |
| PageFooterSection <a id="pagefootersection-inaltime-477-twips-linia-815"></a><a id="pagefootersection-inaltime-636-twips-linia-710"></a> | 8 / 477 / 815 | 6 / 636 / 710 |
| ReportFooter <a id="reportfooter-inaltime-406-twips-linia-878"></a><a id="reportfooter-inaltime-406-twips-linia-773"></a> | 9 / 406 / 878 | 7 / 406 / 773 |

## Controale

| Secțiune | Control | Tip | Text / camp / sursa | Pozitie comună (twips) | Evenimente | Centralizator_Conturi_Furnizori | Centralizator_Furnizori |
|---|---|---|---|---|---|---|---|
| PageHeaderSection | Text20 | TextBox | =DLookUp("firma","caletemp") | 0,0 8946x315 |  | 1:197 | 1:185 |
| PageHeaderSection | Line46 | Line |  | 0,1536 8970x0 |  | 2:217 | — |
| PageHeaderSection | Text22 | TextBox | =[Forms]![sfp]![di] & " - " & [Forms]![sfp]![dsf] | 0,1032 8946x315 |  | 3:228 | 2:205 |
| PageHeaderSection | Label1 | Label | Situații furnizori | 0,576 8946x450 |  | 4:250 | 3:227 |
| GroupHeader0 | Masina | TextBox | Masina | 0,72 8949x270 |  | 1:281 | — |
| GroupHeader0 | Line34 | Line |  | 0,0 8970x0 |  | 2:307 | 9:447 / 0,0 8946x0 |
| GroupHeader0 | Line35 | Line |  | 0,0 0x426 |  | 3:315 | 10:455 |
| GroupHeader0 | Line36 | Line |  | 8949,0 0x426 |  | 4:324 | 11:464 / 8946,0 0x426 |
| GroupHeader0 | Denumire | TextBox | Denumire | 0,72 4122x258 |  | — | 1:258 |
| GroupHeader0 | Adresa | TextBox | Adresa | 4128,72 2826x258 |  | — | 2:280 |
| GroupHeader0 | CF | TextBox | CF | 6960,72 1986x258 |  | — | 3:304 |
| GroupHeader0 | Label9 | Label | Serie/Număr fact. | 0,432 2040x270 |  | — | 4:329 |
| GroupHeader0 | Label10 | Label | Dată | 2040,432 1125x270 |  | — | 5:351 |
| GroupHeader0 | Label26 | Label | NIR | 3165,432 813x270 |  | — | 6:375 |
| GroupHeader0 | Label27 | Label | Cont | 3978,432 3465x270 |  | — | 7:399 |
| GroupHeader0 | Label11 | Label | Valoare | 7443,432 1503x270 |  | — | 8:423 |
| GroupHeader1 | Denumire | TextBox | Denumire | 1752,48 7197x270 |  | 1:349 | — |
| GroupHeader1 | Label9 | Label | Serie/Număr fact. | 3468,372 2040x270 |  | 2:373 | — |
| GroupHeader1 | Label10 | Label | Dată | 5511,372 1125x270 |  | 3:397 | — |
| GroupHeader1 | Label26 | Label | NIR | 6636,372 813x270 |  | 4:421 | — |
| GroupHeader1 | Line38 | Line |  | 8949,0 0x654 |  | 5:445 | — |
| GroupHeader1 | Line37 | Line |  | 0,0 0x654 |  | 6:457 | — |
| GroupHeader1 | Label11 | Label | Valoare | 7449,372 1503x270 |  | 7:466 | — |
| GroupHeader1 | Line39 | Line |  | 1752,0 0x654 |  | 8:490 | — |
| GroupHeader1 | Line41 | Line |  | 1752,0 7197x0 |  | 9:502 | — |
| Detail | NrFact | TextBox | NrFact | 3468,0 2040x270 |  | 1:522 | 1:486 / 0,0 2040x270 |
| Detail | Data | TextBox | DataFc | 5508,0 1125x270 |  | 2:544 | 2:506 / 2040,0 1125x270 |
| Detail | Total | TextBox | Total | 7446,0 1503x270 |  | 3:567 | 3:529 / 7443,0 1503x270 |
| Detail | Numar | TextBox | Numar | 6633,0 813x270 |  | 4:593 | 4:555 / 3165,0 813x270 |
| Detail | Line30 | Line |  | 3468,0 0x270 |  | 5:618 | 6:606 / 0,0 0x270 |
| Detail | Line31 | Line |  | 6633,0 0x270 |  | 6:630 | 7:615 / 3978,0 0x270 |
| Detail | Line32 | Line |  | 7446,0 0x270 |  | 7:642 | 8:627 / 8946,0 0x270 |
| Detail | Line33 | Line |  | 8949,0 0x270 |  | 8:654 | 9:639 / 7443,0 0x270 |
| Detail | Line40 | Line |  | 1752,0 0x270 |  | 9:666 | — |
| Detail | Line42 | Line |  | 0,0 0x270 |  | 10:678 | — |
| Detail | Masina | TextBox | Masina | 3978,0 3465x270 |  | — | 5:580 |
| GroupFooter2 | Text12 | TextBox | =Sum([Total]) | 7446,0 1503x270 |  | 1:700 | — |
| GroupFooter2 | Label13 | TextBox | ="TOTAL " & [Denumire] | 1752,0 5697x270 |  | 2:724 | — |
| GroupFooter2 | Line43 | Line |  | 0,0 0x330 |  | 3:748 | — |
| GroupFooter1 | Text44 | TextBox | =Sum([Total]) | 7446,0 1503x270 |  | 1:767 | — |
| GroupFooter1 | Text45 | TextBox | ="TOTAL " & [Masina] | 0,0 7449x270 |  | 2:791 | — |
| GroupFooter1 | Text12 | TextBox | =Sum([Total]) | 7443,0 1503x270 |  | — | 1:661 |
| GroupFooter1 | Label13 | TextBox | ="TOTAL " & [Denumire] | 0,4 7437x270 |  | — | 2:685 |
| PageFooterSection | Line212 | Line |  | 0,0 8946x0 |  | 1:822 | 1:717 |
| PageFooterSection | Text211 | TextBox | =DLookUp("expl","ver_rap") | 48,0 2736x477 |  | 2:830 | 2:725 |
| PageFooterSection | Text46 | TextBox | ="Pagina " & [Page] & " din " & [Pages] | 3894,0 5040x285 |  | 3:853 | 3:748 |
| ReportFooter | Text18 | TextBox | =Sum([Total]) | 7443,72 1503x330 |  | 1:886 | 1:781 |
| ReportFooter | Label19 | TextBox | ="TOTAL PERIOADA " & fluna(Month([Forms]![sfp]![di])) & "/" & Year([Forms]![sfp]![di]) & " - " & fluna(Month([Forms]![sf | 0,76 7440x330 |  | 2:910 | 2:805 |
