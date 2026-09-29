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
