# Trasabilitate schite consumuri pe produs și pe unități

Convenții și limite: [citirea fișelor consolidate](../INDEX.md#fise-consolidate). SLICE-D003-09, 27.10.2026.

Comune tuturor: Titlu (Caption): —; lățime 9525 twips. Coloana Evenimente era goală la fiecare control.

## Variante și surse

### Trasabilitate schita raport Consumuri_Produs

Sursa: [Reports/Consumuri_Produs.txt](../../VERCON_ACCESS/Reports/Consumuri_Produs.txt) · imagine: [R_Consumuri_Produs.png](../R_Consumuri_Produs.png)

- Sursa de date (RecordSource): `SELECT X.IdMasina, X.Masina, X.CodProdus, X.Denumire, X.UM, X.Cant, X.PUTVA, [Cant]*[putva] AS Valoare FROM (SELECT Masini.IdMasina, Masini.Masina, ProduseP.IdProdus AS Codprodus, Produse.denumirep AS Denumire, Produse.UM, Sum(Iesiri.Cant) AS Cant, Iesiri.PUTVA FROM ((Masini INNER JOIN Produse ON Masini.IdMasina = Produse.IdMasina) INNER JOIN (((Clienti INNER JOIN Iesiri_H ON Clienti.IDC = Iesiri_ …`
- sectiuni: 7 · controale: 31

### Trasabilitate schita raport Consumuri_Unitati

Sursa: [Reports/Consumuri_Unitati.txt](../../VERCON_ACCESS/Reports/Consumuri_Unitati.txt) · imagine: [R_Consumuri_Unitati.png](../R_Consumuri_Unitati.png)

- Sursa de date (RecordSource): `SELECT X.IDC, X.Primitor, X.IdMasina, X.Masina, X.CodProdus, X.Denumire, X.UM, X.Cant, X.PUTVA, [Cant]*[putva] AS Valoare FROM (SELECT Masini.IdMasina, Masini.Masina, Clienti.IDC, Clienti.Denumire AS Primitor, Iesiri.CodProdus, Iesiri.Denumire, Iesiri.UM, Sum(Iesiri.Cant) AS Cant, Iesiri.PUTVA FROM Masini INNER JOIN ((Clienti INNER JOIN Iesiri_H ON Clienti.IDC = Iesiri_H.IDC) INNER JOIN Iesiri ON  …`
- sectiuni: 7 · controale: 31

## Secțiuni

| Secțiune | Consumuri_Produs | Consumuri_Unitati |
|---|---|---|
| PageHeaderSection | 1 / 2244 / 203 | 1 / 2244 / 202 |
| GroupHeader1 | 2 / 1152 / 488 | 2 / 1152 / 487 |
| GroupHeader0 | 3 / 264 / 652 | 3 / 264 / 651 |
| Detail | 4 / 264 / 849 | 4 / 264 / 848 |
| GroupFooter1 | 5 / 72 / 973 | 5 / 72 / 972 |
| GroupFooter2 | 6 / 0 / 1013 | 6 / 0 / 1012 |
| PageFooterSection | 7 / 501 / 1028 | 7 / 501 / 1027 |

## Controale

### PageHeaderSection <a id="pageheadersection-inaltime-2244-twips-linia-203"></a><a id="pageheadersection-inaltime-2244-twips-linia-202"></a>

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Consumuri_Produs | Consumuri_Unitati |
|---|---|---|---|---|---|
| Text18 | TextBox | =DLookUp("Firma","caletemp") | 0,0 9522x306 | 1:213 | 1:212 |
| Text49 | TextBox | =[Forms]![sfp]![di] & " - " & [Forms]![sfp]![dsf] | 0,984 9522x450 | 2:242 | 2:241 |
| Label19 | Label | Situație consumuri / conturi | 0,504 9522x480 | 3:275 | 3:274 |
| L1 | Label | Denumire produs | 408,1968 3840x276 | 4:304 | 4:303 |
| L2 | Label | U/M | 4248,1968 960x276 | 5:334 | 5:333 |
| L3 | Label | Preț unitar | 6456,1968 1380x276 | 6:364 | 6:363 |
| L4 | Label | Valoare | 7836,1968 1380x276 | 7:394 | 7:393 |
| Masina | TextBox | Masina | 0,1608 9525x288 | 8:424 | — |
| Primitor | TextBox | Primitor | 0,1608 9525x288 | — | 8:423 |
| Label26 | Label | Cant. | 5208,1968 1248x276 | 9:456 | 9:455 |

### GroupHeader1 <a id="groupheader1-inaltime-1152-twips-linia-488"></a><a id="groupheader1-inaltime-1152-twips-linia-487"></a>

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Consumuri_Produs | Consumuri_Unitati |
|---|---|---|---|---|---|
| Label0 | Label | Denumire produs | 408,480 3840x276 | 1:500 | 1:499 |
| Label1 | Label | U/M | 4248,480 960x276 | 2:530 | 2:529 |
| Label2 | Label | Preț unitar | 6456,480 1380x276 | 3:560 | 3:559 |
| Label3 | Label | Valoare | 7836,480 1380x276 | 4:590 | 4:589 |
| Label23 | Label | U/M | 5208,480 1248x276 | 5:620 | 5:619 |

### GroupHeader0 <a id="groupheader0-inaltime-264-twips-linia-652"></a><a id="groupheader0-inaltime-264-twips-linia-651"></a>

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Consumuri_Produs | Consumuri_Unitati |
|---|---|---|---|---|---|
| Cantx | TextBox | =Sum([Cant]) | 5208,0 1248x264 | 1:664 | 1:663 |
| Denumirex | TextBox | =UCase([Denumire]) | 408,0 3840x264 | 2:694 | 2:693 |
| UM | TextBox | UM | 4248,0 960x264 | 3:721 | 3:720 |
| Valoarex | TextBox | =Sum([Valoare]) | 7836,0 1380x264 | 4:748 | 4:747 |
| Box8 | Rectangle |  | 6456,0 1380x264 | 5:778 | 5:777 |
| Line13 | Line |  | 408,0 0x264 | 6:795 | 6:794 |
| Line14 | Line |  | 9216,0 0x264 | 7:808 | 7:807 |
| Label17 | Label |  | 408,0 8784x0 | 8:821 | 8:820 |

### Detail <a id="detail-inaltime-264-twips-linia-849"></a><a id="detail-inaltime-264-twips-linia-848"></a>

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Consumuri_Produs | Consumuri_Unitati |
|---|---|---|---|---|---|
| Valoare | TextBox | Valoare | 7836,0 1380x264 | 1:858 | 1:857 |
| PUTVA | TextBox | PUTVA | 6456,0 1380x264 | 2:887 | 2:886 |
| Line15 | Line |  | 408,0 0x264 | 3:916 | 3:915 |
| Line16 | Line |  | 9216,0 0x264 | 4:929 | 4:928 |
| Cant | TextBox | Cant | 5208,0 1248x264 | 5:942 | 5:941 |

### GroupFooter1 <a id="groupfooter1-inaltime-72-twips-linia-973"></a><a id="groupfooter1-inaltime-72-twips-linia-972"></a>

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Consumuri_Produs | Consumuri_Unitati |
|---|---|---|---|---|---|
| Label12 | Label |  | 408,0 8784x0 | 1:985 | 1:984 |

### GroupFooter2 <a id="groupfooter2-inaltime-0-twips-linia-1013"></a><a id="groupfooter2-inaltime-0-twips-linia-1012"></a>

_fara controale_

### PageFooterSection <a id="pagefootersection-inaltime-501-twips-linia-1028"></a><a id="pagefootersection-inaltime-501-twips-linia-1027"></a>

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Consumuri_Produs | Consumuri_Unitati |
|---|---|---|---|---|---|
| Text50 | TextBox | ="Pagina " & [Page] & " din " & [Pages] | 7125,24 2400x285 | 1:1039 | 1:1038 |
| Label107 | TextBox | =DLookUp("expl","ver_rap") | 0,24 2736x477 | 2:1071 | 2:1070 |
| Line51 | Line |  | 0,0 9522x0 | 3:1104 | 3:1103 |
