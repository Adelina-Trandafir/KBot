# Trasabilitate schita raport Fisa_Produsului

Sursa: [Reports/Fisa_Produsului.txt](../../VERCON_ACCESS/Reports/Fisa_Produsului.txt) · imagine: [R_Fisa_Produsului.png](../R_Fisa_Produsului.png)

- Titlu (Caption): —
- Sursa de date (RecordSource): `SELECT Masini.Masina, Produse.DenumireP AS DENP, Produse.UM AS UMP, ProduseP.PUTVA AS PT, Sum([intrari]+[retur]) AS S1, Sum([iesiri]+[storno]) AS S2, Sum(([intrari]+[retur])-([iesiri]+[storno])) AS SF FROM (Masini INNER JOIN Produse ON Masini.IdMasina = Produse.IdMasina) INNER JOIN (QSFP_BL INNER JOIN ProduseP ON QSFP_BL.IdProdus = ProduseP.IdProdus) ON Produse.ID = ProduseP.ID GROUP BY Masini.Mas …`
- Latime: 5042 twips · sectiuni: 3 · controale: 26

## GroupHeader0 (inaltime 1407 twips, linia 207)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| DEN | TextBox | DENP | 978,120 3936x270 | 216 |  |
| Label2 | Label | Denumire  | 96,120 882x270 | 240 |  |
| UM | TextBox | UMP | 978,464 3936x270 | 261 |  |
| Label3 | Label | U / M | 96,464 882x270 | 286 |  |
| Masina | TextBox | Masina | 978,791 3936x270 | 307 |  |
| Label6 | Label | Contul | 96,791 882x270 | 333 |  |
| Label4 | Label | Preț unitar | 192,1152 1146x255 | 354 |  |
| Label7 | Label | Intrări | 1338,1152 1146x255 | 379 |  |
| Label11 | Label | Ieșiri | 2484,1152 1146x255 | 404 |  |
| Label13 | Label | Stoc final | 3630,1152 1146x255 | 429 |  |
| Line32 | Line |  | 0,0 5042x0 | 454 |  |
| Line33 | Line |  | 5040,0 0x1407 | 463 |  |
| Line34 | Line |  | 0,0 0x1407 | 476 |  |

## Detail (inaltime 240 twips, linia 488)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| PU | TextBox | PT | 192,0 1146x240 | 496 |  |
| Text12 | TextBox | Sf | 3630,0 1146x240 | 522 |  |
| Text10 | TextBox | S2 | 2484,0 1146x240 | 548 |  |
| S1 | TextBox | S1 | 1338,0 1146x240 | 574 |  |
| Line35 | Line |  | 0,0 0x240 | 600 |  |
| Line36 | Line |  | 5040,0 0x240 | 610 |  |

## GroupFooter1 (inaltime 504 twips, linia 625)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label25 | Label | TOTAL | 192,0 1146x240 | 634 |  |
| Text26 | TextBox | =Sum([Sf]) | 3630,0 1146x240 | 657 |  |
| Text27 | TextBox | =Sum([S2]) | 2484,0 1146x240 | 683 |  |
| Text28 | TextBox | =Sum([S1]) | 1338,0 1146x240 | 710 |  |
| Line14 | Line |  | 0,336 5042x0 | 737 |  |
| Line39 | Line |  | 5040,0 0x348 | 749 |  |
| Line40 | Line |  | 0,0 0x348 | 762 |  |

