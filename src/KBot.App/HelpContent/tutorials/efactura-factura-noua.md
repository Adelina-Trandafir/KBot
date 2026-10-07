---
id: efactura-factura-noua
title: Cum adaug o factură nouă (E-Factura)
part: contabil
keywords: efactura e-factura factura noua adaug adaugare emit emitere factura client cumparator linii continut cont emitent salvare ciorna vanzare
starts: KbotForm
host-key: efactura
---
<!-- slice: 00EF-13, 0000-58 -->

## Deschide meniul
<!-- slice: 00EF-13, 0000-58 -->
target: KbotForm.btnMeniu
wait: click
optional: yes
why: Dacă fereastra «E-Factura — facturi emise» e deja deschisă, nu mai ai nevoie de pașii de deschidere.
Apasă butonul <b>MENIU</b>.

## E-Factura
<!-- slice: 00EF-13, 0000-58 -->
anchor: menu.efactura
allow: KbotForm.btnMeniu
wait: opens:FacturiForm
optional: yes
why: Dacă fereastra «E-Factura — facturi emise» e deja deschisă, nu mai ai nevoie de pașii de deschidere.
Alege <b>«E-Factura»</b>. Se deschide fereastra facturilor emise.<BR>
<mark>Facturi poți face doar după ce unitatea are completate datele ei (butonul cu litera K din bara de titlu › «Date Unitate»). Dacă nu le are, K-BOT deschide singur fereastra aceea.</mark>

## Adăugare
<!-- slice: 00EF-13, 0000-58 -->
target: FacturiForm.btnAdauga
wait: click
Apasă <b>«Adăugare»</b>. Seria și numărul apar cu mențiunea «provizoriu»: numărul real îl primește factura la salvare.

## Clientul
<!-- slice: 00EF-13, 0000-58 -->
target: FacturiForm.cmbClient
wait: changed
Ești în vederea <b>Cumpărător</b>. Alege <b>clientul</b> facturii din listă (tastează o parte din denumire sau din codul fiscal).<BR>
<mark>Clientul nu e în listă? Apasă «Client nou», completează câmpurile și «Salvează clientul», apoi alege-l.</mark>

## Conținut
<!-- slice: 00EF-13, 0000-58 -->
target: FacturiForm.navDetaliu
wait: tab:continut
Deschide vederea <b>Conținut</b>.

## Linie nouă
<!-- slice: 00EF-13, 0000-58 -->
target: FacturiForm.btnLinieNoua
wait: click
Apasă <b>«Linie nouă»</b>: se adaugă o linie cu unitatea XPP (bucată) și cantitatea 1.

## Completează linia
<!-- slice: 00EF-13, 0000-58 -->
target: FacturiForm.gridLinii
Scrie direct în tabel: <b>Conținut</b> (ce vinzi), <b>Um</b> (unitatea de măsură, din listă), <b>Cant</b> și <b>PU</b> (prețul). <b>Valoarea</b> se calculează singură. Poți adăuga câte linii ai nevoie. Apoi apasă «Înainte».

## Generale
<!-- slice: 00EF-13, 0000-58 -->
target: FacturiForm.navDetaliu
wait: tab:generale
Deschide vederea <b>Generale</b>.

## Cont emitent
<!-- slice: 00EF-13, 0000-58 -->
target: FacturiForm.cmbContPlata
wait: changed
Alege <b>contul emitent (IBAN)</b> în care se plătește factura; implicit e cel al ultimei facturi. Poți scrie și altul.<BR>
<mark>Aici verifici și data facturii, comentariile și referința comenzii (opționale).</mark>

## Salvare
<!-- slice: 00EF-13, 0000-58 -->
target: FacturiForm.btnSalveaza
wait: click
guard: yes
Apasă <b>«Salvare»</b>. Factura apare în arbore, la client, și rămâne <b>ciornă</b> (netrimisă la ANAF).<BR>
<mark>K-BOT nu salvează până nu are un client, o dată, un cont emitent și cel puțin o linie cu conținut și unitate de măsură; îți spune ce lipsește.</mark>

## Mai departe
<!-- slice: 00EF-13, 0000-58 -->
target: FacturiForm.tree
Factura nouă se trimite la ANAF din <b>meniul ei</b> (semnul cu trei puncte din dreapta rândului, la survolare). Trimiterea, validarea și stornarea sunt explicate în ajutorul «E-Factura: facturile emise» (F1). Apasă «Înainte» ca să închei.
