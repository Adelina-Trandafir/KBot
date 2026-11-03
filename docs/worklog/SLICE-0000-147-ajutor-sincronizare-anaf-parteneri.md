# SLICE-0000-147 - help for the ANAF synchronisation of the partners

Topic `contabil.nomenclatoare.parteneri` (`src/KBot.App/HelpContent/contabil/nomenclatoare/parteneri.md`), slice 0093-02 (03.11.2026): new section «Sincronizarea partenerilor cu ANAF»
with the tag `0093-02, 0000-147`; keywords `sincronizare`, `sincronizeaza anaf`, `selecteaza parteneri` added. It tells the steps (button, tick boxes, the three quick choices of the list header, «Sincronizează ANAF»),
what is written (name and address only), which fiscal codes can be synchronised (plain number of 6-12 digits, grey locked box otherwise) and what happens when ANAF is down.
Nothing about the old Access system, the migrator or inner workings.

## Capturi de facut
- `parteneri-anaf` (new tag in the same section; `goto: menu:parteneri`, `prepare` in the tag): the list in choosing mode with a few ticks and the header menu open. **Operator takes it.**
- `parteneri` (existing): the ribbon's form group has a new button now; if that capture includes the ribbon it is out of date (not verified, no `redo:` written).

## Test results
`tools\HelpCheck\Check-Help.ps1` run; the lines about other files were there before (other tours/tutorials); none about `parteneri.md` except the pre-existing step `chkFaraActivitate` of `tutorials\parteneri.md` (not touched).
