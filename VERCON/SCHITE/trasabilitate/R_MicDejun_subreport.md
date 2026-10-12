# Trasabilitate schita raport MicDejun subreport

Sursa: [Reports/MicDejun subreport.txt](../../VERCON_ACCESS/Reports/MicDejun%20subreport.txt) · imagine: [R_MicDejun_subreport.png](../R_MicDejun_subreport.png)

- Titlu (Caption): MicDejun subreport
- Sursa de date (RecordSource): `SELECT MicDejun.Id, MicDejun.IdLista, MicDejun.IdFel, MicDejun.Tip, MicDejun.Explicatie FROM MicDejun ORDER BY MicDejun.Tip, MicDejun.Explicatie;`
- Latime: 3280 twips · sectiuni: 5 · controale: 4

## ReportHeader (inaltime 0 twips, linia 475)

_fara controale_

## PageHeaderSection (inaltime 15 twips, linia 483)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Line8 | Line |  | 0,0 0x0 | 490 |  |

## Detail (inaltime 271 twips, linia 500)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| txtExplicatie | TextBox | =UCase([Explicatie]) | 0,0 3280x271 | 511 |  |
| IdFel | TextBox | IdFel | 0,0 1701x240 | 532 |  |

## PageFooterSection (inaltime 15 twips, linia 547)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Line9 | Line |  | 0,0 0x0 | 554 |  |

## ReportFooter (inaltime 0 twips, linia 565)

_fara controale_

