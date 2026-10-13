# SLICE-VC08-02 — motor amortizare

12.10.2026 — **IMPLEMENTAT_LOCAL**, nerulat și nevalidat pe date reale. Cerere: „fă-l”, după VC08-01.

## Implementare

- `PYTHON/routes/vercon/depreciation.py`: funcții pure pentru lunile calendaristice, valoarea lunară, DF, amortizarea cumulată/la lună, rest și casare. Aritmetică în întregi scalați, cu HALF_EVEN la calcul și fără float/divizare prin durată invalidă.
- `depreciation_source.py`: registrul MF este citit separat de documente. Recețiile/storno/BP/BT/BS au filtre de subunitate, gestiune și exercițiu pe fiecare ramură și pe legăturile NIR. Documentele sunt limitate la variantele din registrul selectat; se citesc toate datele necesare începutului și opririi, fără un filtru de an care ar pierde predarea inițială. Nu există JOIN între recepții și predări care să multiplice activul.
- `depreciation_read.py`: un rezultat per variantă, valori transportate ca text exact, diagnostice pe activ, numerele de linii sursă, reviziile registrului/variantei și totalurile activelor calculabile. `complete=false` dacă există active invalide; activele viitoare/nepredate sunt explicit excluse din totaluri. Registrul dublat este diagnosticat, fără adunarea rândurilor duplicate.
- `GET /api/vercon/mf/depreciation?date=YYYY-MM-DD&variant=<id>`: consultare, drept `raport`, context/versiune verificate de infrastructura existentă, numai tip MF. Fără `variant`, calculează întregul registru. Un ID străin gestiunii este refuzat cu 404. Nu schimbă date, nu ocupă gestiunea, nu execută DDL sau commit business.
- `js/vercon/depreciation.js`: comanda **Amortizare la dată** din meniul unui rând al registrului MF, disponibilă numai cu dreptul `raport` și fără ciornă activă. Dialog comun, DatePicker, DataGrid readonly, sursele valorii lunare, diagnostice și preview/PDF comun. Data schimbată invalidează calculul și previewul; răspunsurile vechi/după închidere sunt ignorate.
- CSS separat în `css/vercon-mf.css`, importat după modulele existente. `decimal-text.js` păstrează exact formatterul half up folosit de Stocuri și îl reutilizează pentru amortizare; fără calcule monetare locale.

## Reguli concretizate din Amortizare.sql / CONTRACT_CALCULE §3.5

1. `DateDiff("m", început, dată)` numără schimbările de lună: 31.01 → 01.02 = 1; 01.02 → 29.02 = 0. Nu se folosește o durată de 30 zile și nici `Abs` pentru o dată anterioară predării.
2. `DataIntrare` este poarta intrării în evidență. Prima BP neanulată cu cantitate pozitivă fixează începutul folosirii; transferurile și predările ulterioare nu îl resetează. Prima predare înainte de intrare este diagnosticată. Fără predare la dată, activul este „nepredat”, fără amortizare inventată.
3. DF este durata deja folosită la prima predare. `folosit = min(DNF, DF + luni_calendaristice)`; `rămas = DNF - folosit`. DNF NULL/0/negativ și DF NULL/negativ/>DNF dau diagnostic, nu înlocuire tacită cu zero.
4. **Baza de evaluare:** `ProduseP.PUTVA` curent, exact ca în `Amortizare.sql`. Valoarea din registrul MF existent („value”, suma valorilor recepțiilor) nu este confundată cu această bază. Nu se multiplică valoarea prin numărul de recepții/predări.
5. `ValoareLuna` prezentă este sursa primară, rotunjită la `ZEC` pentru calcul. `ValoareLunara` este fallback numai dacă prima este NULL; dacă ambele lipsesc, se derivează baza/DNF la `ZEC`. Zero stocat nu este NULL. Ambele originale rămân în răspuns și în baza de date. Diferența între surse sau între valoarea lunară și baza curentă/DNF este vizibilă, fără rescriere automată.
6. Înainte de ultima lună, acumulat = `min(bază, luni_folosite × valoare_lunară)`. La durata epuizată, acumulat = baza exactă și rest = 0. Ex.: 100,00 / 3, valoare lunară 33,33 → 33,33 + 33,33 + **33,34**. Restul poate păstra zecimale suplimentare ale sursei față de ZEC; numai afișarea îl rotunjește.
7. Casarea integrală oprește calendarul la data casării; în lunile ulterioare amortizarea lunii și a lunii următoare sunt zero. Valoarea neamortizată la casare se expune separat; motorul nu inventează o înregistrare contabilă de descărcare. Casările anulate nu intră în calcul.
8. O variantă MF cu număr de inventar este tratată ca un activ de **o bucată**. Mai multe linii de recepție pot însuma aceeași bucată, fără multiplicare. Cantitatea netă diferită de o bucată, casarea parțială sau casarea excesivă sunt diagnosticate, fără alocarea arbitrară a valorii. **Limită explicită:** variantele ce reprezintă grupuri de mai multe bucăți necesită o regulă de repartizare pentru amortizare; editorul NIR existent le permite și nu este schimbat aici.
9. Răspunsul este o proiecție la dată cu **parametrii curenți** (`parameter_mode=current_register`) și reviziile lor. Nu este replay al versiunilor istorice din audit. Citirea nu modifică istoricul sau câmpurile lunare; editorul MF existent continuă să auditeze modificările sale.

## Comparator pregătit, neexecutat

`VERCON/tools/test_vc08_depreciation.py`: 11 probe sintetice pe activ, cu referință literală pentru DateDiff/formula legacy: capete de lună/februarie bisect/an nou, durate după BP, rest ultima lună, DF, dată înainte de predare/intrare, durate invalide, maparea celor două câmpuri/scară, transfer/casare, mai multe documente fără multiplicare, cantități ambigue/registru duplicat, context pe toate ramurile SQL.

Diferențe intenționate față de sursă: `Abs` nu produce amortizare înaintea predării; ultima lună închide 100,00, în loc de 99,99; valoarea lunară folosește ZEC, nu 2 fix; activul nu apare de mai multe ori prin JOIN. **Nu s-a executat Access și nu se declară comparator real trecut.**

## Stare și continuare

Au fost citite contractele și revizuită proiectarea codului. **Nu s-au executat teste, verificări de sintaxă, browser, aplicație, SQL sau acces MariaDB în această etapă**, conform AGENTS.md. Probele locale trecute anterior aparțin reorganizării CSS/VC08-01, nu validează VC08-02. Scriptul de verificare CSS existent a fost adaptat să distingă cele două module CSS noi de stylesheet-ul original, fără a fi rerulat.

Următorul slice: **VC08-03 — catalogul și generarea rapoartelor incluse**. Corective followup-ul UI rămâne separat, conform cererii utilizatorului.
