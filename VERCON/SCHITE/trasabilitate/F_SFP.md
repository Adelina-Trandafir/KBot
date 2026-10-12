# Trasabilitate schita formular SFP

Sursa: [Forms/SFP.txt](../../VERCON_ACCESS/Forms/SFP.txt) · imagine: [F_SFP.png](../F_SFP.png)

- Titlu (Caption): Listare rapoarte
- Sursa de date (RecordSource): —
- Latime: 16584 twips · sectiuni: 5 · controale: 48

## FormHeader (inaltime 0 twips, linia 255)

_fara controale_

## PageHeaderSection (inaltime 285 twips, linia 264)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| J | CheckBox |  | 1728,0 260x240 | 275 |  |
| Cmb1 | ComboBox | SELECT tmpSFP.CodProdus, tmpSFP.DenumireProdus, tmpSFP.UM, tmpSFP.PUTV FROM tmpSFP WHERE (((tmpSFP.S)=True));  | 1968,0 741x255 | 288 |  |
| Cmb2 | ComboBox | SELECT tmpSFP_NIR.Denumire, tmpSFP_NIR.NrFact, tmpSFP_NIR.Data, tmpSFP_NIR.Numar, tmpSFP_NIR.IDF, tmpSFP_NIR.IdIntrare,  | 2819,0 741x255 | 321 |  |
| Cmb3 | ComboBox | SELECT tmpSFP_BC.Denumire, tmpSFP_BC.NrFact, tmpSFP_BC.Data, tmpSFP_BC.SPret, tmpSFP_BC.IdIesire, tmpSFP_BC.IdMasina FRO | 3669,0 741x255 | 356 |  |
| Cmb4 | ComboBox | SELECT tmpSFP.CodProdus, tmpSFP.DenumireProdus, tmpSFP.UM, tmpSFP.PUTV FROM tmpSFP WHERE (((tmpSFP.S)=True));  | 4519,0 741x255 | 391 |  |
| MODEL | TextBox |  | 5280,0 927x276 | 424 |  |
| Bifa1 | CheckBox |  | 6312,30 260x240 | 450 |  |
| Lab2 | Label | Selectează toate produsele | 6542,0 4425x240 | 467 |  |
| TmpTIP | TextBox |  | 0,0 1701x255 | 488 |  |
| TIP | TextBox |  | 11208,0 1871x285 | 502 | OnChange |

## Detail (inaltime 4728 twips, linia 527)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| subProd | Subform | Form.subTreeView | 8136,312 8415x4365 | 535 | OnEnter,OnExit |
| Label144 | Label | Selectare date | 8136,72 8415x240 | 555 |  |
| subOptiuni | Subform | Form.subTreeView | 3384,312 4674x4365 | 583 | OnExit |
| Label111 | Label | Tipul raportului | 3384,72 4674x240 | 603 |  |
| SelTot | CheckBox |  | 8580,105 212x192 | 631 | OnMouseMove,OnClick |
| st | CheckBox |  | 144,3365 260x210 | 653 | OnClick |
| st_lbl | Label | Arată produsele fără stoc? | 386,3312 2853x234 | 672 |  |
| Furn | CheckBox |  | 144,2508 260x210 | 701 | OnClick |
| tt_lbl | Label | Grupare furnizori / cont? | 386,2472 2853x234 | 720 |  |
| Central | CheckBox |  | 144,2796 260x210 | 748 |  |
| AUM_lbl | Label | Centralizator | 386,2760 2853x234 | 767 |  |
| Fapt | CheckBox |  | 144,3077 260x210 | 794 |  |
| Label139 | Label | Stocuri faptice? | 386,3024 2853x234 | 813 |  |
| Label147 | Label | Opțiuni afișare | 66,1833 3210x240 | 840 |  |
| Box148 | Rectangle |  | 72,2073 3210x2593 | 866 |  |
| Xls | CheckBox |  | 144,3918 260x210 | 883 |  |
| Label150 | Label | Export Excel? | 386,3888 2853x234 | 902 |  |
| Caut | ComboBox | SELECT SFP_CautCBX.Key, SFP_CautCBX.Col1, SFP_CautCBX.Col2 FROM SFP_CautCBX ORDER BY SFP_CautCBX.Col1;  | 66,1492 3210x255 | 933 | OnClick |
| Label125 | Label | Căutare... | 66,1237 3210x255 | 968 |  |
| di | TextBox |  | 1392,648 1899x255 | 996 | OnGotFocus,OnLostFocus |
| Label33 | Label | Dată început | 72,648 1320x255 | 1027 |  |
| dsf | TextBox |  | 1392,936 1899x255 | 1053 | OnGotFocus,OnLostFocus |
| Label35 | Label | Dată sfârșit | 72,936 1320x255 | 1084 |  |
| Label107 | Label | Selectează perioada | 78,72 3210x255 | 1110 |  |
| boxWhole | Rectangle |  | 72,4440 3210x240 | 1136 |  |
| boxPct | Rectangle |  | 72,4440 3210x240 | 1154 |  |
| lblBox | Label | Obiecte de inventar in folosinta | 72,4440 3210x240 | 1173 |  |
| AFB | CheckBox |  | 144,3653 260x210 | 1201 | OnClick |
| Label155 | Label | Afișează furnizori? | 386,3600 2853x234 | 1220 |  |
| TotalListaInventar | CheckBox |  | 150,4200 260x210 | 1247 |  |
| Label157 | Label | Total lista inventar? | 392,4170 2853x234 | 1266 |  |
| PerB | CommandButton |  | 84,324 3198x270 | 1295 | OnClick,OnMouseMove |
| PerL | Label |  | 108,339 2943x240 | 1331 |  |
| Subinv | CheckBox |  | 140,2206 260x210 | 1352 | OnClick |
| Label159 | Label | Grupare Subinventare? | 382,2170 2853x234 | 1371 |  |

## PageFooterSection (inaltime 0 twips, linia 1400)

_fara controale_

## FormFooter (inaltime 672 twips, linia 1410)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| btnIesire | CommandButton | IEȘIRE | 96,72 1365x525 | 1417 | OnClick |
| btnListNIR | CommandButton | Listare | 15201,72 1350x525 | 1491 | OnClick |
| btnExpX | CommandButton | Export Excel | 13752,72 1350x525 | 1565 | OnClick |

