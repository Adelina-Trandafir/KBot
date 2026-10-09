# SLICE-ADE6-04 — workspace-ul de prezență

## Ce s-a schimbat și de ce

Situația lunară afișează copilul, SID/SIC, prezența, valorile și mișcările lunii. Numai
Prezența este editabilă; după salvare se recitește calculul complet. A fost adăugată operația
tranzacțională pentru preluarea copiilor activi încă neincluși în luna și grupa selectate.

## Fișiere atinse

- `PYTHON/routes/adechit/__init__.py`
- `PYTHON/routes/adechit/service.py`
- `PYTHON/static/js/adechit/app.js`
- `PYTHON/tests/test_adechit.py`

## Verificări

- suită ADE: 9 teste trecute, inclusiv metadatele editării și preluarea repetată refuzată.
- salvarea, validarea și recalcularea folosesc în continuare endpointurile comune existente.

## Neverificat sau amânat

Operația nu este probată pe MariaDB. Transferul rămâne blocat de M06.
