# SLICE-0124-04 — Client: API for the flow, and the generation with section A only

**Date:** 30.10.2026. Server side: [0124-03](SLICE-0124-03-server-flux-si-restrictii.md).

## What changed and why

- **`KBot.Api`**: `IDdfSendApi` gains `GetDdfDeSemnatAsync`, `PornesteFluxDdfAsync`, `GetCompRolAsync`, `SalveazaCompRolAsync` (+ `DdfCompRol`, `DdfCompRolRand`);
  `DdfDeSemnat` carries `DeSemnat`, `Incarcat`, `Preluat`; `DdfDeSemnatLista.Uneste` joins two lists without a revision twice. `ApiClient.DdfSend.vb`: one shared reader
  for the two lists, and the JSON helper takes the HTTP method (GET / PUT as well as POST). `GetDdfRevizieRow.de_semnat` → `RevizieRow.DeSemnat` (+ `InFluxulKbot`).
- **`DdfView.OnGenerateRequested`**: flow revision (`RevizieRow.InFluxulKbot`, server-set `DeSemnat` at creation) unsigned on A generates locally with `DdfPdfMode.OnlyA` for **the addressed role** (proxy includes accountant) **or always for the accountant**; otherwise a message names the addressed role, without generation. Signing stays as before, **without upload before signature**. Empty `DeSemnat` (older revisions/units without flow) keeps the old interim document. After A, FOREXE sending remains through K-BOT as before.

## Files touched

`src/KBot.Api/IDdfSendApi.vb`, `ApiClient.DdfSend.vb`, `ApiClient.vb`, `UpsertAngajamenteRequest.vb`; `src/KBot.Domain/DdfInfo.vb`; `src/KBot.App/Views/DdfView.vb`.

## Test results

`dotnet build src\KBot.App` (to a scratch output folder, the app was running): **0 errors**. Nothing run on screen.

## Left unverified / deferred

- Not seen: the generation on a real unit.
- The old «Trimite în FOREXE» menu is untouched: a flow revision signed on A is «Semnat A — gata de trimis» and is sent as before.
