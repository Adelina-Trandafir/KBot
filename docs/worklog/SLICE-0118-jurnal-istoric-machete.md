# SLICE-0118 - «Jurnal activitate» si «Istoric versiuni» ca machete proprii (13.10.2026)

Reper ulterior: [0113-01](SLICE-0113-fereastra-principala-machete-pagini.md#slice-0113-01) păstrează paginile deschise când este arătată alta; formularea inițială „una câte una” de mai jos nu este a doua regulă actuală.

Construit si probat in `KBOT_TEST`, adus in `KBOT\src` la 13.10.2026. Istoria: `KBOT_TEST\MERGE_NOTES.md` (sectiunile 1, 3d, 3e). Felie inregistrata dupa unire, ca lucru deja facut.

## Ce s-a schimbat si de ce
1. **Jurnalul si istoricul versiunilor au iesit din fereastra de setari**: paginile «Jurnal» si «Istoric versiuni» (si separatorul de sub ele) scoase din `SetariForm.Designer.vb`, cazurile `jurnal` / `istoric` scoase din `SetariForm.CreateView`. `setari:jurnal` nu mai exista.
2. **Machete proprii**, pagini ale ferestrei principale (0113), una cate una:
   - `JurnalPage` («K-BOT — Jurnal activitate»): gazduieste `SetariJurnalView` (jurnalele aplicatiei si, cu optiunile avansate, ale serverului) + o eticheta de stare jos; deschisa din ribbon, Configurari › Configurare › «Jurnal activitate» (butonul urmeaza comutatorul din Setari › Aplicatie, ca randul vechi din MENIU);
   - `IstoricVersiuniPage` («K-BOT — Istoric versiuni»): gazduieste `SetariIstoricView` (ce a schimbat fiecare versiune, din `NOUTATI.md`); deschisa din ribbon, Configurari › Configurare › «Istoric versiuni».
   `KbotForm.Pagini.vb`: `DeschideJurnal`, `DeschideIstoricVersiuni`, `HasOpenWork`.
3. **Jurnalul**: ziua curenta e desfacuta cand zilele se strang (`KBotDataView.ExpandGroup`, nou in `KBotDataView.Grouping.vb`), iar intre grila si detaliu e un separator reglabil (`splitJurnal`, SplitContainer).

## Fisiere
NOU: `src/KBot.App/JurnalPage.vb` + `.Designer.vb`, `IstoricVersiuniPage.vb` + `.Designer.vb`, `KbotForm.Pagini.vb`. Modificate: `Setari/SetariForm.Designer.vb`, `Setari/SetariForm.vb`, `Setari/SetariJurnalView.vb` + `.Designer.vb`, `KBot.Controls/DataView/KBotDataView.Grouping.vb`.

## Teste / verificare
- Build-ul unirii: 0 erori, 0 avertismente (operator). Nimic rulat in aceasta sesiune. Probat pe ecran in `KBOT_TEST`.

## Neverificat / amanat
- Navigarea ajutorului: `setari:jurnal` (folosit de `avansat.jurnale` si de turul `tur-setari`) nu mai ajunge la tinta; nu exista inca un `goto:` pentru «Istoric versiuni» (vezi raportul din worklog-ul 0000-69, «Navigarea ajutorului»).
- FileVersion `KBot.Controls`, `KBot.App`: nebumpate.
- Ajutorul: 0000-72.
