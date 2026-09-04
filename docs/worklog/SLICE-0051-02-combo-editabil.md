# SLICE-0051-02 — `KBotComboBox` se poate TASTA (`Editable`) și poate accepta valori din afara listei (`LimitToList`)

Rundă corectivă la felia 0051. Cerința operatorului, în două propoziții: combo-ul K-BOT trebuie
să permită tastarea, printr-o proprietate expusă în designer; și trebuie să aibă o proprietate
nouă, `LimitToList`, care — stinsă — lasă operatorul să scrie o valoare inexistentă și o
**păstrează la ieșirea din câmp**.

**Stare:** cod verde. `dotnet build src/KBot.Controls/KBot.Controls.vbproj` → **0 erori /
0 avertismente**. `dotnet build KBot.sln --no-incremental` → **0 erori / 14 avertismente**, toate
`MSB3825` PREEXISTENTE pe `.resx`-urile din `KBot.App` (`*.ImageStream` prin `BinaryFormatter`) —
niciunul nou, niciunul din proiectele atinse aici.

**S-A VĂZUT PE ECRAN** — pe schema întunecată ȘI pe cea clasică, la 150% DPI, cu capturi de
ecran adevărate (nu `DrawToBitmap`; vezi §5 pentru ce anume s-a văzut și de ce `DrawToBitmap` NU
e de ajuns pentru fața editabilă).

**Fără teste.** Nu s-a scris niciun test și nu s-a rulat nicio suită — cerut explicit de operator.

---

## 0. Ce s-a citit înainte de orice

`docs/worklog/CODE_WORKFLOW.md` · `docs/worklog/KBOT_STATUS.md` · `CLAUDE.md` ·
`src/KBot.Controls/CONTROLS.md` (C1..C9) · `src/KBot.Controls/Combo/KBotComboBox.vb` + `.md`
(integral, verbatim) · `src/KBot.Theming/Interop/NativeMethods.vb` ·
`src/KBot.Controls/Calendar/KBotDatePicker.vb` (controlul cel mai nou — tiparul de stil urmat
aici) · `docs/possible_future_directions.md` §«KBotComboBox cannot be editable».

---

## 1. Premisa feliei 0051 era GREȘITĂ, și de aceea felia asta există

0051 a scris, și în cod și în `possible_future_directions.md`, că un combo editabil nu poate fi
tematizat: are un EDIT nativ copil «pe care pictura noastră nu-l atinge». De aceea antetul DDF
poartă și azi două controale pentru o singură valoare — `txtComp` (valoarea autoritară) lângă
`cmbComp` (doar alegătorul).

Partea adevărată: pictura noastră chiar NU ajunge la EDIT. Partea greșită: **culorile lui nu se
dau cu pensula, ci cu un mesaj.** Un EDIT copil își întreabă PĂRINTELE ce culori să folosească
(`WM_CTLCOLOREDIT`), iar părintele EDIT-ului dintr-un combo **este combo-ul însuși**, nu
formularul. Mesajul ajunge deci exact la noi. Nimeni nu-i răspundea: WinForms reflectă un
`WM_CTLCOLOR*` către controlul MANAGED care deține fereastra care l-a trimis, iar EDIT-ul nu e un
control managed — așa că mesajul cădea în `DefWindowProc` și caseta ieșea în culorile SISTEMULUI,
albă cu text negru. Adică exact dreptunghiul alb pentru care a fost scrisă clasa asta.

Asta **nu e o deducție** — e diferența dintre două capturi de ecran făcute la o oră distanță:
prima, cu `Editable` pus și fără `WndProc`, arată caseta albă pe schema întunecată; a doua, după
ce am răspuns la mesaj, o arată în culorile schemei (§5).

## 2. Ce s-a scris

### `src/KBot.Controls/Combo/KBotComboBox.vb`

- **`Editable As Boolean` (implicit `False`)** — proprietate în designer, categoria «K-BOT Combo».
  Aprinsă, mută stilul pe `DropDown`; stinsă, îl aduce înapoi la `DropDownList`. Comutarea reface
  HWND-ul, deci tema ferestrei de listă și marginile casetei se cer din nou la `OnHandleCreated`.
- **`LimitToList As Boolean` (implicit `True`)** — verdictul asupra unui text care nu e în listă:
  aprins ⇒ câmpul se întoarce la ultima valoare acceptată; stins ⇒ textul rămâne cum a fost tastat,
  iar `SelectedIndex` devine −1, fiindcă nu mai reprezintă niciun rând.
- **`CommitText()`** — public. Dă verdictul ACUM, fără să aștepte ieșirea din câmp; e pentru gazda
  care citește valoarea dintr-un buton («Salvează») pe care operatorul poate ajunge fără să mute
  focusul. Idempotentă.
- **`WndProc`** — răspunde la `WM_CTLCOLOREDIT` (și la `WM_CTLCOLORSTATIC`, pe care îl trimite un
  EDIT dezactivat) cu culorile noastre. Fără el, tot restul feliei ar fi produs o casetă albă pe
  schemă întunecată. Regula casei: `WndProc` rămâne fără `Try` (`Application.ThreadException`).
- **`AlignEditText()`** — marginile interioare ale EDIT-ului, calculate din dreptunghiul LUI real
  (nu dintr-o constantă ghicită): la stânga cât lipsește până la spațiul nostru de 8 px logici, la
  dreapta cât ar intra sub săgeată. Se cer din nou la `OnHandleCreated`, `OnResize` și
  `OnFontChanged`. Măsurat: `rcItem = {3, 3, 428, 26}` la 150% ⇒ marginile ies 5 și 4 px.
- **`OnLeave`** cheamă `CommitText`; **`OnKeyDown`** îl cheamă la Enter cu lista închisă.
  `Leave`, nu `LostFocus`: primul vine de la containerul care mută controlul activ, deci nu se
  aprinde când fereastra listei ia focusul, și cade ÎNAINTE de clic-ul care l-a scos din câmp.
- **`OnPaint`** — în modul editabil nu mai desenăm textul (îl desenează EDIT-ul, l-am fi desenat
  a doua oară peste el), iar evidențierea de hover trece de pe fundal pe contur: EDIT-ul își
  repictează dreptunghiul cu fundalul propriu, deci o umplere s-ar fi văzut doar ca un chenar gros.
- **`DropDownStyle`** nu mai aruncă pe `DropDown`, ci doar pe `Simple` (acolo lista e un panou
  permanent, pe care nu-l desenăm și nu-l tematizăm). Scrierea lui mută steagul `Editable`, ca să
  existe o singură sursă de adevăr; nu se serializează (C4 verificat, §4).
- **Fișierul a fost MĂTURAT în engleză ASCII.** Comentariile și `<Description>`-urile erau
  românești cu diacritice, ceea ce încalcă REGULA 0 și regula «cod și comentarii în engleză»;
  controalele noi (`KBotDatePicker`, `KBotChartView`) sunt deja engleză. Regula spune că vechiul
  se mătură, nu se copiază — de aici diff-ul mare pe un fișier cu adaosuri mici. Categoriile din
  grila de proprietăți: «K-BOT Combo - Culori» ▸ «K-BOT Combo Colors».

### `src/KBot.Theming/Interop/NativeMethods.vb`

Trei membri publici noi, din același motiv pentru care `ApplyWindowTheme` e public: `KBotComboBox`
trăiește în `KBot.Controls` și are nevoie de ei.

- `GetComboEditBounds(combo)` — dreptunghiul EDIT-ului, prin `GetComboBoxInfo`, în coordonate
  client ale combo-ului. `Rectangle.Empty` = nu există (stil `DropDownList`) sau apelul a eșuat.
- `SetComboEditMargins(combo, left, right)` — `EM_SETMARGINS` pe EDIT.
- `ApplyControlColors(hdc, back, fore)` — răspunsul la `WM_CTLCOLOR*`: pune culorile pe DC-ul
  primit și întoarce pensula de fundal. Pensulele se **cachează pe culoare, pentru toată viața
  procesului**: răspunsul la `WM_CTLCOLOR*` e o pensulă pe care apelantul NU o deține, deci nu
  poate fi ștearsă după mesaj — cache-ul e singurul fel de a răspunde fără să scurgem o pensulă la
  fiecare repictare. Mulțimea e mărginită de numărul de culori de input din scheme (o cifră).

### Documentație

`Combo/KBotComboBox.md` rescris (secțiune nouă «Typing», tabelul `LimitToList`, limitele
actualizate) · `CONTROLS.md` — linia din index · `docs/possible_future_directions.md` — secțiunea
«KBotComboBox cannot be editable» e marcată ÎNCHISĂ, cu premisa greșită explicată și cu ce a rămas
de făcut (perechea `txtComp` + `cmbComp` din `DdfEditForm`).

### Versiuni

`KBot.Controls` FileVersion **1.43 ▸ 1.44** · `KBot.Theming` FileVersion **1.10 ▸ 1.11**.
`AssemblyVersion` neatins în ambele (nu se rupe nicio interfață: `DropDownStyle` acceptă acum MAI
MULT decât înainte, nu mai puțin).

## 3. O eroare adevărată, prinsă de verificare, nu de citit codul

Prima scriere a lui `CommitText` întorcea textul refuzat astfel: `MyBase.SelectedIndex = target`.
Rulat, rezultatul a fost: text refuzat «CEVA CE NU E IN LISTA», `SelectedIndex` = 1 — și textul
refuzat **a rămas pe ecran**. Cauza: tastarea în casetă NU mută `SelectedIndex`, deci indexul era
deja cel corect, iar scrierea aceleiași valori e un no-op care nu mai atinge textul. Corecția:
indexul se scrie doar dacă diferă, iar textul se pune înapoi întotdeauna, separat.

## 4. Ce s-a verificat, cum și cu ce rezultat

Banc de probă aruncabil, în directorul de lucru temporar (**nu în depozit**): o consolă
`net8.0-windows` care referă `KBot.Controls`, așază patru combo-uri pe un formular fără chenar,
aplică schema și fotografiază ecranul.

Comportament (tipărit de banc, ambele scheme, identic):

| ce s-a făcut | `LimitToList = True` | `LimitToList = False` |
|---|---|---|
| text tastat care nu e în listă, apoi `CommitText` | `'INVATAMANT'`, idx 1 (întors la ultima valoare acceptată) | `'CEVA CE NU E IN LISTA'`, idx −1 |
| tastat `sanatate` (altă grafie) | `'SANATATE'`, idx 2 — potrivirea e insensibilă la litere mari/mici și ia grafia din listă | idem |
| câmp golit de mână | `'SANATATE'`, idx 2 (gol nu e valoare de listă ⇒ se întoarce) | ar rămâne gol |

C4 (serializarea în designer), verificat prin `TypeDescriptor`:
- pe un control proaspăt, nici `Editable`, nici `LimitToList` nu produc vreo linie;
- `DropDownStyle`, cerut pe nume, răspunde `False` chiar și cu `Editable` aprins.

Geometrie, măsurată cu `GetWindowRect`/`GetClientRect` la 150% DPI: combo la `T154 B186`, EDIT la
`T157 B183` — încadrat corect, 3 px de fiecare parte.

## 5. De ce `DrawToBitmap` NU e de ajuns aici (și ce s-a văzut totuși)

`DrawToBitmap` trimite `WM_PRINT`; pe un control cu `UserPaint`, WinForms îl servește din `OnPaint`
și nu mai coboară la `DefWndProc`, deci **EDIT-ul copil nu e desenat niciodată** în bitmap. Prima
imagine obținută așa arăta casete editabile complet goale — un fals negativ. `WM_PRINT` trimis
direct EDIT-ului a ieșit negru, iar `PrintWindow` pe o fereastră ținută în afara ecranului la fel.
Ce a mers: fereastra ADEVĂRATĂ pe ecran, fotografiată din afara procesului. Cele patru rânduri
văzute, pe `Dark` și pe `Classic`:

1. `Editable = False` — neschimbat față de 0051.
2. `Editable = True`, `LimitToList = True`, cu focus — fundal din schemă, text din schemă, inel de
   focus, cursor de text; textul pornește exact de la același x ca rândul 1.
3. `Editable = True`, `LimitToList = False` — «valoare tastata de operator», păstrată.
4. `Editable = True`, dezactivat — text estompat (calea `WM_CTLCOLORSTATIC`).

Lista desfășurată a fost fotografiată separat: rânduri owner-drawn, fundal din schemă, rândul
curent pe culoarea de accent — aceeași cale ca în modul doar-listă.

## 6. Fișiere atinse

| fișier | ce |
|---|---|
| `src/KBot.Controls/Combo/KBotComboBox.vb` | `Editable`, `LimitToList`, `CommitText`, `WndProc`, `AlignEditText`, `OnLeave`, `OnKeyDown`, `OnResize`; măturat în engleză ASCII |
| `src/KBot.Controls/Combo/KBotComboBox.md` | rescris |
| `src/KBot.Controls/CONTROLS.md` | linia din index |
| `src/KBot.Controls/KBot.Controls.vbproj` | FileVersion 1.43 ▸ 1.44 |
| `src/KBot.Theming/Interop/NativeMethods.vb` | `GetComboEditBounds`, `SetComboEditMargins`, `ApplyControlColors` |
| `src/KBot.Theming/KBot.Theming.vbproj` | FileVersion 1.10 ▸ 1.11 |
| `docs/possible_future_directions.md` | secțiunea marcată ÎNCHISĂ |
| `docs/worklog/SLICE-0051-02-combo-editabil.md` | fișierul ăsta |
| `docs/worklog/KBOT_STATUS.md` | rândul feliei |

Niciun formular gazdă nu s-a atins. Cele opt `KBotComboBox`-uri existente rămân exact cum erau:
`Editable` implicit `False` ⇒ comportament identic cu cel dinainte.

## 7. Rămas neverificat / amânat

- **`DdfEditForm` poartă mai departe perechea `txtComp` + `cmbComp`.** Un `KBotComboBox` cu
  `Editable = True, LimitToList = False` o înlocuiește, dar strângerea celor două într-unul atinge
  calea de SALVARE a DDF-ului (0051 §D19 și zona din jur), deci n-a fost măturată aici împreună cu
  controlul. E singurul consumator care așteaptă felia asta.
- **Nici un formular real cu combo editabil n-a fost deschis** — nici `DdfEditForm`, nici altul, și
  niciunul în designerul Visual Studio. Ce s-a văzut e bancul de probă.
- **Schema `Modern` n-a fost fotografiată** (doar `Dark` și `Classic`).
- **Selecția din interiorul casetei rămâne albastrul SISTEMULUI**, nu culoarea schemei: aia e a
  EDIT-ului nativ și nu se dă printr-un `WM_CTLCOLOR*`. Consemnat și în `.md`, la limite.
- **`AutoCompleteMode` / `AutoCompleteSource` rămân cele moștenite** — controlul nici nu le pune,
  nici nu li se opune. Nimeni nu le-a probat împreună cu `LimitToList`.
- **Zero teste**, cerut explicit.
