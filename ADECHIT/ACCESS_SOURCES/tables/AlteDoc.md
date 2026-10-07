# Table: AlteDoc

Linked table. Source: `AlteDoc`  Connect: `;DATABASE=C:\avasit\baza2020_PP.mdb`

## Fields

| # | Name | Type | Size | Required | Default | Validation | Description |
|---|------|------|------|----------|---------|------------|-------------|
| 0 | IDA | Long | 4 | True | 0 |  |  |
| 1 | IDPL | Long | 4 | False | 0 |  |  |
| 2 | NrDoc | Text | 255 | False |  |  |  |
| 3 | FelDoc | Text | 255 | False |  |  |  |
| 4 | DataDoc | Date/Time | 8 | False |  |  |  |
| 5 | Anulata | Yes/No | 1 | False | No |  |  |
| 6 | IDL | Long | 4 | False |  |  |  |

## Indexes

- IDA: (IDA)
- IDPL: (IDPL)
- PlatiAlteDoc: (IDPL) FOREIGN
- PrimaryKey: (IDA) PRIMARY UNIQUE
