# Table: Prezenta

Linked table. Source: `Prezenta`  Connect: `;DATABASE=C:\avasit\baza2020_PP.mdb`

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
| 15 | Avans | Double | 8 | False | 0 |  |  |
| 16 | SI | Long | 4 | False | 0 |  |  |
| 19 | Detalii | Memo/Long Text | 0 | False |  |  |  |
| 20 | Restanta | Double | 8 | False |  |  |  |

## Indexes

- ID: (IDZ)
- IDG: (IDG)
- IDL: (IDL)
- IdPlatitor: (IDP)
- IDV: (IDV)
- LunaDPrezenta: (IDL) FOREIGN
- PlatitoriPrezenta: (IDP) FOREIGN
- PrimaryKey: (IDZ) PRIMARY UNIQUE
