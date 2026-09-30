# Ordonanțare de plată (ORD) — what the MF form does, and what a complete document is

**Status:** findings only, no code. Written 30.09.2026 (slices 0000-09 / 0000-10, while documenting
ORD in the help). **Template: A1.0.11** (`OrdonantareDePlata_2026_05_27_011`). The older A1.0.08
(27.01.2026) behaves differently — see §5; do not reason from it.

**Sources (operator, 30.09.2026):** `Surse/ETAPE ORDONANTARE/` —
- `Etapa1_Semnata.pdf` + `Etapa1_xdp.xml`: after validation #1 and **signature 1** (10:51:19);
- `Etapa2_Semnata.pdf` + `Etapa2_xdp.xml`: after validation #2 and **signature 2** (10:52:06);
- `Etapa3_Semnata.pdf`: after the **ordonator's signature 5** (10:52:24) — the complete document;
- `Etapa3_fullXDP.xml`: the whole A1.0.11 template (scripts, manifests, captions).
`Etapa3_xdp.xml`: the form state saved with signature 5 (replaced by the operator 30.09.2026 — the first copy was a duplicate of Etapa1).
The PDFs hold 1 / 2 / 3 signatures (`/Type /Sig` count, signing times above).

Operator: **once signatures 1 + 2 + 5 are on, the document can be turned into a RECEPȚIE and uploaded
to the FOREXE CAB server.** That step is NOT built and NOT settled yet.

---

## 1. The table (per `SubformInf`, one per beneficiary page)

| Col. | Cell | Header | XML attribute (`rowTfd`) |
|------|------|--------|--------------------------|
| 1.1 | Cell1 | Cod angajament (11 chars) | `cod_angajament` |
| 1.2 | Cell2 | Indicator angajament (3 chars) | `indicator_angajament` |
| 1.3 | Cell3 | Program (10 chars; `0000000000` skips the program/sector check) | `program` |
| 1.4 | Cell4 | Cod SSI (15: sector 01-05, source letter from the list, 12 digits) | `cod_SSI` |
| 2 | Cell5 | Recepții (≥ 0) | `receptii` |
| 3 | Cell6 | Plăți anterioare (≥ 0) | `plati_anterioare` |
| 4 | Cell7 | Suma ordonanțată (may be negative, not beyond col. 3 in absolute value) | `suma_ordonantata_plata` |
| 5 | Cell8 | Recepții neplătite = 2 − 3 − 4 (≥ 0) | `receptii_neplatite` |

Header: DenInstPb, cif (checked), NrOpl, DataOpl — required. Beneficiary: Beneficiar,
DocumenteJustificative, InfPvPlata required; CIF and RO IBAN checked when filled.

## 2. The flow A1.0.11 enforces (two people, two validations, two signatures)

A hidden flag `SubformAntet.StareSemnatura` (0 / 1) and the signature events drive it:

| Step | Who | Action | What the form does |
|------|-----|--------|--------------------|
| 1 | Compartiment de specialitate | header, beneficiaries, rows, **col. 4 only** (cols. 1.1-3 locked) → **«Validare formular»** | flag 0: checks header + beneficiaries only; «Validarea s-a terminat cu succes! Semnati formularul pentru a completa coloanele 1-3 din tabel apoi validati formularul pentru a putea aplica urmatoarea semnatura»; opens **SignatureField1**, flag → 1 |
| 2 | Compartiment de specialitate | **signature 1** → save, hand over | postSign: freezes col. 4, opens cols. 1.1-1.4, 2, 3; opens «Alte Avize» (6 fields); «Completati coloanele 1-3 ale tabelului, validati, apoi aplicati semnatura» |
| 3 | Person with access to the control system | cols. 1.1-1.4, 2, 3 (col. 5 computes) → **«Validare formular»** | flag 1: full check incl. the table; «Validarea s-a terminat cu succes!»; opens **SignatureField2** |
| 4 | same person | **signature 2** | postSign: opens CFP 3 / 4, ordonator 5 and «Verificat/Avizat» (6 fields); closes «Alte Avize» |
| 5 | (optional) | Verificat/Avizat 1-6, CFP propriu (3), CFP delegat (4) | signature 3 closes «Verificat/Avizat» |
| 6 | Ordonator de credite | **signature 5** | locks 3, 4 and «Anulare Validare» |

### ⚠ The flow above only works in ONE Adobe session (operator's test + the files, 30.09.2026)

The template's defaults are cols. 1.1-3 `access="readOnly"` and col. 4 open. Signature 1's
`postSign` script opens cols. 1-3 and freezes col. 4 **in memory only**: a signature saves the file
at the moment of signing, and a postSign change reaches the file only if something is saved after.
Proof: `Etapa1_Semnata.pdf` ends exactly at signature 1's ByteRange (0..255217 = file size) and its
form state (`Etapa1_xdp.xml`) has cols. 1-6 without any `access` → template default (locked); the
flag `StareSemnatura` = 1 was saved (it was set by the validation, before signing). Every later file
shows the same: each saved state is the one BEFORE the last signature's postSign (Etapa2 still has
«Alte Avize» open; the post-signature-2 state only lands in Etapa3, saved by signature 5).

So: sign 1 after col. 4 alone, save, close, reopen → cols. 1-3 locked and empty, flag 1 → «Validare
formular» runs the full table check («Nu ati completat coloana ,Cod SSI, randul 1» / «Nu se accepta
valori negative in coloana 5 Receptii neplatite -1.00») and nothing can unlock them («Anulare
Validare» refuses once a signature exists). The operator's own run worked only because cols. 1-3
were typed in the same session, a minute after signature 1.

**Conclusion (operator + analysis):** stage 1 is finished only when the WHOLE table is filled.
The two stages then split RESPONSIBILITY, not data entry: signature 1 = the department answers for
col. 4; signature 2 = the nominated person answers for cols. 1, 2, 3, 5, after the full-table
validation. K-BOT documents arrive with every column filled, so they follow exactly this and can
move between people / PCs / days between signatures 1 and 2.

Also seen: on reopening after signature 1, SignatureField2's `initialize` opens signature 2 (and
3, 4, 5) without a second validation. The full-table check is therefore not enforced after a
reopen — the help tells the operator to press «Validare formular» before signature 2 anyway.

**«Anulare Validare»** (button under «Validare formular»): only while NO signature is on the form —
flag back to 0, col. 4 editable again, cols. 1.1-3 locked («Am anulat validarea, puteti edita
coloana 4...»). With any signature: «Am semnaturi aplicate nu pot anula validarea!!!!».

**Minimum complete document: signatures 1 + 2 + 5** (confirmed on `Etapa3_Semnata.pdf`) → K-BOT
`Semnatura = "AB,Ordonator"`.

### What each signature locks (manifests)
- **A (field 1):** only itself, the col. 4 footer, the XML buttons, «Compartiment de specialitate»,
  universalCode. The table stays editable by script: col. 4 frozen, cols. 1-3 opened.
- **B (field 2):** everything — header, whole table (cols. 1.1-5, footers), beneficiaries,
  attachments, «Validare formular», field 1.
- **C (3):** CFP checkboxes. **D (4):** field 3. **O (5):** fields 3, 4 and «Anulare Validare».

## 3. The XML

The builders `__GenerateXml` / `__GenerateXmlForORDNT` are still in the template (shape below), but
in A1.0.11 **nothing calls them** — the call in «Validare formular» is commented out, so no
`ORDNT.xml` is attached any more; «Export XML» only exports an attachment named `ORDNT.xml` if one
exists. The machine-readable content is the XFA data itself.

```
<ORDNT xmlns="mfp:anaf:dgti:ORDNT:declaratie:v1" DenInstPb Cif NrOrdonantPl DataOrdontPl>
  <docFd nr_unic_inreg beneficiar documente_justificative cif_beneficiar iban_beneficiar
         banca_beneficiar inf_pv_plata inf_pv_plata1>
    <rowTfd cod_angajament indicator_angajament program cod_SSI receptii plati_anterioare
            suma_ordonantata_plata receptii_neplatite/>
  </docFd>
</ORDNT>
```

## 4. What this means for K-BOT
- K-BOT fills every column at generation (the only safe shape, see §2 ⚠). In Adobe: «Validare formular»
  → signature 1 → «Validare formular» → signature 2 → (CFP) → signature 5. The two validations are
  NOT back to back: signature 1 sits between them.
- The template is downloaded from the legacy API (`TemplateDownloader`, `ORD_URL`); the server serves
  A1.0.11 (operator, 30.09.2026).
- Open for the ORD-sending slice: the RECEPȚIE step (what is uploaded, where, which workflow).
- **No «pre-validation» by K-BOT** (considered and dropped, 30.09.2026): SignatureField1's `initialize`
  re-locks it on every open while it is unsigned, whatever the saved state says, so only a click on
  «Validare formular» can open it. And the DDF analogy does not hold: there K-BOT writes placeholder
  data so the operator's own «Validează» passes; K-BOT's ORD already fills every column, so the
  first validation passes as it is.
- Template bug seen in passing: the «Verificat/Avizat» fields read signature 2's status where they
  mean signature 3's (`TheStatus3 = TheSignature2.signatureInfo()`), harmless in practice.

## 5. The old A1.0.08 (27.01.2026), for the record
There the first validation opened NO signature and only unlocked cols. 1-3; the second validation
checked the whole table, attached `ORDNT.xml` and opened signature 1; signature A locked the whole
table. So all data had to be in before any signature — signing after col. 4 alone was a dead end
(the operator's test with that version: «Nu ati completat coloana ,Cod SSI, randul 1» / «Nu se
accepta valori negative in coloana 5 Receptii neplatite -100»). No «Alte Avize», «Verificat/Avizat»
or «Anulare Validare».
