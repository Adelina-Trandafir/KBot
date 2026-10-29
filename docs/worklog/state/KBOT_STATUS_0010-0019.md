# K-BOT — STATUS, slices 0010–0019

Moved verbatim out of `../KBOT_STATUS.md` (28.09.2026). The index there says what each slice is; this file holds everything recorded about it: its registry row, its «Current focus» notes and its «Open threads» notes.

---

## Slice 0010

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0010 | `KBotDataView` — owner-drawn unbound grid (Access continuous-form) | DONE (code) / **NO VISUAL VERDICT YET** | [0010-01](../SLICE-0010-01-kbotdataview-skeleton.md#slice-0010-01), [0010-02](../SLICE-0010-01-kbotdataview-skeleton.md#slice-0010-02), [0010-03](../SLICE-0010-01-kbotdataview-skeleton.md#slice-0010-03), [0010-04](../SLICE-0010-01-kbotdataview-skeleton.md#slice-0010-04), [0010-05](../SLICE-0010-01-kbotdataview-skeleton.md#slice-0010-05), [0010-06](../SLICE-0010-01-kbotdataview-skeleton.md#slice-0010-06), [0010-07](../SLICE-0010-01-kbotdataview-skeleton.md#slice-0010-07) | Plan pasted in-session; continuation plan supersedes original where they disagree. Contract evolution, file lists and per-stage headless proof are in the consolidated worklog. Historical checkpoint: 158 solution tests after 06; 07 has its own Controls-only result and file-lock limitation. No visual verdict; Sumar was the next consumer. |

### Current focus

- **0010 — historical checkpoint:** code landed; headless results and unresolved visual/input checks are in the [consolidated worklog](../SLICE-0010-01-kbotdataview-skeleton.md). Human harness verdict still unrecorded at this checkpoint; next was Sumar. Later slice-specific acceptance is not inferred here.

---

## Slice 0011

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0011 | Sumar — endpoint, client și corecția fan-out-ului | DONE (code) / **partially run on a live DB (0011-03)** / **NO VISUAL VERDICT** | [0011-01](../SLICE-0011-01-sumar-endpoint.md#slice-0011-01); [0011-02](../SLICE-0011-01-sumar-endpoint.md#slice-0011-02); [0011-03](../SLICE-0011-01-sumar-endpoint.md#slice-0011-03) | Contractele și probele sunt în istoricul reunit; reviziile ulterioare sunt delimitate. |

### Current focus

- [Istoricul Sumar](../SLICE-0011-01-sumar-endpoint.md) păstrează probele și schimbările pe etapă, inclusiv precizările unice ale registrului.

### Open threads

- Probele live/vizuale și întrebările rămân în etapele legate; consolidarea nu le închide. Triage-ul duplicatelor și alegerea arbitrară LIMIT 1 rămân în 03.

---
## Slice 0012

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0012 | Migrare Access → MariaDB — introspecție coloane pentru seed | 0012-01 DONE (code) / **NEVER RUN ON A LIVE DB** | [0012](../SLICE-0012-01-seed-columns.md) | Contractele, deciziile, formulele, probele și limitele sunt în jurnal; Status păstrează checkpointul istoric. |

---

## Slice 0013

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0013 | `KBotDataView` — auto-sizing coloane + moduri de umplere | DONE / **VISUALLY ACCEPTED** in harness playground | [0013](../SLICE-0013-column-sizing.md#slice-0013) | Algoritmii, formulele, probele, limitele și contradicțiile notelor istorice sunt păstrate în etapa legată și în introducerea istoricului. |

---

## Slice 0014

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0014 | Rezervări — endpoint + `RezervariView` (a doua vedere reală) | DONE (code) / **NEVER RUN ON A LIVE DB** / **NO VISUAL VERDICT** | [0014](../SLICE-0014-rezervari-view.md) | Contractele, deciziile, formulele, probele și limitele sunt în jurnal; Status păstrează checkpointul istoric. |

### Current focus

- [0014 — detalii și restanțe](../SLICE-0014-rezervari-view.md): formula totalului este documentată din Access; proba live, ecranul și totalurile numerice din captură rămân distincte.

---

## Slice 0015

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0015 | Recepții — endpoint, tooltip și revizia lunară | 0015-01/02/03 DONE (code) / **NEVER RUN ON A LIVE DB** / **NO VISUAL VERDICT** | [0015-01](../SLICE-0015-01-receptii-view.md#slice-0015-01); [0015-02](../SLICE-0015-01-receptii-view.md#slice-0015-02); [0015-03](../SLICE-0015-01-receptii-view.md#slice-0015-03) | Contractele și probele sunt în istoricul reunit; reviziile ulterioare sunt delimitate. |

### Current focus

- [Istoricul Recepții](../SLICE-0015-01-receptii-view.md) păstrează probele și schimbările pe etapă, inclusiv precizările unice ale registrului.

### Open threads

- Probele live/vizuale și întrebările rămân în etapele legate; consolidarea nu le închide. Ultima variantă de structură este 03, fără verdict vizual/live.

---
## Slice 0016

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0016 | `KBotDataView` — coloane auto-hide (responsive) | DONE (code) / **NO VISUAL VERDICT** | [0016](../SLICE-0013-column-sizing.md#slice-0016) | Algoritmii, formulele, probele, limitele și contradicțiile notelor istorice sunt păstrate în etapa legată și în introducerea istoricului. |

---

## Slice 0017

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0017 | Plăți — endpoint + `PlatiView` (a patra vedere reală) + `KBotDataView` totals row | 0017-01..04 DONE (code) / **0017-04 VĂZUT PE ECRAN ȘI ACCEPTAT 2026-08-13** (prima acceptare vizuală a vederii Plăți) / **NEVER RUN ON A LIVE DB** | [Totaluri](../SLICE-0017-01-kbotdataview-totals-row.md); [02: endpoint](../SLICE-0017-02-plati-endpoint.md#slice-0017-02), [03: vedere](../SLICE-0017-02-plati-endpoint.md#slice-0017-03), [04: revizie](../SLICE-0017-02-plati-endpoint.md#slice-0017-04) | Contractul general este în 01; implementarea, arborele revizuit, probele și restanțele sunt în istoricul 02…04. |

### Current focus

- Reper 13.08.2026: [04 — arbore și acceptare vizuală pe date de test](../SLICE-0017-02-plati-endpoint.md#slice-0017-04). Nu extinde proba la baza reală sau la harnessul general de totaluri.

### Open threads

- [Restanțele etapelor și reperele registrului](../SLICE-0017-02-plati-endpoint.md): rulare live, resurse/tipuri în designer, sortare și cablarea Istoric/DDF. Fixul Rezervări «+» rămâne nefăcut, fără număr după realocarea lui 04.

---
## Slice 0018

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0018 | `KBotNavList` — separatori + orientare + aliniere Near/Far + vizibilitate; gating MainForm prin ascundere | DONE (code) / **NO VISUAL VERDICT** | `SLICE-0018-navlist-separators-orientation-visibility.md` | Four capabilities added to `KBotNavList` (`KBot.Theming`): (1) **separatori** — `AddSeparator(Optional align)`, oricâți, desenați ca linie fină (`Palette.BorderColor`), neselectabili, săriți de mouse/tastatură; cheie internă auto (`__sep_N`) ca să nu colideze cu `FindIndex`; (2) **orientare** — `Orientation` (`KBotNavOrientation.Vertical` implicit / `Horizontal`); orizontal măsoară lățimea butonului din text, centrează textul și remapează navigarea pe Stânga/Dreapta; (3) **aliniere Near/Far** — enum nou `KBotNavAlign` pe `AddItem(key,text,align)` și `AddSeparator(align)`; `Far` ancorează grupul **jos** (vertical) / **dreapta** (orizontal); (4) **vizibilitate** — `SetItemVisible(key,visible)`, un buton ascuns nu ocupă spațiu, nu se pictează, nu se selectează, e sărit de tastatură (rezolvă firul deschis „`KBotNavList` has no `SetItemVisible`"). Layout-ul e acum un singur `RecalcLayout` (invalidat la add/vizibilitate/orientare/resize/font), nu vechiul `index * height`; grupul Far se așază de la `capăt - extindereTotală`. **`MainForm`:** DDF/ORD mutate în grupul `Far` cu `AddSeparator(Far)` deasupra (desprinse la baza barei); **`ApplyViewGating` trecut de la `SetItemEnabled` la `SetItemVisible`** — un flag `Are*=FALSE` acum **ASCUNDE** butonul, nu doar îl dezactivează (cererea operatorului). Fallback-ul la `sumar` când vederea activă dispare rămâne. `KBot.Theming` + `KBot.App` compilează curat, 0 warnings. ⚠️ **Fără verdict vizual** — verificat doar prin compilare; harness-ul controalelor UI rămâne nerulat (ca 0010+); ~~**fără teste xUnit pentru `KBotNavList`** (nu existau înainte) — separator/vizibilitate/aliniere/orientare neacoperite de teste~~ **REZOLVAT în felia 0025** (`KBotNavListTests.vb`, 20 de teste: separatori + chei `__sep_N`, ordinea Near/Far verificată geometric, «ascuns ⇒ `Rectangle.Empty`, sărit de `IndexAt` și de navigarea cu tastatura»). Rămâne fără verdict vizual |

---

## Slice 0019

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0019 | XFA_WRITTER → `KBot.Xfa` (librărie în proces, nu exe separat) | DONE (code) / **NEVER RUN LIVE** / **NO VISUAL/PDF VERDICT** | `SLICE-0019-xfa-writter-library.md` | Portat exe-ul `XFA_WRITTER` din `Surse/SURSA_XFA_WRITTER` în `src/KBot.Xfa` ca **librărie apelată în proces** — K-BOT nu mai lansează un `.exe` separat cum făcea Access prin `WScript.Shell.Run` + cod de ieșire. API public = **`XfaWriter`** (fațadă ce înlocuiește `Program.Main`): `Genereaza` / `GenereazaSiSemneaza` / `Semneaza` / `ExtrageAtasamente`, întorcând un **`XfaResult`** (`Reusit`, `Semnat`, `Masca`, `SemnatAB/CD/Ordonator`, `CaleOutputPdf`, + `CodIesireLegacy` 0/2/11..17 pentru compatibilitate). `docType` ∈ {DDF, ORD}. Motorul `AdobeUtils` (iTextSharp 5.5.13.3 `PdfReader`/`PdfStamper`/`XfaForm`): completează XFA din XML, embedează atașamente base64, deschide Adobe și **așteaptă** semnarea (fără timeout, `WaitForExit`), calculează masca semnatarilor din handler-ul `.pdf` din registry (+ validare că e Adobe). `TemplateDownloader` = cache DDF/ORD de pe API-ul **legacy** (`Configs`: adcredit.avatarsoft.ro:5008 + `X-API-KEY`, `c:\avacont\cache`) — separat de API-ul K-BOT bearer. **Adaptări la regulile casei:** fără bloc `Namespace` (`AdobeUtilsNS` scos); `Logger`→modul `XfaLog` (fără `AllocConsole`, trasare în `C:\AVACONT\logs`); granițele loghează în `GlobalErrorLog` + rearuncă, deci `KBot.Xfa` referă acum `KBot.Common` (fără ciclu — Common referă doar Domain); comentarii/șiruri românești. **Două capcane rezolvate:** (1) `Option Strict On` rupe `PdfStamper(…, "\0", True)` original → `ChrW(0)` (sentinela iTextSharp „păstrează versiunea machetei"; exe-ul pe Option Strict Off trimitea de fapt un backslash); (2) shadowing VB case-insensitive — Sub `DeschidePdf` vs param `deschidePdf` → Sub redenumit (aceeași capcană ca la 0010/`ProposedValue`/`HeaderText`). **Modurile cu semnare BLOCHEAZĂ** (deschid Adobe) → din UI rulează-le pe thread de fundal. iTextSharp/BouncyCastle sunt pachete .NET Framework pe net8.0-windows → NU1701 e așteptat, rulează corect (aceeași versiune ca exe-ul). `FileVersion` 1.0.0.0→1.1.0.0. **Cum se rulează din K-BOT:** DevHarness → categoria **XFA** (`XfaWriterHarnessTest` în `KBot.App/HarnessTests`, doar Debug, descoperit prin scanarea assembly-ului de intrare) — DOAR logica pură offline (separare atașamente base64 + clasificare mască ORD/DDF). Proiect `tests/KBot.Xfa.Tests` xUnit adăugat (**39 teste**: `ExtrageAtasamente` + `MascaSemnatari` ORD/DDF + `XfaResult` coduri 0/2/11..17), adăugat în `KBot.sln`; soluție întreagă verde (0 erori). ⚠️ **Deschis:** (a) generarea completă a PDF-ului cere macheta de pe serverul legacy — niciodată rulată live; (b) semnarea cere Adobe ca handler implicit `.pdf` — neverificată; (c) niciun PDF real nu a fost produs/inspectat; (d) testele acoperă DOAR logica pură (separare atașamente + mască) — motorul XFA propriu-zis (`ModifyXfaFromXml`, clonare `SubformInf`, embed) cere o machetă PDF reală, neacoperit; (e) vederile editor DDF/ORD rămân amânate (vor apela `XfaWriter`) |

### Open threads

- **No real DDF PDF produced or opened** (0019 open items a/c, still open after 0020-05). The generation flow is wired and unit-tested, but `XfaWriter.Genereaza` needs the machete + Adobe; the "Adobe opens the generated PDF correctly" verdict is outstanding.
