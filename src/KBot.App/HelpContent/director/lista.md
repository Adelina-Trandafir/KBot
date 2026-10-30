---
id: director.lista
title: Documente de semnat
part: director
order: 15
parent: director
screens: DirectorForm.lstDocumente, DirectorForm.lblTitlu, DirectorForm.btnReincarca
keywords: lista, documente de semnat, reincarca, unitate, angajament, revizie, total
---
<!-- slice: 0081-06, DIR1-02, 0000-141 -->
Butonul **«Documente de semnat»** din panglică deschide o fereastră cu **toate** documentele de
fundamentare ale unității la care ești conectat, care așteaptă semnătura ta — fără să cauți
angajament cu angajament. Titlul spune câte sunt: «Aveți 3 documente de fundamentare de
semnat» sau «Nu aveți documente de fundamentare de semnat».

<!-- capture: director-lista | caption: Lista documentelor de semnat | prepare: Conectați-vă ca director pe o unitate care are cel puțin un document semnat A și B, apăsați «Documente de semnat» și faceți poza pe calculatorul directorului. -->

| Coloană | Ce arată |
|---------|----------|
| **Angajament** | codul angajamentului |
| **Obiect** | obiectul documentului de fundamentare |
| **Rev.** | numărul reviziei |
| **Data** | data reviziei |
| **Total** | valoarea totală a reviziei, în lei |

Ținând mouse-ul pe un rând vezi descrierea scurtă a documentului. **Un clic pe un rând deschide
documentul în dreapta ferestrei**, direct pe pagina «Document PDF», gata de semnat (pagina «Vizualizare» e alături), fără arborele
de revizii. Vezi [Semnarea ca director](topic:director.semnare).

## Mesajul de lângă ceas
<!-- slice: TRAY-01, 0000-142 -->

Chiar dacă K-BOT nu e deschis, o iconiță mică din zona de notificări (lângă ceas) verifică **din 10 în 10
minute** dacă ai documente de semnat. Dacă ai, apare deasupra ceasului un mesaj cu lista lor: pe fiecare rând
vezi angajamentul, revizia, valoarea, data, unitatea și obiectul.

- **Un clic pe un rând** pornește K-BOT (sau îl aduce în față, dacă e deschis) direct pe lista documentelor de
  semnat, cu documentul acela deja deschis.
- **Un clic pe mesaj, în afara rândurilor,** deschide aceeași listă, fără niciun document ales.
- Mesajul dispare singur după câteva zeci de secunde, la **Esc** sau când dai clic în altă parte.
- **Clic pe iconiță** arată din nou lista; **clic dreapta** deschide meniul: «Deschide K-BOT», «Verifică acum», «Autentificare...»,
  «Pornește cu Windows» și, ultimul, **«Închide»**.

Iconița pornește odată cu Windows și rămâne acolo până o închizi din meniu. Ca să poată verifica, are nevoie de
e-mailul și parola ta, în două feluri: fie te-ai conectat în K-BOT, în această sesiune Windows, cu **«Ține minte
parola»** bifat, fie i le dai chiar iconiței — o fereastră mică de autentificare se deschide la prima verificare (sau din
meniu, **«Autentificare...»**). Parola dată iconiței este ținută doar cât rulează ea, criptat, și nu se scrie pe disc;
la următoarea pornire a Windows te întreabă din nou. Dacă parola e greșită, fereastra revine cu mesajul serverului.

## Ce unitate
<!-- slice: DIR1-02, 0000-141 -->

Lista arată doar documentele unității la care ești conectat. Pentru alta, alege unitatea din bara de
titlu a ferestrei principale și deschide din nou lista. Vezi [Mai multe unități](topic:director.unitati).

## «Reîncarcă lista» și «Închide»
<!-- slice: 0081-06 -->

Lista **nu se actualizează singură** după ce semnezi. Apasă **«Reîncarcă lista»**: documentele
semnate dispar, iar cele ajunse între timp la tine apar. **«Închide»** te întoarce la fereastra
principală.

## Când lista nu se încarcă
<!-- slice: 0081-06 -->

- **«Lista unității nu a putut fi citită acum.»** după titlu — încearcă din nou mai târziu cu
  «Reîncarcă lista»; dacă mesajul rămâne, anunță administratorul K-BOT.
- **Un mesaj de eroare în locul titlului** — lista întreagă n-a putut fi citită (de exemplu, nu
  există legătură cu serverul). Verifică internetul și apasă «Reîncarcă lista».
- **Un document pe care îl aștepți nu apare** — probabil nu are încă ambele semnături, A și B, pe
  varianta finală. Întreabă contabilul unității.
