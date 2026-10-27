# SLICE-VC05-03 — writer atomic si maparea cheilor

Stare: **IMPLEMENTAT_LOCAL**. Se compileaza curat; **nerulat pe nicio baza**, nicio linie scrisa vreodata in MariaDB.

## Facut
- `tools/ddl.js` scrie acum si `VRC.Migrator/target_schema.json` (26 de tabele in ordinea parintilor, cu clasa fiecarei coloane si felul legaturii: FK declarat, legatura logica documentata, grup OI, furnizor, tehnic, neclasificat). Scriitorul se conduce dupa acest fisier, nu dupa 26 de mapari scrise de mana.
- `VrcRunner.vb`: o singura tranzactie: subunitate, gestiuni, exercitii, markerul VC_Imports, surse, apoi randurile (tabelele de gestiune o data, reunite pe ani: aceeasi cheie = aceeasi entitate, anul mai nou castiga atributele; apoi tabelele de exercitiu pe fiecare an), harta VC_IdMap, constatarile, contoarele, soldul initial Alimente.
- Valori exacte INT x 10^scara; refuz (nu rotunjire) la prea multe zecimale sau overflow. Cota TVA x 10000.
- Reguli aplicate: N01/N02 (totalul liniilor cu TVA), pret negativ => pret pozitiv + cantitate negativa (avertisment, numai Intrari/Iesiri/IesiriRetur), ambele negative = blocaj, ProduseP numai cand are folosire, numar NIR negativ numai pentru stoc initial («stoc» in NrFact; TipDoc STOC_INITIAL), furnizor => Parteneri KBOT dupa cod fiscal (lipsa = fara partener + avertisment, CodFiscalSursa pastrat), contoarele pornesc de la ultimul numar real (NIR, BC, LISTA), nu din NR.*.
- Legaturile fara parinte hotarat (IdFel, IdRetur, IdStorno, IDBO, IDM, IdMasinaP) blocheaza doar daca au valori nenule; AutoNumber-urile secundare (LABC.ID, ProduseM.IDPMF) se pastreaza NULL.

## Limite / presupuneri de confirmat
- Numerele NIR cu zecimale (ex. 707,1 = document inserat cronologic) se pastreaza: partea intreaga in Numar, fractia in sutimi in VC_NIR.NumarSub (coloana noua); mai mult de 2 zecimale blocheaza.
- Legaturile logice rezolvate dupa denumirea parintelui: IdMasina -> Masini, IntrariStorno.IdIntrare -> NIR, IntrariStorno/ProduseM.IdProdus -> ProduseP, OI_Transfer.IDC_OLD -> Clienti, OI_Predare.IdBonConsum -> Iesiri_H.
- OI/MF: tabelele OI nu s-au vazut cu date; grupurile IDBP/IDBT/IDBC sunt renumerotate pe gestiune, neprobate.
- Performanta nemasurata (inserare pe rand in tranzactie unica).

## SLICE-VC05-04 — oprire, jurnal si diagnostic <a id="slice-vc05-04"></a>

Stare: **IMPLEMENTAT_LOCAL**. Compilat; nerulat.

### Facut
- Stari: NECITIT, PLAN_CITIT, BLOCAT, TESTAT, IN_IMPORT, OPRIRE_SOLICITATA, ROLLBACK_CONFIRMAT, COMMIT_CONFIRMAT, REZULTAT_NECUNOSCUT, RECONCILIAT (afisate in formular).
- Opreste anuleaza inainte de COMMIT (verificat la tabele si la fiecare 200 de randuri); dupa trimiterea COMMIT-ului nu se promite anulare.
- COMMIT fara raspuns => REZULTAT_NECUNOSCUT, cu indicatia de a folosi Diagnostic; transferul nu se reia singur.
- Diagnostic: citeste markerul VC_Imports, sursele si numarul de randuri pe tabel pentru subunitate; nu insereaza nimic.
- Jurnal: fereastra + fisier `Logs\vrc_migrare_<data>.log` si raportul `.md` alaturat; fara parole.
- O migrare confirmata dezactiveaza definitiv Migreaza in sesiune; pe server, VC_Imports unic pe subunitate o interzice si la o sesiune noua (Testeaza o respinge).
- Formularul nu se inchide cat timp ruleaza o operatie.

## SLICE-VC05-05 — reconciliere si raport <a id="slice-vc05-05"></a>

Stare: **IMPLEMENTAT_LOCAL**. Compilat; **nicio reconciliere reala efectuata**.

### Facut
- Inainte de COMMIT, scriitorul citeste inapoi din MariaDB numarul de randuri si sumele coloanelor de valoare pe fiecare (gestiune, exercitiu, tabel) si le compara cu ce a convertit; orice diferenta sau constatare BLOCAJ => rollback.
- Raport Markdown: numaratori sursa/netransferate/tinta/sume, constatari grupate pe regula (cu exemple), durata, ImportId.
- Rezumat JSON in VC_Imports.Summary si constatarile in VC_MigrationIssues (valoarea originala ramane langa cea tinta).
- Raportul de profilare numara acum ProduseP cu folosire si semnele cantitate/pret pe liniile documentelor.

### Neacoperit
- Comparatia pe cheie si pe NULL/FK camp cu camp (FK le verifica MariaDB la inserare); soldurile anuale nu sunt recalculate independent.
- Proba cu diferenta introdusa deliberat, intreruperea lotului si refuzul dupa marker cu fisier redenumit nu au fost rulate.

## Revizie 12.10.2026 (dupa prima rulare pe 000_DEMO)
- Prima rulare (anulata prin rollback, nimic scris) a gasit un blocaj real: `NIR.IdStorno` (NIR 545 → 532). Din formularele FC/FCS (`DCount("IdIntrare","NIR","IdStorno=" & IdIntrare)`, `DELETE FROM NIR WHERE IdStorno=`) rezulta ca IdStorno este IdIntrare al NIR-ului stornat: legatura logica NIR → NIR. O referinta la un rand din ACEEASI tabela se scrie NULL si se actualizeaza dupa ce toate randurile au cheia noua (`ResolveDeferred`).
- Trimiterea datelor in stilul ADE.Migrator: INSERT multi-rand (pana la 500 de randuri / 20.000 de parametri), apoi citirea cheilor noi (`ReadNewKeys`); un rand refuzat de MariaDB strica doar lotul lui, care se reia pe rand pentru a-l identifica (constatare X07). Inainte: un drum la server pe rand.

## Revizii numerice și de copiere — 14.10.2026

Etapele inițiale INT/scară și de recalculare descrise mai sus sunt istorice, înlocuite prin decizia explicită din 14.10: [stocare DECIMAL directă, precizie, TVA și ordinea demo](../../VERCON/CONTRACT_CALCULE.md#1-stocare-decimal--decizie-14102026). În aceste două revizii au fost pregătite surse/SQL, fără aplicare pe server, compilări, teste, probe sau migrări; sunt necesare aplicarea manuală/sincronizarea schemei, migratoare/aplicație actualizate și reimport demo. Datele EF deja rotunjite nu se recuperează prin ALTER.

- Vechea migrare VC exista numai în demo, aprobată pentru golire/reluare, fără conversia datelor existente. `sql/VC_DEMO_golire_pentru_decimal.sql` golește numai datele VC din 000_DEMO, inclusiv markeri/audit, păstrând tabele/interogări/alte module. `sql/VC_precizie_valori_4_zecimale.sql` înlocuiește propunerea BIGINT; `sql/00EF_24_primite_linii_precizie.sql` include EF emise/primite și totalurile TVA.
- VrcConvert/VrcRunner/VrcPlan/VrcReport/VrcWriter trec la conversie/reconciliere decimală directă, precizie fixă 4, verificarea capacității și blocare înainte de schema INT veche. KBot.Migrator EfImporter/EfTableSpec cer DECIMAL(20,4) pentru cantități/sume; Corectata rămâne întreg. Clasa internă VALOARE-INT rămâne cu tip efectiv Decimal.
- Web VC (numeric/documente/nomenclatoare/bonuri/OI/MF/liste/stoc/rapoarte/amortizare) folosește Decimal direct, fără conversii int care taie fracțiuni; TVA fracție fără x10000. Zec pentru calcul/afișare este separat de stocarea fixă 4, iar schimbarea lui nu rescrie documentele.
- Revizia ulterioară scoate ApplyHeaderTotal/ApplyPriceSign și agregările de completare a antetelor din VrcRunner: NIR/BC inclusiv 0/NULL, prețurile și cantitățile se copiază cu semnul lor. Negativele sunt avertismente în VrcPlan/VrcReport, nu blocaje sau corecții; logul explică regula. Numerotarea după ultimul număr real rămâne explicit neschimbată. Reconcilierea sursă/țintă rămâne; diferența antet/linii nu este diferență de copiere. Validările/calculul documentelor noi din web rămân.
- Generatorul ddl.js și VC_Reguli_DC.sql nu mai adaugă CHECK de preț nenegativ. SQL manual `VC_IMPORT_valori_sursa.sql` elimină numai CHECK-urile vechi din AVACONT_SURSA și 000_DEMO, dacă există; nu a fost executat.

Fișiere istorice ale primei etape: `VC_AVACONT_SURSA.sql`, `00EF_02_efactura_unitate.sql`, `00EF_17_primite_tva.sql`, `VERCON/tools/ddl.js`, `lib_tz.js`, `ddl_parts/tables_01.sql` și `tables_03.sql`; CONTRACT_CALCULE, DICTIONAR_DATE, generatorul dicționarului și README migrator au fost aliniate. A doua etapă actualiza și DECIZII_DESCHISE. Generatorul complet și target_schema au fost retrase ulterior în [VC06-06](SLICE-VC06-06-facturi-comune-corectii-nir-si-schema-unica.md#calcul-factură-și-schemă); căile de aici sunt proveniență, nu sursa actuală de schemă.
