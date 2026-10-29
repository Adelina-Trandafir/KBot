# SLICE-0014 — Rezervări (endpoint + `RezervariView`)

**Data:** 2026-07-21 **Felia:** 0014 (Rezervări) — a doua vedere reală, după Sumar (0011). **Depinde de:** `AdvancedTreeControl` + `KBotDataView` (feliile anterioare, cod gata / **fără verdict vizual**), plasa `WithReauth` a shell-ului.

---

## Ce s-a schimbat și de ce

`MainForm.CreateView("rezervari")` întorcea `New PlaceholderView(key, "Rezervări")`. Acum întoarce un `RezervariView` real — **a doua dintre cele nouă vederi** care nu mai e un placeholder. Structura oglindește `frmFX_MAIN_REZ`: un **master/detail** cu un arbore de rezervări la stânga (foldere pe lună + frunze pe (dată, tip)) și o grilă continuă la dreapta cu detaliul clasificațiilor.

Read-only în această felie: generarea DDF declanșată de «+» este migration-plan item 7 (`IncarcaRezervare` / `mdl_FX_Rezervari` / `QFX_DDF_REZERVARI` ca driver cu stare) — o felie ulterioară. Aici «+» doar **ridică un eveniment** (`AdaugaDdfCerut`); handler-ul nu e conectat.

## Fișiere atinse

| Fișier | Ce |
|---|---|
| `PYTHON/routes/forexe/rezervari.py` | **NOU** — `GET /api/forexe/rezervari?cod=` |
| `PYTHON/routes/forexe/__init__.py` | + `from . import rezervari` |
| `PYTHON/tests/test_forexe_rezervari.py` | **NOU** — 16 teste host-only |
| `src/KBot.Domain/RezervariInfo.vb` | **NOU** — enum `RezervareTip` + `RezervareRow` / `RezervariInfo` |
| `src/KBot.Api/UpsertAngajamenteRequest.vb` | + DTO-urile de fir `GetRezervariResponse/Row` |
| `src/KBot.Api/IApiClient.vb` | + `GetRezervariAsync(cod, ct)` |
| `src/KBot.Api/ApiClient.vb` | + implementarea (URL escapat, mapare wire → POCO) |
| `src/KBot.App/RezervariIcons.vb` | **NOU** — iconițe GDI (tip «=»/«▲»/«▼» + «+»), tematizabile |
| `src/KBot.App/Views/RezervariView.Designer.vb` | **NOU** — SplitContainer + tree + grid + lblEmpty |
| `src/KBot.App/Views/RezervariView.vb` | **NOU** — logica vederii |
| `src/KBot.App/MainForm.vb` | `CreateView`: `"rezervari"` → `RezervariView` |
| `tests/KBot.App.Tests/SumarViewTests.vb` | + stub `GetRezervariAsync` pe `FakeApiClient` |
| `tests/KBot.Api.Tests/ApiClientTests.vb` | + 6 teste `GetRezervariAsync` |
| `tests/KBot.App.Tests/RezervariViewTests.vb` | **NOU** — 8 teste (comportament + shaping) |

## Decizii de proiectare

**Total lunar = `SUM(R_Valoare)`, verificat în `qFX_REZERVARI_TREE`:** `TOTALL` este corelat pe `(CodAngajament, luna, an)`, candidatul (a) din plan §7.1. Frunzele folosesc `SUM(IIf(EInitiala, R_Initiala, R_Valoare))` per (dată, tip), deci suma lor poate diferi legitim de folderul cu toate rândurile lunii. Planul cerea reproducerea capturii (Ian 1.091.940 / Feb 1.081.876 / …), altfel oprire. **Cifrele nu au fost reproduse**, lipsind datele acelui angajament; este stabilită formula din Access, nu validată captura numeric.

**Valoarea frunzei + tipul** vin din `QFX_DDF_REZERVARI`: `Suma = SUM(IIf(EInitiala, R_Initiala, R_Valoare))` și `TipOperatie = IIf(EInitiala,"Initiala",IIf(EMarire,"Marire","Micsorare"))`. Tipul e derivat client-side (`RezervareRow.Tip`), în ordinea Access (Inițială > Mărire > Micșorare). Ordinea frunzelor în interiorul unei zile reproduce cheia `strData` a arborelui Access: Inițială(0) < Mărire(1) < Micșorare(2).

**Clasificația urmează Sumar 0011-03:** prin `FX_Indicatori` pe `CodAI`, ca în `qFX_REZERVARI_TREE` (RZ INNER JOIN indicatori, apoi Clasificatii). `I.IdClsf` este verificat live drept id Access (`Clasificatii.IdClsfAcc`); direcția `FX_Rezervari.IdClsf` nu este verificată, deci nu furnizează eticheta. Subinterogare scalară `LIMIT 1` evită fan-out-ul duplicatelor `(IdClsfAcc, IdUnitate)`. Predicatul `IdUnitate` rămâne: eliminarea lui privește numai tabelele FX_.

**`LEFT JOIN FX_Indicatori`** (Access folosește INNER): o rezervare nu are voie să DISPARĂ pentru că îi lipsește indicatorul/eticheta — ar dispărea bani din arbore fără urmă. În practică există FK-ul `FX_Indicatori__FX_Rezervari` (CodAI), deci INNER și LEFT dau același rezultat; LEFT e centura de siguranță (Clsf gol dacă indicatorul lipsește).

**Endpoint = cititor brut, fără pre-formarea arborelui.** Aceeași listă de rânduri hrănește și arborele și grila; o modelare pe server ar duplica-o pe fir. Serverul trimite un rând per `FX_Rezervari`; clientul grupează (lună / (dată,tip)) și filtrează.

**Fără filtru pe `R_Valoare`** (spre deosebire de `qFX_REZERVARI_TREE`, care are `R_Valoare<>0`): endpoint-ul întoarce rândurile brute; filtrarea/gruparea e a clientului. Un rând cu `R_Valoare=0` nu schimbă totalul lunar (adaugă 0). **Divergență documentată:** clientul NU replică filtrul de frunză `R_Valoare<>0` din Access — o frunză cu valoare 0 ar apărea (rar). De adăugat dacă apar frunze-zero pe date reale.

**Iconițe GDI (`RezervariIcons`):** Inițială «=», Mărire «▲», Micșorare «▼» și «+», din paletă, cu cache (fel, culoare, dimensiune) și retintare la temă. PNG-urile Access `imgDic("Marire"/"Micsorare"/"Initiala"/"Plus"/"Plus_Green")` lipsesc din K-BOT; formele sunt deterministe/tematizabile. «+» este verde la operația inițială (`Plus_Green`), altfel accent.

**401 rămâne în shell**, prin `WithReauth(Of RezervariInfo)` specializat. `_requestedCod` respinge răspunsurile depășite; contextul Nothing/cod gol nu face cerere. `LoadAsync`, pornit fără `await`, tratează toate erorile. `ApplyTheme` reconstruiește arborele cu date pentru retintare; grila se tematizează singură (`IThemedControl`).

**Filtrul de grilă (§7.3):** fiecare nod poartă în `Tag` rândurile lui; click pe lună → rândurile lunii, click pe frunză → rândurile grupului (dată, tip), nimic selectat → toate rândurile angajamentului. Wiring-ul de date e testat prin conținutul `Tag`; simularea click-ului rămâne pentru harness-ul vizual.

## Rezultate teste

`dotnet build KBot.sln` — **0 erori, 0 avertismente.** `dotnet test KBot.sln` — **verde** (Api 36 → 42, App 22 → 30; total per soluție toate proiectele green).

Cele 6 teste `GetRezervariAsync`: URL + bearer + escaparea lui `cod` + absența oricărui `ss=`; `cod` gol aruncă **înainte** de orice cerere; deserializarea rândurilor + tipul derivat + `ValoareOperatie`; textele `null` (clsf/denumire ale unei rezervări fără clasificație) devin `String.Empty`; `rows: []` → `RezervariInfo` gol, nu excepție; non-2xx → mesaj + `reason`.

Cele 8 teste `RezervariViewTests` (fir STA, `Application.DoEvents()` ca la Sumar): context gol/blank nu atinge rețeaua + golește; răspunsul umple grila cu TOATE rândurile; răspuns depășit aruncat; **gruparea pe lună cu total `SUM(R_Valoare)`** (Ian 155 / Feb 20); **valoarea frunzei = `SUM(ValoareOperatie)`**, flag-ul «+» (doar când grupul are un rând `AreDDF=False`), ordinea pe tip în aceeași zi, gruparea (dată,tip) și nodul roșu la valoare negativă.

Cele 16 teste `test_forexe_rezervari.py` (host-only, se sar off-host — confirmat: 1 skipped, py_compile OK): gardă 401 / `cod` lipsă / blank / diacritice literale; `cod` necunoscut → 200 cu `rows []`; forma rândului; toate cele cinci rezervări întoarse; flag-urile booleene ies `True/False` nu 0/1; `AreDDF` distinge rândurile; `R_Valoare` negativă păstrată cu semn; Clsf/Denumire din coloana generată prin `FX_Indicatori`; **nomenclatorul murdar (duplicat + vecin de altă unitate) NU produce fan-out** (rămân 5 rânduri); ordine deterministă pe dată; `data_rezervare` ISO fără oră.

## Rămâne NEVERIFICAT / amânat

1. **NU s-a rulat niciodată vizual.** `RezervariView` nu a fost văzut pe ecran: nici așezarea split-ului, nici iconițele de tip/«+», nici culorile, nici comportamentul de filtrare la click pe nod. Moștenește avertismentul feliilor 0010/0011 — harness-ul vizual al lui `KBotDataView` **tot nu a fost rulat**. Filtrarea grilei la click pe nod (§7.3) e testată doar la nivel de date (`Tag`), nu ca interacțiune reală.
2. **NU s-a rulat împotriva unei baze reale.** `test_forexe_rezervari.py` se sare off-host; de rulat pe host ca să confirme: cele opt tabele/coloane există, join-ul prin `FX_Indicatori` dă Clsf populat (dacă iese gol pe toate rândurile, cauza e cheia, nu vederea — vezi 0011-03), și că nomenclatorul murdar chiar nu face fan-out live.
3. **Totalul lunar** e formula corectă din sursa Access (`SUM(R_Valoare)`), dar cifrele exacte din screenshot (Ian 1.091.940 / …) NU au fost reproduse numeric — nu am datele acelui angajament. De confirmat vizual pe angajamentul din screenshot.
4. **«+» este display-only** (ridică `AdaugaDdfCerut`, fără abonat). Access arată «+» doar pe PRIMA frunză acționabilă (`IsNull(IDREV) And Not existaNodCuRIcon`); felia asta îl arată pe FIECARE frunză cu `AreDDF=False` (regula simplă din plan §4). Nuanța Access (un singur «+», legat de `IDREV`) revine odată cu workflow-ul DDF.
5. **Captiunile coloanelor** sunt concise („Credit bugetar", „Rezervări inițiale", „Rezervare curentă", „Rezervări definitive"), nu variantele lungi din plan §5 — de confirmat cu operatorul.
6. Celelalte șapte vederi rămân `PlaceholderView`.
