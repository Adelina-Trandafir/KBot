# SLICE-0092 — Rezervarea inițială: un singur eveniment, o singură zi

Cererea operatorului, 29.09.2026. Angajamentul `AAB5H2CHDGD` (008_CNJM) arăta DOUĂ rezervări
inițiale în arborele Rezervări.

## Ce s-a întâmplat (verificat din date)

`FX_Rezervari` pentru AAB5H2CHDGD (trimis de operator):

| IDRZ | IDH | Rând | DataRezervare | IDREV | AreDDF | EInitiala |
|---|---|---|---|---|---|---|
| 1192 | 8653 | AA2 3780 | 2026-08-27 | 262 | 1 | 1 |
| 1193 | 8654 | AAB 18000 | 2026-08-28 | — | 0 | 1 |

AA2 a fost adăugat («adaugare rand») pe 27.08, cât angajamentul era «În definitivare»; AAB s-a
definitivat pe 28.08. Pasul 3b (`_calculeaza_val_rezervare_dif`, port fidel al VBA-ului
`CalculeazaValRezervareDif`) le marchează pe amândouă `Rez_Definitiva`, iar pasul 3c le scrie
ca rezervări inițiale (`EInitiala = 1`) — fiecare cu ziua ei. Corect pe rând, greșit ca
eveniment: arborele grupa pe (zi, tip), deci două frunze «Inițială»; generatorul DDF lua doar
ziua cea mai veche fără DDF.

## Ce s-a schimbat (varianta A, aleasă de operator)

Rezervarea inițială se citește ca UN eveniment, pus pe ULTIMA zi inițială a angajamentului
(ziua în care rezervarea a devenit definitivă). Nimic nu se rescrie în bază; se schimbă doar
cititorii, deci și angajamentele deja prelucrate se văd corect.

- **`RezervariView.BuildTree`** — ziua din arbore (`treeDay`) = ultima zi inițială pentru
  rândurile inițiale, `DataRezervare` pentru rest. Folosită la luna, la frunză și la alegerea
  frunzei cu «+». Grila și graficul rămân pe datele reale ale rândurilor.
- **`ddf_edit._SQL_GEN_REZERVARI`** — aceeași regulă pe server: tabel derivat
  `_REZ_NETRIMISE` cu coloana `ZiRez` (MAX(DataRezervare) peste TOATE rândurile inițiale ale
  angajamentului), iar regula «cea mai veche zi, apoi cel mai mic tip» merge pe `ZiRez`. Data
  reviziei generate (`DataRezervare` din primul rând) devine `ZiRez`. Numărul de parametri
  rămâne 5.

## Fișiere atinse

- `src/KBot.App/Views/RezervariView.vb`
- `PYTHON/routes/forexe/ddf_edit.py`

## Teste

- `dotnet build src/KBot.App/KBot.App.vbproj` — 0 avertismente, 0 erori.
- `ddf_edit.py` — parsat (`ast.parse`) cu `.venv`. Suita Python NU a fost rulată (regula
  operatorului: fără teste dacă nu se cer). Interogarea nu a fost rulată pe MariaDB.

## Neverificat / amânat

- **AAB5H2CHDGD rămâne cu DDF doar pe AAB.** AA2 e deja legat de revizia 262 (`AreDDF = 1`),
  deci generatorul propune în continuare o revizie 0 cu o singură linie (AAB). Dacă revizia 262
  conține și AAB, atunci cauza reală e pasul 3e (`step3e_asociaza_idrev`, cazul 2): exclude o
  revizie «deja folosită» de altă rezervare a angajamentului, deși o revizie acoperă mai multe
  rânduri, și AAB rămâne fără legătură. De verificat cu:
  `SELECT R.IDREV, R.NumarRev, R.DC, SA.CodIndicator, SA.Val_Cur FROM FX_DDF_REV R LEFT JOIN FX_DDF_REV_SA SA ON SA.IDREV = R.IDREV WHERE R.CodAngajament = 'AAB5H2CHDGD'`.
- Cele două «lock NUMARREV=0» din jurnal (12:35 și 12:40) pot fi lăsat revizii 0 în plus.
- Nerulat pe ecran, nerulat pe server.
