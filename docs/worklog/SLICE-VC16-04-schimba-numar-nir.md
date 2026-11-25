# SLICE-VC16-04 — Schimbă număr NIR din lista documentelor

17.11.2026 — Implementare locală la cererea Adelinei.

## Comportament

În meniul ⋮ al fiecărui NIR din lista documentelor salvate apare **Schimbă număr NIR**. Comanda deschide o fereastră mică, cu numărul și data NIR-ului, furnizorul, numărul și data facturii, câmpul noului număr și butoanele Salvează / Renunță.

După proba Adelinei, validarea și atribuirea numerelor NIR/FCS au o singură implementare: `PYTHON/routes/vercon/receipt_number.py`. Acest fișier verifică formatul, subnumărul, unicitatea în seria comună a anului calendaristic, păstrarea anului la editare, disponibilitatea coloanei pentru zecimale și poziția între documentele vecine după dată. Numerele din aceeași zi sunt libere în intervalul dintre zilele vecine. Limita inferioară vine din ultima zi anterioară, iar limita superioară din prima zi ulterioară; numerele din alte zile nu restrâng acest interval, dar rămân ocupate pentru verificarea unicității. Pentru 17.09, între 79 din 11.09 și 81 din 23.09, 80 este acceptabil; numerele 28 și 30 din octombrie nu devin limita superioară pentru septembrie.

Aceeași validare este apelată de verificarea din formular, ciornă, repartizare, adăugare/editare NIR și FCS, schimbarea numărului și atribuirea numărului din inventar. Afișarea numerotării curente cere ultimul număr aceluiași modul, păstrând sensul existent de cel mai mare număr din serie. Numărul introdus nu mai este înlocuit cu o fracție doar fiindcă există un NIR ulterior. Câmpurile rămân editabile. Pentru număr necompletat, propunerea caută întâi un întreg liber, apoi zecimale dacă un întreg nu încape între vecini. Schimbarea datei validează numărul existent și este refuzată dacă numărul nu se mai încadrează. Stocul inițial continuă să nu definească seria NIR/FCS, ca înaintea acestui slice. Nu se introduce regula lunii curente folosită la bonuri.

Algoritmul `nir_number.py` a fost eliminat. Au fost eliminate verificarea cronologică paralelă din `receipt_rules.py` și ramurile vechi NIR/FCS din numerotarea `documents.py`. Verificarea formatului din popup a fost mutată pe server; JavaScript verifică doar dacă există text schimbat pentru activarea butonului.

Ruta de salvare păstrează dreptul de recepție, sesiunea/blocarea gestiunii, exercițiul deschis, verificarea versiunii, blocajul eFactura, al storno-urilor și al documentelor dependente de transferuri între subunități. Nu se modifică date, factură, furnizor, linii, stocuri sau alte documente. Se actualizează `Numar`, `NumarSub` când coloana există și `Version`; auditul `nir.number` păstrează numărul anterior și cel nou. Lista se reîncarcă păstrând gruparea și ordonarea existente.

Popup-ul nu mai cere o propunere la deschidere și nu mai afișează mesaje despre citirea condițiilor sau redeschidere. Numărul se introduce direct; un refuz concret al validatorului rămâne în popup. Retrimiterea aceleiași valori păstrează cheia cererii după un răspuns pierdut. Nu se schimbă regulile bonurilor BC/BR/BP/BT/BS.

La cererea ulterioară a Adelinei, butonul **Propune Număr** este lângă câmpul popup-ului. La apăsare folosește ruta existentă `GET /api/vercon/doc/nir/number`, cu data documentului, `exclude` egal cu ID-ul lui, fără număr introdus și cu subnumăr zero. Ruta apelează direct `receipt_number.check`, iar acesta folosește `suggestion` din același fișier. Propunerea completează câmpul, fără salvare automată; salvarea revalidează în același modul. Câmpul și butoanele sunt dezactivate pe durata cererii pentru a evita suprapunerea cu salvarea. Această completare modifică doar fișiere existente și nu introduce alt algoritm sau alt fișier.

## Fișiere proprii și motive pentru eventuala înregistrare

- `PYTHON/routes/vercon/receipt_number.py` — unica implementare a validării și atribuirii NIR/FCS, inclusiv format, unicitate, vecini după dată și zecimale.
- `PYTHON/routes/vercon/nir_number.py` — eliminat; algoritmul separat nu se mai folosește și nu trebuie păstrat pe server la publicarea completă a corecției.
- `PYTHON/routes/vercon/receipt_rules.py` — eliminarea validării cronologice paralele; păstrează aritmetica și simularea stocului.
- `PYTHON/routes/vercon/documents.py` — eliminarea validărilor vechi NIR/FCS și conectarea FCS la validatorul unic; numerotarea BC/BR rămâne distinctă.
- `PYTHON/routes/vercon/receipts.py` — adăugarea și editarea NIR folosesc validatorul unic.
- `PYTHON/routes/vercon/receipt_routes.py` — ciorna folosește validatorul unic, fără validări sau mesaje paralele.
- `PYTHON/routes/vercon/receipt_distribution.py` — repartizarea folosește validatorul unic.
- `PYTHON/routes/vercon/numbering_overview.py` — ultimul număr NIR/FCS este cerut modulului unic, păstrând cel mai mare număr existent.
- `PYTHON/routes/vercon/receipt_number_change.py` — schimbare număr NIR prin validatorul unic, cu versiune și audit.
- `PYTHON/routes/vercon/doc_routes.py` — extinderea rutei existente `/number` pentru NIR, cu aceeași tranzacție și protecție la retrimitere.
- `PYTHON/static/js/vercon/receipt-number-change.js` — popup compact cu informații NIR și număr editabil; eliminarea citirii condițiilor, mesajelor și validării locale a numărului.
- `PYTHON/static/css/vercon-receptie.css` — așezarea câmpului și butonului Propune Număr pe același rând, limitată la popup-ul NIR.
- `PYTHON/static/js/vercon/document-number.js` — afișarea rezultatului serverului, fără forțarea zecimalelor sau blocarea câmpurilor NIR/FCS.
- `PYTHON/static/js/vercon/number-change.js` — delegarea NIR către popup-ul propriu, păstrând fluxul bonurilor.
- `PYTHON/static/js/vercon/docs.js` — comanda din lista NIR, conectarea popup-ului și afișarea numărului complet după salvare.
- `PYTHON/static/js/vercon/document-history.js` — denumirea operației de schimbare număr NIR în Istoric.
- `PYTHON/static/help/vercon.json` — instrucțiunile comenzii și condițiile păstrate.
- `VERCON/VERCON_STATUS.md` — starea reală și trimitere la acest worklog.
- `implementare_vercon_v2.md` — intrare în indexul istoric.
- `docs/worklog/SLICE-VC16-04-schimba-numar-nir.md` — evidența lucrării, fișierelor și limitelor probei.

## Stare și limite

Prima implementare a fost probată de Adelina și respinsă: popup-ul NIR 25 din 17.09.2026 afișa «Nu există un subnumăr zecimal liber», deși între vecinii 79 și 81 există 80. Corecția, centralizarea și butonul Propune Număr au fost confirmate ulterior de Adelina prin `$ssh`, la 17.11.2026; conform skill-ului, comanda confirmă versiunea curentă și autorizează înregistrarea celor 19 fișiere existente din lista de mai sus. Agentul a analizat codul și apelurile, fără teste, compilare, verificări de sintaxă, rulare în aplicație sau SQL executat. Fără modificări de schemă și fără publicare în această intervenție. `nir_number.py` este șters local și nu poate fi înregistrat: procedura de deployment acceptă numai fișiere existente și nu șterge fișiere de pe server. Eliminarea lui de pe server rămâne o operație separată.
