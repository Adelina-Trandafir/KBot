---
id: contabil.nomenclatoare.modificare-operatii
title: Modificare operații - lista operațiilor contabile
part: contabil
order: 47
parent: contabil.nomenclatoare
screens: ModificareOperatiiForm
keywords: modificare operații, modificare note, ștergere operație, ștergere document, note contabile, operații contabile, ModificareNC, MNC, sold partener, ordonanțare, recepție
---
<!-- slice: AV02-05, 0000-177 -->
Din ribbon, fila **Introducere date › Modificare Date › «Modificare operații»** se deschide lista tuturor operațiilor contabile
ale anului, câte un rând pentru fiecare operație. Se deschide ca pagină a ferestrei principale și arată operațiile
**subunității** și **anului** alese în bara de titlu; când schimbi sursa, subunitatea sau anul, lista se citește din nou.

Deocamdată aici poți **vedea, căuta, exporta și șterge** operații; modificarea unei operații nu se face încă din această pagină.

## Coloanele listei
<!-- slice: AV02-05, 0000-177 -->

- **Tip** – o iconiță cu litera felului operației: **E** evidențierea unei facturi, **P** o plată, **A** o operație diversă,
  **I** o operație de închidere, **M** o notă făcută de mână;
- **NC**, **Doc.**, **Data** – nota contabilă, numărul și data documentului;
- **Clasificație**, **Fel**, **Jurn.**, **Explicație** – cum au fost înregistrate;
- **Cont debit** și **Cont credit** – contul, urmat de **.P** și codul partenerului când operația are partener;
  «(+1)» după cont înseamnă că partea aceea mai are o linie;
- **Sumă** – suma operației; subsolul arată totalul rândurilor care se văd;
- **CH** – operația este cheltuială efectivă; **CF** – operația are control financiar preventiv;
- **Ord** – starea ordonanțării făcute din operație, ca iconiță: cerc gol = ordonanțarea e generată, pe jumătate plin =
  semnată de AB, plin în trei sferturi = semnată de CD, cerc plin cu bifă = semnată de ordonator. Fără iconiță = operația nu are ordonanțare;
- **R** – recepția facturilor de care ține operația: pătrat plin cu bifă = toate facturile au recepție, pătrat cu un colț
  plin = doar unele au. Fără iconiță = nicio factură cu recepție (sau operația nu ține de o factură).

## Căutare și ordonare
<!-- slice: AV02-05, 0000-177 -->

Fiecare coloană are în antet butonul de filtru: alegi valorile sau o condiție (conține, între două date, între două sume...).
Poți pune **filtre pe mai multe coloane deodată**; lista arată doar rândurile care trec de toate. La coloanele **Tip**,
**Ord** și **R** filtrul îți oferă cuvintele din spatele iconițelor («Plată», «Semnată CD», «Toate facturile cu recepție»...).

Din tastatură, cu lista selectată, o literă deschide filtrul coloanei ei: **N** nota, **D** documentul, **O** data,
**L** clasificația, **F** felul, **J** jurnalul, **E** explicația, **C** contul debit, **S** suma.

Ordonarea aleasă din antet se ține minte: data viitoare lista se deschide la fel. În grupul din dreapta al ribbonului:

- **«Ordonare implicită»** – pune lista înapoi în ordinea notei, a datei și a documentului;
- **«Curăță filtrele»** – scoate toate filtrele și ordonarea;
- **«Reîncarcă»** – citește din nou operațiile (filtrele rămân);
- **«Excel (listă)»** – salvează în Excel rândurile așa cum se văd (cu filtrele și ordonarea alese), cu un rând de total;
- **«Excel (tot anul)»** – salvează toate operațiile anului, fără filtre.

## Informațiile operației
<!-- slice: AV02-05, 0000-177 -->

Butonul **«Arată informațiile»** din ribbon deschide sub listă un panou despre operația aleasă (iar **«Ascunde informațiile»**
îl închide; alegerea se ține minte). Panoul arată:

- tipul operației, documentul și nota; la o evidențiere, dacă factura e plătită **INTEGRAL** sau **PARȚIAL**;
  la o plată, ordinul de plată (numărul și data);
- partenerul operației și starea ordonanțării și a recepției;
- dacă operația nu se poate șterge, motivul;
- în tabelul din mijloc, **facturile plătite** de o plată;
- în tabelul din dreapta, **soldul partenerului**: la data documentului și pe tot anul, adică soldul inițial al anului plus
  operațiile subunității (D = debitor, C = creditor).

## Ștergerea unei operații sau a unui document
<!-- slice: AV02-05, 0000-177 -->

**«Șterge operația»** (sau tasta **Delete**) șterge operația aleasă; **«Șterge documentul»** șterge documentul ei, cu
toate operațiile lui. Înainte să șteargă, aplicația îți arată ce se șterge și îți cere confirmarea. Dacă operația este
o plată care a achitat facturi, confirmarea le enumeră: după ștergere ele devin neplătite. O evidențiere ștearsă
lasă factura primită din nou necontată, ca s-o poți înregistra iar. Nota contabilă rămasă fără documente dispare.

O operație **nu se poate șterge** (aplicația îți spune motivul) când:

- face parte dintr-o lună închisă sau dintr-un an anterior deja închis;
- este o operație de închidere sau face parte din nota de închidere;
- are o ordonanțare generată (ștergi întâi ordonanțarea);
- este evidențierea unei facturi care are deja plăți;
- ordinul ei de plată este într-un OPME sau este confirmat printr-un extras de cont.

Un document se șterge doar dacă **fiecare** operație a lui se poate șterge.
