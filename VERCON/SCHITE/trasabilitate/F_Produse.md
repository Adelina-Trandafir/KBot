# Trasabilitate schita formular Produse

Sursa: [Forms/Produse.txt](../../VERCON_ACCESS/Forms/Produse.txt) · imagine: [F_Produse.png](../F_Produse.png)

- Titlu (Caption): Adaugă / Modifică produse
- Sursa de date (RecordSource): —
- Latime: 10998 twips · sectiuni: 3 · controale: 23

## FormHeader (inaltime 0 twips, linia 355)

_fara controale_

## Detail (inaltime 3418 twips, linia 367)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| subTreeView | Subform | Form.subTreeView | 30,50 5189x3339 | 379 | OnEnter,OnExit |
| DenumireP | TextBox |  | 7275,50 3634x270 | 408 | OnKeyPress |
| Label42 | Label | Denumire produs | 5311,50 1964x270 | 445 |  |
| UM | TextBox |  | 7275,390 3634x270 | 483 | OnKeyPress |
| Label44 | Label | UM | 5302,390 1964x270 | 520 |  |
| CPV | TextBox |  | 7275,730 3294x270 | 558 | OnKeyDown |
| Label60 | Label | CPV | 5302,730 1964x270 | 595 |  |
| CC | ComboBox | SELECT Clasificari.Cod, Clasificari.Denumire, Clasificari.DNF_MIN, Clasificari.DNF_MAX FROM Clasificari; | 7283,1410 3284x270 | 634 |  |
| Label3 | Label | CLASIFICARE M.F. | 5359,1410 1924x270 | 678 |  |
| bCCPV | CommandButton |  | 10638,730 271x270 | 716 | OnClick |
| Box4 | Rectangle |  | 5300,1244 5673x1304 | 803 |  |
| Label5 | Label | INFORMAȚII MIJLOC FIX | 5413,1130 1990x230 | 822 |  |
| DNF | TextBox |  | 8640,1770 1534x250 | 846 | OnEnter,OnExit |
| Label9 | Label | DURATĂ NORMALĂ FOLOSIRE | 5359,1757 3284x270 | 890 |  |
| Label10 | Label | LUNI | 10177,1767 560x250 | 928 |  |
| Box11 | Rectangle |  | 8627,1757 2121x270 | 962 |  |
| bCCLS | CommandButton |  | 10631,1410 271x270 | 981 | OnClick |
| CLSFDNF | Label |  | 5390,2070 5556x457 | 1069 |  |

## FormFooter (inaltime 680 twips, linia 1096)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| btnAdd | CommandButton | &Adaugă | 8120,60 1350x561 | 1106 | OnClick |
| btnMod | CommandButton | &Modifică | 3845,60 1350x561 | 1200 | OnClick |
| btnOut | CommandButton | IEȘIRE | 30,60 1350x561 | 1295 | OnClick |
| btnSav | CommandButton | &Salvează | 9623,56 1350x561 | 1386 | OnClick |
| btnUnd | CommandButton | &Renunță | 5302,56 1350x561 | 1480 | OnClick |

