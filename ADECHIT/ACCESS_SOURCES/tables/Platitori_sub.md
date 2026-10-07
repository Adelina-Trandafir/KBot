# Table: Platitori_sub

Linked table. Source: `Platitori_sub`  Connect: `;DATABASE=C:\avasit\baza2020_PP.mdb`

## Fields

| # | Name | Type | Size | Required | Default | Validation | Description |
|---|------|------|------|----------|---------|------------|-------------|
| 0 | IDS | Long | 4 | True | 0 |  |  |
| 1 | IDP | Long | 4 | False | 0 |  |  |
| 2 | Nume | Text | 50 | False |  |  |  |
| 3 | Adresa | Text | 255 | False |  |  |  |
| 4 | CUI | Text | 255 | False |  |  |  |
| 5 | J | Text | 255 | False |  |  |  |
| 6 | Cont | Text | 255 | False |  |  |  |
| 7 | Banca | Text | 255 | False |  |  |  |
| 8 | EMail | Text | 255 | False |  |  |  |
| 9 | TrimiteMail | Yes/No | 1 | False | No |  |  |
| 10 | Activ | Yes/No | 1 | False | No |  |  |

## Indexes

- IdCopil: (IDS)
- IdPlatitor: (IDP)
- Nume: (Nume)
- PlatitoriPlatitori_sub: (IDP) FOREIGN
- PrimaryKey: (IDS) PRIMARY UNIQUE
