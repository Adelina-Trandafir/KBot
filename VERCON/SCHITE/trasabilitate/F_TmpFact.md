# Trasabilitate schite rânduri de produse

Convenții și limite: [citirea fișelor consolidate](../INDEX.md#fise-consolidate). SLICE-D003-17, 28.10.2026. Secțiunea este coloana întâi.

## Variante și surse

### Trasabilitate schita formular TmpFact

Sursa: [Forms/TmpFact.txt](../../VERCON_ACCESS/Forms/TmpFact.txt) · imagine: [F_TmpFact.png](../F_TmpFact.png)

- Titlu (Caption): — · Sursa de date (RecordSource): `TmpFact` · Latime: 9918 twips · sectiuni: 3 · controale: 24

### Trasabilitate schita formular TmpFactMF

Sursa: [Forms/TmpFactMF.txt](../../VERCON_ACCESS/Forms/TmpFactMF.txt) · imagine: [F_TmpFactMF.png](../F_TmpFactMF.png)

- Titlu (Caption): — · Sursa de date (RecordSource): `TmpFact` · Latime: 14389 twips · sectiuni: 3 · controale: 32

### Trasabilitate schita formular TmpFactV

Sursa: [Forms/TmpFactV.txt](../../VERCON_ACCESS/Forms/TmpFactV.txt) · imagine: [F_TmpFactV.png](../F_TmpFactV.png)

- Titlu (Caption): — · Sursa de date (RecordSource): `SELECT TmpFactV.* FROM TmpFactV WHERE (((TmpFactV.S)=True)) ORDER BY TmpFactV.Denumire; ` · Latime: 9834 twips · sectiuni: 3 · controale: 13

## Secțiuni

| Secțiune | TmpFact | TmpFactMF | TmpFactV |
|---|---|---|---|
| FormHeader <a id="formheader-inaltime-284-twips-linia-368"></a><a id="formheader-inaltime-284-twips-linia-379"></a><a id="formheader-inaltime-300-twips-linia-358"></a> | 1 / 284 / 368 | 1 / 284 / 379 | 1 / 300 / 358 |
| Detail <a id="detail-inaltime-254-twips-linia-708"></a><a id="detail-inaltime-264-twips-linia-863"></a><a id="detail-inaltime-254-twips-linia-517"></a> | 2 / 254 / 708 | 2 / 264 / 863 | 2 / 254 / 517 |
| FormFooter <a id="formfooter-inaltime-269-twips-linia-1020"></a><a id="formfooter-inaltime-269-twips-linia-1287"></a><a id="formfooter-inaltime-254-twips-linia-741"></a> | 3 / 269 / 1020 | 3 / 269 / 1287 | 3 / 254 / 741 |

## Controale

| Secțiune | Control | Tip | Text / camp / sursa | Pozitie comună (twips) | Evenimente | TmpFact | TmpFactMF | TmpFactV |
|---|---|---|---|---|---|---|---|---|
| FormHeader | Srt | ToggleButton |  | 2829,0 288x284 | OnMouseMove,OnClick | 1:376 | — | — |
| FormHeader | Label7 | Label | Denumire Produs | 390,0 2439x284 |  | 2:438 | 9:600 / 390,0 3167x284 | 1:366 / 390,0 4461x284 |
| FormHeader | Label9 | Label | U/M | 3120,0 614x284 |  | 3:467 | — | 2:391 / 5930,0 1091x284 |
| FormHeader | Label11 | Label | Cant | 3734,0 965x284 |  | 4:494 | — | 3:416 / 4839,0 1091x284 |
| FormHeader | Label13 | Label | Preț | 4699,0 989x284 |  | 5:521 | 12:753 / 3562,0 989x284 | — |
| FormHeader | Label15 | Label | Valoare | 5688,0 989x284 |  | 6:548 | 13:780 / 4551,0 989x284 | 4:441 / 8424,0 1403x284 |
| FormHeader | Label17 | Label | TVA | 6677,0 989x284 |  | 7:575 | 14:807 / 5540,0 989x284 | — |
| FormHeader | Label31 | Label | DEL | 0,0 390x284 |  | 8:602 | 1:387 | 6:491 |
| FormHeader | Label33 | Label | PU TVA | 7666,0 1091x284 |  | 9:628 | 15:834 / 6529,0 1091x284 | — |
| FormHeader | Label36 | Label | VAL. CU TVA | 8760,0 1151x284 |  | 10:655 | — | — |
| FormHeader | FOC | CommandButton | Command46 | 28,18 198x142 |  | 11:682 | 3:440 | — |
| FormHeader | Label36 | Label | Dată IN. | 10105,0 1151x284 |  | — | 2:413 | — |
| FormHeader | Label50 | Label | Nr. Inv. | 8776,0 1329x284 |  | — | 4:463 | — |
| FormHeader | Label52 | Label | VALOARE | 7625,0 1151x284 |  | — | 5:490 | — |
| FormHeader | Label54 | Label | Durată F. | 12247,0 991x284 |  | — | 6:517 | — |
| FormHeader | Label56 | Label | Durată N. | 11256,0 991x284 |  | — | 7:545 | — |
| FormHeader | Label58 | Label | VALOARE L. | 13238,0 1151x284 |  | — | 8:573 | — |
| FormHeader | Srt | ToggleButton |  | 3269,0 288x284 | OnMouseMove | — | 10:627 | — |
| FormHeader | Srt2 | ToggleButton |  | 9817,0 288x284 | OnMouseMove | — | 11:690 | — |
| FormHeader | Label23 | Label | Preț+TVA | 7021,0 1403x284 |  | — | — | 5:466 |
| Detail | Denumire | TextBox | Denumire | 390,0 2727x254 |  | 1:715 | 1:870 / 390,0 3167x264 | 1:524 / 390,0 4461x254 |
| Detail | UM | TextBox | UM | 3120,0 614x254 |  | 2:739 | — | 2:552 / 5930,0 1091x254 |
| Detail | Cant | TextBox | Cant | 3734,0 965x254 | OnEnter | 3:765 | — | — |
| Detail | Pret | TextBox | Pret | 4699,0 989x254 | OnEnter,OnExit | 4:794 | — | — |
| Detail | Valoare | TextBox | Valoare | 5688,0 989x254 |  | 5:838 | 10:1174 / 4551,0 989x264 | — |
| Detail | TVA | TextBox | TVA | 6677,0 989x254 |  | 6:866 | 11:1202 / 5540,0 989x264 | — |
| Detail | PUTVA | TextBox | PUTVA | 7666,0 1091x254 |  | 7:894 | 12:1230 / 6529,0 1091x264 | 5:646 / 7021,0 1403x254 |
| Detail | btnDelRow | CommandButton |  | 0,0 392x254 | OnClick | 8:922 | 2:897 / 0,0 392x264 | — |
| Detail | ValoareCUTVA | TextBox | ValoareCUTVA | 8760,0 1151x254 |  | 9:989 | 5:1020 / 7625,0 1151x264 | — |
| Detail | DataIntrare | TextBox | DataIntrare | 10105,0 1151x264 | OnEnter | — | 3:963 | — |
| Detail | NrInventar | TextBox | NrInventar | 8776,0 1329x264 |  | — | 4:993 | — |
| Detail | DNF | TextBox | DNF | 11256,0 991x264 |  | — | 6:1049 | — |
| Detail | DF | TextBox | DF | 12247,0 991x264 |  | — | 7:1079 | — |
| Detail | ValoareLunara | TextBox | ValoareLunara | 13238,0 1151x264 |  | — | 8:1105 | — |
| Detail | Pret | TextBox | Pret | 3562,0 989x264 |  | — | 9:1133 | — |
| Detail | Cant | TextBox | Cant | 803,0 965x254 |  | — | 13:1258 | 3:582 / 4839,0 1091x254 |
| Detail | Valoare | TextBox | ValoareCuTVA | 8424,0 1403x254 |  | — | — | 4:614 |
| Detail | Command35 | CommandButton |  | 0,0 392x254 | OnClick | — | — | 6:678 |
| FormFooter | TValoare | TextBox | =Sum([Valoare]) | 5688,0 989x269 |  | 1:1028 | 3:1402 / 4656,0 989x269 | — |
| FormFooter | TTVA | TextBox | =Sum([tva]) | 6677,0 989x269 |  | 2:1053 | 4:1428 / 5645,0 989x269 | — |
| FormFooter | TOTAL | TextBox | =Sum([ValoareCUTVA]) | 7676,0 2242x269 |  | 3:1079 | 1:1295 / 7625,0 1151x269 | — |
| FormFooter | Ajustare | CommandButton | Command49 | 9658,0 260x269 | OnClick | 4:1124 | 2:1338 / 548,0 260x269 | — |
| FormFooter | TOTALx | TextBox | =Sum(IIf([s]=True,[valoarecutva],0)) | 7968,0 1859x254 |  | — | — | 1:749 |
