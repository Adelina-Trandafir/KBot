# SLICE-VC06-11 — Asociere NIR manual cu eFactura — 23.10.2026

Stare: TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

## Utilizare

Într-un NIR de cumpărare salvat, neanulat, opțiunea «⋮ → Asociază eFactura…»
din meniul acelui NIR în arbore deschide facturile active trimise de contabil
cu data facturii din NIR. Opțiunea a fost mutată din footer în popup.
Serverul cere și același CUI. Numărul facturii nu este criteriu de asociere,
nici la selecție, nici la salvare sau la calculul valabilității alocărilor;
seria și spațiile pot diferi. NIR-ul trebuie deschis în contextul
gestiunii și exercițiului său, cu drept de recepție, ocupare și perioadă
deschisă. Pentru alte NIR-uri se schimbă contextul în mod normal.

Două liste alfabetice românești prezintă produse NIR și poziții eFactura,
CPV, UM, cantitate și rest. Se trage un rând NIR peste poziția eFactura;
alternativ se selectează cele două rânduri și se apasă butonul de asociere.
Perechile se editează sau se elimină înainte de salvare. Cantitățile sunt
independente pe cele două documente, pentru conversii explicite de UM.
Calculele din UI folosesc BigInt la patru zecimale, serverul Decimal.

O poziție poate fi împărțită între rânduri și NIR-uri, inclusiv în subunități
diferite. Asocierea înlocuiește numai perechile NIR-ului curent pentru
trimiterea aleasă. Celelalte NIR-uri, afișate cu subunitate/gestiune/număr/data,
își păstrează alocările. În etapa inițială nu se modificau NIR-uri, facturi interne VC_FACTURI,
produse, prețuri sau mișcări de stoc. Excepția ulterioară pentru CPV este
consemnată în corecțiile din 23.10 de mai jos.

## Propuneri și memorare

Se reutilizează întâi o denumire memorată univoc pentru CUI/gestiune,
apoi CPV-ul exact normalizat la opt cifre. O contradicție între două CPV-uri
nu este rezolvată automat prin memorie. La CPV-uri sau produse repetate se
compară și cantitățile disponibile. O propunere se face numai dacă perechea
este univocă în ambele sensuri și UM coincide. Nu se folosesc potriviri
aproximative de nume; ambiguitățile și UM diferite se rezolvă manual.

Toate propunerile sunt ciorne până la salvare. Bifa de memorare a fost
eliminată ulterior pentru compactarea dialogului. Memoria se completează
implicit numai pentru perechile realizate prin drag and drop sau editate
manual, la salvarea lor. Propunerile și asocierile din butoane nu produc
singure memorie. Corespondența păstrează: CUI,
denumirea normalizată eFactura, produsul identificat în gestiune, ambele
denumiri, utilizatorul și data. Identitatea produsului rămâne stabilă la
redenumire. Mai multe produse memorate pentru același nume nu sunt alese
arbitrar. Eliminarea unei alocări nu șterge istoricul denumirilor memorate.

## Tranzacții și compatibilitate

Scrierea folosește ocuparea contextului, blocarea gestiunii, revizia NIR și
blocarea facturii/trimiterii, comună cu retragerea contabilului. Recalculează
bugetele tuturor NIR-urilor în aceeași tranzacție și refuză depășirile pe
ambele documente. Cheia de cerere permite reluarea aceleiași salvări după un
răspuns pierdut. Auditul păstrează perechile, cantitățile și memorarea.

Starea efectivă este TRIMISA / PARTIALA / PRELUATA, ultima numai când toate
cantitățile facturii sunt acoperite. În baza veche se stochează în continuare
TRIMISA/PRELUATA; PARTIALA se calculează identic pentru VERCON și K-BOT.
Factura parțială nu poate fi retrasă/returnată și conturile ei propuse nu
se pot schimba până la eliminarea asocierilor active.

Asocierea reține revizia NIR. Orice editare ulterioară a NIR-ului invalidează
asocierile acelei revizii, care trebuie reconfirmate în dialog; anularea sau
dispariția documentului eliberează și ele cantitățile. Revizia monotonă
împiedică reactivarea unor alocări vechi dacă se revine ulterior la aceleași
cantități. NIR-urile deja legate integral prin vechiul flux nu se convertesc
automat. Pentru o trimitere trecută pe alocări, restul se recepționează prin
NIR manual urmat de asociere, nu prin fluxul vechi de preluare integrală.

## Instalare și fișiere

Utilizatorul execută manual `sql/EV_02_vercon_asocieri_partiale.sql` pe
AVACONT_SURSA și sincronizează structura în DC-uri, după EV_01. Sunt trei
tabele locale noi, cu identificatori uppercase: EF_VERCON_ASOCIERI,
EF_VERCON_ALOCARI, EF_VERCON_CORELARI. Nu este SQL pentru AVACONT_COMUN.
Aplicația nu creează schema; opțiunea răspunde EF_ASSOC_SCHEMA_REQUIRED
dacă aceasta lipsește. Fluxul vechi continuă să funcționeze fără schema nouă.

Fișiere noi: scriptul SQL; PYTHON/utils/vercon_efactura.py;
PYTHON/routes/vercon/efactura_association.py;
PYTHON/static/js/vercon/efactura-association.js și efactura-match.js.

Integrare: PYTHON/routes/vercon/{doc_routes,efactura_queue}.py;
PYTHON/routes/efactura/primite_vercon.py;
PYTHON/static/js/vercon/{docs,efactura-intake}.js;
PYTHON/static/css/vercon-efactura.css; PYTHON/static/help/vercon.json;
src/KBot.Domain/EFacturaVercon.vb;
src/KBot.EFactura/Forms/VerconForm.vb; implementare_vercon_v2.md.

Publicare necesară: modulele Python/JS/CSS/help împreună, repornirea
serviciului web, reîncărcarea paginii și recompilarea K-BOT pentru textul
stării parțiale. Nu s-a făcut publicare sau execuție SQL în această lucrare.

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
- Dialogul transmite numai ID NIR curent și contextul verificat. Serverul recitește NIR din subunitate/gestiune/exercițiu și impune CUI + Data facturii; nu se bazează pe partner opțional din browser. Include_completed fără NIR valid este respins și pentru JS vechi; CUI nedeterminabil oprește explicit lista. Active/Inactive/Toate și reîncărcarea păstrează filtrul. Lista generală de preluare și informarea globală rămân separate. Schimbarea privește asocierea, fără a declara implementată migrarea generală 0122-05 a tuturor cititorilor/scriitorilor VERCON pe CodPartener.

Fișiere istorice (numele fără prefix continuă directorul precedent):

| Etapă generală | Fișiere |
|---|---|
| 20 | PYTHON/static/js/vercon/efactura-association.js; PYTHON/static/css/vercon-efactura.css. |
| 22 | PYTHON/routes/vercon/efactura_product_cpv.py (nou); PYTHON/routes/vercon/efactura_association.py; PYTHON/static/help/vercon.json; implementare_vercon_v2.md. |
| 23 | PYTHON/routes/vercon/efactura_association.py; PYTHON/static/js/vercon/efactura-match.js; PYTHON/static/help/vercon.json; implementare_vercon_v2.md. |
| 24 | PYTHON/utils/vercon_efactura.py; PYTHON/routes/vercon/{efactura_association,efactura_queue}.py; PYTHON/static/js/vercon/efactura-association.js; PYTHON/static/help/vercon.json; implementare_vercon_v2.md. |
