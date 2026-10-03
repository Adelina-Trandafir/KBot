# SLICE-0000-43 — help for 0107: right click works on the selected row only

Operator rule (30.09.2026): any change the operator sees is recorded in the help with its slice.
Slice 0107 makes the DDF and ORD trees ignore a right click on a row that is not selected.

## What changed and why

Text only, tag `0107` added to every section touched:

- `contabil.ddf.index` «Vederea «Fundamentare»» - the tree line says the right click works on a row
  already chosen (left click first).
- `contabil.ddf.trimitere` (header section) - «alege revizia cu clic stânga, apoi clic dreapta».
- `contabil.vederi.ord` «Comenzile» - same sentence before the table (table got its own lead-in
  «Comenzile:»).
- `contabil.vederi.ord-generare` - «O ordonanțare, pentru o zi», «Generare în lot», «Ștergerea».
- Tours: `tur-ddf` step «Clic dreapta pe o revizie», `tur-ord` step «Clic dreapta pe arbore».
- Asocieri (same day, after the AsociereForm trees got the property): `contabil.asocieri.cazuri` (lead), `contabil.asocieri.pasi` («Înapoi în coș»), tour step «Clic dreapta» in `tur-asocieri`.

## Files touched

`src/KBot.App/HelpContent/contabil/ddf/index.md`, `.../ddf/trimitere.md`, `.../asocieri/cazuri.md`, `.../asocieri/pasi.md`, `.../tours/tur-asocieri.md`,
`.../vederi/ord.md`, `.../vederi/ord-generare.md`, `.../tours/tur-ddf.md`, `.../tours/tur-ord.md`,
`src/KBot.App/HelpContent/help-version.txt` (2026-10-03).

## Test results

`Check-Help.ps1 -Coverage`: the `0107` tag errors cleared once the slice was in the status; two
older errors remain and are not from this change (`0077-3` in `contabil\fereastra.md`, `0000-39`
in `asocieri\index.md` and `forexe\lista.md`: not in `KBOT_STATUS.md`). Build `KBot.App`: 0
warnings, 0 errors.

## Left unverified / deferred

- No picture changes, none to re-shoot (the existing captures show the menu, not how it is reached).
- Not seen on screen.

## Old Check-Help errors cleared in the same slice (operator, 03.10.2026)

- `0077-3` in `contabil\fereastra.md`: there is no slice 0077-3 (the id was never in the registry and
  has no worklog; it was added by a commit that also added `0777-02`, so most likely a typo). Removed
  from the tag; the facts of that section stay covered by its other tags. If the operator knows what
  it meant, add the right id back.
- `0000-39` in `asocieri\index.md` and `forexe\lista.md`: the sub-slice had text and tags but no
  worklog and no registry row. Written now: `SLICE-0000-39-ajutor-lista-sursa-si-denumire-asocieri.md`
  plus its row in `state/KBOT_STATUS_0000-0009.md`; the index line of 0000 now reads «0000-01…43 GATA».
- Result: `Check-Help.ps1 -Coverage` → «No errors.» (exit 0). It still lists six windows without a topic
  (`HelpCaptureViewForm`, `IstoricIntervalForm`, `PrintListPage`, `SetariAccessView`, `SetariIstoricView`,
  `UpdateOfferForm`): not errors, and not touched. `0000-42` (help of 0105) has no worklog or registry
  row either; the checker does not flag it because no tag uses it.
