# VERCON — Bara de acțiuni a listei de alimente

Data: 2026-10-23.

În consultarea unei liste, bara afișează numai Vizualizare și Modifică,
în această ordine. Istoric este scos din bară și rămâne în meniul contextual
al listei din arborele Liste salvate, împreună cu înregistrările de audit.
Eticheta Previzualizare a listei devine Vizualizare, inclusiv în meniu.

La adăugare, modificare completă sau modificare numai a porțiilor, Renunță
este aliniat la marginea stângă și Salvează lista la marginea dreaptă.
Vizualizare nu apare în editare. Mesajele sunt între cele două butoane;
pe ecrane înguste trec pe rândul următor, păstrând pozițiile butoanelor.
Salvarea este marcată ca acțiune principală. Regulile de eligibilitate,
salvare, calcule și audit rămân cele existente.

Helpul este aliniat cu noile etichete, ordinea și accesul la Istoric din
meniul arborelui. Nu au fost refăcute capturi.

Fișiere: PYTHON/static/js/vercon/lista.js;
PYTHON/static/css/vercon-lista.css; PYTHON/static/help/vercon.json;
implementare_vercon_v2.md și acest jurnal.

Stare: SCRIS LOCAL, NEPUBLICAT. Numai analiză de design pe surse; fără
teste, verificări automate/de sintaxă sau browser, conform AGENTS.md.
Fără SQL ori modificări de schemă. Publicare: JS, CSS și JSON împreună;
nu necesită repornirea serviciului Python pentru această corecție.
