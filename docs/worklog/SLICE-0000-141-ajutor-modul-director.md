# SLICE 0000-141 - help for the director's module (30.10.2026)

## What changed
The `director` part of the help describes the director's main window (family DIR1, `SLICE-DIR1-01..03`), not the old list-only window. Source tags: `DIR1-01` / `DIR1-02` / `DIR1-03` with `0000-141`.

## Topics
- `director` (`index.md`): what a director sees (Fundamentare / Ordonanțare / Note corecție), the window in short, the read-and-sign rule. `DirectorForm` stays in `screens:` so the coverage check stays clean.
- `director.documente` (`documente.md`): the angajamente list, the three pages and when each shows, the empty page, what a director cannot do.
- `director.lista` (`lista.md`): the button «Documente de semnat» (current unit, document straight on its PDF page). The tray section is TRAY-01's.
- `director.unitati` (`unitati.md`): the unit list in the title bar, no password, only director units.
- `director.semnare` (`semnare.md`): the steps, and «Unde ai voie să semnezi» (only the ordonator field; the refusal message; K-BOT restores the document by itself after OK).
- `tours/tur-director.md`: four steps on `KbotForm.tree`, `navViews`, `viewHost`, `capBar`.

## Captures
Tags `director-fereastra`, `director-angajamente`, `director-lista`, `director-semnare`; all still «lipsa», they need a real director login (`prepare:` says how).

## Test results
`Check-Help.ps1 -Coverage`: no line about the director files except the two unknown ids (`TRAY-01`, `0000-142`) of the tray section, which is not this slice's.
