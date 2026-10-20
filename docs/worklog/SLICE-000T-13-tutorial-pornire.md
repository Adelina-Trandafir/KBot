# SLICE-000T-13 — tutorialul de pornire «Primii pași în K-BOT» (ajutorul: 0000-107)

Cererea operatorului, 18.10.2026: un tutorial scurt care rulează o dată la deschidere, se poate opri / porni din Setări și le înlocuiește pe cele vechi
(ambele: turul ferestrei principale și tutorialul «Ce sunt tutorialele»). Pașii, dați de operator: fila Configurări · fila E-Factura · «Token ANAF» ·
(opțional) «Reînnoiește» · «Închide» · fila Extrase · fila Angajamente · «Lista angajamentelor» · un angajament din arbore · butoanele din dreapta sus ·
«Închide» · butonul cardului «Ce este nou» · stop.

## What changed and why

- **Tutorialul:** `HelpContent/tutorials/tutorial-pornire.md` (`starts: AvacontMain`, nu e obligatoriu): 13 pași, unul pe fiecare cerință de mai sus plus
  mesajul de final. «Reînnoiește tokenul» e opțional (`optional: yes`, fără `guard`: operatorul îl apasă cu bună știință). Pasul cu «Închide» al ferestrei
  tokenului așteaptă `click` pe `TokenForm.btnInchide`; deschiderea ferestrei se așteaptă cu `opens:TokenForm` (comanda e modală: semnalul ei vine abia la închidere).
- **Pornirea și oprirea:** `AppSettings.ShowStartTutorial` (cheie nouă, implicit bifat). Cheile vechi `ShowInitialTourOptIn` / `ShowInitialTutorial` nu mai sunt citite
  (de aceea și cine văzuse tutorialul vechi îl primește pe cel nou o dată). `HelpService.StartOpeningTutorial` / `OpeningTutorialSeen` (în locul lui
  `StartInitialTour`, `InitialTourSeen`, `StartInitialTutorial`, `InitialTutorialSeen`, `PostInitialTutorial`); bula are caseta «Nu mai arăta tutorialul de început»;
  se debifează singur după ce e văzut până la capăt. În Setări › Aplicație › Generale a rămas o singură bifă (`chkTutorialInitial`, textul neschimbat);
  `chkTurInitial` a plecat (designer: rândurile mutate, `RowCount` 16).
- **De unde pornește:** turul pornea din `KbotForm`, care din 0113 e ascuns în spatele paginii «Acasă» (`Visible` fals) — deci nu mai pornea deloc.
  Acum `KbotForm.PornesteTutorialulDePornire` (după încărcarea datelor) cere `HelpService.StartOpeningTutorial` cât fereastra principală e pe ecran.
- **Turul:** `tur-fereastra` rămâne în lista «?»; doar pornirea automată a plecat (`HelpTourRunner`: scoase `initial`, `_completed`, `InitialTourSeen`).
- **Ribbonul în tutoriale:** `AvacontMain.Tutorial.vb` (nou) — `AvacontMain` implementează `IKBotTutorialHost` și trimite restul spre `KbotForm`
  (ancore, semnale, `TutorialBegin/End`). Ancore: `ribbon-tab.<nume>` (configurari / angajamente / e-factura / extrase), `ribbon.<cheie>`, `ribbon.right`.
  Semnale: `ribbon-tab:<nume>` (și când fila era deja deschisă) și `ribbon:<cheie>` (după ce comanda a rulat). `KBotRibbon`: `TabClicked`,
  `TabScreenBounds`, `RightEndScreenBounds`; `ButtonScreenBounds` își asigură acum așezarea (`EnsureLayout`).
- **Vălul:** `TutorialRunner.RootWindow` — o pagină găzduită (shell, «Acasă») nu mai e „fereastra” tutorialului; vălul, chenarul și bula aparțin ferestrei
  principale, deci vălul acoperă și ribbonul și panoul lateral. Atinge TOATE tutorialele care lucrează în shell (vezi neverificat).
- **Ajutor (0000-107):** `contabil/ajutor.md`, `contabil/setari.md`, `HelpContent/README.md`, `docs/HELP_SYSTEM.md`. Fișierele `tutoriale-intro.md` și `setare-tutorial-initial.md` (cum pornești / oprești tutorialul de început; pornea prin butonul MENIU, ascuns din 0114) șterse, la cererea operatorului.

## Files touched

`src/KBot.Common/AppSettings.vb` · `src/KBot.Controls/Ribbon/KBotRibbon.vb`, `KBotRibbonItems.vb` · `src/KBot.App/AvacontMain.vb`, `AvacontMain.Tutorial.vb` (nou),
`KbotForm.vb`, `KBot.App.vbproj` · `src/KBot.App/Help/HelpService.vb`, `HelpTourRunner.vb` · `src/KBot.App/Tutorial/TutorialRunner.vb` ·
`src/KBot.App/Setari/SetariAplicatieView.vb`, `.Designer.vb` · `src/KBot.App/HelpContent/tutorials/tutorial-pornire.md` (nou), `tutoriale-intro.md` (șters),
`contabil/ajutor.md`, `contabil/setari.md`, `README.md` · `docs/HELP_SYSTEM.md` · status.

## Test results

`dotnet build src\KBot.App\KBot.App.vbproj` (ieșire în folder de lucru): **0 avertismente, 0 erori**. `Check-Help.ps1 -Coverage`: nicio eroare despre fișierele acestei felii; rămân
22 de erori vechi, ale altor fișiere (tur-setari, `efactura-factura-noua`, `ordonantare-din-plata`, `efactura/index.md`), și un avertisment așteptat: `TokenForm` nu implementează
`IKBotTutorialHost`. Nu s-au scris și nu s-au rulat teste (regula: niciodată teste nerecerute).

## Left unverified or deferred

- **Nimic văzut pe ecran.** De urmărit, în ordine: (1) chenarul pe file și pe butoane (pozițiile vin din așezarea ribbonului; după apăsarea unei file, chenarul se mută la
  următorul tic de 150 ms); (2) pasul 3: butonul «Token ANAF» deschide fereastra modală — chenarul pe «Închide» trebuie să apară în ea; (3) pasul 4: un clic pe «Reînnoiește»
  pornește pasul cu certificatul, tutorialul trece la «Închide» cu acea fereastră deschisă; (4) pasul 8: arborele trebuie să fie încărcat; (5) pasul 12: dacă cardul «Ce este nou»
  e deja strâns, apăsarea lui îl deschide la loc (textul pasului acoperă ambele cazuri); (6) pornirea automată: `Starts = AvacontMain`, după ce `KbotForm` și-a încărcat datele.
- **Ribbon strâns** (dublu clic pe o filă): butoanele nu sunt pe ecran, deci pașii pe butoane spun „nu e pe ecran”; tutorialul nu desface ribbonul.
- **Vălul pe toată fereastra** schimbă și tutorialele mai vechi din shell (acum clicul pe ribbon în timpul lor întreabă „Vrei să ieși?”). Nu am rulat niciunul.
- **NOUTATI.md** nu s-a atins (se scrie la publicarea versiunii). Linie de adăugat, fără diacritice: «Tutorial scurt la pornire, prin panglica de sus; se opreste din Setari > Aplicatie.»
- `TokenForm` nu e gazdă de tutorial (generic funcționează, ca `SetariForm`); `Check-Help.ps1` avertizează.

## Second pass — the main window kept its veil behind a modal window (operator, 18.10.2026)

Report: with a modal pop-up on screen the background loses its grey tint. **Cause (read in the code, not seen on screen):** the veil is laid over the step's host window
only; when the step works in a window of its own (the token window), the host is that window and the main window behind it had no veil.
**Fix:** `TutorialRunner` makes a second veil (`_back`, `WindowBehind`) over the main window while the host is another window, with a hole where that window stands;
it follows the first veil's visibility and goes with the presenter. Build: 0 warnings, 0 errors. **Not seen on screen.** Unchanged on purpose: the veil still steps back
while a window that is not the tutorial's (a file chooser, the certificate picker after «Reînnoiește») is the active one.

## Third pass — «Înainte» with its sign (operator, 18.10.2026)

Request: where a message says «Apasă «Înainte»», the reader does not know which button that is; add the button's sign in brackets. The button reads «Înainte ►»
(`HelpTourBubble`, `TutorialRunner.Present`), so every step / tour text and the two lines of `contabil/ajutor.md` that name it now read «Înainte» (►) — 52 places in
19 tutorial / tour files + 2 in `ajutor.md`; the section tags got `000T-13` (`ajutor.md`: `000T-13, 0000-107`). «Gata» has no sign on its button, so it is unchanged.
`Check-Help.ps1`: no new errors. Not seen on screen: the sign is the character U+25BA, drawn by the bubble's text engine (the button itself shows the same character).

## Fourth pass — «Înainte» described, not drawn (operator, 18.10.2026)

The sign (►) of the third pass did not look like the button: the bubble's buttons are green icons (a bar and an arrow), not text, and the text engine cannot draw an
image. Operator chose «words with a sign»: the 52 places now read «Înainte» (săgeata verde cu bară, jos în dreapta, ⇥) — the same wording in tutorials, tours and the two
lines of `ajutor.md`. Tags unchanged (already `000T-13`). The real icon inside the text would need an inline-image run in `KBotRichText` (not done). Not seen on screen: whether U+21E5
is drawn by the font in use.
