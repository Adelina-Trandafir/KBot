---
id: contabil.nomenclatoare.modificare-operatii
title: Modificare operații - lista operațiilor contabile
part: contabil
order: 47
parent: contabil.nomenclatoare
screens: ModificareOperatiiForm
keywords: modificare operații, modificare note, ștergere operație, ștergere document, note contabile, operații contabile, ModificareNC, MNC, sold partener, ordonanțare, recepție
---
<!-- slice: AV02-05, 0000-177, DGV0-02, 0000-181, AV08-05, 0000-182 -->
Din ribbon, fila **Introducere date › Modificare Date › «Modificare operații»** se deschide lista tuturor operațiilor contabile
ale anului, câte un rând pentru fiecare operație. Se deschide ca pagină a ferestrei principale și arată operațiile
**subunității** și **anului** alese în bara de titlu; când schimbi sursa, subunitatea sau anul, lista se citește din nou.

Aici poți **vedea, căuta, exporta și șterge** operații. **«Modifică operație»** deschide operația aleasă în editor
([Notă manuală și Modifică operație](topic:contabil.nomenclatoare.nota-manuala)); după salvare lista se citește din nou și rămâne pe ea.

## Coloanele listei
<!-- slice: AV02-05, 0000-177, DGV0-02, 0000-181 -->

- **Tip** – o iconiță cu litera felului operației: **E** evidențierea unei facturi, **P** o plată, **A** o operație diversă,
  **I** o operație de închidere, **M** o notă făcută de mână;
- **NC**, **Doc.**, **Data** – nota contabilă, numărul și data documentului;
- **Clasificație**, **Fel**, **Jurn.**, **Explicație** – cum au fost înregistrate;
- **Cont debit** și **Cont credit** – contul, urmat de **.P** și codul partenerului când operația are partener;
  «(+1)» după cont înseamnă că partea aceea mai are o linie;
- **Sumă** – suma operației; subsolul arată totalul rândurilor care se văd;
- **Ch** – operația este cheltuială efectivă; **Cf** – operația are control financiar preventiv;
- **O** – starea ordonanțării făcute din operație, ca iconiță: cerc gol = ordonanțarea e generată, pe jumătate plin =
  semnată de AB, plin în trei sferturi = semnată de CD, cerc plin cu bifă = semnată de ordonator. Fără iconiță = operația nu are ordonanțare;
- **R** – recepția facturilor de care ține operația: bifat = toate facturile au recepție, căsuța cu pătrat plin înăuntru =
  doar unele au, căsuță goală = nicio factură cu recepție (sau operația nu ține de o factură).

## Căutare și ordonare
<!-- slice: AV02-05, 0000-177, DGV0-02, 0000-181 -->

Coloanele au în antet butonul de filtru (o pâlnie), în afară de **Ch**, **Cf**, **O** și **R**, care nu se filtrează. Pâlnia apare când treci cu mouse-ul peste antetul coloanei; pe coloanele care au deja un filtru pus pâlnia rămâne mereu la vedere, ca să știi dintr-o privire ce e filtrat. Cu butonul alegi valorile sau o condiție (conține, între două date, între două sume...).
Poți pune **filtre pe mai multe coloane deodată**; lista arată doar rândurile care trec de toate. La coloana **Tip**
filtrul îți oferă cuvintele din spatele iconițelor («Evidențiere», «Plată», «Diverse»...).

Din tastatură, cu lista selectată, o literă deschide filtrul coloanei ei: **N** nota, **D** documentul, **O** data,
**L** clasificația, **F** felul, **J** jurnalul, **E** explicația, **C** contul debit, **S** suma.

Lista se poate și **grupa**: în meniul butonului de filtru, fila **«Grupare»**, la coloanele **NC**, **Clasificație**, **Fel**, **Jurnal** și **Data**. Rândurile cu aceeași valoare se strâng sub un antet de grup care se poate plia și desplia. La **Data** alegi grupul **pe zi**, **pe lună** sau **pe an**. O coloană după care e grupată lista arată în antet, în locul pâlniei, un semn de grupare (un titlu de grup cu rândurile lui sub el); dacă are și filtru, rămâne pâlnia.

Ordonarea aleasă din antet se ține minte: data viitoare lista se deschide la fel. În grupul din dreapta al ribbonului:

- **«Ordonare implicită»** – pune lista înapoi în ordinea notei, a datei și a documentului;
- **«Curăță filtrele»** – scoate toate filtrele și ordonarea;
- **«Reîncarcă»** – citește din nou operațiile (filtrele rămân);
- **«Excel (listă)»** – salvează în Excel rândurile așa cum se văd (cu filtrele și ordonarea alese), cu un rând de total;
- **«Excel (tot anul)»** – salvează toate operațiile anului, fără filtre.

## Ștergerea unei operații sau a unui document
<!-- slice: AV02-05, 0000-177, AV08-04, 0000-182 -->

**«Șterge operația»** (sau tasta **Delete**) șterge operația aleasă; **«Șterge documentul»** șterge documentul ei, cu
toate operațiile lui. Înainte să șteargă, aplicația îți arată ce se șterge și îți cere confirmarea. Dacă operația este
o plată care a achitat facturi, confirmarea le enumeră: după ștergere ele devin neplătite. O evidențiere ștearsă
lasă factura primită din nou necontată, ca s-o poți înregistra iar. Nota contabilă rămasă fără documente dispare.

O operație **nu se poate șterge** (aplicația îți spune motivul) când:

- face parte dintr-o lună închisă sau dintr-un an anterior deja închis;
- este o operație de închidere sau face parte din nota de închidere;
- are o ordonanțare generată (ștergi întâi ordonanțarea);
- este evidențierea unei facturi care are deja plăți (și o plată din anul următor contează);
- ordinul ei de plată este într-un OPME sau este confirmat printr-un extras de cont.

Un document se șterge doar dacă **fiecare** operație a lui se poate șterge.

Aceleași reguli (luna sau anul închis, ordonanțarea, ordinul blocat) se aplică și butonului **«Șterge plata»** din
**Facturi și plăți** și din **Contări**.
