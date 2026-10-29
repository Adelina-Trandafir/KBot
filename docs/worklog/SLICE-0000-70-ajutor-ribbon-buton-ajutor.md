# SLICE-0000-70…100 — Istoricul ajutorului pentru ribbon

Etapele 70/85/88/89/90/91/100 păstrează traseul inițial → eliminarea butonului Ajutor → grupurile machetei/vederii → modul compact și panoul temporar. Capturile și rezultatele rămân pe checkpoint; numerele 36, 41 și 10 descriu loturi istorice, nu o sumă de capturi distincte. Etapa 88 are jurnal, dar nu un rând propriu în registrul 0000. Nu se modifică ajutorul runtime.

[0000-70](#slice-0000-70) · [0000-85](#slice-0000-85) · [0000-88](#slice-0000-88) · [0000-89](#slice-0000-89) · [0000-90](#slice-0000-90) · [0000-91](#slice-0000-91) · [0000-100](#slice-0000-100)

## SLICE-0000-70 - Ajutorul pentru 0114: ribbonul si butonul «Ajutor» (13.10.2026) <a id="slice-0000-70"></a>

Sub-felie de ajutor a feliei 0114. Context, capturi si navigare: `SLICE-0000-69-ajutor-machete-ca-pagini.md`.

### Ce s-a schimbat si de ce
- Subiect NOU `contabil.ribbon` («Ribbonul (banda de comenzi)», `contabil/ribbon.md`, `screens: AvacontMain.ribbon`): filele si grupurile cu toate butoanele (cu conditiile: «Operatiuni necorelate» doar cand exista, «Jurnal activitate» doar cu comutatorul pornit; grupul Administrare lasat afara, e unealta operatorului / Debug), etichetele la mouse, strangerea (dublu clic pe fila / sageata; clic pe fila o desface; tinuta minte), butoanele fixate «Ajutor» si «Inchide» (cel din urma: inchide macheta, din lista revine la «Acasa», lipseste pe «Acasa», aplicatia se inchide din X; Esc), tabelul **«Unde gasesti ce era in MENIU»**. Capturi noi: `ribbon-file`, `ribbon-configurari`, `ribbon-strans`.
- Subiect NOU `contabil.ajutor.buton` («Butonul «Ajutor» din ribbon», `contabil/ajutor-buton.md`, copil al `contabil.ajutor`): pe «Acasa» = ajutorul ca pagina (in «Machete deschise», inchis cu «Inchide»); cu o macheta pe ecran = meniul «?» al ei; F1 merge oriunde. Capturi noi: `ajutor-buton-pagina`, `ajutor-buton-meniu`.
- `contabil.ajutor`: «trei feluri» de deschidere (F1, «?», butonul «Ajutor»); «Butoanele unui rezultat» spun «macheta din ribbon» in loc de «fereastra din MENIU»; din «Tururile ghidate» a iesit fraza despre pasii care deschid meniul MENIU (tururile acelea sunt rupte, vezi 0000-69); «Fereastra de ajutor sta deasupra» spune ca, deschisa din «Ajutor» pe «Acasa», e pagina. `prepare:` al capturii `ajutor-meniu` trimite la ribbon (redo-ul ei vechi ramane).
- `contabil.setari`: comutatorul `chkLogViewer` citat exact, cu ce face acum (butonul din ribbon).
- Etichete `0114` pe sectiuni.

### Fisiere
`src/KBot.App/HelpContent/contabil/ribbon.md` (nou), `contabil/ajutor-buton.md` (nou), `contabil/ajutor.md`, `contabil/setari.md` (o sectiune).

### Teste / verificare
Vezi 0000-74 (rularea finala `Check-Help.ps1 -Coverage -Map`). Nimic rulat in aplicatie.

### Capturi de refacut
Noi: `ribbon-file`, `ribbon-configurari`, `ribbon-strans`, `ajutor-buton-pagina`, `ajutor-buton-meniu` (doar `prepare:`; fara `goto:` pana la tipurile noi propuse in 0000-69). `prepare:` schimbat: `ajutor-meniu`.

### Neverificat
- Textul butonului «Operatiuni necorelate» din ribbon (copiat din randul meniului, cu numar si culoare): ajutorul nu il citeaza, spune doar cand apare.
- Unde exact sta sageata de strangere («the chevron at the right»): ajutorul spune «sageata din dreapta lui».

## SLICE-0000-85 - Help for 0114-02 / 0084-04: no «Ajutor» button, «Asociaza parteneri» on the ribbon (16.10.2026) <a id="slice-0000-85"></a>

### What changed and why
- The ribbon's «Ajutor» button is gone (0114-02): topic `contabil.ajutor.buton` (`contabil/ajutor-buton.md`) **deleted**; its two links (`ajutor.md`, `ribbon.md`) removed; `ajutor.md` now says help opens in two ways (F1, the «?» of the caption bar); `ribbon.md` sections «Strangerea ribbonului» and «Butoanele din dreapta» rewritten (the right-hand buttons are «Inchide» and the page buttons); `docs/HELP_SYSTEM.md` updated (what stays in the code).
- «Asociaza parteneri» is on the ribbon, visible only while «Sumar» is on screen, before «Inchide» with a thin separator (0084-04 / 0114-02): `contabil/vederi/sumar.md`, `tours/tur-sumar.md` (the step has no `target:`: painted ribbon buttons cannot be ringed by a tour), `contabil/ribbon.md`.
Tags `0114-02, 0114-03, 0084-04, 0000-85`. Extended the same day for the right group (0114-03): the button is in the group «Sumar» at the right of the panel, before «Inchide».

### Files touched
`src/KBot.App/HelpContent/contabil/ajutor.md`, `ribbon.md`, `ajutor-buton.md` (deleted), `vederi/sumar.md`, `tours/tur-sumar.md`; `docs/HELP_SYSTEM.md`.

### Test results
`tools\HelpCheck\Check-Help.ps1` run 16.10.2026: no error from these files once the slice ids exist; the errors left are old (`tutorials\*`, `tur-setari.md` nav item `jurnal`), not caused here.

### Unverified / deferred
- Captures `ribbon-file` and `ribbon-strans` (they showed «Ajutor») are to be redone; the `ajutor-buton-*` captures go with the topic (no image files existed for them).
- Not seen on screen.

## SLICE-0000-88 - Help for 0114-04: ribbon tabs (17.10.2026) <a id="slice-0000-88"></a>

### What changed and why
`contabil/ribbon.md` (tag `0114-04` on the intro and on the tabs table): new tab order, "Angajamente" selected at start, "Date" group now under Configurari, new tab Extrase with "Descarca extrase"; the "unde era in MENIU" table follows. `help-version.txt` already `2026-10-17`.

### Files touched
`src/KBot.App/HelpContent/contabil/ribbon.md`.

### Test results
Check-Help.ps1 not run.

### Unverified / deferred
Captures `ribbon-file`, `ribbon-configurari` not redone (the ribbon looks different now).

## SLICE-0000-89 - Help for 0114-05: the statements are downloaded from the ribbon only (17.10.2026) <a id="slice-0000-89"></a>

### What changed and why
`contabil/vederi/extrase.md` (section «Descarcarea extraselor» rewritten; the opening of the window only via the ribbon), `contabil/ribbon.md` (the button appears only on the Extrase window / the list of angajamente), `contabil/fereastra.md` (the left footer icon line replaced), `tours/tur-extrase.md` and `tours/tur-fereastra.md` (the tree-footer steps removed), `tutorials/descarca-extrase.md` (first step is now a message about the ribbon button, no target: painted ribbon buttons cannot be ringed). Tags `0114-05, 0000-89`.

**Captures marked `redo` (2026-10-17 18:00, why 0114-05)**: `extrase-fereastra`, `lista-actualizare`, `fereastra-zone`, `fereastra-principala` and every capture with `goto: view:*` (the whole window shows the list footer) - 36 in all.

### Files touched
The six help files above plus the capture lines in `contabil/**`.

### Test results
`Check-Help.ps1`: no error from these files once this worklog and the status row exist; the errors left are old (efactura tutorial controls, `ordonantare-din-plata.md` tags).

### Unverified / deferred
Nothing seen on screen; the tutorial's new step is untested in the runner. Captures are to be redone by the operator.

## SLICE-0000-90 - Help for 0114-06 .. 0114-14 (17.10.2026) <a id="slice-0000-90"></a>

### What changed and why
`contabil/fereastra.md` (rotita no longer has «Actualizeaza angajamente...»; «Grupe» only with a group; the footer popup; «Editeaza grupe» in the ribbon; E-Factura row in the views table), `contabil/ribbon.md` (the form group and the view group; «Descarcare» group removed; MENIU table), `contabil/forexe/lista.md`, `contabil/forexe/descarcare-multipla.md` (the «VRAC» row), `contabil/vederi/extrase.md`, `contabil/efactura/index.md`, tours `tur-fereastra` / `tur-extrase`, tutorials `descarcare-angajamente-noi`, `actualizare-multipla`, `grupe-deschide-fereastra`, `fereastra-extrase`, `descarca-extrase`. Captures marked for redo: 41 (whole-window ones, the ribbon ones, the footer / groups menus).

### Files touched
The help files above under `src/KBot.App/HelpContent/`.

### Test results
`tools\HelpCheck\Check-Help.ps1`: see the status row; the old errors (efactura tutorial controls, ordonantare-din-plata tags, tur-setari) are not from this slice.

### Unverified / deferred
Nothing seen on screen. The tutorial steps without a target (`grupe-deschide-fereastra`, `descarca-extrase`, `fereastra-extrase`) and the new anchors `popup.noi` / `popup.vrac` + signal `footer-menu:*` are not tried in the runner. Captures to be redone by the operator.

## SLICE-0000-91 - Help for 0114-15 .. 0114-19 (17.10.2026) <a id="slice-0000-91"></a>

### What changed and why
`contabil/vederi/rezervari.md`, `contabil/vederi/receptii.md`, `contabil/ribbon.md` (the view group now has Sumar / Rezervari / Receptii buttons; two rows in the Before / Now table), `contabil/ddf/rezervare.md`, `contabil/asocieri/index.md` and `fereastra.md` (capture note), `contabil/efactura/index.md` (no month list, rows not cut; the view lists only the angajament's invoices, filter; footer «Actualizeaza»; the ribbon fields look like the popup), tours `tur-rezervari`, `tur-receptii`, `tur-ddf`, `tur-asocieri` (the steps of the removed tree icons became target-less «Ribbon ›» steps), tutorial `legaturile-receptiilor`. Captures marked for redo: 10.

### Files touched
The help files above under `src/KBot.App/HelpContent/`.

### Test results
`tools\HelpCheck\Check-Help.ps1`: no new error from these files; the old ones (tur-setari, efactura-factura-noua controls, ordonantare-din-plata tags) are not from this slice.

### Unverified / deferred
Nothing seen on screen. The target-less steps of the tours were not tried in the runner. Captures to be redone by the operator.

## SLICE-0000-100 - Help for 0114-24 (18.10.2026) <a id="slice-0000-100"></a>

### What changed and why
`contabil/ribbon.md`: the section «Strangerea ribbonului» now says that a click on a tab of a folded ribbon shows its buttons for a moment (closes after use, on a click elsewhere, Esc, switching window; a click in a field keeps it open) and that double click / the arrow unfolds for good; new section «Ribbon compact». `contabil/setari.md`: the new tick in the table of the «Fereastra principala» group. Slice tags `0114-24, 0000-100`.

### Files touched
`src/KBot.App/HelpContent/contabil/ribbon.md`, `src/KBot.App/HelpContent/contabil/setari.md`.

### Test results
`tools\HelpCheck\Check-Help.ps1`: the only messages about these two files were the slice tag `0000-100` not yet being in the status (fixed by the rows added with this slice); the other errors it prints (tutorials without tags, `tur-setari` nav item `jurnal`) were there before and are not from this change.

### Unverified / deferred
Nothing seen on screen. The capture `ribbon-strans` is unchanged (the prepare text now says to unfold with a double click).
