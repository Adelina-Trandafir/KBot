# SLICE-0115 - Pagina «Acasa» si panoul lateral al ferestrei principale (13.10.2026)

Construit si probat in `KBOT_TEST`, adus in `KBOT\src` la 13.10.2026. Istoria: `KBOT_TEST\MERGE_NOTES.md`
(sectiunile 1, 3b, 3d, 3g). Felie inregistrata dupa unire, ca lucru deja facut. Fereastra: 0113; randurile
«Machete deschise»: 0116; randurile «Actiuni recente»: 0117.

## Ce s-a schimbat si de ce
1. **Pagina de start `AcasaPage`** (o pagina a lui `AvacontMain`, tinuta incarcata, doar aratata / ascunsa):
   - cardul **«Ce este nou»**, in coltul din dreapta-sus: ultimele versiuni din `NOUTATI.md`
     (`ReleaseNotesText.RecentVersions`), doar de citit (fara cursor, selectie, focus; rotita deruleaza).
     Butonul cu sageata il strange / desface (`AppSettings.HomeNewsCollapsed`). **Dupa fiecare actualizare** cardul
     se desface singur o data (`AppSettings.NewsSeenVersion`), apoi decide butonul;
   - **logo-ul nou** K-BOT in coltul din dreapta-jos (`Resources/kbot_logo.png`, 256 px; tot el e iconita butonului
     «Lista angajamentelor» din ribbon);
   - bannerul albastru cu unitatea (din primele runde) a fost scos.
   - Fara timere: pagina se citeste din nou la fiecare aducere pe ecran (`RefreshState`).
2. **Panoul lateral** (`AvacontMain.Sidebar.vb`, controlul nou `KBotSidePanel`): sta langa orice pagina, nu doar
   langa «Acasa». Sectiuni pliabile (clic pe titlu), declarate in Designer:
   - **Machete deschise** (0116): istoricul registrului de machete, doar din sesiunea curenta; clic pe un rand o
     deschide din nou; randurile nu au ora;
   - **Actiuni recente** (0117): scrierile pe server ale operatorului;
   - **Informatii** (randuri pe doua linii): Operator, Rol, Baza si perioada, FOREXE (Conectat / Neconectat, cu
     certificatul; verde / portocaliu), Activitate robot, Token E-Factura (starea tokenului ANAF, cu culoare dupa
     zilele ramase), Versiunea K-BOT;
   - **Erori din sesiune**: ultimele 10 erori ale rularii curente (`GlobalErrorLog.RecentErrors`, evenimentul
     `Written`, clasa `SessionErrorEntry`); clic pe una arata detaliul intr-o caseta.
   Panoul se strange la o banda ingusta cu pictogramele sectiunilor (butonul din bara lui de sus; clic pe o
   pictograma il desface; `AppSettings.HomeSidebarCollapsed`); latimea desfacuta 203 (Designer). Rotita deruleaza
   cand sectiunile nu incap. Nimic nu se interogheaza periodic: robotul prin evenimentele controlerului, erorile prin
   `Written`, machetele prin `History.Changed`, tokenul la fiecare reimprospatare a panoului.
3. `ButtonStyles.ApplyTransparent` (KBot.Theming): buton fara corp, pentru o bara (butonul cardului «Ce este nou»).
   Singura schimbare adusa in `ButtonStyles.vb`; varianta portocalie de test NU (vezi 0114).

## Fisiere
- NOU: `src/KBot.App/AcasaPage.vb` + `.Designer.vb`, `src/KBot.App/AvacontMain.Sidebar.vb`,
  `src/KBot.Controls/SidePanel/KBotSidePanel.vb` + `KBotSidePanelItems.vb`, `src/KBot.App/Resources/kbot_logo.png`.
- Modificate: `KBot.Common/GlobalErrorLog.vb` (buffer in memorie; `Write` cheama `Remember`, nu arunca),
  `KBot.Common/AppSettings.vb` (`HomeSidebarCollapsed`, `HomeNewsCollapsed`, `NewsSeenVersion` + DTO nullabil,
  format compatibil cu cel vechi), `KBot.Theming/ButtonStyles.vb` (`ApplyTransparent`),
  `My Project/Resources.resx` + `Resources.Designer.vb` (`kbot_logo`), `AvacontMain.Designer.vb` (sectiunile panoului).

## Teste / verificare
- Build-ul unirii: 0 erori, 0 avertismente (operator). Nimic rulat in aceasta sesiune. Probat pe ecran in `KBOT_TEST`.

## Neverificat / amanat
- Comentariul din `AcasaPage.vb` spune inca «the same text as Setari -> Istoric versiuni» (istoricul e acum macheta,
  0118). Doar comentariu, neschimbat.
- `%AppData%\AVACONT` (setari) e comun cu versiunea veche; aceasta ignora cheile noi.
- FileVersion `KBot.Controls`, `KBot.Common`, `KBot.Theming`, `KBot.App`: nebumpate.
- Ajutorul: 0000-71 (impreuna cu 0116 si 0117).
