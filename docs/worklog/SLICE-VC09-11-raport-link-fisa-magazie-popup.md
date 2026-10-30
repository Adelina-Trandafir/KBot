# SLICE-VC09-11 — Din produsul raportului în fișa de magazie, într-un popup

Data: 30.10.2026. **IMPLEMENTAT_LOCAL, NERULAT, NEPUBLICAT.**

## Cererea și comportamentul

Utilizatorul a autorizat denumiri de produse clicabile în raportul afișat în aplicație: click pe produs → fișă de magazie într-un popup compact; raportul principal rămâne deschis la aceeași pagină și poziție.

Denumirea produsului este colorată și subliniată în PDF. Linkul este pus numai pe rânduri care identifică **o singură variantă de produs/preț**. Totalurile, antetele, rândurile anulate și agregările care combină mai multe variante nu primesc link către o variantă arbitrară. În agregările de date se transportă separat `stock_variant`, fără schimbarea sumelor sau a cheilor de grupare existente. Machetele personalizate folosesc cheia stabilă `name`, independent de eticheta vizibilă a coloanei.

Linkurile există în rapoartele oficiale generate din pagina Rapoarte și din documentele salvate, inclusiv forma PDF NIR/FCS și tabelele dispuse pe mai multe coloane. După schimbarea perioadei în vizualizator, linkurile folosesc modelul și perioada raportului regenerat. Previzualizările locale ale ciornelor nesalvate și cele construite doar din secțiuni locale nu sunt convertite în rapoarte oficiale și nu primesc această navigare.

## Contextul și datele popupului

Modelul conține `stock_card` cu subunitatea, gestiunea, anul și intervalul exact din raport, plus variantele eligibile. Denumirea nu este folosită pentru identificare; varianta este legată de gestiunea raportului, unde `IdProdus` are semnificație locală.

URL-ul este relativ la aplicație și folosește endpointul existent, numai de citire, `/api/vercon/stock/balance`, cu varianta exactă, intervalul și `ledger=warehouse`. Nu se generează un alt PDF sau un raport complet. Nu se filtrează fișa la un singur furnizor/document din raport: sunt mișcările de magazie ale variantei în acel interval, cu soldul inițial calculat din întregul istoric anterior.

Clientul acceptă numai URL-ul de pe aceeași origine, calea de stoc cunoscută, varianta inclusă în model și parametrii identici contextului raportului, fără valori duplicate sau parametri suplimentari. Endpointul existent verifică din nou autentificarea, dreptul de raport și contextul serverului. Nu există un endpoint nou, SQL liber, schimbări de schemă sau scrieri de date.

Popupul afișează gestiunea, perioada, codul, UM și prețul cu TVA; sumarul SI / Intrări nete / Ieșiri nete / SF, cantitativ și valoric; apoi grila cu sold inițial și fiecare mișcare (data, document/factură, tip, intrări, ieșiri, sold și valoarea soldului). Storno este negativ la Intrări, returul negativ la Ieșiri. Valorile zecimale sunt afișate prin formatterul exact existent, fără conversie în float. Ordinea este cea cronologică a motorului de stoc; sortarea/filtrarea grilei este dezactivată pentru a păstra sensul soldului după fiecare document și poziția soldului inițial.

Fișa citește datele curente la click, în contextul și perioada raportului; nu pretinde că este un al doilea snapshot istoric al bazei. O variantă care nu mai este disponibilă sau un refuz de acces produce un mesaj în popup. Lipsa mișcărilor din interval este afișată explicit, păstrând soldul inițial disponibil.

## PDF și ciclul de viață

Rendererul comun PDF primește opțional `href` pe o celulă și produce linkuri relative numai când o aplicație le furnizează; restul textului rămâne escapate. Forma NIR/FCS are același tratament pentru numele articolului. Exportul Excel păstrează valorile celulelor, fără a deveni o interfață de popup.

PDF.js primește opțional `docBaseUrl` pentru rezolvarea linkurilor relative. Numai adaptorul VERCON activează rezolvarea; vizualizatoarele celorlalte aplicații păstrează configurarea existentă. Vizualizatorul comun are un callback opțional `onLink`, iar CSS-ul VERCON permite click numai pe adnotările linkurilor de stoc, când acel callback este prezent. Formularele PDF/XFA și celelalte adnotări rămân inactive ca înainte.

Clickul este interceptat înaintea navigării PDF.js. Popupul este un dialog separat deținut de raport; butonul Închide și Escape îl închid, fără reîncărcarea, derularea sau schimbarea zoomului PDF-ului principal. Există o singură fișă deschisă per model; cererea este anulată la închiderea popupului, iar închiderea raportului eliberează și fișa. Răspunsurile întârziate nu reconstruiesc ferestre închise. La înlocuirea raportului pentru o altă perioadă, suprafața PDF este temporar inertă până când modelul și callbackul noii perioade sunt instalate.

Popupul este o funcție a aplicației autentificate. Nu este o fereastră inclusă în fișierul PDF descărcat și deschis separat într-un cititor extern. Rapoartele deja generate fără metadatele/linkurile noi trebuie regenerate.

## Fișiere

- `PYTHON/routes/vercon/report_stock_card.py` (nou): identificarea variantelor eligibile și linkurile/metadatele modelului.
- `PYTHON/routes/vercon/report_data.py`, `report_service.py`: identitatea sigură a agregărilor și atașarea metadatelor.
- `PYTHON/reporting/output.py`, `PYTHON/routes/vercon/report_reception.py`: linkuri PDF pe denumire.
- `PYTHON/static/js/vercon/report-stock-card.js` (nou): popupul compact și validarea linkului.
- `PYTHON/static/js/vercon/reports.js`, `document-report.js`, `report-period-action.js`, `pdf-preview.js`: conectarea navigării la modelele inițiale și regenerate.
- `PYTHON/static/js/reporting/pdf-preview.js`, `PYTHON/static/js/portal/pdfview.js`: extensiile opționale ale vizualizatorului.
- `PYTHON/static/css/vercon-stock-card.css` (nou), `vercon.css`: stiluri separate, limitate la această funcție.
- Acest worklog, `VERCON/VERCON_STATUS.md`, indexul `implementare_vercon_v2.md`.

## Stare și limite

Doar citire de cod și analiză de proiectare. **Fără teste, build, verificări automate de sintaxă, generare/randare PDF de probă, browser, HTTP, SQL executat, date reale sau publicare.** Clickul în PDF.js, aspectul popupului, paginarea PDF-urilor cu linkuri și recuperarea focusului nu au fost probate în execuție. Este necesară actualizarea serverului și resurselor web și regenerarea rapoartelor pentru utilizarea funcției.
