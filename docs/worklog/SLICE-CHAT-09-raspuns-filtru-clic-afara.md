# SLICE-CHAT-09 — răspuns la mesaj, filtrul V/A/K și închiderea balonului la clic în afară

Data: 2026-10-21. Continuă [SLICE-CHAT-08](SLICE-CHAT-08-robot-si-balon.md). Cerere operator: cele trei lucruri lăsate nefăcute acolo,
plus: **un clic în afara ferestrei de chat o închide singură**. Operatorul nu poate încă încerca cu mesaje de la alții
(nimeni altcineva nu folosește sistemul), deci nimic din ce urmează nu a fost văzut cu mesaje reale.

## Ce s-a schimbat și de ce
- **Răspuns la mesaj.** Sub fiecare mesaj apare cuvântul subliniat «Răspunde»; un clic pe el pune deasupra casetei de scris o
  bandă «Răspuns pentru <autor>: <text>» cu butonul × (renunță). Mesajul trimis poartă `reply_to`; în listă, răspunsul arată
  rândul «Răspuns pentru …» (îl scria deja randarea din CHAT-08). Clicul e găsit după poziție în `RichTextBox` (se verifică
  să fie pe cuvânt, nu doar pe cel mai apropiat caracter); cursorul devine mână deasupra lui. Banda se golește după trimitere.
  Cheia de retrimitere ține cont și de mesajul răspuns.
- **Filtrul V/A/K.** Rând «Afișează: VERCON ADECHIT K-BOT» sub titlu. Se salvează pe server (`POST /api/chat/preferences`,
  aceeași alegere ca pe paginile web — o singură preferință pentru toate), apoi lista se încarcă din nou pentru aplicațiile alese.
  Nu poate rămâne nimic nebifat (cea debifată se pune la loc, cu o remarcă). Un refuz al serverului readuce bifele serverului.
- **Clic în afară.** `AcasaPage` ascultă (`IMessageFilter`) apăsările de mouse din toată aplicația cât balonul e deschis;
  una în afara balonului și a robotului îl închide, iar apăsarea merge mai departe la ce era dedesubt (butonul/lista nu pierd clicul).
  Clicul pe robot rămâne al robotului (deschide/închide). Un clic în altă aplicație Windows nu închide balonul.
- API: `IChatApi.SaveChatPreferencesAsync` are acum și `applications`; `ChatService.SendAsync` primește `replyTo`,
  nou `SaveApplicationsAsync`. Serverul nu s-a schimbat (rutele știau deja `applications` și `reply_to`).
- Ajutor: secțiunea «Discuții» din `acasa.md` descrie răspunsul, filtrul și închiderea; etichete `CHAT-08, CHAT-09`.
- Evidența: felia CHAT are acum fișier de stare (`state/KBOT_STATUS_CHAT.md`), rând în index și acceptare în `Check-Help.ps1`.

## Fișiere atinse
`src/KBot.Controls/Chat/KBotChatBubble.vb`, `KBotChatBubble.Designer.vb`, `src/KBot.Api/IChatApi.vb`, `ApiClient.Chat.vb`,
`src/KBot.App/Chat/ChatService.vb`, `src/KBot.App/AcasaPage.vb`, `HelpContent/contabil/acasa.md`,
`tools/HelpCheck/Check-Help.ps1`, `docs/worklog/KBOT_STATUS.md`, `state/KBOT_STATUS_CHAT.md`.

## Verificări
- `dotnet build src/KBot.App` (spre folder de lucru; aplicația rula): **reușit, 0 avertismente**.
- Teste nerulate (regula operatorului). Nimic văzut pe ecran.

## Neverificat / amânat
- Poziția «Răspunde» pe ecran (găsirea clicului după poziție în `RichTextBox` la 125/150% și cu liste lungi); lățimea rândului
  de filtre la 125/150% (trei casete + eticheta într-un balon de ~360 px logici).
- Clicul în afară: nu s-a încercat cu ferestre modale / meniuri deschise din pagină.
- Un mesaj de la altcineva nu s-a putut vedea: marcarea «citit» și numărul pe robot au fost verificate doar în cod.
- Nu s-a făcut: semnal pe celelalte pagini (operatorul a ales «doar Acasă»); GIF propriu.
