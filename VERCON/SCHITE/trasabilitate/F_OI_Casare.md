# Trasabilitate schita formular OI_Casare

Sursa: [Forms/OI_Casare.txt](../../VERCON_ACCESS/Forms/OI_Casare.txt) · imagine: [F_OI_Casare.png](../F_OI_Casare.png)

- Titlu (Caption): Adăugare / Modificare Bonuri Casare
- Sursa de date (RecordSource): —
- Latime: 15603 twips · sectiuni: 5 · controale: 50

## FormHeader (inaltime 480 twips, linia 236)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label160 | Label | Bon Casare | 0,0 10611x480 | 244 |  |
| Label1 | Label | Detalii produs ales | 10608,0 4995x480 | 262 |  |

## PageHeaderSection (inaltime 285 twips, linia 286)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| E | CheckBox |  | 6633,0 260x240 | 296 |  |
| IDBC | TextBox |  | 0,0 741x240 | 309 |  |
| Masina | ComboBox | SELECT Masini.IdMasina, Masini.Masina FROM Masini WHERE (((Masini.TIP) Like 'OIF')) ORDER BY Masini.[Masina];  | 8401,0 2619x285 | 323 | OnChange |
| Masina_Label | Label | Categorie | 7392,0 1009x285 | 356 |  |
| P | TextBox |  | 727,0 741x240 | 378 |  |

## Detail (inaltime 4680 twips, linia 396)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| NrFact | TextBox |  | 5034,144 1476x285 | 404 | OnKeyDown,OnGotFocus,OnLostFocus |
| Label35 | Label | Nr. B.Cs. | 4128,144 906x285 | 434 |  |
| Data | TextBox |  | 5034,429 1476x285 | 457 | OnGotFocus,OnLostFocus |
| Label36 | Label | Data B.Cs. | 4128,429 906x285 | 487 |  |
| OK | CommandButton | + | 72,144 375x285 | 510 | OnClick |
| Cant | TextBox |  | 7752,429 2739x285 | 581 | OnGotFocus,OnLostFocus,OnChange |
| Label56 | Label | Cantitate | 6624,429 1125x285 | 614 |  |
| Denprodus | ComboBox | SELECT QOIC.IdProdus, QOIC.Denumirep AS Produs, QOIC.UM, QOIC.Cant AS Cant, formatn([PUTVA],11,1) AS Preț, QOIC.P AS Sur | 7752,144 2739x285 | 637 | OnChange |
| Label53 | Label | Produs | 6624,144 1125x285 | 676 |  |
| Tb | Tab |  | 10605,0 4935x4610 | 699 |  |
| Pg1 | Page |  | 10668,60 4812x4488 | 716 |  |
| DP | TextBox |  | 10760,120 4701x810 | 735 |  |
| msn | TextBox |  | 10760,930 4701x285 | 766 |  |
| CTF | TextBox |  | 14207,2453 1254x285 | 797 |  |
| Label127 | Label | Cantitate disponibilă | 10760,2453 3447x285 | 829 |  |
| CTR | TextBox | =Round([ctf],2)-Round([cant],2) | 14207,2738 1254x285 | 853 |  |
| Label131 | Label | Cant. ramasă dupa transfer | 10760,2738 3447x285 | 896 |  |
| CantP | TextBox |  | 14219,1598 1254x285 | 920 |  |
| Label206 | Label | Cantitate predată | 10772,1598 3447x285 | 952 |  |
| CantT | TextBox |  | 14207,1883 1254x285 | 976 |  |
| Label208 | Label | Cantitate transferată | 10760,1883 3447x285 | 1008 |  |
| CantC | TextBox |  | 14207,2168 1254x285 | 1032 |  |
| Label210 | Label | Cantitate casată | 10760,2168 3447x285 | 1064 |  |
| IDB | TextBox |  | 14207,3023 1254x285 | 1088 |  |
| PI | TextBox |  | 14207,1313 1254x285 | 1131 |  |
| Label113 | Label | Preț unitar cu T.V.A. | 10760,1313 3447x285 | 1163 |  |
| Pg2 | Page |  | 10668,24 4812x4524 | 1189 |  |
| subTreeView | Subform | Form.subTreeView | 10680,24 4707x4509 | 1208 | OnEnter,OnExit |
| TmpFactV | Subform | Form.OI_TmpFact | 72,1536 10443x2994 | 1235 |  |
| Expl | TextBox |  | 1417,429 2619x933 | 1257 |  |
| Label193 | Label | Alte inf. | 408,429 1009x285 | 1284 |  |
| Nume | ComboBox | SELECT Clienti.IDC, Clienti.Denumire, Clienti.Adresa, Clienti.CF, Clienti.J, Clienti.CNP FROM Clienti INNER JOIN QOIC ON | 1417,144 2619x285 | 1309 | OnLostFocus |
| Label3 | Label | Predator | 408,144 1009x285 | 1350 |  |
| Adaug | CommandButton | √ | 6624,999 3867x339 | 1374 | OnClick |
| Pret | TextBox |  | 7752,714 2739x285 | 1445 |  |
| Label58 | Label | Preț | 6624,714 1125x285 | 1477 |  |

## PageFooterSection (inaltime 0 twips, linia 1502)

_fara controale_

## FormFooter (inaltime 744 twips, linia 1512)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| btnSavBC | CommandButton | SALVEAZĂ | 10620,48 1351x633 | 1519 | OnClick |
| btnAddBC | CommandButton | ADAUGĂ | 9164,48 1351x633 | 1589 | OnClick |
| btnIesire | CommandButton | IESIRE | 168,48 1351x633 | 1662 | OnClick |
| btnModBC | CommandButton | MODIFICĂ | 7762,48 1351x633 | 1734 | OnClick |
| btnStergBC | CommandButton | ȘTERGE | 14190,48 1350x633 | 1807 | OnClick |
| btnUndoBC | CommandButton | RENUNȚĂ | 6360,48 1351x633 | 1880 | OnClick |
| btnListBC | CommandButton | LISTEAZĂ | 3336,48 1350x633 | 1952 | OnClick |

