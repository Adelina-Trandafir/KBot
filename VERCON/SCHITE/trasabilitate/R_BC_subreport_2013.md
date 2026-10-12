# Trasabilitate schita raport BC subreport 2013

Sursa: [Reports/BC subreport 2013.txt](../../VERCON_ACCESS/Reports/BC%20subreport%202013.txt) · imagine: [R_BC_subreport_2013.png](../R_BC_subreport_2013.png)

- Titlu (Caption): —
- Sursa de date (RecordSource): `SELECT T.IdLista, Produse.DenumireP, Produse.UM, ProduseP.PUTVA, Sum(T.Cant) AS CantT, Sum([cant])*[putva] AS Valoare FROM Produse INNER JOIN ((SELECT LegBCLA.IdLista, Iesiri.IdProdus, Iesiri.Cant FROM (LegBCLA INNER JOIN Iesiri_H ON LegBCLA.IdBonConsum = Iesiri_H.IdBonConsum) INNER JOIN Iesiri ON Iesiri_H.IdBonConsum = Iesiri.IdBonConsum WHERE LegBCLA.IdLista=[Forms]![ListaAlimente]![IDLISTA] UNI …`
- Latime: 5130 twips · sectiuni: 3 · controale: 7

## ReportHeader (inaltime 0 twips, linia 424)

_fara controale_

## Detail (inaltime 255 twips, linia 436)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Produse.Denumire | TextBox | DenumireP | 0,0 2205x255 | 444 |  |
| UM | TextBox | UM | 2205,0 465x255 | 464 |  |
| Text35 | TextBox | CantT | 3225,0 555x255 | 486 |  |
| Pret | TextBox | PUTVA | 3780,0 675x255 | 510 |  |
| Valoare | TextBox | Valoare | 4455,0 675x255 | 534 |  |
| Text48 | TextBox | CantT | 2670,0 555x255 | 558 |  |

## ReportFooter (inaltime 255 twips, linia 584)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| SumValoare | TextBox | =Sum([Valoare]) | 4455,0 675x255 | 593 |  |

