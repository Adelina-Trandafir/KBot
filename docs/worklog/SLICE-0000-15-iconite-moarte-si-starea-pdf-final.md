# SLICE-0000-15 — Dead footer icons removed + «PDF final — de semnat B» (30.09.2026)

Operator answers to 0000-14: the three footer icons with no action are **taken out**; the state label
is **renamed**.

## What changed and why

- `Views/PlatiView.Designer.vb`: tree footer right icon («Descarcă plățile/încăsările din CAB.») and
  its caption «Actualizează» removed. The footer band stays (empty), level with the grid's TOTALURI.
- `Views/IstoricView.Designer.vb`: tree footer right icon («Reîncarcă istoricul angajamentului de la
  server.») removed; the collapse button and «Perioade» stay.
- `Views/DdfView.Designer.vb`: tree footer «+» («Adaugă D.D.F. / Revizie») and its caption «Adaugă»
  removed; the collapse button stays. DDF additions are made from the Rezervări tree.
  None of the three had a click handler, so no code behind them changed.
- `KBot.Domain/DdfRevisionState.vb`: `Label(FinalToSign)` «PDF final — de semnat A și B» →
  **«PDF final — de semnat B»** (since 0078-06 section B goes into the document signed on A; only
  B is signed). XML doc of `FinalToSign` corrected.
- Help: `contabil.ddf` state table row, `tur-ddf` step «Reviziile documentului»; source tags on
  `contabil.vederi.istoric` / `contabil.vederi.plati` (the removed icons were already out of the help
  since 0000-14).

## Files touched

- `src/KBot.App/Views/{PlatiView,IstoricView,DdfView}.Designer.vb`
- `src/KBot.Domain/DdfRevisionState.vb`
- `src/KBot.App/HelpContent/contabil/ddf/index.md`, `contabil/vederi/{istoric,plati}.md`, `tours/tur-ddf.md`
- status: `state/KBOT_STATUS_0000-0009.md`, `KBOT_STATUS.md` index

## Test results

- `Check-Help.ps1 -Coverage`: **No errors**, coverage «(none)».
- `dotnet build src/KBot.App/KBot.App.vbproj`: **0 warnings, 0 errors**. No tests, nothing on screen.

## Left unverified or deferred

- Designer files edited by hand (three / two / three property lines deleted); not opened in the VS
  designer.
- FileVersion of KBot.App / KBot.Domain not bumped (`push-update.ps1` asks).
- Capture to re-shoot if it shows the old footer or label: `plati`, `istoric`, `ddf-vedere`.
