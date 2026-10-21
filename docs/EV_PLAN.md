# EV — Facturi primite (E-Factura) → VERCON (PLAN, nimic implementat)

Scris 20.10.2026, la cererea operatorului; rescris în aceeași zi cu deciziile operatorului. Ce e verificat (citit într-un
fișier) și ce e presupus sunt marcate. Familia **EV** (propunere de nume, ca KE): partea K-BOT și partea Vercon se țin
împreună aici; numerotarea oficială (00EF-NN pe partea K-BOT, VCxx pe partea Vercon) se alege când pornește prima felie.

**Stare (21.10.2026, după implementare):** EV01…EV05 sunt în cod, **nimic rulat**: SQL-ul (`sql/EV_01_primite_vercon.sql`) nerulat, serverul nedeployat, nimic văzut pe ecran sau în browser.
Felii: EV01 = `00EF-45` (server K-BOT), EV02 = `00EF-46`, EV03 = `00EF-47` (fereastra + lista, ajutor `0000-116`), EV04 = `VC06-07`, EV05 = `VC06-08` (worklogurile `SLICE-…` din `docs/worklog/`).
Unde codul diferă de plan, hotărăște codul:
- **EV02** nu e un `KBotTreeComboBox` cu `AdvancedTreeControl`, ci **grupe pliabile în lista `KBotComboBox`** (antet de gestiune cu triunghi, conturile dedesubt); editorul de celulă și combo-ul de pe rândul de grupare merg neschimbate.
- **EV03** (refăcut în `00EF-48`, la cererea operatorului: «un form de sine stătător pus în ribbon la E-Factura») este **pagina «Trimite la Vercon»**, un buton nou în grupul «E-Factura» al ribbonului (lângă Cumpărare și Vânzare): facturile primite ale anului în stânga, cu bifă și coloana «Vercon», liniile facturilor bifate în dreapta, cu contul din Vercon. Prima variantă (fereastră modală + coloană + buton în lista tuturor facturilor, `00EF-47`) a fost scoasă. **Nu** a livrat indicatorul de stare din arbore, filtrul din meniul arborelui și butonul din vederea unei facturi.
- Lista «Din e-Factura» din Vercon nu e filtrată pe gestiune (D12). Cazul „din mai multe NIR-uri se anulează doar unul” (secțiunea 4) **nu** e construit.

## 1. Cerința

Facturile primite din K-BOT (E-Factura › cumpărare) trebuie să ajungă în Vercon, ca Administratorul să le poată
prelua într-un NIR. **Contabilul decide în K-BOT** dacă o factură merge la Vercon; Administratorul nu primește tot, ci
doar ce a trimis contabilul.

**Contabilul poate alege, dacă vrea, contul Vercon pentru fiecare linie a facturii.** Conturile sunt cele existente în
Vercon, din toate subunitățile (deci un arbore: gestiune → conturi). Selecția se face în DGV, ca în asocierea
facturilor cu recepțiile (KE11): linii grupate pe factură, valoarea se alege pe linie sau o dată pe rândul de grupare
(`GroupHeaderCombo`), iar alegerea de pe grupare se scrie pe toate liniile facturii.

Planul Vercon (`VERCON/PLAN_IMPLEMENTARE.md`, N13 / D11) a lăsat integrarea cu E-Factura pentru „un pas separat, la
cerere”. Aceasta este cererea; la implementare se corectează și documentele care spun „e-Factura exclusă”.

## 2. Decizii ale operatorului (20.10.2026)

| # | Decizie |
|---|---------|
| D1 | Contabilul trimite factura la Vercon; **contul pe fiecare linie e opțional**, ales dintr-un arbore cu toate conturile Vercon, indiferent de subunitate. Alegerea se face în DGV, cu valoare pe rândul de grupare (soluția din KE11). |
| D2 | Administratorul poate **returna** o factură, cu motiv obligatoriu. |
| D3 | **Facturile storno** nu se trimit în primele felii (fluxul FCS din Vercon vine la cerere). |
| D4 | Legătura cu recepțiile FOREXE (KE) este **independentă**: aceeași factură poate fi legată de o recepție și trimisă la Vercon; arborele arată ambele stări separat. |
| D5 | **NIR anulat după preluare → factura revine automat în coadă** (fără retrimitere de către contabil). |
| D6 | Lista „Din e-Factura” o vede și preia oricine poate adăuga NIR (`VR_RECEPTIE`); **nu se creează un drept nou**. |
| D8 | **MF și OI sunt incluse** în arborele de conturi. Datele în plus ale unui NIR de MF/OI le completează Administratorul. |
| D9 | Contul ales în K-BOT este **propus**, nu impus: Administratorul îl vede precompletat și îl poate schimba. |
| D10 | **Se trimit TOATE liniile facturii.** Nu există bifă pe linii și nu există linii netrimise. |
| D11 | **Fiecare bază de unitate conține mereu toate tabelele din SURSA** (deci și `EF_*`, și `VC_*`): K-BOT și Vercon văd aceeași bază în orice unitate. |
| D12 | **Administratorul are acces la TOATE conturile și gestiunile.** Consecințe: o factură = o singură trimitere (nu se împarte pe gestiuni); coada nu se filtrează pe gestiunea curentă; contabilul nu alege nicio „gestiune implicită”. |

(D7 din versiunea anterioară — „factura mixtă pe linii” — este acoperit de D1 + D12 și dispare.)

## 3. Ce există azi (verificat)

| Ce | Unde | Fapt |
|----|------|------|
| Factura primită | `EF_Primite`, `EF_PrimiteLinii`, `EF_PrimiteTVA` (`MariaDB_Schema/000_DEMO.sql`) | antet: `NrFact`, `DataFact`, `Total`, `Valoare`, `TVA`, `CuiNormalizat`, `Tip`, `Semn`; linii: `Denumire`, `Unit`, `Cant`, `Pret`, `Valoare`, `Cpv` |
| Aceeași bază | `000_DEMO.sql` conține și `EF_*`, și `VC_*`; operatorul: la fel în orice unitate (D11) | nu e nevoie de apel între servicii; DDL-ul nou se rulează pe `AVACONT_SURSA` și se sincronizează în toate bazele |
| Furnizor comun | `VC_NIR.IdPartener` → `Parteneri`; `EF_Primite.CuiNormalizat` | legătura se face prin codul fiscal normalizat, ca în KE |
| Conturile Vercon | `VC_Masini` (`IdMasina`, `Masina`, `TIP`, `Activ`, cheie pe `SubunitId`+`GestiuneId`), `VC_Gestiuni`, `VC_Subunitati` | un cont aparține unei gestiuni; `receipt_destinations.catalog()` deja construiește „destinații + conturi” (cu filtrul `warehouse(tip gestiune, TIP cont)`); **dar** sare peste MF/OI din afara contextului curent, deci nu e bun ca atare pentru K-BOT |
| NIR | `VC_NIR`, `VC_Intrari` | contul liniei se deduce prin produs (`Intrari → ProduseP → Produse → Masini`); nu există coloană de cont pe linie |
| Editorul „Adăugare NIR” | `FACTURI_NIR_IMPLEMENTARE.md`, `receipt_distribution.py`, `receipt-allocation.js`, `docs.js` | **un singur NIR poate avea mai multe conturi bifate deodată** (în cod: `draft.accounts` este o listă; câmpul „Categorie / cont” este un ComboTree cu `setCheckedLeaves`; conturile cu aceeași rădăcină rămân în același NIR); produsele se aleg în ComboTree pe conturile bifate; **doar conturile din destinații diferite dau NIR-uri diferite, create într-o singură salvare atomică, cu totalul global al facturii păstrat**; total exact față de valoarea facturii |
| Facturi interne Vercon | `VC_FACTURI*`, `VC_NIR_FACTURI` | nu sunt destinația copierii (§4) |
| Soluția DGV din KE11 | `KeLegaForm` (`gridLinii`), `SLICE-KE11-…` | `KBotDataView` grupat pe factură; `GroupHeaderCombo` + `GroupHeaderValuePicked`; `CommitOnSelect`; combo pe celulă prin `ComboItems` |
| Arbore în celulă | **nu există** în `KBot.Controls` (`Combo/` are doar `KBotComboBox` cu listă plată) | trebuie creat: `KBotTreeComboBox` (§5, EV02) |
| Rol de scriere | `PYTHON/routes/efactura/primite_ke_routes.py` | scrie doar rolul `Contabil` (403 `ROL_INTERZIS`), verificat pe server; refolosim regula |
| Drepturi Vercon | `PYTHON/routes/vercon/operations.py` | `VR_RECEPTIE` pentru NIR; K-BOT nu are nevoie de drepturi Vercon (citește doar numele conturilor) |

## 4. Principiul și modelul de date

**Coadă de preluare, nu copie.** Factura rămâne o singură dată, în `EF_Primite`. O trimitere spune „factura Y,
cu conturile propuse pe linii, este gata de preluat în Vercon”. Administratorul o preia într-un „Adăugare NIR”; dacă
conturile liniilor sunt din gestiuni diferite, editorul existent creează mai multe NIR-uri într-o singură salvare
(D12), cu totalul global al facturii. La salvare, trimiterea se închide cu NIR-urile create.

DDL (rulat de mână pe `AVACONT_SURSA`, apoi sincronizare către unități; utf8mb3 / `utf8mb3_general_ci`; **nu** îl
rulează aplicația):

`EF_PrimiteVercon` — trimiterea (una activă pe factură)

| Coloană | Rol |
|---------|-----|
| `IdTrimitere` | cheie |
| `IdPrimita` | factura (FK `EF_Primite`, cascade) |
| `Stare` | `TRIMISA`, `PRELUATA`, `RETURNATA`, `RETRASA` |
| `UnTrimis`, `DataTrimis`, `Observatie` | contabilul, momentul, nota pentru Administrator |
| `MotivReturnare`, `UnReturnat`, `DataReturnat` | returnarea de către Administrator |
| `UnPreluat`, `DataPreluat` | cine a preluat |

`EF_PrimiteVerconLinii` — contul propus pe fiecare linie

| Coloană | Rol |
|---------|-----|
| `IdTrimitere`, `IdLinie` | trimiterea și linia facturii (`EF_PrimiteLinii`); **toate** liniile facturii au rând (D10) |
| `IdMasina` | contul ales de contabil; **NULL = contul îl alege Administratorul** |

`EF_PrimiteVerconNir` — NIR-urile rezultate (0..n pe trimitere)

| Coloană | Rol |
|---------|-----|
| `IdTrimitere` | trimiterea |
| `SubunitId`, `GestiuneId`, `ExercitiuId`, `IdIntrare` | NIR-ul creat (FK către `VC_NIR`) |

Reguli:
- cel mult **o trimitere activă** (`TRIMISA` / `PRELUATA`) pe factură; istoricul (`RETURNATA` / `RETRASA`) rămâne ca rânduri;
- la trimitere se verifică duplicatul (același furnizor + `NrFact` + `DataFact` deja în `VC_NIR`): avertisment, nu blocare;
- **anulare automată (D5):** starea efectivă se calculează **la citire**: o trimitere `PRELUATA` care nu mai are
  niciun NIR neanulat (`VC_NIR.Anulat = 1` pe toate) este tratată ca `TRIMISA` și reapare în coadă. Nu depindem de
  fiecare cale prin care se anulează un NIR. Caz de limită, de fixat la EV05: o trimitere cu mai multe NIR-uri din
  care **doar unul** e anulat; propunere: factura revine în coadă, iar editorul arată ce NIR-uri din ea sunt deja
  valide și preia numai liniile rămase fără NIR.

## 5. Felii

Fiecare felie: worklog, rând în status, ajutor (K-BOT: `0000-NN` cu eticheta feliei; ajutorul web Vercon pentru
partea lui), build curat. **Fără teste, fără git, fără deploy** (regulile operatorului).

- **EV01 — modelul și rutele din K-BOT (server).** DDL `sql/EV_01_…sql`; `PYTHON/routes/efactura/primite_vercon.py`
  + `primite_vercon_routes.py` (perechea lui `primite_ke*.py`):
  - `GET  /api/efactura/vercon/conturi` — arborele pentru K-BOT: toate gestiunile active (și MF/OI) cu conturile
    active; se extrage din `receipt_destinations.catalog()` o funcție comună, fără filtrul „context curent”,
    cu aceeași regulă `warehouse(tip, TIP)`;
  - `GET  /api/efactura/primite/<id>/vercon` — trimiterea facturii, conturile pe linii și starea efectivă;
  - `POST /api/efactura/primite/<id>/vercon` — trimite: toate liniile cu contul lor (opțional), observație; doar
    `Contabil`;
  - `PUT  /api/efactura/primite/<id>/vercon/conturi` — schimbă conturile propuse cât timp trimiterea e `TRIMISA`;
  - `DELETE /api/efactura/primite/<id>/vercon/<idTrimitere>` — retrage, doar cât timp e `TRIMISA`;
  - verificări: factură normală (nu storno), furnizor găsit în `Parteneri`, fără trimitere activă, contul activ.
- **EV02 — controlul „arbore în celulă” (`KBot.Controls`).** `KBotTreeComboBox` în `KBot.Controls/Combo/` (regula:
  toate controalele în `KBot.Controls`, familii): listă derulantă cu un `AdvancedTreeControl` în popup, rădăcini =
  gestiuni (nu se aleg), frunze = conturi; căutare prin tastare; `IThemedControl`; serializare de designer
  (zero proprietăți pe un control proaspăt); DPI logic. În `KBotDataView`: tip de coloană/celula „arbore”
  (`TreeItems`) și editorul de pe rândul de grupare (`GroupHeaderCombo` merge și cu arborele). Documentat în
  `KBotComboBox.md` / `KBotDataView.md`. Folosit în EV03; Vercon web are deja `ComboTree`, nu se atinge.
- **EV03 — K-BOT, interfața „Trimite la Vercon”.** Fereastră (sau pagină) după `KeLegaForm`, pentru una sau mai multe
  facturi bifate în listă:
  - DGV **grupat pe factură** (antet = „nr / dată” + total), coloane: „Denumire”, „Cantitate”, „Valoare cu TVA”,
    **„Cont Vercon”** (arborele, pe linie, opțional); fără bifă pe linii — se trimit toate (D10); pe rândul de
    grupare, același arbore: alegerea se scrie pe toate liniile facturii (ca „Clasificație” în KE11); alegerea pe linie
    suprascrie;
  - o notă pentru Administrator (opțională); nicio „gestiune implicită” (D12);
  - jos, rezumat informativ pe gestiune pentru liniile cu cont („Alimente — 3 linii — 1.240,00 lei”, „fără cont — 2
    linii”) și avertismentul de duplicat;
  - butoane în arborele și lista facturilor primite (doar pentru `Contabil`): „Trimite la Vercon…”, „Retrage”,
    „Modifică conturile” cât e `TRIMISA`; indicator de stare în arbore („Trimisă”, „Preluată în NIR nr. …”,
    „Returnată: motiv”); filtru în meniul arborelui.
  Ajutorul `0000-NN` cu eticheta feliei.
- **EV04 — Vercon, lista „Din e-Factura” (server + web).** Rute noi în `PYTHON/routes/vercon/` (ex.
  `efactura_queue.py`): lista trimiterilor `TRIMISA` (stare efectivă, §4) ale unității, **fără filtru pe gestiunea
  curentă** (D12), drept `VR_RECEPTIE`; detaliul unei trimiteri (antet + linii, cu contul ales de contabil acolo unde
  există); `POST …/returneaza` cu motiv obligatoriu. În „Adăugare NIR” (`docs.js`, `receipt-context.js`): buton/listă
  „Din e-Factura”. La alegere se precompletează furnizorul, numărul, data, **valoarea facturii (totalul facturii)**,
  **toate conturile alese de contabil pe linii, bifate odată în câmpul „Categorie / cont” (`setCheckedLeaves`)** — mai
  multe conturi pot fi într-un singur NIR — și liniile fără produs;
  selectorul de produs se restrânge la conturile precompletate. Administratorul alege produsul fiecărei linii,
  ajustează cantitățile și salvează prin fluxul existent (repartizarea pe mai multe NIR-uri, în aceeași salvare).
- **EV05 — închiderea buclei.** Salvarea NIR-ului (`receipt_distribution.py` / `receipts.py`), când cererea vine din
  coadă, scrie în aceeași tranzacție: `Stare = PRELUATA`, `UnPreluat`, `DataPreluat` și câte un rând în
  `EF_PrimiteVerconNir` pentru fiecare NIR creat. K-BOT arată „Preluată în NIR nr. …”; un NIR anulat redă factura în
  coadă (D5, la citire, cu cazul de limită din §4).
- **EV06 — numai la cerere:** facturi storno (flux FCS), sugestii de produs din legături anterioare (denumire →
  produs).

Ordinea: EV01 → EV02 → EV03 (de aici se poate trimite) → EV04 → EV05. EV06 la cerere.

## 6. Cum se comportă contul ales

- Contul din K-BOT este o **propunere** pentru Administrator (D9): îl vede precompletat în arborele de conturi al
  NIR-ului și produsele propuse sunt ale contului; îl poate schimba.
- Fără cont (NULL), Administratorul alege contul după produs, ca azi.
- Conturile inactivate între trimitere și preluare se semnalează în Vercon („contul nu mai e activ”) și
  Administratorul alege altul.
- Gestiunea nu mai e o alegere separată: rezultă din contul fiecărei linii, la preluare, prin editorul existent.
- Liniile cu **conturi diferite din aceeași gestiune** ajung într-un singur NIR (numărul editabil); numai conturile din
  gestiuni diferite dau NIR-uri diferite, create în aceeași salvare. Deci cazul obișnuit — o factură, o gestiune, câteva
  conturi — produce un singur NIR.

## 7. Riscuri

- **Fișiere deja modificate în lucru** (`doc_routes.py`, `receipt_context.py`, `docs.js`, `receipt-context.js` și altele
  apar modificate în git). EV04/EV05 ating exact zona NIR; înainte de a le porni se citește ce e deja în lucru, ca să
  nu se suprapună.
- **Control nou (`KBotTreeComboBox`).** Cel mai mare element nou al planului pe partea K-BOT; nu există nimic
  asemănător în `KBot.Controls`. Locul de risc: popup-ul cu arbore în celula DGV și pe rândul de grupare (ca și combo-ul
  plat din KE11, ne-văzut la acea vreme pe ecran).
- **MF/OI în arbore (D8).** Un NIR de MF/OI cere date în plus (clasificare MF, deținător OI). Precompletarea aduce
  doar furnizor/factură/linii; restul îl completează Administratorul. De verificat la EV04 că „Adăugare NIR” acceptă
  fluxul din coadă și pentru MF/OI (editorul lor are reguli proprii: `mf.py`, `oi.py`).
- **Linii fără produs.** Liniile facturii sunt text liber; contul din K-BOT restrânge lista de produse, dar nu alege
  produsul. Sugestiile automate vin abia în EV06.
- **Total exact.** Se trimit toate liniile (D10) și valoarea declarată pe NIR este totalul facturii (`Total`), deci
  regula „total egal cu valoarea facturii” se aplică neschimbat, inclusiv când editorul împarte factura pe mai multe
  NIR-uri. La facturile cu reducere pe toată factura (KE_PLAN §7.1) suma liniilor nu dă `Total`; diferența se arată,
  nu se ascunde.
- **Serverul și DDL-ul nu rulează singure.** Nimic nu se deploiază de către asistent; DDL-ul îl rulează operatorul.

## 8. Rămas de hotărât

Nimic care să blocheze pornirea. Un singur detaliu, fixat la EV05: comportamentul când dintr-o trimitere cu mai multe
NIR-uri se anulează doar unul (propunere în §4).

## 9. Ce NU intră în EV

- Contabilitatea (`EF_PrimiteOperatii`) și legătura cu plățile — rămân în KE / E-Factura.
- Orice modificare a importului FOREXE sau a migrării Access în Vercon.
- Modificări în `VC_FACTURI` și în regulile de calcul ale NIR-ului (se refolosesc neschimbate).
- Facturile emise (vânzare); facturile storno (EV06, la cerere).
