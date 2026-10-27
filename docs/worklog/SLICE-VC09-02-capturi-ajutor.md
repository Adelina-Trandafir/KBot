# VC09-02 — Capturi VERCON, cataloage CPV/MF și timer

Data: 2026-10-21.

Capturile sunt realizate în fila in-app existentă, pe `https://k-bot.ro/vercon`, în unitatea DEMO. Au fost parcurse gestiunile Local, Mijloace Fixe, Alimente Cantina și Obiecte de inventar. Ecranul NIR a fost deschis în modificare și închis fără salvare; cataloagele au fost consultate fără preluarea unui cod nou. Nu au fost salvate sau anulate documente și nu au fost trimise mesaje în chat. Gestiunea Alimente Cantina a necesitat preluarea ocupării de la o altă filă a aceluiași cont, în contextul DEMO autorizat.

Sunt ilustrate ecranul și arborele NIR, editarea NIR și fișele contextuale, meniul Configurări, produsele, conturile/TVA, comisiile, gestionarii, precizia gestiunii, selecția și perioada rapoartelor, consumul, produsele MF, clasificările MF și căutarea lor, CPV și căutarea sa, registrul MF, amortizarea, selectorul superior, lista de alimente, chatul și predarea OI. Timerul este capturat separat, în starea reală a sesiunii. Setul final are 24 de capturi, inclusiv avertizarea critică apărută efectiv după pragul de 5 minute. Am înțeles a fost apăsat după captură. Originalele temporare au fost șterse după salvarea variantelor anonimizate și a manifestului public.

Anonimizare: contul conectat, CNP/CF/IBAN și datele personale detectate în câmpuri/celule sunt acoperite cu măști opace. Numele membrilor comisiilor, gestionarilor/deținătorilor și conținutul chatului sunt de asemenea mascate. Măștile sunt aplicate local pe copia raster, fără regenerarea interfeței. Imaginile finale sunt în `PYTHON/static/help/images/vercon/`; originalele temporare nu se includ în proiect. Ferestrele de catalog sunt decupate pentru lizibilitate. Manifestul public nu conține valori de identificare sau căile originalelor temporare.

Helpul VERCON primește capitole distincte pentru CPV și clasificările MF: acces din Produse, selecția celulei și butonul ..., căutare după cod/denumire, arbore, plusul Folosește acest cod și revenirea fără alegere. Este explicată diferența ani în catalog / luni în DNF (exemplu: 8–12 ani = 96–144 luni), precum și diferența dintre clasificarea produsului și numărul de inventar al bunului.

Ambele helpuri primesc capitolul Timerul și expirarea sesiunii. Comportamentul documentat provine din `PYTHON/static/js/adechit/session.js`, folosit și de VERCON: praguri 15 și 5 minute, Am înțeles/Escape ascunde doar avertizarea, activitatea curentă nu reînnoiește termenul, încărcarea paginii îl reînnoiește, iar la expirare ciornele se pierd. Capturile acestei tranșe sunt VERCON; ADECHIT primește explicația comună, fără imagini prezentate drept capturi ADECHIT.

Motorul comun de help acceptă blocul `image: {src, alt, caption}`. Imaginile au descriere alternativă, legendă, încărcare la apropierea de ecran și deschidere la dimensiunea originală. Imprimarea așteaptă decodarea imaginilor; rendererul PDF le încadrează proporțional, păstrând legenda alături de imagine. Căile imaginilor sunt limitate la directorul static al capturilor de help.

Fișiere:

- PYTHON/static/help/vercon.json și adechit.json;
- PYTHON/static/help/images/vercon/*.jpg și manifest.json;
- PYTHON/static/help/images/vercon/README.md;
- PYTHON/static/js/help/help.js;
- PYTHON/static/css/adechit-help.css;
- PYTHON/reporting/help_pdf.py;
- tools/HelpCapture/Protect-WebHelpCapture.ps1;
- docs/worklog/SLICE-VC09-02-capturi-ajutor.md;
- implementare_vercon_v2.md.

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

Publicarea necesită împreună conținutul JSON, imaginile, JS/CSS și rendererul PDF Python; serviciul web trebuie repornit pentru codul Python actualizat.
