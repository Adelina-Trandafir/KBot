# K-BOT — STATUS, slices 0020–0029

Moved verbatim out of `../KBOT_STATUS.md` (28.09.2026). The index there says what
each slice is; this file holds everything recorded about it: its registry row, its
«Current focus» notes and its «Open threads» notes.

---

## Slice 0020

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0020-01 | DDF — endpoint `GET /api/forexe/ddf` + client `GetDdfAsync` | DONE (code) / **NEVER RUN LIVE** | [0020-01](../SLICE-0020-01-ddf-endpoint.md#slice-0020-01) | Contractul, deviațiile și probele sunt în etapa legată; variantele ulterioare sunt delimitate cronologic. |
| 0020-02 | DDF — `DdfView` (arbore + sub-navigare orizontală + grilă + combo clasificații) | DONE (code) / **NO VISUAL VERDICT** / **NEVER RUN LIVE** | [0020-02](../SLICE-0020-01-ddf-endpoint.md#slice-0020-02) | Contractul, deviațiile și probele sunt în etapa legată; variantele ulterioare sunt delimitate cronologic. |
| 0020-03 | DDF — două suprafețe de previzualizare (`XfaXmlPreview` implicit + `ReaderHostPreview` rezervă) | DONE (code) / **NO VISUAL VERDICT** / **ReaderHost NERULAT** | [0020-03](../SLICE-0020-01-ddf-endpoint.md#slice-0020-03) | Contractul, deviațiile și probele sunt în etapa legată; variantele ulterioare sunt delimitate cronologic. |
| 0020-04 | DDF — `KBotPaths` + browser de fișiere + legare încrucișată frunză↔previzualizare | DONE (code) / **NO VISUAL VERDICT** | [0020-04](../SLICE-0020-01-ddf-endpoint.md#slice-0020-04) | Contractul, deviațiile și probele sunt în etapa legată; variantele ulterioare sunt delimitate cronologic. |
| 0020-05 | DDF — generarea PDF (părțile ne-blocate) + `DdfXmlBuilder` + pasul 05-00 (client) | DONE (code) / **NEVER RUN LIVE** / **NO PDF VERDICT** | [0020-05](../SLICE-0020-01-ddf-endpoint.md#slice-0020-05) | Contractul, deviațiile și probele sunt în etapa legată; variantele ulterioare sunt delimitate cronologic. |
| 0020-01 | `c0eab3a` | [Endpoint și client](../SLICE-0020-01-ddf-endpoint.md) | [0020-06](../SLICE-0020-01-ddf-endpoint.md#undefined) | Contractul, deviațiile și probele sunt în etapa legată; variantele ulterioare sunt delimitate cronologic. |
| 0020-02 | `9531adc` | [DdfView](../SLICE-0020-02-ddf-view.md) | [0020-07](../SLICE-0020-01-ddf-endpoint.md#undefined) | Contractul, deviațiile și probele sunt în etapa legată; variantele ulterioare sunt delimitate cronologic. |
| 0020-03 | `cff715a` | [Previzualizări și revizuirea «Document»](../SLICE-0020-03-ddf-preview.md) | [0020-08](../SLICE-0020-01-ddf-endpoint.md#undefined) | Contractul, deviațiile și probele sunt în etapa legată; variantele ulterioare sunt delimitate cronologic. |
| 0020-04 | `df99591` | [Căi, browser și legarea previzualizării](../SLICE-0020-04-ddf-file-browser.md) | [0020-09](../SLICE-0020-01-ddf-endpoint.md#undefined) | Contractul, deviațiile și probele sunt în etapa legată; variantele ulterioare sunt delimitate cronologic. |
| 0020-05 | `628e80f` | [Generarea PDF](../SLICE-0020-05-ddf-generate.md) | [0020-010](../SLICE-0020-01-ddf-endpoint.md#undefined) | Contractul, deviațiile și probele sunt în etapa legată; variantele ulterioare sunt delimitate cronologic. |

### Rezumat istoric de migrare — 24.07.2026

Reper preluat din rezumatul separat retras în D001-03; nu certifică starea actuală. Cele cinci implementări și limitele lor sunt descrise în worklogurile din Registry.

| Pas | Commit consemnat în rezumat | Detalii |
|---|---|---|
| 0020-01 | `c0eab3a` | [Endpoint și client](../SLICE-0020-01-ddf-endpoint.md) |
| 0020-02 | `9531adc` | [DdfView](../SLICE-0020-01-ddf-endpoint.md#slice-0020-02) |
| 0020-03 | `cff715a` | [Previzualizări și revizuirea «Document»](../SLICE-0020-01-ddf-endpoint.md#slice-0020-03) |
| 0020-04 | `df99591` | [Căi, browser și legarea previzualizării](../SLICE-0020-01-ddf-endpoint.md#slice-0020-04) |
| 0020-05 | `628e80f` | [Generarea PDF](../SLICE-0020-01-ddf-endpoint.md#slice-0020-05) |

Actualizarea documentară a statusului era consemnată separat ca `40b1c40`. Rezumatul din 24.07.2026 relua bilanțul pasului 0020-05 (415 teste .NET și 75/14 Python), anterior bilanțului de 436 din revizuirea previzualizării. Limitele și probele rămân în workloguri: rezumatul nu aducea un verdict vizual, PDF real sau rulare live.

### Current focus

- Etapele 01…05 construite; revizuirea din 24.07.2026 este în [03](../SLICE-0020-01-ddf-endpoint.md#slice-0020-03). Probe, avertismente și diferențe de formulare ale vechiului focus: [istoricul reunit](../SLICE-0020-01-ddf-endpoint.md).

### Open threads

- [01/02](../SLICE-0020-01-ddf-endpoint.md#slice-0020-01): rulare live, antete multiple și verdict vizual; [03](../SLICE-0020-01-ddf-endpoint.md#slice-0020-03): mostră DDF autentică, găzduire Adobe/focus; [04](../SLICE-0020-01-ddf-endpoint.md#slice-0020-04): căi/fișiere reale și singleton; [05](../SLICE-0020-01-ddf-endpoint.md#slice-0020-05): GET/HEAD peste 443 și cache/nginx, PDF real, diff XML/base64, progres și amânarea semnării. Eliminarea cheii API rămâne documentată, fără executarea ei.

---

## Slice 0022

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0022 | Istoric — endpoint, filtre și revizia grilei | 0022-01/02 DONE (code) / **NEVER RUN ON A LIVE DB** / **NO VISUAL VERDICT** | [0022-01](../SLICE-0022-01-istoric-endpoint.md#slice-0022-01); [0022-02](../SLICE-0022-01-istoric-endpoint.md#slice-0022-02) | Contractele și probele sunt în istoricul reunit; reviziile ulterioare sunt delimitate. |

### Current focus

- [Istoricul Istoric](../SLICE-0022-01-istoric-endpoint.md) păstrează probele și schimbările pe etapă, inclusiv precizările unice ale registrului.

### Open threads

- Probele live/vizuale și întrebările rămân în etapele legate; consolidarea nu le închide. Riscul numelor de capțiune și testul de constatare rămân în 01; schimbarea de prezentare este la finalul lui 02.

---
## Slice 0023

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0023 | Harness Adobe RHP — patru pârghii contra panoului din dreapta (+ scenarii, layout, veridicitate) | DONE (code) / **rulările din 04.08 sunt INVALIDE (bază contaminată)** | [Etapele 1/2/4/5/6](../SLICE-0023-adobe-rhp-harness.md) | Istoric consolidat prin D001-10: mecanisme, scenarii, corecții de veridicitate, probă modernă lipsă și plasarea ferestrei. Starea mașinii și rezultatele invalidate rămân datate în jurnal. |

---

## Slice 0024

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0024 | DDF, fila «Document» — profiluri Adobe cu comutare la rulare (+ gazdă Adobe partajată + captură/desprindere) | 0024-01/02/03 DONE (code) / **NEVER RUN AGAINST A REAL ADOBE** / **NO VISUAL VERDICT** | [0024](../SLICE-0024-01-adobe-shared-host.md) | Corpuri, probe și restanțe în jurnalul consolidat D001-14; statusul este checkpointul istoric, fără extinderea acceptărilor vizuale. |

### Current focus

Gazda/profilurile și observațiile reale sunt în [0024](../SLICE-0024-01-adobe-shared-host.md). Verificarea aritmetică a stărilor, observațiile capturii/teardown și verdictul ActiveX sunt distincte. Rămân probele aplicației, popup watcher, /n, UI preference inert și limitele fiecărei etape; ipotezele corectate nu devin reguli actuale.

---

## Slice 0025

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0025 | Suprafețe editabile din designer (`KBotNavList`, `KBotDataView`, `KBotCaptionBar`, `KBotBusyBar`) + `MainForm.navViews` autorit în designer | DONE (code, inclusiv pașii 7, 02, 05, 06, 07 și 08) / **VERIFICĂRILE DIN VISUAL STUDIO NU AU RULAT** | [0025 de bază și 02/05/06/07/08/09](../SLICE-0025-designer-surfaces.md) | Planul în conversație presupunea 0024, deja ocupat. Contracte/abateri, serializare, versiuni, probe și restanțe în jurnalul consolidat. Pasul 7 din plan ≠ subfelia 07; pașii 03/04 aveau cod dar nu worklog la checkpoint. Verdictul VS/vizual nu este certificat aici. Mărimea butonului (l) este livrată în 09, proporția imaginii și glifele hover/apăsat rămân limite distincte. |

---

## Slice 0026

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0026 | Consolidarea controalelor în `KBot.Controls`, grupate pe familii | DONE (code) / **NEDESCHIS ÎN DESIGNER** | `SLICE-0026-controls-consolidation.md` | Cerere de operator: «all custom controls MUST live in kbot.controls… their own separate folder… grouped logically». Felie **pur structurală, zero schimbări de comportament**. Cele cinci controale din `KBot.Theming` (`KBotNavList`+`KBotNavItem`, `KBotCaptionBar`, `KBotBusyBar`, `KBotNotice`, `KBotTextField`) au trecut prin `git mv` în `KBot.Controls`, fiecare în folderul lui, cu enum-urile lor cu tot; ce era deja în `KBot.Controls` s-a regrupat din rădăcină în `Tree/` (15 partial-uri + `ColumnDef`/`TreeLogger`/`TooltipTableModel`/`NodeDebugInfo`/`FrmNodeDebug`), `DataView/` (9 partial-uri + coloană/rând/colecție + cele 4 enum-uri) și `DataView/Events/` (verificat prin grep că `EventArgs`-urile sunt consumate exclusiv de grilă, nu de arbore); `Adobe/` era deja corect. **Perimetru întrebat, nu presupus:** form-urile de bază `KBotThemedForm`/`KBotShellForm` **NU** s-au mutat (nu-s controale — și așa `KBot.Theming` rămâne fără nicio referință la `KBot.Controls`, deci sensul referinței rămâne unic, fără ciclu), iar vederile din `KBot.App\Views\` **NU** s-au mutat (sunt ecrane: implementează `IAngajamentView` și cheamă API-ul; mutate ar fi tras `KBot.Api`/`KBot.Forexe`/`KBot.Xfa` într-o bibliotecă de UI care trebuie să rămână frunză). **Lărgiri minime în motor,** fiindcă membrii `Friend` au devenit invizibili peste granița de assembly: `ThemeShapes` `Friend`→`Public` (nu putea fi MUTAT — îl folosesc și `ModernRenderer` și `KBotShellForm`), iar `NativeMethods` devine modul `Public` **cu o suprafață publică de exact un membru** (`DragMove`, cerut de `KBotCaptionBar`): `SetTitleBarDark`/`SetRoundedCorners`/`ApplyMinMaxInfo`/`ApplyWindowTheme` au fost COBORÂTE `Public`→`Friend`, fiind consumate doar din `KBot.Theming`. **Capcană reală prinsă de compilator:** în fișierele de designer forma scurtă `Controls.KBotNavCorner` NU compilează — `Controls` se leagă întâi de `Control.ControlCollection` moștenit (`BC30456`), coliziune pe care vechiul `Theming.X` nu o avea; de aceea tipurile se scriu CALIFICAT COMPLET. La fel, un `Imports KBot.Controls` în `LoginForm.vb` face `Write` ambiguu între `GlobalErrorLog` și `AdobeHostLog` — s-a calificat `KBot.Controls.NoticeKind.Error` în loc. Cele 49 de teste `KBotNavList*` au trecut în `KBot.Controls.Tests` (fără `DisableTestParallelization`: verificat că nimic de acolo nu atinge starea statică `ThemeManager`/`ThemeStore`). `Controls` `FileVersion` 1.3.0.0→**1.4.0.0**; `Theming` era deja pe 1.3.0.0 din 0025-05 (necomis), acoperă și asta; **`AssemblyVersion` stă pe loc peste tot**, deși mutarea a scos cinci tipuri publice din `KBot.Theming` — toată soluția se recompilează împreună (dacă se dorește altfel, e o decizie separată, cu setul COMPLET livrat). Build **0 erori / 0 avertismente**, **812 passed / 0 failed / 0 skipped** — aceleași 812 ca înainte, redistribuite (Theming 76→27, Controls 286→335). Regula e scrisă acum și în `CLAUDE.md` (regulă de casă) și în `src/KBot.Controls/README.md` (tabelul de foldere). ⚠️ **Deschis:** nimic nu s-a deschis în designer-ul VS după mutare — `MainForm`/`LoginForm`/`DdfView` au acum controale din alt assembly, iar Toolbox-ul și cache-ul de design-time trebuie să le regăsească sub `KBot.Controls.*`; firele (a)–(j) ale feliei 0025 rămân toate deschise |

---

## Slice 0027

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0027 | `AdvancedTreeControl` — banda de căutare, tematizare și suprafață de designer | **DONE — VERIFICAT PE ECRAN 08.08.2026** | [0027](../SLICE-0027-tree-designer-surface.md#slice-0027-baza) | Corpuri, probe și restanțe în jurnalul consolidat D001-14; statusul este checkpointul istoric, fără extinderea acceptărilor vizuale. |
| 0027-02 | `AdvancedTreeControl` — un singur font, banda de subsol și strângerea | DONE (code) / **strângerea VĂZUTĂ PE ECRAN 09.08.2026 (raport de operator + corectură)**, restul benzii neconfirmat bucată cu bucată | [0027-02](../SLICE-0027-tree-designer-surface.md#slice-0027-02) | Corpuri, probe și restanțe în jurnalul consolidat D001-14; statusul este checkpointul istoric, fără extinderea acceptărilor vizuale. |
| 0027-03 | Valorile de designer ale arborelui în cele patru vederi (Rezervări / Recepții / Plăți / DDF) | DONE (code) / **niciodată văzut pe ecran** | [0027-03](../SLICE-0027-tree-designer-surface.md#slice-0027-03) | Corpuri, probe și restanțe în jurnalul consolidat D001-14; statusul este checkpointul istoric, fără extinderea acceptărilor vizuale. |

### Current focus

Baza are acceptare vizuală la 08.08; subsolul/strângerea din 02 au verdict parțial și corecții nereprobate, iar cele patru vederi din 03 sunt nevăzute pe ecran. Contractele, lecțiile, exportul designer și restanțele sunt în [0027](../SLICE-0027-tree-designer-surface.md), pe etapă.

---

## Slice 0028

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0028 | `KBotDataView` — subsol cu agregate pe tipul valorii, benzi tematizate, etichetă de celulă, buton de strângere | DONE (code) / **niciodată văzut pe ecran** | [0028](../SLICE-0028-dataview-footer-collapse-tooltip.md#slice-0028-baza) | Contracte, fișiere, probe și limite în jurnalul consolidat D001-10. |
| 0028-02 | `KBotDataView` — titlu de subsol, pictograme pe fiecare coloană, `Format` în stil Access | DONE (code) / **niciodată văzut pe ecran** | [0028-02](../SLICE-0028-dataview-footer-collapse-tooltip.md#slice-0028-02) | Contracte, fișiere, probe și limite în jurnalul consolidat D001-10. |
| 0028-03 | `KBotDataView` — filtru de coloană în stil Access (sortare + condiții + valori bifate) și suprascrierea culorilor pe schema întunecată | DONE (code) / **niciodată văzut pe ecran** | [0028-03](../SLICE-0028-dataview-footer-collapse-tooltip.md#slice-0028-03) | Contracte, fișiere, probe și limite în jurnalul consolidat D001-10. |
| 0028-04 | `KBotDataView` — `AutoSizeMode` PE COLOANĂ, care bate modul grilei | DONE (code) / **niciodată văzut pe ecran** | [0028-04](../SLICE-0028-dataview-footer-collapse-tooltip.md#slice-0028-04) | Contracte, fișiere, probe și limite în jurnalul consolidat D001-10. |
| 0028-05 | `KBotDataView` — cine deține lățimea unei coloane (designer vs trecere de layout) | DONE (code) / **niciodată văzut pe ecran** | [0028-05](../SLICE-0028-dataview-footer-collapse-tooltip.md#slice-0028-05) | Contracte, fișiere, probe și limite în jurnalul consolidat D001-10. |
| 0028-06 | Meniul de filtrare, AUTORAT ÎN DESIGNER (nu desenat în cod) | DONE (code) / **niciodată văzut pe ecran** | [0028-06](../SLICE-0028-dataview-footer-collapse-tooltip.md#slice-0028-06) | Contracte, fișiere, probe și limite în jurnalul consolidat D001-10. |
| 0028-07 | Schema «Modern»: butonul crește cât să încapă și umplutura, și textul | DONE (code) / **niciodată văzut pe ecran** | [0028-07](../SLICE-0028-dataview-footer-collapse-tooltip.md#slice-0028-07) | Contracte, fișiere, probe și limite în jurnalul consolidat D001-10. |
| 0028-08 | Trecerea OPERATORULUI peste cele două ferestre de filtrare (+ ce a rupt designerul pe drum) | DONE (code) / partea operatorului **văzută de el în designer**, restul neconfirmat | [0028-08](../SLICE-0028-dataview-footer-collapse-tooltip.md#slice-0028-08) | Contracte, fișiere, probe și limite în jurnalul consolidat D001-10. |
| 0028-09 | Butonul de TEMĂ intră în bara de titlu (selectorul de teme nu mai stă în gazdă) | DONE (code) / **niciodată văzut pe ecran** | [0028-09](../SLICE-0028-dataview-footer-collapse-tooltip.md#slice-0028-09) | Contracte, fișiere, probe și limite în jurnalul consolidat D001-10. |

### Current focus

Familia 0028 este reunită în [jurnalul DataView](../SLICE-0028-dataview-footer-collapse-tooltip.md), cu ancore pentru bază și 02…09. Stările registrului sunt istorice. Rămân de probat vizual bancul/vederile, DPI, culorile Dark și contractele de designer; PlatiView este reparat pe hârtie, iar observațiile operatorului din 08 nu certifică restul etapelor. Fiecare restanță și lecție tehnică este păstrată la etapa ei.

---

## Slice 0029

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0029 | `KBotDataView` — GRUPARE ca la raportul Access (agregate, culori, fonturi pe nivel) | DONE (code) / **niciodată văzut pe ecran** | `SLICE-0029-01-kbotdataview-grupare.md` | Cerere de operator: «i need the dgv to support grouping JUST like the access report does. it must support aggregation, different coloring and fonts. be careful when ordering / filtering a grouped dgv». Trei hotărâri confirmate ÎNAINTE de a scrie cod (grupurile se STRÂNG; agregatele pe amândouă benzile, alese pe nivel; sortarea pe o coloană negrupată ordonează ÎNĂUNTRUL grupurilor). Colecție nouă `Groups` (`KBotGroupLevel`, ordinea = ierarhia), fiecare nivel cu bandă de antet și de subsol, șabloane de titlu (`{0}` coloana / `{1}` valoarea / `{2}` numărul de rânduri), retragere cumulativă, culori și fonturi proprii, strângere. **A treia numerotare — indici de BANDĂ** (rândurile DESENATE, antetele/subsolurile de grup incluse), lângă cele două de la 0028-03; API-ul public a rămas în indici de MODEL, neatins. Virtualizarea a trecut de la înmulțire la **offset-uri cumulate + căutare binară**, ca benzile să poată avea înălțimi proprii — iar **calea negrupată n-a fost atinsă**: fără niciun nivel, tabloul de benzi nu se alocă deloc și geometria e cea de dinainte (tabloul e O(rânduri vizibile), adică exact ce evita virtualizarea, și n-avea voie plătit de cele șase vederi care nu grupează). **Miezul feliei, cerut explicit:** comparatorul compară CHEILE DE GRUPARE primele, deci un click pe antet ordonează în interiorul grupurilor și nu le poate sparge (altfel «Total ianuarie» ar apărea de patru ori, cu patru sume); iar un click pe coloana DE grupare întoarce sensul nivelului în loc să adauge o sortare, și ridică sortarea de rând (un antet n-are voie să poarte două semne). **A doua hotărâre de fond: filtrarea schimbă totalurile, strângerea NU** — un filtru scoate rânduri din pagină (grupul rămas gol dispare cu antet și subsol cu tot), o strângere doar ascunde, deci sumele stau pe loc; altfel operatorul care închide o lună ar vedea totalul general mișcându-se singur. Aceeași distincție în selecție: rândul filtrat afară o pierde, cel închis într-un grup **o păstrează** — fără asta `Ctrl+Dreapta` n-ar mai putea redeschide ce a închis `Ctrl+Stânga`, fiindcă n-ar mai exista rândul despre care să întrebe (`AnchorBandOfRow` îl sprijină pe antetul grupului). Agregatele: `ComputeAggregateText` parametrizată pe un INTERVAL de poziții de vedere — subsolul grilei îl cheamă pe tot, cel de grup pe felia lui, deci **o singură definiție a lui «sumă»**; niciun `GroupAggregate` pe coloană, fiindcă un raport Access scrie `=Sum([x])` o dată (pe nivel se alege DACĂ se arată, nu care e). Cache leneș per nod. Culorile: roluri PROPRII în temă, cu nuanța pe adâncime CALCULATĂ (nivelul 0 cel mai apăsat) ca un nivel nou să nu ceară un slot de paletă; peste ea, nivel → `GroupFormatting` → iar la ÎNTUNERIC culorile fixate se ignoră (fonturile nu). Bancul a primit secțiunea «Grupare». Build 0 erori; `Controls.Tests` **800** (752 înainte, **+48**). ⚠️ **Deschis:** nimic nu s-a văzut pe ecran (nuanța pe niveluri, retragerea, triunghiul, benzile sub Modern/Dark se judecă privind); dus-întorsul prin designerul VS nefăcut; **niciun ecran real nu grupează încă** (`IstoricView`/`PlatiView` sunt candidații, neatinși deliberat); nu se poate NAVIGA prin antete de la tastatură (o bandă n-are index de model — ar cere o noțiune de «bandă curentă»); ~20 octeți/rând vizibil DOAR când grila e grupată; **nu există raport Access de referință — exportul n-are secțiune `REPORTS`**, deci s-a implementat SEMANTICA de grupare a rapoartelor, nu un raport anume |

### Current focus

- **Slice 0029 (`KBotDataView` — grupare ca la raportul Access) — DONE (code); nevăzut pe ecran.**
  Trei lucruri merită scoase din ea. (1) **Gruparea a cerut o A TREIA numerotare** — indici de
  BANDĂ, adică rândurile desenate, printre care antetele și subsolurile de grup, care nu sunt
  rânduri de model deloc. Cele două de dinainte (MODEL / VEDERE) n-au fost atinse, și tocmai de
  aceea `_view` a rămas exact ce era: lista rândurilor de date care trec de filtre. Consecința
  fericită e că subsolul grilei și măsurarea coloanelor n-au avut nevoie de nicio modificare.
  (2) **Sortarea NU poate rupe gruparea, și asta e o proprietate a comparatorului, nu o
  verificare** — cheile de grupare se compară primele, deci nu există click care să împrăștie
  aceeași cheie prin listă; iar sortarea pe coloana DE grupare întoarce sensul nivelului în loc să
  adauge o a doua cheie. (3) **Filtrarea și strângerea NU sunt același lucru**, deși amândouă
  ascund rânduri: prima schimbă pagina (deci și totalurile, deci și selecția cade), a doua doar ce
  se vede din ea (totalurile stau, selecția rămâne). Dacă s-ar purta la fel, închiderea unei luni
  ar face totalul general să se miște singur — ceea ce se citește ca o greșeală de calcul.
  **Next:** aceeași datorie ca tot 0028 — nimic n-a fost pe ecran, iar aici se judecă privind exact
  lucrurile pentru care a fost cerută felia (nuanța nivelurilor, retragerea, fonturile). Și niciun
  ecran real nu grupează încă: `IstoricView` (clasificații) și `PlatiView` (luni) sunt candidații.
