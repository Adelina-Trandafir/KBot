# K-BOT — STATUS, slices 0090–0099

Everything recorded about each slice: its registry row, its «Current focus» notes and its
«Open threads» notes. The index in `../KBOT_STATUS.md` says what each slice is.

---

## Slice 0090

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0090 | **`KBOT_STATUS.md` împărțit: index + fișiere de câte zece felii (cererea operatorului, 28.09.2026)** | GATA (doar documente, fără cod) | `SLICE-0090-status-split.md` | Fișierul de 658 KB → 28 KB (index, decizii blocate, note transversale) + `state/KBOT_STATUS_<zeci>.md` (rândul complet + «Current focus» + «Open threads» per felie, copiate cuvânt cu cuvânt) + `state/KBOT_STATUS_SLICELESS.md` (munca fără felie + vechiul `project_state.md`, șters din rădăcină). Verificare: toate cele 1.570 de linii ale originalelor regăsite, în afară de cele două reguli vechi «How to update» înlocuite intenționat. Regula de citire în `CLAUDE.md`, `KBOT_STATUS.md`, `CODE_WORKFLOW.md`. |

### Open threads

- **0090 — două fișiere încă mari:** `KBOT_STATUS_0020-0029.md` (~184 KB) și
  `KBOT_STATUS_0040-0049.md` (~174 KB). Operatorul a refuzat deocamdată împărțirea lor pe felii.

---

## Slice 0091

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0091 | **Detaliul recepției citit tăiat din FOREXE + viteză/așteptări în Setări → FOREXE (cererea operatorului, 29.09.2026)** | GATA pe cod (build curat; nimic rulat, nimic testat) | `SLICE-0091-detaliu-taiat-viteza-asteptari.md` | Server: `detaliu_incomplet` în pasul 4b (rând fără valori / linii ≠ Suma / mai puțini indicatori decât angajamentul) ▸ recepția existentă rămâne neatinsă, cea nouă se creează doar cu rândurile întregi; F14/F15 doar semnalează pe ele; `receptii_incomplete` în ambele faze. Client: întrebare «Le citesc din nou acum?» după salvare (doar zilele tăiate). Setări → FOREXE: «Testează viteza» (fast.com, Chromium ascuns), «Validează de 2×» (doar sub 20 Mb/s), multiplicator ×1/1,5/2/3 pentru timpii WFL + așteptarea Ajax. `F14_PAUSED` / `F14Paused` = True (pauza cerută de operator). |

### Open threads

- **0091 — nimic rulat.** Testul de viteză depinde de pagina fast.com (`#speed-value.succeeded`, `#speed-units`); dacă își schimbă pagina, butonul spune că măsurarea a eșuat.
- **0091 — presupunere:** semnul 3 («mai puțini indicatori decât angajamentul») se sprijină pe un singur angajament (017_SCNB / AAB2DH3X6SK), unde fiecare recepție completă lista toți cei 7 indicatori. Dacă FOREXE listează altundeva doar o parte, recepțiile acelea vor fi semnalate mereu și RHR-ul lor nu se va mai actualiza — atunci semnul 3 trebuie scos.
- **0091 — F14 e încă în pauză** (`F14_PAUSED` în `prelucrare_asociere.py`, `F14Paused` în `AsociereForm.vb`). Scutirea țintită pe recepțiile tăiate o face inutilă; operatorul decide când se repune.
- **0091 — RHR-ul deja stricat** (017_SCNB: recepțiile 84, 86, 90) se repară la prima recitire în care detaliul lor vine întreg: pasul 4b adaugă liniile lipsă și corectează valorile.

---

## Slice 0092

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0092 | **Rezervarea inițială = un singur eveniment, pe ultima zi inițială (cererea operatorului, 29.09.2026)** | GATA pe cod (build curat; nimic rulat) | `SLICE-0092-rezervare-initiala-o-singura-zi.md` | AAB5H2CHDGD: AA2 adăugat «În definitivare» pe 27.08, AAB definitivat pe 28.08 → două rânduri `EInitiala` pe zile diferite → două frunze «Inițială». Varianta A (doar cititorii, fără rescriere în bază): `RezervariView.BuildTree` pune toate rândurile inițiale pe ultima zi inițială; `_SQL_GEN_REZERVARI` folosește aceeași zi (`ZiRez`) pentru regula «cea mai veche zi, cel mai mic tip» și ca dată a reviziei. |

### Open threads

- **0092 — AAB5H2CHDGD are DDF doar pe AAB:** AA2 e legat de revizia 262 (`AreDDF = 1`). Dacă 262 conține și AAB, cauza e pasul 3e cazul 2 (exclude revizia «deja folosită», deși o revizie acoperă mai multe rânduri). Aștept rezultatul interogării pe `FX_DDF_REV` / `FX_DDF_REV_SA`.
- **0092 — revizii 0 în plus?** Două «lock NUMARREV=0» în jurnal (12:35, 12:40).

---

## Slice 0093

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0093 | **Parteneri: filtrare, ANAF, banca din IBAN, cod fiscal unic (cererea operatorului, 29.09.2026)** | GATA pe cod (build curat; nimic rulat, nimic testat) | `SLICE-0093-parteneri-anaf-cf-unic.md` | `GET /parteneri` întoarce doar Tip = '1', CodFiscal completat și ≠ `AVACONT_COMUN.Unitati.CF` (DC-ul sesiunii), `Ascuns = 0`, un singur partener pe cod fiscal (primul după IdPartener; codurile comparate doar pe cifre). Banca goală → din IBAN (car. 5-8) prin `AVACONT_COMUN.BIC`, pe server (GET + POST) și în fereastră la tastarea IBAN-ului. Rută nouă `GET /parteneri/anaf/<cf>` (refolosește `routes/inregistrare/anaf.py`, v9). Fereastra: Tip și «Arată partenerii ascunși» scoase, «Alte detalii» → «Adresa», «Cod fiscal *» obligatoriu; la ieșirea din câmp / Enter se caută la ANAF și se completează denumirea + adresa. Salvarea refuză codul unității și un cod fiscal deja folosit (verificare în Python, NU constrângere MariaDB — datele din Access au dubluri; un partener vechi cu cod dublat rămâne editabil cât timp nu-și schimbă codul). Tip scris mereu '1'. |

### Open threads

- **0093 — nimic rulat.** Nici ruta ANAF, nici fereastra nu au fost încercate pe server/ecran.
- **0093 — partenerii ascunși / cu alt Tip / fără CF nu mai apar deloc** în fereastră; un partener marcat «Ascuns» la salvare dispare din listă. Cererea operatorului, dar nu mai există cale din K-BOT de a-l reafișa.
- **0093 — cod dublat ascuns:** verificarea de unicitate pe server include și partenerii ascunși sau cu alt Tip (mesajul spune «partener ascuns»); fereastra nu-i vede, deci operatorul află doar la salvare.

---

## Slice 0094

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0094 | **`KBotComboBox` rescris pe `Control` (nu mai moștenește `ComboBox`) și folosit ca editor de combo în `KBotDataView` (cererea operatorului, 29.09.2026)** — fără `DataSource`/`DisplayMember` (elementele în `Items`, afișate prin `CaptionSelector` sau `ToString()`); textul tastat într-o casetă fără chenar așezată ca textul pictat (rețeta 0085); O SINGURĂ listă: săgeata o deschide cu toate elementele, tastarea (`FindAsYouType`) cu cele potrivite; se închide la un clic în afara ei; bară de derulare trasă cu mouse-ul. În grilă: editorul de combo stă pe o linie, centrat ca editorul de text, peste textul celulei; săgeata celulei (pictată de grilă) deschide lista sub celulă; textul liber rămâne la `CellValidating` | GATA pe cod (build Controls/App/Migrator/Theming/Updater **0 / 0**) / **nevăzut pe ecran, fără teste** | `SLICE-0094-combo-own-control-grid-editor.md` | 14 fișiere Designer curățate de liniile ComboBox (`DrawMode`, `DropDownStyle`, `FlatStyle`, `IntegralHeight`, `ItemHeight`, `FormattingEnabled`). An/SS și unitatea de la login: `Items` în loc de `DataSource`. Controls 1.57.0.0, DevHarness 1.0.29.0. |

### Current focus

- **Slice 0094 — combo-ul propriu (29.09.2026).** `KBotComboBox` e un `Control` cu casetă de text,
  listă proprie și colecție de elemente proprie; grila îl folosește ca editor de combo, așezat ca
  editorul de text. **Next:** operatorul îl vede în aplicație (grila DDF Secțiunea A, Parteneri,
  combo-urile din Setări / login / MainForm).

### Open threads

- **0094 — nimic văzut pe ecran.** Alinierea la pixel a editorului de combo cu textul celulei se
  sprijină pe rețeta măsurată la 0085 pentru editorul de text; pentru combo nu s-a măsurat.
- **0094 — `AssemblyVersion` a KBot.Controls a rămas 1.0.0.0** deși tipul de bază al
  `KBotComboBox` s-a schimbat; merge doar pentru că actualizarea livrează tot setul.
- **0094 — cod rămas fără folosință:** ajutoarele pentru EDIT-ul nativ din
  `KBot.Theming/Interop/NativeMethods.vb` (`GetComboEditBounds`, `SetComboEditMargins`,
  `SetComboEditBounds`, `GetComboEditTextTop`, `GetComboEditLineHeight`, `ApplyControlColors`).
- **0094 — diferențe de comportament:** rotița peste un combo ÎNCHIS nu mai schimbă selecția;
  `Items.Clear()` pe un combo editabil păstrează textul tastat.

---

## Slice 0095

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0095 | **Istoric: nod rădăcină «Tot istoricul» (cererea operatorului, 29.09.2026)** — arborele Istoric are acum un nivel rădăcină, ca celelalte arbori (Extrase: «Toate extrasele»); lunile stau sub el, zilele sub luni. Clic pe rădăcină = ce face vederea la încărcare: toate filtrele golite, toate rândurile FX_Istoric ale angajamentului | GATA pe cod (build KBot.App **0 erori**) / **nevăzut pe ecran, fără teste** | `SLICE-0095-istoric-tree-root.md` | Doar `IstoricView.vb`: `NodPerioada.Tot()` + `EsteTot`. Numărul din dreapta rădăcinii = toate rândurile, inclusiv cele fără dată (până acum fără nod în arbore). Clicul pe rădăcină renunță și la intervalul cerut de FOREXE (0073). FileVersion KBot.App nebumped (îl cere `push-update.ps1`). |
| 0095-02 | **Extrase: iconița din stânga subsolului arborelui descarcă din nou direct extrasele SNM; fereastra «Extrase de cont» se deschide din «Meniu → Extrase» (nemodală, una singură); `btnMeniu` ia lățimea lui `navViews` când bara se strânge (doar iconița) (cererea operatorului, 29.09.2026)** | GATA pe cod (build KBot.App **0 avertismente, 0 erori**) / **nevăzut pe ecran, fără teste** | `SLICE-0095-02-extrase-menu-footer-meniu-width.md` | Operatorul a numit-o «slice 95», deja ocupată de Istoric → sub-pas 0095-02. `KbotForm.Extrase.vb` (subsol + `DeschideExtrasele`), `KbotForm.Nomenclatoare.vb` (meniu + lățime), `KbotForm.CabNotes.vb` (`_menuMarked`), intrarea de meniu din Designer (a operatorului). Coloana 0 din `tlyHeader` urmează bara abia după prima strângere. FileVersion KBot.App nebumped. |

### Open threads

- **0095-02 — nevăzut pe ecran.** Butonul strâns (doar iconiță), alinierea la 1px după
  desfășurare (239 vs 240 logic); tooltipul lui `btnMeniu` nu pomenește Extrasele.

---

## Slice 0096

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0096 | **Extrase: meniu de afișare în antetul arborelui (cererea operatorului, 29.09.2026)** — iconița de setări din dreapta antetului arborelui (vederea Extrase și fereastra «Extrase de cont») deschide două rânduri: «Arată antet + operații» (ca până acum) și «Arată operații + detalii» (sus operațiunile FX_Extrase ale perioadei alese în arbore, jos detaliul celei selectate; «Data bancă» primește grupare doar în acest mod, «Plătitor» și «CUI» filtrare + grupare) | GATA pe cod (build KBot.App **0 avertismente, 0 erori**) / **nevăzut pe ecran, fără teste** | `SLICE-0096-extrase-display-menu.md` | Doar `ExtrasePanel.vb` + o iconiță/tooltip în `ExtrasePanel.Designer.vb`. Presupuneri: «Data» = `o_data_banca`; gruparea e OFERITĂ în meniul coloanei, nu aplicată automat; alegerea nu se salvează în `AppSettings`. FileVersion KBot.App nebumped. |

### Open threads

- **0096 — nevăzut pe ecran.** De confirmat cu operatorul: gruparea pe dată oferită vs. aplicată
  automat la intrarea în mod; dacă alegerea modului trebuie ținută minte între sesiuni.
