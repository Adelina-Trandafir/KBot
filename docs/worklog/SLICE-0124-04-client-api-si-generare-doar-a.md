# SLICE-0124-04 — Client: API for the flow, and the generation with section A only

**Date:** 30.10.2026. Server side: [0124-03](SLICE-0124-03-server-flux-si-restrictii.md).

## What changed and why

- **`KBot.Api`**: `IDdfSendApi` gains `GetDdfDeSemnatAsync`, `PornesteFluxDdfAsync`, `GetCompRolAsync`, `SalveazaCompRolAsync` (+ `DdfCompRol`, `DdfCompRolRand`);
  `DdfDeSemnat` carries `DeSemnat`, `Incarcat`, `Preluat`; `DdfDeSemnatLista.Uneste` joins two lists without a revision twice. `ApiClient.DdfSend.vb`: one shared reader
  for the two lists, and the JSON helper takes the HTTP method (GET / PUT as well as POST). `GetDdfRevizieRow.de_semnat` → `RevizieRow.DeSemnat` (+ `InFluxulKbot`).
- **`DdfView.OnGenerateRequested`**: for a revision of the flow (`RevizieRow.InFluxulKbot`, `DeSemnat` set by the server when the revision was created) not yet signed on A: if the role in `DeSemnat` is **the user's own** (a proxy also takes the accountant's) **or the user is the accountant, always** the document is made with
  `DdfPdfMode.OnlyA` on this PC and signed as before; otherwise the operator is told it was handed to that role and nothing is generated. **Nothing is uploaded before the signature.** A revision outside the flow (`DeSemnat` empty: older revisions, units without the flow) → the old interim document, unchanged. After A is signed the sending to FOREXE is the old one, from K-BOT.

## Files touched

`src/KBot.Api/IDdfSendApi.vb`, `ApiClient.DdfSend.vb`, `ApiClient.vb`, `UpsertAngajamenteRequest.vb`; `src/KBot.Domain/DdfInfo.vb`; `src/KBot.App/Views/DdfView.vb`.

## Test results

`dotnet build src\KBot.App` (to a scratch output folder, the app was running): **0 errors**. Nothing run on screen.

## Left unverified / deferred

- Not seen: the generation on a real unit.
- The old «Trimite în FOREXE» menu is untouched: a flow revision signed on A is «Semnat A — gata de trimis» and is sent as before.
