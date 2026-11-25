# Plan: rândul 2 din `tlyAntet` (FacturiPlatiPage) nu rămâne la înălțimea scrisă în designer

Problema: în `FacturiPlatiPage`, rândul 2 din `tlyAntet` e pus la 40, dar după OK se vede la ~60.
Ținta: **ce scrie operatoarea în designer, pe ecranul ei, e ce vede.** Fără calcule de DPI.

Planul e de investigație. Nu se repară nimic până nu există cifre măsurate și până nu alege
Adelina remedierea.

## 0. Starea de acum (fapte citite din fișiere, 16.11.2026)

- `FacturiPlatiPage.Designer.vb`: `AutoScaleMode = Dpi`, `AutoScaleDimensions = (144, 144)`, deci
  pagina e salvată pe un ecran de 150%.
- `tlyAntet`: `AutoFitToTheme = False`, `ScaleAbsoluteStyles = False`, rândurile `Absolute 36` /
  `Absolute 40`, `Size = (1285, 104)` (rândurile însumează 76, restul de 28 e gol).
- Copiii din rândul 2 sunt `cmbPartener` (KBotComboBox), `txtCodClient` și `txtIban` (KBotTextBox).
  Toți sunt `Dock = Fill`, cu margine 4 sus și 4 jos.
- Copiii au `Size.Height` 40 / 60 / 60, valorile salvate de designer. O încercare de a le pune la 32
  a fost întoarsă la cererea Adelinei.
- Înainte de această modificare, designerul salvase `txtCodClient` și `txtIban` cu înălțimea 60
  într-un rând de 40. Un copil `Dock = Fill` ar trebui să primească înălțimea celulei (40 − 8 = 32),
  nu 60.

## 1. Ipoteze, cu dovezi pentru și contra

| # | Ipoteza | Pentru | Contra / nevăzut |
|---|---------|--------|------------------|
| A | **Regula de „fit”** (`KBotTableLayoutPanel.Fit.vb`: rând = max(scris, ce cer copiii)) | Ar da exact „rândul crește peste valoarea scrisă” | `AutoFitToTheme = False` pe `tlyAntet`, deci nu adaugă nimic. **Exclusă** cât timp e False. |
| B | **`KBotTextBox` nu se lasă strâns de celulă** | Designerul a salvat 60 într-un rând de 40 (vezi 0) | Nu am găsit în `KBotTextBox.vb` nici `SetBoundsCore`, nici `MinimumSize`. Cauza nu se vede în cod, trebuie măsurată. |
| C | **Designer nereîncărcat** (poza arată valorile vechi 40/60/60) | Cifrele din poză coincid cu valorile vechi din fișier | Nu se știe dacă poza e din designer sau din aplicația pornită. |
| D | **`ScaleAbsoluteStyles = False` + pagină salvată la 144 dpi** | Cod citit (`KBotTableLayoutPanel.Dpi.vb`): la prima citire, rândul scris se împarte la 144/96 (40 devine 26,7 logic), apoi NU se mai înmulțește la loc. La rulare, pe ecranul de 150%, rândul iese ~27 px, nu 40. | Explică un rând **mai mic** decât cel scris și copii tăiați, nu unul mai mare. E totuși o abatere de la ținta „ce scrii = ce vezi” și trebuie confirmată. |

## 2. Măsurători (nimic nu se schimbă în cod)

**2.1 În designer.** Le face Adelina sau, cu permisiunea ei, Claude prin captură de ecran.
1. Închide designerul și fișierul `.Designer.vb`, rulează Rebuild pe `KBot.App`, redeschide designerul.
2. Pe `tlyAntet`, în `Rows`: cât arată rândul 2.
3. Pe `txtCodClient`: `Size`, `Dock`, `MinimumSize`, `Margin`, `Location`.
4. Răspunde la întrebarea: textbox-ul iese sub linia punctată a rândului (ipoteza B) sau nu
   (ipoteza C)?

  Aici ipoteza C se poate verifica doar cu designerul reîncărcat: valorile din fișier sunt acum tot
  40 / 60 / 60, deci o poză făcută înainte nu mai poate deosebi B de C.

**2.2 În aplicația pornită, pe ecranul de 150%.** Doar cu acordul Adelinei: e o rulare a aplicației,
iar regula „fără rulări” se ridică numai la cererea ei.
1. Citește: `tlyAntet.DesignDpi`, `DebugAuthoredRow(1)`, `RowStyles(1).Height`, `DpiScale`, plus
   `txtCodClient.Bounds` și `txtCodClient.GetPreferredSize(Size.Empty)`.
2. Mod de citire, la alegere: o captură măsurată pe pixeli, sau o linie temporară de log scoasă
   după măsurare.

## 3. Decizia (după cifre, aleasă de Adelina)

- Dacă se confirmă **B**: remedierea e în `KBotTextBox` (controlul), nu în designerul paginii.
  Regula casei: aspectul se schimbă doar în control.
- Dacă se confirmă **C**: nu e nimic de reparat.
- Dacă se confirmă **D**: `ScaleAbsoluteStyles = False` nu respectă ținta pe o pagină salvată la
  144 dpi. Variante de propus:
  1. Valoarea scrisă se ia ca pixeli ai ecranului pe care s-a salvat pagina (nu se mai împarte la
     144/96).
  2. Se scoate `ScaleAbsoluteStyles = False` de pe `tlyAntet`.

  Varianta 1 schimbă controlul, deci toate tabelele care au False.
- Orice remediere: o singură modificare, apoi build pe `KBot.App` și măsurătoarea de la 2.1 / 2.2
  repetată de Adelina.

## 4. Ce nu face planul

- Nu atinge `AutoFitToTheme` pe alte tabele și nu modifică regula de fit pentru toate tabelele fără
  decizia Adelinei.
- Nu adaugă worklog, status sau ajutor: e o ajustare de aranjare în pagină (regula „Layout fixes are
  NOT logged”). Dacă remedierea ajunge în control (B sau D, varianta 1), atunci se cere o felie
  (slice) de la Adelina **înainte** de cod.
