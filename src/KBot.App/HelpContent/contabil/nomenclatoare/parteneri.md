---
id: contabil.nomenclatoare.parteneri
title: Parteneri
part: contabil
order: 20
parent: contabil.nomenclatoare
screens: ParteneriForm
keywords: parteneri, furnizori, beneficiari, cod fiscal, cui, anaf, iban, banca, adresa, sincronizare, sincronizeaza anaf, selecteaza parteneri
open: menu:parteneri
---
<!-- slice: 0087, 0093 -->
<!-- capture: parteneri | caption: Fereastra «Parteneri» | goto: menu:parteneri | prepare: Alegeți un partener cu cod fiscal, IBAN și adresă completate. | redo: 2026-10-13 17:30 | why: 0113: «Parteneri» e o pagină a ferestrei principale, fără bara ei de titlu și fără «Ieșire» -->

Lista din stânga are partenerii unității care au **cod fiscal**, câte unul pe cod fiscal.

- **Cod fiscal**: la Enter sau la ieșirea din câmp, K-BOT întreabă **ANAF** și completează singur
  denumirea și adresa.
- **Banca** se completează singură din **IBAN** (cât timp câmpul e gol).
- **Adresa** e câmpul liber pentru adresă.

La salvare:

- codul fiscal e **obligatoriu** și nu poate fi al unității;
- nu pot exista doi parteneri cu același cod fiscal. Un partener vechi cu cod dublat rămâne
  editabil cât timp nu îi schimbi codul.

Mesajele ANAF: cod invalid, cod negăsit sau ANAF indisponibil.

## Lista și tabelul «Coduri angajament»
<!-- slice: 0087, 0093, 0000-14, 0113, 0121-03, AV01-08, AV01-10, 0093-03, 0000-148 -->

- **Capul listei**: **lupa** caută (de la 3 caractere) după o parte din cod, din denumire sau din **codul fiscal** (cu sau fără «RO»). Lista arată partenerii care apar pe un document (DDF sau
  ORD); **iconița din dreapta capului** deschide un meniu cu trei alegeri, care adaugă în listă și alți parteneri: **«Arată partenerii
  fără activitate»**, **«Arată partenerii cu sold inițial»** (cei care au un sold inițial în anul de lucru) și **«Arată partenerii cu
  angajamente»** (cei care au coduri de angajament). Partenerul deschis rămâne mereu în listă. Cu **săgețile** de la tastatură treci de
  la un partener la altul, ca la clic.
- **«Coduri angajament»** — tabelul de sub datele partenerului: angajamentele furnizorului, legate de
  **codul lui fiscal** (le vezi la fel oricând furnizorul apare cu același cod fiscal). Un rând are
  clasificația (doar de citit, vine din FOREXE), cont bancar asociat, cod angajament și cod
  indicator. **«+»** din subsolul tabelului adaugă un rând, pe care îl completezi direct în tabel;
  **✕** de pe un rând îl scoate. Codul angajamentului e obligatoriu; un rând fără cod indicator ține
  doar angajamentul. Același cod de angajament cu același cod de indicator nu poate apărea de două
  ori. Cât timp codul fiscal nu e tastat (sau l-ai schimbat și nu ai salvat încă), tabelul e
  dezactivat, iar titlul lui spune ce lipsește. Ștergerea unui partener lasă codurile de
  angajament pe loc.
- **«Solduri inițiale»** — tabelul de sub «Coduri angajament»: soldurile partenerului pe conturi, pentru anul de lucru
  (titlul spune anul). Un rând are contul, soldul inițial debitor și creditor și rulajele cumulate debitor și creditor.
  **«+»** din subsol adaugă un rând: alegi contul din listă (planul de conturi al tuturor subunităților, cu
  denumirea lui; când sunt mai multe subunități apare și capitolul), apoi tastezi sumele direct în tabel; sumele
  sunt numere cu două zecimale, cel puțin zero. **✕** scoate rândul. Un cont poate apărea o singură dată la un
  partener. Tabelul se salvează odată cu partenerul, la **«Salvare»**.
- **«Salvare»** scrie denumirea și adresa (iar IBAN-ul și banca, dacă sunt completate) și la același furnizor din **alte unități**, adică la
  partenerii cu același cod fiscal, ca să nu rămână în alte locuri din K-BOT datele vechi. Codul partenerului, ascunderea și restul rămân ale fiecărei unități.
- **«Adăugare»**, **«Salvare»**, **«Ștergere»**, **«Renunță»** (anulează modificările nesalvate ale
  partenerului) — butoanele de jos. Macheta se închide cu **«Închide»** din ribbon sau cu Esc.

## Sincronizarea partenerilor cu ANAF
<!-- slice: 0093-02, 0000-147 -->
<!-- capture: parteneri-anaf | caption: Parteneri: alegerea partenerilor pentru sincronizarea cu ANAF | goto: menu:parteneri | prepare: Apăsați «Selectează Parteneri Sincronizare Anaf» din ribbon (grupul din dreapta) și bifați câțiva parteneri; lăsați deschis meniul de la iconița din capul listei. -->

Cât timp fereastra «Parteneri» e pe ecran, ribbonul (grupul din dreapta) are butonul **«Selectează Parteneri Sincronizare Anaf»**.
Cu el poți aduce de la **ANAF** denumirea și adresa mai multor parteneri deodată, fără să-i deschizi pe rând:

1. Apeși **«Selectează Parteneri Sincronizare Anaf»**. Lista îi arată pe **toți** partenerii — și pe cei ascunși, și pe cei fără activitate —
   fiecare cu o **căsuță de bifat**. Butonul dispare, locul lui îl ia **«Sincronizează ANAF»**. Capul listei spune câți sunt bifați.
   Dacă fereastra are modificări nesalvate, te întreabă întâi dacă le salvezi.
2. Bifezi partenerii doriți, unul câte unul, sau folosești **iconița din dreapta capului listei**: în acest timp ea deschide un meniu cu trei alegeri
   rapide, care **înlocuiesc** bifele de până atunci: **«Selectează DOAR partenerii folosiți»** (cei care apar pe un document, DDF sau ORD),
   **«Selectează TOȚI partenerii»** și **«Selectează TOȚI partenerii NESINCRONIZAȚI»** (cei pe care nu i-ai sincronizat încă niciodată).
   Ținând mouse-ul pe un partener, vezi când a fost sincronizat ultima dată.
3. Apeși **«Sincronizează ANAF»**. Dacă nu e bifat niciun partener, nu se întâmplă nimic. Altfel K-BOT cere confirmarea, apoi citește de la ANAF
   partenerii bifați, **unul după altul** (cam o secundă pe partener; linia de stare arată câți au rămas), și **scrie denumirea și adresa
   primite de la ANAF în locul celor de acum**. Restul datelor partenerului (cod, IBAN, bancă, coduri de angajament) rămân neschimbate.
4. La sfârșit lista se citește din nou și revine la felul ei obișnuit (meniul capului are iar cele trei alegeri de mai înainte), iar un mesaj spune
   câți parteneri au fost sincronizați, câți aveau date schimbate și pentru care codul fiscal nu a fost găsit la ANAF — aceștia rămân **nesincronizați**.

**Care parteneri se pot sincroniza:** doar cei al căror cod fiscal este un **număr de 6 până la 12 cifre** (un «RO» la început nu contează). Câmpul «Cod fiscal»
mai conține uneori CNP-uri sau alte texte: partenerii cu astfel de coduri au căsuța **gri**, nu se pot bifa și nu sunt luați de nicio alegere rapidă.

Dacă serverul ANAF nu răspunde, sincronizarea **se oprește**: cei deja sincronizați rămân sincronizați, ceilalți **rămân bifați**, iar apăsând din nou
**«Sincronizează ANAF»** se continuă cu ei.
