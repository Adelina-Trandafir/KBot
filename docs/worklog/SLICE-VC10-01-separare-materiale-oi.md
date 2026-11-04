# SLICE-VC10-01 — separarea conturilor din Materiale

Data: 03.11.2026. Stare: implementare în surse **0.3.19.0**, reguli `VC10-OI-OPTIONS-HTML-NULL-2026-11-03`. Fără compilare, teste, lansare aplicație, importuri, acces MariaDB sau DDL executate de agent. Înregistrarea pentru publicare nu a fost executată; este necesară confirmarea explicită a Adelinei.

## Cerințe actuale

- Fila custom VRC.Migrator citește bazele încărcate și separă conturi în gestiuni noi sub aceeași subunitate.
- `303 OBIECTE INVENTAR` merge la OI. `302.8.07 PROIECT SCOLAR - OBIECTE INVENTAR` **nu este OI**: destinație separată Materiale. Nu se deduce tipul din nume.
- Rândurile noi pornesc cu MATERIALE. Separă → OI + `Obiecte Inventar Folosinta`. Contul de folosință are TIP=OIF; contul `Obiecte inventar Magazie` are TIP=OIM. Produsele/intrările și BC-urile care scad magazia folosesc OIM, BP/registrul folosesc OIF.
- Combo Tip destinație include `Nu migra`: exclude contul, produsele și documentele sale eligibile fără destinație nouă. Nu depinde de bifa Separă. Se cere aceeași alegere pe toți anii contului; documentele amestecate cu un cont păstrat rămân semnalate, nu se șterg sau împart tacit.
- Grila deținătorilor este eliminată. Asocierile sunt automate: aceeași identitate locală în aceeași serie (folder/subunitate/gestiune) folosește cel mai recent nume disponibil. Numele normalizate identice se reunesc în destinație. Istoricul sursă/an/bon/nume → destinație apare în HTML. Regula include și persoanele scrise succesiv peste aceeași înregistrare; este explicată în raport. Vechile asocieri manuale salvate nu se mai aplică. O identitate lipsă fără nume recuperabil în serie cere corectarea sursei, fără persoană inventată.
- Produsele cu preț NULL sunt **omise fără blocaj**, conform ultimei instrucțiuni. Se elimină variantele și mișcările dependente, precum și liniile cu preț propriu NULL. Un produs fără nicio variantă păstrată se elimină. Documentele devenite goale se omit; la documentele parțiale antetele își păstrează valorile sursă, fără recalculare. Zero nu este NULL. Omiterea se aplică snapshoturilor Materiale încărcate în acest flux, inclusiv reziduului rămas în gestiunea originală.
- Caseta de log a separării este eliminată. Analiza/generarea deschide raportul HTML; butonul Raport HTML îl redeschide și fereastra existentă permite exportul complet.
- Toate erorile se colectează cu nume/documente/baze/produse. Documentele respinse nu produc afirmații concluzive despre stoc.

## Context citit și probe ale utilizatorului

În etapa inițială au fost citite numai copii temporare ale `tz_2022.accdb`–`tz_2026.accdb` din `C:\vercon\Scoala GEP\14`. **tz.accdb a fost exclus și nu a fost citit.** Copia JSON temporară de context a fost eliminată din proiect. Tabelele OI_Predare/OI_Produse din conturile discutate erau goale. Comparația inițială a surselor a găsit reporturi egale cu soldurile precedente, fără repetări exacte produs–preț–deținător între ani. Aceasta nu este un test al generatorului și nu dovedește identitatea fizică a obiectelor.

Adelina a probat prototipul și a furnizat logul cu schimbările de nume, prețurile NULL și NIR-urile care amestecă conturi. Continuarea 0.3.19 a citit acel text atașat și sursele locale; nu a recitit bazele Access. Problemele cu NIR mixt/cont antet diferit sunt explicate în raport, cu toate produsele și conturile lor. Împărțirea automată a acestor documente nu a fost autorizată sau implementată. Sold neconcluziv înseamnă documente insuficiente pentru calcul, nu lipsă de stoc demonstrată.

## Implementare

- `VrcOiData.vb`: snapshoturi, modele, amprente, aplicarea reziduului/destinațiilor și notelor de excludere la plan. Modele pentru raportul anual, stoc, documente și omiteri.
- `VrcOiNullPrices.vb`: copii de liste ale snapshotului, omiterea prețurilor NULL și dependentelor, inventarul omiterilor. Nu modifică datele sursă și nu inventează prețuri.
- `VrcOiHolders.vb`: asocieri automate pe seria anuală și rezolvarea numelor/referințelor. Remaparea țintă nu reutilizează chei Access neidentificate.
- `VrcOiBuilder.vb`: conversie BC → BP + BC aferent + registru; conturi OIM/OIF; reconciliere multianuală; diagnostic continuu; asocieri și suprapuneri; detalii structurate de document și stoc pentru raport.
- `VrcAccountSeparation.vb`: destinații multiple OI/Materiale, excluderea conturilor, pregătire fără NULL-uri și reziduuri intersectate pentru aceeași bază. Analiza permite numai omiterea NULL-urilor și dacă nu sunt conturi selectate, prin generarea explicită a opțiunilor.
- `VrcOiWorkspace*.vb`: browse read-only, conturi cu valori implicite, Nu migra, confirmări de predări distincte, raport HTML în loc de log/grilă deținători. Selecțiile/excluderile sunt memorate; generarea invalidată trebuie refăcută.
- `VrcOiHtmlReport.vb` / `.css`: HTML structurat și codificat ca text; fără scripturi/resurse externe. Destinații și excluderi; blocaje grupate + explicații; NIR/BC amestecate + linii; NULL-uri omise; istoric asocieri; ani/stoc; suprapuneri; documente candidate cu traseu, cantități, prețuri și valori sursă. Datele/numerotarea/prețurile/stocul au chei de sortare de tipul real.
- `VrcReportForm.vb`: constructor suplimentar pentru HTML deja formatat; refolosește fereastra și exportul existente, fără schimbarea raportului general.
- `VrcPlan.vb`: surse pregătite/separate, hash/export cu transformări și versiunea regulilor. `VrcRunner.vb` / `VrcRun.vb`: snapshoturi și note N22, inclusiv excluderi pe sursele reziduale; tranzacția existentă. `VrcSettings.vb`: alegeri persistente, fără persistarea rândurilor generate.

## Limite și continuare

Nu s-au executat verificări automate, build, teste, importuri sau comenzi Git. S-a făcut numai revizuire de proiectare în surse și editare. Nici raportul HTML nou nu a fost lansat/randat de agent; proba aplicației rămâne necesară, dacă Adelina o solicită. Nu sunt binare generate de agent și nu există fișiere Python proprii în acest task.

Pentru utilizare: recompilare de către utilizator, recitirea bazelor, încărcarea conturilor, OI pentru 303, Materiale separat pentru proiect, Nu migra unde este necesar în toți anii, apoi Analizează/Generează și consultarea HTML. Documentele mixte, corecțiile/stornările reale sau operațiile OI preexistente în Materiale necesită decizie explicită înaintea transformării; nu se extinde automat aria. Lipsa obiectelor din anul următor nu generează casări, iar folosința dinaintea primei baze nu se inventează.

Se păstrează interdicția de a atinge tz.accdb, de a executa DDL, de a rula teste/build fără cerere și de a înregistra publicarea fără confirmarea explicită a Adelinei.

Adelina a cerut oprirea când limita comună de 5 ore ajunge la **7% rămas**: `mcp__codex_app__get_usage_limits`, `rateLimitsByLimitId.codex.primary`, 300 minute, rămas=100-usedPercent. La usedPercent>=93 se consemnează punctul de continuare și se oprește. Nu se consumă credite de resetare. Ultima citire din această continuare: 8% folosit / 92% rămas în fereastra de 5 ore.

## Fișiere proprii pentru înregistrare după confirmarea explicită

1. `VERCON/VRC.Migrator/VRC.Migrator.vbproj`
2. `VERCON/VRC.Migrator/VrcMigratorForm.Designer.vb`
3. `VERCON/VRC.Migrator/VrcMigratorForm.vb`
4. `VERCON/VRC.Migrator/VrcPlan.vb`
5. `VERCON/VRC.Migrator/VrcRun.vb`
6. `VERCON/VRC.Migrator/VrcRunner.vb`
7. `VERCON/VRC.Migrator/VrcSettings.vb`
8. `VERCON/VRC.Migrator/VrcOiData.vb`
9. `VERCON/VRC.Migrator/VrcOiBuilder.vb`
10. `VERCON/VRC.Migrator/VrcOiWorkspace.Designer.vb`
11. `VERCON/VRC.Migrator/VrcOiWorkspace.vb`
12. `VERCON/VRC.Migrator/VrcAccountSeparation.vb`
13. `VERCON/VRC.Migrator/README.md`
14. `docs/worklog/SLICE-VC10-01-separare-materiale-oi.md`
15. `VERCON/VRC.Migrator/VrcOiHolders.vb`
16. `VERCON/VRC.Migrator/VrcOiNullPrices.vb`
17. `VERCON/VRC.Migrator/VrcOiHtmlReport.vb`
18. `VERCON/VRC.Migrator/VrcOiHtmlReport.css`
19. `VERCON/VRC.Migrator/VrcReportForm.vb`

Motiv concret: separarea/excluderea conturilor Materiale, conversie OI multianuală cu OIM/OIF, asocieri automate ale deținătorilor, omiterea prețurilor NULL și raport HTML explicativ cu detalii. Se înregistrează numai după confirmarea explicită, fără binare sau modificări străine.
