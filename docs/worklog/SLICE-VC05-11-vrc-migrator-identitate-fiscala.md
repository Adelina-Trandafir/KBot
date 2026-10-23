# SLICE-VC05-11 — VRC.Migrator: aceeași identitate fiscală ca VERCON online

Data: 2026-10-23. Parte din ansamblul identității fiscale a furnizorului (vezi [SLICE-VC06-18](SLICE-VC06-18-furnizori-exclusiv-cod-fiscal.md)).

Cerere: alinierea strict a VRC.Migrator la fluxul fiscal VERCON, cu protejarea
celorlalte module și a partenerilor Ascuns=1 (care nu au documente).

Eliminat dicționarul CF → IdPartener și încărcarea ID-urilor din Parteneri.
Importul NIR scrie direct CF normalizat în CodPartener, păstrând separat
CodFiscalSursa; facturile provizorii copiază aceeași identitate din NIR în
VC_FACTURI.CodPartener, fără PARTENER_ID. Lipsa rândului din Parteneri nu
anulează CF-ul sursă. Lipsa CF/IDF nerecunoscut produce X04, valoare NULL;
CF mai lung de 50 produce X07, fără trunchierea identității. Stocul inițial
păstrează regula existentă și identitatea NULL. Normalizarea elimină RO doar
înainte de 0–9, ca implementarea online, păstrând literele și zerourile inițiale.

Actualizate mesajele preflight/raport/ANAF. Setul codurilor existente include
și partenerii ascunși EXCLUSIV ca gardă împotriva propunerii/creării unui
duplicat: nu se modifică/reactivează niciun partener existent. AddInTransaction
normalizează și codul pregătit înaintea verificării existenței și a inserării.
Numerele locale deja ocupate (inclusiv ascunse) se rezervă ca înainte pentru
a evita coliziuni; codul local Parteneri.CodPartener nu leagă documentele.

Controlul de schemă folosit de Testează și înaintea tranzacției cere CodPartener
textual, scriibil, minimum 50, NULL permis, pe VC_NIR și VC_FACTURI; ID-urile
istorice, dacă există, trebuie să permită NULL. DDL-ul existent este doar indicat
utilizatorului, nu executat. Nu s-a introdus rularea automată a V3, reimport,
actualizare a documentelor istorice sau creare automată de schemă.
Versiune fișier VRC.Migrator: 0.3.12.0; RuleVersion:
VC05-SUPPLIERS-CF-2026-10-23. CPV din Access păstrează maparea existentă;
asocierea eFactura rămâne responsabilitatea aplicației online.

Fișiere: VERCON/VRC.Migrator/VrcRunner.vb, VrcRunner.Invoices.vb,
VrcWriter.vb, VrcWriter.Schema.vb, VrcConvert.vb, VrcPartnerStore.vb,
VrcMigratorForm.vb, VrcPartnersForm.vb, VrcRun.vb, VrcImportMap.vb,
VrcPlan.vb, VRC.Migrator.vbproj și README.md; jurnalul de față și
implementare_vercon_v2.md. Nicio modificare în src/KBot.Migrator,
ADE.Migrator, migratorul contabil, DDF/ORD/extrase sau maparea produselor.

Stare: SCRIS LOCAL, NEPUBLICAT. Citire și analiză de proiectare în surse;
fără teste, build, verificări automate/de sintaxă, rulare de executabil,
importuri sau acces MariaDB/DDL, conform AGENTS.md. Executabilul existent
în _VrcMigrator trebuie recompilat/publicat înainte de folosirea regulii noi.
