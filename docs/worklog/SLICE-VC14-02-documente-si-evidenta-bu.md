# SLICE-VC14-02 — Documente și evidență BU

09.11.2026 — Ruta dedicată BU înregistrează ieșirea din magazia sau din folosința sursei, cu marcajul transferului în afara subunității. În destinație creează un NIR normal. Dacă se alege un primitor, creează și BP și BC. Furnizorul este unitatea identificată după codul fiscal. NIR-ul folosește o cotă activă de 0% în destinație.

Documentele legate se salvează și se anulează împreună. Rapoartele preiau ieșirea BU și, la MF, amortizarea păstrează proveniența și data primei predări efective. Scriptul [VC14_02_TRANSFER_INTRE_SUBUNITATI.sql](../../sql/VC14_02_TRANSFER_INTRE_SUBUNITATI.sql) se aplică manual pe `AVACONT_SURSA`, urmat de sincronizarea bazelor VERCON.

Implementat local. Fără probe sau publicare executate în această etapă.

## Fișiere

- `PYTHON/routes/vercon/bu.py`
- `PYTHON/routes/vercon/bu_catalog.py`
- `PYTHON/routes/vercon/bu_links.py`
- `PYTHON/routes/vercon/bu_mf.py`
- `PYTHON/routes/vercon/bu_routes.py`
- `PYTHON/routes/vercon/depreciation_read.py`
- `PYTHON/routes/vercon/doc_routes.py`
- `PYTHON/routes/vercon/documents.py`
- `PYTHON/routes/vercon/mf.py`
- `PYTHON/routes/vercon/oi.py`
- `PYTHON/routes/vercon/oi_accounts.py`
- `PYTHON/routes/vercon/oi_context.py`
- `PYTHON/routes/vercon/product_movements.py`
- `PYTHON/routes/vercon/receipts.py`
- `PYTHON/routes/vercon/report_data.py`
- `PYTHON/routes/vercon/report_mf.py`
- `PYTHON/routes/vercon/report_oi.py`
- `PYTHON/routes/vercon/report_voucher.py`
- `PYTHON/routes/vercon/stock_source.py`
- `sql/VC14_02_TRANSFER_INTRE_SUBUNITATI.sql`
- `docs/worklog/SLICE-VC14-02-documente-si-evidenta-bu.md`
