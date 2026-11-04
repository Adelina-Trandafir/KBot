# SLICE-0122-12 — Fereastra de asociere: roșu / portocaliu, tooltip-tabel, culorile din grafic (04.11.2026)

Sub-felie a lui [0122](SLICE-0122-parteneri-fara-idpartener.md) (numărul e al ultimei felii care a atins asocierea recepțiilor: [0122-10](SLICE-0122-10-receptii-vs-rezervari-o-luna.md); lucrarea nu privește partenerii). Cererea operatorului, 04.11.2026, după cazul AAB42RDKAGD, unde un instantaneu (30.09 18:21) fusese așezat pe altă recepție decât a lui și denatura toate cifrele.

**Stare: SCRIS, build curat (KBot.App 0 warnings, 0 errors). NERULAT pe ecran, netestat.**

### What changed and why

Patru cereri ale operatorului în `AsociereForm.vb`; la două întrebări a ales variantele de mai jos.

1. **Culorile aleatorii la selecția unei recepții dispar.** Rândurile din arbore iau culoarea liniei / punctului din grafic doar cât se privește graficul sau benzile, adică cât e deschisă fereastra «Grafice și benzi» (`ArataCuloriDinGrafic`; ele stau altfel într-un panou ascuns). La deschiderea ferestrei culorile se aplică imediat; la închidere rândurile revin la culoarea de bază.
2. **Roșu = EROARE: după VALOARE.** Regula de azi rămâne: lanțul nu se închide (ultimul instantaneu ≠ valoarea recepției) colorează recepția și ultimul ei instantaneu. Fără regulă nouă pe dată (operatorul a ales «doar după valoare»).
3. **Portocaliu = AVERTISMENT** (`ColoreazaAvertismentele`, `IndicatoriLipsa`): un instantaneu a cărui recepție are valoare pe un indicator pe care instantaneul nu îl are. Se verifică FIECARE instantaneu față de recepție (alegerea operatorului; ies și cele vechi, dinaintea unui indicator adăugat). **Un indicator pe 0 nu e avertisment** (operator), nici pe recepție, nici pe instantaneu: se compară mulțimile de indicatori cu valoare nenulă, sumate pe indicator, ca la `_valori_pe_indicator` de pe server (F15). Roșul are întâietate; portocaliul trece peste griul unui rând blocat sau «fără schimbare». Nu se aplică pentru o recepție ștearsă, un rând de ștergere sau o recepție pornită aici (fără linii).
4. **Hover pe recepție:** tooltip-tabel ca în arborele principal (`TooltipReceptieTabel`): liniile obișnuite, apoi un bloc roșu **EROARE** și un bloc portocaliu **AVERTISMENT** cu instantaneele și indicatorii care lipsesc (cel mult 8 rânduri, apoi «și încă N»). Pentru portocaliu textul spune AVERTISMENT, pentru roșu EROARE. `TooltipReceptie` (text simplu) rămâne pentru benzi.

Ajutor: `contabil/asocieri/fereastra.md` (eticheta `0122-12` pe secțiunile atinse).

### Files touched

`src/KBot.App/Forexe/AsociereForm.vb`; `src/KBot.App/HelpContent/contabil/asocieri/fereastra.md`.

### Test results

`dotnet build src/KBot.App/KBot.App.vbproj`: 0 warnings, 0 errors. `Check-Help.ps1`: nicio problemă nouă pe `fereastra.md` (altele, vechi, în alte fișiere). Nimic rulat pe ecran, niciun test (regula proiectului).

### Unverified / deferred

- Nimic văzut pe ecran: nici culorile, nici tabelul tooltip. Nu e verificat că `WarningColor` din temă se citește ca portocaliu.
- Tooltip-ul unui rând de instantaneu și cel al punctelor din grafic nu au textele noi, doar tooltip-ul de pe recepție.
- Pe angajamentele cu multe instantanee vechi, regula «fiecare instantaneu față de recepție» va colora portocaliu multe rânduri; e alegerea operatorului.
- Nu s-a făcut nicio operațiune git.
