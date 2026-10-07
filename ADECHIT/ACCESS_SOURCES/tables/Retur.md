# Table: Retur

Linked table. Source: `Retur`  Connect: `;DATABASE=C:\avasit\baza2020_PP.mdb`

## Fields

| # | Name | Type | Size | Required | Default | Validation | Description |
|---|------|------|------|----------|---------|------------|-------------|
| 0 | IDR | Long | 4 | True | 0 |  |  |
| 1 | IDP | Long | 4 | False | 0 |  |  |
| 2 | IDZ | Long | 4 | False | 0 |  |  |
| 3 | IDL | Long | 4 | False | 0 |  |  |
| 4 | IDS | Long | 4 | False | 0 |  |  |
| 5 | Data | Date/Time | 8 | False |  |  |  |
| 6 | Explicatie | Text | 255 | False |  |  |  |
| 8 | Anulat | Yes/No | 1 | False | No |  |  |
| 9 | Motivul | Text | 255 | False |  |  |  |
| 10 | NrDoc | Text | 255 | False |  |  |  |
| 11 | Suma | Double | 8 | False |  |  |  |

## Indexes

- IDL: (IDL)
- IDS: (IDS)
- IDZ: (IDP)
- IDZ1: (IDZ)
- PrimaryKey: (IDR) PRIMARY UNIQUE
