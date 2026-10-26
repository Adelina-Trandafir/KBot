# SLICE-0123-07 - Arbore: switch «click la fiecare miscare cu tastele» (26.10.2026)

Operator: la defilarea cu butoanele in cadrul arborelui sa existe un switch (daca nu exista deja) care, la fiecare miscare, sa execute si click-ul pe nodul respectiv.

## What changed and why
- **Nu exista** un asemenea switch: `OnKeyDown` muta doar selectia (sageti, PageUp/PageDown, Home/End), iar `NodeMouseUp` (click-ul pe care il asculta gazdele) se ridica doar la Enter sau cu mouse-ul.
- **`KBot.Controls/Tree/AdvancedTreeControl.Keyboard.vb`**: proprietate noua `ClickOnKeyNavigation As Boolean` (Category «K-BOT», implicit **False** = comportamentul de pana acum, deci nicio schimbare pentru arborii existenti). Pe True, de fiecare data cand o tasta de navigare schimba efectiv nodul selectat, se ridica `NodeMouseUp(nodul ajuns, Left, 1 click)` - acelasi eveniment ca la mouse si la Enter - si `pOldSelectedItem` se aliniaza ca la Enter. Stanga/Dreapta care doar pliaza/desfac un nod (selectia ramane pe loc) nu ridica nimic.
- `KBot.Controls` FileVersion 1.69.0.0 -> 1.70.0.0.
- Nu se vede nimic nou pe ecran cat switch-ul e oprit si nicio fereastra nu l-a aprins inca => fara ajutor de actualizat.

## Files touched
`src/KBot.Controls/Tree/AdvancedTreeControl.Keyboard.vb`, `src/KBot.Controls/KBot.Controls.vbproj`, `docs/worklog/state/KBOT_STATUS_0120-0129.md`, `docs/worklog/KBOT_STATUS.md`.

## Test results
`dotnet build src\KBot.App\KBot.App.vbproj -c Debug`: 0 avertismente, 0 erori. Nimic rulat.

## Unverified / deferred
- Nicio fereastra nu foloseste inca switch-ul: se aprinde in Designer / cod (`tree.ClickOnKeyNavigation = True`) pe arborele dorit; operatorul nu a spus care.
- Cat se tine o tasta apasata, repetarea ridica cate un click la fiecare pas (cum a cerut «la fiecare miscare»); daca gazda deschide ceva greu (document), poate fi nevoie de o intarziere. Nevazut pe ecran.
- Cu `MultiSelect` si Shift, click-ul se ridica pe nodul ajuns, fara a modifica grupul.
