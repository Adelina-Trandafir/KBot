# Trasabilitate schita raport Plan_Achizitii

Sursa: [Reports/Plan_Achizitii.txt](../../VERCON_ACCESS/Reports/Plan_Achizitii.txt) · imagine: [R_Plan_Achizitii.png](../R_Plan_Achizitii.png)

- Titlu (Caption): Plan anual de achiziții
- Sursa de date (RecordSource): `SELECT Masini.Masina, Masini.IdMasina, Produse.CPV, Nz([RO],[denumirep]) AS Denumireprodus, Sum(Intrari.Cant) AS Cant, Sum(Intrari.ValoareCUTVA) AS ValoareCUTVA FROM (((Masini INNER JOIN Produse ON Masini.IdMasina = Produse.IdMasina) INNER JOIN (Intrari INNER JOIN ProduseP ON Intrari.IdProdus = ProduseP.IdProdus) ON Produse.ID = ProduseP.ID) INNER JOIN tmpSFP ON Intrari.IdProdus = tmpSFP.CodProdus …`
- Latime: 9039 twips · sectiuni: 7 · controale: 21

## ReportHeader (inaltime 0 twips, linia 217)

_fara controale_

## PageHeaderSection (inaltime 2004 twips, linia 225)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| DenumireProdus_Label | Label | Denumire Produs | 192,1704 3696x300 | 232 |  |
| UM_Label | Label | CPV | 3888,1704 1629x300 | 260 |  |
| SumOfCant_Label | Label | Cantitate | 5517,1704 1275x300 | 288 |  |
| Label13 | Label | Valoare | 6792,1704 1800x300 | 316 |  |
| Label6 | Label | Situație intrări produse în perioada | 4,0 9015x510 | 343 |  |
| Text11 | TextBox | =[Forms]![sfp]![di] & " - " & [Forms]![sfp]![dsf] | 0,506 9021x510 | 364 |  |
| Text17 | TextBox | =DLookUp("Firma","caleTemp") | 4,1020 9021x510 | 383 |  |

## GroupHeader0 (inaltime 270 twips, linia 405)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Masina | TextBox | Masina | 192,0 8397x270 | 415 |  |

## Detail (inaltime 280 twips, linia 440)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| DenumireProdus | TextBox | Denumireprodus | 192,1 3696x279 | 448 |  |
| UM | TextBox | CPV | 3888,1 1629x279 | 474 |  |
| Cant | TextBox | Cant | 5517,1 1275x279 | 501 |  |
| Valoare | TextBox | ValoareCUTVA | 6792,0 1800x279 | 530 |  |
| Line21 | Line |  | 192,0 0x279 | 557 |  |
| Line22 | Line |  | 8592,0 0x279 | 570 |  |

## GroupFooter0 (inaltime 300 twips, linia 585)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text19 | TextBox | =Sum([ValoareCUTVA]) | 6792,0 1800x300 | 596 |  |
| Label20 | Label | Total produse perioada / gestiunea | 192,0 6600x300 | 624 |  |

## PageFooterSection (inaltime 477 twips, linia 653)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label107 | TextBox | =DLookUp("expl","ver_rap") | 0,0 2736x477 | 660 |  |
| Line23 | Line |  | 24,0 9015x0 | 680 |  |
| Text46 | TextBox | ="Pagina " & [Page] & " din " & [Pages] | 3999,0 5040x285 | 695 |  |

## ReportFooter (inaltime 432 twips, linia 719)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text14 | TextBox | =Sum([ValoareCUTVA]) | 6792,56 1800x315 | 727 |  |
| Label15 | Label | Total produse perioada | 192,56 6600x315 | 755 |  |

