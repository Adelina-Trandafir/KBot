# SLICE-0124-08 — Help for the K-BOT signing flow

**Date:** 30.10.2026.

## What changed and why

- `contabil/ddf/semnare.md`: new section «Documente noi, create în K-BOT: cine semnează A» (the role per compartment, the hand-over message when the role is another's, the list of documents to sign, the no-role message, the unit without the flow; the sending to FOREXE stays as it was).
- `contabil/ddf/secretar.md` (new topic, `screens: DeSemnatRestransForm…`): the narrowed window of a Secretar / Administrator.
- `contabil/setari.md`: the «Semnare DDF» page (+ `SetariSemnareDdfView` in `screens`).
- Written for the user only: what they see and do; nothing of the server rules, headers or windows' inner working.

## Test results

`tools\HelpCheck\Check-Help.ps1`: the errors it prints are older ones in other topics; nothing was reported for the new topic or the new sections. No screenshots taken (the new screens never ran).

## Left unverified / deferred

- Captures of the new screens: none.
- `director/lista.md` still says the list is «of a director»; the same button now exists for the accountant and the proxy — a later help pass should say so there too.
