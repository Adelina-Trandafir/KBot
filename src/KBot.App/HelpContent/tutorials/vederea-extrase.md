---
id: vederea-extrase
title: Vederea «Extrase» a unui angajament
part: contabil
keywords: extrase extras vedere angajament filtrare filtru cauta mod afisare antet operatii detalii luna zi
starts: KbotForm
host-key: vederea-extrase
---
<!-- slice: 000T-10 -->

## Alege un angajament
<!-- slice: 000T-10 -->
target: KbotForm.tree
wait: select
Alege din lista de angajamente <b>angajamentul</b> ale cărui extrase vrei să le vezi.

## Deschide vederea «Extrase»
<!-- slice: 000T-10 -->
target: KbotForm.navViews
part: item:extrase
wait: tab:extrase
Apasă butonul <b>«Extrase»</b> din stânga ferestrei.<BR>
<mark>Dacă butonul lipsește sau e stins, angajamentul ales nu are extrase; alege altul.</mark>

## Arborele de extrase
<!-- slice: 000T-10 -->
target: KbotForm>ExtrasePanel.tree
Arborele are trei niveluri: <b>«Toate extrasele»</b>, apoi <b>luna</b> și apoi <b>ziua</b>. Zilele sunt datele băncii ale operațiunilor angajamentului. Apasă «Înainte».

## Alege un nod
<!-- slice: 000T-10 -->
target: KbotForm>ExtrasePanel.tree
wait: select
Alege <b>«Toate extrasele»</b> sau o lună: sus vezi antetele extraselor, jos operațiunile antetului ales. Alege o zi: vezi operațiunile zilei, iar sub ele <b>detaliul</b> operațiunii alese.

## Filtrarea coloanelor
<!-- slice: 000T-10 -->
target: KbotForm>ExtrasePanel.gridAntete
part: header.filter
when: visible
optional: yes
why: Filtrarea nu e obligatorie; o folosești doar când cauți ceva anume în tabel.
Pe fiecare coloană care are iconița de meniu poți <b>filtra</b> rândurile (după dată, plătitor, CUI...). Apasă iconița de pe coloană, alege valorile dorite, apoi apasă «Înainte».<BR>
<mark>Pasul se vede doar când tabelul de antete e pe ecran: alege «Toate extrasele» sau o lună.</mark>

## Modul de afișare
<!-- slice: 000T-10 -->
target: KbotForm>ExtrasePanel.tree
part: header.right
wait: click
Apasă <b>iconița din dreapta antetului arborelui</b>. Se deschide lista cu cele două moduri de afișare.

## Alege modul
<!-- slice: 000T-10 -->
anchor: popup.antet-operatii+operatii-detalii
<b>«Arată antet + operații»</b>: sus antetele, jos operațiunile antetului ales. <b>«Arată operații + detalii»</b>: pentru fiecare nod vezi sus operațiunile perioadei, iar jos operațiunea aleasă în întregime. Alege unul, apoi apasă «Înainte».<BR>
<mark>În modul «operații + detalii» coloanele «Data bancă», «Plătitor» și «CUI» primesc și filtrare și grupare.</mark>

## Gata
<!-- slice: 000T-10 -->
Poți schimba oricând modul de afișare. Coloanele afișate se aleg din Setări › Extrase. Apasă «Gata».
