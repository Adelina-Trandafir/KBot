# Table: Mail

Linked table. Source: `Mail`  Connect: `;DATABASE=C:\avasit\baza2020_PP.mdb`

## Fields

| # | Name | Type | Size | Required | Default | Validation | Description |
|---|------|------|------|----------|---------|------------|-------------|
| 0 | IDM | Long | 4 | True | 0 |  |  |
| 1 | IDP | Long | 4 | False | 0 |  |  |
| 2 | IDL | Long | 4 | False | 0 |  |  |
| 3 | IDZ | Long | 4 | False | 0 |  |  |
| 4 | Trimis | Yes/No | 1 | False | No |  |  |
| 5 | DataTrimis | Date/Time | 8 | False |  |  |  |
| 6 | Rezultat | Text | 255 | False |  |  |  |
| 7 | TipMail | Text | 255 | False |  |  |  |
| 8 | FD | Long | 4 | False | 0 |  |  |

## Indexes

- IDL: (IDL)
- IDM: (IDM)
- IDP: (IDP)
- IDZ: (IDZ)
- PrezentaMail: (IDZ) FOREIGN
- PrimaryKey: (IDM) PRIMARY UNIQUE
