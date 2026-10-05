---
id: revizie-din-rezervare
title: Adaugă o revizie pe baza unei rezervări existente
part: contabil
keywords: revizie rezervare rezervari ddf document fundamentare adaug creez nou plus compartiment partener descriere fisiere salvez
starts: KbotForm
host-key: rezervare-plus
---
<!-- slice: 000T -->

## Alege un angajament cu rezervări
<!-- slice: 000T -->
target: KbotForm.tree
anchor: tree.angajamente-cu-rezervari
wait: signal:angajament-cu-rezervari
merge: yes
Alege din lista de angajamente unul care are rezervări; sunt cele luminate. Doar pentru ele apare butonul «Rezervări» în stânga ferestrei.

## Deschide vederea «Rezervări»
<!-- slice: 000T -->
target: KbotForm.navViews
part: item:rezervari
wait: tab:rezervari
Apasă butonul «Rezervări».

## Alege rezervarea care are «+»
<!-- slice: 000T -->
target: RezervariView.tree
anchor: rezervari.plus
wait: select
merge: yes
«+» apare în dreapta primei rezervări din care nu s-a făcut încă un document. Dacă nu îl vezi, deschide luna în care se află rezervarea.

## Apasă «+»
<!-- slice: 000T -->
target: RezervariView.tree
anchor: rezervari.plus
wait: click
Apasă «+» din dreapta rezervării. K-BOT pregătește documentul și îl deschide.

## Se deschide documentul
<!-- slice: 000T -->
wait: opens:DdfEditForm
Așteaptă puțin: K-BOT pregătește documentul de fundamentare și îl deschide într-o fereastră nouă.

## Completează compartimentul
<!-- slice: 000T -->
target: DdfEditForm.cmbComp
when: enabled
wait: changed
Alege din listă compartimentul documentului. Dacă nu există în listă, tastează-l. Fiecare compartiment adăugat va fi disponibil la următoarea revizie zero. <BR> <mark>Câmpul se completează doar la documentul inițial; la o revizie ulterioară compartimentul vine de la prima revizie și acest pas nu apare.</mark>

## Bifează partenerul
<!-- slice: 000T -->
target: DdfEditForm.chkPartAng
when: enabled
wait: checked
optional: yes
why: Îl bifezi doar dacă documentul se leagă de un partener; dacă nu are partener, lași căsuța nebifată și treci mai departe.
Dacă angajamentul se referă la unul sau mai mulți beneficiari, bifează «Partener asociat».<BR> <mark>Nu bifa «Partener asociat» pentru angajamentele de salarii, burse, transport, sa. Ele NU se referă la un beneficiar anume!</mark>

## Alege partenerul
<!-- slice: 000T -->
target: DdfEditForm.cmbPartener
when: checked:DdfEditForm.chkPartAng
wait: changed
optional: yes
why: Câmpul se folosește doar când «Partener asociat» e bifat.
Scrie începutul numelui sau al codului fiscal și alege partenerul din listă.<BR>
<mark>Poți selecta cu mausul sau apăsa ENTER!</mark>

## Descriere scurtă
<!-- slice: 000T -->
target: DdfEditForm.txtDescScurta
wait: changed
optional: yes
why: Descrierea scurtă ajută la recunoașterea documentului în listă, dar documentul se poate salva și fără ea.
Completează «Descriere scurtă» cu câteva cuvinte despre document, apoi apasă Enter sau treci în alt câmp.<BR> <mark>Pentru Revizia 0, descrierea reprezintă numele angajamentului din CAB. Pentru restul reviziilor, acesta este completat implicit cu ”mărire” sau ”creștere” in funcție de valoarea rezervării.</mark>

## Element fundamentare
<!-- slice: 000T -->
target: DdfEditSectiuneaAPage.grd
when: editable
optional: yes
why: Celula se completează de mână doar la un document care nu vine din rezervări; la unul făcut din rezervări rândurile sunt gata și acest pas nu apare.
Completează celula «Element fundamentare» de pe primul rând, apoi apasă «Înainte».<BR>
<mark>Pentru ușurarea muncii contabilului, această celulă este completată în mod implicit cu denumirea clasificației folosite. Dacă se dorește modificarea ei, se poate suprascrie.</mark>

## Fila «Descriere»
<!-- slice: 000T -->
target: DdfEditForm.navSub
part: item:descriere
wait: tab:descriere
optional: yes
why: Descrierea lungă nu e obligatorie.
Dacă dorești și modificarea câmpului «Descriere Lungă», deschide fila «Descriere».

## Descriere lungă
<!-- slice: 000T -->
target: DdfEditDescrierePage.edtLunga
optional: yes
why: Descrierea lungă nu e obligatorie.
Scrie descrierea lungă a documentului, apoi apasă «Înainte».<BR>
<mark>În caseta «Descriere Lungă» se poate scrie text formatat sau se poate lipi un text scris într-un editor de texte (precum Microsoft Word). Acesta își va păstra formatarea.</mark>

## Fila «Fișiere»
<!-- slice: 000T -->
target: DdfEditForm.navSub
part: item:fisiere
wait: tab:fisiere
optional: yes
why: Atașamentele nu sunt obligatorii.
Dacă dorești atașarea unor fișiere, precum <b>Nota de Comanda</b>, sau <b>Ștat de plată</b> în cazul angajamentelor de salarii eschide fila «Fișiere».

## Atașează fișiere
<!-- slice: 000T -->
target: DdfEditFisierePage.btnAdauga
wait: click
optional: yes
why: Atașezi fișiere doar dacă ai documente care trebuie să însoțească fundamentarea.
Apasă «Atașează fișier» și alege fișierele de pe disc.

## Fila «Parteneri»
<!-- slice: 000T -->
target: DdfEditForm.navSub
part: item:parteneri
when: checked:DdfEditForm.chkPartAng
wait: tab:parteneri
optional: yes
why: Fila e pentru documentele cu mai mulți parteneri și se vede doar când «Partener asociat» e bifat.
Dacă angajamentul curent se referă la mai mulți beneficiari, aceștia pot fi adăugați/editați în fila «Parteneri».

## Alege încă un partener
<!-- slice: 000T -->
target: DdfPartnersView.cmbPartner
wait: changed
optional: yes
why: Partenerul principal e deja în listă; adaugi aici doar pe ceilalți.
Caută în listă un alt partener și alege-l.

## Asociază partenerul
<!-- slice: 000T -->
target: DdfPartnersView.btnAdd
wait: click
optional: yes
why: Partenerul ales se leagă de document doar după ce apeși «Asociază».
Apasă «Asociază».

## Salvează documentul
<!-- slice: 000T -->
target: DdfEditForm.btnSalveaza
wait: click
guard: yes
Apasă «Salvează documentul». Pașii opționali de mai sus se pot sări cu «Sari peste».
