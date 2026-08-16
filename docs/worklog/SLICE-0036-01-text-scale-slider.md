# SLICE-0036-01 — Mărimea textului și a controalelor (cursor)

Trecere peste felia 0036, cerută imediat după ea: «se poate prelua și scalarea textului? mă
gândeam că s-ar putea un buton sau un cursor care să permită text și controale mai mari sau mai
mici».

## Deciziile luate înainte de cod (întrebate)

| Întrebare | Alegerea operatorului |
|---|---|
| Cât de departe merge mărirea? | **Fonturi + lăsăm WinForms să redimensioneze** — zoom adevărat, text ȘI controale |
| Unde stă comanda? | Cursor în fereastra de opțiuni **și** un element-cursor nou în meniul `CustomPopup` |

## Cum funcționează (și de ce așa)

**Mărirea nu redimensionează controale cu mâna — o face platforma.** Toate formularele sunt
`AutoScaleMode.Font`. Când li se schimbă fontul, WinForms rulează singur `PerformAutoScale` și
rescalează dreptunghiurile copiilor. Deci e destul să scriem fontul formularului (din baza lui) ca
să crească și literele, și controalele. Peste asta, factorul intră și în `AppScaling.FactorFor`, ca
măsurile pe care le desenăm NOI (rândul din arbore și din grilă, constantele din pictură) să crească
în același pas — altfel textul ar crește într-un rând care nu crește, adică fix defectul reparat în
0035.

Cele două scări se **înmulțesc**, nu se aleg: la 150% pe ecran cu textul pus pe 125%, un rând
trebuie să fie de 1,875 ori cel de la 96 dpi.

**`FontBaseline`** (modul nou) ține fontul de la 100% pentru fiecare control, ca înmulțirea să
pornească mereu din același loc. Nu se refolosește `DesignerBaseline`: acela există ca să PUNĂ LA
LOC fontul din designer — exact ce face «Colorat» — deci mărirea sprijinită pe el ar fi dispărut
tăcut pe o singură schemă. Se ating doar formularele și controalele cu font PROPRIU; unul care
moștenește fontul ambiental nu se atinge, fiindcă scriindu-i-l l-am fixa și l-am rupe de formular
pentru totdeauna (crește oricum, prin moștenire).

Ordinea în `ThemeManager.Apply`: tema întâi, mărirea **la sfârșit**. Invers, `ApplyBaseFont` și
restaurarea «Colorat» ar fi șters mărirea.

### Bugul prins de un test, nu de ochi

Prima formă a lui `FontBaseline` **ghicea** dacă tema rescrisese fontul, comparând REFERINȚA
obiectului scris de noi cu cea de pe control. Nu ține pe un `Form`: fiindcă formularele sunt
`AutoScaleMode.Font`, scrierea fontului declanșează `PerformAutoScale`, iar autoscalarea își face
**propria** instanță de `Font` — deci obiectul de pe control nu mai era niciodată cel scris de noi,
**tocmai în cazul care contează**. Baza se muta la fiecare pas și mărimile se compuneau: 10 → 15 →
**30** în loc de 20.

Leacul nu e o comparație mai deșteaptă, ci renunțarea la ghicit: cele două locuri care scriu
fonturi peste ale noastre — `ThemeManager.ApplyBaseFont` și `DesignerBaseline.Restore` — cheamă
acum `FontBaseline.Rebase`. Semnal explicit, care nu depinde de ce face WinForms în interior.

### Rândul-CURSOR din `CustomPopup`

Al treilea rol al lui `CustomPopupItem`, după rândul obișnuit și separator — steag, nu clasă, din
același motiv ca `IsSeparator`: mai multe tipuri într-o colecție ordonată ar cere un editor de
colecții propriu, deci un assembly de design-time.

Regulile lui, toate din același miez («un cursor se trage, nu se alege»):

- **nu închide meniul** și nu trece prin `ItemClicked`, ci prin `SliderValueChanged` — altfel n-ar
  exista previzualizare, adică chiar rostul lui;
- **n-are literă de acces** — litera ALEGE un rând;
- `Enter` pe el nu face nimic, și asta **nu** e un no-op tăcut, ci refuzul cinstit al unui rând care
  n-a promis niciodată o alegere;
- apăsarea începe tragerea (spre deosebire de rândurile obișnuite, care se aleg la RIDICARE), iar
  cât se trage, mouse-ul e al cursorului: mișcarea nu mai mută evidențierea, nici dacă degetul iese
  din rând;
- săgețile stânga/dreapta mută valoarea cu 5, iar `Home`/`End` sar la capetele ȘINEI — dar numai
  când evidențierea e pe cursor; altfel rămân ale meniului;
- evenimentul se ridică **doar la schimbare reală**: tragerea produce zeci de mesaje pe același
  pixel, iar gazda rescrie fonturile întregii aplicații în handler.

Croiala meniului rezervă etichetă + șină + valoare (o valoare de lățime FIXĂ, ca șina să nu-și
schimbe lungimea când se trece de la «100%» la «95%» — o șină care tresare sub deget e cel mai ușor
fel de a rata pasul dorit). Capetele se pot chiar atinge cu mouse-ul: valoarea se măsoară pe șina
utilă, cea din care s-a scăzut lățimea degetului, fiindcă degetul se desenează centrat pe poziție.

Culorile nu introduc nimic nou: șina nefolosită e culoarea separatorului, partea parcursă și degetul
sunt culoarea de evidențiere, prin aceeași `ThemeShapes.FillModern` ca rândul evidențiat.

### Cele două suprafețe

- **Meniul butonului de temă** — cursorul «Mărime text» în CAP, deasupra schemelor, cu separator.
  Sus fiindcă e singurul rând care nu închide meniul, deci singurul folosit de mai multe ori la
  rând; jos, ar fi trebuit căutat sub o listă care crește cu fiecare schemă salvată. Comutator
  propriu: `ShowTextScaleSlider`.
- **Fereastra de opțiuni** — `TrackBar` 75–200% cu etichetă de procent, în panoul «Scalare».

Meniul nu poate produce niciodată doi separatori alăturați, oricâte grupuri s-ar stinge.

## Fișiere atinse

**Noi**
- `src/KBot.Theming/FontBaseline.vb`
- `src/KBot.Controls/Popup/CustomPopup.Slider.vb`
- `tests/KBot.Controls.Tests/CustomPopupSliderTests.vb`

**Modificate**
- `src/KBot.Theming/AppScaling.vb` — `TextScale`, `SetTextScale`, `ApplyTextScale`, `FactorFor`
  înmulțește cele două scări, `Broadcast` scalează fonturile înaintea măsurilor
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

## Rezultatele testelor

- **Build soluție: 0 erori, 5 `MSB3825` preexistente.**
- **`KBot.Theming.Tests`: 110 verzi** (96 înainte, +14).
- **`KBot.Controls.Tests`: 855 verzi** (834 înainte, +21). Patru teste existente ale meniului au
  fost **rescrise, nu șterse** — cursorul nu e nici schemă, nici rând cu pictogramă, nici purtător
  de literă de acces, iar separatorul lui rămâne când uneltele se sting.
- `KBot.App.Tests` neatins de această trecere (cele 10 căderi preexistente din arborele de lucru).

## Rămas neverificat / amânat

1. **Nimic n-a fost privit pe ecran.** Pentru trecerea asta e mai important ca de obicei: efectul e
   chiar despre cum ARATĂ, iar tot ce urmează sunt lucruri care nu se pot deduce din teste.
2. **Autoscalarea repetată** e locul cel mai probabil de surprize: `PerformAutoScale` se declanșează
   la fiecare pas al cursorului, iar formularele K-BOT sunt puternic andocate (asta ajută) dar au
   `MinimumSize` fixe (asta nu) — la 200% o fereastră poate să nu mai poată fi micșorată destul.
3. **Fereastra de opțiuni se scalează pe ea însăși** în timp ce tragi de cursorul din ea; la fel
   meniul, care își recalculează geometria din mers. Corect ca purtare, de văzut ca senzație.
4. **Fonturile create nu se eliberează**, deliberat (un `Font` poate fi într-o pictură chiar atunci,
   iar copiii care moștenesc împart instanța formularului). Câteva obiecte per schimbare de mărime.
5. **Controalele cu font propriu autorit** se scalează individual — dar dacă un `.Designer.vb` a
   înghețat un font pe care nimeni nu l-a ales (capcana `ShouldSerialize` din regulile casei), acel
   control se va scala ca și cum alegerea ar fi fost intenționată.
6. **Nu s-a exercitat mutarea între monitoare** cu mărire aplicată — DPI-ul și mărirea se înmulțesc,
   iar drumul acela n-a fost parcurs.
7. Formatul valorii din cursorul de meniu e fix «%» — un cursor viitor cu altă unitate va cere o
   proprietate de format, nu ghicit în `SliderValueText`.
