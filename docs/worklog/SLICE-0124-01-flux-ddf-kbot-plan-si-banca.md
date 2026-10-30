# SLICE-0124-01 — The K-BOT-made DDF flow: plan and the bench with captures

**Date:** 30.10.2026. **Operator request** (chat, 30.10.2026). Family: 0124. Next sub-slice: 0124-02.

## What changed and why

The operator proved in the bench (`DdfFluxServerHarnessForm`, Debug only) that a DDF built with section A ONLY can be signed on A, kept
on the server, read back, get section B written into it, be signed on B, kept, read back and be signed by the director — each step
ending with the upload, Adobe closed and the local copies deleted (log of 30.10.2026, 16:44-16:46: three uploads, sha identical to
what was sent, A's signed bytes kept as the prefix of the next version, the director's signature covers the whole document).
They want this flow applied in the real application **for DDFs created in K-BOT** (the DDFs that come from FOREXE stay as they are).

### The flow (operator, 30.10.2026) — AUTHORITATIVE, dictated again after the first attempts; do NOT change it without asking him expressly

1. The accountant creates the revision. STOP — that is all he does there.
2. Whoever must sign section A (the role of the document's compartment) FIRST GENERATES the document (on their own PC), then signs it, saves, and it goes to the server.
3. The accountant or the FOREXE proxy sends the document to FOREXE (the existing steps of changing the reservation — K-BOT's own robot; nobody works directly in FOREXE).
4. K-BOT does what it has to and also saves the captures and attaches them to the reservation.
5. AFTER the reservation process has ended (K-BOT did its part AND took back the history that proves the upload) section B is generated AUTOMATICALLY — the DATA only, nothing in the PDF yet.
6. The accountant or the proxy, AFTER FOREXE answered OK — i.e. after the reservation exists in the reservations table the program downloads by itself after writing the data to FOREXE (that flow already exists) — gets the option to sign signature B. Signs and sends to the server.
7. The director finds the document to sign and closes it.

**How the code follows the seven steps (30.10.2026, after the operator's «FA-LE»):**
- Step 1: the revision enters the flow when it is **created** (the server's save sets `DeSemnat` from the compartment; a compartment with no role stops the save with a message naming it; a unit without sql/0124 saves the old way). Nothing else happens at creation.
- Step 2: «Generează» on a revision of the flow not yet signed on A generates the document (section A only) for the role that signs A **and ALWAYS for the accountant, whatever the compartment** (operator: «contabilul e rege in fluxul asta»); anyone else is told whom it is addressed to; the Secretar / Administrator generate in their own window. Nothing is stored on the server before the signature.
- Steps 3-5: the existing sending (`KbotForm.DdfSend`): the robot sends, K-BOT reads the reservations back, saves the real codes into the section B rows (= section B's DATA, no PDF yet), brings the reservations into K-BOT's table (the history that proves the upload) and only then sets the stage «sent». Not changed.
- Step 6: for a revision of the flow, `DdfSectionBInsert.IsCandidate` also requires that a reservation linked to the revision exists in the table (`AreRezervari`); only then B is written into the document signed on A and the option to sign B is there.
- Step 7: the director's list (flow list + the older one).

### Decisions already taken

- **New column `DeSemnat`** on the DDF revision (`FX_DDF_REV`): the ROLE of the person who has to sign next. Filled when the document
  is generated, from the document's compartment; moves on with every signature (A → the role that signs B → Director → empty).
  **Existing revisions keep it empty: they are not in the flow** (and so not in any tray list).
- **Compartment → role link:** a NEW TABLE PER UNIT (compartment → role), edited from a window in «Setări». `FX_DDF.Comp` stays free text.
- **Step 4's trigger:** the import seeing the `(REV:n)` reservation, not the end of the K-BOT workflow.
- **Tray:** the list extends from the director to every role that has something in `DeSemnat`.
- Section A alone needs no placeholder section B: the form's «Valideaza» let the signature through (bench, template of 06.10.2026).
  The old comment in `DdfXmlBuilder.BuildComplete` (28.09.2026, «with B empty Valideaza never enables the signature») does not hold
  for that template.

### CRITICAL requirement (operator, 30.10.2026): Secretar / Administrator see ONLY what is addressed to them

A `Secretar` or `Administrator` gets a narrowed K-BOT (like the director's) in which they see **only the documents whose `DeSemnat` is their own role,
and NOTHING else**. The operator marked this CRITICAL. Consequence for the design: **it is enforced on the SERVER, not only in the window.**
(The director's narrowing is today only in the window — DIR1 «restanțe: restricția e doar în aplicație» — so this is a new, stricter rule.)

Server design (0124-03), proposed:

- **Deny by default for the restricted roles.** In `routes/auth/guard.py` (`_resolve`, the one place every bearer route passes through) a session whose
  role of the unit logged into (`g.session.ctx["Role"]`) is `Secretar` or `Administrator` may reach only an allow-list of endpoints; every other route
  answers 403. A new route can never leak by being forgotten, because the list is of what IS allowed.
- **Allow-list** = the login / session routes, plus new routes of the flow only: the list «de semnat» for the role; the data of ONE revision to generate
  the document from (header + section A of that revision only); that revision's PDF GET / PUT. Each of the last three checks `DeSemnat = role` on that
  revision (else 403/404). The existing `GET /api/forexe/ddf` (all revisions of an angajament) stays closed to them.
- **Window:** a narrowed shell for these roles (list of documents to sign; open one; sign) — the same pattern as `DirectorMode`; the server rule, not the
  window, is the guarantee.

Defaults I propose (to be confirmed): (a) `DeSemnat` is set when the accountant FIRST generates the document (a new server call, once per revision, only for
a K-BOT-made revision not yet signed), from `FX_DDF_COMP_ROL` by the compartment; a compartment with no role makes that call refuse with a message naming it.
(b) After A: `DeSemnat = 'Contabil'` and an `Imputernicit` also sees what is addressed to `Contabil`; after B: `Director`; after Ordonator: empty.

### Sub-slices (in this order)

| Sub-slice | What | State |
|-----------|------|-------|
| 0124-01 | This plan; the bench carries captures in step 2 (sample files or the revision's own PrtScr attachments) | done on code |
| 0124-02 | SQL: `FX_DDF_REV.DeSemnat` + the compartment → role table (the operator runs it, never me); roles `Secretar`, `Imputernicit` | run by the operator on SURSA → [0124-02](SLICE-0124-02-sql-desemnat-comp-rol.md) |
| 0124-03 | Server: fill / move `DeSemnat`; mapping routes; the per-role list for the tray; the restricted roles | done on code → [0124-03](SLICE-0124-03-server-flux-si-restrictii.md) |
| 0124-04 | Client: DTO + API; the real DDF view generates with `DdfPdfMode.OnlyA` | done on code → [0124-04](SLICE-0124-04-client-api-si-generare-doar-a.md) |
| 0124-05 | «Setări» window for the compartment → role table; role `Imputernicit` for KBotAdmin | done on code → [0124-05](SLICE-0124-05-setari-comp-rol-si-roluri.md) |
| 0124-06 | List / tray for every role; the narrowed window of Secretar / Administrator | done on code → [0124-06](SLICE-0124-06-lista-pentru-toate-rolurile-si-fereastra-ingusta.md) |
| 0124-07 | Step 4 (section B + captures) | NO CHANGE: the old logic after K-BOT's own sending stays → [0124-07](SLICE-0124-07-sectiunea-b-la-vederea-rezervarii.md) |
| 0124-08 | Help (0000-NN) for every visible change | done → [0124-08](SLICE-0124-08-ajutor.md) |
| 0124-09 | Corrective found in the bench: the token's PIN window is brought to the front by the Adobe trap | done on code → [0124-09](SLICE-0124-09-fereastra-pin-in-fata.md) |

## Files touched (0124-01)

- `src/KBot.App/HarnessTests/DdfFluxServerHarnessForm.vb` — `btnCapturi` (sample captures), `CapturiAsync` (sample files, else the
  revision's PrtScr attachments from the server), captures written into section B's `Table4` for both the server-rows path and the
  typed-code path.
- `src/KBot.App/HarnessTests/DdfFluxServerHarnessForm.Designer.vb` — `btnCapturi`, `dlgCapturi`.

Earlier in the same thread (no slice number then, recorded here so nothing is lost): `DdfFluxServerHarnessForm` + `DdfFluxServerHarnessTest`
(new bench); `DdfPdfMode.OnlyA` in `DdfXmlBuilder` (section A only: option 1 of B explicitly unticked, no rows, NOTAFD likewise);
`TemplateDownloader.UseLocalCache` (False in both benches: the template always comes from the server); `XfaWriter.Genereaza` returns
`XfaResult.Nereusit()` and the callers stop quietly when there is no signed-in session (a message, no exception).

## Test results

`dotnet build src\KBot.App` (output redirected, the app was running): **0 errors**. Nothing run, no test suite run, nothing seen on screen.

## Left unverified / deferred

- Captures in the bench: run by the operator on 30.10.2026 (17:29-17:30, one 222,813-byte sample image written into section B): the
  incremental update kept A's signed bytes, «Valideaza» on B accepted it, B was signed and uploaded (570,136 → 1,080,085 bytes), and
  the operator reports the image shows correctly. Not tried: several images, the revision's own PrtScr attachments from the server.
- The table `KBOT_BANC_PDF` was dropped from `000_DEMO` by a schema sync on 30.09.2026 (`schema_diff_log` 3875); recreated by hand on
  30.10.2026. It is not in the model the sync follows, so the next sync can drop it again.
- Roles in the dump are `Administrator` and `Contabil` (and `Director` in code); the «secretary» and the «FOREXE proxy» of the flow have no
  role of their own yet — to decide in 0124-02/03.
- The flow was run by one person who signed A, B and Ordonator; different signers on different PCs, the real upload route (sha
  precedent, 409) and the revision's state moves are NOT tested.
