# SLICE-VC06-25 — Furnizori: încadrarea antetului de opțiuni

Data: 30.10.2026.

Stare: **IMPLEMENTAT_LOCAL, NERULAT, NEPUBLICAT**.

Utilizatorul a arătat printr-o nouă captură că scrollbarul orizontal persistă după VC06-24. Proprietatea `proportionalWidths` singură nu a acoperit regulile CSS necesare grilei.

Din surse: `.dgv__hc` are `padding: 0 22px`, deci 44px plus bordură depășesc antetul coloanei fixe `actions` de 36px. Regulile suplimentare din `vercon-nomenclatoare.css` sunt limitate la `.vc-nom--grid`, clasă absentă în Furnizori.

În `PYTHON/static/css/vercon-partners.css`, strict pentru Furnizori: `min-width: 0`, `box-sizing: border-box` la antete/celule și `padding: 0` la antetul `actions`. Coloanele rămân proporționale, opțiunile fixe la 36px; fără `overflow-x: hidden` sau modificarea DGV-ului comun.

Fără teste, verificări automate/de sintaxă, browser, HTTP sau SQL. Concluzia provine din citirea CSS-ului comun și a regulilor nomenclatoarelor, nu din măsurători în browser. Captura utilizatorului nu a fost copiată în proiect; rezultatul local nu este probat vizual. Publicarea acestei corecții cere numai `PYTHON/static/css/vercon-partners.css`, presupunând VC06-24 deja publicat.
