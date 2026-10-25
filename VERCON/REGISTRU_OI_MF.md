# Registrul OI/MF — identitatea bonului și proveniența

Decizie confirmată de utilizator la **18.10.2026**, pentru aplicația VERCON
și VRC.Migrator. Se aplică atât obiectelor de inventar, cât și mijloacelor fixe.
Această clarificare înlocuiește interpretarea anterioară în care exact una
dintre coloanele `IDBP`, `IDBT`, `IDBC` trebuia completată în registru.

## Recuperarea provenienței după refacerea unui bon — 19.10.2026

**Revizie curentă în surse: 0.3.9.0**, reguli
`VC05-OI-ORIGIN-RECOVERY-2026-10-19`. Această regulă completează validările
stricte de mai jos pentru referințele dispărute din sursa Access.

Caz real citit read-only la cererea utilizatorului: IDOI 1576, produs 2673,
casare 2950051 / BS 1113 cu IDBO 2950049 inexistent. Singura primire găsită
pentru produs este IDOI 1577, predare 2950052 / BP 72. Ambele documente au
data 09.12.2025, deținător 35, cont 7, preț 1252,9924 și cantitate 1.
Casarea a fost salvată pe 15.01.2026, iar predarea existentă pe 16.01.2026.
Datele sunt compatibile cu o predare ștearsă/refăcută; istoricul ștergerii
nu este demonstrat de starea actuală. Utilizatorul a autorizat tratarea
automată a acestor cazuri în migrator, pe criterii de asociere univocă.

Recuperarea este permisă numai pentru IDBO nenul **absent din ambele
tabele BP/BT**, în aceeași sursă. Primirea înlocuitoare trebuie să aibă:

- document BP/BT activ și poziție pozitivă în registru;
- același produs și același preț;
- același deținător predător și același cont brut din registru;
- dată document anterioară sau egală cu data transferului/casării;
- disponibil suficient după toate ieșirile deja legate, inclusiv cele ulterioare.

Pentru transfer, predătorul se citește din IDC_OLD al bonului, nu din
rândul pozitiv al primitorului. Minusul și plusul trebuie să identifice
deținătorii corecți și cantități totale egale; aceeași înlocuire se aplică
ambelor laturi. Propriul bon nu este candidat. Primiri compatibile în
două bonuri diferite înseamnă ambiguitate dacă ambele au disponibil suficient.
Un cont lipsă sau TC cu cont divergent combinat cu proveniență dispărută
poate împiedica recuperarea; nu se relaxează criteriile pentru a ghici sursa.
După recuperare, rezolvarea contului continuă să urmărească BP-ul inițial.

Se folosește **data documentului**, nu DataSalvare: bonul refăcut poate
fi salvat mai târziu. Rândurile aceluiași lot/document sunt însumate, iar
cantitățile deja rezervate prin alte ieșiri nu sunt reutilizate. Ciclurile
de proveniență rămân blocate de resolverul conturilor.

`VrcRunner.OiOrigins.vb` construiește un snapshot clonat, comun conversiei
IDBO și resolverului contului. Sursa Access și dicționarele originale nu
sunt rescrise. Raportul N14/CONVERSIE listează fiecare poziție și referința
veche → familia/ID-ul sursă recuperat; apoi se aplică remaparea normală a
documentului. Nu se fabrică un bon pentru vechiul ID dispărut.
Fără candidat unic: X17/BLOCAJ, cu motiv și rândul afectat. Un IDBO existent,
dar contradictoriu sau ambiguu, nu este înlocuit prin această regulă.

Pentru cazul analizat, asocierea așteptată este 2950049 → IDBP 2950052,
în snapshotul de import al poziției 1576. Aceasta este concluzie de design,
nu rezultat al unei rulări a codului nou. Nu s-au rulat teste, build sau
migrare pentru această revizie și nu s-au modificat schema/datele existente.
Este necesară recompilarea migratorului înaintea reluării importului.

## Semnificația coloanelor

În `OI_Produse` din Access / `VC_OI_Produse` din MariaDB:

| Coloană | Semnificație |
|---|---|
| `IDOI` | Identitatea rândului din registru; nu este ID-ul bonului. |
| `IDBC` | ID-ul **bonului curent**, pentru toate cele trei operații: predare, transfer sau casare. Nu este o coloană rezervată casării. |
| `IDBP` | ID din `OI_Predare` / `VC_OI_Predare`; completat numai la predare. |
| `IDBT` | ID din `OI_Transfer` / `VC_OI_Transfer`; completat numai la transfer. |
| `IDBO` | ID-ul bonului BP/BT de proveniență, din care se transferă/casează produsul; completat numai la transfer/casare. |

Casarea nu are o coloană de marcaj dedicată în registru. Dacă `IDBP` și
`IDBT` sunt ambele goale/0, `IDBC` trebuie să identifice o casare.
`IDBP` și `IDBT` completate simultan sunt contradictorii.

În tabelele documentelor, cheile de grup rămân distincte:
`VC_OI_Predare.IDBP`, `VC_OI_Transfer.IDBT`, `VC_OI_Casare.IDBC`.
Mai multe linii ale aceluiași bon au același ID de grup; cheia rândului
documentului este `ID`. Nu se introduce un FK simplu care presupune
unicitatea ID-ului de grup pe fiecare linie. `IDBC` din registru nu se
confundă cu `VC_Iesiri_H.IdBonConsum`, bonul de consum asociat predării.

## Exemplu complet

Predarea 100 trimite un produs la Ion. Transferul 200 îl mută de la Ion
la Maria. Casarea 300 îl scoate de la Maria. Pentru o bucată:

| Mișcare | Deținător | Cant | IDBP | IDBT | IDBC | IDBO |
|---|---|---:|---:|---:|---:|---:|
| Predare | Ion | +1 | 100 | — | 100 | — |
| Transfer, scădere | Ion | -1 | — | 200 | 200 | 100 |
| Transfer, primire | Maria | +1 | — | 200 | 200 | 100 |
| Casare | Maria | -1 | — | — | 300 | 200 |

`—` reprezintă NULL/0. `IDBO=100` se păstrează pe **ambele** rânduri ale
transferului 200. Totuși, lotul primit de Maria este identificat prin
**transferul curent 200**; o ieșire ulterioară a Mariei va indica `IDBO=200`.
Transferul scrie minus și plus atomic. Predarea scade separat stocul din
magazie prin BC-ul asociat; registrul de folosință nu dublează acel consum.

## Compatibilitatea cu VBA vechi

VBA-ul disponibil în repository scria `IDBC=0` la predare și transfer.
Acest format este acceptat: tipul și bonul se deduc din `IDBP`/`IDBT`, iar
migratorul completează `IDBC` cu ID-ul renumerotat al aceluiași bon.
Bonurile noi din web scriu întotdeauna `IDBC` pentru toate cele trei familii.

Dacă `IDBC` este deja nenul la predare/transfer, trebuie să fie egal cu
marcajul corespunzător. O contradicție nu se ignoră și nu se repară automat.
Clarificarea de business a utilizatorului are prioritate față de vechea
interpretare dedusă numai din VBA.

## Regula de migrare

ID-urile interne ale bonurilor P/T/C se alocă dintr-o **singură secvență
pe gestiune**. Identitatea sursă rămâne delimitată prin sursă, familie și
ID-ul vechi; același număr sursă din două familii nu le face același bon.

Exemplu de remapare: predare 100 → 1, transfer 200 → 2, casare 300 → 3.
Documentele și registrul trebuie să folosească aceleași valori:

| Mișcare migrată | IDBP | IDBT | IDBC | IDBO |
|---|---:|---:|---:|---:|
| Predare | 1 | — | 1 | — |
| Transfer, scădere | — | 2 | 2 | 1 |
| Transfer, primire | — | 2 | 2 | 1 |
| Casare | — | — | 3 | 2 |

Vechea mapare renumerota separat grupurile `IDBP`, `IDBT`, `IDBC`.
Putea produce atât `IDBP=33` / `IDBC=37` pentru aceeași predare, cât și
ID-uri identice pentru o predare și un transfer. În al doilea caz `IDBO`
nu mai identifica sigur proveniența în destinație.

Acum `GroupNumber` păstrează mapările sursă pe familie, dar folosește
contorul comun `OI_DOCUMENT`. `OI_Produse.IDBC` are regula `oi_document`,
care alege familia după marcajele rândului; `IDBO` are regula `origin`.
Ambele reguli sunt păstrate și de generatorul `tools/import_map.js`.
Proveniența ambiguă în sursă (același ID găsit și ca BP, și ca BT) blochează
importul; migratorul nu ghicește familia.

**Acestea sunt ID-uri interne, nu numerele afișate ale bonurilor.**
Numerele BP/BT/BS au serii separate și nu se schimbă prin această corecție.
Nu se recalculează cantitățile, prețurile, valorile sau semnele istorice.

## Regula aplicației

`oi_custody.py` centralizează familia bonului, citirea registrului,
construirea referințelor rândurilor noi și identificarea loturilor.
`oi.py` folosește aceeași logică la salvare, reîncărcare, modificare și anulare.
La ștergerea rândurilor unei casări se cer și `IDBP`/`IDBT` goale/0;
filtrarea numai prin `IDBC` ar putea afecta alt tip de bon.

`IDBO` este verificat în documentele BP/BT active pentru produs: sursa
trebuie să existe, să fie neambiguă, să nu fie propriul bon și să nu aibă
o dată ulterioară mișcării. Modificarea/anularea unui BP/BT cu ieșiri
dependente este blocată până la anularea documentelor dependente.

Predarea oferă produse cu stoc pozitiv în magazie; transferul și casarea
oferă loturi cu disponibil pozitiv la predător. Data bonului, celelalte
rânduri din ciornă și mișcările ulterioare limitează disponibilul.

## Contul de folosință se păstrează până la casare

Regulă confirmată de utilizator la **18.10.2026**, după diagnosticul BP 27 /
BS 5 pentru BADEA ION, CALCULATOR PORTABIL ASUS: contul lotului este cel din
primirea inițială BP în registru. Transferul și casarea îl moștenesc prin
`IDBO`; un BT primit continuă același cont pentru următorul deținător.
`IDM` dintr-o poziție TC și `IdMasina` din linia TC nu pot muta produsul pe
alt cont. Nu se elimină contul din identitatea primirilor BP distincte.

`oi_accounts.py` rezolvă lanțul din registru după bonul de proveniență,
variantă și preț, până la BP. Disponibilul, cronologia, reîncărcarea TC,
validarea/scrierea bonurilor și evidența/rapoartele folosesc contul rezolvat.
Contul trimis de client pentru TC nu este folosit pentru a schimba lotul.
Citirea datelor deja migrate nu execută UPDATE și nu schimbă reviziile.
Un lanț lipsă, ciclic sau cu mai multe conturi de proveniență produce
eroare explicită; nu se alege arbitrar un cont și nu se cumulează BP-uri
distincte pentru a ascunde ambiguitatea.

Migratorul **0.3.8.0**, regula `VC05-OI-ACCOUNT-LINEAGE-2026-10-18`, aplică
aceeași regulă în sursă, înaintea remapării cheii contului: corectează
`OI_Produse.IDM` la TC și `OI_Transfer/OI_Casare.IdMasina`, fără schimbarea
fișierului Access. Conversiile sunt consemnate ca **N13**, cu valoarea
originală și contul rezultat. Proveniențele nerezolvabile blochează cu X16.
Cantitățile, prețurile, valorile, semnele și ID-urile bonurilor se păstrează.
Corecția contului este autorizată explicit de regula de mai sus; celelalte
referințe istorice nu sunt reparate implicit.

În exemplul diagnosticat, IDOI 1821 este BP +1 pe contul 34, iar IDOI 1848
este BS -1 cu IDBO=12, dar cont 33 în Access. Contul efectiv al BS devine
34, moștenit din BP: aceeași cheie de lot are sold 0 și nu este eligibilă.
Acesta este rezultatul așteptat din regulă; implementarea nu a fost rulată.

## Diagnosticul datelor istorice

Mesajul vechi «O poziție din registrul OI nu identifică un singur document»
provenea din cerința greșită de exclusivitate între cele trei coloane.
`IDBP` + `IDBC` sau `IDBT` + `IDBC` completate împreună sunt normale.
**O interogare care numără cele trei coloane și cere exact una completată
nu mai este un diagnostic valid pentru acest registru.**

Erorile actuale indică poziția `IDOI` și referința contradictorie/lipsă.
Datele deja migrate cu maparea veche nu sunt remapate retroactiv prin
actualizarea codului. Recuperarea de mai sus se aplică numai importului
din sursa Access, nu citirii datelor existente în MariaDB. Repararea
acestora cere analiză separată a documentelor
și a mapărilor sursă; nu se golește automat `IDBP`, `IDBT`, `IDBC` sau `IDBO`.

## ToolVersion și schema

La 18.10.2026 utilizatorul a raportat rollback în `000_DEMO`, la crearea
contextului, cu `Data too long for column 'ToolVersion' at row 1`.
`VC_Imports.ToolVersion` primea `VRC.Migrator ` concatenat cu identificatorul
lung al regulilor. Acum primește versiunea executabilului din
`FileVersionInfo`: **0.3.7.0** pentru această revizie.

`RuleVersion = VC05-OI-DOCUMENTS-2026-10-18` rămâne integral în rezumatul
importului și în manifest/hashul planului. Nu se trunchiază identificatorul
regulilor și nu este necesară lărgirea coloanei `ToolVersion` pentru această corecție.

Nu este necesară schimbarea schemei pentru semantica registrului sau
pentru această eroare. Nu se adaugă o coloană dedicată casării.
Migratorul trebuie recompilat înaintea reluării importului, iar aplicația
trebuie actualizată împreună cu el. O tentativă cu rollback confirmat nu
este import confirmat; nu se confundă cu un COMMIT cu rezultat necunoscut.

## Stare și surse

**IMPLEMENTAT ÎN SURSE, NERULAT.** Nu s-au rulat de către asistent teste,
build, migrator, aplicație, browser, HTTP sau interogări MariaDB pentru
aceste modificări. Eșecul cu `ToolVersion` este o observație raportată de
utilizator; după corecție nu există încă o confirmare de migrare reușită.
Schema și datele deja migrate nu au fost modificate de asistent.

Surse relevante:

- [VrcRunner.Oi.vb](VRC.Migrator/VrcRunner.Oi.vb), [VrcRunner.vb](VRC.Migrator/VrcRunner.vb).
- [VrcImportMap.vb](VRC.Migrator/VrcImportMap.vb), [import_map.json](VRC.Migrator/import_map.json), [generator](tools/import_map.js).
- [VrcPlan.vb](VRC.Migrator/VrcPlan.vb), [proiect și FileVersion](VRC.Migrator/VRC.Migrator.vbproj).
- [oi_custody.py](../PYTHON/routes/vercon/oi_custody.py), [oi.py](../PYTHON/routes/vercon/oi.py).
- [Jurnalul implementării](../implementare_vercon_v2.md).

## Istoric absorbit din jurnalul general — D001-04

Reper 18–19.10.2026, fără probe noi. Regula primirii BP în migrator a fost ulterior revizuită la 21.10 în [VC09-05](../docs/worklog/SLICE-VC09-05-reglare-registru-oi-si-produs-la-migrare.md) și [README VRC](VRC.Migrator/README.md#obiecte-de-inventar-primirea-din-oi_predare-produs-completat-în-registru--21102026-slice-vc09-05); explicațiile datate despre cont/proveniență de aici se citesc cu acea revizie, fără presupunerea că toate sursele sunt încă la 0.3.9.0.

### Diagnosticul BP 27 / BS 5 — cele trei etape distincte

1. Scriptul manual sql/VC_DIAG_disponibil_transfer.sql a fost pregătit pentru cazul BADEA ION / CALCULATOR PORTABIL ASUS. Cere context/variantă explicite, fără alegerea primei potriviri; arată documentele cu semnele așteptate, registrul brut pentru toți deținătorii și cheia lot/proveniență–variantă–cont–preț cu sold la data bonului. Inițial cauza era nedemonstrată; fără corecții automate sau calcul paralel care ocolește registrul.
2. Rezultatele furnizate de operator: 000_DEMO, subunitate 10, gestiune 8, exercițiu 9, deținător 164, variantă 3878; BP ID 12 și BS ID 38 au sold net 0 în bonuri. Registrul are lot 12/preț 4408,1600, dar IDOI 1821 este +1 pe IDM 34 (MF ÎN FOLOSINȚĂ), IDOI 1848 este −1 pe IDM 33 (MF ÎN MAGAZIE). Gruparea separa conturile: +1 rămânea eligibil, grupul −1 fără intrare era omis de batches. Acesta era mecanismul imediat demonstrat; proveniența Access vs migrare rămânea încă nedemonstrată. Scriptul a primit secțiunea 6 cu ambele conturi BP și mapările ROW ale liniilor/conturilor, fără schimbarea cheii sau datelor înaintea comparației.
3. Ulterior, utilizatorul a confirmat eroarea de cont din Access și a cerut ambele corecții. Contul efectiv se moștenește recursiv prin BP/BT, pe variantă/preț, cu memoizare/cicluri/ambiguități verificate; IDM brut rămâne separat. TC se reîncarcă din registru cu cantitate/preț din linie; contul clientului nu schimbă lotul. Evidența/rapoartele păstrează cantitățile/datele sursei configurate și actualizează încadrarea/numele contului; citirea nu execută UPDATE. Migratorul indexează primiri/linii TC per sursă și convertește IDM/IdMasina înaintea remapării, fără rescriere Access; N13 păstrează original/țintă/cont, X16 blochează lipsa/ciclul/ambiguitatea. Pentru BS38/IDOI1848, contul 34 în loc de 33 și +1−1=0 este rezultat de design, nu rulare; diagnosticul continuă să arate cheile BRUTE.

### Identitate, implementare și proveniență documentară

- Alocarea comună P/T/C păstrează maparea sursei după fișier/familie; documentele, marcajele IDBP/IDBT, IDBC și IDBO folosesc aceeași mapare. Resolverul IDBC verifică documentul sursă și completează 0 la BP/BT din marcaj; contradicțiile numesc IDOI/valorile, fără reparație tacită. import_map.js păstrează oi_document/origin și versiunea regulilor planului.
- oi_custody centralizează citirea și rândurile noi, familia și lotul primit prin transferul curent; protecția dependentelor folosește evenimente normalizate/IDBO și exclude propriul document prin familie+ID. Ștergerea este delimitată pe familie. Rapoartele stoc/amortizare citesc tabelele P/T/C cu cheile proprii și nu au cerut schimbări pentru semantica IDBC din registru.
- Corecția ToolVersion a urmat rollbackului la crearea contextului, înaintea migrării; nu trunchiază RuleVersion și cere recompilare. La documentarea din 18.10 nu exista confirmare de import reușit după corecție. Modificate atunci numai Markdown: acest contract, DICTIONAR_DATE, PLAN_IMPLEMENTARE, VERCON_STATUS, README VRC și CODEX_SCHIMBARI_MIGRATOR_2026-10-18. Celelalte cerințe istorice nu au fost declarate rezolvate prin documentare.
- Recuperarea 0.3.9.0 folosește același snapshot în BuildRow și OiAccounts; X17 raportează imposibilitatea înaintea conversiilor rândului. Citirea cazului real a fost read-only, la cererea utilizatorului; algoritmul nou nu a fost executat și nu remapează retroactiv importurile existente.

Fișiere ale etapelor de identitate/cont: VERCON/VRC.Migrator/{VrcRunner,VrcRunner.Oi,VrcRunner.OiAccounts,VrcImportMap,VrcPlan}.vb, import_map.json, VRC.Migrator.vbproj, README; VERCON/tools/import_map.js; PYTHON/routes/vercon/{oi,oi_custody,oi_accounts,stock_source,report_data,report_oi}.py; SQL-ul de diagnostic și acest contract. Recuperarea adaugă VrcRunner.OiOrigins.vb și atinge și VrcRun.vb. ToolVersion atingea VrcRunner.vb și proiectul VRC. Nu s-au executat teste, build, import, HTTP, SQL sau modificări de schemă/date în aceste corecții; executabilul necesita recompilare.
