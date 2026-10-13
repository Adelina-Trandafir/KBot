# SLICE-0000-69 - Ajutorul pentru 0113: fereastra principala noua, machetele ca pagini, fara butoane de inchidere (13.10.2026)

Prima din sase sub-felii de ajutor pentru noua fereastra principala (0000-69 … 0000-74; feliile 0113 … 0120).
Facut dupa `docs/HELP_SYSTEM.md` §4 si §5, din codul din `src` (nu din MERGE_NOTES). Nimic rulat, nicio poza facuta.

## Ce s-a schimbat si de ce
- `contabil.fereastra`: zonele ferestrei principale rescrise (bara de titlu + ribbon, panoul lateral, pagina; lista
  angajamentelor are la randul ei cele patru zone vechi); sectiune noua **«Machetele se deschid ca pagini»**
  (una cate una, titlul in bara de titlu, «Inchide» din ribbon / Esc, revine pagina de dedesubt, ce ramane fereastra
  separata); «Grupe de angajamente» = pagina, nu mai blocheaza fereastra; nota de la «Unitatea de lucru» = machete;
  sectiunea **«MENIU» inlocuita** de «Comenzile: ribbonul»; «Cum porneste fereastra» rescrisa (porneste pe «Acasa»,
  pornirea marita bifata o data, K-BOT se inchide din X). `screens:` fara `KbotForm.btnMeniu` (ascuns), cu `AvacontMain`.
  Fraza «La primele porniri, K-BOT iti arata singur turul ferestrei principale» SCOASA: turul initial nu mai porneste
  (problema cunoscuta, de reparat separat); se pune la loc odata cu reparatia.
- `contabil` (Despre K-BOT): o fraza despre ribbon, «Acasa», machete ca pagini.
- Machete fara butoanele proprii de inchidere: `contabil.nomenclatoare` (machete din Date › Nomenclatoare),
  `contabil.nomenclatoare.parteneri` (fara «Iesire»), `contabil.asocieri.fereastra` (randul «Renunta» din tabel),
  `contabil.asocieri.cazuri` (renuntarea = «Inchide» / Esc; `screens:` fara `AsociereForm.btnRenunta`),
  `contabil.vederi.sumar` (asocierea partenerilor), `contabil.ord.editor`, `contabil.vederi.rezervari` (graficul),
  `contabil.forexe.descarcare` (Asocieri ca pagina), `contabil.vederi.extrase`, `contabil.notecab` (ribbon
  Angajamente › Verificari; fara «Iesire»), `contabil.ddf.nou` (ribbon Angajamente › Adaugare), `contabil.forexe.browser`
  («Creeaza in FOREXE»), `contabil.forexe.descarcare-multipla` si `avansat` (Setari din ribbon).
- `HelpContent/README.md`: lista `goto:` (`menu:jurnal` = macheta din 0118, `setari:jurnal` disparut, ce nu se poate
  atinge inca prin `goto:`), lista de capturi e in ribbon (Configurari › Administrare).
- Eticheta `<!-- slice: 0113 -->` (si 0114 / 0115 unde e cazul) pe fiecare sectiune atinsa.

## Fisiere
`src/KBot.App/HelpContent/`: `contabil/fereastra.md`, `contabil/index.md`, `contabil/nomenclatoare/index.md`,
`contabil/nomenclatoare/parteneri.md`, `contabil/nomenclatoare/clasificatii.md`, `contabil/asocieri/fereastra.md`,
`contabil/asocieri/cazuri.md`, `contabil/asocieri/pasi.md`, `contabil/vederi/sumar.md`, `contabil/vederi/ord-editor.md`,
`contabil/vederi/rezervari.md`, `contabil/vederi/extrase.md`, `contabil/forexe/descarcare.md`, `contabil/notecab.md`,
`contabil/ddf/nou.md`, `contabil/forexe/browser.md`, `contabil/forexe/descarcare-multipla.md`, `avansat/index.md`,
`README.md`, `help-version.txt` (`2026-10-13`).

## Teste / verificare
- `tools\HelpCheck\Check-Help.ps1 -Coverage -Map` la sfarsitul lui 0000-74: vezi worklog-ul 0000-74 (subiectele: nicio
  eroare; raman erorile tururilor / tutorialelor de mai jos si cele vechi).
- Fara build (doar fisiere de ajutor, copiate la build) si fara rulare, dupa regula proiectului.

## Capturi de refacut (toate cele sase sub-felii, 13.10.2026)
Rosii in lista «Capturi pentru ajutor» (`redo: 2026-10-13 17:30`, eticheta pastrata, poza veche neatinsa):
- 0113 / 0114 / 0115: `fereastra-principala`, `fereastra-zone`, `unitate-selector`;
- 0113 (machete ca pagini, fara bara de titlu proprie si fara buton de inchidere): `extrase-fereastra`, `clasificatii`,
  `parteneri`, `grafic-rezervari`, `asocieri-fereastra`, `asocieri-tragere`, `asocieri-propunere`, `ord-editor`;
- 0113 / 0120: `ddf-editor`, `angajament-nou`;
- 0113 / 0118: `setari`.
Noi (lipsa, in aceeasi lista): `acasa`, `ribbon-file`, `ribbon-configurari`, `ribbon-strans`, `panou-lateral`,
`panou-lateral-strans`, `ajutor-buton-pagina`, `ajutor-buton-meniu`, `jurnal-activitate` (`goto: menu:jurnal`),
`istoric-versiuni`, `ribbon-efactura`, `efactura-cumparare` (toate cu `prepare:`; doar jurnalul are `goto:`, vezi mai jos).
Doar `prepare:` actualizat (poza nu exista inca sau aspectul nu s-a schimbat): `operatiuni-necorelate`, `date-unitate`,
`conturi-unitate`, `efactura-token`, `ajutor-meniu` (pastreaza `redo`-ul ei vechi).
Nerosii, verificate pe poza: capturile vederilor (doar interiorul vederii) si ale paginilor din Setari (doar interiorul
paginii: `setari-autentificare`, `avansat-*`, `setari-descarcari-multiple`), `ajutor-cautare` (doar meniul «?»),
`asocieri-grafic` («Grafice si benzi» ramane fereastra separata), `verificare-buget` (dialog).

## Navigarea ajutorului (pasul 6 al sesiunii) — NIMIC schimbat in cod, asteapta confirmarea
`HelpService.Navigate` gaseste `KbotForm` prin `Application.OpenForms` (presupus: forma gazduita, cu handle, e in
lista — operatorul foloseste deja lista de capturi, deci merge) si cheama `KbotForm.NavigateForCapture`:
1. **`view:<cheie>` NU ajunge cand pe ecran e «Acasa» sau alta macheta**: selecteaza vederea in lista angajamentelor,
   dar lista ramane ascunsa (nu se cheama `AvacontMain.ShowShell`). Tinta e atinsa doar daca lista e deja pe ecran.
   Atinge: 33 de capturi `goto: view:*`, 16 subiecte cu `open: view:*` (butonul «Deschide «...»»), tururile cu `goto: view:*`.
   Tot acolo, `WindowState` / `Activate()` lucreaza pe forma gazduita, nu pe fereastra principala (minimizata nu revine).
2. **`menu:<cheie>` ajunge** pentru intrarile care deschid o macheta (extrase, efactura = «Vanzare», clasificatii,
   parteneri, operatiuni_necorelate, jurnal, setari, angajament_nou = editorul DDF ca pagina): `MenuNou_ItemClicked`
   le deschide prin registru ca pagini. Doar textul butonului «Deschide «...»» vine din randul meniului vechi
   (`TargetCaption`): `menu:efactura` spune «E-Factura», nu «Vanzare».
3. **`setari:<pagina>` ajunge** (Setari = pagina, `ShowFor` + `ShowPage`), **in afara de `setari:jurnal`** (si
   `istoric`): pagina nu mai exista, `navViews.SelectedKey` arunca, iar ajutorul spune «Destinatia ... nu e cunoscuta».
   `avansat.jurnale` a trecut pe `open: menu:jurnal`; ramane pasul din `tur-setari` (vezi mai jos).
4. **Fara `goto:` deloc**: pagina «Acasa», o fila a ribbonului, «Istoric versiuni», «E-Factura Cumparare», «Token ANAF»,
   «Conturi Unitate», «Date Unitate» (doar butoane din ribbon, fara rand in meniul vechi). Capturile lor au `prepare:`.
**Propunere de cod (de confirmat):**
- `KbotForm.NavigateForCapture`: la `view:` (si la intrarile `angajament_nou` / `angajament_forexe`) aduce intai lista pe
  ecran (`TryCast(HostForm, AvacontMain)?.ShowShell()`; False = macheta deschisa a refuzat -> mesaj); restaurarea si
  activarea pe fereastra de sus (`TopLevelForm`/`HostForm`), nu pe forma gazduita.
- tipuri noi de `goto:`: `home` (`AvacontMain.ShowHome`) si `ribbon:<cheie buton>` (ce face un clic pe butonul din
  ribbon, cu fila lui aleasa: `istoric_versiuni`, `efactura_cumparare`, `pagina_angajamente`, `ef_token`, `unitate_*`;
  o fila: `ribbon:tab:<nume>`); `TargetCaption` ia textul butonului din ribbon. Atinge `KbotForm.HelpCapture.vb`,
  `AvacontMain.vb` (o metoda Friend), `HelpLibrary.GotoPattern`, `$GotoPrefix` din `Check-Help.ps1`, `HelpContent/README.md`
  (§6 din HELP_SYSTEM.md). Dupa aceea capturile noi primesc `goto:`.

## Tururi si tutoriale (pasul 7 al sesiunii) — NEMODIFICATE, asteapta confirmarea
Tururi (`Check-Help` le raporteaza doar pe primul; restul sunt rupte la rulare):
- `tur-setari`, pasul «Jurnal» (`part: item:jurnal`, `goto: setari:jurnal`): pagina nu mai exista — eroare de verificare.
- `tur-fereastra` (turul initial, `HelpService.InitialTourId`), pasul «Butonul MENIU» (`KbotForm.btnMeniu`, `reveal: menu`):
  butonul e ascuns, iar regula «ascuns de stare -> aratat pentru pas» (0000-30) l-ar putea **arata** in lista gazduita.
- `tur-ddf`, pasul «De unde porneste» (`KbotForm.btnMeniu`).
- `tur-notecab`, pasul «Unde faci o nota noua» (`KbotForm.btnMeniu`, `reveal: menu`).
- `tur-ddf-editor`, pasul «Renunta» (`DdfEditForm.btnRenunta`, acum ascuns): aceeasi regula l-ar arata in tur.
- toate tururile cu `goto: view:*` (`tur-fereastra`, `tur-ddf`, `tur-istoric`, `tur-notecab`, `tur-ord`, `tur-plati`,
  `tur-receptii`, `tur-rezervari`, `tur-sumar`): pornite de pe «Acasa», raman pe «Acasa» (punctul 1 de mai sus).
Tutoriale:
- deschise din MENIU (pas `target: KbotForm.btnMeniu` + `anchor: menu.<cheie>`): `clasificatii-adaugare`,
  `clasificatii-buget`, `clasificatii-rectificare`, `parteneri`, `fereastra-extrase`, `efactura-factura-noua`,
  `setare-tutorial-initial`. Butonul e ascuns, iar ribbonul (desenat, fara parti de ajutor) blocat de val: utilizatorul nu
  poate deschide macheta fara sa iasa din tutorial.
- `fereastra-extrase`, ultimul pas `ExtraseForm.btnInchide` (`wait: click`): butonul e ascuns.
- toate cu `starts: KbotForm` (20): de pe «Acasa» lista e ascunsa, deci tutorialul nu gaseste fereastra de start.
- `wait: opens:<Form>` pe machetele devenite pagini (Clasificatii, Extrase, Facturi, Grupe, Asociere, OrdEdit, Parteneri,
  DdfEdit, Setari): `FindTarget` cauta in `Application.OpenForms` formele vizibile — probabil merg; neverificat.
**Propunere (operatorul: «urmeaza sa ramana doar tutorialele»):**
1. Tururile: nu le repar unul cate unul; le retrag (fisierele din `tours/` si legaturile «Tur ghidat» din subiecte, ajutorul
   «Tururile ghidate» din `contabil.ajutor`), odata cu reparatia turului initial — care atunci devine un tutorial sau dispare
   (`InitialTourId`, comutatorul din Setari). Pana atunci raman cum sunt.
2. Tutorialele: pasii de deschidere trec de la MENIU la ribbon. Cere cod: `KBotRibbon` implementeaza `IKBotHelpParts` (parti
   `tab:<nume>` si `button:<cheie>`), `$PartsByType` in `Check-Help.ps1`, iar pornirea unui tutorial cu `starts: KbotForm`
   aduce lista angajamentelor pe ecran. Apoi rescriu cele 7 tutoriale (pas «Alege fila ...», pas «Apasa ...» pe parte a
   ribbonului, `wait: opens:` neschimbat) si scot pasul `ExtraseForm.btnInchide` (inchiderea = «Inchide» din ribbon).

## Neverificat / de citit de operator
- Nimic vazut pe ecran. Textele sunt scrise din cod (`AvacontMain.vb`, `.Designer.vb`, `AvacontMain.Sidebar.vb`,
  `AcasaPage.*`, `KbotForm.Machete.vb`, `KbotForm.EFactura.vb`, `KBotRibbon.vb`, `KBotSidePanel.vb`).
- «Esc inchide macheta, ca inainte»: Esc e ascultat doar pe machetele al caror buton de anulare a fost ascuns; restul se
  poarta ca inainte. Fraza din ajutor e generala.
- Textul de pe ecran invechit, lasat cum e (cod): comutatorul «Randul «Jurnal activitate» in meniul MENIU al ferestrei
  principale» (citat exact in `contabil.setari`) si mesajul «Reinnoiti tokenul cu butonul «Token ANAF» din subsolul
  ferestrei.» (ajutorul spune unde e butonul, fara sa citeze mesajul).
