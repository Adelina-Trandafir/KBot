---
id: grupe-adauga-angajament
title: Adaugă un angajament într-o grupă existentă
part: contabil
keywords: adaug angajament grupa existenta negrupate trag drag drop mut grup
starts: GrupeForm
host-key: grupe-adauga
---
<!-- slice: 000T-12 -->

## Angajamente negrupate
<!-- slice: 000T-12 -->
target: GrupeForm.tree
wait: select
Alege primul rând din arbore, <b>«Angajamente negrupate»</b>. Tabelul arată angajamentele care nu sunt în nicio grupă.<BR>
<mark>Dacă lista e goală, toate angajamentele sunt deja în grupe.</mark>

## Trage un angajament
<!-- slice: 000T-12 -->
target: GrupeForm.gridAng
allow: GrupeForm.tree
wait: signal:angajament-adaugat
Apasă pe un angajament din tabel (nu pe coloana «Alias»), <b>ține apăsat butonul mouse-ului și trage rândul peste o grupă</b> din arbore, apoi dă drumul. Angajamentul intră în grupă pe loc și dispare din listă; nu mai trebuie să apeși «Salvează».

## Gata
<!-- slice: 000T-12 -->
target: GrupeForm.tree
Alege acum grupa în care ai tras angajamentul: îl găsești în tabelul ei, <b>bifat</b>.
