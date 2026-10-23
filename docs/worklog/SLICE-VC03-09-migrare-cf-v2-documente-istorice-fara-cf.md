# SLICE-VC03-09 — Migrare VERCON CF v2: documentele istorice fără CF nu blochează baza

Data: 2026-10-23. Parte din ansamblul identității fiscale a furnizorului (vezi [SLICE-VC06-18](SLICE-VC06-18-furnizori-exclusiv-cod-fiscal.md)).

Diagnostic furnizat de utilizator (Portare_parteneri_codfiscal_test_1,
hash prefix 14ccf76d81d6702c): N=33/1/147/9/7/2/1 pentru
000_DEMO/001_GR23/005_CEVM/012_GR30/018_GRRS/030_SCTC/050_GRSA,
în total 200 de NIR-uri fără CF recuperabil prin sursele migrării.
FMT=0 în toate bazele: nu sunt coduri valide mascate de ordinea normalizării.
STOC=13/0/13/8/1/0/1, în total 36: numai indicii textuale, nu documente
confirmate ca stoc inițial. Exemplele vizibile au P0,SRC0; nu se deduce
că toate cele 200 au exact aceeași stare a vechii legături de partener.
Diagnosticarea a creat doar tabele temporare, fără schimbarea datelor NIR.
Numărul de rânduri OK include operațiile tehnice, nu documente modificate.

Garda inițială era prea strictă pentru istoricul cu informații lipsă.
Pregătit sql/VC_20261023_FURNIZORI_CF_KBOTADMIN_V2.sql,
nume NOU Portare_parteneri_codfiscal_v2. Poate rula pe cele șapte baze
rămase (sau pe toate țintele originale); sursa este inclusă automat.
Vechile fișiere/nume/hash-uri înregistrate rămân neschimbate.

V2 normalizează fiecare candidat fiscal înainte de alegerea priorității,
completează CF-urile recuperabile, păstrează necunoscutele NULL și nu
reclasifică NIR-uri pe baza textului STOC/SOLD INITIAL. Nu creează
furnizori și nu deduce CF după nume. Nu există fallback de identitate
în online; vechile chei sunt citite doar în fișierul de tranziție.
Valorile/cantitățile/reviziile și coloanele istorice nu sunt modificate.
DONE în registru înseamnă conversie aplicată, nu toate CF-urile rezolvate.
Se păstrează rollback-ul pe contradicții între CF factura/NIR cunoscute
și pe facturi care au NIR-uri legate cu mai multe CF-uri cunoscute.
Un NULL și un CF cunoscut nu sunt tratate drept două identități contradictorii.

În arborii NIR/FCS, selectorul de sursă storno și ramurile de rapoarte,
grupul fără CF este etichetat explicit Fără cod fiscal, nu cu numele
istoric arbitrar al primului document. Cantitățile și numele istorice pe
document rămân vizibile; grupul nu reprezintă un furnizor dedus din nume.
Asocierea eFactura rămâne interzisă fără CF. Antetul comun de deployment
indică V2 drept migrare recomandată; help descrie documentele nerezolvate.

Fișiere: sql/VC_20261023_FURNIZORI_CF_KBOTADMIN_V2.sql (nou);
PYTHON/routes/vercon/{receipt_tree,report_incoming_selection,
report_selection,supplier_identity}.py;
PYTHON/static/js/vercon/{document-tree-order,storno-editor}.js;
PYTHON/static/help/vercon.json; implementare_vercon_v2.md;
docs/worklog/SLICE-VC03-09-migrare-cf-v2-documente-istorice-fara-cf.md.

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026). NIR-urile fără CF au nevoie în continuare de date fiscale explicite pentru a primi o identitate de furnizor; migrarea nu pretinde rezolvarea acestor informații absente.
