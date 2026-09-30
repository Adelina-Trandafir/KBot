---
id: avansat.documente
title: Documente: PDF, Word, Excel
part: avansat
order: 10
parent: avansat
screens: SetariAplicatieView.cboAdobeMotor, SetariAplicatieView.btnAdobeGazduire, SetariAplicatieView.chkAcroTrace, SetariAplicatieView.chkAcroNou, SetariAplicatieView.btnMesajeAdobe, SetariAplicatieView.cboExcelRibbon, AdobeGazduireForm, AdobeMesajeForm
keywords: adobe, pdf, activex, fereastra gazduita, acropdf, excel, panglica, mesaje javascript, previzualizare
---
<!-- slice: 0072, 0072-01 -->
Fila **Documente** din **Setări › Aplicație** alege cum afișează K-BOT documentele în ferestrele
lui: PDF-urile DDF și ORD, atașamentele Word și Excel.

<!-- capture: avansat-documente | caption: Setări › Aplicație, fila «Documente» | goto: setari:aplicatie | prepare: Alegeți fila «Documente» din pagina «Aplicație». -->

## PDF — motorul de previzualizare
<!-- slice: 0072-01, 0078-05 -->

| Motor | Ce înseamnă |
|-------|-------------|
| **ActiveX** | controlul Adobe (AcroPDF) rulează în interiorul K-BOT |
| **Fereastră găzduită** | fereastra Adobe e mutată în panoul K-BOT |

Semnarea documentelor se face în Adobe, oricare ar fi motorul.

**Opțiuni fereastră găzduită...** (se deschid singure când alegi «Fereastră găzduită»):

- **Instanță nouă Adobe (/n)** — «Da»: Adobe pornește un proces nou, al K-BOT; «Nu»: Adobe poate
  preda documentul unui Adobe deja deschis de tine. «Automat» = Da.
- **La schimbarea documentului** — A: oprește procesul pornit de K-BOT (sigur); B: închide doar
  fereastra și lasă procesul pornit (documentul următor se deschide mai repede).
- **Readu fereastra Adobe la dimensiunea ecranului** — la închiderea K-BOT, Adobe se maximizează
  înainte să se închidă, ca data viitoare să nu se deschidă la mărimea panoului K-BOT.

Doar pentru ActiveX:

- **Jurnal de diagnostic detaliat** — scrie în `Logs\acropdf_trace.log` tot ce face controlul
  Adobe. Pornește-l doar cât cauți o problemă.
- **Control Adobe nou la fiecare document** — la schimbarea reviziei, controlul se distruge și
  documentul se deschide într-unul nou.

## Mesaje de script Adobe
<!-- slice: 0078-03 -->

Adobe arată uneori mesaje «Warning: JavaScript Window» fără importanță. **Mesaje de script
Adobe...** ține lista celor pe care K-BOT le închide singur (câte o expresie pe rând; literele
mari și mici nu contează). Orice mesaj care nu se potrivește rămâne pe ecran.

- Caseta **Probă** verifică pe loc textul unui mesaj față de listă. Textul exact al unui mesaj
  îl găsești în jurnalul Adobe (`adobe_preview.log`).
- **Lista implicită** pune la loc lista livrată cu K-BOT; **Salvează** o păstrează.

## Excel — cum se ascunde panglica
<!-- slice: 0072 -->

- **Macro**: Excel își ascunde singur panglica (o politică a calculatorului o poate refuza).
- **Fereastră**: K-BOT ascunde fereastra panglicii, ca la Word; nu poate fi refuzat.

Word are o singură metodă și nu se configurează.
