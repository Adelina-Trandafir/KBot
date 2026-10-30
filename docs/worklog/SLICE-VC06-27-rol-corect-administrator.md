# SLICE-VC06-27 — Rolul corect: Administrator

Data: 30.10.2026.

Stare: **IMPLEMENTAT_LOCAL, NERULAT, NEPUBLICAT**.

Utilizatorul a clarificat: rolul Gestionar nu există; denumirea corectă în această cerere este **Administrator**. Această precizare înlocuiește denumirea din discuția și worklogurile VC06-22–26; documentele anterioare rămân înregistrări istorice.

În `PYTHON/routes/vercon/partners.py`, helperul de citire a rolului din unitatea curentă este redenumit `is_administrator` și compară cu `administrator`, fără diferențe de majuscule și fără spații marginale. În `PYTHON/routes/vercon/supplier_merge.py`, atât lista de combinații disponibile, cât și refuzul apelului direct folosesc rolul real Administrator. Mesajul refuzului folosește denumirea corectă.

Sincronizarea ANAF rămâne disponibilă tuturor utilizatorilor care pot deschide lista furnizorilor, conform ultimei cerințe implementate în VC06-26. Nu este reintrodusă condiționarea opțiunii ANAF de un anumit rol. Pentru Administrator se aplică restricția cerută inițial: actualizare numai prin ANAF, respectiv numai CodFiscal după „negăsit”; combinarea nu ocolește această restricție.

Nomenclatorul de persoane „Gestionari” și etichetele sale reprezintă alt concept și nu sunt modificate. Nu se redenumesc sau creează roluri în MariaDB și nu se schimbă atribuiri de roluri.

Fără teste, verificări automate/de sintaxă, browser, HTTP sau SQL. Doar citirea surselor pentru proiectarea corecției. Publicarea cere cele două fișiere Python menționate și restartarea serviciului web, presupunând VC06-26 deja publicat.
