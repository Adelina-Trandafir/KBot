# Trasabilitate schita formular TmpFactMF

Sursa: [Forms/TmpFactMF.txt](../../VERCON_ACCESS/Forms/TmpFactMF.txt) · imagine: [F_TmpFactMF.png](../F_TmpFactMF.png)

- Titlu (Caption): —
- Sursa de date (RecordSource): `TmpFact`
- Latime: 14389 twips · sectiuni: 3 · controale: 32

## FormHeader (inaltime 284 twips, linia 379)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label31 | Label | DEL | 0,0 390x284 | 387 |  |
| Label36 | Label | Dată IN. | 10105,0 1151x284 | 413 |  |
| FOC | CommandButton | Command46 | 28,18 198x142 | 440 |  |
| Label50 | Label | Nr. Inv. | 8776,0 1329x284 | 463 |  |
| Label52 | Label | VALOARE | 7625,0 1151x284 | 490 |  |
| Label54 | Label | Durată F. | 12247,0 991x284 | 517 |  |
| Label56 | Label | Durată N. | 11256,0 991x284 | 545 |  |
| Label58 | Label | VALOARE L. | 13238,0 1151x284 | 573 |  |
| Label7 | Label | Denumire Produs | 390,0 3167x284 | 600 |  |
| Srt | ToggleButton |  | 3269,0 288x284 | 627 | OnMouseMove |
| Srt2 | ToggleButton |  | 9817,0 288x284 | 690 | OnMouseMove |
| Label13 | Label | Preț | 3562,0 989x284 | 753 |  |
| Label15 | Label | Valoare | 4551,0 989x284 | 780 |  |
| Label17 | Label | TVA | 5540,0 989x284 | 807 |  |
| Label33 | Label | PU TVA | 6529,0 1091x284 | 834 |  |

## Detail (inaltime 264 twips, linia 863)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Denumire | TextBox | Denumire | 390,0 3167x264 | 870 |  |
| btnDelRow | CommandButton |  | 0,0 392x264 | 897 | OnClick |
| DataIntrare | TextBox | DataIntrare | 10105,0 1151x264 | 963 | OnEnter |
| NrInventar | TextBox | NrInventar | 8776,0 1329x264 | 993 |  |
| ValoareCUTVA | TextBox | ValoareCUTVA | 7625,0 1151x264 | 1020 |  |
| DNF | TextBox | DNF | 11256,0 991x264 | 1049 |  |
| DF | TextBox | DF | 12247,0 991x264 | 1079 |  |
| ValoareLunara | TextBox | ValoareLunara | 13238,0 1151x264 | 1105 |  |
| Pret | TextBox | Pret | 3562,0 989x264 | 1133 |  |
| Valoare | TextBox | Valoare | 4551,0 989x264 | 1174 |  |
| TVA | TextBox | TVA | 5540,0 989x264 | 1202 |  |
| PUTVA | TextBox | PUTVA | 6529,0 1091x264 | 1230 |  |
| Cant | TextBox | Cant | 803,0 965x254 | 1258 |  |

## FormFooter (inaltime 269 twips, linia 1287)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| TOTAL | TextBox | =Sum([ValoareCUTVA]) | 7625,0 1151x269 | 1295 |  |
| Ajustare | CommandButton | Command49 | 548,0 260x269 | 1338 | OnClick |
| TValoare | TextBox | =Sum([Valoare]) | 4656,0 989x269 | 1402 |  |
| TTVA | TextBox | =Sum([tva]) | 5645,0 989x269 | 1428 |  |

