# SLICE-0036 — Tema, scara și cursorul de mărime

Baza 0036 introduce AppScaling și opțiunile; 01 adaugă TextScale și cursorul. 02 înlocuiește aplicarea grea la fiecare pas cu predarea la MouseUp/KeyUp/Leave, păstrând actualizarea ieftină a procentului. Rezultatele testelor citate sunt istorice și nu certifică aspectul sau activarea reală a ferestrelor. Nu se citește ori modifică documentația proiectelor de teste.

- [0036](#slice-0036)
- [0036-01](#slice-0036-01)
- [0036-02](#slice-0036-02)

## SLICE-0036 — Opțiunile temei (fereastră proprie) + scalarea reglabilă <a id="slice-0036"></a>

### Ce s-a cerut

Două lucruri, una singură ca miez:

1. **Toate opțiunile temei într-un formular**, ajuns din butonul de temă al barei de titlu — nu valorile unor controale de pe un ecran anume (aia există deja, e `ThemeEditorForm`), ci **valorile generale ale temei: scalare, spațiere și așa mai departe, pentru fiecare schemă** (Modern, Întunecat, …).
2. **Un comutator pentru scalarea calculată.** Motivul, în cuvintele operatorului: proiectarea se face la 1920×1080 cu 100%, iar rulată la altă scalare «dimensiunea a tot se distorsionează».

### Deciziile luate înainte de cod (întrebate, nu presupuse)

| Întrebare | Alegerea operatorului |
|---|---|
| Ce înseamnă «scalare oprită»? | **Amândouă**: mod tri-stare pentru măsurile K-BOT (Automat / Fix 100% / Manual) **și** un comutator separat «Windows întinde fereastra» (DPI-unaware, cere repornire) |
| Scalarea e a schemei sau a aplicației? | **A aplicației** — în `theme.json`, lângă schema activă |
| Cum se păstrează editarea unei scheme built-in? | **Fișier de suprascriere per schemă** în `…\AVACONT\Themes\<Nume>.json`, încărcat peste cea compilată; «Restaurează implicit» îl șterge |

### Ce s-a schimbat și de ce

#### (a) `AppScaling` — sursa UNICĂ a scării

Până acum formula `DeviceDpi / 96` era scrisă în **trei** locuri: `ThemeShapes.ScaleDpi` (drumul pe care merg ~157 de locuri din pictura controalelor), plus câte un `_dpiScale` în `AdvancedTreeControl` și în `KBotDataView`. Formula era una singură, dar **nu exista niciun loc din care s-o poți schimba**. Modulul nou e acel loc; cele trei drumuri întreabă acum acolo, deci o alegere ajunge peste tot deodată sau nicăieri — niciodată pe jumătate. Asta a și fost condiția de proiectare: o setare aplicată doar arborelui și grilei ar fi fost mai rea decât niciuna.

`Automatic` calculează exact ce se calcula înainte, deci **implicitul e comportamentul dinaintea feliei**, bit cu bit.

Difuzarea (`AppScaling.Broadcast`) merge pe ferestrele deschise și cheamă `IDpiScaledControl.RefreshDpiMetrics` — interfață nouă în `KBot.Theming`, implementată de arbore și de grilă. De ce doar ele: constantele dintr-un `OnPaint` trec prin `ScaleDpi` la **fiecare** pictare, deci le prinde o invalidare; cine își ține înălțimea de rând sau lățimile de coloană într-un **câmp** le-a calculat o dată, la scara de atunci, și trebuie chemat pe nume. Difuzarea **nu** se oprește la `IThemedControl` (spre deosebire de `ThemeManager.Traverse`): scara nu e o culoare, n-are cum să strice un copil intern.

Evenimentul `ScalingChanged` există, dar difuzarea **nu** merge pe el — un eveniment de modul ar fi ținut referințe tari la controale și le-ar fi scurs.

#### (b) Onestitatea celor două comutatoare

`Fix 100%` oprește scalarea măsurilor **NOASTRE**. Fonturile (care sunt în puncte) și `Bounds`-urile controalelor obișnuite le scalează în continuare WinForms prin `AutoScaleMode.Font`, iar acela nu poate fi oprit din afară — deci la 150% textul rămâne mai mare decât geometria din jurul lui. **E compromisul modului, nu o scăpare**, și e scris ca atare atât în cod, cât și pe eticheta din fereastră.

Singurul comutator care dă proporții IDENTICE cu proiectarea e `DpiUnaware`, fiindcă acolo întinde Windows toată fereastra ca bitmap; costul e textul mai moale. De aceea sunt **două** setări.

Nota din felia 0035 spunea că a face aplicația surdă la scalare «ar înrăutăți». Rămâne adevărată ca implicit — de aceea bifa e **stinsă** din start și poartă avertismentul ei; dar operatorul a cerut-o explicit ca opțiune, iar alegerea e a lui.

`Program.Main` citește setarea **înaintea** lui `SetHighDpiMode`, fiindcă modul DPI al unui proces nu se mai poate schimba după prima fereastră — de aici «necesită repornire» din UI.

#### (c) `ThemeOptionsForm` — opțiunile SCHEMEI

Fereastră nouă în `KBot.Controls/ThemeOptions/` (unde stă și `ThemeEditorForm`, aceeași precedență). Un `PropertyGrid` pe categorii, hrănit de `SchemeOptionsProxy`: **cele 23 de sloturi de culoare ale paletei** (ca `Color`, deci cu selectorul de culoare, nu ca șiruri `#RRGGBB`) plus **toate opțiunile de stil** — culori de sistem, păstrează culorile din designer, controale plate, randarea butoanelor, raza colțului, accent pe focus, bară de titlu întunecată, desenează filele, font de bază (listă derulantă cu fonturile instalate), dimensiune font, spațiere internă.

Trei reguli scrise în ea:

- **Alegerea schemei din listă o și ACTIVEAZĂ.** Editorul arată efectul pe ferestrele din spate, deci ce editezi trebuie să fie ce vezi.
- **Efect imediat, salvare explicită.** Fiecare valoare atinsă se vede pe loc (`ThemeManager.Refresh`, nou — difuzează fără să schimbe și fără să persiste schema); pe disc nu ajunge nimic până la «Salvează». Închiderea cu modificări nesalvate întreabă.
- **Nu se confundă cu «Stiluri...».** Rândurile de meniu sunt scrise diferit, iar comentariul din capul fiecărei ferestre spune care face ce.

#### (d) Suprascrierea schemelor built-in

`ThemeManager.AvailableSchemes` nu mai concatenează orbește. Un fișier cu numele unei scheme built-in o **înlocuiește**, păstrându-i poziția în listă. Regula veche ar fi produs două rânduri «Modern» în meniul de teme, iar `ResolveByName` — care întoarce primul potrivit — ar fi ales mereu pe cel **NEeditat**: salvarea ar fi părut că nu face nimic.

`ThemeStore` a primit `SaveScheme` / `DeleteScheme` / `SchemeFilePath` și — important — **toate scrierile în `theme.json` trec acum prin citire-modificare-scriere**: schema activă și scalarea stau în același fișier și se scriu din locuri diferite, deci fără asta una ar fi ștears-o pe cealaltă. Un `theme.json` vechi, fără câmpurile noi, primește implicitele.

#### (e) Meniul butonului de temă

Rând nou «Opțiuni temă...», **înaintea** lui «Stiluri...» (prima e ce caută operatorul în nouă din zece cazuri; a doua e unealta rară). Comutator propriu — `ShowThemeOptions` — deliberat **separat** de `ShowThemeEditor`: cele două unelte fac lucruri diferite, deci o fereastră care o vrea pe una n-are de ce s-o capete și pe cealaltă. `LoginForm`, care stingea deja editorul, stinge acum și opțiunile: înainte de autentificare rămâne un comutator de scheme curat.

Separatorul aparține **grupului** de unelte: se pune o dată dacă rămâne măcar una aprinsă și pleacă doar când se sting amândouă.

### Fișiere atinse

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
- `src/KBot.Theming/ThemeManager.vb` — `MergeSchemes`, `Refresh`, `SaveScheme`, `ResetScheme`, încărcarea scalării în `Initialize`
- `src/KBot.Controls/Tree/AdvancedTreeControl.Dpi.vb`, `src/KBot.Controls/DataView/KBotDataView.Dpi.vb` — scara din `AppScaling`, `IDpiScaledControl`
- `src/KBot.Controls/CaptionBar/KBotCaptionBar.ThemeButton.vb` / `.vb` — rândul + `ShowThemeOptions`
- `src/KBot.App/Program.vb` — `HighDpiMode` după setare
- `src/KBot.App/LoginForm.Designer.vb` — `ShowThemeOptions = False`
- `tests/KBot.Controls.Tests/KBotCaptionBarThemeButtonTests.vb`
- cele trei `.vbproj`: `FileVersion` 1.8.0.0 / 1.27.0.0 / 1.0.19.0

### Rezultatele testelor

- **Build soluție: 0 erori, 5 avertismente `MSB3825`** — exact cele preexistente (resurse `ImageStream` din vederile DDF/ORD/Plăți/Recepții/Rezervări).
- **`KBot.Theming.Tests`: 96 verzi** (71 înainte, +25 noi — scalarea și suprascrierea schemelor).
- **`KBot.Controls.Tests`: 834 verzi** (831 înainte, +3 noi pe meniul butonului de temă). Două teste existente au fost **rescrise, nu șterse**: meniul are acum două rânduri de unealtă, deci numărătoarea de scheme le scoate pe amândouă, iar separatorul nu mai pleacă odată cu «Stiluri...».
- **`KBot.App.Tests`: 151 verzi / 10 căzute.** Toate cele 10 sunt **preexistente**, din lucrul necomis aflat în arborele de lucru (eticheta de navigare `Doc. Fundamentare` → `Fundamentare`, coloana `obs` lipsă din grila Istoric, luna `Martie 2026` → `Martie`, parserul XFA care întoarce gol). **Niciuna nu atinge tema, scalarea sau bara de titlu.** ⚠️ Verificarea s-a făcut **citind mesajele de eșec**, nu printr-o probă `git stash`: cele două fișiere `.Dpi.vb` din felia 0035 sunt încă **neurmărite** (felia aceea n-a fost comisă), deci nu pot fi date deoparte cu `git stash` fără să atingă lucrul operatorului.

### Rămas neverificat / amânat

1. **Nimic n-a fost privit pe ecran** — nici fereastra nouă, nici meniul, nici vreunul dintre modurile de scalare la 100% / 125% / 150%. Riscul standing 0025/0027/0035.
2. **`DpiUnaware` n-a fost pornit niciodată** — calea cere repornirea aplicației și un ecran cu scalare ≠ 100%. Efectul lui asupra ferestrelor fără chenar (`KBotShellForm`, `WM_NCHITTEST` / `WM_GETMINMAXINFO`) e **neexercitat** și e locul cel mai probabil de surprize.
3. **Dus-întorsul prin designerul VS** al lui `ThemeOptionsForm.Designer.vb` — neconfirmat; fișierul e scris de mână, în forma pe care o produce designerul.
4. **Modul «Manual» la design time** e forțat pe 1 (suprafața VS desenează la 96 dpi), deci un factor manual **nu** se vede în designer. Intenționat, dar de văzut dacă nu surprinde.
5. `ThemeStyleOptions.ControlPadding` e expus și editabil, dar **motorul nu-l aplică nicăieri** astăzi (`PaddingValue` n-are consumator). Se salvează corect; nu se vede. Nu e regresie — era deja așa —, dar acum are un buton, deci devine vizibil ca lipsă.
6. Culorile paletei sunt opace prin contract (`ColorHex` scrie `#RRGGBB`): o culoare cu alfa aleasă din selector **își pierde alfa** la salvare, tăcut.
7. **Fără migrare a schemelor de utilizator existente**: dacă cineva avea deja un `…\Themes\Modern.json` dinainte, de acum el **înlocuiește** built-in-ul în loc să apară ca rând separat. E chiar regula cerută, dar e o schimbare de înțeles pentru fișierele vechi.

## SLICE-0036-01 — Mărimea textului și a controalelor (cursor) <a id="slice-0036-01"></a>

Trecere peste felia 0036, cerută imediat după ea: «se poate prelua și scalarea textului? mă gândeam că s-ar putea un buton sau un cursor care să permită text și controale mai mari sau mai mici».

### Deciziile luate înainte de cod (întrebate)

| Întrebare | Alegerea operatorului |
|---|---|
| Cât de departe merge mărirea? | **Fonturi + lăsăm WinForms să redimensioneze** — zoom adevărat, text ȘI controale |
| Unde stă comanda? | Cursor în fereastra de opțiuni **și** un element-cursor nou în meniul `CustomPopup` |

### Cum funcționează (și de ce așa)

**Mărirea nu redimensionează controale cu mâna — o face platforma.** Toate formularele sunt `AutoScaleMode.Font`. Când li se schimbă fontul, WinForms rulează singur `PerformAutoScale` și rescalează dreptunghiurile copiilor. Deci e destul să scriem fontul formularului (din baza lui) ca să crească și literele, și controalele. Peste asta, factorul intră și în `AppScaling.FactorFor`, ca măsurile pe care le desenăm NOI (rândul din arbore și din grilă, constantele din pictură) să crească în același pas — altfel textul ar crește într-un rând care nu crește, adică fix defectul reparat în 0035.

Cele două scări se **înmulțesc**, nu se aleg: la 150% pe ecran cu textul pus pe 125%, un rând trebuie să fie de 1,875 ori cel de la 96 dpi.

**`FontBaseline`** (modul nou) ține fontul de la 100% pentru fiecare control, ca înmulțirea să pornească mereu din același loc. Nu se refolosește `DesignerBaseline`: acela există ca să PUNĂ LA LOC fontul din designer — exact ce face «Colorat» — deci mărirea sprijinită pe el ar fi dispărut tăcut pe o singură schemă. Se ating doar formularele și controalele cu font PROPRIU; unul care moștenește fontul ambiental nu se atinge, fiindcă scriindu-i-l l-am fixa și l-am rupe de formular pentru totdeauna (crește oricum, prin moștenire).

Ordinea în `ThemeManager.Apply`: tema întâi, mărirea **la sfârșit**. Invers, `ApplyBaseFont` și restaurarea «Colorat» ar fi șters mărirea.

#### Bugul prins de un test, nu de ochi

Prima formă a lui `FontBaseline` **ghicea** dacă tema rescrisese fontul, comparând REFERINȚA obiectului scris de noi cu cea de pe control. Nu ține pe un `Form`: fiindcă formularele sunt `AutoScaleMode.Font`, scrierea fontului declanșează `PerformAutoScale`, iar autoscalarea își face **propria** instanță de `Font` — deci obiectul de pe control nu mai era niciodată cel scris de noi, **tocmai în cazul care contează**. Baza se muta la fiecare pas și mărimile se compuneau: 10 → 15 → **30** în loc de 20.

Leacul nu e o comparație mai deșteaptă, ci renunțarea la ghicit: cele două locuri care scriu fonturi peste ale noastre — `ThemeManager.ApplyBaseFont` și `DesignerBaseline.Restore` — cheamă acum `FontBaseline.Rebase`. Semnal explicit, care nu depinde de ce face WinForms în interior.

#### Rândul-CURSOR din `CustomPopup`

Al treilea rol al lui `CustomPopupItem`, după rândul obișnuit și separator — steag, nu clasă, din același motiv ca `IsSeparator`: mai multe tipuri într-o colecție ordonată ar cere un editor de colecții propriu, deci un assembly de design-time.

Regulile lui, toate din același miez («un cursor se trage, nu se alege»):

- **nu închide meniul** și nu trece prin `ItemClicked`, ci prin `SliderValueChanged` — altfel n-ar exista previzualizare, adică chiar rostul lui;
- **n-are literă de acces** — litera ALEGE un rând;
- `Enter` pe el nu face nimic, și asta **nu** e un no-op tăcut, ci refuzul cinstit al unui rând care n-a promis niciodată o alegere;
- apăsarea începe tragerea (spre deosebire de rândurile obișnuite, care se aleg la RIDICARE), iar cât se trage, mouse-ul e al cursorului: mișcarea nu mai mută evidențierea, nici dacă degetul iese din rând;
- săgețile stânga/dreapta mută valoarea cu 5, iar `Home`/`End` sar la capetele ȘINEI — dar numai când evidențierea e pe cursor; altfel rămân ale meniului;
- evenimentul se ridică **doar la schimbare reală**: tragerea produce zeci de mesaje pe același pixel, iar gazda rescrie fonturile întregii aplicații în handler.

Croiala meniului rezervă etichetă + șină + valoare (o valoare de lățime FIXĂ, ca șina să nu-și schimbe lungimea când se trece de la «100%» la «95%» — o șină care tresare sub deget e cel mai ușor fel de a rata pasul dorit). Capetele se pot chiar atinge cu mouse-ul: valoarea se măsoară pe șina utilă, cea din care s-a scăzut lățimea degetului, fiindcă degetul se desenează centrat pe poziție.

Culorile nu introduc nimic nou: șina nefolosită e culoarea separatorului, partea parcursă și degetul sunt culoarea de evidențiere, prin aceeași `ThemeShapes.FillModern` ca rândul evidențiat.

#### Cele două suprafețe

- **Meniul butonului de temă** — cursorul «Mărime text» în CAP, deasupra schemelor, cu separator. Sus fiindcă e singurul rând care nu închide meniul, deci singurul folosit de mai multe ori la rând; jos, ar fi trebuit căutat sub o listă care crește cu fiecare schemă salvată. Comutator propriu: `ShowTextScaleSlider`.
- **Fereastra de opțiuni** — `TrackBar` 75–200% cu etichetă de procent, în panoul «Scalare».

Meniul nu poate produce niciodată doi separatori alăturați, oricâte grupuri s-ar stinge.

### Fișiere atinse

**Noi**
- `src/KBot.Theming/FontBaseline.vb`
- `src/KBot.Controls/Popup/CustomPopup.Slider.vb`
- `tests/KBot.Controls.Tests/CustomPopupSliderTests.vb`

**Modificate**
- `src/KBot.Theming/AppScaling.vb` — `TextScale`, `SetTextScale`, `ApplyTextScale`, `FactorFor` înmulțește cele două scări, `Broadcast` scalează fonturile înaintea măsurilor
- `src/KBot.Theming/ThemeManager.vb` — mărirea la sfârșitul lui `Apply`; `Rebase` în `ApplyBaseFont`
- `src/KBot.Theming/DesignerBaseline.vb` — `Rebase` după restaurarea fontului
- `src/KBot.Theming/ThemeStore.vb` — câmpul `textScale`
- `src/KBot.Controls/Popup/CustomPopupItem.vb` — rolul de cursor
- `src/KBot.Controls/Popup/CustomPopup.vb` / `.Painting.vb` / `.Input.vb`
- `src/KBot.Controls/CaptionBar/KBotCaptionBar.ThemeButton.vb` / `.vb`
- `src/KBot.Controls/ThemeOptions/ThemeOptionsForm.vb` / `.Designer.vb`
- `tests/KBot.Theming.Tests/AppScalingTests.vb`
- `tests/KBot.Controls.Tests/KBotCaptionBarThemeButtonTests.vb`
- `.vbproj`: Theming `1.9.0.0`, Controls `1.28.0.0`

### Rezultatele testelor

- **Build soluție: 0 erori, 5 `MSB3825` preexistente.**
- **`KBot.Theming.Tests`: 110 verzi** (96 înainte, +14).
- **`KBot.Controls.Tests`: 855 verzi** (834 înainte, +21). Patru teste existente ale meniului au fost **rescrise, nu șterse** — cursorul nu e nici schemă, nici rând cu pictogramă, nici purtător de literă de acces, iar separatorul lui rămâne când uneltele se sting.
- `KBot.App.Tests` neatins de această trecere (cele 10 căderi preexistente din arborele de lucru).

### Rămas neverificat / amânat

1. **Nimic n-a fost privit pe ecran.** Pentru trecerea asta e mai important ca de obicei: efectul e chiar despre cum ARATĂ, iar tot ce urmează sunt lucruri care nu se pot deduce din teste.
2. **Autoscalarea repetată** e locul cel mai probabil de surprize: `PerformAutoScale` se declanșează la fiecare pas al cursorului, iar formularele K-BOT sunt puternic andocate (asta ajută) dar au `MinimumSize` fixe (asta nu) — la 200% o fereastră poate să nu mai poată fi micșorată destul.
3. **Fereastra de opțiuni se scalează pe ea însăși** în timp ce tragi de cursorul din ea; la fel meniul, care își recalculează geometria din mers. Corect ca purtare, de văzut ca senzație.
4. **Fonturile create nu se eliberează**, deliberat (un `Font` poate fi într-o pictură chiar atunci, iar copiii care moștenesc împart instanța formularului). Câteva obiecte per schimbare de mărime.
5. **Controalele cu font propriu autorit** se scalează individual — dar dacă un `.Designer.vb` a înghețat un font pe care nimeni nu l-a ales (capcana `ShouldSerialize` din regulile casei), acel control se va scala ca și cum alegerea ar fi fost intenționată.
6. **Nu s-a exercitat mutarea între monitoare** cu mărire aplicată — DPI-ul și mărirea se înmulțesc, iar drumul acela n-a fost parcurs.
7. Formatul valorii din cursorul de meniu e fix «%» — un cursor viitor cu altă unitate va cere o proprietate de format, nu ghicit în `SliderValueText`.

## SLICE-0036-02 — Cursorul aplică la SFÂRȘITUL gestului, nu la fiecare pas <a id="slice-0036-02"></a>

Corecție cerută de operator după prima probă a cursorului din 0036-01:

> «apply-ul pe cursor ar trebui făcut pe mouse up sau pe un eveniment care se declanșează după ce
> termin de mutat cursorul. așa cum e acum, nu pot face nimic pentru că aplică totul la orice
> schimbare, deci popup-ul dispare»

### Ce era greșit

`SliderValueChanged` se ridica la fiecare pixel al tragerii, iar bara de titlu rescria din el fonturile ÎNTREGII aplicații. Două urmări:

1. **De nefolosit ca viteză** — zeci de reașezări de ferestre pe secundă.
2. **Meniul dispărea**, ceea ce e mai rău și e chiar plângerea. Cauza nu e evidentă din cod și merită scrisă: aplicarea mărimii reașază toate ferestrele deschise, fereastra de dedesubt se REACTIVEAZĂ, iar `CustomPopup` se închide singur pe `Deactivate` — comportamentul corect al oricărui meniu la un clic în afară, declanșat aici de propria noastră comandă. Cursorul se sabota singur la prima mișcare.

Diagnosticul contează pentru viitor: **orice** handler de meniu care mută ferestre va lovi aceeași capcană.

### Ce s-a schimbat

**Un al doilea eveniment, cu o împărțire clară a muncii.** `SliderValueChanged` rămâne, la fiecare pas, pentru ce e IEFTIN (cifra de pe șină, desenată de meniu). Nou: `SliderValueCommitted`, ridicat **o singură dată, la sfârșitul gestului** — ridicarea butonului de mouse sau a tastei — și doar dacă valoarea chiar s-a schimbat față de începutul gestului (o apăsare care n-a mișcat nimic nu e o comandă). Acolo se pune lucrul greu. Cele două suprafețe consumatoare au trecut pe el.

**Tastatura are propriul gest.** Săgețile se REPETĂ cât ții tasta apăsată, deci predarea la fiecare apăsare ar fi comandat lucrul greu de zeci de ori — exact ce făcea mouse-ul. `OnKeyUp` încheie gestul; apăsările dintre timp doar mută valoarea.

**Garda de `Deactivate`.** Cât ține predarea, meniul nu se mai închide pe pierderea activării: aceea vine din propria comandă, nu dintr-un clic al operatorului. După predare își ia activarea înapoi, ca să rămână folosibil pentru pasul următor. Steagul se coboară într-un `Finally` — lăsat ridicat de o excepție, meniul n-ar mai putea fi închis printr-un clic în afară și ar părea agățat pe ecran.

**`TrackBar`-ul din fereastra de opțiuni** a primit aceeași regulă (`MouseUp` / `KeyUp` / `Leave`), din al doilea motiv: fereastra de opțiuni se rescala pe ea însăși în timpul tragerii, deci cursorul se plimba sub deget. Eticheta cu procentul rămâne live.

### Fișiere atinse

- `src/KBot.Controls/Popup/CustomPopup.Slider.vb` — `SliderValueCommitted`, `CommitSlider`, gestul de tastatură, `IsCommittingSlider`, cusătura de test `TestDeactivate`
- `src/KBot.Controls/Popup/CustomPopup.vb` — garda din `OnDeactivate`
- `src/KBot.Controls/Popup/CustomPopup.Input.vb` — `OnKeyUp`
- `src/KBot.Controls/CaptionBar/KBotCaptionBar.ThemeButton.vb` — abonare la `Committed`
- `src/KBot.Controls/ThemeOptions/ThemeOptionsForm.vb` — aplicare la sfârșitul gestului
- `tests/KBot.Controls.Tests/CustomPopupSliderTests.vb` — șase teste noi
- `src/KBot.Controls/KBot.Controls.vbproj` — `FileVersion` 1.29.0.0

### Rezultatele testelor

- Build soluție **0 erori**, 5 `MSB3825` preexistente.
- **`KBot.Controls.Tests`: 861 verzi** (855 înainte, +6): tragerea previzualizează la fiecare pas dar predă o singură dată; o apăsare fără mișcare nu predă; săgețile predau la ridicarea tastei; predarea fără gest nu face nimic; meniul NU se închide când gazda reașază ferestrele; o dezactivare obișnuită îl închide, ca înainte.
- `KBot.Theming.Tests` neatins (110 verzi).

Nota de test: un meniu care n-a ajuns pe ecran se închide prin `Dispose` fără să ridice `FormClosed` (vezi comentariul din `CustomPopup.OnFormClosed`), deci probele citesc `IsDisposed`.

### Rămas neverificat / amânat

1. **Tot nevăzut pe ecran.** Diagnosticul («meniul se închide fiindcă pierde activarea») vine din citirea codului plus raportul operatorului, iar garda e ținută de un test care SIMULEAZĂ dezactivarea prin cusătura `TestDeactivate` — nu de o schimbare reală de activare între două ferestre.
2. `Activate()` de după predare n-a fost exercitat cu o fereastră adevărată; dacă gazda ridică o fereastră modală din handler, ordinea poate fi alta.
3. Rămân deschise punctele din 0036-01 (autoscalarea repetată, `MinimumSize` fixe la 200%).
