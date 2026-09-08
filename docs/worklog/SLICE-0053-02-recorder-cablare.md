# SLICE-0053-02 — Cablarea recorderului (buton în consolă + monitor Wicket)

Data: 2026-09-07

Continuarea feliei 0053 (`SLICE-0053-recorder.md`), care lăsase formularul scris dar
neajuns de nicăieri. Aici se rezolvă punctul (1) din firele deschise: **RecorderForm are
acum call-site**, iar `WicketMonitorForm` — care nici el nu se instanția nicăieri — se
deschide din interiorul recorderului.

## Ce s-a schimbat și de ce

### Seam-ul: `ShowRecorder`, nu `Executor`

`RecorderForm` are nevoie de `WorkflowExecutor`, care trăiește privat în `ForexeRunner`.
Varianta „expunem executorul” ar fi mutat un obiect de motor în `KBot.App` doar ca să fie
pasat înapoi într-un formular care oricum stă în `KBot.Forexe`. În loc de asta, interfața
primește o intenție:

```
IForexeRunner:  Sub ShowRecorder(owner As IWin32Window)
```

`ForexeRunner` construiește formularul la prima cerere, îl ține (modeless) și îl leagă la
executorul CURENT la fiecare deschidere — o reconectare înseamnă alt executor, deci
legarea nu se poate face o singură dată. `FormClosed` anulează referința, așa că
închiderea de către operator lasă loc unui formular nou.

Fără sesiune vie `ShowRecorder` **aruncă**, nu tace: exact tiparul lui `ShowBrowserAsync`.

`DisposeExecutorAsync` cheamă `DetachRecorder()` ÎNAINTE de a închide executorul — altfel
recorderul ar rămâne cu un executor mort în mână și cu butoanele de andocare aprinse
degeaba. Desprinderea se marșalează pe firul de UI (`BeginInvoke`), fiindcă închiderea
sesiunii vine de pe firul robotului.

### Butonul din consolă

`btnRecorder` exista în `ForexeConsoleForm.Designer.vb` (cu iconiță și tooltip) dar
n-avea handler, n-avea stare și nu era temat. Acum:

- `BtnRecorder_Click` → `ForexeController.ShowRecorder()` → `IForexeRunner.ShowRecorder`;
  fără sesiune, un mesaj care spune de ce, nu un buton care nu face nimic;
- `ActualizeazaStarea` îl stinge când nu există sesiune (ca `btnAfiseazaBrowser`);
- `OnThemeChanged` îi aplică `ButtonStyles.ApplySecondary`, ca celorlalte trei.

Proprietarul ferestrei e `ForexeController.Owner` (shell-ul), nu consola: consola se
ascunde la închidere, iar un recorder deținut de ea ar dispărea odată cu ea.

### Monitorul Wicket în recorder

Buton nou `btnMonitor` pe bara de înregistrare (pozițiile celorlalte trei rămân
neatinse; `374,6` × `74,28`, dreapta la 448 într-un panou de 454 — verificat pe ecran).

`BtnMonitor_Click` pornește întâi `StartWicketMonitoringAsync` dacă nu rulează deja:
`WicketMonitorForm` se hrănește exclusiv din `OnWicketStateChange`, deci fără asta
fereastra s-ar deschide goală și ar părea stricată. Apoi creează monitorul o singură
dată, îl leagă la același executor și îl arată; la a doua apăsare doar îl aduce în față
(și îl restaurează dacă e minimizat).

Monitorul își anulează propriul `FormClosing` la închiderea de către operator și se
ascunde, deci NIMIC în afară de recorder nu-l distruge: `RecorderForm.Dispose` îl
`Dispose`-uiește direct, ocolind `FormClosing` — exact ce trebuie.

`AttachExecutor` / `DetachExecutor` din recorder duc mai departe schimbarea și către
monitor, ca să nu rămână legat de un executor pe care recorderul l-a lăsat deja.

## Fișiere atinse

- `src/KBot.Forexe/IForexeRunner.vb` — `ShowRecorder(owner)`
- `src/KBot.Forexe/ForexeRunner.vb` — `_recorder`, `ShowRecorder`, `Recorder_FormClosed`,
  `DetachRecorder`, apel în `DisposeExecutorAsync`
- `src/KBot.Forexe/Helpers/RecorderForm.vb` — `_monitor`, `BtnMonitor_Click`,
  monitorul urmărit în `AttachExecutor`/`DetachExecutor`/`Dispose`, gardă `IsDisposed`
  în `UpdateButtons`
- `src/KBot.Forexe/Helpers/RecorderForm.Designer.vb` — `btnMonitor`
- `src/KBot.App/Forexe/ForexeController.vb` — `ShowRecorder()`
- `src/KBot.App/Forexe/ForexeConsoleForm.vb` — handler, stare, temă pe `btnRecorder`
- FileVersion: `KBot.Forexe` 1.0.5 ▸ 1.0.6, `KBot.App` 1.0.25 ▸ 1.0.26

## Rezultate

- `dotnet build src\KBot.App\KBot.App.vbproj` — **0 erori**; cele 7 `MSB3825` sunt
  preexistente (`.resx` din `KBot.App`, BinaryFormatter), fără legătură cu felia.
- **Văzut pe ecran** (`DrawToBitmap`, Classic și Dark): `btnMonitor` stă în bara de
  înregistrare, nu depășește panoul (dreapta 448 < 456) și e stins cât nu există
  executor; `btnRecorder` din consolă arată acum ca vecinii lui, nu ca un buton de
  sistem.

## Rămas neverificat

- Nimic din asta n-a atins un browser adevărat. Deschiderea recorderului dintr-o sesiune
  vie, andocarea și monitorul cu trafic Wicket real rămân punctele (2) și (3) din firele
  deschise ale feliei 0053.
- Fereastra e deținută de shell: nu s-a verificat pe ecran cum se comportă z-order-ul
  când operatorul aduce MainForm în față peste browserul andocat.
