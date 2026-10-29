# SLICE-VC06-04 — BR și FCS

Stare: **IMPLEMENTAT_LOCAL**. Doar sintaxa verificata; **nimic rulat**, nimic vazut pe ecran, nimic pe MariaDB. Cererea operatorului, 12.10.2026 («fa tot VC06»).

## Ce s-a facut
- **Retur BC (BR)** (`create_br/edit_br`): se alege un BC existent; produsele lui apar cu cantitatea de pe bon, ce s-a returnat deja si ce a ramas; se returneaza partial si de mai multe ori. Antet nou in `VC_Iesiri_H` (`Retur=1`, `IdRetur` = bonul sursa, primitor/categorie mostenite, numar `BX<n>` ca in Access), linii in `VC_IesiriRetur` la **pretul bonului**. Cantitate peste cea ramasa sau data inaintea bonului → refuz. Mareste stocul.
- **Storno recepție (FCS)** (`create_storno/edit_storno`): se alege un NIR; produsele cu cantitatea primita, deja stornata si ramasa; storno partial/multiplu. Antet nou in `VC_NIR` (`Storno=1`, `IdStorno` = NIR-ul sursa, furnizor mostenit, numarul stocat **negativ**, contor separat), linii in `VC_IntrariStorno` la **pretul din recepție**. Scade stocul de la data documentului storno (un storno ulterior nu schimba disponibilul unei date anterioare).
- Pretul nu poate fi negativ (vine din documentul sursa, care are pret ≥ 0); eroarea nu lasa document partial (o singura tranzactie).
- Pagina: alegerea sursei, apoi grila cu «De stornat / De returnat»; produsele fara cantitate nu intra in document.

## Fisiere
`PYTHON/routes/vercon/`: `documents.py`, `doc_routes.py`, `protocol.py`; `PYTHON/static/js/vercon/docs.js`, `app.js`.

## Presupuneri si neverificat
- Pretul de storno/retur este pretul **documentului sursa**, nu `ProduseP.PUTVA` actual (liniile isi pastreaza valorile proprii, Q9). Daca o recepție are doua linii ale aceleiasi variante la preturi diferite, se foloseste cel mai mic.
- Retururile/storno nu ating registrul OI/MF (VC07). Efectul unui BR/FCS pe MF/OI nu este definit (D04/D10 deschise): FCS este permis pe toate tipurile, BR doar pe Materiale/Alimente.
- Nimic rulat.

## Extensie 18.10.2026 — documente migrate fara sursa

Stare: **IMPLEMENTAT ÎN SURSE, NERULAT**. Cererea operatorului: implementarea predării din 18.10.2026, secțiunile 3/3b, absorbite în acest jurnal și în [README VRC](../../VERCON/VRC.Migrator/README.md#predarea-online-din-18102026--absorbită-prin-d001-04) prin D001-04.

### Ce s-a schimbat si de ce

- BR cu `IdRetur IS NULL`: indicator explicit in lista si antet; alegerea BC este disponibila in consultare doar pentru returul activ fara sursa. Actiunea dedicata valideaza fiecare varianta si suma cantitatilor sale fata de `return_eligible(..., exclude=doc_id)`. Refuzurile sunt in romana, la campul `source`, cu lista produselor incompatibile. Asocierea modifica numai `IdRetur` si `Version`, cu audit `br.link`, fara controlul cronologiei stocului. Modificarea obisnuita cere explicit asocierea prealabila.
- FCS: lista liniilor `VC_IntrariStorno.IdIntrare IS NULL`, inclusiv cele anulate, cu produs, cantitate, pret si data salvarii. Asocierea individuala cere o FCS neanulata din acelasi context, cu receptie sursa valida, si verifica disponibilul prin regulile storno existente. Cantitatile deja legate de FCS aleasa sunt incluse in consumul disponibilului. Se pastreaza cantitatea si pretul liniei migrate. Versiunile liniei si antetului cresc; se aplica `check_timeline` si audit `storno.link` in aceeasi tranzactie. Anularea separata a liniei orfane cere motiv si lasa linia vizibila.
- Rutele pastreaza drepturile BR/FCS, contextul rezolvat de server, tokenul blocarii si perioada deschisa. Cheia cererii permite reluarea aceleiasi operatii dupa pierderea raspunsului. UI blocheaza schimbarea contextului cat timp operatia este in curs sau rezultatul ramane neconfirmat.
- Ajutorul web explica ambele fluxuri si avertizeaza asupra coliziunilor identificatorilor OI migrati. Numerotarea OI nu a fost schimbata.

### Fisiere atinse

`PYTHON/routes/vercon/documents.py`, `doc_routes.py`, `migration_links.py` (nou). `PYTHON/static/js/vercon/docs.js`, `orphan-storno.js` (nou), `app.js` (ajutor). `PYTHON/static/css/vercon.css`, `vercon-migration.css` (nou). `implementare_vercon_v2.md`, `VERCON/VERCON_STATUS.md`, acest worklog.

### Rezultate si limite

Numai analiză de design a integrării cu protocolul/repository-ul; coloanele provin din dumpul local `MariaDB_Schema/AVACONT_SURSA.sql`. Fără teste, compilări, sintaxă, browser/HTTP, SQL, verificări automate, schemă modificată sau creată automat. Nedeployat/neacceptat vizual; modificări locale fără commit/push. Anularea BR nu cere sursă în codul existent, dar regulile de stoc se aplică și la anulare. Scenariile de mai jos, absorbite din consemnarea 18.10.2026, sunt toate NERULATE. Numerotarea OI/MF rămâne decizia operatorului.

## Detalii și criterii de acceptare absorbite în D001-04

BR: POST /api/vercon/doc/br/<id>/link; «Bonul returnat» / «Asociază cu un bon de consum» numai în consultarea returului activ fără sursă. Răspunsul întoarce documentul/versiunea actualizată. La storno: GET /api/vercon/doc/storno/orphans și POST /api/vercon/doc/storno/orphans/<id>/link|cancel; lista are și UM, preț cu TVA și stare. Anularea orfanei păstrează motiv/utilizator/dată/audit și nu introduce mișcare în stoc. Nu se fac asocieri sau anulări automate.

Criterii de acceptare pregatite, **TOATE NERULATE**:

| Scenariu | Rezultat asteptat |
| --- | --- |
| BR fara `IdRetur`, consultare | Marcaj Fără bon sursă in lista si antet; alegerea BC disponibila numai cu drept si document activ. |
| BR orfan, Modifica / PUT direct | Mesaj explicit de asociere prealabila, fara pierderea liniilor. |
| Asociere BR cu BC potrivit | `IdRetur` completat, versiune +1, audit `br.link`; liniile, valorile si stocul neschimbate. |
| BC fara produs sau cantitate ramasa insuficienta | Refuz la `source` cu toate produsele incompatibile; nicio modificare. Cantitatile mai multor linii ale aceleiasi variante sunt insumate. |
| BR dupa asociere, Modifica | Editorul normal foloseste BC asociat si cantitatile eligibile. |
| BR inainte/dupa asociere, Anuleaza | Fluxul obisnuit cu motiv si versiune, fara dependenta de existenta sursei; regulile existente de stoc raman aplicabile. |
| Anularea BC cu retur asociat activ | Blocata de `_guard_dependents` pana la anularea returului. |
| Lista storno fara receptie | Afiseaza liniile din context, cantitate, pret, data salvarii, stare; cele anulate raman vizibile si nu se pot lega. |
| Legare storno de FCS potrivita | Legatura si versiunile cresc; audit; linia incepe sa conteze in stoc numai dupa commit. |
| Storno: produs absent, depasire, sursa nevalida sau cronologie negativa | Refuz si rollback integral, inclusiv versiunea antetului. |
| Anularea unei linii storno orfane | Motiv obligatoriu, `Anulat=1`, versiune +1 si audit; ramane in lista, fara efect in stoc. |
| Context diferit, perioada inchisa, blocare lipsa, versiune veche | Refuz fara modificari. |
| Raspuns pierdut la asociere/anulare orfana | Reia aceeasi cerere cu aceeasi cheie, fara dublarea efectului sau auditului. |

La acel checkpoint, config.py, rapoartele, AGENTS și migratorul nu fuseseră modificate; fără modificări de subunități, IDM/IdMasinaP/IDBO, NumarSub ori CodFiscalSursa. Decizia OI/MF era încă deschisă atunci; confirmarea ulterioară și limitele remapării sunt în [REGISTRU_OI_MF](../../VERCON/REGISTRU_OI_MF.md).

## FCS în pagină și numerotare comună — 17.10.2026

Patru etape de surse, nerulate: fără teste, compilare, verificări automate, browser, SQL sau acces MariaDB/publicare. Python cere publicare/restart; schimbările numai JS/CSS cer publicarea lor. Acestea înlocuiesc editorul și contorul separat FCS descrise la 12.10; extensia pentru orfane de la 18.10 rămâne distinctă.

- Adăugare/modificare în spațiul normal al recepțiilor, antet cu patru coloane; Recepția stornată și Comisie ocupă câte două. ComboTree grupează NIR-urile eligibile la Data FCS pe furnizori, în gestiunea curentă; la facturi repartizate sunt numai recepția/liniile locale.
- DGV: Produs/UM/Cantitate/Preț/TVA %/Valoare/TVA/TOTAL, rând gol permanent, ComboTree și ștergere ca NIR. Sunt oferite numai variantele sursei cu stoc la Data FCS; preț/TVA din recepție. Stocarea rămâne Cant pozitiv/preț cu TVA în VC_IntrariStorno. Serverul limitează atât restul nestornat NIR, cât și stocul variantei la dată; FCS viitoare rezervă cantitatea sursei, documentul editat este exclus și cronologia integrală se validează la salvare.
- În editare, arborele este înlocuit de mini fișa furnizorului/produsului la Data FCS, autorizate FCS. Fără preview; Renunță stânga/Salvează FCS dreapta. În consultare, adăugare în antetul arborelui, fără + Adaugă de jos.
- NIR/FCS au numerotare anuală comună în gestiune, ABS(Numar)/NumarSub, cu anulatele ocupate. La inserare înaintea unui NIR/FCS ulterior se propune subnumăr liber, maximum două zecimale; data/unicitatea se revalidează sub lock tranzacțional. Și NIR ia FCS în calcul la numărul propus/limitele cronologice. FCS rămâne stocat cu număr negativ; NumarSub exista, fără schemă creată automat.
- Produsul FCS exclude combinațiile produs/preț cu TVA deja în draft la fiecare deschidere; ștergerea/schimbarea rândului le eliberează, alt preț rămâne disponibil. Comparație exactă la patru zecimale, fără binar; celula și serverul refuză duplicatul chiar cu alt ID de variantă.
- Listare fără acțiune este scoasă din NIR și meniul INTRARI. Ulterior, `docs.js/totals` se oprește la storno după totalurile DGV: fără comparație antet/linii sau mesaj de reconciliere, în consultare/adăugare/modificare; totalurile produselor și celelalte familii rămân.

### Cache-ul comun al mini fișelor

- NIR/FCS au cache numai în memoria instanței editorului, maximum 128 de fișe, evacuând cele folosite cel mai demult. Cheia include toți parametrii: context, dată, variantă ori produs/preț/TVA după familie. Fișa validă reapare imediat, cererile în curs sunt reutilizate, erorile nu se memorează; răspunsul după invalidare nu repopulează cache-ul. Fără localStorage/sessionStorage.
- Invalidare la schimbarea ciornei/documentului/contextului, ieșire din editare inclusiv salvare și distrugere; schimbarea datei/prețului/variantei are altă cheie. Fișele sunt informative; stocul decisiv se revalidează în server la salvare.
- Durata inițială fixă de două minute în JS este înlocuită de config.py la pornire: `VERCON_MINI_STOCK_CACHE_ENABLED=True`, `VERCON_MINI_STOCK_CACHE_SECONDS=120`. Activarea cere bool, durata întreg nenegativ; invalid = eroare explicită la pornire. False ori 0 dezactivează memorarea, deci revenirea pe rând recitește serverul; când e activ se păstrează limita/invalidările.
- lookups expune numai aceste două valori publice, nu restul configurației. Editorul le aplică la încărcare și schimbarea contextului. config.py nu a fost citit/modificat; valorile au fost furnizate în conversație. `PYTHON/config_sesiune.example.py` anterior este eliminat la cererea utilizatorului pentru a nu fi publicat accidental; nu se creează alt exemplu. Sunt necesare setările și restartul Python.

| Etapă | Fișiere (relative la grupul indicat) |
|---|---|
| Editor/stoc/numerotare | PYTHON/static/js/vercon/: docs.js, storno-editor.js, receipt-context.js; PYTHON/static/css/: vercon.css, vercon-storno.css; PYTHON/routes/vercon/: documents.py, storno_stock.py, doc_routes.py, receipt_context.py, receipt_number.py, receipt_rules.py |
| Fără duplicate/cache inițial/Listare | PYTHON/static/js/vercon/: app.js, docs.js, storno-editor.js, receipt-context.js, statement-cache.js; PYTHON/routes/vercon/documents.py |
| Cache configurabil | PYTHON/routes/vercon/: receipt_context.py, doc_routes.py; PYTHON/static/js/vercon/: docs.js, receipt-context.js, statement-cache.js; PYTHON/config_sesiune.example.py eliminat |
| Fără reconciliere FCS | PYTHON/static/js/vercon/docs.js |

Jurnalul general este evidența comună. Macheta raportului FCS și semnul afișării rămân în [VC08-03](SLICE-VC08-03-rapoarte-magazie-alimente.md#macheta-comună-fcs-apoi-semnul-de-afișare).
