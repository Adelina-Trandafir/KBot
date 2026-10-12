# Trasabilitate schita formular ListaAlimente

Sursa: [Forms/ListaAlimente.txt](../../VERCON_ACCESS/Forms/ListaAlimente.txt) · imagine: [F_ListaAlimente.png](../F_ListaAlimente.png)

- Titlu (Caption): Lista Alimente
- Sursa de date (RecordSource): —
- Latime: 16314 twips · sectiuni: 5 · controale: 68

## FormHeader (inaltime 450 twips, linia 210)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label0 | Label | Listă de alimente | 0,0 10800x450 | 220 |  |
| Label44 | Label | Liste salvate | 10800,0 5514x450 | 239 |  |

## PageHeaderSection (inaltime 299 twips, linia 262)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| J | CheckBox |  | 0,0 260x240 | 272 |  |
| E | CheckBox |  | 168,0 260x240 | 283 |  |
| IDLL | TextBox |  | 432,0 516x240 | 297 |  |
| IDLISTA | TextBox |  | 948,0 516x240 | 314 |  |
| DV | TextBox |  | 1464,0 516x240 | 331 |  |
| CV | TextBox |  | 1980,0 516x240 | 348 |  |
| MDV | TextBox |  | 2496,0 516x240 | 365 |  |
| IDBC | TextBox |  | 3012,0 516x240 | 382 |  |
| DataBC | TextBox |  | 3528,0 516x240 | 399 |  |
| IDL | TextBox |  | 4044,0 516x240 | 416 |  |
| CAED | CheckBox |  | 4824,24 260x240 | 433 |  |
| Dif | TextBox | =nz([valbc],0)-nz([vall],0) | 7944,0 1011x285 | 450 |  |
| Command77 | CommandButton |  Alege B.C. | 9203,0 487x299 | 480 | OnClick |
| NrBC | TextBox |  | 9826,0 1437x285 | 550 |  |
| ValoareBon | TextBox |  | 11348,0 1437x285 | 578 |  |
| IdBonConsum | TextBox |  | 5168,0 516x240 | 608 |  |

## Detail (inaltime 5130 twips, linia 628)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| MD | Subform | Form.md | 120,2421 3405x2652 | 636 | OnExit |
| DataLA | TextBox |  | 1806,333 1749x285 | 657 | OnEnter,OnExit,OnChange |
| Label3 | Label | Data L.A.: | 123,333 1683x285 | 690 |  |
| NumarLA | TextBox |  | 1806,48 1749x285 | 714 |  |
| Label4 | Label | Număr listă | 123,48 1683x285 | 742 |  |
| MicDejun_P | TextBox |  | 120,2094 825x255 | 766 |  |
| Label54 | Label | Cant. | 120,1839 825x255 | 790 |  |
| MicDejun_V | TextBox | =nz([MicDejun_P],0)*nz([vmd],0) | 1770,2094 825x255 | 817 |  |
| Label8 | Label | Valoare | 1770,1839 825x255 | 840 |  |
| MicDejun_D | TextBox |  | 2595,2094 930x255 | 866 |  |
| Label9 | Label | Distribuit | 2595,1839 930x255 | 889 |  |
| Label11 | Label |  Mic Dejun | 120,1586 3405x255 | 915 |  |
| D | Subform | Form.d | 3692,2421 3441x2652 | 941 | OnExit |
| Dejun_P | TextBox |  | 3692,2094 825x255 | 962 |  |
| Label58 | Label | Cant. | 3692,1839 825x255 | 985 |  |
| Dejun_V | TextBox | =nz([Dejun_P],0)*nz([vd],0) | 5342,2094 825x255 | 1012 |  |
| Label29 | Label | Valoare | 5342,1839 825x255 | 1035 |  |
| Dejun_D | TextBox |  | 6167,2094 966x255 | 1062 |  |
| Label31 | Label | Distribuit | 6167,1839 966x255 | 1085 |  |
| Label32 | Label | Dejun | 3692,1586 3441x255 | 1111 |  |
| CN | Subform | Form.cn | 7263,2421 3405x2652 | 1137 | OnExit |
| Cina_P | TextBox |  | 7263,2094 825x255 | 1158 |  |
| Label62 | Label | Cant. | 7263,1839 825x255 | 1181 |  |
| Cina_V | TextBox | =nz([cina_P],0)*nz([vc],0) | 8913,2094 825x255 | 1208 |  |
| Label38 | Label | Valoare | 8913,1839 825x255 | 1231 |  |
| Cina_D | TextBox |  | 9738,2094 930x255 | 1258 |  |
| Label40 | Label | Distribuit | 9738,1839 930x255 | 1281 |  |
| VMD | TextBox |  | 945,2094 825x255 | 1307 |  |
| Label55 | Label | Preț | 945,1839 825x255 | 1329 |  |
| VD | TextBox |  | 4517,2094 825x255 | 1356 |  |
| Label59 | Label | Preț | 4517,1839 825x255 | 1378 |  |
| VC | TextBox |  | 8088,2094 825x255 | 1405 |  |
| Label63 | Label | Preț | 8088,1839 825x255 | 1427 |  |
| Label86 | Label | Cină / Gustare | 7263,1584 3405x255 | 1454 |  |
| subTreeView | Subform | Form.subTreeView | 10752,48 5442x5033 | 1480 |  |
| ValoareLista | TextBox |  | 1806,618 1749x285 | 1501 |  |
| DEP | TextBox |  | 1806,903 1749x285 | 1532 |  |
| ValL | TextBox | =Nz([MicDejun_V],0)+Nz([Dejun_V],0)+Nz([Cina_V],0) | 1804,618 1749x285 | 1563 |  |
| LEGBCLA | Subform | Form.LEGBCLA | 3692,48 6966x1150 | 1595 |  |
| Text71 | TextBox | =IIf([DEP]<0,"Depășiri","Economii") | 123,903 1683x285 | 1615 |  |
| Label68 | Label | Valoare Lista: | 123,618 1683x285 | 1661 |  |
| Label103 | Label | Comentarii Lista | 120,1260 1683x285 | 1683 |  |
| Comentarii | TextBox |  | 1803,1262 8841x285 | 1705 |  |

## PageFooterSection (inaltime 0 twips, linia 1736)

_fara controale_

## FormFooter (inaltime 766 twips, linia 1747)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| btnSavNIR | CommandButton | SALVEAZĂ | 10752,72 1350x633 | 1757 | OnClick |
| btnAddNIR | CommandButton | ADAUGĂ | 9318,72 1350x633 | 1823 | OnClick |
| btnIesire | CommandButton | IEȘIRE | 72,72 1365x633 | 1892 | OnClick |
| btnListNIR | CommandButton | LISTEAZĂ | 3264,72 1350x633 | 1962 | OnClick |
| btnModNIR | CommandButton | MODIFICĂ | 7927,72 1350x633 | 2032 | OnClick |
| btnUndoBC | CommandButton | RENUNȚĂ | 6528,72 1351x633 | 2101 | OnClick |
| btnStergNIR | CommandButton | ȘTERGE | 14844,72 1350x633 | 2168 | OnClick |

