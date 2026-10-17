# SLICE-0000-95 - Help for 00EF-39 (17.10.2026)

## What changed and why
`contabil/efactura/index.md`, section «Recepțiile unei facturi primite»: the step «Primul pas: clasificațiile liniilor» (with the button «Clasificații pe linii…» and its own window) is now «Clasificațiile liniilor»: no separate button, the classifications are chosen in the window opened by any of the three link buttons (angajament on top, the table of lines, «Ca la prima linie»), and they are saved with the link. «Al doilea pas: recepția» is just «Recepția». The front matter no longer lists `KeClasificariForm`; the keyword «clasificatii pe linii» became «clasificatiile liniilor»; the section tag carries `00EF-39, 0000-95`.

## Files touched
`src/KBot.App/HelpContent/contabil/efactura/index.md`.

## Test results
`tools\HelpCheck\Check-Help.ps1 -Coverage`: the only error about this page was the unknown slice `0000-95`, which this worklog and its status row answer; the other errors printed (tutorial `efactura-factura-noua.md` controls, sections without a slice tag in other tutorials) were there before and are not touched.

## Unverified / deferred
Nothing seen on screen. The capture `efactura-receptii` (the «Receptii» view) shows five buttons and has to be redone.
