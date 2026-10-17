---
id: contabil.ddf.trimitere
title: Trimiterea în FOREXE
part: contabil
order: 60
parent: contabil.ddf
keywords: trimite in forexe, reia trimiterea, trimitere intrerupta, creare angajament, incarca rezervare, capturi
open: view:ddf
---
<!-- slice: 0081-04, 0107 -->
O revizie **semnată pe A** se trimite în FOREXE din vederea «Fundamentare»: alege revizia cu clic
stânga, apoi **clic dreapta** pe ea › **Trimite în FOREXE**. Trebuie să fii conectat la FOREXE.

<!-- capture: ddf-trimite-meniu | caption: Meniul reviziei, cu «Trimite în FOREXE» | goto: view:ddf | prepare: Alegeți o revizie în starea «Semnat A — gata de trimis» și faceți clic dreapta pe ea. | redo: 2026-10-17 20:00 | why: 0114-06/14: ribbonul are in dreapta grupul machetei (langa «Inchide») si grupul vederii; meniul din subsolul listei are doua randuri; E-Factura sta sub Extrase in bara vederilor; 0114-05: arborii nu mai au iconita de descarcare a extraselor (cea cu cardul din lista angajamentelor a plecat; descarcarea e butonul din ribbon) -->

## Ce face robotul
<!-- slice: 0081-03, 0081-04, 0081-05, 0078-06 -->

- **Revizia 0 a unui angajament nou** — creează angajamentul în FOREXE, cu câte un rând pe fiecare
  rând din Secțiunea A. Codul real al angajamentului înlocuiește peste tot codul provizoriu «!».
- **O revizie nouă a unui angajament existent** — pune în FOREXE valoarea nouă pe fiecare rând
  (un cod SSI nou devine rând nou).
- Motivul trimis în FOREXE este descrierea scurtă a reviziei, urmată de **(REV:n)**, ca din
  FOREXE să se vadă ce revizie a făcut schimbarea.
- La fiecare pas robotul face **capturi de ecran**; ele intră în Secțiunea B a PDF-ului final.
- La sfârșit K-BOT **verifică** tabelul din FOREXE față de Secțiunea A și descarcă din nou
  angajamentul.

Apoi:

- la un **angajament nou**, revizia trece în «Trimis în FOREXE — în lucru»: urmează Definitivează,
  Derulează și Generează PDF final — [Definitivează, Derulează](topic:contabil.ddf.rezervare);
- la **orice altă revizie**, K-BOT pune imediat Secțiunea B în documentul semnat pe A și îl deschide
  pentru semnătura B — [Semnarea](topic:contabil.ddf.semnare).

## Trimitere întreruptă
<!-- slice: 0081-04, 0081-07 -->

Dacă trimiterea se oprește la jumătate (FOREXE nu răspunde, sesiunea cade), FOREXE poate fi
deja schimbat. Revizia trece în **«Trimitere întreruptă»**. Clic dreapta › **Reia trimiterea în
FOREXE** continuă de unde a rămas; un angajament nou **nu** se creează a doua oară.

> Nu corecta de mână în FOREXE ce a rămas la jumătate. Reia trimiterea din K-BOT.
