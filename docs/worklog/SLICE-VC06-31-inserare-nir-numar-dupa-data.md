# SLICE-VC06-31 — Inserarea NIR între documente existente

Data: 03.11.2026.

**COMPORTAMENT INTERMEDIAR ÎNLOCUIT DE [VC06-32](SLICE-VC06-32-nir-zecimal-automat-blocat-dupa-data.md).** Utilizatorul cere recalculare și pentru numere manuale/NIR existente când se schimbă datele, cu număr zecimal obligatoriu și ambele câmpuri dezactivate dacă există NIR ulterior. Dacă nu există, valorile curente se păstrează. Descrierea de mai jos documentează implementarea intermediară, nu regula finală.

## Problema și cauza din cod

Utilizatorul raportează mesajul „Data NIR/FCS trebuie să fie între 24.09.2026 (NIR 27) și sfârșitul anului”, cod `15e799fb81c94ca38c6f2580b3cd4cd5`, când încearcă să introducă o factură între documente existente.

Mesajul provine din `receipt_rules.chronology`: data trebuie să se încadreze în poziția numărului și subnumărului NIR/FCS. O poziție după NIR 27 datat 24.09 nu poate primi o dată mai veche. Această regulă nu a fost introdusă de corecția Data NIR din VC06-29.

Cauza identificată în cod: propunerea unui număr/subnumăr după data aleasă era activată automat numai pentru FCS. NIR păstra numărul propus la sfârșitul seriei când operatorul alegea o dată mai veche; schimbarea datei era apoi refuzată pentru acel număr. Subnumerele manuale existau deja.

## Corecția

- `PYTHON/static/js/vercon/document-number.js`: propunerea după dată este disponibilă pentru NIR nou și FCS nou. Se actualizează atât numărul, cât și subnumărul. Revalidarea numărului gol păstrează propunerea automată; răspunsurile vechi sunt ignorate dacă operatorul a trecut între timp la numerotare manuală.
- `PYTHON/static/js/vercon/docs.js`: la schimbarea Data NIR (inclusiv prin completarea după Data facturii), NIR-ul nou cu numerotare automată cere o nouă propunere. Modificarea manuală a numărului oprește actualizarea automată. Dacă validarea datei/ciornei refuză schimbarea, se restaurează și perechea număr/subnumăr, împreună cu datele anterioare.
- `PYTHON/routes/vercon/doc_routes.py`: endpointul de număr folosește sugestia de inserare și pentru NIR când numărul nu este furnizat.
- `PYTHON/routes/vercon/receipt_routes.py`, `receipt_distribution.py`: validarea NIR nou permite propunerea după dată; repartizarea unei facturi pe mai multe destinații validează fiecare serie la data aleasă.
- `PYTHON/routes/vercon/receipts.py`: la crearea NIR obișnuit, `assign_pair` reverifică sub blocarea gestiunii și persistă împreună numărul și subnumărul propuse. Traseul stocului inițial este păstrat.
- `PYTHON/routes/vercon/receipt_number.py`: comentariile responsabilității existente de inserare sunt actualizate pentru NIR/FCS. Algoritmul de alegere a poziției și regulile de cronologie nu sunt relaxate.
- Ajutorul și README-ul rapoartelor descriu inserarea prin propunerea potrivită datei.

Exemplu de design: dacă data dorită este între NIR 26 și NIR 27, se poate propune 26,1, dacă poziția este liberă și datele se încadrează. La sfârșitul seriei se propune următorul număr întreg. Un număr/subnumăr introdus manual rămâne supus verificării cronologice; documentele existente nu se renumerotează.

Limita intervenției: inserarea NIR nou în seria comună NIR/FCS și coerența propunerii/validării/salvării. Nu se schimbă Data NIR ca bază de stoc, politica lunii active, controlul stocului, datele istorice sau schemele MariaDB. Numerele salvate și numerotarea BC/OI nu sunt modificate automat.

## Fișiere proprii și motiv pentru înregistrare după confirmare

Motiv: `Inserarea NIR intre documente existente prin recalcularea numarului si subnumarului automat dupa Data NIR, cu verificare coerenta la validare si salvare.`

- `PYTHON/static/js/vercon/document-number.js`
- `PYTHON/static/js/vercon/docs.js`
- `PYTHON/routes/vercon/doc_routes.py`
- `PYTHON/routes/vercon/receipt_routes.py`
- `PYTHON/routes/vercon/receipt_distribution.py`
- `PYTHON/routes/vercon/receipts.py`
- `PYTHON/routes/vercon/receipt_number.py`
- `PYTHON/static/help/vercon.json`
- `PYTHON/reports/vercon/README.md`
- `docs/worklog/SLICE-VC06-31-inserare-nir-numar-dupa-data.md`
- `VERCON/VERCON_STATUS.md`
- `implementare_vercon_v2.md`

## Stare

**IMPLEMENTAT_LOCAL, NERULAT, NEPUBLICAT, NEÎNREGISTRAT PENTRU GIT.** Numai analiză de design din surse și mesajul furnizat. Fără teste, compilare, verificări automate, browser, HTTP, SQL, SSH, publicare, restart sau Git. Nu s-a consultat jurnalul serverului pentru acest identificator. Înregistrările anterioare rămân imuabile; versiunile noi ale fișierelor editate din nou necesită confirmarea explicită pentru înregistrare, conform AGENTS.md. Cererea anterioară de înregistrare a acoperit versiunile anterioare, nu aceste modificări ulterioare.

Actualizare 03.11.2026: la cererea explicită de a scrie mesajele pentru publicarea prin SSH, s-au înregistrat cele 9 fișiere proprii pentru server, cu motivul concret al propunerii și salvării perechii număr/subnumăr. Versiunile înregistrate includ regula finală din VC06-32, nu comportamentul intermediar descris anterior. Mesajul lotului și detaliile înregistrării sunt în [worklog VC06-32](SLICE-VC06-32-nir-zecimal-automat-blocat-dupa-data.md#mesaje-și-înregistrare-pentru-publicare--03112026). Nu s-a executat publicarea, SSH sau Git.
