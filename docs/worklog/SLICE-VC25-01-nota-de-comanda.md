# SLICE-VC25-01 — Macheta notei de comandă

24.11.2026 — Nota de comandă folosește implicit formularul din fotografia din 20.11.2026, pe A4 în orientare peisaj. Macheta include emitentul, furnizorul, câmpurile de comandă, expediere și plată, tabelul cu opt coloane și semnăturile Director, Administrator și Vizat CFP. Se generează pagini separate pe furnizor, cu până la 12 poziții pe pagină. PDF și Excel folosesc aceleași date și rubrici.

Raportul este disponibil în **Rapoarte → Magazie → Intrări** pentru materiale și alimente, în **Achiziții** pentru obiecte de inventar și în **Mijloace fixe** pentru gestiunile MF. Se refolosește selecția din NIR-uri. Cantitățile și valorile se cumulează pe produs, furnizor, variantă și preț din recepție. Numărul și data comenzii, condițiile de livrare și celelalte rubrici care nu au o valoare în sursa raportului rămân libere pentru completare.

Macheta anterioară este păstrată în generatorul generic, iar definițiile deja salvate pe unități rămân pe disc. Pentru a reactiva formatul anterior, se setează `ENABLED = False` în `PYTHON/routes/vercon/report_purchase_order.py`.

25.11.2026 — Ultimul rând al tabelului afișează totalul coloanei **Valoare**. Pentru o comandă pe mai multe pagini, totalul însumează toate pozițiile furnizorului și apare pe ultima lui pagină, în PDF și Excel.

Fișierele de cod și resurse ale lucrării

- `PYTHON/routes/vercon/report_purchase_order.py`, `report_catalog.py`, `report_data.py`, `report_layouts.py`, `report_validation.py`, `report_service.py`, `report_routes.py`, `report_stock_card.py`, `report_incoming_selection.py` și `report_selection.py`
- `PYTHON/reporting/form_grid.py` și `output.py`
- `PYTHON/static/js/vercon/report-result.js` și `src/KBot.Domain/VerconReportLogic.vb`
- `VERCON/VERCON_STATUS.md`, `implementare_vercon_v2.md` și acest worklog

Implementat local. Proba în aplicație și publicarea sunt în așteptare. Teste nerulate.
