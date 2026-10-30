# SLICE-VC09-06 — Prețul cu TVA al stornărilor și includerea în balanță

Data: 30.10.2026. **IMPLEMENTAT_LOCAL, NERULAT, NEPUBLICAT.**

## Scop

Sesizarea privește `047_GR47`, Grădinița 47 / Alimente, produsul `10043` (iaurt, varianta cu preț 14,4855), și Balanța pentru 01.10.2026–31.10.2026. Intervenția în cod este limitată la prețul stornărilor în VRC.Migrator. Nu au fost modificate sursele rapoartelor, alte tabele sau fluxuri.

## Cauza prețului și corecția

Schema Access exportată (`VERCON/VERCON_ACCESS/Databases/TZ_SCHEMA/tables/IntrariStorno.md`) declară `PUTVA` ca `Long`, spre deosebire de prețul zecimal al variantei din `ProduseP`. Migratorul copia întregul fără un calcul de TVA: diferența 14,0000 / 14,4855 exista deja în datele sursă. Formularul FCS folosește `QFCS` cu `ProduseP.PUTVA`; și raportul Access `NIR_Storno.txt` folosește direct prețul variantei, nu întregul din `IntrariStorno`.

În `VrcRunner.Storno.vb`, `PrepareStornoRow` recuperează prețul din `ProduseP` numai din aceeași sursă și partiție/gestiune. Cheia de cache este sursa planului, iar cheia produsului este `IdProdus` local acelei surse; același număr din alte gestiuni nu este consultat. Recuperarea se face numai pentru valori Access `Int32` egale cu rotunjirea la întreg a prețului variantei (ToEven). Prețurile deja zecimale, nule și egale cu varianta rămân neschimbate. Dacă întregul diferă de rotunjirea variantei, rămâne neschimbat și este semnalat N21. Nu se recalculează cota TVA și nu se adaugă TVA peste un preț care o include deja.

Se prelucrează o copie a rândului, după filtrarea părinților storno eligibili și înaintea conversiilor și acumulării sumelor din `BuildRow`. Reconcilierea existentă compară sumele convertite cu ținta, deci include corecția. Raportul N20 păstrează ID-ul liniei și prețul vechi/nou, inclusiv în rezumatul pe fișier. Access nu se modifică.

Versiunea sursă: **0.3.14.0**; `RuleVersion=VC09-STORNO-PRICE-2026-10-30` schimbă și amprenta planului. Executabilul trebuie recompilat de operator înaintea utilizării; nu a fost compilat în această intervenție.

Pentru linia comunicată, 35 × 14,4855 = **506,9925 lei** cu TVA; baza din intrare este 35 × 13,05 = **456,7500 lei**, diferența TVA **50,2425 lei**. Cu prețul întreg, valoarea era 490,0000 lei, iar diferența față de bază 33,2500 lei. Sunt calcule de proiectare pornind de la datele furnizate, nu rezultate obținute prin rulare.

## Balanța

Atât `stock_source.WAREHOUSE`, cât și sursa standard `PYTHON/reports/vercon/queries/ledger.sql` au ramură explicită pentru `VC_IntrariStorno`, cu cantitate negativă. `stock.BUCKETS` include storno în ieșiri, iar `report_data.balances` îl însumează în `outgoing`. `report_balance.prepare` verifică SI, intrări, ieșiri și SF; nu elimină rândul doar pentru că SF este zero.

Din liniile și starea documentelor comunicate, pentru octombrie rezultă **SI 0, intrări 69, consum 34, storno 35, ieșiri totale 69, SF 0**. Rândul trebuie să rămână în Balanță. Evaluarea balanței folosește deja 14,4855 din variantă; corectarea prețului documentului storno nu schimbă cantitățile sau această evaluare.

**Absența din raportul efectiv nu este încă explicată.** Nu au fost citite definiția SQL personalizată a unității, selecția efectiv trimisă raportului sau versiunea publicată. Codul standard nu justifică omiterea acestui produs. Nu s-a introdus o a doua scădere a stornărilor și nu s-a modificat balanța pe baza unei presupuneri.

## Fișiere SQL manuale — retrase prin VC09-07

- `sql/VC09_06_047_GR47_storno_10043.sql`: corectează numai linia `VC_IntrariStorno` ID 12, subunitate 9, gestiune 29, exercițiu 46, NIR 4124, produs 10043. Are condiții pe cantitate, preț, versiunea 1, varianta 5019/14,4855 și starea activă a NIR-ului storno; crește versiunea. Nu rescrie alte linii, antete sau prețuri. Nu este o reparație generală a importurilor existente.
- `sql/VC09_06_047_GR47_balanta_10043_diagnostic.sql`: SELECT pentru SI/rulaje/SF ale aceluiași produs și context, cu cele patru familii și filtrele sursei standard. Poate stabili dacă mișcările din baza actuală corespund celor comunicate. Nu inspectează definițiile personalizate sau filtrele din browser.

Ambele fișiere au fost pregătite, **niciodată rulate**, apoi eliminate la cererea utilizatorului: reparația trebuie oferită pentru întreaga bază selectată, în KBotAdmin → Comenzi. Implementarea care le înlocuiește este în [SLICE-VC09-07](SLICE-VC09-07-kbotadmin-preturi-storno.md). Descrierile de mai sus păstrează istoricul predării inițiale. Nicio schimbare de schemă, nicio conexiune sau modificare MariaDB.

## Fișiere modificate și limite

`VERCON/VRC.Migrator/VrcRunner.Storno.vb`, `VrcRunner.vb`, `VrcRun.vb`, `VrcPlan.vb`, `VRC.Migrator.vbproj`, `README.md`; cele două fișiere SQL; acest worklog și trimiterile din status/index.

Au fost făcute numai citirea codului, urmărirea regulilor și analiza de proiectare. Fără teste, build, verificări automate de sintaxă, citire de baze Access reale, browser, HTTP, SQL executat sau publicare. Noua recuperare și SQL-ul pregătit rămân neprobate în execuție; datele deja migrate nu au fost schimbate.
