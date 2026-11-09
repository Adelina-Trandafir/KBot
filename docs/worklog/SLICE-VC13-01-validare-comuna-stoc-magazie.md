# SLICE-VC13-01 — Validare comună pentru stocul de magazie

09.11.2026

La Grădinița 38, gestiunea Alimente, UNT 200G avea −7 BUC la 25.05.2026 și +18 BUC la 09.11.2026. Validarea pe întregul istoric putea refuza operațiuni din cauza deficitului din mai, deși soldul de pornire al lunii noiembrie era +1 BUC. Adelina a cerut aplicarea regulii din [VC06-30](SLICE-VC06-30-bc-stoc-ultima-luna-inchisa.md) în toate rutele care validează stocul de magazie.

`consumption_timeline.py` validează atât registrul din tranzacție, cât și registrul în care un NIR este înlocuit cu ciorna. Luna în lucru folosește regula pentru bonuri de consum și liste de alimente, limitată la exercițiul magaziei. În lipsa acestor documente se folosește luna documentului, cu data cea mai veche dintre cea salvată și cea propusă la editare. Crearea unui BC, inclusiv prin BP, ia în calcul luna bonului nou. Anularea reține luna înainte de operațiune. Soldul negativ la sfârșitul lunii anterioare și deficitele începând din luna în lucru sunt semnalate de același validator.

Crearea, editarea și anularea documentelor care apelau verificarea cronologică, inventarul și legarea stornărilor migrate apelează validatorul comun. La înlocuirea liniilor de BR și FCS sunt incluse și variantele scoase. Simularea și salvarea NIR folosesc aceeași alegere a variantei de preț și verifică variantele cu contribuție redusă. Regula se aplică registrului de magazie. Evidența pe deținători are propriul registru.

## Fișierele slice-ului

- `PYTHON/routes/vercon/consumption_period.py`
- `PYTHON/routes/vercon/consumption_timeline.py`
- `PYTHON/routes/vercon/protocol.py`
- `PYTHON/routes/vercon/documents.py`
- `PYTHON/routes/vercon/receipt_choices.py`
- `PYTHON/routes/vercon/receipt_rules.py`
- `PYTHON/routes/vercon/receipts.py`
- `PYTHON/routes/vercon/oi.py`
- `PYTHON/routes/vercon/inventory_write.py`
- `PYTHON/routes/vercon/migration_links.py`
- `PYTHON/routes/vercon/stock.py`
- `PYTHON/static/help/vercon.json`
- `PYTHON/reports/vercon/README.md`
- `VERCON/CONTRACT_CALCULE.md`
- `VERCON/DECIZII_DESCHISE.md`
- `VERCON/FACTURI_NIR_IMPLEMENTARE.md`
- `VERCON/tools/test_vc08_stock.py`
- `VERCON/VERCON_STATUS.md`
- `implementare_vercon_v2.md`
- `docs/worklog/SLICE-VC06-30-bc-stoc-ultima-luna-inchisa.md`
- `docs/worklog/SLICE-VC13-01-validare-comuna-stoc-magazie.md`

**Stare.** Implementat local. Testul existent acoperă deficitul recuperat înaintea lunii în lucru și același refuz în ciornă și după salvarea provizorie. Verificarea s-a limitat la analiza de design. Proba, publicarea și înregistrarea pentru Git sunt în așteptare.
