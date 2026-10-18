# VERCON online — ce s-a schimbat în migrator (18.10.2026) și ce trebuie făcut în browser

Document pentru Codex, inițiat după modificările din `VERCON/VRC.Migrator` 0.3.5.0.
**Actualizare 18.10.2026:** decizia privind identitățile OI/MF a fost confirmată și implementată în sursele
migratorului și aplicației. Referința actuală este [REGISTRU_OI_MF.md](REGISTRU_OI_MF.md), inclusiv corecția
`ToolVersion` din 0.3.7.0. Secțiunea 4 de mai jos este închisă prin această decizie.
Restul cerințelor din acest document nu sunt certificate prin actualizarea OI/MF. Respectați instrucțiunile proiectului
(fără diacritice în cod, CSS în fișiere separate, module sub ~1000 de linii, progresul se notează în
`implementare_vercon_v2.md`). Fără teste, build sau interogări pe server, conform instrucțiunilor proiectului.

Nu atingeți: `config.py`, fișierele `report_*` / `reports.js` / `report-*.js` (altă lucrare în curs), `AGENTS.md`.

## 1. Ce s-a schimbat în migrator (faptele)

1. **Mai multe subunități într-un singur transfer.** O singură tranzacție, câte un `VC_Subunitati` + `VC_Imports` pe
   subunitate, ZEC pe subunitate. Numele subunității se propune din `cale.accdb`. Același tip de gestiune (ex. Alimente)
   poate exista în mai multe subunități ale aceluiași DC.
2. **`Iesiri_H.IdRetur` poate fi NULL la retururi.** În programul vechi un bon de consum putea fi șters sau modificat, iar
   returul făcut din el rămânea „în aer". Valorile returului trebuie să rămână în balanță, deci returul se transferă cu
   `IdRetur = NULL`. Cazul se notează în `VC_MigrationIssues` (`RuleCode = 'N12'`, `Severity = 'CONVERSIE'`,
   `FieldName = 'IdRetur'`, `SourceKey` = cheia veche a returului, `OriginalValue` = cheia veche a bonului dispărut).
   Exemplu real: `tz_2026` Alimente Cantina, 3 retururi (chei vechi 49, 51, 136, arătau spre 47, 50, 135).
3. **`IntrariStorno.IdIntrare` poate fi NULL** (linii de storno al căror NIR a dispărut; exemplu `tz_2100`, chei vechi 7 și 8). Notate la fel (`N12`, `TableName = IntrariStorno`). Atenție: o linie de storno nu are antet propriu, iar `stock_source.py`, `ledger.sql` și `receipts.sql` o citesc prin `VC_NIR` pe `IdIntrare`; cu NULL ea nu intră în stoc și în fișă. Vezi secțiunea 3b.
4. `OI_Produse.IDM` și `OI_Predare.IdMasinaP` arată acum spre `VC_Masini` (IdMasinaP = contul DIN care se transferă, IdMasina =
   contul ÎN care). `OI_Produse.IDBO` = numărul bonului de origine (BP sau BT), renumerotat la fel ca documentul.
5. Valorile cu mai mult de 4 zecimale se rotunjesc la 4. `Tip` din mese (MicDejun, Dejun, Cina) devine `VC_FeluriMancare`.
   Numerele de document cu fracție devin `Numar` + `NumarSub`. Furnizor fără partener: `IdPartener` NULL + `CodFiscalSursa`.

## 2. Verificat în codul online — nu cere schimbare

- **Subunități multiple:** `provider.py` citește toate rândurile active din `VC_Subunitati` și le pune în arbore
  (subunitate -> gestiune -> an). Nimic nu citește `VC_Imports`. Nu se schimbă nimic.
- **IDM, IdMasinaP:** sensul conturilor rămâne cel descris mai sus. Pentru `IDBO`, `IDBC` și identitatea comună
  P/T/C, aplicația și migratorul au fost aliniate explicit la [contractul OI/MF](REGISTRU_OI_MF.md).
- **NumarSub, CodFiscalSursa:** deja folosite (`documents.py`, `receipts.py`).

## 3. De făcut: retururi fără bon sursă (`Retur = 1` și `IdRetur IS NULL`)

Astăzi un astfel de retur se poate vedea, dar nu se poate modifica: `edit_br` (`documents.py`, ~linia 714) apelează
`_source_bc(repo, before['IdRetur'])`, iar cu NULL ridică `SOURCE_INVALID` („Bonul de consum ales nu există sau este
anulat"). În `docs.js` (~linia 338) câmpul „Bonul returnat" este editabil doar în modul adăugare, deci utilizatorul nu
poate alege bonul.

Cerințe:

1. **Listă și document:** returul fără sursă se vede marcat „Fără bon sursă" (listă și antet). `list_documents` (~linia 149)
   și `get_document` (~linia 221) întorc deja `source` (acum `null`); adăugați un indicator clar, nu un câmp gol.
2. **Asociere:** acțiune nouă „Asociază cu un bon de consum" pe un retur fără sursă (rută nouă în `doc_routes.py`, logica într-un
   modul nou, nu în `documents.py`, care e deja mare). Intrare: id retur, `version`, id bon sursă.
3. **Validări** (toate cu mesaj în română, la nivel de câmp `source`):
   - returul există, în contextul curent, are `Retur = 1`, nu e anulat, are `IdRetur IS NULL` și `version` se potrivește;
   - bonul sursă există în același context, nu e anulat, nu e el însuși retur (regula din `_source_bc`);
   - fiecare produs din `VC_IesiriRetur` al returului există în bonul sursă, iar cantitatea nu depășește ce a rămas de
     returnat (`return_eligible(..., exclude=doc_id)`). Dacă nu se potrivește: respingeți cu lista produselor care nu se
     potrivesc. Nu forțați asocierea.
4. **Efect:** `UPDATE VC_Iesiri_H SET IdRetur=<bon>, Version=Version+1` cu verificarea versiunii (ca în `edit_br`), plus
   `protocol.audit(..., 'br.link', ...)` și răspuns cu noua versiune. Nu se schimbă stocul și nu se rulează
   `check_timeline` (liniile returului nu se modifică; `stock_source.py` le citește din `VC_IesiriRetur`).
5. **Modificare:** `edit_br` pe un retur fără sursă cere întâi asocierea. Mesaj clar (nu eroarea generică), iar în browser
   câmpul „Bonul returnat" devine editabil, cu lista bonurilor, doar cât timp returul nu are sursă.
6. După asociere, regulile existente se aplică de la sine: `_guard_dependents` (`documents.py`, ~linia 356) interzice
   anularea bonului sursă cât timp are retur activ.
7. **Anulare** a unui retur fără sursă: trebuie să meargă ca până acum (nu depinde de sursă). Verificați `cancel_document`.

Criterii de acceptare (de descris în `implementare_vercon_v2.md`, fără rulare): returul orfan apare marcat; asocierea cu un bon
potrivit reușește; asocierea cu un bon în care produsul lipsește sau cantitatea e depășită e respinsă cu motiv; după asociere
„Modifică" funcționează ca la un retur normal; anularea merge înainte și după asociere.

## 3b. De făcut: linii de storno fără recepție (`VC_IntrariStorno.IdIntrare IS NULL`)

Spre deosebire de retur, aici nu există document de afișat: liniile nu au antet. Cerințe minime:

1. Un loc unde operatorul le găsește (de exemplu o listă „Linii de storno fără recepție" în gestiunea curentă, cu produs, cantitate, preț, data salvării).
2. Acțiune „Leagă de o factură storno": alege un document `VC_NIR` cu `Storno = 1`, neanulat, din același context; validează produsul și cantitatea față de recepția stornată (regulile din `create_storno`). Efect: `UPDATE VC_IntrariStorno SET IdIntrare=...`, cu `Version`, audit `storno.link`; apoi se rulează `protocol.check_timeline` (liniile încep să conteze în stoc).
3. Nu se face nimic automat: operatorul decide dacă leagă liniile sau le anulează (`Anulat = 1`).

## 4. Decizie OI/MF confirmată și implementată în surse — 18.10.2026

Vechea renumerotare separată pe coloane putea produce coliziuni BP/BT și
referințe contradictorii în registru. Utilizatorul a confirmat un singur
spațiu de ID-uri interne P/T/C pe gestiune. Migratorul păstrează familia
în identitatea sursă și reutilizează maparea documentului în toate referințele.

`OI_Produse.IDBC` este bonul curent pentru toate operațiile. `IDBP` este
completat numai la predare, `IDBT` numai la transfer; fără acestea, `IDBC`
indică o casare. `IDBO` este bonul BP/BT de proveniență numai la transfer/casare,
inclusiv pe ambele laturi ale transferului. Numerele afișate au în continuare
serii separate pe tip de bon. Nu este necesară schimbarea schemei.

Aplicația folosește aceeași regulă la citire, salvare, loturi, modificare și
anulare. Nu se impune exact o singură coloană completată între IDBP/IDBT/IDBC.
Formatul VBA vechi, cu IDBC=0 la BP/BT, este acceptat; o valoare nenulă
contradictorie nu se ignoră. Datele deja migrate nu sunt reparate automat.

Corecție suplimentară: `VC_Imports.ToolVersion` stochează versiunea
executabilului (`0.3.7.0`), nu numele migratorului plus identificatorul lung
al regulilor. `RuleVersion` integral rămâne în rezumat și în manifest/hash.
Este necesară recompilarea migratorului; nu se cere ALTER pentru aceste schimbări.

Stare: **IMPLEMENTAT ÎN SURSE, NERULAT**; fără teste/build/import/server rulate
de asistent. [Exemplu complet și surse modificate](REGISTRU_OI_MF.md).

## 5. De scris la sfârșit

Intrare în `implementare_vercon_v2.md` cu data, fișierele modificate, starea reală (IMPLEMENTAT ÎN SURSE, NERULAT) și limitele
verificării. Fără modificări de schemă: `VC_Iesiri_H.IdRetur` este deja `NULL`-able și nu are cheie străină.
