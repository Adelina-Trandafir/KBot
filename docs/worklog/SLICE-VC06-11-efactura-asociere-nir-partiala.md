# SLICE-VC06-11 — Asociere NIR manual cu eFactura — 23.10.2026

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

## Utilizare

Într-un NIR de cumpărare salvat, neanulat, opțiunea «⋮ → Asociază eFactura…»
din meniul acelui NIR în arbore deschide facturile active trimise de contabil
cu data facturii din NIR. Opțiunea a fost mutată din footer în popup.
Serverul cere și același CUI. Numărul facturii nu este criteriu de asociere,
nici la selecție, nici la salvare sau la calculul valabilității alocărilor;
seria și spațiile pot diferi. NIR-ul trebuie deschis în contextul
gestiunii și exercițiului său, cu drept de recepție, ocupare și perioadă
deschisă. Pentru alte NIR-uri se schimbă contextul în mod normal.

Două liste alfabetice românești prezintă produse NIR și poziții eFactura,
CPV, UM, cantitate și rest. Se trage un rând NIR peste poziția eFactura;
alternativ se selectează cele două rânduri și se apasă butonul de asociere.
Perechile se editează sau se elimină înainte de salvare. Cantitățile sunt
independente pe cele două documente, pentru conversii explicite de UM.
Calculele din UI folosesc BigInt la patru zecimale, serverul Decimal.

O poziție poate fi împărțită între rânduri și NIR-uri, inclusiv în subunități
diferite. Asocierea înlocuiește numai perechile NIR-ului curent pentru
trimiterea aleasă. Celelalte NIR-uri, afișate cu subunitate/gestiune/număr/data,
își păstrează alocările. Nu se modifică NIR-uri, facturi interne VC_FACTURI,
produse, prețuri sau mișcări de stoc.

## Propuneri și memorare

Se reutilizează întâi o denumire memorată univoc pentru CUI/gestiune,
apoi CPV-ul exact normalizat la opt cifre. O contradicție între două CPV-uri
nu este rezolvată automat prin memorie. La CPV-uri sau produse repetate se
compară și cantitățile disponibile. O propunere se face numai dacă perechea
este univocă în ambele sensuri și UM coincide. Nu se folosesc potriviri
aproximative de nume; ambiguitățile și UM diferite se rezolvă manual.

Toate propunerile sunt ciorne până la salvare. Bifa de memorare a fost
eliminată ulterior pentru compactarea dialogului. Memoria se completează
implicit numai pentru perechile realizate prin drag and drop sau editate
manual, la salvarea lor. Propunerile și asocierile din butoane nu produc
singure memorie. Corespondența păstrează: CUI,
denumirea normalizată eFactura, produsul identificat în gestiune, ambele
denumiri, utilizatorul și data. Identitatea produsului rămâne stabilă la
redenumire. Mai multe produse memorate pentru același nume nu sunt alese
arbitrar. Eliminarea unei alocări nu șterge istoricul denumirilor memorate.

## Tranzacții și compatibilitate

Scrierea folosește ocuparea contextului, blocarea gestiunii, revizia NIR și
blocarea facturii/trimiterii, comună cu retragerea contabilului. Recalculează
bugetele tuturor NIR-urilor în aceeași tranzacție și refuză depășirile pe
ambele documente. Cheia de cerere permite reluarea aceleiași salvări după un
răspuns pierdut. Auditul păstrează perechile, cantitățile și memorarea.

Starea efectivă este TRIMISA / PARTIALA / PRELUATA, ultima numai când toate
cantitățile facturii sunt acoperite. În baza veche se stochează în continuare
TRIMISA/PRELUATA; PARTIALA se calculează identic pentru VERCON și K-BOT.
Factura parțială nu poate fi retrasă/returnată și conturile ei propuse nu
se pot schimba până la eliminarea asocierilor active.

Asocierea reține revizia NIR. Orice editare ulterioară a NIR-ului invalidează
asocierile acelei revizii, care trebuie reconfirmate în dialog; anularea sau
dispariția documentului eliberează și ele cantitățile. Revizia monotonă
împiedică reactivarea unor alocări vechi dacă se revine ulterior la aceleași
cantități. NIR-urile deja legate integral prin vechiul flux nu se convertesc
automat. Pentru o trimitere trecută pe alocări, restul se recepționează prin
NIR manual urmat de asociere, nu prin fluxul vechi de preluare integrală.

## Instalare și fișiere

Utilizatorul execută manual `sql/EV_02_vercon_asocieri_partiale.sql` pe
AVACONT_SURSA și sincronizează structura în DC-uri, după EV_01. Sunt trei
tabele locale noi, cu identificatori uppercase: EF_VERCON_ASOCIERI,
EF_VERCON_ALOCARI, EF_VERCON_CORELARI. Nu este SQL pentru AVACONT_COMUN.
Aplicația nu creează schema; opțiunea răspunde EF_ASSOC_SCHEMA_REQUIRED
dacă aceasta lipsește. Fluxul vechi continuă să funcționeze fără schema nouă.

Fișiere noi: scriptul SQL; PYTHON/utils/vercon_efactura.py;
PYTHON/routes/vercon/efactura_association.py;
PYTHON/static/js/vercon/efactura-association.js și efactura-match.js.

Integrare: PYTHON/routes/vercon/{doc_routes,efactura_queue}.py;
PYTHON/routes/efactura/primite_vercon.py;
PYTHON/static/js/vercon/{docs,efactura-intake}.js;
PYTHON/static/css/vercon-efactura.css; PYTHON/static/help/vercon.json;
src/KBot.Domain/EFacturaVercon.vb;
src/KBot.EFactura/Forms/VerconForm.vb; implementare_vercon_v2.md.

Publicare necesară: modulele Python/JS/CSS/help împreună, repornirea
serviciului web, reîncărcarea paginii și recompilarea K-BOT pentru textul
stării parțiale. Nu s-a făcut publicare sau execuție SQL în această lucrare.
