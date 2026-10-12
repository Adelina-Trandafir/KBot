# Trasabilitate schita raport Dejun subreport

Sursa: [Reports/Dejun subreport.txt](../../VERCON_ACCESS/Reports/Dejun%20subreport.txt) · imagine: [R_Dejun_subreport.png](../R_Dejun_subreport.png)

- Titlu (Caption): MicDejun subreport
- Sursa de date (RecordSource): `SELECT Dejun.Id, Dejun.IdLista, Dejun.IdFel, Dejun.Tip, Dejun.Explicatie FROM Dejun ORDER BY Dejun.Tip, Dejun.Explicatie;`
- Latime: 3288 twips · sectiuni: 5 · controale: 4

## ReportHeader (inaltime 0 twips, linia 475)

_fara controale_

## PageHeaderSection (inaltime 15 twips, linia 483)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Line8 | Line |  | 0,0 0x0 | 490 |  |

## Detail (inaltime 271 twips, linia 500)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| txtExplicatie | TextBox | =UCase([Explicatie]) | 0,0 3285x271 | 511 |  |
| IdFel | TextBox | IdFel | 0,0 1701x240 | 532 |  |

## PageFooterSection (inaltime 15 twips, linia 547)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Line9 | Line |  | 0,0 0x0 | 554 |  |

## ReportFooter (inaltime 0 twips, linia 565)

_fara controale_

