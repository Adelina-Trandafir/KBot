---
id: tur-extrase
title: Fereastra «Extrase de cont»
part: contabil
topic: contabil.vederi.extrase
---
## Toate extrasele unității
<!-- slice: 0080-03 -->
target: ExtraseForm
goto: menu:extrase
Fereastra arată toate extrasele de cont ale unității, cu sau fără angajament. Vederea Extrase din fereastra principală arată doar operațiunile angajamentului selectat.

## Arborele
<!-- slice: 0096 -->
target: ExtrasePanel.tree
«Toate extrasele», apoi lunile și zilele. Urmează, pe rând, butoanele arborelui.

## Arbore › Lupa
<!-- slice: 0027, 0000-23 -->
target: ExtrasePanel.tree
part: header.search
Caută o lună sau o zi. Esc golește căutarea și închide banda.

## Arbore › Modul de afișare
<!-- slice: 0096, 0000-23 -->
target: ExtrasePanel.tree
part: header.right
Alege cum se văd datele: «antet + operații» (sus anteturile, jos operațiunile antetului ales) sau «operații + detalii» (sus operațiunile, jos operațiunea aleasă, întreagă).

## Arbore › Descarcă extrasele
<!-- slice: 0057, 0080-03, 0000-23 -->
target: ExtrasePanel.tree
part: footer.right
Descarcă extrasele de cont (SNM) din FOREXE; K-BOT se conectează întâi, dacă nu ești conectat.

## Grilele
<!-- slice: 0080-02, 0080-03 -->
target: ExtrasePanel.innerSplit
Sus: anteturile (sau operațiunile) perioadei alese. Jos: operațiunile antetului selectat (sau operațiunea selectată, întreagă). În modul operații + detalii, coloanele Data bancă, Plătitor și CUI se pot filtra și grupa din capul coloanei.

## Descarcă extrasele
<!-- slice: 0057, 0080-03 -->
target: ExtraseForm.btnDescarca
Descarcă extrasele din FOREXE; K-BOT se conectează întâi, dacă nu ești conectat. Ce coloane se văd alegi în Setări › Extrase.
