# Trasabilitate schita raport Nota_Comanda

Sursa: [Reports/Nota_Comanda.txt](../../VERCON_ACCESS/Reports/Nota_Comanda.txt) · imagine: [R_Nota_Comanda.png](../R_Nota_Comanda.png)

- Titlu (Caption): —
- Sursa de date (RecordSource): `SELECT Furnizori.Denumire, NIR.DataFc, Produse.denumirep AS DenumireProdus, Produse.UM, Sum(Intrari.Cant) AS SumOfCant, Sum(Intrari.ValoareCUTVA) AS SumOfValoareCUTVA FROM (Produse INNER JOIN ((Furnizori INNER JOIN (NIR INNER JOIN Intrari ON NIR.IdIntrare = Intrari.IdIntrare) ON Furnizori.IDF = NIR.IDF) INNER JOIN ProduseP ON Intrari.IdProdus = ProduseP.IdProdus) ON Produse.ID = ProduseP.ID) INNER …`
- Latime: 10788 twips · sectiuni: 5 · controale: 19

## ReportHeader (inaltime 0 twips, linia 163)

_fara controale_

## PageHeaderSection (inaltime 2987 twips, linia 171)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text2 | TextBox | =DLookUp("firma","caletemp") | 0,0 4176x285 | 178 |  |
| Label6 | TextBox | ="CĂTRE <B>" & [DENUMIRE] & "</B>" | 0,1962 10770x345 | 195 |  |
| Label7 | Label | NR. CRT. | 1303,2699 915x285 | 216 |  |
| Label8 | Label | Denumire | 2218,2700 4425x285 | 239 |  |
| Label9 | Label | U/ M | 6643,2701 840x285 | 262 |  |
| Label10 | Label | Cantitate | 7483,2702 1365x285 | 285 |  |
| Text23 | TextBox | ="CUI: " & DLookUp("CUI","cale","ID=" & DLookUp("ID","caletemp")) | 0,285 4176x285 | 308 |  |
| Label5 | TextBox | ="NOTĂ DE COMANDĂ" | 0,1680 10770x285 | 327 |  |
| Label24 | Label | Aprobat, DIRECTOR | 6504,144 1215x792 | 350 |  |
| Label25 | Label | Administrator financiar | 7992,144 2445x792 | 368 |  |

## Detail (inaltime 255 twips, linia 388)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| DenumireProdus | TextBox | DenumireProdus | 2218,0 4425x255 | 396 |  |
| UM | TextBox | UM | 6643,0 840x255 | 418 |  |
| SumOfCant | TextBox | SumOfCant | 7483,0 1365x255 | 442 |  |
| Text14 | TextBox | =1 | 1303,0 915x255 | 467 |  |

## PageFooterSection (inaltime 1461 twips, linia 493)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label18 | Label | ÎNTOCMIT, | 3750,210 2865x285 | 500 |  |
| Line23 | Line |  | 24,984 10764x0 | 520 |  |
| Text19 | Label | Responsabil achiziții | 3768,496 2841x315 | 534 |  |
| Label107 | TextBox | =DLookUp("expl","ver_rap") | 0,963 2736x477 | 554 |  |

## ReportFooter (inaltime 225 twips, linia 580)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| V | TextBox | =Sum([SumOfValoareCUTVA]) | 3685,0 1701x225 | 589 |  |

