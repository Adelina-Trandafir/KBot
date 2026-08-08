# SLICE-0026 — toate controalele în `KBot.Controls`, grupate pe familii

Cerere de operator, fără plan prealabil:

> «all custom controls MUST live in kbot.controls. all controls must have their own separate
> folder in that project. update everywhere.»
>
> «they must be grouped logically. all stuff for dgv in the same folder. all for treeview in
> the same folder, all for adobe viewer in the same folder... and so on»

Felie pur structurală: **niciun comportament nu s-a schimbat**. Zero modificări de logică în
vreun control — doar mutări de fișiere, calificări de tip și lărgirea a exact două puncte de
acces din motorul de teme.

---

## 1. Decizii de perimetru (întrebate, nu presupuse)

| Întrebare | Răspuns operator | Consecință |
|---|---|---|
| `KBotThemedForm` / `KBotShellForm` se mută? | **Nu** — rămân în `KBot.Theming` | Sunt form-uri de bază, nu controale. Mai important: `KBot.Theming` rămâne astfel **fără nicio referință** la `KBot.Controls`, deci sensul referinței rămâne unic și fără ciclu |
| Vederile din `KBot.App\Views\` se mută? | **Nu** — sunt ecrane ale aplicației | `SumarView`/`IstoricView`/`DdfView`/`DdfFileBrowser`/`XfaXmlPreview`/`ReaderHostPreview`/`PlaceholderView` implementează `IAngajamentView` și cheamă API-ul; mutate, ar fi tras `KBot.Api`/`KBot.Forexe`/`KBot.Xfa` în biblioteca de UI, care trebuie să rămână frunză |

## 2. Ce s-a mutat

### 2.1 Din `KBot.Theming` în `KBot.Controls` (cinci controale)

Toate prin `git mv`, deci istoricul se urmărește:

| Fișier | Destinație |
|---|---|
| `KBotNavList.vb`, `KBotNavItem.vb` | `KBot.Controls/NavList/` |
| `KBotCaptionBar.vb` | `KBot.Controls/CaptionBar/` |
| `KBotBusyBar.vb` | `KBot.Controls/BusyBar/` |
| `KBotNotice.vb` | `KBot.Controls/Notice/` |
| `KBotTextField.vb` | `KBot.Controls/TextField/` |

Enum-urile călătoresc cu controlul lor: `KBotNavOrientation`/`KBotNavAlign`/`KBotNavCorner`/
`KBotNavCollapseState` sunt în `NavList/KBotNavList.vb`, `NoticeKind` în `Notice/KBotNotice.vb`.

În `KBot.Theming` au rămas: `ThemeManager`, `ThemePalette`, `ThemeScheme`, `ThemeStyleOptions`,
`ThemeStore`, `BuiltInSchemes`, `ColorHex`, `ButtonStyles`, `ModernRenderer`, `IThemedControl`,
`ThemeShapes`, `KBotDesignTime`, `Interop/NativeMethods` și cele două form-uri de bază.

### 2.2 Regruparea a ceea ce era deja în `KBot.Controls`

Proiectul avea 40+ fișiere în rădăcină. Acum:

| Folder | Fișiere |
|---|---|
| `Tree/` | cele 15 `AdvancedTreeControl*.vb` + `ColumnDef.vb`, `TreeLogger.vb`, `TooltipTableModel.vb`, `NodeDebugInfo.vb`, `FrmNodeDebug.vb` + `.Designer.vb` |
| `DataView/` | cele 9 `KBotDataView*.vb` + `KBotDataColumn.vb`, `KBotDataColumnCollection.vb`, `KBotDataRow.vb`, `KBotAggregate.vb`, `KBotAutoSizeMode.vb`, `KBotColumnType.vb`, `KBotFillMode.vb` |
| `DataView/Events/` | fostul `Events/` — cele șase `KBotCell*`/`KBotRow*`/`KBotButtonClickEventArgs`; verificat prin grep că sunt consumate **exclusiv** de `KBotDataView`, nu de arbore |
| `Adobe/` | neatins (era deja grupat corect) |

În rădăcină au rămas doar `AssemblyInfo.vb`, `KBot.Controls.vbproj` și `README.md`.

## 3. Ce a trebuit lărgit în `KBot.Theming` (și de ce exact atât)

Controalele mutate au trecut într-un **alt assembly**, deci membrii `Friend` de care se
sprijineau au devenit invizibili. Două puncte, ambele minime:

1. **`ThemeShapes`** `Friend Module` → `Public Module`, iar `ScaleDpi`/`RoundedRect`/`Blend`
   `Friend` → `Public`. Modulul e folosit ȘI de ce a rămas în motor (`ModernRenderer`,
   `KBotShellForm`), deci **nu putea fi mutat** în `KBot.Controls` — trebuia expus.
   (Nota din `SLICE-0010-01` — «`ThemeShapes` e `Friend`, deci invizibil din `KBot.Controls`;
   am replicat» — descrie situația de dinainte de felia asta.)
2. **`Interop/NativeMethods`** `Friend Module` → `Public Module`, dar **numai `DragMove` a
   rămas `Public`**: `SetTitleBarDark`, `SetRoundedCorners`, `ApplyMinMaxInfo` și
   `ApplyWindowTheme` au fost coborâte `Public` → `Friend`, fiindcă sunt consumate exclusiv
   din `KBot.Theming` (`ThemeManager` și `KBotShellForm`). Suprafața publică nouă a modulului
   e deci **un singur membru**, cel cerut de `KBotCaptionBar`. Constantele `WM_*`/`HT*` și
   structurile `NativePoint`/`MINMAXINFO` rămân `Friend`.

`KBotDesignTime` era deja `Public` (lărgit la felia 0025 exact din același motiv).

## 4. Actualizări la apelanți

- `MainForm.Designer.vb`, `LoginForm.Designer.vb`, `InternalInfoForm.Designer.vb`,
  `DdfView.Designer.vb` — tipurile calificate `KBot.Theming.KBotX` → `KBot.Controls.KBotX`.
- **Capcană reală, prinsă de compilator:** forma scurtă `Controls.KBotNavCorner` **nu
  compilează** într-un fișier de designer de formular — `Controls` se leagă mai întâi de
  proprietatea moștenită `Control.ControlCollection`, iar eroarea e
  `BC30456: 'KBotNavCorner' is not a member of 'Control.ControlCollection'`. Vechiul
  `Theming.X` nu avea coliziunea asta. De aceea **tot ce vine din `KBot.Controls` în cele
  patru fișiere de designer se scrie CALIFICAT COMPLET** (`KBot.Controls.KBotNavItem`).
  Ciudățenia care merge totuși: `New Controls.KBotDataView()` (context `New`) — există de
  dinainte în vederile existente și a fost lăsat neatins, dar forma corectă e cea calificată.
- `LoginForm.vb` — `NoticeKind.Error` → `KBot.Controls.NoticeKind.Error`. Un `Imports
  KBot.Controls` la începutul fișierului **NU merge**: face `Write` ambiguu între
  `KBot.Common.GlobalErrorLog.Write` și `KBot.Controls.AdobeHostLog.Write` (ambele module).
- `tests/KBot.App.Tests/MainFormNavItemsTests.vb` — `Imports KBot.Controls` adăugat.
- Testele `KBotNavListTests.vb` (20), `KBotNavListSelectionTests.vb` (4) și
  `KBotNavListCollapseTests.vb` (25) mutate din `KBot.Theming.Tests` în `KBot.Controls.Tests`.
  `KBot.Controls.Tests` **nu** a avut nevoie de `DisableTestParallelization` (spre deosebire
  de `KBot.Theming.Tests`): verificat prin grep că niciun test de acolo nu atinge starea
  statică `ThemeManager`/`ThemeStore`.

## 5. Fișiere atinse

| Fișier | Ce |
|---|---|
| `src/KBot.Controls/{NavList,CaptionBar,BusyBar,Notice,TextField}/*.vb` | **mutate** din `KBot.Theming` |
| `src/KBot.Controls/{Tree,DataView,DataView/Events}/*.vb` | **mutate** din rădăcina proiectului |
| `src/KBot.Controls/KBot.Controls.vbproj` | convenția de organizare documentată în comentariu; comentariul referinței `KBot.Theming` rescris; `FileVersion` 1.3.0.0 → **1.4.0.0** |
| `src/KBot.Controls/README.md` | rescris — era o notă de import din 2025, acum e tabelul de foldere + regula |
| `src/KBot.Theming/ThemeShapes.vb` | `Friend` → `Public` (modul + 3 funcții) |
| `src/KBot.Theming/Interop/NativeMethods.vb` | modul `Public`; 4 membri coborâți la `Friend`, doar `DragMove` public |
| `src/KBot.Theming/KBot.Theming.vbproj` | comentariu nou: «niciun control aici». `FileVersion` era deja urcat la 1.3.0.0 de pasul 0025-05 (necomis), acoperă și schimbarea asta |
| `src/KBot.App/*.Designer.vb` (4), `src/KBot.App/LoginForm.vb` | calificări de tip |
| `tests/KBot.Controls.Tests/KBotNavList*.vb` | **mutate** din `KBot.Theming.Tests` |
| `tests/KBot.App.Tests/MainFormNavItemsTests.vb` | `Imports` |
| `CLAUDE.md` | tabelul de proiecte + două reguli noi de casă + lista corectă de proiecte de test |

## 6. Verificare

```
dotnet build KBot.sln   →  0 erori / 0 avertismente
dotnet test  KBot.sln   →  812 passed / 0 failed / 0 skipped
```

Aceleași 812 teste ca înainte de felie, redistribuite: `KBot.Theming.Tests` 76 → **27**,
`KBot.Controls.Tests` 286 → **335**.

⚠️ **Ce NU s-a verificat.** Nimic nu s-a deschis în designer-ul Visual Studio după mutare.
Riscul specific al feliei: `MainForm`/`LoginForm`/`DdfView` conțin controale acum dintr-un alt
assembly, iar Toolbox-ul și cache-ul de design-time al VS trebuie să le regăsească sub numele
nou (`KBot.Controls.*`). Firele (a)–(j) din felia 0025 rămân toate deschise — felia asta nu a
încercat să închidă niciunul.
