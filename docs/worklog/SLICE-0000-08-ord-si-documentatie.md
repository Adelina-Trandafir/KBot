# SLICE-0000-08…10 — Istoricul ajutorului ORD și al machetelor sale

Consolidare documentară D002-13. 08 descrie inițial vederea/editorul ORD și documentarea întreținerii ajutorului; 09 adaugă semnarea din macheta A1.0.08, iar 10 o corectează pentru A1.0.11. Toate corpurile, inventarele, dovezile și limitele rămân. Ordinea inițială a validării din 09 este istorică; diferența de machetă și corecția postSign din a doua rundă 10 nu sunt confundate.

Fișierul Etapa3 identic cu Etapa1 este constatarea inițială din 10; înlocuirea lui și confirmarea operatorului că serverul servește A1.0.11 sunt consemnate în runda a doua. Nu s-au recitit exporturile, nu s-au schimbat machete, ajutorul runtime sau fișierele de release și nu s-a făcut o nouă probă.

[08](#slice-0000-08) · [09 — A1.0.08](#slice-0000-09) · [10 — A1.0.11](#slice-0000-10)

<a id="slice-0000-08"></a>

## SLICE 0000-08 — ORD in the help + documentation of the help system

**Date:** 30.09.2026. **Standing slice:** 0000 (help). Operator: «let's do ORD as well — what is done is done; I'm not sure about the flow required by the MF» + «this help system should be properly documented, so another thread picks it right off».

### What changed and why

#### ORD topics (Part 1)
- `contabil/vederi/ord.md` rewritten: the tree, the pages «Vizualizare» / «Document», the right-click menu (Adaugă / Modifică / Șterge / Generare în lot) and the footer «Adaugă», «Generează» and signing on «Document», the question before regenerating over a signed PDF. Closing note: what MF requires when an ORD is SENT to FOREXE is not described (K-BOT does not send ORDs yet).
- NEW `contabil/vederi/ord-generare.md` (`contabil.ord.generare`): one ORD per day (the «+» on a day in Plăți, or «Adaugă» + the day dialog `OrdZiuaForm`), the >25 partners warning, «Generare în lot» (the «+» on a month = that month), stop at the first error, deletion and what goes with it.
- NEW `contabil/vederi/ord-editor.md` (`contabil.ord.editor`): header, Beneficiari, Documente justificative (incl. «< TOȚI BENEFICIARII >», `OrdTextForm`), Atașamente (Adaugă / Lipește / Șterge), save validation (all problems at once), all-or-nothing save, images after the save.
- `contabil/vederi/plati.md`: the «+» on days / months.
- Captures: `ord` (prepare updated), new `ord-ziua`, `ord-editor`.
- Source: `KbotForm.Ord.vb`, `Views/OrdView.vb`, `Views/PlatiView.vb`, the Designer files of `OrdEditForm`, `OrdBeneficiariPage`, `OrdDocumentePage`, `OrdAtasamentePage`, `OrdZiuaForm`, `OrdTextForm`; validation strings in `ORD_EDIT/`.

#### Fix seen in passing
- `ORD_EDIT/OrdAtasamentePage.Designer.vb`: the «Lipește» button carried the tooltip of «Șterge» («Șterge imaginea» / «Scoate imaginea...»). Now «Lipește imaginea» / «Adaugă imaginea din memoria temporară...». KBot.App FileVersion not bumped (`push-update.ps1` asks).

#### Documentation for later threads
- NEW `docs/HELP_SYSTEM.md`: what the system is, the parts and who sees them, where every piece lives, how F1 picks a topic, **the step-by-step procedure for updating the help after a change**, what a feature slice must leave behind, writing rules and what is deliberately left out, engine extension points.
- NEW `tools/HelpCheck/Check-Help.ps1` (ASCII, PowerShell 5.1): the static check done by hand until now — headers, ids, parents, links, capture tags, tours, screen keys / tour targets against the code; `-Coverage` lists windows without a topic, `-Map` prints the topic tree (so no hand-kept map goes stale).
- Pointers: `CLAUDE.md` (new «Help (slice 0000)» section), `CODE_WORKFLOW.md` (definition of done item 4: update the help or name the stale topics), `KBOT_STATUS.md` (the locked decision points to the guide), `HelpContent/README.md` (points to the guide).
- `state/KBOT_STATUS_0000-0009.md`: a watermark «Ajutorul e la zi până la» (30.09.2026, slice 0096) and an Open threads line «Ajutor de actualizat» where feature slices leave their notes.

### Files touched
`src/KBot.App/HelpContent/contabil/vederi/{ord,ord-generare,ord-editor,plati}.md`, `src/KBot.App/HelpContent/README.md`, `src/KBot.App/ORD_EDIT/OrdAtasamentePage.Designer.vb`, `docs/HELP_SYSTEM.md`, `tools/HelpCheck/Check-Help.ps1`, `CLAUDE.md`, `docs/worklog/CODE_WORKFLOW.md`, `docs/worklog/KBOT_STATUS.md`, `docs/worklog/state/KBOT_STATUS_0000-0009.md`.

### Test results
`Check-Help.ps1 -Coverage -Map`: 43 topics, 6 tours, 46 capture tags, no errors, coverage «(none)». Build App 0 warnings, 0 errors. Nothing on screen.

### To read (operator)
- ORD signing: the text says «ca documentul de fundamentare» and does not name the ORD's signature fields.
- The generation warning text (>25 partners): described from the code comment and the day dialog's tooltip.

<a id="slice-0000-09"></a>

## SLICE 0000-09 — ORD signing in the help, from the MF form

> **Superseded by 0000-10:** everything below describes the OLD template A1.0.08. The current A1.0.11
> flow is in [0000-10](#slice-0000-10) and `docs/FUNDAMENT_Ordonantare_CAB.md`.

**Date:** 30.09.2026. **Standing slice:** 0000 (help). Operator: `Surse/ord_xdp.xml` is a fully signed ORD with the minimum signatures; once they are on, the document can be turned into a RECEPȚIE and uploaded to the FOREXE CAB server. Also: `Surse/ord_xdp_full.xml` shows what the document does behind the scenes.

### What changed and why
- NEW `docs/FUNDAMENT_Ordonantare_CAB.md` (findings only, English): the table columns and their checks, the two-pass «Validare formular / Generare XML» (hidden flag `StareSemnatura`; the second pass attaches `ORDNT.xml`, shape recorded), the five signature fields with captions, K-BOT roles, opening order and what each manifest locks; the minimum complete set; open questions for the ORD-sending slice.
- `HelpContent/contabil/vederi/ord.md`: «Documentul și semnarea» rewritten — validation (press twice, what each press says), the five signatures in order with who / required, «the ordonator's signature closes the CFP ones» (so CFP goes first), complete = 1 + 2 + 5, and the closing note now names the next step (recepție + upload to the CAB server) as not done by K-BOT yet. +1 capture `ord-semnaturi`. Keywords extended.
- `docs/HELP_SYSTEM.md` §5: the ORD exclusion narrowed to the step after signing.

### Findings worth knowing (details in the FUNDAMENT file)
- The form does NOT force the CFP signatures: after SignatureField2, fields 3, 4 and 5 all open.
- SignatureField5 (Ordonator) locks fields 3 and 4 → a CFP signature must precede the ordonator's.
- K-BOT's generated ORD leaves `StareSemnatura` at 0, so the operator validates twice (unlike the DDF, where K-BOT's placeholders make one «Validează» enough).

### Files touched
`docs/FUNDAMENT_Ordonantare_CAB.md` (new), `src/KBot.App/HelpContent/contabil/vederi/ord.md`, `docs/HELP_SYSTEM.md`, status files.

### Test results
`Check-Help.ps1 -Coverage`: no errors, coverage «(none)». Build App 0 warnings, 0 errors.

### Confirmed by the operator (30.09.2026, second round)
- 1 + 2 + 5 is the signed minimum (screenshot of the signed PDF).
- Validation takes two presses. The operator's test (sign after col. 4 only, reopen, validate) hit the dead end the scripts predict; the help now warns «sign only after the second validation» and gives the two-person order (department: col. 4 + validation #1 → CAB person: cols. 1-3 + validation #2 → signature 1 → signature 2). Recorded in the FUNDAMENT file §2.

### Still to read (operator)
- Who signs field 1 vs field 2 (both «Compartiment de specialitate»): the help gives field 2's own sentence and leaves field 1 as «compartimentul care face plata».

<a id="slice-0000-10"></a>

## SLICE 0000-10 — ORD signing redone for the A1.0.11 template

**Date:** 30.09.2026. **Standing slice:** 0000 (help). Operator: the XDP used in 0000-09 (`Surse/ord_xdp*.xml`) is an OLD version of the form; the new one, step by step, is in `Surse/ETAPE ORDONANTARE/` (three signed PDFs, their XDPs, the full A1.0.11 XDP).

### What changed and why
0000-09 described A1.0.08 (27.01.2026). A1.0.11 (27.05.2026) works differently:
- validation #1 (header + beneficiaries only) opens **signature 1**; signature 1 freezes col. 4 and opens cols. 1-3; validation #2 (full table) opens **signature 2**; signature 2 locks everything;
- new optional blocks «Alte Avize» (6 fields, between signatures 1 and 2) and «Verificat/Avizat» (6 fields, after signature 2, closed by CFP propriu);
- new «Anulare Validare» button, only while no signature is on;
- validation no longer attaches `ORDNT.xml` (the call is commented out); the button caption is now just «Validare formular». So the operator's own idea of the flow (department: col. 4 → validate → sign 1 → hand over; CAB person: cols. 1-3 → validate → sign 2) IS the A1.0.11 flow. The «dead end» of 0000-09 belonged to A1.0.08 only.

Files:
- `docs/FUNDAMENT_Ordonantare_CAB.md` rewritten for A1.0.11 (flow table, manifests, XML note, meaning for K-BOT, a template bug seen in passing); A1.0.08 kept as a short §5.
- `HelpContent/contabil/vederi/ord.md`: «Pașii în Adobe» (validate → sign 1 → validate → sign 2 → CFP → ordonator, who does what), «Anulare Validare», signatures table with the two new blocks, «semnătura 2 blochează tot documentul». The wrong warning of 0000-09 removed.

### Evidence
- `Etapa1/2/3_Semnata.pdf`: 1 / 2 / 3 `/Type /Sig` objects, signed 10:51:19 / 10:52:06 / 10:52:24.
- `Etapa1_xdp` → `Etapa2_xdp`: col. 4 = 1.00 with cols. 1-3 empty and the table open → cols. 1-3 filled and every table field `readOnly`.
- ⚠ `Etapa3_xdp.xml` is byte-identical to `Etapa1_xdp.xml` — wrong file saved (not needed: the PDF and the full XDP cover stage 3).

### Test results
`Check-Help.ps1`: no errors. Build App 0 warnings, 0 errors.

### Unverified / to read (operator)
- Which ORD template version the legacy API serves to K-BOT (`TemplateDownloader`, `ORD_URL`): the help assumes A1.0.11.
- Signature 1's signer is written as «compartimentul care propune plata (coloana 4)».

### Second round (operator, 30.09.2026)
- Server serves A1.0.11 — confirmed. `Etapa3_xdp.xml` replaced with the real stage-3 extract.
- Operator: stage 1 as the form suggests it (col. 4 only → sign 1 → save → reopen) is a dead end. Explained from the files: signature 1's postSign unlocking of cols. 1-3 is in memory only; the signed file ends at signature 1 and its form state has cols. 1-6 at the template default (locked) with the flag at 1 → full-table check that can never pass. Every stage file shows the same «state before the last postSign» pattern. Also: after a reopen, signature 2 opens without the second validation.
- Help (`ord.md`): the step table now describes a K-BOT document (table complete → validate + sign 1; validate + sign 2, possibly another person / PC / day; CFP; ordonator), a warning «signature 1 only on a complete table», and what the two stages are for (responsibility for col. 4 vs cols. 1, 2, 3, 5). FUNDAMENT §2 got the ⚠ section with the evidence.
