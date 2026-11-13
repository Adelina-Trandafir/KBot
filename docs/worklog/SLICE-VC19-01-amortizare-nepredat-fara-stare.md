# SLICE-VC19-01 — Raport Amortizare: fără coloana «Stare», valori și pentru bunurile nepredate

13.11.2026 — Cerere (din raportul «Amortizare» la 31.10.2026, patru bunuri «Nepredat»): (1) scoasă coloana «Stare»; (2) «Valoare lunară» apare indiferent dacă există amortizare în luna aleasă; (3) «Amortizare cumulată» nu mai e goală; (4) «Luni folosite» = luni deja folosite (DF) + luni amortizate, nu gol.

## Cauza celor patru celule goale

Toate cele patru bunuri nu aveau nicio predare până la 31.10.2026, deci `depreciation.calculate` ieșea cu starea `not_in_use` înainte de orice calcul și lăsa `used_months`, `remaining_months`, `monthly`, `accumulated`, `remaining` pe `None` (afișat «—»). Era intenționat în codul vechi («niciun 0 inventat»), dar nu mai corespunde cu ce vrea Adelina.

## Ce s-a schimbat

- `PYTHON/routes/vercon/depreciation.py`: valoarea lunară se calculează acum înainte de verificarea predării, pentru orice bun valid și intrat. Pentru `not_in_use`: luni calendaristice 0, `used_months = DF`, `remaining_months = DNF - DF`, `accumulated = DF × lunar` (aceeași formulă ca la bunurile active), `remaining = valoare - cumulat`, amortizarea lunii și a lunii următoare 0. Pentru bunurile predate calculul e neschimbat (tot `DF + luni calendaristice de la prima predare`).
- `PYTHON/routes/vercon/report_layouts.py`: scoasă coloana «Stare» din raportul de amortizare. (Fereastra «Amortizare la dată» din Registru MF își păstrează coloana «Stare», cererea fiind despre raport.)
- `PYTHON/static/help/vercon.json`: propoziție nouă în «Calcul la o dată», etichetă VC19-01.

## Pasul 2 (aceeași zi): grupare pe clasificare și centralizator

Cerere: «o grupare cu totaluri în subsol pe clasificare» și «un centralizator Amortizări grupat pe clasificări». Clasificare = `VC_Produse.CC`, cu numele din `AVACONT_COMUN.VC_ClasificariMF` (aceeași etichetă «cod / denumire» ca în arborele de selecție).

- `PYTHON/routes/vercon/depreciation_report.py` (nou): `names`, `label`, `summarize`.
- `report_data.py` (`dataset`): fiecare rând primește `classification`; la raportul-centralizator rândurile se însumează pe clasificare (`summary=True`).
- `report_layouts.py`: Amortizare are `groups=['classification']` și totaluri pe cele 5 coloane de sume (un tabel pe clasificare, total în subsol, TOTAL GENERAL la final când sunt mai multe clasificări); centralizatorul are coloanele Clasificare, Nr. bunuri, cele 5 sume, fără grupare, cu rând de total.
- `report_catalog.py`: raport nou `Centralizator_Amortizari` (MF, grupul «Mijloace fixe», `kind='depreciation'` + `summary=True`, deci interfața existentă — selecția, «La data», «Grupează după Primitori» — merge fără JS nou). `report_mf.py`: mod de selecție `depreciation` și pentru el.
- `report_body.py`: titlul grupului «Clasificare: …». `report_sort.py`: centralizatorul nu se mai sortează pe nume de produs. `report_service.py`: notă cu numărul bunurilor neincluse.
- `vercon.json`: paragraful «Calcul la o dată» completat (VC19-01).

Decizii luate de mine, de confirmat:
- Bunurile **intrate după data raportului** (`not_entered`) nu mai apar în niciunul din cele două rapoarte (nu au sume la acea dată și ar fi făcut «—» totalul clasificării); nota de sub raport spune câte. Înainte apăreau, cu coloana «Stare».
- Un bun cu date de corectat (`invalid`) rămâne în raportul detaliat (cu motivul în «Observații»), iar totalul clasificării lui arată «—», nu o sumă parțială. Același lucru în centralizator.
- Dacă unitatea a salvat vreodată o machetă proprie pentru Amortizare (`instance/vercon-reports/<unitate>/layout-amortizare.json`), aceea suprascrie noile valori implicite (grupare, fără «Stare»); nu am putut verifica pe server.

## Presupuneri (de confirmat de Adelina)

- Pentru un bun nepredat, cumulatul = DF × valoarea lunară (nu 0). Luat din Access, unde luni folosite înainte de predare = DF.
- Bunurile `not_in_use` rămân în continuare **excluse din totalurile** ferestrei de calcul (starea nu s-a schimbat), deși rândul lor are acum valori.

## Teste

Nerulate (regula «fără teste»). **Atenție:** `VERCON/tools/test_vc08_depreciation.py`, `test_no_abs_depreciation_before_future_handover_or_entry`, afirmă `accumulated is None` pentru `not_in_use`; după această schimbare afirmația nu mai e adevărată și testul trebuie actualizat când se cer teste. Nu s-a rulat nici raportul, nici PDF-ul.

## Neverificat

- Cum arată raportul pe datele reale (valorile DF ale celor patru bunuri nu au fost citite din bază).
- Fișierele nu sunt pe server; înregistrarea pentru publicare se face abia după proba Adelinei.
