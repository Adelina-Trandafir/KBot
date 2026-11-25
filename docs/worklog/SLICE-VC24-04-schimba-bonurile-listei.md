# SLICE-VC24-04 — Schimbarea bonurilor atașate listei de alimente

25.11.2026 — În arborele **Liste salvate**, meniul **⋮ → Schimbă bonul atașat** deschide selectorul de bonuri cu selecție multiplă și cu bonurile listei deja bifate. Comanda este disponibilă pentru listele active dintr-un exercițiu deschis, cu drept de scriere. Aceeași fereastră se folosește la adăugarea bonurilor în ciornă.

Bonurile noi trebuie să fie din aceeași gestiune și același exercițiu și să aibă data cel târziu în ziua listei. Bonurile atașate altei liste, cele anulate și retururile sunt excluse. Serverul verifică din nou la salvare. La mutarea datei listei mai devreme se verifică și bonurile păstrate. Un bon scos devine disponibil pentru altă listă. La salvarea fără bonuri se folosește dialogul comun de avertisment și confirmare.

Schimbarea se salvează într-o tranzacție cu revizie și cheie de cerere. Se calculează consumul net după retururi și diferența listei. Istoricul consemnează schimbarea și recalcularea soldurilor care depind de ea.

Selectorul pornește cu ramurile restrânse. Bifa **Grupează după dată**, sub arbore, schimbă ordinea nivelurilor între gestionar → dată și dată → gestionar. Selecția bonurilor se păstrează.

Fișierele lucrării

- Server — `PYTHON/routes/vercon/doc_routes.py`, `lista.py`, `lista_bc.py` și `food_list_portions.py`
- Interfață și ajutor — `PYTHON/static/js/vercon/lista.js`, `document-history.js`, `PYTHON/static/css/vercon-lista.css` și `PYTHON/static/help/vercon.json`
- Documentație — `VERCON/VERCON_STATUS.md`, `implementare_vercon_v2.md` și acest worklog

Implementat local. Proba în aplicație și publicarea sunt în așteptare. Teste nerulate.
