# SLICE-0051-04 - KBotRichTextEditor: bara de derulare verticala urmeaza tema (02.11.2026)

Cererea operatorului: bara de derulare a editorului de text imbogatit (`KBotRichTextEditor`, RichTextBox-ul cu bara de unelte) sa urmeze tema. Editorul are slice-ul 0051 (portul formularului RTB); ultimul sub-numar era 0051-03. **Inchisa: operatorul a verificat pe ecran ca merge (03.11.2026).**

## What it is now
- Suprafata de scris e `KBotRichEdit` (`RichText/KBotRichEdit.vb`, `Friend`, derivat din `RichTextBox`), cu bara nativa oprita (`ScrollBars.None`). Editorul tine o `KBotScrollBar` proprie (`vBar`, in `KBotRichTextEditor.Designer.vb`) langa suprafata, tematizata ca restul barelor K-BOT; `ApplyTheme(scheme)` ii da schema direct. Nu foloseste `KBotRichScrollBox` (ME01-14): bara sta langa suprafata, nu in ea, deci nu are problema barelor-copil.
- `KBotRichEdit` anunta gazda: `ViewChanged` (taste, click, tragere cu butonul apasat, lipire, redimensionare, `EM_SCROLLCARET` / `EM_LINESCROLL` / `EM_SETSCROLLPOS`) si `WheelTurned` (`WM_MOUSEWHEEL` nu mai e trimis mai departe: deruleaza gazda). Pozitia se citeste / scrie in pixeli (`ScrollY`, `EM_GETSCROLLPOS` / `EM_SETSCROLLPOS`).
- `KBotRichTextEditor.Scroll.vb`: `SyncVerticalBar` (intinderea = inaltimea documentului din pozitia ultimului caracter + pozitia de derulare, fereastra = zona de formatare minus padding; bara apare doar daca are ce derula, sau mereu la `ForcedVertical` / `ForcedBoth`), `VBar_Scroll`, `Rtb_WheelTurned` (liniile din setarea Windows), sincronizare la text schimbat. Cand bara apare / dispare, suprafata se reasaza si se citeste a doua oara (se stabilizeaza: o suprafata mai ingusta e doar mai inalta). `RebuildLayout` scade latimea barei din latimea suprafetei.
- Proprietatea publica `ScrollBars` pastreaza semnatura: `Vertical` / `ForcedVertical` / `Both` / `ForcedBoth` = bara noastra; o orizontala ceruta ramane **nativa** (nimeni nu o foloseste: editorul are `WordWrap`).
- KBot.Controls 1.80.0.0. `KBotRichTextEditor.md` actualizat. Ajutor: nicio sectiune nu descrie bara editorului, nimic de etichetat.

## Traps
- Un EDIT / RichEdit fara bare native ignora rotita si nu emite `WM_VSCROLL`: gazda trebuie sa prinda rotita si mesajele care pot muta vederea.
- `CShort` pe delta rotitei arunca la rotita in jos: se extinde semnul cu `((v And &HFFFF) Xor &H8000) - &H8000` (vezi SLICE-00EF-54, SLICE-ME01-14).

## Files touched
`KBot.Controls/RichText/KBotRichEdit.vb` (nou), `KBotRichTextEditor.Scroll.vb` (nou), `KBotRichTextEditor.vb` (`RebuildLayout`, `Rtb_HandleCreated`), `.Properties.vb` (`ScrollBars`), `.Theming.vb` (`ApplyTheme`), `.Designer.vb` (`rtb` ca `KBotRichEdit`, `vBar`), `KBotRichTextEditor.md`; `KBot.Controls.vbproj`; `state/KBOT_STATUS_0050-0059.md`, `KBOT_STATUS.md`.

## Test results
Build `KBot.Controls`: 0 avertismente, 0 erori. Rulat si verificat pe ecran de operator.

## Unverified / deferred
- Rotita peste editor nu mai ajunge la parinte, iar Ctrl + rotita nu mai mareste textul (mesajul e inghitit in `KBotRichEdit`).
- Marimea cursorului la documente cu fonturi mixte: inaltimea ultimei linii se ia din fontul casetei (aproximare).
- `KBotLogBox` / `KBotSqlBox`: vezi [ME01-14](SLICE-ME01-14-casete-text-bare-pe-tema.md).
