# Table: OPuri

Linked table. Source: `OPuri`  Connect: `;DATABASE=C:\avasit\baza2020_PP.mdb`

## Fields

| # | Name | Type | Size | Required | Default | Validation | Description |
|---|------|------|------|----------|---------|------------|-------------|
| 0 | IdOP | Long | 4 | True | 0 |  |  |
| 1 | IdPlatitor_sub | Long | 4 | False | 0 |  |  |
| 2 | NumarOP | Long | 4 | False | 0 |  |  |
| 3 | DataOP | Date/Time | 8 | False |  |  |  |
| 4 | ValoareOP | Double | 8 | False | 0 |  |  |

## Indexes

- IdOP: (IdOP)
- IdPlatitor_sub: (IdPlatitor_sub)
- NumarOP: (NumarOP)
- PrimaryKey: (IdOP) PRIMARY UNIQUE
