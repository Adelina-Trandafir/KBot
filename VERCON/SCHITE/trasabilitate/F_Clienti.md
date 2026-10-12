# Trasabilitate schita formular Clienti

Sursa: [Forms/Clienti.txt](../../VERCON_ACCESS/Forms/Clienti.txt) · imagine: [F_Clienti.png](../F_Clienti.png)

- Titlu (Caption): Clienti
- Sursa de date (RecordSource): `SELECT Clienti.* FROM Clienti ORDER BY Clienti.Denumire; `
- Latime: 7914 twips · sectiuni: 3 · controale: 9

## FormHeader (inaltime 300 twips, linia 261)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| IDC_Label | Label | IDC | 0,0 864x300 | 269 |  |
| Denumire_Label | Label | Denumire | 864,0 2946x300 | 287 |  |
| Label6 | Label | Alte detalii | 3810,0 3357x300 | 308 |  |
| Label8 | Label | TIP | 7167,0 744x300 | 329 |  |

## Detail (inaltime 300 twips, linia 352)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| IDC | TextBox | IDC | 0,0 864x300 | 359 |  |
| Denumire | TextBox | Denumire | 864,0 2946x300 | 378 | OnKeyPress |
| Adresa | TextBox | Adresa | 3810,0 3357x300 | 401 | OnKeyPress |
| CF | TextBox | CF | 7167,0 747x300 | 423 |  |

## FormFooter (inaltime 720 twips, linia 448)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| btnIesire | CommandButton | IEȘIRE | 3288,48 1365x633 | 455 | OnClick |

