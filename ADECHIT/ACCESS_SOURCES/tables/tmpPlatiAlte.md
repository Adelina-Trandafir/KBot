# Table: tmpPlatiAlte

## Fields

| # | Name | Type | Size | Required | Default | Validation | Description |
|---|------|------|------|----------|---------|------------|-------------|
| 0 | ID | AutoNumber | 4 | False |  |  |  |
| 1 | IDPL | Long | 4 | True | 0 |  |  |
| 2 | IDP | Long | 4 | False | 0 |  |  |
| 3 | IDZ | Long | 4 | False | 0 |  |  |
| 4 | IDS | Long | 4 | False | 0 |  |  |
| 5 | IDL | Long | 4 | False | 0 |  |  |
| 6 | IDA | Long | 4 | True | 0 |  |  |
| 7 | Data | Date/Time | 8 | False |  |  |  |
| 8 | Valoare | Double | 8 | False | 0 |  |  |
| 9 | Plata | Long | 4 | False | 0 |  |  |
| 10 | TIP | Text | 255 | False |  |  |  |
| 11 | Anulata | Yes/No | 1 | False | No |  |  |
| 12 | Valid | Yes/No | 1 | False | No |  |  |
| 13 | NrDoc | Text | 50 | False |  |  |  |
| 15 | FelDoc | Text | 255 | False |  |  |  |
| 16 | Salvata | Yes/No | 1 | False | No |  |  |

## Indexes

- IDC: (IDA) UNIQUE
- IDL: (IDL)
- IDP: (IDP)
- IDPL: (IDPL) UNIQUE
- IDS: (IDS)
- IDZ: (IDZ)
- PrimaryKey: (ID) PRIMARY UNIQUE
- Valid: (Valid)
