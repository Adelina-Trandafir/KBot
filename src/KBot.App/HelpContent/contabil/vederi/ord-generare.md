---
id: contabil.ord.generare
title: Ordonanțări noi (și ștergerea lor)
part: contabil
order: 71
parent: contabil.vederi.ord
screens: OrdZiuaForm
keywords: ordonantare noua, adauga ordonantare, generare in lot, plati neordonantate, ziua, sterge ordonantarea
---
<!-- slice: 0049, 0049-01 -->
O ordonanțare se face **din plăți**: acoperă plățile **neordonanțate** ale unei zile. Plățile
vin din vederea [Plăți](topic:contabil.vederi.plati).

## O ordonanțare, pentru o zi
<!-- slice: 0049-01, 0049-02 -->

Două drumuri:

- **Din vederea Plăți** — semnul **«+»** de pe o **zi** (apare doar pe zilele cu plăți
  neordonanțate). Ziua e chiar cea apăsată, deci nu mai e întrebată.
- **Din vederea Ordonanțare** — «Adaugă ordonanțare…» (clic dreapta) sau iconița «Adaugă» din
  subsolul arborelui. K-BOT întreabă întâi **ziua**:

<!-- capture: ord-ziua | caption: Fereastra «Ordonanțare nouă» — alegerea zilei | goto: view:ord | prepare: Clic dreapta pe arborele ordonanțărilor › «Adaugă ordonanțare…». -->

Alegi ziua și apeși **«Generează»**. K-BOT pregătește ordonanțarea din plățile zilei și o
deschide în [editor](topic:contabil.ord.editor). **Nimic nu se scrie până nu apeși «Salvează
ordonanțarea»** în editor.

Dacă ziua are **peste 25 de parteneri**, editorul te avertizează de la început: plățile zilei
trebuie împărțite pe mai multe ordonanțări.

## Generare în lot
<!-- slice: 0049-01 -->

**«Generare în lot…»** (clic dreapta în vederea Ordonanțare) face câte o ordonanțare pentru
**fiecare zi** cu plăți neordonanțate a angajamentului. Semnul **«+»** de pe o **lună**, în
vederea Plăți, face același lucru doar pentru acea lună.

- K-BOT spune întâi câte zile și câte ordonanțări estimate sunt și cere confirmarea.
- Fiecare zi se generează și **se salvează direct**, fără editor și fără alte întrebări.
- **La prima eroare generarea se oprește** și spune ziua și motivul. Ordonanțările salvate până
  acolo **rămân salvate**; după ce rezolvi cauza, reiei — zilele deja acoperite nu se mai propun.

## Ștergerea
<!-- slice: 0033, 0097 -->

**«Șterge ordonanțarea»** (clic dreapta pe o ordonanțare) cere confirmarea cu numărul, data și
totalul. Odată cu ordonanțarea se șterg beneficiarii, rândurile de plată, documentele
justificative, atașamentele și PDF-ul semnat de pe server. **Plățile acoperite redevin
neordonanțate**, deci pot intra într-o ordonanțare nouă.

**Toate odată**: clic dreapta pe o **lună** › «Șterge TOATE ordonanțările lunii», sau pe rădăcina
«Toate ordonanțările» › «Șterge TOATE ordonanțările». K-BOT cere o singură confirmare, cu numărul și
totalul lor, apoi le șterge pe rând. Dacă una nu se poate șterge, se oprește acolo și spune câte
s-au șters până atunci; celelalte rămân.

O ordonanțare **semnată** (fie și cu o singură semnătură) nu se mai șterge: pe ea nu apare meniul,
iar pe o lună sau pe rădăcină ștergerea tuturor nu se oferă cât timp printre ele e una semnată.

După o ștergere reușită K-BOT nu mai arată niciun mesaj: ce s-a șters (beneficiari, rânduri,
documente, plăți eliberate) se scrie în jurnalul de mesaje — **Setări › Jurnal**.

După orice salvare sau ștergere, vederile «Ordonanțare» și «Plăți» se reîncarcă singure.
