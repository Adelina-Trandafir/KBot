# SLICE-0027-04 - AdvancedTreeControl: bifele si expanderele urmeaza tema (02.11.2026)

Cererea operatorului: casutele si expanderele [+]/[-] ale arborelui nu urmau tema (albe pe Dark). Sub-felie 0027-04 (0027-03 era ultimul numar). **Inchisa: operatorul a confirmat ca arata bine, rulat si testat (02.11.2026).**

## What it is now
- `DrawCheckbox` / `DrawExpander` (`AdvancedTreeControl.Painting.vb`) folosesc sapte culori „auto" din `AdvancedTreeControl.Theming.vb`, scrise de `ApplyTheme` din paleta: `_autoCheckBack` / `_autoExpanderBack` = `InputBackColor`, `_autoCheckBorder` = `InputBorderColor`, `_autoCheckAccent` = `AccentColor`, `_autoCheckMark` = `AccentTextColor`, `_autoCheckLocked` = `DisabledTextColor`, `_autoExpanderSign` = `TextColor`. Inainte de prima tema raman valorile vechi (alb, gri, `DodgerBlue`). Chenarul expanderului ramane `LineColor`. Fara proprietati publice noi.
- Casuta / radio bifat: fond accent, semn in culoarea de text de pe accent; casuta goala: fond si bordura ca un camp de text; `CheckLocked`: text dezactivat. In `Tree/` nu mai ramane `Brushes.White` / `Pens.Black` in desen.
- KBot.Controls 1.79.0.0. Ajutor: nicio sectiune nu descrie culorile bifelor / expanderelor, nimic de etichetat.

## Trap
Culorile fixe din desen (`Brushes.White`, `Color.DodgerBlue`) trec neobservate pana pe o schema intunecata; in desen, culorile vin din paleta prin campuri `_auto*` scrise in `ApplyTheme`.

## Files touched
`KBot.Controls/Tree/AdvancedTreeControl.Painting.vb`, `AdvancedTreeControl.Theming.vb`; `KBot.Controls.vbproj`; `state/KBOT_STATUS_0020-0029.md`.

## Test results
Build `KBot.Controls`: 0 avertismente, 0 erori. Rulat si verificat pe ecran de operator.

## Unverified / deferred
Alte controale cu casute albe fixe nu au fost cautate in afara folderului `Tree/`.
