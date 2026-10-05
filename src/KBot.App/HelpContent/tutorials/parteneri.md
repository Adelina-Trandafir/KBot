---
id: parteneri
title: Adaugă sau editează parteneri
part: contabil
keywords: parteneri partener adaug adaugare editez editare cod fiscal cui anap denumire iban coduri angajament ascuns nomenclatoare
starts: KbotForm
host-key: parteneri
---
<!-- slice: 000T-10 -->

## Deschide meniul
<!-- slice: 000T-10 -->
target: KbotForm.btnMeniu
wait: click
optional: yes
why: Dacă fereastra «Adăugare / editare parteneri» e deja deschisă, nu mai ai nevoie de pașii de deschidere.
Apasă butonul <b>MENIU</b>.

## Nomenclatoare
<!-- slice: 000T-10 -->
anchor: menu.nomenclatoare
allow: KbotForm.btnMeniu
wait: anchor:menu.parteneri
optional: yes
why: Dacă fereastra «Adăugare / editare parteneri» e deja deschisă, nu mai ai nevoie de pașii de deschidere.
Treci cu mausul peste <b>«Nomenclatoare»</b> sau apasă-l: se deschide lista lui.

## Parteneri
<!-- slice: 000T-10 -->
anchor: menu.parteneri
allow: KbotForm.btnMeniu
wait: opens:ParteneriForm
optional: yes
why: Dacă fereastra «Adăugare / editare parteneri» e deja deschisă, nu mai ai nevoie de pașii de deschidere.
Alege <b>«Parteneri»</b>.

## Lista partenerilor
<!-- slice: 000T-10 -->
target: ParteneriForm.tree
Aici sunt <b>partenerii unității</b>. Alegi unul ca să-i vezi datele în dreapta și ca să-l editezi. Apasă «Înainte».

## Bifa «Ascunde partenerii fără activitate»
<!-- slice: 000T-10 -->
target: ParteneriForm.chkFaraActivitate
optional: yes
why: Bifa e doar un filtru al listei; nu schimbă nimic în date.
Bifată, lista <b>ascunde partenerii care nu apar pe niciun document</b> (DDF sau ORD). E util când lista e lungă și cauți un partener folosit. Apoi apasă «Înainte».

## Adăugare
<!-- slice: 000T-10 -->
target: ParteneriForm.btnAdauga
wait: click
Pentru un partener nou apasă <b>«Adăugare»</b>. Câmpurile se golesc, iar la «Cod partener» se propune următorul cod numeric liber.

## Cod fiscal
<!-- slice: 000T-10 -->
target: ParteneriForm.txtCodFiscal
wait: changed
Scrie <b>codul fiscal</b> al partenerului, apoi apasă Enter sau treci în alt câmp.<BR>
<mark>La ieșirea din câmp (sau Enter) K-BOT caută codul fiscal la ANAF și completează singur denumirea și adresa. Un cod fiscal poate avea un singur partener.</mark>

## Denumirea
<!-- slice: 000T-10 -->
target: ParteneriForm.txtDenumire
optional: yes
why: Denumirea se completează de la ANAF; o verifici sau o corectezi doar dacă e nevoie.
Verifică <b>denumirea</b> adusă de la ANAF. Dacă codul fiscal nu a fost găsit, scrie-o tu. Apoi apasă «Înainte».

## Contul, banca și adresa
<!-- slice: 000T-10 -->
target: ParteneriForm.txtIban
optional: yes
why: IBAN-ul, banca și adresa nu sunt obligatorii.
Completează, dacă le ai, <b>contul IBAN principal</b> și <b>banca</b>; adresa vine de la ANAF. Câmpurile marcate cu * sunt obligatorii. Apoi apasă «Înainte».

## Codurile de angajament
<!-- slice: 000T-10 -->
target: ParteneriForm.gridCoduri
optional: yes
why: Codurile de angajament le adaugi doar dacă partenerul se leagă de anumite clasificații.
Aici legi partenerul de <b>coduri de angajament</b>. Cu <b>«+»</b> din subsol adaugi un rând, pe care îl completezi direct în tabel; <b>«✕»</b> îl șterge.<BR>
<mark>O clasificație poate apărea o singură dată pe partener.</mark> Apoi apasă «Înainte».

## Partener ascuns
<!-- slice: 000T-10 -->
target: ParteneriForm.chkAscuns
optional: yes
why: Un partener se ascunde doar când nu mai trebuie să apară în liste.
Bifa <b>«Partener ascuns»</b> scoate partenerul din liste. E singura cale de a scăpa de un partener deja folosit pe documente, fiindcă acela nu se poate șterge. Apoi apasă «Înainte».

## Salvare
<!-- slice: 000T-10 -->
target: ParteneriForm.btnSalveaza
wait: click
Apasă <b>«Salvare»</b>. Se scriu în baza de date partenerul și codurile lui de angajament.<BR>
<mark>«Renunță» anulează modificările nesalvate ale partenerului.</mark>

## Editarea unui partener
<!-- slice: 000T-10 -->
target: ParteneriForm.tree
wait: select
optional: yes
why: Treci peste acest pas dacă nu ai de editat un partener existent.
Pentru <b>editare</b>, alege un partener din listă: datele lui apar în dreapta.

## Modifică și salvează
<!-- slice: 000T-10 -->
target: ParteneriForm.btnSalveaza
wait: click
optional: yes
why: Salvezi doar dacă ai modificat ceva.
Schimbă câmpurile dorite (codul fiscal se caută din nou la ANAF când îl modifici) și apasă <b>«Salvare»</b>.<BR>
<mark>«Ștergere» șterge partenerul ales, dar un partener folosit pe documente nu se poate șterge, doar ascunde.</mark>
