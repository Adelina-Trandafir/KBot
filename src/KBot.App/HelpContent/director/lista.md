---
id: director.lista
title: Lista documentelor de semnat
part: director
order: 10
parent: director
screens: DirectorForm.lstDocumente, DirectorForm.lblTitlu, DirectorForm.btnReincarca
keywords: lista, documente de semnat, reincarca, unitate, angajament, revizie, total
---
La deschidere, K-BOT caută în **toate unitățile** în care ai rolul Director reviziile semnate A și
B care așteaptă semnătura ta. Titlul de sus spune câte sunt: «Aveți 3 documente de fundamentare de
semnat» sau «Nu aveți documente de fundamentare de semnat».

<!-- capture: director-lista | caption: Lista documentelor de semnat | prepare: Conectați-vă ca director pe o unitate care are cel puțin un document semnat A și B. Faceți poza pe calculatorul directorului și încărcați-o cu «Încarcă». -->

| Coloană | Ce arată |
|---------|----------|
| **Unitate** | unitatea căreia îi aparține documentul |
| **Angajament** | codul angajamentului |
| **Obiect** | obiectul documentului de fundamentare |
| **Rev.** | numărul reviziei |
| **Data** | data reviziei |
| **Total** | valoarea totală a reviziei, în lei |

Ținând mouse-ul pe un rând vezi descrierea scurtă a documentului.

**Un clic pe un rând deschide documentul în dreapta.** Dacă documentul e din altă unitate decât
cea pe care ești conectat, K-BOT îți cere întâi să te conectezi pe unitatea lui — vezi
[Documente din alte unități](topic:director.unitati).

## «Reîncarcă lista»

Lista **nu se actualizează singură** după ce semnezi. Apasă **«Reîncarcă lista»**: documentele
semnate dispar, iar cele ajunse între timp la tine apar.

## Când lista nu se încarcă

- **«Nu s-au putut citi: ...»** după titlu — unitățile numite n-au putut fi citite acum. Restul
  listei e bună. Încearcă din nou mai târziu cu «Reîncarcă lista»; dacă mesajul rămâne, anunță
  administratorul K-BOT.
- **Un mesaj de eroare în locul titlului** — lista întreagă n-a putut fi citită (de exemplu, nu
  există legătură cu serverul). Verifică internetul și apasă «Reîncarcă lista».
- **Un document pe care îl aștepți nu apare** — probabil nu are încă ambele semnături, A și B, pe
  varianta finală. Întreabă contabilul unității.
