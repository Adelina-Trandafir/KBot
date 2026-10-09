---
id: grupe-scoate-angajament
title: Scoate un angajament dintr-o grupă
part: contabil
keywords: scot scoate sterg debifez angajament grupa grup existenta elimin din grupa
starts: GrupeForm
host-key: grupe-scoate
---
<!-- slice: 000T-12 -->

## Alege o grupă
<!-- slice: 000T-12 -->
target: GrupeForm.tree
wait: select
Alege în arbore <b>o grupă existentă</b> (nu rândul «Angajamente negrupate»).

## Angajamentele grupei
<!-- slice: 000T-12 -->
target: GrupeForm.gridAng
Tabelul arată <b>doar angajamentele grupei</b>, toate bifate. Apasă «Înainte».

## Debifează
<!-- slice: 000T-12 -->
target: GrupeForm.gridAng
wait: signal:angajament-scos
<b>Debifează angajamentul</b> pe care vrei să-l scoți din grupă. Rândul dispare din listă.

## Salvează
<!-- slice: 000T-12 -->
target: GrupeForm.btnSalveaza
wait: click
guard: yes
Angajamentul iese din grupă abia când apeși <b>«Salvează»</b>. Un angajament scos rămâne în K-BOT; apare la «Angajamente negrupate», dacă nu mai e în nicio altă grupă.
