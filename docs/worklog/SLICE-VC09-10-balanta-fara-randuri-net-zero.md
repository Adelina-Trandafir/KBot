# SLICE-VC09-10 — Balanță: eliminarea rândurilor cu solduri și rulaje nete zero

Data: 30.10.2026. **IMPLEMENTAT_LOCAL, NERULAT, NEPUBLICAT.**

Utilizatorul a corectat explicit regula introdusă în VC09-09: simpla existență a documentelor în lună nu justifică păstrarea rândului dacă se compensează complet. NIR 10 și storno 10 în aceeași lună, fără sold inițial/final și fără alte rulaje nete, nu trebuie să apară în balanța implicită.

În `report_data.balances` s-au eliminat `period_activity` și indicatorul `has_movements`, iar filtrarea balanței verifică din nou numai soldul inițial, Intrările nete, Ieșirile nete și soldul final. `report_balance.prepare` aplică aceeași regulă, fără derogare pentru documente compensate. Comparația este exactă, înainte de rotunjirea afișării: un sold sau rulaj negativ ori foarte mic, dar nenul, păstrează rândul.

Exemple de proiectare, nerulate:

- SI 0, NIR 10, storno 10, Ieșiri nete 0, SF 0 → rând eliminat.
- SI 0, consum 10, retur 10, Intrări nete 0, SF 0 → rând eliminat.
- SI sau SF nenul → rând păstrat, inclusiv fără rulaj net în lună.
- Intrări nete 10 și Ieșiri nete 10, SI/SF 0 → rând păstrat: există rulaje nete în coloane.

Opțiunea existentă **Afișează FĂRĂ mișcări**, dacă este aleasă explicit în pagina Rapoarte, își păstrează funcția de a include rândurile fără solduri/rulaje. Nu a fost eliminată sau schimbată. Previzualizarea Balanță din Stocuri elimină rândurile cu toate cele patru mărimi zero, comparând șirurile zecimale exacte, fără conversie în float. Filtrul separat „Doar produse cu stoc” al grilei Stocuri nu este schimbat.

`report_balance.VERSION` este 2; mecanismul existent din VC09-09 refuză exportul modelelor vechi de balanță/rulaje până la regenerare. Ecranul/PDF/Excel folosesc același model filtrat. Calculul Intrări = NIR − storno, Ieșiri = consum − retur, soldul și reparațiile prețurilor rămân neschimbate.

Fișiere: `PYTHON/routes/vercon/report_data.py`, `report_balance.py`, `PYTHON/static/js/vercon/stock.js`; acest worklog, trimiterea din VC09-09, `VERCON/VERCON_STATUS.md`, indexul `implementare_vercon_v2.md`.

Doar analiză de proiectare și modificare de cod. **Fără teste, build, verificări automate de sintaxă, browser, SQL, date reale, modificări MariaDB sau publicare.** După actualizarea serverului/resurselor web, balanțele trebuie regenerate.
