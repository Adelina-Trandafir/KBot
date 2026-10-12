# Trasabilitate schita raport Situatie_Intrari

Sursa: [Reports/Situatie_Intrari.txt](../../VERCON_ACCESS/Reports/Situatie_Intrari.txt) · imagine: [R_Situatie_Intrari.png](../R_Situatie_Intrari.png)

- Titlu (Caption): Situație anuală intrări
- Sursa de date (RecordSource): `SELECT Masini.Masina, Masini.IdMasina, Produse.denumirep AS DenumireProdus, Produse.UM, Sum(Intrari.Cant) AS Cant, Sum(Intrari.ValoareCUTVA) AS ValoareCUTVA FROM ((Masini INNER JOIN Produse ON Masini.IdMasina = Produse.IdMasina) INNER JOIN (Intrari INNER JOIN ProduseP ON Intrari.IdProdus = ProduseP.IdProdus) ON Produse.ID = ProduseP.ID) INNER JOIN tmpSFP ON Intrari.IdProdus = tmpSFP.CodProdus WHER …`
- Latime: 9039 twips · sectiuni: 7 · controale: 21

## ReportHeader (inaltime 0 twips, linia 216)

_fara controale_

## PageHeaderSection (inaltime 2004 twips, linia 224)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| DenumireProdus_Label | Label | Denumire Produs | 192,1704 4560x300 | 231 |  |
| UM_Label | Label | UM | 4752,1704 765x300 | 259 |  |
| SumOfCant_Label | Label | Cantitate | 5517,1704 1275x300 | 287 |  |
| Label13 | Label | Valoare | 6792,1704 1800x300 | 315 |  |
| Label6 | Label | Situație intrări produse în perioada | 4,0 9015x510 | 342 |  |
| Text11 | TextBox | =[Forms]![sfp]![di] & " - " & [Forms]![sfp]![dsf] | 0,506 9021x510 | 363 |  |
| Text17 | TextBox | =DLookUp("Firma","caleTemp") | 4,1020 9021x510 | 382 |  |

## GroupHeader0 (inaltime 270 twips, linia 404)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Masina | TextBox | Masina | 192,0 8397x270 | 414 |  |

## Detail (inaltime 280 twips, linia 439)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| DenumireProdus | TextBox | DenumireProdus | 192,1 4560x279 | 447 |  |
| UM | TextBox | UM | 4752,1 765x279 | 473 |  |
| Cant | TextBox | Cant | 5517,1 1275x279 | 499 |  |
| Valoare | TextBox | ValoareCUTVA | 6792,0 1800x279 | 528 |  |
| Line21 | Line |  | 192,0 0x279 | 555 |  |
| Line22 | Line |  | 8592,0 0x279 | 568 |  |

## GroupFooter0 (inaltime 300 twips, linia 583)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text19 | TextBox | =Sum([ValoareCUTVA]) | 6792,0 1800x300 | 594 |  |
| Label20 | Label | Total produse perioada / gestiunea | 192,0 6600x300 | 622 |  |

## PageFooterSection (inaltime 477 twips, linia 651)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label107 | TextBox | =DLookUp("expl","ver_rap") | 0,0 2736x477 | 658 |  |
| Line23 | Line |  | 24,0 9015x0 | 678 |  |
| Text46 | TextBox | ="Pagina " & [Page] & " din " & [Pages] | 3999,0 5040x285 | 693 |  |

## ReportFooter (inaltime 432 twips, linia 717)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text14 | TextBox | =Sum([ValoareCUTVA]) | 6792,56 1800x315 | 725 |  |
| Label15 | Label | Total produse perioada | 192,56 6600x315 | 753 |  |

