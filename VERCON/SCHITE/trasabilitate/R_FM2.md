# Trasabilitate schita raport FM2

Sursa: [Reports/FM2.txt](../../VERCON_ACCESS/Reports/FM2.txt) · imagine: [R_FM2.png](../R_FM2.png)

- Titlu (Caption): —
- Sursa de date (RecordSource): `SELECT X.IDFM, X.IDF, X.CODP, X.ID, X.Numar, X.CIN, X.CIE, X.CR, X.Data, X.IdIntrareIesire FROM (SELECT FM_2.IDFM, FM_2.CODP, FM_2.ID, FM_2.Numar, FM_2.CIN, FM_2.CIE, FM_2.CR, FM_2.Data, FM_2.IDF, FM_2.IdIntrareIesire FROM FM_2 GROUP BY FM_2.IDFM, FM_2.CODP, FM_2.ID, FM_2.Numar, FM_2.CIN, FM_2.CIE, FM_2.CR, FM_2.Data, FM_2.IDF, FM_2.IdIntrareIesire HAVING (((FM_2.Data) Between [forms]![sfp]![di] A …`
- Latime: 9675 twips · sectiuni: 6 · controale: 22

## ReportHeader (inaltime 0 twips, linia 464)

_fara controale_

## PageHeaderSection (inaltime 0 twips, linia 472)

_fara controale_

## Detail (inaltime 285 twips, linia 479)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| NrIN | TextBox | Numar | 1264,0 2926x285 | 487 |  |
| CIN | TextBox | CIN | 4190,0 1199x285 | 512 |  |
| CIE | TextBox | CIE | 5389,0 1199x285 | 551 |  |
| Text8 | TextBox | Data | 0,0 1260x285 | 590 |  |
| CR | TextBox | CR | 6588,0 1199x285 | 611 |  |
| Text11 | TextBox |  | 7787,0 851x285 | 640 |  |
| Text12 | TextBox |  | 8638,0 1022x285 | 665 |  |
| Line25 | Line |  | 0,0 0x285 | 690 |  |
| Line26 | Line |  | 4190,0 0x285 | 701 |  |
| Line27 | Line |  | 9660,0 0x285 | 715 |  |
| Line28 | Line |  | 7787,0 0x285 | 729 |  |
| Line29 | Line |  | 9660,0 0x285 | 743 |  |

## GroupFooter0 (inaltime 285 twips, linia 759)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| tCIN | TextBox | =Sum([CIN]) | 4190,0 1199x285 | 767 |  |
| tCIE | TextBox | =Sum([cie]) | 5389,0 1199x285 | 796 |  |
| Text18 | TextBox | =[tcin]-[tcie] | 6588,0 1199x285 | 826 |  |
| Text19 | TextBox | ="Total luna " & fluna(Month([Data])) & " / " & Year([Data]) | 6,0 4184x285 | 857 |  |
| Text22 | TextBox |  | 7787,0 1866x285 | 882 |  |

## PageFooterSection (inaltime 0 twips, linia 908)

_fara controale_

## ReportFooter (inaltime 285 twips, linia 915)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Text13 | TextBox | =Sum([CIN]) | 4190,0 1199x285 | 923 |  |
| Text14 | TextBox | =Sum([CIE]) | 5389,0 1199x285 | 952 |  |
| Text15 | TextBox | =[text13]-[text14] | 6588,0 1199x285 | 982 |  |
| Text23 | TextBox | ="TOTAL GENERAL" | 0,0 4184x285 | 1012 |  |
| Text24 | TextBox |  | 7787,0 1866x285 | 1035 |  |

