# Table: Platitori_sub_L

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
| 10 | activ | Yes/No | 1 | False | No |  |  |
| 11 | Sters | Yes/No | 1 | False | No |  |  |
| 12 | Editat | Yes/No | 1 | False | No |  |  |
| 13 | Nou | Yes/No | 1 | False | No |  |  |

## Indexes

- {D5909822-35BE-41D5-9D85-0EA4640FA8F8}: (IDP) FOREIGN
- IdCopil: (IDS)
- IdPlatitor: (IDP)
- Nume: (Nume)
- PrimaryKey: (IDS) PRIMARY UNIQUE
