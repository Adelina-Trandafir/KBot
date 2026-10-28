# Trasabilitate schite mese: cină, dejun și mic dejun

Convenții și limite: [citirea fișelor consolidate](../INDEX.md#fise-consolidate). SLICE-D003-13, 28.10.2026. Secțiunea este coloana întâi a matricei de controale.

## Variante și surse

### Trasabilitate schita formular CN

Sursa: [Forms/CN.txt](../../VERCON_ACCESS/Forms/CN.txt) · imagine: [F_CN.png](../F_CN.png)

- Titlu (Caption): MD · Sursa de date (RecordSource): `SELECT Tmp_Cina.* FROM Tmp_Cina ORDER BY Tmp_Cina.Tip; ` · Latime: 3408 twips · sectiuni: 3 · controale: 7

### Trasabilitate schita formular D

Sursa: [Forms/D.txt](../../VERCON_ACCESS/Forms/D.txt) · imagine: [F_D.png](../F_D.png)

- Titlu (Caption): MD · Sursa de date (RecordSource): `SELECT Tmp_Dejun.* FROM Tmp_Dejun ORDER BY Tmp_Dejun.Tip; ` · Latime: 3408 twips · sectiuni: 3 · controale: 7

### Trasabilitate schita formular MD

Sursa: [Forms/MD.txt](../../VERCON_ACCESS/Forms/MD.txt) · imagine: [F_MD.png](../F_MD.png)

- Titlu (Caption): MD · Sursa de date (RecordSource): `SELECT Tmp_MicDejun.* FROM Tmp_MicDejun ORDER BY Tmp_MicDejun.Tip; ` · Latime: 3408 twips · sectiuni: 3 · controale: 7

## Secțiuni

| Secțiune | CN | D | MD |
|---|---|---|---|
| FormHeader <a id="formheader-inaltime-255-twips-linia-151"></a><a id="formheader-inaltime-255-twips-linia-152"></a> | 1 / 255 / 151 | 1 / 255 / 151 | 1 / 255 / 152 |
| Detail <a id="detail-inaltime-483-twips-linia-202"></a><a id="detail-inaltime-483-twips-linia-203"></a> | 2 / 483 / 202 | 2 / 483 / 202 | 2 / 483 / 203 |
| FormFooter <a id="formfooter-inaltime-0-twips-linia-416"></a><a id="formfooter-inaltime-0-twips-linia-417"></a> | 3 / 0 / 416 | 3 / 0 / 416 | 3 / 0 / 417 |

## Controale

| Secțiune | Control | Tip | Text / camp / sursa | Pozitie (twips) | Evenimente | CN | D | MD |
|---|---|---|---|---|---|---|---|---|
| FormHeader | Tip_Label | Label | Tip | 264,0 840x255 |  | 1:159 | 1:159 | 1:160 |
| FormHeader | Explicatie_Label | Label | Explicatie | 1104,0 2304x255 |  | 2:179 | 2:179 | 2:180 |
| Detail | Tip | ComboBox | Tip | 264,0 840x483 | OnGotFocus,OnLostFocus | 1:209 | 1:209 | 1:210 |
| Detail | Explicatie | ComboBox | Explicatie | 1104,0 2304x483 | OnLostFocus | 2:240 | 2:240 | 2:241 |
| Detail | TipT | TextBox | Tip | 264,0 840x483 | OnEnter | 3:289 | 3:289 | 3:290 |
| Detail | ExplicatieT | TextBox | Explicatie | 1104,0 2304x483 | OnEnter | 4:315 | 4:315 | 4:316 |
| Detail | Dlt | CommandButton | Command12 | 0,0 264x483 | OnClick | 5:353 | 5:353 | 5:354 |
