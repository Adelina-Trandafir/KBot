# Trasabilitate schite documente OI/MF

Convenții și limite: [citirea fișelor consolidate](../INDEX.md#fise-consolidate). SLICE-D003-16, 28.10.2026. Secțiunea este coloana întâi.

## Variante și surse

### Trasabilitate schita formular OI_Predare

Sursa: [Forms/OI_Predare.txt](../../VERCON_ACCESS/Forms/OI_Predare.txt) · imagine: [F_OI_Predare.png](../F_OI_Predare.png)

- Titlu (Caption): Adăugare / Modificare Bonuri Predare · Sursa de date (RecordSource): — · Latime: 15603 twips · sectiuni: 5 · controale: 49

### Trasabilitate schita formular OI_Casare

Sursa: [Forms/OI_Casare.txt](../../VERCON_ACCESS/Forms/OI_Casare.txt) · imagine: [F_OI_Casare.png](../F_OI_Casare.png)

- Titlu (Caption): Adăugare / Modificare Bonuri Casare · Sursa de date (RecordSource): — · Latime: 15603 twips · sectiuni: 5 · controale: 50

### Trasabilitate schita formular OI_Transfer

Sursa: [Forms/OI_Transfer.txt](../../VERCON_ACCESS/Forms/OI_Transfer.txt) · imagine: [F_OI_Transfer.png](../F_OI_Transfer.png)

- Titlu (Caption): Adăugare / Modificare Bonuri Transfer · Sursa de date (RecordSource): — · Latime: 15603 twips · sectiuni: 5 · controale: 51

## Secțiuni

| Secțiune | OI_Predare | OI_Casare | OI_Transfer |
|---|---|---|---|
| FormHeader <a id="formheader-inaltime-480-twips-linia-624"></a><a id="formheader-inaltime-480-twips-linia-236"></a><a id="formheader-inaltime-480-twips-linia-249"></a> | 1 / 480 / 624 | 1 / 480 / 236 | 1 / 480 / 249 |
| PageHeaderSection <a id="pageheadersection-inaltime-240-twips-linia-675"></a><a id="pageheadersection-inaltime-285-twips-linia-286"></a><a id="pageheadersection-inaltime-285-twips-linia-299"></a> | 2 / 240 / 675 | 2 / 285 / 286 | 2 / 285 / 299 |
| Detail <a id="detail-inaltime-5007-twips-linia-730"></a><a id="detail-inaltime-4680-twips-linia-396"></a><a id="detail-inaltime-4680-twips-linia-440"></a> | 3 / 5007 / 730 | 3 / 4680 / 396 | 3 / 4680 / 440 |
| PageFooterSection <a id="pagefootersection-inaltime-0-twips-linia-1853"></a><a id="pagefootersection-inaltime-0-twips-linia-1502"></a><a id="pagefootersection-inaltime-0-twips-linia-1579"></a> | 4 / 0 / 1853 | 4 / 0 / 1502 | 4 / 0 / 1579 |
| FormFooter <a id="formfooter-inaltime-744-twips-linia-1863"></a><a id="formfooter-inaltime-744-twips-linia-1512"></a><a id="formfooter-inaltime-744-twips-linia-1589"></a> | 5 / 744 / 1863 | 5 / 744 / 1512 | 5 / 744 / 1589 |

## Controale

| Secțiune | Control | Tip | Text / camp / sursa | Pozitie comună (twips) | Evenimente | OI_Predare | OI_Casare | OI_Transfer |
|---|---|---|---|---|---|---|---|---|
| FormHeader | Label160 | Label | Bon Predare | 0,0 10611x480 |  | 1:633 | — | — |
| FormHeader | Label1 | Label | Detalii produs ales | 10608,0 4995x480 |  | 2:651 | 2:262 | — |
| FormHeader | Label160 | Label | Bon Casare | 0,0 10611x480 |  | — | 1:244 | — |
| FormHeader | Label160 | Label | Bon Transfer | 0,0 10611x480 |  | — | — | 1:257 |
| FormHeader | Label1 | Label | Bonuri salvate | 10608,0 4995x480 |  | — | — | 2:275 |
| PageHeaderSection | E | CheckBox |  | 6633,0 260x240 |  | 1:685 | 1:296 | 1:309 |
| PageHeaderSection | IDBP | TextBox |  | 0,0 741x240 |  | 2:698 | — | — |
| PageHeaderSection | IdBonConsum | TextBox |  | 768,0 741x240 |  | 3:712 | — | — |
| PageHeaderSection | IDBC | TextBox |  | 0,0 741x240 |  | — | 2:309 | — |
| PageHeaderSection | Masina | ComboBox | SELECT Masini.IdMasina, Masini.Masina FROM Masini WHERE (((Masini.TIP) Like 'OIF')) ORDER BY Masini.[Masina];  | 8401,0 2619x285 | OnChange | — | 3:323 | — |
| PageHeaderSection | Masina_Label | Label | Categorie | 7392,0 1009x285 |  | — | 4:356 | — |
| PageHeaderSection | P | TextBox |  | 727,0 741x240 |  | — | 5:378 | 5:421 / 1946,0 741x240 |
| PageHeaderSection | IDBT | TextBox |  | 0,0 741x240 |  | — | — | 2:322 |
| PageHeaderSection | Masina | TextBox |  | 744,0 741x240 |  | — | — | 3:336 |
| PageHeaderSection | OK | CommandButton | + | 1512,0 375x285 | OnClick | — | — | 4:353 |
| Detail | NrFact | TextBox |  | 7770,63 2739x285 | OnKeyDown,OnGotFocus,OnLostFocus | 1:738 | 1:404 / 5034,144 1476x285 | 1:448 / 1417,429 2619x285 |
| Detail | Label35 | Label | Nr. B.P. | 6642,63 1125x285 |  | 2:768 | — | — |
| Detail | Data | TextBox |  | 7770,348 2739x285 | OnGotFocus,OnLostFocus | 3:791 | 3:457 / 5034,429 1476x285 | 3:505 / 1411,714 2619x285 |
| Detail | Label36 | Label | Data B.P. | 6642,348 1125x285 |  | 4:821 | — | — |
| Detail | OK | CommandButton | + | 72,63 375x285 | OnClick | 5:844 | 5:510 / 72,144 375x285 | — |
| Detail | Cant | TextBox |  | 7770,918 2739x285 | OnGotFocus,OnLostFocus,OnChange | 6:915 | 6:581 / 7752,429 2739x285 | 5:562 / 6924,714 3084x285 |
| Detail | Label56 | Label | Cantitate | 6642,918 1125x285 |  | 7:948 | 7:614 / 6624,429 1125x285 | 6:596 / 5645,714 1279x285 |
| Detail | Denprodus | ComboBox | SELECT QBP.IdProdus, QBP.DenumireP, QBP.UM, formatn(Sum([Cant]),10) AS Stoc, formatn([PUTVA],14) AS Preț, QBP.COD FROM Q | 7770,633 2739x285 |  | 8:971 | — | — |
| Detail | Label53 | Label | Produs | 6642,633 1125x285 |  | 9:1009 | 9:676 / 6624,144 1125x285 | 35:1480 / 5645,429 1279x285 |
| Detail | Masina | ComboBox | SELECT Masini.IdMasina, Masini.Masina FROM Masini WHERE (((Masini.TIP) Like 'OIF')) ORDER BY Masini.[Masina];  | 1777,348 4734x285 | OnChange | 10:1032 | — | — |
| Detail | Masina_Label | Label | Cont Primitor | 408,348 1369x285 |  | 11:1067 | — | — |
| Detail | Tb | Tab |  | 10605,0 4935x4610 |  | 12:1091 / 10605,0 4935x4920 | 10:699 | 7:622 |
| Detail | Pg1 | Page |  | 10668,60 4812x4488 |  | 13:1108 / 10668,60 4812x4800 | 11:716 | 8:639 |
| Detail | PI | TextBox |  | 14207,1215 1254x285 |  | 14:1127 | 25:1131 / 14207,1313 1254x285 | 9:658 |
| Detail | Label113 | Label | Preț unitar cu T.V.A. | 10760,1215 3447x285 |  | 15:1159 | 26:1163 / 10760,1313 3447x285 | 10:690 |
| Detail | CTI | TextBox |  | 14207,1500 1254x285 |  | 16:1183 | — | — |
| Detail | CTO | TextBox |  | 14207,1785 1254x285 |  | 17:1215 | — | — |
| Detail | DP | TextBox |  | 10760,120 4701x810 |  | 18:1247 | 12:735 | 11:714 |
| Detail | CTF | TextBox |  | 14207,2070 1254x285 |  | 19:1278 | 14:797 / 14207,2453 1254x285 | 12:746 / 14190,2355 1254x285 |
| Detail | Label127 | Label | Cant. totală ce poate fi scoasă | 10760,2070 3447x285 |  | 20:1310 | — | — |
| Detail | CTR | TextBox | =Round([ctf],2)-Round([cant],2) | 14207,2355 1254x285 |  | 21:1334 | 16:853 / 14207,2738 1254x285 | 14:803 / 14190,2640 1254x285 |
| Detail | Label131 | Label | Cant. ramasă dupa scădere | 10760,2355 3447x285 |  | 22:1377 | — | — |
| Detail | msn | TextBox |  | 10760,930 4701x285 |  | 23:1401 | 13:766 | 16:871 |
| Detail | Label118 | TextBox | ="Cant. intrată la " & [Data] | 10760,1500 3447x285 |  | 24:1432 | — | — |
| Detail | Label120 | TextBox | ="Cant. ieșită la " & [Data] | 10760,1785 3447x285 |  | 25:1460 | — | — |
| Detail | Pg2 | Page |  | 10668,24 4812x4524 |  | 26:1490 / 10668,24 4812x4836 | 27:1189 | 24:1120 |
| Detail | subTreeView | Subform | Form.subTreeView | 10680,24 4707x4509 | OnEnter,OnExit | 27:1509 | 28:1208 | 25:1139 |
| Detail | TmpFactV | Subform | Form.TmpFactV | 72,1926 10443x2994 |  | 28:1536 | — | — |
| Detail | Expl | TextBox |  | 1777,918 4734x285 |  | 29:1558 | 30:1257 / 1417,429 2619x933 | 29:1256 / 1411,999 4059x285 |
| Detail | Label193 | Label | Alte inf. | 408,918 1369x285 |  | 30:1585 | 31:1284 / 408,429 1009x285 | 30:1284 / 132,999 1279x285 |
| Detail | Nume | ComboBox | SELECT Clienti.IDC, Clienti.Denumire, Clienti.Adresa, Clienti.CF, Clienti.J, Clienti.CNP FROM Clienti WHERE (((Clienti.C | 1777,63 4734x285 |  | 31:1610 | — | — |
| Detail | Label3 | Label | Primitor | 408,63 1369x285 |  | 32:1644 | — | — |
| Detail | Adaug | CommandButton | √ | 6642,1488 3867x339 | OnClick | 33:1668 | 34:1374 / 6624,999 3867x339 | 31:1311 / 10008,144 507x1149 |
| Detail | Pret | TextBox |  | 7770,1203 2739x285 |  | 34:1737 | 35:1445 / 7752,714 2739x285 | 32:1381 / 6924,999 3084x285 |
| Detail | Label58 | Label | Preț | 6642,1203 1125x285 |  | 35:1769 | 36:1477 / 6624,714 1125x285 | 33:1414 / 5645,999 1279x285 |
| Detail | MasinaP | ComboBox | SELECT Masini.IdMasina, Masini.Masina FROM Masini WHERE (((Masini.TIP) Like 'OIM')) ORDER BY Masini.Masina; | 1777,633 4734x285 |  | 36:1792 | — | — |
| Detail | Label205 | Label | Cont Predător | 408,633 1369x285 |  | 37:1827 | — | — |
| Detail | Label35 | Label | Nr. B.Cs. | 4128,144 906x285 |  | — | 2:434 | — |
| Detail | Label36 | Label | Data B.Cs. | 4128,429 906x285 |  | — | 4:487 | — |
| Detail | Denprodus | ComboBox | SELECT QOIC.IdProdus, QOIC.Denumirep AS Produs, QOIC.UM, QOIC.Cant AS Cant, formatn([PUTVA],11,1) AS Preț, QOIC.P AS Sur | 7752,144 2739x285 | OnChange | — | 8:637 | — |
| Detail | Label127 | Label | Cantitate disponibilă | 10760,2453 3447x285 |  | — | 15:829 | 13:779 / 10743,2355 3447x285 |
| Detail | Label131 | Label | Cant. ramasă dupa transfer | 10760,2738 3447x285 |  | — | 17:896 | 15:847 / 10743,2640 3447x285 |
| Detail | CantP | TextBox |  | 14219,1598 1254x285 |  | — | 18:920 | 17:903 / 14202,1500 1254x285 |
| Detail | Label206 | Label | Cantitate predată | 10772,1598 3447x285 |  | — | 19:952 | 18:936 / 10755,1500 3447x285 |
| Detail | CantT | TextBox |  | 14207,1883 1254x285 |  | — | 20:976 | 19:960 / 14190,1785 1254x285 |
| Detail | Label208 | Label | Cantitate transferată | 10760,1883 3447x285 |  | — | 21:1008 | 20:993 / 10743,1785 3447x285 |
| Detail | CantC | TextBox |  | 14207,2168 1254x285 |  | — | 22:1032 | 21:1017 / 14190,2070 1254x285 |
| Detail | Label210 | Label | Cantitate casată | 10760,2168 3447x285 |  | — | 23:1064 | 22:1050 / 10743,2070 3447x285 |
| Detail | IDB | TextBox |  | 14207,3023 1254x285 |  | — | 24:1088 | 23:1074 / 14190,3005 1254x285 |
| Detail | TmpFactV | Subform | Form.OI_TmpFact | 72,1536 10443x2994 |  | — | 29:1235 | — |
| Detail | Nume | ComboBox | SELECT Clienti.IDC, Clienti.Denumire, Clienti.Adresa, Clienti.CF, Clienti.J, Clienti.CNP FROM Clienti INNER JOIN QOIC ON | 1417,144 2619x285 | OnLostFocus | — | 32:1309 | — |
| Detail | Label3 | Label | Predator | 408,144 1009x285 |  | — | 33:1350 | — |
| Detail | Label35 | Label | Număr Bon | 132,429 1279x285 |  | — | — | 2:479 |
| Detail | Label36 | Label | Data Bon | 132,714 1279x285 |  | — | — | 4:536 |
| Detail | Primitor | ComboBox | SELECT Clienti.IDC, Clienti.Denumire, Clienti.Adresa, Clienti.CF, Clienti.J, Clienti.CNP FROM Clienti GROUP BY Clienti.I | 1417,144 4059x285 | OnLostFocus | — | — | 26:1166 |
| Detail | Masina_Label | Label | Primitor | 138,144 1279x285 |  | — | — | 27:1207 |
| Detail | TmpFactV | Subform | Form.oi_tmpfact | 72,1536 10443x2994 |  | — | — | 28:1233 |
| Detail | Denprodus | ComboBox | SELECT QOIT.IdProdus, QOIT.Denumirep AS Produs, QOIT.UM, QOIT.Cant AS Cant, formatn([PUTVA],11,1) AS Preț, QOIT.P AS Sur | 6924,429 3084x285 | OnLostFocus | — | — | 34:1440 |
| Detail | Nume | ComboBox | SELECT Clienti.IDC, Clienti.Denumire, Clienti.Adresa, Clienti.CF, Clienti.J, Clienti.CNP FROM Clienti INNER JOIN QOIT ON | 6924,144 3084x285 |  | — | — | 36:1506 |
| Detail | Label3 | Label | Predător | 5645,144 1279x285 |  | — | — | 37:1551 |
| FormFooter | btnSavBC | CommandButton | SALVEAZĂ | 10620,48 1351x633 | OnClick | 1:1870 | 1:1519 | 1:1596 |
| FormFooter | btnAddBC | CommandButton | ADAUGĂ | 9164,48 1351x633 | OnClick | 2:1940 | 2:1589 | 2:1667 |
| FormFooter | btnIesire | CommandButton | IESIRE | 168,48 1351x633 | OnClick | 3:2013 | 3:1662 | 3:1739 |
| FormFooter | btnModBC | CommandButton | MODIFICĂ | 7762,48 1351x633 | OnClick | 4:2085 | 4:1734 | 4:1811 |
| FormFooter | btnStergBC | CommandButton | ȘTERGE | 14190,48 1350x633 | OnClick | 5:2158 | 5:1807 | 5:1883 |
| FormFooter | btnUndoBC | CommandButton | RENUNȚĂ | 6360,48 1351x633 | OnClick | 6:2231 | 6:1880 | 6:1955 |
| FormFooter | btnListBC | CommandButton | LISTEAZĂ | 3336,48 1350x633 | OnClick | 7:2303 | 7:1952 | 7:2028 |
