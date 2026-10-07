# Table: Variabile

## Fields

| # | Name | Type | Size | Required | Default | Validation | Description |
|---|------|------|------|----------|---------|------------|-------------|
| 0 | ID | AutoNumber | 4 | False |  |  | ID |
| 1 | P_Nume | Text | 255 | False |  |  | Nume plătitor |
| 2 | Nume | Text | 255 | False |  |  | Nume copil |
| 3 | CNP | Text | 255 | False |  |  | CNP copil |
| 4 | LA | Text | 255 | False |  |  | Luna/Anul |
| 5 | Z_Prez | Text | 255 | False |  |  | Zile Prezență |
| 6 | Z_Abs | Text | 255 | False |  |  | Zile Absență |
| 7 | D_Prez | Text | 255 | False |  |  | Detalii prezență |
| 8 | P_Suma | Text | 255 | False |  |  | Valoare lunară totala |
| 9 | R_Suma | Text | 255 | False |  |  | Valoare restanță |
| 10 | T_Suma | Text | 255 | False |  |  | Valoare totala lunară - Valoare restanță |
| 11 | C_Suma | Text | 255 | False |  |  | Valoare lunară contract |
| 12 | M_Suma | Text | 255 | False |  |  | Valoare lunară hrană |
| 13 | A_Suma | Text | 255 | False |  |  | Valoare avans inițial |
| 14 | F_Suma | Text | 255 | False |  |  | Reducere lunară frate |
| 15 | T_DocP | Text | 255 | False |  |  | Fel document plată |
| 16 | N_DocP | Text | 255 | False |  |  | Număr document plată |
| 17 | D_DocP | Text | 255 | False |  |  | Detalii document plată |

## Indexes

- Nume: (Nume)
- PrimaryKey: (ID) PRIMARY UNIQUE
