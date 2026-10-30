# SLICE-0040 — Arborele, trecut PRIN TOT cu sistemul de scalare

## Ce s-a cerut

«Analizează arborele și asigură-te că sistemul de scalare e folosit peste tot. Peste tot!»

0035 scalase măsurile proprii (`ItemHeight`, `HeaderHeight`, iconițe), iar 0039 marginile `.Paddings`. 0040 citește sistematic toate cele 31 fișiere `Tree/`, căutând pixeli bruți în `AdvancedTreeControl`.

Metodă reproductibilă: numărarea apelurilor `SX(`/`SY(`/`ScaleDpi(` pe fișier. Erau 14 în `.Properties`/`.Paddings`/`.Dpi`, dar numai 0 sau 1 în pictură/așezare/hit-test: scara era definită și aproape nefolosită.

## Ce era stricat (verificat, citind fișierele)

Toate defectele de mai jos sunt aceeași boală, cea descrisă în `.Dpi.vb`: la 150% fontul crește singur (e în puncte), geometria din jurul lui nu.

1. **TreeListView nescalat integral:** COLUMN_HEADER_HEIGHT=24, MIN_CAPTION_WIDTH=120 și ColumnDef.Width (XML/gazdă, ex. MainForm.ConfigureListMode) erau brute în pictură/așezare/hit-test. La 150%, antetul rămânea 24 px sub fontul crescut cu 50%, coloanele de 96 dpi tăiau textul. Bulina «filtru activ» avea formule separate în .Painting/GetColFilterIndicatorRect, ambele cu 13/8 bruți.
2. **Banda de căutare.** `Math.Max(_itemHeight + 8, Font.Height + 10)` — cei doi termeni cresc, aerul dintre ei nu; plus `Math.Max(40, …)` (lățimea minimă a casetei), `+ 4` (spațiul după etichetă), `+ 2` (aerul casetei) și `CLEAR_BTN_WIDTH = 18` (glifa ✕ desenată de noi).
3. **Bara de derulare:** SystemInformation.VerticalScrollBarWidth reflectă DPI-ul de pornire, nu monitorul curent: la altă scalare, bara depășea locul rezervat sau lăsa spațiu gol, tăind textul ori lăsând o dungă. Corect: GetVerticalScrollBarWidthForDpi.
4. **Mărunțișurile din pictura rândului:** raza casetei de bifă (`3`), grosimea semnului de bifă și a liniuței indeterminate (`2.0F`), marginea semnelor `+`/`−` din expander (`2`), cercul și textul loaderului (`14`, `2`, `20`), aerul din interiorul unei celule (`4`/`8`), umflarea și raza plajei de sub un buton survolat (`BUTTON_HOVER_INFLATE`/`_RADIUS`), aerul butonului de strângere din subsol (`- 4`) și grosimea unghiului lui.
5. **SelectionCornerRadius**, logic 1..4 din schemă, rămânea nescalată în rând și TreeNodeFlyout: la 150%, colțul Modern se pierdea pe banda cu 50% mai înaltă.
6. **Cele două ferestre proprii ale arborelui erau nescalate integral.**
   - TooltipPopup (noduri inclusiv TABEL): aer 4, colțuri 6, max 400, decalaj cursor 16/20, spațiu între rânduri 2; toate măsurile XML RowHeight/CellPaddingH/V/MaxWidth/Width și MIN_AUTO_COL_WIDTH nescalate. Corectat comentariul fals «px deja scalați DPI» de lângă lățimea coloanei.
   - `ColFilterPopup` (meniul de filtrare pe coloană): fereastră construită în cod, cu `230`, `24`, `218`, `6`, `28` scriși direct. Nimic n-o scala în locul nostru.
7. **AdvancedTreeControl.NodeInspector** raporta SelectionBounds prin vechea formulă `gridLeft + m_ExpanderSize * 2 - 3`, anterioară marginilor de designer, cu lățimea oprită la PaddingTreeEndPx în loc de -1: altă selecție decât DrawSelection.

## Ce s-a făcut

Aceeași regulă ca în 0039, aplicată mai departe: **cine pictează, așază sau face hit-test citește varianta `…Px`; valoarea logică rămâne pentru designer, pentru XML, pentru serializare și pentru teste.** Scara vine, ca peste tot, din `AppScaling` (deci și modul «fix 100%» al operatorului e respectat), nu dintr-un `DeviceDpi` citit local.

- **.Paddings.vb:** secțiune nouă «MĂRUNȚIȘURILE DE PICTURĂ», 14 constante logice + …Px pentru celule, filtru, căutare, subsol, loader, bifă și expander. Conform contractului, toate spațierile se caută aici; fără proprietăți noi de designer/serializare.
- **`AdvancedTreeControl.vb`** — `ColumnHeaderHeightPx`, `MinCaptionWidthPx` și `ColWidthPx(cd As ColumnDef)`; cele cinci locuri de geometrie de coloană trecute pe ele.
- **`.Properties.vb`** — `ScrollBarThicknessPx` (prin `GetVerticalScrollBarWidthForDpi`), folosit în cele trei locuri care scriau lățimea barei; `SearchClearButtonWidth` scalează acum glifa proprie (o IMAGINE a operatorului rămâne la mărimea ei — pictogramele se aleg, nu se măresc).
- **`.Theming.vb`** — `SelectionCornerRadiusPx`.
- **`.Painting` / `.Search` / `.Footer` / `.ButtonHover`** — toate locurile din lista de mai sus. În antetul de coloane, bulina de filtru a fost adusă pe **aceeași** formulă ca hit-testul.
- **.Popup/.Popup.Table:** PADDING_H/PADDING_V/MAX_WIDTH/CORNER_RADIUS devin accesorii scalate cu aceleași nume; 12 utilizări neatinse. XML se scalează la măsurare, iar PaintRow folosește aceleași margini, ca textul să încapă în celula măsurată.
- **`.ColFilter`** — măsurile ferestrei mutate în constante logice + `SP()`.
- **`.NodeInspector`** — formula de selecție adusă pe cea reală din `DrawSelection`.

## Fișiere atinse

```
src/KBot.Controls/Tree/AdvancedTreeControl.vb
src/KBot.Controls/Tree/AdvancedTreeControl.Paddings.vb
src/KBot.Controls/Tree/AdvancedTreeControl.Properties.vb
src/KBot.Controls/Tree/AdvancedTreeControl.Theming.vb
src/KBot.Controls/Tree/AdvancedTreeControl.Painting.vb
src/KBot.Controls/Tree/AdvancedTreeControl.Search.vb
src/KBot.Controls/Tree/AdvancedTreeControl.Footer.vb
src/KBot.Controls/Tree/AdvancedTreeControl.Header.vb        (verificat — era deja curat)
src/KBot.Controls/Tree/AdvancedTreeControl.ButtonHover.vb
src/KBot.Controls/Tree/AdvancedTreeControl.Overrides.vb
src/KBot.Controls/Tree/AdvancedTreeControl.NodeInspector.vb
src/KBot.Controls/Tree/AdvancedTreeControl.Popup.vb
src/KBot.Controls/Tree/AdvancedTreeControl.Popup.Table.vb
src/KBot.Controls/Tree/AdvancedTreeControl.ColFilter.vb
src/KBot.Controls/KBot.Controls.vbproj                       (FileVersion 1.31 → 1.32)
```

Verificate și găsite deja corecte, fără modificări: `.Header.vb` (trecuse integral prin 0039), `TreeNodeFlyout.vb` (primește px deja scalați și are `AutoScaleMode.None` tocmai ca să nu-i mai ajusteze nimeni), `.Keyboard.vb`, `.ButtonTips.vb` (lucrează în coordonate de ecran), `.API.vb`, `.ListMode.vb`, `.DesignerNodes.vb`, `.Popup.Branching.vb`, `ColumnDef.vb`.

## Rezultatele probelor

- `dotnet build KBot.sln` — **0 erori**, 5 avertismente `MSB3825` **preexistente** (resx-uri `BinaryFormatter` din `KBot.App`, fără legătură cu felia).
- `dotnet test tests/KBot.Controls.Tests` — **911 verzi**, 0 eșecuri. Suita rulează headless, deci la scara 1: e o plasă de non-regresie (nimic nu s-a mutat la 100%), **nu** o dovadă că valorile scalate sunt cele bune.

## Rămâne nefăcut / neverificat

1. **Nimic nu a fost văzut pe ecran** — nici la 100%, nici la 150%, nici pe două monitoare cu scalări diferite. Ca la 0035/0036: felia e despre cum ARATĂ, iar dovada lipsește.
2. **Fără teste noi.** Ar merita cel puțin: `ColWidthPx` la scară 1,5; bulina de filtru desenată și hit-testată în același dreptunghi; `_searchBarHeight` la scară; `ScrollBarThicknessPx`. Suita headless nu le poate produce fără o cusătură care să forțeze scara — nu s-a adăugat una.
3. **Modul TreeListView schimbă înțeles pentru gazde.** `ColumnDef.Width` era, de facto, în pixeli de ecran; de acum e LOGIC. La 100% nimic nu se schimbă, dar orice gazdă care își calculase lățimile pornind de la lățimea reală a controlului le va vedea altfel la 150%. Singurul apelant din soluție e `MainForm.ConfigureListMode`, care le are scrise ca numere fixe — deci corect — dar n-a fost rulat.
4. **Ferestrele proprii la mutarea între monitoare.** `TooltipPopup` și `ColFilterPopup` își citesc scara o dată, la construcție/măsurare. Sunt de scurtă durată, deci e acceptabil, dar o etichetă deschisă în clipa trecerii pe alt monitor va rămâne la scara veche.
5. **Nu s-a comis nimic** — arborele de lucru conține WIP masiv, nelegat, din feliile 0038/0039 (vezi mai jos), iar regula casei interzice adunarea lor într-un singur commit.
6. ⚠️ **Contradicție de registru, nerezolvată de mine.** Codul se referă în comentarii la «felia 0038» (culoarea/grosimea separatorilor) și «felia 0039» (marginile scalate), dar `KBOT_STATUS.md` nu are rânduri pentru ele și încă declară «Next free slice number: 0038». Am luat **0040** pentru felia asta, ca să nu suprascriu ce e deja în cod, dar rândurile 0038 și 0039 lipsesc din registru și **nu le pot scrie eu** — nu știu ce anume s-a promis acolo. De completat de cine le-a făcut.
