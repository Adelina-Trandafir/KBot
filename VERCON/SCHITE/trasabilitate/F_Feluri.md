# Trasabilitate schita formular Feluri

Sursa: [Forms/Feluri.txt](../../VERCON_ACCESS/Forms/Feluri.txt) · imagine: [F_Feluri.png](../F_Feluri.png)

- Titlu (Caption): Adaugare / editare parteneri
- Sursa de date (RecordSource): —
- Latime: 11792 twips · sectiuni: 3 · controale: 21

## FormHeader (inaltime 391 twips, linia 776)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| Label1 | Label | Adăugare / Editare feluri mâncare.. | 0,0 11790x391 | 784 |  |
| txt_Export_Excel | TextBox |  | 0,0 741x240 | 804 |  |
| CC | TextBox |  | 737,0 681x240 | 817 |  |
| CD | TextBox |  | 1417,0 681x240 | 833 |  |
| CP | TextBox |  | 2097,0 681x240 | 849 |  |
| tOA | TextBox |  | 2777,0 681x240 | 865 |  |
| lblInfo | Label | Dublu-click pentru căutare avansată!!! | 3855,56 3600x285 | 881 |  |

## Detail (inaltime 4195 twips, linia 906)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| IdFel | TextBox |  | 2430,165 1485x285 | 915 |  |
| CodPartener Label | Label | IdFel | 630,165 1800x285 | 946 |  |
| DenFel | TextBox |  | 2437,450 4770x285 | 975 | OnKeyPress,OnLostFocus |
| DenumirePartener Label | Label | Denumire | 637,450 1800x285 | 1006 |  |
| PART | Subform | Form.Retetar | 60,1140 7995x3000 | 1035 |  |
| E | CheckBox |  | 7653,340 260x240 | 1055 |  |
| F8 | TextBox |  | 2430,735 4770x285 | 1074 | OnLostFocus |
| Label17 | Label | Alte detalii | 630,735 1800x285 | 1104 |  |
| LstFel | ListBox | SELECT FeluriMancare.IDFel, FeluriMancare.DenFel, FeluriMancare.Explicatie FROM FeluriMancare ORDER BY FeluriMancare.Den | 8160,60 3561x4087 | 1132 | OnClick |

## FormFooter (inaltime 566 twips, linia 1164)

| Control | Tip | Text / camp / sursa | Pozitie (twips) | Linia | Evenimente |
|---|---|---|---|---:|---|
| AddPart | CommandButton |  <- Adăugare | 8445,56 1642x456 | 1172 | OnClick |
| StergPart | CommandButton |  <- Ștergere | 5161,56 1642x456 | 1238 |  |
| Iesire | CommandButton |  <- Ieșire | 105,45 2032x456 | 1304 | OnClick |
| SavPart | CommandButton |  <- Salvare | 10087,56 1642x456 | 1350 | OnClick |
| ModPart | CommandButton |  <- Modificare | 6803,56 1642x456 | 1420 | OnClick |

