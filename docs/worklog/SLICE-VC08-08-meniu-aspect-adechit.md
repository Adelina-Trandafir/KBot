# 2026-10-19 — Bara VERCON cu aspectul de referință ADECHIT

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

ADECHIT este sursa pentru aspectul barei. Diferențele proveneau din reguli separate: VERCON fixa 46px prin `vercon-workspace.css`, în timp ce ADECHIT primea minimum 53px pe desktop din stilurile Portal. VERCON avea padding lateral de 12px, față de 28px în ADECHIT, și butonul nativ «ⓘ Ajutor», față de butonul compact «?» din ADECHIT.

`js/components/menu/application-header.css` furnizează acum dimensiunile comune pentru cele două bare: 53px pe desktop, 46px pe ecrane mici, margine interioară de 28px / 8px și lățime maximă de 2048px, centrată. VERCON preia tipografia și lipsa paddingului butonului de aplicații, spațierea și aspectul acțiunilor din dreapta și marcajul «?» pentru Ajutor. Butonul Zoom păstrează dimensiunea textului și paddingul ca în ADECHIT. Culorile barei, inclusiv în tema întunecată, urmează ADECHIT; avertizarea de final de sesiune continuă să schimbe fundalul în portocaliu.

Spațiul principal ADECHIT scade înălțimea efectivă a barei comune, inclusiv cei 53px de pe desktop. Pe VERCON îngust se păstrează al doilea rând al selectoarelor de context, cu înălțimea barei adaptată conținutului.

Fișiere: `PYTHON/static/vercon.html`, `PYTHON/static/css/vercon.css`, `PYTHON/static/css/adechit.css`, `PYTHON/static/js/components/menu/application-header.css`, `PYTHON/static/js/components/menu/menu-zoom.css`, acest jurnal și `implementare_vercon_v2.md`.

Numai analiză de design a surselor. Fără teste, build-uri sau verificări automate/vizuale, conform AGENTS.md. Fără acces MariaDB sau modificări de schemă. Necesită publicarea HTML/CSS.
