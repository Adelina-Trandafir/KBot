# SLICE-00EF-54 - KBotTextBox: rotita mouse-ului deruleaza si deasupra textului (02.11.2026)

Cererea operatorului: in `KBotTextBox` rotita nu derula. Ultima felie care a atins `KBotTextBox` in cod: 00EF-34 / 00EF-37; primul numar liber din 00EF: 00EF-54 (propus si mutat la 01EF in SLICE-01EF-01, deci neluat). **Inchisa: operatorul a confirmat ca merge, rulat si testat (02.11.2026).**

## What it is now
- `EditIntern.WndProc` prinde `WM_MOUSEWHEEL`, ridica evenimentul `WheelTurned` (delta din `WParam`) si nu mai trimite mesajul mai departe; cadrul il trateaza in `OnInnerWheel` -> `ScrolleazaCuRotita(delta)`, aceeasi cale ca `OnMouseWheel` (linii pe pas din `SystemInformation.MouseWheelScrollLines`, `_vBar.Value`, `EM_LINESCROLL`). Fara bara verticala vizibila nu se intampla nimic.
- KBot.Controls 1.78.0.0. Ajutor: nimic de etichetat. `KBotTextBox.md` actualizat.

## Trap
Caseta interna are `ScrollBars.None` (barele sunt `KBotScrollBar` ale cadrului), iar un EDIT nativ fara bare ignora rotita; `OnMouseWheel` al cadrului primea doar rotita de pe margine. Orice control care ascunde barele native si le deseneaza el trebuie sa rutezeze singur rotita. Controalele cu bare native (`KBotLogBox`, `KBotSqlBox`, RTB-ul din `KBotRichTextEditor`) nu au problema; `KBotComboBox` / `KBotDateEditBox` sunt pe o linie.

**Trap (02.11.2026, found on a wheel-down over an empty box: «Arithmetic operation resulted in an overflow»):** the wheel delta is the high word of `wParam`; `CShort(x And &HFFFF)` THROWS for a negative delta (wheel down gives 65416, out of Int16). Sign-extend with `((v And &HFFFF) Xor &H8000) - &H8000` instead. Same fix for `EM_POSFROMCHAR` X in `KBotTextBox.DecalajOrizontal` (negative when scrolled sideways). The first version of 00EF-54 had the bug and was reported as working.

## Files touched
`KBot.Controls/TextField/KBotTextBox.vb`, `KBotTextBox.md`; `KBot.Controls.vbproj`; `state/KBOT_STATUS_0000-0009.md` (rand 00EF-54); `KBOT_STATUS.md`.

## Test results
Build `KBot.Controls`: 0 avertismente, 0 erori. Rulat si verificat de operator.

## Unverified / deferred
Rotita peste o caseta fara bara vizibila nu mai ajunge la parinte (mesajul e inghitit in caseta interna); daca o fereastra parinte trebuie sa deruleze atunci, se ridica separat.
