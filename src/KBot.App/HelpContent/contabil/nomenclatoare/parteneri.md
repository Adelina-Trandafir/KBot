---
id: contabil.nomenclatoare.parteneri
title: Parteneri
part: contabil
order: 20
parent: contabil.nomenclatoare
screens: ParteneriForm
keywords: parteneri, furnizori, cod fiscal, cui, anaf, iban, banca, adresa
---
<!-- slice: 0087, 0093 -->
<!-- capture: parteneri | caption: Fereastra «Parteneri» | goto: menu:parteneri | prepare: Alegeți un partener cu cod fiscal, IBAN și adresă completate. -->

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
<!-- slice: 0087, 0093, 0000-14 -->

- **Capul listei**: **lupa** caută după o parte din cod sau din denumire; bifa **«Ascunde partenerii
  fără activitate»** lasă doar partenerii care apar pe un document (DDF sau ORD).
- **«Coduri angajament»** — tabelul de sub datele partenerului (clasificație, cont bancar asociat,
  cod angajament, cod indicator). **«+»** din subsolul tabelului adaugă un rând, pe care îl
  completezi direct în tabel; **✕** de pe un rând îl scoate.
- **«Adăugare»**, **«Salvare»**, **«Ștergere»**, **«Renunță»**, **«Ieșire»** — butoanele de jos ale
  ferestrei.
