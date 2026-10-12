# SLICE-VC03-03 — importuri, audit, idempotency

Stare: **IMPLEMENTAT_LOCAL**. Scripturile sunt scrise si generatorul a rulat (a produs fisierul); **nimic nu a fost aplicat pe MariaDB si nicio proba nu a fost rulata**.

## Facut
- `sql/VC_AVACONT_SURSA.sql`: `VC_Imports` (UNIQUE pe subunitate), `VC_ImportSources`, `VC_IdMap` (sursa+tabel+tip+cheie semnata), `VC_MigrationIssues`, `VC_Operations` (cheia comenzilor web, separata de markerul de import), `VC_Audit`, `VC_AnnualClosings`.
- `VC_ContextLock` (titular, jeton de sesiune, expirare) este in VC_01.

## Limite
Nerulat; atomicitatea markerului cu lotul o asigura `VRC.Migrator` (VC05-03).
