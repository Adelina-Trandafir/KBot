# SLICE-VC06-40 — Eliminarea informării despre recalcularea prețului

Data: 05.11.2026.

La cererea Adelinei se elimină mesajul «Prețul a fost recalculat din valoarea facturii și cantitatea primită. Îl puteți modifica.» din editorul NIR, la schimbarea cantității primite pe o linie preluată din eFactura.

În `efacturaPrice` se elimină numai apelul de informare. Calculul `Pret = ef_net / Cant`, rotunjirea la patru zecimale, condițiile de eligibilitate și păstrarea prețului introdus manual rămân neschimbate. Celelalte mesaje nu sunt afectate.

## Fișiere proprii și motivul înregistrării

Motivul înregistrării: «Eliminarea informării despre recalcularea prețului la schimbarea cantității primite în NIR din eFactura, cu păstrarea calculului automat.»

- `PYTHON/static/js/vercon/docs.js`: eliminarea apelului de informare.
- `docs/worklog/SLICE-VC06-40-fara-informare-recalculare-pret.md`: cerere, modificare și stare.
- `VERCON/VERCON_STATUS.md`: stare și legătură la worklog.
- `implementare_vercon_v2.md`: intrare datată în indexul istoric.

## Stare și limite

**ÎNREGISTRAT PENTRU SERVER, LA CEREREA ADELINEI.** La 05.11.2026, după confirmarea «am verificat si validat: inregistreaza fisierele», Adelina a semnalat și rândul `static/js/vercon/docs.js` rămas «FĂRĂ GIT» («si asta: ?»), completând cererea de înregistrare cu această modificare VERCON. Lotul anterior AD20-01–13 privea ADECHIT; această completare păstrează slice-ul și motivul propriu VC06-40.

Toate cele patru fișiere proprii enumerate mai sus, inclusiv documentația, sunt înregistrate prin `tools/Register-DeploymentTask.ps1` pentru versiunea curentă. Agentul nu a rulat teste, compilare, verificări automate sau de execuție, browser, SQL, publicare ori Git.
