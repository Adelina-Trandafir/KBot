# KBOTADMIN – grile și Istoric MD (08.11.2026)

Fișă comună pentru lucrările din sesiune, de la SLICE-ME01-25 la SLICE-ME01-28.

## SLICE-ME01-25 – Meniurile grilelor și căutarea în Istoric MD

- Grilele KBOTADMIN folosesc `AdminDataView` pentru sortare și filtrare din antetul coloanelor de date, inclusiv în dialoguri și în grilele cu coloane create la rulare. Grupările existente au fost păstrate. Coloana «Rulează» din Scripturi este o comandă și rămâne buton.
- Ordinea textelor folosește limba română. Datele, numerele și bifele se compară după valorile lor, iar coloanele cu etichete compuse au chei de sortare explicite. În «Tabele», sortarea aleasă se păstrează la reîncărcarea aceleiași vederi. Meniul grilei lucrează cu rândurile deja încărcate.
- În «Noutăți», rândul gol pentru introducere rămâne disponibil la sfârșitul grupului și în timpul filtrării. În «Cereri», când filtrarea ascunde selecția, se golește fișa din dreapta și se dezactivează comenzile ei.
- În **Monitorizare → Istoric MD**, căutarea acceptă numele complet sau un fragment din numele fișierului, indiferent de majuscule. Cronologia păstrează barele cu cel puțin o potrivire, împreună cu marcajele acelor bare.
- Prima variantă evidenția legăturile potrivite în Graf și le lăsa gri pe celelalte. La cererea Adelinei, varianta finală ascunde restul. Rămân fișierele găsite, legăturile lor directe și nodurile necesare ca să se vadă ambele capete. Filtrul de familie și data cursorului se aplică în continuare. Golirea căutării restabilește vederea completă pentru familia și data alese.

### Fișiere

- `KBotAdmin/Core/AdminDataView.vb`
- În `KBotAdmin/Dialogs/`, `RecordHistoryForm.vb`, `RecordHistoryForm.Designer.vb` și `RightsForm.Designer.vb`.
- În `KBotAdmin/Views/`, codul și designerul pentru `AuditView`, `BackupsView`, `DbBrowserView`, `LocalBackupView`, `LoginResetView`, `MfImportView`, `OnceQueryView`, `PushFilesView`, `ReleaseNotesView`, `RequestsView`, `ScriptsView`, `SourceQueryView`, `VisitsView` și `MdHistoryView`. Au mai fost modificate `DiffView.Designer.vb`, `RightsView.Designer.vb`, `InventoryCompareView.vb` și `InventoryCompareView.Export.vb`.
- În `src/KBot.Controls/DataView/`, `KBotDataColumn.vb`, `KBotDataRow.vb`, `KBotDataView.vb` și partialele `CheckBoxes`, `Editing`, `Filtering`, `Input` și `Painting`. În `src/KBot.Controls/Graph/`, `KBotGraphView.vb` și `KBotGraphView.Painting.vb`.

## SLICE-ME01-26 – Cititorul Markdown

- Dublul clic pe un punct din Cronologie sau pe un nod din Graf deschide documentul într-o fereastră KBOTADMIN cu un control dedicat și un `WebBrowser`.
- Markdig transformă fișierul în HTML. Ideea vine din cititorul de ajutor ForexeBot. Foaia de stil folosește tema aplicației și formatează titlurile, tabelele, listele, citatele și blocurile de cod. Cititorul afișează imaginile locale și interpretează marcajele pentru capturile din ajutorul KBOT.

Fișierele principale sunt `KBotAdmin/Dialogs/MdReaderForm.vb`, `KBotAdmin/Views/MdReaderControl.vb` și `KBotAdmin/Corpus/MdReaderHtml.vb`. Integrarea dublului clic este în `KBotAdmin/Views/MdHistoryView.vb` și `src/KBot.Controls/Lane/KBotLaneView.Painting.vb`. Referința la Markdig 0.45.0 este în `KBotAdmin/KBotAdmin.vbproj`.

## SLICE-ME01-27 – Zoomul cititorului

Bara cititorului are butoanele **− / +**, un glisor și procentul curent. Din VERCON online au fost preluate intervalul **75–150%** și opririle la **90%, 100%, 110% și 125%**. Butoanele schimbă zoomul cu câte 10 puncte procentuale. La tragerea glisorului se actualizează procentul, iar documentul se redimensionează la eliberare. Zoomul se aplică și documentelor deschise prin linkuri și se păstrează la redeschiderea cititorului în aceeași sesiune KBOTADMIN.

Fișierele modificate sunt `KBotAdmin/Dialogs/MdReaderForm.vb` și `KBotAdmin/Views/MdReaderControl.vb`.

## SLICE-ME01-28 – Navigarea prin linkuri

Adelina a semnalat că linkurile erau vizibile în HTML, dar clicul nu deschidea documentul. Cititorul preia acum clicul pe legăturile locale și încarcă direct fișierul Markdown în aceeași fereastră. Rezolvă căile relative, căile de la rădăcina proiectului și legăturile `topic:` din ajutor. O ancoră din link poziționează documentul la secțiunea indicată. Butonul **Înapoi** revine la documentul anterior.

Fișierele modificate sunt `KBotAdmin/Corpus/MdReaderHtml.vb` și `KBotAdmin/Views/MdReaderControl.vb`.

## Stare

Buildul și testele nu au fost rulate în această sesiune. Corecția navigării prin linkuri a rămas fără o probă confirmată pe ecran. Înregistrarea fișierelor pentru publicare așteaptă confirmarea explicită a Adelinei.
