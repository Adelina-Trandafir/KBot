# SLICE-0088-10 - sending a document to FOREXE survives the portal frame reloading (09.11.2026)

Sub-slice of SLICE-0088 (correction notes CAB): «Trimite în CAB» sends the note's PDF through `UploadElectronicDocumentAsync`.

## What changed and why
- Before the click: the portal's frame can still be reloading when it first shows the form, and a query landing in that moment fails with «Execution context was destroyed». Nothing has been sent yet, so the form is now looked up again (up to 3 attempts) when the failure is a navigation race (`IsNavigationRace`); every retry is written to `GlobalErrorLog`.
- At the click: the click itself running into the frame navigating counts as «the form was posted» (the POST response is waited for, the navigation race is logged and ignored).
- After the click: the document may already be in FOREXE, so any failure from here on says so (`AfterSend`), otherwise the operator is told «failed» and sends the note a second time. FOREXE's answer is read by the new `ReadAnswerAsync` once the new document has loaded (a mark set on the form's document disappears when the answer page replaces it).

## Files touched
- `src/KBot.Forexe/Executor/WorkflowExecutor.DocumentUpload.vb`
- Status: `state/KBOT_STATUS_0080-0089.md` (row 0088-10)

## Test results
- Nothing run, no tests (operator's rule).

## Left unverified / deferred
- Not run against the real portal. The assumption that the failure at the first query is a reload of the frame comes from the error text, not from a trace.
- No help change: the operator sees the same windows; only the failure message after sending is more careful.
