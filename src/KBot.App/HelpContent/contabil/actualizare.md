---
id: contabil.actualizare
title: Actualizarea K-BOT
part: contabil
order: 90
parent: contabil
screens: UpdateProgressForm, SetariInfoView.btnActualizari
keywords: actualizare, versiune noua, update, cauta actualizari, descarcare, repornire
---
<!-- slice: 0067 -->
K-BOT se actualizează singur, de pe serverul K-BOT.

## La pornire
<!-- slice: 0067 -->

La fiecare pornire, înainte de conectare, K-BOT întreabă serverul dacă există o versiune mai nouă.

- **Nu există** — pornește ca de obicei, fără niciun mesaj.
- **Există o versiune nouă** — K-BOT îți arată versiunea ta, versiunea nouă, ce aduce și cât are
  de descărcat. **«Da»** o descarcă, închide K-BOT, îl actualizează și îl pornește din nou.
  **«Nu»** = mai târziu; lucrezi mai departe pe versiunea de acum.
- **Versiunea ta nu mai poate fi folosită** (actualizare obligatorie) — **OK** actualizează;
  **Anulare** închide K-BOT, fiindcă fără actualizare nu poate continua.

Dacă serverul nu răspunde (fără internet, de exemplu), K-BOT pornește oricum; verificarea se face
la pornirea următoare.

## Oricând: «Caută actualizări»
<!-- slice: 0067, 0072 -->

În **Setări › Informații**, butonul **«Caută actualizări»** face aceeași verificare pe loc și
spune de fiecare dată rezultatul, inclusiv «Aveți ultima versiune».

## Descărcarea
<!-- slice: 0067, 0067-01 -->

Cât se descarcă, fereastra **«Actualizare K-BOT»** arată progresul.

<!-- capture: actualizare | caption: Fereastra «Actualizare K-BOT» în timpul descărcării | prepare: Apare doar când există o versiune nouă; fotografiați-o la prima actualizare. -->

- **«Renunță»** oprește descărcarea. Actualizarea se poate relua oricând.
- După descărcare, K-BOT se închide, programul de actualizare pune fișierele noi și K-BOT pornește
  din nou. Jurnalele tale și setările rămân neatinse.

Dacă apare «Lipsește ... din folderul aplicației», instalează o dată versiunea nouă din pachetul
de instalare; de acolo încolo actualizările se fac singure.
