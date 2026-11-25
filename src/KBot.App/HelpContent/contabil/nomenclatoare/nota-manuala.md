---
id: contabil.nomenclatoare.nota-manuala
title: Notă manuală și Modifică operație
part: contabil
order: 48
parent: contabil.nomenclatoare
screens: NotaManualaForm
keywords: notă manuală, nota contabila manuala, modifică operație, modificare operatie, editare operatie, Note, protecție operații, cont nepotrivit, clasificație, lună nouă, document dublat, reîncarcă
---
<!-- slice: AV02-07, AV08-05, 0000-182 -->
Fereastra în care scrii de mână o operație contabilă, cu liniile ei de **Debit** și **Credit**. Se deschide în două feluri:

- din ribbon, **Introducere date › Introducere Date › «Notă manuală»** – o operație nouă;
- din [Modificare operații](topic:contabil.nomenclatoare.modificare-operatii), cu **«Modifică operație»** – operația aleasă în listă.

Sus alegi unde intră operația (o notă nouă, un document nou într-o notă existentă sau o operație nouă într-un document existent),
apoi documentul, datele, jurnalul, clasificația, compartimentul și explicația. Jos completezi liniile: contul, partenerul și suma.
Totalul de la Debit trebuie să fie egal cu totalul de la Credit, iar cel puțin una dintre părți are o singură linie.

## Datele și perioada închisă
<!-- slice: AV08-02, AV08-05, 0000-182 -->

- **«Dată operație»** ia singură data documentului când o schimbi pe aceasta; o poți apoi alege altfel.
- Datele trebuie să fie în anul de lucru și cel mult cu un an în urmă.
- O dată din luna închisă se refuză, **în afară de jurnalul JI**.
- O operație dintr-o lună închisă nu se mai poate salva. Documentul de închidere (CCV) se poate modifica doar dacă este
  ultimul, la data ultimei închideri.
- O operație dintr-un an anterior se poate modifica și după ianuarie; fereastra îți amintește jos că anul este închis.

## Avertismentele de la salvare
<!-- slice: AV08-05, 0000-182 -->

La **«Salvează»** aplicația te poate întreba, cu Da / Nu:

- dacă o dată este **într-o lună nouă** (după luna celei mai noi operații a anului);
- dacă **numărul documentului există deja** în aceeași notă contabilă.

**Da** salvează mai departe; **Nu** oprește salvarea și lasă datele în fereastră.

## Casetele marcate cu roșu
<!-- slice: AV08-05, 0000-182 -->

Când lipsește ceva sau aplicația refuză salvarea, caseta de corectat (sau linia din tabel) se colorează în culoarea de eroare,
iar jos scrie ce trebuie făcut, de exemplu «Alegeți jurnalul din listă.» sau «Alegeți contul pe linia 2 de la Debit.».
Marcajul dispare când schimbi caseta.

Dacă operația a fost schimbată între timp de altcineva, datele tale rămân în fereastră, **«Salvează»** se oprește și apare
butonul **«Reîncarcă»**, care aduce operația așa cum este salvată acum.

După salvare, dacă pagina **Modificare operații** este deschisă, lista se citește din nou și rămâne pe operația salvată.

## Protecția operațiilor
<!-- slice: AV08-01, 0000-182 -->

Cu bifa **«Protecție operații»** pusă ([Setări › Aplicație › AVACONT](topic:contabil.setari)), lista de conturi de pe o linie
arată doar conturile potrivite clasificației alese, iar salvarea refuză un cont nepotrivit. Fără clasificație vezi toate conturile.
Protecția nu se aplică operațiilor de închidere.

Când schimbi clasificația, liniile cu un cont legat de clasificație trec singure pe același cont al clasificației noi (asta se
întâmplă și fără bifă). Dacă un astfel de cont nu există, linia se marchează și jos îți spune care linie trebuie corectată.

## Ce se poate modifica
<!-- slice: AV02-07, AV08-06, 0000-182 -->

- O notă manuală sau o operație diversă se modifică în întregime; bifa **«Modifică documentul»** deschide și câmpurile
  documentului (număr, fel, dată, notă), pentru toate operațiile lui.
- O **evidențiere neplătită** își poate corecta liniile, suma și, cu **«Modifică documentul»**, numărul, felul și data facturii.
- La o evidențiere plătită sau la o plată se pot schimba doar explicația și compartimentul.
