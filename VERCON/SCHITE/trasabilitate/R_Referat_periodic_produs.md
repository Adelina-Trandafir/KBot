# Trasabilitate schita raport Referat_periodic_produs

Sursa: [Reports/Referat_periodic_produs.txt](../../VERCON_ACCESS/Reports/Referat_periodic_produs.txt) · imagine: [R_Referat_periodic_produs.png](../R_Referat_periodic_produs.png)

- Titlu (Caption): —
- Sursa de date (RecordSource): `SELECT X.Denumire, X.DenumireP, X.UM, Sum(X.scant) AS Cant, Sum([scant]*[putva]) AS ValoareCUTVA FROM (SELECT T.IdProdus, T.Denumire, Produse.DenumireP, Produse.UM, Sum(T.SumOfCant) AS SCant, ProduseP.PUTVA FROM Produse INNER JOIN ((SELECT ProduseP.IdProdus, Furnizori.Denumire, Sum(Intrari.Cant) AS SumOfCant FROM Produse INNER JOIN ((ProduseP INNER JOIN tmpSFP ON ProduseP.IdProdus = tmpSFP.CodProd …`
- Latime: 10788 twips · sectiuni: 5 · controale: 21

## ReportHeader (inaltime 0 twips, linia 192)

_fara controale_

## PageHeaderSection (inaltime 2102 twips, linia 205)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text2 | TextBox | =DLookUp("firma","caletemp") | 0,0 4320x579 | 216 |  |
| Label4 | Label | APROBAT, ORDONATOR DE CREDITE | 6975,0 3795x570 | 240 |  |
| Label5 | Label | REFERAT DE ACHIZIȚIE | 0,795 10770x285 | 264 |  |
| Label6 | TextBox | =" PENTRU BUNA DESFĂȘURARE A ACTIVITĂȚII ÎN CADRUL UNITĂȚII <B>" & DLookUp("FIRMA","CALETEMP") & "</B>, ESTE NECESAR A S | 0,1077 10770x615 | 290 |  |
| Label7 | Label | NR. CRT. | 1303,1814 915x285 | 322 |  |
| Label8 | Label | Denumire | 2218,1815 4425x285 | 353 |  |
| Label9 | Label | U/ M | 6643,1816 840x285 | 384 |  |
| Label10 | Label | Cantitate | 7483,1817 1365x285 | 415 |  |

## Detail (inaltime 255 twips, linia 448)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| DenumireProdus | TextBox | DenumireP | 2218,0 4425x255 | 457 |  |
| UM | TextBox | UM | 6643,0 840x255 | 485 |  |
| SumOfCant | TextBox | Cant | 7483,0 1365x255 | 514 |  |
| Text14 | TextBox | =1 | 1303,0 915x255 | 545 |  |

## PageFooterSection (inaltime 2721 twips, linia 577)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text15 | TextBox | ="  MENȚIONEZ CĂ PRODUSELE / SERVICIILE / LUCRĂRILE DESCRISE MAI SUS SUNT CUPRINSE ÎN PLANUL ANUAL DE ACHIZIȚII AL UNITĂ | 0,0 10770x570 | 587 |  |
| Text16 | TextBox | ="      VALOARE ESTIMATĂ A PRODUSELOR ESTE DE <B>" & Format(Round([v],zec()),"Standard") & " LEI </B>CU TVA INCLUS." | 0,570 10770x285 | 616 |  |
| Label17 | Label | DATA: _________________ | 0,960 2835x570 | 647 |  |
| Label18 | Label | ÎNTOCMIT, | 0,1530 2835x285 | 673 |  |
| Text19 | TextBox | =DLookUp("Reprezentant","CALE","ID=" & DLookUp("ID","caletemp")) | 0,1810 2841x315 | 699 |  |
| Label21 | Label | VIZĂ CFP, ________________________ | 7937,1527 2835x615 | 728 |  |
| Label107 | TextBox | =DLookUp("expl","ver_rap") | 0,2244 2736x477 | 756 |  |
| Line23 | Line |  | 24,2244 10764x0 | 789 |  |

## ReportFooter (inaltime 360 twips, linia 805)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| V | TextBox | =Sum([Valoarecutva]) | 3685,0 1701x225 | 818 |  |

