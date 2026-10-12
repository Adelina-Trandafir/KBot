# Trasabilitate schita formular OI_TmpFact

Sursa: [Forms/OI_TmpFact.txt](../../VERCON_ACCESS/Forms/OI_TmpFact.txt) · imagine: [F_OI_TmpFact.png](../F_OI_TmpFact.png)

- Titlu (Caption): —
- Sursa de date (RecordSource): `SELECT OI_TmpFact.* FROM OI_TmpFact WHERE (((OI_TmpFact.S)=True)) ORDER BY OI_TmpFact.DenumireProdus; `
- Latime: 9812 twips · sectiuni: 3 · controale: 15

## FormHeader (inaltime 300 twips, linia 354)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label7 | Label | Denumire Produs | 390,0 3075x284 | 362 |  |
| Label9 | Label | U/M | 3465,0 674x284 | 387 |  |
| Label11 | Label | Cant | 4139,0 1139x284 | 412 |  |
| Label13 | Label | Preț | 5278,0 1139x284 | 437 |  |
| Label15 | Label | Valoare | 6417,0 1139x284 | 462 |  |
| Label31 | Label | DEL | 0,0 390x284 | 487 |  |
| Label40 | Label | Proveniență | 7556,0 2256x284 | 511 |  |

## Detail (inaltime 254 twips, linia 538)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| DenumireProdus | TextBox | DenumireProdus | 390,0 3075x254 | 545 |  |
| UM | TextBox | UM | 3465,0 674x254 | 573 |  |
| Cant | TextBox | Cant | 4139,0 1139x254 | 602 |  |
| Pret | TextBox | Pret | 5278,0 1139x254 | 633 |  |
| Valoare | TextBox | Valoare | 6417,0 1139x254 | 664 |  |
| Command35 | CommandButton |  | 0,0 392x254 | 695 | OnClick |
| Predator | TextBox | Predator | 7556,0 2256x254 | 756 |  |

## FormFooter (inaltime 254 twips, linia 787)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| TOTALx | TextBox | =Abs(Sum(IIf([s]=True,[valoare],0))) | 6129,0 1427x254 | 795 |  |

