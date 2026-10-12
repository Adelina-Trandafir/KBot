# Trasabilitate schita formular OI_Transfer

Sursa: [Forms/OI_Transfer.txt](../../VERCON_ACCESS/Forms/OI_Transfer.txt) · imagine: [F_OI_Transfer.png](../F_OI_Transfer.png)

- Titlu (Caption): Adăugare / Modificare Bonuri Transfer
- Sursa de date (RecordSource): —
- Latime: 15603 twips · sectiuni: 5 · controale: 51

## FormHeader (inaltime 480 twips, linia 249)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label160 | Label | Bon Transfer | 0,0 10611x480 | 257 |  |
| Label1 | Label | Bonuri salvate | 10608,0 4995x480 | 275 |  |

## PageHeaderSection (inaltime 285 twips, linia 299)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| E | CheckBox |  | 6633,0 260x240 | 309 |  |
| IDBT | TextBox |  | 0,0 741x240 | 322 |  |
| Masina | TextBox |  | 744,0 741x240 | 336 |  |
| OK | CommandButton | + | 1512,0 375x285 | 353 | OnClick |
| P | TextBox |  | 1946,0 741x240 | 421 |  |

## Detail (inaltime 4680 twips, linia 440)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| NrFact | TextBox |  | 1417,429 2619x285 | 448 | OnKeyDown,OnGotFocus,OnLostFocus |
| Label35 | Label | Număr Bon | 132,429 1279x285 | 479 |  |
| Data | TextBox |  | 1411,714 2619x285 | 505 | OnGotFocus,OnLostFocus |
| Label36 | Label | Data Bon | 132,714 1279x285 | 536 |  |
| Cant | TextBox |  | 6924,714 3084x285 | 562 | OnGotFocus,OnLostFocus,OnChange |
| Label56 | Label | Cantitate | 5645,714 1279x285 | 596 |  |
| Tb | Tab |  | 10605,0 4935x4610 | 622 |  |
| Pg1 | Page |  | 10668,60 4812x4488 | 639 |  |
| PI | TextBox |  | 14207,1215 1254x285 | 658 |  |
| Label113 | Label | Preț unitar cu T.V.A. | 10760,1215 3447x285 | 690 |  |
| DP | TextBox |  | 10760,120 4701x810 | 714 |  |
| CTF | TextBox |  | 14190,2355 1254x285 | 746 |  |
| Label127 | Label | Cantitate disponibilă | 10743,2355 3447x285 | 779 |  |
| CTR | TextBox | =Round([ctf],2)-Round([cant],2) | 14190,2640 1254x285 | 803 |  |
| Label131 | Label | Cant. ramasă dupa transfer | 10743,2640 3447x285 | 847 |  |
| msn | TextBox |  | 10760,930 4701x285 | 871 |  |
| CantP | TextBox |  | 14202,1500 1254x285 | 903 |  |
| Label206 | Label | Cantitate predată | 10755,1500 3447x285 | 936 |  |
| CantT | TextBox |  | 14190,1785 1254x285 | 960 |  |
| Label208 | Label | Cantitate transferată | 10743,1785 3447x285 | 993 |  |
| CantC | TextBox |  | 14190,2070 1254x285 | 1017 |  |
| Label210 | Label | Cantitate casată | 10743,2070 3447x285 | 1050 |  |
| IDB | TextBox |  | 14190,3005 1254x285 | 1074 |  |
| Pg2 | Page |  | 10668,24 4812x4524 | 1120 |  |
| subTreeView | Subform | Form.subTreeView | 10680,24 4707x4509 | 1139 | OnEnter,OnExit |
| Primitor | ComboBox | SELECT Clienti.IDC, Clienti.Denumire, Clienti.Adresa, Clienti.CF, Clienti.J, Clienti.CNP FROM Clienti GROUP BY Clienti.I | 1417,144 4059x285 | 1166 | OnLostFocus |
| Masina_Label | Label | Primitor | 138,144 1279x285 | 1207 |  |
| TmpFactV | Subform | Form.oi_tmpfact | 72,1536 10443x2994 | 1233 |  |
| Expl | TextBox |  | 1411,999 4059x285 | 1256 |  |
| Label193 | Label | Alte inf. | 132,999 1279x285 | 1284 |  |
| Adaug | CommandButton | √ | 10008,144 507x1149 | 1311 | OnClick |
| Pret | TextBox |  | 6924,999 3084x285 | 1381 |  |
| Label58 | Label | Preț | 5645,999 1279x285 | 1414 |  |
| Denprodus | ComboBox | SELECT QOIT.IdProdus, QOIT.Denumirep AS Produs, QOIT.UM, QOIT.Cant AS Cant, formatn([PUTVA],11,1) AS Preț, QOIT.P AS Sur | 6924,429 3084x285 | 1440 | OnLostFocus |
| Label53 | Label | Produs | 5645,429 1279x285 | 1480 |  |
| Nume | ComboBox | SELECT Clienti.IDC, Clienti.Denumire, Clienti.Adresa, Clienti.CF, Clienti.J, Clienti.CNP FROM Clienti INNER JOIN QOIT ON | 6924,144 3084x285 | 1506 |  |
| Label3 | Label | Predător | 5645,144 1279x285 | 1551 |  |

## PageFooterSection (inaltime 0 twips, linia 1579)

_fara controale_

## FormFooter (inaltime 744 twips, linia 1589)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| btnSavBC | CommandButton | SALVEAZĂ | 10620,48 1351x633 | 1596 | OnClick |
| btnAddBC | CommandButton | ADAUGĂ | 9164,48 1351x633 | 1667 | OnClick |
| btnIesire | CommandButton | IESIRE | 168,48 1351x633 | 1739 | OnClick |
| btnModBC | CommandButton | MODIFICĂ | 7762,48 1351x633 | 1811 | OnClick |
| btnStergBC | CommandButton | ȘTERGE | 14190,48 1350x633 | 1883 | OnClick |
| btnUndoBC | CommandButton | RENUNȚĂ | 6360,48 1351x633 | 1955 | OnClick |
| btnListBC | CommandButton | LISTEAZĂ | 3336,48 1350x633 | 2028 | OnClick |

