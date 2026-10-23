# SLICE-ADE9-04 — Situație de plată și text HTML configurabil

13.10.2026. TESTAT, RULAT, PUBLICAT (confirmat de operator, 23.10.2026).

## Comportament

Raport A4 portret după imagine: grupă, copii în ordine alfabetică, SID/SIC,
taxa pentru zilele configurate ale lunii și suma SID + taxa − SIC. Utilizatorul
a confirmat explicit soldurile inițiale și taxa lunară integrală. Datele provin
din situația calculată/salvată existentă; taxa și zilele sunt cele ale lunii selectate.
Sumele negative se păstrează. Nu se schimbă calculele contabile existente.

Butonul Situație de plată este sub Situație lunară și folosește Toate grupele.
Toate grupele introduce mereu salt de pagină între grupe, independent de Separă
grupe. HTML/PDF repetă antetul tabelului la continuare și nota sub fiecare grupă.
Grupele numeroase sau notele lungi pot ocupa mai multe pagini.

Meniul Text situație de plată deschide editor vizual și sursă HTML. Configurare
per subunitate, drept Catalog, versiune pentru conflicte, cheie de idempotentă
păstrată la reîncercare și blocare a schimbării subunității cât timp dialogul este
deschis. Notele acceptă liste, paragrafe, titluri, aldine/cursive/subliniere,
aliniere, șapte culori, trei familii de font și șapte mărimi. Randarea trece
printr-o listă explicită de formatare acceptată, fără atribute sau CSS arbitrar.
Editorul primește numai HTML filtrat; lipirea este text simplu, sursa permite HTML.
Variabile: [UNITATE], [CONT], [COD_FISCAL], [GRUPA], [LUNA], [AN]. Primul cont este
implicit, dar utilizatorul poate scrie alt IBAN în text. Datele instituției din
imagine nu sunt introduse ca valori reale în aplicație.

PDF folosește Helvetica/Times/Courier, cu DejaVu pentru caractere Unicode.
Excel exportă nota ca text simplu și introduce salturi între grupe; lățimile
se citesc la fiecare export prin reporting/excel_widths.py cu chei stabile.

## Fișiere de publicat

- Python: `routes/adechit/__init__.py`, `reports.py`, `report_render.py`,
  `report_excel.py`, modulele noi `payment_notice.py` și `report_richtext.py`.
- Pagina: `static/adechit.html`; JS: `static/js/adechit/app.js`, `reports.js`,
  `payment-notice-config.js`.
- CSS: `static/css/adechit.css`, `adechit-payment-notice.css`,
  `adechit-richtext.css`, `adechit-payment-notice-config.css`.
- Ajutor: `static/help/adechit.json`; ghidul UTILIZARE_WEB și statusurile au
  fost actualizate în aceeași intervenție.

## Instalare manuală

1. După schema de subunități AD_05, executați
   `sql/AD_10_raport_situatie_plata.sql` în fiecare bază ADECHIT.
   Schema de referință citită: MariaDB_Schema/000_DEMO.sql, AD_Subunits,
   AD_LunaD, AD_ValoriTaxe și AD_Operations.
2. Opțional, după AD_08_rapoarte_excel_coloane.sql, executați
   `sql/AD_10_raport_situatie_plata_excel.sql` pentru lățimile din AVACONT_COMUN.
3. Publicați fișierele prin AvacontPush și configurați textul în subunitatea dorită.

Tabela nouă este AD_REPORT_CONFIG (SUBUNIT_ID, REPORT_KEY, FOOTER_HTML, VERSION),
cu FK către AD_Subunits. Absența unei înregistrări folosește modelul implicit.
Tabela trebuie să existe înainte de folosirea raportului/configurării. Nu există
DDL automat în aplicație. Revenire: restaurați fișierele anterioare; tabela nouă
poate rămâne fără să afecteze versiunea veche. Nu sunt necesare ștergeri de date.

## Rămâne de probat de utilizator

*Rezolvat: operatorul a confirmat, la 23.10.2026, că nu mai este nimic nevalidat sau deschis.*

Lună deschisă/închisă, SID/SIC și taxa integrală, totaluri, o grupă/toate grupele,
separarea paginilor, nota cu liste/culori/font/mărime și variabile, conflictele
de salvare și reîncercarea după o eroare HTTP, drepturile Catalog, izolarea
subunităților, imprimarea/PDF/Excel și regresia rapoartelor existente.
