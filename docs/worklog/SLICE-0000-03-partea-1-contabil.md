# SLICE-0000-03 — Part 1 «Contabil»: the text, with capture tags

Operator request, 30.09.2026: write Part 1 from the slice .md files; no custom-control or browser
internals; the FOREXE page as it is meant to work, not what is clicked on it; a section on what
MF expects for a NEW DDF (ORD creation left out: it does not exist yet).

## What changed and why

31 Romanian topics under `src/KBot.App/HelpContent/contabil/` (+ README example), 30 capture tags
for the operator to shoot through «Capturi pentru ajutor» (0000-02).

| Chapter | Topics |
|---------|--------|
| Despre K-BOT | overview + the golden rule; Conectarea; Fereastra principală; Cum folosești ajutorul |
| Legătura cu FOREXE | band, conectare (+ operațiuni necorectate), lista, descărcarea (+ Asocieri, recepții tăiate, reîmprospătări parțiale), Browser FOREXE, consola |
| Vederile | Sumar, Istoric, Rezervări, Recepții (+ asocieri oricând), Plăți, Extrase (view + window + modes + columns), Ordonanțare (view only) |
| Documentul de fundamentare | flow + the seven revision states + tree menu; **Ce cere MF la un DDF nou**; Angajament nou; Editorul; Adaugă rezervare / Definitivează / Derulează; Semnarea; Trimiterea |
| Other | Operațiuni necorelate + note CAB; Nomenclatoare (Clasificații, Parteneri); Setări și aspect |

Sources actually read: worklogs 0011-02, 0014, 0015-03, 0017-03/04, 0022-02, 0033, 0034, 0048-04,
0055, 0057, 0060, 0063, 0074, 0080-02/03, 0081-12, 0084, 0087, 0088, 0091, 0092, 0093, 0095, 0096;
`docs/FUNDAMENT_DocumentFundamentare_CAB.md` (MF guide for OMF 1140/2025 — the MF section is built
from it), `docs/HANDOFF_0081_DDF_Trimitere.md`, `docs/PLAN_DDF_Trimitere.md` (states + Rezervări
menu table); operator strings from the designers (tooltips, captions) and `DdfView` / signing messages.
State names and menu captions checked against the code, not only the plan.

`screens:` keys use the real form / view / control names, so F1 lands on these topics.

## Files touched

- New: `HelpContent/contabil/{fereastra,notecab,setari}.md`, `contabil/forexe/*` (6),
  `contabil/vederi/*` (8), `contabil/ddf/*` (7), `contabil/nomenclatoare/*` (3).
- Rewritten: `contabil/index.md`, `contabil/autentificare.md`.

## Test results

- Build KBot.App: 0 warnings, 0 errors; 33 topic files copied to `bin\...\Help\`.
- Static check (bash): no duplicate topic ids, every `parent:` and every `topic:` link resolves,
  no duplicate capture id. Not opened on screen (operator's rule).

## Left unverified or deferred

- **To be read by the operator** — written from the code/worklogs, not from how accountants work:
  especially «Asocieri» (what to decide in the basket), the Recepții footer LEFT icon (described
  as «reface din istoric recepțiile care lipsesc», from slice 0062's name only), the DDF «+» on a
  reservation leaf, the signing steps' button names inside the PDF («Validez antet și secțiunea A»,
  «Validez secțiunea B», from the MF guide summary), and the Setări page list.
- **Found in passing:** the «Sursă/Sector» combo's tooltip says «Subperioada (SS) din anul ales»
  but the combo holds a sector/source (e.g. 02A). The help says sector/source; the tooltip in
  `KbotForm.Designer.vb` (`cboSs`) was NOT changed.
- ORD creation not described (does not exist yet); Part 2 and Part 3 not written (0000-05/06).
- Guided tours (0000-04) not started.
