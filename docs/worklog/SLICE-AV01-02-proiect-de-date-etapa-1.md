# SLICE-AV01-02 - proiect de date, etapa 1

Data: 23.10.2026. Felie doar de documentare: niciun cod, niciun DDL rulat, nimic pe server.

## Ce s-a facut si de ce
Dupa fisele AV01-01 si deciziile operatorului, am scris propunerea de model de date pentru etapa 1:
`docs/access-map/avacont/proiect-de-date-etapa-1.md` (tabele `TZ_*`, cheia de subunitate, drepturi `AV_STERGE_NC` / `AV_STERGE_CLSF`,
Clasificatii Venituri aduse la modelul normal fara a atinge nomenclatorul citit de `extrase.py`, migrarea din `baza2026.accdb`).

## Fisiere atinse
Create: `docs/access-map/avacont/proiect-de-date-etapa-1.md`, `docs/worklog/state/KBOT_STATUS_AV.md`, acest worklog.
Modificate: `docs/worklog/KBOT_STATUS.md` (un rand nou, familia AV).

## Verificari (ce am citit)
`MariaDB_Schema/000_DEMO.sql` si `AVACONT_COMUN.sql` (tabele `Unitati`, `VC_Subunitati`, `Roluri*`, `Sectiuni`, `DefaSursaSector`),
`AVACONT_SURSA.sql` (`Clasificatii*`, `Clasificatii_Venituri*`, `Parteneri*`), `PYTHON/routes/forexe/extrase.py` (cum citeste
`Clasificatii_Venituri`). Constatari: `DefaCont`/`DefaClase` nu sunt in dump-ul COMUN din 16.10.2026; sectiunea de roluri `AV` exista deja;
`Clasificatii_Venituri` nu are `IdUnitate` prin design si e citita fara filtru pe unitate.

## Teste
Niciunul (regula operatorului). Niciun build: nu s-a atins cod.

## Nerezolvat / neverificat
Tot ce e marcat DE VERIFICAT / DE CONFIRMAT in sectiunea 8 a documentului. Propunerea NU e confirmata de operator.
Nu s-a rulat nimic pe server; nimic commitat (regula operatorului).
