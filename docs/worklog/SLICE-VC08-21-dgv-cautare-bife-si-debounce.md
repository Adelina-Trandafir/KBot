# SLICE-VC08-21 — Căutare în filtrele DGV, bife și debounce

Data: 06.11.2026.

## Cerere și implementare

În filtrele pe coloane ale grilelor VERCON, fiecare modificare a textului de căutare debifează toate valorile, inclusiv cele ascunse sau peste limita de 500 de opțiuni afișate. Golirea căutării bifează din nou toate valorile, inclusiv dacă filtrul avea anterior o selecție parțială. Valorile se aleg apoi manual, individual sau prin «Selectează tot», care operează asupra tuturor rezultatelor căutării, inclusiv cele peste limita de afișare. Filtrul grilei se aplică prin butonul existent «Aplică».

Căutarea în valori pornește de la 3 caractere și se execută după 300 ms de la ultima tastare. Cu 1–2 caractere lista afișează valorile fără restrângere, debifate, și indică pragul minim. Golirea câmpului reface imediat lista completă și bifele. Potrivirea textului rămâne «conține», fără diferențiere între majuscule și minuscule, echivalentul `%text%`; nu cere tastarea semnelor `%`. Condițiile explicite alese separat în popup își păstrează operatorii.

Comportamentul este configurabil în controlul comun prin opțiunea `filterSearch` și activat doar de constructorii grilelor VERCON și VERCON Inventar. Pentru ceilalți consumatori ai DGV rămân căutarea imediată și păstrarea bifelor. Temporizările depășite sunt anulate la o nouă tastare; callbackul ignoră popupul închis/înlocuit, inclusiv după distrugerea grilei. Ordinea valorilor după tipul real rămâne cea existentă.

## Stare și livrare

**VERIFICAT DE ADELINA, ÎNREGISTRAT PENTRU SERVER (06.11.2026).** Adelina a confirmat explicit: «inregistreaza fisierele. am verificat». Agentul a făcut numai analiză de cod pentru proiectare; nu a rulat teste, verificări automate, verificări de sintaxă, browser, SQL, publicare sau Git.

Cele șase fișiere proprii de mai jos sunt înregistrate prin `tools/Register-DeploymentTask.ps1` pentru `SLICE-VC08-21`, după confirmarea verificării de către Adelina, conform AGENTS.md. Închiderea administrativă privește înregistrarea; agentul nu a executat publicare sau Git.

## Fișiere proprii create/modificate

- `PYTHON/static/js/dgv/filtering.js`
- `PYTHON/static/js/vercon/ui.js`
- `PYTHON/static/js/vercon-inventar/grids.js`
- `docs/worklog/SLICE-VC08-21-dgv-cautare-bife-si-debounce.md`
- `VERCON/VERCON_STATUS.md`
- `implementare_vercon_v2.md`
