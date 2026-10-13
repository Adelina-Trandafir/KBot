# SLICE-VC08-01 — motor stoc și balanțe

12.10.2026 — **IMPLEMENTAT_LOCAL**. Fără deploy sau operații MariaDB.

## Cod și contract

`stock_source.py` citește NIR/BC/BR/storno cu filtre subunitate, gestiune și exercițiu pe fiecare ramură/JOIN. `protocol.movements` folosește aceeași sursă, păstrând interfața pentru disponibil și controlul stocului negativ în tranzacția salvării. Documentul editat poate fi exclus din familia sa; ordinea ramurilor, semnele și datele corespund protocolului anterior.

Deținerile OI/MF se calculează separat din BP/BT/BS: predare plus la primitor, transfer minus la predător și plus la primitor, casare minus. Nu se adună registrul la consumul magaziei. Exercițiul multianual este rezolvat de protocol; anul din UI filtrează intervalul. Citirea directă a liniilor evită multiplicarea prin JOIN la loturi.

`stock.py`: SI pentru `data < start`, rulaje în interval închis `[start,end]`, SF și fișă cu sold după fiecare mișcare. Cheia integrală separă variantele, deținătorii și, în motorul generic, furnizor/variantă (N09). Cantitățile folosesc întregi cu precizie arbitrară. N07 separă cantitățile/valorile casate și include capetele; N08 aplică data storno și precizia draftului. Evaluarea folosește **prețul curent** `ProduseP.PUTVA` (Q9), fără rescrierea documentelor. Produsul exact este transportat la `2*SCARA`, afișat cu `ZEC`, half up. Totalurile valorice sunt independente de filtrarea vizuală.

API:

- `GET /api/vercon/stock/balance?start=YYYY-MM-DD&end=YYYY-MM-DD`: drept `raport`, context server/versiune, filtre `product`, `variant`, `category`, `supplier`, `holder`, `ledger=warehouse|custody`, `group=variant|supplier`.
- `POST /api/vercon/stock/draft-available`: drept `read`, fără scriere/commit; `{subunit,gestiune,year,date,exclude?:{kind,id},lines:[{variant,quantity:"1.0040"}]}`. Agregă liniile pe variantă, refuză variante străine și cantități inexacte. Informativ; salvarea controlează cronologia completă în tranzacție.
- `/api/vercon/stock/available` existent păstrează contractul.

**Limită furnizori:** consumul nu are legătură la furnizor în schema inclusă. Fișa legacy folosea `IntrariIesiri`/Coduri, excluse. Nu se inventează FIFO. Filtrul selectează variantele aprovizionate de furnizor, apoi păstrează toate mișcările lor, inclusiv SI. `group=supplier` este marcat `document_provenance` cu `supplier_notice`; consumul/returul neatribuit are furnizor NULL. **Nu reprezintă stoc atribuit furnizorilor.** N09 este corectat în motor; paritatea cu fișa legacy pe Coduri nu este declarată.

## Pagina și verificarea

`js/vercon/stock.js`, `css/vercon-stock.css`, meniul Stocuri: DataGrid, TreeView, DatePicker, Combobox magazie/deținători OI/MF, preview/PDF comun. Perioadă/categorii în stânga, stoc/mișcări în centru, detalii/comenzi în dreapta. NIR/BC navighează la editor numai când meniul serverului permite operația. Previewul este local; rapoartele oficiale sunt VC08-03. Intervalul modificat invalidează grila, răspunsurile vechi/după destroy sunt ignorate.

Cererea „asigură-te că nu se strică ceva” autorizează verificările locale ale acestei etape:

- `VERCON/tools/test_vc08_stock.py`: **10/10 probe sintetice trecute** (SI/capete, N07, transfer, N08, N09, filtru furnizor/SI, preț curent, context SQL/excludere, selecție goală, disponibil și refuz stoc negativ).
- `VERCON/tools/check_v2_browser.cjs`: CSS reconstruit identic cu originalul; importurile HTML-urilor PORTAL/ADECHIT/VERCON se încarcă în Chromium cu JS aplicațiilor oprit și resurse locale. Fixture Stocuri: selecție/mișcări, schimbare lună/DatePicker, căutare, 390/1280px, dark și cleanup, fără erori JS/resurse lipsă. Prima probă a găsit bootstrapul `utils/css.js` lipsă din fixture; fixture-ul a fost corectat, apoi proba a trecut.
- Fără aplicație reală/MariaDB. Integrarea pe date reale, concurența și salvarea completă rămân nevalidate; VC07-05 și comparatorul VC08-05 sunt separate.

Următorul slice: **VC08-02 — motor amortizare**.
