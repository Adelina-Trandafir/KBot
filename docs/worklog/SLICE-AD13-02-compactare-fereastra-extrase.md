# ADECHIT · ajustările machetei după adnotările operatorului

13.10.2026. Ajustări aplicate peste AD13, conform imaginii și corecțiilor ulterioare.

## Machetă

- Fereastra de asociere ocupă întregul viewport, fără margini, limite de 1680/900 px
  sau colțuri rotunjite, și se suprapune modal peste pagina principală. Înălțimea
  folosește 100dvh cu fallback 100vh. Antetul are 46 px, ca aplicația principală;
  subsolul are minimum 38 px, butoane de minimum 28 px și padding vertical de 4 px.
  La rezoluții mici, zona de lucru derulează separat, păstrând antetul și acțiunile
  din subsol vizibile. Subsolul poate crește când butoanele sau mesajul necesită
  mai multe rânduri. Padding-ul interior este redus pentru mai mult spațiu util.
- Zona explicativă de sus și rezumatul permanent de jos au fost eliminate.
- Butoanele Asociere manuală / Previzualizare automată sunt în subsol, lângă acțiuni.
- Arborele Lună/An folosește controlul TreeView din pagina principală și stă în
  stânga extraselor, cu lățimea de 258 px pe desktop. Datele lui sunt lunile extraselor
  din întreaga unitate; implicit se selectează ultima lună disponibilă.
- Luna extraselor filtrează sursa; Luna înregistrării este un câmp separat pentru
  documentul ADE. Alegerea unui extras nu mai schimbă luna înregistrării.
- Căutarea este doar în FX_Extrase.Explicatii, prin substring (*text*), după minimum
  două caractere, cu actualizare după 300 ms de pauză în tastare. Ștergerea / scurtarea
  textului sub două caractere elimină filtrul. Conform precizării ulterioare,
  prefixul bancar înaintea primei cratime este exclus din căutare atunci când
  corespunde câmpului Referinta. Căutarea ignoră majusculele/minusculele:
  `gr` găsește atât `Gr40`, cât și `Grădinița 40`, fără a cere numele copilului
  sau scrierea consecventă a denumirii grădiniței. Explicația originală rămâne
  intactă pentru afișare, amprentă și proveniență. Regula se aplică și previzualizării.
- ID-ul extrasului și ReferintaDest nu se afișează în tabel. Codul fiscal și Încasarea
  rămân, la 90 / 80 px. Detalii extras umple spațiul rămas. Autosizing-ul inițial este
  dezactivat, astfel încât lățimile explicite să fie respectate.
- Grupele au un panou compact, copiii păstrează CNP-ul într-o coloană de 100 px,
  iar Plătitorii primesc spațiul rămas dintre aceste liste și formular.
- Luna înregistrării și Explicație document sunt în dreapta Plătitorilor, una sub
  alta, într-un formular de 260 px (230 px pe desktop mai îngust). Explicația are
  textarea compactă, editabilă.
- Bifa Arată ascunse este sub Grupe și este debifată la deschiderea ferestrei.
- Erorile și confirmările scurte apar în subsol; nu există text permanent care
  repetă selecția. FelDoc rămâne exclusiv în bază.

## Asociere fără prezență introdusă

Cererea identifică direct copilul și luna, împreună cu versiunile copilului,
plătitorului și lunii. Nici interfața manuală, nici propunerile automate nu cer
existența prezenței sau completarea numărului de zile.

Dacă rândul lunar lipsește, se creează în aceeași tranzacție un rând de legătură
în AD_Prezenta cu zero zile și valori contractuale zero, folosind grupa copilului
și taxa lunii. O prezență existentă nu este modificată. Într-o lună închisă eligibilă,
se creează și situația salvată pentru rândul nou, înaintea actualizării cu plata.
Astfel, plata rămâne vizibilă prin fluxurile existente de situații și rapoarte,
fără a introduce documente fără legătură financiară sau a fabrica zile de prezență.

Se păstrează regulile existente privind lunile eligibile. Mai multe extrase pentru
același copil în aceeași lună generează în continuare plăți și documente distincte,
inclusiv când rândul lunar este creat de primul element al lotului.

Previzualizarea automată folosește copilul drept cheie a opțiunii, nu ID-ul prezenței.
Căutarea în timpul tastării păstrează editările și bifele rândurilor revăzute, inclusiv
la revenirea unui rând după ștergerea filtrului. Se generează numai rândurile bifate
din rezultatul vizibil. Amprenta sursei și versiunile țintei revăzute se păstrează;
o căutare nu acceptă implicit date schimbate între timp. Reîncărcarea explicită sau
schimbarea lunii cer confirmare înainte de abandonarea editărilor.

## Fișiere și stare

Modificate: adechit.html, adechit-bank-associations.css, bank-associations.js,
bank-preview.js, bank_associations.py, bank_preview.py, routes/adechit/__init__.py
și nota AD13 inițială. Noi: bank-periods.js și bank_targets.py.

Nu sunt necesare modificări suplimentare de schemă față de SQL-ul AD_11 pregătit
anterior. Nu s-a executat SQL și nu s-au rulat teste sau alte verificări executabile,
conform AGENTS.md. Stare: modificări în surse; browserul și MariaDB nu au fost probate.
