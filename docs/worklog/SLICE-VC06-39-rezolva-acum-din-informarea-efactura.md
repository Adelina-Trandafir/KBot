# SLICE-VC06-39 — «Rezolvă Acum» în informarea facturilor eFactura

Data: 03.11.2026.

## Cerere și comportament

În fereastra «Facturi eFactura de preluat», butonul «Rezolvă Acum» apare în stânga, iar «Am înțeles» rămâne în dreapta. Clicul pe butonul nou închide informarea și execută aceeași comandă ca «Intrări → NIR din e-Factura…», cu protecțiile existente pentru schimbarea contextului, încărcare, ciornă și ocuparea gestiunii. Acțiunea ignoră contextul devenit vechi și raportează erorile prin mecanismul VERCON existent. Închiderea simplă a informării păstrează comportamentul anterior.

## Fișiere proprii și motivul înregistrării

Motiv: «Adăugarea butonului Rezolvă Acum în informarea eFactura, cu deschiderea comenzii existente NIR din e-Factura și documentarea slice-ului.»

- `PYTHON/static/js/vercon/efactura-notice.js`: acțiunea din informare și protecția contextului.
- `PYTHON/static/js/vercon/app.js`: conectarea la `openEfactura`.
- `PYTHON/static/js/vercon/messages.js`: transmiterea acțiunii opționale către dialogul comun.
- `PYTHON/static/js/utils/confirm-box.js`: suport opțional pentru un buton de acțiune înaintea butonului de închidere; utilizat de informarea VERCON.
- `PYTHON/static/css/confirm-box.css`: alinierea în stânga a butonului opțional.
- `docs/worklog/SLICE-VC06-39-rezolva-acum-din-informarea-efactura.md`: cerere, implementare și starea reală.
- `VERCON/VERCON_STATUS.md`: stare și legătură la worklog.
- `implementare_vercon_v2.md`: intrare în indexul istoric.

## Stare și limite

TESTAT DE ADELINA (OK), ÎNREGISTRAT PENTRU SERVER. La 03.11.2026, Adelina a confirmat explicit «merge. inregistreaza». Toate cele opt fișiere proprii enumerate mai sus sunt înregistrate pentru SLICE-VC06-39, inclusiv documentația actualizată cu rezultatul probei. Agentul a făcut numai analiză de design pe surse și înregistrarea autorizată; fără teste, verificări automate de execuție, browser, build, SQL, publicare sau Git.

## Completare ulterioară — ajutor VERCON, 03.11.2026

La cererea «si scrie si in help pentru vercon asta», `PYTHON/static/help/vercon.json`, subcapitolul `nir-efactura`, explică butonul «Rezolvă Acum» din stânga informării, deschiderea directă a ferestrei «NIR din e-Factura…» și închiderea simplă prin «Am înțeles» din dreapta.

Fișierele acestei completări: `PYTHON/static/help/vercon.json`, acest worklog, `VERCON/VERCON_STATUS.md` și `implementare_vercon_v2.md`. Motiv: «Documentarea butonului Rezolvă Acum și a deschiderii directe NIR din e-Factura în ajutorul VERCON.» Completare implementată local, fără teste sau publicare. La 03.11.2026, Adelina a cerut închiderea și pentru `vercon.json` («inchiderea ma refer»); cele patru fișiere ale completării sunt înregistrate pentru SLICE-VC06-39.
