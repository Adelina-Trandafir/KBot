# SLICE-0010-05 — KBotDataView: input + selecție

A cincea trecere din slice-ul 0010. Grila devine INTERACTIVĂ: celulă curentă, navigație de la
tastatură în stil Access, click/dublu-click, comutarea bifelor/opțiunilor, acționarea butoanelor
și redimensionarea coloanelor din antet. Totul respectă activarea efectivă definită în 0010-04.

## Ce s-a schimbat și de ce

### Selecție
`CurrentRowIndex`, `CurrentColumnKey`, `CurrentRow` + evenimentul `SelectionChanged`. Selecția
trece printr-un punct UNIC (`SetCurrentCell`), care derulează la rând (`EnsureVisible`),
repictează și ridică evenimentul **o singură dată și doar la o schimbare reală** (același
contract ca la `KBotNavList`). Index de rând în afara intervalului => „fără selecție” (-1);
cheie de coloană necunoscută => `ArgumentException` (fără no-op tăcut). Selecție simplă în v1;
rândul curent se pictează cu spălarea de accent introdusă în 0010-02.

### Tastatură (stil formular continuu Access)
`IsInputKey` revendică săgețile / Tab / Enter / F2 / PageUp / PageDown / Home / End / Space —
altfel WinForms le-ar folosi pentru schimbarea focusului. Apoi:
Sus/Jos = rând, Stânga/Dreapta = coloană (**sar peste coloanele ascunse și dezactivate**, fără
wrap), **Enter = un rând mai jos** (senzația Access), Tab/Shift+Tab = coloană dreapta/stânga,
PageUp/PageDown = o pagină vizibilă, Home/End = prima/ultima coloană activă,
Ctrl+Home/Ctrl+End = primul/ultimul rând, Space = comută/acționează celula curentă.

### Mouse
`OnMouseDown` ia focusul (ca `KBotNavList`) și selectează celula; `OnMouseUp` comută/acționează
și ridică `CellClick`; `OnMouseDoubleClick` ridică `CellDoubleClick` (intrarea în editare se
adaugă în 0010-06). Rotița era deja cablată din 0010-02.

### Comutare / acționare (`ActivateCell`)
Punct comun pentru click și Space, care **verifică întâi `IsCellEnabled`** — o celulă inertă nu
face nimic (nici valoare schimbată, nici eveniment). Apoi:
- **CheckBox** — comută și ridică `CellValueChanged`.
- **OptionButton** — selectează prin `SetOptionValue` (stinge surorile din grup) și ridică
  `CellValueChanged`. Un radio deja bifat **nu se de-bifează** (no-op, fără eveniment) — așa se
  comportă un grup radio.
- **Button** — ridică doar `ButtonClick`; **nu** ține valoare, deci **nu murdărește rândul**.

`ActivateCell` e `Friend`, ca testele să-l poată chema headless (nu se pot trimite taste reale
fără buclă de mesaje) — același tipar pe care planul îl cere pentru editare în 0010-06.

### Redimensionare de coloană
Drag pe marginea dreaptă a unei coloane din antet (toleranță ~4px, DPI-scalată), doar dacă
`Column.Resizable`. Cursorul devine `SizeWE` deasupra unei margini. Lățimea nu coboară sub
`MinWidth` (clamp-ul e în model, din 0010-01). Reordonarea prin drag rămâne în afara scopului.

### Evenimente noi
`KBotCellEventArgs` (bază), `KBotButtonClickEventArgs`, `KBotCellValueEventArgs` — **NU** se
refolosesc, spre deosebire de argumentele de formatare: evenimentele de interacțiune sunt rare
(o acțiune a operatorului), deci o alocare per eveniment e irelevantă și scapă handler-ele de
capcana „nu reține instanța”.

## Fișiere atinse

- `src/KBot.Controls/KBotDataView.Input.vb` (nou — selecție, hit-testing, tastatură, mouse, resize)
- `src/KBot.Controls/KBotDataView.vb` — `_currentColumnKey`, cele cinci evenimente de interacțiune
- `src/KBot.Controls/Events/KBotCellEventArgs.vb`, `KBotButtonClickEventArgs.vb`,
  `KBotCellValueEventArgs.vb` (noi)
- `tests/KBot.Controls.Tests/KBotDataViewInputTests.vb` (nou, 18 teste)
- `docs/worklog/KBOT_STATUS.md`

## Rezultate teste

- `dotnet build KBot.sln -c Debug` — **0 warnings / 0 errors**.
- `dotnet test KBot.sln` — **141 passed / 0 failed / 0 skipped** (57 în `KBot.Controls.Tests`).
- Acoperire nouă: `NextEnabledColumn` (un pas, sare peste dezactivate ȘI ascunse, fără wrap,
  fără punct de plecare => prima/ultima activă), selecția (eveniment doar la schimbare reală,
  index invalid => -1, `CurrentRow` urmează indexul, cheie necunoscută => excepție),
  `EnsureVisible` (derulează în jos ca rândul să încapă COMPLET în fereastră, derulează înapoi
  sus, ignoră un index invalid), și comutarea (bifă dus-întors, opțiune care stinge sora,
  radio deja bifat = no-op, buton care nu murdărește rândul, celulă/rând dezactivat = nimic).

## Rămas neverificat / amânat

- **Verdictul vizual uman tot NU a fost dat**, iar acum lipsa lui acoperă și interacțiunea:
  navigația cu tastatura, drag-ul de redimensionare și senzația de derulare **nu pot fi testate
  headless** (nu există buclă de mesaje). Testele acoperă LOGICA din spatele handler-ilor, nu
  traseul WinForms tastă→handler. Se validează rulând harness-ul (DevHarness → Controls/UI).
- Hover/pressed pe celulele `Button` — **tot neimplementat**; acum ar fi posibil (există
  urmărire de mouse), dar nu era cerut de acceptanța pasului. Rămâne datorie mică.
- F2 e revendicat de `IsInputKey`, dar încă nu face nimic — intrarea în editare vine în 0010-06.
- Selecție multiplă, reordonare de coloane — în afara scopului v1.
- Urmează: **0010-06** (editare: editorii flotanți, commit/veto/cancel, `IsDirty` real).
