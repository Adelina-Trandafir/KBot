# SLICE-VC06-22 — Gestionar: furnizori și actualizare numai prin ANAF

Data: 30.10.2026.

Stare: **IMPLEMENTAT_LOCAL, NERULAT, NEPUBLICAT**.

Cererea utilizatorului: în VERCON, rolul **Gestionar** vede lista partenerilor și îi poate actualiza numai prin ANAF; dacă nu sunt găsiți în ANAF, poate salva manual numai `CodFiscal`. Utilizatorul a corectat explicit formularea inițială „admin” în „Gestionar” și a cerut layout/look existent, cu controale CUSTOM.

Pagina existentă în meniu `Configurări → Furnizori (Parteneri)` era un placeholder. Acum folosește workspace-ul nomenclatoarelor: DataGrid CUSTOM în stânga, detalii în dreapta, TextBox CUSTOM pentru căutare/câmpuri și bara comună de acțiuni/status. Adresa folosește TextBox multiline. Confirmările și notificările sunt dialogurile comune ale aplicației. CSS separat, limitat la această pagină; adaptare pentru ecrane înguste.

Lista conține partenerii vizibili de tip furnizor (`Tip=1`, `Ascuns=0`), inclusiv cei fără cod fiscal. Arată Denumire, Cod fiscal, Cod partener și detaliile partenerului selectat. Ordinea implicită este alfabetică în limba română, cu chei explicite; sortarea/filtrele grilei se păstrează la căutare, reîmprospătare și salvare. ID-ul intern identifică rândul, fără a stabili ordinea afișării.

Drepturi și scriere:

- Accesul la listă păstrează dreptul VERCON `read`. Permisiunea limitată `parteneri_anaf` se acordă în contextul VERCON numai când există `read` și rolul curent din `AVACONT_COMUN.Unitati_Utilizatori` este Gestionar. Apartenența se recitește pe server; rolul memorat în sesiune și datele trimise de client nu acordă drepturi. Nu se adaugă roluri în bază și nu se extinde `nomenclatoare`/`receptie`.
- Consultarea nu ocupă gestiunea. La actualizare se ia lock-ul comun și serverul revalidează contextul, tipul gestiunii și tokenul lock-ului.
- Actualizarea ANAF primește numai codul fiscal, modul și versiunea rândului. Numele și adresa sunt preluate de server din ANAF. Sunt actualizate numai `CodFiscal`, `DenumirePartener`, `Adresa`.
- După un răspuns explicit „negăsit”, apare „Salvează numai CodulFiscal”. Salvarea cere confirmarea operatorului și repetă interogarea ANAF. Dacă între timp codul este găsit, salvarea manuală este refuzată; dacă rămâne negăsit, se scrie numai `CodFiscal`.
- Indisponibilitatea, răspunsurile neconforme sau lipsa denumirii ANAF nu autorizează salvarea manuală. Clientul ANAF comun primește un parametru opțional `strict_missing`, folosit numai de această pagină, pentru confirmarea explicită a codului în `notFound`; politica celorlalți apelanți rămâne aceeași.
- Sunt acceptate coduri românești de 2–10 cifre, cu normalizarea prefixului RO/separatorilor. Codurile de alte forme nu sunt tratate automat drept „negăsite”. Codul unității și codul fiscal deja folosit de alt partener sunt refuzate, inclusiv când celălalt partener este ascuns.
- Versiunea este amprenta datelor citite; modificările concurente cer reîmprospătare. Scrierea și auditul `VC_Audit` sunt în aceeași tranzacție. Datele ANAF sunt cerute fără a ține lock-uri de rând MariaDB; verificarea versiunii se repetă cu lock la scriere.
- Nu există adăugare, ștergere, ascundere sau editare liberă în noua pagină. Codul intern, IBAN-ul și banca sunt read-only. Câmpurile suplimentare trimise direct la API sunt refuzate.
- Calea VERCON existentă „Parteneri dublați / Combină” este refuzată pentru Gestionar, inclusiv la apel direct; celelalte roluri își păstrează drepturile anterioare. Astfel combinarea nu ocolește restricția „doar ANAF”.

Fișiere de aplicație:

- `PYTHON/routes/vercon/partners.py` (nou), `__init__.py`, `operations.py`, `supplier_merge.py`;
- `PYTHON/routes/inregistrare/anaf.py` (opțiune strictă folosită numai aici);
- `PYTHON/static/js/vercon/partners.js` (nou), `app.js`;
- `PYTHON/static/css/vercon-partners.css` (nou), `vercon.css`.

Limita intervenției: se actualizează numai partenerul selectat și auditul său. Nu se remapează documente istorice, coduri de angajament sau e-Facturi după schimbarea codului fiscal. Nomenclatorul desktop K-BOT și celelalte aplicații nu primesc noua interfață sau alte drepturi prin acest slice.

Nu s-au rulat teste, build, verificări automate/de sintaxă, browser, HTTP sau SQL. S-au citit sursele și contractele controalelor pentru proiectare. Layout-ul nu a fost probat vizual. Nu s-au modificat date sau scheme MariaDB. Publicarea cere toate fișierele de aplicație de mai sus și repornirea serviciului web; nu cere DDL/seed SQL. Helpul și capturile nu sunt schimbate în acest slice.
