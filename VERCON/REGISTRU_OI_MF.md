# Registrul OI/MF — identitatea bonului și proveniența

Decizie confirmată de utilizator la **18.10.2026**, pentru aplicația VERCON
și VRC.Migrator. Se aplică atât obiectelor de inventar, cât și mijloacelor fixe.
Această clarificare înlocuiește interpretarea anterioară în care exact una
dintre coloanele `IDBP`, `IDBT`, `IDBC` trebuia completată în registru.

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
actualizarea codului. Repararea lor cere analiză separată a documentelor
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
