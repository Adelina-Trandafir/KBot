# Table: SS_Buget

Linked table. Source: `SS_Buget`  Connect: `;DATABASE=C:\avasit\baza2020_PP.mdb`

## Fields

| # | Name | Type | Size | Required | Default | Validation | Description |
|---|------|------|------|----------|---------|------------|-------------|
| 0 | ID | AutoNumber | 4 | False |  |  |  |
| 1 | IDG | Long | 4 | False |  |  |  |
| 2 | IDP | Long | 4 | False |  |  |  |
| 3 | IDL | Long | 4 | False |  |  |  |
| 4 | IDZ | Long | 4 | False |  |  |  |
| 5 | Luna | Text | 255 | False |  |  |  |
| 6 | Anul | Long | 4 | False |  |  |  |
| 9 | Nume | Text | 255 | False |  |  |  |
| 10 | CNP | Text | 255 | False |  |  |  |
| 11 | ZilePrezenta | Long | 4 | False |  |  |  |
| 12 | SID | Long | 4 | False | 0 |  |  |
| 13 | SIC | Long | 4 | False | 0 |  |  |
| 14 | ValoareContract | Double | 8 | False |  |  |  |
| 15 | ValoareTotala | Long | 4 | False |  |  |  |
| 16 | Plata | Double | 8 | False |  |  |  |
| 17 | Restanta | Double | 8 | False |  |  |  |
| 18 | Compensare | Double | 8 | False |  |  |  |
| 19 | Anticipat | Double | 8 | False |  |  |  |
| 20 | Plati | Long | 4 | False | 0 |  |  |
| 21 | Retur | Long | 4 | False | 0 |  |  |
| 22 | SFD | Long | 4 | False | 0 |  |  |
| 23 | SFC | Long | 4 | False | 0 |  |  |
| 24 | Detalii | Memo/Long Text | 0 | False |  |  |  |
| 25 | Plecat | Yes/No | 1 | False | No |  |  |
| 30 | Educator | Text | 255 | False |  |  |  |
| 31 | Grupa | Text | 255 | False |  |  |  |

## Indexes

- ID: (ID)
- PrimaryKey: (ID) PRIMARY UNIQUE
- SID: (SID)
