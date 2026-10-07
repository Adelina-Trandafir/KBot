# Table: tmpPlatiRetur

## Fields

| # | Name | Type | Size | Required | Default | Validation | Description |
|---|------|------|------|----------|---------|------------|-------------|
| 0 | ID | AutoNumber | 4 | False |  |  |  |
| 1 | IDP | Long | 4 | False | 0 |  |  |
| 2 | IDZ | Long | 4 | False | 0 |  |  |
| 3 | IDS | Long | 4 | False | 0 |  |  |
| 4 | IDL | Long | 4 | False | 0 |  |  |
| 5 | IDR | Long | 4 | True | 0 |  |  |
| 6 | Data | Date/Time | 8 | False |  |  |  |
| 7 | Suma | Double | 8 | False | 0 |  |  |
| 8 | Salvata | Yes/No | 1 | False | No |  |  |
| 9 | Explicatie | Text | 255 | False |  |  |  |
| 10 | Anulat | Yes/No | 1 | False | No |  |  |
| 11 | NrDoc | Text | 255 | False |  |  |  |

## Indexes

- IDC: (IDR) UNIQUE
- IDL: (IDL)
- IDP: (IDP)
- IDS: (IDS)
- IDZ: (IDZ)
- PrimaryKey: (ID) PRIMARY UNIQUE
