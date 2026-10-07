# Table: Prezenta_L

## Fields

| # | Name | Type | Size | Required | Default | Validation | Description |
|---|------|------|------|----------|---------|------------|-------------|
| 0 | IDZ | Long | 4 | True | 0 |  |  |
| 1 | IDP | Long | 4 | False | 0 |  |  |
| 2 | IDL | Long | 4 | False | 0 |  |  |
| 3 | IDV | Long | 4 | False | 0 |  |  |
| 4 | IDG | Long | 4 | False | 0 |  |  |
| 5 | ZileLuna | Long | 4 | False | 0 |  |  |
| 6 | ZilePrezenta | Long | 4 | False | 0 |  |  |
| 7 | ZileAbsenta | Long | 4 | False | 0 |  |  |
| 8 | MZCPrezenta | Long | 4 | False | 0 |  | Numarul maxim de zile prezente consecutive |
| 9 | MZCAbsenta | Long | 4 | False | 0 |  | Numarul maxim de zile absente consecutive |
| 10 | ValoareContract | Double | 8 | False | 0 |  |  |
| 11 | ValoareMancare | Long | 4 | False | 0 |  |  |
| 12 | ValoareTotala | Long | 4 | False | 0 |  |  |
| 13 | ReducereFrate | Double | 8 | False | 0 |  |  |
| 14 | ReducereBonus | Long | 4 | False | 0 |  |  |
| 15 | SI | Long | 4 | False | 0 |  |  |
| 16 | Avans | Double | 8 | False | 0 |  |  |
| 17 | Detalii | Memo/Long Text | 0 | False |  |  |  |
| 18 | Sters | Yes/No | 1 | False | No |  |  |
| 19 | Edit | Yes/No | 1 | False | No |  |  |
| 20 | Murdar | Yes/No | 1 | False | No |  |  |
| 21 | Restanta | Long | 4 | False | 0 |  |  |
| 22 | SID | Long | 4 | False | 0 |  |  |
| 23 | SIC | Long | 4 | False | 0 |  |  |
| 24 | Plata | Long | 4 | False | 0 |  |  |
| 25 | Retur | Long | 4 | False | 0 |  |  |

## Indexes

- Grupe_LPrezenta_L: (IDG) FOREIGN
- ID: (IDZ)
- IDG: (IDG)
- IDL: (IDL)
- IdPlatitor: (IDP)
- IDV: (IDV)
- LunaD_LPrezenta_L: (IDL) FOREIGN
- PrimaryKey: (IDZ) PRIMARY UNIQUE
