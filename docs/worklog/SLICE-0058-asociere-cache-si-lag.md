# SLICE 0058 — Așezarea recepțiilor: cache la descărcare, buton de golire, tăcere pe dată, lag pe benzi

Șapte sesizări ale operatorului, toate din 09.09.2026, toate pe drumul
«descărcare FOREXE ▸ propunere ▸ `AsociereForm` ▸ salvare».

---

## 1. Datele descărcate rămân în memorie și se pot refolosi

**Ce se întâmpla.** Fiecare apăsare pe iconița de descărcare pornea robotul de la
zero — browser, token, tot fluxul, minute bune. Dar a doua apăsare pe același
angajament e aproape întotdeauna o RELUARE (s-a închis formularul fără salvare, s-a
greșit ceva la așezat), nu o cerere de date mai proaspete.

**Ce s-a făcut.** `WorkflowResultStore` ținea deja pachetele în memorie
(`_ultimulNod`) — nu s-a inventat niciun cache nou, s-a folosit cel care exista.
S-au adăugat:

- `MomentLista` — când s-a descărcat lista (pachetele de nod își poartă singure
  `Moment`);
- `NumaraRanduri(pachet)` — rânduri, nu tabele (fluxul întoarce cele cinci tabele ale
  lui și când toate sunt goale, deci `Tabele.Count` ar răspunde 5 pe un pachet gol);
- `PachetBunDeRefolosit(cod)` — pachetul, DAR numai dacă e bun: există și are măcar un
  rând. Asta e «doar dacă sunt corect descărcate»: în depozit intră doar ce a raportat
  succes, iar un pachet fără rânduri nu se oferă fiindcă ingestia l-ar respinge oricum.

`MainForm.IntreabaDacaRefolosescPachetul` (iconița nodului) și
`IntreabaDacaRefolosescLista` (sincronizarea) pun întrebarea, cu ORA descărcării în ea —
ăla e faptul din care operatorul poate hotărî. Da = din memorie, Nu = descărcare nouă.
Dacă întrebarea însăși cade, se descarcă din nou; niciodată invers.

Refolosirea e sigură prin construcție: contractul în două faze al ingestiei (felia
0055) poartă ACELAȘI payload prin amândouă fazele, iar un pachet din memorie e chiar
obiectul care ar fi ieșit dintr-o descărcare.

`ForexeController.SpuneStare` e nou și public: fără el consola ar fi arătat o ingestie
fără descărcarea ei.

## 2. Buton de golire a așezărilor pasului curent

`btnReseteaza` («Golește așezările»), vizibil DOAR în modul propunere — în editorul de
oricând nu există «pas curent», iar un buton care ar desprinde tot ar fi o unealtă de
stricat.

Linia de despărțire nu e o listă ținută pe lângă, e `InstantaneuLegat.Blocat`: serverul
îl pune `True` pe tot contextul (ce e deja scris în bază) și `False` pe exact mulțimea
de decis în rularea asta — aceeași linie pe care o citesc și `NehotarateleCount`, și
`DeciziiDin`. Deci butonul nu poate crede altceva decât restul formularului.

Se golește la NEAȘEZAT, nu la sugestia automată: rostul e să șteargă tot ce s-a hotărât,
iar o revenire la sugestie ar lăsa în urmă hotărâri pe care operatorul nu le-a luat. Cere
confirmare — e o ștergere de muncă fără Undo.

## 3. Niciun avertisment despre data recepției — nicăieri

F13 fusese retras ca veto pe 31.08.2026 și lăsat ca SEMN. Semnul se aprindea pe date
perfect corecte (`DataR` e tastat pe site și sosește la miezul nopții, deci orice
instantaneu din chiar ziua recepției ieșea «înainte de ea»), deci apărea pe rând după
rând fără să spună nimic. Șters din:

- `AsociereForm.CaptionInstantaneu` (semnul de pe rând), `TooltipReceptie` (numărătoarea),
  `TooltipInstantaneu` (paragraful), plus funcția `EsteInainteDeDataReceptiei` cu totul;
- `prelucrare_asociere.valideaza_plasarile` — blocul care scria în `avertismente`.

Parametrul `rec` a ieșit din `CaptionInstantaneu` și `TooltipInstantaneu`: nu-l mai
folosea nimeni.

## 4. «Recepția 235» nu exista nicăieri în listă

**Ce s-a văzut.** `ApiException: Recepția 235: liniile ultimului instantaneu nu se
potrivesc cu cele ale recepției.` — cu recepțiile puse corect, și cu un număr pe care
operatorul nu-l găsea nicăieri.

**Două defecte, nu unul.**

**(a) Numărul.** Același defect ca «Recepția 188 nu există pe acest angajament» din
felia 0056: pe calea de ingestie, o recepție născută în rularea curentă își primește
`IDRR` ÎNĂUNTRUL tranzacției, iar mesajele de validare îl scriau ca și cum ar fi un nume.
`valideaza_plasarile` are acum `id_stabil`; `aplica_decizii` trece `False` și recepția se
numește prin DATĂ și VALOARE — adică prin chiar textul rândului din formular
(`CaptionReceptie`). Editorul de oricând trece implicit `True`: acolo `IDRR` e real.

**(b) Refuzul însuși.** Comparația de capăt de lanț era o dicționar-comprehensiune pe
`cod_indicator`, deci o recepție cu MAI MULTE linii pe același indicator (alt `CodAI`,
alt `CodSSI`) păstra doar ULTIMA — iar cele două laturi, citite din tabele diferite cu
ordini diferite, puteau păstra linii diferite. Refuza atunci o plasare perfect corectă.
`_valori_pe_indicator` adună pe indicator (bine definit indiferent de ordine; totalul pe
antet e deja verificat mai sus) și scoate zerourile (F16 spune că un indicator poate
cădea la zero, iar el apare pe o latură și lipsește de pe cealaltă). Mesajul spune acum
și CARE indicator nu se potrivește.

## 5. Arborii nu mai sar la primul rând după fiecare tragere

`AdvancedTreeControl.Clear` pune derularea la zero — pe drept, e o golire — iar
`AsociereForm.Reconstruieste` golește și reumple amândoi arborii la fiecare tragere.
Rezultatul: operatorul era aruncat înapoi la primul rând, adică departe de recepția pe
care tocmai lucra.

`AdvancedTreeControl.ScrollOffsetY` (nou) se citește înainte și se scrie după.
`AutoScrollPosition` NU era destul: după `Clear` cursa barei e zero și abia următoarea
pictare o reface, deci o scriere directă s-ar fi tăiat la zero — exact defectul.
Setter-ul cheamă întâi `RefreshScrollVisibility()`. Selecția NU se ține minte: are
înțelesul ei aici (hrănește grila și graficul) și se schimbă oricum prin tragere.

## 6. Recepțiile noi se văd altfel

Toate rădăcinile erau aldine, deci aldinul singur nu deosebea nimic. Recepțiile născute
de descărcarea curentă sunt acum **aldine + cursive**; cele venite de pe server rămân
doar aldine. Semnul e `ReceptiePropusa.RandReceptie` — serverul îl pune EXACT pe
recepțiile create în rularea curentă (felia 0056), Nothing pentru toate celelalte.

Eticheta plutitoare a unei recepții noi nu mai scrie numărul, ci «Recepție NOUĂ, din
descărcarea curentă»: numărul de acum nu e cel de după salvare (vezi §4a).

## 7. Lag pe machetele de distribuție

**Dubla tamponare era deja pornită** (`OptimizedDoubleBuffer` din constructor) și nu ea
era problema. Costul stătea în DESEN, nu în pâlpâire: o suprafață cu douăzeci de benzi a
câte douăzeci de marcaje construia și elibera peste o mie de `Pen` și `SolidBrush` la
FIECARE repictare — iar o repictare se face de câte ori cursorul trece peste o bandă, un
marcaj sau un reper.

Ce s-a schimbat în `KBotLaneView`:

- **Obiecte GDI+ de lucru**, reutilizate în loc de alocate: `_fillBrush`, `_backBrush`,
  `_edgePen`, `_detailPen`. Patru, nu unul, fiindcă unele forme se desenează cât timp
  alta e încă pregătită. Toți trec prin `ScratchPen`/`ScratchBrush`, care rescriu FIECARE
  proprietate — o capătă rotundă sau un stil punctat rămas de la forma dinainte ar fi un
  defect care iese la iveală trei forme mai încolo, în altă metodă.
- **`KBotLane.PlottedInOrder`**: marcajele desenate, deja sortate pe X, umplute de pasul
  de așezare. Înainte, `DrawLaneSegments` construia și sorta o listă PE BANDĂ, PE
  REPICTARE.
- **`_px1` / `_px2` / `_px3`**, cele trei grosimi scalate la DPI, calculate cu așezarea:
  `ScaleDpi` era chemat de patru-cinci ori pe marcaj pentru trei numere care nu se pot
  schimba între două marcaje.
- **`EffectivePlotBackColor()` citit o dată** pe pictare, nu de două-trei ori pe marcaj.
- **`ControlStyles.Opaque`**: fără el WinForms ștergea tot dreptunghiul înainte de
  `OnPaint`, iar `OnPaint` începe cu `g.Clear(BackColor)` — aceeași suprafață umplută de
  două ori degeaba. Ieșirea devreme pe `_updateDepth > 0` curăță acum ea însăși
  suprafața, fiindcă nimeni n-o mai face în locul ei.

Amândouă suprafețele beneficiază: sunt același control (`benzi` din `AsociereForm` și
`benziMari` din `AsociereBenziForm`).

---

## Fișiere atinse

**VB.NET**
- `src/KBot.App/Forexe/AsociereForm.vb` — §2, §3, §5, §6
- `src/KBot.App/Forexe/AsociereForm.Designer.vb` — `btnReseteaza`, `tlyAsociere` la 3 coloane
- `src/KBot.App/Forexe/WorkflowResultStore.vb` — §1
- `src/KBot.App/Forexe/ForexeController.vb` — `SpuneStare`
- `src/KBot.App/KBOT.vb` — §1 (cele două întrebări)
- `src/KBot.Controls/Tree/AdvancedTreeControl.vb` — `ScrollOffsetY`
- `src/KBot.Controls/Lane/KBotLane.vb` — `PlottedInOrder`
- `src/KBot.Controls/Lane/KBotLaneView.vb` — câmpurile de lucru, `Opaque`, `Dispose`
- `src/KBot.Controls/Lane/KBotLaneView.Painting.vb` — §7

**Python**
- `PYTHON/routes/forexe/prelucrare_asociere.py` — §3, §4
- `PYTHON/routes/forexe/asociere.py` — nota din antet
- `PYTHON/tests/test_forexe_prelucrare_asociere.py`, `PYTHON/tests/test_forexe_asociere.py`

## Rezultatele rulărilor

- `dotnet build KBot.sln` — **0 erori, 0 avertismente**.
- `dotnet build src\KBot.App\KBot.App.vbproj` (curat) — 0 erori, cele **7 `MSB3825`
  preexistente** pe `.resx`.
- `PYTHON/.venv` suita întreagă — **514 trecute / 19 sărite / 0 picate** (de la 509).
  Cinci teste noi în `test_forexe_prelucrare_asociere.py`; două rescrise acolo și unul în
  `test_forexe_asociere.py`, fiindcă pineau semnul F13 care tocmai a fost șters.

## Ce NU s-a verificat

- **Nimic nu s-a văzut pe ecran.** Nici formularul cu butonul nou, nici arborii care își
  țin derularea, nici cursivul recepțiilor noi, nici benzile după curățarea desenului.
  Câștigul de viteză e ARGUMENTAT din cod (alocări per repictare), **nu măsurat** — nu
  există niciun cronometru pus pe `OnPaint` și nicio rulare pe calculatorul operatorului.
- **`KBotLaneView` nu are NICIUN test.** `tests/KBot.Controls.Tests` nu conține niciun
  fișier de bandă, deci refactorizarea desenului n-are plasă de siguranță în afară de
  compilare. Suitele VB.NET nu s-au rulat în sesiunea asta (cerere explicită a
  operatorului: «no tests»).
- **Nimic n-a plecat spre serverul viu.** Repararea de la §4 (suma pe indicator, numele
  recepției) e verificată doar pe funcții pure; nicio salvare adevărată n-a trecut prin
  ea, deci nu se știe încă dacă exact ăsta era cazul care refuza salvarea operatorului —
  se știe doar că e un caz care refuza, și că mesajul numea o recepție de negăsit.
- **Refolosirea din memorie n-a fost încercată.** Nicio sesiune FOREXE n-a fost deschisă
  în sesiunea asta.
- Nimic nu s-a comis și nimic nu s-a împins (cerere explicită: «no git»).
