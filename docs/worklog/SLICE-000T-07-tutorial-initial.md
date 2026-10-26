# SLICE-000T-07/08/13 — Istoricul tutorialului de început

Consolidare documentară D002-07. Tutorialul inițial din 05.10.2026 și înlocuirea lui din 18.10.2026 rămân etape distincte: vechiul flux devine obligatoriu în a doua trecere (înregistrată ca 000T-08), iar noul «Primii pași» nu este obligatoriu și folosește cheia nouă ShowStartTutorial. Cheile, fișierele retrase, regulile de văl și cele două corecții ale descrierii butonului Înainte rămân la data lor. Nicio limită de probă nu devine confirmare vizuală.

[07](#slice-000t-07) · [08 — mandatory](#slice-000t-08) · [13 — înlocuirea](#slice-000t-13). Jurnalul de ajutor 0000-107 rămâne separat: descrierea sa istorică numește 11 pași, iar jurnalul tehnic 13; numărătorile nu sunt uniformizate.

<a id="slice-000t-07"></a>

## SLICE-000T-07 — the tutorial of the start («Ce sunt tutorialele») + its setting

Request (operator, 05.10.2026): the way the guided tour is shown at K-BOT's start, a very small tutorial: first a message about the new tutorial system, then a ring on the «?» of the main window, the user clicks it, then a ring on the list of current tutorials. Switched on / off by a setting in Setări (like the tour). «For this we also need a tutorial.»

### What changed and why

- **Setting:** `AppSettings.ShowInitialTutorial` (default True; property, DTO, `FromDto`, `ToDto`). `SetariAplicatieView`: new switch `chkTutorialInitial` («Arată tutorialul de început la pornirea K-BOT»), right under the tour's switch (group «Fereastra principală»), designer row added (`RowCount` 13), loaded in both places the tour's is, saved through `SalveazaComutator`.
- **The tutorial:** `HelpContent/tutorials/tutoriale-intro.md` — 3 steps: (1) a message with no target (manual, «Înainte»); (2) ring on `KbotForm.capBar` part `help`, `wait: opens:KBotHelpPopup` (the user presses «?»); (3) ring (`dim: ring`) on the «Tutoriale» block of the popup, last step «Gata».
- **Pointing at the block:** `KBotHelpList` is now partial; `KBotHelpList.HelpParts.vb` implements `IKBotHelpParts` with the part `tutorials` (the header row with `Key = "tutorials"` + the tutorial rows under it; a block scrolled out of view is scrolled in first). `KBotHelpRow.Key` (new, generic string). `HelpService.PopupHomeRows` sets that key on the «Tutoriale» header. `Check-Help.ps1` knows `KBotHelpList` / `tutorials`.
- **Start rules** (`HelpService`): `InitialTutorialId`, `StartInitialTutorial`, `InitialTutorialSeen`, `PostInitialTutorial`. The tutorial runs only when its setting is on, the login reads its part, the main window is on screen and **the initial tour is not owed** — a tour still due goes first; when it ends, `StartInitialTour`'s finish callback posts the tutorial (after the tour's own «seen» has been saved). Not owed = the tour setting is off, the login has no tour, or the tour was seen to the end. A tour closed early (still due) therefore keeps the tutorial waiting, so the two never overlap.
- **Runner** (`TutorialRunner`): `Start(..., k_initial)`; an initial tutorial's bubble shows the box «Nu mai arăta tutorialul de început» (`HelpTourBubble.NeverAgainText`, `NeverAgain` now settable); the tick is kept when the runner moves to a new bubble (each host window gets its own); `InitialTutorialSeen` is called when the tutorial ends past its last step (`_completed`) or with the box ticked. **Also fixed:** a step with no target / anchor no longer says «Ce trebuie să faci nu e pe ecran acum…» (it fired for every message-only step, e.g. «Se deschide documentul» in the older tutorials too).
- **The second tutorial** (my reading of «for this we also need a tutorial»): `tutorials/setare-tutorial-initial.md` — MENIU › «Configurare K-BOT» (`wait: opens:SetariForm`) → page «Aplicație» → tab «Generale» → the switch (optional step) → «Gata». Both tutorials are listed under «Tutoriale» in every window's «?».
- **Help** (`0000-51`): `contabil.ajutor` (new section «Tutorialul de început»), `contabil.setari` (new bullet in «Pagina «Aplicație»: pornirea K-BOT»), tags `000T-07, 0000-51`; `HelpContent/README.md` + `docs/HELP_SYSTEM.md` (the initial tutorial, the `KBotHelpList` part).

### Files touched

`src/KBot.Common/AppSettings.vb` · `src/KBot.App/Setari/SetariAplicatieView.vb` + `.Designer.vb` · `src/KBot.Controls/Popup/KBotHelpRow.vb` · `KBotHelpList.vb` · `KBotHelpList.HelpParts.vb` (new) · `src/KBot.App/Help/HelpService.vb` · `HelpTourBubble.vb` · `src/KBot.App/Tutorial/TutorialRunner.vb` · `src/KBot.App/HelpContent/tutorials/tutoriale-intro.md` (new) · `setare-tutorial-initial.md` (new) · `contabil/ajutor.md` · `contabil/setari.md` · `HelpContent/README.md` · `help-version.txt` · `tools/HelpCheck/Check-Help.ps1` · `docs/HELP_SYSTEM.md` · status files.

### Test results

`dotnet build src\KBot.App\KBot.App.vbproj` (output redirected to a scratch folder: K-BOT was running from `bin\Debug`, so the normal build stops at the file copy — MSB3027 — not at the compile): **0 warnings, 0 errors**. `Check-Help.ps1 -Coverage`: «No errors.» (warnings only: `KBotHelpPopup` / `SetariForm` do not implement `IKBotTutorialHost`, expected; the six uncovered windows listed are older, not from this slice). No tests written or run (rule: never tests unless asked).

### Left unverified or deferred

- **Nothing was seen on screen.** To check by hand: start K-BOT with both settings on → tour first; after it, the tutorial. Step 2: the veil must leave «?» clickable; step 3: the popup must stay open while the bubble is clicked (the bubble never takes the focus, the popup closes on losing it) and the ring must sit on «Tutoriale» + its rows (scroll case: a popup with many tours).
- Step 1 has no ring and is centred on the screen the cursor is on; the window behind is NOT veiled there (no target).
- Opening Setări from the second tutorial: the MENIU menu is a window of its own; the step uses `dim: ring` so the menu is not blocked — unverified. If «Aplicație» is already the open page, or «Generale» the open tab, those steps are skipped by state. *(Superseded by the fourth pass below.)*
- The tutorial «Ce sunt tutorialele» is not offered when the login is a director (part `contabil`), same as the tour.
- NOUTATI.md not touched (written when a version is published): a line to add — «Tutorial scurt la pornire despre tutoriale (se poate opri din Setări › Aplicație).»

<a id="slice-000t-08"></a>

### Second pass — mandatory tutorials (operator, 05.10.2026)

Request: the main tutorial must NOT be closable except by reaching its end; a property per tutorial (true / false) says so; a press outside the tutorial shows a message that the user has to reach the end.

- **Property:** `TutorialFlow.Mandatory` (header key `mandatory: yes|no`, default no; parsed, written by `ToMarkdown`, listed in `Check-Help.ps1` with a yes/no check, documented in `HelpContent/README.md`). `tutoriale-intro.md` is `mandatory: yes`. It shows in the designer's flow property grid like the other flow properties; the designer's «Testează» run forces it to False so a test can always be left.
- **Runner, mandatory flow:** «Mă opresc» is disabled (`HelpTourBubble.CloseAllowed`); Esc / Alt+F4 on the bubble, a press on the veil and a key typed outside the allowed places call `Remind()` — «Tutorialul «…» trebuie parcurs până la final…», OK only, no «Vrei să ieși?». A message-only step (no target) veils the whole window in such a tutorial, otherwise the first step of the intro would catch no press. The «Nu mai arăta…» box is not shown (it would only be a way out); the setting now goes off only when the tutorial is completed.
- **Window closed under the step** (the «?» popup closes by itself when it loses the focus, so a press outside it closes it): `HostClosed` reminds, then `ReturnToLastOpening` goes back to the nearest earlier step with `wait: opens:` (step 2, «Apasă «?»») so the popup can be opened again.
- **Help:** `contabil.ajutor` / `contabil.setari` rewritten (no box, must be finished); README + HELP_SYSTEM; step texts.
- **Build:** 0 warnings, 0 errors (separate output dir); `Check-Help.ps1`: «No errors.». Nothing seen on screen.
- **Open:** a program crash / closing K-BOT still ends it (nothing can prevent that); an error inside the runner ends the tutorial (`OnTick` catch) even when mandatory, so a bug cannot trap the operator. The popup's focus behaviour on a press in the main window is the case to watch.

### Third pass — the reminder fired on a correct press (operator, 05.10.2026)

Report: «mesajul că tutorialul trebuie dus până la capăt apare și când userul apasă pe elementul corect». **Cause not proven** (nothing can be run here); three paths that could do it were closed:
- **The «?» popup closed under the tutorial.** It closes on any loss of focus, and the windows the tutorial builds for step 3 can take the focus for a moment → `HostClosed` → the reminder right after the correct press of «?». `KBotHelpPopup.KeepOpen` / `CloseNow` (new): the runner holds the popup open while it is the step's window and closes it when the step / tutorial ends.
- **A press on the veil that is really on a hole, the ring or the bubble** (region lag, the 2 px between the hole and the ring): `OnDimClicked` now checks `Control.MousePosition` against the holes (grown 10 px), the ring and the bubble first.
- **A row of the «?» list started another tutorial**, which ended the mandatory one: `TutorialRunner.Start` now reminds and keeps the mandatory tutorial running.
- Build 0 warnings, 0 errors (separate output dir). **Nothing seen on screen** — if the message still shows on a correct press, the step (1 «Înainte», 2 «?», 3 «Gata») and whether the popup was open are what is needed.

### Fourth pass — «Setări» tutorial: veil on step 1, then ring on the menu row (operator, 05.10.2026)

Report: (1) at step 1 of `setare-tutorial-initial` the ring shows but the rest of the app stays bright; (2) after the click on MENIU the next step must show which menu option to press, with a ring and the app dimmed.

- **1 — cause:** the step carried `dim: ring` (added in the first pass so the menu would not be blocked). Removed: the step is now «Deschide meniul» (`target: KbotForm.btnMeniu`, `wait: click`) with the default veil.
- **2 — new step «Configurare K-BOT»:** `anchor: menu.setari`, `allow: KbotForm.btnMeniu`, `wait: opens:SetariForm`. Needed three small pieces: `KBotMenuWindow.RowScreenBounds(key)` (screen rectangle of a row), `KBotDropDownMenu.RowScreenBounds(key)` (looks in every open level), and `KbotForm.TutorialAnchor` answering `menu.<key>` (constant `AnchorMenuPrefix`); empty while the menu is closed. `allow: btnMeniu` keeps the button usable on the veil (and keys typed with it focused are not read as a deviation), so a closed menu can be opened again — the step text says so.
- **Docs:** `HelpContent/README.md` anchor row (`menu.<key>`).
- **Build:** 0 warnings, 0 errors (separate output dir); `Check-Help.ps1`: «No errors.».
- **Unverified (nothing seen on screen):** (a) the menu windows are shown after the veil, so they sit ABOVE it — the veil dims the app behind, the other menu rows stay bright, only the ring marks the target row; (b) the host must stay active while the menu is open (menu windows never activate) so the veil does not hide; (c) while the menu is closed in step 2 there is no ring and no veil (the anchor is not on screen) and the bubble carries the «nu e pe ecran» note.

<a id="slice-000t-13"></a>

## SLICE-000T-13 — tutorialul de pornire «Primii pași în K-BOT» (ajutorul: 0000-107)

Cererea operatorului, 18.10.2026: un tutorial scurt care rulează o dată la deschidere, se poate opri / porni din Setări și le înlocuiește pe cele vechi (ambele: turul ferestrei principale și tutorialul «Ce sunt tutorialele»). Pașii, dați de operator: fila Configurări · fila E-Factura · «Token ANAF» · (opțional) «Reînnoiește» · «Închide» · fila Extrase · fila Angajamente · «Lista angajamentelor» · un angajament din arbore · butoanele din dreapta sus · «Închide» · butonul cardului «Ce este nou» · stop.

### What changed and why

- **Tutorialul:** `HelpContent/tutorials/tutorial-pornire.md` (`starts: AvacontMain`, nu e obligatoriu): 13 pași, unul pe fiecare cerință de mai sus plus mesajul de final. «Reînnoiește tokenul» e opțional (`optional: yes`, fără `guard`: operatorul îl apasă cu bună știință). Pasul cu «Închide» al ferestrei tokenului așteaptă `click` pe `TokenForm.btnInchide`; deschiderea ferestrei se așteaptă cu `opens:TokenForm` (comanda e modală: semnalul ei vine abia la închidere).
- **Pornirea și oprirea:** `AppSettings.ShowStartTutorial` (cheie nouă, implicit bifat). Cheile vechi `ShowInitialTourOptIn` / `ShowInitialTutorial` nu mai sunt citite (de aceea și cine văzuse tutorialul vechi îl primește pe cel nou o dată). `HelpService.StartOpeningTutorial` / `OpeningTutorialSeen` (în locul lui `StartInitialTour`, `InitialTourSeen`, `StartInitialTutorial`, `InitialTutorialSeen`, `PostInitialTutorial`); bula are caseta «Nu mai arăta tutorialul de început»; se debifează singur după ce e văzut până la capăt. În Setări › Aplicație › Generale a rămas o singură bifă (`chkTutorialInitial`, textul neschimbat); `chkTurInitial` a plecat (designer: rândurile mutate, `RowCount` 16).
- **De unde pornește:** turul pornea din `KbotForm`, care din 0113 e ascuns în spatele paginii «Acasă» (`Visible` fals) — deci nu mai pornea deloc. Acum `KbotForm.PornesteTutorialulDePornire` (după încărcarea datelor) cere `HelpService.StartOpeningTutorial` cât fereastra principală e pe ecran.
- **Turul:** `tur-fereastra` rămâne în lista «?»; doar pornirea automată a plecat (`HelpTourRunner`: scoase `initial`, `_completed`, `InitialTourSeen`).
- **Ribbonul în tutoriale:** `AvacontMain.Tutorial.vb` (nou) — `AvacontMain` implementează `IKBotTutorialHost` și trimite restul spre `KbotForm` (ancore, semnale, `TutorialBegin/End`). Ancore: `ribbon-tab.<nume>` (configurari / angajamente / e-factura / extrase), `ribbon.<cheie>`, `ribbon.right`. Semnale: `ribbon-tab:<nume>` (și când fila era deja deschisă) și `ribbon:<cheie>` (după ce comanda a rulat). `KBotRibbon`: `TabClicked`, `TabScreenBounds`, `RightEndScreenBounds`; `ButtonScreenBounds` își asigură acum așezarea (`EnsureLayout`).
- **Vălul:** `TutorialRunner.RootWindow` — o pagină găzduită (shell, «Acasă») nu mai e „fereastra” tutorialului; vălul, chenarul și bula aparțin ferestrei principale, deci vălul acoperă și ribbonul și panoul lateral. Atinge TOATE tutorialele care lucrează în shell (vezi neverificat).
- **Ajutor (0000-107):** `contabil/ajutor.md`, `contabil/setari.md`, `HelpContent/README.md`, `docs/HELP_SYSTEM.md`. Fișierele `tutoriale-intro.md` și `setare-tutorial-initial.md` (cum pornești / oprești tutorialul de început; pornea prin butonul MENIU, ascuns din 0114) șterse, la cererea operatorului.

### Files touched

`src/KBot.Common/AppSettings.vb` · `src/KBot.Controls/Ribbon/KBotRibbon.vb`, `KBotRibbonItems.vb` · `src/KBot.App/AvacontMain.vb`, `AvacontMain.Tutorial.vb` (nou), `KbotForm.vb`, `KBot.App.vbproj` · `src/KBot.App/Help/HelpService.vb`, `HelpTourRunner.vb` · `src/KBot.App/Tutorial/TutorialRunner.vb` · `src/KBot.App/Setari/SetariAplicatieView.vb`, `.Designer.vb` · `src/KBot.App/HelpContent/tutorials/tutorial-pornire.md` (nou), `tutoriale-intro.md` (șters), `contabil/ajutor.md`, `contabil/setari.md`, `README.md` · `docs/HELP_SYSTEM.md` · status.

### Test results

`dotnet build src\KBot.App\KBot.App.vbproj` (ieșire în folder de lucru): **0 avertismente, 0 erori**. `Check-Help.ps1 -Coverage`: nicio eroare despre fișierele acestei felii; rămân 22 de erori vechi, ale altor fișiere (tur-setari, `efactura-factura-noua`, `ordonantare-din-plata`, `efactura/index.md`), și un avertisment așteptat: `TokenForm` nu implementează `IKBotTutorialHost`. Nu s-au scris și nu s-au rulat teste (regula: niciodată teste nerecerute).

### Left unverified or deferred

- **Nimic văzut pe ecran.** De urmărit, în ordine: (1) chenarul pe file și pe butoane (pozițiile vin din așezarea ribbonului; după apăsarea unei file, chenarul se mută la următorul tic de 150 ms); (2) pasul 3: butonul «Token ANAF» deschide fereastra modală — chenarul pe «Închide» trebuie să apară în ea; (3) pasul 4: un clic pe «Reînnoiește» pornește pasul cu certificatul, tutorialul trece la «Închide» cu acea fereastră deschisă; (4) pasul 8: arborele trebuie să fie încărcat; (5) pasul 12: dacă cardul «Ce este nou» e deja strâns, apăsarea lui îl deschide la loc (textul pasului acoperă ambele cazuri); (6) pornirea automată: `Starts = AvacontMain`, după ce `KbotForm` și-a încărcat datele.
- **Ribbon strâns** (dublu clic pe o filă): butoanele nu sunt pe ecran, deci pașii pe butoane spun „nu e pe ecran”; tutorialul nu desface ribbonul.
- **Vălul pe toată fereastra** schimbă și tutorialele mai vechi din shell (acum clicul pe ribbon în timpul lor întreabă „Vrei să ieși?”). Nu am rulat niciunul.
- **NOUTATI.md** nu s-a atins (se scrie la publicarea versiunii). Linie de adăugat, fără diacritice: «Tutorial scurt la pornire, prin panglica de sus; se opreste din Setari > Aplicatie.»
- `TokenForm` nu e gazdă de tutorial (generic funcționează, ca `SetariForm`); `Check-Help.ps1` avertizează.

### Second pass — the main window kept its veil behind a modal window (operator, 18.10.2026)

Report: with a modal pop-up on screen the background loses its grey tint. **Cause (read in the code, not seen on screen):** the veil is laid over the step's host window only; when the step works in a window of its own (the token window), the host is that window and the main window behind it had no veil. **Fix:** `TutorialRunner` makes a second veil (`_back`, `WindowBehind`) over the main window while the host is another window, with a hole where that window stands; it follows the first veil's visibility and goes with the presenter. Build: 0 warnings, 0 errors. **Not seen on screen.** Unchanged on purpose: the veil still steps back while a window that is not the tutorial's (a file chooser, the certificate picker after «Reînnoiește») is the active one.

### Third pass — «Înainte» with its sign (operator, 18.10.2026)

Request: where a message says «Apasă «Înainte»», the reader does not know which button that is; add the button's sign in brackets. The button reads «Înainte ►» (`HelpTourBubble`, `TutorialRunner.Present`), so every step / tour text and the two lines of `contabil/ajutor.md` that name it now read «Înainte» (►) — 52 places in 19 tutorial / tour files + 2 in `ajutor.md`; the section tags got `000T-13` (`ajutor.md`: `000T-13, 0000-107`). «Gata» has no sign on its button, so it is unchanged. `Check-Help.ps1`: no new errors. Not seen on screen: the sign is the character U+25BA, drawn by the bubble's text engine (the button itself shows the same character).

### Fourth pass — «Înainte» described, not drawn (operator, 18.10.2026)

The sign (►) of the third pass did not look like the button: the bubble's buttons are green icons (a bar and an arrow), not text, and the text engine cannot draw an image. Operator chose «words with a sign»: the 52 places now read «Înainte» (săgeata verde cu bară, jos în dreapta, ⇥) — the same wording in tutorials, tours and the two lines of `ajutor.md`. Tags unchanged (already `000T-13`). The real icon inside the text would need an inline-image run in `KBotRichText` (not done). Not seen on screen: whether U+21E5 is drawn by the font in use.
