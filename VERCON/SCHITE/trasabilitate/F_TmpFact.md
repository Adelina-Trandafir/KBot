# Trasabilitate schita formular TmpFact

Sursa: [Forms/TmpFact.txt](../../VERCON_ACCESS/Forms/TmpFact.txt) · imagine: [F_TmpFact.png](../F_TmpFact.png)

- Titlu (Caption): —
- Sursa de date (RecordSource): `TmpFact`
- Latime: 9918 twips · sectiuni: 3 · controale: 24

## FormHeader (inaltime 284 twips, linia 368)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Srt | ToggleButton |  | 2829,0 288x284 | 376 | OnMouseMove,OnClick |
| Label7 | Label | Denumire Produs | 390,0 2439x284 | 438 |  |
| Label9 | Label | U/M | 3120,0 614x284 | 467 |  |
| Label11 | Label | Cant | 3734,0 965x284 | 494 |  |
| Label13 | Label | Preț | 4699,0 989x284 | 521 |  |
| Label15 | Label | Valoare | 5688,0 989x284 | 548 |  |
| Label17 | Label | TVA | 6677,0 989x284 | 575 |  |
| Label31 | Label | DEL | 0,0 390x284 | 602 |  |
| Label33 | Label | PU TVA | 7666,0 1091x284 | 628 |  |
| Label36 | Label | VAL. CU TVA | 8760,0 1151x284 | 655 |  |
| FOC | CommandButton | Command46 | 28,18 198x142 | 682 |  |

## Detail (inaltime 254 twips, linia 708)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Denumire | TextBox | Denumire | 390,0 2727x254 | 715 |  |
| UM | TextBox | UM | 3120,0 614x254 | 739 |  |
| Cant | TextBox | Cant | 3734,0 965x254 | 765 | OnEnter |
| Pret | TextBox | Pret | 4699,0 989x254 | 794 | OnEnter,OnExit |
| Valoare | TextBox | Valoare | 5688,0 989x254 | 838 |  |
| TVA | TextBox | TVA | 6677,0 989x254 | 866 |  |
| PUTVA | TextBox | PUTVA | 7666,0 1091x254 | 894 |  |
| btnDelRow | CommandButton |  | 0,0 392x254 | 922 | OnClick |
| ValoareCUTVA | TextBox | ValoareCUTVA | 8760,0 1151x254 | 989 |  |

## FormFooter (inaltime 269 twips, linia 1020)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| TValoare | TextBox | =Sum([Valoare]) | 5688,0 989x269 | 1028 |  |
| TTVA | TextBox | =Sum([tva]) | 6677,0 989x269 | 1053 |  |
| TOTAL | TextBox | =Sum([ValoareCUTVA]) | 7676,0 2242x269 | 1079 |  |
| Ajustare | CommandButton | Command49 | 9658,0 260x269 | 1124 | OnClick |

