# SLICE 0078-09 — Întrebarea Adobe «salvați modificările?» la închiderea documentului

Data: 01.10.2026. Cererea operatorului: când vizualizatorul se închide (sau operatorul dă clic în altă parte),
documentul se închide, dar uneori rămâne nesalvat și apare «Do you want to save changes to '…PDF' before
closing?» (Yes / No / Cancel). Propunere: prinde fereastra și închide-o, sau trimite Ctrl+S înainte de închidere.
Apoi: «the trap works only when the user clicks the save button». Număr: **0078-09** (propus).

## 1. Ce s-a schimbat și de ce

**Cauza (din cod, nu din jurnal).** Capcana `AdobeSaveTrap` reacționează doar la dialogul «Salvare ca». Iar
`AdobeReaderHost.Detach`, `AdobeReaderHost.ReleaseAtScreenSize` și `AcroPdfSurface.Clear` o opreau ÎNAINTE de a
închide fereastra / controlul. Întrebarea de la închidere (nici «Salvare ca», nici alertă de script) apărea deci
fără nimeni care să o urmărească și rămânea pe ecran. La modul de închidere (A) cu proces pornit de K-BOT nu apare
(procesul e oprit); apare când fereastra e a unui Adobe pe care nu l-a pornit K-BOT (închisă doar cu `WM_CLOSE`) și la
motorul ActiveX.

**Alegerea: Ctrl+S nu, răspuns pe dialog da.** Ctrl+S cere focus și prim-plan (în jurnalele din 29.09 s-a pierdut
chiar și așa) și nu merge cât panoul e ascuns, adică exact la «clic în altă parte». Un mesaj către dialog nu cere nimic.
Răspunsul e **«Da»** (salvează); «Salvare ca» care poate urma trece prin capcană ca de obicei, deci fișierul se
întoarce pe propria cale.

- `AdobeSaveTrap.BeginClose` / `ClosePoll`: capcana rămâne pornită cât se închide documentul (una oprită e reluată);
  întrebarea e recunoscută (`AdobeSaveDialogFilter.IsSaveOnClosePrompt`: butoane Da + Nu, sau TaskDialog cu titlu
  «Adobe…»; dacă textul se citește, trebuie să conțină numele documentului) și primește `WM_COMMAND IDYES`
  (`TDM_CLICK_BUTTON` pentru TaskDialog), o singură dată pe fereastră, mutată în afara ecranului.
  Doar cât `_closing` e activ și doar pentru procesele urmărite, deci documentele proprii ale operatorului nu sunt atinse.
- O salvare făcută la închidere NU ridică `Saved` (nu e semnătură; următorul document poate fi deja cel urmărit de sesiune).
- `AdobeWindowTeardown.Run` și `AdobeScreenRelease.Run`: parametru opțional `whileClosing` apelat la fiecare pas al
  așteptării; cât spune că o salvare e în curs, răgazul pornește din nou (cel mult 15 s), ca să nu fie oprit procesul
  în mijlocul salvării.
- `AdobeReaderHost.Detach` / `ReleaseAtScreenSize`: `BeginClose` înainte, `Stop` în `Finally`.
- `AcroPdfSurface.Clear` și înlocuirea unui document în același control: `BeginClose` + `PumpClosePrompt`
  (minimum 300 ms, mai mult cât o salvare rulează).
- `AdobeNativeMethods.IDNO`; `AdobeDialogFacts.HasNoButton` / `Text`.
- Controls 1.59 ▸ 1.60.

## 2. Fișiere atinse

`src/KBot.Controls/Adobe/`: `AdobeSaveTrap.vb`, `AdobeSaveDialogFilter.vb`, `AdobeWindowTeardown.vb`,
`AdobeScreenRelease.vb`, `AdobeReaderHost.vb`, `AcroPdfSurface.vb`, `AdobeNativeMethods.vb`; `KBot.Controls.vbproj`.

## 3. Rezultate

`dotnet build src\KBot.App\KBot.App.vbproj -c Debug`: **0 erori, 0 avertismente**. Nimic rulat, nicio suită
(cerere permanentă), nimic pe ecran, nimic comis.

## 4. Neverificat / amânat

- **Clasa ferestrei întrebării.** Presupusă `#32770` cu butoane cu id 6/7 (MessageBox standard; captura arată așa).
  Dacă e o fereastră proprie Adobe, capcana nu o vede: jurnalul `adobe_preview.log` nu va avea linia
  «Întrebarea Adobe la închidere acceptată cu «Da»», iar întrebarea va apărea ca înainte.
- **«Da» scrie fișierul.** Dacă Adobe salvează pe loc, fără «Salvare ca», nu apare nicio linie de capcană în afară de a
  noastră. Dacă operatorul preferă «Nu» (renunță la modificări), schimbarea e într-un singur loc: `AnswerClosePrompt`.
- **ActiveX:** nu se știe dacă distrugerea controlului AcroPDF întreabă; dacă `LoadFile` blochează pe întrebare, firul UI
  nu poate răspunde (capcana rulează pe el). Cele 300 ms de așteptare pe fiecare schimbare de document sunt o presupunere.
- Ajutor: nicio pagină de ajutor nu vorbea despre această întrebare; nimic de actualizat (dispare, nu apare ceva nou).

## 5. A doua trecere (01.10.2026, după prima rulare)

Jurnalul `temp_logs\jurnal_20261001_173437.log` (17:33:05): capcana a văzut întrebarea — `#32770`, proces urmărit (7220),
titlu «Adobe Acrobat», textul citit — dar **`butonDa=False butonNu=False`**: nu are butoane cu id 6 / 7. Primul filtru
cerea acele butoane, deci a lăsat-o în pace («alt dialog Adobe (lăsat în pace)»). Corectat:
- `IsSaveOnClosePrompt`: titlu «Adobe…» + text care numește documentul închis = destul (butoanele nu mai contează).
- `AnswerClosePrompt`: butonul «Da» se caută după text (`IsYesCaption`: Yes/Da/Ja/Oui/Si/Sim), nu după id; apăsat în
  trei feluri, câte unul pe încercare (WM_COMMAND/BN_CLICKED pe buton ▸ activare + BM_CLICK ▸ BM_CLICK postat),
  maximum 3 încercări; niciodată WM_CLOSE (ar fi «Cancel»). Dialogul NU se mai mută în afara ecranului: dacă nimic nu
  merge, operatorul îl vede și răspunde. La prima vedere, jurnalul scrie TOȚI copiii dialogului (clasă, id, text) — dacă
  nici acum nu merge, se vede din ce e făcut.
- Build: `KBot.Controls` 0 erori / 0 avertismente; `KBot.App` nu s-a putut copia (aplicația rula sub Visual Studio) —
  de reconstruit cu aplicația oprită. Tot nerulat.

## 6. A treia trecere (01.10.2026): «Nu», nu «Da»

Cu «Da» răspunsul a mers, dar Adobe a încercat să scrie fișierul, ținut încă deschis de vizualizatorul care se închidea, și a
arătat «The file may be read-only, or another user may have it open. Please save the document with a different name or in a
different folder.» (OK). Cererea operatorului: «better click NO instead of yes». `AnswerClosePrompt` apasă acum butonul
**«Nu»** (`IsNoCaption`: No/Nu/Nein/Non/Nao; rezerve `IDNO` / `TDM_CLICK_BUTTON IDNO`); nimic nu se mai salvează, deci nu mai
urmează nici «Salvare ca», nici mesajul de fișier ocupat. Așteptarea după răspuns scade de la 4 s la 1 s
(`CloseAnswerWaitMs`). Modificările făcute în document și nesalvate se pierd la închidere — semnăturile au propria salvare
(capcana «Salvare ca» + Ctrl+S după semnătură), deci nu depind de această întrebare. Build `KBot.Controls`: vezi mai jos.
