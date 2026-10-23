---
id: efactura-trimite-la-vercon
title: Trimite facturile primite la Vercon
part: contabil
keywords: vercon trimite trimitere factura facturi primite efactura cumparare nir administrator cont propus gestiune linii retrage schimba conturile furnizor luna
starts: AvacontMain
host-key: efactura-vercon
---
<!-- slice: 00EF-48, 0000-118 -->

## Fila VERCON
<!-- slice: 00EF-48, 0000-118, 0123-01, 0000-119 -->
anchor: ribbon-tab.vercon
wait: signal:ribbon-tab:vercon
Facturile primite de la furnizori ajung la <b>Vercon</b> prin pagina «Trimite la Vercon»: tu alegi ce se trimite, iar Administratorul le preia apoi într-un NIR.<br><br>Apasă fila <b>VERCON</b> din panglica de sus.

## Butonul «Trimite la Vercon»
<!-- slice: 00EF-48, 0000-118 -->
anchor: ribbon.efactura_vercon
wait: signal:ribbon:efactura_vercon
Apasă <b>Trimite la Vercon</b>. Se deschide o pagină care rămâne deschisă până o închizi.<BR>
<mark>Doar un utilizator cu rolul «Contabil» poate trimite facturi.</mark>

## Arborele facturilor
<!-- slice: 00EF-48, 0000-118 -->
target: VerconForm.tree
În stânga sunt <b>facturile primite din anul ales</b> (fără notele de credit), pe luni și furnizori, ca în «Cumpărare». După număr, o factură care a mai fost trimisă are un cuvânt: <b>trimisă</b>, <b>preluată</b> sau <b>returnată</b>; cu mouse-ul deasupra vezi starea întreagă. Apasă «Înainte» (săgeata verde cu bară, jos în dreapta, ⇥).

## Gruparea
<!-- slice: 00EF-48, 0000-118 -->
target: VerconForm.tree
optional: yes
why: Gruparea după furnizor e doar o altă înfățișare a arborelui; poți trimite la fel de bine din gruparea pe luni.
Butonul din <b>dreapta antetului arborelui</b> schimbă gruparea în <b>«Grupare după furnizor»</b> și înapoi. Bifele rămân. Apasă «Înainte» (săgeata verde cu bară, jos în dreapta, ⇥).

## Bifează facturile
<!-- slice: 00EF-48, 0000-118 -->
target: VerconForm.tree
Bifează <b>facturile de trimis</b>. Bifa unei <b>luni</b> sau a unui <b>furnizor</b> le bifează pe toate facturile lui, iar în dreapta apare câte un grup pentru fiecare factură.<BR>
<mark>O factură deja preluată într-un NIR rămâne bifată în gri și nu se mai trimite. La un furnizor cu multe facturi, citirea durează puțin.</mark> Apasă «Înainte» (săgeata verde cu bară, jos în dreapta, ⇥) după ce ai bifat.

## Liniile facturilor
<!-- slice: 00EF-48, 0000-118 -->
target: VerconForm.gridLinii
În dreapta vezi <b>liniile facturilor bifate</b>, grupate pe factură. Se trimit <b>toate</b> liniile fiecărei facturi; nu bifezi linii. Apasă «Înainte» (săgeata verde cu bară, jos în dreapta, ⇥).

## Contul din Vercon
<!-- slice: 00EF-48, 0000-118 -->
target: VerconForm.gridLinii
Pe fiecare linie poți alege, dacă vrei, <b>contul din Vercon</b>: lista arată conturile grupate pe gestiune, din toate gestiunile; un clic pe numele unei gestiuni o strânge sau o desface, iar dacă tastezi o parte din nume rămân doar rândurile potrivite. Contul ales pe <b>rândul unei facturi</b> (bara ei) se scrie pe toate liniile ei.<BR>
<mark>Contul este doar o propunere: Administratorul îl găsește ales când preia factura și îl poate schimba. «(fără cont)» lasă alegerea lui.</mark> Apasă «Înainte» (săgeata verde cu bară, jos în dreapta, ⇥).

## Nota pentru Administrator
<!-- slice: 00EF-48, 0000-118 -->
target: VerconForm.txtNota
optional: yes
why: Nota e opțională; o scrii doar dacă ai ceva de spus Administratorului.
Aici poți scrie o <b>notă scurtă</b> pe care Administratorul o vede când preia factura. Se salvează o singură dată, la trimitere. Apasă «Înainte» (săgeata verde cu bară, jos în dreapta, ⇥).

## Trimite
<!-- slice: 00EF-48, 0000-118 -->
target: VerconForm.btnTrimite
wait: click
guard: yes
Apasă <b>«Trimite la Vercon»</b>. Facturile bifate pleacă cu toate liniile și conturile alese și dispar din dreapta; în arbore primesc cuvântul «trimisă».<BR>
<mark>Nimic nu pleacă până la acest buton. Ca să nu trimiți din greșeală, tutorialul se oprește aici: apasă butonul tu, după ce ai ieșit din tutorial.</mark>

## Retragere sau schimbare
<!-- slice: 00EF-48, 0000-118 -->
Cât timp Administratorul nu a preluat o factură, o poți <b>retrage</b> sau îi poți <b>schimba conturile</b>: bifează-o singură în arbore și apar butoanele <b>«Salvează conturile»</b> și <b>«Retrage de la Vercon»</b>. O factură retrasă sau returnată de Administrator se poate trimite din nou. Dacă un NIR făcut din ea este anulat în Vercon, factura așteaptă iarăși Administratorul.

## În Vercon
<!-- slice: 00EF-48, 0000-118 -->
Administratorul deschide în Vercon «Adăugare NIR» și apasă <b>«NIR din e-Factura…»</b>: vede facturile trimise de tine, alege una și ciorna NIR se completează cu furnizorul, numărul, data, valoarea și conturile propuse. El alege produsul fiecărei linii sau poate returna factura, cu un motiv pe care îl vezi în arbore.
