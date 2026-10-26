# SLICE-VC03-07 — Migrare fiscală din KBotAdmin / Interogări unice

Data comună VC03-07…10: 2026-10-23. Parte din ansamblul identității fiscale a furnizorului (vezi [SLICE-VC06-18](SLICE-VC06-18-furnizori-exclusiv-cod-fiscal.md)).

Evoluția aceleiași migrări prin Interogări unice: garda inițială → diagnosticul utilizatorului → V2 pentru istoricul fără CF → V3 pentru partenerii ascunși. Stările și limitele de execuție rămân consemnate pe etapă.

<a id="slice-vc03-07"></a>

Pregătită sql/VC_20261023_FURNIZORI_CF_KBOTADMIN.sql pentru mecanismul existent routes/one_time/runner.py: fără START TRANSACTION/COMMIT și fără SELECT-uri finale ale căror rezultate runnerul le-ar consuma fără afișare. KBotAdmin păstrează tranzacția per bază și scrie registrul numai după reușita întregii migrări. Nu a fost schimbat runnerul sau KBotAdmin.

Datele migrează ca în scriptul manual. Înainte de final, o gardă în tabelă temporară oprește baza cu eroare duplicate-key explicită dacă există NIR_FARA_CF, FACTURA_FARA_CF sau CF_FACTURA_DIFERIT_DE_NIR. Cheia duplicată este intenționată și indică motivul; nu depinde de CHECK sau strict mode. Runnerul face rollback la modificările de date și nu marchează baza ca rulată. Pe eroare de AVACONT_SURSA, runnerul nu continuă la DC-uri. După corectarea datelor, aceeași interogare poate fi reluată; bazele deja marcate sunt sărite. Scriptul manual și cele trei SELECT-uri diagnostice rămân disponibile separat în VC_20261023_FURNIZORI_CF_DATE.sql.

Antetul scriptului de structură a fost adaptat pentru rulare și ca interogare unică, cu numele VC_20261023_FURNIZORI_CF_STRUCTURA. Poate rula pe AVACONT_SURSA + DC-urile bifate, apoi migrarea de date pe aceleași baze; alternativa structură manual pe sursă + sincronizare rămâne valabilă. DDL este pregătit în fișier și va fi executat exclusiv de utilizator.

Pași KBotAdmin: Interogări unice -> Din fișier pentru structură, nume VC_20261023_FURNIZORI_CF_STRUCTURA, baze bifate, Vezi (nu execută), Execută. Apoi Din fișier pentru varianta KBOTADMIN; SCHIMBAȚI explicit numele în VC_20261023_FURNIZORI_CF_KBOTADMIN (UI păstrează numele anterior dacă nu e gol), aceleași baze, Vezi (nu execută), Execută. Publicați online după reușita migrării. Interogarea include întotdeauna AVACONT_SURSA.

Fișiere: sql/VC_20261023_FURNIZORI_CF_KBOTADMIN.sql (nou); sql/VC_20261023_FURNIZORI_CF_STRUCTURA.sql (antet); implementare_vercon_v2.md; docs/worklog/SLICE-VC03-07-migrare-fiscala-kbotadmin-interogari-unice.md. Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

<a id="slice-vc03-08"></a>

## SLICE-VC03-08 — Diagnostic NIR_FARA_CF după rularea utilizatorului

Log furnizat de utilizator: Portare_parteneri_codfiscal, hash prefix 24beed22b0f0c6f2, 15 instrucțiuni. AVACONT_SURSA și 15 DC-uri OK; șapte DC-uri s-au oprit în garda NIR_FARA_CF: 000_DEMO,001_GR23,005_CEVM,012_GR30, 018_GRRS,030_SCTC,050_GRSA. Conform runnerului, datele din bazele eșuate sunt rollback și bazele nu sunt marcate. Cele trei rânduri raportate de mai multe baze OK sunt cele trei inserări tehnice în gardă, nu trei NIR-uri.

Pregătit numai SQL de diagnostic: sql/VC_20261023_FURNIZORI_CF_DIAGNOSTIC_KBOTADMIN.sql, nume Portare_parteneri_codfiscal_diagnostic. Nu modifică tabelele de date; folosește exclusiv tabele temporare și transmite un sumar prin eroarea intenționată DIAG_CF (runnerul existent nu afișează rezultate SELECT). Sumarul include numărul NIR-urilor fără CF după regula exactă a migrării, numărul cazurilor cu normalizare mascată de prioritate, indicii textuale STOC/SOLD INITIAL (doar diagnostic, fără reclasificare), până la trei exemple IdIntrare/Numar și context, existența rândului partener și a CF sursă. Mesajele duplicate-key pot fi trunchiate de server; prefixul păstrează numărătorile, apoi exemplele. Baza cu rezultate diagnostice rămâne nemarcată și se poate reinteroga; fără rezultate se marchează normal în registru.

Nu a fost modificat textul migrării originale deja executate pe unele baze; nu se va relansa alt text sub numele Portare_parteneri_codfiscal. Nu s-a relaxat garda, nu s-a inventat CF și nu s-a reclasificat vreun document. Fără acces MariaDB, SQL executat, teste sau verificări automate. Cauza concretă a documentelor rămâne de stabilit din diagnosticul utilizatorului.

<a id="slice-vc03-09"></a>

## SLICE-VC03-09 — Migrare VERCON CF v2: documentele istorice fără CF nu blochează baza

Diagnostic furnizat de utilizator (Portare_parteneri_codfiscal_test_1, hash prefix 14ccf76d81d6702c): N=33/1/147/9/7/2/1 pentru 000_DEMO/001_GR23/005_CEVM/012_GR30/018_GRRS/030_SCTC/050_GRSA, în total 200 de NIR-uri fără CF recuperabil prin sursele migrării. FMT=0 în toate bazele: nu sunt coduri valide mascate de ordinea normalizării. STOC=13/0/13/8/1/0/1, în total 36: numai indicii textuale, nu documente confirmate ca stoc inițial. Exemplele vizibile au P0,SRC0; nu se deduce că toate cele 200 au exact aceeași stare a vechii legături de partener. Diagnosticarea a creat doar tabele temporare, fără schimbarea datelor NIR. Numărul de rânduri OK include operațiile tehnice, nu documente modificate.

Garda inițială era prea strictă pentru istoricul cu informații lipsă. Pregătit sql/VC_20261023_FURNIZORI_CF_KBOTADMIN_V2.sql, nume NOU Portare_parteneri_codfiscal_v2. Poate rula pe cele șapte baze rămase (sau pe toate țintele originale); sursa este inclusă automat. Vechile fișiere/nume/hash-uri înregistrate rămân neschimbate.

V2 normalizează fiecare candidat fiscal înainte de alegerea priorității, completează CF-urile recuperabile, păstrează necunoscutele NULL și nu reclasifică NIR-uri pe baza textului STOC/SOLD INITIAL. Nu creează furnizori și nu deduce CF după nume. Nu există fallback de identitate în online; vechile chei sunt citite doar în fișierul de tranziție. Valorile/cantitățile/reviziile și coloanele istorice nu sunt modificate. DONE în registru înseamnă conversie aplicată, nu toate CF-urile rezolvate. Se păstrează rollback-ul pe contradicții între CF factura/NIR cunoscute și pe facturi care au NIR-uri legate cu mai multe CF-uri cunoscute. Un NULL și un CF cunoscut nu sunt tratate drept două identități contradictorii.

În arborii NIR/FCS, selectorul de sursă storno și ramurile de rapoarte, grupul fără CF este etichetat explicit Fără cod fiscal, nu cu numele istoric arbitrar al primului document. Cantitățile și numele istorice pe document rămân vizibile; grupul nu reprezintă un furnizor dedus din nume. Asocierea eFactura rămâne interzisă fără CF. Antetul comun de deployment indică V2 drept migrare recomandată; help descrie documentele nerezolvate.

Fișiere: sql/VC_20261023_FURNIZORI_CF_KBOTADMIN_V2.sql (nou); PYTHON/routes/vercon/{receipt_tree,report_incoming_selection, report_selection,supplier_identity}.py; PYTHON/static/js/vercon/{document-tree-order,storno-editor}.js; PYTHON/static/help/vercon.json; implementare_vercon_v2.md; docs/worklog/SLICE-VC03-09-migrare-cf-v2-documente-istorice-fara-cf.md.

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026). NIR-urile fără CF au nevoie în continuare de date fiscale explicite pentru a primi o identitate de furnizor; migrarea nu pretinde rezolvarea acestor informații absente.

<a id="slice-vc03-10"></a>

## SLICE-VC03-10 — Migrare CF v3: ignoră partenerii Ascuns=1

Clarificare explicită a utilizatorului: partenerii cu Ascuns=1 NU au NIR-uri sau alte documente. Nu este necesară și nu s-a implementat vreo logică de propagare/excludere pentru documente ale unor parteneri ascunși.

Pregătit sql/VC_20261023_FURNIZORI_CF_KBOTADMIN_V3.sql, nume NOU Portare_parteneri_codfiscal_v3. Păstrează V2 și adaugă doar AND COALESCE(P.Ascuns,0)<>1 la cele două căutări în Parteneri (recuperarea CF pentru NIR și factura comună). NULL este tratat ca 0. Tabela Parteneri nu este actualizată, ca și în variantele precedente. Documentele istorice fără CF nu blochează baza; contradicțiile între CF-uri cunoscute rămân blocante. Nu se inventează CF și nu se reclasifică documente.

Fișierele și interogările originale/V2 rămân neschimbate, deoarece pot fi deja înregistrate în Interogari_Unice. V3 folosește nume și hash noi; nu inversează schimbări ale unei rulari deja încheiate. Fișiere: sql/VC_20261023_FURNIZORI_CF_KBOTADMIN_V3.sql (nou); PYTHON/routes/vercon/supplier_identity.py (referință la V3); implementare_vercon_v2.md; docs/worklog/SLICE-VC03-10-migrare-cf-v3-parteneri-ascunsi.md. Stare: SQL PREGĂTIT, NEEXECUTAT de asistent. Fără teste, verificări automate/de sintaxă sau acces MariaDB. Nicio modificare de date/structură executată. De rulat de utilizator prin Interogări unice, cu numele V3.
