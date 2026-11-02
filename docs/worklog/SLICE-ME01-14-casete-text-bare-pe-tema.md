# SLICE-ME01-14 - KBotLogBox / KBotSqlBox: barele de derulare urmeaza tema (02.11.2026)

Cererea operatorului (cu captura din «Publicare»): casuta de log avea barele native, deschise la culoare pe Dark. Cele doua controale au slice-ul ME01 (`KBotLogBox`: ME01; `KBotSqlBox`: ME01-05); ultimul sub-numar era ME01-13, deci ME01-14. Continuarea lui 0051-04 (acelasi tratament pentru editorul de text imbogatit).

## What it is
- **`KBotRichScrollBox`** (nou, `RichText/KBotRichScrollBox.vb`, `Public`, derivat din `RichTextBox`): baza ambelor controale. Barele native sunt oprite; doua `KBotScrollBar` (verticala si orizontala) sunt controale-copil ale casetei, in cate o fasie pe marginea dreapta si jos; dreptunghiul de formatare (`EM_SETRECT`) e scurtat cu fasiile, ca textul sa nu intre sub ele, si reasezat dupa fiecare redimensionare. Fara bara vizibila ramane dreptunghiul implicit.
- Pozitia se citeste / scrie in pixeli (`EM_GETSCROLLPOS` / `EM_SETSCROLLPOS`). **Intinderea se masoara**: vederea e dusa in coltul de departe, se citeste unde s-a oprit si se pune la loc (redesenarea oprita, o singura reimprospatare), la 60 ms dupa ultima schimbare de text / marime / font, nu la fiecare derulare. Ramane exacta cu fonturi mixte, linii infasurate si taburi.
- `ScrollBars` si `WordWrap` isi pastreaza numele si sensul (`Shadows`, cu aceleasi `DefaultValue` ca la `RichTextBox`, deci designerele nu serializeaza altceva); bara orizontala apare doar fara `WordWrap`. Barele native nu se mai folosesc niciodata.
- `AutoHideScrollBars` (implicit **True**): o bara apare doar cand textul nu incape; modurile `Forced*` (cum le au casetele din KBotAdmin) se poarta atunci ca cele simple; oprit, `Forced*` tine bara mereu. O pozitie de masurare neprinsa de caseta (caseta goala) inseamna «nimic de derulat», nu o intindere uriasa.
- Cursorul peste bare e sageata normala: `KBotScrollBar.DefaultCursor = Arrow` (altfel mostenea I-beam-ul casetei-parinte; se aplica tuturor gazdelor barei).
- Rotita e preluata de caseta (Shift = lateral); Ctrl + rotita nu mai mareste textul.
- `KBotLogBox` si `KBotSqlBox` deriva din `KBotRichScrollBox` si cheama `ApplyBarTheme(scheme)` din `ApplyTheme`, deci barele iau culorile schemei.
- KBot.Controls 1.82.0.0. `KBotLogBox.md` actualizat. Ajutor: nicio sectiune nu descrie barele acestor casete, nimic de etichetat.

**Trap (02.11.2026, seen on screen: bars shown on a box with 4 short lines):** the far scroll position of a RichTextBox is NOT "document minus window": the view may run on until the last line (the end of the longest line) reaches the top (left) edge, so far = about the size of the document itself. The first version showed a bar whenever far > 0 (always). Now: content = far + one line (one character), a bar shows only if content > window, and the wheel stops at the bar's last position. The size is an inference from two thumb ratios read off a screenshot (0.63 and 0.55), not a measurement.

**Trap (02.11.2026, seen on screen: the horizontal bar ended up in the middle of the box after a scroll):** a RichTextBox scrolls its window with ScrollWindow, which MOVES child windows along; bars that are children of the box drift. `KBotRichScrollBox` keeps the strips in `_vSlot` / `_hSlot` and puts the bars back after EVERY message (`KeepBarsInPlace` at the end of `WndProc`, before any repaint).

Same wheel trap as in SLICE-00EF-54 (signed high word of `wParam`; fixed in `KBotRichScrollBox`).

## Files touched
`KBot.Controls/RichText/KBotRichScrollBox.vb` (nou), `KBotLogBox.vb`, `KBotSqlBox.vb`, `KBotLogBox.md`; `KBot.Controls.vbproj`; `state/KBOT_STATUS_ME01.md`, `KBOT_STATUS.md`.

## Test results
`dotnet build` pe `KBot.Controls.vbproj` si pe `KBotAdmin.vbproj` (cele patru pagini cu `KBotLogBox` / `KBotSqlBox`): 0 avertismente, 0 erori. Nimic rulat, nici un test.

## Unverified / deferred
- **Neincercat pe ecran, de verificat de operator**: ca `EM_SETSCROLLPOS` deruleaza o caseta fara bare native (dedus din modul in care e folosit deja in `KBotSqlBox.Highlight` si de documentatia mesajului, nemasurat fara bare); ca fasiile si coltul arata bine; ca masurarea nu face ecranul sa palpaie la loguri lungi; derularea la capat dupa `SetLines` (`ScrollToCaret`), selectia cu tragere, rotita si Shift + rotita.
- Copilul unui `RichTextBox` este un truc: functioneaza fiindca `Control` pune `WS_CLIPCHILDREN`, deci caseta nu deseneaza peste bare; daca pe ecran apar urme de text sub bare, de aici se porneste.
- Designerul nu mai arata bare in caseta (barele apar doar la rulare).
- Alte `RichTextBox`-uri simple din ferestre (consola FOREXE, istoric, ActiveX log) nu sunt controale K-BOT si raman cu bare native.
