# Trasabilitate schite achiziții, referate și situații de intrări

Convenții și limite: [citirea fișelor consolidate](../INDEX.md#fise-consolidate). SLICE-D003-09, 27.10.2026.

Coloana Evenimente era goală la fiecare control.

## Variante și surse

### Trasabilitate schita raport Plan_Achizitii

Sursa: [Reports/Plan_Achizitii.txt](../../VERCON_ACCESS/Reports/Plan_Achizitii.txt) · imagine: [R_Plan_Achizitii.png](../R_Plan_Achizitii.png)

- Titlu (Caption): Plan anual de achiziții
- Sursa de date (RecordSource): `SELECT Masini.Masina, Masini.IdMasina, Produse.CPV, Nz([RO],[denumirep]) AS Denumireprodus, Sum(Intrari.Cant) AS Cant, Sum(Intrari.ValoareCUTVA) AS ValoareCUTVA FROM (((Masini INNER JOIN Produse ON Masini.IdMasina = Produse.IdMasina) INNER JOIN (Intrari INNER JOIN ProduseP ON Intrari.IdProdus = ProduseP.IdProdus) ON Produse.ID = ProduseP.ID) INNER JOIN tmpSFP ON Intrari.IdProdus = tmpSFP.CodProdus …`
- Latime: 9039 twips · sectiuni: 7 · controale: 21

### Trasabilitate schita raport Referat_periodic_Intrari

Sursa: [Reports/Referat_periodic_Intrari.txt](../../VERCON_ACCESS/Reports/Referat_periodic_Intrari.txt) · imagine: [R_Referat_periodic_Intrari.png](../R_Referat_periodic_Intrari.png)

- Titlu (Caption): —
- Sursa de date (RecordSource): `SELECT T.IdProdus, T.Denumire, Produse.DenumireP, Produse.UM, T.SumOfCant AS Cant, ProduseP.PUTVA, Sum([sumofcant])*[putva] AS ValoareCUTVA FROM Produse INNER JOIN ((SELECT ProduseP.IdProdus, Furnizori.Denumire, Sum(Intrari.Cant) as SumOfCant FROM Produse INNER JOIN (ProduseP INNER JOIN ((Furnizori INNER JOIN NIR ON Furnizori.IDF = NIR.IDF) INNER JOIN Intrari ON NIR.IdIntrare = Intrari.IdIntrare)  …`
- Latime: 10788 twips · sectiuni: 5 · controale: 21

### Trasabilitate schita raport Referat_periodic_produs

Sursa: [Reports/Referat_periodic_produs.txt](../../VERCON_ACCESS/Reports/Referat_periodic_produs.txt) · imagine: [R_Referat_periodic_produs.png](../R_Referat_periodic_produs.png)

- Titlu (Caption): —
- Sursa de date (RecordSource): `SELECT X.Denumire, X.DenumireP, X.UM, Sum(X.scant) AS Cant, Sum([scant]*[putva]) AS ValoareCUTVA FROM (SELECT T.IdProdus, T.Denumire, Produse.DenumireP, Produse.UM, Sum(T.SumOfCant) AS SCant, ProduseP.PUTVA FROM Produse INNER JOIN ((SELECT ProduseP.IdProdus, Furnizori.Denumire, Sum(Intrari.Cant) AS SumOfCant FROM Produse INNER JOIN ((ProduseP INNER JOIN tmpSFP ON ProduseP.IdProdus = tmpSFP.CodProd …`
- Latime: 10788 twips · sectiuni: 5 · controale: 21

### Trasabilitate schita raport Situatie_Intrari

Sursa: [Reports/Situatie_Intrari.txt](../../VERCON_ACCESS/Reports/Situatie_Intrari.txt) · imagine: [R_Situatie_Intrari.png](../R_Situatie_Intrari.png)

- Titlu (Caption): Situație anuală intrări
- Sursa de date (RecordSource): `SELECT Masini.Masina, Masini.IdMasina, Produse.denumirep AS DenumireProdus, Produse.UM, Sum(Intrari.Cant) AS Cant, Sum(Intrari.ValoareCUTVA) AS ValoareCUTVA FROM ((Masini INNER JOIN Produse ON Masini.IdMasina = Produse.IdMasina) INNER JOIN (Intrari INNER JOIN ProduseP ON Intrari.IdProdus = ProduseP.IdProdus) ON Produse.ID = ProduseP.ID) INNER JOIN tmpSFP ON Intrari.IdProdus = tmpSFP.CodProdus WHER …`
- Latime: 9039 twips · sectiuni: 7 · controale: 21

## Secțiuni

| Secțiune | Plan_Achizitii | Referat_periodic_Intrari | Referat_periodic_produs | Situatie_Intrari |
|---|---|---|---|---|
| ReportHeader | 1 / 0 / 217 | 1 / 0 / 189 | 1 / 0 / 192 | 1 / 0 / 216 |
| PageHeaderSection | 2 / 2004 / 225 | 2 / 2102 / 202 | 2 / 2102 / 205 | 2 / 2004 / 224 |
| GroupHeader0 | 3 / 270 / 405 | — | — | 3 / 270 / 404 |
| Detail | 4 / 280 / 440 | 3 / 255 / 445 | 3 / 255 / 448 | 4 / 280 / 439 |
| GroupFooter0 | 5 / 300 / 585 | — | — | 5 / 300 / 583 |
| PageFooterSection | 6 / 477 / 653 | 4 / 2721 / 573 | 4 / 2721 / 577 | 6 / 477 / 651 |
| ReportFooter | 7 / 432 / 719 | 5 / 360 / 801 | 5 / 360 / 805 | 7 / 432 / 717 |

## Controale

### ReportHeader <a id="reportheader-inaltime-0-twips-linia-217"></a><a id="reportheader-inaltime-0-twips-linia-189"></a><a id="reportheader-inaltime-0-twips-linia-192"></a><a id="reportheader-inaltime-0-twips-linia-216"></a>

_fara controale_

### PageHeaderSection <a id="pageheadersection-inaltime-2004-twips-linia-225"></a><a id="pageheadersection-inaltime-2102-twips-linia-202"></a><a id="pageheadersection-inaltime-2102-twips-linia-205"></a><a id="pageheadersection-inaltime-2004-twips-linia-224"></a>

| Control | Tip | Text / camp / sursa | Pozitie comună (twips) | Plan_Achizitii | Referat_periodic_Intrari | Referat_periodic_produs | Situatie_Intrari |
|---|---|---|---|---|---|---|---|
| DenumireProdus_Label | Label | Denumire Produs | 192,1704 3696x300 | 1:232 | — | — | 1:231 / 192,1704 4560x300 |
| UM_Label | Label | CPV | 3888,1704 1629x300 | 2:260 | — | — | — |
| SumOfCant_Label | Label | Cantitate | 5517,1704 1275x300 | 3:288 | — | — | 3:287 |
| Label13 | Label | Valoare | 6792,1704 1800x300 | 4:316 | — | — | 4:315 |
| Label6 | Label | Situație intrări produse în perioada | 4,0 9015x510 | 5:343 | — | — | 5:342 |
| Text11 | TextBox | =[Forms]![sfp]![di] & " - " & [Forms]![sfp]![dsf] | 0,506 9021x510 | 6:364 | — | — | 6:363 |
| Text17 | TextBox | =DLookUp("Firma","caleTemp") | 4,1020 9021x510 | 7:383 | — | — | 7:382 |
| Text2 | TextBox | =DLookUp("firma","caletemp") | 0,0 4320x579 | — | 1:213 | 1:216 | — |
| Label4 | Label | APROBAT, ORDONATOR DE CREDITE | 6975,0 3795x570 | — | 2:237 | 2:240 | — |
| Label5 | Label | REFERAT DE ACHIZIȚIE | 0,795 10770x285 | — | 3:261 | 3:264 | — |
| Label6 | TextBox | =" PENTRU BUNA DESFĂȘURARE A ACTIVITĂȚII ÎN CADRUL UNITĂȚII <B>" & DLookUp("FIRMA","CALETEMP") & "</B>, ESTE NECESAR A S | 0,1077 10770x615 | — | 4:287 | 4:290 | — |
| Label7 | Label | NR. CRT. | 1303,1814 915x285 | — | 5:319 | 5:322 | — |
| Label8 | Label | Denumire | 2218,1815 4425x285 | — | 6:350 | 6:353 | — |
| Label9 | Label | U/ M | 6643,1816 840x285 | — | 7:381 | 7:384 | — |
| Label10 | Label | Cantitate | 7483,1817 1365x285 | — | 8:412 | 8:415 | — |
| UM_Label | Label | UM | 4752,1704 765x300 | — | — | — | 2:259 |

### GroupHeader0 <a id="groupheader0-inaltime-270-twips-linia-405"></a><a id="groupheader0-inaltime-270-twips-linia-404"></a>

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Plan_Achizitii | Referat_periodic_Intrari | Referat_periodic_produs | Situatie_Intrari |
|---|---|---|---|---|---|---|---|
| Masina | TextBox | Masina | 192,0 8397x270 | 1:415 | — | — | 1:414 |

### Detail <a id="detail-inaltime-280-twips-linia-440"></a><a id="detail-inaltime-255-twips-linia-445"></a><a id="detail-inaltime-255-twips-linia-448"></a><a id="detail-inaltime-280-twips-linia-439"></a>

| Control | Tip | Text / camp / sursa | Pozitie comună (twips) | Plan_Achizitii | Referat_periodic_Intrari | Referat_periodic_produs | Situatie_Intrari |
|---|---|---|---|---|---|---|---|
| DenumireProdus | TextBox | Denumireprodus | 192,1 3696x279 | 1:448 | — | — | — |
| UM | TextBox | CPV | 3888,1 1629x279 | 2:474 | — | — | — |
| Cant | TextBox | Cant | 5517,1 1275x279 | 3:501 | — | — | 3:499 |
| Valoare | TextBox | ValoareCUTVA | 6792,0 1800x279 | 4:530 | — | — | 4:528 |
| Line21 | Line |  | 192,0 0x279 | 5:557 | — | — | 5:555 |
| Line22 | Line |  | 8592,0 0x279 | 6:570 | — | — | 6:568 |
| DenumireProdus | TextBox | DenumireP | 2218,0 4425x255 | — | 1:454 | 1:457 | — |
| UM | TextBox | UM | 6643,0 840x255 | — | 2:481 | 2:485 | 2:473 / 4752,1 765x279 |
| SumOfCant | TextBox | Cant | 7483,0 1365x255 | — | 3:510 | 3:514 | — |
| Text14 | TextBox | =1 | 1303,0 915x255 | — | 4:541 | 4:545 | — |
| DenumireProdus | TextBox | DenumireProdus | 192,1 4560x279 | — | — | — | 1:447 |

### GroupFooter0 <a id="groupfooter0-inaltime-300-twips-linia-585"></a><a id="groupfooter0-inaltime-300-twips-linia-583"></a>

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Plan_Achizitii | Referat_periodic_Intrari | Referat_periodic_produs | Situatie_Intrari |
|---|---|---|---|---|---|---|---|
| Text19 | TextBox | =Sum([ValoareCUTVA]) | 6792,0 1800x300 | 1:596 | — | — | 1:594 |
| Label20 | Label | Total produse perioada / gestiunea | 192,0 6600x300 | 2:624 | — | — | 2:622 |

### PageFooterSection <a id="pagefootersection-inaltime-477-twips-linia-653"></a><a id="pagefootersection-inaltime-2721-twips-linia-573"></a><a id="pagefootersection-inaltime-2721-twips-linia-577"></a><a id="pagefootersection-inaltime-477-twips-linia-651"></a>

| Control | Tip | Text / camp / sursa | Pozitie comună (twips) | Plan_Achizitii | Referat_periodic_Intrari | Referat_periodic_produs | Situatie_Intrari |
|---|---|---|---|---|---|---|---|
| Label107 | TextBox | =DLookUp("expl","ver_rap") | 0,0 2736x477 | 1:660 | 7:752 / 0,2244 2736x477 | 7:756 / 0,2244 2736x477 | 1:658 |
| Line23 | Line |  | 24,0 9015x0 | 2:680 | 8:785 / 24,2244 10764x0 | 8:789 / 24,2244 10764x0 | 2:678 |
| Text46 | TextBox | ="Pagina " & [Page] & " din " & [Pages] | 3999,0 5040x285 | 3:695 | — | — | 3:693 |
| Text15 | TextBox | ="  MENȚIONEZ CĂ PRODUSELE / SERVICIILE / LUCRĂRILE DESCRISE MAI SUS SUNT CUPRINSE ÎN PLANUL ANUAL DE ACHIZIȚII AL UNITĂ | 0,0 10770x570 | — | 1:583 | 1:587 | — |
| Text16 | TextBox | ="      VALOARE ESTIMATĂ A PRODUSELOR ESTE DE <B>" & Format(Round([v],zec()),"Standard") & " LEI </B>CU TVA INCLUS." | 0,570 10770x285 | — | 2:612 | 2:616 | — |
| Label17 | Label | DATA: _________________ | 0,960 2835x570 | — | 3:643 | 3:647 | — |
| Label18 | Label | ÎNTOCMIT, | 0,1530 2835x285 | — | 4:669 | 4:673 | — |
| Text19 | TextBox | =DLookUp("Reprezentant","CALE","ID=" & DLookUp("ID","caletemp")) | 0,1810 2841x315 | — | 5:695 | 5:699 | — |
| Label21 | Label | VIZĂ CFP, ________________________ | 7937,1527 2835x615 | — | 6:724 | 6:728 | — |

### ReportFooter <a id="reportfooter-inaltime-432-twips-linia-719"></a><a id="reportfooter-inaltime-360-twips-linia-801"></a><a id="reportfooter-inaltime-360-twips-linia-805"></a><a id="reportfooter-inaltime-432-twips-linia-717"></a>

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Plan_Achizitii | Referat_periodic_Intrari | Referat_periodic_produs | Situatie_Intrari |
|---|---|---|---|---|---|---|---|
| Text14 | TextBox | =Sum([ValoareCUTVA]) | 6792,56 1800x315 | 1:727 | — | — | 1:725 |
| V | TextBox | =Sum([Valoarecutva]) | 3685,0 1701x225 | — | 1:814 | 1:818 | — |
| Label15 | Label | Total produse perioada | 192,56 6600x315 | 2:755 | — | — | 2:753 |
