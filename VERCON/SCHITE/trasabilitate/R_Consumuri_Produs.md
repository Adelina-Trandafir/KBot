# Trasabilitate schita raport Consumuri_Produs

Sursa: [Reports/Consumuri_Produs.txt](../../VERCON_ACCESS/Reports/Consumuri_Produs.txt) · imagine: [R_Consumuri_Produs.png](../R_Consumuri_Produs.png)

- Titlu (Caption): —
- Sursa de date (RecordSource): `SELECT X.IdMasina, X.Masina, X.CodProdus, X.Denumire, X.UM, X.Cant, X.PUTVA, [Cant]*[putva] AS Valoare FROM (SELECT Masini.IdMasina, Masini.Masina, ProduseP.IdProdus AS Codprodus, Produse.denumirep AS Denumire, Produse.UM, Sum(Iesiri.Cant) AS Cant, Iesiri.PUTVA FROM ((Masini INNER JOIN Produse ON Masini.IdMasina = Produse.IdMasina) INNER JOIN (((Clienti INNER JOIN Iesiri_H ON Clienti.IDC = Iesiri_ …`
- Latime: 9525 twips · sectiuni: 7 · controale: 31

## PageHeaderSection (inaltime 2244 twips, linia 203)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text18 | TextBox | =DLookUp("Firma","caletemp") | 0,0 9522x306 | 213 |  |
| Text49 | TextBox | =[Forms]![sfp]![di] & " - " & [Forms]![sfp]![dsf] | 0,984 9522x450 | 242 |  |
| Label19 | Label | Situație consumuri / conturi | 0,504 9522x480 | 275 |  |
| L1 | Label | Denumire produs | 408,1968 3840x276 | 304 |  |
| L2 | Label | U/M | 4248,1968 960x276 | 334 |  |
| L3 | Label | Preț unitar | 6456,1968 1380x276 | 364 |  |
| L4 | Label | Valoare | 7836,1968 1380x276 | 394 |  |
| Masina | TextBox | Masina | 0,1608 9525x288 | 424 |  |
| Label26 | Label | Cant. | 5208,1968 1248x276 | 456 |  |

## GroupHeader1 (inaltime 1152 twips, linia 488)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label0 | Label | Denumire produs | 408,480 3840x276 | 500 |  |
| Label1 | Label | U/M | 4248,480 960x276 | 530 |  |
| Label2 | Label | Preț unitar | 6456,480 1380x276 | 560 |  |
| Label3 | Label | Valoare | 7836,480 1380x276 | 590 |  |
| Label23 | Label | U/M | 5208,480 1248x276 | 620 |  |

## GroupHeader0 (inaltime 264 twips, linia 652)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Cantx | TextBox | =Sum([Cant]) | 5208,0 1248x264 | 664 |  |
| Denumirex | TextBox | =UCase([Denumire]) | 408,0 3840x264 | 694 |  |
| UM | TextBox | UM | 4248,0 960x264 | 721 |  |
| Valoarex | TextBox | =Sum([Valoare]) | 7836,0 1380x264 | 748 |  |
| Box8 | Rectangle |  | 6456,0 1380x264 | 778 |  |
| Line13 | Line |  | 408,0 0x264 | 795 |  |
| Line14 | Line |  | 9216,0 0x264 | 808 |  |
| Label17 | Label |  | 408,0 8784x0 | 821 |  |

## Detail (inaltime 264 twips, linia 849)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Valoare | TextBox | Valoare | 7836,0 1380x264 | 858 |  |
| PUTVA | TextBox | PUTVA | 6456,0 1380x264 | 887 |  |
| Line15 | Line |  | 408,0 0x264 | 916 |  |
| Line16 | Line |  | 9216,0 0x264 | 929 |  |
| Cant | TextBox | Cant | 5208,0 1248x264 | 942 |  |

## GroupFooter1 (inaltime 72 twips, linia 973)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label12 | Label |  | 408,0 8784x0 | 985 |  |

## GroupFooter2 (inaltime 0 twips, linia 1013)

_fara controale_

## PageFooterSection (inaltime 501 twips, linia 1028)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text50 | TextBox | ="Pagina " & [Page] & " din " & [Pages] | 7125,24 2400x285 | 1039 |  |
| Label107 | TextBox | =DLookUp("expl","ver_rap") | 0,24 2736x477 | 1071 |  |
| Line51 | Line |  | 0,0 9522x0 | 1104 |  |

