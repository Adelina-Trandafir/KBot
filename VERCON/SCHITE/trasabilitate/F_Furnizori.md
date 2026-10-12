# Trasabilitate schita formular Furnizori

Sursa: [Forms/Furnizori.txt](../../VERCON_ACCESS/Forms/Furnizori.txt) · imagine: [F_Furnizori.png](../F_Furnizori.png)

- Titlu (Caption): Furnizori
- Sursa de date (RecordSource): `SELECT Furnizori.* FROM Furnizori WHERE (((Furnizori.IDF)<>999)) ORDER BY IIf(Nz([cf],'')='',0,1), Furnizori.Denumire;`
- Latime: 5646 twips · sectiuni: 3 · controale: 7

## FormHeader (inaltime 255 twips, linia 631)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Denumire_Label | Label | Denumire | 0,0 4188x255 | 639 |  |
| CF_Label | Label | Cod Fiscal | 4188,0 1440x255 | 659 |  |

## Detail (inaltime 255 twips, linia 684)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Bck | TextBox |  | 0,0 5646x255 | 691 | OnEnter |
| Denumire | TextBox | Denumire | 0,0 4188x255 | 728 | OnKeyPress |
| CF | TextBox | CF | 4188,0 1440x255 | 757 |  |
| S | CommandButton | Command21 | 0,0 5631x255 | 787 | OnClick |

## FormFooter (inaltime 285 twips, linia 813)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| CL | TextBox |  | 0,0 681x285 | 821 |  |

