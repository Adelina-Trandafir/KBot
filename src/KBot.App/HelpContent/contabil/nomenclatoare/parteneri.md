---
id: contabil.nomenclatoare.parteneri
title: Parteneri
part: contabil
order: 20
parent: contabil.nomenclatoare
screens: ParteneriForm
keywords: parteneri, furnizori, cod fiscal, cui, anaf, iban, banca, adresa
---
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
