---
id: contabil.nomenclatoare.clasificatii
title: Clasificații cheltuieli
part: contabil
order: 10
parent: contabil.nomenclatoare
screens: ClasificatiiForm, ClasificatiiBugetView, ClasificatiiVenituriView, ClasificatiiAddForm, ClasificatiiVenituriAddForm
keywords: clasificatii, clasificatie bugetara, venituri, clasificatii venituri, buget venituri, paragraf, bara verticala, indicatori, buget, trimestre, rectificari, capitol, subcapitol, articol, alineat, verificare buget, credit bugetar, diferenta, trimite in access
open: menu:clasificatii
---
<!-- slice: 0087, 0075-00, 0102, 0103-04, 0103-06, 0105, 0107-02, 0107, 0113, AV01-11, AV01-12, AV01-13, 0000-137, 0126-01, 0126-02, 0126-03, 0126-04, 0126-06, 0010-09 -->
<!-- capture: clasificatii | caption: Fereastra «Clasificații cheltuieli» | goto: menu:clasificatii | prepare: Alegeți în arbore un alineat care are buget și rectificări. | redo: 2026-10-26 17:10 | why: numele ferestrei («Clasificații cheltuieli») și paginile «Cheltuieli» / «Venituri» din bara verticală -->

Fereastra are în stânga o **bară verticală**, ca cea a ferestrei principale, cu două pagini: **Cheltuieli** (cea care se deschide) și **Venituri**. O alegi cu un clic sau cu săgețile sus / jos. Dacă pagina de pe ecran are modificări nesalvate, K-BOT te întreabă ce faci cu ele înainte să treci pe cealaltă. Tot ce urmează, în afară de secțiunea [Venituri](#venituri), descrie pagina **Cheltuieli**.

- **Arborele** din stânga: Capitol › Subcapitol › Titlu › Articol › Alineat, cu denumirile alături. Implicit arată doar clasificațiile cu **mișcare** în anul de lucru: cele care au vreun trimestru de buget sau de rectificare diferit de zero. Din iconița din dreapta capului arborelui (cea cu rotița) alegi **«Arată toate clasificațiile»**, ca să le vezi pe toate cele configurate; rândul e bifat cât timp filtrul e pornit. Celălalt rând, **«Arată DOAR clasificațiile folosite în FOREXE»**, lasă în arbore numai clasificațiile pentru care FOREXE a raportat un credit bugetar; are prioritate față de cealaltă bifă. Arborele urmează **sursa aleasă în bara de titlu**: cu o sursă anume (de exemplu 02A) vezi doar clasificațiile ei, cu «Toate» le vezi pe toate sursele. Când schimbi sursa, arborele se reface singur (dacă ai modificări nesalvate, K-BOT te întreabă mai întâi ce faci cu ele). În dreapta fiecărui rând din arbore apare **totalul final** al clasificației: ultimul buget plus toate rectificările, fără zecimale; la un capitol, subcapitol, titlu sau articol este suma clasificațiilor de sub el. După **Salvează**, totalurile se refac singure.
- În dreapta, pentru clasificația aleasă:
  - **bugetul** anului, pe **versiuni**: fiecare rând este bugetul de la data din coloana
    **«Început»** încolo, pe trimestre (Trim. 1–4), cu **totalul** rândului în ultima coloană. Prima coloană arată clasificația aleasă pe fiecare rând, iar în antetul ei scrie titlul tabelului (de exemplu «Buget 2026 — 65.10.04.02…»); trecând cu mausul peste antet vezi denumirea clasificației. La fel, antetul primei coloane de la rectificări și cel de la angajamente poartă titlul tabelului lor (nu mai există titluri deasupra tabelelor). Sub grilă nu
    există un total general, pentru că versiunile unui buget nu se adună între ele. O versiune nouă se adaugă scriind în **rândul gol** de la sfârșit; **«✕»** de pe rând o șterge (butonul umple celula și se colorează doar când treci cu mausul peste el);
  - **rectificările**: numărul și data documentului, plus trimestrul (sau trimestrele) pe care le
    schimbă. Ele nu sunt buget, dar îl influențează. O rectificare nouă se adaugă tot în
    **rândul gol** de la sfârșit. Rândul de jos însumează rectificările clasificației alese;
  - **rândul de sub cele două tabele** adună **ultimul buget** (versiunea cu data de început cea mai nouă) cu **toate rectificările**, pe fiecare trimestru. Se schimbă singur când modifici o sumă sau o dată în tabele. Are culoarea subsolului celorlalte tabele;
  - **tabelul «Angajamente»**, cel mai de jos, arată ce intră pe clasificație (vezi [Angajamentele clasificației](#angajamentele-clasificatiei)).
- Dacă alegi un nod **mai sus de alineat** (capitol, subcapitol sau articol), tabelele din dreapta arată doar un sumar, fără editare: **Clsf** (coloana care se întinde), **ultimul buget** al fiecărei clasificații de sub nod și, la rectificări, **totalul** pe fiecare clasificație. Nu apar «Început», «Nr. doc.», «Data», «✕» și «+». Rândul de jos adună ultimul buget al tuturor clasificațiilor de sub nod cu totalurile rectificărilor lor. În aceste sumare, ca și când nu e ales nimic, nu există rând de total jos: rectificările tuturor clasificațiilor la un loc nu înseamnă nimic.
- Subsolul arborelui arată câte clasificații sunt afișate. Dacă sursa clasificației alese are **subunități copil**, aici apare mesajul că se lucrează direct în datele părintelui.
- **Salvează** (în ribbon, grupul **Clasificații** din dreapta) scrie versiunile de buget și rectificările. Dacă treci pe alt nod sau închizi
  fereastra cu modificări nesalvate, K-BOT te întreabă ce faci cu ele.
- **Verifică bugetul** (în același grup din ribbon) compară bugetul din K-BOT cu cel din FOREXE (vezi mai jos).
- **Trimite în Access** (în același grup din ribbon) scrie în baza Access a unității bugetul și rectificările clasificației alese
  (vezi mai jos).

Cele două tabele au **aceleași coloane, de aceeași lățime**, una sub alta, ca să poți citi trimestrele în jos; la fel și rândul de jos.

Fiecare din cele două tabele se termină cu un **rând gol de adăugare**: scrii în el (data, documentul sau o sumă) și devine rând nou, iar dedesubt apare altul gol. Tabelul de buget încape **trei versiuni**, iar cel de rectificări **două**, fără bară de derulare (rândul gol vine pe lângă ele).

> Data de început a unei versiuni și data unei rectificări trebuie să fie în anul de lucru. Două
> versiuni ale aceleiași clasificații nu pot începe în aceeași zi.

O dată din tabel («Început», «Data») se poate scrie de mână sau se alege dintr-un **calendar**: în dreapta celulei e un mic buton cu calendar; un clic pe el (sau Alt+săgeată jos, sau F4 cu cursorul în celulă) deschide calendarul, iar ziua aleasă intră în celulă.

## Cum se folosește bugetul în documentul de fundamentare
<!-- slice: 0102, 0108 -->

Documentul de fundamentare arată la «Buget» ce buget avea clasificația **la data reviziei**, nu cel
de azi: **totalul** versiunii cu cea mai mare dată de început până la acea zi (cele patru trimestre
adunate), plus totalul rectificărilor de la data ei de început până la data reviziei. Trimestrul
zilei nu mai contează: o versiune care începe la 01.04 cu 100, 200, 300 și 400 dă 1.000 în orice zi
de la 01.04 încolo.

Ca să iasă corect o revizie din ianuarie – martie, adaugă pe clasificație o versiune care începe
la **01.01** a anului. Dacă la data reviziei nu există nicio versiune, documentul folosește creditul
bugetar de azi din FOREXE și te anunță pe ecran care clasificații sunt în cazul acesta.

## Angajamentele clasificației
<!-- slice: 0126-01, 0126-05 -->

Tabelul **«Angajamente»**, sub rândul de total, arată **toate angajamentele care duc la clasificația aleasă**, câte un rând pe indicator: **codul angajamentului** urmat de descrierea lui, în aceeași celulă, **codul indicatorului** și **totalurile** rezervărilor, recepțiilor și plăților lui, aceleași cifre ca în sumarul angajamentului. În subsol, **Total** adună fiecare coloană. Dacă alegi un nod mai sus de alineat, tabelul cuprinde angajamentele tuturor clasificațiilor de sub nod. Angajamentele ascunse nu apar. Tabelul se citește doar; nu ține de anul de lucru, ci de tot ce s-a înregistrat pe angajament.

**Datele se citesc o singură dată, când se deschide fereastra**: bugetul și rectificările tuturor clasificațiilor și angajamentele lor. De aceea un clic pe arbore arată totul pe loc, fără să mai aștepte serverul; în primele clipe după deschidere, tabelul «Angajamente» scrie «se încarcă…» și se umple singur. Dacă între timp se schimbă ceva în altă parte (K-BOT aduce angajamente noi din FOREXE, se salvează o rezervare, o recepție sau o plată, se salvează bugetul), fereastra își reîmprospătează singură datele și rescrie ce vezi. Dacă bugetul clasificației pe care o editezi s-a schimbat în altă parte, K-BOT îți spune în linia de jos, fără să-ți șteargă ce ai scris.

## Clasificații noi
<!-- slice: 0087 -->

Pe pagina **Cheltuieli**, butonul **«Adaugă»** (plusul verde) din grupul **Clasificații** din dreapta ribbonului deschide fereastra în care bifezi sursele / sectoarele,
apoi clasificațiile funcționale și economice. Se pot alege doar surse / sectoare pe care unitatea
le are deja. Clasificațiile care există deja sunt sărite și numărate.

## Verificarea bugetului față de FOREXE
<!-- slice: 0103-04, 0105, 0108, 0107, 0000-47, 0126-03 -->

Butonul **«Verifică bugetul»** din ribbon (grupul **Clasificații**, în dreapta) deschide fereastra [Verificare buget FOREXE](topic:contabil.nomenclatoare.verificare-buget): fiecare clasificație folosită în FOREXE, cu bugetul din K-BOT, creditul din FOREXE și diferența. Dublu clic pe un rând te duce direct la clasificația respectivă, în arbore.

## Trimite în Access
<!-- slice: 0103-06, 0126-03 -->

Butonul **«Trimite în Access»** (activ doar după ce ai salvat, cu o clasificație aleasă) scrie în
baza Access a unității:

- bugetul versiunii în vigoare azi (trimestrele 1–4);
- rectificările anului: cele care există deja în Access (același document și aceeași dată) sunt
  actualizate, celelalte sunt adăugate.

K-BOT te întreabă înainte. O rectificare ștearsă în K-BOT **nu** se șterge din Access. Dacă fișierul
Access al unității nu se găsește sau este deschis exclusiv, primești un mesaj cu motivul.

## Venituri
<!-- slice: AV01-11, AV01-12, 0126-03 -->

Pagina **Venituri** din bara verticală arată **clasificațiile de venituri**, cu bugetul lor, ca la cheltuieli.

- **Arborele**: Capitol › Subcapitol › Paragraf, cu codul în prima coloană și denumirea alături; denumirile capitolelor și subcapitolelor sunt cele din nomenclator. Căutarea din capul arborelui merge ca la Cheltuieli. O clasificație de venituri aparține unei **surse**, iar arborele urmează sursa din bara de titlu: cu o sursă anume vezi doar clasificațiile ei, cu «Toate» le vezi pe toate (iar dacă același cod există pe mai multe surse, sursa apare în paranteză lângă cod).
- **Bugetul de venituri este al unei unități**: cea care se ghicește din **sursa / sectorul din bara de titlu** și din **anul de lucru**. Dacă în bara de titlu este aleasă varianta «Toate» (sau o sursă care nu are unitate în anul ales), arborele se vede, dar tabelele rămân goale și un mesaj îți spune ce să alegi. Când schimbi anul sau sursa, tabelele arată bugetul noii unități; dacă ai modificări nesalvate, K-BOT te întreabă mai întâi ce faci cu ele.
- **Subunitățile copil lucrează direct în datele părintelui.** O subunitate copil (una care, la transferul din Access, are o subunitate părinte) are aceeași sursă ca părintele ei și folosește clasificațiile și bugetul părintelui. Tabelele se pot modifica, iar butonul **Adaugă** din ribbon scrie clasificațiile direct în datele părintelui, pentru toate subunitățile cu sursa respectivă. Subsolul arborelui arată câte clasificații sunt afișate sau, pentru o sursă cu subunități copil, indică folosirea datelor părintelui.
- **Tabelele din dreapta** sunt aceleași ca la Cheltuieli: **versiunile de buget** (coloana «Început», cele patru trimestre, totalul rândului, «+» pentru o versiune nouă, «✕» pentru ștergere), **rectificările** (numărul și data documentului, trimestrele, totalul) și **rândul de jos**, care adună ultimul buget cu toate rectificările. La un nod mai sus de paragraf vezi, la fel, doar sumarul. Data unei versiuni și data unei rectificări trebuie să fie în anul de lucru; două versiuni ale aceleiași clasificații nu pot începe în aceeași zi.
- **Salvează** (în ribbon, grupul **Clasificații**) scrie versiunile și rectificările clasificației alese, pentru unitatea din bara de titlu. Aici nu există «Verifică bugetul» și nici «Trimite în Access», nici bifele de sub arbore.

### Ștergerea unei clasificații de venituri
<!-- slice: AV01-13, 0126-03 -->

Butonul **«Șterge clasificația»**, din ribbon (grupul **Clasificații**, lângă «Salvează»), șterge clasificația aleasă în arbore, după ce te întreabă. Se activează doar când e aleasă o clasificație (nu un capitol sau subcapitol) și nu ai modificări nesalvate. **O clasificație folosită nu se șterge**: dacă are buget, rectificări sau apare în extrasele de cont, K-BOT îți spune unde este folosită și nu șterge nimic. Modificarea unei clasificații nu există: denumirea vine din nomenclator.

### Clasificație de venituri nouă
<!-- slice: AV01-11, AV01-12 -->

Butonul **Adaugă** (plusul verde) din grupul **Clasificații** din dreapta ribbonului deschide fereastra **«Adaugă clasificație de venituri»**, cu un arbore în care poți căuta codurile de venituri din nomenclator. Clasificația se adaugă la **sursa din bara de titlu**. Cu «Toate» alese, K-BOT îți cere să alegi mai întâi o sursă. Alegi un cod de capitol, subcapitol sau paragraf și apeși **«Adaugă clasificația»**. Denumirea vine din nomenclator, cu majuscule. Codurile care există deja pe sursa respectivă apar cu mențiunea «(există deja)» și nu se pot alege. După adăugare, clasificația nouă este aleasă în arbore.
