# FC / NIR — validările Access și flux propus pentru web

**Clarificare ulterioară, VC06-06 (16.10.2026):** [deciziile VC06-06-D01–D11](DECIZII_DESCHISE.md#g-decizii-nir-și-contul-produsului--16102026-slice-vc06-06) înlocuiesc propunerile de mai jos care presupun un NIR pe cont sau un editor separat al facturii. Un NIR nou poate avea mai multe conturi din aceeași subunitate, determinate prin produsele liniilor; nu se adaugă cont în linie. Numai destinațiile diferite impun numere «auto» și NIR-uri separate. Migratorul păstrează modelul Access, iar contul produselor cu mișcare este blocat la editare în UI și server. **Implementare în surse, fără rulare**; analiza Access de mai jos rămâne o descriere a sursei vechi.

Data: 16.10.2026. Analiză statică a surselor; fără rularea aplicației, teste, interogări sau modificări de baze de date. La redactarea analizei inițiale nu s-a modificat codul aplicației; implementarea ulterioară este descrisă în FACTURI_NIR_IMPLEMENTARE.md.

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

## 3. Proiectarea inițială a validării — reper 16.10.2026

Propunerea folosea un singur motor pentru modificare, scoatere rând și anulare: mișcări existente fără efectul vechi al NIR + toate liniile ciornei la datele propuse. Se verifică fiecare variantă și fiecare dată, inclusiv ambele variante la schimbarea produsului/prețului, toate rândurile distincte (fără comprimarea celor identice), Decimal și precizia gestiunii, fără float/toleranțe arbitrare. Anularea are efect nou zero.

Consultarea arată document/stare/total/istoric; intrarea în modificare cere eligibilitate și motive (drept, perioadă, anulare, storno/dependențe active), păstrând documentul până la confirmare. Contextul nu mută NIR în altă subunitate/gestiune, iar schimbarea contului nu reinterpretează liniile incompatibile.

La părăsirea rândului: produs în contul ales, cantitate pozitivă, preț nenegativ, TVA existent și precizie validă; simularea întregii ciorne validează produs/cantitate/preț/TVA/date. Se păstrează ultima stare validă, cu mesaj concret și limită recalculată pentru ciorna curentă: exemplul proiectat era reducere la 50 bucăți refuzată pentru lipsă 10 la 10.01.2026, minim 60. Nu era o probă pe date. Liniile economice schimbate se recalculează; cele istorice nemodificate rămân exacte, inclusiv la editare numai de antet.

Propunerea MF prevedea o identitate de activ pe rând, cantitate 1 la intrări noi, inventar unic în gestiune și ciornă, DNF în clasificare și dată conform regulii explicite; identitatea existentă blocată, înlocuirea tratată ca eliminare+adăugare cu verificarea dependențelor. Era detaliere de proiectare, nu fapt deja implementat.

Scoaterea rândului nou afectează numai ciorna; un rând persistent se scoate după confirmare și simulare validă, cu refuz pentru stoc negativ (produs/dată/lipsă) sau activ dependent (lista documentelor). Persistă numai la Salvează; Renunță restaurează documentul.

Salvarea: context/drept/ocupare/perioadă/revizie/dependențe active → antet și toate liniile (cel puțin una), număr/subnumăr liber excluzând documentul curent → calculul numai al valorilor schimbate și egalitatea exactă a liniilor noi cu ținta aferentă → variante/linii/antet/MF/audit atomic și recontrolul cronologiei. Refuzul face rollback și păstrează ciorna; succesul revine în consultare și selectează NIR. Răspunsul pierdut cere reconciliere înainte de declararea eșecului sau repetare, inclusiv la editare/anulare.

Documentul salvat se anulează cu motiv și rămâne în istoric, fără efect în stoc; nu se portează ștergerea fizică ori eliminarea automată a stornărilor din Access. Storno activ și dependențele OI/MF valabile trebuie rezolvate explicit; simplul istoric anulat nu blochează. Se simulează toate variantele/datele, apoi anulare+audit atomic sau refuz cu document rămas activ.

La acel checkpoint, intrările foloseau DataFc; se propunea păstrarea regulii și afișarea ei în simulare. Data storno trebuia aliniată între sursa executabilă, contract, preview și rapoarte, fără schimbare tacită. Ordinea inițială era motor comun → validări de rând/mesaje → conservarea istoricului și regulile MF/dependențe → data storno → rezultatul necunoscut. Implementarea autorizată ulterior este în [VC06-06](../docs/worklog/SLICE-VC06-06-facturi-comune-corectii-nir-si-schema-unica.md) și [contractul editorului](FACTURI_NIR_IMPLEMENTARE.md); propunerea nu certifică singură fiecare comportament.

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

Dacă documentul are un rând auto-calculat, acesta nu poate fi editat nici prin propagarea TVA din antet. Selectorul comun rămâne vizibil dacă toate cotele coincid, dar schimbarea lui cere întâi ștergerea corecției cu revenire în grupul de origine. În modelul facturii comune, selectorul rămâne local NIR-ului; cote uniforme într-un NIR nu implică o cotă uniformă în toate recepțiile facturii.

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
7. Conform completării utilizatorului, rândul auto-calculat este nemodificabil. Singura operație de rând este ștergerea cu revenirea cantității în grupul de origine, la prețul și cota originale, apoi recalcularea grupului și simularea stocului. Dacă grupul nu mai există, se reconstruiește fragmentul original. Revenirea este refuzată dacă lasă stoc negativ pe varianta corectată. O altă corecție se aplică numai după această revenire; nu se acumulează diferențe la fiecare salvare.
8. Se păstrează în audit valorile inițiale, diferența, rândul/fragmentul, modul de calcul și valorile finale. Corecția trebuie reflectată în costul recepționat, în document și în rapoarte; nu poate fi o diferență ascunsă numai în antet.

Propun păstrarea plafonului de 1 leu pentru comanda automată de ajustare, ca protecție împotriva lipsei unor rânduri sau a unei repartizări incomplete. Peste plafon se corectează datele/repartizarea, fără înlocuirea automată a ValoareFC. Plafonul nu este o toleranță la finalizare: după corecție diferența trebuie să fie exact zero. Dacă ValoareFC nu este încă introdusă, documentul poate rămâne în lucru cu un total calculat; finalizarea față de factura furnizorului cere o țintă explicită. Zero introdus explicit și valoare lipsă sunt situații diferite.

Preferința utilizatorului este păstrarea cotei reale. Varianta 0% rămâne o opțiune explicită de compatibilitate, dacă se alege păstrarea strictă a vechiului model de calcul. Nu se folosește automat drept soluție pentru lipsa preciziei sau pentru consumuri care blochează mutarea între variante: acestea sunt probleme independente de cotă. La migrare, rândurile deja ajustate la 0% se păstrează astfel; cota lor originală nu se poate deduce sigur.

## 8. Modelul inițial de factură comună și migrarea — propuneri istorice

**Înlocuit parțial în aceeași zi:** editorul separat de factură și NIR-ul per cont au fost respinse prin VC06-06-D01–D11. Contractul adoptat este [FACTURI_NIR_IMPLEMENTARE](FACTURI_NIR_IMPLEMENTARE.md): introducere în Adăugare NIR, mai multe conturi în aceeași destinație, împărțire numai între destinații. Celelalte idei de mai jos sunt păstrate ca justificare și cerințe de reconciliere, fără a le declara toate implementate.

Identitatea facturii furnizorului este comună în DC, separată de NIR-urile locale: VC_FACTURI (furnizor, număr original, dată, monedă, ValoareFC, stare/revizie), VC_FACTURI_LINII, VC_FACTURI_REPARTIZARI și legătura spre VC_NIR. Alimente/Materiale/OI/MF sunt gestiuni ale subunităților; se acoperă mai multe gestiuni/subunități în același DC, fără operații între baze DC. Fiecare NIR își păstrează număr/data/comisie/stare/revizie/drepturi și stoc/registru local; propunerea inițială de NIR pe subunitate+gestiune+cont nu este regula actuală.

O linie poate fi împărțită în cantități/valori pe mai multe destinații și recepții parțiale, inclusiv mai multe active MF, fără mapare 1:1 ori dublarea cantității active. Produsele locale se aleg în destinație; ID-urile nu sunt interschimbabile.

Total cu TVA al unui NIR = valoarea repartizată lui; suma activă la finalizarea completă = ValoareFC introdus o singură dată. Exemplu: 1.000 lei = 200 Alimente + 300 Materiale + 500 MF, fără numărarea facturii de trei ori. În proiectul cu recepții parțiale, «în lucru / parțial / reconciliat» erau stări distincte, iar anularea unui NIR elibera numai partea lui, păstrând factura. Nu se absoarbe artificial restul nerepartizat în fiecare NIR. Corecția globală este făcută o dată, pe rânduri/destinații explicite, numai când diferența nu reprezintă bunuri încă nerecepționate. ZEC diferit nu autorizează pierderea resturilor; repartizarea trebuie să fie reprezentabilă, iar afișarea la două zecimale nu dovedește egalitatea la patru.

Ecranul inițial propunea factura → alegerea destinațiilor/cantităților → NIR-uri rezultate și rest, cu cale simplă directă pentru o destinație; a fost înlocuit după deciziile utilizatorului. Principiile tranzacționale: ocuparea tuturor gestiunilor cerute în ordine stabilă și blocarea/revizia facturii, refuzul întregului lot la lipsă de acces, o tranzacție în DC; salvările locale succesive blochează factura pentru a preveni supra-alocarea. Modificarea datei/furnizorului/totalului comun după recepții cere analiza destinațiilor și a efectului datei asupra stocului, fără propagare tacită în perioade închise sau consumuri incompatibile. Storno/creditul furnizorului rămâne document distinct legat de factura inițială/stornările locale; stocul inițial nu primește factură fictivă.

### Proveniență și reconciliere fără reimport

- NIR-ul Access rămâne local, cu cont/număr/subnumăr/date/valori/variante/MF/stornări/proveniență. Neconcordanțele se păstrează cu diagnostic, fără schimbarea cantităților/cotelor/prețurilor. VC_ImportSources/VC_IdMap identifică sursa/importul și cheia documentului; IdIntrare singur se poate repeta în fișiere/subunități. Asocierea nu renumerotează și nu reimportă.
- Access nu are identitate globală a facturii. Furnizor+număr+dată este candidat, nu dovadă de identitate/completitudine sau că antetul local conține totalul global. Nici numărul NIR, nici totalul egal nu sunt chei de factură.
- Propunerea creează provizoriu fragment per NIR obișnuit, cu antetul sursă declarat numai pentru fragment. Gruparea candidată folosește furnizorul fiscal mapat, numărul original/normalizat conservator, data și moneda cunoscută, fără completarea necunoscutelor. Operatorul confirmă reunirea și totalul din factura furnizorului după importul celorlalte subunități.
- Nu se însumează antetele locale, care pot repeta același total; se arată separat antetele și sumele liniilor. Liniile originale ale facturii reconstruite din fragmente sunt marcate ca atare, fără pretenția de a reproduce numărul/ordinea liniilor furnizorului. Rapoartele separă recepțiile certe de totalurile globale provizorii.
- Stocurile inițiale rămân fără factură, legăturile locale NIR–storno rămân mapate. În VrcPartition.vb:83, contul antetului determina destinația, iar liniile contradictorii Alimente/Materiale erau conflict; factura comună nu îl transformă în repartizare automată. Nu se inventează numere sau împărțiri pentru istoricul contradictoriu.
- Pentru date deja migrate, completarea este separată/reluabilă pe identitățile existente, cu markerul importului unic păstrat. Reunirea schimbă numai asocieri/metadate cu audit, fără mișcări/valori istorice rescrise.

Ordinea revizuită a proiectării era modelul și sensul țintelor → fișiere DDL/adaptarea importului inclusiv date existente → cronologia/TVA derivat → corecția cu TVA real și efectul în variante/MF/rapoarte → simulare/atomicitate multi-context → migrare conservatoare/reconciliere/rezultat necunoscut. DDL exclusiv manual, fără creare automată în aplicație.

## 9. Implementarea autorizată ulterior, 16.10.2026

Utilizatorul a cerut implementarea și blocarea rândurilor AUTO, cu ștergere/revenire în grup. Codul și SQL-ul pregătit sunt descrise în [FACTURI_NIR_IMPLEMENTARE](FACTURI_NIR_IMPLEMENTARE.md), iar evoluția/probele în [VC06-06](../docs/worklog/SLICE-VC06-06-facturi-comune-corectii-nir-si-schema-unica.md). Starea la predare: surse scrise, fără execuție/compilare/teste/browser/MariaDB; DDL pentru utilizator și sincronizare înaintea folosirii. D001-04 condensează propunerile; nu schimbă regulile sau starea prin presupunere.
