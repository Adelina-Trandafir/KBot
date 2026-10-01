# SLICE-0000-36 — Help for slice 0100-02 («Descărcări multiple» is a server-controlled page) (operator request, 01.10.2026)

Help work for feature slice **0100-02** (`SLICE-0100-02-setari-server-multithread.md`), branch `SLICE-0100-Multithreading`
(`master` was merged INTO this branch first, so the help engine of 0000-34 — the `redo:` / `why:` capture tags — is here;
`master` itself was not touched). Tags: `<!-- slice: 0100-02 -->`.

## What changed and why
The group «Descărcări multiple» left Setări › Aplicație and became its own Setări page, shown only while the unit allows
multi-thread; the advanced options no longer matter; the tabs limit comes from the unit.
- `contabil.forexe.descarcare-multipla`: intro + «Cum o pornești» rewritten (Setări › Descărcări multiple; «Nu vezi pagina
  în listă?» = not allowed for the unit, ask who administers K-BOT; no advanced options; «Numărul de taburi deodată (1–N)»
  with N = the unit's limit, never above 10; the page disappears at the next connection if the unit stops allowing it);
  the two later capture prepares point to the new page; keywords added.
- `contabil.setari`: `screens:` + `SetariMultithreadView`; new row «Descărcări multiple» in the page table; the group table no
  longer lists it (one sentence points to the topic).
- `avansat`: the bullet about the group removed from «Cum le pornești».
- `tours/tur-setari.md`: new step «Descărcări multiple» (`part: item:multithread`; skipped on its own when the page is not
  in the list). `HelpContent/README.md`: `multithread` added to the `setari:<page>` list.
- Nothing in the help names the server, tables or setting keys (help rule: users only).

## Captures
No picture was taken: it needs K-BOT running on a unit whose `Setari` has `Multithread = 1` (and `Multithread_Max` ≥ 3), i.e.
the SQL applied and `setari.py` deployed.
- Red («de refăcut», `redo: 2026-10-01 23:30`): `setari`, `avansat-activare`, `setari-descarcari-multiple` (new screen).
- Missing, unchanged apart from the way to switch the mode on: `arbore-meniu-actualizare`, `actualizare-multipla`.

## Files touched
`src/KBot.App/HelpContent/` `contabil/forexe/descarcare-multipla.md`, `contabil/setari.md`, `avansat/index.md`,
`tours/tur-setari.md`, `README.md`; status files.

## Test results
`Check-Help.ps1 -Coverage`: the only error is the older `0077-3` tag in `contabil\fereastra.md` (not from this change); the
«without a topic» list holds four windows that were already there (`HelpCaptureViewForm`, `PrintListPage`,
`SetariIstoricView`, `UpdateOfferForm`). Nothing run on screen.

## Unverified / deferred
- The tour step with `part: item:multithread` was not run; if `Check-Help` / the runner rejects a part whose nav item starts
  `Visible = False`, drop `part:` and keep `goto:`.
- `goto: setari:multithread` opens the page only when the unit allows it (the row is hidden otherwise).
