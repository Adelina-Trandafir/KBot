# SLICE 0000-12 — the Asocieri window, explained properly

**Date:** 30.09.2026. **Standing slice:** 0000 (help). Operator: «how do we do the Asociere form.
THAT is a critical step which needs extensive explanation, as it is not easy to understand».

## What changed and why
Before: four bullets inside «Descărcarea unui angajament» and two lines in «Recepții». Now a section
of its own, `HelpContent/contabil/asocieri/` (under Part 1, order 35, between FOREXE and the views):
- `index.md` — **«Asocierile recepțiilor — de ce există»**: the two halves (the reception list = today;
  the history = dated snapshots that never name their reception), a worked example (1.000 → 1.300 →
  600, absolute values, every indicator named), why it matters (the total at the payment date for
  every ORD; a wrong placement is silent and permanent), why K-BOT cannot do it alone (value, date,
  no-change saves), when the window opens (after a download / anytime from Recepții).
- `fereastra.md` — **the window, part by part**: left tree + marks ([ștearsă], [reconstituită],
  [nouă], [ștergere], [fără schimbare], blocked = dim, «⚠ Lanțul nu se închide»), grids, the basket,
  Grafic (Recepția / Tot angajamentul, click on a point) and Distribuție (bands), the payment guides
  and their arithmetic as the quickest check, «Grafice și benzi», the three buttons, the message band.
- `pasi.md` — **how to place, step by step**: drag, place given by the hour, detach, Ctrl/Shift
  groups; how to tell whose a snapshot is (value, indicators with the «cannot disappear» refusal,
  hour, documents, payment guides); the recommended order; automatic placements (server after a
  download; unique-value pairs in the anytime window) and that they can be wrong.
- `cazuri.md` — **special cases**: «Nu consemnează nicio schimbare», «Este rândul de ștergere»,
  «Începe o recepție nouă» (reconstituted receptions, its two rules, «Reconstituire nesigură»),
  blocked links, drag refusals, save / «Golește așezările» / closing after a download (throws the
  download away) vs in the anytime window, someone else changed it meanwhile.
- `tours/tur-asocieri.md` — 8 steps. It must be STARTED FROM INSIDE the window (F1 → topic → tour):
  the window is modal, and a bubble created before it opens stays disabled; one created after is
  fine (`HelpTourRunner` makes a new bubble per tour, `HelpService.EnsureWindow` rebuilds a help
  window a modal disabled). Step 1 says so.
- `forexe/descarcare.md`: the Asocieri part cut to a summary + links; the Grafice/Benzi paragraph
  moved into `fereastra.md`; AsociereForm / GraficeAsociereForm / AsociereBenziForm keys moved to the
  new topics. `vederi/receptii.md` and the Contabil home page link to the new section.
- Captures: new `asocieri-fereastra`, `asocieri-grafic`, `asocieri-tragere`; `asocieri-grafice`
  moved (id kept); `asocieri-propunere` stays in «Descărcarea».

## Sources
`docs/FUNDAMENT_Asociere_Receptii.md` (Parts 1-2: F7, F9-F17, F21, F26-F28, F31), worklogs 0048-04,
-06, -08, -09, 0059, 0061, 0062; `Forexe/AsociereForm.vb` + `.Designer.vb` + `.resx` (captions,
menus, messages, modes). Left out on purpose: F14 (switched off 29.09.2026 — `F14Paused`), the
withdrawn date rule (F13), the server's two-phase mechanics.

## Test results
`Check-Help.ps1 -Coverage`: 47 topics, 12 tours, 50 capture tags, no errors, coverage «(none)».
Build App 0 warnings, 0 errors. Nothing on screen; the tour never run.

## To read (operator)
- «Legături blocate»: the text says the ORD using the snapshot must be changed first to move it.
- The recommended working order in `pasi.md` is my proposal — replace it with how you actually do it.
- The worked example is the FUNDAMENT's (AAB / AA2); a real one from a client could replace it.
