# SLICE-CHAT-08 — chatul comun în K-BOT.app: robotul din «Acasă» și balonul cu mesaje

Data: 2026-10-21. Cerere operator: sistemul de chat din aplicațiile web să fie în K-BOT.app, legat vizual de imaginea K-BOT din colțul dreapta-jos al paginii «Acasă»; la mesaje noi robotul se mișcă, iar fereastra cu mesaje iese din el ca un balon. Felia CHAT-08 desktop; CHAT-01…09 web sunt lucrări distincte în [proiectul și istoricul web](../../PYTHON/docs/chat/PROIECT.md), consolidate prin D001-04 fără renumerotare. În help: `CHAT-08`. Continuarea: [CHAT-09](#slice-chat-09).

Alegerile operatorului (21.10.2026): animația este **desenată din cod** (nu un GIF); robotul și mesajele apar **doar pe pagina «Acasă»**.

## Ce s-a schimbat și de ce

**Server (Python)** — chatul web folosea doar tokenul de portal (`X-Portal-Token`); K-BOT.app are bearer.
- `routes/chat/api.py`: `authorized` are acum două uși. Cu antet `Authorization: Bearer` intră sesiunea K-BOT (`g.session.username` = contul chatului); fără el, ca înainte, tokenul de portal. Aceleași rute `/api/chat/*`, aceeași conversație, același depozit. Corpul vechi (retry, 503, jurnal fără text) a trecut neschimbat în `serve(...)`.
- `routes/auth/guard.py`: `require_session_no_touch` (+ parametrul `touch` în `_resolve`).
- `routes/auth/session_store.py`: `validate_and_touch_ex(token, touch=True)` în ambele depozite (memorie, Redis); cu `touch=False` verifică (inclusiv plafonul absolut) dar **nu prelungește** sesiunea. Motiv: sondajul chatului la 60 s ar ține în viață o sesiune lăsată în pace — exact ce evită și chatul web.

**Client (VB.NET)**
- `KBot.Domain/ChatDtos.vb`: `ChatState`, `ChatMessage`, `ChatMessagePage`, `ChatSendRequest`, `ChatSendResult` (nume de fir ASCII).
- `KBot.Api/IChatApi.vb` + `ApiClient.Chat.vb`: stare, mesaje (`before`/`after`), trimitere, preferințe, marcare citit. Textul mesajelor nu intră în jurnal.
- `KBot.Controls/Chat/KBotChatRobot.vb`: imaginea K-BOT, cu salt + legănat la câteva secunde cât sunt necitite și insignă cu număr (nemișcată). Un singur timer, pornit doar cu necitite + vizibil; oprit dacă Windows are animațiile oprite.
- `KBot.Controls/Chat/KBotChatBubble.vb` + `.Designer.vb`: balonul (contur rotunjit cu coadă spre robot), titlu + «×», lista de mesaje (`KBotRichTextEditor`, doar citire), «Arată mesajele mai vechi», «Cu pseudonim» / nume, caseta de mesaj, «Trimite». Enter trimite, Ctrl+Enter rând nou, Esc închide.
- `KBot.App/Chat/ChatService.vb`: un singur obiect pe toată sesiunea de lucru; sondează (60 s închis → doar numărul de necitite; 5 s deschis → mesajele de după cel mai nou), trimite cu cheie de reîncercare, marchează citit ce se vede în balon, uită totul la altă sesiune. Un sondaj eșuat tace; doar ce cere operatorul arată eroare.
- `AcasaPage`: `picLogo` e acum `KBotChatRobot`; `bubble` stă deasupra, cu vârful cozii pe capul robotului. Pagina ascunsă închide balonul. `AvacontMain` primește `ChatService` prin DI (`Program.vb`: `IChatApi`, `ChatService`).
- Ajutor: secțiunea «Discuții» în `HelpContent/contabil/acasa.md` (etichetă `CHAT-08`).

## Fișiere atinse
`PYTHON/routes/chat/api.py`, `PYTHON/routes/auth/guard.py`, `PYTHON/routes/auth/session_store.py`, `src/KBot.Domain/ChatDtos.vb`, `src/KBot.Api/IChatApi.vb`, `src/KBot.Api/ApiClient.Chat.vb`, `src/KBot.Controls/Chat/KBotChatRobot.vb`, `KBotChatBubble.vb`, `KBotChatBubble.Designer.vb`, `src/KBot.App/Chat/ChatService.vb`, `src/KBot.App/AcasaPage.vb`, `AcasaPage.Designer.vb`, `AvacontMain.vb`, `Program.vb`, `src/KBot.App/HelpContent/contabil/acasa.md`.

## Verificări
- `dotnet build src/KBot.App` (spre un folder de lucru, fiindcă KBot.App.exe rula): **reușit, 0 avertismente**.
- Python: doar `py_compile` pe cele trei fișiere (curat). Teste nerulate (regula operatorului).
- `Check-Help.ps1`: `acasa.md` fără erori; rămân 22 de erori vechi în alte fișiere (tutorials, tours, `efactura/index.md`).

## Neverificat / amânat
- **Nimic văzut pe ecran**: aspectul balonului, coada, poziția față de robot, mișcarea, scalarea la 125/150% (balonul își scalează singur copiii de la 96 dpi, pagina e proiectată la 144), tema închisă. De privit la prima rulare.
- **Contul**: se presupune că `username`-ul sesiunii K-BOT este același e-mail cu cel din portalul web (identitatea chatului e cheia `CONT`). Dacă diferă, aceeași persoană ar avea două identități în chat.
- **Serverul nu e urcat** (îl urcă operatorul de mână): până atunci chatul din K-BOT.app primește 401/eroare și tace. Tabelele `CHAT_*` din `PYTHON/sql/chat` trebuie să existe deja (chatul web le folosește).
- Nu s-au făcut aici (răspuns, filtru, clic în afară -> CHAT-09): semnal pe celelalte pagini (operatorul a ales «doar Acasă»), emoji/GIF. Un GIF propriu se poate pune în loc de imagine dacă îl dă operatorul.

## SLICE-CHAT-09 — răspuns la mesaj, filtrul V/A/K și închiderea balonului la clic în afară <a id="slice-chat-09"></a>

Data: 2026-10-21. Continuă [SLICE-CHAT-08](SLICE-CHAT-08-robot-si-balon.md). Cerere operator: cele trei lucruri lăsate nefăcute acolo, plus: **un clic în afara ferestrei de chat o închide singură**. Operatorul nu poate încă încerca cu mesaje de la alții (nimeni altcineva nu folosește sistemul), deci nimic din ce urmează nu a fost văzut cu mesaje reale.

### Ce s-a schimbat și de ce
- **Răspuns la mesaj.** Sub fiecare mesaj apare cuvântul subliniat «Răspunde»; un clic pe el pune deasupra casetei de scris o bandă «Răspuns pentru <autor>: <text>» cu butonul × (renunță). Mesajul trimis poartă `reply_to`; în listă, răspunsul arată rândul «Răspuns pentru …» (îl scria deja randarea din CHAT-08). Clicul e găsit după poziție în `RichTextBox` (se verifică să fie pe cuvânt, nu doar pe cel mai apropiat caracter); cursorul devine mână deasupra lui. Banda se golește după trimitere. Cheia de retrimitere ține cont și de mesajul răspuns.
- **Filtrul V/A/K.** Rând «Afișează: VERCON ADECHIT K-BOT» sub titlu. Se salvează pe server (`POST /api/chat/preferences`, aceeași alegere ca pe paginile web — o singură preferință pentru toate), apoi lista se încarcă din nou pentru aplicațiile alese. Nu poate rămâne nimic nebifat (cea debifată se pune la loc, cu o remarcă). Un refuz al serverului readuce bifele serverului.
- **Clic în afară.** `AcasaPage` ascultă (`IMessageFilter`) apăsările de mouse din toată aplicația cât balonul e deschis; una în afara balonului și a robotului îl închide, iar apăsarea merge mai departe la ce era dedesubt (butonul/lista nu pierd clicul). Clicul pe robot rămâne al robotului (deschide/închide). Un clic în altă aplicație Windows nu închide balonul.
- API: `IChatApi.SaveChatPreferencesAsync` are acum și `applications`; `ChatService.SendAsync` primește `replyTo`, nou `SaveApplicationsAsync`. Serverul nu s-a schimbat (rutele știau deja `applications` și `reply_to`).
- Ajutor: secțiunea «Discuții» din `acasa.md` descrie răspunsul, filtrul și închiderea; etichete `CHAT-08, CHAT-09`.
- Evidența: felia CHAT are acum fișier de stare (`state/KBOT_STATUS_CHAT.md`), rând în index și acceptare în `Check-Help.ps1`.

### Fișiere atinse
`src/KBot.Controls/Chat/KBotChatBubble.vb`, `KBotChatBubble.Designer.vb`, `src/KBot.Api/IChatApi.vb`, `ApiClient.Chat.vb`, `src/KBot.App/Chat/ChatService.vb`, `src/KBot.App/AcasaPage.vb`, `HelpContent/contabil/acasa.md`, `tools/HelpCheck/Check-Help.ps1`, `docs/worklog/KBOT_STATUS.md`, `state/KBOT_STATUS_CHAT.md`.

### Verificări
- `dotnet build src/KBot.App` (spre folder de lucru; aplicația rula): **reușit, 0 avertismente**.
- Teste nerulate (regula operatorului). Nimic văzut pe ecran.

### Neverificat / amânat
- Poziția «Răspunde» pe ecran (găsirea clicului după poziție în `RichTextBox` la 125/150% și cu liste lungi); lățimea rândului de filtre la 125/150% (trei casete + eticheta într-un balon de ~360 px logici).
- Clicul în afară: nu s-a încercat cu ferestre modale / meniuri deschise din pagină.
- Un mesaj de la altcineva nu s-a putut vedea: marcarea «citit» și numărul pe robot au fost verificate doar în cod.
- Nu s-a făcut: semnal pe celelalte pagini (operatorul a ales «doar Acasă»); GIF propriu.
