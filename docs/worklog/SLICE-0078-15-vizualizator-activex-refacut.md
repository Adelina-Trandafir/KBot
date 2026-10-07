# SLICE-0078-15 — Vizualizatorul ActiveX (AcroPDF) refăcut de la zero

Cererea operatorului, 06–07.10.2026. **Doar motorul ActiveX** — fereastra găzduită (`AdobeReaderHost`) nu e
atinsă (regula firului: «THIS REFERS ONLY TO THE ACTIVEX PDF, NOT HOSTED WINDOW»).

Partea din 06.10 și dimineața lui 07.10 a fost pusă în git ca **SLICELESS** (commit `c42929e`, «SLICELESS -
Modificare mod incarcare AcroPDF In activex controll»), pentru că eram încă în teste. **Tot ce s-a făcut pe PDF-ul
ActiveX în aceste două zile e o singură felie: 0078-15.** Codul o numea deja așa.

Starea la sfârșit: **GATA pe cod** (build `KBot.App` Debug: **0 erori, 0 avertismente**). Deschiderea, retrimiterea
mărimii, Ctrl+H, capcana «Salvare ca» și închiderea curată sunt **probate pe bancă** de operator. **Ctrl+S automat
după semnătură NU a mers** (§6). Nimic rulat în aplicație cu un DDF real pe server.

---

## 1. De ce

Pe 06.10 operatorul: «there is a big issue with the opening interface (the activex one)». Cu vechiul vizualizator
(`AcroPdfSurface`):

- documentul rămânea **gol până la un clic** în panou, mai ales la primul document dintr-un control nou;
- apăreau **erori JavaScript la întâmplare** («Warning: JavaScript Window», «JavaScript Debugger»), pe același
  document, cu aceleași setări;
- tastele (Ctrl+H) trimise prea devreme nu aveau efect;
- totul stătea pe **patru cronometre** și pe bucle de așteptare.

Decizia operatorului: **un vizualizator nou, construit pas cu pas**, fiecare pas verificat în jurnale pe PC-ul lui.
Vechiul rămâne doar ca sursă de inspirație («we're committed on the new implementation… we'll draw from it»), fără
comutator între ele. Pe 07.10, după probe, **vechiul a fost scos complet din proiect**.

---

## 2. Ce făcea vechiul vizualizator (`AcroPdfSurface`, șters pe 07.10.2026)

Construit în felia 0078-03 și trecerile ei (24.09–06.10.2026). Pașii, în ordine:

1. **Încărcarea** prin `LoadFile` (reflecție pe `AcroPdfHost`).
2. **«Trezirea»:** Adobe amână prima așezare până primește intrare; vechiul dădea focus + o redimensionare de 1 px.
3. **Așteptarea arborelui de panouri:** până la 10 încercări × 400 ms, pornirea la rece până la 20 s, plafon 40 s.
   Control gol după toate astea ▸ controlul **recreat și documentul reîncărcat o dată**.
4. **Colapsarea panourilor:** clic pe butonul Adobe de restrângere (`AVExpandCollapseButtonView`, apăsat de două ori
   dacă era deja restrâns), apoi `ShowWindow` ascuns pe `AVDockableTabStripView` și `AVTaskPaneHostView`.
5. **Antetul documentului** (`AVDocumentHeaderView`) ascuns, iar înălțimea lui dată ferestrei de sub el, reaplicat
   de un cronometru de 400 ms (15 bătăi după încărcare, 3 după o redimensionare).
6. **Calea «mod citire»** (motorul `ActiveXCitire`, 0078-03 trecerea 03): în locul pașilor 3–5, Ctrl+H (+ F8,
   + Ctrl+2) când pagina era așezată, cu condițiile: formularul activ, în prim-plan, focus în control, fără alerte
   de script. Plus:
   - așteptarea «liniștii» lui Adobe: minim 3000 ms de la încărcare și 1000 ms fără evenimente, verificate de un
     cronometru de 250 ms;
   - controlul gol: verificare o singură dată după 5 s (cronometru), control înlocuit și document reîncărcat;
   - după o redimensionare: Ctrl+2 din nou după 500 ms (cronometru).
7. **Capcana «Salvare ca»** (`AdobeSaveTrap`, procesele găsite prin `OwnerPids`), închiderea alertelor de script și
   răspunsul «Nu» la «salvați modificările?» (`PumpClosePrompt`, minim 300 ms).
8. **Jurnalul `acropdf_trace.log`**: sesiuni de înregistrare de la încărcare până la document deschis.

**Cronometre:** `_refitTimer` (400 ms), `_settleTimer` (250 ms), `_deadCheckTimer` (5 s), `_resizeTimer` (500 ms),
plus buclele `Task.Delay` ale așteptării.

**Pe 06.10, înainte de rescriere,** vechiul fusese deja redus în două trepte:
- **«Mereu calea mod citire»:** pașii 3–5 comentați.
- **«HANDS-OFF TEST»:** doar `LoadFile`, nimic altceva, ca să se vadă ce face Adobe singur.

**De ce nu era bun** (măsurat pe 06.10):
- **Primul `LoadFile` într-un control nou** din fereastra principală K-BOT nu se lega de control: Adobe își crea
  `AVL_AVWindow` ca fereastră de sus, ascunsă, 300×200. Reîncărcarea după 5 s ascundea problema.
- **Tastele trimise la 2 s după încărcare,** cu panoul din dreapta încă deschis, nu aveau efect. De aici așteptarea
  pe cronometru, care întârzia totul.
- **Cronometrele rulau pe firul interfeței,** peste jurnalul de pe bancă, iar banca se bloca.

`AcroPdfStatus` / `AcroPdfResult` (folosite și de cel nou) au fost mutate neschimbate în `AcroPdfResult.vb`.

---

## 3. Ce face acum vizualizatorul nou (`AcroPdfViewer`)

`src/KBot.Controls/Adobe/AcroPdfViewer*.vb`, o clasă parțială în mai multe fișiere. Folosit de banca ActiveX și de
`ReaderHostPreview` (DDF, ORD, Note CAB) pentru ambele valori ale motorului ActiveX (`ActiveX` vechi rămas în
`kbot_paths.json` pe unele PC-uri, și `ActiveXCitire`).

**Fără cronometre în vizualizator.** Totul pornește din evenimente de ferestre Windows (WinEvent, în afara
procesului). Singurul cronometru rămas pe această rută e verificarea de 200 ms a capcanei comune (`AdobeSaveTrap`),
neschimbată.

| Pas | Fișier | Ce face |
|-----|--------|---------|
| Încărcarea | `AcroPdfViewer.vb`, `AcroPdfHost.LoadThroughSrc` | Documentul intră prin proprietatea `src` = `file:///…` (`LoadThroughSrc = True`, implicit). `LoadFile` rămâne ca rezervă (bifa de pe bancă). Sincron, fără așteptări. |
| Fereastra Adobe născută 0×0 | `AcroPdfViewer.SizeNudge.vb`, `AcroPdfHost.ResendRectangle` | Cât pagina nu e așezată: dacă o fereastră pusă de Adobe direct în control are 0×0 iar controlul nu, îi retrimite controlului dreptunghiul (`IOleInPlaceObject.SetObjectRects`: 1 px mai îngust, apoi real). Doar geometrie, **documentul nu se reîncarcă**. Max. 3 pe încărcare, la min. 250 ms. Jurnal: «fereastra Adobe din control era goală (0×0) — i-am retrimis dimensiunea controlului». |
| Modul citire (Ctrl+H) | `AcroPdfViewer.ReadMode.vb` | Pleacă imediat ce `AVPageView` e vizibilă **cu mărime**, cu condițiile: formularul activ și în prim-plan, focusul pus și găsit în pagină, fără rafală de alerte de script. **Verificat, nu presupus:** modul citire = `AVTaskPaneHostView` și `AVDockableTabStripView` există și sunt ascunse (stilul lor propriu). Dacă e deja pornit, nu trimite (Ctrl+H comută). Dacă după trimitere nu apare: **retrimis o dată**, la primul eveniment de după ≥ 700 ms, când `AVDocumentHeaderView` are înălțime (interfața Adobe gata). După 2 trimiteri renunță și cere operatorului Ctrl+H. |
| Ctrl+2 (pagina pe lățime) | `AcroPdfViewer.ReadMode.vb` | Opțional, din Setări (`AcroPdfFitWidth`, implicit oprit): o dată, după ce modul citire e confirmat. |
| Urmărirea | `AcroPdfViewer.LightWatch.vb` / `.Watch.vb` | Două urmăriri interschimbabile (`DetailedWatch`): **mică** (implicită, și în aplicație) = doar ce le trebuie reparațiilor, nu scrie nimic, se oprește singură când nu mai e nimic de făcut; **mare** (ACTIVEX-CHECK) = fiecare eveniment, arbori, eliberare, textul casetelor Adobe în `Logs\activex_check.log`. Codul mare rămâne pentru investigații viitoare, oprit. |
| Capcana «Salvare ca» | `AcroPdfViewer.SaveTrap.vb` | Portată din vechi, aceeași funcționare: `AdobeSaveTrap` pe procesele Adobe ale controlului (`OwnerPids`, broker + renderer + tot ce a pornit K-BOT), «Nu» la «salvați modificările?» la înlocuire / închidere, alertele de script închise doar dacă sunt în lista din Setări. Suspendată cât vizualizatorul nu e pe ecran. Sfârșitul unei rafale de alerte repornește Ctrl+H. |
| Ctrl+S după semnătură | `AcroPdfViewer.SaveKeys.vb` | Opțional (`AcroPdfSaveAfterSignature`, implicit oprit): `RequestSave` ▸ Ctrl+S pe același drum și cu aceleași condiții ca Ctrl+H ▸ `SaveKeysSent` / `SaveNotSent` către sesiunea de semnare. **Nu a mers la probă (§6).** |
| Eliberarea | `AcroPdfViewer.vb` | `Clear` distruge controlul (următorul document primește unul nou). **Eliberare la `FormClosed`** reactivată pe 07.10 după-amiază: controlul pleacă cât ferestrele lui încă există. Procesele Adobe rămân pornite și sunt refolosite (decizia operatorului). |

**Păstrate în cod, oprite** (cererea operatorului: «nu șterge cod încă»):
- **primerul** (un PDF gol încărcat înaintea documentului, `empty_pdf.pdf`) și cronometrul lui;
- **hook-ul de mouse** `WH_MOUSE_LL`;
- **reactivarea formularului** pe `FOREGROUND`;
- **urmărirea mare** ACTIVEX-CHECK.

**Șterse încă din 07.10 dimineața:** evenimentele controlului (`AcroPdfEventSink.vb`, `AcroPdfViewer.Events.vb`),
pentru că `OnError` / `OnMessage` nu s-au declanșat niciodată.

### În aplicație

- **`ReaderHostPreview`:**
  - creează `AcroPdfViewer`, cu capcana pornită și evenimentele `DocumentSaved`, `SaveTrapFailed`, `SaveKeysSent`,
    `SaveNotSent`;
  - `FitWidthAfterReadMode` se ia din Setări la fiecare document;
  - `RequestSave` pe ActiveX trimite Ctrl+S doar cu bifa pusă; altfel scrie în jurnal și răspunde `False`, ca
    înainte.
- **Acțiune unică `0078-15-activex`** (`OneTimeActions.RunActiveXEngine`, la prima pornire, **fără mesaj**):
  - motorul «Fereastră găzduită» trece pe `ActiveXCitire`;
  - un motor deja ActiveX rămâne neschimbat;
  - rezultatul se notează în `kbot_paths.json` și în `adobe_preview.log`.
  Pe un PC nou rulează întâi acțiunea veche din 0078-05 (fereastra găzduită), apoi aceasta.
- **Setări ▸ Aplicație ▸ Documente:** două bife noi, vizibile doar pe ActiveX, ambele **debifate implicit**:
  - «ActiveX — pagina pe toată lățimea la deschidere» (`chkAcroLatime`, `AcroPdfFitWidth`);
  - «ActiveX — salvare automată după semnătură» (`chkAcroSalvare`, `AcroPdfSaveAfterSignature`).
- **Teste de pe 06.10 încă active în `ReaderHostPreview`:**
  - **UNCOVER TEST:** panoul e descoperit și redesenat înainte de încărcare;
  - **`LocalCopyDeleteEnabled = False`:** ștergerea copiilor locale semnate e dezactivată. ⚠ Asta atinge
    **ambele** rute, inclusiv fereastra găzduită, la cererea operatorului («leave the deletion but deactivate it»).

### Bancurile (DevHarness ▸ «Adobe/PDF», doar Debug)

- **«ActiveX — doar deschiderea unui PDF»** (`ActiveXPdfHarnessForm`):
  - Butoane: deschide / închide; «Îl văd încărcat»; «Jurnalul ActiveX…»; «Arată copia»; «Ctrl+S acum».
  - Bife:
    - **`src`** — bifată;
    - **«Urmărire detaliată»**;
    - **«Semnare și salvare ca în aplicație (pe o copie)»** — bifată; copia se face în `TempPdf`, cu capcana;
    - **«Ctrl+S automat după semnătură»** — bifată; la prima scriere a copiei după «Salvare ca» cere Ctrl+S, ca
      sesiunea din aplicație.
  - Fișierul semnat e citit înapoi de **un singur `FileSystemWatcher`**, refolosit de la o copie la alta, cu citire
    care nu-l blochează pe Adobe (§5.6). Calea veche, pe cronometru (`tmrSaved`), rămâne în cod, oprită de
    `SavedCheckByWatcher`.
  - Bara de sus crește pe două rânduri când nu încape pe ecran.
- **Vizualizatorul `activex_check.log`** (`ActivexLogViewerForm`):
  - două jurnale / încărcări alese de operator, unul lângă altul, în doi arbori: rădăcini pe tip, cu emoticon;
    noduri = elemente; frunze = rânduri;
  - perechea identică din celălalt arbore se selectează singură; cutiile de text de dedesubt arată conținutul
    selecției, fără rupere de rând, cu derulare pe ambele axe;
  - bifa «Ascunde ce e identic» și butoanele diferență anterioară / următoare;
  - export Excel cu ambii arbori unul lângă altul: o coloană pe nivel, o coloană despărțitoare.
- **«Semnare PDF»** (`PdfSigningHarnessForm`): butonul «Îl văd încărcat», cu ora.

---

## 4. Ce am descoperit despre erorile JavaScript

**De când documentul intră prin `src` (07.10.2026 dimineața), erorile JS nu au mai apărut niciodată:** nici pe
bancă, nici la semnare, în nicio încărcare din 07.10.

**Certitudine ~70% (operatorul, 07.10.2026):** e o singură zi de probe, pe un singur PC. Nu e dovedit că se
întâmplă mereu, deci «`src` = fără erori JS» rămâne o ipoteză probabilă, nu un fapt.

Ce știm, în ordine:

1. **28–29.09 (0078-04 / 0078-05):** «GeneralErrorOperation failed» la deschiderea unui DDF semnat, doar pe
   motorul ActiveX. Pe «Fereastră găzduită» cu Reader recent nu apăreau.
2. **05–06.10 (0078-12 / 0078-14):** pe un **Acrobat Pro 2020 vechi și crăpat** apar și pe fereastra găzduită și
   chiar fără K-BOT, deci acolo cauza e Adobe-ul vechi, nu K-BOT. Pe Reader gratuit 2025+, pe același PC, nu apar.
3. **06.10 seara, 11 încărcări cu `LoadFile`** ale aceluiași DDF semnat pe bancă, aceleași setări: erorile au
   apărut **într-una singură** (22:51:16). Ce o deosebea:
   - era o pereche de procese Adobe care **pornise «în modul rău»**: își luase fereastra activă pe `Edit`-ul ascuns
     (la -16384,-16384), iar pagina apăruse doar după reactivare;
   - pagina s-a așezat **cel mai repede din toate** (+857 ms, față de 1,7–4,9 s);
   - prima «Warning: JavaScript Window» a venit la +1104 ms, apoi «JavaScript Debugger» la +6,7 s și încă 6
     avertismente.

   Cealaltă pereche de procese, pornită normal, n-a dat nicio eroare în 7 încărcări. **Cu un singur caz, cauza nu
   s-a putut separa.**
4. **Evenimentele controlului (`OnError` / `OnMessage`) nu se declanșează niciodată,** nici în timpul erorilor
   JS. Abonarea mergea; au fost scoase.
5. **Nu orice «Warning: JavaScript Window» e o eroare.** Pe 07.10 la 09:31:46, după un clic al operatorului în
   document, caseta citită era **validarea formularului** (`app.alert` din scripturile DDF-ului: «Denumirea
   instituției publice trebuie completată / Cod de identificare fiscală…»). Textul casetelor se poate citi acum
   (`BOX TEXT` în `activex_check.log`, cu urmărirea detaliată).
6. **Cu `src`:** pagina a apărut fără clic în toate încărcările de pe 07.10 și **nicio eroare JS**, inclusiv la
   semnare, la «Salvare ca» și la redeschiderea documentului semnat.

**Ipoteză, nedovedită:** erorile veneau din încărcarea prin `LoadFile` într-un Adobe care pornea «în modul rău»
(fereastra activă luată, așezare forțată de reactivare), cu scripturile formularului rulând pe un document încă
neașezat. `src` ia alt drum de încărcare în Adobe. Nu avem un caz cu `src` + erori care s-o infirme. Dacă erorile
reapar, se pornește «Urmărire detaliată» pe bancă și se compară încărcarea rea cu una bună în vizualizatorul de
jurnal; textul casetelor apare în `BOX TEXT`.

**Notă pentru memorie:** regula «erori JS = motorul ActiveX, semnați pe fereastra găzduită» (28–29.09) era
adevărată pentru vechiul vizualizator cu `LoadFile`. Cu cel nou nu mai e confirmată.

---

## 5. Ce am descoperit în tura asta (07.10.2026)

### 5.1 Pagina goală la pornirea de la zero = fereastra Adobe născută 0×0
Am comparat încărcarea 1 (09:35:38, a cerut clic) cu încărcarea 2 (09:35:54, a mers singură):
- **Când procesul Acrobat pornește chiar în timpul lui `src`**, fereastra pusă de Adobe în control («Acrobat External
  Window») se naște **0×0**, toate panourile documentului se construiesc 0×0 și nimeni nu le retrimite mărimea.
- **Clicul operatorului a ajutat prin controlul nostru, nu prin Adobe:** controlul a primit o schimbare de poziție,
  i-a retrimis lui Adobe dreptunghiul și totul s-a așezat în 30 ms.
- **Când procesele Adobe erau deja pornite** la crearea controlului (încărcarea 5), fereastra s-a născut cu mărime,
  chiar și cu Adobe pornit de la zero.
- **Furtul ferestrei active** pe `Edit`-ul ascuns e doar un efect al pornirii: banca și-a luat fereastra înapoi în
  18 ms, iar pagina tot n-a apărut.
- **Reparația:** retrimiterea mărimii (§3). **Probat:** la 10:09:46 și 10:11:33 a intrat o dată, iar pagina a
  apărut singură la ~2 s; cu Adobe deja pornit nu intră.
- **Detaliu:** `src` care înlocuiește un document blochează K-BOT ~0,9 s, cât Adobe închide documentul vechi.

### 5.2 Ctrl+H la pornirea de la zero e ignorat prima dată
- Cu Adobe deja pornit, Ctrl+H prinde în ~200 ms.
- Cu Adobe pornit de la zero, prima tastă (~2,1 s) e ignorată: interfața Adobe încă se construiește, iar bara de
  sus capătă înălțime abia la ~3,4 s.
- **Reparația:** verificare + o singură retrimitere (§3). **Probat la 10:22–10:23:** pornire de la zero ▸ a doua
  trimitere la +3479 ms ▸ mod citire la +3545 ms; controlul refolosit ▸ o singură trimitere.
- **Modul citire NU se păstrează** de la un document la altul în același control.
- **Limită:** un Adobe cu panoul din dreapta și bara de file ascunse din setările lui proprii arată ca «mod citire
  pornit», deci Ctrl+H nu mai pleacă.

### 5.3 Capcana «Salvare ca» pe ActiveX: merge
Pe bancă, pe o copie (12:02, 12:17, 12:30):
- fereastra «Sign Document» lăsată în pace;
- «Save As» prins, calea scrisă și citită înapoi, «Salvare» apăsat, «Confirm Save As» acceptat;
- copia semnată scrisă pe disc (385.230 / 376.234 octeți față de 321.584, sumă schimbată);
- la închidere, «salvați modificările?» ▸ «Nu».

Fără erori. **Pauzele de la semnare erau timpul operatorului** (alesul certificatului, «Salvează»), nu o înghețare.

### 5.4 Adobe scrie fișierul DUPĂ ce se închide «Salvare ca»
- Evenimentul `DocumentSaved` vine la închiderea casetei. Citit imediat, fișierul avea încă mărimea și suma vechi:
  banca a scris fals «conținut IDENTIC».
- Sesiunea din aplicație aștepta deja (`PdfSigningSession`).
- Pe bancă: un `FileSystemWatcher`; ultima schimbare dă starea finală.

### 5.5 Capcana pornea din nou pe documentul vechi
Când opțiunea se seta la aceeași valoare, capcana pornea din nou. Reparat: setter-ul nu face nimic dacă valoarea
nu se schimbă.

### 5.6 ⚠ Riscul de blocare a fișierului (REGULĂ)
- `harness_errors.log`, 12:17:54.422, `PdfHash.ComputeFile`: «fișierul e folosit de alt proces». Banca a citit
  copia cât Adobe încă scria.
- **Citirea picată e inofensivă:** nu a deschis fișierul.
- **Riscul real e citirea REUȘITĂ.** `PdfHash.ComputeFile` deschide cu `FileShare.Read`. Cât durează citirea
  (milisecunde), **Adobe nu poate scrie**. O salvare a lui Adobe exact atunci poate eșua sau rămâne pe jumătate,
  deci PDF-ul semnat se poate strica.
- **Aplicația nu are problema:** sesiunea de semnare citește prin `SignedPdfFiles.ReadShared`
  (`FileShare.ReadWrite Or FileShare.Delete`).
- **Banca citește acum la fel** (`ReadSharedQuiet`), iar citirea picată nu mai ajunge în jurnalul de erori.
- **REGULA:** niciun cod nu citește un PDF pe care Adobe îl poate ține deschis decât cu
  `FileShare.ReadWrite Or FileShare.Delete` (`SignedPdfFiles.ReadShared`). `PdfHash.ComputeFile` și orice
  `File.ReadAllBytes` / `FileShare.Read` pe un astfel de fișier sunt interzise.
- **Neverificat:** dacă mai există în cod astfel de citiri pe fișiere ținute de Adobe, în afara sesiunii de
  semnare.

### 5.7 Eliberarea doar din `Dispose` vine prea târziu
Eliberat doar din `Dispose`, controlul găsea fereastra deja distrusă de Windows («control 0x0, form=(none)»), cu
28 de ferestre Adobe rămase. În acel moment întrebarea «salvați modificările?» nu mai poate primi răspuns.
- **Reparația:** eliberare la `FormClosed`, reactivată.
- **Probat la 12:30 și 12:36:** capcana a răspuns «Nu» la închidere, fără erori de eliberare.

### 5.8 Procesele Adobe rămân după eliberare
Rămân 10–30 s sau mai mult și sunt refolosite la următoarea deschidere. Eliberarea noastră e corectă: ferestrele din
control dispar în sub 1 s. Decizia operatorului: le păstrăm pentru refolosire. `AdobeProcessRegistry` (0078-11) le
închide la ieșirea din K-BOT.

### 5.9 Bara băncii ascundea butoane
Bara avea înălțime fixă (42 px) și nu încăpea pe ecranul operatorului (2718 / 3002 px față de 2538), deci
«Ctrl+S acum» ajunsese pe un rând ascuns. Acum bara crește singură.

---

## 6. Ctrl+S automat după semnătură — NU a mers

**De ce există:** după o semnătură, scripturile formularului (postSign) modifică documentul DUPĂ salvare. Asta
deblochează semnatarul următor (A → B → Ordonator, probat pe 28.09 pe fereastra găzduită cu un Ctrl+S suplimentar).
Fără el, pe ActiveX, **B sau Ordonator s-ar putea să nu fie deblocați**. Din cauza asta Ctrl+S a fost adăugat ca
opțiune.

**Probele (bancă, copie, 07.10):**

| Ora | Ce s-a întâmplat | Rezultat |
|-----|------------------|----------|
| 12:29–12:30 | Semnat, «Salvare ca» prins, copia scrisă. Butonul «Ctrl+S acum» nu se vedea (bara ascunsă, §5.9). | Ctrl+S netrimis. |
| 12:36 | «Ctrl+S acum» apăsat la 12:36:01 (`RequestSave` ▸ «Ctrl+S trimis documentului» în 8 ms), **ÎNAINTE** de semnătură (fereastra «Signing» la 12:36:10). Fără «Salvare ca», document închis la 12:36:16. | Fișierul nu s-a schimbat. |
| 12:38 | Banca cu «Ctrl+S automat după semnătură». «Sign Document» la 12:38:29, apoi **niciun «Salvare ca» și nicio scriere a copiei** până la închidere (12:38:45). | Ctrl+S automat n-a plecat, pentru că semnătura nu a ajuns pe disc (ce o declanșează pe bancă). |

**Ce nu știm:**
- **Dacă Adobe primește Ctrl+S trimis prin `SendInput` pe ActiveX.** `SendInput` a răspuns OK, dar nu am văzut
  niciodată o scriere după un Ctrl+S. Pe un document nemodificat, Adobe nu salvează nimic la Ctrl+S, deci proba de
  la 12:36 nu dovedește nici că a ajuns, nici că nu.
- **De ce semnarea de la 12:36 și 12:38 nu a mers până la «Salvare ca»:** jurnalul nu spune. Nu există casetă
  prinsă și nici eroare.
- **Dacă pe ActiveX postSign chiar modifică documentul după salvare,** adică dacă mai e nevoie de Ctrl+S.

**Starea codului:** opțiunea e oprită implicit, deci aplicația se poartă ca înainte (fișierul urcă așa cum a fost
salvat la semnare). Se scoate ușor:
- debifezi opțiunea;
- sau ștergi `AcroPdfViewer.SaveKeys.vb`, linia `If _savePending Then SendSaveStep(k_layout)` din `TrySendReadMode`
  și cele câteva linii din `ReaderHostPreview` și din bancă.

---

## 7. Fișiere atinse

**KBot.Controls (`src/KBot.Controls/Adobe/`):**

| Fișier | Starea | Ce |
|--------|--------|----|
| `AcroPdfViewer.vb` | nou | Încărcarea (`src` / `LoadFile`), `Clear`, eliberarea la `FormClosed`, primerul oprit |
| `AcroPdfViewer.LightWatch.vb` | nou | Urmărirea mică; alegerea între urmăriri (`DetailedWatch`) |
| `AcroPdfViewer.Watch.vb` | nou | Urmărirea mare (ACTIVEX-CHECK), păstrată, oprită |
| `AcroPdfViewer.CheckLog.vb` | nou | Jurnalul `activex_check.log` pe fir de fundal |
| `AcroPdfViewer.Release.vb` | nou | Instantanee după eliberare (ACTIVEX-CHECK) |
| `AcroPdfViewer.SizeNudge.vb` | nou | Retrimiterea mărimii |
| `AcroPdfViewer.ReadMode.vb` | nou | Ctrl+H verificat și retrimis, Ctrl+2 |
| `AcroPdfViewer.SaveTrap.vb` | nou | Capcana «Salvare ca» |
| `AcroPdfViewer.SaveKeys.vb` | nou | Ctrl+S după semnătură |
| `AcroPdfResult.vb` | nou | `AcroPdfStatus` / `AcroPdfResult` mutate din vechi |
| `AcroPdfSurface.vb` | **șters** | Vechiul vizualizator |
| `AcroPdfEventSink.vb`, `AcroPdfViewer.Events.vb` | create și șterse în aceeași felie | Evenimentele controlului |
| `AcroPdfHost.vb` | modificat | `LoadThroughSrc`, `ResendRectangle` |
| `AdobeNativeMethods.vb` | modificat | `IOleInPlaceObject`, `ReadAllText`, hook-ul de mouse (declarații) |
| `AdobeReadModeKeys.vb`, `AdobeSaveTrap.vb` | modificat | Doar comentarii: referința spre vechi |
| `AdobeReaderHost.md`, `KBot.Controls.vbproj`, `README.md` | modificat | Doar documentație |

**KBot.Common:** `AppSettings.vb` — `AcroPdfFitWidth`, `AcroPdfSaveAfterSignature`.

**KBot.App:**
- `Views/Ddf/ReaderHostPreview.vb` — vizualizatorul nou, capcana, Ctrl+S opțional, UNCOVER TEST, ștergerea oprită.
- `KbotForm.Views.vb` — `ReleaseDocument` condiționat de `LocalCopyDeleteEnabled`.
- `OneTimeActions.vb` — acțiunea unică `0078-15-activex`.
- `Setari/SetariAplicatieView(.Designer).vb` — cele două bife noi.
- `KBot.App.vbproj`:
  - `empty_pdf.pdf` copiat lângă exe;
  - bancurile cu `.resx` excluse din build-urile non-Debug, în locul lui `#If DEBUG`, ca să dispară MSB3042.
- Bancurile din `HarnessTests/`:
  - `ActiveXPdfHarnessForm(.Designer).vb` și `ActiveXPdfHarnessTest.vb`;
  - `ActivexLogParser.vb`, `ActivexLogCompare.vb`, `ActivexCompareExcel.vb`;
  - `ActivexLogViewerForm(.Designer).vb`;
  - `PdfSigningHarnessForm(.Designer).vb`;
  - `#If DEBUG` scos din `DdfSectiuneaBHarnessForm*`, `PdfSaveTimeline`, `PdfSignatureReport`, `RecordingPdfApi`
    (vezi vbproj).

**Teste (06.10, doar ca să compileze):** `tests/KBot.App.Tests/MainFormNavItemsTests.vb`,
`MainFormPoartaDdfTests.vb`, `ForexeAnswerStoreTests.vb`.

---

## 8. Rezultatele testelor

- **Build:** `dotnet build src\KBot.App\KBot.App.vbproj -c Debug` ▸ **0 erori, 0 avertismente** (07.10, după
  ștergerea vechiului).
- **Teste automate:** nescrise, nerulate (regula operatorului).
- **Probe pe ecran,** făcute de operator pe banca ActiveX, cu jurnalele citite:
  - deschidere prin `src` fără clic;
  - retrimiterea mărimii;
  - Ctrl+H cu retrimitere;
  - capcana «Salvare ca» pe copie;
  - «Nu» la închidere;
  - eliberarea la `FormClosed`.
- **Neprobat:**
  - aplicația cu un DDF real (server, sesiune de semnare, lanțul A → B → Ordonator pe ActiveX);
  - acțiunea unică pe un PC de client;
  - Ctrl+2;
  - Ctrl+S (nu a mers, §6).

---

## 9. Rămas / amânat

- **Ctrl+S după semnătură (§6):**
  - de aflat dacă Adobe primește tasta și dacă e nevoie de ea;
  - proba completă A → B → Ordonator pe ActiveX, în aplicație sau pe banca «Semnare PDF» cu server;
  - **de hotărât, înainte de orice actualizare:** acțiunea unică trece pe toată lumea pe ActiveX, iar fără Ctrl+S
    lanțul de semnături poate rămâne blocat după A.
- **De curățat, la cererea operatorului:**
  - UNCOVER TEST și ștergerea copiilor locale oprită (aceasta atinge și fereastra găzduită);
  - primerul, hook-ul de mouse și reactivarea, comentate;
  - calea `tmrSaved` de pe bancă.
- **Valoarea veche `ActiveX` a motorului** (`AdobePreviewEngine.ActiveX`):
  - nu mai poate fi aleasă în Setări, dar se mai citește din `kbot_paths.json` și merge pe același vizualizator;
  - n-am scos-o, pentru că ar cere schimbarea testelor `AdobePreviewEngineSettingTests`.
- **`acropdf_trace.log`:**
  - vechiul vizualizator deschidea sesiunile de înregistrare;
  - pe ActiveX, capcana mai are `Traced = True`, dar nimeni nu mai deschide o sesiune, deci bifa «Jurnal de
    diagnostic detaliat» din Setări nu mai produce nimic pe ActiveX;
  - de hotărât: o scoatem, sau o legăm de urmărirea detaliată.
- **Ajutorul** (`avansat.documente`, `avansat.jurnale`) nu e actualizat; trecut în 0000 ▸ «Ajutor de actualizat».
- **Versiunile** (FileVersion Controls / Common / App) nu au fost mărite: nu s-a cerut încă.
- **Regula din §5.6:** de căutat în cod alte citiri cu `FileShare.Read` pe PDF-uri ținute de Adobe.
