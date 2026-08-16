# SLICE-0036 — Opțiunile temei (fereastră proprie) + scalarea reglabilă

## Ce s-a cerut

Două lucruri, una singură ca miez:

1. **Toate opțiunile temei într-un formular**, ajuns din butonul de temă al barei de titlu — nu
   valorile unor controale de pe un ecran anume (aia există deja, e `ThemeEditorForm`), ci
   **valorile generale ale temei: scalare, spațiere și așa mai departe, pentru fiecare schemă**
   (Modern, Întunecat, …).
2. **Un comutator pentru scalarea calculată.** Motivul, în cuvintele operatorului: proiectarea se
   face la 1920×1080 cu 100%, iar rulată la altă scalare «dimensiunea a tot se distorsionează».

## Deciziile luate înainte de cod (întrebate, nu presupuse)

| Întrebare | Alegerea operatorului |
|---|---|
| Ce înseamnă «scalare oprită»? | **Amândouă**: mod tri-stare pentru măsurile K-BOT (Automat / Fix 100% / Manual) **și** un comutator separat «Windows întinde fereastra» (DPI-unaware, cere repornire) |
| Scalarea e a schemei sau a aplicației? | **A aplicației** — în `theme.json`, lângă schema activă |
| Cum se păstrează editarea unei scheme built-in? | **Fișier de suprascriere per schemă** în `…\AVACONT\Themes\<Nume>.json`, încărcat peste cea compilată; «Restaurează implicit» îl șterge |

## Ce s-a schimbat și de ce

### (a) `AppScaling` — sursa UNICĂ a scării

Până acum formula `DeviceDpi / 96` era scrisă în **trei** locuri: `ThemeShapes.ScaleDpi` (drumul pe
care merg ~157 de locuri din pictura controalelor), plus câte un `_dpiScale` în
`AdvancedTreeControl` și în `KBotDataView`. Formula era una singură, dar **nu exista niciun loc din
care s-o poți schimba**. Modulul nou e acel loc; cele trei drumuri întreabă acum acolo, deci o
alegere ajunge peste tot deodată sau nicăieri — niciodată pe jumătate. Asta a și fost condiția de
proiectare: o setare aplicată doar arborelui și grilei ar fi fost mai rea decât niciuna.

`Automatic` calculează exact ce se calcula înainte, deci **implicitul e comportamentul dinaintea
feliei**, bit cu bit.

Difuzarea (`AppScaling.Broadcast`) merge pe ferestrele deschise și cheamă
`IDpiScaledControl.RefreshDpiMetrics` — interfață nouă în `KBot.Theming`, implementată de arbore și
de grilă. De ce doar ele: constantele dintr-un `OnPaint` trec prin `ScaleDpi` la **fiecare** pictare,
deci le prinde o invalidare; cine își ține înălțimea de rând sau lățimile de coloană într-un **câmp**
le-a calculat o dată, la scara de atunci, și trebuie chemat pe nume. Difuzarea **nu** se oprește la
`IThemedControl` (spre deosebire de `ThemeManager.Traverse`): scara nu e o culoare, n-are cum să
strice un copil intern.

Evenimentul `ScalingChanged` există, dar difuzarea **nu** merge pe el — un eveniment de modul ar fi
ținut referințe tari la controale și le-ar fi scurs.

### (b) Onestitatea celor două comutatoare

`Fix 100%` oprește scalarea măsurilor **NOASTRE**. Fonturile (care sunt în puncte) și `Bounds`-urile
controalelor obișnuite le scalează în continuare WinForms prin `AutoScaleMode.Font`, iar acela nu
poate fi oprit din afară — deci la 150% textul rămâne mai mare decât geometria din jurul lui. **E
compromisul modului, nu o scăpare**, și e scris ca atare atât în cod, cât și pe eticheta din
fereastră.

Singurul comutator care dă proporții IDENTICE cu proiectarea e `DpiUnaware`, fiindcă acolo întinde
Windows toată fereastra ca bitmap; costul e textul mai moale. De aceea sunt **două** setări.

Nota din felia 0035 spunea că a face aplicația surdă la scalare «ar înrăutăți». Rămâne adevărată ca
implicit — de aceea bifa e **stinsă** din start și poartă avertismentul ei; dar operatorul a cerut-o
explicit ca opțiune, iar alegerea e a lui.

`Program.Main` citește setarea **înaintea** lui `SetHighDpiMode`, fiindcă modul DPI al unui proces nu
se mai poate schimba după prima fereastră — de aici «necesită repornire» din UI.

### (c) `ThemeOptionsForm` — opțiunile SCHEMEI

Fereastră nouă în `KBot.Controls/ThemeOptions/` (unde stă și `ThemeEditorForm`, aceeași precedență).
Un `PropertyGrid` pe categorii, hrănit de `SchemeOptionsProxy`: **cele 23 de sloturi de culoare ale
paletei** (ca `Color`, deci cu selectorul de culoare, nu ca șiruri `#RRGGBB`) plus **toate opțiunile
de stil** — culori de sistem, păstrează culorile din designer, controale plate, randarea butoanelor,
raza colțului, accent pe focus, bară de titlu întunecată, desenează filele, font de bază (listă
derulantă cu fonturile instalate), dimensiune font, spațiere internă.

Trei reguli scrise în ea:

- **Alegerea schemei din listă o și ACTIVEAZĂ.** Editorul arată efectul pe ferestrele din spate,
  deci ce editezi trebuie să fie ce vezi.
- **Efect imediat, salvare explicită.** Fiecare valoare atinsă se vede pe loc (`ThemeManager.Refresh`,
  nou — difuzează fără să schimbe și fără să persiste schema); pe disc nu ajunge nimic până la
  «Salvează». Închiderea cu modificări nesalvate întreabă.
- **Nu se confundă cu «Stiluri...».** Rândurile de meniu sunt scrise diferit, iar comentariul din
  capul fiecărei ferestre spune care face ce.

### (d) Suprascrierea schemelor built-in

`ThemeManager.AvailableSchemes` nu mai concatenează orbește. Un fișier cu numele unei scheme built-in
o **înlocuiește**, păstrându-i poziția în listă. Regula veche ar fi produs două rânduri «Modern» în
meniul de teme, iar `ResolveByName` — care întoarce primul potrivit — ar fi ales mereu pe cel
**NEeditat**: salvarea ar fi părut că nu face nimic.

`ThemeStore` a primit `SaveScheme` / `DeleteScheme` / `SchemeFilePath` și — important — **toate
scrierile în `theme.json` trec acum prin citire-modificare-scriere**: schema activă și scalarea stau
în același fișier și se scriu din locuri diferite, deci fără asta una ar fi ștears-o pe cealaltă.
Un `theme.json` vechi, fără câmpurile noi, primește implicitele.

### (e) Meniul butonului de temă

Rând nou «Opțiuni temă...», **înaintea** lui «Stiluri...» (prima e ce caută operatorul în nouă din
zece cazuri; a doua e unealta rară). Comutator propriu — `ShowThemeOptions` — deliberat **separat**
de `ShowThemeEditor`: cele două unelte fac lucruri diferite, deci o fereastră care o vrea pe una
n-are de ce s-o capete și pe cealaltă. `LoginForm`, care stingea deja editorul, stinge acum și
opțiunile: înainte de autentificare rămâne un comutator de scheme curat.

Separatorul aparține **grupului** de unelte: se pune o dată dacă rămâne măcar una aprinsă și pleacă
doar când se sting amândouă.

## Fișiere atinse

**Noi**
- `src/KBot.Theming/AppScaling.vb` — enum `ScalingMode` + modulul de scalare
- `src/KBot.Theming/IDpiScaledControl.vb`
- `src/KBot.Controls/ThemeOptions/ThemeOptionsForm.vb` / `.Designer.vb`
- `src/KBot.Controls/ThemeOptions/SchemeOptionsProxy.vb` (+ `InstalledFontNameConverter`)
- `tests/KBot.Theming.Tests/AppScalingTests.vb`
- `tests/KBot.Theming.Tests/SchemeOverrideTests.vb`

**Modificate**
- `src/KBot.Theming/ThemeShapes.vb` — `ScaleDpi` devine drum, nu decizie
- `src/KBot.Theming/ThemeStore.vb` — config citire-modificare-scriere, scalare, fișiere de schemă
- `src/KBot.Theming/ThemeManager.vb` — `MergeSchemes`, `Refresh`, `SaveScheme`, `ResetScheme`,
  încărcarea scalării în `Initialize`
- `src/KBot.Controls/Tree/AdvancedTreeControl.Dpi.vb`, `src/KBot.Controls/DataView/KBotDataView.Dpi.vb`
  — scara din `AppScaling`, `IDpiScaledControl`
- `src/KBot.Controls/CaptionBar/KBotCaptionBar.ThemeButton.vb` / `.vb` — rândul + `ShowThemeOptions`
- `src/KBot.App/Program.vb` — `HighDpiMode` după setare
- `src/KBot.App/LoginForm.Designer.vb` — `ShowThemeOptions = False`
- `tests/KBot.Controls.Tests/KBotCaptionBarThemeButtonTests.vb`
- cele trei `.vbproj`: `FileVersion` 1.8.0.0 / 1.27.0.0 / 1.0.19.0

## Rezultatele testelor

- **Build soluție: 0 erori, 5 avertismente `MSB3825`** — exact cele preexistente (resurse
  `ImageStream` din vederile DDF/ORD/Plăți/Recepții/Rezervări).
- **`KBot.Theming.Tests`: 96 verzi** (71 înainte, +25 noi — scalarea și suprascrierea schemelor).
- **`KBot.Controls.Tests`: 834 verzi** (831 înainte, +3 noi pe meniul butonului de temă). Două teste
  existente au fost **rescrise, nu șterse**: meniul are acum două rânduri de unealtă, deci
  numărătoarea de scheme le scoate pe amândouă, iar separatorul nu mai pleacă odată cu «Stiluri...».
- **`KBot.App.Tests`: 151 verzi / 10 căzute.** Toate cele 10 sunt **preexistente**, din lucrul
  necomis aflat în arborele de lucru (eticheta de navigare `Doc. Fundamentare` → `Fundamentare`,
  coloana `obs` lipsă din grila Istoric, luna `Martie 2026` → `Martie`, parserul XFA care întoarce
  gol). **Niciuna nu atinge tema, scalarea sau bara de titlu.**
  ⚠️ Verificarea s-a făcut **citind mesajele de eșec**, nu printr-o probă `git stash`: cele două
  fișiere `.Dpi.vb` din felia 0035 sunt încă **neurmărite** (felia aceea n-a fost comisă), deci nu
  pot fi date deoparte cu `git stash` fără să atingă lucrul operatorului.

## Rămas neverificat / amânat

1. **Nimic n-a fost privit pe ecran** — nici fereastra nouă, nici meniul, nici vreunul dintre
   modurile de scalare la 100% / 125% / 150%. Riscul standing 0025/0027/0035.
2. **`DpiUnaware` n-a fost pornit niciodată** — calea cere repornirea aplicației și un ecran cu
   scalare ≠ 100%. Efectul lui asupra ferestrelor fără chenar (`KBotShellForm`, `WM_NCHITTEST` /
   `WM_GETMINMAXINFO`) e **neexercitat** și e locul cel mai probabil de surprize.
3. **Dus-întorsul prin designerul VS** al lui `ThemeOptionsForm.Designer.vb` — neconfirmat; fișierul
   e scris de mână, în forma pe care o produce designerul.
4. **Modul «Manual» la design time** e forțat pe 1 (suprafața VS desenează la 96 dpi), deci un
   factor manual **nu** se vede în designer. Intenționat, dar de văzut dacă nu surprinde.
5. `ThemeStyleOptions.ControlPadding` e expus și editabil, dar **motorul nu-l aplică nicăieri**
   astăzi (`PaddingValue` n-are consumator). Se salvează corect; nu se vede. Nu e regresie — era
   deja așa —, dar acum are un buton, deci devine vizibil ca lipsă.
6. Culorile paletei sunt opace prin contract (`ColorHex` scrie `#RRGGBB`): o culoare cu alfa aleasă
   din selector **își pierde alfa** la salvare, tăcut.
7. **Fără migrare a schemelor de utilizator existente**: dacă cineva avea deja un
   `…\Themes\Modern.json` dinainte, de acum el **înlocuiește** built-in-ul în loc să apară ca rând
   separat. E chiar regula cerută, dar e o schimbare de înțeles pentru fișierele vechi.
