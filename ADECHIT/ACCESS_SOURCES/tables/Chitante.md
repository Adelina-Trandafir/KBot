# Table: Chitante

Linked table. Source: `Chitante`  Connect: `;DATABASE=C:\avasit\baza2020_PP.mdb`

## Fields

| # | Name | Type | Size | Required | Default | Validation | Description |
|---|------|------|------|----------|---------|------------|-------------|
| 0 | IDC | Long | 4 | True | 0 |  |  |
| 1 | IDPL | Long | 4 | False | 0 |  |  |
| 2 | Data | Date/Time | 8 | False |  |  |  |
| 3 | Serie | Text | 50 | False |  |  |  |
| 4 | Numar | Long | 4 | False | 0 |  |  |
| 5 | Explicatie | Text | 255 | False |  |  |  |
| 6 | Anulata | Yes/No | 1 | False |  |  |  |
| 7 | Valoare | Double | 8 | False | 0 |  |  |
| 8 | IDL | Long | 4 | False |  |  |  |

## Indexes

- IdChit: (IDC)
- IDPL: (IDPL)
- Numar: (Numar)
- PlatiChitante: (IDPL) FOREIGN
- PrimaryKey: (IDC) PRIMARY UNIQUE
