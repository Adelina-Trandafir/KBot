# Trasabilitate schita formular tmpSFP

Sursa: [Forms/tmpSFP.txt](../../VERCON_ACCESS/Forms/tmpSFP.txt) · imagine: [F_tmpSFP.png](../F_tmpSFP.png)

- Titlu (Caption): —
- Sursa de date (RecordSource): `SELECT tmpSFP.IdMasina, tmpSFP.Masina, Min(tmpSFP.S) AS S FROM tmpSFP GROUP BY tmpSFP.IdMasina, tmpSFP.Masina ORDER BY tmpSFP.Masina; `
- Latime: 3855 twips · sectiuni: 3 · controale: 4

## FormHeader (inaltime 0 twips, linia 614)

_fara controale_

## Detail (inaltime 2702 twips, linia 622)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| IdMasina | TextBox | IdMasina | 0,0 3285x359 | 630 |  |
| Masina | TextBox | Masina | 0,340 3285x359 | 652 |  |
| S | CheckBox | S | 2948,113 260x240 | 678 | OnMouseDown,OnMouseMove,OnClick |
| tmpSFP_sub | Subform | Form.tmpSFP_sub | 0,737 3855x1965 | 699 |  |

## FormFooter (inaltime 0 twips, linia 719)

_fara controale_

