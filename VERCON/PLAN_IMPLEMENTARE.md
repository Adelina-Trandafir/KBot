# VERCON — plan de portare Access în aplicația KBOT

Actualizat: 11.10.2026. Plan documentar pentru migrarea datelor și portarea fluxurilor VERCON în aplicația web existentă, după structura planului ADECHIT: felii, subfelii, dependențe, livrabile și criterii de acceptare.

## Checkpoint de reluare

- Analiza de referință este în `VERCON/DETECTIE_VBA.md`; a fost citită înaintea completării și nu a fost modificată în această rundă.
- Această intervenție documentează portarea. Nu implementează încă schema, migratorul ori aplicația și nu importă date.
- Surse de referință: `ADECHIT/PLAN_IMPLEMENTARE.md`, regulile/documentația actuală ADECHIT, `VERCON/DETECTIE_VBA.md` și exportul `VERCON_ACCESS`.
- **Completat documentar:** constatările acceptate au fost scrise incremental aici, împreună cu contractele, deciziile deschise, 50 de subfelii VC00–VC09 și 34 de scenarii de verificare.
- **Ultima revizie:** regulile utilizatorului înlocuiesc propunerile anterioare: migrare unică DC+subunitate, INT/zecimale pe subunitate, N01/N02 cu prag >1 leu și TVA individuală, preț nenegativ, acces exclusiv, surse MF/OI multianuale și Materiale/Alimente anuale, tip imuabil, excluderi și schițe VC00-04.
- Starea tuturor implementărilor este **PLANIFICAT** până la existența codului și a rezultatelor consemnate.

## Obiectiv

Păstrarea datelor istorice de gestiune, recepțiilor, consumurilor, retururilor, storno, listelor de alimente, obiectelor de inventar și mijloacelor fixe; mutarea calculelor și operațiilor din formularele Access în servicii și ecrane ale aplicației KBOT. Datele și codul Access rămân referință nemodificată.

## Organizarea propusă

| Felie | Domeniu | Dependență |
|---|---|---|
| VC00 | Mapare, reguli și decizii de portare | Detecția VBA + datele reale |
| VC01 | Integrare KBOT și sisteme comune | VC00 |
| VC02 | Grile, context și interfață de lucru | VC01 |
| VC03 | Schema persistentă și tipurile | VC00 + schema MariaDB locală |
| VC04 | Drepturi, sesiuni și izolare | VC01 + VC03 |
| VC05 | Migrator Access → MariaDB și reconciliere | VC03 + VC04 |
| VC06 | Nomenclatoare, NIR, consum, retur, storno | VC02–VC05 |
| VC07 | Alimente, obiecte de inventar și mijloace fixe | VC06 |
| VC08 | Calcule, rapoarte și închidere an | Motor: VC00/VC03/VC05; acceptare rapoarte/închidere: VC06/VC07 |
| VC09 | Predare, basculare și verificare de către utilizator | VC01–VC08 |

Contractele și fixture-urile pot fi pregătite înaintea dependențelor, dar acceptarea unei funcții cere îndeplinirea dependențelor. Sunt planificate 50 de subfelii, câte cinci în fiecare felie VC00–VC09, conform acoperirii necesare VERCON.

## Decizii confirmate de utilizator

1. Portarea se face în aplicația web KBOT existentă, cu un plan detaliat după modelul ADECHIT; livrabilul curent este documentația, nu implementarea.
2. Ierarhia este **DC (bază MariaDB) → subunități → gestiuni**. O subunitate poate avea mai multe gestiuni, inclusiv mai multe de același tip.
3. Tipurile includ Mijloace fixe, Obiecte de inventar, Alimente și Materiale. Specificul gestiunii determină machetele, coloanele și rapoartele; unele rapoarte sunt comune.
4. OI deschide machetele `OI_*` și nu BC. MF adaugă coloane în grila FC și utilizează tot machetele `OI_*`. Pentru gestiunile care nu sunt MF și nici OI este disponibil BC. Alimente deschide și `ListaAlimente`; BC se creează separat și se asociază listei prin popup cu arbore de bonuri eligibile, fără generare la salvarea listei (corecții utilizator, 12.10.2026).
5. Utilitarul separat se va numi **VRC.Migrator**, după modelul `ADE.Migrator`.
6. Toate tabelele noi proprii VERCON în MariaDB au prefixul **`VC_`**. Denumirea executabilului `VRC` și prefixul tabelelor `VC_` nu se uniformizează artificial.
7. Se reutilizează tabelele și datele existente din **`AVACONT_COMUN`**, inclusiv numele unității și adresa.
8. **Schema funcțională a backendului este identică pentru toate tipurile de gestiune**, conform clarificării explicite a utilizatorului. `TZ_SCHEMA` este referința comună pentru schema și FK, deși exportul a fost produs din baza Alimente. Diferențele observate în fișierele istorice sunt abateri de versiune față de schema comună, nu justifică trei scheme țintă diferite.
9. Tot ce este descoperit și acceptat pentru plan se scrie în Markdown pe parcurs. Nu se amână documentarea până la sfârșitul taskului.
10. **N01/N02 — totalul documentului:** când antetul este 0 (ValoareFC neintrodusă) sau diferența absolută dintre antet și suma tuturor liniilor este **mai mare de 1 leu**, valoarea folosită/păstrată în țintă este suma liniilor. Suma include TVA pe fiecare linie conform datelor sale; nu se adaugă o cotă TVA globală. Originalul rămâne în proveniența importului. Regula se aplică atât NIR/FC, cât și BC.
11. **N03/N08 — stocare și precizie:** toate coloanele de valori MariaDB vor fi `INT`. Numărul de zecimale este ales de utilizator și este configurat **pe subunitate**, nu pe DC și nu fixat automat la 2. Reprezentarea/scalarea întregilor trebuie documentată înainte de DDL/import, fără trunchierea fracțiunilor.
12. **N06 — legacy:** tot ce folosește `Coduri` și vechiul sistem asociat este ignorat complet; nu se analizează pentru portare, nu se migrează și nu se implementează. Sistemul nu mai este folosit.
13. **N07 — defect OI:** inversarea cantitate/valoare din OI_Balanta se repară în motorul web, cu probă dimensională explicită; nu se modifică sursele Access.
14. **N13 — e-Factura exclusă momentan:** nu se transferă, nu se dezvoltă și nu se investighează nimic e-Factura în această portare. Integrarea viitoare va fi cu **Kbot.Efactura**, într-un pas separat solicitat ulterior.
15. **Modificarea schemei Access este în afara scopului:** DBS, clsDatabase și clasele/mecanismele lui de creare/reparare/actualizare a schemei nu se portează, nu se repară și nu se execută. Citirea read-only a schemei/FK pentru mapare rămâne necesară.
16. Feliile sunt **VC00–VC09**, iar subfeliile au forma **SLICE-VC00-04**. Cerința de schițe menționată ca SLICE-VR00-04 este atribuită SLICE-VC00-04, conform prefixului VC cerut. Executabilul își păstrează numele VRC.Migrator.
17. **Schițe machete:** SLICE-VC00-04 include schițe ale ecranelor și rapoartelor, bazate pe exporturile FORMS/REPORTS; se livrează PNG-uri mici și indexul lor, cu legături la obiectele/câmpurile sursă. Sunt livrabile ale analizei viitoare, nu imagini deja create în această actualizare.
18. **Specific imuabil după populare:** gestiunea cu date nu își schimbă tipul. Alegerea/schimbarea specificului este permisă numai în contextul unei baze/evidențe noi, fără date; nu prin golire sau reinterpretare automată a unei gestiuni existente.
19. **Organizarea surselor pe ani:** MF și OI au fiecare o singură bază cu întreg istoricul, toți anii de la început la sfârșit. Materiale și Alimente au baze anuale `tz_aaaa.accdb` în folderele fiecărei subunități. Aceasta este regulă confirmată, nu decizie încă deschisă.
20. **Prețul nu poate fi niciodată negativ**, pentru toate tipurile și toate operațiile VERCON. Semnul consumului/storno/transferului/casării aparține mișcării/cantității/efectului contabil, nu prețului unitar.
21. **Acces exclusiv:** la aceeași combinație bază/DC + subunitate + gestiune poate lucra cel mult un utilizator simultan, indiferent de anul ales. Al doilea utilizator este blocat la acces; același mecanism protejează închiderea de an (T24). Tranzacțiile și idempotency rămân necesare pentru întreruperi și cereri repetate.
22. **Migrare unică pe DC + Subunitate:** un singur lot include toate gestiunile și toate bazele necesare subunității (MF/OI cu întreg istoricul, Materiale/Alimente cu toate bazele anuale selectate). După migrarea confirmată nu există import suplimentar, repetare, actualizare din Access sau sincronizare. Vechea aplicație VBA încetează să fie folosită. Nu se codează compatibilitate operațională cu ea; citirea/conversia surselor pentru transferul unic nu este un mecanism de compatibilitate permanentă.

## Surse și limite ale dovezilor

| Sursă | Ce dovedește / cum se folosește |
|---|---|
| [DETECTIE_VBA](DETECTIE_VBA.md) | Citit înaintea extinderii planului; constatări statice și verificări DAO read-only din 11.10.2026. Analiza încheiată nu înseamnă defecte reparate. |
| [Plan ADECHIT](../ADECHIT/PLAN_IMPLEMENTARE.md) | Model pentru felii, livrabile, dependențe, acceptare și separarea validării locale de server. |
| [Plan subunități ADE](../ADECHIT/plan_subunitati.md), [migrator ADE](../ADECHIT/ADE.Migrator/README.md) | Context, import separat pe subunitate, remaparea ID-urilor, jurnal și rezultat COMMIT necunoscut. Se citește și codul actual; unele paragrafe istorice descriu starea de dinaintea implementării subunităților. |
| [Inventar frontend](VERCON_ACCESS/INDEX.md) | 377 obiecte: 109 tabele, 80 queries, 53 formulare, 61 rapoarte, 44 module, 27 clase, 3 macrocomenzi. 40 tabele legate: 5 spre cale și 35 spre backendul anual. |
| [Inventar backend comun](VERCON_ACCESS/Databases/TZ_SCHEMA/INDEX.md), [relații backend](VERCON_ACCESS/Databases/TZ_SCHEMA/relationships.md) | Export `tz_Alimente.accdb`, 11.10.2026, 16:00; 41 descrieri de tabele, 29 relații (26 business + 3 metadate). Referință comună pentru toate gestiunile, conform utilizatorului. Indexul mai declară un query și un modul; fișierele acestora nu apăreau în inventarul pe disc la citirea curentă și trebuie verificate dacă devin necesare. |
| `VERCON_ACCESS/Databases/*.accdb` | Date reale din perioade/configurații diferite; numele fișierului nu stabilește singur subunitatea, gestiunea sau anul de evidență. Se citește fără pornire Access/VBA. |
| [Schema comună exportată](../MariaDB_Schema/AVACONT_COMUN.sql), [schema DC exemplu](../MariaDB_Schema/000_DEMO.sql) | Dovezi locale despre structuri existente, nu confirmare a stării actuale pe toate serverele. Dumpurile pot conține date personale; nu se reproduc în plan și nu se folosesc ca fixture public. |
| [Schema sync](../PYTHON/routes/schema_sync/README.md) | AVACONT_SURSA este șablonul; AVACONT_COMUN nu este țintă de sincronizare. Extensiile comune cer script separat. |

**Constatare locală:** `PYTHON/routes/vercon.py` există, dar are 0 octeți. `PYTHON/main.py` înregistrează ADECHIT; nu a fost găsită acolo înregistrare VERCON. Nu există dovadă că aplicația VERCON ori migratorul VRC sunt deja implementate. Exportul DC examinat precede unele extensii ADE; nu se copiază starea lui ca și cum ar fi schema live curentă.

## Contextul obligatoriu de lucru

```text
DC validat prin portal = baza MariaDB a unității părinte
  Subunitate A
    Gestiune A1: ALIMENTE → surse tz_aaaa → exerciții web
    Gestiune A2: MF       → o sursă, întreg istoricul → filtru an/interval
    Gestiune A3: OI       → o sursă, întreg istoricul → filtru an/interval
    Gestiune A4: MATERIALE→ surse tz_aaaa → exerciții web
    Gestiune A5: MATERIALE→ surse tz_aaaa → exerciții web
  Subunitate B
    Gestiune B1: ALIMENTE → surse tz_aaaa → exerciții web
```

- `DC` este baza aleasă și autorizată de infrastructura existentă. `CAI.DbName` desemnează baza; `CAI.DC` are și semnificație legacy și nu se confundă automat cu `Unitati.DC`.
- `SubunitId` și `GestiuneId` sunt identități permanente; denumirea, calea Access, tipul sau hashul fișierului nu sunt chei de identitate.
- `TipGestiune` este un atribut/cod funcțional, nu o bază distinctă și nu valoarea `Masini.IdMasina`. `Masini` reprezintă categorii/conturi în interiorul gestiunii, nu catalogul gestiunilor web.
- **MF/OI:** fiecare gestiune are o bază Access unică pentru întreg istoricul; toți anii intră în același transfer și în aceeași evidență web. Selecția anului/intervalului este un filtru de lucru/raportare, nu o bază sursă distinctă și nu o copie anuală a activelor.
- **Materiale/Alimente:** se inventariază toate `tz_aaaa.accdb` din folderele subunității și se includ în lotul ei unic. `AnEvidenta` este anul bazei/exercițiului, distinct de anul datei fiecărui document; o deschidere la 31.12 din anul precedent nu mută rândul în altă sursă. Nu se deduce exercițiul prin MAX(Data).
- Orice citire, scriere, raport, export, draft, job, import și cache precizează contextul relevant. DC provine din sesiune; ceilalți identificatori sunt validați pe server în DC și între ei.
- Lipsa contextului cere alegerea lui. Autoselecția este permisă numai când există exact o opțiune eligibilă la nivelul respectiv și răspunsul API o declară explicit.
- Se păstrează același model de tabele `VC_` pentru toate gestiunile. Configurația și tipul controlează funcțiile disponibile; nu se creează `VC_MF_NIR`, `VC_OI_NIR` ori tabele per an/per fișier.
- Precizia este configurată pe `SubunitId` și este comună gestiunilor acelei subunități. Anul selectat nu schimbă scara valorilor deja stocate. D02 privește numai maparea/reportarea soldurilor anuale și evitarea dublării lor în transferul unic; organizarea surselor este deja confirmată.
- Tipul se alege numai pentru baza/evidența nouă fără date. După populare, API-ul și DB interzic schimbarea specificului; nu există comandă de conversie OI↔MF↔Alimente↔Materiale pentru o gestiune existentă.
- Accesul se obține exclusiv pentru `(DC, SubunitId, GestiuneId)`, fără an în cheia de exclusivitate. Alegerea altui an nu permite unui al doilea utilizator să intre în aceeași gestiune.

## Matricea funcțională a gestiunilor

| Funcție | MATERIALE / alt tip fără MF și OI | OI | MF | ALIMENTE |
|---|---|---|---|---|
| Produse, variante, categorii, furnizori, recepție FC/NIR | Da | Da | Da | Da |
| FC cu date MF: nr. inventar, data intrării, durate, amortizare | Nu | Nu; cod inventar numai după maparea sursei | Da | Nu |
| Consum BC | Da | Nu ca machetă de lucru | Nu ca machetă de lucru | Da în circuitul consumului; acces manual de verificat |
| Predare / transfer / casare `OI_*` | Nu implicit | Da | Da, cu reguli/rapoarte MF | Nu implicit |
| `ListaAlimente`, mese, rețetar și legătura la BC | Nu | Nu | Nu | Da |
| Retur BR și storno FCS | După maparea fluxului | Disponibilitatea și efectul pe registru de verificat | Disponibilitatea și efectul pe activ/amortizare de verificat | După maparea fluxului |
| Rapoarte comune de intrări/stoc/fișă/inventar | Numai variante compatibile | Numai variante compatibile | Numai variante compatibile | Numai variante compatibile |
| Rapoarte OI / MF / liste alimente | Nu | OI | MF și machete comune OI | Alimente |

**Regulă API:** permisiune de utilizator ∩ apartenență DC/subunitate/gestiune ∩ capabilitate a tipului ∩ stare a anului/documentului. Ascunderea unui buton nu ține loc de verificare pe server. Tip necunoscut sau MF și OBINV simultan active în sursa istorică se semnalează și se rezolvă în planul de migrare; nu se alege după ordinea unui `If`.

**Surse confirmate:** `FC.Form_Load` citește `MF` și `OBINV`, folosește `TmpFactMF` pentru MF; `SFP` clasifică meniul prin G/O/M și `tmpOptiuniSFP.TIP`. Datele metadatelor meniului trebuie recuperate, nu doar schema tabelului. Ribbonul exportat conține callbackuri, dar XML-ul/condițiile lui nu sunt recuperate integral. Cerința utilizatorului stabilește comportamentul web de mai sus.

## Relații backend — bază pentru FK și ordinea importului

Tabelul următor transcrie toate cele **26 relații business** din `TZ_SCHEMA/relationships.md`. `U+D` = exportul declară cascade update și cascade delete; `—` = nu declară aceste etichete, fără concluzie despre integritatea impusă. Nu se inventează FK din simpla existență a unui index.

| Părinte.câmp | Copil.câmp | Atribute exportate |
|---|---|---|
| Clienti.IDC | Iesiri_H.IDC | U+D |
| Clienti.IDC | OI_Casare.IDC_BC | U+D |
| Clienti.IDC | OI_Predare.IDC_BP | U+D |
| Clienti.IDC | OI_Produse.IDC_PTC | U+D |
| Clienti.IDC | OI_Transfer.IDC_BT | U+D |
| ComisieNIR.IdComisie | NIR.IdComisie | U+D |
| Furnizori.IDF | NIR.IDF | U+D |
| Iesiri_H.IdBonConsum | IesiriRetur.IdBonConsum | U+D |
| Iesiri_H.IdBonConsum | LegBCLA.IdBonConsum | — |
| Iesiri_H.IdBonConsum | Iesiri.IdBonConsum | U+D |
| LABC.IdLista | LegBCLA.IdLista | U+D; one-to-one |
| LABC.IdLista | Cina.IdLista | U+D |
| LABC.IdLista | Dejun.IdLista | U+D |
| LABC.IdLista | ListaAlimente.IDLISTA | U+D |
| LABC.IdLista | MicDejun.IdLista | U+D |
| Masini.IdMasina | Produse.IdMasina | U+D |
| NIR.IdIntrare | Intrari.IdIntrare | U+D |
| ProduseP.IdProdus | Iesiri.IdProdus | U+D |
| ProduseP.IdProdus | IesiriRetur.IdProdus | U+D |
| ProduseP.IdProdus | Intrari.IdProdus | U+D |
| ProduseP.IdProdus | OI_Casare.IdProdus | U+D |
| ProduseP.IdProdus | OI_Predare.IdProdus | U+D |
| ProduseP.IdProdus | OI_Produse.IdProdus | U+D |
| ProduseP.IdProdus | OI_Transfer.IdProdus | U+D |
| Produse.ID | ProduseP.ID | U+D |
| TVA.IdTVA | Intrari.IDTVA | U+D |

Celelalte 3 relații sunt `_Struct_.IDS → _IDX_.IDS`, `_Struct_.IDS → _REL_.IDS`, `_TBLS_.IdStruct → _Struct_.IdStruct`, toate U+D; sunt infrastructura Access și nu se copiază ca motor de reparare web.

### Legături logice care cer validare suplimentară

- `IntrariStorno.IdIntrare/IdProdus`, `ProduseM.IdProdus`, `OI_Transfer.IDC_OLD` nu au relații declarate în lista backendului, deși au roluri de referință în cod. Se profilează separat înainte de adăugarea FK țintă.
- `NIR.IdMasina`, `IdStorno`, `IDFV`, `Iesiri_H.IdMasina`, `IdRetur`, `OI_Predare.IdBonConsum`, `IdMasinaP` și `OI_Produse.IDBP/IDBT/IDBC/IDBO/IDM` nu primesc automat părinți după nume. Se urmăresc salvarea, ștergerea și interogările.
- În `OI_Predare`, PK este `ID`; `IDBP` nu este unic. Similar, `OI_Transfer.IDBT` și `OI_Casare.IDBC` grupează linii/documente, iar PK este `ID`. Un FK simplu către aceste câmpuri ar presupune o unicitate inexistentă. Dacă web normalizează antete OI, creează mapări separate pentru ID de linie și ID de grup.
- În exportul Access, `LABC.IdLista` este PK; `LegBCLA.IdLista` este PK și FK unic către LABC, iar `IdBonConsum` nu are unicitate declarată. Acestea rămân constatări despre sursă. **Ținta web, decisă 12.10.2026 revizia 2: o listă poate avea mai multe BC-uri.** Legăturile țintă au RowId, IdLista neunic, pereche listă–BC unică pe context și verificarea că un BC nu aparține altei liste. DDL/generatorul scris anterior trebuie aliniat la această decizie; importul nu pierde și nu repară tacit asocieri istorice incompatibile.
- `Retetar.IdFel` este PK în schema comună. Nu se inventează un tabel de ingrediente cu mai multe rânduri pentru aceeași cheie fără verificarea semnificației `Continut` și a fluxului din formular.
- `IntrariStorno.PUTVA` **și `IesiriRetur.PUTVA`** sunt `Long` în schema backendului, spre deosebire de variantele/prețurile `Double`. Al doilea caz rezultă din noua citire a `TZ_SCHEMA`; datele de retur furnizate sunt goale și nu probează efectul istoric.
- Relația `Iesiri_H → LegBCLA` cere citirea atributelor DAO brute și o probă pe copie. `DETECTIE_VBA` a observat atributul 8192 și nu îi atribuie o semnificație neverificată. Lipsa etichetelor în Markdown nu tranșează cazul.

### Politica FK propusă pentru MariaDB

Schema țintă adaugă identitatea contextului la cheile candidate/FK, astfel încât o linie nu poate referi un antet sau un produs din altă gestiune. De exemplu `(SubunitId, GestiuneId, ParentId)`; pentru entitățile anuale se include și cheia exercițiului unde relația o cere. FK către catalogul gestiunilor verifică simultan subunitatea. Nu se impune anul documentului pe identitatea unui activ permanent.

Cascadele Access sunt documentate pentru paritate, dar **nu se copiază mecanic** peste documentele istorice: ștergerea unui furnizor, client/deținător, produs sau comisii poate șterge în Access documente și linii. Propunere țintă: `RESTRICT`/inactivare pentru nomenclatoare folosite; antet+linii și toate efectele se anulează/șterg numai prin serviciul tranzacțional autorizat. Decizia D05 fixează politica finală pe fiecare relație. `SET NULL` nu trebuie să golească apartenența obligatorie la gestiune.

## Reutilizarea AVACONT_COMUN

| Informație / mecanism | Sursa existentă verificată local | Regula VERCON |
|---|---|---|
| Identitatea DC, nume și CF | `Unitati(DC,NumeUnitate,CF)` | Sursă pentru unitatea părinte, prin portal; fără copie editabilă `VC_Unitati` a aceleiași identități. |
| Adresă și contact, date emitent | `Unitati_Date(DC,Denumire,CodFiscal,Adresa,Orasul,Judetul,Mail,Telefon,...)` | Se reutilizează; numele fiscal/emitent poate diferi de numele portalului. Se documentează câmpul folosit pe fiecare document; nu se suprascriu unul pe altul automat. |
| Conturi și bancă | `Unitati_Conturi(DC,Cont,Banca,IdCont,...)`; `BIC` | Pentru unitatea părinte; nu sunt conturile furnizorilor sau deținătorilor. Nu se mută în VC și nu se schimbă seria facturii/chitanțelor. |
| Utilizatori și asociere DC | `Unitati_Utilizatori`; sesiuni portal | Loginul existent; fără parole/conturi VERCON paralele. |
| Secțiuni și operații | `Sectiuni`, `Roluri`, `Roluri_Operatii`, `Utilizatori_Roluri`; `routes/portal/drepturi.py` | Codul `VR` există în export și SQL. Operațiile se citesc din `Roluri_Operatii`; se adaugă roluri necesare fără a presupune că sunt deja acordate. |
| Baze și contexte comune an/sursă | `CAI.DbName`, `Unitati_Ani(DC,AN,SS,CodProgram)` | Nu se echivalează SS cu gestiunea. Anii VERCON pot fi istorici și cer catalog propriu de exerciții; integrarea cu catalogul comun se decide în D02. |
| Identitatea subunității / sediului | Nu s-a confirmat un catalog comun de subunități cu semantica cerută | `VC_Subunitati` este propunere locală; înainte de DDL se verifică disponibilitatea unui catalog comun reutilizabil. `AD_Subunits` nu se folosește arbitrar drept catalog comun tuturor modulelor. |
| Setări, gestionar, comisii, specific local | `UNIT`, `cale`, `Setari`, comisii Access | Se clasifică pe DC/subunitate/gestiune/an. Gestionarul și semnatarii locali nu înlocuiesc reprezentanții unității părinte. |

**Regulă de migrare:** comparație `cale/UNIT` ↔ datele comune înainte de scriere. Identitate fiscală contradictorie blochează asocierea până la rezolvare. Nume/adrese vechi se păstrează în proveniență/snapshot istoric când sunt necesare, nu prin rescrierea configurației curente a altor module. Lipsa datelor comune produce diagnostic și propunere de completare separată. `VRC.Migrator` nu execută automat UPDATE/INSERT în `AVACONT_COMUN`.

Un antet istoric poate păstra datele emitentului/semnatarilor de la emitere; aceasta este o copie documentară cu proveniență, nu al doilea catalog al unităților. Șablonul folosește datele curente numai pentru documentele noi, după regula explicită a raportului. Drepturile de modificare a datelor comune rămân în mecanismul lor existent.

## Necongruențe și defecte: păstrare, diagnostic, rezolvare

**Transfer unic cu transformări aprobate:** toate rândurile business incluse sunt păstrate, iar valorile țintă aplică regulile utilizatorului, inclusiv înlocuirea totalului de antet prin suma liniilor în N01/N02. Valoarea originală și regula aplicată rămân în proveniență. Motorul web corectează N07; nu reproduce defectele pentru compatibilitate cu VBA. Codul Coduri, e-Factura și mecanismele de modificare a schemei Access sunt excluse explicit.

| ID | Constatare din DETECTIE / schema comună | Tratament obligatoriu în portare |
|---|---|---|
| N01 | MF: NIR local IdIntrare=1, antet 0; exemplul analizat are linii 1.262.155,34; alt NIR are antet NULL | Antet 0 sau diferență absolută >1 leu: valoarea țintă este suma tuturor liniilor, calculată corect cu TVA pe fiecare linie. Antetul original rămâne în proveniență. Antet NULL se marchează lipsă, fără conversie oarbă la 0; completarea prin suma validată este tratată explicit în contract. |
| N02 | MF: bon local IdBonConsum=7, antet 843.762,24; exemplul analizat are linii 843.275,53, diferență 486,71 | Aceeași regulă ca N01: dacă suma TVA-inclus corectă confirmă diferența >1 leu, ținta păstrează suma liniilor. Identificatorii sunt locali fișierului MF. |
| N03 | Prețurile storno/retur Access sunt Long; prețul sursă poate diferi de varianta curentă | Toate coloanele de valori MariaDB sunt INT cu reprezentare numerică explicită; zecimale configurabile pe subunitate, fără presupunerea 2. Nu se inventează fracțiuni istorice absente din sursă și nu se introduce DECIMAL în DDL pentru valori. |
| N04 | OI: 40 intrări cu Cant=0; verificările nu au găsit Cant<0 în aceste intrări | Păstrează liniile; clasifică proveniența/rolul. Cantitate 0 istorică nu este automat corupție. |
| N05 | MF are și ValoareLunara; OI lipsește DNF/COD/Comentarii și unele tabele în copiile analizate | Citire/conversie punctuală pentru transferul unic către schema comună; fără repararea sursei, fără framework de compatibilitate VBA și fără execuția claselor DBS/clsDatabase. Câmp absent diferă de NULL prezent. |
| N06 | Coduri și vechiul sistem asociat | EXCLUS COMPLET prin decizia utilizatorului: fără hartă apeluri pentru portare, fără migrare, implementare sau probe de compatibilitate. Setarea FIFO nu reactivează sistemul exclus. |
| N07 | OI_Balanta inversează C2/V2; capetele intervalului diferă la casare | Repară inversarea în web; caz sintetic 2×100, prima/ultima zi. Prețul este 100 pozitiv, efectul casării este −2/−200; preț negativ este interzis în tot VERCON. |
| N08 | QBC nu filtrează temporal storno ca celelalte; validarea BC folosește o precizie fixă greșită | Calcule la dată explicită și aceeași precizie configurată de utilizator pe subunitate în UI/calcul/validare. Nu există precizie globală DC și nici limitare implicită la 2. |
| N09 | AdaugaStoc_Furnizor resetează numai când se schimbă simultan furnizorul și produsul | Sold separat pe cheia completă; test pe două produse și doi furnizori. |
| N10 | FC face operații MF după commit, FCS fără tranzacție, ștergerea BC neatomică | Web tranzacționează toate efectele, inclusiv variante, MF, registru OI și LegBCLA. Nu se copiază limitele tranzacționale defecte. |
| N11 | DMax+1 pentru ID; validări prin evenimente UI și verificări RecordCount pe Count | Chei tehnice generate atomic; contor document separat; validarea se execută pe server, agregatul se citește ca valoare. |
| N12 | IA copiază/curăță baza anuală; repararea și procesele pot raporta succes fals | Închidere web pe exercițiu, atomică/idempotentă, cu reconciliere. Nu se execută copierea/ștergerea Access pe originale. |
| N13 | e-Factura și toate datele/procedurile/fișierele sale | EXCLUS din transfer și dezvoltarea actuală, fără investigații/recuperări suplimentare. Integrarea cu Kbot.Efactura aparține unui pas viitor separat; nu blochează lansarea VERCON curentă. |

Controalele de orfani efectuate au rezultat 0 în cele trei baze, dar sunt **parțiale**. Toate trei aveau 0 retururi, casări și transferuri: scenariile acestea necesită fixture-uri sintetice și, pentru paritate reală, documente reprezentative suplimentare. Pragul istoric 0,01 nu este o certificare a preciziei și exclude anumite NULL; se adaugă verificări separate.

### Registrul de reconciliere și cele trei stări de utilizare

Fiecare problemă primește: `IssueId`, `SourceId`, tabel/cheie/câmp sursă, context țintă, cod regulă, severitate, valoare originală, valoare propusă, justificare, responsabil, decizie, dată, efecte și teste. Datele personale/valorile complete rămân într-un artefact local protejat, nu în planul public sau loggerul general.

1. **Blocaj de import:** identitate ambiguă, FK obligatoriu fără părinte, cheie duplicată, schemă necunoscută, conversie cu pierdere neaprobată, sursă schimbată, context țintă greșit. Nu se elimină rândul și nu se inventează un părinte generic.
2. **Date cu avertizare:** necongruență neacoperită de o regulă deja confirmată (de exemplu NULL fără sumă determinabilă). Activarea cere rezolvarea problemelor relevante pentru solduri. N01/N02 rezolvate prin regula >1 leu nu devin blocaje care cer aprobarea aceleiași reguli încă o dată.
3. **Conversie deterministă documentată:** schimbare de chei, boolean -1/0, reprezentarea INT/scală și normalizarea totalului N01/N02. Originalul, valoarea țintă, suma validată, TVA și numărul conversiilor sunt consemnate în raportul transferului unic.

Un import confirmat nu înseamnă automat gestiune activată. Propunere: stări `IN_PREGATIRE`, `IMPORTATA_IN_REVIZIE`, `ACTIVA`, `ARHIVATA`. Controlul de reconciliere și rezolvarea blocajelor funcționale preced trecerea în ACTIVA. Datele în revizie pot fi inspectate și exportate cu avertizare; nu pot produce operații noi pe un sold nesigur.

## Modelul persistent propus

**Toate denumirile de mai jos sunt propuneri până la VC03.** Nu reprezintă DDL deja aplicat. Schema comună Access este baza mapării, iar schema web adaugă context, audit, tranzacții și proveniență. Nu este necesar ca fiecare tabel Access să devină tabel web.

| Grup / tabele țintă propuse | Sursă / responsabilitate | Apartenență |
|---|---|---|
| `VC_Subunitati`, `VC_Gestiuni`, `VC_Exercitii` | Cataloage permanente; tip gestiune, stare, an de evidență și închidere | DC → subunitate → gestiune → exercițiu |
| `VC_Settings`, `VC_Numbering`, `VC_ContextLock` | Setari, contoare (pornesc de la ultimul numar real salvat, nu din NR.*) și acces exclusiv cu titular/sesiune/expirare | Precizie pe subunitate; contor după tip/exercițiu; acces exclusiv pe DC+subunitate+gestiune, fără an |
| `VC_Masini`, `VC_Produse`, `VC_ProduseP` | Categorii/conturi, nomenclator, variante preț/cod inventar | Gestiune; continuitatea anuală explicită în D02 |
| `VC_Furnizori`, `VC_Clienti`, `VC_TVA`, `VC_ComisieNIR`, `VC_ComisieInventar` | Nomenclatoare și deținători; TVA/comisii după verificarea oportunității reutilizării unor cataloage existente | Inițial gestiune; partajarea se decide, nu se deduplicatează după nume |
| `VC_NIR`, `VC_Intrari`, `VC_IntrariStorno` | Antet recepție/storno, linii intrări și linii storno | Gestiune + exercițiu; data documentului rămâne distinctă |
| `VC_Iesiri_H`, `VC_Iesiri`, `VC_IesiriRetur` | Consum/retur antet+linii | Gestiune + exercițiu |
| `VC_OI_Predare`, `VC_OI_Transfer`, `VC_OI_Casare`, `VC_OI_Produse` | Documente și registru OI utilizate și în MF | Gestiune; mișcări/exercițiu și activ/deținător la dată |
| `VC_ProduseM` | Informații MF și durate/valori de amortizare | Activ permanent în gestiune, cu istoric al schimbărilor |
| `VC_LABC`, `VC_ListaAlimente`, `VC_LegBCLA`, `VC_MicDejun`, `VC_Dejun`, `VC_Cina` | Lista, detalii mese, relația cu consumul | Gestiune + exercițiu |
| `VC_FeluriMancare`, `VC_Retetar` | Feluri și rețete; structura reală de verificat | Gestiune; versionare la schimbări care afectează documente salvate |
| `VC_Imports`, `VC_ImportSources`, `VC_IdMap`, `VC_MigrationIssues` | Un singur transfer confirmat per DC+subunitate, toate sursele/gestiunile și mapările lui | Unicitate pe subunitate în baza DC; proveniență pe fiecare sursă și gestiune |
| `VC_Operations`, `VC_Audit`, `VC_AnnualClosings` | Idempotency, schimbări business, închideri/solduri reportate | Gestiune/exercițiu și operație |
| `VC_SubunitAccess`, `VC_GestiuneAccess` | Restricții locale dacă infrastructura comună nu le acoperă | Referă utilizatorul comun; nu stochează parole |
| `VC_ReportCatalog` și snapshoturi/drafturi, numai dacă sunt necesare | Catalog rapoarte/metadate versiuni; date persistente de lucru izolate | Raport/tip sau utilizator+context, după rol |

### Clasificarea tabelelor Access înainte de DDL

- **Date business:** toate antetele/liniile și nomenclatoarele de mai sus; păstrate integral dacă intră în fluxuri sau istoric, inclusiv rânduri cu valoare/număr 0 după profilare.
- **Identitate/configurație:** cale/UNIT/Setari/NR/comisii; mapare câmp cu câmp către comune ori contextul VC, fără copiere globală. `PASS` și credentialele nu se importă.
- **Tabele temporare și rezultate:** TmpFact/TmpFactV/OI_TmpFact/tmpSFP/tmp_Balanta/Caletemp/FM_2/Alimente și variante; se urmărește fiecare utilizare înainte de clasificare. Denumirea singură nu dovedește că datele sunt dispensabile. Drafturile se reconstruiesc în spațiu izolat; calculele nu partajează scratch între utilizatori.
- **Mecanismele schemei Access — EXCLUSE:** DBS, clsDatabase și clasele copil, `_TBLS_`, `_Struct_`, `_IDX_`, `_REL_`, `_ADODAODDL`, SQL/RunOnce de reparare și tabelele @ de structură nu se portează/repară/execută. Readerul citește direct schema/FK read-only necesare mapării; Ver poate fi citit ca diagnostic dacă există, fără crearea lui. Schema MariaDB are registru propriu, independent de motorul Access.
- **Nomenclatoare locale frontend:** cpv/CPV1/Clasificari/gest_subCPV/Jud/UM/TVA și altele se inventariază, se verifică datele efective și disponibilitatea comună. Nu se inventează conținut din descrieri de câmpuri.
- **e-Factura — EXCLUSĂ:** EF/EFS/EFT/EF_TMP și orice obiect/procedură/XML/token/certificat asociat primesc dispoziția exclus acum; nu se citesc datele lor pentru migrare și nu se lucrează la integrare în această rundă. WinSCP/RustDesk/registry/GDI/updater nu sunt funcții web incluse. Șabloanele și fișierele rapoartelor de gestiune se păstrează în scopul lor propriu.
- **Coduri/vechiul sistem — EXCLUS COMPLET:** obiectele care depind de el primesc direct această dispoziție; nu se investighează apelurile lui și nu se codifică compatibilitate. Celelalte obiecte primesc migrare/reutilizare/recalculare/înlocuire/lipsă date ori excludere justificată; inventarul nu redeschide excluderile deja decise.

### Chei, tipuri și integritate

1. MariaDB generează cheile tehnice; se păstrează separat ID-ul sursă și numărul documentului. `Produse.ID` și `ProduseP.IdProdus` sunt două identități diferite. ID-urile random Access pot fi negative: cheia sursă se citește în tip semnat și se păstrează fără conversie la pozitiv.
2. Harta folosește cel puțin `(SourceId, TableName, SourceKey)`; SourceId identifică fișierul/snapshotul și contextul sursă. Aceeași valoare locală 1 din MF/OI/Alimente sau din doi ani nu desemnează aceeași entitate. Cheile compuse/fără PK sunt serializate canonic; nu se inventează o PK din numele coloanei.
3. `VC_IdMap` distinge mapările de linie, document/grup OI și identitate permanentă între ani. Dublarea registrului OI prin import plus regenerare este interzisă.
4. FK verifică contextul și părintele; se adaugă indici pentru gestiune/exercițiu/data documentului/produs/deținător/furnizor și părinții liniilor. Unicitățile de număr document sunt stabilite în D06, nu globale în DC.
5. Baza nu modifică automat istoricul prin `ON DELETE CASCADE` de pe nomenclatoare. Documentele utilizate/închise sunt inactivate/anulate după contract; hard delete are condiții explicite.
6. **Toate coloanele de valori/prețuri în MariaDB sunt INT**, conform deciziei utilizatorului. Reprezentarea fracțiunilor trebuie stabilită explicit prin coeficient întreg+scară documentată, fără CAST/trunchiere și fără introducerea DECIMAL în DDL. Exemplu de reprezentare, nu scară aprobată: 100,25 cu scară 4 se encodează 1.002.500. Limitele INT, overflowul, scalele cantităților și operațiile intermediare sunt obligatoriu profilate în D03; dacă o combinație nu încape, se semnalează înainte de DDL/import, fără schimbare tacită a tipului cerut.
7. **Numărul de zecimale este configurabil de utilizator pe subunitate**, nu pe gestiune/DC și nu implicit 2. Scara de stocare este explicită și nu se confundă cu formatul afișat: schimbarea preferinței nu reinterpretează întregii existenți. D03 fixează codificarea/scara și round, inclusiv istoricul schimbării preciziei. Calculele aplicației pot folosi Decimal/întregi ca aritmetică exactă; aceasta nu schimbă tipul INT din MariaDB. API transportă coeficientul/scara sau o reprezentare textuală exactă și nu pierde precizia prin JS float.
8. NULL, șir vid și 0 rămân distincte. Boolean Access -1/0 se convertește explicit, NULL separat. Coduri de inventar/numere document/CUI/CNP/IBAN rămân text când semantica este textuală; nu se pierd zerourile inițiale.
9. Datele documentelor sunt date locale, fără conversie arbitrară la UTC; timestampurile auditului au convenție explicită. `Data`, `DataFc`, `DataBC`, `DataSalvare`, `DataIntrare` nu se substituie reciproc. Intervalele includ/exclud capete după contract, inclusiv documente cu oră.
10. Lungimile și collation/charset pornesc de la exporturile MariaDB efective, compatibile cu FK comune; nu se schimbă global utf8mb3/utf8mb4 în acest proiect. Se detectează trunchierea/coliziunile accent/case și se raportează, fără eliminare silențioasă de diacritice.
11. **Preț >=0 este invariant global**, inclusiv recepții, variante, retur, storno, OI/MF/Alimente, migrator și rapoarte. Cantitățile/efectele valorice pot avea semn potrivit mișcării, dar prețul unitar nu. Prețurile negative din sursă (decis 12.10.2026): numai în Intrari/Iesiri/IesiriRetur prețul devine pozitiv și cantitatea negativă (aceeași valoare absolută), cu avertisment; cantitatea ȘI prețul negative pe aceeași linie blochează transferul; ProduseP se transferă doar cu folosire în documente. DB/API verifică invarianta, nu numai UI.

**Sursă numerică:** codul actual citește NR.ZEC cu fallback 4; aceasta este dovadă despre sursă, nu default impus aplicației noi. În transfer se compară ZEC din toate gestiunile subunității și se consemnează precizia aleasă pe subunitate, fără „ultimul fișier câștigă”. NR conține și SI_LA/NIR/NRFCT/NRBC/NR/NUM; fiecare se mapează, fără pierderea soldului inițial Alimente.

### Total document — regulă aprobată N01/N02

`S` = suma tuturor liniilor, cu TVA tratată **pe linie** și cu precizia configurată pe subunitate. Se verifică Valoare/TVA/ValoareCUTVA și modul în care sursa salvează prețul: un preț sau total deja TVA-inclus nu primește TVA a doua oară. Linii cu cote diferite, cotă zero și fără TVA sunt cazuri obligatorii. Lipsa câmpurilor necesare calculului nu se înlocuiește cu o cotă globală inventată.

Pentru antetul original `H` cunoscut: **TotalTinta = S dacă H=0 sau abs(H−S)>1 leu; în celelalte cazuri TotalTinta=H.** Pragul este strict: exact 1 leu păstrează H, dacă H nu este 0. Comparația se face în unități monetare reale, nu între coeficienți cu scări diferite și nu după rotunjire automată la 2. Antetul NULL este diagnosticat separat ca lipsă; completarea trebuie consemnată explicit, nu presupusă prin abs(NULL).

Regula este implementată o singură dată în serviciul numeric comun și folosită de migrator/FC/BC, cu H/S/TVA/scară/regulă în proveniența importului și auditul documentelor noi. Exemplele N01/N02 se recalculează cu TVA corect înainte de stabilirea rezultatului țintă; totalurile din DETECTIE sunt repere, nu substitut pentru calculul validat.

## Contractele de calcul și scriere

Serviciile primesc parametri expliciți și date verificate; nu citesc `Forms!...`, `TempVars` sau `Eval`. Contractul fiecăruia precizează semne, date, precizie, rotunjire, anul/documentul curent și sursa valorii.

| Serviciu | Intrări / rezultat cerut | Probe și reguli obligatorii |
|---|---|---|
| Disponibil la dată | Context, variantă, categorie, dată, document exclus, draft separat | Intrări + retururi − ieșiri − storno eligibil la dată − draft. Nu dublează liniile documentului editat; filtrare context pe fiecare ramură UNION, inclusiv retur/storno. |
| Balanță / fișă magazie | Context, interval, produse/furnizor, sold inițial, rulaje, sold final, cantitate și valoare separat | Egalitatea SI+I+R−E−S=SF conform mișcărilor canonice; totaluri pe produs/variantă înainte de total general; două produse/doi furnizori. |
| Validare/salvare FC și BC | Antet original, toate liniile și TVA per linie, precizia subunității, MF după caz | H=0 sau abs(H−S)>1 leu → totalul S; altfel H. Prețuri >=0. Document nevid, MF inventar/data/durate/cantitate; toate efectele în aceeași tranzacție. |
| Consum/retur/storno | Tip, document referit, cantități/prețuri istorice, dată, revizie | Disponibil reverificat sub lock; retur peste consum și storno peste intrarea rămasă tratate explicit. Schimbarea prețului variantei nu rescrie documentele salvate. |
| OI predare/transfer/casare | Variantă, deținător sursă/destinație, document, dată | Transfer minus la vechiul deținător și plus la noul; total net zero, fără dublare. Predarea poate avea efect în Iesiri și OI; registrele nu se însumează mecanic ca două consumuri. |
| MF / amortizare | Activ, DataIntrare, DNF, DF, valoare lunară, dată calcul, istoric | Început/sfârșit lună, februarie/an bisect, durată 0/NULL/epuizată, ultima lună cu rest, casare și transfer. DateDiff și Round Access necesită comparator; nu se presupune echivalență cu Python round. |
| Alimente | Listă, mese, porții/beneficiari, feluri, cantități, prețuri, set de BC-uri existente alese prin popup și afișate în DGV | Antet+detalii+întregul set de legături atomice; fiecare BC revalidat, fără creare/resalvare sau consum repetat; rețeta actualizată nu schimbă retrospectiv lista salvată; valoare per BC, totalul sumei, valoare listă și diferență. |
| Închidere an | Gestiune/exercițiu, solduri pe variantă/deținător/activ, țintă | Snapshot validat, blocare concurență, reportare unică, continuitate MF; repetarea nu dublează soldul. Copiile anuale importate nu se adună peste întregul istoric. |

**Protocol comun de scriere:** sesiune + drepturi + capabilitate + drept de acces exclusiv valid → tranzacție scurtă → verificare titular/jeton exclusiv/context/stare/revizie → RequestKey/payload → calcul INT/scară, N01/N02, TVA per linie și preț >=0 → toate efectele+audit+rezultat → COMMIT → succes. Și citirea/rapoartele/exporturile cer accesul exclusiv; tranzacția SQL nu rămâne deschisă pe durata întregii sesiuni.

Fiecare document are revizie pentru cereri vechi/drafturi și comanda web are RequestKey/context/hash payload. Același utilizator poate produce cereri duplicate sau pierde răspunsul; accesul exclusiv nu elimină necesitatea rezultatului idempotent. Nu se permite lucru concurent al doi utilizatori în aceeași gestiune. Retry-ul de conexiune nu reexecută automat business.

### Acces exclusiv obligatoriu, inclusiv închiderea de an

Se ocupă atomic un singur drept de lucru pentru cheia `(DC,SubunitId,GestiuneId)`. Rândul păstrează titularul (utilizator și sesiune), jeton/revizie, data ocupării și ultima reînnoire. Propunere tehnică: lease cu heartbeat, eliberare explicită și expirare controlată, verificată la fiecare API/job. Al doilea utilizator primește „Gestiunea este ocupată”; nu este admis pentru alt an sau doar pentru un raport. Gestiuni diferite pot avea utilizatori diferiți.

Durata/reînnoirea/recuperarea accesului sunt fixate în VC04: expirarea nu permite ca o cerere veche să continue să scrie după preluare. Revizia/jetonul și tranzacțiile scurte asigură acest lucru; un simplu flag în browser nu este suficient. Pentru același utilizator în mai multe taburi se definește o singură sesiune titulară ori serializarea cererilor, fără operații paralele necontrolate.

Închiderea anului folosește aceeași ocupare, indiferent de an; nu se construiește un al doilea mecanism pentru doi operatori în paralel. Rămân necesare rollback, detectarea rezultatului necunoscut și prevenirea dublării unei comenzi după întrerupere. Migratorul rezervă întreaga subunitate, blocând accesul la toate gestiunile ei până la încheierea transferului unic.

## Contractul VRC.Migrator

Utilitar Windows separat în `VERCON/VRC.Migrator`, cu executabil **`VRC.Migrator.exe`**, destinat **unui singur transfer complet per DC+Subunitate**. Readerul/conversiile există numai pentru extracția surselor acestui transfer; nu formează un strat permanent de compatibilitate Access. Se reutilizează bibliotecile/controalele/loggerul ADE/KBot.Migrator, cu mapări proprii VERCON.

### Fluxul utilizatorului și manifestul

1. **Alege toate sursele subunității**: baza completă MF, baza completă OI și toate tz_aaaa.accdb Materiale/Alimente, după gestiunile existente; opțional cale/frontend pentru datele locale incluse. e-Factura, Coduri și motoarele schemei Access sunt excluse. Nu se începe cu o gestiune urmând ca celelalte să fie adăugate după import.
2. **Citește și profilează**: hashuri/snapshot stabil, tabele/câmpuri/PK/FK business și numărători, MF/OBINV/ZEC, intervale, totaluri cu TVA per linie și prețuri negative. Nu se urmărește sistemul Coduri/FIFO legacy și nu se rulează DBS/clsDatabase.
3. **Asociază explicit** fiecare sursă cu DC destinație, subunitate, gestiune, tip și exercițiu; numele fișierului și setările oferă propuneri, nu aprobări automate. Mai multe backenduri pot intra în gestiuni diferite ale aceleiași subunități. Nu se creează o subunitate pentru fiecare gestiune ori an.
4. **Revizuiește planul complet**: toate gestiunile/anii/sursele, hărți de chei, precizie comună subunității și reprezentare INT, N01/N02, blocaje/avertizări și date comune. Modificarea intrărilor invalidează Testează; operatorul verifică completitudinea înainte de singurul transfer.
5. **Testează**: conexiune/DC/date comune/schema INT/FK/InnoDB/charset/drepturi și eligibilitatea subunității. VC_Imports nu trebuie să conțină transfer confirmat pentru ea. Alte subunități din DC pot avea date; nu se cere golirea întregului DC.
6. **Migrează o singură dată**: rezervă subunitatea și toate gestiunile, reverifică hashuri/plan/țintă/registru, scrie și reconciliază lotul complet, apoi confirmă atomic transferul. Fără DDL, UPDATE în comune, scriere Access ori folosire VBA după migrare.
7. **Reconciliază**: numărători/câmpuri/chei/sume/solduri și probleme păstrate. Exportă un raport local lizibil și un rezultat structurat, legat de ImportId și versiunea contractelor; „rând pregătit” nu înseamnă COMMIT.

Manifestul canonic include `PlanVersion`, `RuleVersion`, contract numeric, surse/hashuri, profilurile schema, contextul țintă pentru fiecare sursă, dispoziția fiecărui tabel, mapările/opțiunile și deciziile. Hashul planului include transformările și contextul, nu doar fișierul. Parole/tokenuri și rânduri personale nu intră în manifestul public.

### Transfer unic pe subunitate și atomicitate

- Snapshotul citit trebuie să fie stabil: preferabil copie de lucru făcută cu Access oprit pentru scrieri; hash înainte/după extracție și înainte de import. Deschiderea read-only nu garantează că alt utilizator nu scrie în fișier. Dacă snapshotul se schimbă, se recitește și se reprofilează.
- **VC_Imports are unicitate pe SubunitId în baza DC.** Un transfer confirmat interzice orice al doilea import în aceeași subunitate, chiar cu alte hashuri/fișiere/ani/gestiuni. Nu există buton de repetare/actualizare/sincronizare/clonare sau ștergere automată a markerului. Hashurile sunt dovadă de proveniență, nu permisiune de import nou.
- Înainte de transfer, ținta business a subunității trebuie să fie nouă/eligibilă, fără import confirmat și fără date conflictuale. Toate gestiunile și sursele sunt reunite în **același pachet**, nu în loturi business succesive pe gestiune.
- MF/OI: se transferă o singură dată întreg istoricul din baza gestiunii, fără fragmentare/copie pe ani. Materiale/Alimente: toate bazele anuale sunt citite împreună și soldurile lor inițiale sunt identificate pentru a nu fi adunate peste istoricul deja reprezentat. Acesta este un calcul de transfer unic, nu suport pentru importul ulterior al arhivelor.
- Propunere de atomicitate: o tranzacție pentru toate gestiunile/sursele subunității, inclusiv mapări/registru/conversii și verificările de import înainte de commit. Nu se publică MF reușit cu Alimente eșuat în aceeași subunitate. Dacă volumul cere staging/chunking, datele rămân invizibile business, iar publicarea tuturor gestiunilor și markerului de migrare este atomică. Strategia finală este VC05-03.
- Înainte de commit eșuat/rollback confirmat nu există migrare reușită; se diagnostichează tentativa. Nu se codează flux de „reimport” al unei subunități migrate. Dacă COMMIT are rezultat necunoscut, se verifică markerul și datele prin instrumentul de diagnostic, fără o nouă inserare automată.
- Ordine inițială: context → categorii/nomenclatoare/furnizori/clienți/comisii/TVA/feluri → produse → variante → antete NIR/Iesiri_H/LABC → linii intrări/ieșiri/retur/storno → OI/MF și mese/listă/LegBCLA → relații circulare logice validate → registre/reconciliere. Relațiile logice lipsă pot schimba ordinea; nu se dezactivează FK pentru a masca erori.
- Dacă OI se normalizează în antete/linii, se construiesc antetele pe cheia de grup verificată și se păstrează ID-ul sursă de linie. Dacă antetele repetate au date contradictorii, lotul se blochează pentru reconciliere.

### Oprire și rezultat necunoscut

Stări UI propuse: `NECITIT`, `PLAN_CITIT`, `BLOCAT`, `TESTAT`, `IN_IMPORT`, `OPRIRE_SOLICITATA`, `ROLLBACK_CONFIRMAT`, `COMMIT_CONFIRMAT`, `REZULTAT_NECUNOSCUT`, `RECONCILIAT`.

Oprește cere anulare înainte de COMMIT; instrucțiunea curentă poate trebui să se termine. După COMMIT trimis nu se promite anulare. Pierderea confirmării declanșează REZULTAT_NECUNOSCUT: se citește VC_Imports și se verifică întregul lot, fără repetarea transferului. O migrare confirmată închide definitiv opțiunea Migrează pentru subunitate. Nu se închid procese MSACCESS; închiderea formularului așteaptă eliberarea resurselor.

Jurnal: context/hashuri, sursă/gestiune/rânduri pregătite, conversii/probleme, durată și COMMIT/rollback confirmat ori necunoscut. SQL parametrizat și eventualul dump local sunt diagnostic, nu comenzi de reimport; rămân protejate și nu se publică. Secretele nu apar în comenzi/excepții. Eșecul loggerului nu falsifică rezultatul tranzacției.

## Integrarea în aplicația existentă

| Sistem | Sursă existentă | Extensia necesară VERCON |
|---|---|---|
| Flask/blueprint | [main.py](../PYTHON/main.py), [ADE blueprint](../PYTHON/routes/adechit/__init__.py) | Pachet propus `PYTHON/routes/vercon/`, pagină `/vercon`, API `/api/vercon`. Se rezolvă fișierul gol `routes/vercon.py` fără două surse cu același nume de import. |
| Portal/context/autorizare | [portal.py](../PYTHON/routes/portal/portal.py), [drepturi.py](../PYTHON/routes/portal/drepturi.py), [session_store.py](../PYTHON/routes/auth/session_store.py) | Secțiunea VR, DC validat, apoi subunitate/gestiune/exercițiu. Fără sesiune paralelă VRC. |
| Structură vizuală/teme | [app.js](../PYTHON/static/js/portal/app.js), [portal.css](../PYTHON/static/css/portal.css), [theme-init.js](../PYTHON/static/js/portal/theme-init.js) | Pagina VERCON și navigarea; context vizibil permanent pe document/raport. |
| Grilă/editare | [datagrid.js](../PYTHON/static/js/dgv/datagrid.js), [engine.js](../PYTHON/static/js/dgv/engine.js), implementarea ADE actuală | Refolosește editorii existenți, adaptând contractul pe coloane. Nu se construiește o grilă VERCON paralelă și nu se copiază validările ADE în motorul comun. |
| Preferințe/selectoare | [layouts.js](../PYTHON/static/js/portal/layouts.js), [grid-layouts.defaults.js](../PYTHON/static/js/portal/grid-layouts.defaults.js), Combobox/TreeView comune | Chei VERCON distincte, inclusiv tipul/contextul unde afectează structura; coloane MF nu se propagă la Materiale. Referința istorică ADE la colprefs.js nu corespunde unui fișier prezent în inventarul curent. |
| Evenimente/cleanup | EventBus, ListenerTracker, instances-registry existente | O singură instanță comună; distruge grile, abonamente, timere, cereri la schimbarea contextului. |
| Log/timing/conexiuni | [logger.py](../PYTHON/utils/logger.py), [timing.py](../PYTHON/utils/timing.py), [database.py](../PYTHON/utils/database.py), [db_retry.py](../PYTHON/utils/db_retry.py) | Context complet și correlation ID; conexiuni prin fabricile existente, comenzi parametrizate, fără retry orb al scrierilor. |
| PDF/listare/export | [pdfview.js](../PYTHON/static/js/portal/pdfview.js), [rapoarte ADE](../PYTHON/routes/adechit/reports.py) | Viewer/print comun; servicii și șabloane VERCON cu filtre/context și versiune. Exportul și PDF au aceleași drepturi și date ca ecranul. |
| e-Factura | Kbot.Efactura, integrare viitoare separată | Exclusă integral din această portare: fără inventariere suplimentară, transfer sau dezvoltare; nu este dependență de lansare. |
| Schema și publicare | Schema sync; [AvacontPush](../AvacontPush/README.md), `PYTHON/.pushignore` | VC în șablon și propagare controlată; AVACONT_COMUN separat. Exporturile Access, baze, jurnale și fixture-uri personale excluse din publicare. |
| Migrator desktop | [proiect ADE](../ADECHIT/ADE.Migrator/ADE.Migrator.vbproj), `src/KBot.Migrator` și `src/KBot.Controls` | Proiect VRC separat, biblioteci/UI comune și hărți VERCON; verifică disponibilitatea/bitness a motorului de citire numai pentru extracția unică. Nu construiește compatibilitate VBA. |

### Contract API și UI

- Un endpoint de context întoarce opțiunile autorizate, tipul imuabil după populare, anul/intervalul, precizia subunității, drepturile și starea ocupării exclusive. Alegerea clientului nu conferă acces; intrarea în gestiune ocupă atomic dreptul exclusiv.
- Rutele de date folosesc numai tabele/câmpuri/rapoarte din allowlist. Nu acceptă SQL, numele arbitrar de tabel/bază sau o cale de fișier furnizată de browser pentru executare pe server.
- Erori structurate: cod stabil, mesaj românesc, detalii de câmp/linie permise, correlation ID și context. Se disting validarea, lipsa dreptului, conflictul de revizie, anul închis, indisponibilitatea și rezultatul necunoscut.
- Drafturile sunt ale utilizatorului și contextului. Schimbarea DC/subunitate/gestiune/an oferă salvare, renunțare sau păstrarea contextului; răspunsurile cererilor vechi nu se aplică noului ecran. Datele din scratch Access nu se mută într-un tabel global comun sesiunilor.
- Gridul gestionează NULL/zero/text gol, parse românesc explicit, calendar comun, câmpuri calculate readonly, rânduri cu chei stabile, stări salvare/eroare/conflict. Eroarea păstrează datele introduse.
- Disponibilul afișat este informativ până la validarea tranzacțională. Mesajul „salvat” apare numai după commit confirmat; documentul persistat și totalurile recalculate sunt întoarse de server.
- Se verifică scroll/virtualizare, tastatură, focus, mobil, selecții multiple, grupare/totaluri, redeschidere document, meniuri dinamice, sesiune expirată și taburi concurente. Preferințele nu pot ascunde avertizări obligatorii.
- Rapoartele lansate dintr-un document salvat folosesc versiunea lui; un raport la dată/interval are parametri declarați și snapshot consistent. Previzualizarea unui draft este marcată ca atare.

## Decizii deschise — fără răspunsuri inventate de model

Acestea nu blochează redactarea planului. Se rezolvă cu dovezi și, când sunt alegeri de business, cu utilizatorul înainte de implementarea dependentă. Propunerile de mai sus sunt punct de lucru, nu aprobări implicite ale regulilor contabile.

| Decizie | Întrebarea / rezultatul necesar | Blochează |
|---|---|---|
| D01 — identificarea evidențelor | Asocierea fiecărui fișier/cale cu DC/subunitate/gestiune, inclusiv gestiuni multiple de același tip și proveniența configurației Alimente | Import real VC05 și activare |
| D02 — maparea anilor confirmată; detalii solduri | MF/OI au bază unică multianuală; Materiale/Alimente au tz_aaaa pe subunitate. Rămân maparea exercițiului/filtrului web, identitatea între fișierele anuale și eliminarea dublei contribuții a soldurilor de deschidere în lotul unic | Model anual VC03, transfer VC05, IA VC08 |
| D03 — reprezentarea INT | Tipul INT și zecimalele configurabile pe subunitate sunt decise. Rămân scara de codificare, domeniul numeric/overflow, reprezentarea cantităților, round pe linie/total, schimbarea preciziei fără reinterpretarea datelor și NULL; fără default 2 și fără DDL DECIMAL | DDL numeric, calcule și transfer |
| D04 — stoc/valoare actuale | Numai modelul Produse/ProduseP și fluxurile actuale QBC; sistemul Coduri/FIFO legacy este exclus. Rămân sursa valorii (variantă/linie), datele efective, retroactivitatea și regula stocului negativ; preț negativ deja interzis | BC/BR/FCS, balanțe și reportare |
| D05 — editare/anulare/ștergere | Politică document salvat/listat/închis; FK/cascade, rectificări și audit, corecții istorice, redeschidere | VC03 FK finale, scrieri VC06–VC08 |
| D06 — numerotare | Unicitate pe gestiune/exercițiu/tip/serie, reset anual, numere manuale, goluri/anulate, dubluri istorice | Numerotarea atomică și DDL unicități |
| D07 — drepturi locale | Operații VR, acces la subunitate/gestiune, politica listă ACL absentă; cine aprobă activarea/importul/reconcilierea și redeschiderea | Granturi și activarea scrierilor |
| D08 — partajarea nomenclatoarelor | Furnizori/clienți/CPV/UM/TVA/comisii comune sau locale, identitate reală și istoricul nume/preț | DDL și mapări de identitate; nu blochează extractul fidel |
| D09 — cazuri neacoperite de reguli | N01/N02 au regula H=0 sau abs(H−S)>1 → S și N07 se repară; nu cer reconfirmare. Rămân antete NULL/sume nedeterminabile, prețuri istorice lipsă și rolul intrărilor zero | Numai rezolvarea cazurilor neacoperite și verificarea rezultatelor |
| D10 — rapoarte/machete | Toate opțiunile SFP și cele 61 rapoarte, variante comune/specifice, metadate meniu/șabloane lipsă, traseu BC manual Alimente, BR/FCS pe MF/OI | Acceptarea UI/raportului afectat |
| D11 — e-Factura: EXCLUSĂ, decizie închisă pentru scopul curent | Fără transfer sau lucru în această rundă; viitoarea integrare cu Kbot.Efactura se planifică numai la cerere separată | Nu blochează implementarea/predarea VERCON curentă |
| D12 — transfer între contexte | Necesitatea transferului între gestiuni/subunități/DC și regulile lui, distinct de transferul OI între deținători | Numai noul flux dacă este solicitat; nu se inventează în nucleul curent |

Sunt deja decise și nu se redeschid: schema comună, prefixele VC_/VCxx și VRC.Migrator, ierarhia, N01/N02 și pragul 1 leu, stocarea INT, precizia pe subunitate, preț >=0, excluderile Coduri/e-Factura/DBS/clsDatabase, tipul imuabil, organizarea surselor, accesul exclusiv și transferul unic DC+subunitate. Detaliile tehnice rămase nu anulează aceste decizii.

## Reguli de implementare și urmărire

1. Implementarea începe numai când este cerută; această rundă editează documentația. Sursele Access/bazele reale rămân nemodificate, repo-ul și modificările preexistente nu se resetează.
2. Pentru audit se folosește motorul Access direct, read-only/snapshot, cu închidere în finally. Nu se apelează `clsDatabase.Version` (poate crea Ver), `Repara2024`, AExec, RunOnce, Functii sau IA. Nu se afișează parolele găsite local.
3. Fiecare subfelie are surse, scop, pași, livrabile, dependențe și acceptare. Starea se actualizează numai pe baza rezultatului, nu fiindcă textul planului a fost scris.
4. Stări de urmărire: PLANIFICAT, ÎN_LUCRU, IMPLEMENTAT_LOCAL, VERIFICAT_LOCAL, VALIDAT_SERVER, ACCEPTAT; blocajul are cauză, dovada și partea independentă realizabilă. Un build trecut nu este validare a migrării ori paritate Access.
5. Fișiere propuse de documentare: `VERCON/INVENTAR_OBIECTE.md`, `MAPARE_ACCESS_WEB.md`, `CONTRACT_CALCULE.md`, `CONTRACT_MIGRARE.md`, `DECIZII_DESCHISE.md`, `UTILIZARE_WEB.md`, `RAPOARTE.md` și `VERCON_STATUS.md`. Nu există implicit doar pentru că sunt numite aici.
6. Fiecare subfelie implementată primește `docs/worklog/SLICE-VCxx-yy-<scop>.md`, unde xx are două cifre (00–09): schimbare/motiv, fișiere, comenzi/probe și rezultate, limitări și următorul pas. Indexul/starea/ajutorul se actualizează împreună. Prefixul secțiunii portal VR și numele executabilului VRC.Migrator rămân distincte de numerotarea feliilor VCxx.
7. Workflow de predare propus după ADE: Codex dezvoltă/verifică local; utilizatorul publică prin AvacontPush, aplică SQL și validează Linux/MariaDB. Nu se presupune acces direct la server. După transferul unic nu mai există exploatare VBA, import ulterior sau compatibilitate cu vechea aplicație.
8. Preview local fără conexiuni externe sau credentiale reale: reutilizare model ADE, localhost:5050 dacă este liber, cu launcher separat. Portul/pornirea de producție și configurația live nu se schimbă pentru probe; adaptorul local nu poate fi activat ca bypass în producție.
9. DDL este livrat separat, versionat și cu precondiții/backup/restaurare. Migratorul nu face DDL. Schema sync trebuie să păstreze VC; eventualele modificări AVACONT_COMUN au script propriu și impact asupra celorlalte module descris.
10. Verificările de mai jos sunt de executat în implementare. Nu s-au executat teste de cod, migrare ori SQL în această rundă documentară. Testele sunt orientate pe invarianti și defecte reale, nu copii ale implementării.

## Slice VC00 — reguli și mapare verificabilă

**Dependențe:** DETECTIE_VBA, TZ_SCHEMA, exporturile și cerințele confirmate. **Rezultat:** alt model poate implementa fără să ghicească identitatea, formulele ori scopul obiectelor.

### SLICE-VC00-01 — inventarul și mediul de referință

**Pași:** inventariază sursele subunității (bazele multianuale MF/OI și toate bazele anuale Materiale/Alimente), frontendul și TZ_SCHEMA; hashes/schema/obiecte incluse. Obiectele Coduri/e-Factura/DBS/clsDatabase sunt marcate direct excluse. Metadatele SFP și FORMS/REPORTS se folosesc pentru reguli/schițe; nu se construiește un mediu de compatibilitate ori reparare Access. Eventuala referință dinamică pentru fluxurile incluse folosește numai o copie izolată.

**Livrabile:** manifest și inventar cu dispoziția fiecărui obiect; listă de lipsuri și impact funcțional. **Acceptare:** fiecare obiect este clasificat o singură dată în inventarul lui; sursele de date efective sunt identificate; lipsurile nu sunt prezentate ca funcții portate. Lipsa unui mediu Access executabil poate permite analiza, dar blochează validarea dinamică dependentă.

### SLICE-VC00-02 — dicționarul de date și FK

**Pași:** mapare tabel/câmp/tip/NULL/default/PK/index/FK → țintă și conversie; transcrie 29 relații TZ_SCHEMA, separă legăturile logice și profilează abaterile de versiune din toate bazele. Distinge ID de linie de ID de grup OI, zero/sentinel valid, ID random semnat, tabele fără PK.

**Livrabile:** dicționar complet și graf al importului, plus matrice FK/cascade sursă→țintă. **Acceptare:** toate cele 26 relații business au tratament explicit; niciun FK spre IDBP/IDBT/IDBC presupus unic și nicio pierdere de câmp necunoscut prin reader.

### SLICE-VC00-03 — contractele de calcul și datele etalon

**Pași:** urmărește numai fluxurile actuale QBC/QFCS/QSFP, Balanta22/IA, OI/MF/amortizare și fișa furnizor; extrage parametri/semne, N01/N02 cu TVA pe linie, INT/scară și precizia subunității. Codul care folosește Coduri/vechiul sistem este ignorat complet. Creează exemple pentru retur/transfer/casare, preț negativ refuzat și pragul strict 1 leu.

**Livrabile:** CONTRACT_CALCULE și rezultate așteptate conform regulilor confirmate. **Acceptare:** fiecare formulă inclusă are sursă și oracle; N07–N09 sunt corectate/demonstrate, N01/N02 respectă TVA și pragul, nicio probă sau implementare pentru sistemul Coduri.

### SLICE-VC00-04 — matricea machetelor și rapoartelor

**Pași:** mapează formularele/rapoartele incluse la tipuri/capabilități/drepturi/queries, urmărind SFP.MODEL și șabloanele de gestiune. Excluderile Coduri/e-Factura/DBS/clsDatabase nu se reanalizează. Pentru fiecare familie inclusă realizează schițe din proprietățile/controlurile/coloanele/secțiunile FORMS și REPORTS: context, selecții, antet, grilă, totaluri, comenzi și grupare/listare. Produce variante FC normal/MF, BC, BR/FCS, OI predare/transfer/casare, ListaAlimente, nomenclatoare, SFP și rapoarte comune/specifice.

**Livrabile:** MAPARE_ACCESS_WEB, RAPOARTE și `VERCON/SCHITE/INDEX.md` cu PNG-uri mici lizibile și legături la obiectele/controlurile sursă. Dimensiunile și ordinea din export sunt repere; adaptările web/mobil și elementele nerecuperabile se marchează explicit. **Acceptare:** fiecare familie inclusă are schiță, câmpurile/coloanele/grupările esențiale pot fi urmărite în export, OI/MF nu expun BC, Alimente are ListaAlimente și FC MF are extensia corectă. Imaginile nu reprezintă funcționalitate implementată.

### SLICE-VC00-05 — deciziile și planul executabil

**Pași:** înregistrează D01–D12, rezultatul sau investigația necesară, dependențele și criteriile de activare. Fixează sursele autoritative și structura worklog/status; actualizează documentația incremental.

**Livrabile:** registru decizii și backlog stabil. **Acceptare:** deciziile utilizatorului sunt distincte de propunerile modelului; fiecare implementare blocată de o decizie este identificată, iar activitatea independentă poate continua. Planul curent este livrabil documentar, nu dovada executării VC00-01–04.

## Slice VC01 — infrastructură web și sisteme comune

**Dependențe:** VC00-05 pentru schelet; maparea finală pentru funcții business.

### SLICE-VC01-01 — pachetul VERCON și publicarea

**Pași:** creează pachetul/rutele propuse, rezolvă fișierul gol vercon.py, înregistrează blueprint și secțiunea VR în portal fără coliziuni. Inspectează manifestul AvacontPush/LocalRoot/excluderile; nu publică automat întreg repo-ul.

**Livrabile:** schelet importabil și manifest de publicare local. **Acceptare:** importul fără MariaDB nu produce scrieri/conexiuni; toate fișierele executabile au o singură sursă/destinație; baze/exporturi/secrete nu intră în static ori upload.

### SLICE-VC01-02 — pagina și ciclul de viață

**Pași:** reutilizează portal/teme/meniuri/EventBus/ListenerTracker/registrul instanțelor; inițializare/cleanup pentru toate componentele și stări încărcare/gol/eroare/expirat.

**Livrabile:** pagină de bază și matrice sistem→import→inițializare→utilizare→cleanup→probă. **Acceptare:** deschidere/închidere repetată și schimbarea contextului nu cresc nejustificat listeners/timere; erorile folosesc mecanismul comun.

### SLICE-VC01-03 — adaptarea sesiunii, logului și conexiunilor

**Pași:** leagă contextul portal la logger/timing/repository; fabrici conexiuni și correlation ID. Verifică retry-ul, erorile și închiderea resurselor. Nu introduce pool/sesiune/auth paralele.

**Livrabile:** adaptoare și convenții diagnostice. **Acceptare:** refuz și eroare pot fi urmărite pe utilizator/context/operație fără parole ori date personale complete; scrierea nu este repetată de db_retry.

### SLICE-VC01-04 — preview reproductibil

**Pași:** launcher local cu adapter fictiv, două DC-uri, două subunități, gestiuni MF/OI/Alimente/Materiale și ani deschiși/închiși. Baze izolate/reset, fără config de producție la import.

**Livrabile:** comanda reală după implementare, reset și fixture-uri. **Acceptare:** zero conexiuni externe; port ocupat explicat; producția refuză fără sesiune indiferent de preview. SQLite/mock nu certifică FK/INT/ocuparea exclusivă MariaDB.

### SLICE-VC01-05 — integrarea documentelor și schemelor

**Pași:** integrează viewer/print/export, preferințe comune și schema sync; documentează extensiile VC și actualizarea separatei baze comune. Verifică regresia ADE/portal.

**Livrabile:** adapter documente și plan de propagare schema/publicare. **Acceptare:** raportul deschis din VERCON păstrează contextul; preferințele ADE sunt intacte; AVACONT_COMUN nu este țintă automată de schema sync.

## Slice VC02 — context și interfață de lucru

**Dependențe:** VC01, VC00-04; probele pot începe pe adapter fictiv.

### SLICE-VC02-01 — alegerea DC/subunității/gestiunii/anului

**Pași:** selectoare în cascadă, context permanent vizibil, endpoint context și versiune selecție; verifică autoselecția unei singure opțiuni și tipurile multiple. Păstrează/abandonează explicit draftul la tranziție.

**Livrabile:** selector complet și contract context. **Acceptare:** două gestiuni Materiale în aceeași subunitate sunt distincte; alegerea unui an nu mută un activ în altă gestiune; un răspuns vechi nu intră în context nou.

### SLICE-VC02-02 — meniuri, capabilități și coloane

**Pași:** construiește meniul din tip+drepturi+stare și ocuparea exclusivă; FC comun cu extensie MF, OI comun OI/MF, ListaAlimente numai Alimente. Tipul este editabil numai pentru baza/evidența nouă fără date; după populare, controlul și API-ul îl blochează. UI pornește de la schițele VC00-04.

**Livrabile:** matrice implementată UI/API. **Acceptare:** BC nu apare pentru OI/MF și un apel direct este refuzat; coloanele/duratele MF nu sunt aplicate gestiunii Alimente prin preferințe salvate.

### SLICE-VC02-03 — editare în grila comună

**Pași:** contract per coloană/editor/parse/format/validare, reutilizare controale, cheie stabilă rând și draft independent de DOM reciclat. Totalurile/read-only nu sunt editabile.

**Livrabile:** editori și formulare de antet/linii funcționale pe fixture. **Acceptare:** apostrof/diacritice, virgula zecimală, NULL și 0, date locale, combobox/date custom, scroll și focus sunt păstrate; nu se pierde draftul la virtualizare.

### SLICE-VC02-04 — salvare, conflict și sesiune expirată

**Pași:** conectează callbackurile asincrone la contractul revizie+RequestKey/context; distinge saved/error/conflict/unknown. Invalidările refreshului și răspunsurile lente sunt corelate cu selecția.

**Livrabile:** protocol JS de salvare și diagnostic pe linie/câmp. **Acceptare:** dublu Enter nu dublează documentul; timeoutul nu invită la o nouă cheie înainte de reconciliere; eroarea/sesiunea expirată păstrează datele editate.

### SLICE-VC02-05 — probe browser și ajutor

**Pași:** scenarii desktop/mobil/tastatură/teme/ferestre/grile, schimbarea contextului, date mari și deschideri repetate. Ajutor explicit pe flux și gestiune, folosind mecanismul existent.

**Livrabile:** matrice de probe efective și ajutor. **Acceptare:** toate combinațiile tip/context au traseul cerut; fără erori consolă sau resurse rămase; performanța este măsurată, nu declarată din implementare.

## Slice VC03 — schema VC și contractele persistente

**Dependențe:** VC00-02/VC00-03, D02/D03/D05/D06/D08 pentru partea afectată. VC01 nu este condiție pentru redactarea DDL.

### SLICE-VC03-01 — ierarhia și politica anuală

**Pași:** modelează ierarhia, tipul imuabil după populare, precizia pe subunitate și ocuparea pe DC+subunitate+gestiune fără an. MF/OI au istoric unic și filtru anual; Materiale/Alimente au exercițiile provenite din toate tz_aaaa, incluse în același transfer al subunității. Definește AnEvidenta vs DataDocument și soldurile fără dublare.

**Livrabile:** model logic și schema de context. **Acceptare:** surse cu date din ani multipli și mai multe gestiuni/tipuri în același DC sunt reprezentabile fără baze/tabele separate pe gestiune.

### SLICE-VC03-02 — tabelele business, tipurile și FK

**Pași:** dicționar final INT/scară/domeniu/NULL/lungimi, chei generate/numere și FK business valide. Include invariantul preț >=0, totalul N01/N02 și scara separată de numărul de zecimale configurabil pe subunitate. Introduce chei de context/indexuri/politica de ștergere fără modificarea schemei Access. Pentru LegBCLA aplică decizia din 12.10.2026 revizia 2: o listă are mai multe BC-uri; înlocuiește unicitatea contextuală IdLista cu index neunic, protejează perechea listă–BC și atribuirea unui BC unei singure liste. Generatorul, DDL și diagrama trebuie aliniate împreună; importul semnalează istoricul incompatibil fără deduplicare tacită.

**Livrabile:** DDL propus `sql/VC_01_schema.sql` și diagramă relații. **Acceptare:** toate tabelele proprii încep VC_; legătura între gestiuni e refuzată de FK și API; operațiile business nu pot pierde istoricul prin ștergerea unui nomenclator; aceeași listă persistă două BC-uri distincte, fără duplicate sau asocierea concurentă a unui BC la altă listă.

### SLICE-VC03-03 — importuri, audit și idempotency

**Pași:** modelează VC_Imports cu unicitate pe subunitate (DC implicit în baza țintă), toate sursele/mapările lotului unic, probleme/conversii și audit. Distinge markerul migrare unică de RequestKey al comenzilor web. ContextLock păstrează titular/sesiune/jeton/reînnoire pentru exclusivitate; document/grup OI și linie rămân identități distincte.

**Livrabile:** DDL registre/ocupare și contract diagnostic. **Acceptare:** aceleași ID-uri din surse diferite se remapează; al doilea transfer pentru subunitate este interzis indiferent de hash; markerul este atomic cu toate gestiunile; al doilea titular nu poate ocupa aceeași gestiune.

### SLICE-VC03-04 — configurare, numerotare și date comune

**Pași:** mapează UNIT/cale/Setari/NR și semnatarii, cu zecimale configurabile pe SubunitId, fără DC global sau Zec diferit implicit pentru fiecare gestiune. Dacă sursele subunității au ZEC diferit, planul consemnează alegerea comună și conversiile. Contoare/reference comune/ACL după contract.

**Livrabile:** maparea configurării și script separat de roluri/date comune dacă necesar. **Acceptare:** două gestiuni cu același număr istoric nu se contopesc; lipsa adresei comune este diagnosticată; importul nu rescrie numele/seria facturilor celorlalte module.

### SLICE-VC03-05 — aplicare, sincronizare și probă MariaDB

**Pași:** preflight, backup verificat, ordine scripturi/upgrade, impact schema sync AVACONT_SURSA→DC și restaurare în caz de DDL parțial. Verifică pe MariaDB de test, prin workflow-ul disponibil.

**Livrabile:** instrucțiuni și rezultate MariaDB pentru INT/scală/overflow, preț >=0, FK, ocupare exclusivă, unicitate migrare/subunitate și tip imuabil. **Acceptare:** upgrade DDL repetat detectat, eroarea nu avansează versiunea; regulile sunt dovedite pe motorul țintă. Acestea sunt verificări MariaDB, nu compatibilitate sau modificare Access.

## Slice VC04 — drepturi și izolare

**Dependențe:** VC01 și modelul VC03; D07 înainte de granturi finale.

### SLICE-VC04-01 — matricea operațiilor VR

**Pași:** mapează roluri comune la citire, nomenclatoare, recepție, consum, retur/storno, OI predare/transfer/casare, MF, alimente, rapoarte/export, import/reconciliere/activare, setări, închidere/redeschidere. Tipul limitează operațiile chiar dacă rolul le permite.

**Livrabile:** matrice operație→rută→rol→tip→stare și SQL granturi pentru utilizator. **Acceptare:** niciun rol nou nu primește implicit toate operațiile; operațiile provin din Roluri_Operatii, nu dintr-un câmp Roluri presupus suficient.

### SLICE-VC04-02 — guard și repository cu context verificat

**Pași:** validează DC din sesiune, subunitate/gestiune/an pe server și revizia contextului; repository limitează toate queries. Permisiunile și protecțiile comune pentru scrieri sunt reutilizate; ID-ul trimis singur nu autorizează.

**Livrabile:** guard și contract repository. **Acceptare:** citire/scriere/PDF/export/job pe altă gestiune sunt refuzate; lipsa tabelului drepturilor/eroarea verificării nu deschide accesul.

### SLICE-VC04-03 — ACL locale și contul migratorului

**Pași:** aplică D07 pentru restricții locale, fără conturi/parole duplicate; separă privilegii API business, citire rapoarte, migrator și DDL. Migratorul direct MariaDB validează ținta și are numai granturile necesare.

**Livrabile:** ACL și instrucțiuni privilegii/revocare. **Acceptare:** contul de import nu poate modifica arbitrar AVACONT_COMUN sau alte module; ACL absentă are comportament definit și testat, fără copiere implicită a regulii ADE.

### SLICE-VC04-04 — ocupare exclusivă, cache, drafturi și joburi

**Pași:** ocupare atomică pe DC+subunitate+gestiune pentru un singur utilizator, titular+session+jeton, heartbeat/eliberare/expirare/recuperare. Citește și scrie numai cu ocupare validă, inclusiv rapoarte și joburi. Definește tratamentul taburilor aceluiași utilizator și blocarea cererii vechi după expirare/preluare. Migratorul rezervă toate gestiunile subunității. Cache/drafturi sunt contextualizate, fără TmpFact global.

**Livrabile:** serviciu de ocupare, diagnostic „ocupat”, convenții și invalidări. **Acceptare:** al doilea utilizator nu intră în aceeași gestiune nici pe alt an; gestiuni distincte permit titulari diferiți. Expirarea/preluarea nu permite scrieri ale vechiului titular; închiderea anului folosește aceeași exclusivitate, fără sistem paralel pentru T24.

### SLICE-VC04-05 — probe de acces și audit

**Pași:** fiecare operație permis/refuzat, inclusiv ocupare simultană, alt an în aceeași gestiune, expirare/heartbeat/preluare, apel API/PDF direct, revocare și sesiune veche. Auditul distinge ocupat, refuz, rollback, commit și necunoscut.

**Livrabile:** rezultate fixture/API și instrucțiuni server. **Acceptare:** fiecare refuz așteptat este dovedit fără efecte business; conturile/granturile reale sunt validate separat de mock.

## Slice VC05 — VRC.Migrator și reconcilierea importului

**Dependențe:** VC00, VC03, VC04; D01–D04/D08/D09 pentru transformările afectate. Citirea read-only poate începe înaintea DDL.

### SLICE-VC05-01 — reader și profilare fără scriere

**Pași:** proiect Windows VRC și reader read-only pentru toate sursele incluse în lotul unic, cu resurse eliberate, fără VBA/DBS/clsDatabase sau modificarea Access. Citește câmpurile business necesare și raportează lipsurile; nu construiește compatibilitate cu versiuni ale aplicației vechi. Verifică inventarul complet MF/OI multianual și Materiale/Alimente anual, linked tables și motorul de citire.

**Livrabile:** VRC.Migrator.exe cu Citește și raport profilare. **Acceptare:** hashes/schema/date ale originalelor nu sunt modificate de reader; o bază fără Ver rămâne fără Ver; câmpurile absente sunt raportate diferit de NULL. Doar Access pe 64 de biți (decis 12.10.2026).

### SLICE-VC05-02 — planul și formularul de asociere

**Pași:** manifest al tuturor surselor subunității, DC/subunitate și asocierea fiecărei surse la gestiune/tip/an după organizarea confirmată. Precizie comună aleasă pe subunitate, conversii INT și N01/N02. UI ADE: Citește/Testează/Migrează/Oprește/jurnal; markerul migrare existent blochează definitiv Migrează, chiar pentru un fișier nou. Schimbarea planului înainte de transfer cere retestare.

**Livrabile:** plan complet exportabil și UI cu motive concrete de blocare. **Acceptare:** toate gestiunile/anii intră împreună în singurul lot; o sursă lipsă e diagnosticată înainte de transfer; N01/N02 normalizează totalul în plan/țintă, păstrând originalul fără a scrie în Access. Specificul importat nu se schimbă după populare.

### SLICE-VC05-03 — writer atomic și maparea cheilor

**Pași:** preflight/schema/țintă/marker reverificate, rezervare întreagă subunitate, ID generate și remapare surse în ordinea grafului, conversii INT/scară/preț >=0/N01/N02, solduri anuale fără dublare. Tranzacție ori staging cu publicare atomică a **tuturor gestiunilor lotului unic**, fără commit business independent pe gestiune.

**Livrabile:** writer parametrizat și diagnostic transfer unic. **Acceptare:** ID-uri locale remapate fără coliziuni, alte subunități intacte; eroarea în orice gestiune nu publică parțial subunitatea; registrul OI și soldurile nu sunt dublate. Markerul migrare împiedică orice import ulterior. Fără DDL, corecții sursă/comune sau compatibilitate VBA.

### SLICE-VC05-04 — oprire, jurnal și diagnostic al rezultatului

**Pași:** cancel înainte de commit, rollback explicit pentru lotul complet, COMMIT necunoscut verificat prin markerul DC+subunitate și manifest; progres/sursă/gestiune fără promisiune de commit parțial. Logurile sunt pe DC/subunitate/lot și UI se închide numai după eliberarea resurselor/ocupării.

**Livrabile:** stări UI/jurnal și instrument read-only de diagnostic. **Acceptare:** oprirea nu produce migrare parțială; COMMIT trimis nu este pretins anulat; răspunsul pierdut cere verificarea transferului existent, nu inserarea din nou. Un lot confirmat închide definitiv migrarea acelei subunități.

### SLICE-VC05-05 — reconciliere și probe complete de migrare

**Pași:** compară sursa cu ținta după transformările aprobate, INT/scară, zecimale pe subunitate, TVA per rând și N01/N02; chei/rânduri/NULL/FK/solduri. Verifică refuzul oricărei noi migrări după markerul unic, chiar cu fișier redenumit/modificat ori alt an. Testează întreruperea lotului, proveniența originalelor și o diferență introdusă deliberat. Nu implementează flux de reimport/compatibilitate.

**Livrabile:** raport pe subunitate/lot/sursă/gestiune, README VRC și pachet de probă. **Acceptare:** zero pierderi/diferențe neexplicate după regulile aprobate, N01/N02 vizibile în proveniență și nicio gestiune publicată separat; VBA nu mai este folosită după transfer. Validarea rapoartelor web se închide în VC08-05.

## Slice VC06 — nomenclatoare și documente comune

**Dependențe:** VC02–VC05 pentru acceptarea pe date importate; D03–D06/D08. Serviciile folosesc protocolul tranzacțional comun.

### SLICE-VC06-01 — nomenclatoare și configurarea gestiunii

**Pași:** produse, variante, categorii/conturi Masini, furnizori, clienți/deținători, UM/CPV/TVA/comisii, gestionari și setări după mapare. Inactivare în locul pierderii istoricului, revizie și validare comună, numere/code texte păstrate.

**Livrabile:** API/ecrane grilă/selectoare și ajutor. **Acceptare:** schimbarea nomenclatorului nu rescrie cantitatea/prețul/emitentul documentului istoric; variantele și produsele sunt identități separate; aceeași denumire din altă gestiune nu este deduplicată automat.

### SLICE-VC06-02 — FC/NIR, inclusiv extensia MF

**Pași:** selectare/document nou/editare, furnizor/comisie, toate liniile și TVA individuală, total declarat/calculat după N01/N02, preț >=0, INT/scară și zecimale ale subunității. MF: COD/inventar/data/durate/ProduseM; toate efectele în tranzacție cu ocupare exclusivă validă.

**Livrabile:** recepție completă și listare NIR salvat. **Acceptare:** antet 0 sau abs(H−S)>1 → S; exact 1 și antet nenul → H; TVA mixtă/zero/already-included nu este dublată. Preț negativ/document fără linii refuzat; rollback la ultima linie/MF. Numărul de zecimale nu este fixat la 2.

### SLICE-VC06-03 — BC și controlul disponibilului

**Pași:** alegere produse/variante la dată, draft curent exclus din istoricul aceluiași document, calcul disponibil, revizie, lock și validare la salvare, editare/anulare în limitele D05. Destinat gestiunilor fără MF/OI, inclusiv circuitului Alimente.

**Livrabile:** BC/stoc/operații atomice. **Acceptare:** al doilea utilizator e blocat înainte de acces, inclusiv pe alt an; titularul validează disponibilul la precizia subunității și aplică N02/INT/preț >=0. La precizie 4, 1,0040 nu este permis pe 1,0000; storno ulterior nu schimbă disponibilul istoric.

### SLICE-VC06-04 — BR și FCS

**Pași:** documente referite/cantități eligibile, preț document/preț variantă, antet/linii/legături atomice, tratament retur/storno parțial și multiplu. Decide efectul la dată și capabilitățile pe MF/OI după D04/D10.

**Livrabile:** retur/storno și rapoarte. **Acceptare:** cantitate peste permis sau alt context refuzat; preț >=0 inclusiv storno, fracțiunea existentă este reprezentată pe INT/scară și nu inventată din NULL/Long. Semnul reversării aparține mișcării; eroarea nu lasă document parțial.

### SLICE-VC06-05 — editare, anulare și traseul documentului

**Pași:** stări/revizii/listare/anulare, documente legate și regula anului închis; verifică efectul editării/ștergerii asupra LegBCLA, OI și MF. Auditul arată valorile înainte/după și motivul fără a distribui date personale în loggerul general.

**Livrabile:** operații de corecție și istoric document. **Acceptare:** ștergerea BC legat nu lasă lista fără relație; revizie veche produce conflict; document închis nu este editabil prin apel API direct. NIR/BC/retur/storno au același protocol și succes numai după commit.

## Slice VC07 — specificul Alimente, OI și MF

**Dependențe:** VC06 și VC00-03/VC00-04; rezultatele de stoc disponibile din contract/VC08-01 înainte de acceptarea fluxurilor dependente.

### SLICE-VC07-01 — ListaAlimente și rețetar

**Pași:** LABC/ListaAlimente, mese MicDejun/Dejun/Cina, porții/beneficiari/feluri, rețetar, costuri și consum asociat. BC se creează separat. O listă are mai multe BC-uri, afișate într-un DGV identic vizual cu NIR: un rând per BC, valoare pe rând, TOTAL BC în footer, valoare listă și diferență față de suma BC-urilor. Adaugă rând din footer deschide popupul cu arbore de bonuri din gestiunea Alimente curentă neasociate altei liste; alegerea adaugă, nu înlocuiește setul. Fără duplicate; ⋮ scoate numai asocierea din draft. Salvarea antetului/meselor/întregului set LegBCLA este atomică, cu revalidarea fiecărui BC; nu generează/resalvează bonuri și nu repetă consumul. DGV-uri NIR compacte pentru rezumate și feluri, fără padding mare/carduri în celule și fără Alimente necesare. Arborele listelor în dreapta, formatat ca NIR/BC/BP, se ascunde în editor. Vezi FLUXURI_UI, corecțiile din 12.10.2026 revizia 2; aliniază cardinalitatea țintă VC03-02 înainte de implementare.

**Livrabile:** flux complet listă nouă/editare/listare/legare/dezlegare autorizată. **Acceptare:** 127 liste istorice Alimente din profilul DETECTIE sunt păstrate și reconciliate când se folosește același snapshot; o eroare între listă și bon nu lasă legături parțiale. Se testează masă absentă/porții 0/NULL și schimbare rețetă fără recalcul tacit al istoricului.

### SLICE-VC07-02 — OI predare și registrul deținătorului

**Pași:** predare la deținător, legătura consum/predare/registru, cod inventar după tip; identitatea documentului și liniilor și istoricul la dată. Aceleași machete și pentru MF, cu capabilități distincte.

**Livrabile:** OI_Predare și fișa deținător/produs. **Acceptare:** predarea scade stocul de magazie și crește deținerea fără dublarea consumului prin însumarea registrelor; IDBP repetat între linii se importă și se operează corect; altă gestiune este refuzată.

### SLICE-VC07-03 — OI transfer și casare

**Pași:** vechi/nou deținător, document/linii/registru, minus+plus atomice la transfer, casare și sold eligibil. Date capete interval, stare an și revizie; nu confunda IDBC OI cu BC.IdBonConsum.

**Livrabile:** OI_Transfer/OI_Casare și documente listabile. **Acceptare:** transfer net zero, solduri corecte; casarea 2×100 scade 2 bucăți/200 lei, inclusiv capetele intervalului. Prețul rămâne pozitiv/zero și orice preț negativ e refuzat; semnele mișcării nu schimbă această invariantă. Titular exclusiv și cantitate eligibilă reverificate.

### SLICE-VC07-04 — evidența MF și continuitatea activului

**Pași:** ProduseM, cod inventar, dată intrare, DNF/DF/valoare lunară, asocierea celor două câmpuri istorice ValoareLuna/ValoareLunara; istoric schimbări, deținător și casare. Nu suprascrie datele anilor închiși prin modificarea variantei curente.

**Livrabile:** evidență MF, extensii FC și documente OI. **Acceptare:** activul păstrează identitatea în istoricul multianual unic, fără copiere/reimport pe an; datele amortizării sunt diagnosticate și valorile codificate INT/scară. Cele 12 rânduri ProduseM observate nu sunt confundate cu intrările/ieșirile.

### SLICE-VC07-05 — verificarea end-to-end pe tipuri

**Pași:** recepție→consum/listă ori predare→transfer/casare→retur/storno eligibil→raport, pentru fiecare tip și două gestiuni de același tip. Rulează și scenarii de rollback la actualizarea registrelor, nu doar API de antet.

**Livrabile:** matrice flux/rezultat și ajutor pentru specific. **Acceptare:** niciun efect dublat între stoc/OI/MF/Alimente; meniurile corespund tipului și API refuză funcțiile străine; scenariile fără date reale sunt marcate sintetice, nu paritate pe producție.

## Slice VC08 — calcule, rapoarte și închidere an

**Dependențe:** VC03/VC05 și contractele VC00; fiecare acceptare folosește fluxurile VC06/VC07 relevante. **Ordine internă:** motor stoc înaintea acceptării consumului; raportul urmează motorul, iar reportarea urmează reconcilierea.

### SLICE-VC08-01 — motorul stocului și balanțelor

**Pași:** implementare fără Forms/TempVars, filtre context pe toate ramurile, sold la dată/interval, produs/variantă/furnizor/categorie și disponibil draft. Corectează explicit N07–N09 în comparația etalon.

**Livrabile:** servicii de stoc, fișă și balanță cu contract comun UI/validare/raport. **Acceptare:** SI+rulaje=SF pe fiecare variantă și deținător unde aplicabil; mutarea între furnizori/produse resetează soldul corect; schimbarea filtrelor nu scoate mișcări necesare soldului inițial. Egalitatea generală nu maschează diferențe locale.

### SLICE-VC08-02 — motorul amortizării

**Pași:** implementează calendar/durate/rest ultima lună/DF conform contractului, rolul ValoareLuna/ValoareLunara, efectul datei intrării/predării/casării. Detectează multiplicarea prin JOIN când activul are mai multe intrări/predări.

**Livrabile:** amortizare reproductibilă la dată, fără modificarea istoricului. **Acceptare:** ultima lună închide restul conform regulii, durata 0/NULL nu provoacă divizare invalidă, transferul nu resetează arbitrar amortizarea; rezultatele pe activ sunt comparate, nu doar totalul MF.

### SLICE-VC08-03 — catalogul și generarea tuturor rapoartelor incluse

**Pași:** aplică întâi excluderile confirmate asupra inventarului de 61 rapoarte; cele care folosesc Coduri/e-Factura nu se portează și nu se investighează suplimentar. Pentru cele incluse, mapează comun/specific/subreport, MODEL/TIP/query/parametri/câmpuri/sortare/grupare/totaluri și schițele VC00-04. PDF/listare/XLS au precizia subunității și valori INT decodificate după contract.

**Livrabile:** catalog RAPOARTE, generatoare și exemple verificate vizual, cu drepturi server. **Acceptare:** numere/rânduri/totaluri corecte, fără coloane lipsă sau text tăiat; rapoartele OI/MF/Alimente nu apar la alt tip. Funcția comună nu presupune că toate variantele de balanță sunt interschimbabile.

**Acoperire minimă de verificat:** NIR/NIR_Storno/NIR_SFP; Bon_Consum/BonRetur; fișe magazie/produs/furnizor; balanțe produse și variante BF/AFB; centralizatoare BC/conturi/furnizori/consumuri; inventare; toate familiile OI_Bon/centralizator/fișă/registru/inventar; MF_Bon/Balanta/inventar/Amortizare; ListaAlimente2015 cu subreports mese; comenzi și referate/plan achiziții/situații intrări/stoc final. Lista exactă și eventualele excluderi justificate sunt în VC00-04, nu se limitează la această enumerare pe familii.

### SLICE-VC08-04 — închiderea și redeschiderea anului

**Pași:** folosește ocuparea exclusivă deja implementată în VC04-04; validează titularul/anul/documentele și soldurile. Materiale/Alimente: închidere și solduri de deschidere ale exercițiului următor. MF/OI: păstrează baza/evidența multianuală, cu calcul/snapshot la dată fără copierea tuturor activelor într-o altă bază. Redeschiderea și efectele ulterioare urmează D05.

**Livrabile:** IA web/AnnualClosings/raport înainte-după. **Acceptare:** al doilea operator e refuzat prin T12; nu se codează mecanism paralel de închidere concurentă. Eroarea/cererea repetată a titularului nu dublează reportarea, soldurile se reconciliază și MF/OI rămân istoric unic. Niciun import Access ulterior nu este necesar.

### SLICE-VC08-05 — comparator Access/import/web

**Pași:** compară trei straturi: sursa stocată, ținta importată, rezultatul calculat web. Utilizează documente reale pe copii și cazurile sintetice, toleranțe pe câmp/raport, inclusiv NULL și limite interval; urmărește fiecare diferență intenționată a motorului.

**Livrabile:** raport paritate pe gestiune/an/document/produs/deținător/activ/raport, cazuri reproduse și decizii. **Acceptare:** zero diferențe neexplicate și corecțiile cunoscute sunt demonstrate; raportul care moștenește defectul Access nu este acceptat doar pentru „egalitate”. Paritatea pe exemple nu este etichetată audit legal/fiscal complet.

## Slice VC09 — integrare finală, basculare și predare

**Dependențe:** funcțiile incluse VC01–VC08. e-Factura este exclusă prin decizie și nu reprezintă dependență a lansării curente.

### SLICE-VC09-01 — verificarea scopului și funcțiilor auxiliare incluse

**Pași:** verifică numai funcțiile auxiliare incluse pentru gestiune: exporturi/șabloane/fișiere rapoarte și ajutor. Confirmă că nu există rute/readere/writere/UI pentru e-Factura, Coduri, DBS/clsDatabase și compatibilitate VBA. Nu inventariază în detaliu, recuperează ori integrează EF/EFS/EFT/XML/tokenuri. Integrarea Kbot.Efactura rămâne în afara taskului actual.

**Livrabile:** matrice scop/excluderi și funcțiile auxiliare de gestiune incluse. **Acceptare:** excluderile nu au cod/date de migrare și nu blochează predarea; ajutorul/rapoartele folosite sunt disponibile. Nu se declară e-Factura implementată în această portare.

### SLICE-VC09-02 — backup, restaurare și operare

**Pași:** plan backup DC+config comun relevant+registre+documente, înainte de import/DDL/închidere; probă de restaurare într-o țintă separată și verificare integritate/hashuri. Retenție/acces/monitorizare operațională după infrastructura existentă.

**Livrabile:** procedură de backup/restaurare și rezultate reale. **Acceptare:** restaurarea reconstituie relații/solduri/registre și documente, nu doar un fișier prezent; logurile și sursele rămân protejate; nu se presupune consistența copiei DB în timpul scrierii fără metoda potrivită.

### SLICE-VC09-03 — performanță și regresie

**Pași:** date/volum reprezentativ, paginare/queries/indexuri/rapoarte; ocupare exclusivă la același context și utilizatori simultani numai pe gestiuni distincte. Probe Linux/MariaDB/portal/ADE/schema sync/publicare, fără compatibilitate VBA.

**Livrabile:** măsurători și corecții motivate, matrice regresie. **Acceptare:** latențe/praguri sunt stabilite din măsurători și necesitățile utilizării; raport mare nu blochează inutil toate gestiunile; fără schimbări arbitrare ale workerilor/infrastructurii live.

### SLICE-VC09-04 — migrarea finală și bascularea

**Pași:** probă înaintea basculării pe țintă de test separată, rezolvarea problemelor, oprire scrieri VBA și snapshot final complet pentru subunitate. Testează și transferă **o singură dată toate gestiunile/anii**, reconciliază, înregistrează markerul unic și activează web. Din momentul migrării VBA nu mai este folosită; problemele ulterioare se rezolvă în web/backup, fără reimport sau revenire la exploatare VBA.

**Livrabile:** checklist și rezultat pe DC+subunitate cu toate gestiunile/sursele. **Acceptare:** nu există funcționare paralelă, import suplimentar sau compatibilitate cu VBA; markerul unic blochează o nouă migrare. Restaurarea web păstrează markerul/proveniența și nu pierde documentele noi; sursele rămân arhivă read-only, nu aplicație operațională.

### SLICE-VC09-05 — predarea și acceptarea

**Pași:** manual utilizare pe tip, README VRC.Migrator, SQL/publicare/backup, status și worklogs, exemple/cazuri de eroare, inventarul funcțiilor incluse/reportate și deciziile. Închide acceptarea cu rezultatele efective ale utilizatorului.

**Livrabile:** pachet complet reviewabil și matrice criteriu→dovadă→stare. **Acceptare:** toate gestiunile incluse au migrare reconciliată, funcții/rapoarte validate și drepturi verificate; limitele rămase sunt exacte. Cod scris, build, test local, publicare, DDL server și acceptare utilizator sunt consemnate separat.

## Probe obligatorii și trasabilitate

Lista de mai jos este minimul relevant pentru defectele/contextul cunoscut. Nu înlocuiește scenariile pe fiecare raport și operație mapată. Rezultatele se consemnează în implementare; coloana „Felie” arată responsabilitatea, nu teste deja trecute.

| Caz | Date / acțiune | Rezultat cerut | Felie |
|---|---|---|---|
| T01 — izolare ierarhică | Două DC, două subunități, două gestiuni Materiale; aceleași ID/numere locale | API/FK/cache/draft/PDF nu amestecă datele | VC03/VC04/VC05 |
| T02 — specific imuabil | Gestiuni distincte OI/MF/Alimente/Materiale; încearcă modificarea tipului unei gestiuni populate, inclusiv API direct | Schimbarea e refuzată; alegerea tipului numai pe bază/evidență nouă fără date. Selectarea altei gestiuni schimbă meniul, nu tipul celei populate | VC02/VC03/VC04/VC07 |
| T03 — surse pe ani | O bază completă per MF/OI; toate tz_aaaa Materiale/Alimente din folderele subunității | Un lot DC+subunitate include toate sursele; MF/OI rămân multianuale, soldurile anuale Materiale/Alimente nu se dublează | VC00/VC03/VC05 |
| T04 — NULL/zero/absent | Antet NULL/0 și câmp absent | Originalul/distincția rămân în proveniență; antet 0 → suma liniilor; NULL/sumă lipsă se tratează explicit, fără abs(NULL) | VC03/VC05 |
| T05 — N01/N02 | Recalculează NIR 1/BC 7 cu TVA individuală; H=0 și abs(H−S)>1 | Ținta păstrează S conform regulii, originalul și conversia sunt în raport; nu se cere reconfirmarea pragului aprobat | VC05/VC06/VC08 |
| T06 — preț și stocare | Preț existent 100,25; preț sursă Long; schimbă afișarea la 3/4 zecimale | Valori pe INT/scară exactă, fără trunchiere/scară fixă 2; fracțiunea absentă nu este inventată, preț >=0 | VC00/VC03/VC06 |
| T07 — cantitate zero | 40 intrări OI cu Cant=0 | Rânduri păstrate, rolul explicat | VC05 |
| T08 — casare și preț nenegativ | Casare 2×100; încearcă preț −100 la recepție/BC/storno/retur/OI/MF/Alimente/import | Casarea are efect −2/−200 la preț +100; orice preț negativ e refuzat global, fără ABS automat | VC03/VC05/VC06/VC07/VC08 |
| T09 — capete interval | Casare/storno la început, sfârșit, cu oră și ulterior | Includere conform contractului, istoricul nu se schimbă de la un storno viitor | VC08 |
| T10 — fișă furnizor | Două produse/un furnizor și același produs/doi furnizori | Reset sold când se schimbă oricare componentă | VC08 |
| T11 — precizie pe subunitate | Subunitate cu precizie 4: disponibil 1,0000, cerere 1,0040; altă subunitate cu altă precizie | Validarea folosește setarea subunității, refuză excesul la precizie 4; fără default 2/DC global | VC03/VC06 |
| T12 — acces exclusiv | Doi utilizatori încearcă aceeași DC/subunitate/gestiune, inclusiv pe ani diferiți; apoi gestiuni distincte | Al doilea e refuzat înainte de acces în aceeași gestiune; titulari diferiți numai pe contexte diferite, jeton verificat la fiecare operație | VC03/VC04/VC06 |
| T13 — rollback | Eroare după antet, după ștergere linii, la ultima linie ori actualizare MF/LegBCLA/OI | Starea business/registre/mapări revine complet | VC05/VC06/VC07 |
| T14 — chei OI | Mai multe linii cu același IDBP/IDBT/IDBC | Linie și grup distincte, normalizare fără FK nevalid | VC03/VC05/VC07 |
| T15 — transfer OI | Deținător A→B, apoi casare B | Minus/plus net zero, deținere corectă, fără dublare consum | VC07/VC08 |
| T16 — linked tables | Frontend fără backend disponibil; TZ_SCHEMA fără metadate de meniu | Lipsuri diagnosticate, nu import gol prezentat ca reușit | VC00/VC05 |
| T17 — migrare unică | Marker confirmat DC+subunitate; încearcă accesul la Migrează cu alt hash/fișier/an/gestiune | Blocare definitivă, fără reimport/update/sincronizare; nu se execută din nou writerul | VC03/VC05 |
| T18 — rezultat necunoscut | Importul pierde confirmarea COMMIT; separat, o comandă web a titularului pierde răspunsul | Import: doar diagnostic marker/lot fără reinserare. Web: RequestKey întoarce rezultatul confirmat fără document duplicat | VC05/VC06 |
| T19 — anulare migrator | Oprește înainte de commit, în timpul SQL și după commit trimis | Rollback confirmat sau necunoscut explicat, fără promisiune falsă | VC05 |
| T20 — format/texte | O'Brien, diacritice, 01/02, virgulă zecimală, cod cu zero inițial | Parametri, parsare explicită și texte exacte | VC02/VC03/VC06 |
| T21 — log indisponibil | Disc plin/acces log refuzat, eșec rollback/conexiune | Stare și diagnostic reale, fără secrete/succes fals | VC01/VC05 |
| T22 — lista și bonurile | Adaugă două BC-uri existente, încearcă duplicat, scoate unul, Renunță; un BC devine neeligibil; editare/anulare BC legat; modifică rețeta după salvare | DGV NIR cu valori per BC și totalul sumei; popup filtrat pe Alimente; antet/mese/set de legături atomice, fără modificări parțiale, BC noi/resalvate sau consum repetat; cardinalitate 1:N | VC03/VC06/VC07 |
| T23 — amortizare | Durată 0/NULL, lună limită, februarie bisect, ultim rest, mai multe predări | Rezultat per activ, fără multiplicare JOIN/divizare invalidă | VC08 |
| T24 — închidere protejată prin T12 | Al doilea operator încearcă închiderea gestiunii ocupate; titularul pierde răspunsul ori anul următor există | Refuz prin aceeași ocupare exclusivă T12, fără logică paralelă de concurență; rollback/RequestKey previn reportarea dublă a titularului | VC04/VC08 |
| T25 — FK/cascade | Șterge furnizor/client/produs folosit; referă antet/variantă din altă gestiune | Politica D05, fără dispariția istoricului ori legături între contexte | VC03/VC04/VC06 |
| T26 — drepturi și expirare | Rol citire, revocare, manipulare ID/context, PDF direct | Refuz coerent fără efecte; draft păstrat în UI | VC02/VC04 |
| T27 — raportare | Toate rapoartele incluse, totaluri/parametri/layout/export | Paritate explicată și verificare vizuală, fără text/coloane tăiate | VC08 |
| T28 — recuperare numai web | Backup/restaurare separată după transferul unic și documente web noi | Date/marker migrare/registre/documente păstrate; fără revenire la folosirea VBA ori reimport Access | VC09 |
| T29 — date comune contradictorii | UNIT/cale vs Unitati/Unitati_Date/Conturi | Diagnostic separat; fără UPDATE automat în comune | VC00/VC03/VC05 |
| T30 — regresie comună | Portal/ADE/grilă readonly/teme/schema sync/publicare | Funcțiile existente păstrate, VC sincronizat numai unde este țintă | VC01/VC02/VC09 |
| T31 — INT/scară și precizie | Două subunități cu 3 și 4 zecimale, fiecare cu mai multe gestiuni; schimbare afișare și valori la limita INT | DDL valori INT; toate gestiunile folosesc precizia subunității; valorile existente nu se reinterpretează, overflowul/trunchierea sunt refuzate | VC00/VC03/VC05/VC06 |
| T32 — prag 1 leu și TVA mixtă | Linii cu cote distincte/0/fără TVA/already-included; H=0, diferențe 0,99/1,00/1,01 | H=0 → S; diferență >1 → S; exact 1 și H nenul → H; TVA o singură dată pe linie, fără round implicit 2 | VC00/VC05/VC06/VC08 |
| T33 — expirare acces | Titular dispare; expiră ocuparea și apare alt titular; ajunge o cerere întârziată a celui vechi | Recuperare controlată, vechiul jeton refuzat; aceeași regulă pentru raport/consum/închidere | VC04/VC06/VC08 |
| T34 — scop și schițe | Inventar excluși Coduri/e-Factura/DBS/clsDatabase; schițe FORMS/REPORTS pentru familiile incluse | Fără cod/transfer/compatibilitate pentru excluși; PNG-uri/index cu trasabilitate câmp/control, fără a declara UI implementată | VC00/VC09 |

## Criterii de finalizare și ordinea concretă de reluare

**Documentația de față:** acoperă cerințele, constatările acceptate, contractele și 50 subfelii planificate. Exhaustivitatea inventarului și paritatea efectivă trebuie demonstrate în VC00/VC08; planul nu afirmă că toate cele 377 obiecte au fost auditate dinamic.

**Implementarea este completă numai când:** funcțiile incluse au sursă/mapare/schiță/serviciu/UI/raport și acceptare; transferul unic complet pe DC+subunitate și soldurile sunt reconciliate după regulile aprobate; toate valorile sunt INT cu codificare explicită, precizia este pe subunitate, prețul >=0 și accesul exclusiv sunt impuse; specificul populat nu se schimbă. MariaDB/Linux/publicarea/restaurarea și fluxurile reale sunt validate. Excluderile Coduri/e-Factura/motoare schemă Access sunt respectate; nu există compatibilitate ori exploatare VBA după transfer.

**Ordine recomandată pentru următorul model:**

1. Citește `DETECTIE_VBA.md`, prezentul plan și clarificările confirmate; verifică inventarul actual și regulile repo. Nu redeschide schema comună identică ori ierarhia aprobată.
2. La solicitarea implementării, începe cu VC00-01/VC00-02/VC00-03/VC00-04 și consemnează incremental dovezile. Între timp poate pregăti VC01 și probe UI pe date fictive fără conexiuni reale.
3. Rezolvă D01–D08 relevante înainte de DDL/import/calcul; finalizează VC03/VC04. Nu deduce decizii contabile dintr-un defect VBA ori din datele unui singur an.
4. Construiește VC05 pentru pachetul complet DC+subunitate și probează înaintea basculării pe țintă de test separată. În producție, singurul transfer include toate gestiunile/anii și închide definitiv exploatarea VBA; nu există import separat ulterior ori compatibilitate de codat.
5. Implementează motorul VC08-01 și folosește-l în VC06/VC07; închide paritatea și rapoartele VC08, apoi VC09. Ordinea numerică a feliilor nu obligă scrierea consumului înainte de motorul lui de stoc.
6. La fiecare pas actualizează status/worklog/contract/ajutor; separă documentat, implementat, verificat local, validat server și acceptat. Nu executa upgrade/reparare/macrocomenzi pe sursele originale.

**Checkpoint curent — 11.10.2026:** planul a fost actualizat incremental cu toate clarificările utilizatorului, inclusiv transferul unic complet DC+subunitate și încetarea folosirii VBA, fără compatibilitate de implementat. Cele 50 subfelii VCxx rămân PLANIFICAT, iar PNG-urile sunt livrabile planificate la VC00-04. În această actualizare s-a modificat numai PLAN_IMPLEMENTARE.md: fără cod, schemă/date Access sau MariaDB, DETECTIE_VBA, migrator construit, import/publicare/commit.

**Verificare documentară efectuată:** 50 identificatori de subfelii unici, fără goluri VC00-01–VC09-05; fiecare are Pași/Livrabile/Acceptare; 34 scenarii T01–T34; toate linkurile locale existente se rezolvă. Hashul SHA-256 al DETECTIE_VBA a rămas identic cu cel citit înaintea editării. Aceasta validează structura documentului și păstrarea analizei, nu executarea scenariilor de implementare.

## Checkpoint implementare — 11.10.2026 (SLICE-VC00-01…05)

Implementarea a inceput cu felia VC00 (documente + generatoare locale; **fara cod de aplicatie, DDL, migrator sau validare pe server**). Starea pe subfelii si dovezile sunt in [VERCON_STATUS](VERCON_STATUS.md); detaliul in `docs/worklog/SLICE-VC00-0N-*.md`. Livrabile: [MANIFEST_SURSE](MANIFEST_SURSE.md), [INVENTAR_OBIECTE](INVENTAR_OBIECTE.md), [DICTIONAR_DATE](DICTIONAR_DATE.md), [CONTRACT_CALCULE](CONTRACT_CALCULE.md), [MAPARE_ACCESS_WEB](MAPARE_ACCESS_WEB.md), [RAPOARTE](RAPOARTE.md), [SCHITE/INDEX](SCHITE/INDEX.md), [DECIZII_DESCHISE](DECIZII_DESCHISE.md).

Corectii ale planului rezultate din dovezi (detaliu in DECIZII_DESCHISE §B):

- Rapoartele `Balanta_produse_AFB`, `Balanta_Produse_BF`, `Balanta_produse_furnizor` depind tranzitiv de Coduri (model legacy) si sunt excluse (N06); enumerarea „variante BF/AFB" din VC08-03 nu mai se aplica.
- `Intrari.TVA` este suma TVA a liniei, nu procent; nu este autoritara (inconsistenta pe 114 linii Alimente).
- Numarul de zecimale este `NR.ZEC` (decizie utilizator, 11.10.2026; fallback 4 din cod). Cu `Z = NR.ZEC` totalurile incap in INT; propunerea `AjustareAntet` a fost **retrasa**. La OI, 13 valori au a treia zecimala (diferenta exact 0,005) si se rotunjesc la conversie cu inregistrare in proveniență.
- `Intrari.TVA` este suma TVA calculata (confirmat de utilizator).
- Gestiunea in Access este **folderul** `cale.Cale`, exercitiile sunt `tz_<an>.accdb`; subunitatea nu exista in Access. Tipul vine din `Setari.OBINV`/`MF`; Alimente vs Materiale nu este distins de niciun steag (alegere la migrare).
- Meniul SFP (`tmpOptiuniSFP`) si ribbonul au fost primite si sunt in `VERCON/REFERINTE/`; vizibilitatea butoanelor ribbon (`Rbb`) ramane necunoscuta.

## Decizii ale utilizatorului — 11.10.2026, runda 3 (inlocuiesc orice text contrar de mai sus)

Aceasta sectiune este autoritara. **Documentele `CONTRACT_CALCULE.md` si `DECIZII_DESCHISE.md` au fost aliniate la ea (11.10.2026).

**Reguli generale**
1. **Zecimale:** `NR.ZEC` este valabil **pe gestiune**. La Materiale/Alimente, cand fisierele anuale au `NR.ZEC` diferit, se ia cel din **anul cel mai recent**. Fallback 4 cand lipseste. Gestiuni diferite ale aceleiasi subunitati pot avea `NR.ZEC` diferit (nu mai este conflict).
2. **Stocare exacta, afisare cu `NR.ZEC`:** valorile sursa se pastreaza exact; se afiseaza rotunjite la `NR.ZEC`, **in sus la jumatate** (12,345 → 12,35). Prin urmare scara de stocare INT a unei gestiuni poate fi mai mare decat `NR.ZEC` (cea mai mica scara care reprezinta exact toate datele; pe datele reale: Alimente 4, MF 2, OI 3, caci 13 valori OI au a treia zecimala). Propunerea anterioara de a rotunji la migrare cele 13 valori si `AjustareAntet` sunt retrase.
3. **La migrare nu se face niciun calcul**, cu **singura exceptie** a antetelor NIR si BC: antet `0`, `NULL` sau cu diferenta **peste 1 leu** fata de suma liniilor → total = suma liniilor (N01/N02 ramane valabil integral); altfel antetul ramane neatins. Restul (TVA, preturi, valori de linie) se copiaza ca in sursa.
4. **OI si MF NU se inchid anual.** Inchiderea anuala exista numai pentru Materiale si Alimente.
5. **Furnizorii VERCON = Parteneri KBOT** (`Parteneri`, cheie `CodFiscal`, pe unitate): se leaga dupa CodFiscal; **furnizorii fara nicio inregistrare nu se migreaza**; verificarea ANAF din `Furnizori2022` si clasele `clsFirma*` nu se portează. Cazul unui furnizor cu NIR dar fara CodFiscal sau fara potrivire (acum: Alimente 1, MF 1): **alege operatorul in migrator** (leaga de un partener existent, creeaza unul nou sau lasa fara partener).
6. **«STOC INITIAL»** (si variantele: „Stoc Initial 2025", „S.I.", cu/fara diacritice) **nu este un furnizor real**: documentele lui se migreaza ca **document de tip «Stoc initial», fara partener** (acum: Alimente 2 NIR / 79 linii, MF 1 NIR / 154 linii); lista candidatilor o confirma operatorul in migrator.
7. **Export Excel:** fiecare raport are export `.xlsx` simplu, fara sabloanele `.tmpl` (nu se portează), cu aceleasi drepturi si date ca ecranul.
8. **Editare/anulare document salvat:** ca la ADECHIT — anulare cu motiv obligatoriu, documentul ramane in lista marcat „anulat"; editare numai cat timp perioada este deschisa; dreptul de anulare se da pe cont. **OI/MF:** nu au buton de inchidere, deci perioada este mereu deschisa. In selectia datelor (SFP) arborele an/luna contine **toti anii de la prima inregistrare (NIR) si de la prima predare (BP)**, cu **doar ultimul an expandat**.
9. **Numerotare:** automata, **unica pe gestiune, tip de document si an** (contor reia de la 1 in fiecare an); numar manual permis numai daca nu exista deja in aceeasi gestiune si an.
10. **Valorizarea stocului in balante/fise:** pretul variantei (`ProduseP.PUTVA`), **modificabil oricand**, ca in Access (o schimbare de pret modifica valoarea stocului si in trecut; liniile documentelor isi pastreaza valorile proprii).
11. **Stoc negativ interzis** (ca in Access): salvarea e refuzata cand cantitatea depaseste disponibilul la data documentului.
12. **Drepturi:** doar roluri pe DC (sectiunea VR); fara liste de acces pe subunitate/gestiune (deci `VC_SubunitAccess` si `VC_GestiuneAccess` nu se mai creeaza); accesul exclusiv ramane pe gestiune.

**Clarificari de pornire**
- Atribuirea fisierelor la subunitate/gestiune/tip/DC se face de operator in `VRC.Migrator`, la import (nu este intrebare de documentare). In Access gestiunea este folderul `cale.Cale`, exercitiile sunt `tz_<an>.accdb`, tipul vine din `Setari.OBINV`/`MF`; Alimente vs Materiale nu are steag si se alege la migrare.
- `Intrari.TVA` este suma TVA calculata (confirmat).
- Meniul SFP (`tmpOptiuniSFP`, 47 randuri) si `Rbb` (49 randuri; toate `V=1,E=1`, in afara tabului `Tab_MF` ascuns) au fost primite; ribbonul nu diferentiaza dupa tip prin date, diferentierea o fac `Form_Open` din BC/BR/ListaAlimente (refuza la OBINV sau MF), `OI_*` (refuza daca nici OBINV nici MF) si `IA`.
- Raman nedecise si nesolicitate acum: D12 (transfer intre contexte), rolul intrarilor cu `Cant=0` (pastrate), semnificatia `AnDate=2100`, eventuale baze `tz_aaaa` lipsa.

## Corecții UI și fluxuri ale utilizatorului — 12.10.2026

Sursa: imaginile adnotate din `MOCKUPURI/Corectii_mockup-uri_01` și mesajul ulterior cu cinci atașamente, **revizia 2**: NIR este unica referință vizuală de tabel și o listă poate avea mai multe BC-uri. Aceste clarificări înlocuiesc propunerile UI/flux contradictorii anterioare. Contractul detaliat este [FLUXURI_UI.md](FLUXURI_UI.md); livrabilul vizual este [setul corectat v2](MOCKUPURI/2026-10-12-corectate-v2/INDEX.md). Documentare/mockupuri; nu certifică implementarea aplicației. Cardinalitatea țintă 1:N necesită alinierea DDL/generatorului în VC03-02 înaintea VC07-01.

1. **Consultare/editor:** NIR, BC, ListaAlimente și BP/BT/BS folosesc în dreapta arbori de documente salvate. La Adaugă/Modifică arborele dispare; reapare după salvare confirmată sau Renunță. Frunzele au ⋯ la hover/focus. Butonul Adaugă de document există în toate familiile. Draftul rămâne în editor dacă salvarea eșuează sau rezultatul e necunoscut (VC02-02/03/04, VC06-05).
2. **NIR:** informațiile facturii (număr/dată/valoare) sunt pe aceeași coloană; numărul/data NIR pe o altă coloană. Valoare factură este activă și se introduce manual; necompletată → suma produselor. N01/N02 și pragul existent rămân valabile. Produsele se aleg prin combobox cu căutare la tastare. Coloanele MF sunt în aceeași grilă Produse recepționate, fără secțiune MF separată. Arbore NIR: furnizor → dată → număr factură + total (VC06-02).
3. **BC:** grilă DGV identică vizual cu NIR, cu aceeași densitate/editoare și Adaugă rând în footer; arbore de bonuri în dreapta și aceleași tranziții; bonul se poate crea separat în Alimente (VC06-03).
4. **Alimente:** rezumatele meselor, felurile și BC-urile asociate sunt DGV-uri identice vizual cu NIR; fără tabel Alimente necesare. O listă poate avea mai multe BC-uri existente, neasociate altei liste, numai din Alimente curentă. Adaugă rând din footer deschide popupul; un rând per BC, valoare pe rând și total general. Salvează lista scrie atomic lista/mese/set de legături și revalidează toate bonurile, fără consum repetat (VC07-01). Arbore Liste salvate în dreapta, formatat ca NIR/BC/BP, ascuns în editor.
5. **OI/MF documente:** grile identice vizual cu NIR, compacte, cu Adaugă rând și totaluri în footer; fără panourile laterale Perioada/Deținători și fără Efectul transferului/Istoric produs. În consultare, arborele tuturor BP/BT/BS ale familiei este în dreapta; în editor se ascunde. Predare folosește drept sursă conturile în care au intrat produsele. Transfer/casare oferă numai produse ale predătorului ales. Casare nu are secțiune Primitor (VC07-02/03).
6. **Rapoarte:** arborele multianual SFP cerut la 11.10.2026 rămâne; eliminarea panourilor de pe pagina OI nu schimbă această regulă. Criteriile browser sunt extinse în PROBE_BROWSER (VC02-05).

**Corectie la checkpointul anterior:** punctul despre `Z = NR.ZEC` pe subunitate, cu rotunjirea celor 13 valori la conversie, este inlocuit de regulile 1–3 de mai sus.
