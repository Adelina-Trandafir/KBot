# SLICE-VC06-25 — Furnizori: încadrarea antetului de opțiuni

Data: 30.10.2026.

Stare: **IMPLEMENTAT_LOCAL, NERULAT, NEPUBLICAT**.

Utilizatorul a arătat printr-o nouă captură că scrollbarul orizontal persistă după VC06-24. Proprietatea `proportionalWidths` singură nu a acoperit regulile CSS necesare grilei.

Din surse: antetul comun `.dgv__hc` are `padding: 0 22px`; coloana fixă `actions` are numai 36px. Paddingul de 44px, împreună cu bordura, depășește lățimea calculată pentru antetul respectiv. Nomenclatoarele existente au reguli suplimentare pentru această situație în `vercon-nomenclatoare.css`, limitate la `.vc-nom--grid`, clasă care nu este pe pagina Furnizori.

În `PYTHON/static/css/vercon-partners.css`, strict pentru grila Furnizori, s-au adăugat regulile echivalente: `min-width: 0` și `box-sizing: border-box` pentru antete/celule, respectiv `padding: 0` pentru antetul coloanei `actions`. Se păstrează dimensionarea proporțională a coloanelor și lățimea fixă de 36px a opțiunilor. Nu se ascunde scrollbarul prin `overflow-x: hidden` și nu se modifică DGV-ul comun.

Fără teste, verificări automate/de sintaxă, browser, HTTP sau SQL. Concluzia provine din citirea CSS-ului comun și a regulilor nomenclatoarelor, nu din măsurători în browser. Captura utilizatorului nu a fost copiată în proiect; rezultatul local nu este probat vizual. Publicarea acestei corecții cere numai `PYTHON/static/css/vercon-partners.css`, presupunând VC06-24 deja publicat.
