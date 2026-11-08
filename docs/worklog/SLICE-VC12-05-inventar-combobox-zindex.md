# SLICE-VC12-05 — Inițializarea dropdown-urilor în inventarul OI

Data: 06.11.2026.

## Problema și corecția

Adelina a raportat că dropdown-urile miniaplicației nu se deschid, cu eroarea din browser:

```text
Uncaught (in promise) TypeError: Cannot read properties of undefined (reading 'getNext')
    at Combobox.show (combobox-ui.js:79:65)
    at Combobox.performSearch (combobox-search.js:49:12)
```

Controlul custom `Combobox` folosește `window.ZIndexManager.getNext()` la deschiderea listei. Managerul este definit de `/static/js/utils/z-max.js`, încărcat deja de pagina VERCON principală, dar omis la construirea paginii separate de inventar.

`PYTHON/static/vercon-inventar.html` încarcă acum scriptul comun **înaintea** modulului `vercon-inventar/app.js`, cu aceeași ordine de inițializare ca pagina principală. Corecția este limitată la bootstrap-ul paginii de inventar; controlul comun, sortarea, datele și API-urile nu sunt modificate.

## Stare și probă

**ÎNREGISTRAT PENTRU SERVER DUPĂ CONFIRMAREA ADELINEI (06.11.2026).** Adelina a confirmat «inregistreaza fisierele. am verificat», apoi a cerut completarea înregistrărilor lipsă ale inventarului. Stack trace-ul furnizat de Adelina și sursele au fost analizate pentru proiectare. Agentul nu a executat teste, browser, verificări de execuție, SQL, publicare sau Git.

Pentru probă se republică numai `PYTHON/static/vercon-inventar.html`, apoi se reîncarcă pagina (Ctrl+F5). Scriptul comun `z-max.js` este o resursă existentă; nu a fost modificat și nu face parte din fișierele proprii de înregistrat pentru această corecție. Nu este necesară recompilarea KBOTADMIN sau repornirea backendului pentru modificarea HTML.

Cele patru fișiere proprii, inclusiv documentația, sunt înregistrate prin `tools/Register-DeploymentTask.ps1`, cu motivul acestui slice. Înregistrarea nu execută publicare sau Git pe server.

## Fișiere proprii create/modificate

- `PYTHON/static/vercon-inventar.html`
- `docs/worklog/SLICE-VC12-05-inventar-combobox-zindex.md`
- `VERCON/VERCON_STATUS.md`
- `implementare_vercon_v2.md`
