# Trasabilitate schita formular BR

Sursa: [Forms/BR.txt](../../VERCON_ACCESS/Forms/BR.txt) · imagine: [F_BR.png](../F_BR.png)

- Titlu (Caption): Bonuri de Consum
- Sursa de date (RecordSource): —
- Latime: 15603 twips · sectiuni: 5 · controale: 57

## FormHeader (inaltime 480 twips, linia 239)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label160 | Label | Bon Retur | 0,0 10611x480 | 247 |  |
| Label1 | Label | Detalii produs ales | 10608,0 4995x480 | 266 |  |

## PageHeaderSection (inaltime 255 twips, linia 290)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| v | TextBox |  | 0,0 453x240 | 300 |  |
| S19 | CheckBox |  | 3750,35 260x195 | 314 |  |
| J | CheckBox |  | 5782,0 260x240 | 331 |  |
| s | CheckBox |  | 7426,0 260x240 | 345 |  |
| E | CheckBox |  | 6633,0 260x240 | 359 |  |
| xx | CheckBox |  | 8503,0 260x240 | 373 |  |
| lstCNT | TextBox |  | 9216,0 1701x255 | 387 |  |
| DisableTree | CheckBox |  | 11832,0 260x226 | 403 |  |
| NRBC | TextBox |  | 1080,0 741x240 | 418 |  |
| IdBonConsum | TextBox |  | 1800,0 717x240 | 434 |  |
| IdBonConsum_Old | TextBox |  | 2520,0 717x240 | 450 |  |

## Detail (inaltime 4680 twips, linia 468)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| NrFact | TextBox |  | 5034,144 1476x285 | 476 | OnKeyDown,OnGotFocus,OnLostFocus |
| Label35 | Label | Nr. B.C. | 4128,144 906x285 | 506 |  |
| Data | TextBox |  | 5034,429 1476x285 | 529 | OnGotFocus,OnLostFocus |
| Label36 | Label | Data B.C. | 4128,429 906x285 | 559 |  |
| OK | CommandButton | + | 72,144 375x285 | 582 | OnClick |
| Cant | TextBox |  | 7752,429 2739x285 | 653 | OnGotFocus,OnLostFocus,OnChange |
| Label56 | Label | Cantitate | 6624,429 1125x285 | 686 |  |
| Denprodus | ComboBox | SELECT QBCR.IdProdus, QBCR.IdProdus, QBCR.DenumireP, QBCR.UM, formatn([putva],11,1) AS PretF, formatn([cant],11,1) AS St | 7752,144 2739x285 | 709 |  |
| Label53 | Label | Produs | 6624,144 1125x285 | 744 |  |
| Masina | ComboBox | SELECT [Masini].[IdMasina], [Masini].[Masina] FROM Masini ORDER BY [Masina];  | 1417,429 2619x285 | 767 | OnChange |
| Masina_Label | Label | Categorie | 408,429 1009x285 | 803 |  |
| Tb | Tab |  | 10605,0 4935x4608 | 827 |  |
| Pg1 | Page |  | 10668,60 4812x4488 | 844 |  |
| PI | TextBox |  | 14207,1215 1254x285 | 863 |  |
| Label113 | Label | Preț unitar cu T.V.A. | 10760,1215 3447x285 | 895 |  |
| CTI | TextBox |  | 14207,1500 1254x285 | 919 |  |
| Label118 | Label | Cant. Bon Consum | 10760,1500 3447x285 | 952 |  |
| DP | TextBox |  | 10760,120 4701x810 | 976 |  |
| CTR | TextBox | =Round([cds],2)-Round([cant],2) | 14202,2640 1254x285 | 1008 |  |
| Label131 | Label | Cant. ramasă dupa scădere | 10755,2640 3447x285 | 1052 |  |
| msn | TextBox |  | 10760,930 4701x285 | 1076 |  |
| CAR | TextBox |  | 14207,2070 1254x285 | 1108 |  |
| Label206 | Label | Cant. alte Retururi | 10760,2070 3447x285 | 1141 |  |
| CDS | TextBox |  | 14202,2355 1254x285 | 1165 |  |
| Label208 | Label | Cant. disponibilă | 10755,2355 3447x285 | 1198 |  |
| BRC | TextBox |  | 14202,1785 1254x285 | 1222 |  |
| Label210 | Label | Cant. Bon Retur Curent | 10755,1785 3447x285 | 1255 |  |
| Pg2 | Page |  | 10668,24 4812x4524 | 1281 |  |
| subTreeView | Subform | Form.subTreeView | 10680,24 4707x4509 | 1300 | OnEnter,OnExit |
| TmpFactV | Subform | Form.TmpFactV | 72,1536 10443x2994 | 1327 |  |
| Expl | TextBox |  | 1417,714 2619x453 | 1349 |  |
| Label193 | Label | Alte inf. | 408,714 1009x285 | 1376 |  |
| Nume | ComboBox | SELECT Clienti.IDC, Clienti.Denumire, Clienti.Adresa, Clienti.CF, Clienti.J, Clienti.CNP FROM Clienti ORDER BY Clienti.D | 1417,144 2619x285 | 1401 |  |
| Label3 | Label | Gestionar | 408,144 1009x285 | 1439 |  |
| Adaug | CommandButton | √ | 6624,999 3867x339 | 1463 | OnClick |
| Pret | TextBox |  | 7752,714 2739x285 | 1533 |  |
| Label58 | Label | Preț | 6624,714 1125x285 | 1565 |  |

## PageFooterSection (inaltime 0 twips, linia 1590)

_fara controale_

## FormFooter (inaltime 744 twips, linia 1600)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| btnSavBC | CommandButton | SALVEAZĂ | 10620,48 1351x633 | 1607 | OnClick |
| btnAddBC | CommandButton | ADAUGĂ | 9164,48 1351x633 | 1677 | OnClick |
| btnIesire | CommandButton | IESIRE | 168,48 1351x633 | 1750 | OnClick |
| btnModBC | CommandButton | MODIFICĂ | 7762,48 1351x633 | 1822 | OnClick |
| btnStergBC | CommandButton | ȘTERGE | 14190,48 1350x633 | 1895 | OnClick |
| btnUndoBC | CommandButton | RENUNȚĂ | 6360,48 1351x633 | 1968 | OnClick |
| btnListBC | CommandButton | LISTEAZĂ | 3336,48 1350x633 | 2040 | OnClick |

