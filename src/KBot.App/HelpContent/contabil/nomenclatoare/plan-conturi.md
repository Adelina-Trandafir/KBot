---
id: contabil.nomenclatoare.plan-conturi
title: Plan de conturi
part: contabil
order: 30
parent: contabil.nomenclatoare
screens: PlanContForm, PlanContConturiView, PlanContSolduriView, SubunitateEditForm
keywords: plan de conturi, modificare, export excel, sector, sursa, conturi, cont, subunitate, subunitati, capitol, analitic, simbol, clasificatie functionala, clasificatie economica, zecimale salarii, zec2, solduri initiale, rulaje, sold initial, rulaj cumulat
---
<!-- slice: AV01-04 -->
Din ribbon, fila **Configurări › Date › Plan de conturi** se deschide planul de conturi al unei **subunități**.
O subunitate este o altă unitate subordonată, cu contabilitatea ei: de ea țin **capitolul** (de exemplu 65.01),
**sectorul și sursa** (de exemplu 01A) și numărul de zecimale al salariilor.

Fereastra se deschide la fel ca [Parteneri](topic:contabil.nomenclatoare.parteneri): ca pagină a ferestrei principale.
Are două vederi, una câte una: **conturile** (arbore și câmpurile contului) și **soldurile inițiale** (un tabel). Trecerea de la una la
alta se face cu un buton din ribbon, în grupul din dreapta (vezi mai jos).

<!-- slice: AV01-07 -->
Mesajele ferestrei (o atenționare sau o eroare) apar într-o **casetă colorată** deasupra conținutului,
ca în ordonanțarea de plată: galben pentru o atenționare, roșu pentru o eroare. Nu arată indicații de lucru. Când nu e nimic de spus,
caseta nu se vede și nu ocupă loc.

## Subunitatea
<!-- slice: AV01-04, AV01-06 -->

- Planul care se vede este al subunității **sursei și sectorului alese în bara de titlu** a ferestrei principale (de exemplu 02A).
  Când schimbi sursa acolo, planul de conturi se încarcă din nou pentru ea. Dacă nicio subunitate nu are sursa aleasă, planul rămâne gol.
- Dacă **mai multe subunități au aceeași sursă**, în bara de titlu apare, lângă sursă, o listă **«Subunitate»** cu numele lor; când
  alegi alta, planul se schimbă. Pe **«Toate»** lista cuprinde toate subunitățile unității.
- Sectorul și sursa subunității sunt cele **propuse unui cont nou** când bara de titlu este pe «Toate»; fiecare cont își are propriul
  sector și propria sursă.
- Conturile scoase din uz (inactive) nu apar în arbore și în tabelul de solduri. Le arăți din **butonul din dreapta capului arborelui**
  (rotița), cu rândul **«Arată și conturile inactive»**; alegerea se aplică la ambele vederi.

## Conturile
<!-- slice: AV01-04, 0123-06, AV01-06 -->

Arborele din stânga are conturile subunității, pe **clase**, apoi pe **cont de bază**, apoi (când contul le are) pe
**clasificație funcțională** și **economică**; frunza este contul complet. Un cont fără clasificație economică (sau fără
niciuna) apare direct sub nivelul cel mai de jos pe care îl are. Cu **lupa** din capul arborelui cauți după
o parte din simbol sau din denumire. Când denumirea din dreapta nu încape pe rând, ea nu dispare: se taie și se încheie cu «...».

La dreapta, câmpurile contului ales. **Ele nu se pot schimba până nu apeși «Modificare»** (sau «Adăugare», pentru un cont nou):
așa nu se mai schimbă un cont din greșeală.

- **Cont de bază** — din nomenclatorul comun (7 cifre); tastezi o parte din cifre sau din denumire și lista se restrânge.
  El spune dacă contul poate avea clasificație funcțională și / sau economică.
- **Sector** și **Sursă** — urmează alegerea din **bara de titlu** a ferestrei principale: un cont nou le ia de acolo și câmpurile
  doar le arată. Când bara de titlu este pe **«Toate»**, le alegi chiar aici, pe cont (la un cont nou apar cele ale subunității).
- **Clsf. funcțională** / **Clsf. economică** — bifa se poate pune doar când contul de bază admite clasificația; ea este
  **opțională**. Când o bifezi, devine activ câmpul de lângă ea și îl completezi tu (nu se propune nimic). Când o debifezi, câmpul
  se golește și se oprește. Clasificația economică nu merge fără cea funcțională. Când contul nu admite clasificația, bifa este oprită.
- **Analitic** — cifre, litere mari și «_», cel mult 10 caractere; gol = cont sintetic. Pentru conturile de furnizori (401),
  cheltuieli și venituri K-BOT întreabă o dată dacă ești sigur.
- **Fel cont** (A activ, B bifuncțional, P pasiv) și **Denumire cont** — la un cont nou se propun din nomenclator.
- **Sold inițial debitor / creditor** și **Rulaj cumul. debitor / creditor** — ale contului, pentru anul de lucru.
- Simbolul contului (de exemplu 117000001A) se compune singur: cont + sector și sursă + clasificații + analitic. Jos, lângă butoane,
  vezi doar previzualizarea; la **«Salvare»** îl scrie serverul.

Butoane: **«Modificare»** deblochează câmpurile contului ales, **«Adăugare»** golește câmpurile pentru un cont nou,
**«Salvare»** scrie contul și soldurile lui (apoi câmpurile se blochează la loc), **«Renunță»** renunță la modificări,
**«Ștergere»** șterge contul ales. **«Exportă în Excel»** (în ribbon, în grupul din dreapta) salvează planul subunității, cu soldurile inițiale și rulajele, în
ordinea **debit, apoi credit**, cu sumele în format cu două zecimale.

Un cont care are **operații**, apare într-un **șablon** sau are **sold inițial** nu se șterge niciodată: K-BOT spune unde este
folosit. Simbolul unui cont folosit nu se mai poate schimba.

## Soldurile inițiale
<!-- slice: AV01-04, AV01-06 -->

Butonul **«Afișează solduri inițiale»** (în ribbon, în grupul din dreapta, cât timp fereastra este deschisă) schimbă vederea: arborele **și câmpurile contului dispar**, iar în loc apare un
tabel cu câte un rând pentru fiecare cont al subunității: **Simbol Cont, S.I. Debitor, S.I. Creditor, R.P. Debitor, R.P. Creditor**
(rulajele precedente; mereu debit înaintea creditului). Nu există rând gol de adăugat: conturile vin din plan.

- Tastezi direct în tabel; sumele sunt numere cu două zecimale, cel puțin zero.
- **Un rând modificat se salvează singur** când treci pe alt rând (și când închizi vederea sau fereastra). Dacă serverul refuză, ești
  adus înapoi pe rând, cu mesajul.
- **«Ascunde soldurile inițiale»** aduce înapoi arborele și câmpurile.
- Soldurile sunt ale **anului de lucru** al ferestrei (se vede în titlu).
