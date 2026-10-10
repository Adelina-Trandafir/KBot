# ADE10-04 — Selectorul „Subunitate" și comutarea

Implementează secțiunea 7 din [planul subunităților](../../ADECHIT/plan_subunitati.md).

## Ce s-a schimbat și de ce

- În antet, lângă unitate, combobox-ul comun **Subunitate** (denumirea rămâne vizibilă). Selecția se
  ține în `sessionStorage` (independentă în fiecare filă, pe cheia DC-ului: schimbarea DC-ului o
  invalidează) și se trimite pe fiecare cerere în `X-Ade-Subunit`.
- La deschidere: subunitatea reținută dacă serverul o acceptă; singura subunitate se alege automat; cu
  mai multe și fără una reținută, ecranul cere alegerea și nu citește nimic din evidență; fără nicio
  subunitate, mesaj care trimite la migrator. Dacă serverul refuză subunitatea reținută
  (`SUBUNIT_CONTEXT`), selecția se uită și se cere din nou.
- **Comutarea reîncarcă pagina**: anii, lunile și grupele selectate, grilele, cataloagele, dialogurile,
  cache-urile și orice răspuns întârziat al subunității vechi dispar odată cu ea. Dacă există o fereastră
  deschisă sau un rând nou de document completat, comutarea este refuzată cu mesaj (nu se pierde nimic în
  tăcere). Cererile de listare și PDF trimit și ele subunitatea.

## Fișiere

`PYTHON/static/js/adechit/app.js`, `PYTHON/static/adechit.html` (selector și paragraf de ajutor),
`PYTHON/static/css/adechit.css`.

## Teste

Niciun test rulat; nicio verificare vizuală. Ajutorul din pagină și `ADECHIT/UTILIZARE_WEB.md`
actualizate cu referința ADE10-04.

## Rămâne neverificat / amânat

Aspectul în antet pe PC și telefon, comutarea cu două file deschise, tema închisă — de probat de utilizator.

## Corecturi după review (ADE10-06)

- Selectorul apare numai dacă utilizatorul are acces la mai mult de o subunitate; una singură se alege automat.
- Comutarea salvează întâi celula de prezență aflată în editare (ca Enter) și este refuzată dacă salvarea eșuează sau dacă o salvare este în curs
  (`state.pending` în `api()`).

## Revizie de interfață și sesiune (10.10.2026, același sub-slice AD10)

- **Interfață:** Subunitate mutat deasupra LUNA / ANUL, fără etichetă; Plătitori, Taxe și tema într-un meniu ☰ în antet
  (aceeași fereastră popup ca filtrul din subsolul grilei, cu emoticoane); „A” deschide aplicațiile contului (K-BOT activ,
  Vercon/Avacont în curând); căutarea este centrată pe grila principală (antetul are coloanele panoului); 🔄 în bara
  LUNA / ANUL; 🔒/🔓 ca iconiță dreapta pe rândurile lunilor (nou: `rightIcon` în TreeView); toate antetele de 32 px;
  coloanele numerice egale; panoul din stânga se restrânge; butonul panoului Rapoarte rămâne centrat; la alegerea unui
  copil se deschide fila documentelor lui; „Grupa ” dispare din nume (majuscule, fără spații); filtre 🔍 în subsolul grilei;
  banda de mesaje afișează numai erori.
- **Sesiune:** o oră de la cod, fără prelungire și fără alunecare (`PORTAL_MAX = PORTAL_IDLE = 3600`); cronometru în ultimele
  5 minute (`adechit/session.js`); la expirare se face deconectare chiar cu modificări nesalvate. Codul de pe e-mail rămâne
  valabil o oră **în același browser** (`kbot-portal-trust` în localStorage + notă `portal_trust` pe server): după deconectare,
  e-mailul și parola ajung. Pagina de autentificare are combobox „Aplicația” (K-BOT / ADECHIT active); cu ADECHIT ales,
  portalul nu mai este desenat deloc.
- **Date locale:** `ADECHIT/tools/preview.py` pornește implicit cu două subunități și cu o lună închisă (septembrie 2026) și
  una deschisă (octombrie 2026) în fiecare (`--single-subunit` oprește a doua subunitate).
- Ajutorul din pagină actualizat. Teste: `adechit_lazy.test.mjs` adaptat la modulele noi, `test_adechit.py` neschimbat.
