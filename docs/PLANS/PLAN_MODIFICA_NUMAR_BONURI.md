# PLAN — «Modifică număr» în arborii BC / BP / BT / BS

Draft pentru aprobare, 12.11.2026. Slice: de stabilit de Adelina (nimic nu se scrie în cod până atunci). Planul nu conține progres.

## Ce am înțeles

Un punct nou în meniul unui document din arborele salvat, la **bonuri de consum (BC)** și la **bonurile PTC (BP, BT, BS)**: «Modifică număr». Operatorul schimbă doar numărul; restul documentului rămâne.

- Apare **doar pentru documente din luna în curs** (în rest nu se afișează, sau e dezactivat cu explicație; vezi întrebarea 1).
- Se salvează **doar dacă numărul respectă ordinea cronologică**: toate documentele din aceeași serie cu număr mai mic au data ≤ data acestui document, iar cele cu număr mai mare au data ≥. Aceeași zi e permisă, ca la `require_position` din `document_order.py`.
- Numărul trebuie să fie și **liber** în serie (fără dublură).
- **La BC** se verifică opțiunea de numerotare pe cont (`NUM_BC_PE_CONT`, `consumption_numbering.per_account`): dacă e activă, seria este contul bonului (`IdMasina`), iar unicitatea și cronologia se verifică numai între bonurile aceluiași cont; dacă nu, seria este toată gestiunea/exercițiul. BP/BT/BS au serie pe tip, gestiune și exercițiu (ca în `oi_number.check`).

## Unde se leagă în cod (citit, nu încă modificat)

- Meniul arborelui: `PYTHON/static/js/vercon/docs.js` (~linia 843-845), lângă «Modifică / Previzualizează / Istoric / Anulează»; arborii PTC folosesc aceeași familie de cod (de confirmat la implementare unde e meniul BP/BT/BS).
- Formatul numărului: `bon_number.py` (prefix + un spațiu + număr: `BC 13`, `BP 13` ...). Coloane: BC `VC_Iesiri_H.NumarBC`, BP `VC_OI_Predare.NRBP`, BT `VC_OI_Transfer.NRBT`, BS `VC_OI_Casare.NRBC`.
- Seria și duplicatele: `consumption_number.check` și `oi_number.check`; ordinea după număr: `document_order.number_neighbours`.
- Documentul poate avea mai multe rânduri (un BP/BT/BS are un rând pe produs, cu același număr), deci actualizarea se face pe toate rândurile documentului.

## Pași

1. **Server, o rută nouă** `POST …/<familie>/<id>/number` cu `{number}`: sub blocarea gestiunii, ca la celelalte scrieri; refuză un document anulat; verifică luna; verifică seria (per cont la BC) pentru dublură și cronologie; scrie noul text prin `bon_number.text`; scrie auditul (`protocol.audit`, valoare veche → nouă). Mesaje în română, cu intervalul permis («Numărul trebuie să fie între X și Y»). Verificarea se face la salvare, nu doar în interfață.
2. **Server, vezi și lista**: documentele din arbore primesc un semnal `number_editable` (și motivul), ca meniul să nu ghicească.
3. **Client**: «Modifică număr» în meniul arborelui, la BC/BP/BT/BS, cu o casetă mică (număr cu prefixul fix, ca în câmpul Număr existent); la succes reîmprospătează arborele; mesajele prin sistemul de mesaje existent.
4. **Ajutor**: secțiune în `vercon.json` cu eticheta de slice.
5. **Documentare**: worklog, `VERCON_STATUS.md`, `implementare_vercon_v2.md`; proba de către Adelina înainte de orice înregistrare pentru publicare.

## Întrebări înainte de cod

1. **«Luna în curs»**: luna calendaristică de azi (noiembrie 2026) sau «luna activă» existentă la BC (`consumption_period.active_month` = ultima lună cu date, nu cea calendaristică)? Propun luna calendaristică pentru toate patru; dacă la BC vrei regula existentă, spune-mi.
2. **Care dată contează**: data documentului (BC `DataBC`, BP `DATABP` ...), nu data salvării. Așa înțeleg.
3. **Rămân în afară**: BX (retur), BU (transfer între subunități, are număr în `NRBT`) și listele de alimente. Dacă vrei și BX/BU, le adaug.
4. **BP creează și un bon în `VC_Iesiri_H`** (după comentariul din `bon_number.py`): dacă numărul BP e copiat acolo, îl schimb și acolo. O confirm din cod la implementare.
5. Contorul `VC_Numbering` nu se atinge: propunerea de număr nou se calculează din maximul numerelor existente și al contorului, deci rămâne corectă.

## Limite

Planul se bazează pe citirea codului; nu am rulat nimic. Neverificat: unde exact e meniul arborilor BP/BT/BS și dacă BP copiază numărul în `VC_Iesiri_H`.
