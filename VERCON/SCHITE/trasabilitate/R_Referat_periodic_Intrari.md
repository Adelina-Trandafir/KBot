# Trasabilitate schita raport Referat_periodic_Intrari

Sursa: [Reports/Referat_periodic_Intrari.txt](../../VERCON_ACCESS/Reports/Referat_periodic_Intrari.txt) · imagine: [R_Referat_periodic_Intrari.png](../R_Referat_periodic_Intrari.png)

- Titlu (Caption): —
- Sursa de date (RecordSource): `SELECT T.IdProdus, T.Denumire, Produse.DenumireP, Produse.UM, T.SumOfCant AS Cant, ProduseP.PUTVA, Sum([sumofcant])*[putva] AS ValoareCUTVA FROM Produse INNER JOIN ((SELECT ProduseP.IdProdus, Furnizori.Denumire, Sum(Intrari.Cant) as SumOfCant FROM Produse INNER JOIN (ProduseP INNER JOIN ((Furnizori INNER JOIN NIR ON Furnizori.IDF = NIR.IDF) INNER JOIN Intrari ON NIR.IdIntrare = Intrari.IdIntrare)  …`
- Latime: 10788 twips · sectiuni: 5 · controale: 21

## ReportHeader (inaltime 0 twips, linia 189)

_fara controale_

## PageHeaderSection (inaltime 2102 twips, linia 202)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text2 | TextBox | =DLookUp("firma","caletemp") | 0,0 4320x579 | 213 |  |
| Label4 | Label | APROBAT, ORDONATOR DE CREDITE | 6975,0 3795x570 | 237 |  |
| Label5 | Label | REFERAT DE ACHIZIȚIE | 0,795 10770x285 | 261 |  |
| Label6 | TextBox | =" PENTRU BUNA DESFĂȘURARE A ACTIVITĂȚII ÎN CADRUL UNITĂȚII <B>" & DLookUp("FIRMA","CALETEMP") & "</B>, ESTE NECESAR A S | 0,1077 10770x615 | 287 |  |
| Label7 | Label | NR. CRT. | 1303,1814 915x285 | 319 |  |
| Label8 | Label | Denumire | 2218,1815 4425x285 | 350 |  |
| Label9 | Label | U/ M | 6643,1816 840x285 | 381 |  |
| Label10 | Label | Cantitate | 7483,1817 1365x285 | 412 |  |

## Detail (inaltime 255 twips, linia 445)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| DenumireProdus | TextBox | DenumireP | 2218,0 4425x255 | 454 |  |
| UM | TextBox | UM | 6643,0 840x255 | 481 |  |
| SumOfCant | TextBox | Cant | 7483,0 1365x255 | 510 |  |
| Text14 | TextBox | =1 | 1303,0 915x255 | 541 |  |

## PageFooterSection (inaltime 2721 twips, linia 573)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text15 | TextBox | ="  MENȚIONEZ CĂ PRODUSELE / SERVICIILE / LUCRĂRILE DESCRISE MAI SUS SUNT CUPRINSE ÎN PLANUL ANUAL DE ACHIZIȚII AL UNITĂ | 0,0 10770x570 | 583 |  |
| Text16 | TextBox | ="      VALOARE ESTIMATĂ A PRODUSELOR ESTE DE <B>" & Format(Round([v],zec()),"Standard") & " LEI </B>CU TVA INCLUS." | 0,570 10770x285 | 612 |  |
| Label17 | Label | DATA: _________________ | 0,960 2835x570 | 643 |  |
| Label18 | Label | ÎNTOCMIT, | 0,1530 2835x285 | 669 |  |
| Text19 | TextBox | =DLookUp("Reprezentant","CALE","ID=" & DLookUp("ID","caletemp")) | 0,1810 2841x315 | 695 |  |
| Label21 | Label | VIZĂ CFP, ________________________ | 7937,1527 2835x615 | 724 |  |
| Label107 | TextBox | =DLookUp("expl","ver_rap") | 0,2244 2736x477 | 752 |  |
| Line23 | Line |  | 24,2244 10764x0 | 785 |  |

## ReportFooter (inaltime 360 twips, linia 801)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| V | TextBox | =Sum([Valoarecutva]) | 3685,0 1701x225 | 814 |  |

