# SLICE-AD11-05 — copil + părinte într-un singur formular, linkuri în erori, email pe copii activi

11.10.2026. TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

## Ce s-a schimbat și de ce

1. **Mesaj de eroare comun.** Tokeni `--err-bg/--err-ink/--err-line` în `variables.css` (cu temă
   întunecată) și regula `.msg[role="alert"]` în `adechit.css`: orice eroare din ADECHIT are același
   fundal. Alte pagini care au culorile lor proprii (portal, înregistrare) nu au fost atinse.
2. **Conflict de email cu linkuri.** `DomainError`/`require` primesc `details`; `guard` îl trimite în
   JSON. Mesajul „Adresa de email este deja utilizată de …” arată părintele, copilul și grupa ca
   linkuri (închid formularul și deschid înregistrarea); pentru altă subunitate a DC-ului, linkul
   comută subunitatea (`sessionStorage` `ade.pendingOpen`, reîncărcare, apoi `openForChild`).
   Subunitatea e afișată doar dacă DC-ul are mai mult de una accesibilă.
3. **Dublu clic.** În Prezență: rând → administrare plătitori cu grupa și copilul selectate
   (`openForChild`). În administrare plătitori: dublu clic pe un rând din oricare dintre cele trei
   liste deschide editorul.
4. **„+” din Prezență.** Un singur formular „Adaugă copil” cu secțiunea „Părinte (plătitor)”, un
   singur „Salvează”, nume părinte obligatoriu. Ruta nouă `POST /api/adechit/child-with-payer`
   (`save_child_with_payer`) salvează copilul și părintele în aceeași tranzacție.
5. **Verificarea emailului** (la salvare) se face doar pe copiii activi (nu Plecat) și doar în
   subunitatea curentă; la fel „acest CNP are deja alt email”. Același părinte (același CNP) la
   mai mulți copii nu a fost niciodată conflict.
6. **Mesaj informativ „similare”.** La părăsirea câmpurilor CNP și Email din formularul
   plătitorului, `GET /api/adechit/parents/similar` caută în tot DC-ul (subunitățile accesibile
   contului, inclusiv copii plecați) alți părinți cu același CNP / email și îi listează în același
   mesaj, cu aceleași linkuri; „(plecat)” și subunitatea (dacă sunt mai multe) apar în text.
7. **Copacul lunilor rămâne deschis** după salvarea unui copil (din «+» sau din administrare
   plătitori): `loadContext` (ramura de reîncărcare) păstra anii deschiși doar la încărcarea unui
   an, apoi `setData` îi uita; acum se reîncarcă toți anii deschiși și se redesenează cu
   `renderMonthTree`, deci luna selectată rămâne vizibilă.
8. **«Adaugă copil» din administrare plătitori** deschide același formular copil + părinte ca «+»
   din Prezență (fără grupă selectată: «Selectați grupa copilului.»).
9. **Portal K-BOT: meniul de aplicații pe logo.** Click pe logo, autentificat, deschide același meniu
   «Aplicații» ca «A» din ADECHIT (contul, K-BOT, ADECHIT, Vercon/Avacont «în curând»); nelogat,
   logo-ul rămâne link spre prezentare. CSS-ul meniului s-a mutat neschimbat din `adechit.css` în
   `css/app-menu.css` (încărcat de ADECHIT și de portal; `--avacont-blue` are valoare implicită în
   `:root`).
10. **Rapoartele pe ecran urmăresc tema** (întunecat/luminos) a paginii care le-a deschis:
    `reports.js` pune `data-theme` pe fereastra raportului, iar `adechit-report.css` are reguli doar
    `@media screen`. Listarea și PDF-ul rămân pe paleta Access.

## Fișiere atinse

`PYTHON/routes/adechit/{domain,__init__,parent_identity,catalog_forms}.py`,
`PYTHON/static/js/adechit/{app,catalogs,payers}.js`, `PYTHON/static/js/portal/apps-menu.js` (nou),
`PYTHON/static/css/{variables,adechit,adechit-report}.css`, `PYTHON/static/css/app-menu.css` (nou),
`PYTHON/static/portal.html`, `PYTHON/static/adechit.html`,
`ADECHIT/UTILIZARE_WEB.md`, statusuri.

## Teste

Fără teste rulate (preferința utilizatorului). `node --check` și `py_compile` curate.
Eroarea `AD_PortalParents lipsă` din 000_DEMO a venit din AD_08 neaplicat; utilizatorul a rulat
scripturile AD_08 / AD_08_02 și raportează „pare ok”.

## Nerezolvat / neverificat

*Rezolvat: operatorul a confirmat, la 23.10.2026, că nu mai este nimic nevalidat sau deschis.*

- Linkurile din mesajul de conflict nu au fost văzute funcționând; presupunerea (părinte din altă
  subunitate) a fost tratată prin comutarea subunității, dar neprobată.
- Atomicitatea `child-with-payer` presupune anularea tranzacției la `DomainError` (citită în
  `repository.transaction`, neprobată).
- Identitatea portalului (`reconcile_contacts`, lista „Portal blocat”) rămâne pe tot DC-ul,
  inclusiv copii plecați și alte subunități: un email nou acceptat la salvare poate bloca portalul.
- Meniul de pe logo, mutarea CSS-ului meniului (aspectul meniurilor ADECHIT trebuie să rămână
  neschimbat) și copacul lunilor după salvare: doar sintaxă, neprobate.
