# FC / NIR — validările Access și flux propus pentru web

Data: 16.10.2026. Analiză statică a surselor; fără rularea aplicației, teste, interogări sau modificări de baze de date. Nu s-a modificat codul aplicației.

Comportamentele descrise ca existente rezultă din sursa citită. Recomandările sunt propuneri și nu înlocuiesc automat deciziile aprobate din proiect. Completările din aceeași zi consemnează cerințele utilizatorului despre cronologia numerelor, TVA comun în antet, egalitatea exactă cu ValoareFC și factura repartizată între mai multe destinații. Preferința explicită pentru corecție este păstrarea cotei reale de TVA; varianta de 0% din Access era o simplificare intenționată.

## 1. Cum funcționa în Access

### 1.1. Încărcarea și intrarea în modificare

În `Forms/FC.txt`, `mcTree_Click` (4684) citește antetul selectat și încarcă liniile în `tmpFact`, prin `QFCTMP` sau `QFCTMPMF`. Se lucrează pe această copie temporară.

`btnModNIR_Click` (4159) cere selectarea unei facturi, activează editorul și dezactivează arborele. Nu verifică aici dacă există consumuri sau dacă poate fi redusă cantitatea. Restricțiile sunt aplicate ulterior în subformular.

`QFCP.sql` calculează pentru **varianta `IdProdus`**, nu pentru orice produs cu aceeași denumire:

| Simbol în formular | Ce reprezintă |
|---|---|
| `CIN` | Intrări din celelalte NIR-uri; NIR-ul editat este exclus explicit |
| `CIE` | Ieșiri |
| `CIR` | Retururi din ieșiri |
| `CIS` | Stornări de intrări |
| `CIT` | Cantitatea variantei din copia temporară a NIR-ului curent |
| `CTF` | `CIT + CIN + CIR - CIE - CIS` |

Aceste interogări nu filtrează mișcările după dată. Ele verifică un sold global în baza conectată, nu soldul la fiecare dată din istoric. Variantele pot fi comune mai multor NIR-uri; controlul nu demonstrează proveniența unui consum dintr-un anumit NIR.

### 1.2. Modificarea unui rând

În `Forms/TmpFact.txt:1276`, `Cant_BeforeUpdate` refuză modificarea când:

```text
Round(cantitate_nouă + CIN + CIR, ZEC) < CIE + CIS
```

Cu alte cuvinte, cantitatea nouă trebuie să acopere ieșirile și stornările pe care celelalte intrări și retururi nu le acoperă. La refuz setează `Cancel = True` și execută `Me.Undo`.

Exemplu conceptual: fără alte intrări sau retururi, la 60 bucăți ieșite și 10 stornate, cantitatea rândului nu poate coborî sub 70. Exemplul nu provine dintr-o rulare pe date.

După acceptare, `Cant_AfterUpdate` (1268) recalculează valoarea, TVA și valoarea cu TVA. `TmpFactMF.txt:1543` repetă aceeași verificare de cantitate.

Limitări ale implementării:

- Formula folosește cantitatea nouă a rândului și intrările din celelalte NIR-uri; nu adaugă celelalte rânduri ale aceleiași variante din NIR-ul curent. Poate bloca excesiv dacă varianta apare pe mai multe rânduri.
- Procedura nu conține separat condiția `cantitate_nouă > 0`.
- Cantitățile din formular sunt încărcate prin `Form_Current`, deci validarea depinde de starea UI, nu de o recitire atomică a datelor la salvare.
- Modificarea prețului în `Pret_AfterUpdate` (1323; MF: 1659) este respinsă dacă `CotaTVA <> 0`. La TVA zero recalculează valorile. Nu este o regulă generală de recalculare a oricărui preț cu orice cotă TVA.
- La MF există verificări suplimentare: număr de inventar folosit de alt activ, data intrării și DNF în intervalul clasificării. Comparația datei folosește `Parent!Data`, adică data facturii din FC, deși mesajul spune „data NIR-ului”.

### 1.3. Ștergerea unui rând

În `Forms/TmpFact.txt:1237`, `btnDelRow_Click` refuză când:

```text
Round(CTF, ZEC) < Round(cantitate_rând, ZEC)
```

Interpretarea urmărită este:

```text
stoc_după_scoaterea_rândului = CTF - cantitate_rând >= 0
```

Dacă este permisă, cere confirmare, copiază produsul/cantitatea/prețul/TVA în câmpurile de introducere ale FC și șterge rândul din `tmpFact`. Liniile persistente din `Intrari` sunt schimbate ulterior, la salvarea NIR-ului. În formularul FCS comanda aceasta este oprită.

`Forms/TmpFactMF.txt:1504` are aceeași logică.

Există și o problemă în calculul lui `CIT`: ramura temporară a `QFCP.sql:33` grupează după variantă, cantitate și produs, fără să însumeze fiecare rând distinct. Două rânduri identice pot fi comprimate într-un singur grup, iar soldul folosit de UI poate fi subevaluat. Aceeași structură apare în `QFCDEL`.

### 1.4. Ștergerea întregului NIR

Ruta butonului este `Forms/FC.txt:4112`, `btnStergNIR_Click`:

1. Cere selectarea unei facturi, nu a furnizorului.
2. Rulează `QFCDEL`.
3. Dacă interogarea întoarce un produs cu sold negativ, oprește ștergerea.
4. Cere confirmare; dacă există NIR-uri cu `IdStorno = IdIntrare`, avertizează că vor fi șterse și acestea.
5. Șterge antetele stornărilor asociate și apoi antetul NIR-ului.

Formula efectivă din `QFCDEL.sql:4` și `:39` este:

```text
intrări_totale + retururi - cantități_tmpFact - ieșiri < 0
```

**Deși interogarea calculează `CS`, nu îl scade în formula soldului.** Omiterea stornărilor asociate NIR-ului poate corespunde intenției de a le elimina împreună cu acesta. Problema este că interogarea nu separă acele stornări de stornările altor NIR-uri ale aceleiași variante, care rămân valabile. În acest caz poate permite o ștergere care lasă stoc negativ.

Ștergerea se bazează pe copia `tmpFact`, nu pe o selecție directă a tuturor liniilor persistente ale NIR-ului. Cele două ștergeri de antete nu sunt încadrate aici într-o tranzacție explicită. Codul butonului nu șterge explicit liniile; efectul complet depinde de relațiile bazei Access.

Mai există `StergereNIR()` la 4146, o funcție separată care șterge din `Intrari`, `NIR` și vechiul `IntrariIesiri`, fără controlul de stoc de mai sus. Butonul analizat nu apelează această funcție; nu trebuie confundate cele două implementări.

### 1.5. Salvarea

`VerificariSalvare` (`FC.txt:4777`) cere cont/categorie, comisie, furnizor, număr/data facturii și număr/data NIR. Verificarea care anunță lipsa valorii facturii testează din greșeală încă o dată `DataNIR`.

Pentru non-MF, diferența dintre valoarea declarată și suma liniilor poate fi acceptată prin confirmare. Nu există aici o reluare completă a validărilor de stoc pe toate liniile.

`btnSavNIR_Click` (4229) deschide tranzacția, rezolvă variantele noi, actualizează antetul, înlocuiește liniile din `Intrari` cu cele selectate (`S=True`) și actualizează datele MF. La editare, ramura de antet nu persistă `DataFc` și `Data`, deși `DataNIR` este activată în UI. După commit mai execută actualizări/curățări de variante MF în afara tranzacției.

## 2. Ce există acum în web

| Zonă | Comportament observat în sursă |
|---|---|
| Copie de lucru | `docs.js:114` creează ciorna documentului; Renunță o abandonează |
| Scoatere rând | `docs.js:305` elimină rândul din ciornă; nu face atunci o simulare de stoc |
| Salvare modificări | `documents.py:560` aplică noua structură și cheamă `protocol.check_timeline` pentru variantele vechi și noi, în tranzacția rutei |
| Anulare NIR | `documents.py:616` marchează documentul anulat, verifică stocul și scrie audit; un refuz produce rollback |
| Cronologie | `protocol.py:142` refuză stocul negativ la oricare dată, cu intrările înaintea ieșirilor din aceeași zi |
| Storno activ | `_guard_dependents` blochează modificarea și anularea NIR-ului până la anularea stornării |
| Registru OI/MF | Același control blochează documentul dacă variantele lui apar în predări/transferuri/casări |
| Perioadă și revizie | Ruta cere perioadă deschisă; documentul este citit cu blocare și comparat cu revizia trimisă |
| Produs/preț | La non-MF, `receipt_choices.price_variant` reutilizează sau creează o variantă la prețul calculat; nu schimbă prețul tuturor documentelor existente |

Controlul cronologic din web este mai puternic decât soldul global din Access. De exemplu, o intrare din februarie nu trebuie să permită eliminarea unei intrări din ianuarie dacă ar rămâne un consum neacoperit în ianuarie.

Aspecte de rezolvat în proiectare/implementare:

- Utilizatorul află despre unele blocaje abia la salvare. Fișa produsului din `receipt_context.py` citește mișcări salvate; nu simulează efectul întregii ciorne.
- Interogările pentru dependențele OI/MF din `documents.py:340` nu filtrează `Anulat=0`. Simplul fapt că un document dependent rămâne în istoric după anulare poate continua să blocheze NIR-ul.
- `_clean_nir` recalculează toate liniile trimise și cere cantități pozitive. O editare doar de antet a unui document migrat cu valori istorice speciale poate astfel să modifice valorile sau să fie refuzată. Decizia din 14.10.2026 privind păstrarea exactă a valorilor migrate trebuie respectată explicit.
- Pentru MF, validarea NIR din server nu reproduce toate controalele VBA pentru unicitatea numărului de inventar, data intrării și intervalul DNF din clasificare. Cantitatea este cerută întreagă, dar nu este limitată la 1.
- La editarea unui MF existent, `_write_receipt_line` păstrează identitatea variantei și nu îi actualizează produsul de bază, deși formularul trimite un produs editabil. Identitatea activului trebuie tratată explicit.
- Sursa executabilă `stock_source.py:9` folosește `COALESCE(DataFc,Data)` atât pentru recepții, cât și pentru stornări. Comentariile/contractul nu sunt complet aliniate pentru data storno. Regula datei trebuie unificată între validare, previzualizare și rapoarte.

## 3. Flux propus

### 3.1. Principiul comun

Pentru modificare, eliminare de rând și anulare, folosim același motor care simulează documentul final:

```text
mișcări existente fără efectul vechi al NIR-ului
+ efectul tuturor liniilor din ciornă, la datele propuse
= stoc rezultat pentru fiecare variantă și fiecare dată afectată
```

Operația este permisă numai dacă stocul rezultat nu este negativ și sunt respectate cronologia număr–dată, dependențele și regulile de valori descrise mai jos. Anularea înseamnă că noul efect al NIR-ului este zero. Schimbarea produsului/prețului poate muta cantitatea între variante: se verifică atât varianta veche, cât și cea nouă.

Se însumează toate liniile distincte ale aceleiași variante; nu se elimină implicit rândurile identice și nu se verifică numai rândul activ. Calculul folosește valorile zecimale și precizia gestiunii, fără conversii prin numere binare sau toleranțe arbitrare.

### 3.2. Consultare și intrare în modificare

1. Operatorul selectează NIR-ul și vede documentul, starea, totalul și istoricul.
2. La Modifică, serverul comunică eligibilitatea și motivele blocajelor: drept, perioadă, anulare existentă, storno activ, documente OI/MF dependente.
3. Dacă este eligibil, se creează ciorna. NIR-ul salvat rămâne intact până la salvarea confirmată.
4. Câmpurile de identificare a contextului nu pot muta documentul într-o altă subunitate/gestiune. Schimbarea contului este refuzată când liniile nu îi aparțin; liniile nu sunt reinterpretate automat.

### 3.3. Adăugare și modificare de rând

1. Produsul aparține contului ales, cantitatea este pozitivă, prețul este nenegativ, cota TVA există și valorile respectă precizia permisă.
2. La părăsirea rândului se simulează întreaga ciornă. Pentru cantitate, produs, preț/TVA care schimbă varianta și datele cu efect asupra stocului, se reevaluează variantele afectate.
3. O operație invalidă nu se validează în grilă; se păstrează ultima stare validă și valoarea propusă poate fi corectată.
4. Mesajul trebuie să fie concret: „Nu poți reduce la 50 buc. Pe 10.01.2026 ar lipsi 10 buc. Cantitatea minimă pentru această modificare este 60 buc.” Limita se calculează pentru ciorna și datele curente, nu se memorează definitiv.
5. Valorile calculate se actualizează pentru liniile economice modificate. Valorile istorice ale liniilor nemodificate se păstrează exact, inclusiv la o editare exclusiv de antet.

Pentru MF propun o identitate de activ pe rând și cantitate 1 pentru intrări noi. Numărul de inventar este unic în gestiune și în ciornă; DNF respectă clasificarea, iar data intrării se validează după regula explicit stabilită. Schimbarea identității unui activ existent se blochează; înlocuirea lui se tratează ca scoatere a activului vechi și adăugare a unuia nou, cu verificarea dependențelor. Aceasta este o propunere de detaliere, nu o regulă deja implementată.

### 3.4. Scoaterea unui rând

| Situație | Acțiune propusă |
|---|---|
| Rând nou, nesalvat | Se scoate din ciornă; nu afectează baza |
| Rând existent, efectul eliminării este valid | Confirmare, apoi scoatere din ciornă; persistare numai la Salvează |
| Eliminarea lasă stoc negativ la o dată | Refuz cu produs, dată și cantitate lipsă |
| Activ cu dependențe valabile | Refuz și listarea documentelor dependente |

Renunță restabilește documentul salvat. Un rând eliminat din ciornă nu se șterge imediat din baza de date.

### 3.5. Salvarea NIR-ului

1. Serverul revalidează contextul, dreptul, ocuparea gestiunii, perioada, revizia și dependențele active.
2. Validează antetul și toate liniile; nu acceptă document fără linii. Numărul/subnumărul trebuie să fie liber, excluzând documentul curent din controlul de duplicat.
3. Calculează numai valorile economice care trebuie schimbate. Pentru documentele noi finalizate, noua cerință este egalitatea exactă dintre suma cu TVA a liniilor și valoarea declarată aferentă recepției. Nu este suficient să înlocuiască numai totalul din antet. La factura cu mai multe NIR-uri, ValoareFC este ținta întregii facturi, iar fiecare NIR are propria valoare repartizată. Detaliile sunt în completările de mai jos; migrarea păstrează valorile istorice fără recalculare.
4. În aceeași tranzacție scrie variantele, liniile, antetul, datele MF și auditul, apoi verifică din nou cronologia stocului pentru toate variantele afectate.
5. La refuz face rollback și păstrează ciorna pentru corectare. La succes revine în consultare și selectează NIR-ul salvat.
6. Dacă se pierde răspunsul, reconciliază operația înainte să declare eșec sau să permită repetarea cu efect dublu. Protecția trebuie definită și pentru editare/anulare, nu doar pentru creare.

### 3.6. Anularea întregului NIR

Păstrăm decizia proiectului: documentul salvat se **anulează cu motiv**, rămâne în istoric și nu participă la stoc. Nu preluăm ștergerea fizică și ștergerea automată a stornărilor din Access.

1. Se cere motivul anulării.
2. Dacă există storno activ, operatorul primește lista lui și rezolvă explicit acea dependență înainte de anularea NIR-ului.
3. Dacă există documente OI/MF dependente valabile, acestea blochează operația. Documentele anulate nu trebuie tratate ca dependențe active numai pentru că rândurile lor au rămas în istoric.
4. Se simulează eliminarea efectului întregului NIR și se verifică toate variantele la toate datele afectate.
5. Dacă este permisă, anularea și auditul se salvează atomic. Dacă nu, documentul rămâne activ și utilizatorul primește cauza exactă.

### 3.7. Data efectului asupra stocului

În implementarea actuală intrările folosesc data facturii (`DataFc`). Propun păstrarea acestei reguli în această etapă și afișarea ei clară în simulare. Pentru storno trebuie aliniate sursa executabilă și contractul: alegerea datei nu se schimbă tacit în cadrul refacerii validărilor.

## 4. Ordinea inițială recomandată a implementării

1. Motor comun de simulare a ciornei, bazat pe aceeași sursă de mișcări ca validarea finală.
2. Validare la modificarea/scoaterea rândului și mesaje cu produs, dată, lipsă și dependențe.
3. Păstrarea valorilor nemodificate din documentele migrate și corectarea controalelor MF/dependențelor anulate.
4. Alinierea datei stornării, a previzualizării și a rapoartelor cu regula stabilită.
5. Reconcilierea rezultatului necunoscut pentru creare, editare și anulare.

## 5. Cerință: data NIR păstrează cronologia numerelor

Data documentului și stocul sunt două controale distincte: chiar dacă mutarea datei nu produce stoc negativ, ea se refuză dacă inversează ordinea numerelor NIR.

Propunere de regulă pentru seria de numerotare a gestiunii, tipului și anului:

```text
max(data tuturor NIR-urilor cu număr mai mic) <= data propusă
data propusă <= min(data tuturor NIR-urilor cu număr mai mare)
```

Numărul se compară ca pereche `(Numar, NumarSub)`, nu ca text sau număr binar cu virgulă. Mai multe numere pot avea aceeași dată. Dacă o limită nu există, nu impune o restricție pe acea parte. Pentru date istorice deja ordonate sunt suficiente documentele vecine; folosirea maximului/minimului tuturor documentelor relevante tratează și inversiunile migrate.

- Numărul și subnumărul curent sunt excluse prin identitatea documentului din căutare. Dacă se editează și numărul, se validează perechea finală număr–dată.
- Nu se schimbă automat numărul pentru a accepta data nouă și nu se renumerotează documentele din jur.
- Se propune păstrarea în control și a pozițiilor documentelor anulate, care rămân în registru. Stocurile inițiale cu numere istorice speciale se tratează separat de seria normală.
- Se verifică la părăsirea datei și din nou la salvare. Exemplu de mesaj: „NIR 102 trebuie datat între 10.01.2026 (NIR 101) și 15.01.2026 (NIR 103).”
- Traversarea în alt an/altă serie se refuză ca simplă modificare de dată. La MF/OI, anul seriei este anul calendaristic al documentului, chiar dacă istoricul este într-un singur exercițiu multianual; codul actual bazat pe exercițiu trebuie adaptat pentru această regulă.
- Inversiunile istorice se raportează și se păstrează la migrare. O modificare exclusiv de comisie sau alt câmp fără efect asupra cronologiei nu trebuie blocată pentru o anomalie istorică neschimbată.

## 6. Cerință: TVA în antet numai când toate rândurile au aceeași cotă

În sursa actuală, `docs.js:193` creează selectorul la orice editare, iar `syncVatLock` (323) îl dezactivează când există produse. Acest comportament nu îndeplinește noua cerință.

La modificare:

| Liniile reale din ciornă | Selector TVA în antet |
|---|---|
| Toate au aceeași cotă reală, inclusiv 0% | Vizibil, completat cu acea cotă și activ |
| Există cote diferite | Ascuns |
| Există cotă lipsă/neidentificabilă | Ascuns până la completarea liniilor |
| Există numai rândul nou gol | Nu se deduce o cotă comună din el |

Se ignoră rândurile goale de introducere. Comparația folosește valoarea efectivă a cotei, nu eticheta sau coincidența numerică a unor ID-uri din gestiuni diferite.

Schimbarea cotei din antet aplică noua cotă tuturor rândurilor reale ale NIR-ului, recalculează valorile și simulează efectul asupra variantelor/stocului. Întreaga schimbare reușește sau se revine la starea precedentă; nu se acceptă propagare parțială. Modificarea TVA pe un rând poate face selectorul să dispară; revenirea tuturor rândurilor la aceeași cotă îl poate readuce după validarea rândului.

La adăugare se poate păstra cota de antet ca valoare inițială pentru rânduri noi. La modificare ea este derivată din rândurile existente, nu dintr-un câmp gol al ciornei. Serverul recalculează și validează toate rândurile afectate.

Dacă documentul avea corecție, nu se adaugă încă o dată diferența veche peste noile calcule: se reface previzualizarea corecției din valorile de bază și aceeași țintă declarată. În modelul facturii comune, acest selector rămâne local NIR-ului; cote uniforme într-un NIR nu implică o cotă uniformă în toate recepțiile facturii.

## 7. Corecția exactă la ValoareFC

### 7.1. Mecanismul Access găsit

`Forms/TmpFact.txt:1200` și `TmpFactMF.txt:1467`, `Ajustare_Click`, permit comanda la introducere, o opresc la modificare, cer o valoare declarată nenulă și refuză diferența absolută peste 1 leu. După confirmare apelează `Modules/Utile.bas.txt:485`, `CorectieFactura`.

Rutina caută un rând cu `Cant > 1`, scade o bucată din rândul original și adaugă o bucată pe un rând cu preț cu TVA ajustat. Pe rândul nou setează `IDTVA = 3`, `CotaTVA = 0`, `TVA = 0` și prețul net egal cu prețul cu TVA. Acest tratament a fost ales intenționat de utilizator pentru a simplifica ajustarea. ID-ul 3 este particular bazei și nu trebuie interpretat universal ca 0% în web.

Codul actualizează cantitatea și unele valori ale rândului original, fără recalcularea explicită a TVA-ului acestuia în aceeași comandă. Diferența este citită prin totalul formularului în timpul operației, iar rutina nu are o tranzacție explicită proprie. Aceste detalii nu constituie un algoritm sigur de copiat literal; analiza nu confirmă prin execuție rezultatul lui în Access.

În web, `numeric.header_total` poate păstra valoarea antetului diferită de suma liniilor. Aceasta nu este corecție efectivă și nu satisface noua cerință de egalitate exactă. Pentru documentele noi finalizate, noua cerință înlocuiește comportamentul de a accepta pur și simplu această diferență. Importul istoric rămâne fără recalculare.

### 7.2. Varianta propusă: păstrăm cota reală prin calcul invers

Se poate păstra cota reală folosind valoarea cu TVA corectată ca punct de pornire. Pentru rândul/fragmentul ales pentru ajustare:

```text
delta = ținta declarată - totalul calculat înaintea ajustării
G = valoarea cu TVA inițială a fragmentului + delta
B = rotunjire(G / (1 + cota reală), precizia valorilor)
TVA = G - B
```

Astfel `B + TVA = G` exact. Rândul are un mod de calcul explicit „din valoarea cu TVA”; o salvare ulterioară nu reconstruiește G cu formula obișnuită dintr-un preț net deja rotunjit. La valori mici, cele două sensuri de calcul nu sunt întotdeauna echivalente după rotunjire. Păstrarea cotei reale este posibilă în acest mod, dar nu prin forțarea simultană a tuturor egalităților de preț/valoare din calculul înainte, la o precizie fixă.

Exemplu cu valori la două zecimale: 10 bucăți × 10 lei net, TVA 21%, total 121,00 lei; ValoareFC = 120,99 lei. Se păstrează 9 bucăți la 12,10 lei cu TVA și se ajustează o bucată la 12,09 lei cu TVA. Pentru aceasta: bază 9,99 lei și TVA 2,10 lei, cu cota 21% păstrată. Totalul este 108,90 + 12,09 = 120,99 lei.

Reguli de aplicare propuse:

1. Diferența se calculează o singură dată pe un instantaneu stabil al întregii ciorne, în Decimal, la precizia monetară declarată.
2. Operatorul vede diferența și rândul ales înainte să aplice corecția. Pentru o cantitate de cel puțin 1 se poate izola o bucată, ca în Access; dacă rândul are exact o bucată, se corectează acea bucată. Pentru cantități subunitare se verifică reprezentabilitatea la precizia permisă înainte de alegerea fragmentului.
3. Se păstrează produsul, cota reală și cantitatea totală. Variantele de preț necesare se rezolvă la salvare, fără modificarea prețului unei variante comune altor documente.
4. Prețul/valoarea rezultatului nu poate deveni negativ. Mutarea cantității între variante este supusă verificării de stoc, inclusiv la modificarea unui NIR deja consumat.
5. Pentru MF se păstrează identitatea activului, cantitatea 1 și se recalculează valoarea de intrare/amortizarea în aceleași limite de dependențe.
6. La cote mixte se ajustează un rând ales explicit; nu se schimbă cotele tuturor liniilor pentru a obține totalul. Dacă factura furnizează și baze/TVA pe cote, se verifică separat acele ținte; numai totalul cu TVA nu determină unic repartiția între cote.
7. Modificarea produsului, cantității, cotei sau valorii declarate invalidează vechea ajustare și generează o nouă previzualizare; corecțiile nu se acumulează la fiecare salvare.
8. Se păstrează în audit valorile inițiale, diferența, rândul/fragmentul, modul de calcul și valorile finale. Corecția trebuie reflectată în costul recepționat, în document și în rapoarte; nu poate fi o diferență ascunsă numai în antet.

Propun păstrarea plafonului de 1 leu pentru comanda automată de ajustare, ca protecție împotriva lipsei unor rânduri sau a unei repartizări incomplete. Peste plafon se corectează datele/repartizarea, fără înlocuirea automată a ValoareFC. Plafonul nu este o toleranță la finalizare: după corecție diferența trebuie să fie exact zero. Dacă ValoareFC nu este încă introdusă, documentul poate rămâne în lucru cu un total calculat; finalizarea față de factura furnizorului cere o țintă explicită. Zero introdus explicit și valoare lipsă sunt situații diferite.

Preferința utilizatorului este păstrarea cotei reale. Varianta 0% rămâne o opțiune explicită de compatibilitate, dacă se alege păstrarea strictă a vechiului model de calcul. Nu se folosește automat drept soluție pentru lipsa preciziei sau pentru consumuri care blochează mutarea între variante: acestea sunt probleme independente de cotă. La migrare, rândurile deja ajustate la 0% se păstrează astfel; cota lor originală nu se poate deduce sigur.

## 8. Factură comună, mai multe NIR-uri locale

### 8.1. Model recomandat

Separăm identitatea facturii furnizorului de documentele care introduc efectiv bunurile în fiecare destinație:

```text
Factură de cumpărare, comună în DC
  ├─ linii de factură și total declarat unic ValoareFC
  └─ repartizări ale liniilor/cantităților/valorilor
       ├─ NIR subunitatea A / Alimente / cont 1
       ├─ NIR subunitatea A / Materiale / cont 2
       ├─ NIR subunitatea B / Obiecte de inventar / cont 3
       └─ NIR subunitatea B / MF / cont 4
```

În ierarhia actuală, Alimente/Materiale/OI/MF sunt tipuri de gestiuni dintr-o subunitate. Modelul acoperă atât mai multe asemenea gestiuni în aceeași subunitate, cât și subunități distincte din același DC. Nu presupune operații între baze DC diferite.

Propun păstrarea unui NIR pe combinația subunitate + gestiune + cont/categorie, legat de factura comună. O factură poate avea mai multe asemenea NIR-uri și recepții în zile diferite. Fiecare NIR păstrează număr, dată, comisie, stare, revizie și drepturile contextului lui. Stocul și registrul MF/OI rămân locale. Modelul nu obligă la rescrierea tuturor bonurilor și rapoartelor într-un singur document cu contexte amestecate.

Entități conceptuale noi, nu DDL executat:

| Entitate | Rol |
|---|---|
| `VC_FACTURI` | Furnizor, număr original, dată, monedă, ValoareFC, stare și revizie; identitate comună în DC |
| `VC_FACTURI_LINII` | Liniile economice ale facturii, fără a impune un singur cont sau produs local |
| `VC_FACTURI_REPARTIZARI` | Cantitate/valoare de pe o linie de factură alocată unei destinații și legătura cu linia NIR rezultată |
| Legătură nouă din `VC_NIR` | Referință la factura comună; antetele și liniile istorice își păstrează identitățile |

O linie de factură se poate repartiza în mai multe destinații și se poate recepționa parțial; nu se cere o corespondență 1:1 între linia facturii și linia NIR. Aceeași linie poate produce mai multe active MF. O legătură activă nu poate dubla aceeași cantitate recepționată.

### 8.2. Valorile și corecția

```text
total cu TVA al liniilor unui NIR = valoarea repartizată acelui NIR
suma valorilor repartizate și recepționate, la finalizarea completă = ValoareFC
```

ValoareFC se introduce o singură dată pe factura comună. În NIR se afișează distinct totalul întregii facturi și „Valoare aferentă acestui NIR”. Nu se compară fiecare NIR cu întreaga factură și nu se copiază integral ValoareFC pe toate recepțiile.

Exemplu: factura este 1.000 lei; recepțiile repartizate sunt 200 lei Alimente, 300 lei Materiale și 500 lei MF. Totalurile NIR sunt 200, 300, 500, iar factura este numărată o singură dată cu 1.000 lei.

Factura are stări de repartizare/recepție distincte: în lucru, parțial repartizată/recepționată și complet reconciliată. O recepție parțială validă nu obligă la corectarea artificială a diferenței față de factura întreagă. Numai la finalizarea completă se impune egalitatea globală. Anularea unui NIR păstrează factura, scoate cantitatea/valoarea lui din recepțiile active și readuce factura la starea corespunzătoare.

Corecția finală se calculează la nivel de factură și se repartizează o singură dată pe rânduri/destinații explicite. NIR-urile locale preiau valorile corectate rezultate; fiecare nu încearcă separat să absoarbă întreaga diferență a facturii. Nu se corectează factura cât timp diferența reprezintă bunuri nerepartizate sau nerecepționate.

Se folosesc valori zecimale exacte și o precizie monetară comună explicită. Gestiuni cu ZEC diferit nu justifică pierderea resturilor la însumare. Repartizarea trebuie să fie reprezentabilă în destinațiile alese; dacă nu este, se explică problema înainte de finalizare. Afișarea la două zecimale nu este o probă că valorile stocate la patru zecimale sunt egale.

### 8.3. Ecranul și salvarea

1. „Factură de cumpărare”: furnizor, număr, dată, valoare declarată și produsele din documentul furnizorului.
2. Pentru fiecare linie se aleg subunitatea, gestiunea și contul, cu cantitatea repartizată. Produsele locale se aleg/mapază în destinație; ID-urile lor nu sunt interschimbabile între gestiuni.
3. Pagina arată NIR-urile rezultate, cu număr/data și totalul fiecăruia, sumele repartizate/recepționate și restul facturii.
4. Pentru cazul simplu, cu o singură destinație, ecranul poate păstra experiența actuală de introducere directă a NIR-ului și creează legătura la factură în spate.
5. Recepțiile pot fi salvate local în etape. O operație care generează sau modifică simultan mai multe NIR-uri cere ocuparea tuturor gestiunilor afectate, în ordine stabilă, plus blocarea/revizia facturii. Lipsa accesului într-o destinație refuză întregul lot solicitat, fără salvare parțială tacită.
6. În aceeași bază DC, lotul simultan se salvează într-o singură tranzacție. Salvările locale succesive verifică și blochează factura comună pentru a preveni supra-alocarea prin două sesiuni.
7. Schimbarea datei/furnizorului/totalului comun după recepții cere analiza tuturor destinațiilor afectate. Modificarea datei facturii poate schimba și data de efect asupra stocului în contractul actual; nu se propagă tacit în perioade închise sau în NIR-uri cu consumuri incompatibile.

O factură storno/credit se păstrează ca document furnizor distinct, legat de factura inițială și de stornările locale. Stocul inițial nu primește o factură furnizor fictivă.

## 9. Migrarea din Access în acest model

### 9.1. Ce se păstrează

Fiecare NIR Access se importă în continuare ca un NIR local, cu același cont atribuit, număr/subnumăr, date, valori, variante, MF, stornări și proveniență. Nu se schimbă cantități, cote sau prețuri pentru a uniformiza modelul nou. Valorile migrate neconcordante rămân neconcordante, însoțite de diagnostic.

Se reutilizează proveniența existentă `VC_ImportSources` și `VC_IdMap`: identitatea documentului sursă include sursa/importul și cheia NIR, nu numai `IdIntrare`, care poate fi repetat în alte fișiere/subunități. Asocierea la factura comună nu renumerotează NIR-urile și nu reimportă datele.

### 9.2. Ce nu poate fi dedus sigur

Access nu are o identitate globală de factură care să lege recepțiile din fișiere/gestiuni/subunități diferite. Coincidența furnizorului + numărului + datei este un candidat bun, dar nu dovedește identitatea documentului, completitudinea recepției sau dacă antetul local conține totalul întregii facturi ori numai al părții recepționate.

De aceea propun:

1. Pentru fiecare NIR obișnuit, se creează inițial o asociere provizorie de factură sau un fragment de factură din migrare. Totalul sursă este păstrat și etichetat ca valoare declarată a fragmentului; nu este afirmat automat ca total global confirmat.
2. Se caută grupuri candidate pe furnizorul fiscal mapat, numărul original/normalizat conservator, data și moneda cunoscută. Datele lipsă sau ambigue nu se completează prin presupuneri. Numărul NIR și totalul egal nu sunt chei de identificare a facturii furnizorului.
3. Operatorul confirmă reunirea fragmentelor într-o factură comună și totalul global din documentul furnizorului. Gruparea se poate face după import, când există și celelalte subunități, fără repetarea migrării.
4. Nu se însumează automat antetele locale pentru a inventa ValoareFC: suma poate dubla un total de factură repetat în mai multe baze. Se afișează separat antetele sursă și sumele liniilor ca dovezi pentru reconciliere.
5. Liniile NIR importate rămân fapte de recepție. Liniile originale ale facturii, care nu există separat în Access, se marchează reconstruite din fragmente; nu se afirmă că numărul sau ordinea lor coincide cu factura furnizorului.
6. Pentru istoricul nereconciliat, rapoartele separă recepțiile certe de totalurile facturilor globale încă neconfirmate. O sumă provizorie nu se prezintă drept total sigur al facturilor.
7. Stocurile inițiale rămân fără factură. Legăturile locale dintre NIR și stornări se păstrează prin mapările existente; reunirea facturilor nu le schimbă.

În `VrcPartition.vb:83`, documentul este astăzi plasat după contul antetului, iar liniile din altă parte Alimente/Materiale sunt conflict de import. Modelul facturii comune permite reunirea NIR-urilor distincte, fără a transforma acest conflict în repartizare automată. Dacă se întâlnește un document istoric contradictoriu, se diagnostichează explicit; nu se împarte tacit și nu i se inventează alte numere.

### 9.3. Tranziție pentru date deja migrate

Asocierea factură–NIR se adaugă printr-o completare separată și reluabilă pe identitățile deja importate, nu prin reimportul subunității. Se păstrează markerul importului unic. Reunirea provizoriilor schimbă numai asocierile și metadatele de reconciliere, cu audit; nu rescrie mișcările sau valorile istorice.

## 10. Ordinea revizuită a lucrărilor

1. Stabilirea modelului factură comună → repartizări → NIR-uri locale și a sensului distinct ValoareFC / valoare NIR.
2. Pregătirea fișierelor DDL și a adaptării migratorului, inclusiv completarea pentru importuri deja efectuate; DDL se execută exclusiv de utilizator, fără creare automată de schemă în aplicație.
3. Cronologia număr–dată și selectorul TVA derivat din rânduri.
4. Corecția din valoarea cu TVA, păstrând cota reală și modul de calcul, plus reflectarea valorilor finale în variante, MF și rapoarte.
5. Simularea ciornei și validarea atomică a recepțiilor afectate, inclusiv ocuparea mai multor contexte.
6. Migrarea conservatoare, reconcilierea explicită a fragmentelor și fluxul de rezultat necunoscut.

Documentul consemnează analiza și propunerea. În această rundă s-a modificat numai acest document; nu s-au implementat reguli în aplicație, nu s-a pregătit/aplicat DDL și nu s-au rulat teste sau interogări pe baze de date.
