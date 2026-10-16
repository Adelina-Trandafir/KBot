# KE — Facturi primite ↔ Recepții ↔ Plăți (PLAN, nimic implementat)

Scris 16.10.2026, la cererea operatorului. Ce e verificat (citit într-un fișier) și ce e presupus sunt marcate.

**Stare (16.10.2026, după implementare):** KE01…KE07 sunt în cod; KE08 (plăți) nu e pornit. Detalii, ce e nerulat și ce rămâne
de verificat: [`docs/worklog/state/KBOT_STATUS_KE.md`](worklog/state/KBOT_STATUS_KE.md) și worklogurile `SLICE-KEnn-*.md`. Restul documentului este
planul de dinainte de implementare; unde codul diferă, hotărăște codul (de ex. fluxul de modificare caută recepția după dată + sumă +
marcaj, nu după `RangZi`).

**Decizie 16.10.2026 (recepții în aceeași zi):** K-BOT nu interzice a doua recepție în aceeași zi pe un angajament. Ce scrie K-BOT poartă
marcajul `(IDRH; IDR)`, care îl identifică, deci ziua nu contează; departajarea după `RangZi` nu se construiește (rămâne doar pentru
recepții vechi, fără marcaj, dacă va fi nevoie). Blocarea este **opțională**: setarea «La o factură primită, nu permite a doua recepție
nouă în aceeași zi» din «Setări» › «Aplicație» (oprită implicit), verificată în fereastra «Recepție nouă».

Felia **KE** e o familie nouă, în K-BOT (nu registru separat ca VC / ADE): rânduri în indexul din
`docs/worklog/KBOT_STATUS.md`, detalii într-un fișier propriu din `docs/worklog/state/`
(`KBOT_STATUS_KE.md`, ca `KBOT_STATUS_SLICELESS.md`), worklog-uri `SLICE-KEnn-…`. Numerotare KE01, KE02…
Combină E-Factura cu Recepțiile și Plățile, dar rămâne K-BOT.

## 1. Cerința, în cuvintele operatorului

O factură primită se plătește la un moment dat, integral sau parțial. Pentru asta, factura trebuie
legată de o **recepție**:

- **recepție existentă** (se poate modifica dacă valoarea ei nu ajunge pentru plată);
- **recepție nouă**, implicit cu valoarea facturii, sau cu o valoare parțială.

Legarea de **plăți** și ordinele de plată vin după (KE08). Acum doar facturi ↔ recepții.

## 2. Reguli fixate de operator (16.10.2026)

1. **O factură are de regulă o singură clasificație, dar nu e garantat: fiecare linie a facturii
   aparține unei clasificații.** Alocarea se face pe linie / clasificație, nu pe factura întreagă.
1b. **Recepția se face cu valoarea CU TVA (suma de plată).** Valoarea unei linii pentru recepție =
   baza liniei + TVA-ul ei; suma pe clasificație = suma valorilor cu TVA ale liniilor ei. Restul
   facturii se calculează față de `Total` (suma de plată), nu față de `Valoare` (baza).
2. **Clasificația recepției vine din `FX_Indicatori`.** Este obligatoriu ca ea să existe acolo
   *înaintea* recepției (legea). Clasificațiile ajung în `FX_Indicatori` printr-o **revizie nouă de
   rezervare**, încărcată în FOREXE (`Incarca Rezervare`, indicator `!NOU`) — calea din slice 0081.
3. **Un furnizor poate avea mai multe angajamente.** Dacă factura nu are niciun angajament asociat
   furnizorului, operatorul poate **alege un angajament** direct.
4. **Storno = o recepție normală cu semn negativ.** După salvarea unei recepții cu semn negativ
   (nouă sau modificată), diferența dintre recepții și `FX_Plati` crește. Nu există regulă separată.
5. **Modificarea unei recepții** se face pe același formular ca adăugarea (după ochi). Nu există
   câmpuri în plus la «Modifică». Greul este **identificarea rândului pe al cărui ochi apăsăm**.
6. **Nu există «propuneri» sau ciorne.** K-BOT scrie recepția în FOREXE prin robot, descarcă din nou,
   iar legătura cu factura se înregistrează pe recepția rezultată.

## 3. Ce există azi (verificat)

| Ce | Unde | Fapt |
|----|------|------|
| Factura primită | `EF_Primite` (schema live, `MariaDB_Schema/AVACONT_SURSA.sql`) | `Total` (cu `Semn`), `Valoare`, `TVA`, `CuiNormalizat`, `NrFact`, `DataFact`, `DataScad`, `Tip` (FC / storno), `IdPrimitaRef` |
| Liniile | `EF_PrimiteLinii` | `Denumire`, `Cpv`, `Cant`, `Pret`, `Valoare`. **Nu are clasificație și nu are cotă TVA pe linie** |
| TVA | `EF_PrimiteTVA` | pe cote, la nivel de factură |
| Legătura cu DDF | `EF_PrimiteAsocieri` (`IdPrimita`, `IDDF`), unică pe pereche | automată după `FX_DDF_Parteneri.CodFiscal` = `CuiNormalizat` (toate DDF-urile furnizorului), sau manuală; rutele în `PYTHON/routes/efactura/primite.py` |
| Prelucrare contabilă | `EF_PrimiteOperatii` | doar «prelucrat în contabilitate»; **nu are legătură cu plata**, nu se amestecă |
| Recepția | `FX_Receptii_R` (identitate, `DataR`, `SumaAntet`, `NrCrtForexe`, `RangZi`, `Sters`), `FX_Receptii_H` (instantanee, `Total`), `FX_Receptii` (linii pe `CodAI`/`IdClsf`, `Valoare`, `SumaPlatita`) | `FX_Receptii.CodAI` → `FX_Indicatori` |
| Plata | `FX_Plati` | pe `CodAI`, `Suma`, `NrOP`, `Data_plata` |
| Indicatori | `FX_Indicatori` | `CodAI`, `CodAngajament`, `IdClsf`, `Angajament_Legal`, `Receptii`, `Plati` |
| Angajament ↔ DDF | `FX_DDF.CodAngajament` (NOT NULL), `FX_Angajamente.IDDF` | un angajament fără DDF în K-BOT nu are partener |
| Marcajul de recepție | `routes/forexe/marcaj.py`, `IMarcajApi.vb` | când se salvează în pagina FOREXE, meniul K-BOT adaugă în *Descriere* textul `(IDRH: n; IDR: m)`; numerele se rezervă pe server (`POST /api/forexe/marcaj/rezerva`, tip `receptie`, o pereche nouă la fiecare salvare, valabilă 2 zile în `FX_NumberLock`); FOREXE copiază textul în istoric, iar descărcarea știe direct cui aparține rândul (`prelucrare_pasi.py` 4a) |
| Potrivirea fără marcaj | `prelucrare_pasi.py` | ordine pe zi: întâi `RangZi`, apoi valoarea; la rânduri vechi (fără `RangZi`) valoarea exactă; la fiecare descărcare K-BOT pune `NrCrtForexe` (poziția în lista FOREXE) și `RangZi` pe rânduri |
| Data recepției | `asociere.py` (F29) | `DataR` e un câmp tastat pe site și schimbabil, **nu e o dovadă a identității** |
| Adăugare recepție în FOREXE | `Workflows/Creare/adlop - Rezervare si Receptie.wfl`, liniile 80–113 | tab «Recepții» → «Adaugă» → tip, dată, descriere, valoare pe fiecare indicator (după «Sector - Sursa - Indicator») → captură → «Salvează». **Nu așteaptă mesajul de reușită.** Nu e în `WorkflowCatalog`, nu rulează din K-BOT, nu e verificat. Primele 78 de linii fac rezervare, nu intră în KE |
| Modificare recepție | `Receptie Editata.wfl` (doar citire, ochiul rândului) | formularul de scriere = același ca «Adaugă» (operatorul), dar fără flux înregistrat |

**Tipul recepției:** «Avans», «Partial», «Final» (preselectat «Final») — HTML-ul `select` dat de operator.
Mesajul de succes: același element ca la orice salvare corectă (operatorul).

## 4. Model de date (KE01)

Un tabel nou `EF_PrimiteReceptii`, mulți-la-mulți, granularitate pe clasificație:

| Coloană | Rol |
|---------|-----|
| `IdLegatura` | cheie |
| `IdPrimita` | factura (FK `EF_Primite`, cascade) |
| `CodAngajament` | angajamentul recepției (FK `FX_Angajamente`) |
| `IdClsf` | clasificația (`Clasificatii.IDClsf`, ca pe celelalte tabele FX, fără FK) |
| `IDRR` | recepția (identitatea); NULL până la descărcare dacă marcajul nu a fost folosit |
| `IDRH`, `IDR` | numerele rezervate prin marcaj (pot fi cunoscute înainte de salvare) |
| `Suma` | valoarea alocată; negativă pentru storno |
| `UN`, `DataAdaugare` | cine / când |

Plus două coloane noi pe `EF_PrimiteLinii`:

- `IdClsf` — alegerea operatorului pe linie, NULL = nealeasă;
- `CotaTVA` — cota liniei, citită din XML-ul facturii (`InvoiceLine/Item/ClassifiedTaxCategory/Percent`,
  presupus, de verificat pe cele 3 XML-uri reale din 00EF-17). Cititorul UBL (`primite_ubl.py`) nu o
  păstrează azi; facturile deja sincronizate trebuie recitite din XML-ul stocat (vederea «XML Factura»
  arată că XML-ul se păstrează).

**Valoarea unei linii cu TVA** (`ValoareCuTVA`) se calculează pe server, nu se stochează:
`Valoare × (1 + CotaTVA / 100)`, rotunjită la 2 zecimale pe linie. Diferența de rotunjire față de TVA-ul
facturii pe cota respectivă (`EF_PrimiteTVA.TVA`) se pune pe ultima linie a cotei, ca suma pe cote să
iasă exact și suma totală să fie `Total`. **Exemplu verificat pe XML-ul Digi** (`FDB26 78290032`, 06.10.2026): linia 1.1 are baza 53,31, linia 1.2
are baza 10,66, ambele la 21%; TVA-ul facturii e 13,43, iar `Total` 77,40. Pe linie: 53,31 + 11,20 =
64,51 și 10,66 + 2,24 = 12,90, adică 77,41, cu un ban peste. Regula de mai sus mută banul pe ultima
linie: 12,89. Rezultat: 64,51 + 12,89 = 77,40 = `Total`. Cele două linii au același CPV (64200000-8), dar CPV nu
este clasificația bugetară: aceasta o alege operatorul pe linie (KE02).

Dacă o linie nu are cotă în XML: se folosește singura cotă a
facturii, iar dacă sunt mai multe, TVA-ul cotelor se împarte proporțional cu baza și linia se marchează
«cotă dedusă» în interfață.

Calcule (pe server, nu pe client):

- **rest de alocat pe factură** = `Total × Semn` − `SUM(Suma)`, pe clasificație dacă liniile o au;
- **rest pe recepție** = valoarea recepției − `SUM(Suma)` alocată de alte facturi;
- **stare factură**: «nelegată» / «parțial» / «acoperită» / «depășită».

Plata (KE08): se leagă de recepție, nu de factură. Starea de plată a facturii se deduce din plățile
recepțiilor ei. `FX_Receptii.SumaPlatita` există deja.

DDL: rulat de mână pe `AVACONT_SURSA`, apoi sincronizarea schemei către bazele unităților, ca la
00EF-17. Un fișier `sql/KE_01_….sql`, utf8mb3 / `utf8mb3_general_ci`.

## 5. Felii

Fiecare felie: worklog `SLICE-KEnn-…`, rând în status, ajutor (`0000-NN` cu eticheta KEnn), build
curat. Fără teste și fără git (regulile operatorului), dacă nu se cere altfel.

- **KE01 — modelul de date și calculele.** Tabelul, coloanele pe linii (`IdClsf`, `CotaTVA`), citirea
  cotei din XML (cititor + recitirea facturilor deja sincronizate), `ValoareCuTVA` pe linie, rutele de
  citire a restului.
- **KE02 — angajamentul și clasificația pe liniile facturii.**
  - Angajamentul: din DDF-urile asociate; dacă nu există, operatorul alege un angajament (selector
    asemănător `AlegeDdfForm`). Un furnizor cu mai multe angajamente: lista tuturor.
  - Clasificația pe linie: automat dacă angajamentul are un singur indicator, altfel o alege
    operatorul din indicatorii angajamentului (`FX_Indicatori`). Vederea «Continut»
    (`PrimiteLiniiPage`) primește coloanele «Clasificație» și «Valoare cu TVA», plus totaluri pe
    clasificație (ele devin valorile implicite ale recepției).
  - Clasificație care nu e în `FX_Indicatori`: mesaj «lipsește indicatorul; trebuie întâi o revizie
    nouă de rezervare», cu trimitere la calea DDF existentă (0081). Aici K-BOT nu trimite nimic.
- **KE03 — fila «Recepții» în factura primită, doar citire.** Recepții legate, rest de alocat, candidate
  (recepțiile angajamentului cu rest). Stare vizibilă și în arborele facturilor.
- **KE04 — legare la o recepție existentă.** Dialog cu candidatele; suma implicită
  `min(rest factură, rest recepție)`, reductibilă. Scrie doar `EF_PrimiteReceptii`. Dacă recepția nu
  ajunge: două ieșiri, «Modifică recepția» (KE07) sau «Recepție nouă pentru diferență» (KE06).
- **KE05 — fluxul «Adaugă recepție» curat.** Flux propriu (`Creare/…Receptie Noua.wfl`), scos din
  «Rezervare si Receptie» fără pașii de rezervare; variabile `COD_ANGAJAMENT`, `TIP_RECEPTIE`,
  `DATA_RECEPTIE`, `DESCRIERE_RECEPTIE`, `DATE_RECEPTIE`. Pași noi: marcajul în descriere (vezi mai jos)
  și așteptarea mesajului de reușită (de aflat). Intră în `WorkflowCatalog`. Verificat întâi pe un
  angajament de test; nimic nu pleacă pe un angajament viu până nu e văzut.
- **KE06 — «Recepție nouă» din factură.** Dialog precompletat: angajament, dată (implicit data
  facturii; se poate schimba), descriere («Factura NR/DATA …»), valoare pe indicator (implicit restul
  facturii, sau parțial), **verificări înainte de trimitere**: indicatorul există în `FX_Indicatori`;
  valoarea nu depășește rezervarea (`Angajament_Legal` − recepții deja făcute, de verificat formula);
  semn negativ pentru storno. Apoi: cere marcajul → rulează KE05 → descarcă angajamentul → legătura se
  confirmă pe recepția rezultată.
- **KE07 — «Modifică recepția».** Identificarea rândului (§6), același formular ca la adăugare, valoare
  nouă, motiv obligatoriu (ca `CorectatMotiv`), marcaj nou (fiecare salvare = instantaneu nou).
- **KE08 — plăți și ordine de plată.** Nepornit; se decide după KE01–KE07. Partea de ordine de plată e
  legată de `FUNDAMENT_Ordonantare_CAB.md` și de editorul 0049.

## 6. Identificarea recepției pe care apăsăm ochiul (KE07)

1. **Recepție salvată prin K-BOT** (are marcaj `(IDRH; IDR)` în Descriere): robotul caută pe site
   rândul a cărui descriere conține marcajul. Sigur, indiferent de dată.
2. **Recepție creată prin KE05:** marcajul e pus de KE05 din start (aceeași rută de rezervare), deci
   cazul 1 se aplică și aici, iar legătura cu factura se poate scrie *înainte* de descărcare.
3. **Recepție fără marcaj** (veche, creată pe site): se caută ca la descărcare, cu `NrCrtForexe` și
   `RangZi` din `FX_Receptii_R` (cititura de la ultima descărcare), `DataR` și `SumaAntet`. Dacă
   citirea e veche, se face întâi o descărcare a angajamentului. `DataR` singură nu identifică.
4. **Plasa de siguranță (ambele cazuri):** după ce ochiul deschide formularul, robotul citește tipul,
   data, descrierea și valorile și le compară cu ce știe K-BOT. **Dacă nu coincid, nu salvează**: iese
   cu mesaj «Nu am găsit recepția sigur», fără nicio modificare pe site.

Atenție la regula din `asociere.py`: legăturile instantaneelor nu se mai pot edita după o ordonanțare
din ziua instantaneului. Nu e o piedică pentru o modificare nouă (creează un instantaneu nou), dar KE07
trebuie să citească ce spune FOREXE despre recepții deja ordonanțate.

## 7. Decizii deschise

1. ~~TVA pe linie~~ — **decis 16.10.2026: recepția se face cu TVA** (suma de plată). Verificat pe un XML
   real (factura Digi, 77,40 lei): fiecare linie are `Item/ClassifiedTaxCategory/Percent`, deci cota pe
   linie se poate citi. Rămâne un singur lucru de verificat: facturile cu **reducere sau majorare pe
   toată factura** (în UBL, un `AllowanceCharge` în afara liniilor, cu bază și TVA proprii). La ele,
   suma liniilor nu mai dă `Total`. Dacă apare așa ceva, diferența se arată ca «rest nealocat» pe factură
   și operatorul o vede; nu o împărțim pe linii pe ascuns. XML-ul Digi nu are așa ceva.
2. ~~Tipul recepției~~ — **decis 16.10.2026.** `select[name='receptie']` are trei opțiuni (HTML dat de
   operator): «Avans», «Partial», «Final», cu valori GUID; pe site e preselectat «Final». **K-BOT propune
   implicit «Final»**, la fel ca site-ul; operatorul poate schimba în dialog. Fluxul alege după **text**
   (`Select text=`), nu după GUID, deci TIP_RECEPTIE ia exact una dintre cele trei denumiri.
3. ~~Mesajul de reușită~~ — **decis 16.10.2026:** e același element ca la orice salvare corectă
   (`.alert.alert-success`, ca în celelalte fluxuri). KE05 / KE07 așteaptă acel element, fără text.
   **După fiecare salvare, apare sau nu mesajul, fluxul citește istoricul** angajamentului ca să preia
   rândurile nou adăugate. **Doar dacă mesajul NU a apărut**, fluxul face în plus o recitire a listei de
   recepții și caută recepția (după marcaj) ca să afle dacă s-a salvat totuși. Legătura cu factura se
   scrie numai dacă recepția a apărut (mesaj văzut, sau găsită la recitire). Dacă mesajul lipsește și
   recepția nu se găsește, nu se scrie nimic și operatorul primește mesajul «Nu s-a putut confirma
   salvarea recepției».
4. **Factură pe mai multe angajamente** (rar): permisă de model (`CodAngajament` pe rândul legăturii),
   dar interfața din KE02 începe cu un angajament pe factură. De confirmat.
5. **Rezervare insuficientă pentru o clasificație existentă:** trimite spre revizie de rezervare
   (0081). K-BOT nu face revizia automat din KE.
6. ~~Cine poate lega~~ — **decis 16.10.2026: doar rolul `Contabil`** (`Unitati_Utilizatori.Rol`). Cine
   nu e `Contabil` (de ex. `Director`, `Administrator`) vede legăturile, restul de alocat și recepțiile
   din factură, dar **nu poate** lega, crea sau modifica recepții: butoanele nu apar. Regula se aplică și
   **pe server**, în fiecare rută de scriere din KE (un client modificat nu o ocolește). **Neverificat:**
   cum ajunge rolul în sesiune (`SessionContext` / `require_session`) — se citește la KE01; dacă sesiunea
   nu îl poartă, se adaugă acolo, nu se citește din altă parte.

## 8. Riscuri

- Fluxul de scriere în FOREXE (KE05 / KE07) e partea cea mai riscantă: nu a rulat niciodată din K-BOT.
  Se face ultimul, pe angajament de test, cu verificarea rezultatului.
- Marcajul rezervat expiră în 2 zile (`FX_NumberLock`): o recepție trimisă și descărcată târziu pierde
  marcajul. KE06 trebuie să descarce imediat după trimitere.
- Legătura scrisă înainte de descărcare trebuie anulată dacă salvarea pe site eșuează (vezi regula de
  confirmare de la §7, punctul 3).

## 9. Ce NU intră în KE

- Contabilitatea (`EF_PrimiteOperatii`) și orice nu e legat de plată.
- Modificări în importul FOREXE → K-BOT (descărcare, asociere) — rămân neatinse.
- Trimiterea revizilor de rezervare — rămâne la slice 0081.
