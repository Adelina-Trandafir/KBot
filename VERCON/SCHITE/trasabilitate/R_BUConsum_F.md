# Trasabilitate schita raport BUConsum_F

Sursa: [Reports/BUConsum_F.txt](../../VERCON_ACCESS/Reports/BUConsum_F.txt) · imagine: [R_BUConsum_F.png](../R_BUConsum_F.png)

- Titlu (Caption): —
- Sursa de date (RecordSource): `SELECT Day([databc]) AS Ziua, Month([databc]) AS Luna, Year([databc]) AS Anul, Iesiri_H.NumarBC AS NrFact, Iesiri.Denumire, Sum(Iesiri.Cant) AS Cant, Iesiri.CodProdus, Iesiri.UM, Sum(Iesiri.Cant) AS SumOfCant, Round([PUTVA],[tempvars]![zec]) AS Pret, Sum(Round([Iesiri]![cant]*[putva],[tempvars]![zec])) AS Valoare, Clienti.Denumire AS cDenumire, Iesiri_H.Expl, Masini.Masina, Furnizori.Denumire AS F …`
- Latime: 11189 twips · sectiuni: 5 · controale: 4

## PageHeaderSection (inaltime 0 twips, linia 472)

_fara controale_

## GroupHeader0 (inaltime 1704 twips, linia 479)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| BUConsum_F1 | Subform | Report.BUConsum_F1 | 0,0 11189x1340 | 488 |  |
| Line59 | Line |  | 0,1536 11189x0 | 502 |  |
| Label63 | Label |  | 0,1392 600x312 | 516 |  |

## Detail (inaltime 0 twips, linia 534)

_fara controale_

## GroupFooter1 (inaltime 1532 twips, linia 542)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Child58 | Subform | Report.BUConsum_F1 | 0,192 11189x1340 | 553 |  |

## PageFooterSection (inaltime 0 twips, linia 571)

_fara controale_

