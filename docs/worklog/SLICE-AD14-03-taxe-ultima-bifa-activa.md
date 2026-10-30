# SLICE-AD14-03 — Ultima bifă Activ din Taxe are prioritate

30.10.2026. Cerință: orice taxă existentă poate fi bifată oricând; bifa curentă trebuie acceptată și celelalte taxe debifate automat.

## Ce s-a schimbat și de ce

Citirea codului a identificat două cauze: reconstrucția grilei procesa taxele active în ordinea începuturilor, favorizând taxa mai nouă; salvarea pe server trata reactivarea unei taxe existente ca pe deschiderea unei perioade noi și putea refuza o taxă mai veche pentru suprapunere.

- În `catalogs.js`, activările/dezactivările se reconstituie în ordinea acțiunilor operatorului. O bifă nouă debifează toate celelalte taxe, independent de date și de sortarea grilei. Ultima alegere se păstrează și după editarea altui câmp, iar payload-ul folosește starea finală a bifelor.
- Payload-ul include și debifările automate ale taxelor inițial active, inclusiv dacă ultima bifă este ulterior scoasă și toate rămân debifate înainte de salvare.
- Adăugarea unei taxe creează o alegere nouă în aceeași ordine. La anularea unui rând se elimină și acțiunile lui, astfel încât alegerea anterioară rămasă valabilă să poată fi refăcută.
- Simpla bifare a unei taxe existente nu modifică Început/Sfârșit. Previzualizarea sfârșiturilor se reconstruiește numai pentru perioade noi, separat de alegerea taxei active.
- În `service.py`, activarea unei taxe existente dezactivează celelalte taxe din subunitatea curentă, fără să le schimbe perioadele sau să impună o ordine cronologică a activărilor. Comanda care schimbă numai Activ nu validează din nou perioada istorică. Validările sumei/explicației și regulile perioadelor nou create/editate se păstrează.
- În loturile de taxe se controlează toate versiunile revizuite înaintea primei scrieri. Pentru scrierile următoare se folosesc versiunile rezultate din modificările interne ale aceleiași tranzacții. Acest lucru permite adăugarea unei taxe urmată de reactivarea unei taxe existente din același lot, fără conflict produs de dezactivarea/închiderea internă. Modificările externe anterioare cererii rămân refuzate.

Regula se aplică în Taxe și Închidere lună, care refolosesc aceeași grilă și salvare. Nu se rescriu lunile/prezența, nu se schimbă drepturile și nu se modifică schema. Debifarea explicită poate lăsa temporar nicio taxă activă; deschiderea unei luni noi păstrează cerința existentă de exact o taxă activă.

## Fișiere

De publicat împreună prin AvacontPush:

- `PYTHON/static/js/adechit/catalogs.js`
- `PYTHON/routes/adechit/service.py`
- `PYTHON/static/help/adechit.json`

Documentație actualizată: `ADECHIT/UTILIZARE_WEB.md`, `ADECHIT_STATUS.md`, `docs/worklog/state/ADECHIT_STATUS_ADE10-ADE19.md` și acest worklog.

## Probe și limite

**SCRIS LOCAL.** Conform AGENTS.md nu s-au rulat teste, compilări, verificări executabile sau aplicația. S-a făcut numai analiză de cod/proiectare, folosind schema locală AD_ValoriTaxe. Fără SQL, DDL, commit, publicare sau restart.

Operatorul probează în ambele ferestre: taxă veche bifată după una nouă, revenire la taxa inițială, alternarea bifelor înainte de salvare, editarea explicației după bifare, salvare și redeschidere, debifare explicită, adăugarea și anularea unei taxe noi, adăugare urmată de reactivarea unei taxe existente și conflict de versiune extern. Se urmăresc bifa finală și păstrarea perioadelor la activarea simplă.

Ajutorul textual este actualizat cu AD14-03. Nu s-au realizat capturi noi; controalele și coloanele rămân cele existente. Nu sunt necesare scripturi SQL pentru această corecție.
