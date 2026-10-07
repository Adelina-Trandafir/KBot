# ADECHIT — STATUS, felii ADE0–ADE9

Detalii pentru [indexul din rădăcină](../../../ADECHIT_STATUS.md).
Actualizat: 07.10.2026. Numerele de mai jos sunt alocate; implementarea nu a început.
Subfeliile sunt planificate, cu excepția ADE0-01, documentată local.

## Slice ADE0

### Registry

| Subfelie | Livrabil | Stare | Worklog |
|---|---|---|---|
| SLICE-ADE0-01 | Analiza inițială, deciziile utilizatorului și planul | DOCUMENTAT LOCAL | [Plan inițial](../SLICE-ADE0-01-plan-initial.md) |
| SLICE-ADE0-02 | Matrice formular → acțiune → interogare → tabel; date persistente/temporare/excluse; contractul calculelor | PLANIFICAT | — |

### Current focus

- Următoarea lucrare: ADE0-02. Următoarea subfelie liberă: **ADE0-03**.
- Inventar inițial citit: 66 tabele, 27 interogări, 44 formulare, 18 rapoarte,
  34 module, 7 clase, 5 macrocomenzi. Inventarul nu dovedește că toate sunt active.
- Criteriu pentru ADE0-02: pentru fiecare flux inclus avem sursa exactă și dependențele;
  variantele vechi sunt deosebite de cele folosite. Formularele cu `_L` în nume nu sunt
  excluse automat: interdicția utilizatorului privește tabelele-cache.

### Open threads

- `mdl_Situatie` include mai multe proceduri. `CalculSituatieDebitori_Buget2021` este
  apelată din închiderea lunii; trebuie urmărite toate apelurile active și variantele.
- Lanțul 2021: `qPrezenta` → `qSolduri` → `Update_Solduri` → `Update_Situatie` →
  `qExplicatie` → `Update_Detalii` → `Update_Compensare` → opțional `Salvare_Lunara`.
- Identificăm datele necesare din tabelele cu SQL/configurări și tratamentul istoric al
  documentelor excluse din noua interfață, fără pierderea contribuțiilor la sold.

## Slice ADE1

### Registry

| Subfelie | Livrabil | Stare |
|---|---|---|
| SLICE-ADE1-01 | Structura proiectului ADECHIT și integrarea cu aplicația Flask/PYTHON și AvacontPush | PLANIFICAT |
| SLICE-ADE1-02 | Pagina de bază cu aspectul portalului și TOATE sistemele comune conectate | PLANIFICAT |
| SLICE-ADE1-03 | Preview local la localhost:5050, date de probă și instrucțiuni de pornire | PLANIFICAT |

### Current focus

- Depinde de maparea ADE0; următoarea subfelie liberă: **ADE1-04**.
- Definim o singură sursă pentru codul comun. Stabilim explicit cum ajung fișierele
  din proiectul ADECHIT în arborele publicat de AvacontPush, fără copiere manuală ambiguă.
- Inventariem importurile comune: controale, EventBus, ListenerTracker, registru,
  sesiuni, teme, preferințe, PDF, logare, audit, timpi și tratarea erorilor.
- Acceptare locală: pagina pornește pe 5050; navigarea și închiderea componentelor
  curăță abonamentele/timerele; erorile ajung prin sistemele existente.

### Open threads

- Nu există încă schelet ADECHIT sau preview ADE pornit. Ruta web finală se definește aici.
- Adaptoarele locale nu trebuie să ofere ocolirea autentificării în pachetul de producție.

## Slice ADE2

### Registry

| Subfelie | Livrabil | Stare |
|---|---|---|
| SLICE-ADE2-01 | Contractul editării și editori integrați în DataGrid comun | PLANIFICAT |
| SLICE-ADE2-02 | Validare, salvare/anulare, erori, conflicte și evenimente | PLANIFICAT |
| SLICE-ADE2-03 | Probă pe 5050: tastatură, virtualizare, filtre, grupări și regresii doar-citire | PLANIFICAT |

### Current focus

- **Prioritate critică, înaintea ecranelor de lucru ADE6.** Depinde de ADE1.
- Următoarea subfelie liberă: **ADE2-04**.
- Refolosim DataGrid existent, editorii custom aplicabili și ListenerTracker/EventBus;
  salvarea este delegată API-ului paginii, fără logică ADE introdusă în componenta comună.
- Definim Enter/Tab/Escape, focusul, starea modificată/în curs/eroare, validarea pe tip,
  editabilitatea pe rând/coloană și protecția față de răspunsuri asincrone întârziate.
- Acceptare: editarea rezistă la scroll și reciclarea rândurilor, sortare/filtrare și
  schimbarea selecției; un refuz de salvare nu apare ca succes; paginile actuale rămân
  doar-citire dacă nu activează explicit editarea.

### Open threads

- DataGrid actual are numai vizualizare. Contractul de salvare și tratarea conflictelor
  se stabilesc înaintea legării la rutele reale; întâi se probează local cu răspunsuri simulate.

## Slice ADE3

### Registry

| Subfelie | Livrabil | Stare |
|---|---|---|
| SLICE-ADE3-01 | Modelul persistent ADE_, chei, tipuri și maparea din Access | PLANIFICAT |
| SLICE-ADE3-02 | DDL Unitati_Chitante și maparea CFGs/CH pe unitate | PLANIFICAT |
| SLICE-ADE3-03 | Integrarea cu schema_sync și pachetul SQL pentru utilizator | PLANIFICAT |

### Current focus

- Depinde de ADE0; următoarea subfelie liberă: **ADE3-04**.
- Candidați de mapat, nu listă definitivă: Grupe, Platitori, Platitori_sub, Delegati,
  ValoriTaxe, LunaD, Prezenta, Prezenta_sub, Plati, Chitante, AlteDoc, Retur, SS_Buget.
- `SS_Buget` este situație salvată, nu cache de eliminat. Tabelele de lucru din calcule
  devin date izolate per operație/conexiune, după păstrarea exactă a semanticii.
- `AVACONT_COMUN.Unitati_Chitante`: legare prin DC; serie, număr și șablon de explicație.
  Schema finală și cheia pentru numerotare se justifică prin fluxul Access activ.
- Acceptare: niciun `_L`, nicio funcție de facturare/bon fiscal; scripturile sunt limitate
  la destinațiile declarate; schema_sync păstrează ADE fără a modifica alte module.

### Open threads

- Numele bazei țintă și DDL-ul live vor fi furnizate/confirmate de utilizator.
- Tipurile monetare și conversiile trebuie să reproducă rezultatele Access; nu adoptăm
  automat o precizie care schimbă rotunjirile existente.
- Cazul literelor din `AVACONT_COMUN` se confirmă pentru Linux; scripturile folosesc
  numele real, fără a presupune echivalența cu `avacont_comun`.
- SQL nescris și nerulat la data planului. Datele `GR40`/`783` nu sunt seed-uri.

## Slice ADE4

### Registry

| Subfelie | Livrabil | Stare |
|---|---|---|
| SLICE-ADE4-01 | Matrice de drepturi și integrarea conturilor ADE cu autentificarea existentă | PLANIFICAT |
| SLICE-ADE4-02 | Autorizare API, izolare pe unitate și scripturi de privilegii | PLANIFICAT |
| SLICE-ADE4-03 | Probe de acces permis/refuzat și corelarea jurnalelor | PLANIFICAT |

### Current focus

- Depinde de ADE1 și ADE3; următoarea subfelie liberă: **ADE4-04**.
- Conturile ADE nu primesc implicit acces la rutele portalului/desktopului K-BOT pentru
  alte module. Verificarea se face și la apel direct, nu doar prin ascunderea meniurilor.
- Refolosim sesiuni, autentificare, limitarea încercărilor, logare și audit; adaptăm
  contextul pentru identificarea utilizatorului și unității în toate cererile ADE.
- Acceptare: refuz fără drepturi, refuz între unități, expirare sesiune corectă,
  cont ADE fără acces accidental la date non-ADE; aceeași verificare la fiecare scriere.

### Open threads

- Rolurile și operațiile permise se definesc cu utilizatorul; nu inventăm conturi/parole.
- Excepțiile pentru Unitati_Detalii, Unitati_Chitante și alte tabele comune vor avea
  privilegii explicite, după operațiile necesare. Contul de serviciu trebuie inclus în analiză.

## Slice ADE5

### Registry

| Subfelie | Livrabil | Stare |
|---|---|---|
| SLICE-ADE5-01 | Extractul de date și verificarea mapărilor înainte de import | PLANIFICAT |
| SLICE-ADE5-02 | Import repetabil în ADE_ și Unitati_Chitante, cu jurnal | PLANIFICAT |
| SLICE-ADE5-03 | Reconciliere număr de rânduri, relații, solduri și istoric | PLANIFICAT |

### Current focus

- Depinde de ADE3/ADE4; următoarea subfelie liberă: **ADE5-04**.
- Reutilizăm mecanismele existente de migrare potrivite după verificarea lor; nu pornim
  automat vechiul flux Python orientat către alte tabele/servere.
- Păstrăm relațiile și istoricul necesar. Repetarea importului nu dublează înregistrări.
- Acceptare: raport de diferențe înainte/după, fără `_L`, fără recrearea distructivă a
  tabelelor existente și fără înlocuirea datelor unității deja gestionate de K-BOT.

### Open threads

- Date reale neprimite; instrumentul concret de import se alege după inventariere.
- Utilizatorul execută importul pe server și întoarce rezultatele reconcilierii.

## Slice ADE6

### Registry

| Subfelie | Livrabil | Stare |
|---|---|---|
| SLICE-ADE6-01 | Grupe, copii/plătitori, persoane asociate și delegați | PLANIFICAT |
| SLICE-ADE6-02 | Taxele relevante și prezența lunară cu editare în celule | PLANIFICAT |
| SLICE-ADE6-03 | Transferuri/plecări și verificarea fluxurilor de editare | PLANIFICAT |

### Current focus

- Depinde de ADE2–ADE5; următoarea subfelie liberă: **ADE6-04**.
- Refolosim aspectul portalului și controalele comune; structura arborelui și coloanele
  urmează operațiile Access incluse, stabilite în ADE0.
- Acceptare: modificările se salvează prin API cu validare; lunile închise refuză scrierea;
  schimbarea grupei/lunii și erorile nu pierd în tăcere editarea în curs.

### Open threads

- Regulile detaliate de prezență și transfer se extrag din fluxul activ, fără introducerea
  reducerilor excluse. Nu adăugăm pagina de configurare a datelor unității.

## Slice ADE7

### Registry

| Subfelie | Livrabil | Stare |
|---|---|---|
| SLICE-ADE7-01 | Transpunerea mdl_Situatie și a interogărilor sale, cu rezultate intermediare comparabile | PLANIFICAT |
| SLICE-ADE7-02 | Închidere/redeschidere, situația salvată și luna următoare | PLANIFICAT |
| SLICE-ADE7-03 | Comparație Access/web 1 la 1 pe cazurile de referință | PLANIFICAT |

### Current focus

- Depinde de ADE0, ADE3–ADE6; următoarea subfelie liberă: **ADE7-04**.
- **Fidelitatea calculelor este criteriu obligatoriu.** Comparam pe fiecare persoană/lună
  SID/SIC, obligații, plăți, compensare, anticipat, retur, SFD/SFC și totalurile relevante.
- Cazuri: sold debitor/creditor/zero, fără prezență, fără plăți, plată parțială, supraplată,
  avans, restituire, document anulat, perioade succesive și situație lunară salvată.
- Acceptare: zero diferențe neexplicate față de Access, inclusiv conversii/rotunjiri;
  operația lunară este atomică și nu poate fi executată dublu de utilizatori simultani.

### Open threads

- Rezultatele Access de referință sunt necesare validării finale și vin de la utilizator.
- Diferențele dintre proceduri, folosirea IDL drept ordine temporală și parametrii citiți
  din formulare se explică prin apelurile active înainte de transpunere.
- Dacă se descoperă un defect vechi, îl raportăm; nu schimbăm rezultatul unilateral.

## Slice ADE8

### Registry

| Subfelie | Livrabil | Stare |
|---|---|---|
| SLICE-ADE8-01 | Încasări, alte documente, restituiri și anulări | PLANIFICAT |
| SLICE-ADE8-02 | Chitanțe: serie/număr/explicație, document și tipărire | PLANIFICAT |
| SLICE-ADE8-03 | Probe tranzacționale, cereri repetate și utilizatori simultani | PLANIFICAT |

### Current focus

- Depinde de ADE2–ADE7; următoarea subfelie liberă: **ADE8-04**.
- Datele unității se citesc din sistemul comun; configurația chitanței din Unitati_Chitante.
- Reutilizăm componentele de vizualizare/print aplicabile. Generarea documentului ADE
  se construiește după raportul Access Chitanta, fără funcții de facturare/bonuri fiscale.
- Acceptare: o încasare și documentul ei se salvează coerent; numere fără coliziuni,
  anulări reflectate exact în solduri; o reîncercare nu produce dublarea încasării.

### Open threads

- Stabilim din codul activ și datele importate dacă NUMAR este număr curent/următor;
  nu deducem că prima chitanță web trebuie să aibă numărul 783 sau 784.
- Formatul final al chitanței va fi verificat de utilizator la tipărire.

## Slice ADE9

### Registry

| Subfelie | Livrabil | Stare |
|---|---|---|
| SLICE-ADE9-01 | Situații de debitori, fișe, registru de casă și rapoartele incluse | PLANIFICAT |
| SLICE-ADE9-02 | Probă completă pe 5050 și documentația fluxului de lucru | PLANIFICAT |
| SLICE-ADE9-03 | Pachet pentru AvacontPush, instrucțiuni SQL și consemnarea probelor utilizatorului | PLANIFICAT |

### Current focus

- Depinde de ADE1–ADE8; următoarea subfelie liberă: **ADE9-04**.
- Probă completă: grupă → copil/plătitor → prezență → calcul → încasare/chitanță →
  restituire/anulare → situație → închidere → luna următoare; roluri și unități diferite.
- Probele relevante de regresie includ grilele existente, sesiunile, logarea,
  EventBus/ListenerTracker și curățarea componentelor după navigări repetate.
- Predarea enumeră exact fișierele, scripturile în ordine, verificările înainte/după,
  pașii de revenire și limitările cunoscute. Publicarea se face de utilizator prin AvacontPush.

### Open threads

- Lista finală a rapoartelor se stabilește în ADE0, excluzând facturile și bonurile fiscale.
- Exporturile, importul SIIR și trimiterea de mesaje din vechea aplicație se inventariază
  în ADE0; nu sunt declarate automat necesare sau implementate în prima versiune.
- Validarea pe server și tipărirea reală rămân în așteptare până la confirmarea utilizatorului.
