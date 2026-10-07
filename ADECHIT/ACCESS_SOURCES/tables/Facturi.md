# Table: Facturi

Linked table. Source: `Facturi`  Connect: `;DATABASE=C:\avasit\baza2020_PP.mdb`

## Fields

| # | Name | Type | Size | Required | Default | Validation | Description |
|---|------|------|------|----------|---------|------------|-------------|
| 0 | IDF | Long | 4 | True | 0 |  |  |
| 1 | IDPL | Long | 4 | False | 0 |  |  |
| 2 | IDD | Long | 4 | False | 0 |  |  |
| 3 | IDS | Long | 4 | False | 0 |  |  |
| 4 | Serie | Text | 50 | False |  |  |  |
| 5 | Numar | Long | 4 | False | 0 |  |  |
| 6 | Data | Date/Time | 8 | False |  |  |  |
| 7 | Continut | Memo/Long Text | 0 | False |  |  |  |
| 8 | UM | Text | 50 | False |  |  |  |
| 9 | Cant | Double | 8 | False | 0 |  |  |
| 10 | PU | Double | 8 | False | 0 |  |  |
| 11 | Valoare | Double | 8 | False | 0 |  |  |
| 12 | Anulata | Yes/No | 1 | False |  |  |  |
| 13 | Rand1 | Text | 100 | False |  |  |  |
| 14 | Rand2 | Text | 100 | False |  |  |  |
| 15 | Listata | Yes/No | 1 | False | No |  |  |

## Indexes

- IDBeneficiar: (IDS)
- IDDelegat: (IDD)
- IdFact: (IDF)
- IDPL: (IDPL)
- Numar: (Numar)
- PlatiFacturi: (IDPL) FOREIGN
- PrimaryKey: (IDF) PRIMARY UNIQUE
