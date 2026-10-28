# Trasabilitate schite subrapoarte mese

Convenții și limite: [citirea fișelor consolidate](../INDEX.md#fise-consolidate). SLICE-D003-13, 28.10.2026. Secțiunea este coloana întâi a matricei de controale.

## Variante și surse

### Trasabilitate schita raport Cina subreport

Sursa: [Reports/Cina subreport.txt](../../VERCON_ACCESS/Reports/Cina%20subreport.txt) · imagine: [R_Cina_subreport.png](../R_Cina_subreport.png)

- Titlu (Caption): MicDejun subreport · Sursa de date (RecordSource): `SELECT Cina.Id, Cina.IdLista, Cina.IdFel, Cina.Tip, Cina.Explicatie FROM Cina ORDER BY Cina.Tip;` · Latime: 3288 twips · sectiuni: 5 · controale: 4

### Trasabilitate schita raport Dejun subreport

Sursa: [Reports/Dejun subreport.txt](../../VERCON_ACCESS/Reports/Dejun%20subreport.txt) · imagine: [R_Dejun_subreport.png](../R_Dejun_subreport.png)

- Titlu (Caption): MicDejun subreport · Sursa de date (RecordSource): `SELECT Dejun.Id, Dejun.IdLista, Dejun.IdFel, Dejun.Tip, Dejun.Explicatie FROM Dejun ORDER BY Dejun.Tip, Dejun.Explicatie;` · Latime: 3288 twips · sectiuni: 5 · controale: 4

### Trasabilitate schita raport MicDejun subreport

Sursa: [Reports/MicDejun subreport.txt](../../VERCON_ACCESS/Reports/MicDejun%20subreport.txt) · imagine: [R_MicDejun_subreport.png](../R_MicDejun_subreport.png)

- Titlu (Caption): MicDejun subreport · Sursa de date (RecordSource): `SELECT MicDejun.Id, MicDejun.IdLista, MicDejun.IdFel, MicDejun.Tip, MicDejun.Explicatie FROM MicDejun ORDER BY MicDejun.Tip, MicDejun.Explicatie;` · Latime: 3280 twips · sectiuni: 5 · controale: 4

## Secțiuni

| Secțiune | Cina_subreport | Dejun_subreport | MicDejun_subreport |
|---|---|---|---|
| ReportHeader <a id="reportheader-inaltime-0-twips-linia-475"></a> | 1 / 0 / 475 | 1 / 0 / 475 | 1 / 0 / 475 |
| PageHeaderSection <a id="pageheadersection-inaltime-15-twips-linia-483"></a> | 2 / 15 / 483 | 2 / 15 / 483 | 2 / 15 / 483 |
| Detail <a id="detail-inaltime-270-twips-linia-500"></a><a id="detail-inaltime-271-twips-linia-500"></a> | 3 / 270 / 500 | 3 / 271 / 500 | 3 / 271 / 500 |
| PageFooterSection <a id="pagefootersection-inaltime-15-twips-linia-547"></a> | 4 / 15 / 547 | 4 / 15 / 547 | 4 / 15 / 547 |
| ReportFooter <a id="reportfooter-inaltime-0-twips-linia-565"></a> | 5 / 0 / 565 | 5 / 0 / 565 | 5 / 0 / 565 |

## Controale

| Secțiune | Control | Tip | Text / camp / sursa | Pozitie (twips) | Evenimente | Cina_subreport | Dejun_subreport | MicDejun_subreport |
|---|---|---|---|---|---|---|---|---|
| PageHeaderSection | Line8 | Line |  | 0,0 0x0 |  | 1:490 | 1:490 | 1:490 |
| Detail | txtExplicatie | TextBox | =UCase([Explicatie]) | 0,0 3285x270 |  | 1:511 | — | — |
| Detail | IdFel | TextBox | IdFel | 0,0 1701x240 |  | 2:532 | 2:532 | 2:532 |
| Detail | txtExplicatie | TextBox | =UCase([Explicatie]) | 0,0 3285x271 |  | — | 1:511 | — |
| Detail | txtExplicatie | TextBox | =UCase([Explicatie]) | 0,0 3280x271 |  | — | — | 1:511 |
| PageFooterSection | Line9 | Line |  | 0,0 0x0 |  | 1:554 | 1:554 | 1:554 |
