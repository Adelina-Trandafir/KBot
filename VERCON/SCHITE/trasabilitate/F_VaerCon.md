# Trasabilitate schita formular VaerCon

Sursa: [Forms/VaerCon.txt](../../VERCON_ACCESS/Forms/VaerCon.txt) · imagine: [F_VaerCon.png](../F_VaerCon.png)

- Titlu (Caption): VerCon 2.09®
- Sursa de date (RecordSource): —
- Latime: 8254 twips · sectiuni: 3 · controale: 12

## FormHeader (inaltime 240 twips, linia 1236)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| EroX | CheckBox |  | 30,0 260x240 | 1247 |  |
| CD | TextBox |  | 1326,0 1701x240 | 1259 |  |

## Detail (inaltime 2544 twips, linia 1275)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| an | ListBox | SELECT cale.Capital FROM cale GROUP BY cale.Capital ORDER BY cale.Capital DESC;  | 102,334 1269x1896 | 1282 | OnClick |
| Label10 | Label | Anul | 102,76 1269x258 | 1320 |  |
| Combo1 | ListBox | SELECT cale.ID, cale.Denumire, cale.Cale, cale.Detalii, cale.Adresa, cale.Reprezentant, cale.CUI, cale.J, cale.Banca, ca | 1501,330 6693x1896 | 1348 |  |
| Denumire_Label | Label | Unitatea | 1494,72 6693x258 | 1387 |  |
| boxWhole | Rectangle |  | 102,2280 8092x240 | 1415 |  |
| boxPct | Rectangle |  | 102,2280 5149x240 | 1439 |  |
| lblBox | Label | Verifica: MicDejun | 102,2280 8092x240 | 1464 |  |
| FOC | CommandButton | Command16 | 28,9 142x104 | 1492 |  |

## FormFooter (inaltime 576 twips, linia 1519)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Intrare | CommandButton |  Intră | 6967,72 1227x405 | 1528 | OnClick |
| Command8 | CommandButton |  Ieșire | 102,72 1227x405 | 1603 | OnClick |

