# SLICE-VC06-09 — BC în luna activă, protecția stocului și istoricul recalculărilor

Data: 2026-10-23.

Modificarea oricărui BC activ din luna activă este permisă, cu exercițiul deschis, dreptul Consum, blocarea gestiunii și versiunea curentă. Pentru Alimente se folosește ultima lună cu liste active, cu fallback la ultima lună cu BC active dacă nu există liste. Materiale folosește ultima lună cu BC active. Luna se determină pe toate exercițiile. Data modificată rămâne în aceeași lună. Un exercițiu cu documente de stoc într-un an ulterior nu este redeschis prin selectarea anului vechi.

Permisiunile de modificare sunt separate de cele de anulare. Anularea păstrează regula ultimului document și protecția BC legat la listă. Retururile active și legăturile către liste din afara lunii active blochează modificarea. Aceleași reguli sunt afișate în arbore/formular și reverificate pe server în tranzacția blocată.

Bonul este înlocuit provizoriu în tranzacție, apoi `check_timeline` validează toate datele variantelor vechi și noi, inclusiv mișcările ulterioare. Nu este suficient stocul curent pozitiv. Intrările preced ieșirile aceleiași zile. Mișcările fără dată sau cantitate sunt refuzate explicit. Un sold negativ produce rollback pentru întregul bon, lista asociată, contoare și audit.

Exemplul solicitat: din stoc 6, consum inițial 3 și ulterior 1 + 1, corecția la 5 trebuie respinsă (sold −1), la 4 admisă (sold 0), la 2 admisă (sold 2), sub rezerva celorlalte reguli. Acestea sunt rezultate de design; scenariile nu au fost executate, conform AGENTS.md.

Lista de alimente asociată se recalculează atomic chiar dacă nu este ultima din luna activă: total BC, DEP, versiune și audit. Soldurile cumulative continuă să fie calculate din istoricul actualizat, fără rescrierea artificială a documentelor ulterioare.

## Istoricul explică de ce s-au schimbat soldurile

Noua responsabilitate `recalculation_audit.py` capturează înainte și după în aceeași tranzacție. În VC_Audit existent se înregistrează:

- `bc.stock_recalculated` pe fiecare BC afectat: stocul variantelor sale afectate la sfârșitul zilei, înainte/după; cantitățile și prețurile documentului ulterior nu sunt modificate.
- `lista.bc_update` pe lista asociată: total BC și DEP, cu bonul sursă.
- `lista.balance_recalculated` pe listele ale căror solduri se schimbă: sold anterior, DEP/EC zilnic și sold final, înainte/după. Se aplică și modificărilor de porții, atât prin ruta dedicată, cât și prin editarea completă a ultimei liste.
- `bc.update` pe bonul modificat: și detaliile liniilor înainte/după.

Cauza cuprinde tip, ID, număr, dată și versiune a documentului sursă, explicație lizibilă, utilizator și ID-ul cererii. Soldurile identice nu generează evenimente de diferență. Nicio înregistrare nu persistă dacă tranzacția este respinsă. Recalculările DEP/EC folosesc sursele/calculele raportului, iar stocurile folosesc registrul comun de magazie.

Istoricul comun are înregistrări expandabile, motiv și Înainte/După, cu cele mai recente primele și previzualizare PDF. Pagina Lista Alimente are acum Istoric în bara documentului și în meniul arborelui. Helpul VERCON descrie explicit modificarea porțiilor oricărei liste din luna deschisă, modificarea BC din ultima lună deschisă, protecția cronologică a stocului, recalcularea DEP/EC și motivarea din istoric. Capturile nu sunt modificate.

Fișiere de aplicație:

- PYTHON/routes/vercon/consumption_period.py (nou)
- PYTHON/routes/vercon/recalculation_audit.py (nou)
- PYTHON/routes/vercon/document_order.py, documents.py, doc_routes.py
- PYTHON/routes/vercon/food_list_recalculation.py, food_list_portions.py, lista.py, stock_source.py
- PYTHON/static/js/vercon/document-permissions.js (nou)
- PYTHON/static/js/vercon/document-history.js (nou)
- PYTHON/static/js/vercon/docs.js, lista.js
- PYTHON/static/css/vercon-history.css
- PYTHON/static/help/vercon.json

Documentație: PYTHON/reports/vercon/README.md, implementare_vercon_v2.md, acest jurnal.

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).
