# K-BOT — STATUS, slices 0120–0129

Everything recorded about each slice. The index in `../KBOT_STATUS.md` says what each slice is.

---

## Slice 0120 — DDF editor: the action row beside «Salvează documentul»

Part of the new main window recorded on 13.10.2026 (slices 0113–0120; decisions and the shared open threads in [KBOT_STATUS_0110-0119.md](KBOT_STATUS_0110-0119.md), «Slices 0113–0120»).

| Slice | Name | Status | Worklog |
|------:|------|--------|---------|
| 0120 | While page «Secțiunea A» is open, its row «Adaugă rând / Șterge rândul / state» moves into the editor's bottom row, beside «Salvează documentul» (`DdfEditSectiuneaAPage.AttachActionsTo` / `DetachActions`, `DdfEditForm.ActivatePage`); with 0113 «Renunță» is hidden (ribbon «Închide» / Esc; numbers released on close as before) | GATA pe cod (build of the merge 0/0), seen only in `KBOT_TEST`; help 0000-74 | [SLICE-0120](../SLICE-0120-ddf-randul-de-jos.md) |

### Open threads — 0120

- Tour `tur-ddf-editor` has a step on `DdfEditForm.btnRenunta` (now hidden): proposal in the 0000-74 worklog; tours not changed without the operator's OK.

---

## Slice 0121 — Supplier <-> angajament links by CodFiscal (PLAN)

Operator, 19.10.2026 (page «Recepții», KE16): an angajament picked for a supplier belongs to the supplier; the durable link is keyed by the normalized CodFiscal, not by the per-unit `IdPartener` / `CodPartener`. `Parteneri` is not altered. Full plan, decisions and facts read: [SLICE-0121-parteneri-angajamente-pe-cod-fiscal.md](../SLICE-0121-parteneri-angajamente-pe-cod-fiscal.md).

| Slice | Name | Status | Worklog |
|------:|------|--------|---------|
| 0121-01 | Schema: legături și indicatori | RULAT și văzut pe ecran de operator, 08.11.2026 (`sql/P121_01_parteneri_angajamente.sql`, AVACONT_SURSA + sync) | [0121-01](../SLICE-0121-parteneri-angajamente-pe-cod-fiscal.md#slice-0121-01) |
| 0121-02 | Migrator: conversie și raport | RULAT și văzut pe ecran de operator, 08.11.2026 (build 0 warn/0 err) | [0121-02](../SLICE-0121-parteneri-angajamente-pe-cod-fiscal.md#slice-0121-02) |
| 0121-03 | Fereastra Parteneri: noile legături | RULAT și văzut pe ecran de operator, 08.11.2026 (build 0 warn/0 err, py_compile); ajutor 0000-113 | [0121-03](../SLICE-0121-parteneri-angajamente-pe-cod-fiscal.md#slice-0121-03) |
| 0121-04 | Asociere DDF/furnizor: hook-uri | RULAT și văzut pe ecran de operator, 08.11.2026 (py_compile); route `POST /api/forexe/parteneri/angajament` for 0121-05 | [0121-04](../SLICE-0121-parteneri-angajamente-pe-cod-fiscal.md#slice-0121-04) |
| 0121-05 | Recepții: alegere persistentă pe furnizor | RULAT și văzut pe ecran de operator, 08.11.2026 (build 0 warn/0 err, py_compile); ajutor 0000-114 | [0121-05](../SLICE-0121-parteneri-angajamente-pe-cod-fiscal.md#slice-0121-05) |
| 0121-07 | Copie unică din vechile coduri | RULAT și văzut pe ecran de operator, 08.11.2026 («Interogari unice», după P121_01 + sync + fișierele Python) | [0121-07](../SLICE-0121-parteneri-angajamente-pe-cod-fiscal.md#slice-0121-07) |
| 0121-06 | Închidere condiționată de probe și operator | NEPORNIT: rămâne la cuvântul operatorului (șterge `Parteneri_Coduri`) | [0121-06](../SLICE-0121-parteneri-angajamente-pe-cod-fiscal.md#slice-0121-06) |

### Open threads — 0121

- [Închiderea 0121-06](../SLICE-0121-parteneri-angajamente-pe-cod-fiscal.md): 01…05 și 07 sunt rulate și văzute pe ecran de operator (08.11.2026). 06 (ștergerea `Parteneri_Coduri`) așteaptă ultima unitate trecută prin Migrator și cuvântul operatorului; neconfirmat pe câte baze/unități s-a rulat.

---

## Slice 0122 — Partner only by CodFiscal, no IdPartener/CodPartener (ANALYSIS)

Operator, 20.10.2026: end the dependence of `FX_DDF_REV_SA`, `FX_DDF_REV_SB`, `FX_ORD_TBL` and `VC_NIR` on `Parteneri.IdPartener` (outside the Access migration), so `Parteneri` can later hold one row per CodFiscal. The next update is mandatory, so old clients are not a constraint. Full analysis, sub-slices and open decisions: [SLICE-0122-parteneri-fara-idpartener.md](../SLICE-0122-parteneri-fara-idpartener.md).

| Slice | Name | Status | Worklog |
|------:|------|--------|---------|
| 0122 | Plan: CodFiscal în CodPartener, fără dependența de id | ÎNCHIS: RULAT și testat de operator, 08.11.2026 | [0122](../SLICE-0122-parteneri-fara-idpartener.md#slice-0122-plan) |
| 0122-01 | Conversia datelor și corecția 01b | RULAT și testat de operator, 08.11.2026 | [0122-01](../SLICE-0122-parteneri-fara-idpartener.md#slice-0122-01) |
| 0122-02 | Server DDF/ORD: CodFiscal normalizat | RULAT și testat de operator, 08.11.2026 | [0122-02](../SLICE-0122-parteneri-fara-idpartener.md#slice-0122-02) |
| 0122-03 | Cititori și protecția la ștergere | RULAT și testat de operator, 08.11.2026 | [0122-03](../SLICE-0122-parteneri-fara-idpartener.md#slice-0122-03) |
| 0122-08 | Ascunderea partenerilor nefolosiți, cu previzualizare | RULAT și testat de operator, 08.11.2026 | [0122-08](../SLICE-0122-parteneri-fara-idpartener.md#slice-0122-08) |
| 0122-11 | Partenerii fără activitate (ascunși de `0122_08`) lipseau din lista «Parteneri» și din combo-ul de parteneri al DDF: ambele interogări de server trimit acum și rândurile cu `Ascuns = 1` | RULAT și testat de operator, 08.11.2026 | [0122-11](../SLICE-0122-11-parteneri-ascunse-fara-activitate.md) |
| 0122-09 | DDF edit: «Recepții» din revizia din rezervare însumează `DIF`, nu `Valoare` | RULAT și testat de operator, 08.11.2026 | [0122-09](../SLICE-0122-09-ddf-receptii-dif.md) |
| 0122-10 | Recepții vs rezervări: prag de o lună la așezarea automată (server + Asocieri) și detector în tooltipul arborelui | RULAT și testat de operator, 08.11.2026 | [0122-10](../SLICE-0122-10-receptii-vs-rezervari-o-luna.md) |
| 0122-12 | Fereastra de asociere: roșu = EROARE (lanțul nu se închide), portocaliu = AVERTISMENT (recepția are valori pe indicatori pe care instantaneul nu îi are; zerourile nu se socotesc), tooltip-tabel pe recepție, culorile din grafic pe rânduri doar cât e deschisă fereastra «Grafice și benzi» | RULAT și testat de operator, 08.11.2026 | [0122-12](../SLICE-0122-12-asociere-culori-rosu-portocaliu.md) |

---

## Slice 0123 — VERCON and ADECHIT reports in K-BOT.app (native pages)

Operator, 21.10.2026: bring the reports of the VERCON and ADECHIT sites into K-BOT as pages that look like the application (not a hosted web page), in two new ribbon tabs, VERCON and ADECHIT, each with a «Rapoarte» button; VERCON also holds «Trimite la Vercon». The reports stay computed by the server (same code as the site); K-BOT chooses the parameters, shows the PDF and saves the Excel. Decisions: ADECHIT first, VERCON with full parity afterwards; nothing may stay on the server (the snapshot system is kept but now deletes what is expired and releases a report when its viewer closes).

| Slice | Name | Status | Worklog |
|------:|------|--------|---------|
| 0123-01 | File ADECHIT/VERCON, rapoarte ADECHIT și snapshoturi comune; ajutor 0000-119 | GATA pe cod (KBot.App build 0 warnings / 0 errors, py_compile ok); **server NEDEPLOYAT, nimic rulat, pagina nevazuta pe ecran** | [SLICE-0123-01](../SLICE-0123-01-adechit-rapoarte.md) |
| 0123-03 | Corective ADECHIT: încărcare, antet, Ctrl+2 și căutarea copilului (22.10.2026) | GATA pe cod (build 0/0, py_compile ok); **server NEDEPLOYAT, nimic rulat**; cauza listei «Copil» gri NEDOVEDITA | [SLICE-0123-03](../SLICE-0123-03-corective-test-adechit-rapoarte.md) |
| 0123-04 | Ctrl+H/Ctrl+2 după ferestrele Adobe; retragerea FitWidthOnly | GATA pe cod (build 0/0), **nimic rulat**; numele/starea ferestrei panoului in mod citire NECAPTURATE | [SLICE-0123-04](../SLICE-0123-04-ctrl-h-dupa-ferestre.md) |
| 0123-05 | File ADECHIT/VERCON condiționate de datele unității | GATA pe cod (build 0/0, py_compile ok); **server NEDEPLOYAT (setari.py), nimic rulat**; ce inseamna «are date» de confirmat | [SLICE-0123-05](../SLICE-0123-05-file-doar-cu-date.md) |
| 0123-06 | Textul drept al arborelui tăiat cu «...»; Controls 1.69.0.0 | GATA pe cod (build 0/0); **nevazut pe ecran** | [SLICE-0123-06](../SLICE-0123-06-arbore-text-dreapta-cu-puncte.md) |
| 0123-07 | ClickOnKeyNavigation, implicit oprit; Controls 1.70.0.0 | GATA pe cod (build 0/0); **nicio fereastra nu l-a aprins inca, nevazut** | [SLICE-0123-07](../SLICE-0123-07-arbore-click-la-miscarea-cu-tastele.md) |
| 0123-02 | Rapoarte VERCON native: parametri, arbori lazy, PDF/Excel; ajutor contabil/vercon-rapoarte.md | GATA pe cod (KBot.App build 0 warnings / 0 errors, py_compile ok); **server NEDEPLOYAT, nimic rulat sau vazut**; lazy tree and tick-an-unread-branch NEEXERSATE | [SLICE-0123-02](../SLICE-0123-02-vercon-rapoarte.md) |

### Open threads — 0123

- The server files of 0123-01 and 0123-02 (lists in the worklogs) are for the operator to publish; the bridges assume the K-BOT user name is the e-mail of the AD / VR roles.
- Other VERCON server files + the site's `reports.js` / `report-result.js` were being changed in the same tree by someone else during 0123-02 (OI reports rework, `tree_group`); the page copies the site's report-tree / selection rules as of 23.10.2026 and must follow if they change.
- Excel exports all columns (no column picker yet). VERCON «Rapoarte»: the period tree combo opens all years (the site opens the latest only).
- Ajutor de actualizat: none (done in 0000-119); ribbon captures to redo are listed in the 0000-119 worklog.

---

## Slice 0124 — The DDF flow for DDFs created in K-BOT (DeSemnat, compartment -> role, tray for every role)

Operator, 30.10.2026. Plan and decisions: [SLICE-0124-01](../SLICE-0124-01-flux-ddf-kbot-plan-si-banca.md). Existing revisions stay out of the flow (`DeSemnat` empty).

| Slice | Name | Status | Worklog |
|------:|------|--------|---------|
| 0124-01 | The plan; the bench (`DdfFluxServerHarnessForm`) carries captures in step 2 | GATA pe cod (build 0 erori); **capturile probate de operator 30.10.2026 (o imagine de proba in B, semnat si incarcat)**; nimic altceva rulat | [SLICE-0124-01](../SLICE-0124-01-flux-ddf-kbot-plan-si-banca.md) |
| 0124-02 | SQL: `FX_DDF_REV.DeSemnat` + `FX_DDF_COMP_ROL` (`sql/0124_ddf_desemnat_comp_rol.sql`); rolurile `Secretar` si `Imputernicit` | SCRIS, **NERULAT** (de rulat de operator pe SURSA, apoi sincronizare) | [SLICE-0124-02](../SLICE-0124-02-sql-desemnat-comp-rol.md) |
| 0124-03 | Server: `ddf_flux_core.py` + `ddf_flux.py` (list per role, `porneste`, compartment -> role table), `DeSemnat` moves in `pdf.py`; Secretar / Administrator see no DDF but the addressed ones (`guard.py` → `refuza_restrans`, `ddf.py` filter), everything else as before | GATA pe cod (py_compile + import OK); **nerulat pe server, de publicat de operator** | [SLICE-0124-03](../SLICE-0124-03-server-flux-si-restrictii.md) |
| 0124-04 | Client API + generation with section A only (`DdfPdfMode.OnlyA`) by the role that signs A, on that PC, nothing stored before the signature; 412 = old way | GATA pe cod (build 0 erori); **nerulat** | [SLICE-0124-04](../SLICE-0124-04-client-api-si-generare-doar-a.md) |
| 0124-05 | Setari page «Semnare DDF» (compartment -> role) + `sql/0124_roluri_imputernicit.sql` (AVACONT_COMUN) | GATA pe cod; **nerulat, SQL-ul de comun de rulat de operator** | [SLICE-0124-05](../SLICE-0124-05-setari-comp-rol-si-roluri.md) |
| 0124-06 | List of documents to sign for Director / Contabil / Imputernicit (+ ribbon button, tray) and the narrowed window `DeSemnatRestransForm` for Secretar / Administrator | GATA pe cod; **nerulat, fereastra nedeschisa niciodata** | [SLICE-0124-06](../SLICE-0124-06-lista-pentru-toate-rolurile-si-fereastra-ingusta.md) |
| 0124-07 | Section B + captures: the old logic after K-BOT's own sending, plus the operator's step 6 gate (flow revision: the reservation must exist in K-BOT's table) | GATA pe cod; **nerulat** | [SLICE-0124-07](../SLICE-0124-07-sectiunea-b-la-vederea-rezervarii.md) |
| 0124-08 | Help: `ddf/semnare.md` (new section), `ddf/secretar.md` (new), `setari.md` (page «Semnare DDF») | GATA; fara poze | [SLICE-0124-08](../SLICE-0124-08-ajutor.md) |
| 0124-09 | Corectiv: fereastra de PIN a tokenului (si «Signing») adusa in fata de capcana Adobe, o data, FARA sa i se citeasca textul (a doua trecere: prima n-a tinut); `KBot.Controls` 1.74.0.0; ajutor 0124-09 in `ddf/semnare.md` | GATA pe cod (build 0 erori); **nevazut pe ecran, nerulat**; titlurile sunt cele englezesti masurate azi | [SLICE-0124-09](../SLICE-0124-09-fereastra-pin-in-fata.md) |

### Open threads — 0124

- **Nothing of 0124-03 .. 0124-08 ran.** To try, in order: the operator publishes the py files; runs `sql/0124_roluri_imputernicit.sql` on AVACONT_COMUN; checks the sync carried `sql/0124` to the unit; sets the compartments in Setari › Semnare DDF; generates a new DDF; signs A as the role; (old «Trimite în FOREXE» from K-BOT) opens it as the accountant for B; the director signs.
- The director's narrowing is still only in the window (DIR1); the mechanism of 0124-03 could close it on the server if asked.
- `director/lista.md` (help) still speaks of the director only; the accountant / proxy have the same button now.
- `KBOT_BANC_PDF` (000_DEMO) can be dropped again by the next schema sync; it is not in the model the sync follows.
- Captures in the bench: one sample image works (operator, 30.10.2026); not tried with several images or the revision's own PrtScr attachments.

---

## Slice 0125 — A simulated FOREXE for testing the signatures (Debug only)

Operator, 30.10.2026. A FOREXE answered inside K-BOT (no browser, forexecab untouched), reached only from the Debug start window («Aplicația cu FOREXE simulat»).

| Slice | Name | Status | Worklog |
|------:|------|--------|---------|
| 0125-01 | `SimulatedForexeRunner` (Debug, `HarnessTests`) answers `CreareAngajament`, `IncarcaRezervare`, `DefinitivareAngajament`, `DerulareAngajament` and the read-back `RezervariAngajament` in the shape of the real answer; the answer goes through the REAL import on the server (needed: section B appears only when `FX_Rezervari` has a row for the revision, 0124-07); launcher entry, `ForexeController.ForexeSimulat`, a line in the send question | GATA pe cod (build Debug 0 erori, 0 avertismente); **nimic rulat, importul nevazut pe server**; scrie in baza de date — doar 000_DEMO | [SLICE-0125-01](../SLICE-0125-01-forexe-simulat-debug.md) |
| 0125-02 | The simulated FOREXE also answers the node download (`PrelucrareCompleta`/`Reverse`), the receptions refresh (`ReceptiiAngajament`, skipped days with empty detail), `RezervariEditate`, `DeschideAngajament`; one sample reception is made up when an angajament «În derulare» is first read; the «Browser FOREXE» view docks a made-up page (`SimulatedForexePage`) whose «Salvează rezervarea» runs the real page's road (gate, «before» picture, `(IDREV: n)` marker, history row, watcher report) so `CaptureFlow` is exercised; an angajament the simulator never saw is refused | GATA pe cod (build Debug 0 erori, 0 avertismente); **nimic rulat**; scrie in baza de date prin import — doar 000_DEMO | [SLICE-0125-02](../SLICE-0125-02-forexe-simulat-nod-receptii-pagina.md) |

### Open threads — 0125

- 0125-02, first run to watch: save a reservation on a `SIM…` angajament (marker in the history description, both pictures uploaded under the `IDREV` number), a node download «În derulare», a receptions refresh with a day unticked.
- Not simulated: saving a reception or a new angajament by hand in the page, `InfoCompleteContract`.

- First run to watch: send a K-BOT-made revision, see it reach «PDF final / Semnat A», then `AreRezervari` true and section B written; the import of the simulated history was matched against the server code by reading only.
- The simulated import writes `FX_Indicatori_Buget` with «largest value + 100.000» per classification: never on a real database.
- No failure injection (a send that stops half-way) yet.
- **Found, not from this slice:** `HarnessTests/DdfFluxServerHarnessForm.vb` (0124-01) has no `#If DEBUG` while `PdfSignatureReport` / `RecordingPdfApi` do, so `dotnet build -c Release` of KBot.App fails with 4 errors (BC30451). Needs the file wrapped before the next Release publish.

---

## Slice 0126 — Clasificații: rânduri goale de adăugare, totaluri în arbore, tabel de angajamente

Operator, 30.10.2026, fereastra «Clasificații cheltuieli».

| Slice | Name | Status | Worklog |
|------:|------|--------|---------|
| 0126-01 | Rând gol de adăugare în tabelele de buget și rectificări (3 și 6 rânduri fără derulare), totalul final în arbore (`~~~` a treia parte, aliniat la dreapta, fără zecimale), rândul de total cu culoarea subsolului, tabel nou «Angajamente» (rută `POST …/clasificatii/angajamente`) | GATA pe cod (build Debug 0 erori / 0 avertismente, `py_compile` verde); **nevăzut pe ecran, rută nedeployată, nimic rulat** | [0126-01](../SLICE-0126-01-clasificatii-randuri-goale-totaluri-angajamente.md) |
| 0126-03 | «Salvează», «Verifică bugetul», «Trimite în Access» mutate în ribbon (grupul din dreapta, `ClasificatiiForm` implementează `IViewRibbon`; cheile `clasificatii_*` în `AvacontMain`), banda de jos doar cu linia de stare, rectificări 6 → 2 rânduri ca «Angajamente» să încapă 3 rânduri fără bară; ajutor `0126-03` (cererea operatorului, 07.11.2026) | GATA pe cod (build Debug 0 erori / 0 avertismente); **nevăzut pe ecran, nimic rulat**; pagina Venituri mutată și ea (+ «Șterge clasificația») | [0126-03](../SLICE-0126-03-clasificatii-butoane-in-ribbon.md) |
| 0126-04 | Bifele arborelui mutate într-un popup al iconiței din dreapta a antetului; arborii din Clasificații (Cheltuieli și Venituri): culorile de căutare fixate scoase, chenar în culoarea tabelelor (`ClasificatiiTreeStyle`); ajutor `0126-04` (cererea operatorului, 07.11.2026) | GATA pe cod (compilare 0 erori / 0 avertismente); **nevăzut pe ecran, nimic rulat** | [0126-04](../SLICE-0126-04-clasificatii-filtre-in-antet-chenar-arbori.md) |
| 0126-05 | Memoria ferestrei Clasificații: bugetul, rectificările și angajamentele tuturor clasificațiilor citite o dată la deschidere, legate din memorie la clic; `DataChangeBus` anunță scrierile (buget, angajamente, rezervări, recepții, plăți) și fereastra își reîmprospătează memoria; rută nouă `buget-toate` + `{"all": true}` pe `angajamente` (cererea operatorului, 07.11.2026) | GATA pe cod (compilare 0 erori / 0 avertismente, `py_compile` verde); **nevăzut pe ecran, rutele nedeployate, nimic rulat** | [0126-05](../SLICE-0126-05-clasificatii-memorie-si-anunt-modificari.md) |
| 0126-06 | Titlurile de deasupra tabelelor (buget, rectificări, angajamente) mutate în antetul primei coloane; etichetele scoase din formular, și la Cheltuieli, și la Venituri (cererea operatorului, 07.11.2026) | GATA pe cod (compilare 0 erori / 0 avertismente); **nevăzut pe ecran** | [0126-06](../SLICE-0126-06-clasificatii-titluri-in-antet.md) |

### Open threads — 0126

- 0126-03: numărul de felie ales de mine (următorul liber din 0126); operatorul de confirmat. De văzut pe ecran: butoanele din ribbon (pictogramele alese de mine), «Angajamente» cu 3 rânduri fără bară la 100 / 125 / 150%. Idem pentru pagina Venituri.

- Numărul de felie ales de mine (primul liber); operatorul de confirmat.
- Interpretare: 3 și 6 sunt rânduri cu date, rândul gol vine în plus; două constante de schimbat dacă nu.
- De văzut pe ecran la 100 / 125 / 150%: fără bară de derulare, coloanele aliniate, culoarea rândului de total, fereastra mai înaltă.
- Ajutor: captura `clasificatii` de refăcut (ajutor de actualizat: `contabil.nomenclatoare.clasificatii`).

---

## Slice 0127 — Motivul rezervării din FOREXE, oferit la revizia > 0

Operator, 04.11.2026, numărul ales de el («ultimul disponibil»). Doar modul Lucru din browserul din aplicație.

| Slice | Name | Status | Worklog |
|------:|------|--------|---------|
| 0127-01 | Motivul din fereastra FOREXE de la salvarea unei rezervări e citit de pagină, trimis cu salvarea, păstrat pe server în `FX_RezervariMotiv` (cheie `IDREV` + `CodIndicator`, `IDREV` dat de server) și oferit ca «Descriere scurtă» la generarea unei revizii > 0; salvările aceluiași indicator se unesc cu «; », indicatori diferiți rămân separați (« | »), tăiere la 255 cu avertisment; `FX_Istoric` nu e folosit | GATA pe cod (build Debug 0 erori / 0 avertismente, `ast.parse` verde); **DDL `sql/0127_fx_rezervari_motiv.sql` nerulat, server nedeployat, nimic văzut sau rulat**; ajutor 0000-151 | [0127-01](../SLICE-0127-01-motivul-rezervarii-pentru-revizie.md) |

### Open threads — 0127

- Operatorul rulează DDL-ul `0127` pe SURSA și sincronizează unitățile; până atunci ruta dă 500 cu mesaj și revizia rămâne ca înainte.
- De verificat pe pagină reală: că `Indicator_ang` citit din tabel se potrivește cu `FX_Rezervari.CodIndicator`, și că fereastra de motiv apare în operația `rezervare` armată (consola K-BOT spune «Motivul rezervării ... e păstrat»).
- De confirmat: motive diferite la indicatori diferiți ai aceleiași revizii apar împreună cu codul indicatorului, separate prin « | ».
- Rezervările făcute altfel decât în modul Lucru (robot, FOREXE în alt browser) nu au motiv păstrat; revizia primește tipul operației, ca până acum.
- Nu s-a rulat `tools\Register-DeploymentTask.ps1`: se face doar după ce Adelina confirmă testul (fișierele py + sql sunt cele care ajung pe server).

---

## Slice 0128 — E-Factura: vederea «Recepții» a facturii primite

Operator, 05-06.11.2026: felie nouă cu subfelii, cerută pe loc («slice nou cu subslice-uri»). Vederea «Recepții» din bara de vederi a facturilor primite (fereastra «Facturi primite» și vederea «E-Factura» a unui angajament).

| Slice | Name | Status | Worklog |
|------:|------|--------|---------|
| 0128-01 | Așezarea la prima afișare: bara «Recepție nouă / Recepții existente» își recalculează butoanele când devine vizibilă; formularul incorporat refă layoutul; decalajul spre dreapta (prima coloană scrisă de mână în `ColumnStyles`, citită înapoi ca lățimea copilului ascuns) reparat cu `SetColumnWidth` + `SetColumnCollapsed` | GATA pe cod (build 0/0); văzut pe ecran de operator la 06.11.2026; cauza decalajului dedusă din cod | [0128-01](../SLICE-0128-01-receptii-prima-afisare-si-decalaj.md) |
| 0128-02 | `KBotNavList`: `BackColor` pus în designer rămâne (nu mai e suprascris de temă; sub temă întunecată câștigă tema) + bordură de subsol cu `FooterBorderVisible` / `FooterBorderWidth` (2 px) / `FooterBorderColor` (gol = Classic Gainsboro, Modern accent, restul InactiveBorder) | GATA pe cod (build 0/0); văzut pe ecran de operator la 08.11.2026; bordura e pornită implicit pe toate barele; fără ajutor | [0128-02](../SLICE-0128-02-navlist-fundal-si-bordura-subsol.md) |
| 0128-03 | Butonul «Creează recepția» / «Leagă» / «Modifică recepția» în grupul din dreapta al ribbon-ului, doar cât vederea e pe ecran; scos din subsol | GATA pe cod (build 0/0); nicio recepție reală făcută din ribbon | [0128-03](../SLICE-0128-03-receptii-buton-in-ribbon.md) |

### Open threads — 0128

- Nevăzut: rolul «Director» (butonul din ribbon nu e în `RibbonKeys`), textul butonului pe fiecare mod.
- `FileVersion` neridicat (se ridică la publicare); nimic în `NOUTATI.md` (se scrie la publicare).
- Nu s-a rulat `toolsRegister-DeploymentTask.ps1`: nu s-a atins niciun fișier care ajunge pe server.

---

## Slice 0129 — Culorile ferestrelor, grilelor și arborilor vin din temă

Operator, 07.11.2026: fundal view/formă = «Control», fundal controale = «Window», chenar = «Active Border»; valorile sunt cele din Classic, dar trebuie să se schimbe cu tema.

| Slice | Name | Status | Worklog |
|------:|------|--------|---------|
| 0129-01 | 137 de linii `BackColor` / `BorderColor` fixate pe `SystemColors` (Window, ActiveBorder, Control) sau pe alb / WhiteSmoke scoase din 70 de `.Designer.vb`; controalele revin la «din temă» | GATA pe cod (build 0/0); aspectul în Classic / Modern / Dark / Colorful testat și rulat de operator, 08.11.2026; ajutor 0000-166 | [0129-01](../SLICE-0129-01-culori-din-tema-in-designere.md) |
| 0129-02 | Regula generică de `Panel` din `ThemeManager` ca la `KBotTableLayoutPanel`: Classic = Window, Dark = SurfaceAlt, restul = Surface (Card, split, tabele, GroupBox: vezi 0129-03); `pnlHeader` din `SumarView` / `KbotForm` fără `Window` fixat | GATA (build `KBot.Theming` 0/0); verificat pe ecran și testat de operator, 08.11.2026; fără ajutor | [0129-02](../SLICE-0129-02-panel-fundal-ca-tabelul.md) |
| 0129-03 | Toate controalele care gazduiesc alte controale (SplitContainer + panouri, Panel, Card, TableLayoutPanel, GroupBox, KBotTableLayoutPanel, vederi / pagini / formulare cu etichetele lor) iau acelasi fundal `ThemeScheme.PanelBack()`; `KBotNavList` neatins | GATA (build `KBot.App` + `KBot.EFactura` 0/0); verificat pe ecran si aprobat de operator, 08.11.2026; fara ajutor | [0129-03](../SLICE-0129-03-toate-containerele-acelasi-fundal.md) |

### Open threads — 0129

- `FileVersion` neridicat; nimic în `NOUTATI.md`; nu s-a atins niciun fișier care ajunge pe server.
