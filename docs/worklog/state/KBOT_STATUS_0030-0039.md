# K-BOT — STATUS, slices 0030–0039

Moved verbatim out of `../KBOT_STATUS.md` (28.09.2026). The index there says what
each slice is; this file holds everything recorded about it: its registry row, its
«Current focus» notes and its «Open threads» notes.

---

## Slice 0030

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0030 | Meniul de coloană: TREI FILE (Sortare / Filtrare / Grupare), rânduri de tabel pe măsura temei, și bifele care se pierduseră | DONE (code) / **niciodată văzut pe ecran** | `SLICE-0030-filter-popup-file-si-grupare.md` | Patru cereri de operator după ce a rearanjat singur `KBotFilterPopup` cu un `TableLayoutPanel`. **(a) Filtrarea „nu mai mergea”, arăta o grilă goală:** `LstValori_ItemCheck` rămăsese fără clauza `Handles`, deci bifele puse cu mouse-ul nu ajungeau în model — pe drumul obișnuit («Selectează tot» stinge tot, apoi se bifează una) mulțimea predată la OK era GOALĂ; «Șterge filtrul» mergea fiindcă nu trece prin bife. **(b)** `pnlCorp` nu era andocat (Location/Size scrise de designer), deci nu urma `ClientSize`; iar re-măsurarea citea `lstValori.Height` înainte de orice layout. Acum `Dock = Fill` + o singură formulă pentru toate filele (rama corpului + cât cere fila activă), plus o re-verificare a poziției DUPĂ `Show()`, fiindcă tema se aplică abia în `OnLoad`. **(c)** `ThemeTableFit` (nou, în `KBot.Theming`): un rând fix crește cu EXACT surplusul cerut peste măsura AUTORATĂ și se întoarce la ea când surplusul dispare — nu «rândul devine cât conținutul», altfel aerul ales de operator s-ar pierde la prima comutare. Trei lucruri ieșite din implementare: baza e un INSTANTANEU (fără el, a doua comutare măsoară peste rezultatul primeia și rândurile cresc la nesfârșit); se întreabă `GetPreferredSize`, **NU `Height`**, fiindcă `TableLayoutPanel` taie orice control andocat la dreptunghiul celulei — un buton de 56px într-o celulă de 40 raportează 40, deci creșterea lui `ModernRenderer` e invizibilă (prima versiune a testului a picat exact pe asta); și `Control.Visible` **nu poate fi întrebat de nimeni** (răspunde despre lanțul de părinți, deci headless tot raportează False), de unde `SetRowCollapsed` explicit pentru rândul butonului de condiții și fila curentă ținută într-un câmp, nu în `pnlX.Visible`. `KBotTextField` a primit un `GetPreferredSize` cinstit. **(d)** Bară `KBotNavList` orizontală + trei panouri `Fill`; **bara OK/Anulează e a FILTRULUI** și se ascunde pe celelalte file, fiindcă cele trei hotărâri se predau diferit: filtrul la OK (închide), sortarea imediat (închide), **gruparea imediat dar meniul RĂMÂNE deschis** — are șapte opțiuni, o filă care s-ar închide la prima bifă ar trebui redeschisă de șase ori. Fila de grupare arată opțiunile de nivel 0029 pentru COLOANA aceasta + ierarhia grilei cu coloana curentă marcată, toate citite din `Groups` la deschidere (controalele sunt ale designerului, textele nu — aceeași împărțire ca lista de valori de la 0028-06); nivelul EXISTENT se refolosește (culorile/fonturile lui de designer supraviețuiesc) și se înlocuiește pe LOCUL LUI din ierarhie. `KBotDataView.EnableGrouping` (implicit STINS) gatează doar fila, nu `Groups` autorate. Pe drum: `btnSortClear` era un buton MORT (fără handler); liniile despărțitoare erau `Label`-uri, deci deveneau invizibile sub regula generică `Label → Transparent` (sunt `Panel`-uri acum); fonturile «Segoe UI, 9» pinuite pe rândurile de meniu au fost ȘTERSE, nu rescrise (ca la 0027-02). Build 0 erori; `Controls.Tests` **808** (800 înainte, **+8**). ⚠️ **Deschis:** nimic pe ecran, iar tocmai ce s-a cerut se judecă privind (bara de file în cele patru scheme, dacă rândurile crescute chiar rezolvă ce se vedea, saltul ferestrei între file); dus-întorsul prin designerul VS nefăcut |

### Current focus

- **Slice 0030 (meniul de coloană: trei file + rânduri pe măsura temei) — DONE (code); nevăzut pe ecran.**
  Trei lucruri merită scoase din ea. (1) **O clauză `Handles` pierdută la o redenumire nu arată ca
  un defect de cod, arată ca un defect de DATE** — meniul se desena corect, bifele se puneau
  corect, și totuși grila ieșea goală, fiindcă modelul nu afla niciodată de ele. Comentariul de
  deasupra lui `LstValori_ItemCheck` spune acum ce se rupe fără clauză; e singura apărare pe care o
  are o legătură care nu se vede în corpul metodei. (2) **`ModernRenderer` crește butoanele, dar
  într-un `TableLayoutPanel` creșterea aia nu se vede niciodată** — motorul de așezare taie orice
  control andocat la dreptunghiul celulei, deci un buton de 56px într-o celulă de 40 raportează 40.
  De aceea `ThemeTableFit` întreabă `GetPreferredSize`, nu `Height`; și de aceea rândurile autorate
  în designer (care nu știe de teme) trebuiau să capete pe cineva care le crește cu exact surplusul
  cerut, dintr-un instantaneu al valorii AUTORATE — altfel a doua comutare de schemă măsoară peste
  rezultatul primeia. (3) **`Control.Visible` nu poate fi întrebat de nimeni**: getter-ul răspunde
  despre lanțul de părinți, deci pe un formular încă nearătat totul raportează `False`. Capcana a
  mușcat de două ori în aceeași felie (măsurarea rândurilor, apoi fila curentă) și a fost rezolvată
  la fel de amândouă dățile — starea se ține explicit, nu se deduce dintr-un control.
  **Next:** aceeași datorie ca tot 0028–0029 — nimic n-a fost pe ecran, iar aici se judecă privind
  exact lucrurile pentru care a fost cerută felia (bara de file în cele patru scheme, dacă
  rândurile crescute sub Modern chiar rezolvă ce se vedea, saltul ferestrei la schimbarea filei).

---

## Slice 0031

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0031-01 | Nucleul jurnalizării: căi, rotație, analiză, filtre | DONE (code) / **fără UI în trecerea asta** | [0031-01](../SLICE-0031-01-log-core.md#slice-0031-01) | Contractele, abaterile, probele și limitele sunt în etapa legată. |
| 0031-03 | KBotChipBar — control de selecție multiplă | DONE (code) / văzută pe ecran DOAR în `LogViewerForm` | [0031-03](../SLICE-0031-03-chipbar.md) | Contractele, abaterile, probele și limitele sunt în etapa legată. |
| 0031-04 | Vizualizator, golire locală, cablare și launcher | Documentat în jurnal: cod/probe locale; confirmare separată a operatorului înainte de reparațiile de plasare; restanțe vizuale și server păstrate | [0031-04](../SLICE-0031-01-log-core.md#slice-0031-04) | Rând de navigare completat în D001-28; nu certifică o rulare nouă. |

### Open threads

- [01/04](../SLICE-0031-01-log-core.md) și [03](../SLICE-0031-03-chipbar.md) păstrează limitele nucleului, trecerea prin designer și probele vizuale. 02 era încă nelivrat la reperul acestor jurnale; [planul](../../PLAN_LogViewer.md) este specificația istorică, iar 0089 documentează ulterior jurnalele pe utilizator/sesiune.

---
## Slice 0033

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0033 | Vederea ORD (Ordonanțări), read-only + ruta nouă `GET /api/forexe/ord` | DONE (code) / **niciodată văzută pe ecran, ruta niciodată pe o bază vie** | `SLICE-0033-ord-view.md` | Ultima dintre cele trei vederi reale rămase — `MainForm.CreateView("ord")` nu mai întoarce `PlaceholderView`. Forma sub-vederilor DDF: arbore lună → ordonanțare în `split.Panel1`, `navSub` orizontal, două sub-pagini leneșe editabile în designer, hrănite de părinte (fără DI ⇒ constructor fără parametri ⇒ se deschid în designerul VS): «Vizualizare» (grilă cu ȘASE coloane autorite) și «Document» (PDF real, montare leneșă cu gardă pe perechea `(cale, existență)`). **Ruta**: un singur drum dus-întors, două tablouri (`ordonantari` + `linii`); clientul filtrează local pe `IDORDP`. Cele trei capcane `FX_ORD` tratate explicit — legăturile merg pe cheile «…P» (nu pe id-urile Access omonime), `ClasificatiiG` nu se atinge, iar inversiunea `IdClsf` (aici `IdClsf` = FK `Clasificatii`, `IdClsfAcc` = id Access — invers față de `FX_Indicatori`) e respectată. Filtrul e DIRECT pe `FX_ORD.CodAngajament` (scris cu `_strict_str_nonempty` de ambele căi de migrare), nu prin ocolul Access `FX_ORD.IDDF = FX_DDF.IDDF`, care ar fi adus și fan-out (PK compus). `total_ord` = subinterogare scalară, deci mai mulți beneficiari nu-l pot umfla (familia `aggOrd`). **Clasificația: AMÂNDOUĂ drumurile într-un `COALESCE`** — planul cerea alegerea după o probă pe date reale, proba n-a fost posibilă, deci se încearcă directul (documentat de sync-ul ORD, identic cu `ddf.py`) și se cade pe cel VERIFICAT în 0011-03 (`FX_Indicatori` → `IdClsfAcc + IdUnitate`); a ghici unul reintra fix în capcana «Clsf gol în producție». **`pdf` — abatere motivată:** coloana se numește `CalePDF` (nu `PDF`) și nu e scrisă de nicio rută de migrare, ca cele patru coloane PDF ale DDF-ului; se PROBEAZĂ o dată prin `information_schema` (memorat per bază) și se trimite doar dacă există — un `SELECT` orb ar fi dat 500 pe toată ruta. E oricum doar un SEMNAL: existența se decide printr-o probă pe discul clientului. Convenția PDF **citită din VBA**, nu ghicită (`mdl_FX_ORD_PDF`): `<root>\<partener\|GENERAL>\ORD_NR_{NrORD}_{Cod}.PDF`, partenerul din `FX_DDF` prin `IDDF` (de unde `part_ang`/`nume_partener` pe fir); ciudățenia Access care produce nume `ORD_NR_0_…` e documentată, dar **nu reprodusă**. Pagina «Document» NU folosește suprafața „document lipsă" a lui `ReaderHostPreview`: aceea poartă un buton «Generează», iar generarea ORD e o felie ulterioară. Build 0 erori; **10 avertismente `MSB3825`** (8 preexistente + 2 de la `OrdView.resx` — aceeași familie structurală ca orice vedere cu `ImageList` autorit), deci **nu «0 avertismente»** cum cerea planul. `pytest` 75 trecute / 15 sărite (neschimbat); ruta se înregistrează (`['/api/forexe/ord']`, cu un `config.py` de umplutură — config-ul real lipsește de pe mașină). **Fără teste noi** (planul §11); cele șapte dubluri de `IApiClient` din `KBot.App.Tests` au primit doar ciotul cerut de contract. ⚠️ **Deschis:** (1) ruta n-a atins niciodată o bază vie — care ramură a `COALESCE`-ului câștige, dacă `CalePDF` mai există, dacă `information_schema` e citibil cu utilizatorul aplicației; (2) nimic randat pe ecran, dus-întorsul prin designerul VS e al operatorului; (3) calea PDF neconfirmată pe un disc real; (4) iconițele pornesc de la setul lui `RezervariView` (`neutral` = glifa «=»), de schimbat din designer; (5) amânate deliberat: generarea, Atașamente/Documente/Fișiere, gruparea pe beneficiar, bifele de selecție multiplă |
| 0033-02 | Arborele `DdfView` adus la paritate cu cel din `RezervariView` (iconițe din `ImageList` + dimensiuni) | DONE (code) / **niciodată văzut pe ecran** | `SLICE-0033-02-ddf-tree-parity.md` | Cerere de operator, rulată în felia 0033. `DdfView` desena TOATE iconițele cu formele GDI din `DdfIcons`, deși `tree_image_list` exista de mult în designer și era legat prin `tree.NodeImages` — pozele erau acolo, nimeni nu le cerea. Acum regula listă-întâi din `RezervariView.TipIconOf`: se caută cheia (`folder_closed`/`folder_open`/`up`/`down`/`neutral`), fallback GDI colorat din paletă doar dacă lipsește. Câștig secundar: luna are în sfârșit două poze distincte (închis/deschis) — înainte ambele stări primeau aceeași imagine. `ItemHeight` 24 → 30, singura dimensiune care mai diferea (fontul și dimensiunile iconițelor erau deja aliniate de 0027-03). Structura, cheile, etichetele, valorile, tooltip-urile și comportamentul arborelui: NEATINSE. ⚠️ **Deschis:** nerandat pe ecran; cheia `neutral` **nu e** încă în `tree_image_list`, deci reviziile neutre rămân pe forma GDI până când operatorul pune poza (nu e regresie — e fallback-ul) |

---

## Slice 0034

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0034 | Descărcarea FOREXE: bandă de subsol + consolă + descărcări din arbore | DONE (code) / **niciodată rulat pe FOREXE viu, nicio suprafață văzută pe ecran** | `SLICE-0034-forexe-footer.md` | Prima suprafață prin care operatorul chiar vede și comandă robotul. `ForexeController` (singleton DI) devine **singurul** care vorbește cu `IForexeRunner`; `MainForm` pierde `EnsureForexeSessionAsync` / `SelectCertificate` / `HasLiveForexeSession` / `UpdateForexeStatus`. Două suprafețe PROASTE legate la el: `ForexeFooterView` (UserControl `IThemedControl`, în `pnlStatus`, înlocuiește `lblProgram` și înghite `lblForexe`) și `ForexeConsoleForm` (`KBotShellForm` redimensionabilă: jurnal complet, progres, stare, Anulează, Arată browserul, Deschide jurnalul). **Descărcările se comandă din ARBORE**, nu din bandă: subsol-stânga = lista, iconița din dreapta unui nod = angajamentul acela. **Tot ce se descarcă rămâne LOCAL** (memorie + JSON cu diacritice literale în `<AppDir>\WorkflowResults\`); singura scriere pe server rămâne vechiul upsert al listei, mutat pe rândul «&Sincronizare (server)» din meniul butonului de opțiuni — `btnSinc` a dispărut din subsol. **Cea mai mare corecție față de plan:** cele două `.wfl` de prelucrare (puse de operator după raportul pasului 0) NU produc un tabel `saveTo`, ci **cinci** (`TabelIndicatori`, `BugetIndicator`, `ListaReceptii`, `Detaliu`, `TabelIstoric`) plus nouă scalari `<Read>` — deci sinkul salvează întreg `JobResult`-ul, cu cheile devenite tabele filtrate din `Data` (altfel fiecare tabel s-ar scrie de două ori, o dată structurat și o dată ca șirul JSON brut). Înainte-vs-înapoi e **citit** din Access `mdl_FX_Tasks_Send.FX_Angajament_InfoComplete` (`DMax("DataFX","FX_Istoric",…)`), inclusiv formatul `dd/MM/yyyy HH:mm:ss` pe care planul îl dădea doar ca fallback — scris cu `InvariantCulture`, fiindcă valoarea intră într-o expresie regulată comparată cu textul din pagină. Echivalentul K-BOT al lui `DMax`: `GetIstoricAsync` → `MAX(DataFx)`, prin `WithReauth`. **`KBot.Controls` a primit `FooterLeftIconClicked`** — planul presupunea că iconița din stânga subsolului e deja un buton; **nu era** (se desena, dar fără dreptunghi, survolare sau eveniment, spre deosebire de sora din dreapta). Adăugat pur aditiv, oglindind exact fratele existent. **Strângerea arborelui a dispărut din shell** (`tree_CollapsedChanged` + `ClampSplitter` șterse; `FooterCollapseButton` nici nu era pornit, deci colțul era deja liber) — arborii din VEDERI o păstrează. Jurnalul: `RichTextBoxLogger` n-are buffer public nici eveniment pe linie, deci consola se construiește **o dată, în `MainForm_Load`**, înaintea logger-ului, și închiderea o ASCUNDE (altfel logger-ul ar rămâne cu un control eliberat). `IForexeRunner` a primit `StatusUpdated`, `HasLiveSession` și `ShowBrowserAsync`. Build Debug **0/0**, Release 0 erori / 5 avertismente `MSB3825` **preexistente**; `Controls.Tests` **831** verzi; `App.Tests` 155/6 — mulțime de eșecuri **IDENTICĂ** cu baza (verificat prin `git stash`). **Fără teste noi.** ⚠️ **Deschis:** (1) nimic n-a atins FOREXE-ul viu — nicio sesiune, niciun certificat, niciun workflow rulat; (2) nicio suprafață randată, dus-întorsul prin designerul VS e al operatorului (riscul standing 0025/0027); (3) JSON-ul nu s-a scris niciodată pe disc real; (4) **nu există mapper de ingestie** pentru «Prelucrare Completa» — rezultatul se ține BRUT, deliberat, până când operatorul vede coloanele reale; (5) calea REVERSE e rar exercitată cât timp descărcările rămân locale (istoricul local reflectă doar ce era deja în bază); (6) `{{DATA_IESIRE}}` nu e documentat în antetul propriu al fișierului Reverse, iar `collectFields` pentru recepții DIFERĂ între cele două `.wfl` (varianta completă omite `TipReceptie`/`CodIndicator`) — nereparat, nu e al feliei; (7) iconița de subsol e `database`, aleasă din ce exista — nu e o glifă de descărcare. **Trecerea operatorului peste bandă** (după prima livrare) a adus controlul nou **`KBotProgressBar`** (`KBot.Controls/Progress/`), fratele DETERMINAT al lui `KBotBusyBar`: `System.Windows.Forms.ProgressBar` e desenat de Windows, deci tema nu-l poate atinge — rămânea verde pe schemă întunecată. Regulile casei cap-coadă (`Color.Empty` = din temă + `ShouldSerialize`/`Reset` pe cele trei culori; `Font` cu perechea lui, fiindcă nu poate purta `<DefaultValue>`), iar `Value` **prinde** valorile din afara intervalului în loc să arunce (sursa e `IProgress(Of Integer)` de pe firul robotului — o excepție acolo ar opri descărcarea din cauza unei bare de progres). **Capcana `IThemedControl` a lovit a treia oară:** banda e ea însăși `IThemedControl`, deci `ThemeManager` NU recurge în copiii ei și bara ar fi rămas pe culorile implicite — `ApplyTheme` o împinge explicit mai departe. În bandă, `pbProgress` se vede doar cât rulează ceva și `lblCert` doar după alegerea unui certificat (andocate la stânga ⇒ ascunse nu lasă gol); steagurile pornesc din CONSTRUCTOR, nu din designer, ca suprafața de proiectare să rămână editabilă. Consola a primit același control (doar tipul), ca să nu existe o bară tematizată și una nu pentru același progres. **Cele două `.Designer.vb` fuseseră între timp deschise în designerul VS și re-serializate — deci dus-întorsul s-a făcut și au supraviețuit.** ⚠️ `KBotProgressBar` **n-a fost privit niciodată pe ecran** și nu are teste |

---

## Slice 0035

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0035 | `KBotToolTip` (control nou) + scalarea la DPI a arborelui și a grilei + etichete în română | DONE (code) / **nicio suprafață văzută pe ecran** | [0035](../SLICE-0035-tooltip-control-dpi.md) | Contractele, raționamentul, probele și limitele sunt în jurnal; starea este a etapei istorice. |

---

## Slice 0036

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0036 | Opțiunile temei într-o fereastră proprie + scalarea reglabilă | DONE (code) / **nicio suprafață văzută pe ecran, `DpiUnaware` niciodată pornit** | [0036](../SLICE-0036-theme-options-scaling.md#slice-0036) | Contractele, raționamentul, probele și limitele sunt în jurnal; starea este a etapei istorice. |
| 0036-01 | Mărimea textului și a controalelor (cursor în meniu + în opțiuni) | DONE (code) / **nicio suprafață văzută pe ecran** | [0036-01](../SLICE-0036-theme-options-scaling.md#slice-0036-01) | Contractele, raționamentul, probele și limitele sunt în jurnal; starea este a etapei istorice. |
| 0036-02 | Cursorul aplică la SFÂRȘITUL gestului, nu la fiecare pas | DONE (code) / **nevăzut pe ecran; diagnosticul vine din raportul operatorului + citirea codului** | [0036-02](../SLICE-0036-theme-options-scaling.md#slice-0036-02) | Contractele, raționamentul, probele și limitele sunt în jurnal; starea este a etapei istorice. |

---

## Slice 0037

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0037 | `KBotDataView` — coloana care se întinde poate fi ORICARE (`SpecificColumn` + `FillColumnKey`) | DONE (code) / **nevăzut pe ecran, FĂRĂ TESTE (cerute sărite)** | [0037](../SLICE-0037-dataview-fill-specific-column.md) | Contractele, raționamentul, probele și limitele sunt în jurnal; starea este a etapei istorice. |
