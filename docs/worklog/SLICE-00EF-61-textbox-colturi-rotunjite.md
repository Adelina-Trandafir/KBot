# SLICE-00EF-61 — KBotTextBox: colțuri rotunjite curate pe tema Modern (08.11.2026)

Ultimul slice care a atins direct controlul: 00EF-54 (rotița mouse-ului). Cererea operatorului: colțurile câmpurilor de text se vedeau urât pe tema Modern. Include ajutoarele noi din `ThemeShapes`, folosite și de 00EF-62…64 și 0028-10.

## What changed and why
Trei cauze, toate în desen:
1. **Colțuri albe.** `BackColor` e opac (alb); pasul de fundal al WinForms umple tot dreptunghiul cu el, iar arcul se desena peste. Pixelii din afara arcului rămâneau albi pe pagina gri. Acum `OnPaint` începe cu `g.Clear(ThemeShapes.BackdropColor(Me))`.
2. **Pătrat alb în interior.** Fără `Multiline = False` (implicit `True`), caseta interioară (`TextBox` real, dreptunghi opac) umple tot interiorul, cu colțurile drepte peste arc. Noul `ClipInnerToFrame` îi pune o regiune rotunjită, cu 1 px sub marginea interioară a chenarului (marginea tăiată nu se vede: dedesubt cadrul e tot alb). Se reface și la `ApplyTheme` (raza se schimbă cu tema).
3. **Arc subțire și palid.** Calea trecea pe coordonate întregi, iar GDI+ pe modul implicit așază arcul altfel decât laturile. Acum cadrul e pe jumătăți de pixel (`PixelOffsetMode.Half` + `FrameBounds` / `RoundedRectF`).

Plus `OnParentBackColorChanged` → `Invalidate()`: colțul urmează fundalul părintelui când acesta se schimbă.

`ThemeShapes` (public, nou): `BackdropColor(control)` = primul părinte cu `BackColor` opac, altfel `SurfaceColor` a temei; `RoundedRectF`; `FrameBounds(size, grosime)`.

## Files touched
`src/KBot.Controls/TextField/KBotTextBox.vb`, `src/KBot.Theming/ThemeShapes.vb`, `KBotTextBox.md`, `KBot.Controls.vbproj` (1.85.0.5 → 1.85.1.0), `KBot.Theming.vbproj` (1.14.0.1 → 1.14.1.0).

## Test results
`dotnet build` KBot.Controls: 0 erori, 0 avertismente. Niciun test scris sau rulat (regula operatorului). Verificat cu un program de probă separat (în afara repo-ului): captură reală de ecran la 150% DPI, valori de pixeli în colț. **Văzut pe ecran de operator: corect.**

## Unverified / deferred
- Capcană: `DrawToBitmap` ignoră regiunea copilului, iar un program neconștient de DPI nu arată problema la 150% — probele de colț se fac cu `CopyFromScreen` și `HighDpiMode.PerMonitorV2`.
- Neîncercat: comutarea efectivă Classic/Dark/Modern în aplicație; caseta cu bare de derulare vizibile; părinte cu gradient (colțul ia culoarea plată a celui mai apropiat părinte opac).
- Ajutorul nu s-a schimbat (nu e schimbare de flux); capturile existente cu colțuri vechi nu au fost refăcute.
