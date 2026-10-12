# Trasabilitate schita raport Consumuri_Unitati

Sursa: [Reports/Consumuri_Unitati.txt](../../VERCON_ACCESS/Reports/Consumuri_Unitati.txt) · imagine: [R_Consumuri_Unitati.png](../R_Consumuri_Unitati.png)

- Titlu (Caption): —
- Sursa de date (RecordSource): `SELECT X.IDC, X.Primitor, X.IdMasina, X.Masina, X.CodProdus, X.Denumire, X.UM, X.Cant, X.PUTVA, [Cant]*[putva] AS Valoare FROM (SELECT Masini.IdMasina, Masini.Masina, Clienti.IDC, Clienti.Denumire AS Primitor, Iesiri.CodProdus, Iesiri.Denumire, Iesiri.UM, Sum(Iesiri.Cant) AS Cant, Iesiri.PUTVA FROM Masini INNER JOIN ((Clienti INNER JOIN Iesiri_H ON Clienti.IDC = Iesiri_H.IDC) INNER JOIN Iesiri ON  …`
- Latime: 9525 twips · sectiuni: 7 · controale: 31

## PageHeaderSection (inaltime 2244 twips, linia 202)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text18 | TextBox | =DLookUp("Firma","caletemp") | 0,0 9522x306 | 212 |  |
| Text49 | TextBox | =[Forms]![sfp]![di] & " - " & [Forms]![sfp]![dsf] | 0,984 9522x450 | 241 |  |
| Label19 | Label | Situație consumuri / conturi | 0,504 9522x480 | 274 |  |
| L1 | Label | Denumire produs | 408,1968 3840x276 | 303 |  |
| L2 | Label | U/M | 4248,1968 960x276 | 333 |  |
| L3 | Label | Preț unitar | 6456,1968 1380x276 | 363 |  |
| L4 | Label | Valoare | 7836,1968 1380x276 | 393 |  |
| Primitor | TextBox | Primitor | 0,1608 9525x288 | 423 |  |
| Label26 | Label | Cant. | 5208,1968 1248x276 | 455 |  |

## GroupHeader1 (inaltime 1152 twips, linia 487)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label0 | Label | Denumire produs | 408,480 3840x276 | 499 |  |
| Label1 | Label | U/M | 4248,480 960x276 | 529 |  |
| Label2 | Label | Preț unitar | 6456,480 1380x276 | 559 |  |
| Label3 | Label | Valoare | 7836,480 1380x276 | 589 |  |
| Label23 | Label | U/M | 5208,480 1248x276 | 619 |  |

## GroupHeader0 (inaltime 264 twips, linia 651)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Cantx | TextBox | =Sum([Cant]) | 5208,0 1248x264 | 663 |  |
| Denumirex | TextBox | =UCase([Denumire]) | 408,0 3840x264 | 693 |  |
| UM | TextBox | UM | 4248,0 960x264 | 720 |  |
| Valoarex | TextBox | =Sum([Valoare]) | 7836,0 1380x264 | 747 |  |
| Box8 | Rectangle |  | 6456,0 1380x264 | 777 |  |
| Line13 | Line |  | 408,0 0x264 | 794 |  |
| Line14 | Line |  | 9216,0 0x264 | 807 |  |
| Label17 | Label |  | 408,0 8784x0 | 820 |  |

## Detail (inaltime 264 twips, linia 848)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Valoare | TextBox | Valoare | 7836,0 1380x264 | 857 |  |
| PUTVA | TextBox | PUTVA | 6456,0 1380x264 | 886 |  |
| Line15 | Line |  | 408,0 0x264 | 915 |  |
| Line16 | Line |  | 9216,0 0x264 | 928 |  |
| Cant | TextBox | Cant | 5208,0 1248x264 | 941 |  |

## GroupFooter1 (inaltime 72 twips, linia 972)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label12 | Label |  | 408,0 8784x0 | 984 |  |

## GroupFooter2 (inaltime 0 twips, linia 1012)

_fara controale_

## PageFooterSection (inaltime 501 twips, linia 1027)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text50 | TextBox | ="Pagina " & [Page] & " din " & [Pages] | 7125,24 2400x285 | 1038 |  |
| Label107 | TextBox | =DLookUp("expl","ver_rap") | 0,24 2736x477 | 1070 |  |
| Line51 | Line |  | 0,0 9522x0 | 1103 |  |

