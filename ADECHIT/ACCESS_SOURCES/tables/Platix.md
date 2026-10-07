# Table: Platix

## Fields

| # | Name | Type | Size | Required | Default | Validation | Description |
|---|------|------|------|----------|---------|------------|-------------|
| 0 | IDPL | Long | 4 | True | 0 |  |  |
| 1 | IDP | Long | 4 | False | 0 |  |  |
| 2 | IDZ | Long | 4 | False | 0 |  |  |
| 3 | IDS | Long | 4 | False | 0 |  |  |
| 4 | IDL | Long | 4 | False | 0 |  |  |
| 5 | Data | Date/Time | 8 | False |  |  |  |
| 6 | Valoare | Double | 8 | False | 0 |  |  |
| 7 | Plata | Long | 4 | False | 0 |  |  |
| 8 | TIP | Text | 255 | False |  |  |  |
| 9 | Anulata | Yes/No | 1 | False | No |  |  |
| 10 | Valid | Yes/No | 1 | False | No |  |  |
| 11 | Anticipat | Double | 8 | False |  |  |  |
| 12 | Motivul | Text | 255 | False |  |  |  |
| 13 | Restanta | Double | 8 | False |  |  |  |

## Indexes

- IDL: (IDL)
- IDP: (IDP)
- IDPL: (IDPL)
- IDS: (IDS)
- IDZ: (IDZ)
- PrimaryKey: (IDPL) PRIMARY UNIQUE
- Valid: (Valid)
