# Trasabilitate schita formular tmp_Balanta subform

Sursa: [Forms/tmp_Balanta subform.txt](../../VERCON_ACCESS/Forms/tmp_Balanta%20subform.txt) · imagine: [F_tmp_Balanta_subform.png](../F_tmp_Balanta_subform.png)

- Titlu (Caption): tmp_Balanta subform
- Sursa de date (RecordSource): `SELECT tmp_Balanta.IDM, tmp_Balanta.Masina, Sum(tmp_Balanta.VF) AS SOLD FROM tmp_Balanta GROUP BY tmp_Balanta.IDM, tmp_Balanta.Masina ORDER BY tmp_Balanta.Masina; `
- Latime: 4080 twips · sectiuni: 3 · controale: 6

## FormHeader (inaltime 0 twips, linia 80)

_fara controale_

## Detail (inaltime 1260 twips, linia 93)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Masina | TextBox | Masina | 2610,0 1470x330 | 101 |  |
| Masina_Label | Label | Masina | 60,0 2460x330 | 121 |  |
| IDM | TextBox | IDM | 2550,420 1530x330 | 140 |  |
| IDM_Label | Label | IDM | 0,420 2460x330 | 164 |  |
| SOLD | TextBox | SOLD | 2550,870 1530x330 | 183 |  |
| Label29 | Label | SOLD | 0,870 2460x330 | 209 |  |

## FormFooter (inaltime 0 twips, linia 230)

_fara controale_

