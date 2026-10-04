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
Apasă butonul «Rezervări». K-BOT nu îl apasă pentru tine.

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
Alege din listă compartimentul documentului. Câmpul se completează doar la documentul inițial; la o revizie ulterioară compartimentul vine de la prima revizie și acest pas nu apare.

## Bifează partenerul
<!-- slice: 000T -->
target: DdfEditForm.chkPartAng
when: enabled
wait: checked
optional: yes
why: Îl bifezi doar dacă documentul se leagă de un partener; dacă nu are partener, lași căsuța nebifată și treci mai departe.
Bifează «Partener asociat» dacă documentul are un partener principal.

## Alege partenerul
<!-- slice: 000T -->
target: DdfEditForm.cmbPartener
when: checked:DdfEditForm.chkPartAng
wait: changed
optional: yes
why: Câmpul se folosește doar când «Partener asociat» e bifat.
Scrie începutul numelui sau al codului fiscal și alege partenerul din listă.

## Descriere scurtă
<!-- slice: 000T -->
target: DdfEditForm.txtDescScurta
wait: changed
optional: yes
why: Descrierea scurtă ajută la recunoașterea documentului în listă, dar documentul se poate salva și fără ea.
Completează «Descriere scurtă» cu câteva cuvinte despre document.

## Element fundamentare
<!-- slice: 000T -->
target: DdfEditSectiuneaAPage.grd
when: editable
wait: manual
optional: yes
why: Celula se completează de mână doar la un document care nu vine din rezervări; la unul făcut din rezervări rândurile sunt gata și acest pas nu apare.
Completează celula «Element fundamentare» de pe primul rând, apoi apasă «Înainte».

## Fila «Descriere»
<!-- slice: 000T -->
target: DdfEditForm.navSub
part: item:descriere
wait: tab:descriere
optional: yes
why: Descrierea lungă nu e obligatorie.
Deschide fila «Descriere».

## Descriere lungă
<!-- slice: 000T -->
target: DdfEditDescrierePage.edtLunga
wait: manual
optional: yes
why: Descrierea lungă nu e obligatorie.
Scrie descrierea lungă a documentului, apoi apasă «Înainte».

## Fila «Fișiere»
<!-- slice: 000T -->
target: DdfEditForm.navSub
part: item:fisiere
wait: tab:fisiere
optional: yes
why: Atașamentele nu sunt obligatorii.
Deschide fila «Fișiere».

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
Deschide fila «Parteneri».

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
Apasă «Salvează documentul». Pașii opționali de mai sus pot fi sărituți: poți salva oricând.
