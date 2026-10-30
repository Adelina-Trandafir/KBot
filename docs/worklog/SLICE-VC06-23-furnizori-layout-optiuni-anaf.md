# SLICE-VC06-23 — Furnizori: layout și opțiunea ANAF în DGV

Data: 30.10.2026.

Stare: **IMPLEMENTAT_LOCAL, NERULAT, NEPUBLICAT**.

Continuarea [VC06-22](SLICE-VC06-22-gestionari-parteneri-anaf.md), după captura și cele patru cerințe ale utilizatorului:

- Căutarea CUSTOM este în antetul coloanei DGV, aliniată la marginea sa dreaptă. Coloanele antetului și ale corpului au aceleași proporții și același spațiu între ele.
- Detaliile au o singură coloană, în ordinea: Cod partener, Cod fiscal, Denumire, Cont IBAN, Bancă, Adresă. Adresa rămâne TextBox CUSTOM multiline și este ultimul câmp.
- Grila CUSTOM are coloana de opțiuni `⋮`, construită după nomenclatoarele existente. Meniul CUSTOM comun conține o singură opțiune: **Sincronizează cu ANAF**. Acțiunea selectează furnizorul rândului respectiv, cu confirmarea abandonării codului fiscal nesalvat dacă se schimbă rândul, apoi folosește fluxul ANAF existent.
- Butonul separat ANAF din footer a fost eliminat. Butonul `⋮` rămâne vizibil și în consultare; opțiunea este dezactivată când serverul nu acordă `can_update`, cu explicația că dreptul este al Gestionarului. În VC06-22, butonul ANAF era ascuns în acest caz. Nu se extind drepturile altor roluri.

Salvarea exclusivă a CodFiscal după răspunsul ANAF „negăsit” rămâne în bara de acțiuni. Sortarea, filtrele și regulile serverului rămân cele din VC06-22. Meniul rândului se închide când pagina este distrusă.

Fișiere de aplicație: `PYTHON/static/js/vercon/partners.js`, `PYTHON/static/css/vercon-partners.css`.

Nu s-au rulat teste, build, verificări automate/de sintaxă, browser, HTTP sau SQL. Captura utilizatorului a fost folosită ca referință pentru proiectare; nu a fost copiată în proiect. Nu există probă vizuală a modificării locale. Publicarea acestei tranșe cere cele două fișiere statice; nu cere schimbări Python, restart de serviciu sau SQL, presupunând VC06-22 deja publicat. Helpul și capturile nu au fost modificate.
