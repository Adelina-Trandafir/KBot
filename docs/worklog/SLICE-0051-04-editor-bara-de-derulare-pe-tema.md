# SLICE-0051-04 - KBotRichTextEditor: bara de derulare verticala urmeaza tema (02.11.2026)

Cererea operatorului: «nici in custom rtb nu sunt corecte scroll bar-urile, nu urmaresc tema». Editorul are slice-ul 0051 (portul formularului RTB); ultimul sub-numar era 0051-03, deci 0051-04. Citit ca `KBotRichTextEditor` (RichTextBox-ul cu bara de unelte); `KBotLogBox` si `KBotSqlBox` au aceeasi problema si sunt tratate in ME01-14.

## What it is
- Suprafata de scris e acum `KBotRichEdit` (nou, `RichText/KBotRichEdit.vb`, `Friend`, derivat din `RichTextBox`), cu bara nativa verticala oprita (`ScrollBars.None`). Editorul tine o `KBotScrollBar` proprie (`vBar`, declarata in `KBotRichTextEditor.Designer.vb`) langa suprafata, deci tematizata ca restul barelor K-BOT; `ApplyTheme(scheme)` o ajusteaza direct.
- `KBotRichEdit` raporteaza gazdei: `ViewChanged` (taste, click, tragere cu butonul apasat, lipire, redimensionare, `EM_SCROLLCARET` / `EM_LINESCROLL` / `EM_SETSCROLLPOS`) si `WheelTurned` (mesajul `WM_MOUSEWHEEL` nu mai e trimis mai departe: deruleaza gazda). Pozitia de derulare se citeste / scrie in PIXELI (`ScrollY`, `EM_GETSCROLLPOS` / `EM_SETSCROLLPOS`), deci ramane corecta cu fonturi de marimi diferite.
- Logica in `KBotRichTextEditor.Scroll.vb`: `SyncVerticalBar` (interval = inaltimea documentului, fereastra = zona de formatare minus padding, pozitia = `ScrollY`; bara apare doar daca are ce derula, sau mereu la `ForcedVertical` / `ForcedBoth`), `VBar_Scroll`, `Rtb_WheelTurned` (liniile din setarea Windows), sincronizare la text schimbat. Cand bara apare / dispare, suprafata se reasaza si se citeste a doua oara (se stabilizeaza: o suprafata mai ingusta e doar mai inalta). `RebuildLayout` ia latimea barei din latimea suprafetei.
- Proprietatea publica `ScrollBars` pastreaza semnatura: `Vertical` / `ForcedVertical` / `Both` / `ForcedBoth` = bara noastra; orizontala ceruta ramane **nativa** (nimeni nu o foloseste: editorul are `WordWrap`).
- KBot.Controls 1.80.0.0. `KBotRichTextEditor.md` actualizat. Ajutor: nicio sectiune nu descrie bara editorului, nimic de etichetat.

Same wheel trap as in SLICE-00EF-54 (signed high word of `wParam`; fixed in `KBotRichEdit`).

## Files touched
`KBot.Controls/RichText/KBotRichEdit.vb` (nou), `KBotRichTextEditor.Scroll.vb` (nou), `KBotRichTextEditor.vb` (`RebuildLayout`, `Rtb_HandleCreated`), `.Properties.vb` (`ScrollBars`), `.Theming.vb` (`ApplyTheme`), `.Designer.vb` (`rtb` ca `KBotRichEdit`, `vBar`), `KBotRichTextEditor.md`; `KBot.Controls.vbproj`; `state/KBOT_STATUS_0050-0059.md`, `KBOT_STATUS.md`.

## Test results
`dotnet build src\KBot.Controls\KBot.Controls.vbproj`: 0 avertismente, 0 erori. Nimic rulat, nici un test.

## Unverified / deferred
- **Neincercat pe ecran, de verificat de operator**: ca `EM_SETSCROLLPOS` deruleaza o suprafata fara bara nativa (dedus din documentatia mesajului, nemasurat); marimea cursorului la documente cu fonturi mixte (inaltimea ultimei linii se ia din fontul casetei); rotita; ca bara apare / dispare fara tremur; editorul restrans («collapsed») si cu antet / subsol ascunse.
- Rotita peste editor nu mai ajunge la parinte, iar Ctrl+rotita nu mai mareste textul (mesajul e inghitit in `KBotRichEdit`).
- `KBotLogBox` / `KBotSqlBox` (RichTextBox-uri cu bare native): vezi [ME01-14](SLICE-ME01-14-casete-text-bare-pe-tema.md).
