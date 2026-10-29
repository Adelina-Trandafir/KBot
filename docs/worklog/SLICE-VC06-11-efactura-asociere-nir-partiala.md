# SLICE-VC06-11 — Asociere NIR manual cu eFactura — 23.10.2026

Stare comună VC06-11/13/14/16/17: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

Evoluția aceluiași dialog de asociere: bază tranzacțională și propuneri → cantități/grile → drag and drop/memorare → protecția NIR și PDF → selector automat. Fiecare etapă își păstrează limitele și fișierele; VC06-12 (gruparea arborelui) și VC06-15 (informarea/vizibilitatea globală) rămân separate.

<a id="slice-vc06-11"></a>

## Utilizare

Pentru un NIR de cumpărare salvat, neanulat, accesul din meniu urmează [VC06-12](SLICE-VC06-12-nir-popup-asociere-si-grupare-furnizori.md). Dialogul oferă facturile active trimise de contabil, cu aceeași dată a facturii și același CUI ca NIR-ul. Numărul facturii nu este criteriu de asociere, nici la selecție, nici la salvare sau la calculul valabilității alocărilor; seria și spațiile pot diferi. NIR-ul trebuie deschis în contextul gestiunii și exercițiului său, cu drept de recepție, ocupare și perioadă deschisă. Pentru alte NIR-uri se schimbă contextul în mod normal.

Listele sursă sunt alfabetice românești. Grilele, asocierea prin drag and drop/buton și editarea/eliminarea perechilor sunt detaliate în [VC06-13](#slice-vc06-13) și [VC06-14](#slice-vc06-14). Cantitățile celor două documente sunt independente, pentru conversii explicite de UM; UI calculează cu BigInt la patru zecimale, serverul cu Decimal.

O poziție poate fi împărțită între rânduri și NIR-uri, inclusiv în subunități diferite. Asocierea înlocuiește numai perechile NIR-ului curent pentru trimiterea aleasă. Celelalte NIR-uri, afișate cu subunitate/gestiune/număr/data, își păstrează alocările. În etapa inițială nu se modificau NIR-uri, facturi interne VC_FACTURI, produse, prețuri sau mișcări de stoc. Excepția ulterioară pentru CPV este consemnată în corecțiile din 23.10 de mai jos.

## Propuneri și memorare

Ordine: denumire memorată univoc pentru CUI/gestiune, apoi CPV exact normalizat la opt cifre; memoria nu rezolvă CPV-uri contradictorii. La CPV/produse repetate se compară disponibilul. Propunerea cere univocitate în ambele sensuri și aceeași UM, fără nume aproximative; ambiguitățile/UM diferite se rezolvă manual.

Propunerile rămân ciorne până la salvare; [VC06-14](#slice-vc06-14) definește eliminarea bifei și eligibilitatea memorării pe pereche. Corespondența păstrează CUI, denumirea normalizată eFactura, produsul din gestiune, ambele denumiri, utilizatorul și data. Identitatea produsului supraviețuiește redenumirii; mai multe produse memorate pentru același nume nu sunt alese arbitrar.

## Tranzacții și compatibilitate

Scrierea cere ocuparea contextului, lock-ul gestiunii, revizia NIR și lock-ul facturii/trimiterii comun cu retragerea contabilului. Tranzacția recalculează bugetele tuturor NIR-urilor și refuză depășirile ambelor documente. Cheia de cerere permite reluarea după răspuns pierdut; auditul păstrează perechile, cantitățile și memorarea.

Starea efectivă: TRIMISA / PARTIALA / PRELUATA (ultima numai când toate cantitățile sunt acoperite). Baza veche stochează doar TRIMISA/PRELUATA; PARTIALA se calculează identic în VERCON/K-BOT. Până la eliminarea asocierilor active, factura parțială nu poate fi retrasă/returnată și nici conturile propuse schimbate.

Asocierea reține revizia monotonă a NIR: editarea invalidează asocierile acelei revizii și cere reconfirmare, chiar dacă ulterior se revine la aceleași cantități; anularea/dispariția eliberează cantitățile. NIR-urile legate integral prin fluxul vechi nu se convertesc automat. După trecerea trimiterii pe alocări, restul cere NIR manual și asociere, fără preluare integrală veche.

## Instalare și fișiere

Utilizatorul execută manual `sql/EV_02_vercon_asocieri_partiale.sql` pe AVACONT_SURSA și sincronizează structura în DC-uri, după EV_01. Sunt trei tabele locale noi, cu identificatori uppercase: EF_VERCON_ASOCIERI, EF_VERCON_ALOCARI, EF_VERCON_CORELARI. Nu este SQL pentru AVACONT_COMUN. Aplicația nu creează schema; opțiunea răspunde EF_ASSOC_SCHEMA_REQUIRED dacă aceasta lipsește. Fluxul vechi continuă să funcționeze fără schema nouă.

Fișiere noi: scriptul SQL; PYTHON/utils/vercon_efactura.py; PYTHON/routes/vercon/efactura_association.py; PYTHON/static/js/vercon/efactura-association.js și efactura-match.js.

Integrare: PYTHON/routes/vercon/{doc_routes,efactura_queue}.py; PYTHON/routes/efactura/primite_vercon.py; PYTHON/static/js/vercon/{docs,efactura-intake}.js; PYTHON/static/css/vercon-efactura.css; PYTHON/static/help/vercon.json; src/KBot.Domain/EFacturaVercon.vb; src/KBot.EFactura/Forms/VerconForm.vb; implementare_vercon_v2.md.

Publicare necesară: modulele Python/JS/CSS/help împreună, repornirea serviciului web, reîncărcarea paginii și recompilarea K-BOT pentru textul stării parțiale. Nu s-a făcut publicare sau execuție SQL în această lucrare.

## Limită istorică a corecției criteriilor

La corecția CUI + data facturii, fără condiție pe număr/serie/spații, jurnalul general consemna explicit: fără DDL nou și fără teste sau alte verificări executate. Confirmarea ulterioară a operatorului din 23.10.2026 se păstrează separat de această probă locală absentă.

## Corecțiile asocierii — 23.10.2026 <a id="d001-12-efactura"></a>

Patru etape preluate prin D001-12, toate **Confirmat 23.10**. Regula inițială fără modificarea produselor primește mai jos excepția CPV; egalitatea textuală UM primește echivalența KG/KGM. Celelalte protecții de mai sus rămân.

- vc-efi__status trece din corp în footer, între Închide și Salvează asocierile, în locul spacerului. Ocupă spațiul flexibil, cu butoanele la extremități și când mesajul e gol.
- Cauza CPV: poziția era citită numai pentru propuneri; salvarea nu citea Cpv și nu actualiza VC_Produse. Acum toate perechile validate copiază CPV-ul poziției în produsul subunității/gestiunii NIR, independent de memorie/metoda asocierii. Codul complet se rezolvă în AVACONT_COMUN.CPV după opt cifre, inclusiv surse cu cifra de control fără separator. Lipsa/formatul invalid nu șterg CPV existent. Codul absent din catalog sau mai multe CPV-uri pentru același produs în cerere opresc întreaga salvare cu mesaj.
- CPV și auditul sunt în tranzacția asocierii; Version/DataSalvare se actualizează numai la schimbare. Cererea confirmată repetată folosește răspunsul idempotent. Eliminarea asocierii nu restaurează vechiul CPV; cele existente se pot resalva pentru copiere. Vechiul flux integral, fără maparea pozițiilor, rămâne exclus.
- Diagnosticul inițial compara deja Parteneri.CodFiscal normalizat cu EF_Primite.CuiNormalizat, fără nume; ID identifica numai partenerul NIR. Același mesaj ascundea și nepotrivirea VC_NIR.DataFc/EF_Primite.DataFact. Mesajele sunt separate pentru furnizor absent, CUI lipsă/diferit și dată lipsă/diferită, includ valorile și explică Data facturii față de Data NIR, fără relaxarea criteriilor. Cazul concret nu poate fi atribuit unei condiții fără valorile celor două documente.
- KG/KGM devin echivalente fără conversie numerică. Competitorii propunerii folosesc aceleași criterii de eligibilitate ca perechea, evitând blocarea de către o poziție cu UM incompatibilă sau o memorie neeligibilă. Rămân necesare rest pozitiv, CPV/memorie și univocitate. Încărcarea dialogului poate deja adăuga propuneri înainte de Propune.
- **Corecția diagnosticului fiscal:** Parteneri negăsit prin IdPartener nu dovedește lipsa furnizorului fiscal. Identitatea poate fi în CodPartener (coloana pregătită P122) sau, la migrare fără legătură rezolvată, CodFiscalSursa. Nu au fost consultate datele utilizatorului și nu se declară una dintre situații confirmată în baza lui.
- Rezolvarea comună ia primul cod normalizat nevid, în ordine: CodPartener dacă există coloana; CodFiscal din Parteneri legat; CodFiscalSursa dacă disponibil. Aceeași expresie în lista facturilor, deschidere/salvare și proiecția valabilității alocărilor, inclusiv bugete/stare/protecția NIR. Nu modifică automat IdPartener/antetul. SQL normalizează prin CASE/SUBSTRING, fără backreference dependent de backslash, inclusiv CUI eFactura.
- Dialogul trimite numai ID NIR curent și contextul verificat. Serverul recitește NIR în subunitate/gestiune/exercițiu, impunând CUI + Data facturii independent de `partner` opțional din browser. Respinge Include_completed fără NIR valid inclusiv pentru JS vechi și oprește lista la CUI nedeterminabil. Active/Inactive/Toate și refresh păstrează filtrul; preluarea generală și informarea globală rămân separate. Această corecție a asocierii nu declară implementată migrarea generală 0122-05 pe CodPartener.

Fișiere istorice (numele fără prefix continuă directorul precedent):

| Etapă generală | Fișiere |
|---|---|
| 20 | PYTHON/static/js/vercon/efactura-association.js; PYTHON/static/css/vercon-efactura.css. |
| 22 | PYTHON/routes/vercon/efactura_product_cpv.py (nou); PYTHON/routes/vercon/efactura_association.py; PYTHON/static/help/vercon.json; implementare_vercon_v2.md. |
| 23 | PYTHON/routes/vercon/efactura_association.py; PYTHON/static/js/vercon/efactura-match.js; PYTHON/static/help/vercon.json; implementare_vercon_v2.md. |
| 24 | PYTHON/utils/vercon_efactura.py; PYTHON/routes/vercon/{efactura_association,efactura_queue}.py; PYTHON/static/js/vercon/efactura-association.js; PYTHON/static/help/vercon.json; implementare_vercon_v2.md. |

<a id="slice-vc06-13"></a>

## SLICE-VC06-13 — eFactura: DGV și cantități automate la asocierea NIR — 23.10.2026

### Asocierea NIR/eFactura

Tabelul HTML al perechilor a fost înlocuit cu DataGrid-ul comun, prin același adaptor grid/column folosit la NIR, BC și bonuri. Cantitățile se editează în celule. Opțiunea de eliminare este în meniul ⋮ al rândului. Grila păstrează sortarea aleasă și lățimile la schimbarea perechilor; ordinea inițială este după denumirile produselor în română, cu cantități comparate numeric. Etichetele includ UM, dar cheile de sortare ale produselor sunt denumirile.

La drag and drop sau la asociere prin buton, ambele cantități se completează cu minimul disponibil în NIR și eFactura, inclusiv când UM diferă textual, ca KG/KGM. Nu se deduce un factor de conversie; utilizatorul poate ajusta separat cele două cantități. La confirmarea celulei se validează valori pozitive, maximum patru zecimale și restul documentului corespunzător, scăzând toate celelalte perechi. Serverul păstrează verificarea tranzacțională a resturilor dintre toate NIR-urile.

Celula activă se confirmă înainte de salvare, asociere nouă, propuneri sau eliminare. Un edit invalid împiedică salvarea. Grila este ocupată în timpul cererii și este distrusă la închiderea dialogului. Stilurile butoanelor mari ale dialogului sunt limitate la acțiunile dialogului, fără a afecta butoanele și editorii DGV.

### SQL și livrare

Cele trei declarații din sql/EV_02_vercon_asocieri_partiale.sql au acum CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci, conform precizării utilizatorului. Nu este necesară schimbarea structurii pentru DGV/cantități; nu s-a executat SQL și nu s-au modificat baze de date.

Fișiere: PYTHON/static/js/vercon/efactura-association-grid.js (nou), efactura-association.js; PYTHON/static/css/vercon-efactura.css; PYTHON/static/help/vercon.json; sql/EV_02_vercon_asocieri_partiale.sql; implementare_vercon_v2.md; acest jurnal.

Publicare necesară: modulele JS și CSS/help împreună, apoi reîncărcarea paginii. Scriptul SQL rămâne numai pentru execuție manuală de către utilizator, la instalarea structurii.

<a id="slice-vc06-14"></a>

## SLICE-VC06-14 — eFactura: grile sursă, țintă vizibilă și memorare implicită — 23.10.2026

Eliminate complet din dialog paragraful vc-efi__hint și eticheta/bifa vc-efi__remember. Cele două butoane de asociere/propunere sunt pe rândul selectorului facturii, după «Deschide factura». Selectorul cedează din lățime; bara nu se înfășoară pe mai multe rânduri la îngustarea ecranului.

Ambele liste de sus folosesc DataGrid-ul comun NIR/BC, cu denumire, CPV, UM, cantitate și rest. Tabelul din stânga are butonul ↕️ pe rând; acesta deschide alegerea unei poziții eFactura cu rest. Modelul provine din ADECHIT Prezență: actionColumn/pickGroupFor și wireChildDrop din app.js.

Drag and drop folosește rowFromNode și evenimentele comune DGV. Rândul eFactura aflat sub cursor este marcat cu is-drop-target, fundal colorat și contur punctat. Nu se acceptă antete, zone goale, poziții fără rest sau drag extern. Evidențierea se curăță la ieșire, drop, dragend, reîncărcare, ocupare ori închidere. Identificarea țintei urmărește rândul real al DGV, inclusiv după sortare, filtrare și virtualizare.

Pozițiile eFactura cu cantitatea integral asociată dispar imediat din lista de sus. Produsele NIR cu rest zero dispar și ele. Perechile rămân în grila asocierilor, pentru editare/eliminare. Reducerea sau eliminarea unei alocări reintroduce automat rândurile cu rest pozitiv. Se păstrează filtrele, lățimile, sensul de sortare ales și selecția dacă rândul mai este vizibil.

Memorarea trece de la flag global la `learn=true` pe pereche, numai după drag and drop ori editarea manuală a cantității. Propunerile automate/butonul Propune și asocierile prin selecții/↕️ nu o marchează; editarea ei ulterioară o poate marca, editarea altui rând nu. Serverul memorează numai perechile marcate, în tranzacția «Salvează asocierile», cu audit. Eliminarea perechii înainte de salvare elimină marcajul, păstrând istoricul denumirilor deja memorate.

Fișiere: PYTHON/static/js/vercon/efactura-source-grids.js (nou), efactura-association.js, efactura-association-grid.js; PYTHON/routes/vercon/efactura_association.py; PYTHON/static/css/vercon-efactura.css; PYTHON/static/help/vercon.json; docs/worklog/VERCON-2026-10-23-efactura-asociere-nir-partiala.md; implementare_vercon_v2.md; acest jurnal.

Nu necesită DDL sau schimbări de schemă. Publicare: serverul Python și modulele JS/CSS/help împreună, repornirea serviciului web și reîncărcarea paginii. Structura existentă rămâne utf8mb3/utf8mb3_general_ci; nu s-a executat SQL.

<a id="slice-vc06-16"></a>

## SLICE-VC06-16 — eFactura: overflow, NIR asociat protejat și afișare PDF — 23.10.2026

### Dimensiunile dialogului

Paddingul DGV de 22 px pe fiecare parte depășea lățimea calculată de 40 px a coloanelor _move/_action, fără headerAction, producând scroll orizontal în grila NIR goală și în asocieri. Numai în dialog, aceste antete primesc padding/gap zero; overflow-ul și DGV-ul comun nu sunt modificate.

ComboBox-ul era limitat la max-width:300px din stylesheetul comun. Selectorul facturii are acum o bază flexibilă de 460 px și max-width:none, limitat la acest dialog; se poate restrânge pe ecrane înguste.

### NIR-urile asociate

efactura_receipts.py calculează metadatele pentru lotul de NIR-uri al unei ramuri: asocieri integrale din fluxul vechi și alocări parțiale valabile din fluxul nou. Datele sunt în contextul subunitate/gestiune/exercițiu. Asocierile expirate sau eliminate nu blochează NIR-ul. Fără tabelele eFactura existente, citirea documentelor continuă fără aceste metadate.

Lista lazy, lista generală și documentul deschis primesc has_efactura, lista facturilor asociate și edit_locked/edit_message. Nodurile sunt bold italic, inclusiv valoarea. Rendererul TreeView acceptă opțional italic; stilul este limitat la NIR. După salvarea/eliminarea asocierilor se reîncarcă documentul și ramurile deschise, păstrând gruparea și selecția.

Modifică este dezactivat atât în popup, cât și în bara documentului. _guard_dependents refuză și pe server editarea NIR-ului asociat cu EF_NIR_LOCKED, înainte de calcul sau scriere; acoperă și validarea ciornei. Asocierea poate fi corectată/eliminată prin dialogul existent. Regula anulării documentelor nu este schimbată.

### Afișarea facturii

Meniul unui NIR asociat oferă «Afișează eFactura». Se deschide factura originală în vizualizatorul PDF comun, reutilizând primite.pdf_anaf și XML-ul salvat. PDF-ul este randat de serviciul ANAF existent. Dacă sunt mai multe facturi, utilizatorul alege din lista ordonată după furnizor/data/număr.

GET /api/vercon/doc/nir/<nir>/efactura/<invoice>/pdf cere sesiunea și drepturile VERCON existente, validează contextul și verifică asocierea activă a facturii cu acel NIR. Nu permite accesul la altă factură prin înlocuirea ID-ului în URL. Erorile XML/ANAF sunt transmise prin protocolul VERCON; nu s-a făcut nicio cerere ANAF în această lucrare.

Fișiere noi: PYTHON/routes/vercon/efactura_receipts.py; PYTHON/static/js/vercon/efactura-view.js. Integrare: PYTHON/utils/vercon_efactura.py; PYTHON/routes/vercon/{receipt_tree,invoices,documents,doc_routes,receipt_distribution}.py; PYTHON/static/js/vercon/{docs,document-tree-order}.js; PYTHON/static/js/components/treeview/treeview-render.js; PYTHON/static/css/{vercon-efactura,vercon-document-tree}.css; PYTHON/static/help/vercon.json; implementare_vercon_v2.md; acest jurnal.

Fără DDL nou sau execuție SQL. Publicare: Python/JS/CSS/help și rendererul TreeView împreună, repornirea serviciului și reîncărcarea paginii.

<a id="slice-vc06-17"></a>

## SLICE-VC06-17 — eFactura: ComboBox custom și încărcare automată — 23.10.2026

Selectorul nativ select a fost înlocuit cu Combobox-ul comun, configurat dropDownStyle=simple, cu etichetă accesibilă și dropdown în dialog. Poate afișa toate cele 500 de opțiuni oferite de API. Cheile de sortare sunt explicit furnizorul, data și numărul facturii ca text, independent de ID.

Eliminat butonul «Deschide factura». După încărcarea listei se selectează și se citește automat prima factură potrivită. API-ul listei acceptă filtrul opțional partner, rezolvat la CUI-ul normalizat al partenerului NIR; se păstrează filtrul de dată. Numărul nu este criteriu de asociere. Facturile preluate integral prin fluxul vechi, care nu pot fi convertite în alocări, nu sunt oferite pentru acest dialog.

Selectarea altei opțiuni în ComboBox încarcă automat pozițiile. Se confirmă celula în editare și se protejează ciorna nesalvată; dacă utilizatorul renunță la schimbare sau editul este invalid, ComboBox revine la factura curentă. În timpul scrierii ori al unui răspuns neconfirmat selectorul rămâne blocat. O citire eșuată se poate relua prin reselectarea facturii. Controlul și dropdownul sunt distruse la închiderea dialogului.

Butoanele sunt acum «🔗 Asociaza», apoi «💡 Propune», cu descrierile lungi în title. Bara conține numai ComboBox-ul și aceste două acțiuni.

Fișiere: PYTHON/static/js/vercon/efactura-association.js; PYTHON/routes/vercon/efactura_queue.py; PYTHON/static/css/vercon-efactura.css; PYTHON/static/help/vercon.json; implementare_vercon_v2.md; acest jurnal.

Fără DDL nou sau execuție SQL. Publicare: Python/JS/CSS/help împreună, repornirea serviciului web și reîncărcarea paginii.
