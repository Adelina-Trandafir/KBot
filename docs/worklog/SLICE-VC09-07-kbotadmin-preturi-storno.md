# SLICE-VC09-07 — KBotAdmin: verificarea și repararea prețurilor storno pe baza selectată

Data: 30.10.2026. **IMPLEMENTAT_LOCAL, NERULAT, NEPUBLICAT.**

Continuare autorizată: [VC09-09](SLICE-VC09-09-retururi-si-rulaje-balanta.md) extinde comanda și repararea la bonurile de retur, cu titlul **Preturi storno si retur VERCON**. Descrierea de mai jos consemnează etapa inițială numai pentru storno.

## Cererea și comportamentul

Utilizatorul a cerut înlocuirea corecției SQL pentru un singur produs cu o comandă în **KBotAdmin → Comenzi**, care verifică această categorie de erori în întreaga bază selectată și oferă repararea tuturor cazurilor eligibile.

Comanda **Prețuri storno VERCON** (`storno_preturi`) nu cere IdProdus și doar citește. Parcurge `VC_IntrariStorno` din toate subunitățile, gestiunile și exercițiile bazei selectate, fără filtru de produs sau dată. Listează prețurile diferite de `VC_ProduseP.PUTVA` și prețurile/variantele lipsă. Legăturile variantelor și produselor includ subunitatea și gestiunea; legătura NIR include și exercițiul. Un IdProdus egal din altă gestiune nu poate furniza prețul de corecție.

Rezultatul are sumar pe subunitate/gestiune (diferențe, reparabile, de analizat, diferența valorică exactă a cazurilor reparabile) și fiecare linie cu produs, exercițiu, număr/data NIR, IdProdus, ID linie, cantitate, preț actual, preț propus, versiune și motiv. Detaliile și corecțiile sunt returnate integral, cu antet explicit în blocurile raw; nu există o limită de primele N rânduri care să limiteze repararea. Ordinea afișării este alfabetică românească pe nume, numerică pe preț și cronologică pe dată; numărul NIR este numeric. Comenzile din selector sunt ordonate alfabetic în română.

## Regula de reparare

**Repară** apare după verificare dacă sumarul are cazuri reparabile. Comanda `storno_preturi_repara` se poate selecta și direct, fiind declarată ca operație de scriere; există confirmarea obișnuită a KBotAdmin. Verificarea și repararea cer **o singură bază** atât în client, cât și pe server. `ALL` este refuzat înaintea conectării pentru noua comandă. Baza trebuie să fie una dintre bazele de unitate acceptate de `_resolve_db`; nu se repară AVACONT_SURSA sau AVACONT_COMUN.

Se repară automat numai o linie pentru care:

- prețul actual este întreg și corespunde rotunjirii ToEven a prețului variantei din aceeași gestiune;
- părintele este un NIR cu `Storno=1`, linia și documentul sunt active, cantitatea și versiunea documentului există;
- `VC_IdMap` confirmă o linie `IntrariStorno` / `ROW` cu acel ID țintă, legată de `VC_ImportSources` din aceeași subunitate și gestiune;
- versiunea liniei este încă 1, iar infrastructura existentă de audit este disponibilă.

Celelalte diferențe sunt afișate cu motiv și nu sunt rescrise: nu se presupune că orice diferență de preț este o eroare de rotunjire. Tabelele/coloanele lipsă sunt raportate; nu se creează automat structuri.

Repararea face o citire proaspătă după obținerea blocărilor pe gestiunile bazei, în aceeași tranzacție `READ COMMITTED`. Nu repară o listă de ID-uri primită din client. Fiecare UPDATE reverifică toate cheile, prețul/cantitatea/versiunea veche, prețul variantei și starea/revizia părintelui. Dacă un rând nu mai corespunde, tranzacția revine integral.

Se schimbă `VC_IntrariStorno.PUTVA` și versiunea liniei. Pentru fiecare document atins se crește **o singură dată** `VC_NIR.Version`, cu audit, pentru a invalida editorii deja deschiși care verifică revizia antetului. Cantitățile, cotele TVA, prețurile variantelor și valorile din antetele NIR rămân neschimbate. Nu se aplică un nou calcul de TVA peste prețul care include deja TVA.

Fiecare corecție are audit vechi/nou în `VC_Audit` (`admin.storno.price.repair`), iar actualizarea reviziei documentului are `admin.storno.price.repair.document`. Rezumatul este scris în Jurnal prin mecanismul existent al KBotAdmin. Liniile, documentele, auditul și Jurnalul trebuie să fie InnoDB; altfel repararea este refuzată fără scrieri. Confirmarea tranzacției are loc numai după pregătirea întregului rezultat: număr reparat, toate prețurile vechi/noi și diferențele rămase. O reluare după o reparare completă nu rescrie liniile deja corecte.

Butonul din client este legat de comanda și baza verificată. Schimbarea bazei/comenzii îl ascunde; un rezultat întârziat din alt context nu îl reactivează. Erorile unei rulări ascund accesul la repararea bazată pe un rezultat anterior. Același control păstrează și repararea existentă a semnelor din registrul OI.

## Fișiere

- `PYTHON/routes/admin_cli/vercon_storno_prices.py`: noul modul de verificare/reparare.
- `PYTHON/routes/admin_cli/admin_cli.py`: whitelist, handler-e, restricția pe o singură bază, metadată `single_db`.
- `KBotAdmin/Views/CommandsView.vb`: explicații, afișarea și rutarea butonului, legarea rezultatului de context, ordonarea comenzilor.
- `KBotAdmin/Server/Dtos.vb`: `CommandInfo.SingleDb`.
- `VERCON/VRC.Migrator/README.md`, `VERCON/VERCON_STATUS.md`, `implementare_vercon_v2.md`, worklogul VC09-06 și acest worklog.
- Cele două fișiere `sql/VC09_06_047_GR47_*` pregătite în etapa anterioară au fost eliminate; nu fuseseră executate.

Corecția din migrator pentru importurile viitoare (VC09-06 / surse 0.3.14.0) se păstrează. Calculul balanței și sursele rapoartelor nu sunt modificate; motivul lipsei produsului din raportul efectiv rămâne cel consemnat în VC09-06.

## Stare și limite

Doar citire de cod și analiză de proiectare. **Fără build, teste, verificări automate de sintaxă, rulare KBotAdmin, browser, SSH, HTTP, SQL executat, modificări MariaDB sau publicare.** Nu s-a măsurat câte cazuri există în baza reală și nu se afirmă că toate stornările sunt greșite. Sunt necesare republicarea modulului server și recompilarea KBotAdmin de către operator pentru ca funcția să fie disponibilă în aplicația instalată.
