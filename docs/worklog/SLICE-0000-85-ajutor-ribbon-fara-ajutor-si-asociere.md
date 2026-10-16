# SLICE-0000-85 - Help for 0114-02 / 0084-04: no «Ajutor» button, «Asociaza parteneri» on the ribbon (16.10.2026)

## What changed and why
- The ribbon's «Ajutor» button is gone (0114-02): topic `contabil.ajutor.buton` (`contabil/ajutor-buton.md`) **deleted**; its
  two links (`ajutor.md`, `ribbon.md`) removed; `ajutor.md` now says help opens in two ways (F1, the «?» of the caption bar);
  `ribbon.md` sections «Strangerea ribbonului» and «Butoanele din dreapta» rewritten (the right-hand buttons are «Inchide» and the
  page buttons); `docs/HELP_SYSTEM.md` updated (what stays in the code).
- «Asociaza parteneri» is on the ribbon, visible only while «Sumar» is on screen, before «Inchide» with a thin separator
  (0084-04 / 0114-02): `contabil/vederi/sumar.md`, `tours/tur-sumar.md` (the step has no `target:`: painted ribbon buttons cannot
  be ringed by a tour), `contabil/ribbon.md`.
Tags `0114-02, 0114-03, 0084-04, 0000-85`. Extended the same day for the right group (0114-03): the button is in the group «Sumar» at the right of the panel, before «Inchide».

## Files touched
`src/KBot.App/HelpContent/contabil/ajutor.md`, `ribbon.md`, `ajutor-buton.md` (deleted), `vederi/sumar.md`, `tours/tur-sumar.md`;
`docs/HELP_SYSTEM.md`.

## Test results
`tools\HelpCheck\Check-Help.ps1` run 16.10.2026: no error from these files once the slice ids exist; the errors left are old
(`tutorials\*`, `tur-setari.md` nav item `jurnal`), not caused here.

## Unverified / deferred
- Captures `ribbon-file` and `ribbon-strans` (they showed «Ajutor») are to be redone; the `ajutor-buton-*` captures go with the
  topic (no image files existed for them).
- Not seen on screen.
