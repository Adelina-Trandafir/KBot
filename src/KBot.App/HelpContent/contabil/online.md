---
id: contabil.online
title: Accesul online la date
part: contabil
order: 95
parent: contabil
screens:
keywords: portal, browser, web, telefon, acasa, de la distanta, certificat, token, semnatura digitala, 2fa, doi pasi, cod e-mail, gdpr, pdf aplatizat
---
<!-- slice: 0110-03, 0110-04, 0110-06, 0110-08, 0000-52 -->
K-BOT nu este singurul loc în care îți poți vedea datele.
Le poți consulta și online, direct din browser, de oriunde te-ai afla: de la birou, de acasă sau
chiar de pe telefon. Nu trebuie să instalezi nimic.

Pagina online este gândită exclusiv pentru consultare. Poți vedea:

* lista angajamentelor;
* Sumarul;
* Rezervările;
* Recepțiile;
* Plățile;
* documentele PDF.

Nu poți modifica date, nu poți descărca informații noi din FOREXE și nu poți trimite operațiuni.
Lucrul propriu-zis rămâne în K-BOT. Online vezi informația de care ai nevoie, fără riscul de a
modifica ceva din greșeală.

## Documentele PDF văzute online
<!-- slice: 0110-08, 0000-52 -->

PDF-urile din pagina online sunt o **copie aplatizată** a fișierelor originale (fără câmpuri de
completat). Se pot vedea **doar dacă au fost semnate inițial în aplicația locală** K-BOT. Un document
care nu a trecut prin semnarea din K-BOT nu apare online.

## Cum intri
<!-- slice: 0110-03, 0000-52 -->

Accesul este simplu:

1. Deschizi pagina K-BOT în browser și apeși **Intră în cont**.
2. Introduci adresa de e-mail și parola — aceleași pe care le folosești în K-BOT.
3. Primești pe e-mail un cod de 6 cifre și îl introduci în pagină. Codul poate fi folosit o singură
   dată și expiră după 10 minute.
4. Alegi unitatea, apoi anul și sursa cu care vrei să lucrezi.

Dacă ai acces la mai multe unități, le poți schimba oricând din bara de sus.

Fără conturi separate, fără încă o parolă de ținut minte. Avem deja destule.

## Intrare cu certificatul digital, fără codul primit pe e-mail
<!-- slice: 0110-04, 0000-52 -->

Dacă folosești un token cu semnătură digitală calificată, același tip de certificat cu care
semnezi documentele, îl poți înrola și pentru accesul online. După înrolare, poți intra folosind
certificatul, fără să mai introduci codul primit pe e-mail.

**Cum îl înrolezi**

1. Intri prima dată folosind parola și codul primit pe e-mail.
2. Conectezi tokenul la calculator.
3. Apeși **Certificat** în partea de sus a paginii.
4. Alegi **Înrolează certificatul acesta**.

La următoarea vizită poți apăsa direct **Intră cu certificat digital** și alegi certificatul
corespunzător.

**De reținut**

* Un singur certificat poate fi activ pentru contul tău. Dacă certificatul este reînnoit, îl
  înrolezi din nou, iar cel vechi este dezactivat.
* Din același dialog **Certificat** îl poți dezactiva în orice moment.
* Autentificarea cu certificat funcționează de pe calculator, unde tokenul poate fi conectat
  fizic. De pe telefon sau tabletă intri în continuare cu adresa de e-mail, parola și codul primit
  pe e-mail.
* Dacă certificatul a expirat sau browserul nu îl poate identifica, pagina revine automat la
  autentificarea cu parolă și cod.

## Datele pot fi văzute de oriunde. Nu de oricine.
<!-- slice: 0110-03, 0110-04, 0000-52 -->

Accesul din browser nu înseamnă că datele devin mai puțin protejate. Din contră, accesul online
folosește mai multe mecanisme de protecție tocmai pentru că informațiile pot fi consultate din
afara calculatorului pe care este instalat K-BOT.

**Autentificare în doi pași.** Fără certificat digital, autentificarea cere întotdeauna parola
**și** codul primit pe e-mail. Parola singură nu este suficientă pentru acces. Dacă folosești
certificatul digital, al doilea factor este chiar tokenul fizic pe care îl deții.

**Vezi doar unitățile la care ai acces.** După autentificare, K-BOT îți arată doar unitățile
pentru care contul tău are deja drepturi. Nu poți vedea datele altor unități.

**Accesul online este doar pentru citire.** Din browser poți consulta informațiile, dar nu le poți
modifica. Nu poți:

* modifica angajamente;
* schimba recepții;
* trimite operațiuni;
* descărca informații noi din FOREXE;
* altera documentele existente.

Poți să te uiți. Nu poți să strici nimic.

**Sesiunile nu rămân deschise la nesfârșit.** După 20 de minute fără activitate, sesiunea se
închide automat. Cu două minute înainte, K-BOT te avertizează și îți permite să continui dacă încă
lucrezi. Indiferent de activitate, o sesiune nu poate rămâne deschisă mai mult de 8 ore.

**Parola nu este păstrată de pagina online.** Parola este verificată atunci când te autentifici și
nu este păstrată ulterior de pagină. Codul primit pe e-mail are o durată de viață limitată, poate
fi folosit o singură dată și este păstrat doar într-o formă criptată până la expirare.

**Accesările lasă o urmă.** K-BOT păstrează informații despre autentificări: cine a accesat
sistemul, când a avut loc accesul și unitatea consultată. Astfel, accesările pot fi verificate
ulterior dacă este nevoie.

**Încercările greșite sunt limitate.** Dacă sunt introduse în mod repetat parole sau coduri
greșite, autentificarea este blocată temporar. Nu poți încerca la nesfârșit până când „poate
nimerește cineva parola”.

## Protecția datelor și certificatul digital
<!-- slice: 0110-04, 0000-52 -->

Datele rămân pe serverele K-BOT și sunt disponibile numai persoanelor autorizate pentru unitățile
respective. Pentru certificatul digital, serverul păstrează doar amprenta necesară identificării
certificatului. Nu este necesară păstrarea CNP-ului sau a altor date personale din certificat
pentru această autentificare.

## Când ai terminat
<!-- slice: 0110-03, 0000-52 -->

Apasă **Ieși**. Este mai bine decât să închizi pur și simplu fila browserului, mai ales atunci
când folosești un calculator care nu îți aparține. Un click în plus. De data asta chiar merită.
