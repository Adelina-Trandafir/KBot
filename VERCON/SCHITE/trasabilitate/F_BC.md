# Trasabilitate schite consum și retur

Convenții și limite: [citirea fișelor consolidate](../INDEX.md#fise-consolidate). SLICE-D003-16, 28.10.2026. Secțiunea este coloana întâi.

## Variante și surse

### Trasabilitate schita formular BC

Sursa: [Forms/BC.txt](../../VERCON_ACCESS/Forms/BC.txt) · imagine: [F_BC.png](../F_BC.png)

- Titlu (Caption): Bonuri de Consum · Sursa de date (RecordSource): — · Latime: 15603 twips · sectiuni: 5 · controale: 60

### Trasabilitate schita formular BR

Sursa: [Forms/BR.txt](../../VERCON_ACCESS/Forms/BR.txt) · imagine: [F_BR.png](../F_BR.png)

- Titlu (Caption): Bonuri de Consum · Sursa de date (RecordSource): — · Latime: 15603 twips · sectiuni: 5 · controale: 57

## Secțiuni

| Secțiune | BC | BR |
|---|---|---|
| FormHeader <a id="formheader-inaltime-480-twips-linia-261"></a><a id="formheader-inaltime-480-twips-linia-239"></a> | 1 / 480 / 261 | 1 / 480 / 239 |
| PageHeaderSection <a id="pageheadersection-inaltime-264-twips-linia-312"></a><a id="pageheadersection-inaltime-255-twips-linia-290"></a> | 2 / 264 / 312 | 2 / 255 / 290 |
| Detail <a id="detail-inaltime-4680-twips-linia-501"></a><a id="detail-inaltime-4680-twips-linia-468"></a> | 3 / 4680 / 501 | 3 / 4680 / 468 |
| PageFooterSection <a id="pagefootersection-inaltime-0-twips-linia-1745"></a><a id="pagefootersection-inaltime-0-twips-linia-1590"></a> | 4 / 0 / 1745 | 4 / 0 / 1590 |
| FormFooter <a id="formfooter-inaltime-744-twips-linia-1755"></a><a id="formfooter-inaltime-744-twips-linia-1600"></a> | 5 / 744 / 1755 | 5 / 744 / 1600 |

## Controale

| Secțiune | Control | Tip | Text / camp / sursa | Pozitie comună (twips) | Evenimente | BC | BR |
|---|---|---|---|---|---|---|---|
| FormHeader | Label160 | Label | Bon Consum | 0,0 10611x480 |  | 1:270 | — |
| FormHeader | Label1 | Label | Detalii produs ales | 10608,0 4995x480 |  | 2:288 | 2:266 |
| FormHeader | Label160 | Label | Bon Retur | 0,0 10611x480 |  | — | 1:247 |
| PageHeaderSection | E | CheckBox |  | 6444,24 260x240 |  | 1:322 | 5:359 / 6633,0 260x240 |
| PageHeaderSection | Label207 | Label | Edit | 5448,0 996x240 |  | 2:338 | — |
| PageHeaderSection | IDF | TextBox |  | 4644,0 717x240 |  | 3:356 | — |
| PageHeaderSection | Label206 | Label | IdFurnizor | 3648,0 996x240 |  | 4:374 | — |
| PageHeaderSection | IdIntrare | TextBox |  | 996,0 717x240 |  | 5:392 | — |
| PageHeaderSection | Label204 | Label | IdIntrare | 0,0 996x240 |  | 6:410 | — |
| PageHeaderSection | IdBonConsum | TextBox |  | 2820,0 717x240 |  | 7:426 | 10:434 / 1800,0 717x240 |
| PageHeaderSection | Label205 | Label | IdBonConsum | 1824,0 996x240 |  | 8:445 | — |
| PageHeaderSection | IGP | CheckBox |  | 6696,24 260x240 |  | 9:463 | — |
| PageHeaderSection | Label191 | Label | Inverseaza gestionar / primitor | 6956,0 2211x240 |  | 10:482 | — |
| PageHeaderSection | v | TextBox |  | 0,0 453x240 |  | — | 1:300 |
| PageHeaderSection | S19 | CheckBox |  | 3750,35 260x195 |  | — | 2:314 |
| PageHeaderSection | J | CheckBox |  | 5782,0 260x240 |  | — | 3:331 |
| PageHeaderSection | s | CheckBox |  | 7426,0 260x240 |  | — | 4:345 |
| PageHeaderSection | xx | CheckBox |  | 8503,0 260x240 |  | — | 6:373 |
| PageHeaderSection | lstCNT | TextBox |  | 9216,0 1701x255 |  | — | 7:387 |
| PageHeaderSection | DisableTree | CheckBox |  | 11832,0 260x226 |  | — | 8:403 |
| PageHeaderSection | NRBC | TextBox |  | 1080,0 741x240 |  | — | 9:418 |
| PageHeaderSection | IdBonConsum_Old | TextBox |  | 2520,0 717x240 |  | — | 11:450 |
| Detail | NrFact | TextBox |  | 5034,144 1476x285 | OnKeyDown,OnGotFocus,OnLostFocus | 1:509 | 1:476 |
| Detail | Label35 | Label | Nr. B.C. | 4128,144 906x285 |  | 2:540 | 2:506 |
| Detail | Data | TextBox |  | 5034,429 1476x285 | OnGotFocus,OnLostFocus | 3:563 | 3:529 |
| Detail | Label36 | Label | Data B.C. | 4128,429 906x285 |  | 4:593 | 4:559 |
| Detail | OK | CommandButton | + | 72,144 375x285 | OnClick | 5:616 | 5:582 |
| Detail | Cant | TextBox |  | 7752,429 2739x285 | OnGotFocus,OnLostFocus,OnChange | 6:687 | 6:653 |
| Detail | Label56 | Label | Cantitate | 6624,429 1125x285 |  | 7:720 | 7:686 |
| Detail | Denprodus | ComboBox | SELECT QBC.IdProdus, QBC.IdProdus, QBC.DenumireP, QBC.UM, formatn([PUTVA],11,1) AS Pret, formatn([cant],11,1) AS Cantita | 7752,144 2739x285 | OnEnter | 8:743 | — |
| Detail | Label53 | Label | Produs | 6624,144 1125x285 |  | 9:780 | 9:744 |
| Detail | Masina | ComboBox | SELECT Masini.IdMasina, Masini.Masina FROM Masini WHERE (((Nz([TIP],''))<>'OIM' And (Nz([TIP],''))<>'OIF')) ORDER BY Mas | 1417,429 2619x285 | OnChange | 10:803 | — |
| Detail | Masina_Label | Label | Categorie | 408,429 1009x285 |  | 11:838 | 11:803 |
| Detail | Tb | Tab |  | 10605,0 4935x4610 |  | 12:862 | 12:827 / 10605,0 4935x4608 |
| Detail | Pg1 | Page |  | 10668,60 4812x4488 |  | 13:879 | 13:844 |
| Detail | PI | TextBox |  | 14199,1215 1254x285 |  | 14:898 | 14:863 / 14207,1215 1254x285 |
| Detail | Label113 | Label | Preț unitar cu T.V.A. | 10752,1215 3447x285 |  | 15:930 | 15:895 / 10760,1215 3447x285 |
| Detail | CTI | TextBox |  | 14199,1500 1254x285 |  | 16:954 | 16:919 / 14207,1500 1254x285 |
| Detail | CTO | TextBox |  | 14199,1785 1254x285 |  | 17:987 | — |
| Detail | DP | TextBox |  | 10760,120 4701x810 |  | 18:1020 | 18:976 |
| Detail | CTF | TextBox |  | 14202,2925 1254x285 |  | 19:1052 | — |
| Detail | Label127 | Label | Cant. totală ce poate fi scoasă | 10755,2925 3447x285 |  | 20:1085 | — |
| Detail | CTR | TextBox | =Round([ctf],2)-Round([cant],2) | 14202,3210 1254x285 |  | 21:1109 | — |
| Detail | Label131 | Label | Cant. ramasă dupa scădere | 10755,3210 3447x285 |  | 22:1153 | 20:1052 / 10755,2640 3447x285 |
| Detail | msn | TextBox |  | 10760,930 4701x285 |  | 23:1177 | 21:1076 |
| Detail | Label118 | TextBox | ="Cant. intrată la " & [Data] | 10752,1500 3447x285 |  | 24:1209 | — |
| Detail | Label120 | TextBox | ="Cant. ieșită la " & [Data] | 10752,1785 3447x285 |  | 25:1238 | — |
| Detail | CBON | TextBox |  | 14199,2070 1254x285 |  | 26:1267 | — |
| Detail | Text209 | Label | Cantitate bon curent | 10752,2070 3447x285 |  | 27:1300 | — |
| Detail | CRE | TextBox |  | 14199,2355 1254x285 |  | 28:1322 | — |
| Detail | Label211 | Label | Cantitate retur | 10752,2355 3447x285 |  | 29:1355 | — |
| Detail | CS | TextBox |  | 14199,2640 1254x285 |  | 30:1379 | — |
| Detail | Label213 | Label | Cantitate ștornată | 10752,2640 3447x285 |  | 31:1412 | — |
| Detail | Pg2 | Page |  | 10668,24 4812x4524 |  | 32:1438 | 28:1281 |
| Detail | subTreeView | Subform | Form.subTreeView | 10680,24 4707x4509 | OnEnter,OnExit | 33:1457 | 29:1300 |
| Detail | TmpFactV | Subform | Form.TmpFactV | 72,1536 10443x2994 |  | 34:1484 | 30:1327 |
| Detail | Expl | TextBox |  | 1417,714 5103x285 |  | 35:1506 | 31:1349 / 1417,714 2619x453 |
| Detail | Label193 | Label | Alte inf. | 408,714 1009x285 |  | 36:1533 | 32:1376 |
| Detail | Nume | ComboBox | SELECT Clienti.IDC, Clienti.Denumire, Clienti.Adresa, Clienti.CF, Clienti.J, Clienti.CNP FROM Clienti ORDER BY Clienti.D | 1417,144 2619x285 |  | 37:1558 | 33:1401 |
| Detail | Label3 | Label | Primitor | 408,144 1009x285 |  | 38:1595 | — |
| Detail | Adaug | CommandButton | √ | 6624,999 3867x315 | OnClick | 39:1619 | 35:1463 / 6624,999 3867x339 |
| Detail | Pret | TextBox |  | 7752,714 2739x285 |  | 40:1688 | 36:1533 |
| Detail | Label58 | Label | Preț | 6624,714 1125x285 |  | 41:1720 | 37:1565 |
| Detail | Denprodus | ComboBox | SELECT QBCR.IdProdus, QBCR.IdProdus, QBCR.DenumireP, QBCR.UM, formatn([putva],11,1) AS PretF, formatn([cant],11,1) AS St | 7752,144 2739x285 |  | — | 8:709 |
| Detail | Masina | ComboBox | SELECT [Masini].[IdMasina], [Masini].[Masina] FROM Masini ORDER BY [Masina];  | 1417,429 2619x285 | OnChange | — | 10:767 |
| Detail | Label118 | Label | Cant. Bon Consum | 10760,1500 3447x285 |  | — | 17:952 |
| Detail | CTR | TextBox | =Round([cds],2)-Round([cant],2) | 14202,2640 1254x285 |  | — | 19:1008 |
| Detail | CAR | TextBox |  | 14207,2070 1254x285 |  | — | 22:1108 |
| Detail | Label206 | Label | Cant. alte Retururi | 10760,2070 3447x285 |  | — | 23:1141 |
| Detail | CDS | TextBox |  | 14202,2355 1254x285 |  | — | 24:1165 |
| Detail | Label208 | Label | Cant. disponibilă | 10755,2355 3447x285 |  | — | 25:1198 |
| Detail | BRC | TextBox |  | 14202,1785 1254x285 |  | — | 26:1222 |
| Detail | Label210 | Label | Cant. Bon Retur Curent | 10755,1785 3447x285 |  | — | 27:1255 |
| Detail | Label3 | Label | Gestionar | 408,144 1009x285 |  | — | 34:1439 |
| FormFooter | btnSavBC | CommandButton | SALVEAZĂ | 10620,48 1351x633 | OnClick | 1:1762 | 1:1607 |
| FormFooter | btnAddBC | CommandButton | ADAUGĂ | 9164,48 1351x633 | OnClick | 2:1832 | 2:1677 |
| FormFooter | btnIesire | CommandButton | IESIRE | 168,48 1351x633 | OnClick | 3:1905 | 3:1750 |
| FormFooter | btnModBC | CommandButton | MODIFICĂ | 7762,48 1351x633 | OnClick | 4:1977 | 4:1822 |
| FormFooter | btnStergBC | CommandButton | ȘTERGE | 14190,48 1350x633 | OnClick | 5:2050 | 5:1895 |
| FormFooter | btnUndoBC | CommandButton | RENUNȚĂ | 6360,48 1351x633 | OnClick | 6:2123 | 6:1968 |
| FormFooter | btnListBC | CommandButton | LISTEAZĂ | 3336,48 1350x633 | OnClick | 7:2195 | 7:2040 |
