# VRC.Migrator — utilizare

## Obiecte de inventar: primirea din OI_Predare, produs completat în registru — 21.10.2026 (SLICE-VC09-05)

Versiune în surse: **0.3.11.0**. Doar sursele; nerulat.

- **Contul unei scoateri (transfer/casare)** se află acum din linia de primire a bonului din `OI_Predare`
  (bon + produs, fără preț, ca în programul Access), nu din rândurile de primire ale `OI_Produse`,
  care au adesea `IdProdus` gol. Contul este `IdMasina` de pe linia din `OI_Predare`. Primirile prin bon de
  transfer se iau în continuare din registru, cu lanțul prin transferuri neschimbat.
- **Produsul lipsă din registru** se completează la transfer pentru pozițiile de primire BP: se ia produsul
  liniei bonului din `OI_Predare` cu aceeași cantitate și același preț, numai când există exact atâtea linii
  libere câte poziții goale și toate au același produs. Restul rămân fără produs (raport N17) și se reglează din
  VERCON, butonul «Reglare registru OI». Raport N16 pentru cele completate. Fișierele Access nu se modifică.
- Mesajele de blocaj și din secțiunea «Doar de informare» sunt rescrise în limbaj clar; blocajul X16 are titlu
  general și câte un rând explicit pentru fiecare poziție.

Regula veche din secțiunea «Contul lotului OI/MF până la casare» (primirea căutată în `OI_Produse`, cu preț) este
înlocuită de aceasta pentru primirile BP.

## IntrariStorno fără NIR storno părinte — 19.10.2026

Versiune curentă în surse: **0.3.10.0**, reguli
`VC05-STORNO-PARENTS-2026-10-19`. Decizie explicită a utilizatorului:
se identifică NIR-urile cu bifa `Storno` din aceeași sursă/gestiune, apoi
se importă numai liniile `IntrariStorno` cu `IdIntrare` în acel set.
Dacă nu există niciun NIR bifat Storno, toate liniile IntrariStorno se ignoră.
Un NIR existent fără bifa Storno nu este un părinte valid pentru aceste linii.

Liniile fără părinte eligibil sunt omise **înainte** de conversia IdProdus
și a celorlalte câmpuri: nu blochează importul, chiar dacă produsul lipsește.
Nu sunt importate cu IdIntrare NULL. Raportul N15 le listează cu ID-ul,
IdIntrare și IdProdus originale; reconcilierea le numără la rânduri ignorate
și nu le include în sumele transferate. Storno-urile cu părinte eligibil
păstrează toate validările, inclusiv existența produsului. Variantele folosite
numai de aceste storno-uri valide sunt incluse în produsele migrate.

Regula înlocuiește pentru importurile noi politica veche N12 de păstrare a
liniilor storno fără NIR cu legătura NULL. Nu elimină date din importuri deja
făcute și nu schimbă asocierea manuală din web pentru orfani deja existenți.
Access rămâne nemodificat. Implementat în surse, fără teste/build/import;
necesită recompilarea migratorului, fără modificarea schemei.

## Proveniență OI/MF după refacerea unui bon — 19.10.2026

Versiune în surse: **0.3.9.0**, reguli `VC05-OI-ORIGIN-RECOVERY-2026-10-19`.
Un IDBO nenul care nu mai există nici în predări, nici în transferuri poate
fi recuperat dintr-o singură primire BP/BT existentă în aceeași sursă.
Se cer același produs, preț, deținător și cont, dată document cel mult egală
cu data mișcării și cantitate rămasă suficientă după celelalte ieșiri,
inclusiv cele ulterioare. Pentru transfer se folosesc predătorul din bon
și ambele laturi, cu cantități egale; asocierea se aplică pe ambele rânduri.
DataSalvare nu limitează căutarea: o predare refăcută ulterior poate avea
aceeași dată document ca predarea dispărută.

Corecția se face într-un snapshot în memorie, comun conversiei IDBO și
rezolvării contului moștenit; Access rămâne nemodificat. Raportul listează
fiecare IDOI și legătura veche/nouă ca N14/CONVERSIE. Dacă nu există o
singură primire compatibilă, X17 blochează importul cu motivul concret.
Nu se inventează bonuri și nu se înlocuiesc referințe care există deja.

Detalii și exemplu: [contractul OI/MF](../REGISTRU_OI_MF.md).
Implementat în surse, fără teste/build/import pentru această corecție.
Necesită recompilarea migratorului, fără schimbarea schemei.

## Revizie OI/MF și ToolVersion — 18.10.2026

Versiune în surse: **0.3.7.0**. [Contractul registrului OI/MF](../REGISTRU_OI_MF.md)
documentează decizia confirmată și exemplul predare → transfer → casare.
ID-urile interne P/T/C au un contor comun pe gestiune; `OI_Produse.IDBC`
identifică bonul curent pentru toate familiile, iar `IDBO` identifică
proveniența BP/BT numai la transfer/casare. Marcajele `IDBP`/`IDBT` se
completează numai pentru familia lor. Generatorul mapării păstrează aceste reguli.

`VC_Imports.ToolVersion` primește versiunea executabilului din FileVersionInfo,
fără concatenarea numelui și a identificatorului lung al regulilor care a
provocat eroarea «Data too long». Regula completă rămâne în rezumat și în
manifest/hash. **Nu este necesară schimbarea schemei pentru această revizie.**

Migratorul trebuie recompilat înainte de reluarea importului. Corecția nu
repară retroactiv date deja migrate cu maparea veche. Stare: implementat
în surse, fără build, teste sau import rulate de asistent după corecție.

## Context și profilare

**Clarificare VC06-06, 16.10.2026:** noile decizii de interfață NIR nu cer
adaptarea importului Access la mai multe conturi pe document. Sursa veche are
un NIR cu un singur cont din unitatea respectivă; relațiile importate dintre
linie, variantă de preț și produs determină deja contul. Nu se copiază contul
antetului într-o coloană nouă a liniei și nu se reunesc documente istorice
pentru noul flux. Această clarificare nu retrage infrastructura comună de
schemă implementată anterior. [Decizii D09–D11](../DECIZII_DESCHISE.md#g-decizii-nir-și-contul-produsului--16102026-slice-vc06-06).

Utilitar Windows separat pentru citirea, profilarea și transferul **unic**
Access → MariaDB al subunităților VERCON. Descrierea profilării provine din
VC05-01; fluxul de planificare și transfer este descris în secțiunea Transferul.

## Ce face

- Profilarea deschide fiecare bază **doar în mod citire** (`Mode=Read`); nu scrie nimic în Access, nu creează tabelul `Ver` și nu scrie în MariaDB. Transferul în MariaDB este o operație separată, descrisă mai jos.
- Calculează SHA-256 înainte și după citire: dacă diferă, raportul spune că fișierul s-a schimbat (se recitește pe o copie).
- Pentru fiecare tabel așteptat: rânduri, coloane **lipsă** (absentă ≠ NULL) și coloane în plus; pentru coloanele numerice: valori, NULL, zero, negative, minim, maxim și zecimalele necesare.
- Numără antetele NIR / bon de consum față de suma liniilor (N01/N02): antet 0, NULL, diferență > 1 leu. Nu modifică nimic.
- Citește configurarea (`Setari`, `NR`, doar `DC` și `AnDate` din `UNIT`; **numele persoanelor nu se citesc**) și propune tipul, ZEC și scara de stocare (propuneri, nu decizii).
- Raportează tabelele legate (datele lor nu sunt în fișier) și tabelele neprevăzute.

## Folosire

1. Pornește `VRC.Migrator.exe` → **Alege folder…** (folderul cu bazele subunității). Apare un tabel cu bifă pe fișier; **bifează anii dorești**, apoi **Citește bifatele** și **Salvează raportul…**.
   - Se listează fișierele `tz_aaaa` (exercițiul unei gestiuni Materiale/Alimente) și `tz_2100` (bază specială OI sau MF: **2100 nu este un an**), atât `.accdb` cât și `.mdb`; un an prezent în ambele formate apare de două ori.
   - `tz.mdb` / `tz.accdb` (fără an) nu se iau niciodată. Alte nume `tz_...` apar nebifate.
2. Fără fereastră: `VRC.Migrator.exe --profile raport.md "C:\cale\folder"` (sau fișiere). Un folder aduce toate fișierele tz_aaaa/tz_2100; cod de ieșire 0 = ok, 2 = nimic de citit sau o bază nu s-a putut citi.
3. Nu închide procese MSACCESS: dacă o bază este deschisă de cineva, hash-ul se citește oricum, iar raportul arată dacă s-a schimbat.

Schema așteptată este încorporată în executabil din `expected_schema.json`, generat cu `node VERCON/tools/expected.js` (din TZ_SCHEMA).
Parola comună a fișierelor se încearcă prin `KBot.Common.AccessFilePassword` și nu apare nicăieri.

x64, ca restul migratoarelor (furnizorul ACE este doar pe 64 de biți pe aceste stații; presupunerea că toate stațiile au ACE nu este verificată).

## Transferul (VC05-02…05)

Nerulat pe baze reale. Pașii:

1. După **Citește bifatele** apare planul: pe fiecare fișier alegeți **gestiunea** (denumire), **tipul** (MATERIALE, ALIMENTE, OI, MF) și **anul** exercițiului; **subunitatea** se alege **pe fiecare fișier** (coloana «Subunitate», propusă din UNIT.NumeUnitate al fișierului; o puteți modifica), deci un singur transfer poate duce mai multe subunități, fiecare cu propriile Alimente / Materiale / OI / MF (ZEC se alege tot pe subunitate); apoi serverul. Propunerile vin din nume și steaguri, nu sunt aprobări. **Verifică planul** arată motivele concrete de blocare; **Exportă planul** salvează JSON (fără parole).
2. **Testează** verifică baza DC (din UNIT.DC), tabelele VC_* cu coloanele scrise, InnoDB și utf8mb3_general_ci, subunitatea nouă și furnizorii fără partener KBOT. Orice modificare a planului cere retestare.
   **Parteneri ANAF…** (opțional, după avertismentul cu furnizori fără partener): pentru fiecare cod fiscal lipsă se interoghează ANAF (v9, câte 100 de coduri pe cerere), operatorul bifează per furnizor sursa-sector (una sau mai multe; click pe titlul coloanei bifează tot) și «Pregătește pentru transfer» reține partenerii în plan. Nu se scrie nimic în DC din fereastră: **«Migrează» îi inserează în `Parteneri` pe aceeași tranzacție cu transferul**, deci dacă transferul eșuează sau este oprit nu rămâne niciunul. Un cod fiscal = un singur partener în DC; `IdUnitate` = cea mai mică unitate dintre SS bifate, `CodPartener` = următorul număr liber (minim 3 cifre) din acel SS, `Tip` = 1, `Provenienta` = `MIGRARE_VERCON` (coloană nouă în `Parteneri`: rulați întâi `sql/Parteneri_Provenienta.sql` pe AVACONT_SURSA și sincronizați DC-urile). Lista pregătită se pierde la alegerea altui folder / citirea altor fișiere. Raportul de migrare are secțiunea «Parteneri din ANAF».
3. **Migrează** face tot transferul într-o tranzacție: nimic nu e vizibil înainte de COMMIT. **Oprește** anulează înainte de COMMIT. După COMMIT subunitatea nu mai poate fi importată (VC_Imports unic pe subunitate); nu există reimport.
4. Dacă răspunsul la COMMIT se pierde, starea este REZULTAT_NECUNOSCUT: folosiți **Diagnostic** (doar citire); nu repetați transferul.
5. Jurnalul și raportul sunt în `Logs\vrc_migrare_<data>.log` și `.md`.

Reguli decise (12.10.2026): numerotarea pornește de la ultimul număr real, nu din NR.*; ProduseP se transferă doar cu folosire; valorile documentelor sunt copiate ca în Access (revizie 14.10.2026): semnele și totalurile, inclusiv 0/NULL, nu se corectează; diferențele sunt doar raportate. Detalii: `DECIZII_DESCHISE.md`, secțiunea F.

Revizie 16.10.2026: sursa unică a structurii este baza AVACONT_SURSA de pe server, sincronizată separat în DC-uri. La verificare și înaintea transferului, VRC citește schema reală a DC-ului prin `KBot.Migrator.TargetSchema.Load` și stabilește ordinea după FK prin `WriteOrder.Derive`, aceleași clase folosite de KBot.Migrator. Nu creează baze sau tabele și nu repară automat schema. Incompatibilitățile opresc importul înainte de tranzacție.

`import_map.json`, produs de `node VERCON/tools/import_map.js`, conține numai identitatea rândurilor sursă, clasele de conversie și remaparea referințelor Access; nu declară tipuri SQL, chei generate sau nulabilitate pentru țintă. Câmpurile obligatorii, coloanele generate, limitele textelor și cheia AUTO_INCREMENT se citesc din MariaDB. Legăturile logice Access și fazele nomenclatoare/documente sunt verificate suplimentar față de ordinea FK. Scriptul complet generat și vechiul `target_schema.json` au fost eliminate; schimbările de structură se livrează în DDL-uri incrementale pentru aplicare manuală.

Sortarea comună acceptă acum și dependențele logice Access, adăugate la FK-urile reale în același graf. Semnătura existentă cu două argumente este păstrată pentru apelanții KBot; noua suprasarcină este folosită de VRC. Versiuni sursă curente în slice-ul VC06-06: VRC.Migrator 0.2.1.0 și KBot.Migrator 1.19.0.0, cu AssemblyVersion neschimbat. La publicare se livrează împreună executabilul VRC și biblioteca KBot.Migrator actualizată.

Stare pentru această revizie: implementat în surse; fără compilare, teste sau rulare pe server. Generatorul mapării nu a fost executat; resursa existentă a fost transformată pentru a elimina metadatele fizice ale țintei și a păstra regulile de import.

## Stocare DECIMAL — 14.10.2026

Cantitățile, prețurile, sumele, cotele TVA fracționare și soldul SI_LA se scriu
ca valori directe `DECIMAL(20,4)`, fără înmulțire cu `10^Scara`. `Scara=4`
este precizia fixă de stocare; `Zec` rămâne precizia afișării și calculelor.
Denumirea internă `VALOARE-INT` a clasei din exporturi este păstrată pentru
compatibilitatea descrierilor de coloane; conversia ei produce acum `Decimal`.

Pentru demo-ul care conține vechea migrare scalată:

1. Executați manual `sql/VC_DEMO_golire_pentru_decimal.sql` — șterge numai
   datele VC din `000_DEMO`, inclusiv markerii de import; păstrează tabelele,
   interogările salvate, partenerii și datele celorlalte module.
2. Executați manual `sql/VC_precizie_valori_4_zecimale.sql` și
   `sql/00EF_24_primite_linii_precizie.sql` pe `AVACONT_SURSA`, apoi sincronizați
   schema către DC-uri. ALTER-ul VC presupune lipsa datelor scalate în sursă/țintă.
3. Folosiți migratoarele recompilate și aplicația web actualizată, apoi reluați
   migrarea VERCON. Migratoarele blochează scrierea dacă schema de valori nu
   este `DECIMAL(20,4)`; nu creează și nu modifică automat scheme.

Nu s-au executat SQL-uri, compilări sau teste în cadrul acestei modificări.
Datele EF deja rotunjite nu sunt recuperate prin ALTER și nu sunt șterse de resetul VC.

## Copierea valorilor din Access — revizie 14.10.2026

VRC.Migrator nu calculează totaluri, TVA, prețuri, cantități sau amortizare și nu
corectează semne. Antetul NIR/BC rămâne ca în sursă, chiar dacă este 0, NULL sau
nu corespunde sumei liniilor. Cantitatea și prețul negative se păstrează simultan.
Constatările despre aceste situații sunt informative, fără modificarea datelor.
Numerotarea actuală după ultimul număr real rămâne, conform confirmării utilizatorului;
reprezentarea fără pierdere Numar/NumarSub și remaparea cheilor rămân necesare.

Dacă vechile constrângeri de preț nenegativ au fost instalate, executați manual
`sql/VC_IMPORT_valori_sursa.sql` înainte de migrare. Elimină numai aceste CHECK-uri
pe AVACONT_SURSA și 000_DEMO; nu se execută automat din aplicație. Regula de validare
a documentelor noi din web rămâne separată și neschimbată.

Nu s-au rulat compilări, teste sau importuri pentru această revizie.


## Raport HTML în fereastră separată

Butonul **Raport HTML…**, în partea de sus a zonei Raport / jurnal, deschide
rezultatul curent în HTML. După Verifică planul sau Testează sunt incluse și detaliile
profilării fișierelor deja citite, cu tabele, titluri și evidențierea blocajelor și
avertizărilor. Nu citește din nou Access și nu pornește o nouă verificare pe server.
Fereastra poate fi redimensionată; **Salvează HTML…** exportă un fișier complet,
cu stilurile incluse. **Închide** / Escape revine la formularul principal.

Folosește controlul WebBrowser existent și Markdig 0.45.0 (aceeași versiune ca
ajutorul KBot.App). HTML-ul din textul sursă este dezactivat; imaginile și linkurile
active sunt eliminate. Stilurile urmăresc tema la deschiderea ferestrei.
Modificare de sursă: fără teste, compilare, lansare aplicație sau comenzi Git.

## Feluri de mâncare din TIP — revizie 15.10.2026

`VC_FeluriMancare` se construiește din valorile distincte `TIP` din MicDejun,
Dejun și Cina, pe toți anii unei gestiuni. Denumirile sunt „Felul 1”, „Felul 2”,
etc.; diferențele de majuscule, spații și zerouri inițiale se normalizează.
Explicațiile meselor rămân în tabelele meselor. Vechea tabelă Access
FeluriMancare nu mai este sursa acestui nomenclator.

`IdFel` țintă este cheia noului nomenclator, derivată din `TIP`; valoarea
`IdFel` din Access nu este folosită pentru această asociere. `TIP` și
`Explicatie` de pe rândul sursă se păstrează. Un TIP gol produce IdFel NULL;
un TIP nenul care nu respectă forma „Felul n” oprește importul cu o constatare
X13. Asocierea TIP → IDFel este inclusă în VC_IdMap și în constatările N11.
Versiunea regulilor planului este `VC05-MEAL-TYPES-2026-10-15`.

Pentru date deja migrate este pregătit `sql/VC_Feluri_din_TIP.sql`, de executat
manual în baza DC după completarea ID-urilor subunității și gestiunii.
Completează nomenclatorul din TIP și corectează referințele celor trei mese,
fără ștergerea înregistrărilor existente sau modificarea schemei.

Nu s-au executat SQL-uri, importuri, compilări sau teste pentru această revizie.

## Contul lotului OI/MF până la casare — revizie 18.10.2026

În 0.3.8.0, contul se moștenește din primirea BP în OI_Produse, urmărind
IDBO prin eventualele transferuri succesive. Conturile TC greșite din
Access sunt corectate în OI_Produse.IDM și în OI_Transfer/OI_Casare.IdMasina
înaintea scrierii în țintă. Nu se schimbă Access, cantitățile, prețurile,
valorile, semnele sau identificatorii bonurilor. Conturile BP distincte nu
se cumulează. Ciclurile, sursele lipsă și proveniențele care duc la conturi
diferite sunt blocaje X16. Conversiile N13 păstrează originalul și contul
țintă, cu explicația contului de proveniență din sursă.

Regula planului: VC05-OI-ACCOUNT-LINEAGE-2026-10-18.
Referință: ../REGISTRU_OI_MF.md. Schimbarea cere recompilarea migratorului
pentru importurile viitoare. Aplicația Python folosește deja aceeași
moștenire la citirea datelor existente, fără UPDATE automat. Nu s-au rulat
build, teste, importuri sau SQL pentru această revizie.
