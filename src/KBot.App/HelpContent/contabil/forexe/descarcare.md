---
id: contabil.forexe.descarcare
title: Descărcarea unui angajament
part: contabil
order: 30
parent: contabil.forexe
screens: SelectieReceptiiForm, AlegereUnitateForm
keywords: descarcare, reimprospatare, prelucrare, asociere, receptii, instantanee, cos, alegerea unitatii
---
Fiecare angajament din listă are în dreapta o iconiță de **reîmprospătare**. Apăsată, robotul
deschide angajamentul în FOREXE și citește tot: antetul, indicatorii, rezervările, recepțiile
cu detaliul lor și istoricul.

<!-- capture: descarcare-iconita-nod | caption: Iconița de reîmprospătare de pe un angajament | prepare: Treceți cu mouse-ul peste un angajament din listă, ca iconița lui din dreapta să se vadă. -->

## Asocierile — ce trebuie să hotărăști

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

Uneori o clasificație (cu sursa ei) aparține **mai multor unități**. Atunci salvarea se oprește
înainte să scrie ceva și K-BOT deschide **«Alegerea unității»**, cu angajamentul, indicatorul și
clasificația în cauză și lista unităților posibile.

<!-- capture: alegere-unitate | caption: Fereastra «Alegerea unității» | prepare: Apare singură, doar când o clasificație aparține mai multor unități. Fotografiați-o când o întâlniți. -->

- Alege rândul unității potrivite și apasă **«Alege unitatea»**: salvarea se reia cu ea.
- **«Nu mă mai întreba pentru această combinație»** ține minte răspunsul pentru aceeași sursă și
  clasificație.
- **«Renunță»** oprește salvarea. Nu s-a scris nimic.

## Recepții citite incomplet

Uneori FOREXE trimite pagina unei recepții mai încet decât o citește robotul și detaliul vine
tăiat. K-BOT recunoaște asta, nu atinge recepțiile deja existente cu date tăiate și te întreabă
la sfârșit dacă le citește din nou, doar pe ele. Dacă se întâmplă des, mărește timpii de
așteptare din **Setări › FOREXE**.

## Reîmprospătări parțiale

În vederile **Rezervări** și **Recepții**, iconița din dreapta, jos, a arborelui reîmprospătează
doar rezervările, respectiv doar recepțiile. La recepții te întreabă întâi **care** recepții să
fie citite din nou.

<!-- capture: selectie-receptii | caption: Alegerea recepțiilor de reîmprospătat | goto: view:receptii | prepare: Apăsați iconița din dreapta, jos, a arborelui de recepții, ca să se deschidă lista de alegere. -->
