# SLICE-0116 - Registrul de machete si istoricul «Machete deschise» (13.10.2026)

Construit si probat in `KBOT_TEST`, adus in `KBOT\src` la 13.10.2026. Istoria: `KBOT_TEST\MERGE_NOTES.md`
(sectiunile 1, 3c, 3d, 3e, 3i). Felie inregistrata dupa unire, ca lucru deja facut.

## Ce s-a schimbat si de ce
1. **Registrul, fara UI** (`KBot.Common/Machete/`): `MachetaRef` (tip + argumente, ex. `an`, `cod`),
   `MachetaType` (cheie, titlu, pictograma, functia de redeschidere, titlul dupa argumente), `MachetaRegistry`
   (`Register`, `OpenAsync`, `Note`, `NoteCount`, `IsRegistered`), `MachetaHistory` (10 intrari pe operator + unitate,
   evenimentul `Changed`).
2. **Decizie: istoricul e doar in sesiune.** `Program.vb` foloseste `MemoryMachetaHistoryStore`: lista «Machete
   deschise» se goleste la inchiderea K-BOT. Depozitul JSON (`%AppData%\AVACONT\KBot\Machete`) ramane in cod,
   nefolosit.
3. **O deschidere = o intrare**: `OpenAsync` nu mai inregistreaza a doua oara cand macheta s-a notat singura in
   timpul deschiderii; `ShowPage` noteaza si cand pagina e deja afisata (urca in istoric).
4. **Punctul unic de deschidere** din meniu / ribbon: `KbotForm.OpenRegistered(tip, actiune)` (fara registru ruleaza
   direct). Trec prin el: extrase, efactura_vanzare / efactura_cumparare, clasificatii, parteneri, mesaje, jurnal,
   setari, istoric versiuni, operatiuni necorelate, lista angajamentelor (`angajamente`), ajutorul ca pagina (`ajutor`).
   Raman in afara: ef_token, ef_primite_sync, unitate_conturi, unitate_date (dialoguri, nu machete).
5. **Tipurile inregistrate de lista angajamentelor** (`KbotForm.Machete.vb`): paginile si ferestrele de mai sus,
   editoarele si ferestrele unui angajament (Editor DDF, Editor ORD, Legaturile receptiilor, Graficul rezervarilor,
   Asociaza parteneri) si cate un tip pentru fiecare vedere din bara din stanga (`view.<cheie>`). Vederile se noteaza
   la deschidere (`NoteViewOpened`, din `KbotForm.Views.vb`).
6. **Redeschiderea** unei vederi sau a unui editor: aduce lista angajamentelor pe ecran, schimba anul daca macheta e
   din alt an (`ChangeYearAsync`, cu `await`), selecteaza angajamentul in arbore (`SelectAngajament`, `_treeNodes` in
   `KbotForm.Tree.vb`) si deschide vederea. Editoarele DDF / ORD se redeschid in vederea DDF / ORD a angajamentului
   (un draft nu se poate relua). Mesaje, sub titlul «Machete deschise»: anul nu mai e disponibil; angajamentul nu e in
   lista curenta; pagina nu e disponibila pentru acest angajament.
7. Machetele unui angajament nou nesalvat (cod cu «!») nu se inregistreaza (`KbotForm.Recordable`, `NoteViewOpened`).

## Fisiere
- NOU: `src/KBot.Common/Machete/MachetaRef.vb`, `MachetaHistory.vb`, `MachetaRegistry.vb`, `src/KBot.App/KbotForm.Machete.vb`.
- Modificate: `KBot.App/Program.vb` (DI `MachetaHistory` + `MachetaRegistry`), `KbotForm.Views.vb` (`NoteViewOpened`),
  `KbotForm.Tree.vb` (`_treeNodes`), `KbotForm.Nomenclatoare.vb` (`MenuNou_ItemClicked` prin `OpenRegistered`).

## Teste / verificare
- Build-ul unirii: 0 erori, 0 avertismente (operator). Nimic rulat in aceasta sesiune. Probat pe ecran in `KBOT_TEST`.

## Neverificat / amanat
- Redeschiderea din alt an / alt sector pe date reale cu multe angajamente: probata doar in `KBOT_TEST`.
- FileVersion `KBot.Common`, `KBot.App`: nebumpate.
- Ajutorul: 0000-71 (sectiunea «Machete deschise» a panoului lateral).
