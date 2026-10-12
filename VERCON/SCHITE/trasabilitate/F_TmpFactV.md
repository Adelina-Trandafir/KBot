# Trasabilitate schita formular TmpFactV

Sursa: [Forms/TmpFactV.txt](../../VERCON_ACCESS/Forms/TmpFactV.txt) · imagine: [F_TmpFactV.png](../F_TmpFactV.png)

- Titlu (Caption): —
- Sursa de date (RecordSource): `SELECT TmpFactV.* FROM TmpFactV WHERE (((TmpFactV.S)=True)) ORDER BY TmpFactV.Denumire; `
- Latime: 9834 twips · sectiuni: 3 · controale: 13

## FormHeader (inaltime 300 twips, linia 358)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label7 | Label | Denumire Produs | 390,0 4461x284 | 366 |  |
| Label9 | Label | U/M | 5930,0 1091x284 | 391 |  |
| Label11 | Label | Cant | 4839,0 1091x284 | 416 |  |
| Label15 | Label | Valoare | 8424,0 1403x284 | 441 |  |
| Label23 | Label | Preț+TVA | 7021,0 1403x284 | 466 |  |
| Label31 | Label | DEL | 0,0 390x284 | 491 |  |

## Detail (inaltime 254 twips, linia 517)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Denumire | TextBox | Denumire | 390,0 4461x254 | 524 |  |
| UM | TextBox | UM | 5930,0 1091x254 | 552 |  |
| Cant | TextBox | Cant | 4839,0 1091x254 | 582 |  |
| Valoare | TextBox | ValoareCuTVA | 8424,0 1403x254 | 614 |  |
| PUTVA | TextBox | PUTVA | 7021,0 1403x254 | 646 |  |
| Command35 | CommandButton |  | 0,0 392x254 | 678 | OnClick |

## FormFooter (inaltime 254 twips, linia 741)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| TOTALx | TextBox | =Sum(IIf([s]=True,[valoarecutva],0)) | 7968,0 1859x254 | 749 |  |

