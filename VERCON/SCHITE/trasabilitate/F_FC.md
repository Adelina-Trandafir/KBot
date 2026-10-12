# Trasabilitate schita formular FC

Sursa: [Forms/FC.txt](../../VERCON_ACCESS/Forms/FC.txt) · imagine: [F_FC.png](../F_FC.png)

- Titlu (Caption): Facturi cumparare
- Sursa de date (RecordSource): —
- Latime: 16203 twips · sectiuni: 5 · controale: 83

## FormHeader (inaltime 480 twips, linia 916)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label0 | Label | Introducere factură cumpărare (N.I.R.) | 96,0 10341x480 | 924 |  |
| Lab | Label | Facturi Salvate | 10560,0 5643x480 | 944 |  |

## PageHeaderSection (inaltime 329 twips, linia 969)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| DisableTree | CheckBox |  | 1224,0 260x226 | 979 |  |
| E | CheckBox |  | 1488,0 227x226 | 993 |  |
| IdIntrare | TextBox |  | 0,0 1184x226 | 1010 |  |
| DenumireP | TextBox |  | 1606,0 1184x226 | 1026 |  |
| UM | TextBox |  | 2941,0 1184x226 | 1045 |  |
| PUTVA | TextBox |  | 4125,0 1184x226 | 1064 |  |
| IdProdus | TextBox |  | 5309,0 1184x226 | 1083 |  |
| Text158 | TextBox |  | 6493,0 1184x226 | 1102 |  |

## Detail (inaltime 5544 twips, linia 1123)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| OK | CommandButton | + | 84,72 315x285 | 1131 | OnClick |
| AddProd | CommandButton | + | 6648,72 315x285 | 1207 | OnClick |
| Nume | ComboBox | SELECT Furnizori.IDF, Furnizori.Denumire, Furnizori.Adresa, Furnizori.CF, Furnizori.J, Furnizori.Cont FROM Furnizori WHE | 1377,72 2241x285 | 1280 |  |
| Label3 | Label | Furnizor | 387,72 990x285 | 1319 |  |
| NrFact | TextBox |  | 4872,72 1656x285 | 1345 |  |
| Label35 | Label | Nr. fact. | 3720,72 1152x285 | 1377 |  |
| Data | TextBox |  | 4872,357 1656x285 | 1402 | OnLostFocus |
| Label36 | Label | Dată fact. | 3720,357 1152x285 | 1435 |  |
| Pret | TextBox |  | 7875,642 2577x285 | 1460 |  |
| Label58 | Label | Preț unitar | 6639,642 1236x285 | 1493 |  |
| Adaug | CommandButton |  Validează | 6644,1212 3809x285 | 1518 | OnClick |
| Cant | TextBox |  | 7875,357 2577x285 | 1592 | OnLostFocus |
| Label56 | Label | Cantitate | 6639,357 1236x285 | 1626 |  |
| NrNIR | TextBox |  | 4872,927 1656x285 | 1651 | OnEnter,OnExit,OnGotFocus |
| Label84 | Label | Nr. NIR | 3720,927 1152x285 | 1687 |  |
| DataNIR | TextBox |  | 4872,642 1656x285 | 1712 |  |
| Label85 | Label | Data NIR | 3720,642 1152x285 | 1744 |  |
| TVA | ComboBox | SELECT TVA.IDtva, TVA.TVA, TVA.Explicatie FROM TVA ORDER BY TVA.TVA;  | 7875,927 2577x285 | 1769 | OnExit |
| TVA_Etichet | Label | TVA | 6639,927 1236x285 | 1809 |  |
| DenProdus | ComboBox | SELECT Produse.ID, Produse.DenumireP, UCase([UM]) AS UMS, Produse.DNF FROM Produse GROUP BY Produse.ID, Produse.Denumire | 7875,72 2577x285 | 1834 |  |
| Label109 | Label | Produsul | 6939,72 936x285 | 1879 |  |
| Masina | ComboBox | SELECT Masini.IdMasina, Masini.Masina, Nz([TIP],'') FROM Masini WHERE (((Nz([TIP],''))='OIM')) ORDER BY Masini.Masina; | 1377,357 2241x285 | 1905 |  |
| Masina_Label | Label | Categorie | 387,357 990x285 | 1945 |  |
| TmpFact | Subform | Form.tmpFactMF | 100,2169 10344x3330 | 1972 |  |
| Label63 | Label | Produse adaugate... | 100,1959 1575x210 | 1992 |  |
| Comisie | ComboBox | SELECT ComisieNIR.IdComisie, ComisieNIR.Receptie1, ComisieNIR.Receptie2, ComisieNIR.Receptie3, ComisieNIR.Receptie4 FROM | 1377,642 2241x285 | 2015 |  |
| Etichetă140 | Label | Comisie | 387,642 990x285 | 2058 |  |
| Valoare | TextBox |  | 4872,1212 1656x285 | 2083 | OnEnter,OnExit |
| Label148 | Label | Val. reală | 3720,1212 1152x285 | 2119 |  |
| Hlp1 | Label | Se va intorduce valoare reală a facturii,    așa cum este tiparită pe aceasta! | 100,1785 10344x384 | 2146 |  |
| Hlp2 | Label | Numărul NIR-ului este oferit de program automat. El poate fi modificat, doar daca nu exista NIR-uri salvate cu data ulte | 100,1785 10344x384 | 2171 |  |
| OKM | CommandButton | + | 84,357 315x285 | 2196 | OnClick |
| OKC | CommandButton | + | 84,642 315x285 | 2269 | OnClick |
| TB | Tab |  | 10450,0 5740x5544 | 2342 |  |
| P1 | Page |  | 10500,50 5640x5440 | 2362 |  |
| Filtru1 | ComboBox | SELECT [%$##@_Alias].IDF, [%$##@_Alias].Furnizor FROM (SELECT '*' as IDF, ' <TOȚI FURNIZORII> ' as Furnizor from caletem | 12094,4946 4015x259 | 2381 |  |
| IdIntrare_Label | Label | Filtru furnizori | 10534,4946 1560x259 | 2426 |  |
| Filtru2 | ComboBox | SELECT [%$##@_Alias].IdM, [%$##@_Alias].Expr1001 AS Categorie FROM (SELECT '*' as IdM, ' <TOATE CATEGORIILE> ' FROM Cale | 12094,5205 4015x259 | 2451 |  |
| Label136 | Label | Filtru categorie | 10534,5205 1560x259 | 2491 |  |
| subTreeView | Subform | Form.subTreeView | 10534,74 5571x4821 | 2515 |  |
| P2 | Page |  | 10500,50 5640x5440 | 2538 |  |
| CIN | TextBox |  | 14486,750 1254x285 | 2557 |  |
| Label118 | Label | Cantitate intrată | 11039,750 3447x285 | 2589 |  |
| CIE | TextBox |  | 14486,1035 1254x285 | 2613 |  |
| Label120 | Label | Cantitate ieșită | 11039,1035 3447x285 | 2646 |  |
| CTF | TextBox |  | 14488,1999 1584x285 | 2670 |  |
| Label127 | Label | Cantitate finală | 10665,1999 3817x285 | 2703 |  |
| CIR | TextBox |  | 14486,1320 1254x285 | 2729 |  |
| Label211 | Label | Cantitate retur | 11039,1320 3447x285 | 2762 |  |
| CIS | TextBox |  | 14486,1605 1254x285 | 2786 |  |
| Label213 | Label | Cantitate ștornată | 11039,1605 3447x285 | 2819 |  |
| PROD | Label | TABLA INTERACTIVA 75 | 10676,56 5407x285 | 2843 |  |
| CIT | TextBox |  | 14486,465 1254x285 | 2868 |  |
| Label164 | Label | Cantitate NIR curent | 11039,465 3447x285 | 2901 |  |
| P3 | Page |  | 10500,50 5640x5440 | 2927 |  |
| NrInventar | TextBox |  | 12810,72 1661x285 | 2946 | OnGotFocus |
| Label8 | Label | Număr Inventar | 10580,72 2230x285 | 2978 |  |
| DataIntrare | TextBox |  | 12810,357 1661x285 | 3003 | OnGotFocus |
| Label2 | Label | Dată intrare MF | 10580,357 2230x285 | 3036 |  |
| DNF | TextBox |  | 12810,642 1661x285 | 3061 |  |
| Label170 | Label | Durată Normală | 10580,642 2230x285 | 3094 |  |
| DF | TextBox |  | 12810,927 1661x285 | 3119 |  |
| Label4 | Label | Durată Folosită | 10580,927 2230x285 | 3151 |  |
| ValoareLunara | TextBox |  | 12810,1212 1671x285 | 3176 |  |
| Label5 | Label | Valoare Lunară | 10580,1212 2230x285 | 3210 |  |
| Adaug2 | CommandButton |  Validează | 10580,1497 3899x285 | 3235 | OnClick |

## PageFooterSection (inaltime 0 twips, linia 3312)

_fara controale_

## FormFooter (inaltime 793 twips, linia 3322)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| btnSavNIR | CommandButton | SALVEAZĂ | 10560,48 1350x633 | 3330 | OnClick |
| btnAddNIR | CommandButton | ADAUGĂ | 6766,48 1350x633 | 3401 | OnClick |
| btnIesire | CommandButton | IEȘIRE | 72,48 1365x633 | 3473 | OnClick |
| btnListNIR | CommandButton | LISTEAZĂ | 2231,48 1350x633 | 3548 | OnClick |
| btnModNIR | CommandButton | MODIFICĂ | 5292,48 1350x633 | 3621 | OnClick |
| btnUndoBC | CommandButton | RENUNȚĂ | 3825,48 1351x633 | 3693 | OnClick |
| btnStergNIR | CommandButton | ȘTERGE | 14785,48 1350x633 | 3765 | OnClick |

