# SLICE-0066-03 — KBotTableLayoutPanel: fundalul implicit după temă, Window pe Classic/Dark și Control pe Modern (08.11.2026)

Ultimul slice care a atins direct controlul: 0066-02 (autoscalare DPI). Cerere: operatorul a pus `BackColor = Window` pe `tlySus`, `tlyEvid` și `tlyPlata` și la schimbarea temei culoarea nu s-a mai schimbat.

## What changed and why
`BackColor` setat de mână fixează culoarea («pinned»), deci tema n-o mai atinge — așa e contractul controalelor K-BOT. Lipsea valoarea implicită potrivită. Fundalul cât timp NU e setat de operator (`KBotTableLayoutPanel.ApplyTheme`):
- Classic (`Style.UseSystemColors`): `SystemColors.Window`;
- Dark (`IsDark`): `SurfaceAlt` (#1C1C1C), locul lui «Window» în Dark;
- Modern și orice altă schemă (Colorful, proprii): `Surface` (#FAFAFA), adică Control;
- panoul cu `Tag = "Card"` rămâne pe `SurfaceAlt`.

În `PrimiteContarePage.Designer.vb` s-au șters cele trei linii `BackColor = Drawing.SystemColors.Window` ale `tlySus`, `tlyEvid`, `tlyPlata`; altfel rămâneau fixate.

## Files touched
- `src/KBot.Controls/Table/KBotTableLayoutPanel.vb` (`ApplyTheme`) și `KBotTableLayoutPanel.md` (secțiunea «BackColor»); versiunea proiectului: cea din 00EF-61.
- `src/KBot.EFactura/Views/Primite/PrimiteContarePage.Designer.vb` (3 linii șterse).

## Test results
Build `KBot.EFactura` (cu `KBot.Controls`): 0 erori, 0 avertismente. **Verificat pe ecran și testat de operator: corect** la schimbarea temelor.

## Unverified / deferred
- `pnlJos` (tot `KBotTableLayoutPanel`, `PrimiteContarePage.Designer.vb`) are încă `BackColor = Window` fixat, deci nu urmează tema. Nu e `tly*`, nu s-a atins.
- `tlyHeader` (`KbotForm.Designer.vb`) are `BackColor = Transparent`, deliberat; neatins.
- Help: nimic de scris (operator).
