# K-BOT — STATUS, slices 0030–0039

Moved verbatim out of `../KBOT_STATUS.md` (28.09.2026). The index there says what each slice is; this file holds everything recorded about it: its registry row, its «Current focus» notes and its «Open threads» notes.

---

## Slice 0030

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0030 | Meniul de coloană: TREI FILE (Sortare / Filtrare / Grupare), rânduri de tabel pe măsura temei, și bifele care se pierduseră | DONE (code) / **niciodată văzut pe ecran** | [0030](../SLICE-0030-filter-popup-file-si-grupare.md) | Cereri, Handles, măsurarea ThemeTableFit, file/grupare, probe și restanțe în jurnal. Registrul numea comentariul Handles «singura apărare»; jurnalul consemnează și testul dedicat. Capcana Control.Visible a apărut în două locuri. |

### Current focus

- **0030 — DONE (code), nevăzut pe ecran.** Cele trei lecții (Handles, măsurarea ThemeTableFit și Control.Visible), mecanismele și restanțele vizuale sunt în [jurnal](../SLICE-0030-filter-popup-file-si-grupare.md).

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
| 0033 | Vederea ORD (Ordonanțări), read-only + ruta nouă `GET /api/forexe/ord` | DONE (code) / **niciodată văzută pe ecran, ruta niciodată pe o bază vie** | [0033](../SLICE-0033-ord-view.md) | Ruta/vederea, cheile și clasificația, PDF-ul, cele 10 avertismente (8+2), probele și amânările: în jurnal. Alegerea live a ramurii COALESCE rămâne neconfirmată. |
| 0033-02 | Arborele `DdfView` adus la paritate cu cel din `RezervariView` (iconițe din `ImageList` + dimensiuni) | DONE (code) / **niciodată văzut pe ecran** | [0033-02](../SLICE-0033-02-ddf-tree-parity.md) | Prioritatea ImageList, înălțimea 24→30 și păstrarea structurii: în jurnal; neutral lipsește încă și folosește fallback GDI. |

---

## Slice 0034

### Registry

| Slice | Name | Status | Worklog(s) | Notes |
|------:|------|--------|-----------|-------|
| 0034 | Descărcarea FOREXE: bandă de subsol + consolă + descărcări din arbore | DONE (code) / **niciodată rulat pe FOREXE viu, nicio suprafață văzută pe ecran** | [0034](../SLICE-0034-forexe-footer.md) | Coordonatorul, descărcările locale/JSON, cele cinci tabele WFL, corecțiile față de plan și probele: în jurnal. Trecerea ulterioară KBotProgressBar și reserializarea celor două gazde în VS rămân distincte de lipsa probei pe ecran a controlului. |

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
| 0037 | `KBotDataView` — coloana care se întinde poate fi ORICARE (`SpecificColumn` + `FillColumnKey`) | DONE (code) / **nevăzut pe ecran, FĂRĂ TESTE (cerute sărite)** | [0037](../SLICE-0013-column-sizing.md#slice-0037) | Contractele, raționamentul, probele și limitele sunt în jurnal; starea este a etapei istorice. |
