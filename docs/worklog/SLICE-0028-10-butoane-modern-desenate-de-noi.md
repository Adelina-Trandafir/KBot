# SLICE-0028-10 — Schema «Modern»: butoanele Windows obișnuite se desenează singure, fără regiune (08.11.2026)

Ultimul slice care a atins direct `ModernRenderer`: 0028-07 (butonul crește cât să încapă umplutura și textul). Cererea operatorului: butoanele din ferestre («Pornește» / «Ieșire») se vedeau urât pe tema Modern.

## What changed and why
`ModernRenderer` rotunjea butoanele cu `Button.Region`: o tăietură fără netezire, în trepte de pixeli, care ștergea și colțurile chenarului dreptunghiular desenat de Windows — laturile aveau chenar, colțurile nu aveau arc.
Acum un buton cu text se desenează în handlerul `Paint` (`OnButtonPaint`), care rulează după desenul standard: culoarea din spate, fața rotunjită cu netezire și chenarul pe jumătăți de pixel (vezi 00EF-61), apoi textul (aliniere din `TextAlign`, `UseMnemonic`). Culorile rămân ale butonului (`BackColor`, `FlatAppearance.MouseOver/MouseDown`, `ForeColor`); dezactivat = fundal și text din paletă; chenar accent cu focus. Hover/apăsat se deduc din poziția mouse-ului.
Neschimbat: butoanele cu imagine păstrează calea veche cu regiune; la raza 0 (Classic) desenul standard rămâne.

## Files touched
`src/KBot.Theming/ModernRenderer.vb` (versiunea proiectului: vezi 00EF-61).

## Test results
Build: 0 erori. Niciun test scris sau rulat. Captură reală de ecran la 150% DPI cu două butoane obișnuite: arc cu netezire, hover, buton dezactivat. **Văzut pe ecran de operator: corect.**

## Unverified / deferred
Neîncercate: apăsarea cu mouse-ul, textul aliniat în alt mod decât centrat, butoane cu `Padding` mare, buton implicit (`AcceptButton`) — acum doar `Focused` aprinde chenarul accent.
