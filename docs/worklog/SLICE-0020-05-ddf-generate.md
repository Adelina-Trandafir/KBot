# SLICE-0020-05 — DDF PDF generation (un-gated parts)

Pass 5 of slice 0020 (vederea DDF), per `PLAN_DdfView.md` §8 bis. **Scope decision (operator):**
build the parts that do not depend on the live machete download; leave step 05-00's live
verification and the real-PDF verdict for a host run. **Pass 06 (signing) deferred to slice 0021**
(operator decision).

## What changed and why

### Step 05-00 (client side)
- `src/KBot.Xfa/Config.vb`: `BASE_URL` `http://adcredit.avatarsoft.ro:5008/api/` →
  `https://kbot.avatarsoft.ro/api/`. Corrected the stale comment (the machete endpoint does
  **not** live on a separate "old server" — `mfp_bp` is registered in the *same* Flask app,
  confirmed at `main.py:52`; the two hostnames are two front doors onto one app). `FileVersion`
  1.1.0.0 → 1.2.0.0. The `X-API-KEY` **stays** — a deliberate deferral, logged as an open thread
  in STATUS (removing it means `@require_session` on the two `mfp` routes + a bearer token into
  `TemplateDownloader`, which also touches ORD).
- **Server side confirmed present (offline):** `routes/mfp.py:177` serves `/api/mfp/template_ddf`
  for **both GET and HEAD** under `@require_api_key`, from the server's own cache. So
  `TemplateDownloader`'s HEAD-then-GET is supported. **The live GET/HEAD over 443 (and whether
  nginx routes `/api/mfp/*` on that vhost) is NOT verified here — that is step 05-00's host run.**

### Endpoint extension (one route, `GET /api/forexe/ddf`)
- `linii` gains `ss` — `COALESCE(NULLIF(SA.SS,''), Clasificatii.SS by IDClsf)`. The XML builder
  needs it for `Cell3` (form1) and `codSSI` (NOTAFD). **Finding:** MariaDB `Clasificatii` has **no
  `CodSSI`** column (the VBA reads Access `Clasificatii.CodSSI`) — it has the generated `SS`
  (Sector+Sursa), which is the analog. One `ss` field serves both the form1 and NOTAFD needs.
- `sectiuneb` (FX_DDF_REV_SB) and `atasamente` (FX_DDF_REV_ATT) arrays, **opt-in behind a single
  `?pentru_generare=1` flag**. Both are generation-only (§2.8: the view must not carry data it
  never shows). **Deviation from the plan**, which proposed a separate `?atasamente=1` for
  attachments only — one flag is cleaner and avoids a half-loaded generation payload; documented
  in the route.

### The XML builder — `DdfXmlBuilder` (pure, the crown jewel of the pass)
A faithful port of the three module functions, generating **per revision** (as the Access
`tmpFX_*` tables hold one revision):
- `BuildFormXml` ← `GenereazaXML_PentruPython`: `form1` → `SubformAntet`, `SubformSectiuneaA`
  (`Subform123` + `Subform4/Table1` with the **dummy first `Row1`** + real rows `Cell1..6`,
  `Subform5`), `SubformSectiuneaB/Table3` (dummy row + rows `Cell1..6,8,9` — **`Cell7` skipped**).
- `BuildNotafdXml` ← `GenereazaXML_NOTAFD`: the namespaced `NOTAFD` with `sectiuneaA`
  (`rowT_ang_pl_val`) and `sectiuneaB` (`rowT_ang_ctrl_ang`), sums **computed** (anterior+influență).
- `InsertAttachments` ← `InsereazaAtasamente`: the `<Attachments>` node with `NOTAFD.xml`
  (base64) + the already-base64 `FX_DDF_REV_ATT` rows.

Fidelity details ported and pinned by tests: `ToXmlNum` (comma→dot, no forced decimals),
`AdaugaSubnod`'s empty-element omission, the dummy-row skip, `Int()` = **floor** (not toward
zero), the `Left(…,N)` field widths, and §2.9 (program from the session global, not
`FX_DDF.Program`).

**Two deliberate deviations from a literal VBA port, both documented in the code:**
1. **NOTAFD elements are namespace-qualified.** The VBA used `createElement` (no namespace) for
   children, producing elements in *no* namespace — **invalid** against `notafd_v0.xsd`, which is
   `elementFormDefault="qualified"`. The port qualifies every element so the output validates.
   (Confirmed against the XSD, not guessed.)
2. **NOTAFD base64 is UTF-8, not ANSI.** The VBA's `EncodeBase64` used `StrConv(…, vbFromUnicode)`
   = system-ACP bytes. Without a real DDF `.xml` to compare (the repo's `ddf_demo.xml` is actually
   ORD — open thread), the correct codepage can't be decided; UTF-8 is lossless for Romanian text
   whereas ANSI truncates out-of-codepage characters. **Flagged as an unverified fidelity risk.**

### The generation flow — `DdfView.OnGenerateRequested`
`Async Sub` (UI boundary: log + swallow): fetch `GetDdfAsync(cod, pentruGenerare:=True)` → filter
to the selected revision's rows → `DdfXmlBuilder.BuildComplete` → write the sibling `.xml` under
the partener/`GENERAL` folder (created on demand) → `Await Task.Run(XfaWriter.Genereaza(...))` on a
background thread → refresh browser + preview from disk. **No write-back to the DB** (§2.4 — the
four columns don't exist; existence is the disk scan). Guarded against double-invocation. `DdfView`
now takes an optional `SessionContext` (for the unit globals); `MainForm` passes `_session`.

## Files touched

- `src/KBot.Xfa/Config.vb`, `src/KBot.Xfa/KBot.Xfa.vbproj` (FileVersion).
- `PYTHON/routes/forexe/ddf.py` — `ss` on linii, `sectiuneb`/`atasamente` opt-in, `_truthy`.
- `PYTHON/tests/test_forexe_ddf.py` — updated key-sets + 5 new tests (ss, flag on/off, sectiuneb,
  atasamente).
- `src/KBot.Domain/DdfInfo.vb` — `LinieSaRow.SS`, `SectiuneBRow`, `AtasamentRow`, `DdfInfo` lists.
- `src/KBot.Api/UpsertAngajamenteRequest.vb`, `IApiClient.vb`, `ApiClient.vb` — wire DTOs +
  `pentruGenerare` flag + mapping.
- `src/KBot.App/Views/Ddf/DdfXmlBuilder.vb` — **new**.
- `src/KBot.App/Views/DdfView.vb` — generation flow; `src/KBot.App/MainForm.vb` — passes `_session`.
- Test fakes in the four other `*ViewTests` + `DdfViewTests` updated for the new signature.
- `tests/KBot.App.Tests/DdfXmlBuilderTests.vb` — **new**, 20 structural tests.
- `tests/KBot.Api.Tests/ApiClientTests.vb` — 2 generation-flag tests.

## Test results

- **.NET** — `dotnet build KBot.sln`: **0 errors, 0 BC warnings** (only pre-existing NU1701).
  `dotnet test KBot.sln`: **415 passed, 0 failed** (App 120, Api 63, Controls 134, Theming 27,
  Xfa 39, Domain 17, Common 14, LocalStore 1).
- **Python** — `pytest tests/`: **75 passed, 14 skipped, 0 failed** (the DDF tests skip off-host).

## Anything left unverified or deferred

1. **Step 05-00 live verification NOT done** — `GET`/`HEAD` on
   `https://kbot.avatarsoft.ro/api/mfp/template_ddf` with the `X-API-KEY`, over 443. The route and
   HEAD support exist in the Flask app (offline-confirmed), but whether nginx routes `/api/mfp/*`
   on the 443 vhost and a template sits in the server cache is unverified. **Run this on the host
   before trusting generation** — if the machete can't be fetched, `XfaWriter.Genereaza` fails at
   `TemplateDownloader`.
2. **No real PDF has been produced or opened.** `XfaWriter.Genereaza` is wired and called on a
   background thread, but it needs the machete + iTextSharp against a real template; the "Adobe
   Reader opens the generated PDF correctly" verdict (0019 open items a/c, and this slice's DoD)
   is **still open**.
3. **No structural diff against a known-good Access `.xml`** — impossible from the repo (the DDF
   sample is really ORD; open thread). The builder is pinned by 20 structural tests against the
   *module source*, not against a real artifact. Capture a real Access-written DDF `.xml` on the
   host and diff it.
4. **NOTAFD base64 ANSI-vs-UTF-8** (see above) — unverified fidelity risk.
5. **The generation UI has no busy bar.** The plan wanted "the busy bar up"; `DdfView` has no
   handle to the shell's busy bar, so it only guards against re-entry (`_generating`). A visible
   progress indication is a follow-up.
6. **Pass 06 (signing) deferred to slice 0021** (operator decision). `Semnatura` stays carried but
   unused; the write-back route is not built.
