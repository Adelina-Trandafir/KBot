# SLICE-ME01-31 — Ștergerea definitivă a unui bon de consum din KBOTADMIN

09.11.2026. Implementat local. Compilarea, probele și publicarea sunt în așteptare.

În **Comenzi → VERCON → Șterge definitiv un bon de consum**, operatorul alege o bază și introduce numărul bonului sau `ID <IdBonConsum>`. Dacă numărul se repetă, rezultatul afișează data, gestiunea, exercițiul, contul, primitorul și identificatorul complet al fiecărui bon. Introducerea identificatorului complet permite selectarea exactă. Comanda prezintă bonul și numărul de poziții înainte de confirmare.

Se pot șterge bonuri active și deja anulate. Comanda oprește operația când există un retur, o asociere la o listă de alimente, o legătură cu un bon de predare sau un exercițiu închis. La confirmare, serverul blochează gestiunea, recitește bonul și liniile și verifică dacă datele corespund celor afișate. Șterge rândurile din `VC_Iesiri` și `VC_Iesiri_H` în aceeași tranzacție cu înregistrarea din Jurnal. Stocul se calculează din mișcările rămase. Pentru pozițiile istorice cu cantitate negativă se verifică dacă eliminarea lor ar produce stoc negativ.

Fișierele de implementare sunt `KBotAdmin/Views/CommandsView.vb`, `KBotAdmin/Views/CommandsView.Designer.vb`, `KBotAdmin/Server/Dtos.vb`, `PYTHON/routes/admin_cli/admin_cli.py` și `PYTHON/routes/admin_cli/vercon_bc_delete.py`.
