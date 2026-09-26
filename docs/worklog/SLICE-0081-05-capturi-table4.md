# SLICE 0081-05 — the FOREXE captures in the final PDF (G5)

**Date:** 25.09.2026. **Plan:** `docs/PLAN_DDF_Trimitere.md` §0081-05.

## What changed

- **`DdfXmlBuilder.Table4Of(capturi)`** — `SubformSectiuneaB/Subform51/Table4`: the hidden template `Row1`
  (empty `Cell1`) first, then one `Row1/Cell1 xfa:contentType="image/png" href=""` per capture, the PNG as
  base64 text. Shape taken from the template data in `Surse/doc_fund_xdp.xml`. No capture → no
  `Subform51` at all (the template keeps its defaults).
- **`BuildComplete(…, mode, capturi)`** — Final mode only. The `PrtScr = 1` attachments never go into the
  PDF's attachment list in either mode (decision in the plan: Table4 only, the form label says «or as an
  annex», and both would double the size).
- **`DdfPdfGenerator`** (written in 0081-04) gathers the captures in `IdRevAtt` order = the order the
  workflows took them: from `DateFisier` when the old column carries them, else from the attachment store
  (`GET /api/forexe/ddf/att/{id}/imagine`). A capture that cannot be read STOPS the generation — a final
  PDF missing a capture would be signed as complete.
- **`KBot.Xfa/AdobeUtils` — a real fix.** `ModifyXfaFromXml` rebuilt table cells by name and value only:
  `xfa:contentType` was dropped, so an image cell would have come out as a wall of base64 text. Cells now
  copy their attributes (`CopyAttributes`, namespace declarations skipped). Repeated `Row1` nodes were
  already written one by one (checked in `ProcessXmlNodes` before changing anything). `CopyAttributes`
  is `Public` so the tests can reach it.

## Assumptions
1. The XFA template's `Table4/Row1/Cell1` is an image field that takes `image/png` base64 data in the
   data DOM (as `doc_fund_xdp.xml` shows). Not proven until a final PDF is opened in Adobe.
2. Size: a full-page PNG is a few hundred KB; several captures make the PDF several MB. Left as the plan
   says — to watch on the first real document.

## Files touched
- `src/KBot.App/Views/Ddf/DdfXmlBuilder.vb` (`Table4Of`, `DdfPdfMode`, mode parameter).
- `src/KBot.App/Views/Ddf/DdfPdfGenerator.vb` (captures gathering).
- `src/KBot.Xfa/AdobeUtils.vb` (`CopyAttributes`, used for every table cell).
- Tests (written, NOT run): `tests/KBot.App.Tests/DdfXmlBuilderModeTests.vb` (4 new: Table4 shape,
  order, placement, none), `tests/KBot.Xfa.Tests/CopyAttributesTests.vb` (new).

## State
Builds 0 / 0 (App, Xfa). **Done when:** a final PDF opened in Adobe shows the captures under Section B.
