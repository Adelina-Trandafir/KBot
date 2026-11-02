# SLICE-VC06-27 — Rolul corect: Administrator

Data: 30.10.2026.

Stare: **IMPLEMENTAT_LOCAL, NERULAT, NEPUBLICAT**.

Utilizatorul a clarificat: rolul Gestionar nu există; denumirea corectă în această cerere este **Administrator**. Această precizare înlocuiește denumirea din discuția și worklogurile VC06-22–26; documentele anterioare rămân înregistrări istorice.

În `PYTHON/routes/vercon/partners.py`, helperul rolului din unitatea curentă devine `is_administrator`, comparând cu `administrator` fără majuscule/spații marginale. În `PYTHON/routes/vercon/supplier_merge.py`, lista combinațiilor disponibile și refuzul apelului direct folosesc Administrator, inclusiv în mesajul de refuz.

ANAF rămâne disponibil tuturor celor care pot deschide Furnizori, ca în VC06-26, fără condiție de rol. Administrator păstrează restricția inițială: actualizare numai prin ANAF, respectiv numai CodFiscal după „negăsit”; combinarea nu o ocolește.

Nomenclatorul de persoane „Gestionari” și etichetele sale reprezintă alt concept și nu sunt modificate. Nu se redenumesc sau creează roluri în MariaDB și nu se schimbă atribuiri de roluri.

Fără teste, verificări automate/de sintaxă, browser, HTTP sau SQL. Doar citirea surselor pentru proiectarea corecției. Publicarea cere cele două fișiere Python menționate și restartarea serviciului web, presupunând VC06-26 deja publicat.
