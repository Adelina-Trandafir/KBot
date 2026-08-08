# SLICE-0025-07 — `KBotNavList`: eticheta plutitoare a barei strânse

Pasul 7 al feliei 0025 (nu confunda cu «PASUL 7» din planul original = migrarea `MainForm.navViews`;
numerotarea `-0x` a sub-feliilor merge separat de pașii planului). Vezi
`SLICE-0025-05-navlist-collapse-padding.md` pentru strângere și
`SLICE-0025-06-navlist-collapse-button-icons.md` pentru butonul din colț. A patra cerere de
operator în afara planului:

> «i removed the complete collapse. only icons or expanded. what i need is when is collapsed and i
> hover any navbar button (except for the sep) to have a sort of tooltip with the entire button.
> but it mustn't be a real tooltip (yellow and ugly), but some kind of floating control which takes
> on all the props of the hovered button and displays it on a small timer from left to right (if
> the navbar is vertical - not yet sure for the horizontal)»

---

## 1. Ce s-a schimbat și de ce

### 1.1 Nu e un `ToolTip` — e butonul care se desfășoară

Fișier nou: `src/KBot.Controls/NavList/KBotNavFlyout.vb` (fereastra + `KBotNavFlyoutStyle`).

Cheia întregii felii e **de unde pleacă** eticheta. Nu se așază lângă buton (aia ar fi tot un
tooltip, doar mai frumos): pleacă **exact din dreptunghiul butonului strâns** și crește doar spre
dreapta, iar pictograma se desenează **centrată în prima bandă**, adică fix peste locul unde bara
o pictează deja. Ce se vede e un buton care se **desfășoară**, cu pictograma nemișcată și textul
ieșind de sub ea — nu două pictograme una lângă alta. Asta rezolvă și ambiguitatea din cerere
(«the entire button» vs. «doar ce s-a tăiat»): se desenează butonul ÎNTREG, dar suprapus peste el
însuși, deci nimic nu apare de două ori.

La progres `0` eticheta e **nedeosebită** de buton (test dedicat); la `1` are lățimea completă.

Preia toate proprietățile butonului de sub cursor: pictograma (estompată la fel, prin aceeași
`KBotNavList.DrawItemImage`, devenită `Friend`), textul, pastila de badge, fundalul de selecție
(`_selectedFill`) sau de hover, culoarea de accent + fontul semibold pe cel selectat, textul
estompat pe cel dezactivat, conturul din `BorderColor` și raza de colț a schemei. Regulile de
culoare **NU** sunt rescrise în fereastră: bara le calculează (`BuildFlyoutStyle`) exact ca în
`OnPaint` și i le predă gata făcute în `KBotNavFlyoutStyle`, ca logica de temă să nu ajungă în două
locuri.

### 1.2 O fereastră care nu se bagă în seamă

`WS_EX_NOACTIVATE` + `ShowWithoutActivation` (nu fură focusul), `WS_EX_TOOLWINDOW` (fără buton de
bară de activități), `AutoScaleMode.None` (bara dă px deja scalați; o a doua ajustare ar dezalinia
eticheta de butonul din care trebuie să pară că iese), `Region` tăiată din calea rotunjită (altfel
în colțuri s-ar vedea dreptunghiul ferestrei peste vederea de dedesubt).

**`HTTRANSPARENT` pe `WM_NCHITTEST` — nu e cosmetică.** Eticheta acoperă chiar butonul peste care
stă cursorul. Fără click-through, apariția ei ar fi însemnat «mouse-ul a părăsit bara» ⇒ eticheta
se ascunde ⇒ mouse-ul e iar pe bară ⇒ eticheta apare… la infinit. Cu `HTTRANSPARENT` mouse-ul trece
prin ea la bară, hover-ul se păstrează, și click-ul pe pictogramă ajunge tot la buton.

### 1.3 Cronometrul

Două proprietăți, amândouă în grilă:

| Proprietate | Implicit | Ce face |
|---|---|---|
| `CollapsedFlyout` | **True** | poarta capabilității |
| `FlyoutDelay` | 250 ms | cât stă cursorul înainte să iasă eticheta (0 = imediat) |
| `FlyoutSlideDuration` | 120 ms | cât durează desfășurarea (0 = fără animație) |

`CollapsedFlyout` e **True implicit**, spre deosebire de `Collapsible` (care e False). Nu e o
inconsecvență: poarta rămâne `Collapsible` — dacă bara nu se strânge, eticheta nu există. Dar o
bară STRÂNSĂ fără etichete e nefolosibilă (nu mai scrie nimic pe butoane), deci implicitul care
respectă intenția e «pornit». Negativele se aduc la 0, ca la `IconSize`/`ItemWidth`/`ItemPadding`.

Pasul animației e fix, 15 ms; progresul avansează `15 / FlyoutSlideDuration` pe tic și se oprește
exact la `1.0`.

### 1.4 Cine primește etichetă

Cere: bară strânsă la `Icons`, `CollapsedFlyout = True`, și sub cursor un **buton vizibil cu text**.
Săriți: separatorii (cerut explicit), butoanele ascunse, butoanele **fără text** (eticheta n-ar
dezvălui nimic peste pictograma deja vizibilă), și butonul de strângere din colț (are prioritate).

**Butoanele DEZACTIVATE primesc etichetă**, spre deosebire de hover-ul obișnuit care le sare:
tocmai pe o bară strânsă e cel mai greu de ghicit ce e butonul stins de sub cursor. Se desenează
stins, cu culorile lui.

Eticheta se retrage la: părăsirea barei, trecerea pe alt buton (repornește de acolo), trecerea pe
butonul din colț, **orice invalidare de layout** (add/remove, vizibilitate, `ItemPadding`,
orientare, resize, strângere/desfășurare — ține minte un index și un dreptunghi din layout-ul
vechi, deci se retrage, nu se recalculează), și `CollapsedFlyout = False`. Mișcarea ÎN INTERIORUL
aceluiași buton nu repornește nimic (altfel temporizarea s-ar reseta la fiecare pixel și eticheta
n-ar ieși niciodată). Se reîmprospătează, fără să se retragă, la schimbarea selecției (click pe
butonul de sub etichetă) și la schimbarea temei.

### 1.5 Bara orizontală — întrebarea se închide singură

Operatorul a lăsat-o deschisă («not yet sure for the horizontal»). Nu e nevoie de o decizie:
`IconsCollapseAvailable` întoarce False pe orizontală, iar în `Complete` `RecalcLayout` iese din
start și **niciun** element nu primește slot. Deci pe o bară orizontală **nu există stare strânsă
cu butoane de survolat** — eticheta nu se poate declanșa, prin construcție, nu prin gardă.

---

## 2. Fișiere atinse

| Fișier | Ce |
|---|---|
| `src/KBot.Controls/NavList/KBotNavFlyout.vb` | **NOU** — fereastra + `KBotNavFlyoutStyle` |
| `src/KBot.Controls/NavList/KBotNavList.vb` | `CollapsedFlyout`/`FlyoutDelay`/`FlyoutSlideDuration`, geometria (`FlyoutFullWidth`/`FlyoutClientBounds`), decizia (`FlyoutTargetAt`), mașina de stări + cele două cronometre, cârlige în `OnMouseMove`/`OnMouseLeave`/`InvalidateLayout`/`SelectIndex`/`ApplyTheme`/`Dispose`, `DrawItemImage` `Private`→`Friend`, 10 cârlige `Friend` de test |
| `src/KBot.Controls/KBot.Controls.vbproj` | `FileVersion` 1.5.0.0 → **1.6.0.0** |
| `src/KBot.Controls/README.md` | tabelul de foldere: `NavList/` are acum și fereastra |
| `tests/KBot.Controls.Tests/KBotNavListFlyoutTests.vb` | **NOU** — 21 de teste |

Nimic în `MainForm`/`DdfView`: s-a cerut capacitatea. `MainForm.navViews` n-are nici azi
`Collapsible = True`, deci nu se schimbă nimic în aplicație.

---

## 3. Rezultate

- Build `KBot.sln`: **0 erori / 0 avertismente**.
- `dotnet test KBot.sln`: **843 passed / 0 failed / 0 skipped** (819 înainte; `KBot.Controls.Tests`
  342 → 366 — 21 de teste noi de etichetă + 3 din felia trecută recontorizate pe fișier nou).
- Ce fixează testele: implicitele; bara desfășurată nu scoate etichetă; separator / buton ascuns /
  buton fără text nu primesc; butonul DEZACTIVAT primește; la progres 0 eticheta e **exact**
  butonul; crește doar spre dreapta, păstrând `Left`/`Top`/`Height`; lățimea completă = bandă +
  text + aer (+ pastilă); progresul e limitat la [0,1]; temporizare → desfășurare → oprire fix la
  1.0; `FlyoutDelay = 0` și `FlyoutSlideDuration = 0`; negativele se aduc la 0; mișcarea în același
  buton nu repornește, pe altul repornește; ieșirea de pe bară, butonul din colț, mutarea
  sloturilor și desfășurarea retrag eticheta; fereastra se pictează fără excepție pe butonul
  selectat-cu-pastilă și pe cel stins; bara nu eliberează pictograma primită.

---

## 4. Ce rămâne neverificat / amânat

- **Nimic nu s-a văzut pe ecran** — al patrulea pas la rând în situația asta. Fereastra nu se arată
  headless (`RenderFlyout` se retrage când bara n-are handle și n-are formular părinte), deci ce e
  probat e DECIZIA și GEOMETRIA, nu pixelii. Nedovedite: că fereastra apare unde crede bara, că
  `HTTRANSPARENT` chiar lasă mouse-ul să treacă la butonul de dedesubt (dacă NU, simptomul e o
  etichetă care pâlpâie la infinit — vezi §1.2), că `WS_EX_NOACTIVATE` chiar nu fură focusul, cum
  arată desfășurarea de 120 ms, și dacă `Region` taie colțurile curat sau zimțat.
- **Fără clamp la marginea ecranului.** Eticheta crește spre dreapta din poziția butonului; dacă
  bara strânsă ar fi lipită de marginea DREAPTĂ a ecranului, eticheta ar ieși în afară. Nu s-a pus
  gardă: o bară de navigație strânsă e ancorată la stânga formularului prin construcție
  (`MainForm.navViews` e `Dock.Left`). Limitare consemnată, nu rezolvată.
- **Fereastra nu urmărește formularul.** Dacă formularul se mută sau se redimensionează cât timp
  eticheta e afară, ea rămâne în coordonatele de ecran vechi până la următoarea mișcare de mouse.
  În practică nu se poate muta un formular fără să miști mouse-ul de pe bară, dar o mutare
  programatică ar arăta-o.
- **Fără tastatură.** Navigarea cu Sus/Jos pe o bară strânsă NU scoate etichetă (eticheta e legată
  de hover, nu de selecție). N-a fost cerut, dar e golul evident dacă bara strânsă trebuie folosită
  fără mouse.
- **Fără întârziere la retragere.** Eticheta dispare instantaneu la ieșirea de pe buton; nu se
  desfășoară înapoi. Animația e într-un singur sens, cum s-a cerut («from left to right»).
- Firele (a)–(l) ale feliilor 0025 și 0026 rămân toate deschise.
