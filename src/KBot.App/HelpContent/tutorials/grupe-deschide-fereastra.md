---
id: grupe-deschide-fereastra
title: Deschide fereastra «Grupe de angajamente»
part: contabil
keywords: grupe grupa angajamente editeaza editare deschid fereastra meniu rotita arbore grupare alias
starts: KbotForm
host-key: grupe-meniu
---
<!-- slice: 000T-12 -->

## Iconița de opțiuni a arborelui
<!-- slice: 000T-12 -->
target: KbotForm.tree
part: header.right
wait: click
Apasă <b>iconița din dreapta antetului</b> listei de angajamente (rotița). Se deschide lista cu opțiunile arborelui.

## «Grupe»
<!-- slice: 000T-12 -->
anchor: popup.grupe
wait: anchor:grupe.grupe-editeaza
Alege <b>«Grupe»</b>. Lista rămâne deschisă, iar lângă ea apare <b>lista grupelor</b>: grupele în ordine alfabetică, fiecare în culoarea ei, iar jos rândul «Editează grupe...».<BR>
<mark>Dacă alegi o grupă, lista de angajamente arată doar angajamentele ei.</mark>

## «Editează grupe...»
<!-- slice: 000T-12 -->
anchor: grupe.grupe-editeaza
wait: opens:GrupeForm
Alege <b>«Editează grupe...»</b>. Se deschide fereastra în care adaugi și modifici grupele.

## Fereastra grupelor
<!-- slice: 000T-12 -->
target: GrupeForm.tree
În stânga e arborele: primul rând, <b>«Angajamente negrupate»</b>, apoi grupele. În dreapta sunt denumirea și culoarea grupei și tabelul cu angajamente.<BR>
Din acest ecran înveți trei lucruri, fiecare într-un tutorial: <link tutorial="grupe-grupa-noua">cum adaugi o grupă</link>, <link tutorial="grupe-scoate-angajament">cum scoți un angajament dintr-o grupă</link> și <link tutorial="grupe-adauga-angajament">cum adaugi un angajament într-o grupă existentă</link>.
