# Harta Access -> K-BOT: Avacont, etapa 1 (AV01)

Fise de citire (fara cod). Fiecare fisa are: ce face operatorul, date, reguli de pastrat,
mecanisme Access de lasat, suprapuneri cu K-BOT, propunere PORT/EXISTA/ELIMINAT/DE DECIS,
intrebari. Marcaj in fise: VERIFICAT = citit intr-un fisier numit; PRESUPUS = dedus.

| Fisa | Buton ribbon Access | Stare K-BOT | Eticheta propusa |
|---|---|---|---|
| [conturi-plan-de-conturi](conturi-plan-de-conturi.md) | Conturi | lipseste | PORT |
| [jurnale](jurnale.md) | Jurnale | lipseste | PORT (mic) |
| [clasificatii-venituri](clasificatii-venituri.md) | Clasificatii Venituri | **INCHIS 26.10.2026** (AV01-11 / AV01-12: pagina «Venituri» din fereastra «Clasificatii», pe sursa din bara de titlu) | PORTAT |
| [sablon-operatii-oci](sablon-operatii-oci.md) | Sablon materiale / servicii / diverse | lipseste | PORT, proiectat nou (logica da, mecanismul nu) |
| [sablon-operatii-ocs](sablon-operatii-ocs.md) | Sablon operatii salarii | lipseste | PORT, proiectat nou; **nu se salveaza articol fara minim un rand PC** |
| [setari-implicite-si-bife](setari-implicite-si-bife.md) | Configurari Avacont + 4 bife | partial (E-Factura, date unitate exista) | pagina «Avacont» in Setari > Aplicatie, nu formular separat |
| [parteneri-ca-analitic](parteneri-ca-analitic.md) | Parteneri | exista ecran pentru furnizori; analiticele nu | DE DECIS (partener = furnizor SAU analitic) |

Clasificatii (cheltuieli): EXISTA (doar verificare de lipsuri, fara fisa noua). Parteneri: NU mai e «EXISTA» - vezi fisa.
ELIMINAT (decis de operator): Adauga/Incarca/Transfera unitate, Backup online + Descarca
actualizare, bifele «Stergere conturi» si «Mod Noapte».

## Constatari care schimba planul

- CORECTIE: `PlanCont`, `DefaCont`, `Jurnale`, `DefaNote`, `DefaNoteA`, `DefaSalarii*`, `Parteneri` NU sunt tabele
  legate: sunt **locale** in `AVACONT/DATABASES/baza2026.accdb` (69 de tabele locale, 0 legate). Lipseau doar din
  `EXPORT_VBA/tables` (alt fisier, 275 tabele). Structura si cifrele din fise sunt acum citite din `baza2026.accdb`
  (copie, read-only; originalul neatins). `DefaClase` si `DefaClsfV` nu sunt in acel fisier, dar sunt exportate ca date in
  `AVACONT/EXPORT_VBA/tabledata/` (132 CSV-uri cu datele tabelelor locale): `DefaClase.csv` = 8 clase (1 CAPITALURI ...
  8 CONTURI EXTRABILANTIERE), `DefaClsfV.csv` = 1404 randuri (ID, C, S, P, D = denumire, CV, ...).
- In Access toate relatiile sunt cu **Update + Delete in cascada**: stergerea unui cont sau jurnal sterge liniile
  care il folosesc. In K-BOT: **stergere BLOCATA** (decis de operator pentru conturi si jurnale), legaturi `RESTRICT`.
- Partener nu inseamna doar furnizor: in `baza2026.accdb` 32 din 75 de parteneri n-au cod fiscal, iar 13 din cele 20 de
  coduri folosite in note sunt fara cod fiscal (analitice: personal, asigurari, retineri).
- `DefaNoteA.CodPartener` are trei continuturi (cheie clasificatie / cod partener / `XXX`) - vezi fisa OCI.
- Fereastra Access de Clasificatii Venituri stie practic doar sa adauge; modificarea si stergerea
  sunt cod mort, iar butoanele de golire/preluare buget lucreaza pe tabelele de cheltuieli.
- Setarile din K-BOT se salveaza azi **per utilizator Windows** (`app_settings.json`), fara API;
  setarile per unitate cer o ruta noua de scriere pe server.
- Sabloane: VA = Retineri, PC = Viramente, PN = Plati cu numerar. Contul din sablon e stocat REZOLVAT
  (cu sector si sursa ale unitatii), deci sablonul nu e reutilizabil la alt sector/sursa.

## Decizii luate de operator (23.10.2026)

- Plan de conturi: **pe unitate**; nomenclatoarele (`DefaCont`, `DefaClsfE`, `DefaClsfF`...) se citesc din `AVACONT_COMUN`.
- Clasificatii Venituri: **identic cu clasificatiile normale** (versiuni de buget, sume zecimale, modificare/stergere ca la
  cheltuieli). Vezi fisa: azi `Clasificatii_Venituri` nu are An/versiuni si are sume intregi, deci se aduce la modelul normal.
- Setari Avacont: **per utilizator**. «Stergere N.C.» si «Stergere clasificatii» devin **drept de rol**, nu bife.
- Jurnale: **pe unitate**. Cerinta noua: **subunitati**, ca la VERCON (web). (Vezi intrebarea S mai jos.)
- Parteneri: `Tip` NU se portheaza; `ContPl` NU se adauga; informatiile unui analitic = denumire, adresa, IBAN, banca.
- Stergere BLOCATA pentru conturi si jurnale cu operatii; sablon salarii: minim un rand PC.
- **S1:** SID/SIC (solduri initiale) stau **separat**, in «solduri initiale» (nu in planul de conturi), langa `ParteneriSI`.
- **S2:** sablonul tine **doar contul de baza**; sectorul, sursa si clasificatia se completeaza automat din operatia curenta
  (regula de compunere `MakeCont`, vezi fisa OCI A1). Deci sablonul devine reutilizabil la alt sector/sursa.
- **S3:** partenerul pe o linie de sablon poate fi **niciunul / cel al operatiei / unul fix** (toate trei).
- **S7:** o **subunitate = alta unitate subordonata**, cu contabilitatea ei, a aceleiasi unitati parinte (ca VERCON:
  DC -> subunitate). Impact: fiecare tabel contabil are nevoie de cheia subunitatii; se proiecteaza inainte de DDL.
- (CM = concediu medical, cont 423: VERIFICAT in tooltip-ul din `Forms\OCS_header`; nu mai e intrebare.)

- **S4 (`Chelt`) - DECIS de operator (23.10.2026): bifa `Oper.Chelt` RAMANE si inseamna «operatia este Cheltuiala Efectiva».
  Cand e bifata, suma se aseaza intr-o coloana anume din unele rapoarte (fisa bugetara, Anexa7...).** In fisa de citire a
  rapoartelor (etapa 6) se trasaza exact care rapoarte si ce coloana; verificarea mea in cod (de mai jos) nu a gasit
  interogarea Anexa7/fisa bugetara care o citeste direct - de cautat prin campuri calculate/alte interogari la etapa 6.
  Constatari anterioare din cod: este campul `Chelt` (Da/Nu) din tabela `Oper` (`baza2026.accdb`).
  Se scrie din linia de sablon (`DefaNote.Chelt` -> `Oper.Chelt`, queries `SAV_OPER_EV*`/`SAV_OPER_PL*`, `mdl_Salvare_NoteContabile`),
  din ecranul Note, iar la postarea salariilor este fortat `False` (`SalariiSavDoc2019_code.txt`, linia 376, in blocul `.AddNew`
  cu coloanele din Oper). VERIFICAT ca se afiseaza: raportul `NotaContabila2` are o coloana «Chelt» cu `=IIf([Chelt],"X","")`,
  iar `OrdonantarePlata1280` o preia din `TmpOrd.Chelt`; ecranul Note foloseste `Chelt.Tag` ca sursa listei de parteneri.
  NU am gasit un raport care sa SUME sau sa FILTRAMA dupa el (in prima varianta a acestei note am numit gresit Anexa7,
  FiseBugetareCentralizare, InchidereChelt - potriveau doar subsiruri din `ContChelt`/numele interogarii). Sensul («linia e
  cheltuiala») ramane PRESUPUS dupa coloana tiparita; se pastreaza ca marcaj pe operatie si se decide la etapa 6 (rapoarte).
- **S5 (`Zec2`):** NU e rotunjire. E comutatorul «salarii cu 2 zecimale» pentru proiectele UE (altfel fara zecimale).
  Se leaga de **subunitate**, nu de o bifa pe ecran. Randurile PN (ridicare numerar -> casa -> plata) se genereaza automat.
- **S6 («Protectie operatii»):** nu e o setare generala, ci o regula a ecranului «Note» (note introduse manual, fara
  sabloane): cand alegi o clasificatie, lista de conturi arata DOAR conturile care au in ele acea clasificatie si pe
  cele care nu au deloc clasificatie. Exemplu dat de operator: la `650402200101` apar `121000002A` si
  `4010102A650402200101`, dar nu `4010102A650402201010`. In Access era fortata pe Da la fiecare deschidere. **Se pastreaza ca
  regula a ecranului Note (activa implicit); nu devine setare in pagina Avacont** - daca vrei sa o poti opri, spune.
- **S8:** analiticele de parteneri sunt **proprii fiecarei unitati**.
- **Capitol / sector-sursa (raspuns operator):** **subunitatea hotaraste capitolul** unitatii, iar capitolul da sectorul si
  sursa: `65.01` = `01A`, `65.02` = `02A`, `65.10` = `02E`, `65.11` = `01A` (exemplele operatorului, o unitate cu 65.11).
  Consecinta: capitolul (si implicit SS) se ia din subunitate; articolele de salarii se definesc deci **per subunitate**
  (fiecare cu capitolul ei), nu o singura lista pe unitate. Maparea capitol -> SS ramane o configurare, nu cod fix.

## Intrebari ramase

Cele 8 decizii mari ale etapei 1 sunt luate. Raman intrebari de DETALIU in fise (ex. stergerea articolelor de sablon
deja folosite, cod fiscal obligatoriu la parteneri, sensul `Tip = M` la antetul de sablon); se lamuresc in
felia de proiectare, cand se scriu tabelele. Urmeaza: proiectul de date (inclusiv cheia de subunitate) si, dupa
confirmarea ta, codul etapei 1.

Intrebarile complete sunt la finalul fiecarei fise.

- De facut (nu critic): interfata pentru adaugarea subunitatilor - [plan-interfata-subunitati](plan-interfata-subunitati.md).
