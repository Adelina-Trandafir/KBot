# 2026-10-19 — Bara VERCON cu aspectul de referință ADECHIT

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

Referință pentru bară: ADECHIT, cu minimum 53px pe desktop din stilurile Portal, padding lateral 28px și Ajutor «?». VERCON avea 46px din `vercon-workspace.css`, padding 12px și buton nativ «ⓘ Ajutor».

`js/components/menu/application-header.css`: dimensiuni comune 53px desktop / 46px ecrane mici, padding 28px / 8px, lățime maximă 2048px, centrată. VERCON preia din ADECHIT tipografia și lipsa paddingului butonului de aplicații, spațierea/aspectul acțiunilor din dreapta, Ajutor «?», textul/paddingul Zoom și culorile barei inclusiv în tema întunecată. Avertizarea de final de sesiune păstrează fundalul portocaliu.

Spațiul principal ADECHIT scade înălțimea efectivă a barei comune, inclusiv cei 53px de pe desktop. Pe VERCON îngust se păstrează al doilea rând al selectoarelor de context, cu înălțimea barei adaptată conținutului.

Fișiere: `PYTHON/static/vercon.html`, `PYTHON/static/css/vercon.css`, `PYTHON/static/css/adechit.css`, `PYTHON/static/js/components/menu/application-header.css`, `PYTHON/static/js/components/menu/menu-zoom.css`, acest jurnal și `implementare_vercon_v2.md`.

Numai analiză de design a surselor. Fără teste, build-uri sau verificări automate/vizuale, conform AGENTS.md. Fără acces MariaDB sau modificări de schemă. Necesită publicarea HTML/CSS.
