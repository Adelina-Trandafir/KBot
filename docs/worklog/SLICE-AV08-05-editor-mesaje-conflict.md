# SLICE-AV08-05 — Editorul: avertismente, date, mesaje cu câmpul de corectat, conflict, răspunsul salvării

Plan: [`PLAN_AV08_OPERATII_CONTABILE.md`](../PLANS/PLAN_AV08_OPERATII_CONTABILE.md). Deciziile Adelinei (24.11.2026): avertismentele 217 / 347; data operației urmează data documentului; răspunsul complet; la conflict datele rămân, casetele greșite marcate ca eroare, reîncărcare.

## What changed and why

- **Server (`NoteC_editor.py`, `note_manuale.py`, `NoteC.NoteCEroare`)**: refuzul are `camp` (caseta), `parte` + `linie` (linia din Debit / Credit, de la 0), `confirmare`; corpul JSON prin `NoteCEroare.corp()`. Mesajele numesc caseta («Alegeți jurnalul din listă.», «Alegeți contul pe linia 2 de la Debit.», «Completați «Explicație».»…), nu coloana din bază. Explicația e obligatorie mereu (Access).
- **Avertismente** confirmate de operator: `LUNA_NOUA` (Access 217: lună după luna celei mai noi operații a subunității și anului) și `DOCUMENT_DUBLAT` (Access 347: același număr de document în aceeași notă, la document nou sau la schimbarea numărului / notei). Refuz 409 `reason = CONFIRMARE` până când cererea are codul în `Confirmari`.
- **Conflict**: versiunea schimbată → 409 `reason = VERSIUNE`.
- **Răspunsul** salvării: `IdOper, IdDocument, IdNota, NumarNC, NumarDocument, DataDocument, DataOperatie, OperatiiDocument`.
- **Client (`NotaManualaForm` împărțit: `.vb` încărcarea, `.Salvare.vb`, `.Protectie.vb`)**:
  - casetele de text sunt `KBotTextField` (au chenar care se poate colora); verificări înainte de trimitere (clasificație, jurnal, notă, document, număr / fel document, explicație, cont și sumă pe fiecare linie); caseta / linia de corectat primește culoarea de eroare a temei (`BorderColor`, rândul din grilă cu fundal nuanțat), marcajul dispare la schimbare;
  - avertismentul serverului = întrebare Da / Nu, apoi retrimitere cu codul; Nu = salvarea se oprește, datele rămân;
  - conflict: datele rămân, «Salvează» se oprește, apare «Reîncarcă» (reîncarcă contextul și operația);
  - data operației ia data documentului la fiecare schimbare a acesteia (și la alegerea documentului existent pentru o operație nouă);
  - butoanele de jos: «Închide» la stânga, «Salvează» la dreapta (regula operatorului);
  - `Rezultat` (`NotaManualaRezultat`): `DeschideNotaManualaAsync` îl întoarce; «Modificare operații» se reîncarcă și selectează operația salvată (`LoadList(k_select)`).
- `ApiClient.SalveazaNotaManualaAsync` întoarce rezultatul și transformă refuzul în `NotaManualaException` (un 401 rămâne `ApiException`, pentru reautentificare).

## Files touched

`PYTHON/routes/avacont/introducere_date/operatii/NoteC_editor.py`, `note_manuale.py`, `NoteC`; `src/KBot.Api/Avacont/IntroducereDate/Operatii/ApiClient.NotaManuala.vb`; `src/KBot.Domain/Avacont/IntroducereDate/Operatii/NotaManualaInfo.vb`; `src/KBot.App/Avacont/IntroducereDate/Operatii/NotaManualaForm.vb`, `NotaManualaForm.Salvare.vb` (nou), `NotaManualaForm.Designer.vb`, `KbotForm.Operatii.vb`; `src/KBot.App/Avacont/ModificareDate/Operatii/Views/ModificareOperatiiForm.vb`, `ModificareOperatiiForm.Ribbon.vb`; `src/KBot.App/KBot.App.vbproj`.

## Test results

Build App 0/0; `py_compile` fără erori. Nimic rulat, nevăzut pe ecran.

## Unverified / deferred

- Neverificat pe ecran: înălțimea `KBotTextField` în rândurile de 39 px ale antetului și culoarea rândului marcat în grilă.
- Textele mesajelor Access 217 / 347 nu sunt în export; textele KBOT sunt scrise de mine.
