# SLICE-0000-05 — Part 2 «Opțiuni avansate»: the text, with capture tags

Operator, 30.09.2026: «go ahead with 0000-05».

## What changed and why

Six Romanian topics under `src/KBot.App/HelpContent/avansat/` (part `avansat`, shown only while
«Activează opțiuni avansate» is on), 7 capture tags:

| Topic | Covers |
|-------|--------|
| `avansat` — Opțiuni avansate | how to switch them on (password), what appears (fila «Documente», «Pagina FOREXE», «Temă», «Căi fișiere»), the password is a guard not security, the manual then carries all parts |
| `avansat.documente` | PDF engine (ActiveX / Fereastră găzduită), the hosted-window dialog (/n, release A/B, full screen on exit), ActiveX trace + new control per document, «Mesaje de script Adobe» (list, Probă, Lista implicită), Excel ribbon (Macro / Fereastră) |
| `avansat.pagina` | FOREXE page CSS rules: fields, «Salvează și aplică», «Regulă nouă...» (element tree, framed in the page), «Implicite» (the five default rules); the documents' captures «pagina originală / așa cum se vede» |
| `avansat.tema` | scheme edit / save / restore; scaling Automat / Fix 100% / Manual + factor; «Windows întinde fereastra» (restart); text size, «Font din temă», window base |
| `avansat.foldere` | the six folders, defaults, empty = default, relative paths, effect at next start, startup refusal on a bad path |
| `avansat.jurnale` | the log files and what each holds, server logs on the Jurnal page, «Consola FOREXE detaliată», what to send when reporting a problem |

**Deliberately NOT described:** the help capture mode (operator's rule, 0000-02).

Sources: the designers' operator text of `SetariAplicatieView`, `SetariPaginaView`,
`SetariTemaView`, `SetariFolder`, `AdobeGazduireForm`, `AdobeMesajeForm`, `RegulaPaginaForm`;
`SetariForm.AdvancedPageKeys`; `KBot.Common/SetariFoldere.vb` (folder list); `docs/SETARI_UTILIZATOR.md`
§6–7 (settings window, FOREXE page rules, document captures). §1–2 of that guide (Adobe clipping /
moving profiles) were NOT used: they describe the old hosting that the operator has since ruled out.

`screens:` keys point F1 on the «Documente» tab's controls and on the advanced pages here.

## Files touched

- Rewritten: `HelpContent/avansat/index.md`.
- New: `HelpContent/avansat/{documente,pagina,tema,foldere,jurnale}.md`.

## Test results

- Build KBot.App: 0 warnings, 0 errors.
- Static check: no duplicate ids / capture ids, every parent and topic link resolves, every
  `Type.control` screen key names a control declared in that type's designer. Not run on screen.

## Left unverified or deferred

- To be read by the operator: whether the Adobe options should be described at all for clients,
  and the «Ce trimiți când ceri ajutor» advice.
- No guided tour for Part 2 (none asked for; the engine takes one file in `tours/` if wanted).
- Part 3 «Director» (0000-06) next.
