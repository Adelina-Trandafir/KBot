---
id: setare-tutorial-initial
title: Pornește sau oprește tutorialul de început
part: contabil
keywords: tutorial tutoriale inceput pornire setari setare oprit opresc pornesc bifa debifez nu mai arata
starts: KbotForm
host-key: setare-tutorial-initial
---
<!-- slice: 000T-07 -->
## Deschide meniul
<!-- slice: 000T-07 -->
target: KbotForm.btnMeniu
wait: click
Apasă butonul <b>MENIU</b>. Se deschide lista cu opțiuni.

## Configurare K-BOT
<!-- slice: 000T-07 -->
anchor: menu.setari
allow: KbotForm.btnMeniu
wait: opens:SetariForm
Alege <b>«Configurare K-BOT»</b>, chenarul de pe ecran. Se deschide fereastra de setări. Dacă s-a închis lista, apasă din nou <b>MENIU</b>.

## Pagina «Aplicație»
<!-- slice: 000T-07 -->
target: SetariForm.navViews
part: item:aplicatie
wait: tab:aplicatie
Alege pagina <b>«Aplicație»</b> din lista din stânga.

## Fila «Generale»
<!-- slice: 000T-07 -->
target: SetariAplicatieView.navPagini
part: item:generale
wait: tab:generale
Casetele sunt pe fila <b>«Generale»</b>. Dacă ești deja pe ea, mergem mai departe.

## Casuța tutorialului
<!-- slice: 000T-07 -->
target: SetariAplicatieView.chkTutorialInitial
wait: changed
optional: yes
why: Poți lăsa setarea așa cum e; schimbi bifa doar dacă vrei ca tutorialul de început să pornească sau să nu mai pornească singur.
<b>Bifat</b>: tutorialul de început pornește singur la pornirea K-BOT, până îl vezi până la capăt. <b>Debifat</b>: nu mai pornește; îl găsești oricând la «?», sub «Tutoriale». Schimbă bifa.

## Gata
<!-- slice: 000T-07 -->
Setarea se salvează singură, nu e nevoie de alt buton. Poți închide fereastra de setări. Apasă «Gata».
