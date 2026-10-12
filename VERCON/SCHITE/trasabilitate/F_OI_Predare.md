# Trasabilitate schita formular OI_Predare

Sursa: [Forms/OI_Predare.txt](../../VERCON_ACCESS/Forms/OI_Predare.txt) · imagine: [F_OI_Predare.png](../F_OI_Predare.png)

- Titlu (Caption): Adăugare / Modificare Bonuri Predare
- Sursa de date (RecordSource): —
- Latime: 15603 twips · sectiuni: 5 · controale: 49

## FormHeader (inaltime 480 twips, linia 624)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label160 | Label | Bon Predare | 0,0 10611x480 | 633 |  |
| Label1 | Label | Detalii produs ales | 10608,0 4995x480 | 651 |  |

## PageHeaderSection (inaltime 240 twips, linia 675)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| E | CheckBox |  | 6633,0 260x240 | 685 |  |
| IDBP | TextBox |  | 0,0 741x240 | 698 |  |
| IdBonConsum | TextBox |  | 768,0 741x240 | 712 |  |

## Detail (inaltime 5007 twips, linia 730)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| NrFact | TextBox |  | 7770,63 2739x285 | 738 | OnKeyDown,OnGotFocus,OnLostFocus |
| Label35 | Label | Nr. B.P. | 6642,63 1125x285 | 768 |  |
| Data | TextBox |  | 7770,348 2739x285 | 791 | OnGotFocus,OnLostFocus |
| Label36 | Label | Data B.P. | 6642,348 1125x285 | 821 |  |
| OK | CommandButton | + | 72,63 375x285 | 844 | OnClick |
| Cant | TextBox |  | 7770,918 2739x285 | 915 | OnGotFocus,OnLostFocus,OnChange |
| Label56 | Label | Cantitate | 6642,918 1125x285 | 948 |  |
| Denprodus | ComboBox | SELECT QBP.IdProdus, QBP.DenumireP, QBP.UM, formatn(Sum([Cant]),10) AS Stoc, formatn([PUTVA],14) AS Preț, QBP.COD FROM Q | 7770,633 2739x285 | 971 |  |
| Label53 | Label | Produs | 6642,633 1125x285 | 1009 |  |
| Masina | ComboBox | SELECT Masini.IdMasina, Masini.Masina FROM Masini WHERE (((Masini.TIP) Like 'OIF')) ORDER BY Masini.[Masina];  | 1777,348 4734x285 | 1032 | OnChange |
| Masina_Label | Label | Cont Primitor | 408,348 1369x285 | 1067 |  |
| Tb | Tab |  | 10605,0 4935x4920 | 1091 |  |
| Pg1 | Page |  | 10668,60 4812x4800 | 1108 |  |
| PI | TextBox |  | 14207,1215 1254x285 | 1127 |  |
| Label113 | Label | Preț unitar cu T.V.A. | 10760,1215 3447x285 | 1159 |  |
| CTI | TextBox |  | 14207,1500 1254x285 | 1183 |  |
| CTO | TextBox |  | 14207,1785 1254x285 | 1215 |  |
| DP | TextBox |  | 10760,120 4701x810 | 1247 |  |
| CTF | TextBox |  | 14207,2070 1254x285 | 1278 |  |
| Label127 | Label | Cant. totală ce poate fi scoasă | 10760,2070 3447x285 | 1310 |  |
| CTR | TextBox | =Round([ctf],2)-Round([cant],2) | 14207,2355 1254x285 | 1334 |  |
| Label131 | Label | Cant. ramasă dupa scădere | 10760,2355 3447x285 | 1377 |  |
| msn | TextBox |  | 10760,930 4701x285 | 1401 |  |
| Label118 | TextBox | ="Cant. intrată la " & [Data] | 10760,1500 3447x285 | 1432 |  |
| Label120 | TextBox | ="Cant. ieșită la " & [Data] | 10760,1785 3447x285 | 1460 |  |
| Pg2 | Page |  | 10668,24 4812x4836 | 1490 |  |
| subTreeView | Subform | Form.subTreeView | 10680,24 4707x4509 | 1509 | OnEnter,OnExit |
| TmpFactV | Subform | Form.TmpFactV | 72,1926 10443x2994 | 1536 |  |
| Expl | TextBox |  | 1777,918 4734x285 | 1558 |  |
| Label193 | Label | Alte inf. | 408,918 1369x285 | 1585 |  |
| Nume | ComboBox | SELECT Clienti.IDC, Clienti.Denumire, Clienti.Adresa, Clienti.CF, Clienti.J, Clienti.CNP FROM Clienti WHERE (((Clienti.C | 1777,63 4734x285 | 1610 |  |
| Label3 | Label | Primitor | 408,63 1369x285 | 1644 |  |
| Adaug | CommandButton | √ | 6642,1488 3867x339 | 1668 | OnClick |
| Pret | TextBox |  | 7770,1203 2739x285 | 1737 |  |
| Label58 | Label | Preț | 6642,1203 1125x285 | 1769 |  |
| MasinaP | ComboBox | SELECT Masini.IdMasina, Masini.Masina FROM Masini WHERE (((Masini.TIP) Like 'OIM')) ORDER BY Masini.Masina; | 1777,633 4734x285 | 1792 |  |
| Label205 | Label | Cont Predător | 408,633 1369x285 | 1827 |  |

## PageFooterSection (inaltime 0 twips, linia 1853)

_fara controale_

## FormFooter (inaltime 744 twips, linia 1863)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| btnSavBC | CommandButton | SALVEAZĂ | 10620,48 1351x633 | 1870 | OnClick |
| btnAddBC | CommandButton | ADAUGĂ | 9164,48 1351x633 | 1940 | OnClick |
| btnIesire | CommandButton | IESIRE | 168,48 1351x633 | 2013 | OnClick |
| btnModBC | CommandButton | MODIFICĂ | 7762,48 1351x633 | 2085 | OnClick |
| btnStergBC | CommandButton | ȘTERGE | 14190,48 1350x633 | 2158 | OnClick |
| btnUndoBC | CommandButton | RENUNȚĂ | 6360,48 1351x633 | 2231 | OnClick |
| btnListBC | CommandButton | LISTEAZĂ | 3336,48 1350x633 | 2303 | OnClick |

