# SLICE-ME01-14 - KBotLogBox / KBotSqlBox: barele de derulare urmeaza tema (02-03.11.2026)

Cererea operatorului: casetele de log si SQL (KBotAdmin) aveau barele native, deschise la culoare pe Dark. `KBotLogBox` are slice-ul ME01, `KBotSqlBox` ME01-05; ultimul sub-numar era ME01-13. **Inchisa: operatorul a verificat pe ecran, in KBotAdmin si in vizualizatorul XML care foloseste aceeasi baza, ca merge (03.11.2026).**

## What it is now
- **`KBotRichScrollBox`** (`RichText/KBotRichScrollBox.vb`, `Public`, derivat din `RichTextBox`): baza lui `KBotLogBox`, `KBotSqlBox` si a casetei din `XmlViz` (00EF-57). Barele native sunt oprite; doua `KBotScrollBar` (verticala, orizontala) sunt controale-copil ale casetei, in cate o fasie pe marginea dreapta si jos. Dreptunghiul de formatare (`EM_SETRECT`) e scurtat cu fasiile si reasezat dupa fiecare redimensionare; fara bara vizibila ramane cel implicit.
- Pozitia se citeste / scrie in pixeli (`EM_GETSCROLLPOS` / `EM_SETSCROLLPOS`). **Intinderea se masoara**: vederea e dusa in coltul de departe, se citeste unde s-a oprit si se pune la loc (redesenare oprita, o reimprospatare), la 60 ms dupa ultima schimbare de text / marime / font. Marimea documentului = pozitia de departe + o linie (pe orizontala + un caracter).
- `ScrollBars` si `WordWrap` isi pastreaza numele si sensul (`Shadows`, aceleasi `DefaultValue` ca la `RichTextBox`); bara orizontala apare doar fara `WordWrap`.
- `AutoHideScrollBars` (implicit True): o bara apare doar cand documentul depaseste fereastra, pe fiecare axa; modurile `Forced*` se poarta atunci ca cele simple; oprit, `Forced*` tine bara mereu. Caseta goala = nimic de derulat.
- Cursorul peste bare e sageata (`KBotScrollBar.DefaultCursor = Arrow`, pentru toate gazdele barei). Rotita e preluata de caseta (Shift = lateral; se opreste unde se opreste bara); Ctrl + rotita nu mai mareste textul.
- `ApplyBarTheme(scheme)` e `Public`: il cheama `ApplyTheme` din `KBotLogBox` / `KBotSqlBox` si din `XmlViz`.
- KBot.Controls 1.84.0.0 (impreuna cu 00EF-57). `KBotLogBox.md` actualizat. Ajutor: nicio sectiune nu descrie barele, nimic de etichetat.

## Traps
- **Pozitia «de departe» nu e «document minus fereastra».** O caseta RichTextBox lasa vederea sa alerge pana cand ultima linie (capatul celei mai lungi linii) ajunge la marginea de sus (stanga): far = cam cat documentul. O conditie «bara daca far > 0» arata bara mereu, chiar la 4 linii scurte.
- **`ScrollWindow` muta ferestrele-copil.** Un RichTextBox isi deruleaza fereastra cu `ScrollWindow`, care muta si barele-copil (orizontala ajungea in mijlocul casetei). `KeepBarsInPlace` le pune inapoi dupa FIECARE mesaj, la sfarsitul `WndProc`, inainte de orice redesenare.
- **`CShort` pe delta rotitei arunca la rotita in jos** (cuvantul de sus din `wParam` e 65416, in afara Int16: «Arithmetic operation resulted in an overflow»). Se extinde semnul cu `((v And &HFFFF) Xor &H8000) - &H8000`.
- **Un control-copil mosteneste cursorul parintelui** (I-beam peste bare): se pune `DefaultCursor` pe bara.
- Dimensionarea intinderii e dedusa din doua proportii citite pe o captura, nu masurata; a mers pe ecran la operator.

## Files touched
`KBot.Controls/RichText/KBotRichScrollBox.vb` (nou), `KBotLogBox.vb`, `KBotSqlBox.vb`, `KBotLogBox.md`; `KBot.Controls/Scroll/KBotScrollBar.vb` (`DefaultCursor`); `KBot.Controls.vbproj`; `state/KBOT_STATUS_ME01.md`, `KBOT_STATUS.md`.

## Test results
Build `KBot.Controls` si `KBotAdmin`: 0 avertismente, 0 erori. Rulat si verificat de operator.

## Unverified / deferred
- Documente foarte lungi (zeci de mii de linii): masurarea face o reimprospatare la fiecare schimbare de text; nemasurat ca timp.
- Rotita peste o caseta nu mai ajunge la parinte (mesajul e inghitit).
- Designerul nu arata bare in aceste casete (apar doar la rulare).
- Alte `RichTextBox`-uri simple din ferestre (consola FOREXE, istoric, log ActiveX) raman cu bare native.
