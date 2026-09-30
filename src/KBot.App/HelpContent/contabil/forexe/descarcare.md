---
id: contabil.forexe.descarcare
title: Descărcarea unui angajament
part: contabil
order: 30
parent: contabil.forexe
screens: SelectieReceptiiForm, AlegereUnitateForm
keywords: descarcare, reimprospatare, prelucrare, asociere, receptii, instantanee, cos, alegerea unitatii, ce receptii reimprospatez, receptii bifate, reverificare receptii, actualizare angajament
---
<!-- slice: 0034, 0055 -->
Fiecare angajament din listă are în dreapta o iconiță de **reîmprospătare**. Apăsată, robotul
deschide angajamentul în FOREXE și citește tot: antetul, indicatorii, rezervările, recepțiile
cu detaliul lor și istoricul.

<!-- capture: descarcare-iconita-nod | caption: Iconița de reîmprospătare de pe un angajament | prepare: Treceți cu mouse-ul peste un angajament din listă, ca iconița lui din dreapta să se vadă. -->

## Înainte de descărcare: ce recepții se citesc din nou
<!-- slice: 0060, 0072, 0000-14 -->

Partea lungă a unei descărcări sunt recepțiile: robotul deschide în FOREXE pagina **fiecăreia**. De
obicei te interesează doar una sau două — cele la care s-a schimbat ceva pe site. De aceea, înainte
să pornească robotul, K-BOT deschide fereastra **«Ce recepții reîmprospătez?»**, cu recepțiile pe
care le are deja pentru acest angajament:

<!-- capture: selectie-receptii | caption: Fereastra «Ce recepții reîmprospătez?» | goto: view:receptii | prepare: Apăsați iconița din dreapta, jos, a arborelui de recepții (sau iconița de reîmprospătare a unui angajament care are deja recepții), ca să se deschidă fereastra. -->

| Coloana | Ce arată |
|---------|----------|
| **S** | bifa: recepția se citește din nou. Un clic pe capul coloanei bifează sau debifează tot. |
| **Nr.**, **Data**, **Valoare** | recepția, așa cum e acum în K-BOT |
| **Instantanee** | câte salvări din istoric are deja lanțul ei |
| **Stare** | «reconstituită» (refăcută din istoric), «încărcată» (trimisă în FOREXE din K-BOT sau din vechiul program), «preluată» (venită dintr-o descărcare din FOREXE) |

- **Bifată** — robotul îi deschide pagina și o rescrie cu ce e acum în FOREXE.
- **Nebifată** — nu se atinge: rămâne în K-BOT exact cum e.
- Recepțiile care există în FOREXE, dar **nu și în K-BOT**, se descarcă **întotdeauna**: nu au cum
  să fie în listă.
- Rândul de jos spune ce se va întâmpla, de exemplu «2 din 14 recepții se reîmprospătează».
- **Două recepții din aceeași zi** nu se pot deosebi decât după dată: dacă bifezi doar una, ziua se
  descarcă întreagă, iar rândul de jos te avertizează cu ⚠.

**«Descarcă»** pornește robotul; **«Renunță»** închide fereastra și **nu descarcă nimic**. Dacă
angajamentul nu are încă nicio recepție în K-BOT, fereastra nu se deschide și se descarcă tot.

Fereastra se deschide implicit cu **toate** recepțiile bifate; în **Setări › Aplicație**,
«Selectorul de recepții se deschide cu toate recepțiile bifate» o poate face să se deschidă cu
nimic bifat.

> Dacă nu știi ce s-a schimbat pe site, lasă totul bifat: durează mai mult, dar nu scapă nimic.

## A doua apăsare, cât K-BOT e deschis
<!-- slice: 0058 -->

Dacă apeși din nou pe un angajament descărcat deja cât K-BOT a rămas deschis, K-BOT întreabă întâi
dacă folosește datele din memorie: **«Da»** — imediat, fără robot; **«Nu»** — descarcă din nou
(durează, dar aduce orice s-a schimbat între timp).

## Asocierile — ce trebuie să hotărăști
<!-- slice: 0048-04, 0055, 0056, 0058, 0000-12 -->

După citire, K-BOT propune unde se așază față de recepții fiecare modificare din istoricul
FOREXE (fiecare «instantaneu»). Dacă totul se potrivește, salvarea se face direct, fără nicio
întrebare. Dacă rămâne ceva nehotărât, se deschide fereastra **Asocieri**:

<!-- capture: asocieri-propunere | caption: Fereastra «Asocieri» după o descărcare | prepare: Descărcați un angajament care are recepții noi, până se deschide fereastra de asocieri. -->

Aici spui, pentru fiecare instantaneu adus de descărcare, a cărei recepții este. E pasul cel mai
important al descărcării, explicat pe larg în
**[Asocierile recepțiilor — de ce există](topic:contabil.asocieri)** și
**[Cum așezi instantaneele](topic:contabil.asocieri.pasi)**.

- Butonul de salvare se aprinde abia când **nu mai e niciun instantaneu nehotărât**.
- După salvare fereastra se închide, iar datele angajamentului selectat se reîncarcă.

> Dacă închizi fereastra fără să salvezi, **se pierde toată descărcarea** și trebuie reluată.

Asocierile se pot corecta și mai târziu, oricând, din vederea [Recepții](topic:contabil.vederi.receptii).

## Alegerea unității
<!-- slice: 0048-02 -->

Uneori o clasificație (cu sursa ei) aparține **mai multor unități**. Atunci salvarea se oprește
înainte să scrie ceva și K-BOT deschide **«Alegerea unității»**, cu angajamentul, indicatorul și
clasificația în cauză și lista unităților posibile.

<!-- capture: alegere-unitate | caption: Fereastra «Alegerea unității» | prepare: Apare singură, doar când o clasificație aparține mai multor unități. Fotografiați-o când o întâlniți. -->

- Alege rândul unității potrivite și apasă **«Alege unitatea»**: salvarea se reia cu ea.
- **«Nu mă mai întreba pentru această combinație»** ține minte răspunsul pentru aceeași sursă și
  clasificație.
- **«Renunță»** oprește salvarea. Nu s-a scris nimic.

## Recepții citite incomplet
<!-- slice: 0091 -->

Uneori FOREXE trimite pagina unei recepții mai încet decât o citește robotul și detaliul vine
tăiat. K-BOT recunoaște asta, nu atinge recepțiile deja existente cu date tăiate și te întreabă
la sfârșit dacă le citește din nou, doar pe ele. Dacă se întâmplă des, mărește timpii de
așteptare din **Setări › FOREXE**.

## Reîmprospătări parțiale
<!-- slice: 0060 -->

În vederile **Rezervări** și **Recepții**, iconița din dreapta, jos, a arborelui reîmprospătează
doar rezervările, respectiv doar recepțiile. La recepții se deschide întâi aceeași fereastră,
«Ce recepții reîmprospătez?» (vezi mai sus).
