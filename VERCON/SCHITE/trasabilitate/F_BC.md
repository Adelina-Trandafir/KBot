# Trasabilitate schita formular BC

Sursa: [Forms/BC.txt](../../VERCON_ACCESS/Forms/BC.txt) · imagine: [F_BC.png](../F_BC.png)

- Titlu (Caption): Bonuri de Consum
- Sursa de date (RecordSource): —
- Latime: 15603 twips · sectiuni: 5 · controale: 60

## FormHeader (inaltime 480 twips, linia 261)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label160 | Label | Bon Consum | 0,0 10611x480 | 270 |  |
| Label1 | Label | Detalii produs ales | 10608,0 4995x480 | 288 |  |

## PageHeaderSection (inaltime 264 twips, linia 312)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| E | CheckBox |  | 6444,24 260x240 | 322 |  |
| Label207 | Label | Edit | 5448,0 996x240 | 338 |  |
| IDF | TextBox |  | 4644,0 717x240 | 356 |  |
| Label206 | Label | IdFurnizor | 3648,0 996x240 | 374 |  |
| IdIntrare | TextBox |  | 996,0 717x240 | 392 |  |
| Label204 | Label | IdIntrare | 0,0 996x240 | 410 |  |
| IdBonConsum | TextBox |  | 2820,0 717x240 | 426 |  |
| Label205 | Label | IdBonConsum | 1824,0 996x240 | 445 |  |
| IGP | CheckBox |  | 6696,24 260x240 | 463 |  |
| Label191 | Label | Inverseaza gestionar / primitor | 6956,0 2211x240 | 482 |  |

## Detail (inaltime 4680 twips, linia 501)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| NrFact | TextBox |  | 5034,144 1476x285 | 509 | OnKeyDown,OnGotFocus,OnLostFocus |
| Label35 | Label | Nr. B.C. | 4128,144 906x285 | 540 |  |
| Data | TextBox |  | 5034,429 1476x285 | 563 | OnGotFocus,OnLostFocus |
| Label36 | Label | Data B.C. | 4128,429 906x285 | 593 |  |
| OK | CommandButton | + | 72,144 375x285 | 616 | OnClick |
| Cant | TextBox |  | 7752,429 2739x285 | 687 | OnGotFocus,OnLostFocus,OnChange |
| Label56 | Label | Cantitate | 6624,429 1125x285 | 720 |  |
| Denprodus | ComboBox | SELECT QBC.IdProdus, QBC.IdProdus, QBC.DenumireP, QBC.UM, formatn([PUTVA],11,1) AS Pret, formatn([cant],11,1) AS Cantita | 7752,144 2739x285 | 743 | OnEnter |
| Label53 | Label | Produs | 6624,144 1125x285 | 780 |  |
| Masina | ComboBox | SELECT Masini.IdMasina, Masini.Masina FROM Masini WHERE (((Nz([TIP],''))<>'OIM' And (Nz([TIP],''))<>'OIF')) ORDER BY Mas | 1417,429 2619x285 | 803 | OnChange |
| Masina_Label | Label | Categorie | 408,429 1009x285 | 838 |  |
| Tb | Tab |  | 10605,0 4935x4610 | 862 |  |
| Pg1 | Page |  | 10668,60 4812x4488 | 879 |  |
| PI | TextBox |  | 14199,1215 1254x285 | 898 |  |
| Label113 | Label | Preț unitar cu T.V.A. | 10752,1215 3447x285 | 930 |  |
| CTI | TextBox |  | 14199,1500 1254x285 | 954 |  |
| CTO | TextBox |  | 14199,1785 1254x285 | 987 |  |
| DP | TextBox |  | 10760,120 4701x810 | 1020 |  |
| CTF | TextBox |  | 14202,2925 1254x285 | 1052 |  |
| Label127 | Label | Cant. totală ce poate fi scoasă | 10755,2925 3447x285 | 1085 |  |
| CTR | TextBox | =Round([ctf],2)-Round([cant],2) | 14202,3210 1254x285 | 1109 |  |
| Label131 | Label | Cant. ramasă dupa scădere | 10755,3210 3447x285 | 1153 |  |
| msn | TextBox |  | 10760,930 4701x285 | 1177 |  |
| Label118 | TextBox | ="Cant. intrată la " & [Data] | 10752,1500 3447x285 | 1209 |  |
| Label120 | TextBox | ="Cant. ieșită la " & [Data] | 10752,1785 3447x285 | 1238 |  |
| CBON | TextBox |  | 14199,2070 1254x285 | 1267 |  |
| Text209 | Label | Cantitate bon curent | 10752,2070 3447x285 | 1300 |  |
| CRE | TextBox |  | 14199,2355 1254x285 | 1322 |  |
| Label211 | Label | Cantitate retur | 10752,2355 3447x285 | 1355 |  |
| CS | TextBox |  | 14199,2640 1254x285 | 1379 |  |
| Label213 | Label | Cantitate ștornată | 10752,2640 3447x285 | 1412 |  |
| Pg2 | Page |  | 10668,24 4812x4524 | 1438 |  |
| subTreeView | Subform | Form.subTreeView | 10680,24 4707x4509 | 1457 | OnEnter,OnExit |
| TmpFactV | Subform | Form.TmpFactV | 72,1536 10443x2994 | 1484 |  |
| Expl | TextBox |  | 1417,714 5103x285 | 1506 |  |
| Label193 | Label | Alte inf. | 408,714 1009x285 | 1533 |  |
| Nume | ComboBox | SELECT Clienti.IDC, Clienti.Denumire, Clienti.Adresa, Clienti.CF, Clienti.J, Clienti.CNP FROM Clienti ORDER BY Clienti.D | 1417,144 2619x285 | 1558 |  |
| Label3 | Label | Primitor | 408,144 1009x285 | 1595 |  |
| Adaug | CommandButton | √ | 6624,999 3867x315 | 1619 | OnClick |
| Pret | TextBox |  | 7752,714 2739x285 | 1688 |  |
| Label58 | Label | Preț | 6624,714 1125x285 | 1720 |  |

## PageFooterSection (inaltime 0 twips, linia 1745)

_fara controale_

## FormFooter (inaltime 744 twips, linia 1755)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| btnSavBC | CommandButton | SALVEAZĂ | 10620,48 1351x633 | 1762 | OnClick |
| btnAddBC | CommandButton | ADAUGĂ | 9164,48 1351x633 | 1832 | OnClick |
| btnIesire | CommandButton | IESIRE | 168,48 1351x633 | 1905 | OnClick |
| btnModBC | CommandButton | MODIFICĂ | 7762,48 1351x633 | 1977 | OnClick |
| btnStergBC | CommandButton | ȘTERGE | 14190,48 1350x633 | 2050 | OnClick |
| btnUndoBC | CommandButton | RENUNȚĂ | 6360,48 1351x633 | 2123 | OnClick |
| btnListBC | CommandButton | LISTEAZĂ | 3336,48 1350x633 | 2195 | OnClick |

