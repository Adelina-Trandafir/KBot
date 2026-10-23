# SLICE-0000-122 - Help: «Trimiterea la Vercon», one account for all lines and one supplier at a time (23.10.2026)

Follows slice 00EF-49 (see `SLICE-00EF-49-vercon-cont-pentru-toate-liniile-un-furnizor.md`).

## What changed and why
- `contabil/efactura/index.md`, section «Trimiterea la Vercon»: ticking is for one supplier at a time (a month has no tick; a second supplier takes the first one's ticks off), the list «Cont pentru toate liniile» beside the note writes one account on every row of the grid, the note box is named «Notă».
- Tutorial `tutorials/efactura-trimite-la-vercon.md`: steps «Bifează facturile» and «Contul din Vercon» say the same; the step «Nota pentru Administrator» only carries the new slice tag.

## Files touched
`KBot.App/HelpContent/contabil/efactura/index.md`, `KBot.App/HelpContent/tutorials/efactura-trimite-la-vercon.md`.

## Test results
`tools\HelpCheck\Check-Help.ps1`: the new slice tags resolve; the remaining messages are older ones (other tutorials, KE12 tags).

## Unverified / deferred
- Nothing seen on screen; the tutorial step `target:` names (`VerconForm.gridLinii`, `VerconForm.txtNota`) are unchanged.
