# SLICE-VC06-15 — informare la intrarea în gestiune și facturi inactive

## Comportament

- Informare la intrarea în gestiune pentru facturile trimise de contabil cu
  rest de preluat sau asociat. Include facturi parțial asociate; exclude
  preluările integrale și marcajele Inactiv. Funcționează și înainte de
  deschiderea unui ecran NIR. Nu repetă informarea la simpla reîmprospătare
  a aceluiași context; revenirea din altă gestiune recitește situația.
- Marcaj **global pentru unitate**, conform corecției explicite a utilizatorului:
  o factură ascunsă de un gestionar este ascunsă pentru toți. Nu este o
  preferință pe utilizator, subunitate, gestiune sau exercițiu.
- Active este filtrul implicit în lista de preluare și selectorul din
  asociere. Inactive și Toate permit restaurarea prin debifarea Inactiv.
  Facturile inactive preluate între timp integral rămân accesibile pentru
  restaurare. Nu se limitează răspunsul listei la primele 500 de facturi.
- Lista de preluare folosește DGV-ul comun, cu coloană Inactiv (checkbox),
  tipuri reale pentru date/sume/stare și sortare cronologică implicită.
  Numărul pozițiilor, expeditorul și nota contabilului sunt în mesajul
  selecției, cu elipsis și tooltipul custom pentru textele lungi.
- Fereastra de asociere păstrează ComboBox-ul custom și încărcarea automată.
  Bifa Inactiv și filtrul sunt pe aceeași bară; asocierile nesalvate sunt
  protejate prin confirmare înainte de schimbarea afișării.
- Marcajul salvează numai vizibilitatea; nu returnează factura, nu schimbă
  NIR-uri, stocuri, cantități, corelări de produse sau starea contabilului.

## Implementare

- `sql/EV_03_vercon_facturi_inactive.sql`: EF_VERCON_VIZIBILITATE cu cheie
  ID_TRIMITERE, INACTIV, MODIFICAT_DE și MODIFICAT_LA; identificatori noi
  uppercase, utf8mb3 / utf8mb3_general_ci. Executare manuală în AVACONT_SURSA,
  apoi sincronizarea structurii în DC-uri; nu AVACONT_COMUN.
- `PYTHON/routes/vercon/efactura_visibility.py`: citire globală și POST
  visibility, boolean strict, drept de recepție, context validat, lock de
  gestiune, tranzacție și audit. Lock-ul facturii serializează modificările
  făcute din gestiuni diferite. Setarea aceleiași valori este idempotentă.
- `efactura_queue.py`: filtre Active/Inactive/Toate și GET notice cu
  numărul tuturor facturilor active nepreluate integral, înainte de filtrele
  pe data/furnizor ale unei asocieri. Include efectul anulării unui NIR.
- `PYTHON/utils/vercon_efactura.py`: proiecție în lot a stărilor parțiale,
  pentru ca lista/informarea să nu lanseze verificări de structură și
  interogări de buget separate pentru fiecare factură.
- `doc_routes.py`: înregistrarea rutei de vizibilitate.
- `PYTHON/static/js/vercon/efactura-notice.js`, `app.js`: informare după
  acceptarea contextului, anularea cererii vechi și ignorarea răspunsurilor
  din alt context, cleanup la închiderea paginii; fără polling.
- `efactura-queue-dialog.js`, `efactura-intake.js`: DGV, bife, filtre,
  restaurare și preluarea/returnarea existente, footer cu status între acțiuni.
- `efactura-association.js`: filtre și bifă globală, păstrând selectarea
  după dată/CUI, încărcarea automată și confirmarea ciornei.
- `PYTHON/static/css/vercon-efactura.css`: dimensiuni DGV și bifă compactă.
- `PYTHON/static/help/vercon.json`: informare, scope global și restaurare.

## Stare și limite

TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026). Fără creare automată de schemă în cod. În lipsa structurii EV_03 lista existentă funcționează, iar
marcarea este dezactivată cu explicația instalării manuale necesare.
Informarea și progresul folosesc lista comună unității (trimiterea contabilului
nu stabilește o singură gestiune destinatară).

Numai analiză de design pe surse; fără teste, build, verificări automate/de
sintaxă, browser, ANAF sau MariaDB, conform instrucțiunilor proiectului.
Publicare: aplicare manuală SQL/sincronizare, Python/JS/CSS/help împreună,
repornirea serviciului și reîncărcarea paginii.
