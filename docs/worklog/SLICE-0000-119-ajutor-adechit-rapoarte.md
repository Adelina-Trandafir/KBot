# SLICE-0000-119 - Ajutor pentru filele VERCON / ADECHIT si pagina «ADECHIT Rapoarte» (21.10.2026, felia 0123-01)

## What changed and why
- **`contabil/adechit.md`** (new topic `contabil.adechit`, `screens: AdechitRapoarteForm`): how the page works (what is chosen on the left, the six reports and what each needs, the PDF on screen, «Salveaza Excel»,
  that a button stays grey until everything it needs is chosen, that a problem is told on the line under the report).
- **`contabil/ribbon.md`**: the list of tabs has VERCON and ADECHIT; two table rows (VERCON › Vercon › «Trimite la Vercon»; ADECHIT › ADECHIT › «Rapoarte»).
- **`contabil/efactura/index.md`**: «Trimiterea la Vercon» - the button is in the **VERCON** tab now (was E-Factura).
- **`tutorials/efactura-trimite-la-vercon.md`**: the first step is the **VERCON** tab (`ribbon-tab.vercon`, signal `ribbon-tab:vercon`).
- Left out on purpose: how the server keeps and removes the saved report, the bearer token, the Adobe surface.

## Capturi de refacut
`ribbon-file`, `ribbon-efactura` (E-Factura loses «Trimite la Vercon»; two new tabs), `ribbon-strans` (more tabs). No capture tag added for the new page (a capture needs real report data).

## Test results
`Check-Help.ps1 -Coverage` was run before the status rows existed (it reported the new slice ids as unknown, nothing else of this work). Unrelated old messages are in the output
(`efactura-factura-noua` controls, `ordonantare-din-plata` slice tags, KE12 slice ids): not touched here. No tutorial run on screen.

## Unverified / deferred
- The VERCON tab shows only «Trimite la Vercon» until slice 0123-02 adds «Rapoarte».
- The text of the page was written from the code (captions and tooltips of the Designer), the page was never seen on screen.
