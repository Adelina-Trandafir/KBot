---
id: contabil.fereastra
title: Fereastra principală
part: contabil
order: 20
parent: contabil
screens: AvacontMain, KbotForm.tree, KbotForm.capBar, KbotForm.navViews, KbotForm.btnInfo, InternalInfoForm, GrupeForm
keywords: grupe, grupa, alias, culoare grupa, arbore, lista angajamente, lista angajamentelor, vederi, bara de jos, cautare, unitate, schimba unitatea, alta unitate, fereastra marita, tot ecranul, maximizat, la pornire, macheta, machete, pagina, pagini, ferestre, inchide macheta
---
<!-- slice: 0006, 0086, 0097-03, 0113, 0114, 0115 -->
K-BOT lucrează într-o singură **fereastră principală**:

| Zona | Ce este |
|------|---------|
| **Sus** | bara de titlu cu **unitatea** (sau alegerea ei), **anul** și **sursa/sectorul**; sub ea **ribbonul**, cu toate comenzile — [Ribbonul](topic:contabil.ribbon) |
| **Stânga** | **panoul lateral**: machetele deschise, acțiunile recente, informații, erorile — [Panoul lateral](topic:contabil.panou) |
| **Restul** | **pagina** pe care lucrezi: «Acasă» la pornire ([Pagina «Acasă»](topic:contabil.acasa)), **lista angajamentelor** sau o altă machetă (Extrase de cont, Clasificații, un editor, Setări…) |

**Lista angajamentelor** (Angajamente › Navigare › «Lista angajamentelor» în ribbon) are, la rândul ei, patru zone:

<!-- capture: fereastra-zone | caption: Fereastra principală cu lista angajamentelor: ribbonul, panoul lateral, vederile, lista și vederea aleasă | goto: view:sumar | prepare: Deschideți «Lista angajamentelor» din ribbon și selectați un angajament cu date, ca toate zonele să fie pline. | redo: 2026-10-17 20:00 | why: 0114-06/14: ribbonul are in dreapta grupul machetei (langa «Inchide») si grupul vederii; meniul din subsolul listei are doua randuri; E-Factura sta sub Extrase in bara vederilor; 0114-05: arborii nu mai au iconita de descarcare a extraselor (cea cu cardul din lista angajamentelor a plecat; descarcarea e butonul din ribbon); 0113 / 0114: butonul MENIU a fost înlocuit de ribbon, în stânga a apărut panoul lateral, iar lista angajamentelor e o pagină a ferestrei principale -->

| Zona | Ce este |
|------|---------|
| **Stânga** | bara de **vederi** (Sumar, Istoric, Rezervări...) |
| **Mijloc** | **Lista angajamentelor** — alegi aici angajamentul pe care lucrezi |
| **Dreapta** | vederea aleasă, pentru angajamentul selectat |
| **Jos** | banda **FOREXE**: conectarea, progresul și ultimul mesaj al robotului |

## Pagini și ferestre
<!-- slice: 0113, 0114, 0113-01, 0114-20, 0000-92 -->

K-BOT are două feluri de machete.

**Pagini** — Extrase de cont, Clasificații bugetare, Parteneri, Setări, Jurnal activitate, Istoric versiuni, Ajutor,
E-Factura Vânzare și Cumpărare. Se deschid **în fereastra principală**, în locul paginii de pe ecran, cu titlul în bara de
titlu a ferestrei, și **rămân deschise** cât timp nu le închizi: dacă deschizi alta, prima se ascunde, cu tot ce ai
scris în ea, și revine când o ceri din nou (din ribbon sau din **Machete deschise** din panoul lateral).

- O pagină se închide cu **«Închide»** din dreapta ribbonului sau cu **Esc**. Dacă ai modificări nesalvate, pagina te
  întreabă, ca înainte.
- Când închizi pagina de pe ecran, revine cea folosită înainte; când nu mai e niciuna, revine «Acasă» sau lista
  angajamentelor, cum le-ai lăsat.
- Când treci la altă pagină, K-BOT **închide documentul PDF** care era deschis în pagina pe care o lași și o mută pe o
  filă fără PDF (bara de file rămâne la locul ei). Când te întorci, apeși din nou fila cu documentul și el se deschide.
- La închiderea K-BOT, fiecare pagină rămasă deschisă este adusă pe ecran și te întreabă dacă are ceva nesalvat.

**Ferestre** — editoarele DDF și de ordonanțare, legăturile recepțiilor, operațiunile necorelate, graficul
rezervărilor și fereastra «Grupe de angajamente». Se deschid ca **ferestre ale lor**, cu bara lor de titlu, **mai mici
decât spațiul de lucru**, în mijlocul ferestrei principale. Cu butonul de maximizare (unde îl au), o fereastră ocupă tot **spațiul paginilor**: sub
ribbon și lângă panoul lateral, nu tot ecranul. Cât sunt deschise, nu poți
lucra în fereastra principală; se închid cu butoanele lor («Renunță», «Închide» sau «Ieșire», după fereastră). Paginile
rămân cum erau când fereastra se închide; documentul PDF din pagina de dedesubt se închide când se deschide fereastra.

Ferestrele mici (o alegere, o confirmare, alegerea unui fișier), consola FOREXE, coada robotului, «Informații
interne» și fereastra de ajutor deschisă cu F1 sunt tot ferestre separate.

## Lista angajamentelor
<!-- slice: 0009, 0777, 0777-02, 0034, 0080-03, 0095-02, 0078-08, 0098, 0000-31, 0101, 0109, 0008-02, 000T-12, 0092-01, 0000-68, 0113, 0113-01, 0114-05, 0000-89, 0114-11, 0114-12, 0114-13, 0000-90, 0114-26, 0000-120 -->

- Clic pe un angajament îl selectează; vederea din dreapta se umple cu datele lui. Cât Adobe încă
  deschide un document (în Fundamentare, Ordonanțare sau Note corecție), lista nu primește alt
  angajament — [Cât se deschide un document](topic:contabil.liste).
- **Lupa** din capul listei deschide căutarea; **Esc** o golește și o închide. Toate butoanele comune
  ale arborilor și tabelelor: [Arborii și tabelele](topic:contabil.liste).
- **Rotița** din capul listei alege sortarea și ce coloane se văd (cod, surse, dată).
- Meniul rotiței are și rândul **«Grupe»**, **doar cât există cel puțin o grupă**: alegi o grupă ca să vezi în listă doar angajamentele ei (vezi mai jos «Grupele de angajamente»). Grupele se fac din ribbon, cu **«Editează grupe»**.
- Când lista e sortată după **data creării**, ea cuprinde toate sursele anului, așa că alegerea **Sursă/Sector** din bara de titlu se ascunde; reapare la sortarea după nume.
- Iconița **din dreapta, jos** (și textul «Actualizează» de lângă ea) deschide un meniu mic, deasupra ei, cu două rânduri:
  **«Descarcă angajamente NOI»** aduce din FOREXE lista angajamentelor — cele noi se adaugă, cele existente rămân cum sunt
  — [Lista de angajamente](topic:contabil.forexe.lista); **«Actualizează angajamente VRAC»** deschide fereastra în care bifezi
  angajamentele de adus la zi din FOREXE (cu descărcarea pe mai multe taburi pornită se descarcă deodată, altfel intră în coadă,
  unul după altul) — [Mai multe descărcări deodată](topic:contabil.forexe.descarcare-multipla).
- Cât lista e pe ecran, ribbonul are în dreapta, lângă «Închide», grupul **«Lista angajamentelor»**, cu **«Editează grupe»** și
  **«Descarcă extrase»** — [Ribbonul](topic:contabil.ribbon).
- Extrasele de cont noi se descarcă din ribbon, **Extrase › Descărcare › «Descarcă extrase»** (lista nu mai are iconiță pentru asta) — [Extrase](topic:contabil.vederi.extrase).
- Un angajament scris cu **roșu** are recepții al căror lanț **nu se închide**: ultimul instantaneu al recepției nu are valoarea ei. Ținând mouse-ul pe rând vezi, pentru fiecare recepție, data, valoarea ultimului instantaneu și valoarea recepției. Se repară în fereastra de asociere — [Lanțul nu se închide](topic:contabil.asocieri.cazuri). Un lanț care se termină cu rândul de ștergere nu se socotește.
- Același **roșu** îl are și un angajament cu **rezervări greșite**: o rezervare nu pornește de la valoarea la care s-a oprit cea de dinaintea ei. Ținând mouse-ul pe rând vezi ziua, indicatorul și valorile; dacă are ambele probleme, apar ambele mesaje. Se repară din vederea «Rezervări» — [Rezervări greșite și repararea lor](topic:contabil.vederi.rezervari).
- Un angajament scris într-un **alt roșu** (mai spre carmin decât primul) are **rezervări legate greșit**: sunt trecute ca avându-și revizia, dar revizia lor nu le cuprinde, deci nu au «+». Ținând mouse-ul pe rând vezi rezervarea (ziua, indicatorul, valoarea) și revizia de care e legată. Se repară tot din vederea «Rezervări» — [Rezervări legate greșit](topic:contabil.vederi.rezervari).
- Biletul care apare când ții mouse-ul pe un angajament e un **tabel cu litere mai mari**: sus descrierea, apoi codul, starea, datele și sursele, iar erorile de mai sus în culoarea lor roșie. Rămâne pe ecran până muți mouse-ul pe alt rând sau în afara listei.
- Angajamentele descărcate sau actualizate de la pornirea programului sunt **subliniate** în listă. Sublinierea dispare când închideți programul.
- Dacă apăsați reîmprospătarea pe mai multe angajamente cât timp se descarcă deja unul, pentru ele nu se mai deschide fereastra de alegere a recepțiilor: se descarcă toate.
- Când mai multe acțiuni FOREXE așteaptă una după alta, se deschide fereastra **Coada robotului**; ea se închide singură după ce coada s-a golit. Butonul **«Coadă N»** din banda FOREXE o deschide oricând, dar se vede doar cât coada are ceva în ea — [Coada robotului](topic:contabil.forexe.coada).

## Grupele de angajamente
<!-- slice: 0008-02, 000T-12, 0113, 0114-11, 0114-13, 0000-90 -->

O grupă strânge laolaltă angajamente care merg împreună. Un angajament poate fi în mai multe grupe. Fiecare grupă are un nume și o culoare.

Grupele se fac și se schimbă din ribbon, cu butonul **«Editează grupe»** din grupul **«Lista angajamentelor»** (în dreapta, lângă «Închide»; se vede cât lista angajamentelor e pe ecran).

**Rotița** din capul listei › **«Grupe»** deschide o listă cu grupele, în ordine alfabetică, fiecare cu culoarea ei. Rândul «Grupe» **apare doar cât există cel puțin o grupă** (sau cât e aleasă una); are o săgeată în dreapta: lista grupelor se deschide lângă el, iar meniul rotiței rămâne deschis.

<!-- capture: grupe-meniu | caption: Rotița listei › «Grupe»: grupele | prepare: Aveți cel puțin două grupe făcute, de culori diferite. Apăsați rotița listei și apoi rândul «Grupe». | redo: 2026-10-17 20:00 | why: 0114-11: «Editează grupe...» a ieșit din meniu (e butonul din ribbon); 0114-13: rândul «Grupe» apare doar cu cel puțin o grupă -->

- Alegând o grupă, lista arată **doar angajamentele ei**. Pe fiecare rând apare **aliasul** angajamentului (dacă nu are alias, denumirea), iar textul are culoarea grupei. Rândurile cu roșu, cele cu lanț care nu se închide, rămân roșii. Capul listei spune ce grupă e aleasă.
- Cât o grupă e aleasă, meniul are în plus rândul **«Toate angajamentele»**, care readuce lista întreagă. Atunci lista cuprinde angajamentele grupei din **toți anii și din toate sursele**, așa că alegerea **Sursă/Sector** din bara de titlu și rândurile de sortare din meniul rotiței dispar până revii la «Toate angajamentele».

<!-- capture: grupe-arbore-filtrat | caption: Lista filtrată pe o grupă: aliasul pe rânduri, culoarea grupei, capul «GRUPA: ...» | prepare: Alegeți o grupă cu angajamente, dintre care unele au alias. -->

### Fereastra «Grupe de angajamente»

Se deschide din butonul **«Editează grupe»** al ribbonului, ca fereastră a ei, cu bara ei de titlu, peste fereastra principală; o închizi cu **«Ieșire»** și lista rămâne cum era, cu grupele la zi. Tutorial: «Deschide fereastra «Grupe de angajamente»» (din meniul «?»). În stânga e arborele, cu primul rând **«Angajamente negrupate»**, apoi grupele. În dreapta: denumirea grupei, culoarea (butonul deschide paleta de culori) și tabelul cu angajamente.

<!-- capture: grupe-fereastra | caption: Fereastra «Grupe de angajamente» cu o grupă aleasă | prepare: Deschideți din ribbon, grupul «Lista angajamentelor» › Editează grupe. Alegeți o grupă cu mai multe angajamente. | redo: 2026-10-16 12:00 | why: 0113-01: fereastra Grupe e o fereastră cu bara ei de titlu și cu «Ieșire», nu o pagină a ferestrei principale -->

- **Angajamente negrupate**: tabelul arată angajamentele care nu sunt în nicio grupă. **Trage un rând** cu mouse-ul peste o grupă din arbore: angajamentul intră în acea grupă pe loc, fără să mai apeși «Salvează». Tutorial: «Adaugă un angajament într-o grupă existentă».

<!-- capture: grupe-negrupate | caption: «Angajamente negrupate»: un rând tras peste o grupă | prepare: Alegeți primul rând din arbore. Începeți să trageți un rând (țineți butonul apăsat) deasupra unei grupe și faceți captura cu rândul tras și grupa luminată. -->

- **O grupă existentă**: tabelul arată doar angajamentele ei, toate bifate. **Debifezi** un angajament și el dispare din listă; iese din grupă când apeși **«Salvează»**. Tutorial: «Scoate un angajament dintr-o grupă».
- **Grupă nouă**: apeși **«+ Adăugare grupă»** din subsolul arborelui. În tabel apar **toate** angajamentele, nebifate; le bifezi pe cele din grupă. «Salvează» cere o denumire, o culoare (implicit negru) și cel puțin un angajament bifat; două grupe nu pot avea aceeași denumire. Tutorial: «Adaugă o grupă nouă».

<!-- capture: grupe-grupa-noua | caption: O grupă nouă: denumire, culoare și angajamentele bifate | prepare: Apăsați «+ Adăugare grupă», scrieți o denumire, alegeți o culoare și bifați câteva angajamente. -->

- Coloana **«Indicatori»** arată câți indicatori are angajamentul; ținând mouse-ul pe ea vezi care sunt, cu clasificația lor.

<!-- capture: grupe-indicatori | caption: Eticheta cu indicatorii unui angajament | prepare: Țineți mouse-ul pe o celulă din coloana «Indicatori» până apare eticheta. -->

- Coloana **«Alias»** se poate scrie direct în tabel. Aliasul se salvează îndată ce ieși din celulă și e al angajamentului, nu al grupei: îl vezi în orice grupă în care e angajamentul. Un câmp gol șterge aliasul.
- Dacă ai modificări nesalvate și alegi alt rând din arbore sau închizi fereastra, K-BOT te întreabă dacă renunți la ele.

**Tutoriale** (în meniul «?», la «Tutoriale»): «Deschide fereastra «Grupe de angajamente»» (din fereastra principală), apoi, din fereastra grupelor: «Adaugă o grupă nouă», «Scoate un angajament dintr-o grupă» și «Adaugă un angajament într-o grupă existentă».

## Vederile
<!-- slice: 0018, 0074, 0088, 0097, 0000-30, 0000-31, 0114-14, 0000-90 -->

O vedere apare doar dacă angajamentul selectat are date de acel fel; altfel butonul ei **nu se vede** în bara vederilor (nu e gri, lipsește). Când nu ai ales niciun angajament, se văd doar «Sumar» și, cât ești conectat la FOREXE, «Browser FOREXE». Ce trebuie să fie adevărat ca să apară fiecare:

| Vederea | Apare când |
|---------|------------|
| Sumar | mereu; lipsește doar cât lucrezi în formularul gol «Angajament nou» din pagina FOREXE |
| Istoric | angajamentul are rânduri de istoric |
| Rezervări | angajamentul are rezervări |
| Recepții | angajamentul are recepții |
| Plăți | angajamentul are plăți |
| Extrase | angajamentul are operațiuni în extrasele de cont |
| E-Factura | fundamentarea angajamentului are facturi primite; butonul stă sub «Extrase», sus, printre vederile de lucru (nu mai e jos, lângă Fundamentare și Ordonanțare) — [E-Factura](topic:contabil.efactura) |
| Browser FOREXE | ești conectat la FOREXE (butonul «Conectare» din banda de jos) |
| Fundamentare | angajamentul are document de fundamentare (se face din «Rezervări», cu semnul «+») |
| Ordonanțare | angajamentul are ordonanțări (se fac din «Plăți», cu semnul «+») |
| Note corecție | s-a făcut cel puțin o notă de corecție pe angajament |

Alege în listă alt angajament sau rezolvă condiția și butonul apare singur. Turul ghidat al ferestrei (din «?») îți arată și vederile care lipsesc acum, cu o notă care spune că le vezi doar pentru tur.

## Anul și sursa/sectorul
<!-- slice: 0097-03 -->

În **bara de titlu**, după unitate, sunt două liste: **An Date** (anul de lucru) și **Sursă/Sector**
(de exemplu 02A). Un clic pe ele arată alegerile; schimbarea oricăreia reîncarcă lista de
angajamente și toate ecranele. Sursa/sectorul se ține minte pentru data viitoare. Când lista de
angajamente e sortată după dată, **Sursă/Sector** nu se vede.

## Unitatea de lucru
<!-- slice: 0097, 0113 -->

Bara de titlu arată unitatea pe care lucrezi: «K-BOT — Numele unității».

Dacă ai acces la **două sau mai multe unități**, numele unității devine o listă: un clic pe el
arată unitățile tale, cu o bifă pe cea curentă. Alegi alta și K-BOT o deschide **fără să-ți
ceară din nou parola**, ca după o conectare: lista de angajamente, anul, sursa/sectorul și vederile
se reîncarcă pentru unitatea nouă, iar ea se ține minte pentru conectarea următoare (dacă în
Setări › Autentificare e bifată «Ține minte și unitatea aleasă»).

<!-- capture: unitate-selector | caption: Alegerea unității din bara de titlu | goto: view:sumar | prepare: Conectați-vă cu un utilizator care are acces la cel puțin două unități, deschideți «Lista angajamentelor» din ribbon, apoi faceți clic pe numele unității din bara de titlu. | redo: 2026-10-17 20:00 | why: 0114-06/14: ribbonul are in dreapta grupul machetei (langa «Inchide») si grupul vederii; meniul din subsolul listei are doua randuri; E-Factura sta sub Extrase in bara vederilor; 0114-05: arborii nu mai au iconita de descarcare a extraselor (cea cu cardul din lista angajamentelor a plecat; descarcarea e butonul din ribbon); 0114: sub bara de titlu nu mai e butonul MENIU, ci ribbonul -->


Dacă ești **conectat la FOREXE**, K-BOT închide singur conexiunea înainte să treacă pe unitatea
nouă: conexiunea aparține unității pe care o părăsești, iar ce descarcă robotul se scrie în baza
ei. Pe unitatea nouă apeși din nou «Conectare»; ea folosește certificatul memorat, afară de cazul
în care ai bifat în Setări › FOREXE «Uită certificatul memorat când schimb unitatea din bara de
titlu» — atunci ți-l cere din nou (vezi [Setări și aspect](topic:contabil.setari)).

Schimbarea **nu se poate face**:

- cât timp robotul **lucrează** (o descărcare sau o trimitere în curs) — aștepți să termine;
- spre o unitate pe care ai rolul **«Director»** — ea se deschide în fereastra de semnare, după o
  conectare nouă.

> O machetă deschisă (Extrase de cont, Clasificații bugetare…) nu se schimbă singură: închide-o
> și deschide-o din nou după ce ai trecut pe altă unitate.

## Comenzile: ribbonul
<!-- slice: 0114 -->

Butonul **MENIU** nu mai există: comenzile lui (angajament nou, operațiuni necorelate, extrase, nomenclatoare,
E-Factura, jurnal, setări) sunt în **ribbon**, pe file. Unde a ajuns fiecare rând al meniului:
[Ribbonul](topic:contabil.ribbon). Operațiunile necorelate au butonul lor, **Angajamente › Verificări ›
«Operațiuni necorelate»**, care apare doar cât ai operațiuni necorelate de rezolvat.

## Cum pornește fereastra
<!-- slice: 0097-02, 0113, 0115 -->

K-BOT pornește pe pagina [«Acasă»](topic:contabil.acasa); lista angajamentelor se încarcă în spate și o deschizi din
ribbon. Fereastra pornește la mărimea ei obișnuită, în mijlocul ecranului, sau **mărită pe tot ecranul** dacă e bifat
în **Setări › Aplicație** «Fereastra principală pornește mărită (pe tot ecranul)». La prima pornire după actualizarea
care a adus ribbonul, K-BOT bifează o dată această opțiune; apoi alegerea e a ta —
[Setări și aspect](topic:contabil.setari).

K-BOT se închide din **X**-ul ferestrei (dreapta sus); «Închide» din ribbon închide doar macheta de pe ecran.

## Butonul ⓘ
<!-- slice: fara-felie -->

Butonul **ⓘ** din capul listei de angajamente deschide **«Informații interne»**: toate datele pe care
K-BOT le ține despre angajamentul selectat. Fereastra rămâne deschisă cât lucrezi și se schimbă
singură la fiecare angajament ales. E utilă mai ales când ceri ajutor: arată exact ce vede K-BOT.
