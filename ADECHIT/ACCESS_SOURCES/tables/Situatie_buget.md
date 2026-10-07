# Table: Situatie_buget

## Fields

| # | Name | Type | Size | Required | Default | Validation | Description |
|---|------|------|------|----------|---------|------------|-------------|
| 0 | ID | AutoNumber | 4 | False |  |  |  |
| 1 | IDG | Long | 4 | False | 0 |  |  |
| 2 | IDP | Long | 4 | False | 0 |  |  |
| 3 | IDL | Long | 4 | False | 0 |  |  |
| 4 | IDZ | Long | 4 | False | 0 |  |  |
| 5 | Luna | Text | 255 | False |  |  |  |
| 6 | Anul | Long | 4 | False | 0 |  |  |
| 7 | Grupa | Text | 50 | False |  |  |  |
| 8 | Educator | Text | 50 | False |  |  |  |
| 9 | Nume | Text | 255 | False |  |  |  |
| 10 | CNP | Text | 255 | False |  |  |  |
| 11 | ZilePrezenta | Long | 4 | False | 0 |  |  |
| 12 | SoldInitial | Long | 4 | False | 0 |  |  |
| 13 | SID | Long | 4 | False | 0 |  |  |
| 14 | SIC | Long | 4 | False | 0 |  |  |
| 15 | ValoareContract | Double | 8 | False | 0 |  |  |
| 16 | ValoareTotala | Long | 4 | False | 0 |  |  |
| 17 | Plata | Double | 8 | False | 0 |  |  |
| 18 | Restanta | Double | 8 | False | 0 |  |  |
| 19 | Compensare | Double | 8 | False | 0 |  |  |
| 20 | Anticipat | Double | 8 | False | 0 |  |  |
| 21 | Plati | Long | 4 | False | 0 |  |  |
| 22 | Retur | Long | 4 | False | 0 |  |  |
| 23 | SFD | Long | 4 | False | 0 |  |  |
| 24 | SFC | Long | 4 | False | 0 |  |  |
| 25 | Detalii | Memo/Long Text | 0 | False |  |  |  |
| 26 | Plecat | Yes/No | 1 | False | No |  |  |

## Indexes

- ID: (ID)
- PrimaryKey: (ID) PRIMARY UNIQUE
- SID: (SID)
