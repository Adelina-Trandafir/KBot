---
id: tur-setari
title: Fereastra «Setări»
part: contabil
topic: contabil.setari
---
## Paginile Setărilor
<!-- slice: 0072 -->
target: SetariForm.navViews
goto: setari:info
Câte o pagină pentru fiecare subiect. Setările se salvează pe măsură ce le schimbi.

## Pagini › Informații
<!-- slice: 0072, 0000-23 -->
target: SetariForm.navViews
part: item:info
Datele sesiunii (utilizator, unitate, an, versiunile K-BOT) și schimbarea parolei.

## Caută actualizări
<!-- slice: 0072, 0067 -->
target: SetariInfoView.btnActualizari
Butonul «Caută actualizări» verifică dacă există o versiune nouă a K-BOT.

## Aplicație
<!-- slice: 0072, 0777, 0097-02 -->
target: SetariForm.navViews
part: item:aplicatie
goto: setari:aplicatie
Comutatoarele de zi cu zi (între ele: fereastra principală mărită la pornire și turul de la pornire) și opțiunile listei de angajamente: sortarea și ce coloane se văd.

## FOREXE
<!-- slice: 0072, 0091, 0097, 0097-02 -->
target: SetariForm.navViews
part: item:forexe
goto: setari:forexe
Starea robotului, certificatul memorat (și dacă se uită la schimbarea unității), mini-meniul K-BOT din pagina FOREXE, testul de viteză și timpii de așteptare. Dacă recepțiile vin des tăiate din FOREXE, mărește aici timpii.

## Descărcări multiple
<!-- slice: 0100-02 -->
target: SetariForm.navViews
part: item:multithread
goto: setari:multithread
Apare doar dacă unitatea ta permite descărcarea pe mai multe taburi. Aici alegi câte angajamente se descarcă deodată (cel mult cât îngăduie unitatea) și dacă cele vechi se actualizează singure la conectare.

## Extrase
<!-- slice: 0080-02, 0000-23 -->
target: SetariForm.navViews
part: item:extrase
goto: setari:extrase
Ce coloane se văd în grilele de extrase și în ce ordine, separat pentru vedere și pentru fereastra «Extrase de cont». «Revino la implicit» le readuce la forma inițială.

## Autentificare
<!-- slice: 0063, 0072, 0097 -->
target: SetariForm.navViews
part: item:autentificare
goto: setari:autentificare
Ce ține minte fereastra de conectare (utilizatorul, unitatea; «Uită datele memorate» le șterge), ce face K-BOT când expiră sesiunea (îți arată fereastra de conectare sau se reconectează singur, cel mult o dată la intervalul ales) și adresa serverului K-BOT.

## Jurnal
<!-- slice: 0031-04, 0072-01, 0089 -->
target: SetariForm.navViews
part: item:jurnal
goto: setari:jurnal
Jurnalele K-BOT și mesajele pe care ți le-a arătat, pentru când ceva nu merge. Golește jurnale șterge fișierele alese, definitiv.
