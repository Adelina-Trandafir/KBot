# Trasabilitate schita formular FCS

Sursa: [Forms/FCS.txt](../../VERCON_ACCESS/Forms/FCS.txt) · imagine: [F_FCS.png](../F_FCS.png)

- Titlu (Caption): Facturi stornare
- Sursa de date (RecordSource): —
- Latime: 15555 twips · sectiuni: 5 · controale: 68

## FormHeader (inaltime 480 twips, linia 269)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label0 | Label | Introducere factură stornare NIR | 96,0 10341x480 | 277 |  |
| Label94 | Label | Storno salvate | 10560,0 4995x480 | 297 |  |

## PageHeaderSection (inaltime 285 twips, linia 320)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| DisableTree | CheckBox |  | 1224,0 260x226 | 330 |  |
| E | CheckBox |  | 1896,24 227x226 | 344 |  |
| IdIntrare | TextBox |  | 0,0 1184x226 | 363 |  |
| Masina | ComboBox | SELECT [Masini].[IdMasina], [Masini].[Masina] FROM Masini ORDER BY [Masina];  | 3720,0 2286x285 | 379 | OnChange |
| Masina_Label | Label | Categorie | 2760,0 960x285 | 412 |  |
| DataV | TextBox |  | 6207,0 1184x271 | 434 |  |
| IdIntrareV | TextBox |  | 7474,0 1184x226 | 453 |  |

## Detail (inaltime 5256 twips, linia 474)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Nume | ComboBox | SELECT Furnizori.IDF, Furnizori.Denumire, Furnizori.CF FROM NIR INNER JOIN Furnizori ON NIR.IDF = Furnizori.IDF WHERE (( | 1347,72 2286x285 | 482 | OnEnter,OnExit,OnChange |
| Label3 | Label | Furnizor | 387,72 960x285 | 526 |  |
| NrFact | TextBox |  | 4872,72 1656x285 | 551 | OnLostFocus |
| Label35 | Label | Nr. fact. | 3720,72 1152x285 | 582 |  |
| Data | TextBox |  | 4872,357 1656x285 | 606 | OnLostFocus |
| Label36 | Label | Dată fact. | 3720,357 1152x285 | 638 |  |
| Pret | TextBox |  | 7875,642 2577x285 | 662 |  |
| Label58 | Label | Preț | 6939,642 936x285 | 694 |  |
| Adaug | CommandButton |  Validează | 6951,1212 3504x285 | 718 | OnClick |
| Label63 | Label | Produse adaugate... | 96,1686 1575x210 | 791 |  |
| Cant | TextBox |  | 7875,357 2577x285 | 812 | OnGotFocus,OnLostFocus |
| Label56 | Label | Cantitate | 6939,357 936x285 | 847 |  |
| NrNIR | TextBox |  | 4872,642 1656x285 | 871 | OnEnter,OnExit,OnGotFocus |
| Label84 | Label | Nr. NIR | 3720,642 1152x285 | 906 |  |
| DataNIR | TextBox |  | 4872,927 1656x285 | 930 |  |
| Label85 | Label | Data NIR | 3720,927 1152x285 | 961 |  |
| Filtru1 | ComboBox | SELECT [%$##@_Alias].IDF, [%$##@_Alias].Furnizor FROM (SELECT '*' as IDF, ' <TOȚI FURNIZORII> ' as Furnizor from caletem | 12120,4656 3355x259 | 985 |  |
| IdIntrare_Label | Label | Filtru furnizori | 10560,4656 1560x259 | 1029 |  |
| TVA | ComboBox | SELECT TVA.IDtva, TVA.TVA, TVA.Explicatie FROM TVA ORDER BY TVA.TVA;  | 7878,927 2577x285 | 1053 |  |
| TVA_Etichet | Label | TVA | 6942,927 936x285 | 1089 |  |
| DenProdus | ComboBox | SELECT QFCS.IdProdus, QFCS.DenumireP, QFCS.UM, QFCS.PUTVA, QFCS.Cant, QFCS.Pret, QFCS.TVA FROM QFCS ORDER BY QFCS.Denumi | 7875,72 2577x285 | 1113 | OnEnter,OnExit,OnGotFocus |
| Label109 | Label | Produsul | 6939,72 936x285 | 1154 |  |
| TmpFact | Subform | Form.tmpFact | 96,1896 10341x3258 | 1179 | OnEnter |
| Filtru2 | ComboBox | SELECT [%$##@_Alias].IdM, [%$##@_Alias].Expr1001 AS Categorie FROM (SELECT '*' as IdM, ' <TOATE CATEGORIILE> ' FROM Cale | 12120,4915 3355x259 | 1201 |  |
| Label136 | Label | Filtru categorie | 10560,4915 1560x259 | 1242 |  |
| Comisie | ComboBox | SELECT ComisieNIR.IdComisie, ComisieNIR.Receptie1, ComisieNIR.Receptie2, ComisieNIR.Receptie3, ComisieNIR.Receptie4 FROM | 1347,642 2286x285 | 1266 |  |
| Label100 | Label | Comisie | 387,642 960x285 | 1304 |  |
| Hlp1 | Label | ! Se va intorduce valoare reală a facturii, așa cum este tiparită pe aceasta ! | 96,1512 10341x384 | 1328 |  |
| Hlp2 | Label | ! Numărul NIR-ului este oferit de program automat. El poate fi modificat, doar daca nu exista NIR-uri salvate cu data ul | 96,1512 10341x384 | 1352 |  |
| FactS | ComboBox | SELECT NIR.IdIntrare, NIR.NrFact, NIR.DataFc, NIR.IdMasina, NIR.Data, NIR.Numar FROM NIR WHERE (((NIR.IDF)=[Forms]![FCS] | 1344,360 2286x285 | 1377 | OnEnter,OnExit,OnChange |
| Label152 | Label | Factură | 384,360 960x285 | 1418 |  |
| Hlp3 | Label | !Atenție! Apar doar furnizorii pentru care se pot face stornări! | 96,1512 10341x384 | 1442 |  |
| Hlp4 | Label | !Atenție! Apar doar facturile care conțin produse viabile! | 96,1512 10341x384 | 1466 |  |
| Hlp5 | Label | !Atenție! Apar doar produsele care mai au stoc în prezent! | 96,1512 10341x384 | 1490 |  |
| Tb | Tab |  | 10545,0 4995x4605 | 1514 |  |
| Pg1 | Page |  | 10620,75 4845x4455 | 1542 |  |
| PI | TextBox |  | 14147,1215 1254x285 | 1563 |  |
| Label113 | Label | Preț unitar cu T.V.A. | 10700,1215 3447x285 | 1595 |  |
| CF | TextBox |  | 14147,1500 1254x285 | 1619 |  |
| Label156 | Label | Cantitate factură curentă | 10700,1500 3447x285 | 1652 |  |
| CIN | TextBox |  | 14147,1785 1254x285 | 1676 |  |
| Label157 | Label | Cantitate totală intrată | 10700,1785 3447x285 | 1709 |  |
| DP | TextBox |  | 10700,120 4701x810 | 1733 |  |
| CTF | TextBox |  | 14142,2355 1254x285 | 1765 |  |
| Label127 | Label | Cant. ce poate fi stornată | 10695,2355 3447x285 | 1798 |  |
| CTR | TextBox | =Round([CTF],[TempVars]![zec])-Round([Cant],[TempVars]![zec]) | 14142,2640 1254x285 | 1822 |  |
| Label131 | Label | Cant. ramasă dupa stornare | 10695,2640 3447x285 | 1866 |  |
| msn | TextBox |  | 10700,930 4701x285 | 1890 |  |
| COUT | TextBox |  | 14142,2070 1254x285 | 1922 |  |
| Label159 | Label | Cantitate totală ieșită | 10695,2070 3447x285 | 1955 |  |
| Pg2 | Page |  | 10620,24 4845x4509 | 1981 |  |
| subTreeView | Subform | Form.subTreeView | 10620,24 4839x4509 | 2002 | OnExit |

## PageFooterSection (inaltime 0 twips, linia 2030)

_fara controale_

## FormFooter (inaltime 744 twips, linia 2040)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| btnSavNIR | CommandButton | SALVEAZĂ | 10560,48 1350x633 | 2047 | OnClick |
| btnAddNIR | CommandButton | ADAUGĂ | 6766,48 1350x633 | 2118 | OnClick |
| btnIesire | CommandButton | IEȘIRE | 72,52 1365x633 | 2190 | OnClick |
| btnListNIR | CommandButton | LISTEAZĂ | 2231,48 1350x633 | 2265 | OnClick |
| btnModNIR | CommandButton | MODIFICĂ | 5292,55 1350x633 | 2338 | OnClick |
| btnUndoBC | CommandButton | RENUNȚĂ | 3825,48 1351x633 | 2410 | OnClick |
| btnStergNIR | CommandButton | ȘTERGE | 14125,48 1350x633 | 2482 | OnClick |

