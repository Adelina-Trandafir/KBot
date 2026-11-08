# SLICE-0088-08 - signed file lands on the Desktop, not in the target folder (05.11.2026)

## What was found (GR23 machine, K-BOT 1.1.5.2, Acrobat 26.002.22031 - same as the operator's PC)
- Signing in the hosted Adobe window looked right (signatures visible in the window), but «Verificare dupa salvare» said «documentul de pe disc nu are nicio semnatura» every time, for DDF, ORD and note CAB.
- Operator's files compared: the DDF after a failed attempt (`C:\KBOT\Temp\PDF`) had 4 parts (what K-BOT wrote at generation, all stamped with the generation time) and only the old Reader-rights signature; the operator's own signed DDF has 6 parts, the two extra ones written by Adobe, and the user's signature.
- `activex_check.log` of GR23: at load the note file was «written 10:21:04.507» (generation); after the «Save As» of 10:21:31 it was still 10:21:04.507 - Adobe had not touched the target.
- The operator ran a search for PDFs written in the last 15 minutes: `C:\KBOT\Temp\PDF\DDF_NR_1_REV_0_...PDF` 273728 bytes (11:01:56, generation) and `C:\Users\Lungu Gabriela\Desktop\DDF_NR_1_REV_0_...pdf` 349300 bytes (11:02:29). The operator confirmed: ALL signed documents (DDF, ORD, note CAB) went to the Desktop and ARE signed.
- The Desktop name is lowercase `.pdf` = the name Adobe itself proposes; K-BOT writes `.PDF`. The log also fits: first save «fara confirmare de suprascriere» although the target existed (Adobe was saving elsewhere), later ones «cu confirmare» (the Desktop copy existed by then).
- Conclusion (verified from the files and logs above): on GR23 the path K-BOT writes into the Save As box is not what Adobe saves to; it saves under its own name in its own default folder, and the trap reports «Salvare ca incheiat» for the target all the same.

## What the diagnostic build showed on GR23 (11:22, DDF 51)
- The box K-BOT writes into: id 1001, parent «ComboBox» (the real file-name box); text read back = the full target path, both times.
- Dialog 1: closed in ~350 ms WITHOUT the «replace?» question although the target existed (273728 bytes, 11:22:30.881) -> Adobe saved to another path. Target unchanged afterwards (the new «ATENTIE» line). Dialog 2, 3 s later: «replace?» asked (the first copy on the Desktop existed), target still unchanged.
- So (verified): the right text is in the right box and Adobe still does not save there. What is NOT known: why. Open: the dialog takes the name from typed input, not from the box's text; or something else on that PC.

## Second change (an attempt, NOT verified - needs a run on GR23)
- `AdobeNativeMethods.TypeText` (new): selects the box's content and sends one WM_CHAR per character, like a keyboard.
- `AdobeSaveTrap.HandleSaveAs`: after the WM_SETTEXT write, the same path is TYPED; if the box then holds another text (auto-complete tail) the plain write is put back. The «Salvare apasat» log line says which happened (`tastat=True/False` or «tastarea a lasat ...»). The check «target unchanged» stays: it tells at once whether the typing helped.
- Risk: the dialog is the operator's signing flow on every PC; the typing is after the proven write and falls back to it, but it was NOT run (not even on the developer PC).

## Result of the typing build on GR23 (11:30, DDF 51) - worked
- Log: «Salvare apasat ... (tastat=True)»; this time «cu confirmare de suprascriere» (Adobe saved to the target); «Verificare dupa salvare: campuri semnate acum: ...SignatureField1» one second later; then K-BOT's own Ctrl+S after the signature, four more verifications with the same signed field, no error.
- Cause (verified by the before/after logs, not by Adobe's internals): that dialog takes the name from TYPED input; a WM_SETTEXT write alone is ignored and Adobe saves under its own name in its own folder. Typing (WM_CHAR) fixes it.
- Only DDF 51 with one signature was seen. NOT yet seen: ORD, note CAB, the second signature, other PCs, and the developer PC (the typing runs there too).
- The «ATENTIE: fisierul tinta nu s-a schimbat» line fired in that run although the save worked: it was read when the box closed, 1 s BEFORE Adobe wrote the file. A false alarm, so the verdict was removed: the line now only states the stamp «la inchiderea dialogului» (information); the verdict stays «Verificare dupa salvare».

## What changed first (diagnostics only)
`src/KBot.Controls/Adobe/AdobeSaveTrap.vb`:
- `HandleSaveAs`: the always-on log line after «Salvare apasat» now also says which box was written into (handle, control id, parent class), the text read back, and the target file's size + last write at that moment.
- `CheckPending` (dialog gone): compares the target's size + last write with the value taken before; if unchanged it logs «ATENTIE: fisierul tinta nu s-a schimbat dupa Salvare ca ... Adobe a scris documentul in alta parte sau nu l-a scris».
- `TargetStamp` helper (never throws; a failure goes to `GlobalErrorLog`).

## Files touched
- `src/KBot.Controls/Adobe/AdobeSaveTrap.vb`, `src/KBot.Controls/Adobe/AdobeNativeMethods.vb` (`TypeText`, `EM_SETSEL`, `WM_CHAR`).

## Test results
- `dotnet build src/KBot.Controls`: 0 errors, 0 warnings. Nothing run, no tests (operator's rule).

## Left unverified / deferred
- **The cause is not found, only the symptom pinned down.** Open hypotheses: (a) the box K-BOT writes into is not the one Adobe's dialog reads on that PC; (b) the dialog resets its name/folder after the text is written (slower PC). Neither is verified.
- Next step: the operator sends `adobe_preview.log` from GR23 after a signing with this build - the new lines show the box and the target's state; with the trace switch on (`acropdf_trace.log`) the full box tree is there too.
- The real fix (making Adobe save to the target, or picking up the file it wrote elsewhere) is NOT built: it is the operator's design decision once the cause is known.
- The version of `KBot.Controls` is not bumped here (push-update asks).
- No help change: nothing the operator sees differs.
